using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers.Vendor
{
    [Authorize]
    public class PurchaseOrderController : Controller
    {
        // GET: PurchaseOrder
        public ActionResult Index()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            html.ReportTitle = "List Purchase Order";
            if (html.HasPermission("Add Purchase Order"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Purchase Order"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Purchase Order"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New";
            html.AddURL = "PurchaseOrder/CreatePurchaseOrder";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "PO Date:",
                DBField = "PODate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "PO Status:",
                DBField = "s.StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtPONumber",
                ISAutoComplete = true,
                Label = "PO Number:",
                DBField = "PONumber",
                SQLAutoComplete = "select distinct top 10 PONumber from dbo.TblPurchaseOrder where lower(PONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by PONumber"
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "p.ClassName",
                SQL_CBO = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Vendor Name",
                SQL = "select distinct top 10 v.VendorName from dbo.TblVendor v where lower(v.VendorName) like @SearchText and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " order by v.VendorName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("p.VendorName"),
                DefaultCheckBok = true
            });

            string sql = @"select row_number() over(partition by p.ClassID order by p.ClassID, p.PONumber desc) as [No],
p.VendorID, p.VendorName, p.ClassName, p.PurchaseOrderID, p.PONumber, p.PODate, p.StatusID, s.Status, t.TotalOrderQty, p.GrandTotal
from dbo.TblPurchaseOrder p 
inner join dbo.TblStatus s on s.StatusID = p.StatusID
cross apply(
  select 
    sum(pd.QTY) as TotalOrderQty
  from dbo.TblPurchaseOrderDetail pd 
  where pd.PurchaseOrderID = p.PurchaseOrderID
) as t
where p.Type = 'PurchaseOrder'";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="p.SystemID", Value =html.SystemID },
            };
            html.GH.SQL = sql;
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right", Width="80px" },
                new GridField() { Header = "Class Name", FieldName = "ClassName", IsGroupByField = true },
                new GridField() { Header = "PO. Date", FieldName = "PODate", FormatDateTime = "dd-MMM-yyyy", ClassName = "text-center", Width="120px"},
                new GridField() { Header = "PO. Number", FieldName = "PONumber", OnClickFunction="ShowPOInvoice(#=VendorID#,#=PurchaseOrderID#,'#=PONumber#');", Width="120px" },
                new GridField() { Header = "Vendor", FieldName = "VendorName", Width="300px"},
                new GridField() { Header = "Status", FieldName = "Status", TextAlign ="center", Width="100px",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string>
                    {
                        { "Approve", "badge-primary" },
                        { "Disapprove", "badge-danger" },
                        { "Checked", "badge-info" },
                        { "Draft", "badge-danger" },
                    },
                },
                new GridField() { Header = "Order QTY", FieldName = "TotalOrderQty", ClassName="text-right", Width="110px" },
                new GridField() { Header = "Amount", FieldName = "GrandTotal", ClassName="text-right", Format = "#,##.00", Width="110px" },
                new GridField() { Header = "Action", FieldName = "PurchaseOrderID", TextAlign = "Center",  IsModifyField = true, Width="110px",
                    DeleteWithTableName="TblPurchaseOrder, TblPurchaseOrderDetail, TblBills",
                    HideEditOnActionNameNotMatch = AdminEdit==false,
                    ShowActionNameOnConditionField ="StatusID",
                    ShowActionNameOnConditionValue="60, 91",
                    HideDeteleOnNotMatch=true
                }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult CreatePurchaseOrder(string PrimaryFieldValue, string Type = "", bool Clone = false, int ItemID = 0, string ItemName = "")
        {
            ViewBag.ItemName = ItemName;
            ViewBag.ItemID = ItemID;
            ViewBag.Clone = Clone;
            ViewBag.Type = Type;
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.VendorName = "";

            HTML html = new HTML();
            ViewBag.SystemId = html.SystemID;
            html.isCloned = Clone;
            html.FormID = "CreatePurchaseOrder";
            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";
            string vendor = "";
            string vendorID = "0";
            string StatusID = "60";
            string sql;
            if (Type == "NewPO")
            {
                sql = "select v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName from dbo.TblVendor v ";
                sql += "where v.VendorID = " + PrimaryFieldValue;
                sql += " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
                html.PrimaryFieldValue = "0";
                DataSet dsv = html.LoadData(sql);
                vendor = dsv.Tables[0].Rows[0]["VendorName"].ToString();
                vendorID = dsv.Tables[0].Rows[0]["VendorID"].ToString();
                ViewBag.VendorName = vendor;
            }
            else if (PrimaryFieldValue != "0")
            {
                html.PrimaryFieldValue = PrimaryFieldValue;
                sql = "select p.StatusID, v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName from dbo.TblVendor v ";
                sql += "inner join dbo.TblPurchaseOrder p on p.VendorID = v.VendorID ";
                sql += "where p.PurchaseOrderID = " + html.PrimaryFieldValue.ToString();
                sql += " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID;
                DataSet dsv = html.LoadData(sql);
                vendor = dsv.Tables[0].Rows[0]["VendorName"].ToString();
                vendorID = dsv.Tables[0].Rows[0]["VendorID"].ToString();
                StatusID = dsv.Tables[0].Rows[0]["StatusID"].ToString() == "91" ? "60" : dsv.Tables[0].Rows[0]["StatusID"].ToString();
            }
            if (Clone) StatusID = "60";
            ViewBag.StatusID = StatusID;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblPurchaseOrder",
                    FormTitle="Purchase Order Information",
                    GetIDENTITYByFiled="PurchaseOrderID",
                    PrimaryFieldName="PurchaseOrderID",
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
                            DBFieldName="VendorID", Value="Vendor_Name_VendorID", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VendorName", Value="Vendor_Name_Reference_VendorName", GetValueFromInputName= true
                        },new DBField()
                        {
                            DBFieldName="WarehouseName", Value="TxtContactName_WarehouseName", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SubTotal", Value="SubTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VAT", Value="VAT", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Total", Value="Total", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountP", Value="TxtDisP", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountUSD", Value="TxtDisUSD", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="GrandTotal", Value="GrandTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Deposit", Value="TxtDeposit", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Balance", Value="TxtBalance", GetValueFromInputName= true, IsNumber = true
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
                            DBFieldName="BankInformation", Value="BankInfo", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Type", SetValue="PurchaseOrder"
                        }
                    },
                    SQL="update dbo.TblPurchaseOrder set CheckVerifyDate = NULL, ApprovalDate = NULL, CheckVerifyBy = '', CheckVerifyByUserID=0, ApproveBy='', ApproveByUserID=0, DraftPOStatusID = 60, POStatus = 60, StatusID = 60 where PurchaseOrderID = @PurOrderID",
                    UpdateFiledName="PurOrderID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="PO Status", ID="POStatus", Required=true,
                            DBField="POStatus", Value="0", IsReadOnly = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="PurchaseOrderID", ID="PurOrderID", Required=true,
                            Value=PrimaryFieldValue.ToString(), IsReadOnly = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Vendor Name:", ID="Vendor_Name", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            HiddenField="VendorID,Reference_VendorName",
                            Value=vendor,
                            HiddenFieldValue=vendorID+",0",
                            SQL="select VendorID VendorID, (VendorCode + ' - ' + VendorName) VendorName, VendorName Reference_VendorName from dbo.TblVendor where (Status = 'Enable' or Status is null or Status = '')  and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            isTypeahead = true,
                            FieldName="VendorName",
                            TypeAheadAddNewFunctionName="AddNewVendor()",
                            CallBackFunction="LoadVendorBankInfo",
                        },
                        new HtmlTextBox()
                        {
                            Label="Class Name", ID="ClassName", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ClassName", isTypeahead = true,
                            SQL="select ClassID, ClassName, PrefixPO from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            HiddenField="ClassID",
                            SQLCallBack="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = PrefixPO, @Module = 'PO' from dbo.TblClass where ClassID = @ClassID;exec [SP_SHOW_PREFIX] @Prefix, @Module, "+html.SystemID+", "+html.WarehouseID+", '"+html.DomainID+"';",
                            CallBackToControlID="PONumber, PrefixPO", UpdatePrefixModule="PO", Prefix="PrefixPO", SetPrefixToDbFieldName="PONumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="PO Number", ID="PONumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            isCloned=Clone,
                            DBField="PONumber", IsReadOnly = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Ref no.", ID="Reference",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Reference", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="PO Date", ID="PODate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PODate",
                            SingleDatePicker = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Request By", ID="RequestBy",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RequestBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedToCheckUserID, u.TelegramUserName, u.FullName RequestBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                        },
                        new HtmlTextBox()
                        {
                            Label="Draft Check By", ID="DraftCheckBy",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="DraftCheckBy", isTypeahead = true,
                            SQL="select u.UserLoginID DraftCheckUserID, u.TelegramUserName DraftCheckUsername, u.FullName DraftCheckBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="DraftCheckUsername, DraftCheckUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto",
                            Required = html.SystemID != "27" || html.WarehouseID != "25",
                        },
                        new HtmlTextBox()
                        {
                            Label="Check By", ID="NeedToCheckBy",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedToCheckBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedToCheckUserID, u.TelegramUserName CheckUsername, u.FullName NeedToCheckBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="CheckUsername, NeedToCheckUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto",
                           Required = html.SystemID != "27" || html.WarehouseID != "25",
                        },
                        new HtmlTextBox()
                        {
                            Label="Approval By", ID="NeedToApproval",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedToApproval", isTypeahead = true,
                            SQL="select u.UserLoginID ApproveByUserID, u.TelegramUserName, u.FullName NeedToApproval, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="TelegramUserName, ApproveByUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto",
                            Required = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Estimated arrival", ID="DeliveryDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="DeliveryDate",
                            SingleDatePicker = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Deliver To", ID="DeliverTo", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="DeliverTo", isTypeahead = true,
                            SQL="select uw.WID, uw.WarehouseName DeliverTo, uw.WarehouseName WName, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " order by uw.WarehouseName",
                            HiddenField="WID",
                            TypeAheadAddNewFunctionName="AddNewWarehouse()",
                            DisplayOnFields= JsonConvert.SerializeObject(new { TxtContactName="ContactName", TxtContactPhone="ContactPhone",TxtContactName_WarehouseName="WName"})
                        },
                        new HtmlTextBox()
                        {
                            Label="Contact Name", ID="TxtContactName", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ContactName", HiddenField="WarehouseName"
                        },
                        new HtmlTextBox()
                        {
                            Label="Contact Phone", ID="TxtContactPhone", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ContactPhone"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Payment Terms", ID="PaymentTerms", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PaymentTerms", isTypeahead = true,
                            SQL="select convert(varchar, NumberofDays) NumberofDays, TermName PaymentTerms from dbo.TblPaymentTerms where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" union select '0' NumberofDays, 'Due on Receipt' PaymentTerms order by PaymentTerms",
                            HiddenField="NumberofDays", Value="Due on Receipt", HiddenFieldValue = "0",
                            DivClassCol="col-sm",TypeAheadAddNewFunctionName="AddNewPaymentTerms()",
                        },
                        new HtmlTextBox()
                        {
                            Label="Payment Due Date", ID="PaymentDueDate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PaymentDueDate",
                            SingleDatePicker = true,DivClassCol="col-sm"
                        },
                        new HtmlTextBox()
                        {
                            Label="Tax", ID="Tax",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Tax", isTypeahead = true,
                            SQL="select Rate, TaxName + ' [' + convert(varchar, format(Rate, '#')) + '%]' Tax from dbo.TblTax where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by TaxName",
                            HiddenField="Rate", HiddenFieldValue="0", DivClassCol="col-sm"
                        },
                        new HtmlTextBox()
                        {
                            Label="Status", ID="TxtStatusID", Value = StatusID, Hide = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="StatusID", IsReadOnly=true,DivClassCol="col-sm-auto",
                        },
                        new HtmlTextBox()
                        {
                            Label="Need To Deposit", ID="NeedToDeposit",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedToDeposit",
                            DivClassCol="col-sm",
                        },
                    }
                },
                new  FormData() {
                    TableName="TblPurchaseOrderDetail",
                    GetIDENTITYByFiled="PurchaseOrderID",
                    PrimaryFieldName="PurchaseOrderID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                             //ScriptCallBackFunction= html.SystemID == "27"?"CallSetAutoComplete_ItemName, Reload_UM(), calculateTotals()":"CallSetAutoComplete_ItemName, calculateTotals()",
                            ScriptCallBackFunction=html.SystemID == "27"?"Reload_UM() ,TotalAll()":"CallSetAutoComplete_ItemName,TotalAll()",
                            //ScriptCallBackFunction="calcSubtotal(), Reload_iUM()"  // ← Notice "Reload_iUM()"
                            //ScriptCallBackFunction="CallSetAutoComplete_ItemName,TotalAll()",//,ApplySummernote('summernote'),Reload_UM,Reload_AccountName
                            FooterColspan=2,
                            //SubTotalColspan=5,
                            SubTotalColspan= html.SystemID == "27" ? 6:5,
                            CustomerNotesLabel="Remark",
                            ShowBankInfo= true,
                            ShowDeposit = false,
                            ShowBalance = false,
                            GetIDENTITYByFiled="PurchaseOrderID",
                            PrimaryFieldName="PurchaseOrderID",
                            TableName="TblPurchaseOrderDetail",
                            OrderBy="PurchaseOrderDetailID",
                            ReturnFieldNameValue="PurchaseOrderID, VendorID, PONumber, VendorName, NeedToApproval, TelegramUserName",
                            ReturnFieldNameValueFromTblName = "TblPurchaseOrder",
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
                                //new DBField()
                                //{
                                //    DBFieldName="ItemDescription", Value="ItemName_Description", GetValueFromTextAreaName = true  //ISSummerNote = true
                                //},
                                new DBField()
{
                                    DBFieldName="ItemDescription",
                                    Value = html.SystemID == "27" && html.WarehouseID == "25" ? "ItemName" : "ItemName_Description",
                                    GetValueFromTextAreaName = true
                                },
                                new DBField()
                                {
                                    DBFieldName="PONumber", SetValue="$(\"#PONumber\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="PODate", SetValue="StringToDate($(\"#PODate\").val())"
                                },
                                new DBField()
                                {
                                    DBFieldName="DeliveryDate", SetValue="StringToDate($(\"#DeliveryDate\").val())"
                                },
                                new DBField()
                                {
                                    DBFieldName="StatusID", SetValue="60"
                                },
                                new DBField()
                                {
                                    DBFieldName="DeliverTo", SetValue="$(\"#DeliverTo\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="RequestBy", SetValue="$(\"#RequestBy\").val()"
                                },
                            },
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
                                    Class="col-1", Label="វិមាត្រ", Hide= html.SystemID != "27"
                                },
                                new TblTHead()
                                {
                                     Label="បរិមាណ បញ្ចាទិញ", Class="col-1 text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="ថ្លៃឯកតា", Class="col-1",
                                },
                                new TblTHead()
                                {
                                     Label="តម្លៃលក់", Class="col-1",
                                },
                                new TblTHead()
                                {
                                    Label="បញ្ចុះតម្លៃ", Class="col-1",
                                },
                                new TblTHead()
                                {
                                     Label="សរុប$", Class="col-1",
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
                                     Label="Dimension", Hide= html.SystemID!="27"
                                },
                                new TblTHead()
                                {
                                     Label="UOM", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Order QTY"
                                },
                                new TblTHead()
                                {
                                     Label="Unit Price", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Sale Price", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Dis$"
                                },
                                new TblTHead()
                                {
                                     Label="Amount$",
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

                                html.SystemID == "27" && html.WarehouseID == "25"
                                    ? new HtmlTextarea()
                                        {
                                            Name="ItemName",
                                            Class="input2 CustInfo",
                                            IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName','TotalAll');",
                                            DBField = "ItemDescription",
                                            //InputDescription = true,
                                            placeholder="Item Name",
                                            Required=true,
                                        }

                                    : (HTMLControl)new HtmlTextBox()
                                        {
                                            Name="ItemName",
                                            Required=true,
                                            Class="input2 CustInfo",
                                            IsCustomForm = true,
                                            DBField="ItemName",
                                            DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName','TotalAll');",
                                            ISAutoComplete = true,
                                            HiddenField="ItemID",
                                            AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete=@"select distinct top 10 i.ItemID, i.ItemName + case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end ItemName
                        from dbo.TblItem i 
                        inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID 
                        where lower(i.ItemName + case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end + case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID +" and i.DomainID like '%"+html.DomainID+@"%'",
                                            SQLCallBack ="select distinct iv.UM,iv.UomID, iv.CostPrice UnitPrice, iv.SellingPrice SalePrice, iv.ItemDescription ItemName_Description from dbo.TblItemVariant iv where iv.ItemID = @SearchText",
                                            DisplayOnFields="UM, UomID, UnitPrice, SalePrice, ItemName_Description",
                                            SQLTypeaheadCallBack = "select distinct ItemID, UomID, UM, CostPrice UnitPrice, SellingPrice SalePrice from dbo.TblItemVariant where ItemID = @ItemID ",
                                            DisplayOnFieldsTypeahead="UnitPrice, SalePrice, UomID",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="UM",
                                            FieldName = "ItemID",
                                            CallBackDisplayText="UM",
                                            TypeaheadCallBackFunction="TotalAll",
                                            InputDescription = true,
                                            placeholder="Item Name"
                                        },
                                         new HtmlTextBox()
                                        {
                                            Name="Dimension", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Dimension",
                                            placeholder="E.g., m * m * pcs",
                                            Hide= html.SystemID!="27"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM", Required = true,
                                            SQL = html.SystemID == "27" ? "select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]" : "",
                                            isTypeahead = html.SystemID == "27",
                                            HiddenField="UomID",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                            placeholder = "Enter QTY",
                                            OnKeyupFunction="TotalAll();", Digit="0.000"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UnitPrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            placeholder = "Enter Unit Price",
                                            DBField="UnitPrice", IsAllowInputOnlyNumber =true, Digit="0.000",
                                            OnChangeFunction="TotalAll();",OnKeyupFunction="TotalAll();",
                                            Value = "0"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="SalePrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            placeholder = "Enter Sale Price",
                                            DBField="SalePrice", IsAllowInputOnlyNumber =true,
                                            OnChangeFunction="TotalAll();",OnKeyupFunction="TotalAll();",
                                            Value = "0"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="DisUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                            placeholder = "Enter Discount",
                                            OnKeyupFunction="onChangeDisUSD(this);",
                                            HiddenField="DisP"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Amount", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Amount", IsAllowInputOnlyNumber =true,
                                            placeholder = "Enter Amount",
                                            OnKeyupFunction="AmountKeyup(this);"
                                        }
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },
                new FormData() {
                    TableName = "TblPurchaseOrder",
                    PrimaryFieldName = "PurchaseOrderID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlFile()
                        {
                            ID = "AttachmentFile",
                            Class = "input2 CustInfo", IsCustomForm = true,
                            DBField = "AttachmentFile",
                            AcceptImageOnly = true,
                            DestinationFile = "Uploads/Vendors/PurchaseOrder",
                            Multiple = true,
                            UploadFilesV2 = true,
                            TableName = "TblPurchaseOrder",
                            PrimaryFieldName = "PurchaseOrderID",
                            PrimaryFieldValue = PrimaryFieldValue
                        },
                    }
                },

            };
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult InvoicePO(int VendorID, int POID, string PONumber)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string sql = @"select SystemID, WarehouseID from TblPurchaseOrder where PurchaseOrderID = @POID";
            para.Add("POID", POID.ToString());
            DataSet dsSystem = html.LoadData(sql, false, para);

            string systid = dsSystem.Tables[0].Rows.Count > 0
                ? dsSystem.Tables[0].Rows[0]["SystemID"].ToString()
                : html.SystemID;

            string warehouseid = dsSystem.Tables[0].Rows.Count > 0
                ? dsSystem.Tables[0].Rows[0]["WarehouseID"].ToString()
                : html.WarehouseID;

            para.Clear();
            para.Add("VendorID", VendorID.ToString());
            para.Add("POID", POID.ToString());
            para.Add("SystemID", systid);
            para.Add("WarehouseID", warehouseid);

            sql = @"select CompanyName, CompanyAddress, Phone, Email
                    from dbo.TblCompany
                    where ISDefault = 1 and SystemID = @SystemID and WarehouseID = @WarehouseID;

                    select VendorName, [Address], ContactPersonName, ContactPhoneNumber, Phone HendPhone, Email BillingEmail
                    from dbo.TblVendor
                    where VendorID = @VendorID and SystemID = @SystemID and WarehouseID = @WarehouseID;

                    select p.PONumber, format(p.PODate, 'dd-MM-yyyy') PODate, format(p.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, v.VendorCode, format(p.GrandTotal, '#,#.00') GrandTotal,
                    format(p.SubTotal, '#,#0.00') SubTotal, format(p.VAT, '#,#0.00') VAT, format(p.Total, '#,#0.00') Total, format(p.DiscountP, '#,#0.00') DiscountP, format(p.DiscountUSD, '#,#0.00') DiscountUSD, 
                    p.Tax, format(p.Rate, '#') Rate, case when CustomerNotes <> '' then p.CustomerNotes else '' end CustomerNotes, p.TermsConditions, 
                    uw.[Address] + ', ' + uw.Commune + ', ' + uw.District + ', ' + uw.ProvinceCity DeliverTo
                    , p.ContactName, p.ContactPhone, p.StatusID,p.StockInStatusID,
                    case when (p.StatusID = 15 or p.StatusID = 66) then '<span class=""badge badge-warning"">' + s.[Status] + '</span>'
                    when p.StatusID in(60,91) then '<span class=""badge badge-danger"">' + s.[Status] + '</span>'
                    else '<span class=""badge badge-primary"">' + s.[Status] + '</span>'
                    end [Status], p.ClassName,p.ClassID, p.CreateBy, format(p.CreateDate, 'dd-MM-yyyy HH:mm:ss tt') CreateDate, p.WarehouseName, p.POStatus,
                    case when isnull(p.ApproveBy, '') <> '' then ul2.FullName else '' end ApproveBy,
                    --case when isnull(p.POStatus, 0) = 90 then (select Job from dbo.TblUserLogin where UserLoginID = p.ApproveByUserID) else p.ApproveBy end ApproveBy,
                    ul.FullName IssuedBy, format(p.ApprovalDate, 'dd-MM-yyyy HH:mm:ss tt') ApprovalDate, p.DisapproveBy, isnull(p.Deposit, 0) Deposit, isnull(p.Balance, 0) Balance, ul1.FullName CheckVerifyBy, format(p.CheckVerifyDate, 'dd-MM-yy HH:mm:ss tt') CheckVerifyDate, p.UserID,
                    p.DisapproveByUserID, p.ApproveByUserID, p.IssuedByUserID,p.NeedToCheckUserID, p.CheckVerifyByUserID, p.RequestBy, p.PaymentTerms, ul.Job IssuedByPosition, ul1.Job CheckVerifyByPosition, case when isnull(p.ApproveBy, '') <> '' then ul2.Job else '' end ApprovedByPosition, p.BankInformation, p.NeedToDeposit
                    from dbo.TblPurchaseOrder p
                    inner join dbo.TblUserLogin ul on ul.UserLoginID = p.IssuedByUserID
                    left join dbo.TblUserLogin ul1 on ul1.UserLoginID = p.CheckVerifyByUserID
                    left join dbo.TblUserLogin ul2 on ul2.UserLoginID = p.ApproveByUserID
                    inner join dbo.TblVendor v on v.VendorID = p.VendorID
                    inner join dbo.TblStatus s on s.StatusID = p.StatusID
                    left join dbo.TblUserWarehouse uw on uw.WID = p.WID
                    where p.PurchaseOrderID = @POID and p.SystemID = @SystemID and p.WarehouseID = @WarehouseID;
                    select pd.QTY, i.ItemName, isnull(i.SerialNumber, '') SerialNumber, pd.ItemDescription, format(pd.UnitPrice, '#,#0.000') UnitPrice, format(pd.DisP, '#,#.00') DisP, format(pd.DisUSD, '#,#0.00') DisUSD, format(pd.Amount, '#,#0.00') Amount, pd.UM, format(pd.SalePrice, '#,#0.00') SalePrice, 
                    isnull((select top 1 value from TblItemVariant iv
                    CROSS APPLY STRING_SPLIT(isnull(iv.ItemImage,''), '~') where iv.ItemID = pd.ItemID and iv.UM = pd.UM),'') ItemImage
                    from dbo.TblPurchaseOrderDetail pd
                    left join dbo.TblItem i on i.ItemID = pd.ItemID
                    where pd.PurchaseOrderID = @POID and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID 
                    order by pd.PurchaseOrderDetailID;
 
                    select 'Uploads/Items' as DestinationFolder, s.value as FileName  
                    from dbo.TblPurchaseOrder p
                    inner join dbo.TblPurchaseOrderDetail pd on p.PurchaseOrderID = pd.PurchaseOrderID
                    inner join dbo.TblItemVariant iv on iv.ItemID = pd.ItemID and iv.UM = pd.UM
                    cross apply string_split(iv.ItemImage, '~') s
                    where p.PurchaseOrderID = @POID
                    union all
                    select p.DestinationFolder, s.value as FileName
                    from dbo.TblPurchaseOrder p
                    cross apply string_split(p.AttachmentFile, '~') s
                    where p.PurchaseOrderID = @POID;";

            DataSet ds = html.LoadData(sql, false, para);
            ViewBag.Data = ds.Tables;
            string ClassID = ds.Tables[2].Rows[0]["ClassID"].ToString();
            sql = @"select * from dbo.TblClass where ClassID =" + ClassID;
            DataSet dsLogo = html.LoadData(sql);
            ViewBag.ClassLogo = dsLogo.Tables[0].Rows[0]["FileLogo"];
            var StatusID = ds.Tables[2].Rows[0]["StatusID"].ToString();
            var CreateByID = ds.Tables[2].Rows[0]["UserID"].ToString();
            var CheckVerifyByUserID = ds.Tables[2].Rows[0]["CheckVerifyByUserID"].ToString();
            var ApproveByUserID = ds.Tables[2].Rows[0]["ApproveByUserID"].ToString();
            para.Clear();
            para.Add("CreateByID", CreateByID);
            para.Add("CheckVerifyByUserID", CheckVerifyByUserID);
            para.Add("ApproveByUserID", ApproveByUserID);
            sql = @"select [Signature] from dbo.TblUserLogin where UserLoginID = @CreateByID; 
select [Signature] from dbo.TblUserLogin where UserLoginID = @CheckVerifyByUserID; 
select [Signature] from dbo.TblUserLogin where UserLoginID = @ApproveByUserID; 
";
            ds = html.LoadData(sql, false, para);

            string CreateBySignature;
            if (ds.Tables[0].Rows.Count > 0)
            {
                string[] s1 = ds.Tables[0].Rows[0]["Signature"].ToString().Split('~');
                CreateBySignature = s1[0];
            }

            else
                CreateBySignature = "";
            string CheckBySignature;
            if (ds.Tables[1].Rows.Count > 0)
            {
                string[] s2 = ds.Tables[1].Rows[0]["Signature"].ToString().Split('~');
                CheckBySignature = s2[0];
            }
            else
                CheckBySignature = "";
            string ApproveBySignature;
            if (ds.Tables[2].Rows.Count > 0 && StatusID == "90")
            {
                string[] s3 = ds.Tables[2].Rows[0]["Signature"].ToString().Split('~');
                ApproveBySignature = s3[0];
            }
            else
                ApproveBySignature = "";
            ViewBag.systemID = systid;
            ViewBag.CreateBySignature = CreateBySignature;
            ViewBag.CheckBySignature = CheckBySignature;
            ViewBag.ApproveBySignature = ApproveBySignature;
            ViewBag.PrintDate = DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt");
            return View();
        }
        public JsonResult LoadItem()
        {
            HTML html = new HTML();
            string SQL = "select i.ItemID, i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName, i.SellingPrice, i.ItemDescription from dbo.TblItem i where i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " order by i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadItemDetail(int ItemID)
        {
            HTML html = new HTML();
            string SQL = "select top 1 UM, isnull(i.CostPrice, 0) CostPrice, isnull(i.SellingPrice, 0) SellingPrice, c.ChartOfAccountID, (c.AccountNo + ' - ' + c.AccountName) AccountName, i.ItemDescription  from dbo.TblItem i inner join dbo.TblChartOfAccount c on c.ChartOfAccountID = i.PurchaseChartOfAccountID where i.ItemID = @ItemID and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID;
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ItemID", ItemID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadPONumber()
        {
            HTML html = new HTML();
            string SQL = "declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'PO' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + ";";
            SQL += "exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ", '" + html.DomainID + "'";
            return Json(new { data = html.JsonData(SQL, false) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadTermCondition()
        {
            HTML html = new HTML();
            string SQL = "select TermConditionNonVAT, TermConditionVAT from dbo.TblCompany where ISDefault = 1 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            return Json(new { data = html.JsonData(SQL, false) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadAccount()
        {
            HTML html = new HTML();
            string SQL = "select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName + ' (' + AccountType + ')') AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType";
            DataSet ds = html.LoadData(SQL);
            StringBuilder sb = new StringBuilder();
            if (ds.Tables[0].Rows.Count > 0)
            {
                string AccountType2 = "";
                sb.AppendLine("<script>");
                sb.AppendLine("function ApplyTypeAheadGroup(TypeAheadName) {");
                sb.AppendLine("var data = JSON.parse(" + html.JsonData(SQL, false) + ");");
                sb.AppendLine("data = JSON.parse(data);");
                sb.AppendLine("Account = data;");
                sb.AppendLine("$(\"input[name=AccountName]\").typeahead(");
                sb.AppendLine("{");
                sb.AppendLine("hint: true,");
                sb.AppendLine("highlight: true,");
                sb.AppendLine("minLength: 0");
                sb.AppendLine("},");
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string AccountType1 = dr["AccountType"].ToString();
                    if (AccountType1 != AccountType2)
                    {
                        sb.AppendLine("{");
                        sb.AppendLine("limit: 9999,");
                        sb.AppendLine("display: 'Name',");
                        sb.AppendLine("source: function (query, cb) {");
                        sb.AppendLine("var matches = [];");
                        sb.AppendLine("var Data = Account.filter(function (data) {");
                        sb.AppendLine("return data.AccountType === \"" + AccountType1 + "\";");
                        sb.AppendLine("});");
                        sb.AppendLine("$.each(Data, function (i, str) {");
                        sb.AppendLine("const value = str.AccountName;");
                        sb.AppendLine("const globalRegex = new RegExp(query, \"\\ig\");");
                        sb.AppendLine("if (globalRegex.test(value)) {");
                        sb.AppendLine("matches.push({ Name: str.AccountName, ID: str.ChartOfAccountID });");
                        sb.AppendLine("}");
                        sb.AppendLine("});");
                        sb.AppendLine("cb(matches);");
                        sb.AppendLine("},");
                        sb.AppendLine("templates: {");
                        sb.AppendLine("header: '<h5>" + AccountType1 + "</h5>'");
                        sb.AppendLine("}");
                        sb.AppendLine("},");
                    }
                    AccountType2 = dr["AccountType"].ToString();
                }
                sb.AppendLine("}");
                sb.AppendLine("</script>");
            }

            return Json(new { data = sb.ToString() }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReceiveNote(string PrimaryFieldValue, string Type = "", string PO_Number = "", bool Clone = false, string Status = "Stock In")
        {
            string BillType = Status;
            ViewBag.Status = Status;
            if (Status == "ConvertToBill")
                BillType = "Bill";
            ViewBag.Type = Type;
            ViewBag.VendorName = "";
            ViewBag.PONumber = PO_Number;
            HTML html = new HTML
            {
                isCloned = Clone,
                FormID = "CreateBills",
                FieldIDFocus = "VendorName",
                SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);"
            };
            string vendornamevalue = "";
            string vendorID = "0";
            string PurchaseOrderID = "0";
            string PONumber = "";
            string VendorDONumber = "";
            string VendorInvoiceNumber = "";
            string Attention = "";
            string sqlv;
            if (Type == "NewBill")
            {
                sqlv = "select v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName from dbo.TblVendor v ";
                sqlv += "where v.VendorID = " + PrimaryFieldValue;
                sqlv += " and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
                html.PrimaryFieldValue = "0";
                DataSet dsv = html.LoadData(sqlv);
                vendornamevalue = dsv.Tables[0].Rows[0]["VendorName"].ToString();
                vendorID = dsv.Tables[0].Rows[0]["VendorID"].ToString();
                ViewBag.VendorName = vendornamevalue;
            }
            else if (PrimaryFieldValue != "0")
            {
                html.PrimaryFieldValue = PrimaryFieldValue;
                sqlv = "select v.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName, p.PONumber, p.VendorDONumber, p.VendorInvoiceNumber, p.PurchaseOrderID, p.Attention from dbo.TblVendor v ";
                sqlv += "inner join dbo.TblBills p on p.VendorID = v.VendorID ";
                sqlv += "where p.BillID = " + html.PrimaryFieldValue.ToString();
                sqlv += " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID;
                DataSet dsv = html.LoadData(sqlv);
                vendornamevalue = dsv.Tables[0].Rows[0]["VendorName"].ToString();
                ViewBag.VendorName = vendornamevalue;
                vendorID = dsv.Tables[0].Rows[0]["VendorID"].ToString();
                PurchaseOrderID = dsv.Tables[0].Rows[0]["PurchaseOrderID"].ToString();
                PONumber = dsv.Tables[0].Rows[0]["PONumber"].ToString();
                VendorDONumber = dsv.Tables[0].Rows[0]["VendorDONumber"].ToString();
                VendorInvoiceNumber = dsv.Tables[0].Rows[0]["VendorInvoiceNumber"].ToString();
                Attention = dsv.Tables[0].Rows[0]["Attention"].ToString();
            }

            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Vendor Name:", ID="Vendor_Name", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            ISAutoComplete = true, HiddenField="VendorID",
                            Value=vendornamevalue, HiddenFieldValue=vendorID,
                            ISCallBack = true,
                            SQL_AutoComplete="select top 10 VendorID, (VendorCode + ' - ' + VendorName) VendorName from dbo.TblVendor where lower((VendorCode + ' - ' + VendorName)) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            SQLCallBack="select Attention from dbo.TblVendorBillingInformation b inner join dbo.TblVendor v on v.VendorID = b.VendorID where lower(v.VendorCode + ' - ' + v.VendorName) = @SearchText and b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID,
                            DisplayOnFields="Txt_Attention"
                        },
                        //new HtmlButton()
                        //{
                        //    ID="btnAddNewVendor", IsCustomForm = true,
                        //    TextValue="New Vendor", Event="onclick=\"AddNewVendor();\""
                        //}
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="PO Number", ID="PONumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 PONumber from dbo.TblPurchaseOrder where lower(PONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            CallBackFunction="OnPONumberChange",
                            Value = PONumber
                        },
                        new HtmlTextBox()
                        {
                            Label="PurchaseOrderID", ID="PurchaseOrderID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm-auto", Hide = true, Value=PurchaseOrderID
                        },
                        new HtmlTextBox()
                        {
                            Label="Attention (ATT)", ID="Txt_Attention",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Value= Attention
                        },
                        new HtmlTextBox()
                        {
                            Label="Type", ID="Type",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Value= BillType, Hide = true,
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Vendor DO Number", ID="VendorDONumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Value=VendorDONumber
                        },
                        new HtmlTextBox()
                        {
                            Label="Vendor Invoice Number", ID="VendorInvoiceNumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Value= VendorInvoiceNumber
                        }
                    }
                },
                new  FormData() {
                    TableName="TblBills",
                    FormTitle="Bills Information",
                    GetIDENTITYByFiled="BillID",
                    PrimaryFieldName="BillID",
                    ISAddAttachmentFile = true,
                    DestinationFile = "Uploads/Vendors/Bills",
                    SQL="update dbo.TblPurchaseOrder set StatusID = 64 where PurchaseOrderID = ",
                    UpdateFiledName="PurchaseOrderID",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="VendorID", Value="VendorID", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="PurchaseOrderID", Value="PurchaseOrderID", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="PONumber", Value="PONumber", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Attention", Value="Txt_Attention", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="VendorDONumber", Value="VendorDONumber", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="VendorInvoiceNumber", Value="VendorInvoiceNumber", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SubTotal", Value="SubTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VAT", Value="VAT", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Total", Value="Total", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountP", Value="TxtDisP", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountUSD", Value="TxtDisUSD", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="GrandTotal", Value="GrandTotal", GetValueFromLabelName= true, IsNumber = true
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
                            DBFieldName="Type", Value="Type", GetValueFromInputName= true
                        },
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Bill Number", ID="BillNumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BillNumber", IsReadOnly = true,
                            isCloned = Clone,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'Bills' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ", '"+html.DomainID+"'",
                            CallBackToControlID="BillNumber, PrefixBillNumber", UpdatePrefixModule="Bills", Prefix="PrefixBillNumber", SetPrefixToDbFieldName="BillNumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="Bill Date", ID="BillDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BillDate", DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Payment Terms", ID="PaymentTerms", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PaymentTerms", isTypeahead = true, DivClassCol="col-sm-auto",
                            SQL="select convert(varchar, NumberofDays) NumberofDays, TermName PaymentTerms from dbo.TblPaymentTerms where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" union select '0' NumberofDays, 'Due on Receipt' PaymentTerms order by PaymentTerms",
                            HiddenField="NumberofDays", Value="Due on Receipt"
                        },
                        new HtmlTextBox()
                        {
                            Label="Payment Due Date", ID="PaymentDueDate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PaymentDueDate", DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Warehouse", ID="Warehouse", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Warehouse", isTypeahead = true,
                            //SQL="select BranchID, BranchName BranchName, ContactName, ContactPhone, [Address] + case when Commune <> '' then ', ' + Commune end + case when District <> '' then ', ' + District end + case when ProvinceCity <> '' then ', ' + ProvinceCity end Warehouse from dbo.TblBranch where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" union select WID BranchID, WarehouseName BranchName, ContactName, ContactPhone, [Address] + case when Commune <> '' then ', ' + Commune end + case when District <> '' then ', ' + District end + case when ProvinceCity <> '' then ', ' + ProvinceCity end Warehouse from dbo.TblUserWarehouse where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID,
                            SQL="select uw.WID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName, uw.ContactName, uw.ContactPhone, uw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + case when uw.District <> '' then ', ' + uw.District end + case when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end Warehouse from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID,
                            HiddenField="WID", TypeaheadGroupByField="WarehouseName",
                            DisplayOnFields= JsonConvert.SerializeObject(new { TxtContactName="ContactName", TxtContactPhone="ContactPhone"})
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Contact Name", ID="TxtContactName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ContactName", DivClassCol="col-sm-2"
                        },
                        new HtmlTextBox()
                        {
                            Label="Contact Phone", ID="TxtContactPhone", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ContactPhone", DivClassCol="col-sm-2"
                        },
                        new HtmlTextBox()
                        {
                            Label="Class Name", ID="ClassName", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ClassName", isTypeahead = true,
                            SQL="select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            HiddenField="ClassID"
                        },
                        new HtmlTextBox()
                        {
                            Label="Tax", ID="Tax", DivClassCol="col-sm-2",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Tax", isTypeahead = true,
                            SQL="select Rate, TaxName + ' [' + convert(varchar, format(Rate, '#')) + '%]' Tax from dbo.TblTax where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by TaxName",
                            HiddenField="Rate", HiddenFieldValue="0"
                        }
                    }
                },
                new  FormData() {
                    TableName="TblBillsDetail",
                    GetIDENTITYByFiled="BillID",
                    PrimaryFieldName="BillID",

                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            FooterColspan = 3,
                            SubTotalColspan = 4,
                            ScriptCallBackGetDataTblEditFunction="getDateEdit",
                            GetIDENTITYByFiled="BillID",
                            PrimaryFieldName="BillDetailID",
                            TableName="TblBillsDetail",
                            ReturnFieldNameValue="BillID, VendorID, BillNumber",
                            ReturnFieldNameValueFromTblName = "TblBills",
                            UpdateValueFiledName="ItemDescription",
                            UpdateValueFiledNameToTblName="TblPurchaseInfo",
                            UpdateValueFiledNameConditionFiledName="ItemID",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", Value="_ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemDescription", Value="ItemDescription"
                                },
                                new DBField()
                                {
                                    DBFieldName="ChartOfAccountID", Value="_ChartOfAccountID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemExpirationDate", Value="ItemExpirationDate", IsDate = true,
                                },
                                new DBField()
                                {
                                    DBFieldName="Type", Value="_Type"
                                },
                            },
                            ScriptCallBackFunction="Reload_ItemName",
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
                                     Class="col-3", Label="គណនេយ្យ"
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
                                     Label="ថ្លៃឯកតា"
                                },
                                new TblTHead()
                                {
                                     Class="col-2", Label="បញ្ចុះតម្លៃ", colspan=2
                                },
                                new TblTHead()
                                {
                                     Label="សរុប$", Class="col-1",
                                     CloseTR="</tr>"
                                },
                                //new TblTHead()
                                //{
                                //     Label="", Class="col-1",
                                //     CloseTR="</tr>"
                                //},
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
                                     Label="Account"
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
                                     Label="Unit Price", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Dis%"
                                },
                                new TblTHead()
                                {
                                     Label="Dis$"
                                },
                                new TblTHead()
                                {
                                     Label="Amount$",
                                     CloseTR="</tr>"
                                },
                                //new TblTHead()
                                //{
                                //     Label="",
                                //     CloseTR="</tr>"
                                //}
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
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemName", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName','TotalAll');",
                                            isTypeahead = true, HiddenField="ItemID",
                                            //SQL="select ItemID, (case when Barcode <> '' then Barcode + ' - ' else '' end + ItemName) ItemName from dbo.TblItem where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                                            //SQLCallBack="select UM, p.CostPrice from dbo.TblItem i inner join dbo.TblPurchaseInfo p on p.ItemID = i.ItemID where i.ItemID = @ItemID and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                                            //CallBackToControlID="UM, UnitPrice"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="AccountName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="AccountName", DivClass="wrap-input3",
                                            isTypeahead = true, HiddenField="ChartOfAccountID",
                                            //SQL="select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                                            //TypeaheadGroupByField = "AccountType"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="OrderQTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="OrderQTY", Value = "0", IsAllowInputOnlyNumber =true,
                                            IsReadOnly = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                            Hide = Status == "ConvertToBill"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UnitPrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UnitPrice", IsAllowInputOnlyNumber =true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisP", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="DisP", Value = "0", IsAllowInputOnlyNumber =true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="DisUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Amount", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Amount", IsAllowInputOnlyNumber =true,
                                            HiddenField="Type", HiddenFieldValue= BillType
                                        }
                                    },
                                    //TDAlign="center",
                                    //TDDelete="<div class=\"btn-group\"><a name=\"Delete\" onclick=\"RemoveTableRow('TRNo', 'TDNo', this,'','TxtItemDescription');\" class=\"btn btn-danger btn-sm\" href=\"javascript:void(0);\" title=\"Delete\" alt=\"Delete\"><i class=\"fa fa-trash\"></i></a></div>",
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();

            //string SQL = "select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType";
            //DataSet ds = html.LoadData(SQL);
            //StringBuilder sb = new StringBuilder();
            //if (ds.Tables[0].Rows.Count > 0)
            //{
            //    string AccountType1 = "", AccountType2 = "";
            //    sb.AppendLine("<script>");
            //    sb.AppendLine("function ApplyTypeAheadGroup(TypeAhead, Index) {");
            //    sb.AppendLine("$(\"input[name=\"+TypeAhead+\"]\").eq(Index).typeahead(\"destroy\");");
            //    sb.AppendLine("var data = " + html.JsonData(SQL, false) + ";");
            //    sb.AppendLine("var Account = data;");
            //    sb.AppendLine("$(\"input[name=\"+TypeAhead+\"]\").eq(Index).typeahead(");
            //    sb.AppendLine("{");
            //    sb.AppendLine("hint: true,");
            //    sb.AppendLine("highlight: true,");
            //    sb.AppendLine("minLength: 0");
            //    sb.AppendLine("},");
            //    foreach (DataRow dr in ds.Tables[0].Rows)
            //    {
            //        AccountType1 = dr["AccountType"].ToString();
            //        if (AccountType1 != AccountType2)
            //        {
            //            sb.AppendLine("{");
            //            sb.AppendLine("limit: 9999,");
            //            sb.AppendLine("display: 'Name',");
            //            sb.AppendLine("source: function (query, cb) {");
            //            sb.AppendLine("var matches = [];");
            //            sb.AppendLine("var Data = Account.filter(function (data) {");
            //            sb.AppendLine("return data.AccountType === \"" + AccountType1 + "\";");
            //            sb.AppendLine("});");
            //            sb.AppendLine("$.each(Data, function (i, str) {");
            //            sb.AppendLine("const value = str.AccountName;");
            //            sb.AppendLine("const globalRegex = new RegExp(query, \"\\ig\");");
            //            sb.AppendLine("if (globalRegex.test(value)) {");
            //            sb.AppendLine("matches.push({ Name: str.AccountName, ID: str.ChartOfAccountID });");
            //            sb.AppendLine("}");
            //            sb.AppendLine("});");
            //            sb.AppendLine("cb(matches);");
            //            sb.AppendLine("},");
            //            sb.AppendLine("templates: {");
            //            sb.AppendLine("header: '<div class=\"border-bottom p-2 border-top typeahead-head\">" + AccountType1 + "</div>'");
            //            sb.AppendLine("}");
            //            sb.AppendLine("},");
            //        }
            //        AccountType2 = dr["AccountType"].ToString();
            //    }
            //    sb.AppendLine(").bind(\"typeahead:selected\", function (obj, data, name) {");
            //    sb.AppendLine("$(\"input[name=_ChartOfAccountID]\").eq(Index).val(data.ID);");
            //    sb.AppendLine("});");
            //    sb.AppendLine("$(\"input[name=\"+TypeAhead+\"]\").on(\"click\", function () {");

            //    //sb.AppendLine("$(\"div[name=divmenu\"+TypeAhead+\"]\").remove();");
            //    //sb.AppendLine("$(\"div.tt-menu\").prepend(`<div name=divmenu`+TypeAhead+` onclick=\"alert('ok')\" class=\"tt-suggestion tt-selectable\"><a href=\"javascript:void(0);\"><i class=\"fa fa-plus\"></i> Add New</a></div>`);");

            //    sb.AppendLine("$(this).select();");
            //    sb.AppendLine("var index = $(this).closest(\"tr\").index();");
            //    sb.AppendLine("$(\"input[name=\"+TypeAhead+\"]\").eq(index).typeahead(\"open\");");
            //    sb.AppendLine("})");
            //    sb.AppendLine("}");
            //    sb.AppendLine("</script>");
            //}
            //ViewBag.AccountTypeGroup = sb.ToString();
            return View();
        }
        public ActionResult PurchaseOrderInof(string PrimaryFieldValue)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            ViewBag.VendorID = PrimaryFieldValue;
            string sqlPayee = @"SELECT v.VendorID, v.VendorName
