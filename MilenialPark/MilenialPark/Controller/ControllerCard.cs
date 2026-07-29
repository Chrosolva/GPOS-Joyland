using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using MilenialPark.Models;
using MilenialPark.Master;

namespace MilenialPark.Controller
{
    public class ControllerCard
    {
        #region properties

        public ClsCard objCard = new ClsCard();
        public ClsTransaction objTrans = new ClsTransaction();
        public string query;
        public DataTable dt;
        public DataRow dr;
        public string CardID;

        #endregion
        #region constructor

        public ControllerCard()
        {

        }

        #endregion

        #region function

        public DataTable getCardList()
        {
            query = "Select * from WHNPOS.dbo.TblCard";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable getCardListActive()
        {
            query = "Select * from WHNPOS.dbo.TblCard where Active = 1";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public bool CheckCard(string cardID)
        {
            if (string.IsNullOrWhiteSpace(cardID))
            {
                return false;
            }

            query =
                "Select top 1 CardID " +
                "from WHNPOS.dbo.TblCard " +
                "where CardID = " +
                ClsFungsi.C2Q(cardID);

            dt = ClsStaticVariable.objConnection
                .objsqlconnection
                .Filldatatable(query);

            return dt.Rows.Count > 0;
        }

        public bool CheckCard2(string cardID)
        {
            if (string.IsNullOrWhiteSpace(cardID))
            {
                return false;
            }

            query =
                "Select top 1 CardID " +
                "from WHNPOS.dbo.TblCard " +
                "where CardID = " +
                ClsFungsi.C2Q(cardID) +
                " and Active = 1";

            dt = ClsStaticVariable.objConnection
                .objsqlconnection
                .Filldatatable(query);

            return dt.Rows.Count > 0;
        }

        public DataTable getCardTransactHistory(string CardID, DateTime from , DateTime to)
        {
            query = "Select TR.TransactionID, TR.TransactionDate , TR.TotalAmount, TR.PaymentType, TR.CardID, C.CustomerName, " +
                    " TR.ShopID, SH.ShopName, SH.UserID , TR.Remarks, TR.Subtotal, TR.PPN, TR.InitialBalance, TR.FinalBalance, TR.TransactionStatus from " +
                    " WHNPOS.dbo.Transaksi as TR left join WHNPOS.dbo.TblCard as C on TR.CardID = C.CardID left join WHNPOS.dbo.Shop as SH on TR.ShopID = SH.ShopID " +
                    $" where TR.CardID = {ClsFungsi.C2Q(CardID)} and TR.TransactionDate >= {ClsFungsi.C2QTime(Convert.ToDateTime(from.ToString("dd/M/yyyy HH:mm:ss tt")))} " +
                    $" and TR.TransactionDate <= {ClsFungsi.C2QTime(Convert.ToDateTime(to.ToString("dd/M/yyyy HH:mm:ss tt")))} " +
                    " Order By TR.TransactionDate desc ";
            return ClsStaticVariable.objConnection.objsqlconnection.Filldatatable(query);
        }

        public DataTable getCardTransactHistoryWithShopID(
    string shopID,
    string cardID,
    DateTime from,
    DateTime to)
        {
            query =
    "Select " +
    "ISNULL(TR.TransactionID, '') as TransactionID, " +
    "ISNULL(TR.TransactionDate, GETDATE()) as TransactionDate, " +
    "ISNULL(TR.TotalAmount, 0) as TotalAmount, " +
    "ISNULL(TR.PaymentType, '') as PaymentType, " +
    "ISNULL(TR.CardID, '') as CardID, " +
    "ISNULL(C.CustomerName, '') as CustomerName, " +
    "ISNULL(TR.ShopID, '') as ShopID, " +
    "ISNULL(SH.ShopName, '') as ShopName, " +
    "ISNULL(SH.UserID, '') as UserID, " +
    "ISNULL(TR.Remarks, '') as Remarks, " +
    "ISNULL(TR.Subtotal, 0) as Subtotal, " +
    "ISNULL(TR.PPN, 0) as PPN, " +
    "ISNULL(TR.InitialBalance, 0) as InitialBalance, " +
    "ISNULL(TR.FinalBalance, 0) as FinalBalance, " +
    "ISNULL(TR.TransactionStatus, '') as TransactionStatus " +
    "from WHNPOS.dbo.Transaksi TR " +
    "left join WHNPOS.dbo.TblCard C " +
    "on TR.CardID = C.CardID " +
    "left join WHNPOS.dbo.Shop SH " +
    "on TR.ShopID = SH.ShopID " +
    "where TR.ShopID = " +
    ClsFungsi.C2Q(shopID) + " " +
    "and TR.CardID = " +
    ClsFungsi.C2Q(cardID) + " " +
    "and TR.TransactionDate >= " +
    ClsFungsi.C2QTime(from) + " " +
    "and TR.TransactionDate <= " +
    ClsFungsi.C2QTime(to) + " " +
    "order by TR.TransactionDate desc";

            return ClsStaticVariable.objConnection
                .objsqlconnection
                .Filldatatable(query);
        }

        public void InsertCard(ClsCard card)
        {
            query = "Insert Into WHNPOS.dbo.TblCard " +
                    " (CardID, CustomerName, NoIdentitas, Saldo, Active) " +
                    " values " +
                    $" ({ClsFungsi.C2Q(card.CardID)}, {ClsFungsi.C2Q(card.CustomerName)}, {ClsFungsi.C2Q(card.Noidentitas)}, {ClsFungsi.C2Q(card.Saldo)}, {ClsFungsi.C2Q(card.Active)}); ";
            ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
        }

        public void UpdateCard(ClsCard card)
        {
            query = "Update WHNPOS.dbo.TblCard " + 
                    $" set CustomerName = {ClsFungsi.C2Q(card.CustomerName)}, NoIdentitas = {ClsFungsi.C2Q(card.Noidentitas)}, Saldo = {ClsFungsi.C2Q(card.Saldo)}, Active = {ClsFungsi.C2Q(card.Active)} " + 
                    $" where CardID = {ClsFungsi.C2Q(card.CardID)}";
            ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
        }

        public void DeleteCard(string CardID)
        {
            query = "Delete WHNPOS.dbo.TblCard where CardID = " + ClsFungsi.C2Q(CardID);
            ClsStaticVariable.objConnection.objSqlServerIUDClass.ExecuteNonQuery(query);
        }
        

        #endregion
    }
}
