using System.Collections.Generic;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts;

public sealed class RelocationApproverAssignmentsProfileDataContract
{
    public string NextApproverId { get; set; }

    public IReadOnlyCollection<RelocationApproverAssignmentDataContract> Assignments { get; set; }
}