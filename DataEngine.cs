using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace LY_SyncTicketData
{
    internal class DataEngine
    {
        //���η������ݿ�����ͨ��
        private SqlConnection lyConn = null;

        //����Ʊ�����ݿ�����ͨ��
        private SqlConnection centerConn = null;

        private object objFx = new object();
        private object objCenter = new object();

        internal DataEngine(string lyConnStr, string centerConnStr)
        {
            lyConn = new SqlConnection(lyConnStr);
            centerConn = new SqlConnection(centerConnStr);
        }

        #region �������η���ͬ������

        /// <summary>
        /// ��ȡҪͬ������(����)
        /// </summary>
        /// <returns></returns>
        internal DataTable LY_queryOrder()
        {
            lock (objFx)
            {
                try
                {
                    string nowDate = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                    //��ȡ��һ��ͬ��ʱ��
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
                    

                    //��ȡҪ�·��Ķ�������
                    SqlCommand lyCmd = new SqlCommand();
                    lyCmd.Connection = lyConn;
                    string query_sql = "select * from TicketOrder where Status=1 and IsDelete=0 ";
                    if (lastSyncLastTime != null && lastSyncLastTime != "")
                    {
                        query_sql += " and Convert(varchar(19),PayTime,120)>='" + lastSyncLastTime + "' and Convert(varchar(19),paytime,120)<'" + nowDate + "' ";
                    }
                    lyCmd.CommandText = query_sql;
                    SqlDataAdapter lyadap = new SqlDataAdapter(lyCmd);

                    DataTable ds = new DataTable();
                    lyadap.Fill(ds);

                    //�޸�ͬ��ʱ��
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
                    writeLog("��ѯ���η������ݳ���" + ex.Message, 0);
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
        /// ��ȡҪͬ�����ݣ��������飩
        /// </summary>
        /// <returns></returns>
        internal DataTable LY_queryOrderDetail(string orderId)
        {
            lock (objFx)
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
                    writeLog("��ѯ���η������ݳ���" + ex.Message, 0);
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
        /// ��ȡҪͬ�����ݣ�Ʊ����Ϣ��
        /// </summary>
        /// <returns></returns>
        internal DataTable LY_queryTicket(string ticketId)
        {
            lock (objFx)
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
                    writeLog("��ѯ���η������ݳ���" + ex.Message, 0);
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
        /// ��ȡҪ���µĶ�������
        /// </summary>
        /// <returns></returns>
        internal DataTable LY_queryUpdateTicket()
        {
            lock (objFx)
            {
                try
                {
                    //��ȡ��һ��ͬ��ʱ��
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

                    //��ȡҪ����״̬�Ķ���
                    SqlCommand lyCmd = new SqlCommand();
                    lyCmd.Connection = lyConn;
                    //lyConn.Open();
                    string sql_query = "select t1.OrderId,t1.PId,t2.TicketTypeCode from TicketOrderDetail t1,TicketType t2" +
                                        " where t1.TicketTypeId=t2.Id and (t1.OrderId+cast(t1.TicketTypeId as varchar(10))+cast(t1.PId as varchar(10)) not in "+
                                        " (select OrderId+cast(TicketTypeId as varchar(10))+cast(PId as varchar(10)) from TicketTaken where PId is not null)) "+
                                        " and t1.OrderId in(select Id from TicketOrder where Status=1 and IsDelete=0 and FetchWay=1 ";

                    if(lastSyncLastTime != null && lastSyncLastTime != ""){
                        sql_query += " and Convert(varchar(19),PayTime,120) <='"+lastSyncLastTime+"') ";
                    }else{
                        sql_query += ")";
                    }
                    sql_query += " union all " +
                                 " select t1.OrderId,t1.PId,t2.TicketTypeCode from TicketOrderDetail t1,TicketType t2 " +
                                 " where t1.TicketTypeId=t2.Id and t1.OrderId in(select Id from TicketOrder where Status=1 and IsDelete=0 and FetchWay=2 ";
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
                    writeLog("��ѯ���η������ݳ���" + ex.Message, 0);
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
        /// �������η��ض���״̬
        /// </summary>
        /// <param name="ordersid"></param>
        /// <param name="idnum"></param>
        /// <param name="tickettypeid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        internal bool LY_updateOrderStatus(string ordersid, string idnum, string tickettypeid, int status, string entertime)
        {
            lock (objFx)
            {
                try
                {
                    int seq = 0;
                    string Amount = "";
                    string Persons = "";
                    string Price = "";
                    string TicketTypeId = "";
                    int FetchWay = 0;
                    
                    //���ݶ��������ѯȡƱ��ʽ��1�����֤ȡƱ��2��ȡƱ��ȡƱ
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

                    //���֤ȡƱ
                    if (FetchWay == 1)
                    {
                        //��������
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

                        //���Ҹö��������е�Ʊ�Ƿ���ȡƱ����԰��TicketTaken��TickeOrder��TicketQty��������seq���ж�
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

                        if (seq < ticketQty)//���seqС�ڸö������е�Ʊ���������ȡƱ��
                        {
                            //��ѯ���������
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
                            //����ȡƱ��
                            SqlCommand lyCmd1 = new SqlCommand();
                            lyCmd1.Connection = lyConn;
                            lyCmd1.CommandText = "insert into TicketTaken(OrderId,Seq,Amount,Persons,Price,TicketTypeId,PId,ModifyBy,InputBy,InputTime,Res1)" +
                                                 " values('" + ordersid + "'," + (seq + 1) + "," + Amount + "," + Persons + "," + Price + "," + TicketTypeId + ",'" + idnum + "',0,0,getdate(),'"+entertime+"')";
                            lyCmd1.ExecuteNonQuery();

                            if ((seq + 1) == ticketQty)//���¶���״̬Ϊ�������
                            {
                                SqlCommand lyCmd4 = new SqlCommand();
                                lyCmd4.Connection = lyConn;
                                lyCmd4.CommandText = "update TicketOrder set Status=3,ModifyBy=0 where Id = '" + ordersid + "' and IsDelete=0";
                                lyCmd4.ExecuteNonQuery();
                            }
                        }
                    }
                    else if (FetchWay == 2)//ȡƱ��ȡƱ
                    {
                        //��ѯ���������
                        SqlCommand lyCmdDetail = new SqlCommand();
                        lyCmdDetail.Connection = lyConn;
                        lyCmdDetail.CommandText = "select * from TicketOrderDetail where OrderId='" + ordersid + "'  and IsDelete=0";
                        SqlDataAdapter adap = new SqlDataAdapter(lyCmdDetail);
                        DataSet dsDetail = new DataSet();
                        adap.Fill(dsDetail);
                        //ѭ������ȡƱ������
                        for (int i = 0; i < dsDetail.Tables[0].Rows.Count;i++ )
                        {
                            if (lyConn != null && lyConn.State == ConnectionState.Closed)
                            {
                                lyConn.Open();
                            }
                            //����ȡƱ��
                            SqlCommand lyCmdinsert = new SqlCommand();
                            lyCmdinsert.Connection = lyConn;
                            lyCmdinsert.CommandText = "insert into TicketTaken(OrderId,Seq,Amount,Persons,Price,TicketTypeId,PId,ModifyBy,InputBy,InputTime,Res1)" +
                                                 " values('" + ordersid + "'," + (i + 1) + "," + dsDetail.Tables[0].Rows[i]["Amount"].ToString() + "," + dsDetail.Tables[0].Rows[i]["Persons"].ToString() + "," + dsDetail.Tables[0].Rows[i]["Price"].ToString() + "," + dsDetail.Tables[0].Rows[i]["TicketTypeId"].ToString() + ",'',0,0,getdate(),'"+entertime+"')";
                            lyCmdinsert.ExecuteNonQuery();
                        }
                        //���¶���״̬Ϊ�������
                        SqlCommand lyCmdstatus = new SqlCommand();
                        lyCmdstatus.Connection = lyConn;
                        lyCmdstatus.CommandText = "update TicketOrder set Status=3,ModifyBy=0 where Id = '" + ordersid + "' and IsDelete=0";
                        lyCmdstatus.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    writeLog("�������η��ض���״̬����" + ex.Message, 0);
                }
                finally
                {
                    lyConn.Close();
                }
                return false;
            }
        }

        #endregion

        #region �����������η�������

        /// <summary>
        /// ��ȡ����Ҫͬ������
        /// ��ȡinputtime/modifytime>���ȡ����ʱ�䣬�����Ѿ���԰�����η��ض���
        /// </summary>
        /// <returns></returns>
        internal DataSet center_QueryOrderStatus(string ordersid, string goodsid, string idnum)
        {
            lock (objCenter)
            {
                try
                {
                    SqlCommand zxCmd = new SqlCommand();
                    zxCmd.Connection = centerConn;
                    zxCmd.CommandText = "pr_getFXOrders_StatusChanged";
                    zxCmd.CommandType = CommandType.StoredProcedure;
                    zxCmd.Parameters.Add(new SqlParameter("@ordersid", ordersid));
                    zxCmd.Parameters.Add(new SqlParameter("@goodsid", goodsid));
                    zxCmd.Parameters.Add(new SqlParameter("@idnum", idnum));
                    SqlDataAdapter zxadap = new SqlDataAdapter(zxCmd);

                    DataSet ds = new DataSet();
                    zxadap.Fill(ds);

                    return ds;
                }
                catch (Exception ex)
                {

                    writeLog("��ѯ����Ʊ�����ݳ���" + ex.Message, 1);
                }
                return null;
            }
        }

        /// <summary>
        /// �������ύ����
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
        internal bool center_AddOrder(OrderInfo order)
        {
            lock (objCenter)
            {
                try
                {
                    SqlCommand zxCmd = new SqlCommand();
                    zxCmd.Connection = centerConn;
                    zxCmd.CommandText = "pr_fx_saveplanticket";
                    zxCmd.CommandType = CommandType.StoredProcedure;
                    zxCmd.Parameters.Add(new SqlParameter("@parkid", order.Parkid));
                    zxCmd.Parameters.Add(new SqlParameter("@fangteno", order.Saleno));
                    zxCmd.Parameters.Add(new SqlParameter("@ticketcode", order.Ticketcode));
                    zxCmd.Parameters.Add(new SqlParameter("@plandate", order.Plandate));
                    zxCmd.Parameters.Add(new SqlParameter("@paymode", order.Paymode));
                    zxCmd.Parameters.Add(new SqlParameter("@tradeno", order.Tradeno));
                    zxCmd.Parameters.Add(new SqlParameter("@paydate", order.Paydate));
                    zxCmd.Parameters.Add(new SqlParameter("@tel", order.Tel));
                    zxCmd.Parameters.Add(new SqlParameter("@ticketTypeIdList", order.Tickettypeidlist));
                    zxCmd.Parameters.Add(new SqlParameter("@idlist", order.Idlist));
                    zxCmd.Parameters.Add(new SqlParameter("@factPriceList", order.FactPriceList));
                    zxCmd.Parameters.Add(new SqlParameter("@personsamountList", order.PersonsamountList));
                    zxCmd.Parameters.Add(new SqlParameter("@namelist", order.Namelist));
                    zxCmd.Parameters.Add(new SqlParameter("@groupname", order.Groupname));

                    if (centerConn != null && centerConn.State == ConnectionState.Closed)
                    {
                        centerConn.Open();
                    }

                    zxCmd.ExecuteNonQuery();

                    return true;
                }
                catch (Exception ex)
                {
                    writeLog("����Ʊ����Ӷ�������(" + order.Saleno + ")��" + ex.Message, 1);
                }
                finally
                {
                    centerConn.Close();
                }
                return false;
            }
        }
        #endregion

        /// <summary>
        /// �����־
        /// </summary>
        /// <param name="msg">��־����</param>
        /// <param name="type">��־���� 0���������ݿ���־ 1���������ݿ���־</param>
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
