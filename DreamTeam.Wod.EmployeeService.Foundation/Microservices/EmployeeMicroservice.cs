using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Observable;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Core.DepartmentService.Units;
using DreamTeam.Core.ProfileService;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Core.TimeTrackingService.DataContracts;
using DreamTeam.DomainModel;
using DreamTeam.Foundation;
using DreamTeam.Identity;
using DreamTeam.Logging;
using DreamTeam.Microservices.Annotations;
using DreamTeam.Microservices.DataContracts;
using DreamTeam.Microservices.DomainModel.DataContracts;
using DreamTeam.Microservices.DomainModel.Extensions;
using DreamTeam.Microservices.Implementation;
using DreamTeam.Migrations;
using DreamTeam.Services.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Countries;
using DreamTeam.Wod.EmployeeService.Foundation.CurrentLocations;
using DreamTeam.Wod.EmployeeService.Foundation.DataContracts;
using DreamTeam.Wod.EmployeeService.Foundation.DismissalRequests;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.EmployeeUnitHistory;
using DreamTeam.Wod.EmployeeService.Foundation.EmploymentRequests;
using DreamTeam.Wod.EmployeeService.Foundation.Internships;
using DreamTeam.Wod.EmployeeService.Foundation.Offices;
using DreamTeam.Wod.EmployeeService.Foundation.Organizations;
using DreamTeam.Wod.EmployeeService.Foundation.RelocationApprovers;
using DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans;
using DreamTeam.Wod.EmployeeService.Foundation.Roles;
using DreamTeam.Wod.EmployeeService.Foundation.SeniorityManagement;
using DreamTeam.Wod.EmployeeService.Foundation.StudentLabSync;
using DreamTeam.Wod.EmployeeService.Foundation.TitleRoles;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;
using DreamTeam.Wod.EmployeeService.Foundation.WspSync;
using DreamTeam.Wod.WspService;
using Employee = DreamTeam.Wod.EmployeeService.DomainModel.Employee;
using EmploymentPeriodDataContract = DreamTeam.Wod.EmployeeService.Foundation.DataContracts.EmploymentPeriodDataContract;
using EmploymentType = DreamTeam.Wod.EmployeeService.DomainModel.EmploymentType;
using Language = DreamTeam.Services.DataContracts.Language;
using PaginationDirection = DreamTeam.DomainModel.PaginationDirection;
using WageRatePeriodDataContract = DreamTeam.Wod.EmployeeService.Foundation.DataContracts.WageRatePeriodDataContract;
using WorkplaceDataContract = DreamTeam.Wod.EmployeeService.Foundation.DataContracts.WorkplaceDataContract;

namespace DreamTeam.Wod.EmployeeService.Foundation.Microservices
{
    [UsedImplicitly]
    public sealed class EmployeeMicroservice : Microservice, IEmployeeMicroservice, IWodObservable
    {
        private const string EmployeeCreatedEvent = "EmployeeService.EmployeeCreated";
        private const string EmployeeUpdatedEvent = "EmployeeService.EmployeeUpdated";

        private const string InternshipCreatedEvent = "EmployeeService.InternshipCreated";
        private const string InternshipUpdatedEvent = "EmployeeService.InternshipUpdated";
        private const string InternshipDeletedEvent = "EmployeeService.InternshipDeleted";

        private const string RelocationPlanCreatedEvent = "EmployeeService.RelocationPlanCreated";
        private const string RelocationPlanUpdatedEvent = "EmployeeService.RelocationPlanUpdated";
        private const string RelocationPlanClosedEvent = "EmployeeService.RelocationPlanClosed";

        private const string InternshipDomainNameGeneratedEvent = "EmployeeService.InternshipDomainNameGenerated";

        private const string RoleConfigurationCreatedEvent = "EmployeeService.RoleConfigurationCreated";
        private const string RoleConfigurationUpdatedEvent = "EmployeeService.RoleConfigurationUpdated";

        private const string DismissalRequestCreatedEvent = "EmployeeService.DismissalRequestCreated";
        private const string DismissalRequestUpdatedEvent = "EmployeeService.DismissalRequestUpdated";

        private const string EmployeeSnapshotsChangedEvent = "EmployeeService.EmployeeSnapshotsChanged";

        private const string EmployeeUnitHistoryCreatedEvent = "EmployeeService.EmployeeUnitHistoryCreated";
        private const string EmployeeUnitHistoryUpdatedEvent = "EmployeeService.EmployeeUnitHistoryUpdated";
        private const string EmployeeUnitHistoryDeletedEvent = "EmployeeService.EmployeeUnitHistoryDeleted";

        private static readonly IReadOnlyDictionary<string, Language> DefaultCountryLanguages;
        private static readonly Func<DismissalRequest, bool> CheckIfDismissalRequestIsLinked;

        private readonly IMigrator _migrator;
        private readonly IEmployeeService _employeeService;
        private readonly IDisplayManagerProvider _displayManagerProvider;
        private readonly ISeniorityService _seniorityService;
        private readonly IRoleService _roleService;
        private readonly ITitleRoleService _titleRoleService;
        private readonly IInternshipService _internshipService;
        private readonly IProfileService _profileService;
        private readonly IDepartmentService _departmentService;
        private readonly ICurrentLocationService _currentLocationService;
        private readonly IRelocationPlanService _relocationPlanService;
        private readonly IDomainNameService _domainNameService;
        private readonly IEnvironmentInfoService _environmentInfoService;
        private readonly IStudentLabSyncService _studentLabSyncService;
        private readonly IWspSyncService _wspSyncService;
        private readonly IRelocationApproverService _relocationApproverService;
        private readonly IEmployeeSnapshotService _employeeSnapshotService;
        private readonly ICountryProvider _countryProvider;
        private readonly IOfficeProvider _officeProvider;
        private readonly IOrganizationProvider _organizationProvider;
        private readonly IWspService _wspService;
        private readonly IEmployeeLocationProvider _employeeLocationProvider;
        private readonly IEmploymentRequestService _employmentRequestService;
        private readonly IEmploymentRequestSyncService _employmentRequestSyncService;
        private readonly IDismissalRequestSyncService _dismissalRequestSyncService;
        private readonly IDismissalRequestService _dismissalRequestService;
        private readonly IEmploymentPeriodService _employmentPeriodService;
        private readonly IUnitProvider _unitProvider;
        private readonly IEmployeeUnitHistoryService _employeeUnitHistoryService;
        private readonly IEmployeeUnitHistorySyncService _employeeUnitHistorySyncService;
        private readonly IWageRatePeriodService _wageRatePeriodService;


        public event AsyncObserver<CountryDataContract> CountryChanged;

        public event AsyncObserver<ServiceEntityChanged<CityDataContract>> CityCreated;

        public event AsyncObserver<ServiceEntityChanged<CityDataContract>> CityUpdated;

        public event AsyncObserver<ServiceEntityChanged<UnitDataContract>> UnitChanged;

        public event AsyncObserver<OrganizationDataContract> OrganizationChanged;

        public event AsyncObserver<ServiceEntityChanged<OfficeDataContract>> OfficeCreated;

        public event AsyncObserver<ServiceEntityChanged<OfficeDataContract>> OfficeUpdated;

        static EmployeeMicroservice()
        {
            DefaultCountryLanguages = new Dictionary<string, Language>
            {
                { Core.DepartmentService.DataContracts.Countries.Belarus, Language.Russian },
            };
            CheckIfDismissalRequestIsLinked = DismissalRequestSpecification.IsLinked.Predicate.Compile();
        }

        public EmployeeMicroservice(
            IMigrator migrator,
            IEmployeeService employeeService,
            IDisplayManagerProvider displayManagerProvider,
            ISeniorityService seniorityService,
            IRoleService roleService,
            ITitleRoleService titleRoleService,
            IInternshipService internshipService,
            IProfileService profileService,
            IDepartmentService departmentService,
            ICurrentLocationService currentLocationService,
            IRelocationPlanService relocationPlanService,
            IDomainNameService domainNameService,
            IEnvironmentInfoService environmentInfoService,
            IStudentLabSyncService studentLabSyncService,
            IWspSyncService wspSyncService,
            IRelocationApproverService relocationApproverService,
            IEmployeeSnapshotService employeeSnapshotService,
            ICountryProvider countryProvider,
            IOfficeProvider officeProvider,
            IOrganizationProvider organizationProvider,
            IWspService wspService,
            IEmployeeLocationProvider employeeLocationProvider,
            IEmploymentRequestService employmentRequestService,
            IEmploymentRequestSyncService employmentRequestSyncService,
            IDismissalRequestSyncService dismissalRequestSyncService,
            IDismissalRequestService dismissalRequestService,
            IEmploymentPeriodService employmentPeriodService,
            IUnitProvider unitProvider,
            IEmployeeUnitHistoryService employeeUnitHistoryService,
            IEmployeeUnitHistorySyncService employeeUnitHistorySyncService,
            IWageRatePeriodService wageRatePeriodService)
        {
            _migrator = migrator;
            _employeeService = employeeService;
            _displayManagerProvider = displayManagerProvider;
            _seniorityService = seniorityService;
            _roleService = roleService;
            _titleRoleService = titleRoleService;
            _internshipService = internshipService;
            _profileService = profileService;
            _departmentService = departmentService;
            _currentLocationService = currentLocationService;
            _relocationPlanService = relocationPlanService;
            _domainNameService = domainNameService;
            _environmentInfoService = environmentInfoService;
            _studentLabSyncService = studentLabSyncService;
            _wspSyncService = wspSyncService;
            _relocationApproverService = relocationApproverService;
            _employeeSnapshotService = employeeSnapshotService;
            _countryProvider = countryProvider;
            _officeProvider = officeProvider;
            _organizationProvider = organizationProvider;
            _wspService = wspService;
            _employeeLocationProvider = employeeLocationProvider;
            _employmentRequestService = employmentRequestService;
            _employmentRequestSyncService = employmentRequestSyncService;
            _dismissalRequestSyncService = dismissalRequestSyncService;
            _dismissalRequestService = dismissalRequestService;
            _employmentPeriodService = employmentPeriodService;
            _unitProvider = unitProvider;
            _employeeUnitHistoryService = employeeUnitHistoryService;
            _employeeUnitHistorySyncService = employeeUnitHistorySyncService;
            _wageRatePeriodService = wageRatePeriodService;
        }


        public void Subscribe(AsyncObserver<ServiceEntityChanged<UnitDataContract>> observer)
        {
            UnitChanged += observer;
        }

        public async Task<IReadOnlyCollection<UnitInternshipsCountDataContract>> GetUnitInternshipsCounts()
        {
            var counts = await _internshipService.GetUnitInternshipCountsAsync();
            var unitInternshipsCounts = counts.Select(c => new UnitInternshipsCountDataContract
            {
                UnitId = c.UnitId,
                InternshipsCount = c.InternshipsCount,
            }).ToList();

            return unitInternshipsCounts;
        }

        public async Task<bool> VerifyDomainName(string domainName)
        {
            var verifyResult = await _domainNameService.VerifyDomainNameAsync(domainName);

            return verifyResult.IsSuccessful && verifyResult.Result;
        }

        public async Task<bool> CheckIfDomainNameIsTaken(string domainName)
        {
            var isDomainNameTaken = await _internshipService.CheckIfDomainNameIsTakenAsync(domainName);

            return isDomainNameTaken;
        }

        public async Task<OperationResultDataContract<EmployeeLocationInfoDataContract>> GetEmployeeLocationInfo(string employeeId)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return EntityNotFound<EmployeeLocationInfoDataContract>();
            }

            var relocationPlan = await _relocationPlanService.GetByEmployeeIdAsync(employee.Id);

            var locationInfo = CreateLocationInfoFrom(employee, relocationPlan);

