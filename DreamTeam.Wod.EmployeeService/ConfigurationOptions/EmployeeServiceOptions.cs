using System;

namespace DreamTeam.Wod.EmployeeService.ConfigurationOptions
{
    public sealed class EmployeeServiceOptions
    {
        public const string SectionName = "EmployeeService";


        public string MqQueueName { get; set; }

        public int AutomaticallyCloseInternshipInDays { get; set; }

        public int AutomaticallyCloseInternshipFromEmploymentInDays { get; set; }

        public string CloseUnconfirmedRelocationPlansCron { get; set; }

        public TimeSpan InductionValidityPeriod { get; set; }

        public bool IgnoreInvalidEmployeesOnInitialImport { get; set; }

        public string InternEmailTemplate { get; set; }
    }
}