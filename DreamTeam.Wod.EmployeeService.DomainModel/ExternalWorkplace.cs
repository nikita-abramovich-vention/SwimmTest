using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class ExternalWorkplace
    {
        public const int NameMaxLength = 200;


        public int Id { get; set; }

        public string SourceId { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public string SchemeUrl { get; set; }

        public string OfficeSourceId { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public Workplace Workplace { get; set; }
    }
}