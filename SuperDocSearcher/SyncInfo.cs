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
    public partial class SyncInfo : Form
    {
        public SyncInfo()
        {
            InitializeComponent();
        }
        public List<SuperDocSearcher.CFileInfo> AddList { get; set; }
        public List<SuperDocSearcher.CFileInfo> ModList { get; set; }
        public List<SuperDocSearcher.CFileInfo> RmList { get; set; }
        public List<SuperDocSearcher.CFileInfo> ComList { get; set; }
        public List<SuperDocSearcher.CFileInfo> EmptyList { get; set; }
        private void SyncInfo_Load(object sender, EventArgs e)
        {
            if (this.AddList != null)
            {
                int index = 0;
                foreach (var fileInfo in AddList)
                {
                    ListViewItem item = new ListViewItem();
                    item.SubItems[0].Text = (++index).ToString();
                    item.SubItems.Add(fileInfo.FileName);
                    item.SubItems.Add(fileInfo.FullPath);
                    item.SubItems.Add(SuperDocSearcher.SizeConverter(fileInfo.FileSize));
                    item.SubItems.Add(fileInfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
                    item.SubItems.Add(Path.GetExtension(fileInfo.FullPath).Replace(".", ""));
                    this.listViewAdd.Items.Add(item);
                }
            }
            if (this.ModList != null)
            {
                int index = 0;
                foreach (var fileInfo in ModList)
                {
                    ListViewItem item = new ListViewItem();
                    item.SubItems[0].Text = (++index).ToString();
                    item.SubItems.Add(fileInfo.FileName);
                    item.SubItems.Add(fileInfo.FullPath);
                    item.SubItems.Add(SuperDocSearcher.SizeConverter(fileInfo.FileSize));
                    item.SubItems.Add(fileInfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
                    item.SubItems.Add(Path.GetExtension(fileInfo.FullPath).Replace(".", ""));
                    this.listViewMod.Items.Add(item);
                }
            }
            if (this.RmList != null)
            {
                int index = 0;
                foreach (var fileInfo in RmList)
                {
                    ListViewItem item = new ListViewItem();
                    item.SubItems[0].Text = (++index).ToString();
                    item.SubItems.Add(fileInfo.FileName);
                    item.SubItems.Add(fileInfo.FullPath);
                    item.SubItems.Add(SuperDocSearcher.SizeConverter(fileInfo.FileSize));
                    item.SubItems.Add(fileInfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
                    item.SubItems.Add(Path.GetExtension(fileInfo.FullPath).Replace(".", ""));
                    this.listViewRm.Items.Add(item);
                }
            }
            if (this.ComList != null)
            {
                int index = 0;
                foreach (var fileInfo in ComList)
                {
                    ListViewItem item = new ListViewItem();
                    item.SubItems[0].Text = (++index).ToString();
                    item.SubItems.Add(fileInfo.FileName);
                    item.SubItems.Add(fileInfo.FullPath);
                    item.SubItems.Add(SuperDocSearcher.SizeConverter(fileInfo.FileSize));
                    item.SubItems.Add(fileInfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
                    item.SubItems.Add(Path.GetExtension(fileInfo.FullPath).Replace(".", ""));
                    this.listViewCom.Items.Add(item);
                }
            }
            if (this.EmptyList != null)
            {
                int index = 0;
                foreach (var fileInfo in EmptyList)
                {
                    ListViewItem item = new ListViewItem();
                    item.SubItems[0].Text = (++index).ToString();
                    item.SubItems.Add(fileInfo.FileName);
                    item.SubItems.Add(fileInfo.FullPath);
                    item.SubItems.Add(SuperDocSearcher.SizeConverter(fileInfo.FileSize));
                    item.SubItems.Add(fileInfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
                    item.SubItems.Add(Path.GetExtension(fileInfo.FullPath).Replace(".", ""));
                    this.listViewEmpty.Items.Add(item);
                }
            }
            this.listViewAdd.ListViewItemSorter = new CListViewColumnSorter();
            this.listViewMod.ListViewItemSorter = new CListViewColumnSorter();
            this.listViewCom.ListViewItemSorter = new CListViewColumnSorter();
            this.listViewRm.ListViewItemSorter = new CListViewColumnSorter();
            this.listViewEmpty.ListViewItemSorter = new CListViewColumnSorter();

        }
        private void listViewAdd_DoubleClick(object sender, EventArgs e)
        {
            if (this.listViewAdd.SelectedItems.Count == 0)
            {
                return;
            }
            string strFileName = this.listViewAdd.SelectedItems[0].SubItems[2].Text;
            SuperDocSearcher.OpenFile(strFileName);
        }
        private void listViewMod_DoubleClick(object sender, EventArgs e)
        {
            if (this.listViewMod.SelectedItems.Count == 0)
            {
                return;
            }
            string strFileName = this.listViewMod.SelectedItems[0].SubItems[2].Text;
            SuperDocSearcher.OpenFile(strFileName);
        }
        private void listViewCom_DoubleClick(object sender, EventArgs e)
        {
            if (this.listViewCom.SelectedItems.Count == 0)
            {
                return;
            }
            string strFileName = this.listViewCom.SelectedItems[0].SubItems[2].Text;
            SuperDocSearcher.OpenFile(strFileName);
        }
        private void listViewEmpty_DoubleClick(object sender, EventArgs e)
        {
            if (this.listViewEmpty.SelectedItems.Count == 0)
            {
                return;
            }
            string strFileName = this.listViewEmpty.SelectedItems[0].SubItems[2].Text;
            SuperDocSearcher.OpenFile(strFileName);
        }
        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            ContextMenuStrip ms = (ContextMenuStrip)mi.GetCurrentParent();
            ListView curListView = (ListView)ms.SourceControl;

            if (curListView.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (ListViewItem item in curListView.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                SuperDocSearcher.OpenFile(strFileName);
            }
        }
        private void OpenDirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            ContextMenuStrip ms = (ContextMenuStrip)mi.GetCurrentParent();
            ListView curListView = (ListView)ms.SourceControl;

            if (curListView.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (ListViewItem item in curListView.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                string strPath = Path.GetDirectoryName(strFileName);
                if (Directory.Exists(strPath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start("explorer", "/select,\"" + strFileName + "\"");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\n" + strPath, "打开目录失败");
                    }
                }
            }

        }
        private void CopyPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            ContextMenuStrip ms = (ContextMenuStrip)mi.GetCurrentParent();
            ListView curListView = (ListView)ms.SourceControl;

            if (curListView.SelectedItems.Count == 0)
            {
                return;
            }
            string strPath = string.Empty;
            foreach (ListViewItem item in curListView.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                string strFilePath = Path.GetDirectoryName(strFileName);
                strPath += strFilePath + "\n";
            }
            strPath = strPath.Remove(strPath.Length - 1);
            Clipboard.SetDataObject(strPath);
        }
        private void CopyFullPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            ContextMenuStrip ms = (ContextMenuStrip)mi.GetCurrentParent();
            ListView curListView = (ListView)ms.SourceControl;

            if (curListView.SelectedItems.Count == 0)
            {
                return;
            }
            string strPath = string.Empty;
            foreach (ListViewItem item in curListView.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                strPath += strFileName + "\n";
            }
            strPath = strPath.Remove(strPath.Length - 1);
            Clipboard.SetDataObject(strPath);
        }

        public event Func<string, bool> ClearEmpty;
        private void buttonClear_Click(object sender, EventArgs e)
        {
            if (this.ClearEmpty != null)
            {
                if (this.ClearEmpty(string.Empty))
                {
                    this.listViewEmpty.Items.Clear();
                }
            }
        }
        private void buttonClearSelected_Click(object sender, EventArgs e)
        {
            if (this.listViewEmpty.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (ListViewItem item in this.listViewEmpty.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                if (this.ClearEmpty != null)
                {
                    if (this.ClearEmpty(strFileName))
                    {
                        this.listViewEmpty.Items.Remove(listViewEmpty.SelectedItems[0]);
                    }
                }
            }
        }
        private void CopyFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            ContextMenuStrip ms = (ContextMenuStrip)mi.GetCurrentParent();
            ListView curListView = (ListView)ms.SourceControl;

            if (curListView.SelectedItems.Count == 0)
            {
                return;
            }
            System.Collections.Specialized.StringCollection strcoll = new System.Collections.Specialized.StringCollection();
            foreach (ListViewItem item in curListView.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                if (!File.Exists(strFileName))
                {
                    MessageBox.Show("文件已不存在[" + strFileName + "]！", "复制文件");
                    return;
                }
                strcoll.Add(strFileName);
            }

            Clipboard.SetFileDropList(strcoll);
        }
        private void listViewAdd_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            CListViewColumnSorter.SortListViewColumn(sender, e);
        }
        private void listViewMod_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            CListViewColumnSorter.SortListViewColumn(sender, e);
        }
        private void listViewRm_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            CListViewColumnSorter.SortListViewColumn(sender, e);
        }
        private void listViewCom_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            CListViewColumnSorter.SortListViewColumn(sender, e);
        }
        private void listViewEmpty_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            CListViewColumnSorter.SortListViewColumn(sender, e);
        }
        private void CopyFileNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            ContextMenuStrip ms = (ContextMenuStrip)mi.GetCurrentParent();
            ListView curListView = (ListView)ms.SourceControl;

            if (curListView.SelectedItems.Count == 0)
            {
                return;
            }
            string strPath = string.Empty;
            foreach (ListViewItem item in curListView.SelectedItems)
            {
                string strFileName = item.SubItems[1].Text;
                strPath += strFileName + "\n";
            }
            strPath = strPath.Remove(strPath.Length - 1);
            Clipboard.SetDataObject(strPath);
        }
        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            ContextMenuStrip ms = (ContextMenuStrip)mi.GetCurrentParent();
            ListView curListView = (ListView)ms.SourceControl;

            if (curListView.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (ListViewItem item in curListView.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                Common.ShowFileProperties(strFileName);
            }
        }
        private void listViewAdd_KeyDown(object sender, KeyEventArgs e)
        {
            SuperDocSearcher.ListViewKeyDown(sender, e);
        }
        private void listViewMod_KeyDown(object sender, KeyEventArgs e)
        {
            SuperDocSearcher.ListViewKeyDown(sender, e);
        }
        private void listViewRm_KeyDown(object sender, KeyEventArgs e)
        {
            SuperDocSearcher.ListViewKeyDown(sender, e);
        }
        private void listViewCom_KeyDown(object sender, KeyEventArgs e)
        {
            SuperDocSearcher.ListViewKeyDown(sender, e);
        }
        private void listViewEmpty_KeyDown(object sender, KeyEventArgs e)
        {
            SuperDocSearcher.ListViewKeyDown(sender, e);
        }

        public event Func<bool> Vacuum;
        private void buttonVacuum_Click(object sender, EventArgs e)
        {
            if (m_bVacuuming)
            {
                MessageBox.Show("已经在压缩了，等会吧！", "压缩索引");
                return;
            }
            backgroundWorkerVacuum.RunWorkerAsync();
        }
        private bool m_bVacuuming = false;
        private void backgroundWorkerVacuum_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Vacuum != null)
            {
                m_bVacuuming = true;
                Vacuum();
            }
        }
        private void backgroundWorkerVacuum_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_bVacuuming = false;
            MessageBox.Show("任务完成", "压缩索引");
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (m_bVacuuming)
            {
                MessageBox.Show("压缩索引完成后才能关闭！", "关闭");
                e.Cancel = true;
            }
            else
            {
                base.OnClosing(e);
            }
        }

        
    }
}
