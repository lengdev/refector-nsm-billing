using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Report.AccountingReport
{
    public class CustomerAgingController : Controller
    {

        [Authorize(Roles = "Customer Aging Report")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;


            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DateTimePickerWithRanges = false,
                DBField = "ag.FilterDate"
            });

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode from (select CustomerCode, SystemID, WarehouseID, DomainID from dbo.TblCustomer\r\nunion all\r\nselect CustomerCode, SystemID, WarehouseID, DomainID from dbo.TblCustomerInvoice) as c where lower(c.CustomerCode) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%' order by c.CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CID"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select distinct top 10 c.CustomerName from (select CustomerName, SystemID, WarehouseID, DomainID from dbo.TblCustomer\r\nunion all\r\nselect CustomerName, SystemID, WarehouseID, DomainID from dbo.TblCustomerInvoice) as c where lower(c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%' order by c.CustomerName",
                RadioValue = EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
            });

            //// Connectivity Status filter
            //html.HtmlAddControls.Add(new HtmlAddControls()
            //{
            //    ID = "txtConnStatus",
            //    Label = "Conn. Status:",
            //    DBField = "ag.ConnectivityStatus",
            //    SQL_CBO = "select distinct bi.ConnectivityStatus StatusID, bi.ConnectivityStatusName Status " +
            //               "from dbo.TblCustomerBillingInvoice bi " +
            //               "where bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " " +
            //               "and isnull(bi.ConnectivityStatusName,'') <> '' order by bi.ConnectivityStatusName",
            //    DisplayText = "Status",
            //    Value = "StatusID",
            //    ComboBox = true
            //});

            string sql = $@"SELECT
    ag.[No],
    ag.CID,
    ag.CustomerName,
    ag.Phone,
    ag.ConnStatus,
    ag.[Current],
    ag.Days1_30,
    ag.Days31_60,
    ag.Days61_90,
    ag.Over90Days,
    ag.TotalBalance,
    ag.ConnectivityStatus,
    ag.FilterDate
FROM (
    SELECT
        FORMAT(ROW_NUMBER() OVER(ORDER BY SUM(b.Unpaid) DESC, COALESCE(NULLIF(c.CustomerName,''), NULLIF(c.CustomerNameLatin,'')) ASC), '00') AS [No],
        c.CustomerCode                                                                      AS CID,
        COALESCE(NULLIF(c.CustomerName,''), NULLIF(c.CustomerNameLatin,''))                 AS CustomerName,
        MAX(bi.CompanyContactMobileNumber)                                                  AS Phone,
        MAX(COALESCE(latest.ConnectivityStatusName,''))                                     AS ConnStatus,
        SUM(CASE WHEN b.PaymentDueDate IS NULL OR DATEDIFF(day, b.PaymentDueDate, GETDATE()) <= 0  THEN b.Unpaid ELSE 0 END) AS [Current],
        SUM(CASE WHEN DATEDIFF(day, b.PaymentDueDate, GETDATE()) BETWEEN 1  AND 30 THEN b.Unpaid ELSE 0 END)              AS Days1_30,
        SUM(CASE WHEN DATEDIFF(day, b.PaymentDueDate, GETDATE()) BETWEEN 31 AND 60 THEN b.Unpaid ELSE 0 END)              AS Days31_60,
        SUM(CASE WHEN DATEDIFF(day, b.PaymentDueDate, GETDATE()) BETWEEN 61 AND 90 THEN b.Unpaid ELSE 0 END)              AS Days61_90,
        SUM(CASE WHEN DATEDIFF(day, b.PaymentDueDate, GETDATE()) > 90               THEN b.Unpaid ELSE 0 END)              AS Over90Days,
        SUM(b.Unpaid)                                                                       AS TotalBalance,
        MAX(COALESCE(latest.ConnectivityStatus, 0))                                         AS ConnectivityStatus,
        MAX(b.PaymentDueDate)                                                               AS FilterDate
    FROM dbo.TblCustomerBalance b
    INNER JOIN dbo.TblCustomer c ON c.CustomerID = b.CustomerID
    LEFT JOIN dbo.TblCustomerBillInformation bi ON bi.CustomerID = b.CustomerID
    LEFT JOIN (
        SELECT CustomerID,
               ConnectivityStatusName,
               ConnectivityStatus,
               ROW_NUMBER() OVER (PARTITION BY CustomerID ORDER BY BillingInvoiceID DESC) AS rn
        FROM dbo.TblCustomerBillingInvoice
        WHERE SystemID = {html.SystemID} AND WarehouseID = {html.WarehouseID}
  
    ) latest ON latest.CustomerID = b.CustomerID AND latest.rn = 1
    WHERE b.StatusID IN (11, 15)
      AND b.Unpaid > 0
      AND b.SystemID = {html.SystemID}
      AND b.WarehouseID = {html.WarehouseID}
      AND b.DomainID LIKE '%{html.DomainID}%'
    GROUP BY c.CustomerCode,
             COALESCE(NULLIF(c.CustomerName,''), NULLIF(c.CustomerNameLatin,''))
) ag
WHERE 1=1";

            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "ag.TotalBalance DESC, ag.CustomerName ASC";
            html.ReportTitle = "Customer Aging Report";
            html.ShowPaging = false;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº",               FieldName = "No",           TextAlign = "center"                                                    },
                new GridField() { Header = "CID",              FieldName = "CID",           TextAlign = "center"                                                    },
                new GridField() { Header = "Customer Name",    FieldName = "CustomerName"                                               },
                new GridField() { Header = "Phone",            FieldName = "Phone",          TextAlign = "center"                                                    },
                new GridField() { Header = "Conn. Status",     FieldName = "ConnStatus", Width="300px", ClassName="truncate-cell",    TextAlign = "center"  ,   TotalLabel = "Total:"                                                      },
                new GridField() { Header = "Current",          FieldName = "Current",        Format = "#,#0.00", ClassName = "text-right text-danger", IsTotalFooter = true },
                new GridField() { Header = "1 - 30 Days",      FieldName = "Days1_30",       Format = "#,#0.00", ClassName = "text-right text-primary", IsTotalFooter = true },
                new GridField() { Header = "31 - 60 Days",     FieldName = "Days31_60",      Format = "#,#0.00", ClassName = "text-right text-danger", IsTotalFooter = true },
                new GridField() { Header = "61 - 90 Days",     FieldName = "Days61_90",      Format = "#,#0.00", ClassName = "text-right text-primary", IsTotalFooter = true },
                new GridField() { Header = "> 90 Days",        FieldName = "Over90Days",     Format = "#,#0.0", ClassName = "text-right text-danger", IsTotalFooter = true },
                new GridField() { Header = "Total Balance",    FieldName = "TotalBalance",   Format = "#,#0.0", ClassName = "text-right text-primary", IsTotalFooter = true },
                new GridField() { Header = "ConnectivityStatus", FieldName = "ConnectivityStatus", IsHiddenField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View("~/Views/CustomerAging/index.cshtml");
        }
    }
}