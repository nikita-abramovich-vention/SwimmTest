using System;
using System.Collections.Generic;
using DreamTeam.Microservices.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RelocationPlanDataContract : IHasCreateUpdateInfoDataContract
    {
        public string Id { get; set; }

        public string EmployeeId { get; set; }

        public string SourceId { get; set; }

        public string LocationId { get; set; }

        public CurrentLocationDataContract Location { get; set; }

        public DateOnly EmployeeDate { get; set; }

        public string EmployeeComment { get; set; }

        public DateTime? EmployeeCommentChangeDate { get; set; }

        public string GmManagerId { get; set; }

        public string GmComment { get; set; }

        public DateTime? GmCommentChangeDate { get; set; }

        public bool IsInductionPassed { get; set; }

        public bool IsConfirmed { get; set; }

        public string ApproverComment { get; set; }

        public DateTime? ApproverCommentChangeDate { get; set; }

        public DateOnly? ApproverDate { get; set; }

        public string Salary { get; set; }

        public string ApproverId { get; set; }

        public string RelocationUnitId { get; set; }

        public bool IsApproved { get; set; }

        public string ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public string HrManagerId { get; set; }

        public string HrManagerComment { get; set; }

        public DateTime? HrManagerCommentChangeDate { get; set; }

        public DateOnly? HrManagerDate { get; set; }

        public string InductionStatusChangedBy { get; set; }

        public DateTime? InductionStatusChangeDate { get; set; }

        public RelocationPlanState State { get; set; }

        public bool IsEmploymentConfirmedByEmployee { get; set; }

        public CurrentLocationStatus CurrentLocationStatus { get; set; }

        public RelocationCaseProgressDataContract RelocationCaseProgress { get; set; }

        public string StatusId { get; set; }

        public RelocationPlanStatusDataContract Status { get; set; }

        public IReadOnlyCollection<RelocationPlanStepDataContract> Steps { get; set; }

        public CompensationDataContract Compensation { get; set; }

        public DateTime StatusStartDate { get; set; }

        public DateTime? StatusDueDate { get; set; }

        public string CloseComment { get; set; }

        public string ClosedBy { get; set; }

        public DateTime? CloseDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}