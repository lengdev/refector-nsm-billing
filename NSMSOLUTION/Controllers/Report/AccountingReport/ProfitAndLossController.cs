using MVC;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Report.AccountingReport
{
    public class ProfitAndLossController : Controller
    {
        [Authorize(Roles = "Report Profit And Loss")]
        public ActionResult Index()
        {
            try
            {
                HTML html = new HTML();
                int sysId = 13;
                if (!string.IsNullOrEmpty(Request["SystemID"])) int.TryParse(Request["SystemID"], out sysId);
                else if (!string.IsNullOrEmpty(html.SystemID)) int.TryParse(html.SystemID, out sysId);
                int whId = 0;
                if (!string.IsNullOrEmpty(Request["WarehouseID"])) int.TryParse(Request["WarehouseID"], out whId);
                else if (!string.IsNullOrEmpty(html.WarehouseID)) int.TryParse(html.WarehouseID, out whId);

                int defaultClassId;
                switch (sysId)
                {
                    case 13: defaultClassId = 5; break;
                    case 28: defaultClassId = 14; break;
                    case 25: defaultClassId = 10; break;
                    case 27: defaultClassId = 12; break;
                    default: defaultClassId = 0; break;
                }

                string compKh = "";
                string compEn = "";

                if (sysId > 0)
                {
                    string connStr = html.GetConnectionString();
                    using (var con = new System.Data.SqlClient.SqlConnection(connStr))
                    {
                        con.Open();
                        string sqlComp = @"
                            SELECT TOP 1 *
                            FROM dbo.TblClass WITH (NOLOCK)
                            WHERE (@DefaultClassID > 0 AND ClassID = @DefaultClassID)
                               OR (@DefaultClassID = 0 AND (@SysID = 0 OR SystemID = @SysID))
                            ORDER BY
                                CASE WHEN @DefaultClassID > 0 AND ClassID = @DefaultClassID THEN 0 ELSE 1 END,
                                CASE WHEN StatusID = 40 THEN 0 ELSE 1 END,
                                CASE WHEN @WarehouseID > 0 AND WarehouseID = @WarehouseID THEN 0 ELSE 1 END,
                                ClassID ASC";
                        using (var cmd = new System.Data.SqlClient.SqlCommand(sqlComp, con))
                        {
                            cmd.Parameters.AddWithValue("@DefaultClassID", defaultClassId);
                            cmd.Parameters.AddWithValue("@SysID", sysId);
                            cmd.Parameters.AddWithValue("@WarehouseID", whId);
                            using (var dr = cmd.ExecuteReader())
                            {
                                if (dr.Read())
                                {
                                    for (int i = 0; i < dr.FieldCount; i++)
                                    {
                                        string name = dr.GetName(i);
                                        if (string.Equals(name, "CompanyNameKH", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "KHName", StringComparison.OrdinalIgnoreCase))
                                            compKh = dr[i] != DBNull.Value ? dr[i].ToString().Trim() : compKh;
                                        if (string.Equals(name, "FullName", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "CompanyName", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "CompanyNameEN", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "ClassName", StringComparison.OrdinalIgnoreCase))
                                            compEn = dr[i] != DBNull.Value ? dr[i].ToString().Trim() : compEn;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(compKh))
                {
                    string clean = compKh.Trim(' ', '\t', '\r', '\n', '\u200B', '\u00A0');
                    if (!clean.StartsWith("ក្រុមហ៊ុន")) compKh = "ក្រុមហ៊ុន " + clean;
                    else compKh = clean;
                }

                ViewBag.SystemID    = sysId;
                ViewBag.WarehouseID = whId;

                ViewBag.CompanyNameKh = compKh;
                ViewBag.CompanyNameEn = compEn;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Profit and Loss report: failed to load company header: {0}", ex.Message);
            }

            return View("~/Views/ProfitAndLoss/index.cshtml");
        }

        [HttpPost]
        public JsonResult Read(DateTime? fromDate, DateTime? toDate, string location = "", string account = "", string name = "", string className = "", string txType = "", string refNo = "", string createdBy = "", string displayColumns = "Total")
        {
            try
            {
                HTML html = new HTML();
                string connStr = html.GetConnectionString();
                var data = new System.Collections.Generic.List<object>();

                int sysId = 13;
                if (!string.IsNullOrEmpty(Request["SystemID"])) int.TryParse(Request["SystemID"], out sysId);
                else if (!string.IsNullOrEmpty(html.SystemID)) int.TryParse(html.SystemID, out sysId);

                int whId = 0;
                if (!string.IsNullOrEmpty(Request["WarehouseID"])) int.TryParse(Request["WarehouseID"], out whId);
                if (whId == 0 && !string.IsNullOrEmpty(html.WarehouseID)) int.TryParse(html.WarehouseID, out whId);

                string domId = html.DomainID ?? "";

                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(connStr))   
                {
                    string sql = @"
-- Multi-select filters parsing (XML split compatible with all SQL Server versions)
DECLARE @LocationList TABLE (Val NVARCHAR(200));
IF @Location IS NOT NULL AND LTRIM(RTRIM(@Location)) <> ''
BEGIN
    INSERT INTO @LocationList (Val)
    SELECT DISTINCT LTRIM(RTRIM(m.n.value('.[1]', 'NVARCHAR(200)')))
    FROM (SELECT CAST('<m>' + REPLACE(REPLACE(REPLACE(@Location, '&', '&amp;'), '<', '&lt;'), ',', '</m><m>') + '</m>' AS XML) AS x) t
    CROSS APPLY x.nodes('/m') AS m(n)
    WHERE LTRIM(RTRIM(m.n.value('.[1]', 'NVARCHAR(200)'))) <> '';
END

DECLARE @AccountList TABLE (Val NVARCHAR(200));
IF @Account IS NOT NULL AND LTRIM(RTRIM(@Account)) <> ''
BEGIN
    INSERT INTO @AccountList (Val)
    SELECT DISTINCT LTRIM(RTRIM(m.n.value('.[1]', 'NVARCHAR(200)')))
    FROM (SELECT CAST('<m>' + REPLACE(REPLACE(REPLACE(@Account, '&', '&amp;'), '<', '&lt;'), ',', '</m><m>') + '</m>' AS XML) AS x) t
    CROSS APPLY x.nodes('/m') AS m(n)
    WHERE LTRIM(RTRIM(m.n.value('.[1]', 'NVARCHAR(200)'))) <> '';
END

DECLARE @ClassList TABLE (Val NVARCHAR(200), ClassID INT, ClassName NVARCHAR(250));
IF @ClassName IS NOT NULL AND LTRIM(RTRIM(@ClassName)) <> ''
BEGIN
    INSERT INTO @ClassList (Val, ClassID, ClassName)
    SELECT DISTINCT 
        s.Item,
        COALESCE(c.ClassID, TRY_CAST(s.Item AS INT)),
        COALESCE(c.ClassName, s.Item)
    FROM (
        SELECT LTRIM(RTRIM(m.n.value('.[1]', 'NVARCHAR(200)'))) AS Item
        FROM (SELECT CAST('<m>' + REPLACE(REPLACE(REPLACE(@ClassName, '&', '&amp;'), '<', '&lt;'), ',', '</m><m>') + '</m>' AS XML) AS x) t
        CROSS APPLY x.nodes('/m') AS m(n)
        WHERE LTRIM(RTRIM(m.n.value('.[1]', 'NVARCHAR(200)'))) <> ''
          AND LTRIM(RTRIM(m.n.value('.[1]', 'NVARCHAR(200)'))) NOT IN ('--- All Classes ---', 'All')
    ) s
    LEFT JOIN dbo.TblClass c WITH (NOLOCK) ON (LTRIM(RTRIM(c.ClassName)) = s.Item OR CAST(c.ClassID AS NVARCHAR(50)) = s.Item)
      AND (@SysID = 0 OR c.SystemID = @SysID OR c.SystemID IS NULL)
      AND (@WhID = 0 OR c.WarehouseID = @WhID OR c.WarehouseID IS NULL);

    IF EXISTS (SELECT 1 FROM dbo.TblClass WITH (NOLOCK) WHERE LTRIM(RTRIM(ClassName)) = LTRIM(RTRIM(@ClassName)))
    BEGIN
        INSERT INTO @ClassList (Val, ClassID, ClassName)
        SELECT TOP 1 LTRIM(RTRIM(@ClassName)), ClassID, ClassName
        FROM dbo.TblClass WITH (NOLOCK)
        WHERE LTRIM(RTRIM(ClassName)) = LTRIM(RTRIM(@ClassName))
          AND NOT EXISTS (SELECT 1 FROM @ClassList WHERE Val = LTRIM(RTRIM(@ClassName)) OR ClassName = LTRIM(RTRIM(@ClassName)));
    END
END

DECLARE @TxTypeList TABLE (Val NVARCHAR(200));
IF @TxType IS NOT NULL AND LTRIM(RTRIM(@TxType)) <> ''
BEGIN
    INSERT INTO @TxTypeList (Val)
    SELECT DISTINCT LTRIM(RTRIM(m.n.value('.[1]', 'NVARCHAR(200)')))
    FROM (SELECT CAST('<m>' + REPLACE(REPLACE(REPLACE(@TxType, '&', '&amp;'), '<', '&lt;'), ',', '</m><m>') + '</m>' AS XML) AS x) t
    CROSS APPLY x.nodes('/m') AS m(n)
    WHERE LTRIM(RTRIM(m.n.value('.[1]', 'NVARCHAR(200)'))) <> '';
END

DECLARE @CreatedByList TABLE (Val NVARCHAR(200));
IF @CreatedBy IS NOT NULL AND LTRIM(RTRIM(@CreatedBy)) <> ''
BEGIN
    INSERT INTO @CreatedByList (Val)
    SELECT DISTINCT LTRIM(RTRIM(m.n.value('.[1]', 'NVARCHAR(200)')))
    FROM (SELECT CAST('<m>' + REPLACE(REPLACE(REPLACE(@CreatedBy, '&', '&amp;'), '<', '&lt;'), ',', '</m><m>') + '</m>' AS XML) AS x) t
    CROSS APPLY x.nodes('/m') AS m(n)
    WHERE LTRIM(RTRIM(m.n.value('.[1]', 'NVARCHAR(200)'))) <> '';
END

;WITH AllPLRows AS (
    -- 1. From Journal Entries: ONLY Income Statement Accounts (Revenue, Other Income, COGS, Expense, OtherExpense)
    SELECT 
        CASE 
            WHEN ISNULL(at.AccountTypes, '') IN ('Other Expense', 'OtherExpense', 'Other Expenses') 
              OR ISNULL(cat.AccountCategory, '') LIKE '%Other Expense%' 
              OR ISNULL(cat.AccountCategory, '') LIKE '%OtherExpense%' THEN 'OtherExpense'
            WHEN ISNULL(cat.AccountCategory, '') LIKE '%COGS%' 
              OR ISNULL(cat.AccountCategory, '') LIKE '%Cost%' 
              OR ISNULL(at.AccountTypes, '') LIKE '%Cost%' 
              OR ISNULL(at.AccountTypes, '') LIKE '%COGS%' THEN 'COGS'
            WHEN ISNULL(at.AccountTypes, '') LIKE '%Revenue%' 
              OR ISNULL(at.AccountTypes, '') LIKE '%Income%' 
              OR ISNULL(cat.AccountCategory, '') LIKE '%Revenue%' 
              OR ISNULL(cat.AccountCategory, '') LIKE '%Income%' THEN 'Revenue'
            ELSE 'Expense' 
        END AS AccountTypes,
        ISNULL(pcoa.AccountNo, '') AS ParentAccountNo,
        ISNULL(pcoa.AccountName, '') AS ParentAccountName,
        COALESCE(NULLIF(coa.AccountNo, ''), def_coa.AccountNo, '522000') AS AccountNo,
        COALESCE(NULLIF(coa.AccountName, ''), def_coa.AccountName, 'Operating Expense') AS AccountName,
        ISNULL(cat.AccountCategory, 'Expense') AS AccountCategory,
        CASE 
            WHEN at.AccountTypes LIKE '%Revenue%' OR at.AccountTypes LIKE '%Income%' OR cat.AccountCategory LIKE '%Revenue%' OR cat.AccountCategory LIKE '%Income%'
                THEN ISNULL(je.Credit,0) - ISNULL(je.Debit,0)
            ELSE ISNULL(je.Debit,0) - ISNULL(je.Credit,0)
        END AS Amount,
        CASE 
            WHEN je.ExpenseID > 0 OR LOWER(LTRIM(RTRIM(COALESCE(act.[Type],'')))) IN ('expense', 'expenses', 'write check', 'writecheck', 'check')
                THEN COALESCE(exp.PaymentDate, exp.PaidOnDate, exp.CreatedDate, act.TransactionDate, je.EntryDate, act.CreateDate, je.CreateDate)
            ELSE COALESCE(act.TransactionDate, je.EntryDate, act.CreateDate, je.CreateDate)
        END AS TxDate,
        COALESCE(act.SystemID, je.SystemID, exp.SystemID, coa.SystemID, 0) AS TxSysID,
        COALESCE(act.WarehouseID, je.WarehouseID, exp.WarehouseID, wh.WarehouseID, 0) AS TxWhID,
        COALESCE(act.DomainID, je.DomainID, exp.DomainID, coa.DomainID, '') AS TxDomID,
        COALESCE(wh.WarehouseName, wh_exp.WarehouseName, '') AS LocationName,
        CAST(COALESCE(act.WarehouseID, je.WarehouseID, exp.WarehouseID, 0) AS NVARCHAR(50)) AS LocationID,
        COALESCE(c.CustomerName, c.CustomerNameLatin, v.VendorName, v_exp.VendorName, '') AS PartyName,
        COALESCE(act.InvoiceNo, je.InvoiceNo, act.ExpenseNo, exp.[No], '') AS RefNo,
        COALESCE(act.[Type], 'Journal') AS TxType,
        COALESCE(u_act.FullName, u_act.UserName, u_je.FullName, u_je.UserName, u_exp.FullName, u_exp.UserName, CAST(COALESCE(act.CreateByUserID, je.CreateByUserID, exp.CreateByUserID, 0) AS NVARCHAR(50))) AS CreatedByUserID,
        COALESCE(cls_je.ClassID, cls_act.ClassID, cls_exp.ClassID, TRY_CAST(je.ClassNameID AS INT), TRY_CAST(act.ClassNameID AS INT), TRY_CAST(exp.ClassName AS INT), 0) AS ClassID,
        COALESCE(cls_je.ClassName, cls_act.ClassName, cls_exp.ClassName, CAST(je.ClassNameID AS NVARCHAR(100)), CAST(act.ClassNameID AS NVARCHAR(100)), exp.ClassName, '') AS ClassName
    FROM dbo.TblAccount_JournalEntries je
    LEFT JOIN dbo.TblAccount_Transaction act ON act.TransactionID = je.TransactionID
    LEFT JOIN dbo.Expenses exp ON exp.ExpenseID = je.ExpenseID
    LEFT JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = je.AccountID
    LEFT JOIN dbo.TblChartOfAccount pcoa ON pcoa.ChartOfAccountID = coa.ParentAccountID
    LEFT JOIN dbo.TblAccountCategories cat ON cat.AccountCategoryID = coa.AccountCategoryID
    LEFT JOIN dbo.TblAccountTypes at ON (at.AccountTypesID = coa.AccountTypesID OR at.AccountTypesID = cat.AccountTypesID)
    LEFT JOIN dbo.TblWarehouse wh ON (wh.WarehouseID = act.WarehouseID OR wh.WarehouseID = je.WarehouseID)
    LEFT JOIN dbo.TblWarehouse wh_exp ON wh_exp.WarehouseID = exp.WarehouseID
    LEFT JOIN dbo.TblCustomer c ON c.CustomerID = je.CustomerID
    LEFT JOIN dbo.TblVendor v ON (v.VendorID = act.PayeeID OR v.VendorID = je.CustomerID)
    LEFT JOIN dbo.TblVendor v_exp ON v_exp.VendorID = exp.PayeeID
    LEFT JOIN dbo.TblClass cls_je ON (cls_je.ClassID = TRY_CAST(je.ClassNameID AS INT) AND cls_je.ClassID > 0)
    LEFT JOIN dbo.TblClass cls_act ON (cls_act.ClassID = TRY_CAST(act.ClassNameID AS INT) AND cls_act.ClassID > 0)
    LEFT JOIN dbo.TblClass cls_exp ON (cls_exp.ClassID = TRY_CAST(exp.ClassName AS INT) AND cls_exp.ClassID > 0)
    LEFT JOIN dbo.TblUserLogin u_act ON u_act.UserLoginID = act.CreateByUserID
    LEFT JOIN dbo.TblUserLogin u_je ON u_je.UserLoginID = je.CreateByUserID
    LEFT JOIN dbo.TblUserLogin u_exp ON u_exp.UserLoginID = exp.CreateByUserID
    OUTER APPLY (
        SELECT TOP 1 coa_d.AccountNo, coa_d.AccountName
        FROM dbo.TblChartOfAccount coa_d
        INNER JOIN dbo.TblAccountTypes at_d ON at_d.AccountTypesID = coa_d.AccountTypesID
        WHERE (coa_d.SystemID = @SysID OR @SysID = 0)
          AND at_d.AccountTypes IN ('Expense', 'Expenses', 'Operating Expense')
          AND (coa_d.AccountName LIKE '%Operating%' OR coa_d.AccountNo LIKE '522%' OR coa_d.AccountNo LIKE '6%')
        ORDER BY CASE WHEN coa_d.AccountNo LIKE '522%' THEN 1 WHEN coa_d.AccountName LIKE '%Operating%' THEN 2 ELSE 3 END
    ) def_coa
    WHERE (
        coa.AccountNo NOT LIKE '1%' 
        AND coa.AccountNo NOT LIKE '2%' 
        AND coa.AccountNo NOT LIKE '3%'
        AND ISNULL(at.AccountTypes, '') NOT IN (
            'Asset', 'Assets', 'Current Asset', 'Current Assets', 'Fixed Asset', 'Fixed Assets', 'Other Asset', 'Other Assets', 'Other Current Asset', 'Other Current Assets', 'Non-Current Asset', 'Non-Current Assets',
            'Liability', 'Liabilities', 'Current Liability', 'Current Liabilities', 'Long Term Liability', 'Long Term Liabilities', 'Other Current Liability', 'Other Current Liabilities', 'Non-Current Liability', 'Non-Current Liabilities',
            'Equity', 'Equities', 'Owner Equity', 'Retained Earnings',
            'Bank', 'Cash', 'Cash and Cash Equivalents', 'Accounts Receivable', 'Accounts Payable', 'Credit Card', 'Payroll Liabilities'
        )
        AND ISNULL(cat.AccountCategory, '') NOT IN (
            'Asset', 'Assets', 'Current Asset', 'Current Assets', 'Fixed Asset', 'Fixed Assets', 'Other Asset', 'Other Assets', 'Other Current Asset', 'Other Current Assets',
            'Liability', 'Liabilities', 'Current Liability', 'Current Liabilities', 'Long Term Liability', 'Long Term Liabilities', 'Other Current Liability', 'Other Current Liabilities',
            'Equity', 'Owner Equity', 'Bank', 'Cash', 'Accounts Receivable', 'Accounts Payable'
        )
        AND (
            (
                at.AccountTypes IN ('Revenue', 'Other Income', 'Expense', 'Expenses', 'Other Expense', 'OtherExpense', 'Operating Expense', 'Cost', 'COGS', 'Cost of Goods Sold', 'Income', 'Sales')
                OR cat.AccountCategory LIKE '%Expense%'
                OR cat.AccountCategory LIKE '%Revenue%'
                OR cat.AccountCategory LIKE '%Income%'
                OR cat.AccountCategory LIKE '%Cost%'
                OR cat.AccountCategory LIKE '%COGS%'
                OR cat.[Statement] LIKE '%Profit%'
                OR cat.[Statement] LIKE '%Income%'
                OR coa.AccountNo LIKE '4%'
                OR coa.AccountNo LIKE '5%'
                OR coa.AccountNo LIKE '6%'
                OR coa.AccountNo LIKE '7%'
                OR coa.AccountNo LIKE '8%'
            )
            OR (
                (je.ExpenseID > 0 OR COALESCE(act.[Type], '') = 'Expense')
                AND ISNULL(je.Debit, 0) > 0
            )
        )
    )

    UNION ALL

    -- 2. Direct Expenses from Expenses & ExpenseDetails that have no debit JournalEntries
    SELECT 
        CASE 
            WHEN ISNULL(at.AccountTypes, '') IN ('Other Expense', 'OtherExpense', 'Other Expenses') 
              OR ISNULL(cat.AccountCategory, '') LIKE '%Other Expense%' 
              OR ISNULL(cat.AccountCategory, '') LIKE '%OtherExpense%' THEN 'OtherExpense'
            ELSE 'Expense' 
        END AS AccountTypes,
        ISNULL(pcoa.AccountNo, '') AS ParentAccountNo,
        ISNULL(pcoa.AccountName, '') AS ParentAccountName,
        COALESCE(NULLIF(coa.AccountNo, ''), def_coa.AccountNo, '522000') AS AccountNo,
        COALESCE(NULLIF(coa.AccountName, ''), def_coa.AccountName, 'Operating Expense') AS AccountName,
        ISNULL(cat.AccountCategory, 'Expense') AS AccountCategory,
        ISNULL(ed.ItemGrandTotal, 0) AS Amount,
        e.PaymentDate AS TxDate,
        e.SystemID AS TxSysID,
        e.WarehouseID AS TxWhID,
        e.DomainID AS TxDomID,
        ISNULL(wh.WarehouseName, '') AS LocationName,
        CAST(e.WarehouseID AS NVARCHAR(50)) AS LocationID,
        ISNULL(v.VendorName, '') AS PartyName,
        ISNULL(e.[No], '') AS RefNo,
        'Expense' AS TxType,
        COALESCE(u_e.FullName, u_e.UserName, CAST(COALESCE(e.CreateByUserID, e.ApprovedByUserID, e.PostedByUserID, 0) AS NVARCHAR(50))) AS CreatedByUserID,
        COALESCE(cl.ClassID, TRY_CAST(e.ClassName AS INT), 0) AS ClassID,
        COALESCE(cl.ClassName, e.ClassName, '') AS ClassName
    FROM dbo.Expenses e
    INNER JOIN dbo.ExpenseDetails ed ON e.ExpenseID = ed.ExpenseID
    LEFT JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = TRY_CAST(ed.ExpenseCategory AS INT)
    LEFT JOIN dbo.TblChartOfAccount pcoa ON pcoa.ChartOfAccountID = coa.ParentAccountID
    LEFT JOIN dbo.TblAccountCategories cat ON cat.AccountCategoryID = coa.AccountCategoryID
    LEFT JOIN dbo.TblAccountTypes at ON (at.AccountTypesID = coa.AccountTypesID OR at.AccountTypesID = cat.AccountTypesID)
    LEFT JOIN dbo.TblVendor v ON e.PayeeID = v.VendorID
    LEFT JOIN dbo.TblWarehouse wh ON wh.WarehouseID = e.WarehouseID
    LEFT JOIN dbo.TblClass cl ON (cl.ClassID = TRY_CAST(e.ClassName AS INT) AND cl.ClassID > 0)
    LEFT JOIN dbo.TblUserLogin u_e ON u_e.UserLoginID = e.CreateByUserID
    OUTER APPLY (
        SELECT TOP 1 coa_d.AccountNo, coa_d.AccountName
        FROM dbo.TblChartOfAccount coa_d
        INNER JOIN dbo.TblAccountTypes at_d ON at_d.AccountTypesID = coa_d.AccountTypesID
        WHERE (coa_d.SystemID = @SysID OR @SysID = 0)
          AND at_d.AccountTypes IN ('Expense', 'Expenses', 'Operating Expense')
          AND (coa_d.AccountName LIKE '%Operating%' OR coa_d.AccountNo LIKE '522%' OR coa_d.AccountNo LIKE '6%')
        ORDER BY CASE WHEN coa_d.AccountNo LIKE '522%' THEN 1 WHEN coa_d.AccountName LIKE '%Operating%' THEN 2 ELSE 3 END
    ) def_coa
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.TblAccount_JournalEntries je2 
        WHERE je2.ExpenseID = e.ExpenseID 
          AND je2.ExpenseID > 0 
          AND ISNULL(je2.Debit, 0) > 0
    )
      AND (
        coa.AccountNo NOT LIKE '1%' 
        AND coa.AccountNo NOT LIKE '2%' 
        AND coa.AccountNo NOT LIKE '3%'
      )
      AND ISNULL(at.AccountTypes, '') NOT IN (
        'Asset', 'Assets', 'Current Asset', 'Current Assets', 'Fixed Asset', 'Fixed Assets', 'Other Asset', 'Other Assets', 'Other Current Asset', 'Other Current Assets', 'Non-Current Asset', 'Non-Current Assets',
        'Liability', 'Liabilities', 'Current Liability', 'Current Liabilities', 'Long Term Liability', 'Long Term Liabilities', 'Other Current Liability', 'Other Current Liabilities', 'Non-Current Liability', 'Non-Current Liabilities',
        'Equity', 'Equities', 'Owner Equity', 'Retained Earnings',
        'Bank', 'Cash', 'Cash and Cash Equivalents', 'Accounts Receivable', 'Accounts Payable', 'Credit Card', 'Payroll Liabilities'
      )
      AND ISNULL(cat.AccountCategory, '') NOT IN (
        'Asset', 'Assets', 'Current Asset', 'Current Assets', 'Fixed Asset', 'Fixed Assets', 'Other Asset', 'Other Assets', 'Other Current Asset', 'Other Current Assets',
        'Liability', 'Liabilities', 'Current Liability', 'Current Liabilities', 'Long Term Liability', 'Long Term Liabilities', 'Other Current Liability', 'Other Current Liabilities',
        'Equity', 'Owner Equity', 'Bank', 'Cash', 'Accounts Receivable', 'Accounts Payable'
      )
)
SELECT 
    AccountTypes,
    AccountCategory,
    ParentAccountNo,
    ParentAccountName,
    AccountNo,
    AccountName,
    CASE 
        WHEN @DisplayColumns = 'Week' THEN 'Week ' + CAST(DATEPART(WEEK, TxDate) AS NVARCHAR(10))
        WHEN @DisplayColumns = 'Month' THEN RIGHT('0' + CAST(MONTH(TxDate) AS NVARCHAR(2)), 2) + '/' + CAST(YEAR(TxDate) AS NVARCHAR(4))
        WHEN @DisplayColumns = 'Quarter' THEN 'Q' + CAST(DATEPART(QUARTER, TxDate) AS NVARCHAR(10)) + '/' + CAST(YEAR(TxDate) AS NVARCHAR(4))
        WHEN @DisplayColumns = 'Year' THEN CAST(YEAR(TxDate) AS NVARCHAR(4))
        WHEN @DisplayColumns = 'Location' THEN COALESCE(NULLIF(LocationName, ''), 'Unassigned')
        WHEN @DisplayColumns = 'Name' THEN COALESCE(NULLIF(PartyName, ''), 'Unassigned')
        WHEN @DisplayColumns = 'Class' THEN COALESCE(NULLIF(ClassName, ''), 'Unclassified')
        WHEN @DisplayColumns = 'CreatedBy' OR @DisplayColumns = 'Created By' THEN COALESCE(NULLIF(CreatedByUserID, ''), 'Unknown')
        ELSE 'Total'
    END AS ColumnGroup,
    SUM(Amount) AS Amount
FROM AllPLRows
WHERE (@SysID = 0 OR TxSysID = @SysID OR TxSysID = 0 OR TxSysID IS NULL)
  AND (@WhID = 0 OR TxWhID = @WhID OR TxWhID = 0 OR TxWhID IS NULL)
  AND (@DomID = '' OR TxDomID LIKE '%' + @DomID + '%' OR TxDomID IS NULL)
  AND (@FromDate IS NULL OR TxDate >= @FromDate)
  AND (@ToDate IS NULL OR TxDate < DATEADD(DAY, 1, @ToDate))
  AND (
      NOT EXISTS (SELECT 1 FROM @LocationList)
      OR LocationName IN (SELECT Val FROM @LocationList)
      OR LocationID IN (SELECT Val FROM @LocationList)
      OR CAST(TxWhID AS NVARCHAR(50)) IN (SELECT Val FROM @LocationList)
  )
  AND (
      NOT EXISTS (SELECT 1 FROM @AccountList)
      OR AccountName IN (SELECT Val FROM @AccountList)
      OR AccountNo IN (SELECT Val FROM @AccountList)
      OR EXISTS (SELECT 1 FROM @AccountList al WHERE AccountName LIKE '%' + al.Val + '%' OR AccountNo LIKE '%' + al.Val + '%')
  )
  AND (@Name IS NULL OR @Name = '' OR PartyName LIKE '%' + @Name + '%')
  AND (
      NOT EXISTS (SELECT 1 FROM @ClassList)
      OR (ClassID > 0 AND ClassID IN (SELECT ClassID FROM @ClassList WHERE ClassID IS NOT NULL))
      OR (ClassID > 0 AND CAST(ClassID AS NVARCHAR(50)) IN (SELECT Val FROM @ClassList))
      OR (ClassName <> '' AND ClassName IN (SELECT Val FROM @ClassList))
      OR (ClassName <> '' AND ClassName IN (SELECT ClassName FROM @ClassList WHERE ClassName IS NOT NULL AND ClassName <> ''))
  )
  AND (
      NOT EXISTS (SELECT 1 FROM @TxTypeList)
      OR TxType IN (SELECT Val FROM @TxTypeList)
      OR (TxType IN ('Payment', 'Receive Payment', 'Customer Payment', 'ReceivePayment', 'Receipt', 'Official Receipt') AND EXISTS (SELECT 1 FROM @TxTypeList WHERE Val = 'Receipt'))
      OR (TxType IN ('Journal', 'Journal Entry') AND EXISTS (SELECT 1 FROM @TxTypeList WHERE Val = 'Journal'))
      OR (TxType IN ('OpeningBalance', 'Opening Balance') AND EXISTS (SELECT 1 FROM @TxTypeList WHERE Val IN ('OpeningBalance', 'Opening Balance')))
  )
  AND (@RefNo IS NULL OR @RefNo = '' OR RefNo LIKE '%' + @RefNo + '%')
  AND (
      NOT EXISTS (SELECT 1 FROM @CreatedByList)
      OR CreatedByUserID IN (SELECT Val FROM @CreatedByList)
  )
GROUP BY 
    AccountTypes,
    AccountCategory,
    ParentAccountNo,
    ParentAccountName,
    AccountNo,
    AccountName,
    CASE 
        WHEN @DisplayColumns = 'Week' THEN 'Week ' + CAST(DATEPART(WEEK, TxDate) AS NVARCHAR(10))
        WHEN @DisplayColumns = 'Month' THEN RIGHT('0' + CAST(MONTH(TxDate) AS NVARCHAR(2)), 2) + '/' + CAST(YEAR(TxDate) AS NVARCHAR(4))
        WHEN @DisplayColumns = 'Quarter' THEN 'Q' + CAST(DATEPART(QUARTER, TxDate) AS NVARCHAR(10)) + '/' + CAST(YEAR(TxDate) AS NVARCHAR(4))
        WHEN @DisplayColumns = 'Year' THEN CAST(YEAR(TxDate) AS NVARCHAR(4))
        WHEN @DisplayColumns = 'Location' THEN COALESCE(NULLIF(LocationName, ''), 'Unassigned')
        WHEN @DisplayColumns = 'Name' THEN COALESCE(NULLIF(PartyName, ''), 'Unassigned')
        WHEN @DisplayColumns = 'Class' THEN COALESCE(NULLIF(ClassName, ''), 'Unclassified')
        WHEN @DisplayColumns = 'CreatedBy' OR @DisplayColumns = 'Created By' THEN COALESCE(NULLIF(CreatedByUserID, ''), 'Unknown')
        ELSE 'Total'
    END
HAVING SUM(Amount) <> 0
ORDER BY 
    CASE 
        WHEN ISNULL(AccountCategory, '') LIKE '%COGS%' OR ISNULL(AccountCategory, '') LIKE '%Cost%' THEN 3
        WHEN AccountTypes = 'Revenue' THEN 1 
        WHEN AccountTypes = 'Other Income' THEN 2 
        WHEN AccountTypes = 'Expense' THEN 5 
        ELSE 6 
    END, 
    ISNULL(ParentAccountNo, ''), 
    AccountNo;";

                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@SysID", sysId);
                        cmd.Parameters.AddWithValue("@WhID", whId);
                        cmd.Parameters.AddWithValue("@DomID", domId);
                        cmd.Parameters.AddWithValue("@FromDate", fromDate.HasValue ? (object)fromDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToDate", toDate.HasValue ? (object)toDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Location", string.IsNullOrWhiteSpace(location) ? (object)DBNull.Value : location.Trim());
                        cmd.Parameters.AddWithValue("@Account", string.IsNullOrWhiteSpace(account) ? (object)DBNull.Value : account.Trim());
                        cmd.Parameters.AddWithValue("@Name", string.IsNullOrWhiteSpace(name) ? (object)DBNull.Value : name.Trim());
                        cmd.Parameters.AddWithValue("@ClassName", string.IsNullOrWhiteSpace(className) ? (object)DBNull.Value : className.Trim());
                        cmd.Parameters.AddWithValue("@TxType", string.IsNullOrWhiteSpace(txType) ? (object)DBNull.Value : txType.Trim());
                        cmd.Parameters.AddWithValue("@RefNo", string.IsNullOrWhiteSpace(refNo) ? (object)DBNull.Value : refNo.Trim());
                        cmd.Parameters.AddWithValue("@CreatedBy", string.IsNullOrWhiteSpace(createdBy) ? (object)DBNull.Value : createdBy.Trim());
                        cmd.Parameters.AddWithValue("@DisplayColumns", string.IsNullOrWhiteSpace(displayColumns) ? "Total" : displayColumns.Trim());

                        con.Open();
                        using (System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                data.Add(new
                                {
                                    AccountTypes = dr["AccountTypes"].ToString(),
                                    AccountCategory = dr["AccountCategory"].ToString(),
                                    ParentAccountNo = dr["ParentAccountNo"].ToString(),
                                    ParentAccountName = dr["ParentAccountName"].ToString(),
                                    AccountNo = dr["AccountNo"].ToString(),
                                    AccountName = dr["AccountName"].ToString(),
                                    ColumnGroup = dr["ColumnGroup"].ToString(),
                                    Amount = Convert.ToDecimal(dr["Amount"])
                                });
                            }
                        }
                    }
                    // Query dynamic company names based on SystemID, WarehouseID/Location, and ClassName
                    string companyNameKh = "";
                    string companyNameEn = "";

                    string targetClass = !string.IsNullOrWhiteSpace(className) ? className.Trim() : "";
                    if (targetClass == "--- All Classes ---" || targetClass == "All") targetClass = "";

                    int targetWhId = whId;
                    if (targetWhId == 0 && !string.IsNullOrWhiteSpace(location) && location != "All" && location != "--- All Locations ---")
                    {
                        int parsedWh = 0;
                        if (int.TryParse(location.Split(',')[0], out parsedWh)) targetWhId = parsedWh;
                    }

                    Func<System.Data.SqlClient.SqlDataReader, string[], string> readCol = (r, names) =>
                    {
                        foreach (var targetName in names)
                        {
                            for (int i = 0; i < r.FieldCount; i++)
                            {
                                string colName = r.GetName(i);
                                if (string.Equals(colName, targetName, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (r[i] != DBNull.Value && !string.IsNullOrWhiteSpace(r[i].ToString()))
                                        return r[i].ToString().Trim();
                                }
                            }
                        }
                        return "";
                    };

                    // 1. If Class filter is selected, search dbo.TblClass FIRST (for KHName & FullName)
                    if (!string.IsNullOrEmpty(targetClass))
                    {
                        try
                        {
                            string sqlClassComp = @"
                                SELECT TOP 1 * 
                                FROM dbo.TblClass WITH (NOLOCK)
                                WHERE (
                                    ClassName = @ClassName 
                                    OR FullName = @ClassName 
                                    OR CAST(ClassID AS NVARCHAR(50)) = @ClassName
                                    OR @ClassName LIKE '%' + ClassName + '%'
                                    OR @ClassName LIKE '%' + FullName + '%'
                                    OR ClassName LIKE '%' + @ClassName + '%'
                                    OR FullName LIKE '%' + @ClassName + '%'
                                )
                                ORDER BY 
                                    CASE 
                                        WHEN NULLIF(KHName, '') IS NOT NULL AND NULLIF(FullName, '') IS NOT NULL THEN 1
                                        WHEN NULLIF(KHName, '') IS NOT NULL THEN 2
                                        WHEN NULLIF(FullName, '') IS NOT NULL THEN 3
                                        ELSE 4 
                                    END,
                                    CASE 
                                        WHEN FullName = @ClassName THEN 1
                                        WHEN ClassName = @ClassName THEN 2
                                        WHEN @SysID > 0 AND SystemID = @SysID THEN 3
                                        ELSE 4 
                                    END, ClassID ASC";
                            using (System.Data.SqlClient.SqlCommand cmdCls = new System.Data.SqlClient.SqlCommand(sqlClassComp, con))
                            {
                                cmdCls.Parameters.AddWithValue("@SysID", sysId);
                                cmdCls.Parameters.AddWithValue("@ClassName", targetClass);
                                using (System.Data.SqlClient.SqlDataReader drCls = cmdCls.ExecuteReader())
                                {
                                    if (drCls.Read())
                                    {
                                        // ONLY check Khmer fields for Khmer name
                                        companyNameKh = readCol(drCls, new string[] { "KHName", "CompanyNameKH", "CompanyNameKhmer", "CompanyNameKh" });
                                        companyNameEn = readCol(drCls, new string[] { "FullName", "ClassName" });

                                        // In database, KHName does not have ក្រុមហ៊ុន. Add ក្រុមហ៊ុន as prefix:
                                        if (!string.IsNullOrWhiteSpace(companyNameKh))
                                        {
                                            string clean = companyNameKh.Trim(' ', '\t', '\r', '\n', '\u200B', '\u00A0');
                                            if (!clean.StartsWith("ក្រុមហ៊ុន"))
                                            {
                                                companyNameKh = "ក្រុមហ៊ុន " + clean;
                                            }
                                            else
                                            {
                                                companyNameKh = clean;
                                            }
                                        }

                                    }
                                }
                            }
                        }
                        catch (Exception exCls) { System.Diagnostics.Trace.TraceWarning("PL Class lookup error: " + exCls.Message); }
                    }

                    // 2. Fallback to dbo.TblCompany only when no class is selected (or class has no names at all)
                    if (string.IsNullOrEmpty(targetClass) && (string.IsNullOrEmpty(companyNameKh) || string.IsNullOrEmpty(companyNameEn)))
                    {
                        try
                        {
                            string sqlComp = @"
                                SELECT TOP 1 * 
                                FROM dbo.TblCompany WITH (NOLOCK)
                                WHERE (@SysID = 0 OR SystemID = @SysID OR SystemID IS NULL)
                                ORDER BY CASE WHEN @WhID > 0 AND WarehouseID = @WhID THEN 1 ELSE 2 END, CompanyID ASC";

                            using (System.Data.SqlClient.SqlCommand cmdComp = new System.Data.SqlClient.SqlCommand(sqlComp, con))
                            {
                                cmdComp.Parameters.AddWithValue("@SysID", sysId);
                                cmdComp.Parameters.AddWithValue("@WhID", targetWhId);
                                using (System.Data.SqlClient.SqlDataReader drComp = cmdComp.ExecuteReader())
                                {
                                    if (drComp.Read())
                                    {
                                        string compKh = readCol(drComp, new string[] { "CompanyNameKH", "KHName", "CompanyNameKhmer", "CompanyNameKh" });
                                        string compEn = readCol(drComp, new string[] { "FullName", "CompanyName", "CompanyNameEN", "CompanyNameEn", "CompanyNameLatin" });
                                        if (string.IsNullOrEmpty(companyNameKh) && !string.IsNullOrEmpty(compKh))
                                        {
                                            string clean = compKh.Trim(' ', '\t', '\r', '\n', '\u200B', '\u00A0');
                                            if (!clean.StartsWith("ក្រុមហ៊ុន")) companyNameKh = "ក្រុមហ៊ុន " + clean;
                                            else companyNameKh = clean;
                                        }
                                        if (string.IsNullOrEmpty(companyNameEn) && !string.IsNullOrEmpty(compEn)) companyNameEn = compEn;
                                    }
                                }
                            }
                        }
                        catch (Exception exComp) { System.Diagnostics.Trace.TraceWarning("PL Company lookup error: " + exComp.Message); }
                    }

                    // 3. Fallback to Warehouse / System Name if English is still missing
                    if (string.IsNullOrEmpty(companyNameEn))
                    {
                        try
                        {
                            string sqlSys = @"
                                SELECT TOP 1 
                                    COALESCE(NULLIF(CAST(WarehouseName AS NVARCHAR(250)), ''), NULLIF(CAST(SystemID AS NVARCHAR(250)), '')) AS SysName
                                FROM dbo.TblWarehouse WITH (NOLOCK)
                                WHERE WarehouseID = @WhID OR SystemID = @SysID";
                            using (System.Data.SqlClient.SqlCommand cmdSys = new System.Data.SqlClient.SqlCommand(sqlSys, con))
                            {
                                cmdSys.Parameters.AddWithValue("@SysID", sysId);
                                cmdSys.Parameters.AddWithValue("@WhID", targetWhId);
                                using (System.Data.SqlClient.SqlDataReader drSys = cmdSys.ExecuteReader())
                                {
                                    if (drSys.Read())
                                    {
                                        companyNameEn = drSys["SysName"] != DBNull.Value ? drSys["SysName"].ToString() : "";
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    // Prepend "ក្រុមហ៊ុន" (Company) to Khmer company name if not present
                    if (!string.IsNullOrWhiteSpace(companyNameKh))
                    {
                        string clean = companyNameKh.Trim(' ', '\t', '\r', '\n', '\u200B', '\u00A0');
                        if (!clean.StartsWith("ក្រុមហ៊ុន"))
                        {
                            companyNameKh = "ក្រុមហ៊ុន " + clean;
                        }
                        else
                        {
                            companyNameKh = clean;
                        }
                    }

                    var companyObj = new { CompanyNameKh = companyNameKh, CompanyNameEn = companyNameEn };
                    return Json(new { Success = true, Data = data, Company = companyObj }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [HttpPost]
        public JsonResult GetAdvanceFilterLookups(int? SystemID = null, int? WarehouseID = null)
        {
            try
            {
                HTML html = new HTML();
                string connStr = html.GetConnectionString();

                int sysId = SystemID ?? 0;
                if (sysId == 0 && !string.IsNullOrEmpty(Request["SystemID"])) int.TryParse(Request["SystemID"], out sysId);
                if (sysId == 0 && !string.IsNullOrEmpty(html.SystemID)) int.TryParse(html.SystemID, out sysId);

                int whId = WarehouseID ?? 0;
                if (whId == 0 && !string.IsNullOrEmpty(Request["WarehouseID"])) int.TryParse(Request["WarehouseID"], out whId);
                if (whId == 0 && !string.IsNullOrEmpty(html.WarehouseID)) int.TryParse(html.WarehouseID, out whId);

                var accounts = new List<object>();
                var locations = new List<object>();
                var classes = new List<object>();
                var users = new List<object>();

                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    con.Open();

                    // 1. Accounts (Only P&L Accounts: Revenue, Cost, Expense)
                    try
                    {
                        string sqlAcc = @"
                            SELECT DISTINCT coa.AccountNo, coa.AccountName 
                            FROM dbo.TblChartOfAccount coa WITH (NOLOCK) 
                            LEFT JOIN dbo.TblAccountTypes at WITH (NOLOCK) ON at.AccountTypesID = coa.AccountTypesID
                            LEFT JOIN dbo.TblAccountCategories cat WITH (NOLOCK) ON cat.AccountCategoryID = coa.AccountCategoryID
                            WHERE (@SysID = 0 OR coa.SystemID = @SysID) 
                              AND ISNULL(coa.AccountName, '') <> '' 
                              AND coa.AccountNo NOT LIKE '1%' 
                              AND coa.AccountNo NOT LIKE '2%' 
                              AND coa.AccountNo NOT LIKE '3%'
                              AND ISNULL(at.AccountTypes, '') NOT IN ('Asset', 'Assets', 'Current Asset', 'Current Assets', 'Fixed Asset', 'Fixed Assets', 'Other Asset', 'Other Assets', 'Other Current Asset', 'Other Current Assets', 'Liability', 'Liabilities', 'Current Liability', 'Current Liabilities', 'Long Term Liability', 'Equity', 'Bank', 'Cash', 'Accounts Receivable', 'Accounts Payable')
                            ORDER BY coa.AccountNo";
                        using (var cmd = new System.Data.SqlClient.SqlCommand(sqlAcc, con))
                        {
                            cmd.Parameters.AddWithValue("@SysID", sysId);
                            using (var dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    accounts.Add(new { AccountNo = dr["AccountNo"].ToString(), AccountName = dr["AccountName"].ToString() });
                                }
                            }
                        }
                    }
                    catch (Exception exAcc) { System.Diagnostics.Trace.TraceWarning("PL Accounts lookup error: " + exAcc.Message); }

                    // 2. Locations / Warehouses
                    try
                    {
                        string sqlLoc = @"
                            SELECT DISTINCT uw.WID AS WarehouseID, uw.WarehouseName 
                            FROM dbo.TblUserWarehouse uw WITH (NOLOCK)
                            WHERE (@SysID = 0 OR uw.SystemID = @SysID) AND ISNULL(uw.WarehouseName, '') <> ''
                            ORDER BY uw.WarehouseName";
                        using (var cmd = new System.Data.SqlClient.SqlCommand(sqlLoc, con))
                        {
                            cmd.Parameters.AddWithValue("@SysID", sysId);
                            using (var dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    locations.Add(new { WarehouseID = dr["WarehouseID"].ToString(), WarehouseName = dr["WarehouseName"].ToString() });
                                }
                            }
                        }
                    }
                    catch
                    {
                        try
                        {
                            string sqlLocFallback = @"SELECT WarehouseID, WarehouseName FROM dbo.TblWarehouse WITH (NOLOCK) WHERE ISNULL(WarehouseName, '') <> '' ORDER BY WarehouseName";
                            using (var cmd = new System.Data.SqlClient.SqlCommand(sqlLocFallback, con))
                            using (var dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    locations.Add(new { WarehouseID = dr["WarehouseID"].ToString(), WarehouseName = dr["WarehouseName"].ToString() });
                                }
                            }
                        }
                        catch { }
                    }

                    // 3. Classes (By SystemID & WarehouseID)
                    try
                    {
                        string sqlCls = @"
                            SELECT ClassID, ClassName 
                            FROM dbo.TblClass WITH (NOLOCK)
                            WHERE (@SysID = 0 OR SystemID = @SysID) 
                              AND (@WhID = 0 OR WarehouseID = @WhID) 
                              AND ISNULL(ClassName, '') <> '' 
                            ORDER BY ClassName";
                        using (var cmd = new System.Data.SqlClient.SqlCommand(sqlCls, con))
                        {
                            cmd.Parameters.AddWithValue("@SysID", sysId);
                            cmd.Parameters.AddWithValue("@WhID", whId);
                            using (var dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    classes.Add(new { ClassID = dr["ClassID"].ToString(), ClassName = dr["ClassName"].ToString() });
                                }
                            }
                        }
                    }
                    catch (Exception exCls) { System.Diagnostics.Trace.TraceWarning("PL Classes lookup error: " + exCls.Message); }

                    // 4. Users
                    try
                    {
                        string sqlUsr = @"SELECT DISTINCT UserLoginID AS UserID, COALESCE(NULLIF(FullName, ''), UserName, UserLoginID) AS FullName FROM dbo.TblUserLogin WITH (NOLOCK) WHERE ISNULL(FullName, UserName) <> '' ORDER BY FullName";
                        using (var cmd = new System.Data.SqlClient.SqlCommand(sqlUsr, con))
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                users.Add(new { UserID = dr["UserID"].ToString(), FullName = dr["FullName"].ToString() });
                            }
                        }
                    }
                    catch (Exception exUsr) { System.Diagnostics.Trace.TraceWarning("PL Users lookup error: " + exUsr.Message); }
                }

                return Json(new { success = true, accounts = accounts, locations = locations, classes = classes, users = users }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [HttpPost]
        public JsonResult GetLookups()
        {
            return GetAdvanceFilterLookups();
        }
    }
}