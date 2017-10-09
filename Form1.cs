using Newtonsoft.Json;
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
        private string centerUrl = "";
        private string apiName = "";
        private string apiPassword = "";
        private string token = "";
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
                centerUrl = node.Attributes["centerUrl"].Value;
                apiName = node.Attributes["apiName"].Value;
                apiPassword = node.Attributes["apiPassword"].Value;
                Int32.TryParse(node.Attributes["syncTimeSpan"].Value, out syncTimeSpan);
                break;
            }
            if (!string.IsNullOrEmpty(lyConnStr) && !string.IsNullOrEmpty(centerUrl) && !string.IsNullOrEmpty(apiName) && !string.IsNullOrEmpty(apiPassword))
            {
                de = new DataEngine(lyConnStr, centerUrl,apiName,apiPassword);
            }
            //获取乐游数据进程
            Thread thGetTokenData = new Thread(new ThreadStart(getToken));
            thGetTokenData.Start();
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
                            order.OrderId = dr["Id"].ToString();
                            order.Paymode = dr["Paymode"].ToString();
                            order.Tradeno = dr["Tradeno"].ToString();
                            order.PayTime = dr["Paytime"].ToString();
                            order.Totalmoney = (decimal)dr["Amount"];
                            order.Parkid = Convert.ToInt32(dr["ParkId"].ToString());

                            DataTable dsDetail = de.LY_queryOrderDetail(order.OrderId);//获取整个订单的游客信息

                            order.Plandate = dsDetail.Rows[0]["PlanInParkDate"].ToString();
                            order.Phone = dsDetail.Rows[0]["phone"].ToString();
                            if (dsDetail != null && dsDetail.Rows.Count > 0)
                            {
                                List<TicketModel> ticketList = new List<TicketModel>();
                                foreach (DataRow drDetail in dsDetail.Rows)//
                                {
                                    TicketModel ticketModel = new TicketModel();
                                    DataTable dsTicket = de.LY_queryTicket(drDetail["TicketTypeId"].ToString());//获取票类信息
                                    if (dsTicket != null && dsTicket.Rows.Count > 0)
                                    {
                                        ticketModel.tickettypeid += dsTicket.Rows[0]["TicketTypeCode"].ToString();
                                    }
                                    ticketModel.name = "乐游方特";
                                    ticketModel.idnum = (drDetail["PId"].ToString() == null ? "" : drDetail["PId"].ToString());
                                    ticketModel.number = (int)drDetail["Persons"];
                                    ticketModel.saleprice = (decimal)drDetail["Price"];

                                    ticketList.Add(ticketModel);
                                }
                                order.TicketList = ticketList;
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
                            os.ordersid = dr["OrderId"].ToString();
                            os.tickettypeid = dr["TicketTypeCode"].ToString();
                            os.idnum = dr["PId"].ToString();
                            os.parkid = (int)dr["ParkId"];

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
                        List<OrderSerchModel> ds = de.center_QueryOrderStatus(token, os.ordersid, os.parkid.ToString(), os.idnum);
                        if (ds != null && ds.Count > 0)
                        {
                            foreach(OrderSerchModel o in ds)
                            {
                                if (o.planstate == 2)
                                {
                                    OrderSimple osC = new OrderSimple();
                                    osC.ordersid = os.ordersid;
                                    osC.tickettypeid = os.tickettypeid;
                                    osC.idnum = os.idnum;
                                    osC.status = o.planstate;
                                    osC.entertime = null;//接口暂未提供

                                    listChangeOrderStatus.Add(osC);
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        de.writeLog("获取中心订单详情出错：" + ex.Message, 1);
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
                    if (de.center_AddOrder(listAdd[0],token) == true)
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
                    if (de.LY_updateOrderStatus(listChangeOrderStatus[0].ordersid, listChangeOrderStatus[0].idnum, listChangeOrderStatus[0].tickettypeid, listChangeOrderStatus[0].status, listChangeOrderStatus[0].entertime) == true)
                    {
                        listChangeOrderStatus.RemoveAt(0);
                    }
                }
            }
        }
        private void getToken()
        {
            while (!is_exit)
            {
                token = de.QueryToken(apiName, apiPassword);

                Thread.Sleep(85 * 60 * 1000);
            }

        }
    }
}
