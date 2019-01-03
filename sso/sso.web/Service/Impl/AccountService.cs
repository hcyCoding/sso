using Microsoft.AspNetCore.Http;
using sso.web.Core.Repository;
using sso.web.Infrastructure;
using sso.web.Infrastructure.Configuration;
using sso.web.Infrastructure.Status;
using System.Collections.Generic;

namespace sso.web.Service.Impl
{
    public class AccountService : IAccountService
    {
        private readonly IUsersRepository _usersRepository;

        public AccountService(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="Name">用户名</param>
        /// <param name="Password">密码</param>
        /// <param name="Context">HTTP请求上下文</param>
        /// <param name="Redirect">回调URL</param>
        /// <returns></returns>
        public ResponseModel Login(string Name, string Password, HttpContext Context, out string Redirect)
        {
            var User = _usersRepository.Get(new { Name = Name }, new List<string>() { "Id", "Name", "PassWord" });
            Redirect = "";
            if (User != null)
            {
                if (User.PassWord == Password)
                {
                    StackExchangeHelper stackExchangeHelper = new StackExchangeHelper();
                    string JseSessionId = CookiesOperation.GetCookies(Context, "JSESSIONID");
                    string UA = Context.Request.Headers["User-Agent"].ToString();
                    string Key = Helper.GeneratorDesKey();

                    //获取回调的URL
                    Redirect = ServiceHelper.GetRollBackUrl(Context);
                    if (!string.IsNullOrEmpty(Redirect))
                    {
                        //获取回调的URL的域名
                        string Domain = Helper.GetDomain(Redirect);
                        //生成Token
                        string Token = Helper.GeneratorToken(User.Id, UA, Key);
                        //服务端存储Token密钥
                        stackExchangeHelper.StringSet(JseSessionId + AppConfig.DESKeyStr, Key, ExpiresTime.ServerExpiresTime);
                        //服务端存储Token
                        stackExchangeHelper.StringSet(JseSessionId + AppConfig.TokenStr, Token, ExpiresTime.ServerExpiresTime);
                        //服务端注册本次登陆的域名,并设置过期时间
                        stackExchangeHelper.ListRightPush(JseSessionId + AppConfig.DomainStr, Domain);
                        stackExchangeHelper.KeyExpire(JseSessionId + AppConfig.DomainStr, ExpiresTime.ServerExpiresTime);

                        return new ResponseModel(ResponseStatus.OK, "登陆成功！", new { token = Token, redirect = Redirect });
                    }
                    else
                    {
                        return new ResponseModel(ResponseStatus.Warnning, "未找到回调URL");
                    }
                }
                else
                {
                    return new ResponseModel(ResponseStatus.Warnning, "密码错误！");
                }
            }
            else
            {
                return new ResponseModel(ResponseStatus.Warnning, "未找到相应用户信息！");
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="Token">Token</param>
        /// <returns></returns>
        public ResponseModel GetUserName(string Token, HttpContext Context)
        {
            StackExchangeHelper stackExchangeHelper = new StackExchangeHelper();
            string JseSessionId = CookiesOperation.GetCookies(Context, "JSESSIONID");
            string DESKey = stackExchangeHelper.StringGet(JseSessionId + AppConfig.DESKeyStr);
            string DecryptClientToken = Helper.DESDecrypt(Token, DESKey);
            //解析Token，数组第一个元素是UserId,第二个元素是时间戳
            string[] ClientTokenArray = DecryptClientToken.Split(AppConfig.SplitCode);
            if (ClientTokenArray != null && ClientTokenArray.Length > 0)
            {
                string UserId = ClientTokenArray[0];
                var User = _usersRepository.Get(new { Id = UserId }, new List<string>() { "Id", "Name", "Phone", "Email" });
                return new ResponseModel(ResponseStatus.OK, "", User);
            }
            else
            {
                return new ResponseModel(ResponseStatus.ErrorParameters, "Token解析错误！");
            }
        }
    }
}
