namespace SuperDocSearcher
{
    partial class DigestConfiguration
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownUpline = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownDownLine = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownUpline)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDownLine)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.BackColor = System.Drawing.Color.White;
            this.buttonOK.Location = new System.Drawing.Point(118, 92);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "确定";
            this.buttonOK.UseVisualStyleBackColor = false;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "显示关键字向上";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(176, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "行";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(176, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "行";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "显示关键字向下";
            // 
            // numericUpDownUpline
            // 
            this.numericUpDownUpline.Location = new System.Drawing.Point(108, 17);
            this.numericUpDownUpline.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownUpline.Name = "numericUpDownUpline";
            this.numericUpDownUpline.Size = new System.Drawing.Size(62, 21);
            this.numericUpDownUpline.TabIndex = 7;
            this.numericUpDownUpline.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // numericUpDownDownLine
            // 
            this.numericUpDownDownLine.Location = new System.Drawing.Point(108, 51);
            this.numericUpDownDownLine.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownDownLine.Name = "numericUpDownDownLine";
            this.numericUpDownDownLine.Size = new System.Drawing.Size(62, 21);
            this.numericUpDownDownLine.TabIndex = 8;
            this.numericUpDownDownLine.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // DigestConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(203, 127);
            this.ControlBox = false;
            this.Controls.Add(this.numericUpDownDownLine);
            this.Controls.Add(this.numericUpDownUpline);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DigestConfiguration";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "摘要显示";
            this.Load += new System.EventHandler(this.DigestConfiguration_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownUpline)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDownLine)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownUpline;
        private System.Windows.Forms.NumericUpDown numericUpDownDownLine;
    }
}