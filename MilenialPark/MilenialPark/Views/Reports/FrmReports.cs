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
                ClsStaticVariable.controllerUser.objUser.TipeUser ?? "";

            bool isAdmin = tipeUser.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase
            );

            if (isAdmin)
            {
                // Admin melihat actual/default normal report.
                chkAdjustedReport.Checked = false;
                chkAdjustedReport.Enabled = true;

                nudAdjustmentPercentage.Enabled = false;
                cbxRoundingMode.Enabled = false;
            }
            else
            {
                // Untuk internal management view.
                chkAdjustedReport.Checked = true;
                chkAdjustedReport.Enabled = false;

                nudAdjustmentPercentage.Enabled = false;
                cbxRoundingMode.Enabled = false;
            }

            UpdateAdjustmentControls();
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

        private decimal ApplyRounding(
    decimal value,
    ReportRoundingMode roundingMode)
        {
            switch (roundingMode)
            {
                case ReportRoundingMode.Down:
                    return Math.Floor(value);

                case ReportRoundingMode.Up:
                    return Math.Ceiling(value);

                case ReportRoundingMode.Nearest:
                    return Math.Round(
                        value,
                        0,
                        MidpointRounding.AwayFromZero
                    );

                default:
                    return value;
            }
        }

        private decimal CalculateAdjustedValue(
    decimal originalValue,
    decimal percentage,
    ReportRoundingMode roundingMode)
        {
            decimal multiplier =
                1M - (percentage / 100M);

            decimal result =
                originalValue * multiplier;

            return ApplyRounding(
                result,
                roundingMode
            );
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
            DateTime from = dtpFrom.Value;
            DateTime to = dtpTo.Value;

            setParameter();

            if (cbxReportType.SelectedIndex == 0)
            {
                ds = controllerReport.LoadPendapatan(dtpFrom.Value, dtpTo.Value, TransactionTypeVal, PaymentTypeVal, UserIDVal, RemarksVal);

                reportDoc = new MilenialPark.Reports.PrintLaporanDetailPenjualan();
                reportDoc.SetDataSource(ds);
                reportDoc.SetParameterValue("StartDate", new DateTime(from.Year, from.Month, from.Day, 0, 0, 0));
                reportDoc.SetParameterValue("EndDate", new DateTime(to.Year, to.Month, to.Day, 23, 59, 59));
                reportDoc.SetParameterValue("Title", txtReportTitle.Text);

                crViewer.ReportSource = reportDoc;
            }
            else if (cbxReportType.SelectedIndex == 1)
            {
                ds = controllerReport.LoadPendapatanGroup(dtpFrom.Value, dtpTo.Value, TransactionTypeVal, PaymentTypeVal, UserIDVal, RemarksVal);

                reportDoc = new MilenialPark.Reports.LaporanPenjualan();
                reportDoc.SetDataSource(ds);
                reportDoc.SetParameterValue("StartDate", new DateTime(from.Year, from.Month, from.Day, 0, 0, 0));
                reportDoc.SetParameterValue("EndDate", new DateTime(to.Year, to.Month, to.Day, 23, 59, 59));
                reportDoc.SetParameterValue("Title", txtReportTitle.Text);

                crViewer.ReportSource = reportDoc;
            }
            else if(cbxReportType.SelectedIndex == 2)
            {
                ds = controllerReport.LoadPenjualan(dtpFrom.Value, dtpTo.Value, TransactionTypeVal, PaymentTypeVal, UserIDVal, RemarksVal);

                reportDoc = new MilenialPark.Reports.PrintLaporanDetailPenjualan();
                reportDoc.SetDataSource(ds);
                reportDoc.SetParameterValue("StartDate", new DateTime(from.Year, from.Month, from.Day, 0, 0, 0));
                reportDoc.SetParameterValue("EndDate", new DateTime(to.Year, to.Month, to.Day, 23, 59, 59));
                reportDoc.SetParameterValue("Title", txtReportTitle.Text);

                crViewer.ReportSource = reportDoc;
            }

            else if (cbxReportType.SelectedIndex == 3)
            {
                ds = controllerReport.LoadPenjualanGroup(dtpFrom.Value, dtpTo.Value, TransactionTypeVal, PaymentTypeVal, UserIDVal, RemarksVal);

                reportDoc = new MilenialPark.Reports.LaporanPenjualan();
                reportDoc.SetDataSource(ds);
                reportDoc.SetParameterValue("StartDate", new DateTime(from.Year, from.Month, from.Day, 0, 0, 0));
                reportDoc.SetParameterValue("EndDate", new DateTime(to.Year, to.Month, to.Day, 23, 59, 59));
                reportDoc.SetParameterValue("Title", txtReportTitle.Text);

                crViewer.ReportSource = reportDoc;
            }

            else if (cbxReportType.SelectedIndex == 4)
            {
                ds = controllerReport.LoadPendapatanSummary(dtpFrom.Value, dtpTo.Value, TransactionTypeVal, PaymentTypeVal, UserIDVal, RemarksVal);

                reportDoc = new MilenialPark.Reports.LaporanSummaryPenjualan();
                reportDoc.SetDataSource(ds);
                reportDoc.SetParameterValue("StartDate", new DateTime(from.Year, from.Month, from.Day, 0, 0, 0));
                reportDoc.SetParameterValue("EndDate", new DateTime(to.Year, to.Month, to.Day, 23, 59, 59));
                reportDoc.SetParameterValue("Title", txtReportTitle.Text);

                crViewer.ReportSource = reportDoc;
            }
            else if (cbxReportType.SelectedIndex == 5)
            {
                ds = controllerReport.LoadPenjualanSummary(dtpFrom.Value, dtpTo.Value, TransactionTypeVal, PaymentTypeVal, UserIDVal, RemarksVal);

                reportDoc = new MilenialPark.Reports.LaporanSummaryPenjualan();
                reportDoc.SetDataSource(ds);
                reportDoc.SetParameterValue("StartDate", new DateTime(from.Year, from.Month, from.Day, 0, 0, 0));
                reportDoc.SetParameterValue("EndDate", new DateTime(to.Year, to.Month, to.Day, 23, 59, 59));
                reportDoc.SetParameterValue("Title", txtReportTitle.Text);

                crViewer.ReportSource = reportDoc;
            }

            else if (cbxReportType.SelectedIndex == 6)
            {
                ds = controllerReport.LoadPendapatanSummary(dtpFrom.Value, dtpTo.Value, TransactionTypeVal, PaymentTypeVal, UserIDVal, RemarksVal);

                reportDoc = new MilenialPark.Reports.PrintShiftSummary();
                reportDoc.SetDataSource(ds);
                reportDoc.SetParameterValue("StartDate", new DateTime(from.Year, from.Month, from.Day, 0, 0, 0));
                reportDoc.SetParameterValue("EndDate", new DateTime(to.Year, to.Month, to.Day, 23, 59, 59));
                reportDoc.SetParameterValue("Title", txtReportTitle.Text);

                crViewer.ReportSource = reportDoc;
            }

            else if (cbxReportType.SelectedIndex == 7)
            {
                ds = controllerReport.LoadPenjualanSummary(dtpFrom.Value, dtpTo.Value, TransactionTypeVal, PaymentTypeVal, UserIDVal, RemarksVal);

                reportDoc = new MilenialPark.Reports.PrintShiftSummary();
                reportDoc.SetDataSource(ds);
                reportDoc.SetParameterValue("StartDate", new DateTime(from.Year, from.Month, from.Day, 0, 0, 0));
                reportDoc.SetParameterValue("EndDate", new DateTime(to.Year, to.Month, to.Day, 23, 59, 59));
                reportDoc.SetParameterValue("Title", txtReportTitle.Text);

                crViewer.ReportSource = reportDoc;
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
            bool isAdmin =
                ClsStaticVariable.controllerUser.objUser.TipeUser
                .Equals(
                    "Admin",
                    StringComparison.OrdinalIgnoreCase
                );

            bool enabled = chkAdjustedReport.Checked;

            nudAdjustmentPercentage.Enabled =
                isAdmin && enabled;

            cbxRoundingMode.Enabled =
                isAdmin && enabled;
        }
    }
}
