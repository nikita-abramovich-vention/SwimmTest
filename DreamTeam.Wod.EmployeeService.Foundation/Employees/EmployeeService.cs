using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.BackgroundJobs;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Observable;
using DreamTeam.Common.Reflection;
using DreamTeam.Common.Specification;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Common.Threading;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Core.DepartmentService.Units;
using DreamTeam.Core.ProfileService;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Core.TimeTrackingService;
using DreamTeam.Core.TimeTrackingService.DataContracts;
using DreamTeam.Logging;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Foundation.Countries;
using DreamTeam.Wod.EmployeeService.Foundation.EmployeeChanges;
using DreamTeam.Wod.EmployeeService.Foundation.Internships;
using DreamTeam.Wod.EmployeeService.Foundation.Microservices;
using DreamTeam.Wod.EmployeeService.Foundation.Offices;
using DreamTeam.Wod.EmployeeService.Foundation.Organizations;
using DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans;
using DreamTeam.Wod.EmployeeService.Foundation.SeniorityManagement;
using DreamTeam.Wod.EmployeeService.Foundation.TitleRoles;
using DreamTeam.Wod.EmployeeService.Repositories;
using EmploymentType = DreamTeam.Wod.EmployeeService.DomainModel.EmploymentType;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    [UsedImplicitly]
    public sealed class EmployeeService : IEmployeeService
    {
        private const string UpdateEmployeeResourceName = "Update employee - {0}";
        private static readonly IReadOnlyCollection<string> SmgProfileFields =
        [
            nameof(SmgProfileSlimDataContract.SmgId)
        ];

        private static readonly IEqualityComparer<EmploymentPeriod> EmploymentPeriodComparer = new EmploymentPeriodEqualityComparer();
        private static readonly IEqualityComparer<WageRatePeriod> WageRatePeriodComparer = new WageRatePeriodEqualityComparer();

        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;
        private readonly IProfileService _profileService;
        private readonly ICountryProvider _countryProvider;
        private readonly IOfficeProvider _officeProvider;
        private readonly IEmployeeLocationProvider _employeeLocationProvider;
        private readonly IInternshipService _internshipService;
        private readonly IEnvironmentInfoService _environmentInfoService;
        private readonly IJobScheduler _jobScheduler;
        private readonly IEmployeeServiceConfiguration _employeeServiceConfiguration;
        private readonly IRelocationPlanService _relocationPlanService;
        private readonly ITimeTrackingService _timeTrackingService;
        private readonly IEmployeeSnapshotService _employeeSnapshotService;
        private readonly IEmployeeChangesService _employeeChangesService;
        private readonly ISmgProfileMapper _smgProfileMapper;
        private readonly ISeniorityService _seniorityService;
        private readonly IUnitProvider _unitProvider;
        private readonly ITitleRoleService _titleRoleService;
        private readonly IOrganizationProvider _organizationProvider;
        private readonly IResourceLockService _resourceLockService;


        public event AsyncObserver<EmployeeChangedEventArgs> EmployeeMaternityLeaveStateUpdated;

        public event AsyncObserver<EmployeeChangingEventArgs> EmployeeCreating;

        public event AsyncObserver<EmployeeChangingEventArgs> EmployeeUpdating;

        public event AsyncObserver<EmployeeChangedEventArgs> EmployeeCreated;

        public event AsyncObserver<EmployeeChangedEventArgs> EmployeeUpdated;


        public EmployeeService(
            IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider,
            IProfileService profileService,
            ICountryProvider countryProvider,
            IOfficeProvider officeProvider,
            IEmployeeLocationProvider employeeLocationProvider,
            IInternshipService internshipService,
            IEnvironmentInfoService environmentInfoService,
            IJobScheduler jobScheduler,
            IEmployeeServiceConfiguration employeeServiceConfiguration,
            IRelocationPlanService relocationPlanService,
            ITimeTrackingService timeTrackingService,
            IEmployeeSnapshotService employeeSnapshotService,
            IEmployeeChangesService employeeChangesService,
            ISmgProfileMapper smgProfileMapper,
            ISeniorityService seniorityService,
            IUnitProvider unitProvider,
            ITitleRoleService titleRoleService,
            IOrganizationProvider organizationProvider,
            IResourceLockService resourceLockService)
        {
            _uowProvider = uowProvider;
            _profileService = profileService;
            _countryProvider = countryProvider;
            _officeProvider = officeProvider;
            _employeeLocationProvider = employeeLocationProvider;
            _internshipService = internshipService;
            _environmentInfoService = environmentInfoService;
            _jobScheduler = jobScheduler;
            _employeeServiceConfiguration = employeeServiceConfiguration;
            _relocationPlanService = relocationPlanService;
            _timeTrackingService = timeTrackingService;
            _employeeSnapshotService = employeeSnapshotService;
            _employeeChangesService = employeeChangesService;
            _smgProfileMapper = smgProfileMapper;
            _seniorityService = seniorityService;
            _unitProvider = unitProvider;
            _titleRoleService = titleRoleService;
            _organizationProvider = organizationProvider;
            _resourceLockService = resourceLockService;
        }


        public async Task<IReadOnlyCollection<Employee>> GetEmployeesAsync(bool shouldIncludeInactive, bool shouldIncludeInterns = false)
        {
            var uow = _uowProvider.CurrentUow;

            return await GetEmployeesAsync(shouldIncludeInactive, uow, shouldIncludeInterns);
        }

        public async Task<IReadOnlyCollection<Employee>> GetEmployeesAsync(bool shouldIncludeInactive, IEmployeeServiceUnitOfWork uow, bool shouldIncludeInterns = false)
        {
            var employeeRepository = uow.GetRepository<Employee>();
            var employeeLoadStrategy = GetEmployeeSlimLoadStrategy();
            Specification<Employee> specification = new TrueSpecification<Employee>();
            if (!shouldIncludeInactive)
            {
                specification &= EmployeeSpecification.Active;
            }

            if (!shouldIncludeInterns)
            {
                specification &= EmployeeSpecification.EmployeeNotInternship;
            }

            var employees = await employeeRepository.GetWhereAsync(specification, employeeLoadStrategy);

            var employeeRoleRepository = uow.GetRepository<EmployeeRole>();
            var roleLoadStrategy = new EntityLoadStrategy<EmployeeRole>(r => r.Role);
            await employeeRoleRepository.GetAllAsync(roleLoadStrategy);

            var employeeWorkplaceRepository = uow.GetRepository<EmployeeWorkplace>();
            var workplaceLoadStrategy = new EntityLoadStrategy<EmployeeWorkplace>(w => w.Workplace);
            await employeeWorkplaceRepository.GetAllAsync(workplaceLoadStrategy);

            foreach (var employee in employees)
            {
                employee.Roles ??= new List<EmployeeRole>(0);
                employee.Workplaces ??= new List<EmployeeWorkplace>(0);
            }

            return employees;
        }

        public async Task<IReadOnlyCollection<Employee>> GetEmployeesWithRoleAsync(string roleId)
        {
            var uow = _uowProvider.CurrentUow;
            var employeeRepository = uow.GetRepository<Employee>();
            var employeeLoadStrategy = GetEmployeeLoadStrategy();
            var specification = EmployeeSpecification.Active &
                                EmployeeSpecification.ByRoleId(roleId);

            var employees = await employeeRepository.GetWhereAsync(specification, employeeLoadStrategy);

            return employees;
        }

        public async Task<IReadOnlyCollection<Employee>> GetEmployeesByIdsAsync(
            IReadOnlyCollection<string> ids,
            bool shouldIncludeInactive = false,
            IEmployeeServiceUnitOfWork uow = null)
        {
            var currentUow = uow ?? _uowProvider.CurrentUow;
            var employeeRepository = currentUow.GetRepository<Employee>();
            var employeeLoadStrategy = GetEmployeeLoadStrategy();
            var specification = EmployeeSpecification.ByIds(ids);
            if (!shouldIncludeInactive)
            {
                specification &= EmployeeSpecification.Active;
            }

            var employees = await employeeRepository.GetWhereAsync(specification, employeeLoadStrategy);

            return employees;
        }

        public async Task<IReadOnlyCollection<Employee>> GetEmployeesByPeopleIdsAsync(IReadOnlyCollection<string> peopleIds, bool shouldIncludeInactive = false, IEmployeeServiceUnitOfWork uow = null)
        {
            var currentUow = uow ?? _uowProvider.CurrentUow;
            var employeeRepository = currentUow.GetRepository<Employee>();
            var employeeLoadStrategy = GetEmployeeLoadStrategy();
            var specification = EmployeeSpecification.ByPersonIds(peopleIds);
            if (!shouldIncludeInactive)
            {
                specification &= EmployeeSpecification.Active;
            }

            var employees = await employeeRepository.GetWhereAsync(specification, employeeLoadStrategy);

            return employees;
        }

        public async Task<IReadOnlyCollection<Employee>> GetEmployeesByUnitIdsAsync(IReadOnlyCollection<string> unitIds, bool shouldIncludeInactive)
        {
            var uow = _uowProvider.CurrentUow;
            var employeeRepository = uow.GetRepository<Employee>();
            var employeeLoadStrategy = GetEmployeeLoadStrategy();

            var specification = unitIds.Count > 1 ? EmployeeSpecification.ByUnitIds(unitIds) : EmployeeSpecification.ByUnitId(unitIds.Single());
            if (!shouldIncludeInactive)
            {
                specification &= EmployeeSpecification.Active;
            }

            var employees = await employeeRepository.GetWhereAsync(specification, employeeLoadStrategy);

            return employees;
        }

        public async Task<IReadOnlyCollection<Employee>> GetEmployeesByTitleRoleIdsAsync(
            IReadOnlyCollection<string> titleRoleIds,
            bool shouldIncludeInactive = false,
            IEmployeeServiceUnitOfWork uow = null)
        {
            var currentUow = uow ?? _uowProvider.CurrentUow;
            var employeeRepository = currentUow.GetRepository<Employee>();

            var specification = EmployeeSpecification.ByTitleRoleIds(titleRoleIds) &
                                EmployeeSpecification.EmployeeNotInternship;
            if (!shouldIncludeInactive)
            {
                specification &= EmployeeSpecification.Active;
            }

            var employeeLoadStrategy = GetEmployeeLoadStrategy();

            var employees = await employeeRepository.GetWhereAsync(specification, employeeLoadStrategy);

            return employees;
        }

        public async Task<IReadOnlyCollection<Employee>> GetEmployeesByRoleParametersAsync(
            IReadOnlyCollection<RoleConfigurationTitleRole> titleRoles,
            IReadOnlyCollection<RoleConfigurationUnit> units,
            IReadOnlyCollection<RoleConfigurationEmployee> configurationEmployees)
        {
            var uow = _uowProvider.CurrentUow;
            var employeeRepository = uow.GetRepository<Employee>();
            var employeeLoadStrategy = GetEmployeeLoadStrategy();
            Specification<Employee> specification = new FalseSpecification<Employee>();
            if (configurationEmployees.Count > 0)
            {
                var employeeIds = configurationEmployees.Select(e => e.Employee.Id).ToList();
                specification |= EmployeeSpecification.ByIds(employeeIds);
            }

            if (titleRoles.Count > 0)
            {
                var titleRoleIds = titleRoles.Select(e => e.TitleRole.Id).ToList();
                specification |= EmployeeSpecification.ByTitleRoleIds(titleRoleIds);
            }

            if (units.Count > 0)
            {
                var unitIds = units.Select(e => e.UnitId).ToList();
                specification |= EmployeeSpecification.ByUnitIds(unitIds);
            }

            specification &= EmployeeSpecification.EmployeeNotInternship;
            var employees = await employeeRepository.GetWhereAsync(specification, employeeLoadStrategy);

            return employees;
        }

        public async Task<IReadOnlyDictionary<string, Employee>> GetSmgToEmployeesMapAsync(IEmployeeServiceUnitOfWork uow)
        {
            var peopleWithSmgProfiles = await _profileService.GetPeopleWithProfilesAsync<SmgProfileSlimDataContract>(ProfileTypes.Smg, SmgProfileFields);
            var employeeRepository = uow.GetRepository<Employee>();
            var employeeLoadStrategy = GetEmployeeLoadStrategy();
            var employees = await employeeRepository.GetAllAsync(employeeLoadStrategy);
            var employeeByIdMap = employees.ToDictionary(e => e.PersonId);

            var smgToEmployeeMap = peopleWithSmgProfiles
                .Where(p => p.Profile.SmgId != null && employeeByIdMap.ContainsKey(p.Person.Id))
                .ToDictionary(p => p.Profile.SmgId, p =>
                {
                    var employee = employeeByIdMap[p.Person.Id];

                    return employee;
                });

            return smgToEmployeeMap;
        }

        public void CreateEmployees(IReadOnlyCollection<Employee> employees, IEmployeeServiceUnitOfWork uow)
        {
            var now = _environmentInfoService.CurrentUtcDateTime;
            employees.ForEach(e => e.CreationDate = now);
            var employeeRepository = uow.GetRepository<Employee>();
            employeeRepository.AddRange(employees);
        }

        public async Task<Employee> GetEmployeeByPersonIdAsync(string personId)
        {
            var uow = _uowProvider.CurrentUow;
            var employeeRepository = uow.GetRepository<Employee>();
            var employeeLoadStrategy = GetEmployeeLoadStrategy();
            var specification = EmployeeSpecification.ByPersonId(personId);
            var employee = await employeeRepository.GetSingleOrDefaultAsync(specification, employeeLoadStrategy);

            return employee;
        }

        public async Task<Employee> GetEmployeeByIdAsync(string id)
        {
            var uow = _uowProvider.CurrentUow;
            var employeeRepository = uow.GetRepository<Employee>();
            var employeeLoadStrategy = GetEmployeeLoadStrategy();
            var specification = EmployeeSpecification.ById(id);

            var employee = await employeeRepository.GetSingleOrDefaultAsync(specification, employeeLoadStrategy);

            return employee;
        }

        public async Task UpdateEmployeePartiallyAsync(Employee employee, Employee fromEmployee)
        {
            var uow = _uowProvider.CurrentUow;

            var previousTitle = employee.GetTitle();

            employee.SeniorityId = fromEmployee.SeniorityId;
            employee.UpdatedBy = fromEmployee.UpdatedBy;
            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            employee.UpdateDate = currentDate;

            await uow.SaveChangesAsync();

            await AddOrUpdateEmployeeProfileAsync(employee);

            var currentTitle = employee.GetTitle();
            if (previousTitle != currentTitle)
            {
                await UpdateEmployeePersonJobTitleAsync(employee);
            }
        }

        public async Task UpdateEmployeeMaternityLeaveStateAsync(
            Employee employee,
            MaternityLeaveDataContract maternityLeave,
            MaternityLeaveDataContract previousMaternityLeave,
            IEmployeeServiceUnitOfWork uow = null)
        {
            await UpdateEmployeeMaternityLeaveStateAsync(employee, maternityLeave, uow);

            if (maternityLeave.IsDeleted)
            {
                return;
            }

            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            if (maternityLeave.EndDate > currentDate && maternityLeave.EndDate != previousMaternityLeave?.EndDate)
            {
                _jobScheduler.ScheduleJob<EmployeeService>(
                    s => s.UpdateEmployeeMaternityLeaveStateAsync(employee.ExternalId, maternityLeave.Id), maternityLeave.EndDate.Value);
            }

            if (maternityLeave.StartDate > currentDate && maternityLeave.StartDate != previousMaternityLeave?.StartDate)
            {
                _jobScheduler.ScheduleJob<EmployeeService>(
                    s => s.UpdateEmployeeMaternityLeaveStateAsync(employee.ExternalId, maternityLeave.Id), maternityLeave.StartDate);
            }
        }

        public async Task UpdateEmployeeRolesAsync(Employee employee, IReadOnlyCollection<EmployeeRole> fromEmployeeRoles)
        {
            var uow = _uowProvider.CurrentUow;

            employee.Roles.Reconcile(fromEmployeeRoles.ToList(), r => r.RoleId);

            await uow.SaveChangesAsync();

            await AddOrUpdateEmployeeProfileAsync(employee);
        }

        public async Task AddOrUpdateEmployeeProfileAsync(Employee employee)
        {
            var employeeProfile = CreateFrom(employee);
            var result = await _profileService.AddOrUpdateProfileAsync(employee.PersonId, employeeProfile);
            if (!result.IsSuccessful)
            {
                LoggerContext.Current.LogError($"Failed to add or update employee profile {{employeeId}} due to service error {result.ErrorCodes.JoinStrings()}.", employee.ExternalId);
                throw new ArgumentException($"Failed to add or update employee profile {employee.ExternalId} due to service error {result.ErrorCodes.JoinStrings()}.");
            }
        }

        public async Task<IReadOnlyCollection<EmployeeOrganizationChange>> GetEmployeeOrganizationChanges(string employeeId)
        {
            var uow = _uowProvider.CurrentUow;
            var organizationChangeRepository = uow.GetRepository<EmployeeOrganizationChange>();
            var loadStrategy = GetEmployeeOrganizationChangeLoadStrategy();
            var organizationChanges = await organizationChangeRepository.GetWhereAsync(c => c.Employee.ExternalId == employeeId, loadStrategy);

            return organizationChanges;
        }

        public async Task<IReadOnlyCollection<EmployeeCurrentLocationChange>> GetEmployeeCurrentLocationChanges(string employeeId)
        {
            var uow = _uowProvider.CurrentUow;
            var locationChangeRepository = uow.GetRepository<EmployeeCurrentLocationChange>();
            var loadStrategy = GetEmployeeCurrentLocationChangeLoadStrategy();
            var locationChanges = await locationChangeRepository.GetWhereAsync(c => c.Employee.ExternalId == employeeId, loadStrategy);

            return locationChanges;
        }

        [UsedImplicitly]
        public async Task CloseActiveInternshipAsync(string employeeExternalId, string internshipExternalId)
        {
            var employee = await GetEmployeeByIdAsync(employeeExternalId);
            if (employee == null || !employee.IsActive)
            {
                return;
            }

            var internship = await _internshipService.GetInternshipByIdAsync(internshipExternalId);
            if (internship == null || !internship.IsActive)
            {
                return;
            }

            var closeAfterDate = GetCloseAfterDate(employee);
            if (closeAfterDate <= _environmentInfoService.CurrentUtcDate)
            {
                await _internshipService.CloseInternshipAsync(internship, InternshipCloseReason.AutomaticallyDueEmployment);
            }
        }

        [UsedImplicitly]
        public async Task UpdateEmployeeMaternityLeaveStateAsync(string employeeId, string maternityLeaveId)
        {
            var employee = await GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return;
            }

            var maternityLeave = await _timeTrackingService.GetMaternityLeaveAsync(maternityLeaveId);

            await UpdateEmployeeMaternityLeaveStateAsync(employee, maternityLeave);
        }

        public async Task CreateOrUpdateEmployeeFromSmgProfileAsync(PersonDataContract person, SmgProfileDataContract smgProfile, bool shouldCreateEmployee = false)
        {
            using var access = await _resourceLockService.GetAccessAsync(String.Format(UpdateEmployeeResourceName, person.Id));

            var uow = _uowProvider.CurrentUow;
            var employee = await GetEmployeeByPersonIdAsync(person.Id);
            EmployeeChangedEventArgs eventArgs;
            if (employee != null)
            {
                LoggerContext.Current.Log("Updating employee {employeeId} ({domainName})...", employee.ExternalId, smgProfile.DomainName);

                var previousEmployee = employee.Clone();
                var updatedEmployee = await UpdateEmployeeFromSmgProfileAsync(employee, smgProfile, uow);

                if (updatedEmployee.EmploymentDate != previousEmployee.EmploymentDate)
                {
                    await ScheduleCloseActiveInternshipAsync(updatedEmployee);
                }

                await AddOrUpdateEmployeeProfileAsync(updatedEmployee);

                if (previousEmployee.GetTitle() != updatedEmployee.GetTitle())
                {
                    await UpdateEmployeePersonJobTitleAsync(updatedEmployee);
                }

                LoggerContext.Current.Log("Employee updated successfully.");

                eventArgs = new EmployeeChangedEventArgs(previousEmployee, updatedEmployee);
                await EmployeeUpdated.RaiseAsync(eventArgs);

                return;
            }

            if (!shouldCreateEmployee)
            {
                return;
            }

            LoggerContext.Current.Log("Creating employee for person {personId} ({domainName})...", person.Id, smgProfile.DomainName);

            var newEmployee = await CreateEmployeeFromSmgProfileAsync(person, smgProfile, uow);

            await ScheduleCloseActiveInternshipAsync(newEmployee);

            await AddOrUpdateEmployeeProfileAsync(newEmployee);

            await UpdateEmployeePersonJobTitleAsync(newEmployee);

            LoggerContext.Current.Log("Employee {employeeId} created successfully.", newEmployee.ExternalId);

            eventArgs = new EmployeeChangedEventArgs(null, newEmployee);
            await EmployeeCreated.RaiseAsync(eventArgs);
        }


        private async Task UpdateEmployeeMaternityLeaveStateAsync(Employee employee, MaternityLeaveDataContract maternityLeave, IEmployeeServiceUnitOfWork uow = null)
        {
            var isEmployeeInMaternityLeave = !employee.IsActive && employee.DeactivationReason == DeactivationReason.MaternityLeave;
            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            var isMaternityLeaveActive = CheckIfMaternityLeaveIsActive(maternityLeave, currentDate);

            if (!isMaternityLeaveActive)
            {
                var employeeMaternityLeaves = await _timeTrackingService.GetEmployeeMaternityLeavesAsync(employee.ExternalId);
                isMaternityLeaveActive = employeeMaternityLeaves.Any(ml => CheckIfMaternityLeaveIsActive(ml, currentDate));
            }

            if (employee.IsActive || employee.IsDismissed || isEmployeeInMaternityLeave == isMaternityLeaveActive)
            {
                return;
            }

            var currentUow = uow ?? _uowProvider.CurrentUow;

            var previousEmployee = employee.Clone();

            employee.DeactivationReason = isMaternityLeaveActive ? DeactivationReason.MaternityLeave : null;
            employee.UpdateDate = currentDate;

            await currentUow.SaveChangesAsync();

            await AddOrUpdateEmployeeProfileAsync(employee);

            await OnEmployeeMaternityLeaveStateChanged(employee, previousEmployee);
        }

        private async Task ScheduleCloseActiveInternshipAsync(Employee employee)
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.GetRepository<Internship>();
            var internship = await internshipRepository.GetSingleOrDefaultAsync(i => i.PersonId == employee.PersonId && i.IsActive);
            if (internship == null)
            {
                return;
            }

            var closeAfterDate = GetCloseAfterDate(employee);
            _jobScheduler.ScheduleJob<EmployeeService>(
                s => s.CloseActiveInternshipAsync(employee.ExternalId, internship.ExternalId), closeAfterDate.ToDateTime());
        }

        private async Task<Employee> CreateEmployeeFromSmgProfileAsync(PersonDataContract person, SmgProfileDataContract smgProfile, IEmployeeServiceUnitOfWork uow)
        {
            var employee = _smgProfileMapper.CreateEmployeeFrom(person.Id, smgProfile);
            var isIntern = employee.EmploymentType == EmploymentType.Internship;

            await SetEmployeeFieldsPartiallyAsync(smgProfile, employee, isIntern);

            var isSeniorityRequired = employee.IsSeniorityRequired();
            if (isSeniorityRequired)
            {
                var seniorityId = SmgRankMapper.MapToSeniorityId(smgProfile.Rank);
                employee.Seniority = await _seniorityService.GetSeniorityByIdAsync(seniorityId ?? Seniority.Default);
            }
            else
            {
                employee.Seniority = null;
            }

            if (!employee.IsActive)
            {
                employee.DeactivationReason = employee.IsDismissed ? DeactivationReason.Dismissed : null;
            }

            employee.CreationDate = _environmentInfoService.CurrentUtcDateTime;

            var employeeRepository = uow.GetRepository<Employee>();
            employeeRepository.Add(employee);

            await _employeeSnapshotService.CreateSnapshotForEmployeeAsync(employee, uow);

            var eventArgs = new EmployeeChangingEventArgs(employee, uow);
            await EmployeeCreating.RaiseAsync(eventArgs);

            await uow.SaveChangesAsync();

            return employee;
        }

        private async Task<Employee> UpdateEmployeeFromSmgProfileAsync(Employee employee, SmgProfileDataContract smgProfile, IEmployeeServiceUnitOfWork uow)
        {
            var fromEmployee = _smgProfileMapper.CreateEmployeeFrom(employee.PersonId, smgProfile);
            fromEmployee.Id = employee.Id;

            var isInternBefore = employee.EmploymentType == EmploymentType.Internship;
            var isIntern = fromEmployee.EmploymentType == EmploymentType.Internship;

            await SetEmployeeFieldsPartiallyAsync(smgProfile, fromEmployee, isIntern);

            var seniorityId = SmgRankMapper.MapToSeniorityId(smgProfile.Rank);
            fromEmployee.Seniority = await _seniorityService.GetSeniorityByIdAsync(seniorityId ?? Seniority.Default);

            var previousEmployee = employee.Clone();
            await UpdateEmployeePartiallyFromSmgProfileAsync(employee, fromEmployee);
            await _relocationPlanService.HandleEmployeeUpdateAsync(previousEmployee, employee, uow);
            await _employeeChangesService.HandleEmployeeUpdateAsync(previousEmployee, employee, uow);

            if (isInternBefore && !isIntern)
            {
                await _employeeSnapshotService.UpdateLastSnapshotForEmployeeAsync(employee, uow);
            }
            else if (!isInternBefore && isIntern)
            {
                await _employeeSnapshotService.RewriteLastSnapshotEmploymentTypeForEmployeeAsync(employee, uow);
            }

            var eventArgs = new EmployeeChangingEventArgs(previousEmployee, employee, uow);
            await EmployeeUpdating.RaiseAsync(eventArgs);

            await uow.SaveChangesAsync();

            return employee;
        }

        private async Task UpdateEmployeePartiallyFromSmgProfileAsync(Employee employee, Employee fromEmployee)
        {
            DeactivationReason? deactivationReason = null;
            if (!fromEmployee.IsActive)
            {
                var employeeMaternityLeaves = await _timeTrackingService.GetEmployeeMaternityLeavesAsync(employee.ExternalId);
                var currentDate = _environmentInfoService.CurrentUtcDateTime.Date;
                var isMaternityLeaveActive = employeeMaternityLeaves.Any(ml => CheckIfMaternityLeaveIsActive(ml, currentDate));

                deactivationReason = fromEmployee.IsDismissed
                    ? DeactivationReason.Dismissed
                    : isMaternityLeaveActive
                        ? DeactivationReason.MaternityLeave
                        : null;
            }

            var seniority = employee.Seniority;
            var isSeniorityRequired = fromEmployee.IsSeniorityRequired();
            if (isSeniorityRequired && !employee.SeniorityId.HasValue)
            {
                seniority = fromEmployee.Seniority;
            }
            else if (!isSeniorityRequired && employee.SeniorityId.HasValue)
            {
                seniority = null;
            }

            var isUpdated = Reflector.SetProperty(() => employee.UnitId, fromEmployee.UnitId) |
                            Reflector.SetProperty(() => employee.CountryId, fromEmployee.CountryId) |
                            Reflector.SetProperty(() => employee.OrganizationId, fromEmployee.OrganizationId) |
                            Reflector.SetProperty(() => employee.ResponsibleHrManager, fromEmployee.ResponsibleHrManager) |
                            Reflector.SetProperty(() => employee.Mentor, fromEmployee.Mentor) |
                            Reflector.SetProperty(() => employee.DomainName, fromEmployee.DomainName) |
                            Reflector.SetProperty(() => employee.Email, fromEmployee.Email) |
                            Reflector.SetProperty(() => employee.EmploymentDate, fromEmployee.EmploymentDate) |
                            Reflector.SetProperty(() => employee.DismissalDate, fromEmployee.DismissalDate) |
                            Reflector.SetProperty(() => employee.TitleRole, fromEmployee.TitleRole) |
                            Reflector.SetProperty(() => employee.EmploymentOfficeId, fromEmployee.EmploymentOfficeId) |
                            Reflector.SetProperty(() => employee.EmploymentType, fromEmployee.EmploymentType) |
                            Reflector.SetProperty(() => employee.IsActive, fromEmployee.IsActive) |
                            Reflector.SetProperty(() => employee.IsDismissed, fromEmployee.IsDismissed) |
                            Reflector.SetProperty(() => employee.DeactivationReason, deactivationReason) |
                            Reflector.SetProperty(() => employee.Seniority, seniority) |
                            UpdateEmploymentPeriods(employee.EmploymentPeriods, fromEmployee.EmploymentPeriods) |
                            UpdateWageRatePeriods(employee.WageRatePeriods, fromEmployee.WageRatePeriods);

            if (isUpdated)
            {
                employee.UpdateDate = _environmentInfoService.CurrentUtcDateTime;
                employee.UpdatedBy = fromEmployee.UpdatedBy;
            }
        }

        private static bool UpdateEmploymentPeriods(ICollection<EmploymentPeriod> employmentPeriods, ICollection<EmploymentPeriod> fromEmploymentPeriods)
        {
            var isUpdated = employmentPeriods.Reconcile(fromEmploymentPeriods, EmploymentPeriodComparer);

            return isUpdated;
        }

        private static bool UpdateWageRatePeriods(ICollection<WageRatePeriod> wageRatePeriods, ICollection<WageRatePeriod> fromWageRatePeriods)
        {
            var isUpdated = wageRatePeriods.Reconcile(fromWageRatePeriods, WageRatePeriodComparer);

            return isUpdated;
        }

        private async Task SetEmployeeFieldsPartiallyAsync(SmgProfileDataContract smgProfile, Employee employee, bool isIntern)
        {
            var titleRoleId = SmgRankMapper.MapToTitleRoleId(smgProfile.Rank);
            if (!String.IsNullOrEmpty(titleRoleId))
            {
                var titleRole = await _titleRoleService.GetTitleRoleByIdAsync(titleRoleId) ?? new TitleRole
                {
                    ExternalId = titleRoleId,
                    Name = smgProfile.Rank,
                };

                employee.TitleRole = titleRole;
            }
            else if (isIntern)
            {
                var titleRole = await _titleRoleService.GetTitleRoleByIdAsync(TitleRole.BuiltIn.Intern);
                employee.TitleRole = titleRole;
            }

            employee.UnitId = _unitProvider.GetUnitIdByImportId(smgProfile.UnitId);
            employee.CountryId = GetCountryIdByImportId(smgProfile.CountryId);
            employee.OrganizationId = GetOrganizationIdByImportId(smgProfile.OrganizationId);
            await SetEmployeeResponsibleHrManagerAsync(employee, smgProfile);
            await SetEmployeeMentorAsync(employee, smgProfile);

            foreach (var employmentPeriod in employee.EmploymentPeriods)
            {
                employmentPeriod.OrganizationId = GetOrganizationIdByImportId(employmentPeriod.OrganizationId);
            }
        }

        private async Task SetEmployeeResponsibleHrManagerAsync(Employee employee, SmgProfileDataContract smgProfile)
        {
            if (smgProfile.ResponsibleHrManagerId != null)
            {
                var smgIdFilter = new Dictionary<string, object>
                {
                    { nameof(SmgProfileDataContract.SmgId), smgProfile.ResponsibleHrManagerId },
                };
                var responsibleHrManagerSmgProfile = await _profileService.FindPersonWithProfileAsync<SmgProfileDataContract>(ProfileTypes.Smg, smgIdFilter);
                if (responsibleHrManagerSmgProfile == null)
                {
                    LoggerContext.Current.LogWarning(
                        "Failed to get responsible hr manager smg profile by smg id {hrManagerSmgId} for employee {employeeId}.",
                        smgProfile.ResponsibleHrManagerId,
                        employee.ExternalId);
                    employee.ResponsibleHrManager = null;

                    return;
                }

                var responsibleHrManager = await GetEmployeeByIdAsync(responsibleHrManagerSmgProfile.Profile.Id);
                if (responsibleHrManager == null)
                {
                    LoggerContext.Current.LogWarning(
                        "Failed to get responsible hr manager employee by profile id {profileId} for employee {employeeId}.",
                        responsibleHrManagerSmgProfile.Profile.Id,
                        employee.ExternalId);
                    employee.ResponsibleHrManager = null;

                    return;
                }

                employee.ResponsibleHrManager = responsibleHrManager;
            }
            else
            {
                employee.ResponsibleHrManager = null;
            }
        }

        private async Task UpdateEmployeePersonJobTitleAsync(Employee employee)
        {
            var updatePersonJobTitleResult =
                await _profileService.UpdatePersonJobTitleAsync(employee.PersonId, employee.GetTitle());
            if (!updatePersonJobTitleResult.IsSuccessful)
            {
                var serviceError = updatePersonJobTitleResult.ErrorCodes.JoinStrings();
                LoggerContext.Current.LogError("Failed to update person {personId} job title due to service error {serviceError}.", employee.PersonId, serviceError);
                throw new ArgumentException($"Failed to update person {employee.PersonId} job title due to service error {serviceError}.");
            }
        }

        private async Task SetEmployeeMentorAsync(Employee employee, SmgProfileDataContract smgProfile)
        {
            if (smgProfile.CuratorId != null)
            {
                var smgIdFilter = new Dictionary<string, object>
                {
                    { nameof(SmgProfileDataContract.SmgId), smgProfile.CuratorId },
                };
                var mentorSmgProfile = await _profileService.FindPersonWithProfileAsync<SmgProfileDataContract>(ProfileTypes.Smg, smgIdFilter);
                if (mentorSmgProfile == null)
                {
                    LoggerContext.Current.LogWarning(
                        "Failed to get mentor smg profile by smg id {curatorId} for employee {employeeId}.",
                        smgProfile.CuratorId,
                        employee.ExternalId);
                    employee.Mentor = null;

                    return;
                }

                var mentor = await GetEmployeeByIdAsync(mentorSmgProfile.Profile.Id);
                if (mentor == null)
                {
                    LoggerContext.Current.LogWarning(
                        "Failed to get mentor employee by profile id {profileId} for employee {employeeId}.",
                        mentorSmgProfile.Profile.Id,
                        employee.ExternalId);
                    employee.Mentor = null;

                    return;
                }

                employee.Mentor = mentor;
            }
            else
            {
                employee.Mentor = null;
            }
        }

        private string GetCountryIdByImportId(string importId)
        {
            if (importId == null)
            {
                return null;
            }

            var country = _countryProvider.GetCountryByImportId(importId);

            return country.Id;
        }

        private string GetOrganizationIdByImportId(string importId)
        {
            if (importId == null)
            {
                return null;
            }

            var organization = _organizationProvider.GetOrganizationByImportId(importId);

            return organization.Id;
        }

        private static bool CheckIfMaternityLeaveIsActive(MaternityLeaveDataContract maternityLeave, DateTime currentDate)
        {
            return !maternityLeave.IsDeleted && maternityLeave.StartDate <= currentDate &&
                   (!maternityLeave.EndDate.HasValue || maternityLeave.EndDate >= currentDate);
        }

        private DateOnly GetCloseAfterDate(Employee employee)
        {
            return employee.EmploymentDate.AddDays(_employeeServiceConfiguration.AutomaticallyCloseInternshipFromEmploymentInDays);
        }

        private async Task OnEmployeeMaternityLeaveStateChanged(Employee employee, Employee previousEmployee)
        {
            var employeeChangedArgs = new EmployeeChangedEventArgs(previousEmployee, employee);
            await EmployeeMaternityLeaveStateUpdated.RaiseAsync(employeeChangedArgs);
        }

        private static IEntityLoadStrategy<Employee> GetEmployeeSlimLoadStrategy()
        {
            IEntityLoadStrategy<Employee> loadStrategy = new EntityLoadStrategy<Employee>(
                e => e.Seniority,
                e => e.CurrentLocation.Location,
                e => e.TitleRole,
                e => e.EmploymentPeriods,
                e => e.WageRatePeriods,
                e => e.Mentor);

            return loadStrategy;
        }

        private static IEntityLoadStrategy<Employee> GetEmployeeLoadStrategy()
        {
            return new EntityLoadStrategy<Employee>(
                e => e.ResponsibleHrManager,
                e => e.Roles.Select(r => r.Role),
                e => e.Workplaces.Select(w => w.Workplace))
                .MergeWith(GetEmployeeSlimLoadStrategy());
        }

        private static IEntityLoadStrategy<EmployeeCurrentLocationChange> GetEmployeeCurrentLocationChangeLoadStrategy()
        {
            return new EntityLoadStrategy<EmployeeCurrentLocationChange>(c => c.Employee, c => c.PreviousLocation, c => c.NewLocation);
        }

        private static IEntityLoadStrategy<EmployeeOrganizationChange> GetEmployeeOrganizationChangeLoadStrategy()
        {
            return new EntityLoadStrategy<EmployeeOrganizationChange>(c => c.Employee);
        }

        private EmployeeProfileDataContract CreateFrom(Employee employee)
        {
            var employmentPeriods = employee.EmploymentPeriods.Select(CreateFrom).ToList();

            var currentLocationCountryId = employee.CurrentLocation?.Location.CountryId;
            CountryDataContract currentLocationCountry = null;
            if (currentLocationCountryId != null)
            {
                _countryProvider.TryGetCountry(currentLocationCountryId, out currentLocationCountry);
            }

            CityDataContract city = null;

            if (employee.EmploymentOfficeId != null)
            {
                var office = _officeProvider.GetOffice(employee.EmploymentOfficeId);
                city = office?.City;
            }

            var location = _employeeLocationProvider.GetLocation(employee);
            var employmentType = EmploymentTypeCreator.CreateFrom(employee.EmploymentType);

            var employeeProfile = new EmployeeProfileDataContract
            {
                Id = employee.ExternalId,
                Type = ProfileTypes.Employee,
                IsActive = employee.IsActive,
                DomainName = employee.DomainName,
                Email = employee.Email,
                EmploymentPeriods = employmentPeriods,
                UnitId = employee.UnitId,
                CountryId = employee.CountryId,
                OrganizationId = employee.OrganizationId,
                Title = employee.GetTitle(),
                TitleRole = employee.TitleRole?.ExternalId,
                Roles = employee.Roles.Select(r => r.Role.ExternalId).ToList(),
                EmploymentDate = employee.EmploymentDate.ToDateTime(),
                Location = location,
                Workplaces = employee.Workplaces.Select(w => w.Workplace.Name).ToList(),
                EmploymentType = employmentType,
                City = city?.Name,
                CurrentLocationCity = employee.CurrentLocation?.Location.Name,
                CurrentLocationCountry = currentLocationCountry?.Name,
            };

            return employeeProfile;
        }

        private static EmploymentPeriodDataContract CreateFrom(EmploymentPeriod employmentPeriod)
        {
            return new EmploymentPeriodDataContract
            {
                StartDate = employmentPeriod.StartDate.ToDateTime(),
                EndDate = employmentPeriod.EndDate?.ToDateTime(),
                OrganizationId = employmentPeriod.OrganizationId,
                IsInternship = employmentPeriod.IsInternship,
            };
        }



        private sealed class WageRatePeriodEqualityComparer : IEqualityComparer<WageRatePeriod>
        {
            public bool Equals(WageRatePeriod x, WageRatePeriod y)
            {
                if (x == null || y == null)
                {
                    return x == null && y == null;
                }

                return x.EmployeeId == y.EmployeeId &&
                       x.StartDate == y.StartDate &&
                       Nullable.Equals(x.EndDate, y.EndDate) &&
                       x.Rate == y.Rate;
            }

            public int GetHashCode(WageRatePeriod obj)
            {
                return HashCode.Combine(obj.EmployeeId, obj.StartDate, obj.EndDate, obj.Rate);
            }
        }

        /// <summary>
        /// Optimized data contract for SMG profiles. Make sure to update SmgProfileFields when adding new fields.
        /// </summary>
        [UsedImplicitly]
        private sealed class SmgProfileSlimDataContract : ProfileDataContract, IHasSmgId
        {
            public string SmgId { get; set; }
        }
    }
}
