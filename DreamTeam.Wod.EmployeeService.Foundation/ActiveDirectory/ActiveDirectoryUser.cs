using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.ActiveDirectory
{
    public class ActiveDirectoryUser
    {
        public string DomainName { get; set; }

        public DateTime PasswordChangeDate { get; set; }
    }
}