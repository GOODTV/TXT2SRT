using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using log4net;

public partial class _Default : System.Web.UI.Page
{
    public ILog logger = LogManager.GetLogger("RollingFileAppender");

    /// <summary>
    /// Page_Load
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    /// <history>
    /// 1.Tanya Wu, 2012/9/20, Create
    /// </history>
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            //設定初始值 ex.D:\Export\20120922
            this.txtImportDir.Text = "D:\\Import\\" + DateTime.Today.Year + DateTime.Today.Month.ToString().PadLeft(2, '0') + DateTime.Today.Day.ToString().PadLeft(2, '0');

            //grid初始值為空
            this.gridFileList.DataSource = null;
            this.gridFileList.DataBind();
        }
    }

    #region Button
    /// <summary>
    /// 單筆轉檔按鈕
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    /// <history>
    /// 1.Tanya Wu, 2012/9/21, Create
    /// </history>
    protected void btnTransfer_Click(object sender, EventArgs e)
    {
        string ImportFileName = string.Empty;
        string ImportSource = string.Empty;
        string js = string.Empty;
        string appPath = Request.PhysicalApplicationPath;
        string saveDir = "\\Files\\txt\\";
        string exportDir = "\\Files\\srt\\";
        
        try
        {
            //檢查匯入檔案是否已選取
            if (fuFile.HasFile)
            {
                ImportFileName = this.fuFile.FileName;

                if (Path.GetExtension(ImportFileName) == ".txt")
                {
                    ImportSource = appPath + saveDir + ImportFileName;
                    //文件存檔
                    fuFile.SaveAs(ImportSource);

                    //取得預匯出檔案名稱
                    this.hidFileName.Value = GetFileName(ImportFileName);

                    //建立匯出檔案
                    if (this.CreateFile())
                    {
                        //匯出檔
                        string ExportFile = appPath + exportDir + "\\" + this.hidFileName.Value;                        
                        StreamWriter sw = new StreamWriter(ExportFile,true,System.Text.Encoding.UTF8);

                        //讀取匯入檔案
                        IList<string> ilistImportData = File.ReadAllLines(ImportSource);

                        //資料處理並存入*.srt
                        int seqno = 0; //匯出資料的seqno
                        for (int i = 0; i < ilistImportData.Count-1; i++)
                        {                            
                            string strData = ilistImportData[i].ToString();
                            string strData_next = ilistImportData[i+1].ToString();
                            if (strData.Split(' ').Length > 1)
                            {
                                seqno++;
                                if (seqno != 1)
                                    sw.Write("\n"); //每筆紀錄與前段空一行(除第一筆) 

                                this.Process(sw, seqno, strData, strData_next);
                            }
                        }
                        
                        //關閉StreamWriter
                        sw.Close();
                        sw.Dispose();

                        //轉檔完成：開啟檔案
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "text/plain";
                        Response.AddHeader("content-disposition", "attachment;filename=" + this.hidFileName.Value);
                        Response.WriteFile(ExportFile);
                        Response.Flush();
                        Response.End();
                    }
                    else
                    {
                        this.AlertMessage("匯出檔建立失敗!");
                    }
                    
                }
                else
                {
                    this.AlertMessage("檔案格式必需為*.txt");
                }
            }
            else
            {
                this.AlertMessage("請選取檔案!");
            }
        }
        catch (Exception ex)
        {
            this.AlertMessage("轉檔失敗!");
            this.AlertMessage(ex.Message.ToString());

            this.logger.Error(ex.Message, ex);
        }
    }

    /// <summary>
    /// 取得檔案清單按鈕
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    /// <history>
    /// 1.Tanya Wu, 2012/9/21, Create
    /// </history>
    protected void btnGetFile_Click(object sender, EventArgs e)
    {
        string ImportDir = string.Empty;
        string js = string.Empty;

        try
        {
            ImportDir = this.txtImportDir.Text;
            
            //取得目錄內檔案清單
            DirectoryInfo dirinfo = new DirectoryInfo(ImportDir);
            FileInfo[] fileList = dirinfo.GetFiles();

            if (fileList.Length > 0)
            {
                //轉為DataTable
                DataTable dt = this.ConvertToDataTable(fileList);
                //grid資料繫結
                this.gridFileList.DataSource = dt;
                this.gridFileList.DataBind();

                //顯示控制項
                this.btnBatchTransfer.Visible = true;
            }
            else
            {
                this.AlertMessage("資料夾無檔案!");
            }
        }
        catch (Exception ex)
        {
            this.AlertMessage("讀取失敗!");

            this.logger.Error(ex.Message, ex);
        }
    }

    /// <summary>
    /// 批次轉檔按鈕
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    /// <history>
    /// 1.Tanya Wu, 2012/9/24, Create
    /// </history>
    protected void btnBatchTransfer_Click(object sender, EventArgs e)
    {
        string js = string.Empty;
        string ImportFile = string.Empty;
        string ExportFile = string.Empty;
        string ErrFileList = string.Empty;
        int CheckCount = 0;

        try
        {
            foreach (GridViewRow rowItem in this.gridFileList.Rows)
            {
                Label id = (Label)(rowItem.Cells[0].FindControl("lblID"));
                CheckBox chk = (CheckBox)(rowItem.Cells[0].FindControl("checkitem"));
                Label Name = (Label)(rowItem.Cells[0].FindControl("lblName"));
                Label FullName = (Label)(rowItem.Cells[0].FindControl("lblFullName"));

                if (chk.Checked)
                {
                    CheckCount++;

                    //取得匯入檔完整路徑及名稱
                    ImportFile = FullName.Text;
                    //檢查檔案是否存在
                    if (File.Exists(ImportFile))
                    {
                        //指定匯出檔名稱
                        hidFileName.Value = this.GetFileName(Name.Text);

                        //建立匯出檔
                        if (this.CreateFile())
                        {
                            //匯出檔
                            //ExportFile = this.txtExportDir.Text + "\\" + this.hidFileName.Value;
                            StreamWriter sw = new StreamWriter(ExportFile);

                            //讀取匯入檔案
                            IList<string> ilistImportData = File.ReadAllLines(ImportFile);

                            //資料處理並存入*.srt
                            int seqno = 0; //匯出資料的seqno
                            for (int i = 0; i < ilistImportData.Count; i++)
                            {
                                seqno++;
                                string strData = ilistImportData[i].ToString();
                                string strData_next = ilistImportData[i+1].ToString();
                                this.Process(sw, seqno, strData, strData_next);
                            }

                            //關閉StreamWriter
                            sw.Close();
                            sw.Dispose();

                        }
                        else
                            //匯出檔建立失敗，加入轉檔失敗清單
                            ErrFileList = ErrFileList + Name + ",";
                    }
                    else
                        //檔案不存在，加入轉檔失敗清單
                        ErrFileList = ErrFileList + Name + ",";
                }
            }

            //顯示執行結果訊息
            if (!string.IsNullOrEmpty(ErrFileList))
            {
                this.AlertMessage("轉檔失敗清單：" + ErrFileList.Substring(0, ErrFileList.Length - 2));
            }
            else if (CheckCount == 0)
            {
                this.AlertMessage("請勾選檔案!");
            }
            else
            {
                this.AlertMessage("轉檔完成!");
            }
        }
        catch (Exception ex)
        {
            this.AlertMessage("轉檔失敗!");

            this.logger.Error(ex.Message, ex);
        }

    }
    
    #endregion

    #region Grid
    /// <summary>
    /// Grid RowDataBound
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">GridViewRowEventArgs.</param>
    /// <history>
    /// 1.Tanya Wu, 2012/9/20, Create
    /// </history>
    protected void gridDataList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        //光棒效果:滑鼠經過變色
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Attributes.Add("OnMouseover", "this.style.backgroundColor='#008A8C';this.style.color='White'");
            if (e.Row.RowIndex % 2 == 0)
            {
                //單數列
                e.Row.Attributes.Add("OnMouseout", "this.style.backgroundColor='#EEEEEE';this.style.color='Black'");
            }
            else
            {
                //雙數列
                e.Row.Attributes.Add("OnMouseout", "this.style.backgroundColor='#DCDCDC';this.style.color='Black'");
            }
        }
    }

    /// <summary>
    /// Grid PageIndexChanging
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">GridViewPageEventArgs.</param>
    /// <history>
    /// 1.Tanya Wu, 2012/9/20, Create
    /// </history>
    protected void gridDataList_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.gridFileList.PageIndex = e.NewPageIndex;
    }

    /// <summary>
    /// Grid PageIndexChanged
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">EventArgs.</param>
    /// <history>
    /// 1.Tanya Wu, 2012/9/20, Create
    /// </history>
    protected void gridDataList_PageIndexChanged(object sender, EventArgs e)
    {
        string ImportDir = this.txtImportDir.Text;
        
        //取得目錄內檔案清單
        DirectoryInfo dirinfo = new DirectoryInfo(ImportDir);
        FileInfo[] fileList = dirinfo.GetFiles();
        //轉為DataTable
        DataTable dt = this.ConvertToDataTable(fileList);
        //grid資料繫結
        this.gridFileList.DataSource = dt;
        this.gridFileList.DataBind();
    }
    #endregion

    #region Other Method
    /// <summary>
    /// 建立指定資料夾與檔案
    /// </summary>
    /// <history>
    /// 1.Tanya Wu, 2012/9/20, Create
    /// </history>
    private bool CreateFile()
    {
        string appPath = Request.PhysicalApplicationPath;
        string exportDir = "\\Files\\srt\\";
        string strExportFullDir = appPath + exportDir;
        StreamWriter sw;

        try
        {
            //if (!Directory.Exists(strExportFullDir))
            //{
            //    //建立資料夾
            //    Directory.CreateDirectory(strExportFullDir);
            //}

            //檢查匯出檔是否存在
            string ExportFile = strExportFullDir + "\\" + this.hidFileName.Value;
            if (!File.Exists(ExportFile))
            {
                //建立匯出檔
                sw = File.CreateText(ExportFile);
            }
            else
            {
                //先刪除再重新建立匯出檔
                File.Delete(ExportFile);
                sw = File.CreateText(ExportFile);
            }

            //關閉StreamWriter
            sw.Close();
            sw.Dispose();

            return true;
        }
        catch (Exception ex)
        {
            return false;

            this.logger.Error(ex.Message, ex);
        }
    }

    /// <summary>
    /// 取得檔案名稱
    /// </summary>
    /// <param name="FileName">string</param>
    /// <returns>string: the file full name e.g. abc.srt</returns>
    /// <history>
    /// 1.Tanya Wu, 2012/9/20, Create
    /// </history>
    private string GetFileName(string FileName)
    {
        string[] split = FileName.Split('\\');
        string UploadFileName = split[split.Length - 1];
        return UploadFileName.Replace(".txt",".srt");
    }

    /// <summary>
    /// 轉換成 DataTable
    /// </summary>
    /// <param name="list"></param>
    /// <returns>DataTable</returns>
    /// <history>
    /// 1.Tanya Wu, 2012/9/24, Create
    /// </history>
    public DataTable ConvertToDataTable(FileInfo[] listFile)
    {
        DataTable dt = new DataTable("DataList");

        // Create a DataColumn and set various properties. 
        DataColumn column1 = new DataColumn();
        column1.DataType = System.Type.GetType("System.String");
        column1.AllowDBNull = false;
        column1.Caption = "id";
        column1.ColumnName = "id";
        dt.Columns.Add(column1);

        DataColumn column2 = new DataColumn();
        column2.DataType = System.Type.GetType("System.String");
        column2.AllowDBNull = false;
        column2.Caption = "Name";
        column2.ColumnName = "Name";
        dt.Columns.Add(column2);

        DataColumn column3 = new DataColumn();
        column3.DataType = System.Type.GetType("System.String");
        column3.AllowDBNull = false;
        column3.Caption = "FullName";
        column3.ColumnName = "FullName";
        dt.Columns.Add(column3);

        // Add rows and set values. 
        DataRow row;
        for (int i = 0; i < listFile.Length; i++)
        {
            row = dt.NewRow();
            row["id"] = (i + 1).ToString().PadLeft(3, '0');
            row["Name"] = listFile[i].Name;
            row["FullName"] = listFile[i].FullName;

            // Be sure to add the new row to the 
            // DataRowCollection. 
            dt.Rows.Add(row);
        }
        return dt;
    }

    /// <summary>
    /// Process
    /// </summary>
    /// <param name="strData">string</param>
    /// <history>
    /// 1.Tanya Wu, 2012/9/20, Create
    /// </history>
    private void Process(StreamWriter sw, int seqno, string strData, string strData_next)
    {
        string[] startData = strData.Split(' ');
        string[] nextData = strData_next.Split(' ');
        //string[] startTime = a[0].Split(';');
        //string[] endTime = a[1].Split(';');
        string[] startTime = startData[0].Split(':');
        string[] endTime = nextData[0].Split(':');

        //毫秒轉換
        string startMS = this.ConvertMS(startTime[3]).ToString().PadLeft(3, '0');
        string endMS = string.Empty;
        string Time = string.Empty;

        if (this.ConvertMS(endTime[3]) >= 100)
        {
            endMS = (this.ConvertMS(endTime[3]) - 100).ToString().PadLeft(3,'0');

            //時間組合字串
            Time = startTime[0] + ":" + startTime[1] + ":" + startTime[2] + "," + startMS;
            Time += " --> " + endTime[0] + ":" + endTime[1] + ":" + endTime[2] + "," + endMS;
        }
        else
        {
            string[] New_endTime = Convert.ToDateTime(endTime[0] + ":" + endTime[1] + ":" + endTime[2]).AddSeconds(-1).ToString("HH:mm:ss").Split(':');

            endMS = (this.ConvertMS(endTime[3]) + 900).ToString().PadLeft(3, '0');
            //endMS = this.ConvertMS(endTime[3]).ToString().PadLeft(3,'0');

            //時間組合字串
            Time = startTime[0] + ":" + startTime[1] + ":" + startTime[2] + "," + startMS;
            Time += " --> " + New_endTime[0] + ":" + New_endTime[1] + ":" + New_endTime[2] + "," + endMS;
            //Time += " --> " + endTime[0] + ":" + endTime[1] + ":" + (Convert.ToInt32(endTime[2]) - 1).ToString().PadLeft(2,'0') + "," + endMS;
        }

        //insert *.srt
        //-Seqno
        //sw.WriteLine(seqno.ToString());
        sw.Write(seqno.ToString() + "\n");
        //-Time
        //sw.WriteLine(Time);
        sw.Write(Time + "\n");
        //-文字
        if (startData.Length > 1)
        {
            for (int i = 1; i < startData.Length; i++)
            {
                if (startData[i].IndexOf("//") > -1)
                {                                     
                    string[] b = startData[i].Split('/');
                    for (int j = 0; j < b.Length; j++)
                    {
                        if (!string.IsNullOrEmpty(b[j]))
                        {
                            if (j == b.Length - 1)
                                sw.Write(b[j]);
                            else
                                //sw.WriteLine(b[j]);
                                sw.Write(b[j] + "\n");
                        }
                    }

                }
                else
                    if (i == startData.Length - 1)
                        sw.Write(startData[i]);
                    else
                        sw.Write(startData[i] + " ");
            }            
        }
        //else
        //    sw.Write(""); //無文字空檔

        sw.Write("\n"); //每筆紀錄文字結束換行                   
        
    }

    /// <summary>
    /// ConvertMS
    /// </summary>
    /// <param name="strData">string</param>
    /// <history>
    /// 1.Tanya Wu, 2012/9/20, Create
    /// </history>
    private int ConvertMS(string strTimeMS)
    {
        double b = Convert.ToDouble(strTimeMS);
        double MS = (b / 29.97) * 1000;

        return Convert.ToInt16(MS);
        //string[] c = strTimeHMS.Split(':');
        //double c1 = (Convert.ToDouble(c[0]) * 60 * 60) + (Convert.ToDouble(c[1]) * 60) + Convert.ToDouble(c[2]);

        //MS = Math.Round(MS + c1, 0, MidpointRounding.AwayFromZero);

        //string strMS = MS.ToString();

        //if (strMS.Length > 3)
        //    return strMS.Substring(strMS.Length - 3, 3);
        //else
        //    return strMS;
    }

    /// <summary>
    /// alert訊息
    /// </summary>
    /// <param name="message">要alert的訊息</param>
    public void AlertMessage(string message)
    {
        string js = "setTimeout(function() { alert('" + EscapeStringForJS(message) + "'); alertFlag = false;},0);";
        //string js = "alert('" + EscapeStringForJS(message) + "');";
        RegisterStartupJS(message, js);
    }

    /// <summary>
    /// Replace characters for Javscript string literals
    /// 放訊息內文，不要連語法一起進去編碼，例如alert('"+ abc +"')";，應該只編碼abc
    /// </summary>
    /// <param name="text">raw string</param>
    /// <returns>escaped string</returns>
    public static string EscapeStringForJS(string s)
    {
        //REF: http://www.javascriptkit.com/jsref/escapesequence.shtml

        return s.Replace(@"\", @"\\")
                .Replace("\b", @"\b")
                .Replace("\f", @"\f")
                .Replace("\n", @"\n")
                .Replace("\0", @"\0")
                .Replace("\r", @"\r")
                .Replace("\t", @"\t")
                .Replace("\v", @"\v")
                .Replace("'", @"\'")
                .Replace(@"""", @"\""");
    }

    /// <summary>
    /// Registers the startup JS.
    /// </summary>
    /// <param name="RegisterName">Name of the register.</param>
    /// <param name="myJavascript">My javascript.</param>
    public void RegisterStartupJS(string RegisterName, string myJavascript)
    {
        string wholeJS = "<SCRIPT language=\"JavaScript\"  type=\"text/javascript\" >" + myJavascript + "</SCRIPT>";
        //wholeJS=EscapeStringForJS(wholeJS);
        if (ExistSM())
        { ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), RegisterName, wholeJS, false); }
        else
        { Page.ClientScript.RegisterStartupScript(Page.GetType(), RegisterName, wholeJS); }

    }

    /// <summary>
    /// 檢查頁面上是否存在ScriptManager
    /// </summary>
    /// <returns></returns>
    private bool ExistSM()
    {
        return (ScriptManager.GetCurrent(this.Page) != null);
    }
    #endregion

}