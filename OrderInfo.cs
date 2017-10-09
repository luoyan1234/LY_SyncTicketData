using System;
using System.Collections.Generic;
using System.Text;

namespace LY_SyncTicketData
{
    internal class OrderInfo
    {
        int parkid;

        public int Parkid
        {
            get { return parkid; }
            set { parkid = value; }
        }

        string saleno;

        public string Saleno
        {
            get { return saleno; }
            set { saleno = value; }
        }

        string ticketcode;

        public string Ticketcode
        {
            get { return ticketcode; }
            set { ticketcode = value; }
        }

        string plandate;

        public string Plandate
        {
            get { return plandate; }
            set { plandate = value; }
        }
        string paymode;

        public string Paymode
        {
            get { return paymode; }
            set { paymode = value; }
        }
        string tradeno;

        public string Tradeno
        {
            get { return tradeno; }
            set { tradeno = value; }
        }
        string paydate;

        public string Paydate
        {
            get { return paydate; }
            set { paydate = value; }
        }
        string tel;

        public string Tel
        {
            get { return tel; }
            set { tel = value; }
        }
        string tickettypeidlist;

        public string Tickettypeidlist
        {
            get { return tickettypeidlist; }
            set { tickettypeidlist = value; }
        }
        string idlist;

        public string Idlist
        {
            get { return idlist; }
            set { idlist = value; }
        }
        string factPriceList;

        public string FactPriceList
        {
            get { return factPriceList; }
            set { factPriceList = value; }
        }
        string personsamountList;

        public string PersonsamountList
        {
            get { return personsamountList; }
            set { personsamountList = value; }
        }
        string namelist;

        public string Namelist
        {
            get { return namelist; }
            set { namelist = value; }
        }
        string groupname;

        public string Groupname
        {
            get { return groupname; }
            set { groupname = value; }
        }
    }
}
