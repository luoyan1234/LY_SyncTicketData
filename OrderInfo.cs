using System;
using System.Collections.Generic;
using System.Text;

namespace LY_SyncTicketData
{
    internal class OrderInfo
    {
        public int Parkid { get; set; }//��԰����
        public string Plandate { get; set; }//��԰ʱ��
        public string Phone { get; set; }//�ֻ���
        public decimal Totalmoney { get; set; }//�����ܼ�
        public string Paymode { get; set; }//֧����ʽ
        public string OrderId { get; set; }//��������
        public string Tradeno { get; set; }//���׺�
        public string PayTime { get; set; }//֧��ʱ��
        public List<TicketModel> TicketList { get; set; }//Ʊ����Ϣ�б�

    }
    public class TicketModel
    {
        public string tickettypeid { get; set; }//Ʊ�����
        public int number { get; set; }//����
        public decimal saleprice { get; set; }//�ۼ�
        public string name { get; set; }//�ο�����
        public string idnum { get; set; }//���֤��
    }
}