            return locationInfo;
        }

        public async Task<IReadOnlyCollection<EmployeeLocationInfoDataContract>> GetEmployeeLocationInfos(IReadOnlyCollection<string> employeeIds)
        {
            var employees = await _employeeService.GetEmployeesByIdsAsync(employeeIds);
            var employeeDatabaseIds = employees.Select(e => e.Id).ToList();
            var relocationPlans = await _relocationPlanService.GetByEmployeeIdsAsync(employeeDatabaseIds);
            var relocationPlansMap = relocationPlans.ToDictionary(p => p.EmployeeId, p => p);

            var locationInfos = employees.Select(e => CreateLocationInfosFrom(e, relocationPlansMap)).ToList();

            return locationInfos;
        }

        public async Task<IReadOnlyCollection<RelocationPlanHistoryDataContract>> GetEmployeeRelocationPlanHistory(string employeeId)
        {
            var relocationPlans = await _relocationPlanService.GetAllByEmployeeIdAsync(employeeId);
            if (!relocationPlans.Any())
            {
                return [];
            }

            var relocationPlanChanges = await _relocationPlanService.GetEmployeeRelocationPlanChanges(employeeId);
            var organizationChanges = await _employeeService.GetEmployeeOrganizationChanges(employeeId);
            var locationChanges = await _employeeService.GetEmployeeCurrentLocationChanges(employeeId);
            var changesByRelocationPlanMap = relocationPlanChanges.ToGroupedDictionary(c => c.RelocationPlanId);
            var relocationPlanChangeDataContracts = relocationPlans
                .Select(p => CreateFrom(changesByRelocationPlanMap.GetValueOrEmptyCollection(p.Id), organizationChanges, locationChanges, p))
                .ToList();

            return relocationPlanChangeDataContracts;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> GetRelocationPlanByEmployeeId(string employeeId)
        {
            var relocationPlan = await _relocationPlanService.GetByExternalEmployeeIdAsync(employeeId);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }
            var relocationPlanDataContract = CreateFrom(relocationPlan);

            return relocationPlanDataContract;
        }

        public async Task<IReadOnlyCollection<RelocationPlanDataContract>> GetRelocationPlansByEmployeeIds(IReadOnlyCollection<string> employeeIds, bool includeInactive = false)
        {
            var relocationPlans = await _relocationPlanService.GetByExternalEmployeeIdsAsync(employeeIds, includeInactive);
            var relocationPlanDataContracts = relocationPlans.Select(CreateFrom).ToList();

            return relocationPlanDataContracts;
        }

        public async Task<IReadOnlyCollection<CurrentLocationDataContract>> GetActiveRelocationLocations()
        {
            var locations = await _relocationPlanService.GetActiveRelocationLocationsAsync();
            var locationDataContracts = locations.Select(CreateFrom).ToList();

            return locationDataContracts;
        }

        public async Task<OperationResultDataContract<EmployeeDataContract>> GetEmployeeById(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return EntityNotFound<EmployeeDataContract>();
            }

            var employeeDataContract = CreateFrom(employee);

            return employeeDataContract;
        }

        public async Task<OperationResultDataContract<EmployeeDataContract>> GetEmployeeByPersonId(string personId)
        {
            var employee = await _employeeService.GetEmployeeByPersonIdAsync(personId);
            if (employee == null)
            {
                return EntityNotFound<EmployeeDataContract>();
            }

            var employeeDataContract = CreateFrom(employee);

            return employeeDataContract;
        }

        public async Task<OperationResultDataContract<IReadOnlyCollection<EmployeeDataContract>>> GetEmployeeManagers(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return EntityNotFound<IReadOnlyCollection<EmployeeDataContract>>();
            }

            var unitWithParentUnits = _unitProvider.GetUnitWithParentUnitsChain(employee.UnitId, UnitType.Division);
            var managerIds = unitWithParentUnits.SelectMany(u => u.GetManagerIds()).Distinct().ToList();
            var managers = await _employeeService.GetEmployeesByIdsAsync(managerIds);
            var employeeDataContracts = managers.Select(CreateFrom).ToList();

            return employeeDataContracts;
        }

        public async Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesWithRole(string roleId)
        {
            var employees = await _employeeService.GetEmployeesWithRoleAsync(roleId);
            var employeesDataContract = employees.Select(CreateFrom).ToList();

            return employeesDataContract;
        }

        public async Task<OperationResultDataContract<IReadOnlyCollection<EmployeeDataContract>>> GetEmployeeImmediateManagers(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return EntityNotFound<IReadOnlyCollection<EmployeeDataContract>>();
            }

            var managerIds = _unitProvider.GetEmployeeManagerIds(employee.UnitId, employee.ExternalId);

            var managers = await _employeeService.GetEmployeesByIdsAsync(managerIds);
            var employeeDataContracts = managers
                .Select(CreateFrom)
                .OrderBy(m => managerIds.IndexOf(m.Id))
                .ToList();

            return employeeDataContracts;
        }

        public async Task<IReadOnlyDictionary<string, IReadOnlyCollection<EmployeeDataContract>>> GetEmployeeImmediateManagersMap(IReadOnlyCollection<string> ids, bool shouldIncludeInactiveManagers = false)
        {
            var employees = await _employeeService.GetEmployeesByIdsAsync(ids, true);

            var employeeIdManagerEmployeeIdsMap = employees.Select(e =>
            {
                var managerIds = _unitProvider.GetEmployeeManagerIds(e.UnitId, e.ExternalId);

                return new
                {
                    ManagerIds = managerIds,
                    e.ExternalId,
                };
            }).ToDictionary(p => p.ExternalId, p => p.ManagerIds);

            var resultMap = await GetEmployeesOrInternshipsManagersMap(employeeIdManagerEmployeeIdsMap, shouldIncludeInactiveManagers);

            return resultMap;
        }

        public async Task<OperationResultDataContract<IReadOnlyCollection<EmployeeDataContract>>> GetInternshipImmediateManagers(string id)
        {
            var internship = await _internshipService.GetInternshipByIdAsync(id);
            if (internship == null)
            {
                return EntityNotFound<IReadOnlyCollection<EmployeeDataContract>>();
            }

            var unit = _unitProvider.GetUnitById(internship.UnitId);
            var managerIds = unit.GetManagerIds();

            var managers = await _employeeService.GetEmployeesByIdsAsync(managerIds);
            var employeeDataContracts = managers
                .Select(CreateFrom)
                .OrderBy(m => managerIds.IndexOf(m.Id))
                .ToList();
            return employeeDataContracts;
        }

        public async Task<IReadOnlyDictionary<string, IReadOnlyCollection<EmployeeDataContract>>> GetInternshipImmediateManagersMap(IReadOnlyCollection<string> ids, bool shouldIncludeInactiveManagers = false)
        {
            var internships = await _internshipService.GetInternshipsAsync(ids, true);

            var internshipIdManagerEmployeeIdsMap = internships.Select(i =>
            {
                var unit = _unitProvider.GetUnitById(i.UnitId);
                var managerIds = unit.GetManagerIds();

                return new
                {
                    ManagerIds = managerIds,
                    i.ExternalId,
                };
            }).ToDictionary(p => p.ExternalId, p => p.ManagerIds);

            var resultMap = await GetEmployeesOrInternshipsManagersMap(internshipIdManagerEmployeeIdsMap, shouldIncludeInactiveManagers);

            return resultMap;
        }

        public async Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployees(bool shouldIncludeInactive)
        {
            var employees = await _employeeService.GetEmployeesAsync(shouldIncludeInactive, shouldIncludeInterns: true);
            var employeeDataContracts = employees.Select(CreateFrom).ToList();

            return employeeDataContracts;
        }

        public async Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesWithoutInterns(bool shouldIncludeInactive)
        {
            var employees = await _employeeService.GetEmployeesAsync(shouldIncludeInactive, shouldIncludeInterns: false);
            var employeeDataContracts = employees.Select(CreateFrom).ToList();

            return employeeDataContracts;
        }

        public async Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesByIds(IReadOnlyCollection<string> ids, bool shouldIncludeInactive = false)
        {
            var employees = await _employeeService.GetEmployeesByIdsAsync(ids, shouldIncludeInactive);
            var employeeDataContracts = employees.Select(CreateFrom).ToList();

            return employeeDataContracts;
        }

        public async Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesByPeopleIds(IReadOnlyCollection<string> peopleIds, bool shouldIncludeInactive = false)
        {
            var employees = await _employeeService.GetEmployeesByPeopleIdsAsync(peopleIds, shouldIncludeInactive);
            var employeeDataContracts = employees.Select(CreateFrom).ToList();

            return employeeDataContracts;
        }

        public async Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesByUnitId(string unitId, bool shouldIncludeSubUnits, bool shouldIncludeInactive)
        {
            var unitIds = new List<string> { unitId };
            if (shouldIncludeSubUnits)
            {
                var subUnits = _unitProvider.GetSubUnits(unitId, recursive: true);
                unitIds.AddRange(subUnits.Select(u => u.Id));
            }

            var employees = await _employeeService.GetEmployeesByUnitIdsAsync(unitIds, shouldIncludeInactive);
            var employeeDataContracts = employees.Select(CreateFrom).ToList();

            return employeeDataContracts;
        }

        public async Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesByUnitIds(IReadOnlyCollection<string> unitIds, bool shouldIncludeInactive)
        {
            var employees = await _employeeService.GetEmployeesByUnitIdsAsync(unitIds, shouldIncludeInactive);
            var employeeDataContracts = employees.Select(CreateFrom).ToList();

            return employeeDataContracts;
        }

        public async Task<IReadOnlyCollection<string>> GetAllExistingRelocationPlanIds(IReadOnlyCollection<string> idsToCheck = null)
        {
            var relocationPlanIds = await _relocationPlanService.GetAllExistingRelocationPlanIds(idsToCheck);

            return relocationPlanIds;
        }

        public async Task<IReadOnlyCollection<RelocationPlanDataContract>> GetAllRelocationPlans(DateTime? changeDate = null)
        {
            var relocationPlans = await _relocationPlanService.GetAllAsync(changeDate, false);
            var relocationPlanDataContracts = relocationPlans.Select(CreateFrom).ToList();

            return relocationPlanDataContracts;
        }

        public async Task<IReadOnlyCollection<RelocationPlanDataContract>> GetActiveRelocationPlans()
        {
            var relocationPlans = await _relocationPlanService.GetAllAsync();
            var relocationPlanDataContracts = relocationPlans.Select(CreateFrom).ToList();

            return relocationPlanDataContracts;
        }

        public async Task<OperationResultDataContract<EmployeeDataContract>> UpdateEmployee(string id, [Parameter(Name = "employee")] EmployeeDataContract employeeDataContract)
        {
            LoggerContext.Current.Log("Updating employee {employeeId}...", id);
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                LoggerContext.Current.LogWarning("Employee does not exist.");
                return EntityNotFound<EmployeeDataContract>();
            }

            var currentEmployee = CreateFrom(employee);
            var fromEmployee = new Employee
            {
                SeniorityId = employee.SeniorityId,
            };
            if (!String.IsNullOrEmpty(employeeDataContract.SeniorityId))
            {
                var seniority = await _seniorityService.GetSeniorityByIdAsync(employeeDataContract.SeniorityId);
                fromEmployee.SeniorityId = seniority.Id;
            }

            fromEmployee.UpdatedBy = Context.User.GetPersonId();
            await _employeeService.UpdateEmployeePartiallyAsync(employee, fromEmployee);

            LoggerContext.Current.Log("Employee updated successfully.");
            var updatedEmployee = CreateFrom(employee);

            await EmployeeServiceOnEmployeeUpdated(currentEmployee, updatedEmployee);

            return updatedEmployee;
        }

        public async Task<OperationResultDataContract<InternshipDataContract>> GetInternshipById(string id)
        {
            var internship = await _internshipService.GetInternshipByIdAsync(id);
            if (internship == null)
            {
                return EntityNotFound<InternshipDataContract>();
            }

            var internshipDataContract = CreateFrom(internship);

            return internshipDataContract;
        }

        public async Task<InternshipDataContract> GetLastInternshipByDomainName(string domainName)
        {
            var lastInternship = await _internshipService.GetLastInternshipByDomainNameAsync(domainName);
            if (lastInternship == null)
            {
                return null;
            }

            var internshipDataContract = CreateFrom(lastInternship);

            return internshipDataContract;
        }

        public async Task<IReadOnlyCollection<InternshipDataContract>> GetInternships(bool shouldIncludeInactive)
        {
            var internships = await _internshipService.GetInternshipsAsync(shouldIncludeInactive);
            var internshipDataContracts = internships.Select(CreateFrom).ToList();

            return internshipDataContracts;
        }

        public async Task<IReadOnlyCollection<InternshipDataContract>> GetInternships(IReadOnlyCollection<string> ids, bool shouldIncludeInactive = false)
        {
            var internships = await _internshipService.GetInternshipsAsync(ids, shouldIncludeInactive);
            var internshipDataContracts = internships.Select(CreateFrom).ToList();

            return internshipDataContracts;
        }

        public async Task<DreamTeam.Microservices.DataContracts.PaginatedItemsDataContract<InternshipDataContract>> GetInternshipsPaginated(
            IReadOnlyCollection<string> ids,
            DateOnly fromDate,
            DateOnly toDate,
            PaginationDirection direction,
            bool shouldIncludeInactive = false)
        {
            var paginatedInternships = await _internshipService.GetInternshipsPaginatedAsync(ids, fromDate, toDate, direction, shouldIncludeInactive);
            var internshipDataContracts = paginatedInternships.Items.Select(CreateFrom).ToList();

            var paginatedInternshipDataContracts = new DreamTeam.Microservices.DataContracts.PaginatedItemsDataContract<InternshipDataContract>
            {
                Items = internshipDataContracts,
                HasNext = paginatedInternships.HasNext,
            };

            return paginatedInternshipDataContracts;
        }

        public async Task<IReadOnlyCollection<InternshipDataContract>> GetInternshipsByUnitId(string unitId, bool shouldIncludeSubUnits = false, bool shouldIncludeInactive = false)
        {
            var unitIds = new List<string> { unitId };
            if (shouldIncludeSubUnits)
            {
                var subUnits = _unitProvider.GetSubUnits(unitId, recursive: true);
                unitIds.AddRange(subUnits.Select(u => u.Id));
            }

            var internships = await _internshipService.GetInternshipsByUnitIdsAsync(unitIds, shouldIncludeInactive);
            var internshipDataContracts = internships.Select(CreateFrom).ToList();

            return internshipDataContracts;
        }

        public async Task<IReadOnlyCollection<InternshipDataContract>> GetInternshipsByUnitIds(IReadOnlyCollection<string> unitIds, bool shouldIncludeInactive = false)
        {
            var internships = await _internshipService.GetInternshipsByUnitIdsAsync(unitIds, shouldIncludeInactive);
            var internshipDataContracts = internships.Select(CreateFrom).ToList();

            return internshipDataContracts;
        }

        public async Task<IReadOnlyCollection<InternshipDataContract>> GetInternshipsByPersonId(string personId, bool shouldIncludeInactive = false)
        {
            var internships = await _internshipService.GetInternshipsByPersonIdAsync(personId, shouldIncludeInactive);
            var internshipDataContracts = internships.Select(CreateFrom).ToList();

            return internshipDataContracts;
        }

        public async Task<DreamTeam.Microservices.DataContracts.PaginatedItemsDataContract<InternshipDataContract>> GetInternshipsByPersonIdPaginated(
            string personId,
            DateOnly fromDate,
            DateOnly toDate,
            PaginationDirection direction,
            bool shouldIncludeInactive = false)
        {
            var paginatedInternships = await _internshipService.GetInternshipsByPeopleIdsPaginatedAsync([personId], fromDate, toDate, direction, shouldIncludeInactive);
            var internshipDataContracts = paginatedInternships.Items.Select(CreateFrom).ToList();

            var paginatedInternshipDataContracts = new DreamTeam.Microservices.DataContracts.PaginatedItemsDataContract<InternshipDataContract>
            {
                Items = internshipDataContracts,
                HasNext = paginatedInternships.HasNext,
            };

            return paginatedInternshipDataContracts;
        }

        public async Task<OperationResultDataContract<InternshipDataContract>> GetLastInternshipByEmployeeId(string employeeId)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return EntityNotFound<InternshipDataContract>();
            }

            var internship = await _internshipService.GetLastInternshipByPersonIdAsync(employee.PersonId);
            var internshipDataContract = internship != null ? CreateFrom(internship) : null;

            return internshipDataContract;
        }

        public async Task<IReadOnlyCollection<InternshipDataContract>> GetInternshipsByPeopleIds(IReadOnlyCollection<string> peopleIds, bool shouldIncludeInactive = false)
        {
            var internships = await _internshipService.GetInternshipsByPeopleIdsAsync(peopleIds, shouldIncludeInactive);
            var internshipDataContracts = internships.Select(CreateFrom).ToList();

            return internshipDataContracts;
        }

        public async Task<OperationResultDataContract<InternshipDataContract>> CreateInternship([Parameter(Name = "internship")] InternshipDataContract internshipDataContract)
        {
            if (!Validate(internshipDataContract))
            {
                return InvalidArguments<InternshipDataContract>();
            }

            var internship = CreateFrom(internshipDataContract);
            internship.CreatedBy = Context.User.GetPersonId();
            internship.IsDomainNameVerified = true;
            var internshipCreationResult = await _internshipService.CreateInternshipAsync(internship);

            var operationResult = CreateFrom(internshipCreationResult);
            if (internshipCreationResult.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Create, InternshipCreatedEvent, null, operationResult.Result);
            }

            return operationResult;
        }

        public async Task<OperationResultDataContract<InternshipDataContract>> UpdateInternship(string id, [Parameter(Name = "internship")] InternshipDataContract internshipDataContract)
        {
            if (!Validate(internshipDataContract))
            {
                return InvalidArguments<InternshipDataContract>();
            }

            var internship = await _internshipService.GetInternshipByIdAsync(id);
            if (internship == null)
            {
                return EntityNotFound<InternshipDataContract>();
            }

            var currentInternship = CreateFrom(internship);
            var fromInternship = CreateFrom(internshipDataContract);
            fromInternship.UpdatedBy = Context.User.GetPersonId();
            var internshipUpdateResult = await _internshipService.UpdateInternshipAsync(internship, fromInternship);

            var operationResult = CreateFrom(internshipUpdateResult);
            if (internshipUpdateResult.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Update, InternshipUpdatedEvent, currentInternship, operationResult.Result);
            }

            return operationResult;
        }

        public async Task<OperationResultDataContract<InternshipDataContract>> OpenInternship(string id)
        {
            var internship = await _internshipService.GetInternshipByIdAsync(id);
            if (internship == null)
            {
                return EntityNotFound<InternshipDataContract>();
            }

            var currentInternship = CreateFrom(internship);
            var fromInternship = internship.Clone();
            fromInternship.IsActive = true;
            fromInternship.UpdatedBy = Context.User.GetPersonId();

            var internshipUpdateResult = await _internshipService.UpdateInternshipAsync(internship, fromInternship);

            var operationResult = CreateFrom(internshipUpdateResult);
            if (internshipUpdateResult.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Update, InternshipUpdatedEvent, currentInternship, operationResult.Result);
            }

            return operationResult;
        }

        public async Task<OperationResultDataContract<InternshipDataContract>> CloseInternship(string id)
        {
            var internship = await _internshipService.GetInternshipByIdAsync(id);
            if (internship == null)
            {
                return EntityNotFound<InternshipDataContract>();
            }

            var currentInternship = CreateFrom(internship);
            var fromInternship = internship.Clone();
            fromInternship.IsActive = false;
            fromInternship.UpdatedBy = Context.User.GetPersonId();

            var internshipUpdateResult = await _internshipService.UpdateInternshipAsync(internship, fromInternship);

            var operationResult = CreateFrom(internshipUpdateResult);
            if (internshipUpdateResult.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Update, InternshipUpdatedEvent, currentInternship, operationResult.Result);
            }

            return operationResult;
        }

        public async Task DeleteInternship(string id)
        {
            var deletedInternship = await _internshipService.DeleteInternshipAsync(id);
            if (deletedInternship != null)
            {
                var deletedInternshipDataContract = CreateFrom(deletedInternship);
                await PublishEntityChangedEventAsync(EntityChangeType.Delete, InternshipDeletedEvent, deletedInternshipDataContract, null);
            }
        }

        public async Task<IReadOnlyCollection<SeniorityDataContract>> GetAllSeniority()
        {
            var allSeniority = await _seniorityService.GetAllSeniorityAsync();
            var allSeniorityDataContracts = allSeniority.Select(CreateFrom).ToList();

            return allSeniorityDataContracts;
        }

        public async Task<IReadOnlyCollection<RoleDataContract>> GetRoles()
        {
            var roles = await _roleService.GetRolesAsync();
            var roleDataContracts = roles.Select(CreateFrom).ToList();

            return roleDataContracts;
        }

        public async Task<IReadOnlyCollection<RoleWithConfigurationDataContract>> GetRolesWithConfiguration(RoleType? type = null)
        {
            var roleConfigurations = await _roleService.GetRoleConfigurationsAsync(type);
            var roleWithConfigurationDataContracts = roleConfigurations.Select(CreateRoleWithConfigurationFrom).ToList();

            return roleWithConfigurationDataContracts;
        }

        public async Task<OperationResultDataContract<RoleWithConfigurationDataContract>> GetRoleWithConfigurationById(string id)
        {
            var roleConfiguration = await _roleService.GetRoleConfigurationByIdAsync(id);
            if (roleConfiguration == null)
            {
                return EntityNotFound<RoleWithConfigurationDataContract>();
            }

            var roleWithConfigurationDataContract = CreateRoleWithConfigurationFrom(roleConfiguration);

            return roleWithConfigurationDataContract;
        }

        public async Task<OperationResultDataContract<RoleWithConfigurationDataContract>> CreateCustomRole(RoleWithConfigurationDataContract customRoleWithConfigurationDataContract)
        {
            if (!ValidateCustomRole(customRoleWithConfigurationDataContract))
            {
                return InvalidArguments<RoleWithConfigurationDataContract>();
            }

            var role = CreateFrom(customRoleWithConfigurationDataContract);

            role.CreatedBy = Context.User.GetPersonId();

            var createDomainConfigurationResult = await CreateFromAsync(customRoleWithConfigurationDataContract.Configuration, role);

            if (!createDomainConfigurationResult.IsSuccessful)
            {
                return InvalidArguments<RoleWithConfigurationDataContract>();
            }

            var roleCreationResult = await _roleService.CreateRoleWithConfigurationAsync(createDomainConfigurationResult.Result);

            var roleWithConfigurationDataContract = CreateFrom(roleCreationResult);

            if (roleWithConfigurationDataContract.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Create, RoleConfigurationCreatedEvent, null, roleWithConfigurationDataContract.Result);
            }

            return roleWithConfigurationDataContract;
        }


        public async Task<OperationResultDataContract<RoleWithConfigurationDataContract>> UpdateRoleWithConfiguration(RoleWithConfigurationDataContract roleWithConfiguration)
        {
            var existingConfiguration = await _roleService.GetRoleConfigurationByIdAsync(roleWithConfiguration.Id);
            if (existingConfiguration == null)
            {
                return EntityNotFound<RoleWithConfigurationDataContract>();
            }

            if (!Validate(roleWithConfiguration))
            {
                return InvalidArguments<RoleWithConfigurationDataContract>();
            }

            var role = CreateFrom(roleWithConfiguration);

            var createDomainConfigurationResult = await CreateFromAsync(roleWithConfiguration.Configuration, role);
            if (!createDomainConfigurationResult.IsSuccessful)
            {
                return InvalidArguments<RoleWithConfigurationDataContract>();
            }

            var fromConfiguration = createDomainConfigurationResult.Result;
            fromConfiguration.UpdatedBy = Context.User.GetPersonId();
            var previousConfiguration = CreateRoleWithConfigurationFrom(existingConfiguration);
            var updatedConfiguration = await _roleService.UpdateRoleConfigurationAsync(existingConfiguration, fromConfiguration);

            var roleWithConfigurationDataContract = CreateFrom(updatedConfiguration);

            if (updatedConfiguration.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Update, RoleConfigurationUpdatedEvent, previousConfiguration, roleWithConfigurationDataContract.Result);
            }

            return roleWithConfigurationDataContract;
        }

        public async Task<OperationResultDataContract> DeleteCustomRole(string id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);

            if (role == null)
            {
                return Success();
            }

            var deleteRoleResult = await _roleService.DeleteRoleAsync(role);

            var operationResult = CreateFrom(deleteRoleResult);

            return operationResult;
        }

        public async Task<IReadOnlyCollection<CurrentLocationDataContract>> GetCurrentLocations(bool shouldIncludeCustom = false)
        {
            var currentLocations = await _currentLocationService.GetAsync(shouldIncludeCustom);
            var currentLocationDataContracts = currentLocations.Select(CreateFrom).ToList();

            return currentLocationDataContracts;
        }

        public async Task<CurrentLocationDataContract> CreateCurrentLocation(string name, string countryId = null)
        {
            var createdLocation = await _currentLocationService.GetOrCreateAsync(name, null, countryId);
            var createdLocationDataContract = CreateFrom(createdLocation);

            return createdLocationDataContract;
        }

        public async Task<OperationResultDataContract> UpdateRelocationPlanPartially(IReadOnlyCollection<RelocationPlanUpdateDataContract> relocationPlanUpdates)
        {
            if (relocationPlanUpdates.Count != relocationPlanUpdates.Select(u => u.EmployeeId).Distinct().Count())
            {
                return InvalidArguments();
            }

            var domainRelocationPlanUpdates = relocationPlanUpdates.Select(CreateFrom).ToList();
            var employeeIds = relocationPlanUpdates.Select(u => u.EmployeeId).ToList();
            var relocationPlans = await _relocationPlanService.GetByExternalEmployeeIdsAsync(employeeIds);
            var relocationPlanDataContracts = relocationPlans.Select(CreateFrom).ToList();
            var currentPersonId = Context.User.GetPersonId();
            var updateGmVisaStatesResult = await _relocationPlanService.UpdateGmStatusesAsync(relocationPlans, domainRelocationPlanUpdates, currentPersonId);
            if (!updateGmVisaStatesResult.IsSuccessful)
            {
                return InvalidArguments();
            }

            var relocationPlanDataContractMap = relocationPlanDataContracts.ToDictionary(p => p.EmployeeId);
            var updatedRelocationPlans = updateGmVisaStatesResult.Result;
            var updatedRelocationPlanDataContracts = updatedRelocationPlans.Select(CreateFrom).ToList();
            foreach (var updatedRelocationPlan in updatedRelocationPlanDataContracts)
            {
                var relocationPlan = relocationPlanDataContractMap[updatedRelocationPlan.EmployeeId];
                await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, relocationPlan, updatedRelocationPlan);
            }

            return Success();
        }

        public async Task<IReadOnlyCollection<TitleRoleDataContract>> GetTitleRoles()
        {
            var titleRoles = await _titleRoleService.GetTitleRolesAsync();
            var titleRoleDataContracts = titleRoles.Select(CreateFrom).ToList();

            return titleRoleDataContracts;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> GetRelocationPlanById(string id)
        {
            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            var relocationPlanDataContract = CreateFrom(relocationPlan);

            return relocationPlanDataContract;
        }

        public async Task<OperationResultDataContract<int>> GetPendingApprovalRelocationPlanCountByEmployeeId(string employeeId)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return EntityNotFound<int>();
            }

            var pendingApprovalCount = await _relocationPlanService.GetPendingApprovalRelocationPlanCountByApproverIdAsync(employee.Id);

            return pendingApprovalCount;
        }

        public async Task<IReadOnlyCollection<RelocationPlanDataContract>> GetRelocationPlansByStatusId(string statusId)
        {
            var relocationPlans = await _relocationPlanService.GetByStatusIdAsync(statusId);
            var relocationPlanDataContracts = relocationPlans.Select(CreateFrom).ToList();

            return relocationPlanDataContracts;
        }

        public async Task<IReadOnlyCollection<RelocationPlanDataContract>> GetConfirmedRelocationPlans(DateTime from, DateTime to)
        {
            var relocationPlans = await _relocationPlanService.GetConfirmedAsync(from, to);
            var relocationPlanDataContracts = relocationPlans.Select(CreateFrom).ToList();

            return relocationPlanDataContracts;
        }

        public async Task<IReadOnlyCollection<RelocationPlanDataContract>> GetPendingInductionRelocationPlans(DateTime from, DateTime to)
        {
            var relocationPlans = await _relocationPlanService.GetPendingInductionAsync(from, to);
            var relocationPlanDataContracts = relocationPlans.Select(CreateFrom).ToList();

            return relocationPlanDataContracts;
        }

        public async Task<IReadOnlyCollection<RelocationPlanDataContract>> GetPendingConfirmationRelocationPlans(DateTime confirmationDueDate)
        {
            var relocationPlans = await _relocationPlanService.GetPendingConfirmationAsync(confirmationDueDate);
            var relocationPlanDataContracts = relocationPlans.Select(CreateFrom).ToList();

            return relocationPlanDataContracts;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> CreateRelocationPlan(string employeeId, RelocationPlanDataContract relocationPlan, bool isSync = false)
        {
            if (!Validate(relocationPlan))
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            var createRelocationPlanResult = await CreateFromAsync(relocationPlan, employee);
            if (!createRelocationPlanResult.IsSuccessful)
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var domainRelocationPlan = createRelocationPlanResult.Result;
            if (!Validate(domainRelocationPlan, null, isSync))
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            Employee gmManager = null;
            if (relocationPlan.GmManagerId != null)
            {
                gmManager = await _employeeService.GetEmployeeByIdAsync(relocationPlan.GmManagerId);
                if (gmManager == null)
                {
                    LoggerContext.Current.LogWarning("Failed to find GM manager {employeeId}", relocationPlan.GmManagerId);
                }
            }

            domainRelocationPlan.GmManager = gmManager;
            domainRelocationPlan.GmManagerId = gmManager?.Id;
            domainRelocationPlan.GmComment = relocationPlan.GmComment;

            var byPersonId = Context.User.GetPersonId() ?? domainRelocationPlan.Employee.PersonId;
            var relocationPlanCreationResult = await _relocationPlanService.CreateAsync(domainRelocationPlan, byPersonId, isSync);

            var relocationPlanCreationResultDataContract = CreateFrom(relocationPlanCreationResult);
            if (relocationPlanCreationResult.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Create, RelocationPlanCreatedEvent, null, relocationPlanCreationResultDataContract.Result);
            }

            return relocationPlanCreationResultDataContract;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> UpdateRelocationPlan(string employeeId, string id, RelocationPlanDataContract relocationPlan)
        {
            if (!Validate(relocationPlan))
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var existingRelocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (existingRelocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            if (existingRelocationPlan.Employee.ExternalId != employeeId)
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var createRelocationPlanResult = await CreateFromAsync(relocationPlan, existingRelocationPlan.Employee);
            if (!createRelocationPlanResult.IsSuccessful)
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var fromRelocationPlan = createRelocationPlanResult.Result;
            if (!Validate(fromRelocationPlan, existingRelocationPlan))
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var currentPersonId = Context.User.GetPersonId();
            var existingRelocationPlanDataContract = CreateFrom(existingRelocationPlan);
            await _relocationPlanService.UpdateAsync(existingRelocationPlan, fromRelocationPlan, currentPersonId);
            var updatedRelocationPlanDataContract = CreateFrom(existingRelocationPlan);
            await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, existingRelocationPlanDataContract, updatedRelocationPlanDataContract);

            return updatedRelocationPlanDataContract;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> UpdateRelocationPlanGlobalMobilityInfo(string employeeId, string id, RelocationPlanGlobalMobilityInfoDataContract relocationPlanGlobalMobilityInfo)
        {
            if (!Validate(relocationPlanGlobalMobilityInfo))
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            if (relocationPlan.Employee.ExternalId != employeeId)
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var createFromRelocationPlanGlobalMobilityInfoResult = await CreateFromAsync(relocationPlanGlobalMobilityInfo);
            if (!createFromRelocationPlanGlobalMobilityInfoResult.IsSuccessful)
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var fromRelocationPlanGlobalMobilityInfo = createFromRelocationPlanGlobalMobilityInfoResult.Result;
            var currentPersonId = Context.User.GetPersonId();
            var existingRelocationPlanDataContract = CreateFrom(relocationPlan);
            var updateGlobalMobilityInfoResult = await _relocationPlanService.UpdateGlobalMobilityInfoAsync(relocationPlan, fromRelocationPlanGlobalMobilityInfo, currentPersonId);
            var updateGlobalMobilityInfoResultDataContract = CreateFrom(updateGlobalMobilityInfoResult);
            if (updateGlobalMobilityInfoResult.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, existingRelocationPlanDataContract, updateGlobalMobilityInfoResultDataContract.Result);
            }

            return updateGlobalMobilityInfoResultDataContract;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> SetRelocationPlanGmManager(string id, string employeeId)
        {
            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            if (!EmployeeSpecification.EmployeeNotInternship.IsSatisfiedBy(employee))
            {
                LoggerContext.Current.LogWarning("Attempt to proceed internship employee.");

                return InvalidArguments<RelocationPlanDataContract>();
            }

            var currentPersonId = Context.User.GetPersonId();
            var existingRelocationPlanDataContract = CreateFrom(relocationPlan);
            var setGmManagerResult = await _relocationPlanService.SetGmManagerAsync(relocationPlan, employee, currentPersonId);
            var setGmManagerResultDataContract = CreateFrom(setGmManagerResult);
            if (setGmManagerResult.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, existingRelocationPlanDataContract, setGmManagerResultDataContract.Result);
            }

            return setGmManagerResultDataContract;
        }

        public async Task<OperationResultDataContract> SyncRelocationPlan(string id, RelocationPlanDataContract relocationPlanDataContract, RelocationPlanSyncInfoDataContract syncInfo)
        {
            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            var caseProgress = relocationPlanDataContract.RelocationCaseProgress == null
                ? null
                : CreateFrom(relocationPlanDataContract.RelocationCaseProgress);
            var compensation = relocationPlanDataContract.Compensation == null
                ? null
                : CreateFrom(relocationPlanDataContract.Compensation);

            var existingRelocationPlanDataContract = CreateFrom(relocationPlan);
            await _relocationPlanService.SyncRelocationPlanAsync(relocationPlan, caseProgress, compensation, relocationPlanDataContract.SourceId, syncInfo.CaseStatusSourceId, syncInfo.IsCaseStatusHistoryRequired);
            var updatedRelocationPlanDataContract = CreateFrom(relocationPlan);

            await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, existingRelocationPlanDataContract, updatedRelocationPlanDataContract);

            return OperationResultDataContract.CreateSuccessful();
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> ConfirmRelocationPlan(string employeeId, string id)
        {
            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            if (relocationPlan.Employee.ExternalId != employeeId)
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var currentPersonId = Context.User.GetPersonId();
            var existingRelocationPlanDataContract = CreateFrom(relocationPlan);
            var (confirmResult, isConfirmed) = await _relocationPlanService.ConfirmRelocationPlanAsync(relocationPlan, currentPersonId);
            var confirmResultDataContract = CreateFrom(confirmResult);
            if (confirmResult.IsSuccessful && isConfirmed)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, existingRelocationPlanDataContract, confirmResultDataContract.Result);
            }

            return confirmResultDataContract;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> ApproveRelocationPlan(string id)
        {
            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            var currentPersonId = Context.User.GetPersonId();
            var existingRelocationPlanDataContract = CreateFrom(relocationPlan);
            var (approveResult, isApproved) = await _relocationPlanService.ApproveRelocationPlanAsync(relocationPlan, currentPersonId);
            var approveResultDataContract = CreateFrom(approveResult);
            if (approveResult.IsSuccessful && isApproved)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, existingRelocationPlanDataContract, approveResultDataContract.Result);
            }

            return approveResultDataContract;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> UpdateRelocationPlanApproverInfo(string id, RelocationPlanApproverInfoDataContract relocationPlanApproverInfo)
        {
            var isValid = await ValidateAsync(relocationPlanApproverInfo);
            if (!isValid)
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            var currentPersonId = Context.User.GetPersonId();
            var fromRelocationPlanApproverInfo = CreateFrom(relocationPlanApproverInfo);
            var existingRelocationPlanDataContract = CreateFrom(relocationPlan);
            await _relocationPlanService.UpdateApproverInfoAsync(relocationPlan, fromRelocationPlanApproverInfo, currentPersonId);
            var updatedRelocationPlanDataContract = CreateFrom(relocationPlan);
            await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, existingRelocationPlanDataContract, updatedRelocationPlanDataContract);

            return updatedRelocationPlanDataContract;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> SetRelocationPlanApprover(string id, string employeeId)
        {
            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            if (!EmployeeSpecification.EmployeeNotInternship.IsSatisfiedBy(employee))
            {
                LoggerContext.Current.LogWarning("Attempt to proceed internship employee.");

                return InvalidArguments<RelocationPlanDataContract>();
            }

            var currentPersonId = Context.User.GetPersonId();
            var existingRelocationPlanDataContract = CreateFrom(relocationPlan);
            var setApproverResult = await _relocationPlanService.SetApproverAsync(relocationPlan, employee, currentPersonId);
            var setApproverResultDataContract = CreateFrom(setApproverResult);
            if (setApproverResult.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, existingRelocationPlanDataContract, setApproverResultDataContract.Result);
            }

            return setApproverResultDataContract;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> UpdateRelocationPlanHrManagerInfo(string id, RelocationPlanHrManagerInfoDataContract relocationPlanHrManagerInfo)
        {
            if (!Validate(relocationPlanHrManagerInfo))
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            var currentPersonId = Context.User.GetPersonId();
            var fromRelocationPlanHrManagerInfo = CreateFrom(relocationPlanHrManagerInfo);
            var existingRelocationPlanDataContract = CreateFrom(relocationPlan);
            var updateHrManagerInfoResult = await _relocationPlanService.UpdateHrManagerInfoAsync(relocationPlan, fromRelocationPlanHrManagerInfo, currentPersonId);
            var updatedRelocationPlanDataContract = CreateFrom(updateHrManagerInfoResult);

            if (updateHrManagerInfoResult.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, existingRelocationPlanDataContract, updatedRelocationPlanDataContract.Result);
            }

            return updatedRelocationPlanDataContract;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> SetRelocationPlanHrManager(string id, string employeeId)
        {
            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            if (!EmployeeSpecification.EmployeeNotInternship.IsSatisfiedBy(employee))
            {
                LoggerContext.Current.LogWarning("Attempt to proceed internship employee.");

                return InvalidArguments<RelocationPlanDataContract>();
            }

            var currentPersonId = Context.User.GetPersonId();
            var existingRelocationPlanDataContract = CreateFrom(relocationPlan);
            var setHrManagerResult = await _relocationPlanService.SetHrManagerAsync(relocationPlan, employee, currentPersonId);
            var setHrManagerResultDataContract = CreateFrom(setHrManagerResult);
            if (setHrManagerResult.IsSuccessful)
            {
                await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, existingRelocationPlanDataContract, setHrManagerResultDataContract.Result);
            }

            return setHrManagerResultDataContract;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> ConfirmRelocationPlanEmploymentByEmployee(string employeeId, string id)
        {
            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            if (relocationPlan.Employee.ExternalId != employeeId)
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var currentPersonId = Context.User.GetPersonId();
            var existingRelocationPlanDataContract = CreateFrom(relocationPlan);
            var (confirmEmploymentResult, isEmploymentConfirmed) = await _relocationPlanService.ConfirmRelocationPlanEmploymentByEmployeeAsync(relocationPlan, currentPersonId);
            var confirmEmploymentResultDataContract = CreateFrom(confirmEmploymentResult);
            if (confirmEmploymentResultDataContract.IsSuccessful && isEmploymentConfirmed)
            {
                await PublishEntityChangedEventAsync(
                    EntityChangeType.Update,
                    RelocationPlanUpdatedEvent,
                    existingRelocationPlanDataContract,
                    confirmEmploymentResultDataContract.Result);
            }

            return confirmEmploymentResultDataContract;
        }

        public async Task<OperationResultDataContract<RelocationPlanDataContract>> CloseRelocationPlan(
            string employeeId,
            string id,
            RelocationPlanCloseInfoDataContract relocationPlanCloseInfo)
        {
            var relocationPlan = await _relocationPlanService.GetByExternalIdAsync(id);
            if (relocationPlan == null)
            {
                return EntityNotFound<RelocationPlanDataContract>();
            }

            if (relocationPlan.Employee.ExternalId != employeeId)
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            if (!Validate(relocationPlanCloseInfo))
            {
                return InvalidArguments<RelocationPlanDataContract>();
            }

            var currentPersonId = Context.User.GetPersonId();
            await _relocationPlanService.CloseAsync(
                relocationPlan,
                currentPersonId,
                relocationPlanCloseInfo.CloseReason,
                relocationPlanCloseInfo.CloseComment,
                relocationPlanCloseInfo.CloseDate);
            var closedRelocationPlan = CreateFrom(relocationPlan);

            return closedRelocationPlan;
        }

        public async Task<IReadOnlyCollection<RelocationApproverDataContract>> GetRelocationApprovers(string countryId = null)
        {
            var approvers = await _relocationApproverService.GetAsync(countryId);
            var serviceApprovers = approvers.Select(CreateFrom).ToList();

            return serviceApprovers;
        }

        public async Task<IReadOnlyCollection<RelocationApproverDataContract>> GetRelocationApproversByEmployeeId(string employeeId)
        {
            var approvers = await _relocationApproverService.GetByEmployeeIdAsync(employeeId);
            var serviceApprovers = approvers.Select(CreateFrom).ToList();

            return serviceApprovers;
        }

        public async Task<bool> CheckIsEmployeeRelocationApproverOrHasAssignedRequests(string employeeId)
        {
            var isRelocationApproverOrHasAssignedRequests = await _relocationApproverService.CheckIsEmployeeRelocationApproverOrHasAssignedRequestsAsync(employeeId);

            return isRelocationApproverOrHasAssignedRequests;
        }

        public async Task<RelocationApproverDataContract> GetPrimaryRelocationApprover(string countryId)
        {
            var approver = await _relocationApproverService.GetPrimaryAsync(countryId);
            var serviceApprover = approver == null ? null : CreateFrom(approver);

            return serviceApprover;
        }

        public async Task<IReadOnlyCollection<RelocationApproverDataContract>> GetPrimaryRelocationApprovers()
        {
            var approvers = await _relocationApproverService.GetAllPrimaryApproversAsync();
            var serviceApprovers = approvers.Select(CreateFrom).ToList();

            return serviceApprovers;
        }

        public async Task<IReadOnlyCollection<RelocationApproverDataContract>> GetPrimaryRelocationApproversByCountryIds(IReadOnlyCollection<string> countryIds)
        {
            var approvers = await _relocationApproverService.GetPrimaryByCountryIdsAsync(countryIds);
            var serviceApprovers = approvers.Select(CreateFrom).ToList();

            return serviceApprovers;
        }

        public async Task<OperationResultDataContract<IReadOnlyCollection<RelocationApproverDataContract>>> UpdateRelocationApprovers(
            string countryId,
            IReadOnlyCollection<RelocationApproverDataContract> approverDataContracts)
        {
            LoggerContext.Current.Log("Updating country {countryId} relocation approvers", countryId);
            var employeeIds = approverDataContracts.Select(a => a.EmployeeId).ToHashSet();
            var employees = await _employeeService.GetEmployeesByIdsAsync(employeeIds, shouldIncludeInactive: true);
            var employeesMap = employees.ToDictionary(e => e.ExternalId);

            if (!Validate(approverDataContracts, employeesMap))
            {
                return InvalidArguments<IReadOnlyCollection<RelocationApproverDataContract>>();
            }

            var currentPersonId = Context.User.GetPersonId();
            var fromApprovers = approverDataContracts.Select(a => CreateFrom(countryId, a, employeesMap[a.EmployeeId])).ToList();
            var approvers = await _relocationApproverService.UpdateAsync(countryId, fromApprovers, currentPersonId);

            var serviceApprovers = approvers.Select(CreateFrom).ToList();

            LoggerContext.Current.Log("Country {countryId} relocation approvers updated successfully", countryId);

            return serviceApprovers;
        }

        public async Task<RelocationApproverAssignmentsProfileDataContract> GetRelocationApproversAssignmentsProfile(string countryId)
        {
            var approversProfile = await _relocationApproverService.GetApproverAssignmentsProfileAsync(countryId);
            var approversProfileDataContract = CreateFrom(approversProfile);

            return approversProfileDataContract;
        }

        public async Task<OperationResultDataContract<EmployeeCurrentLocationDataContract>> GetEmployeeCurrentLocation(string employeeId)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return EntityNotFound<EmployeeCurrentLocationDataContract>();
            }

            EmployeeCurrentLocationDataContract employeeCurrentLocationDataContract = null;
            if (employee.CurrentLocation != null)
            {
                employeeCurrentLocationDataContract = CreateFrom(employee.CurrentLocation);
            }

            return employeeCurrentLocationDataContract;
        }

        public async Task<OperationResultDataContract<EmployeeCurrentLocationDataContract>> UpdateEmployeeCurrentLocation(
            string employeeId,
            EmployeeCurrentLocationDataContract currentLocation)
        {
            if (!Validate(currentLocation))
            {
                return InvalidArguments<EmployeeCurrentLocationDataContract>();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return EntityNotFound<EmployeeCurrentLocationDataContract>();
            }

            var createCurrentLocationResult = await CreateFromAsync(currentLocation);
            if (!createCurrentLocationResult.IsSuccessful)
            {
                return InvalidArguments<EmployeeCurrentLocationDataContract>();
            }

            var currentEmployee = CreateFrom(employee);

            var domainCurrentLocation = createCurrentLocationResult.Result;
            var personId = Context.User.GetPersonId();
            var updatedLocation = await _currentLocationService.UpdateEmployeeCurrentLocationAsync(employee, domainCurrentLocation, personId);

            var updatedEmployee = CreateFrom(employee);
            await PublishEntityChangedEventAsync(EntityChangeType.Update, EmployeeUpdatedEvent, currentEmployee, updatedEmployee);

            await _employeeService.AddOrUpdateEmployeeProfileAsync(employee);

            var updatedCurrentLocation = CreateFrom(updatedLocation);

            return updatedCurrentLocation;
        }

        public async Task<IReadOnlyCollection<EmployeeSnapshotDataContract>> GetEmployeeSnapshotsPerDay(DateOnly fromDate, DateOnly toDate, bool shouldIncludeInactive = false)
        {
            var employeeSnapshots = await _employeeSnapshotService.GetEmployeeSnapshotsPerDayAsync(fromDate, toDate, shouldIncludeInactive: shouldIncludeInactive);
            var employeeSnapshotDataContracts = employeeSnapshots.Select(CreateFrom).ToList();

            return employeeSnapshotDataContracts;
        }

        public async Task<IReadOnlyCollection<EmployeeSnapshotDataContract>> GetEmployeeSnapshotsPerDayByUnitIds(DateOnly fromDate, DateOnly toDate, IReadOnlyCollection<string> unitIds, bool shouldIncludeInactive = false)
        {
            var employeeSnapshots = await _employeeSnapshotService.GetEmployeeSnapshotsPerDayAsync(fromDate, toDate, unitIds, shouldIncludeInactive);
            var employeeSnapshotDataContracts = employeeSnapshots.Select(CreateFrom).ToList();

            return employeeSnapshotDataContracts;
        }

        public async Task<IReadOnlyCollection<EmployeeSnapshotDataContract>> GetAllEmployeeSnapshots()
        {
            var employeeSnapshots = await _employeeSnapshotService.GetAllEmployeeSnapshotsAsync();
            var employeeSnapshotDataContracts = employeeSnapshots.Select(CreateFrom).ToList();

            return employeeSnapshotDataContracts;
        }

        public async Task<IReadOnlyCollection<EmploymentRequestDataContract>> GetEmploymentRequests(bool includeWithEmployees = false, bool includeInternshipEmploymentRequests = false)
        {
            var employmentRequests = await _employmentRequestService.GetEmploymentRequestsAsync(includeWithEmployees, includeInternshipEmploymentRequests);
            var employmentRequestsDataContracts = employmentRequests.Select(CreateFrom).ToList();

            return employmentRequestsDataContracts;
        }

        public async Task<IReadOnlyCollection<EmploymentRequestDataContract>> GetEmploymentRequestsByIds(IReadOnlyCollection<string> ids, bool includeInternshipEmploymentRequests = false)
        {
            var employmentRequests = await _employmentRequestService.GetByIdsAsync(ids, includeInternshipEmploymentRequests);
            var employmentRequestsDataContracts = employmentRequests.Select(CreateFrom).ToList();

            return employmentRequestsDataContracts;
        }

        public async Task<IReadOnlyCollection<EmployeeUnitHistoryDataContract>> GetAllEmployeeUnitHistory()
        {
            var employeeUnitHistory = await _employeeUnitHistoryService.GetAllAsync();
            var employeeUnitHistoryDataContracts = employeeUnitHistory.Select(CreateFrom).ToList();

            return employeeUnitHistoryDataContracts;
        }

        public async Task<IReadOnlyCollection<EmployeeUnitHistoryDataContract>> GetEmployeeUnitHistoryByEmployeeId(string employeeId)
        {
            var employeeUnitHistory = await _employeeUnitHistoryService.GetByEmployeeIdAsync(employeeId);
            var employeeUnitHistoryDataContracts = employeeUnitHistory.Select(CreateFrom).ToList();

            return employeeUnitHistoryDataContracts;
        }

        public async Task<OperationResultDataContract<byte[]>> GetWorkplaceSchemeImage(string id)
        {
            var getWorkplaceSchemeResult = await _wspService.GetWorkplaceSchemeImageAsync(id);
            if (!getWorkplaceSchemeResult.IsSuccessful)
            {
                return InvalidArguments<byte[]>();
            }

            return getWorkplaceSchemeResult.Result;
        }

        public async Task<EmployeeDynamicRequestsProfileDataContract> GetActiveEmployeeWithoutInternsDynamicRequests(DateOnly fromDate, DateOnly toDate, bool includeRelocations = false)
        {
            var profile = new EmployeeDynamicRequestsProfileDataContract();
            var employmentRequests = await _employmentRequestService.GetByPeriodWithoutInternsAsync(fromDate, toDate);
            profile.EmploymentRequests = employmentRequests.Select(CreateFrom).ToList();
            var dismissalRequests = await _dismissalRequestService.GetByPeriodWithoutInternsAsync(fromDate, toDate);
            profile.DismissalRequests = dismissalRequests.Select(CreateFrom).ToList();
            if (includeRelocations)
            {
                var relocationPlans = await _relocationPlanService.GetSlimAndApprovedByHrManagerDatePeriodWithoutInternsAsync(fromDate, toDate);
                profile.ApprovedRelocationPlans = relocationPlans.Select(CreateInfoFrom).ToList();
            }

            return profile;
        }

        public async Task<IReadOnlyCollection<DismissalRequestDataContract>> GetAllDismissalRequests(bool activeOnly = false)
        {
            var dismissalRequests = await _dismissalRequestService.GetAllAsync(activeOnly);
            var dismissalRequestDataContracts = dismissalRequests.Select(CreateFrom).ToList();

            return dismissalRequestDataContracts;
        }

        public async Task<IReadOnlyCollection<DismissalRequestDataContract>> GetEmployeeDismissalRequests(string employeeId)
        {
            var dismissalRequests = await _dismissalRequestService.GetByEmployeeIdAsync(employeeId);
            var dismissalRequestDataContracts = dismissalRequests.Select(CreateFrom).ToList();

            return dismissalRequestDataContracts;
        }

        public async Task<IReadOnlyCollection<DismissalRequestDataContract>> GetActiveDismissalRequestsByPeriodAndEmployeeIds(DateOnly fromDate, DateOnly toDate, IReadOnlyCollection<string> employeeIds)
        {
            var dismissalRequests = await _dismissalRequestService.GetByPeriodAndEmployeeIdsAsync(fromDate, toDate, employeeIds);
            var dismissalRequestDataContracts = dismissalRequests.Select(CreateFrom).ToList();

            return dismissalRequestDataContracts;
        }

        public async Task<IReadOnlyCollection<DismissalRequestDataContract>> GetActiveDismissalRequestsByEmployeeIds(IReadOnlyCollection<string> employeeIds, IReadOnlyCollection<DismissalRequestType> types = null)
        {
            var dismissalRequests = await _dismissalRequestService.GetByEmployeeIdsAsync(employeeIds, types);
            var dismissalRequestDataContracts = dismissalRequests.Select(CreateFrom).ToList();

            return dismissalRequestDataContracts;
        }

        public async Task<OperationResultDataContract<DismissalRequestDataContract>> GetDismissalRequestById(string id)
        {
            var dismissalRequest = await _dismissalRequestService.GetByIdAsync(id);
            if (dismissalRequest == null)
            {
                return EntityNotFound<DismissalRequestDataContract>();
            }

            var dismissalRequestDataContract = CreateFrom(dismissalRequest);

            return dismissalRequestDataContract;
        }

        public async Task<OperationResultDataContract<DismissalRequestDataContract>> CreateDismissalRequest([Parameter(Name = "dismissalRequest")] DismissalRequestDataContract dismissalRequestDataContract)
        {
            if (!Validate(dismissalRequestDataContract))
            {
                return InvalidArguments<DismissalRequestDataContract>();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(dismissalRequestDataContract.EmployeeId);
            if (employee == null)
            {
                return EntityNotFound<DismissalRequestDataContract>();
            }

            var dismissalRequest = CreateFrom(dismissalRequestDataContract);
            dismissalRequest.EmployeeId = employee.Id;
            dismissalRequest.Employee = employee;
            var newDismissalRequest = await _dismissalRequestService.CreateAsync(dismissalRequest);

            var newDismissalRequestDataContract = CreateFrom(newDismissalRequest);
            await OnDismissalRequestCreated(newDismissalRequestDataContract);

            return newDismissalRequestDataContract;
        }

        public async Task<OperationResultDataContract<DismissalRequestDataContract>> UpdateDismissalRequest(string id, [Parameter(Name = "dismissalRequest")] DismissalRequestDataContract dismissalRequestDataContract)
        {
            if (!Validate(dismissalRequestDataContract))
            {
                return InvalidArguments<DismissalRequestDataContract>();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(dismissalRequestDataContract.EmployeeId);
            if (employee == null)
            {
                return EntityNotFound<DismissalRequestDataContract>();
            }

            var dismissalRequest = await _dismissalRequestService.GetByIdAsync(id);
            if (dismissalRequest == null)
            {
                return EntityNotFound<DismissalRequestDataContract>();
            }

            var currentDismissalRequest = CreateFrom(dismissalRequest);
            var fromDismissalRequest = CreateFrom(dismissalRequestDataContract);
            var updatedDismissalRequest = await _dismissalRequestService.UpdateAsync(dismissalRequest, fromDismissalRequest);

            var updatedDismissalRequestDataContract = CreateFrom(updatedDismissalRequest);
            await OnDismissalRequestUpdated(currentDismissalRequest, updatedDismissalRequestDataContract);

            return updatedDismissalRequestDataContract;
        }

        public async Task<IReadOnlyCollection<EmploymentPeriodDataContract>> GetAllEmploymentPeriods()
        {
            var employmentPeriods = await _employmentPeriodService.GetAllReadOnlyAsync();
            var employmentPeriodDataContracts = employmentPeriods.Select(CreateFrom).ToList();

            return employmentPeriodDataContracts;
        }

        public async Task<IReadOnlyCollection<EmploymentPeriodDataContract>> GetEmploymentPeriodsByDate(DateOnly date)
        {
            var employmentPeriods = await _employmentPeriodService.GetByDateAsync(date);
            var employmentPeriodDataContracts = employmentPeriods.Select(CreateFrom).ToList();

            return employmentPeriodDataContracts;
        }

        public async Task<IReadOnlyCollection<EmploymentPeriodDataContract>> GetEmploymentPeriodsByEmployeeIds(IReadOnlyCollection<string> employeeIds)
        {
            var employmentPeriods = await _employmentPeriodService.GetByEmployeeIdsAsync(employeeIds);
            var employmentPeriodDataContracts = employmentPeriods.Select(CreateFrom).ToList();

            return employmentPeriodDataContracts;
        }

        public async Task<IReadOnlyCollection<EmploymentPeriodDataContract>> GetEmploymentPeriodsForPeriod(DateOnly startDate, DateOnly endDate, IReadOnlyCollection<string> employeeIds = null)
        {
            var employmentPeriods = await _employmentPeriodService.GetForPeriodAsync(startDate, endDate, employeeIds);
            var employmentPeriodDataContracts = employmentPeriods.Select(CreateFrom).ToList();

            return employmentPeriodDataContracts;
        }

        public async Task<DreamTeam.Microservices.DataContracts.PaginatedItemsDataContract<EmploymentPeriodDataContract>> GetEmploymentPeriodsByEmployeeIdPaginated(string employeeId, DateOnly fromDate, DateOnly toDate, PaginationDirection direction)
        {
            var paginatedEmploymentPeriods = await _employmentPeriodService.GetByEmployeeIdPaginatedAsync(employeeId, fromDate, toDate, direction);
            var paginatedEmploymentPeriodDataContracts = CreatePaginatedFrom(paginatedEmploymentPeriods, CreateFrom);

            return paginatedEmploymentPeriodDataContracts;
        }

        public async Task<OperationResultDataContract> RecalculateEmployeeSnapshots(IReadOnlyCollection<string> employeeIds, DateOnly fromDate)
        {
            var recalculateEmployeeSnapshotsResult = await _employeeSnapshotService.RecalculateEmployeeSnapshotsAsync(employeeIds, fromDate);
            var operationResult = CreateFrom(recalculateEmployeeSnapshotsResult);

            return operationResult;
        }

        public async Task<OperationResultDataContract> SyncCountryRelocationSteps(
            IReadOnlyCollection<CountryRelocationStepsDataContract> steps)
        {
            var domainSteps = CreateFrom(steps);
            await _relocationPlanService.SyncCountryRelocationStepsAsync(domainSteps);

            return Success();
        }

        public async Task<IReadOnlyCollection<double>> GetAllWageRates()
        {
            var allWageRates = await _wageRatePeriodService.GetAllWageRatesAsync();

            return allWageRates;
        }


        protected override async Task InitializeAsync()
        {
            _internshipService.InternshipCreated += InternshipServiceOnInternshipCreated;
            _internshipService.InternshipUpdated += InternshipServiceOnInternshipUpdated;
            _roleService.EmployeeUpdated += RoleServiceOnEmployeeUpdated;
            _employeeService.EmployeeMaternityLeaveStateUpdated += EmployeeServiceOnEmployeeMaternityLeaveStateChanged;
            _relocationPlanService.RelocationPlanClosed += RelocationPlanServiceOnRelocationPlanClosed;
            _relocationPlanService.RelocationPlanUpdated += RelocationPlanServiceOnRelocationPlanUpdated;
            _wspSyncService.EmployeeWorkplacesChanged += WspSyncServiceOnEmployeeWorkplacesChanged;
            _dismissalRequestSyncService.DismissalRequestCreated += DismissalRequestSyncServiceOnDismissalRequestCreated;
            _dismissalRequestSyncService.DismissalRequestUpdated += DismissalRequestSyncServiceOnDismissalRequestUpdated;
            _employeeService.EmployeeCreated += EmployeeServiceOnEmployeeCreated;
            _employeeService.EmployeeUpdated += EmployeeServiceOnEmployeeUpdated;
            _employeeUnitHistorySyncService.EmployeeUnitHistoryCreated += EmployeeUnitHistorySyncServiceOnEmployeeUnitHistoryCreated;
            _employeeUnitHistorySyncService.EmployeeUnitHistoryUpdated += EmployeeUnitHistorySyncServiceOnEmployeeUnitHistoryUpdated;
            _employeeUnitHistorySyncService.EmployeeUnitHistoryDeleted += EmployeeUnitHistorySyncServiceOnEmployeeUnitHistoryDeleted;


            await _unitProvider.InitializeAsync(this);
            await _countryProvider.InitializeAsync(this);
            await _officeProvider.InitializeAsync(this);
            await _organizationProvider.InitializeAsync(this);

            var migrationResult = await _migrator.MigrateAsync();
            if (migrationResult.MigrationsAppliedCount > 0)
            {
                await PublishEventAsync(EmployeeSnapshotsChangedEvent);
            }

            _employeeSnapshotService.EmployeeSnapshotsChanged += EmployeeSnapshotServiceOnEmployeeSnapshotsChanged;

            await _studentLabSyncService.ActivateRegularSyncAsync();

            await _wspSyncService.ActivateRegularSyncAsync();

            await _employmentRequestSyncService.ActivateRegularSyncAsync();

            await _dismissalRequestSyncService.ActivateRegularSyncAsync();

            await _employeeUnitHistorySyncService.ActivateRegularSyncAsync();

            await _employeeSnapshotService.InitializeAsync(this);

            await _displayManagerProvider.InitializeAsync();

            _currentLocationService.Initialize(this);

            _relocationPlanService.Initialize();

            _roleService.Initialize(this);

            await base.InitializeAsync();
        }

        [EventHandler(ProfileServiceEvents.PersonCreated)]
        private async Task ProfileServiceOnPersonCreated(PersonDataContract personDataContract)
        {
            await AddOrUpdateEmployeeAsync(personDataContract);
            await AddInternAsync(personDataContract);
        }

        [EventHandler(ProfileServiceEvents.PersonUpdated)]
        private async Task ProfileServiceOnPersonUpdated(PersonDataContract personDataContract)
        {
            await AddOrUpdateEmployeeAsync(personDataContract);
        }

        [EventHandler(ProfileServiceEvents.SmgProfileAddedOrUpdatedTyped)]
        private async Task ProfileServiceOnProfileAddedOrUpdated(PersonWithProfileDataContract<SmgProfileDataContract> personWithProfileDataContract)
        {
            await _employeeService.CreateOrUpdateEmployeeFromSmgProfileAsync(personWithProfileDataContract.Person, personWithProfileDataContract.Profile);
        }

        [EventHandler(TimeTrackingServiceEvents.MaternityLeaveUpdatedEvent)]
        [EventHandler(TimeTrackingServiceEvents.MaternityLeaveCreatedEvent)]
        private async Task TimeTrackingServiceOnMaternityLeaveAddedOrUpdated(ServiceEntityChanged<MaternityLeaveDataContract> maternityLeaveChanged)
        {
            var employee = await _employeeService.GetEmployeeByPersonIdAsync(maternityLeaveChanged.NewValue.EmployeeId);
            if (employee != null)
            {
                await _employeeService.UpdateEmployeeMaternityLeaveStateAsync(employee, maternityLeaveChanged.NewValue, maternityLeaveChanged.PreviousValue);
            }
        }

        [EventHandler(DepartmentServiceEvents.UnitCreated)]
        [EventHandler(DepartmentServiceEvents.UnitUpdated)]
        private async Task DepartmentServiceOnUnitCreatedOrUpdated(ServiceEntityChanged<UnitDataContract> unitChanged)
        {
            await UnitChanged.RaiseAsync(unitChanged);
            await UpdateManagersAndDeputiesRolesAsync(unitChanged);
            await _relocationApproverService.HandleUnitChangedAsync(unitChanged);
        }

        [EventHandler(DepartmentServiceEvents.CountryCreated)]
        [EventHandler(DepartmentServiceEvents.CountryUpdated)]
        private async Task DepartmentServiceOnCountryCreatedOrUpdated(CountryDataContract country)
        {
            await CountryChanged.RaiseAsync(country);
        }

        [EventHandler(DepartmentServiceEvents.CityCreated)]
        private async Task DepartmentServiceOnCityCreated(ServiceEntityChanged<CityDataContract> cityChanged)
        {
            await CityCreated.RaiseAsync(cityChanged);
        }

        [EventHandler(DepartmentServiceEvents.CityUpdated)]
        private async Task DepartmentServiceOnCityUpdated(ServiceEntityChanged<CityDataContract> cityChanged)
        {
            await CityUpdated.RaiseAsync(cityChanged);
        }

        [EventHandler(DepartmentServiceEvents.OrganizationCreated)]
        [EventHandler(DepartmentServiceEvents.OrganizationUpdated)]
        private async Task DepartmentServiceOnOrganizationCreatedOrUpdated(OrganizationDataContract organization)
        {
            await OrganizationChanged.RaiseAsync(organization);
        }

        [EventHandler(DepartmentServiceEvents.OfficeCreated)]
        private async Task DepartmentServiceOnOfficeCreated(ServiceEntityChanged<OfficeDataContract> officeChanged)
        {
            await OfficeCreated.RaiseAsync(officeChanged);
        }

        [EventHandler(DepartmentServiceEvents.OfficeUpdated)]
        private async Task DepartmentServiceOnOfficeUpdated(ServiceEntityChanged<OfficeDataContract> officeChanged)
        {
            await OfficeUpdated.RaiseAsync(officeChanged);
        }

        [EventHandler(RelocationPlanUpdatedEvent)]
        private async Task OnRelocationPlanModified(ServiceEntityChanged<RelocationPlanDataContract> relocationPlanChanged)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(relocationPlanChanged.NewValue.EmployeeId);
            var createRelocationPlanResult = await CreateFromAsync(relocationPlanChanged.PreviousValue, employee);
            if (!createRelocationPlanResult.IsSuccessful)
            {
                return;
            }

            var previousValue = createRelocationPlanResult.Result;
            var newValue = await _relocationPlanService.GetByEmployeeIdAsync(employee.Id);
            if (newValue == null)
            {
                return;
            }

            var personId = newValue.UpdatedBy;
            await _relocationPlanService.AddRelocationPlanUpdateHistoryAsync(previousValue, newValue, personId);
        }

        private async Task UpdateManagersAndDeputiesRolesAsync(ServiceEntityChanged<UnitDataContract> unitChanged)
        {
            var previousManagerIds = new List<string>();
            if (unitChanged.ChangeType != ServiceEntityChangeType.Create)
            {
                previousManagerIds = unitChanged.PreviousValue.DeputyIds
                    .Append(unitChanged.PreviousValue.ManagerId)
                    .Where(id => id != null)
                    .ToList();
            }

            var newManagerIds = unitChanged.NewValue.DeputyIds
                .Append(unitChanged.NewValue.ManagerId)
                .Where(id => id != null)
                .ToList();

            var (added, removed) = previousManagerIds.Diff(newManagerIds, id => id);
            var affectedManagerIds = added.Concat(removed).ToList();

            if (affectedManagerIds.Count > 0)
            {
                await UpdateEmployeeRolesAsync(affectedManagerIds);
            }
        }

        private async Task UpdateEmployeeRolesAsync(IReadOnlyCollection<string> employeeIds)
        {
            var employees = await _employeeService.GetEmployeesByIdsAsync(employeeIds);
            var roleConfigurations = await _roleService.GetRoleConfigurationsAsync();
            foreach (var employee in employees)
            {
                var employeeRoles = _roleService.GetEmployeeRoles(employee, roleConfigurations);
                var (added, removed) = employee.Roles.Diff(employeeRoles.ToList(), r => r.RoleId);
                if (added.Count == 0 && removed.Count == 0)
                {
                    continue;
                }
                var employeeDataContract = CreateFrom(employee);
                await _employeeService.UpdateEmployeeRolesAsync(employee, employeeRoles);

                var updatedEmployeeDataContract = CreateFrom(employee);
                await EmployeeServiceOnEmployeeUpdated(employeeDataContract, updatedEmployeeDataContract);
            }
        }

        private async Task InternshipServiceOnInternshipUpdated(InternshipChangedEventArgs e)
        {
            var previousInternship = CreateFrom(e.PreviousInternship);
            var internship = CreateFrom(e.Internship);
            await PublishEntityChangedEventAsync(EntityChangeType.Update, InternshipUpdatedEvent, previousInternship, internship);
        }

        private async Task InternshipServiceOnInternshipCreated(InternshipChangedEventArgs e)
        {
            var internship = CreateFrom(e.Internship);
            var internshipDomainNameGeneratedEvent = new InternshipDomainNameGeneratedEventDataContract
            {
                InternshipId = internship.Id,
                DomainName = internship.DomainName,
            };

            await PublishEventAsync(InternshipDomainNameGeneratedEvent, internshipDomainNameGeneratedEvent);
            await PublishEntityChangedEventAsync(EntityChangeType.Create, InternshipCreatedEvent, null, internship);
        }

        private async Task RoleServiceOnEmployeeUpdated(EmployeeChangedEventArgs e)
        {
            var previousEmployee = CreateFrom(e.PreviousEmployee);
            var employee = CreateFrom(e.Employee);
            await PublishEntityChangedEventAsync(EntityChangeType.Update, EmployeeUpdatedEvent, previousEmployee, employee);
        }

        private async Task RelocationPlanServiceOnRelocationPlanClosed(RelocationPlanChangedEventArgs e)
        {
            var previousRelocationPlan = CreateFrom(e.PreviousRelocationPlan);
            var newRelocationPlan = CreateFrom(e.RelocationPlan);
            await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanClosedEvent, previousRelocationPlan, newRelocationPlan);
        }

        private async Task RelocationPlanServiceOnRelocationPlanUpdated(RelocationPlanChangedEventArgs e)
        {
            var previousRelocationPlan = CreateFrom(e.PreviousRelocationPlan);
            var newRelocationPlan = CreateFrom(e.RelocationPlan);
            await PublishEntityChangedEventAsync(EntityChangeType.Update, RelocationPlanUpdatedEvent, previousRelocationPlan, newRelocationPlan);
        }

        private async Task EmployeeServiceOnEmployeeMaternityLeaveStateChanged(EmployeeChangedEventArgs e)
        {
            var previousEmployee = CreateFrom(e.PreviousEmployee);
            var employee = CreateFrom(e.Employee);
            await PublishEntityChangedEventAsync(EntityChangeType.Update, EmployeeUpdatedEvent, previousEmployee, employee);
        }

        private async Task WspSyncServiceOnEmployeeWorkplacesChanged(EmployeeChangedEventArgs e)
        {
            var previousEmployee = CreateFrom(e.PreviousEmployee);
            var employee = CreateFrom(e.Employee);
            await PublishEntityChangedEventAsync(EntityChangeType.Update, EmployeeUpdatedEvent, previousEmployee, employee);
        }

        private async Task EmployeeServiceOnEmployeeUpdated(EmployeeDataContract previousEmployee, EmployeeDataContract employee)
        {
            await _relocationApproverService.HandleEmployeeChangedAsync(previousEmployee, employee);
            await _relocationPlanService.HandleEmployeeUpdatedAsync(previousEmployee, employee);

            await PublishEntityChangedEventAsync(EntityChangeType.Update, EmployeeUpdatedEvent, previousEmployee, employee);
        }

        private async Task DismissalRequestSyncServiceOnDismissalRequestCreated(EntityChangedEventArgs<DismissalRequest> e)
        {
            var newDismissalRequest = CreateFrom(e.NewValue);
            await OnDismissalRequestCreated(newDismissalRequest);
        }

        private async Task DismissalRequestSyncServiceOnDismissalRequestUpdated(EntityChangedEventArgs<DismissalRequest> e)
        {
            var previousDismissalRequest = CreateFrom(e.PreviousValue);
            var newDismissalRequest = CreateFrom(e.NewValue);
            await OnDismissalRequestUpdated(previousDismissalRequest, newDismissalRequest);
        }

        private async Task EmployeeServiceOnEmployeeCreated(EmployeeChangedEventArgs e)
        {
            var newEmployee = CreateFrom(e.Employee);
            await PublishEntityChangedEventAsync(EntityChangeType.Create, EmployeeCreatedEvent, null, newEmployee);
        }

        private async Task EmployeeServiceOnEmployeeUpdated(EmployeeChangedEventArgs e)
        {
            var previousEmployee = CreateFrom(e.PreviousEmployee);
            var newEmployee = CreateFrom(e.Employee);
            await EmployeeServiceOnEmployeeUpdated(previousEmployee, newEmployee);
        }

        private async Task EmployeeUnitHistorySyncServiceOnEmployeeUnitHistoryCreated(EntityChangedEventArgs<DomainModel.EmployeeUnitHistory> e)
        {
            var newEmployeeUnitHistory = CreateFrom(e.NewValue);
            await OnEmployeeUnitHistoryCreated(newEmployeeUnitHistory);
        }

        private async Task EmployeeUnitHistorySyncServiceOnEmployeeUnitHistoryUpdated(EntityChangedEventArgs<DomainModel.EmployeeUnitHistory> e)
        {
            var previousEmployeeUnitHistory = CreateFrom(e.PreviousValue);
            var newEmployeeUnitHistory = CreateFrom(e.NewValue);
            await OnEmployeeUnitHistoryUpdated(previousEmployeeUnitHistory, newEmployeeUnitHistory);
        }

        private async Task EmployeeUnitHistorySyncServiceOnEmployeeUnitHistoryDeleted(EntityChangedEventArgs<DomainModel.EmployeeUnitHistory> e)
        {
            var previousEmployeeUnitHistory = CreateFrom(e.PreviousValue);
            await OnEmployeeUnitHistoryDeleted(previousEmployeeUnitHistory);
        }

        private async Task OnDismissalRequestCreated(DismissalRequestDataContract newValue)
        {
            await PublishEntityChangedEventAsync(EntityChangeType.Create, DismissalRequestCreatedEvent, null, newValue);
        }

        private async Task OnDismissalRequestUpdated(DismissalRequestDataContract previousValue, DismissalRequestDataContract newValue)
        {
            await PublishEntityChangedEventAsync(EntityChangeType.Update, DismissalRequestUpdatedEvent, previousValue, newValue);
        }

        private async Task EmployeeSnapshotServiceOnEmployeeSnapshotsChanged()
        {
            await PublishEventAsync(EmployeeSnapshotsChangedEvent);
        }

        private async Task OnEmployeeUnitHistoryCreated(EmployeeUnitHistoryDataContract newValue)
        {
            await PublishEntityChangedEventAsync(EntityChangeType.Create, EmployeeUnitHistoryCreatedEvent, null, newValue);
        }

        private async Task OnEmployeeUnitHistoryUpdated(EmployeeUnitHistoryDataContract previousValue, EmployeeUnitHistoryDataContract newValue)
        {
            await PublishEntityChangedEventAsync(EntityChangeType.Update, EmployeeUnitHistoryUpdatedEvent, previousValue, newValue);
        }

        private async Task OnEmployeeUnitHistoryDeleted(EmployeeUnitHistoryDataContract previousValue)
        {
            await PublishEntityChangedEventAsync(EntityChangeType.Delete, EmployeeUnitHistoryDeletedEvent, previousValue, null);
        }

        private async Task AddOrUpdateEmployeeAsync(PersonDataContract personDataContract)
        {
            var smgProfile = await _profileService.GetProfileAsync<SmgProfileDataContract>(personDataContract.Id, ProfileTypes.Smg);
            if (smgProfile == null)
            {
                return;
            }

            await _employeeService.CreateOrUpdateEmployeeFromSmgProfileAsync(personDataContract, smgProfile, true);
        }

        private async Task AddInternAsync(PersonDataContract personDataContract)
        {
            var smgInternProfile = await _profileService.GetProfileAsync<SmgInternProfileDataContract>(personDataContract.Id, ProfileTypes.SmgIntern);
            if (smgInternProfile == null)
            {
                return;
            }

            var internships = await _internshipService.GetInternshipsByPersonIdAsync(personDataContract.Id, shouldIncludeInactive: false);
            var internship = internships.SingleOrDefault();
            if (internship != null)
            {
                return;
            }

            await _internshipService.CreateInternshipFromSmgInternProfileAsync(personDataContract, smgInternProfile);
        }

        private async Task<Dictionary<string, IReadOnlyCollection<EmployeeDataContract>>> GetEmployeesOrInternshipsManagersMap(
            Dictionary<string, IReadOnlyCollection<string>> employeeOrInternshipIdManagerEmployeeIdsMap,
            bool shouldIncludeInactiveManagers = false)
        {
            var managerEmployeeIds = employeeOrInternshipIdManagerEmployeeIdsMap.SelectMany(pair => pair.Value).ToHashSet();

            var managers = await _employeeService.GetEmployeesByIdsAsync(managerEmployeeIds, shouldIncludeInactiveManagers);

            var resultMap = employeeOrInternshipIdManagerEmployeeIdsMap.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyCollection<EmployeeDataContract>)managers
                    .Where(m => pair.Value.Contains(m.ExternalId))
                    .Select(CreateFrom)
                    .OrderBy(m => pair.Value.IndexOf(m.Id))
                    .ToList());

            return resultMap;
        }

        private async Task<OperationResult<RelocationPlanGlobalMobilityInfo>> CreateFromAsync(RelocationPlanGlobalMobilityInfoDataContract relocationPlanGlobalMobilityInfo)
        {
            Employee gmManager = null;
            if (relocationPlanGlobalMobilityInfo.GmManagerId != null)
            {
                gmManager = await _employeeService.GetEmployeeByIdAsync(relocationPlanGlobalMobilityInfo.GmManagerId);
                if (gmManager == null)
                {
                    LoggerContext.Current.LogWarning("Failed to get GM manager employee {gmManagerId}.", relocationPlanGlobalMobilityInfo.GmManagerId);

                    return OperationResult<RelocationPlanGlobalMobilityInfo>.CreateUnsuccessful();
                }
            }

            return new RelocationPlanGlobalMobilityInfo
            {
                GmComment = relocationPlanGlobalMobilityInfo.GmComment,
                GmManager = gmManager,
                IsInductionPassed = relocationPlanGlobalMobilityInfo.IsInductionPassed,
            };
        }

        private static EmployeeSnapshotDataContract CreateFrom(EmployeeSnapshot snapshot)
        {
            return new EmployeeSnapshotDataContract
            {
                EmployeeId = snapshot.Employee.ExternalId,
                FromDate = snapshot.FromDate,
                ToDate = snapshot.ToDate,
                IsActive = snapshot.IsActive,
                SeniorityId = snapshot.SeniorityId.HasValue ? snapshot.Seniority.ExternalId : null,
                TitleRoleId = snapshot.TitleRoleId.HasValue ? snapshot.TitleRole.ExternalId : null,
                CountryId = snapshot.CountryId,
                OrganizationId = snapshot.OrganizationId,
                UnitId = snapshot.UnitId,
                EmploymentType = snapshot.EmploymentType,
            };
        }

        private static RelocationCaseProgress CreateFrom(RelocationCaseProgressDataContract relocationCaseProgressDataContract)
        {
            var visaProgress = relocationCaseProgressDataContract.VisaProgress == null
                ? new RelocationCaseVisaProgress()
                : CreateFrom(relocationCaseProgressDataContract.VisaProgress);

            return new RelocationCaseProgress
            {
                VisaProgress = visaProgress,
                IsTransferBooked = relocationCaseProgressDataContract.IsTransferBooked,
                IsAccommodationBooked = relocationCaseProgressDataContract.IsAccommodationBooked,
                IsVisaGathered = relocationCaseProgressDataContract.IsVisaGathered,
                TrpState = relocationCaseProgressDataContract.TrpState,
            };
        }

        private static RelocationCaseVisaProgress CreateFrom(RelocationCaseVisaProgressDataContract visaProgressDataContract)
        {
            return new RelocationCaseVisaProgress
            {
                IsScheduled = visaProgressDataContract.IsScheduled,
                AreDocsGathered = visaProgressDataContract.AreDocsGathered,
                AreDocsSentToAgency = visaProgressDataContract.AreDocsSentToAgency,
                IsAttended = visaProgressDataContract.IsAttended,
                IsPassportCollected = visaProgressDataContract.IsPassportCollected,
            };
        }

        private static RelocationPlanApproverInfo CreateFrom(RelocationPlanApproverInfoDataContract relocationPlanApproverInfo)
        {
            return new RelocationPlanApproverInfo
            {
                ApproverComment = relocationPlanApproverInfo.ApproverComment,
                ApproverDate = relocationPlanApproverInfo.ApproverDate,
                Salary = relocationPlanApproverInfo.Salary,
                UnitId = relocationPlanApproverInfo.UnitId,
            };
        }

        private static RelocationPlanHrManagerInfo CreateFrom(RelocationPlanHrManagerInfoDataContract relocationPlanHrManagerInfo)
        {
            return new RelocationPlanHrManagerInfo
            {
                HrManagerComment = relocationPlanHrManagerInfo.HrManagerComment,
                HrManagerDate = relocationPlanHrManagerInfo.HrManagerDate,
            };
        }

        private static PreviousCompensationInfo CreateFrom(PreviousCompensationDataContract previousCompensation)
        {
            return new PreviousCompensationInfo
            {
                Amount = previousCompensation.Amount,
                Currency = previousCompensation.Currency,
            };
        }

        private static CompensationInfoDetailsItem CreateFrom(CompensationDetailsItemDataContract compensationDetailsItem)
        {
            return new CompensationInfoDetailsItem
            {
                Amount = compensationDetailsItem.Amount,
                Enabled = compensationDetailsItem.Enabled,
                NumberOfPeople = compensationDetailsItem.NumberOfPeople,
            };
        }

        private static CompensationInfoDetails CreateFrom(CompensationDetailsDataContract compensationDetails)
        {
            return new CompensationInfoDetails
            {
                Child = CreateFrom(compensationDetails.Child),
                Spouse = CreateFrom(compensationDetails.Spouse),
                Employee = CreateFrom(compensationDetails.Employee),
            };
        }

        private static CompensationInfo CreateFrom(CompensationDataContract compensationDataContract)
        {
            return new CompensationInfo
            {
                Total = compensationDataContract.Total,
                Currency = compensationDataContract.Currency,
                Details = compensationDataContract.Details == null
                ? new CompensationInfoDetails
                {
                    Child = new CompensationInfoDetailsItem(),
                    Spouse = new CompensationInfoDetailsItem(),
                    Employee = new CompensationInfoDetailsItem(),
                }
                : CreateFrom(compensationDataContract.Details),
                PreviousCompensation = compensationDataContract.PreviousCompensation == null ? new PreviousCompensationInfo() : CreateFrom(compensationDataContract.PreviousCompensation),
                PaidInAdvance = compensationDataContract.PaidInAdvance,
            };
        }

        private static PreviousCompensationDataContract CreateFrom(PreviousCompensationInfo previousCompensationInfo)
        {
            return new PreviousCompensationDataContract
            {
                Amount = previousCompensationInfo.Amount,
                Currency = previousCompensationInfo.Currency,
            };
        }

        private static CompensationDetailsItemDataContract CreateFrom(CompensationInfoDetailsItem compensationInfoDetailsItem)
        {
            return new CompensationDetailsItemDataContract
            {
                Amount = compensationInfoDetailsItem.Amount,
                Enabled = compensationInfoDetailsItem.Enabled,
                NumberOfPeople = compensationInfoDetailsItem.NumberOfPeople,
            };
        }

        private static CompensationDetailsDataContract CreateFrom(CompensationInfoDetails compensationInfoDetails)
        {
            return new CompensationDetailsDataContract
            {
                Child = CreateFrom(compensationInfoDetails.Child),
                Spouse = CreateFrom(compensationInfoDetails.Spouse),
                Employee = CreateFrom(compensationInfoDetails.Employee),
            };
        }

        private static CompensationDataContract CreateFrom(CompensationInfo compensationInfo)
        {
            return new CompensationDataContract
            {
                Total = compensationInfo.Total,
                Currency = compensationInfo.Currency,
                Details = CreateFrom(compensationInfo.Details),
                PreviousCompensation = string.IsNullOrEmpty(compensationInfo.PreviousCompensation.Currency) ? null : CreateFrom(compensationInfo.PreviousCompensation),
                PaidInAdvance = compensationInfo.PaidInAdvance,
            };
        }

        private static OperationResultDataContract<RoleDataContract> CreateFrom(EntityManagementResult<Role, RoleManagementError> roleManagementResult)
        {
            if (roleManagementResult.IsSuccessful)
            {
                var roleDataContract = CreateFrom(roleManagementResult.Entity);

                return roleDataContract;
            }

            var errorCodes = roleManagementResult.Errors.Select(GetErrorCode).ToList();

            return OperationResultDataContract<RoleDataContract>.CreateUnsuccessful(errorCodes);
        }

        private OperationResultDataContract<RelocationPlanDataContract> CreateFrom(EntityManagementResult<RelocationPlan, RelocationPlanManagementError> relocationPlanManagementResult)
        {
            if (relocationPlanManagementResult.IsSuccessful)
            {
                var relocationPlanDataContract = CreateFrom(relocationPlanManagementResult.Entity);

                return relocationPlanDataContract;
            }

            var errorCodes = relocationPlanManagementResult.Errors.Select(GetErrorCode).ToList();

            return OperationResultDataContract<RelocationPlanDataContract>.CreateUnsuccessful(errorCodes);
        }

        private static OperationResultDataContract CreateFrom(RecalculateEmployeeSnapshotsStatus recalculateEmployeeSnapshotsStatus)
        {
            switch (recalculateEmployeeSnapshotsStatus)
            {
                case RecalculateEmployeeSnapshotsStatus.Success:
                    return Success();
                case RecalculateEmployeeSnapshotsStatus.InvalidArguments:
                    return InvalidArguments();
                default:
                    throw new ArgumentOutOfRangeException(nameof(recalculateEmployeeSnapshotsStatus), recalculateEmployeeSnapshotsStatus, "Unknown status.");
            }
        }

        private static RelocationPlanUpdate CreateFrom(RelocationPlanUpdateDataContract relocationPlanUpdate)
        {
            return new RelocationPlanUpdate
            {
                EmployeeId = relocationPlanUpdate.EmployeeId,
                IsInductionPassed = relocationPlanUpdate.IsInductionPassed,
            };
        }

        private static DismissalRequest CreateFrom(DismissalRequestDataContract dismissalRequestDataContract)
        {
            var dismissalRequest = new DismissalRequest
            {
                IsActive = dismissalRequestDataContract.IsActive,
                DismissalDate = dismissalRequestDataContract.DismissalDate,
                CloseDate = dismissalRequestDataContract.CloseDate,
            };

            return dismissalRequest;
        }

        private OperationResultDataContract<InternshipDataContract> CreateFrom(EntityManagementResult<Internship, InternshipManagementError> internshipManagementResult)
        {
            if (internshipManagementResult.IsSuccessful)
            {
                var internshipDataContract = CreateFrom(internshipManagementResult.Entity);

                return internshipDataContract;
            }

            var errorCodes = internshipManagementResult.Errors.Select(GetErrorCode).ToList();

            return OperationResultDataContract<InternshipDataContract>.CreateUnsuccessful(errorCodes);
        }

        private EmployeeDataContract CreateFrom(Employee employee)
        {
            var employmentPeriods = employee.EmploymentPeriods.Where(p => !p.IsInternship).Select(CreateFrom).ToList();
            var internshipPeriods = employee.EmploymentPeriods.Where(p => p.IsInternship).Select(CreateFrom).ToList();

            var wageRatePeriods = employee.WageRatePeriods.Select(CreateFrom).ToList();
            var employeeTitle = new TitleDataContract { Name = employee.GetTitle() };
            var seniority = employee.Seniority != null ? CreateFrom(employee.Seniority) : null;
            var unit = _unitProvider.GetUnitById(employee.UnitId);
            var departmentUnit = _unitProvider.GetParentUnit(employee.UnitId, UnitType.Department);
            var unitUnit = _unitProvider.GetParentUnit(employee.UnitId, UnitType.Unit);
            var responsibleHrManagerId = employee.ResponsibleHrManager?.ExternalId;
            var mentorId = employee.MentorId.HasValue ? employee.Mentor.ExternalId : null;
            var displayManagerId = _displayManagerProvider.GetDisplayManagerId(employee);
            var language = GetEmployeeLanguage(employee);
            var workplaces = employee.Workplaces.Select(w => CreateFrom(w.Workplace)).ToList();
            var location = _employeeLocationProvider.GetLocation(employee);
            var titleRole = employee.TitleRoleId.HasValue ? CreateFrom(employee.TitleRole) : null;

            var employeeDataContract = new EmployeeDataContract
            {
                Id = employee.ExternalId,
                PersonId = employee.PersonId,
                IsActive = employee.IsActive,
                DomainName = employee.DomainName,
                EmploymentPeriods = employmentPeriods,
                InternshipPeriods = internshipPeriods,
                WageRatePeriods = wageRatePeriods,
                UnitId = employee.UnitId,
                ParentUnitId = unit.ParentUnitId,
                ResponsibleHrManagerId = responsibleHrManagerId,
                MentorId = mentorId,
                CountryId = employee.CountryId,
                OrganizationId = employee.OrganizationId,
                DisplayManagerId = displayManagerId,
                UnitManagerId = unit.ManagerId,
                IsUnitManager = unit.ManagerId == employee.ExternalId || unit.DeputyIds.Contains(employee.ExternalId),
                DepartmentUnitId = departmentUnit.Id,
                UnitUnitId = unitUnit.Id,
                EmploymentDate = employee.EmploymentDate,
                DismissalDate = employee.DismissalDate,
                Title = employeeTitle,
                SeniorityId = employee.Seniority?.ExternalId,
                Seniority = seniority,
                RoleIds = employee.Roles.Select(r => r.Role.ExternalId).ToList(),
                TitleRoleId = titleRole?.Id,
                TitleRole = titleRole,
                Location = location,
                Workplaces = workplaces,
                IsProduction = unit.IsProduction,
                Language = language,
                CurrentLocation = employee.CurrentLocationId.HasValue ? CreateFrom(employee.CurrentLocation) : null,
                DeactivationReason = employee.DeactivationReason,
                EmploymentOfficeId = employee.EmploymentOfficeId,
                EmploymentType = employee.EmploymentType,
            };

            employeeDataContract.CopyUpdateInfo(employee);

            return employeeDataContract;
        }

        private static EmploymentPeriodDataContract CreateFrom(EmploymentPeriod employmentPeriod)
        {
            return new EmploymentPeriodDataContract
            {
                EmployeeId = employmentPeriod.Employee.ExternalId,
                StartDate = employmentPeriod.StartDate,
                EndDate = employmentPeriod.EndDate,
                OrganizationId = employmentPeriod.OrganizationId,
                IsInternship = employmentPeriod.IsInternship,
            };
        }

        private static WageRatePeriodDataContract CreateFrom(WageRatePeriod wageRatePeriod)
        {
            return new WageRatePeriodDataContract
            {
                EmployeeId = wageRatePeriod.Employee.ExternalId,
                StartDate = wageRatePeriod.StartDate,
                EndDate = wageRatePeriod.EndDate,
                Rate = wageRatePeriod.Rate,
            };
        }

        private static Language GetEmployeeLanguage(Employee employee)
        {
            var country = employee.CountryId ?? Core.DepartmentService.DataContracts.Countries.Default;
            var language = DefaultCountryLanguages.GetValueOrDefault(country, Language.English);

            return language;
        }

        private static WorkplaceDataContract CreateFrom(Workplace workplace)
        {
            return new WorkplaceDataContract
            {
                Id = workplace.ExternalId,
                Name = workplace.Name,
                SchemeUrl = workplace.SchemeUrl,
                OfficeId = workplace.OfficeId,
            };
        }

        private static bool Validate(InternshipDataContract internshipDataContract)
        {
            return !String.IsNullOrEmpty(internshipDataContract.PersonId)
                   && !String.IsNullOrEmpty(internshipDataContract.DomainName)
                   && !String.IsNullOrEmpty(internshipDataContract.UnitId)
                   && !String.IsNullOrEmpty(internshipDataContract.FirstName) && internshipDataContract.FirstName.Length <= Internship.FirstNameLastNameMaxLength
                   && !String.IsNullOrEmpty(internshipDataContract.LastName) && internshipDataContract.LastName.Length <= Internship.FirstNameLastNameMaxLength
                   && !String.IsNullOrEmpty(internshipDataContract.Skype) && internshipDataContract.Skype.Length <= Internship.SkypeMaxLength
                   && !String.IsNullOrEmpty(internshipDataContract.Email) && internshipDataContract.Email.Length <= Internship.EmailMaxLength
                   && !String.IsNullOrEmpty(internshipDataContract.Location) && internshipDataContract.Location.Length <= Internship.LocationMaxLength
                   && (String.IsNullOrEmpty(internshipDataContract.Phone) || internshipDataContract.Phone.Length <= Internship.PhoneMaxLength);
        }

        private static bool Validate(DismissalRequestDataContract dismissalRequest)
        {
            return !String.IsNullOrEmpty(dismissalRequest.EmployeeId);
        }

        private async Task<bool> ValidateAsync(RelocationPlanApproverInfoDataContract relocationPlanApproverInfo)
        {
            if (!String.IsNullOrEmpty(relocationPlanApproverInfo.Salary) && relocationPlanApproverInfo.Salary.Length > RelocationPlan.SalaryMaxLength)
            {
                return false;
            }

            if (!String.IsNullOrEmpty(relocationPlanApproverInfo.UnitId))
            {
                var getUnitResult = await _departmentService.GetUnitByIdAsync(relocationPlanApproverInfo.UnitId);
                if (!getUnitResult.IsSuccessful)
                {
                    return false;
                }
            }

            return String.IsNullOrEmpty(relocationPlanApproverInfo.ApproverComment) || relocationPlanApproverInfo.ApproverComment.Length <= RelocationPlan.CommentMaxLength;
        }

        private static bool Validate(RelocationPlanGlobalMobilityInfoDataContract relocationPlanHrManagerInfo)
        {
            return String.IsNullOrEmpty(relocationPlanHrManagerInfo.GmComment) || relocationPlanHrManagerInfo.GmComment.Length <= RelocationPlan.CommentMaxLength;
        }

        private static bool Validate(RelocationPlanHrManagerInfoDataContract relocationPlanHrManagerInfo)
        {
            return String.IsNullOrEmpty(relocationPlanHrManagerInfo.HrManagerComment) || relocationPlanHrManagerInfo.HrManagerComment.Length <= RelocationPlan.CommentMaxLength;
        }

        private static bool Validate(RelocationPlanCloseInfoDataContract relocationPlanCloseInfo)
        {
            return !String.IsNullOrEmpty(relocationPlanCloseInfo.CloseComment) && relocationPlanCloseInfo.CloseComment.Length <= RelocationPlan.CommentMaxLength;
        }

        private static Internship CreateFrom(InternshipDataContract internshipDataContract)
        {
            var internship = new Internship { PersonId = internshipDataContract.PersonId };
            UpdateFrom(internship, internshipDataContract);

            return internship;
        }

        private static void UpdateFrom(Internship internship, InternshipDataContract internshipDataContract)
        {
            internship.IsActive = internshipDataContract.IsActive;
            internship.FirstName = internshipDataContract.FirstName;
            internship.LastName = internshipDataContract.LastName;
            internship.FirstNameLocal = internshipDataContract.FirstNameLocal;
            internship.LastNameLocal = internshipDataContract.LastNameLocal;
            internship.PhotoId = internshipDataContract.PhotoId;
            internship.Skype = internshipDataContract.Skype;
            internship.Telegram = internshipDataContract.Telegram;
            internship.Phone = internshipDataContract.Phone;
            internship.Email = internshipDataContract.Email;
            internship.Location = internshipDataContract.Location;
            internship.DomainName = internshipDataContract.DomainName;
            internship.UnitId = internshipDataContract.UnitId;
            internship.StartDate = internshipDataContract.StartDate;
            internship.EndDate = internshipDataContract.EndDate;
        }

        private InternshipDataContract CreateFrom(Internship internship)
        {
            var unit = _unitProvider.GetUnitById(internship.UnitId);
            var departmentUnit = _unitProvider.GetParentUnit(internship.UnitId, UnitType.Department);
            var unitUnit = _unitProvider.GetParentUnit(internship.UnitId, UnitType.Unit);

            return new InternshipDataContract
            {
                Id = internship.ExternalId,
                PersonId = internship.PersonId,
                IsActive = internship.IsActive,
                FirstName = internship.FirstName,
                LastName = internship.LastName,
                FirstNameLocal = internship.FirstNameLocal,
                LastNameLocal = internship.LastNameLocal,
                PhotoId = internship.PhotoId,
                Skype = internship.Skype,
                Telegram = internship.Telegram,
                Phone = internship.Phone,
                Email = internship.Email,
                Location = internship.Location,
                DomainName = internship.DomainName,
                IsDomainNameVerified = internship.IsDomainNameVerified,
                UnitId = internship.UnitId,
                DepartmentUnitId = departmentUnit.Id,
                UnitUnitId = unitUnit.Id,
                StudentLabId = internship.StudentLabId,
                StudentLabProfileUrl = internship.StudentLabProfileUrl,
                StartDate = internship.StartDate,
                EndDate = internship.EndDate,
                IsProduction = unit.IsProduction,
                CreatedBy = internship.CreatedBy,
                CreationDate = internship.CreationDate,
                UpdatedBy = internship.UpdatedBy,
                UpdateDate = internship.UpdateDate,
                CloseReason = internship.CloseReason,
            };
        }

        private static bool Validate(RoleWithConfigurationDataContract roleWithConfigurationDataContract)
        {
            return roleWithConfigurationDataContract.IsBuiltIn
                ? ValidateBuiltInRole(roleWithConfigurationDataContract)
                : ValidateCustomRole(roleWithConfigurationDataContract);
        }

        private static bool ValidateCustomRole(RoleWithConfigurationDataContract customRoleWithConfigurationDataContract)
        {
            return !customRoleWithConfigurationDataContract.IsBuiltIn &&
                !String.IsNullOrEmpty(customRoleWithConfigurationDataContract.Name) && customRoleWithConfigurationDataContract.Name.Length <= Role.NameMaxLength &&
                !String.IsNullOrEmpty(customRoleWithConfigurationDataContract.Description) && customRoleWithConfigurationDataContract.Description.Length <= Role.DescriptionMaxLength;
        }

        private static bool ValidateBuiltInRole(RoleWithConfigurationDataContract builtInRoleWithConfigurationDataContract)
        {
            return builtInRoleWithConfigurationDataContract.IsBuiltIn &&
                   !String.IsNullOrEmpty(builtInRoleWithConfigurationDataContract.Name) && builtInRoleWithConfigurationDataContract.Name.Length <= Role.NameMaxLength &&
                   builtInRoleWithConfigurationDataContract.Description == null || builtInRoleWithConfigurationDataContract.Description.Length <= Role.DescriptionMaxLength;
        }

        private static Role CreateFrom(RoleDataContract roleDataContract)
        {
            var role = new Role();
            UpdateFrom(role, roleDataContract);

            return role;
        }

        private static void UpdateFrom(Role role, RoleDataContract roleDataContract)
        {
            role.ExternalId = roleDataContract.Id;
            role.Name = roleDataContract.Name;
            role.Description = roleDataContract.Description;
            role.IsBuiltIn = roleDataContract.IsBuiltIn;
            role.RoleManagerId = roleDataContract.RoleManagerId;
        }

        private static SeniorityDataContract CreateFrom(Seniority seniority)
        {
            return new SeniorityDataContract
            {
                Id = seniority.ExternalId,
                Name = seniority.Name,
                IsHidden = seniority.IsHidden,
                Order = seniority.Order,
            };
        }

        private static RoleDataContract CreateFrom(Role role)
        {
            return new RoleDataContract
            {
                Id = role.ExternalId,
                Name = role.Name,
                Description = role.Description,
                IsBuiltIn = role.IsBuiltIn,
                RoleManagerId = role.RoleManagerId,
            };
        }

        private static RoleWithConfigurationDataContract CreateRoleWithConfigurationFrom(RoleConfiguration roleConfiguration)
        {
            var configuration = CreateFrom(roleConfiguration);

            return new RoleWithConfigurationDataContract
            {
                Id = roleConfiguration.Role.ExternalId,
                Name = roleConfiguration.Role.Name,
                Description = roleConfiguration.Role.Description,
                IsBuiltIn = roleConfiguration.Role.IsBuiltIn,
                RoleManagerId = roleConfiguration.Role.RoleManagerId,
                Configuration = configuration,
            };
        }

        private static OperationResultDataContract<RoleWithConfigurationDataContract> CreateFrom(EntityManagementResult<RoleConfiguration, RoleManagementError> roleManagementResult)
        {
            if (roleManagementResult.IsSuccessful)
            {
                var roleDataContract = CreateRoleWithConfigurationFrom(roleManagementResult.Entity);

                return roleDataContract;
            }

            var errorCodes = roleManagementResult.Errors.Select(GetErrorCode).ToList();

            return OperationResultDataContract<RoleWithConfigurationDataContract>.CreateUnsuccessful(errorCodes);
        }

        private static RoleConfigurationDataContract CreateFrom(RoleConfiguration roleConfiguration)
        {
            return new RoleConfigurationDataContract
            {
                TitleRoleIds = roleConfiguration.TitleRoles.Select(r => r.TitleRole.ExternalId).ToList(),
                IsAllTitleRoles = roleConfiguration.IsAllTitleRoles,
                UnitIds = roleConfiguration.Units.Select(u => u.UnitId).ToList(),
                IsAllUnits = roleConfiguration.IsAllUnits,
                EmployeeIds = roleConfiguration.Employees.Select(e => e.Employee.ExternalId).ToList(),
            };
        }

        private static TitleRoleDataContract CreateFrom(TitleRole titleRole)
        {
            return new TitleRoleDataContract
            {
                Id = titleRole.ExternalId,
                Name = titleRole.Name,
                HasSeniority = titleRole.HasSeniority,
            };
        }

        private async Task<OperationResult<RoleConfiguration>> CreateFromAsync(RoleConfigurationDataContract roleConfigurationDataContract, Role role)
        {
            var titleRoleIds = roleConfigurationDataContract.TitleRoleIds ?? Array.Empty<string>();
            var titleRoles = await _titleRoleService.GetTitleRolesByIdsAsync(titleRoleIds);
            if (titleRoleIds.Count != titleRoles.Count)
            {
                return OperationResult<RoleConfiguration>.CreateUnsuccessful();
            }

            var employeeIds = roleConfigurationDataContract.EmployeeIds ?? Array.Empty<string>();
            var employees = await _employeeService.GetEmployeesByIdsAsync(roleConfigurationDataContract.EmployeeIds, true);
            if (employeeIds.Count != employees.Count || employees.Any(e => e.EmploymentType == EmploymentType.Internship))
            {
                return OperationResult<RoleConfiguration>.CreateUnsuccessful();
            }

            var unitIds = roleConfigurationDataContract.UnitIds ?? Array.Empty<string>();

            return new RoleConfiguration
            {
                Role = role,
                TitleRoles = titleRoles.Select(r => new RoleConfigurationTitleRole { TitleRole = r }).ToList(),
                Units = unitIds.Select(id => new RoleConfigurationUnit { UnitId = id }).ToList(),
                Employees = employees.Select(e => new RoleConfigurationEmployee { Employee = e }).ToList(),
            };
        }

        private static bool Validate(EmployeeCurrentLocationDataContract employeeCurrentLocation)
        {
            return !String.IsNullOrEmpty(employeeCurrentLocation.LocationId) ||
                   !String.IsNullOrEmpty(employeeCurrentLocation.LocationName) &&
                   employeeCurrentLocation.LocationName.Length <= CurrentLocation.NameMaxLength;
        }

        private async Task<OperationResult<EmployeeCurrentLocation>> CreateFromAsync(EmployeeCurrentLocationDataContract employeeCurrentLocation)
        {
            CurrentLocation currentLocation;
            if (!String.IsNullOrEmpty(employeeCurrentLocation.LocationId))
            {
                currentLocation = await _currentLocationService.GetByIdAsync(employeeCurrentLocation.LocationId);
                if (currentLocation == null)
                {
                    return OperationResult<EmployeeCurrentLocation>.CreateUnsuccessful();
                }
            }
            else
            {
                var personId = Context.User.GetPersonId();
                currentLocation = await _currentLocationService.GetOrCreateAsync(employeeCurrentLocation.LocationName, personId);
            }

            return new EmployeeCurrentLocation
            {
                Location = currentLocation,
                SinceDate = employeeCurrentLocation.SinceDate,
                UntilDate = employeeCurrentLocation.UntilDate,
            };
        }

        private static bool Validate(RelocationPlanDataContract relocationPlan)
        {
            if (String.IsNullOrEmpty(relocationPlan.LocationId))
            {
                LoggerContext.Current.LogWarning("LocationId is missing for Relocation plan {relocationPlanId}.", relocationPlan.Id);
                return false;
            }

            if (relocationPlan.EmployeeComment != null && relocationPlan.EmployeeComment.Length > RelocationPlan.CommentMaxLength)
            {
                LoggerContext.Current.LogWarning("Comment length {commentLength} is too big for Relocation plan {relocationPlanId}.", relocationPlan.EmployeeComment.Length, relocationPlan.Id);
                return false;
            }

            return true;
        }

        private bool Validate(RelocationPlan relocationPlan, RelocationPlan existingRelocationPlan, bool isSync = false)
        {
            if (existingRelocationPlan != null && relocationPlan.LocationId == existingRelocationPlan.LocationId)
            {
                return true;
            }

            var countryId = relocationPlan.Location?.CountryId;
            if (String.IsNullOrEmpty(countryId))
            {
                LoggerContext.Current.LogWarning("Location.CountryId is missing for Relocation plan {relocationPlanId}.", relocationPlan.Id);
                return false;
            }

            var isCountryFound = _countryProvider.TryGetCountry(countryId, out var country);
            if (!isCountryFound)
            {
                LoggerContext.Current.LogWarning("Country {countryId} does not exist. Relocation plan {relocationPlanId}.", countryId, relocationPlan.Id);
                return false;
            }

            if (isSync)
            {
                return true;
            }

            if (!country.SupportsRelocation)
            {
                LoggerContext.Current.LogWarning("Country {countryId} does not support relocation. Relocation plan {relocationPlanId}.", countryId, relocationPlan.Id);
                return false;
            }

            return true;
        }

        private bool Validate(IReadOnlyCollection<RelocationApproverDataContract> approvers, IReadOnlyDictionary<string, Employee> employeesMap)
        {
            if (approvers.Count != employeesMap.Keys.Count())
            {
                LoggerContext.Current.LogWarning("Employees not found.");
                return false;
            }

            var primaryApproversCount = approvers.Count(a => a.IsPrimary);
            if (primaryApproversCount > 1)
            {
                LoggerContext.Current.LogWarning("Cannot assign multiple primary approvers for a single country.");
                return false;
            }

            if (employeesMap.Values.Any(e => !e.IsActive))
            {
                LoggerContext.Current.LogWarning("Cannot assign inactive employees as approvers.");
                return false;
            }

            var approversWithoutPrimary = approvers
                .Where(a => !a.IsPrimary)
                .Select(a => a.EmployeeId)
                .ToHashSet();
            foreach (var approverId in approversWithoutPrimary)
            {
                var parentUnitsManagers = _unitProvider.GetEmployeeManagedUnits(approverId)
                    .SelectMany(u => _unitProvider.GetEmployeeParentUnitsManagers(u.Id, approverId))
                    .ToList();

                if (parentUnitsManagers.Any(id => approversWithoutPrimary.Contains(id)))
                {
                    LoggerContext.Current.LogWarning("Cannot assign multiple approvers for one units hierarchy.");
                    return false;
                }
            }

            return true;
        }

        private async Task<OperationResult<RelocationPlan>> CreateFromAsync(
            RelocationPlanDataContract relocationPlanDataContract,
            Employee employee)
        {
            if (String.IsNullOrEmpty(relocationPlanDataContract.LocationId))
            {
                return OperationResult<RelocationPlan>.CreateUnsuccessful();
            }

            var compensation = relocationPlanDataContract.Compensation == null
                ? null
                : CreateFrom(relocationPlanDataContract.Compensation);
            var location = await _currentLocationService.GetByIdAsync(relocationPlanDataContract.LocationId);
            if (location == null)
            {
                return OperationResult<RelocationPlan>.CreateUnsuccessful();
            }

            return new RelocationPlan
            {
                EmployeeId = employee.Id,
                Employee = employee,
                LocationId = location.Id,
                Location = location,
                EmployeeDate = relocationPlanDataContract.EmployeeDate,
                EmployeeComment = relocationPlanDataContract.EmployeeComment,
                IsInductionPassed = relocationPlanDataContract.IsInductionPassed,
                IsConfirmed = relocationPlanDataContract.IsConfirmed,
                IsApproved = relocationPlanDataContract.IsApproved,
                ApprovalDate = relocationPlanDataContract.ApprovalDate,
                IsEmploymentConfirmedByEmployee = relocationPlanDataContract.IsEmploymentConfirmedByEmployee,
                Compensation = compensation,
            };
        }

        private EmployeeLocationInfoDataContract CreateLocationInfoFrom(Employee employee, RelocationPlan relocationPlan)
        {
            var currentLocationDateContract = employee.CurrentLocationId.HasValue ? CreateFrom(employee.CurrentLocation) : null;
            var relocationPlanDataContract = relocationPlan != null ? CreateFrom(relocationPlan) : null;

            return new EmployeeLocationInfoDataContract
            {
                EmployeeId = employee.ExternalId,
                CurrentLocation = currentLocationDateContract,
                RelocationPlan = relocationPlanDataContract,
            };
        }

        private EmployeeLocationInfoDataContract CreateLocationInfosFrom(Employee employee, IReadOnlyDictionary<int, RelocationPlan> relocationPlansMap)
        {
            relocationPlansMap.TryGetValue(employee.Id, out var relocationPlan);

            return CreateLocationInfoFrom(employee, relocationPlan);
        }

        private RelocationPlanDataContract CreateFrom(RelocationPlan relocationPlan)
        {
            var now = _environmentInfoService.CurrentUtcDateTime;
            var location = CreateFrom(relocationPlan.Location);
            var status = CreateFrom(relocationPlan.Status);
            var compensation = relocationPlan.Compensation == null
                ? null
                : CreateFrom(relocationPlan.Compensation);

            var relocationPlanDataContract = new RelocationPlanDataContract
            {
                Id = relocationPlan.ExternalId,
                SourceId = relocationPlan.SourceId,
                EmployeeId = relocationPlan.Employee.ExternalId,
                LocationId = relocationPlan.Location.ExternalId,
                Location = location,
                EmployeeDate = relocationPlan.EmployeeDate,
                EmployeeComment = relocationPlan.EmployeeComment,
                EmployeeCommentChangeDate = relocationPlan.EmployeeCommentChangeDate,
                GmManagerId = relocationPlan.GmManager?.ExternalId,
                GmComment = relocationPlan.GmComment,
                GmCommentChangeDate = relocationPlan.GmCommentChangeDate,
                IsInductionPassed = relocationPlan.IsInductionPassed,
                IsConfirmed = relocationPlan.IsConfirmed,
                ApproverComment = relocationPlan.ApproverComment,
                ApproverCommentChangeDate = relocationPlan.ApproverCommentChangeDate,
                ApproverDate = relocationPlan.ApproverDate,
                Salary = relocationPlan.Salary,
                ApproverId = relocationPlan.Approver?.ExternalId,
                RelocationUnitId = relocationPlan.RelocationUnitId,
                IsApproved = relocationPlan.IsApproved,
                ApprovedBy = relocationPlan.ApprovedBy,
                ApprovalDate = relocationPlan.ApprovalDate,
                HrManagerId = relocationPlan.HrManager?.ExternalId,
                HrManagerComment = relocationPlan.HrManagerComment,
                HrManagerCommentChangeDate = relocationPlan.HrManagerCommentChangeDate,
                HrManagerDate = relocationPlan.HrManagerDate,
                InductionStatusChangedBy = relocationPlan.InductionStatusChangedBy,
                InductionStatusChangeDate = relocationPlan.InductionStatusChangeDate,
                State = relocationPlan.State,
                IsEmploymentConfirmedByEmployee = relocationPlan.IsEmploymentConfirmedByEmployee,
                CurrentLocationStatus = CreateCurrentLocationStatusFrom(relocationPlan),
                StatusId = status.Id,
                Status = status,
                Steps = CreateStepsFrom(relocationPlan, now),
                Compensation = compensation,
                StatusStartDate = relocationPlan.StatusStartDate,
                StatusDueDate = relocationPlan.StatusDueDate,
                CloseComment = relocationPlan.CloseComment,
                ClosedBy = relocationPlan.ClosedBy,
                CloseDate = relocationPlan.CloseDate,
            };

            CreateUpdateInfoMapper.CopyCreateUpdateInfo(relocationPlanDataContract, relocationPlan);

            return relocationPlanDataContract;
        }

        private static IReadOnlyCollection<RelocationPlanStepDataContract> CreateStepsFrom(RelocationPlan relocationPlan, DateTime now)
        {
            var currentStep = relocationPlan.CurrentStep;

            return relocationPlan.GetOrderedSteps()
                .Select(step =>
                {
                    var status = step switch
                    {
                        { } when relocationPlan.State == RelocationPlanState.Completed => RelocationPlanStepStatus.Completed,
                        { CompletedAt: { } } => RelocationPlanStepStatus.Completed,
                        { } when step.StepId == relocationPlan.CurrentStepId &&
                                                (relocationPlan.State == RelocationPlanState.Cancelled ||
                                                relocationPlan.State == RelocationPlanState.Rejected)
                            => RelocationPlanStepStatus.Canceled,
                        { ExpectedAt: { } } when step.ExpectedAt.Value.Date < now.Date &&
                                                 step.Order <= currentStep.Order &&
                                                 relocationPlan.Status.ExternalId != RelocationPlanStatus.BuiltIn.OnHold
                            => RelocationPlanStepStatus.Overdue,
                        { } when step.StepId == relocationPlan.CurrentStepId => RelocationPlanStepStatus.InProgress,
                        _ => RelocationPlanStepStatus.Pending,
                    };

                    var expectedAt =
                        relocationPlan.Status.ExternalId == RelocationPlanStatus.BuiltIn.OnHold
                            ? null
                            : step.ExpectedAt;

                    return new RelocationPlanStepDataContract
                    {
                        Id = step.StepId,
                        Order = step.Order,
                        CompletedAt = step.CompletedAt,
                        IsCompletionDateHidden = step.IsCompletionDateHidden,
                        ExpectedAt = expectedAt,
                        Status = status,
                    };
                })
                .ToList();
        }

        private static CurrentLocationStatus CreateCurrentLocationStatusFrom(RelocationPlan relocationPlan)
        {
            if (relocationPlan.Status.ExternalId != RelocationPlanStatus.BuiltIn.EmploymentConfirmationByEmployee)
            {
                return CurrentLocationStatus.Ok;
            }

            if (relocationPlan.Employee.CurrentLocationId == null)
            {
                return CurrentLocationStatus.CountryDoesNotMatch;
            }

            if (relocationPlan.Employee.CurrentLocation.Location.CountryId == null)
            {
                return CurrentLocationStatus.CountryDoesNotMatch;
            }

            if (relocationPlan.Employee.CurrentLocation.Location.CountryId != relocationPlan.Location.CountryId)
            {
                return CurrentLocationStatus.CountryDoesNotMatch;
            }

            if (relocationPlan.Employee.CurrentLocation.LocationId != relocationPlan.LocationId)
            {
                return CurrentLocationStatus.CityDoesNotMatch;
            }

            return CurrentLocationStatus.Ok;
        }

        private static RelocationPlanStatusDataContract CreateFrom(RelocationPlanStatus relocationPlanStatus)
        {
            return new RelocationPlanStatusDataContract
            {
                Id = relocationPlanStatus.ExternalId,
                Name = relocationPlanStatus.Name,
            };
        }

        private static RelocationPlanChangeDataContract CreateFrom(RelocationPlanChange relocationPlanChange)
        {
            var relocationPlanChangeDataContract = new RelocationPlanChangeDataContract
            {
                EmployeeId = relocationPlanChange.Employee.ExternalId,
                UpdatedBy = relocationPlanChange.UpdatedBy,
                UpdateDate = relocationPlanChange.UpdateDate,
            };

            switch (relocationPlanChange.Type)
            {
                case DomainModel.RelocationPlanChangeType.InductionPassed:
                    {
                        relocationPlanChangeDataContract.Type = DataContracts.RelocationPlanChangeType.InductionPassed;
                        relocationPlanChangeDataContract.PreviousIsInductionPassed = relocationPlanChange.PreviousIsInductionPassed;
                        relocationPlanChangeDataContract.IsInductionPassed = relocationPlanChange.NewIsInductionPassed;
                        break;
                    }
                case DomainModel.RelocationPlanChangeType.Confirmed:
                    {
                        relocationPlanChangeDataContract.Type = DataContracts.RelocationPlanChangeType.Confirmed;
                        relocationPlanChangeDataContract.PreviousIsConfirmed = relocationPlanChange.PreviousIsConfirmed;
                        relocationPlanChangeDataContract.IsConfirmed = relocationPlanChange.NewIsConfirmed;
                        break;
                    }
                case DomainModel.RelocationPlanChangeType.Status:
                    {
                        relocationPlanChangeDataContract.Type = DataContracts.RelocationPlanChangeType.Status;
                        relocationPlanChangeDataContract.PreviousStatus = relocationPlanChange.PreviousStatus == null
                            ? null
                            : CreateFrom(relocationPlanChange.PreviousStatus);
                        relocationPlanChangeDataContract.Status = CreateFrom(relocationPlanChange.NewStatus);
                        break;
                    }
                case DomainModel.RelocationPlanChangeType.Destination:
                    {
                        relocationPlanChangeDataContract.Type = DataContracts.RelocationPlanChangeType.Destination;
                        relocationPlanChangeDataContract.PreviousDestination = CreateFrom(relocationPlanChange.PreviousDestination);
                        relocationPlanChangeDataContract.Destination = CreateFrom(relocationPlanChange.NewDestination);
                        break;
                    }
                case DomainModel.RelocationPlanChangeType.Approved:
                    {
                        relocationPlanChangeDataContract.Type = DataContracts.RelocationPlanChangeType.Approved;
                        relocationPlanChangeDataContract.PreviousIsApproved = relocationPlanChange.PreviousIsApproved;
                        relocationPlanChangeDataContract.IsApproved = relocationPlanChange.NewIsApproved;
                        break;
                    }
                case DomainModel.RelocationPlanChangeType.EmploymentConfirmedByEmployee:
                    {
                        relocationPlanChangeDataContract.Type = DataContracts.RelocationPlanChangeType.EmploymentConfirmedByEmployee;
                        relocationPlanChangeDataContract.PreviousIsEmploymentConfirmedByEmployee = relocationPlanChange.PreviousIsEmploymentConfirmedByEmployee;
                        relocationPlanChangeDataContract.IsEmploymentConfirmedByEmployee = relocationPlanChange.NewIsEmploymentConfirmedByEmployee;
                        break;
                    }
            }

            return relocationPlanChangeDataContract;
        }

        private static RelocationPlanChangeDataContract CreateFrom(RelocationPlan relocationPlan, bool isCreation)
        {
            var type = isCreation
                ? DataContracts.RelocationPlanChangeType.RelocationPlanCreated
                : GetRelocationPlanChangeType(relocationPlan.State);

            return new RelocationPlanChangeDataContract
            {
                EmployeeId = relocationPlan.Employee.ExternalId,
                Type = type,
                UpdatedBy = isCreation ? relocationPlan.CreatedBy : relocationPlan.UpdatedBy,
                UpdateDate = isCreation ? relocationPlan.CreationDate : relocationPlan.CloseDate!.Value,
            };
        }

        private static DataContracts.RelocationPlanChangeType GetRelocationPlanChangeType(RelocationPlanState state)
        {
            return state switch
            {
                RelocationPlanState.Completed => DataContracts.RelocationPlanChangeType.RelocationCompleted,
                RelocationPlanState.Cancelled => DataContracts.RelocationPlanChangeType.RelocationCancelled,
                RelocationPlanState.Rejected => DataContracts.RelocationPlanChangeType.Rejected,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown state type."),
            };
        }

        private static RelocationPlanChangeDataContract CreateFrom(EmployeeOrganizationChange change)
        {
            return new RelocationPlanChangeDataContract
            {
                EmployeeId = change.Employee.ExternalId,
                Type = DataContracts.RelocationPlanChangeType.Organization,
                PreviousOrganizationId = change.PreviousOrganizationId,
                OrganizationId = change.NewOrganizationId,
                UpdatedBy = change.UpdatedBy,
                UpdateDate = change.UpdateDate,
            };
        }

        private static RelocationPlanChangeDataContract CreateFrom(EmployeeCurrentLocationChange change)
        {
            var previousLocationDataContract = change.PreviousLocation == null
                ? null
                : CreateFrom(change.PreviousLocation);

            return new RelocationPlanChangeDataContract
            {
                EmployeeId = change.Employee.ExternalId,
                Type = DataContracts.RelocationPlanChangeType.CurrentLocation,
                PreviousEmployeeLocation = previousLocationDataContract,
                EmployeeLocation = CreateFrom(change.NewLocation),
                UpdatedBy = change.UpdatedBy,
                UpdateDate = change.UpdateDate,
            };
        }

        private RelocationPlanHistoryDataContract CreateFrom(
            IReadOnlyCollection<RelocationPlanChange> changes,
            IReadOnlyCollection<EmployeeOrganizationChange> organizationChanges,
            IReadOnlyCollection<EmployeeCurrentLocationChange> locationChanges,
            RelocationPlan relocationPlan)
        {
            var organizationChangeDataContracts = organizationChanges
                .Where(c => c.UpdateDate > relocationPlan.CreationDate &&
                    (relocationPlan.CloseDate == null || c.UpdateDate < relocationPlan.CloseDate))
                .Select(CreateFrom)
                .ToList();
            var locationChangeDataContracts = locationChanges
                .Where(c => c.UpdateDate > relocationPlan.CreationDate &&
                    (relocationPlan.CloseDate == null || c.UpdateDate < relocationPlan.CloseDate))
                .Select(CreateFrom)
                .ToList();

            var relocationCreated = CreateFrom(relocationPlan, isCreation: true);
            var changeDataContracts = changes
                .Select(CreateFrom)
                .Prepend(relocationCreated)
                .Concat(organizationChangeDataContracts)
                .Concat(locationChangeDataContracts)
                .OrderBy(c => c.UpdateDate)
                .ThenBy(c => c.Type)
                .ToList();
            if (relocationPlan.State != RelocationPlanState.Active)
            {
                var relocationClosed = CreateFrom(relocationPlan, isCreation: false);
                changeDataContracts.Add(relocationClosed);
            }

            var relocationStartedFrom = locationChangeDataContracts.Any()
                ? locationChangeDataContracts.OrderBy(c => c.UpdateDate).First().PreviousEmployeeLocation
                : relocationPlan.Employee.CurrentLocation?.Location is CurrentLocation location
                    ? CreateFrom(location)
                    : null;

            return new RelocationPlanHistoryDataContract
            {
                Plan = CreateFrom(relocationPlan),
                RelocationStartedFrom = relocationStartedFrom,
                Changes = changeDataContracts,
            };
        }

        private static EmployeeCurrentLocationDataContract CreateFrom(EmployeeCurrentLocation employeeCurrentLocation)
        {
            var currentLocation = CreateFrom(employeeCurrentLocation.Location);
            var dataContract = new EmployeeCurrentLocationDataContract
            {
                EmployeeId = employeeCurrentLocation.Employee.ExternalId,
                LocationId = currentLocation.Id,
                Location = currentLocation,
                SinceDate = employeeCurrentLocation.SinceDate,
                UntilDate = employeeCurrentLocation.UntilDate,
                ChangedBy = employeeCurrentLocation.ChangedBy,
                ChangeDate = employeeCurrentLocation.ChangeDate,
            };

            return dataContract;
        }

        private static CurrentLocationDataContract CreateFrom(CurrentLocation currentLocation)
        {
            var dataContract = new CurrentLocationDataContract
            {
                Id = currentLocation.ExternalId,
                Name = currentLocation.Name,
                IsCustom = currentLocation.IsCustom,
                HasCompanyOffice = currentLocation.HasCompanyOffice,
                IsRelocationDisabled = currentLocation.IsRelocationDisabled,
                CountryId = currentLocation.CountryId,
            };
            CreateUpdateInfoMapper.CopyCreateInfo(dataContract, currentLocation);

            return dataContract;
        }

        private static RelocationApproverDataContract CreateFrom(RelocationApprover approver)
        {
            return new RelocationApproverDataContract
            {
                CountryId = approver.CountryId,
                EmployeeId = approver.Employee.ExternalId,
                IsPrimary = approver.IsPrimary,
            };
        }

        private static RelocationApprover CreateFrom(string countryId, RelocationApproverDataContract approverDataContract, Employee employee)
        {
            return new RelocationApprover
            {
                EmployeeId = employee.Id,
                Employee = employee,
                CountryId = countryId,
                IsPrimary = approverDataContract.IsPrimary,
            };
        }

        private static EmploymentRequestDataContract CreateFrom(EmploymentRequest employmentRequest)
        {
            return new EmploymentRequestDataContract
            {
                Id = employmentRequest.ExternalId,
                EmployeeId = employmentRequest.Employee?.ExternalId,
                FirstName = employmentRequest.FirstName,
                LastName = employmentRequest.LastName,
                UnitId = employmentRequest.UnitId,
                Location = employmentRequest.Location,
                CountryId = employmentRequest.CountryId,
                OrganizationId = employmentRequest.OrganizationId,
                EmploymentDate = employmentRequest.EmploymentDate,
                CreationDate = employmentRequest.CreationDate,
                UpdateDate = employmentRequest.UpdateDate,
            };
        }

        private static DismissalRequestDataContract CreateFrom(DismissalRequest dismissalRequest)
        {
            return new DismissalRequestDataContract
            {
                Id = dismissalRequest.ExternalId,
                IsActive = dismissalRequest.IsActive,
                EmployeeId = dismissalRequest.Employee.ExternalId,
                Type = dismissalRequest.Type,
                DismissalDate = dismissalRequest.DismissalDate,
                CloseDate = dismissalRequest.CloseDate,
                IsLinked = CheckIfDismissalRequestIsLinked(dismissalRequest),
                CreationDate = dismissalRequest.CreationDate,
            };
        }

        private static RelocationPlanInfoDataContract CreateInfoFrom(RelocationPlan relocationPlan)
        {
            return new RelocationPlanInfoDataContract
            {
                Id = relocationPlan.ExternalId,
                EmployeeId = relocationPlan.Employee.ExternalId,
                RelocationUnitId = relocationPlan.RelocationUnitId,
                ApproverDate = relocationPlan.ApproverDate,
                HrManagerDate = relocationPlan.HrManagerDate,
            };
        }

        private static EmployeeUnitHistoryDataContract CreateFrom(DomainModel.EmployeeUnitHistory employeeUnitHistory)
        {
            return new EmployeeUnitHistoryDataContract
            {
                Id = employeeUnitHistory.ExternalId,
                EmployeeId = employeeUnitHistory.Employee.ExternalId,
                UnitId = employeeUnitHistory.UnitId,
                StartDate = employeeUnitHistory.StartDate,
                EndDate = employeeUnitHistory.EndDate,
            };
        }

        private RelocationApproverAssignmentsProfileDataContract CreateFrom(RelocationApproverAssignmentsProfile assignmentsProfile)
        {
            var assignments = assignmentsProfile.Assignments.Select(CreateFrom).ToList();

            return new RelocationApproverAssignmentsProfileDataContract
            {
                NextApproverId = assignmentsProfile.NextApprover?.Employee.ExternalId,
                Assignments = assignments,
            };
        }

        private RelocationApproverAssignmentDataContract CreateFrom(RelocationApproverAssignment assignment)
        {
            return new RelocationApproverAssignmentDataContract
            {
                EmployeeId = assignment.RelocationPlan.Employee.ExternalId,
                ApproverId = assignment.Approver.ExternalId,
                Date = assignment.Date,
            };
        }

        private DreamTeam.Microservices.DataContracts.PaginatedItemsDataContract<T> CreatePaginatedFrom<T, TEntity>(PaginatedItems<TEntity> items, Func<TEntity, T> map)
        {
            var itemDataContracts = items.Items.Select(map).ToList();

            return new DreamTeam.Microservices.DataContracts.PaginatedItemsDataContract<T>
            {
                Count = items.Count,
                HasNext = items.HasNext,
                Items = itemDataContracts,
            };
        }

        private static string GetErrorCode(RoleManagementError error)
        {
            return error switch
            {
                RoleManagementError.RoleAlreadyExists => RoleManagementErrorCodes.RoleAlreadyExists,
                _ => throw new ArgumentOutOfRangeException(nameof(error), error, "Unknown error type."),
            };
        }

        private static string GetErrorCode(RelocationPlanManagementError error)
        {
            return error switch
            {
                RelocationPlanManagementError.RelocationPlanAlreadyExists => RelocationPlanManagementErrorCodes.RelocationPlanAlreadyExists,
                RelocationPlanManagementError.CanNotConfirmRelocationPlanInCurrentState => RelocationPlanManagementErrorCodes.CanNotConfirmRelocationPlanInCurrentState,
                RelocationPlanManagementError.CanNotApproveRelocationPlanInCurrentState => RelocationPlanManagementErrorCodes.CanNotApproveRelocationPlanInCurrentState,
                RelocationPlanManagementError.CanNotChangeRelocationPlanInductionPassedAfterConfirmation => RelocationPlanManagementErrorCodes.CanNotChangeRelocationPlanInductionPassedAfterConfirmation,
                RelocationPlanManagementError.CurrentCountryDoesNotMatchRelocationCountry => RelocationPlanManagementErrorCodes.CurrentCountryDoesNotMatchRelocationCountry,
                RelocationPlanManagementError.CanNotConfirmEmploymentByEmployeeInCurrentState => RelocationPlanManagementErrorCodes.CanNotConfirmEmploymentByEmployeeInCurrentState,
                RelocationPlanManagementError.CanNotChangeEmploymentDateInCurrentState => RelocationPlanManagementErrorCodes.CanNotChangeEmploymentDateInCurrentState,
                RelocationPlanManagementError.CanNotSetApproverInCurrentState => RelocationPlanManagementErrorCodes.CanNotSetApproverInCurrentState,
                RelocationPlanManagementError.CanNotSetInactiveApprover => RelocationPlanManagementErrorCodes.CanNotSetInactiveApprover,
                _ => throw new ArgumentOutOfRangeException(nameof(error), error, "Unknown error type."),
            };
        }

        private static string GetErrorCode(InternshipManagementError error)
        {
            return error switch
            {
                InternshipManagementError.InternshipAlreadyExists => InternshipManagementErrorCodes.InternshipAlreadyExists,
                InternshipManagementError.DomainNameIsTaken => InternshipManagementErrorCodes.DomainNameIsTaken,
                InternshipManagementError.InvalidDomainName => InternshipManagementErrorCodes.InvalidDomainName,
                InternshipManagementError.LocalNameRequired => InternshipManagementErrorCodes.LocalNameRequired,
                _ => throw new ArgumentOutOfRangeException(nameof(error), error, "Unknown error type."),
            };
        }

        private static IReadOnlyCollection<CountryRelocationStep> CreateFrom(IReadOnlyCollection<CountryRelocationStepsDataContract> stepDataContracts)
        {
            return stepDataContracts
                .SelectMany(stepsDataContract => stepsDataContract.Steps
                    .Select(step => new CountryRelocationStep
                    {
                        StepId = step.Id,
                        Order = step.Order,
                        DurationInDays = step.DurationInDays,
                        CountryId = stepsDataContract.CountryId,
                    }))
                .ToList();
        }
    }
}
