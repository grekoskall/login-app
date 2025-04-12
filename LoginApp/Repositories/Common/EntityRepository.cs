using LoginApp.Data;
using LoginApp.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace LoginApp.Repositories.Common
{
    public class EntityRepository : IEntityRepository
    {
        private readonly AppDatabaseContext _dbContext;
        private bool _disposed = true;

        public EntityRepository(AppDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ModelIT>> FetchAllAsync<ModelIT>() where ModelIT : ModelBase
        {
            var dbSet = _dbContext.Set<ModelIT>();
            return await dbSet.ToListAsync();
        }

        public async Task<ModelIT?> FetchModelAsync<ModelIT>(ModelIT model) where ModelIT : ModelBase
        {
            var keyProperty = typeof(ModelIT).GetProperties()
                                       .FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyAttribute)));
            var keyValue = keyProperty?.GetValue(model) ?? throw new InvalidOperationException("No key value for model");
            var dbSet = _dbContext.Set<ModelIT>();
            return await dbSet.FindAsync(keyValue);
        }

        public async Task InsertModelAsync<ModelIT>(ModelIT model) where ModelIT : ModelBase
        {
            var dbSet = _dbContext.Set<ModelIT>();
            await dbSet.AddAsync(model);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateModelAsync<ModelIT>(ModelIT model) where ModelIT : ModelBase
        {
            var dbSet = _dbContext.Set<ModelIT>();
            dbSet.Update(model);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertOrUpdateModelAsync<ModelIT>(ModelIT model, Expression<Func<ModelIT, bool>> predicate) where ModelIT : ModelBase
        {
            var dbSet = _dbContext.Set<ModelIT>();

            var existingEntity = await dbSet.FirstOrDefaultAsync(predicate);
            if (existingEntity != null)
            {
                _dbContext.Entry(existingEntity).CurrentValues.SetValues(model);
            }
            else
            {
                await dbSet.AddAsync(model);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteEntityAsync<ModelIT>(ModelIT model) where ModelIT : ModelBase
        {
            var dbSet = _dbContext.Set<ModelIT>();
            var dbModel = await FetchModelAsync(model);
            if (dbModel is not null)
            {
                _dbContext.Entry(dbModel).State = EntityState.Detached;
                dbSet.Remove(model);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<T> GetModelByConditionAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await _dbContext.Set<T>().FirstOrDefaultAsync(predicate);
        }


        public void Dispose()
        {
            if (!_disposed)
            {
                _dbContext.Dispose();
                _disposed = true;
            }
        }

    }
}
