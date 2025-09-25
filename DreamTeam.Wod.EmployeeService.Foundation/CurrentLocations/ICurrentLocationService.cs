using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;

namespace DreamTeam.Wod.EmployeeService.Foundation.CurrentLocations
{
    public interface ICurrentLocationService
    {
        void Initialize(IWodObservable wodObservable);

        Task<IReadOnlyCollection<CurrentLocation>> GetAsync(bool shouldIncludeCustom = false);

        Task<CurrentLocation> GetByIdAsync(string id);

        Task<CurrentLocation> GetOrCreateAsync(string name, string byPeronId, string countryId = null);

        Task<EmployeeCurrentLocation> UpdateEmployeeCurrentLocationAsync(Employee employee, [CanBeNull]EmployeeCurrentLocation currentLocation, string byPersonId);
    }
}
