using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddItemTypeController : Controller
    {
        // GET: AddItemType
        [Authorize(Roles = "Add Item Type, Edit Item Type, Delete Item Type")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Item Type"
            };
            if (html.HasPermission("Add Item Type"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Item Type"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Item Type"))
                html.AllowDelete = true;

            html.AddButton = "Add Item Type";
            html.AddURL = "AddItemType/CreateItemType";
            html.TableName = "TblItemType";
            html.PrimaryField = "ItemTypeID";
            html.OrderBy = "ItemTypeID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Type",
                SQL = "select ItemTypeID, TypeName from dbo.TblItemType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by TypeName",
                SelectValue = "ItemTypeID",
                DisplayField = "TypeName",
                RadioValue = EncryAndDecry.Encryp("ItemTypeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Type", FieldName = "TypeName" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Action", FieldName = "ItemTypeID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Item Type, Edit Item Type")]
        public ActionResult CreateItemType(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateItemType",
                FieldIDFocus = "TxtItemType",
                TableName = "TblItemType",
                PrimaryField = "ItemTypeID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Type:",
                ID = "TxtItemType",
                DBField = "TypeName",
                Required = true,
                ISUniqueField = true
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