FROM dbo.TblVendor v
inner join dbo.TblPurchaseOrder po on po.VendorID= v.VendorID
where v.SystemID=" + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + @"
group by v.VendorID, v.VendorName
ORDER BY v.VendorID,v.VendorName COLLATE SQL_Latin1_General_CP1_CI_AS ASC;";

            html.BunSearch = "btnserachpo";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "po.PODate"
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Vendor Name",
                SQL = sqlPayee,
                RadioValue = EncryAndDecry.Encryp("v.VendorID"),
                SelectValue = "VendorID",
                DisplayField = "VendorName",
                ShowAll = false,
                DefaultCheckBok = true
            });

            string sql = @"select distinct po.PurchaseOrderID, v.VendorID, v.VendorName, po.PONumber, po.PODate, po.GrandTotal
from dbo.TblPurchaseOrder po
inner join dbo.TblVendor v on v.VendorID = po.VendorID where v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " and v.DomainID like '%" + html.DomainID + @"%'";
            string sql2 = @"select distinct v.VendorID,po.PONumber,
i.ItemName + case when isnull(iv.ItemCode,'') <> '' then '-' + iv.ItemCode else '' end
+ case when ISNULL(s.SerialNumber,'') <> '' then '-' + s.SerialNumber else '' end ItemName,
pd.UM, pd.QTY, pd.Amount, po.PurchaseOrderID
from dbo.TblPurchaseOrder po
inner join dbo.TblVendor v on v.VendorID = po.VendorID
inner join dbo.TblPurchaseOrderDetail pd on pd.PurchaseOrderID = po.PurchaseOrderID
inner join dbo.TblItem i on i.ItemID = pd.ItemID
left join dbo.TblItemVariant iv on iv.ItemID = i.ItemID
left join dbo.TblItemSerialDetail s on s.ItemID = i.ItemID
where po.SystemID = " + html.SystemID + " and po.WarehouseID = " + html.WarehouseID + " and po.DomainID like '%" + html.DomainID + "%'";

            html.IsHaveWhereCondition = true;
            html.OrderBy = "v.VendorName, po.PONumber desc";
            html.ShowPaging = false;
            html.GH.SQL = sql;
            html.TblID = "tblpo";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Vendor Name", FieldName = "VendorName", IsGroupByField = true },
                new GridField() { Header = "PurchaseOrderID", FieldName = "PurchaseOrderID", DisplayCheckBox = true, TextAlign="center" },
                new GridField() { Header = "PO Number", FieldName = "PONumber", TextAlign="center" },
                new GridField() { Header = "PO Date", FieldName = "PODate", FormatDateTime="dd-MM-yyyy" },
                new GridField() { Header = "GrandTotal", FieldName = "GrandTotal", TextAlign = "right", Format="#,#0.00" },
            };
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                KendoGridStickyColumn = false,
            };
            html.DefaultSearch = false;

            HTML html2 = new HTML(); html.IsHaveWhereCondition = true;
            html2.DefaultSearch = false;
            html2.ShowPaging = false;
            html.OrderBy = "po.PONumber desc";
            html2.TblID = "tblpodetail";
            html2.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "PO Number", FieldName = "PONumber", TextAlign="center" },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                new GridField() { Header = "UOM", FieldName = "UM", TextAlign="center" },
                new GridField() { Header = "QTY", FieldName = "QTY", TextAlign="right" },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign = "right", Format="#,#0.00" },
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "loadPO", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString(), html2.GH.GridTable.ToList().ToJsonString(), sql2, "PurchaseOrderID");
            return View();
        }
        public ActionResult ReceiveNoteInof(string PrimaryFieldValue)
        {

            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            ViewBag.VendorID = PrimaryFieldValue;
            string sqlPayee = @"SELECT v.VendorID, v.VendorName
FROM dbo.TblVendor v
inner join dbo.TblStockIn si on si.VendorID= v.VendorID
where v.SystemID=" + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + @"
group by v.VendorID, v.VendorName
ORDER BY v.VendorID,v.VendorName COLLATE SQL_Latin1_General_CP1_CI_AS ASC;";

            html.BunSearch = "btnserachpo";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "po.PODate"
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Vendor Name",
                //SQL = "select distinct v.VendorID, v.VendorName from dbo.TblVendor v\r\ninner join dbo.TblPurchaseOrder po on po.VendorID = v.VendorID where v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " order by v.VendorName",
                SQL = sqlPayee,
                RadioValue = EncryAndDecry.Encryp("v.VendorID"),
                SelectValue = "VendorID",
                DisplayField = "VendorName",
                ShowAll = false,
                DefaultCheckBok = true
            });

            string sql = @"select distinct si.StockInID, v.VendorID, v.VendorName, si.ReceiveNoteNumber, si.ReceiveDate, isnull(po.GrandTotal, 0) GrandTotal
