namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class TitleRole
    {
        public const int NameMaxLength = 200;


        public int Id { get; set; }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public bool HasSeniority { get; set; }



        public static class BuiltIn
        {
            public const string Designer = "designer";
            public const string QaEngineer = "qa_engineer";
            public const string QaAutomationEngineer = "qa_automation_engineer";
            public const string SoftwareEngineer = "software_engineer";
            public const string ProjectManager = "project_manager";
            public const string BusinessAnalyst = "business_analyst";
            public const string GroupManager = "group_manager";
            public const string TeamManager = "team_manager";
            public const string DivisionManager = "division_manager";
            public const string DepartmentManager = "department_manager";
            public const string ChiefExecutiveOfficer = "chief_executive_officer";
            public const string ChiefOperationOfficer = "chief_operation_officer";
            public const string ChiefTechnologyOfficer = "chief_technology_officer";
            public const string ChiefInformationOfficer = "chief_information_officer";
            public const string ChiefFinancialOfficer = "chief_financial_officer";
            public const string HrManager = "hr_manager";
            public const string HrDirector = "hr_director";
            public const string DeliveryManager = "delivery_manager";
            public const string Director = "director";
            public const string VicePresidentBusinessDevelopment = "vice_president_business_development";
            public const string BusinessDevelopmentManager = "business_development_manager";
            public const string CorporateDevelopmentManager = "corporate_development_manager";
            public const string MarketingManager = "marketing_manager";
            public const string ToplineController = "topline_controller";
            public const string ReportingController = "reporting_controller";
            public const string FixedCostsController = "fixed_costs_controller";
            public const string HeadOfControlling = "head_of_controlling";
            public const string HeadOfDomainExpertise = "head_of_domain_expertise";
            public const string TechnicalSupportEngineerIt = "technical_support_engineer_it";
            public const string Intern = "intern";
        }
    }
}