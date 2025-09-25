using System;
using System.Collections.Generic;
using DreamTeam.Services.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;
using IHasUpdateInfoDataContract = DreamTeam.Microservices.DataContracts.IHasUpdateInfoDataContract;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class EmployeeDataContract : IHasUpdateInfoDataContract
    {
        public string Id { get; set; }

        public string PersonId { get; set; }

        public bool IsActive { get; set; }

        public string DomainName { get; set; }

        public IReadOnlyCollection<EmploymentPeriodDataContract> EmploymentPeriods { get; set; }

        public IReadOnlyCollection<EmploymentPeriodDataContract> InternshipPeriods { get; set; }

        public IReadOnlyCollection<WageRatePeriodDataContract> WageRatePeriods { get; set; }

        public string UnitId { get; set; }

        public string ParentUnitId { get; set; }

        public string ResponsibleHrManagerId { get; set; }

        public string MentorId { get; set; }

        public string CountryId { get; set; }

        public string OrganizationId { get; set; }

        public string DisplayManagerId { get; set; }

        public string UnitManagerId { get; set; }

        public bool IsUnitManager { get; set; }

        public string DepartmentUnitId { get; set; }

        public string UnitUnitId { get; set; }

        public DateOnly EmploymentDate { get; set; }

        public DateOnly? DismissalDate { get; set; }

        public TitleDataContract Title { get; set; }

        public ICollection<string> RoleIds { get; set; }

        public string TitleRoleId { get; set; }

        public TitleRoleDataContract TitleRole { get; set; }

        public string SeniorityId { get; set; }

        public SeniorityDataContract Seniority { get; set; }

        public string Location { get; set; }

        public IReadOnlyCollection<WorkplaceDataContract> Workplaces { get; set; }

        public bool IsProduction { get; set; }

        public Language Language { get; set; }

        public EmployeeCurrentLocationDataContract CurrentLocation { get; set; }

        public DeactivationReason? DeactivationReason { get; set; }

        public string EmploymentOfficeId { get; set; }

        public EmploymentType EmploymentType { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}