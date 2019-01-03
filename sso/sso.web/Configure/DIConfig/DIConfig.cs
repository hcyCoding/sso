using Microsoft.Extensions.DependencyInjection;
using sso.web.Core.Repository;
using sso.web.Core.Repository.Impl;
using sso.web.Service;
using sso.web.Service.Impl;

namespace sso.web.Configure.DIConfig
{
    /// <summary>
    /// 依赖注入
    /// </summary>
    public class DIConfig
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddTransient<IUsersRepository, UsersRepository>();

            services.AddTransient<IAccountService, AccountService>();
        }
    }
}
