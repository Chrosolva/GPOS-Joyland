using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MilenialPark.Models;
using MilenialPark.Master;
using System.Data.SqlClient;
using System.Data;

namespace MilenialPark.Controller
{
    public class ControllerTransaction
    {
        #region properties

        public ClsTransaction objTransaction = new ClsTransaction();
        public ClsCard objCard = new ClsCard();
        public ClsTransactionDetail objTransactionDetail = new ClsTransactionDetail();
        public ClsTransactionTiketDetail objTransactionTiketDetail = new ClsTransactionTiketDetail();
        public ClsExtend objExtend = new ClsExtend();
        public string query;
        public string query2;
        public DataTable dt;
        public DataRow dr;
        public string TransactionID;
        public string TransactionDetailID;

        #endregion

        #region constructor

        public ControllerTransaction()
        {

        }

        #endregion

        #region function

        public void AutogenereateTransactionID (string type, string ID)
        {
            if(type == "KREDIT")
            {
                query = $"Select top 1 TransactionID from WHNPOS.dbo.Transaksi where TransactionID like 'TRK.{ID}%'  Order By TransactionID desc";  
                dt = ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
                if (dt.Rows.Count > 0)
                {
                    int tmp = Convert.ToInt32(dt.Rows[0]["TransactionID"].ToString().Substring(dt.Rows[0]["TransactionID"].ToString().Length - 6, 6));
                    TransactionID = "TRK." + ID + "-" + DateTime.Now.Year.ToString().Substring(2, 2) + "-" + (tmp + 1).ToString("D6");
                }
                else
                {
                    TransactionID = "TRK." + ID + "-" + DateTime.Now.Year.ToString().Substring(2, 2) + "-" + 1.ToString("D6");
                }
            }
            else if(type == "DEBIT")
            {
                query = $"Select top 1 TransactionID from WHNPOS.dbo.Transaksi where TransactionID like 'TRD.{ID}%'  Order By TransactionID desc";
                dt = ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
                if (dt.Rows.Count > 0)
                {
                    int tmp = Convert.ToInt32(dt.Rows[0]["TransactionID"].ToString().Substring(dt.Rows[0]["TransactionID"].ToString().Length - 6, 6));
                    TransactionID = "TRD." + ID + "-" + DateTime.Now.Year.ToString().Substring(2, 2) + "-" + (tmp + 1).ToString("D6");
                }
                else
                {
                    TransactionID = "TRD." + ID + "-" + DateTime.Now.Year.ToString().Substring(2, 2) + "-" + 1.ToString("D6");
                }
            }
            else if(type == "REFUND")
            {
                query = $"Select top 1 TransactionID from WHNPOS.dbo.Transaksi where TransactionID like 'TRR.{ID}%'  Order By TransactionID desc";
                dt = ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
                if (dt.Rows.Count > 0)
                {
                    int tmp = Convert.ToInt32(dt.Rows[0]["TransactionID"].ToString().Substring(dt.Rows[0]["TransactionID"].ToString().Length - 6, 6));
                    TransactionID = "TRR." + ID + "-" + DateTime.Now.Year.ToString().Substring(2, 2) + "-" + (tmp + 1).ToString("D6");
                }
                else
                {
                    TransactionID = "TRR." + ID + "-" + DateTime.Now.Year.ToString().Substring(2, 2) + "-" + 1.ToString("D6");
                }
            }
            else if(type == "CANCEL")
            {
                query = $"Select top 1 TransactionID from WHNPOS.dbo.Transaksi where TransactionID like 'TRC.{ID}%'  Order By TransactionID desc";
                dt = ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
                if (dt.Rows.Count > 0)
                {
                    int tmp = Convert.ToInt32(dt.Rows[0]["TransactionID"].ToString().Substring(dt.Rows[0]["TransactionID"].ToString().Length - 6, 6));
                    TransactionID = "TRC." + ID + "-" + DateTime.Now.Year.ToString().Substring(2, 2) + "-" + (tmp + 1).ToString("D6");
                }
                else
                {
                    TransactionID = "TRC." + ID + "-" + DateTime.Now.Year.ToString().Substring(2, 2) + "-" + 1.ToString("D6");
                }
            }
            else if(type == "TICKET")
            {
                query = $"Select top 1 TransactionID from WHNPOS.dbo.Transaksi where TransactionID like 'TRT.{ID}%'  Order By TransactionID desc";
                dt = ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
                if (dt.Rows.Count > 0)
                {
                    int tmp = Convert.ToInt32(dt.Rows[0]["TransactionID"].ToString().Substring(dt.Rows[0]["TransactionID"].ToString().Length - 6, 6));
                    TransactionID = "TRT." + ID + "-" + DateTime.Now.Year.ToString().Substring(2, 2) + "-" + (tmp + 1).ToString("D6");
                }
                else
                {
                    TransactionID = "TRT." + ID + "-" + DateTime.Now.Year.ToString().Substring(2, 2) + "-" + 1.ToString("D6");
                }
            }
        } 

