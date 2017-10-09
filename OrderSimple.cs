using System;
using System.Collections.Generic;
using System.Text;

namespace LY_SyncTicketData
{
    internal class OrderSimple
    {
        string ordersid;
        string goodsid;
        string idnum;
        int status;
        string plandate;
        string entertime;

        /// <summary>
        /// 入园日期
        /// </summary>
        public string Plandate
        {
            get { return plandate; }
            set { plandate = value; }
        }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int Status
        {
            get { return status; }
            set { status = value; }
        }

        /// <summary>
        /// 订单身份证
        /// </summary>
        public string Idnum
        {
            get { return idnum; }
            set { idnum = value; }
        }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string Ordersid
        {
            get { return ordersid; }
            set { ordersid = value; }
        }

        /// <summary>
        /// 商品编号
        /// </summary>
        public string Goodsid
        {
            get { return goodsid; }
            set { goodsid = value; }
        }

        /// <summary>
        /// 入园时间
        /// </summary>
        public string Entertime
        {
            get { return entertime; }
            set { entertime = value; }
        }
    }
}
