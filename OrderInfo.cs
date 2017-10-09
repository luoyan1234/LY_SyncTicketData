using System;
using System.Collections.Generic;
using System.Text;

namespace LY_SyncTicketData
{
    internal class OrderInfo
    {
        public int Parkid { get; set; }//公园编码
        public string Plandate { get; set; }//入园时间
        public string Phone { get; set; }//手机号
        public decimal Totalmoney { get; set; }//订单总价
        public string Paymode { get; set; }//支付方式
        public string OrderId { get; set; }//订单编码
        public string Tradeno { get; set; }//交易号
        public string PayTime { get; set; }//支付时间
        public List<TicketModel> TicketList { get; set; }//票类信息列表

    }
    public class TicketModel
    {
        public string tickettypeid { get; set; }//票类编码
        public int number { get; set; }//数量
        public decimal saleprice { get; set; }//售价
        public string name { get; set; }//游客姓名
        public string idnum { get; set; }//身份证号
    }
}
