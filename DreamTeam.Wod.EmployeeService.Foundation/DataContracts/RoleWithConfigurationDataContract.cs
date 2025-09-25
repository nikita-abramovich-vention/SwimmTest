namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RoleWithConfigurationDataContract : RoleDataContract
    {
        public RoleConfigurationDataContract Configuration { get; set; }
    }
}