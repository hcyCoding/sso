using Microsoft.AspNetCore.Mvc.Filters;
using sso.web.Infrastructure;
using sso.web.Infrastructure.Configuration;
using System.Linq;

namespace sso.web.Filter
{
    /// <summary>
    /// 过滤器
    /// 判断用户是否登陆
    /// 没有登陆，重定向登陆页面
    /// 如果已经登陆过，但是域名没有注册，直接发放Token
    /// </summary>
    public class RequestOperationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string JseSessionId = CookiesOperation.GetCookies(context.HttpContext, "JSESSIONID");
            string Referrence = context.HttpContext.Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(JseSessionId))
            {
                //设置登陆标识
                CookiesOperation.SetCookies(context.HttpContext, "JSESSIONID", Helper.GenerateGuid(), ExpiresTime.ServerExpiresTime);
                //如果没有登陆，重定向
                context.HttpContext.Response.Redirect("/Account/Login?redirect=" + Helper.UrlEncode(Referrence));
            }
            else
            {
                //已经登陆
                StackExchangeHelper stackExchangeHelper = new StackExchangeHelper();
                string Domain = Helper.GetDomain(Referrence);
                //获取所有已注册的域名
                var RegisterUrl = stackExchangeHelper.ListRange(JseSessionId + AppConfig.DomainStr);
                //拿到之前登陆的令牌
                string Token = stackExchangeHelper.StringGet(JseSessionId + AppConfig.TokenStr);
                //判断该域名是否已在服务端注册
                if (!RegisterUrl.Contains(Domain) && !string.IsNullOrEmpty(Token))
                {
                    //服务端注册本次登陆的域名,并设置过期时间
                    stackExchangeHelper.ListRightPush(JseSessionId + AppConfig.DomainStr, Domain);
                    stackExchangeHelper.KeyExpire(JseSessionId + AppConfig.DomainStr, ExpiresTime.ServerExpiresTime);
                    //发放令牌并重定向
                    context.HttpContext.Response.Redirect(Referrence + "?token=" + Token);
                }
                else
                {
                    context.HttpContext.Response.Redirect(Referrence);
                }
            }
        }
    }
}
