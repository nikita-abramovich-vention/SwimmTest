namespace DreamTeam.Wod.EmployeeService.Foundation.ActiveDirectory
{
    public sealed class ActiveDirectoryServerOptions
    {
        public string Path { get; set; }

        public bool IsEnabled { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}