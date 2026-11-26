using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddDeviceSplitterNameController : Controller
    {
        // GET: AddDeviceSplitterName
        [Authorize(Roles = "Add Device Splitter Name, Edit Device Splitter Name, Delete Device Splitter Name")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Device Splitter Name"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Device Splitter Name"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Device Splitter Name"))
                html.AllowDelete = true;

            html.AddButton = "Add Device Splitter Name";
            html.AddURL = "AddDeviceSplitterName/CreateSplitterName";
            html.TableName = "TblDeviceSplitterName";
            html.PrimaryField = "SplitterNameID";
            html.OrderBy = "SplitterNameID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Splitter Name",
                SQL = "select SplitterNameID, SplitterName from dbo.TblDeviceSplitterName where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by SplitterName",
                SelectValue = "SplitterNameID",
                DisplayField = "SplitterName",
                RadioValue = EncryAndDecry.Encryp("SplitterNameID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Splitter Name", FieldName = "SplitterName",Width = "100%" },
                new GridField() { Header = "Action", FieldName = "SplitterNameID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Device Splitter Name, Edit Device Splitter Name")]
        public ActionResult CreateSplitterName(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateSplitterName",
                FieldIDFocus = "TxtSplitterName",
                TableName = "TblDeviceSplitterName",
                PrimaryField = "SplitterNameID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Splitter Name:",
                ID = "TxtSplitterName",
                DBField = "SplitterName",
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