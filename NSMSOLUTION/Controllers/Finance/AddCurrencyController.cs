using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddCurrencyController : Controller
    {
        // GET: AddCurrency
        [Authorize(Roles = "Add Currency, Edit Currency, Delete Currency")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Currency"
            };
            if (html.HasPermission("Add Currency"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Currency"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Currency"))
                html.AllowDelete = true;

            html.AddButton = "Add Currencies";
            html.AddURL = "AddCurrency/CreateCurrency";
            html.TableName = "TblCurrency";
            html.PrimaryField = "CurrencyID";
            html.OrderBy = "CurrencyID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Currency Name",
                SQL = "select CurrencyID, CurrencyName from dbo.TblCurrency where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CurrencyName",
                SelectValue = "CurrencyID",
                DisplayField = "CurrencyName",
                RadioValue = EncryAndDecry.Encryp("CurrencyID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Currency Name", FieldName = "CurrencyName",Width="100%" },
                new GridField() { Header = "Currency Symbol", FieldName = "CurrencySymbol" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Action", FieldName = "CurrencyID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Currency, Edit Currency")]
        public ActionResult CreateCurrency(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateCurrency",
                FieldIDFocus = "TxtCurrencyName",
                TableName = "TblCurrency",
                PrimaryField = "CurrencyID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Currency Name:",
                ID = "TxtCurrencyName",
                DBField = "CurrencyName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Currency Symbol:",
                ID = "TxtCurrencySymbol",
                DBField = "CurrencySymbol",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo",
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}