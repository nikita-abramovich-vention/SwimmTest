using System;
using DreamTeam.Common;
using DreamTeam.Wod.EmployeeService.ConfigurationOptions;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using Microsoft.Extensions.Options;

namespace DreamTeam.Wod.EmployeeService.Configurations
{
    [UsedImplicitly]
    public sealed class EmployeeServiceConfiguration : IEmployeeServiceConfiguration
    {
        private readonly EmployeeServiceOptions _options;


        public int AutomaticallyCloseInternshipInDays => _options.AutomaticallyCloseInternshipInDays;

        public int AutomaticallyCloseInternshipFromEmploymentInDays => _options.AutomaticallyCloseInternshipFromEmploymentInDays;

        public string CloseUnconfirmedRelocationPlansCron => _options.CloseUnconfirmedRelocationPlansCron;

        public TimeSpan InductionValidityPeriod => _options.InductionValidityPeriod;

        public bool IgnoreInvalidEmployeesOnInitialImport => _options.IgnoreInvalidEmployeesOnInitialImport;

        public string InternEmailTemplate => _options.InternEmailTemplate;


        public EmployeeServiceConfiguration(IOptions<EmployeeServiceOptions> options)
        {
            _options = options.Value;
        }
    }
}
