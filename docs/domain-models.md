# Domain Model Documentation

This document describes the domain entities in `DreamTeam.Wod.EmployeeService.DomainModel`. For each model the tables list the available fields, their CLR types, and any validation rules or relationships inferred from the code. Optional values are marked by nullable types (e.g. `int?`, `DateOnly?`).

## Employee Lifecycle

### Employee

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary identifier. |
| `ExternalId` | `string` | Identifier from external systems. |
| `PersonId` | `string` | Identifier in the people service. |
| `IsActive` | `bool` | Indicates whether the profile is active. |
| `IsDismissed` | `bool` | Marks employees flagged as dismissed. |
| `DomainName` | `string` | Corporate account name, max length 200 (`DomainNameMaxLength`). |
| `Email` | `string` | Contact email, max length 254 (`EmailMaxLength`). |
| `EmploymentPeriods` | `ICollection<EmploymentPeriod>` | Employment history; at least one active period is expected when the employee is employed. |
| `WageRatePeriods` | `ICollection<WageRatePeriod>` | Rate history for compensation calculations. |
| `UnitId` | `string` | Current business unit. |
| `ResponsibleHrManagerId` | `int?` | Reference to HR manager employee. |
| `ResponsibleHrManager` | `Employee` | Navigation to the HR manager. |
| `MentorId` | `int?` | Reference to mentor employee. |
| `Mentor` | `Employee` | Navigation to the mentor. |
| `EmploymentDate` | `DateOnly` | Employment start date. |
| `DismissalDate` | `DateOnly?` | Actual dismissal date if dismissed. |
| `SeniorityId` | `int?` | Lookup to `Seniority`. |
| `Seniority` | `Seniority` | Navigation to seniority level. |
| `TitleRoleId` | `int?` | Lookup to `TitleRole`. |
| `TitleRole` | `TitleRole` | Navigation to role title. |
| `Roles` | `ICollection<EmployeeRole>` | Assigned functional roles. |
| `CountryId` | `string` | Country code of employment. |
| `OrganizationId` | `string` | Current organization identifier. |
| `EmploymentOfficeId` | `string` | Office identifier provided by HR systems. |
| `CurrentLocationId` | `int?` | Current location record identifier. |
| `CurrentLocation` | `EmployeeCurrentLocation` | Navigation to the latest location binding. |
| `DeactivationReason` | `DeactivationReason?` | Reason why the profile is inactive. |
| `Workplaces` | `ICollection<EmployeeWorkplace>` | Linked workplaces or desks. |
| `EmploymentType` | `EmploymentType` | Contractor/office/remote/hybrid/internship. |
| `CreationDate` | `DateTime` | Audit field from `IHasUpdateInfo`. |
| `UpdatedBy` | `string` | Last editor identifier. |
| `UpdateDate` | `DateTime?` | Last update timestamp. |

`Clone()` creates a shallow copy and duplicates the `Roles` collection list to allow safe modifications.

### EmploymentPeriod

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `EmployeeId` | `int` | Back-reference to the employee. |
| `Employee` | `Employee` | Navigation property. |
| `StartDate` | `DateOnly` | Period start. |
| `EndDate` | `DateOnly?` | Period end; `null` means active. |
| `OrganizationId` | `string` | Organization for the period. |
| `IsInternship` | `bool` | Indicates the period is an internship. |

### WageRatePeriod

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `EmployeeId` | `int` | Employee reference. |
| `Employee` | `Employee` | Navigation property. |
| `StartDate` | `DateOnly` | Rate applicability start. |
| `EndDate` | `DateOnly?` | Rate applicability end. |
| `Rate` | `double` | Wage rate value. |

### DeactivationReason (enum)

- `Dismissed` — employee was dismissed.
- `MaternityLeave` — profile deactivated due to maternity leave.

### EmploymentType (enum)

- `Contractor`
- `Office`
- `Remote`
- `Hybrid`
- `Internship`

### EmployeeSnapshot

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `EmployeeId` | `int` | Employee reference. |
| `Employee` | `Employee` | Navigation property. |
| `FromDate` | `DateOnly` | Snapshot start. |
| `ToDate` | `DateOnly` | Snapshot end. |
| `IsActive` | `bool` | Indicates the snapshot covers an active state. |
| `SeniorityId` | `int?` | Lookup to `Seniority`. |
| `Seniority` | `Seniority` | Navigation to seniority. |
| `TitleRoleId` | `int?` | Lookup to `TitleRole`. |
| `TitleRole` | `TitleRole` | Navigation to title role. |
| `CountryId` | `string` | Country at the time of the snapshot. |
| `OrganizationId` | `string` | Organization at the time of the snapshot. |
| `UnitId` | `string` | Unit at the time of the snapshot. |
| `EmploymentType` | `EmploymentType` | Employment type captured in the snapshot. |

