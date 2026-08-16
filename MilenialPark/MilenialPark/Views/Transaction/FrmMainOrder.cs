using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MilenialPark.Views;
using MilenialPark.Master;
using MilenialPark.UserControls;
using MilenialPark.Controller;
using MilenialPark;
using MilenialPark.Views.Admin;


namespace MilenialPark.Views.Transaction
{
    public partial class FrmMainOrder : Form
    {
        #region properties

        public Mainform parentfrm;
        public ControllerShop controllerShop = new ControllerShop();
        public Card.FrmCards frmCard;
        public Admin.FrmCardManagement frmCardManagement;
        public Admin.FrmRFIDManagement frmRFIDManagement;
        public Transaction.FrmOrderTiket frmOrderTiket;
        public Transaction.FrmOrder frmOrder;


        #endregion

        public FrmMainOrder()
        {
            InitializeComponent();
        }

        public FrmMainOrder(Mainform parent)
        {
            InitializeComponent();
            parentfrm = parent;
        }

        private void FrmMainOrder_Load(
    object sender,
    EventArgs e)
        {
            #region Card

            frmCard =
                new Card.FrmCards(parentfrm);

            frmCard.Text =
                "Card";

            frmCard.TopLevel =
                false;

            frmCard.FormBorderStyle =
                FormBorderStyle.None;

            frmCard.Dock =
                DockStyle.Fill;

            #endregion


            #region Card Management

            frmCardManagement =
                new Admin.FrmCardManagement(parentfrm);

            frmCardManagement.Text =
                "Card Management";

            frmCardManagement.TopLevel =
                false;

            frmCardManagement.FormBorderStyle =
                FormBorderStyle.None;

            frmCardManagement.Dock =
                DockStyle.Fill;

            #endregion


            #region RFID Management

            frmRFIDManagement =
                new Admin.FrmRFIDManagement();

            frmRFIDManagement.Text =
                "RFID Management";

            frmRFIDManagement.TopLevel =
                false;

            frmRFIDManagement.FormBorderStyle =
                FormBorderStyle.None;

            frmRFIDManagement.Dock =
                DockStyle.Fill;

            #endregion


            #region Order Ticket

            frmOrderTiket =
                new Transaction.FrmOrderTiket(
                    parentfrm
                );

            frmOrderTiket.Text =
                "Order Tiket";

            frmOrderTiket.TopLevel =
                false;

            frmOrderTiket.FormBorderStyle =
                FormBorderStyle.None;

            frmOrderTiket.Dock =
                DockStyle.Fill;

            #endregion


            #region Create Order

            if (ClsStaticVariable.CurrentShop == null)
            {
                ClsFungsi.Pesan(
                    "Universal Shop belum dimuat. " +
                    "Silakan login kembali.",
                    "ERROR"
                );

                BeginInvoke(
                    new Action(Close)
                );

                return;
            }

            controllerShop.objShop =
                ClsStaticVariable.CurrentShop;

            frmOrder =
                new Transaction.FrmOrder(
                    parentfrm,
                    ClsStaticVariable.CurrentShop
                );

            frmOrder.Text =
                "Order";

            frmOrder.TopLevel =
                false;

            frmOrder.FormBorderStyle =
                FormBorderStyle.None;

            frmOrder.Dock =
                DockStyle.Fill;

            #endregion


            #region Add Forms To Tabs

            TPOrders.Controls.Clear();
            TPOrders.Controls.Add(
                frmOrderTiket
            );
            frmOrderTiket.Show();

            TPNEOrders.Controls.Clear();
            TPNEOrders.Controls.Add(
                frmOrder
            );
            frmOrder.Show();

            TPTopUpCard.Controls.Clear();
            TPTopUpCard.Controls.Add(
                frmCard
            );
            frmCard.Show();

            TPDaftarKartu.Controls.Clear();
            TPDaftarKartu.Controls.Add(
                frmCardManagement
            );
            frmCardManagement.Show();

            TPRFID.Controls.Clear();
            TPRFID.Controls.Add(
                frmRFIDManagement
            );
            frmRFIDManagement.Show();

            #endregion
        }

        private void TCMainOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TCMainOrder.SelectedIndex == 0)
            {
                frmOrderTiket.btnFilter_Click(null, null);
            }
        }

        
    }
}
