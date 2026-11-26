using MVC;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    [Authorize]
    public class StockTransfersController : Controller
    {
        // GET: StockTransfers
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Stock Transfers"
            };
            if (html.HasPermission("Add Stock Transfers"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Stock Transfers"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Stock Transfers"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add Stock Transfers";
            html.AddURL = "StockTransfers/StockTransfers";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Transfer Date:",
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Transfer No",
                SQL = "select distinct top 10 StockTransfersNumber as TransferNo from dbo.TblStockIn where ItemType = 'Stock Transfers' and lower(StockTransfersNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by StockTransfersNumber desc",
                RadioValue = EncryAndDecry.Encryp("TransferNo"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Transfer Date", FieldName = "TransferDate", TextAlign = "center" },
                new GridField() { Header = "Transfer No", FieldName = "TransferNo", TextAlign = "center" },
                new GridField() { Header = "From Warehouse", FieldName = "FromWarehouseName"},
                new GridField() { Header = "To Warehouse", FieldName = "ToWarehouseName"},
                new GridField() { Header = "Created By", FieldName = "CreatedBy"},
                new GridField() { Header = "Description", FieldName = "Description"},
                new GridField() { Header = "Action", FieldName = "StockInID", TextAlign = "Center",  IsModifyField = true,
                    DeleteWithTableName="TblStockIn, TblStockInDetail"},
            };
            var kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_SHOW_STOCK_TRANSFERS"
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        public ActionResult StockTransfers(string PrimaryFieldValue, string Type = "", string ItemID = "", string ItemName = "", string LocationID = "", string LocationName = "")
        {
            HTML html = new HTML
            {
                SaveChangeAndClose = true,
                FormID = "StockTransfers",
                PrimaryFieldValue = PrimaryFieldValue
            };
            ViewBag.ItemID = ItemID;
            ViewBag.ItemName = ItemName;
            if (Type != "")
                html.SaveChangeAndClose = true;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Stock Transfers",
                    TableName="TblStockIn",
                    GetIDENTITYByFiled="StockInID",
                    PrimaryFieldName="StockInID",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="PreparedUserID", SetValue ="$(\"#PreparedBy_PreparedUserID\").val()"
                        },
                        new DBField()
                        {
                            DBFieldName="CheckByID", SetValue ="$(\"#CheckBy_CheckByID\").val()"
                        },
                        new DBField()
                        {
                            DBFieldName="ReceivedByID", SetValue ="$(\"#ReceivedBy_ReceivedID\").val()"
                        },
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Stock Transfers#:", ID="StockTransfersNumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="StockTransfersNumber", IsReadOnly = true,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'Stock Transfers' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ",'"+html.DomainID+"'",
                            CallBackToControlID="StockTransfersNumber, PrefixStockTransfersNumber", UpdatePrefixModule="Stock Transfers", Prefix="PrefixStockTransfersNumber", SetPrefixToDbFieldName="StockTransfersNumber"
                        },
                        //new HtmlTextBox()
                        //{
                        //    Label="Reference Number:", ID="ReferenceNumber",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField = "ReferenceNo"
                        //},
                        new HtmlTextBox()
                        {
                            Label="Date:", ID="Date",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ReceiveDate", SingleDatePicker= true, Required = true
                        }
                    }
                },
                new FormData()
                {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Prepared By:", ID="PreparedBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PreparedBy",isTypeahead = true,
                            SQL="select u.UserLoginID PreparedUserID, u.FullName PreparedBy, u.Telephone PreparedPhone\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="PreparedUserID",
                            DisplayOnFields="PreparedPhone"
                        },
                        new HtmlTextBox()
                        {
                            Label="Prepared's Phone:", ID="PreparedPhone", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PreparedPhone", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Checked By:", ID="CheckBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CheckBy",isTypeahead = true,
                            SQL="select u.UserLoginID CheckByID, u.FullName CheckBy from dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="CheckByID",
                        },
                         new HtmlTextBox()
                        {
                            Label="Received By:", ID="ReceivedBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ReceivedBy",isTypeahead = true,
                            SQL="select u.UserLoginID ReceivedByID, u.FullName ReceivedBy from dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="ReceivedByID",
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="From Warehouse:", ID="FromWarehouse", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="FromWarehouse", isTypeahead = true,
                            //SQL="select uw.WID FromWarehouseID, (b.BranchName + ' - ' + uw.WarehouseName) FromWarehouse, uw.ContactName, uw.ContactPhone, uw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + case when uw.District <> '' then ', ' + uw.District end + case when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end Warehouse from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            SQL="select uw.WID FromWarehouseID, uw.WarehouseName FromWarehouse, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="FromWarehouseID", Value= LocationName, HiddenFieldValue = LocationID
                        },
                        new HtmlTextBox()
                        {
                            Label="To Warehouse:", ID="ToWarehouse", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Warehouse", isTypeahead = true,
                            //SQL="select uw.WID, (b.BranchName + ' - ' + uw.WarehouseName) Warehouse, uw.ContactName, uw.ContactPhone, uw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + case when uw.District <> '' then ', ' + uw.District end + case when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end Warehouse from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            SQL="select uw.WID, uw.WarehouseName Warehouse, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="WID",
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Description:", ID="Description",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Description"
                        },
                        new HtmlTextBox()
                        {
                            Label="ItemType", ID="ItemType",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ItemType", Hide = true, Value="Stock Transfers"
                        },
						//new HtmlTextBox()
						//{
						//	Label="StatusID", ID="StatusID",
						//	Class="input2 CustInfo", IsCustomForm = true,
						//	DBField="StatusID", Hide = true, Value="83"//Stock Transfers
      //                  },
					}
                },
                new  FormData() {
                    TableName="TblStockInDetail",
                    GetIDENTITYByFiled="StockInID",
                    PrimaryFieldName="StockInID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            SQL=@"select i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end) ItemName,
