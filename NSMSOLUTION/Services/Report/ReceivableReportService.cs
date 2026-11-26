
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using NSMSOLUTION.Services;
using System;
using System.Data.Entity;
using System.Linq;

namespace AllReportAPi.Services
{
    public interface IReceivableReportService
    {
        IQueryable<ArAgingDto> ARAgingSummary(ReportFilter filter);
        IQueryable<ArAgingDetailDto> ARAgingDetail(ReportFilter filter);
        IQueryable<ArAgingBySalesmanDto> ARAgingBySaleman(ReportFilter filter);
        IQueryable<ArAgingDto> ARAgingByCustomer(ReportFilter filter);
        IQueryable<CollectionDto> Collection(ReportFilter filter);
        IQueryable<CollectionDto> DailyIncome(ReportFilter filter);
        IQueryable<RevenueEstimate> RevenueEstimate(ReportFilter filter);
    }

    public class ReceivableReportService : IReceivableReportService
    {
        private readonly DBContext db;
        private readonly ReportScope scope;
        private readonly CustomerService customerService;

        public ReceivableReportService(DBContext db, ReportScope scope)
        {
            this.db = db;
            this.scope = scope;
            this.customerService = new CustomerService();
        }

        // =========================================================
        // SHARED
        // =========================================================

        public IQueryable<CustomerBalance> OpenInvoices(DateTime asOf)
        {
            var cutoff = asOf.AddDays(1);

            return db.TblCustomerBalance.AsNoTracking()
                .WhereScope(scope)
                .Where(i => i.InvoiceDate < cutoff
                         && i.InvoiceNo != null
                         && i.Unpaid > 0m
                         && i.StatusID == 11
                         && i.Type == "Official Invoice"
                         && i.CancelDate == null
                         && i.DeleteDate == null);
        }

        private static DateTime AsOf(ReportFilter filter)
        {
            return (filter.To ?? DateTime.Today).Date;
        }

        // =========================================================
        // A/R AGING SUMMARY
        // =========================================================

        public IQueryable<ArAgingDto> ARAgingSummary(ReportFilter filter)
        {
            var asOf = AsOf(filter);

            var rows =
                from i in OpenInvoices(asOf)
                join c in db.TblCustomer.AsNoTracking()
                    on i.CustomerID equals (int?)c.CustomerID
                select new
                {
                    Balance = i.Unpaid ?? 0m,
                    Days = DbFunctions.DiffDays(
                        i.PaymentDueDate ?? i.InvoiceDate,
                        asOf
                    ) ?? 0
                };

            return rows
                .GroupBy(r => 1)
                .Select(g => new ArAgingDto
                {
                    Current = g.Sum(r => r.Days <= 0 ? r.Balance : 0m),
                    Days1To30 = g.Sum(r => r.Days >= 1 && r.Days <= 30 ? r.Balance : 0m),
                    Days31To60 = g.Sum(r => r.Days >= 31 && r.Days <= 60 ? r.Balance : 0m),
                    Days61To90 = g.Sum(r => r.Days >= 61 && r.Days <= 90 ? r.Balance : 0m),
                    Over90 = g.Sum(r => r.Days > 90 ? r.Balance : 0m),
                    Total = g.Sum(r => r.Balance)
                })
                .OrderBy(r => 1);
        }

        // =========================================================
        // A/R AGING DETAIL
        // =========================================================

