using System;
using System.Linq;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Repositories.EntityFramework;
using DreamTeam.Wod.EmployeeService.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Wod.EmployeeService.Repositories
{
    [UsedImplicitly]
    public sealed class EmployeeServiceDatabaseInitializer : RelationalDatabaseInitializer<EmployeeServiceDbContext>
    {
        protected override void Seed(EmployeeServiceDbContext dbContext)
        {
            SeedRoles(dbContext);
            SeedTitleRoles(dbContext);
        }


        private static void SeedRoles(EmployeeServiceDbContext dbContext)
        {
            var builtInRoles = new[]
            {
                new Role
                {
                    ExternalId = Role.BuiltIn.BusinessDevelopmentManager,
                    Name = "Business development manager",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.CorporateDevelopmentManager,
                    Name = "Corporate development manager",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.MarketingManager,
                    Name = "Marketing manager",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.InternsHrManager,
                    Name = "Interns HR manager",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.HrManager,
                    Name = "HR manager",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.Recruiter,
                    Name = "Recruiter",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.DeliveryManager,
                    Name = "Delivery manager",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.DomainExpert,
                    Name = "Domain expert",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.Chief,
                    Name = "Chief",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.GlobalMobilityManager,
                    Name = "Global mobility manager",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.ItSupport,
                    Name = "IT support",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.Financier,
                    Name = "Financier",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.TechnicalExpert,
                    Name = "Technical expert",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.ProjectIterationMaintainer,
                    Name = "Project iteration maintainer",
                    IsBuiltIn = true,
                },
                new Role
                {
                    ExternalId = Role.BuiltIn.WodAdmin,
                    Name = "WoD Admin",
                    IsBuiltIn = true,
                },
            };

            builtInRoles.ForEach(r => AddOrUpdateRole(dbContext, r));
        }

        private static void AddOrUpdateRole(DbContext dbContext, Role role)
        {
            var roles = dbContext.Set<Role>();
            var existingRole = roles.SingleOrDefault(r => r.ExternalId == role.ExternalId);
            if (existingRole != null)
            {
                existingRole.Name = role.Name;
                existingRole.IsBuiltIn = role.IsBuiltIn;
            }
            else
            {
                role.Description ??= String.Empty;
                roles.Add(role);
            }
        }

        private static void SeedTitleRoles(EmployeeServiceDbContext dbContext)
        {
            var builtInTitleRoles = new[]
            {
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.Designer,
                    Name = "Designer",
                    HasSeniority = true,
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.QaEngineer,
                    Name = "QA Engineer",
                    HasSeniority = true,
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.QaAutomationEngineer,
                    Name = "QA Automation Engineer",
                    HasSeniority = true,
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.SoftwareEngineer,
                    Name = "Software Engineer",
                    HasSeniority = true,
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.ProjectManager,
                    Name = "Project Manager",
                    HasSeniority = true,
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.BusinessAnalyst,
                    Name = "Business Analyst",
                    HasSeniority = true,
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.GroupManager,
                    Name = "Group Manager",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.DivisionManager,
                    Name = "Division Manager",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.DepartmentManager,
                    Name = "Department Manager",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.ChiefExecutiveOfficer,
                    Name = "Chief Executive Officer",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.ChiefOperationOfficer,
                    Name = "Chief Operation Officer",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.ChiefTechnologyOfficer,
                    Name = "Chief Technology Officer",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.ChiefInformationOfficer,
                    Name = "Chief Information Officer",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.ChiefFinancialOfficer,
                    Name = "Chief Financial Officer",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.HrManager,
                    Name = "HR Manager",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.HrDirector,
                    Name = "HR Director",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.DeliveryManager,
                    Name = "Delivery Manager",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.Director,
                    Name = "Director",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.VicePresidentBusinessDevelopment,
                    Name = "Vice President Business Development",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.BusinessDevelopmentManager,
                    Name = "Business Development Manager",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.CorporateDevelopmentManager,
                    Name = "Corporate Development Manager",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.MarketingManager,
                    Name = "Marketing Manager",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.ToplineController,
                    Name = "Topline Controller",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.ReportingController,
                    Name = "Reporting Controller",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.FixedCostsController,
                    Name = "Fixed costs Controller",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.HeadOfControlling,
                    Name = "Head of Controlling",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.HeadOfDomainExpertise,
                    Name = "Head of domain expertise",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.TechnicalSupportEngineerIt,
                    Name = "Technical Support Engineer (IT)",
                },
                new TitleRole
                {
                    ExternalId = TitleRole.BuiltIn.Intern,
                    Name = "Intern",
                },
            };

            builtInTitleRoles.ForEach(r => AddOrUpdateTitleRole(dbContext, r));
        }

        private static void AddOrUpdateTitleRole(DbContext dbContext, TitleRole titleRole)
        {
            var roles = dbContext.Set<TitleRole>();
            var existingRole = roles.SingleOrDefault(r => r.ExternalId == titleRole.ExternalId);
            if (existingRole != null)
            {
                existingRole.Name = titleRole.Name;
                existingRole.HasSeniority = titleRole.HasSeniority;
            }
            else
            {
                roles.Add(titleRole);
            }
        }
    }
}