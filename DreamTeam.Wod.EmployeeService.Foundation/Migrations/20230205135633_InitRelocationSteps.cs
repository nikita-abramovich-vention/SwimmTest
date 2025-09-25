using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class InitRelocationSteps : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IRelocationPlanService _relocationPlanService;
        private readonly IEnvironmentInfoService _environmentInfoService;


        public string Name => "InitRelocationSteps";

        public DateTime CreationDate => new DateTime(2023, 02, 05, 13, 56, 33);


        public InitRelocationSteps(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
            IRelocationPlanService relocationPlanService,
            IEnvironmentInfoService environmentInfoService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _relocationPlanService = relocationPlanService;
            _environmentInfoService = environmentInfoService;
        }


        public async Task UpAsync()
        {
            var now = _environmentInfoService.CurrentUtcDateTime;
            using var uow = _unitOfWorkFactory.Create();

            var repository = uow.GetRepository<RelocationPlan>();
            var loadStrategy = new EntityLoadStrategy<RelocationPlan>(
                p => p.Location,
                p => p.Employee.CurrentLocation.Location,
                p => p.Status.CaseStatus,
                p => p.Steps);
            var relocationPlans = await repository.GetAllAsync(loadStrategy);

            var countryIds = relocationPlans
                .Select(r => r.Location.CountryId)
                // some old requests on our dev envs have unknown country
                // its breaks relocation preconditions so just hacking for dev/local deployment
                .Where(id => id != null)
                .Distinct();
            var countryStepsByCountry = await countryIds
                .SelectAsync(async countryId => (countryId, steps: await _relocationPlanService.GetCountryStepsAsync(countryId, uow)));
            var countryStepsMap = countryStepsByCountry.ToDictionary(x => x.countryId, x => x.steps);

            foreach (var relocationPlan in relocationPlans)
            {
                // ignore such relocation for dev/local deployment
                if (relocationPlan.Location.CountryId == null)
                {
                    continue;
                }

                var countrySteps = countryStepsMap[relocationPlan.Location.CountryId];
                if (!relocationPlan.Steps.Any())
                {
                    relocationPlan.InitSteps(countrySteps);

                    var inductionStep = relocationPlan.GetStep(RelocationStepId.Induction);
                    var countryInductionStep = countrySteps.SingleOrDefault(s => s.StepId == RelocationStepId.Induction);
                    var creationDate = relocationPlan.CreationDate;
                    if (inductionStep != null && countryInductionStep != null)
                    {
                        inductionStep.ExpectedAt = creationDate.AddDays(countryInductionStep.DurationInDays ?? 0);
                    }

                    relocationPlan.UpdateStepExpectedDates(countrySteps, now);
                }

                var stepId = relocationPlan.Status.ExternalId switch
                {
                    RelocationPlanStatus.BuiltIn.Induction => RelocationStepId.Induction,
                    RelocationPlanStatus.BuiltIn.EmployeeConfirmation => RelocationStepId.RelocationConfirmation,
                    RelocationPlanStatus.BuiltIn.PendingApproval => RelocationStepId.PendingApproval,
                    RelocationPlanStatus.BuiltIn.RelocationApproved => RelocationStepId.ProcessingQueue,
                    RelocationPlanStatus.BuiltIn.InProgress => RelocationStepId.ProcessingQueue,
                    RelocationPlanStatus.BuiltIn.VisaDocsPreparation => RelocationStepId.VisaDocsPreparation,
                    RelocationPlanStatus.BuiltIn.WaitingEmbassyAppointment => RelocationStepId.WaitingEmbassyAppointment,
                    RelocationPlanStatus.BuiltIn.EmbassyAppointment => RelocationStepId.EmbassyAppointment,
                    RelocationPlanStatus.BuiltIn.VisaInProgress => RelocationStepId.VisaInProgress,
                    RelocationPlanStatus.BuiltIn.VisaDone => RelocationStepId.VisaDone,
                    RelocationPlanStatus.BuiltIn.TrpDocsPreparation => RelocationStepId.TrpDocsPreparation,
                    RelocationPlanStatus.BuiltIn.TrpApplicationSubmission => RelocationStepId.TrpApplicationSubmission,
                    RelocationPlanStatus.BuiltIn.TrpInProgress => RelocationStepId.TrpInProgress,
                    RelocationPlanStatus.BuiltIn.EmploymentConfirmationByEmployee => RelocationStepId.EmploymentConfirmation,
                    RelocationPlanStatus.BuiltIn.ReadyForEmployment => RelocationStepId.EmploymentInProgress,
                    _ => (RelocationStepId?)null,
                };

                if (stepId != null)
                {
                    relocationPlan.SetStep(stepId.Value, countrySteps, relocationPlan.StatusStartDate);
                    var currentStep = relocationPlan.CurrentStep;
                    currentStep.ExpectedAt = relocationPlan.StatusDueDate ?? currentStep.ExpectedAt;
                }
            }

            await uow.SaveChangesAsync();
        }
    }
}