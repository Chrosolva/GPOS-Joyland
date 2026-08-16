using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MilenialPark.Controller;
using MilenialPark.Models;
using MilenialPark.Master;
using MilenialPark.UserControls;
using QRCoder;
using CrystalDecisions.CrystalReports.Engine;
using MilenialPark.Views.Reports;
using MilenialPark.Reports;

namespace MilenialPark.Views.Transaction
{
    public partial class FrmPayment : Form
    {
        #region properties

        public ControllerTransaction controllerTran = new ControllerTransaction();
        public ClsTransaction objtrans = new ClsTransaction();
        public ControllerReport controllerReport = new ControllerReport();
        public ReportDocument reportDoc = new ReportDocument();
        public ReportDocument reportQRDoc2 = new ReportDocument();
        bool Mastercard = false;
        public string CustomerName;
        public DataTable dt;
        public DataSet ds;
        public DataSet dsQR;
        public ControllerRFID controllerRFID = new ControllerRFID();


        #endregion
        public FrmPayment()
        {
            InitializeComponent();
        }
        public FrmPayment(ControllerTransaction trans)
        {
            InitializeComponent();
            this.controllerTran = trans;
            lblTotal.Text = controllerTran.objTransaction.totalAmount.ToString("#,##0");
            lblTransactionID.Text = controllerTran.objTransaction.TransactionID;
            cbxPaymentType.SelectedIndex = 0;
            cbxTransType.Text = controllerTran.objTransaction.TransactionType;
        }

        private void cbxPaymentType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void scan()
        {
            txtCardID.Text = Convert.ToInt32(txtCardID.Text).ToString();
            controllerTran.dt = controllerTran.getCard(txtCardID.Text);
            controllerTran.SetCard(txtCardID.Text);
            CustomerName = controllerTran.objCard.CustomerName;
            // check MasterCard 
            if (CustomerName == "MASTER_CARD1")
            {
                cbxPaymentType.SelectedIndex = 1;
                Mastercard = true;
                cbxRemarks.Enabled = true;
            }
            else if (CustomerName == "MASTER_CARD2")
            {
                cbxPaymentType.SelectedIndex = 1;
                Mastercard = true;
                cbxRemarks.Enabled = true;
            }
            else
            {
                cbxPaymentType.SelectedIndex = 0;
                Mastercard = false;
                cbxRemarks.Enabled = false;
                cbxRemarks.SelectedIndex = 0;
            }


            if (controllerTran.dt.Rows.Count == 0)
            {
                ClsFungsi.Pesan("Data Kartu tidak terdaftar pada sistem , mohon hubungi admin !!!", "ERROR");
            }
            else
            {
                controllerTran.objCard = new ClsCard(controllerTran.dt.Rows[0]["CardID"].ToString(), controllerTran.dt.Rows[0]["CustomerName"].ToString(), controllerTran.dt.Rows[0]["NoIdentitas"].ToString(), Convert.ToDecimal(controllerTran.dt.Rows[0]["Saldo"]), Convert.ToBoolean(controllerTran.dt.Rows[0]["Active"]));
                lblCustomerName.Text = controllerTran.objCard.CustomerName;
                lblCardBalance.Text = controllerTran.objCard.Saldo.ToString("#,##0");
            }
        }

        private void txtCardID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;

            txtCardID.Text =
                ClsFungsi.NormalizeCardID(txtCardID.Text);

            if (string.IsNullOrWhiteSpace(txtCardID.Text))
            {
                ClsFungsi.Pesan(
                    "Card ID belum diisi.",
                    "INFO"
                );

                return;
            }

            scan();
        }

