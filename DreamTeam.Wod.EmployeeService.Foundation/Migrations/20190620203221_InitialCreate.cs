using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Core.ProfileService;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.Microservices;
using DreamTeam.Wod.EmployeeService.Foundation.Roles;
using DreamTeam.Wod.EmployeeService.Foundation.SeniorityManagement;
using DreamTeam.Wod.EmployeeService.Foundation.TitleRoles;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class InitialCreate : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IEmployeeService _employeeService;
        private readonly IProfileService _profileService;
        private readonly IDepartmentService _departmentService;
        private readonly ISeniorityService _seniorityService;
        private readonly IRoleService _roleService;
        private readonly ITitleRoleService _titleRoleService;
        private readonly ISmgProfileMapper _smgProfileMapper;
        private readonly IEmployeeServiceConfiguration _configuration;


        public string Name => "InitialCreate";

        public DateTime CreationDate => new DateTime(2019, 06, 20, 20, 32, 21);


        public InitialCreate(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
            IEmployeeService employeeService,
            IProfileService profileService,
            IDepartmentService departmentService,
            ISeniorityService seniorityService,
            IRoleService roleService,
            ITitleRoleService titleRoleService,
            ISmgProfileMapper smgProfileMapper,
            IEmployeeServiceConfiguration configuration)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _employeeService = employeeService;
            _profileService = profileService;
            _departmentService = departmentService;
            _seniorityService = seniorityService;
            _roleService = roleService;
            _titleRoleService = titleRoleService;
            _smgProfileMapper = smgProfileMapper;
            _configuration = configuration;
        }


        public async Task UpAsync()
        {
            var units = await _departmentService.GetUnitsAsync();
            var unitByIdMap = units.ToDictionary(u => u.Id);

            using (var uow = _unitOfWorkFactory.Create())
            {
                var roles = await _roleService.GetRolesAsync(uow);
                var roleMap = roles.ToDictionary(r => r.ExternalId);

                var titleRoles = await _titleRoleService.GetTitleRolesAsync(uow);
                var titleRoleMap = titleRoles.ToDictionary(r => r.ExternalId);

                var context = new MigrationContext(units, roleMap, titleRoleMap);

                var roleConfigurations = AddRoleConfigurations(uow, context);
                await ImportEmployeesAsync(uow, context, roleConfigurations);

                await uow.SaveChangesAsync();
            }
        }

        private async Task ImportEmployeesAsync(
            IEmployeeServiceUnitOfWork uow,
            MigrationContext context,
            IReadOnlyCollection<RoleConfiguration> roleConfigurations)
        {
            var countries = await _departmentService.GetCountriesAsync();
            var organizations = await _departmentService.GetOrganizationsAsync();
            var peopleWithSmgProfiles = await _profileService.GetPeopleWithProfilesAsync<SmgProfileDataContract>(ProfileTypes.Smg);
            var allSeniority = await _seniorityService.GetAllSeniorityAsync(uow);
            var seniorityMap = allSeniority.ToDictionary(s => s.ExternalId);
            var existingRoles = await _roleService.GetRolesAsync(uow);
            var employeeMap = new Dictionary<string, Employee>();

            foreach (var personWithSmgProfile in peopleWithSmgProfiles.OrderBy(p => CheckIfGroupManager(p) ? 0 : 1))
            {
                var smgProfile = personWithSmgProfile.Profile;
                var employee = _smgProfileMapper.CreateEmployeeFrom(personWithSmgProfile.Person.Id, smgProfile);
                var titleRoleId = SmgRankMapper.MapToTitleRoleId(smgProfile.Rank);
                if (!String.IsNullOrEmpty(titleRoleId))
                {
                    if (!context.TitleRoleMap.TryGetValue(titleRoleId, out var titleRole))
                    {
                        titleRole = new TitleRole
                        {
                            ExternalId = titleRoleId,
                            Name = smgProfile.Rank,
                        };
                        context.TitleRoleMap.Add(titleRoleId, titleRole);
                    }

                    employee.TitleRole = titleRole;
                }

                var isSeniorityRequired = employee.IsSeniorityRequired();
                if (isSeniorityRequired)
                {
                    var seniorityId = SmgRankMapper.MapToSeniorityId(smgProfile.Rank);
                    employee.Seniority = seniorityMap[seniorityId ?? Seniority.Default];
                }
                else
                {
                    employee.Seniority = null;
                }

                if (!context.UnitByImportIdMap.TryGetValue(smgProfile.UnitId, out var unit)
                    && _configuration.IgnoreInvalidEmployeesOnInitialImport)
                {
                    continue;
                }

                employee.UnitId = unit.Id;
                var countryId = smgProfile.CountryId != null
                    ? countries.Single(c => c.ImportId == smgProfile.CountryId).Id
                    : null;
                employee.CountryId = countryId;
                var organizationId = smgProfile.OrganizationId != null
                    ? organizations.Single(o => o.ImportId == smgProfile.OrganizationId).Id
                    : null;
                employee.OrganizationId = organizationId;

                if (EmployeeSpecification.EmployeeNotInternship.IsSatisfiedBy(employee))
                {
                    employee.Roles = roleConfigurations
                        .Where(c => _roleService.CheckIfEmployeeHasRole(employee, c, context.UnitByIdMap))
                        .Select(c => new EmployeeRole { RoleId = c.Role.Id, Role = c.Role })
                        .ToList();
                }
                else
                {
                    employee.Roles = [];
                }

                if (!employee.IsActive)
                {
                    employee.DeactivationReason = employee.IsDismissed ? DeactivationReason.Dismissed : null;
                }

                employeeMap.Add(smgProfile.SmgId, employee);
            }

            var employees = employeeMap.Values.ToList();
            _employeeService.CreateEmployees(employees, uow);
            foreach (var employee in employees)
            {
                await _employeeService.AddOrUpdateEmployeeProfileAsync(employee);
            }
        }

        private IReadOnlyCollection<RoleConfiguration> AddRoleConfigurations(IEmployeeServiceUnitOfWork uow, MigrationContext context)
        {
            var roleConfigurationRepository = uow.GetRepository<RoleConfiguration>();

            var hrmUnitNames = new[]
            {
                "HRD",
                "HRT U1",
                "HRT U2",
                "HRT U4",
                "HRT U5",
                "HRT U6",
                "HRT Regions",
                "HRT NonProd",
                "HRT U3",
            };
            var roleConfigurations = new[]
            {
                CreateRoleConfiguration(
                    Role.BuiltIn.BusinessDevelopmentManager,
                    context,
                    titleRoleIds: new[] { TitleRole.BuiltIn.BusinessDevelopmentManager },
                    unitNames: new[]
                    {
                        "BDU",
                        "BDU.BY",
                        "BDU.EU",
                        "BDU.UK",
                        "BDD-UK.EMG1",
                        "BDD-UK.EMG2",
                        "BDD-UK.TLG1",
                        "BDU.US",
                        "BDU.US.NY1",
                        "BDU.US.NY2",
                        "BDU.US.SV",
                    }),
                CreateRoleConfiguration(
                    Role.BuiltIn.CorporateDevelopmentManager,
                    context,
                    titleRoleIds: new[] { TitleRole.BuiltIn.CorporateDevelopmentManager },
                    new string[0]),
                CreateRoleConfiguration(
                    Role.BuiltIn.MarketingManager,
                    context,
                    titleRoleIds: new[] { TitleRole.BuiltIn.MarketingManager },
                    unitNames: new[]
                    {
                        "MU.Growth",
                    }),
                CreateRoleConfiguration(
                    Role.BuiltIn.InternsHrManager,
                    context,
                    titleRoleIds: new[] { TitleRole.BuiltIn.HrManager },
                    unitNames: new[]
                    {
                        "IMG",
                        "PMU.IMG.T1",
                    }.Concat(hrmUnitNames).ToList()),
                CreateRoleConfiguration(
                    Role.BuiltIn.HrManager,
                    context,
                    titleRoleIds: new[] { TitleRole.BuiltIn.HrManager },
                    unitNames: hrmUnitNames),
                CreateRoleConfiguration(
                    Role.BuiltIn.DeliveryManager,
                    context,
                    titleRoleIds: new[]
                    {
                        TitleRole.BuiltIn.DeliveryManager,
                    },
                    unitNames: new string[0]),
                CreateRoleConfiguration(
                    Role.BuiltIn.DomainExpert,
                    context,
                    titleRoleIds: new[]
                    {
                        TitleRole.BuiltIn.HeadOfDomainExpertise,
                    },
                    unitNames: new string[0]),
                CreateRoleConfiguration(
                    Role.BuiltIn.Chief,
                    context,
                    titleRoleIds: new[]
                    {
                        TitleRole.BuiltIn.ChiefExecutiveOfficer,
                        TitleRole.BuiltIn.ChiefInformationOfficer,
                        TitleRole.BuiltIn.ChiefOperationOfficer,
                        TitleRole.BuiltIn.ChiefTechnologyOfficer,
                        TitleRole.BuiltIn.ChiefFinancialOfficer,
                        TitleRole.BuiltIn.VicePresidentBusinessDevelopment,
                    },
                    unitNames: new string[0]),
                CreateRoleConfiguration(
                    Role.BuiltIn.GlobalMobilityManager,
                    context,
                    titleRoleIds: new string[0],
                    unitNames: new[] { "CDD.GM" }),
                CreateRoleConfiguration(
                    Role.BuiltIn.ItSupport,
                    context,
                    titleRoleIds: new[] { TitleRole.BuiltIn.TechnicalSupportEngineerIt },
                    unitNames: new string[0]),
                CreateRoleConfiguration(
                    Role.BuiltIn.Financier,
                    context,
                    new string[0],
                    new string[0]),
                CreateRoleConfiguration(
                    Role.BuiltIn.TechnicalExpert,
                    context,
                    new string[0],
                    new string[0]),
                CreateRoleConfiguration(
                    Role.BuiltIn.ProjectIterationMaintainer,
                    context,
                    new string[0],
                    new string[0]),
                CreateRoleConfiguration(
                    Role.BuiltIn.Recruiter,
                    context,
                    new string[0],
                    new string[0]),
                CreateRoleConfiguration(
                    Role.BuiltIn.WodAdmin,
                    context,
                    new string[0],
                    new string[0]),
            };
            roleConfigurationRepository.AddRange(roleConfigurations);

            return roleConfigurations;
        }

        private RoleConfiguration CreateRoleConfiguration(
            string roleId,
            MigrationContext context,
            IReadOnlyCollection<string> titleRoleIds = null,
            IReadOnlyCollection<string> unitNames = null)
        {
            var role = context.RoleMap[roleId];
            var titleRoles = titleRoleIds
                .Select(id => new RoleConfigurationTitleRole { TitleRole = context.TitleRoleMap[id] })
                .ToList();
            var units = unitNames
                .Where(name => context.UnitByNameMap.ContainsKey(name))
                .Select(name => new RoleConfigurationUnit { UnitId = context.UnitByNameMap[name].Id })
                .ToList();

            return new RoleConfiguration
            {
                Role = role,
                TitleRoles = titleRoles,
                Units = units,
                Employees = new List<RoleConfigurationEmployee>(0),
            };
        }

        private static bool CheckIfGroupManager(
            PersonWithProfileDataContract<SmgProfileDataContract> personWithSmgProfile)
        {
            return SmgRankMapper.MapToTitleRoleId(personWithSmgProfile.Profile.Rank) == TitleRole.BuiltIn.GroupManager;
        }



        private sealed class MigrationContext
        {
            public IReadOnlyDictionary<string, UnitDataContract> UnitByIdMap { get; set; }

            public IReadOnlyDictionary<string, UnitDataContract> UnitByNameMap { get; set; }

            public IReadOnlyDictionary<string, UnitDataContract> UnitByImportIdMap { get; set; }

            public IReadOnlyDictionary<string, Role> RoleMap { get; }

            public IDictionary<string, TitleRole> TitleRoleMap { get; }


            public MigrationContext(
                IReadOnlyCollection<UnitDataContract> units,
                IReadOnlyDictionary<string, Role> roleMap,
                IDictionary<string, TitleRole> titleRoleMap)
            {
                UnitByIdMap = units.ToDictionary(u => u.Id);
                UnitByNameMap = units.ToDictionary(u => u.Name);
                UnitByImportIdMap = units.ToDictionary(u => u.ImportId);
                RoleMap = roleMap;
                TitleRoleMap = titleRoleMap;
            }
        }
    }
}