### EmployeeSnapshotLog

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `Date` | `DateTime` | Snapshot run time. |
| `IsSuccessful` | `bool` | Indicates whether the snapshot generation was successful. |

## Locations and Workplaces

### CurrentLocation

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | External system identifier. |
| `Name` | `string` | Location name, max length 200 (`NameMaxLength`). |
| `IsCustom` | `bool` | Marks locations created manually. |
| `HasCompanyOffice` | `bool` | Indicates a company office exists in the location. |
| `IsRelocationDisabled` | `bool` | Blocks relocation to the location when true. |
| `CountryId` | `string` | Country code. |
| `CreatedBy` | `string` | Creation audit field. |
| `CreationDate` | `DateTime` | Creation timestamp. |

### EmployeeCurrentLocation

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `EmployeeId` | `int` | Employee reference. |
| `Employee` | `Employee` | Navigation property. |
| `LocationId` | `int` | Current location reference. |
| `Location` | `CurrentLocation` | Navigation property. |
| `ChangedBy` | `string` | Identifier of the operator who changed the location. |
| `ChangeDate` | `DateTime` | Change timestamp. |
| `SinceDate` | `DateOnly` | Effective date of the location. |
| `UntilDate` | `DateOnly?` | End date if the location is no longer current. |

### EmployeeCurrentLocationChange

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `EmployeeId` | `int` | Employee reference. |
| `Employee` | `Employee` | Navigation property. |
| `PreviousLocationId` | `int?` | Previous location identifier. |
| `PreviousLocation` | `CurrentLocation` | Navigation to previous location. |
| `NewLocationId` | `int?` | New location identifier. |
| `NewLocation` | `CurrentLocation` | Navigation to new location. |
| `UpdatedBy` | `string` | Operator identifier. |
| `UpdateDate` | `DateTime` | Change timestamp. |

### Workplace

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | External workplace identifier. |
| `Name` | `string` | Short workplace name, max length 200 (`NameMaxLength`). |
| `FullName` | `string` | Full descriptive name. |
| `SchemeUrl` | `string` | Link to workplace scheme or map. |
| `OfficeId` | `string` | Office identifier. |
| `CreationDate` | `DateTime` | Creation timestamp. |
| `UpdateDate` | `DateTime?` | Last update timestamp. |
| `LastSyncDate` | `DateTime` | Last synchronization time with external source. |
| `ExternalWorkplaceId` | `int` | Foreign key to `ExternalWorkplace`. |
| `ExternalWorkplace` | `ExternalWorkplace` | Navigation property. |

### EmployeeWorkplace

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `EmployeeId` | `int` | Employee reference. |
| `Employee` | `Employee` | Navigation property. |
| `WorkplaceId` | `int` | Workplace reference. |
| `Workplace` | `Workplace` | Navigation property. |
| `CreationDate` | `DateTime` | Link creation timestamp. |
| `ExternalEmployeeWorkplace` | `ExternalEmployeeWorkplace` | Mirror link from the external system. |

### ExternalWorkplace

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key within the service. |
| `SourceId` | `string` | Identifier in the external workplace registry. |
| `Name` | `string` | Short name, max length 200 (`NameMaxLength`). |
| `FullName` | `string` | Full descriptive name. |
| `SchemeUrl` | `string` | Link provided by the external system. |
| `OfficeSourceId` | `string` | External office identifier. |
| `CreationDate` | `DateTime` | Import timestamp. |
| `UpdateDate` | `DateTime?` | Last synchronization time. |
| `Workplace` | `Workplace` | Navigation back to the internal workplace. |

### ExternalEmployeeWorkplace

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `SourceEmployeeId` | `string` | Employee identifier in the external system. |
| `SourceWorkplaceId` | `string` | Workplace identifier in the external system. |
| `CreationDate` | `DateTime` | Import timestamp. |
| `EmployeeWorkplaceId` | `int?` | Link to the internal `EmployeeWorkplace` record. |
| `EmployeeWorkplace` | `EmployeeWorkplace` | Navigation property. |

## Organizational History

