using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class Workplace
    {
        public const int NameMaxLength = 200;


        public int Id { get; set; }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public string SchemeUrl { get; set; }

        public string OfficeId { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public DateTime LastSyncDate { get; set; }

        public int ExternalWorkplaceId { get; set; }

        public ExternalWorkplace ExternalWorkplace { get; set; }
    }
}