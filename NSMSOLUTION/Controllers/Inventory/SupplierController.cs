using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    public class SupplierController : Controller
    {
        // GET: Supplier
        //[Authorize(Roles = "Add Supplier, Edit Supplier, Delete Supplier")]
        public ActionResult Index()
        {

            HTML html = new HTML
            {
                ReportTitle = "List Supplier",
                DialogBoxWidth = "900",
                //if (html.HasPermission("Add Supplier"))
                AllowAdd = true,
                //if (html.HasPermission("Edit Supplier"))
                AllowEdit = true,
                //if (html.HasPermission("Delete Supplier"))
                AllowDelete = true,

                AddButton = "Add Suppiler",
                AddURL = "AddSupplier/CreateSupplier",
                TableName = "TblSupplier",
                PrimaryField = "SupplierID",
                OrderBy = "SupplierName"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Supplier Name",
                SQL = "select SupplierID, SupplierName from dbo.TblSupplier where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by SupplierName",
                SelectValue = "SupplierID",
                DisplayField = "SupplierName",
                RadioValue = EncryAndDecry.Encryp("SupplierID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Supplier Code", FieldName = "SupplierCode" },
                new GridField() { Header = "Supplier Name", FieldName = "SupplierName" },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Telephone", FieldName = "Telephone" },
                new GridField() { Header = "Email", FieldName = "Email", isEmail = true },
                new GridField() { Header = "Website", FieldName = "Website", isUrl = true },
                new GridField() { Header = "Action", FieldName = "SupplierID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        //[Authorize(Roles = "Add Supplier, Edit Supplier")]
        //[EnableCors(origins: "http://localhost", headers: "*", methods: "*")]
        public ActionResult CreateSupplier(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                UseTable = true,
                TableColumn = 2,
                FormID = "CreateSupplier",
                FieldIDFocus = "TxtFullName",
                TableName = "TblSupplier",
                PrimaryField = "SupplierID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Full Name:",
                ID = "TxtFullName",
                DBField = "FullName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Code:",
                ID = "TxtSupplierCode",
                DBField = "SupplierCode",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Display Name:",
                ID = "TxtDisplayName",
                DBField = "DisplayName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company:",
                ID = "TxtCompany",
                DBField = "Company",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Address:",
                ID = "TxtAddress",
                DBField = "Address"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "City:",
                ID = "TxtCity",
                DBField = "City"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "State Region:",
                ID = "TxtStateRegion",
                DBField = "StateRegion"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Country:",
                ID = "TxtCountry",
                DBField = "Country"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Email:",
                ID = "TxtEmail",
                DBField = "Email",
                Type = "email"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "SecondaryEmail:",
                ID = "TxtSecondaryEmail",
                DBField = "SecondaryEmail",
                Type = "email"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Telephone:",
                ID = "TxtTelephone",
                DBField = "Telephone"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Owner:",
                ID = "TxtOwner",
                DBField = "Owner"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "UserName:",
                ID = "TxtUserName",
                DBField = "UserName"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Password:",
                ID = "TxtPassword",
                DBField = "Password"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Confirm Password:",
                ID = "TxtCPassword",
                DBField = "Password"
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}