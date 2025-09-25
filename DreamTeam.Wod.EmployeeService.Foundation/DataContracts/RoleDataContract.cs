namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public class RoleDataContract
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsBuiltIn { get; set; }

        public string RoleManagerId { get; set; }
    }
}