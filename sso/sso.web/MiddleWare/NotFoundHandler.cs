using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace sso.web.MiddleWare
{
    /// <summary>
    /// 中间件
    /// 捕捉404异常
    /// </summary>
    public class NotFoundHandler
    {
        private readonly RequestDelegate _requestDelegate;

        public NotFoundHandler(RequestDelegate requestDelegate)
        {
            this._requestDelegate = requestDelegate;
        }

        public async Task Invoke(HttpContext context)
        {
            await _requestDelegate.Invoke(context);

            HttpResponse response = context.Response;

            int status = response.StatusCode;
            if (status == 404)
            {
                await response.WriteAsync("404error");
            }
        }
    }
}
