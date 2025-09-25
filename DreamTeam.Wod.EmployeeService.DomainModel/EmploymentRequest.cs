using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class EmploymentRequest
    {
        public const int FirstNameLastNameMaxLength = 200;
        public const int LocationMaxLength = 200;


        public int Id { get; set; }

        public string ExternalId { get; set; }

        public int SourceId { get; set; }

        public ExternalEmploymentRequest SourceEmploymentRequest { get; set; }

        public int? EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UnitId { get; set; }

        public string Location { get; set; }

        public string CountryId { get; set; }

        public string OrganizationId { get; set; }

        public DateOnly EmploymentDate { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}
