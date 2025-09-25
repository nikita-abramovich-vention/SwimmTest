using System;
using System.Linq;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.Microservices
{
    public static class SmgRankMapper
    {
        public static string MapToSeniorityId(string smgProfileRank)
        {
            if (smgProfileRank == null)
            {
                return null;
            }

            var trimmedRank = smgProfileRank.TrimEnd();
            switch (trimmedRank)
            {
                case "Software Engineer":
                case "Quality Assurance Engineer":
                case "Project Manager":
                case "Business Analyst":
                case "QA Automation Engineer":
                    return Seniority.BuiltIn.Middle;
                case "Senior Software Engineer":
                case "Senior Quality Assurance Engineer":
                case "Senior QA Automation Engineer":
                    return Seniority.BuiltIn.Senior;
                case "Lead Software Engineer":
                case "Lead Quality Assurance Engineer":
                case "Lead QA Automation Engineer":
                    return Seniority.BuiltIn.Lead;
                default:
                    return null;
            }
        }

        public static string MapToTitleRoleId(string smgProfileRank)
        {
            if (smgProfileRank == null)
            {
                return null;
            }

            var trimmedRank = smgProfileRank.TrimEnd();
            switch (trimmedRank)
            {
                case "Designer":
                    return TitleRole.BuiltIn.Designer;
                case "Quality Assurance Engineer":
                case "Senior Quality Assurance Engineer":
                case "Lead Quality Assurance Engineer":
                    return TitleRole.BuiltIn.QaEngineer;
                case "QA Automation Engineer":
                case "Senior QA Automation Engineer":
                case "Lead QA Automation Engineer":
                    return TitleRole.BuiltIn.QaAutomationEngineer;
                case "Software Engineer":
                case "Senior Software Engineer":
                case "Lead Software Engineer":
                    return TitleRole.BuiltIn.SoftwareEngineer;
                case "Project Manager":
                    return TitleRole.BuiltIn.ProjectManager;
                case "Business Analyst":
                    return TitleRole.BuiltIn.BusinessAnalyst;
                case "Group Manager":
                    return TitleRole.BuiltIn.GroupManager;
                case "Team Manager":
                    return TitleRole.BuiltIn.TeamManager;
                case "Division Manager":
                    return TitleRole.BuiltIn.DivisionManager;
                case "Department Manager":
                    return TitleRole.BuiltIn.DepartmentManager;
                case "Chief Executive Officer":
                    return TitleRole.BuiltIn.ChiefExecutiveOfficer;
                case "Chief Operation Officer":
                    return TitleRole.BuiltIn.ChiefOperationOfficer;
                case "Chief Technology Officer":
                    return TitleRole.BuiltIn.ChiefTechnologyOfficer;
                case "Chief Information Officer":
                    return TitleRole.BuiltIn.ChiefInformationOfficer;
                case "Chief Financial Officer":
                    return TitleRole.BuiltIn.ChiefFinancialOfficer;
                case "HR Manager":
                    return TitleRole.BuiltIn.HrManager;
                case "HR Director":
                    return TitleRole.BuiltIn.HrDirector;
                case "Delivery Manager":
                    return TitleRole.BuiltIn.DeliveryManager;
                case "Director":
                    return TitleRole.BuiltIn.Director;
                case "Vice President Business Development":
                    return TitleRole.BuiltIn.VicePresidentBusinessDevelopment;
                case "Business Development Manager":
                    return TitleRole.BuiltIn.BusinessDevelopmentManager;
                case "Corporate Development Manager":
                    return TitleRole.BuiltIn.CorporateDevelopmentManager;
                case "Marketing Manager":
                    return TitleRole.BuiltIn.MarketingManager;
                case "Topline Controller":
                    return TitleRole.BuiltIn.ToplineController;
                case "Reporting Controller":
                    return TitleRole.BuiltIn.ReportingController;
                case "Fixed costs Controller":
                    return TitleRole.BuiltIn.FixedCostsController;
                case "Head of Controlling":
                    return TitleRole.BuiltIn.HeadOfControlling;
                case "Head of domain expertise":
                    return TitleRole.BuiltIn.HeadOfDomainExpertise;
                default:
                    return new String(trimmedRank.Select(ch => Char.IsLetterOrDigit(ch) ? ch : '_').ToArray()).ToLowerInvariant().Trim('_').Replace("__", "_").Replace("__", "_");
            }
        }
    }
}