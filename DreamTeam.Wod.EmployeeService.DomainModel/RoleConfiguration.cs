using System;
using System.Collections.Generic;
using DreamTeam.DomainModel;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RoleConfiguration : IHasUpdateInfo
    {
        public int Id { get; set; }

        public Role Role { get; set; }

        public ICollection<RoleConfigurationTitleRole> TitleRoles { get; set; }

        public bool IsAllTitleRoles => TitleRoles.Count == 0 && Units.Count != 0;

        public ICollection<RoleConfigurationUnit> Units { get; set; }

        public bool IsAllUnits => Units.Count == 0 && TitleRoles.Count != 0;

        public ICollection<RoleConfigurationEmployee> Employees { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}