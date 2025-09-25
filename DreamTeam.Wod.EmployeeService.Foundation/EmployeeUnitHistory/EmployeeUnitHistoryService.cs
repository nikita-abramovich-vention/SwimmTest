using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.EmployeeUnitHistory;

[UsedImplicitly]
public sealed class EmployeeUnitHistoryService : IEmployeeUnitHistoryService
{
    private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _unitOfWorkProvider;


    public EmployeeUnitHistoryService(IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }


    public async Task<IReadOnlyCollection<DomainModel.EmployeeUnitHistory>> GetAllAsync()
    {
        var uow = _unitOfWorkProvider.CurrentUow;
        var repository = uow.GetRepository<DomainModel.EmployeeUnitHistory>();
        var loadStrategy = GetEmployeeUnitHistoryWithEmployeeLoadStrategy();

        var employeeUnitHistory = await repository.GetAllAsync(loadStrategy);

        return employeeUnitHistory;
    }

    public async Task<IReadOnlyCollection<DomainModel.EmployeeUnitHistory>> GetByEmployeeIdAsync(string employeeId)
    {
        var uow = _unitOfWorkProvider.CurrentUow;
        var repository = uow.GetRepository<DomainModel.EmployeeUnitHistory>();
        var specification = EmployeeUnitHistorySpecification.ByEmployeeId(employeeId);
        var loadStrategy = GetEmployeeUnitHistoryWithEmployeeLoadStrategy();
        var employeeUnitHistory = await repository.GetWhereAsync(specification, loadStrategy);

        return employeeUnitHistory;
    }


    private static IEntityLoadStrategy<DomainModel.EmployeeUnitHistory> GetEmployeeUnitHistoryWithEmployeeLoadStrategy()
    {
        return new EntityLoadStrategy<DomainModel.EmployeeUnitHistory>(h => h.Employee);
    }
}