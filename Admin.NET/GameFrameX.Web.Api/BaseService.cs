using System.Linq.Expressions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity;
using GameFrameX.Web.Api.SqlSugar;

namespace GameFrameX.Web.Api
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseService
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseService<T> : BaseService where T : EntityBase, new()
    {
        protected readonly Repository<T> Repository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public BaseService(Repository<T> settings)
        {
            this.Repository = settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await Repository.AsQueryable().Where(m => m.IsDelete == false).FirstAsync(filter);
        }
    }
}