using System;
using System.Threading.Tasks;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.EmployeeUnitHistory;
using DreamTeam.Wod.EmployeeService.Repositories;
using Hangfire.Annotations;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations;

[UsedImplicitly]
public sealed class InitialEmployeeUnitHistorySync : IMigration
{
    private readonly IEmployeeUnitHistorySyncService _employeeUnitHistorySyncService;


    public string Name => "InitialEmployeeUnitHistorySync";

    public DateTime CreationDate => new(2023, 11, 15, 14, 50, 00);


    public InitialEmployeeUnitHistorySync(IEmployeeUnitHistorySyncService employeeUnitHistorySyncService)
    {
        _employeeUnitHistorySyncService = employeeUnitHistorySyncService;
    }


    public async Task UpAsync()
    {
        var isEmployeeUnitHistorySyncSuccessful = await _employeeUnitHistorySyncService.SyncAsync();
        if (!isEmployeeUnitHistorySyncSuccessful)
        {
            throw new InvalidOperationException("Failed to perform employee unit history sync.");
        }
    }
}