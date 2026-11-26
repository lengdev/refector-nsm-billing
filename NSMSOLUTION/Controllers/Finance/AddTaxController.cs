using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddTaxController : Controller
    {
        // GET: AddTax
        [Authorize(Roles = "Add Tax, Edit Tax, Delete Tax")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Tax"
            };
            if (html.HasPermission("Add Tax"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Tax"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Tax"))
                html.AllowDelete = true;

            html.AddButton = "Add Tax";
            html.AddURL = "AddTax/CreateTax";
            html.TableName = "TblTax";
            html.PrimaryField = "TaxID";
            html.OrderBy = "TaxID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Tax Name",
                SQL = "select TaxID, TaxName from dbo.TblTax where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by TaxName",
                SelectValue = "TaxID",
                DisplayField = "TaxName",
                RadioValue = EncryAndDecry.Encryp("TaxID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Tax", FieldName = "TaxName" },
                new GridField() { Header = "Rate", FieldName = "Rate", TextAlign="right", Format="#",Width = "100%" },
                new GridField() { Header = "Action", FieldName = "TaxID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Tax, Edit Tax")]
        public ActionResult CreateTax(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateTax",
                FieldIDFocus = "TxtTaxName",
                TableName = "TblTax",
                PrimaryField = "TaxID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Tax Name:",
                ID = "TxtTaxName",
                DBField = "TaxName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Rate (%):",
                ID = "TxtTaxRate",
                DBField = "Rate",
                Required = true,
                ISNumeric = true,
                ISInteger = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}