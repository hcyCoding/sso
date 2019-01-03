using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sso.web.Infrastructure
{
    /// <summary>
    /// Cookie帮助类
    /// </summary>
    public class CookiesOperation
    {
        /// <summary>
        /// 获取cookie值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetCookies(HttpContext context, string key)
        {
            context.Request.Cookies.TryGetValue(key, out string value);
            if (string.IsNullOrEmpty(value))
                value = string.Empty;
            return value;
        }

        /// <summary>
        /// 设置cookie
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="minutes"></param>
        public static void SetCookies(HttpContext context, string key, string value, int minutes = 30)
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(minutes);
            context.Response.Cookies.Append(key, value, options);
        }

        public static void SetCookies(HttpContext context, string key, string value, string domain, int minutes = 30)
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(minutes);
            options.Domain = domain;
            context.Response.Cookies.Append(key, value, options);
        }

        /// <summary>
        /// 删除cookie
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        public static void DeleteCookies(HttpContext context, string key)
        {
            context.Response.Cookies.Delete(key);
        }
    }
}
