using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LY_SyncTicketData
{
    public class LoginRes
    {
        public TokenModel Data { get; set; }
        public int ResultStatus { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
    }
    public class TokenModel
    {
        public string token { get; set; }

        public string expiredTime { get; set; }
    }
    public class AddReturnRes
    {
        public ToCenterBackDetail Data { get; set; }
        public int ResultStatus { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
    }
    public class ToCenterBackDetail
    {
        public string orderid { get; set; }
        public string qrcodeimg { get; set; }
        public string ticketcode { get; set; }
        public string idnum { get; set; }
        public string tickettypeid { get; set; }
    }
    public class QueryDetailRes
    {
        public List<OrderSerchModel> Data { get; set; }
        public int ResultStatus { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
    }
    /// <summary>
    /// 订单详细信息
    /// </summary>
    public class OrderSerchModel
    {
        public string orderid { get; set; }
        public int parkid { get; set; }
        public string name { get; set; }
        public string ticketcode { get; set; }
        public string idnum { get; set; }
        public string inputtime { get; set; }
        public string plandate { get; set; }
        public string tickettypeid { get; set; }
        public string tickettypename { get; set; }
        public int number { get; set; }
        public int planstate { get; set; }//0:未出票 2 已出票 3 已取消
        public string statename { get; set; }
        public int salesqty { get; set; }
    }
}
