using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnitTestExampleMVC.Web.Models;

namespace UnitTestExampleMVC.Web.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly MVCUnitTestDBContext _dbContext;
        private readonly DbSet<TEntity> _entity;

        public Repository(MVCUnitTestDBContext dbContext)
        {
            _dbContext = dbContext;
            _entity = _dbContext.Set<TEntity>();
        }

        public async Task Create(TEntity entity)
        {
            await _dbContext.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public void Delete(TEntity entity)
        {
            _entity.Remove(entity);
            _dbContext.SaveChanges();
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await _entity.ToListAsync();
        }

        public async Task<TEntity> GetById(int id)
        {
            return await _entity.FindAsync(id);
        }

        public void Update(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;

            // _entity.Update(entity);

            _dbContext.SaveChanges();
        }
    }
}
