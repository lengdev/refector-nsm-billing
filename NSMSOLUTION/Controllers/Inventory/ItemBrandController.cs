using MVC;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    [Authorize]
    public class ItemBrandController : Controller
    {
        // GET: ItemBrand
        [Authorize(Roles = "Add Item Brand, Edit Item Brand, Delete Item Brand")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Item Brand"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Item Brand"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Item Brand"))
                html.AllowDelete = true;
            html.ReportTitle = "Item Brand List";
            html.AddButton = "Add Item Brand";
            html.AddURL = "ItemBrand/AddItemBrand";
            html.TableName = "TblItemBrand";
            html.PrimaryField = "ItemBrandID";
            html.FieldName = "ItemBrand";
            html.OrderBy = "ItemBrandID desc";
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Brand",
                SQL = "select ItemBrandID, ItemBrand from dbo.TblItemBrand where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ItemBrand",
                SelectValue = "ItemBrandID",
                DisplayField = "ItemBrand",
                RadioValue = EncryAndDecry.Encryp("ItemBrandID"),
                DefaultCheckBok = true
            });
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Logo", FieldName = "Logo", ImagePath="Uploads/Brand/Logo/" },
                new GridField() { Header = "Item Brand", FieldName = "ItemBrand" ,Width = "100%"},
                new GridField() { Header = "Action", FieldName = "ItemBrandID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Item Brand, Add Item Brand")]
        public ActionResult AddItemBrand(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "NewItem")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                html.BtnCloseDialog = "CloseModal2";
                html.SaveChangeAndClose = true;
            }
            html.FormID = "AddItemBrand";
            html.FieldIDFocus = "TxtItemBrand";
            html.TableName = "TblItemBrand";
            html.PrimaryField = "ItemBrandID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Item Brand:",
                ID = "TxtItemBrand",
                DBField = "ItemBrand",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlFile()
            {
                ID = "Logo",
                Class = "input2 CustInfo",
                DBField = "Logo",
                IsDragDropFile = true,
                AcceptImageOnly = true,
                DestinationFile = "Uploads/Brand/Logo/",
                TableName = "TblItemBrand",
                PrimaryFieldName = "ItemBrandID",
                PrimaryFieldValue = PrimaryFieldValue,
                Align = "right"
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}