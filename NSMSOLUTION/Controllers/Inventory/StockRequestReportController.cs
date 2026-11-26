using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Inventory
{
    public class StockRequestReportController : Controller
    {
        // GET: StockStatus
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Stock Request Report"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                FromDateID = "txtStockRequestReportDate",
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
                ID = "txtRequestNo",
                ISAutoComplete = true,
                Label = "Request No:",
                DBField = "RequestNo",
                SQLAutoComplete = "select distinct top 10 RequestNo from dbo.TblItem_Request where lower(RequestNo) like @SearchText and RequestMode = 'Item Request' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RequestNo desc"
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
                ID = "txtBlock",
                Label = "Block:",
                DBField = "CustomerType",
                SQL_CBO = "select distinct t.CustomerTypeID, t.CustomerType from dbo.TblCustomerType t where t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%" + html.DomainID + "%' order by t.CustomerTypeID",
                DisplayText = "CustomerType",
                Value = "CustomerType",
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
                SQL = "select distinct top 10 i.ItemName from dbo.TblItem i inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID where lower(i.ItemName) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID=" + html.WarehouseID + " order by i.ItemName",
                RadioValue = EncryAndDecry.Encryp("ItemName") + "," + EncryAndDecry.Encryp("SerialNumber"),
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);


            html.GH.GridProcedureName = "SP_SHOW_STOCK_REQUEST";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Request No", FieldName = "RequestNo" },
                new GridField() { Header = "Date", FieldName = "RequestDate"},
                new GridField() { Header = "Request Name", FieldName = "RequestName" },
                new GridField() { Header = "Block", FieldName = "Block",TextAlign="center" },
                new GridField() { Header = "Item Name", FieldName = "ItemName"},
                new GridField() { Header = "Item Code", FieldName = "ItemCode",TextAlign="center", TotalLabel="Total:"},
                new GridField() { Header = "QTY", FieldName = "QTY", ClassName="text-red text-center", Format="n0", IsTotalFooter=true},
                new GridField() { Header = "UOM", FieldName = "Unit", ClassName="text-red text-center"},
                new GridField() { Header = "Need To Approval", FieldName = "NeedToApproval" },
                new GridField() { Header = "Approve", FieldName = "ApproveOrDisapprove", TextAlign = "center" },
                new GridField() { Header = "Purpose or Reason", FieldName = "Remark", LinkField="RequestItemID", DataTableNumber=2, DisplayFields="Total QTY"},
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}