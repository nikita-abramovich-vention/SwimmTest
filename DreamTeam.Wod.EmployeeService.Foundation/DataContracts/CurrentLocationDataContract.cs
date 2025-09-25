using System;
using DreamTeam.Microservices.DataContracts;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class CurrentLocationDataContract : IHasCreateInfoDataContract
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsCustom { get; set; }

        public bool HasCompanyOffice { get; set; }

        public bool IsRelocationDisabled { get; set; }

        public string CountryId { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }
    }
}