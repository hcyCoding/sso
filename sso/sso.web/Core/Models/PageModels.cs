using System.Collections.Generic;

namespace sso.web.Core.Models
{
    /// <summary>
    /// 分页实体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageModels<T> where T : class
    {
        /// <summary>
        /// 数据
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// 数据总数
        /// </summary>
        public int Count { get; set; } = 0;

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount { get; set; } = 0;
    }
}