        public IQueryable<ArAgingDetailDto> ARAgingDetail(ReportFilter filter)
        {
            var asOf = AsOf(filter);

            var rows =
                from i in OpenInvoices(asOf)
                join c in db.TblCustomer.AsNoTracking()
                    on i.CustomerID equals (int?)c.CustomerID
                select new
                {
                    Invoice = i,
                    Customer = c,
                    AgeDays = DbFunctions.DiffDays(
                        i.InvoiceDate,
                        asOf
                    ) ?? 0
                };

            var result = rows
                .WhereEquals(r => r.Customer.CustomerID, filter.CustomerID)
                .WhereEquals(r => r.Invoice.ClassName, filter.Class)
                .ToList();

            return result
                .Select(r => new ArAgingDetailDto
                {
                    CustomerID = r.Customer.CustomerID,

                    CustomerName = CustomerService.GetCustomerNamePrefix(r.Customer.CustomerID),

                    InvoiceNo = r.Invoice.InvoiceNo,
                    Class = r.Invoice.ClassName,
                    InvoiceDate = r.Invoice.InvoiceDate,
                    DueDate = r.Invoice.PaymentDueDate,
                    AgeDays = r.AgeDays,
                    Balance = r.Invoice.Unpaid ?? 0m
                })
                .OrderBy(r => r.CustomerName)
                .ThenBy(r => r.DueDate ?? r.InvoiceDate)
                .ThenBy(r => r.InvoiceNo)
                .AsQueryable();
        }

        // =========================================================
        // A/R AGING BY SALESMAN
        // =========================================================

        public IQueryable<ArAgingBySalesmanDto> ARAgingBySaleman(ReportFilter filter)
        {
            var asOf = AsOf(filter);

            var rows =
                from i in OpenInvoices(asOf)
                join c in db.TblCustomer.AsNoTracking()
                    on i.CustomerID equals (int?)c.CustomerID
                join e in db.TblEmployee.AsNoTracking()
                    on i.EmployeeID equals (int?)e.LeadID into employeeGroup
                from e in employeeGroup.DefaultIfEmpty()
                select new
                {
                    EmployeeID = i.EmployeeID,
                    Name = e != null ? e.FullName : null,
                    Balance = i.Unpaid ?? 0m,
                    Days = DbFunctions.DiffDays(
                        i.PaymentDueDate ?? i.InvoiceDate,
                        asOf
                    ) ?? 0
                };

            return rows
                .GroupBy(r => new
                {
                    r.EmployeeID,
                    r.Name
                })
                .Select(g => new ArAgingBySalesmanDto
                {
                    Saleman = g.Key.Name ?? "(No salesman)",

                    Current = g.Sum(r => r.Days <= 0 ? r.Balance : 0m),
                    Days1To30 = g.Sum(r => r.Days >= 1 && r.Days <= 30 ? r.Balance : 0m),
                    Days31To60 = g.Sum(r => r.Days >= 31 && r.Days <= 60 ? r.Balance : 0m),
                    Days61To90 = g.Sum(r => r.Days >= 61 && r.Days <= 90 ? r.Balance : 0m),
                    Over90 = g.Sum(r => r.Days > 90 ? r.Balance : 0m),

                    Total = g.Sum(r => r.Balance)
                })
                .OrderBy(r => r.Saleman);
        }

        // =========================================================
        // A/R AGING BY CUSTOMER
        // =========================================================

        public IQueryable<ArAgingDto> ARAgingByCustomer(ReportFilter filter)
        {
            var asOf = AsOf(filter);

            var rows =
                from i in OpenInvoices(asOf)
                select new
                {
                    CustomerID = i.CustomerID,
                    Balance = i.Unpaid ?? 0m,
                    Days = DbFunctions.DiffDays(
                        i.PaymentDueDate ?? i.InvoiceDate,
                        asOf
                    ) ?? 0
                };

            // Aggregate in SQL
            var grouped = rows
                .GroupBy(r => r.CustomerID)
                .Select(g => new
                {
                    CustomerID = g.Key,

                    Current = g.Sum(r => r.Days <= 0 ? r.Balance : 0m),
                    Days1To30 = g.Sum(r => r.Days >= 1 && r.Days <= 30 ? r.Balance : 0m),
                    Days31To60 = g.Sum(r => r.Days >= 31 && r.Days <= 60 ? r.Balance : 0m),
                    Days61To90 = g.Sum(r => r.Days >= 61 && r.Days <= 90 ? r.Balance : 0m),
                    Over90 = g.Sum(r => r.Days > 90 ? r.Balance : 0m),
                    Total = g.Sum(r => r.Balance)
                });

            // Join customer after aggregation
            var result =
                from g in grouped
                join c in db.TblCustomer.AsNoTracking()
                    on g.CustomerID equals (int?)c.CustomerID
                select new
                {
                    Customer = c,
                    g.Current,
                    g.Days1To30,
                    g.Days31To60,
                    g.Days61To90,
                    g.Over90,
                    g.Total
                };

            return result
                .ToList()
                .Select(r => new ArAgingDto
                {
                    CustomerID = r.Customer.CustomerID,
                    CustomerCode = r.Customer.CustomerCode,
                    CustomerName = CustomerService.GetCustomerNamePrefix(r.Customer.CustomerID),
                    Current = r.Current,
                    Days1To30 = r.Days1To30,
                    Days31To60 = r.Days31To60,
                    Days61To90 = r.Days61To90,
                    Over90 = r.Over90,
                    Total = r.Total
                })
                .OrderBy(r => r.CustomerName)
                .AsQueryable();
        }

