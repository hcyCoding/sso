using Microsoft.AspNetCore.Builder;

namespace sso.web.MiddleWare
{
    public static class ExportNotFoundHandler
    {
        public static IApplicationBuilder UseNotFoundHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<NotFoundHandler>();
        }
    }
}
