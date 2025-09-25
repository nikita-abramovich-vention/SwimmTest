using System;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.Foundation.Internships;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class ScheduleInternshipsAutoclose : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IInternshipService _internshipService;


        public string Name => "ScheduleInternshipsAutoclose";

        public DateTime CreationDate => new DateTime(2022, 04, 01, 17, 38, 21);


        public ScheduleInternshipsAutoclose(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
            IInternshipService internshipService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _internshipService = internshipService;
        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var activeInternships = await _internshipService.GetInternshipsAsync(false, uow);
                foreach (var internship in activeInternships)
                {
                    _internshipService.ScheduleCloseActiveInternship(internship);
                }
            }
        }
    }
}