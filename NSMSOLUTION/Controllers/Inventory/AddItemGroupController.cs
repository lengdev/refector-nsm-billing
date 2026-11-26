using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddItemGroupController : Controller
    {
        // GET: AddItemGroup
        [Authorize(Roles = "Add Item Group, Edit Item Group, Delete Item Group")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Item Group"
            };
            if (html.HasPermission("Add Item Group"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Item Group"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Item Group"))
                html.AllowDelete = true;

            html.AddButton = "Add Item Group";
            html.AddURL = "AddItemGroup/CreateItemGroup";
            html.TableName = "TblItemGroup";
            html.PrimaryField = "ItemGroupID";
            html.OrderBy = "ItemGroup";
            html.DialogBoxHeight = 300;
            html.DialogBoxWidth = "700";
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "Cbo_Stock_Name",
                Label = "Stock Name:",
                DBField = "s.StockNameID",
                SQL_CBO = "select distinct s.StockNameID, s.StockName from dbo.TblStockName s inner join dbo.TblItemGroup g on s.StockNameID = g.StockNameID where g.SystemID = " + html.SystemID + " and g.WarehouseID=" + html.WarehouseID,
                DisplayText = "StockName",
                Value = "StockNameID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Group",
                SQL = "select ItemGroupID, ItemGroup from dbo.TblItemGroup where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ItemGroup",
                SelectValue = "ItemGroupID",
                DisplayField = "ItemGroup",
                RadioValue = EncryAndDecry.Encryp("ItemGroupID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string SQL = "select ROW_NUMBER() OVER(ORDER BY g.ItemGroup) AS[No], g.ItemGroupID, g.ItemGroup, s.StockName, g.Memo ";
            SQL += "from dbo.TblItemGroup g inner join dbo.TblStockName s on s.StockNameID = g.StockNameID ";
            SQL += " where g.SystemID = " + html.SystemID + " and g.WarehouseID = " + html.WarehouseID;
            html.GH.SQL = SQL;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "s.StockName, g.ItemGroup";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Stock Name", FieldName = "StockName", IsGroupByField = true, Colspan = 4 },
                new GridField() { Header = "Item Group", FieldName = "ItemGroup" },
                new GridField() { Header = "Memo", FieldName = "Memo" , Width = "100%"},
                new GridField() { Header = "Action", FieldName = "ItemGroupID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Item Group, Edit Item Group")]
        public ActionResult CreateItemGroup(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML
            {
                FormID = "CreateItemGroup",
                FieldIDFocus = "Txt_Item_Group",
                TableName = "TblItemGroup",
                PrimaryField = "ItemGroupID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            if (Type == "NewItem")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
                //html.SaveChangeCallbackFunction = "callbackSaveChangeAddNewItem();";
            }
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Stock Name:",
                ID = "Txt_Stock_Name",
                DBField = "StockNameID",
                SQL_CBO = "select StockNameID, StockName from dbo.TblStockName where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by StockName",
                Value = "StockNameID",
                DisplayText = "StockName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Item Group:",
                ID = "Txt_Item_Group",
                DBField = "ItemGroup",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithControlName = "Txt_Stock_Name",
                CheckExistingWithField = "StockNameID"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo"
            });
            html.RestFields = "Txt_Item_Group";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}