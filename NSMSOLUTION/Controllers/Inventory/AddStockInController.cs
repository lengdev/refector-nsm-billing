using MVC;
using Newtonsoft.Json;
using NSMSOLUTION.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Vendor
{
    [Authorize]
    public class AddStockInController : Controller
    {
        // GET: AddStockIn
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Stock In"
            };
            if (html.HasPermission("Add Stock In"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Stock In"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Stock In"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add Stock In";
            html.AddURL = "AddStockIn/StockIn";
            html.BunSearch = "BtnSearchStock";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Stock In Date:",
                FromDateID = "txtStockDate",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtWarehouseName",
                Label = "Warehouse Name:",
                DBField = "WarehouseName",
                SQL_CBO = "select distinct uw.WID, uw.WarehouseName from dbo.TblStockIn b\r\ninner join dbo.TblUserWarehouse uw on uw.WID = b.WID\r\ninner join dbo.TblBranch br on br.BranchID = uw.BranchID\r\n where b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID + " order by uw.WarehouseName",
                DisplayText = "WarehouseName",
                Value = "WID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStockName",
                Label = "Stock Name:",
                DBField = "StockName",
                SQL_CBO = "select sn.StockNameID, sn.StockName from dbo.TblStockName sn inner join dbo.TblStockIn si on si.StockInID = sn.StockNameID where sn.SystemID = " + html.SystemID + " and sn.WarehouseID = " + html.WarehouseID + " order by sn.StockName",
                DisplayText = "StockName",
                Value = "StockNameID",
                ComboBox = true
            });
            //html.HtmlAddControls.Add(new HtmlAddControls()
            //{
            //	ID = "txt_Item_Group_StockIN",
            //	Label = "Item Group:",
            //	DBField = "ItemGroup",
            //	SQL_CBO = "select distinct ig.ItemGroup from dbo.TblStockInDetail s inner join dbo.TblItem i on i.ItemID = s.ItemID inner join dbo.TblItemGroup ig on ig.ItemGroupID = i.ItemGroupID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
            //	DisplayText = "ItemGroup",
            //	Value = "ItemGroup",
            //	ComboBox = true
            //});
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBillNumber",
                ISAutoComplete = true,
                Label = "Bill Number:",
                DBField = "BillNumber",
                SQLAutoComplete = "select distinct top 10 BillNumber from dbo.TblStockIn where lower(BillNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtPONumber",
                ISAutoComplete = true,
                Label = "PO Number:",
                DBField = "PONumber",
                SQLAutoComplete = "select distinct top 10 PONumber from dbo.TblStockIn where lower(PONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID,
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtItemName",
                ISAutoComplete = true,
                Label = "Item Name:",
                DBField = "ItemName",
                SQLAutoComplete = "select distinct top 10 i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName from dbo.TblStockInDetail s inner join dbo.TblItem i on i.ItemID = s.ItemID where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Receive Note#",
                ISAutoComplete = true,
                SQL = "select distinct top 10 ReceiveNoteNumber from dbo.TblStockIn where lower(ReceiveNoteNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("ReceiveNoteNumber"),
                DefaultCheckBok = true,
                Name = "lblReceiveNoteNumber",
                ID = "TXTReceiveNoteNumber"
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.TblID = "tblStock";
            html.FormID = "frmStock";
            html.GH.GridProcedureName = "SP_SHOW_STOCK_IN";
            //html.OrderBy = "No";
            //html.GridEventClick = "onGridStockInClick(this)";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "VendorID", FieldName = "VendorID", IsHiddenField =true, HideOnPrint = true },
                new GridField() { Header = "BillID", FieldName = "BillID", IsHiddenField =true, HideOnPrint = true },
                new GridField() { Header = "Warehouse Name", FieldName = "WarehouseName", IsGroupByField = true, Colspan = 8, DisplayOneRow= true, },
                new GridField() { Header = "Receive Date", FieldName = "ReceiveDateNumber", IsGroupByField= true, Colspan = 8 },
                new GridField() { Header = "VendorName", FieldName = "VendorName", IsGroupByField = true, Colspan = 8, GroupLabel="Vendor Name: " },
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true },
                new GridField() { Header = "Type", FieldName = "Type", },
                new GridField() { Header = "Order QTY", FieldName = "OrderQTY", TextAlign="right", Format="#,#0" },
                new GridField() { Header = "Received QTY", FieldName = "QTYIN", TextAlign="right", Format="#,#0" },
                new GridField() { Header = "UM", FieldName = "UM" },
                new GridField() { Header = "Expire", FieldName = "ItemExpirationDate", Format="dd-MM-yyyy",
                    LinkField="StockInID",
                    DisplayFields="Total QTY IN",
                    DataTableNumber = 2,
                    Colspan = 6 },                //},
                new GridField() { Header = "Action", FieldName = "StockInID",
                    TextAlign = "Center",
                    IsModifyField = true,
                    DeleteWithTableName="TblStockInDetail",
                    //LinkField="StockInID",
                    //DisplayFields="Total QTY IN",
                    //DataTableNumber = 2,
                    Colspan = 8,
                    IsGroupByField=true
                },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        public ActionResult StockIn(string PrimaryFieldValue, string BillID = "0", string Type = "", string PO_Number = "", string BillNumber = "0", bool Clone = false, string Status = "Stock In")
        {
            ViewBag.StockInID = PrimaryFieldValue;
            string BillType = Status;
            ViewBag.Status = Status;
            if (Status == "ConvertToBill")
            {
                BillType = "Bill";
            }
            else if (Status == "ConvertToReceived")
            {
            }
            ViewBag.Type = Type;
            ViewBag.VendorName = "";
            ViewBag.PONumber = PO_Number;
            ViewBag.BillID = BillID;
            HTML html = new HTML
            {
                PrimaryFieldValue = PrimaryFieldValue,
                isCloned = Clone,
                FormID = "StockIn",
                FieldIDFocus = "PONumber",
                SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);",
                SaveChangeAndClose = true
            };
            string vendorID = "0";
            string PurchaseOrderID = "0";
            string PONumber = "";
            ViewBag.StockInData = "";

            if (Type == "NewBill")
            {
                string sqlv = "select v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName from dbo.TblVendor v ";
                sqlv += "where v.VendorID = " + PrimaryFieldValue;
                sqlv += " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
                html.PrimaryFieldValue = "0";
                DataSet dsv = html.LoadData(sqlv);
                string vendornamevalue = dsv.Tables[0].Rows[0]["VendorName"].ToString();
                vendorID = dsv.Tables[0].Rows[0]["VendorID"].ToString();
                ViewBag.VendorName = vendornamevalue;
            }

            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle ="Stock In Information",
                    TableName="TblStockIn",
                    GetIDENTITYByFiled="StockInID",
                    PrimaryFieldName="StockInID",
                    SQL="update dbo.TblBills set [Type] = 'Stock In' where PurchaseOrderID =@PurchaseOrderID and BillID = @BillID; update dbo.TblPurchaseOrder set StockInStatusID = 67 where PurchaseOrderID = @PurchaseOrderID; update dbo.TblPurchaseOrderDetail set StockInStatusID = 67 where PurchaseOrderID = @PurchaseOrderID;",
                    UpdateFiledName="PurchaseOrderID, BillID",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="IssuedBy", SetValue=""+html.UserFullName+""
                        },
                        new DBField()
                        {
                            DBFieldName="IssuedByUserID", SetValue=""+html.UserID+""
                        },
                        new DBField()
                        {
                            DBFieldName="VendorName", Value="Vendor_Name_Reference_VendorName", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="VendorID", Value="Vendor_Name_VendorID", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="CustomerNotes", Value="Customer_Notes", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="TermsConditions", Value="TermsConditions", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="PreparedUserID", SetValue ="$(\"#PreparedBy_PreparedUserID\").val()"
                        },
                        new DBField()
                        {
                            DBFieldName="DeliveredByID", SetValue ="$(\"#DeliveredBy_DeliveredByID\").val()"
                        },
                        new DBField()
                        {
                            DBFieldName="ReceivedByID", SetValue ="$(\"#ReceivedBy_ReceivedByID\").val()"
                        },
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="PO Number:", ID="PONumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "PONumber",
                            isTypeahead = true,
                            SQL="select distinct top 10 PurchaseOrderID, PONumber from dbo.TblPurchaseOrder where StatusID = 90 and isnull(IsClose, 0) <> 1 and isnull(IsCancel, 0) <> 1 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by PurchaseOrderID desc",
                            //DisplayOnFields ="Vendor_Name_VendorID, Vendor_Name, Vendor_Name_Reference_VendorName",
                            Value = PONumber,
                            CallBackFunction="OnPONumberStockINChange",
                        },
                        new HtmlTextBox()
                        {
                            Label="PurchaseOrderID", ID="PurchaseOrderID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm-auto", Hide = true,
                            Value=PurchaseOrderID, DBField="PurchaseOrderID"
                        },
                        new HtmlTextBox()
                        {
                            ID="BillID",IsCustomForm = true,
                            DivClassCol="col-sm-auto", Hide = true,
                            DBField="BillID"
                        },
                        new HtmlTextBox()
                        {
                            ID="BillNumber",IsCustomForm = true,
                            DivClassCol="col-sm-auto", Hide = true,
                            DBField="BillNumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="Reference:", ID="ReferenceNo",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ReferenceNo", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            ID="BillDate",
                            IsCustomForm = true,
                            Hide = true,
                            ISDateTime = true,
                            DBField="BillDate"
                        },
                        new HtmlTextBox()
                        {
                            Label="Vendor Name:", ID="Vendor_Name", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            HiddenField="VendorID,Reference_VendorName",
                            HiddenFieldValue=vendorID+",0",
                            DBField = "VendorName",
                            SQL="select VendorID, (VendorCode + ' - ' + VendorName) VendorName, VendorName Reference_VendorName from dbo.TblVendor where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            isTypeahead = true, FieldName="VendorName",
                            TypeAheadAddNewFunctionName="AddNewVendor();",
                        },
                        new HtmlTextBox()
                        {
                            Label="Vendor DO Number:", ID="VendorDONumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="VendorDONumber", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Vendor Invoice Number:", ID="VendorInvoiceNumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="VendorInvoiceNumber", Hide = true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Receive Note Number:", ID="ReceiveNoteNumber", Required=BillType!="Bill",
                            Class="input2 CustInfo", IsCustomForm = true,
                            IsReadOnly = true, isCloned = Clone,
                            SQLPreFix= "declare @Prefix nvarchar(50), @Module nvarchar(50) = 'Receive Note'; select @Prefix = Prefix from dbo.TblPrefix where Module = 'Receive Note' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'RN-{yy}{MM}{dd}-{0000}' ",
                            CallBackToControlID="ReceiveNoteNumber, PrefixReceiveNoteNumber",
                            UpdatePrefixModule= "Receive Note",
                            Prefix="PrefixReceiveNoteNumber", SetPrefixToDbFieldName="ReceiveNoteNumber",
                            DBField="ReceiveNoteNumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="Stock In Date:", ID="ReceiveDate", Required=BillType!="Bill",
                            Class="input2 CustInfo", IsCustomForm = true,
                            SingleDatePicker = true, DBField = "ReceiveDate"
                        },
                        new HtmlTextBox()
                        {
                            Label="Type:", ID="Type", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "Type",
                            SQL="select 79 StatusID, 'Stock In' [Type] union all select 83 StatusID, 'Opening Stock' [Type]",
                            isTypeahead = true, HiddenField = "StatusID", HiddenFieldValue="79",
                            Value = "Stock In"
                        },
                        new HtmlTextBox()
                        {
                            Label="Prepared By:", ID="PreparedBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PreparedBy",isTypeahead = true,
                            SQL="select u.UserLoginID PreparedUserID,u.FullName PreparedBy, u.Telephone PreparedPhone\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="PreparedUserID",
                            //DisplayOnFields="PreparedPhone"
                        },
                    }
                },
                 new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Delivered By:", ID="DeliveredBy", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="DeliveredBy",isTypeahead = true,
                            SQL="select u.UserLoginID DeliveredByID, u.FullName DeliveredBy\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="DeliveredByID",
                        },
                        new HtmlTextBox()
                        {
                            Label="Delivered Date:", ID="DeliveredDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            SingleDatePicker = true, DBField = "DeliveredDate"
                        },
                        new HtmlTextBox()
                        {
                            Label="Received By:", ID="ReceivedBy", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ReceivedBy",isTypeahead = true,
                            SQL="select u.UserLoginID ReceivedByID, u.FullName ReceivedBy, u.Telephone RequestPhone from dbo.TblUserLogin u inner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID where u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="ReceivedByID",
                            //DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Warehouse:", ID="Warehouse", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Warehouse", isTypeahead = true,
                            //SQL="select BranchID, BranchName BranchName, ContactName, ContactPhone, [Address] + case when Commune <> '' then ', ' + Commune end + case when District <> '' then ', ' + District end + case when ProvinceCity <> '' then ', ' + ProvinceCity end Warehouse from dbo.TblBranch where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" union select WID BranchID, WarehouseName BranchName, ContactName, ContactPhone, [Address] + case when Commune <> '' then ', ' + Commune end + case when District <> '' then ', ' + District end + case when ProvinceCity <> '' then ', ' + ProvinceCity end Warehouse from dbo.TblUserWarehouse where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID,
                            SQL="select uw.WID, uw.WarehouseName Warehouse, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="WID", //TypeaheadGroupByField="WarehouseName",
                            DisplayOnFields= JsonConvert.SerializeObject(new { TxtContactName="ContactName", TxtContactPhone="ContactPhone"}),
                            //CallBackFunction="onWarehouseChange"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Contact Name:", ID="TxtContactName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ContactName", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Contact Phone:", ID="TxtContactPhone",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ContactPhone", Hide = true,
                            HiddenField="IssuedBy,IssuedByUserID", HiddenFieldValue=html.UserFullName+","+html.UserID
                        },
                        new HtmlTextBox()
                        {
                            Name="TotalOrderQty",ID="TotalOrderQty",
                            DBField="TotalOrderQty",
                            Hide=true
                        },
                        new HtmlTextBox()
                        {
                            Name="TotalReceivedQty",ID="TotalReceivedQty",
                            DBField="TotalReceivedQty",
                            Hide=true
                        }
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
                            SQL="select i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName, sd.* from dbo.TblStockInDetail sd\r\ninner join dbo.TblItem i on i.ItemID = sd.ItemID where sd.StatusID in(79, 83) and sd.StockInID =" + PrimaryFieldValue + " and sd.SystemID = " + html.SystemID + " and sd.WarehouseID = " + html.WarehouseID,
                            ShowFooter = false,
                            FooterColspan = 3,
                            ShowTermsConditions = false,
                            ShowCustomerNotes = false,
                            SubTotalColspan = 3,
                            ShowBalance=false,
                            ShowDeposit=false,
                            //ScriptCallBackGetDataTblEditFunction="getDateEdit",
                            GetIDENTITYByFiled="StockInID",
                            PrimaryFieldName="StockInID",
                            TableName="TblStockInDetail",
                            CustomCondition="[Type] in ('Stock In', 'Opening Stock')",
                            ReturnFieldNameValue="StockInID, VendorID, VendorName, PONumber, BillNumber, ReceiveNoteNumber, PurchaseOrderID",
                            ReturnFieldNameValueFromTblName = "TblStockIn",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
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
                                    DBFieldName="VendorID", SetValue="$(\"#Vendor_Name_VendorID\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemDescription", Value="ItemName_Description", GetValueFromTextAreaName=true,//, ISSummerNote = true
                                },
                                //new DBField()
                                //{
                                //    DBFieldName="ChartOfAccountID", Value="ChartOfAccountID"
                                //},
                                //new DBField()
                                //{
                                //    DBFieldName="ItemExpirationDate", Value="ItemExpirationDate", IsDate = true,
                                //},        //                        
								new DBField()
                                {
                                    DBFieldName="BillID", SetValue="$(\"#BillID\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="StockInDate", SetValue="$(\"#ReceiveDate\").val()", IsDate = true
                                },
                                new DBField()
                                {
                                    DBFieldName="Type", SetValue="$(\"#Type\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="StatusID", SetValue="$(\"#Type_StatusID\").val()",
                                },
                                new DBField()
                                {
                                    DBFieldName="CostPrice", Value="CostPrice", IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="SalePrice", Value="SalePrice", IsNumber = true
                                }
                                //new DBField()
                                //{
                                //    DBFieldName="VendorID", Value="VendorID", SetValue="$(\"#Vendor_Name_VendorID\").val()"
                                //}
                                ,new DBField()
                                {
                                    DBFieldName="LocationID", SetValue="$(\"#Warehouse_WID\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="ClassNameID", SetValue ="$(\"#ClassName_ClassID\").val()"
                                },
                            },
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName,TotalAll",//,ApplySummernote('summernote'),ItemExpirationDate_ApplyDatePicker,TotalAll, Reload_Reason()
                            //ScriptCallBackFunction="CallSetAutoComplete_ItemName",
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>", Label="ល.រ"
                                },
                                new TblTHead()
                                {
                                     Class="col-5", Label="បរិយាយ"
                                },
                                new TblTHead()
                                {
                                     Label="ខ្នាត", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="បរិមាណ បញ្ចាទិញ", Class="col-1 text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="បរិមាណ ទទួល", Class="col-1 text-nowrap", Hide = Status == "ConvertToBill"
                                },
                                new TblTHead()
                                {
                                     Label="បរិមាណ នៅសល់", Class="col-1 text-nowrap", Hide=true
                                },
                                new TblTHead()
                                {
                                     Label="ថ្ងៃផុតកំណត់", Class="col-2"
                                },
                                new TblTHead()
                                {
                                     Label="មូលហេតុ", Class="col-1",
                                     CloseTR="</tr>"
                                },
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="Description"
                                },
                                new TblTHead()
                                {
                                     Label="UM", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Order QTY"
                                },
                                new TblTHead()
                                {
                                     Label="Received QTY", Class="text-nowrap", Hide = Status == "ConvertToBill"
                                },
                                new TblTHead()
                                {
                                     Label="QTY Remain", Hide=true
                                },
                                new TblTHead()
                                {
                                     Label="Expire Date", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Reason",Class="text-nowrap col-1",
                                     CloseTR="</tr>"
                                },
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
//                                        new HtmlTextBox()
//                                        {
//                                            Name="ItemName", Required=true,
//                                            Class="input2 CustInfo", IsCustomForm = true,
//                                            DBField="ItemName", DivClass="wrap-input3",
//                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName');",
//                                            placeholder="Item Name",
//                                            ISAutoComplete = true,
//                                            HiddenFeild="ItemID",
//                                            AddButtonFunction="AddNewItem()",
//                                            SQL_AutoComplete=@"select top 10 ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName
//from dbo.TblItem i where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " and i.DomainID like '%"+html.DomainID+"%'",
//											//SQLCallBack="select i.UM, format(i.CostPrice, '#0.##') UnitPrice, i.ItemDescription ItemName_Description from dbo.TblItem i inner join dbo.TblChartOfAccount c on c.ChartOfAccountID = i.PurchaseChartOfAccountID where i.ItemID = @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " and i.DomainID = '%"+html.DomainID+"%'",
//											SQLCallBack="select i.UM, format(i.CostPrice, '#0.##') UnitPrice, ItemDescription ItemName_Description from dbo.TblItemVariant i where i.ItemID = @SearchText",
//                                            DisplayOnFields="UM, UnitPrice,ItemName_Description",
//                                            InputDescription = true,
//                                            isApplyTypeaheadOnCallBack=true,
//                                            CallBackToControlID="UM", FieldName = "ItemID", CallBackDisplayText="UM",
//                                            SQLTypeaheadCallBack = "select distinct * from(select ItemID, UM, format(CostPrice,'#0.##') UnitPrice from dbo.TblItem where ItemID = @ItemID union all select ItemID, UM, format(CostPrice,'#0.##') UnitPrice from dbo.TblUMItem where UM <> '' and ItemID = @ItemID) as a order by ItemID",
//                                            DisplayOnFieldsTypeahead="UnitPrice",
//                                            AddCallBackFunction="ItemExpirationDate_ApplyDatePicker(),Reload_Reason()",
//                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemName", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName','TotalAll');",
                                            //isTypeahead = true,
                                            ISAutoComplete = true,
                                            HiddenField="ItemID", AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete=@"select top 10 ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end + case when isnull(i.ItemSize,'') <> '' then ' - ' + isnull(i.ItemSize,'') else '' end) ItemName
