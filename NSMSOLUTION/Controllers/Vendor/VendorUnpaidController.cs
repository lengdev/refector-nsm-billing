using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Vendor
{
    [Authorize]
    public class VendorUnpaidController : Controller
    {
        // GET: VendorUnpaid
        public ActionResult Unpaid()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Vendor Unpaid",
                TblID = "tblVendorBills"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Bill Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtVendorCode",
                ISAutoComplete = true,
                Label = "Vendor Code:",
                DBField = "VendorCode",
                SQLAutoComplete = "select distinct top 10 v.VendorCode from dbo.TblBills po inner join dbo.TblVendor v on v.VendorID = po.VendorID where lower(v.VendorCode) like @SearchText and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " order by v.VendorCode"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtVendorName",
                ISAutoComplete = true,
                Label = "Vendor Name:",
                DBField = "VendorName",
                SQLAutoComplete = "select distinct top 10 v.VendorName from dbo.TblBills po inner join dbo.TblVendor v on v.VendorID = po.VendorID where lower(v.VendorName) like @SearchText and v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " order by v.VendorName"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBillNumber",
                ISAutoComplete = true,
                Label = "Bill Number:",
                DBField = "BillNumber",
                SQLAutoComplete = "select distinct top 10 BillNumber from dbo.TblBills where lower(BillNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID,
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Purchase Number",
                ISAutoComplete = true,
                SQL = "select distinct top 10 PONumber from dbo.TblBills where lower(PONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("PONumber"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            //string sql = @"select ROW_NUMBER() Over(order by b.BillID desc) as [No], 
            //b.BillID, b.VendorID, (v.VendorCode + ' - ' + v.VendorName) VendorName,
            //b.BillDate, b.PaymentDueDate, b.BillNumber, format(b.GrandTotal, '#,#.00') BillAmount, 
            //case when isnull(b.AmountDue, 0) = 0 then b.GrandTotal else b.AmountDue end AmountDue,
            //s.[Status]
            //from dbo.TblBills b
            //inner join dbo.TblVendor v on v.VendorID = b.VendorID
            //inner join dbo.TblStatus s on s.StatusID = b.StatusID";
            //html.GH.SQL = sql;
            //html.OrderBy = "(v.VendorCode + ' - ' + v.VendorName), b.BillID desc";
            html.GridEventClick = "onGridBillClick(this);";
            html.DrawTotalAllHeader = true;

            html.DrawTotalAllHeaderDataTableNumber = 2;
            html.GH.GridProcedureName = "SP_SHOW_VENDOR_UNPAID";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "BillID", FieldName = "BillID", IsHiddenField= true },
                new GridField() { Header = "VendorID", FieldName = "VendorID", IsHiddenField= true },
                //new GridField() { Header = "Vendor Code", FieldName = "VendorCode", IsGroupByField = true },
                new GridField() { Header = "Vendor Name", FieldName = "VendorName", IsGroupByField = true, Colspan = 9 },
                new GridField() { Header = "Payments Made", FieldName = "PaymentsMade", IsGroupByField = true, Colspan = 9 },
                new GridField() { Header = "Bill Date", FieldName = "BillDate", Format = "dd-MM-yy" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", Format = "dd-MM-yy" },
                new GridField() { Header = "Bill#", FieldName = "BillNumber", },
                new GridField() { Header = "PO#", FieldName = "PONumber", },
                new GridField() { Header = "Amount", FieldName = "BillAmount", TextAlign = "right", Format="#,#0.00" },
                new GridField() { Header = "Amount Paid", FieldName = "AmountPaid", TextAlign = "right", Format="#,#0.00", ClassName="text-primary" },
                new GridField() { Header = "Amount Due", FieldName = "AmountDue", TextAlign = "right", Format="#,#0.00", ClassName="text-danger" },
                new GridField() { Header = "Status", FieldName = "Status", LinkField="VendorID", TextAlign = "center", DataTableNumber = 3, DisplayFields="Total Amount, Amount Paid, Amount Due", Colspan = 9 }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

    }
}