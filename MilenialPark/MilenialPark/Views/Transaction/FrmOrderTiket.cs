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
using MilenialPark.Master;
using MilenialPark.Views;
using QRCoder;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Windows.Forms;
using MilenialPark.Reports;
using MilenialPark.Views.Reports;

namespace MilenialPark.Views.Transaction
{
    public partial class FrmOrderTiket : Form
    {
        public Mainform parentfrm;
        public ControllerShop controllerShop = new ControllerShop();
        public ControllerTransaction controllerTrans = new ControllerTransaction();
        public BindingSource bind = new BindingSource();
        public BindingSource bind2 = new BindingSource();
        public string filepath;
        public DataTable dt2 = new DataTable();
        DateTime from;
        DateTime to;
        string SearchCard = "";

        public DataSet ds = new DataSet();
        public DataSet dsQR = new DataSet();
        public ReportDocument reportQRDoc2 = new ReportDocument();
        public ControllerReport controllerReport = new ControllerReport();

        public ReportDocument reportDoc = new ReportDocument();
        public string substring3;

        public FrmOrderTiket()
        {
            InitializeComponent();
        }

        public FrmOrderTiket(Mainform parent)
        {
            parentfrm = parent;
            InitializeComponent();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            FrmNEOrderTiket frmNEOrderTiket = new FrmNEOrderTiket(ClsStaticVariable.CurrentShop.ShopID);
            frmNEOrderTiket.Tag = "ADD";
            frmNEOrderTiket.lblFormTitle.Text = "Add Tiket Order";
            frmNEOrderTiket.btnAddorEdit.Image = Resource.tab;
            frmNEOrderTiket.btnAddorEdit.Text = "Create";

            //FormBlank frmBlank = new FormBlank();
            //frmBlank.Show();
            frmNEOrderTiket.ShowDialog();
            //frmBlank.Close();
            //getShop();

            btnFilter_Click(sender, e);
        }

        private void FrmOrderTiket_Load(
    object sender,
    EventArgs e)
        {
            if (ClsStaticVariable.CurrentShop == null)
            {
                ClsFungsi.Pesan(
                    "Universal Shop belum dimuat. " +
                    "Silakan login kembali.",
                    "ERROR"
                );

                BeginInvoke(new Action(Close));
                return;
            }

            dtpFrom.Value =
                DateTime.Today;

            dtpTo.Value =
                DateTime.Today;

            from = dtpFrom.Value;
            to = dtpTo.Value;

            cbxOption.SelectedIndex = 0;
            cbxTransType.SelectedIndex = 0;

            ApplyUserPermission();

            btnFilter_Click(null, null);
        }

        private void ApplyUserPermission()
        {
            bool isAdmin =
                ClsStaticVariable.controllerUser != null &&
                ClsStaticVariable.controllerUser.objUser != null &&
                string.Equals(
                    ClsStaticVariable
                        .controllerUser
                        .objUser
                        .TipeUser,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase
                );

            btnEdit.Enabled = isAdmin;
            btnDelete.Enabled = isAdmin;
        }

        public void btnFilter_Click(
    object sender,
    EventArgs e)
        {
            if (ClsStaticVariable.CurrentShop == null)
            {
                ClsFungsi.Pesan(
                    "Universal Shop belum dimuat.",
                    "ERROR"
                );

                return;
            }

            string shopID =
                ClsStaticVariable.CurrentShop.ShopID;

            SearchCard =
                txtCardID.Text.Trim();

            string searchOption =
                GetSafeSearchOption(cbxOption.Text);

            string transactionType =
                cbxTransType.Text == "ALL"
                    ? "%"
                    : cbxTransType.Text;

            DateTime dateFrom =
                new DateTime(
                    dtpFrom.Value.Year,
                    dtpFrom.Value.Month,
                    dtpFrom.Value.Day,
                    0,
                    0,
                    0
                );

            DateTime dateTo =
                new DateTime(
                    dtpTo.Value.Year,
                    dtpTo.Value.Month,
                    dtpTo.Value.Day,
                    23,
                    59,
                    59
                );

            controllerTrans.dt =
                controllerTrans.getTransaction2(
                    shopID,
                    dateFrom,
                    dateTo,
                    SearchCard,
                    searchOption,
                    transactionType
                );

            bind.DataSource =
                controllerTrans.dt;

            dgvTransTiket.DataSource =
                bind;

            lblrow.Text =
                "Row Count : " +
                controllerTrans.dt.Rows.Count;

            if (controllerTrans.dt.Rows.Count > 0)
            {
                dgvTransTiket.ClearSelection();

                dgvTransTiket.CurrentCell =
                    dgvTransTiket.Rows[0]
                        .Cells["TransactionID"];

                dgvTransTiket.Rows[0].Selected =
                    true;

                LoadSelectedTransactionDetails();
            }
            else
            {
                dgvTransTiketDetail.DataSource =
                    null;

                bind2.DataSource =
                    null;
            }
        }

        private string GetSafeSearchOption(
    string selectedOption)
        {
            switch (selectedOption)
            {
                case "TransactionID":
                    return "TransactionID";

                case "CardID":
                    return "CardID";

                case "Remarks":
                    return "Remarks";

                case "PaymentType":
                    return "PaymentType";

                default:
                    return "TransactionID";
            }
        }

        private void dgvTransTiket_SelectionChanged(
    object sender,
    EventArgs e)
        {
            if (dgvTransTiket.DataSource == null)
            {
                return;
            }

            LoadSelectedTransactionDetails();
        }