from dbo.TblItem i where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end + case when isnull(i.ItemSize,'') <> '' then ' - ' + isnull(i.ItemSize,'') else '' end)) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID +" and DomainID like '%"+html.DomainID+"%'",
                                            SQLCallBack ="select distinct iv.UM,iv.UomID, iv.CostPrice, iv.SellingPrice SalePrice, iv.ItemDescription ItemName_Description from dbo.TblItemVariant iv where iv.ItemID = @SearchText",
                                            DisplayOnFields="UM, UomID, CostPrice, SalePrice, ItemName_Description",
                                            SQLTypeaheadCallBack = "select distinct ItemID, UomID, UM, CostPrice, SellingPrice SalePrice from dbo.TblItemVariant where ItemID = @ItemID ",
                                            DisplayOnFieldsTypeahead="CostPrice, SalePrice, UomID",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="UM", FieldName = "ItemID",
                                            CallBackDisplayText="UM",
                                            TypeaheadCallBackFunction="TotalAll()",
                                            InputDescription = true, placeholder="Item Name",
                                            AddCallBackFunction="ItemExpirationDate_ApplyDatePicker(),Reload_Reason()",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM", Required = true,
                                            HiddenField="UomID, Bill_ID, Vendor_NameVendorID", HiddenFieldValue="0,0,0",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="OrderQTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="OrderQTY", Value = "0", IsAllowInputOnlyNumber =true,
                                            IsReadOnly = true, HiddenFieldValue="0,0,0,0,0",
                                            HiddenField="Status_ID, LocationID,ClassNameID,CostPrice,SalePrice",Digit="0.000",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",Required=true,
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", //Value = "1", 
                                            IsAllowInputOnlyNumber =true,
                                            //HiddenFeild="StockInDate", HidenFieldValue=ReceiveDate,
                                            placeholder="Enter Received QTY",
                                            //OnKeyupFunction="TotalAll();",Digit="0.000"
                                            OnChangeFunction="TotalQty();"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTYRemain", DivClass="wrap-input3 text-center​​", Hide=true,
                                            Class="input2 CustInfo text-center text-red", IsCustomForm = true,
                                            DBField="QTYRemain",Value = "0", IsAllowInputOnlyNumber =true,IsReadOnly=true,
                                            //OnKeyupFunction="TotalAll();",Digit="0.000"
                                            HiddenField="qtyRemain"
                                        },
                                        new HtmlTextBox()
                                        {
                                            SingleDatePicker=true, DefaultShowDateValue = false,
                                            Name="ItemExpirationDate", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="ItemExpirationDate",
                                            placeholder="Enter Item Expiration Date", ISDateTime=true,
                                            SingleDatePickerPosition="up",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Reason", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="Reason", isTypeahead = true, HiddenField="ReasonID",
                                            SQL="select ReasonID, Reason from dbo.TblStockReason where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by case when Reason = 'New Stock' then 0 else ReasonID end",
                                            Value = "New Stock", TypeAheadAddNewFunctionName="AddNewStockReason()", placeholder="Enter Reason",
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            //html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult InvoiceStockReceive(int ID)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string SQL = @"select si.ReceiveNoteNumber, si.PONumber, case when isnull(si.PreparedBy, '') <> '' then isnull(si.PreparedBy, '') else si.IssuedBy end [PreparedBy], format(si.CreateDate, 'dd-MM-yyyy') PreparedDate, up.Job PreparedPosition, up.Signature PreparedSignature, si.DeliveredBy,  format(si.DeliveredDate, 'dd-MM-yyyy') DeliveredDate, ud.Job DeliveredPosition, 
ud.Signature DeliveredSignature,si.ReceivedBy, format(si.ReceiveDate, 'dd-MM-yyyy') ReceivedDate, ur.Job ReceivedPosition, ur.Signature ReceivedSignature,  v.VendorName, v.Address, v.Phone, w.WarehouseName, i.ItemCode, i.ItemName + CASE WHEN ISNULL(i.ItemSize, '') <> '' THEN ' - ' + ISNULL(i.ItemSize, '') ELSE '' END as ItemName, sid.QTY, sid.UM, sid.Reason, format(sid.ItemExpirationDate, 'dd-MM-yyyy') ItemExpirationDate, sid.SystemID, sid.WarehouseID,
sid.ItemDescription
from dbo.TblStockIn si
inner join dbo.TblStockInDetail sid on sid.StockInID = si.StockInID
inner join dbo.TblVendor v on v.VendorID = si.VendorID 
inner join dbo.TblUserWarehouse w on w.WID = si.WID
inner join dbo.TblItem i on i.ItemID = sid.ItemID
left join dbo.TblItemVariant iv on iv.UomID = sid.UomID and iv.ItemID = i.ItemID
inner join dbo.TblUserLogin up on up.UserLoginID = case when isnull(si.PreparedUserID, '') <> '' then si.PreparedUserID else si.UserID end
left join dbo.TblUserLogin ud on ud.UserLoginID = si.DeliveredByID
left join dbo.TblUserLogin ur on ur.UserLoginID = si.ReceivedByID
where si.StockInID = @StockInID and si.SystemID = @SystemID and si.WarehouseID = @WarehouseID and si.DomainID like @DomainID;
select Logo from TblWarehouse where WarehouseID = @WarehouseID and isnull(Logo,'') <> '';";
            para.Add("StockInID", ID.ToString());
            para.Add("SystemID", html.SystemID);
            para.Add("WarehouseID", html.WarehouseID);
            para.Add("DomainID", "%" + html.DomainID + "%");

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
        public JsonResult LoadPONumber()
        {
            HTML html = new HTML();
            string SQL = "declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'PO' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + ",'" + html.DomainID + "';";
            SQL += "exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ",'" + html.DomainID + "'";
            return Json(new { data = html.JsonData(SQL, false) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadPO(string PONumber, string BillID = "0")
        {
            if (PONumber == "")
                return Json(new { data = "[]" }, JsonRequestBehavior.AllowGet);
            HTML html = new HTML();
            string SQL = @"select​​ p.PurchaseOrderID, p.VendorID, w.WarehouseName, p.Tax, p.Rate, p.CurrencyID, p.CurrencyName, pd.ItemID, pd.ItemName, pd.ItemDescription, pd.ChartOfAccountID, pd.AccountName, p.ContactName, p.ContactPhone,pd.QTY [OrderQTY], isnull(sd.QTYRemain, pd.QTY) QTYRemain, pd.UnitPrice CostPrice, pd.DisP, pd.DisUSD, p.WID, (v.VendorCode + ' - ' + v.VendorName) VendorName, p.ClassID, p.ClassName,p.PONumber, pd.SalePrice, pd.UM, pd.UomID, v.VendorName Vendor_Name_Reference_VendorName, b.BillID,format(b.BillDate, 'dd/MM/yyyy') BillDate, b.BillNumber
            from dbo.TblPurchaseOrder p
            inner join dbo.TblVendor v on v.VendorID = p.VendorID
            left join dbo.TblUserWarehouse w on w.WID = p.WID
            left join dbo.TblBills b on b.PurchaseOrderID = p.PurchaseOrderID
            inner join dbo.TblPurchaseOrderDetail pd on pd.PurchaseOrderID = p.PurchaseOrderID
            OUTER APPLY (
                SELECT TOP 1 si.StockInID
                FROM dbo.TblStockIn si
                WHERE si.PurchaseOrderID = pd.PurchaseOrderID
                ORDER BY si.StockInID DESC
            ) LatestStockIn
            LEFT JOIN dbo.TblStockInDetail sd 
                ON sd.StockInID = LatestStockIn.StockInID 
                AND sd.ItemID = pd.ItemID
			--inner join dbo.TblSaleInfo si on si.ItemID = pd.ItemID
            where p.PONumber = case when @PONumber <> '' then @PONumber else p.PONumber end and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID and p.DomainID = @DomainID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "PONumber", PONumber },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, ValidateInput(false)]
        public async Task<JsonResult> SendNotificationToTelegram(int VendorID, int PurchaseOrderID, string VendorName, string PONumber, int StockInID, string Data)
        {
            SqlResult result = new SqlResult();
            if (VendorName != "")
            {
                string sql = @"select top 1 TelegramMessageID from dbo.TblPOTelegramMessage where POID = @PurchaseOrderID order by TelegramPOMessageID desc;
select ReceiveNoteNumber from dbo.TblStockIn where StockInID = @StockInID";
                HTML html = new HTML();
                Dictionary<string, string> para = new Dictionary<string, string>
                {
                    { "PurchaseOrderID", PurchaseOrderID.ToString()},
                    { "StockInID", StockInID.ToString()},
                };
                NotificationTelegram notificationTelegram = new NotificationTelegram();
                DataSet ds = html.LoadData(sql, false, para);
                string RNNumber = "";
                if (ds.Tables.Count > 0)
                {
                    string TelegramMessageID;
                    if (ds.Tables[0].Rows.Count > 0)
                        TelegramMessageID = ds.Tables[0].Rows[0]["TelegramMessageID"].ToString();
                    if (ds.Tables[1].Rows.Count > 0)
                        RNNumber = ds.Tables[1].Rows[0]["ReceiveNoteNumber"].ToString();
                }

                //result = await html.URLtoPDF("Profile/InvoiceTMP?VendorID=" + VendorID + "&POID=" + StockInID + "&Type=ReceivingNoteFromStockInOnly&ReturnHtmlData=false&ReturnAllHtmlData=false&RedirectToPOView=true", "", "Uploads/Vendors/" + html.ToUrlSlug(VendorName), html.ToUrlSlug(PONumber));
                //result = await html.HTMLToPDF(Data, "Uploads/Vendors/" + html.ToUrlSlug(VendorName), html.ToUrlSlug(PONumber));
                var sw = new StringWriter();
                var controller = DependencyResolver.Current.GetService<AddStockInController>();
                controller.ControllerContext = ControllerContext;
                controller.InvoiceStockReceive(StockInID);
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, "InvoiceStockReceive");
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                string data = sw.GetStringBuilder().ToString();
                string FilePath = HttpContext.Server.MapPath("~/Templates/HtmlTemplate.html");
                StreamReader str = new StreamReader(FilePath);
                string Text = str.ReadToEnd();

                str.Close();
                Text = Text.Replace("{Title}", "Good Received");
                Text = Text.Replace("{BodyMessage}", data);
                result = await html.HTMLToPDF(Text, "Uploads/Vendors/" + html.ToUrlSlug(VendorName), html.ToUrlSlug(PONumber != "" ? PONumber : RNNumber));

                string msg = "Dear Team,<br><br>This is stock in for PO Number: " + PONumber;
                msg += "<br><br>Please check attachment file for detail.<br><br>";
                msg += "Thank you and best regards,<br><br>";
                msg += "NSM ISP BILLING SYSTEM AUTO SEND STOCK IN NOTIFICATION. #" + PONumber;
                if (PONumber == "")
                {
                    msg = "Dear Team,<br><br>This is stock in.";
                    msg += "<br><br>Please check attachment file for detail.<br><br>";
                    msg += "Thank you and best regards,<br><br>";
                    msg += "NSM ISP BILLING SYSTEM AUTO SEND STOCK IN NOTIFICATION.";
                }
                // -986646049 = check po // -967102140 = approval po // -844276850 = test//-4019202888 = stock in
                para.Clear();
                para.Add("SystemID", html.SystemID);
                para.Add("WarehouseID", html.WarehouseID);
                para.Add("DomainID", "%" + html.DomainID + "%");
                sql = @"select * from dbo.TblTelegramGroupNotification 
where Module = 'GoodsReceived' and SystemID = @SystemID and WarehouseID = @WarehouseID and DomainID like @DomainID";
                ds = html.LoadData(sql, false, para);
                long TGID = Convert.ToInt64(ds.Tables[0].Rows[0]["TelegramGroupID"].ToString());
                result = result.Result == ResultType.Successful
                    ? await notificationTelegram.NotificationTelegramBot("", TGID, "", result.OutPutFile, msg, false, false, 0, 0, "", null, false)
                    : await notificationTelegram.NotificationTelegramBot("", TGID, "", result.OutPutFile, result.Message, false, false, 0, 0, "", null, false);
                if (System.IO.File.Exists(result.OutPutFile))
                {
                    System.IO.File.Delete(result.OutPutFile);
                }
            }
            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateQtyCal(int StockInID)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            string sql = @"select ItemID, UomID, CostPrice,QTY from dbo.TblStockInDetail where Type in ('Stock In', 'Opening Stock') and StockInID = " + StockInID;
            DataSet ds = html.LoadData(sql);
            if (ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int ItemID = int.Parse(dr["ItemID"].ToString());
                    int UomID = int.Parse(dr["UomID"].ToString());
                    double QtyRecieved = double.Parse(dr["QTY"].ToString());
                    double CostPrice = double.Parse(dr["CostPrice"].ToString());
                    var GetUnitCal = html.GetUnitCal(ItemID, UomID, QtyRecieved, CostPrice);
                    sql = @"update dbo.TblStockInDetail set QtyCal = " + GetUnitCal["QtyCal"] + @"
where StockInID = " + StockInID + "and ItemID = " + ItemID + "and UomID = " + UomID;
                    result = html.SaveChangeData(sql, false, false);
                }
            }
            return Json(new { Resutl = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetLastExpireDateItem(int StockInID)
        {
            HTML html = new HTML();
            //SqlResult result = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "StockInID", StockInID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            string sql = @"select sd.ItemID, sd.StockInID, format(sd.ItemExpirationDate, 'yyyy-MM-dd') ExpireDate, s.ReceiveDate, sd.QTY, sd.UomID, sd.UM, sd.LocationID
from dbo.TblStockInDetail sd
inner join dbo.TblStockIn s on sd.StockInID = s.StockInID
where sd.StockInID = @StockInID and sd.ItemExpirationDate is not null and sd.SystemID = @SystemID and sd.WarehouseID = @WarehouseID;";
            var result = html.JsonData(sql, false, para);
            if (string.IsNullOrEmpty(result) || result == "[]")
            {
                sql = @"select sd.ItemID, sd.StockInID, format((select top 1 ExpireDate from dbo.TblStockExpireDate se where se.ItemID = sd.ItemID order by ExpireDate desc), 'yyyy-MM-dd') ExpireDate, s.ReceiveDate, sd.QTY, sd.UomID, sd.UM, sd.LocationID
from dbo.TblStockInDetail sd
inner join dbo.TblStockIn s on sd.StockInID = s.StockInID
where sd.StockInID = @StockInID and sd.SystemID = @SystemID and sd.WarehouseID = @WarehouseID;";
                result = html.JsonData(sql, false, para);
            }
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public void SaveChangeItemExpireWithBarcode(int StockInID, string Data, bool IsEdit = false)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            var values = JsonConvert.DeserializeObject<List<StockExpireDate>>(Data);

            foreach (var item in values)
            {
                if (item.ExpireDate != null)
                {
                    Dictionary<string, string> para = new Dictionary<string, string>
                    {
                        { "ItemID", item.ItemID.ToString() },
                        { "StockInID", StockInID.ToString() },
                        { "Barcode", item.Barcode.ToString() },
                        { "ExpireDate", item.ExpireDate.ToString() },
                        { "StockInDate", item.ReceiveDate.ToString() },
                        { "QTY", item.QTY.ToString() },
                        { "UomID", item.UomID.ToString() },
                        { "UM", item.UM.ToString() },
                        { "LocationID", item.LocationID.ToString() },
                        { "CreateDate", html.GetCurrentDate() },
                        { "SystemID", html.SystemID },
                        { "WarehouseID", html.WarehouseID },
                        { "DomainID",html.DomainID}
                    };

                    string sql = @"
            IF NOT EXISTS (SELECT 1 FROM TblStockExpireDate WHERE ItemID = @ItemID AND StockInID = @StockInID AND ExpireDate = @ExpireDate)
            BEGIN
                declare @OldBarcode nvarchar(50);
                select top 1 @OldBarcode = Barcode from dbo.TblStockExpireDate where ItemID = @ItemID and ExpireDate = @ExpireDate order by CreateDate;
                INSERT INTO TblStockExpireDate (ItemID, StockInID, Barcode, StockInDate, ExpireDate, QTYIN, UomID, UM, LocationID, CreateDate, SystemID, WarehouseID, DomainID) 
                VALUES (@ItemID, @StockInID, case when @OldBarcode IS NOT NULL AND @OldBarcode <> @Barcode then @OldBarcode else @Barcode end, @StockInDate, @ExpireDate, @QTY, @UomID, @UM, @LocationID, @CreateDate, @SystemID, @WarehouseID, @DomainID)
            END
            ELSE 
            BEGIN
                UPDATE TblStockExpireDate 
                SET ItemID = @ItemID, StockInID = @StockInID, StockInDate = @StockInDate, ExpireDate = @ExpireDate, QTYIN = @QTY, UomID = @UomID, UM = @UM, LocationID = @LocationID, CreateDate = @CreateDate, SystemID=@SystemID, WarehouseID=@WarehouseID, DomainID=@WarehouseID 
                WHERE ItemID = @ItemID AND StockInID = @StockInID AND ExpireDate = @ExpireDate
            END";

                    html.SaveChangeData(sql, false, para);
                }
            }
        }

    }
}