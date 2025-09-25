using System;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Core.ProfileService;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.Microservices;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations;

[UsedImplicitly]
public sealed class SyncEmployeeWagePeriods : IMigration
{
    private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
    private readonly IEmployeeService _employeeService;
    private readonly IProfileService _profileService;
    private readonly ISmgProfileMapper _smgProfileMapper;


    public string Name => "SyncEmployeeWagePeriods";

    public DateTime CreationDate => new DateTime(2024, 08, 20, 12, 32, 00);


    public SyncEmployeeWagePeriods(
        IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
        IEmployeeService employeeService,
        IProfileService profileService,
        ISmgProfileMapper smgProfileMapper)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _employeeService = employeeService;
        _profileService = profileService;
        _smgProfileMapper = smgProfileMapper;
    }


    public async Task UpAsync()
    {
        using var uow = _unitOfWorkFactory.Create();
        var wageRatePeriodRepository = uow.GetRepository<WageRatePeriod>();
        var wageRatePeriods = await wageRatePeriodRepository.GetAllAsync();
        wageRatePeriodRepository.DeleteAll(wageRatePeriods);
        await uow.SaveChangesAsync();

        var employees = await _employeeService.GetEmployeesAsync(true, uow, true);
        var peopleWithSmgProfiles = await _profileService.GetPeopleWithProfilesAsync<SmgProfileDataContract>(ProfileTypes.Smg);
        var profileByPersonIdMap = peopleWithSmgProfiles.ToDictionary(p => p.Person.Id, p => p.Profile);
        foreach (var employee in employees)
        {
            if (!profileByPersonIdMap.TryGetValue(employee.PersonId, out var profile))
            {
                continue;
            }

            var employeeWithWageRatePeriods = _smgProfileMapper.CreateEmployeeFrom(employee.PersonId, profile);

            employee.WageRatePeriods = employeeWithWageRatePeriods.WageRatePeriods;
        }

        await uow.SaveChangesAsync();
    }
}