sd.Reason, sd.StockTransfers, sd.CostPrice, sd.UnitPrice, sd.UomID, sd.UM, sum(isnull(sd.QTY, 0)) QTY, 
sum(isnull(sd.QTYOUT, 0)) QTYOUT, sd.ItemID, sd.ClassNameID, sd.VendorID, sd.SerialNumber, sd.SerialNumberID, sd.StockExpireDate
from dbo.TblStockInDetail sd
inner join dbo.TblItem i on i.ItemID = sd.ItemID
where sd.StockInID = "+PrimaryFieldValue+@" 
group by i.ItemName + (case when isnull(i.ItemCode, '') <> '' then ' - ' + isnull(i.ItemCode, '') else '' end), sd.StockExpireDate,
sd.Reason, sd.StockTransfers, sd.CostPrice, sd.UnitPrice,sd.UomID, sd.UM, sd.ItemID, sd.ClassNameID, sd.VendorID, sd.SerialNumber, sd.SerialNumberID",
                            ShowFooter=false,
                            TableResponsive=false,
                            //ScriptCallBackGetDataTblEditFunction="get_Date_Edit",
                            GetIDENTITYByFiled="StockInID",
                            PrimaryFieldName="StockInID, StockInID",
                            TableName="TblStockInDetail, TblStockInDetail",
                            CustomCondition="ItemType='Stock Transfers'",
                            ReturnFieldNameValue="StockInID, VendorID, PONumber, BillNumber, ReceiveNoteNumber",
                            ReturnFieldNameValueFromTblName = "TblStockIn",
                            //UpdateValueFiledName="ItemDescription",
                            //UpdateValueFiledNameToTblName="TblPurchaseInfo",
                            //UpdateValueFiledNameConditionFiledName="ItemID",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName, Reload_Reason()",
                            //ScriptCallBackFunction="Reload_ItemName",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", Value="ItemID", TblName= "TblStockInDetail,TblStockInDetail"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", Value="UomID", TblName= "TblStockInDetail,TblStockInDetail"
                                },
                                //new DBField()
                                //{
                                //    DBFieldName="ItemDescription", Value="ItemDescription", TblName= "TblStockInDetail,TblStockInDetail"
                                //},                                
                                new DBField()
                                {
                                    DBFieldName="StockInDate", TblName= "'',TblStockInDetail", SetValue="$(\"#Date\").val()", IsDate = true
                                },
                                new DBField()
                                {
                                    DBFieldName="StockOutDate", TblName= "TblStockInDetail,''", SetValue="$(\"#Date\").val()", IsDate = true
                                },
                                new DBField()
                                {
                                    DBFieldName="Type", SetValue="'Stock Transfer'", TblName= "'', TblStockInDetail",
                                },
                                new DBField()
                                {
                                    DBFieldName="Type", SetValue="'Stock Out'", TblName= "TblStockInDetail, ''"
                                },
                                //new DBField()
                                //{
                                //    DBFieldName="Type", Value="ItemINType", TblName= "'',TblStockInDetail", SetValue="'Stock In'"
                                //},
                                new DBField()
                                {
                                    DBFieldName="ItemType", TblName= "TblStockInDetail,TblStockInDetail", SetValue="$(\"#ItemType\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="LocationID", TblName= "TblStockInDetail,''", SetValue="$(\"#FromWarehouse_FromWarehouseID\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="LocationID", TblName= "'',TblStockInDetail", SetValue="$(\"#ToWarehouse_WID\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="UnitPrice", Value="UnitPrice", TblName= "TblStockInDetail,TblStockInDetail", IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="CostPrice", Value="CostPrice", TblName= "TblStockInDetail,TblStockInDetail", IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="QTYOUT", Value="QTYOUT", IsNumber =true, TblName= "TblStockInDetail,''"
                                }
                                ,new DBField()
                                {
                                    DBFieldName="QTY", Value="QTYIN", IsNumber =true, TblName= "'',TblStockInDetail"
                                }
								//,new DBField()
								//{
								//	DBFieldName="StatusID", TblName= "'', TblStockInDetail", SetValue="'79'"
								//},new DBField()
								//{
								//	DBFieldName="StatusID", TblName= "TblStockInDetail, ''", SetValue="'80'"
								//}
								,new DBField()
                                {
                                    DBFieldName="UM", Value="UM", TblName= "TblStockInDetail,TblStockInDetail"
                                }
                                ,new DBField()
                                {
                                    DBFieldName="VendorID", Value="VendorID", TblName= "TblStockInDetail,TblStockInDetail"
                                },
                                new DBField()
                                {
                                    DBFieldName="ClassNameID", Value="ClassNameID", TblName= "TblStockInDetail,TblStockInDetail"
                                },
                                new DBField()
                                {
                                    DBFieldName="SerialNumberID", Value="SerialNumberID", GetValueFromInputName = true, IsNumber=true, TblName= "TblStockInDetail,TblStockInDetail"
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
								//new TblTHead()
								//{
								//	OpenTR="<tr>", Label="ល.រ",Class="col-auto"
								//},
								//new TblTHead()
								//{
								//	 Class="col-6", Label="បរិយាយ"
								//},
								//new TblTHead()
								//{
								//	 Label="មូលហេតុ"
								//},
								//new TblTHead()
								//{
								//	 Label="ចំនួន", Class="col-auto",
								//},
								//new TblTHead()
								//{
								//	 Label="ខ្នាត", Class="col-auto",
								//	 CloseTR="</tr>"
								//},
								new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="Description", Class="col-4"
                                },
                                new TblTHead()
                                {
                                     Label="Serial Number", Class="text-nowrap", Hide = html.SystemID != "13"
                                },
                                new TblTHead()
                                {
                                     Label="Expire Date", Class="text-nowrap", Hide = html.SystemID == "13"
                                },
                                new TblTHead()
                                {
                                     Label="Reason"
                                },
                                new TblTHead()
                                {
                                     Label="Quantity", Class="text-nowrap",
                                },
                                new TblTHead()
                                {
                                     Label="UM", Class="text-nowrap",
                                     CloseTR="</tr>"
                                }
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TRNo\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TDNo"
                                        },
//										new HtmlTextBox()
//										{
//											Name="ItemName",
//											Class="input2 CustInfo", IsCustomForm = true,
//											DBField="ItemName", DivClass="wrap-input3",
//											DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName');",
//                                            //isTypeahead = true,
//                                            HiddenFeild="ItemID",
//											placeholder="Item Name",
//											ISAutoComplete = true,
//											SQL_AutoComplete=@"select distinct top 10 i.ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end +  case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end) ItemName
//from dbo.TblItem i inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end + case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end)) like @SearchText and sd.LocationID = @SearchText2 and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " order by i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end + case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end)",
//                                            DisplayOnFields="QUANTITY, UM, CostPrice, UnitPrice, VendorID, VendorName, ClassNameID",
//											SQLCallBack="select isnull(format((sum(isnull(sd.QTY,0)) - sum(isnull(sd.QTYOUT,0))),'#,#0'),0) QuantityAvailable,i.UM, isnull((select top 1 ssd.CostPrice from dbo.TblStockInDetail ssd where ssd.ItemID = sd.ItemID and ssd.CostPrice <> 0 order by ssd.StockInDetailID desc),0) CostPrice, isnull((select top 1 ssd.UnitPrice from dbo.TblStockInDetail ssd where ssd.ItemID = sd.ItemID and ssd.UnitPrice <> 0 order by ssd.StockInDetailID desc),0) UnitPrice, sd.VendorID, v.VendorName, sd.ClassNameID from dbo.TblStockInDetail sd inner join dbo.TblItem i on i.ItemID = sd.ItemID inner join dbo.TblVendor v on v.VendorID = sd.VendorID where sd.ItemID = @SearchText and sd.LocationID = @Search_Text2 and sd.SystemID = "+html.SystemID+" and sd.WarehouseID = "+html.WarehouseID+" group by i.UM,sd.ItemID, sd.VendorID, v.VendorName, sd.ClassNameID",
//											CboValue="FromWarehouse_FromWarehouseID",
//											CallBackWithControllValueID="FromWarehouse_FromWarehouseID",
//											isApplyTypeaheadOnCallBack=true,
//                                            SQLTypeaheadCallBack = "select distinct ItemID, UM, format(CostPrice,'#0.##') CostPrice\r\nfrom TblItemVariant where ItemID = @ItemID order by ItemID",
//                                            DisplayOnFieldsTypeahead="UM",
//											CallBackDisplayText="UM",
//											CallBackToControlID="UM",
//											FieldName = "ItemID", Required = true
//										},

										new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemName", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName');",
                                            placeholder="Item Name",
                                            ISAutoComplete = true, HiddenField="ItemID",
                                            SQL_AutoComplete=@"select distinct top 10 i.ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end) ItemName
from dbo.TblItem i 
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID 
left join dbo.TblItemSerialDetail srd on srd.ItemID = sd.ItemID
where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end + case when isnull(srd.SerialNumber,'') <> '' then ' - ' + isnull(srd.SerialNumber,'') else '' end)) like @SearchText and sd.LocationID = @SearchText2 and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + @" 
GROUP BY i.ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end)
HAVING (SUM(ISNULL(sd.QTY, 0)) - SUM(ISNULL(sd.QTYOUT, 0))) > 0
order by i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end)",
                                            CboValue="FromWarehouse_FromWarehouseID",
                                            SQLCallBack=@"select isnull(format((sum(isnull(sd.QTY,0)) - sum(isnull(sd.QTYOUT,0))),'#,#0'),0) QUANTITYAVAILABLE,sd.UomID, sd.UM, isnull((select top 1 ssd.CostPrice from dbo.TblStockInDetail ssd where ssd.ItemID = sd.ItemID and ssd.CostPrice <> 0 order by ssd.StockInDetailID desc),0) CostPrice, isnull((select top 1 ssd.UnitPrice from dbo.TblStockInDetail ssd where ssd.ItemID = sd.ItemID and ssd.UnitPrice <> 0 order by ssd.StockInDetailID desc),0) UnitPrice, @Search_Text2 Location_ID 
