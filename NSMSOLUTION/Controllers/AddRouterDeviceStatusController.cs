using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class AddRouterDeviceStatusController : Controller
    {
        // GET: AddRouterDeviceStatus
        [Authorize(Roles = "Add Router Device Status, Edit Router Device Status, Delete Router Device Status")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Router Device Status"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Router Device Status"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Router Device Status"))
                html.AllowDelete = true;

            html.AddButton = "Add Router Device Status";
            html.AddURL = "AddRouterDeviceStatus/AddRouterDeviceStatus";
            html.TableName = "TblRouterDeviceStatus";
            html.PrimaryField = "StatusID";
            html.OrderBy = "StatusID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Status",
                SQL = "select StatusID, Status from dbo.TblRouterDeviceStatus where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by Status",
                SelectValue = "StatusID",
                DisplayField = "Status",
                RadioValue = EncryAndDecry.Encryp("StatusID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Action", FieldName = "StatusID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Router Device Status, Edit Router Device Status")]
        public ActionResult AddRouterDeviceStatus(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "NewItem")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
            }
            html.FormID = "RouterDeviceStatus";
            html.FieldIDFocus = "TxtStatus";
            html.TableName = "TblRouterDeviceStatus";
            html.PrimaryField = "StatusID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Status:",
                ID = "TxtStatus",
                DBField = "Status",
                Required = true,
                ISUniqueField = true
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo",
                Required = true,
                ISUniqueField = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}