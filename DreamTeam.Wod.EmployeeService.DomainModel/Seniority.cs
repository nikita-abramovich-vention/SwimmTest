namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class Seniority
    {
        public const int MaxNameLength = 200;
        public const string Default = BuiltIn.Middle;


        public int Id { get; set; }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public bool IsHidden { get; set; }

        public int Order { get; set; }



        public static class BuiltIn
        {
            public const string Junior = "junior";
            public const string Middle = "middle";
            public const string Senior = "senior";
            public const string Lead = "lead";
        }
    }
}