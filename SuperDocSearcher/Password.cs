using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperDocSearcher
{
    public partial class Password : Form
    {
        public Password()
        {
            InitializeComponent();
            Passwd = string.Empty;
            NewPasswd = string.Empty;
            DoDetecting = false;
            FirstTimeRun = false;
            ForgotPasswd = false;
        }

        public string Passwd { get; set; }
        public string NewPasswd { get; set; }
        public bool FirstTimeRun { get; set; }
        public bool DoDetecting
        {
            get
            {
                return this.checkBoxDoDetecting.Checked;
            }
            set
            {
                this.checkBoxDoDetecting.Checked = value;
            }
        }
        public bool ForgotPasswd { get; set; }

        private void Password_Load(object sender, EventArgs e)
        {
            if (this.FirstTimeRun)
            {
                this.radioButtonChangePasswd.Checked = true;
                this.radioButtonInputPasswd.Enabled = false;
                this.radioButtonChangePasswd.Enabled = false;
                this.textBoxPassword.Enabled = false;
                this.Text = "请设置访问密码";
            }
        }
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (!FirstTimeRun)
            {
                if (string.IsNullOrEmpty(this.textBoxPassword.Text))
                {
                    MessageBox.Show("密码不能为空！", "输入密码");
                    return;
                }
            }
            if (this.radioButtonChangePasswd.Checked)
            {
                if (string.IsNullOrEmpty(this.textBoxNewPassword.Text)
                    || string.IsNullOrEmpty(this.textBoxConfirm.Text)
                    || this.textBoxNewPassword.Text.Length < 6)
                {
                    MessageBox.Show("密码不能少于6位！", "修改密码");
                    return;
                }
                if (this.textBoxNewPassword.Text != this.textBoxConfirm.Text)
                {
                    MessageBox.Show("两次输入的密码不一致！", "修改密码");
                    return;
                }
                if (this.textBoxPassword.Text == this.textBoxNewPassword.Text)
                {
                    MessageBox.Show("新密码与原密码相同！", "修改密码");
                    return;
                }
            }

            this.Passwd = this.textBoxPassword.Text;
            this.NewPasswd = this.textBoxConfirm.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
        private void radioButtonChangePasswd_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButtonChangePasswd.Checked)
            {
                this.textBoxNewPassword.Enabled = true;
                this.textBoxConfirm.Enabled = true;
            }
            else
            {
                this.textBoxNewPassword.Enabled = false;
                this.textBoxConfirm.Enabled = false;
            }
        }

        private void Password_Shown(object sender, EventArgs e)
        {
            if (FirstTimeRun)
            {
                this.textBoxNewPassword.Focus();
            }
            else
            {
                this.textBoxPassword.Focus();
            }

        }

        private void textBoxPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                this.buttonOK_Click(null, null);
            }
        }

        private void textBoxNewPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                this.buttonOK_Click(null, null);
            }
        }

        private void textBoxConfirm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                this.buttonOK_Click(null, null);
            }
        }

        private void linkLabelForgetPasswd_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.FirstTimeRun)
            {
                MessageBox.Show("据我所知，你还没有设置密码呢！", "你点错了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("确认将清除已同步文档的缓存，请谨慎操作！", "请确认清除操作", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
            {
                ForgotPasswd = true;
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.Close();
            }
        }
    }
}
