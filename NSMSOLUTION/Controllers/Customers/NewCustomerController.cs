using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Customers
{
    [Authorize]
    public class NewCustomerController : Controller
    {
        // GET:NewCustomer
        [Authorize(Roles = "Add Customer, Edit Customer, Delete Customer")]
        public ActionResult Index(string FormStatus = "")
        {
            HTML html = new HTML
            {
                ReportTitle = "List Customers"
            };
            if (html.HasPermission("Add Customer"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Customer"))
                html.AllowEdit = true;

            html.DialogBoxWidth = "Auto";
            html.DialogBoxHeight = 600;
            html.AddButton = "New Register Customer";
            html.AddURL = "AddCustomer/CreateCustomerV2/?FormType=NewRegister";
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Register Date:"
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtLocation",
                Label = "Location:",
                DBField = "Location",
                SQL_CBO = "select distinct CustomerProvinceCity as Location from dbo.TblCustomer where SystemID = " + html.SystemID,
                DisplayText = "Location",
                Value = "Location",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Customer Group:",
                DBField = "CustomerGroupID",
                SQL_CBO = "select distinct cg.CustomerGroup, cg.CustomerGroupID from dbo.TblCustomerGroup cg where cg.SystemID = " + html.SystemID + " and cg.WarehouseID = " + html.WarehouseID + " order by cg.CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroupID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType from dbo.TblCustomerType ct where ct.SystemID = " + html.SystemID + " and ct.WarehouseID=" + html.WarehouseID,
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 c.CustomerName from dbo.TblCustomer c where lower(c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " union all select top 10 c.CustomerNameLatin as CustomerName from dbo.TblCustomer c where lower(c.CustomerNameLatin) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Identification/Passport",
                SQL = "select distinct top 10 c.CustomerIdentification from dbo.TblCustomer where lower(c.CustomerIdentification) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " order by c.CustomerIdentification",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
                Hide = true
            });

            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_SEARCH_CUSTOMER"
            };

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID="},
                new GridField() { Header = "CustomerGroup", FieldName = "CustomerGroup", },
                new GridField() { Header = "Block", FieldName = "CustomerType", },
                new GridField() { Header = "Phone Number", FieldName = "PhoneNumber" },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", ClassName="text-danger text-center" },
                new GridField() { Header = "Register By", FieldName = "RegisterBy" },
                new GridField() { Header = "Location", FieldName = "Location" },
                new GridField() { Header = "Sale Name", FieldName = "SaleName" },
                new GridField() { Header = "Status", FieldName = "CustomerStatus",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string>
                    {
                        { "Enable", "badge-primary" },
                        { "Disable", "badge-danger" },
                    }
                },
                new GridField() { Header = "Modify By", FieldName = "ModifyBy" },
                new GridField() { Header = "Modify Date", FieldName = "ModifyDate", FormatDateTime="dd-MM-yyyy", TextAlign="center" },
                new GridField() { Header = "Action", FieldName = "CustomerID", TextAlign = "Center",
                    IsModifyField = true
                }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
    }
}