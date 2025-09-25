using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RelocationCaseStatus
    {
        public int Id { get; set; }

        public string ExternalId { get; set; }

        public string SourceId { get; set; }

        public string Name { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
