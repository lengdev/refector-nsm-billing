using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddSupplierController : Controller
    {
        // GET: AddSupplier
        [Authorize(Roles = "Add Supplier, Edit Supplier, Delete Supplier")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                DialogBoxWidth = "900"
            };
            if (html.HasPermission("Add Supplier"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Supplier"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Supplier"))
                html.AllowDelete = true;

            html.AddButton = "Add Suppiler";
            html.AddURL = "AddSupplier/CreateSupplier";
            html.TableName = "TblSupplier";
            html.PrimaryField = "SupplierID";
            html.OrderBy = "SupplierID desc";

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
                new GridField() { Header = "Telephone", FieldName = "Phone" },
                new GridField() { Header = "Email", FieldName = "Email", isEmail = true },
                new GridField() { Header = "Website", FieldName = "Website", isUrl = true },
                new GridField() { Header = "Action", FieldName = "SupplierID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Supplier, Edit Supplier")]
        public ActionResult CreateSupplier(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                UseTable = true,
                TableColumn = 2,
                FormID = "CreateSupplierForm",
                FieldIDFocus = "TxtSupplierCode",
                TableName = "TblSupplier",
                PrimaryField = "SupplierID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            string DestinationFolder = "/Uploads/Supplier/";
            html.DestinationFile = DestinationFolder;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "AttachmentFile:",
                ID = "TxtAttachmentFile",
                DBField = "AttachmentFile",
                Type = "File",
                Hide = true,
                ISFile = true
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
                Label = "Full Name:",
                ID = "TxtFullName",
                DBField = "SupplierName",
                Required = true
            });

            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Display Name:",
            //    ID = "TxtDisplayName",
            //    DBField = "DisplayName",
            //    Required = true
            //});
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Company:",
                ID = "TxtCompany",
                DBField = "CompanyID",
                SQL_CBO = "select UserCompanyID, (CompanyCode + ' - ' + CompanyName) CompanyName from dbo.TblUserCompany where SystemID =" + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by (CompanyCode + ' - ' + CompanyName)",
                DisplayText = "CompanyName",
                Value = "UserCompanyID",
                Infinity = false,
                placeholder = "None"
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
                Label = "ZIP/Postal Code:",
                ID = "TxtZIPPostalCode",
                DBField = "ZIPPostalCode",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Country:",
                ID = "TxtCountry",
                DBField = "Country"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Phone:",
                ID = "TxtTelephone",
                DBField = "Phone"
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
                Label = "Website:",
                ID = "TxtWebsite",
                DBField = "Website",
                Type = "url"
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "SecondaryEmail:",
            //    ID = "TxtSecondaryEmail",
            //    DBField = "SecondaryEmail",
            //    Type="email"
            //});

            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Owner:",
            //    ID = "TxtOwner",
            //    DBField = "Owner"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "UserName:",
            //    ID = "TxtUserName",
            //    DBField = "UserName"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Password:",
            //    ID = "TxtPassword",
            //    DBField = "Password"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Confirm Password:",
            //    ID = "TxtCPassword",
            //    DBField = "Password"
            //});
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}