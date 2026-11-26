using MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddVendorController : Controller
    {
        // GET: AddVendor
        [Authorize(Roles = "Add Vendor, Edit Vendor")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Vendors",
                GridEventClick = "onGridClick(this);",
                DialogBoxWidth = "Auto"
            };
            if (html.HasPermission("Add Vendor"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Vendor"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Vendor"))
                html.AllowDelete = true;

            html.AddButton = "Add Vendor";
            html.AddURL = "AddVendor/CreateVendor";
            html.TableName = "TblVendor";
            html.PrimaryField = "VendorID";

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Register Date:",
                DBField = "RegisterDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (3,4)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtVendorType",
                Label = "Vendor Type:",
                DBField = "v.VendorType",
                SQL_CBO = "select distinct VendorType from dbo.TblVendor where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by VendorType",
                DisplayText = "VendorType",
                Value = "VendorType",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Vendor Code",
                SQL = "select distinct top 10 VendorCode from dbo.TblVendor where lower(VendorCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by VendorCode",
                //SelectValue = "VendorID",
                //DisplayField = "VendorName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("VendorCode") + "," + EncryAndDecry.Encryp("VendorName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {

                OPTLabel = "Vendor Name",
                SQL = "select distinct top 10 VendorName from dbo.TblVendor where lower(VendorName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by VendorName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("VendorCode") + "," + EncryAndDecry.Encryp("VendorName"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);


            string sql = @"select ROW_NUMBER() over(order by v.VendorCode desc) as [No], v.VendorID, v.RegisterDate, 
vt.VendorType, '<a class=""text-primary"" style=""cursor: pointer;"" onclick=""showVendorInfo('+convert(varchar, v.VendorID)+')"">'+v.VendorCode+'</a>' VendorCode, 
'<a class=""text-primary"" style=""cursor: pointer;"" onclick=""showVendorInfo('+convert(varchar, v.VendorID)+')"">'+v.VendorName+'</a>' VendorName, v.Phone, v.CreateBy, isnull(v.Status, '') Status, v.ContactName, v.ContactPersonName, v.ContactPhoneNumber
from dbo.TblVendor v 
left join dbo.TblVendorType vt on vt.VendorTypeID = v.VendorTypeID
where v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.SQL = sql;
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
				//new GridField() { Header = "VendorID", FieldName = "VendorID", IsHiddenField= true },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", FormatDateTime="dd-MM-yy", TextAlign = "center" },
                new GridField() { Header = "VendorType", FieldName = "VendorType"},
                new GridField() { Header = "Vendor Code", FieldName = "VendorCode", TextAlign="center" },
                new GridField() { Header = "Vendor Name", FieldName = "VendorName" },
                new GridField() { Header = "Company Name", FieldName = "CompanyName" },
                new GridField() { Header = "Phone", FieldName = "Phone" },
                new GridField() { Header = "Contact Name", FieldName = "ContactPersonName" },
                new GridField() { Header = "Contact Phone", FieldName = "ContactPhoneNumber" },
                new GridField() { Header = "Status", FieldName = "Status" },
                //new GridField() { Header = "Address", FieldName = "Address" },				
				//new GridField() { Header = "Email", FieldName = "Email", isEmail = true },
				//new GridField() { Header = "Website", FieldName = "Website", isUrl = true },
				new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Action", FieldName = "VendorID", TextAlign = "Center",
                    IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Vendor, Edit Vendor")]
        public ActionResult CreateVendor(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML
            {
                SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);",
                FormID = "CreateVendor",
                PrimaryFieldValue = PrimaryFieldValue
            };
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            if (PrimaryFieldValue != "0")
            {
                ViewBag.IsEdit = 1;
            }
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblVendor",
                    FormTitle="Vendor Information",
                    GetIDENTITYByFiled="VendorID",
                    PrimaryFieldName="VendorID",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="BankerName", Value="BankerName", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="BankerAddress", Value="BankerAddress", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="SwiftCode", Value="SwiftCode", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="AccountNo", Value="AccountNo", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="AccountName", Value="AccountName", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="BankerRemark", Value="BankerRemark", GetValueFromInputName = true
                        },
                        new DBField()
                        {
                            DBFieldName="Seq", SetValue="`+$(\"#VendorCode_CurrentNumber\").val()+`"
                        },
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="RegisterDate", ID="RegisterDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="RegisterDate", Value = DateTime.Parse(html.GetCurrentDate()).ToString("yyyy-MM-dd HH:mm:ss tt"), Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Vendor Type", ID="VendorType", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="VendorType", TypeAheadAddNewFunctionName="AddNewVendorType()",
                            SQL="select distinct VendorTypeID, VendorType, Prefix PrefixVendor from dbo.TblVendorType where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%'",
                            isTypeahead=true, HiddenField="VendorTypeID"
                        },
                        new HtmlTextBox()
                        {
                            Label="Vendor Code", ID="VendorCode", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="VendorCode",
							//ISUniqueFiled = true, 
							IsReadOnly = true,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'VD'; select @Prefix = Prefix from dbo.TblPrefix where Module = 'VD' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'VD-{000}'",
                            CallBackToControlID="VendorCode, PrefixVD", Prefix="PrefixVD", SetPrefixToDbFieldName="VendorCode",
                            HiddenField="CurrentNumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="Vendor Name", ID="VendorNames", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="VendorName", ///SameEntryToControlID="TXTContactPersonNames",
                            ISUniqueField = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Gender", ID="Gender",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Gender",
                            SQL="select 'Male' Gender union all select 'Female' Gender",
                            isTypeahead = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Phone", ID="Phone", Class="input2 CustInfo",
                            IsCustomForm = true, DBField="Phone", Required = true,
                            SameEntryToControlID = "ContactPhoneNumber"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Address", ID="Address", Class="input2 CustInfo", Required = true,
                            IsCustomForm = true, DBField="Address", SameEntryToControlID="BillingAddress"
                        },
                        new HtmlTextBox()
                        {
                            Label="Email", ID="Email", Class="input2 CustInfo", Type ="email",
                            IsCustomForm = true, DBField="Email", //SameEntryToControlID="BillingEmail", //Hide = true
						},
                        new HtmlTextBox()
                        {
                            Label="Country", ID="Country", Class="input2 CustInfo",
                            IsCustomForm = true, DBField="Country", SameEntryToControlID="BillingCountry",
                            isTypeahead= true,
                            SQL="select CountryID, Country from dbo.TblCountry where SystemID = "+html.SystemID+" and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' order by Country",
                            AddValueToTblNameIfNotExistingData="TblCountry",
                            AddValueToTblNameIfNotExistingDataFieldName="Country",
                            DivClassCol="col-auto", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Province / City", ID="City", Class="input2 CustInfo",
                            IsCustomForm = true, DBField="City", SameEntryToControlID="BillingCity",
                            isTypeahead= true,
                            SQL="select CityID, City from dbo.TblCity where SystemID = "+html.SystemID+" and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' order by City",
                            AddValueToTblNameIfNotExistingData="TblCity", AddValueToTblNameIfNotExistingDataFieldName="City",
                            HiddenField="CityID", DivClassCol="col-auto", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="State/Region", ID="StateRegion", Class="input2 CustInfo",
                            IsCustomForm = true, DBField="StateRegion",
                            isTypeahead= true,
                            SQL="select StateRegionID, StateRegion from dbo.TblStateRegion where SystemID = "+html.SystemID+" and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' order by StateRegion",
                            AddValueToTblNameIfNotExistingData="TblStateRegion", AddValueToTblNameIfNotExistingDataFieldName="StateRegion",
                            HiddenField="StateRegionID",DivClassCol="col-auto", Hide=true
                        },
                        new HtmlTextBox()
                        {
                            Label="Website", ID="Website", Class="input2 CustInfo",
                            IsCustomForm = true, DBField="Website", Hide = true
                        }
                    }
                },
                new FormData(){
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Payment Terms", ID="VendorPaymentTerms",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PaymentTerms", isTypeahead = true,
                            SQL="select convert(varchar, NumberofDays) NumberofDays, TermName PaymentTerms from dbo.TblPaymentTerms where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" and DomainID like '%"+html.DomainID+"%' union select '0' NumberofDays, 'Due on Receipt' PaymentTerms order by PaymentTerms",
                            HiddenField="NumberofDays", Value="Due on Receipt"
                        },
                        new HtmlTextBox()
                        {
                            Label="Contact Person", ID="TXTContactPersonNames", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true, DBField="ContactPersonName"
                        },
                        new HtmlTextBox()
                        {
                            Label="Position", ID="Position", Class="input2 CustInfo",
                            IsCustomForm = true, DBField="Position"
                        },
                        new HtmlTextBox()
                        {
                            Label="Contact Phone Number", ID="ContactPhoneNumber", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true, DBField="ContactPhoneNumber"
                        }
                    }
                },
                new FormData(){
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Company Name", ID="CompanyName", Class="input2 CustInfo",
                            IsCustomForm = true, DBField="CompanyName",
                        },
                        new HtmlTextBox()
                        {
                            Label="VAT TIN", ID="VATTIN", Class="input2 CustInfo",
                            IsCustomForm = true, DBField="VATTIN",
                        },
                        new HtmlTextBox()
                        {
                            Label="Company Address", ID="BillingAddress", Class="input2 CustInfo",
                            IsCustomForm = true, DBField="CompanyAddress"
                        },
                        new HtmlTextBox()
                        {
                            Label="Status", ID="Status",
                            //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Status",isTypeahead = true,
                            SQL="select StatusID, [Status] from dbo.TblStatus where StatusID in (3,4)",
                            HiddenField ="StatusID", Value="Enable",
                            HiddenFieldValue="3"
                        },
                    }
                },
                new FormData(){
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Remark", ID="Remark", Class="input2 CustInfo",
                            IsCustomForm = true, DBField="Remark"
                        },
                    }
                },
                new  FormData() {
                    FormTitle="Bank Information",
                    ShowBtnExpans=true,
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Banker's Name", ID="BankerName",
                            IsCustomForm = true, Class="input2 CustInfo",
                            isTypeahead =true, DBField="PrimaryFieldValue",
                            SQL="select distinct [BankerName] PrimaryFieldValue from dbo.TblBankerName order by [BankerName]",
                            TypeAheadAddNewFunctionName="AddNewBankerName()",
                        },new HtmlTextBox()
                        {
                            Label="Swift Code", ID="SwiftCode", Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Banker's Address", ID="BankerAddress", Class="input2 CustInfo",
                            IsCustomForm = true,
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Account No", ID="AccountNo", Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Account Name", ID="AccountName", Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Remark", ID="BankerRemark", Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                    }
                },
            };
            html.SaveChangeAndClose = true;
            html.SaveChangeCallbackOnCloseDialogValue = "VendorCode";
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult BankerName()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Banker Name"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Banker Name"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Banker Name"))
                html.AllowDelete = true;

            html.AddButton = "Add New";
            html.AddURL = "AddVendor/CreateBankerName";
            html.TableName = "TblBankerName";
            html.PrimaryField = "BankerNameID";
            html.OrderBy = "BankerNameID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Banker Name",
                SQL = "select BankerNameID, BankerName from dbo.TblBankerName where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by BankerName",
                SelectValue = "BankerNameID",
                DisplayField = "BankerName",
                RadioValue = EncryAndDecry.Encryp("BankerNameID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = @"select ROW_NUMBER() Over(order by BankerNameID desc) as [No],BankerNameID, BankerName, format(CreateDate,'dd-MM-yyyy') as CreateDate, CreateBy
from dbo.TblBankerName";
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Banker Name", FieldName = "BankerName" ,Width="100%"},
                new GridField() { Header = "Create By", FieldName = "CreateBy", TextAlign="center"},
                new GridField() { Header = "Create Date", FieldName = "CreateDate",  TextAlign="center" },
                new GridField() { Header = "Action", FieldName = "BankerNameID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Banker Name, Edit Banker Name")]
        public ActionResult CreateBankerName(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML
            {
                FormID = "CreateBankerName",
                FieldIDFocus = "TxtBankerName",
                TableName = "TblBankerName",
                PrimaryField = "BankerNameID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            if (Type == "AddNewBankerName")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.BtnCloseDialog = "CloseModal2";
                html.SaveChangeAndClose = true;
            }
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Banker Name:",
                ID = "TxtBankerName",
                DBField = "BankerName",
                Required = true,
                ISUniqueField = true,
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        public string LoadVendorBankInfo(int VendorID)
        {
            HTML html = new HTML();
            string sql = @"select isnull(v.BankerName, '') BankerName, isnull(v.SwiftCode, '') SwiftCode, isnull(v.AccountNo, '') AccountNo, isnull(v.AccountName, '') AccountName from dbo.TblVendor v
where VendorID = @VendorID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "VendorID", VendorID.ToString() },
            };
            return html.JsonData(sql, false, para);
        }
        public JsonResult GetCurrentNumberPrefix(string Prefix, int IsEdit, int VendorID)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "VendorID", VendorID.ToString() },
            };
            string sql = IsEdit == 1
                ? @"select Seq CurrentNumber from dbo.TblVendor where VendorID = @VendorID and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '" + html.DomainID + "';"
                : @"select (CurrentNumber + 1) as CurrentNumber from dbo.TblPrefix 
