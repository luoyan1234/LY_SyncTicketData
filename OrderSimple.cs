using System;
using System.Collections.Generic;
using System.Text;

namespace LY_SyncTicketData
{
    internal class OrderSimple
    {
        public string ordersid {get; set;}//��������
        public int parkid { get; set; }//��԰����
        public string tickettypeid { get; set; }//Ʊ�����
        public string idnum { get; set; }//���֤����
        public int status { get; set; }//����״̬
        public string plandate { get; set; }//��԰����
        public string entertime { get; set; }//��԰ʱ��

    }
}
