using System;
using DreamTeam.DomainModel;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RelocationApprover : IHasCreateUpdateInfo
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public string CountryId { get; set; }

        public bool IsPrimary { get; set; }

        public int? ApproverOrderId { get; set; }

        public RelocationApproverOrder ApproverOrder { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}