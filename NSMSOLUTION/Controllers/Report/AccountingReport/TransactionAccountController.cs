using MVC;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;


namespace NSMSOLUTION.Controllers.Report.AccountingReport
{
    public class TransactionAccountController : Controller
    {
        [Authorize(Roles = "Report Transaction Account")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            int systemId = 0;
            int warehouseId = 0;
            int.TryParse(html.SystemID, out systemId);
            int.TryParse(html.WarehouseID, out warehouseId);

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
            var headerParameters = new Dictionary<string, string>
            {
                { "DefaultClassID", defaultClassId.ToString() },
                { "SystemID", systemId.ToString() },
                { "WarehouseID", warehouseId.ToString() }
            };
            try
            {
                var companyData = html.LoadData(companySql, false, headerParameters);
                if (companyData != null && companyData.Tables.Count > 0 && companyData.Tables[0].Rows.Count > 0)
                {
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
                    ViewBag.CompanyNameKh = companyKh;
                    ViewBag.CompanyNameEn = companyEn;
                }
                else
                {
                    System.Diagnostics.Trace.TraceWarning(
                        "Transaction Account report: company header was not found for SystemID {0}, WarehouseID {1}.",
                        systemId, warehouseId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Transaction Account report: failed to load company header: {0}", ex.Message);
            }

            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = true,
                DateRangePicker = true,
                LabelFromDate = "Date Range:",
                DateTimePickerWithRanges = true,
                DBField = "" // empty so it doesn't automatically append
            });

            // Fetch accounts for the dropdown
            string accountSql = $@"SELECT AccountNo, AccountNo + ' - ' + AccountName AS AccountName 
                                   FROM dbo.TblChartOfAccount 
                                   WHERE SystemID = {html.SystemID} 
                                   AND WarehouseID = {html.WarehouseID} 
                                   AND DomainID LIKE '%{html.DomainID}%' 
                                   ORDER BY AccountNo";
            System.Data.DataSet dsAccounts = html.LoadData(accountSql);
            ViewBag.Accounts = dsAccounts != null && dsAccounts.Tables.Count > 0 ? dsAccounts.Tables[0] : new System.Data.DataTable();

            html.HtmlTextBox.Add(new HtmlTextBox()
            {
                ID = "TxtAccount",
                Label = "Account No:",
                DBField = "" // empty so it doesn't automatically append
            });

            string sql = $@"
DECLARE @AccountNo NVARCHAR(50) = COALESCE(@TxtAccount, '');
DECLARE @FromDate DATETIME = CASE WHEN ISNULL(@TxtFromDate, '') = '' THEN DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0) ELSE CONVERT(datetime, SUBSTRING(@TxtFromDate, 1, 10), 103) END;
DECLARE @ToDate DATETIME = CASE WHEN ISNULL(@TxtFromDate, '') = '' THEN GETDATE() ELSE DATEADD(second, -1, DATEADD(day, 1, CONVERT(datetime, SUBSTRING(@TxtFromDate, 14, 10), 103))) END;

DECLARE @NormalSide NVARCHAR(10) = 'DR';
SELECT TOP 1 @NormalSide = at.NormalSide
FROM dbo.TblChartOfAccount coa
LEFT JOIN dbo.TblAccountCategories cat ON cat.AccountCategoryID = coa.AccountCategoryID
LEFT JOIN dbo.TblAccountTypes at ON at.AccountTypesID = cat.AccountTypesID
WHERE coa.AccountNo = @AccountNo;

DECLARE @OpeningBalance DECIMAL(18,2) = 0.00;
SELECT @OpeningBalance = ISNULL(
    SUM(
        CASE 
            WHEN @NormalSide = 'DR' THEN je.Debit - je.Credit
            ELSE je.Credit - je.Debit
        END
    ), 0.00)
FROM dbo.TblAccount_JournalEntries je
LEFT JOIN dbo.TblAccount_Transaction act ON act.TransactionID = je.TransactionID
INNER JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = je.AccountID
WHERE coa.AccountNo = @AccountNo
  AND CAST(COALESCE(act.TransactionDate, je.EntryDate, je.TransactionDate, act.CreateDate, je.CreateDate) AS DATE) < CAST(@FromDate AS DATE)
  AND ({html.SystemID} = 0 OR act.SystemID = {html.SystemID} OR je.SystemID = {html.SystemID} OR je.SystemID IS NULL)
  AND ({html.WarehouseID} = 0 OR act.WarehouseID = {html.WarehouseID} OR je.WarehouseID = {html.WarehouseID} OR je.WarehouseID IS NULL OR je.WarehouseID = 0);

SELECT 
    No,
    Type,
    TransactionDate AS Date,
    ReferenceNo,
    Name,
    AccountName,
    Debit,
    Credit,
    -- Running Balance Calculation
    SUM(NetChange) OVER (ORDER BY SortOrder, TransactionDate, EntryID ROWS UNBOUNDED PRECEDING) AS Balance,
    IsSubtotal
FROM (
    -- 1. Opening Balance Row
    SELECT 
        '' AS No,
        'Opening Balance' AS Type,
        NULL AS TransactionDate,
        '' AS ReferenceNo,
        '' AS Name,
        '' AS AccountName,
        NULL AS Debit,
        NULL AS Credit,
        @OpeningBalance AS NetChange,
        0 AS SortOrder,
        0 AS EntryID,
        1 AS IsSubtotal

    UNION ALL

    -- 2. Transaction Rows
    SELECT 
        FORMAT(ROW_NUMBER() OVER (ORDER BY act.TransactionDate ASC, je.EntryID ASC), '00') AS No,
        COALESCE(act.[Type], 'Journal') AS Type,
        act.TransactionDate,
        CASE 
            WHEN COALESCE(act.[Type], '') = 'Transfer' OR COALESCE(act.[Type], '') = 'Fund Transfer' THEN
                '<a href=""#!/FundTransfer?id=' + CONVERT(VARCHAR, ft.FundTransferID) + '"" target=""_blank"">' + ISNULL(ft.RefNo, act.InvoiceNo) + '</a>'
            WHEN (COALESCE(act.[Type], '') = 'Expense' OR ISNULL(act.IsPosted, 0) = 1) AND ISNULL(act.ExpenseID, 0) > 0 THEN
                '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher(' + CONVERT(VARCHAR, act.ExpenseID) + ', ''' + act.ExpenseNo + ''', ''Checked'')"">' + act.InvoiceNo + '</a>'
            ELSE
                '<a href=""javascript:void(0);"" onclick=""PrintInvoice(' + CONVERT(VARCHAR, je.CustomerID) + ', ''' + act.InvoiceNo + ''', 0, 10, '''', false, this);"">' + act.InvoiceNo + '</a>'
        END AS ReferenceNo,
        CASE 
            WHEN (COALESCE(act.[Type], '') = 'Expense' OR ISNULL(act.IsPosted, 0) = 1) AND act.VendorID IS NOT NULL THEN 
                '<a href=""#!/VendorCenter?id=' + CONVERT(VARCHAR, act.VendorID) + '"" target=""_blank"">' + COALESCE(NULLIF(c.CustomerName, ''), NULLIF(c.CustomerNameLatin, ''), v.VendorName) + '</a>'
            WHEN je.CustomerID IS NOT NULL THEN
                '<a href=""#!/CustomerProfile?ID=' + CONVERT(VARCHAR, je.CustomerID) + '"" target=""_blank"">' + COALESCE(NULLIF(c.CustomerName, ''), NULLIF(c.CustomerNameLatin, ''), v.VendorName) + '</a>'
            ELSE
                COALESCE(NULLIF(c.CustomerName, ''), NULLIF(c.CustomerNameLatin, ''), v.VendorName)
        END AS Name,
        CA.ContraAccount AS AccountName,
        je.Debit,
        je.Credit,
        CASE 
            WHEN @NormalSide = 'DR' THEN je.Debit - je.Credit
            ELSE je.Credit - je.Debit
        END AS NetChange,
        1 AS SortOrder,
        je.EntryID,
        0 AS IsSubtotal
    FROM dbo.TblAccount_JournalEntries je
    LEFT JOIN dbo.TblAccount_Transaction act ON act.TransactionID = je.TransactionID
    INNER JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = je.AccountID
    LEFT JOIN dbo.TblCustomer c ON c.CustomerID = je.CustomerID
    LEFT JOIN dbo.TblVendor v ON v.VendorID = act.VendorID
    LEFT JOIN dbo.TblFundTransfer ft ON 'FT-' + CAST(ft.FundTransferID AS VARCHAR) = act.InvoiceNo AND (act.[Type] = 'Transfer' OR act.[Type] = 'Fund Transfer')
    OUTER APPLY
    (
        SELECT TOP 1
            coa2.AccountName AS ContraAccount
        FROM dbo.TblAccount_JournalEntries je2
        INNER JOIN dbo.TblChartOfAccount coa2 ON coa2.ChartOfAccountID = je2.AccountID
        WHERE je2.TransactionID = je.TransactionID
          AND je2.AccountID <> je.AccountID
        ORDER BY je2.EntryID
    ) CA
    WHERE coa.AccountNo = @AccountNo
      AND CAST(COALESCE(act.TransactionDate, je.EntryDate, je.TransactionDate, act.CreateDate, je.CreateDate) AS DATE) >= CAST(@FromDate AS DATE)
      AND CAST(COALESCE(act.TransactionDate, je.EntryDate, je.TransactionDate, act.CreateDate, je.CreateDate) AS DATE) <= CAST(@ToDate AS DATE)
      AND ({html.SystemID} = 0 OR act.SystemID = {html.SystemID} OR je.SystemID = {html.SystemID} OR je.SystemID IS NULL)
      AND ({html.WarehouseID} = 0 OR act.WarehouseID = {html.WarehouseID} OR je.WarehouseID = {html.WarehouseID} OR je.WarehouseID IS NULL OR je.WarehouseID = 0)
) t
WHERE 1=1";

            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "SortOrder ASC, TransactionDate ASC, EntryID ASC";
            html.ReportTitle = "Report Transaction Account";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "center" },
                new GridField() { Header = "Type", FieldName = "Type", TextAlign = "center" },
                new GridField() { Header = "Date", FieldName = "Date", Format = "dd/MM/yyyy", ClassName="text-center" },
                new GridField() { Header = "Reference Nº", FieldName = "ReferenceNo", ClassName="text-center" },
                new GridField() { Header = "Name", FieldName = "Name" },
                new GridField() { Header = "Account Name", FieldName = "AccountName" },
                new GridField() { Header = "Debit", FieldName = "Debit", Format = "#,##0.00;-#,##0.00;-", TextAlign = "right", ClassName = "text-right" },
                new GridField() { Header = "Credit", FieldName = "Credit", Format = "#,##0.00;-#,##0.00;-", TextAlign = "right", ClassName = "text-right" },
                new GridField() { Header = "Balance", FieldName = "Balance", Format = "#,##0.00;-#,##0.00;-", TextAlign = "right", ClassName = "text-right" },
                new GridField() { Header = "IsSubtotal", FieldName = "IsSubtotal", IsHiddenField = true }
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View("~/Views/TransactionAccount/index.cshtml");
        }


        [HttpPost]
        [Authorize(Roles = "Report Transaction Account")]
        public JsonResult Read(DateTime? fromDate, DateTime? toDate, string account, int? SystemID, int? WarehouseID)
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

                string domId = html.DomainID ?? "";

                string normalSide = "DR";
                string accountName = "";
                decimal openingBalance = 0;
                var data = new List<object>();

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();

                    // 1. Get NormalSide and AccountName
                    string normalSideSql = @"
                        SELECT TOP 1 
                            UPPER(LTRIM(RTRIM(COALESCE(coa.NormalSide, at.NormalSide, 'DR')))) AS NormalSide, 
                            coa.AccountName
                        FROM dbo.TblChartOfAccount coa
                        LEFT JOIN dbo.TblAccountCategories cat ON cat.AccountCategoryID = coa.AccountCategoryID
                        LEFT JOIN dbo.TblAccountTypes at ON at.AccountTypesID = cat.AccountTypesID
                        WHERE coa.AccountNo = @AccountNo;";

                    using (SqlCommand cmd = new SqlCommand(normalSideSql, con))
                    {
                        cmd.Parameters.AddWithValue("@AccountNo", (object)account ?? "");
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                normalSide = dr["NormalSide"].ToString();
                                accountName = dr["AccountName"].ToString();
                            }
                        }
                    }

                    string opBalanceSql = @"
                        SELECT ISNULL(
                            SUM(
                                CASE 
                                    WHEN @NormalSide = 'DR' THEN je.Debit - je.Credit
                                    ELSE je.Credit - je.Debit
                                END
                            ), 0.00)
                        FROM dbo.TblAccount_JournalEntries je
                        LEFT JOIN dbo.TblAccount_Transaction act ON act.TransactionID = je.TransactionID
                        INNER JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = je.AccountID
                        WHERE coa.AccountNo = @AccountNo
                          AND CAST(COALESCE(act.TransactionDate, je.EntryDate, je.TransactionDate, act.CreateDate, je.CreateDate) AS DATE) < CAST(@FromDate AS DATE)
                          AND (@SystemID = 0 OR act.SystemID = @SystemID OR je.SystemID = @SystemID OR je.SystemID IS NULL)
                          AND (@WarehouseID = 0 OR act.WarehouseID = @WarehouseID OR je.WarehouseID = @WarehouseID OR je.WarehouseID IS NULL OR je.WarehouseID = 0);";

                    using (SqlCommand cmd = new SqlCommand(opBalanceSql, con))
                    {
                        cmd.Parameters.AddWithValue("@NormalSide", normalSide);
                        cmd.Parameters.AddWithValue("@AccountNo", (object)account ?? "");
                        cmd.Parameters.AddWithValue("@FromDate", fromDate.HasValue ? fromDate.Value : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SystemID", sysId);
                        cmd.Parameters.AddWithValue("@WarehouseID", whId);
                        cmd.Parameters.AddWithValue("@DomainID", domId);

                        var res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value)
                        {
                            openingBalance = Convert.ToDecimal(res);
                        }
                    }

                    string txSql = @"
                        SELECT 
                            CAST(COALESCE(act.TransactionDate, je.EntryDate, je.TransactionDate, act.CreateDate, je.CreateDate) AS DATE) AS TransactionDate,
                            COALESCE(act.[Type], 'Journal') AS Type,
                            CASE 
                                WHEN COALESCE(act.[Type], '') = 'Transfer' OR COALESCE(act.[Type], '') = 'Fund Transfer' THEN ISNULL(ft.RefNo, act.InvoiceNo)
                                ELSE COALESCE(act.InvoiceNo, act.ExpenseNo, je.InvoiceNo, '') 
                            END AS ReferenceNo,
                            ft.FundTransferID,
                            act.ExpenseID,
                            act.ExpenseNo,
                            je.CustomerID,
                            act.VendorID,
                            ISNULL(act.IsPosted, 0) AS IsPosted,
                            COALESCE(NULLIF(c.CustomerName, ''), NULLIF(c.CustomerNameLatin, ''), v.VendorName) AS Name,
                            CA.ContraAccount AS AccountName,
                            je.Debit,
                            je.Credit
                        FROM dbo.TblAccount_JournalEntries je
                        LEFT JOIN dbo.TblAccount_Transaction act ON act.TransactionID = je.TransactionID
                        INNER JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = je.AccountID
                        LEFT JOIN dbo.TblCustomer c ON c.CustomerID = je.CustomerID
                        LEFT JOIN dbo.TblVendor v ON v.VendorID = act.VendorID
                        LEFT JOIN dbo.TblFundTransfer ft ON 'FT-' + CAST(ft.FundTransferID AS VARCHAR) = act.InvoiceNo AND (act.[Type] = 'Transfer' OR act.[Type] = 'Fund Transfer')
                        OUTER APPLY
                        (
                            SELECT TOP 1
                                coa2.AccountName AS ContraAccount
                            FROM dbo.TblAccount_JournalEntries je2
                            INNER JOIN dbo.TblChartOfAccount coa2 ON coa2.ChartOfAccountID = je2.AccountID
                            WHERE je2.TransactionID = je.TransactionID
                              AND je2.AccountID <> je.AccountID
                            ORDER BY je2.EntryID
                        ) CA
                        WHERE coa.AccountNo = @AccountNo
                          AND (@FromDate IS NULL OR CAST(COALESCE(act.TransactionDate, je.EntryDate, je.TransactionDate, act.CreateDate, je.CreateDate) AS DATE) >= CAST(@FromDate AS DATE))
                          AND (@ToDate IS NULL OR CAST(COALESCE(act.TransactionDate, je.EntryDate, je.TransactionDate, act.CreateDate, je.CreateDate) AS DATE) <= CAST(@ToDate AS DATE))
                          AND (@SystemID = 0 OR act.SystemID = @SystemID OR je.SystemID = @SystemID OR je.SystemID IS NULL)
                          AND (@WarehouseID = 0 OR act.WarehouseID = @WarehouseID OR je.WarehouseID = @WarehouseID OR je.WarehouseID IS NULL OR je.WarehouseID = 0)
                        ORDER BY CAST(COALESCE(act.TransactionDate, je.EntryDate, je.TransactionDate, act.CreateDate, je.CreateDate) AS DATE) ASC, je.EntryID ASC;";

                    using (SqlCommand cmd = new SqlCommand(txSql, con))
                    {
                        cmd.Parameters.AddWithValue("@AccountNo", (object)account ?? "");
                        cmd.Parameters.AddWithValue("@FromDate", fromDate.HasValue ? fromDate.Value : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToDate", toDate.HasValue ? toDate.Value.Date.AddDays(1).AddSeconds(-1) : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SystemID", sysId);
                        cmd.Parameters.AddWithValue("@WarehouseID", whId);
                        cmd.Parameters.AddWithValue("@DomainID", domId);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                data.Add(new
                                {
                                    TransactionDate = dr["TransactionDate"] != DBNull.Value ? Convert.ToDateTime(dr["TransactionDate"]).ToString("yyyy-MM-ddTHH:mm:ss") : null,
                                    Type = dr["Type"].ToString(),
                                    ReferenceNo = dr["ReferenceNo"].ToString(),
                                    FundTransferID = dr["FundTransferID"] != DBNull.Value ? (int?)Convert.ToInt32(dr["FundTransferID"]) : null,
                                    ExpenseID = dr["ExpenseID"] != DBNull.Value ? (int?)Convert.ToInt32(dr["ExpenseID"]) : null,
                                    ExpenseNo = dr["ExpenseNo"].ToString(),
                                    CustomerID = dr["CustomerID"] != DBNull.Value ? (int?)Convert.ToInt32(dr["CustomerID"]) : null,
                                    VendorID = dr["VendorID"] != DBNull.Value ? (int?)Convert.ToInt32(dr["VendorID"]) : null,
                                    IsPosted = Convert.ToInt32(dr["IsPosted"]),
                                    Name = dr["Name"].ToString(),
                                    AccountName = dr["AccountName"].ToString(),
                                    Debit = dr["Debit"] != DBNull.Value ? Convert.ToDecimal(dr["Debit"]) : 0,
                                    Credit = dr["Credit"] != DBNull.Value ? Convert.ToDecimal(dr["Credit"]) : 0
                                });
                            }
                        }
                    }
                }

                var jsonResult = Json(new
                {
                    Success = true,
                    OpeningBalance = openingBalance,
                    NormalSide = normalSide,
                    AccountNo = account,
                    AccountName = accountName,
                    Data = data
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {
                var jsonResult = Json(new
                {
                    Success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
    }
}