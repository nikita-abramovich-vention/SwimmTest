using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class EmployeeSnapshot
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }

        public bool IsActive { get; set; }

        public int? SeniorityId { get; set; }

        public Seniority Seniority { get; set; }

        public int? TitleRoleId { get; set; }

        public TitleRole TitleRole { get; set; }

        public string CountryId { get; set; }

        public string OrganizationId { get; set; }

        public string UnitId { get; set; }

        public EmploymentType EmploymentType { get; set; }
    }
}
