using System;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.Microservices
{
    public static class EmploymentTypeCreator
    {
        public static EmploymentType CreateFrom(Core.ProfileService.DataContracts.EmploymentType employmentType)
        {
            return employmentType switch
            {
                Core.ProfileService.DataContracts.EmploymentType.Contractor => EmploymentType.Contractor,
                Core.ProfileService.DataContracts.EmploymentType.Office => EmploymentType.Office,
                Core.ProfileService.DataContracts.EmploymentType.Remote => EmploymentType.Remote,
                Core.ProfileService.DataContracts.EmploymentType.Hybrid => EmploymentType.Hybrid,
                Core.ProfileService.DataContracts.EmploymentType.Internship => EmploymentType.Internship,
                _ => throw new ArgumentOutOfRangeException(nameof(employmentType)),
            };
        }

        public static Core.ProfileService.DataContracts.EmploymentType CreateFrom(EmploymentType employmentType)
        {
            return employmentType switch
            {
                EmploymentType.Contractor => Core.ProfileService.DataContracts.EmploymentType.Contractor,
                EmploymentType.Office => Core.ProfileService.DataContracts.EmploymentType.Office,
                EmploymentType.Remote => Core.ProfileService.DataContracts.EmploymentType.Remote,
                EmploymentType.Hybrid => Core.ProfileService.DataContracts.EmploymentType.Hybrid,
                EmploymentType.Internship => Core.ProfileService.DataContracts.EmploymentType.Internship,
                _ => throw new ArgumentOutOfRangeException(nameof(employmentType)),
            };
        }
    }
}