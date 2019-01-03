
namespace sso.web.Infrastructure.Configuration
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// 获取DES机密密钥后缀名
        /// </summary>
        public static string DESKeyStr
        {
            get
            {
                return ConfigurationManager.Configuration["SysConfig:DESKey"].ToString();
            }
        }

        /// <summary>
        /// 获取Token后缀名
        /// </summary>
        public static string TokenStr
        {
            get
            {
                return ConfigurationManager.Configuration["SysConfig:Token"].ToString();
            }
        }

        /// <summary>
        /// 获取域名注册key值后缀名
        /// </summary>
        public static string DomainStr
        {
            get
            {
                return ConfigurationManager.Configuration["SysConfig:Domain"].ToString();
            }
        }

        /// <summary>
        /// 获取字符串分隔符号
        /// </summary>
        public static string SplitCode
        {
            get
            {
                return ConfigurationManager.Configuration["SplitCode"].ToString();
            }
        }

        /// <summary>
        /// 跨域白名单
        /// </summary>
        public static string AllowCors
        {
            get
            {
                return ConfigurationManager.Configuration["AllowCors"].ToString();
            }
        }
    }
}
