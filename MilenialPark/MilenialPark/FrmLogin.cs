using System;
using System.Windows.Forms;
using MilenialPark.Views;
using MilenialPark.Master;
using MilenialPark.Models;

namespace MilenialPark
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            try
            {
                ClsStaticVariable.setNewConnection("WHNPOS", txtServer.Text);
                ClsStaticVariable.controllerUser.SetCabang();
                setcbxCategory();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal terhubung ke database.\n\n" + ex.Message,
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void txtpassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            iconButton2_Click(null, EventArgs.Empty);
        }

        private void iconButton2_Click(object sender, EventArgs e)
        {
            string userID = txtUserID.Text.Trim();
            string password = txtpassword.Text;

            if (string.IsNullOrWhiteSpace(userID) ||
                string.IsNullOrWhiteSpace(password))
            {
                ClsFungsi.Pesan(
                    "UserID atau Password Kosong",
                    "INFO"
                );

                return;
            }

            if (cbxCategory.SelectedItem == null ||
                string.IsNullOrWhiteSpace(ClsStaticVariable.KodeBranch))
            {
                ClsFungsi.Pesan(
                    "Cabang belum dipilih",
                    "INFO"
                );

                return;
            }

            try
            {
                ClsStaticVariable.controllerUser.objUser =
                    ClsStaticVariable.controllerUser.getOneUser(
                        userID,
                        password
                    );

                if (ClsStaticVariable.controllerUser.objUser == null)
                {
                    ClsFungsi.Pesan(
                        "UserID atau Password Salah",
                        "INFO"
                    );

                    return;
                }

                string decryptedPassword =
                    new ClsCrypthography().DecryptString(
                        ClsStaticVariable.controllerUser.objUser.Password
                    );

                if (password != decryptedPassword)
                {
                    ClsFungsi.Pesan(
                        "UserID atau Password Salah",
                        "INFO"
                    );

                    return;
                }

                Mainform frmMainForm = new Mainform(this);
                frmMainForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Login Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public void setcbxCategory()
        {
            cbxCategory.Items.Clear();
            cbxCategory.DisplayMember = "Text";
            cbxCategory.ValueMember = "Value";

            int selectedIndex = -1;
            int index = 0;

            foreach (ClsCabang cabang in
                     ClsStaticVariable.controllerUser.listcabang)
            {
                cbxCategory.Items.Add(new
                {
                    Text = cabang.KodeCabang
                           + " - "
                           + cabang.NamaCabang,

                    Value = cabang.KodeCabang
                });

                if (!string.IsNullOrWhiteSpace(
                        ClsStaticVariable.KodeBranch) &&
                    string.Equals(
                        cabang.KodeCabang,
                        ClsStaticVariable.KodeBranch,
                        StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndex = index;
                }

                index++;
            }

            if (selectedIndex >= 0)
            {
                cbxCategory.SelectedIndex = selectedIndex;
            }
            else if (cbxCategory.Items.Count > 0)
            {
                cbxCategory.SelectedIndex = 0;
            }
            else
            {
                ClsStaticVariable.KodeBranch = "";
            }
        }

        private void cbxCategory_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (cbxCategory.SelectedItem == null)
                return;

            ClsStaticVariable.KodeBranch =
                Convert.ToString(
                    (cbxCategory.SelectedItem as dynamic).Value
                );
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
    }
}