using MVC;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    [Authorize]
    public class ItemSizeController : Controller
    {
        // GET: ItemSize
        [Authorize(Roles = "Add Item Size, Edit Item Size, Delete Item Size")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Item Size"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Item Size"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Item Size"))
                html.AllowDelete = true;
            html.ReportTitle = "Item Size List";
            html.AddButton = "Add Item Size";
            html.AddURL = "ItemSize/AddItemSize";
            html.TableName = "TblItemSize";
            html.PrimaryField = "ItemSizeID";
            html.FieldName = "ItemSize";
            html.OrderBy = "ItemSizeID desc";
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Size",
                SQL = "select ItemSizeID, ItemSize from dbo.TblItemSize where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ItemSize",
                SelectValue = "ItemSizeID",
                DisplayField = "ItemSize",
                RadioValue = EncryAndDecry.Encryp("ItemSizeID"),
                DefaultCheckBok = true
            });
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Item Size", FieldName = "ItemSize" },
                new GridField() { Header = "Action", FieldName = "ItemSizeID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Item Size, Add Item Size")]
        public ActionResult AddItemSize(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "NewItem")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.BtnCloseDialog = "CloseModal2";
                html.SaveChangeAndClose = true;
            }
            html.FormID = "AddItemSize";
            html.FieldIDFocus = "TxtItemSize";
            html.TableName = "TblItemSize";
            html.PrimaryField = "ItemSizeID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Item Size:",
                ID = "TxtItemSize",
                DBField = "ItemSize",
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