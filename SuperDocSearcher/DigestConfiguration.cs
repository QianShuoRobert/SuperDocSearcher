using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperDocSearcher
{
    public partial class DigestConfiguration : Form
    {
        public DigestConfiguration()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            UpLine = (int)this.numericUpDownUpline.Value;
            DownLine = (int)this.numericUpDownDownLine.Value;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        public Point StartLocation
        {
            get;
            set;
        }
        public int UpLine
        {
            get;
            set;
        }
        public int DownLine
        {
            get;
            set;
        }
        private void DigestConfiguration_Load(object sender, EventArgs e)
        {
            this.Location = new Point(StartLocation.X - Width / 2, StartLocation.Y - Height / 2);
            UpLine = UpLine < 0 ? 0 : UpLine;
            DownLine = DownLine < 0 ? 0 : DownLine;
            this.numericUpDownUpline.Value = UpLine;
            this.numericUpDownDownLine.Value = DownLine;
        }
    }
}
