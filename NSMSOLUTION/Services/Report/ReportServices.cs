using NSMSOLUTION.Models.EFCore;
using System;

namespace AllReportAPi.Services
{
    /// <summary>
    /// One DBContext + one ReportScope shared by every report service.
    /// Dispose it when you are finished reading the query results.
    ///
    /// Inside a report controller:   RunService&lt;T&gt;(f, (s, filter) =&gt; s.Sale.SaleByInvoice(filter))
    /// From any other controller:    using (var s = new ReportServices(scope)) { ... }
    /// </summary>
    public sealed class ReportServices : IDisposable
    {
        private readonly DBContext db;
        private readonly ReportScope scope;

        private IPurchaseReportService purchase;
        private IReceivableReportService receivable;
        private ISaleReportService sale;
        private ICustomerReportService customer;
        private IInventoryReportService inventory;
        private IAccountingReportService accounting;
        private ILookupService lookup;

        public ReportServices(ReportScope scope)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            this.scope = scope;
            this.db = new DBContext();
        }

        public IPurchaseReportService Purchase
        {
            get { return purchase ?? (purchase = new PurchaseReportService(db, scope)); }
        }

        public IReceivableReportService Receivable
        {
            get { return receivable ?? (receivable = new ReceivableReportService(db, scope)); }
        }

        public ISaleReportService Sale
        {
            get { return sale ?? (sale = new SaleReportService(db, scope)); }
        }

        public ICustomerReportService Customer
        {
            get { return customer ?? (customer = new CustomerReportService(db, scope)); }
        }

        public IInventoryReportService Inventory
        {
            get { return inventory ?? (inventory = new InventoryReportService(db, scope)); }
        }

        // static sample data for now: no database needed
        public IAccountingReportService Accounting
        {
            get { return accounting ?? (accounting = new AccountingReportService(db, scope)); }
        }

        public ILookupService Lookup
        {
            get { return lookup ?? (lookup = new LookupService(db, scope)); }
        }


        public void Dispose()
        {
            db.Dispose();
        }
    }
}