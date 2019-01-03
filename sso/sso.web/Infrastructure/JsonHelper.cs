using Newtonsoft.Json;
using System.Collections.Generic;

namespace sso.web.Infrastructure
{
    /// <summary>
    /// json的序列化和反序列化
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// 将对象转化为json字符串
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public static T Deserialize<T>(string json) where T : class
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public static List<T> DeserializeToList<T>(string json) where T : class
        {
            object obj = JsonConvert.DeserializeObject(json);
            List<T> list = obj as List<T>;
            return list;
        }
    }
}
