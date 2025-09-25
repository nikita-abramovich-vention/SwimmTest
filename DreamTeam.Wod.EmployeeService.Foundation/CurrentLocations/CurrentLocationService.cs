using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Repositories;
using DreamTeam.Services.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.CurrentLocations
{
    [UsedImplicitly]
    public sealed class CurrentLocationService : ICurrentLocationService
    {
        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;
        private readonly IEnvironmentInfoService _environmentInfoService;


        public CurrentLocationService(
            IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider,
            IEnvironmentInfoService environmentInfoService)
        {
            _uowProvider = uowProvider;
            _environmentInfoService = environmentInfoService;
        }


        public void Initialize(IWodObservable wodObservable)
        {
            wodObservable.CityCreated += OnCityCreatedAsync;
        }


        public async Task<IReadOnlyCollection<CurrentLocation>> GetAsync(bool shouldIncludeCustom = false)
        {
            var uow = _uowProvider.CurrentUow;
            var currentLocationRepository = uow.GetRepository<CurrentLocation>();
            var currentLocations = await currentLocationRepository.GetWhereAsync(l => shouldIncludeCustom || !l.IsCustom);

            return currentLocations;
        }

        public async Task<CurrentLocation> GetByIdAsync(string id)
        {
            var uow = _uowProvider.CurrentUow;
            var currentLocationRepository = uow.GetRepository<CurrentLocation>();
            var currentLocation = await currentLocationRepository.GetSingleOrDefaultAsync(l => l.ExternalId == id);

            return currentLocation;
        }

        public async Task<CurrentLocation> GetOrCreateAsync(string name, string byPeronId, string countryId = null)
        {
            var uow = _uowProvider.CurrentUow;
            var currentLocationRepository = uow.GetRepository<CurrentLocation>();
            var currentLocation = await currentLocationRepository.GetSingleOrDefaultAsync(l => l.Name == name);
            if (currentLocation == null)
            {
                currentLocation = new CurrentLocation
                {
                    ExternalId = Guid.NewGuid().ToString("N"),
                    Name = name,
                    IsCustom = true,
                    CountryId = countryId,
                    CreatedBy = byPeronId,
                    CreationDate = _environmentInfoService.CurrentUtcDateTime,
                };
                currentLocationRepository.Add(currentLocation);
            }

            await uow.SaveChangesAsync();

            return currentLocation;
        }

        public async Task<EmployeeCurrentLocation> UpdateEmployeeCurrentLocationAsync(Employee employee, EmployeeCurrentLocation fromCurrentLocation, string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;
            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            employee.UpdatedBy = byPersonId;
            employee.UpdateDate = currentDate;
            var previousLocationId = employee.CurrentLocation?.Location.Id;
            if (fromCurrentLocation != null)
            {
                var currentLocation = employee.CurrentLocation ?? new EmployeeCurrentLocation
                {
                    EmployeeId = employee.Id,
                };

                currentLocation.Location = fromCurrentLocation.Location;
                currentLocation.ChangedBy = employee.UpdatedBy;
                currentLocation.ChangeDate = currentDate;
                currentLocation.SinceDate = fromCurrentLocation.SinceDate;
                currentLocation.UntilDate = fromCurrentLocation.UntilDate;
                employee.CurrentLocation = currentLocation;
            }
            else
            {
                employee.CurrentLocation = null;
            }

            var newLocationId = fromCurrentLocation?.Location.Id;
            if (previousLocationId != newLocationId)
            {
                var locationChangeRepository = uow.GetRepository<EmployeeCurrentLocationChange>();
                var locationChange = new EmployeeCurrentLocationChange
                {
                    EmployeeId = employee.Id,
                    PreviousLocationId = previousLocationId,
                    NewLocationId = newLocationId,
                    UpdatedBy = byPersonId,
                    UpdateDate = currentDate,
                };
                locationChangeRepository.Add(locationChange);
            }

            await uow.SaveChangesAsync();

            return employee.CurrentLocation;
        }


        private async Task OnCityCreatedAsync(ServiceEntityChanged<CityDataContract> cityChanged)
        {
            var city = cityChanged.NewValue;
            var uow = _uowProvider.CurrentUow;
            var currentLocationRepository = uow.GetRepository<CurrentLocation>();
            var currentLocation = await currentLocationRepository.GetSingleOrDefaultAsync(l => l.Name == city.Name);
            if (currentLocation == null)
            {
                currentLocation = new CurrentLocation
                {
                    ExternalId = Guid.NewGuid().ToString("N"),
                    Name = city.Name,
                    HasCompanyOffice = true,
                    CreationDate = _environmentInfoService.CurrentUtcDateTime,
                    CountryId = city.CountryId,
                };
                currentLocationRepository.Add(currentLocation);
            }
            else
            {
                currentLocation.HasCompanyOffice = true;
                currentLocation.IsCustom = false;
            }

            await uow.SaveChangesAsync();
        }
    }
}