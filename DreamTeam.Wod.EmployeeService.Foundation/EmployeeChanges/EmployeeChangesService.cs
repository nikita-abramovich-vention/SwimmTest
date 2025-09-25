using System.Threading.Tasks;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.EmployeeChanges;

public sealed class EmployeeChangesService : IEmployeeChangesService
{
    private readonly IEnvironmentInfoService _environmentInfoService;


    public EmployeeChangesService(IEnvironmentInfoService environmentInfoService)
    {
        _environmentInfoService = environmentInfoService;
    }


    public Task HandleEmployeeUpdateAsync(Employee previousEmployee, Employee employee, IEmployeeServiceUnitOfWork uow)
    {
        if (employee.OrganizationId != previousEmployee.OrganizationId)
        {
            AddEmployeeOrganizationChange(previousEmployee, employee, uow);
        }

        return Task.CompletedTask;
    }


    private void AddEmployeeOrganizationChange(Employee previousEmployee, Employee employee, IUnitOfWork uow)
    {
        var updateDate = _environmentInfoService.CurrentUtcDateTime;

        var organizationChange = new EmployeeOrganizationChange
        {
            Employee = employee,
            PreviousOrganizationId = previousEmployee.OrganizationId,
            NewOrganizationId = employee.OrganizationId,
            UpdateDate = updateDate,
            UpdatedBy = employee.UpdatedBy,
        };

        var organizationRepository = uow.GetRepository<EmployeeOrganizationChange>();
        organizationRepository.Add(organizationChange);
    }
}