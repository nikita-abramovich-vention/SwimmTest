using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class ExternalEmployeeWorkplace
    {
        public int Id { get; set; }

        public string SourceEmployeeId { get; set; }

        public string SourceWorkplaceId { get; set; }

        public DateTime CreationDate { get; set; }

        public int? EmployeeWorkplaceId { get; set; }

        public EmployeeWorkplace EmployeeWorkplace { get; set; }
    }
}