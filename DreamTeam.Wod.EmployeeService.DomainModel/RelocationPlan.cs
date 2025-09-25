using System;
using System.Collections.Generic;
using System.Linq;
using DreamTeam.DomainModel;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RelocationPlan : IHasCreateUpdateInfo
    {
        public const int CommentMaxLength = 5000;
        public const int SalaryMaxLength = 1000;


        public int Id { get; set; }

        public string ExternalId { get; set; }

        public string SourceId { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public int LocationId { get; set; }

        public CurrentLocation Location { get; set; }

        public DateOnly EmployeeDate { get; set; }

        public string EmployeeComment { get; set; }

        public DateTime? EmployeeCommentChangeDate { get; set; }

        public int? GmManagerId { get; set; }

        public Employee GmManager { get; set; }

        public string GmComment { get; set; }

        public DateTime? GmCommentChangeDate { get; set; }

        public bool IsInductionPassed { get; set; }

        public bool IsConfirmed { get; set; }

        public DateTime? ConfirmationDate { get; set; }

        public string ApproverComment { get; set; }

        public DateTime? ApproverCommentChangeDate { get; set; }

        public DateOnly? ApproverDate { get; set; }

        public string Salary { get; set; }

        public int? ApproverId { get; set; }

        public Employee Approver { get; set; }

        public string RelocationUnitId { get; set; }

        public bool IsApproved { get; set; }

        public string ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public int? HrManagerId { get; set; }

        public Employee HrManager { get; set; }

        public string HrManagerComment { get; set; }

        public DateTime? HrManagerCommentChangeDate { get; set; }

        public DateOnly? HrManagerDate { get; set; }

        public string InductionStatusChangedBy { get; set; }

        public DateTime? InductionStatusChangeDate { get; set; }

        public bool IsEmploymentConfirmedByEmployee { get; set; }

        public RelocationPlanState State { get; set; }

        public RelocationStepId CurrentStepId { get; set; }

        public RelocationPlanStep CurrentStep => Steps.Single(s => s.StepId == CurrentStepId);

        public ICollection<RelocationPlanStep> Steps { get; set; }

        public int StatusId { get; set; }

        public RelocationPlanStatus Status { get; set; }

        public CompensationInfo Compensation { get; set; }

        public DateTime StatusStartDate { get; set; }

        public DateTime? StatusDueDate { get; set; }

        public string CloseComment { get; set; }

        public string ClosedBy { get; set; }

        public DateTime? CloseDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }


        public IReadOnlyCollection<RelocationPlanStep> GetOrderedSteps() => Steps.OrderBy(s => s.Order).ToList();

        public RelocationPlanStep GetStep(RelocationStepId id)
        {
            return Steps.SingleOrDefault(s => s.StepId == id);
        }

        public void InitSteps(IReadOnlyCollection<CountryRelocationStep> countrySteps)
        {
            Steps = countrySteps
                .Select(s => new RelocationPlanStep
                {
                    RelocationPlan = this,
                    StepId = s.StepId,
                    Order = s.Order,
                    DurationInDays = s.DurationInDays,
                })
                .ToList();
        }

        public void SetStep(RelocationStepId id, IReadOnlyCollection<CountryRelocationStep> countrySteps, DateTime now)
        {
            var newStep = GetStep(id);
            if (newStep == null)
            {
                return;
            }

            CurrentStepId = id;

            FillCompletedAt(now);
            UpdateStepExpectedDates(countrySteps, now);
        }

        public void UpdateStepExpectedDates(IReadOnlyCollection<CountryRelocationStep> countrySteps, DateTime now)
        {
            var untilStep = CurrentStep;
            var futureSteps = GetOrderedSteps().SkipWhile(s => s.Order < untilStep.Order);
            var lastStepExpectedDate = now;
            var defaultCountrySteps = CountryRelocationStep.GetDefaultSteps(Location.CountryId);
            foreach (var step in futureSteps)
            {
                var countryStep =
                    countrySteps.SingleOrDefault(s => s.StepId == step.StepId) ??
                    defaultCountrySteps.Single(s => s.StepId == step.StepId);
                step.DurationInDays = countryStep.DurationInDays;
                step.ExpectedAt = lastStepExpectedDate;
                if (step.DurationInDays != null)
                {
                    lastStepExpectedDate = lastStepExpectedDate.AddDays(step.DurationInDays.Value);
                    step.ExpectedAt = lastStepExpectedDate;
                }
            }
        }

        public RelocationStepId MatchStep(
            RelocationCaseProgress caseProgress,
            RelocationPlanStatus caseStatus)
        {
            var matchers = new Dictionary<RelocationStepId, Func<bool>>
            {
                { RelocationStepId.Induction, () => !IsInductionPassed },
                { RelocationStepId.RelocationConfirmation, () => IsInductionPassed && !IsConfirmed },
                { RelocationStepId.PendingApproval, () => IsConfirmed },
                { RelocationStepId.ProcessingQueue, () => IsApproved },
                { RelocationStepId.VisaDocsPreparation, () => caseProgress.VisaProgress.AreDocsGathered },
                { RelocationStepId.WaitingEmbassyAppointment, () => caseProgress.VisaProgress.AreDocsSentToAgency },
                { RelocationStepId.EmbassyAppointment, () => caseProgress.VisaProgress.IsScheduled },
                { RelocationStepId.VisaInProgress, () => caseProgress.VisaProgress.IsAttended },
                { RelocationStepId.VisaDone, () => caseProgress.IsVisaGathered },
                { RelocationStepId.TrpDocsPreparation, () => caseProgress.TrpState == RelocationPlanTrpState.DocsPreparation },
                { RelocationStepId.TrpDocsTranslationAndLegalization, () => caseProgress.TrpState == RelocationPlanTrpState.DocsTranslationAndLegalization },
                { RelocationStepId.TrpDocsSubmissionToMigrationDirectorate, () => caseProgress.TrpState == RelocationPlanTrpState.SubmissionToMigrationDirectorate },
                { RelocationStepId.TrpApplicationSubmission, () => caseProgress.TrpState == RelocationPlanTrpState.ApplicationSubmission },
                { RelocationStepId.TrpInProgress, () => caseProgress.TrpState == RelocationPlanTrpState.InProgress },
                { RelocationStepId.TrpIdCardDocsInProgress, () => caseProgress.TrpState == RelocationPlanTrpState.IdCardDocsInProgress },
                { RelocationStepId.EmploymentConfirmation, () => caseStatus.ExternalId == RelocationPlanStatus.BuiltIn.EmploymentConfirmationByEmployee },
                { RelocationStepId.EmploymentInProgress, () => IsEmploymentConfirmedByEmployee },
            };

            var reverseSteps = Steps.OrderByDescending(s => s.Order);
            var matchedStep = reverseSteps.First(s => matchers[s.StepId]());

            return matchedStep.StepId;
        }

        public RelocationPlan Clone()
        {
            return (RelocationPlan)MemberwiseClone();
        }


        private void FillCompletedAt(DateTime now)
        {
            var untilStep = CurrentStep;
            var stepsToComplete = GetOrderedSteps();
            foreach (var relocationPlanStep in stepsToComplete)
            {
                if (relocationPlanStep.Order < untilStep.Order)
                {
                    relocationPlanStep.CompletedAt ??= now;
                }
                else
                {
                    relocationPlanStep.CompletedAt = null;
                }
            }
        }
    }
}