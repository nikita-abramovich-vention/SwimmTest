using System;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class InternshipDataContract
    {
        public string Id { get; set; }

        public string PersonId { get; set; }

        public bool IsActive { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FirstNameLocal { get; set; }

        public string LastNameLocal { get; set; }

        public string PhotoId { get; set; }

        public string Skype { get; set; }

        public string Telegram { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Location { get; set; }

        public string DomainName { get; set; }

        public bool IsDomainNameVerified { get; set; }

        public string UnitId { get; set; }

        public string DepartmentUnitId { get; set; }

        public string UnitUnitId { get; set; }

        public string StudentLabId { get; set; }

        public string StudentLabProfileUrl { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public InternshipCloseReason? CloseReason { get; set; }

        public bool IsProduction { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}