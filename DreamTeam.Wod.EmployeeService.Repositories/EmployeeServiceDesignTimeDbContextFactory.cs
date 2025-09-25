using DreamTeam.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DreamTeam.Wod.EmployeeService.Repositories
{
    [UsedImplicitly]
    public sealed class EmployeeServiceDesignTimeDbContextFactory : IDesignTimeDbContextFactory<EmployeeServiceDbContext>
    {
        private const string DesignTimeConnectionString = "Server=(localdb)\\mssqllocaldb;Database=WodEmployees;Trusted_Connection=True;MultipleActiveResultSets=true";


        public EmployeeServiceDbContext CreateDbContext(string[] args)
        {
            var dbContextOptions = new DbContextOptionsBuilder<EmployeeServiceDbContext>().UseSqlServer(DesignTimeConnectionString).Options;

            return new EmployeeServiceDbContext(dbContextOptions);
        }
    }
}