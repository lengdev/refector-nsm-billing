using MVC;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    public class StockStatusController : Controller
    {
        // GET: StockStatus
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Stock Status",
                BunSearch = "BtnSearchStockStatus"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                FromDateID = "txtStockStatusDate",
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
                ID = "txtStockName",
                Label = "Stock Name:",
                DBField = "StockName",
                SQL_CBO = "select sn.StockNameID, sn.StockName from dbo.TblStockName sn inner join dbo.TblStockIn si on si.StockInID = sn.StockNameID where sn.SystemID = " + html.SystemID + " and sn.WarehouseID = " + html.WarehouseID + " order by sn.StockName",
                DisplayText = "StockName",
                Value = "StockNameID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Item_Group_StockStatus",
                Label = "Item Group:",
                DBField = "ItemGroup",
                SQL_CBO = "select distinct ig.ItemGroup from dbo.TblStockInDetail s inner join dbo.TblItem i on i.ItemID = s.ItemID inner join dbo.TblItemGroup ig on ig.ItemGroupID = i.ItemGroupID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                DisplayText = "ItemGroup",
                Value = "ItemGroup",
                ComboBox = true
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                ISAutoComplete = true,
                SQL = "select distinct top 10 i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end) ItemName from dbo.TblStockIn si inner join dbo.TblVendor v on v.VendorID = si.VendorID inner join dbo.TblStockInDetail sd on sd.StockInID = si.StockInID inner join dbo.TblItem i on i.ItemID = sd.ItemID where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)) like @SearchText and si.SystemID = " + html.SystemID + " and si.WarehouseID=" + html.WarehouseID + " order by i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)",
                RadioValue = EncryAndDecry.Encryp("ItemName"),
                DefaultCheckBok = true,
                Name = "lbl_Item_Name_StockStatus",
                ID = "TXT_Item_Name_StockStatus"
            });
            html.TblID = "tblStockStatus";
            html.FormID = "frmStockStatus";
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 3;
            html.GH.GridProcedureName = "SP_SHOW_STOCK_STATUS";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "WarehouseName", FieldName = "WarehouseName", IsGroupByField = true, Colspan = 10, DisplayOneRow= true },
                new GridField() { Header = "Stock Name", FieldName = "StockName", GroupLabel="StockName: ", IsGroupByField = true, Colspan = 10 },
                new GridField() { Header = "ItemGroup", FieldName = "ItemGroup", GroupLabel="Item Group: ", IsGroupByField = true, Colspan = 10 },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                new GridField() { Header = "IN", FieldName = "QTYIN", TextAlign = "right", Format="#,#0" },
                new GridField() { Header = "OUT", FieldName = "QTYOUT", TextAlign = "right", Format="#,#0", ClassName="text-red" },
                new GridField() { Header = "Available", FieldName = "AvailableStock", TextAlign = "right", Format="#,#0", ClassName="text-red" },
                new GridField() { Header = "Status", FieldName = "Status", DataTableNumber = 2, LinkField="WarehouseName, ItemGroup", DisplayFields="Total QTY IN, Total QTY OUT", TextAlign = "center" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
    }
}