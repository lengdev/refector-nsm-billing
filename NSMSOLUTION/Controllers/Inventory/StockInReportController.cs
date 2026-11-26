using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    public class StockInReportController : Controller
    {
        // GET: StockStatus
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Stock IN Report",
                BunSearch = "BtnSearchStockInReport"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "StockIn Date:",
                FromDateID = "txtStockInReportDate",
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
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                RadioValue = EncryAndDecry.Encryp("ItemName"),
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.GH.GridProcedureName = "SP_SHOW_STOCK_IN_REPORT";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date In", FieldName = "StockInDate"},
                new GridField() { Header = "Good Received No.", FieldName = "StockInNo" },
                new GridField() { Header = "Type", FieldName = "Type", TextAlign="center" },
                new GridField() { Header = "PO Number", FieldName = "PONumber" },
                new GridField() { Header = "Vendor Name", FieldName = "VendorName" },
                new GridField() { Header = "Item Code", FieldName = "ItemCode",TextAlign="center"},
                new GridField() { Header = "Item Name", FieldName = "ItemName", Link="#!/ItemProfile?ID=", LinkParameterField="ItemID", NewPage = true},
                new GridField() { Header = "Item Size", FieldName = "ItemSize",TextAlign="center", TotalLabel="Total:"},
                new GridField() { Header = "QTYIN", FieldName = "QTYIN", ClassName="text-primary text-right", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "UOM", FieldName = "UM", ClassName="text-center"},
                new GridField() { Header = "Expired Date", FieldName = "ExpDate", ClassName="text-center text-danger"},
                new GridField() { Header = "Purpose or Reason", FieldName = "Reason",},
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}