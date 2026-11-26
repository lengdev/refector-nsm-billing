// using MVC;
// using Newtonsoft.Json;
// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.Linq;
// using System.Text;
// using System.Web.Mvc;
// using Utilities;

// namespace NSMSOLUTION.Controllers.Finance
// {
//     public class PayBillController : Controller
//     {
      
//         private string GetSystemID(HTML html, int? modelSysId = null)
//         {
//             if (modelSysId.HasValue && modelSysId.Value > 0)
//                 return modelSysId.Value.ToString();

//             if (!string.IsNullOrEmpty(html.SystemID) && html.SystemID != "0")
//                 return html.SystemID;

//             var ctx = System.Web.HttpContext.Current;
//             if (ctx != null)
//             {
//                 if (ctx.Session != null && ctx.Session["SystemID"] != null && !string.IsNullOrEmpty(ctx.Session["SystemID"].ToString()) && ctx.Session["SystemID"].ToString() != "0")
//                     return ctx.Session["SystemID"].ToString();

//                 if (ctx.Request != null)
//                 {
//                     if (!string.IsNullOrEmpty(ctx.Request["SystemID"]) && ctx.Request["SystemID"] != "0")
//                         return ctx.Request["SystemID"];

//                     if (ctx.Request.Cookies["SystemID"] != null && !string.IsNullOrEmpty(ctx.Request.Cookies["SystemID"].Value) && ctx.Request.Cookies["SystemID"].Value != "0")
//                         return ctx.Request.Cookies["SystemID"].Value;

//                     if (ctx.Request.UrlReferrer != null)
//                     {
//                         var q = System.Web.HttpUtility.ParseQueryString(ctx.Request.UrlReferrer.Query);
//                         if (!string.IsNullOrEmpty(q["SystemID"]) && q["SystemID"] != "0")
//                             return q["SystemID"];
//                     }
//                 }
//             }
//             return "13";
//         }

//         private string GetWarehouseID(HTML html, int? modelWhId = null)
//         {
//             if (modelWhId.HasValue && modelWhId.Value > 0)
//                 return modelWhId.Value.ToString();

//             if (!string.IsNullOrEmpty(html.WarehouseID) && html.WarehouseID != "0")
//                 return html.WarehouseID;

//             var ctx = System.Web.HttpContext.Current;
//             if (ctx != null)
//             {
//                 if (ctx.Session != null && ctx.Session["WarehouseID"] != null && !string.IsNullOrEmpty(ctx.Session["WarehouseID"].ToString()) && ctx.Session["WarehouseID"].ToString() != "0")
//                     return ctx.Session["WarehouseID"].ToString();

//                 if (ctx.Request != null)
//                 {
//                     if (!string.IsNullOrEmpty(ctx.Request["WarehouseID"]) && ctx.Request["WarehouseID"] != "0")
//                         return ctx.Request["WarehouseID"];

//                     if (ctx.Request.Cookies["WarehouseID"] != null && !string.IsNullOrEmpty(ctx.Request.Cookies["WarehouseID"].Value) && ctx.Request.Cookies["WarehouseID"].Value != "0")
//                         return ctx.Request.Cookies["WarehouseID"].Value;

//                     if (ctx.Request.UrlReferrer != null)
//                     {
//                         var q = System.Web.HttpUtility.ParseQueryString(ctx.Request.UrlReferrer.Query);
//                         if (!string.IsNullOrEmpty(q["WarehouseID"]) && q["WarehouseID"] != "0")
//                             return q["WarehouseID"];
//                     }
//                 }
//             }
//             return "16";
//         }

//         // ─────────────────────────────────────────────────────────────────────
//         //  INDEX  →  Default shows Bills to Pay (Open Bills), with Tab for History
//         // ─────────────────────────────────────────────────────────────────────
//         [Authorize]
//         public ActionResult Index(string view = "unpaid")
//         {
//             HTML html = new HTML();
//             string sysId = GetSystemID(html);
//             string whId = GetWarehouseID(html);

//             html.DialogBoxWidth = "Auto";
//             html.TblAlign = "auto";
//             html.Cellpadding = 4;
//             html.AllowAdd = true;
//             html.AllowEdit = true;
//             html.AllowDelete = true;

//             ViewBag.CurrentView = view;
//             ViewBag.SystemID = sysId;
//             ViewBag.WarehouseID = whId;

//             if (view == "payments")
//             {
//                 // ── PAYMENT HISTORY VIEW ──
//                 html.ReportTitle = "Payment History (Vendor Payments)";
//                 html.AddButton = "Pay Bill";
//                 html.AddURL = "PayBill/Create?systemId=" + sysId + "&warehouseId=" + whId;
//                 html.TableName = "dbo.TblVendorPayment";
//                 html.PrimaryField = "VendorPaymentID";
//                 html.OrderBy = "VendorPaymentID desc";

//                 html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
//                 {
//                     DefaultValueDate = false,
//                     DateRangePicker = true,
//                     LabelFromDate = "Payment Date:",
//                     LabelAlign = "left",
//                     DBField = "vp.PaymentDate",
//                     DateTimePickerWithRanges = true
//                 });

//                 html.HtmlAddControls.Add(new HtmlAddControls()
//                 {
//                     ID = "txtReceiptNo",
//                     ISAutoComplete = true,
//                     Label = "Payment No:",
//                     LabelAlign = "left",
//                     DBField = "vp.OfficalReceiptNo",
//                     SQLAutoComplete = "SELECT DISTINCT TOP 10 OfficalReceiptNo FROM dbo.TblVendorPayment WHERE LOWER(OfficalReceiptNo) LIKE @SearchText AND SystemID = " + sysId + " AND WarehouseID = " + whId + " ORDER BY OfficalReceiptNo DESC"
//                 });

//                 html.HtmlAddControls.Add(new HtmlAddControls()
//                 {
//                     ID = "txtPayee",
//                     ISAutoComplete = true,
//                     Label = "Vendor / Payee:",
//                     LabelAlign = "left",
//                     DBField = "vp.Payee",
//                     SQLAutoComplete = "SELECT DISTINCT TOP 10 Payee FROM dbo.TblVendorPayment WHERE LOWER(Payee) LIKE @SearchText AND SystemID = " + sysId + " AND WarehouseID = " + whId + " ORDER BY Payee"
//                 });

//                 html.HtmlAddControls.Add(new HtmlAddControls()
//                 {
//                     ID = "txtPaymentModeID",
//                     Label = "Payment Method:",
//                     LabelAlign = "left",
//                     DBField = "vp.PaymentModeID",
//                     SQL_CBO = "SELECT DISTINCT pm.PaymentMethodID, pm.PaymentMethod FROM dbo.TblPaymentMethod pm WHERE pm.SystemID = " + sysId + " AND pm.WarehouseID = " + whId + " ORDER BY pm.PaymentMethod",
//                     DisplayText = "PaymentMethod",
//                     Value = "PaymentMethodID",
//                     ComboBox = true
//                 });