### EmployeeOrganizationChange

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `EmployeeId` | `int` | Employee reference. |
| `Employee` | `Employee` | Navigation property. |
| `PreviousOrganizationId` | `string` | Organization identifier before the change. |
| `NewOrganizationId` | `string` | Organization identifier after the change. |
| `UpdatedBy` | `string` | Operator identifier. |
| `UpdateDate` | `DateTime` | Change timestamp. |

### EmployeeUnitHistory

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | Identifier from external systems. |
| `EmployeeId` | `int` | Employee reference. |
| `Employee` | `Employee` | Navigation property. |
| `UnitId` | `string` | Unit identifier. |
| `StartDate` | `DateOnly` | Assignment start. |
| `EndDate` | `DateOnly?` | Assignment end. |
| `CreationDate` | `DateTime` | Creation timestamp. |
| `UpdateDate` | `DateTime?` | Last update timestamp. |
| `ExternalEmployeeUnitHistory` | `ExternalEmployeeUnitHistory` | Navigation to external history entry. |

`Clone()` returns a shallow copy of the record.

### ExternalEmployeeUnitHistory

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `SourceEmployeeId` | `string` | Employee identifier in the external system. |
| `SourceUnitId` | `string` | Unit identifier in the external system. |
| `StartDate` | `DateOnly` | Assignment start. |
| `EndDate` | `DateOnly?` | Assignment end. |
| `EmployeeUnitHistoryId` | `int?` | Link to the internal history record. |
| `EmployeeUnitHistory` | `EmployeeUnitHistory` | Navigation property. |
| `CreationDate` | `DateTime` | Import timestamp. |
| `UpdateDate` | `DateTime?` | Last synchronization timestamp. |

### UnitInternshipsCount

| Property | Type | Notes |
| --- | --- | --- |
| `UnitId` | `string` | Business unit identifier. |
| `InternshipsCount` | `int` | Number of internships in the unit. |

## Relocation and Compensation

### RelocationPlan

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | Relocation plan identifier in upstream systems. |
| `SourceId` | `string` | Identifier of the source case. |
| `EmployeeId` | `int` | Reference to relocating employee. |
| `Employee` | `Employee` | Navigation to the employee. |
| `LocationId` | `int` | Destination location identifier. |
| `Location` | `CurrentLocation` | Navigation to destination. |
| `EmployeeDate` | `DateOnly` | Employee-provided target date. |
| `EmployeeComment` | `string` | Employee comment, max length 5000 (`CommentMaxLength`). |
| `EmployeeCommentChangeDate` | `DateTime?` | Timestamp of the latest employee comment update. |
| `GmManagerId` | `int?` | Global mobility manager reference. |
| `GmManager` | `Employee` | Navigation to GM manager. |
| `GmComment` | `string` | GM manager comment, max length 5000 (`CommentMaxLength`). |
| `GmCommentChangeDate` | `DateTime?` | Timestamp of the latest GM comment update. |
| `IsInductionPassed` | `bool` | Indicates whether induction is completed. |
| `IsConfirmed` | `bool` | Employee confirmation flag. |
| `ConfirmationDate` | `DateTime?` | Date when the plan was confirmed by the employee. |
| `ApproverComment` | `string` | Approver comment, max length 5000 (`CommentMaxLength`). |
| `ApproverCommentChangeDate` | `DateTime?` | Approver comment change timestamp. |
| `ApproverDate` | `DateOnly?` | Date when approval decision is expected or recorded. |
| `Salary` | `string` | Salary information, max length 1000 (`SalaryMaxLength`). |
| `ApproverId` | `int?` | Reference to approver employee. |
| `Approver` | `Employee` | Navigation to approver. |
| `RelocationUnitId` | `string` | Relocation unit identifier. |
| `IsApproved` | `bool` | Approval flag. |
| `ApprovedBy` | `string` | Identifier of the approver. |
| `ApprovalDate` | `DateTime?` | Actual approval timestamp. |
| `HrManagerId` | `int?` | HR manager reference. |
| `HrManager` | `Employee` | Navigation to HR manager. |
| `HrManagerComment` | `string` | HR manager comment, max length 5000 (`CommentMaxLength`). |
| `HrManagerCommentChangeDate` | `DateTime?` | Timestamp of the latest HR comment update. |
| `HrManagerDate` | `DateOnly?` | HR expected action date. |
| `InductionStatusChangedBy` | `string` | Operator who toggled induction status. |
| `InductionStatusChangeDate` | `DateTime?` | Timestamp of induction status change. |
| `IsEmploymentConfirmedByEmployee` | `bool` | Indicates whether employment is confirmed by the employee. |
| `State` | `RelocationPlanState` | Overall plan state (active/completed/cancelled/rejected). |
| `CurrentStepId` | `RelocationStepId` | Identifier of the active step. |
| `CurrentStep` | `RelocationPlanStep` | Computed navigation to the step matching `CurrentStepId`. |
| `Steps` | `ICollection<RelocationPlanStep>` | Ordered workflow steps for the relocation. |
| `StatusId` | `int` | Reference to the case status. |
| `Status` | `RelocationPlanStatus` | Navigation to status metadata. |
| `Compensation` | `CompensationInfo` | Compensation package offered for relocation. |
| `StatusStartDate` | `DateTime` | Timestamp when the current status started. |
| `StatusDueDate` | `DateTime?` | Expected completion date for the current status. |
| `CloseComment` | `string` | Comment provided on closing the plan. |
| `ClosedBy` | `string` | Identifier of the operator who closed the plan. |
| `CloseDate` | `DateTime?` | Plan closure timestamp. |
| `CreatedBy` | `string` | Creator identifier. |
| `CreationDate` | `DateTime` | Creation timestamp. |
| `UpdatedBy` | `string` | Last editor identifier. |
| `UpdateDate` | `DateTime?` | Last update timestamp. |

