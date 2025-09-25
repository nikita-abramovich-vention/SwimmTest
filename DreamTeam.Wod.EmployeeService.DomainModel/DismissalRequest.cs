using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class DismissalRequest
    {
        public int Id { get; set; }

        public string ExternalId { get; set; }

        public bool IsActive { get; set; }

        public int? SourceDismissalRequestId { get; set; }

        public ExternalDismissalRequest SourceDismissalRequest { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public DateOnly DismissalDate { get; set; }

        public DismissalRequestType Type { get; set; }

        public DateTime? CloseDate { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? UpdateDate { get; set; }


        public DismissalRequest Clone()
        {
            var clone = (DismissalRequest)MemberwiseClone();

            return clone;
        }
    }
}