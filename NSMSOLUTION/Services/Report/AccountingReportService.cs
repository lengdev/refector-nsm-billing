using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AllReportAPi.Services
{
    public interface IAccountingReportService
    {
        IQueryable<LedgerDto> GeneralLedger(ReportFilter filter);
        IQueryable<LedgerDto> JournalReport(ReportFilter filter);
        IQueryable<LedgerDto> TransactionDetailByAccount(ReportFilter filter);
        TransactionLogDto TransactionLog(int transactionId);
        IQueryable<BalanceSheetDto> BalanceSheet(ReportFilter filter);

        IQueryable<TrialBalanceDto> TrialBalance(ReportFilter filter);

        IQueryable<WorkingTrialBalanceDto> AdjustedTrialBalance(ReportFilter filter);
        IQueryable<WorkingTrialBalanceDto> WorkingTrialBalance(ReportFilter filter);

        IQueryable<ProfitLossDto> ProfitLossStandard(ReportFilter filter);
        IQueryable<LedgerDto> ProfitLossDetail(ReportFilter filter);
        IQueryable<ProfitLossDto> ProfitLossYearToDate(ReportFilter filter);
        IQueryable<ProfitLossByMonthDto> ProfitLossByMonth(ReportFilter filter);
        IQueryable<ProfitLossByYearDto> ProfitLossByYear(ReportFilter filter);
        IQueryable<ProfitLossByClassCustomerDto> ProfitLossByClass(ReportFilter filter);
        IQueryable<ProfitLossByClassCustomerDto> ProfitLossByCustomer(ReportFilter filter);
    }
    public class AccountingReportService : IAccountingReportService
    {

        private readonly DBContext db;
        private readonly ReportScope scope;

        public AccountingReportService(DBContext db, ReportScope scope)
        {
            this.db = db;
            this.scope = scope;

        }

        private IQueryable<TrialBalanceDto> TrialBalance(ReportFilter filter, bool includeAdjusting)
        {
            var selectedAccounts = (filter.Account ?? "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(account => account.Trim())
                .Where(account => account.Length > 0)
                .ToArray();

            var accounts = db.TblChartOfAccount.AsNoTracking()
                .WhereScope(scope)
                .Select(account => new TrialBalanceAccountNode
                {
                    AccountID = account.ChartOfAccountID,
                    ParentAccountID = account.ParentAccountID,
                    AccountCode = account.AccountNo,
                    AccountName = account.AccountName
                })
                .ToList();
            var accountsById = accounts.ToDictionary(account => account.AccountID);
            var roots = new List<TrialBalanceAccountNode>();

            foreach (var account in accounts)
            {
                TrialBalanceAccountNode parent;
                if (account.ParentAccountID.HasValue &&
                    accountsById.TryGetValue(account.ParentAccountID.Value, out parent))
                {
                    parent.Children.Add(account);
                }
                else
                {
                    account.ParentAccountID = null;
                    roots.Add(account);
                }
            }

            foreach (var account in accounts)
                account.Children = account.Children
                    .OrderBy(child => child.AccountCode)
                    .ToList();

            if (selectedAccounts.Length > 0)
            {
                var selectedIds = new HashSet<int>(accounts
                    .Where(account => MatchesAccount(
                        selectedAccounts,
                        account.AccountCode,
                        account.AccountName))
                    .Select(account => account.AccountID));
                if (selectedIds.Count == 0)
                    return Enumerable.Empty<TrialBalanceDto>().AsQueryable();

                var selectedRootIds = new HashSet<int>();
                foreach (var selectedId in selectedIds)
                {
                    var account = accountsById[selectedId];
                    var hasSelectedAncestor = false;
                    var parentId = account.ParentAccountID;
                    while (parentId.HasValue)
                    {
                        if (selectedIds.Contains(parentId.Value))
                        {
                            hasSelectedAncestor = true;
                            break;
                        }

                        parentId = accountsById[parentId.Value].ParentAccountID;
                    }

                    if (!hasSelectedAncestor)
                        selectedRootIds.Add(selectedId);
                }

                roots = roots
                    .Where(account => selectedRootIds.Contains(account.AccountID))
                    .ToList();
                if (roots.Count == 0)
                {
                    roots = accounts
                        .Where(account => selectedRootIds.Contains(account.AccountID))
                        .ToList();
                }
            }

            var includedAccountIds = new HashSet<int>();
            Action<TrialBalanceAccountNode> includeSubtree = null;
            includeSubtree = account =>
            {
                if (!includedAccountIds.Add(account.AccountID))
                    return;

                foreach (var child in account.Children)
                    includeSubtree(child);
            };
            foreach (var root in roots)
                includeSubtree(root);

            var entries = AsOf(
                    ByBranch(Journal(
                        includeCategoryAccountType: true,
                        includeUnbalancedTransactions: true,
                        includeAccountHierarchy: true), filter),
                    filter)
                .Where(l => includeAdjusting || l.JournalType != "Adjusting")
                .ToList();

            foreach (var group in entries
                .Where(entry => entry.AccountID.HasValue &&
                    includedAccountIds.Contains(entry.AccountID.Value))
                .GroupBy(entry => entry.AccountID.Value))
            {
                var account = accountsById[group.Key];
                var first = group.First();
                account.DirectBalance = group.Sum(entry => entry.Debit - entry.Credit);
                account.AccountType = first.AccountType;
                account.AccountCategory = first.AccountCategory;
                account.Status = first.Status;
            }

            foreach (var account in accounts)
            {
                account.Balance = account.DirectBalance;
                account.HasBalance = account.DirectBalance != 0m;
            }

            var rows = new List<TrialBalanceDto>();
            Action<TrialBalanceAccountNode> addRows = null;
            addRows = account =>
            {
                if (account.HasBalance)
                {
                    decimal debit, credit;
                    Split(account.Balance, out debit, out credit);
                    rows.Add(new TrialBalanceDto
                    {
                        AccountCode = account.AccountCode,
                        AccountName = account.AccountName,
                        AccountType = account.AccountType,
                        AccountCategory = account.AccountCategory,
                        Status = account.Status,
                        Debit = debit,
                        Credit = credit
                    });
                }

                foreach (var child in account.Children)
                    addRows(child);
            };
            foreach (var root in roots.OrderBy(account => account.AccountCode))
                addRows(root);

            return rows
                .OrderBy(row => row.AccountCode)
                .AsQueryable();
        }
        public IQueryable<TrialBalanceDto> TrialBalance(ReportFilter filter) => TrialBalance(filter, true);
        public IQueryable<WorkingTrialBalanceDto> WorkingTrialBalance(ReportFilter filter)
        {
            DateTime s, e;
            DateTime? start, endEx;
            if (ReportHelpers.TryGetPeriod(filter.Month, out s, out e)) { start = s; endEx = e; }
            else { start = filter.From?.Date; endEx = filter.To?.Date.AddDays(1); }

            var all = ByBranch(Journal(), filter).ToList();

            var rows = all
                .GroupBy(l => new { l.AccountCode, l.AccountName })
                .Select(g =>
                {
                    var before = start.HasValue ? g.Where(x => x.Date < start).Sum(x => x.Debit - x.Credit) : 0m;
                    var inPeriod = g.Where(x => (!start.HasValue || x.Date >= start) &&
                                                (!endEx.HasValue || x.Date < endEx)).ToList();
                    var d = inPeriod.Sum(x => x.Debit);
                    var c = inPeriod.Sum(x => x.Credit);
                    return new WorkingTrialBalanceDto
                    {
                        AccountCode = g.Key.AccountCode,
                        AccountName = g.Key.AccountName,
                        Begin = before,
                        Debit = d,
                        Credit = c,
                        Change = d - c,
                        Ending = before + d - c
                    };
                })
                .Where(x => x.Begin != 0m || x.Debit != 0m || x.Credit != 0m)
                .OrderBy(x => x.AccountCode)
                .ToList();

            return rows.AsQueryable();
        }

        public IQueryable<WorkingTrialBalanceDto> AdjustedTrialBalance(ReportFilter filter)
        {
            var rows = AsOf(ByBranch(Journal(), filter), filter)
                .GroupBy(l => new { l.AccountCode, l.AccountName })
                .Select(g => new
                {
                    g.Key.AccountCode,
                    g.Key.AccountName,
                    Reg = g.Sum(x => x.JournalType != "Adjusting" ? x.Debit - x.Credit : 0m),
                    Adj = g.Sum(x => x.JournalType == "Adjusting" ? x.Debit - x.Credit : 0m),
                    All = g.Sum(x => x.Debit - x.Credit)
                })
                .OrderBy(x => x.AccountCode)
                .ToList();

            return rows.Select(x =>
            {
                decimal d, c, ad, ac, fd, fc;
                Split(x.Reg, out d, out c);   // unadjusted
                Split(x.Adj, out ad, out ac); // adjustments
                Split(x.All, out fd, out fc); // adjusted = unadjusted + adjustments
                return new WorkingTrialBalanceDto
                {
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    Debit = d,
                    Credit = c,
                    AdjDebit = ad,
                    AdjCredit = ac,
                    AdjustedDebit = fd,
                    AdjustedCredit = fc
                };
            })
            .Where(x => x.AdjustedDebit != 0m || x.AdjustedCredit != 0m || x.AdjDebit != 0m || x.AdjCredit != 0m)
            .AsQueryable();
        }


        private IQueryable<GeneralJournalDto> Journal(
            bool includeCategoryAccountType = false,
            bool includeUnbalancedTransactions = false,
            bool useCategoryAccountTypeOnly = false,
            bool includeAccountHierarchy = false,
            bool scopeRelatedEntities = false)
        {
            var validTx =
                from je in db.TblAccountJournalEntries.AsNoTracking().WhereScope(scope)
                group je by je.TransactionID into g
                where g.Sum(x => x.Debit ?? 0m) == g.Sum(x => x.Credit ?? 0m)
                select g.Key;

            return
                from je in db.TblAccountJournalEntries.AsNoTracking().WhereScope(scope)
                where includeUnbalancedTransactions || validTx.Contains(je.TransactionID)
                join tr in (scopeRelatedEntities
                    ? db.TblAccountTransaction.AsNoTracking().WhereScope(scope)
                    : db.TblAccountTransaction.AsNoTracking())
                    on je.TransactionID equals tr.TransactionID
                join coa in (scopeRelatedEntities
                    ? db.TblChartOfAccount.AsNoTracking().WhereScope(scope)
                    : db.TblChartOfAccount.AsNoTracking())
                    on je.AccountID equals coa.ChartOfAccountID
                join parent0 in (scopeRelatedEntities
                    ? db.TblChartOfAccount.AsNoTracking().WhereScope(scope)
                    : db.TblChartOfAccount.AsNoTracking())
                    on (int?)coa.ParentAccountID equals (int?)parent0.ChartOfAccountID into parents
                from parent in parents.DefaultIfEmpty()
                join cat0 in (scopeRelatedEntities
                    ? db.TblAccountCategories.AsNoTracking().WhereScope(scope)
                    : db.TblAccountCategories.AsNoTracking())
                    on coa.AccountCategoryID equals cat0.AccountCategoryID into cats
                from cat in cats.DefaultIfEmpty()
                join act0 in (scopeRelatedEntities
                    ? db.TblAccountType.AsNoTracking().WhereScope(scope)
                    : db.TblAccountType.AsNoTracking())
                    on (useCategoryAccountTypeOnly
                        ? (int?)cat.AccountTypesID
                        : includeCategoryAccountType
                            ? (int?)coa.AccountTypesID ?? (int?)cat.AccountTypesID
                            : (int?)coa.AccountTypesID) equals (int?)act0.AccountTypesID into accountTypes
                from act in accountTypes.DefaultIfEmpty()
                join st0 in (scopeRelatedEntities
                    ? db.TblReportStatuses.AsNoTracking().WhereScope(scope)
                    : db.TblReportStatuses.AsNoTracking())
                    on coa.StatusID equals st0.StatusID into sts
                from st in sts.DefaultIfEmpty()
                join cl0 in (scopeRelatedEntities
                    ? db.TblClass.AsNoTracking().WhereScope(scope)
                    : db.TblClass.AsNoTracking())
                    on tr.ClassNameID equals (int?)cl0.ClassID into cls       
                from cl in cls.DefaultIfEmpty()
                join cu0 in (scopeRelatedEntities
                    ? db.TblCustomer.AsNoTracking().WhereScope(scope)
                    : db.TblCustomer.AsNoTracking())
                    on (je.CustomerID ?? tr.CustomerID) equals (int?)cu0.CustomerID into cus
                from cu in cus.DefaultIfEmpty()
                join ve0 in (scopeRelatedEntities
                    ? db.TblVendor.AsNoTracking().WhereScope(scope)
                    : db.TblVendor.AsNoTracking())
                    on tr.VendorID equals (int?)ve0.VendorID into vendors
                from ve in vendors.DefaultIfEmpty()
                where includeCategoryAccountType || useCategoryAccountTypeOnly || act != null
                select new GeneralJournalDto
                {
                    Date = tr.TransactionDate,
                    JournalNo = tr.InvoiceNo,
                    TransactionType = tr.Type,
                    JournalType = tr.Type == "Adjusting" ? "Adjusting" : "General",
                    AccountID = includeAccountHierarchy ? (int?)coa.ChartOfAccountID : null,
                    ParentAccountID = includeAccountHierarchy ? coa.ParentAccountID : null,
                    AccountCode = coa.AccountNo,
                    AccountName = coa.AccountName,
                    ParentCode = parent == null ? null : parent.AccountNo,
                    ParentName = parent == null ? null : parent.AccountName,
                    AccountType = act.AccountTypes,
                    AccountCategory = cat == null ? "" : cat.AccountCategory,
                    Status = st == null ? "" : st.Status,
                    Memo = je.Description,
                    Debit = je.Debit ?? 0m,
                    Credit = je.Credit ?? 0m,
                    Class = cl == null ? null : cl.ClassName,
                    Customer = cu == null ? null
                        : (cu.CustomerName != null && cu.CustomerName != "" ? cu.CustomerName : cu.CustomerNameLatin),
                    PartyName = ve == null ? null : ve.VendorName,
                    Branch = je.WarehouseID.ToString(),
                };
        }

        public IQueryable<LedgerDto> JournalReport(ReportFilter filter)
        {
            var selectedAccounts = SelectedAccounts(filter);
            string type = filter.TransactionType;
            var expenses = db.TblExpense.AsNoTracking()
                .Select(e => new { e.ExpenseID, e.PaymentDate, e.Status, e.PaymentNo, e.PVNo })
                .ToList().ToDictionary(e => e.ExpenseID);

            var raw = (
                from je in db.TblAccountJournalEntries.AsNoTracking().WhereScope(scope)
                join tt in db.TblAccountTransaction.AsNoTracking() on je.TransactionID equals tt.TransactionID
                join coa in db.TblChartOfAccount.AsNoTracking() on je.AccountID equals coa.ChartOfAccountID
                join category0 in db.TblAccountCategories.AsNoTracking()
                    on coa.AccountCategoryID equals category0.AccountCategoryID into categories
                from category in categories.DefaultIfEmpty()
                join cu in db.TblCustomer.AsNoTracking() on (je.CustomerID ?? tt.CustomerID) equals (int?)cu.CustomerID into cus
                from c in cus.DefaultIfEmpty()
                join ve in db.TblVendor.AsNoTracking() on tt.VendorID equals (int?)ve.VendorID into ves
                from v in ves.DefaultIfEmpty()
                where string.IsNullOrEmpty(type) || tt.Type == type
                select new
                {
                    je.EntryID,
                    tt.TransactionID,
                    tt.Type,
                    tt.TransactionDate,
                    tt.InvoiceNo,
                    JeInvoice = je.InvoiceNo,
                    ExpenseID = (int?)tt.ExpenseID,
                    AccountID = je.AccountID,
                    coa.AccountNo,
                    coa.AccountName,
                    AccountCategory = category == null ? null : category.AccountCategory,
                    Memo = je.Description ?? "",
                    Debit = je.Debit ?? 0m,
                    Credit = je.Credit ?? 0m,
                    CustomerID = je.CustomerID ?? tt.CustomerID,
                    VendorName = v.VendorName,
                    CustomerName = c.CustomerName != null && c.CustomerName != "" ? c.CustomerName : c.CustomerNameLatin
                }).ToList();

            if (selectedAccounts.Length > 0)
                raw = raw.Where(r => MatchesAccount(selectedAccounts, r.AccountNo, r.AccountName)).ToList();

            var lines = raw.Select(r =>
            {
                bool isExp = (r.ExpenseID ?? 0) > 0;
                var ex = (r.ExpenseID ?? 0) > 0 && expenses.ContainsKey(r.ExpenseID.Value) ? expenses[r.ExpenseID.Value] : null;
                return new
                {
                    r,
                    isExp,
                    ex,
                    Date = isExp && ex != null && ex.PaymentDate != null ? ex.PaymentDate : r.TransactionDate,
                    Number = isExp ? (ex != null && !string.IsNullOrEmpty(ex.PVNo) ? ex.PVNo : r.InvoiceNo)
                                   : (string.IsNullOrEmpty(r.InvoiceNo) ? r.JeInvoice : r.InvoiceNo),
                    Name = isExp ? (r.VendorName ?? r.CustomerName) : (r.CustomerName ?? r.VendorName)
                };
            }).ToList();

            // date filter uses the corrected date
            var inPeriod = lines.AsQueryable().WherePeriod(x => x.Date, filter).ToList();

            var rows = new List<LedgerDto>();

            foreach (var t in inPeriod.GroupBy(x => x.r.TransactionID)
                                      .OrderBy(g => g.Min(x => x.Date)).ThenBy(g => g.First().Number))
            {
                var items = new List<LedgerDto>();
                var debit = t.Where(x => x.r.Debit > 0 || x.r.Credit == 0).OrderBy(x => x.r.EntryID);
                var credit = t.Where(x => x.r.Debit == 0 && x.r.Credit > 0);

                Func<dynamic, decimal, decimal, string, LedgerDto> make = (x, d, c, memo) => new LedgerDto
                {
                    RowType = "line",
                    TransactionID = x.r.TransactionID,
                    Transaction = x.r.Type,
                    TransactionType = x.r.Type,
                    TypeLabel = x.r.Type,
                    Date = x.Date,
                    Number = x.Number,
                    Name = x.Name,
                    AccountName = x.r.AccountName,
                    AccountCode = x.r.AccountNo,
                    Memo = memo,
                    Debit = d,
                    Credit = c,
                    ExpenseID = x.r.ExpenseID,
                    CustomerID = x.r.CustomerID,
                    Status = x.ex == null ? null : x.ex.Status,
                    PaymentNo = x.ex == null ? null : x.ex.PaymentNo
                };

                foreach (var x in debit) items.Add(make(x, x.r.Debit, x.r.Credit, x.r.Memo));

                if (t.First().isExp)   // expenses: one credit line per account
                    foreach (var g in credit.GroupBy(x => x.r.AccountID))
                        items.Add(make(g.First(), 0m, g.Sum(x => x.r.Credit), g.First().r.Memo));
                else
                    foreach (var x in credit.OrderBy(x => x.r.EntryID)) items.Add(make(x, 0m, x.r.Credit, x.r.Memo));

                if (string.Equals(t.First().r.Type, "Invoice", StringComparison.OrdinalIgnoreCase))
                {
                    var displayLines = t
                        .Select(x => new JournalDisplayLine
                        {
                            TransactionID = x.r.TransactionID,
                            AccountID = x.r.AccountID.GetValueOrDefault(),
                            AccountCategory = x.r.AccountCategory,
                            AccountName = x.r.AccountName,
                            Debit = x.r.Debit,
                            Credit = x.r.Credit
                        })
                        .ToList();
                    int receivableAccountId;
                    decimal missingReceivableDebit;

                    if (ReportHelpers.TryGetDisplayOnlyVatReceivableCorrection(
                        displayLines,
                        out receivableAccountId,
                        out missingReceivableDebit))
                    {
                        var receivableLine = t.First(x =>
                            x.r.AccountID == receivableAccountId && x.r.Debit > 0m);
                        var receivableDisplayRow = items.First(item =>
                            item.AccountCode == receivableLine.r.AccountNo &&
                            item.AccountName == receivableLine.r.AccountName &&
                            item.Debit.GetValueOrDefault() > 0m);
                        receivableDisplayRow.Debit =
                            receivableDisplayRow.Debit.GetValueOrDefault() + missingReceivableDebit;

                        var adjustmentMemo =
                            "Includes display-only VAT correction of " +
                            missingReceivableDebit.ToString("N2", CultureInfo.InvariantCulture) +
                            "; not posted to the ledger";
                        receivableDisplayRow.Memo = string.IsNullOrWhiteSpace(receivableDisplayRow.Memo)
                            ? adjustmentMemo
                            : receivableDisplayRow.Memo + " | " + adjustmentMemo;
                    }
                }

                rows.AddRange(items);
                rows.Add(new LedgerDto
                {
                    RowType = "subtotal",
                    TransactionID = t.Key,
                    Transaction = " ",
                    Date = t.First().Date ?? DateTime.MinValue,
                    Number = " ",
                    AccountName = " ",
                    Name = "Total",
                    Memo = " ",
                    Debit = items.Sum(i => i.Debit),
                    Credit = items.Sum(i => i.Credit),
                    ExcludeFromTotals = true
                });
            }
            return rows.AsQueryable();
        }

        public TransactionLogDto TransactionLog(int transactionId)
        {
            if (transactionId <= 0)
                throw new ArgumentOutOfRangeException("transactionId");

            var transaction = db.TblAccountTransaction
                .AsNoTracking()
                .WhereScope(scope)
                .Where(x => x.TransactionID == transactionId)
                .Select(x => new
                {
                    x.TransactionID,
                    x.Type,
                    x.TransactionType,
                    x.TransactionDate,
                    x.Description,
                    x.CreateByUserID,
                    x.CreateDate,
                    x.CustomerID
                })
                .FirstOrDefault();
            if (transaction == null) return null;

            var lines = (
                from entry in db.TblAccountJournalEntries.AsNoTracking().WhereScope(scope)
                where entry.TransactionID == transactionId
                join account0 in db.TblChartOfAccount.AsNoTracking().WhereScope(scope)
                    on entry.AccountID equals (int?)account0.ChartOfAccountID into accounts
                from account in accounts.DefaultIfEmpty()
                join item0 in db.TblItem.AsNoTracking().WhereScope(scope)
                    on entry.ItemID equals item0.ItemID into items
                from item in items.DefaultIfEmpty()
                orderby entry.EntryID
                select new
                {
                    entry.CustomerID,
                    entry.TransactionDate,
                    Memo = entry.Description,
                    ItemName = item == null ? "" : item.ItemName,
                    Account = account == null
                        ? ""
                        : account.AccountNo + "-" + account.AccountName,
                    Debit = entry.Debit ?? 0m,
                    Credit = entry.Credit ?? 0m
                }).ToList();

            var firstLine = lines.FirstOrDefault();
            var createOrModifyUserId = transaction.CreateByUserID;
            var createOrModifyDate = transaction.CreateDate;

            string createOrModifyBy = null;
            if (createOrModifyUserId.HasValue)
            {
                createOrModifyBy = db.TblUserLogin
                    .AsNoTracking()
                    .Where(x =>
                        x.UserLoginID == createOrModifyUserId.Value &&
                        x.SystemID == scope.SystemId)
                    .Select(x => x.FullName != null && x.FullName != ""
                        ? x.FullName
                        : x.UserName)
                    .FirstOrDefault();
            }

            var customerId = transaction.CustomerID ??
                lines.Select(x => x.CustomerID).FirstOrDefault(x => x.HasValue);

            var customerName = customerId.HasValue
                ? db.TblCustomer.AsNoTracking()
                    .WhereScope(scope)
                    .Where(x => x.CustomerID == customerId.Value)
                    .Select(x => x.CustomerName != null && x.CustomerName != ""
                        ? x.CustomerName
                        : x.CustomerNameLatin)
                    .FirstOrDefault()
                : null;

            return new TransactionLogDto
            {
                TransactionID = transaction.TransactionID,
                TransactionType = transaction.Type ?? transaction.TransactionType,
                CreateOrModifyBy = createOrModifyBy ?? "",
                TransactionDate = transaction.TransactionDate ??
                    (firstLine == null ? (DateTime?)null : firstLine.TransactionDate),
                CreateOrModifyDate = createOrModifyDate,
                Name = customerName ?? "",
                Memo = !string.IsNullOrWhiteSpace(transaction.Description)
                    ? transaction.Description
                    : lines.Select(x => x.Memo)
                        .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "",
                Details = lines.Select(x => new TransactionLogLineDto
                {
                    ItemName = x.ItemName ?? "",
                    Account = x.Account ?? "",
                    Memo = x.Memo ?? "",
                    Debit = x.Debit,
                    Credit = x.Credit
                }).ToList()
            };
        }

        private IQueryable<GeneralJournalDto> ByBranch(IQueryable<GeneralJournalDto> all, ReportFilter f)
        {
            var accounts = SelectedAccounts(f);
            if (accounts.Length > 0)
            {
                all = all.Where(l =>
                    accounts.Contains((l.AccountCode ?? "") + "-" + (l.AccountName ?? "")) ||
                    accounts.Contains(l.AccountCode) ||
                    accounts.Contains(l.AccountName));
            }

            var b = ReportHelpers.Clean(f.Branch);
            return b == null ? all : all.Where(l => l.Branch == b);
        }

        private static string[] SelectedAccounts(ReportFilter filter)
        {
            return (filter.Account ?? "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(account => account.Trim())
                .Where(account => account.Length > 0)
                .ToArray();
        }

        private static bool MatchesAccount(string[] selectedAccounts, string accountCode, string accountName)
        {
            return selectedAccounts.Any(account =>
                string.Equals(account, accountCode, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(account, accountName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    account,
                    (accountCode ?? "") + "-" + (accountName ?? ""),
                    StringComparison.OrdinalIgnoreCase));
        }

        private IQueryable<GeneralJournalDto> AsOf(IQueryable<GeneralJournalDto> all, ReportFilter f)
        {
            DateTime start, endExclusive;
            DateTime? end = ReportHelpers.TryGetPeriod(f.Month, out start, out endExclusive)
                ? endExclusive
                : f.To.HasValue ? f.To.Value.Date.AddDays(1) : (DateTime?)null;
            return end == null ? all : all.Where(l => l.Date < end);
        }

        private IQueryable<GeneralJournalDto> InPeriod(IQueryable<GeneralJournalDto> all, ReportFilter f)
        {
            return all.WherePeriod(l => l.Date, f);
        }


        private static string PlSection(string accountType)
        {
            var type = (accountType ?? "").Trim();
            return type.Equals("COGS", StringComparison.OrdinalIgnoreCase)
                    || type.IndexOf("Cost", StringComparison.OrdinalIgnoreCase) >= 0 ? "Cost of Sales"
                 : type.IndexOf("Income", StringComparison.OrdinalIgnoreCase) >= 0
                    || type.IndexOf("Revenue", StringComparison.OrdinalIgnoreCase) >= 0
                    || type.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0 ? "Income"
                 : type.Equals("Expense", StringComparison.OrdinalIgnoreCase)
                    || type.Equals("Expenses", StringComparison.OrdinalIgnoreCase)
                    || type.IndexOf("Expense", StringComparison.OrdinalIgnoreCase) >= 0 ? "Expenses"
                 : null;
        }

        private static string PlSection(GeneralJournalDto line)
        {
            var category = (line.AccountCategory ?? "").Trim();
            if (category.IndexOf("Other Expense", StringComparison.OrdinalIgnoreCase) >= 0
                || category.IndexOf("OtherExpense", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Expenses";
            if (category.IndexOf("Cost", StringComparison.OrdinalIgnoreCase) >= 0
                || category.IndexOf("COGS", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Cost of Sales";
            if (category.IndexOf("Income", StringComparison.OrdinalIgnoreCase) >= 0
                || category.IndexOf("Revenue", StringComparison.OrdinalIgnoreCase) >= 0
                || category.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Income";
            return PlSection(line.AccountType);
        }

        //═════════════ ledger style reports ═════════════

        private IQueryable<LedgerDto> LedgerByGroup(ReportFilter filter)
        {
            var selectedAccounts = SelectedAccounts(filter);
            var hasAccountFilter = selectedAccounts.Length > 0;
            string type = filter.TransactionType;

            var all =
                from je in db.TblAccountJournalEntries.AsNoTracking().WhereScope(scope)
                join tt in db.TblAccountTransaction on je.TransactionID equals tt.TransactionID
                join coa in db.TblChartOfAccount on je.AccountID equals coa.ChartOfAccountID
                join aty in db.TblAccountType
                    on (int?)coa.AccountTypesID equals (int?)aty.AccountTypesID into atys
                from at in atys.DefaultIfEmpty()
                join pc in db.TblChartOfAccount on coa.ParentAccountID equals (int?)pc.ChartOfAccountID into pcs
                from p in pcs.DefaultIfEmpty()
                join cu in db.TblCustomer on je.CustomerID equals (int?)cu.CustomerID into cus
                from c in cus.DefaultIfEmpty()
                join ve in db.TblVendor on tt.VendorID equals (int?)ve.VendorID into ves
                from v in ves.DefaultIfEmpty()
                join it in db.TblItem on je.ItemID equals (int?)it.ItemID into its
                from i in its.DefaultIfEmpty()
                where !hasAccountFilter
                   || selectedAccounts.Contains(coa.AccountNo + "-" + coa.AccountName)
                   || selectedAccounts.Contains(coa.AccountNo)
                   || selectedAccounts.Contains(coa.AccountName)
                   || selectedAccounts.Contains((p.AccountNo ?? "") + "-" + (p.AccountName ?? ""))
                   || selectedAccounts.Contains(p.AccountNo)
                   || selectedAccounts.Contains(p.AccountName)
                select new
                {
                    je.EntryID,
                    je.AccountID,
                    TransactionID = tt.TransactionID,
                    ExpenseID = (int?)tt.ExpenseID,
                    CustomerID = je.CustomerID ?? tt.CustomerID,
                    Date = je.TransactionDate,
                    Type = tt.Type,
                    ItemName = i == null ? "" : i.ItemName,
                    Number = tt.Type == "Expense" ? tt.InvoiceNo : je.InvoiceNo,
                    AccountCode = coa.AccountNo,
                    AccountName = coa.AccountName,
                    GroupCode = p == null ? coa.AccountNo : p.AccountNo,
                    GroupName = p == null ? coa.AccountName : p.AccountName,
                    Name = tt.Type == "Expense"
                        ? v.VendorName
                        : (c.CustomerName != null && c.CustomerName != "" ? c.CustomerName : c.CustomerNameLatin),
                    Memo = je.Description ?? "",
                    Debit = je.Debit ?? 0m,
                    Credit = je.Credit ?? 0m,
                    IsDebitSide = ((at.NormalSide ?? coa.NormalSide) != "CR"),
                };

            var period = all.WherePeriod(r => r.Date, filter)
                .Where(r => string.IsNullOrEmpty(type) || r.Type == type)
                .OrderBy(r => r.GroupCode).ThenBy(r => r.AccountCode)
                .ThenBy(r => r.Date).ThenBy(r => r.EntryID)
                .ToList();

            var rows = new List<LedgerDto>();
            foreach (var parent in period.GroupBy(r => new { r.GroupCode, r.GroupName }))
            {
                string groupAccount = parent.Key.GroupCode + "-" + parent.Key.GroupName;
                decimal groupBalance = 0m;

                rows.Add(new LedgerDto
                {
                    TransactionID = 0,
                    Date = DateTime.MinValue,
                    Transaction = " ",
                    ItemName = " ",
                    Number = " ",
                    Name = " ",
                    Memo = " ",
                    Account = groupAccount,
                    Debit = 0m,
                    Credit = 0m,
                    Balance = 0m,
                    GroupAccount = groupAccount,
                    RowType = "parent",
                    Level = 0
                });

                foreach (var g in parent.GroupBy(r => r.AccountID))
                {
                    var first = g.First();
                    string account = first.AccountCode + "-" + first.AccountName;
                    decimal balance = 0m;
                    var isMainAccount = string.Equals(
                        account,
                        groupAccount,
                        StringComparison.OrdinalIgnoreCase);

                    if (!isMainAccount)
                    {
                        rows.Add(new LedgerDto
                        {
                            TransactionID = 0,
                            Date = DateTime.MinValue,
                            Transaction = " ",
                            ItemName = " ",
                            Number = " ",
                            Name = " ",
                            Memo = " ",
                            Account = account,
                            Debit = 0m,
                            Credit = 0m,
                            Balance = 0m,
                            GroupAccount = groupAccount,
                            RowType = "account",
                            Level = 1
                        });
                    }

                    foreach (var r in g)
                    {
                        balance += r.IsDebitSide ? r.Debit - r.Credit : r.Credit - r.Debit;
                        rows.Add(new LedgerDto
                        {
                            TransactionID = r.TransactionID,
                            ExpenseID = r.ExpenseID,
                            CustomerID = r.CustomerID,
                            Transaction = r.Type,
                            Date = r.Date,
                            ItemName = r.ItemName,
                            Number = r.Number,
                            AccountName = r.AccountName,
                            AccountCode = r.AccountCode,
                            Name = r.Name,
                            Memo = r.Memo,
                            Debit = r.Debit,
                            Credit = r.Credit,
                            TransactionType = r.Type,
                            Balance = balance,
                            GroupAccount = groupAccount,
                            Account = " ",
                            RowType = "line",
                            Level = isMainAccount ? 1 : 2
                        });
                    }

                    if (!isMainAccount)
                    {
                        rows.Add(new LedgerDto
                        {
                            TransactionID = 0,
                            Date = DateTime.MinValue,
                            Transaction = " ",
                            ItemName = " ",
                            Number = " ",
                            Name = " ",
                            Memo = " ",
                            Account = account,
                            Debit = g.Sum(r => r.Debit),
                            Credit = g.Sum(r => r.Credit),
                            Balance = balance,
                            GroupAccount = groupAccount,
                            RowType = "subtotal",
                            Level = 1,
                            ExcludeFromTotals = true
                        });
                    }

                    groupBalance += balance;
                }

                rows.Add(new LedgerDto
                {
                    TransactionID = 0,
                    Date = DateTime.MinValue,
                    Transaction = " ",
                    ItemName = " ",
                    Number = " ",
                    Name = " ",
                    Memo = " ",
                    Account = groupAccount,
                    Debit = parent.Sum(r => r.Debit),
                    Credit = parent.Sum(r => r.Credit),
                    Balance = groupBalance,
                    GroupAccount = groupAccount,
                    RowType = "subtotal",
                    Level = 0,
                    ExcludeFromTotals = true
                });
            }
            return rows.AsQueryable();
        }

        private IQueryable<LedgerDto> Ledger(ReportFilter filter, bool running, bool plOnly)
        {
            var all = ByBranch(Journal(), filter);
            if (plOnly)
                all = all.Where(l => l.AccountType == "Income" || l.AccountType == "COGS" || l.AccountType == "Expense");

            var period = InPeriod(all, filter).ToList();
            var rows = new List<LedgerDto>();

            if (!running)
            {
                rows = period.OrderBy(l => l.Date).ThenBy(l => l.JournalNo).ThenBy(l => l.AccountCode)
                    .Select(l => new LedgerDto
                    {
                        Date = l.Date,
                        Number = l.JournalNo,
                        AccountName = l.AccountName,
                        Memo = l.Memo,
                        Debit = l.Debit,
                        Credit = l.Credit
                    }).ToList();
            }
            else
            {
                foreach (var g in period.GroupBy(l => l.Account).OrderBy(g => g.Key))
                {
                    decimal balance = 0m;
                    if (filter.From.HasValue)
                    {
                        var from = filter.From.Value.Date;
                        var key = g.Key;
                        balance = all.Where(l => (l.AccountCode + " - " + l.AccountName) == key && l.Date < from)
                                     .Sum(l => (decimal?)(l.Debit - l.Credit)) ?? 0m;
                        rows.Add(new LedgerDto { AccountName = g.Key, Memo = "Opening balance", Balance = balance });
                    }
                    foreach (var l in g.OrderBy(x => x.Date).ThenBy(x => x.JournalNo))
                    {
                        balance += l.Debit - l.Credit;
                        rows.Add(new LedgerDto
                        {
                            Date = l.Date,
                            Number = l.JournalNo,
                            AccountName = g.Key,
                            Memo = l.Memo,
                            Debit = l.Debit,
                            Credit = l.Credit,
                            Balance = balance
                        });
                    }
                }
            }
            return rows.AsQueryable();
        }

        public IQueryable<LedgerDto> GeneralLedger(ReportFilter filter)
        {
            var selectedAccounts = SelectedAccounts(filter);
            var hasAccountFilter = selectedAccounts.Length > 0;
            string type = filter.TransactionType;

            var all =
                from je in db.TblAccountJournalEntries.AsNoTracking().WhereScope(scope)
                join tt in db.TblAccountTransaction on je.TransactionID equals tt.TransactionID
                join coa in db.TblChartOfAccount on je.AccountID equals coa.ChartOfAccountID
                join aty in db.TblAccountType
                    on (int?)coa.AccountTypesID equals (int?)aty.AccountTypesID into atys
                from at in atys.DefaultIfEmpty()
                join category0 in db.TblAccountCategories.AsNoTracking()
                    on coa.AccountCategoryID equals category0.AccountCategoryID into categories
                from category in categories.DefaultIfEmpty()
                join pc in db.TblChartOfAccount
                    on coa.ParentAccountID equals (int?)pc.ChartOfAccountID into pcs
                from p in pcs.DefaultIfEmpty()
                join cu in db.TblCustomer on je.CustomerID equals (int?)cu.CustomerID into cus
                from c in cus.DefaultIfEmpty()
                join ve in db.TblVendor on tt.VendorID equals (int?)ve.VendorID into ves
                from v in ves.DefaultIfEmpty()
                join it in db.TblItem on je.ItemID equals (int?)it.ItemID into its
                from i in its.DefaultIfEmpty()
                where !hasAccountFilter
                   || selectedAccounts.Contains(coa.AccountNo + "-" + coa.AccountName)
                   || selectedAccounts.Contains(coa.AccountNo)
                   || selectedAccounts.Contains(coa.AccountName)
                   || selectedAccounts.Contains((p.AccountNo ?? "") + "-" + (p.AccountName ?? ""))
                   || selectedAccounts.Contains(p.AccountNo)
                   || selectedAccounts.Contains(p.AccountName)
                select new
                {
                    je.EntryID,
                    je.AccountID,
                    TransactionID = tt.TransactionID,
                    ExpenseID = (int?)tt.ExpenseID,
                    CustomerID = je.CustomerID ?? tt.CustomerID,
                    Date = je.TransactionDate,
                    Type = tt.Type,
                    ItemName = i == null ? "" : i.ItemName,
                    Number = tt.Type == "Expense" ? tt.InvoiceNo : je.InvoiceNo,
                    AccountCode = coa.AccountNo,
                    AccountName = coa.AccountName,
                    ParentAccountID = coa.ParentAccountID,
                    AccountCategory = category == null ? null : category.AccountCategory,
                    CategoryOrder = category == null
                        ? int.MaxValue
                        : (category.OrderBy ?? category.AccountCategoryID),
                    Name = tt.Type == "Expense"
                        ? v.VendorName
                        : (c.CustomerName != null && c.CustomerName != "" ? c.CustomerName : c.CustomerNameLatin),
                    Memo = je.Description ?? "",
                    Debit = je.Debit ?? 0m,
                    Credit = je.Credit ?? 0m,
                    IsDebitSide = ((at.NormalSide ?? coa.NormalSide) != "CR"),
                };
            var opening = new Dictionary<int?, decimal>();
            if (filter.From.HasValue)
            {
                var fromDate = filter.From.Value.Date;
                opening = all.Where(x => x.Date < fromDate)
                    .GroupBy(x => new { x.AccountID, x.IsDebitSide })
                    .Select(g => new
                    {
                        g.Key.AccountID,
                        g.Key.IsDebitSide,
                        Debit = g.Sum(x => x.Debit),
                        Credit = g.Sum(x => x.Credit)
                    })
                    .ToList()
                    .ToDictionary(x => x.AccountID,
                                  x => x.IsDebitSide ? x.Debit - x.Credit : x.Credit - x.Debit);

                var standaloneOpeningBalances = db.TblAccountOpeningBalances
                    .AsNoTracking()
                    .WhereScope(scope)
                    .Where(openingEntry => openingEntry.AccountID.HasValue)
                    .Where(openingEntry => openingEntry.OpeningBalanceDate.HasValue)
                    .Where(openingEntry => openingEntry.OpeningBalanceDate.Value < fromDate)
                    .Where(openingEntry => !db.TblAccountJournalEntries
                        .Any(entry => entry.OpeningBalanceID == openingEntry.OpeningBalanceID))
                    .Where(openingEntry => !db.TblAccountTransaction
                        .Any(transaction => transaction.OpeningBalanceID == openingEntry.OpeningBalanceID))
                    .Select(openingEntry => new
                    {
                        AccountID = openingEntry.AccountID.Value,
                        Amount = openingEntry.OpeningBalance ?? 0m
                    })
                    .ToList();

                foreach (var openingEntry in standaloneOpeningBalances)
                {
                    decimal accountOpening;
                    opening.TryGetValue(openingEntry.AccountID, out accountOpening);
                    opening[openingEntry.AccountID] = accountOpening + openingEntry.Amount;
                }
            }

            var period = all.WherePeriod(r => r.Date, filter)
                .Where(r => string.IsNullOrEmpty(type) || r.Type == type)
                .OrderBy(r => r.CategoryOrder)
                .ThenBy(r => r.AccountCode)
                .ThenBy(r => r.Date).ThenBy(r => r.EntryID)
                .ToList();

            var accountNodes = new Dictionary<int, LedgerAccountNode>();
            var pendingParentIds = new HashSet<int>();
            foreach (var accountEntries in period
                .Where(entry => entry.AccountID.HasValue)
                .GroupBy(entry => entry.AccountID.Value))
            {
                var first = accountEntries.First();
                accountNodes.Add(accountEntries.Key, new LedgerAccountNode
                {
                    AccountID = accountEntries.Key,
                    ParentAccountID = first.ParentAccountID,
                    AccountCode = first.AccountCode,
                    AccountName = first.AccountName,
                    AccountCategory = first.AccountCategory,
                    CategoryOrder = first.CategoryOrder
                });

                if (first.ParentAccountID.HasValue)
                    pendingParentIds.Add(first.ParentAccountID.Value);
            }

            while (pendingParentIds.Count > 0)
            {
                var batch = pendingParentIds
                    .Where(accountId => !accountNodes.ContainsKey(accountId))
                    .Take(500)
                    .ToList();
                pendingParentIds.ExceptWith(batch);
                if (batch.Count == 0)
                    break;

                var parentAccounts = (
                    from account in db.TblChartOfAccount.AsNoTracking()
                    where batch.Contains(account.ChartOfAccountID)
                    join category0 in db.TblAccountCategories.AsNoTracking()
                        on account.AccountCategoryID equals category0.AccountCategoryID into categories
                    from category in categories.DefaultIfEmpty()
                    select new
                    {
                        AccountID = account.ChartOfAccountID,
                        account.ParentAccountID,
                        AccountCode = account.AccountNo,
                        AccountName = account.AccountName,
                        AccountCategory = category == null ? null : category.AccountCategory,
                        CategoryOrder = category == null
                            ? int.MaxValue
                            : (category.OrderBy ?? category.AccountCategoryID)
                    }).ToList();

                foreach (var parentAccount in parentAccounts)
                {
                    if (accountNodes.ContainsKey(parentAccount.AccountID))
                        continue;

                    accountNodes.Add(parentAccount.AccountID, new LedgerAccountNode
                    {
                        AccountID = parentAccount.AccountID,
                        ParentAccountID = parentAccount.ParentAccountID,
                        AccountCode = parentAccount.AccountCode,
                        AccountName = parentAccount.AccountName,
                        AccountCategory = parentAccount.AccountCategory,
                        CategoryOrder = parentAccount.CategoryOrder
                    });

                    if (parentAccount.ParentAccountID.HasValue)
                        pendingParentIds.Add(parentAccount.ParentAccountID.Value);
                }
            }

            var accountRecords = (
                from account in db.TblChartOfAccount.AsNoTracking().WhereScope(scope)
                join category0 in db.TblAccountCategories.AsNoTracking().WhereScope(scope)
                    on account.AccountCategoryID equals category0.AccountCategoryID into categories
                from category in categories.DefaultIfEmpty()
                select new
                {
                    AccountID = account.ChartOfAccountID,
                    account.ParentAccountID,
                    AccountCode = account.AccountNo,
                    AccountName = account.AccountName,
                    AccountCategory = category == null ? null : category.AccountCategory,
                    CategoryOrder = category == null
                        ? int.MaxValue
                        : (category.OrderBy ?? category.AccountCategoryID)
                }).ToList();
            var accountRecordsById = accountRecords.ToDictionary(account => account.AccountID);
            var parentAccountIds = accountRecords.ToDictionary(
                account => account.AccountID,
                account => account.ParentAccountID);
            var periodAccountIds = new HashSet<int>(
                period.Where(entry => entry.AccountID.HasValue)
                    .Select(entry => entry.AccountID.Value));
            var includedAccountIds = new HashSet<int>(accountNodes.Keys);

            foreach (var account in accountRecords)
            {
                decimal accountOpeningBalance;
                var hasOpeningBalance =
                    opening.TryGetValue((int?)account.AccountID, out accountOpeningBalance) &&
                    accountOpeningBalance != 0m;
                var selectedAccountMatches = hasAccountFilter &&
                    (selectedAccounts.Contains(account.AccountCode + "-" + account.AccountName) ||
                     selectedAccounts.Contains(account.AccountCode) ||
                     selectedAccounts.Contains(account.AccountName));
                var parentMatches = account.ParentAccountID.HasValue &&
                    accountRecordsById.ContainsKey(account.ParentAccountID.Value) &&
                    (selectedAccounts.Contains(
                         accountRecordsById[account.ParentAccountID.Value].AccountCode + "-" +
                         accountRecordsById[account.ParentAccountID.Value].AccountName) ||
                     selectedAccounts.Contains(
                         accountRecordsById[account.ParentAccountID.Value].AccountCode) ||
                     selectedAccounts.Contains(
                         accountRecordsById[account.ParentAccountID.Value].AccountName));
                var accountMatchesSelection =
                    !hasAccountFilter || selectedAccountMatches || parentMatches;
                var accountHasActivityOrBalance =
                    filter.ShowAllAccounts ||
                    periodAccountIds.Contains(account.AccountID) ||
                    hasOpeningBalance ||
                    selectedAccountMatches;

                if (accountMatchesSelection && accountHasActivityOrBalance)
                    includedAccountIds.Add(account.AccountID);
            }

            var includedAccountQueue = new Queue<int>(includedAccountIds);
            while (includedAccountQueue.Count > 0)
            {
                var accountId = includedAccountQueue.Dequeue();
                int? parentAccountId;
                if (parentAccountIds.TryGetValue(accountId, out parentAccountId) &&
                    parentAccountId.HasValue &&
                    accountRecordsById.ContainsKey(parentAccountId.Value) &&
                    includedAccountIds.Add(parentAccountId.Value))
                    includedAccountQueue.Enqueue(parentAccountId.Value);
            }

            foreach (var account in accountRecords
                .Where(account => includedAccountIds.Contains(account.AccountID)))
            {
                if (accountNodes.ContainsKey(account.AccountID))
                    continue;

                accountNodes.Add(account.AccountID, new LedgerAccountNode
                {
                    AccountID = account.AccountID,
                    ParentAccountID = account.ParentAccountID,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    AccountCategory = account.AccountCategory,
                    CategoryOrder = account.CategoryOrder
                });
            }

            var roots = new List<LedgerAccountNode>();
            foreach (var account in accountNodes.Values)
            {
                LedgerAccountNode parent;
                if (account.ParentAccountID.HasValue &&
                    accountNodes.TryGetValue(account.ParentAccountID.Value, out parent))
                    parent.Children.Add(account);
                else
                    roots.Add(account);
            }

            foreach (var account in accountNodes.Values)
                account.Children = account.Children
                    .OrderBy(child => child.AccountCode)
                    .ToList();

            var entriesByAccount = period.ToLookup(entry => entry.AccountID);
            Action<LedgerAccountNode> calculateAccountBalances = null;
            calculateAccountBalances = account =>
            {
                decimal openingBalance = 0m;
                if (filter.From.HasValue)
                    opening.TryGetValue((int?)account.AccountID, out openingBalance);

                account.OpeningBalance = openingBalance;
                account.HasPeriodActivity = entriesByAccount[(int?)account.AccountID].Any();
                foreach (var child in account.Children)
                {
                    calculateAccountBalances(child);
                    account.OpeningBalance += child.OpeningBalance;
                    account.HasPeriodActivity |= child.HasPeriodActivity;
                }
            };
            foreach (var account in roots)
                calculateAccountBalances(account);

            var rows = new List<LedgerDto>();
            Action<LedgerAccountNode, int, string> addAccountRows = null;
            addAccountRows = (account, level, groupAccount) =>
            {
                string accountLabel = account.AccountCode + "-" + account.AccountName;
                rows.Add(new LedgerDto
                {
                    TransactionID = 0,
                    Date = DateTime.MinValue,
                    Transaction = " ",
                    ItemName = " ",
                    Number = " ",
                    Name = " ",
                    Memo = " ",
                    Account = accountLabel,
                    Debit = 0m,
                    Credit = 0m,
                    Balance = account.OpeningBalance,
                    GroupAccount = groupAccount,
                    RowType = account.Children.Count > 0 ? "parent" : "account",
                    Level = level
                });

                decimal balance = 0m;
                if (filter.From.HasValue)
                    opening.TryGetValue((int?)account.AccountID, out balance);

                decimal debitTotal = 0m;
                decimal creditTotal = 0m;
                foreach (var entry in entriesByAccount[(int?)account.AccountID]
                    .OrderBy(entry => entry.Date)
                    .ThenBy(entry => entry.EntryID))
                {
                    balance += entry.IsDebitSide
                        ? entry.Debit - entry.Credit
                        : entry.Credit - entry.Debit;
                    debitTotal += entry.Debit;
                    creditTotal += entry.Credit;
                    rows.Add(new LedgerDto
                    {
                        TransactionID = entry.TransactionID,
                        ExpenseID = entry.ExpenseID,
                        CustomerID = entry.CustomerID,
                        Transaction = entry.Type,
                        Date = entry.Date,
                        ItemName = entry.ItemName,
                        Number = entry.Number,
                        AccountName = entry.AccountName,
                        AccountCode = entry.AccountCode,
                        Name = entry.Name,
                        Memo = entry.Memo,
                        Debit = entry.Debit,
                        Credit = entry.Credit,
                        TransactionType = entry.Type,
                        Balance = balance,
                        GroupAccount = groupAccount,
                        Account = " ",
                        RowType = "line",
                        Level = level + 1
                    });
                }

                foreach (var child in account.Children)
                {
                    addAccountRows(child, level + 1, groupAccount);
                    debitTotal += child.DebitTotal;
                    creditTotal += child.CreditTotal;
                }

                account.DebitTotal = debitTotal;
                account.CreditTotal = creditTotal;
                account.Balance = balance + account.Children.Sum(child => child.Balance);
                if (account.HasPeriodActivity)
                {
                    rows.Add(new LedgerDto
                    {
                        TransactionID = 0,
                        Date = DateTime.MinValue,
                        Transaction = " ",
                        ItemName = " ",
                        Number = " ",
                        Name = " ",
                        Memo = " ",
                        Account = accountLabel,
                        Debit = account.DebitTotal,
                        Credit = account.CreditTotal,
                        Balance = account.Balance,
                        GroupAccount = groupAccount,
                        RowType = "subtotal",
                        Level = level,
                        ExcludeFromTotals = true
                    });
                }
            };

            foreach (var account in roots
                .OrderBy(item => item.AccountCode))
            {
                string groupAccount = account.AccountCode + "-" + account.AccountName;
                addAccountRows(account, 0, groupAccount);
            }

            return rows.AsQueryable();
        }

        public IQueryable<LedgerDto> TransactionDetailByAccount(ReportFilter filter)
        {
            return LedgerByGroup(filter);
        }

        private static void Split(decimal net, out decimal debit, out decimal credit)
        {
            debit = net > 0m ? net : 0m;
            credit = net < 0m ? -net : 0m;
        }

        public IQueryable<LedgerDto> ProfitLossDetail(ReportFilter filter)
        {
            var lines = PlLines(filter)
                .Where(line =>
                    string.IsNullOrEmpty(filter.TransactionType) ||
                    string.Equals(
                        line.TransactionType,
                        filter.TransactionType,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            var rows = new List<LedgerDto>();

            decimal AmountOf(GeneralJournalDto line, bool creditSide) =>
                creditSide ? line.Credit - line.Debit : line.Debit - line.Credit;

            decimal AddSection(
                string section,
                string titleKey,
                string totalKey,
                bool creditSide,
                bool alwaysShow)
            {
                var sectionLines = lines
                    .Where(line => PlSection(line) == section)
                    .ToList();

                if (sectionLines.Count == 0 && !alwaysShow)
                    return 0m;

                rows.Add(new LedgerDto
                {
                    Display = L(titleKey),
                    RowType = "header"
                });

                decimal sectionTotal = 0m;

                var parentGroups = sectionLines
                    .GroupBy(line => new
                    {
                        HasParent = !string.IsNullOrEmpty(line.ParentCode),
                        Code = !string.IsNullOrEmpty(line.ParentCode)
                            ? line.ParentCode
                            : line.AccountCode,
                        Name = !string.IsNullOrEmpty(line.ParentCode)
                            ? line.ParentName
                            : line.AccountName
                    })
                    .OrderBy(group => group.Key.HasParent)
                    .ThenBy(group => group.Key.Code);

                foreach (var parentGroup in parentGroups)
                {
                    if (parentGroup.Key.HasParent)
                    {
                        rows.Add(new LedgerDto
                        {
                            Display = parentGroup.Key.Code + "-" + parentGroup.Key.Name,
                            AccountCode = parentGroup.Key.Code,
                            AccountName = parentGroup.Key.Name,
                            Level = 1,
                            RowType = "parent"
                        });
                    }

                    var accountGroups = parentGroup
                        .GroupBy(line => new { line.AccountCode, line.AccountName })
                        .OrderBy(group => group.Key.AccountCode);

                    foreach (var accountGroup in accountGroups)
                    {
                        var account = accountGroup.Key.AccountCode + "-" +
                                    accountGroup.Key.AccountName;
                        var level = parentGroup.Key.HasParent ? 2 : 1;

                        rows.Add(new LedgerDto
                        {
                            Display = account,
                            AccountCode = accountGroup.Key.AccountCode,
                            AccountName = accountGroup.Key.AccountName,
                            Level = level,
                            RowType = "account"
                        });

                        var accountTotal = 0m;

                        foreach (var line in accountGroup
                            .OrderBy(line => line.Date)
                            .ThenBy(line => line.JournalNo))
                        {
                            var amount = AmountOf(line, creditSide);
                            accountTotal += amount;

                            rows.Add(new LedgerDto
                            {
                                Display = account,
                                Transaction = line.TransactionType,
                                TransactionType = line.TransactionType,
                                Date = line.Date,
                                Number = line.JournalNo,
                                Name = line.Customer ?? line.PartyName,
                                AccountCode = line.AccountCode,
                                AccountName = line.AccountName,
                                Memo = line.Memo,
                                Amount = amount,
                                Level = level + 1,
                                RowType = "line"
                            });
                        }

                        if (accountTotal != 0m)
                        {
                            sectionTotal += accountTotal;
                            rows.Add(new LedgerDto
                            {
                                Display = L("PL_Total") + " " + account,
                                Amount = accountTotal,
                                Level = level,
                                RowType = "subtotal"
                            });
                        }
                    }
                }

                rows.Add(new LedgerDto
                {
                    Display = L(totalKey),
                    Amount = sectionTotal,
                    RowType = "total"
                });

                return sectionTotal;
            }

            var income = AddSection("Income", "PL_Revenue", "PL_TotalRevenue", true, false);
            var costOfSales = AddSection(
                "Cost of Sales", "PL_CostOfSales", "PL_TotalCostOfSales", false, false);

            if (income != 0m || costOfSales != 0m || lines.Any(l => PlSection(l) == "Income" || PlSection(l) == "Cost of Sales"))
            {
                rows.Add(new LedgerDto
                {
                    Display = L("PL_GrossProfit"),
                    Amount = income - costOfSales,
                    RowType = "grand"
                });
            }

            var expenses = AddSection(
                "Expenses", "PL_Expenses", "PL_TotalExpenses", false, false);

            if (rows.Count > 0)
            {
                rows.Add(new LedgerDto
                {
                    Display = L("PL_NetProfit"),
                    Amount = income - costOfSales - expenses,
                    RowType = "grand"
                });
            }

            return rows.AsQueryable();
        }

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { "PL_Revenue", "Revenue" },
            { "PL_TotalRevenue", "Total Revenue" },
            { "PL_CostOfSales", "Cost of Sales" },
            { "PL_TotalCostOfSales", "Total Cost of Sales" },
            { "PL_GrossProfit", "Gross Profit" },
            { "PL_Expenses", "Expense" },
            { "PL_TotalExpenses", "Total Expense" },
            { "PL_NetProfit", "Net Profit" },
            { "PL_Total", "Total" },
            { "PL_Unclassified", "Unclassified" },
            { "PL_Unassigned", "Unassigned" },
        };

        private string L(string key) => En.TryGetValue(key, out var v) ? v : key;
        private List<GeneralJournalDto> PlLines(
            ReportFilter filter,
            bool includeCategoryAccountType = false,
            bool includeUnbalancedTransactions = false)
        {
            var q = InPeriod(
                ByBranch(Journal(includeCategoryAccountType, includeUnbalancedTransactions), filter),
                filter);
            var customer = ReportHelpers.Clean(filter.Customer);
            if (customer != null)
            {
                int customerId;
                if (int.TryParse(customer, out customerId))
                {
                    var name = db.TblCustomer.AsNoTracking()
                        .Where(c => c.CustomerID == customerId)
                        .Select(c => c.CustomerName != null && c.CustomerName != "" ? c.CustomerName : c.CustomerNameLatin)
                        .FirstOrDefault();
                    if (name != null) customer = name;
                }
                q = q.Where(l => l.Customer == customer);
            }

            return q.ToList().Where(l => (includeCategoryAccountType ? PlSection(l) : PlSection(l.AccountType)) != null).ToList();
        }

        private static int PlOrder(string section)
        {
            return section == "Income" ? 1 : section == "Cost of Sales" ? 2 : 3;
        }

        private string PlTitle(string section)
        {
            return section == "Income" ? L("PL_Revenue")
                 : section == "Cost of Sales" ? L("PL_CostOfSales")
                 : L("PL_Expenses");
        }

        public IQueryable<ProfitLossDto> ProfitLossStandard(ReportFilter filter)
        {
            var lines = PlLines(
                filter,
                includeCategoryAccountType: true,
                includeUnbalancedTransactions: true);
            var rows = new List<ProfitLossDto>();
            decimal incomeTotal = lines.Where(l => PlSection(l) == "Income")
                                       .Sum(l => l.Credit - l.Debit);

            ProfitLossDto R(string type, int level, string display, decimal amount,
                            string code = null, string name = null)
            {
                return new ProfitLossDto
                {
                    RowType = type,
                    Level = level,
                    Display = display,
                    AccountCode = code,
                    AccountName = name ?? display,
                    Amount = amount,
                    Percent = incomeTotal == 0m ? 0m : Math.Round(amount / incomeTotal * 100m, 2)
                };
            }
            Func<string, string, string, bool, bool, decimal> addSection =
                (section, titleKey, totalKey, creditSide, always) =>
            {
                Func<IEnumerable<GeneralJournalDto>, decimal> amt =
                    g => creditSide ? g.Sum(x => x.Credit - x.Debit) : g.Sum(x => x.Debit - x.Credit);

                var part = lines.Where(l => PlSection(l) == section).ToList();
                if (part.Count == 0 && !always) return 0m;

                rows.Add(R("header", 0, L(titleKey), 0m));
                decimal total = 0m;

                foreach (var grp in part
                    .GroupBy(l => new
                    {
                        IsChild = !string.IsNullOrEmpty(l.ParentCode),
                        Code = !string.IsNullOrEmpty(l.ParentCode) ? l.ParentCode : l.AccountCode
                    })
                    .OrderBy(g => g.Key.IsChild).ThenBy(g => g.Key.Code))
                {
                    var first = grp.First();
                    var sum = amt(grp);
                    if (sum == 0m) continue;
                    total += sum;

                    if (!grp.Key.IsChild)
                    {
                        rows.Add(R("account", 1, first.AccountCode + "-" + first.AccountName, sum,
                                   first.AccountCode, first.AccountName));
                        continue;
                    }

                    var parent = first.ParentCode + "-" + first.ParentName;
                    rows.Add(R("parent", 1, parent, 0m, first.ParentCode, first.ParentName));

                    foreach (var c in grp.GroupBy(x => new { x.AccountCode, x.AccountName })
                                         .OrderBy(x => x.Key.AccountCode))
                    {
                        var a = amt(c);
                        if (a == 0m) continue;
                        rows.Add(R("account", 2, c.Key.AccountCode + "-" + c.Key.AccountName, a,
                                   c.Key.AccountCode, c.Key.AccountName));
                    }
                    rows.Add(R("subtotal", 1, L("PL_Total") + " " + parent, sum));
                }

                rows.Add(R("total", 0, L(totalKey), total));
                return total;
            };

            var income = addSection("Income", "PL_Revenue", "PL_TotalRevenue", true, false);
            var cogs = addSection("Cost of Sales", "PL_CostOfSales", "PL_TotalCostOfSales", false, false);

            if (income != 0m || cogs != 0m || lines.Any(l => PlSection(l) == "Income" || PlSection(l) == "Cost of Sales"))
            {
                rows.Add(R("grand", 0, L("PL_GrossProfit"), income - cogs));
            }

            var expense = addSection("Expenses", "PL_Expenses", "PL_TotalExpenses", false, false);

            if (rows.Count > 0)
            {
                rows.Add(R("net", 0, L("PL_NetProfit"), income - cogs - expense));
            }

            return rows.AsQueryable();

            
        }
        
        private List<int> PlYears(ReportFilter filter, IList<GeneralJournalDto> lines)
        {
            var lineDates = lines
                .Where(line => line.Date.HasValue)
                .Select(line => line.Date.Value)
                .ToList();
            DateTime periodStart, periodEnd;
            int monthOnly;
            if (ReportHelpers.TryGetPeriod(filter.Month, out periodStart, out periodEnd))
                return new List<int> { periodStart.Year };

            if (ReportHelpers.TryGetMonthOnly(filter.Month, out monthOnly))
            {
                var matchingYears = lineDates
                    .Where(date => date.Month == monthOnly)
                    .Select(date => date.Year)
                    .Distinct()
                    .OrderBy(year => year)
                    .ToList();
                return matchingYears.Count > 0
                    ? matchingYears
                    : new List<int> { (filter.To ?? filter.From ?? DateTime.Today).Year };
            }

            DateTime? firstDate = filter.From.HasValue
                ? filter.From.Value.Date
                : lineDates.Count > 0 ? lineDates.Min().Date : (DateTime?)null;
            DateTime? lastDate = filter.To.HasValue
                ? filter.To.Value.Date
                : lineDates.Count > 0 ? lineDates.Max().Date : (DateTime?)null;
            if (firstDate.HasValue && lastDate.HasValue && firstDate.Value <= lastDate.Value)
            {
                var years = new List<int>();
                for (var year = firstDate.Value.Year; year <= lastDate.Value.Year; year++)
                    years.Add(year);
                return years;
            }

            return lineDates
                .Select(date => date.Year)
                .Distinct()
                .OrderBy(year => year)
                .ToList();
        }

        private IQueryable<ProfitLossByClassCustomerDto> ProfitLossByClassCustomer(
            ReportFilter filter,
            Func<GeneralJournalDto, string> label)
        {
            var lines = PlLines(filter);
            var groupKeys = lines
                .Select(label)
                .Distinct()
                .OrderBy(key => key)
                .ToList();
            var groupIndexes = groupKeys
                .Select((key, index) => new { key, index })
                .ToDictionary(item => item.key, item => item.index);
            var rows = new List<ProfitLossByClassCustomerDto>();
            var sections = new[] { "Income", "Cost of Sales", "Expenses" };

            decimal LineAmount(GeneralJournalDto line)
            {
                var section = PlSection(line.AccountType);
                return section == "Income" ? line.Credit - line.Debit : line.Debit - line.Credit;
            }

            decimal[] Amounts(IEnumerable<GeneralJournalDto> source)
            {
                var amounts = new decimal[groupKeys.Count];
                foreach (var line in source)
                    amounts[groupIndexes[label(line)]] += LineAmount(line);
                return amounts;
            }

            decimal[] Subtract(params decimal[][] values)
            {
                var result = values[0].ToArray();
                for (var i = 1; i < values.Length; i++)
                    for (var j = 0; j < result.Length; j++)
                        result[j] -= values[i][j];
                return result;
            }

            bool HasAmounts(decimal[] amounts) => amounts.Any(amount => amount != 0m);

            ProfitLossByClassCustomerDto MakeRow(
                string section,
                string display,
                string rowType,
                int level,
                decimal[] amounts,
                string accountCode = null,
                string accountName = null)
            {
                var groupValues = new Dictionary<string, object>();
                for (var i = 0; i < groupKeys.Count; i++)
                    groupValues[groupKeys[i]] = amounts[i];

                return new ProfitLossByClassCustomerDto
                {
                    Section = PlTitle(section),
                    Display = display,
                    RowType = rowType,
                    Level = level,
                    AccountCode = accountCode,
                    AccountName = accountName,
                    Amount = amounts.Sum(),
                    GroupValues = groupValues
                };
            }

            foreach (var section in sections)
            {
                var sectionLines = lines
                    .Where(line => PlSection(line.AccountType) == section)
                    .ToList();
                var accountGroups = sectionLines
                    .GroupBy(line => new
                    {
                        IsChild = !string.IsNullOrEmpty(line.ParentCode),
                        Code = !string.IsNullOrEmpty(line.ParentCode)
                            ? line.ParentCode
                            : line.AccountCode
                    })
                    .OrderBy(accountGroup => accountGroup.Key.IsChild)
                    .ThenBy(accountGroup => accountGroup.Key.Code);
                var sectionRows = new List<ProfitLossByClassCustomerDto>();
                var sectionTotals = new decimal[groupKeys.Count];

                foreach (var accountGroup in accountGroups)
                {
                    var amounts = Amounts(accountGroup);
                    if (!HasAmounts(amounts)) continue;

                    for (var i = 0; i < amounts.Length; i++)
                        sectionTotals[i] += amounts[i];

                    var first = accountGroup.First();
                    if (!accountGroup.Key.IsChild)
                    {
                        sectionRows.Add(MakeRow(
                            section,
                            first.AccountCode + "-" + first.AccountName,
                            "account",
                            1,
                            amounts,
                            first.AccountCode,
                            first.AccountName));
                        continue;
                    }

                    var parent = first.ParentCode + "-" + first.ParentName;
                    sectionRows.Add(MakeRow(
                        section,
                        parent,
                        "parent",
                        1,
                        new decimal[groupKeys.Count],
                        first.ParentCode,
                        first.ParentName));

                    foreach (var account in accountGroup
                        .GroupBy(line => new { line.AccountCode, line.AccountName })
                        .OrderBy(account => account.Key.AccountCode))
                    {
                        var accountAmounts = Amounts(account);
                        if (!HasAmounts(accountAmounts)) continue;

                        sectionRows.Add(MakeRow(
                            section,
                            account.Key.AccountCode + "-" + account.Key.AccountName,
                            "account",
                            2,
                            accountAmounts,
                            account.Key.AccountCode,
                            account.Key.AccountName));
                    }

                    sectionRows.Add(MakeRow(
                        section,
                        L("PL_Total") + " " + parent,
                        "subtotal",
                        1,
                        amounts));
                }

                if (sectionRows.Count > 0)
                {
                    rows.Add(MakeRow(
                        section,
                        PlTitle(section),
                        "header",
                        0,
                        new decimal[groupKeys.Count]));
                    rows.AddRange(sectionRows);
                    var totalKey = section == "Income" ? "PL_TotalRevenue"
                        : section == "Cost of Sales" ? "PL_TotalCostOfSales"
                        : "PL_TotalExpenses";
                    rows.Add(MakeRow(
                        section,
                        L(totalKey),
                        "total",
                        0,
                        sectionTotals));
                }

                if (section == "Cost of Sales" &&
                    (HasAmounts(Amounts(lines.Where(line => PlSection(line.AccountType) == "Income"))) ||
                     HasAmounts(sectionTotals) ||
                     rows.Count > 0))
                {
                    var grossProfit = Subtract(
                        Amounts(lines.Where(line => PlSection(line.AccountType) == "Income")),
                        Amounts(lines.Where(line => PlSection(line.AccountType) == "Cost of Sales")));
                    rows.Add(MakeRow(
                        section,
                        L("PL_GrossProfit"),
                        "grand",
                        0,
                        grossProfit));
                }
            }

            if (rows.Count > 0)
            {
                var netProfit = Subtract(
                    Amounts(lines.Where(line => PlSection(line.AccountType) == "Income")),
                    Amounts(lines.Where(line => PlSection(line.AccountType) == "Cost of Sales")),
                    Amounts(lines.Where(line => PlSection(line.AccountType) == "Expenses")));
                rows.Add(MakeRow(
                    "Expenses",
                    L("PL_NetProfit"),
                    "grand",
                    0,
                    netProfit));
            }

            return rows.AsQueryable();
        }
        public IQueryable<ProfitLossDto> ProfitLossYearToDate(ReportFilter filter)
        {
            DateTime s, e;
            DateTime refDate = ReportHelpers.TryGetPeriod(filter.Month, out s, out e)
                ? e.AddDays(-1)
                : (filter.To ?? DateTime.Today).Date;

            var thisYear = refDate.Year;
            var thisMonth = refDate.Month;

            var thisMonthStart = new DateTime(thisYear, thisMonth, 1);
            var thisMonthEnd = thisMonthStart.AddMonths(1);

            var prevMonthStart = thisMonthStart.AddMonths(-1);
            var prevMonthEnd = thisMonthStart;

            var lastYearMonthStart = thisMonthStart.AddYears(-1);
            var lastYearMonthEnd = thisMonthEnd.AddYears(-1);

            var ytdStart = new DateTime(thisYear, 1, 1);
            var ytdEnd = refDate.Date.AddDays(1);

            var minDate = lastYearMonthStart < ytdStart ? lastYearMonthStart : ytdStart;
            var maxDate = thisMonthEnd > ytdEnd ? thisMonthEnd : ytdEnd;

            var allLines = ByBranch(Journal(includeCategoryAccountType: true, includeUnbalancedTransactions: true), filter)
                .Where(l => l.Date >= minDate && l.Date < maxDate)
                .ToList();

            var plLines = allLines.Where(l => PlSection(l) != null).ToList();

            var customer = ReportHelpers.Clean(filter.Customer);
            if (customer != null)
            {
                int customerId;
                if (int.TryParse(customer, out customerId))
                {
                    var name = db.TblCustomer.AsNoTracking()
                        .Where(c => c.CustomerID == customerId)
                        .Select(c => c.CustomerName != null && c.CustomerName != "" ? c.CustomerName : c.CustomerNameLatin)
                        .FirstOrDefault();
                    if (name != null) customer = name;
                }
                plLines = plLines.Where(l => l.Customer == customer).ToList();
            }

            decimal LineAmt(GeneralJournalDto l)
            {
                var sec = PlSection(l);
                return sec == "Income" ? (l.Credit - l.Debit) : (l.Debit - l.Credit);
            }

            var rows = new List<ProfitLossDto>();

            var sections = new[] { "Income", "Cost of Sales", "Expenses" };
            var secTotals = new Dictionary<string, (decimal ytd, decimal prev, decimal cur, decimal lastYear)>();

            foreach (var sec in sections)
            {
                var secLines = plLines.Where(l => PlSection(l) == sec).ToList();
                var secGroups = secLines
                    .GroupBy(l => new
                    {
                        IsChild = !string.IsNullOrEmpty(l.ParentCode),
                        Code = !string.IsNullOrEmpty(l.ParentCode) ? l.ParentCode : l.AccountCode
                    })
                    .OrderBy(g => g.Key.IsChild)
                    .ThenBy(g => g.Key.Code)
                    .ToList();

                decimal sYtd = 0m, sPrev = 0m, sCur = 0m, sLastYear = 0m;
                var secRows = new List<ProfitLossDto>();

                foreach (var g in secGroups)
                {
                    var ytd = g.Where(l => l.Date >= ytdStart && l.Date < ytdEnd).Sum(l => LineAmt(l));
                    var prev = g.Where(l => l.Date >= prevMonthStart && l.Date < prevMonthEnd).Sum(l => LineAmt(l));
                    var cur = g.Where(l => l.Date >= thisMonthStart && l.Date < thisMonthEnd).Sum(l => LineAmt(l));
                    var last = g.Where(l => l.Date >= lastYearMonthStart && l.Date < lastYearMonthEnd).Sum(l => LineAmt(l));

                    if (ytd == 0m && prev == 0m && cur == 0m && last == 0m)
                        continue;

                    sYtd += ytd;
                    sPrev += prev;
                    sCur += cur;
                    sLastYear += last;

                    var first = g.First();
                    if (!g.Key.IsChild)
                    {
                        secRows.Add(new ProfitLossDto
                        {
                            RowType = "account",
                            Level = 1,
                            Section = PlTitle(sec),
                            AccountCode = first.AccountCode,
                            AccountName = first.AccountName,
                            Display = first.AccountCode + "-" + first.AccountName,
                            YearToDate = ytd,
                            PreviousMonth = prev,
                            ThisMonth = cur,
                            ThisMonthLastYear = last,
                            Amount = ytd
                        });
                        continue;
                    }

                    var parent = first.ParentCode + "-" + first.ParentName;
                    secRows.Add(new ProfitLossDto
                    {
                        RowType = "parent",
                        Level = 1,
                        Section = PlTitle(sec),
                        AccountCode = first.ParentCode,
                        AccountName = first.ParentName,
                        Display = parent
                    });

                    foreach (var account in g.GroupBy(l => new { l.AccountCode, l.AccountName })
                                             .OrderBy(account => account.Key.AccountCode))
                    {
                        var accountYtd = account.Where(l => l.Date >= ytdStart && l.Date < ytdEnd).Sum(l => LineAmt(l));
                        var accountPrev = account.Where(l => l.Date >= prevMonthStart && l.Date < prevMonthEnd).Sum(l => LineAmt(l));
                        var accountCur = account.Where(l => l.Date >= thisMonthStart && l.Date < thisMonthEnd).Sum(l => LineAmt(l));
                        var accountLast = account.Where(l => l.Date >= lastYearMonthStart && l.Date < lastYearMonthEnd).Sum(l => LineAmt(l));

                        if (accountYtd == 0m && accountPrev == 0m && accountCur == 0m && accountLast == 0m)
                            continue;

                        secRows.Add(new ProfitLossDto
                        {
                            RowType = "account",
                            Level = 2,
                            Section = PlTitle(sec),
                            AccountCode = account.Key.AccountCode,
                            AccountName = account.Key.AccountName,
                            Display = account.Key.AccountCode + "-" + account.Key.AccountName,
                            YearToDate = accountYtd,
                            PreviousMonth = accountPrev,
                            ThisMonth = accountCur,
                            ThisMonthLastYear = accountLast,
                            Amount = accountYtd
                        });
                    }

                    secRows.Add(new ProfitLossDto
                    {
                        RowType = "subtotal",
                        Level = 1,
                        Section = PlTitle(sec),
                        Display = L("PL_Total") + " " + parent,
                        YearToDate = ytd,
                        PreviousMonth = prev,
                        ThisMonth = cur,
                        ThisMonthLastYear = last,
                        Amount = ytd
                    });
                }

                secTotals[sec] = (sYtd, sPrev, sCur, sLastYear);

                if (secRows.Count > 0)
                {
                    rows.Add(new ProfitLossDto
                    {
                        RowType = "header",
                        Section = PlTitle(sec),
                        Display = PlTitle(sec)
                    });

                    rows.AddRange(secRows);

                    string totalKey = sec == "Income" ? "PL_TotalRevenue"
                                    : sec == "Cost of Sales" ? "PL_TotalCostOfSales"
                                    : "PL_TotalExpenses";

                    rows.Add(new ProfitLossDto
                    {
                        RowType = "total",
                        Section = PlTitle(sec),
                        Display = L(totalKey),
                        YearToDate = sYtd,
                        PreviousMonth = sPrev,
                        ThisMonth = sCur,
                        ThisMonthLastYear = sLastYear,
                        Amount = sYtd
                    });
                }

                if (sec == "Cost of Sales" && (secTotals["Income"].ytd != 0m || secTotals["Cost of Sales"].ytd != 0m || rows.Count > 0))
                {
                    var inc = secTotals["Income"];
                    var cos = secTotals["Cost of Sales"];
                    rows.Add(new ProfitLossDto
                    {
                        RowType = "grand",
                        Display = L("PL_GrossProfit"),
                        YearToDate = inc.ytd - cos.ytd,
                        PreviousMonth = inc.prev - cos.prev,
                        ThisMonth = inc.cur - cos.cur,
                        ThisMonthLastYear = inc.lastYear - cos.lastYear,
                        Amount = inc.ytd - cos.ytd
                    });
                }
            }

            if (rows.Count > 0)
            {
                var inc = secTotals["Income"];
                var cos = secTotals["Cost of Sales"];
                var exp = secTotals["Expenses"];
                rows.Add(new ProfitLossDto
                {
                    RowType = "grand",
                    Display = L("PL_NetProfit"),
                    YearToDate = inc.ytd - cos.ytd - exp.ytd,
                    PreviousMonth = inc.prev - cos.prev - exp.prev,
                    ThisMonth = inc.cur - cos.cur - exp.cur,
                    ThisMonthLastYear = inc.lastYear - cos.lastYear - exp.lastYear,
                    Amount = inc.ytd - cos.ytd - exp.ytd
                });
            }

            return rows.AsQueryable();
        }

        public IQueryable<ProfitLossByMonthDto> ProfitLossByMonth(ReportFilter filter)
        {
            var lines = PlLines(filter);
            var lineDates = lines
                .Where(line => line.Date.HasValue)
                .Select(line => line.Date.Value)
                .ToList();
            var months = new List<DateTime>();
            DateTime periodStart, periodEnd;
            int monthOnly;
            if (ReportHelpers.TryGetPeriod(filter.Month, out periodStart, out periodEnd))
            {
                for (var month = new DateTime(periodStart.Year, periodStart.Month, 1);
                     month < periodEnd;
                     month = month.AddMonths(1))
                    months.Add(month);
            }
            else if (ReportHelpers.TryGetMonthOnly(filter.Month, out monthOnly))
            {
                months = lineDates
                    .Where(date => date.Month == monthOnly)
                    .Select(date => new DateTime(date.Year, date.Month, 1))
                    .Distinct()
                    .OrderBy(month => month)
                    .ToList();
                if (months.Count == 0)
                {
                    var year = (filter.To ?? filter.From ?? DateTime.Today).Year;
                    months.Add(new DateTime(year, monthOnly, 1));
                }
            }
            else
            {
                DateTime? firstDate = filter.From.HasValue
                    ? filter.From.Value.Date
                    : lineDates.Count > 0 ? lineDates.Min().Date : (DateTime?)null;
                DateTime? lastDate = filter.To.HasValue
                    ? filter.To.Value.Date
                    : lineDates.Count > 0 ? lineDates.Max().Date : (DateTime?)null;

                if (firstDate.HasValue && lastDate.HasValue && firstDate.Value <= lastDate.Value)
                {
                    for (var month = new DateTime(firstDate.Value.Year, firstDate.Value.Month, 1);
                         month <= new DateTime(lastDate.Value.Year, lastDate.Value.Month, 1);
                         month = month.AddMonths(1))
                        months.Add(month);
                }
                else
                {
                    months = lineDates
                        .Select(date => new DateTime(date.Year, date.Month, 1))
                        .Distinct()
                        .OrderBy(month => month)
                        .ToList();
                }
            }
            var monthKeys = months
                .Select(month => month.ToString("yyyyMM", CultureInfo.InvariantCulture))
                .ToList();

            decimal LineAmount(GeneralJournalDto line)
            {
                var section = PlSection(line.AccountType);
                return section == "Income" ? line.Credit - line.Debit : line.Debit - line.Credit;
            }

            decimal[] Amounts(IEnumerable<GeneralJournalDto> source)
            {
                var accountLines = source.ToList();
                return months.Select(month => accountLines
                    .Where(line => line.Date.HasValue &&
                                   line.Date.Value >= month &&
                                   line.Date.Value < month.AddMonths(1))
                    .Sum(LineAmount)).ToArray();
            }

            bool HasAmounts(decimal[] amounts) => amounts.Any(amount => amount != 0m);

            ProfitLossByMonthDto MakeRow(
                string display,
                string rowType,
                int level,
                decimal[] amounts,
                string accountCode = null,
                string accountName = null)
            {
                var monthlyValues = new Dictionary<string, object>();
                for (var i = 0; i < monthKeys.Count; i++)
                    monthlyValues[monthKeys[i]] = amounts[i];

                return new ProfitLossByMonthDto
                {
                    Display = display,
                    RowType = rowType,
                    Level = level,
                    AccountCode = accountCode,
                    AccountName = accountName,
                    Amount = amounts.Sum(),
                    MonthlyValues = monthlyValues
                };
            }

            var rows = new List<ProfitLossByMonthDto>();
            var sections = new[] { "Income", "Cost of Sales", "Expenses" };
            var sectionTotals = sections.ToDictionary(section => section, _ => new decimal[months.Count]);

            foreach (var section in sections)
            {
                var sectionLines = lines.Where(line => PlSection(line.AccountType) == section).ToList();
                var accountGroups = sectionLines
                    .GroupBy(line => new
                    {
                        IsChild = !string.IsNullOrEmpty(line.ParentCode),
                        Code = !string.IsNullOrEmpty(line.ParentCode) ? line.ParentCode : line.AccountCode
                    })
                    .OrderBy(group => group.Key.IsChild)
                    .ThenBy(group => group.Key.Code);
                var sectionRows = new List<ProfitLossByMonthDto>();

                foreach (var group in accountGroups)
                {
                    var amounts = Amounts(group);
                    if (!HasAmounts(amounts)) continue;

                    for (var i = 0; i < amounts.Length; i++)
                        sectionTotals[section][i] += amounts[i];

                    var first = group.First();
                    if (!group.Key.IsChild)
                    {
                        sectionRows.Add(MakeRow(
                            first.AccountCode + "-" + first.AccountName,
                            "account",
                            1,
                            amounts,
                            first.AccountCode,
                            first.AccountName));
                        continue;
                    }

                    var parent = first.ParentCode + "-" + first.ParentName;
                    sectionRows.Add(MakeRow(parent, "parent", 1, new decimal[months.Count],
                        first.ParentCode, first.ParentName));

                    foreach (var account in group
                        .GroupBy(line => new { line.AccountCode, line.AccountName })
                        .OrderBy(account => account.Key.AccountCode))
                    {
                        var accountAmounts = Amounts(account);
                        if (!HasAmounts(accountAmounts)) continue;

                        sectionRows.Add(MakeRow(
                            account.Key.AccountCode + "-" + account.Key.AccountName,
                            "account",
                            2,
                            accountAmounts,
                            account.Key.AccountCode,
                            account.Key.AccountName));
                    }

                    sectionRows.Add(MakeRow(
                        L("PL_Total") + " " + parent,
                        "subtotal",
                        1,
                        amounts));
                }

                if (sectionRows.Count > 0)
                {
                    rows.Add(MakeRow(PlTitle(section), "header", 0, new decimal[months.Count]));
                    rows.AddRange(sectionRows);
                    var totalKey = section == "Income" ? "PL_TotalRevenue"
                        : section == "Cost of Sales" ? "PL_TotalCostOfSales"
                        : "PL_TotalExpenses";
                    rows.Add(MakeRow(L(totalKey), "total", 0, sectionTotals[section]));
                }

                if (section == "Cost of Sales" &&
                    (HasAmounts(sectionTotals["Income"]) ||
                     HasAmounts(sectionTotals["Cost of Sales"]) ||
                     rows.Count > 0))
                {
                    var grossProfit = new decimal[months.Count];
                    for (var i = 0; i < grossProfit.Length; i++)
                        grossProfit[i] = sectionTotals["Income"][i] - sectionTotals["Cost of Sales"][i];
                    rows.Add(MakeRow(L("PL_GrossProfit"), "grand", 0, grossProfit));
                }
            }

            if (rows.Count > 0)
            {
                var netProfit = new decimal[months.Count];
                for (var i = 0; i < netProfit.Length; i++)
                    netProfit[i] = sectionTotals["Income"][i] -
                                   sectionTotals["Cost of Sales"][i] -
                                   sectionTotals["Expenses"][i];
                rows.Add(MakeRow(L("PL_NetProfit"), "grand", 0, netProfit));
            }

            var monthSort = monthKeys.FirstOrDefault(key =>
                string.Equals(key, filter.SortBy, StringComparison.OrdinalIgnoreCase));
            if (monthSort != null)
            {
                var descending = string.Equals(filter.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
                rows = (descending
                    ? rows.OrderByDescending(row => Convert.ToDecimal(row.MonthlyValues[monthSort]))
                    : rows.OrderBy(row => Convert.ToDecimal(row.MonthlyValues[monthSort])))
                    .ToList();
            }

            return rows.AsQueryable();
        }

        public IQueryable<ProfitLossByYearDto> ProfitLossByYear(ReportFilter filter)
        {
            var lines = PlLines(filter);
            var years = PlYears(filter, lines);
            var yearKeys = years
                .Select(year => year.ToString(CultureInfo.InvariantCulture))
                .ToList();

            decimal LineAmount(GeneralJournalDto line)
            {
                var section = PlSection(line.AccountType);
                return section == "Income" ? line.Credit - line.Debit : line.Debit - line.Credit;
            }

            decimal[] Amounts(IEnumerable<GeneralJournalDto> source)
            {
                var accountLines = source.ToList();
                return years.Select(year => accountLines
                    .Where(line => line.Date.HasValue && line.Date.Value.Year == year)
                    .Sum(LineAmount)).ToArray();
            }

            bool HasAmounts(decimal[] amounts) => amounts.Any(amount => amount != 0m);

            ProfitLossByYearDto MakeRow(
                string display,
                string rowType,
                int level,
                decimal[] amounts,
                string accountCode = null,
                string accountName = null)
            {
                var yearlyValues = new Dictionary<string, object>();
                for (var i = 0; i < yearKeys.Count; i++)
                    yearlyValues[yearKeys[i]] = amounts[i];

                return new ProfitLossByYearDto
                {
                    Display = display,
                    RowType = rowType,
                    Level = level,
                    AccountCode = accountCode,
                    AccountName = accountName,
                    Amount = amounts.Sum(),
                    YearlyValues = yearlyValues
                };
            }

            var rows = new List<ProfitLossByYearDto>();
            var sections = new[] { "Income", "Cost of Sales", "Expenses" };
            var sectionTotals = sections.ToDictionary(section => section, _ => new decimal[years.Count]);

            foreach (var section in sections)
            {
                var sectionLines = lines.Where(line => PlSection(line.AccountType) == section).ToList();
                var accountGroups = sectionLines
                    .GroupBy(line => new
                    {
                        IsChild = !string.IsNullOrEmpty(line.ParentCode),
                        Code = !string.IsNullOrEmpty(line.ParentCode) ? line.ParentCode : line.AccountCode
                    })
                    .OrderBy(group => group.Key.IsChild)
                    .ThenBy(group => group.Key.Code);
                var sectionRows = new List<ProfitLossByYearDto>();

                foreach (var group in accountGroups)
                {
                    var amounts = Amounts(group);
                    if (!HasAmounts(amounts)) continue;

                    for (var i = 0; i < amounts.Length; i++)
                        sectionTotals[section][i] += amounts[i];

                    var first = group.First();
                    if (!group.Key.IsChild)
                    {
                        sectionRows.Add(MakeRow(
                            first.AccountCode + "-" + first.AccountName,
                            "account",
                            1,
                            amounts,
                            first.AccountCode,
                            first.AccountName));
                        continue;
                    }

                    var parent = first.ParentCode + "-" + first.ParentName;
                    sectionRows.Add(MakeRow(parent, "parent", 1, new decimal[years.Count],
                        first.ParentCode, first.ParentName));

                    foreach (var account in group
                        .GroupBy(line => new { line.AccountCode, line.AccountName })
                        .OrderBy(account => account.Key.AccountCode))
                    {
                        var accountAmounts = Amounts(account);
                        if (!HasAmounts(accountAmounts)) continue;

                        sectionRows.Add(MakeRow(
                            account.Key.AccountCode + "-" + account.Key.AccountName,
                            "account",
                            2,
                            accountAmounts,
                            account.Key.AccountCode,
                            account.Key.AccountName));
                    }

                    sectionRows.Add(MakeRow(
                        L("PL_Total") + " " + parent,
                        "subtotal",
                        1,
                        amounts));
                }

                if (sectionRows.Count > 0)
                {
                    rows.Add(MakeRow(PlTitle(section), "header", 0, new decimal[years.Count]));
                    rows.AddRange(sectionRows);
                    var totalKey = section == "Income" ? "PL_TotalRevenue"
                        : section == "Cost of Sales" ? "PL_TotalCostOfSales"
                        : "PL_TotalExpenses";
                    rows.Add(MakeRow(L(totalKey), "total", 0, sectionTotals[section]));
                }

                if (section == "Cost of Sales" &&
                    (HasAmounts(sectionTotals["Income"]) ||
                     HasAmounts(sectionTotals["Cost of Sales"]) ||
                     rows.Count > 0))
                {
                    var grossProfit = new decimal[years.Count];
                    for (var i = 0; i < grossProfit.Length; i++)
                        grossProfit[i] = sectionTotals["Income"][i] - sectionTotals["Cost of Sales"][i];
                    rows.Add(MakeRow(L("PL_GrossProfit"), "grand", 0, grossProfit));
                }
            }

            if (rows.Count > 0)
            {
                var netProfit = new decimal[years.Count];
                for (var i = 0; i < netProfit.Length; i++)
                    netProfit[i] = sectionTotals["Income"][i] -
                                   sectionTotals["Cost of Sales"][i] -
                                   sectionTotals["Expenses"][i];
                rows.Add(MakeRow(L("PL_NetProfit"), "grand", 0, netProfit));
            }

            var yearSort = yearKeys.FirstOrDefault(key =>
                string.Equals(key, filter.SortBy, StringComparison.OrdinalIgnoreCase));
            if (yearSort != null)
            {
                var descending = string.Equals(filter.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
                rows = (descending
                    ? rows.OrderByDescending(row => Convert.ToDecimal(row.YearlyValues[yearSort]))
                    : rows.OrderBy(row => Convert.ToDecimal(row.YearlyValues[yearSort])))
                    .ToList();
            }

            return rows.AsQueryable();
        }

        public IQueryable<ProfitLossByClassCustomerDto> ProfitLossByClass(ReportFilter filter) =>
            ProfitLossByClassCustomer(filter,
                l => string.IsNullOrEmpty(l.Class) ? L("PL_Unclassified") : l.Class);

        public IQueryable<ProfitLossByClassCustomerDto> ProfitLossByCustomer(ReportFilter filter) =>
            ProfitLossByClassCustomer(filter,
                l => string.IsNullOrEmpty(l.Customer) ? L("PL_Unassigned") : l.Customer);



        public IQueryable<BalanceSheetDto> BalanceSheet(ReportFilter filter)
        {
            DateTime s, e;
            DateTime asOfDate = ReportHelpers.TryGetPeriod(filter.Month, out s, out e)
                ? e.AddDays(-1)
                : (filter.To ?? DateTime.Today).Date;
            DateTime endExclusive = asOfDate.Date.AddDays(1);

            var lines = AsOf(
                ByBranch(Journal(
                    useCategoryAccountTypeOnly: true,
                    includeAccountHierarchy: true), filter),
                filter).ToList();
            var rows = new List<BalanceSheetDto>();
            string domainId = ReportHelpers.Clean(filter.DomainID);
            string branch = ReportHelpers.Clean(filter.Branch);
            int branchId;
            bool hasBranchId = int.TryParse(branch, out branchId);
            var openingBalanceRows = (
                from opening in db.TblAccountOpeningBalances.AsNoTracking().WhereScope(scope)
                join account in db.TblChartOfAccount.AsNoTracking()
                    on opening.AccountID equals (int?)account.ChartOfAccountID
                join category0 in db.TblAccountCategories.AsNoTracking()
                    on account.AccountCategoryID equals category0.AccountCategoryID into categories
                from category in categories.DefaultIfEmpty()
                join type0 in db.TblAccountType.AsNoTracking()
                    on category.AccountTypesID equals (int?)type0.AccountTypesID into accountTypes
                from accountType in accountTypes.DefaultIfEmpty()
                where (domainId == null ||
                        (account.DomainID != null && account.DomainID.Contains(domainId)) ||
                        (opening.DomainID != null && opening.DomainID.Contains(domainId)))
                    && opening.OpeningBalanceDate.HasValue
                    && opening.OpeningBalanceDate.Value < endExclusive
                    && (branch == null ||
                        (hasBranchId &&
                         (!opening.WarehouseID.HasValue || opening.WarehouseID.Value == branchId)))
                    && !db.TblAccountJournalEntries
                        .Any(entry => entry.OpeningBalanceID == opening.OpeningBalanceID)
                    && !db.TblAccountTransaction
                        .Any(transaction => transaction.OpeningBalanceID == opening.OpeningBalanceID)
                select new
                {
                    account.AccountNo,
                    account.AccountName,
                    AccountID = account.ChartOfAccountID,
                    AccountCategory = category == null ? "" : category.AccountCategory,
                    AccountType = accountType == null ? null : accountType.AccountTypes,
                    NormalSide = accountType == null ? null : accountType.NormalSide,
                    OpeningBalance = opening.OpeningBalance,
                    Date = opening.OpeningBalanceDate,
                    opening.WarehouseID
                }).ToList();

            var selectedAccounts = SelectedAccounts(filter);
            foreach (var opening in openingBalanceRows)
            {
                if (selectedAccounts.Length > 0 &&
                    !MatchesAccount(selectedAccounts, opening.AccountNo, opening.AccountName))
                    continue;

                decimal amount = opening.OpeningBalance ?? 0m;
                bool isCreditNormalSide = string.Equals(
                    opening.NormalSide,
                    "CR",
                    StringComparison.OrdinalIgnoreCase);
                lines.Add(new GeneralJournalDto
                {
                    Date = opening.Date,
                    AccountID = opening.AccountID,
                    AccountCode = opening.AccountNo,
                    AccountName = opening.AccountName,
                    AccountType = opening.AccountType,
                    AccountCategory = opening.AccountCategory,
                    Debit = isCreditNormalSide ? 0m : amount,
                    Credit = isCreditNormalSide ? amount : 0m,
                    Branch = opening.WarehouseID.HasValue
                        ? opening.WarehouseID.Value.ToString()
                        : null
                });
            }

            Func<string, string> normalizeAccountType = value =>
            {
                return (value ?? "").Trim().ToLowerInvariant()
                    .Replace(" ", "")
                    .Replace("-", "");
            };
            Func<string, string, bool> isAssetAccount = (accountType, category) =>
            {
                var type = normalizeAccountType(accountType);
                var categoryName = normalizeAccountType(category);
                if (type.Contains("liability") || type.Contains("payable") ||
                    categoryName.Contains("liability") || categoryName.Contains("payable"))
                    return false;

                return type == "asset" || type == "assets" ||
                       type == "fixedasset" || type == "fixedassets" ||
                       type == "currentasset" || type == "currentassets" ||
                       type == "noncurrentasset" || type == "noncurrentassets" ||
                       type == "otherasset" || type == "otherassets" ||
                       type == "othercurrentasset" || type == "othercurrentassets" ||
                       type == "bank" || type == "cash" ||
                       type == "cashandcashequivalents" ||
                       type == "accountreceivable" || type == "accountsreceivable" ||
                       type == "inventory" ||
                       categoryName.Contains("asset") ||
                       categoryName == "bank" || categoryName == "cash" ||
                       categoryName.Contains("receivable") ||
                       categoryName == "inventory";
            };
            Func<string, string, bool> isFixedAsset = (accountType, category) =>
            {
                var type = normalizeAccountType(accountType);
                var categoryName = normalizeAccountType(category);
                return type.Contains("fixedasset") || type.Contains("noncurrentasset") ||
                       categoryName.Contains("fixedasset") ||
                       categoryName.Contains("noncurrentasset");
            };
            Func<string, string, bool> isLiabilityAccount = (accountType, category) =>
            {
                var type = normalizeAccountType(accountType);
                var categoryName = normalizeAccountType(category);
                return type.Contains("liability") || type.Contains("payable") ||
                       categoryName.Contains("liability") || categoryName.Contains("payable");
            };
            Func<string, string, bool> isLongTermLiability = (accountType, category) =>
            {
                var type = normalizeAccountType(accountType);
                var categoryName = normalizeAccountType(category);
                return type.Contains("longterm") || type.Contains("noncurrent") ||
                       categoryName.Contains("longterm") || categoryName.Contains("noncurrent");
            };
            var assetAccountLines = lines
                .Where(line => isAssetAccount(line.AccountType, line.AccountCategory))
                .ToList();
            var assetRoots = BuildBalanceSheetAccountTree(assetAccountLines);
            var liabilityAccountLines = lines
                .Where(line => isLiabilityAccount(line.AccountType, line.AccountCategory))
                .ToList();
            var liabilityRoots = BuildBalanceSheetAccountTree(
                liabilityAccountLines,
                creditNormal: true);
            var equityAccountLines = lines
                .Where(line => line.AccountType == "Equity")
                .ToList();
            var equityRoots = BuildBalanceSheetAccountTree(
                equityAccountLines,
                creditNormal: true);

            // "as of" date decides which year is "current"
            var yearStart = new DateTime(asOfDate.Year, 1, 1);

            var plLines = lines.Where(l => PlSection(l.AccountType) != null).ToList();
            var profit = plLines.Where(l => l.Date >= yearStart).Sum(l => l.Credit - l.Debit);   // current year
            var retained = plLines.Where(l => l.Date < yearStart).Sum(l => l.Credit - l.Debit);   // earlier years

            decimal currentAssets = 0m;
            decimal fixedAssets = 0m;

            Action<BalanceSheetAccountNode, string, string, string, string, int, int> addAccountNodeRows = null;
            addAccountNodeRows = (node, section, subsection, category, accountCategory, baseLevel, depth) =>
            {
                bool hasChildren = node.Children.Count > 0;
                rows.Add(new BalanceSheetDto
                {
                    RowType = hasChildren ? "parent" : "account",
                    Section = section,
                    Subsection = subsection,
                    Category = category,
                    AccountCategory = accountCategory,
                    AccountCode = node.AccountCode,
                    AccountName = node.AccountName,
                    Level = baseLevel + depth,
                    Amount = hasChildren ? 0m : node.Amount
                });

                foreach (var child in node.Children)
                    addAccountNodeRows(
                        child,
                        section,
                        subsection,
                        category,
                        accountCategory,
                        baseLevel,
                        depth + 1);

                if (hasChildren)
                {
                    rows.Add(new BalanceSheetDto
                    {
                        RowType = "subtotal",
                        Section = section,
                        Subsection = subsection,
                        Category = category,
                        AccountCategory = accountCategory,
                        AccountName = "Total " + node.AccountCode + "-" + node.AccountName,
                        Level = baseLevel + depth,
                        Amount = node.Total
                    });
                }
            };

            foreach (var assetGroup in assetRoots
                .GroupBy(account => isFixedAsset(account.AccountType, account.AccountCategory)
                    ? "Fixed Assets"
                    : "Current Assets")
                .OrderBy(group => group.Key == "Fixed Assets" ? 1 : 0))
            {
                decimal groupTotal = 0m;

                foreach (var category in assetGroup
                    .GroupBy(account => string.IsNullOrWhiteSpace(account.AccountCategory)
                        ? "Uncategorized"
                        : account.AccountCategory.Trim())
                    .OrderBy(category => category.Min(account => account.CategoryOrder))
                    .ThenBy(category => category.Key))
                {
                    var categoryRoots = category
                        .OrderBy(account => account.AccountCode)
                        .ToList();
                    decimal categoryTotal = categoryRoots.Sum(account => account.Total);
                    groupTotal += categoryTotal;

                    foreach (var account in categoryRoots)
                        addAccountNodeRows(
                            account,
                            "Assets",
                            assetGroup.Key,
                            category.Key,
                            category.Key,
                            3,
                            0);

                    rows.Add(new BalanceSheetDto
                    {
                        RowType = "subtotal",
                        Section = "Assets",
                        Subsection = assetGroup.Key,
                        Category = category.Key,
                        AccountCategory = category.Key,
                        AccountName = "Total " + category.Key,
                        Level = 2,
                        Amount = categoryTotal
                    });
                }

                if (assetGroup.Key == "Fixed Assets")
                    fixedAssets = groupTotal;
                else
                    currentAssets = groupTotal;

                rows.Add(new BalanceSheetDto
                {
                    RowType = "total",
                    Section = "Assets",
                    Subsection = assetGroup.Key,
                    Category = assetGroup.Key,
                    AccountCategory = assetGroup.Key,
                    AccountName = "Total " + assetGroup.Key,
                    Level = 1,
                    Amount = groupTotal
                });
            }

            var assets = currentAssets + fixedAssets;
            rows.Add(new BalanceSheetDto
            {
                RowType = "total",
                Section = "Assets",
                Subsection = "Assets",
                Category = "Assets",
                AccountCategory = "Assets",
                AccountName = "Total Assets",
                Amount = assets
            });

            decimal liabilities = 0m;
            foreach (var liabilityGroup in liabilityRoots
                .GroupBy(account => isLongTermLiability(account.AccountType, account.AccountCategory)
                    ? "Long Term Liabilities"
                    : "Current Liabilities")
                .OrderBy(group => group.Key == "Long Term Liabilities" ? 1 : 0))
            {
                decimal groupTotal = 0m;

                foreach (var category in liabilityGroup
                    .GroupBy(account => string.IsNullOrWhiteSpace(account.AccountCategory)
                        ? "Uncategorized"
                        : account.AccountCategory.Trim())
                    .OrderBy(category => category.Min(account => account.CategoryOrder))
                    .ThenBy(category => category.Key))
                {
                    var categoryRoots = category
                        .OrderBy(account => account.AccountCode)
                        .ToList();
                    decimal categoryTotal = categoryRoots.Sum(account => account.Total);
                    groupTotal += categoryTotal;

                    foreach (var account in categoryRoots)
                        addAccountNodeRows(
                            account,
                            "Liabilities and Equity",
                            "Liabilities",
                            liabilityGroup.Key,
                            category.Key,
                            4,
                            0);

                    rows.Add(new BalanceSheetDto
                    {
                        RowType = "subtotal",
                        Section = "Liabilities and Equity",
                        Subsection = "Liabilities",
                        Category = liabilityGroup.Key,
                        AccountCategory = category.Key,
                        AccountName = "Total " + category.Key,
                        Level = 3,
                        Amount = categoryTotal
                    });
                }

                liabilities += groupTotal;
                rows.Add(new BalanceSheetDto
                {
                    RowType = "total",
                    Section = "Liabilities and Equity",
                    Subsection = "Liabilities",
                    Category = liabilityGroup.Key,
                    AccountCategory = liabilityGroup.Key,
                    AccountName = "Total " + liabilityGroup.Key,
                    Level = 2,
                    Amount = groupTotal
                });
            }

            rows.Add(new BalanceSheetDto
            {
                RowType = "total",
                Section = "Liabilities and Equity",
                Subsection = "Liabilities",
                Category = "Liabilities",
                AccountCategory = "Liabilities",
                AccountName = "Total Liabilities",
                Level = 1,
                Amount = liabilities
            });

            decimal equity = equityRoots.Sum(account => account.Total);
            foreach (var account in equityRoots
                .OrderBy(account => account.CategoryOrder)
                .ThenBy(account => account.AccountCode))
            {
                addAccountNodeRows(
                    account,
                    "Liabilities and Equity",
                    "Equity",
                    "Equity",
                    "Equity",
                    2,
                    0);
            }

            if (retained != 0m)
            {
                equity += retained;
                rows.Add(new BalanceSheetDto
                {
                    RowType = "account",
                    Section = "Liabilities and Equity",
                    Subsection = "Equity",
                    Category = "Equity",
                    AccountCategory = "Equity",
                    AccountName = "Retained Earnings",
                    Level = 2,
                    Amount = retained
                });
            }
            if (profit != 0m)
            {
                equity += profit;
                rows.Add(new BalanceSheetDto
                {
                    RowType = "account",
                    Section = "Liabilities and Equity",
                    Subsection = "Equity",
                    Category = "Equity",
                    AccountCategory = "Equity",
                    AccountName = "Net Profit (current year)",
                    Level = 2,
                    Amount = profit
                });
            }

            rows.Add(new BalanceSheetDto
            {
                RowType = "total",
                Section = "Liabilities and Equity",
                Subsection = "Equity",
                Category = "Equity",
                AccountCategory = "Equity",
                AccountName = "Total Equity",
                Level = 1,
                Amount = equity
            });

            rows.Add(new BalanceSheetDto
            {
                RowType = "grand",
                Section = "Liabilities and Equity",
                Subsection = "Total",
                Category = "Total",
                AccountCategory = "Total",
                AccountName = "Total Liabilities & Equity",
                Level = 1,
                Amount = liabilities + equity
            });

            return rows.AsQueryable();
        }

        private List<BalanceSheetAccountNode> BuildBalanceSheetAccountTree(
            IEnumerable<GeneralJournalDto> lines,
            bool creditNormal = false)
        {
            var pendingIds = new HashSet<int>(
                lines.Where(line => line.AccountID.HasValue)
                    .Select(line => line.AccountID.Value));
            var nodes = new Dictionary<int, BalanceSheetAccountNode>();

            while (pendingIds.Count > 0)
            {
                var batch = pendingIds.Take(500).ToList();
                pendingIds.ExceptWith(batch);

                var accountRows = (
                    from account in db.TblChartOfAccount.AsNoTracking()
                    where batch.Contains(account.ChartOfAccountID)
                    join category0 in db.TblAccountCategories.AsNoTracking()
                        on account.AccountCategoryID equals category0.AccountCategoryID into categories
                    from category in categories.DefaultIfEmpty()
                    join type0 in db.TblAccountType.AsNoTracking()
                        on category.AccountTypesID equals (int?)type0.AccountTypesID into accountTypes
                    from accountType in accountTypes.DefaultIfEmpty()
                    select new
                    {
                        AccountID = account.ChartOfAccountID,
                        account.ParentAccountID,
                        account.AccountNo,
                        account.AccountName,
                        AccountCategory = category == null ? null : category.AccountCategory,
                        CategoryOrder = category == null
                            ? int.MaxValue
                            : (category.OrderBy ?? category.AccountCategoryID),
                        AccountType = accountType == null ? null : accountType.AccountTypes
                    }).ToList();

                foreach (var account in accountRows)
                {
                    if (nodes.ContainsKey(account.AccountID))
                        continue;

                    nodes.Add(account.AccountID, new BalanceSheetAccountNode
                    {
                        AccountID = account.AccountID,
                        ParentAccountID = account.ParentAccountID,
                        AccountCode = account.AccountNo,
                        AccountName = account.AccountName,
                        AccountCategory = account.AccountCategory,
                        CategoryOrder = account.CategoryOrder,
                        AccountType = account.AccountType
                    });

                    if (account.ParentAccountID.HasValue &&
                        !nodes.ContainsKey(account.ParentAccountID.Value))
                        pendingIds.Add(account.ParentAccountID.Value);
                }
            }

            foreach (var line in lines)
            {
                BalanceSheetAccountNode node;
                if (line.AccountID.HasValue &&
                    nodes.TryGetValue(line.AccountID.Value, out node))
                    node.Amount += creditNormal
                        ? line.Credit - line.Debit
                        : line.Debit - line.Credit;
            }

            var roots = new List<BalanceSheetAccountNode>();
            foreach (var node in nodes.Values)
            {
                BalanceSheetAccountNode parent;
                if (node.ParentAccountID.HasValue &&
                    nodes.TryGetValue(node.ParentAccountID.Value, out parent))
                    parent.Children.Add(node);
                else
                    roots.Add(node);
            }

            for (int i = roots.Count - 1; i >= 0; i--)
            {
                if (!PruneEmptyBalanceSheetAccounts(roots[i]))
                    roots.RemoveAt(i);
                else
                    CalculateBalanceSheetAccountTotal(roots[i]);
            }

            return roots.OrderBy(node => node.AccountCode).ToList();
        }

        private static bool PruneEmptyBalanceSheetAccounts(BalanceSheetAccountNode node)
        {
            for (int i = node.Children.Count - 1; i >= 0; i--)
            {
                if (!PruneEmptyBalanceSheetAccounts(node.Children[i]))
                    node.Children.RemoveAt(i);
            }

            node.Children = node.Children.OrderBy(child => child.AccountCode).ToList();
            return node.Amount != 0m || node.Children.Count > 0;
        }

        private static decimal CalculateBalanceSheetAccountTotal(BalanceSheetAccountNode node)
        {
            node.Total = node.Amount;
            foreach (var child in node.Children)
                node.Total += CalculateBalanceSheetAccountTotal(child);
            return node.Total;
        }

        private sealed class TrialBalanceAccountNode
        {
            public int AccountID { get; set; }
            public int? ParentAccountID { get; set; }
            public string AccountCode { get; set; }
            public string AccountName { get; set; }
            public string AccountType { get; set; }
            public string AccountCategory { get; set; }
            public string Status { get; set; }
            public decimal DirectBalance { get; set; }
            public decimal Balance { get; set; }
            public bool HasBalance { get; set; }
            public List<TrialBalanceAccountNode> Children { get; set; } =
                new List<TrialBalanceAccountNode>();
        }

        private sealed class LedgerAccountNode
        {
            public int AccountID { get; set; }
            public int? ParentAccountID { get; set; }
            public string AccountCode { get; set; }
            public string AccountName { get; set; }
            public string AccountCategory { get; set; }
            public int CategoryOrder { get; set; }
            public decimal DebitTotal { get; set; }
            public decimal CreditTotal { get; set; }
            public decimal Balance { get; set; }
            public decimal OpeningBalance { get; set; }
            public bool HasPeriodActivity { get; set; }
            public List<LedgerAccountNode> Children { get; set; } =
                new List<LedgerAccountNode>();
        }

        private sealed class BalanceSheetAccountNode
        {
            public int AccountID { get; set; }
            public int? ParentAccountID { get; set; }
            public string AccountCode { get; set; }
            public string AccountName { get; set; }
            public string AccountCategory { get; set; }
            public int CategoryOrder { get; set; }
            public string AccountType { get; set; }
            public decimal Amount { get; set; }
            public decimal Total { get; set; }
            public List<BalanceSheetAccountNode> Children { get; set; } =
                new List<BalanceSheetAccountNode>();
        }

    }
}