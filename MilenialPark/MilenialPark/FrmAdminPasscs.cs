using MilenialPark.Controller;
using MilenialPark.Master;
using System;
using System.Windows.Forms;

namespace MilenialPark
{
    public partial class FrmAdminPass : Form
    {
        public bool IsVerified { get; private set; } = false;
        public string VerifiedUserId { get; private set; } = "";

        public string VoidReason
        {
            get
            {
                return (txtRemarks.Text ?? "").Trim();
            }
        }

        private bool _isVoidMode = false;
        private string _transactionID = "";

        public ControllerUser controllerUser = new ControllerUser();

        // ==============================
        // CONSTRUCTOR LAMA
        // Dipakai Gate
        // Jangan diubah behaviour-nya
        // ==============================
        public FrmAdminPass()
        {
            InitializeComponent();

            _isVoidMode = false;
        }

        // ==============================
        // CONSTRUCTOR KHUSUS VOID
        // ==============================
        public FrmAdminPass(string transactionID)
        {
            InitializeComponent();

            _isVoidMode = true;
            _transactionID = transactionID;

            this.Text = "VOID TRANSACTION - " + transactionID;
            btnVerify.Text = "VERIFY & VOID";
        }

        private void FrmAdminPass_Load(object sender, EventArgs e)
        {
            txtUserID.Focus();
            txtUserID.SelectAll();
        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            string uid = (txtUserID.Text ?? "").Trim();
            string pwd = (txtpassword.Text ?? "").Trim();
            string remarks = (txtRemarks.Text ?? "").Trim();

            // ==============================
            // VALIDASI USER/PASSWORD
            // berlaku untuk Gate & Void
            // ==============================
            if (uid == "" || pwd == "")
            {
                MessageBox.Show(
                    "UserID/Password wajib diisi.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // ==============================
            // REMARKS HANYA WAJIB SAAT VOID
            // ==============================
            if (_isVoidMode && remarks == "")
            {
                MessageBox.Show(
                    "Alasan VOID wajib diisi.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtRemarks.Focus();
                return;
            }

            // ==============================
            // VERIFIKASI ADMIN
            // Logic lama tetap
            // ==============================
            bool ok = false;

            controllerUser.objUser =
                controllerUser.getOneUser(uid, pwd);

            if (controllerUser.objUser != null)
            {
                if (controllerUser.objUser.TipeUser == "Admin")
                {
                    ok = true;
                }
            }

            if (!ok)
            {
                MessageBox.Show(
                    "Akses ditolak. User bukan admin / password salah.",
                    "Akses Ditolak",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            IsVerified = true;
            VerifiedUserId = uid;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void txtpassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnVerify_Click(sender, e);
            }
        }
    }
}