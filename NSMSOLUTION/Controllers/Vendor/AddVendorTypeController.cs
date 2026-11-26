using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddVendorTypeController : Controller
    {
        // GET: AddVendorType
        [Authorize(Roles = "Add Vendor Type, Edit Vendor Type, Delete Vendor Type")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Vendor Type"
            };
            if (html.HasPermission("Add Vendor Type"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Vendor Type"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Vendor Type"))
                html.AllowDelete = true;

            html.AddButton = "Add Vendor Type";
            html.AddURL = "AddVendorType/CreateVendorType";
            html.TableName = "TblVendorType";
            html.PrimaryField = "VendorTypeID";
            html.OrderBy = "VendorTypeID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Vendor Type",
                SQL = "select VendorTypeID, VendorType from dbo.TblVendorType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by VendorType",
                SelectValue = "VendorTypeID",
                DisplayField = "VendorType",
                RadioValue = EncryAndDecry.Encryp("VendorTypeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Vendor Type", FieldName = "VendorType" },
                new GridField() { Header = "Prefix", FieldName = "Prefix" },
                new GridField() { Header = "Memo", FieldName = "Memo" ,Width="100%"},
                new GridField() { Header = "Action", FieldName = "VendorTypeID", TextAlign = "Center",  IsModifyField = true }
            };

            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Vendor Type, Edit Vendor Type")]
        public ActionResult CreateVendorType(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "AddNewVendorType")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.BtnCloseDialog = "CloseModal2";
                html.SaveChangeAndClose = true;
            }
            html.FormID = "CreateVendorType";
            html.FieldIDFocus = "TxtVendorType";
            html.TableName = "TblVendorType";
            html.PrimaryField = "VendorTypeID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Vendor Type:",
                ID = "TxtVendorType",
                DBField = "VendorType",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Value = "VD-{000}",
                Label = "Prfix:",
                ID = "TxtPrefix",
                DBField = "Prefix",
                Required = true,
                ExLabel = "Ex: AnyPrefix{yy} = Year, {MM} = Month, {dd} = Day, {000}= Number"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo"
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}