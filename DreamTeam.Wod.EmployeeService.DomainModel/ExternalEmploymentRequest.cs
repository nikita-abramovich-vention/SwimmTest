using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class ExternalEmploymentRequest
    {
        public const int StatusNameMaxLength = 50;
        public const int FirstNameLastNameMaxLength = 200;
        public const int LocationMaxLength = 200;
        public const int TypeMaxLength = 50;


        public int Id { get; set; }

        public int SourceId { get; set; }

        public string Type { get; set; }

        public int StatusId { get; set; }

        public string StatusName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UnitId { get; set; }

        public int LocationId { get; set; }

        public string Location { get; set; }

        public string OrganizationId { get; set; }

        public DateOnly EmploymentDate { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public DateTime? CloseDate { get; set; }
    }
}
