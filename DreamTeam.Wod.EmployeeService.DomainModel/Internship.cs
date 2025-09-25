using System;
using DreamTeam.DomainModel;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class Internship : IHasCreateUpdateInfo
    {
        public const int DomainNameMaxLength = 200;
        public const string DomainNameRegex = "[A-z]+\\.[A-z]+";
        public const int FirstNameLastNameMaxLength = 200;
        public const int SkypeMaxLength = 256;
        public const int PhoneMaxLength = 64;
        public const int EmailMaxLength = 254;
        public const int TelegramMaxLength = 32;
        public const int LocationMaxLength = 100;


        public int Id { get; set; }

        public string ExternalId { get; set; }

        public string PersonId { get; set; }

        public bool IsActive { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FirstNameLocal { get; set; }

        public string LastNameLocal { get; set; }

        public string PhotoId { get; set; }

        public string Skype { get; set; }

        public string Telegram { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string DomainName { get; set; }

        public bool IsDomainNameVerified { get; set; }

        public string UnitId { get; set; }

        public string StudentLabId { get; set; }

        public string StudentLabProfileUrl { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public InternshipCloseReason? CloseReason { get; set; }

        public string Location { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }


        public Internship Clone()
        {
            return (Internship)MemberwiseClone();
        }
    }
}