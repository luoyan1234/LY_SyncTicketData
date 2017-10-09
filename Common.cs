using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Security;

namespace LY_SyncTicketData
{
    public class Common
    {
        /// <summary>
        /// 返回JSon数据
        /// </summary>
        /// <param name="jsonData">要处理的JSON数据</param>
        /// <param name="url">要提交的URL</param>
        /// <returns>返回的JSON处理字符串</returns>
        public static string PostHttpService(string url, string jsonData)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            byte[] datas = Encoding.UTF8.GetBytes(jsonData);
            request.ContentLength = datas.Length;

            HttpWebResponse response = null;
            string responseDatas = string.Empty;

            try
            {
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(datas, 0, datas.Length);
                requestStream.Close();
            }
            catch (Exception ex)
            {
                request.Abort();
            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                Stream streamResponse = response.GetResponseStream();
                using (StreamReader sr = new StreamReader(streamResponse))
                {
                    responseDatas = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                request.Abort();
            }
            finally
            {
                if (response != null)
                {
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                        request.Abort();
                    }
                }
            }
            return responseDatas;
        }
        /// <summary>
        /// 对实体类进行json序列化
        /// </summary>
        /// <param name="item">实体类对象</param>
        /// <returns>json格式字符串</returns>
        public static string ToJsonData(object item)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(item.GetType());
            string result = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, item);
                ms.Position = 0;
                using (StreamReader reader = new StreamReader(ms))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="sendMsg"></param>
        /// <returns></returns>
        public static string SendMessage(string phone, string sendMsg)
        {
            LY_SyncTicketData.cn.entinfo.sdk3.WebService sms = new LY_SyncTicketData.cn.entinfo.sdk3.WebService();
            string strSN = "SDK-BBX-010-13663";
            string strPWD = "727100";
            strPWD = FormsAuthentication.HashPasswordForStoringInConfigFile(strSN + strPWD, "md5");

            string i = sms.mt(strSN, strPWD, phone, sendMsg, "", "", "");
            return i;
        }
    }
}
