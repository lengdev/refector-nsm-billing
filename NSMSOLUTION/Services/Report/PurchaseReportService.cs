using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AllReportAPi.Services
{
    public interface IPurchaseReportService
    {
        IQueryable<POListResponseDto> PurchaseOrderList(ReportFilter filter);
        IQueryable<PODetailResponseDto> PurchaseOrderDetail(ReportFilter filter);
        IQueryable<ExpsenseSummaryDto> ExpsenseSummary(ReportFilter filter);
        IQueryable<ExpenseDetailDto> ExpenseDetail(ReportFilter filter);
        IQueryable<PaymentSummaryDto> PaymentSummary(ReportFilter filter);
        IQueryable<PaymentDetailDto> PaymentDetail(ReportFilter filter);
        void FillExpenseMemo(IList<ExpsenseSummaryDto> rows);
        void FillExpenseMemo(IList<ExpenseDetailDto> rows);
    }

    public class PurchaseReportService : IPurchaseReportService
    {
        private readonly DBContext db;
        private readonly ReportScope scope;

        public PurchaseReportService(DBContext db, ReportScope scope)
        {
            this.db = db;
            this.scope = scope;
        }

        private static string[] SelectedAccounts(ReportFilter filter)
        {
            return (filter.Account ?? "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(account => account.Trim())
                .Where(account => account.Length > 0)
                .ToArray();
        }

        public IQueryable<POListResponseDto> PurchaseOrderList(ReportFilter filter)
        {
            var orders = db.TblPurchaseOrder.AsNoTracking().WhereScope(scope);

            var source =
                from po in orders
                join s in db.TblReportStatuses.AsNoTracking()
                    on po.StatusID equals (int?)s.StatusID
                where po.Type == "PurchaseOrder"
                select new { PurchaseOrder = po, s.Status };

            return source
                .WhereContains(r => r.PurchaseOrder.VendorName, filter.Supplier)
                .WhereContains(r => r.PurchaseOrder.PONumber, filter.PONumber)
                .WherePeriod(r => r.PurchaseOrder.PODate, filter)
                .WhereNonZero(r => r.PurchaseOrder.GrandTotal, filter)
                .OrderBy(r => r.PurchaseOrder.PODate)
                .ThenBy(r => r.PurchaseOrder.PONumber)
                .Select(r => new POListResponseDto
                {
                    Date = r.PurchaseOrder.PODate,
                    PoNo = r.PurchaseOrder.PONumber,
                    Vendor = r.PurchaseOrder.VendorName,
                    Status = r.Status,
                    Amount = r.PurchaseOrder.GrandTotal ?? 0m,
                });
        }

        public IQueryable<PODetailResponseDto> PurchaseOrderDetail(ReportFilter filter)
        {
            var orders = db.TblPurchaseOrder.AsNoTracking().WhereScope(scope);

            var source =
                from purchaseOrder in orders
                join purchaseOrderDetail in db.TblPurchaseOrderDetail.AsNoTracking()
                    on purchaseOrder.PurchaseOrderID equals purchaseOrderDetail.PurchaseOrderID
                select new { PurchaseOrder = purchaseOrder, PurchaseOrderDetail = purchaseOrderDetail };

            return source
                .WhereContains(row => row.PurchaseOrder.VendorName, filter.Supplier)
                .WhereContains(row => row.PurchaseOrder.WarehouseName, filter.Warehouse)
                .WherePeriod(row => row.PurchaseOrder.PODate, filter)
                .WhereNonZero(row => row.PurchaseOrder.GrandTotal, filter)
                .OrderBy(row => row.PurchaseOrder.PODate)
                .ThenBy(row => row.PurchaseOrder.PONumber)
                .ThenBy(row => row.PurchaseOrderDetail.PurchaseOrderDetailID)
                .Select(row => new PODetailResponseDto
                {
                    Date = row.PurchaseOrder.PODate,
                    PoNo = row.PurchaseOrder.PONumber,
                    ItemName = row.PurchaseOrderDetail.ItemName,
                    Description = row.PurchaseOrderDetail.ItemDescription,
                    Qty = row.PurchaseOrderDetail.QTY,
                    Cost = row.PurchaseOrderDetail.UnitPrice,
                    Amount = row.PurchaseOrderDetail.QTY * row.PurchaseOrderDetail.UnitPrice,
                });
        }

        public IQueryable<ExpsenseSummaryDto> ExpsenseSummary(ReportFilter filter)
        {
            var selectedAccounts = SelectedAccounts(filter);
            var source =
                from expense in db.TblExpense.AsNoTracking().WhereScope(scope)
                join vendor in db.TblVendor.AsNoTracking()
                    on expense.PayeeID equals (int?)vendor.VendorID into vendors
                from v in vendors.DefaultIfEmpty()
                join emp in db.TblEmployee.AsNoTracking()
                    on expense.PayeeID equals (int?)emp.LeadID into employees
                from e in employees.DefaultIfEmpty()
                join c in db.TblClass.AsNoTracking()
                    on expense.ClassName equals c.ClassID
                join pm0 in db.TblPaymentMethod.AsNoTracking()
                on expense.PaymentMethodID equals pm0.PaymentMethodID into pms
                from pm in pms.DefaultIfEmpty()

                let payAcc = (from tt in db.TblAccountTransaction
                              where tt.Type == "Expense" && tt.InvoiceNo == expense.ExpenseNo   // confirm link column
                              join je in db.TblAccountJournalEntries on tt.TransactionID equals je.TransactionID
                              where (je.Credit ?? 0m) > 0m
                              join coa in db.TblChartOfAccount on je.AccountID equals coa.ChartOfAccountID
                              orderby je.EntryID
                              select new { coa.AccountNo, coa.AccountName }).FirstOrDefault()

                select new
                {
                    Expense = expense,
                    c.ClassName,
                    Payee = expense.PayeeType == "Vendor" ? v.VendorName : e.FullName,
                    AccountCode = payAcc == null ? "" : payAcc.AccountNo,
                    AccountName = payAcc == null ? "" : payAcc.AccountName,
                    PayMethod = pm == null ? "" : pm.Method,
                };

            if (selectedAccounts.Length > 0)
            {
                source = source.Where(row =>
                    selectedAccounts.Contains(row.AccountCode + "-" + row.AccountName) ||
                    selectedAccounts.Contains(row.AccountCode) ||
                    selectedAccounts.Contains(row.AccountName));
            }

            return source
                .WhereEquals(row => row.ClassName, filter.Class)
                .WhereEquals(row => row.Payee, filter.Payee)
                .WherePeriod(row => row.Expense.PaymentDate, filter)
                .OrderBy(row => row.Expense.PaymentDate)
                .ThenBy(row => row.Expense.ExpenseID)
                .Select(row => new ExpsenseSummaryDto
                {
                    ExpenseDate = row.Expense.PaymentDate,
                    TranType = "Exp",
                    ExpenseID = row.Expense.ExpenseID,
                    PVNumber = row.Expense.PVNo,
                    PaymentNo = row.Expense.PaymentNo,
                    Status = row.Expense.Status,
                    ClassName = row.ClassName,
                    AccountCode = row.AccountCode,
                    AccountName = row.AccountName,
                    PaymentMethod = row.PayMethod,
                    Payee = row.Payee,
                    Memo = row.Expense.Description,
                    Amount = row.Expense.Total ?? 0m,
                });
        }

        public IQueryable<ExpenseDetailDto> ExpenseDetail(ReportFilter filter)
        {
            var selectedAccounts = SelectedAccounts(filter);
            var lines =
                from d in db.TblExpenseDetail
                group d by new { d.ExpenseID, d.ExpenseCategory } into g
                select new
                {
                    g.Key.ExpenseID,
                    g.Key.ExpenseCategory,
                    LineTotal = g.Sum(x => (decimal?)x.ItemGrandTotal) ?? 0m,
                    LineVat = g.Sum(x => (decimal?)x.ItemVAT) ?? 0m,
                };

            var source =
                from expense in db.TblExpense.AsNoTracking().WhereScope(scope)
                join ln in lines on expense.ExpenseID equals ln.ExpenseID into lns
                from l in lns.DefaultIfEmpty()
                join vendor in db.TblVendor.AsNoTracking()
                    on expense.PayeeID equals (int?)vendor.VendorID into vendors
                from v in vendors.DefaultIfEmpty()
                join emp in db.TblEmployee.AsNoTracking()
                    on expense.PayeeID equals (int?)emp.LeadID into employees
                from e in employees.DefaultIfEmpty()
                join c in db.TblClass.AsNoTracking()
                    on expense.ClassName equals c.ClassID
                join coa in db.TblChartOfAccount
                    on l.ExpenseCategory equals coa.ChartOfAccountID.ToString() into coas
                from a in coas.DefaultIfEmpty()
                select new
                {
                    Expense = expense,
                    c.ClassName,
                    Payee = expense.PayeeType == "Vendor" ? v.VendorName : e.FullName,
                    AccountCode = a == null ? "" : a.AccountNo,
                    AccountName = a == null ? "" : a.AccountName,
                    LineTotal = (decimal?)l.LineTotal,
                    LineVat = (decimal?)l.LineVat,
                };

            if (selectedAccounts.Length > 0)
            {
                source = source.Where(row =>
                    selectedAccounts.Contains(row.AccountCode + "-" + row.AccountName) ||
                    selectedAccounts.Contains(row.AccountCode) ||
                    selectedAccounts.Contains(row.AccountName));
            }

            return source
                .WhereEquals(row => row.ClassName, filter.Class)
                .WhereEquals(row => row.Payee, filter.Payee)
                .WherePeriod(row => row.Expense.PaymentDate, filter)
                .OrderBy(row => row.Expense.PaymentDate)
                .ThenBy(row => row.Expense.ExpenseID)
                .ThenBy(row => row.AccountCode)
                .Select(row => new ExpenseDetailDto
                {
                    ExpenseDate = row.Expense.PaymentDate,
                    TranType = "Exp",
                    PVNumber = row.Expense.PVNo,
                    ExpenseID = row.Expense.ExpenseID,
                    PaymentNo = row.Expense.PaymentNo,
                    Status = row.Expense.Status,
                    ClassName = row.ClassName,
                    Payee = row.Payee,
                    Account = row.AccountCode == "" ? "" : row.AccountCode + "-" + row.AccountName,
                    AccountCode = row.AccountCode,
                    AccountName = row.AccountName,
                    Memo = row.Expense.Description,
                    Amount = row.LineTotal == null
                                    ? (row.Expense.Total ?? 0m)
                                    : row.LineTotal.Value + (row.LineVat ?? 0m),
                    Vat = row.LineTotal == null
                                    ? (row.Expense.VAT ?? 0m)
                                    : (row.LineVat ?? 0m),
                });

        }

        public IQueryable<PaymentSummaryDto> PaymentSummary(ReportFilter filter)
        {
            var selectedAccounts = SelectedAccounts(filter);
            var source =
                from expense in db.TblExpense.AsNoTracking().WhereScope(scope)
                join c in db.TblClass.AsNoTracking()
                    on expense.ClassName equals c.ClassID

                let payAcc = (from tt in db.TblAccountTransaction
                              where tt.Type == "Expense" && tt.InvoiceNo == expense.ExpenseNo
                              join je in db.TblAccountJournalEntries on tt.TransactionID equals je.TransactionID
                              where (je.Credit ?? 0m) > 0m
                              join coa in db.TblChartOfAccount on je.AccountID equals coa.ChartOfAccountID
                              orderby je.EntryID
                              select new { coa.AccountNo, coa.AccountName }).FirstOrDefault()

                let lineMemo = db.TblExpenseDetail
                                 .Where(d => d.ExpenseID == expense.ExpenseID
                                          && d.ItemName != null && d.ItemName != "")
                                 .OrderBy(d => d.ExpenseCategory)
                                 .Select(d => d.ItemName)
                                 .FirstOrDefault()

                select new
                {
                    Expense = expense,
                    c.ClassName,
                    Payee = db.TblVendor
                      .Where(v => v.VendorID == expense.PayeeID)
                      .Select(v => v.VendorName)
                      .FirstOrDefault(),
                    AccountCode = payAcc == null ? "" : payAcc.AccountNo,
                    AccountName = payAcc == null ? "" : payAcc.AccountName,
                    Memo = (expense.Description != null && expense.Description != "")
                               ? expense.Description
                               : lineMemo,
                };

            var filtered = source;
            if (selectedAccounts.Length > 0)
            {
                filtered = filtered.Where(row =>
                               selectedAccounts.Contains(row.AccountCode + "-" + row.AccountName) ||
                               selectedAccounts.Contains(row.AccountCode) ||
                               selectedAccounts.Contains(row.AccountName));
            }

            filtered = filtered
                .WhereEquals(row => row.ClassName, filter.Class)
                .WhereEquals(row => row.Payee, filter.Payee)
                .WherePeriod(row => row.Expense.PaymentDate, filter);

            var sort = (filter.SortOption ?? "").Trim().ToLower();

            var ordered = filtered
                .OrderBy(r => r.Expense.PaymentDate)
                .ThenBy(r => r.Expense.ExpenseID);

            if (sort == "number")
                ordered = filtered.OrderBy(r => r.Expense.PVNo).ThenBy(r => r.Expense.ExpenseID);
            else if (sort == "vendor name" || sort == "vendor")
                ordered = filtered.OrderBy(r => r.Payee)
                                  .ThenBy(r => r.Expense.PaymentDate)
                                  .ThenBy(r => r.Expense.ExpenseID);

            return ordered.Select(row => new PaymentSummaryDto
            {
                PaymentDate = row.Expense.PaymentDate,
                TransactionType = "VDP",
                ClassName = row.ClassName,
                Number = row.Expense.PVNo,
                Vendor = row.Payee,
                Currency = "Dollar USD",
                Discount = row.Expense.Discount ?? 0m,
                Memo = row.Memo,
                AccountCode = row.AccountCode,
                AccountName = row.AccountName,
                Amount = row.Expense.Total ?? 0m,
                ExpenseID = row.Expense.ExpenseID,
                PaymentNo = row.Expense.PaymentNo,
                Status = row.Expense.Status,
            });
        }


        public List<PaymentDetailGroupDto> PaymentDetailByVendor(ReportFilter filter)
        {
            return PaymentDetail(filter)
                .ToList()
                .Where(r => r.RowType != "total")
                .GroupBy(r => r.Vendor)
                .Select(g => new PaymentDetailGroupDto
                {
                    Vendor = g.Key,
                    TotalPayAmount = g.Sum(x => x.PayAmount),
                    TotalDiscount = g.Sum(x => x.DiscountAmount),
                    TotalGrandAmount = g.Sum(x => x.GrandAmount),
                    Rows = g.ToList(),
                })
                .OrderBy(g => g.Vendor)
                .ToList();
        }
        public IQueryable<PaymentDetailDto> PaymentDetail(ReportFilter filter)
        {
            var selectedAccounts = SelectedAccounts(filter);
            var source =
                from expense in db.TblExpense.AsNoTracking().WhereScope(scope)
                join c in db.TblClass.AsNoTracking()
                    on expense.ClassName equals c.ClassID
                join pm0 in db.TblPaymentMethod.AsNoTracking()
                    on expense.PaymentMethodID equals pm0.PaymentMethodID into pms
                from pm in pms.DefaultIfEmpty()

                    // purchase date = earliest PO date among the expense lines' RefNo
                let purchaseDate = (from d in db.TblExpenseDetail
                                    where d.ExpenseID == expense.ExpenseID
                                    join po in db.TblPurchaseOrder
                                        on d.RefNo equals po.PONumber
                                    orderby po.PODate
                                    select (DateTime?)po.PODate).FirstOrDefault()

                let payAcc = (from tt in db.TblAccountTransaction
                              where tt.Type == "Expense" && tt.InvoiceNo == expense.ExpenseNo
                              join je in db.TblAccountJournalEntries on tt.TransactionID equals je.TransactionID
                              where (je.Credit ?? 0m) > 0m
                              join coa in db.TblChartOfAccount on je.AccountID equals coa.ChartOfAccountID
                              orderby je.EntryID
                              select new { coa.AccountNo, coa.AccountName }).FirstOrDefault()

                select new
                {
                    Expense = expense,
                    c.ClassName,
                    Payee = db.TblVendor
                        .Where(v => v.VendorID == expense.PayeeID)
                        .Select(v => v.VendorName)
                        .FirstOrDefault(),
                    AccountCode = payAcc == null ? "" : payAcc.AccountNo,
                    AccountName = payAcc == null ? "" : payAcc.AccountName,
                    PayMethod = pm == null ? "" : pm.Method,
                    PurchaseDate = purchaseDate,
                };

            var filtered = source;
            if (selectedAccounts.Length > 0)
            {
                filtered = filtered.Where(row =>
                    selectedAccounts.Contains(row.AccountCode + "-" + row.AccountName) ||
                    selectedAccounts.Contains(row.AccountCode) ||
                    selectedAccounts.Contains(row.AccountName));
            }

            var rows = filtered
                .WhereEquals(row => row.ClassName, filter.Class)
                .WhereEquals(row => row.Payee, filter.Payee)
                .WherePeriod(row => row.Expense.PaymentDate, filter)
                .OrderBy(row => row.Payee)
                .ThenBy(row => row.Expense.PaymentDate)
                .ThenBy(row => row.Expense.ExpenseID)
                .Select(row => new PaymentDetailDto
                {
                    Vendor = row.Payee,
                    TransactionType = "VDP",
                    BillNo = "",
                    PurchaseDate = row.PurchaseDate,
                    PaymentDate = row.Expense.PaymentDate,
                    PaymentNo = row.Expense.PVNo,
                    PayAmount = row.Expense.Total ?? 0m,
                    PaymentMethod = row.PayMethod,
                    Currency = "Dollar USD",
                    DiscountAmount = row.Expense.Discount ?? 0m,
                    GrandAmount = row.Expense.Total ?? 0m,
                    ClassName = row.ClassName,
                    Memo = row.Expense.Description,
                    AccountCode = row.AccountCode,
                    AccountName = row.AccountName,
                    ExpenseID = row.Expense.ExpenseID,
                    Status = row.Expense.Status,
                })
                .ToList();

            // memo detail: item names of every expense line, joined into one text
            var ids = rows.Select(r => r.ExpenseID).Distinct().ToList();

            var details = db.TblExpenseDetail.AsNoTracking()
                .Where(d => ids.Contains(d.ExpenseID))
                .Select(d => new { d.ExpenseID, d.ItemName })
                .ToList()
                .GroupBy(d => d.ExpenseID)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join("; ", g.Select(x => x.ItemName)
                                            .Where(s => !string.IsNullOrWhiteSpace(s))
                                            .Distinct()));

            foreach (var r in rows)
                r.MemoDetail = details.TryGetValue(r.ExpenseID, out var m) ? m : null;

            var groupedRows = new List<PaymentDetailDto>();
            foreach (var vendorRows in rows.GroupBy(r => r.Vendor))
            {
                groupedRows.AddRange(vendorRows);
                var firstRow = vendorRows.First();
                groupedRows.Add(new PaymentDetailDto
                {
                    Vendor = vendorRows.Key,
                    TransactionType = " ",
                    BillNo = " ",
                    PurchaseDate = firstRow.PurchaseDate ?? DateTime.MinValue,
                    PaymentDate = firstRow.PaymentDate ?? DateTime.MinValue,
                    PaymentNo = "Total",
                    AccountCode = " ",
                    AccountName = " ",
                    PaymentMethod = " ",
                    Currency = " ",
                    PayAmount = vendorRows.Sum(r => r.PayAmount),
                    DiscountAmount = vendorRows.Sum(r => r.DiscountAmount),
                    GrandAmount = vendorRows.Sum(r => r.GrandAmount),
                    Memo = " ",
                    MemoDetail = " ",
                    RowType = "total",
                    SortKey = 1,
                    ExcludeFromTotals = true
                });
            }

            return groupedRows.AsQueryable();
        }

        public void FillExpenseMemo(IList<ExpsenseSummaryDto> rows)
        {
            var ids = rows.Select(r => r.ExpenseID).Distinct().ToList();

            var items = db.TblExpenseDetail.AsNoTracking()
                .Where(d => ids.Contains(d.ExpenseID))
                .Select(d => new { d.ExpenseID, d.ItemName })
                .ToList()
                .GroupBy(d => d.ExpenseID)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(", ", g.Select(x => x.ItemName)
                                           .Where(n => !string.IsNullOrWhiteSpace(n))));

            foreach (var r in rows)
            {
                string detail;
                items.TryGetValue(r.ExpenseID, out detail);
                r.Memo = string.Join(", ",
                    new[] { r.Memo, detail }.Where(s => !string.IsNullOrWhiteSpace(s)));
            }
        }

        public void FillExpenseMemo(IList<ExpenseDetailDto> rows)
        {
            var ids = rows.Select(r => r.ExpenseID).Distinct().ToList();

            var items = db.TblExpenseDetail.AsNoTracking()
                .Where(d => ids.Contains(d.ExpenseID))
                .Select(d => new { d.ExpenseID, d.ItemName })
                .ToList()
                .GroupBy(d => d.ExpenseID)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(", ", g.Select(x => x.ItemName)
                                           .Where(n => !string.IsNullOrWhiteSpace(n))));

            foreach (var r in rows)
            {
                string detail;
                items.TryGetValue(r.ExpenseID, out detail);
                r.Memo = string.Join(", ",
                    new[] { r.Memo, detail }.Where(s => !string.IsNullOrWhiteSpace(s)));
            }
        }
    }
}