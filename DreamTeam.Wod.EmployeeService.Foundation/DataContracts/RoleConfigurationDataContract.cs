using System.Collections.Generic;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RoleConfigurationDataContract
    {
        public IReadOnlyCollection<string> TitleRoleIds { get; set; }

        public bool IsAllTitleRoles { get; set; }

        public IReadOnlyCollection<string> UnitIds { get; set; }

        public bool IsAllUnits { get; set; }

        public IReadOnlyCollection<string> EmployeeIds { get; set; }
    }
}