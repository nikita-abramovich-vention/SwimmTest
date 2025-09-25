using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.Microservices
{
    public interface ISmgProfileMapper
    {
        Employee CreateEmployeeFrom(string personId, SmgProfileDataContract smgProfile);

        void UpdateEmployeeFrom(Employee employee, SmgProfileDataContract smgProfile);

        Internship CreateInternshipFrom(PersonDataContract person, SmgInternProfileDataContract smgInternProfile);
    }
}