using DreamTeam.Common.Observable;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Services.DataContracts;

namespace DreamTeam.Wod.EmployeeService.Foundation.WodEvents
{
    public interface IWodObservable : IAsyncObservable<ServiceEntityChanged<UnitDataContract>>
    {
        event AsyncObserver<ServiceEntityChanged<UnitDataContract>> UnitChanged;

        event AsyncObserver<CountryDataContract> CountryChanged;

        event AsyncObserver<ServiceEntityChanged<CityDataContract>> CityCreated;

        event AsyncObserver<ServiceEntityChanged<CityDataContract>> CityUpdated;

        event AsyncObserver<OrganizationDataContract> OrganizationChanged;

        event AsyncObserver<ServiceEntityChanged<OfficeDataContract>> OfficeCreated;

        event AsyncObserver<ServiceEntityChanged<OfficeDataContract>> OfficeUpdated;
    }
}