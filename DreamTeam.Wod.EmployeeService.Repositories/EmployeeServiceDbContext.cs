using DreamTeam.Common;
using DreamTeam.DomainModel;
using DreamTeam.Repositories.EntityFramework;
using DreamTeam.Repositories.EntityFramework.Interfaces;
using DreamTeam.Wod.EmployeeService.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Wod.EmployeeService.Repositories
{
    [UsedImplicitly]
    public sealed class EmployeeServiceDbContext : DbContext, IDbContext
    {
        private const int ExternalSystemKeyMaxLength = 64;


        public EmployeeServiceDbContext(DbContextOptions<EmployeeServiceDbContext> options)
            : base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Enum<EmploymentType>();

            modelBuilder.Entity<Employee>().Property(e => e.ExternalId).IsRequired();
            modelBuilder.Entity<Employee>().HasIndex(e => e.ExternalId).IsUnique();
            modelBuilder.Entity<Employee>().Property(e => e.PersonId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<Employee>().HasIndex(e => e.PersonId).IsUnique();
            modelBuilder.Entity<Employee>().Property(e => e.DomainName).IsRequired().HasMaxLength(Employee.DomainNameMaxLength);
            modelBuilder.Entity<Employee>().HasIndex(e => e.DomainName).IsUnique();
            modelBuilder.Entity<Employee>().Property(e => e.Email).HasMaxLength(Employee.EmailMaxLength);
            modelBuilder.Entity<Employee>().Property(e => e.UnitId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<Employee>().HasOne(e => e.ResponsibleHrManager).WithMany().HasForeignKey(e => e.ResponsibleHrManagerId);
            modelBuilder.Entity<Employee>().HasOne(e => e.Mentor).WithMany().HasForeignKey(e => e.MentorId);
            modelBuilder.Entity<Employee>().Property(e => e.CountryId).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<Employee>().Property(e => e.OrganizationId).HasMaxLength(ExternalSystemKeyMaxLength).IsConcurrencyToken();
            modelBuilder.Entity<Employee>().Property(e => e.EmploymentOfficeId).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<Employee>().HasIndex(e => new { e.IsActive, e.UnitId });
            modelBuilder.Entity<Employee>().HasOne<DeactivationReasons>().WithMany().HasForeignKey(e => e.DeactivationReason).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Employee>().Property(e => e.DeactivationReason).HasConversion<string>();
            modelBuilder.Entity<Employee>().WithEnum<EmploymentType>();
            ConfigureUpdateInfo<Employee>(modelBuilder);

            modelBuilder.Entity<EmploymentPeriod>().Property(p => p.OrganizationId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);

            modelBuilder.Entity<Seniority>().Property(s => s.ExternalId).IsRequired();
            modelBuilder.Entity<Seniority>().HasIndex(s => s.ExternalId).IsUnique();
            modelBuilder.Entity<Seniority>().Property(s => s.Name).IsRequired().HasMaxLength(Seniority.MaxNameLength);
            modelBuilder.Entity<Seniority>().HasData(
                new Seniority
                {
                    Id = 1,
                    ExternalId = Seniority.BuiltIn.Junior,
                    Name = "Junior",
                    Order = 1,
                },
                new Seniority
                {
                    Id = 2,
                    ExternalId = Seniority.BuiltIn.Middle,
                    Name = "Middle",
                    IsHidden = true,
                    Order = 2,
                },
                new Seniority
                {
                    Id = 3,
                    ExternalId = Seniority.BuiltIn.Senior,
                    Name = "Senior",
                    Order = 3,
                },
                new Seniority
                {
                    Id = 4,
                    ExternalId = Seniority.BuiltIn.Lead,
                    Name = "Lead",
                    Order = 4,
                });

            modelBuilder.Entity<InternshipCloseReasons>().ToTable("InternshipCloseReason");
            modelBuilder.Entity<InternshipCloseReasons>().Property(r => r.Id).HasConversion<string>();

            modelBuilder.Entity<TitleRole>().Property(r => r.ExternalId).IsRequired();
            modelBuilder.Entity<TitleRole>().HasIndex(r => r.ExternalId).IsUnique();
            modelBuilder.Entity<TitleRole>().Property(r => r.Name).IsRequired().HasMaxLength(TitleRole.NameMaxLength);
            modelBuilder.Entity<TitleRole>().HasIndex(r => r.Name).IsUnique();

            modelBuilder.Entity<Role>().Property(r => r.ExternalId).IsRequired();
            modelBuilder.Entity<Role>().HasIndex(r => r.ExternalId).IsUnique();
            modelBuilder.Entity<Role>().Property(r => r.Name).IsRequired().HasMaxLength(Role.NameMaxLength);
            modelBuilder.Entity<Role>().HasIndex(r => r.Name).IsUnique();
            modelBuilder.Entity<Role>().Property(r => r.Description).HasMaxLength(Role.DescriptionMaxLength);

            modelBuilder.Entity<Internship>().Property(i => i.ExternalId).IsRequired();
            modelBuilder.Entity<Internship>().HasIndex(i => i.ExternalId).IsUnique();
            modelBuilder.Entity<Internship>().Property(i => i.PersonId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.DomainName).IsRequired().HasMaxLength(Internship.DomainNameMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.UnitId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.CreatedBy).IsRequired(false);
            modelBuilder.Entity<Internship>().Property(i => i.FirstName).IsRequired().HasMaxLength(Internship.FirstNameLastNameMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.LastName).IsRequired().HasMaxLength(Internship.FirstNameLastNameMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.FirstNameLocal).HasMaxLength(Internship.FirstNameLastNameMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.LastNameLocal).HasMaxLength(Internship.FirstNameLastNameMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.PhotoId).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.Skype).HasMaxLength(Internship.SkypeMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.Telegram).HasMaxLength(Internship.TelegramMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.Email).HasMaxLength(Internship.EmailMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.Phone).HasMaxLength(Internship.PhoneMaxLength);
            modelBuilder.Entity<Internship>().HasIndex(i => i.DomainName).IsUnique().HasFilter($"{nameof(Internship.IsActive)} = 1");
            modelBuilder.Entity<Internship>().Property(i => i.Location).HasMaxLength(Internship.LocationMaxLength);
            modelBuilder.Entity<Internship>().Property(i => i.StudentLabId).HasMaxLength(ExternalSystemKeyMaxLength).IsRequired(false);
            modelBuilder.Entity<Internship>().Property(i => i.StudentLabProfileUrl).IsRequired(false);

            modelBuilder.Entity<Internship>().Property(r => r.CloseReason).HasConversion<string>();
            modelBuilder.Entity<Internship>().HasOne<InternshipCloseReasons>().WithMany().HasForeignKey(r => r.CloseReason).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CurrentLocation>().Property(l => l.ExternalId).IsRequired();
            modelBuilder.Entity<CurrentLocation>().HasIndex(l => l.ExternalId).IsUnique();
            modelBuilder.Entity<CurrentLocation>().Property(l => l.Name).IsRequired().HasMaxLength(CurrentLocation.NameMaxLength);
            modelBuilder.Entity<CurrentLocation>().HasIndex(l => l.Name).IsUnique();
            modelBuilder.Entity<CurrentLocation>().HasIndex(l => l.IsCustom);
            modelBuilder.Entity<CurrentLocation>().Property(l => l.CreatedBy).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<CurrentLocation>().Property(l => l.CountryId).HasMaxLength(ExternalSystemKeyMaxLength);

            modelBuilder.Entity<Employee>().HasOne(e => e.CurrentLocation).WithMany().HasForeignKey(e => e.CurrentLocationId);
            modelBuilder.Entity<Employee>()
                .HasOne<EmployeeCurrentLocation>()
                .WithOne(l => l.Employee)
                .HasForeignKey<EmployeeCurrentLocation>(l => l.EmployeeId);

            modelBuilder.Entity<EmployeeCurrentLocation>().Property(l => l.ChangedBy).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmployeeCurrentLocation>().HasIndex(l => l.EmployeeId).IsUnique();

            modelBuilder.Entity<DeactivationReasons>().ToTable("DeactivationReason");
            modelBuilder.Entity<DeactivationReasons>().Property(r => r.Id).HasConversion<string>();

            modelBuilder.Entity<RelocationPlanStates>().ToTable("RelocationPlanState");
            modelBuilder.Entity<RelocationPlanStates>().Property(s => s.Id).HasConversion<string>();

            modelBuilder.Entity<RelocationPlanChangeTypes>().ToTable("RelocationPlanChangeType");
            modelBuilder.Entity<RelocationPlanChangeTypes>().Property(t => t.Id).HasConversion<string>();

            modelBuilder.Entity<RelocationSteps>().ToTable("RelocationStep");
            modelBuilder.Entity<RelocationSteps>().Property(t => t.Id).HasConversion<string>();
            modelBuilder.Entity<RelocationSteps>().HasData(
                new RelocationSteps { Id = RelocationStepId.Induction },
                new RelocationSteps { Id = RelocationStepId.RelocationConfirmation },
                new RelocationSteps { Id = RelocationStepId.PendingApproval },
                new RelocationSteps { Id = RelocationStepId.ProcessingQueue },
                new RelocationSteps { Id = RelocationStepId.VisaDocsPreparation },
                new RelocationSteps { Id = RelocationStepId.WaitingEmbassyAppointment },
                new RelocationSteps { Id = RelocationStepId.EmbassyAppointment },
                new RelocationSteps { Id = RelocationStepId.VisaInProgress },
                new RelocationSteps { Id = RelocationStepId.VisaDone },
                new RelocationSteps { Id = RelocationStepId.TrpDocsPreparation },
                new RelocationSteps { Id = RelocationStepId.TrpDocsTranslationAndLegalization },
                new RelocationSteps { Id = RelocationStepId.TrpDocsSubmissionToMigrationDirectorate },
                new RelocationSteps { Id = RelocationStepId.TrpApplicationSubmission },
                new RelocationSteps { Id = RelocationStepId.TrpInProgress },
                new RelocationSteps { Id = RelocationStepId.TrpIdCardDocsInProgress },
                new RelocationSteps { Id = RelocationStepId.EmploymentConfirmation },
                new RelocationSteps { Id = RelocationStepId.EmploymentInProgress });

            modelBuilder.Entity<RelocationPlan>().Property(p => p.ExternalId).IsRequired();
            modelBuilder.Entity<RelocationPlan>().HasIndex(p => p.ExternalId).IsUnique();
            modelBuilder.Entity<RelocationPlan>().HasOne(p => p.Employee).WithMany().HasForeignKey(p => p.EmployeeId);
            modelBuilder.Entity<RelocationPlan>().HasIndex(p => new { p.EmployeeId, p.State }).IsUnique().HasFilter($"[{nameof(RelocationPlan.State)}] = '{RelocationPlanState.Active}'");
            modelBuilder.Entity<RelocationPlan>().Property(p => p.EmployeeComment).HasMaxLength(RelocationPlan.CommentMaxLength);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.GmComment).HasMaxLength(RelocationPlan.CommentMaxLength);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.ApproverComment).HasMaxLength(RelocationPlan.CommentMaxLength);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.HrManagerComment).HasMaxLength(RelocationPlan.CommentMaxLength);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.CloseComment).HasMaxLength(RelocationPlan.CommentMaxLength);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.Salary).HasMaxLength(RelocationPlan.SalaryMaxLength);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.CreatedBy).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.UpdatedBy).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.ApprovedBy).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.InductionStatusChangedBy).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.ClosedBy).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.RelocationUnitId).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<RelocationPlan>().HasOne<RelocationPlanStates>().WithMany().HasForeignKey(p => p.State).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RelocationPlan>().HasOne(p => p.Status).WithMany().HasForeignKey(p => p.StatusId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RelocationPlan>().HasOne(p => p.Compensation).WithOne(c => c.RelocationPlan).HasForeignKey<CompensationInfo>(c => c.RelocationPlanId);
            modelBuilder.Entity<RelocationPlan>().HasOne<RelocationSteps>().WithMany().HasForeignKey(s => s.CurrentStepId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RelocationPlan>().Property(p => p.CurrentStepId).HasDefaultValue(RelocationStepId.Induction);

            modelBuilder.Entity<RelocationPlanStep>().HasKey(s => new { s.RelocationPlanId, s.StepId });
            modelBuilder.Entity<RelocationPlanStep>().HasOne<RelocationSteps>().WithMany().HasForeignKey(s => s.StepId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CountryRelocationStep>().HasKey(s => new { s.CountryId, s.StepId });
            modelBuilder.Entity<CountryRelocationStep>().HasOne<RelocationSteps>().WithMany().HasForeignKey(s => s.StepId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompensationInfo>().OwnsOne(
                c => c.Details, cd =>
                {
                    cd.OwnsOne(cid => cid.Child);
                    cd.Navigation(cid => cid.Child).IsRequired();
                    cd.OwnsOne(cid => cid.Spouse);
                    cd.Navigation(cid => cid.Spouse).IsRequired();
                    cd.OwnsOne(cid => cid.Employee);
                    cd.Navigation(cid => cid.Employee).IsRequired();
                });
            modelBuilder.Entity<CompensationInfo>().Navigation(c => c.Details).IsRequired();
            modelBuilder.Entity<CompensationInfo>().OwnsOne(p => p.PreviousCompensation);
            modelBuilder.Entity<CompensationInfo>().Navigation(c => c.PreviousCompensation).IsRequired();

            modelBuilder.Entity<RelocationPlanTrpStates>().ToTable("RelocationPlanTrpState");
            modelBuilder.Entity<RelocationPlanTrpStates>().Property(t => t.Id).HasConversion<string>();
            modelBuilder.Entity<RelocationPlanTrpStates>().HasData(
                new RelocationPlanTrpStates { Id = RelocationPlanTrpState.DocsPreparation },
                new RelocationPlanTrpStates { Id = RelocationPlanTrpState.DocsTranslationAndLegalization },
                new RelocationPlanTrpStates { Id = RelocationPlanTrpState.SubmissionToMigrationDirectorate },
                new RelocationPlanTrpStates { Id = RelocationPlanTrpState.ApplicationSubmission },
                new RelocationPlanTrpStates { Id = RelocationPlanTrpState.InProgress },
                new RelocationPlanTrpStates { Id = RelocationPlanTrpState.IdCardDocsInProgress });

            modelBuilder.Entity<RelocationCaseStatus>().Property(s => s.ExternalId).IsRequired();
            modelBuilder.Entity<RelocationCaseStatus>().HasIndex(s => s.ExternalId).IsUnique();
            modelBuilder.Entity<RelocationCaseStatus>().Property(s => s.SourceId).IsRequired();
            modelBuilder.Entity<RelocationCaseStatus>().HasIndex(s => s.SourceId).IsUnique();
            modelBuilder.Entity<RelocationCaseStatus>().Property(s => s.Name).IsRequired();

            modelBuilder.Entity<RelocationPlanStatus>().Property(s => s.ExternalId).IsRequired();
            modelBuilder.Entity<RelocationPlanStatus>().HasIndex(s => s.ExternalId).IsUnique();
            modelBuilder.Entity<RelocationPlanStatus>().Property(s => s.Name).IsRequired().HasMaxLength(RelocationPlanStatus.NameMaxLength);
            modelBuilder.Entity<RelocationPlanStatus>().HasIndex(s => s.CaseStatusId).IsUnique().HasFilter($"[{nameof(RelocationPlanStatus.CaseStatusId)}] IS NOT NULL");
            modelBuilder.Entity<RelocationPlanStatus>().HasOne(s => s.CaseStatus).WithMany().HasForeignKey(s => s.CaseStatusId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RelocationPlanChange>().HasOne(c => c.Employee).WithMany().HasForeignKey(c => c.EmployeeId);
            modelBuilder.Entity<RelocationPlanChange>().HasOne(c => c.PreviousDestination).WithMany().HasForeignKey(c => c.PreviousDestinationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RelocationPlanChange>().HasOne(c => c.NewDestination).WithMany().HasForeignKey(c => c.NewDestinationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RelocationPlanChange>().HasOne(c => c.PreviousStatus).WithMany().HasForeignKey(c => c.PreviousStatusId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RelocationPlanChange>().HasOne(c => c.NewStatus).WithMany().HasForeignKey(c => c.NewStatusId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RelocationPlanChange>().HasOne(c => c.RelocationPlan).WithMany().HasForeignKey(c => c.RelocationPlanId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RelocationPlanChange>().HasOne<RelocationPlanChangeTypes>().WithMany().HasForeignKey(c => c.Type).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RelocationPlanChange>().Property(c => c.UpdatedBy).HasMaxLength(ExternalSystemKeyMaxLength);

            modelBuilder.Entity<EmployeeCurrentLocationChange>().HasOne(c => c.Employee).WithMany().HasForeignKey(c => c.EmployeeId);
            modelBuilder.Entity<EmployeeCurrentLocationChange>().HasOne(c => c.PreviousLocation).WithMany().HasForeignKey(c => c.PreviousLocationId);
            modelBuilder.Entity<EmployeeCurrentLocationChange>().HasOne(c => c.NewLocation).WithMany().HasForeignKey(c => c.NewLocationId);
            modelBuilder.Entity<EmployeeCurrentLocationChange>().Property(c => c.UpdatedBy).HasMaxLength(ExternalSystemKeyMaxLength);

            modelBuilder.Entity<EmployeeOrganizationChange>().HasOne(c => c.Employee).WithMany().HasForeignKey(c => c.EmployeeId);
            modelBuilder.Entity<EmployeeOrganizationChange>().Property(c => c.NewOrganizationId).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmployeeOrganizationChange>().Property(c => c.PreviousOrganizationId).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmployeeOrganizationChange>().Property(c => c.UpdatedBy).HasMaxLength(ExternalSystemKeyMaxLength);

            modelBuilder.Entity<RoleConfiguration>().HasKey(c => c.Id);
            modelBuilder.Entity<RoleConfiguration>().HasOne(c => c.Role).WithOne().HasForeignKey<RoleConfiguration>(c => c.Id);
            modelBuilder.Entity<RoleConfiguration>().Property(c => c.UpdatedBy).HasMaxLength(ExternalSystemKeyMaxLength);

            modelBuilder.Entity<RoleConfigurationTitleRole>().HasKey(c => new { c.RoleConfigurationId, c.TitleRoleId });

            modelBuilder.Entity<RoleConfigurationUnit>().HasKey(c => new { c.RoleConfigurationId, c.UnitId });
            modelBuilder.Entity<RoleConfigurationUnit>().Property(c => c.UnitId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);

            modelBuilder.Entity<RoleConfigurationEmployee>().HasKey(c => new { c.RoleConfigurationId, c.EmployeeId });

            modelBuilder.Entity<EmployeeSnapshot>().HasIndex(s => new { s.EmployeeId, s.FromDate }).IsUnique();
            modelBuilder.Entity<EmployeeSnapshot>().HasIndex(s => new { s.FromDate, s.ToDate });
            modelBuilder.Entity<EmployeeSnapshot>().Property(s => s.UnitId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmployeeSnapshot>().HasIndex(s => s.UnitId).IncludeProperties(s => new { s.FromDate, s.ToDate, s.IsActive });
            modelBuilder.Entity<EmployeeSnapshot>().Property(s => s.CountryId).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmployeeSnapshot>().Property(s => s.OrganizationId).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmployeeSnapshot>().WithEnum<EmploymentType>();

            modelBuilder.Entity<EmployeeSnapshotLog>().HasIndex(l => l.Date);
            modelBuilder.Entity<EmployeeSnapshotLog>().HasIndex(l => new { l.IsSuccessful, l.Date });

            modelBuilder.Enum<DismissalRequestType>();

            modelBuilder.Entity<InternshipCloseReasons>().HasData(
                new InternshipCloseReasons { Id = InternshipCloseReason.Manually },
                new InternshipCloseReasons { Id = InternshipCloseReason.AutomaticallyDueInactivity },
                new InternshipCloseReasons { Id = InternshipCloseReason.AutomaticallyDueEmployment });

            modelBuilder.Entity<DeactivationReasons>().HasData(
                new DeactivationReasons { Id = DeactivationReason.Dismissed },
                new DeactivationReasons { Id = DeactivationReason.MaternityLeave });

            modelBuilder.Entity<RelocationPlanStates>().HasData(
                new RelocationPlanStates { Id = RelocationPlanState.Active },
                new RelocationPlanStates { Id = RelocationPlanState.Completed },
                new RelocationPlanStates { Id = RelocationPlanState.Cancelled },
                new RelocationPlanStates { Id = RelocationPlanState.Rejected });

            modelBuilder.Entity<RelocationPlanChangeTypes>().HasData(
                new RelocationPlanChangeTypes { Id = RelocationPlanChangeType.InductionPassed },
                new RelocationPlanChangeTypes { Id = RelocationPlanChangeType.Status },
                new RelocationPlanChangeTypes { Id = RelocationPlanChangeType.Destination },
                new RelocationPlanChangeTypes { Id = RelocationPlanChangeType.Approved },
                new RelocationPlanChangeTypes { Id = RelocationPlanChangeType.Confirmed },
                new RelocationPlanChangeTypes { Id = RelocationPlanChangeType.EmploymentConfirmedByEmployee });

            var statuses = new[]
            {
                new RelocationPlanStatus { Id = 1, ExternalId = RelocationPlanStatus.BuiltIn.Induction, Name = "Induction" },
                new RelocationPlanStatus { Id = 2, ExternalId = RelocationPlanStatus.BuiltIn.EmployeeConfirmation, Name = "Employee confirmation" },
                new RelocationPlanStatus { Id = 3, ExternalId = RelocationPlanStatus.BuiltIn.PendingApproval, Name = "Pending approval" },
                new RelocationPlanStatus { Id = 4, ExternalId = RelocationPlanStatus.BuiltIn.RelocationApproved, Name = "Relocation approved" },
                new RelocationPlanStatus { Id = 5, ExternalId = RelocationPlanStatus.BuiltIn.InProgress, Name = "In progress" },
                new RelocationPlanStatus { Id = 6, ExternalId = RelocationPlanStatus.BuiltIn.VisaDocsPreparation, Name = "Visa docs preparation" },
                new RelocationPlanStatus { Id = 7, ExternalId = RelocationPlanStatus.BuiltIn.WaitingEmbassyAppointment, Name = "Waiting for embassy appointment" },
                new RelocationPlanStatus { Id = 8, ExternalId = RelocationPlanStatus.BuiltIn.EmbassyAppointment, Name = "Embassy appointment" },
                new RelocationPlanStatus { Id = 9, ExternalId = RelocationPlanStatus.BuiltIn.VisaInProgress, Name = "Visa in progress" },
                new RelocationPlanStatus { Id = 10, ExternalId = RelocationPlanStatus.BuiltIn.VisaDone, Name = "Visa done" },
                new RelocationPlanStatus { Id = 30, ExternalId = RelocationPlanStatus.BuiltIn.TrpDocsPreparation, Name = "TRP docs preparation" },
                new RelocationPlanStatus { Id = 31, ExternalId = RelocationPlanStatus.BuiltIn.TrpApplicationSubmission, Name = "TRP application submission" },
                new RelocationPlanStatus { Id = 32, ExternalId = RelocationPlanStatus.BuiltIn.TrpInProgress, Name = "TRP in progress" },
                new RelocationPlanStatus { Id = 40, ExternalId = RelocationPlanStatus.BuiltIn.TrpDocsTranslationAndLegalization, Name = "TRP docs translation and legalization" },
                new RelocationPlanStatus { Id = 41, ExternalId = RelocationPlanStatus.BuiltIn.TrpDocsSubmissionToMigrationDirectorate, Name = "TRP docs submission to the migration directorate" },
                new RelocationPlanStatus { Id = 42, ExternalId = RelocationPlanStatus.BuiltIn.TrpIdCardDocsInProgress, Name = "ID card docs in progress" },
            };

            modelBuilder.Entity<RelocationPlanStatus>().HasData(statuses);

            modelBuilder.Entity<EmployeeRole>().HasKey(er => new { er.EmployeeId, er.RoleId });

            modelBuilder.Entity<ExternalWorkplace>().Property(ew => ew.SourceId).IsRequired();
            modelBuilder.Entity<ExternalWorkplace>().Property(ew => ew.Name).IsRequired().HasMaxLength(ExternalWorkplace.NameMaxLength);
            modelBuilder.Entity<ExternalWorkplace>().Property(ew => ew.FullName).IsRequired().HasMaxLength(ExternalWorkplace.NameMaxLength);
            modelBuilder.Entity<ExternalWorkplace>().Property(ew => ew.SchemeUrl).IsRequired();
            modelBuilder.Entity<ExternalWorkplace>().Property(ew => ew.OfficeSourceId).IsRequired();

            modelBuilder.Entity<Workplace>().Property(w => w.ExternalId).IsRequired();
            modelBuilder.Entity<Workplace>().HasIndex(w => w.ExternalId).IsUnique();
            modelBuilder.Entity<Workplace>().Property(w => w.Name).IsRequired().HasMaxLength(Workplace.NameMaxLength);
            modelBuilder.Entity<Workplace>().Property(w => w.FullName).IsRequired().HasMaxLength(Workplace.NameMaxLength);
            modelBuilder.Entity<Workplace>().Property(w => w.SchemeUrl).IsRequired();
            modelBuilder.Entity<Workplace>().Property(w => w.OfficeId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);

            modelBuilder.Entity<ExternalEmployeeWorkplace>().Property(ew => ew.SourceEmployeeId).IsRequired();
            modelBuilder.Entity<ExternalEmployeeWorkplace>().Property(ew => ew.SourceWorkplaceId).IsRequired();
            modelBuilder.Entity<ExternalEmployeeWorkplace>().HasIndex(ew => new { ew.SourceEmployeeId, ew.SourceWorkplaceId }).IsUnique();

            modelBuilder.Entity<EmployeeWorkplace>().HasIndex(ew => new { ew.EmployeeId, ew.WorkplaceId }).IsUnique();
            modelBuilder.Entity<EmployeeWorkplace>().HasOne(ew => ew.ExternalEmployeeWorkplace).WithOne(ew => ew.EmployeeWorkplace).HasForeignKey<ExternalEmployeeWorkplace>(ew => ew.EmployeeWorkplaceId);

            modelBuilder.Entity<RelocationApprover>().HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId);
            modelBuilder.Entity<RelocationApprover>().HasIndex(a => new { a.EmployeeId, a.CountryId }).IsUnique();
            modelBuilder.Entity<RelocationApprover>().HasIndex(a => a.CountryId).IsUnique().HasFilter($"[{nameof(RelocationApprover.IsPrimary)}] = 1");
            modelBuilder.Entity<RelocationApprover>().Property(a => a.CountryId).IsRequired();
            modelBuilder.Entity<RelocationApprover>().Property(a => a.CreatedBy).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<RelocationApprover>().Property(a => a.UpdatedBy).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<RelocationApprover>().HasOne(a => a.ApproverOrder).WithOne().HasForeignKey<RelocationApprover>(q => q.ApproverOrderId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RelocationApproverOrder>().Property(q => q.IsNext).IsConcurrencyToken();

            modelBuilder.Entity<RelocationApproverAssignment>().HasOne(a => a.RelocationPlan).WithMany().HasForeignKey(a => a.RelocationPlanId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RelocationApproverAssignment>().HasOne(a => a.Approver).WithMany().HasForeignKey(a => a.ApproverId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExternalEmploymentRequest>().Property(r => r.UnitId).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<ExternalEmploymentRequest>().Property(r => r.StatusName).HasMaxLength(ExternalEmploymentRequest.StatusNameMaxLength);
            modelBuilder.Entity<ExternalEmploymentRequest>().Property(r => r.FirstName).HasMaxLength(ExternalEmploymentRequest.FirstNameLastNameMaxLength);
            modelBuilder.Entity<ExternalEmploymentRequest>().Property(r => r.LastName).HasMaxLength(ExternalEmploymentRequest.FirstNameLastNameMaxLength);
            modelBuilder.Entity<ExternalEmploymentRequest>().Property(r => r.Location).HasMaxLength(ExternalEmploymentRequest.LocationMaxLength);
            modelBuilder.Entity<ExternalEmploymentRequest>().Property(r => r.Type).HasMaxLength(ExternalEmploymentRequest.TypeMaxLength);
            modelBuilder.Entity<ExternalEmploymentRequest>().Property(r => r.OrganizationId).HasMaxLength(ExternalSystemKeyMaxLength);

            modelBuilder.Entity<EmploymentRequest>().Property(r => r.UnitId).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmploymentRequest>().Property(r => r.FirstName).HasMaxLength(EmploymentRequest.FirstNameLastNameMaxLength);
            modelBuilder.Entity<EmploymentRequest>().Property(r => r.LastName).HasMaxLength(EmploymentRequest.FirstNameLastNameMaxLength);
            modelBuilder.Entity<EmploymentRequest>().Property(r => r.Location).HasMaxLength(EmploymentRequest.LocationMaxLength);
            modelBuilder.Entity<EmploymentRequest>().Property(r => r.CountryId).HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmploymentRequest>().Property(r => r.OrganizationId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmploymentRequest>().Property(r => r.ExternalId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmploymentRequest>().HasIndex(r => r.ExternalId).IsUnique();
            modelBuilder.Entity<EmploymentRequest>().HasOne(r => r.SourceEmploymentRequest).WithMany().HasForeignKey(r => r.SourceId);

            modelBuilder.Entity<ExternalDismissalRequest>().HasIndex(r => r.SourceId).IsUnique();
            modelBuilder.Entity<ExternalDismissalRequest>().Property(r => r.SourceEmployeeId).IsRequired();
            modelBuilder.Entity<ExternalDismissalRequest>().Property(r => r.DismissalSpecificId).HasMaxLength(ExternalDismissalRequest.DismissalSpecificIdMaxLength);

            modelBuilder.Entity<DismissalRequest>().Property(r => r.ExternalId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<DismissalRequest>().HasIndex(r => r.ExternalId).IsUnique();
            modelBuilder.Entity<DismissalRequest>().HasIndex(r => r.SourceDismissalRequestId).IsUnique();
            modelBuilder.Entity<DismissalRequest>().HasOne(r => r.SourceDismissalRequest).WithMany().HasForeignKey(r => r.SourceDismissalRequestId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DismissalRequest>().WithEnum<DismissalRequestType>();

            modelBuilder.Entity<ExternalEmployeeUnitHistory>().Property(h => h.SourceEmployeeId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<ExternalEmployeeUnitHistory>().Property(h => h.SourceUnitId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<ExternalEmployeeUnitHistory>().HasIndex(h => new { h.SourceEmployeeId, h.SourceUnitId, h.StartDate });
            modelBuilder.Entity<ExternalEmployeeUnitHistory>().HasIndex(h => new { h.SourceEmployeeId, h.SourceUnitId, h.EndDate });

            modelBuilder.Entity<EmployeeUnitHistory>().Property(h => h.UnitId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmployeeUnitHistory>().Property(h => h.ExternalId).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
            modelBuilder.Entity<EmployeeUnitHistory>().HasIndex(h => h.ExternalId).IsUnique();
            modelBuilder.Entity<EmployeeUnitHistory>().HasOne(h => h.ExternalEmployeeUnitHistory).WithOne(h => h.EmployeeUnitHistory).HasForeignKey<ExternalEmployeeUnitHistory>(h => h.EmployeeUnitHistoryId);
            modelBuilder.Entity<EmployeeUnitHistory>().HasIndex(h => new { h.EmployeeId, h.UnitId, h.StartDate });
            modelBuilder.Entity<EmployeeUnitHistory>().HasIndex(h => new { h.EmployeeId, h.UnitId, h.EndDate });

            modelBuilder.Entity<SyncTypes>().ToTable("SyncType");
            modelBuilder.Entity<SyncTypes>().Property(t => t.Id).HasConversion<string>();
            modelBuilder.Entity<SyncTypes>().HasData(
                new SyncTypes { Id = SyncType.DownloadExternalWspData },
                new SyncTypes { Id = SyncType.LinkEmployeeWorkplaces },
                new SyncTypes { Id = SyncType.DownloadExternalEmploymentRequestData },
                new SyncTypes { Id = SyncType.LinkEmploymentRequests },
                new SyncTypes { Id = SyncType.DownloadExternalDismissalRequestData },
                new SyncTypes { Id = SyncType.LinkDismissalRequests },
                new SyncTypes { Id = SyncType.DownloadExternalEmployeeUnitHistory },
                new SyncTypes { Id = SyncType.LinkEmployeeUnitHistory });

            modelBuilder.Entity<StudentLabSyncLog>().HasKey(l => l.Id);
            modelBuilder.Entity<StudentLabSyncLog>().HasIndex(l => new { l.IsSuccessful, l.SyncCompletedDate });

            modelBuilder.Entity<SyncLog>().HasOne<SyncTypes>().WithMany().HasForeignKey(l => l.Type).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SyncLog>().HasIndex(l => new { l.Type, l.IsSuccessful, l.SyncCompletedDate });

            base.OnModelCreating(modelBuilder);
        }


        private static void ConfigureCreateUpdateInfo<T>(ModelBuilder modelBuilder) where T : class, IHasCreateUpdateInfo
        {
            ConfigureCreateInfo<T>(modelBuilder);
            ConfigureUpdateInfo<T>(modelBuilder);
        }

        private static void ConfigureUpdateInfo<T>(ModelBuilder modelBuilder) where T : class, IHasUpdateInfo
        {
            modelBuilder.Entity<T>().Property(t => t.UpdatedBy).HasMaxLength(ExternalSystemKeyMaxLength);
        }

        private static void ConfigureCreateInfo<T>(ModelBuilder modelBuilder) where T : class, IHasCreateInfo
        {
            modelBuilder.Entity<T>().Property(t => t.CreatedBy).IsRequired().HasMaxLength(ExternalSystemKeyMaxLength);
        }



        private sealed class InternshipCloseReasons
        {
            public InternshipCloseReason Id { get; set; }
        }

        private sealed class DeactivationReasons
        {
            public DeactivationReason Id { get; set; }
        }

        private sealed class RelocationPlanStates
        {
            public RelocationPlanState Id { get; set; }
        }

        private sealed class RelocationPlanChangeTypes
        {
            public RelocationPlanChangeType Id { get; set; }
        }

        private sealed class RelocationPlanTrpStates
        {
            public RelocationPlanTrpState Id { get; set; }
        }

        private sealed class SyncTypes
        {
            public SyncType Id { get; set; }
        }

        private sealed class RelocationSteps
        {
            public RelocationStepId Id { get; set; }
        }
    }
}