**Key behaviours**
- `GetOrderedSteps()` returns steps ordered by `Order`.
- `GetStep(RelocationStepId)` selects a specific step by identifier.
- `InitSteps(IReadOnlyCollection<CountryRelocationStep>)` builds default `Steps` for the plan.
- `SetStep(RelocationStepId, countrySteps, now)` switches the active step, updates completed dates, and recalculates expected dates. It exits early if the requested step is missing from `Steps`.
- `UpdateStepExpectedDates(countrySteps, now)` refreshes expected dates and durations using either provided country steps or the defaults from `CountryRelocationStep.GetDefaultSteps`.
- `MatchStep(RelocationCaseProgress, RelocationPlanStatus)` determines which step matches the external case progress/status.
- `Clone()` returns a shallow copy.

### RelocationPlanStep

| Property | Type | Notes |
| --- | --- | --- |
| `RelocationPlanId` | `int` | Back-reference to the relocation plan. |
| `RelocationPlan` | `RelocationPlan` | Navigation property. |
| `StepId` | `RelocationStepId` | Identifier of the workflow step. |
| `Order` | `int` | Sequential order within the plan. |
| `CompletedAt` | `DateTime?` | Completion timestamp. |
| `IsCompletionDateHidden` | `bool` | Suppresses exposing completion date when true. |
| `DurationInDays` | `int?` | Planned duration; `null` when undefined. |
| `ExpectedAt` | `DateTime?` | Expected completion timestamp. |

### RelocationPlanChange

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `RelocationPlanId` | `int` | Relocation plan reference. |
| `RelocationPlan` | `RelocationPlan` | Navigation property. |
| `EmployeeId` | `int` | Employee reference. |
| `Employee` | `Employee` | Navigation to the employee. |
| `Type` | `RelocationPlanChangeType` | Type of change performed. |
| `PreviousIsInductionPassed` | `bool?` | Previous value of `IsInductionPassed`. |
| `NewIsInductionPassed` | `bool?` | New value of `IsInductionPassed`. |
| `PreviousIsConfirmed` | `bool?` | Previous value of `IsConfirmed`. |
| `NewIsConfirmed` | `bool?` | New value of `IsConfirmed`. |
| `PreviousDestinationId` | `int?` | Previous destination location. |
| `PreviousDestination` | `CurrentLocation` | Navigation to previous destination. |
| `NewDestinationId` | `int?` | New destination location identifier. |
| `NewDestination` | `CurrentLocation` | Navigation to new destination. |
| `PreviousIsApproved` | `bool?` | Previous value of `IsApproved`. |
| `NewIsApproved` | `bool?` | New value of `IsApproved`. |
| `PreviousIsEmploymentConfirmedByEmployee` | `bool?` | Previous employment confirmation flag. |
| `NewIsEmploymentConfirmedByEmployee` | `bool?` | New employment confirmation flag. |
| `PreviousStatusId` | `int?` | Previous status identifier. |
| `PreviousStatus` | `RelocationPlanStatus` | Navigation to previous status. |
| `NewStatusId` | `int?` | New status identifier. |
| `NewStatus` | `RelocationPlanStatus` | Navigation to new status. |
| `UpdatedBy` | `string` | Operator identifier. |
| `UpdateDate` | `DateTime` | Change timestamp. |

