using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.Configuration
{
    public interface IEmployeeServiceConfiguration
    {
        int AutomaticallyCloseInternshipInDays { get; }

        int AutomaticallyCloseInternshipFromEmploymentInDays { get; }

        string CloseUnconfirmedRelocationPlansCron { get; }

        TimeSpan InductionValidityPeriod { get; }

        bool IgnoreInvalidEmployeesOnInitialImport { get; }

        string InternEmailTemplate { get; }
    }
}
