using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class ReportSentToDMCController : Controller
    {
        [Authorize(Roles = "Show Report Sent To DMC")]
        public ActionResult Index()
        {
            return View();
        }
        public string LoadInvoiceSummary()
        {
            HTML html = new HTML();
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoice Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoiceNo",
                Label = "Invoice No:",
                DBField = "InvoiceNo",
                SQLAutoComplete = "select distinct top 10 cb.InvoiceNo from dbo.TblCustomer c inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID inner join dbo.TblLeads l on l.LeadID = cb.EmployeeID inner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.StatusID not in(12,13,14,20) and lower(cb.InvoiceNo) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true,
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
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", TextAlign="center" },
                new GridField() { Header = "Invoice No.", FieldName = "InvoiceNo" },
                new GridField() { Header = "CID", FieldName = "CID", Link = "#!/CustomerProfile?ID=", LinkParameterField = "CustomerID", NewPage = true },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                new GridField() { Header = "UM", FieldName = "UM", ClassName="text-center", TotalLabel = "Total Amount:" },
                new GridField() { Header = "Amount", FieldName = "Amount", ClassName="text-right text-red", Format="#,#0", IsTotalFooter = true },
            };
            html.TblID = "tblInvoiceSummary";
            html.ShowPaging = false;
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                HeaderReportNameExcel = "Report Invoice Summary",
                ServerPaging = true,
                ProcedureName = "SP_NSM_INVOICE_SUMMARY"
            };
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
        }
        public string LoadCustomerInfoList()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DateRangePicker = true,
                DefaultValueDate = false,
                LabelFromDate = "Deactivate Date:",
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                ComboBox = true,
                Label = "Activate Status:",
                DBField = "StatusID",
                SQL_CBO = "select StatusID, Status from dbo.TblStatus where StatusID in (40,41,42,43)",
                DisplayText = "Status",
                Value = "StatusID",
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
                SQL = "select distinct top 10 SID from dbo.TblCustomerInvoice where ClassName = 'NSM' and lower(SID) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by SID",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("SID"),
                ISAutoComplete = true,
            });

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", TextAlign="center" },
                new GridField() { Header = "Customer ID", FieldName = "CustomerCode", Link = "#!/CustomerProfile?ID=", LinkParameterField = "CustomerID", NewPage = true },
                new GridField() { Header = "Register Account IDs", FieldName = "ActivateDate", TextAlign="center" },
                new GridField() { Header = "Service ID/Account ID", FieldName = "SID" },
                new GridField() { Header = "Bill Date (DD-MMM-YYYY)", FieldName = "BillDate", TextAlign="center" },
                new GridField() { Header = "Name", FieldName = "CustomerName" },
                new GridField() { Header = "Customer Type (Firm or Individual)", FieldName = "IndustryType" },
                new GridField() { Header = "Installation Address", FieldName = "InstallAddress" },
                new GridField() { Header = "Location", FieldName = "Location", ClassName="text-center" },
                new GridField() { Header = "Bandwidth", FieldName = "Bandwidth", ClassName="text-right" },
                new GridField() { Header = "PackageName", FieldName = "PackageName" },
                new GridField() { Header = "Connection Type", FieldName = "ConnectionType" },
                new GridField() { Header = "Data Usage (Limited or Unlimited)", FieldName = "DataUsage" },
                new GridField() { Header = "Status (Active or Deactive)", FieldName = "Status", DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string> {
                        { "Actived", "badge-success" },
                        { "Locked", "badge-danger" },
                        { "Terminate", "badge-danger" },
                        { "Inactive", "badge-warning" },
                    }
                },
                new GridField() { Header = "Deactivate Date (DD-MMM-YYYY)", FieldName = "DeactivateDate" },
                new GridField() { Header = "GoogleMap_FullAddress", FieldName = "GoogleMap_FullAddress" },
                new GridField() { Header = "GoogleMap_Latitude", FieldName = "GoogleMap_Latitude" },
                new GridField() { Header = "GoogleMap_Longitude", FieldName = "GoogleMap_Longitude" },
            };
            html.TblID = "tblCustInfoList";
            html.ShowPaging = false;
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                HeaderReportNameExcel = "Report Customer Info List",
                ServerPaging = true,
                ProcedureName = "SP_NSM_CUSTOMER_INFO_LIST"
            };
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
        }
        public string DailyInvoice()
        {
            HTML html = new HTML();
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoice Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoiceNo",
                Label = "Invoice No:",
                DBField = "InvoiceNo",
                SQLAutoComplete = "select distinct top 10 cb.InvoiceNo from dbo.TblCustomer c inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID inner join dbo.TblLeads l on l.LeadID = cb.EmployeeID inner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.StatusID not in(12,13,14,20) and lower(cb.InvoiceNo) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true,
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", FormatDateTime = "dd-MMM-yyyy", TextAlign="center" },
                new GridField() { Header = "Invoice No.", FieldName = "InvoiceNo", OnClickFunction="PrintInvoice(#=CustomerID#,'#=InvoiceNo#','1','10','',false,this);" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link = "#!/CustomerProfile?ID=", LinkParameterField = "CustomerID", NewPage = true },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Status", FieldName = "InvoiceType",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string> {
                        { "Renew", "badge-success" },
                    }
                },
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                new GridField() { Header = "UM", FieldName = "UM", ClassName="text-center", TotalLabel = "Total Amount:" },
                new GridField() { Header = "Amount", FieldName = "Amount", ClassName="text-right text-red", Format="#,#0", IsTotalFooter = true },
            };
            html.TblID = "tblDailyInvoice";
            html.ShowPaging = false;
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                HeaderReportNameExcel = "Daily Invoice",
                ServerPaging = true,
                ProcedureName = "SP_NSM_DAILY_INVOICE"
            };
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
        }
    }
}