using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees;

[UsedImplicitly]
public sealed class WageRatePeriodService : IWageRatePeriodService
{
    private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _unitOfWorkProvider;


    public WageRatePeriodService(IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }


    public async Task<IReadOnlyCollection<double>> GetAllWageRatesAsync()
    {
        var uow = _unitOfWorkProvider.CurrentUow;
        var repository = uow.WageRatePeriods;
        var wageRates = await repository.GetAllWageRatesAsync();

        return wageRates;
    }
}