        private void btnSave_Click(
    object sender,
    EventArgs e)
        {
            if (!btnSave.Enabled)
                return;

            btnSave.Enabled = false;

            try
            {
                #region Validasi awal

                if (controllerTran == null ||
                    controllerTran.objTransaction == null)
                {
                    ClsFungsi.Pesan(
                        "Data transaksi tidak tersedia.",
                        "ERROR"
                    );

                    return;
                }

                if (ClsStaticVariable.CurrentShop == null)
                {
                    ClsFungsi.Pesan(
                        "Universal Shop belum dimuat.",
                        "ERROR"
                    );

                    return;
                }

                if (string.IsNullOrWhiteSpace(
                    cbxTransType.Text))
                {
                    ClsFungsi.Pesan(
                        "Transaction Type belum dipilih.",
                        "INFO"
                    );

                    return;
                }

                if (string.IsNullOrWhiteSpace(
                    cbxPaymentType.Text))
                {
                    ClsFungsi.Pesan(
                        "Payment Type belum dipilih.",
                        "INFO"
                    );

                    return;
                }

                if (string.IsNullOrWhiteSpace(
                    txtCardID.Text))
                {
                    ClsFungsi.Pesan(
                        "Silakan scan Card ID terlebih dahulu.",
                        "INFO"
                    );

                    txtCardID.Focus();
                    return;
                }

                // Pastikan data kartu terbaru sudah dimuat.
                try
                {
                    scan();
                }
                catch (Exception ex)
                {
                    ClsFungsi.Pesan(
                        "Gagal membaca kartu. " +
                        ex.Message,
                        "ERROR"
                    );

                    txtCardID.Focus();
                    return;
                }

                if (controllerTran.objCard == null ||
                    string.IsNullOrWhiteSpace(
                        controllerTran.objCard.CardID))
                {
                    ClsFungsi.Pesan(
                        "Data kartu tidak ditemukan.",
                        "ERROR"
                    );

                    return;
                }

                if (!controllerTran.objCard.Active)
                {
                    ClsFungsi.Pesan(
                        "Kartu tidak aktif atau sudah diblokir.",
                        "ERROR"
                    );

                    return;
                }

                if (!ValidateTicketRFID())
                {
                    FocusRFIDScan();
                    return;
                }

                #endregion

                #region Validasi saldo

                decimal totalAmount =
                    controllerTran.objTransaction.totalAmount;

                decimal cardBalance =
                    controllerTran.objCard.Saldo;

                bool isCardPayment =
                    string.Equals(
                        cbxPaymentType.Text,
                        "CARD",
                        StringComparison.OrdinalIgnoreCase
                    );

                bool isMasterCard =
                    string.Equals(
                        cbxPaymentType.Text,
                        "MASTER_CARD",
                        StringComparison.OrdinalIgnoreCase
                    );

                if (isCardPayment && cardBalance <= 0)
                {
                    ClsFungsi.Pesan(
                        "Maaf, saldo kartu kosong. " +
                        "Silakan isi terlebih dahulu.",
                        "INFO"
                    );

                    return;
                }

                if (isCardPayment &&
                    cardBalance < totalAmount)
                {
                    ClsFungsi.Pesan(
                        "Maaf, saldo kartu tidak mencukupi.",
                        "ERROR"
                    );

                    return;
                }

                if (!isCardPayment && !isMasterCard)
                {
                    ClsFungsi.Pesan(
                        "Payment Type tidak dikenali.",
                        "ERROR"
                    );

                    return;
                }

                #endregion

                #region Validasi total grid

                decimal gridTotal =
                    CalculatePaymentGridTotal();

                if (controllerTran.objTransaction.totalAmount
                    != gridTotal)
                {
                    ClsFungsi.Pesan(
                        "Terjadi perbedaan total transaksi.\n\n" +
                        "Total transaksi : " +
                        controllerTran.objTransaction
                            .totalAmount
                            .ToString("#,##0") +
                        "\nTotal detail : " +
                        gridTotal.ToString("#,##0") +
                        "\n\nSilakan tutup pembayaran dan " +
                        "muat ulang pesanan.",
                        "ERROR"
                    );

                    return;
                }

                #endregion

                DialogResult confirmation =
                    MessageBox.Show(
                        "Apakah Anda yakin ingin menyimpan " +
                        "transaksi ini?\n\n" +
                        "Transaction ID sementara: " +
                        lblTransactionID.Text +
                        "\nTotal: " +
                        totalAmount.ToString("#,##0"),
                        "Konfirmasi Pembayaran",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                if (confirmation != DialogResult.Yes)
                {
                    return;
                }

                #region Generate Transaction ID

                string shopID =
                    ClsStaticVariable.CurrentShop.ShopID;

                controllerTran.AutogenereateTransactionID(
                    "TICKET",
                    shopID
                );

                if (string.IsNullOrWhiteSpace(
                    controllerTran.TransactionID))
                {
                    ClsFungsi.Pesan(
                        "Gagal membuat Transaction ID.",
                        "ERROR"
                    );

                    return;
                }

                string finalTransactionID =
                    controllerTran.TransactionID;

                controllerTran.objTransaction.TransactionID =
                    finalTransactionID;

                controllerTran.objTransaction.ShopId =
                    shopID;

                controllerTran.objTransaction.TransactionType =
                    cbxTransType.Text;

                lblTransactionID.Text =
                    finalTransactionID;

                #endregion

                #region Update ordinary transaction detail

                if (controllerTran.objTransaction.listtransdet ==
                    null)
                {
                    controllerTran.objTransaction.listtransdet =
                        new List<ClsTransactionDetail>();
                }

                foreach (
                    ClsTransactionDetail detail
                    in controllerTran.objTransaction.listtransdet)
                {
                    detail.TransactionID =
                        finalTransactionID;
                }

                #endregion

                #region Build ticket detail

                BuildTicketDetails(finalTransactionID);
                BuildOrdinaryDetails(finalTransactionID);


                if (controllerTran.objTransaction
                        .listtranstikdet.Count == 0 &&
                    controllerTran.objTransaction
                        .listtransdet.Count == 0)
                {
                    ClsFungsi.Pesan(
                        "Transaksi tidak memiliki detail item.",
                        "ERROR"
                    );

                    return;
                }

                #endregion

                #region Set payment data

                controllerTran.objTransaction.CardID =
                    controllerTran.objCard.CardID;

                controllerTran.objTransaction.Remarks =
                    txtRemarks.Text.Trim();

                controllerTran.objTransaction.PaymentType =
                    cbxPaymentType.Text;

                controllerTran.objTransaction.InitialBalance =
                    isMasterCard
                        ? controllerTran.objCard.Saldo
                        : cardBalance;

                #endregion

                #region Precheck log

                StringBuilder itemChecking =
                    new StringBuilder();

                int itemIndex = 1;

                foreach (
                    ClsTransactionTiketDetail detail
                    in controllerTran.objTransaction
                        .listtranstikdet)
                {
                    decimal subtotal =
                        detail.Price * detail.Qty;

                    itemChecking.Append(
                        "Item-" + itemIndex +
                        ": ItemID=" + detail.ItemId +
                        ", ItemName=" + detail.ItemName +
                        ", Qty=" + detail.Qty +
                        ", Price=" + detail.Price +
                        ", Subtotal=" + subtotal +
                        ", NoUrut=" + detail.NoUrut +
                        ", RFID=" + detail.RFID +
                        ", TagID=" + detail.TagID +
                        "; "
                    );

                    itemIndex++;
                }

                foreach (
                    ClsTransactionDetail detail
                    in controllerTran.objTransaction
                        .listtransdet)
                {
                    decimal subtotal =
                        detail.Price * detail.Qty;

                    itemChecking.Append(
                        "Item-" + itemIndex +
                        ": ItemID=" + detail.ItemId +
                        ", ItemName=" + detail.ItemName +
                        ", Qty=" + detail.Qty +
                        ", Price=" + detail.Price +
                        ", Subtotal=" + subtotal +
                        "; "
                    );

                    itemIndex++;
                }

                string logMessage =
                    "PRECHECK! " +
                    "TOTALAMOUNT=" +
                    gridTotal +
                    "; ITEMCHECKING=" +
                    itemChecking;

                controllerTran.InsertLogMessage(
                    finalTransactionID,
                    logMessage
                );

                #endregion

                #region Insert transaction

                string insertResult =
                    controllerTran.InsertTransactionTicket(
                        controllerTran.objTransaction,
                        controllerTran.objCard
                    );

                if (!ClsStaticVariable.sukses)
                {
                    ClsFungsi.Pesan(
                        insertResult,
                        "ERROR"
                    );

                    return;
                }

                ClsFungsi.Pesan(
                    insertResult,
                    "INFO"
                );

                #endregion

                #region Printing

                //try
                //{
                //    PrintQR(controllerTran);
                //}
                //catch (Exception ex)
                //{
                //    ClsFungsi.Pesan(
                //        "Transaksi berhasil disimpan, tetapi " +
                //        "QR Code gagal dicetak.\n" +
                //        ex.Message,
                //        "WARNING"
                //    );
                //}

                try
                {
                    PrintStruck(controllerTran);
                }
                catch (Exception ex)
                {
                    ClsFungsi.Pesan(
                        "Transaksi berhasil disimpan, tetapi " +
                        "struk gagal dicetak.\n" +
                        ex.Message,
                        "WARNING"
                    );
                }

                #endregion

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ClsFungsi.Pesan(
                    "Terjadi error pada proses pembayaran.\n" +
                    ex.Message,
                    "ERROR"
                );
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnSave.Enabled = true;
                }
            }
        }

