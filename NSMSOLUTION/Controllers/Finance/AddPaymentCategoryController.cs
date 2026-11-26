using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddPaymentCategoryController : Controller
    {
        // GET: AddPaymentCategory
        [Authorize(Roles = "Add Payment Category, Edit Payment Category, Delete Payment Category")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Payment Category"
            };
            if (html.HasPermission("Add Payment Category"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Payment Category"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Payment Category"))
                html.AllowDelete = true;

            html.AddButton = "Add Payment Category";
            html.AddURL = "AddPaymentCategory/CreatePaymentCategory";
            html.TableName = "TblPaymentCategory";
            html.PrimaryField = "PaymentCategoryID";
            html.OrderBy = "Category";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Category",
                SQL = "select PaymentCategoryID, Category from dbo.TblPaymentCategory where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by Category",
                SelectValue = "PaymentCategoryID",
                DisplayField = "Category",
                RadioValue = EncryAndDecry.Encryp("PaymentCategoryID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Category", FieldName = "Category" , Width = "100%"},
                new GridField() { Header = "Action", FieldName = "PaymentCategoryID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Payment Category, Edit Payment Category")]
        public ActionResult CreatePaymentCategory(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "NewPaymentCategory")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.SaveChangeCallbackFunction = "callbackSaveChangeAddNewPaymentCategory();";
            }
            html.FormID = "CreatePaymentCategory";
            html.FieldIDFocus = "TxtCategory";
            html.TableName = "TblPaymentCategory";
            html.PrimaryField = "PaymentCategoryID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Category:",
                ID = "TxtCategory",
                DBField = "Category",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}