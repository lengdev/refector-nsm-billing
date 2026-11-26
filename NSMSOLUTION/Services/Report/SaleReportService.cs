using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Data.Entity;
using System.Linq;

namespace AllReportAPi.Services
{
    public interface ISaleReportService
    {
        IQueryable<QuotationListDto> QuotationList(ReportFilter filter);
        IQueryable<PiListDto> PiList(ReportFilter filter);
        IQueryable<QuotationDetailDto> QuotationDetail(ReportFilter filter);

        IQueryable<SaleByCustomerSummaryDto> SaleByCustomerSummary(ReportFilter filter);
        IQueryable<SaleInvoiceDto> SaleByCustomerDetail(ReportFilter filter);
        IQueryable<SaleInvoiceDto> SaleByInvoice(ReportFilter filter);
        IQueryable<SaleInvoiceDto> SaleBySalemanDetail(ReportFilter filter);

        IQueryable<SaleByBlockSummaryDto> SaleByBlockSummary(ReportFilter filter);
        IQueryable<SaleBySalemanSummaryDto> SaleBySalemanSummary(ReportFilter filter);
        IQueryable<SaleByMonthDto> SaleSummaryByMonth(ReportFilter filter);

        IQueryable<SaleItemLineDto> SaleByItemDetail(ReportFilter filter);
        IQueryable<SaleByItemSummaryDto> SaleByItemSummary(ReportFilter filter);

        IQueryable<SaleDeliveryDto> SaleDeliverySummary(ReportFilter filter);
        IQueryable<SaleDeliveryDto> SaleDeliveryDetail(ReportFilter filter);
        IQueryable<SaleDeliveryDto> OpenSaleDelivery(ReportFilter filter);

        IQueryable<SaleOrderDto> SaleOrderList(ReportFilter filter);
        IQueryable<SaleOrderDto> SaleOrderDetail(ReportFilter filter);
        IQueryable<OpenOrderDto> OpenSaleOrderByCustomer(ReportFilter filter);
        IQueryable<OpenOrderDto> OpenSaleOrderByItem(ReportFilter filter);
    }

    public class SaleReportService : ISaleReportService
    {
        private const string QuotationType = "Quotation Invoice";
        private const string PiType = "Sale Order Invoice";
        private const string OrderType = "Sale Order Invoice";
        private const string SaleType = "Official Invoice";
        private const string DeliveryType = "Delivery Invoice";
        private const int PaidStatus = 10;
        private const int NSMIspSystemID = 13;

        private readonly DBContext db;
        private readonly ReportScope scope;

        public SaleReportService(DBContext db, ReportScope scope)
        {
            this.db = db;
            this.scope = scope;
        }

