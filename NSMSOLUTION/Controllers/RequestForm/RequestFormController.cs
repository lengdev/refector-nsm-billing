using MVC;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers.RequestForms
{
  [Authorize]
  public class RequestFormController : Controller
  {
    public ActionResult Index()
    {
      HTML html = new HTML();
      bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
      html.ReportTitle = "List Request Form";
      if (html.HasPermission("Add Request Form"))
        html.AllowAdd = true;
      if (html.HasPermission("Edit Request Form"))
        html.AllowEdit = true;
      if (html.HasPermission("Delete Request Form"))
        html.AllowDelete = true;
      html.TableName = "TblRequestForm";
      html.AddButton = "Add New";
      html.AddURL = "RequestForm/CreateRequestForm";

      html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
      {
        DefaultValueDate = false,
        DateRangePicker = true,
        LabelFromDate = "Date:",
        DBField = "rf.RequestDate",
      });
      html.HtmlAddControls.Add(new HtmlAddControls()
      {
        ID = "txtClassName",
        Label = "Class Name:",
        DBField = "ClassName",
        SQL_CBO = "select distinct c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%' order by c.ClassName",
        DisplayText = "ClassName",
        Value = "ClassName",
        ComboBox = true
      });
      html.HtmlAddControls.Add(new HtmlAddControls()
      {
        ID = "txtVendorName",
        //ISAutoComplete = true,
        Label = "Vendor Name:",
        DBField = "vd.VendorName",
        SQL_CBO = "select​​ ​distinct v.VendorName from dbo.TblVendor v where v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " and v.DomainID like '%" + html.DomainID + "%' order by v.VendorName",
        DisplayText = "VendorName",
        Value = "VendorName",
        ComboBox = true
      });
      html.HtmlAddControls.Add(new HtmlAddControls()
      {
        ID = "txtDepartmentName",
        //ISAutoComplete = true,
        Label = "Department Name:",
        DBField = "DepartmentName",
        SQL_CBO = "select distinct d.DepartmentName from dbo.TblDepartment d where d.SystemID = " + html.SystemID + " and d.WarehouseID = " + html.WarehouseID + " and d.DomainID like '%" + html.DomainID + "%' order by d.DepartmentName",
        DisplayText = "DepartmentName",
        Value = "DepartmentName",
        ComboBox = true
      });
      html.HtmlAddControls.Add(new HtmlAddControls()
      {
        ID = "txtPaymentMean",
        Label = "Payment Mean:",
        DBField = "PaymentMean",
        SQL_CBO = "select distinct p.PaymentMeanName PaymentMean from dbo.TblPaymentMean p order by p.PaymentMeanName",
        DisplayText = "PaymentMean",
        Value = "PaymentMean",
        ComboBox = true
      });
      html.HtmlAddControls.Add(new HtmlAddControls()
      {
        ID = "txtStatusID",
        Label = "Request Status:",
        DBField = "rf.StatusID",
        SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 91, 92, 93, 94)",
        DisplayText = "Status",
        Value = "StatusID",
        ComboBox = true
      });

      List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
      htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
      {
        OPTLabel = "Reference Number",
        SQL = "select distinct top 10 r.ReferenceNumber from dbo.TblRequestForm r where lower(r.ReferenceNumber) like @SearchText and r.SystemID = " + html.SystemID + " and r.WarehouseID=" + html.WarehouseID + " and r.DomainID like '%" + html.DomainID + "%' order by r.ReferenceNumber",
        RadioValue = EncryAndDecry.Encryp("ReferenceNumber"),
        ISAutoComplete = true,
        DefaultCheckBok = true,
        placeholder = "Enter Reference Number"
      });


      string sql = @"select ROW_NUMBER() Over(order by rf.RequestFormID desc) as [No], rf.RequestFormID, Format(rf.RequestDate, 'dd-MM-yyyy') RequestDate,'<a href=""javascript:void(0);"" style=""cursor: pointer;"" onclick=""ShowRequestFormInvoice('+convert(varchar, isnull(rf.VendorID, 0))+','+convert(varchar, rf.RequestFormID)+', '''+rf.ReferenceNumber+''', '''+rf.PaymentMean+''')"">'+rf.ReferenceNumber+'</a>' ReferenceNumber,
