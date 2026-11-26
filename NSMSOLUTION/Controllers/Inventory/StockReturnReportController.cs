using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    public class StockReturnReportController : Controller
    {
        // GET: StockStatus
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Stock Return Report",
                BunSearch = "BtnSearchStockReturnReport"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                FromDateID = "txtStockReturnReportDate",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtDepartment",
                Label = "Department Name:",
                DBField = "Department",
                SQL_CBO = "select distinct d.DepartmentName Department from dbo.TblDepartment d where d.SystemID = " + html.SystemID + " and d.WarehouseID = " + html.WarehouseID + " and d.DomainID like '%" + html.DomainID + "%' order by d.DepartmentName",
                DisplayText = "Department",
                Value = "Department",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSituation",
                Label = "Situation:",
                DBField = "Situation",
                SQL_CBO = "select distinct Situation from dbo.TblItem_Request_Detail where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by Situation",
                DisplayText = "Situation",
                Value = "Situation",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtRequestNo",
                ISAutoComplete = true,
                Label = "Request Return No:",
                DBField = "RequestReturnNo",
                SQLAutoComplete = "select distinct top 10 RequestReturnNo from dbo.TblItem_Request where lower(RequestReturnNo) like @SearchText and RequestMode in ('Request Stock Return ','Stock Return') and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RequestReturnNo desc"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtRequestName",
                ISAutoComplete = true,
                Label = "Request Name:",
                DBField = "RequestName",
                SQLAutoComplete = "select distinct top 10 RequestName from dbo.TblItem_Request where lower(RequestName) like @SearchText and RequestMode = 'Item Request' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RequestName"
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
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("SerialNumber"),
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.GH.GridProcedureName = "SP_SHOW_STOCK_RETURN";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Request Return No", FieldName = "RequestReturnNo", TextAlign="center"},
                new GridField() {Header = "Date", FieldName = "RequestDate"},
                new GridField() {Header = "Request Name", FieldName = "RequestName"},
                new GridField() {Header = "Department", FieldName = "Department"},
                new GridField() { Header = "Item Name", FieldName = "ItemName"},
                new GridField() { Header = "Item Code", FieldName = "ItemCode", TextAlign="center" },
                new GridField() { Header = "Serial Number", FieldName = "SerialNumber", TextAlign="center", TotalLabel = "Total:"},
                new GridField() { Header = "QTY", FieldName = "QTY", ClassName="text-red text-center", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "UOM", FieldName = "Unit" , TextAlign="center" },
                new GridField() { Header = "Situation", FieldName = "Situation", TextAlign = "center" },
                new GridField() { Header = "Purpose or Reason", FieldName = "Remark", LinkField="RequestItemID", DataTableNumber=2, DisplayFields="Total QTY"},
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}