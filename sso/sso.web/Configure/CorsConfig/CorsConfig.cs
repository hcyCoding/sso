using Microsoft.Extensions.DependencyInjection;
using sso.web.Infrastructure.Configuration;

namespace sso.web.Configure.CorsConfig
{
    public class CorsConfig
    {
        /// <summary>
        /// 配置跨域策略
        /// </summary>
        /// <param name="services"></param>
        /// <param name="PolicyName">策略名称</param>
        public static void Config(IServiceCollection services, string PolicyName)
        {
            string allowCors = AppConfig.AllowCors;
            if (!string.IsNullOrEmpty(allowCors))
            {
                string[] domain = allowCors.Split(",");
                services.AddCors(options =>
                {
                    options.AddPolicy(PolicyName, builder =>
                    {
                        builder.WithOrigins(domain).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    });
                });
            }
            else
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(PolicyName, builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    });
                });
            }
        }
    }
}
