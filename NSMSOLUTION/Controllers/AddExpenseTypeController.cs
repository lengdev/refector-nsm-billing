using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddExpenseTypeController : Controller
    {
        // GET: AddExpenseType
        //[Authorize(Roles = "Add System, Edit System, Delete System")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Expense Type"))
                html.AllowAdd = true;
            //if (html.HasPermission("Edit System"))
            html.AllowEdit = true;
            //if (html.HasPermission("Delete System"))
            html.AllowDelete = true;

            html.AddButton = "Add Expense Type";
            html.AddURL = "AddExpenseType/CreateExpeseTypeForm";
            html.TableName = "TblExpenseType";
            html.PrimaryField = "ExpenseTypeID";

            html.OrderBy = "ExpenseType";
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Expense Type",
                SQL = "select ExpenseTypeID, ExpenseType from dbo.TblExpenseType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ExpenseType",
                SelectValue = "ExpenseTypeID",
                DisplayField = "ExpenseType",
                RadioValue = EncryAndDecry.Encryp("ExpenseTypeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Expense Type", FieldName = "ExpenseType" },
                new GridField() { Header = "Action", FieldName = "ExpenseTypeID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        //[Authorize(Roles = "Add System, Edit System")]
        public ActionResult CreateExpeseTypeForm(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateExpenseTypeForm",
                FieldIDFocus = "TxtExpenseType",
                TableName = "TblExpenseType",
                PrimaryField = "ExpenseTypeID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Expense Type:",
                ID = "TxtExpenseType",
                DBField = "ExpenseType",
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