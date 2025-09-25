using System;
using DreamTeam.Wod.EmployeeService.ConfigurationOptions;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using Microsoft.Extensions.Options;

namespace DreamTeam.Wod.EmployeeService.Configurations
{
    public class StudentLabSyncServiceConfiguration : IStudentLabSyncServiceConfiguration
    {
        private readonly StudentLabSyncServiceOptions _options;


        public bool Enable => _options.Enable;

        public TimeSpan SyncInterval => _options.SyncInterval;

        public Uri StdLabApiUrl => _options.StdLabApiUrl;

        public string ApiKeyHeaderName => _options.ApiKeyHeaderName;

        public string ApiKeyValue => _options.ApiKeyValue;


        public StudentLabSyncServiceConfiguration(IOptions<StudentLabSyncServiceOptions> options)
        {
            _options = options.Value;
        }
    }
}
