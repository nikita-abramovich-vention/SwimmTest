using System;
using System.Threading.Tasks;
using DreamTeam.BackgroundJobs;
using DreamTeam.Common.Serializers;
using DreamTeam.Common.Threading;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Core.DepartmentService.Units;
using DreamTeam.Core.ProfileService;
using DreamTeam.Core.ProfileService.Providers;
using DreamTeam.Core.TimeTrackingService;
using DreamTeam.Logging;
using DreamTeam.Logging.Microsoft;
using DreamTeam.Logging.Serilog;
using DreamTeam.Microservices.DependencyInjection;
using DreamTeam.Microservices.DependencyInjection.Options;
using DreamTeam.Migrations.DependencyInjection;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityFramework;
using DreamTeam.Wod.EmployeeService.ConfigurationOptions;
using DreamTeam.Wod.EmployeeService.Configurations;
using DreamTeam.Wod.EmployeeService.Foundation;
using DreamTeam.Wod.EmployeeService.Foundation.ActiveDirectory;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Foundation.Countries;
using DreamTeam.Wod.EmployeeService.Foundation.CurrentLocations;
using DreamTeam.Wod.EmployeeService.Foundation.DismissalRequests;
using DreamTeam.Wod.EmployeeService.Foundation.EmployeeChanges;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.EmployeeUnitHistory;
using DreamTeam.Wod.EmployeeService.Foundation.EmploymentRequests;
using DreamTeam.Wod.EmployeeService.Foundation.Internships;
using DreamTeam.Wod.EmployeeService.Foundation.Microservices;
using DreamTeam.Wod.EmployeeService.Foundation.Offices;
using DreamTeam.Wod.EmployeeService.Foundation.Organizations;
using DreamTeam.Wod.EmployeeService.Foundation.RelocationApprovers;
using DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans;
using DreamTeam.Wod.EmployeeService.Foundation.Roles;
using DreamTeam.Wod.EmployeeService.Foundation.SeniorityManagement;
using DreamTeam.Wod.EmployeeService.Foundation.StudentLabSync;
using DreamTeam.Wod.EmployeeService.Foundation.TitleRoles;
using DreamTeam.Wod.EmployeeService.Foundation.WspSync;
using DreamTeam.Wod.EmployeeService.Repositories;
using DreamTeam.Wod.SmgService;
using DreamTeam.Wod.WspService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ILogger = DreamTeam.Logging.ILogger;