        private bool ValidateTicketRFID()
        {
            List<string> missingRows = new List<string>();
            List<string> tagIDs = new List<string>();

            foreach (DataGridViewRow row in dgvTransacTiketDet.Rows)
            {
                if (row.IsNewRow)
                    continue;

                int noUrut = ToIntSafe(row.Cells["NoUrut"].Value);
                string rfid = GetCellString(row, "RFID");
                string tagID = GetCellString(row, "TagID");

                if (string.IsNullOrWhiteSpace(rfid) ||
                    string.IsNullOrWhiteSpace(tagID))
                {
                    missingRows.Add(noUrut.ToString());
                }

                if (!string.IsNullOrWhiteSpace(tagID))
                {
                    if (tagIDs.Any(x =>
                        string.Equals(
                            x,
                            tagID,
                            StringComparison.OrdinalIgnoreCase)))
                    {
                        ClsFungsi.Pesan(
                            "TagID RFID '" + tagID +
                            "' digunakan lebih dari satu tiket.",
                            "ERROR"
                        );

                        return false;
                    }

                    tagIDs.Add(tagID);
                }
            }

            if (missingRows.Count > 0)
            {
                ClsFungsi.Pesan(
                    "RFID belum lengkap pada NoUrut: " +
                    string.Join(", ", missingRows),
                    "ERROR"
                );

                return false;
            }

            return true;
        }

