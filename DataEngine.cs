using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace LY_SyncTicketData
{
    internal class DataEngine
    {
        //乐游方特数据库连接通道
        private SqlConnection lyConn = null;

        //中心票务对接接口基地址
        private string centerUrl = null;
        //中心票务对接账号
        private string apiName = null;
        //中心票务对接密码
        private string apiPassword = null;

        private object objLy = new object();
        private object objCenter = new object();

        internal DataEngine(string lyConnStr, string centerurl, string apiname, string apipassword)
        {
            lyConn = new SqlConnection(lyConnStr);
            centerUrl = centerurl;
            apiName = apiname;
            apiPassword = apipassword;
        }

        #region 处理乐游方特同步数据

        /// <summary>
        /// 获取要同步数据(订单)
        /// </summary>
        /// <returns></returns>
        internal DataTable LY_queryOrder()
        {
            lock (objLy)
            {
                try
                {
                    string nowDate = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                    //获取上一次同步时间
                    string lastSyncLastTime = "";
                    SqlCommand lyCmd1 = new SqlCommand();
                    lyCmd1.Connection = lyConn;
                    lyConn.Open();
                    lyCmd1.CommandText = "select dicValue from  Dictionary where dicKey='lySyncLastTime'";
                    SqlDataReader reader = lyCmd1.ExecuteReader();
                    if (reader.Read())
                    {
                        lastSyncLastTime = (string)reader["dicValue"];
                    }
                    reader.Close();
                    lyConn.Close();
                    

                    //获取要下发的订单数据
                    SqlCommand lyCmd = new SqlCommand();
                    lyCmd.Connection = lyConn;
                    string query_sql = "select * from TicketOrder where Status=1 and IsDelete=0 and (QRCode is null or QRCode = '')";
                    if (lastSyncLastTime != null && lastSyncLastTime != "")
                    {
                        query_sql += " and Convert(varchar(19),PayTime,120)>='" + lastSyncLastTime + "' and Convert(varchar(19),paytime,120)<'" + nowDate + "' ";
                    }
                    lyCmd.CommandText = query_sql;
                    SqlDataAdapter lyadap = new SqlDataAdapter(lyCmd);

                    DataTable ds = new DataTable();
                    lyadap.Fill(ds);

                    //修改同步时间
                    SqlCommand lyCmd2 = new SqlCommand();
                    lyCmd2.Connection = lyConn;
                    lyConn.Open();
                    lyCmd2.CommandText = "update Dictionary set dicValue='" + nowDate + "' where dicKey='lySyncLastTime'";
                    lyCmd2.ExecuteNonQuery();

                    //lyConn.Close();

                    return ds;
                }
                catch (Exception ex)
                {
                    writeLog("查询乐游方特数据出错：" + ex.Message, 0);
                    //lyConn.Close();
                }
                finally
                {
                    lyConn.Close();
                }
                return null;
            }

        }
        /// <summary>
        /// 获取要同步数据（订单详情）
        /// </summary>
        /// <returns></returns>
        internal DataTable LY_queryOrderDetail(string orderId)
        {
            lock (objLy)
            {
                try
                {
                    SqlCommand lyCmd = new SqlCommand();
                    lyCmd.Connection = lyConn;
                    lyConn.Open();
                    lyCmd.CommandText = "select * from TicketOrderDetail where IsDelete=0 and OrderId='" + orderId + "'";
                    SqlDataAdapter lyadap = new SqlDataAdapter(lyCmd);

                    DataTable ds = new DataTable();
                    lyadap.Fill(ds);

                    //lyConn.Close();

                    return ds;
                }
                catch (Exception ex)
                {
                    writeLog("查询乐游方特数据出错：" + ex.Message, 0);
                    //lyConn.Close();
                }
                finally
                {
                    lyConn.Close();
                }
                return null;
            }

        }
        /// <summary>
        /// 获取要同步数据（票类信息）
        /// </summary>
        /// <returns></returns>
        internal DataTable LY_queryTicket(string ticketId)
        {
            lock (objLy)
            {
                try
                {
                    SqlCommand lyCmd = new SqlCommand();
                    lyCmd.Connection = lyConn;
                    lyConn.Open();
                    lyCmd.CommandText = "select * from TicketType where Id=" + ticketId + "";
                    SqlDataAdapter lyadap = new SqlDataAdapter(lyCmd);

                    DataTable ds = new DataTable();
                    lyadap.Fill(ds);

                    //lyConn.Close();

                    return ds;
                }
                catch (Exception ex)
                {
                    writeLog("查询乐游方特数据出错：" + ex.Message, 0);
                    //lyConn.Close();
                }
                finally
                {
                    lyConn.Close();
                }
                return null;
            }
        }
        /// <summary>
        /// 获取要更新的订单数据
        /// </summary>
        /// <returns></returns>
        internal DataTable LY_queryUpdateTicket()
        {
            lock (objLy)
            {
                try
                {
                    //获取上一次同步时间
                    string lastSyncLastTime = "";
                    SqlCommand lyCmd1 = new SqlCommand();
                    lyCmd1.Connection = lyConn;
                    lyConn.Open();
                    lyCmd1.CommandText = "select dicValue from  Dictionary where dicKey='lySyncLastTime'";
                    SqlDataReader reader = lyCmd1.ExecuteReader();
                    if (reader.Read())
                    {
                        lastSyncLastTime = (string)reader["dicValue"];
                    }
                    reader.Close();
                    lyConn.Close();

                    //获取要更新状态的订单
                    SqlCommand lyCmd = new SqlCommand();
                    lyCmd.Connection = lyConn;
                    //lyConn.Open();
                    string sql_query = "select t1.OrderId,t1.PId,t2.TicketTypeCode,t2.ParkId from TicketOrderDetail t1,TicketType t2" +
                                        " where t1.TicketTypeId=t2.Id and (t1.OrderId+cast(t1.TicketTypeId as varchar(10))+cast(t1.PId as varchar(10)) not in "+
                                        " (select OrderId+cast(TicketTypeId as varchar(10))+cast(PId as varchar(10)) from TicketTaken where PId is not null)) "+
                                        " and t1.OrderId in(select Id from TicketOrder where Status=1 and IsDelete=0 and QRCode is not null and QRCode != '' and FetchWay=1 ";
                    sql_query += " and Convert(varchar(19),PayTime,120) >='2016-06-06 00:00:00' ";
                    if(lastSyncLastTime != null && lastSyncLastTime != ""){
                        sql_query += " and Convert(varchar(19),PayTime,120) <='"+lastSyncLastTime+"') ";
                    }else{
                        sql_query += ")";
                    }
                    sql_query += " union all " +
                                 " select t1.OrderId,t1.PId,t2.TicketTypeCode,t2.ParkId from TicketOrderDetail t1,TicketType t2 " +
                                 " where t1.TicketTypeId=t2.Id and t1.OrderId in(select Id from TicketOrder where Status=1 and IsDelete=0 and QRCode is not null and QRCode != '' and FetchWay=2 ";
                    sql_query += " and Convert(varchar(19),PayTime,120) >='2016-06-06 00:00:00' ";
                    if(lastSyncLastTime != null && lastSyncLastTime != ""){
                        sql_query += " and Convert(varchar(19),PayTime,120) <='"+lastSyncLastTime+"') ";
                    }else{
                        sql_query += ")";
                    }

                    lyCmd.CommandText = sql_query;
                    SqlDataAdapter lyadap = new SqlDataAdapter(lyCmd);

                    DataTable ds = new DataTable();
                    lyadap.Fill(ds);

                    //lyConn.Close();

                    return ds;
                }
                catch (Exception ex)
                {
                    writeLog("查询乐游方特数据出错：" + ex.Message, 0);
                    //lyConn.Close();
                }
                finally
                {
                    if(lyConn.State == ConnectionState.Open)
                        lyConn.Close();
                }
                return null;
            }

        }
        /// <summary>
        /// 更新乐游方特订单状态
        /// </summary>
        /// <param name="ordersid"></param>
        /// <param name="idnum"></param>
        /// <param name="tickettypeid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        internal bool LY_updateOrderStatus(string ordersid, string idnum, string tickettypeid, int status, string entertime)
        {
            lock (objLy)
            {
                try
                {
                    int seq = 0;
                    string Amount = "";
                    string Persons = "";
                    string Price = "";
                    string TicketTypeId = "";
                    int FetchWay = 0;
                    
                    //根据订单编码查询取票方式，1：身份证取票；2：取票码取票
                    SqlCommand lyCmdFetch = new SqlCommand();
                    lyCmdFetch.Connection = lyConn;
                    lyCmdFetch.CommandText = "select FetchWay from TicketOrder where Id='" + ordersid + "'  and IsDelete=0";
                    if (lyConn != null && lyConn.State == ConnectionState.Closed)
                    {
                        lyConn.Open();
                    }
                    SqlDataReader readerFetch = lyCmdFetch.ExecuteReader();
                    if (readerFetch.Read())
                    {
                        FetchWay = (int)readerFetch["FetchWay"];
                    }
                    readerFetch.Close();

                    //身份证取票
                    if (FetchWay == 1)
                    {
                        //查找排序
                        SqlCommand lyCmd = new SqlCommand();
                        lyCmd.Connection = lyConn;
                        lyCmd.CommandText = "select seq from TicketTaken where OrderId='" + ordersid + "' and PId='" + idnum + "' and IsDelete=0";
                        if (lyConn != null && lyConn.State == ConnectionState.Closed)
                        {
                            lyConn.Open();
                        }
                        SqlDataReader reader = lyCmd.ExecuteReader();
                        if (reader.Read())
                        {
                            seq = (int)reader["seq"];
                        }
                        reader.Close();

                        //查找该订单下所有的票是否已取票或入园（TicketTaken和TickeOrder的TicketQty），利用seq来判断
                        int ticketQty = 0;
                        SqlCommand lyCmd3 = new SqlCommand();
                        lyCmd3.Connection = lyConn;
                        lyCmd3.CommandText = "select TicketQty from TicketOrder where Id='" + ordersid + "' and IsDelete=0";
                        if (lyConn != null && lyConn.State == ConnectionState.Closed)
                        {
                            lyConn.Open();
                        }
                        SqlDataReader reader3 = lyCmd3.ExecuteReader();
                        if (reader3.Read())
                        {
                            ticketQty = (int)reader3["TicketQty"];
                        }
                        reader3.Close();

                        if (seq < ticketQty)//如果seq小于该订单所有的票数，则插入取票表
                        {
                            //查询订单详情表
                            SqlCommand lyCmd2 = new SqlCommand();
                            lyCmd2.Connection = lyConn;
                            lyCmd2.CommandText = "select * from TicketOrderDetail where OrderId='" + ordersid + "' and PId='" + idnum + "' and IsDelete=0";
                            if (lyConn != null && lyConn.State == ConnectionState.Closed)
                            {
                                lyConn.Open();
                            }
                            SqlDataReader reader2 = lyCmd2.ExecuteReader();
                            if (reader2.Read())
                            {
                                Amount = reader2["Amount"].ToString();
                                Persons = reader2["Persons"].ToString();
                                Price = reader2["Price"].ToString();
                                TicketTypeId = reader2["TicketTypeId"].ToString();
                            }
                            reader2.Close();
                            //插入取票表
                            SqlCommand lyCmd1 = new SqlCommand();
                            lyCmd1.Connection = lyConn;
                            lyCmd1.CommandText = "insert into TicketTaken(OrderId,Seq,Amount,Persons,Price,TicketTypeId,PId,ModifyBy,InputBy,InputTime,Res1)" +
                                                 " values('" + ordersid + "'," + (seq + 1) + "," + Amount + "," + Persons + "," + Price + "," + TicketTypeId + ",'" + idnum + "',0,0,getdate(),'"+entertime+"')";
                            lyCmd1.ExecuteNonQuery();

                            if ((seq + 1) == ticketQty)//更新订单状态为交易完成
                            {
                                SqlCommand lyCmd4 = new SqlCommand();
                                lyCmd4.Connection = lyConn;
                                lyCmd4.CommandText = "update TicketOrder set Status=3,ModifyBy=0 where Id = '" + ordersid + "' and IsDelete=0";
                                lyCmd4.ExecuteNonQuery();
                            }
                        }
                    }
                    else if (FetchWay == 2)//取票码取票
                    {
                        //查询订单详情表
                        SqlCommand lyCmdDetail = new SqlCommand();
                        lyCmdDetail.Connection = lyConn;
                        lyCmdDetail.CommandText = "select * from TicketOrderDetail where OrderId='" + ordersid + "'  and IsDelete=0";
                        SqlDataAdapter adap = new SqlDataAdapter(lyCmdDetail);
                        DataSet dsDetail = new DataSet();
                        adap.Fill(dsDetail);
                        //循环插入取票表数据
                        for (int i = 0; i < dsDetail.Tables[0].Rows.Count;i++ )
                        {
                            if (lyConn != null && lyConn.State == ConnectionState.Closed)
                            {
                                lyConn.Open();
                            }
                            //插入取票表
                            SqlCommand lyCmdinsert = new SqlCommand();
                            lyCmdinsert.Connection = lyConn;
                            lyCmdinsert.CommandText = "insert into TicketTaken(OrderId,Seq,Amount,Persons,Price,TicketTypeId,PId,ModifyBy,InputBy,InputTime,Res1)" +
                                                 " values('" + ordersid + "'," + (i + 1) + "," + dsDetail.Tables[0].Rows[i]["Amount"].ToString() + "," + dsDetail.Tables[0].Rows[i]["Persons"].ToString() + "," + dsDetail.Tables[0].Rows[i]["Price"].ToString() + "," + dsDetail.Tables[0].Rows[i]["TicketTypeId"].ToString() + ",'',0,0,getdate(),'"+entertime+"')";
                            lyCmdinsert.ExecuteNonQuery();
                        }
                        //更新订单状态为交易完成
                        SqlCommand lyCmdstatus = new SqlCommand();
                        lyCmdstatus.Connection = lyConn;
                        lyCmdstatus.CommandText = "update TicketOrder set Status=3,ModifyBy=0 where Id = '" + ordersid + "' and IsDelete=0";
                        lyCmdstatus.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    writeLog("更新乐游方特订单状态出错：" + ex.Message, 0);
                }
                finally
                {
                    lyConn.Close();
                }
                return false;
            }
        }
        /// <summary>
        /// 更新乐游方特取票码和二维码
        /// </summary>
        /// <param name="ordersid"></param>
        /// <param name="ticketcode">取票码</param>
        /// <param name="qrcode">二维码</param>
        /// <returns></returns>
        internal bool LY_updateTicketCode(string ordersid, string ticketcode, string qrcode)
        {
            lock (objLy)
            {
                try
                {

                    SqlCommand lyCmd4 = new SqlCommand();
                    lyCmd4.Connection = lyConn;
                    lyCmd4.CommandText = "update TicketOrder set QRCode='" + ticketcode + "',Res1='" + qrcode + "',ModifyBy=0,ModifyTime=getdate() where Id = '" + ordersid + "' and IsDelete=0";
                    if (lyConn != null && lyConn.State == ConnectionState.Closed)
                    {
                        lyConn.Open();
                    }
                    lyCmd4.ExecuteNonQuery();
                    //当是取票码取票的时候发送短信给游客（身份证入园已在程序中）

                    lyCmd4.CommandText = "select t4.ParkName,t2.TicketTypeName,t2.TicketCenterRemark,t1.Persons,t1.PlanInParkDate,t1.Phone,t3.FetchWay from ( " +
                                                          " select tt.ticketTypeid,sum(tt.Persons) Persons,tt.PlanInParkDate,tt.Phone,tt.OrderId from TicketOrderDetail tt where tt.OrderId='" + ordersid + "' " +
                                                          " group by ticketTypeId,tt.PlanInParkDate,tt.Phone,tt.OrderId) t1 " +
                                                          " left join TicketType t2 on t1.TicketTypeId=t2.Id " +
                                                          " left join TicketOrder t3 on t1.OrderId=t3.Id " +
                                                          " left join Park t4 on t3.ParkId = t4.Id ";
                    SqlDataReader reader = lyCmd4.ExecuteReader();
                    string parkName = "";
                    string phone = "";
                    string planInParkDate = "";
                    int fetchWay = 0;
                    string ticketInfo = "";
                    while (reader.Read())
                    {
                        //string[] format = new string[5];//定义和msg标记数目一致的数组 
                        parkName = (string)reader["ParkName"];
                        phone = (string)reader["Phone"];
                        int persons = (int)reader["Persons"];
                        fetchWay = (int)reader["FetchWay"];
                        string ticketTypeName = (string)reader["TicketTypeName"];
                        string remark = Convert.IsDBNull(reader["TicketCenterRemark"]) ? null : reader["TicketCenterRemark"].ToString();
                        if (!string.IsNullOrEmpty(remark))
                            ticketTypeName += "(" + remark + ")";
                        planInParkDate = ((DateTime)reader["PlanInParkDate"]).ToString("yyyy-MM-dd");

                        ticketInfo += ticketTypeName + persons + "张，";
                    }
                    if (ticketInfo != null && ticketInfo != "")
                    {
                        ticketInfo.Substring(0, ticketInfo.Length - 1);
                    }
                    if (fetchWay == 2)
                    {
                        string ticketMsg = "【方特主题公园】确认码：{0},您已成功通过“乐游APP”预定{1}：{2}，{3}当日有效。入园方式：凭确认码到售票窗口取票。";
                        ticketMsg = string.Format(ticketMsg, ticketcode, parkName, ticketInfo, planInParkDate);

                        Common.SendMessage(phone, ticketMsg);
                    }
                    reader.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    writeLog("更新乐游方特取票码出错：" + ex.Message+",订单编码为："+ordersid, 0);
                }
                finally
                {
                    lyConn.Close();
                }
                return false;
            }
        }
        #endregion

        #region 处理中心乐游方特数据

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <returns></returns>
        internal List<OrderSerchModel> center_QueryOrderStatus(string token, string ordersid, string parkid, string idnum)
        {
            lock (objCenter)
            {
                try
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        token = QueryToken(apiName, apiPassword);
                    }
                    if (!string.IsNullOrEmpty(ordersid) && !string.IsNullOrEmpty(parkid))
                    {
                        string json = "orderid=" + ordersid;
                        json += "&parkid=" + parkid;
                        json += "&idnum=" + idnum;
                        json += "&token=" + token;
                        string result = Common.PostHttpService(centerUrl + "orderdetail/get", json);
                        if (string.IsNullOrEmpty(result))
                        {
                            writeLog("访问订单详情接口失败", 1);
                            return null;
                        }
                        QueryDetailRes resp = JsonConvert.DeserializeObject<QueryDetailRes>(result);
                        if (resp != null)
                        {
                            if (resp.ResultStatus == 6)//token验证失败
                            {
                                token = QueryToken(apiName, apiPassword);

                                json = json.Replace(json.Substring(json.IndexOf("&token="), json.Length - 1), "&token=" + token);
                                result = Common.PostHttpService(centerUrl + "orderdetail/get", json);
                                resp = JsonConvert.DeserializeObject<QueryDetailRes>(result);
                                if (resp.ResultStatus == 0)
                                {
                                    return resp.Data;
                                }
                                else
                                {
                                    writeLog(resp.Message, 1);
                                    return null;
                                }
                            }
                            else if (resp.ResultStatus == 0)
                            {
                                return resp.Data;
                            }
                            else
                            {
                                writeLog(resp.Message, 1);
                                return null;
                            }
                        }
                        else
                        {
                            writeLog("查询订单详情出错,订单编码：" + ordersid, 1);
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    writeLog("查询订单详情出错：" + ex.Message + ",订单编码：" + ordersid, 1);
                }
                return null;
            }
        }

        /// <summary>
        /// 向中心提交订单
        /// </summary>
        /// <param name="parkid"></param>
        /// <param name="saleno"></param>
        /// <param name="ticketcode"></param>
        /// <param name="plandate"></param>
        /// <param name="paymode"></param>
        /// <param name="tradeno"></param>
        /// <param name="paydate"></param>
        /// <param name="tel"></param>
        /// <param name="tickettypeidlist"></param>
        /// <param name="idlist"></param>
        /// <param name="factPriceList"></param>
        /// <param name="personsamountList"></param>
        /// <param name="namelist"></param>
        /// <param name="groupname"></param>
        /// <returns></returns>
        internal bool center_AddOrder(OrderInfo order,string token)
        {
            lock (objCenter)
            {
                try
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        token = QueryToken(apiName,apiPassword);
                    }
                    if (order != null)
                    {

                        string json = "parkid=" + order.Parkid;
                        json += "&plandate=" + order.Plandate;
                        json += "&phone=" + order.Phone;
                        json += "&totalmoney=" + order.Totalmoney;
                        json += "&paymodename=" + order.Paymode;
                        json += "&outorderid=" + order.OrderId;
                        json += "&paydate=" + order.PayTime;
                        json += "&tradeno=" + order.Tradeno;
                        json += "&ticketList=" + Common.ToJsonData(order.TicketList);
                        json += "&token=" + token;
                        string result = Common.PostHttpService(centerUrl + "orderticket/post", json);
                        if (string.IsNullOrEmpty(result))
                        {
                            writeLog("访问下发接口失败", 1);
                            return false;
                        }
                        AddReturnRes resp = JsonConvert.DeserializeObject<AddReturnRes>(result);
                        if (resp != null)
                        {
                            if(resp.ResultStatus == 6)//token验证失败
                            {
                                token = QueryToken(apiName, apiPassword);

                                json = json.Replace(json.Substring(json.IndexOf("&token="),json.Length-1), "&token=" + token);
                                result = Common.PostHttpService(centerUrl + "orderticket/post", json);
                                resp = JsonConvert.DeserializeObject<AddReturnRes>(result);
                                if (resp.ResultStatus == 100 || resp.ResultStatus == 104)//成功或该订单已存在于票务系统
                                {
                                    if(LY_updateTicketCode(order.OrderId, resp.Data.ticketcode, resp.Data.qrcodeimg))
                                        return true;
                                }
                                else
                                {
                                    writeLog(resp.Message, 1);
                                    return false;
                                }
                            }
                            else if (resp.ResultStatus == 100)
                            {
                                if (LY_updateTicketCode(order.OrderId, resp.Data.ticketcode, resp.Data.qrcodeimg))
                                    return true;
                            }
                            else
                            {
                                writeLog(resp.Message, 1);
                                return false;
                            }
                        }
                        else
                        {
                            writeLog("下发订单出错,订单编码：" + order.OrderId, 1);
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    writeLog("下发订单出错：" + ex.Message + ",订单编码：" + order.OrderId, 1);
                }
                return false;
            }
        }
        internal string QueryToken(string apiName, string apiPassword)
        {
            string apiPassword_md5 = EncryptUtils.MD5Encrypt(apiPassword, Encoding.Default);
                try
                {
                    try
                    {
                        File.AppendAllText(Application.StartupPath + "\\Log" + "\\LY_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", "[" + DateTime.Now.ToString() + "] 请求token\r\n");
                    }
                    catch { }
                    string json = "ft_api_name=" + apiName + "&ft_api_password=" + apiPassword_md5;
                    string result = Common.PostHttpService(centerUrl + "token/generate", json);

                    if (string.IsNullOrEmpty(result))
                    {
                        writeLog("获取token失败", 1);
                        return null;
                    }
                    LoginRes resp = JsonConvert.DeserializeObject<LoginRes>(result);
                    if (resp.ResultStatus == 0)
                    {
                        return resp.Data.token;
                    }
                    else
                    {
                        writeLog(resp.Message, 1);
                        return null;
                    }
                }
                catch
                {
                    writeLog("获取token失败", 1);
                }
                return null;
        }
        #endregion

        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="msg">日志内容</param>
        /// <param name="type">日志类型 0：乐游数据库日志 1：中心数据库日志</param>
        internal void writeLog(string msg, int type)
        {
            if (type == 0)
            {
                try
                {
                    File.AppendAllText(Application.StartupPath + "\\Log" + "\\LY_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", "[" + DateTime.Now.ToString() + "] " + msg + "\r\n");
                }
                catch { }
            }
            else
            {
                try
                {
                    File.AppendAllText(Application.StartupPath + "\\Log" + "\\ZX_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", "[" + DateTime.Now.ToString() + "] " + msg + "\r\n");
                }
                catch { }
            }
        }
    }
}