//                 string sqlPayments = @"
//                     SELECT 
//                         ROW_NUMBER() OVER (ORDER BY vp.VendorPaymentID DESC) AS [No],
//                         vp.VendorPaymentID,
//                         '<a class=""text-primary font-weight-bold"" style=""cursor: pointer; text-decoration: none;"" onclick=""showPaymentVoucher(' + CONVERT(VARCHAR, vp.VendorPaymentID) + ', ''' + ISNULL(vp.OfficalReceiptNo, '') + ''')"">' + ISNULL(vp.OfficalReceiptNo, 'PAY-' + CONVERT(VARCHAR, vp.VendorPaymentID)) + '</a>' AS PaymentNoLink,
//                         ISNULL(vp.OfficalReceiptNo, 'PAY-' + CONVERT(VARCHAR, vp.VendorPaymentID)) AS PaymentNo,
//                         FORMAT(vp.PaymentDate, 'dd/MM/yyyy') AS PaymentDateFormatted,
//                         ISNULL(vp.Payee, ISNULL(v.VendorName, '')) AS Vendor,
//                         ISNULL(pm.PaymentMethod, ISNULL(vp.PaymentMode, 'Cash')) AS PaymentMethod,
//                         ISNULL(c.AccountNo + ' - ' + c.AccountName, ISNULL(vp.PaymentAccount, '')) AS PaymentAccount,
//                         vp.TotalPaymentAmount AS TotalAmount,
//                         ISNULL(vp.PaidBy, '') AS PaidBy,
//                         ISNULL(vp.Memo, '') AS Memo,
//                         '<div class=""d-flex justify-content-center gap-1"">' +
//                         '<button type=""button"" class=""btn btn-xs btn-outline-primary px-2 font-weight-bold"" title=""Print Voucher"" onclick=""showPaymentVoucher(' + CONVERT(VARCHAR, vp.VendorPaymentID) + ', ''' + ISNULL(vp.OfficalReceiptNo, '') + ''')""><i class=""fas fa-print mr-1""></i>Voucher</button>' +
//                         '</div>' AS ActionButtons
//                     FROM dbo.TblVendorPayment vp
//                     LEFT JOIN dbo.TblVendor v ON v.VendorID = vp.VendorID
//                     LEFT JOIN dbo.TblPaymentMethod pm ON pm.PaymentMethodID = vp.PaymentModeID
//                     LEFT JOIN dbo.TblChartOfAccount c ON c.ChartOfAccountID = vp.ChartOfAccountID
//                     WHERE vp.SystemID = " + sysId + @" 
//                       AND vp.WarehouseID = " + whId;

//                 html.GH.SQL = sqlPayments;
//                 html.OrderBy = "[No]";
//                 html.IsHaveWhereCondition = true;

//                 html.GH.GridTable = new GridField[]
//                 {
//                     new GridField() { Header = "#", FieldName = "No", TextAlign = "center"},
//                     new GridField() { Header = "Payment Nº", FieldName = "PaymentNoLink", TextAlign = "left"},
//                     new GridField() { Header = "Date", FieldName = "PaymentDateFormatted", TextAlign = "center" },
//                     new GridField() { Header = "Vendor / Payee", FieldName = "Vendor", TextAlign = "left"},
//                     new GridField() { Header = "Payment Method", FieldName = "PaymentMethod", TextAlign = "center"},
//                     new GridField() { Header = "Payment Account", FieldName = "PaymentAccount", TextAlign = "left" },
//                     new GridField() { Header = "Total Paid", FieldName = "TotalAmount", Format = "#,##0.00;-#,##0.00;-", TextAlign = "right", IsTotalFooter = true },
//                     new GridField() { Header = "Paid By", FieldName = "PaidBy", TextAlign = "left" },
//                     new GridField() { Header = "Memo", FieldName = "Memo", TextAlign = "left"},
//                     new GridField() { Header = "Action", FieldName = "ActionButtons", TextAlign = "center" }
//                 };

//                 ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
//             }
//             else
//             {
//                 // ── DEFAULT VIEW: DIRECT QUICKBOOKS "PAY BILLS" WITH SELECT ALL ──
//                 ViewBag.CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");
//                 ViewBag.CurrentUserName = html.UserName;
//                 ViewBag.SelectedBillID = "";
//                 ViewBag.SelectedBillNumber = "";
//                 ViewBag.VendorID = 0;

//                 // Load Cash & Bank Accounts
//                 string sqlAccounts = @"
//                     SELECT 
//                         ChartOfAccountID, 
//                         AccountType, 
//                         (AccountNo + ' - ' + AccountName) AS AccountName,
//                         AccountNo
//                     FROM dbo.TblChartOfAccount
//                     WHERE SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
//                       AND (
//                           AccountType IN ('Cash and Cash Equivalents', 'Cash', 'Bank', 'Current Assets')
//                           OR AccountNo LIKE '111%' OR AccountNo LIKE '112%' OR AccountNo LIKE '100%'
//                       )
//                     ORDER BY 
//                         CASE 
//                             WHEN AccountNo LIKE '111%' THEN 0 
//                             WHEN AccountNo LIKE '100%' THEN 1 
//                             ELSE 2 
//                         END, AccountNo ASC";

//                 DataSet dsAccounts = html.LoadData(sqlAccounts);
//                 ViewBag.PaymentAccounts = dsAccounts != null && dsAccounts.Tables.Count > 0 ? dsAccounts.Tables[0] : new DataTable();

//                 // Load Payment Methods
//                 string sqlMethods = @"
//                     SELECT PaymentMethodID, PaymentMethod 
//                     FROM dbo.TblPaymentMethod 
//                     WHERE SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
//                     ORDER BY 
//                         CASE 
//                             WHEN PaymentMethod = 'Bank Transfer' THEN 0 
//                             WHEN PaymentMethod = 'Cash' THEN 1 
//                             ELSE 2 
//                         END, PaymentMethod ASC";

//                 DataSet dsMethods = html.LoadData(sqlMethods);
//                 ViewBag.PaymentMethods = dsMethods != null && dsMethods.Tables.Count > 0 ? dsMethods.Tables[0] : new DataTable();

//                 // Load Vendors who have open bills
//                 string sqlVendors = @"
//                     SELECT DISTINCT 
//                         v.VendorID, 
//                         (v.VendorCode + ' - ' + v.VendorName) AS VendorDisplayName,
//                         v.VendorName,
//                         v.VendorCode
//                     FROM dbo.TblVendor v
//                     WHERE v.SystemID = " + sysId + @" AND v.WarehouseID = " + whId + @"
//                       AND EXISTS (
//                           SELECT 1 FROM dbo.TblBills b 
//                           WHERE b.VendorID = v.VendorID 
//                             AND ISNULL(NULLIF(b.AmountDue, 0), b.GrandTotal - ISNULL(b.AmountPaid, 0)) > 0
//                             AND ISNULL(b.StatusID, 0) <> 10
//                       )
//                     ORDER BY VendorDisplayName ASC";

//                 DataSet dsVendors = html.LoadData(sqlVendors);
//                 ViewBag.Vendors = dsVendors != null && dsVendors.Tables.Count > 0 ? dsVendors.Tables[0] : new DataTable();

//                 // Load Classes / Companies
//                 string sqlClasses = @"
//                     SELECT ClassID, ClassName, FullName, PrefixPV 
//                     FROM dbo.TblClass 
//                     WHERE ClassID IN (5, 6, 7) OR PrefixPV IS NOT NULL
//                     ORDER BY ClassID ASC";
//                 DataSet dsClasses = html.LoadData(sqlClasses);
//                 ViewBag.Classes = dsClasses != null && dsClasses.Tables.Count > 0 ? dsClasses.Tables[0] : new DataTable();

//                 ViewBag.DrawHTMLSearchForm = null;
//             }

//             return View("~/Views/PayBill/Index.cshtml");
//         }

//         // ─────────────────────────────────────────────────────────────────────
//         //  CREATE  →  QuickBooks-Style Pay Bill Form in JournalEntry UI Theme
//         // ─────────────────────────────────────────────────────────────────────
//         [HttpGet]
//         public ActionResult Create(int? vendorId = null, string billId = "", int? paymentId = null, int? systemId = null, int? warehouseId = null)
//         {
//             HTML html = new HTML();
//             string sysId = GetSystemID(html, systemId);
//             string whId = GetWarehouseID(html, warehouseId);

//             ViewBag.SystemID = sysId;
//             ViewBag.WarehouseID = whId;
//             ViewBag.PaymentID = paymentId ?? 0;
//             ViewBag.CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");
//             ViewBag.CurrentUserName = html.UserName;

//             // If a billId or billNumber is provided, look it up to resolve vendor and bill details
//             string selectedBillIdStr = "";
//             string selectedBillNumberStr = "";
//             if (!string.IsNullOrEmpty(billId))
//             {
//                 string safeBillParam = billId.Trim().Replace("'", "''");
//                 string findBillSql = @"
//                     SELECT TOP 1 BillID, VendorID, BillNumber, GrandTotal, AmountDue 
//                     FROM dbo.TblBills 
//                     WHERE (BillNumber = '" + safeBillParam + @"' OR CONVERT(VARCHAR, BillID) = '" + safeBillParam + @"')
//                       AND SystemID = " + sysId + @" AND WarehouseID = " + whId;

