using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;

namespace DreamTeam.Wod.EmployeeService.Foundation.Countries
{
    [UsedImplicitly]
    public sealed class CountryProvider : ICountryProvider
    {
        private readonly IDepartmentService _departmentService;

        private readonly ConcurrentDictionary<string, CountryDataContract> _countryByImportIdMap;
        private readonly ConcurrentDictionary<string, CountryDataContract> _countryMap;


        public CountryProvider(IDepartmentService departmentService)
        {
            _departmentService = departmentService;

            _countryByImportIdMap = new ConcurrentDictionary<string, CountryDataContract>();
            _countryMap = new ConcurrentDictionary<string, CountryDataContract>();
        }


        public async Task InitializeAsync(IWodObservable wodObservable)
        {
            var countries = await _departmentService.GetCountriesAsync();
            countries.ForEach(AddOrUpdateCountry);
            wodObservable.CountryChanged += OnCountryChanged;
        }

        public bool TryGetCountry(string countryId, out CountryDataContract country)
        {
            return _countryMap.TryGetValue(countryId, out country);
        }

        public CountryDataContract GetCountryByImportId(string importId)
        {
            return _countryByImportIdMap[importId];
        }


        private void AddOrUpdateCountry(CountryDataContract country)
        {
            if (!String.IsNullOrEmpty(country.ImportId))
            {
                _countryByImportIdMap[country.ImportId] = country;
            }
            _countryMap[country.Id] = country;
        }

        private Task OnCountryChanged(CountryDataContract country)
        {
            AddOrUpdateCountry(country);

            return Task.CompletedTask;
        }
    }
}