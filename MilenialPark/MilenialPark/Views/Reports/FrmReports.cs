using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MilenialPark.Master;
using MilenialPark.UserControls;
using MilenialPark.Controller;
using MilenialPark.Views;
using MilenialPark.Models;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Windows.Forms;
using MilenialPark.Reports;

namespace MilenialPark.Views.Reports
{
    public partial class FrmReports : Form
    {
        #region properties

        public Mainform parentfrm;
        public ControllerTransaction controllerTran = new ControllerTransaction();
        public ControllerCard controllerCard = new ControllerCard();
        public ControllerShop controllerShop = new ControllerShop();
        public ControllerUser controllerUser = new ControllerUser();
        public DataTable dt = new DataTable();
        bool exist = false;
        bool close = false;
        public BindingSource bind = new BindingSource();
        public decimal finalbalance = 0;

        public ReportDocument reportDoc = new ReportDocument();
        public DataSet ds = new DataSet();
        public ControllerReport controllerReport = new ControllerReport();

        public string TransactionTypeVal = "%%";
        public string PaymentTypeVal = "%%";
        public string UserIDVal = "%%";
        public string RemarksVal = "%%";

        #endregion
        public FrmReports()
        {
            InitializeComponent();
        }

        public FrmReports(Mainform main)
        {
            InitializeComponent();
            this.parentfrm = main;
            //dtpFrom.Value = DateTime.Now.AddMonths(-3);
            //dtpTo.Value = DateTime.Now;
            //txtShopID.Text = parentfrm.lblShopID.Text;

        }

        public void setcbxUser()
        {
            cbxUserID.Items.Clear();
            cbxUserID.Items.Add("ALL");

            dt = controllerUser.getListUser();
            cbxUserID.DisplayMember = "Text";
            cbxUserID.ValueMember = "Value";
            int selected = 0;
            if (dt.Rows.Count != 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    cbxUserID.Items.Add(new { Text = dt.Rows[i]["UserName"].ToString(), Value = dt.Rows[i]["UserID"].ToString() });
                }
            }
            cbxUserID.SelectedIndex = selected;


        }

        private void FrmReports_Load(object sender, EventArgs e)
        {
            setcbxUser();
            cbxTransType.SelectedIndex = 0;
            dtpFrom.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            dtpTo.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            cbxReportType.SelectedIndex = 0;

            SetupRoundingMode();
            SetupAdjustmentAccess();
        }

        private void SetupAdjustmentAccess()
        {
            string tipeUser =
                ClsStaticVariable.controllerUser.objUser?.TipeUser ?? "";

            bool isAdmin = tipeUser.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase
            );

            if (isAdmin)
            {
                // Admin bebas memilih Actual / Adjusted.
                chkAdjustedReport.Checked = false;
                chkAdjustedReport.Enabled = true;
            }
            else
            {
                // User biasa:
                // 1 hari = Actual
                // > 1 hari = Adjusted
                ApplyRegularUserAdjustmentRule();

                chkAdjustedReport.Enabled = false;
            }