//                 DataSet dsFind = html.LoadData(findBillSql);
//                 if (dsFind != null && dsFind.Tables.Count > 0 && dsFind.Tables[0].Rows.Count > 0)
//                 {
//                     selectedBillIdStr = dsFind.Tables[0].Rows[0]["BillID"].ToString();
//                     selectedBillNumberStr = dsFind.Tables[0].Rows[0]["BillNumber"].ToString();
//                     int.TryParse(dsFind.Tables[0].Rows[0]["VendorID"].ToString(), out int foundVendorId);
//                     if (foundVendorId > 0 && (!vendorId.HasValue || vendorId.Value <= 0))
//                     {
//                         vendorId = foundVendorId;
//                     }
//                 }
//                 else
//                 {
//                     selectedBillIdStr = billId;
//                     selectedBillNumberStr = billId;
//                 }
//             }

//             ViewBag.SelectedBillID = selectedBillIdStr;
//             ViewBag.SelectedBillNumber = selectedBillNumberStr;
//             ViewBag.VendorID = vendorId ?? 0;

//             // Load Cash & Bank Accounts
//             string sqlAccounts = @"
//                 SELECT 
//                     ChartOfAccountID, 
//                     AccountType, 
//                     (AccountNo + ' - ' + AccountName) AS AccountName,
//                     AccountNo
//                 FROM dbo.TblChartOfAccount
//                 WHERE SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
//                   AND (
//                       AccountType IN ('Cash and Cash Equivalents', 'Cash', 'Bank', 'Current Assets')
//                       OR AccountNo LIKE '111%' OR AccountNo LIKE '112%' OR AccountNo LIKE '100%'
//                   )
//                 ORDER BY 
//                     CASE 
//                         WHEN AccountNo LIKE '111%' THEN 0 
//                         WHEN AccountNo LIKE '100%' THEN 1 
//                         ELSE 2 
//                     END, AccountNo ASC";

//             DataSet dsAccounts = html.LoadData(sqlAccounts);

//             // Load Vendors who have open bills, plus Contact Person & Phone
//             string sqlVendors = @"
//                 SELECT DISTINCT 
//                     v.VendorID, 
//                     (v.VendorCode + ' - ' + v.VendorName) AS VendorDisplayName,
//                     v.VendorName,
//                     v.VendorCode,
//                     ISNULL(v.ContactPersonName, '') AS ContactPerson,
//                     ISNULL(NULLIF(v.ContactPhoneNumber, ''), ISNULL(v.Phone, '')) AS Phone
//                 FROM dbo.TblVendor v
//                 WHERE v.SystemID = " + sysId + @" AND v.WarehouseID = " + whId + @"
//                   AND EXISTS (
//                       SELECT 1 FROM dbo.TblBills b 
//                       WHERE b.VendorID = v.VendorID 
//                         AND ISNULL(NULLIF(b.AmountDue, 0), b.GrandTotal - ISNULL(b.AmountPaid, 0)) > 0
//                         AND ISNULL(b.StatusID, 0) <> 10
//                   )
//                 ORDER BY VendorDisplayName ASC";

//             DataSet dsVendors = html.LoadData(sqlVendors);
//             ViewBag.Vendors = dsVendors != null && dsVendors.Tables.Count > 0 ? dsVendors.Tables[0] : new DataTable();

//             // Load Payment Methods
//             string sqlPaymentMethods = "SELECT PaymentMethodID, PaymentMethod FROM dbo.TblPaymentMethod ORDER BY CASE WHEN PaymentMethod = 'Cash' THEN 0 WHEN PaymentMethod = 'Bank Transfer' THEN 1 ELSE 2 END, PaymentMethod ASC";
//             DataSet dsPM = html.LoadData(sqlPaymentMethods);
//             ViewBag.PaymentMethods = dsPM != null && dsPM.Tables.Count > 0 ? dsPM.Tables[0] : new DataTable();

//             // Load Classes / Companies
//             string sqlClassesCreate = @"
//                 SELECT ClassID, ClassName, FullName, PrefixPV 
//                 FROM dbo.TblClass 
//                 WHERE ClassID IN (5, 6, 7) OR PrefixPV IS NOT NULL
//                 ORDER BY ClassID ASC";
//             DataSet dsClassesCreate = html.LoadData(sqlClassesCreate);
//             ViewBag.Classes = dsClassesCreate != null && dsClassesCreate.Tables.Count > 0 ? dsClassesCreate.Tables[0] : new DataTable();

//             return View("~/Views/PayBill/Create.cshtml");
//         }

//         // ─────────────────────────────────────────────────────────────────────
//         //  GET UNPAID BILLS & VENDOR EXPENSES API  →  Returns unified JSON
//         // ─────────────────────────────────────────────────────────────────────
//         [HttpGet]
//         [AllowAnonymous]
//         public JsonResult GetUnpaidBills(int? vendorId = null, string searchText = "", string billId = "", string fromDate = "", string toDate = "", int? systemId = null, int? warehouseId = null, string itemType = "all")
//         {
//             HTML html = new HTML();
//             string sysId = GetSystemID(html, systemId);
//             string whId = GetWarehouseID(html, warehouseId);

//             string vendorFilter = (vendorId.HasValue && vendorId.Value > 0) ? " AND t.VendorID = " + vendorId.Value : "";
            
//             string searchFilter = "";
//             if (!string.IsNullOrEmpty(searchText))
//             {
//                 string safe = searchText.Trim().Replace("'", "''");
//                 searchFilter = " AND (t.BillNumber LIKE '%" + safe + "%' OR t.PONumber LIKE '%" + safe + "%' OR t.VendorName LIKE '%" + safe + "%' OR t.VendorCode LIKE '%" + safe + "%' OR t.Memo LIKE '%" + safe + "%')";
//             }

//             string billFilter = "";
//             if (!string.IsNullOrEmpty(billId))
//             {
//                 string safeB = billId.Trim().Replace("'", "''");
//                 billFilter = " AND (t.BillNumber = '" + safeB + "' OR CONVERT(VARCHAR, t.BillID) = '" + safeB + "')";
//             }

//             string dateFilter = "";
//             if (!string.IsNullOrEmpty(fromDate))
//             {
//                 DateTime dtFrom;
//                 if (DateTime.TryParseExact(fromDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtFrom) || DateTime.TryParse(fromDate, out dtFrom))
//                 {
//                     dateFilter += " AND t.BillDate >= '" + dtFrom.ToString("yyyy-MM-dd") + "'";
//                 }
//             }
//             if (!string.IsNullOrEmpty(toDate))
//             {
//                 DateTime dtTo;
//                 if (DateTime.TryParseExact(toDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtTo) || DateTime.TryParse(toDate, out dtTo))
//                 {
//                     dateFilter += " AND t.BillDate <= '" + dtTo.ToString("yyyy-MM-dd") + " 23:59:59'";
//                 }
//             }

//             string targetBillIdStr = (!string.IsNullOrEmpty(billId) ? billId.Trim().Replace("'", "''") : "0");