        // =========================================================
        // COLLECTION
        // =========================================================
        public IQueryable<CollectionDto> Collection(ReportFilter filter)
        {
            var paid = db.TblCustomerBalance.AsNoTracking()
                .WhereScope(scope)
                .Where(i => i.PaidDate != null
                         && i.Paid > 0m
                         && i.StatusID == 10
                         && i.CancelDate == null
                         && i.DeleteDate == null);

            var rows =
                from i in paid
                join c in db.TblCustomer.AsNoTracking()
                    on i.CustomerID equals (int?)c.CustomerID
                join b in db.TblCustomerBlock.AsNoTracking()
                    on c.CustomerTypeID equals (int?)b.CustomerTypeID
                join p in db.TblPayment.AsNoTracking()
                    .Where(x => x.StatusID == 10)
                    on new { CustomerID = (int?)i.CustomerID, i.InvoiceNo }
                    equals new { CustomerID = (int?)p.CustomerID, p.InvoiceNo }
                join a in db.TblChartOfAccount.AsNoTracking()
                    on p.ChartOfAccountID equals (int?)a.ChartOfAccountID
                    into accounts
                from a in accounts.DefaultIfEmpty()
                join cd in db.TblCollectionDetail.AsNoTracking().WhereScope(scope)
                    on p.PaymentID equals (int?)cd.PaymentID
                    into detail
                from cd in detail.DefaultIfEmpty()
                join ct in db.TblCollection.AsNoTracking().WhereScope(scope)
                    on cd.CollectionID equals (int?)ct.CollectionID
                    into collection
                from ct in collection.DefaultIfEmpty()
                select new
                {
                    Invoice = i,
                    Payment = p,
                    Customer = c,
                    RVNumber = ct.Number,
                    PaymentDate = db.TblPaymentHistory
                        .Where(h => h.InvoiceNo == p.InvoiceNo)
                        .OrderBy(h => h.PaymentHistoryID)
                        .Select(h => h.PayDate)
                        .FirstOrDefault() ?? p.PaymentDate,
                    b.CustomerType,
                    Memo = p.PaymentDescription,
                    Account = a.AccountName
                };

            var result = rows
                .WhereEquals(r => r.Customer.CustomerID, filter.CustomerID)
                .WhereContains(r => r.CustomerType, filter.Block)
                .WhereEquals(r => r.Invoice.ClassName, filter.Class)
                .WherePeriod(r => r.PaymentDate, filter)
                .OrderBy(r => r.PaymentDate)
                .ThenBy(r => r.Payment.OfficialReceiptNo)
                .ToList();

            return result
                .Select(r => new CollectionDto
                {
                    InvoiceDate = r.Invoice.InvoiceDate,
                    ReceivedDate = r.PaymentDate,
                    InvoiceNo = r.Invoice.InvoiceNo,
                    ReceiptNo = string.IsNullOrEmpty(r.RVNumber)
                        ? r.Payment.OfficialReceiptNo
                        : r.RVNumber,
                    CustomerID = r.Customer.CustomerID,
                    CustomerCode = r.Customer.CustomerCode,
                    Customer = CustomerService.GetCustomerNamePrefix(r.Customer.CustomerID),
                    Block = r.CustomerType,
                    Class = r.Invoice.ClassName,
                    Account = r.Account,
                    Memo = r.Memo,
                    Balance = r.Invoice.Unpaid ?? 0m,
                    Amount = r.Payment.PaymentAmount,
                    ReceivedBy = r.Invoice.PaidBy
                })
                .AsQueryable();
        }

