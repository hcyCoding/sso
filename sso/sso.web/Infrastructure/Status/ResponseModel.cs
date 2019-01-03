

using System;

namespace sso.web.Infrastructure.Status
{
    /// <summary>
    /// 请求返回信息
    /// </summary>
    public class ResponseModel
    {

        public ResponseModel(ResponseStatus status)
        {
            this.status = status;
        }

        public ResponseModel(ResponseStatus status, string msg)
            : this(status)
        {
            this.msg = msg;
        }

        public ResponseModel(ResponseStatus status, string msg, object data)
            : this(status, msg)
        {
            this.data = data;
        }

        /// <summary>
        /// 状态
        /// </summary>
        public ResponseStatus status { get; set; }

        /// <summary>
        /// 主要数据
        /// </summary>
        public object data { get; set; }

        /// <summary>
        /// 内容说明
        /// </summary>
        public string msg { get; set; }
    }
}
