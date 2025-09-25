using System;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.Internships
{
    public sealed class InternshipChangedEventArgs : EventArgs
    {
        public Internship Internship { get; }

        public Internship PreviousInternship { get; }


        public InternshipChangedEventArgs(Internship internship, Internship previousInternship = null)
        {
            Internship = internship;
            PreviousInternship = previousInternship;
        }
    }
}