namespace DreamTeam.Wod.EmployeeService
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            var hostBuilder = CreateHostBuilder(args);
            using var host = hostBuilder.Build();
            InitializeContexts(host.Services);
            InitializeDatabase(host.Services);

            await host.RunAsync();
        }


        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(ConfigureLogging)
                .ConfigureServices(ConfigureServices);

            return hostBuilder;
        }

        private static void ConfigureLogging(HostBuilderContext hostBuilderContext, ILoggingBuilder builder)
        {
            builder.ClearProviders();
            builder.AddConfiguration(hostBuilderContext.Configuration.GetSection("Logging"));
            builder.AddSerilogConsole(hostBuilderContext.Configuration.GetSection(SerilogOptions.SectionName));
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            services.Configure<RabbitMqMessageTransportOptions>(hostBuilderContext.Configuration.GetSection(RabbitMqMessageTransportOptions.SectionName));
            services.Configure<EmployeeServiceOptions>(hostBuilderContext.Configuration.GetSection(EmployeeServiceOptions.SectionName));
            services.Configure<ActiveDirectoryAuthenticationOptions>(hostBuilderContext.Configuration.GetSection(ActiveDirectoryAuthenticationOptions.SectionName));
            services.Configure<StudentLabSyncServiceOptions>(hostBuilderContext.Configuration.GetSection(StudentLabSyncServiceOptions.SectionName));
            services.Configure<WspSyncServiceOptions>(hostBuilderContext.Configuration.GetSection(WspSyncServiceOptions.SectionName));
            services.Configure<EmploymentRequestSyncServiceOptions>(hostBuilderContext.Configuration.GetSection(EmploymentRequestSyncServiceOptions.SectionName));
            services.Configure<DismissalRequestSyncServiceOptions>(hostBuilderContext.Configuration.GetSection(DismissalRequestSyncServiceOptions.SectionName));
            services.Configure<EmployeeUnitHistorySyncServiceOptions>(hostBuilderContext.Configuration.GetSection(EmployeeUnitHistorySyncServiceOptions.SectionName));
            services.ConfigureAll<MicroserviceClientOptions>(hostBuilderContext.Configuration.GetSection(MicroserviceClientOptions.SectionName).Bind);

            services.AddSingleton(GetLogger);

            var connectionString = hostBuilderContext.Configuration.GetConnectionString("DefaultConnection");
            services.AddSingleton(sp => new DbContextOptionsBuilder<EmployeeServiceDbContext>()
                .UseApplicationServiceProvider(sp)
                .UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .Options);
            services.AddSingleton<IDatabaseInitializer<EmployeeServiceDbContext>, EmployeeServiceDatabaseInitializer>();

            services.AddUnitOfWorkProvider<IEmployeeServiceUnitOfWork, EmployeeServiceUnitOfWorkFactory>();

            services.AddMicroservices()
                .AddRabbitMqTransport()
                .AddMicroserviceClient<IProfileService>()
                .AddMicroserviceClient<IDepartmentService>()
                .AddMicroserviceClient<ISmgService>()
                .AddMicroserviceClient<ITimeTrackingService>()
                .AddMicroserviceClient<IWspService>()
                .AddMicroservice<IEmployeeMicroservice, EmployeeMicroservice>();

            services.AddMigrations(typeof(EmployeeMicroservice).Assembly)
                .AddEntityFrameworkStore(options => options.UseSqlServer(connectionString));

            services.AddSingleton<Foundation.Employees.EmployeeService>();
            services.AddSingleton<IEmployeeService>(sp => sp.GetRequiredService<Foundation.Employees.EmployeeService>());
            services.AddSingleton<IDisplayManagerProvider, DisplayManagerProvider>();
            services.AddSingleton<ISeniorityService, SeniorityService>();
            services.AddSingleton<IRoleService, RoleService>();
            services.AddSingleton<InternshipService>();
            services.AddSingleton<IInternshipService>(sp => sp.GetRequiredService<InternshipService>());
            services.AddSingleton<IEmployeeServiceConfiguration, EmployeeServiceConfiguration>();
            services.AddSingleton<ICurrentLocationService, CurrentLocationService>();
            services.AddSingleton<RelocationPlanService>();
            services.AddSingleton<IRelocationPlanService>(sp => sp.GetRequiredService<RelocationPlanService>());
            services.AddSingleton<IActiveDirectoryService, ActiveDirectoryService>();
            services.AddSingleton<IDomainNameService, DomainNameService>();
            services.AddSingleton<ITitleRoleService, TitleRoleService>();
            services.AddSingleton<IRelocationApproverService, RelocationApproverService>();
            services.AddSingleton<IEmployeeSnapshotService, EmployeeSnapshotService>();
            services.AddSingleton<ICountryProvider, CountryProvider>();
            services.AddSingleton<IOfficeProvider, OfficeProvider>();
            services.AddSingleton<IUnitProvider, UnitProvider>();
            services.AddSingleton<IOrganizationProvider, OrganizationProvider>();
            services.AddSingleton<IEmployeeLocationProvider, EmployeeLocationProvider>();
            services.AddSingleton<ISmgProfileMapper, SmgProfileMapper>();
            services.AddSingleton<ISmgIdProvider, SmgIdProvider>();

            services.AddSingleton<IStudentLabSyncServiceConfiguration, StudentLabSyncServiceConfiguration>();
            services.AddSingleton<IStudentLabSyncService, StudentLabSyncService>();
            services.AddSingleton<IStudentLabService, StudentLabService>();

            services.AddSingleton<IWspSyncServiceConfiguration, WspSyncServiceConfiguration>();
            services.AddSingleton<IWspSyncService, WspSyncService>();

            services.AddSingleton<IEmploymentRequestSyncServiceConfiguration, EmploymentRequestSyncServiceConfiguration>();
            services.AddSingleton<IEmploymentRequestSyncService, EmploymentRequestSyncService>();
            services.AddSingleton<IEmploymentRequestService, EmploymentRequestService>();

            services.AddSingleton<IDismissalRequestSyncServiceConfiguration, DismissalRequestSyncServiceConfiguration>();
            services.AddSingleton<IDismissalRequestSyncService, DismissalRequestSyncService>();
            services.AddSingleton<IDismissalRequestService, DismissalRequestService>();

            services.AddSingleton<IEmploymentPeriodService, EmploymentPeriodService>();

            services.AddSingleton<IEmployeeChangesService, EmployeeChangesService>();

            services.AddSingleton<IEmployeeUnitHistorySyncServiceConfiguration, EmployeeUnitHistorySyncServiceConfiguration>();
            services.AddSingleton<IEmployeeUnitHistorySyncService, EmployeeUnitHistorySyncService>();
            services.AddSingleton<IEmployeeUnitHistoryService, EmployeeUnitHistoryService>();

            services.AddSingleton<IResourceLockService, ResourceLockService>();

            services.AddSingleton<IWageRatePeriodService, WageRatePeriodService>();

            services.AddBackgroundJobs(hostBuilderContext.Configuration)
                .AddSqlServerStorage(connectionString);

            services.AddHostedService<EmployeeHostedService>();
        }


        private static ILogger GetLogger(IServiceProvider serviceProvider)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("MainLogger");
            var loggerAdapter = new MicrosoftLoggerAdapter(logger);

            return loggerAdapter;
        }

        private static void InitializeContexts(IServiceProvider serviceProvider)
        {
            LoggerContext.Current = serviceProvider.GetRequiredService<ILogger>();
            JsonSerializerContext.Current = serviceProvider.GetRequiredService<IJsonSerializer>();
        }

        private static void InitializeDatabase(IServiceProvider serviceProvider)
        {
            var dbInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer<EmployeeServiceDbContext>>();
            var dbContextOptions = serviceProvider.GetRequiredService<DbContextOptions<EmployeeServiceDbContext>>();
            using (var dbContext = new EmployeeServiceDbContext(dbContextOptions))
            {
                dbInitializer.Initialize(dbContext);
            }
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LoggerContext.Current.LogFatalError("Some unexpected exception has occurred.", (Exception)e.ExceptionObject);
        }
    }
}