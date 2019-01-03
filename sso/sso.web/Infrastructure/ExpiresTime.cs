

namespace sso.web.Infrastructure
{
    public static class ExpiresTime
    {
        private static readonly int BaseExpiresTime = 30;

        /// <summary>
        /// 服务端存储数据过期时间
        /// </summary>
        public static int ServerExpiresTime
        {
            get
            {
                return BaseExpiresTime;
            }
        }

        /// <summary>
        /// 数据即将过期时，每次延时时间
        /// </summary>
        public static int AddtionExpiresTime
        {
            get
            {
                return BaseExpiresTime / 3;
            }
        }
    }
}
