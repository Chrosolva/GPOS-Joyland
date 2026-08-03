using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MilenialPark.Controller;
using MilenialPark.Master;
using MilenialPark.Models;
using MilenialPark.UserControls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Windows.Forms;
using MilenialPark.Reports;
using MilenialPark.Views.Reports;

namespace MilenialPark.Views.Transaction
{
    public partial class FrmOrder : Form
    {
        #region properties

        public Mainform parentfrm;
        public ImageList imgList = new ImageList();
        public Image img;
        public ControllerShop controllerShop = new ControllerShop();
        public List<UCShopItem> listShopItem = new List<UCShopItem>();
        public ControllerTransaction controllerTrans = new ControllerTransaction();
        public ControllerCard controllerCard = new ControllerCard();
        public ControllerReport controllerReport = new ControllerReport();
        public ClsShop objShop = new ClsShop();

        public ReportDocument reportDoc = new ReportDocument();
        public DataSet ds = new DataSet();
        public bool exist = false;
        PrintDialog printdialog1 = new PrintDialog();
        PrintDocument printdocument = new PrintDocument();
        public string excludecategory;

        #endregion

        public FrmOrder()
        {
            InitializeComponent();
        }

        public FrmOrder(Mainform parent, ClsShop shop)
        {
            InitializeComponent();

            parentfrm = parent;

            if (ClsStaticVariable.CurrentShop == null)
            {
                ClsStaticVariable.CurrentShop = shop;
            }

            if (!ValidateUniversalShop())
            {
                return;
            }

            imgList.Images.Add(Resource.food_app);
            imgList.Images.Add(Resource.mobile_payment);
            imgList.Images.Add(Resource.history);

            ordertabs.ImageList = imgList;
            ordertabs.TabPages[0].ImageIndex = 0;
            ordertabs.TabPages[1].ImageIndex = 2;

            SetDefaultTransactionType();
        }

        private void SetDefaultTransactionType()
        {
            bool isWeekend =
                DateTime.Now.DayOfWeek == DayOfWeek.Saturday ||
                DateTime.Now.DayOfWeek == DayOfWeek.Sunday;

            if (isWeekend)
            {
                cbxTransType.SelectedIndex = 1;
                excludecategory = "WEEKDAY";
            }
            else
            {
                cbxTransType.SelectedIndex = 0;
                excludecategory = "WEEKEND";
            }
        }

        public void FillFLPanel(
    object sender,
    EventArgs e)
        {
            FLMenu.SuspendLayout();

            try
            {
                FLMenu.Controls.Clear();
                listShopItem.Clear();

                if (!ValidateUniversalShop())
                {
                    return;
                }

                string shopID =
                    ClsStaticVariable.CurrentShop.ShopID;

                bool loaded =
                    controllerShop.getShopandShopItem2Union(
                        shopID,
                        excludecategory
                    );

                if (!loaded ||
                    controllerShop.objShop == null ||
                    controllerShop.objShop.listShopitem == null)
                {
                    ClsFungsi.Pesan(
                        "Gagal memuat item Universal Shop.",
                        "ERROR"
                    );

                    return;
                }

                HashSet<string> loadedItemKeys =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase
                    );

                foreach (
                    ClsShopItem shopItem
                    in controllerShop.objShop.listShopitem)
                {
                    if (shopItem == null ||
                        string.IsNullOrWhiteSpace(shopItem.ItemID))
                    {
                        continue;
                    }

                    string itemKey =
                        (shopItem.ItemSource ?? "") +
                        "|" +
                        shopItem.ItemID.Trim();

                    if (!loadedItemKeys.Add(itemKey))
                    {
                        MessageBox.Show(
                            "Ditemukan menu yang benar-benar duplikat:\n\n" +
                            "Key: " + itemKey + "\n" +
                            "ItemName: " + shopItem.ItemName,
                            "Duplicate Menu",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );

                        continue;
                    }

                    UCShopItem itemControl =
                        new UCShopItem(shopItem);

                    itemControl.Name =
                        "SHOPITEM_" +
                        MakeSafeControlName(itemKey);

                    WireShopItemClick(
                        itemControl,
                        itemControl
                    );

                    listShopItem.Add(
                        itemControl
                    );
                }

                FLMenu.Controls.AddRange(
                    listShopItem.ToArray()
                );
            }
            finally
            {
                FLMenu.ResumeLayout(true);
            }
        }