//             string sql = @"
//                 SELECT * FROM (
//                     -- Bills from TblBills
//                     SELECT 
//                         'Bill' AS ItemType,
//                         b.BillID,
//                         0 AS ExpenseID,
//                         ISNULL(b.ClassID, 0) AS ClassID,
//                         b.VendorID,
//                         ISNULL(v.VendorName, '') AS VendorName,
//                         ISNULL(v.VendorCode, '') AS VendorCode,
//                         (ISNULL(v.VendorCode, '') + ' - ' + ISNULL(v.VendorName, '')) AS VendorDisplayName,
//                         ISNULL(b.BillNumber, '') AS BillNumber,
//                         ISNULL(b.PONumber, '') AS PONumber,
//                         b.BillDate,
//                         FORMAT(b.BillDate, 'dd/MM/yyyy') AS BillDateFormatted,
//                         b.PaymentDueDate,
//                         FORMAT(ISNULL(b.PaymentDueDate, b.BillDate), 'dd/MM/yyyy') AS DueDateFormatted,
//                         b.GrandTotal,
//                         ISNULL(b.AmountPaid, 0) AS AmountPaid,
//                         ISNULL(NULLIF(b.AmountDue, 0), b.GrandTotal - ISNULL(b.AmountPaid, 0)) AS AmountDue,
//                         ISNULL(dep.AvailableDeposit, 0) AS AvailableDeposit,
//                         ISNULL(dep.DepositExpenseID, 0) AS DepositExpenseID,
//                         ISNULL(dep.DepositPVNumber, '') AS DepositPVNumber,
//                         DATEDIFF(day, CAST(ISNULL(b.PaymentDueDate, b.BillDate) AS DATE), CAST(GETDATE() AS DATE)) AS DaysOverdue,
//                         CASE 
//                             WHEN DATEDIFF(day, CAST(ISNULL(b.PaymentDueDate, b.BillDate) AS DATE), CAST(GETDATE() AS DATE)) > 0 THEN 'Overdue'
//                             WHEN DATEDIFF(day, CAST(ISNULL(b.PaymentDueDate, b.BillDate) AS DATE), CAST(GETDATE() AS DATE)) = 0 THEN 'Due Today'
//                             ELSE 'Due Soon'
//                         END AS DueStatus,
//                         ISNULL(b.CustomerNotes, '') AS Memo,
//                         b.StatusID
//                     FROM dbo.TblBills b
//                     LEFT JOIN dbo.TblVendor v ON v.VendorID = b.VendorID
//                     OUTER APPLY (
//                         SELECT TOP 1
//                             ISNULL(NULLIF(e.PaidAmount, 0), e.TotalAmount) AS AvailableDeposit,
//                             e.ExpenseID AS DepositExpenseID,
//                             ISNULL(e.[No], e.PaymentNo) AS DepositPVNumber
//                         FROM dbo.Expenses e
//                         WHERE (
//                             (e.ExpenseTransactionType = 'Vendor Deposit' AND (
//                                 (ISNULL(b.PONumber, '') <> '' AND (e.Reference = b.PONumber OR e.Reference LIKE '%' + b.PONumber + '%'))
//                                 OR (ISNULL(b.PONumber, '') = '' AND e.PayeeID = b.VendorID)
//                             ))
//                             OR (ISNULL(b.PONumber, '') <> '' AND (e.Reference = b.PONumber OR e.Reference LIKE '%' + b.PONumber + '%' OR EXISTS (SELECT 1 FROM dbo.ExpenseDetails ed WHERE ed.ExpenseID = e.ExpenseID AND (ed.RefNo = b.PONumber OR ed.RefNo LIKE '%' + b.PONumber + '%'))))
//                             OR (ISNULL(b.BillNumber, '') <> '' AND (e.Reference = b.BillNumber OR e.Reference LIKE '%' + b.BillNumber + '%' OR EXISTS (SELECT 1 FROM dbo.ExpenseDetails ed WHERE ed.ExpenseID = e.ExpenseID AND (ed.RefNo = b.BillNumber OR ed.RefNo LIKE '%' + b.BillNumber + '%'))))
//                         )
//                         AND ISNULL(e.PaymentStatus, '') <> 'Applied'
//                         AND (e.PayeeID = b.VendorID OR ISNULL(b.VendorID, 0) = 0 OR ISNULL(e.PayeeID, 0) = 0)
//                         AND (ISNULL(e.PaidAmount, 0) > 0 OR (e.ExpenseTransactionType = 'Vendor Deposit' AND ISNULL(e.TotalAmount, 0) > 0))
//                         AND e.SystemID = b.SystemID
//                         AND e.WarehouseID = b.WarehouseID
//                         ORDER BY e.ExpenseID DESC
//                     ) dep
//                     WHERE b.SystemID = " + sysId + @" AND b.WarehouseID = " + whId + @"
//                       AND ISNULL(NULLIF(b.AmountDue, 0), b.GrandTotal - ISNULL(b.AmountPaid, 0)) > 0
//                       AND ISNULL(b.StatusID, 0) <> 10
//                 ) t
//                 WHERE 1=1
//                 " + vendorFilter + searchFilter + billFilter + dateFilter + @"
//                 ORDER BY 
//                     CASE WHEN (t.BillNumber = '" + targetBillIdStr + @"' OR CONVERT(VARCHAR, t.BillID) = '" + targetBillIdStr + @"') THEN 0 ELSE 1 END,
//                     t.BillDate DESC, 
//                     t.DaysOverdue DESC";

//             DataSet ds = html.LoadData(sql);
//             if (ds != null && ds.Tables.Count > 0)
//             {
//                 string json = JsonConvert.SerializeObject(ds.Tables[0]);
//                 return Json(new { success = true, data = json }, JsonRequestBehavior.AllowGet);
//             }

//             return Json(new { success = true, data = "[]" }, JsonRequestBehavior.AllowGet);
//         }

//         // ─────────────────────────────────────────────────────────────────────
//         //  GET PV NUMBER API  →  Retrieves next sequential Payment Voucher Nº
//         // ─────────────────────────────────────────────────────────────────────
//         [HttpGet]
//         [AllowAnonymous]
//         public JsonResult GetPVNumber(int classId = 0, string paymentDate = "")
//         {
//             HTML html = new HTML();
//             string sysId = GetSystemID(html);
//             string whId = GetWarehouseID(html);
//             string domainId = html.DomainID ?? "001";

//             DateTime dtDate = DateTime.Now;
//             if (!string.IsNullOrEmpty(paymentDate))
//             {
//                 if (!DateTime.TryParseExact(paymentDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtDate))
//                 {
//                     DateTime.TryParse(paymentDate, out dtDate);
//                 }
//             }

//             try
//             {
//                 string pvNumber = "";
//                 string prefix = "";

//                 // 1. Determine prefix from TblClass if classId provided, otherwise check TblPrefix
//                 string checkPrefixSql = @"
//                     DECLARE @Pref NVARCHAR(50) = '', @Mod NVARCHAR(50) = 'PrefixPV';
//                     IF (" + classId + @" > 0)
//                     BEGIN
//                         SELECT @Pref = PrefixPV FROM dbo.TblClass WHERE ClassID = " + classId + @";
//                     END

//                     IF (@Pref IS NULL OR @Pref = '')
//                     BEGIN
//                         SELECT TOP 1 @Pref = Prefix FROM dbo.TblPrefix 
//                         WHERE Module = 'PrefixPV' AND SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
//                         ORDER BY PrefixID DESC;
//                     END

//                     IF (@Pref IS NULL OR @Pref = '')
//                     BEGIN
//                         SELECT TOP 1 @Pref = Prefix FROM dbo.TblPrefix 
//                         WHERE Module = 'Expense' AND Prefix LIKE 'NPV%' AND SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
//                         ORDER BY PrefixID DESC;
//                     END

//                     IF (@Pref IS NULL OR @Pref = '')
//                     BEGIN
//                         SET @Pref = 'NPV-{yy}{MM}-{000000}';
//                     END

//                     EXEC dbo.SP_SHOW_PREFIX @Pref, @Mod, " + sysId + @", " + whId + @", '" + domainId + @"';
//                 ";

//                 DataSet ds = html.LoadData(checkPrefixSql);
//                 if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
//                 {
//                     var row = ds.Tables[0].Rows[0];
//                     if (row.Table.Columns.Contains("PrefixNumber") && row["PrefixNumber"] != DBNull.Value)
//                         pvNumber = row["PrefixNumber"].ToString();
//                     else if (row.Table.Columns.Contains("Prefix") && row["Prefix"] != DBNull.Value)
//                         pvNumber = row["Prefix"].ToString();
//                     else if (row[0] != DBNull.Value)
//                         pvNumber = row[0].ToString();

