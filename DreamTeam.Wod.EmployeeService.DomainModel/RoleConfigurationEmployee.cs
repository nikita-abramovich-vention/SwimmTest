namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RoleConfigurationEmployee
    {
        public int RoleConfigurationId { get; set; }

        public RoleConfiguration RoleConfiguration { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }
    }
}