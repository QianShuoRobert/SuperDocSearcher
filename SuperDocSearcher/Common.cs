using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SuperDocSearcher
{
    public static class Common
    {
        /// <summary>
        /// 获取文件的MD5值
        /// </summary>
        /// <param name="file"></param>
        /// <returns>成功返回文件md5sum，失败返回空字符串</returns>
        public static string GetFileMd5Sum(string file)
        {
            string strMd5Sum = string.Empty;
            if (!File.Exists(file))
            {
                return strMd5Sum;
            }
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    MD5CryptoServiceProvider md5sum = new MD5CryptoServiceProvider();
                    md5sum.ComputeHash(fs);
                    byte[] by = md5sum.Hash;
                    StringBuilder sb = new StringBuilder(32);
                    for (int i = 0; i < by.Length; ++i)
                    {
                        sb.Append(by[i].ToString("X2"));
                    }
                    strMd5Sum = sb.ToString();
                    md5sum.Clear();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
            return strMd5Sum;
        }

        /// <summary>
        /// 将Excel转换成Pdf，需要安装插件SaveAsPDFandXPS.exe
        /// </summary>
        /// <param name="excelApp"></param>
        /// <param name="strInputFile"></param>
        /// <param name="strOutFile"></param>
        /// <returns></returns>
        public static bool ConverterExcelToPdf(Microsoft.Office.Interop.Excel.Application excelApp, string strInputFile, string strOutFile)
        {
            if (excelApp == null || string.IsNullOrEmpty(strInputFile) || string.IsNullOrEmpty(strOutFile))
            {
                return false;
            }
            //Microsoft.Office.Interop.Excel.Application lobjExcelApp = null;
            //Microsoft.Office.Interop.Excel.Workbooks lobjExcelWorkBooks = null;
            Microsoft.Office.Interop.Excel.Workbook lobjExcelWorkBook = null;
            //string lstrTemp = string.Empty;
            object lobjMissing = System.Reflection.Missing.Value;
            try
            {
                //lobjExcelApp = new Microsoft.Office.Interop.Excel.Application();
                //lobjExcelApp.Visible = true;
                //lobjExcelWorkBooks = excelApp.Workbooks;
                lobjExcelWorkBook = excelApp.Workbooks.Open(
                    strInputFile, //FileName 可选 Variant String。 要打开的工作簿的文件名。 
                    false, /*UpdateLinks 可选 Variant 
                           * 指定更新文件中的外部引用 (链接) 的方式,
                           * 例如, 在以下公式=SUM([Budget.xls]Annual!C10:C25)中对位于 "xls" 工作簿中的区域的引用。 
                           * 如果省略此参数，则会提示用户指定链接的更新方式。 
                           * 有关此参数使用的值的详细信息，请参阅“备注”部分。
                           * 如果 Microsoft Excel 打开的是 WKS、WK1 或 WK3 格式的文件, 
                           * 并且 UpdateLinks 参数为 0, 
                           * 则不会创建任何图表;
                           * 否则, Microsoft Excel 将从附加到文件的图表中生成图表。 */
                    true, //ReadOnly 可选 Variant 如果为 True，则以只读模式打开工作簿。 
                    lobjMissing, //Format 可选 Variant 如果 Microsoft Excel 打开文本文件，则此参数指定分隔符字符。 如果省略此参数，则使用当前分隔符。 有关此参数使用的值的详细信息，请参阅"备注"部分。 
                    lobjMissing, //Password 可选 Variant 包含打开受保护工作簿所需密码的字符串。 如果省略此参数并且工作簿需要密码，则会提示用户输入密码。 
                    lobjMissing, //WriteResPassword 可选 Variant 包含写入写保护的工作簿所需密码的字符串。 如果省略此参数并且工作簿需要密码，则将提示用户输入密码。 
                    true, //IgnoreReadOnlyRecommended 可选 Variant 如果为 True，则不让 Microsoft Excel 显示只读的建议消息（如果该工作簿以建议只读选项保存）。 
                    lobjMissing, //Origin 可选 Variant 如果文件是文本文件，则此参数表示其来源，这样就可正确映射代码页和回车/换行 (CR/LF)。 可以是下列的**XlPlatform** 常量之一: xlMacintosh、 xlWindows或xlMSDOS。 如果省略此参数，则使用当前操作系统。 
                    lobjMissing, //Delimiter 可选 Variant 如果文件是文本文件, 而_Format_参数为 6, 则此参数是一个字符串, 用于指定要用作分隔符的字符。 例如，可使用 Chr(9) 代表制表符，使用“,”代表逗号，使用“;”代表分号，或者使用自定义字符。 只使用字符串的第一个字符。 
                    lobjMissing, /*Editable 可选 Variant 如果文件为 Microsoft Excel 4.0 外接程序，
                                  * 则此参数为 True 时可打开该外接程序以使其成为可见窗口。 
                                  * 如果此参数为 False 或被省略，则以隐藏方式打开外接程序，并且无法设为可见。 
                                  * 此选项不适用于在 Microsoft Excel 5.0 或更高版本中创建的加载项。
                                  * 如果文件是 Excel 模板，则为 True，可打开指定的模板进行编辑。 
                                  * 如果为 False，则可根据指定的模板打开新工作簿。 The default value is False. */
                    lobjMissing, //Notify 可选 Variant 当文件不能以可读写模式打开时，如果此参数为 True，则可将该文件添加到文件通知列表。 Microsoft Excel 将以只读模式打开该文件并轮询文件通知列表，并在文件可用时向用户发出通知。 如果此参数为 False 或省略，则不会请求通知，并且任何打开不可用文件的尝试都将失败。 
                    lobjMissing, //Converter 可选 Variant 打开文件时要尝试的第一个文件转换器的索引。 首先尝试指定的文件转换器；如果此转换器无法识别该文件，则尝试所有其他转换器。 转换器索引由**FileConverters** 属性返回的转换器的行号组成。 
                    false, //AddToMru 可选 Variant 如果为 True，则将该工作簿添加到最近使用的文件列表中。 默认值为 False 。 
                    lobjMissing, //Local 可选 Variant 如果为 True，则以 Microsoft Excel（包括控制面板设置）的语言保存文件。 如果为 False（默认值），则以 Visual Basic for Applications (VBA) 的语言保存文件，其中 Visual Basic for Applications (VBA) 通常为美国英语版本，除非从中运行 Workbooks.Open 的 VBA 项目是旧的已国际化的 XL5/95 VBA 项目。 
                    lobjMissing //CorruptLoad 可选 XlCorruptLoad 可为以下常量之一：xlNormalLoad、xlRepairFile 和 xlExtractData。 如果未指定任何值, 则默认行为为xlNormalLoad, 并且在通过 OM 启动时不尝试恢复。 
                    );
                //Microsoft.Office.Interop.Excel 12.0.0.0之后才有这函数
                //lstrTemp = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xls" + (lobjExcelWorkBook.HasVBProject ? 'm' : 'x');
                //lstrTemp = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xls";
                //lobjExcelWorkBook.SaveAs(lstrTemp, Microsoft.Office.Interop.Excel.XlFileFormat.xlExcel4Workbook, Type.Missing, Type.Missing, Type.Missing, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing,
                //  false, Type.Missing, Type.Missing, Type.Missing);
                //输出为PDF 第一个选项指定转出为PDF,还可以指定为XPS格式
                lobjExcelWorkBook.ExportAsFixedFormat(
                    Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, //Type 必需 XlFixedFormatType 要导出的文件格式类型。 
                    strOutFile, //FileName 可选 Variant 要保存的文件的文件名。 可以包括完整路径，否则 Microsoft Excel 会将文件保存在当前文件夹中。 
                    Microsoft.Office.Interop.Excel.XlFixedFormatQuality.xlQualityStandard, //Quality 可选 Variant 可选**XlFixedFormatQuality**。 指定已发布文件的质量。 
                    Type.Missing, //IncludeDocProperties 可选 Variant 如果为 True , 则包括文档属性;否则为 False。 
                    true, //IgnorePrintAreas 可选 Variant 如果为 True, 则忽略在发布时设置的任何打印区域;否则为 False。 
                    Type.Missing, //From 可选 Variant 要开始发布的页码。 如果省略此参数，则从头开始发布。 
                    Type.Missing, //To 可选 Variant 发布的终止页码。 如果省略此参数，则发布至最后一页。 
                    false, //OpenAfterPublish 可选 Variant 如果在查看器发布文件后将其显示在查看器中, 则为 True; 否则为否则为 False。 
                    Type.Missing //FixedFormatExtClassPtr 可选 Variant 指向 FixedFormatExt 类的指针。 
                    );
                lobjExcelWorkBook.Close(false);
                lobjExcelWorkBook = null;
                //lobjExcelWorkBooks.Close();
                //lobjExcelApp.Quit();
            }
            catch (Exception ex)
            {
                //其他日志操作；
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                if (lobjExcelWorkBook != null)
                {
                    lobjExcelWorkBook.Close(false, Type.Missing, Type.Missing);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(lobjExcelWorkBook);
                    lobjExcelWorkBook = null;
                }
                //if (lobjExcelWorkBooks != null)
                //{
                //    lobjExcelWorkBooks.Close();
                //    Marshal.ReleaseComObject(lobjExcelWorkBooks);
                //    lobjExcelWorkBooks = null;
                //}
                //if (lobjExcelApp != null)
                //{
                //    lobjExcelApp.Quit();
                //    Marshal.ReleaseComObject(lobjExcelApp);
                //    lobjExcelApp = null;
                //}
                //主动激活垃圾回收器，主要是避免超大批量转文档时，内存占用过多，而垃圾回收器并不是时刻都在运行！
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return true;
        }
        /// <summary>
        /// 将excel文档转换成PDF格式
        /// </summary>
        /// <param name="sourcePath">源文件路径</param>
        /// <param name="targetPath">目标文件路径</param>
        /// <returns></returns>
        //public static bool ExcelToPDF(string sourcePath, string targetPath)
        //{
        //    bool result;
        //    Microsoft.Office.Interop.Excel.XlFixedFormatType targetType = Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF; //PDF格式
        //    object missing = Type.Missing;
        //    Microsoft.Office.Interop.Excel.Application application = null;
        //    Microsoft.Office.Interop.Excel.Workbook workBook = null;
        //    try
        //    {
        //        application = new Microsoft.Office.Interop.Excel.Application();
        //        object target = targetPath;
        //        object type = targetType;
        //        workBook = application.Workbooks.Open(sourcePath, missing, missing, missing, missing, missing,
        //                missing, missing, missing, missing, missing, missing, missing, missing, missing);
        //        if (workBook != null)
        //        {

        //            Microsoft.Office.Interop.Excel.Sheets msheets = workBook.Worksheets;
        //            foreach (Microsoft.Office.Interop.Excel.Worksheet sheet in msheets)
        //            {
        //                WorkSheetPageSet(sheet);
        //            }
        //            workBook.ExportAsFixedFormat(targetType, target, Microsoft.Office.Interop.Excel.XlFixedFormatQuality.xlQualityStandard, true, false, missing, missing, missing, missing);
        //            result = true;
        //        }
        //        else
        //            result = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        result = false;
        //    }
        //    finally
        //    {
        //        if (workBook != null)
        //        {
        //            workBook.Close(true, missing, missing);
        //            workBook = null;
        //        }
        //        if (application != null)
        //        {
        //            application.Quit();
        //            application = null;
        //        }
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //    }
        //    return result;
        //}
        ///// <summary>
        /// 1.Clear CircleReference
        /// 2.Set Page to Fit Wide
        /// 3.Set Column Text fit
        /// </summary>
        /// <param name="app"></param>
        /// <param name="ws"></param>
        //private static void WorkSheetPageSet(Microsoft.Office.Interop.Excel.Worksheet ws)
        //{
        //    ClearCircleReference(ws);
        //    SetPagetoFitWide(ws);
        //    SetColumnFit(ws);
        //}
        ///// <summary>
        /// Clear CircleReference
        /// </summary>
        /// <param name="sheet">Worksheet object</param>
        //private static void ClearCircleReference(Microsoft.Office.Interop.Excel.Worksheet sheet)
        //{
        //    Microsoft.Office.Interop.Excel.Range range = sheet.CircularReference;
        //    while (range != null)
        //    {
        //        range.Clear();
        //        range = sheet.CircularReference;
        //    }
        //}
        ///// <summary>
        /// Set Page to Fit Wide
        /// </summary>
        /// <param name="ws">Worksheet object</param>
        //private static void SetPagetoFitWide(Microsoft.Office.Interop.Excel.Worksheet ws)
        //{
        //    ws.PageSetup.Zoom = false;
        //    ws.PageSetup.FitToPagesWide = 1;
        //    ws.PageSetup.FitToPagesTall = false;
        //}
        ///// <summary>
        /// Set Column Text fit
        /// </summary>
        /// <param name="sheet"></param>
        //private static void SetColumnFit(Microsoft.Office.Interop.Excel.Worksheet sheet)
        //{
        //    char column = 'B';
        //    for (int i = 0; i < 25; i++)
        //    {
        //        Microsoft.Office.Interop.Excel.Range range = sheet.get_Range(String.Format("{0}1", column.ToString()),
        //         String.Format("{0}1", column.ToString()));
        //        if (range != null)
        //        {
        //            range.EntireColumn.AutoFit();
        //        }
        //        column++;
        //    }
        //}

        #region win32 API
        [DllImport("user32", EntryPoint = "HideCaret")]
        public static extern bool HideCaret(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }
        public enum falshType : uint
        {
            FLASHW_STOP = 0,    //停止闪烁
            FALSHW_CAPTION = 1,  //只闪烁标题
            FLASHW_TRAY = 2,   //只闪烁任务栏
            FLASHW_ALL = 3,     //标题和任务栏同时闪烁
            FLASHW_PARAM1 = 4,
            FLASHW_PARAM2 = 12,
            FLASHW_TIMER = FLASHW_TRAY | FLASHW_PARAM1,   //无条件闪烁任务栏直到发送停止标志，停止后高亮
            FLASHW_TIMERNOFG = FLASHW_TRAY | FLASHW_PARAM2  //未激活时闪烁任务栏直到发送停止标志或者窗体被激活，停止后高亮
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }
        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;

        [DllImport("shell32.dll")]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);
        public static void ShowFileProperties(string filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            ShellExecuteEx(ref info);
        }
        #endregion

        #region 读取文档内容
        public static bool ReadWordText(Microsoft.Office.Interop.Word._Application app, string filePath, out string docText)
        {
            docText = string.Empty;
            try
            {
                Microsoft.Office.Interop.Word._Document doc = null;//实例化一个新的word文档
                object unknow = Type.Missing;
                object paramSourceDocPath = filePath;
                object ConfirmConversions = false;
                object readOnly = true;
                object AddToRecentFiles = false;
                object Visible = false;
                object Revert = false;
                object Format = Microsoft.Office.Interop.Word.WdOpenFormat.wdOpenFormatAuto;
                object OpenAndRepair = false;
                object NoEncodingDialog = true;
                doc = app.Documents.Open(ref paramSourceDocPath,
                ref ConfirmConversions, ref readOnly, ref AddToRecentFiles, ref unknow, ref unknow,
                ref Revert, ref unknow, ref unknow, ref Format, ref unknow,
                ref Visible, ref OpenAndRepair, ref NoEncodingDialog, ref unknow, ref unknow);

                if (doc != null)
                {
                    docText = doc.Content.Text + " ";//将全篇内容存入字符串中,最后补一个空格，表示文档能够打开
                    doc.Close(ref unknow, ref unknow, ref unknow);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }
        public static bool ReadPdfText(string filepath, out string docText)
        {
            docText = string.Empty;
            try
            {
                string pdffilename = filepath;
                PdfReader pdfReader = new PdfReader(pdffilename);
                int numberOfPages = pdfReader.NumberOfPages;
                StringBuilder sbDocText = new StringBuilder();
                for (int i = 1; i <= numberOfPages; ++i)
                {
                    iTextSharp.text.pdf.parser.ITextExtractionStrategy strategy = new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy();
                    sbDocText.Append(iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(pdfReader, i, strategy));
                }
                sbDocText.Append(" ");
                pdfReader.Close();
                docText = sbDocText.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        //public static bool ReadExcelText(Microsoft.Office.Interop.Excel.Application app, string filePath, out string docText)
        //{
        //    docText = string.Empty;
        //    if (app == null || string.IsNullOrEmpty(filePath))
        //    {
        //        return false;
        //    }
        //    try
        //    {
        //        string strConvertTempFile = Path.Combine(System.Windows.Forms.Application.StartupPath, "convertExcel2Pdf.temp.pdf");
        //        if (!Common.ConverterExcelToPdf(app, filePath, strConvertTempFile))
        //        {
        //            return false;
        //        }
        //        return ReadPdfText(strConvertTempFile, out docText);
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    return false;
        //}

        public static bool ReadExcelText(string filepath, out string docText)
        {
            docText = string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                List<string> lSheets = GetExcelTableName(filepath);
                foreach (string strSheet in lSheets)
                {
                    stringBuilder.Append(strSheet).Append("\n\n");
                    System.Data.DataTable dt = ExcelToDataTable(filepath, strSheet);
                    if (dt == null)
                    {
                        continue;
                    }
                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        for (int index = 0; index < dt.Columns.Count; ++index)
                        {
                            stringBuilder.Append(row[index].ToString()).Append("\t");
                        }
                        stringBuilder.Append("\n");
                    }
                }
                stringBuilder.Append(" ");
                docText = stringBuilder.ToString();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }


        #endregion

        /// <summary>
        /// C#中获取Excel文件的表名 
        /// </summary>
        /// <param name="excelFileName">路径名</param>
        /// <returns></returns>
        private static List<string> GetExcelTableName(string pathName)
        {
            List<string> tableName = new List<string>();
            if (File.Exists(pathName))
            {
                string strConn = string.Empty;
                FileInfo file = new FileInfo(pathName);
                string extension = file.Extension;
                switch (extension)
                {
                    case ".xls":
                        strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=No;IMEX=1;'";
                        //strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathName + ";Extended Properties='Excel 12.0;HDR=No;IMEX=1;'";
                        break;
                    case ".xlsx":
                        strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathName + ";Extended Properties='Excel 12.0;HDR=No;IMEX=1;'";
                        break;
                    default:
                        strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=No;IMEX=1;'";
                        break;
                }
                using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(strConn))
                {
                    conn.Open();
                    System.Data.DataTable dt = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        string strSheetTableName = row["TABLE_NAME"].ToString();
                        //过滤无效SheetName   
                        if (strSheetTableName.Contains("$") && strSheetTableName.Replace("'", "").EndsWith("$"))
                        {
                            strSheetTableName = strSheetTableName.Replace("'", "");   //可能会有 '1X$' 出现
                            strSheetTableName = strSheetTableName.Substring(0, strSheetTableName.Length - 1);
                            tableName.Add(strSheetTableName);
                        }
                    }
                }
            }
            return tableName;
        }

        //用OLEDB通过设置连接字符串可以像读取sqlserver一样将excel中的数据读取出来，但是excel2003和excel2007/2010的连接字符串是不同的
        /// <summary>
        /// 把数据从Excel装载到DataTable
        /// </summary>
        /// <param name="pathName">带路径的Excel文件名</param>
        /// <param name="sheetName">工作表名</param>
        /// <param name="tbContainer">将数据存入的DataTable</param>
        /// <returns></returns>
        private static System.Data.DataTable ExcelToDataTable(string pathName, string sheetName)
        {
            System.Data.DataTable tbContainer = new System.Data.DataTable();
            string strConn = string.Empty;
            if (string.IsNullOrEmpty(sheetName)) { sheetName = "Sheet1"; }
            FileInfo file = new FileInfo(pathName);
            if (!file.Exists) { throw new Exception("文件不存在"); }
            string extension = file.Extension;
            switch (extension)
            {
                case ".xls":
                    strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=No;IMEX=1;'";
                    break;
                case ".xlsx":
                    strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathName + ";Extended Properties='Excel 12.0;HDR=No;IMEX=1;'";
                    break;
                default:
                    strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=No;IMEX=1;'";
                    break;
            }
            //链接Excel
            System.Data.OleDb.OleDbConnection cnnxls = new System.Data.OleDb.OleDbConnection(strConn);
            //读取Excel里面有 表Sheet1
            System.Data.OleDb.OleDbDataAdapter oda = new System.Data.OleDb.OleDbDataAdapter(string.Format("select * from [{0}$]", sheetName), cnnxls);
            System.Data.DataSet ds = new System.Data.DataSet();
            //将Excel里面有表内容装载到内存表中！
            oda.Fill(tbContainer);
            return tbContainer;
            //这里需要注意的地方是，当文件的后缀名为.xlsx(excel2007/2010)时的连接字符串是"Provider=Microsoft.ACE.OLEDB.12.0;...."，注意中间红色部分不是"Jet"。
        }
    }
}
