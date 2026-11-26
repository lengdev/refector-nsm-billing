using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddCustomerTypeController : Controller
    {
        // GET: AddCustomerType
        [Authorize(Roles = "Add Customer Type, Edit Customer Type, Delete Customer Type")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            html.GH.SQL = @"select ROW_NUMBER() over (order by ct.DistrictCode) as [No], ct.CreateBy,
ct.CustomerType, ct.CustomerTypeID, ct.DistrictCode, ct.Prefix, ct.StartNumber, ct.Memo , ct.OrderBy, 
case when ct.Status = 'Enable' then '<span class=""badge badge-primary"">'+ct.Status+'</span>' else '<span class=""badge badge-danger"">'+ct.Status+'</span>' end as Status
from TblCustomerType ct
where ct.SystemID = " + html.SystemID + " and ct.WarehouseID = " + html.WarehouseID;
            //--cg.CustomerGroup, inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = ct.CustomerGroupID
            html.IsHaveWhereCondition = true;
            html.OrderBy = "ct.DistrictCode, ct.CustomerType";
            html.ReportTitle = "List Customers Type";
            if (html.HasPermission("Add Customer Type"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Customer Type"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Customer Type"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "900";
            html.AddButton = "Add Customer Type";
            html.AddURL = "AddCustomerType/CreateCustomerType";
            html.TableName = "TblCustomerType";
            html.PrimaryField = "CustomerTypeID";
            string sql = @"select distinct DistrictCode from dbo.TblCustomerType where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            html.ReportTitle = "Customer Block";

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                Label = "Status:",
                DBField = "Status",
                SQL_CBO = "select 'Enable' [Status]\r\nunion \r\nselect 'Disable' [Status]",
                DisplayText = "Status",
                Value = "Status",
                ComboBox = true
            });
            //html.HtmlAddControls.Add(new HtmlAddControls()
            //{
            //	ID = "txtDistrictCode",
            //	Label = "District Code:",
            //	DBField = "DistrictCode",
            //	SQL_CBO = sql,
            //	DisplayText = "DistrictCode",
            //	Value = "DistrictCode",
            //	ComboBox = true
            //});
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Type",
                SQL = "select CustomerTypeID, CustomerType from dbo.TblCustomerType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerType",
                SelectValue = "CustomerTypeID",
                DisplayField = "CustomerType",
                RadioValue = EncryAndDecry.Encryp("ct.CustomerTypeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                //new GridField() { Header = "Customer Group", FieldName = "CustomerGroup" },
                new GridField() { Header = "Customer Type", FieldName = "CustomerType" },
				//new GridField() { Header = "District Code", FieldName = "DistrictCode" },
				new GridField() { Header = "Prefix", FieldName = "Prefix" },
                new GridField() { Header = "Start Number", FieldName = "StartNumber" },
                new GridField() { Header = "Order", FieldName = "OrderBy" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center"},
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Action", FieldName = "CustomerTypeID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Customer Type, Edit Customer Type")]
        public ActionResult CreateCustomerType(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "AddNewCustomerType")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.SaveChangeCallbackFunction = "callbackSaveChangeAddCustomerType();";
            }
            html.UseTable = true;
            html.TableColumn = 2;
            html.FormID = "CreateCustomerType";
            html.FieldIDFocus = "TxtCustomerType";
            html.TableName = "TblCustomerType";
            html.PrimaryField = "CustomerTypeID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Customer Group:",
            //    ID = "TxtCustomerGroup",
            //    DBField = "CustomerGroupID",
            //    Required = true,
            //    SQL_CBO= "select cg.CustomerGroupID, cg.CustomerGroup from dbo.TblCustomerGroup cg where cg.SystemID="+html.SystemID+" and cg.WarehouseID = "+html.WarehouseID+" order by cg.CustomerGroup",
            //    DisplayText= "CustomerGroup",
            //    Value= "CustomerGroupID"
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Customer Type:",
                ID = "TxtCustomerType",
                DBField = "CustomerType",
                Required = true,
                //ISUniqueFiled = true,
                //CheckExsitingWithField = "WarehouseID",
                //CheckExsitingWithDefaultValue = html.WarehouseID
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //	Label = "District Code:",
            //	ID = "TxtDistrictCode",
            //	DBField = "DistrictCode",
            //	Required = true,
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "CUS-{000}",
                Label = "Prefix:",
                ID = "TxtPrefix",
                DBField = "Prefix",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //	Value = "TPP{000000}",
            //	Label = "Prefix VAT:",
            //	ID = "PrefixVAT",
            //	DBField = "PrefixVAT",
            //	Required = true,
            //	ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //	Value = "CPP{000000}",
            //	Label = "Prefix CM:",
            //	ID = "PrefixCM",
            //	DBField = "PrefixCM",
            //	Required = true,
            //	ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "1",
                Label = "Start Number:",
                ID = "TxtStartNumber",
                DBField = "StartNumber",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Order:",
                ID = "TxtOrderBy",
                DBField = "OrderBy",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
                IsAllowInputOnlyNumber = true,
            });
            //         html.HTMLControls.Add(new HtmlComboBox()
            //         {
            //             Label = "IS VAT:",
            //             ID = "TxtISVAT",
            //             DBField = "ISVAT",
            //             Required = true,
            //             SQL_CBO= "select 0 ISVATID, 'None VAT' [ISVAT]\r\nunion \r\nselect 1 ISVATID, 'VAT' [ISVAT]",
            //             Value= "ISVATID", DisplayText= "ISVAT"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Value = "Connection-{00}",
            //    Label = "CO. Non VAT:",
            //    ID = "TxtCOIDNonVAT",
            //    DBField = "COIDNonVAT",
            //    Required = true,
            //    ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Value = "CON{000000}",
            //    Label = "CO. VAT:",
            //    ID = "TxtCOIDVAT",
            //    DBField = "COIDVAT",
            //    Required = true,
            //    ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Value = "SID{000000}",
            //    Label = "SID Non VAT:",
            //    ID = "TxtSIDNonVAT",
            //    DBField = "SIDNonVAT",
            //    Required = true,
            //    ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Value = "SID{000000}",
            //    Label = "SID VAT:",
            //    ID = "TxtSIDVAT",
            //    DBField = "SIDVAT",
            //    Required = true,
            //    ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            //});
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Status:",
                ID = "TxtStatus",
                DBField = "Status",
                Required = true,
                SQL_CBO = "select 'Enable' [Status]\r\nunion \r\nselect 'Disable' [Status]",
                Value = "Status",
                DisplayText = "Status"
            });
            //html.HTMLControls.Add(new HtmlTextarea()
            //{
            //	Label = "Memo:",
            //	ID = "TxtMemo",
            //	DBField = "Memo"
            //});

            if (PrimaryFieldValue != "0")
            {
                html.insTable.Add(
                    new InsertTable()
                    {
                        PrimaryFieldValue = PrimaryFieldValue.ToString(),
                        TableName = "TblPrefix",
                        Fields = new
                        {
                            CurrentNumber = "$(\"#TxtStartNumber\").val().toString()",
                        },
                        UpdateDateConditionFields = new
                        {
                            Module = "'Customer'",
                            Prefix = "$(\"#TxtPrefix\").val().toString()",
                            SystemID = "'" + html.SystemID + "'",
                            WarehouseID = "'" + html.WarehouseID + "'"
                        }
                    }
               );
            }
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}