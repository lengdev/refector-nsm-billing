using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddStockReasonController : Controller
    {
        // GET: AddItemGroup
        [Authorize(Roles = "Add Stock Reason, Edit Stock Reason, Delete Stock Reason")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Stock Reason"
            };
            if (html.HasPermission("Add Stock Reason"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Stock Reason"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Stock Reason"))
                html.AllowDelete = true;

            html.AddButton = "Add Stock Reason";
            html.AddURL = "AddStockReason/CreateStockReason";
            html.TableName = "TblStockReason";
            html.PrimaryField = "ReasonID";
            html.OrderBy = "Reason";


            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Reason",
                SQL = "select ReasonID, Reason from dbo.TblStockReason where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by Reason",
                SelectValue = "ReasonID",
                DisplayField = "Reason",
                RadioValue = EncryAndDecry.Encryp("ReasonID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Reason", FieldName = "Reason",Width = "100%" },
                new GridField() { Header = "Action", FieldName = "ReasonID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Stock Reason, Edit Stock Reason")]
        public ActionResult CreateStockReason(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML
            {
                FormID = "CreateStockReason",
                FieldIDFocus = "TxtReason",
                TableName = "TblStockReason",
                PrimaryField = "ReasonID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            if (Type == "AddNewStockReason")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                //html.SaveChangeCallbackFunction = "callbackSaveChangeAddNewStockReason";
            }
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Reason:",
                ID = "TxtReason",
                DBField = "Reason",
                Required = true,
                ISUniqueField = true,
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}