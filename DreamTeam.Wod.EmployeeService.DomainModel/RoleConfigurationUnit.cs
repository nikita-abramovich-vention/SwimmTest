namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RoleConfigurationUnit
    {
        public int RoleConfigurationId { get; set; }

        public RoleConfiguration RoleConfiguration { get; set; }

        public string UnitId { get; set; }
    }
}