### RelocationApprover

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `EmployeeId` | `int` | Employee reference for the approver. |
| `Employee` | `Employee` | Navigation to the approver profile. |
| `CountryId` | `string` | Country whose plans the approver handles. |
| `IsPrimary` | `bool` | Primary approver flag. |
| `ApproverOrderId` | `int?` | Optional reference to an order preference. |
| `ApproverOrder` | `RelocationApproverOrder` | Navigation to order information. |
| `CreatedBy` | `string` | Creator identifier. |
| `CreationDate` | `DateTime` | Creation timestamp. |
| `UpdatedBy` | `string` | Last editor identifier. |
| `UpdateDate` | `DateTime?` | Last update timestamp. |

### RelocationApproverOrder

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `Order` | `int` | Execution order number. |
| `IsNext` | `bool` | Marks the next approver in the sequence. |

### RelocationApproverAssignment

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `RelocationPlanId` | `int` | Relocation plan reference. |
| `RelocationPlan` | `RelocationPlan` | Navigation property. |
| `ApproverId` | `int` | Reference to the assigned approver (`Employee`). |
| `Approver` | `Employee` | Navigation to the approver. |
| `Date` | `DateTime` | Assignment date. |

### CountryRelocationStep

| Property | Type | Notes |
| --- | --- | --- |
| `CountryId` | `string` | Country identifier (can be `null` for defaults). |
| `StepId` | `RelocationStepId` | Step identifier. |
| `DurationInDays` | `int?` | Expected duration in days. |
| `Order` | `int` | Sequence order. |

`GetDefaultSteps(string countryId)` returns the default workflow for a country. It uses hard-coded templates for specific countries (Uzbekistan, Georgia, Kyrgyzstan, Kazakhstan, Poland, Lithuania, Bulgaria, Slovakia, Mexico) and falls back to a generic sequence when no match is found.

### RelocationCaseStatus

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | Identifier used by external relocation case management. |
| `SourceId` | `string` | Source case identifier. |
| `Name` | `string` | Human-readable status name. |
| `CreationDate` | `DateTime` | Import timestamp. |

### RelocationPlanStatus

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | Status identifier shared with external systems. |
| `Name` | `string` | Status display name, max length 100 (`NameMaxLength`). |
| `CaseStatusId` | `int?` | Optional link to `RelocationCaseStatus`. |
| `CaseStatus` | `RelocationCaseStatus` | Navigation property. |

`From(RelocationPlanState state, RelocationPlanStatus status, RelocationStepId stepId)` maps a plan state and step to a pair of status identifiers and originating steps. The nested `BuiltIn` class defines canonical status codes such as `induction`, `pending_approval`, `visa_in_progress`, `rejected`, `completed`, etc., used when synchronising with external workflows.

### RelocationCaseProgress

| Property | Type | Notes |
| --- | --- | --- |
| `VisaProgress` | `RelocationCaseVisaProgress` | Nested progress for visa workflow. |
| `IsTransferBooked` | `bool` | Indicates transfer is booked. |
| `IsAccommodationBooked` | `bool` | Indicates accommodation is booked. |
| `IsVisaGathered` | `bool` | Indicates visa documents have been collected. |
| `TrpState` | `RelocationPlanTrpState?` | Current TRP (temporary residence permit) state if applicable. |

### RelocationCaseVisaProgress

| Property | Type | Notes |
| --- | --- | --- |
| `IsScheduled` | `bool` | Appointment scheduled flag. |
| `AreDocsGathered` | `bool` | Visa documents gathered flag. |
| `AreDocsSentToAgency` | `bool` | Documents sent to the agency. |
| `IsAttended` | `bool` | Embassy appointment attended. |
| `IsPassportCollected` | `bool` | Passport collected. |

### CompensationInfo

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `RelocationPlanId` | `int` | Reference to the associated relocation plan. |
| `RelocationPlan` | `RelocationPlan` | Navigation property. |
| `Total` | `float` | Total compensation amount. |
| `Currency` | `string` | Currency code. |
| `Details` | `CompensationInfoDetails` | Breakdown by beneficiary categories. |
| `PreviousCompensation` | `PreviousCompensationInfo` | Previous compensation data for comparison. |
| `PaidInAdvance` | `bool` | Indicates whether the compensation is paid upfront. |

