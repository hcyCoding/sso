using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using sso.web.Infrastructure;
using sso.web.Infrastructure.Configuration;
using sso.web.Infrastructure.Status;
using System;

namespace sso.web.Filter
{
    public class IdentityTokenFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string ClientToken = string.Empty;
            string JseSessionId = CookiesOperation.GetCookies(context.HttpContext, "JSESSIONID");
            if (context.HttpContext.Request.Method == "POST")
            {
                ClientToken = context.HttpContext.Request.Form["token"].ToString();
            }
            else
            {
                ClientToken = context.HttpContext.Request.Query["token"].ToString();
            }

            if (!string.IsNullOrEmpty(ClientToken) && !string.IsNullOrEmpty(JseSessionId))
            {
                StackExchangeHelper stackExchangeHelper = new StackExchangeHelper();
                //key值剩余时间
                TimeSpan? KeyRemainTime = stackExchangeHelper.GetRemainTime(JseSessionId + AppConfig.TokenStr);

                if (KeyRemainTime != null)
                {
                    string ServerToken = stackExchangeHelper.StringGet(JseSessionId + AppConfig.TokenStr);
                    if (ClientToken == ServerToken)
                    {
                        //key值剩余分钟数
                        int RemainMinutes = Convert.ToInt32(((TimeSpan)KeyRemainTime).TotalMinutes);
                        //如果key值即将过期，需要对Token进行延时
                        if (RemainMinutes < 5)
                        {
                            //服务端存储Token密钥
                            stackExchangeHelper.KeyExpire(JseSessionId + AppConfig.DESKeyStr, ExpiresTime.AddtionExpiresTime);
                            //服务端存储Token
                            stackExchangeHelper.KeyExpire(JseSessionId + AppConfig.TokenStr, ExpiresTime.AddtionExpiresTime);
                            //服务端注册本次登陆的域名
                            stackExchangeHelper.KeyExpire(JseSessionId + AppConfig.DomainStr, ExpiresTime.AddtionExpiresTime);
                        }
                    }
                    else
                    {
                        //token匹配错误
                        context.Result = new JsonResult(new ResponseModel(ResponseStatus.ErrorParameters, "Token错误！"));
                    }
                }
                else
                {
                    //服务端token已过期
                    context.Result = new JsonResult(new ResponseModel(ResponseStatus.Redirect, "Token过期！"));
                }
            }
            else
            {
                //缺少参数
                context.Result = new JsonResult(new ResponseModel(ResponseStatus.Warnning, "缺少重要参数！"));
            }
        }
    }
}
