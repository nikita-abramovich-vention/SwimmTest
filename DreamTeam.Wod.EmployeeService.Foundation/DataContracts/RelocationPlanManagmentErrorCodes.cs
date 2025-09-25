namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public static class RelocationPlanManagementErrorCodes
    {
        public const string RelocationPlanAlreadyExists = "RelocationPlanAlreadyExists";
        public const string CanNotConfirmRelocationPlanInCurrentState = "CanNotConfirmRelocationPlanInCurrentState";
        public const string CanNotApproveRelocationPlanInCurrentState = "CanNotApproveRelocationPlanInCurrentState";
        public const string CanNotChangeRelocationPlanInductionPassedAfterConfirmation = "CanNotChangeRelocationPlanInductionPassedAfterConfirmation";
        public const string CurrentCountryDoesNotMatchRelocationCountry = "CurrentCountryDoesNotMatchRelocationCountry";
        public const string CanNotConfirmEmploymentByEmployeeInCurrentState = "CanNotConfirmEmploymentByEmployeeInCurrentState";
        public const string CanNotChangeEmploymentDateInCurrentState = "CanNotChangeEmploymentDateInCurrentState";
        public const string CanNotSetApproverInCurrentState = "CanNotSetApproverInCurrentState";
        public const string CanNotSetInactiveApprover = "CanNotSetInactiveApprover";
    }
}
