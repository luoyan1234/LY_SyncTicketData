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
        /// ��԰����
        /// </summary>
        public string Plandate
        {
            get { return plandate; }
            set { plandate = value; }
        }

        /// <summary>
        /// ����״̬
        /// </summary>
        public int Status
        {
            get { return status; }
            set { status = value; }
        }

        /// <summary>
        /// �������֤
        /// </summary>
        public string Idnum
        {
            get { return idnum; }
            set { idnum = value; }
        }

        /// <summary>
        /// �������
        /// </summary>
        public string Ordersid
        {
            get { return ordersid; }
            set { ordersid = value; }
        }

        /// <summary>
        /// ��Ʒ���
        /// </summary>
        public string Goodsid
        {
            get { return goodsid; }
            set { goodsid = value; }
        }

        /// <summary>
        /// ��԰ʱ��
        /// </summary>
        public string Entertime
        {
            get { return entertime; }
            set { entertime = value; }
        }
    }
}