//                     if (row.Table.Columns.Contains("Prefix") && row["Prefix"] != DBNull.Value)
//                         prefix = row["Prefix"].ToString();
//                 }

//                 // Fallback calculation if SP_SHOW_PREFIX returned empty
//                 if (string.IsNullOrEmpty(pvNumber))
//                 {
//                     string yyMM = dtDate.ToString("yyMM");
//                     string pat = "NPV-" + yyMM + "-";
//                     string maxSql = @"
//                         SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(t.PVNo, LEN('" + pat + @"') + 1, 10) AS INT)), 0)
//                         FROM (
//                             SELECT PVNo FROM dbo.TblAccount_Transaction WHERE PVNo LIKE '" + pat + @"%' AND SystemID = " + sysId + @"
//                             UNION
//                             SELECT PVNo FROM dbo.Expenses WHERE PVNo LIKE '" + pat + @"%' AND SystemID = " + sysId + @"
//                         ) t";
//                     DataSet dsMax = html.LoadData(maxSql);
//                     int nextNo = 1;
//                     if (dsMax != null && dsMax.Tables.Count > 0 && dsMax.Tables[0].Rows.Count > 0 && dsMax.Tables[0].Rows[0][0] != DBNull.Value)
//                     {
//                         if (int.TryParse(dsMax.Tables[0].Rows[0][0].ToString(), out int mVal))
//                         {
//                             nextNo = mVal + 1;
//                         }
//                     }
//                     pvNumber = pat + nextNo.ToString("D6");
//                     prefix = "NPV-{yy}{MM}-{000000}";
//                 }

//                 return Json(new { success = true, pvNumber = pvNumber, prefix = prefix }, JsonRequestBehavior.AllowGet);
//             }
//             catch (Exception ex)
//             {
//                 string fallbackPV = "NPV-" + dtDate.ToString("yyMM") + "-000001";
//                 return Json(new { success = true, pvNumber = fallbackPV, message = ex.Message }, JsonRequestBehavior.AllowGet);
//             }
//         }

//         // ─────────────────────────────────────────────────────────────────────
//         //  SAVE PAYMENT API  →  Processes batch or single payments (Bills + Expenses)
//         // ─────────────────────────────────────────────────────────────────────
//         [HttpPost]
//         [AllowAnonymous]
//         public JsonResult SavePayment(string paymentHeader, string paymentDetails)
//         {
//             HTML html = new HTML();
//             string sysId = GetSystemID(html);
//             string whId = GetWarehouseID(html);

//             try
//             {
//                 dynamic header = JsonConvert.DeserializeObject(paymentHeader);
//                 dynamic details = JsonConvert.DeserializeObject(paymentDetails);

//                 int vendorId = 0;
//                 if (header.VendorID != null)
//                 {
//                     int.TryParse(header.VendorID.ToString(), out vendorId);
//                 }

//                 int classId = 0;
//                 if (header.ClassID != null)
//                 {
//                     int.TryParse(header.ClassID.ToString(), out classId);
//                 }

//                 string payee = header.Payee != null ? header.Payee.ToString() : "";
//                 string pvNumber = header.PVNumber != null ? header.PVNumber.ToString().Trim() : "";
//                 string reference = header.Reference != null ? header.Reference.ToString() : "";
//                 string paymentDateStr = header.PaymentDate != null ? header.PaymentDate.ToString() : DateTime.Now.ToString("yyyy-MM-dd");
//                 int chartOfAccountId = header.ChartOfAccountID != null ? (int)header.ChartOfAccountID : 0;
//                 int paymentModeId = header.PaymentModeID != null ? (int)header.PaymentModeID : 0;
//                 string memo = header.Memo != null ? header.Memo.ToString() : "";
//                 decimal totalPaymentAmount = header.TotalPaymentAmount != null ? (decimal)header.TotalPaymentAmount : 0;
//                 string fromBankName = header.FromBankName != null ? header.FromBankName.ToString() : "";
//                 string fromAccountNumber = header.FromAccountNumber != null ? header.FromAccountNumber.ToString() : "";
//                 string chequeNumber = header.ChequeNumber != null ? header.ChequeNumber.ToString() : "";

//                 DateTime dtPaymentDate;
//                 if (!DateTime.TryParseExact(paymentDateStr, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtPaymentDate) && !DateTime.TryParse(paymentDateStr, out dtPaymentDate))
//                 {
//                     dtPaymentDate = DateTime.Now;
//                 }

//                 // 1. Generate or validate PV Number (Payment Voucher Nº)
//                 if (string.IsNullOrEmpty(pvNumber))
//                 {
//                     string pvSql = @"
//                         DECLARE @Pref NVARCHAR(50) = '', @Mod NVARCHAR(50) = 'PrefixPV';
//                         IF (" + classId + @" > 0)
//                         BEGIN
//                             SELECT @Pref = PrefixPV FROM dbo.TblClass WHERE ClassID = " + classId + @";
//                         END

//                         IF (@Pref IS NULL OR @Pref = '')
//                         BEGIN
//                             SELECT TOP 1 @Pref = Prefix FROM dbo.TblPrefix 
//                             WHERE Module = 'PrefixPV' AND SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
//                             ORDER BY PrefixID DESC;
//                         END

//                         IF (@Pref IS NULL OR @Pref = '')
//                         BEGIN
//                             SELECT TOP 1 @Pref = Prefix FROM dbo.TblPrefix 
//                             WHERE Module = 'Expense' AND Prefix LIKE 'NPV%' AND SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
//                             ORDER BY PrefixID DESC;
//                         END

//                         IF (@Pref IS NULL OR @Pref = '')
//                         BEGIN
//                             SET @Pref = 'NPV-{yy}{MM}-{000000}';
//                         END

//                         DECLARE @OutPV NVARCHAR(50);
//                         EXEC dbo.SP_INSERT_PREFIX_BASE_ON_YEAR @Pref, @Mod, " + sysId + @", " + whId + @", @OutPV OUT, 1, 0, '" + html.DomainID + @"', '" + dtPaymentDate.ToString("yyyy-MM-dd") + @"';
//                         SELECT @OutPV AS PVNo;";

//                     DataSet dsPV = html.LoadData(pvSql);
//                     if (dsPV != null && dsPV.Tables.Count > 0)
//                     {
//                         for (int t = dsPV.Tables.Count - 1; t >= 0; t--)
//                         {
//                             var dt = dsPV.Tables[t];
//                             if (dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value && !string.IsNullOrEmpty(dt.Rows[0][0].ToString()))
//                             {
//                                 pvNumber = dt.Rows[0][0].ToString();
//                                 break;
//                             }
//                         }
//                     }
//                     if (string.IsNullOrEmpty(pvNumber))
//                     {
//                         pvNumber = "NPV-" + dtPaymentDate.ToString("yyMM") + "-000001";
//                     }
//                 }
//                 else
//                 {
//                     // User confirmed custom or generated PV number -> synchronize counter in TblPrefix if numerical suffix is present
//                     try
//                     {
//                         int lastDash = pvNumber.LastIndexOf('-');
//                         if (lastDash >= 0 && lastDash < pvNumber.Length - 1)
//                         {
//                             string numPart = pvNumber.Substring(lastDash + 1);
//                             if (int.TryParse(numPart, out int numVal))
//                             {
//                                 string syncPrefixSql = @"
//                                     UPDATE dbo.TblPrefix 
//                                     SET CurrentNumber = " + numVal + @", CurrentPrefix = N'" + pvNumber.Replace("'", "''") + @"'
//                                     WHERE Module IN ('PrefixPV', 'Expense') 
//                                       AND (Prefix LIKE 'NPV%' OR Prefix LIKE 'PV%') 
//                                       AND SystemID = " + sysId + @" AND WarehouseID = " + whId + @"
//                                       AND ISNULL(CurrentNumber, 0) < " + numVal + ";";
//                                 html.LoadData(syncPrefixSql);
//                             }
//                         }
//                     }
//                     catch { }
//                 }

