using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts;

public sealed class RelocationApproverAssignmentDataContract
{
    public string EmployeeId { get; set; }

    public string ApproverId { get; set; }

    public DateTime Date { get; set; }
}