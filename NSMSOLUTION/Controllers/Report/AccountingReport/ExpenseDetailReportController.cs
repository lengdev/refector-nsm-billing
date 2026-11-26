using MVC;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Report.AccountingReport
{
    public class ExpenseDetailReportController : Controller
    {
        [Authorize(Roles = "Report Expense Detail")]
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
                DBField = "COALESCE(ep.PaymentDate, e.PaymentDate, ep.PaidOnDate, e.PaidOnDate, ep.CreatedDate, e.CreatedDate)"
            });

            // Get the groupId parameter from the query string
            string groupId = Request["groupId"] ?? "";
            string additionalWhere = "";
            string reportSubtitle = "";

            // Parse the groupId to determine the filter type
            if (!string.IsNullOrEmpty(groupId))
            {
                if (groupId.StartsWith("V"))
                {
                    // Vendor filter: V123 -> VendorID = 123
                    string vendorId = groupId.Substring(1);
                    additionalWhere = " AND e.PayeeID = " + vendorId;
                    reportSubtitle = " (Filtered by Vendor)";
                }
                else if (groupId.StartsWith("A"))
                {
                    // Account filter: A456 -> ChartOfAccountID = 456
                    string accountId = groupId.Substring(1);
                    additionalWhere = " AND (itemSummary.ExpenseCategory = " + accountId + " OR e.PaymentAccountID = " + accountId + ")";
                    reportSubtitle = " (Filtered by Account)";
                }
                else if (groupId.StartsWith("M"))
                {
                    // Month filter: M2026-07 -> PaymentDate month = 2026-07
                    string month = groupId.Substring(1);
                    additionalWhere = " AND FORMAT(COALESCE(ep.PaymentDate, e.PaymentDate, ep.PaidOnDate, e.PaidOnDate, ep.CreatedDate, e.CreatedDate), 'yyyy-MM') = '" + month + "'";
                    reportSubtitle = " (Filtered by Month: " + month + ")";
                }
            }

            html.GH.SQL = @"
                SELECT 
                    ROW_NUMBER() OVER(ORDER BY COALESCE(ep.PaymentDate, e.PaymentDate, ep.PaidOnDate, e.PaidOnDate, ep.CreatedDate, e.CreatedDate) ASC, e.ExpenseID ASC, itemSummary.ExpenseCategory ASC) AS [No],
                    COALESCE(ep.PaymentDate, e.PaymentDate, ep.PaidOnDate, e.PaidOnDate, ep.CreatedDate, e.CreatedDate) AS PaymentDate,
                    '<a href=' + CHAR(34) + 'javascript:void(0);' + CHAR(34) + ' onclick=' + CHAR(34) + 'showPaymentVoucher(' + CONVERT(NVARCHAR, e.ExpenseID) + ',' + CHAR(39) + ISNULL(e.[No], '') + CHAR(39) + ',' + CHAR(39) + ISNULL(e.[Status], '') + CHAR(39) + ',' + CHAR(39) + ISNULL(e.PaymentNo, '') + CHAR(39) + ');' + CHAR(34) + '>' + ISNULL(COALESCE(NULLIF(e.ExpenseNo, ''), NULLIF(ep.ExpenseNo, ''), e.[No]), '') + '</a>' AS ExpenseIDLink,
                    ISNULL(COALESCE(NULLIF(e.ExpenseNo, ''), NULLIF(ep.ExpenseNo, ''), e.[No]), '') AS ExpenseNo,
                    e.ExpenseID,
                    itemSummary.Account,
                    itemSummary.AccountName,
                    itemSummary.SplitAccountCode,
                    itemSummary.SplitAccountName,
                    itemSummary.AccountCode,
                    payAcc.AccountName AS PaymentAccount,
                    itemTextSummary.ItemName,
                    CASE
                        WHEN COALESCE(NULLIF(ep.Description, ''), NULLIF(e.Description, '')) IS NOT NULL
                         AND NULLIF(itemTextSummary.ItemDescriptions, '') IS NOT NULL
                         AND COALESCE(NULLIF(ep.Description, ''), NULLIF(e.Description, '')) <> itemTextSummary.ItemDescriptions
                            THEN COALESCE(NULLIF(ep.Description, ''), NULLIF(e.Description, '')) + ' - ' + itemTextSummary.ItemDescriptions
                        WHEN COALESCE(NULLIF(ep.Description, ''), NULLIF(e.Description, '')) IS NOT NULL
                            THEN COALESCE(NULLIF(ep.Description, ''), NULLIF(e.Description, ''))
                        ELSE COALESCE(itemTextSummary.ItemDescriptions, '')
                    END AS Memo,
                    itemSummary.QTY,
                    itemSummary.UnitPrice,
                    itemSummary.Discount,
                    itemSummary.Tax,
                    itemSummary.VAT,
                    itemSummary.AmountUSD,
                    e.PayeeID AS VendorID,
                    v.VendorName AS ContactName,
                    pm.PaymentMethod AS PaymentMethod,
                    cl.ClassName AS ClassName,
                    cl.ClassID AS ClassID,
                    e.ApprovedBy,
                    e.Reference AS ReceiptNumber,
                    'USD' AS Currency,
                    itemTextSummary.ItemNames AS Notes
                FROM dbo.Expenses e
                LEFT JOIN (
                    SELECT
                        detailRows.ExpenseID,
                        detailRows.ExpenseCategory,
                        detailRows.Account,
                        detailRows.AccountName,
                        detailRows.SplitAccountCode,
                        detailRows.SplitAccountName,
                        detailRows.AccountCode,
                        SUM(detailRows.QTY) AS QTY,
                        CASE WHEN COUNT(DISTINCT detailRows.UnitPrice) <= 1 THEN MAX(detailRows.UnitPrice) ELSE NULL END AS UnitPrice,
                        SUM(detailRows.Discount) AS Discount,
                        CASE WHEN COUNT(DISTINCT detailRows.Tax) <= 1 THEN MAX(detailRows.Tax) ELSE NULL END AS Tax,
                        SUM(detailRows.RowVAT) AS VAT,
                        SUM(detailRows.ItemGrandTotal) + SUM(detailRows.RowVAT) AS AmountUSD
                    FROM (
                        SELECT
                            ed.ExpenseID,
                            ed.ExpenseCategory,
                            CASE
                                WHEN NULLIF(coaDetail.AccountNo, '') IS NOT NULL
                                    THEN coaDetail.AccountNo + '-' + COALESCE(coaDetail.AccountName, '')
                                ELSE COALESCE(coaDetail.AccountName, '')
                            END AS Account,
                            COALESCE(coaDetail.AccountName, '') AS AccountName,
                            COALESCE(parentCoa.AccountNo, '') AS SplitAccountCode,
                            COALESCE(parentCoa.AccountName, '') AS SplitAccountName,
                            COALESCE(coaDetail.AccountNo, '') AS AccountCode,
                            ISNULL(ed.QTY, 0) AS QTY,
                            ed.UnitPrice,
                            ISNULL(ed.DisUSD, 0) AS Discount,
                            COALESCE(NULLIF(ed.ItemTAX, 0), NULLIF(eCalc.HeaderTax, 0),
                                CASE WHEN ISNULL(eCalc.VAT, 0) > 0 AND ISNULL(eCalc.SubTotal, 0) > 0
                                    THEN ROUND((eCalc.VAT * 100.0) / eCalc.SubTotal, 2)
                                    ELSE 0
                                END, 0) AS Tax,
                            ISNULL(ed.ItemGrandTotal, 0) AS ItemGrandTotal,
                            COALESCE(NULLIF(ed.ItemVAT, 0),
                                CASE
                                    WHEN COALESCE(NULLIF(ed.ItemTAX, 0), eCalc.HeaderTax, 0) > 0
                                        THEN ROUND(((ISNULL(ed.QTY, 1) * ISNULL(ed.UnitPrice, ed.ItemGrandTotal) - ISNULL(ed.DisUSD, 0)) * COALESCE(NULLIF(ed.ItemTAX, 0), eCalc.HeaderTax, 0)) / 100.0, 2)
                                    WHEN ISNULL(eCalc.VAT, 0) > 0
                                     AND COALESCE(NULLIF(eCalc.SubTotal, 0), NULLIF(eCalc.TotalAmount - eCalc.VAT, 0), NULLIF(expenseTotal.LineSubtotal, 0)) > 0
                                        THEN ROUND((ed.ItemGrandTotal / COALESCE(NULLIF(eCalc.SubTotal, 0), NULLIF(eCalc.TotalAmount - eCalc.VAT, 0), NULLIF(expenseTotal.LineSubtotal, 0))) * eCalc.VAT, 2)
                                    ELSE ISNULL(eCalc.VAT, 0)
                                END, 0) AS RowVAT
                        FROM dbo.ExpenseDetails ed
                        INNER JOIN dbo.Expenses eCalc ON eCalc.ExpenseID = ed.ExpenseID
                        LEFT JOIN dbo.TblChartOfAccount coaDetail ON coaDetail.ChartOfAccountID = ed.ExpenseCategory
                        LEFT JOIN dbo.TblChartOfAccount parentCoa ON parentCoa.ChartOfAccountID = coaDetail.ParentAccountID
                        CROSS APPLY (
                            SELECT SUM(ISNULL(edTotal.ItemGrandTotal, 0)) AS LineSubtotal
                            FROM dbo.ExpenseDetails edTotal
                            WHERE edTotal.ExpenseID = ed.ExpenseID
                        ) expenseTotal
                    ) detailRows
                    GROUP BY
                        detailRows.ExpenseID,
                        detailRows.ExpenseCategory,
                        detailRows.Account,
                        detailRows.AccountName,
                        detailRows.SplitAccountCode,
                        detailRows.SplitAccountName,
                        detailRows.AccountCode
                ) itemSummary ON itemSummary.ExpenseID = e.ExpenseID
                CROSS APPLY (
                    SELECT
                        STUFF((
                            SELECT ' | ' + CASE
                                WHEN NULLIF(edText.ItemName, '') IS NOT NULL
                                 AND NULLIF(edText.ItemName_Description, '') IS NOT NULL
                                 AND edText.ItemName <> edText.ItemName_Description
                                    THEN edText.ItemName + ' - ' + edText.ItemName_Description
                                ELSE COALESCE(NULLIF(edText.ItemName_Description, ''), edText.ItemName, '')
                            END
                            FROM dbo.ExpenseDetails edText
                            WHERE edText.ExpenseID = e.ExpenseID
                              AND (edText.ExpenseCategory = itemSummary.ExpenseCategory OR (edText.ExpenseCategory IS NULL AND itemSummary.ExpenseCategory IS NULL))
                            FOR XML PATH(''), TYPE
                        ).value('.', 'NVARCHAR(MAX)'), 1, 3, '') AS ItemDescriptions,
                        STUFF((
                            SELECT ' | ' + COALESCE(edName.ItemName_Description, edName.ItemName, '')
                            FROM dbo.ExpenseDetails edName
                            WHERE edName.ExpenseID = e.ExpenseID
                              AND (edName.ExpenseCategory = itemSummary.ExpenseCategory OR (edName.ExpenseCategory IS NULL AND itemSummary.ExpenseCategory IS NULL))
                            FOR XML PATH(''), TYPE
                        ).value('.', 'NVARCHAR(MAX)'), 1, 3, '') AS ItemName,
                        STUFF((
                            SELECT ' | ' + COALESCE(edNote.ItemName, '')
                            FROM dbo.ExpenseDetails edNote
                            WHERE edNote.ExpenseID = e.ExpenseID
                              AND (edNote.ExpenseCategory = itemSummary.ExpenseCategory OR (edNote.ExpenseCategory IS NULL AND itemSummary.ExpenseCategory IS NULL))
                            FOR XML PATH(''), TYPE
                        ).value('.', 'NVARCHAR(MAX)'), 1, 3, '') AS ItemNames
                ) itemTextSummary
                LEFT JOIN dbo.TblChartOfAccount payAcc ON payAcc.ChartOfAccountID = e.PaymentAccountID
                LEFT JOIN dbo.TblVendor v ON e.PayeeID = v.VendorID
                LEFT JOIN dbo.TblPaymentMethod pm ON e.PaymentMethodID = pm.PaymentMethodID
                LEFT JOIN dbo.TblClass cl ON (cl.ClassID = TRY_CAST(e.ClassName AS INT) OR cl.ClassName = e.ClassName)
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
                  AND EXISTS (SELECT 1 FROM dbo.ExpenseDetails edExists WHERE edExists.ExpenseID = e.ExpenseID)
                  " + additionalWhere + @"
            ";

            html.IsHaveWhereCondition = true;
            html.ShowPaging = false;
            html.GroupableKendoGrid = true;
            html.OrderBy = "COALESCE(ep.PaymentDate, e.PaymentDate, ep.PaidOnDate, e.PaidOnDate, ep.CreatedDate, e.CreatedDate) ASC, e.ExpenseID ASC, itemSummary.ExpenseCategory ASC";
            html.ReportTitle = "Expense Detail";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", Width = "60px", TextAlign = "center" },
                new GridField() { Header = "Expense No.", FieldName = "ExpenseIDLink", Width = "160px", TextAlign = "left" },
                new GridField() { Header = "Date", FieldName = "PaymentDate", Width = "120px", FormatDateTime = "dd-MM-yyyy", TextAlign = "left" },
                new GridField() { Header = "Contact Name", FieldName = "ContactName", Width = "250px", TextAlign = "left", Link = "#!/VendorCenter?id=", LinkParameterField = "VendorID", NewPage = true },
                new GridField() { Header = "Account", FieldName = "Account", Width = "230px", TextAlign = "left" },
                new GridField() { Header = "Account Name", FieldName = "AccountName", Width = "200px", TextAlign = "left", IsHiddenField = true },
                new GridField() { Header = "Split Account Code", FieldName = "SplitAccountCode", Width = "150px", TextAlign = "left", IsHiddenField = true },
                new GridField() { Header = "Split Account Name", FieldName = "SplitAccountName", Width = "200px", TextAlign = "left", IsHiddenField = true },
                new GridField() { Header = "Account Code", FieldName = "AccountCode", Width = "150px", TextAlign = "left", IsHiddenField = true },
                new GridField() { Header = "Amount", FieldName = "AmountUSD", Width = "130px", Format = "c", TextAlign = "right", ClassName = "text-right", IsTotalFooter = true, TotalLabel = "Grand Total" },
                new GridField() { Header = "Memo", FieldName = "Memo", Width = "380px", TextAlign = "left", ClassName = "truncate-cell" },
                new GridField() { Header = "Payment Account", FieldName = "PaymentAccount", Width = "200px", TextAlign = "left", IsHiddenField = true },
                new GridField() { Header = "Item Name", FieldName = "ItemName", Width = "200px", TextAlign = "left", IsHiddenField = true },
                new GridField() { Header = "Quantity", FieldName = "QTY", Width = "100px", Format = "#,##0.###", TextAlign = "right", IsHiddenField = true },
                new GridField() { Header = "Unit Price", FieldName = "UnitPrice", Width = "120px", Format = "c", TextAlign = "right", IsHiddenField = true },
                new GridField() { Header = "Discount", FieldName = "Discount", Width = "120px", Format = "c", TextAlign = "right", IsHiddenField = true },
                new GridField() { Header = "Tax Rate (%)", FieldName = "Tax", Width = "110px", Format = "#,##0.00", TextAlign = "right", IsHiddenField = true },
                new GridField() { Header = "VAT", FieldName = "VAT", Width = "110px", Format = "c", TextAlign = "right", IsHiddenField = true },
                new GridField() { Header = "Payment Method", FieldName = "PaymentMethod", Width = "160px", TextAlign = "left", IsHiddenField = true },
                new GridField() { Header = "Class", FieldName = "ClassName", Width = "140px", TextAlign = "left", IsHiddenField = true },
                new GridField() { Header = "ClassID", FieldName = "ClassID", IsHiddenField = true },
                new GridField() { Header = "Approved By", FieldName = "ApprovedBy", Width = "160px", TextAlign = "left", IsHiddenField = true },
                new GridField() { Header = "Receipt No.", FieldName = "ReceiptNumber", Width = "160px", TextAlign = "left", IsHiddenField = true },
                new GridField() { Header = "Currency", FieldName = "Currency", Width = "100px", TextAlign = "center", IsHiddenField = true },
                new GridField() { Header = "Notes", FieldName = "Notes", Width = "200px", TextAlign = "left", IsHiddenField = true },
            };

            ViewBag.ReportTitle = "Expense Detail";

            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = false,
                ServerPaging = false,
            };
            string drawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            ViewBag.DrawHTMLSearchForm = drawHTMLSearchForm;
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
                    System.Diagnostics.Trace.TraceWarning("Expense Detail report: company header was not found for SystemID {0}, WarehouseID {1}.", systemId, warehouseId);
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
                System.Diagnostics.Trace.TraceWarning("Expense Detail report: failed to load company header: {0}", ex.Message);
            }
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public JsonResult GetCompanyHeader(string className, string classId)
        {
            string targetClass = !string.IsNullOrWhiteSpace(className) ? className.Trim() : (!string.IsNullOrWhiteSpace(classId) ? classId.Trim() : "");
            if (targetClass == "--- All Classes ---" || targetClass == "All") targetClass = "";

            if (string.IsNullOrWhiteSpace(targetClass))
                return Json(new { success = false, message = "A class must be selected." }, JsonRequestBehavior.AllowGet);

            string companyNameKh = "";
            string companyNameEn = "";
            string resolvedClassId = classId ?? "";
            bool classFound = false;

            try
            {
                HTML html = new HTML();
                string connStr = html.GetConnectionString();

                int sysId = 13;
                if (!string.IsNullOrEmpty(Request["SystemID"])) int.TryParse(Request["SystemID"], out sysId);
                else if (!string.IsNullOrEmpty(html.SystemID)) int.TryParse(html.SystemID, out sysId);

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

                using (var con = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    con.Open();

                    string sqlClassComp = @"
                        SELECT TOP 1 *
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
                            CASE WHEN @SysID > 0 AND SystemID = @SysID THEN 0 ELSE 1 END,
                            ClassID ASC";

                    using (var cmdCls = new System.Data.SqlClient.SqlCommand(sqlClassComp, con))
                    {
                        cmdCls.Parameters.AddWithValue("@SysID", sysId);
                        cmdCls.Parameters.AddWithValue("@ClassID", classId ?? "");
                        cmdCls.Parameters.AddWithValue("@ClassName", targetClass);
                        using (var drCls = cmdCls.ExecuteReader())
                        {
                            if (drCls.Read())
                            {
                                classFound = true;
                                companyNameKh = readCol(drCls, new string[] { "KHName", "CompanyNameKH", "CompanyNameKhmer", "CompanyNameKh" });
                                companyNameEn = readCol(drCls, new string[] { "FullName", "ClassName" });

                                if (!string.IsNullOrWhiteSpace(companyNameKh))
                                {
                                    string clean = companyNameKh.Trim(' ', '\t', '\r', '\n', '\u200B', '\u00A0');
                                    if (!clean.StartsWith("ក្រុមហ៊ុន")) companyNameKh = "ក្រុមហ៊ុន " + clean;
                                    else companyNameKh = clean;
                                }

                                string dbClassId = readCol(drCls, new string[] { "ClassID" });
                                resolvedClassId = dbClassId;
                            }
                        }
                    }
                }

                if (!classFound)
                    return Json(new { success = false, message = "The selected class could not be found." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    companyNameKh = companyNameKh,
                    companyNameEn = companyNameEn,
                    classId = resolvedClassId
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Expense Detail GetCompanyHeader error: " + ex.Message);
                return Json(new
                {
                    success = true,
                    companyNameKh = companyNameKh,
                    companyNameEn = companyNameEn,
                    classId = resolvedClassId
                }, JsonRequestBehavior.AllowGet);
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
                            WHERE (@SysID = 0 OR SystemID = @SysID OR SystemID IS NULL) 
                              AND (@WhID = 0 OR WarehouseID = @WhID OR WarehouseID IS NULL) 
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
                            SELECT ClassID, ClassName 
                            FROM dbo.TblClass WITH (NOLOCK)
                            WHERE (@SysID = 0 OR SystemID = @SysID OR SystemID IS NULL)
                              AND (@WhID = 0 OR WarehouseID = @WhID OR WarehouseID IS NULL)
                              AND (@DomID = '' OR DomainID LIKE '%' + @DomID + '%' OR DomainID IS NULL)
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
                                WHERE (@SysID = 0 OR SystemID = @SysID OR SystemID IS NULL)
                                  AND (@WhID = 0 OR WarehouseID = @WhID OR WarehouseID IS NULL)
                                  AND (@DomID = '' OR DomainID LIKE '%' + @DomID + '%' OR DomainID IS NULL)

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