from dbo.TblStockInDetail sd 
inner join dbo.TblItem i on i.ItemID = sd.ItemID 
where sd.ItemID = @SearchText and sd.LocationID = @Search_Text2 and sd.SystemID = "+html.SystemID+" and sd.WarehouseID = "+html.WarehouseID+@" 
group by sd.UomID,sd.UM,sd.ItemID
HAVING (SUM(ISNULL(sd.QTY, 0)) - SUM(ISNULL(sd.QTYOUT, 0))) > 0",
                                            CallBackWithControlValueID="FromWarehouse_FromWarehouseID",
                                            DisplayOnFields="QUANTITYAVAILABLE,UomID,UM, CostPrice, UnitPrice, Location_ID",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="UM", FieldName = "ItemID", CallBackDisplayText="UM",
                                            SQLTypeaheadCallBack = "select distinct ItemID, UomID, UM, format(CostPrice,'#0.##') CostPrice\r\nfrom TblItemVariant where ItemID = @ItemID order by ItemID",
                                            DisplayOnFieldsTypeahead="UomID,CostPrice",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="SerialNumber", DivClass="wrap-input3",
                                            DBField="SerialNumber",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            placeholder="Enter Serial Number", HiddenField="SerialNumberID",
                                            Hide = html.SystemID != "13"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ExpireDate",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="StockExpireDate",placeholder="Enter Expire Date",
                                            DivClass="wrap-input3", Hide = html.SystemID == "13"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Reason", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="Reason", isTypeahead = true,
                                            TypeAheadAddNewFunctionName="AddNewStockReason()",
                                            SQL="select ReasonID, Reason from dbo.TblStockReason where ReasonID = 18 and SystemID = "+html.SystemID+" and WarehouseID="+html.WarehouseID+" order by Reason",
                                            placeholder="Enter Reason", HiddenField="ReasonID", Required =true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QUANTITY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="StockTransfers", Required = true, Value = "",
                                            placeholder="Enter Quantity", IsAllowInputOnlyNumber=true,
                                            HiddenField="QTYOUT, QTYIN, CostPrice, UnitPrice, StockInDate, Item_Type, FromLocationID, ToLocationID, ItemINType, StatusIN, StatusOUT, VendorID, VendorName, ClassNameID",
                                            HiddenFieldValue="1,1,0,0,0,0,0,0,0,79,80,0,0,0",
                                            OnChangeFunction="QUANTITYChange(this)",

                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UM", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",HiddenField="UomID",
                                            placeholder="Enter UM", Required =true
                                        }
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";
            html.CustomValidateFunction = "CheckWarehouse();";
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();

            return View();
        }
        public void UpdateQtyCal(int stockInID)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "StockInID", stockInID.ToString() }
            };

            string sql = @"
                SELECT si.StockInID, sd.ItemID, sd.UomID, sd.SerialNumberID, sd.SerialNumber, sd.Type, 
                       ISNULL(sd.QTY, 0) QTY, ISNULL(sd.QTYOUT, 0) QTYOUT
                FROM dbo.TblStockIn si
                INNER JOIN dbo.TblStockInDetail sd ON sd.StockInID = si.StockInID
                WHERE si.StockInID = @StockInID";

            DataSet ds = html.LoadData(sql, false, para);
            if (ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int itemID = int.Parse(dr["ItemID"].ToString());
                    int uomID = int.Parse(dr["UomID"].ToString());
                    double qty = double.Parse(dr["QTY"].ToString());
                    double qtyOut = double.Parse(dr["QTYOUT"].ToString());
                    string serialNumber = dr["SerialNumber"].ToString();
                    string type = dr["Type"].ToString();

                    var data = type == "Stock Out"
                        ? html.GetUnitCal(itemID, uomID, qtyOut, 0)
                        : html.GetUnitCal(itemID, uomID, qty, 0);

                    para.Clear();
                    para.Add("StockInID", stockInID.ToString());
                    para.Add("QtyCal", data["QtyCal"].ToString());
                    para.Add("ItemID", itemID.ToString());
                    para.Add("SerialNumber", serialNumber);
                    para.Add("Type", type);

                    sql = type == "Stock Out"
                        ? @"UPDATE dbo.TblStockInDetail SET QtyCalOut = @QtyCal WHERE Type= @Type AND StockInID = @StockInID AND ItemID = @ItemID"
                        : @"UPDATE dbo.TblStockInDetail SET QtyCal = @QtyCal WHERE Type= @Type AND StockInID = @StockInID AND ItemID = @ItemID";

                    if (!string.IsNullOrEmpty(serialNumber))
                    {
                        sql += " AND SerialNumber = @SerialNumber";
                    }

                    html.SaveChangeData(sql, false, para);
                }
            }
        }
        public ActionResult InvoiceStockTransfer(int ID)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "StockInID", ID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", "%" + html.DomainID + "%" }
            };
            string SQL = @"select i.ItemID, i.ItemCode, i.ItemName,bd.UM, bd.Reason, b.SystemID, b.[Description], bd.SerialNumber, b.StockTransfersNumber as TransferNo, format(b.ReceiveDate, 'dd-MM-yyyy') as TransferDate, u.FullName CreatedBy, u.Telephone, 