        public DataTable getCard(string CardID)
        {
            query = "Select * from WHNPOS.dbo.TblCard where CardID = " + ClsFungsi.C2Q(CardID);
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public void SetCard(string CardID)
        {
            dt = this.getCard(CardID);
            if(dt.Rows.Count > 0)
            {
                this.objCard = new ClsCard(dt.Rows[0]["CardID"].ToString(), dt.Rows[0]["CustomerName"].ToString(), dt.Rows[0]["NoIdentitas"].ToString() ?? "", Convert.ToDecimal(dt.Rows[0]["Saldo"]), Convert.ToBoolean(dt.Rows[0]["Active"]));
            }
        }

        public DataTable getTransactHistory (string ShopID, DateTime from , DateTime to)
        {
            query = $" Select * from WHNPOS.dbo.Transaksi where ShopID = {ClsFungsi.C2Q(ShopID)} " +
                    $" and TransactionDate >= {ClsFungsi.C2QTime(Convert.ToDateTime(from.ToString("dd/M/yyyy HH:mm:ss tt")))} and TransactionDate <= {ClsFungsi.C2QTime(Convert.ToDateTime(to.ToString("dd/M/yyyy HH:mm:ss tt")))} " +
                    " Order By TransactionDate desc;";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable getTransactTiketHistory(string ShopID, DateTime from, DateTime to, string Value, string Option)
        {
            query = $" Select * from WHNPOS.dbo.Transaksi where ShopID = {ClsFungsi.C2Q(ShopID)} and TransactionID like '%TRT%'  " +
                    $" and TransactionDate >= {ClsFungsi.C2QTime(Convert.ToDateTime(from.ToString("dd/M/yyyy HH:mm:ss tt")))} and TransactionDate <= {ClsFungsi.C2QTime(Convert.ToDateTime(to.ToString("dd/M/yyyy HH:mm:ss tt")))} and {Option} like '%{Value}%' " +
                    " Order By TransactionDate desc;";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable getTransaction(string ShopID, DateTime from, DateTime to, string Value, string Option, string TransactionType, string ID)
        {
            query = $" Select TR.TransactionID, TR.KodeCabang, BR.NamaCabang , TR.TransactionDate, TR.TotalAmount, TR.PaymentType, TR.CardID, TR.ShopID, TR.TransactionStatus, TR.TransactionType , TR.Remarks, " +
                    " TR.Subtotal, TR.InitialBalance, TR.FInalBalance from WHNPOS.dbo.Transaksi as TR left join WHNPOS.dbo.TblCabang as BR on TR.KodeCabang = BR.KodeCabang " + 
                    $" where TR.ShopID = {ClsFungsi.C2Q(ShopID)} and TR.TransactionID like '%{ID}%' and TR.TransactionType like {ClsFungsi.C2Q(TransactionType)} " +
                    $" and TR.TransactionDate >= {ClsFungsi.C2QTime(Convert.ToDateTime(from.ToString("dd/M/yyyy HH:mm:ss tt")))} and TR.TransactionDate <= {ClsFungsi.C2QTime(Convert.ToDateTime(to.ToString("dd/M/yyyy HH:mm:ss tt")))} and TR.{Option} like '%{Value}%' " +
                    " Order By TR.TransactionDate desc;";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable getTransaction2(string ShopID, DateTime from, DateTime to, string Value, string Option, string TransactionType)
        {
            query = $" Select TR.TransactionID, TR.KodeCabang, BR.NamaCabang , TR.TransactionDate, TR.TotalAmount, TR.PaymentType, TR.CardID, TR.ShopID, TR.TransactionStatus, TR.TransactionType , TR.Remarks, " +
                    " TR.Subtotal, TR.InitialBalance, TR.FInalBalance from WHNPOS.dbo.Transaksi as TR left join WHNPOS.dbo.TblCabang as BR on TR.KodeCabang = BR.KodeCabang  " +
                    $" where TR.ShopID = {ClsFungsi.C2Q(ShopID)} and TR.TransactionType like {ClsFungsi.C2Q(TransactionType)} " +
                    $" and TR.TransactionDate >= {ClsFungsi.C2QTime(Convert.ToDateTime(from.ToString("dd/M/yyyy HH:mm:ss tt")))} and TR.TransactionDate <= {ClsFungsi.C2QTime(Convert.ToDateTime(to.ToString("dd/M/yyyy HH:mm:ss tt")))} and TR.{Option} like '%{Value}%' " +
                    " Order By TR.TransactionDate desc;";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable gettransactionDetail (string TransactionID)
        {
            query = $" Select Nourut, ItemID , ItemName, Price, Qty, OrderStatus from WHNPOS.dbo.TransaksiDetail where TransactionID = {ClsFungsi.C2Q(TransactionID)} Order By NoUrut asc";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable gettransactionTiketDetail(string TransactionID)
        {
            query = $" Select TRD.*, isNULL(SHT.Category, 'ACTIVITY') as category from WHNPOS.dbo.TransaksiTiketDetail as TRD left join WHNPOS.dbo.ShopItemTiket as SHT " +
                    $" on TRD.ItemID = SHT.ItemID where TRD.TransactionID = {ClsFungsi.C2Q(TransactionID)} Order By TRD.NoUrut asc";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable GetCombinedTransactionDetails(
    string transactionID)
        {
            query =
                "SELECT " +
                "    TTD.TransactionID, " +
                "    TTD.TransactionDate, " +
                "    TTD.ItemID, " +
                "    TTD.ItemName, " +
                "    ISNULL(TTD.Price, 0) AS Price, " +
                "    ISNULL(TTD.Qty, 0) AS Qty, " +
                "    ISNULL(TTD.NoUrut, 0) AS NoUrut, " +
                "    ISNULL(TTD.OrderStatus, '') AS OrderStatus, " +
                "    TTD.JamMasuk, " +
                "    TTD.JamKeluar, " +
                "    ISNULL(TTD.WaktuBermain, 0) AS WaktuBermain, " +
                "    ISNULL(TTD.Toleransi, 0) AS Toleransi, " +
                "    ISNULL(TTD.RFID, '') AS RFID, " +
                "    ISNULL(TTD.TagID, '') AS TagID, " +
                "    ISNULL(TTD.Keterangan, '') AS Keterangan, " +
                "    ISNULL(SIT.Category, 'ACTIVITY') AS Category, " +
                "    'TICKET' AS DetailType " +
                "FROM WHNPOS.dbo.TransaksiTiketDetail TTD " +
                "LEFT JOIN WHNPOS.dbo.ShopItemTiket SIT " +
                "    ON TTD.ItemID = SIT.ItemID " +
                "WHERE TTD.TransactionID = " +
                ClsFungsi.C2Q(transactionID) + " " +

                "UNION ALL " +

                "SELECT " +
                "    TD.TransactionID, " +
                "    TD.TransactionDate, " +
                "    TD.ItemID, " +
                "    TD.ItemName, " +
                "    ISNULL(TD.Price, 0) AS Price, " +
                "    ISNULL(TD.Qty, 0) AS Qty, " +
                "    ISNULL(TD.NoUrut, 0) AS NoUrut, " +
                "    ISNULL(TD.OrderStatus, '') AS OrderStatus, " +
                "    CAST(NULL AS DATETIME) AS JamMasuk, " +
                "    CAST(NULL AS DATETIME) AS JamKeluar, " +
                "    0 AS WaktuBermain, " +
                "    0 AS Toleransi, " +
                "    '' AS RFID, " +
                "    '' AS TagID, " +
                "    '' AS Keterangan, " +
                "    'ITEM' AS Category, " +
                "    'ITEM' AS DetailType " +
                "FROM WHNPOS.dbo.TransaksiDetail TD " +
                "WHERE TD.TransactionID = " +
                ClsFungsi.C2Q(transactionID) + " " +

                "ORDER BY NoUrut, DetailType DESC";

            return ClsStaticVariable.objConnection
                .objsqlconnection
                .Filldatatable(query);
        }

        public DataTable getOnetransactionTiketDetail(string TransactionID, int NoUrut)
        {
            query = $" Select TRD.*, isNULL(SHT.Category, 'ACTIVITY') as category from WHNPOS.dbo.TransaksiTiketDetail as TRD left join WHNPOS.dbo.ShopItemTiket as SHT " +
                    $" on TRD.ItemID = SHT.ItemID where TRD.TransactionID = {ClsFungsi.C2Q(TransactionID)} and TRD.NoUrut = {ClsFungsi.C2Q(NoUrut)} Order By TRD.NoUrut asc";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable gettransactionDetail2(string TransactionID)
        {
            query = $" Select * from WHNPOS.dbo.GetUnionTransaction() where TransactionID = {ClsFungsi.C2Q(TransactionID)} Order By NoUrut asc";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable getTop1Tiket (string CardID, string OrderStatus, string OrderStatus2, DateTime start, DateTime end)
        {
            query = $"Select * from WHNPOS.dbo.Top1TiketToday({ClsFungsi.C2Q(CardID)}, {ClsFungsi.C2Q(OrderStatus)}, {ClsFungsi.C2Q(OrderStatus2)}, {ClsFungsi.C2QTime(start)}, {ClsFungsi.C2QTime(end)})";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable getTiketwithTID(string TransactionID, string OrderStatus, int NoUrut, DateTime start, DateTime end)
        {
            query = $"Select * from WHNPOS.dbo.Top1TiketTodayQR({ClsFungsi.C2Q(TransactionID)}, {ClsFungsi.C2Q(NoUrut)}, {ClsFungsi.C2Q(OrderStatus)},  " + 
                    $" {ClsFungsi.C2QTime(start)} ,  {ClsFungsi.C2QTime(end)})";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable get1TicketDetail(string TransactionID, int NoUrut, DateTime Start, DateTime End)
        {
            query = $"Select * from TransaksiTiketDetail where TransactionID = {ClsFungsi.C2Q(TransactionID)} and NoUrut = {ClsFungsi.C2Q(NoUrut)} and TransactionDate >= {ClsFungsi.C2QTime(Start)} and TransactionDate <= {ClsFungsi.C2QTime(End)}";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable checkOvertimetoday(string CardID, DateTime Start, DateTime End)
        {
            query = $" Select TR.TransactionID, TR.CardID, TR.TransactionDate, TRD.ItemID, TRD.ItemName, TRD.Price, " + 
                    " TRD.Qty, TRD.NoUrut, TRD.OrderStatus, TRD.JamMasuk, TRD.JamKeluar, TRD.WaktuBermain, TRD.Toleransi " +  
                    " from WHNPOS.dbo.Transaksi as TR left join WHNPOS.dbo.TransaksiTiketDetail as TRD on TR.TransactionID = TRD.TransactionID " + 
                    $" where TR.CardID = {ClsFungsi.C2Q(CardID)} and TR.TransactionID like '%TRT%'  and TR.TransactionDate >= {ClsFungsi.C2QTime(Start)}" + 
                    $" and TR.TransactionDate <= {ClsFungsi.C2QTime(End)} and TRD.OrderStatus like '%OVERTIME%' and TR.TransactionType = 'TICKET'" + 
                    $" Order By  TRD.OrderStatus, TRD.TransactionID, TRD.JamMasuk";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public void UpdateOrderStatusTiket(string TransactionID, int NoUrut, string OrderStatus)
        {
            query = $"Update WHNPOS.dbo.TransaksiTiketDetail set OrderStatus = {ClsFungsi.C2Q(OrderStatus)} where TransactionID = {ClsFungsi.C2Q(TransactionID)} and NoUrut = {ClsFungsi.C2Q(NoUrut)}";
            ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
            string logmessage = $"Update  OrderStatus = {OrderStatus} for TransactionID = {TransactionID} and NoUrut = {NoUrut} ";
            query = $"Insert Into WHNPOS.dbo.Datalog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(TransactionID)}, {ClsFungsi.C2Q(logmessage)},'INFO' )";
            ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
        }

        public void UpdateOrderStatusTiketandTime (string TransactionID, int NoUrut, int WaktuBermain, int Toleransi, string OrderStatus)
        {
            DateTime now = DateTime.Now;
            DateTime end = now.AddHours(WaktuBermain);
            end = end.AddMinutes(Toleransi);
            query = $"Update WHNPOS.dbo.TransaksiTiketDetail set JamMasuk = {ClsFungsi.C2QTime(now)} , JamKeluar = {ClsFungsi.C2QTime(end)}, OrderStatus = {ClsFungsi.C2Q(OrderStatus)} where TransactionID = {ClsFungsi.C2Q(TransactionID)} and NoUrut = {ClsFungsi.C2Q(NoUrut)}";
            ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
            string logmessage = $"Update Jam Masuk  = {now.ToString()} , Jam Keluar = {end.ToString()} OrderStatus = {OrderStatus} for TransactionID = {TransactionID} and NoUrut = {NoUrut} ";
            query = $"Insert Into WHNPOS.dbo.Datalog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(TransactionID)}, {ClsFungsi.C2Q(logmessage)},'INFO' )";
            ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
        }

        public void setOneTransaction(string TransactionID)
        {
            query = "Select * from WHNPOS.dbo.Transaksi where TransactionID = " + ClsFungsi.C2Q(TransactionID);
            dt = ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query); 
            if(dt.Rows.Count != 0 )
            {
                this.objTransaction = new ClsTransaction(dt.Rows[0]["TransactionID"].ToString(), Convert.ToDateTime(dt.Rows[0]["TransactionDate"]), Convert.ToDecimal(dt.Rows[0]["TotalAmount"]), dt.Rows[0]["PaymentType"].ToString(), dt.Rows[0]["CardID"].ToString(), dt.Rows[0]["ShopID"].ToString(), dt.Rows[0]["Remarks"].ToString(), Convert.ToDecimal(dt.Rows[0]["Subtotal"]), Convert.ToDecimal(dt.Rows[0]["PPN"]), Convert.ToDecimal(dt.Rows[0]["InitialBalance"]), Convert.ToDecimal(dt.Rows[0]["FinalBalance"]), dt.Rows[0]["TransactionStatus"].ToString());
            }
        }

        public DataTable checkovertimetoday(string TransactionID , string CardID, DateTime from, DateTime end)
        {
            query = " Select TR.TransactionID, TR.CardID, TR.TransactionDate, TRD.ItemID, TRD.ItemName, TRD.Price, " +
                    " TRD.Qty, TRD.NoUrut, TRD.OrderStatus, TRD.JamMasuk, TRD.JamKeluar, TRD.WaktuBermain, TRD.Toleransi " + 
                    $" from WHNPOS.dbo.Transaksi as TR left join WHNPOS.dbo.TransaksiTiketDetail as TRD on TR.TransactionID = TRD.TransactionID " + 
                    $" where TR.CardID = {ClsFungsi.C2Q(CardID)} and TR.TransactionID like '%TRT%' and TR.TransactionDate >= {ClsFungsi.C2QTime(from)} " + 
                    $" and TR.TransactionDate <= {ClsFungsi.C2QTime(end)} " + 
                    " Order By  TRD.OrderStatus, TRD.TransactionID, TRD.JamMasuk";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable checkCategory(string ItemID)
        {
            query = "Select TRD.*, isNULL(SHT.Category, 'ACTIVITY') as category from WHNPOS.dbo.TransaksiTiketDetail as TRD " +
                    " left join WHNPOS.dbo.ShopItemTiket as SHT " +
                    $" on TRD.ItemID = SHT.ItemID where TRD.ItemID = {ClsFungsi.C2Q(ItemID)}";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable checkCategory2(string ItemID)
        {
            query = $" Select * from WHNPOS.dbo.ShopItemTIket where ItemID = {ClsFungsi.C2Q(ItemID)}";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        #endregion

        #region CRUD

        public string InsertTransaction(ClsTransaction trans, ClsCard card)
        {
            ClsStaticVariable.sukses = false;
            // Cut Card Balance 
            decimal finalbalance = 0;
            if (trans.PaymentType == "CARD")
            {
                finalbalance = card.Saldo - trans.totalAmount;
            }
            else
            {
                finalbalance = 0;
            }
            if (trans.PaymentType == "CARD" && finalbalance < 0)
            {
                return "Data Transaksi Gagal ditambah karena saldo kartu tidak mencukupi !!!";
            }
            else
            {
                trans.finalBalance = finalbalance;


                string logmessage = "1. Adding Transaction ID = " + trans.TransactionID + " Into Database, Date = " + trans.TransactionDate.ToString() + "  , Final Balance = " + finalbalance + " , Card ID = " + card.CardID + " , ShopID = " + trans.ShopId + " , TotalAmount = " + trans.totalAmount.ToString();
                string queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                //Insert Transaction

                query = " Insert Into WHNPOS.dbo.Transaksi " +
                    " (TransactionID, TransactionDate, TotalAmount, PaymentType, CardID, ShopID, Remarks, Subtotal, PPN, InitialBalance, FinalBalance, TransactionStatus, TransactionType, KodeCabang) " +
                    " values " +
                    $" ({ClsFungsi.C2Q(trans.TransactionID)}, GETDATE() , {ClsFungsi.C2Q(trans.totalAmount)}, {ClsFungsi.C2Q(trans.PaymentType)}, {ClsFungsi.C2Q(trans.CardID)}, {ClsFungsi.C2Q(trans.ShopId)}, {ClsFungsi.C2Q(trans.Remarks)}, {ClsFungsi.C2Q(trans.Subtotal)}, {ClsFungsi.C2Q(trans.PPN)}, {ClsFungsi.C2Q(trans.InitialBalance)}, {ClsFungsi.C2Q(trans.finalBalance)}, {ClsFungsi.C2Q("PAID")}, {ClsFungsi.C2Q(trans.TransactionType)}, {ClsFungsi.C2Q(ClsStaticVariable.KodeBranch)})";

                try
                {
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
                    ClsStaticVariable.sukses = true;
                    logmessage = "2. Transaction ADD SUCCESS" + " query = " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                }
                catch (Exception ex)
                {
                    logmessage = "2. Transaction ADD FAILED , error Message = " + ex.Message + " query = " + query; 
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi Gagal Ditambah !!! error message = " + ex.Message;
                }
                
                //Insert Detail
                ClsStaticVariable.sukses = true;

                if (ClsStaticVariable.sukses)
                {
                    ClsStaticVariable.sukses = false;
                    if (trans.listtransdet.Count != 0)
                    {
                        int index = 1;
                        foreach (ClsTransactionDetail transdet in trans.listtransdet)
                        {
                            query2 = " Insert Into WHNPOS.dbo.TransaksiDetail " +
                                     " (TransactionID, TransactionDate, ItemID, ItemName, Price, Qty, NoUrut, OrderStatus) " +
                                     " values " +
                                     $"({ClsFungsi.C2Q(transdet.TransactionID)}, GETDATE(), {ClsFungsi.C2Q(transdet.ItemId)}, {ClsFungsi.C2Q(transdet.ItemName)}, {ClsFungsi.C2Q(transdet.Price)}, {ClsFungsi.C2Q(transdet.Qty)}, {ClsFungsi.C2Q(index)}, {ClsFungsi.C2Q(transdet.OrderStatus)})";

                            index++;
                            try
                            {
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query2);
                                logmessage = $"3. ADD NOUrut = {(index -1).ToString()},  ItemID =  {transdet.ItemId}, ItemName = {transdet.ItemName}, Qty = {transdet.Qty}, Price = {transdet.Price} SUCCESS" + " query = " + query2;
                                queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                            }
                            catch (Exception ex)
                            {
                                logmessage = "3. TransactionDetail ADD FAILED , ID = " + transdet.TransactionID + " error message = " + ex.Message + " query = " + query2;
                                queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                                return "Data Transaksi Detail Gagal Ditambah !!! error message = " + ex.Message;
                            }
                        }

                        logmessage = "3. Transaction Detail ADD SUCCESS";
                        queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                        ClsStaticVariable.sukses = true;
                    }
                    else
                    {
                        ClsStaticVariable.sukses = true;
                    }
                }

                else
                {
                    ClsStaticVariable.sukses = false;
                }

                // Update Balance
                if (ClsStaticVariable.sukses)
                {
                    
                    query = $" Update WHNPOS.dbo.TblCard set Saldo = {ClsFungsi.C2Q(finalbalance)} where CardID = {ClsFungsi.C2Q(card.CardID)}";
                    try
                    {
                        logmessage = "4. EDIT BALANCE query =  " + query;
                        queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
                        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
                        ClsStaticVariable.sukses = true;

                        logmessage = "4. Transaction ADD SUCCESS, DETAIL ADD SUCCESS, BALANCE ADD SUCCESS";
                        queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                        return "Data Transaksi dan Detail Berhasil ditambah !!! Saldo Kartu berhasil di potong!!!";
                    }
                    catch (Exception ex)
                    {
                        logmessage = "4. BALANCE UPDATE FAILED , error message = " + ex.Message;
                        queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                        return "Data Transaksi Gagal Ditambah !!! error message = " + ex.Message;
                    }

                }
                else
                {
                    logmessage = "5. !!!Transaction ADD FAILED ";
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi Gagal Ditambah !!! ";
                }


            }


        }

        public void InsertLogMessage(string TransactionID, string Message)
        {
            string logmessage = Message;
            string queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("PRECHECK")})";
            ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
        }

        public string InsertTransactionTicket(
    ClsTransaction trans,
    ClsCard card)
        {
            ClsStaticVariable.sukses = false;

            #region Validasi awal

            if (trans == null)
            {
                return "Data transaksi tidak tersedia.";
            }

            if (string.IsNullOrWhiteSpace(trans.TransactionID))
            {
                return "TransactionID tidak tersedia.";
            }

            if (string.IsNullOrWhiteSpace(trans.ShopId))
            {
                return "ShopID transaksi tidak tersedia.";
            }

            if (string.IsNullOrWhiteSpace(trans.PaymentType))
            {
                return "Payment Type belum ditentukan.";
            }

            if (card == null)
            {
                return "Data kartu tidak tersedia.";
            }

            if (trans.listtranstikdet == null)
            {
                trans.listtranstikdet =
                    new List<ClsTransactionTiketDetail>();
            }

            bool isMasterCard =
                string.Equals(
                    trans.PaymentType,
                    "MASTER_CARD",
                    StringComparison.OrdinalIgnoreCase
                );

            if (!isMasterCard &&
                string.IsNullOrWhiteSpace(card.CardID))
            {
                return "CardID tidak tersedia.";
            }

            // Pastikan seluruh detail memakai TransactionID header.
            foreach (
                ClsTransactionTiketDetail detail
                in trans.listtranstikdet)
            {
                if (detail == null)
                {
                    return "Ditemukan detail tiket yang tidak valid.";
                }

                detail.TransactionID =
                    trans.TransactionID;

                if (detail.NoUrut <= 0)
                {
                    return
                        "NoUrut tiket tidak valid untuk item " +
                        detail.ItemName +
                        ".";
                }

                if (string.IsNullOrWhiteSpace(detail.ItemId))
                {
                    return
                        "ItemID tiket tidak tersedia pada NoUrut " +
                        detail.NoUrut +
                        ".";
                }

                if (detail.WaktuBermain > 0 &&
                    string.IsNullOrWhiteSpace(detail.TagID))
                {
                    return
                        "TagID RFID belum diisi pada tiket NoUrut " +
                        detail.NoUrut +
                        ".";
                }

                if (detail.WaktuBermain > 0 &&
                    string.IsNullOrWhiteSpace(detail.RFID))
                {
                    return
                        "Nama/nomor RFID belum diisi pada tiket NoUrut " +
                        detail.NoUrut +
                        ".";
                }
            }

            // Pastikan TagID tidak duplikat dalam transaksi yang sama.
            List<string> duplicateTagIDs =
                trans.listtranstikdet
                    .Where(x =>
                        x != null &&
                        !string.IsNullOrWhiteSpace(x.TagID))
                    .GroupBy(
                        x => x.TagID.Trim(),
                        StringComparer.OrdinalIgnoreCase
                    )
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToList();

            if (duplicateTagIDs.Count > 0)
            {
                return
                    "TagID RFID duplikat ditemukan: " +
                    string.Join(", ", duplicateTagIDs);
            }

            #endregion

            #region Hitung saldo

            decimal finalBalance;

            if (isMasterCard)
            {
                // MASTER_CARD tidak memotong saldo kartu.
                finalBalance = card.Saldo;
            }
            else
            {
                finalBalance =
                    card.Saldo - trans.totalAmount;
            }

            if (!isMasterCard && finalBalance < 0)
            {
                InsertDataLogSafe(
                    trans.TransactionID,
                    "PRECHECK FAILED: Saldo kartu tidak mencukupi. " +
                    "Saldo=" + card.Saldo +
                    ", Total=" + trans.totalAmount +
                    ", FinalBalance=" + finalBalance,
                    "ERROR"
                );

                return
                    "Data transaksi gagal ditambah karena " +
                    "saldo kartu tidak mencukupi.";
            }

            trans.finalBalance =
                finalBalance;

            #endregion

            string logMessage =
                "1. Adding Transaction. " +
                "TransactionID=" + trans.TransactionID +
                ", Date=" + trans.TransactionDate +
                ", InitialBalance=" + card.Saldo +
                ", FinalBalance=" + finalBalance +
                ", CardID=" + card.CardID +
                ", ShopID=" + trans.ShopId +
                ", TotalAmount=" + trans.totalAmount +
                ", PaymentType=" + trans.PaymentType;

            InsertDataLogSafe(
                trans.TransactionID,
                logMessage,
                "INFO"
            );

            InsertDataLogSafe(trans.TransactionID,
                "DETAIL COUNT BEFORE INSERT. " +
                "TicketCount=" +
                (
                    trans.listtranstikdet == null
                        ? 0
                        : trans.listtranstikdet.Count
                ) +
                ", OrdinaryCount=" +
                (
                    trans.listtransdet == null
                        ? 0
                        : trans.listtransdet.Count
                ),
                "INFO"
            );

            #region Insert header transaksi

            query =
                "INSERT INTO WHNPOS.dbo.Transaksi " +
                "(" +
                "TransactionID, " +
                "TransactionDate, " +
                "TotalAmount, " +
                "PaymentType, " +
                "CardID, " +
                "ShopID, " +
                "Remarks, " +
                "Subtotal, " +
                "PPN, " +
                "InitialBalance, " +
                "FinalBalance, " +
                "TransactionStatus, " +
                "TransactionType, " +
                "KodeCabang" +
                ") VALUES (" +
                ClsFungsi.C2Q(trans.TransactionID) + ", " +
                "GETDATE(), " +
                ClsFungsi.C2Q(trans.totalAmount) + ", " +
                ClsFungsi.C2Q(trans.PaymentType) + ", " +
                ClsFungsi.C2Q(trans.CardID) + ", " +
                ClsFungsi.C2Q(trans.ShopId) + ", " +
                ClsFungsi.C2Q(trans.Remarks) + ", " +
                ClsFungsi.C2Q(trans.Subtotal) + ", " +
                ClsFungsi.C2Q(trans.PPN) + ", " +
                ClsFungsi.C2Q(trans.InitialBalance) + ", " +
                ClsFungsi.C2Q(trans.finalBalance) + ", " +
                ClsFungsi.C2Q("PAID") + ", " +
                ClsFungsi.C2Q(trans.TransactionType) + ", " +
                ClsFungsi.C2Q(ClsStaticVariable.KodeBranch) +
                ")";

            try
            {
                int affectedHeader =
                    ClsStaticVariable.objConnection
                        .objSqlServerIUDClass
                        .ExecuteNonQuery(query);

                InsertDataLogSafe(
                    trans.TransactionID,
                    "2. Transaction ADD SUCCESS. " +
                    "ExecuteNonQueryResult=" + affectedHeader +
                    ", Query=" + query,
                    "SUCCESS"
                );

                // Jangan gunakan affectedHeader <= 0.
                // Wrapper dapat mengembalikan -1 walaupun INSERT berhasil.
                ClsStaticVariable.sukses = true;
            }
            catch (Exception ex)
            {
                ClsStaticVariable.sukses = false;

                InsertDataLogSafe(
                    trans.TransactionID,
                    "2. Transaction ADD FAILED. " +
                    "Error=" + ex.Message +
                    ", Query=" + query,
                    "ERROR"
                );

                return
                    "Data transaksi gagal ditambah. " +
                    "Error message: " +
                    ex.Message;
            }

            #endregion

            #region Insert detail tiket

            if (!ClsStaticVariable.sukses)
            {
                InsertDataLogSafe(
                    trans.TransactionID,
                    "3. Ticket Detail ADD CANCELLED karena header gagal.",
                    "ERROR"
                );

                return
                    "Detail tiket tidak dapat ditambahkan karena " +
                    "header transaksi gagal.";
            }

            if (trans.listtranstikdet == null)
            {
                trans.listtranstikdet =
                    new List<ClsTransactionTiketDetail>();
            }

            if (trans.listtranstikdet.Count == 0)
            {
                InsertDataLogSafe(
                    trans.TransactionID,
                    "3. Transaction tidak mempunyai detail tiket.",
                    "INFO"
                );
            }
            else
            {
                foreach (
                    ClsTransactionTiketDetail transdet
                    in trans.listtranstikdet)
                {
                    transdet.TransactionID =
                        trans.TransactionID;

                    DateTime jamMasuk =
                        transdet.JamMasuk == DateTime.MinValue
                            ? DateTime.Now
                            : transdet.JamMasuk;

                    DateTime jamKeluar =
                        transdet.JamKeluar == DateTime.MinValue
                            ? DateTime.Now
                            : transdet.JamKeluar;

                    string orderStatus =
                        string.IsNullOrWhiteSpace(
                            transdet.OrderStatus)
                            ? "BOUGHT"
                            : transdet.OrderStatus;

                    query2 =
                        "INSERT INTO WHNPOS.dbo.TransaksiTiketDetail " +
                        "(" +
                        "TransactionID, " +
                        "TransactionDate, " +
                        "ItemID, " +
                        "ItemName, " +
                        "Price, " +
                        "Qty, " +
                        "NoUrut, " +
                        "OrderStatus, " +
                        "JamMasuk, " +
                        "JamKeluar, " +
                        "WaktuBermain, " +
                        "Toleransi, " +
                        "RFID, " +
                        "TagID, " +
                        "Keterangan" +
                        ") VALUES (" +
                        ClsFungsi.C2Q(
                            transdet.TransactionID) + ", " +
                        "GETDATE(), " +
                        ClsFungsi.C2Q(
                            transdet.ItemId) + ", " +
                        ClsFungsi.C2Q(
                            transdet.ItemName) + ", " +
                        ClsFungsi.C2Q(
                            transdet.Price) + ", " +
                        ClsFungsi.C2Q(
                            transdet.Qty) + ", " +
                        ClsFungsi.C2Q(
                            transdet.NoUrut) + ", " +
                        ClsFungsi.C2Q(
                            orderStatus) + ", " +
                        ClsFungsi.C2QTime(
                            jamMasuk) + ", " +
                        ClsFungsi.C2QTime(
                            jamKeluar) + ", " +
                        ClsFungsi.C2Q(
                            transdet.WaktuBermain) + ", " +
                        ClsFungsi.C2Q(
                            transdet.Toleransi) + ", " +
                        ClsFungsi.C2Q(
                            transdet.RFID ?? "") + ", " +
                        ClsFungsi.C2Q(
                            transdet.TagID ?? "") + ", " +
                        ClsFungsi.C2Q(
                            transdet.Keterangan ?? "") +
                        ")";

                    try
                    {
                        int affectedTicket =
                            ClsStaticVariable.objConnection
                                .objSqlServerIUDClass
                                .ExecuteNonQuery(query2);

                        InsertDataLogSafe(
                            trans.TransactionID,
                            "3. Ticket Detail ADD SUCCESS. " +
                            "ExecuteNonQueryResult=" +
                            affectedTicket +
                            ", NoUrut=" +
                            transdet.NoUrut +
                            ", ItemID=" +
                            transdet.ItemId +
                            ", ItemName=" +
                            transdet.ItemName +
                            ", Qty=" +
                            transdet.Qty +
                            ", RFID=" +
                            transdet.RFID +
                            ", TagID=" +
                            transdet.TagID +
                            ", Query=" +
                            query2,
                            "SUCCESS"
                        );
                    }
                    catch (Exception ex)
                    {
                        ClsStaticVariable.sukses =
                            false;

                        InsertDataLogSafe(
                            trans.TransactionID,
                            "3. Ticket Detail ADD FAILED. " +
                            "NoUrut=" +
                            transdet.NoUrut +
                            ", ItemID=" +
                            transdet.ItemId +
                            ", RFID=" +
                            transdet.RFID +
                            ", TagID=" +
                            transdet.TagID +
                            ", Error=" +
                            ex.Message +
                            ", Query=" +
                            query2,
                            "ERROR"
                        );

                        return
                            "Detail tiket NoUrut " +
                            transdet.NoUrut +
                            " gagal disimpan. " +
                            ex.Message;
                    }
                }

                InsertDataLogSafe(
                    trans.TransactionID,
                    "3. Semua Ticket Detail ADD SUCCESS. " +
                    "TotalTicket=" +
                    trans.listtranstikdet.Count,
                    "SUCCESS"
                );
            }

            #endregion

            #region Insert detail barang biasa

            if (trans.listtransdet == null)
            {
                trans.listtransdet =
                    new List<ClsTransactionDetail>();
            }

            foreach (ClsTransactionDetail transdet
                     in trans.listtransdet)
            {
                transdet.TransactionID =
                    trans.TransactionID;

                query2 =
                    "INSERT INTO WHNPOS.dbo.TransaksiDetail " +
                    "(" +
                    "TransactionID, " +
                    "TransactionDate, " +
                    "ItemID, " +
                    "ItemName, " +
                    "Price, " +
                    "Qty, " +
                    "NoUrut, " +
                    "OrderStatus" +
                    ") VALUES (" +
                    ClsFungsi.C2Q(trans.TransactionID) + ", " +
                    "GETDATE(), " +
                    ClsFungsi.C2Q(transdet.ItemId) + ", " +
                    ClsFungsi.C2Q(transdet.ItemName) + ", " +
                    ClsFungsi.C2Q(transdet.Price) + ", " +
                    ClsFungsi.C2Q(transdet.Qty) + ", " +
                    ClsFungsi.C2Q(transdet.NoUrut) + ", " +
                    ClsFungsi.C2Q("BOUGHT") +
                    ")";

                try
                {
                    int affectedDetail =
                        ClsStaticVariable.objConnection
                            .objSqlServerIUDClass
                            .ExecuteNonQuery(query2);

                    InsertDataLogSafe(
                        trans.TransactionID,
                        "3B. Ordinary Detail ADD SUCCESS. " +
                        "ExecuteNonQueryResult=" +
                        affectedDetail +
                        ", NoUrut=" + transdet.NoUrut +
                        ", ItemID=" + transdet.ItemId +
                        ", ItemName=" + transdet.ItemName +
                        ", Qty=" + transdet.Qty +
                        ", Query=" + query2,
                        "SUCCESS"
                    );
                }
                catch (Exception ex)
                {
                    ClsStaticVariable.sukses = false;

                    InsertDataLogSafe(
                        trans.TransactionID,
                        "3B. Ordinary Detail ADD FAILED. " +
                        "ItemID=" + transdet.ItemId +
                        ", Error=" + ex.Message +
                        ", Query=" + query2,
                        "ERROR"
                    );

                    return
                        "Detail barang biasa gagal ditambah. " +
                        ex.Message;
                }
            }

            #endregion

            #region Update saldo kartu

            if (!ClsStaticVariable.sukses)
            {
                InsertDataLogSafe(
                    trans.TransactionID,
                    "4. BALANCE UPDATE CANCELLED " +
                    "karena insert detail gagal.",
                    "ERROR"
                );

                return
                    "Saldo kartu tidak diperbarui karena " +
                    "detail transaksi gagal ditambahkan.";
            }

            // MASTER_CARD tidak perlu diperbarui saldonya.
            if (isMasterCard)
            {
                ClsStaticVariable.sukses = true;

                InsertDataLogSafe(
                    trans.TransactionID,
                    "4. BALANCE UPDATE SKIPPED. " +
                    "PaymentType=MASTER_CARD, " +
                    "CardID=" + card.CardID,
                    "SUCCESS"
                );

                return
                    "Data transaksi dan detail berhasil ditambah " +
                    "menggunakan MASTER_CARD.";
            }

            query =
                "UPDATE WHNPOS.dbo.TblCard " +
                "SET Saldo = " +
                ClsFungsi.C2Q(finalBalance) + " " +
                "WHERE CardID = " +
                ClsFungsi.C2Q(card.CardID);

            try
            {
                InsertDataLogSafe(
                    trans.TransactionID,
                    "4. EDIT BALANCE START. " +
                    "CardID=" + card.CardID +
                    ", OldBalance=" + card.Saldo +
                    ", NewBalance=" + finalBalance +
                    ", Query=" + query,
                    "INFO"
                );

                int affectedBalance =
                    ClsStaticVariable.objConnection
                        .objSqlServerIUDClass
                        .ExecuteNonQuery(query);

                if (affectedBalance <= 0)
                {
                    ClsStaticVariable.sukses = false;

                    InsertDataLogSafe(
                        trans.TransactionID,
                        "4. BALANCE UPDATE FAILED. " +
                        "CardID tidak ditemukan atau saldo " +
                        "tidak diperbarui. Query=" +
                        query,
                        "ERROR"
                    );

                    return
                        "Transaksi tersimpan, tetapi saldo kartu " +
                        "gagal diperbarui karena kartu tidak ditemukan.";
                }

                card.Saldo =
                    finalBalance;

                ClsStaticVariable.sukses = true;

                InsertDataLogSafe(
                    trans.TransactionID,
                    "4. Transaction ADD SUCCESS, " +
                    "DETAIL ADD SUCCESS, " +
                    "BALANCE UPDATE SUCCESS. " +
                    "CardID=" + card.CardID +
                    ", FinalBalance=" + finalBalance,
                    "SUCCESS"
                );

                return
                    "Data transaksi dan detail berhasil ditambah. " +
                    "Saldo kartu berhasil dipotong.";
            }
            catch (Exception ex)
            {
                ClsStaticVariable.sukses = false;

                InsertDataLogSafe(
                    trans.TransactionID,
                    "4. BALANCE UPDATE FAILED. " +
                    "CardID=" + card.CardID +
                    ", Error=" + ex.Message +
                    ", Query=" + query,
                    "ERROR"
                );

                return
                    "Data transaksi tersimpan, tetapi saldo kartu " +
                    "gagal diperbarui. Error message: " +
                    ex.Message;
            }

            #endregion
        }



        public string InsertTopUp(ClsTransaction trans, ClsCard card)
        {
            ClsStaticVariable.sukses = false;
            // Add Card Balance 
            decimal finalbalance = card.Saldo + trans.totalAmount;
            if (finalbalance < 0)
            {
                return "Data Transaksi Gagal ditambah karena saldo kartu tidak mencukupi !!!";
            }
            else
            {
                trans.finalBalance = finalbalance;
                string logmessage = "1. Adding Transaction ID = " + trans.TransactionID + " Into Database, Date = " + trans.TransactionDate.ToString() + "  , Final Balance = " + finalbalance + " , Card ID = " + card.CardID + " , ShopID = " + trans.ShopId + " , TotalAmount = " + trans.totalAmount.ToString();
                string queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                // Insert Transaksi 
                ClsStaticVariable.sukses = false;
                query = " Insert Into WHNPOS.dbo.Transaksi " +
                        " (TransactionID, TransactionDate, TotalAmount, PaymentType, CardID, ShopID, Remarks, Subtotal, PPN, InitialBalance, FinalBalance, TransactionStatus, TransactionType) " +
                        " values " +
                        $" ({ClsFungsi.C2Q(trans.TransactionID)}, GETDATE() , {ClsFungsi.C2Q(trans.totalAmount)}, {ClsFungsi.C2Q(trans.PaymentType)}, {ClsFungsi.C2Q(trans.CardID)}, {ClsFungsi.C2Q(trans.ShopId)}, {ClsFungsi.C2Q(trans.Remarks)}, {ClsFungsi.C2Q(trans.Subtotal)}, {ClsFungsi.C2Q(trans.PPN)}, {ClsFungsi.C2Q(trans.InitialBalance)}, {ClsFungsi.C2Q(trans.finalBalance)}, 'COMPLETE', {ClsFungsi.C2Q(trans.TransactionType)})";

                try
                {
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
                    ClsStaticVariable.sukses = true;
                    logmessage = "2. Transaction ADD SUCCESS" + " query = " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                }
                catch (Exception ex)
                {
                    logmessage = "2. Transaction ADD FAILED , error Message = " + ex.Message + " query = " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi Gagal Ditambah !!! error message = " + ex.Message;
                }

                // Insert Detail
                if (ClsStaticVariable.sukses)
                {
                    ClsStaticVariable.sukses = false;
                    if (trans.listtransdet.Count != 0)
                    {
                        int index = 1;
                        foreach (ClsTransactionDetail transdet in trans.listtransdet)
                        {
                            query2 = " Insert Into WHNPOS.dbo.TransaksiDetail " +
                                     " (TransactionID, TransactionDate, ItemID, ItemName, Price, Qty, NoUrut, OrderStatus) " +
                                     " values " +
                                     $"({ClsFungsi.C2Q(transdet.TransactionID)}, GETDATE(), {ClsFungsi.C2Q(transdet.ItemId)}, {ClsFungsi.C2Q(transdet.ItemName)}, {ClsFungsi.C2Q(transdet.Price)}, {ClsFungsi.C2Q(transdet.Qty)}, {ClsFungsi.C2Q(index)}, 'DONE')";

                            index++;
                            try
                            {

                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query2);
                                logmessage = "3. Transaction Detail ADD SUCCESS" + " query = " + query2;
                                queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                            }
                            catch (Exception ex)
                            {
                                logmessage = "3. TransactionDetail ADD FAILED , ID = " + transdet.TransactionID + " error message = " + ex.Message + " query = " + query2;
                                queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                                return "Data Transaksi Detail Gagal Ditambah !!! error message = " + ex.Message;
                            }
                        }

                        ClsStaticVariable.sukses = true;
                        logmessage = "3. Transaction Detail ADD SUCCESS";
                        queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    }
                    else
                    {
                        ClsStaticVariable.sukses = true;
                    }
                }
                else
                {
                    ClsStaticVariable.sukses = false;
                }

                // Update Balance

                query = $" Update WHNPOS.dbo.TblCard set Saldo = {ClsFungsi.C2Q(finalbalance)} where CardID = {ClsFungsi.C2Q(card.CardID)}";
                try
                {
                    logmessage = "4. EDIT BALANCE query =  " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                    int hasil = ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
                    ClsStaticVariable.sukses = true;
                    logmessage = "4. Transaction ADD SUCCESS, DETAIL ADD SUCCESS, BALANCE ADD SUCCESS , hasil = " + hasil;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi dan Detail Berhasil ditambah !!! Saldo Kartu berhasil di tambah!!!";
                }
                catch (Exception ex)
                {
                    logmessage = "5. !!!Transaction ADD FAILED ";
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi Gagal Ditambah !!! error message = " + ex.Message;
                }
            }

        }

        private void InsertDataLog(
    string transactionID,
    string logMessage,
    string logType)
        {
            string logQuery =
                "INSERT INTO WHNPOS.dbo.DataLog " +
                "(TransactionID, LogMessage, LogType) VALUES (" +
                ClsFungsi.C2Q(transactionID) + ", " +
                ClsFungsi.C2Q(logMessage) + ", " +
                ClsFungsi.C2Q(logType) +
                ")";

            ClsStaticVariable.objConnection
                .objSqlServerIUDClass
                .ExecuteNonQuery(logQuery);
        }

        private void InsertDataLogSafe(
    string transactionID,
    string logMessage,
    string logType)
        {
            try
            {
                InsertDataLog(
                    transactionID,
                    logMessage,
                    logType
                );
            }
            catch
            {
                // Jangan menggagalkan transaksi hanya karena DataLog gagal ditulis.
            }
        }

        public string InsertTopUpMinus(ClsTransaction trans, ClsCard card)
        {
            ClsStaticVariable.sukses = false;
            // Add Card Balance 
            decimal finalbalance = card.Saldo + (-1 * trans.totalAmount);
            if (finalbalance < 0)
            {
                return "Data Transaksi Gagal ditambah karena saldo kartu tidak mencukupi !!!";
            }
            else
            {
                trans.finalBalance = finalbalance;
                string logmessage = "1. Adding Transaction ID = " + trans.TransactionID + " Into Database, Date = " + trans.TransactionDate.ToString() + "  , Final Balance = " + finalbalance + " , Card ID = " + card.CardID + " , ShopID = " + trans.ShopId + " , TotalAmount = " + trans.totalAmount.ToString();
                string queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                // Insert Transaksi 
                ClsStaticVariable.sukses = false;
                query = " Insert Into WHNPOS.dbo.Transaksi " +
                        " (TransactionID, TransactionDate, TotalAmount, PaymentType, CardID, ShopID, Remarks, Subtotal, PPN, InitialBalance, FinalBalance, TransactionStatus, TransactionType) " +
                        " values " +
                        $" ({ClsFungsi.C2Q(trans.TransactionID)}, GETDATE() , {ClsFungsi.C2Q(-1 * trans.totalAmount)}, {ClsFungsi.C2Q(trans.PaymentType)}, {ClsFungsi.C2Q(trans.CardID)}, {ClsFungsi.C2Q(trans.ShopId)}, {ClsFungsi.C2Q(trans.Remarks)}, {ClsFungsi.C2Q(trans.Subtotal)}, {ClsFungsi.C2Q(trans.PPN)}, {ClsFungsi.C2Q(trans.InitialBalance)}, {ClsFungsi.C2Q(trans.finalBalance)}, 'COMPLETE', {ClsFungsi.C2Q(trans.TransactionType)})";

                try
                {
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
                    ClsStaticVariable.sukses = true;
                    logmessage = "2. Transaction ADD SUCCESS" + " query = " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                }
                catch (Exception ex)
                {
                    logmessage = "2. Transaction ADD FAILED , error Message = " + ex.Message + " query = " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi Gagal Ditambah !!! error message = " + ex.Message;
                }

                // Insert Detail
                if (ClsStaticVariable.sukses)
                {
                    ClsStaticVariable.sukses = false;
                    if (trans.listtransdet.Count != 0)
                    {
                        int index = 1;
                        foreach (ClsTransactionDetail transdet in trans.listtransdet)
                        {
                            query2 = " Insert Into WHNPOS.dbo.TransaksiDetail " +
                                     " (TransactionID, TransactionDate, ItemID, ItemName, Price, Qty, NoUrut, OrderStatus) " +
                                     " values " +
                                     $"({ClsFungsi.C2Q(transdet.TransactionID)}, GETDATE(), {ClsFungsi.C2Q(transdet.ItemId)}, {ClsFungsi.C2Q(transdet.ItemName)}, {ClsFungsi.C2Q(-1 * transdet.Price)}, {ClsFungsi.C2Q(transdet.Qty)}, {ClsFungsi.C2Q(index)}, 'DONE')";

                            index++;
                            try
                            {

                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query2);
                                logmessage = "3. Transaction Detail ADD SUCCESS" + " query = " + query2;
                                queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                            }
                            catch (Exception ex)
                            {
                                logmessage = "3. TransactionDetail ADD FAILED , ID = " + transdet.TransactionID + " error message = " + ex.Message + " query = " + query2;
                                queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                                return "Data Transaksi Detail Gagal Ditambah !!! error message = " + ex.Message;
                            }
                        }

                        ClsStaticVariable.sukses = true;
                        logmessage = "3. Transaction Detail ADD SUCCESS";
                        queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    }
                    else
                    {
                        ClsStaticVariable.sukses = true;
                    }
                }
                else
                {
                    ClsStaticVariable.sukses = false;
                }

                // Update Balance

                query = $" Update WHNPOS.dbo.TblCard set Saldo = {ClsFungsi.C2Q(finalbalance)} where CardID = {ClsFungsi.C2Q(card.CardID)}";
                try
                {
                    logmessage = "4. EDIT BALANCE query =  " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                    int hasil = ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
                    ClsStaticVariable.sukses = true;
                    logmessage = "4. Transaction ADD SUCCESS, DETAIL ADD SUCCESS, BALANCE ADD SUCCESS , hasil = " + hasil;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi dan Detail Berhasil ditambah !!! Saldo Kartu berhasil di tambah!!!";
                }
                catch (Exception ex)
                {
                    logmessage = "5. !!!Transaction ADD FAILED ";
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi Gagal Ditambah !!! error message = " + ex.Message;
                }
            }

        }


        public string InsertRefund(ClsTransaction trans, ClsCard card)
        {
            ClsStaticVariable.sukses = false;
            // Add Card Balance 
            decimal finalbalance = card.Saldo - trans.totalAmount;
            if (finalbalance < 0)
            {
                return "Data Transaksi Gagal ditambah karena saldo kartu tidak mencukupi !!!";
            }
            else
            {
                trans.finalBalance = finalbalance;
                string logmessage = "1. Adding Transaction ID = " + trans.TransactionID + " Into Database, Date = " + trans.TransactionDate.ToString() + "  , Final Balance = " + finalbalance + " , Card ID = " + card.CardID + " , ShopID = " + trans.ShopId + " , TotalAmount = " + trans.totalAmount.ToString();
                string queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                //Insert Transaction
                ClsStaticVariable.sukses = false;
                query = " Insert Into WHNPOS.dbo.Transaksi " +
                        " (TransactionID, TransactionDate, TotalAmount, PaymentType, CardID, ShopID, Remarks, Subtotal, PPN, InitialBalance, FinalBalance, TransactionStatus, TransactionType) " +
                        " values " +
                        $" ({ClsFungsi.C2Q(trans.TransactionID)}, GETDATE() , {ClsFungsi.C2Q(trans.totalAmount)}, {ClsFungsi.C2Q(trans.PaymentType)}, {ClsFungsi.C2Q(trans.CardID)}, {ClsFungsi.C2Q(trans.ShopId)}, {ClsFungsi.C2Q(trans.Remarks)}, {ClsFungsi.C2Q(trans.Subtotal)}, {ClsFungsi.C2Q(trans.PPN)}, {ClsFungsi.C2Q(trans.InitialBalance)}, {ClsFungsi.C2Q(trans.finalBalance)}, 'DONE', {ClsFungsi.C2Q(trans.TransactionType)})";

                try
                {
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
                    ClsStaticVariable.sukses = true;
                    logmessage = "2. Transaction ADD SUCCESS" + " query = " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                }
                catch (Exception ex)
                {
                    logmessage = "2. Transaction ADD FAILED , error Message = " + ex.Message + " query = " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi Gagal Ditambah !!! error message = " + ex.Message;
                }

                //Insert Detail
                if (ClsStaticVariable.sukses)
                {
                    ClsStaticVariable.sukses = false;
                    if (trans.listtransdet.Count != 0)
                    {
                        int index = 1;
                        foreach (ClsTransactionDetail transdet in trans.listtransdet)
                        {
                            query2 = " Insert Into WHNPOS.dbo.TransaksiDetail " +
                                     " (TransactionID, TransactionDate, ItemID, ItemName, Price, Qty, NoUrut, OrderStatus) " +
                                     " values " +
                                     $"({ClsFungsi.C2Q(transdet.TransactionID)}, GETDATE(), {ClsFungsi.C2Q(transdet.ItemId)}, {ClsFungsi.C2Q(transdet.ItemName)}, {ClsFungsi.C2Q(transdet.Price)}, {ClsFungsi.C2Q(transdet.Qty)}, {ClsFungsi.C2Q(index)}, 'DONE')";

                            index++;
                            try
                            {
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query2);
                                logmessage = "3. Transaction DETAIL ADD SUCCESS" + " query = " + query2;
                                queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                            }
                            catch (Exception ex)
                            {
                                logmessage += "3. TransactionDetail ADD FAILED , ID = " + transdet.TransactionID + " error message = " + ex.Message + " query = " + query2;
                                queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                                return "Data Transaksi Detail Gagal Ditambah !!! error message = " + ex.Message;
                            }
                        }

                        ClsStaticVariable.sukses = true;
                        logmessage = "3. Transaction Detail ADD SUCCESS";
                        queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    }
                    else
                    {
                        ClsStaticVariable.sukses = true;
                    }
                }

                else
                {
                    ClsStaticVariable.sukses = false;
                }


                // Update Balance
                query = $" Update WHNPOS.dbo.TblCard set Saldo = {ClsFungsi.C2Q(finalbalance)} where CardID = {ClsFungsi.C2Q(card.CardID)}";
                try
                {
                    logmessage = "4. EDIT BALANCE query =  " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
                    ClsStaticVariable.sukses = true;
                    logmessage = "4. Transaction ADD SUCCESS, DETAIL ADD SUCCESS, BALANCE ADD SUCCESS";
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi dan Detail Berhasil ditambah !!! Saldo Kartu berhasil di kembalikan !!!";
                }
                catch (Exception ex)
                {
                    logmessage = "5. !!!Transaction ADD FAILED ";
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi Gagal Ditambah !!! error message = " + ex.Message;
                }
            }

            
        }

        public string InsertRefundandBlockCard(ClsTransaction trans, ClsCard card)
        {
            ClsStaticVariable.sukses = false;
            // Add Card Balance 
            decimal finalbalance = card.Saldo - trans.totalAmount;
            if (finalbalance < 0)
            {
                return "Data Transaksi Gagal ditambah karena saldo kartu tidak mencukupi !!!";
            }
            else
            {
                trans.finalBalance = finalbalance;
                string logmessage = "1. Adding Transaction ID = " + trans.TransactionID + " Into Database, Date = " + trans.TransactionDate.ToString() + "  , Final Balance = " + finalbalance + " , Card ID = " + card.CardID + " , ShopID = " + trans.ShopId + " , TotalAmount = " + trans.totalAmount.ToString();
                string queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                //Insert Transaction
                ClsStaticVariable.sukses = false;
                query = " Insert Into WHNPOS.dbo.Transaksi " +
                        " (TransactionID, TransactionDate, TotalAmount, PaymentType, CardID, ShopID, Remarks, Subtotal, PPN, InitialBalance, FinalBalance, TransactionStatus) " +
                        " values " +
                        $" ({ClsFungsi.C2Q(trans.TransactionID)}, GETDATE() , {ClsFungsi.C2Q(trans.totalAmount)}, {ClsFungsi.C2Q(trans.PaymentType)}, {ClsFungsi.C2Q(trans.CardID)}, {ClsFungsi.C2Q(trans.ShopId)}, {ClsFungsi.C2Q(trans.Remarks)}, {ClsFungsi.C2Q(trans.Subtotal)}, {ClsFungsi.C2Q(trans.PPN)}, {ClsFungsi.C2Q(trans.InitialBalance)}, {ClsFungsi.C2Q(trans.finalBalance)}, 'DONE')";

                try
                {
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
                    ClsStaticVariable.sukses = true;
                    logmessage = "2. Transaction ADD SUCCESS" + " query = " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                }
                catch (Exception ex)
                {
                    logmessage = "2. Transaction ADD FAILED , error Message = " + ex.Message + " query = " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi Gagal Ditambah !!! error message = " + ex.Message;
                }

                //Insert Detail
                if (ClsStaticVariable.sukses)
                {
                    ClsStaticVariable.sukses = false;
                    if (trans.listtransdet.Count != 0)
                    {
                        int index = 1;
                        foreach (ClsTransactionDetail transdet in trans.listtransdet)
                        {
                            query2 = " Insert Into WHNPOS.dbo.TransaksiDetail " +
                                     " (TransactionID, TransactionDate, ItemID, ItemName, Price, Qty, NoUrut, OrderStatus) " +
                                     " values " +
                                     $"({ClsFungsi.C2Q(transdet.TransactionID)}, GETDATE(), {ClsFungsi.C2Q(transdet.ItemId)}, {ClsFungsi.C2Q(transdet.ItemName)}, {ClsFungsi.C2Q(transdet.Price)}, {ClsFungsi.C2Q(transdet.Qty)}, {ClsFungsi.C2Q(index)}, 'DONE')";

                            index++;
                            try
                            {
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query2);
                                logmessage = "3. Transaction DETAIL ADD SUCCESS" + " query = " + query2;
                                queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                            }
                            catch (Exception ex)
                            {
                                logmessage = "3. TransactionDetail ADD FAILED , ID = " + transdet.TransactionID + " error message = " + ex.Message + " query = " + query2;
                                queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                                return "Data Transaksi Detail Gagal Ditambah !!! error message = " + ex.Message;
                            }
                        }

                        ClsStaticVariable.sukses = true;
                        logmessage = "3. Transaction Detail ADD SUCCESS";
                        queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    }
                    else
                    {
                        ClsStaticVariable.sukses = true;
                    }
                }

                else
                {
                    ClsStaticVariable.sukses = false;
                }


                // Update Balance
                query = $" Update WHNPOS.dbo.TblCard set Saldo = {ClsFungsi.C2Q(finalbalance)} where CardID = {ClsFungsi.C2Q(card.CardID)}";
                try
                {
                    logmessage = "4. EDIT BALANCE query =  " + query;
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);

                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
                    ClsStaticVariable.sukses = true;
                    //Update Status Kartu Active = false
                    query2 = " Update WHNPOS.dbo.TblCard set Active = 0 where CardID = " + ClsFungsi.C2Q(card.CardID);
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query2);
                    logmessage = "4. Transaction ADD SUCCESS, DETAIL ADD SUCCESS, BALANCE ADD SUCCESS, CARD BLOCKED";
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("SUCCESS")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi dan Detail Berhasil ditambah !!! Saldo Kartu berhasil di kembalikan , Kartu Berhasil diblokir !!!";
                }
                catch (Exception ex)
                {
                    logmessage = "5. !!!Transaction ADD FAILED ";
                    queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(trans.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
                    ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
                    return "Data Transaksi Gagal Ditambah !!! error message = " + ex.Message;
                }
            }

            
        }


        public void UpdateStatusTransItem(string TransdetID, string Status, int NoUrut)
        {
            query = "Update WHNPOS.dbo.TransaksiDetail " + 
                    $" set OrderStatus = {ClsFungsi.C2Q(Status)} where TransactionID = {ClsFungsi.C2Q(TransdetID)} and NoUrut = {ClsFungsi.C2Q(NoUrut)} and OrderStatus not like 'CANCELED'; ";
            ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
        }

        public void UpdateAllItemStatusTransItem(string TransdetID, string Status)
        {
            query = "Update WHNPOS.dbo.TransaksiDetail " +
                    $" set OrderStatus = {ClsFungsi.C2Q(Status)} where TransactionID = {ClsFungsi.C2Q(TransdetID)} and OrderStatus not like 'CANCELED'; ";
            ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
        }

        public void CompleteTransaction(string TransactionID)
        {
            query = $" Update WHNPOS.dbo.Transaksi set TransactionStatus = {ClsFungsi.C2Q("COMPLETE")} where TransactionID = {ClsFungsi.C2Q(TransactionID)}";
            ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
        }

        public void UpdateCancelStatus(List<ClsTransactionDetail> list, string CurrentID)
        {
            foreach(ClsTransactionDetail transdet in list)
            {
                query = $" Update WHNPOS.dbo.TransaksiDetail set OrderStatus = 'CANCELED' where TransactionID = {ClsFungsi.C2Q(CurrentID)} and ItemID = {ClsFungsi.C2Q(transdet.ItemId)}";
                ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
            }
        }

        public DataSet LoadListQRCodes(List<byte[]> listQrcodes, List<string> listticket, List<string> listitemname)
        {
            DataSet ds = new DataSet();

            DataTable TblQRCode = new DataTable();
            TblQRCode.TableName = "TblQRCode";
            TblQRCode.Columns.Add("TicketID", typeof(string));
            TblQRCode.Columns.Add("QRCode", typeof(byte[]));
            TblQRCode.Columns.Add("ItemName", typeof(string));
            for (int i = 0; i < listticket.Count; i++)
            {
                DataRow row = TblQRCode.NewRow();
                row["TicketID"] = listticket[i];
                row["QRCode"] = listQrcodes[i];
                row["ItemName"] = listitemname[i];
                TblQRCode.Rows.Add(row);
            }

            ds.Tables.Add(TblQRCode);
            return ds;
        }

        public DataTable GetTicketByTagID(
    string tagID,
    string orderStatus,
    DateTime from,
    DateTime to)
        {
            query =
                "SELECT TOP 1 " +
                "TransactionID, NoUrut, TagID, RFID, " +
                "OrderStatus, TransactionDate " +
                "FROM WHNPOS.dbo.TransaksiTiketDetail " +
                "WHERE ISNULL(TagID, '') = " +
                ClsFungsi.C2Q(tagID) + " " +
                "AND OrderStatus = " +
                ClsFungsi.C2Q(orderStatus) + " " +
                "AND TransactionDate >= " +
                ClsFungsi.C2QTime(from) + " " +
                "AND TransactionDate <= " +
                ClsFungsi.C2QTime(to);

            return ClsStaticVariable.objConnection
                .objsqlconnection
                .Filldatatable(query);
        }



        //public string ExtendOvertime(ClsExtend data2, ClsTransaction trans, ClsCard card)
        //{
        //    try
        //    {
        //        this.InsertTransactionTicket(trans, card); 
        //    }
        //    catch (Exception ex)
        //    {
        //        string logmessage = "Data Ticket gagal di Extend , pesan error = " + ex.Message;
        //        string queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(data2.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
        //        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
        //        return "Data Ticket gagal di Extend , pesan error = " + ex.Message;
        //    }

        //    query = $"Update WHNPOS.dbo.TransaksiTiketDetail set OrderStatus = 'ENTER-IN', JamKeluar = {ClsFungsi.C2QTime(data2.JamKeluarAkhir)} where TransactionID = {ClsFungsi.C2Q(data2.TransactionID)} and NoUrut = {ClsFungsi.C2Q(data2.NoUrut)}";
        //    query2 = $"Insert Into WHNPOS.dbo.TblExtend(TransactionID, TransactionDate, NoUrut, ItemID, ItemIDExtend, Price, WaktuBermain, JamKeluarAwal, JamKeluarAkhir, TransactIDExt) values " + 
        //             $" ({ClsFungsi.C2Q(data2.TransactionID)}, {ClsFungsi.C2Q(DateTime.Now)}, {ClsFungsi.C2Q(data2.NoUrut)}, {ClsFungsi.C2Q(data2.ItemID)}, {ClsFungsi.C2Q(data2.ItemIDExtend)}, {ClsFungsi.C2Q(data2.Price)}, {ClsFungsi.C2Q(data2.WaktuBermain)}, {ClsFungsi.C2QTime(data2.JamKeluarAwal)}, {ClsFungsi.C2QTime(data2.JamKeluarAwal)}, {ClsFungsi.C2Q(data2.TransactIDExtend)}) ";

        //    try
        //    {
        //        string logmessage = "Update WHNPOS.dbo.TransaksiTiketDetail, query = " + query;
        //        string queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(data2.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
        //        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
        //        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
        //        logmessage = "Insert WHNPOS.dbo.TransaksiTiketDetail, query = " + query2;
        //        queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(data2.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("INFO")})";
        //        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
        //        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query2);
        //        return "Extend Ticket Berhasil Dilakukan, Jam Keluar anda adalah : " + data2.JamKeluarAkhir.ToString();
        //    }
        //    catch(Exception ex)
        //    {
        //        string logmessage = "5. !!! Extend Transaction Tiket FAILED ";
        //        string queryy = $" Insert Into WHNPOS.dbo.DataLog (TransactionID, LogMessage, LogType) values ({ClsFungsi.C2Q(data2.TransactionID)}, {ClsFungsi.C2Q(logmessage)}, {ClsFungsi.C2Q("ERROR")})";
        //        ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(queryy);
        //        return "Data Transaksi Tiket Gagal Diextend !!! error message = " + ex.Message;
        //    }


        //}

        #endregion
    }
}
