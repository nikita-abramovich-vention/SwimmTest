namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RoleConfigurationTitleRole
    {
        public int RoleConfigurationId { get; set; }

        public RoleConfiguration RoleConfiguration { get; set; }

        public int TitleRoleId { get; set; }

        public TitleRole TitleRole { get; set; }
    }
}