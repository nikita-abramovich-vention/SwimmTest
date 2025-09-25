using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class ExternalEmployeeUnitHistory
    {
        public int Id { get; set; }

        public string SourceEmployeeId { get; set; }

        public string SourceUnitId { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public int? EmployeeUnitHistoryId { get; set; }

        public EmployeeUnitHistory EmployeeUnitHistory { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}