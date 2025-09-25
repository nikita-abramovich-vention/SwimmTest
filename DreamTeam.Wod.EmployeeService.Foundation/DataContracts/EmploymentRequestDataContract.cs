using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class EmploymentRequestDataContract
    {
        public string Id { get; set; }

        public string EmployeeId { get; set; }

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
