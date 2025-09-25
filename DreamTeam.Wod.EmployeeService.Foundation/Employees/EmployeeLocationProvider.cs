using System;
using System.Linq;
using DreamTeam.Common;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Offices;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    [UsedImplicitly]
    public sealed class EmployeeLocationProvider : IEmployeeLocationProvider
    {
        private readonly IOfficeProvider _officeProvider;


        public EmployeeLocationProvider(IOfficeProvider officeProvider)
        {
            _officeProvider = officeProvider;
        }


        public string GetLocation(Employee employee)
        {
            CityDataContract city = null;
            if (employee.EmploymentOfficeId != null)
            {
                var office = _officeProvider.GetOffice(employee.EmploymentOfficeId);
                city = office?.City;
            }

            var workplace = employee.Workplaces.FirstOrDefault();
            if (workplace == null)
            {
                return city == null ? String.Empty : $"{city.Name} | - | Remote";
            }

            return workplace.Workplace.Name;
        }
    }
}