using System;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class AddHasCompanyOffice : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IDepartmentService _departmentService;
        private readonly IEnvironmentInfoService _environmentInfoService;


        public string Name => "AddHasCompanyOffice";

        public DateTime CreationDate => new DateTime(2022, 05, 18, 13, 50, 54);


        public AddHasCompanyOffice(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
            IDepartmentService departmentService,
            IEnvironmentInfoService environmentInfoService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _departmentService = departmentService;
            _environmentInfoService = environmentInfoService;
        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var cities = await _departmentService.GetCitiesAsync();
                var currentLocationRepository = uow.GetRepository<CurrentLocation>();
                var currentLocations = await currentLocationRepository.GetAllAsync();
                foreach (var city in cities)
                {
                    var currentLocation = currentLocations.SingleOrDefault(l => l.Name == city.Name);
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
                }

                await uow.SaveChangesAsync();
            }
        }
    }
}