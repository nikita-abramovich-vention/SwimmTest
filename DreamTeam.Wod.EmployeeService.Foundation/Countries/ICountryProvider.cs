using System.Threading.Tasks;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;

namespace DreamTeam.Wod.EmployeeService.Foundation.Countries
{
    public interface ICountryProvider
    {
        Task InitializeAsync(IWodObservable wodObservable);

        bool TryGetCountry(string countryId, out CountryDataContract country);

        CountryDataContract GetCountryByImportId(string importId);
    }
}