        private decimal ParseDecimalSafe(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            decimal result;

            if (decimal.TryParse(
                value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.CurrentCulture,
                out result))
            {
                return result;
            }

            string normalized =
                value.Replace(".", "")
                     .Replace(",", "");

            return decimal.TryParse(normalized, out result)
                ? result
                : 0m;
        }

        private decimal CalculatePaymentGridTotal()
        {
            decimal total = 0m;

            foreach (DataGridViewRow row in dgvTransacTiketDet.Rows)
            {
                if (row.IsNewRow)
                    continue;

                decimal price =
                    ToDecimalSafe(row.Cells["Price"].Value);

                int qty =
                    ToIntSafe(row.Cells["Qty"].Value);

                total += price * qty;
            }

            foreach (DataGridViewRow row in dgvTransaksiDetail.Rows)
            {
                if (row.IsNewRow)
                    continue;

                decimal price =
                    ToDecimalSafe(row.Cells["Price2"].Value);

                int qty =
                    ToIntSafe(row.Cells["Qty2"].Value);

                total += price * qty;
            }

            return total;
        }

        private void BuildTicketDetails(string transactionID)
        {
            controllerTran.objTransaction.listtranstikdet =
                new List<ClsTransactionTiketDetail>();

            foreach (DataGridViewRow row in dgvTransacTiketDet.Rows)
            {
                if (row.IsNewRow)
                    continue;

                ClsTransactionTiketDetail detail =
                    new ClsTransactionTiketDetail(
                        transactionid: transactionID,
                        transactiondate: DateTime.Now,
                        itemid: GetCellString(row, "ItemID"),
                        itemname: GetCellString(row, "ItemName"),
                        price: ToDecimalSafe(
                            row.Cells["Price"].Value
                        ),
                        qty: ToIntSafe(
                            row.Cells["Qty"].Value
                        ),
                        noUrut: ToIntSafe(
                            row.Cells["NoUrut"].Value
                        ),
                        orderStatus: "BOUGHT",
                        jamMasuk: DateTime.Now,
                        jamKeluar: DateTime.Now,
                        waktuBermain: ToIntSafe(
                            row.Cells["WaktuBermain"].Value
                        ),
                        toleransi: ToIntSafe(
                            row.Cells["Toleransi"].Value
                        ),
                        rfid: GetCellString(row, "RFID"),
                        keterangan:
                            GetCellString(row, "Keterangan"),
                        tagid: GetCellString(row, "TagID")
                    );

                controllerTran.objTransaction
                    .listtranstikdet
                    .Add(detail);
            }
        }
        private void BuildOrdinaryDetails(
    string transactionID)
        {
            controllerTran.objTransaction.listtransdet =
                new List<ClsTransactionDetail>();

            foreach (DataGridViewRow row
                     in dgvTransaksiDetail.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                ClsTransactionDetail detail =
                    new ClsTransactionDetail(
                        transactionID,
                        DateTime.Now,
                        Convert.ToString(row.Cells[2].Value),
                        Convert.ToString(row.Cells[3].Value),
                        ToDecimalSafe(
                            row.Cells["Price2"].Value
                        ),
                        ToIntSafe(
                            row.Cells["Qty2"].Value
                        ),
                        ToIntSafe(
                            row.Cells[6].Value
                        ),
                        "BOUGHT"
                    );

                controllerTran.objTransaction
                    .listtransdet
                    .Add(detail);
            }
        }