        public IQueryable<CollectionDto> DailyIncome(ReportFilter filter)
        {
            var paymentRows =
                from p in db.TblPayment.AsNoTracking().WhereScope(scope)
                where p.PaymentAmount > 0m && p.StatusID == 10
                join c in db.TblCustomer.AsNoTracking().WhereScope(scope)
                    on p.CustomerID equals (int?)c.CustomerID
                join b in db.TblCustomerBalance.AsNoTracking()
                    on p.InvoiceNo equals b.InvoiceNo
                join cl in db.TblClass.AsNoTracking()
                    on b.ClassNameID equals cl.ClassID
                join a in db.TblChartOfAccount.AsNoTracking().WhereScope(scope)
                    on p.ChartOfAccountID equals (int?)a.ChartOfAccountID
                    into accounts
                from a in accounts.DefaultIfEmpty()
                join cd in db.TblCollectionDetail.AsNoTracking().WhereScope(scope)
                    on p.PaymentID equals (int?)cd.PaymentID
                    into detail
                from cd in detail.DefaultIfEmpty()
                join ct in db.TblCollection.AsNoTracking().WhereScope(scope)
                    on cd.CollectionID equals (int?)ct.CollectionID
                    into collection
                from ct in collection.DefaultIfEmpty()
                select new
                {
                    Payment = p,
                    Customer = c,
                    RVNumber = ct.Number,
                    Balance = b,
                    cl.ClassName,
                    Account = a.AccountName,
                    Memo = p.PaymentDescription,
                    PaymentDate = db.TblPaymentHistory
                        .Where(h => h.InvoiceNo == p.InvoiceNo)
                        .OrderBy(h => h.PaymentHistoryID)
                        .Select(h => h.PayDate)
                        .FirstOrDefault() ?? p.PaymentDate
                };

            var paid = paymentRows
                .WhereEquals(r => r.Customer.CustomerID, filter.CustomerID)
                .WhereEquals(r => r.ClassName, filter.Class)
                .WherePeriod(r => r.PaymentDate, filter)
                .ToList()
                .Select(r => new CollectionDto
                {
                    InvoiceDate = r.Balance.InvoiceDate,
                    ReceivedDate = r.PaymentDate,
                    InvoiceNo = r.Payment.InvoiceNo,
                    ReceiptNo = string.IsNullOrEmpty(r.RVNumber) 
                        ? r.Payment.OfficialReceiptNo
                        : r.RVNumber,
                    CustomerID = r.Customer.CustomerID,
                    CustomerCode = r.Customer.CustomerCode,
                    Customer = CustomerService.GetCustomerNamePrefix(r.Customer.CustomerID),
                    Class = r.ClassName,
                    Account = r.Account,
                    Memo = r.Memo,
                    Balance = r.Balance.Unpaid ?? 0m,
                    Amount = r.Payment.PaymentAmount,
                    ReceivedBy = r.Payment.PaidBy,
                })
                .ToList();

            var depositRows =
                from col in db.TblCollection.AsNoTracking().WhereScope(scope)
                where col.UnappliedAmount > 0m
                join cu in db.TblCustomer.AsNoTracking()
                    on col.CustomerID equals cu.CustomerID
                join cl in db.TblClass.AsNoTracking()
                    on col.ClassID equals cl.ClassID
                join a in db.TblChartOfAccount.AsNoTracking()
                    on col.DepositAccountID equals (int?)a.ChartOfAccountID
                    into accounts
                from a in accounts.DefaultIfEmpty()
                select new
                {
                    Collection = col,
                    cl.ClassName,
                    Customer = cu,
                    Account = a.AccountName,
                    ReceivedDate = col.CollectionDate
                };

            var deposits = depositRows
                .WhereEquals(r => r.Customer.CustomerID, filter.CustomerID)
                .WhereEquals(r => r.ClassName, filter.Class)
                .WherePeriod(r => r.ReceivedDate, filter)
                .ToList()
                .Select(r => new CollectionDto
                {
                    InvoiceDate = null,
                    ReceivedDate = r.ReceivedDate,
                    InvoiceNo = null,
                    ReceiptNo = r.Collection.Number,
                    CustomerID = r.Customer.CustomerID,
                    CustomerCode = r.Customer.CustomerCode,
                    Customer = CustomerService.GetCustomerNamePrefix(r.Customer.CustomerID),
                    Class = r.ClassName,
                    Account = r.Account,
                    Memo = r.Collection.Memo,
                    Balance = 0m,
                    Amount = r.Collection.AmountPaid,
                    ReceivedBy = r.Collection.CreatedBy,
                })
                .ToList();

            return paid
                .Concat(deposits)
                .OrderBy(r => r.ReceiptNo)
                .ThenBy(r => r.ReceivedDate)
                .AsQueryable();
        }

