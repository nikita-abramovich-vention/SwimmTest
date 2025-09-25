using System.Collections.Generic;
using System.Linq;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class CountryRelocationStep
    {
        public string CountryId { get; set; }

        public RelocationStepId StepId { get; set; }

        public int? DurationInDays { get; set; }

        public int Order { get; set; }


        public static IReadOnlyCollection<CountryRelocationStep> GetDefaultSteps(string countryId)
        {
            var noVisaAndTrpSteps = new (RelocationStepId Id, int DurationInDays, int Order)[]
            {
                (RelocationStepId.Induction, 14, 1),
                (RelocationStepId.RelocationConfirmation, 30, 2),
                (RelocationStepId.PendingApproval, 15, 3),
                (RelocationStepId.ProcessingQueue, 60, 4),
                (RelocationStepId.EmploymentConfirmation, 24, 5),
                (RelocationStepId.EmploymentInProgress, 5, 6),
            };

            var defaultStepsMap = new Dictionary<string, IReadOnlyCollection<(RelocationStepId Id, int DurationInDays, int Order)>>
            {
                // Uzbekistan
                ["7"] = noVisaAndTrpSteps,
                // Georgia
                ["georgia"] = noVisaAndTrpSteps,
                // Kyrgyzstan
                ["kyrgyzstan"] = noVisaAndTrpSteps,
                // Kazakhstan
                ["kazakhstan"] = noVisaAndTrpSteps,
                // Poland
                ["4"] = new (RelocationStepId Id, int DurationInDays, int Order)[]
                {
                    (RelocationStepId.Induction, 15, 1),
                    (RelocationStepId.RelocationConfirmation, 30, 2),
                    (RelocationStepId.PendingApproval, 15, 3),
                    (RelocationStepId.ProcessingQueue, 7, 4),
                    (RelocationStepId.VisaDocsPreparation, 30, 5),
                    (RelocationStepId.WaitingEmbassyAppointment, 30, 6),
                    (RelocationStepId.EmbassyAppointment, 30, 7),
                    (RelocationStepId.VisaInProgress, 21, 8),
                    (RelocationStepId.VisaDone, 14, 9),
                    (RelocationStepId.EmploymentConfirmation, 194, 10),
                    (RelocationStepId.EmploymentInProgress, 180, 11),
                },
                // Lithuania
                ["8"] = new (RelocationStepId Id, int DurationInDays, int Order)[]
                {
                    (RelocationStepId.Induction, 14, 1),
                    (RelocationStepId.RelocationConfirmation, 30, 2),
                    (RelocationStepId.PendingApproval, 15, 3),
                    (RelocationStepId.ProcessingQueue, 14, 4),
                    (RelocationStepId.VisaDocsPreparation, 14, 5),
                    (RelocationStepId.WaitingEmbassyAppointment, 42, 6),
                    (RelocationStepId.EmbassyAppointment, 42, 7),
                    (RelocationStepId.VisaInProgress, 14, 8),
                    (RelocationStepId.VisaDone, 14, 9),
                    (RelocationStepId.TrpDocsPreparation, 14, 10),
                    (RelocationStepId.TrpApplicationSubmission, 40, 11),
                    (RelocationStepId.TrpInProgress, 70, 12),
                    (RelocationStepId.EmploymentConfirmation, 41, 13),
                    (RelocationStepId.EmploymentInProgress, 20, 14),
                },
                // Bulgaria
                ["bulgaria"] = new (RelocationStepId Id, int DurationInDays, int Order)[]
                {
                    (RelocationStepId.Induction, 14, 1),
                    (RelocationStepId.RelocationConfirmation, 30, 2),
                    (RelocationStepId.PendingApproval, 15, 3),
                    (RelocationStepId.ProcessingQueue, 7, 4),
                    (RelocationStepId.TrpDocsPreparation, 21, 5),
                    (RelocationStepId.TrpDocsTranslationAndLegalization, 14, 6),
                    (RelocationStepId.TrpDocsSubmissionToMigrationDirectorate, 30, 7),
                    (RelocationStepId.EmbassyAppointment, 20, 8),
                    (RelocationStepId.VisaInProgress, 14, 9),
                    (RelocationStepId.VisaDone, 180, 10),
                    (RelocationStepId.TrpIdCardDocsInProgress, 50, 11),
                    (RelocationStepId.EmploymentConfirmation, 1, 12),
                    (RelocationStepId.EmploymentInProgress, 5, 13),
                },
                // Slovakia
                ["13"] = new (RelocationStepId Id, int DurationInDays, int Order)[]
                {
                    (RelocationStepId.Induction, 14, 1),
                    (RelocationStepId.RelocationConfirmation, 30, 2),
                    (RelocationStepId.PendingApproval, 14, 3),
                    (RelocationStepId.ProcessingQueue, 14, 4),
                    (RelocationStepId.VisaDocsPreparation, 21, 5),
                    (RelocationStepId.WaitingEmbassyAppointment, 28, 6),
                    (RelocationStepId.EmbassyAppointment, 70, 7),
                    (RelocationStepId.VisaInProgress, 35, 8),
                    (RelocationStepId.VisaDone, 14, 9),
                    (RelocationStepId.EmploymentConfirmation, 54, 10),
                    (RelocationStepId.EmploymentInProgress, 30, 11),
                },
                // Mexico
                ["mexico"] = new (RelocationStepId Id, int DurationInDays, int Order)[]
                {
                    (RelocationStepId.Induction, 14, 1),
                    (RelocationStepId.RelocationConfirmation, 30, 2),
                    (RelocationStepId.PendingApproval, 15, 3),
                    (RelocationStepId.ProcessingQueue, 7, 4),
                    (RelocationStepId.VisaDocsPreparation, 35, 5),
                    (RelocationStepId.WaitingEmbassyAppointment, 20, 6),
                    (RelocationStepId.EmbassyAppointment, 42, 7),
                    (RelocationStepId.VisaInProgress, 14, 8),
                    (RelocationStepId.VisaDone, 14, 9),
                    (RelocationStepId.TrpDocsPreparation, 14, 10),
                    (RelocationStepId.TrpApplicationSubmission, 30, 11),
                    (RelocationStepId.TrpInProgress, 20, 12),
                    (RelocationStepId.EmploymentConfirmation, 34, 13),
                    (RelocationStepId.EmploymentInProgress, 20, 14),
                },
            };

            var unknownCountrySteps = new (RelocationStepId Id, int DurationInDays, int Order)[]
            {
                (RelocationStepId.Induction, 7 * 2, 1),
                (RelocationStepId.RelocationConfirmation, 30, 2),
                (RelocationStepId.PendingApproval, 7 * 2, 3),
                (RelocationStepId.ProcessingQueue, 7 * 2, 4),
                (RelocationStepId.EmploymentConfirmation, 7 * 2, 5),
                (RelocationStepId.EmploymentInProgress, 7 * 2, 6),
            };

            var countrySteps =
                countryId != null
                    ? defaultStepsMap.GetValueOrDefault(countryId) ?? unknownCountrySteps
                    : unknownCountrySteps;

            return countrySteps
                .Select(step =>
                {
                    var (id, durationInDays, order) = step;

                    return new CountryRelocationStep
                    {
                        CountryId = countryId,
                        StepId = id,
                        DurationInDays = durationInDays,
                        Order = order,
                    };
                })
                .ToList();
        }
    }
}