            UpdateAdjustmentControls();
        }

        private void ApplyRegularUserAdjustmentRule()
        {
            string tipeUser =
                ClsStaticVariable.controllerUser.objUser?.TipeUser ?? "";

            bool isAdmin = tipeUser.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase
            );

            if (isAdmin)
                return;

            DateTime fromDate = dtpFrom.Value.Date;
            DateTime toDate = dtpTo.Value.Date;
            DateTime today = DateTime.Today;

            bool todayOnly =
                fromDate == today &&
                toDate == today;

            chkAdjustedReport.Checked =
                !todayOnly;
        }

        private void SetupRoundingMode()
        {
            cbxRoundingMode.Items.Clear();

            cbxRoundingMode.Items.Add("No Rounding");
            cbxRoundingMode.Items.Add("Nearest");
            cbxRoundingMode.Items.Add("Round Down");
            cbxRoundingMode.Items.Add("Round Up");

            cbxRoundingMode.SelectedIndex = 1;
        }

        private ReportAdjustmentOptions GetAdjustmentOptions()
        {
            ReportAdjustmentOptions options =
                new ReportAdjustmentOptions();

            options.Enabled =
                chkAdjustedReport.Checked;

            options.Percentage =
                nudAdjustmentPercentage.Value;

            switch (cbxRoundingMode.SelectedIndex)
            {
                case 0:
                    options.RoundingMode =
                        ReportRoundingMode.None;
                    break;

                case 1:
                    options.RoundingMode =
                        ReportRoundingMode.Nearest;
                    break;

                case 2:
                    options.RoundingMode =
                        ReportRoundingMode.Down;
                    break;

                case 3:
                    options.RoundingMode =
                        ReportRoundingMode.Up;
                    break;

                default:
                    options.RoundingMode =
                        ReportRoundingMode.Nearest;
                    break;
            }

            return options;
        }


        public void setParameter()
        {
            if (cbxTransType.SelectedIndex == 0)
            {
                TransactionTypeVal = "%%";
            }
            else
            {
                TransactionTypeVal = cbxTransType.Text;
            }

            if (cbxPaymentType.SelectedIndex == 0)
            {
                PaymentTypeVal = "%%";
            }
            else
            {
                PaymentTypeVal = cbxPaymentType.Text;
            }

            if (cbxUserID.SelectedIndex == 0)
            {
                UserIDVal = "%%";
            }
            else
            {
                UserIDVal = Convert.ToString((cbxUserID.SelectedItem as dynamic).Value);
            }

            if (txtRemarks.Text.Trim().Length == 0)
            {
                RemarksVal = "%%";
            }
            else
            {
                RemarksVal = "%" + txtRemarks.Text + "%";
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime from =
                    dtpFrom.Value.Date;

                DateTime to =
                    dtpTo.Value.Date
                    .AddDays(1)
                    .AddSeconds(-1);

                setParameter();

                ReportAdjustmentOptions adjustmentOptions =
                    GetAdjustmentOptions();

                LoadSelectedReport(
                    from,
                    to
                );

                controllerReport.ApplyReportAdjustment(
                    ds,
                    adjustmentOptions
                );

                reportDoc.SetDataSource(ds);

                reportDoc.SetParameterValue(
                    "StartDate",
                    from
                );

                reportDoc.SetParameterValue(
                    "EndDate",
                    to
                );

                reportDoc.SetParameterValue(
                    "Title",
                    txtReportTitle.Text
                );

                crViewer.ReportSource =
                    reportDoc;

                crViewer.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Report Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LoadSelectedReport(
    DateTime from,
    DateTime to)
        {
            switch (cbxReportType.SelectedIndex)
            {
                case 0:

                    ds = controllerReport.LoadPendapatan(
                        from,
                        to,
                        TransactionTypeVal,
                        PaymentTypeVal,
                        UserIDVal,
                        RemarksVal
                    );

                    reportDoc =
                        new MilenialPark.Reports
                        .PrintLaporanDetailPenjualan();

                    break;


                case 1:

                    ds = controllerReport.LoadPendapatanGroup(
                        from,
                        to,
                        TransactionTypeVal,
                        PaymentTypeVal,
                        UserIDVal,
                        RemarksVal
                    );

                    reportDoc =
                        new MilenialPark.Reports
                        .LaporanPenjualan();

                    break;


                case 2:

                    ds = controllerReport.LoadPenjualan(
                        from,
                        to,
                        TransactionTypeVal,
                        PaymentTypeVal,
                        UserIDVal,
                        RemarksVal
                    );

                    reportDoc =
                        new MilenialPark.Reports
                        .PrintLaporanDetailPenjualan();

                    break;


                case 3:

                    ds = controllerReport.LoadPenjualanGroup(
                        from,
                        to,
                        TransactionTypeVal,
                        PaymentTypeVal,
                        UserIDVal,
                        RemarksVal
                    );

                    reportDoc =
                        new MilenialPark.Reports
                        .LaporanPenjualan();

                    break;


                case 4:

                    ds = controllerReport.LoadPendapatanSummary(
                        from,
                        to,
                        TransactionTypeVal,
                        PaymentTypeVal,
                        UserIDVal,
                        RemarksVal
                    );

                    reportDoc =
                        new MilenialPark.Reports
                        .LaporanSummaryPenjualan();

                    break;


                case 5:

                    ds = controllerReport.LoadPenjualanSummary(
                        from,
                        to,
                        TransactionTypeVal,
                        PaymentTypeVal,
                        UserIDVal,
                        RemarksVal
                    );

                    reportDoc =
                        new MilenialPark.Reports
                        .LaporanSummaryPenjualan();

                    break;


                case 6:

                    ds = controllerReport.LoadPendapatanSummary(
                        from,
                        to,
                        TransactionTypeVal,
                        PaymentTypeVal,
                        UserIDVal,
                        RemarksVal
                    );

                    reportDoc =
                        new MilenialPark.Reports
                        .PrintShiftSummary();

                    break;


                case 7:

                    ds = controllerReport.LoadPenjualanSummary(
                        from,
                        to,
                        TransactionTypeVal,
                        PaymentTypeVal,
                        UserIDVal,
                        RemarksVal
                    );

                    reportDoc =
                        new MilenialPark.Reports
                        .PrintShiftSummary();

                    break;


                default:

                    throw new InvalidOperationException(
                        "Please select a report type."
                    );
            }
        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void cbxReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbxReportType.Text.Contains("Penjualan"))    
            {
                cbxPaymentType.Items.Clear();
                cbxPaymentType.Items.Add("ALL");
                cbxPaymentType.Items.Add("CASH");
                cbxPaymentType.Items.Add("DEBIT");
                cbxPaymentType.Items.Add("CARD");
                cbxPaymentType.Items.Add("MASTER_CARD");
                cbxPaymentType.SelectedIndex = 0;
            }
            else if(cbxReportType.Text.Contains("Pendapatan"))
            {
                cbxPaymentType.Items.Clear();
                cbxPaymentType.Items.Add("ALL");
                cbxPaymentType.Items.Add("CASH");
                cbxPaymentType.Items.Add("DEBIT");
                cbxPaymentType.Items.Add("MASTER_CARD");
                cbxPaymentType.SelectedIndex = 0;
            }

            txtReportTitle.Text = cbxReportType.Text;
        }

        private void chkAdjustedReport_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAdjustmentControls();
        }

        private void UpdateAdjustmentControls()
        {
            string tipeUser =
                ClsStaticVariable.controllerUser.objUser?.TipeUser ?? "";

            bool isAdmin = tipeUser.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase
            );

            bool adjustmentEnabled = chkAdjustedReport.Checked;

            // Hanya admin yang boleh mengubah setting.
            nudAdjustmentPercentage.Enabled =
                isAdmin && adjustmentEnabled;

            cbxRoundingMode.Enabled =
                isAdmin && adjustmentEnabled;
        }

        private void dtpTo_ValueChanged(object sender, EventArgs e)
        {
            ApplyRegularUserAdjustmentRule();
        }

        private void dtpFrom_ValueChanged(object sender, EventArgs e)
        {
            ApplyRegularUserAdjustmentRule();
        }
    }
}