//                 string paymentNo = pvNumber;

//                 // Get account & payment mode display names
//                 string accountName = "";
//                 string paymentModeName = "";
//                 string nameLookupSql = @"
//                     SELECT 
//                         (SELECT (AccountNo + ' - ' + AccountName) FROM dbo.TblChartOfAccount WHERE ChartOfAccountID = " + chartOfAccountId + @") AS AccountName,
//                         (SELECT PaymentMethod FROM dbo.TblPaymentMethod WHERE PaymentMethodID = " + paymentModeId + @") AS PaymentMethod;";
//                 DataSet dsNames = html.LoadData(nameLookupSql);
//                 if (dsNames != null && dsNames.Tables.Count > 0 && dsNames.Tables[0].Rows.Count > 0)
//                 {
//                     accountName = dsNames.Tables[0].Rows[0]["AccountName"].ToString();
//                     paymentModeName = dsNames.Tables[0].Rows[0]["PaymentMethod"].ToString();
//                 }

//                 // 2. Insert into dbo.TblVendorPayment
//                 StringBuilder sbSql = new StringBuilder();
//                 sbSql.AppendLine("BEGIN TRANSACTION;");
//                 sbSql.AppendLine("BEGIN TRY");

//                 sbSql.AppendFormat(@"
//                     DECLARE @NewPaymentID INT;
//                     INSERT INTO dbo.TblVendorPayment (
//                         VendorID, Payee, Reference, PaymentDate, ChartOfAccountID, PaymentAccount,
//                         PaymentModeID, PaymentMode, FromBankName, FromAccountNumber, ChequeNumber,
//                         OfficalReceiptNo, TotalPaymentAmount, PaidBy, [Type], Memo,
//                         SystemID, WarehouseID, DomainID
//                     ) VALUES (
//                         {0}, N'{1}', N'{2}', '{3:yyyy-MM-dd HH:mm:ss}', {4}, N'{5}',
//                         {6}, N'{7}', N'{8}', N'{9}', N'{10}',
//                         N'{11}', {12}, N'{13}', 'Expense', N'{14}',
//                         {15}, {16}, '{17}'
//                     );
//                     SET @NewPaymentID = SCOPE_IDENTITY();
//                 ", 
//                     vendorId, 
//                     payee.Replace("'", "''"), 
//                     (!string.IsNullOrEmpty(reference) ? reference : paymentNo).Replace("'", "''"), 
//                     dtPaymentDate, 
//                     chartOfAccountId, 
//                     accountName.Replace("'", "''"),
//                     paymentModeId, 
//                     paymentModeName.Replace("'", "''"),
//                     fromBankName.Replace("'", "''"), 
//                     fromAccountNumber.Replace("'", "''"), 
//                     chequeNumber.Replace("'", "''"),
//                     paymentNo.Replace("'", "''"), 
//                     totalPaymentAmount, 
//                     (html.UserName ?? "Admin").Replace("'", "''"), 
//                     memo.Replace("'", "''"),
//                     sysId, 
//                     whId, 
//                     html.DomainID
//                 );

//                 decimal totalDepositApplied = 0;
//                 string depositMemo = "";

//                 // 3. Insert Details & Update TblBills / Expenses
//                 foreach (dynamic bill in details)
//                 {
//                     int bId = bill.BillID != null ? (int)bill.BillID : 0;
//                     int expId = bill.ExpenseID != null ? (int)bill.ExpenseID : 0;
//                     int bVendorId = bill.VendorID != null ? (int)bill.VendorID : vendorId;
//                     string bNumber = bill.BillNumber != null ? bill.BillNumber.ToString() : "";
//                     string bPoNumber = bill.PONumber != null ? bill.PONumber.ToString() : "";
//                     decimal bAmount = bill.GrandTotal != null ? (decimal)bill.GrandTotal : 0;
//                     decimal bPriorPaid = bill.AmountPaid != null ? (decimal)bill.AmountPaid : 0;
//                     decimal bPayAmount = bill.PaymentAmount != null ? (decimal)bill.PaymentAmount : 0;
//                     decimal bDepositAmount = bill.DepositAmount != null ? (decimal)bill.DepositAmount : 0;
//                     int bDepositExpenseId = bill.DepositExpenseID != null ? (int)bill.DepositExpenseID : 0;

//                     decimal bTotalApplied = bPayAmount + bDepositAmount;
//                     if (bTotalApplied <= 0) continue;

//                     totalDepositApplied += bDepositAmount;

//                     // Retrieve description of deposit expense for GL remark
//                     if (bDepositExpenseId > 0 && bDepositAmount > 0 && string.IsNullOrEmpty(depositMemo))
//                     {
//                         try
//                         {
//                             string descSql = "SELECT Description FROM dbo.Expenses WHERE ExpenseID = " + bDepositExpenseId;
//                             DataSet dsDesc = html.LoadData(descSql);
//                             if (dsDesc != null && dsDesc.Tables.Count > 0 && dsDesc.Tables[0].Rows.Count > 0 && dsDesc.Tables[0].Rows[0]["Description"] != DBNull.Value)
//                             {
//                                 depositMemo = dsDesc.Tables[0].Rows[0]["Description"].ToString().Trim();
//                             }
//                         }
//                         catch { }
//                     }

//                     if (bId > 0)
//                     {
//                         sbSql.AppendFormat(@"
//                             INSERT INTO dbo.TblVendorPaymentDetail (
//                                 VendorPaymentID, BillID, VendorID, BillDate, DueDate, BillNumber,
//                                 PONumber, Amount, AmountPaid, AmountDue, PaymentAmount, OfficalReceiptNo,
//                                 SystemID, WarehouseID, DomainID
//                             )
//                             SELECT 
//                                 @NewPaymentID, {0}, {1}, b.BillDate, b.PaymentDueDate, N'{2}',
//                                 N'{3}', b.GrandTotal, (ISNULL(b.AmountPaid, 0) + {4}),
//                                 (b.GrandTotal - (ISNULL(b.AmountPaid, 0) + {4})),
//                                 {5}, N'{6}',
//                                 {7}, {8}, '{9}'
//                             FROM dbo.TblBills b WHERE b.BillID = {0};

//                             -- Update TblBills record (settles both deposit and cash/bank payment; AmountDue and StatusID are auto-computed)
//                             UPDATE dbo.TblBills SET 
//                                 AmountPaid = ISNULL(AmountPaid, 0) + {4},
//                                 OfficalReceiptNo = N'{6}'
//                             WHERE BillID = {0};

//                             -- Update Purchase Order status if linked
//                             UPDATE po SET 
//                                 PaymentStatusID = CASE WHEN (b.GrandTotal - (ISNULL(b.AmountPaid, 0) + {4})) <= 0 THEN 10 ELSE 15 END,
//                                 PaymentStatus = CASE WHEN (b.GrandTotal - (ISNULL(b.AmountPaid, 0) + {4})) <= 0 THEN 'Paid' ELSE 'Partially Paid' END,
//                                 PaidAmount = ISNULL(po.PaidAmount, 0) + {4}
//                             FROM dbo.TblPurchaseOrder po
//                             INNER JOIN dbo.TblBills b ON b.PurchaseOrderID = po.PurchaseOrderID
//                             WHERE b.BillID = {0};
//                         ",
//                             bId,
//                             bVendorId,
//                             bNumber.Replace("'", "''"),
//                             bPoNumber.Replace("'", "''"),
//                             bTotalApplied,
//                             bPayAmount,
//                             paymentNo.Replace("'", "''"),
//                             sysId,
//                             whId,
//                             html.DomainID
//                         );
//                     }
//                     else if (expId > 0)
//                     {
//                         sbSql.AppendFormat(@"
//                             INSERT INTO dbo.TblVendorPaymentDetail (
//                                 VendorPaymentID, BillID, VendorID, BillDate, DueDate, BillNumber,
//                                 PONumber, Amount, AmountPaid, AmountDue, PaymentAmount, OfficalReceiptNo,
//                                 SystemID, WarehouseID, DomainID
//                             )
//                             SELECT 
//                                 @NewPaymentID, 0, {1}, e.PaymentDate, e.PaymentDate, N'{2}',
//                                 N'{3}', e.TotalAmount, (ISNULL(e.PaidAmount, 0) + {4}),
//                                 (e.TotalAmount - (ISNULL(e.PaidAmount, 0) + {4})),
//                                 {5}, N'{6}',
//                                 {7}, {8}, '{9}'
//                             FROM dbo.Expenses e WHERE e.ExpenseID = {0};

