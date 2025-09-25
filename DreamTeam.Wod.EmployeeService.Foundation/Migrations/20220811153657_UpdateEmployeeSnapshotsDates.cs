using System;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class UpdateEmployeeSnapshotsDates : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;


        public string Name => "UpdateEmployeeSnapshotsDates";

        public DateTime CreationDate => new DateTime(2022, 08, 11, 15, 36, 57);


        public UpdateEmployeeSnapshotsDates(IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var employeeSnapshotRepository = uow.GetRepository<EmployeeSnapshot>();
                var employeeSnapshots = await employeeSnapshotRepository.GetAllAsync();

                foreach (var employeeSnapshot in employeeSnapshots)
                {
                    var snapshotCreationDate = employeeSnapshot.FromDate;

                    employeeSnapshot.FromDate = snapshotCreationDate.StartOfMonth();
                    employeeSnapshot.ToDate = snapshotCreationDate.EndOfMonth();
                }

                await uow.SaveChangesAsync();
            }
        }
    }
}