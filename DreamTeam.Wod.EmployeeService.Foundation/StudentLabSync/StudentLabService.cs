using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Serializers;
using DreamTeam.Logging;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Foundation.StudentLabSync.Models;

namespace DreamTeam.Wod.EmployeeService.Foundation.StudentLabSync
{
    public sealed class StudentLabService : IStudentLabService
    {
        private const string GetInternshipsMethod = "./internships";
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

        private readonly IStudentLabSyncServiceConfiguration _configuration;
        private readonly IJsonSerializer _jsonSerializer;


        public StudentLabService(
            IStudentLabSyncServiceConfiguration configuration,
            IJsonSerializer jsonSerializer)
        {
            _configuration = configuration;
            _jsonSerializer = jsonSerializer;
        }


        public Task<IReadOnlyCollection<StudentLabInternship>> GetInternshipsAsync(DateTime? changeDate = null)
        {
            return GetAsync<IReadOnlyCollection<StudentLabInternship>>(GetInternshipsMethod, changeDate);
        }


        private async Task<T> GetAsync<T>(string method, DateTime? changeDate = null)
        {
            try
            {
                using (var apiClient = CreateApiClient())
                {
                    if (changeDate.HasValue)
                    {
                        method = $"{method}?changeDate={changeDate.Value}";
                    }
                    var serializedResponse = await apiClient.GetStringAsync(method);
                    var response = _jsonSerializer.Deserialize<T>(serializedResponse);

                    return response;
                }
            }
            catch (Exception ex) when (ex.IsCatchableExceptionType())
            {
                LoggerContext.Current.LogError($"Failed to get data from Student Lab. Method - {method}", ex);

                return default;
            }
        }

        private HttpClient CreateApiClient()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = _configuration.StdLabApiUrl,
                Timeout = Timeout,
            };
            httpClient.DefaultRequestHeaders.Add(_configuration.ApiKeyHeaderName, _configuration.ApiKeyValue);

            return httpClient;
        }
    }
}