        private string MakeSafeControlName(
    string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "EMPTY";
            }

            StringBuilder result =
                new StringBuilder();

            foreach (char character in value)
            {
                if (char.IsLetterOrDigit(character) ||
                    character == '_')
                {
                    result.Append(character);
                }
                else
                {
                    result.Append('_');
                }
            }

            return result.ToString();
        }

        private void WireShopItemClick(
    Control control,
    UCShopItem owner)
        {
            if (control == null || owner == null)
            {
                return;
            }

            // Pastikan handler tidak terpasang dua kali.
            control.Click -= ShopItemControl_Click;
            control.Click += ShopItemControl_Click;

            // Semua child menyimpan reference ke UCShopItem pemiliknya.
            control.Tag =
                owner;

            foreach (Control child in control.Controls)
            {
                WireShopItemClick(
                    child,
                    owner
                );
            }
        }

        private void ShopItemControl_Click(
    object sender,
    EventArgs e)
        {
            Control clickedControl =
                sender as Control;

            if (clickedControl == null)
            {
                return;
            }

            UCShopItem owner =
                clickedControl.Tag as UCShopItem;

            if (owner == null ||
                owner.objShopItem == null)
            {
                return;
            }

            ClsShopItem selectedItem =
                owner.objShopItem;

            System.Diagnostics.Debug.WriteLine(
                "CLICKED CONTROL=" +
                clickedControl.Name +
                " | ITEM ID=" +
                selectedItem.ItemID +
                " | ITEM NAME=" +
                selectedItem.ItemName
            );

            AddItemToOrder(
                selectedItem
            );
        }

        private void AddItemToOrder(ClsShopItem selectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }

            string selectedSource =
    (selectedItem.ItemSource ?? "")
        .Trim();

            string selectedItemID =
                (selectedItem.ItemID ?? "").Trim();

            string selectedItemName =
                (selectedItem.ItemName ?? "").Trim();

            if (selectedItemID.Length == 0)
            {
                ClsFungsi.Pesan(
                    "ItemID kosong untuk item " +
                    selectedItemName,
                    "ERROR"
                );

                return;
            }

            UCOrderItem existingItem =
                null;

            foreach (
                Control control
                in FLNewOrder.Controls)
            {
                UCOrderItem orderItem =
                    control as UCOrderItem;

                if (orderItem == null ||
                    orderItem.objTransdet == null)
                {
                    continue;
                }

                string existingItemID =
    (orderItem.objTransdet.ItemId ?? "")
        .Trim();

                string existingSource =
                    (orderItem.ItemSource ?? "")
                        .Trim();

                bool sameSource =
                    string.Equals(
                        existingSource,
                        selectedSource,
                        StringComparison.OrdinalIgnoreCase
                    );

                bool sameItemID =
                    string.Equals(
                        existingItemID,
                        selectedItemID,
                        StringComparison.OrdinalIgnoreCase
                    );

                if (sameSource && sameItemID)
                {
                    existingItem = orderItem;
                    break;
                }
            }

            if (existingItem != null)
            {
                decimal nextQuantity =
                    existingItem.NUDQty.Value + 1;

                if (nextQuantity <=
                    existingItem.NUDQty.Maximum)
                {
                    existingItem.NUDQty.Value =
                        nextQuantity;
                }

                return;
            }

            // lanjutkan pembuatan item baru...
            ClsTransactionDetail transactionDetail =
    new ClsTransactionDetail(
        "ORDTMP",
        DateTime.Now,
        selectedItemID,
        selectedItemName,
        selectedItem.Price,
        1,
        "NOTSERVED"
    );

            ClsTransactionTiketDetail ticketDetail =
                new ClsTransactionTiketDetail(
                    "ORDTMP",
                    DateTime.Now,
                    selectedItemID,
                    selectedItemName,
                    selectedItem.Price,
                    1,
                    "NOTSERVED",
                    DateTime.Now,
                    DateTime.Now,
                    selectedItem.WaktuBermain,
                    selectedItem.Toleransi
                );

            UCOrderItem newOrderItem =
                new UCOrderItem(
                    transactionDetail,
                    ticketDetail
                );

            newOrderItem.Name =
                "ORDERITEM_" +
                MakeSafeControlName(
                    selectedSource + "_" + selectedItemID
                );

            newOrderItem.ItemSource = selectedSource;
            newOrderItem.WaktuBermain = selectedItem.WaktuBermain;
            newOrderItem.Toleransi = selectedItem.Toleransi;

            newOrderItem.NUDQty.Minimum = 1;
            newOrderItem.NUDQty.Value = 1;

            newOrderItem.btnDelete.Click +=
                delegate
                {
                    RemoveOrderItem(newOrderItem);
                };

            newOrderItem.NUDQty.ValueChanged +=
                delegate
                {
                    UpdateOrderQuantity(newOrderItem);
                };

            FLNewOrder.Controls.Add(newOrderItem);

            UpdateOrderQuantity(newOrderItem);
        }

        private void RemoveOrderItem(
    UCOrderItem orderItem)
        {
            if (orderItem == null)
            {
                return;
            }

            FLNewOrder.Controls.Remove(
                orderItem
            );

            orderItem.Dispose();

            CalculateSubtotal();
        }

        private void UpdateOrderQuantity(
    UCOrderItem orderItem)
        {
            if (orderItem == null ||
                orderItem.objTransdet == null)
            {
                return;
            }

            int quantity =
                Convert.ToInt32(
                    orderItem.NUDQty.Value
                );

            orderItem.objTransdet.Qty =
                quantity;

            orderItem.lblQty.Text =
                quantity.ToString();

            CalculateSubtotal();
        }



        private bool ValidateUniversalShop()
        {
            if (ClsStaticVariable.CurrentShop == null)
            {
                ClsFungsi.Pesan(
                    "Universal Shop belum dimuat. Silakan logout dan login kembali.",
                    "ERROR"
                );

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                ClsStaticVariable.CurrentShop.ShopID))
            {
                ClsFungsi.Pesan(
                    "ShopID Universal Shop tidak valid.",
                    "ERROR"
                );

                return false;
            }

            objShop = ClsStaticVariable.CurrentShop;
            controllerShop.objShop = ClsStaticVariable.CurrentShop;

            return true;
        }

        private void FrmOrder_Load(object sender, EventArgs e)
        {
            if (!ValidateUniversalShop())
            {
                BeginInvoke(new Action(Close));
                return;
            }

            FillFLPanel(sender, e);

            parentfrm.btnFind.Click += OrderSearch;
            parentfrm.txtSearch.TextChanged += OrderSearch;

            parentfrm.cbxCategory.Items.Clear();
            parentfrm.cbxCategory.Items.Add("Item Name");
            parentfrm.cbxCategory.SelectedIndex = 0;
            parentfrm.cbxCategory.Enabled = false;
        }

        public void OrderSearch(object sender, EventArgs e)
        {
            if (parentfrm.cbxCategory.Text == "Item Name")
            {
                foreach (Control x in FLMenu.Controls)
                {
                    UCShopItem shopitem = (UCShopItem)x;
                    if (shopitem.lblItemName.Text.ToUpper().Contains(parentfrm.txtSearch.Text.ToUpper()))
                    {
                        shopitem.Visible = true;
                    }
                    else
                    {
                        shopitem.Visible = false;
                    }
                }
            }
        }

        public void CalculateSubtotal()
        {
            int subtotal = 0;
            foreach (Control x in FLNewOrder.Controls)
            {
                var tmp = (UCOrderItem)x;
                subtotal += Convert.ToInt32(tmp.NUDQty.Value * Convert.ToDecimal(tmp.objTransdet.Price));
            }
            lblSubtotal.Text = subtotal.ToString("#,##0");

            chkPPN_CheckedChanged(null, null);
            CalculateTotal();

        }

        public void CalculateTotal()
        {
            decimal total = Convert.ToDecimal(lblSubtotal.Text) + Convert.ToDecimal(lblPPN.Text);
            lblTotal.Text = total.ToString("#,##0");
        }

        private void FLNewOrder_ControlAdded(object sender, ControlEventArgs e)
        {
            CalculateSubtotal();
        }

        private void chkPPN_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPPN.Checked)
            {
                decimal ppn = Convert.ToDecimal(Math.Floor((float)Convert.ToDecimal(lblSubtotal.Text) * 0.11));
                lblPPN.Text = ppn.ToString("#,##0");
            }
            else
            {
                lblPPN.Text = "0";
            }
            CalculateTotal();
        }



        private void FrmOrder_FormClosing(object sender, FormClosingEventArgs e)
        {
            parentfrm.btnFind.Click -= this.OrderSearch;
            parentfrm.txtSearch.TextChanged -= this.OrderSearch;
            parentfrm.cbxCategory.Enabled = true;
            parentfrm.cbxCategory.Items.Clear();
        }



        private void btnPay_Click_1(object sender, EventArgs e)
        {
            CalculateTotal();
            if (FLNewOrder.Controls.Count != 0)
            {
                string shopID = ClsStaticVariable.CurrentShop.ShopID;
                controllerTrans.AutogenereateTransactionID("TICKET", shopID);
                controllerTrans.objTransaction =
    new ClsTransaction(
        controllerTrans.TransactionID,
        DateTime.Now,
        Convert.ToDecimal(lblTotal.Text),
        "",
        "",
        shopID,
        "",
        Convert.ToDecimal(lblSubtotal.Text),
        Convert.ToDecimal(lblPPN.Text),
        0,
        0,
        "NOTPAID",
        cbxTransType.Text
    );

                controllerTrans.objTransaction.listtransdet = new List<ClsTransactionDetail>();
                controllerTrans.objTransaction.listtranstikdet = new List<ClsTransactionTiketDetail>();

                foreach (Control control in FLNewOrder.Controls)
                {
                    UCOrderItem itemControl =
                        control as UCOrderItem;

                    if (itemControl == null)
                    {
                        continue;
                    }

                    itemControl.objTransdet.TransactionID =
                        controllerTrans.TransactionID;

                    ClsTransactionTiketDetail ticketDetail =
                        new ClsTransactionTiketDetail(
                            transactionid:
                                controllerTrans.TransactionID,

                            transactiondate:
                                DateTime.Now,

                            itemid:
                                itemControl.objTransdet.ItemId,

                            itemname:
                                itemControl.objTransdet.ItemName,

                            price:
                                itemControl.objTransdet.Price,

                            qty:
                                itemControl.objTransdet.Qty,

                            noUrut:
                                0,

                            orderStatus:
                                "BOUGHT",

                            jamMasuk:
                                DateTime.Now,

                            jamKeluar:
                                DateTime.Now,

                            waktuBermain:
                                itemControl.WaktuBermain,

                            toleransi:
                                itemControl.Toleransi
                        );

                    // listtranstikdet sementara membawa semua item ke FrmPayment.
                    // FrmPayment akan memisahkan berdasarkan WaktuBermain.
                    controllerTrans.objTransaction
                        .listtranstikdet
                        .Add(ticketDetail);

                    // TransaksiDetail hanya untuk barang non-tiket.
                    if (itemControl.WaktuBermain <= 0)
                    {
                        controllerTrans.objTransaction
                            .listtransdet
                            .Add(itemControl.objTransdet);
                    }
                }

                FrmPayment frmPayment = new FrmPayment(controllerTrans);
                //FormBlank frmBlank = new FormBlank();
                //frmBlank.Show();
                frmPayment.ShowDialog();
                //frmBlank.Close();

                // Auto Print Ticket 


                if (ClsStaticVariable.sukses)
                {
                    ClsStaticVariable.sukses = false;
                    FLNewOrder.Controls.Clear();
                    lblSubtotal.Text = "0";
                    lblPPN.Text = "0";
                    chkPPN.Checked = false;
                    lblTotal.Text = "0";
                }
            }
            else
            {
                ClsFungsi.Pesan("Daftar Pesanan Kosong, Mohon diisi terlebih dahulu !!!", "ERROR");
            }
        }

        private void txtCardID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                controllerTrans.dt = controllerTrans.getCard(txtCardID.Text);
                controllerTrans.SetCard(txtCardID.Text);

                if (controllerTrans.dt.Rows.Count == 0)
                {
                    exist = false;
                    ClsFungsi.Pesan("Data Kartu tidak terdaftar pada sistem , mohon hubungi admin !!!", "ERROR");
                }
                else
                {
                    exist = true;
                    controllerTrans.objCard = new ClsCard(controllerTrans.dt.Rows[0]["CardID"].ToString(), controllerTrans.dt.Rows[0]["CustomerName"].ToString(), controllerTrans.dt.Rows[0]["NoIdentitas"].ToString(), Convert.ToDecimal(controllerTrans.dt.Rows[0]["Saldo"]), Convert.ToBoolean(controllerTrans.dt.Rows[0]["Active"]));
                    lblCardID.Text = "Card ID : " + controllerTrans.objCard.CardID;
                    lblCustomerName.Text = "Name : " + controllerTrans.objCard.CustomerName;
                    lblIdentity.Text = "No (KTP/SIM) : " + controllerTrans.objCard.Noidentitas;
                    lblBalance.Text = "Balance (Saldo) : Rp. " + controllerTrans.objCard.Saldo.ToString("#,##0");

                    btnFilter2_Click(sender, e);
                }
            }
        }

        private void btnFilter2_Click(object sender, EventArgs e)
        {
            if (exist)
            {
                LoadCardList(sender, e, controllerTrans.objCard.CardID, dtpFrom.Value, dtpTo.Value);
            }
        }

        public void LoadCardList(object sender, EventArgs e, string CardID, DateTime from, DateTime to)
        {
            FLCardTransList.Controls.Clear();
            string shopID = ClsStaticVariable.CurrentShop.ShopID;
            controllerCard.dt = controllerCard.getCardTransactHistoryWithShopID(shopID, controllerTrans.objCard.CardID, new DateTime(from.Year, from.Month, from.Day, 0, 0, 0), new DateTime(to.Year, to.Month, to.Day, 23, 59, 59));
            if (controllerCard.dt.Rows.Count != 0)
            {
                int index = 0;
                foreach (DataRow row in controllerCard.dt.Rows)
                {
                    controllerTrans.objTransaction = new ClsTransaction(row["TransactionID"].ToString(), Convert.ToDateTime(row["TransactionDate"]), Convert.ToDecimal(row["TotalAmount"]), row["PaymentType"].ToString(), row["CardID"].ToString(), row["ShopID"].ToString(), row["Remarks"].ToString(), Convert.ToDecimal(row["Subtotal"]), Convert.ToDecimal(row["PPN"]), Convert.ToDecimal(row["InitialBalance"]), Convert.ToDecimal(row["FinalBalance"]), row["TransactionStatus"].ToString());
                    UCCardTransList ucCardTranList = new UCCardTransList(controllerTrans.objTransaction);
                    ucCardTranList.Name = "CH" + index.ToString();
                    index++;
                    ucCardTranList.btnDetails.Click += (se, ev) => this.DetailsClick(sender, e, ucCardTranList.objtrans);
                    ucCardTranList.btnReceipt.Click += (se, ev) => this.ReceiptClick(sender, e, ucCardTranList.objtrans);
                    ucCardTranList.btnPrint.Click += (se, ev) => this.PrintClick(sender, e, ucCardTranList.objtrans);
                    FLCardTransList.Controls.Add(ucCardTranList);
                }
            }
        }

        public void DetailsClick(object sender, EventArgs e, ClsTransaction trans)
        {
            //ClsTransaction objtrans = trans;
            //FrmTransactionDetail frmTransdet = new FrmTransactionDetail(trans);
            //FormBlank frmBlank = new FormBlank();
            //frmBlank.Show();
            //frmTransdet.ShowDialog();
            //frmBlank.Close();
        }

        public void ReceiptClick(object sender, EventArgs e, ClsTransaction trans)
        {
            ClsTransaction objtrans = trans;

            DateTime from = dtpFrom.Value;
            DateTime to = dtpTo.Value;
            string shopID = ClsStaticVariable.CurrentShop.ShopID;
            ds = controllerReport.LoadTransactionReceipt(objtrans.TransactionID, shopID, new DateTime(from.Year, from.Month, from.Day, 0, 0, 0), new DateTime(to.Year, to.Month, to.Day, 23, 59, 59));
            reportDoc = new MilenialPark.Reports.PrintTransactionReceipt();
            reportDoc.SetDataSource(ds);

            FrmShowReport frmShowReport = new FrmShowReport(reportDoc);
            FormBlank frmBlank = new FormBlank();
            frmBlank.Show();
            frmShowReport.ShowDialog();
            frmBlank.Close();
        }

        public void PrintClick(object sender, EventArgs e, ClsTransaction trans)
        {
            ClsTransaction objtrans = trans;

            DateTime from = dtpFrom.Value;
            DateTime to = dtpTo.Value;
            ds = controllerReport.LoadTransactionReceipt2(objtrans.TransactionID);
            if (objtrans.TransactionID.Substring(0, 3) == "TRD" || objtrans.TransactionID.Substring(0, 3) == "TRT")
            {
                reportDoc = new MilenialPark.Reports.PrintTransactionReceipt();
            }
            else
            {
                reportDoc = new MilenialPark.Reports.PrintTopUpReceipt();
            }
            reportDoc.SetDataSource(ds);

            this.printdialog1.Document = printdocument;
            DialogResult dr = this.printdialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                //Get the Copy times
                int nCopy = this.printdocument.PrinterSettings.Copies;
                //Get the number of Start Page
                int sPage = this.printdocument.PrinterSettings.FromPage;
                //Get the number of End Page
                int ePage = this.printdocument.PrinterSettings.ToPage;
                //Get the printer name
                string PrinterName = this.printdocument.PrinterSettings.PrinterName;

                try
                {
                    //Set the printer name to print the report to. By default the sample
                    //report does not have a default printer specified. This will tell the
                    //engine to use the specified printer to print the report. Print out 
                    //a test page (from Printer properties) to get the correct value.

                    reportDoc.PrintOptions.PrinterName = this.printdocument.PrinterSettings.PrinterName;

                    //Start the printing process. Provide details of the print job
                    //using the arguments.
                    reportDoc.PrintToPrinter(nCopy, false, sPage, ePage);

                    //Let the user know that the print job is completed
                    MessageBox.Show("Report finished printing!");
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.ToString());
                }
            }
        }

        private void cbxTransType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxTransType.SelectedIndex == 0)
            {
                excludecategory = "WEEKEND";
            }
            else
            {
                excludecategory = "WEEKDAY";
            }
            FillFLPanel(null, null);
        }
    }
}
