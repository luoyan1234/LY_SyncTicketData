using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace LY_SyncTicketData
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string lyConnStr = "";
        private string centerConnStr = "";
        private int syncTimeSpan = 120;
        private bool is_exit = false;

        List<OrderInfo> listAdd = new List<OrderInfo>();
        List<OrderSimple> listChangePlanDate = new List<OrderSimple>();
        List<OrderSimple> listReturnCard = new List<OrderSimple>();
        List<OrderSimple> listChangeOrderStatus = new List<OrderSimple>();
        List<OrderSimple> listGetOrderStatus = new List<OrderSimple>();//需要获取订单状态队列

        DataEngine de = null;
        private void button1_Click(object sender, EventArgs e)
        {
            is_exit = false;

            XmlDocument doc = new XmlDocument();
            doc.Load(Application.StartupPath + "\\SysConfig.xml");
            XmlElement root = doc.DocumentElement;

            foreach (XmlNode node in root)
            {
                lyConnStr = node.Attributes["lyConn"].Value;
                centerConnStr = node.Attributes["centerConn"].Value;
                Int32.TryParse(node.Attributes["syncTimeSpan"].Value, out syncTimeSpan);
                break;
            }
            if (!string.IsNullOrEmpty(lyConnStr) && !string.IsNullOrEmpty(centerConnStr))
            {
                de = new DataEngine(lyConnStr, centerConnStr);
            }
            //获取分销数据进程
            Thread thGetFXData = new Thread(new ThreadStart(getLYData));
            thGetFXData.Start();
            //获取中心数据进程
            Thread thGetCenterData = new Thread(new ThreadStart(getCenterData));
            thGetCenterData.Start();

            //更新分销系统订单状态
            Thread thUpdateOrderStatus = new Thread(new ThreadStart(changeLYOrderStatus));
            thUpdateOrderStatus.Start();
            //新增支付订单到中心
            Thread thAddOrder = new Thread(new ThreadStart(addOrderToCenter));
            thAddOrder.Start();
        }
        /// <summary>
        /// 获取分销数据
        /// </summary>
        private void getLYData()
        {
            while (!is_exit)
            {
                try
                {
                    DataTable ds = de.LY_queryOrder();
                    if (ds != null && ds.Rows.Count > 0)
                    {
                        foreach (DataRow dr in ds.Rows)//向中心添加订单队列
                        {
                            OrderInfo order = new OrderInfo();
                            order.Saleno = dr["Id"].ToString();
                            order.Ticketcode = order.Saleno.Substring(1);
                            order.Paymode = dr["Paymode"].ToString();
                            order.Tradeno = dr["Tradeno"].ToString();
                            order.Paydate = dr["Paytime"].ToString();
                            order.Groupname = "ly";
                            order.Parkid = Convert.ToInt32(dr["ParkId"].ToString());
                            DataTable dsDetail = de.LY_queryOrderDetail(order.Saleno);//获取整个订单的游客信息

                            order.Plandate = dsDetail.Rows[0]["PlanInParkDate"].ToString();
                            order.Tel = dsDetail.Rows[0]["phone"].ToString();
                            if (dsDetail != null && dsDetail.Rows.Count > 0)
                            {
                                foreach (DataRow drDetail in dsDetail.Rows)//
                                {
                                    DataTable dsTicket = de.LY_queryTicket(drDetail["TicketTypeId"].ToString());//获取票类信息
                                    if (dsTicket != null && dsTicket.Rows.Count > 0)
                                    {
                                        order.Tickettypeidlist += dsTicket.Rows[0]["TicketTypeCode"].ToString() + "-";
                                    }
                                    order.FactPriceList += drDetail["Price"].ToString() + "-";
                                    order.Idlist += drDetail["PId"].ToString() + "-";
                                    order.PersonsamountList += drDetail["Persons"].ToString() + "-";
                                    order.Namelist += "乐游方特-";
                                }
                            }

                            listAdd.Add(order);

                        }
                    }
                    DataTable dt = de.LY_queryUpdateTicket();
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)//需要获取订单状态的队列
                        {
                            OrderSimple os = new OrderSimple();
                            os.Ordersid = dr["OrderId"].ToString();
                            os.Goodsid = dr["TicketTypeCode"].ToString();
                            os.Idnum = dr["PId"].ToString();

                            listGetOrderStatus.Add(os);
                        }
                    }
                }
                catch (Exception ex)
                {
                    de.writeLog("获取乐游方特数据出错：" + ex.Message, 0);
                }
                Thread.Sleep(syncTimeSpan * 1000);
            }
        }

        /// <summary>
        /// 获取中心票务乐游方特改变数据
        /// </summary>
        private void getCenterData()
        {
            while (!is_exit)
            {
                if (listGetOrderStatus.Count > 0)
                {
                    try
                    {
                        OrderSimple os = listGetOrderStatus[0];
                        DataSet ds = de.center_QueryOrderStatus(os.Ordersid, os.Goodsid, os.Idnum);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            int status = Convert.ToInt32(ds.Tables[0].Rows[0]["status"]);
                            if (status > 1)
                            {
                                OrderSimple osC = new OrderSimple();
                                osC.Ordersid = os.Ordersid;
                                osC.Goodsid = os.Goodsid;
                                osC.Idnum = os.Idnum;
                                osC.Status = status;
                                osC.Entertime = ds.Tables[0].Rows[0]["entertime"] == null ? null : ds.Tables[0].Rows[0]["entertime"].ToString();

                                listChangeOrderStatus.Add(osC);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        de.writeLog("获取中心订单状态出错：" + ex.Message, 1);
                    }
                    finally
                    {
                        listGetOrderStatus.RemoveAt(0);
                    }
                }

                //Thread.Sleep(syncTimeSpan * 1000);
            }
        }

        /// <summary>
        /// 向中心添加数据
        /// </summary>
        private void addOrderToCenter()
        {
            while (!is_exit)
            {
                if (listAdd.Count > 0)
                {
                    if (de.center_AddOrder(listAdd[0]) == true)
                    {
                        listAdd.RemoveAt(0);
                    }

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.is_exit = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 更新乐游方特订单状态
        /// </summary>
        private void changeLYOrderStatus()
        {
            while (!is_exit)
            {
                if (listChangeOrderStatus.Count > 0)
                {
                    if (de.LY_updateOrderStatus(listChangeOrderStatus[0].Ordersid, listChangeOrderStatus[0].Idnum, listChangeOrderStatus[0].Goodsid, listChangeOrderStatus[0].Status, listChangeOrderStatus[0].Entertime) == true)
                    {
                        listChangeOrderStatus.RemoveAt(0);
                    }
                }
            }
        }
    }
}
