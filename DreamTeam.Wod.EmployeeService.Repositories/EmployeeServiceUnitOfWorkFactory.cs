using DreamTeam.Common;
using DreamTeam.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Wod.EmployeeService.Repositories
{
    [UsedImplicitly]
    public sealed class EmployeeServiceUnitOfWorkFactory : IUnitOfWorkFactory<IEmployeeServiceUnitOfWork>, IUnitOfWorkFactory
    {
        private readonly DbContextOptions<EmployeeServiceDbContext> _dbContextOptions;


        public EmployeeServiceUnitOfWorkFactory(DbContextOptions<EmployeeServiceDbContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }


        IUnitOfWork IUnitOfWorkFactory.Create()
        {
            return Create();
        }


        public IEmployeeServiceUnitOfWork Create()
        {
            var dbContext = new EmployeeServiceDbContext(_dbContextOptions);
            var uow = new EmployeeServiceUnitOfWork(dbContext);

            return uow;
        }
    }
}