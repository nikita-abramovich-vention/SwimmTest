using System.Collections.Generic;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.RelocationApprovers;

public sealed class RelocationApproverAssignmentsProfile
{
    public RelocationApprover NextApprover { get; set; }

    public IReadOnlyCollection<RelocationApproverAssignment> Assignments { get; set; }
}