using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using sso.web.Core.Models;
using sso.web.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace sso.web.Infrastructure.BaseRepository
{
    /// <summary>
    /// 数据仓储基础实现
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        public string connectionStr { get; protected set; }

        public BaseRepository()
        {
            connectionStr = ConfigurationManager.Configuration.GetConnectionString("MysqlConnection");
        }

        public BaseRepository(string connection)
        {
            connectionStr = ConfigurationManager.Configuration.GetConnectionString(connection);
        }

        #region  异步

        /// <summary>
        /// 新增实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(T model)
        {
            bool result = false;
            if (model == null) throw new ArgumentNullException("model");

            PropertyInfo[] properties = model.GetType().GetProperties();
            string propertiesStr = string.Join(",", properties.Select(p => p.Name));
            string valueStr = string.Join(",", properties.Select(p => $"@{p.Name}"));
            string stringSql = $"insert into {typeof(T).Name} ({propertiesStr}) values ({valueStr})";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                int count = connection.Execute(stringSql, model);
                if (count > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 新增实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(List<T> models)
        {
            bool result = false;

            if (models == null) throw new ArgumentNullException("models");

            var model = models[0];
            PropertyInfo[] properties = model.GetType().GetProperties();
            string propertiesStr = string.Join(",", properties.Select(p => p.Name));
            string valueStr = string.Join(",", properties.Select(p => $"@{p.Name}"));
            string stringSql = $"insert into {typeof(T).Name} ({propertiesStr}) values ({valueStr})";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                int count = connection.Execute(stringSql, models);
                if (count > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Remove(string Id)
        {
            bool result = false;
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException("Id");

            string stringSql = $"update {typeof(T).Name} set Status=1 where Id=@Id";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                int count = connection.Execute(stringSql, new { @Id = Id });
                if (count > 0)
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(T model)
        {
            bool result = false;
            if (model == null) throw new ArgumentNullException("model");

            PropertyInfo[] properties = model.GetType().GetProperties();
            string propertiesStr = string.Join(",", properties.Where(p => p.Name != "Id").Select(p => $"{p.Name}=@{p.Name}"));
            string stringSql = $"update {typeof(T).Name} set {propertiesStr} where @Id=Id";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                int count = connection.Execute(stringSql, model);
                if (count > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(List<T> models)
        {
            bool result = false;
            if (models == null) throw new ArgumentNullException("model");

            var model = models[0];
            PropertyInfo[] properties = model.GetType().GetProperties();
            string propertiesStr = string.Join(",", properties.Where(p => p.Name != "Id").Select(p => $"{p.Name}=@{p.Name}"));
            string stringSql = $"update {typeof(T).Name} set {propertiesStr} where @Id=Id";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                int count = connection.Execute(stringSql, models);
                if (count > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        public T Get(object whereParameters = null, List<string> columnParameters = null)
        {
            string columnStr = "*";
            string strWhere = " Status=0";

            if (columnParameters != null)
            {
                columnStr = string.Join(",", columnParameters);
            }
            if (whereParameters != null)
            {
                PropertyInfo[] properties = whereParameters.GetType().GetProperties();
                string propertiesStr = string.Join(",", properties.Select(p => $" and {p.Name}='{p.GetValue(whereParameters)}'"));
                strWhere += propertiesStr;
            }

            string stringSql = $"select {columnStr} from {typeof(T).Name} where {strWhere}";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                T model = connection.QueryFirstOrDefault<T>(stringSql);
                if (model != null)
                {
                    return model;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="dyOrder">排序</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        public List<T> GetList(object whereParameters = null, string dyOrder = null, List<string> columnParameters = null)
        {
            string columnStr = "*";
            string strWhere = " Status=0";
            string orderStr = "Id";

            if (columnParameters != null)
            {
                columnStr = string.Join(",", columnParameters);
            }
            if (whereParameters != null)
            {
                PropertyInfo[] properties = whereParameters.GetType().GetProperties();
                string propertiesStr = string.Join(",", properties.Select(p => $" and {p.Name}='{p.GetValue(whereParameters)}'"));
                strWhere += propertiesStr;
            }
            if (!string.IsNullOrEmpty(dyOrder))
            {
                orderStr = dyOrder;
            }

            string stringSql = $"select {columnStr} from {typeof(T).Name} where {strWhere} order by {orderStr}";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                IEnumerable<T> models = connection.Query<T>(stringSql);
                if (models != null && models.Count() > 0)
                {
                    return models.ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="pageSize">每一页数据量</param>
        /// <param name="currentPage">当前页</param>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="dyOrder">排序</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        public PageModels<T> GetByPager(int pageSize, int currentPage, object whereParameters = null, string dyOrder = null,
            List<string> columnParameters = null)
        {
            string columnStr = "*";
            string strWhere = " Status=0";
            string orderStr = "Id";

            if (columnParameters != null)
            {
                columnStr = string.Join(",", columnParameters);
            }
            if (whereParameters != null)
            {
                PropertyInfo[] properties = whereParameters.GetType().GetProperties();
                string propertiesStr = string.Join(",", properties.Select(p => $" and {p.Name}='{p.GetValue(whereParameters)}'"));
                strWhere += propertiesStr;
            }
            if (!string.IsNullOrEmpty(dyOrder))
            {
                orderStr = dyOrder;
            }

            string stringSql = $"select {columnStr} from {typeof(T).Name} where {strWhere} order by {orderStr} limit @Index,@PageSize";
            string stringSqlCount = $"select count(1) from {typeof(T).Name} where {strWhere}";
            int index = (currentPage - 1) * pageSize;
            PageModels<T> result = new PageModels<T>();
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                IEnumerable<T> models = connection.Query<T>(stringSql, new { @Index = index, @PageSize = pageSize });
                if (models != null && models.Count() > 0)
                {
                    result.Data = models.ToList();
                    result.Count = connection.QuerySingle<int>(stringSqlCount);
                    result.PageCount = (int)Math.Ceiling((double)result.Count / pageSize);
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region  异步

        /// <summary>
        /// 新增实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> InsertAsync(T model)
        {
            bool result = false;
            if (model == null) throw new ArgumentNullException("model");

            PropertyInfo[] properties = model.GetType().GetProperties();
            string propertiesStr = string.Join(",", properties.Select(p => p.Name));
            string valueStr = string.Join(",", properties.Select(p => $"@{p.Name}"));
            string stringSql = $"insert into {typeof(T).Name} ({propertiesStr}) values ({valueStr})";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                int count = await connection.ExecuteAsync(stringSql, model);
                if (count > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 新增实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> InsertAsync(List<T> models)
        {
            bool result = false;

            if (models == null) throw new ArgumentNullException("models");

            var model = models[0];
            PropertyInfo[] properties = model.GetType().GetProperties();
            string propertiesStr = string.Join(",", properties.Select(p => p.Name));
            string valueStr = string.Join(",", properties.Select(p => $"@{p.Name}"));
            string stringSql = $"insert into {typeof(T).Name} ({propertiesStr}) values ({valueStr})";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                int count = await connection.ExecuteAsync(stringSql, models);
                if (count > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> RemoveAsync(string Id)
        {
            bool result = false;
            if (string.IsNullOrEmpty(Id)) throw new ArgumentNullException("Id");

            string stringSql = $"update {typeof(T).Name} set Status=1 where Id=@Id";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                int count = await connection.ExecuteAsync(stringSql, new { @Id = Id });
                if (count > 0)
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(T model)
        {
            bool result = false;
            if (model == null) throw new ArgumentNullException("model");

            PropertyInfo[] properties = model.GetType().GetProperties();
            string propertiesStr = string.Join(",", properties.Where(p => p.Name != "Id").Select(p => $"{p.Name}=@{p.Name}"));
            string stringSql = $"update {typeof(T).Name} set {propertiesStr} where @Id=Id";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                int count = await connection.ExecuteAsync(stringSql, model);
                if (count > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(List<T> models)
        {
            bool result = false;
            if (models == null) throw new ArgumentNullException("model");

            var model = models[0];
            PropertyInfo[] properties = model.GetType().GetProperties();
            string propertiesStr = string.Join(",", properties.Where(p => p.Name != "Id").Select(p => $"{p.Name}=@{p.Name}"));
            string stringSql = $"update {typeof(T).Name} set {propertiesStr} where @Id=Id";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                int count = await connection.ExecuteAsync(stringSql, models);
                if (count > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        public async Task<T> GetAsync(object whereParameters = null, List<string> columnParameters = null)
        {
            string columnStr = "*";
            string strWhere = " Status=0";

            if (columnParameters != null)
            {
                columnStr = string.Join(",", columnParameters);
            }
            if (whereParameters != null)
            {
                PropertyInfo[] properties = whereParameters.GetType().GetProperties();
                string propertiesStr = string.Join(",", properties.Select(p => $" and {p.Name}='{p.GetValue(whereParameters)}'"));
                strWhere += propertiesStr;
            }

            string stringSql = $"select {columnStr} from {typeof(T).Name} where {strWhere}";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                T model = await connection.QueryFirstOrDefaultAsync<T>(stringSql);
                if (model != null)
                {
                    return model;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="dyOrder">排序</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        public async Task<List<T>> GetListAsync(object whereParameters = null, string dyOrder = null, List<string> columnParameters = null)
        {
            string columnStr = "*";
            string strWhere = " Status=0";
            string orderStr = "Id";

            if (columnParameters != null)
            {
                columnStr = string.Join(",", columnParameters);
            }
            if (whereParameters != null)
            {
                PropertyInfo[] properties = whereParameters.GetType().GetProperties();
                string propertiesStr = string.Join(",", properties.Select(p => $" and {p.Name}='{p.GetValue(whereParameters)}'"));
                strWhere += propertiesStr;
            }
            if (!string.IsNullOrEmpty(dyOrder))
            {
                orderStr = dyOrder;
            }

            string stringSql = $"select {columnStr} from {typeof(T).Name} where {strWhere} order by {orderStr}";
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                IEnumerable<T> models = await connection.QueryAsync<T>(stringSql);
                if (models != null && models.Count() > 0)
                {
                    return models.ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="pageSize">每一页数据量</param>
        /// <param name="currentPage">当前页</param>
        /// <param name="whereParameters">限制条件</param>
        /// <param name="dyOrder">排序</param>
        /// <param name="columnParameters">查询属性</param>
        /// <returns></returns>
        public async Task<PageModels<T>> GetByPagerAsync(int pageSize, int currentPage, object whereParameters = null, string dyOrder = null,
            List<string> columnParameters = null)
        {
            string columnStr = "*";
            string strWhere = " Status=0";
            string orderStr = "Id";

            if (columnParameters != null)
            {
                columnStr = string.Join(",", columnParameters);
            }
            if (whereParameters != null)
            {
                PropertyInfo[] properties = whereParameters.GetType().GetProperties();
                string propertiesStr = string.Join(",", properties.Select(p => $" and {p.Name}='{p.GetValue(whereParameters)}'"));
                strWhere += propertiesStr;
            }
            if (!string.IsNullOrEmpty(dyOrder))
            {
                orderStr = dyOrder;
            }

            string stringSql = $"select {columnStr} from {typeof(T).Name} where {strWhere} order by {orderStr} limit @Index,@PageSize";
            string stringSqlCount = $"select count(1) from {typeof(T).Name} where {strWhere}";
            int index = (currentPage - 1) * pageSize;
            PageModels<T> result = new PageModels<T>();
            using (IDbConnection connection = new MySqlConnection(connectionStr))
            {
                IEnumerable<T> models = await connection.QueryAsync<T>(stringSql, new { @Index = index, @PageSize = pageSize });
                if (models != null && models.Count() > 0)
                {
                    result.Data = models.ToList();
                    result.Count = await connection.QuerySingleAsync<int>(stringSqlCount);
                    result.PageCount = (int)Math.Ceiling((double)result.Count / pageSize);
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion
    }
}
