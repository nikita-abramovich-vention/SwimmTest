using System.Collections.Generic;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RelocationPlanHistoryDataContract
    {
        public RelocationPlanDataContract Plan { get; set; }

        public CurrentLocationDataContract RelocationStartedFrom { get; set; }

        public IReadOnlyCollection<RelocationPlanChangeDataContract> Changes { get; set; }
    }
}
