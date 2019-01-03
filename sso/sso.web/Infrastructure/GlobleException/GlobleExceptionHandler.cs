using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace sso.web.Infrastructure.GlobleException
{
    /// <summary>
    /// 全局异常捕捉
    /// </summary>
    public class GlobleExceptionHandler
    {
        public static Task ExceptionHandler(HttpContext Context)
        {
            var Feature = Context.Features.Get<IExceptionHandlerFeature>();
            var Error = Feature?.Error;
            var Status = Context.Response.StatusCode;
            return Context.Response.WriteAsync(Status.ToString());
        }
    }
}
