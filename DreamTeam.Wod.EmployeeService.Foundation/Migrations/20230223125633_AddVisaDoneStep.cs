using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans;
using DreamTeam.Wod.EmployeeService.Foundation.Roles;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class AddVisaDoneStep : IMigration
    {
        private const int VisaDoneStepDurationInDays = 14;

        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IRelocationPlanService _relocationPlanService;
        private readonly IEnvironmentInfoService _environmentInfoService;


        public string Name => "AddVisaDoneStep";

        public DateTime CreationDate => new DateTime(2023, 02, 23, 12, 56, 33);


        public AddVisaDoneStep(
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

            var countryStepsRepository = uow.GetRepository<CountryRelocationStep>();
            var countrySteps = await countryStepsRepository.GetAllAsync();
            var countryStepsMap = countrySteps
                .GroupBy(s => s.CountryId)
                .ToDictionary(
                    g => g.Key,
                    steps => steps.OrderBy(s => s.Order).ToList());
            foreach (var (countryId, steps) in countryStepsMap)
            {
                if (steps.Any(s => s.StepId == RelocationStepId.VisaDone))
                {
                    continue;
                }

                var lastVisaStepIndex = steps.FindLastIndex(s =>
                    s.StepId == RelocationStepId.VisaDocsPreparation ||
                    s.StepId == RelocationStepId.WaitingEmbassyAppointment ||
                    s.StepId == RelocationStepId.EmbassyAppointment ||
                    s.StepId == RelocationStepId.VisaInProgress);
                if (lastVisaStepIndex == -1)
                {
                    continue;
                }

                var lastVisaStep = steps[lastVisaStepIndex];
                var visaDoneStep = new CountryRelocationStep()
                {
                    StepId = RelocationStepId.VisaDone,
                    CountryId = countryId,
                    DurationInDays = VisaDoneStepDurationInDays,
                    Order = lastVisaStep.Order + 1,
                };

                var stepsAfterVisa = steps.Skip(lastVisaStepIndex + 1);

                steps.Insert(lastVisaStepIndex + 1, visaDoneStep);
                countryStepsRepository.Add(visaDoneStep);

                foreach (var step in stepsAfterVisa)
                {
                    step.Order = step.Order + 1;
                }
            }

            var relocationPlanRepository = uow.GetRepository<RelocationPlan>();
            var relocationPlanLoadStrategy = new EntityLoadStrategy<RelocationPlan>(
                p => p.Location,
                p => p.Employee.CurrentLocation.Location,
                p => p.Status.CaseStatus,
                p => p.Steps);
            var relocationPlans = await relocationPlanRepository.GetAllAsync(relocationPlanLoadStrategy);

            foreach (var plan in relocationPlans)
            {
                if (plan.Steps.Any(s => s.StepId == RelocationStepId.VisaDone))
                {
                    continue;
                }

                var stepsList = plan.Steps.OrderBy(s => s.Order).ToList();
                var lastVisaStepIndex = stepsList.FindLastIndex(s =>
                    s.StepId == RelocationStepId.VisaDocsPreparation ||
                    s.StepId == RelocationStepId.WaitingEmbassyAppointment ||
                    s.StepId == RelocationStepId.EmbassyAppointment ||
                    s.StepId == RelocationStepId.VisaInProgress);
                if (lastVisaStepIndex == -1)
                {
                    continue;
                }

                var lastVisaStep = stepsList[lastVisaStepIndex];
                var visaDoneStep = new RelocationPlanStep()
                {
                    RelocationPlanId = plan.Id,
                    StepId = RelocationStepId.VisaDone,
                    DurationInDays = VisaDoneStepDurationInDays,
                    Order = lastVisaStep.Order + 1,
                    CompletedAt = lastVisaStep.CompletedAt,
                    ExpectedAt = lastVisaStep.ExpectedAt + TimeSpan.FromDays(VisaDoneStepDurationInDays),
                };

                var stepsAfterVisa = stepsList.Skip(lastVisaStepIndex + 1);
                plan.Steps.Add(visaDoneStep);

                foreach (var step in stepsAfterVisa)
                {
                    step.Order = step.Order + 1;
                }
            }

            await uow.SaveChangesAsync();
        }
    }
}