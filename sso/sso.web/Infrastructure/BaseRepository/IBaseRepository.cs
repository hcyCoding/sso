using sso.web.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sso.web.Infrastructure.BaseRepository
{
    /// <summary>
    /// 数据仓储基础契约
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBaseRepository<T> where T : BaseEntity
    {
        #region  同步

        /// <summary>
        /// 新增实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Insert(T model);

        /// <summary>
        /// 新增实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Insert(List<T> models);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Remove(string Id);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Update(T model);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Update(List<T> models);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        T Get(object whereParameters = null, List<string> columnParameters = null);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="dyOrder">排序</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        List<T> GetList(object whereParameters = null, string dyOrder = null, List<string> columnParameters = null);

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="pageSize">每一页数据量</param>
        /// <param name="currentPage">当前页</param>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="dyOrder">排序</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        PageModels<T> GetByPager(int pageSize, int currentPage, object whereParameters = null, string dyOrder = null,
            List<string> columnParameters = null);

        #endregion

        #region  异步

        /// <summary>
        /// 新增实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> InsertAsync(T model);

        /// <summary>
        /// 新增实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> InsertAsync(List<T> models);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> RemoveAsync(string Id);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(T model);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(List<T> models);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        Task<T> GetAsync(object whereParameters = null, List<string> columnParameters = null);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="dyOrder">排序</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        Task<List<T>> GetListAsync(object whereParameters = null, string dyOrder = null, List<string> columnParameters = null);

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="pageSize">每一页数据量</param>
        /// <param name="currentPage">当前页</param>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="dyOrder">排序</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        Task<PageModels<T>> GetByPagerAsync(int pageSize, int currentPage, object whereParameters = null, string dyOrder = null,
            List<string> columnParameters = null);

        #endregion
    }
}