where Module = 'VD' and Prefix = '" + Prefix + "' and SystemID =" + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '" + html.DomainID + "';";
            return Json(new { data = html.JsonData(sql, false, para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdatePrefixVD(int VendorID, int IsEdit)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "VendorID", VendorID.ToString() },
            };
            string sql = @"declare @CurrentNumber int, @CurrentPrefix nvarchar(50);
if(not exists(select top 1 Prefix from dbo.TblPrefix where Module = 'VD' and Prefix='VD-{000}' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '" + html.DomainID + @"'))
begin 
insert into  dbo.TblPrefix (StartNumber, CurrentNumber, Prefix, CurrentPrefix, Module, SystemID, WarehouseID, DomainID)
		values (1, 1, 'VD-{000}', 'VD-001', 'VD', " + html.SystemID + ", " + html.WarehouseID + ", '" + html.DomainID + @"')	
end
else
begin
select @CurrentNumber = Seq, @CurrentPrefix = VendorCode from dbo.TblVendor where VendorID = @VendorID and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '" + html.DomainID + @"';
if(" + IsEdit + @" != 1)
begin
    update dbo.TblPrefix set CurrentNumber = @CurrentNumber, CurrentPrefix = @CurrentPrefix where Module = 'VD' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '" + html.DomainID + @"';
end
end
";
            result = html.SaveChangeData(sql, false, para);
            return Json(new { Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}