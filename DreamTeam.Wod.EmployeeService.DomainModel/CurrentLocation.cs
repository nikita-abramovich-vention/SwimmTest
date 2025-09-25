using System;
using DreamTeam.DomainModel;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class CurrentLocation : IHasCreateInfo
    {
        public const int NameMaxLength = 200;


        public int Id { get; set; }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public bool IsCustom { get; set; }

        public bool HasCompanyOffice { get; set; }

        public bool IsRelocationDisabled { get; set; }

        public string CountryId { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }
    }
}