using System;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Core.DepartmentService.Units;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees;

public sealed class DisplayManagerProvider : IDisplayManagerProvider
{
    private readonly IEmployeeService _employeeService;
    private readonly IUnitProvider _unitProvider;
    private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _uowFactory;

    private string _defaultDisplayManagerExternalId;


    public DisplayManagerProvider(
        IEmployeeService employeeService,
        IUnitProvider unitProvider,
        IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> uowFactory)
    {
        _employeeService = employeeService;
        _unitProvider = unitProvider;
        _uowFactory = uowFactory;
    }

    public async Task InitializeAsync()
    {
        using var uow = _uowFactory.Create();
        var units = _unitProvider.GetUnitMap();
        _defaultDisplayManagerExternalId = await GetDefaultManagerIdAsync(uow);
    }

    public string GetDisplayManagerId(Employee employee)
    {
        return GetDisplayManagerId(employee.ExternalId, employee.UnitId);
    }

    private async Task<string> GetDefaultManagerIdAsync(IEmployeeServiceUnitOfWork uow)
    {
        var chiefExecutiveOfficer = TitleRole.BuiltIn.ChiefExecutiveOfficer;
        var chiefEmployees = await _employeeService.GetEmployeesByTitleRoleIdsAsync([chiefExecutiveOfficer], uow: uow);
        var defaultManagerId = chiefEmployees.FirstOrDefault()?.ExternalId;

        return defaultManagerId;
    }

    private string GetDisplayManagerId(
        string employeeId,
        string employeeUnitId)
    {
        var unitId = employeeUnitId;

        while (unitId != null)
        {
            var unit = _unitProvider.GetUnitById(unitId);

            var isEmployeeUnitManager = unit.ManagerId == employeeId;
            if (!String.IsNullOrEmpty(unit.ManagerId) && !isEmployeeUnitManager)
            {
                return unit.ManagerId;
            }

            unitId = unit.ParentUnitId;
        }

        var displayManagerId = employeeId != _defaultDisplayManagerExternalId
            ? _defaultDisplayManagerExternalId
            : null;

        return displayManagerId;
    }
}
