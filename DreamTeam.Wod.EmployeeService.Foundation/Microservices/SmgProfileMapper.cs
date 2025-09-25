using System.Linq;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Offices;
namespace DreamTeam.Wod.EmployeeService.Foundation.Microservices
{
    [UsedImplicitly]
    public sealed class SmgProfileMapper : ISmgProfileMapper
    {
        private readonly IOfficeProvider _officeProvider;


        public SmgProfileMapper(IOfficeProvider officeProvider)
        {
            _officeProvider = officeProvider;
        }


        public Employee CreateEmployeeFrom(string personId, SmgProfileDataContract smgProfile)
        {
            var employmentPeriods = smgProfile.EmploymentPeriods?.Select(CreateFrom).ToList() ?? [];
            var wageRatePeriods = smgProfile.WageRatePeriods?.Select(CreateFrom).ToList() ?? [];
            var employee = new Employee
            {
                ExternalId = smgProfile.Id,
                PersonId = personId,
                Roles = [],
                Workplaces = [],
                EmploymentPeriods = employmentPeriods,
                WageRatePeriods = wageRatePeriods,
            };
            UpdateEmployeeFrom(employee, smgProfile);

            return employee;
        }

        public void UpdateEmployeeFrom(Employee employee, SmgProfileDataContract smgProfile)
        {
            employee.IsActive = smgProfile.IsActive;
            employee.IsDismissed = smgProfile.IsDismissed;
            employee.DomainName = smgProfile.DomainName;
            employee.Email = smgProfile.Email;
            employee.EmploymentDate = smgProfile.EmploymentDate.ToDateOnly();
            employee.DismissalDate = smgProfile.DismissalDate?.ToDateOnly();
            employee.EmploymentType = EmploymentTypeCreator.CreateFrom(smgProfile.EmploymentType);

            var office = smgProfile.EmploymentOfficeId == null ? null : _officeProvider.GetOfficeByImportId(smgProfile.EmploymentOfficeId);
            employee.EmploymentOfficeId = office?.Id;
        }

        public Internship CreateInternshipFrom(PersonDataContract person, SmgInternProfileDataContract smgInternProfile)
        {
            var internship = new Internship
            {
                ExternalId = smgInternProfile.Id,
                PersonId = person.Id,
            };
            UpdateInternshipFrom(internship, smgInternProfile);

            return internship;
        }


        private static void UpdateInternshipFrom(Internship internship, SmgInternProfileDataContract smgInternProfile)
        {
            internship.FirstName = smgInternProfile.FirstName;
            internship.FirstNameLocal = smgInternProfile.FirstNameLocal;
            internship.LastName = smgInternProfile.LastName;
            internship.LastNameLocal = smgInternProfile.LastNameLocal;
            internship.Phone = smgInternProfile.Phone;
            internship.IsActive = smgInternProfile.IsActive;
            internship.StartDate = smgInternProfile.StartDate.ToDateOnly();
            internship.Location = smgInternProfile.Room;
        }

        private static EmploymentPeriod CreateFrom(EmploymentPeriodDataContract employmentPeriod)
        {
            return new EmploymentPeriod
            {
                StartDate = employmentPeriod.StartDate.ToDateOnly(),
                EndDate = employmentPeriod.EndDate?.ToDateOnly(),
                OrganizationId = employmentPeriod.OrganizationId,
                IsInternship = employmentPeriod.IsInternship,
            };
        }

        private static WageRatePeriod CreateFrom(WageRatePeriodDataContract wageRatePeriod)
        {
            return new WageRatePeriod
            {
                StartDate = wageRatePeriod.StartDate.ToDateOnly(),
                EndDate = wageRatePeriod.EndDate?.ToDateOnly(),
                Rate = wageRatePeriod.Rate,
            };
        }
    }
}