using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RelocationPlanStatus
    {
        public const int NameMaxLength = 100;


        public int Id { get; set; }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public int? CaseStatusId { get; set; }

        public RelocationCaseStatus CaseStatus { get; set; }


        public static (string StatusId, RelocationStepId? FromStepId) From(RelocationPlanState state, RelocationPlanStatus status, RelocationStepId stepId)
        {
            return state switch
            {
                RelocationPlanState.Rejected => (BuiltIn.Rejected, null),
                RelocationPlanState.Cancelled => (BuiltIn.Canceled, null),
                RelocationPlanState.Completed => (BuiltIn.Completed, null),
                RelocationPlanState.Active when status.ExternalId == BuiltIn.OnHold => (status.ExternalId, null),
                RelocationPlanState.Active => (FromStepId(stepId), stepId),
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
            };
        }

        private static string FromStepId(RelocationStepId stepId)
        {
            return stepId switch
            {
                RelocationStepId.Induction => BuiltIn.Induction,
                RelocationStepId.RelocationConfirmation => BuiltIn.EmployeeConfirmation,
                RelocationStepId.PendingApproval => BuiltIn.PendingApproval,
                RelocationStepId.ProcessingQueue => BuiltIn.InProgress,
                RelocationStepId.VisaDocsPreparation => BuiltIn.VisaDocsPreparation,
                RelocationStepId.WaitingEmbassyAppointment => BuiltIn.WaitingEmbassyAppointment,
                RelocationStepId.EmbassyAppointment => BuiltIn.EmbassyAppointment,
                RelocationStepId.VisaInProgress => BuiltIn.VisaInProgress,
                RelocationStepId.VisaDone => BuiltIn.VisaDone,
                RelocationStepId.TrpDocsPreparation => BuiltIn.TrpDocsPreparation,
                RelocationStepId.TrpDocsTranslationAndLegalization => BuiltIn.TrpDocsTranslationAndLegalization,
                RelocationStepId.TrpDocsSubmissionToMigrationDirectorate => BuiltIn.TrpDocsSubmissionToMigrationDirectorate,
                RelocationStepId.TrpApplicationSubmission => BuiltIn.TrpApplicationSubmission,
                RelocationStepId.TrpInProgress => BuiltIn.TrpInProgress,
                RelocationStepId.TrpIdCardDocsInProgress => BuiltIn.TrpIdCardDocsInProgress,
                RelocationStepId.EmploymentConfirmation => BuiltIn.EmploymentConfirmationByEmployee,
                RelocationStepId.EmploymentInProgress => BuiltIn.ReadyForEmployment,
                _ => throw new ArgumentOutOfRangeException(nameof(stepId), stepId, null),
            };
        }


        public static class BuiltIn
        {
            public const string Induction = "induction";
            public const string EmployeeConfirmation = "employee_confirmation";
            public const string PendingApproval = "pending_approval";
            public const string RelocationApproved = "relocation_approved";
            public const string InProgress = "in_progress";
            public const string VisaDocsPreparation = "visa_docs_preparation";
            public const string WaitingEmbassyAppointment = "waiting_for_embassy_appointment";
            public const string EmbassyAppointment = "embassy_appointment";
            public const string VisaInProgress = "visa_in_progress";
            public const string VisaDone = "visa_done";
            public const string TrpDocsPreparation = "trp_docs_preparation";
            public const string TrpDocsTranslationAndLegalization = "trp_docs_translation_and_legalization";
            public const string TrpDocsSubmissionToMigrationDirectorate = "trp_docs_submission_to_migration_directorate";
            public const string TrpApplicationSubmission = "trp_application_submission";
            public const string TrpInProgress = "trp_in_progress";
            public const string TrpIdCardDocsInProgress = "trp_id_card_docs_in_progress";
            public const string EmploymentConfirmationByEmployee = "employment_confirmation_by_employee";
            public const string ReadyForEmployment = "ready_for_employment";
            public const string Rejected = "rejected";
            public const string Canceled = "canceled";
            public const string Completed = "completed";
            public const string OnHold = "on_hold";
        }
    }
}