        public void PrintQR(ControllerTransaction trans)
        {
            string tmp;
            string tmp2;
            List<string> listqrcode = new List<string>();
            List<string> listitemname = new List<string>();
            // get list ticket and 
            dt = controllerTran.gettransactionTiketDetail(trans.objTransaction.TransactionID);
            foreach (DataRow row in dt.Rows)
            {
                if (row["category"].ToString() != "ACTIVITY")
                {
                    tmp = "(&" + row["TransactionID"].ToString() + "&" + row["NoUrut"].ToString() + ")";
                    tmp2 = row["ItemName"].ToString();
                    listqrcode.Add(tmp);
                    listitemname.Add(tmp2);
                }
            }
            //ClsFungsi.Pesan(listqrcode.ToString(), "INFO");
            // generate qrcode 
            List<byte[]> listQrCodes = new List<byte[]>();
            QRCodeGenerator qrGenerator = new QRCodeGenerator();

            foreach (String t in listqrcode)
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(t, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(5);

                byte[] yourByteArray;
                using (var mStream = new System.IO.MemoryStream())
                {
                    qrCodeImage.Save(mStream, System.Drawing.Imaging.ImageFormat.Bmp);
                    yourByteArray = mStream.ToArray();
                    listQrCodes.Add(yourByteArray);
                }
            }

            dsQR = controllerTran.LoadListQRCodes(listQrCodes, listqrcode, listitemname);
            // Show or Print Tiket 

            reportQRDoc2 = new PrintQRCode();
            reportQRDoc2.SetDataSource(dsQR);

            //MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            //DialogResult result = MessageBox.Show("Data Ticket berhasil Diload !!! \n Lanjut Cetak Ticket ?", "Print Ticket ? ", buttons);
            //if (result == DialogResult.Yes)
            //{

            //}
            //else
            //{

            //}

            if (listQrCodes.Count > 0)
            {
                PrintQRCode(reportQRDoc2);
            }
            else
            {

            }
            //// check transaction type 
            //if (dgvTransTiket.CurrentRow.Cells["TransactionType"].Value.ToString() == "ONE-TIME-TICKET")
            //{

            //}
            //else
            //{
            //    ClsFungsi.Pesan("Tidak bisa mencetak QRCode karena tiket tersebut bukan ONE TIME TICKET, silahkan gunakan kartu untuk masuk", "INFO");
            //}
        }

        public void PrintQRCode(ReportDocument reportQRDoc)
        {

            if (reportQRDoc != null)
            {
                //Reports.FrmShowReport formListItem = new Reports.FrmShowReport(reportQRDoc);
                //formListItem.ShowDialog();
                reportQRDoc.PrintToPrinter(1, false, 0, 0);
            }
            else
            {
                MessageBox.Show("Tidak ada QRCode yang akan di cetak");
            }
        }

        public void PrintStruck(ControllerTransaction trans)
        {
            ds = controllerReport.LoadTransactionReceipt2(trans.objTransaction.TransactionID, trans.objTransaction.ShopId, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59));
            string sub3 = trans.objTransaction.TransactionID.ToString().Substring(0, 3);
            if (sub3 == "TRK" || sub3 == "TRR")
            {
                reportDoc = new MilenialPark.Reports.PrintTopUpReceipt();
            }
            else
            {
                reportDoc = new MilenialPark.Reports.PrintTransactionReceipt();
            }
            reportDoc.SetDataSource(ds);

            FrmShowReport frmShowReport = new FrmShowReport(reportDoc);
            FormBlank frmBlank = new FormBlank();
            frmBlank.Show();
            frmShowReport.ShowDialog();
            frmBlank.Close();