case when rf.StatusID in (60,91) then '<span class=""badge badge-danger"">' + s.[Status] + '</span>' when (rf.StatusID = 90 and rf.RequestStatusID = 90) then '<span class=""badge badge-primary"">' + s.[Status] + '</span>' when rf.StatusID = 92 then '<span class=""badge badge-info"">' + s.[Status] + '</span>' when rf.RequestStatusID = 67 then '<span class=""badge badge-info text-white"">' + s1.[Status] + '</span>' else '<span class=""badge badge-warning text-white"">' + s.[Status] + '</span>' end [RequestStatus],
rf.StatusID, ur.FullName [RequestBy], format(rf.SubTotal, 'N2') Amount, vd.VendorName, rf.PaymentMean, case when rf.ClassName = '0' then '' else rf.ClassName end ClassName, rf.DepartmentName, case when rf.UserID is null then rf.CreateBy else u.FullName end [IssueBy]
from dbo.TblRequestForm rf
inner join dbo.TblStatus s on s.StatusID = rf.StatusID
left join dbo.TblStatus s1 on s1.StatusID = rf.RequestStatusID
left join dbo.TblVendor vd on vd.VendorID = rf.VendorID
--left join dbo.TblDepartment d on d.DepartmentID = rf.DepartmentID
left join dbo.TblUserLogin u on u.UserLoginID = rf.UserID
inner join dbo.TblUserLogin ur on ur.UserLoginID = rf.RequestByID
where rf.RequestType = 'RF'";
      html.OrderBy = "[No]";
      html.IsHaveWhereCondition = true;
      html.GH.GridPara = new GridPara[] {
            new GridPara() { ParaName="rf.SystemID", Value =html.SystemID },
            new GridPara() { ParaName="rf.WarehouseID", Value =html.WarehouseID },
            new GridPara() { ParaName="rf.DomainID", Value =html.DomainID },
        };
      html.GH.SQL = sql;
      html.GH.GridTable = new GridField[]
      {
            new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
            new GridField() { Header = "Request Date", FieldName = "RequestDate", TextAlign="center" },
            new GridField() { Header = "Reference Number#", FieldName = "ReferenceNumber", ClassName="text-center" },
            new GridField() { Header = "Payment Mean", FieldName = "PaymentMean", },
            new GridField() { Header = "Vendor", FieldName = "VendorName", ClassName="text-truncate", Width="400px"},
            new GridField() { Header = "Class Name", FieldName = "ClassName" },
            new GridField() { Header = "Department Name", FieldName = "DepartmentName" },
            new GridField() { Header = "Amount", FieldName = "Amount",TextAlign="center" },
            new GridField() { Header = "Status", FieldName = "RequestStatus", TextAlign ="center" },
            new GridField() { Header = "Request By", FieldName = "RequestBy", },
            new GridField() { Header = "Issue By", FieldName = "IssueBy",},
            new GridField() { Header = "Action", FieldName = "RequestFormID", TextAlign = "Center",  IsModifyField = true,
                DeleteWithTableName="TblRequestFormDetail",
                HideEditOnActionNameNotMatch = true, //(AdminEdit==false? true:false),
                ShowActionNameOnConditionField ="StatusID",
                ShowActionNameOnConditionValue="60, 91, 90, 92",
                HideDeteleOnNotMatch=true
            }
      };
      html.ShowPaging = false;
      html.DialogBoxWidth = "auto";

      ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
      return View();
    }
    public ActionResult CreateRequestForm(string PrimaryFieldValue, bool IsClone = false)
    {
      HTML html = new HTML
      {
        FormID = "CreateRequestForm",
        isCloned = IsClone,
        PrimaryFieldValue = PrimaryFieldValue
      };
      html.FormData = new HTMLControl[]
      {
            new  FormData() {
                TableName="TblRequestForm",
                FormTitle="Request Form Information",
                GetIDENTITYByFiled="RequestFormID",
                PrimaryFieldName="RequestFormID",
                dbField=new DBField[]
                {
                    new DBField()
                    {
                        DBFieldName="SubTotal", Value="SubTotal", GetValueFromLabelName = true, IsNumber = true
                    },
                    new DBField()
                    {
                        DBFieldName="TotalEstimate", Value="TotalEstimate", GetValueFromLabelName= true, IsNumber = true
                    },
                    new DBField()
                    {
                        DBFieldName="Remark", Value="RemarkInfo", GetValueFromTextAreaName= true
                    },
                    new DBField()
                    {
                        DBFieldName="BankInformation", Value="BankInfo", GetValueFromTextAreaName= true
                    },
                    new DBField()
                    {
                        DBFieldName="StatusID", SetValue = "60"
                    },
                    new DBField()
                    {
                        DBFieldName="RequestType", SetValue = "RF"
                    },
                },
                //When update request form
                SQL=@"update dbo.TblRequestForm set CheckedBy=NULL,CheckedByUserID=NULL,CheckedDate=NULL, ApprovedByUserID=NULL,ApprovedBy=NULL,ApprovalDate=NULL,ApprovalFrom=NULL,
NextApprovedBy=NULL,NextApprovedByID=NULL,NextApprovedByDate=NULL,NextApprovedFrom=NULL,NextApproveStatusID=NULL,NextDisapproveByID=NULL,NextDisapproveBy=NULL,NextDisapproveDate=NULL, Comment = NULL
where RequestFormID = @RFID",
                UpdateFiledName="RFID",
                htmlControl = new HTMLControl[]
                {
                    new HtmlTextBox()
                    {
                        Label="RFID", ID="RFID", Required=true,
                        Value = IsClone ? "0" : PrimaryFieldValue.ToString(), IsReadOnly = true, Hide = true
                    },
                    new HtmlTextBox()
                    {
                        Label="Payment Mean",ID="PaymentMean", Required=true,
                        Class="input2 CustInfo", IsCustomForm = true,
                        isTypeahead = true,
                        DBField="PaymentMean",
                        SQL="select PaymentMeanID, PaymentMeanName [PaymentMean] from dbo.TblPaymentMean order by PaymentMeanName",
                        HiddenField="PaymentMean,PaymentMeanID",
                        HiddenFieldValue="0,0",
                    },
                    new HtmlTextBox()
                    {
                        Label="Class Name", ID="ClassName", Required = true,
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="ClassName", isTypeahead = true,
                        SQL="select ClassID, ClassName, PrefixRequestForm from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                        HiddenField="ClassID",
                        HiddenFieldValue="0",
                        SQLCallBack="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = PrefixRequestForm, @Module = 'RF' from dbo.TblClass where ClassID = @ClassID;	exec [SP_SHOW_PREFIX] @Prefix, @Module, "+html.SystemID+", "+html.WarehouseID+", '"+html.DomainID+"';",
                        CallBackToControlID="ReferenceNumber, PrefixRequestForm", UpdatePrefixModule="RF", Prefix="PrefixRequestForm", SetPrefixToDbFieldName="ReferenceNumber",
                    },
                    new HtmlTextBox()
                    {
                        Label="Vendor Name", ID = "VendorName", Required=true,
                        Class="input2 CustInfo", IsCustomForm = true,
                        isTypeahead = true,
                        DBField="VendorName",
                        SQL="select VendorID, (VendorCode + ' - ' + VendorName) VendorName from dbo.TblVendor where (Status = 'Enable' or Status is null or Status = '')  and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                        HiddenField="VendorName,VendorID",
                        HiddenFieldValue="0,0",
                        TypeAheadAddNewFunctionName="AddNewVendor()",
                    },
                }
            },
            new FormData()
            {
                htmlControl = new HTMLControl[]
                {
                    new HtmlTextBox()
                    {
                        Label="Reference Number", ID="ReferenceNumber", Required=true,
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="ReferenceNumber", IsReadOnly = true,
                        isCloned = IsClone,
                    },
                    new HtmlTextBox()
                    {
                        Label="Request Date", ID="RequestDate", Required=true,
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="RequestDate", IsDisabled = true,
                        SingleDatePicker = true,
                    },
                    new HtmlTextBox()
                    {
                        Label="Requirement Date", ID="RequirementDate", Required=true,
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="RequirementDate",
                        SingleDatePicker = true,
                    },
                    new HtmlTextBox()
                    {
                        Label="Department", ID="Department",
                        Required=true,
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="DepartmentName",isTypeahead = true,
                        SQL="select DepartmentID, DepartmentName from dbo.TblDepartment where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' order by DepartmentName",
                        HiddenField ="DepartmentID",
                        AddValueToTblNameIfNotExistingData="TblDepartment",
                        AddValueToTblNameIfNotExistingDataFieldName="DepartmentName",
                        TypeAheadAddNewFunctionName="AddNewDepartment()",
                    },
                }
            },
            new  FormData() {
                htmlControl = new HTMLControl[]
                {
                    new HtmlTextBox()
                    {
                        Label="Request By", ID="RequestBy", Required=true,
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="RequestBy", isTypeahead = true,
                        SQL="select u.UserLoginID RequestByID, u.TelegramUserName, u.FullName RequestBy,u.Telephone PhoneNumber, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                        HiddenField="RequestByID",
                        DisplayOnFields="PhoneNumber",
                        DivClassCol="col-sm"
                    },
                    new HtmlTextBox()
                    {
                        Label="Phone", ID="PhoneNumber",
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="PhoneNumber", isTypeahead = true,
                        DivClassCol="col-sm"
                    },
                    new HtmlTextBox()
                    {
                        Label="Check By", ID="NeedToCheckBy",
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="NeedToCheckBy", isTypeahead = true,
                        SQL="select u.UserLoginID NeedToCheckUserID, u.TelegramUserName CheckedByUsername, u.FullName NeedToCheckBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                        HiddenField="CheckedByUsername, NeedToCheckUserID",
                        HiddenFieldValue="0,0",
                        DivClassCol="col-sm"
                    },

                    new HtmlTextBox()
                    {
                        Label="Approval By", ID="NeedApprovalBy", Required=true,
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="NeedApprovalBy", isTypeahead = true,
                        SQL="select u.UserLoginID NeedApprovalByUserID, u.TelegramUserName, u.FullName NeedApprovalBy, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                        HiddenField="TelegramUserName, NeedApprovalByUserID",
                        HiddenFieldValue="0,0",
                        DivClassCol="col-sm"
                    },
                }
            },
            new FormData() {
                htmlControl = new HTMLControl[]
                {
                    new HtmlTextBox()
                    {
                        Label="Next Approval By", ID="NeedNextApprovalBy",
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="NeedNextApprovalBy", isTypeahead = true,
                        SQL="select u.UserLoginID NeedNextApprovalByUserID, u.TelegramUserName NeedNextApprovalByUsername, u.FullName NeedNextApprovalBy, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                        HiddenField="NeedNextApprovalByUsername, NeedNextApprovalByUserID",
                        HiddenFieldValue="0,0",
                        DivClassCol="col-sm"
                    },
                    new HtmlTextBox()
                    {
                        Label="Payment Method", ID="PaymentMethod",
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="PaymentMethod", isTypeahead = true,
                        SQL="select PaymentMethod from dbo.TblPaymentMethod where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%'",
                    },

                    new HtmlTextBox()
                    {
                        Label="Exchange Rate", ID="ExchangeRate",
                        Class="input2 CustInfo", IsCustomForm=true,
                        DBField="ExchangeRate",
                        Value="4000",IsAllowInputOnlyNumber =true,
                    },
                        new HtmlTextBox()
                    {
                        Label="Deliver To", ID="DeliverTo",
                        Class="input2 CustInfo", IsCustomForm = true,
                        DBField="DeliverTo", isTypeahead = true,
                        SQL="select uw.WID, uw.WarehouseName DeliverTo, uw.WarehouseName WName, uw.ContactName, uw.ContactPhone from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " order by uw.WarehouseName",
                        HiddenField="WID",
                        TypeAheadAddNewFunctionName="AddNewWarehouse()",
                        DisplayOnFields= JsonConvert.SerializeObject(new { TxtContactName="ContactName", TxtContactPhone="ContactPhone",TxtContactName_WarehouseName="WName"})
                    },
                }
            },
            new  FormData() {
                htmlControl = new HTMLControl[]
                {
                    new HtmlMyTable()
                    {
                        ScriptCallBackFunction="calcSubtotal(), Reload_iUM()",
                        FooterColspan=2,
                        SubTotalColspan=4,
                        GetIDENTITYByFiled="RequestFormID",
                        PrimaryFieldName="RequestFormID",
                        TableName="TblRequestFormDetail",
                        OrderBy="RequestFormDetailID",
                        ShowTotal=false,
                        ShowTermsConditions=false,
                        ShowCustomerNotes=false,
                        ShowRemark=true,
                        ShowBankInfo=true,
                        ShowTotalOnly=true,
                        ReturnFieldNameValue="RequestFormID,VendorID, VendorName,RequestType,PaymentMean, ReferenceNumber, NeedApprovalBy, TelegramUserName",
                        ReturnFieldNameValueFromTblName = "TblRequestForm",
                        HtmlTblID="TableItemInformation",
                        InputName="ItemName",
                        TRName="TRNo",
                        TDName="TDNo",
                        SQL=$@"select format(rd.QTY, 'N3') as [QTY], format(rd.UnitPrice, 'N3') as [UnitPrice], format(rd.EstimateAmount, 'N3') as [EstimateAmount], format(rd.Amount, 'N2') as [Amount], rd.*
                            from dbo.TblRequestForm r
                            inner join dbo.TblRequestFormDetail rd on rd.RequestFormID = r.RequestFormID
                            where r.RequestFormID = {PrimaryFieldValue} and r.SystemID = {html.SystemID};",
                        dbField=new DBField[]
                        {
                            new DBField()
                            {
                                DBFieldName="ItemID", Value="ItemID"
                            },
                            new DBField()
                            {
                                DBFieldName="UomID", Value="UomID", GetValueFromInputName=true,
                            },
                            new DBField()
                            {
                                DBFieldName="ReferenceNumber", SetValue="$(\"#ReferenceNumber\").val()"
                            },
                            new DBField()
                            {
                                DBFieldName="RequestType", SetValue="$(\"#RequestType\").val()"
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
                                    Label="បរិមាណ", Class="col-1 text-nowrap"
                            },
                            new TblTHead()
                            {
                                    Label="ខ្នាត", Class="col-1 text-nowrap"
                            },
                            new TblTHead()
                            {
                                    Label="ប៉ាន់ប្រមាណ$", Class="col-1",
                            },
                            new TblTHead()
                            {
                                    Label="ថ្លៃឯកតា"
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
                                    Label="Quantity"
                            },
                            new TblTHead()
                            {
                                Label="UOM", Class="text-nowrap"
                            },
                            new TblTHead()
                            {
                                Label="Estimate Amount$",
                            },
                            new TblTHead()
                            {
                                    Label="Unit Price", Class="text-nowrap"
                            },

                            new TblTHead()
                            {
                                    Label="Total Amount$",
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
                                    new HtmlTextarea()
                                    {
                                        Name="ItemName",
                                        DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName','calcSubtotal');",
                                        DBField="ItemDescription",
                                        Class="input form-control", IsCustomForm = true,
                                        DivClass="wrap-input3",
                                        Required=true,
                                    },
                                    new HtmlTextBox()
                                    {
                                        Name="QTY", DivClass="wrap-input3 text-center",
                                        Class="input2 CustInfo text-center", IsCustomForm = true,
                                        DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                        Digit = "0.000"
                                    },
                                    new HtmlTextBox()
                                    {
                                        Name="iUM",
                                        Class="input2 CustInfo", IsCustomForm = true,
                                        DBField="UM",
                                        DivClass="wrap-input3",
                                        placeholder="Enter UOM",
                                        SQL="select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]",
                                        isTypeahead=true,
                                        HiddenField="UomID",
                                    },
                                    new HtmlTextBox()
                                    {
                                        Name="EstimateAmount", DivClass="wrap-input3",
                                        Class="input2 CustInfo text-center", IsCustomForm = true,
                                        DBField="EstimateAmount", IsAllowInputOnlyNumber =true,
                                        Digit = "0.000", Value = "0",
                                        placeholder="Estimate Amount"
                                    },
                                    new HtmlTextBox()
                                    {
                                        Name="UnitPrice", DivClass="wrap-input3",
                                        Class="input2 CustInfo text-center", IsCustomForm = true,
                                        DBField="UnitPrice", IsAllowInputOnlyNumber =true,
                                        placeholder="Enter Unit Price", Digit = "0.000", Value = "0",
                                        Required=true
                                    },
                                    new HtmlTextBox()
                                    {
                                        Name="Amount", DivClass="wrap-input3",
                                        Class="input2 CustInfo text-center", IsCustomForm = true,
                                        DBField="Amount", IsAllowInputOnlyNumber =true,
                                    },
                                },
                                CloseTR="</tr>"
                            }
                        }
                    }
                }
            },
            new  FormData() {
                htmlControl = new HTMLControl[]
                {
                    new HtmlFile()
                    {
                        ID = "ItemImage",
                        Class = "input2 CustInfo", IsCustomForm = true,
                        DBField = "ImagePath",
                        AcceptImageOnly = true,
                        DestinationFile = "Uploads/RequestForm/CashAdvance",
                        Multiple = true,
                        UploadFilesV2 = true,
                        TableName = "TblRequestForm",
                        PrimaryFieldName = "RequestFormID",
                        PrimaryFieldValue = PrimaryFieldValue
                    },
                }   
            },
      };
      html.FieldIDFocus = "PaymentMean";
      html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";
      html.SaveChangeAndClose = true;
      ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
      return View();
    }
    [HttpPost]
    public JsonResult DeleteRequestFormImage(int RequestFormID)
    {
        var html = new HTML();
        try
        {
            string sqlSelect = "SELECT ImagePath FROM dbo.TblRequestForm WHERE RequestFormID = @RequestFormID";
            var para = new Dictionary<string, string> { { "@RequestFormID", RequestFormID.ToString() } };
            var ds = html.LoadData(sqlSelect, false, para);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                string imagePath = ds.Tables[0].Rows[0]["ImagePath"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(imagePath))
                {
                    string destinationFolder = Server.MapPath("~/Uploads/RequestForm/CashAdvance");
                    string[] files = imagePath.Split('~');
                    foreach (string f in files)
                    {
                        if (!string.IsNullOrWhiteSpace(f))
                        {
                            string physPath = System.IO.Path.Combine(destinationFolder, System.IO.Path.GetFileName(f.Trim()));
                            if (System.IO.File.Exists(physPath))
                            {
                                try { System.IO.File.Delete(physPath); } catch { }
                            }
                        }
                    }

                    string sqlUpdate = "UPDATE dbo.TblRequestForm SET ImagePath = '' WHERE RequestFormID = @RequestFormID";
                    html.SaveChangeData(sqlUpdate, false, para);
                }
            }
            return Json(new { Result = ResultType.Successful, Message = "Image deleted successfully." }, JsonRequestBehavior.AllowGet);
        }
        catch (System.Exception ex)
        {
            return Json(new { Result = ResultType.Failed, Message = ex.Message }, JsonRequestBehavior.AllowGet);
        }
    }
        public async Task<JsonResult> SendNotificationToTelegram(int RequestFormID)
    {
      var html = new HTML();
      var result = new SqlResult();

      string sql = "SELECT * FROM dbo.TblRequestForm WHERE RequestFormID = @RequestFormID";
      var para = new Dictionary<string, string> { { "@RequestFormID", RequestFormID.ToString() } };
      var ds = html.LoadData(sql, false, para);
      var dr = ds.Tables[0].Rows[0];

      bool isNeedCheck = !string.IsNullOrEmpty(dr["NeedToCheckBy"]?.ToString());
      string approverId = isNeedCheck ? dr["NeedToCheckUserID"]?.ToString() : dr["NeedApprovalByUserID"]?.ToString();
      string fullName = isNeedCheck ? dr["NeedToCheckBy"]?.ToString() : dr["NeedApprovalBy"]?.ToString();
      string username = isNeedCheck ? dr["CheckedByUsername"]?.ToString() : dr["TelegramUserName"]?.ToString();
      string preparedBy = dr["CreateBy"]?.ToString();
      string paymentMean = dr["PaymentMean"]?.ToString();
      string referenceNo = dr["ReferenceNumber"]?.ToString();
      string systemID = dr["SystemID"]?.ToString();
      string action = isNeedCheck ? "check" : "approve";
      string recipient = int.Parse(approverId) == 38 ? "Boss" : fullName;
      string botToken = html.GetBotTokenBySystemID(int.Parse(systemID));

      var msg = new StringBuilder();
      msg.AppendLine($"{username}\n");
      msg.AppendLine($"Dear {recipient},\n");
      msg.AppendLine($"Now you have a request need to {action}.\n");
      msg.AppendLine($"<i><b>Prepared by: {preparedBy}</b></i>.\n");
      msg.AppendLine($"Please kindly review and take action, Thanks you.");

      var notification = new NotificationTelegram();
      var inlineKeyboard = new InlineKeyboardMarkup(new[]
      {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"Approve~{RequestFormID}~{username}"),
                    InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"Disapprove~{RequestFormID}~{username}"),
                    InlineKeyboardButton.WithCallbackData(text: "Reject ❌",     callbackData: $"Reject~{RequestFormID}~{username}")
                }
            });

      if (isNeedCheck)
      {
        inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Check ✍️",    callbackData: $"Check~{RequestFormID}~{username}"),
                        InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisCheck~{RequestFormID}~{username}"),
                        InlineKeyboardButton.WithCallbackData(text: "Reject ❌",     callbackData: $"RejectCheck~{RequestFormID}~{username}")
                    }
                });
      }

      sql = @"SELECT *
                FROM dbo.TblTelegramGroupNotification
                WHERE [Module] = 'RequestForm' AND SystemID = @SystemID";
      para.Clear();
      para.Add("@SystemID", html.SystemID);
      ds = html.LoadData(sql, false, para);
      dr = ds.Tables[0].Rows[0];

      long chatId = long.Parse(dr["TelegramGroupID"]?.ToString());

      ProfileController controller = new ProfileController();
      dynamic frm = controller.InvoiceTMPRequestForm(0, RequestFormID, paymentMean, ReturnAllHtmlData: true);
      string fileName = html.ToUrlSlug(referenceNo);
      string destination = $"Uploads/RequestForm";
      var pdfResult = await html.HTMLToPDF(frm.Data.Data, destination, fileName);

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

        sql = @"update dbo.TblRequestForm set TelegramMessageID = @TelegramMessageID, TelegramMessage = @TelegramMessage, TelegramGroupID = @TelegramGroupID where RequestFormID = @RequestFormID";
        para.Clear();
        para.Add("@TelegramMessageID", sent.Value.ToString());
        para.Add("@TelegramMessage", msg.ToString());
        para.Add("@TelegramGroupID", chatId.ToString());
        para.Add("@RequestFormID", RequestFormID.ToString());
        html.SaveChangeData(sql, false, para);
      }

      return Json(new
      {
        Result = sent.Result,
        Message = sent.Message
      }, JsonRequestBehavior.AllowGet);
    }
  }

}
