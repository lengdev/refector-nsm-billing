using MVC;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Vendor
{
    [Authorize]
    public class AdjustmentStockController : Controller
    {
        // GET: AdjustmentStock
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Adjustment Stock"
            };
            if (html.HasPermission("Add Adjustment Stock"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Adjustment Stock"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Adjustment Stock"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Adjust Stock";
            html.AddURL = "AdjustmentStock/AdjustStock";

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Adjust Date:",
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Adjust No",
                SQL = "select distinct top 10 ReferenceNo from dbo.TblStockIn where StatusID = 82 and lower(ReferenceNo) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ReferenceNo desc",
                RadioValue = EncryAndDecry.Encryp("ReferenceNo"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Adjust Date", FieldName = "ReceiveDate", FormatDateTime = "dd-MMM-yyyy", TextAlign = "center" },
                new GridField() { Header = "Adjust No", FieldName = "AdjustNo", TextAlign = "center" },
                new GridField() { Header = "Warehouse", FieldName = "WarehouseName"},
                new GridField() { Header = "Prepared By", FieldName = "PreparedBy"},
                new GridField() { Header = "Check By", FieldName = "CheckBy"},
                new GridField() { Header = "Created By", FieldName = "CreatedBy"},
                new GridField() { Header = "Description", FieldName = "Description"},
                new GridField() { Header = "Action", FieldName = "StockInID", TextAlign = "Center",  IsModifyField = true, DeleteWithTableName="TblStockInDetail"},
            };

            var kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_SHOW_STOCK_ADJUSTMENT"
            };

            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        public ActionResult AdjustStock(string PrimaryFieldValue, string Type = "", string ItemID = "", string ItemName = "", string LocationID = "", string LocationName = "")
        {
            HTML html = new HTML
            {
                SaveChangeAndClose = true,
                FormID = "AdjustmentStock",
                PrimaryFieldValue = PrimaryFieldValue
            };
            ViewBag.ItemID = ItemID;
            ViewBag.ItemName = ItemName;
            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";
            if (Type != "")
                html.SaveChangeAndClose = true;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="New Adjustment Stock",
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
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Adjustment No:", ID="ReferenceNo", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ReferenceNo", IsReadOnly = true,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'Stock Adjustment'; select @Prefix = Prefix from dbo.TblPrefix where Module = @Module and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, '{00000}'",
                            CallBackToControlID="ReferenceNo, PrefixReferenceNo", UpdatePrefixModule="Stock Adjustment", Prefix="PrefixReferenceNo", SetPrefixToDbFieldName="ReferenceNo"
                        },
                        new HtmlTextBox()
                        {
                            Label="Date:", ID="Date",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ReceiveDate", SingleDatePicker= true, Required = true
                        }
                    }
                },new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
						//new HtmlTextBox()
						//{
						//	Label="Date:", ID="Date",
						//	Class="input2 CustInfo", IsCustomForm = true,
						//	DBField="ReceiveDate", SingleDatePicker= true, Required = true
						//},
						new HtmlTextBox()
                        {
                            Label="Class Name:", ID="ClassName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ClassName", isTypeahead = true,
                            SQL="select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            HiddenField="ClassID", DivClassCol="col-sm-auto", Hide = true
                           // CallBackFunction="onClassNameChange"
                        },
                        new HtmlTextBox()
                        {
                            Label="Warehouse:", ID="Warehouse", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Warehouse", isTypeahead = true,
                            //SQL="select BranchID, BranchName BranchName, ContactName, ContactPhone, [Address] + case when Commune <> '' then ', ' + Commune end + case when District <> '' then ', ' + District end + case when ProvinceCity <> '' then ', ' + ProvinceCity end Warehouse from dbo.TblBranch where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" union select WID BranchID, WarehouseName BranchName, ContactName, ContactPhone, [Address] + case when Commune <> '' then ', ' + Commune end + case when District <> '' then ', ' + District end + case when ProvinceCity <> '' then ', ' + ProvinceCity end Warehouse from dbo.TblUserWarehouse where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID,
                            //SQL="select uw.WID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName, uw.ContactName, uw.ContactPhone, uw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + case when uw.District <> '' then ', ' + uw.District end + case when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end Warehouse from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            SQL="select uw.WID, uw.WarehouseName Warehouse, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="WID", HiddenFieldValue = LocationID, Value = LocationName
							//TypeaheadGroupByField="WarehouseName",
                            //DisplayOnFields= JsonConvert.SerializeObject(new { TxtContactName="ContactName", TxtContactPhone="ContactPhone"}),
                            //CallBackFunction="onWarehouseChange"
                        },
                    }
                },
				//new  FormData() {
				//	htmlControl = new HTMLControl[]
				//	{
				//		new HtmlTextBox()
				//		{
				//			Label="Contact Name:", ID="TxtContactName", Required=true,
				//			Class="input2 CustInfo", IsCustomForm = true,
				//			DBField="ContactName"
				//		},
				//		new HtmlTextBox()
				//		{
				//			Label="Contact Phone:", ID="TxtContactPhone", Required=true,
				//			Class="input2 CustInfo", IsCustomForm = true,
				//			DBField="ContactPhone"
				//		},
				//	}
				//},
                //new  FormData() {
                //    htmlControl = new HTMLControl[]
                //    {
                //        new HtmlTextBox()
                //        {
                //            Label="Account:", ID="ChartOfAccountID",
                //            Required=true,
                //            Class="input2 CustInfo", IsCustomForm = true,
                //            //AddNewFuntionName="AddNewChartOfAccount('ChartOfAccountID')",
                //            SQL="select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                //            //DisplayText="AccountName",
                //            //Value="ChartOfAccountID",
                //            //GroupByField="AccountType",
                //            HiddenFeild ="ChartOfAccountID",
                //            DBField="AccountName", isTypeahead=true, TypeaheadGroupByField="AccountType",
                //            Hide = true

                //        },
                //        //new HtmlComboBox()
                //        //{
                //        //    Label="Reason:", ID="Reason",
                //        //    Required=true,
                //        //    Class="input2 CustInfo", IsCustomForm = true,
                //        //    AddNewFuntionName="AddNewStockReason();",
                //        //    SQL_CBO ="select ReasonID, Reason from dbo.TblStockReason where SystemID = "+html.SystemID+" and WarehouseID="+html.WarehouseID+" order by Reason",
                //        //    DisplayText="Reason",
                //        //    Value="ReasonID",
                //        //    DBField="ReasonID"
                //        //}
                //    }
                //},
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
                            DBField="PreparedPhone"
                        },
                        new HtmlTextBox()
                        {
                            Label="Check By:", ID="CheckBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CheckBy",isTypeahead = true,
                            SQL="select u.UserLoginID CheckByID, u.FullName CheckBy from dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="CheckByID",
                        },
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
                            DBField="ItemType", Hide = true, Value="Adjustment Stock"
                        },
                        new HtmlTextBox()
                        {
                            Label="StatusID", ID="StatusID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="StatusID", Hide = true, Value="82"//Adjustment Stock
                        },
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
                            ShowFooter=false,
                            TableResponsive=false,
                            //ScriptCallBackGetDataTblEditFunction="get_Date_Edit",
                            GetIDENTITYByFiled="StockInID",
                            PrimaryFieldName="StockInID",
                            TableName="TblStockInDetail",
                            CustomCondition="ItemType='Adjustment Stock'",
                            ReturnFieldNameValue="StockInID, VendorID, PONumber, BillNumber, ReceiveNoteNumber",
                            ReturnFieldNameValueFromTblName = "TblStockIn",
                            //UpdateValueFiledName="ItemDescription",
                            //UpdateValueFiledNameToTblName="TblPurchaseInfo",
                            //UpdateValueFiledNameConditionFiledName="ItemID",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName, Reload_Reason(), Reload_AdjustType()",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", Value="ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", Value="UomID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemDescription", Value="ItemDescription"
                                },
                                new DBField()
                                {
                                    DBFieldName="LocationID", Value="LocationID", IsNumber =true,
                                },
                                new DBField()
                                {
                                    DBFieldName="QTYOUT", Value="QTYOUT", GetValueFromInputName = true, IsNumber =true,
                                },
                                new DBField()
                                {
                                    DBFieldName="QTY", Value="QTYIN", GetValueFromInputName = true, IsNumber =true,
                                },
                                new DBField()
                                {
                                    DBFieldName="StockInDate", Value="StockInDate", SetValue="$(\"#Date\").val()", IsDate=true,
                                },
                                new DBField()
                                {
                                    DBFieldName="UnitPrice", Value="UnitPrice", GetValueFromInputName = true, IsNumber=true,
                                },
                                new DBField()
                                {
                                    DBFieldName="VendorID", Value="Vendor_ID", GetValueFromInputName = true, IsNumber=true,
                                },
                                new DBField()
                                {
                                    DBFieldName="Type", SetValue="'Stock Adjustment'"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemType", Value="Item_Type", GetValueFromInputName = true
                                },
                                new DBField()
                                {
                                    DBFieldName="LocationID", Value="Location_ID", GetValueFromInputName = true, IsNumber=true,
                                },
                                new DBField()
                                {
                                    DBFieldName="QuantityAdjusted", Value="QTYADJUST", GetValueFromInputName=true, IsNumber=true,
                                },
                                new DBField()
                                {
                                    DBFieldName="AdjustID", Value="AdjustID"
                                },
                                //new DBField()
                                //{
                                //    DBFieldName="ChartOfAccountID", Value="ChartOfAccountID"
                                //},
                                new DBField()
                                {
                                    DBFieldName="StatusID", Value="Status_ID", GetValueFromInputName = true, IsNumber=true,
                                },
                                //new DBField()
                                //{
                                //    DBFieldName="UM", Value="UM"
                                //},
								new DBField()
                                {
                                    DBFieldName="SerialNumberID", Value="SerialNumberID", GetValueFromInputName = true, IsNumber=true,
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
        //                        new TblTHead()
        //                        {
        //                            OpenTR="<tr>", Label="ល.រ"
        //                        },
        //                        new TblTHead()
        //                        {
        //                             Class="col-5", Label="បរិយាយ"
        //                        },
								//new TblTHead()
								//{
								//	 Label="ខ្នាត", Class="col-1"
								//},
								//new TblTHead()
								//{
								//	 Label="តំលៃដើម", Class="col-1"
								//},
								//new TblTHead()
        //                        {
        //                             Label="មូលហេតុ", Class="col-3"
        //                        },
        //                        new TblTHead()
        //                        {
        //                             Label="បរិមាណ អាចប្រើបាន", Class="col-1 text-nowrap"
        //                        },                                
        //                        new TblTHead()
        //                        {
        //                             Label="បរិមាណថ្មី ក្នុងដៃ", Class="col-3"
        //                        },
        //                        new TblTHead()
        //                        {
        //                             Label="បរិមាណកែប្រែ", Class="col-1",
        //                             CloseTR="</tr>"
        //                        },
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="Description", Class="col-5"
                                },
                                new TblTHead()
                                {
                                     Label="UM", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="Cost", Class="col-1"
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
                                     Label="Adjust Type", Class="col-1 text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Reason", Class="col-2"
                                },
                                new TblTHead()
                                {
                                     Label="QUANTITY AVAILABLE", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="NEW QUANTITY ON HAND", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="QUANTITY ADJUSTED", Class="text-nowrap",
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
//											Name="ItemName", Required=true,
//											Class="input2 CustInfo", IsCustomForm = true,
//											DBField="ItemName", DivClass="wrap-input3",
//											DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName');",
//											placeholder="Item Name",
//											ISAutoComplete = true, HiddenFeild="ItemID",
//											SQL_AutoComplete=@"select distinct top 10 i.ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end +  case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end) ItemName
//from dbo.TblItem i inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end + case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end)) like @SearchText and sd.LocationID = @SearchText2 and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " order by i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end + case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end)",
//											CboValue="Warehouse_WID",
//											SQLCallBack="select isnull(format((sum(isnull(sd.QTY,0)) - sum(isnull(sd.QTYOUT,0))),'#,#0'),0) QUANTITYAVAILABLE,sd.UM, isnull((select top 1 ssd.CostPrice from dbo.TblStockInDetail ssd where ssd.ItemID = sd.ItemID and ssd.CostPrice <> 0 order by ssd.StockInDetailID desc),0) CostPrice, isnull((select top 1 ssd.UnitPrice from dbo.TblStockInDetail ssd where ssd.ItemID = sd.ItemID and ssd.UnitPrice <> 0 order by ssd.StockInDetailID desc),0) UnitPrice, sd.VendorID Vendor_ID, @Search_Text2 Location_ID, v.VendorName from dbo.TblStockInDetail sd inner join dbo.TblItem i on i.ItemID = sd.ItemID left join dbo.TblVendor v on v.VendorID = sd.VendorID where sd.ItemID = @SearchText and sd.LocationID = @Search_Text2 and sd.SystemID = "+html.SystemID+" and sd.WarehouseID = "+html.WarehouseID+" group by sd.UM,sd.ItemID, sd.VendorID, v.VendorName",
//											CallBackWithControllValueID="Warehouse_WID",
//											DisplayOnFields="QUANTITYAVAILABLE,UM, CostPrice, UnitPrice, Vendor_ID, Location_ID",
//											isApplyTypeaheadOnCallBack=true,
//											CallBackToControlID="UM", FieldName = "ItemID", CallBackDisplayText="UM",
//											SQLTypeaheadCallBack = "select distinct ItemID, UM, format(CostPrice,'#0.##') CostPrice\r\nfrom TblItemVariant where ItemID = @ItemID order by ItemID",
//											DisplayOnFieldsTypeahead="CostPrice",
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
where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end + case when isnull(srd.SerialNumber,'') <> '' then ' - ' + isnull(srd.SerialNumber,'') else '' end)) like @SearchText and sd.LocationID = @SearchText2 and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " order by i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end)",
                                            CboValue="Warehouse_WID",
                                            SQLCallBack="select isnull(format((sum(isnull(sd.QTY,0)) - sum(isnull(sd.QTYOUT,0))),'#,#0'),0) QUANTITYAVAILABLE,iv.UomID, iv.UM, isnull((select top 1 ssd.CostPrice from dbo.TblStockInDetail ssd where ssd.ItemID = sd.ItemID and ssd.CostPrice <> 0 order by ssd.StockInDetailID desc),0) CostPrice, isnull((select top 1 ssd.UnitPrice from dbo.TblStockInDetail ssd where ssd.ItemID = sd.ItemID and ssd.UnitPrice <> 0 order by ssd.StockInDetailID desc),0) UnitPrice, @Search_Text2 Location_ID from dbo.TblStockInDetail sd inner join dbo.TblItem i on i.ItemID = sd.ItemID inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID where sd.ItemID = @SearchText and sd.LocationID = @Search_Text2 and sd.SystemID = "+html.SystemID+" and sd.WarehouseID = "+html.WarehouseID+" group by iv.UomID, iv.UM,sd.ItemID",
                                            CallBackWithControlValueID="Warehouse_WID",
                                            DisplayOnFields="QUANTITYAVAILABLE, UomID, UM, CostPrice, UnitPrice, Location_ID",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="UM", FieldName = "ItemID", CallBackDisplayText="UM",
                                            SQLTypeaheadCallBack = "select distinct ItemID, UomID, UM, format(CostPrice,'#0.##') CostPrice\r\nfrom TblItemVariant where ItemID = @ItemID order by ItemID",
                                            DisplayOnFieldsTypeahead="UomID, CostPrice",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",HiddenField="UomID",
                                            placeholder="Enter UM", Required = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="CostPrice",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="CostPrice", ISNumeric=true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter CostPrice", Required = true
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
                                            Name="AdjustType", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="AdjustType", isTypeahead = true, Required = true,
                                            HiddenField="AdjustID",
                                            SQL="select AdjustID, AdjustType from dbo.TblAdjust",
                                            placeholder="Enter Adjust Type"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Reason", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="Reason", isTypeahead = true, Required = true,
                                            TypeAheadAddNewFunctionName="AddNewStockReason()",
                                            SQL="select ReasonID, Reason from dbo.TblStockReason where ReasonID in (8,9) and SystemID = "+html.SystemID+" and WarehouseID="+html.WarehouseID+" order by Reason",
                                            placeholder="Enter Reason"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QUANTITYAVAILABLE", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            IsReadOnly = true, HiddenField="Date",
                                            DBField="QuantityAvliable", IsAllowInputOnlyNumber=true,
                                            Required = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="NEWQUANTITYONHAND", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QuantityOnHand", IsAllowInputOnlyNumber=true,
                                            placeholder="NEW QUANTITY ON HAND",// HiddenFeild= "QTYIN",
                                            OnKeyupFunction="NEQTYONHAND(this);", Required = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTYADJUST", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            HiddenField="QTYOUT, QTYIN, UnitPrice, StockInDate,Location_ID,Status_ID,Item_Type",
                                            HiddenFieldValue="0,0,0,0,0,82,Adjustment Stock", ISNumeric=true,
                                            //DBField="QuantityAdjusted",
                                            placeholder="Eg. 10, -10", //IsallowInputOnlyNumber=true,
											OnKeyupFunction="QTYADJUSTED(this);"
                                        }
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();

            return View();
        }
        public JsonResult GetItemInfo(int ItemID)
        {
            HTML html = new HTML();
            //string SQL = @"select format((sum(isnull(sd.QTY,0)) - sum(isnull(sd.QTYOUT,0))),'#,#0') QuantityAvailable,
            //i.UM, sd.CostPrice, sd.UnitPrice from dbo.TblStockInDetail sd
            //inner join dbo.TblItem i on i.ItemID = sd.ItemID
            //where sd.ItemID = @ItemID and sd.SystemID = @SystemID and sd.WarehouseID = @WarehouseID
            //group by i.UM, sd.CostPrice, sd.UnitPrice";
            string SQL = @"select format((sum(isnull(sd.QTY,0)) - sum(isnull(sd.QTYOUT,0))),'#,#0') QuantityAvailable,
            i.UM,
            isnull((select top 1 ssd.CostPrice from dbo.TblStockInDetail ssd where ssd.ItemID = sd.ItemID and ssd.CostPrice <> 0 order by ssd.StockInDetailID desc),0) CostPrice,
			isnull((select top 1 ssd.UnitPrice from dbo.TblStockInDetail ssd where ssd.ItemID = sd.ItemID and ssd.UnitPrice <> 0 order by ssd.StockInDetailID desc),0) UnitPrice
            from dbo.TblStockInDetail sd
            inner join dbo.TblItem i on i.ItemID = sd.ItemID
            where sd.ItemID = @ItemID and sd.SystemID = @SystemID and sd.WarehouseID = @WarehouseID
            group by i.UM,sd.ItemID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ItemID",ItemID.ToString() },
                {"SystemID",html.SystemID },
                {"WarehouseID", html.WarehouseID }
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }
        public void UpdateSerialNumberStatus(int StockInID)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "StockInID", StockInID.ToString() }
            };

            string sql = @"
				select si.StockInID, sd.ItemID, sd.Type, sd.AdjustID, sd.SerialNumberID, sd.SerialNumber
				from dbo.TblStockIn si 
				inner join dbo.TblStockInDetail sd on sd.StockInID = si.StockInID
				where si.StockInID = @StockInID and  sd.Type = 'Stock Adjustment'";

            ds = html.LoadData(sql, false, para);
            if (ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int ItemID = int.Parse(dr["ItemID"].ToString());
                    string SerialNumber = dr["SerialNumber"].ToString();
                    if (!string.IsNullOrEmpty(SerialNumber))
                    {
                        para.Clear();
                        para.Add("ItemID", ItemID.ToString());

                        sql = @"-- Create the temp table with an index
							CREATE TABLE #temp (
									ItemID INT,
									QTY DECIMAL(18, 2),
									SerialNumberID INT,
									SerialNumber NVARCHAR(50),
									INDEX IX_Temp_ItemID (ItemID)
							);
		
							WITH InventoryCTE AS (
									SELECT 
											i.ItemID,
											SUM(CASE 
													WHEN ISNULL(srd.SerialNumber, '') IN ('', 'N/A') THEN ISNULL(sd.QTY, 0) - ISNULL(sd.QTYOUT, 0)
													WHEN i.ItemID = sd.ItemID 
															AND sd.SerialNumber = srd.SerialNumber THEN ISNULL(sd.QTY, 0) - ISNULL(sd.QTYOUT, 0)
													ELSE CASE WHEN srd.StockInID = sd.StockInID AND sd.Type IN ('Stock In', 'Opening Stock') THEN 1 END 
											END) AS QTY,
											srd.ItemSerialDetailID SerialNumberID,
											srd.SerialNumber
									FROM dbo.TblItem i 
									LEFT JOIN dbo.TblItemSerialDetail srd ON srd.ItemID = i.ItemID
									LEFT JOIN dbo.TblStockInDetail sd ON i.ItemID = sd.ItemID
											AND (ISNULL(sd.SerialNumber, '') = '' OR sd.SerialNumber = srd.SerialNumber)
									LEFT JOIN dbo.TblUserWarehouse uw ON uw.WID = sd.LocationID
									WHERE i.ItemID = @ItemID  
									AND (
											ISNULL(srd.SerialNumber, '') = '' OR 
											EXISTS (
													SELECT 1 
													FROM dbo.TblStockInDetail 
													WHERE StockInID = srd.StockInID 
													AND Type IN ('Stock In', 'Opening Stock')
											)
									)
									GROUP BY 
											i.ItemID,
											srd.ItemSerialDetailID,
											srd.SerialNumber
							)
							INSERT INTO #temp
							SELECT *
							FROM InventoryCTE
							WHERE QTY IS NOT NULL;

							-- Final select with index usage
							SELECT *
							FROM #temp
							WHERE ItemID = @ItemID 
							ORDER BY SerialNumber;

							DROP TABLE #temp;";

                        DataSet dsSerial = html.LoadData(sql, false, para);
                        if (dsSerial.Tables.Count > 0)
                        {
                            if (dsSerial.Tables[0].Rows.Count > 0)
                            {
                                var rows = dsSerial.Tables[0].Rows;
                                foreach (DataRow row in rows)
                                {
                                    string SerialNumber2 = row["SerialNumber"].ToString();
                                    string QTY = row["QTY"].ToString();
                                    if (SerialNumber == SerialNumber2)
                                    {
                                        sql = @"
										update dbo.TblItemSerialDetail set IsAvailable = @IsAvailable, TransactionType = 'Adjustment Stock'
										where ItemID = @ItemID and SerialNumber = @SerialNumber";
                                        para.Clear();
                                        para.Add("IsAvailable", (QTY == "0.00") ? "0" : "1");
                                        para.Add("ItemID", ItemID.ToString());
                                        para.Add("SerialNumber", SerialNumber);
                                        html.SaveChangeData(sql, false, para);
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }
        public void UpdateQtyCal(int stockInID)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "StockInID", stockInID.ToString() }
            };

            string sql = @"
                SELECT si.StockInID, sd.ItemID, sd.UomID, sd.SerialNumberID, sd.SerialNumber, sd.Type,ISNULL(sd.QTY, 0) QTY, ISNULL(sd.QTYOUT, 0) QTYOUT
				FROM dbo.TblStockIn si
				INNER JOIN dbo.TblStockInDetail sd ON sd.StockInID = si.StockInID
                WHERE si.StockInID = @StockInID AND sd.Type='Stock Adjustment'";

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

                    var data = qtyOut > 0
                        ? html.GetUnitCal(itemID, uomID, qtyOut, 0)
                        : html.GetUnitCal(itemID, uomID, qty, 0);

                    para.Clear();
                    para.Add("StockInID", stockInID.ToString());
                    para.Add("QtyCal", data["QtyCal"].ToString());
                    para.Add("ItemID", itemID.ToString());
                    para.Add("SerialNumber", serialNumber);

                    sql = qtyOut > 0
                        ? @"UPDATE dbo.TblStockInDetail SET QtyCalOut = @QtyCal WHERE StockInID = @StockInID AND ItemID = @ItemID"
                        : @"UPDATE dbo.TblStockInDetail SET QtyCal = @QtyCal WHERE StockInID = @StockInID AND ItemID = @ItemID";

                    if (!string.IsNullOrEmpty(serialNumber))
                    {
                        sql += " AND SerialNumber = @SerialNumber";
                    }

                    html.SaveChangeData(sql, false, para);
                }
            }
        }
        public ActionResult InvoiceAdjustmentStock(int ID)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "StockInID", ID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", "%" + html.DomainID + "%" }
            };
            string SQL = @"select  i.ItemCode, i.ItemName, format(si.ReceiveDate, 'dd-MM-yyyy') ReceivedDate, sd.QuantityOnHand, sd.QuantityAvliable, sd.QuantityAdjusted, sd.UM, sd.Reason, sd.SystemID, si.CreateBy, si.Warehouse, sd.SerialNumber, sd.AdjustType, si.PreparedPhone, si.CheckBy, si.PreparedBy, up.Job PreparedByPosition, up.Signature PreparedBySignature, uc.Job CheckByPosition, uc.Signature CheckSignature, si.ReferenceNo
from dbo.TblStockIn si
inner join dbo.TblStockInDetail sd on sd.StockInID = si.StockInID
inner join dbo.TblItem i on i.ItemID = sd.ItemID
left join dbo.TblUserLogin up on up.UserLoginID = si.PreparedUserID
left join dbo.TblUserLogin uc on uc.UserLoginID = si.CheckByID
where si.StockInID = @StockInID and si.SystemID = @SystemID and si.WarehouseID = @WarehouseID and si.DomainID like @DomainID;
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