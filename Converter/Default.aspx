<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<%@ Register src="UserControl/ucGridViewChoiceAll.ascx" tagname="ucGridViewChoiceAll" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="App_Themes/css/style.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:Panel ID="panel1"  runat="server" Width="100%">
            <asp:Table ID="Table1" runat="server" Width="100%">
                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="Left">
                        <asp:Image ID="imglogo" runat="server" ImageUrl="App_Themes/image/logo.jpg" />
                        <asp:Label ID="lblSystemTitle" runat="server" Text="字 幕 編 碼 轉 換" ForeColor="#003366" Font-Size="32px" Font-Bold="true" />
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow BackColor="#3399FF">
                    <asp:TableCell HorizontalAlign="Center">
                    </asp:TableCell>
                </asp:TableRow>
             </asp:Table>
             <p />
             <asp:Table ID="Table2" runat="server" Width="100%">
                <asp:TableRow>
                    <asp:TableCell Width="8%" HorizontalAlign = "Right">
                        <asp:Label ID="lblImportFile" runat="server" CssClass="tableLabel" Text="轉檔檔案：" />
                    </asp:TableCell>
                    <asp:TableCell Width="92%" HorizontalAlign = "Left">
                        <asp:FileUpload ID="fuFile" runat="server" Width="500px" /> 
                        <asp:Button ID="btnTransfer" runat="server" CssClass="Button" Text="單筆轉檔" OnClick="btnTransfer_Click" />
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow Visible ="false">
                    <asp:TableCell Width="8%" HorizontalAlign = "Right">
                        <asp:Label ID="lblImportDir" runat="server" CssClass="tableLabel" Text="匯入檔案目錄：" />
                    </asp:TableCell>
                    <asp:TableCell Width="92%" HorizontalAlign = "Left">
                        <asp:TextBox ID="txtImportDir" runat="server" Width="500px" />
                        <asp:Button ID="btnGetFile" runat="server" CssClass="Button" Text="取得檔案清單" OnClick="btnGetFile_Click" />
                        <asp:Button ID="btnBatchTransfer" runat="server" CssClass="Button" Visible="false" Text="批次轉檔" OnClick="btnBatchTransfer_Click" />
                    </asp:TableCell>
                </asp:TableRow>
             </asp:Table>
             <asp:HiddenField ID="hidFileName" runat="server" />             
        </asp:Panel>   
        
        <p />
        <asp:Panel ID="panel2" runat="server" Visible="false">
            <asp:GridView ID="gridFileList" runat="server" Width="100%"
                BackColor="White" BorderColor="#999999" BorderStyle="None" BorderWidth="1px" 
                CellPadding="3" GridLines="Vertical" Font-Size="13px"
                DataKeyNames=""
                OnRowDataBound="gridDataList_RowDataBound"
                OnPageIndexChanging="gridDataList_PageIndexChanging" 
                OnPageIndexChanged="gridDataList_PageIndexChanged"
                AllowPaging="true" AutoGenerateColumns="false"
                PageSize="10">

                <AlternatingRowStyle BackColor="#DCDCDC" />

                <Columns>
                    <asp:TemplateField HeaderText="檔案清單" ItemStyle-Width="100%">
                        <HeaderTemplate>
                            <uc1:ucGridViewChoiceAll ID="ucGridViewChoiceAll1" runat="server" CheckBoxName="checkitem" HeaderText="檔案清單"  />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:Label ID="lblID" runat="server" Text='<%# Bind("id") %>' Visible="false" />
                            <asp:CheckBox ID="checkitem" runat="server" Checked="true" />
                            <asp:Label ID="lblName" runat="server" Text='<%# Bind("Name") %>' />
                            <asp:Label ID="lblFullName" runat="server" Text='<%# Bind("FullName") %>' Visible="false" />
                        </ItemTemplate>
                        <HeaderStyle HorizontalAlign="Left" VerticalAlign="Middle" />
                        <ItemStyle HorizontalAlign="Left" VerticalAlign="Middle" />
                    </asp:TemplateField>
                </Columns>

                <EmptyDataTemplate>
                    <table style="width:100%">
                        <tr >
                            <td width="100%" class="gridHead">檔案清單</td>
                        </tr>
                        <tr>
                            <td width="100%" class="gridText">無資料</td>
                        </tr>
                    </table>
                </EmptyDataTemplate>
                
                <HeaderStyle BackColor="#000084" Font-Bold="True" ForeColor="White" HorizontalAlign="Left" />            
                <PagerSettings FirstPageImageUrl="App_Themes/image/arrow_top.gif" 
                    LastPageImageUrl="App_Themes/image/arrow_down.gif" 
                    Mode="NextPreviousFirstLast" 
                    NextPageImageUrl="App_Themes/image/arrow_next.gif" 
                    PreviousPageImageUrl="App_Themes/image/arrow_back.gif" />
                <PagerStyle HorizontalAlign="Center" />
                <RowStyle BackColor="#EEEEEE" ForeColor="Black" />
            </asp:GridView>           
        </asp:Panel>  
        
    <div>
    
    </div>
    </form>
</body>
</html>
