using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Vendor
{
    public class PayBillController : Controller
    {
        private static bool _statusColumnChecked = false;
        private void EnsureStatusColumn(HTML html)
        {
            if (_statusColumnChecked) return;
            try
            {
                string checkSql = @"
                    IF COL_LENGTH('dbo.TblVendorPayment', 'Status') IS NULL
                    BEGIN
                        ALTER TABLE dbo.TblVendorPayment ADD [Status] NVARCHAR(50) NULL CONSTRAINT DF_TblVendorPayment_Status DEFAULT 'Draft';
                        EXEC('UPDATE dbo.TblVendorPayment SET [Status] = ''Draft'' WHERE [Status] IS NULL;');
                    END
                    IF COL_LENGTH('dbo.TblVendorPayment', 'CheckedBy') IS NULL
                    BEGIN
                        ALTER TABLE dbo.TblVendorPayment ADD CheckedBy NVARCHAR(100) NULL, CheckedDate DATETIME NULL;
                    END
                    IF COL_LENGTH('dbo.TblVendorPayment', 'VerifiedBy') IS NULL
                    BEGIN
                        ALTER TABLE dbo.TblVendorPayment ADD VerifiedBy NVARCHAR(100) NULL, VerifiedDate DATETIME NULL;
                    END
                    IF COL_LENGTH('dbo.TblVendorPaymentDetail', 'ExpenseID') IS NULL
                    BEGIN
                        ALTER TABLE dbo.TblVendorPaymentDetail ADD ExpenseID INT NULL;
                    END
                    IF COL_LENGTH('dbo.TblVendorPaymentDetail', 'DepositAmount') IS NULL
                    BEGIN
                        ALTER TABLE dbo.TblVendorPaymentDetail ADD DepositAmount DECIMAL(18, 4) NULL;
                    END
                    IF COL_LENGTH('dbo.TblVendorPaymentDetail', 'DepositExpenseID') IS NULL
                    BEGIN
                        ALTER TABLE dbo.TblVendorPaymentDetail ADD DepositExpenseID INT NULL;
                    END";
                html.SaveChangeData(checkSql, false, false);
                _statusColumnChecked = true;
            }
            catch { }
        }

        [Authorize]
        public ActionResult Index(string view = "")
        {
            HTML html = new HTML();
            EnsureStatusColumn(html);
            string sysId = GetSystemID(html);
            string whId = GetWarehouseID(html);

            ViewBag.SystemID = sysId;
            ViewBag.WarehouseID = whId;

            if (html.HasPermission("Add Bills") || html.HasPermission("Pay Bills") || html.HasPermission("Pay Bill"))
                html.AllowAdd = true;
            else
                html.AllowAdd = true;

            if (html.HasPermission("Edit Bills") || html.HasPermission("Edit PayBill"))
                html.AllowEdit = true;
            else
                html.AllowEdit = true;

            if (html.HasPermission("Delete Bills") || html.HasPermission("Delete PayBill"))
                html.AllowDelete = true;
            else
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New";
            html.AddURL = "PayBill/Create?systemId=" + sysId + "&warehouseId=" + whId;
            html.TableName = "TblVendorPayment";
            html.PrimaryField = "VendorPaymentID";
            html.ShowPaging = false;

            // Date Range Filter (Identical to Bill)
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "vp.PaymentDate",
                DefaultValueDate = false
            });

            // Single search input for Payment No (normal, without radio buttons below)
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Payment No",
                SQL = "select distinct top 10 OfficalReceiptNo from dbo.TblVendorPayment where lower(OfficalReceiptNo) like @SearchText and SystemID = " + sysId + " and WarehouseID=" + whId + " order by OfficalReceiptNo desc",
                RadioValue = EncryAndDecry.Encryp("vp.OfficalReceiptNo"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });

            string sqlPayments = @"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY vp.PaymentDate DESC, vp.VendorPaymentID DESC) AS [RowNo],
                    vp.VendorPaymentID,
                    '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher(' + CONVERT(VARCHAR, vp.VendorPaymentID) + ', ''' + ISNULL(vp.OfficalReceiptNo, 'PV-' + CONVERT(VARCHAR, vp.VendorPaymentID)) + ''', ''' + ISNULL(vp.[Status], 'Draft') + ''')"">' + ISNULL(vp.OfficalReceiptNo, 'PV-' + CONVERT(VARCHAR, vp.VendorPaymentID)) + '</a>' AS [OfficalReceiptNo],
                    vp.PaymentDate,
                    ISNULL(vp.Payee, v.VendorName) AS [VendorName],
                    ISNULL(pm.PaymentMethod, vp.PaymentMode) AS [PaymentMethod],
                    ISNULL(c.AccountNo + ' - ' + c.AccountName, vp.PaymentAccount) AS [PaymentAccount],
                    ISNULL(vp.[Status], 'Draft') AS [Status],
                    vp.TotalPaymentAmount,
                    ISNULL(vp.PaidBy, '') AS [PaidBy],
                    ISNULL(vp.Reference, '') AS [Reference],
                    ISNULL(vp.Memo, '') AS [Memo]
                FROM dbo.TblVendorPayment vp
                LEFT JOIN dbo.TblVendor v ON v.VendorID = vp.VendorID
                LEFT JOIN dbo.TblPaymentMethod pm ON pm.PaymentMethodID = vp.PaymentModeID
                LEFT JOIN dbo.TblChartOfAccount c ON c.ChartOfAccountID = vp.ChartOfAccountID
                WHERE vp.SystemID = " + sysId + @" AND vp.WarehouseID = " + whId;

            html.GH.SQL = sqlPayments;
            html.OrderBy = "vp.PaymentDate DESC, vp.VendorPaymentID DESC";

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "RowNo", TextAlign = "center"},
                new GridField() { Header = "Payment Nº", FieldName = "OfficalReceiptNo", TextAlign = "center" },
                new GridField() { Header = "Payment Date", FieldName = "PaymentDate", TextAlign = "center", FormatDateTime = "dd-MM-yyyy"},
                new GridField() { Header = "Vendor / Payee", FieldName = "VendorName", TextAlign = "left"},
                new GridField() { Header = "Payment Method", FieldName = "PaymentMethod", TextAlign = "center"},
                new GridField() { Header = "Payment Account", FieldName = "PaymentAccount", TextAlign = "left"},
                new GridField() { Header = "Status", FieldName = "Status", TextAlign = "center"},
                new GridField() { Header = "Total Paid (USD)", FieldName = "TotalPaymentAmount", Format = "#,#0.00", TextAlign = "right", IsTotalFooter = true },
                new GridField() { Header = "Paid By", FieldName = "PaidBy", TextAlign = "left"},
                new GridField() { Header = "Reference", FieldName = "Reference", TextAlign = "left"},
                new GridField() { Header = "Memo", FieldName = "Memo", TextAlign = "left"},
                new GridField() 
                { 
                    Header = "Action", 
                    FieldName = "VendorPaymentID", 
                    TextAlign = "Center", 
                    Width = "100px", 
                    IsModifyField = true,
                    DeleteWithTableName = "TblVendorPayment, TblVendorPaymentDetail, TblAccount_Transaction, TblAccount_JournalEntries",
                    HideEditOnActionNameNotMatch = true,
                    ShowActionNameOnConditionField = "Status",
                    ShowActionNameOnConditionValue = "Draft,Rejected,Confirm Rejected"
                }
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View("~/Views/PayBill/Index.cshtml");
        }

       
        [HttpGet]
        public ActionResult Create(int? vendorId = null, string billId = "", int? paymentId = null, int? systemId = null, int? warehouseId = null, string PrimaryFieldValue = "")
        {
            HTML html = new HTML();
            string sysId = GetSystemID(html, systemId);
            string whId = GetWarehouseID(html, warehouseId);

            if ((!paymentId.HasValue || paymentId.Value <= 0) && !string.IsNullOrEmpty(PrimaryFieldValue) && int.TryParse(PrimaryFieldValue, out int parsedPid))
            {
                paymentId = parsedPid;
            }

            ViewBag.SystemID = sysId;
            ViewBag.WarehouseID = whId;
            ViewBag.PaymentID = paymentId ?? 0;
            ViewBag.CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.CurrentUserName = html.UserName;

            // If a billId or billNumber is provided, look it up to resolve vendor and bill details
            string selectedBillIdStr = "";
            string selectedBillNumberStr = "";
            if (!string.IsNullOrEmpty(billId))
            {
                string safeBillParam = billId.Trim().Replace("'", "''");
                string findBillSql = @"
                    SELECT TOP 1 BillID, VendorID, BillNumber, GrandTotal, AmountDue 
                    FROM (
                        SELECT BillID, VendorID, BillNumber, GrandTotal, ISNULL(NULLIF(AmountDue, 0), GrandTotal - ISNULL(AmountPaid, 0)) AS AmountDue, SystemID, WarehouseID
                        FROM dbo.TblBills
                        UNION ALL
                        SELECT 
                            e.ExpenseID AS BillID, 
                            COALESCE(NULLIF(e.PayeeID, 0), NULLIF(exp.PayeeID, 0), 0) AS VendorID, 
                            COALESCE(NULLIF(exp.ExpenseNo, ''), NULLIF(e.ExpenseNo, ''), NULLIF(exp.[No], ''), NULLIF(e.[No], ''), 'EXP-' + CONVERT(VARCHAR, e.ExpenseID)) AS BillNumber, 
                            COALESCE(NULLIF(e.TotalAmount, 0), exp.TotalAmount, 0) AS GrandTotal, 
                            ISNULL(NULLIF(e.BalanceAmount, 0), ISNULL(NULLIF(exp.BalanceAmount, 0), COALESCE(NULLIF(e.TotalAmount, 0), exp.TotalAmount, 0) - COALESCE(e.PaidAmount, exp.PaidAmount, 0))) AS AmountDue, 
                            e.SystemID, 
                            e.WarehouseID
                        FROM dbo.Expense_PaymentTransactions e
                        LEFT JOIN dbo.Expenses exp ON exp.ExpenseID = e.ExpenseID
                        WHERE (
                            e.Type IN ('Bill', 'Expense', 'Enter Bill') 
                            OR e.ExpenseTransactionType IN ('Bill', 'Expense', 'Enter Bill') 
                            OR exp.Type IN ('Bill', 'Expense', 'Enter Bill') 
                            OR exp.ExpenseTransactionType IN ('Bill', 'Expense', 'Enter Bill') 
                            OR ISNULL(e.Type, '') = '' 
                            OR e.ExpenseNo LIKE 'BL%' 
                            OR exp.ExpenseNo LIKE 'BL%'
                        )
                        UNION ALL
                        SELECT 
                            exp.ExpenseID AS BillID, 
                            ISNULL(exp.PayeeID, 0) AS VendorID, 
                            COALESCE(NULLIF(exp.ExpenseNo, ''), NULLIF(exp.[No], ''), 'EXP-' + CONVERT(VARCHAR, exp.ExpenseID)) AS BillNumber, 
                            ISNULL(exp.TotalAmount, 0) AS GrandTotal, 
                            ISNULL(NULLIF(exp.BalanceAmount, 0), ISNULL(exp.TotalAmount, 0) - ISNULL(exp.PaidAmount, 0)) AS AmountDue, 
                            exp.SystemID, 
                            exp.WarehouseID
                        FROM dbo.Expenses exp
                        WHERE (
                            exp.Type IN ('Bill', 'Expense', 'Enter Bill') 
                            OR exp.ExpenseTransactionType IN ('Bill', 'Expense', 'Enter Bill') 
                            OR ISNULL(exp.Type, '') = '' 
                            OR exp.ExpenseNo LIKE 'BL%'
                        )
                          AND NOT EXISTS (SELECT 1 FROM dbo.Expense_PaymentTransactions e2 WHERE e2.ExpenseID = exp.ExpenseID)
                    ) u
                    WHERE (u.BillNumber = '" + safeBillParam + @"' OR CONVERT(VARCHAR, u.BillID) = '" + safeBillParam + @"')";

                DataSet dsFind = html.LoadData(findBillSql);
                if (dsFind != null && dsFind.Tables.Count > 0 && dsFind.Tables[0].Rows.Count > 0)
                {
                    selectedBillIdStr = dsFind.Tables[0].Rows[0]["BillID"].ToString();
                    selectedBillNumberStr = dsFind.Tables[0].Rows[0]["BillNumber"].ToString();
                    int.TryParse(dsFind.Tables[0].Rows[0]["VendorID"].ToString(), out int foundVendorId);
                    if (foundVendorId > 0 && (!vendorId.HasValue || vendorId.Value <= 0))
                    {
                        vendorId = foundVendorId;
                    }
                }
                else
                {
                    selectedBillIdStr = billId;
                    selectedBillNumberStr = billId;
                }
            }

            ViewBag.SelectedBillID = selectedBillIdStr;
            ViewBag.SelectedBillNumber = selectedBillNumberStr;
            ViewBag.VendorID = vendorId ?? 0;

            // Load all Chart of Accounts
            string sqlAccounts = @"
                SELECT 
                    ChartOfAccountID, 
                    AccountType, 
                    AccountNo,
                    AccountName,
                    (AccountNo + ' - ' + AccountName) AS FullAccountName,
                    CASE 
                        WHEN AccountName LIKE '%ABA%' THEN 'ABA Bank'
                        WHEN AccountName LIKE '%ACLEDA%' THEN 'ACLEDA Bank'
                        WHEN AccountName LIKE '%Wing%' THEN 'Wing Bank'
                        WHEN AccountName LIKE '%Canadia%' THEN 'Canadia Bank'
                        WHEN AccountName LIKE '%Deposit%' OR AccountName LIKE '%Advance%' THEN 'Deposit'
                        ELSE AccountName
                    END AS ShortBankName
                FROM dbo.TblChartOfAccount
                WHERE (SystemID = " + sysId + @" OR ISNULL(SystemID, 0) = 0)
                  AND (WarehouseID = " + whId + @" OR ISNULL(WarehouseID, 0) = 0)
                ORDER BY 
                    CASE 
                        WHEN AccountNo LIKE '111%' THEN 0 
                        WHEN AccountNo LIKE '100%' THEN 1 
                        WHEN AccountName LIKE '%Deposit%' OR AccountNo LIKE '122%' THEN 2
                        ELSE 3 
                    END, AccountNo ASC";

            DataSet dsAccounts = html.LoadData(sqlAccounts);
            ViewBag.PaymentAccounts = dsAccounts != null && dsAccounts.Tables.Count > 0 ? dsAccounts.Tables[0] : new DataTable();

            // Load Vendors with display format: "(00014) ABC Company Co.,Ltd", plus Contact Person & Phone
            string sqlVendors = @"
                SELECT 
                    v.VendorID, 
                    MAX(v.VendorCode) AS VendorCode,
                    MAX(v.VendorName) AS VendorName,
                    MAX(ISNULL(v.VendorName, '')) AS VendorDisplayName,
                    MAX(ISNULL(v.ContactPersonName, '')) AS ContactPerson,
                    MAX(ISNULL(NULLIF(v.ContactPhoneNumber, ''), ISNULL(v.Phone, ''))) AS Phone
                FROM dbo.TblVendor v
                WHERE (v.SystemID = " + sysId + @" OR " + sysId + @" = 0 OR ISNULL(v.SystemID, 0) = 0)
                  AND (v.WarehouseID = " + whId + @" OR " + whId + @" = 0 OR ISNULL(v.WarehouseID, 0) = 0)
                GROUP BY v.VendorID
                ORDER BY VendorDisplayName ASC";

            DataSet dsVendors = html.LoadData(sqlVendors);
            ViewBag.Vendors = dsVendors != null && dsVendors.Tables.Count > 0 ? dsVendors.Tables[0] : new DataTable();

            // Load Payment Methods (Cash, Bank Transfer, Cheque, Petty Cash)
            string sqlPaymentMethods = @"
                SELECT MIN(PaymentMethodID) AS PaymentMethodID, PaymentMethod 
                FROM dbo.TblPaymentMethod 
                WHERE (SystemID = " + sysId + @" OR " + sysId + @" = 0 OR ISNULL(SystemID, 0) = 0)
                  AND (WarehouseID = " + whId + @" OR " + whId + @" = 0 OR ISNULL(WarehouseID, 0) = 0)
                  AND PaymentMethod IN ('Cash', 'Bank Transfer', 'Cheque', 'Petty Cash')
                GROUP BY PaymentMethod 
                ORDER BY 
                    CASE 
                        WHEN PaymentMethod = 'Cash' THEN 1 
                        WHEN PaymentMethod = 'Bank Transfer' THEN 2 
                        WHEN PaymentMethod = 'Cheque' THEN 3 
                        WHEN PaymentMethod = 'Petty Cash' THEN 4 
                        ELSE 5 
                    END, PaymentMethod ASC";
            DataSet dsPM = html.LoadData(sqlPaymentMethods);
            if (dsPM == null || dsPM.Tables.Count == 0 || dsPM.Tables[0].Rows.Count == 0)
            {
                sqlPaymentMethods = @"
                    SELECT MIN(PaymentMethodID) AS PaymentMethodID, PaymentMethod 
                    FROM dbo.TblPaymentMethod 
                    WHERE PaymentMethod IN ('Cash', 'Bank Transfer', 'Cheque', 'Petty Cash')
                    GROUP BY PaymentMethod 
                    ORDER BY 
                        CASE 
                            WHEN PaymentMethod = 'Cash' THEN 1 
                            WHEN PaymentMethod = 'Bank Transfer' THEN 2 
                            WHEN PaymentMethod = 'Cheque' THEN 3 
                            WHEN PaymentMethod = 'Petty Cash' THEN 4 
                            ELSE 5 
                        END, PaymentMethod ASC";
                dsPM = html.LoadData(sqlPaymentMethods);
            }
            ViewBag.PaymentMethods = dsPM != null && dsPM.Tables.Count > 0 ? dsPM.Tables[0] : new DataTable();

            // Load Classes / Companies
            string sqlClassesCreate = @"
                SELECT ClassID, ClassName, FullName, PrefixPV 
                FROM dbo.TblClass 
                WHERE (SystemID = " + sysId + @" AND (WarehouseID = " + whId + @" OR ISNULL(WarehouseID, 0) = 0)) OR ClassID IN (5, 6, 7) OR PrefixPV IS NOT NULL
                ORDER BY ClassID ASC";
            DataSet dsClassesCreate = html.LoadData(sqlClassesCreate);
            ViewBag.Classes = dsClassesCreate != null && dsClassesCreate.Tables.Count > 0 ? dsClassesCreate.Tables[0] : new DataTable();

            return View("~/Views/PayBill/Create.cshtml");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET UNPAID BILLS & VENDOR EXPENSES API  →  Returns unified JSON
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetUnpaidBills(int? vendorId = null, string searchText = "", string billId = "", string fromDate = "", string toDate = "", int? systemId = null, int? warehouseId = null, string itemType = "all")
        {
            HTML html = new HTML();
            string sysId = GetSystemID(html, systemId);
            string whId = GetWarehouseID(html, warehouseId);

            bool allowCross = (vendorId.HasValue && vendorId.Value > 0) || !string.IsNullOrEmpty(billId) || !string.IsNullOrEmpty(searchText);

            string vendorFilter = "";
            if (vendorId.HasValue && vendorId.Value > 0)
            {
                vendorFilter = @" AND (
                    t.VendorID = " + vendorId.Value + @" 
                    OR (ISNULL(t.VendorName, '') <> '' AND t.VendorName = (SELECT TOP 1 ISNULL(v_sub.VendorName, '') FROM dbo.TblVendor v_sub WHERE v_sub.VendorID = " + vendorId.Value + @"))
                )";
            }
            
            string searchFilter = "";
            if (!string.IsNullOrEmpty(searchText))
            {
                string safe = searchText.Trim().Replace("'", "''");
                searchFilter = " AND (t.BillNumber LIKE '%" + safe + "%' OR t.PONumber LIKE '%" + safe + "%' OR t.VendorName LIKE '%" + safe + "%' OR t.VendorCode LIKE '%" + safe + "%' OR t.Memo LIKE '%" + safe + "%')";
            }

            string billFilter = "";
            if (!string.IsNullOrEmpty(billId))
            {
                string safeB = billId.Trim().Replace("'", "''");
                billFilter = " AND (t.BillNumber = '" + safeB + "' OR CONVERT(VARCHAR, t.BillID) = '" + safeB + "' OR CONVERT(VARCHAR, t.ExpenseID) = '" + safeB + "')";
            }

            string dateFilter = "";
            if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime dtFrom;
                if (DateTime.TryParseExact(fromDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtFrom) || DateTime.TryParse(fromDate, out dtFrom))
                {
                    dateFilter += " AND t.BillDate >= '" + dtFrom.ToString("yyyy-MM-dd") + "'";
                }
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                DateTime dtTo;
                if (DateTime.TryParseExact(toDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtTo) || DateTime.TryParse(toDate, out dtTo))
                {
                    dateFilter += " AND t.BillDate <= '" + dtTo.ToString("yyyy-MM-dd") + " 23:59:59'";
                }
            }

            string targetBillIdStr = (!string.IsNullOrEmpty(billId) ? billId.Trim().Replace("'", "''") : "0");

            string sql = @"
                WITH LatestExpTransactions AS (
                    SELECT *, ROW_NUMBER() OVER (PARTITION BY ExpenseID ORDER BY ExpensePaymentID DESC) AS rn
                    FROM dbo.Expense_PaymentTransactions
                )
                SELECT * FROM (
                    -- Bills from TblBills (NO approval check: Draft, Pending, or Open bills all show for testing)
                    SELECT 
                        'Bill' AS ItemType,
                        b.BillID,
                        0 AS ExpenseID,
                        ISNULL(b.ClassID, 0) AS ClassID,
                        b.VendorID,
                        ISNULL(v.VendorName, '') AS VendorName,
                        ISNULL(v.VendorCode, '') AS VendorCode,
                        ISNULL(v.VendorName, '') AS VendorDisplayName,
                        ISNULL(b.BillNumber, '') AS BillNumber,
                        ISNULL(b.PONumber, '') AS PONumber,
                        b.BillDate,
                        FORMAT(b.BillDate, 'dd/MM/yyyy') AS BillDateFormatted,
                        b.PaymentDueDate,
                        FORMAT(ISNULL(b.PaymentDueDate, b.BillDate), 'dd/MM/yyyy') AS DueDateFormatted,
                        b.GrandTotal,
                        ISNULL(b.AmountPaid, 0) AS AmountPaid,
                        ISNULL((SELECT SUM(da.AppliedAmount) FROM dbo.TblVendorDepositApplication da WHERE da.ExpenseID = b.BillID), 0.00) AS DepositApplied,
                        ISNULL(NULLIF(b.AmountDue, 0), b.GrandTotal - ISNULL(b.AmountPaid, 0)) AS AmountDue,
                        ISNULL(dep.AvailableDeposit, 0) AS AvailableDeposit,
                        ISNULL(dep.DepositExpenseID, 0) AS DepositExpenseID,
                        ISNULL(dep.DepositPVNumber, '') AS DepositPVNumber,
                        DATEDIFF(day, CAST(ISNULL(b.PaymentDueDate, b.BillDate) AS DATE), CAST(GETDATE() AS DATE)) AS DaysOverdue,
                        CASE 
                            WHEN DATEDIFF(day, CAST(ISNULL(b.PaymentDueDate, b.BillDate) AS DATE), CAST(GETDATE() AS DATE)) > 0 THEN 'Overdue'
                            WHEN DATEDIFF(day, CAST(ISNULL(b.PaymentDueDate, b.BillDate) AS DATE), CAST(GETDATE() AS DATE)) = 0 THEN 'Due Today'
                            ELSE 'Due Soon'
                        END AS DueStatus,
                        ISNULL(b.CustomerNotes, '') AS Memo,
                        ISNULL(b.StatusID, 1) AS StatusID
                    FROM dbo.TblBills b
                    LEFT JOIN dbo.TblVendor v ON v.VendorID = b.VendorID
                    OUTER APPLY (
                        SELECT TOP 1
                            ISNULL(NULLIF(e.PaidAmount, 0), e.TotalAmount) AS AvailableDeposit,
                            e.ExpenseID AS DepositExpenseID,
                            ISNULL(e.[No], e.PaymentNo) AS DepositPVNumber
                        FROM dbo.Expenses e
                        WHERE (
                            (e.ExpenseTransactionType = 'Vendor Deposit' AND (
                                (ISNULL(b.PONumber, '') <> '' AND (e.Reference = b.PONumber OR e.Reference LIKE '%' + b.PONumber + '%'))
                                OR (ISNULL(b.PONumber, '') = '' AND e.PayeeID = b.VendorID)
                            ))
                            OR (ISNULL(b.PONumber, '') <> '' AND (e.Reference = b.PONumber OR e.Reference LIKE '%' + b.PONumber + '%' OR EXISTS (SELECT 1 FROM dbo.ExpenseDetails ed WHERE ed.ExpenseID = e.ExpenseID AND (ed.RefNo = b.PONumber OR ed.RefNo LIKE '%' + b.PONumber + '%'))))
                            OR (ISNULL(b.BillNumber, '') <> '' AND (e.Reference = b.BillNumber OR e.Reference LIKE '%' + b.BillNumber + '%' OR EXISTS (SELECT 1 FROM dbo.ExpenseDetails ed WHERE ed.ExpenseID = e.ExpenseID AND (ed.RefNo = b.BillNumber OR ed.RefNo LIKE '%' + b.BillNumber + '%'))))
                        )
                        AND ISNULL(e.PaymentStatus, '') <> 'Applied'
                        AND (e.PayeeID = b.VendorID OR ISNULL(b.VendorID, 0) = 0 OR ISNULL(e.PayeeID, 0) = 0)
                        AND (ISNULL(e.PaidAmount, 0) > 0 OR (e.ExpenseTransactionType = 'Vendor Deposit' AND ISNULL(e.TotalAmount, 0) > 0))
                        AND (e.SystemID = b.SystemID OR " + (allowCross ? "1=1" : "0=1") + @")
                        AND (e.WarehouseID = b.WarehouseID OR " + (allowCross ? "1=1" : "0=1") + @")
                        ORDER BY e.ExpenseID DESC
                    ) dep
                    WHERE (b.SystemID = " + sysId + @" OR " + sysId + @" = 0 OR ISNULL(b.SystemID, 0) = 0" + (allowCross ? " OR 1=1" : "") + @")
                      AND (b.WarehouseID = " + whId + @" OR " + whId + @" = 0 OR ISNULL(b.WarehouseID, 0) = 0" + (allowCross ? " OR 1=1" : "") + @")
                      AND (ISNULL(NULLIF(b.AmountDue, 0), ISNULL(b.GrandTotal, 0) - ISNULL(b.AmountPaid, 0)) > 0 OR (ISNULL(b.GrandTotal, 0) > 0 AND ISNULL(b.AmountPaid, 0) < ISNULL(b.GrandTotal, 0)))
                      AND ISNULL(b.StatusID, 0) <> 10

                    UNION ALL

                    -- Bills from dbo.Expense_PaymentTransactions (Deduplicated with rn = 1, and excluding bills already in TblBills)
                    SELECT 
                        'Expense' AS ItemType,
                        0 AS BillID,
                        e.ExpenseID,
                        ISNULL(e.ClassName, 0) AS ClassID,
                        COALESCE(NULLIF(e.PayeeID, 0), NULLIF(exp.PayeeID, 0), 0) AS VendorID,
                        ISNULL(py.VendorName, '') AS VendorName,
                        ISNULL(py.VendorCode, '') AS VendorCode,
                        ISNULL(py.VendorName, '') AS VendorDisplayName,
                        COALESCE(NULLIF(exp.ExpenseNo, ''), NULLIF(e.ExpenseNo, ''), NULLIF(exp.[No], ''), NULLIF(e.[No], ''), 'EXP-' + CONVERT(VARCHAR, e.ExpenseID)) AS BillNumber,
                        COALESCE(NULLIF(e.Reference, ''), NULLIF(exp.Reference, ''), '') AS PONumber,
                        ISNULL(e.PaymentDate, GETDATE()) AS BillDate,
                        FORMAT(ISNULL(e.PaymentDate, GETDATE()), 'dd/MM/yyyy') AS BillDateFormatted,
                        ISNULL(e.PaidOnDate, ISNULL(e.PaymentDate, GETDATE())) AS PaymentDueDate,
                        FORMAT(ISNULL(e.PaidOnDate, ISNULL(e.PaymentDate, GETDATE())), 'dd/MM/yyyy') AS DueDateFormatted,
                        COALESCE(NULLIF(e.TotalAmount, 0), exp.TotalAmount, 0) AS GrandTotal,
                        COALESCE(e.PaidAmount, exp.PaidAmount, 0) AS AmountPaid,
                        ISNULL((SELECT SUM(da.AppliedAmount) FROM dbo.TblVendorDepositApplication da WHERE da.ExpenseID = e.ExpenseID), 0.00) AS DepositApplied,
                        ISNULL(NULLIF(e.BalanceAmount, 0), ISNULL(NULLIF(exp.BalanceAmount, 0), COALESCE(NULLIF(e.TotalAmount, 0), exp.TotalAmount, 0) - COALESCE(e.PaidAmount, exp.PaidAmount, 0))) AS AmountDue,
                        0.00 AS AvailableDeposit,
                        0 AS DepositExpenseID,
                        '' AS DepositPVNumber,
                        DATEDIFF(day, CAST(ISNULL(e.PaidOnDate, ISNULL(e.PaymentDate, GETDATE())) AS DATE), CAST(GETDATE() AS DATE)) AS DaysOverdue,
                        CASE 
                            WHEN DATEDIFF(day, CAST(ISNULL(e.PaidOnDate, ISNULL(e.PaymentDate, GETDATE())) AS DATE), CAST(GETDATE() AS DATE)) > 0 THEN 'Overdue'
                            WHEN DATEDIFF(day, CAST(ISNULL(e.PaidOnDate, ISNULL(e.PaymentDate, GETDATE())) AS DATE), CAST(GETDATE() AS DATE)) = 0 THEN 'Due Today'
                            ELSE 'Due Soon'
                        END AS DueStatus,
                        COALESCE(NULLIF(e.Description, ''), NULLIF(exp.Description, ''), '') AS Memo,
                        1 AS StatusID
                    FROM LatestExpTransactions e
                    LEFT JOIN dbo.Expenses exp ON exp.ExpenseID = e.ExpenseID
                    LEFT JOIN dbo.TblVendor py ON py.VendorID = COALESCE(NULLIF(e.PayeeID, 0), NULLIF(exp.PayeeID, 0), 0)
                    WHERE e.rn = 1
                      AND (
                        e.Type IN ('Bill', 'Expense', 'Enter Bill') 
                        OR e.ExpenseTransactionType IN ('Bill', 'Expense', 'Enter Bill') 
                        OR exp.Type IN ('Bill', 'Expense', 'Enter Bill') 
                        OR exp.ExpenseTransactionType IN ('Bill', 'Expense', 'Enter Bill') 
                        OR ISNULL(e.Type, '') = '' 
                        OR e.ExpenseNo LIKE 'BL%' 
                        OR exp.ExpenseNo LIKE 'BL%'
                      )
                      AND (e.SystemID = " + sysId + @" OR " + sysId + @" = 0 OR ISNULL(e.SystemID, 0) = 0" + (allowCross ? " OR 1=1" : "") + @")
                      AND (e.WarehouseID = " + whId + @" OR " + whId + @" = 0 OR ISNULL(e.WarehouseID, 0) = 0" + (allowCross ? " OR 1=1" : "") + @")
                      AND (
                          ISNULL(NULLIF(e.BalanceAmount, 0), ISNULL(NULLIF(exp.BalanceAmount, 0), COALESCE(NULLIF(e.TotalAmount, 0), exp.TotalAmount, 0) - COALESCE(e.PaidAmount, exp.PaidAmount, 0))) > 0 
                          OR (COALESCE(NULLIF(e.TotalAmount, 0), exp.TotalAmount, 0) > 0 AND COALESCE(e.PaidAmount, exp.PaidAmount, 0) < COALESCE(NULLIF(e.TotalAmount, 0), exp.TotalAmount, 0))
                      )
                      AND ISNULL(NULLIF(e.PaymentStatus, ''), ISNULL(exp.PaymentStatus, '')) <> 'Paid'
                      AND NOT EXISTS (
                          SELECT 1 FROM dbo.TblBills tb 
                          WHERE tb.BillNumber = COALESCE(NULLIF(exp.ExpenseNo, ''), NULLIF(e.ExpenseNo, ''), NULLIF(exp.[No], ''), NULLIF(e.[No], ''))
                            AND (tb.SystemID = e.SystemID OR ISNULL(tb.SystemID, 0) = 0)
                      )

                    UNION ALL

                    -- Standalone bills from dbo.Expenses (if not yet in Expense_PaymentTransactions, and excluding bills already in TblBills)
                    SELECT 
                        'Expense' AS ItemType,
                        0 AS BillID,
                        exp.ExpenseID,
                        ISNULL(exp.ClassName, 0) AS ClassID,
                        ISNULL(exp.PayeeID, 0) AS VendorID,
                        ISNULL(py.VendorName, '') AS VendorName,
                        ISNULL(py.VendorCode, '') AS VendorCode,
                        ISNULL(py.VendorName, '') AS VendorDisplayName,
                        COALESCE(NULLIF(exp.ExpenseNo, ''), NULLIF(exp.[No], ''), 'EXP-' + CONVERT(VARCHAR, exp.ExpenseID)) AS BillNumber,
                        ISNULL(exp.Reference, '') AS PONumber,
                        ISNULL(exp.PaymentDate, GETDATE()) AS BillDate,
                        FORMAT(ISNULL(exp.PaymentDate, GETDATE()), 'dd/MM/yyyy') AS BillDateFormatted,
                        ISNULL(exp.PaidOnDate, ISNULL(exp.PaymentDate, GETDATE())) AS PaymentDueDate,
                        FORMAT(ISNULL(exp.PaidOnDate, ISNULL(exp.PaymentDate, GETDATE())), 'dd/MM/yyyy') AS DueDateFormatted,
                        ISNULL(exp.TotalAmount, 0) AS GrandTotal,
                        ISNULL(exp.PaidAmount, 0) AS AmountPaid,
                        0.00 AS DepositApplied,
                        ISNULL(NULLIF(exp.BalanceAmount, 0), ISNULL(exp.TotalAmount, 0) - ISNULL(exp.PaidAmount, 0)) AS AmountDue,
                        0.00 AS AvailableDeposit,
                        0 AS DepositExpenseID,
                        '' AS DepositPVNumber,
                        DATEDIFF(day, CAST(ISNULL(exp.PaidOnDate, ISNULL(exp.PaymentDate, GETDATE())) AS DATE), CAST(GETDATE() AS DATE)) AS DaysOverdue,
                        CASE 
                            WHEN DATEDIFF(day, CAST(ISNULL(exp.PaidOnDate, ISNULL(exp.PaymentDate, GETDATE())) AS DATE), CAST(GETDATE() AS DATE)) > 0 THEN 'Overdue'
                            WHEN DATEDIFF(day, CAST(ISNULL(exp.PaidOnDate, ISNULL(exp.PaymentDate, GETDATE())) AS DATE), CAST(GETDATE() AS DATE)) = 0 THEN 'Due Today'
                            ELSE 'Due Soon'
                        END AS DueStatus,
                        ISNULL(exp.Description, '') AS Memo,
                        1 AS StatusID
                    FROM dbo.Expenses exp
                    LEFT JOIN dbo.TblVendor py ON py.VendorID = exp.PayeeID
                    WHERE (
                        exp.Type IN ('Bill', 'Expense', 'Enter Bill') 
                        OR exp.ExpenseTransactionType IN ('Bill', 'Expense', 'Enter Bill') 
                        OR ISNULL(exp.Type, '') = '' 
                        OR exp.ExpenseNo LIKE 'BL%'
                    )
                      AND NOT EXISTS (SELECT 1 FROM dbo.Expense_PaymentTransactions e2 WHERE e2.ExpenseID = exp.ExpenseID)
                      AND NOT EXISTS (
                          SELECT 1 FROM dbo.TblBills tb 
                          WHERE tb.BillNumber = COALESCE(NULLIF(exp.ExpenseNo, ''), NULLIF(exp.[No], ''))
                            AND (tb.SystemID = exp.SystemID OR ISNULL(tb.SystemID, 0) = 0)
                      )
                      AND (exp.SystemID = " + sysId + @" OR " + sysId + @" = 0 OR ISNULL(exp.SystemID, 0) = 0" + (allowCross ? " OR 1=1" : "") + @")
                      AND (exp.WarehouseID = " + whId + @" OR " + whId + @" = 0 OR ISNULL(exp.WarehouseID, 0) = 0" + (allowCross ? " OR 1=1" : "") + @")
                      AND (ISNULL(NULLIF(exp.BalanceAmount, 0), ISNULL(exp.TotalAmount, 0) - ISNULL(exp.PaidAmount, 0)) > 0 OR (ISNULL(exp.TotalAmount, 0) > 0 AND ISNULL(exp.PaidAmount, 0) < ISNULL(exp.TotalAmount, 0)))
                      AND ISNULL(exp.PaymentStatus, '') <> 'Paid'
                ) t
                WHERE 1=1
                " + vendorFilter + searchFilter + billFilter + dateFilter + @"
                ORDER BY 
                    CASE WHEN (t.BillNumber = '" + targetBillIdStr + @"' OR CONVERT(VARCHAR, t.BillID) = '" + targetBillIdStr + @"' OR CONVERT(VARCHAR, t.ExpenseID) = '" + targetBillIdStr + @"') THEN 0 ELSE 1 END,
                    t.BillDate DESC, 
                    t.DaysOverdue DESC";

            DataSet ds = html.LoadData(sql);
            if (ds != null && ds.Tables.Count > 0)
            {
                string json = JsonConvert.SerializeObject(ds.Tables[0]);
                var list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

                // Strict deduplication:
                // 1) Distinct by Entity Key (BillID or ExpenseID)
                // 2) Distinct by BillNumber / Reference (if non-empty)
                var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var seenBillNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var uniqueList = new List<Dictionary<string, object>>();

                if (list != null)
                {
                    foreach (var row in list)
                    {
                        string bId = Convert.ToString(row.ContainsKey("BillID") ? row["BillID"] : "0").Trim();
                        string expId = Convert.ToString(row.ContainsKey("ExpenseID") ? row["ExpenseID"] : "0").Trim();
                        string billNo = Convert.ToString(row.ContainsKey("BillNumber") ? row["BillNumber"] : "").Trim();

                        string entityKey = (bId != "0" && !string.IsNullOrEmpty(bId)) ? ("B_" + bId) : ("E_" + expId);

                        if (seenKeys.Contains(entityKey))
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(billNo) && !billNo.StartsWith("EXP-", StringComparison.OrdinalIgnoreCase))
                        {
                            if (seenBillNumbers.Contains(billNo))
                            {
                                continue;
                            }
                            seenBillNumbers.Add(billNo);
                        }

                        seenKeys.Add(entityKey);
                        uniqueList.Add(row);
                    }
                }

                return Json(new { success = true, bills = uniqueList, data = uniqueList }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, bills = new object[0], data = new object[0] }, JsonRequestBehavior.AllowGet);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET PV NUMBER API  →  Retrieves next sequential Payment Voucher Nº
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetPVNumber(int classId = 0, string paymentDate = "", int? systemId = null, int? warehouseId = null)
        {
            HTML html = new HTML();
            string sysId = GetSystemID(html, systemId);
            string whId = GetWarehouseID(html, warehouseId);
            string domainId = html.DomainID ?? "001";

            DateTime dtDate = DateTime.Now;
                if (!string.IsNullOrEmpty(paymentDate))
                {
                if (!DateTime.TryParseExact(paymentDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtDate) &&
                    !DateTime.TryParse(paymentDate, out dtDate))
                    {
                    return Json(new { success = false, message = "Please enter a valid payment date." }, JsonRequestBehavior.AllowGet);
                }
            }

            try
                {
                string pvNumber = "";
                string prefix = "";

                // 1. Determine prefix from TblClass if classId provided, otherwise check TblPrefix
                string checkPrefixSql = @"
                    DECLARE @Pref NVARCHAR(50) = '', @Mod NVARCHAR(50) = 'PrefixPV';
                    IF (" + classId + @" > 0)
                        BEGIN
                        SELECT @Pref = PrefixPV FROM dbo.TblClass WHERE ClassID = " + classId + @";
                        END

                    IF (@Pref IS NULL OR @Pref = '')
                    BEGIN
                        SELECT TOP 1 @Pref = Prefix FROM dbo.TblPrefix 
                        WHERE Module = 'PrefixPV' AND SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
                        ORDER BY PrefixID DESC;
                    END

                    IF (@Pref IS NULL OR @Pref = '')
                    BEGIN
                        SELECT TOP 1 @Pref = Prefix FROM dbo.TblPrefix 
                        WHERE Module = 'Expense' AND Prefix LIKE 'NPV%' AND SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
                        ORDER BY PrefixID DESC;
                    END

                    IF (@Pref IS NULL OR @Pref = '')
                    BEGIN
                        SET @Pref = 'NPV-{yy}{MM}-{000000}';
                    END

                    EXEC dbo.SP_SHOW_PREFIX @Pref, @Mod, " + sysId + @", " + whId + @", '" + domainId + @"';
                ";

                DataSet ds = html.LoadData(checkPrefixSql);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    if (row.Table.Columns.Contains("PrefixNumber") && row["PrefixNumber"] != DBNull.Value)
                        pvNumber = row["PrefixNumber"].ToString();
                    else if (row.Table.Columns.Contains("Prefix") && row["Prefix"] != DBNull.Value)
                        pvNumber = row["Prefix"].ToString();
                    else if (row[0] != DBNull.Value)
                        pvNumber = row[0].ToString();

                    if (row.Table.Columns.Contains("Prefix") && row["Prefix"] != DBNull.Value)
                        prefix = row["Prefix"].ToString();
                }

                // Fallback calculation if SP_SHOW_PREFIX returned empty
                if (string.IsNullOrEmpty(pvNumber))
                {
                    string yyMM = dtDate.ToString("yyMM");
                    string pat = "NPV-" + yyMM + "-";
                    string maxSql = @"
                        SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(t.PVNo, LEN('" + pat + @"') + 1, 10) AS INT)), 0)
                        FROM (
                            SELECT PVNo FROM dbo.TblAccount_Transaction WHERE PVNo LIKE '" + pat + @"%' AND SystemID = " + sysId + @"
                            UNION
                            SELECT PVNo FROM dbo.Expenses WHERE PVNo LIKE '" + pat + @"%' AND SystemID = " + sysId + @"
                        ) t";
                    DataSet dsMax = html.LoadData(maxSql);
                    if (dsMax == null || dsMax.Tables.Count == 0 || dsMax.Tables[0].Rows.Count == 0 || dsMax.Tables[0].Rows[0][0] == DBNull.Value)
                    {
                        throw new InvalidOperationException("Unable to determine the next payment voucher number.");
                    }
                    int nextNo = Convert.ToInt32(dsMax.Tables[0].Rows[0][0]) + 1;
                    pvNumber = pat + nextNo.ToString("D6");
                    prefix = "NPV-{yy}{MM}-{000000}";
                }

                return Json(new { success = true, pvNumber = pvNumber, prefix = prefix }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Unable to generate a payment voucher number." }, JsonRequestBehavior.AllowGet);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SAVE PAYMENT API  →  Processes batch or single payments (Bills + Expenses)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [AllowAnonymous]
        public JsonResult SavePayment(string paymentHeader, string paymentDetails)
        {
            HTML html = new HTML();

            try
            {
                dynamic header = JsonConvert.DeserializeObject(paymentHeader);
                dynamic details = JsonConvert.DeserializeObject(paymentDetails);
                if (header == null || details == null)
                {
                    return Json(new { success = false, message = "Payment details are missing or invalid." });
                }

                int? headerSysId = null;
                if (header.SystemID != null)
                {
                    int.TryParse(header.SystemID.ToString(), out int sId);
                    if (sId > 0) headerSysId = sId;
                }
                int? headerWhId = null;
                if (header.WarehouseID != null)
                {
                    int.TryParse(header.WarehouseID.ToString(), out int wId);
                    if (wId > 0) headerWhId = wId;
                }

                string sysId = GetSystemID(html, headerSysId);
                string whId = GetWarehouseID(html, headerWhId);

                int vendorId = 0;
                if (header.VendorID != null)
                {
                    int.TryParse(header.VendorID.ToString(), out vendorId);
                }

                int classId = 0;
                if (header.ClassID != null)
                {
                    int.TryParse(header.ClassID.ToString(), out classId);
                }

                string payee = header.Payee != null ? header.Payee.ToString() : "";
                string pvNumber = header.PVNumber != null ? header.PVNumber.ToString().Trim() : "";
                string reference = header.Reference != null ? header.Reference.ToString() : "";
                string paymentDateStr = header.PaymentDate != null ? header.PaymentDate.ToString() : DateTime.Now.ToString("yyyy-MM-dd");
                int chartOfAccountId = header.ChartOfAccountID != null ? (int)header.ChartOfAccountID : 0;
                int paymentModeId = header.PaymentModeID != null ? (int)header.PaymentModeID : 0;
                string memo = header.Memo != null ? header.Memo.ToString() : "";
                decimal totalPaymentAmount = header.TotalPaymentAmount != null ? (decimal)header.TotalPaymentAmount : 0;
                string fromBankName = header.FromBankName != null ? header.FromBankName.ToString() : "";
                string fromAccountNumber = header.FromAccountNumber != null ? header.FromAccountNumber.ToString() : "";
                string chequeNumber = header.ChequeNumber != null ? header.ChequeNumber.ToString() : "";

                DateTime dtPaymentDate;
                if (!DateTime.TryParseExact(paymentDateStr, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtPaymentDate) && !DateTime.TryParse(paymentDateStr, out dtPaymentDate))
                {
                    return Json(new { success = false, message = "Please enter a valid payment date." });
                }

                if (vendorId <= 0 || chartOfAccountId <= 0 || totalPaymentAmount <= 0)
                {
                    return Json(new { success = false, message = "Select a vendor and payment account, and enter a payment amount greater than zero." });
                }

                decimal detailPaymentTotal = 0;
                int payableDetailCount = 0;
                var validatedBillKeys = new HashSet<string>();
                foreach (dynamic bill in details)
                {
                    int billId = bill.BillID != null ? (int)bill.BillID : 0;
                    int expenseId = bill.ExpenseID != null ? (int)bill.ExpenseID : 0;
                    int detailVendorId = bill.VendorID != null ? (int)bill.VendorID : vendorId;
                    decimal paymentAmount = bill.PaymentAmount != null ? (decimal)bill.PaymentAmount : 0;
                    decimal depositAmount = bill.DepositAmount != null ? (decimal)bill.DepositAmount : 0;

                    if ((billId > 0) == (expenseId > 0) || detailVendorId != vendorId)
                    {
                        return Json(new { success = false, message = "Each payment must contain valid bills from the selected vendor only." });
                    }

                    string billKey = billId > 0 ? "B_" + billId : "E_" + expenseId;
                    if (!validatedBillKeys.Add(billKey))
                    {
                        return Json(new { success = false, message = "A bill can only appear once in a payment." });
                    }

                    if (paymentAmount < 0 || depositAmount < 0)
                    {
                        return Json(new { success = false, message = "Payment and deposit amounts must be non-negative." });
                    }

                    if (paymentAmount == 0 && depositAmount == 0)
                    {
                        continue;
                    }

                    detailPaymentTotal += paymentAmount;
                    payableDetailCount++;
                }

                if (payableDetailCount == 0 || Math.Abs(detailPaymentTotal - totalPaymentAmount) >= 0.01m)
                {
                    return Json(new { success = false, message = "The payment total does not match the bill allocations." });
                }

                // 1. Generate or validate PV Number (Payment Voucher Nº)
                if (string.IsNullOrEmpty(pvNumber))
                {
                    string pvSql = @"
                        DECLARE @Pref NVARCHAR(50) = '', @Mod NVARCHAR(50) = 'PrefixPV';
                        IF (" + classId + @" > 0)
                        BEGIN
                            SELECT @Pref = PrefixPV FROM dbo.TblClass WHERE ClassID = " + classId + @";
                        END

                        IF (@Pref IS NULL OR @Pref = '')
                        BEGIN
                            SELECT TOP 1 @Pref = Prefix FROM dbo.TblPrefix 
                            WHERE Module = 'PrefixPV' AND SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
                            ORDER BY PrefixID DESC;
                        END

                        IF (@Pref IS NULL OR @Pref = '')
                        BEGIN
                            SELECT TOP 1 @Pref = Prefix FROM dbo.TblPrefix 
                            WHERE Module = 'Expense' AND Prefix LIKE 'NPV%' AND SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
                            ORDER BY PrefixID DESC;
                        END

                        IF (@Pref IS NULL OR @Pref = '')
                        BEGIN
                            SET @Pref = 'NPV-{yy}{MM}-{000000}';
                        END

                        DECLARE @OutPV NVARCHAR(50);
                        EXEC dbo.SP_INSERT_PREFIX_BASE_ON_YEAR @Pref, @Mod, " + sysId + @", " + whId + @", @OutPV OUT, 1, 0, '" + html.DomainID + @"', '" + dtPaymentDate.ToString("yyyy-MM-dd") + @"';
                        SELECT @OutPV AS PVNo;";

                    DataSet dsPV = html.LoadData(pvSql);
                    if (dsPV != null && dsPV.Tables.Count > 0)
                    {
                        for (int t = dsPV.Tables.Count - 1; t >= 0; t--)
                        {
                            var dt = dsPV.Tables[t];
                            if (dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value && !string.IsNullOrEmpty(dt.Rows[0][0].ToString()))
                            {
                                pvNumber = dt.Rows[0][0].ToString();
                                break;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(pvNumber))
                    {
                        return Json(new { success = false, message = "Unable to generate a payment voucher number." });
                    }
                }
                else
                {
                    // User confirmed custom or generated PV number -> synchronize counter in TblPrefix if numerical suffix is present
                    try
                    {
                        int lastDash = pvNumber.LastIndexOf('-');
                        if (lastDash >= 0 && lastDash < pvNumber.Length - 1)
                        {
                            string numPart = pvNumber.Substring(lastDash + 1);
                            if (int.TryParse(numPart, out int numVal))
                            {
                                string syncPrefixSql = @"
                                    UPDATE dbo.TblPrefix 
                                    SET CurrentNumber = " + numVal + @", CurrentPrefix = N'" + pvNumber.Replace("'", "''") + @"'
                                    WHERE Module IN ('PrefixPV', 'Expense') 
                                      AND (Prefix LIKE 'NPV%' OR Prefix LIKE 'PV%') 
                                      AND SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
                                      AND ISNULL(CurrentNumber, 0) < " + numVal + ";";
                                html.LoadData(syncPrefixSql);
                            }
                        }
                    }
                    catch { }
                }

                string paymentNo = pvNumber;

                // Get account & payment mode display names
                string accountName = "";
                string paymentModeName = "";
                string nameLookupSql = @"
                    SELECT 
                        (SELECT (AccountNo + ' - ' + AccountName) FROM dbo.TblChartOfAccount WHERE ChartOfAccountID = " + chartOfAccountId + @") AS AccountName,
                        (SELECT PaymentMethod FROM dbo.TblPaymentMethod WHERE PaymentMethodID = " + paymentModeId + @") AS PaymentMethod;";
                DataSet dsNames = html.LoadData(nameLookupSql);
                if (dsNames != null && dsNames.Tables.Count > 0 && dsNames.Tables[0].Rows.Count > 0)
                {
                    accountName = dsNames.Tables[0].Rows[0]["AccountName"].ToString();
                    paymentModeName = dsNames.Tables[0].Rows[0]["PaymentMethod"].ToString();
                }

                EnsureStatusColumn(html);

                // 2. Insert into dbo.TblVendorPayment
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendLine("BEGIN TRANSACTION;");
                sbSql.AppendLine("BEGIN TRY");
                sbSql.AppendLine("DECLARE @PayBillCurrentBalance DECIMAL(19, 4), @PayBillCurrentVendorID INT;");

                foreach (dynamic bill in details)
                {
                    int billId = bill.BillID != null ? (int)bill.BillID : 0;
                    int expenseId = bill.ExpenseID != null ? (int)bill.ExpenseID : 0;
                    int detailVendorId = bill.VendorID != null ? (int)bill.VendorID : vendorId;
                    decimal paymentAmount = bill.PaymentAmount != null ? (decimal)bill.PaymentAmount : 0;
                    decimal depositAmount = bill.DepositAmount != null ? (decimal)bill.DepositAmount : 0;
                    decimal totalApplied = paymentAmount + depositAmount;
                    if (totalApplied <= 0)
                    {
                        continue;
                    }
                    string amountSql = totalApplied.ToString(System.Globalization.CultureInfo.InvariantCulture);

                    if (billId > 0)
                    {
                        sbSql.AppendFormat(@"
                            SET @PayBillCurrentBalance = NULL;
                            SET @PayBillCurrentVendorID = NULL;
                            SELECT
                                @PayBillCurrentBalance = ISNULL(NULLIF(b.AmountDue, 0), ISNULL(b.GrandTotal, 0) - ISNULL(b.AmountPaid, 0)),
                                @PayBillCurrentVendorID = b.VendorID
                            FROM dbo.TblBills b WITH (UPDLOCK, HOLDLOCK)
                            WHERE b.BillID = {0} AND b.SystemID = {1} AND b.WarehouseID = {2}
                              AND ISNULL(b.StatusID, 0) <> 10;
                            IF (@PayBillCurrentBalance IS NULL OR @PayBillCurrentVendorID <> {3} OR
                                @PayBillCurrentBalance <= 0 OR {4} > @PayBillCurrentBalance)
                                RAISERROR('A selected bill has changed or no longer has enough balance. Reload the bills and try again.', 16, 1);
                        ", billId, sysId, whId, detailVendorId, amountSql);
                    }
                    else
                    {
                        sbSql.AppendFormat(@"
                            SET @PayBillCurrentBalance = NULL;
                            SET @PayBillCurrentVendorID = NULL;
                            SELECT
                                @PayBillCurrentBalance = ISNULL(NULLIF(e.BalanceAmount, 0), ISNULL(e.TotalAmount, 0) - ISNULL(e.PaidAmount, 0)),
                                @PayBillCurrentVendorID = e.PayeeID
                            FROM dbo.Expense_PaymentTransactions e WITH (UPDLOCK, HOLDLOCK)
                            WHERE e.ExpenseID = {0} AND e.SystemID = {1} AND e.WarehouseID = {2}
                              AND (e.Type = 'Bill' OR e.Type = 'Expense' OR ISNULL(e.Type, '') = '')
                              AND ISNULL(e.PaymentStatus, '') <> 'Paid';
                            IF (@PayBillCurrentBalance IS NULL OR @PayBillCurrentVendorID <> {3} OR
                                @PayBillCurrentBalance <= 0 OR {4} > @PayBillCurrentBalance)
                                RAISERROR('A selected bill has changed or no longer has enough balance. Reload the bills and try again.', 16, 1);
                        ", expenseId, sysId, whId, detailVendorId, amountSql);
                    }
                }

                sbSql.AppendFormat(@"
                    DECLARE @NewPaymentID INT;
                    INSERT INTO dbo.TblVendorPayment (
                        VendorID, Payee, Reference, PaymentDate, ChartOfAccountID, PaymentAccount,
                        PaymentModeID, PaymentMode, FromBankName, FromAccountNumber, ChequeNumber,
                        OfficalReceiptNo, TotalPaymentAmount, PaidBy, [Type], Memo, [Status],
                        SystemID, WarehouseID, DomainID
                    ) VALUES (
                        {0}, N'{1}', N'{2}', '{3:yyyy-MM-dd HH:mm:ss}', {4}, N'{5}',
                        {6}, N'{7}', N'{8}', N'{9}', N'{10}',
                        N'{11}', {12}, N'{13}', 'Expense', N'{14}', 'Draft',
                        {15}, {16}, '{17}'
                    );
                    SET @NewPaymentID = SCOPE_IDENTITY();
                ", 
                    vendorId, 
                    payee.Replace("'", "''"), 
                    (!string.IsNullOrEmpty(reference) ? reference : paymentNo).Replace("'", "''"), 
                    dtPaymentDate, 
                    chartOfAccountId, 
                    accountName.Replace("'", "''"),
                    paymentModeId, 
                    paymentModeName.Replace("'", "''"),
                    fromBankName.Replace("'", "''"), 
                    fromAccountNumber.Replace("'", "''"), 
                    chequeNumber.Replace("'", "''"),
                    paymentNo.Replace("'", "''"), 
                    totalPaymentAmount, 
                    (html.UserName ?? "Admin").Replace("'", "''"), 
                    memo.Replace("'", "''"),
                    sysId, 
                    whId, 
                    html.DomainID
                );

                decimal totalDepositApplied = 0;
                string depositMemo = "";

                // 3. Insert Details & Update TblBills / Expenses
                foreach (dynamic bill in details)
                {
                    int bId = bill.BillID != null ? (int)bill.BillID : 0;
                    int expId = bill.ExpenseID != null ? (int)bill.ExpenseID : 0;
                    int bVendorId = bill.VendorID != null ? (int)bill.VendorID : vendorId;
                    string bNumber = bill.BillNumber != null ? bill.BillNumber.ToString() : "";
                    string bPoNumber = bill.PONumber != null ? bill.PONumber.ToString() : "";
                    decimal bAmount = bill.GrandTotal != null ? (decimal)bill.GrandTotal : 0;
                    decimal bPriorPaid = bill.AmountPaid != null ? (decimal)bill.AmountPaid : 0;
                    decimal bPayAmount = bill.PaymentAmount != null ? (decimal)bill.PaymentAmount : 0;
                    decimal bDepositAmount = bill.DepositAmount != null ? (decimal)bill.DepositAmount : 0;
                    int bDepositExpenseId = bill.DepositExpenseID != null ? (int)bill.DepositExpenseID : 0;

                    decimal bTotalApplied = bPayAmount + bDepositAmount;
                    if (bTotalApplied <= 0) continue;

                    totalDepositApplied += bDepositAmount;

                    // Retrieve description of deposit expense for GL remark
                    if (bDepositExpenseId > 0 && bDepositAmount > 0 && string.IsNullOrEmpty(depositMemo))
                    {
                        try
                        {
                            string descSql = "SELECT Description FROM dbo.Expenses WHERE ExpenseID = " + bDepositExpenseId;
                            DataSet dsDesc = html.LoadData(descSql);
                            if (dsDesc != null && dsDesc.Tables.Count > 0 && dsDesc.Tables[0].Rows.Count > 0 && dsDesc.Tables[0].Rows[0]["Description"] != DBNull.Value)
                            {
                                depositMemo = dsDesc.Tables[0].Rows[0]["Description"].ToString().Trim();
                            }
                        }
                        catch { }
                    }

                    if (bId > 0)
                    {
                        sbSql.AppendFormat(@"
                            INSERT INTO dbo.TblVendorPaymentDetail (
                                VendorPaymentID, BillID, ExpenseID, VendorID, BillDate, DueDate, BillNumber,
                                PONumber, Amount, AmountPaid, AmountDue, PaymentAmount, OfficalReceiptNo,
                                SystemID, WarehouseID, DomainID, DepositAmount, DepositExpenseID
                            )
                            SELECT 
                                @NewPaymentID, {0}, 0, {1}, b.BillDate, b.PaymentDueDate, N'{2}',
                                N'{3}', b.GrandTotal, (ISNULL(b.AmountPaid, 0) + {4}),
                                CASE
                                    WHEN (ISNULL(NULLIF(b.AmountDue, 0), ISNULL(b.GrandTotal, 0) - ISNULL(b.AmountPaid, 0)) - {4}) < 0 THEN 0
                                    ELSE ISNULL(NULLIF(b.AmountDue, 0), ISNULL(b.GrandTotal, 0) - ISNULL(b.AmountPaid, 0)) - {4}
                                END,
                                {5}, N'{6}',
                                {7}, {8}, '{9}', {10}, {11}
                            FROM dbo.TblBills b WHERE b.BillID = {0};

                            -- Update TblBills record (settles both deposit and cash/bank payment; AmountDue and StatusID are auto-computed)
                            UPDATE dbo.TblBills SET 
                                AmountPaid = ISNULL(AmountPaid, 0) + {4},
                                OfficalReceiptNo = N'{6}'
                            WHERE BillID = {0};

                            -- Update Purchase Order status if linked
                            UPDATE po SET 
                                PaymentStatusID = CASE WHEN (ISNULL(NULLIF(b.AmountDue, 0), ISNULL(b.GrandTotal, 0) - ISNULL(b.AmountPaid, 0)) - {4}) <= 0 THEN 10 ELSE 15 END,
                                PaymentStatus = CASE WHEN (ISNULL(NULLIF(b.AmountDue, 0), ISNULL(b.GrandTotal, 0) - ISNULL(b.AmountPaid, 0)) - {4}) <= 0 THEN 'Paid' ELSE 'Partially Paid' END,
                                PaidAmount = ISNULL(po.PaidAmount, 0) + {4}
                            FROM dbo.TblPurchaseOrder po
                            INNER JOIN dbo.TblBills b ON b.PurchaseOrderID = po.PurchaseOrderID
                            WHERE b.BillID = {0};
                        ",
                            bId,
                            bVendorId,
                            bNumber.Replace("'", "''"),
                            bPoNumber.Replace("'", "''"),
                            bTotalApplied,
                            bPayAmount,
                            paymentNo.Replace("'", "''"),
                            sysId,
                            whId,
                            html.DomainID,
                            bDepositAmount,
                            bDepositExpenseId
                        );
                    }
                    else if (expId > 0)
                    {
                        sbSql.AppendFormat(@"
                            INSERT INTO dbo.TblVendorPaymentDetail (
                                VendorPaymentID, BillID, ExpenseID, VendorID, BillDate, DueDate, BillNumber,
                                PONumber, Amount, AmountPaid, AmountDue, PaymentAmount, OfficalReceiptNo,
                                SystemID, WarehouseID, DomainID, DepositAmount, DepositExpenseID
                            )
                            SELECT 
                                @NewPaymentID, 0, {0}, {1}, e.PaymentDate, ISNULL(e.PaidOnDate, e.PaymentDate), N'{2}',
                                N'{3}', e.TotalAmount, (ISNULL(e.PaidAmount, 0) + {4}),
                                CASE
                                    WHEN (ISNULL(NULLIF(e.BalanceAmount, 0), ISNULL(e.TotalAmount, 0) - ISNULL(e.PaidAmount, 0)) - {4}) < 0 THEN 0
                                    ELSE ISNULL(NULLIF(e.BalanceAmount, 0), ISNULL(e.TotalAmount, 0) - ISNULL(e.PaidAmount, 0)) - {4}
                                END,
                                {5}, N'{6}',
                                {7}, {8}, '{9}', {10}, {11}
                            FROM dbo.Expense_PaymentTransactions e WHERE e.ExpenseID = {0};

                            -- Update dbo.Expense_PaymentTransactions record
                            UPDATE dbo.Expense_PaymentTransactions SET 
                                PaidAmount = ISNULL(PaidAmount, 0) + {4},
                                BalanceAmount = CASE
                                    WHEN (ISNULL(NULLIF(BalanceAmount, 0), ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0)) - {4}) < 0 THEN 0
                                    ELSE ISNULL(NULLIF(BalanceAmount, 0), ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0)) - {4}
                                END,
                                PaymentStatus = CASE
                                    WHEN (ISNULL(NULLIF(BalanceAmount, 0), ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0)) - {4}) <= 0 THEN 'Paid'
                                    ELSE 'Partial Paid'
                                END,
                                PaymentNo = N'{6}'
                            WHERE ExpenseID = {0};

                            -- Update dbo.Expenses record if exists
                            UPDATE dbo.Expenses SET 
                                PaidAmount = ISNULL(PaidAmount, 0) + {4},
                                BalanceAmount = CASE
                                    WHEN (ISNULL(NULLIF(BalanceAmount, 0), ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0)) - {4}) < 0 THEN 0
                                    ELSE ISNULL(NULLIF(BalanceAmount, 0), ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0)) - {4}
                                END,
                                PaymentStatus = CASE
                                    WHEN (ISNULL(NULLIF(BalanceAmount, 0), ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0)) - {4}) <= 0 THEN 'Paid'
                                    ELSE 'Partial Paid'
                                END,
                                PostedPaymentNo = N'{6}'
                            WHERE ExpenseID = {0};
                        ",
                            expId,
                            bVendorId,
                            bNumber.Replace("'", "''"),
                            bPoNumber.Replace("'", "''"),
                            bTotalApplied,
                            bPayAmount,
                            paymentNo.Replace("'", "''"),
                            sysId,
                            whId,
                            html.DomainID,
                            bDepositAmount,
                            bDepositExpenseId
                        );
                    }

                    if (bDepositExpenseId > 0 && bDepositAmount > 0)
                    {
                        sbSql.AppendFormat(@"
                            -- Mark linked Vendor Deposit Expense as applied
                            UPDATE dbo.Expenses SET 
                                PaymentStatus = 'Applied', 
                                BalanceAmount = CASE WHEN ISNULL(BalanceAmount, 0) - {1} < 0 THEN 0 ELSE ISNULL(BalanceAmount, 0) - {1} END
                            WHERE ExpenseID = {0};
                        ", bDepositExpenseId, bDepositAmount);
                    }
                }

                if (string.IsNullOrEmpty(depositMemo))
                {
                    depositMemo = !string.IsNullOrEmpty(memo) ? memo : ("Vendor Deposit applied on Pay Bill " + paymentNo);
                }

                // 4. Record General Ledger Entry in TblAccount_Transaction and TblAccount_JournalEntries (Type: 'Pay Bill')
                decimal totalSettled = totalPaymentAmount + totalDepositApplied;
                sbSql.AppendFormat(@"
                    -- General Ledger Posting for Bill Payment:
                    -- Debit Accounts Payable (dynamically resolved), Credit Cash/Bank, and Credit Vendor Deposit (122000)
                    DECLARE @TxID INT;
                    INSERT INTO dbo.TblAccount_Transaction (
                        InvoiceNo, PVNo, PaymentNo, TransactionNo, PayeeID, VendorID,
                        TransactionDate, Amount, Description,
                        [Type], StatusID, SystemID, WarehouseID, DomainID, CreateByUserID, CreateDate
                    ) VALUES (
                        N'{0}', N'{0}', N'{0}', N'{0}', {12}, {12},
                        '{1:yyyy-MM-dd HH:mm:ss}', {2}, N'Pay Bill - {0}' + CASE WHEN {3} > 0 THEN ' (Applied Deposit: $' + CONVERT(VARCHAR, {3}) + ')' ELSE '' END,
                        'Pay Bill', 10, {4}, {5}, '{6}', {7}, GETDATE()
                    );
                    SET @TxID = SCOPE_IDENTITY();

                    -- Debit Accounts Payable (dynamically resolved from bills being paid, with fallback to 210000 / Account Payable / 464)
                    DECLARE @APAccountID INT, @APAccountNo NVARCHAR(50), @APAccountName NVARCHAR(255);

                    -- 1. Try to find the exact AP Account used by the bills being paid
                    SELECT TOP 1 
                        @APAccountID = je.AccountID, 
                        @APAccountNo = coa.AccountNo, 
                        @APAccountName = coa.AccountName
                    FROM dbo.TblAccount_JournalEntries je
                    INNER JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = je.AccountID
                    WHERE (
                        je.InvoiceNo IN (SELECT BillNumber FROM dbo.TblVendorPaymentDetail WHERE VendorPaymentID = @NewPaymentID)
                        OR je.ExpenseNo IN (SELECT BillNumber FROM dbo.TblVendorPaymentDetail WHERE VendorPaymentID = @NewPaymentID)
                    )
                    AND je.Credit > 0
                    AND (coa.AccountNo LIKE '21%' OR coa.AccountName LIKE '%Payable%')
                    ORDER BY je.EntryID DESC;

                    -- 2. Fallback to standard 210000 in this system/warehouse
                    IF (@APAccountID IS NULL OR @APAccountID = 0)
                    BEGIN
                        SELECT TOP 1 @APAccountID = ChartOfAccountID, @APAccountNo = AccountNo, @APAccountName = AccountName 
                        FROM dbo.TblChartOfAccount WHERE AccountNo = '210000' AND SystemID = {4} AND WarehouseID = {5};
                    END

                    -- 3. Fallback to any Accounts Payable account in this system/warehouse
                    IF (@APAccountID IS NULL OR @APAccountID = 0)
                    BEGIN
                        SELECT TOP 1 @APAccountID = ChartOfAccountID, @APAccountNo = AccountNo, @APAccountName = AccountName 
                        FROM dbo.TblChartOfAccount 
                        WHERE (AccountName LIKE '%Account Payable%' OR AccountName LIKE '%Accounts Payable%')
                          AND (SystemID = {4} OR ISNULL(SystemID, 0) = 0)
                          AND (WarehouseID = {5} OR ISNULL(WarehouseID, 0) = 0);
                    END

                    -- 4. Ultimate fallback
                    IF (@APAccountID IS NULL OR @APAccountID = 0)
                    BEGIN
                        SET @APAccountID = 464;
                        SET @APAccountNo = '210000';
                        SET @APAccountName = 'Account Payable (A/P)';
                    END

                    -- Bank Account Details
                    DECLARE @BankNo NVARCHAR(50), @BankName NVARCHAR(255);
                    SELECT TOP 1 @BankNo = AccountNo, @BankName = AccountName FROM dbo.TblChartOfAccount WHERE ChartOfAccountID = {10};

                    -- Deposit Account Details
                    DECLARE @DepAccountID INT, @DepNo NVARCHAR(50), @DepName NVARCHAR(255);
                    SELECT TOP 1 @DepAccountID = ChartOfAccountID, @DepNo = AccountNo, @DepName = AccountName 
                    FROM dbo.TblChartOfAccount WHERE AccountNo = '122000' AND SystemID = {4} AND WarehouseID = {5};
                    IF (@DepAccountID IS NULL OR @DepAccountID = 0)
                    BEGIN
                        SET @DepAccountID = 432;
                        SET @DepNo = '122000';
                        SET @DepName = 'Vendor Deposit';
                    END

                    INSERT INTO dbo.TblAccount_JournalEntries (
                        TransactionID, AccountID, Debit, Credit, Description,
                        TransactionDate, EntryDate, [Type], InvoiceNo, PaymentNo, CustomerID,
                        SystemID, WarehouseID, DomainID, CreateByUserID, CreateDate, IsPosted,
                        Account
                    ) VALUES (
                        @TxID, @APAccountID, {2}, 0, N'Bill Payment {0} to ' + N'{8}',
                        '{1:yyyy-MM-dd HH:mm:ss}', '{1:yyyy-MM-dd HH:mm:ss}', 'Pay Bill', N'{0}', N'{0}', {12},
                        {4}, {5}, '{6}', {7}, GETDATE(), 1,
                        'Bill-' + ISNULL(@APAccountNo, '210000') + '-' + ISNULL(@APAccountName, 'Account Payable (A/P)')
                    );

                    -- Credit Cash/Bank Account for actual cash disbursement
                    IF ({9} > 0)
                    BEGIN
                        INSERT INTO dbo.TblAccount_JournalEntries (
                            TransactionID, AccountID, Debit, Credit, Description,
                            TransactionDate, EntryDate, [Type], InvoiceNo, PaymentNo, CustomerID,
                            SystemID, WarehouseID, DomainID, CreateByUserID, CreateDate, IsPosted,
                            Account
                        ) VALUES (
                            @TxID, {10}, 0, {9}, N'Bill Payment {0} via ' + N'{11}',
                            '{1:yyyy-MM-dd HH:mm:ss}', '{1:yyyy-MM-dd HH:mm:ss}', 'Pay Bill', N'{0}', N'{0}', {12},
                            {4}, {5}, '{6}', {7}, GETDATE(), 1,
                            'Pay Bill-' + ISNULL(@BankNo, '') + '-' + ISNULL(@BankName, N'{11}')
                        );
                    END

                    -- Credit Vendor Deposit (Account 122000) to clear the balance sheet deposit asset
                    IF ({3} > 0)
                    BEGIN
                        INSERT INTO dbo.TblAccount_JournalEntries (
                            TransactionID, AccountID, Debit, Credit, Description,
                            TransactionDate, EntryDate, [Type], InvoiceNo, PaymentNo, CustomerID,
                            SystemID, WarehouseID, DomainID, CreateByUserID, CreateDate, IsPosted,
                            Account
                        ) VALUES (
                            @TxID, @DepAccountID, 0, {3}, N'{13}',
                            '{1:yyyy-MM-dd HH:mm:ss}', '{1:yyyy-MM-dd HH:mm:ss}', 'Pay Bill', N'{0}', N'{0}', {12},
                            {4}, {5}, '{6}', {7}, GETDATE(), 1,
                            'Vendor Deposit-' + ISNULL(@DepNo, '122000') + '-' + ISNULL(@DepName, 'Vendor Deposit')
                        );
                    END
                ",
                    paymentNo.Replace("'", "''"),
                    dtPaymentDate,
                    totalSettled,
                    totalDepositApplied,
                    sysId,
                    whId,
                    html.DomainID,
                    (html.UserID ?? "200"),
                    payee.Replace("'", "''"),
                    totalPaymentAmount,
                    chartOfAccountId,
                    paymentModeName.Replace("'", "''"),
                    vendorId,
                    depositMemo.Replace("'", "''")
                );

                sbSql.AppendLine("SELECT @NewPaymentID AS PaymentID, N'" + paymentNo + "' AS PaymentNo;");
                sbSql.AppendLine("COMMIT TRANSACTION;");
                sbSql.AppendLine("END TRY");
                sbSql.AppendLine("BEGIN CATCH");
                sbSql.AppendLine("    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;");
                sbSql.AppendLine("    DECLARE @ErrMsg NVARCHAR(MAX) = ERROR_MESSAGE();");
                sbSql.AppendLine("    RAISERROR(@ErrMsg, 16, 1);");
                sbSql.AppendLine("END CATCH");

                DataSet dsResult = html.LoadData(sbSql.ToString());
                int paymentId = 0;
                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    for (int t = dsResult.Tables.Count - 1; t >= 0; t--)
                    {
                        var dt = dsResult.Tables[t];
                        if (dt.Rows.Count > 0 && dt.Columns.Contains("PaymentID"))
                        {
                            int.TryParse(dt.Rows[0]["PaymentID"].ToString(), out paymentId);
                            break;
                        }
                    }
                }

                return Json(new 
                { 
                    success = true, 
                    message = "Payment " + paymentNo + " saved successfully!", 
                    paymentNo = paymentNo, 
                    pvNumber = paymentNo,
                    officialReceiptNo = paymentNo,
                    paymentId = paymentId 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving payment: " + ex.Message });
            }
        }


        private string GetSystemID(HTML html, int? modelSysId = null)
        {
            if (modelSysId.HasValue && modelSysId.Value > 0)
                return modelSysId.Value.ToString();

            if (!string.IsNullOrEmpty(html.SystemID) && html.SystemID != "0")
                return html.SystemID;

            var ctx = System.Web.HttpContext.Current;
            if (ctx != null)
            {
                if (ctx.Session != null && ctx.Session["SystemID"] != null && !string.IsNullOrEmpty(ctx.Session["SystemID"].ToString()) && ctx.Session["SystemID"].ToString() != "0")
                    return ctx.Session["SystemID"].ToString();

                if (ctx.Request != null)
                {
                    if (!string.IsNullOrEmpty(ctx.Request["systemId"]) && ctx.Request["systemId"] != "0")
                        return ctx.Request["systemId"];

                    if (!string.IsNullOrEmpty(ctx.Request["SystemID"]) && ctx.Request["SystemID"] != "0")
                        return ctx.Request["SystemID"];

                    if (ctx.Request.Cookies["SystemID"] != null && !string.IsNullOrEmpty(ctx.Request.Cookies["SystemID"].Value) && ctx.Request.Cookies["SystemID"].Value != "0")
                        return ctx.Request.Cookies["SystemID"].Value;

                    if (ctx.Request.UrlReferrer != null)
                    {
                        var q = System.Web.HttpUtility.ParseQueryString(ctx.Request.UrlReferrer.Query);
                        if (!string.IsNullOrEmpty(q["SystemID"]) && q["SystemID"] != "0")
                            return q["SystemID"];
                        if (!string.IsNullOrEmpty(q["systemId"]) && q["systemId"] != "0")
                            return q["systemId"];
                    }
                }
            }

            return !string.IsNullOrEmpty(html.SystemID) ? html.SystemID : "0";
        }

        private string GetWarehouseID(HTML html, int? modelWhId = null)
        {
            if (modelWhId.HasValue && modelWhId.Value > 0)
                return modelWhId.Value.ToString();

            if (!string.IsNullOrEmpty(html.WarehouseID) && html.WarehouseID != "0")
                return html.WarehouseID;

            var ctx = System.Web.HttpContext.Current;
            if (ctx != null)
            {
                if (ctx.Session != null && ctx.Session["WarehouseID"] != null && !string.IsNullOrEmpty(ctx.Session["WarehouseID"].ToString()) && ctx.Session["WarehouseID"].ToString() != "0")
                    return ctx.Session["WarehouseID"].ToString();

                if (ctx.Request != null)
                {
                    if (!string.IsNullOrEmpty(ctx.Request["warehouseId"]) && ctx.Request["warehouseId"] != "0")
                        return ctx.Request["warehouseId"];

                    if (!string.IsNullOrEmpty(ctx.Request["WarehouseID"]) && ctx.Request["WarehouseID"] != "0")
                        return ctx.Request["WarehouseID"];

                    if (ctx.Request.Cookies["WarehouseID"] != null && !string.IsNullOrEmpty(ctx.Request.Cookies["WarehouseID"].Value) && ctx.Request.Cookies["WarehouseID"].Value != "0")
                        return ctx.Request.Cookies["WarehouseID"].Value;

                    if (ctx.Request.UrlReferrer != null)
                    {
                        var q = System.Web.HttpUtility.ParseQueryString(ctx.Request.UrlReferrer.Query);
                        if (!string.IsNullOrEmpty(q["WarehouseID"]) && q["WarehouseID"] != "0")
                            return q["WarehouseID"];
                        if (!string.IsNullOrEmpty(q["warehouseId"]) && q["warehouseId"] != "0")
                            return q["warehouseId"];
                    }
                }
            }
            return !string.IsNullOrEmpty(html.WarehouseID) ? html.WarehouseID : "0";
        }

        // ─────────────────────────────────────────────────────────────────────
        //  INVOICE / VOUCHER  →  Printable Payment Voucher matching Journal format
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public ActionResult Invoice(int id)
        {
            HTML html = new HTML();
            EnsureStatusColumn(html);
            string sysId = GetSystemID(html);
            string whId = GetWarehouseID(html);

            string sqlHeader = @"
                SELECT TOP 1
                    vp.VendorPaymentID,
                    ISNULL(vp.OfficalReceiptNo, 'PAY-' + CONVERT(VARCHAR, vp.VendorPaymentID)) AS PaymentNo,
                    FORMAT(vp.PaymentDate, 'dd/MM/yyyy') AS PaymentDateFormatted,
                    FORMAT(vp.PaymentDate, 'dd/MM/yyyy hh:mm:ss') AS PaymentDateFullFormatted,
                    ISNULL(vp.Payee, ISNULL(v.VendorName, '')) AS VendorName,
                    ISNULL(v.VendorCode, '') AS VendorCode,
                    ISNULL(v.CompanyName, '') AS VendorCompanyName,
                    ISNULL(pm.PaymentMethod, ISNULL(vp.PaymentMode, 'Cash')) AS PaymentMethod,
                    ISNULL(c.AccountNo + ' - ' + c.AccountName, ISNULL(vp.PaymentAccount, '')) AS PaymentAccount,
                    ISNULL(vp.FromBankName, '') AS BankName,
                    ISNULL(vp.FromAccountNumber, '') AS BankAccountNo,
                    vp.TotalPaymentAmount,
                    ISNULL(vp.PaidBy, '') AS PaidBy,
                    ISNULL(u.FullName, vp.PaidBy) AS PreparedByName,
                    ISNULL(u.[Signature], '') AS PreparedBySignature,
                    CASE WHEN ISNULL(tx.IsPosted, 0) = 1 THEN ISNULL(up.FullName, '') ELSE '' END AS PostedByName,
                    CASE WHEN ISNULL(tx.IsPosted, 0) = 1 THEN ISNULL(up.[Signature], '') ELSE '' END AS PostedBySignature,
                    CASE WHEN ISNULL(tx.IsPosted, 0) = 1 THEN FORMAT(tx.CreateDate, 'dd/MM/yyyy hh:mm:ss') ELSE '' END AS PostedDateFormatted,
                    ISNULL(tx.IsPosted, 0) AS IsPosted,
                    ISNULL(vp.[Status], 'Draft') AS [Status],
                    ISNULL(vp.CheckedBy, '') AS CheckedByName,
                    CASE WHEN vp.CheckedDate IS NOT NULL THEN FORMAT(vp.CheckedDate, 'dd/MM/yyyy') ELSE '' END AS CheckedDateFormatted,
                    ISNULL(vp.VerifiedBy, '') AS VerifiedByName,
                    CASE WHEN vp.VerifiedDate IS NOT NULL THEN FORMAT(vp.VerifiedDate, 'dd/MM/yyyy') ELSE '' END AS VerifiedDateFormatted,
                    ISNULL(vp.Memo, '') AS Memo,
                    ISNULL(vp.Reference, '') AS Reference,
                    ISNULL(vp.ChequeNumber, '') AS ChequeNumber,
                    ISNULL((SELECT TOP 1 cl.FullName
                            FROM dbo.TblClass cl
                            WHERE cl.SystemID = vp.SystemID
                              AND (cl.WarehouseID = vp.WarehouseID OR ISNULL(cl.WarehouseID, 0) = 0)
                            ORDER BY CASE WHEN cl.WarehouseID = vp.WarehouseID THEN 0 ELSE 1 END, cl.ClassID), '') AS CompanyName,
                    ISNULL((SELECT TOP 1 cl.FileLogo
                            FROM dbo.TblClass cl
                            WHERE cl.SystemID = vp.SystemID
                              AND (cl.WarehouseID = vp.WarehouseID OR ISNULL(cl.WarehouseID, 0) = 0)
                            ORDER BY CASE WHEN cl.WarehouseID = vp.WarehouseID THEN 0 ELSE 1 END, cl.ClassID), '') AS CompanyLogo
                FROM dbo.TblVendorPayment vp
                LEFT JOIN dbo.TblVendor v ON v.VendorID = vp.VendorID
                LEFT JOIN dbo.TblPaymentMethod pm ON pm.PaymentMethodID = vp.PaymentModeID
                LEFT JOIN dbo.TblChartOfAccount c ON c.ChartOfAccountID = vp.ChartOfAccountID
                LEFT JOIN dbo.TblUserLogin u ON (u.UserName = vp.PaidBy OR u.FullName = vp.PaidBy)
                LEFT JOIN dbo.TblAccount_Transaction tx ON (tx.InvoiceNo = vp.OfficalReceiptNo AND tx.[Type] = 'Pay Bill' AND ISNULL(tx.IsPosted, 0) = 1)
                LEFT JOIN dbo.TblUserLogin up ON up.UserLoginID = tx.CreateByUserID
                WHERE vp.VendorPaymentID = " + id;

            DataSet dsHeader = html.LoadData(sqlHeader);
            if (dsHeader == null || dsHeader.Tables.Count == 0 || dsHeader.Tables[0].Rows.Count == 0)
            {
                return Content("<div class='alert alert-danger m-3'>Payment voucher #" + id + " not found.</div>");
            }

            ViewBag.PaymentHeader = dsHeader.Tables[0].Rows[0];
            ViewBag.PrintDate = DateTime.Now;

            string prepSig = dsHeader.Tables[0].Rows[0]["PreparedBySignature"]?.ToString();
            string postSig = dsHeader.Tables[0].Rows[0]["PostedBySignature"]?.ToString();
            ViewBag.PreparedSignatureHtml = ResolveSignatureImg(prepSig);
            ViewBag.PostedSignatureHtml = !string.IsNullOrEmpty(postSig) ? ResolveSignatureImg(postSig) : "";

            string sqlDetails = @"
                SELECT 
                    vpd.VendorPaymentDetailID,
                    vpd.BillNumber,
                    vpd.PONumber,
                    FORMAT(vpd.BillDate, 'dd/MM/yyyy') AS BillDateFormatted,
                    FORMAT(vpd.DueDate, 'dd/MM/yyyy') AS DueDateFormatted,
                    vpd.Amount AS GrandTotal,
                    CASE
                        WHEN ISNULL(vpd.AmountPaid, 0) >= ISNULL(vpd.PaymentAmount, 0)
                            THEN ISNULL(vpd.AmountPaid, 0) - ISNULL(vpd.PaymentAmount, 0)
                        ELSE ISNULL(vpd.AmountPaid, 0)
                    END AS PriorPaid,
                    vpd.PaymentAmount,
                    vpd.AmountDue AS RemainBalance
                FROM dbo.TblVendorPaymentDetail vpd
                WHERE vpd.VendorPaymentID = " + id + @"
                ORDER BY vpd.VendorPaymentDetailID ASC";

            DataSet dsDetails = html.LoadData(sqlDetails);
            ViewBag.PaymentDetails = dsDetails != null && dsDetails.Tables.Count > 0 ? dsDetails.Tables[0] : new DataTable();

            string paymentNo = dsHeader.Tables[0].Rows[0]["PaymentNo"].ToString().Replace("'", "''");
            string sqlJournal = @"
                SELECT
                    ISNULL(coa.AccountName, je.Account) AS AccountName,
                    ISNULL(coa.AccountNo, '') AS AccountNo,
                    ISNULL(je.Debit, 0) AS Debit,
                    ISNULL(je.Credit, 0) AS Credit
                FROM dbo.TblAccount_JournalEntries je
                INNER JOIN dbo.TblAccount_Transaction tx ON tx.TransactionID = je.TransactionID
                LEFT JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = je.AccountID
                WHERE tx.InvoiceNo = N'" + paymentNo + @"'
                  AND tx.SystemID = " + sysId + @" AND tx.WarehouseID = " + whId + @"
                  AND tx.[Type] = 'Pay Bill'
                ORDER BY je.EntryID";
            DataSet dsJournal = html.LoadData(sqlJournal);
            ViewBag.PaymentJournal = dsJournal != null && dsJournal.Tables.Count > 0 ? dsJournal.Tables[0] : new DataTable();

            decimal totalAmount = 0;
            if (dsHeader.Tables[0].Rows[0]["TotalPaymentAmount"] != DBNull.Value)
            {
                decimal.TryParse(dsHeader.Tables[0].Rows[0]["TotalPaymentAmount"].ToString(), out totalAmount);
            }
            ViewBag.AmountInWords = NumberToWords(totalAmount);

            return View("~/Views/PayBill/Invoice.cshtml");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  DELETE PAYMENT
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        public JsonResult DeletePayment(int id)
        {
            HTML html = new HTML();
            try
            {
                EnsureStatusColumn(html);
                string statusCheckSql = "SELECT ISNULL([Status], 'Draft') AS [Status] FROM dbo.TblVendorPayment WHERE VendorPaymentID = " + id;
                DataSet dsStatus = html.LoadData(statusCheckSql);
                if (dsStatus != null && dsStatus.Tables.Count > 0 && dsStatus.Tables[0].Rows.Count > 0)
                {
                    string curStatus = dsStatus.Tables[0].Rows[0]["Status"].ToString();
                    if (curStatus != "Draft" && curStatus != "Rejected" && curStatus != "Confirm Rejected")
                    {
                        return Json(new { success = false, message = "Cannot delete a payment voucher with status '" + curStatus + "'. Only Draft or Rejected vouchers can be deleted." });
                    }
                }

                string sql = @"
                    BEGIN TRANSACTION;
                    BEGIN TRY
                        DECLARE @CurStatus NVARCHAR(50);
                        SELECT @CurStatus = ISNULL([Status], 'Draft') FROM dbo.TblVendorPayment WHERE VendorPaymentID = " + id + @";

                        -- Only reverse bill balances if not already reversed via Rejected status
                        IF (@CurStatus NOT IN ('Rejected', 'Confirm Rejected'))
                        BEGIN
                            -- Reverse TblBills amounts (PO Bills)
                            UPDATE b SET 
                                b.AmountPaid = CASE WHEN b.AmountPaid >= vpd.PaymentAmount THEN b.AmountPaid - vpd.PaymentAmount ELSE 0 END,
                                b.OfficalReceiptNo = NULL
                            FROM dbo.TblBills b
                            INNER JOIN dbo.TblVendorPaymentDetail vpd ON vpd.BillID = b.BillID
                            WHERE vpd.VendorPaymentID = " + id + @";

                            -- Reverse linked PO status
                            UPDATE po SET 
                                po.PaymentStatusID = CASE WHEN ISNULL(b.AmountPaid, 0) <= 0 THEN 11 ELSE 15 END,
                                po.PaymentStatus = CASE WHEN ISNULL(b.AmountPaid, 0) <= 0 THEN 'Unpaid' ELSE 'Partially Paid' END,
                                po.PaidAmount = CASE WHEN po.PaidAmount >= vpd.PaymentAmount THEN po.PaidAmount - vpd.PaymentAmount ELSE 0 END
                            FROM dbo.TblPurchaseOrder po
                            INNER JOIN dbo.TblBills b ON b.PurchaseOrderID = po.PurchaseOrderID
                            INNER JOIN dbo.TblVendorPaymentDetail vpd ON vpd.BillID = b.BillID
                            WHERE vpd.VendorPaymentID = " + id + @";

                            -- Reverse Expenses amounts (Expense Bills)
                            UPDATE e SET 
                                e.PaidAmount = CASE WHEN e.PaidAmount >= vpd.PaymentAmount THEN e.PaidAmount - vpd.PaymentAmount ELSE 0 END,
                                e.BalanceAmount = e.TotalAmount - (CASE WHEN e.PaidAmount >= vpd.PaymentAmount THEN e.PaidAmount - vpd.PaymentAmount ELSE 0 END),
                                e.PaymentStatus = CASE 
                                    WHEN (e.TotalAmount - (CASE WHEN e.PaidAmount >= vpd.PaymentAmount THEN e.PaidAmount - vpd.PaymentAmount ELSE 0 END)) <= 0 THEN 'Paid'
                                    WHEN (CASE WHEN e.PaidAmount >= vpd.PaymentAmount THEN e.PaidAmount - vpd.PaymentAmount ELSE 0 END) > 0 THEN 'Partial Paid'
                                    ELSE 'Unpaid' 
                                END,
                                e.PostedPaymentNo = NULL
                            FROM dbo.Expenses e
                            INNER JOIN dbo.TblVendorPaymentDetail vpd ON (
                                (ISNULL(vpd.ExpenseID, 0) > 0 AND vpd.ExpenseID = e.ExpenseID)
                                OR (ISNULL(vpd.ExpenseID, 0) = 0 AND (vpd.BillNumber = e.ExpenseNo OR vpd.BillNumber = e.[No]))
                            )
                            WHERE vpd.VendorPaymentID = " + id + @";

                            UPDATE ep SET 
                                ep.PaidAmount = CASE WHEN ep.PaidAmount >= vpd.PaymentAmount THEN ep.PaidAmount - vpd.PaymentAmount ELSE 0 END,
                                ep.BalanceAmount = ep.TotalAmount - (CASE WHEN ep.PaidAmount >= vpd.PaymentAmount THEN ep.PaidAmount - vpd.PaymentAmount ELSE 0 END),
                                ep.PaymentStatus = CASE 
                                    WHEN (ep.TotalAmount - (CASE WHEN ep.PaidAmount >= vpd.PaymentAmount THEN ep.PaidAmount - vpd.PaymentAmount ELSE 0 END)) <= 0 THEN 'Paid'
                                    WHEN (CASE WHEN ep.PaidAmount >= vpd.PaymentAmount THEN ep.PaidAmount - vpd.PaymentAmount ELSE 0 END) > 0 THEN 'Partial Paid'
                                    ELSE 'Unpaid' 
                                END,
                                ep.PaymentNo = NULL
                            FROM dbo.Expense_PaymentTransactions ep
                            INNER JOIN dbo.TblVendorPaymentDetail vpd ON (
                                (ISNULL(vpd.ExpenseID, 0) > 0 AND vpd.ExpenseID = ep.ExpenseID)
                                OR (ISNULL(vpd.ExpenseID, 0) = 0 AND (vpd.BillNumber = ep.ExpenseNo OR vpd.BillNumber = ep.[No]))
                            )
                            WHERE vpd.VendorPaymentID = " + id + @";

                            -- Restore applied vendor deposit expense
                            UPDATE dep SET 
                                dep.BalanceAmount = ISNULL(dep.BalanceAmount, 0) + vpd.DepositAmount,
                                dep.PaymentStatus = 'Paid'
                            FROM dbo.Expenses dep
                            INNER JOIN dbo.TblVendorPaymentDetail vpd ON vpd.DepositExpenseID = dep.ExpenseID
                            WHERE vpd.VendorPaymentID = " + id + @" AND ISNULL(vpd.DepositAmount, 0) > 0;
                        END

                        -- Delete General Ledger transaction
                        DECLARE @RcptNo NVARCHAR(100);
                        SELECT @RcptNo = OfficalReceiptNo FROM dbo.TblVendorPayment WHERE VendorPaymentID = " + id + @";
                        IF (@RcptNo IS NOT NULL AND @RcptNo <> '')
                        BEGIN
                            DECLARE @TxID INT;
                            SELECT @TxID = TransactionID FROM dbo.TblAccount_Transaction WHERE (InvoiceNo = @RcptNo OR PVNo = @RcptNo) AND [Type] IN ('Pay Bill', 'Expense Payment');
                            IF (@TxID IS NOT NULL AND @TxID > 0)
                            BEGIN
                                DELETE FROM dbo.TblAccount_JournalEntries WHERE TransactionID = @TxID;
                                DELETE FROM dbo.TblAccount_Transaction WHERE TransactionID = @TxID;
                            END
                        END

                        DELETE FROM dbo.TblVendorPaymentDetail WHERE VendorPaymentID = " + id + @";
                        DELETE FROM dbo.TblVendorPayment WHERE VendorPaymentID = " + id + @";

                        COMMIT TRANSACTION;
                        SELECT 2 AS Result, 'Payment deleted successfully' AS Message;
                    END TRY
                    BEGIN CATCH
                        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                        DECLARE @Err NVARCHAR(MAX) = ERROR_MESSAGE();
                        RAISERROR(@Err, 16, 1);
                    END CATCH";

                DataSet ds = html.LoadData(sql);
                return Json(new { success = true, Result = 2, message = "Payment voucher deleted and bill balances restored." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, Result = 1, message = "Error deleting payment: " + ex.Message });
            }
        }

        private string ResolveSignatureImg(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return "";
            try
            {
                var fileNames = fileName.Split('~');
                var folderPath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads/Users");
                var fullPath = System.IO.Path.Combine(folderPath, fileNames[0].Trim());
                if (System.IO.File.Exists(fullPath))
                {
                    byte[] imageArray = System.IO.File.ReadAllBytes(fullPath);
                    string base64 = Convert.ToBase64String(imageArray);
                    string ext = System.IO.Path.GetExtension(fullPath).Replace(".", "").ToLower();
                    if (ext == "svg") ext = "svg+xml";
                    return "<img src=\"data:image/" + ext + ";base64," + base64 + "\" height=\"35\" style=\"max-height:35px; max-width:100%; object-fit:contain;\" />";
                }
            }
            catch { }
            return "";
        }

        private static string NumberToWords(decimal amount)
        {
            amount = Math.Round(amount, 2);

            if (amount == 0)
                return "Zero Dollars Only";

            long dollars = (long)amount;
            int cents = (int)((amount - dollars) * 100);

            string dollarWords = ConvertIntToWords(dollars);
            string result = "";

            if (dollars > 0)
            {
                result += dollarWords + (dollars == 1 ? " Dollar" : " Dollars");
            }

            if (cents > 0)
            {
                if (dollars > 0)
                    result += " and ";

                result += ConvertIntToWords(cents) + (cents == 1 ? " Cent" : " Cents");
            }

            return result + " Only";
        }

        private static string ConvertIntToWords(long number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + ConvertIntToWords(Math.Abs(number));

            string words = "";

            if (number / 1000000000 > 0)
            {
                words += ConvertIntToWords(number / 1000000000) + " Billion ";
                number %= 1000000000;
            }

            if (number / 1000000 > 0)
            {
                words += ConvertIntToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }

            if (number / 1000 > 0)
            {
                words += ConvertIntToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if (number / 100 > 0)
            {
                words += ConvertIntToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                string[] unitsMap =
                {
                    "Zero","One","Two","Three","Four","Five","Six","Seven","Eight","Nine",
                    "Ten","Eleven","Twelve","Thirteen","Fourteen","Fifteen",
                    "Sixteen","Seventeen","Eighteen","Nineteen"
                };

                string[] tensMap =
                {
                    "Zero","Ten","Twenty","Thirty","Forty","Fifty","Sixty","Seventy","Eighty","Ninety"
                };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  APPROVAL / STATUS ACTION  →  Matches Bill status workflow
        // ─────────────────────────────────────────────────────────────────────
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [AllowAnonymous]
        public ActionResult ApprovalPaymentVoucher(int paymentId = 0, string status = "", string paymentNo = "", int ExpenseID = 0, string Status = "", string PaymentNo = "")
        {
            HTML html = new HTML();
            try
            {
                EnsureStatusColumn(html);
                int pId = paymentId > 0 ? paymentId : ExpenseID;
                string st = !string.IsNullOrEmpty(status) ? status : Status;
                string pNo = !string.IsNullOrEmpty(paymentNo) ? paymentNo : PaymentNo;

                string safeStatus = (st ?? "Draft").Replace("'", "''");
                string userName = (html.UserFullName ?? html.UserName ?? "Admin").Replace("'", "''");

                string updateSql = "";
                if (safeStatus == "Rejected" || safeStatus == "Confirm Rejected")
                {
                    updateSql = $@"
                        BEGIN TRANSACTION;
                        BEGIN TRY
                            DECLARE @OldStatus NVARCHAR(50);
                            SELECT @OldStatus = ISNULL([Status], 'Draft') FROM dbo.TblVendorPayment WHERE VendorPaymentID = {pId};

                            -- Only reverse if currently in active state (not already rejected)
                            IF (@OldStatus NOT IN ('Rejected', 'Confirm Rejected'))
                            BEGIN
                                -- 1. Restore TblBills amounts (PO Bills)
                                UPDATE b SET 
                                    b.AmountPaid = CASE WHEN b.AmountPaid >= vpd.PaymentAmount THEN b.AmountPaid - vpd.PaymentAmount ELSE 0 END,
                                    b.OfficalReceiptNo = NULL
                                FROM dbo.TblBills b
                                INNER JOIN dbo.TblVendorPaymentDetail vpd ON vpd.BillID = b.BillID
                                WHERE vpd.VendorPaymentID = {pId};

                                -- 2. Restore linked PO status
                                UPDATE po SET 
                                    po.PaymentStatusID = CASE WHEN ISNULL(b.AmountPaid, 0) <= 0 THEN 11 ELSE 15 END,
                                    po.PaymentStatus = CASE WHEN ISNULL(b.AmountPaid, 0) <= 0 THEN 'Unpaid' ELSE 'Partially Paid' END,
                                    po.PaidAmount = CASE WHEN po.PaidAmount >= vpd.PaymentAmount THEN po.PaidAmount - vpd.PaymentAmount ELSE 0 END
                                FROM dbo.TblPurchaseOrder po
                                INNER JOIN dbo.TblBills b ON b.PurchaseOrderID = po.PurchaseOrderID
                                INNER JOIN dbo.TblVendorPaymentDetail vpd ON vpd.BillID = b.BillID
                                WHERE vpd.VendorPaymentID = {pId};

                                -- 3. Restore Expenses amounts (Expense Bills)
                                UPDATE e SET 
                                    e.PaidAmount = CASE WHEN e.PaidAmount >= vpd.PaymentAmount THEN e.PaidAmount - vpd.PaymentAmount ELSE 0 END,
                                    e.BalanceAmount = e.TotalAmount - (CASE WHEN e.PaidAmount >= vpd.PaymentAmount THEN e.PaidAmount - vpd.PaymentAmount ELSE 0 END),
                                    e.PaymentStatus = CASE 
                                        WHEN (e.TotalAmount - (CASE WHEN e.PaidAmount >= vpd.PaymentAmount THEN e.PaidAmount - vpd.PaymentAmount ELSE 0 END)) <= 0 THEN 'Paid'
                                        WHEN (CASE WHEN e.PaidAmount >= vpd.PaymentAmount THEN e.PaidAmount - vpd.PaymentAmount ELSE 0 END) > 0 THEN 'Partial Paid'
                                        ELSE 'Unpaid' 
                                    END,
                                    e.PostedPaymentNo = NULL
                                FROM dbo.Expenses e
                                INNER JOIN dbo.TblVendorPaymentDetail vpd ON (
                                    (ISNULL(vpd.ExpenseID, 0) > 0 AND vpd.ExpenseID = e.ExpenseID)
                                    OR (ISNULL(vpd.ExpenseID, 0) = 0 AND (vpd.BillNumber = e.ExpenseNo OR vpd.BillNumber = e.[No]))
                                )
                                WHERE vpd.VendorPaymentID = {pId};

                                UPDATE ep SET 
                                    ep.PaidAmount = CASE WHEN ep.PaidAmount >= vpd.PaymentAmount THEN ep.PaidAmount - vpd.PaymentAmount ELSE 0 END,
                                    ep.BalanceAmount = ep.TotalAmount - (CASE WHEN ep.PaidAmount >= vpd.PaymentAmount THEN ep.PaidAmount - vpd.PaymentAmount ELSE 0 END),
                                    ep.PaymentStatus = CASE 
                                        WHEN (ep.TotalAmount - (CASE WHEN ep.PaidAmount >= vpd.PaymentAmount THEN ep.PaidAmount - vpd.PaymentAmount ELSE 0 END)) <= 0 THEN 'Paid'
                                        WHEN (CASE WHEN ep.PaidAmount >= vpd.PaymentAmount THEN ep.PaidAmount - vpd.PaymentAmount ELSE 0 END) > 0 THEN 'Partial Paid'
                                        ELSE 'Unpaid' 
                                    END,
                                    ep.PaymentNo = NULL
                                FROM dbo.Expense_PaymentTransactions ep
                                INNER JOIN dbo.TblVendorPaymentDetail vpd ON (
                                    (ISNULL(vpd.ExpenseID, 0) > 0 AND vpd.ExpenseID = ep.ExpenseID)
                                    OR (ISNULL(vpd.ExpenseID, 0) = 0 AND (vpd.BillNumber = ep.ExpenseNo OR vpd.BillNumber = ep.[No]))
                                )
                                WHERE vpd.VendorPaymentID = {pId};

                                -- 4. Restore applied vendor deposit expense
                                UPDATE dep SET 
                                    dep.BalanceAmount = ISNULL(dep.BalanceAmount, 0) + vpd.DepositAmount,
                                    dep.PaymentStatus = 'Paid'
                                FROM dbo.Expenses dep
                                INNER JOIN dbo.TblVendorPaymentDetail vpd ON vpd.DepositExpenseID = dep.ExpenseID
                                WHERE vpd.VendorPaymentID = {pId} AND ISNULL(vpd.DepositAmount, 0) > 0;

                                -- 5. Mark General Ledger entries as Unposted / Cancelled
                                DECLARE @RcptNo NVARCHAR(100);
                                SELECT @RcptNo = OfficalReceiptNo FROM dbo.TblVendorPayment WHERE VendorPaymentID = {pId};
                                IF (@RcptNo IS NOT NULL AND @RcptNo <> '')
                                BEGIN
                                    UPDATE dbo.TblAccount_JournalEntries 
                                    SET IsPosted = 0 
                                    WHERE TransactionID IN (SELECT TransactionID FROM dbo.TblAccount_Transaction WHERE (InvoiceNo = @RcptNo OR PVNo = @RcptNo) AND [Type] IN ('Pay Bill', 'Expense Payment'));

                                    UPDATE dbo.TblAccount_Transaction 
                                    SET StatusID = 14, IsPosted = 0 
                                    WHERE (InvoiceNo = @RcptNo OR PVNo = @RcptNo) AND [Type] IN ('Pay Bill', 'Expense Payment');
                                END
                            END

                            -- Update status on payment header
                            UPDATE dbo.TblVendorPayment
                            SET [Status] = '{safeStatus}',
                                CheckedBy = CASE WHEN '{safeStatus}' = 'Rejected' THEN N'{userName}' ELSE CheckedBy END,
                                CheckedDate = CASE WHEN '{safeStatus}' = 'Rejected' THEN GETDATE() ELSE CheckedDate END,
                                VerifiedBy = CASE WHEN '{safeStatus}' = 'Confirm Rejected' THEN N'{userName}' ELSE VerifiedBy END,
                                VerifiedDate = CASE WHEN '{safeStatus}' = 'Confirm Rejected' THEN GETDATE() ELSE VerifiedDate END
                            WHERE VendorPaymentID = {pId};

                            COMMIT TRANSACTION;
                        END TRY
                        BEGIN CATCH
                            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                            DECLARE @ErrMsg NVARCHAR(MAX) = ERROR_MESSAGE();
                            RAISERROR(@ErrMsg, 16, 1);
                        END CATCH;";
                }
                else if (safeStatus == "Checked")
                {
                    updateSql = $@"
                        BEGIN TRANSACTION;
                        BEGIN TRY
                            DECLARE @OldStatusChecked NVARCHAR(50);
                            SELECT @OldStatusChecked = ISNULL([Status], 'Draft') FROM dbo.TblVendorPayment WHERE VendorPaymentID = {pId};

                            -- If previously rejected, re-apply the settlement to bills and re-post GL
                            IF (@OldStatusChecked IN ('Rejected', 'Confirm Rejected'))
                            BEGIN
                                UPDATE b SET 
                                    b.AmountPaid = ISNULL(b.AmountPaid, 0) + vpd.PaymentAmount,
                                    b.OfficalReceiptNo = vp.OfficalReceiptNo
                                FROM dbo.TblBills b
                                INNER JOIN dbo.TblVendorPaymentDetail vpd ON vpd.BillID = b.BillID
                                INNER JOIN dbo.TblVendorPayment vp ON vp.VendorPaymentID = vpd.VendorPaymentID
                                WHERE vpd.VendorPaymentID = {pId};

                                UPDATE ep SET 
                                    ep.PaidAmount = ISNULL(ep.PaidAmount, 0) + vpd.PaymentAmount,
                                    ep.BalanceAmount = CASE WHEN ep.TotalAmount - (ISNULL(ep.PaidAmount, 0) + vpd.PaymentAmount) < 0 THEN 0 ELSE ep.TotalAmount - (ISNULL(ep.PaidAmount, 0) + vpd.PaymentAmount) END,
                                    ep.PaymentStatus = CASE WHEN (ep.TotalAmount - (ISNULL(ep.PaidAmount, 0) + vpd.PaymentAmount)) <= 0 THEN 'Paid' ELSE 'Partial Paid' END,
                                    ep.PaymentNo = vp.OfficalReceiptNo
                                FROM dbo.Expense_PaymentTransactions ep
                                INNER JOIN dbo.TblVendorPaymentDetail vpd ON (
                                    (ISNULL(vpd.ExpenseID, 0) > 0 AND vpd.ExpenseID = ep.ExpenseID)
                                    OR (ISNULL(vpd.ExpenseID, 0) = 0 AND (vpd.BillNumber = ep.ExpenseNo OR vpd.BillNumber = ep.[No]))
                                )
                                INNER JOIN dbo.TblVendorPayment vp ON vp.VendorPaymentID = vpd.VendorPaymentID
                                WHERE vpd.VendorPaymentID = {pId};

                                UPDATE e SET 
                                    e.PaidAmount = ISNULL(e.PaidAmount, 0) + vpd.PaymentAmount,
                                    e.BalanceAmount = CASE WHEN e.TotalAmount - (ISNULL(e.PaidAmount, 0) + vpd.PaymentAmount) < 0 THEN 0 ELSE e.TotalAmount - (ISNULL(e.PaidAmount, 0) + vpd.PaymentAmount) END,
                                    e.PaymentStatus = CASE WHEN (e.TotalAmount - (ISNULL(e.PaidAmount, 0) + vpd.PaymentAmount)) <= 0 THEN 'Paid' ELSE 'Partial Paid' END,
                                    e.PostedPaymentNo = vp.OfficalReceiptNo
                                FROM dbo.Expenses e
                                INNER JOIN dbo.TblVendorPaymentDetail vpd ON (
                                    (ISNULL(vpd.ExpenseID, 0) > 0 AND vpd.ExpenseID = e.ExpenseID)
                                    OR (ISNULL(vpd.ExpenseID, 0) = 0 AND (vpd.BillNumber = e.ExpenseNo OR vpd.BillNumber = e.[No]))
                                )
                                INNER JOIN dbo.TblVendorPayment vp ON vp.VendorPaymentID = vpd.VendorPaymentID
                                WHERE vpd.VendorPaymentID = {pId};
                            END

                            -- Activate GL entries
                            DECLARE @RcptNoChecked NVARCHAR(100);
                            SELECT @RcptNoChecked = OfficalReceiptNo FROM dbo.TblVendorPayment WHERE VendorPaymentID = {pId};
                            IF (@RcptNoChecked IS NOT NULL AND @RcptNoChecked <> '')
                            BEGIN
                                UPDATE dbo.TblAccount_JournalEntries 
                                SET IsPosted = 1 
                                WHERE TransactionID IN (SELECT TransactionID FROM dbo.TblAccount_Transaction WHERE (InvoiceNo = @RcptNoChecked OR PVNo = @RcptNoChecked) AND [Type] IN ('Pay Bill', 'Expense Payment'));

                                UPDATE dbo.TblAccount_Transaction 
                                SET StatusID = 10, IsPosted = 1 
                                WHERE (InvoiceNo = @RcptNoChecked OR PVNo = @RcptNoChecked) AND [Type] IN ('Pay Bill', 'Expense Payment');
                            END

                            UPDATE dbo.TblVendorPayment
                            SET [Status] = '{safeStatus}',
                                CheckedBy = N'{userName}',
                                CheckedDate = GETDATE()
                            WHERE VendorPaymentID = {pId};

                            COMMIT TRANSACTION;
                        END TRY
                        BEGIN CATCH
                            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                            DECLARE @ErrMsgC NVARCHAR(MAX) = ERROR_MESSAGE();
                            RAISERROR(@ErrMsgC, 16, 1);
                        END CATCH;";
                }
                else if (safeStatus == "Confirm Verified")
                {
                    updateSql = $@"
                        BEGIN TRANSACTION;
                        BEGIN TRY
                            DECLARE @OldStatusVerified NVARCHAR(50);
                            SELECT @OldStatusVerified = ISNULL([Status], 'Draft') FROM dbo.TblVendorPayment WHERE VendorPaymentID = {pId};

                            -- If previously rejected, re-apply the settlement to bills
                            IF (@OldStatusVerified IN ('Rejected', 'Confirm Rejected'))
                            BEGIN
                                UPDATE b SET 
                                    b.AmountPaid = ISNULL(b.AmountPaid, 0) + vpd.PaymentAmount,
                                    b.OfficalReceiptNo = vp.OfficalReceiptNo
                                FROM dbo.TblBills b
                                INNER JOIN dbo.TblVendorPaymentDetail vpd ON vpd.BillID = b.BillID
                                INNER JOIN dbo.TblVendorPayment vp ON vp.VendorPaymentID = vpd.VendorPaymentID
                                WHERE vpd.VendorPaymentID = {pId};

                                UPDATE ep SET 
                                    ep.PaidAmount = ISNULL(ep.PaidAmount, 0) + vpd.PaymentAmount,
                                    ep.BalanceAmount = CASE WHEN ep.TotalAmount - (ISNULL(ep.PaidAmount, 0) + vpd.PaymentAmount) < 0 THEN 0 ELSE ep.TotalAmount - (ISNULL(ep.PaidAmount, 0) + vpd.PaymentAmount) END,
                                    ep.PaymentStatus = CASE WHEN (ep.TotalAmount - (ISNULL(ep.PaidAmount, 0) + vpd.PaymentAmount)) <= 0 THEN 'Paid' ELSE 'Partial Paid' END,
                                    ep.PaymentNo = vp.OfficalReceiptNo
                                FROM dbo.Expense_PaymentTransactions ep
                                INNER JOIN dbo.TblVendorPaymentDetail vpd ON (
                                    (ISNULL(vpd.ExpenseID, 0) > 0 AND vpd.ExpenseID = ep.ExpenseID)
                                    OR (ISNULL(vpd.ExpenseID, 0) = 0 AND (vpd.BillNumber = ep.ExpenseNo OR vpd.BillNumber = ep.[No]))
                                )
                                INNER JOIN dbo.TblVendorPayment vp ON vp.VendorPaymentID = vpd.VendorPaymentID
                                WHERE vpd.VendorPaymentID = {pId};

                                UPDATE e SET 
                                    e.PaidAmount = ISNULL(e.PaidAmount, 0) + vpd.PaymentAmount,
                                    e.BalanceAmount = CASE WHEN e.TotalAmount - (ISNULL(e.PaidAmount, 0) + vpd.PaymentAmount) < 0 THEN 0 ELSE e.TotalAmount - (ISNULL(e.PaidAmount, 0) + vpd.PaymentAmount) END,
                                    e.PaymentStatus = CASE WHEN (e.TotalAmount - (ISNULL(e.PaidAmount, 0) + vpd.PaymentAmount)) <= 0 THEN 'Paid' ELSE 'Partial Paid' END,
                                    e.PostedPaymentNo = vp.OfficalReceiptNo
                                FROM dbo.Expenses e
                                INNER JOIN dbo.TblVendorPaymentDetail vpd ON (
                                    (ISNULL(vpd.ExpenseID, 0) > 0 AND vpd.ExpenseID = e.ExpenseID)
                                    OR (ISNULL(vpd.ExpenseID, 0) = 0 AND (vpd.BillNumber = e.ExpenseNo OR vpd.BillNumber = e.[No]))
                                )
                                INNER JOIN dbo.TblVendorPayment vp ON vp.VendorPaymentID = vpd.VendorPaymentID
                                WHERE vpd.VendorPaymentID = {pId};
                            END

                            -- Post General Ledger entries
                            DECLARE @RcptNoVerified NVARCHAR(100);
                            SELECT @RcptNoVerified = OfficalReceiptNo FROM dbo.TblVendorPayment WHERE VendorPaymentID = {pId};
                            IF (@RcptNoVerified IS NOT NULL AND @RcptNoVerified <> '')
                            BEGIN
                                UPDATE dbo.TblAccount_JournalEntries 
                                SET IsPosted = 1 
                                WHERE TransactionID IN (SELECT TransactionID FROM dbo.TblAccount_Transaction WHERE (InvoiceNo = @RcptNoVerified OR PVNo = @RcptNoVerified) AND [Type] IN ('Pay Bill', 'Expense Payment'));

                                UPDATE dbo.TblAccount_Transaction 
                                SET StatusID = 10, IsPosted = 1 
                                WHERE (InvoiceNo = @RcptNoVerified OR PVNo = @RcptNoVerified) AND [Type] IN ('Pay Bill', 'Expense Payment');
                            END

                            UPDATE dbo.TblVendorPayment
                            SET [Status] = '{safeStatus}',
                                VerifiedBy = N'{userName}',
                                VerifiedDate = GETDATE()
                            WHERE VendorPaymentID = {pId};

                            COMMIT TRANSACTION;
                        END TRY
                        BEGIN CATCH
                            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                            DECLARE @ErrMsgV NVARCHAR(MAX) = ERROR_MESSAGE();
                            RAISERROR(@ErrMsgV, 16, 1);
                        END CATCH;";
                }
                else
                {
                    // Reset or draft
                    updateSql = $@"
                        UPDATE dbo.TblVendorPayment
                        SET [Status] = '{safeStatus}'
                        WHERE VendorPaymentID = {pId};";
                }

                SqlResult res = html.SaveChangeData(updateSql, false, false);
                bool isSuccess = res != null && res.Result == ResultType.Successful;
                string msg = isSuccess ? "Payment voucher status updated to " + safeStatus : (res != null && !string.IsNullOrEmpty(res.Message) ? res.Message : "Failed to update status");
                return Json(new { 
                    Result = isSuccess ? 2 : 1, 
                    result = isSuccess ? 2 : 1, 
                    success = isSuccess, 
                    Message = msg, 
                    message = msg 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = 1, result = 1, success = false, message = ex.Message, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