//                             -- Update dbo.Expenses record
//                             UPDATE dbo.Expenses SET 
//                                 PaidAmount = ISNULL(PaidAmount, 0) + {4},
//                                 BalanceAmount = CASE WHEN (TotalAmount - (ISNULL(PaidAmount, 0) + {4})) < 0 THEN 0 ELSE (TotalAmount - (ISNULL(PaidAmount, 0) + {4})) END,
//                                 PaymentStatus = CASE WHEN (TotalAmount - (ISNULL(PaidAmount, 0) + {4})) <= 0 THEN 'Paid' ELSE 'Partial Paid' END,
//                                 PostedPaymentNo = N'{6}'
//                             WHERE ExpenseID = {0};
//                         ",
//                             expId,
//                             bVendorId,
//                             bNumber.Replace("'", "''"),
//                             bPoNumber.Replace("'", "''"),
//                             bTotalApplied,
//                             bPayAmount,
//                             paymentNo.Replace("'", "''"),
//                             sysId,
//                             whId,
//                             html.DomainID
//                         );
//                     }

//                     if (bDepositExpenseId > 0 && bDepositAmount > 0)
//                     {
//                         sbSql.AppendFormat(@"
//                             -- Mark linked Vendor Deposit Expense as applied
//                             UPDATE dbo.Expenses SET 
//                                 PaymentStatus = 'Applied', 
//                                 BalanceAmount = CASE WHEN ISNULL(BalanceAmount, 0) - {1} < 0 THEN 0 ELSE ISNULL(BalanceAmount, 0) - {1} END
//                             WHERE ExpenseID = {0};
//                         ", bDepositExpenseId, bDepositAmount);
//                     }
//                 }

//                 if (string.IsNullOrEmpty(depositMemo))
//                 {
//                     depositMemo = !string.IsNullOrEmpty(memo) ? memo : ("Vendor Deposit applied on Pay Bill " + paymentNo);
//                 }

//                 // 4. Record General Ledger Entry in TblAccount_Transaction and TblAccount_JournalEntries (Type: 'Pay Bill')
//                 decimal totalSettled = totalPaymentAmount + totalDepositApplied;
//                 sbSql.AppendFormat(@"
//                     -- General Ledger Posting for Bill Payment:
//                     -- Debit Accounts Payable (210000), Credit Cash/Bank, and Credit Vendor Deposit (122000)
//                     DECLARE @TxID INT;
//                     INSERT INTO dbo.TblAccount_Transaction (
//                         InvoiceNo, PVNo, PaymentNo, TransactionNo, PayeeID, VendorID,
//                         TransactionDate, Amount, Description,
//                         [Type], StatusID, SystemID, WarehouseID, DomainID, CreateByUserID, CreateDate
//                     ) VALUES (
//                         N'{0}', N'{0}', N'{0}', N'{0}', {12}, {12},
//                         '{1:yyyy-MM-dd HH:mm:ss}', {2}, N'Pay Bill - {0}' + CASE WHEN {3} > 0 THEN ' (Applied Deposit: $' + CONVERT(VARCHAR, {3}) + ')' ELSE '' END,
//                         'Pay Bill', 10, {4}, {5}, '{6}', {7}, GETDATE()
//                     );
//                     SET @TxID = SCOPE_IDENTITY();

//                     -- Debit Accounts Payable (ChartOfAccountID 464 or AccountNo 210000) for full settled bill amount
//                     DECLARE @APAccountID INT;
//                     SELECT TOP 1 @APAccountID = ChartOfAccountID FROM dbo.TblChartOfAccount WHERE AccountNo = '210000' AND SystemID = {4} AND WarehouseID = {5};
//                     IF (@APAccountID IS NULL OR @APAccountID = 0) SET @APAccountID = 464;

//                     INSERT INTO dbo.TblAccount_JournalEntries (
//                         TransactionID, AccountID, Debit, Credit, Description,
//                         TransactionDate, EntryDate, [Type], InvoiceNo, PaymentNo, CustomerID,
//                         SystemID, WarehouseID, DomainID, CreateByUserID, CreateDate, IsPosted
//                     ) VALUES (
//                         @TxID, @APAccountID, {2}, 0, N'Bill Payment {0} to ' + N'{8}',
//                         '{1:yyyy-MM-dd HH:mm:ss}', '{1:yyyy-MM-dd HH:mm:ss}', 'Pay Bill', N'{0}', N'{0}', {12},
//                         {4}, {5}, '{6}', {7}, GETDATE(), 1
//                     );

//                     -- Credit Cash/Bank Account for actual cash disbursement
//                     IF ({9} > 0)
//                     BEGIN
//                         INSERT INTO dbo.TblAccount_JournalEntries (
//                             TransactionID, AccountID, Debit, Credit, Description,
//                             TransactionDate, EntryDate, [Type], InvoiceNo, PaymentNo, CustomerID,
//                             SystemID, WarehouseID, DomainID, CreateByUserID, CreateDate, IsPosted
//                         ) VALUES (
//                             @TxID, {10}, 0, {9}, N'Bill Payment {0} via ' + N'{11}',
//                             '{1:yyyy-MM-dd HH:mm:ss}', '{1:yyyy-MM-dd HH:mm:ss}', 'Pay Bill', N'{0}', N'{0}', {12},
//                             {4}, {5}, '{6}', {7}, GETDATE(), 1
//                         );
//                     END

//                     -- Credit Vendor Deposit (Account 122000) to clear the balance sheet deposit asset
//                     IF ({3} > 0)
//                     BEGIN
//                         DECLARE @DepAccountID INT;
//                         SELECT TOP 1 @DepAccountID = ChartOfAccountID FROM dbo.TblChartOfAccount WHERE AccountNo = '122000' AND SystemID = {4} AND WarehouseID = {5};
//                         IF (@DepAccountID IS NULL OR @DepAccountID = 0) SET @DepAccountID = 432;

//                         INSERT INTO dbo.TblAccount_JournalEntries (
//                             TransactionID, AccountID, Debit, Credit, Description,
//                             TransactionDate, EntryDate, [Type], InvoiceNo, PaymentNo, CustomerID,
//                             SystemID, WarehouseID, DomainID, CreateByUserID, CreateDate, IsPosted
//                         ) VALUES (
//                             @TxID, @DepAccountID, 0, {3}, N'{13}',
//                             '{1:yyyy-MM-dd HH:mm:ss}', '{1:yyyy-MM-dd HH:mm:ss}', 'Pay Bill', N'{0}', N'{0}', {12},
//                             {4}, {5}, '{6}', {7}, GETDATE(), 1
//                         );
//                     END
//                 ",
//                     paymentNo.Replace("'", "''"),
//                     dtPaymentDate,
//                     totalSettled,
//                     totalDepositApplied,
//                     sysId,
//                     whId,
//                     html.DomainID,
//                     (html.UserID ?? "200"),
//                     payee.Replace("'", "''"),
//                     totalPaymentAmount,
//                     chartOfAccountId,
//                     paymentModeName.Replace("'", "''"),
//                     vendorId,
//                     depositMemo.Replace("'", "''")
//                 );

