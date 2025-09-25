using System.Threading.Tasks;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;

namespace DreamTeam.Wod.EmployeeService.Foundation.Offices
{
    public interface IOfficeProvider
    {
        Task InitializeAsync(IWodObservable wodObservable);

        OfficeDataContract GetOffice(string officeId);

        OfficeDataContract GetOfficeByImportId(string importId);
    }
}