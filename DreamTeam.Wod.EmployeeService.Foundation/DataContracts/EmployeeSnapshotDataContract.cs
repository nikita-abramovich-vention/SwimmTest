using System;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class EmployeeSnapshotDataContract
    {
        public string EmployeeId { get; set; }

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }

        public bool IsActive { get; set; }

        public string SeniorityId { get; set; }

        public string TitleRoleId { get; set; }

        public string CountryId { get; set; }

        public string OrganizationId { get; set; }

        public string UnitId { get; set; }

        public EmploymentType EmploymentType { get; set; }
    }
}