        // ───────────── shared row ─────────────
        private class InvoiceRow
        {
            public int CustomerInvoiceID { get; set; }
            public int CustomerID { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public string Block { get; set; }
            public string Salesman { get; set; }
            public DateTime? Date { get; set; }
            public string InvoiceNo { get; set; }
            public string Description { get; set; }
            public decimal Qty { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Amount { get; set; }
            public bool IsPaid { get; set; }
        }

        // Latest version of every invoice of one Type, joined to customer, block and salesman.
        private static IQueryable<InvoiceRow> InvoiceRows(
            IQueryable<CustomerInvoice> invoice,
            IQueryable<Customer> customers,
            IQueryable<CustomerBlock> blocks,
            IQueryable<Lead> employees,
            ReportFilter filter, string type, bool checkType)
        {
            var candidates = invoice.Where(x => x.InvoiceNo != null);
            if (checkType)
                candidates = candidates.Where(x => x.Type == type);

            var latestIds = candidates
                .GroupBy(x => x.InvoiceNo)
                .Select(g => g.Max(x => x.CustomerInvoiceID));

            var rows =
                from i in invoice
                where latestIds.Contains(i.CustomerInvoiceID)
                join c in customers on i.CustomerID equals (int?)c.CustomerID
                join b in blocks on c.CustomerTypeID equals (int?)b.CustomerTypeID into bl
                from b in bl.DefaultIfEmpty()
                join e in employees on i.EmployeeID equals (int?)e.LeadID into em
                from e in em.DefaultIfEmpty()
                select new InvoiceRow
                {
                    CustomerInvoiceID = i.CustomerInvoiceID,
                    CustomerID = c.CustomerID,
                    CustomerCode = c.CustomerCode,
                    CustomerName = string.IsNullOrEmpty(c.CustomerName)
                        ? c.CustomerNameLatin : c.CustomerName,
                    Block = b.CustomerType,
                    Salesman = e.FullName ?? "(No salesman)",
                    Date = i.InvoiceDate,
                    InvoiceNo = i.InvoiceNo,
                    Description = i.ItemDescription,
                    Qty = (decimal?)i.QTY ?? 0m,
                    UnitPrice = (decimal?)i.UnitPrice ?? 0m,
                    Amount = (decimal?)i.Amount ?? 0m,
                    IsPaid = i.StatusID == PaidStatus
                };

            return rows
                .WhereEquals(r => r.CustomerID, filter.CustomerID)
                .WherePeriod(r => r.Date, filter);
        }

        private IQueryable<InvoiceRow> Rows(ReportFilter filter, string type)
        {
            var checkType = scope.SystemId == NSMIspSystemID;

            return InvoiceRows(
                db.TblCustomerInvoice.AsNoTracking().WhereScope(scope),
                db.TblCustomer.AsNoTracking(),
                db.TblCustomerBlock.AsNoTracking(),
                db.TblEmployee.AsNoTracking(),
                filter, type, checkType);
        }

        // ───────────── Quotation ─────────────
        public IQueryable<QuotationListDto> QuotationList(ReportFilter filter) =>
            Rows(filter, QuotationType)
                .OrderBy(r => r.Date).ThenBy(r => r.InvoiceNo)
                .Select(r => new QuotationListDto
                {
                    Date = r.Date,
                    QuotationNo = r.InvoiceNo,
                    Customer = r.CustomerName,
                    Salesman = r.Salesman,
                    Amount = r.Amount
                });

        public IQueryable<PiListDto> PiList(ReportFilter filter) =>
            Rows(filter, PiType)
                .OrderBy(r => r.Date).ThenBy(r => r.InvoiceNo)
                .Select(r => new PiListDto
                {
                    CustomerID = r.CustomerID,
                    Date = r.Date,
                    PiNo = r.InvoiceNo,
                    CustomerName = r.CustomerName,
                    Salesman = r.Salesman,
                    Status = r.IsPaid ? "Paid" : "Unpaid",
                    Amount = r.Amount
                });

        public IQueryable<QuotationDetailDto> QuotationDetail(ReportFilter filter) =>
            Rows(filter, QuotationType)
                .OrderBy(r => r.Date).ThenBy(r => r.InvoiceNo)
                .Select(r => new QuotationDetailDto
                {
                    CustomerID = r.CustomerID,
                    Date = r.Date,
                    QuotationNo = r.InvoiceNo,
                    CustomerName = r.CustomerName,
                    Description = r.Description,
                    Qty = r.Qty,
                    UnitPrice = r.UnitPrice,
                    Amount = r.Amount
                });

        // ───────────── Sale: by customer ─────────────
        public IQueryable<SaleByCustomerSummaryDto> SaleByCustomerSummary(ReportFilter filter) =>
            Rows(filter, SaleType)
                .GroupBy(r => new { r.CustomerID, r.CustomerCode, r.CustomerName })
                .Select(g => new SaleByCustomerSummaryDto
                {
                    CustomerID = g.Key.CustomerID,
                    CustomerCode = g.Key.CustomerCode,
                    CustomerName = g.Key.CustomerName,
                    Invoices = g.Count(),
                    Amount = g.Sum(x => x.Amount),
                    Paid = g.Sum(x => x.IsPaid ? x.Amount : 0m),
                    Balance = g.Sum(x => x.IsPaid ? 0m : x.Amount)
                })
                .OrderBy(x => x.CustomerName);

        // one row per invoice, ordered as asked
        private IQueryable<SaleInvoiceDto> SaleInvoices(ReportFilter filter, string orderBy)
        {
            var rows = Rows(filter, SaleType);

            var ordered = orderBy == "customer" ? rows.OrderBy(r => r.CustomerName).ThenBy(r => r.Date)
                        : orderBy == "salesman" ? rows.OrderBy(r => r.Salesman).ThenBy(r => r.Date)
                        : rows.OrderBy(r => r.Date).ThenBy(r => r.InvoiceNo);

            return ordered.Select(r => new SaleInvoiceDto
            {
                CustomerID = r.CustomerID,
                Date = r.Date,
                InvoiceNo = r.InvoiceNo,
                CustomerName = r.CustomerName,
                Block = r.Block,
                Salesman = r.Salesman,
                Amount = r.Amount,
                Paid = r.IsPaid ? r.Amount : 0m,
                Balance = r.IsPaid ? 0m : r.Amount
            });
        }

        public IQueryable<SaleInvoiceDto> SaleByCustomerDetail(ReportFilter filter) => SaleInvoices(filter, "customer");
        public IQueryable<SaleInvoiceDto> SaleByInvoice(ReportFilter filter) => SaleInvoices(filter, "date");
        public IQueryable<SaleInvoiceDto> SaleBySalemanDetail(ReportFilter filter) => SaleInvoices(filter, "salesman");

        // ───────────── Sale: summaries ─────────────
        public IQueryable<SaleByBlockSummaryDto> SaleByBlockSummary(ReportFilter filter) =>
            Rows(filter, SaleType)
                .GroupBy(r => r.Block ?? "(No block)")
                .Select(g => new SaleByBlockSummaryDto
                {
                    Block = g.Key,
                    Invoices = g.Count(),
                    Customers = g.Select(x => x.CustomerID).Distinct().Count(),
                    Amount = g.Sum(x => x.Amount),
                    Paid = g.Sum(x => x.IsPaid ? x.Amount : 0m),
                    Balance = g.Sum(x => x.IsPaid ? 0m : x.Amount)
                })
                .OrderBy(x => x.Block);

        public IQueryable<SaleBySalemanSummaryDto> SaleBySalemanSummary(ReportFilter filter) =>
            Rows(filter, SaleType)
                .GroupBy(r => r.Salesman)
                .Select(g => new SaleBySalemanSummaryDto
                {
                    Salesman = g.Key,
                    Invoices = g.Count(),
                    Customers = g.Select(x => x.CustomerID).Distinct().Count(),
                    Amount = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.Salesman);

        public IQueryable<SaleByMonthDto> SaleSummaryByMonth(ReportFilter filter)
        {
            // grouped in memory: avoids EF date functions, and a period report is small
            var list = Rows(filter, SaleType)
                .Where(r => r.Date != null)
                .Select(r => new { r.Date, r.Amount })
                .ToList();

            return list
                .GroupBy(r => new { r.Date.Value.Year, r.Date.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new SaleByMonthDto
                {
                    Month = g.Key.Year + "-" + g.Key.Month.ToString("00"),
                    Invoices = g.Count(),
                    Amount = g.Sum(x => x.Amount)
                })
                .AsQueryable();
        }

        // ───────────── Sale: items (from the invoice line text) ─────────────
        private IQueryable<InvoiceRow> ItemRows(ReportFilter filter)
        {
            var rows = Rows(filter, SaleType);
            return string.IsNullOrEmpty(filter.Product)
                ? rows
                : rows.Where(r => r.Description.Contains(filter.Product));
        }

        public IQueryable<SaleItemLineDto> SaleByItemDetail(ReportFilter filter) =>
            ItemRows(filter)
                .OrderBy(r => r.Description).ThenBy(r => r.Date)
                .Select(r => new SaleItemLineDto
                {
                    CustomerID = r.CustomerID,
                    Date = r.Date,
                    InvoiceNo = r.InvoiceNo,
                    CustomerName = r.CustomerName,
                    ItemName = r.Description,
                    Qty = r.Qty,
                    UnitPrice = r.UnitPrice,
                    Amount = r.Amount
                });

        public IQueryable<SaleByItemSummaryDto> SaleByItemSummary(ReportFilter filter) =>
            ItemRows(filter)
                .GroupBy(r => r.Description)
                .Select(g => new SaleByItemSummaryDto
                {
                    ItemName = g.Key,
                    Qty = g.Sum(x => x.Qty),
                    Amount = g.Sum(x => x.Amount),
                    AvgPrice = g.Sum(x => x.Qty) == 0m ? 0m : g.Sum(x => x.Amount) / g.Sum(x => x.Qty)
                })
                .OrderBy(x => x.ItemName);

        // ───────────── Delivery (ASSUMED: invoices of DeliveryType; open = not paid/closed) ─────────────
        private IQueryable<SaleDeliveryDto> Deliveries(ReportFilter filter, bool detail, bool openOnly)
        {
            var rows = Rows(filter, DeliveryType);
            if (openOnly) rows = rows.Where(r => !r.IsPaid);

            return rows.OrderBy(r => r.Date).ThenBy(r => r.InvoiceNo)
                .Select(r => new SaleDeliveryDto
                {
                    CustomerID = r.CustomerID,
                    Date = r.Date,
                    DeliveryNo = r.InvoiceNo,
                    CustomerName = r.CustomerName,
                    ItemName = detail ? r.Description : "",
                    Qty = r.Qty,
                    Status = r.IsPaid ? "Closed" : "Open"
                });
        }

        public IQueryable<SaleDeliveryDto> SaleDeliverySummary(ReportFilter filter) => Deliveries(filter, false, false);
        public IQueryable<SaleDeliveryDto> SaleDeliveryDetail(ReportFilter filter) => Deliveries(filter, true, false);
        public IQueryable<SaleDeliveryDto> OpenSaleDelivery(ReportFilter filter) => Deliveries(filter, true, true);

        // ───────────── Sale orders ─────────────
        private IQueryable<SaleOrderDto> Orders(ReportFilter filter, bool detail) =>
            Rows(filter, OrderType)
                .OrderBy(r => r.Date).ThenBy(r => r.InvoiceNo)
                .Select(r => new SaleOrderDto
                {
                    CustomerID = r.CustomerID,
                    Date = r.Date,
                    OrderNo = r.InvoiceNo,
                    CustomerName = r.CustomerName,
                    Salesman = r.Salesman,
                    Status = r.IsPaid ? "Closed" : "Open",
                    ItemName = detail ? r.Description : "",
                    Qty = detail ? r.Qty : 0m,
                    Amount = r.Amount
                });

        public IQueryable<SaleOrderDto> SaleOrderList(ReportFilter filter) => Orders(filter, false);
        public IQueryable<SaleOrderDto> SaleOrderDetail(ReportFilter filter) => Orders(filter, true);

        // open = order not closed yet; grouped by customer or by item
        private IQueryable<OpenOrderDto> OpenOrders(ReportFilter filter, bool byItem) =>
            Rows(filter, OrderType)
                .Where(r => !r.IsPaid)
                .GroupBy(r => byItem ? r.Description : r.CustomerName)
                .Select(g => new OpenOrderDto
                {
                    Name = g.Key,
                    Orders = g.Select(x => x.InvoiceNo).Distinct().Count(),
                    OrderedQty = g.Sum(x => x.Qty),
                    OpenAmount = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.Name);

        public IQueryable<OpenOrderDto> OpenSaleOrderByCustomer(ReportFilter filter) => OpenOrders(filter, false);
        public IQueryable<OpenOrderDto> OpenSaleOrderByItem(ReportFilter filter) => OpenOrders(filter, true);
    }
}