//                 sbSql.AppendLine("SELECT @NewPaymentID AS PaymentID, N'" + paymentNo + "' AS PaymentNo;");
//                 sbSql.AppendLine("COMMIT TRANSACTION;");
//                 sbSql.AppendLine("END TRY");
//                 sbSql.AppendLine("BEGIN CATCH");
//                 sbSql.AppendLine("    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;");
//                 sbSql.AppendLine("    DECLARE @ErrMsg NVARCHAR(MAX) = ERROR_MESSAGE();");
//                 sbSql.AppendLine("    RAISERROR(@ErrMsg, 16, 1);");
//                 sbSql.AppendLine("END CATCH");

//                 DataSet dsResult = html.LoadData(sbSql.ToString());
//                 int paymentId = 0;
//                 if (dsResult != null && dsResult.Tables.Count > 0)
//                 {
//                     for (int t = dsResult.Tables.Count - 1; t >= 0; t--)
//                     {
//                         var dt = dsResult.Tables[t];
//                         if (dt.Rows.Count > 0 && dt.Columns.Contains("PaymentID"))
//                         {
//                             int.TryParse(dt.Rows[0]["PaymentID"].ToString(), out paymentId);
//                             break;
//                         }
//                     }
//                 }

//                 return Json(new 
//                 { 
//                     success = true, 
//                     message = "Payment " + paymentNo + " saved successfully!", 
//                     paymentNo = paymentNo, 
//                     pvNumber = paymentNo,
//                     officialReceiptNo = paymentNo,
//                     paymentId = paymentId 
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return Json(new { success = false, message = "Error saving payment: " + ex.Message });
//             }
//         }

//         // ─────────────────────────────────────────────────────────────────────
//         //  INVOICE / VOUCHER  →  Printable Payment Voucher matching Journal format
//         // ─────────────────────────────────────────────────────────────────────
//         [HttpGet]
//         public ActionResult Invoice(int id)
//         {
//             HTML html = new HTML();
//             string sysId = GetSystemID(html);
//             string whId = GetWarehouseID(html);

//             string sqlHeader = @"
//                 SELECT TOP 1
//                     vp.VendorPaymentID,
//                     ISNULL(vp.OfficalReceiptNo, 'PAY-' + CONVERT(VARCHAR, vp.VendorPaymentID)) AS PaymentNo,
//                     FORMAT(vp.PaymentDate, 'dd/MM/yyyy') AS PaymentDateFormatted,
//                     ISNULL(vp.Payee, ISNULL(v.VendorName, '')) AS VendorName,
//                     ISNULL(v.VendorCode, '') AS VendorCode,
//                     ISNULL(pm.PaymentMethod, ISNULL(vp.PaymentMode, 'Cash')) AS PaymentMethod,
//                     ISNULL(c.AccountNo + ' - ' + c.AccountName, ISNULL(vp.PaymentAccount, '')) AS PaymentAccount,
//                     vp.TotalPaymentAmount,
//                     ISNULL(vp.PaidBy, '') AS PaidBy,
//                     ISNULL(vp.Memo, '') AS Memo,
//                     ISNULL(vp.Reference, '') AS Reference,
//                     ISNULL(vp.ChequeNumber, '') AS ChequeNumber
//                 FROM dbo.TblVendorPayment vp
//                 LEFT JOIN dbo.TblVendor v ON v.VendorID = vp.VendorID
//                 LEFT JOIN dbo.TblPaymentMethod pm ON pm.PaymentMethodID = vp.PaymentModeID
//                 LEFT JOIN dbo.TblChartOfAccount c ON c.ChartOfAccountID = vp.ChartOfAccountID
//                 WHERE vp.VendorPaymentID = " + id;

//             DataSet dsHeader = html.LoadData(sqlHeader);
//             if (dsHeader == null || dsHeader.Tables.Count == 0 || dsHeader.Tables[0].Rows.Count == 0)
//             {
//                 return Content("<div class='alert alert-danger m-3'>Payment voucher #" + id + " not found.</div>");
//             }

//             ViewBag.PaymentHeader = dsHeader.Tables[0].Rows[0];

//             string sqlDetails = @"
//                 SELECT 
//                     vpd.VendorPaymentDetailID,
//                     vpd.BillNumber,
//                     vpd.PONumber,
//                     FORMAT(vpd.BillDate, 'dd/MM/yyyy') AS BillDateFormatted,
//                     FORMAT(vpd.DueDate, 'dd/MM/yyyy') AS DueDateFormatted,
//                     vpd.Amount AS GrandTotal,
//                     vpd.AmountPaid,
//                     vpd.PaymentAmount,
//                     vpd.AmountDue AS RemainBalance
//                 FROM dbo.TblVendorPaymentDetail vpd
//                 WHERE vpd.VendorPaymentID = " + id + @"
//                 ORDER BY vpd.VendorPaymentDetailID ASC";

//             DataSet dsDetails = html.LoadData(sqlDetails);
//             ViewBag.PaymentDetails = dsDetails != null && dsDetails.Tables.Count > 0 ? dsDetails.Tables[0] : new DataTable();

//             return View("~/Views/PayBill/Invoice.cshtml");
//         }

//         // ─────────────────────────────────────────────────────────────────────
//         //  DELETE PAYMENT
//         // ─────────────────────────────────────────────────────────────────────
//         [HttpPost]
//         public JsonResult DeletePayment(int id)
//         {
//             HTML html = new HTML();
//             try
//             {
//                 string sql = @"
//                     BEGIN TRANSACTION;
//                     BEGIN TRY
//                         -- Reverse TblBills amounts
//                         UPDATE b SET 
//                             b.AmountPaid = CASE WHEN b.AmountPaid >= vpd.PaymentAmount THEN b.AmountPaid - vpd.PaymentAmount ELSE 0 END,
//                             b.AmountDue = b.AmountDue + vpd.PaymentAmount,
//                             b.StatusID = CASE WHEN (b.GrandTotal - (CASE WHEN b.AmountPaid >= vpd.PaymentAmount THEN b.AmountPaid - vpd.PaymentAmount ELSE 0 END)) <= 0 THEN 10 ELSE 15 END
//                         FROM dbo.TblBills b
//                         INNER JOIN dbo.TblVendorPaymentDetail vpd ON vpd.BillID = b.BillID
//                         WHERE vpd.VendorPaymentID = " + id + @";

//                         -- Delete General Ledger transaction
//                         DECLARE @RcptNo NVARCHAR(100);
//                         SELECT @RcptNo = OfficalReceiptNo FROM dbo.TblVendorPayment WHERE VendorPaymentID = " + id + @";
//                         IF (@RcptNo IS NOT NULL AND @RcptNo <> '')
//                         BEGIN
//                             DECLARE @TxID INT;
//                             SELECT @TxID = TransactionID FROM dbo.TblAccount_Transaction WHERE (InvoiceNo = @RcptNo OR PVNo = @RcptNo) AND [Type] IN ('Pay Bill', 'Expense Payment');
//                             IF (@TxID IS NOT NULL AND @TxID > 0)
//                             BEGIN
//                                 DELETE FROM dbo.TblAccount_JournalEntries WHERE TransactionID = @TxID;
//                                 DELETE FROM dbo.TblAccount_Transaction WHERE TransactionID = @TxID;
//                             END
//                         END

//                         DELETE FROM dbo.TblVendorPaymentDetail WHERE VendorPaymentID = " + id + @";
//                         DELETE FROM dbo.TblVendorPayment WHERE VendorPaymentID = " + id + @";

//                         COMMIT TRANSACTION;
//                         SELECT 2 AS Result, 'Payment deleted successfully' AS Message;
//                     END TRY
//                     BEGIN CATCH
//                         IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
//                         DECLARE @Err NVARCHAR(MAX) = ERROR_MESSAGE();
//                         RAISERROR(@Err, 16, 1);
//                     END CATCH";

//                 DataSet ds = html.LoadData(sql);
//                 return Json(new { success = true, Result = 2, message = "Payment voucher deleted and bill balances restored." });
//             }
//             catch (Exception ex)
//             {
//                 return Json(new { success = false, Result = 1, message = "Error deleting payment: " + ex.Message });
//             }
//         }
//     }
// }