### CompensationInfoDetails

| Property | Type | Notes |
| --- | --- | --- |
| `Child` | `CompensationInfoDetailsItem` | Compensation for children. |
| `Spouse` | `CompensationInfoDetailsItem` | Compensation for spouse. |
| `Employee` | `CompensationInfoDetailsItem` | Compensation for the employee. |

### CompensationInfoDetailsItem

| Property | Type | Notes |
| --- | --- | --- |
| `Amount` | `float` | Monetary amount for the category. |
| `Enabled` | `bool` | Whether this compensation item is active. |
| `NumberOfPeople` | `int` | Number of people covered by the item. |

### PreviousCompensationInfo

| Property | Type | Notes |
| --- | --- | --- |
| `Amount` | `float` | Previous compensation amount. |
| `Currency` | `string` | Currency code of the previous package. |

### RelocationStepId (enum)

Defines the ordered workflow for relocation cases. Values: `Induction`, `RelocationConfirmation`, `PendingApproval`, `ProcessingQueue`, `VisaDocsPreparation`, `WaitingEmbassyAppointment`, `EmbassyAppointment`, `VisaInProgress`, `VisaDone`, `TrpDocsPreparation`, `TrpDocsTranslationAndLegalization`, `TrpDocsSubmissionToMigrationDirectorate`, `TrpApplicationSubmission`, `TrpInProgress`, `TrpIdCardDocsInProgress`, `EmploymentConfirmation`, `EmploymentInProgress`.

### RelocationPlanState (enum)

- `Active`
- `Completed`
- `Cancelled`
- `Rejected`

### RelocationPlanChangeType (enum)

- `InductionPassed`
- `Confirmed`
- `Status`
- `Destination`
- `Approved`
- `EmploymentConfirmedByEmployee`

### RelocationPlanTrpState (enum)

Represents TRP processing stages: `DocsPreparation`, `DocsTranslationAndLegalization`, `SubmissionToMigrationDirectorate`, `ApplicationSubmission`, `InProgress`, `IdCardDocsInProgress`.

## Employment Requests and Dismissals

### EmploymentRequest

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | Identifier from hiring systems. |
| `SourceId` | `int` | External request source identifier. |
| `SourceEmploymentRequest` | `ExternalEmploymentRequest` | Navigation to raw external data. |
| `EmployeeId` | `int?` | Internal employee identifier if linked. |
| `Employee` | `Employee` | Navigation property. |
| `FirstName` | `string` | Applicant first name, max length 200 (`FirstNameLastNameMaxLength`). |
| `LastName` | `string` | Applicant last name, max length 200. |
| `UnitId` | `string` | Target unit. |
| `Location` | `string` | Textual location, max length 200 (`LocationMaxLength`). |
| `CountryId` | `string` | Target country. |
| `OrganizationId` | `string` | Target organization. |
| `EmploymentDate` | `DateOnly` | Planned employment date. |
| `CreationDate` | `DateTime` | Creation timestamp. |
| `UpdateDate` | `DateTime?` | Last change timestamp. |

### ExternalEmploymentRequest

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `SourceId` | `int` | External source identifier. |
| `Type` | `string` | Request type, max length 50 (`TypeMaxLength`). |
| `StatusId` | `int` | Status identifier in the source system. |
| `StatusName` | `string` | Status name, max length 50 (`StatusNameMaxLength`). |
| `FirstName` | `string` | Candidate first name, max length 200. |
| `LastName` | `string` | Candidate last name, max length 200. |
| `UnitId` | `string` | Target unit identifier. |
| `LocationId` | `int` | Foreign key for location in external system. |
| `Location` | `string` | Location name, max length 200. |
| `OrganizationId` | `string` | Target organization. |
| `EmploymentDate` | `DateOnly` | Planned employment date. |
| `CreationDate` | `DateTime` | Import timestamp. |
| `UpdateDate` | `DateTime?` | Last synchronization timestamp. |
| `CloseDate` | `DateTime?` | External close timestamp. |