        // =========================================================
        // REVENUE ESTIMATE
        // =========================================================

        public IQueryable<RevenueEstimate> RevenueEstimate(ReportFilter filter)
        {
            var invoice = db.TblCustomerBillingInvoice
                .AsNoTracking()
                .WhereScope(scope);

            // Newest revision of each invoice number
            var latestIds = invoice
                .Where(x => x.Type == "Official Invoice" && x.InvoiceNo != null)
                .GroupBy(x => x.InvoiceNo)
                .Select(g => g.Max(x => x.BillingInvoiceID));

            var current = invoice.Where(x =>
                latestIds.Contains(x.BillingInvoiceID));

            // Latest invoice per customer + package
            var latest = current.Where(i => !current.Any(n =>
                n.CustomerID == i.CustomerID
                && n.PackageDetailID == i.PackageDetailID
                && (n.EndDate > i.EndDate
                    || (n.EndDate == i.EndDate
                        && n.BillingInvoiceID > i.BillingInvoiceID))));

            var rows =
                from i in latest
                join c in db.TblCustomer.AsNoTracking()
                    on i.CustomerID equals (int?)c.CustomerID
                join p in db.TblPackageDetail.AsNoTracking()
                    on i.PackageDetailID equals (int?)p.PackageDetailID
                select new
                {
                    Invoice = i,
                    Customer = c,
                    p.PackageName
                };

            var result = rows
                .WhereEquals(r => r.Customer.CustomerID, filter.CustomerID)
                .WhereEquals(r => r.Invoice.ClassName, filter.Class)
                .WherePeriod(r => r.Invoice.EndDate, filter)
                .OrderBy(r => r.Invoice.StatusID == 10 ? 0 : 1)
                .ThenBy(r => r.Invoice.EndDate)
                .ToList();

            return result
                .Select(r => new RevenueEstimate
                {
                    Date = r.Invoice.StatusID == 10
                        ? r.Invoice.InvoiceDate
                        : null,

                    CustomerID = r.Customer.CustomerID,

                        CustomerName = CustomerService.GetCustomerNamePrefix(r.Customer.CustomerID),

                    Class = r.Invoice.ClassName,
                    Package = r.PackageName,
                    Amount = r.Invoice.Amount,
                    EndDate = r.Invoice.EndDate,
                    Remark = r.Invoice.StatusID == 10 ? "Paid" : ""
                })
                .AsQueryable();
        }
    }
}
