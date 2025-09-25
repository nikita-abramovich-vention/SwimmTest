using System.Collections.Concurrent;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;

namespace DreamTeam.Wod.EmployeeService.Foundation.Organizations
{
    [UsedImplicitly]
    public sealed class OrganizationProvider : IOrganizationProvider
    {
        private readonly IDepartmentService _departmentService;

        private readonly ConcurrentDictionary<string, OrganizationDataContract> _organizationByImportIdMap;


        public OrganizationProvider(IDepartmentService departmentService)
        {
            _departmentService = departmentService;

            _organizationByImportIdMap = new ConcurrentDictionary<string, OrganizationDataContract>();
        }


        public async Task InitializeAsync(IWodObservable wodObservable)
        {
            var organizations = await _departmentService.GetOrganizationsAsync();
            organizations.ForEach(AddOrUpdateOrganization);
            wodObservable.OrganizationChanged += OnOrganizationChanged;
        }

        public OrganizationDataContract GetOrganizationByImportId(string importId)
        {
            return _organizationByImportIdMap[importId];
        }


        private Task OnOrganizationChanged(OrganizationDataContract organization)
        {
            AddOrUpdateOrganization(organization);

            return Task.CompletedTask;
        }

        private void AddOrUpdateOrganization(OrganizationDataContract organization)
        {
            _organizationByImportIdMap[organization.ImportId] = organization;
        }
    }
}