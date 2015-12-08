using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class UserControls_Util_ucGridViewChoiceAll : System.Web.UI.UserControl
{
    /// <summary>
    /// 核取方塊的名稱
    /// </summary>
    public string CheckBoxName { get; set; }
    /// <summary>
    /// 設定Header字串
    /// </summary>
    public string HeaderText { set { cbxChoice.Text = value; } }

    /// <summary>
    /// 準備輸出的動作
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Page_PreRender(object sender, EventArgs e)
    {
        // 找出Gridview之中的CheckBox名稱
        GridView gv = (GridView)this.Parent.Parent.Parent.Parent;
        string strCheckBoxsName = "";
        for (int i = 0; i < gv.Rows.Count; i++)
        {
            CheckBox cbx = (CheckBox)gv.Rows[i].FindControl(this.CheckBoxName);
            strCheckBoxsName += cbx.ClientID + ",";
        }

        // 調整陣列值
        if (strCheckBoxsName != "")
            strCheckBoxsName = strCheckBoxsName.Substring(0, strCheckBoxsName.Length - 1);

        // 變更陣列值
        hidCheckBoxs.Value = strCheckBoxsName;
        string strClickScript = "document.getElementById('" + hidCheckBoxs.ClientID + "').value = '" + strCheckBoxsName + "';";
        ScriptManager.RegisterStartupScript(this, this.GetType(), this.ClientID, strClickScript, true);
        cbxChoice.Attributes.Add("onclick", "funChoiceAll" + this.ClientID + "(this);");

        // 註冊Script
        string strScript = "function funChoiceAll" + this.ClientID + "(obj){";
        strScript += " var strCheckBoxs = document.getElementById('" + hidCheckBoxs.ClientID + "').value.split(',');";
        strScript += " var i;";
        strScript += " for (i=0; i<strCheckBoxs.length; i++){ ";
        strScript += " document.getElementById(strCheckBoxs[i]).checked = obj.checked;}}";
        ScriptManager.RegisterStartupScript(this, this.GetType(), "fun" + this.ClientID, strScript, true);
    }
}
