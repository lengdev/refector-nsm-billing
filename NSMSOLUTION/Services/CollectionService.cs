using MVC;
using NSMSOLUTION.Models.Collections;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace NSMSOLUTION.Services
{
    public class CollectionException : Exception
    {
        public CollectionException(string message) : base(message) { }
    }

    public class SaveResult
    {
        public int id { get; set; }
        public int customerId { get; set; }
        public string number { get; set; }
        public decimal unapplied { get; set; }
        public List<string> warnings { get; set; }
    }

    /*
     * Deposit flow
     * ------------
     * - Only TblCollection, TblCollectionDetail and TblCollectionCharge are used.
     * - A customer with NO open invoice who pays money gets a real RV (number by class). Nothing is applied,
     *   so the whole amount sits in TblCollection.UnappliedAmount. That RV is the customer's deposit.
     * - When the customer has invoices, the form opens that RV (GetCredits finds it) and the user updates it:
     *   the invoices are paid from the deposit (plus extra cash if the invoices are bigger), and what is
     *   left stays in UnappliedAmount.
     * - Type: "Deposit" while nothing is applied yet, otherwise "Receipt" (not stored, worked out in the list).
     * - Payment slips are kept in TblCollection.SlipPath (paths separated by ";").
     * - The old "Customer Credit" payment method is kept only so receipts saved before can still be viewed.
     *
     * Needs: TblCollection.SlipPath column (see deposit.sql).
     */
    public class CollectionService
    {
        private const string Prefix = "RV-";
        private const string SaleType = "Official Invoice";
        private const int PaidStatus = 10;
        private const int UnpaidStatus = 11;
        private const string PaymentCategory = "Collection";
        private const decimal Epsilon = 0.004m;
        private const string CreditMethodName = "Customer Credit"; // legacy method, see note above
        private const string PostedStatus = "Posted";
        private const string RollbackStatus = "Rollback";

        private const string DepositAccountsSql = @"
            SELECT ChartOfAccountID AS id,
                   AccountNo        AS accountNo,
                   AccountName      AS name,
                   COALESCE(NULLIF(LTRIM(RTRIM(AccountType)), ''),
                            CASE AccountTypesID
                                 WHEN 1 THEN 'Asset'
                                 WHEN 2 THEN 'Liability'
                                 WHEN 3 THEN 'Equity'
                                 WHEN 4 THEN 'Revenue'
                                 WHEN 5 THEN 'Expense'
                            END)    AS type,
                   ParentAccountID  AS parentId
              FROM TblChartOfAccount
             WHERE SystemID = @p0";

        private const string ClassesScopedSql =
            @"SELECT ClassID, ClassName, PrefixCollection, StartCollectionNumber
                FROM TblClass
               WHERE SystemID = @p0 AND ISNULL(PrefixCollection, '') <> ''
               ORDER BY ClassName";

        private const string ClassesSql =
            @"SELECT ClassID, ClassName, PrefixCollection, StartCollectionNumber
                FROM TblClass
               WHERE ISNULL(PrefixCollection, '') <> ''
               ORDER BY ClassName";

        private const string LockInvoiceSql =
            "SELECT CustomerBalanceID FROM TblCustomerBalance WITH (UPDLOCK, ROWLOCK) WHERE CustomerBalanceID = @p0";

        private static readonly Regex SequenceToken = new Regex(@"\{(0+)\}", RegexOptions.Compiled);

        private static readonly List<LookupItem> Currencies = new List<LookupItem>
        {
            new LookupItem { value = "USD", label = "USD", symbol = "$" },
        };

        private static readonly List<LookupItem> ChargeTypes = new List<LookupItem>
        {
            new LookupItem { value = "Late fee",     label = "Late fee" },
            new LookupItem { value = "Reconnection", label = "Reconnection fee" },
            new LookupItem { value = "Service fee",  label = "Service fee" },
            new LookupItem { value = "Other",        label = "Other" },
        };

        private readonly DBContext db;
        private readonly Func<IQueryable<CustomerBalance>, IQueryable<CustomerBalance>> invoiceScope;
        private readonly bool checkType;

        private int creditMethodId;
        private bool creditMethodLoaded;

        public int? PaidByUserId { get; set; }

        public CollectionService(
            DBContext db,
            Func<IQueryable<CustomerBalance>, IQueryable<CustomerBalance>> invoiceScope,
            bool checkType)
        {
            this.db = db;
            this.invoiceScope = invoiceScope;
            this.checkType = checkType;
        }

        // ============================================================
        // helpers
        // ============================================================
        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
        private static string Iso(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        private static int SystemId { get { return int.Parse(new HTML().SystemID); } }
        private static int WarehouseId { get { return int.Parse(new HTML().WarehouseID); } }

        private static string Symbol(string currency)
        {
            var c = Currencies.FirstOrDefault(x => x.value == currency);
            return c != null ? c.symbol : "$";
        }

        private static void TryRollback(DbContextTransaction tx)
        {
            try { tx.Rollback(); }
            catch { }
        }

        // ---- legacy "Customer Credit" payment method (old receipts only) ----
        private int CreditMethodId()
        {
            if (creditMethodLoaded)
                return creditMethodId;

            var systemId = SystemId;
            creditMethodId = db.TblPaymentMethod.AsNoTracking()
                .Where(m => m.SystemID == systemId && m.Method == CreditMethodName)
                .Select(m => m.PaymentMethodID)
                .FirstOrDefault();

            creditMethodLoaded = true;
            return creditMethodId;
        }

        private bool IsLegacyCredit(int methodId)
        {
            var credit = CreditMethodId();
            return credit > 0 && methodId == credit;
        }

        // ============================================================
        // lookups
        // ============================================================
        private IQueryable<CustomerBalance> LatestInvoices(ICollection<int> customerIds)
        {
            var scoped = invoiceScope(db.TblCustomerBalance.AsNoTracking())
                .Where(x => customerIds.Contains(x.CustomerID));

            var candidates = scoped.Where(x => x.InvoiceNo != null);
            if (checkType)
                candidates = candidates.Where(x => x.Type == SaleType);

            var latestIds = candidates
                .GroupBy(x => x.InvoiceNo)
                .Select(g => g.Max(x => x.CustomerBalanceID));

            return scoped.Where(i => latestIds.Contains(i.CustomerBalanceID));
        }

        private class ClassRow
        {
            public int ClassID { get; set; }
            public string ClassName { get; set; }
            public string PrefixCollection { get; set; }
            public int? StartCollectionNumber { get; set; }
        }

        private List<ClassRow> ClassRows()
        {
            try
            {
                return db.Database.SqlQuery<ClassRow>(ClassesScopedSql, SystemId).ToList();
            }
            catch
            {
                return db.Database.SqlQuery<ClassRow>(ClassesSql).ToList();
            }
        }

        // The dropdown never shows the legacy credit method. includeLegacy is for labels only.
        private List<LookupItem> MethodList(bool includeLegacy = false)
        {
            var systemId = SystemId;

            var query = db.TblPaymentMethod.AsNoTracking()
                .Where(m => m.SystemID == systemId);

            if (!includeLegacy)
                query = query.Where(m => m.Method != CreditMethodName);

            return query
                .OrderBy(m => m.Method)
                .ToList()
                .Select(m => new LookupItem
                {
                    value = m.PaymentMethodID.ToString(),
                    label = m.Method
                })
                .ToList();
        }

        private List<DepositItem> DepositList()
        {
            var rows = db.Database.SqlQuery<AccountRow>(DepositAccountsSql, SystemId).ToList();
            var byId = rows.GroupBy(r => r.id).ToDictionary(g => g.Key, g => g.First());

            var kids = byId.Values
                .GroupBy(r => (r.parentId.HasValue
                               && r.parentId.Value != r.id
                               && byId.ContainsKey(r.parentId.Value)) ? r.parentId.Value : 0)
                .ToDictionary(g => g.Key,
                              g => g.OrderBy(x => x.accountNo, StringComparer.OrdinalIgnoreCase)
                                    .ThenBy(x => x.name, StringComparer.OrdinalIgnoreCase).ToList());

            var result = new List<DepositItem>();
            var seen = new HashSet<int>();

            Action<int, int> walk = null;
            walk = (parent, level) =>
            {
                List<AccountRow> list;
                if (!kids.TryGetValue(parent, out list)) return;
                foreach (var a in list)
                {
                    if (!seen.Add(a.id)) continue;
                    result.Add(new DepositItem
                    {
                        value = a.id.ToString(),
                        label = string.IsNullOrWhiteSpace(a.accountNo) ? a.name : a.accountNo + " · " + a.name,
                        type = a.type ?? "",
                        level = level
                    });
                    walk(a.id, level + 1);
                }
            };
            walk(0, 0);
            return result;
        }

        public object GetLookups()
        {
            return new
            {
                methods = MethodList(),
                allMethods = MethodList(true), // includes the legacy method, for list labels
                creditMethodId = CreditMethodId() > 0 ? (int?)CreditMethodId() : null,

                classes = ClassRows()
                    .Select(c => new LookupItem
                    {
                        value = c.ClassID.ToString(),
                        label = c.ClassName
                    })
                    .ToList(),

                currencies = Currencies,
                chargeTypes = ChargeTypes,
                deposits = DepositList(),
                salespeople = new List<LookupItem>(),
                nextNumber = NextNumber()
            };
        }

        // ============================================================
        // customers
        // ============================================================
        public List<CustomerDto> GetCustomers(string search = null, int take = 30)
        {
            var systemId = SystemId;
            take = take > 0 ? Math.Min(take, 30) : 30;

            var query = db.TblCustomer.AsNoTracking()
                .Where(c => c.SystemID == systemId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                int searchId;
                var isId = int.TryParse(search, out searchId);

                query = query.Where(c =>
                    c.CustomerCode.Contains(search) ||
                    c.CustomerName.Contains(search) ||
                    c.CustomerNameLatin.Contains(search) ||
                    (isId && c.CustomerID == searchId));
            }

            var balances = db.TblCustomerBalance
                .AsNoTracking()
                .Where(x =>
                    x.SystemID == systemId &&
                    x.InvoiceNo != null &&
                    x.StatusID == UnpaidStatus &&
                    x.Type == SaleType &&
                    x.CancelDate == null &&
                    x.DeleteDate == null);

            // customers with open invoices first, then the rest
            var customers = query
                .Where(c => balances.Any(b => b.CustomerID == c.CustomerID))
                .OrderByDescending(c => c.CustomerID)
                .Take(take)
                .Select(c => new { c.CustomerID, c.CustomerCode, c.CustomerName, c.CustomerNameLatin })
                .ToList();

            if (customers.Count < take)
            {
                var others = query
                    .Where(c => !balances.Any(b => b.CustomerID == c.CustomerID))
                    .OrderByDescending(c => c.CustomerID)
                    .Take(take - customers.Count)
                    .Select(c => new { c.CustomerID, c.CustomerCode, c.CustomerName, c.CustomerNameLatin })
                    .ToList();

                customers = customers.Concat(others).ToList();
            }

            var ids = customers.Select(c => c.CustomerID).ToList();

            var open = OpenRows(ids)
                .GroupBy(r => r.CustomerId)
                .ToDictionary(
                    g => g.Key,
                    g => new { Count = g.Count(), Balance = R2(g.Sum(r => r.Open)) });

            // credit = unapplied money on posted receipts - spent by old Customer Credit receipts
            var legacy = CreditMethodId();

            var created = db.TblCollection.AsNoTracking()
                .Where(c => c.Status == PostedStatus && ids.Contains(c.CustomerID))
                .GroupBy(c => c.CustomerID)
                .Select(g => new
                {
                    Id = g.Key,
                    Added = g.Sum(x => x.UnappliedAmount),
                    LegacyUsed = g.Sum(x => x.MethodID == legacy ? x.GrandTotal : 0m)
                })
                .ToList();

            var credit = created.ToDictionary(
                x => x.Id,
                x => Math.Max(0m, R2(x.Added - x.LegacyUsed)));

            return customers
                .Select(c =>
                {
                    var o = open.ContainsKey(c.CustomerID) ? open[c.CustomerID] : null;
                    decimal cr;
                    credit.TryGetValue(c.CustomerID, out cr);

                    return new CustomerDto
                    {
                        id = c.CustomerID,
                        code = c.CustomerCode,
                        name = CustomerService.GetCustomerNamePrefix(c.CustomerID),
                        latin = c.CustomerNameLatin,
                        count = o != null ? o.Count : 0,
                        balance = o != null ? o.Balance : 0m,
                        credit = cr
                    };
                })
                .ToList();
        }

        // ============================================================
        // open invoices
        // ============================================================
        private class OpenRow
        {
            public int? Id { get; set; }
            public string No { get; set; }
            public string ClassName { get; set; }
            public int CustomerId { get; set; }
            public DateTime? Date { get; set; }
            public decimal Total { get; set; }
            public decimal Open { get; set; }
        }

        // collectionId = a receipt being edited: its own allocations are added back.
        private List<OpenRow> OpenRows(ICollection<int> customerIds, int? collectionId = null)
        {
            var restoredAmounts = new Dictionary<string, decimal>();

            if (collectionId.HasValue)
            {
                restoredAmounts = db.TblCollectionDetail
                    .AsNoTracking()
                    .Where(d => d.CollectionID == collectionId.Value)
                    .GroupBy(d => d.InvoiceNumber)
                    .Select(g => new
                    {
                        InvoiceNo = g.Key,
                        Amount = g.Sum(x => x.Payment + x.Discount)
                    })
                    .ToList()
                    .ToDictionary(x => x.InvoiceNo, x => x.Amount);
            }

            var restoredIds = restoredAmounts.Keys.ToList();

            // unpaid invoices, plus the ones this receipt paid
            var invoices = LatestInvoices(customerIds)
                .Where(i => i.StatusID == UnpaidStatus || restoredIds.Contains(i.InvoiceNo))
                .OrderBy(i => i.InvoiceDate)
                .ThenBy(i => i.CustomerBalanceID)
                .Select(i => new
                {
                    i.BillingInvoiceID,
                    i.InvoiceNo,
                    i.CustomerID,
                    i.InvoiceDate,
                    i.ClassName,
                    Total = (decimal?)i.GrandTotal ?? 0m,
                    Open = (decimal?)i.Unpaid ?? 0m
                })
                .ToList();

            return invoices
                .Select(i =>
                {
                    decimal restored;
                    restoredAmounts.TryGetValue(i.InvoiceNo, out restored);

                    return new OpenRow
                    {
                        Id = i.BillingInvoiceID,
                        No = i.InvoiceNo,
                        CustomerId = i.CustomerID,
                        Date = i.InvoiceDate,
                        Total = i.Total,
                        ClassName = i.ClassName,
                        Open = R2(i.Open + restored)
                    };
                })
                .Where(r => r.Open > Epsilon)
                .ToList();
        }

        public List<OpenInvoiceDto> GetOpenInvoices(int customerId, int? collectionId = null)
        {
            return OpenRows(new[] { customerId }, collectionId)
                .Select(r => new OpenInvoiceDto
                {
                    id = r.Id,
                    no = r.No,
                    type = "Invoice",
                    kind = "inv",
                    className = r.ClassName,
                    date = Iso(r.Date ?? DateTime.Today),
                    total = r.Total,
                    open = r.Open
                })
                .ToList();
        }

        // ============================================================
        // customer credit
        // ============================================================

        // Credit receipts that still have a balance.
        // excludeCollectionId = the receipt being edited, so the credit it used is available to it again.
        public List<CreditDto> GetCredits(int customerId, int? excludeCollectionId = null)
        {
            var posted = db.TblCollection.AsNoTracking()
                .Where(c => c.Status == PostedStatus && c.CustomerID == customerId);

            if (excludeCollectionId.HasValue)
                posted = posted.Where(c => c.CollectionID != excludeCollectionId.Value);

            var sources = posted
                .Where(c => c.UnappliedAmount > 0)
                .OrderBy(c => c.CollectionDate).ThenBy(c => c.CollectionID)
                .ToList();

            var result = new List<CreditDto>();
            if (sources.Count == 0)
                return result;

            // credit spent by old Customer Credit receipts: taken from the oldest RV first
            var legacyId = CreditMethodId();
            var legacyUsed = legacyId > 0
                ? posted.Where(c => c.MethodID == legacyId).Sum(c => (decimal?)c.GrandTotal) ?? 0m
                : 0m;

            var methods = MethodList(true).ToDictionary(m => m.value, m => m.label);
            var deposits = DepositList().GroupBy(d => d.value).ToDictionary(g => g.Key, g => g.First().label);

            foreach (var s in sources)
            {
                var balance = R2(s.UnappliedAmount);

                var legacy = Math.Min(legacyUsed, Math.Max(0m, balance));
                legacyUsed = R2(legacyUsed - legacy);
                balance = R2(balance - legacy);
                if (balance <= Epsilon) continue;

                string method, deposit;
                methods.TryGetValue(s.MethodID.ToString(), out method);
                deposits.TryGetValue(s.DepositAccountID.ToString(), out deposit);

                result.Add(new CreditDto
                {
                    id = s.CollectionID,
                    no = s.Number,
                    date = Iso(s.CollectionDate),
                    methodId = s.MethodID.ToString(),
                    method = method ?? s.MethodID.ToString(),
                    depositId = s.DepositAccountID > 0 ? s.DepositAccountID.ToString() : null,
                    deposit = deposit ?? "—",
                    original = s.UnappliedAmount,
                    balance = balance
                });
            }

            return result;
        }

        // ============================================================
        // list + details
        // ============================================================
        public List<object> GetList(int take = 200)
        {
            var rows =
                (from c in db.TblCollection.AsNoTracking()
                 join cu in db.TblCustomer.AsNoTracking()
                     on c.CustomerID equals cu.CustomerID
                 where c.Status == PostedStatus
                 select new
                 {
                     c.CustomerID,
                     c.CollectionID,
                     c.Number,
                     c.CollectionDate,
                     c.MethodID,
                     c.DepositAccountID,
                     c.Currency,
                     c.GrandTotal,
                     c.UnappliedAmount,
                     c.AppliedAmount
                 })
                .Take(take)
                .ToList();

            return rows
                .OrderBy(r => r.Number)
                .Select(c => (object)new
                {
                    id = c.CollectionID,
                    no = c.Number,
                    date = Iso(c.CollectionDate),
                    customer = CustomerService.GetCustomerNamePrefix(c.CustomerID),
                    method = c.MethodID.ToString(),
                    deposit = IsLegacyCredit(c.MethodID) ? "" : c.DepositAccountID.ToString(),
                    sym = Symbol(c.Currency),
                    grand = c.GrandTotal,
                    credit = c.UnappliedAmount,
                    // nothing applied yet = the receipt is still a deposit
                    type = (c.AppliedAmount <= 0 && c.UnappliedAmount > 0) ? "Deposit" : "Receipt"
                })
                .ToList();
        }

        public object GetDetails(int id)
        {
            var head = db.TblCollection.AsNoTracking()
                .FirstOrDefault(c => c.CollectionID == id);

            if (head == null)
                throw new CollectionException("Collection not found.");

            var legacy = IsLegacyCredit(head.MethodID);

            var method = MethodList(true)
                .FirstOrDefault(m => m.value == head.MethodID.ToString());

            var depositId = head.DepositAccountID;

            var deposit = legacy ? "" :
                db.TblChartOfAccount.AsNoTracking()
                    .Where(a => a.ChartOfAccountID == depositId)
                    .Select(a => a.AccountName)
                    .FirstOrDefault();

            var details = db.TblCollectionDetail.AsNoTracking()
                .Where(d => d.CollectionID == id)
                .OrderBy(d => d.InvoiceNumber)
                .ToList();

            var invoiceNos = details.Select(d => d.InvoiceNumber).ToList();

            var invoiceDates = db.TblCustomerBalance.AsNoTracking()
                .Where(i => i.InvoiceNo != null && invoiceNos.Contains(i.InvoiceNo))
                .GroupBy(i => i.InvoiceNo)
                .Select(g => new { No = g.Key, Date = g.Max(x => x.InvoiceDate) })
                .ToList()
                .ToDictionary(x => x.No, x => x.Date);

            var lines = details.Select(d =>
            {
                DateTime? when;
                invoiceDates.TryGetValue(d.InvoiceNumber, out when);

                return new
                {
                    invoiceId = d.InvoiceID,
                    no = d.InvoiceNumber,
                    type = "Invoice",
                    date = Iso(when ?? head.CollectionDate),
                    open = d.OpenBalance,
                    disc = d.Discount,
                    pay = d.Payment
                };
            }).ToList();

            var charges = db.TblCollectionCharge.AsNoTracking()
                .Where(c => c.CollectionID == id)
                .ToList()
                .Select(c => new { type = c.ChargeType, note = c.Note, amount = c.Amount })
                .ToList();

            var history = db.TblCollectionHistory.AsNoTracking()
                .Where(h => h.CollectionID == id)
                .OrderBy(h => h.HistoryID)
                .ToList()
                .Select(h => new
                {
                    date = Iso(h.EventDate),
                    type = h.EventType,
                    invoiceNo = h.InvoiceNo,
                    amount = h.Amount,
                    balance = h.DepositAfter,
                    note = h.Note,
                    by = h.CreatedBy
                })
                .ToList();

            var customerCode = db.TblCustomer.AsNoTracking()
                .Where(c => c.CustomerID == head.CustomerID && c.SystemID == SystemId)
                .Select(c => c.CustomerCode)
                .FirstOrDefault();

            return new
            {
                id = head.CollectionID,
                no = head.Number,
                date = Iso(head.CollectionDate),
                customerId = head.CustomerID,
                classId = head.ClassID,
                methodId = head.MethodID,
                depositId = legacy ? (int?)null : depositId,
                currency = head.Currency,
                amountPaid = head.AmountPaid,
                code = customerCode ?? "",
                customer = CustomerService.GetCustomerNamePrefix(head.CustomerID),
                method = method != null ? method.label : head.MethodID.ToString(),
                deposit = string.IsNullOrEmpty(deposit) ? "—" : deposit,
                memo = head.Memo,
                sym = Symbol(head.Currency),
                grand = head.GrandTotal,
                credit = head.UnappliedAmount,
                applied = head.AppliedAmount,
                status = head.Status,
                type = (head.AppliedAmount <= 0 && head.UnappliedAmount > 0) ? "Deposit" : "Receipt",
                lines,
                charges,
                history
            };
        }

        // ============================================================
        // receipt numbering
        // ============================================================
        public object GetNextNumber(int classId)
        {
            if (classId <= 0)
                throw new CollectionException("Invalid class.");

            if (!ClassRows().Any(c => c.ClassID == classId))
                throw new CollectionException("Class not found.");

            return new { nextNumber = NextNumber(classId) };
        }

        private static string ApplyDate(string text, DateTime date)
        {
            return text
                .Replace("{yyyy}", date.ToString("yyyy", CultureInfo.InvariantCulture))
                .Replace("{yy}", date.ToString("yy", CultureInfo.InvariantCulture))
                .Replace("{mm}", date.ToString("MM", CultureInfo.InvariantCulture))
                .Replace("{dd}", date.ToString("dd", CultureInfo.InvariantCulture));
        }

        public string NextNumber(int? classId = null)
        {
            ClassRow cls = null;
            if (classId.HasValue && classId.Value > 0)
                cls = ClassRows().FirstOrDefault(c => c.ClassID == classId.Value);

            var pattern = cls != null ? (cls.PrefixCollection ?? "").Trim() : "";
            if (!SequenceToken.IsMatch(pattern))
                pattern = Prefix + "{000000}";

            var start = cls != null && cls.StartCollectionNumber.HasValue && cls.StartCollectionNumber.Value > 0
                ? cls.StartCollectionNumber.Value
                : 1;

            var token = SequenceToken.Match(pattern);
            var width = token.Groups[1].Length;
            var today = DateTime.Today;
            var head = ApplyDate(pattern.Substring(0, token.Index), today);
            var tail = ApplyDate(pattern.Substring(token.Index + token.Length), today);

            return head
                + NextSequence(pattern, start, today).ToString(new string('0', width), CultureInfo.InvariantCulture)
                + tail;
        }

        private int NextSequence(string pattern, int start, DateTime today)
        {
            var token = SequenceToken.Match(pattern);
            if (!token.Success) return start;

            var head = ApplyDate(pattern.Substring(0, token.Index), today);

            var numbers = db.TblCollection.AsNoTracking()
                .Where(c => c.Number.StartsWith(head))
                .Select(c => new { c.Number, c.Status })
                .ToList();

            // nothing issued yet for this prefix: use the configured start
            if (numbers.Count == 0)
                return start;

            // only active receipts occupy a number
            var used = new HashSet<int>();
            foreach (var receipt in numbers)
            {
                if (receipt.Status == RollbackStatus)
                    continue;

                var n = SequenceOf(receipt.Number, pattern, today);
                if (n.HasValue)
                    used.Add(n.Value);
            }

            // first free number, starting from 1
            var max = used.Count > 0 ? used.Max() : 0;
            for (var n = 1; n <= max; n++)
            {
                if (!used.Contains(n))
                    return n;
            }

            return max + 1;
        }

        private void SyncCounter(int? classId)
        {
            if (!classId.HasValue || classId.Value <= 0)
                return;

            var cls = db.TblClass.FirstOrDefault(c => c.ClassID == classId.Value);
            if (cls == null)
                return;

            var pattern = (cls.PrefixCollection ?? "").Trim();
            if (!SequenceToken.IsMatch(pattern))
                return;

            var start = Math.Max(1, cls.StartCollectionNumber ?? 1);
            cls.StartCollectionNumber = NextSequence(pattern, start, DateTime.Today);
        }

        private static int? SequenceOf(string number, string pattern, DateTime date)
        {
            var token = SequenceToken.Match(pattern ?? "");
            if (!token.Success || string.IsNullOrEmpty(number)) return null;

            var head = ApplyDate(pattern.Substring(0, token.Index), date);
            var tail = ApplyDate(pattern.Substring(token.Index + token.Length), date);

            if (number.Length <= head.Length + tail.Length ||
                !number.StartsWith(head, StringComparison.Ordinal) ||
                !number.EndsWith(tail, StringComparison.Ordinal))
                return null;

            var middle = number.Substring(head.Length, number.Length - head.Length - tail.Length);
            int n;
            return int.TryParse(middle, NumberStyles.None, CultureInfo.InvariantCulture, out n) ? (int?)n : null;
        }

        // ============================================================
        // validate
        // ============================================================
        private void Validate(
            CollectionSaveRequest r,
            out DateTime date,
            out List<CollectionLineDto> lines,
            out List<CollectionChargeDto> charges)
        {
            if (r == null) throw new CollectionException("Invalid request.");

            if (!DateTime.TryParseExact(r.DateIso, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                throw new CollectionException("Enter a valid date.");
            if (r.CustomerId <= 0)
                throw new CollectionException("Select a customer to collect from.");
            if (!MethodList().Any(m => m.value == r.MethodId)) // legacy credit method is rejected here
                throw new CollectionException("Select a payment method.");
            if (r.DepositId <= 0)
                throw new CollectionException("Choose the account this money goes into.");
            if (!Currencies.Any(c => c.value == r.Currency))
                throw new CollectionException("Unknown currency.");
            if (r.ClassId == null || r.ClassId <= 0)
                throw new CollectionException("Select a class.");
            if (!ClassRows().Any(c => c.ClassID == r.ClassId.Value))
                throw new CollectionException("Unknown class.");

            var allLines = r.Lines ?? new List<CollectionLineDto>();
            var allCharges = r.Charges ?? new List<CollectionChargeDto>();

            if (allLines.Any(l => l.Payment < 0 || l.Discount < 0) || allCharges.Any(c => c.Amount < 0))
                throw new CollectionException("Amounts cannot be negative.");

            lines = allLines.Where(l => l.Payment > 0).ToList();
            charges = allCharges.Where(c => c.Amount > 0).ToList();

            if (lines.Any(l => l.Discount > 0))
                throw new CollectionException("Discount is not supported when posting through the payment procedure. Clear the discount and try again.");

            if (lines.GroupBy(l => l.InvoiceNo).Any(g => g.Count() > 1))
                throw new CollectionException("An invoice appears twice.");

            if (lines.Sum(l => l.Payment) + charges.Sum(c => c.Amount) <= 0 && r.AmountPaid <= 0)
                throw new CollectionException("Select invoices to collect or enter an amount paid.");
        }

        // ============================================================
        // save
        // ============================================================
        public async Task<SaveResult> SaveAsync(CollectionSaveRequest r, string user)
        {
            DateTime date;
            List<CollectionLineDto> lines;
            List<CollectionChargeDto> charges;
            Validate(r, out date, out lines, out charges);

            decimal applied = R2(lines.Sum(l => l.Payment));
            decimal discount = R2(lines.Sum(l => l.Discount));
            decimal otherCharge = R2(charges.Sum(c => c.Amount));
            decimal settle = R2(applied + otherCharge);

            // Amount paid includes any credit applied. Anything above the invoices becomes new credit.
            decimal received = Math.Max(R2(r.AmountPaid), settle);
            decimal credit = R2(received - settle);

            SaveResult saved;
            List<string> invoiceNos;

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    if (!db.TblCustomer.Any(c => c.CustomerID == r.CustomerId))
                        throw new CollectionException("Customer not found.");

                    var details = ApplyToInvoices(r, date, lines, user);

                    var number = (r.Number ?? "").Trim();
                    if (number.Length == 0 || db.TblCollection.Any(c => c.Number == number && c.Status == PostedStatus))
                        number = NextNumber(r.ClassId);

                    var head = new Collection
                    {
                        Number = number,
                        CollectionDate = date,
                        CustomerID = r.CustomerId,
                        ClassID = r.ClassId,
                        MethodID = int.Parse(r.MethodId, CultureInfo.InvariantCulture),
                        DepositAccountID = r.DepositId,
                        Currency = r.Currency,
                        Memo = r.Memo,
                        AmountPaid = received,
                        AppliedAmount = applied,
                        DiscountAmount = discount,
                        OtherCharge = otherCharge,
                        GrandTotal = settle,
                        UnappliedAmount = credit,
                        Status = PostedStatus,
                        CreatedBy = user,
                        CreatedAt = DateTime.Now
                    };
                    db.TblCollection.Add(head);
                    db.SaveChanges();

                    foreach (var d in details)
                    {
                        d.CollectionID = head.CollectionID;
                        db.TblCollectionDetail.Add(d);
                    }

                    foreach (var c in charges)
                    {
                        db.TblCollectionCharge.Add(new CollectionCharge
                        {
                            CollectionID = head.CollectionID,
                            CustomerID = head.CustomerID,
                            ChargeType = c.Type,
                            Note = c.Note,
                            Amount = c.Amount
                        });
                    }

                    LogCreated(head, details, user);

                    db.SaveChanges();            // rows saved, so the counter can see this receipt
                    SyncCounter(r.ClassId);
                    db.SaveChanges();

                    tx.Commit();

                    saved = new SaveResult
                    {
                        id = head.CollectionID,
                        customerId = head.CustomerID,
                        number = head.Number,
                        unapplied = credit,
                        warnings = new List<string>()
                    };
                    invoiceNos = details.Select(d => d.InvoiceNumber).Distinct().ToList();
                }
                catch
                {
                    TryRollback(tx);
                    throw;
                }
            }

            await ReactivateServices(r.CustomerId, invoiceNos, saved.warnings);

            return saved;
        }

        // ============================================================
        // history (TblCollectionHistory)
        // ============================================================
        private void Log(Collection head, string type, string invoiceNo, decimal amount, string note, string user)
        {
            db.TblCollectionHistory.Add(new CollectionHistory
            {
                CollectionID = head.CollectionID,
                CustomerID = head.CustomerID,
                EventDate = head.CollectionDate,
                EventType = type,
                InvoiceNo = invoiceNo,
                Amount = amount,
                DepositAfter = head.UnappliedAmount,
                Note = note,
                CreatedBy = user,
                CreatedAt = DateTime.Now
            });
        }

        // new receipt: a deposit (nothing applied) or the invoices it paid
        private void LogCreated(Collection head, List<CollectionDetail> details, string user)
        {
            if (head.AppliedAmount <= 0 && head.UnappliedAmount > 0)
            {
                Log(head, "Deposit", null, head.UnappliedAmount, "Deposit received", user);
                return;
            }

            foreach (var d in details)
                Log(head, "Payment", d.InvoiceNumber, d.Payment, "Collected from invoice", user);

            if (head.UnappliedAmount > 0)
                Log(head, "Deposit", null, head.UnappliedAmount, "Kept as deposit", user);
        }

        // edit / pay invoices from a deposit: log only what changed
        private void LogChanged(Collection head, Dictionary<string, decimal> before, decimal depositBefore,
            List<CollectionDetail> after, string user)
        {
            var now = after.GroupBy(x => x.InvoiceNumber).ToDictionary(g => g.Key, g => g.Sum(x => x.Payment));

            foreach (var kv in now)
            {
                decimal old;
                before.TryGetValue(kv.Key, out old);
                var delta = R2(kv.Value - old);
                if (delta > Epsilon)
                    Log(head, "Payment", kv.Key, delta, old > 0 ? "Invoice payment increased" : "Collected from invoice", user);
                else if (delta < -Epsilon)
                    Log(head, "Reversal", kv.Key, -delta, "Invoice payment reduced", user);
            }

            foreach (var kv in before)
            {
                if (!now.ContainsKey(kv.Key) && kv.Value > Epsilon)
                    Log(head, "Reversal", kv.Key, kv.Value, "Invoice payment removed", user);
            }

            // still only a deposit, but the amount was changed
            if (head.AppliedAmount <= 0 && R2(head.UnappliedAmount - depositBefore) != 0m)
                Log(head, "Deposit", null, R2(head.UnappliedAmount - depositBefore), "Deposit changed", user);
        }

        // re-activate locked/expired services of the paid invoices
        private async Task ReactivateServices(int customerId, List<string> invoiceNos, List<string> warnings)
        {
            if (invoiceNos == null || invoiceNos.Count == 0)
                return;

            try
            {
                using (var ctx = new DBContext())
                {
                    var sids = await ctx.TblCustomerBillingInvoice.AsNoTracking()
                        .Where(i => i.CustomerID == customerId
                                 && invoiceNos.Contains(i.InvoiceNo)
                                 && i.SID != null && i.SID != "")
                        .Select(i => i.SID)
                        .Distinct()
                        .ToListAsync();

                    foreach (var sid in sids)
                    {
                        var activation = await ctx.TblActivateCustomer
                            .FirstOrDefaultAsync(a => a.CustomerID == customerId && a.SID == sid);

                        if (activation == null) continue;

                        if (activation.ConnectivityStatus != ConnectivityStatusCodes.Locked &&
                            activation.ConnectivityStatus != ConnectivityStatusCodes.Expired)
                            continue;

                        try
                        {
                            string comment = "Re-activate connection due to customer has been paid.";
                            ActivationService.Activate(activation, comment);

                            activation.Comment = comment;
                            activation.ConnectivityStatus = ConnectivityStatusCodes.Active;
                            activation.PaymentStatus = 10;
                            activation.PaidDate = DateTime.Now;
                            await ctx.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            warnings.Add("SID " + sid + ": " + ex.GetBaseException().Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                warnings.Add("Device check failed: " + ex.GetBaseException().Message);
            }
        }

        // ============================================================
        // update (edit a posted receipt, or pay invoices from a deposit)
        // ============================================================
        public async Task<SaveResult> UpdateAsync(int id, CollectionSaveRequest r, string user)
        {
            DateTime date;
            List<CollectionLineDto> lines;
            List<CollectionChargeDto> charges;
            Validate(r, out date, out lines, out charges);

            SaveResult saved;
            List<string> invoiceNos;

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var head = db.TblCollection.FirstOrDefault(x => x.CollectionID == id);

                    if (head == null)
                        throw new CollectionException("Collection not found.");
                    if (head.Status != PostedStatus)
                        throw new CollectionException("Only posted collections can be edited.");
                    if (IsLegacyCredit(head.MethodID))
                        throw new CollectionException("This receipt used the old Customer Credit method. Roll it back and create a new one.");
                    if (head.ClassID != r.ClassId)
                        throw new CollectionException("Collection class cannot be changed when editing.");
                    if (!db.TblCustomer.Any(c => c.CustomerID == r.CustomerId))
                        throw new CollectionException("Customer not found.");

                    var oldClassId = head.ClassID;
                    var oldDetails = db.TblCollectionDetail.Where(x => x.CollectionID == id).ToList();
                    var oldPaid = oldDetails.GroupBy(x => x.InvoiceNumber).ToDictionary(g => g.Key, g => g.Sum(x => x.Payment));
                    var oldDeposit = head.UnappliedAmount;

                    // reverse the original invoice payments
                    var oldRequest = new CollectionSaveRequest
                    {
                        CustomerId = head.CustomerID,
                        DepositId = head.DepositAccountID,
                        Memo = "Edit " + head.Number
                    };

                    ReverseInvoices(oldDetails, oldRequest, head, user);

                    // remove old lines, charges and credit uses
                    db.TblCollectionDetail.RemoveRange(oldDetails);
                    db.TblCollectionCharge.RemoveRange(db.TblCollectionCharge.Where(x => x.CollectionID == id));
                    db.SaveChanges();

                    decimal applied = R2(lines.Sum(x => x.Payment));
                    decimal discount = R2(lines.Sum(x => x.Discount));
                    decimal otherCharge = R2(charges.Sum(x => x.Amount));
                    decimal settle = R2(applied + otherCharge);
                    decimal received = Math.Max(R2(r.AmountPaid), settle);
                    decimal credit = R2(received - settle);

                    var newDetails = ApplyToInvoices(r, date, lines, user);

                    head.CollectionDate = date;
                    head.CustomerID = r.CustomerId;
                    head.MethodID = int.Parse(r.MethodId, CultureInfo.InvariantCulture);
                    head.DepositAccountID = r.DepositId;
                    head.Currency = r.Currency;
                    head.Memo = r.Memo;
                    head.AmountPaid = received;
                    head.AppliedAmount = applied;
                    head.DiscountAmount = discount;
                    head.OtherCharge = otherCharge;
                    head.GrandTotal = settle;
                    head.UnappliedAmount = credit;
                    // kept: CollectionID, Number, CreatedBy, CreatedAt, Status

                    foreach (var d in newDetails)
                    {
                        d.CollectionID = id;
                        db.TblCollectionDetail.Add(d);
                    }

                    foreach (var c in charges)
                    {
                        db.TblCollectionCharge.Add(new CollectionCharge
                        {
                            CollectionID = id,
                            CustomerID = r.CustomerId,
                            ChargeType = c.Type,
                            Note = c.Note,
                            Amount = c.Amount
                        });
                    }

                    LogChanged(head, oldPaid, oldDeposit, newDetails, user);

                    db.SaveChanges();
                    SyncCounter(oldClassId);
                    db.SaveChanges();

                    tx.Commit();

                    saved = new SaveResult
                    {
                        id = head.CollectionID,
                        customerId = head.CustomerID,
                        number = head.Number,
                        unapplied = credit,
                        warnings = new List<string>()
                    };
                    invoiceNos = newDetails.Select(d => d.InvoiceNumber).Distinct().ToList();
                }
                catch
                {
                    TryRollback(tx);
                    throw;
                }
            }

            await ReactivateServices(r.CustomerId, invoiceNos, saved.warnings);
            return saved;
        }

        // ============================================================
        // rollback
        // ============================================================
        public void Rollback(int id, string user)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var head = db.TblCollection.FirstOrDefault(x => x.CollectionID == id);
                    if (head == null) throw new CollectionException("Collection not found.");
                    if (head.Status == RollbackStatus)
                        throw new CollectionException("Collection has already been rolled back.");

                    var lines = db.TblCollectionDetail.Where(x => x.CollectionID == id).ToList();

                    var req = new CollectionSaveRequest
                    {
                        CustomerId = head.CustomerID,
                        DepositId = head.DepositAccountID,
                        Memo = "Rollback " + head.Number
                    };

                    ReverseInvoices(lines, req, head, user);

                    // Credit this receipt used goes back to its source RVs automatically:
                    // only POSTED receipts count as uses, and this one is no longer posted.
                    foreach (var l in lines)
                        Log(head, "Reversal", l.InvoiceNumber, l.Payment, "Receipt rolled back", user);
                    Log(head, "Rollback", null, head.AmountPaid, "Receipt rolled back", user);

                    head.Status = RollbackStatus;
                    db.SaveChanges();            // the number is free now, so the counter can see it
                    SyncCounter(head.ClassID);
                    db.SaveChanges();

                    tx.Commit();
                }
                catch
                {
                    TryRollback(tx);
                    throw;
                }
            }
        }

        // Re-opens every invoice a receipt paid (shared by edit and rollback).
        private void ReverseInvoices(List<CollectionDetail> details, CollectionSaveRequest req, Collection head, string user)
        {
            var customerName = CustomerService.GetCustomerNamePrefix(head.CustomerID);

            foreach (var d in details)
            {
                db.Database.SqlQuery<int>(LockInvoiceSql, d.InvoiceID).ToList();

                if (d.PaymentID != null)
                {
                    var xml = BuildInfo(req, head.CollectionDate, user, customerName, head.MethodID.ToString(),
                        d.InvoiceNumber, 0m, UnpaidStatus, d.PaymentID,
                        BillingInvoiceId(head.CustomerID, d.InvoiceNumber));

                    ExecutePayment(xml, d.InvoiceNumber);
                }
                else if (d.PrevStatusID != null)
                {
                    var invoice = db.TblCustomerBalance.Find(d.InvoiceID);
                    if (invoice != null && invoice.StatusID == PaidStatus)
                        invoice.StatusID = d.PrevStatusID.Value;
                }
            }
        }

        // ============================================================
        // posting through SP_INSERT_PAYMENT
        // ============================================================
        public class PaymentResult
        {
            public int? PaymentID { get; set; }
            public string OfficialReceiptNo { get; set; }
            public int? CompanyID { get; set; }
            public string InvoiceNo { get; set; }
            public string CustomerType { get; set; }
        }

        private List<CollectionDetail> ApplyToInvoices(
            CollectionSaveRequest r,
            DateTime date,
            List<CollectionLineDto> lines,
            string user)
        {
            var details = new List<CollectionDetail>();

            if (lines == null || lines.Count == 0)
                return details;

            if (lines.Any(l => l == null || string.IsNullOrWhiteSpace(l.InvoiceNo)))
                throw new CollectionException("Invalid invoice selection. Please select an invoice and try again.");

            var duplicate = lines
                .GroupBy(l => l.InvoiceNo.Trim(), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate != null)
                throw new CollectionException("Invoice " + duplicate.Key + " is selected more than once.");

            // reload what is open right now
            var openRows = OpenRows(new[] { r.CustomerId });

            var duplicateOpen = openRows
                .GroupBy(x => x.No, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateOpen != null)
                throw new CollectionException("Duplicate invoice number found: " + duplicateOpen.Key);

            var openNow = openRows
                .Where(x => !string.IsNullOrWhiteSpace(x.No))
                .ToDictionary(x => x.No, StringComparer.OrdinalIgnoreCase);

            var customerName = CustomerService.GetCustomerNamePrefix(r.CustomerId);

            foreach (var l in lines)
            {
                var invoiceNo = l.InvoiceNo.Trim();

                OpenRow open;
                if (!openNow.TryGetValue(invoiceNo, out open))
                    throw new CollectionException(
                        "Invoice " + invoiceNo + " is not open for this customer (already paid or changed). Reload and try again.");

                var pay = R2(l.Payment);
                var discount = R2(l.Discount);

                if (pay + discount > open.Open + Epsilon)
                    throw new CollectionException(
                        open.No + " open balance is now " + open.Open.ToString("N2") + ". Reload and try again.");

                var xml = BuildInfo(r, date, user, customerName, r.MethodId,
                    open.No, pay, PaidStatus, null, BillingInvoiceId(r.CustomerId, open.No));

                var result = ExecutePayment(xml, open.No);

                details.Add(new CollectionDetail
                {
                    InvoiceID = open.Id,
                    CustomerID = r.CustomerId,
                    InvoiceNumber = open.No,
                    OpenBalance = open.Open,
                    Discount = discount,
                    Payment = pay,
                    PrevStatusID = UnpaidStatus,
                    PaymentID = result.PaymentID,
                    ReceiptNo = result.OfficialReceiptNo
                });
            }

            return details;
        }

        private static void Add(XElement parent, string name, object value)
        {
            if (value == null) return;
            var text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(text)) return;
            parent.Add(new XElement(name, text));
        }

        private string BuildInfo(
            CollectionSaveRequest r, DateTime date, string user, string customerName, string methodId,
            string invoiceNo, decimal amount, int statusId, int? paymentId, int? billingInvoiceId)
        {
            // old Customer Credit receipts have no deposit account
            int? account = r.DepositId > 0 ? r.DepositId : (int?)null;

            var p = new XElement("PaymentInfo");
            Add(p, "PaymentID", paymentId);
            Add(p, "Category", PaymentCategory);
            Add(p, "SystemID", SystemId);
            Add(p, "WarehouseID", WarehouseId);
            Add(p, "CustomerID", r.CustomerId);
            Add(p, "InvoiceNo", invoiceNo);
            Add(p, "BillingInvoiceID", billingInvoiceId);
            Add(p, "StatusID", statusId);
            Add(p, "PaymentDate", date.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
            Add(p, "PaymentAmount", amount);
            Add(p, "PaymentMenthod", methodId);
            Add(p, "ToAccountNumber", account);
            Add(p, "ChartOfAccountID", account);
            Add(p, "Payer", customerName);
            Add(p, "ReceivedFromName", customerName);
            Add(p, "PaidBy", user);
            Add(p, "PaidByUserID", PaidByUserId);
            Add(p, "PaymentDescription", r.Memo);

            return new XElement("Data", p).ToString(SaveOptions.DisableFormatting);
        }

        private PaymentResult ExecutePayment(string xml, string invoiceNo)
        {
            var param = new SqlParameter("@Data", SqlDbType.Xml) { Value = xml };

            List<PaymentResult> rows;
            try
            {
                rows = db.Database.SqlQuery<PaymentResult>("EXEC dbo.SP_INSERT_PAYMENT @Data", param).ToList();
            }
            catch (Exception ex)
            {
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                throw new CollectionException("Payment for " + invoiceNo + " failed: " + inner.Message);
            }

            var row = rows.FirstOrDefault();
            if (row == null || row.PaymentID == null)
                throw new CollectionException("Payment for " + invoiceNo + " did not return a payment id.");

            return row;
        }

        private int? BillingInvoiceId(int customerId, string invoiceNo)
        {
            try
            {
                return db.Database.SqlQuery<int?>(
                    @"SELECT TOP 1 BillingInvoiceID
                        FROM TblCustomerBillingInvoice
                       WHERE CustomerID = @p0 AND InvoiceNo = @p1 AND SystemID = @p2 AND WarehouseID = @p3",
                    customerId, invoiceNo, SystemId, WarehouseId).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        // ============================================================
        // slips + Telegram notice
        // ============================================================
        // Slip images are kept in one column, TblCollection.SlipPath (paths separated by ";").
        public void SaveSlips(int collectionId, IList<StoredSlip> slips)
        {
            if (slips == null || slips.Count == 0)
                return;

            var head = db.TblCollection.FirstOrDefault(c => c.CollectionID == collectionId);
            if (head == null)
                return;

            var added = slips.Select(s => (s.DestinationFolder ?? "").TrimEnd('/', '\\') + "/" + s.FileName);
            var all = new List<string>();
            if (!string.IsNullOrEmpty(head.SlipPath))
                all.Add(head.SlipPath);
            all.AddRange(added);

            head.SlipPath = string.Join(";", all);
            db.SaveChanges();
        }

        public string BuildNotice(int collectionId, string user)
        {
            var head = db.TblCollection.AsNoTracking().FirstOrDefault(c => c.CollectionID == collectionId);
            if (head == null) return "";

            var invoices = db.TblCollectionDetail.AsNoTracking()
                .Where(d => d.CollectionID == collectionId)
                .OrderBy(d => d.InvoiceNumber)
                .Select(d => d.InvoiceNumber)
                .ToList();

            var depositId = head.DepositAccountID;
            var deposit = IsLegacyCredit(head.MethodID)
                ? "Customer Credit"
                : db.TblChartOfAccount.AsNoTracking()
                    .Where(a => a.ChartOfAccountID == depositId)
                    .Select(a => a.AccountName)
                    .FirstOrDefault();

            // nothing applied yet = the receipt is a deposit
            var isDeposit = head.AppliedAmount <= 0 && head.UnappliedAmount > 0;

            var memo = (head.Memo ?? "").Trim();
            if (memo.Length > 300) memo = memo.Substring(0, 300) + "…";

            Func<string, string> e = System.Net.WebUtility.HtmlEncode;
            var sym = e(Symbol(head.Currency));

            var lines = new List<string>
            {
                isDeposit ? "<b>We received a deposit from</b>" : "<b>We received payment from</b>",
                "<b>Customer:</b> " + CustomerService.GetCustomerNamePrefix(head.CustomerID),
                "<b>Invoice No:</b> " + e(invoices.Count == 0 ? "—" : string.Join(", ", invoices)),
                "<b>Deposit To:</b> " + e(deposit ?? "—"),
                "<b>Amount:</b> " + sym + " " + head.AmountPaid.ToString("0.##", CultureInfo.InvariantCulture)
            };

            //if (isDeposit)
            //    lines.Insert(1, "<b>RV No:</b> " + e(head.Number ?? "—"));
            //else if (head.UnappliedAmount > 0)
            //    lines.Add("<b>Kept as deposit:</b> " + sym + " " + head.UnappliedAmount.ToString("0.##", CultureInfo.InvariantCulture));

            lines.Add("<b>Received By:</b> " + e(user ?? ""));
            lines.Add("<b>Memo:</b> " + e(memo.Length == 0 ? "—" : memo));

            return string.Join("\n", lines);
        }
    }
}