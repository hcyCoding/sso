using sso.web.Infrastructure.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace sso.web.Infrastructure
{
    /// <summary>
    /// 帮助类 
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// 生成一个GUID
        /// </summary>
        /// <returns></returns>
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="UserId">用户id</param>
        /// <param name="UA">UserAgent</param>
        /// <param name="Key">DES加密密钥</param>
        /// <returns>Token</returns>
        public static string GeneratorToken(string UserId, string UA, string Key)
        {
            string timeStamp = GetUnixTimeStamp(DateTime.Now).ToString();
            string SplitCode = AppConfig.SplitCode;
            string OriginToken = UserId + SplitCode + timeStamp;
            string encryptString = DESEncrypt(OriginToken, Key);
            return encryptString;
        }

        #region  时间戳

        /// <summary>
        /// 生成十位的Unix时间戳
        /// </summary>
        /// <param name="dt">要转化的时间</param>
        /// <returns>十位的Unix时间戳</returns>
        public static int GetUnixTimeStamp(DateTime dt)
        {
            DateTime startTime = new DateTime(1970, 1, 1).ToLocalTime();
            int timeStamp = Convert.ToInt32((dt - startTime).TotalSeconds);
            return timeStamp;
        }

        /// <summary>
        /// 十位的时间戳转化为DateTime
        /// </summary>
        /// <param name="stamp">时间戳</param>
        /// <returns>DateTime类型时间</returns>
        public static DateTime GetDateTimeFromUnixTimeStamp(int stamp)
        {
            DateTime startTime = new DateTime(1970, 1, 1);
            DateTime dt = startTime.AddSeconds(stamp);
            return dt.ToLocalTime();
        }

        #endregion

        #region  DES加密/解密

        /// <summary>
        /// DES加密生成一个密钥
        /// </summary>
        /// <returns></returns>
        public static string GeneratorDesKey()
        {
            DESCryptoServiceProvider dES = new DESCryptoServiceProvider();
            string key = ASCIIEncoding.ASCII.GetString(dES.Key);
            return key;
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="pwd">要加密的内容</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        public static string DESEncrypt(string encryptString, string key)
        {
            //默认向量
            byte[] ivs = { 0x13, 0x24, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

            using (DESCryptoServiceProvider dES = new DESCryptoServiceProvider())
            {
                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                dES.Key = ASCIIEncoding.ASCII.GetBytes(key);
                dES.IV = ivs;

                MemoryStream memory = new MemoryStream();
                using (CryptoStream csStream = new CryptoStream(memory, dES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    csStream.Write(inputByteArray, 0, inputByteArray.Length);
                    csStream.FlushFinalBlock();
                }

                string result = Convert.ToBase64String(memory.ToArray());
                memory.Close();
                return result;
            }
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="pwd">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string DESDecrypt(string decryptString, string key)
        {
            //默认向量
            byte[] ivs = { 0x13, 0x24, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

            using (DESCryptoServiceProvider dES = new DESCryptoServiceProvider())
            {
                byte[] inputByteArray = Convert.FromBase64String(decryptString);
                dES.Key = ASCIIEncoding.ASCII.GetBytes(key);
                dES.IV = ivs;

                MemoryStream memory = new MemoryStream();
                using (CryptoStream csStream = new CryptoStream(memory, dES.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    csStream.Write(inputByteArray, 0, inputByteArray.Length);
                    csStream.FlushFinalBlock();
                }

                string result = Encoding.UTF8.GetString(memory.ToArray());
                memory.Close();
                return result;
            }
        }

        #endregion

        #region  URL

        /// <summary>
        /// 对url进行编码
        /// </summary>
        /// <param name="Url">url</param>
        /// <returns></returns>
        public static string UrlEncode(string Url)
        {
            return HttpUtility.UrlEncode(Url);
        }

        /// <summary>
        /// 对url进行解码
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static string UrlDecode(string Url)
        {
            return HttpUtility.UrlDecode(Url);
        }

        /// <summary>
        /// 获取字符串Url中的域名部分
        /// </summary>
        /// <param name="Url">url</param>
        /// <returns></returns>
        public static string GetDomain(string Url)
        {
            Uri uri = new Uri(Url);
            return uri.Authority;
        }

        #endregion
    }
}
