namespace DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans
{
    public enum RelocationPlanManagementError
    {
        RelocationPlanAlreadyExists,
        CanNotConfirmRelocationPlanInCurrentState,
        CanNotApproveRelocationPlanInCurrentState,
        CanNotChangeRelocationPlanInductionPassedAfterConfirmation,
        CurrentCountryDoesNotMatchRelocationCountry,
        CanNotConfirmEmploymentByEmployeeInCurrentState,
        CanNotChangeEmploymentDateInCurrentState,
        CanNotSetApproverInCurrentState,
        CanNotSetInactiveApprover,
    }
}
