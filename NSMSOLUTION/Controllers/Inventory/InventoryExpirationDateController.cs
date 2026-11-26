using MVC;
using NSMSOLUTION.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    public class InventoryExpirationDateController : Controller
    {
        // GET: Stock expire
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Inventory Expiration Date"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date Expire:",
                FromDateID = "txtStockStatusDate",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txt_Item_Group_StockStatus",
                Label = "Item Group:",
                DBField = "ItemGroup",
                SQL_CBO = "select distinct ig.ItemGroup from dbo.TblStockExpireDate s inner join dbo.TblItem i on i.ItemID = s.ItemID inner join dbo.TblItemGroup ig on ig.ItemGroupID = i.ItemGroupID where i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID,
                DisplayText = "ItemGroup",
                Value = "ItemGroup",
                ComboBox = true
            });

            // add filter
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtAlertExpiryStatus",
                Label = "Alert Expiry Status:",
                DBField = "AlertExpiryStatus",
                SQL_CBO = "select 'Normal' AlertExpiryStatus union all select 'Nearly Expired' AlertExpiryStatus union all select 'Expired' AlertExpiryStatus",
                DisplayText = "AlertExpiryStatus",
                Value = "AlertExpiryStatus",
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
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i inner join dbo.TblStockExpireDate sd on sd.ItemID = i.ItemID where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("SerialNumber"),
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {

                OPTLabel = "Barcode",
                SQL = "select distinct top 10 s.Barcode from TblItem i inner join dbo.TblStockExpireDate s on s.ItemID = i.ItemID where isnull(s.Barcode, '') <> '' and lower(s.Barcode) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by s.Barcode",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("Barcode") + "," + EncryAndDecry.Encryp("SerialNumber"),
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.GH.GridProcedureName = "SP_SHOW_ITEM_EXPIRATION_DATE";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "ItemGroup", FieldName = "ItemGroup", GroupLabel="Item Group: ", IsGroupByField = true, Colspan = 10 },
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true },
                new GridField() { Header = "Item Code", FieldName = "ItemCode", ClassName="text-center" },
                new GridField() { Header = "Barcode", FieldName = "Barcode", TextAlign="center" },
                new GridField() { Header = "Item Size", FieldName = "ItemSize", ClassName="text-center",  },
                new GridField() { Header = "Expiry Date", FieldName = "ItemExpirationDate", ClassName="text-center text-red", TotalLabel="Total:"},
                new GridField() { Header = "Stock On Hand", FieldName = "StockOnHand", ClassName="text-right", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "UOM", FieldName = "UM", ClassName="text-center" },
                new GridField() { Header = "Warehouse", FieldName = "WarehouseName", ClassName="text-center" },
                new GridField() { Header = "Alert Expiry Status", FieldName = "AlertStatus", ClassName="text-center" },
                new GridField() { Header = "Days Until Expiry", FieldName = "DaysUntilExpiry", ClassName="text-center" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize]
        public JsonResult getStockExpireDate(int ItemID, int LocationID, string SearchTerm)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ItemID", ItemID.ToString() },
                { "LocationID", LocationID.ToString() },
                { "SearchTerm", "%" + SearchTerm.ToLower() + "%" }
            };
            string sql = @"select format(ExpireDate, 'dd-MM-yyyy') StockExpireDate, sum(isnull(QTYIN, 0) - isnull(QTYOUT, 0)) as QOH 
from dbo.TblStockExpireDate 
where ItemID = @ItemID and LocationID = @LocationID
and (@SearchTerm = '%%' or format(ExpireDate, 'dd-MM-yyyy') like @SearchTerm)
group by ExpireDate 
--having sum(isnull(QTYIN, 0) - isnull(QTYOUT, 0)) > 0";
            return Json(new { data = html.JsonData(sql, false, para) }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult StockExpireOut(int TranID, string TranType)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>();
            StockExpireDate stockExpireDates = new StockExpireDate();
            SqlResult result = new SqlResult();
            para.Add("TranID", TranID.ToString());
            para.Add("TranType", TranType);
            string sql = "SP_STOCK_EXPIRE_DATE_IN_OUT";
            result = html.SaveChangeData(sql, true, para);
            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}