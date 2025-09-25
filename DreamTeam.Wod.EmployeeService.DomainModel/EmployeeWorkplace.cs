using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class EmployeeWorkplace
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public int WorkplaceId { get; set; }

        public Workplace Workplace { get; set; }

        public DateTime CreationDate { get; set; }

        public ExternalEmployeeWorkplace ExternalEmployeeWorkplace { get; set; }
    }
}