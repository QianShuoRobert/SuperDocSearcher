using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperDocSearcher
{
    public partial class SuperDocSearcher : Form
    {
        public SuperDocSearcher()
        {
            InitializeComponent();
            this.textBoxCond.GotFocus += textBoxCond_GotFocus;
            this.LoadConfiguration();
        }

        #region 成员变量
        private bool m_bDoDetecting = true; //输入密码认证后是否自动检测（比对本地和数据库的差异）
        private string m_strThemeName = string.Empty; //配置中读取的主题名称
        private static string[] SIZE_UNIT = new string[] { " B", "KB", "MB", "GB"};
        SqLiteHelper m_sql = null;
        private List<CFileInfo> m_addList = null;
        private List<CFileInfo> m_rmList = null;
        private List<CFileInfo> m_modList = null;
        private List<CFileInfo> m_comList = null;
        private List<CFileInfo> m_emptyList = null;
        private bool m_bSync = false;
        private bool m_bDetect = false;
        System.Threading.Thread m_syncThread = null;
        System.Threading.Thread m_searchThread = null;
        private int m_iTotalKeyword = 0;
        private int m_iCurKeyword = 0;
        private List<int> m_keyWordPos = null;
        private bool m_bSearchFileName = true;
        private bool m_bSearchFileContent = true;
        private string m_strKeyword = string.Empty;
        private string m_strSearchPath = string.Empty;
        private bool m_bCaseSensitive = false; //大小写匹配
        private string m_strSyncDir = string.Empty; //同步目录
        private bool m_bWholeWord = false; //全字匹配
        private long m_lSearchID = 0; //本次搜索ID，用于处理线程池退出
        MyFolderBrowser m_searchFolder = new MyFolderBrowser();
        private string m_strCaption = string.Empty;
        private int m_iUpLine = 0; //摘要显示向上行数
        private int m_iDownLine = 0; //摘要显示向下行数
        private bool m_bDigestMode = false; //是否摘要模式显示
        private string m_strContent = string.Empty; //要显示的文本内容，缓存
        private const string m_strSplit = "=================================================="; //摘要显示的分隔符
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private bool m_bLoadedConfig = false; //是否已完成load，未load之前不保存配置
        private Dictionary<string, Encoding> m_dTextEncodingType = new Dictionary<string, Encoding>
        {
            {"default", Encoding.Default},
            {"utf-8", Encoding.UTF8},
            {"ascii", Encoding.ASCII}
        };
        private string m_strTextEncodingType = "default"; //纯文本文件编码格式
        #endregion

        # region 界面交互
        public void flashTaskBar(Common.falshType type)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<Common.falshType>(flashTaskBar), type);
            }
            else
            {
                Common.FLASHWINFO fInfo = new Common.FLASHWINFO();
                fInfo.cbSize = Convert.ToUInt32(System.Runtime.InteropServices.Marshal.SizeOf(fInfo));
                fInfo.hwnd = this.Handle;//要闪烁的窗口的句柄，该窗口可以是打开的或最小化的
                fInfo.dwFlags = (uint)type;//闪烁的类型
                fInfo.uCount = UInt32.MaxValue;//闪烁窗口的次数
                fInfo.dwTimeout = 0; //窗口闪烁的频度，毫秒为单位；若该值为0，则为默认图标的闪烁频度
                Common.FlashWindowEx(ref fInfo);
            }
        }
        void textBoxCond_GotFocus(object sender, EventArgs e)
        {
            this.textBoxCond.Tag = true;
            this.textBoxCond.SelectAll();
        }
        private void SuperDocSearcher_Load(object sender, EventArgs e)
        {
            m_strCaption = this.Text;
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(4, 4);
            this.listViewResult.ListViewItemSorter = new CListViewColumnSorter();
            this.LoadThemes();
            this.ShowSearchDescription();
        }
        private void LoadThemes()
        {
            try
            {
                string strSkinPath = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Skins");
                System.IO.DirectoryInfo dirSkins = new System.IO.DirectoryInfo(strSkinPath);
                System.IO.FileInfo[] filePlugins = dirSkins.GetFiles("*.ssk");
                foreach (System.IO.FileInfo fileInfo in filePlugins)
                {
                    string strSkinFileName = fileInfo.Name;
                    string strSkinNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(strSkinFileName);
                    ToolStripMenuItem skinItem = new ToolStripMenuItem();
                    skinItem.Text = strSkinNameWithoutExtension;
                    ToolStripMenuItemTheme.DropDownItems.Add(skinItem);
                    skinItem.Click += skinItem_Click;
                    if (strSkinNameWithoutExtension.Equals(m_strThemeName))
                    {
                        skinItem.Checked = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void skinItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem skinItem = sender as ToolStripMenuItem;
            string strSkinPath = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Skins");
            strSkinPath = System.IO.Path.Combine(strSkinPath, skinItem.Text + ".ssk");
            foreach (ToolStripMenuItem item in this.ToolStripMenuItemTheme.DropDownItems)
            {
                item.Checked = false;
            }
            skinItem.Checked = true;
            this.mySkinEngine.SkinFile = strSkinPath;
            config.AppSettings.Settings["overallTheme"].Value = skinItem.Text;
            config.Save();
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            this.Invalidate();
        }
        private void SearchOptionsChanged(object sender = null, EventArgs e = null)
        {
            string strSendName = string.Empty;
            if (sender is ToolStripMenuItem)
            {
                strSendName  = (sender as ToolStripMenuItem).Name;
            }
            else if (sender is ToolStripTextBox)
            {
                strSendName = (sender as ToolStripTextBox).Name;
            }
            if (!string.IsNullOrEmpty(strSendName))
            {
                switch (strSendName)
                {
                        //搜索类型
                    case "ToolStripMenuItemWord":
                    case "ToolStripMenuItemExcel":
                    case "ToolStripMenuItemPdf":
                    case "ToolStripMenuItemTxt":
                    case "ToolStripMenuItemC":
                    case "ToolStripMenuItemCpp":
                    case "ToolStripMenuItemCsharp":
                    case "ToolStripMenuItemJava":
                    case "ToolStripMenuItemXml":
                    case "ToolStripMenuItemJson":
                    case "ToolStripMenuItemSelf":
                    case "toolStripTextBoxSelf":
                        this.ToolStripMenuItemSearchType.ShowDropDown();
                        break;
                        //搜索选项
                    case "ToolStripMenuItemCaseSen":
                    case "ToolStripMenuItemWholeWord":
                    case "ToolStripMenuItemFilePath":
                    case "ToolStripMenuItemFileContent":
                        this.ToolStripMenuItemSearchOptions.ShowDropDown();
                        break;
                        //外观
                    //case "ToolStripMenuItemTheme":
                    //    this.ToolStripMenuItemOutlook.ShowDropDown();
                    //    this.ToolStripMenuItemTheme.ShowDropDown();
                    //    break;
                    case "toolStripTextBoxSyncDir":
                        break;
                    default:
                        break;
                }
            }

            this.ShowSearchDescription();
            this.SaveConfigurations();
        }
        
        private int WM_SYSCOMMAND = 0x112;
        private long SC_NORMAL = 0xD120;
        private long SC_MAXIMIZE = 0xF030;
        private long SC_MINIMIZE = 0xF020;
        private long SC_CLOSE = 0xF060;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)
            {
                long lCommand = m.WParam.ToInt64();
                if (lCommand == SC_NORMAL)
                {
                    //MessageBox.Show("NORMAL ");
                    //return;
                    config.AppSettings.Settings["windowState"].Value = "0";
                    config.Save();
                }
                else if (lCommand == SC_MAXIMIZE)
                {
                    //MessageBox.Show("MAXIMIZE ");
                    //return;
                    config.AppSettings.Settings["windowState"].Value = "2";
                    config.Save();
                }
                else if (lCommand == SC_MINIMIZE)
                {
                    //MessageBox.Show("MINIMIZE ");
                    //return;
                    //config.AppSettings.Settings["windowState"].Value = "1";
                    //config.Save();
                }
                else if (lCommand == SC_CLOSE)
                {
                    //MessageBox.Show("CLOSE ");
                    //return;
                }
            }
            base.WndProc(ref m);
        }
        private void ShowSearchDescription()
        {
            StringBuilder sbDescription = new StringBuilder();
            string strSearchTypes = "";
            if (this.ToolStripMenuItemWord.Checked) strSearchTypes += "Word ";
            if (this.ToolStripMenuItemExcel.Checked) strSearchTypes += "Excel ";
            if (this.ToolStripMenuItemPdf.Checked) strSearchTypes += "PDF ";
            if (this.ToolStripMenuItemTxt.Checked) strSearchTypes += "TXT ";
            if (this.ToolStripMenuItemC.Checked) strSearchTypes += "C ";
            if (this.ToolStripMenuItemCpp.Checked) strSearchTypes += "C++ ";
            if (this.ToolStripMenuItemCsharp.Checked) strSearchTypes += "C# ";
            if (this.ToolStripMenuItemJava.Checked) strSearchTypes += "Java ";
            if (this.ToolStripMenuItemXml.Checked) strSearchTypes += "XML ";
            if (this.ToolStripMenuItemJson.Checked) strSearchTypes += "Json ";
            if (this.ToolStripMenuItemSelf.Checked) strSearchTypes += this.toolStripTextBoxSelf.Text;
            strSearchTypes = strSearchTypes.TrimEnd().Replace(" ", ", ");
            sbDescription.Append("搜索类型[").Append(strSearchTypes).Append("]");
            string strSearchOptions = "";
            if (ToolStripMenuItemCaseSen.Checked)
            {
                strSearchOptions += "大小写匹配";
            }
            else
            {
                strSearchOptions += "非大小写匹配";
            }
            if (ToolStripMenuItemWholeWord.Checked)
            {
                strSearchOptions += ",全词匹配";
            }
            else
            {
                strSearchOptions += ",非全词匹配";
            }

            sbDescription.Append(",搜索条件[").Append(strSearchOptions).Append("]");
            string strDir = this.toolStripTextBoxSearchDir.Text;
            if (string.IsNullOrEmpty(strDir))
            {
                strDir = "全部";
            }
            string strSyncDir = this.toolStripTextBoxSyncDir.Text;
            if (string.IsNullOrEmpty(strSyncDir))
            {
                strSyncDir = "全盘";
            }
            sbDescription.Append(",搜索位置[").Append(strDir).Append("|").Append(strSyncDir).Append("]");
            Dictionary<string, string> dicCon2Desp = new Dictionary<string, string>()
            {
                {"default", "系统默认"},
                {"utf-8", "UTF-8"},
                {"ascii", "ASCII"}
            };
            string strTextEncodingType = dicCon2Desp[m_strTextEncodingType];
            sbDescription.Append(",文本文件编码格式[").Append(strTextEncodingType).Append("]");
            this.labelSearchDescription.Text = sbDescription.ToString();
        }
        private void LoadConfiguration()
        {
            //窗口状态
            int iWindowState = 0;
            int.TryParse(config.AppSettings.Settings["windowState"].Value, out iWindowState);
            this.WindowState = (FormWindowState)iWindowState;

            //认证后检测
            string strDoDetecting = System.Configuration.ConfigurationManager.AppSettings["doDetecting"];
            bool.TryParse(strDoDetecting, out m_bDoDetecting);

            //搜索类型
            bool bSearchWordFiles = true;
            bool bSearchExcelFiles = true;
            bool bSearchPdfFiles = true;
            bool bSearchTxtFiles = false;
            bool bSearchCFiles = false;
            bool bSearchCppFiles = false;
            bool bSearchCsharpFiles = false;
            bool bSearchJavaFiles = false;
            bool bSearchXmlFiles = false;
            bool bSsearchJsonFiles = false;
            bool bSearchSelfFiles = false;
            string strSelfFilesType = string.Empty;
            string strTextEncodingType = string.Empty;
            bool.TryParse(config.AppSettings.Settings["searchWordFiles"].Value, out bSearchWordFiles);
            bool.TryParse(config.AppSettings.Settings["searchExcelFiles"].Value, out bSearchExcelFiles);
            bool.TryParse(config.AppSettings.Settings["searchPdfFiles"].Value, out bSearchPdfFiles);
            bool.TryParse(config.AppSettings.Settings["searchTxtFiles"].Value, out bSearchTxtFiles);
            bool.TryParse(config.AppSettings.Settings["searchCFiles"].Value, out bSearchCFiles);
            bool.TryParse(config.AppSettings.Settings["searchCppFiles"].Value, out bSearchCppFiles);
            bool.TryParse(config.AppSettings.Settings["searchCsharpFiles"].Value, out bSearchCsharpFiles);
            bool.TryParse(config.AppSettings.Settings["searchJavaFiles"].Value, out bSearchJavaFiles);
            bool.TryParse(config.AppSettings.Settings["searchXmlFiles"].Value, out bSearchXmlFiles);
            bool.TryParse(config.AppSettings.Settings["searchJsonFiles"].Value, out bSsearchJsonFiles);
            bool.TryParse(config.AppSettings.Settings["searchSelfFiles"].Value, out bSearchSelfFiles);
            strSelfFilesType = config.AppSettings.Settings["SelfFilesType"].Value;
            this.ToolStripMenuItemWord.Checked = bSearchWordFiles;
            this.ToolStripMenuItemExcel.Checked = bSearchExcelFiles;
            this.ToolStripMenuItemPdf.Checked = bSearchPdfFiles;
            this.ToolStripMenuItemTxt.Checked = bSearchTxtFiles;
            this.ToolStripMenuItemC.Checked = bSearchCFiles;
            this.ToolStripMenuItemCpp.Checked = bSearchCppFiles;
            this.ToolStripMenuItemCsharp.Checked = bSearchCsharpFiles;
            this.ToolStripMenuItemJava.Checked = bSearchJavaFiles;
            this.ToolStripMenuItemXml.Checked = bSearchXmlFiles;
            this.ToolStripMenuItemJson.Checked = bSsearchJsonFiles;
            this.ToolStripMenuItemSelf.Checked = bSearchSelfFiles;
            this.toolStripTextBoxSelf.Text = strSelfFilesType;
            this.toolStripTextBoxSelf.Enabled = this.ToolStripMenuItemSelf.Checked;

            //搜索选项
            bool bSearchCaseSen = false;
            bool bSearchMatchWholeWord = false;
            bool bSearchFilePath = true;
            bool bSearchFileContent = true;
            bool.TryParse(config.AppSettings.Settings["searchCaseSen"].Value, out bSearchCaseSen);
            bool.TryParse(config.AppSettings.Settings["searchMatchWholeWord"].Value, out bSearchMatchWholeWord);
            bool.TryParse(config.AppSettings.Settings["searchFilePath"].Value, out bSearchFilePath);
            bool.TryParse(config.AppSettings.Settings["searchFileContent"].Value, out bSearchFileContent);
            strTextEncodingType = config.AppSettings.Settings["TextEncodingType"].Value;

            this.ToolStripMenuItemCaseSen.Checked = bSearchCaseSen;
            this.ToolStripMenuItemWholeWord.Checked = bSearchMatchWholeWord;
            this.ToolStripMenuItemFilePath.Checked = bSearchFilePath;
            this.ToolStripMenuItemFileContent.Checked = bSearchFileContent;
            this.SetTextEncodingType(strTextEncodingType);

            //搜索位置
            string strSearchDir = string.Empty;
            strSearchDir = config.AppSettings.Settings["searchDir"].Value;
            this.toolStripTextBoxSearchDir.Text = strSearchDir;

            //同步目录
            string strSyncDir = string.Empty;
            strSyncDir = config.AppSettings.Settings["syncDir"].Value;
            this.toolStripTextBoxSyncDir.Text = strSyncDir;

            //外观
            this.m_strThemeName = config.AppSettings.Settings["overallTheme"].Value;
            string strSkinFile = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Skins", m_strThemeName + ".ssk");
            this.mySkinEngine.SkinFile = strSkinFile;

            this.m_bLoadedConfig = true;
        }
        private void SaveConfigurations()
        {
            if (!this.m_bLoadedConfig)
            {
                //未加载过不做保存
                return;
            }
            //搜索类型
            config.AppSettings.Settings["searchWordFiles"].Value = this.ToolStripMenuItemWord.Checked.ToString();
            config.AppSettings.Settings["searchExcelFiles"].Value = this.ToolStripMenuItemExcel.Checked.ToString();
            config.AppSettings.Settings["searchPdfFiles"].Value = this.ToolStripMenuItemPdf.Checked.ToString();
            config.AppSettings.Settings["searchTxtFiles"].Value = this.ToolStripMenuItemTxt.Checked.ToString();
            config.AppSettings.Settings["searchCFiles"].Value = this.ToolStripMenuItemC.Checked.ToString();
            config.AppSettings.Settings["searchCppFiles"].Value = this.ToolStripMenuItemCpp.Checked.ToString();
            config.AppSettings.Settings["searchCsharpFiles"].Value = this.ToolStripMenuItemCsharp.Checked.ToString();
            config.AppSettings.Settings["searchJavaFiles"].Value = this.ToolStripMenuItemJava.Checked.ToString();
            config.AppSettings.Settings["searchXmlFiles"].Value = this.ToolStripMenuItemXml.Checked.ToString();
            config.AppSettings.Settings["searchJsonFiles"].Value = this.ToolStripMenuItemJson.Checked.ToString();
            config.AppSettings.Settings["searchSelfFiles"].Value = this.ToolStripMenuItemSelf.Checked.ToString();
            config.AppSettings.Settings["SelfFilesType"].Value = this.toolStripTextBoxSelf.Text;


            //搜索选项
            config.AppSettings.Settings["searchCaseSen"].Value = this.ToolStripMenuItemCaseSen.Checked.ToString();
            config.AppSettings.Settings["searchMatchWholeWord"].Value = this.ToolStripMenuItemWholeWord.Checked.ToString();
            config.AppSettings.Settings["searchFilePath"].Value = this.ToolStripMenuItemFilePath.Checked.ToString();
            config.AppSettings.Settings["searchFileContent"].Value = this.ToolStripMenuItemFileContent.Checked.ToString();
            config.AppSettings.Settings["TextEncodingType"].Value = this.m_strTextEncodingType;

            //搜索位置
            config.AppSettings.Settings["searchDir"].Value = this.toolStripTextBoxSearchDir.Text;
            config.AppSettings.Settings["syncDir"].Value = this.toolStripTextBoxSyncDir.Text;
            //保存
            config.Save();
        }
        private void SuperDocSearcher_Shown(object sender, EventArgs e)
        {
            uint everythingMajor = EveryThingHelper.Everything_GetMajorVersion();
            uint everythingMinor = EveryThingHelper.Everything_GetMinorVersion();
            uint everythingRevision = EveryThingHelper.Everything_GetRevision();
            if (everythingMajor <= 0 && everythingMinor <= 0)
            {
                if (MessageBox.Show("请先安装并启动Everything，本软件使用了Everything SDK，\n是否打开Everything下载页面(https://www.voidtools.com)？", "未检测到Everything服务", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://www.voidtools.com");
                }
                this.Close();
                return;
            }
            if (everythingMajor > 1
                || (everythingMajor == 1 && everythingMinor > 4)
                || (everythingMajor == 1 && everythingMinor == 4 && everythingRevision >= 1))
            {
                //1.4.1或更高版本，支持从Everything的结果中获取文件修改时间和大小等信息，之前版本只能获取文件名
            }
            else
            {
                string strVersion = everythingMajor.ToString() + "." + everythingMinor.ToString() + "." + everythingRevision.ToString();
                if (MessageBox.Show("当前版本" + strVersion + "，请升级Everything到1.4.1或更高版本！\n是否打开Everything下载页面(https://www.voidtools.com)？", "Everything版本低", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://www.voidtools.com");
                }
                this.Close();
                return;
            }

            //这个文件用来缓存用户数据，最初是没有的，当用户第一次运行，设置完密码之后就生成了
            string strDbFileName = "FileList.db";
            //这个文件是数据库模板文件，包括表的数据结构，在用户第一次启动时生成用户缓存数据库
            string strDb1FileName = "FileList1.db";
            string startPath = Application.StartupPath;
            string dbFile = Path.Combine(startPath, strDbFileName);
            string dbFile1 = Path.Combine(startPath, strDb1FileName);
            bool bFirstTimeRun = false;
            if (!File.Exists(dbFile))
            {
                if (!File.Exists(dbFile1))
                {
                    MessageBox.Show("未检测到数据库文件，程序包已损坏！");
                    this.Close();
                    return;
                }
                else
                {
                    bFirstTimeRun = true;
                }
            }

            Password pswd = new Password();
            pswd.FirstTimeRun = bFirstTimeRun;
            pswd.DoDetecting = m_bDoDetecting;
            if (!(pswd.ShowDialog() == System.Windows.Forms.DialogResult.OK))
            {
                if (pswd.ForgotPasswd)
                {
                    try
                    {
                        File.Delete(strDbFileName);
                    }
                    catch (Exception ex) { }
                }
                this.Close();
                return;
            }
            string strPasswd = string.Empty;
            if (bFirstTimeRun)
            {
                File.Copy(dbFile1, dbFile);
                this.m_sql = new SqLiteHelper("data source=" + strDbFileName);
                this.m_sql.OpenConnection(); //出异常就崩溃，不处理
                this.m_sql.ChangePassword(pswd.NewPasswd); //第一次运行，设置新密码
                this.m_sql.CloseConnection();   //设置完密码要断开，后面再重连
                strPasswd = pswd.NewPasswd;
            }
            else
            {
                if (!string.IsNullOrEmpty(pswd.NewPasswd))
                {
                    //设置了新密码
                    this.m_sql = new SqLiteHelper("data source=" + strDbFileName + ";password=" + pswd.Passwd);
                    try
                    {
                        m_sql.OpenConnection();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        MessageBox.Show("原密码验证失败！", "修改密码");
                        this.Close();
                        return;
                    }
                    //更新密码
                    m_sql.ChangePassword(pswd.NewPasswd);
                    m_sql.CloseConnection();
                    strPasswd = pswd.NewPasswd;
                }
                else
                {
                    //没有设置新密码
                    strPasswd = pswd.Passwd;
                }
            }

            this.m_sql = new SqLiteHelper("data source=" + strDbFileName + ";password=" + strPasswd);
            try
            {
                this.m_sql.OpenConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "打开数据库失败");
                this.Close();
                return;
            }
            this.richTextBoxDetails.AutoWordSelection = false;
            //启动后先检测一下
            if (pswd.DoDetecting)
            {
                ToolStripMenuItemDetect_Click(null, null);
            }
            if (pswd.DoDetecting != m_bDoDetecting)
            {
                //配置发生了修改
                m_bDoDetecting = pswd.DoDetecting;
                config.AppSettings.Settings["doDetecting"].Value = m_bDoDetecting.ToString();
            }

            this.textBoxCond.Focus();
        }
        /// <summary>
        /// 获取quicklook的目录，没有则返回string.Empty;
        /// </summary>
        /// <returns></returns>
        private static string GetQuicklookExe()
        {
            const int bufsize = 1024;
            StringBuilder buf = new StringBuilder(bufsize);

            EveryThingHelper.Everything_SetRequestFlags(EveryThingHelper.EVERYTHING_REQUEST_FILE_NAME
                | EveryThingHelper.EVERYTHING_REQUEST_PATH
                | EveryThingHelper.EVERYTHING_REQUEST_SIZE
                | EveryThingHelper.EVERYTHING_REQUEST_DATE_MODIFIED);
            // set the search
            EveryThingHelper.Everything_SetSearch("file:wholefilename:quicklook.exe");

            // sort by size
            //Everything_SetSort(EVERYTHING_SORT_SIZE_DESCENDING);

            // execute the query
            EveryThingHelper.Everything_Query(true);

            // sort by path
            //EveryThingHelper.Everything_SortResultsByPath();

            uint iResultNum = EveryThingHelper.Everything_GetNumResults();
            if (iResultNum == 0)
            {
                //未安装quicklook
                return string.Empty;
            }
            try
            {
                    EveryThingHelper.Everything_GetResultFullPathName(0, buf, bufsize);
                    return buf.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }
        private void textBoxCond_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                this.DoSearch();
                this.listViewResult.Focus();
            }
        }
        private void SuperDocSearcher_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                //都已经退出了，再有异常也不能崩溃
                if (this.m_searchThread != null && this.m_searchThread.IsAlive)
                {
                    this.m_searchThread.Abort();
                }
                this.SetSyncState(false);
                if (this.m_syncThread != null && this.m_syncThread.IsAlive)
                {
                    Thread.Sleep(500);
                    this.m_syncThread.Abort();
                }
                if (m_sql != null)
                {
                    m_sql.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void ToolStripMenuItemDetect_Click(object sender, EventArgs e)
        {
            if (m_bDetect)
            {
                MessageBox.Show("已经在检测了，等会吧", "检测");
                return;
            }
            if (m_bSync)
            {
                MessageBox.Show("正在同步，请先停止同步", "检测");
                return;
            }
            Thread thread = new Thread(this.DetectThreadEntry);
            thread.Name = "DetectThread";
            thread.Start();
        }
        private void textBoxCond_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (bool)this.textBoxCond.Tag == true)
            {
                textBoxCond.SelectAll();
            }
            //取消全选标记
            textBoxCond.Tag = false;
        }
        public static string SizeConverter(long size)
        {
            int level = 0;
            double dSize = size;
            while (true)
            {
                double dTry = dSize / 1024.0;
                if (dTry < 1.0)
                {
                    break;
                }
                else
                {
                    dSize = dTry;
                    ++level;
                }
            }
            if (level == 0)
            {
                return size.ToString() + " " + SIZE_UNIT[level];
            }
            return dSize.ToString("f2") + " " + SIZE_UNIT[level];
        }
        private void DoSearch()
        {
            this.m_lSearchID = this.m_lSearchID + 1;
            //this.m_bSearchFileName = this.checkBoxFileName.Checked;
            //this.m_bSearchFileContent = this.checkBoxFileContent.Checked;
            this.m_bSearchFileName = ToolStripMenuItemFilePath.Checked;
            this.m_bSearchFileContent = ToolStripMenuItemFileContent.Checked;
            this.m_strKeyword = this.textBoxCond.Text;
            this.m_strSearchPath = this.toolStripTextBoxSearchDir.Text;
            this.m_bCaseSensitive = this.ToolStripMenuItemCaseSen.Checked;
            this.m_bWholeWord = this.ToolStripMenuItemWholeWord.Checked;
            this.textBoxFilePath.Clear();
            this.richTextBoxDetails.Clear();
            this.m_strContent = string.Empty;
            this.m_iCurKeyword = 0;
            this.m_iTotalKeyword = 0;
            this.SetFindState();
            if (!string.IsNullOrEmpty(this.m_strKeyword))
            {
                this.Text = this.m_strKeyword + " - " + this.m_strCaption;
            }
            else
            {
                this.Text = this.m_strCaption;
            }
            //进度条先动一下，表示开始了
            this.SetSearchProgressBarPrecent(5);
            if (m_searchThread != null && m_searchThread.IsAlive)
            {
                m_searchThread.Abort();
                Thread.Sleep(50);
            }
            this.listViewResult.Items.Clear();
            //搜索结果不排序，点标题栏再进行排序
            (this.listViewResult.ListViewItemSorter as CListViewColumnSorter).Order = SortOrder.None;
            m_searchThread = new Thread(this.DoSearchThreadEntry);
            m_searchThread.Name = "DoSearchThread";
            m_searchThread.Start();
        }
        private void AddSearchResultItem(CFileInfo fileInfo)
        {
            if (this.listViewResult.InvokeRequired)
            {
                this.BeginInvoke(new Action<CFileInfo>(AddSearchResultItem), fileInfo);
            }
            else
            {
                string strExtension = Path.GetExtension(fileInfo.FullPath).Replace(".", "").ToLower();
                if ((strExtension.Equals("doc") || strExtension.Equals("docx")) && !this.ToolStripMenuItemWord.Checked)
                {
                    //未勾选搜索Word类型
                    return;
                }
                if ((strExtension.Equals("xls") || strExtension.Equals("xlsx")) && !this.ToolStripMenuItemExcel.Checked)
                {
                    //未勾选搜索Excel类型
                    return;
                }
                if (strExtension.Equals("pdf") && !this.ToolStripMenuItemPdf.Checked)
                {
                    //未勾选搜索PDF类型
                    return;
                }
                ListViewItem item = new ListViewItem();
                item.SubItems[0].Text = (this.listViewResult.Items.Count + 1).ToString();
                item.SubItems.Add(fileInfo.FileName);
                item.SubItems.Add(fileInfo.FullPath);
                item.SubItems.Add(SizeConverter(fileInfo.FileSize));
                item.SubItems.Add(fileInfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
                item.SubItems.Add(strExtension);
                item.SubItems.Add(fileInfo.KeywordCount.ToString());
                this.listViewResult.Items.Add(item);
            }
        }
        private bool MessageBoxShowYesOrNo(string message, string caption)
        {
            if (this.InvokeRequired)
            {
                return Convert.ToBoolean(this.Invoke(new Func<string, string, bool>(MessageBoxShowYesOrNo), message, caption));
            }
            else
            {
                if (MessageBox.Show(message, caption, MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        private void SearchTxtFile(object param)
        {
            CFileInfo fileInfo = param as CFileInfo;
            try
            {
                if (File.Exists(fileInfo.FullPath) && fileInfo.SearchID == this.m_lSearchID)
                {
                    //文件存在且搜索ID未变化才进行搜索
                    if (m_bSearchFileName && m_bSearchFileContent)
                    {
                        //搜索文件名和内容
                        string strContent = File.ReadAllText(fileInfo.FullPath, m_dTextEncodingType[m_strTextEncodingType]);
                        fileInfo.KeywordCount = GetKeyWordCount(strContent, null);
                        if (GetKeyWordCount(fileInfo.FullPath, null) > 0 || fileInfo.KeywordCount > 0)
                        {
                            fileInfo.IsNeedAdd = true;
                        }
                    }
                    else if (m_bSearchFileName && !m_bSearchFileContent)
                    {
                        //只搜索文件名
                        if (GetKeyWordCount(fileInfo.FullPath, null) > 0)
                        {
                            fileInfo.IsNeedAdd = true;
                        }
                    }
                    else if (!m_bSearchFileName && m_bSearchFileContent)
                    {
                        //只搜索文件内容
                        string strContent = File.ReadAllText(fileInfo.FullPath, m_dTextEncodingType[m_strTextEncodingType]);
                        fileInfo.KeywordCount = GetKeyWordCount(strContent, null);
                        if (fileInfo.KeywordCount > 0)
                        {
                            fileInfo.IsNeedAdd = true;
                        }
                    }
                    else
                    {
                        //什么都不搜索
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                fileInfo.IsSearched = true;
            }
        }
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            this.DoSearch();
            this.listViewResult.Focus();
        }
        private string GetDbFileContent(string fileName)
        {
            string strContent = string.Empty;
            string strSql = "select CONTENT from FILES where FILE_NAME='" + fileName.Replace("'", "''") + "'";
            SQLiteDataReader sdr = m_sql.ExecuteQuery(strSql);
            if (sdr != null && sdr.Read())
            {
                strContent = sdr[0].ToString();
            }
            return strContent;
        }
        /// <summary>
        /// 摘要显示时，让分隔符高亮，区别于普通文本
        /// </summary>
        private void HighLightSplit()
        {
            if (!m_bDigestMode)
            {
                return;
            }

            int index = 0;
            while ((index = this.richTextBoxDetails.Find(m_strSplit, index, RichTextBoxFinds.WholeWord)) >= 0)
            {
                richTextBoxDetails.Select(index, m_strSplit.Length);
                richTextBoxDetails.SelectionColor = Color.Orange;
                index += m_strSplit.Length;
            }
        }
        private void HighLightKeyword()
        {
            m_iTotalKeyword = 0;
            if (m_keyWordPos == null)
            {
                return;
            }
            foreach (int pos in m_keyWordPos)
            {
                richTextBoxDetails.Select(pos, this.m_strKeyword.Length);
                richTextBoxDetails.SelectionBackColor = Color.Pink;
            }
            m_iTotalKeyword = m_keyWordPos.Count;
            this.richTextBoxDetails.SelectionLength = 0;
        }
        private void listViewResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.richTextBoxDetails.Clear();
            this.textBoxFilePath.Clear();
            this.m_iTotalKeyword = 0;
            this.m_iCurKeyword = 0;
            this.SetFindState();
            this.toolTipShowInfo.SetToolTip(this.listViewResult, "");
            if (this.listViewResult.SelectedItems.Count != 1)
            {
                //选中多行的话，就不开预览了
                return;
            }
            string strFileName = this.listViewResult.SelectedItems[0].SubItems[2].Text;
            string strContent = string.Empty;
            string strExtension = Path.GetExtension(strFileName).ToLower();
            if (strExtension == ".doc" || strExtension == ".docx" 
                || strExtension == ".pdf" 
                || strExtension == ".xls"
                || strExtension == ".xlsx")
            {
                strContent = this.GetDbFileContent(strFileName);
            }
            else
            {
                try
                {
                    strContent = File.ReadAllText(strFileName, m_dTextEncodingType[m_strTextEncodingType]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            this.textBoxFilePath.Text = strFileName;
            this.richTextBoxDetails.SelectionLength = 0;
            m_strContent = strContent;
            this.ShowDocContent(strContent);
            string strTip = "序号：" + this.listViewResult.SelectedItems[0].Text + "\n"
                + "名称：" + this.listViewResult.SelectedItems[0].SubItems[1].Text + "\n"
                + "路径：" + this.listViewResult.SelectedItems[0].SubItems[2].Text + "\n"
                + "大小：" + this.listViewResult.SelectedItems[0].SubItems[3].Text + "\n"
                + "修改时间：" + this.listViewResult.SelectedItems[0].SubItems[4].Text + "\n"
                + "类型：" + this.listViewResult.SelectedItems[0].SubItems[5].Text + "\n"
                + "计数：" + this.listViewResult.SelectedItems[0].SubItems[6].Text;
            this.toolTipShowInfo.SetToolTip(this.listViewResult, strTip);
        }
        private void listViewResult_KeyDown(object sender, KeyEventArgs e)
        {
            ListViewKeyDown(sender, e);
        }
        public static void ListViewKeyDown(object sender, KeyEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView == null)
            {
                return;
            }
            if (e.Control)
            {
                if (e.KeyCode == Keys.A)
                {
                    //Ctrl+A 全选
                    e.SuppressKeyPress = true;
                    if (listView == null)
                    {
                        return;
                    }
                    foreach (ListViewItem item in listView.Items)
                    {
                        item.Selected = true;
                    }
                }
                else if (e.KeyCode == Keys.C)
                {
                    //Ctrl+C 复制文件
                    e.SuppressKeyPress = true;
                    if (listView.SelectedItems.Count == 0)
                    {
                        return;
                    }
                    System.Collections.Specialized.StringCollection strcoll = new System.Collections.Specialized.StringCollection();
                    foreach (ListViewItem item in listView.SelectedItems)
                    {
                        string strFileName = item.SubItems[2].Text;
                        if (!File.Exists(strFileName))
                        {
                            MessageBox.Show("文件已不存在[" + strFileName + "]！", "复制文件");
                            return;
                        }
                        strcoll.Add(strFileName);
                    }
                    try
                    {
                        Clipboard.SetFileDropList(strcoll);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "发生了异常");
                    }
                }
            }
            else if (e.KeyCode == Keys.Space)
            {
                //空格键实现预览
                string strQuickLookExePath = GetQuicklookExe();
                if (string.IsNullOrEmpty(strQuickLookExePath))
                {
                    //没有安装quicklook,不支持空格预览
                    MessageBox.Show("请安装quicklook实现空格预览！", "未找到应用程序");
                    return;
                }
                e.SuppressKeyPress = true;
                if (listView.SelectedItems.Count != 1)
                {
                    return;
                }

                string strFileName = listView.SelectedItems[0].SubItems[2].Text;
                if (!File.Exists(strFileName))
                {
                    MessageBox.Show("文件已不存在[" + strFileName + "]！", "预览文件");
                    return;
                }
                try
                {
                    strFileName = "\"" + strFileName + "\""; //加上双引号，防止有空格发生错误
                    Process pro = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo(strQuickLookExePath, strFileName);
                    startInfo.UseShellExecute = false;    //是否使用操作系统的shell启动
                    startInfo.RedirectStandardInput = false;      //接受来自调用程序的输入     
                    startInfo.RedirectStandardOutput = true;     //由调用程序获取输出信息
                    startInfo.CreateNoWindow = true;             //不显示调用程序的窗口
                    pro.StartInfo = startInfo;
                    pro.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "发生了异常");
                }
            }
        }
        /// <summary>
        /// 将文本内容显示到界面上
        /// </summary>
        /// <param name="strContent"></param>
        private void ShowDocContent(string strContent)
        {
            this.richTextBoxDetails.Clear();
            if (string.IsNullOrEmpty(strContent))
            {
                return;
            }
            if (this.m_bDigestMode)
            {
                //把文本按行分开
                string[] strLines = strContent.Split(new char[] { '\r', '\n' });
                List<int> lNewContent = new List<int>();
                for (int i = 0; i < strLines.Length; ++i)
                {
                    if (this.GetKeyWordCount(strLines[i], null) > 0)
                    {
                        for(int j = i - m_iUpLine; j < i + m_iDownLine + 1; ++j)
                        {
                            if (j < 0 || j >= strLines.Length)
                            {
                                continue;
                            }
                            if (!lNewContent.Contains(j))
                            {
                                lNewContent.Add(j);
                            }
                        }
                    }
                }
                StringBuilder sbContent = new StringBuilder();
                for (int i = 0; i < lNewContent.Count; ++i)
                {
                    sbContent.Append(strLines[lNewContent[i]]);
                    sbContent.Append("\n");
                    if (i + 1 < lNewContent.Count && lNewContent[i + 1] > lNewContent[i] + 1)
                    {
                        sbContent.Append(m_strSplit);
                        sbContent.Append("\n");
                    }
                }
                this.richTextBoxDetails.Text = sbContent.ToString();
            }
            else
            {
                this.richTextBoxDetails.Text = strContent;
            }
            //分割线高亮一下
            HighLightSplit();
            //这里得重新计算一下，richtextbox的Text赋值前后不一样
            this.m_keyWordPos = new List<int>();
            this.GetKeyWordCount(this.richTextBoxDetails.Text, this.m_keyWordPos);
            this.HighLightKeyword();
            this.richTextBoxDetails.SelectionStart = 0;
            this.richTextBoxDetails.SelectionLength = 0;
            this.FindNextKeyword();
            this.SetFindState();
        }
        private void FindNextKeyword()
        {
            if (string.IsNullOrEmpty(m_strKeyword) || m_iTotalKeyword == 0)
            {
                return;
            }
            int index = this.richTextBoxDetails.SelectionStart;
            for (int i = 0; i < this.m_keyWordPos.Count; ++i )
            {
                if (m_keyWordPos[i] >= index)
                {
                    this.richTextBoxDetails.SelectionStart = m_keyWordPos[i] + m_strKeyword.Length;
                    this.richTextBoxDetails.SelectionLength = 0;
                    this.richTextBoxDetails.ScrollToCaret();
                    m_iCurKeyword = i + 1;
                    break;
                }
            }
            this.SetFindState();
        }
        private void FindPreKeyword()
        {
            if (string.IsNullOrEmpty(m_strKeyword) || m_iTotalKeyword == 0)
            {
                return;
            }
            int index = this.richTextBoxDetails.SelectionStart;
            for (int i = m_keyWordPos.Count - 1; i >= 0; --i)
            {
                if (m_keyWordPos[i] + m_strKeyword.Length < index)
                {
                    this.richTextBoxDetails.SelectionStart = m_keyWordPos[i] + m_strKeyword.Length;
                    this.richTextBoxDetails.SelectionLength = 0;
                    this.richTextBoxDetails.ScrollToCaret();
                    m_iCurKeyword = i + 1;
                    break;
                }
            }
            this.SetFindState();
        }
        private void richTextBoxDetails_SelectionChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.richTextBoxDetails.SelectedText)
                && m_keyWordPos != null && m_keyWordPos.Count > 0
                && !string.IsNullOrEmpty(m_strKeyword))
            {
                int iStart = this.richTextBoxDetails.SelectionStart;
                int index = 0;
                for (index = 0; index < m_keyWordPos.Count; ++index)
                {
                    if (m_keyWordPos[index] + m_strKeyword.Length > iStart)
                    {
                        break;
                    }
                }
                m_iCurKeyword = index;
                SetFindState();
            }
        }
        private void SetFindState()
        {
            this.labelFindState.Text = "第 " + m_iCurKeyword.ToString() + "/" + m_iTotalKeyword.ToString() + " 个";
        }
        private void richTextBoxDetails_KeyPress(object sender, KeyPressEventArgs e)
        {
        }
        private void richTextBoxDetails_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (e.Shift)
                {
                    this.FindPreKeyword();
                }
                else
                {
                    this.FindNextKeyword();
                }
                e.Handled = true;
            }
        }
        private void listViewResult_DoubleClick(object sender, EventArgs e)
        {
            if (this.listViewResult.SelectedItems.Count == 0)
            {
                return;
            }
            string strFileName = this.listViewResult.SelectedItems[0].SubItems[2].Text;
            OpenFile(strFileName);
        }
        public static void OpenFile(string fileFullPath)
        {
            if (File.Exists(fileFullPath))
            {
                try
                {
                    System.Diagnostics.Process.Start(fileFullPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + fileFullPath, "打开文件失败");
                }
            }
        }
        /// <summary>
        /// 右键菜单从列表中移除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveFormListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewResult.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (ListViewItem item in this.listViewResult.SelectedItems)
            {
                this.listViewResult.Items.Remove(item);
            }
        }
        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewResult.SelectedItems.Count == 0)
            {
                return;
            }
            //选中多行的文件逐个打开
            foreach (ListViewItem item in this.listViewResult.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                OpenFile(strFileName);
            }
        }
        private void OpenDirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewResult.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (ListViewItem item in this.listViewResult.SelectedItems)
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
            if (this.listViewResult.SelectedItems.Count == 0)
            {
                return;
            }
            string strPath = string.Empty;
            //选中的多个文件名都复制到剪切板
            foreach (ListViewItem item in this.listViewResult.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                string strFilePath = Path.GetDirectoryName(strFileName);
                strPath += strFilePath + "\n";
            }
            strPath = strPath.Remove(strPath.Length - 1);
            try
            {
                Clipboard.SetDataObject(strPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "发生了异常");
            }
        }
        private void CopyFullPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewResult.SelectedItems.Count == 0)
            {
                return;
            }
            string strPath = string.Empty;
            foreach (ListViewItem item in this.listViewResult.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                strPath += strFileName + "\n";
            }
            strPath = strPath.Remove(strPath.Length - 1);
            try
            {
                Clipboard.SetDataObject(strPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "发生了异常");
            }
        }
        private void CopyFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewResult.SelectedItems.Count == 0)
            {
                return;
            }
            System.Collections.Specialized.StringCollection strcoll = new System.Collections.Specialized.StringCollection();
            //把选中的多个文件，逐一添加到剪切板
            foreach (ListViewItem item in this.listViewResult.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                if (!File.Exists(strFileName))
                {
                    //选中的多个文件中如果有一个不存在就中止复制
                    MessageBox.Show("文件已不存在[" + strFileName + "]！", "复制文件");
                    return;
                }
                strcoll.Add(strFileName);
            }
            try
            {
                Clipboard.SetFileDropList(strcoll);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "发生了异常");
            }
        }
        /// <summary>
        /// 右键菜单复制文件名处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyFileNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewResult.SelectedItems.Count == 0)
            {
                return;
            }
            string strPath = string.Empty;
            foreach (ListViewItem item in this.listViewResult.SelectedItems)
            {
                string strFileName = item.SubItems[1].Text;
                strPath += strFileName + "\n";
            }
            strPath = strPath.Remove(strPath.Length - 1);
            try
            {
                Clipboard.SetDataObject(strPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "发生了异常");
            }
        }
        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewResult.SelectedItems.Count == 0)
            {
                return;
            }
            //选中多行的文件逐个打开
            foreach (ListViewItem item in this.listViewResult.SelectedItems)
            {
                string strFileName = item.SubItems[2].Text;
                Common.ShowFileProperties(strFileName);
            }
        }
        private void SetSyncState(bool sync)
        {
            //一定要放在这里
            m_bSync = sync;
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<bool>(SetSyncState), sync);
            }
            else
            {
                this.panelSync.Visible = true;
                if (sync)
                {
                    //设置状态为同步
                    this.ToolStripMenuItemDetect.Enabled = false;
                    this.ToolStripMenuItemSync.Text = "停止同步";
                }
                else
                {
                    //设置状态为未同步
                    this.ToolStripMenuItemDetect.Enabled = true;
                    this.ToolStripMenuItemSync.Text = "开始同步";
                }
            }
        }
        /// <summary>
        /// 右键菜单自动换行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItemWordWrap_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            ContextMenuStrip ms = (ContextMenuStrip)mi.GetCurrentParent();
            RichTextBox richTextBox = (RichTextBox)ms.SourceControl;
            richTextBox.WordWrap = mi.Checked;
        }
        /// <summary>
        /// 右键菜单复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            ContextMenuStrip ms = (ContextMenuStrip)mi.GetCurrentParent();
            RichTextBox richTextBox = (RichTextBox)ms.SourceControl;
            string strSelected = richTextBox.SelectedText;
            try
            {
                Clipboard.SetDataObject(strSelected);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "发生了异常");
            }
        }
        /// <summary>
        /// 右键菜单摘要显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItemDigest_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            this.m_bDigestMode = mi.Checked;
            this.ShowDocContent(this.m_strContent);
        }
        /// <summary>
        /// 右键菜单摘要行数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItemDigestLine_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            DigestConfiguration formDigest = new DigestConfiguration();
            formDigest.StartLocation = Control.MousePosition;
            formDigest.UpLine = this.m_iUpLine;
            formDigest.DownLine = this.m_iDownLine;
            if (formDigest.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.m_iUpLine = formDigest.UpLine;
                this.m_iDownLine = formDigest.DownLine;
                if (this.m_bDigestMode)
                {
                    this.ShowDocContent(this.m_strContent);
                }
            }
        }
        private void labelStatus_Click(object sender, EventArgs e)
        {
            if (this.m_addList == null
                    || this.m_rmList == null
                    || this.m_modList == null
                    || this.m_comList == null)
            {
                return;
            }
            SyncInfo syncInfo = new SyncInfo();
            syncInfo.ClearEmpty += syncInfo_ClearEmpty;
            syncInfo.Vacuum += syncInfo_Vacuum;
            syncInfo.AddList = this.m_addList;
            syncInfo.RmList = this.m_rmList;
            syncInfo.ModList = this.m_modList;
            syncInfo.ComList = this.m_comList;
            syncInfo.EmptyList = this.m_emptyList;
            syncInfo.ShowDialog();
        }
        private bool syncInfo_Vacuum()
        {
            try
            {
                this.m_sql.ExecuteQuery("vacuum");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        private bool syncInfo_ClearEmpty(string fileName)
        {
            string strSql = string.Empty;
            if (string.IsNullOrEmpty(fileName))
            {
                //为空表示清空所有
                strSql = "delete from FILES where CONTENT= ''";
            }
            else
            {
                //清空指定文件
                strSql = "delete from FILES where FILE_NAME='" + fileName.Replace("'", "''") + "'";
            }
            try
            {
                this.m_sql.ExecuteQuery(strSql);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        private void ToolStripMenuItemSync_Click(object sender, EventArgs e)
        {
            if (this.m_addList == null
                    || this.m_rmList == null
                    || this.m_modList == null
                    || this.m_comList == null)
            {
                MessageBox.Show("还未检测，点下检测按钮！", "同步");
                return;
            }
            if (m_bDetect)
            {
                MessageBox.Show("正在检测，马上就好了！", "同步");
                return;
            }
            if (!m_bSync)
            {
                //没有同步线程，开启线程进行同步
                m_syncThread = new System.Threading.Thread(SyncThreadEntry);
                m_syncThread.Start(new object { });
            }
            else
            {
                this.SetSyncState(false);
                Thread.Sleep(100);
                if (this.m_syncThread.IsAlive)
                {
                    this.m_syncThread.Abort();
                }
            }
        }
        private void SetStatusMessage(string message)
        {
            if (this.labelStatus.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(SetStatusMessage), message);
            }
            else
            {
                this.panelSync.Visible = true;
                this.labelStatus.Text = message;
            }
        }
        private void SetSyncProgressBarPrecent(int percentage)
        {
            if (this.progressBarSync.InvokeRequired)
            {
                this.BeginInvoke(new Action<int>(SetSyncProgressBarPrecent), percentage);
            }
            else
            {
                this.panelSync.Visible = true;
                this.progressBarSync.Value = percentage * this.progressBarSync.Maximum / 100;
            }
        }
        private void SetSearchStateMessage(string message)
        {
            if (this.labelSearchPercentage.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(SetSearchStateMessage), message);
            }
            else
            {
                this.labelSearchPercentage.Text = message;
            }
        }
        private void SetSearchProgressBarPrecent(int percentage)
        {
            if (this.progressBarSearchProgress.InvokeRequired)
            {
                this.BeginInvoke(new Action<int>(SetSearchProgressBarPrecent), percentage);
            }
            else
            {
                this.progressBarSearchProgress.Value = percentage * this.progressBarSearchProgress.Maximum / 100;
                this.labelSearchPercentage.Text = percentage.ToString() + " %";
            }
        }
        //private void textBoxSearchDir_MouseUp(object sender, MouseEventArgs e)
        //{
        //    //隐藏烦人的闪烁光标
        //    Common.HideCaret(((TextBox)sender).Handle);
        //}
        private void listViewResult_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            CListViewColumnSorter.SortListViewColumn(sender, e);
        }
        private bool IsLetterOrDigit(char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                return true;
            }
            else if (c >= 'A' && c <= 'Z')
            {
                return true;
            }
            else if (c >= '0' && c <= '9')
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 返回strContent中关键字的个数，如果lKeyWordPos不为null，则返回关键字的位置列表
        /// </summary>
        /// <param name="strContent"></param>
        /// <param name="lKeyWordPos"></param>
        /// <returns></returns>
        private int GetKeyWordCount(string strContent, List<int> lKeyWordPos)
        {
            int iCount = 0;
            if (lKeyWordPos != null)
            {
                lKeyWordPos.Clear();
            }
            if (!string.IsNullOrEmpty(m_strKeyword) && !string.IsNullOrEmpty(strContent))
            {
                int index = 0;
                System.StringComparison strComp = StringComparison.OrdinalIgnoreCase;
                if (this.m_bCaseSensitive)
                {
                    strComp = StringComparison.Ordinal;
                }
                while ((index = strContent.IndexOf(this.m_strKeyword, index, strComp)) >= 0)
                {
                    if (this.m_bWholeWord)
                    {
                        bool bHead = true;
                        bool bTail = true;
                        if (index - 1 >= 0 && this.IsLetterOrDigit(strContent[index - 1]))
                        {
                            //前面有字母或数字
                            bHead = false;
                        }
                        if (bHead && index + m_strKeyword.Length < strContent.Length && this.IsLetterOrDigit(strContent[index + m_strKeyword.Length]))
                        {
                            bTail = false;
                        }
                        if (bHead && bTail)
                        {
                            ++iCount;
                            if (lKeyWordPos != null)
                            {
                                lKeyWordPos.Add(index);
                            }
                        }
                    }
                    else
                    {
                        ++iCount;
                        if (lKeyWordPos != null)
                        {
                            lKeyWordPos.Add(index);
                        }
                    }
                    index += m_strKeyword.Length;
                }
            }
            return iCount;
        }
        private void toolStripTextBoxSearchDir_Click(object sender, EventArgs e)
        {
            if (m_searchFolder.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                this.toolStripTextBoxSearchDir.Text = m_searchFolder.SelectedPath;
                SearchOptionsChanged();
            }
        }
        private void ToolStripMenuItemSelf_CheckedChanged(object sender, EventArgs e)
        {
            this.toolStripTextBoxSelf.Enabled = this.ToolStripMenuItemSelf.Checked;
        }
        private void progressBarSync_Click(object sender, EventArgs e)
        {
            //this.panelSync.Visible = false;
        }
        #endregion

        #region 文件获取和比对
        public class CFileInfo
        {
            public CFileInfo()
            {
                FileName = string.Empty;
                FullPath = string.Empty;
                FileSize = 0;
                LastWriteTime = new DateTime(0);
                IsSearched = false;
                IsNeedAdd = false;
                SearchID = 0;
                KeywordCount = 0;
            }
            public string FileName { get; set; }
            public string FullPath { get; set; }
            public long FileSize { get; set; }
            public DateTime LastWriteTime { get; set; }
            //标记文件是否被读取过了
            public bool IsSearched { get; set; }
            public bool IsNeedAdd { get; set; }
            public long SearchID { get; set; }
            //包含关键字个数
            public int KeywordCount { get; set; }
            public static bool operator ==(CFileInfo cfileInfo1, CFileInfo cfileInfo2)
            {
                if (cfileInfo1.FileName == cfileInfo2.FileName
                    && cfileInfo1.FullPath == cfileInfo2.FullPath
                    && cfileInfo1.FileSize == cfileInfo2.FileSize
                    && cfileInfo1.LastWriteTime - cfileInfo2.LastWriteTime < new TimeSpan(0, 0, 1))
                {
                    return true;
                }
                return false;
            }
            public static bool operator !=(CFileInfo cfileInfo1, CFileInfo cfileInfo2)
            {
                return !(cfileInfo1 == cfileInfo2);
            }
            public static bool operator >(CFileInfo cfileInfo1, CFileInfo cfileInfo2)
            {
                if (string.Compare(cfileInfo1.FullPath, cfileInfo2.FullPath) > 0)
                {
                    return true;
                }
                return false;
            }
            public static bool operator <(CFileInfo cfileInfo1, CFileInfo cfileInfo2)
            {
                if (string.Compare(cfileInfo1.FullPath, cfileInfo2.FullPath) < 0)
                {
                    return true;
                }
                return false;
            }
            public override bool Equals(object obj)
            {
                return this == (CFileInfo)obj;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        private List<CFileInfo> GetLocalDocList()
        {
            List<CFileInfo> fileList = new List<CFileInfo>();
            const int bufsize = 1024;
            StringBuilder buf = new StringBuilder(bufsize);

            EveryThingHelper.Everything_SetRequestFlags(EveryThingHelper.EVERYTHING_REQUEST_FILE_NAME
                | EveryThingHelper.EVERYTHING_REQUEST_PATH
                | EveryThingHelper.EVERYTHING_REQUEST_SIZE
                | EveryThingHelper.EVERYTHING_REQUEST_DATE_MODIFIED);
            // set the search
            //EveryThingHelper.Everything_SetSearch("file:*.doc|*.docx|*.pdf|*.xls|*.xlsx");
            EveryThingHelper.Everything_SetSearch("\"" + m_strSyncDir + "\" " + "file:*.doc|*.docx|*.pdf|*.xlsx");

            // sort by size
            //Everything_SetSort(EVERYTHING_SORT_SIZE_DESCENDING);

            // execute the query
            EveryThingHelper.Everything_Query(true);

            // sort by path
            //EveryThingHelper.Everything_SortResultsByPath();

            uint iResultNum = EveryThingHelper.Everything_GetNumResults();
            try
            {
                for (uint i = 0; i < iResultNum; ++i)
                {
                    EveryThingHelper.Everything_GetResultFullPathName(i, buf, bufsize);
                    string fileFullPath = buf.ToString();
                    string fileName = Path.GetFileName(fileFullPath);
                    if (fileName.IndexOf('~') == 0 || fileName.IndexOf('$') == 0
                        || fileFullPath.ToUpper().Contains("RECYCLE.BIN"))
                    {
                        continue;
                    }
                    else
                    {
                        CFileInfo cfileInfo = new CFileInfo();
                        long lModTime = 0;
                        long lSize = 0;
                        EveryThingHelper.Everything_GetResultDateModified(i, out lModTime);
                        EveryThingHelper.Everything_GetResultSize(i, out lSize);
                        cfileInfo.FullPath = fileFullPath;
                        cfileInfo.FileName = fileName;
                        cfileInfo.FileSize = lSize;
                        cfileInfo.LastWriteTime = new DateTime(lModTime);
                        cfileInfo.LastWriteTime = cfileInfo.LastWriteTime.AddYears(1600);
                        fileList.Add(cfileInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return fileList;
        }
        private bool CompareLocalToDb(out List<CFileInfo> addedList,
            out List<CFileInfo> removedList,
            out List<CFileInfo> modedList,
            out List<CFileInfo> commonList,
            List<CFileInfo> localList,
            List<CFileInfo> dbList)
        {
            addedList = new List<CFileInfo>();
            removedList = new List<CFileInfo>();
            modedList = new List<CFileInfo>();
            commonList = new List<CFileInfo>();

            if (localList == null || dbList == null)
            {
                return false;
            }
            //重新排序
            var newLocalList =
                from lFile in localList
                orderby lFile.FullPath ascending
                select lFile;
            localList = newLocalList.ToList();

            var newDbList =
                from dFile in dbList
                orderby dFile.FullPath ascending
                select dFile;
            dbList = newDbList.ToList();

            int locIndex = 0;
            int dbIndex = 0;
            while (locIndex < localList.Count && dbIndex < dbList.Count)
            {
                int comRet = string.Compare(localList[locIndex].FullPath, dbList[dbIndex].FullPath);
                if (comRet == 0)
                {
                    //找到相同文件名
                    if (localList[locIndex] == dbList[dbIndex])
                    {
                        commonList.Add(localList[locIndex]);
                    }
                    else
                    {
                        modedList.Add(localList[locIndex]);
                    }
                    ++locIndex;
                    ++dbIndex;
                }
                else if (comRet < 0)
                {
                    //本地文件有，数据库没有
                    addedList.Add(localList[locIndex]);
                    ++locIndex;
                }
                else
                {
                    //本地没有，数据库有
                    removedList.Add(dbList[dbIndex]);
                    ++dbIndex;
                }
            }
            if (locIndex < localList.Count)
            {
                for (; locIndex < localList.Count; ++locIndex)
                {
                    addedList.Add(localList[locIndex]);
                }
            }
            if (dbIndex < dbList.Count)
            {
                for (; dbIndex < dbList.Count; ++dbIndex)
                {
                    removedList.Add(dbList[dbIndex]);
                }
            }

            //去除可能不是不是文件的,everything搜索结果中包含同名目录
            List<CFileInfo> lNeedToRm = new List<CFileInfo>();
            //新增列表中可能会有文件名格式的目录，例如xxx.doc是一个目录而不是文件，这里需要从硬盘上检测文件是否真正存在
            int index = 0;
            while (index < addedList.Count)
            {
                if (File.Exists(addedList[index].FullPath))
                {
                    //文件存在，跳过
                    ++index;
                }
                else
                {
                    //注意，删除之后不能增加索引
                    addedList.RemoveAt(index);
                }
            }
            //已删除、已修改、已同步都是基于数据库的，不会有非文件
            return true;
        }
        private List<CFileInfo> GetDBDocList()
        {
            List<CFileInfo> result = new List<CFileInfo>();
            try
            {
                string strSql = @"select FILE_NAME,FILE_SIZE,DATE_MODIFIED from FILES order by FILE_NAME ASC";
                SQLiteDataReader sdr = m_sql.ExecuteQuery(strSql);
                if (sdr != null)
                {
                    while (sdr.Read())
                    {
                        CFileInfo cfileInfo = new CFileInfo();
                        cfileInfo.FullPath = sdr[0].ToString();
                        cfileInfo.FileName = Path.GetFileName(cfileInfo.FullPath);
                        cfileInfo.FileSize = Convert.ToInt64(sdr[1]);
                        cfileInfo.LastWriteTime = Convert.ToDateTime(sdr[2]);
                        result.Add(cfileInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }
        private List<CFileInfo> GetDBEmptyDocList()
        {
            List<CFileInfo> result = new List<CFileInfo>();
            try
            {
                string strSql = @"select FILE_NAME,FILE_SIZE,DATE_MODIFIED from FILES where CONTENT = ''  order by FILE_NAME ASC";
                SQLiteDataReader sdr = m_sql.ExecuteQuery(strSql);
                if (sdr != null)
                {
                    while (sdr.Read())
                    {
                        CFileInfo cfileInfo = new CFileInfo();
                        cfileInfo.FullPath = sdr[0].ToString();
                        cfileInfo.FileName = Path.GetFileName(cfileInfo.FullPath);
                        cfileInfo.FileSize = Convert.ToInt64(sdr[1]);
                        cfileInfo.LastWriteTime = Convert.ToDateTime(sdr[2]);
                        result.Add(cfileInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }
        private List<CFileInfo> QueryDB(string keyword, string specilDir, bool fileName, bool fileContent)
        {
            List<CFileInfo> queryResult = new List<CFileInfo>();
            string strSql = string.Empty;
            if (fileName && fileContent)
            {
                strSql = "select FILE_NAME,FILE_SIZE,DATE_MODIFIED from FILES where CONTENT like '%" + keyword + "%' or FILE_NAME like '%" + keyword + "%'";
            }
            else if (fileName && !fileContent)
            {
                strSql = "select FILE_NAME,FILE_SIZE,DATE_MODIFIED from FILES where FILE_NAME like '%" + keyword + "%'";
            }
            else if (!fileName && fileContent)
            {
                strSql = "select FILE_NAME,FILE_SIZE,DATE_MODIFIED from FILES where CONTENT like '%" + keyword + "%'";
            }
            else
            {
                return queryResult;
            }
            try
            {
                bool bSpecialDir = false;
                if (!string.IsNullOrEmpty(specilDir))
                {
                    bSpecialDir = true;
                }
                strSql += " order by FILE_NAME ASC";
                if (m_bCaseSensitive)
                {
                    //设置数据库like时大小写敏感，后面显示到UI之前还会再处理一下搜索结果，
                    //包括大小写检测和全字匹配规则，因为sql没有全字匹配规则
                    m_sql.ExecuteQuery("PRAGMA case_sensitive_like=1");
                }
                else
                {
                    m_sql.ExecuteQuery("PRAGMA case_sensitive_like=0");
                }
                SQLiteDataReader sdr = m_sql.ExecuteQuery(strSql);
                if (sdr != null)
                {
                    while (sdr.Read())
                    {
                        CFileInfo cfileInfo = new CFileInfo();
                        cfileInfo.FullPath = sdr[0].ToString();
                        if (bSpecialDir && cfileInfo.FullPath.IndexOf(specilDir) != 0)
                        {
                            //如果文件路径不符合指定路径，不返回
                            continue;
                        }
                        cfileInfo.FileName = Path.GetFileName(cfileInfo.FullPath);
                        cfileInfo.FileSize = Convert.ToInt64(sdr[1]);
                        cfileInfo.LastWriteTime = Convert.ToDateTime(sdr[2]);
                        queryResult.Add(cfileInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return queryResult;
        }
        #endregion

        #region 线程
        private void SyncThreadEntry(object obj)
        {
            DateTime dtStart = DateTime.Now;
            //记录新打开的word进程,如果关闭失败就强制杀死
            bool bNormalExit = true;
            Process myWordProcess = null;

            this.SetSyncState(true);
            if (this.m_addList == null
                    || this.m_rmList == null
                    || this.m_modList == null
                    || this.m_comList == null)
            {
                throw new Exception("还未检测！");
            }
            this.SetSyncProgressBarPrecent(0);
            try
            {
                //把待修改列表加入到添加列表和删除列表，先删除数据库中缓存，再添加
                this.m_rmList.AddRange(this.m_modList);
                this.m_addList.InsertRange(0, m_modList); //先更新有修改的
                this.m_modList.Clear();

                while (m_bSync && m_rmList.Count > 0)
                {
                    //删除数据库中本地不存在的缓存
                    string strSql = "delete from FILES where FILE_NAME='" + m_rmList[0].FullPath.Replace("'", "''") + "'";
                    m_sql.ExecuteQuery(strSql);
                    //每次删除rm列表的第一个元素，直到为空
                    m_rmList.RemoveAt(0);
                }
                if (this.m_addList.Count > 0)
                {
                    Microsoft.Office.Interop.Word.Application wordApp = null;
                    //处理新增的列表
                    int iTotalCount = this.m_addList.Count;
                    int iCurReadCount = 0;
                    while (m_bSync && this.m_addList.Count > 0)
                    {
                        CFileInfo fileInfo = m_addList[0];
                        m_addList.RemoveAt(0);
                        string strFileFullPath = fileInfo.FullPath;
                        string strExtension = Path.GetExtension(strFileFullPath).ToLower();
                        this.SetStatusMessage("正在读取：" + strFileFullPath);
                        string strContent = string.Empty;
                        if (strExtension == ".doc" || strExtension == ".docx")
                        {
                            //需要读取word时再创建word进程
                            if (wordApp == null)
                            {
                                //word进程还未创建
                                //先清理一下空word进程，一般是不存在的
                                Process[] wordProcesses = Process.GetProcessesByName("WINWORD");
                                foreach (Process pro in wordProcesses) //这里是找到那些没有界面的Word进程
                                {
                                    //发现程序中打开跟用户自己打开的区别就在这个属性
                                    string strProcessTitle = pro.MainWindowTitle;
                                    //用户打开的str 是文件的名称，程序中打开的就是空字符串
                                    if (string.IsNullOrEmpty(strProcessTitle))
                                    {
                                        pro.Kill();
                                    }
                                }
                                wordApp = new Microsoft.Office.Interop.Word.Application();
                                wordApp.Visible = false;

                                //找到上面创建的Word进程
                                wordProcesses = Process.GetProcessesByName("WINWORD");
                                foreach (Process pro in wordProcesses) //这里是找到那些没有界面的Word进程
                                {
                                    //发现程序中打开跟用户自己打开的区别就在这个属性
                                    string strProcessTitle = pro.MainWindowTitle;
                                    //用户打开的str 是文件的名称，程序中打开的就是空字符串
                                    if (string.IsNullOrEmpty(strProcessTitle))
                                    {
                                        myWordProcess = pro;
                                    }
                                }
                                if (myWordProcess == null)
                                {
                                    throw new Exception("创建Word进程失败");
                                }
                            }
                            else
                            {
                                if (myWordProcess.HasExited)
                                {
                                    throw new Exception("Word进程已退出");
                                }
                            }
                            //读取Word文件
                            Common.ReadWordText(wordApp, strFileFullPath, out strContent);
                        }
                        else if (strExtension == ".pdf")
                        {
                            //读取PDF文件，不需要创建单独的进程
                            Common.ReadPdfText(strFileFullPath, out strContent);
                        }
                        else if (strExtension == ".xls" || strExtension == ".xlsx")
                        {
                            //读取Excel文本
                            Common.ReadExcelText(strFileFullPath, out strContent);
                        }

                        //字符'会引起sql语句错误，0会引起字符串截断
                        strContent = strContent.Replace('\'', '\"').Replace('\0', ' ');
                        //文件名中的'用两个表示转义
                        this.m_sql.InsertValues("FILES", new string[] { strFileFullPath.Replace("'", "''"),
                        fileInfo.FileSize.ToString(),
                        fileInfo.LastWriteTime.ToString(), strContent });
                        ++iCurReadCount;
                        int percentage = iCurReadCount * 100 / iTotalCount;
                        this.SetSyncProgressBarPrecent(percentage);
                    }
                    if (wordApp != null)
                    {
                        object saveChanges = false;
                        object originalFormat = false;
                        object routeDoc = false;
                        wordApp.Quit(ref saveChanges, ref originalFormat, ref routeDoc);
                    }
                }
                DateTime dtNow = DateTime.Now;
                double dTotalSeconds = (dtNow - dtStart).TotalSeconds;
                string strTimeSpent = string.Empty;
                if (dTotalSeconds < 100.0)
                {
                    strTimeSpent = dTotalSeconds.ToString("f1") + "s";
                }
                else
                {
                    strTimeSpent = dTotalSeconds.ToString("f0") + "s";
                }
                this.SetSyncProgressBarPrecent(100);
                this.SetStatusMessage("同步完成！ " + "结束时间[" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "] 用时[" + strTimeSpent + "]");
            }
            catch (Exception ex)
            {
                bNormalExit = false;
                DateTime dtNow = DateTime.Now;
                double dTotalSeconds = (dtNow - dtStart).TotalSeconds;
                string strTimeSpent = string.Empty;
                if (dTotalSeconds < 100.0)
                {
                    strTimeSpent = dTotalSeconds.ToString("f1") + "s";
                }
                else
                {
                    strTimeSpent = dTotalSeconds.ToString("f0") + "s";
                }
                SetStatusMessage("发生异常：" + ex.Message + "！ 结束时间[" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "] 用时[" + strTimeSpent + "]");

            }
            //非正常退出，尝试进程清理
            if (!bNormalExit)
            {
                try
                {
                    //如果进程不存在了，会有异常发生
                    if (myWordProcess != null)
                    {
                        myWordProcess.Kill();
                    }
                }
                catch (Exception) { }
            }
            this.SetSyncState(false);
        }
        private void DetectThreadEntry(object obj)
        {
            m_bDetect = true;
            DateTime dtBegin = DateTime.Now;
            this.SetStatusMessage("开始检测...");
            this.SetSyncProgressBarPrecent(0);
            this.SetStatusMessage("获取本地文件列表...");
            List<CFileInfo> fileList = this.GetLocalDocList();
            this.SetSyncProgressBarPrecent(20);
            this.SetStatusMessage("获取数据库文件列表...");
            List<CFileInfo> dbFileList = this.GetDBDocList();
            this.SetSyncProgressBarPrecent(40);
            this.m_emptyList = this.GetDBEmptyDocList();
            this.SetSyncProgressBarPrecent(60);
            this.SetStatusMessage("比对本地和数据库文件差异...");
            this.CompareLocalToDb(out m_addList, out m_rmList, out m_modList, out m_comList, fileList, dbFileList);
            this.SetSyncProgressBarPrecent(80);
            DateTime dtEnd = DateTime.Now;
            double dTotalSeconds = (dtEnd - dtBegin).TotalSeconds;
            string strTimeSpent = string.Empty;
            if (dTotalSeconds < 100.0)
            {
                strTimeSpent = dTotalSeconds.ToString("f1") + "s";
            }
            else
            {
                strTimeSpent = dTotalSeconds.ToString("f0") + "s";
            }
            this.SetStatusMessage("检测完成！"
                + "完成时间[" + dtEnd.ToString("yyyy-MM-dd HH:mm:ss") + "] 用时[" + strTimeSpent + "] "
                + "新增[" + m_addList.Count.ToString() + "] "
                + "移除[" + m_rmList.Count.ToString() + "] "
                + "修改[" + m_modList.Count.ToString() + "] "
                + "已同步[" + m_comList.Count.ToString() + "]"
                + "(其中打不开的文档[" + m_emptyList.Count + "])");
            this.SetSyncProgressBarPrecent(100);
            m_bDetect = false;
        }
        private void DoSearchThreadEntry(object param)
        {
            DateTime dtBegin = DateTime.Now;
            try
            {
                if (this.ToolStripMenuItemWord.Checked
                    || this.ToolStripMenuItemPdf.Checked
                    || this.ToolStripMenuItemExcel.Checked)
                {
                    //先搜索doc(x)
                    List<CFileInfo> searchRet = this.QueryDB(
                        this.m_strKeyword,
                        this.m_strSearchPath,
                        this.m_bSearchFileName,
                        this.m_bSearchFileContent);
                    this.SetSearchProgressBarPrecent(15);
                    if (searchRet != null)
                    {
                        foreach (var fileInfo in searchRet)
                        {
                            if (!string.IsNullOrEmpty(m_strKeyword))
                            {
                                string strContent = this.GetDbFileContent(fileInfo.FullPath);
                                fileInfo.KeywordCount = GetKeyWordCount(strContent, null);
                                //除了文档内容，也检测一下文档路径是否符合规则
                                if (fileInfo.KeywordCount == 0 && GetKeyWordCount(fileInfo.FullPath, null) == 0)
                                {
                                    //这里是为了筛选大小写匹配和全字匹配,因为sqlite没有全字匹配的选项
                                    continue;
                                }
                            }
                            this.AddSearchResultItem(fileInfo);
                        }
                    }
                }
                //搜索完doc，进度条调整到
                this.SetSearchProgressBarPrecent(20);

                if (string.IsNullOrEmpty(m_strKeyword))
                {
                    //如果关键字为空，不进行文件内搜索，到此结束
                    this.SetSearchProgressBarPrecent(100);
                    return;
                }
                //先组装搜索关键字
                string strSearchTypes = "";
                if (this.ToolStripMenuItemTxt.Checked)
                {
                    strSearchTypes += "*.txt|";
                }
                if (this.ToolStripMenuItemC.Checked)
                {
                    strSearchTypes += "*.h|*.c";
                }
                if (this.ToolStripMenuItemCpp.Checked)
                {
                    strSearchTypes += "*.h|*.cpp|*.hpp|";
                }
                if (this.ToolStripMenuItemCsharp.Checked)
                {
                    strSearchTypes += "*.cs|";
                }
                if (this.ToolStripMenuItemJava.Checked)
                {
                    strSearchTypes += "*.java|";
                }
                if (this.ToolStripMenuItemXml.Checked)
                {
                    strSearchTypes += "*.xml|";
                }
                if (this.ToolStripMenuItemJson.Checked)
                {
                    strSearchTypes += "*.json|";
                }
                if (this.ToolStripMenuItemSelf.Checked && !string.IsNullOrEmpty(this.toolStripTextBoxSelf.Text))
                {
                    strSearchTypes += "*." + this.toolStripTextBoxSelf.Text + "|";
                }
                //去除最后一个'|'
                if (!string.IsNullOrEmpty(strSearchTypes) && strSearchTypes[strSearchTypes.Length - 1] == '|')
                {
                    strSearchTypes = strSearchTypes.Remove(strSearchTypes.Length - 1);
                }
                //调用everything SDK进行搜索
                List<CFileInfo> fileList = new List<CFileInfo>();
                string strCurFileName = string.Empty;
                if (!string.IsNullOrEmpty(strSearchTypes))
                {
                    const int bufsize = 1024;
                    StringBuilder buf = new StringBuilder(bufsize);

                    // request name and size
                    EveryThingHelper.Everything_SetRequestFlags(EveryThingHelper.EVERYTHING_REQUEST_FILE_NAME
                    | EveryThingHelper.EVERYTHING_REQUEST_PATH
                    | EveryThingHelper.EVERYTHING_REQUEST_SIZE
                    | EveryThingHelper.EVERYTHING_REQUEST_DATE_MODIFIED);

                    // set the search
                    EveryThingHelper.Everything_SetSearch("file:" + strSearchTypes);

                    // sort by size
                    //Everything_SetSort(EVERYTHING_SORT_SIZE_DESCENDING);
                    //EveryThingHelper.Everything_SetSort(EveryThingHelper.EVERYTHING_SORT_FILE_LIST_FILENAME_DESCENDING);

                    // execute the query
                    EveryThingHelper.Everything_Query(true);

                    // sort by path
                    //EveryThingHelper.Everything_SortResultsByPath();

                    this.SetSearchProgressBarPrecent(25);
                    bool bSpecialDir = false;
                    if (!string.IsNullOrEmpty(this.m_strSearchPath))
                    {
                        bSpecialDir = true;
                    }
                    uint iResultNum = EveryThingHelper.Everything_GetNumResults();
                    for (uint i = 0; i < iResultNum; ++i)
                    {
                        this.SetSearchProgressBarPrecent((int)(25 + i * 10 / iResultNum));

                        EveryThingHelper.Everything_GetResultFullPathName(i, buf, bufsize);
                        string fileFullPath = buf.ToString();
                        if (bSpecialDir && fileFullPath.IndexOf(m_strSearchPath) != 0
                            || fileFullPath.ToUpper().Contains("RECYCLE.BIN"))
                        {
                            //不符合路径要求，跳过
                            //不搜索回收站
                            continue;
                        }
                        string fileName = Path.GetFileName(fileFullPath);
                        CFileInfo cfileInfo = new CFileInfo();
                        cfileInfo.FullPath = fileFullPath;

                        cfileInfo.FileName = fileName;
                        //从everything中读取结果信息
                        long lModTime = 0;
                        long lSize = 0;
                        EveryThingHelper.Everything_GetResultDateModified(i, out lModTime);
                        EveryThingHelper.Everything_GetResultSize(i, out lSize);
                        cfileInfo.FileSize = lSize;
                        cfileInfo.LastWriteTime = new DateTime(lModTime);
                        cfileInfo.LastWriteTime = cfileInfo.LastWriteTime.AddYears(1600);
                        cfileInfo.SearchID = this.m_lSearchID;
                        fileList.Add(cfileInfo);
                    }
                    this.SetSearchProgressBarPrecent(35);

                    if (fileList.Count >= 0)
                    {
                        int index = 0;
                        foreach (CFileInfo fileInfo in fileList)
                        {
                            ++index;
                            ThreadPool.QueueUserWorkItem(new WaitCallback(SearchTxtFile), fileInfo);
                        }
                        this.SetSearchProgressBarPrecent(40);
                        int iCount = 0;
                        while (iCount < fileList.Count)
                        {
                            //每次重新数一下，解决多线程计数
                            iCount = 0;
                            foreach (CFileInfo fileInfo in fileList)
                            {
                                if (fileInfo.IsSearched)
                                {
                                    if (fileInfo.IsNeedAdd)
                                    {
                                        fileInfo.IsNeedAdd = false;
                                        //在这里添加而不在线程池回调中，是为了避免线程被中止后线程池线程仍在运行，这样界面还会继续被添加新的结果
                                        this.AddSearchResultItem(fileInfo);
                                    }
                                    ++iCount;
                                }
                            }
                            this.SetSearchProgressBarPrecent(40 + iCount * 60 / fileList.Count);
                            Thread.Sleep(500);
                        }
                    }
                }
                this.SetSearchProgressBarPrecent(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                DateTime dtEnd = DateTime.Now;
                double dTotalSeconds = (dtEnd - dtBegin).TotalSeconds;
                string strTimeSpent = string.Empty;
                if (dTotalSeconds < 100.0)
                {
                    strTimeSpent = dTotalSeconds.ToString("f1") + "s";
                }
                else
                {
                    strTimeSpent = dTotalSeconds.ToString("f0") + "s";
                }
                this.SetSearchStateMessage(strTimeSpent);
                flashTaskBar(Common.falshType.FLASHW_TIMERNOFG);
            }
        }
        #endregion

        private void ToolStripMenuItemShowHide_Click(object sender, EventArgs e)
        {
            this.panelSync.Visible = !this.panelSync.Visible;
        }

        private void ToolStripMenuItemTheme_Click(object sender, EventArgs e)
        {

        }

        private void toolStripTextBoxSyncDir_Click(object sender, EventArgs e)
        {
            if (m_bDetect || m_bSync)
            {
                MessageBox.Show("正在检测或同步，等会吧", "同步目录");
                return;
            }
            string strOldSyncDir = this.toolStripTextBoxSyncDir.Text;
            if (strOldSyncDir == null) strOldSyncDir = string.Empty;
            if (m_searchFolder.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                this.toolStripTextBoxSyncDir.Text = m_searchFolder.SelectedPath;
                SearchOptionsChanged();
            }
            if (strOldSyncDir != this.toolStripTextBoxSyncDir.Text)
            {
                MessageBox.Show("已切换同步目录，请重新检测和同步！", "同步目录");
            }
        }

        private void toolStripTextBoxSyncDir_TextChanged(object sender, EventArgs e)
        {
            this.m_strSyncDir = this.toolStripTextBoxSyncDir.Text;
            this.m_addList = null;
            this.m_rmList = null;
            this.m_modList = null;
            this.m_comList = null;
        }

        private void ToolStripMenuItemFeedback_Click(object sender, EventArgs e)
        {
            MessageBox.Show("请将详细描述发送邮件到 891581750@qq.com", "问题和建议");
        }

        private void SetTextEncodingType(string strTextEncodingType)
        {
            this.ToolStripMenuItemDefault.Checked = false;
            this.ToolStripMenuItemUTF8.Checked = false;
            this.ToolStripMenuItemASCII.Checked = false;

            m_strTextEncodingType = strTextEncodingType;
            switch (strTextEncodingType)
            {
                case "default":
                    this.ToolStripMenuItemDefault.Checked = true;
                    break;
                case "utf-8":
                    this.ToolStripMenuItemUTF8.Checked = true;
                    break;
                case "ascii":
                    this.ToolStripMenuItemASCII.Checked = true;
                    break;
                default:
                    this.ToolStripMenuItemDefault.Checked = true;
                    break;
            }
            //切换了编码类型，刷新一下预览
            this.ShowSearchDescription();
            listViewResult_SelectedIndexChanged(null, null);
        }

        private void richTextBoxDetails_HScroll(object sender, EventArgs e)
        {
            
        }

        private void richTextBoxDetails_VScroll(object sender, EventArgs e)
        {
            int iCharIndex = this.richTextBoxDetails.GetFirstCharIndexOfCurrentLine();
            int iLine = this.richTextBoxDetails.GetLineFromCharIndex(iCharIndex);
            //Trace.WriteLine
        }

        private void ToolStripMenuItemDefault_Click(object sender, EventArgs e)
        {
            this.SetTextEncodingType("default");
        }

        private void ToolStripMenuItemUTF8_Click(object sender, EventArgs e)
        {
            this.SetTextEncodingType("utf-8");
        }

        private void ToolStripMenuItemASCII_Click(object sender, EventArgs e)
        {
            this.SetTextEncodingType("ascii");
        }
    }
}
