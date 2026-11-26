using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class ReportStockAdjustController : Controller
    {
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Inventory"
            };
            //html.BunSearch = "BtnSearchStockSummary1";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                FromDateID = "txtStockDate",
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
            //List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            //RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            //{
            //    OPTLabel = "Item Name",
            //    ISAutoComplete = true,
            //    SQL = "select distinct top 10 i.ItemName from dbo.TblItem i left join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
            //    RadioValue = EncryAndDecry.Encryp("ItemName"),
            //    DefaultCheckBok = true,
            //    //Name = "lbl_Item_Name_StockStatus",
            //    //ID = "TXT_Item_Name_StockStatus",
            //};
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Item Name",
                ISAutoComplete = true,
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("SerialNumber"),
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Serial Number",
                SQL = "select distinct top 10 case when isnull(i.SerialNumber, '') <> '' then i.SerialNumber else srd.SerialNumber end [SerialNumber]\r\nfrom TblItem i \r\ninner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID \r\nleft join dbo.TblItemSerialDetail srd on srd.ItemID = i.ItemID\r\nwhere isnull(i.SerialNumber, '') <> '' and lower(case when isnull(i.SerialNumber, '') <> '' then i.SerialNumber else srd.SerialNumber end) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            //html.GH.SQL = SQL;
            html.GH.GridProcedureName = "SP_GET_STOCK_ADJUSTMENT_DETAIL";
            html.ShowPaging = false;
            //html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign="center" },
                new GridField() { Header = "ItemCode", FieldName = "ItemCode", TextAlign = "center" },
                new GridField() { Header = "Item Name", FieldName = "ItemName",TextAlign="left"},
                new GridField() { Header = "Recieved Date", FieldName = "ReceivedDate", TextAlign="left"},
                new GridField() { Header = "Quantity on hand", FieldName = "QuantityOnHand",  ClassName="text-blue text-center"},
                new GridField() { Header = "Quantity Avialable", FieldName = "QuantityAvliable", ClassName="text-blue text-center" },
                new GridField() { Header = "Quantity Adjusted", FieldName = "QuantityAdjusted", ClassName="text-red text-center"},
                new GridField() { Header = "UOM", FieldName = "UM",TextAlign="center"},
                new GridField() { Header = "Reason", FieldName = "Reason" ,TextAlign="center" , Width="500px", ClassName="truncate-cell"},

            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}