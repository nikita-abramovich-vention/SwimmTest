using System.Collections.Generic;

namespace DreamTeam.Wod.EmployeeService.Foundation.ActiveDirectory
{
    public sealed class ActiveDirectoryAuthenticationOptions
    {
        public const string SectionName = "ActiveDirectoryAuthentication";


        public IReadOnlyDictionary<string, ActiveDirectoryServerOptions> Servers { get; set; }
    }
}