using System;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class DismissalRequestDataContract
    {
        public string Id { get; set; }

        public bool IsActive { get; set; }

        public string EmployeeId { get; set; }

        public DismissalRequestType Type { get; set; }

        public DateOnly DismissalDate { get; set; }

        public DateTime? CloseDate { get; set; }

        public bool IsLinked { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
