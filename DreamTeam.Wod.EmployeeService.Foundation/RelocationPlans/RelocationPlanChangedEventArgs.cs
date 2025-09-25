using System;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans
{
    public sealed class RelocationPlanChangedEventArgs : EventArgs
    {
        public RelocationPlan RelocationPlan { get; }

        public RelocationPlan PreviousRelocationPlan { get; }


        public RelocationPlanChangedEventArgs(RelocationPlan relocationPlan, RelocationPlan previousRelocationPlan = null)
        {
            RelocationPlan = relocationPlan;
            PreviousRelocationPlan = previousRelocationPlan;
        }
    }
}