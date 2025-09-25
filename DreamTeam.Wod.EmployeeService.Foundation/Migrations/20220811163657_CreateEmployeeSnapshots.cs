using System;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class CreateEmployeeSnapshots : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IEnvironmentInfoService _environmentInfoService;


        public string Name => "CreateEmployeeSnapshots";

        public DateTime CreationDate => new DateTime(2022, 08, 11, 16, 36, 57);


        public CreateEmployeeSnapshots(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
            IEnvironmentInfoService environmentInfoService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _environmentInfoService = environmentInfoService;
        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var employeeSnapshotRepository = uow.GetRepository<EmployeeSnapshot>();

                var hasAnyEmployeeSnapshots = await employeeSnapshotRepository.AnyAsync(s => true);

                if (!hasAnyEmployeeSnapshots)
                {
                    var currentDate = _environmentInfoService.CurrentUtcDate;
                    var yesterdayDate = currentDate.AddDays(-1);
                    var fromDate = currentDate.StartOfMonth() == currentDate.GetFirstDayOfQuarter()
                        ? currentDate.AddMonths(-1).GetFirstDayOfQuarter()
                        : currentDate.GetFirstDayOfQuarter();

                    var employeeRepository = uow.GetRepository<Employee>();
                    var employees = await employeeRepository.GetAllAsync();

                    var newSnapshots = employees.Select(e => new EmployeeSnapshot
                    {
                        EmployeeId = e.Id,
                        FromDate = fromDate,
                        ToDate = yesterdayDate,
                        IsActive = e.IsActive,
                        SeniorityId = e.SeniorityId,
                        TitleRoleId = e.TitleRoleId,
                        CountryId = e.CountryId,
                        OrganizationId = e.OrganizationId,
                        UnitId = e.UnitId,
                        EmploymentType = e.EmploymentType,
                    }).ToList();

                    employeeSnapshotRepository.AddRange(newSnapshots);
                }

                await uow.SaveChangesAsync();
            }
        }
    }
}