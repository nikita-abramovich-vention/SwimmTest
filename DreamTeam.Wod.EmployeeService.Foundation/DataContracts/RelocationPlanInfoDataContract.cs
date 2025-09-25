using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RelocationPlanInfoDataContract
    {
        public string Id { get; set; }

        public string EmployeeId { get; set; }

        public string RelocationUnitId { get; set; }

        public DateOnly? ApproverDate { get; set; }

        public DateOnly? HrManagerDate { get; set; }
    }
}
