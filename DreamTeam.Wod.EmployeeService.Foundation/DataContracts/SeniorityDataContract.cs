namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class SeniorityDataContract
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsHidden { get; set; }

        public int Order { get; set; }
    }
}