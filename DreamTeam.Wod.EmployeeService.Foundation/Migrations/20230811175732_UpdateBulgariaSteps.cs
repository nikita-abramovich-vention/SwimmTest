using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Reflection;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Core.ProfileService;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityFramework.Implementations;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.Offices;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class UpdateBulgariaSteps : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IEnvironmentInfoService _environmentInfoService;


        public string Name => "UpdateBulgariaSteps";

        public DateTime CreationDate => new DateTime(2023, 08, 11, 17, 57, 32);


        public UpdateBulgariaSteps(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
            IEnvironmentInfoService environmentInfoService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _environmentInfoService = environmentInfoService;
        }


        public async Task UpAsync()
        {
            using var uow = _unitOfWorkFactory.Create();
            var now = _environmentInfoService.CurrentUtcDateTime;
            var bulgariaCountryId = "bulgaria";
            var relocationPlanRepository = uow.GetRepository<RelocationPlan>();
            var relocationPlanStatusRepository = uow.GetRepository<RelocationPlanStatus>();
            var relocationPlanLoadStrategy = GetRelocationPlanLoadStrategy();
            var bulgariaRelocationPlans = await relocationPlanRepository
                .GetWhereAsync(rp => rp.Location.CountryId == bulgariaCountryId, relocationPlanLoadStrategy);
            var relocationPlanStatuses = await relocationPlanStatusRepository.GetAllAsync();

            var stepIdsToKeep = new List<RelocationStepId>
            {
                RelocationStepId.Induction,
                RelocationStepId.RelocationConfirmation,
                RelocationStepId.PendingApproval,
                RelocationStepId.ProcessingQueue,
                RelocationStepId.EmploymentConfirmation,
                RelocationStepId.EmploymentInProgress,
            };
            var stepIdsToAdd = new List<RelocationStepId>
            {
                RelocationStepId.TrpDocsPreparation,
                RelocationStepId.TrpDocsTranslationAndLegalization,
                RelocationStepId.TrpDocsSubmissionToMigrationDirectorate,
                RelocationStepId.EmbassyAppointment,
                RelocationStepId.VisaInProgress,
                RelocationStepId.VisaDone,
                RelocationStepId.TrpIdCardDocsInProgress,
            };
            var addAfterStepId = RelocationStepId.ProcessingQueue;

            var newSteps = CountryRelocationStep.GetDefaultSteps(bulgariaCountryId);
            var stepsToAdd = newSteps
                .Where(s => stepIdsToAdd.Contains(s.StepId))
                .ToList();

            foreach (var relocationPlan in bulgariaRelocationPlans)
            {
                var steps = relocationPlan.Steps
                    .Where(s => stepIdsToKeep.Contains(s.StepId))
                    .OrderBy(s => s.Order)
                    .ToList();

                var addAfterIndex = steps.FindIndex(s => s.StepId == addAfterStepId);

                if (addAfterIndex == -1)
                {
                    throw new InvalidOperationException($"Can't find step {addAfterStepId} in relocation plan {relocationPlan.ExternalId}");
                }

                foreach (var stepToAdd in stepsToAdd)
                {
                    steps.Insert(addAfterIndex + 1, new RelocationPlanStep
                    {
                        StepId = stepToAdd.StepId,
                    });

                    addAfterIndex++;
                }

                var currentStepId = MatchStep(relocationPlan.CurrentStepId);
                var currentCountryStep = newSteps.Single(s => s.StepId == currentStepId);
                var currentStep = steps.Single(s => s.StepId == currentStepId);
                relocationPlan.CurrentStepId = currentStepId;

                for (var i = 0; i < steps.Count; i++)
                {
                    var step = steps[i];
                    var previousStep = i > 0 ? steps[i - 1] : null;
                    var newStep = newSteps.Single(s => s.StepId == step.StepId);
                    var isStepCompleted = newStep.Order < currentCountryStep.Order;

                    step.DurationInDays = newStep.DurationInDays;
                    step.Order = newStep.Order;
                    step.CompletedAt = CalculateCompletedAt(step, isStepCompleted, now);
                    step.IsCompletionDateHidden = stepIdsToAdd.Contains(step.StepId) && step.CompletedAt.HasValue;
                    step.DurationInDays = newStep.DurationInDays;
                    step.ExpectedAt = CalculateExpectedAt(step, previousStep);
                }

                relocationPlan.Steps = steps;

                var (newStatusId, fromStepId) = RelocationPlanStatus.From(relocationPlan.State, relocationPlan.Status, currentStepId);
                var newStatus = relocationPlanStatuses.Single(s => s.ExternalId == newStatusId);

                var isStatusUpdated = Reflector.SetProperty(() => relocationPlan.StatusId, newStatus.Id);

                if (isStatusUpdated)
                {
                    relocationPlan.Status = newStatus;
                    relocationPlan.StatusStartDate = now;
                    relocationPlan.StatusDueDate = fromStepId != null
                        ? currentStep.ExpectedAt
                        : null;
                }

                relocationPlan.UpdateDate = now;
                relocationPlan.UpdatedBy = null;
            }

            await uow.SaveChangesAsync();
        }


        private static RelocationStepId MatchStep(RelocationStepId previousStepId)
        {
            // Will be overwritten by global mobility service migration
            RelocationStepId newStepId = previousStepId switch
            {
                RelocationStepId.Induction => RelocationStepId.Induction,
                RelocationStepId.RelocationConfirmation => RelocationStepId.RelocationConfirmation,
                RelocationStepId.PendingApproval => RelocationStepId.PendingApproval,
                RelocationStepId.ProcessingQueue => RelocationStepId.ProcessingQueue,
                RelocationStepId.VisaDocsPreparation => RelocationStepId.EmbassyAppointment,
                RelocationStepId.WaitingEmbassyAppointment => RelocationStepId.EmbassyAppointment,
                RelocationStepId.EmbassyAppointment => RelocationStepId.EmbassyAppointment,
                RelocationStepId.VisaInProgress => RelocationStepId.VisaInProgress,
                RelocationStepId.VisaDone => RelocationStepId.VisaDone,
                RelocationStepId.TrpDocsPreparation => RelocationStepId.TrpDocsPreparation,
                RelocationStepId.TrpApplicationSubmission => RelocationStepId.TrpDocsPreparation,
                RelocationStepId.TrpInProgress => RelocationStepId.TrpDocsPreparation,
                RelocationStepId.EmploymentConfirmation => RelocationStepId.EmploymentConfirmation,
                RelocationStepId.EmploymentInProgress => RelocationStepId.EmploymentInProgress,
                _ => throw new ArgumentOutOfRangeException(nameof(previousStepId), previousStepId, null),
            };

            return newStepId;
        }

        private static DateTime? CalculateCompletedAt(RelocationPlanStep step, bool isStepCompleted, DateTime now)
        {
            if (!isStepCompleted)
            {
                return null;
            }

            return step.CompletedAt ?? now;
        }

        private static DateTime? CalculateExpectedAt(RelocationPlanStep step, RelocationPlanStep previousStep)
        {
            if (previousStep == null)
            {
                return step.ExpectedAt;
            }

            if (previousStep.CompletedAt == null)
            {
                return previousStep.ExpectedAt.Value.AddDays(step.DurationInDays.Value);
            }

            return previousStep.CompletedAt.Value.AddDays(step.DurationInDays.Value);
        }

        private static IEntityLoadStrategy<RelocationPlan> GetRelocationPlanLoadStrategy()
        {
            return new EntityLoadStrategy<RelocationPlan>(
                p => p.Location,
                p => p.Employee.CurrentLocation.Location,
                p => p.Approver,
                p => p.HrManager,
                p => p.Status,
                p => p.Compensation,
                p => p.GmManager,
                p => p.Steps);
        }
    }
}