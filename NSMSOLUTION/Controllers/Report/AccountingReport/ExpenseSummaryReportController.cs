using MVC;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Report.AccountingReport
{
    public class ExpenseSummaryReportController : Controller
    {
        [Authorize(Roles = "Report Expense Summary")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            SetCompanyHeader(html);
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = true,
                DateRangePicker = true,
                FromDateValue = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd"),
                LabelFromDate = "Date Range:",
                DateTimePickerWithRanges = true,
                DBField = "T.PaymentDate"
            });

            // Voucher-level Expense Summary (matches Stimulsoft Expense Summary layout)
            string sqlSummary = @"
                SELECT T.[No], T.NumberLink, T.Number, T.ExpenseID, T.ExpenseDate, T.ContactName, T.VendorID, T.ContactCode, T.VATTIN, T.Currency, T.Memo, T.AmountUSD, T.AmountFormatted, T.PaymentDate, T.ClassName, T.PaymentMethod, T.AccountCode, T.AccountName, T.Reference, T.TaxUSD, T.TaxRate
                FROM (
                    SELECT 
                        ROW_NUMBER() OVER(ORDER BY COALESCE(ep.PaymentDate, e.PaymentDate, ep.PaidOnDate, e.PaidOnDate, ep.CreatedDate, e.CreatedDate) ASC, e.ExpenseID ASC) AS [No],
                        '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher(' + CONVERT(NVARCHAR, e.ExpenseID) + ', ''' + ISNULL(e.[No], '') + ''', ''' + ISNULL(e.[Status], '') + ''', ''' + ISNULL(e.PaymentNo, '') + ''');"">' + ISNULL(e.ExpenseNo, '') + '</a>' AS [NumberLink],
                        ISNULL(e.ExpenseNo, '') AS [Number],
                        e.ExpenseID,
                        COALESCE(ep.PaymentDate, e.PaymentDate, ep.PaidOnDate, e.PaidOnDate, ep.CreatedDate, e.CreatedDate) AS [ExpenseDate],
                        COALESCE(ep.PaymentDate, e.PaymentDate, ep.PaidOnDate, e.PaidOnDate, ep.CreatedDate, e.CreatedDate) AS [PaymentDate],
                        ISNULL(v.VendorName, 'N/A') AS [ContactName],
                        v.VendorID,
                        ISNULL(v.VendorCode, '') AS [ContactCode],
                        ISNULL(v.VATTIN, '') AS [VATTIN],
                        'Dollar USD' AS [Currency],
                        ISNULL(COALESCE(NULLIF(ep.Description, ''), NULLIF(e.Description, ''), ''), '') AS [Memo],
                        COALESCE(NULLIF(e.TotalAmount, 0), (SELECT SUM(ISNULL(ed.ItemGrandTotal, 0)) FROM dbo.ExpenseDetails ed WHERE ed.ExpenseID = e.ExpenseID), 0) AS [AmountUSD],
                        '$ ' + FORMAT(COALESCE(NULLIF(e.TotalAmount, 0), (SELECT SUM(ISNULL(ed.ItemGrandTotal, 0)) FROM dbo.ExpenseDetails ed WHERE ed.ExpenseID = e.ExpenseID), 0), 'N2') AS [AmountFormatted],
                        ISNULL(cl.ClassName, 'N/A') AS [ClassName],
                        ISNULL(pm.PaymentMethod, '') AS [PaymentMethod],
                        ISNULL(expenseFields.AccountCode, '') AS [AccountCode],
                        ISNULL(expenseFields.AccountName, '') AS [AccountName],
                        ISNULL(e.Reference, '') AS [Reference],
                        ISNULL(expenseFields.TaxUSD, 0) AS [TaxUSD],
                        ISNULL(expenseFields.TaxRate, '') AS [TaxRate]
                    FROM dbo.Expenses e
                    LEFT JOIN dbo.TblVendor v ON e.PayeeID = v.VendorID
                    LEFT JOIN dbo.TblPaymentMethod pm ON e.PaymentMethodID = pm.PaymentMethodID
                    LEFT JOIN dbo.TblClass cl ON (cl.ClassID = TRY_CAST(e.ClassName AS INT) OR cl.ClassName = e.ClassName) AND cl.SystemID = e.SystemID AND cl.WarehouseID = e.WarehouseID
                    OUTER APPLY (
                        SELECT
                            STUFF((
                                SELECT DISTINCT ', ' + coa.AccountNo
                                FROM dbo.ExpenseDetails ed
                                INNER JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = ed.ExpenseCategory
                                WHERE ed.ExpenseID = e.ExpenseID
                                  AND ISNULL(coa.AccountNo, '') <> ''
                                FOR XML PATH(''), TYPE
                            ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS AccountCode,
                            STUFF((
                                SELECT DISTINCT ', ' + coa.AccountName
                                FROM dbo.ExpenseDetails ed
                                INNER JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = ed.ExpenseCategory
                                WHERE ed.ExpenseID = e.ExpenseID
                                  AND ISNULL(coa.AccountName, '') <> ''
                                FOR XML PATH(''), TYPE
                            ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS AccountName,
                            (SELECT SUM(COALESCE(NULLIF(ed.ItemVAT, 0),
                                CASE
                                    WHEN COALESCE(NULLIF(ed.ItemTAX, 0), eTax.HeaderTax, 0) > 0
                                        THEN ROUND(((ISNULL(ed.QTY, 1) * ISNULL(ed.UnitPrice, ed.ItemGrandTotal) - ISNULL(ed.DisUSD, 0)) * COALESCE(NULLIF(ed.ItemTAX, 0), eTax.HeaderTax, 0)) / 100.0, 2)
                                    WHEN ISNULL(eTax.VAT, 0) > 0
                                     AND COALESCE(NULLIF(eTax.SubTotal, 0), NULLIF(eTax.TotalAmount - eTax.VAT, 0), NULLIF(expenseTotal.LineSubtotal, 0)) > 0
                                        THEN ROUND((ed.ItemGrandTotal / COALESCE(NULLIF(eTax.SubTotal, 0), NULLIF(eTax.TotalAmount - eTax.VAT, 0), NULLIF(expenseTotal.LineSubtotal, 0))) * eTax.VAT, 2)
                                    ELSE ISNULL(eTax.VAT, 0)
                                END, 0))
                             FROM dbo.ExpenseDetails ed
                             INNER JOIN dbo.Expenses eTax ON eTax.ExpenseID = ed.ExpenseID
                             CROSS APPLY (
                                 SELECT SUM(ISNULL(edTotal.ItemGrandTotal, 0)) AS LineSubtotal
                                 FROM dbo.ExpenseDetails edTotal
                                 WHERE edTotal.ExpenseID = eTax.ExpenseID
                             ) expenseTotal
                             WHERE eTax.ExpenseID = e.ExpenseID) AS TaxUSD,
                            STUFF((
                                SELECT DISTINCT ', ' + CONVERT(NVARCHAR(32), CAST(COALESCE(NULLIF(ed.ItemTAX, 0), NULLIF(eTax.HeaderTax, 0),
                                    CASE WHEN ISNULL(eTax.VAT, 0) > 0 AND ISNULL(eTax.SubTotal, 0) > 0
                                        THEN ROUND((eTax.VAT * 100.0) / eTax.SubTotal, 2)
                                        ELSE 0
                                    END, 0) AS DECIMAL(18, 2))) + '%'
                                FROM dbo.ExpenseDetails ed
                                INNER JOIN dbo.Expenses eTax ON eTax.ExpenseID = ed.ExpenseID
                                WHERE eTax.ExpenseID = e.ExpenseID
                                  AND COALESCE(NULLIF(ed.ItemTAX, 0), NULLIF(eTax.HeaderTax, 0),
                                      CASE WHEN ISNULL(eTax.VAT, 0) > 0 AND ISNULL(eTax.SubTotal, 0) > 0
                                          THEN ROUND((eTax.VAT * 100.0) / eTax.SubTotal, 2)
                                          ELSE 0
                                      END, 0) > 0
                                FOR XML PATH(''), TYPE
                            ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS TaxRate
                    ) expenseFields
                    OUTER APPLY (
                        SELECT TOP 1 
                            ep.ExpenseNo,
                            ep.PaymentDate,
                            ep.PaidOnDate,
                            ep.CreatedDate,
                            ep.Description
                        FROM dbo.Expense_PaymentTransactions ep WITH (NOLOCK)
                        WHERE ep.ExpenseID = e.ExpenseID
                        ORDER BY ep.ExpensePaymentID DESC
                    ) ep
                    WHERE e.SystemID = " + html.SystemID + @"
                      AND e.WarehouseID = " + html.WarehouseID + @"
                      AND e.DomainID LIKE '%" + html.DomainID + @"%'
                ) AS T
                WHERE 1=1";

            GridField[] gridTable = new GridField[]
            {
                new GridField() { Header = "No",             FieldName = "No",               TextAlign = "center"},
                new GridField() { Header = "Number",         FieldName = "NumberLink",       TextAlign = "left" },
                new GridField() { Header = "NumberRaw",      FieldName = "Number",           IsHiddenField = true },
                new GridField() { Header = "ExpenseID",      FieldName = "ExpenseID",        IsHiddenField = true },
                new GridField() { Header = "Date",   FieldName = "ExpenseDate",      FormatDateTime = "dd-MM-yyyy", TextAlign = "left"},
                new GridField() { Header = "Contact Name",   FieldName = "ContactName",      TextAlign = "left", Link = "#!/VendorCenter?id=", LinkParameterField = "VendorID", NewPage = true },
                new GridField() { Header = "VendorID",       FieldName = "VendorID",         IsHiddenField = true },
                new GridField() { Header = "Currency",       FieldName = "Currency",         TextAlign = "center"},
                new GridField() { Header = "Memo",           FieldName = "Memo",             TextAlign = "left" },
                new GridField() { Header = "Amount $",       FieldName = "AmountFormatted",  TextAlign = "right", ClassName = "text-right" },
                new GridField() { Header = "Amount",         FieldName = "AmountUSD",        Format = "#,##0.00", TextAlign = "right", ClassName = "text-right", IsTotalFooter = true, TotalLabel = "" },
                new GridField() { Header = "Class",          FieldName = "ClassName",        IsHiddenField = true },
                new GridField() { Header = "Payment Method", FieldName = "PaymentMethod",    IsHiddenField = true },
                new GridField() { Header = "Contact Code",    FieldName = "ContactCode",      IsHiddenField = true },
                new GridField() { Header = "VAT TIN",         FieldName = "VATTIN",           IsHiddenField = true },
                new GridField() { Header = "Account Code",    FieldName = "AccountCode",      IsHiddenField = true },
                new GridField() { Header = "Account Name",    FieldName = "AccountName",      IsHiddenField = true },
                new GridField() { Header = "Reference",       FieldName = "Reference",        IsHiddenField = true },
                new GridField() { Header = "Tax ($)",         FieldName = "TaxUSD",           Format = "#,##0.00", TextAlign = "right", ClassName = "text-right", IsHiddenField = true },
                new GridField() { Header = "Tax",             FieldName = "TaxRate",          IsHiddenField = true },
            };

            html.GH.SQL = sqlSummary;
            html.GH.GridTable = gridTable;
            html.OrderBy = "T.ExpenseDate ASC, T.ExpenseID ASC";
            html.IsHaveWhereCondition = true;
            html.ShowPaging = false;
            html.GroupableKendoGrid = true;
            html.ReportTitle = "Expense Summary";

            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = false,
                ServerPaging = false,
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
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
                FROM dbo.TblClass
                WHERE (@DefaultClassID > 0 AND ClassID = @DefaultClassID)
                   OR (@DefaultClassID = 0 AND (@SystemID = 0 OR SystemID = @SystemID))
                ORDER BY
                    CASE WHEN @DefaultClassID > 0 AND ClassID = @DefaultClassID THEN 0 ELSE 1 END,
                    CASE WHEN StatusID = 40 THEN 0 ELSE 1 END,
                    CASE WHEN (@WarehouseID > 0 AND WarehouseID = @WarehouseID) THEN 0 ELSE 1 END,
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
                    System.Diagnostics.Trace.TraceWarning("Expense Summary report: company header was not found for SystemID {0}, WarehouseID {1}.", systemId, warehouseId);
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

                ViewBag.CompanyNameKh = companyKh;
                ViewBag.CompanyNameEn = companyEn;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Expense Summary report: failed to load company header: {0}", ex.Message);
            }
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public JsonResult GetCompanyHeader(string className, string classId)
        {
            string targetClass = !string.IsNullOrWhiteSpace(className) ? className.Trim() : "";
            if (string.IsNullOrWhiteSpace(targetClass) || targetClass == "--- All Classes ---" || targetClass == "All")
                return Json(new { success = false, message = "A class must be selected." }, JsonRequestBehavior.AllowGet);

            try
            {
                HTML html = new HTML();
                string connStr = html.GetConnectionString();
                int systemId = 0;
                if (!int.TryParse(Request["SystemID"], out systemId))
                    int.TryParse(html.SystemID, out systemId);

                const string sql = @"
                    SELECT TOP 1 ClassID, ClassName, FullName, KHName
                    FROM dbo.TblClass WITH (NOLOCK)
                    WHERE
                        (@ClassID <> '' AND TRY_CONVERT(INT, @ClassID) IS NOT NULL
                            AND ClassID = TRY_CONVERT(INT, @ClassID))
                        OR
                        ((@ClassID = '' OR TRY_CONVERT(INT, @ClassID) IS NULL)
                            AND (ClassName = @ClassName OR FullName = @ClassName))
                    ORDER BY
                        CASE WHEN @ClassID <> '' AND TRY_CONVERT(INT, @ClassID) IS NOT NULL
                                  AND ClassID = TRY_CONVERT(INT, @ClassID) THEN 0 ELSE 1 END,
                        CASE WHEN ClassName = @ClassName THEN 0 ELSE 1 END,
                        CASE WHEN @SystemID > 0 AND SystemID = @SystemID THEN 0 ELSE 1 END,
                        ClassID ASC";

                using (SqlConnection connection = new SqlConnection(connStr))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ClassID", classId ?? "");
                    command.Parameters.AddWithValue("@ClassName", targetClass);
                    command.Parameters.AddWithValue("@SystemID", systemId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return Json(new { success = false, message = "The selected class could not be found." }, JsonRequestBehavior.AllowGet);

                        string companyNameKh = reader["KHName"] == DBNull.Value ? "" : reader["KHName"].ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(companyNameKh) && !companyNameKh.StartsWith("ក្រុមហ៊ុន"))
                            companyNameKh = "ក្រុមហ៊ុន " + companyNameKh;

                        return Json(new
                        {
                            success = true,
                            companyNameEn = reader["FullName"] == DBNull.Value ? "" : reader["FullName"].ToString().Trim(),
                            companyNameKh = companyNameKh,
                            classId = reader["ClassID"].ToString()
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Expense Summary GetCompanyHeader error: {0}", ex.Message);
                return Json(new { success = false, message = "Could not load the selected class company header." }, JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public JsonResult GetFilterOptions()
        {
            var vendors = new List<object>();
            var accounts = new List<object>();
            var classes = new List<object>();
            var paymentMethods = new List<object>();
            var locations = new List<object>();
            var users = new List<object>();

            try
            {
                HTML html = new HTML();
                string connStr = html.GetConnectionString();
                int sysId = 0;
                if (!string.IsNullOrEmpty(Request["SystemID"])) int.TryParse(Request["SystemID"], out sysId);
                else if (!string.IsNullOrEmpty(html.SystemID)) int.TryParse(html.SystemID, out sysId);

                int whId = 0;
                if (!string.IsNullOrEmpty(Request["WarehouseID"])) int.TryParse(Request["WarehouseID"], out whId);
                else if (!string.IsNullOrEmpty(html.WarehouseID)) int.TryParse(html.WarehouseID, out whId);

                string domId = html.DomainID ?? "";

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();

                    // 1. Vendors
                    try
                    {
                        string sqlVendors = @"
                            SELECT DISTINCT VendorID, VendorName 
                            FROM (
                                SELECT v.VendorID, v.VendorName 
                                FROM dbo.TblVendor v WITH (NOLOCK)
                                WHERE (@SysID = 0 OR v.SystemID = @SysID)
                                  AND (@WhID = 0 OR v.WarehouseID = @WhID)

                                UNION

                                SELECT DISTINCT v.VendorID, v.VendorName
                                FROM dbo.Expenses e WITH (NOLOCK)
                                INNER JOIN dbo.TblVendor v WITH (NOLOCK) ON e.PayeeID = v.VendorID
                                WHERE (@SysID = 0 OR e.SystemID = @SysID)
                                  AND (@WhID = 0 OR e.WarehouseID = @WhID)
                                  AND (@DomID = '' OR e.DomainID LIKE '%' + @DomID + '%')
                            ) AS T
                            WHERE ISNULL(VendorName, '') <> ''
                            ORDER BY VendorName";
                        using (SqlCommand cmd = new SqlCommand(sqlVendors, con))
                        {
                            cmd.Parameters.AddWithValue("@SysID", sysId);
                            cmd.Parameters.AddWithValue("@WhID", whId);
                            cmd.Parameters.AddWithValue("@DomID", domId);
                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    int vId = (dr["VendorID"] != DBNull.Value) ? Convert.ToInt32(dr["VendorID"]) : 0;
                                    string vName = (dr["VendorName"] != DBNull.Value) ? dr["VendorName"].ToString() : "";
                                    if (!string.IsNullOrWhiteSpace(vName))
                                    {
                                        vendors.Add(new
                                        {
                                            VendorID = vId,
                                            VendorName = vName
                                        });
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("GetFilterOptions vendors: {0}", ex.Message); }

                    // 2. Accounts
                    try
                    {
                        string sqlAccounts = @"
                            SELECT ChartOfAccountID, AccountNo, AccountName 
                            FROM dbo.TblChartOfAccount WITH (NOLOCK)
                            WHERE (@SysID = 0 OR SystemID = @SysID) 
                            AND (@WhID = 0 OR WarehouseID = @WhID) 
                            AND (@DomID = '' OR DomainID LIKE '%' + @DomID + '%' OR DomainID IS NULL)
                            ORDER BY AccountNo, AccountName";
                        using (SqlCommand cmd = new SqlCommand(sqlAccounts, con))
                        {
                            cmd.Parameters.AddWithValue("@SysID", sysId);
                            cmd.Parameters.AddWithValue("@WhID", whId);
                            cmd.Parameters.AddWithValue("@DomID", domId);
                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    int accId = (dr["ChartOfAccountID"] != DBNull.Value) ? Convert.ToInt32(dr["ChartOfAccountID"]) : 0;
                                    string accNo = (dr["AccountNo"] != DBNull.Value) ? dr["AccountNo"].ToString() : "";
                                    string accName = (dr["AccountName"] != DBNull.Value) ? dr["AccountName"].ToString() : "";
                                    accounts.Add(new
                                    {
                                        ChartOfAccountID = accId,
                                        AccountNo = accNo,
                                        AccountName = accName
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("GetFilterOptions accounts: {0}", ex.Message); }

                    // 3. Classes
                    try
                    {
                        string sqlClasses = @"
                            SELECT DISTINCT ClassID, ClassName 
                            FROM dbo.TblClass WITH (NOLOCK)
                            WHERE (@SysID = 0 OR SystemID = @SysID)
                              AND (@WhID = 0 OR WarehouseID = @WhID)
                              AND (@DomID = '' OR DomainID LIKE '%' + @DomID + '%' OR DomainID IS NULL)
                              AND ISNULL(ClassName, '') <> ''
                            ORDER BY ClassName";
                        using (SqlCommand cmd = new SqlCommand(sqlClasses, con))
                        {
                            cmd.Parameters.AddWithValue("@SysID", sysId);
                            cmd.Parameters.AddWithValue("@WhID", whId);
                            cmd.Parameters.AddWithValue("@DomID", domId);
                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    int clsId = (dr["ClassID"] != DBNull.Value) ? Convert.ToInt32(dr["ClassID"]) : 0;
                                    string clsName = (dr["ClassName"] != DBNull.Value) ? dr["ClassName"].ToString() : "";
                                    classes.Add(new
                                    {
                                        ClassID = clsId,
                                        ClassName = clsName
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("GetFilterOptions classes: {0}", ex.Message); }

                    // 4. Payment Methods
                    try
                    {
                        string sqlPM = @"
                            SELECT DISTINCT PaymentMethodID, PaymentMethod 
                            FROM (
                                SELECT PaymentMethodID, PaymentMethod 
                                FROM dbo.TblPaymentMethod WITH (NOLOCK)
                                WHERE (@SysID = 0 OR SystemID = @SysID)
                                  AND (@WhID = 0 OR WarehouseID = @WhID)

                                UNION

                                SELECT DISTINCT pm.PaymentMethodID, pm.PaymentMethod
                                FROM dbo.Expenses e WITH (NOLOCK)
                                INNER JOIN dbo.TblPaymentMethod pm WITH (NOLOCK) ON e.PaymentMethodID = pm.PaymentMethodID
                                WHERE (@SysID = 0 OR e.SystemID = @SysID)
                                  AND (@WhID = 0 OR e.WarehouseID = @WhID)
                                  AND (@DomID = '' OR e.DomainID LIKE '%' + @DomID + '%')
                            ) AS T
                            WHERE ISNULL(PaymentMethod, '') <> ''
                            ORDER BY PaymentMethod";
                        using (SqlCommand cmd = new SqlCommand(sqlPM, con))
                        {
                            cmd.Parameters.AddWithValue("@SysID", sysId);
                            cmd.Parameters.AddWithValue("@WhID", whId);
                            cmd.Parameters.AddWithValue("@DomID", domId);
                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    int pmId = (dr["PaymentMethodID"] != DBNull.Value) ? Convert.ToInt32(dr["PaymentMethodID"]) : 0;
                                    string pmName = (dr["PaymentMethod"] != DBNull.Value) ? dr["PaymentMethod"].ToString() : "";
                                    if (!string.IsNullOrWhiteSpace(pmName))
                                    {
                                        paymentMethods.Add(new
                                        {
                                            PaymentMethodID = pmId,
                                            PaymentMethod = pmName
                                        });
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("GetFilterOptions paymentMethods: {0}", ex.Message); }

                    // 5. Locations / Warehouses
                    try
                    {
                        string sqlLoc = @"SELECT WarehouseID, WarehouseName FROM dbo.TblWarehouse WITH (NOLOCK) WHERE ISNULL(WarehouseName, '') <> '' ORDER BY WarehouseName";
                        using (SqlCommand cmd = new SqlCommand(sqlLoc, con))
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                locations.Add(new { WarehouseID = dr["WarehouseID"].ToString(), WarehouseName = dr["WarehouseName"].ToString() });
                            }
                        }
                    }
                    catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("GetFilterOptions locations: {0}", ex.Message); }

                    // 6. Users
                    try
                    {
                        string sqlUsr = @"SELECT DISTINCT UserLoginID AS UserID, COALESCE(NULLIF(FullName, ''), UserName, UserLoginID) AS FullName FROM dbo.TblUserLogin WITH (NOLOCK) WHERE ISNULL(FullName, UserName) <> '' ORDER BY FullName";
                        using (SqlCommand cmd = new SqlCommand(sqlUsr, con))
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                users.Add(new { UserID = dr["UserID"].ToString(), FullName = dr["FullName"].ToString() });
                            }
                        }
                    }
                    catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("GetFilterOptions users: {0}", ex.Message); }
                }

                return Json(new { success = true, vendors = vendors, accounts = accounts, classes = classes, paymentMethods = paymentMethods, locations = locations, warehouses = locations, users = users }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, vendors = vendors, accounts = accounts, classes = classes, paymentMethods = paymentMethods, locations = locations, warehouses = locations, users = users }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
