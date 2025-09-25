using System.Threading.Tasks;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;

namespace DreamTeam.Wod.EmployeeService.Foundation.Organizations
{
    public interface IOrganizationProvider
    {
        Task InitializeAsync(IWodObservable wodObservable);

        OrganizationDataContract GetOrganizationByImportId(string importId);
    }
}