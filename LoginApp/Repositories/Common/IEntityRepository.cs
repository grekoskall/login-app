using LoginApp.Models;
using System.Linq.Expressions;

namespace LoginApp.Repositories.Common
{
    public interface IEntityRepository : IDisposable
    {
        public Task<IEnumerable<ModelIT>> FetchAllAsync<ModelIT>() where ModelIT : ModelBase;
        public Task<ModelIT?> FetchModelAsync<ModelIT>(ModelIT model) where ModelIT : ModelBase;
        public Task InsertModelAsync<ModelIT>(ModelIT model) where ModelIT : ModelBase;
        public Task UpdateModelAsync<ModelIT>(ModelIT model) where ModelIT : ModelBase;
        public Task InsertOrUpdateModelAsync<ModelIT>(ModelIT model, Expression<Func<ModelIT, bool>> predicate) where ModelIT : ModelBase;
        public Task DeleteEntityAsync<ModelIT>(ModelIT model) where ModelIT : ModelBase;
        public Task<T> GetModelByConditionAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

    }
}
