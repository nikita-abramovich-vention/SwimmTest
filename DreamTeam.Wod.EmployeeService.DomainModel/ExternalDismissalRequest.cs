using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class ExternalDismissalRequest
    {
        public const int DismissalSpecificIdMaxLength = 200;

        public int Id { get; set; }

        public int SourceId { get; set; }

        public bool IsActive { get; set; }

        public string SourceEmployeeId { get; set; }

        public DateOnly DismissalDate { get; set; }

        public DateTime SourceCreationDate { get; set; }

        public DateTime? CloseDate { get; set; }

        public string DismissalSpecificId { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}