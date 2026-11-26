using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    public class StockAlertController : Controller
    {
        // GET: StockStatus
        [Authorize(Roles = "List Report Stock Alert")]
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
                ID = "txt_Item_Group_StockStatus",
                //ISAutoComplete = true,
                Label = "Item Group:",
                DBField = "ItemGroup",
                SQL_CBO = "select distinct ig.ItemGroup from dbo.TblStockInDetail s inner join dbo.TblItem i on i.ItemID = s.ItemID inner join dbo.TblItemGroup ig on ig.ItemGroupID = i.ItemGroupID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                DisplayText = "ItemGroup",
                Value = "ItemGroup",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                ISAutoComplete = true,
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("ItemCode") + "," + EncryAndDecry.Encryp("SerialNumber"),
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Code",
                SQL = "select distinct top 10 iv.ItemCode from dbo.TblItem i inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID where lower(iv.ItemCode) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by iv.ItemCode",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("ItemCode") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Serial Number",
                SQL = "select distinct top 10 case when isnull(i.SerialNumber, '') <> '' then i.SerialNumber else srd.SerialNumber end [SerialNumber]\r\nfrom TblItem i \r\ninner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID \r\nleft join dbo.TblItemSerialDetail srd on srd.ItemID = i.ItemID\r\nwhere isnull(i.SerialNumber, '') <> '' and lower(case when isnull(i.SerialNumber, '') <> '' then i.SerialNumber else srd.SerialNumber end) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("ItemCode") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridProcedureName = "SP_SHOW_STOCK_ALERT";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "ItemGroup", FieldName = "ItemGroup", GroupLabel="Item Group: ", IsGroupByField = true, Colspan = 5 },
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true },
                new GridField() { Header = "Item Code", FieldName = "ItemCode" },
                new GridField() { Header = "Stock On Hand", FieldName = "StockOnHand", ClassName="text-primary", TextAlign="right" },
                new GridField() { Header = "Qty Alert", FieldName = "ReorderPoint" , ClassName="text-red", TextAlign="right" },
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}