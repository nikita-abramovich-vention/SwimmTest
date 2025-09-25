using System;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts;

public sealed class RelocationPlanStepDataContract
{
    public RelocationStepId Id { get; set; }

    public RelocationPlanStepStatus Status { get; set; }

    public int Order { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsCompletionDateHidden { get; set; }

    public DateTime? ExpectedAt { get; set; }
}