            //reportDoc.PrintToPrinter(1, false, 0, 0);
        }

        private void cbxRemarks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRemarks.SelectedIndex > 0)
            {
                txtRemarks.Text = cbxRemarks.Text + " : ";
            }
            else if (cbxRemarks.SelectedIndex < 0)
            {
                txtRemarks.Text = "";
            }
        }

        public void generatetransacttiketdetail(ClsTransactionTiketDetail det)
        {
            for (int i = 0; i < det.Qty; i++)
            {
                DataGridViewRow row = (DataGridViewRow)dgvTransacTiketDet.Rows[0].Clone();
                //Clstr objtransdet = new ClsTransactionDetail("ORDTMP", DateTime.Now, objShopItem.ItemID, objShopItem.ItemName, objShopItem.Price, Convert.ToInt32(NUDQty.Value), "NOTSERVED");
                row.Cells[0].Value = lblTransactionID.Text;
                row.Cells[1].Value = DateTime.Now;
                row.Cells[2].Value = det.ItemId;
                row.Cells[3].Value = det.ItemName;
                row.Cells[4].Value = det.Price;
                row.Cells[5].Value = 1;
                row.Cells[6].Value = i + 1;
                row.Cells[7].Value = "TMP";
                row.Cells[8].Value = DateTime.Now;
                row.Cells[9].Value = DateTime.Now;
                row.Cells[10].Value = det.WaktuBermain;
                row.Cells[11].Value = det.Toleransi;
                dgvTransacTiketDet.Rows.Add(row);
            }
        }

        private void FrmPayment_Load(
    object sender,
    EventArgs e)
        {
            if (controllerTran == null ||
                controllerTran.objTransaction == null)
            {
                ClsFungsi.Pesan(
                    "Data transaksi tidak tersedia.",
                    "ERROR"
                );

                BeginInvoke(new Action(Close));
                return;
            }

            DataGridViewHelper.ApplyPOSStyle(dgvTransacTiketDet);
            DataGridViewHelper.SizeCompact(dgvTransacTiketDet, 100, 420);
            DataGridViewHelper.ApplyPOSStyle(dgvTransaksiDetail);
            DataGridViewHelper.SizeCompact(dgvTransaksiDetail, 100, 420);

            dgvTransacTiketDet.Rows.Clear();
            dgvTransaksiDetail.Rows.Clear();

            if (controllerTran.objTransaction.listtranstikdet == null)
            {
                controllerTran.objTransaction.listtranstikdet =
                    new List<ClsTransactionTiketDetail>();
            }

            int noUrut = 1;

            foreach (
                ClsTransactionTiketDetail detail
                in controllerTran.objTransaction.listtranstikdet)
            {
                if (detail.WaktuBermain > 0)
                {
                    AddTicketRows(
                        detail,
                        ref noUrut
                    );
                }
                else
                {
                    AddNonTicketRow(
                        detail,
                        noUrut
                    );

                    noUrut++;
                }
            }

            cbxRemarks.SelectedIndex = 0;
            cbxRemarks.Enabled = false;

            dgvTransacTiketDet.ReadOnly = false;

            dgvTransacTiketDet
                .Columns["RFID"]
                .ReadOnly = true;

            dgvTransacTiketDet
                .Columns["TagID"]
                .ReadOnly = true;

            dgvTransacTiketDet
                .Columns["Keterangan"]
                .ReadOnly = false;

            if (dgvTransacTiketDet.Rows.Count > 0)
            {
                dgvTransacTiketDet.CurrentCell =
                    dgvTransacTiketDet
                        .Rows[0]
                        .Cells["RFID"];
            }

            FocusRFIDScan();
        }

        private static decimal ToDecimalSafe(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0m;

            decimal result;

            return decimal.TryParse(
                value.ToString(),
                out result
            )
                ? result
                : 0m;
        }

        private static int ToIntSafe(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            int result;

            return int.TryParse(
                value.ToString(),
                out result
            )
                ? result
                : 0;
        }

        private static string GetCellString(
            DataGridViewRow row,
            string columnName)
        {
            if (row == null ||
                !row.DataGridView.Columns.Contains(columnName))
            {
                return "";
            }

            object value = row.Cells[columnName].Value;

            return value == null ||
                   value == DBNull.Value
                ? ""
                : value.ToString().Trim();
        }

        private void AddTicketRows(
    ClsTransactionTiketDetail detail,
    ref int noUrut)
        {
            int qty = detail.Qty <= 0
                ? 1
                : detail.Qty;

            for (int i = 0; i < qty; i++)
            {
                int rowIndex =
                    dgvTransacTiketDet.Rows.Add();

                DataGridViewRow row =
                    dgvTransacTiketDet.Rows[rowIndex];

                row.Cells["TransactionID"].Value =
                    controllerTran.objTransaction.TransactionID;

                row.Cells["RFID"].Value =
                    detail.RFID ?? "";

                row.Cells["TagID"].Value =
                    detail.TagID ?? "";

                row.Cells["Keterangan"].Value =
                    detail.Keterangan ?? "";

                row.Cells["TransactionDate"].Value =
                    DateTime.Now;

                row.Cells["ItemID"].Value =
                    detail.ItemId;

                row.Cells["ItemName"].Value =
                    detail.ItemName;

                row.Cells["Price"].Value =
                    detail.Price;

                row.Cells["Qty"].Value = 1;

                row.Cells["NoUrut"].Value =
                    noUrut;

                row.Cells["OrderStatus"].Value =
                    "TMP";

                row.Cells["JamMasuk"].Value =
                    DateTime.Now;

                row.Cells["JamKeluar"].Value =
                    DateTime.Now;

                row.Cells["WaktuBermain"].Value =
                    detail.WaktuBermain;

                row.Cells["Toleransi"].Value =
                    detail.Toleransi;

                noUrut++;
            }
        }

        private void AddNonTicketRow(
    ClsTransactionTiketDetail detail,
    int noUrut)
        {
            dgvTransaksiDetail.Rows.Add(
                controllerTran.objTransaction.TransactionID,
                DateTime.Now,
                detail.ItemId,
                detail.ItemName,
                detail.Price,
                detail.Qty,
                noUrut,
                detail.OrderStatus ?? "TMP",
                DateTime.Now,
                DateTime.Now,
                0,
                0
            );
        }

        public void FocusRFIDScan()
        {
            if (!txtRFIDScan.Enabled || !txtRFIDScan.Visible) return;

            txtRFIDScan.Focus();
            txtRFIDScan.SelectAll();   // so next scan overwrites immediately
        }

        private void txtRFIDScan_KeyDown(
    object sender,
    KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;

            if (dgvTransacTiketDet.CurrentRow == null ||
                dgvTransacTiketDet.CurrentRow.IsNewRow)
            {
                ClsFungsi.Pesan(
                    "Pilih baris tiket terlebih dahulu.",
                    "INFO"
                );

                FocusRFIDScan();
                return;
            }

            string tagID =
                NormalizeTagID(txtRFIDScan.Text);

            if (string.IsNullOrWhiteSpace(tagID))
            {
                FocusRFIDScan();
                return;
            }

            DataTable tagData =
                controllerRFID.GetRFIDByTagID(tagID);

            if (tagData == null ||
                tagData.Rows.Count == 0)
            {
                ClsFungsi.Pesan(
                    "TagID tidak terdaftar atau RFID tidak aktif.",
                    "ERROR"
                );

                txtRFIDScan.Clear();
                FocusRFIDScan();
                return;
            }

            DataRow tagRow = tagData.Rows[0];

            string rfidName =
                tagRow["RFIDName"] == DBNull.Value
                    ? ""
                    : tagRow["RFIDName"].ToString().Trim();

            if (string.IsNullOrWhiteSpace(rfidName))
            {
                ClsFungsi.Pesan(
                    "RFIDName belum diisi pada master RFID.",
                    "ERROR"
                );

                txtRFIDScan.Clear();
                FocusRFIDScan();
                return;
            }

            bool duplicateInGrid =
                dgvTransacTiketDet.Rows
                    .Cast<DataGridViewRow>()
                    .Any(row =>
                        !row.IsNewRow &&
                        row != dgvTransacTiketDet.CurrentRow &&
                        string.Equals(
                            GetCellString(row, "TagID"),
                            tagID,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );

            if (duplicateInGrid)
            {
                ClsFungsi.Pesan(
                    "RFID tersebut sudah digunakan pada tiket lain dalam transaksi ini.",
                    "WARNING"
                );

                txtRFIDScan.Clear();
                FocusRFIDScan();
                return;
            }

            DateTime startDay = DateTime.Today;
            DateTime endDay =
                DateTime.Today.AddDays(1).AddTicks(-1);

            DataTable usedBought =
                controllerTran.GetTicketByTagID(
                    tagID,
                    "BOUGHT",
                    startDay,
                    endDay
                );

            DataTable usedEnterIn =
                controllerTran.GetTicketByTagID(
                    tagID,
                    "ENTER-IN",
                    startDay,
                    endDay
                );

            bool alreadyUsed =
                (usedBought != null &&
                 usedBought.Rows.Count > 0) ||
                (usedEnterIn != null &&
                 usedEnterIn.Rows.Count > 0);

            if (alreadyUsed)
            {
                ClsFungsi.Pesan(
                    "RFID sedang digunakan oleh tiket aktif lain.",
                    "INFO"
                );

                txtRFIDScan.Clear();
                FocusRFIDScan();
                return;
            }

            DataGridViewRow currentRow =
                dgvTransacTiketDet.CurrentRow;

            currentRow.Cells["TagID"].Value =
                tagID;

            currentRow.Cells["RFID"].Value =
                rfidName;

            currentRow.Cells["Keterangan"].Value =
                txtKeterangan.Text.Trim();

            controllerRFID.TouchLastScan(tagID);

            MoveToNextTicketRow();

            txtRFIDScan.Clear();
            FocusRFIDScan();
        }

        private void MoveToNextTicketRow()
        {
            if (dgvTransacTiketDet.CurrentRow == null)
            {
                return;
            }

            int currentIndex =
                dgvTransacTiketDet.CurrentRow.Index;

            for (
                int index = currentIndex + 1;
                index < dgvTransacTiketDet.Rows.Count;
                index++)
            {
                DataGridViewRow nextRow =
                    dgvTransacTiketDet.Rows[index];

                if (nextRow.IsNewRow)
                {
                    continue;
                }

                string nextTagID =
                    GetCellString(nextRow, "TagID");

                if (string.IsNullOrWhiteSpace(nextTagID))
                {
                    dgvTransacTiketDet.CurrentCell =
                        nextRow.Cells["RFID"];

                    nextRow.Selected = true;
                    SyncKeteranganFromCurrentRow();
                    return;
                }
            }
        }

        private void txtKeterangan_TextChanged(
    object sender,
    EventArgs e)
        {
            if (dgvTransacTiketDet.CurrentRow == null ||
                dgvTransacTiketDet.CurrentRow.IsNewRow)
            {
                return;
            }

            dgvTransacTiketDet
                .CurrentRow
                .Cells["Keterangan"]
                .Value =
                txtKeterangan.Text;
        }

        private void FrmPayment_Shown(
    object sender,
    EventArgs e)
        {
            BeginInvoke(
                new Action(
                    delegate
                    {
                        FocusRFIDScan();
                    }
                )
            );
        }

        private void dgvTransacTiketDet_SelectionChanged(
    object sender,
    EventArgs e)
        {
            SyncKeteranganFromCurrentRow();
        }

        private void SyncKeteranganFromCurrentRow()
        {
            if (dgvTransacTiketDet.CurrentRow == null ||
                dgvTransacTiketDet.CurrentRow.IsNewRow)
            {
                txtKeterangan.Text = "";
                return;
            }

            string keterangan =
                GetCellString(
                    dgvTransacTiketDet.CurrentRow,
                    "Keterangan"
                );

            if (txtKeterangan.Text != keterangan)
            {
                txtKeterangan.Text = keterangan;
            }
        }

        private string NormalizeTagID(string rawTagID)
        {
            if (string.IsNullOrWhiteSpace(rawTagID))
            {
                return "";
            }

            string value = rawTagID.Trim();

            if (!value.All(char.IsDigit))
            {
                ClsFungsi.Pesan(
                    "TagID RFID harus berupa angka.",
                    "ERROR"
                );

                return "";
            }

            string normalized = value.TrimStart('0');

            return normalized.Length == 0
                ? "0"
                : normalized;
        }

        private void txtCardID_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Izinkan tombol kontrol: Backspace, Delete, Ctrl+C, Ctrl+V, dll.
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Tolak selain angka.
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtCardID_TextChanged(object sender, EventArgs e)
        {
            string numericOnly =
                new string(
                    txtCardID.Text
                        .Where(char.IsDigit)
                        .ToArray()
                );

            if (txtCardID.Text == numericOnly)
            {
                return;
            }

            int selectionStart = txtCardID.SelectionStart;

            txtCardID.Text = numericOnly;

            txtCardID.SelectionStart =
                Math.Min(selectionStart, txtCardID.Text.Length);
        }
    }


}
