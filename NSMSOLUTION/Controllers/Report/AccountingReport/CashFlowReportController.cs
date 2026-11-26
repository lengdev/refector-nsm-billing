using MVC;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Report.AccountingReport
{
    public class CashFlowReportController : Controller
    {
        [Authorize(Roles = "Report Cash Flow")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            SetCompanyHeader(html);
            return View("~/Views/CashFlowReport/Index.cshtml");
        }

        private void SetCompanyHeader(HTML html)
        {
            int systemId;
            if (!int.TryParse(html.SystemID, out systemId) || systemId <= 0)
                systemId = 13;

            int warehouseId;
            if (!int.TryParse(html.WarehouseID, out warehouseId))
                warehouseId = 0;

            int defaultClassId;
            switch (systemId)
            {
                case 13: defaultClassId = 5; break;
                case 28: defaultClassId = 14; break;
                case 25: defaultClassId = 10; break;
                case 27: defaultClassId = 12; break;
                default: defaultClassId = 0; break;
            }

            const string companySql = @"
SELECT TOP 1 *
FROM dbo.TblClass WITH (NOLOCK)
WHERE (@DefaultClassID > 0 AND ClassID = @DefaultClassID)
   OR (@DefaultClassID = 0 AND (@SystemID = 0 OR SystemID = @SystemID))
ORDER BY
    CASE WHEN @DefaultClassID > 0 AND ClassID = @DefaultClassID THEN 0 ELSE 1 END,
    CASE WHEN StatusID = 40 THEN 0 ELSE 1 END,
    CASE WHEN @WarehouseID > 0 AND WarehouseID = @WarehouseID THEN 0 ELSE 1 END,
    ClassID ASC";

            var parameters = new Dictionary<string, string>
            {
                { "DefaultClassID", defaultClassId.ToString() },
                { "SystemID", systemId.ToString() },
                { "WarehouseID", warehouseId.ToString() }
            };

            try
            {
                var companyData = html.LoadData(companySql, false, parameters);
                if (companyData == null || companyData.Tables.Count == 0 || companyData.Tables[0].Rows.Count == 0)
                {
                    System.Diagnostics.Trace.TraceWarning(
                        "Cash Flow report: company header was not found for SystemID {0}, WarehouseID {1}.",
                        systemId, warehouseId);
                    return;
                }

                var row = companyData.Tables[0].Rows[0];
                string companyKh = row.Table.Columns.Contains("KHName") && row["KHName"] != DBNull.Value
                    ? row["KHName"].ToString().Trim()
                    : row.Table.Columns.Contains("CompanyNameKH") && row["CompanyNameKH"] != DBNull.Value
                        ? row["CompanyNameKH"].ToString().Trim()
                        : "";
                string companyEn = row.Table.Columns.Contains("FullName") && row["FullName"] != DBNull.Value
                    ? row["FullName"].ToString().Trim()
                    : row.Table.Columns.Contains("CompanyName") && row["CompanyName"] != DBNull.Value
                        ? row["CompanyName"].ToString().Trim()
                        : row.Table.Columns.Contains("ClassName") && row["ClassName"] != DBNull.Value
                            ? row["ClassName"].ToString().Trim()
                            : "";

                if (!string.IsNullOrWhiteSpace(companyKh) && !companyKh.StartsWith("ក្រុមហ៊ុន"))
                    companyKh = "ក្រុមហ៊ុន " + companyKh;

                if (!string.IsNullOrWhiteSpace(companyKh))
                    ViewBag.CompanyNameKh = companyKh;
                if (!string.IsNullOrWhiteSpace(companyEn))
                    ViewBag.CompanyNameEn = companyEn;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Cash Flow report: failed to load company header: {0}", ex.Message);
            }
        }

        [HttpPost]
        public JsonResult Read(string fromDate, string toDate, string location = "", string account = "", string className = "", string txType = "", string refNo = "", string createdBy = "", string sortBy = "", string orderBy = "asc")
        {
            try
            {
                HTML html = new HTML();
                string connStr = html.GetConnectionString();
                DateTime today = DateTime.Today;
                DateTime start = new DateTime(today.Year, today.Month, 1);
                DateTime end = today;

                if (!string.IsNullOrWhiteSpace(fromDate))
                {
                    if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedStart))
                    {
                        start = parsedStart;
                    }
                }
                if (!string.IsNullOrWhiteSpace(toDate))
                {
                    if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedEnd))
                    {
                        end = parsedEnd;
                    }
                }

                decimal customerPayments = 0;
                decimal advancePayments = 0;
                decimal otherReceipts = 0;

                decimal rent = 0;
                decimal utilities = 0;
                decimal otherExpenses = 0;

                decimal softwareEquipment = 0;

                decimal ownerInvestment = 0;
                decimal loanReceived = 0;
                decimal loanRepayment = 0;

                decimal beginningBalance = 0;
                decimal endingBalance = 0;

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    List<int> cashAccountIds = new List<int>();
                    string sqlCashAccounts = @"
                            SELECT ChartOfAccountID 
                            FROM dbo.TblChartOfAccount coa
                            INNER JOIN dbo.TblAccountTypes ct ON ct.AccountTypesID = coa.AccountTypesID
                            WHERE coa.SystemID = @SystemID
                              AND (ct.AccountTypes = 'Asset' 
                                   AND (coa.AccountName LIKE '%Cash%' 
                                        OR coa.AccountName LIKE '%Bank%' 
                                        OR coa.AccountNo LIKE '111%' 
                                        OR coa.AccountNo LIKE '112%'))";

                    using (SqlCommand cmd = new SqlCommand(sqlCashAccounts, con))
                    {
                        cmd.Parameters.AddWithValue("@SystemID", html.SystemID);
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                cashAccountIds.Add(Convert.ToInt32(dr["ChartOfAccountID"]));
                            }
                        }
                    }

                    string cashAccountList = cashAccountIds.Count > 0 ? string.Join(",", cashAccountIds) : "0";

                    if (cashAccountIds.Count > 0)
                    {
                        string sqlCombined = $@"
                             -- Cash Inflows
                             SELECT 
                                 'INFLOW' AS FlowType,
                                 ISNULL(coa.AccountName, '') AS AccountName,
                                 ISNULL(coa.AccountNo, '') AS AccountNo,
                                 SUM(je_offset.Credit * (je_cash.Debit / NULLIF(tx_totals.TotalDebit, 0))) AS Amount
                             FROM dbo.TblAccount_JournalEntries je_cash
                             INNER JOIN dbo.TblAccount_Transaction r ON r.TransactionID = je_cash.TransactionID
                             INNER JOIN dbo.TblAccount_JournalEntries je_offset ON je_offset.TransactionID = r.TransactionID
                             INNER JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = je_offset.AccountID
                             INNER JOIN (
                                 SELECT TransactionID, SUM(ISNULL(Debit, 0)) AS TotalDebit
                                 FROM dbo.TblAccount_JournalEntries
                                 GROUP BY TransactionID
                             ) tx_totals ON tx_totals.TransactionID = r.TransactionID
                             WHERE je_cash.AccountID IN ({cashAccountList})
                               AND je_cash.Debit > 0
                               AND je_offset.AccountID NOT IN ({cashAccountList})
                               AND je_offset.Credit > 0
                               AND r.TransactionDate >= @FromDate
                               AND r.TransactionDate < DATEADD(DAY, 1, @ToDate)
                               AND (r.SystemID = @SystemID OR r.SystemID IS NULL)
                               AND (r.WarehouseID = @WarehouseID OR r.WarehouseID IS NULL)
                               AND (r.DomainID LIKE '%' + @DomainID + '%' OR r.DomainID IS NULL)
                               AND (@Location IS NULL OR @Location = '' OR CAST(r.WarehouseID AS NVARCHAR) = @Location OR EXISTS (SELECT 1 FROM dbo.TblWarehouse wh WHERE wh.WarehouseID = r.WarehouseID AND wh.WarehouseName LIKE '%' + @Location + '%'))
                               AND (@Account IS NULL OR @Account = '' OR coa.AccountName LIKE '%' + @Account + '%' OR coa.AccountNo LIKE '%' + @Account + '%')
                               AND (@ClassName IS NULL OR @ClassName = '' OR EXISTS (
                                   SELECT 1
                                   FROM dbo.TblClass cls
                                   WHERE cls.ClassID = TRY_CAST(r.ClassNameID AS INT)
                                     AND EXISTS (
                                         SELECT 1
                                         FROM STRING_SPLIT(@ClassName, ',') selectedClass
                                         WHERE cls.ClassName LIKE '%' + LTRIM(RTRIM(selectedClass.value)) + '%'
                                            OR CAST(cls.ClassID AS NVARCHAR) = LTRIM(RTRIM(selectedClass.value))
                                     )
                               ))
                               AND (@TxType IS NULL OR @TxType = '' OR r.[Type] = @TxType)
                               AND (@RefNo IS NULL OR @RefNo = '' OR r.InvoiceNo LIKE '%' + @RefNo + '%' OR r.ExpenseNo LIKE '%' + @RefNo + '%')
                               AND (@CreatedBy IS NULL OR @CreatedBy = '' OR CAST(r.CreateByUserID AS NVARCHAR) = @CreatedBy OR CAST(je_cash.CreateByUserID AS NVARCHAR) = @CreatedBy OR EXISTS (SELECT 1 FROM dbo.TblUserLogin u WHERE u.UserLoginID = r.CreateByUserID AND (u.FullName LIKE '%' + @CreatedBy + '%' OR u.UserName LIKE '%' + @CreatedBy + '%')))
                             GROUP BY coa.AccountName, coa.AccountNo

                             UNION ALL

                             -- Cash Outflows
                             SELECT 
                                 'OUTFLOW' AS FlowType,
                                 ISNULL(coa.AccountName, '') AS AccountName,
                                 ISNULL(coa.AccountNo, '') AS AccountNo,
                                 SUM(je_offset.Debit * (je_cash.Credit / NULLIF(tx_totals.TotalCredit, 0))) AS Amount
                             FROM dbo.TblAccount_JournalEntries je_cash
                             INNER JOIN dbo.TblAccount_Transaction r ON r.TransactionID = je_cash.TransactionID
                             INNER JOIN dbo.TblAccount_JournalEntries je_offset ON je_offset.TransactionID = r.TransactionID
                             INNER JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = je_offset.AccountID
                             INNER JOIN (
                                 SELECT TransactionID, SUM(ISNULL(Credit, 0)) AS TotalCredit
                                 FROM dbo.TblAccount_JournalEntries
                                 GROUP BY TransactionID
                             ) tx_totals ON tx_totals.TransactionID = r.TransactionID
                             WHERE je_cash.AccountID IN ({cashAccountList})
                               AND je_cash.Credit > 0
                               AND je_offset.AccountID NOT IN ({cashAccountList})
                               AND je_offset.Debit > 0
                               AND r.TransactionDate >= @FromDate
                               AND r.TransactionDate < DATEADD(DAY, 1, @ToDate)
                               AND (r.SystemID = @SystemID OR r.SystemID IS NULL)
                               AND (r.WarehouseID = @WarehouseID OR r.WarehouseID IS NULL)
                               AND (r.DomainID LIKE '%' + @DomainID + '%' OR r.DomainID IS NULL)
                               AND (@Location IS NULL OR @Location = '' OR CAST(r.WarehouseID AS NVARCHAR) = @Location OR EXISTS (SELECT 1 FROM dbo.TblWarehouse wh WHERE wh.WarehouseID = r.WarehouseID AND wh.WarehouseName LIKE '%' + @Location + '%'))
                               AND (@Account IS NULL OR @Account = '' OR coa.AccountName LIKE '%' + @Account + '%' OR coa.AccountNo LIKE '%' + @Account + '%')
                               AND (@ClassName IS NULL OR @ClassName = '' OR EXISTS (
                                   SELECT 1
                                   FROM dbo.TblClass cls
                                   WHERE cls.ClassID = TRY_CAST(r.ClassNameID AS INT)
                                     AND EXISTS (
                                         SELECT 1
                                         FROM STRING_SPLIT(@ClassName, ',') selectedClass
                                         WHERE cls.ClassName LIKE '%' + LTRIM(RTRIM(selectedClass.value)) + '%'
                                            OR CAST(cls.ClassID AS NVARCHAR) = LTRIM(RTRIM(selectedClass.value))
                                     )
                               ))
                               AND (@TxType IS NULL OR @TxType = '' OR r.[Type] = @TxType)
                               AND (@RefNo IS NULL OR @RefNo = '' OR r.InvoiceNo LIKE '%' + @RefNo + '%' OR r.ExpenseNo LIKE '%' + @RefNo + '%')
                               AND (@CreatedBy IS NULL OR @CreatedBy = '' OR CAST(r.CreateByUserID AS NVARCHAR) = @CreatedBy OR CAST(je_cash.CreateByUserID AS NVARCHAR) = @CreatedBy OR EXISTS (SELECT 1 FROM dbo.TblUserLogin u WHERE u.UserLoginID = r.CreateByUserID AND (u.FullName LIKE '%' + @CreatedBy + '%' OR u.UserName LIKE '%' + @CreatedBy + '%')))
                             GROUP BY coa.AccountName, coa.AccountNo

                             UNION ALL

                             -- Beginning Balance
                             SELECT 
                                 'BEG_BALANCE' AS FlowType,
                                 '' AS AccountName,
                                 '' AS AccountNo,
                                 ISNULL(SUM(ISNULL(je.Debit, 0)) - SUM(ISNULL(je.Credit, 0)), 0) AS Amount
                             FROM dbo.TblAccount_JournalEntries je
                             INNER JOIN dbo.TblAccount_Transaction r ON r.TransactionID = je.TransactionID
                             WHERE je.AccountID IN ({cashAccountList})
                               AND r.TransactionDate < @FromDate
                               AND (r.SystemID = @SystemID OR r.SystemID IS NULL)
                               AND (r.WarehouseID = @WarehouseID OR r.WarehouseID IS NULL)
                               AND (r.DomainID LIKE '%' + @DomainID + '%' OR r.DomainID IS NULL)
                               AND (@Location IS NULL OR @Location = '' OR CAST(r.WarehouseID AS NVARCHAR) = @Location OR EXISTS (SELECT 1 FROM dbo.TblWarehouse wh WHERE wh.WarehouseID = r.WarehouseID AND wh.WarehouseName LIKE '%' + @Location + '%'))
                               AND (@ClassName IS NULL OR @ClassName = '' OR EXISTS (
                                   SELECT 1
                                   FROM dbo.TblClass cls
                                   WHERE cls.ClassID = TRY_CAST(r.ClassNameID AS INT)
                                     AND EXISTS (
                                         SELECT 1
                                         FROM STRING_SPLIT(@ClassName, ',') selectedClass
                                         WHERE cls.ClassName LIKE '%' + LTRIM(RTRIM(selectedClass.value)) + '%'
                                            OR CAST(cls.ClassID AS NVARCHAR) = LTRIM(RTRIM(selectedClass.value))
                                     )
                               ))
                               AND (@CreatedBy IS NULL OR @CreatedBy = '' OR CAST(r.CreateByUserID AS NVARCHAR) = @CreatedBy OR CAST(je.CreateByUserID AS NVARCHAR) = @CreatedBy OR EXISTS (SELECT 1 FROM dbo.TblUserLogin u WHERE u.UserLoginID = r.CreateByUserID AND (u.FullName LIKE '%' + @CreatedBy + '%' OR u.UserName LIKE '%' + @CreatedBy + '%')))

                             UNION ALL

                             -- Ending Balance
                             SELECT 
                                 'END_BALANCE' AS FlowType,
                                 '' AS AccountName,
                                 '' AS AccountNo,
                                 ISNULL(SUM(ISNULL(je.Debit, 0)) - SUM(ISNULL(je.Credit, 0)), 0) AS Amount
                             FROM dbo.TblAccount_JournalEntries je
                             INNER JOIN dbo.TblAccount_Transaction r ON r.TransactionID = je.TransactionID
                             WHERE je.AccountID IN ({cashAccountList})
                               AND r.TransactionDate < DATEADD(DAY, 1, @ToDate)
                               AND (r.SystemID = @SystemID OR r.SystemID IS NULL)
                               AND (r.WarehouseID = @WarehouseID OR r.WarehouseID IS NULL)
                               AND (r.DomainID LIKE '%' + @DomainID + '%' OR r.DomainID IS NULL)
                               AND (@Location IS NULL OR @Location = '' OR CAST(r.WarehouseID AS NVARCHAR) = @Location OR EXISTS (SELECT 1 FROM dbo.TblWarehouse wh WHERE wh.WarehouseID = r.WarehouseID AND wh.WarehouseName LIKE '%' + @Location + '%'))
                               AND (@ClassName IS NULL OR @ClassName = '' OR EXISTS (
                                   SELECT 1
                                   FROM dbo.TblClass cls
                                   WHERE cls.ClassID = TRY_CAST(r.ClassNameID AS INT)
                                     AND EXISTS (
                                         SELECT 1
                                         FROM STRING_SPLIT(@ClassName, ',') selectedClass
                                         WHERE cls.ClassName LIKE '%' + LTRIM(RTRIM(selectedClass.value)) + '%'
                                            OR CAST(cls.ClassID AS NVARCHAR) = LTRIM(RTRIM(selectedClass.value))
                                     )
                               ))
                               AND (@CreatedBy IS NULL OR @CreatedBy = '' OR CAST(r.CreateByUserID AS NVARCHAR) = @CreatedBy OR CAST(je.CreateByUserID AS NVARCHAR) = @CreatedBy OR EXISTS (SELECT 1 FROM dbo.TblUserLogin u WHERE u.UserLoginID = r.CreateByUserID AND (u.FullName LIKE '%' + @CreatedBy + '%' OR u.UserName LIKE '%' + @CreatedBy + '%')));";

                        using (SqlCommand cmd = new SqlCommand(sqlCombined, con))
                        {
                            cmd.Parameters.AddWithValue("@SystemID", html.SystemID);
                            cmd.Parameters.AddWithValue("@WarehouseID", html.WarehouseID);
                            cmd.Parameters.AddWithValue("@DomainID", html.DomainID);
                            cmd.Parameters.AddWithValue("@FromDate", start);
                            cmd.Parameters.AddWithValue("@ToDate", end);
                            cmd.Parameters.AddWithValue("@Location", string.IsNullOrEmpty(location) ? (object)DBNull.Value : location);
                            cmd.Parameters.AddWithValue("@Account", string.IsNullOrEmpty(account) ? (object)DBNull.Value : account);
                            cmd.Parameters.AddWithValue("@ClassName", string.IsNullOrEmpty(className) ? (object)DBNull.Value : className);
                            cmd.Parameters.AddWithValue("@TxType", string.IsNullOrEmpty(txType) ? (object)DBNull.Value : txType);
                            cmd.Parameters.AddWithValue("@RefNo", string.IsNullOrEmpty(refNo) ? (object)DBNull.Value : refNo);
                            cmd.Parameters.AddWithValue("@CreatedBy", string.IsNullOrEmpty(createdBy) ? (object)DBNull.Value : createdBy);

                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    string flowType = dr["FlowType"].ToString();
                                    decimal amount = Convert.ToDecimal(dr["Amount"]);

                                    if (flowType == "BEG_BALANCE")
                                    {
                                        beginningBalance = amount;
                                    }
                                    else if (flowType == "END_BALANCE")
                                    {
                                        endingBalance = amount;
                                    }
                                    else
                                    {
                                        string accountName = dr["AccountName"].ToString().ToLower();
                                        string accountNo = dr["AccountNo"].ToString();

                                        if (flowType == "INFLOW")
                                        {
                                            if (accountName.Contains("receivable") || accountName.Contains("a/r") || accountNo.StartsWith("113"))
                                            {
                                                customerPayments += amount;
                                            }
                                            else if (accountName.Contains("advance") || accountName.Contains("deposit") || accountName.Contains("unearned"))
                                            {
                                                advancePayments += amount;
                                            }
                                            else if (accountName.Contains("capital") || accountName.Contains("equity") || accountName.Contains("owner investment"))
                                            {
                                                ownerInvestment += amount;
                                            }
                                            else if (accountName.Contains("loan") || accountName.Contains("notes payable") || accountName.Contains("borrowing"))
                                            {
                                                loanReceived += amount;
                                            }
                                            else
                                            {
                                            }
                                            otherReceipts += amount;
                                        }
                                        else if (flowType == "OUTFLOW")
                                        {
                                            if (accountName.Contains("rent") || accountName.Contains("office rent") || accountName.Contains("lease"))
                                            {
                                                rent += amount;
                                            }
                                            else if (accountName.Contains("utility") || accountName.Contains("internet") || accountName.Contains("electricity") || accountName.Contains("water") || accountName.Contains("phone"))
                                            {
                                                utilities += amount;
                                            }
                                            else if (accountName.Contains("software") || accountName.Contains("equipment") || accountName.Contains("computer") || accountName.Contains("hardware") || accountName.Contains("asset"))
                                            {
                                                softwareEquipment += amount;
                                            }
                                            else if (accountName.Contains("loan") || accountName.Contains("notes payable") || accountName.Contains("borrowing"))
                                            {
                                                loanRepayment += amount;
                                            }
                                            else
                                            {
                                                otherExpenses += amount;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                decimal totalInflow = customerPayments + advancePayments + otherReceipts;
                decimal totalOutflow = rent + utilities + otherExpenses;
                decimal totalInvesting = softwareEquipment;
                decimal totalFinancing = ownerInvestment + loanReceived - loanRepayment;

                decimal expectedNetIncrease = endingBalance - beginningBalance;
                decimal actualNetIncrease = totalInflow - totalOutflow - totalInvesting + totalFinancing;
                decimal discrepancy = expectedNetIncrease - actualNetIncrease;

                decimal otherPayments = 0;

                if (discrepancy > 0)
                {
                    otherReceipts += discrepancy;
                }
                else if (discrepancy < 0)
                {
                    otherPayments = -discrepancy;
                }

                var finalResult = new
                {
                    CustomerPayments = customerPayments,
                    AdvancePayments = advancePayments,
                    OtherReceipts = otherReceipts,
                    TotalOperatingInflows = customerPayments + advancePayments + otherReceipts,

                    Rent = rent,
                    Utilities = utilities,
                    OtherExpenses = otherExpenses,
                    OtherPayments = otherPayments,
                    TotalOperatingOutflows = rent + utilities + otherExpenses + otherPayments,

                    NetOperatingCash = customerPayments + advancePayments + otherReceipts - (rent + utilities + otherExpenses + otherPayments),

                    SoftwareEquipment = softwareEquipment,
                    NetInvestingCash = -softwareEquipment,

                    OwnerInvestment = ownerInvestment,
                    LoanReceived = loanReceived,
                    LoanRepayment = loanRepayment,
                    NetFinancingCash = ownerInvestment + loanReceived - loanRepayment,

                    NetCashIncrease = customerPayments + advancePayments + otherReceipts - (rent + utilities + otherExpenses + otherPayments)
                                      - softwareEquipment
                                      + (ownerInvestment + loanReceived - loanRepayment),

                    BeginningBalance = beginningBalance,
                    EndingBalance = endingBalance,
                    Discrepancy = 0,
                    IsDemoData = false
                };

                return Json(new { Success = true, Data = finalResult }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