        private void btnPrintQR_Click(object sender, EventArgs e)
        {
            if (dgvTransTiket.Rows.Count > 0)
            {
                string tmp;
                string tmp2;
                List<string> listqrcode = new List<string>();
                List<string> listitemname = new List<string>();
                // get list ticket and 
                foreach (DataGridViewRow row in dgvTransTiketDetail.Rows)
                {
                    if (row.Cells["category"].Value.ToString() != "ACTIVITY")
                    {
                        tmp = "(&" + row.Cells["TransactionID"].Value.ToString() + "&" + row.Cells["NoUrut"].Value.ToString() + ")";
                        tmp2 = row.Cells["ItemName"].Value.ToString();
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

                dsQR = controllerTrans.LoadListQRCodes(listQrCodes, listqrcode, listitemname);
                // Show or Print Tiket 

                reportQRDoc2 = new PrintQRCode();
                reportQRDoc2.SetDataSource(dsQR);

                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show("Data Ticket berhasil Diload !!! \n Lanjut Cetak Ticket ?", "Print Ticket ? ", buttons);
                if (result == DialogResult.Yes)
                {
                    if (listQrCodes.Count > 0)
                    {
                        PrintQRCode(reportQRDoc2);
                    }
                    else
                    {

                    }
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

        private void btnExtendTicket_Click(object sender, EventArgs e)
        {
            if (dgvTransTiketDetail.Rows.Count > 0 && dgvTransTiketDetail.CurrentRow.Cells["OrderStatus"].Value.ToString() == "OVERTIME")
            {
                FrmChangeTicketStatus frmCTStatus = new FrmChangeTicketStatus(dgvTransTiketDetail.CurrentRow.Cells["TransactionID"].Value.ToString(), Convert.ToInt32(dgvTransTiketDetail.CurrentRow.Cells["NoUrut"].Value), ClsStaticVariable.CurrentShop.ShopID);
                frmCTStatus.ShowDialog();

                btnFilter_Click(null, null);
            }
            else
            {
                ClsFungsi.Pesan("Maaf tidak ada baris pada tabel detail tiket atau status tiket bukan OVERTIME", "INFO");
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (ClsStaticVariable.controllerUser.objUser.TipeUser == "Admin")
            {
                if (dgvTransTiket.Rows.Count != 0)
                {

                }
            }
            else
            {
                ClsFungsi.Pesan("Maaf Anda Bukan Admin !!!");
            }
        }

        private void btnPrintStruk_Click(object sender, EventArgs e)
        {
            if (dgvTransTiket.Rows.Count != 0)
            {
                //ClsTransaction objtrans = trans;

                DateTime from = dtpFrom.Value;
                DateTime to = dtpTo.Value;
                ds = controllerReport.LoadTransactionReceipt2(dgvTransTiket.CurrentRow.Cells["TransactionID"].Value.ToString(), ClsStaticVariable.CurrentShop.ShopID, new DateTime(from.Year, from.Month, from.Day, 0, 0, 0), new DateTime(to.Year, to.Month, to.Day, 23, 59, 59));
                string sub3 = dgvTransTiket.CurrentRow.Cells["TransactionID"].Value.ToString().Substring(0, 3);
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
        }

        private void txtCardID_KeyUp(
    object sender,
    KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;

            if (cbxOption.Text == "CardID")
            {
                txtCardID.Text =
                    ClsFungsi.NormalizeCardID(
                        txtCardID.Text
                    );
            }

            btnFilter_Click(null, null);
        }

        private void cbxTransType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void LoadSelectedTransactionDetails()
        {
            dgvTransTiketDetail.DataSource = null;
            bind2.DataSource = null;
            dt2.Clear();

            if (dgvTransTiket.CurrentRow == null ||
                dgvTransTiket.CurrentRow.IsNewRow)
            {
                return;
            }

            object transactionValue =
                dgvTransTiket.CurrentRow
                    .Cells["TransactionID"]
                    .Value;

            if (transactionValue == null ||
                transactionValue == DBNull.Value)
            {
                return;
            }

            string transactionID =
                transactionValue.ToString().Trim();

            if (string.IsNullOrWhiteSpace(transactionID))
            {
                return;
            }

            dt2 =
                controllerTrans.GetCombinedTransactionDetails(
                    transactionID
                );

            bind2.DataSource = dt2;
            dgvTransTiketDetail.DataSource = bind2;

            lblDetailRow.Text =
                "Detail Count : " + dt2.Rows.Count;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (dgvTransTiket.Rows.Count > 0)
            {

                string tmp;
                string tmp2;
                List<string> listqrcode = new List<string>();
                List<string> listitemname = new List<string>();
                // get list ticket and 
                foreach (DataGridViewRow row in dgvTransTiketDetail.Rows)
                {
                    if (row.Cells["category"].Value.ToString() != "ACTIVITY")
                    {
                        tmp = "(&" + row.Cells["TransactionID"].Value.ToString() + "&" + row.Cells["NoUrut"].Value.ToString() + ")";
                        tmp2 = row.Cells["ItemName"].Value.ToString();
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

                dsQR = controllerTrans.LoadListQRCodes(listQrCodes, listqrcode, listitemname);
                // Show or Print Tiket 

                reportQRDoc2 = new PrintQRCode();
                reportQRDoc2.SetDataSource(dsQR);

                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show("Data Ticket berhasil Diload !!! \n Lanjut Cetak Ticket ?", "Print Ticket ? ", buttons);
                if (result == DialogResult.Yes)
                {
                    Reports.FrmShowReport formListItem = new Reports.FrmShowReport(reportQRDoc2);
                    formListItem.ShowDialog();
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FrmScanQRCODE frmScan = new FrmScanQRCODE();
            frmScan.ShowDialog();
        }


    }
}
