using System;
using System.Collections.Generic;
using DreamTeam.Wod.EmployeeService.ConfigurationOptions;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using Microsoft.Extensions.Options;

namespace DreamTeam.Wod.EmployeeService.Configurations
{
    public sealed class DismissalRequestSyncServiceConfiguration : IDismissalRequestSyncServiceConfiguration
    {
        private readonly DismissalRequestSyncServiceOptions _options;


        public bool Enable => _options.Enable;

        public TimeSpan SyncInterval => _options.SyncInterval;

        public IReadOnlyCollection<string> EmployeeIdsToSync { get; }


        public DismissalRequestSyncServiceConfiguration(IOptions<DismissalRequestSyncServiceOptions> options)
        {
            _options = options.Value;

            EmployeeIdsToSync = !String.IsNullOrEmpty(_options.EmployeeIdsToSync)
                ? _options.EmployeeIdsToSync.Split('|')
                : null;
        }
    }
}