using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    public class InventorySummaryController : Controller
    {
        // GET: InventorySummary
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Inventory",
                BunSearch = "BtnSearchStockSummary"
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
                ID = "txtAlertStockStatus",
                Label = "Alert Stock Status:",
                DBField = "AlertStockStatus",
                SQL_CBO = "select 'Available stock' AlertStockStatus union all select 'Nearly out of stock' AlertStockStatus union all select 'Out of stock' AlertStockStatus",
                DisplayText = "AlertStockStatus",
                Value = "AlertStockStatus",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBrokenStatus",
                Label = "Broken:",
                DBField = "BrokenStatus",
                SQL_CBO = "select 'Broken' BrokenStatus union all select 'Not Broken' BrokenStatus",
                DisplayText = "BrokenStatus",
                Value = "BrokenStatus",
                ComboBox = true
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
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("SerialNumber"),
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {

                OPTLabel = "Barcode",
                SQL = "select distinct top 10 iv.Barcode from TblItem i inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID where isnull(iv.Barcode, '') <> '' and lower(iv.Barcode) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by iv.Barcode",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Serial Number",
                SQL = @"SELECT distinct top 10 SerialNumber
FROM (
    SELECT SerialNumber 
    FROM TblItem
    UNION
    SELECT SerialNumber 
    FROM TblItemVariant
    UNION
    SELECT SerialNumber 
    FROM TblItemSerialDetail
) AS CombinedResults
WHERE ISNULL(SerialNumber, '') <> '' 
AND LOWER(SerialNumber) LIKE  @SearchText
ORDER BY SerialNumber;",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.TblID = "tblStockSummary";
            html.FormID = "frmStockSummary";
            html.GH.GridProcedureName = "SP_SHOW_Inventory_Summary";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "ItemGroup", FieldName = "ItemGroup", GroupLabel="Item Group: ", IsGroupByField = true, Colspan = 8 },
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true },
                new GridField() { Header = "Item Code", FieldName = "ItemCode" },
                new GridField() { Header = "Barcode", FieldName = "Barcode" },
                new GridField() { Header = "Warehouse", FieldName = "WarehouseName"},
                new GridField() { Header = "UOM", FieldName = "UM", TextAlign="center", TotalLabel="Total:"},
                new GridField() { Header = "QTY IN", FieldName = "QTYIN", TextAlign = "right", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "QTY OUT", FieldName = "QTYOUT", ClassName="text-right text-red", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "Stock Return", FieldName = "StockReturn", TextAlign = "right", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "Broken", FieldName = "QuantityBroken", TextAlign = "right", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "Stock On Hand", FieldName = "StockOnHand", ClassName="text-right text-primary", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "Alert Stock Status", FieldName = "AlertStockStatus", ClassName="text-center"},
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}