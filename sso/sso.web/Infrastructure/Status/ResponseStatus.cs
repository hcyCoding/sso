using System.ComponentModel;

namespace sso.web.Infrastructure.Status
{
    /// <summary>
    /// HTTP状态码
    /// </summary>
    public enum ResponseStatus
    {
        [Description("请求成功")]
        OK = 200,

        [Description("参数错误")]
        ErrorParameters = 201,

        [Description("token过期")]
        TimeOutToken = 202,

        [Description("警告")]
        Warnning = 203,

        [Description("重定向")]
        Redirect=301,

        [Description("服务器内部错误")]
        InternalServerError = 500
    };
}
