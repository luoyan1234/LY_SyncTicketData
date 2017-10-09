using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LY_SyncTicketData
{

    /// <summary>
    /// pengxy 2016-3-21  Des3加解密类
    /// </summary>
    public static class EncryptUtils
    {
        /// <summary> 3DES解密</summary>
        /// <returns>解密串</returns> 		
        /// <param name="a_strString">加密串</param> 
        /// <param name="key">密钥</param> 
        public static string Decrypt3DES(string a_strString, string key)
        {
            if (string.IsNullOrEmpty(a_strString))
            {
                return a_strString;
            }
            try
            {
                key = MD5Encrypt(key, Encoding.Default).Substring(0, 24);
                var DES = new TripleDESCryptoServiceProvider();
                DES.Key = ASCIIEncoding.ASCII.GetBytes(key);
                DES.IV = ASCIIEncoding.ASCII.GetBytes(key.Substring(0, 8));
                DES.Mode = CipherMode.CBC;
                DES.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                ICryptoTransform DESDecrypt = DES.CreateDecryptor();
                byte[] Buffer = Convert.FromBase64String(a_strString);
                return ASCIIEncoding.ASCII.GetString(DESDecrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
            }
            catch (Exception e)
            {
                return "";
            }
        }

        /// <summary>
        /// 3DES加密 		
        /// base64小细节，当使用get请求时，base64生成字符中有“+”号， 		
        /// 注意需要转换“%2B”，否则会被替换成空格。POST不存在 		
        /// while (str.IndexOf('+') != -1) { 		
        ///	 str = str.Replace("+","%2B"); 		
        ///  } 	
        ///  
        /// key 值为用户登录帐号  
        /// </summary> 		
        public static string Encrypt3DES(string fValue, string key)
        {
            if (string.IsNullOrEmpty(fValue))
            {
                return fValue;
            }
            try
            {
                key=MD5Encrypt(key, Encoding.Default).Substring(0,24);
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                var DES = new TripleDESCryptoServiceProvider();
                DES.Key = encoding.GetBytes(key);
                DES.IV = encoding.GetBytes(key.Substring(0, 8));
                DES.Mode = CipherMode.CBC;
                ICryptoTransform DESEncrypt = DES.CreateEncryptor();
                byte[] Buffer = encoding.GetBytes(fValue);
                return Convert.ToBase64String(DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
  
         /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
        /// <param name="encode">字符的编码</param>
        /// <returns></returns>
        public static string MD5Encrypt(string input, Encoding encode)
        {

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(encode.GetBytes(input));
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();
        }
    }
}
