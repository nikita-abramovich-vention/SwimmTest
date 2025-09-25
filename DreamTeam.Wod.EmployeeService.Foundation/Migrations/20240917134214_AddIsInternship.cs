using System;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Core.ProfileService;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.Microservices;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations;

[UsedImplicitly]
public sealed class AddIsInternship : IMigration
{
    private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
    private readonly IEmployeeService _employeeService;
    private readonly IProfileService _profileService;
    private readonly ISmgProfileMapper _smgProfileMapper;


    public string Name => "AddIsInternship";

    public DateTime CreationDate => new DateTime(2024, 09, 17, 13, 42, 14);


    public AddIsInternship(
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
        var employmentPeriodComparer = new EmploymentPeriodEqualityComparer();
        using var uow = _unitOfWorkFactory.Create();
        var employees = await _employeeService.GetEmployeesAsync(true, uow);
        var peopleWithSmgProfiles = await _profileService.GetPeopleWithProfilesAsync<SmgProfileDataContract>(ProfileTypes.Smg);
        foreach (var employee in employees)
        {
            var personWithSmgProfile = peopleWithSmgProfiles.FirstOrDefault(p => p.Person.Id == employee.PersonId);
            if (personWithSmgProfile != null)
            {
                var employeeWithEmploymentPeriods = _smgProfileMapper.CreateEmployeeFrom(employee.PersonId, personWithSmgProfile.Profile);
                employee.EmploymentPeriods.Reconcile(employeeWithEmploymentPeriods.EmploymentPeriods, employmentPeriodComparer);
            }
        }

        await uow.SaveChangesAsync();
    }
}