(select top 1 wf.WarehouseName from dbo.TblStockInDetail sd inner join dbo.TblUserWarehouse wf on wf.WID = sd.LocationID where sd.StockInID = b.StockInID and sd.Type = 'Stock Out') as FromWarehouse, 
(select top 1 wt.WarehouseName from dbo.TblStockInDetail sd inner join dbo.TblUserWarehouse wt on wt.WID = sd.LocationID where sd.StockInID = b.StockInID and sd.Type = 'Stock Transfer') as ToWarehouse, b.PreparedBy, up.Job PreparedByPosition, up.Signature PreparedBySignature, b.CheckBy, uc.Job CheckByPosition, uc.Signature CheckSignature, b.ReceivedBy, ur.Job ReceivedByPosition, ur.Signature ReceivedBySignature,
sum(isnull(bd.QTYOUT,0)) QTYOUT
from dbo.TblStockIn b
inner join dbo.TblStockInDetail bd on bd.StockInID = b.StockInID
inner join dbo.TblItem i on i.ItemID = bd.ItemID
left join dbo.TblItemSerialDetail srd on srd.ItemID = i.ItemID and srd.SerialNumber = bd.SerialNumber
inner join dbo.TblUserLogin u on u.UserLoginID = b.UserID
left join dbo.TblUserLogin up on up.UserLoginID = b.PreparedUserID
left join dbo.TblUserLogin uc on uc.UserLoginID = b.CheckByID
left join dbo.TblUserLogin ur on ur.UserLoginID = b.ReceivedByID
where b.StockInID = @StockInID and b.SystemID = @SystemID and b.WarehouseID = @WarehouseID and b.DomainID like @DomainID
group by i.ItemID, i.ItemName,bd.UM, bd.Reason, i.ItemCode, b.[Description], bd.SerialNumber, b.SystemID, b.StockTransfersNumber, 
format(b.ReceiveDate, 'dd-MM-yyyy'), u.FullName, u.Telephone, b.StockInID, b.PreparedBy, up.Job, up.Signature, b.CheckBy, uc.Job, uc.Signature, b.ReceivedBy, ur.Job, ur.Signature;

select Logo from TblWarehouse where WarehouseID = @WarehouseID and isnull(Logo,'') <> '';";

            DataSet ds = html.LoadData(SQL, false, para);
            if (ds.Tables.Count > 0)
            {
                ViewBag.Data = ds.Tables[0];
                string logo = ds.Tables[1].Rows[0]["Logo"].ToString();
                if (!string.IsNullOrEmpty(logo))
                {
                    ViewBag.Logo = "../Uploads/WarehouseLogo/" + logo;
                }
            }
            return View();
        }

    }
}