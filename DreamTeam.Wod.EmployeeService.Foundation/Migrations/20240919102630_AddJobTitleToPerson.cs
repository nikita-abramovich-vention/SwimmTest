using System;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Core.ProfileService;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations;

[UsedImplicitly]
public sealed class AddJobTitleToPerson : IMigration
{
    private readonly IProfileService _profileService;
    private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _uowFactory;


    public string Name => "AddJobTitleToPerson";

    public DateTime CreationDate => new(2024, 09, 19, 10, 26, 30);


    public AddJobTitleToPerson(IProfileService profileService, IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> uowFactory)
    {
        _profileService = profileService;
        _uowFactory = uowFactory;
    }


    public async Task UpAsync()
    {
        using var uow = _uowFactory.Create();
        var employeeRepository = uow.GetRepository<Employee>();
        var employeeLoadStrategy = new EntityLoadStrategy<Employee>(e => e.TitleRole, e => e.Seniority);
        var employees = await employeeRepository.GetAllAsync(employeeLoadStrategy);
        foreach (var employee in employees)
        {
            var title = employee.GetTitle();
            var updatePersonJobTitleResult = await _profileService.UpdatePersonJobTitleAsync(employee.PersonId, title);
            if (!updatePersonJobTitleResult.IsSuccessful)
            {
                var reason = updatePersonJobTitleResult.ErrorCodes.JoinStrings();

                throw new InvalidOperationException($"Failed to update {employee.PersonId} because {reason}.");
            }
        }
    }
}
