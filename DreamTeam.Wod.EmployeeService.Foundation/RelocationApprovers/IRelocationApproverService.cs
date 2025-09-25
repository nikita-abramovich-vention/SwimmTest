using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common.Observable;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Services.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.DataContracts;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.RelocationApprovers
{
    public interface IRelocationApproverService
    {
        event AsyncObserver<IReadOnlyCollection<RelocationApprover>> PrimaryApproversChanged;

        event AsyncObserver<IReadOnlyCollection<RelocationApprover>> ApproversRemoved;


        Task<bool> CheckIsEmployeeRelocationApproverOrHasAssignedRequestsAsync(string employeeId);

        Task<IReadOnlyCollection<RelocationApprover>> GetAllPrimaryApproversAsync();

        Task<IReadOnlyCollection<RelocationApprover>> GetAsync(string countryId = null, IEmployeeServiceUnitOfWork uow = null);

        Task<IReadOnlyCollection<RelocationApprover>> GetAsync(IReadOnlyCollection<string> countryIds, IEmployeeServiceUnitOfWork uow = null);

        Task<IReadOnlyCollection<RelocationApprover>> GetByEmployeeIdAsync(string employeeId);

        Task<IReadOnlyCollection<RelocationApprover>> GetPrimaryByCountryIdsAsync(IReadOnlyCollection<string> countryIds);

        Task<RelocationApprover> GetPrimaryAsync(string countryId);

        Task<IReadOnlyCollection<RelocationApprover>> UpdateAsync(string countryId, IReadOnlyCollection<RelocationApprover> approvers, string byPersonId);

        Task HandleEmployeeChangedAsync(EmployeeDataContract previousEmployee, EmployeeDataContract employee);

        Task HandleUnitChangedAsync(ServiceEntityChanged<UnitDataContract> unitChanged);

        Task<RelocationApprover> PickApproverAsync(RelocationPlan relocationRequest, IEmployeeServiceUnitOfWork uow);

        RelocationApprover PickUpperApprover(RelocationPlan relocationPlan, IReadOnlyCollection<RelocationApprover> relocationCountryApprovers);

        Task<RelocationApproverAssignmentsProfile> GetApproverAssignmentsProfileAsync(string countryId);
    }
}
