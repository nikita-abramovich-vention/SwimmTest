using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Services.DataContracts;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;

namespace DreamTeam.Wod.EmployeeService.Foundation.Offices
{
    [UsedImplicitly]
    public sealed class OfficeProvider : IOfficeProvider
    {
        private readonly IDepartmentService _departmentService;

        private readonly ConcurrentDictionary<string, OfficeDataContract> _officeMap;
        private readonly ConcurrentDictionary<string, OfficeDataContract> _officeByImportIdMap;


        public OfficeProvider(IDepartmentService departmentService)
        {
            _departmentService = departmentService;

            _officeMap = new ConcurrentDictionary<string, OfficeDataContract>();
            _officeByImportIdMap = new ConcurrentDictionary<string, OfficeDataContract>();
        }


        public async Task InitializeAsync(IWodObservable wodObservable)
        {
            var offices = await _departmentService.GetOfficesAsync();
            offices.ForEach(AddOrUpdateOffice);
            wodObservable.OfficeCreated += OnOfficeChangedAsync;
            wodObservable.OfficeUpdated += OnOfficeChangedAsync;
        }

        public OfficeDataContract GetOffice(string officeId)
        {
            return _officeMap.GetValueOrDefault(officeId);
        }

        public OfficeDataContract GetOfficeByImportId(string importId)
        {
            return _officeByImportIdMap.GetValueOrDefault(importId);
        }


        private void AddOrUpdateOffice(OfficeDataContract office)
        {
            _officeMap[office.Id] = office;
            _officeByImportIdMap[office.ImportId] = office;
        }

        private Task OnOfficeChangedAsync(ServiceEntityChanged<OfficeDataContract> officeChanged)
        {
            var office = officeChanged.NewValue;
            AddOrUpdateOffice(office);

            return Task.CompletedTask;
        }
    }
}