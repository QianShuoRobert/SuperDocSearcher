namespace SuperDocSearcher
{
    partial class Password
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Password));
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.radioButtonInputPasswd = new System.Windows.Forms.RadioButton();
            this.radioButtonChangePasswd = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxNewPassword = new System.Windows.Forms.TextBox();
            this.textBoxConfirm = new System.Windows.Forms.TextBox();
            this.checkBoxDoDetecting = new System.Windows.Forms.CheckBox();
            this.linkLabelForgetPasswd = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(87, 59);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(179, 21);
            this.textBoxPassword.TabIndex = 2;
            this.textBoxPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxPassword_KeyPress);
            // 
            // buttonOK
            // 
            this.buttonOK.BackColor = System.Drawing.Color.White;
            this.buttonOK.Location = new System.Drawing.Point(191, 197);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "确定";
            this.buttonOK.UseVisualStyleBackColor = false;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // radioButtonInputPasswd
            // 
            this.radioButtonInputPasswd.AutoSize = true;
            this.radioButtonInputPasswd.Checked = true;
            this.radioButtonInputPasswd.Location = new System.Drawing.Point(35, 22);
            this.radioButtonInputPasswd.Name = "radioButtonInputPasswd";
            this.radioButtonInputPasswd.Size = new System.Drawing.Size(71, 16);
            this.radioButtonInputPasswd.TabIndex = 0;
            this.radioButtonInputPasswd.TabStop = true;
            this.radioButtonInputPasswd.Text = "输入密码";
            this.radioButtonInputPasswd.UseVisualStyleBackColor = true;
            // 
            // radioButtonChangePasswd
            // 
            this.radioButtonChangePasswd.AutoSize = true;
            this.radioButtonChangePasswd.Location = new System.Drawing.Point(160, 22);
            this.radioButtonChangePasswd.Name = "radioButtonChangePasswd";
            this.radioButtonChangePasswd.Size = new System.Drawing.Size(71, 16);
            this.radioButtonChangePasswd.TabIndex = 1;
            this.radioButtonChangePasswd.Text = "修改密码";
            this.radioButtonChangePasswd.UseVisualStyleBackColor = true;
            this.radioButtonChangePasswd.CheckedChanged += new System.EventHandler(this.radioButtonChangePasswd_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(48, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "密码";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "新密码";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 152);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "再次输入";
            // 
            // textBoxNewPassword
            // 
            this.textBoxNewPassword.Enabled = false;
            this.textBoxNewPassword.Location = new System.Drawing.Point(87, 103);
            this.textBoxNewPassword.Name = "textBoxNewPassword";
            this.textBoxNewPassword.PasswordChar = '*';
            this.textBoxNewPassword.Size = new System.Drawing.Size(179, 21);
            this.textBoxNewPassword.TabIndex = 3;
            this.textBoxNewPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxNewPassword_KeyPress);
            // 
            // textBoxConfirm
            // 
            this.textBoxConfirm.Enabled = false;
            this.textBoxConfirm.Location = new System.Drawing.Point(87, 145);
            this.textBoxConfirm.Name = "textBoxConfirm";
            this.textBoxConfirm.PasswordChar = '*';
            this.textBoxConfirm.Size = new System.Drawing.Size(179, 21);
            this.textBoxConfirm.TabIndex = 4;
            this.textBoxConfirm.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxConfirm_KeyPress);
            // 
            // checkBoxDoDetecting
            // 
            this.checkBoxDoDetecting.AutoSize = true;
            this.checkBoxDoDetecting.Checked = true;
            this.checkBoxDoDetecting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDoDetecting.Location = new System.Drawing.Point(101, 201);
            this.checkBoxDoDetecting.Name = "checkBoxDoDetecting";
            this.checkBoxDoDetecting.Size = new System.Drawing.Size(84, 16);
            this.checkBoxDoDetecting.TabIndex = 7;
            this.checkBoxDoDetecting.Text = "认证后检测";
            this.checkBoxDoDetecting.UseVisualStyleBackColor = true;
            // 
            // linkLabelForgetPasswd
            // 
            this.linkLabelForgetPasswd.AutoSize = true;
            this.linkLabelForgetPasswd.Location = new System.Drawing.Point(24, 202);
            this.linkLabelForgetPasswd.Name = "linkLabelForgetPasswd";
            this.linkLabelForgetPasswd.Size = new System.Drawing.Size(53, 12);
            this.linkLabelForgetPasswd.TabIndex = 8;
            this.linkLabelForgetPasswd.TabStop = true;
            this.linkLabelForgetPasswd.Text = "忘记密码";
            this.linkLabelForgetPasswd.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelForgetPasswd_LinkClicked);
            // 
            // Password
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(290, 243);
            this.Controls.Add(this.linkLabelForgetPasswd);
            this.Controls.Add(this.checkBoxDoDetecting);
            this.Controls.Add(this.textBoxConfirm);
            this.Controls.Add(this.textBoxNewPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.radioButtonChangePasswd);
            this.Controls.Add(this.radioButtonInputPasswd);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxPassword);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.AlphaFull;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Password";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "请输入访问密码";
            this.Load += new System.EventHandler(this.Password_Load);
            this.Shown += new System.EventHandler(this.Password_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.RadioButton radioButtonInputPasswd;
        private System.Windows.Forms.RadioButton radioButtonChangePasswd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxNewPassword;
        private System.Windows.Forms.TextBox textBoxConfirm;
        private System.Windows.Forms.CheckBox checkBoxDoDetecting;
        private System.Windows.Forms.LinkLabel linkLabelForgetPasswd;
    }
}