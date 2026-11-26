using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    public class StockOnHandReportController : Controller
    {
        // GET: InventorySummary
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Inventory"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                FromDateID = "txtStockSummaryDate",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtWarehouseName",
                Label = "Warehouse Name:",
                DBField = "WarehouseName",
                SQL_CBO = "select distinct uw.WID, uw.WarehouseName from dbo.TblStockIn b\r\ninner join dbo.TblUserWarehouse uw on uw.WID = b.WID\r\ninner join dbo.TblBranch br on br.BranchID = uw.BranchID\r\n where b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID + " order by uw.WarehouseName",
                DisplayText = "WarehouseName",
                Value = "WID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCategories",
                Label = "Categories:",
                DBField = "CategoryName",
                SQL_CBO = "select distinct sn.StockNameID, sn.StockName CategoryName from dbo.TblStockName sn where sn.SystemID = " + html.SystemID + " and sn.WarehouseID = " + html.WarehouseID + " order by sn.StockName",
                DisplayText = "CategoryName",
                Value = "StockNameID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Item_Group",
                Label = "Item Group:",
                DBField = "ItemGroup",
                SQL_CBO = "select distinct ig.ItemGroupID, ig.ItemGroup from dbo.TblStockInDetail s inner join dbo.TblItem i on i.ItemID = s.ItemID inner join dbo.TblItemGroup ig on ig.ItemGroupID = i.ItemGroupID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by ig.ItemGroup",
                DisplayText = "ItemGroup",
                Value = "ItemGroupID",
                ComboBox = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtItemCode",
                Label = "Item Code:",
                DBField = "ItemCode",
                SQL_CBO = "select distinct i.ItemCode from TblItem i where isnull(i.ItemCode, '') <> '' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                DisplayText = "ItemCode",
                Value = "ItemCode",
                ComboBox = true,
                Multiple = true,
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                ISAutoComplete = true,
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i left join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode"),
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {

                OPTLabel = "Barcode",
                SQL = "select distinct top 10 iv.Barcode from TblItem i inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID where isnull(iv.Barcode, '') <> '' and lower(iv.Barcode) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by iv.Barcode",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.TblID = "tblStockSummary";
            html.FormID = "frmStockSummary";
            html.GH.GridProcedureName = "SP_SHOW_STOCK_ON_HAND";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "ItemGroup", FieldName = "ItemGroup", GroupLabel="Item Group: ", IsGroupByField = true, Colspan=8},
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                new GridField() { Header = "Item Code", FieldName = "ItemCode", TextAlign="center" },
                new GridField() { Header = "Item Size", FieldName = "ItemSize", TextAlign="center" },
                new GridField() { Header = "Barcode", FieldName = "Barcode", TextAlign="center" },
                new GridField() { Header = "Category", FieldName = "Category" },
                new GridField() { Header = "Warehouse", FieldName = "WarehouseName", TotalLabel = "Total QOH:"},
                new GridField() { Header = "Stock On Hand", FieldName = "StockOnHand", Format="n0", ClassName="text-right text-primary", IsTotalFooter=true},
                new GridField() { Header = "UOM", FieldName = "UM", TextAlign="center"},
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}