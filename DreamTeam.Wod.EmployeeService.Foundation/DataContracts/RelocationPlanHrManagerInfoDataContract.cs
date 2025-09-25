using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RelocationPlanHrManagerInfoDataContract
    {
        public string HrManagerComment { get; set; }

        public DateOnly? HrManagerDate { get; set; }
    }
}