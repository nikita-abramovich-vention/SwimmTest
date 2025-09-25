using System;
using DreamTeam.Wod.EmployeeService.ConfigurationOptions;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using Microsoft.Extensions.Options;

namespace DreamTeam.Wod.EmployeeService.Configurations
{
    public sealed class EmploymentRequestSyncServiceConfiguration : IEmploymentRequestSyncServiceConfiguration
    {
        private readonly EmploymentRequestSyncServiceOptions _options;


        public bool Enable => _options.Enable;

        public TimeSpan SyncInterval => _options.SyncInterval;


        public EmploymentRequestSyncServiceConfiguration(IOptions<EmploymentRequestSyncServiceOptions> options)
        {
            _options = options.Value;
        }
    }
}