### DismissalRequest

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | External dismissal request identifier. |
| `IsActive` | `bool` | Indicates whether the request is still open. |
| `SourceDismissalRequestId` | `int?` | Link to `ExternalDismissalRequest`. |
| `SourceDismissalRequest` | `ExternalDismissalRequest` | Navigation property. |
| `EmployeeId` | `int` | Employee reference. |
| `Employee` | `Employee` | Navigation property. |
| `DismissalDate` | `DateOnly` | Requested dismissal date. |
| `Type` | `DismissalRequestType` | Reason for dismissal. |
| `CloseDate` | `DateTime?` | When the request was closed. |
| `CreationDate` | `DateTime` | Creation timestamp. |
| `UpdateDate` | `DateTime?` | Last update timestamp. |

`Clone()` returns a shallow copy.

### DismissalRequestType (enum)

- `Ordinary`
- `Relocation`
- `ContractChange`
- `MaternityLeave`

### ExternalDismissalRequest

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `SourceId` | `int` | External source identifier. |
| `IsActive` | `bool` | Flag indicating open requests. |
| `SourceEmployeeId` | `string` | Employee identifier in the external system. |
| `DismissalDate` | `DateOnly` | Requested dismissal date. |
| `SourceCreationDate` | `DateTime` | Creation timestamp in the external system. |
| `CloseDate` | `DateTime?` | Closure timestamp. |
| `DismissalSpecificId` | `string` | Additional identifier, max length 200 (`DismissalSpecificIdMaxLength`). |
| `CreationDate` | `DateTime` | Import timestamp. |
| `UpdateDate` | `DateTime?` | Last synchronization timestamp. |

## Roles and Titles

### Role

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | External identifier. |
| `Name` | `string` | Role name, max length 200 (`NameMaxLength`). |
| `Description` | `string` | Description, max length 500 (`DescriptionMaxLength`). |
| `IsBuiltIn` | `bool` | Indicates a built-in system role. |
| `RoleManagerId` | `string` | Identifier of the role manager. |
| `CreatedBy` | `string` | Creator identifier. |
| `CreationDate` | `DateTime` | Creation timestamp. |
| `UpdatedBy` | `string` | Last editor identifier. |
| `UpdateDate` | `DateTime?` | Last update timestamp. |
| `Employees` | `ICollection<EmployeeRole>` | Assigned employees. |

The nested `BuiltIn` class lists predefined role codes such as `hr_manager`, `delivery_manager`, `wod_admin`, etc.

### EmployeeRole

| Property | Type | Notes |
| --- | --- | --- |
| `EmployeeId` | `int` | Employee reference. |
| `Employee` | `Employee` | Navigation property. |
| `RoleId` | `int` | Role reference. |
| `Role` | `Role` | Navigation property. |

### RoleConfiguration

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `Role` | `Role` | Role being configured. |
| `TitleRoles` | `ICollection<RoleConfigurationTitleRole>` | Title-role filters. Empty list + non-empty units implies all title roles are selected. |
| `IsAllTitleRoles` | `bool` | True when configuration applies to all title roles (`TitleRoles.Count == 0` and `Units.Count != 0`). |
| `Units` | `ICollection<RoleConfigurationUnit>` | Unit filters. Empty list + non-empty title roles implies all units are selected. |
| `IsAllUnits` | `bool` | True when configuration applies to all units (`Units.Count == 0` and `TitleRoles.Count != 0`). |
| `Employees` | `ICollection<RoleConfigurationEmployee>` | Explicit employee overrides. |
| `UpdatedBy` | `string` | Last editor identifier. |
| `UpdateDate` | `DateTime?` | Last update timestamp. |

### RoleConfigurationTitleRole

| Property | Type | Notes |
| --- | --- | --- |
| `RoleConfigurationId` | `int` | Configuration reference. |
| `RoleConfiguration` | `RoleConfiguration` | Navigation property. |
| `TitleRoleId` | `int` | Title role reference. |
| `TitleRole` | `TitleRole` | Navigation property. |

### RoleConfigurationUnit

| Property | Type | Notes |
| --- | --- | --- |
| `RoleConfigurationId` | `int` | Configuration reference. |
| `RoleConfiguration` | `RoleConfiguration` | Navigation property. |
| `UnitId` | `string` | Unit identifier. |

### RoleConfigurationEmployee

| Property | Type | Notes |
| --- | --- | --- |
| `RoleConfigurationId` | `int` | Configuration reference. |
| `RoleConfiguration` | `RoleConfiguration` | Navigation property. |
| `EmployeeId` | `int` | Employee reference. |
| `Employee` | `Employee` | Navigation property. |

### TitleRole

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | External identifier. |
| `Name` | `string` | Title name, max length 200 (`NameMaxLength`). |
| `HasSeniority` | `bool` | Indicates whether seniority levels apply. |

