using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class EmployeeUnitHistory
    {
        public int Id { get; set; }

        public string ExternalId { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public string UnitId { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public ExternalEmployeeUnitHistory ExternalEmployeeUnitHistory { get; set; }


        public EmployeeUnitHistory Clone()
        {
            var clone = (EmployeeUnitHistory)MemberwiseClone();

            return clone;
        }
    }
}