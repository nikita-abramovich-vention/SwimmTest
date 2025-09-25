using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.StudentLabSync.Models
{
    public class StudentLabInternship
    {
        public string Id { get; set; }

        public string WodInternshipId { get; set; }

        public bool HasStLabActivities { get; set; }

        public string ProfileUrl { get; set; }

        public DateTime ChangeDate { get; set; }
    }
}
