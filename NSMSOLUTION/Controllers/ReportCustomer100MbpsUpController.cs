using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class ReportCustomer100MbpsUpController : Controller
    {
        public ActionResult Index()
        {
            HTML html = new HTML();

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoice Date:",
                //FromDateValue = "2024-01-01",
            });

            // Add search controls
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoiceNo",
                Label = "Invoice No:",
                DBField = "InvoiceNo",
                SQLAutoComplete = "select distinct top 10 InvoiceNo from dbo.TblCustomerInvoice where lower(InvoiceNo) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ClassName",
                SQL_CBO = "select distinct c.ClassName from dbo.TblClass c inner join dbo.TblCustomerBalance cb on cb.ClassNameID = c.ClassID where cb.StatusID not in(12,13,14,20) and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%' order by c.ClassName",
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] \r\nfrom dbo.TblCustomerBillingInvoice cb\r\ninner join dbo.TblStatus s on s.StatusID = cb.ConnectivityStatus\r\nwhere cb.StatusID not in (12,13,14,20) and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID,
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true,
                Multiple = true,
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("SID"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "SID",
                SQL = "select distinct top 10 SID from dbo.TblCustomerInvoice where lower(SID) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by SID",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("SID"),
                ISAutoComplete = true,
            });

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign="center" },
                new GridField() { Header = "Subscriber Name", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true, TextAlign="left" },
                new GridField() { Header = "Subscription Date", FieldName = "SubscriptionDate", TextAlign="center"},
                new GridField() { Header = "Type Of Service", FieldName = "ConnectionType", TextAlign="left" },
                new GridField() { Header = "Speed (Mbps)", FieldName = "Bandwidth", ClassName="text-center text-danger"},
                new GridField() { Header = "Municipal/Province", FieldName = "Location", TextAlign="center"},
                new GridField() { Header = "Address", FieldName = "InstallAddress" },
                new GridField() { Header = "Phone", FieldName = "CustomerMobile", TextAlign="left"},
            };

            html.ShowPaging = false;

            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                //ShowHeaderInfoExcel = true,
                HeaderReportNameExcel = "High Bandwidth Customers Report (>=100 Mbps)",
                ShowPrintDate = true,
                ServerPaging = true,
                ProcedureName = "SP_CUSHIGH_BANDWIDTH"
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());

            return View();
        }
    }
}