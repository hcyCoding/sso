using Microsoft.AspNetCore.Http;
using sso.web.Infrastructure;

namespace sso.web.Service
{
    /// <summary>
    /// 业务相关帮助类
    /// </summary>
    public static class ServiceHelper
    {
        /// <summary>
        /// 获取登陆后的回调URL
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static string GetRollBackUrl(HttpContext Context)
        {
            string Referrence = Context.Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(Referrence))
            {
                int RedirectIndex = Referrence.IndexOf("redirect=");
                string Redirect = Helper.UrlDecode(Referrence.Substring(RedirectIndex + 9));
                return Redirect;
            }
            else
            {
                return "";
            }
        }
    }
}