from dbo.TblStockIn si
inner join dbo.TblVendor v on v.VendorID = si.VendorID 
left join dbo.TblPurchaseOrder po on po.PurchaseOrderID = si.PurchaseOrderID
where v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " and v.DomainID like '%" + html.DomainID + @"%'";
            string sql2 = @"select distinct v.VendorID, si.ReceiveNoteNumber,
i.ItemName + case when isnull(iv.ItemCode,'') <> '' then '-' + iv.ItemCode else '' end
+ case when ISNULL(s.SerialNumber,'') <> '' then '-' + s.SerialNumber else '' end ItemName,
sd.UM, sd.QTY, isnull(pd.Amount,0) Amount
from dbo.TblStockInDetail sd
inner join dbo.TblStockIn si on si.StockInID = sd.StockInID
inner join dbo.TblVendor v on v.VendorID = si.VendorID
inner join dbo.TblItem i on i.ItemID = sd.ItemID
left join dbo.TblPurchaseOrderDetail pd on pd.PurchaseOrderID = si.PurchaseOrderID and pd.ItemID = i.ItemID
left join dbo.TblItemVariant iv on iv.ItemID = i.ItemID
left join dbo.TblItemSerialDetail s on s.ItemID = i.ItemID
where isnull(si.ReceiveNoteNumber,'') <> '' and sd.[Type] <> 'Stock Out' and sd.SystemID = " + html.SystemID + " and sd.WarehouseID = " + html.WarehouseID + " and sd.DomainID like '%" + html.DomainID + "%'";

            html.IsHaveWhereCondition = true;
            html.OrderBy = "v.VendorName, po.PONumber desc";
            html.ShowPaging = false;
            html.GH.SQL = sql;
            html.TblID = "tblpo";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Vendor Name", FieldName = "VendorName", IsGroupByField = true },
                new GridField() { Header = "StockInID", FieldName = "StockInID", DisplayCheckBox = true, TextAlign="center" },
                new GridField() { Header = "Receive Note#", FieldName = "ReceiveNoteNumber", TextAlign="center" },
                new GridField() { Header = "Receive Date", FieldName = "ReceiveDate", FormatDateTime="dd-MM-yyyy" },
                new GridField() { Header = "GrandTotal", FieldName = "GrandTotal", TextAlign = "right", Format="#,#0.00" },
            };
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                KendoGridStickyColumn = false,
            };

            HTML html2 = new HTML(); html.IsHaveWhereCondition = true;
            html2.ShowPaging = false;
            html2.DefaultSearch = false;
            html.OrderBy = "si.ReceiveNoteNumber desc";
            html2.TblID = "tblpodetail";
            html2.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Receive Note#", FieldName = "ReceiveNoteNumber", TextAlign="center" },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                new GridField() { Header = "UOM", FieldName = "UM", TextAlign="center" },
                new GridField() { Header = "QTY", FieldName = "QTY", TextAlign="right" },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign = "right", Format="#,#0.00" },
            };
            html.DefaultSearch = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "loadPO", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString(), html2.GH.GridTable.ToList().ToJsonString(), sql2, "ReceiveNoteNumber");

            return View();
        }
        public JsonResult CheckPOHasStockIN(int POID)
        {
            HTML html = new HTML();
            string sql = "";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"POID", POID.ToString() },
            };
            if (POID > 0)
            {
                sql = @"select coalesce(count(si.StockInID),0) as [qty], isnull(po.IsClose, 0) IsClose, isnull(po.IsCancel, 0) IsCancel
                    from dbo.TblPurchaseOrder po
                    left join dbo.TblStockIn si on po.PurchaseOrderID = si.PurchaseOrderID 
                    where po.PurchaseOrderID = @POID
                    group by po.IsClose, po.IsCancel";
            }

            var result = html.JsonData(sql, false, para);
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<JsonResult> SendNotificationToTelegram(int PurchaseOrderID)
        {
            var html = new HTML();
            var result = new SqlResult();

            string sql = "SELECT * FROM dbo.TblPurchaseOrder WHERE PurchaseOrderID = @PurchaseOrderID";
            var para = new Dictionary<string, string> { { "@PurchaseOrderID", PurchaseOrderID.ToString() } };
            var ds = html.LoadData(sql, false, para);
            var dr = ds.Tables[0].Rows[0];

            bool isDraft = !string.IsNullOrEmpty(dr["DraftCheckBy"]?.ToString());
            bool isNeedCheck = !string.IsNullOrEmpty(dr["NeedToCheckBy"]?.ToString());

            string approverId = isNeedCheck ? dr["NeedToCheckUserID"]?.ToString() : dr["ApproveByUserID"]?.ToString();
            string fullName = isNeedCheck ? dr["NeedToCheckBy"]?.ToString() : dr["NeedToApproval"]?.ToString();
            string username = isNeedCheck ? dr["CheckUsername"]?.ToString() : dr["TelegramUserName"]?.ToString();
            string preparedBy = dr["CreateBy"]?.ToString();
            string referenceNo = dr["PONumber"]?.ToString();
            string systemID = dr["SystemID"]?.ToString();
            string vendorID = dr["VendorID"]?.ToString();
            string action = isNeedCheck ? "check" : "approve";
            string recipient = int.Parse(approverId) == 38 ? "Boss" : fullName;
            string botToken = html.GetBotTokenBySystemID(int.Parse(systemID));

            var msg = new StringBuilder();
            var notification = new NotificationTelegram();
            var inlineKeyboard = new InlineKeyboardMarkup();

            if (isDraft)
            {
                username = dr["DraftCheckUsername"]?.ToString();
                recipient = dr["DraftCheckBy"]?.ToString();

                msg.AppendLine($"{username}\n");
                msg.AppendLine($"🎯 NEW Draft PO.\n");
                msg.AppendLine($"<i><b>Prepared by: {preparedBy}</b></i>.\n");
                msg.AppendLine($"Please kindly review and take action, Thanks you.");

                inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"ApproveDraftPO~{PurchaseOrderID}~{username}"),
                        InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisapproveDraftPO~{PurchaseOrderID}~{username}"),
                    }
                });
            }
            else
            {
                msg.AppendLine($"{username}\n");
                msg.AppendLine($"Dear {recipient},\n");
                msg.AppendLine($"Now you have a request need to {action}.\n");
                msg.AppendLine($"<i><b>Prepared by: {preparedBy}</b></i>.\n");
                msg.AppendLine($"Please kindly review and take action, Thanks you.");

                inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"ApprovePO~{PurchaseOrderID}~{username}"),
                        InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisapprovePO~{PurchaseOrderID}~{username}"),
                    }
                });

                if (isNeedCheck)
                {
                    inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Check ✍️",    callbackData: $"CheckPO~{PurchaseOrderID}~{username}"),
                            InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisCheckPO~{PurchaseOrderID}~{username}"),
                        }
                    });
                }
            }

            sql = @"SELECT * 
                FROM dbo.TblTelegramGroupNotification
                WHERE [Module] = 'PO' AND StatusID = @StatusID AND SystemID = @SystemID";
            para.Clear();
            para.Add("@StatusID", isDraft ? "60" : isNeedCheck ? "92" : "90");
            para.Add("@SystemID", html.SystemID);
            ds = html.LoadData(sql, false, para);
            var row = ds.Tables[0].Rows[0];

            long chatId = long.Parse(row["TelegramGroupID"]?.ToString());

            var pdfResult = await GeneratePDF(vendorID, PurchaseOrderID, referenceNo);

            var sent = await notification.NotificationTelegramBot(
                "",
                chatId: chatId,
                PDFFile: pdfResult?.OutPutFile,
                Caption: msg.ToString(),
                replyMarkupInlineKeyboardMarkup: true,
                inlineKeyboard: inlineKeyboard,
                newBotClient: botToken
            );

            if (sent.Result == ResultType.Successful)
            {
                System.IO.File.Delete(pdfResult.OutPutFile);

                sql = @"update dbo.TblPurchaseOrder set TelegramMessageID = @TelegramMessageID, TelegramMessage = @TelegramMessage, TelegramGroupID = @TelegramGroupID where PurchaseOrderID = @PurchaseOrderID";
                para.Clear();
                para.Add("@TelegramMessageID", sent.Value.ToString());
                para.Add("@TelegramMessage", msg.ToString());
                para.Add("@TelegramGroupID", chatId.ToString());
                para.Add("@PurchaseOrderID", PurchaseOrderID.ToString());
                html.SaveChangeData(sql, false, para);
            }

            return Json(new
            {
                Result = sent.Result,
                Message = sent.Message
            }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> RequestEditPO(RequestEditProp request)
        {
            var result = new SqlResult();
            var html = new HTML();
            var para = new Dictionary<string, string>();

            string bottoken = html.GetBotTokenBySystemID(Convert.ToInt32(html.SystemID));
            string chatID = html.SystemID == "13" ? "-967102140" : "-5123119304";
            para.Add("PurchaseID", request.PurchaseID.ToString());
            para.Add("RequestEditApproveByID", request.ApproveByID.ToString());
            para.Add("RequestEditApproveBy", request.ApproveBy);
            para.Add("RequestEditByUsername", request.Username);
            para.Add("Comment", request.Comment);

            string query = @"
                UPDATE dbo.TblPurchaseOrder
                SET IsRequestEdit = 1, RequestEditApproveByID = @RequestEditApproveByID, RequestEditApproveBy = @RequestEditApproveBy, RequestEditDate = GETDATE(), RequestEditByUsername = @RequestEditByUsername, RequestEditStatusID = 60,
                    RequestEditComment = @Comment
                WHERE PurchaseOrderID = @PurchaseID;
                select * from dbo.TblPurchaseOrder where PurchaseOrderID = @PurchaseID;
            ";

            result = html.SaveChangeData(query, false, para, true, true);

            var data = result.Data.Tables[0].Rows[0];
            string vendorID = data["VendorID"].ToString();
            string referenceNo = data["PONumber"].ToString();

            var pdfResult = await GeneratePDF(vendorID, request.PurchaseID, referenceNo);

            var msg = new StringBuilder();
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Approve ✅", $"ApproveEditPO~{request.PurchaseID}~{request.ApproveByID}"),
                    InlineKeyboardButton.WithCallbackData("Reject ❌", $"RejectEditPO~{request.PurchaseID}~{request.ApproveByID}")
                }
            });

            msg.AppendLine($"<b>📝 PO REQUEST EDIT</b>\n");
            msg.AppendLine($"{request.Username}\n");
            msg.Append($"Dear {(request.ApproveByID == 38 ? "Boss" : request.ApproveBy)}, ");
            msg.AppendLine($"You have a request edit po need to approve.\n");
            msg.AppendLine($"<i>Request By: {html.UserFullName}</i>");
            msg.AppendLine($"<i>Reason: {request.Comment}</i>\n");
            msg.AppendLine($"Please kindly review and take action, Thanks you.\n");

            string messageText = msg.ToString();
            var notification = new NotificationTelegram();
            result = await notification.NotificationTelegramBot(
                messageText,
                long.Parse(chatID),
                PDFFile: pdfResult?.OutPutFile,
                replyMarkupInlineKeyboardMarkup: true,
                inlineKeyboard: keyboard,
                newBotClient: bottoken
            );

            if (result.Result == ResultType.Successful)
            {
                System.IO.File.Delete(pdfResult?.OutPutFile);
            }

            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ClearPOAttachment(int PurchaseOrderID, string RemainingFiles = "")
        {
            HTML html = new HTML();
            try
            {
                string sql = "select AttachmentFile, DestinationFolder from dbo.TblPurchaseOrder where PurchaseOrderID = " + PurchaseOrderID;
                DataSet ds = html.LoadData(sql);
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    string attachmentFile = ds.Tables[0].Rows[0]["AttachmentFile"]?.ToString() ?? "";
                    string destFolder = ds.Tables[0].Rows[0]["DestinationFolder"]?.ToString() ?? "";
                    if (string.IsNullOrEmpty(destFolder))
                        destFolder = "Uploads/Vendors/PurchaseOrder";

                    if (!string.IsNullOrEmpty(attachmentFile))
                    {
                        string path = Server.MapPath("~/" + destFolder);
                        var keptFiles = (RemainingFiles ?? "")
                            .Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(f => f.Trim())
                            .ToList();

                        foreach (string file in attachmentFile.Split('~'))
                        {
                            string f = file.Trim();
                            if (!string.IsNullOrEmpty(f) && !keptFiles.Contains(f, StringComparer.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string fullPath = Path.Combine(path, f);
                                    if (System.IO.File.Exists(fullPath))
                                        System.IO.File.Delete(fullPath);
                                }
                                catch { }
                            }
                        }
                    }
                }

                string cleanRemaining = string.Join("~", (RemainingFiles ?? "")
                    .Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(f => f.Trim()));

                string updateSql;
                if (string.IsNullOrEmpty(cleanRemaining))
                {
                    updateSql = "update dbo.TblPurchaseOrder set AttachmentFile = '', DestinationFolder = '' where PurchaseOrderID = " + PurchaseOrderID;
                }
                else
                {
                    updateSql = "update dbo.TblPurchaseOrder set AttachmentFile = N'" + cleanRemaining.Replace("'", "''") + "' where PurchaseOrderID = " + PurchaseOrderID;
                }
                html.SaveChangeData(updateSql, false, false);
                return Json(new { Result = 1 });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }
        public JsonResult CloseCancelPO(int POID, string Status)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            string sql = "";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"POID", POID.ToString() },
            };
            if (POID > 0)
            {
                if (Status.ToLower() == "close")
                {
                    sql = @"update dbo.TblPurchaseOrder set IsClose = 1 where PurchaseOrderID = @POID";
                }
                else if (Status.ToLower() == "cancel")
                {
                    sql = @"update dbo.TblPurchaseOrder set StatusID = 94, IsCancel = 1 where PurchaseOrderID = @POID";
                }

                result = html.SaveChangeData(sql, false, para);
            }
            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public async Task<SqlResult> GeneratePDF(string vendorID, int purchaseOrderID, string referenceNo)
        {
            var html = new HTML();
            var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);

            var routeData = new RouteData();
            routeData.Values["controller"] = "PurchaseOrder";
            routeData.Values["action"] = "InvoicePO";

            var controller = new PurchaseOrderController();
            controller.ControllerContext = new ControllerContext(httpContext, routeData, controller);

            // Run the action so ViewData/Model is populated before rendering
            controller.InvoicePO(int.Parse(vendorID), purchaseOrderID, referenceNo);

            var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, "InvoicePO");
            if (viewResult?.View == null)
                throw new InvalidOperationException("Could not locate the 'InvoicePO' partial view.");

            string htmlContent;
            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(
                    controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                htmlContent = sw.GetStringBuilder().ToString();
            }

            string templatePath = httpContext.Server.MapPath("~/Templates/HtmlTemplate.html");
            string template = System.IO.File.ReadAllText(templatePath);
            string finalHtml = template.Replace("{Title}", "Purchase Order").Replace("{BodyMessage}", htmlContent);

            return await html.HTMLToPDF(finalHtml, "Uploads/Vendors/", referenceNo, false, true, "A3");
        }
    }

    public class RequestEditProp
    {
        public int PurchaseID { get; set; }
        public string PONumber { get; set; }
        public int ApproveByID { get; set; }
        public string ApproveBy { get; set; }
        public string Username { get; set; }
        public string Comment { get; set; }
    }
}