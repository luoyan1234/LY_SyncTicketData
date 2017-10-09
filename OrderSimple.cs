using System;
using System.Collections.Generic;
using System.Text;

namespace LY_SyncTicketData
{
    internal class OrderSimple
    {
        public string ordersid {get; set;}//订单编码
        public int parkid { get; set; }//公园编码
        public string tickettypeid { get; set; }//票类编码
        public string idnum { get; set; }//身份证编码
        public int status { get; set; }//订单状态
        public string plandate { get; set; }//入园日期
        public string entertime { get; set; }//入园时间

    }
}
