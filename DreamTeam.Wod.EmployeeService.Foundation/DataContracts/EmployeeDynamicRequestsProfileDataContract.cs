using System.Collections.Generic;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class EmployeeDynamicRequestsProfileDataContract
    {
        public IReadOnlyCollection<RelocationPlanInfoDataContract> ApprovedRelocationPlans { get; set; }

        public IReadOnlyCollection<EmploymentRequestDataContract> EmploymentRequests { get; set; }

        public IReadOnlyCollection<DismissalRequestDataContract> DismissalRequests { get; set; }
    }
}
