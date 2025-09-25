using System;
using System.Collections.Generic;
using System.Linq;
using DreamTeam.DomainModel;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class Employee : IHasUpdateInfo
    {
        public const int DomainNameMaxLength = 200;
        public const int EmailMaxLength = 254;


        public int Id { get; set; }

        public string ExternalId { get; set; }

        public string PersonId { get; set; }

        public bool IsActive { get; set; }

        public bool IsDismissed { get; set; }

        public string DomainName { get; set; }

        public string Email { get; set; }

        public ICollection<EmploymentPeriod> EmploymentPeriods { get; set; }

        public ICollection<WageRatePeriod> WageRatePeriods { get; set; }

        public string UnitId { get; set; }

        public int? ResponsibleHrManagerId { get; set; }

        public Employee ResponsibleHrManager { get; set; }

        public int? MentorId { get; set; }

        public Employee Mentor { get; set; }

        public DateOnly EmploymentDate { get; set; }

        public DateOnly? DismissalDate { get; set; }

        public int? SeniorityId { get; set; }

        public Seniority Seniority { get; set; }

        public int? TitleRoleId { get; set; }

        public TitleRole TitleRole { get; set; }

        public ICollection<EmployeeRole> Roles { get; set; }

        public string CountryId { get; set; }

        public string OrganizationId { get; set; }

        public string EmploymentOfficeId { get; set; }

        public int? CurrentLocationId { get; set; }

        public EmployeeCurrentLocation CurrentLocation { get; set; }

        public DeactivationReason? DeactivationReason { get; set; }

        public ICollection<EmployeeWorkplace> Workplaces { get; set; }

        public EmploymentType EmploymentType { get; set; }

        public DateTime CreationDate { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }


        public Employee Clone()
        {
            var roles = Roles?.ToList();
            var clone = (Employee)MemberwiseClone();
            clone.Roles = roles;

            return clone;
        }
    }
}