using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddPositionController : Controller
    {
        // GET: AddPosition
        [Authorize(Roles = "Add Position, Edit Positon, Delete Position")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Position"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Position"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Position"))
                html.AllowDelete = true;

            html.AddButton = "Add Position";
            html.AddURL = "AddPosition/CreatePosition";
            html.TableName = "TblPosition";
            html.PrimaryField = "PositionID";
            html.OrderBy = "PositionName";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Position Name",
                SQL = "select PositionID, PositionName from dbo.TblPosition where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by PositionName",
                SelectValue = "PositionID",
                DisplayField = "PositionName",
                RadioValue = EncryAndDecry.Encryp("PositionID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Position", FieldName = "PositionName" },
                new GridField() { Header = "Action", FieldName = "PositionID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Position, Edit Positon")]
        public ActionResult CreatePosition(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreatePosition",
                FieldIDFocus = "TxtPositionName",
                TableName = "TblPosition",
                PrimaryField = "PositionID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Position Name:",
                ID = "TxtPositionName",
                DBField = "PositionName",
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