The nested `BuiltIn` class lists canonical titles (e.g. `software_engineer`, `project_manager`, `intern`, etc.).

### Seniority

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | External identifier. |
| `Name` | `string` | Seniority name, max length 200 (`MaxNameLength`). |
| `IsHidden` | `bool` | Hides seniority from selection. |
| `Order` | `int` | Ordering weight. |

`Default` references the built-in `middle` level. The nested `BuiltIn` class enumerates default seniorities (`junior`, `middle`, `senior`, `lead`).

## Internships

### Internship

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `ExternalId` | `string` | External internship identifier. |
| `PersonId` | `string` | Person service identifier. |
| `IsActive` | `bool` | Indicates the internship is ongoing. |
| `FirstName` | `string` | Intern first name, max length 200 (`FirstNameLastNameMaxLength`). |
| `LastName` | `string` | Intern last name, max length 200. |
| `FirstNameLocal` | `string` | Localized first name. |
| `LastNameLocal` | `string` | Localized last name. |
| `PhotoId` | `string` | Photo identifier. |
| `Skype` | `string` | Skype handle, max length 256 (`SkypeMaxLength`). |
| `Telegram` | `string` | Telegram handle, max length 32 (`TelegramMaxLength`). |
| `Phone` | `string` | Phone number, max length 64 (`PhoneMaxLength`). |
| `Email` | `string` | Email address, max length 254 (`EmailMaxLength`). |
| `DomainName` | `string` | Corporate domain, regex `[A-z]+\.[A-z]+`, max length 200 (`DomainNameMaxLength`). |
| `IsDomainNameVerified` | `bool` | Domain verification flag. |
| `UnitId` | `string` | Hosting unit. |
| `StudentLabId` | `string` | Student lab identifier. |
| `StudentLabProfileUrl` | `string` | Link to student lab profile. |
| `StartDate` | `DateOnly` | Internship start date. |
| `EndDate` | `DateOnly` | Internship end date. |
| `CloseReason` | `InternshipCloseReason?` | Reason for closing the internship. |
| `Location` | `string` | Location text, max length 100 (`LocationMaxLength`). |
| `CreatedBy` | `string` | Creator identifier. |
| `CreationDate` | `DateTime` | Creation timestamp. |
| `UpdatedBy` | `string` | Last editor identifier. |
| `UpdateDate` | `DateTime?` | Last update timestamp. |

`Clone()` returns a shallow copy of the record.

### InternshipCloseReason (enum)

- `Manually`
- `AutomaticallyDueInactivity`
- `AutomaticallyDueEmployment`

### StudentLabSyncLog

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `SyncStartDate` | `DateTime` | Synchronisation start timestamp. |
| `SyncCompletedDate` | `DateTime` | Synchronisation completion timestamp. |
| `IsSuccessful` | `bool` | Indicates sync success. |
| `IsOutdated` | `bool` | Flag for stale sync results. |
| `AffectedInternshipsCount` | `int` | Number of internships updated. |

## Synchronisation Logs

### SyncLog

| Property | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | Primary key. |
| `Type` | `SyncType` | Type of synchronisation performed. |
| `SyncStartDate` | `DateTime` | Synchronisation start timestamp. |
| `SyncCompletedDate` | `DateTime` | Synchronisation completion timestamp. |
| `IsSuccessful` | `bool` | Indicates whether the sync succeeded. |
| `IsOutdated` | `bool` | Marks the log as obsolete. |
| `AffectedEntitiesCount` | `int` | Number of entities affected. |

### SyncType (enum)

Enumerates supported synchronisation flows: `DownloadExternalWspData`, `LinkEmployeeWorkplaces`, `DownloadExternalEmploymentRequestData`, `LinkEmploymentRequests`, `DownloadExternalDismissalRequestData`, `LinkDismissalRequests`, `DownloadExternalEmployeeUnitHistory`, `LinkEmployeeUnitHistory`.

## Supporting Models

### RelocationCaseProgress & Visa Progress

See the tables above in the relocation section for details on `RelocationCaseProgress` and `RelocationCaseVisaProgress`.

### RelocationApprover Assignment Models

Detailed in the relocation section above (`RelocationApprover`, `RelocationApproverAssignment`, `RelocationApproverOrder`).

### UnitInternshipsCount

Covered in the organizational history section.

### PreviousCompensationInfo and Compensation Details

See the compensation section for breakdown models supporting relocation plans.

