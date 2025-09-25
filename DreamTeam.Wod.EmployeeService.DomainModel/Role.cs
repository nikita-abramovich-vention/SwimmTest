using System;
using System.Collections.Generic;
using DreamTeam.DomainModel;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class Role : IHasCreateUpdateInfo
    {
        public const int NameMaxLength = 200;
        public const int DescriptionMaxLength = 500;

        public int Id { get; set; }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsBuiltIn { get; set; }

        public string RoleManagerId { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }

        public ICollection<EmployeeRole> Employees { get; set; }


        public static class BuiltIn
        {
            public const string BusinessDevelopmentManager = "business_development_manager";
            public const string MarketingManager = "marketing_manager";
            public const string InternsHrManager = "interns_hr_manager";
            public const string HrManager = "hr_manager";
            public const string Recruiter = "recruiter";
            public const string DeliveryManager = "delivery_manager";
            public const string DomainExpert = "domain_expert";
            public const string Chief = "chief";
            public const string GlobalMobilityManager = "global_mobility_manager";
            public const string ItSupport = "it_support";
            public const string Financier = "financier";
            public const string TechnicalExpert = "technical_expert";
            public const string CorporateDevelopmentManager = "corporate_development_manager";
            public const string ProjectIterationMaintainer = "project_iteration_maintainer";
            public const string WodAdmin = "wod_admin";
        }
    }
}