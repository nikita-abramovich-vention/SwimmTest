using System;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class AddDefaultCurrentLocations : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IDepartmentService _departmentService;


        public string Name => "AddDefaultCurrentLocations";

        public DateTime CreationDate => new DateTime(2022, 04, 23, 12, 34, 05);


        public AddDefaultCurrentLocations(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
            IDepartmentService departmentService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _departmentService = departmentService;
        }


        public async Task UpAsync()
        {
            var countries = await _departmentService.GetCountriesAsync();
            var countryNameToIdMap = countries.ToDictionary(c => c.Name, c => c.Id);

            var currentLocationsCountries = new (string CountryName, string LocationName)[]
            {
                ("Belarus", "Minsk"),
                ("Belarus", "Mogilev"),
                ("Belarus", "Vitebsk"),
                ("Belarus", "Brest"),
                ("Belarus", "Gomel"),
                ("Belarus", "Grodno"),
                ("Belarus", "Polotsk"),
                ("US", "New York"),
                ("US", "San Francisco"),
                ("US", "Marietta"),
                ("US", "Iselin"),
                ("UK", "London"),
                ("Poland", "Lodz"),
                ("Poland", "Wroclaw"),
                ("Poland", "Warsaw"),
                ("Poland", "Krakow"),
                ("Poland", "Gdansk"),
                ("Austria", "Vienna"),
                ("Ukraine", "Kiev"),
                ("Ukraine", "Lviv"),
                ("Uzbekistan", "Tashkent"),
                ("Lithuania", "Vilnius"),
                ("Georgia", "Tbilisi"),
                ("Georgia", "Batumi"),
                ("Kazakhstan", "Nur-Sultan"),
            };

            var defaultCurrentLocations = currentLocationsCountries
                .Select(countryLocation => new CurrentLocation
                {
                    ExternalId = countryLocation.LocationName.ToLowerInvariant(),
                    Name = countryLocation.LocationName,
                    CreationDate = new DateTime(2022, 3, 1),
                    CountryId = countryNameToIdMap[countryLocation.CountryName],
                }).ToList();

            using (var uow = _unitOfWorkFactory.Create())
            {
                var currentLocationRepository = uow.GetRepository<CurrentLocation>();
                var currentLocations = await currentLocationRepository.GetAllAsync();

                foreach (var defaultCurrentLocation in defaultCurrentLocations)
                {
                    var existingCurrentLocation = currentLocations.SingleOrDefault(l => l.Name == defaultCurrentLocation.Name);
                    if (existingCurrentLocation != null)
                    {
                        existingCurrentLocation.CountryId = defaultCurrentLocation.CountryId;
                    }
                    else
                    {
                        currentLocationRepository.Add(defaultCurrentLocation);
                    }
                }

                await uow.SaveChangesAsync();
            }
        }
    }
}