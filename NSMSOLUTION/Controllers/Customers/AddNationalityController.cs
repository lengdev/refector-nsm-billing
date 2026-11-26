using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddNationalityController : Controller
    {
        // GET: AddNationality
        [Authorize(Roles = "Add Nationality, Edit Nationality, Delete Nationality")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Nationality"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Nationality"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Nationality"))
                html.AllowDelete = true;

            html.AddButton = "Add Nationality";
            html.AddURL = "AddNationality/CreateNationality";
            html.TableName = "TblNationality";
            html.PrimaryField = "NationalityID";
            html.OrderBy = "NationalityID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Nationality",
                SQL = "select NationalityID, Nationality from dbo.TblNationality where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by Nationality",
                SelectValue = "NationalityID",
                DisplayField = "Nationality",
                RadioValue = EncryAndDecry.Encryp("NationalityID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Nationality En", FieldName = "Nationality" ,Width="100%"},
                new GridField() { Header = "Nationality KH", FieldName = "NationalityKH" },
                new GridField() { Header = "Order", FieldName = "OrderBy" },
                new GridField() { Header = "Action", FieldName = "NationalityID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Nationality, Edit Nationality")]
        public ActionResult CreateNationality(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateNationality",
                FieldIDFocus = "TxtNationality",
                TableName = "TblNationality",
                PrimaryField = "NationalityID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Nationality En:",
                ID = "TxtNationality",
                DBField = "Nationality",
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Nationality KH:",
                ID = "TxtNationalityKH",
                DBField = "NationalityKH",
                ISUniqueField = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Order:",
                ID = "TxtOrderBy",
                DBField = "OrderBy",
                ISNumeric = true,
                ISInteger = true,
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}