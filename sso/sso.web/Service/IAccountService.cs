using Microsoft.AspNetCore.Http;
using sso.web.Infrastructure.Status;

namespace sso.web.Service
{
    public interface IAccountService
    {
        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="Name">用户名</param>
        /// <param name="Password">密码</param>
        /// <param name="Context">HTTP请求上下文</param>
        /// <param name="Redirect">回调URL</param>
        /// <returns></returns>
        ResponseModel Login(string Name, string Password, HttpContext Context, out string Redirect);

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="Token">Token</param>
        /// <returns></returns>
        ResponseModel GetUserName(string Token, HttpContext Context);
    }
}
