using MVC;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace NewInventorySystem.Controllers
{
    public class BalanceSheetController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                HTML html = new HTML();
                int sysId = 13;
                if (!string.IsNullOrEmpty(Request["SystemID"])) int.TryParse(Request["SystemID"], out sysId);
                else if (!string.IsNullOrEmpty(html.SystemID)) int.TryParse(html.SystemID, out sysId);

                int defaultWhId = GetDefaultWarehouseIdForSystem(sysId);
                int whId = defaultWhId;
                if (!string.IsNullOrEmpty(Request["WarehouseID"]))
                {
                    int parsedWh;
                    if (int.TryParse(Request["WarehouseID"], out parsedWh) && parsedWh > 0)
                    {
                        if (parsedWh == 16 && sysId != 13) whId = defaultWhId;
                        else whId = parsedWh;
                    }
                }
                else if (!string.IsNullOrEmpty(html.WarehouseID))
                {
                    int parsedWh;
                    if (int.TryParse(html.WarehouseID, out parsedWh) && parsedWh > 0)
                    {
                        if (parsedWh == 16 && sysId != 13) whId = defaultWhId;
                        else whId = parsedWh;
                    }
                }

                var comp = GetCompanyHeader(sysId, whId);

                string khHeader = comp.CompanyNameKH ?? "";
                string enHeader = comp.CompanyName ?? "";
                string logoHeader = comp.Logo ?? "";

                // If TblClass returned nothing, fall back per system
                if (string.IsNullOrWhiteSpace(khHeader))
                {
                    switch (sysId)
                    {
                        case 28: khHeader = "ក្រុមហ៊ុន អិនអេសអិម ថេក"; enHeader = "NSM TECH CO., LTD"; break;
                        case 25: khHeader = "ក្រុមហ៊ុន ប៊ីអេសអិម"; enHeader = "BSM"; break;
                        case 27: khHeader = "ក្រុមហ៊ុន ប៊ីអេស"; enHeader = "BS CO., LTD"; break;
                        default: khHeader = "ក្រុមហ៊ុន អ៊ិន អេស អិម សូលូសិន អេន ម៉េនធីណេន ឯ.ក"; enHeader = "NSM SOLUTION AND MAINTENANCE CO., LTD"; break;
                    }
                }

                // Ensure ក្រុមហ៊ុន prefix (DB may or may not include it)
                if (!khHeader.StartsWith("ក្រុមហ៊ុន"))
                    khHeader = "ក្រុមហ៊ុន " + khHeader;

                ViewBag.CompanyKH = khHeader;
                ViewBag.CompanyEN = enHeader;
                ViewBag.CompanyLogo = logoHeader;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("BalanceSheet Index comp header: " + ex.Message);
            }
            return View();
        }

        [HttpPost]
        public ActionResult GetBalanceSheetData(
            string toDate,
            string txType = null,
            string accountNo = null,
            string name = null,
            string refNo = null,
            string createdBy = null,
            string classId = null,
            string location = null,
            string sortBy = null,
            string orderBy = null,
            string systemID = null,
            string warehouseID = null,
            string displayColumns = "default")
        {
            try
            {
                if (string.IsNullOrEmpty(toDate))
                    return Json(new { success = false, message = "Please provide a valid date." });

                DateTime dtFrom, dtTo;
                if (toDate.Contains("-"))
                {
                    var parts = toDate.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2
                        || !DateTime.TryParseExact(parts[0].Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dtFrom)
                        || !DateTime.TryParseExact(parts[1].Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dtTo))
                        return Json(new { success = false, message = "Invalid date format. Expected: dd/MM/yyyy - dd/MM/yyyy" });
                }
                else
                {
                    if (!DateTime.TryParseExact(toDate.Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dtTo))
                        return Json(new { success = false, message = "Invalid date format. Expected: dd/MM/yyyy" });
                    dtFrom = new DateTime(dtTo.Year, 1, 1);
                }

                if (dtFrom > dtTo)
                {
                    DateTime temp = dtFrom;
                    dtFrom = dtTo;
                    dtTo = temp;
                }

                displayColumns = (displayColumns ?? "default").Trim().ToLowerInvariant();
                var periodCols = GeneratePeriodColumns(ref dtFrom, dtTo, displayColumns);
                int numCols = periodCols.Count;

                using (var db = new DBContext())
                {
                    HTML html = new HTML();
                    int sysId = 13;
                    if (!string.IsNullOrEmpty(systemID)) int.TryParse(systemID, out sysId);
                    else if (!string.IsNullOrEmpty(Request["SystemID"])) int.TryParse(Request["SystemID"], out sysId);
                    else if (!string.IsNullOrEmpty(html.SystemID)) int.TryParse(html.SystemID, out sysId);

                    int defaultWhId = GetDefaultWarehouseIdForSystem(sysId);
                    int whId = defaultWhId;
                    if (!string.IsNullOrEmpty(location))
                    {
                        int parsedLoc;
                        if (int.TryParse(location, out parsedLoc) && parsedLoc > 0)
                            whId = parsedLoc;
                    }
                    else if (!string.IsNullOrEmpty(warehouseID))
                    {
                        int parsedWh;
                        if (int.TryParse(warehouseID, out parsedWh) && parsedWh > 0)
                        {
                            if (parsedWh == 16 && sysId != 13) whId = defaultWhId;
                            else whId = parsedWh;
                        }
                    }
                    else if (!string.IsNullOrEmpty(Request["WarehouseID"]))
                    {
                        int parsedWh;
                        if (int.TryParse(Request["WarehouseID"], out parsedWh) && parsedWh > 0)
                        {
                            if (parsedWh == 16 && sysId != 13) whId = defaultWhId;
                            else whId = parsedWh;
                        }
                    }
                    else if (!string.IsNullOrEmpty(html.WarehouseID))
                    {
                        int parsedWh;
                        if (int.TryParse(html.WarehouseID, out parsedWh) && parsedWh > 0)
                        {
                            if (parsedWh == 16 && sysId != 13) whId = defaultWhId;
                            else whId = parsedWh;
                        }
                    }

                    string domId = "%" + (html.DomainID ?? "001") + "%";

                    int? createdByUserId = null;
                    if (!string.IsNullOrEmpty(createdBy)) createdByUserId = Convert.ToInt32(createdBy);

                    int? filterClassId = null;
                    if (!string.IsNullOrEmpty(classId)) filterClassId = Convert.ToInt32(classId);

                    var sqlParams = new List<SqlParameter>
                    {
                        new SqlParameter("@FromDate", dtFrom.ToString("yyyy-MM-dd 00:00:00")),
                        new SqlParameter("@ToDate", dtTo.ToString("yyyy-MM-dd 23:59:59")),
                        new SqlParameter("@SystemID", sysId),
                        new SqlParameter("@WarehouseID", whId),
                        new SqlParameter("@DomainID", domId),
                        new SqlParameter("@TxType", string.IsNullOrEmpty(txType) ? (object)DBNull.Value : txType),
                        new SqlParameter("@AccountNo", string.IsNullOrEmpty(accountNo) ? (object)DBNull.Value : accountNo),
                        new SqlParameter("@AccountName", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name),
                        new SqlParameter("@RefNo", string.IsNullOrEmpty(refNo) ? (object)DBNull.Value : refNo),
                        new SqlParameter("@CreatedBy", createdByUserId.HasValue ? (object)createdByUserId.Value : DBNull.Value),
                        new SqlParameter("@ClassID", filterClassId.HasValue ? (object)filterClassId.Value : DBNull.Value)
                    };

                    var flatRows = db.Database.SqlQuery<BalanceSheetFlatNode>(
                        "EXEC [dbo].[SP_GetBalanceSheetData] @FromDate, @ToDate, @SystemID, @WarehouseID, @DomainID, @TxType, @AccountNo, @AccountName, @RefNo, @CreatedBy, @ClassID",
                        sqlParams.ToArray()
                    ).ToList();

                    foreach (var row in flatRows)
                    {
                        string catNameLower = (row.AccountCategory ?? "").ToLower();
                        bool isMisclassifiedLiability = row.AccountTypes == "Asset" &&
                            (catNameLower.Contains("liability") || catNameLower.Contains("payable"));

                        if (isMisclassifiedLiability)
                        {
                            row.AccountTypes = "Liability";
                            row.Balance = row.TotalCredit - row.TotalDebit;
                        }
                    }

                    // If multiple period columns requested (day, week, month, quarter, year), fetch daily movements
                    var deltasByAccount = new Dictionary<int, List<AccountDailyDelta>>();
                    if (numCols > 1)
                    {
                        try
                        {
                            var deltaParams = new List<SqlParameter>
                            {
                                new SqlParameter("@FromDate", dtFrom.ToString("yyyy-MM-dd 00:00:00")),
                                new SqlParameter("@ToDate", dtTo.ToString("yyyy-MM-dd 23:59:59")),
                                new SqlParameter("@SystemID", sysId),
                                new SqlParameter("@WarehouseID", whId),
                                new SqlParameter("@DomainID", domId),
                                new SqlParameter("@TxType", string.IsNullOrEmpty(txType) ? (object)DBNull.Value : txType),
                                new SqlParameter("@AccountNo", string.IsNullOrEmpty(accountNo) ? (object)DBNull.Value : accountNo),
                                new SqlParameter("@AccountName", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name),
                                new SqlParameter("@RefNo", string.IsNullOrEmpty(refNo) ? (object)DBNull.Value : refNo),
                                new SqlParameter("@CreatedBy", createdByUserId.HasValue ? (object)createdByUserId.Value : DBNull.Value),
                                new SqlParameter("@ClassID", filterClassId.HasValue ? (object)filterClassId.Value : DBNull.Value)
                            };

                            var dailyDeltas = db.Database.SqlQuery<AccountDailyDelta>(@"
                                SELECT 
                                    j.AccountID,
                                    CAST(COALESCE(
                                        ob.OpeningBalanceDate,
                                        NULLIF(tx.TransactionDate, '1900-01-01'), 
                                        NULLIF(j.TransactionDate, '1900-01-01'), 
                                        j.EntryDate, 
                                        j.CreateDate
                                    ) AS DATE) AS TxDate,
                                    ISNULL(SUM(j.Debit), 0) AS TotalDebit,
                                    ISNULL(SUM(j.Credit), 0) AS TotalCredit
                                FROM dbo.TblAccount_JournalEntries j WITH (NOLOCK)
                                JOIN dbo.TblChartOfAccount c WITH (NOLOCK) ON j.AccountID = c.ChartOfAccountID
                                LEFT JOIN dbo.TblAccount_Transaction tx WITH (NOLOCK) ON j.TransactionID = tx.TransactionID
                                LEFT JOIN dbo.TblAccount_OpeningBalance ob WITH (NOLOCK) ON COALESCE(j.OpeningBalanceID, tx.OpeningBalanceID) = ob.OpeningBalanceID
                                WHERE c.SystemID = @SystemID
                                  AND (@WarehouseID = 0 OR @WarehouseID IS NULL OR c.WarehouseID = @WarehouseID)
                                  AND c.DomainID LIKE @DomainID
                                  AND COALESCE(
                                      ob.OpeningBalanceDate,
                                      NULLIF(tx.TransactionDate, '1900-01-01'), 
                                      NULLIF(j.TransactionDate, '1900-01-01'), 
                                      j.EntryDate, 
                                      j.CreateDate
                                  ) >= @FromDate
                                  AND COALESCE(
                                      ob.OpeningBalanceDate,
                                      NULLIF(tx.TransactionDate, '1900-01-01'), 
                                      NULLIF(j.TransactionDate, '1900-01-01'), 
                                      j.EntryDate, 
                                      j.CreateDate
                                  ) <= @ToDate
                                  AND (@AccountNo IS NULL OR c.AccountNo = @AccountNo)
                                  AND (@AccountName IS NULL OR c.AccountName LIKE '%' + @AccountName + '%')
                                  AND (@CreatedBy IS NULL OR j.CreateByUserID = @CreatedBy)
                                  AND (@TxType IS NULL 
                                       OR COALESCE(NULLIF(tx.Type, ''), NULLIF(j.Type, '')) = @TxType
                                       OR (@TxType = 'Journal' AND COALESCE(NULLIF(tx.Type, ''), NULLIF(j.Type, '')) = 'Journal Entry')
                                       OR (@TxType = 'OpeningBalance' AND COALESCE(NULLIF(tx.Type, ''), NULLIF(j.Type, '')) IN ('OPENING', 'Opening Balance')))
                                  AND (@RefNo IS NULL OR COALESCE(NULLIF(j.InvoiceNo, ''), NULLIF(tx.InvoiceNo, '')) LIKE '%' + @RefNo + '%')
                                  AND (@ClassID IS NULL OR COALESCE(NULLIF(j.ClassNameID, 0), NULLIF(tx.ClassNameID, 0)) = @ClassID)
                                GROUP BY 
                                    j.AccountID,
                                    CAST(COALESCE(
                                        ob.OpeningBalanceDate,
                                        NULLIF(tx.TransactionDate, '1900-01-01'), 
                                        NULLIF(j.TransactionDate, '1900-01-01'), 
                                        j.EntryDate, 
                                        j.CreateDate
                                    ) AS DATE)

                                UNION ALL

                                SELECT 
                                    ob.ChartOfAccountID AS AccountID,
                                    CAST(ob.OpeningBalanceDate AS DATE) AS TxDate,
                                    ISNULL(SUM(
                                        CASE 
                                            WHEN (t.AccountTypes = 'Asset' AND cat.AccountCategory NOT LIKE '%liability%' AND cat.AccountCategory NOT LIKE '%payable%')
                                                 OR t.AccountTypes = 'Expense'
                                            THEN ob.OpeningBalance 
                                            ELSE 0 
                                        END
                                    ), 0) AS TotalDebit,
                                    ISNULL(SUM(
                                        CASE 
                                            WHEN (t.AccountTypes IN ('Liability', 'Equity', 'Revenue', 'Income'))
                                                 OR (t.AccountTypes = 'Asset' AND (cat.AccountCategory LIKE '%liability%' OR cat.AccountCategory LIKE '%payable%'))
                                            THEN ob.OpeningBalance 
                                            ELSE 0 
                                        END
                                    ), 0) AS TotalCredit
                                FROM dbo.TblAccount_OpeningBalance ob WITH (NOLOCK)
                                JOIN dbo.TblChartOfAccount c WITH (NOLOCK) ON ob.ChartOfAccountID = c.ChartOfAccountID
                                JOIN dbo.TblAccountCategories cat WITH (NOLOCK) ON c.AccountCategoryID = cat.AccountCategoryID
                                JOIN dbo.TblAccountTypes t WITH (NOLOCK) ON cat.AccountTypesID = t.AccountTypesID
                                WHERE (@WarehouseID IS NULL OR @WarehouseID = 0 OR ob.WarehouseID = @WarehouseID)
                                  AND (@SystemID    IS NULL OR @SystemID = 0    OR ob.SystemID    = @SystemID)
                                  AND (c.DomainID LIKE @DomainID OR ob.DomainID LIKE @DomainID)
                                  AND ob.OpeningBalanceDate >= CAST(@FromDate AS DATE)
                                  AND ob.OpeningBalanceDate <= CAST(@ToDate AS DATE)
                                  AND (@AccountNo IS NULL OR c.AccountNo = @AccountNo)
                                  AND (@AccountName IS NULL OR c.AccountName LIKE '%' + @AccountName + '%')
                                  AND (@TxType IS NULL OR @TxType IN ('OpeningBalance', 'OPENING', 'Opening Balance'))
                                  AND (
                                      @ClassID IS NULL 
                                      OR ob.ClassID = @ClassID
                                      OR (ob.ClassID IS NULL AND NOT EXISTS (
                                          SELECT 1 FROM dbo.TblAccount_JournalEntries jCls WITH (NOLOCK)
                                          WHERE jCls.AccountID = c.ChartOfAccountID 
                                            AND COALESCE(NULLIF(jCls.ClassNameID, 0), 0) <> @ClassID
                                            AND jCls.ClassNameID IS NOT NULL
                                      ))
                                  )
                                  AND NOT EXISTS (
                                      SELECT 1 FROM dbo.TblAccount_JournalEntries jExisting WITH (NOLOCK)
                                      LEFT JOIN dbo.TblAccount_Transaction txExisting WITH (NOLOCK) ON jExisting.TransactionID = txExisting.TransactionID
                                      WHERE jExisting.OpeningBalanceID = ob.OpeningBalanceID OR txExisting.OpeningBalanceID = ob.OpeningBalanceID
                                  )
                                GROUP BY 
                                    ob.ChartOfAccountID,
                                    CAST(ob.OpeningBalanceDate AS DATE)",
                                deltaParams.ToArray()).ToList();

                            deltasByAccount = dailyDeltas.GroupBy(d => d.AccountID).ToDictionary(g => g.Key, g => g.ToList());
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.TraceWarning("dailyDeltas query warning: " + ex.Message);
                        }
                    }

                    var nodeMap = flatRows.ToDictionary(
                        r => r.ChartOfAccountID,
                        r =>
                        {
                            var colBalances = new List<decimal>();
                            var periodBalances = new Dictionary<string, decimal>();

                            bool isLiabilityOrRevOrEquity = r.AccountTypes == "Liability"
                                || r.AccountTypes == "Equity"
                                || (r.AccountTypes ?? "").IndexOf("Revenue", StringComparison.OrdinalIgnoreCase) >= 0
                                || (r.AccountTypes ?? "").IndexOf("Income", StringComparison.OrdinalIgnoreCase) >= 0;

                            List<AccountDailyDelta> accDeltas;
                            deltasByAccount.TryGetValue(r.ChartOfAccountID, out accDeltas);

                            if (numCols > 1)
                            {
                                for (int k = 0; k < numCols; k++)
                                {
                                    var col = periodCols[k];
                                    decimal bal = r.Balance;
                                    if (accDeltas != null && accDeltas.Count > 0)
                                    {
                                        // Subtract all transactions that occurred after this period's cutoff date
                                        decimal deltaAfter = accDeltas
                                            .Where(d => d.TxDate.Date > col.EndDate.Date)
                                            .Sum(d => isLiabilityOrRevOrEquity ? (d.TotalCredit - d.TotalDebit) : (d.TotalDebit - d.TotalCredit));
                                        bal -= deltaAfter;
                                    }
                                    colBalances.Add(bal);
                                    periodBalances[col.Key] = bal;
                                }
                            }
                            else
                            {
                                colBalances.Add(r.Balance);
                                string singleKey = periodCols.Count > 0 ? periodCols[0].Key : "col_0";
                                periodBalances[singleKey] = r.Balance;
                            }

                            return new BalanceSheetNode
                            {
                                ChartOfAccountID = r.ChartOfAccountID,
                                AccountNo = r.AccountNo,
                                AccountName = r.AccountName,
                                ParentAccountID = r.ParentAccountID,
                                AccountCategoryID = r.AccountCategoryID,
                                AccountCategory = r.AccountCategory,
                                AccountTypes = r.AccountTypes,
                                Balance = r.Balance,
                                ColBalances = colBalances,
                                PeriodBalances = periodBalances,
                                IsLeaf = true
                            };
                        });

                    var rootNodes = new List<BalanceSheetNode>();
                    foreach (var node in nodeMap.Values)
                    {
                        if (node.ParentAccountID != 0 && nodeMap.ContainsKey(node.ParentAccountID))
                        {
                            nodeMap[node.ParentAccountID].Children.Add(node);
                            nodeMap[node.ParentAccountID].IsLeaf = false;
                        }
                        else
                        {
                            rootNodes.Add(node);
                        }
                    }

                    RollupBalances(rootNodes, numCols);
                    SyncPeriodBalances(rootNodes, periodCols);
                    PruneEmptyLeaves(rootNodes);
                    SortNodes(rootNodes, sortBy, orderBy);

                    var assetRoots = rootNodes.Where(n => string.Equals(n.AccountTypes, "Asset", StringComparison.OrdinalIgnoreCase)).ToList();
                    var liabRoots = rootNodes.Where(n => string.Equals(n.AccountTypes, "Liability", StringComparison.OrdinalIgnoreCase)).ToList();
                    var equityRoots = rootNodes.Where(n => string.Equals(n.AccountTypes, "Equity", StringComparison.OrdinalIgnoreCase)).ToList();

                    // Calculate Net Income for each column period
                    var netIncomeByPeriod = new List<decimal>();
                    for (int i = 0; i < numCols; i++)
                    {
                        var rev_i = rootNodes
                            .Where(n => (n.AccountTypes ?? "").IndexOf("Revenue", StringComparison.OrdinalIgnoreCase) >= 0 || (n.AccountTypes ?? "").IndexOf("Income", StringComparison.OrdinalIgnoreCase) >= 0)
                            .Sum(n => n.ColBalances.Count > i ? n.ColBalances[i] : n.Balance);

                        var exp_i = rootNodes
                            .Where(n => (n.AccountTypes ?? "").IndexOf("Expense", StringComparison.OrdinalIgnoreCase) >= 0 || (n.AccountTypes ?? "").IndexOf("Cost", StringComparison.OrdinalIgnoreCase) >= 0)
                            .Sum(n => n.ColBalances.Count > i ? n.ColBalances[i] : n.Balance);

                        netIncomeByPeriod.Add(rev_i - exp_i);
                    }
                    decimal netIncome = netIncomeByPeriod.LastOrDefault();

                    var allCategories = db.Database.SqlQuery<AccountCategoryDto>(@"
                        SELECT c.AccountCategoryID, c.AccountCategory, c.AccountTypesID, t.AccountTypes, c.OrderBy
                        FROM dbo.TblAccountCategories c WITH (NOLOCK)
                        JOIN dbo.TblAccountTypes t WITH (NOLOCK) ON c.AccountTypesID = t.AccountTypesID
                    ").ToList();

                    var categorySortOrder = new Dictionary<int, int>();
                    foreach (var cat in allCategories)
                    {
                        int sortVal = cat.OrderBy ?? cat.AccountCategoryID;
                        categorySortOrder[cat.AccountCategoryID] = sortVal;
                    }

                    var dynamicAssetGroups = new List<(string GroupName, int SortOrder, List<int> CategoryIDs)>
                    {
                        (" Current Assets",     1, new List<int>()),
                        (" Non Current Assets", 2, new List<int>())
                    };

                    var dynamicLiabilityGroups = new List<(string GroupName, int SortOrder, List<int> CategoryIDs)>
                    {
                        (" Current Liability",   1, new List<int>()),
                        (" Long Term Liability", 2, new List<int>())
                    };

                    foreach (var cat in allCategories)
                    {
                        string catNameLower = (cat.AccountCategory ?? "").ToLower();
                        string typeName = (cat.AccountTypes ?? "").ToLower();

                        bool isLiability = typeName.Contains("liability") || catNameLower.Contains("liability") || catNameLower.Contains("payable");
                        bool isAsset = (typeName.Contains("asset") || catNameLower.Contains("asset") || catNameLower.Contains("bank") || catNameLower.Contains("receivable") || catNameLower.Contains("inventory")) && !isLiability;

                        if (isAsset)
                        {
                            if (catNameLower.Contains("non current") || catNameLower.Contains("non-current") || catNameLower.Contains("fixed"))
                                dynamicAssetGroups[1].CategoryIDs.Add(cat.AccountCategoryID);
                            else
                                dynamicAssetGroups[0].CategoryIDs.Add(cat.AccountCategoryID);
                        }
                        else if (isLiability)
                        {
                            if (catNameLower.Contains("long term") || catNameLower.Contains("long-term"))
                                dynamicLiabilityGroups[1].CategoryIDs.Add(cat.AccountCategoryID);
                            else
                                dynamicLiabilityGroups[0].CategoryIDs.Add(cat.AccountCategoryID);
                        }
                    }

                    var assetGroupsDef = dynamicAssetGroups.Select(g => (g.GroupName, g.SortOrder, g.CategoryIDs.ToArray())).ToList();
                    var liabilityGroupsDef = dynamicLiabilityGroups.Select(g => (g.GroupName, g.SortOrder, g.CategoryIDs.ToArray())).ToList();

                    var assetsSection = BuildSection("ASSETS", assetRoots, assetGroupsDef, sortBy, orderBy, categorySortOrder, numCols);
                    var liabSection = BuildSection("LIABILITIES", liabRoots, liabilityGroupsDef, sortBy, orderBy, categorySortOrder, numCols);
                    var equitySection = BuildEquitySection(equityRoots, netIncome, netIncomeByPeriod, periodCols, sortBy, orderBy, categorySortOrder);

                    var totalLiabAndEquityByPeriod = new List<decimal>();
                    for (int i = 0; i < numCols; i++)
                    {
                        decimal l = liabSection.ColTotals.Count > i ? liabSection.ColTotals[i] : liabSection.Total;
                        decimal e = equitySection.ColTotals.Count > i ? equitySection.ColTotals[i] : equitySection.Total;
                        totalLiabAndEquityByPeriod.Add(l + e);
                    }

                    var response = new BalanceSheetResponse
                    {
                        Assets = assetsSection,
                        Liabilities = liabSection,
                        Equity = equitySection,
                        TotalAssets = assetsSection.Total,
                        TotalLiabilities = liabSection.Total,
                        TotalEquity = equitySection.Total,
                        TotalAssetsByPeriod = assetsSection.ColTotals,
                        TotalLiabilitiesByPeriod = liabSection.ColTotals,
                        TotalEquityByPeriod = equitySection.ColTotals,
                        TotalLiabAndEquityByPeriod = totalLiabAndEquityByPeriod,
                        Columns = periodCols,
                        DisplayColumns = displayColumns
                    };

                    var comp = GetCompanyHeader(sysId, whId);
                    return Json(new { success = true, data = response, toDate = dtTo.ToString("dd/MM/yyyy"), company = comp });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("GetBalanceSheetData error: {0}", ex);
                string errMsg = ex.Message;
                if (ex.InnerException != null) errMsg += " | " + ex.InnerException.Message;
                return Json(new { success = false, message = errMsg });
            }
        }

        public class CompanyHeaderDto
        {
            public int ClassID { get; set; } = 0;
            public string ClassName { get; set; } = "";
            public string CompanyNameKH { get; set; } = "";
            public string CompanyName { get; set; } = "";
            public string Logo { get; set; } = "";
            public string VATNumber { get; set; } = "";
            public string AddressKH { get; set; } = "";
            public string CompanyAddress { get; set; } = "";
        }

        private int GetDefaultWarehouseIdForSystem(int systemId)
        {
            switch (systemId)
            {
                case 13: return 16;
                case 28: return 26;
                case 25: return 23;
                case 27: return 25;
                default: return 16;
            }
        }

        private int GetDefaultClassIdForSystem(int systemId)
        {
            switch (systemId)
            {
                case 13: return 5;
                case 28: return 14;
                case 25: return 10; // BSM
                case 27: return 12; // BS
                default: return 0;
            }
        }

        private CompanyHeaderDto GetCompanyHeader(int systemId, int warehouseId, int? classId = null)
        {
            var comp = new CompanyHeaderDto();
            try
            {
                HTML html = new HTML();
                // Fixed default company class per system:
                // System 13 -> ClassID 5 (NSM)
                // System 28 -> ClassID 14 (NSM Tech Co., Ltd)
                // System 25 (BSM) -> ClassID 10 (BSM)
                // System 27 (BS) -> ClassID 12 (BS Co.,Ltd)
                // Class filter does not change the company header.
                int defaultClassId = GetDefaultClassIdForSystem(systemId);

                string sql = @"
                    SELECT TOP 1 *
                    FROM dbo.TblClass
                    WHERE (@DefaultClassID > 0 AND ClassID = @DefaultClassID)
                       OR (@DefaultClassID = 0 AND (@SystemID = 0 OR SystemID = @SystemID))
                    ORDER BY 
                        CASE WHEN @DefaultClassID > 0 AND ClassID = @DefaultClassID THEN 0 ELSE 1 END,
                        CASE WHEN StatusID = 40 THEN 0 ELSE 1 END,
                        CASE WHEN (@WarehouseID > 0 AND WarehouseID = @WarehouseID) THEN 0 ELSE 1 END,
                        ClassID ASC";

                var p = new Dictionary<string, string>
                {
                    { "DefaultClassID", defaultClassId.ToString() },
                    { "SystemID", systemId.ToString() },
                    { "WarehouseID", warehouseId.ToString() }
                };

                DataSet ds = html.LoadData(sql, false, p);
                if ((ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) && systemId != 13)
                {
                    p["DefaultClassID"] = "5";
                    p["SystemID"] = "13";
                    p["WarehouseID"] = "16";
                    ds = html.LoadData(sql, false, p);
                }

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    if (dr.Table.Columns.Contains("ClassID") && dr["ClassID"] != DBNull.Value)
                        comp.ClassID = Convert.ToInt32(dr["ClassID"]);

                    if (dr.Table.Columns.Contains("ClassName") && dr["ClassName"] != DBNull.Value)
                        comp.ClassName = dr["ClassName"].ToString().Trim();

                    string khName = "";
                    if (dr.Table.Columns.Contains("KHName") && dr["KHName"] != DBNull.Value)
                        khName = dr["KHName"].ToString().Trim();
                    else if (dr.Table.Columns.Contains("CompanyNameKH") && dr["CompanyNameKH"] != DBNull.Value)
                        khName = dr["CompanyNameKH"].ToString().Trim();

                    string enName = "";
                    if (dr.Table.Columns.Contains("FullName") && dr["FullName"] != DBNull.Value)
                        enName = dr["FullName"].ToString().Trim();
                    else if (dr.Table.Columns.Contains("CompanyName") && dr["CompanyName"] != DBNull.Value)
                        enName = dr["CompanyName"].ToString().Trim();
                    else if (!string.IsNullOrEmpty(comp.ClassName))
                        enName = comp.ClassName;

                    string logo = "";
                    if (dr.Table.Columns.Contains("FileLogo") && dr["FileLogo"] != DBNull.Value)
                        logo = dr["FileLogo"].ToString().Trim();
                    else if (dr.Table.Columns.Contains("Logo") && dr["Logo"] != DBNull.Value)
                        logo = dr["Logo"].ToString().Trim();

                    if (dr.Table.Columns.Contains("VATNumber") && dr["VATNumber"] != DBNull.Value)
                        comp.VATNumber = dr["VATNumber"].ToString();
                    if (dr.Table.Columns.Contains("AddressKH") && dr["AddressKH"] != DBNull.Value)
                        comp.AddressKH = dr["AddressKH"].ToString();
                    if (dr.Table.Columns.Contains("CompanyAddress") && dr["CompanyAddress"] != DBNull.Value)
                        comp.CompanyAddress = dr["CompanyAddress"].ToString();

                    comp.CompanyNameKH = khName;
                    comp.CompanyName = enName;
                    comp.Logo = logo;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("GetCompanyHeader error: " + ex.Message);
            }
            return comp;
        }

        private List<BalanceSheetColumnDef> GeneratePeriodColumns(ref DateTime dtFrom, DateTime dtTo, string displayColumns)
        {
            var cols = new List<BalanceSheetColumnDef>();
            string mode = (displayColumns ?? "default").Trim().ToLowerInvariant();

            if (dtFrom > dtTo)
            {
                DateTime tmp = dtFrom;
                dtFrom = dtTo;
                dtTo = tmp;
            }

            switch (mode)
            {
                case "day":
                    {
                        // If same day or less than 6 days difference, expand to show 7 days up to dtTo
                        if (dtFrom.Date == dtTo.Date || (dtTo.Date - dtFrom.Date).TotalDays < 6)
                        {
                            dtFrom = dtTo.Date.AddDays(-6);
                        }
                        DateTime cur = dtFrom.Date;
                        int idx = 0;
                        while (cur <= dtTo.Date && idx < 31)
                        {
                            cols.Add(new BalanceSheetColumnDef
                            {
                                Key = "col_" + idx++,
                                Label = cur.ToString("dd/MM/yyyy"),
                                SubLabel = "Amount",
                                DateStr = cur.ToString("dd/MM/yyyy"),
                                EndDate = new DateTime(cur.Year, cur.Month, cur.Day, 23, 59, 59)
                            });
                            cur = cur.AddDays(1);
                        }
                        break;
                    }
                case "week":
                    {
                        // If within same week or less than 7 days, expand to start of current month
                        if ((dtTo.Date - dtFrom.Date).TotalDays < 7)
                        {
                            dtFrom = new DateTime(dtTo.Year, dtTo.Month, 1);
                        }
                        DateTime cur = dtFrom.Date;
                        int idx = 0;
                        while (cur <= dtTo.Date && idx < 52)
                        {
                            DateTime wEnd = cur.AddDays(6);
                            if (wEnd > dtTo.Date) wEnd = dtTo.Date;

                            cols.Add(new BalanceSheetColumnDef
                            {
                                Key = "col_" + idx++,
                                Label = cur.ToString("dd/MM/yyyy") + " - " + wEnd.ToString("dd/MM/yyyy"),
                                SubLabel = "Amount",
                                DateStr = wEnd.ToString("dd/MM/yyyy"),
                                EndDate = new DateTime(wEnd.Year, wEnd.Month, wEnd.Day, 23, 59, 59)
                            });
                            cur = wEnd.AddDays(1);
                        }
                        break;
                    }
                case "month":
                    {
                        // If same month, expand to start of current year so Jan..current month are all shown (e.g. Jan/2026..Sep/2026)
                        if (dtFrom.Year == dtTo.Year && dtFrom.Month == dtTo.Month)
                        {
                            dtFrom = new DateTime(dtTo.Year, 1, 1);
                        }
                        DateTime cur = new DateTime(dtFrom.Year, dtFrom.Month, 1);
                        DateTime endMonth = new DateTime(dtTo.Year, dtTo.Month, 1);
                        int idx = 0;
                        while (cur <= endMonth && idx < 36)
                        {
                            int daysInM = DateTime.DaysInMonth(cur.Year, cur.Month);
                            DateTime mEnd = new DateTime(cur.Year, cur.Month, daysInM);
                            if (cur.Year == dtTo.Year && cur.Month == dtTo.Month && dtTo.Date < mEnd)
                                mEnd = dtTo.Date;

                            cols.Add(new BalanceSheetColumnDef
                            {
                                Key = "col_" + idx++,
                                Label = cur.ToString("MMM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                                SubLabel = "Amount",
                                DateStr = mEnd.ToString("dd/MM/yyyy"),
                                EndDate = new DateTime(mEnd.Year, mEnd.Month, mEnd.Day, 23, 59, 59)
                            });
                            cur = cur.AddMonths(1);
                        }
                        break;
                    }
                case "quarter":
                    {
                        // If within same year, expand to start of current year so Q1..current quarter are all shown
                        if (dtFrom.Year == dtTo.Year)
                        {
                            dtFrom = new DateTime(dtTo.Year, 1, 1);
                        }
                        int startYear = dtFrom.Year;
                        int endYear = dtTo.Year;
                        int idx = 0;
                        for (int y = startYear; y <= endYear; y++)
                        {
                            for (int q = 1; q <= 4; q++)
                            {
                                int qStartMonth = (q - 1) * 3 + 1;
                                int qEndMonth = q * 3;
                                DateTime qStartDate = new DateTime(y, qStartMonth, 1);
                                int daysInQEnd = DateTime.DaysInMonth(y, qEndMonth);
                                DateTime qEndDate = new DateTime(y, qEndMonth, daysInQEnd);

                                if (qEndDate < dtFrom.Date || qStartDate > dtTo.Date)
                                    continue;

                                DateTime cutoff = qEndDate > dtTo.Date ? dtTo.Date : qEndDate;
                                cols.Add(new BalanceSheetColumnDef
                                {
                                    Key = "col_" + idx++,
                                    Label = "Q" + q + "/" + y,
                                    SubLabel = "Amount",
                                    DateStr = cutoff.ToString("dd/MM/yyyy"),
                                    EndDate = new DateTime(cutoff.Year, cutoff.Month, cutoff.Day, 23, 59, 59)
                                });
                            }
                        }
                        break;
                    }
                case "year":
                    {
                        // If within same year, show comparative 3 years: (year - 2), (year - 1), year
                        if (dtFrom.Year == dtTo.Year)
                        {
                            dtFrom = new DateTime(dtTo.Year - 2, 1, 1);
                        }
                        int idx = 0;
                        for (int y = dtFrom.Year; y <= dtTo.Year; y++)
                        {
                            DateTime yEnd = new DateTime(y, 12, 31);
                            if (yEnd > dtTo.Date) yEnd = dtTo.Date;

                            cols.Add(new BalanceSheetColumnDef
                            {
                                Key = "col_" + idx++,
                                Label = y.ToString(),
                                SubLabel = "Amount",
                                DateStr = yEnd.ToString("dd/MM/yyyy"),
                                EndDate = new DateTime(yEnd.Year, yEnd.Month, yEnd.Day, 23, 59, 59)
                            });
                        }
                        break;
                    }
                default:
                    {
                        cols.Add(new BalanceSheetColumnDef
                        {
                            Key = "col_0",
                            Label = "Amount",
                            SubLabel = "Amount",
                            DateStr = dtTo.ToString("dd/MM/yyyy"),
                            EndDate = new DateTime(dtTo.Year, dtTo.Month, dtTo.Day, 23, 59, 59)
                        });
                        break;
                    }
            }

            if (cols.Count == 0)
            {
                cols.Add(new BalanceSheetColumnDef
                {
                    Key = "col_0",
                    Label = "Amount",
                    SubLabel = "Amount",
                    DateStr = dtTo.ToString("dd/MM/yyyy"),
                    EndDate = new DateTime(dtTo.Year, dtTo.Month, dtTo.Day, 23, 59, 59)
                });
            }

            return cols;
        }

        private BalanceSheetSection BuildSection(
            string sectionName,
            List<BalanceSheetNode> nodes,
            List<(string GroupName, int SortOrder, int[] CategoryIDs)> groupDefs,
            string sortBy,
            string orderBy,
            Dictionary<int, int> categorySortOrder,
            int numCols)
        {
            var section = new BalanceSheetSection { SectionName = sectionName };

            var categoryMap = nodes
                .GroupBy(n => new { n.AccountCategoryID, n.AccountCategory })
                .ToDictionary(
                    g => g.Key.AccountCategoryID,
                    g =>
                    {
                        int sort;
                        categorySortOrder.TryGetValue(g.Key.AccountCategoryID, out sort);

                        IEnumerable<BalanceSheetNode> sortedAccounts = g;
                        sortedAccounts = sortBy == "AccountName"
                            ? (orderBy == "desc") ? g.OrderByDescending(n => n.AccountName) : g.OrderBy(n => n.AccountName)
                            : sortBy == "Balance"
                                ? (orderBy == "desc") ? g.OrderByDescending(n => n.Balance) : g.OrderBy(n => n.Balance)
                                : (orderBy == "desc") ? g.OrderByDescending(n => n.AccountNo) : g.OrderBy(n => n.AccountNo);

                        var accountList = sortedAccounts.ToList();
                        var catColTotals = new List<decimal>();
                        for (int i = 0; i < numCols; i++)
                        {
                            catColTotals.Add(accountList.Sum(n => n.ColBalances.Count > i ? n.ColBalances[i] : n.Balance));
                        }

                        return new BalanceSheetCategory
                        {
                            AccountCategoryID = g.Key.AccountCategoryID,
                            CategoryName = g.Key.AccountCategory,
                            Accounts = accountList,
                            Total = g.Sum(n => n.Balance),
                            ColTotals = catColTotals,
                            SortOrder = sort == 0 ? 99 : sort
                        };
                    });

            var usedCategoryIDs = new HashSet<int>();
            foreach (var grpDef in groupDefs.OrderBy(g => g.SortOrder))
            {
                var grp = new BalanceSheetCategoryGroup
                {
                    GroupName = grpDef.GroupName,
                    SortOrder = grpDef.SortOrder
                };

                foreach (var catID in grpDef.CategoryIDs)
                {
                    if (categoryMap.ContainsKey(catID))
                    {
                        grp.Categories.Add(categoryMap[catID]);
                        usedCategoryIDs.Add(catID);
                    }
                }

                grp.Categories = grp.Categories.OrderBy(c => c.SortOrder).ToList();
                var grpColTotals = new List<decimal>();
                for (int i = 0; i < numCols; i++)
                {
                    grpColTotals.Add(grp.Categories.Sum(c => c.ColTotals.Count > i ? c.ColTotals[i] : c.Total));
                }
                grp.ColTotals = grpColTotals;
                grp.Total = grp.Categories.Sum(c => c.Total);

                if (grp.Categories.Count > 0)
                    section.Groups.Add(grp);
            }

            var orphans = categoryMap.Values
                .Where(c => !usedCategoryIDs.Contains(c.AccountCategoryID))
                .OrderBy(c => c.SortOrder)
                .ToList();

            if (orphans.Count > 0)
            {
                var orphanColTotals = new List<decimal>();
                for (int i = 0; i < numCols; i++)
                {
                    orphanColTotals.Add(orphans.Sum(c => c.ColTotals.Count > i ? c.ColTotals[i] : c.Total));
                }

                section.Groups.Add(new BalanceSheetCategoryGroup
                {
                    GroupName = "Other",
                    SortOrder = 99,
                    Categories = orphans,
                    Total = orphans.Sum(c => c.Total),
                    ColTotals = orphanColTotals
                });
            }

            var secColTotals = new List<decimal>();
            for (int i = 0; i < numCols; i++)
            {
                secColTotals.Add(section.Groups.Sum(g => g.ColTotals.Count > i ? g.ColTotals[i] : g.Total));
            }
            section.ColTotals = secColTotals;
            section.Total = section.Groups.Sum(g => g.Total);
            return section;
        }

        private BalanceSheetSection BuildEquitySection(
            List<BalanceSheetNode> equityRoots,
            decimal netIncome,
            List<decimal> netIncomeByPeriod,
            List<BalanceSheetColumnDef> periodCols,
            string sortBy,
            string orderBy,
            Dictionary<int, int> categorySortOrder)
        {
            int numCols = periodCols.Count;
            var section = new BalanceSheetSection { SectionName = "EQUITY" };

            var equityGroup = new BalanceSheetCategoryGroup
            {
                GroupName = "",
                SortOrder = 1
            };

            if (netIncome != 0 || (netIncomeByPeriod != null && netIncomeByPeriod.Any(x => x != 0)))
            {
                equityGroup.Categories.Add(new BalanceSheetCategory
                {
                    AccountCategoryID = -1,
                    CategoryName = "Net Income",
                    Accounts = new List<BalanceSheetNode>
                    {
                        new BalanceSheetNode
                        {
                            AccountNo   = "",
                            AccountName = "Net Income",
                            Balance     = netIncome,
                            ColBalances = new List<decimal>(netIncomeByPeriod ?? new List<decimal>()),
                            IsLeaf      = true
                        }
                    },
                    Total = netIncome,
                    ColTotals = new List<decimal>(netIncomeByPeriod ?? new List<decimal>()),
                    SortOrder = 0
                });
            }

            var equityByCategory = equityRoots
                .GroupBy(n => new { n.AccountCategoryID, n.AccountCategory })
                .OrderBy(g => g.Key.AccountCategoryID);

            foreach (var grp in equityByCategory)
            {
                int sort;
                categorySortOrder.TryGetValue(grp.Key.AccountCategoryID, out sort);

                IEnumerable<BalanceSheetNode> sortedAccounts = grp;
                sortedAccounts = sortBy == "AccountName"
                    ? (orderBy == "desc") ? grp.OrderByDescending(n => n.AccountName) : grp.OrderBy(n => n.AccountName)
                    : sortBy == "Balance"
                        ? (orderBy == "desc") ? grp.OrderByDescending(n => n.Balance) : grp.OrderBy(n => n.Balance)
                        : (orderBy == "desc") ? grp.OrderByDescending(n => n.AccountNo) : grp.OrderBy(n => n.AccountNo);

                var accountList = sortedAccounts.ToList();
                var catColTotals = new List<decimal>();
                for (int i = 0; i < numCols; i++)
                {
                    catColTotals.Add(accountList.Sum(n => n.ColBalances.Count > i ? n.ColBalances[i] : n.Balance));
                }

                equityGroup.Categories.Add(new BalanceSheetCategory
                {
                    AccountCategoryID = grp.Key.AccountCategoryID,
                    CategoryName = grp.Key.AccountCategory,
                    Accounts = accountList,
                    Total = grp.Sum(n => n.Balance),
                    ColTotals = catColTotals,
                    SortOrder = sort == 0 ? 99 : sort
                });
            }

            var grpColTotals = new List<decimal>();
            for (int i = 0; i < numCols; i++)
            {
                grpColTotals.Add(equityGroup.Categories.Sum(c => c.ColTotals.Count > i ? c.ColTotals[i] : c.Total));
            }
            equityGroup.ColTotals = grpColTotals;
            equityGroup.Total = equityGroup.Categories.Sum(c => c.Total);

            section.Groups.Add(equityGroup);
            section.ColTotals = new List<decimal>(grpColTotals);
            section.Total = equityGroup.Total;
            return section;
        }

        private decimal RollupBalances(List<BalanceSheetNode> nodes, int numCols)
        {
            decimal total = 0;
            foreach (var node in nodes)
            {
                while (node.ColBalances.Count < numCols)
                    node.ColBalances.Add(node.Balance);

                if (node.Children.Count > 0)
                {
                    decimal childTotal = RollupBalances(node.Children, numCols);
                    for (int i = 0; i < numCols; i++)
                    {
                        decimal childColSum = node.Children.Sum(c => c.ColBalances.Count > i ? c.ColBalances[i] : c.Balance);
                        node.ColBalances[i] += childColSum;
                    }
                    node.Balance += childTotal;
                }
                total += node.Balance;
            }
            return total;
        }

        private void SyncPeriodBalances(List<BalanceSheetNode> nodes, List<BalanceSheetColumnDef> periodCols)
        {
            foreach (var node in nodes)
            {
                for (int i = 0; i < periodCols.Count; i++)
                {
                    decimal val = node.ColBalances.Count > i ? node.ColBalances[i] : node.Balance;
                    node.PeriodBalances[periodCols[i].Key] = val;
                }
                if (node.Children.Count > 0)
                    SyncPeriodBalances(node.Children, periodCols);
            }
        }

        private void PruneEmptyLeaves(List<BalanceSheetNode> nodes)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                if (nodes[i].Children.Count > 0)
                    PruneEmptyLeaves(nodes[i].Children);

                bool isAllZero = nodes[i].Balance == 0 && (nodes[i].ColBalances.Count == 0 || nodes[i].ColBalances.All(b => b == 0));
                if (nodes[i].Children.Count == 0 && isAllZero)
                    nodes.RemoveAt(i);
            }
        }

        private void SortNodes(List<BalanceSheetNode> nodes, string sortBy, string orderBy)
        {
            if (sortBy == "AccountName")
            {
                if (orderBy == "desc")
                    nodes.Sort((a, b) => string.Compare(b.AccountName, a.AccountName, StringComparison.OrdinalIgnoreCase));
                else
                    nodes.Sort((a, b) => string.Compare(a.AccountName, b.AccountName, StringComparison.OrdinalIgnoreCase));
            }
            else if (sortBy == "Balance")
            {
                if (orderBy == "desc")
                    nodes.Sort((a, b) => b.Balance.CompareTo(a.Balance));
                else
                    nodes.Sort((a, b) => a.Balance.CompareTo(b.Balance));
            }
            else
            {
                if (orderBy == "desc")
                    nodes.Sort((a, b) => string.Compare(b.AccountNo, a.AccountNo, StringComparison.OrdinalIgnoreCase));
                else
                    nodes.Sort((a, b) => string.Compare(a.AccountNo, b.AccountNo, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var n in nodes)
                SortNodes(n.Children, sortBy, orderBy);
        }

        [HttpPost]
        public ActionResult GetFilterOptions(string systemID = null, string warehouseID = null)
        {
            using (var db = new DBContext())
            {
                var accounts = new List<AccountFilterItem>();
                var locations = new List<LocationFilterItem>();
                var classes = new List<ClassFilterItem>();
                var users = new List<UserFilterItem>();

                HTML html = new HTML();
                int sysId = 13;
                if (!string.IsNullOrEmpty(systemID)) int.TryParse(systemID, out sysId);
                else if (!string.IsNullOrEmpty(Request["SystemID"])) int.TryParse(Request["SystemID"], out sysId);
                else if (!string.IsNullOrEmpty(html.SystemID)) int.TryParse(html.SystemID, out sysId);

                int whId = 0;
                if (!string.IsNullOrEmpty(warehouseID)) int.TryParse(warehouseID, out whId);
                else if (!string.IsNullOrEmpty(Request["WarehouseID"])) int.TryParse(Request["WarehouseID"], out whId);
                else if (!string.IsNullOrEmpty(html.WarehouseID)) int.TryParse(html.WarehouseID, out whId);

                string domId = html.DomainID ?? "";

                try
                {
                    accounts = db.Database.SqlQuery<AccountFilterItem>(@"
                        SELECT c.AccountNo, c.AccountName, ISNULL(cat.AccountCategory, '') AS AccountCategory
                        FROM   dbo.TblChartOfAccount c WITH (NOLOCK)
                        LEFT JOIN dbo.TblAccountCategories cat WITH (NOLOCK) ON cat.AccountCategoryID = c.AccountCategoryID
                        WHERE  (@SystemID = 0 OR c.SystemID = @SystemID OR c.SystemID IS NULL)
                          AND  (@WarehouseID = 0 OR c.WarehouseID = @WarehouseID OR c.WarehouseID IS NULL)
                          AND  (@DomainID = '' OR c.DomainID LIKE '%' + @DomainID + '%' OR c.DomainID IS NULL)
                        ORDER BY c.AccountNo",
                        new SqlParameter("@SystemID", sysId),
                        new SqlParameter("@WarehouseID", whId),
                        new SqlParameter("@DomainID", domId)).ToList();
                }
                catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("GetFilterOptions accounts: {0}", ex.Message); }

                try
                {
                    locations = db.Database.SqlQuery<LocationFilterItem>(@"
                        SELECT DISTINCT uw.WID AS WarehouseID, 
                               COALESCE(NULLIF(b.BranchName + ' - ' + uw.WarehouseName, ''), uw.WarehouseName) AS WarehouseName
                        FROM   dbo.TblUserWarehouse uw WITH (NOLOCK)
                        LEFT JOIN dbo.TblBranch b WITH (NOLOCK) ON b.BranchID = uw.BranchID
                        WHERE  (@SystemID = 0 OR uw.SystemID = @SystemID)
                        ORDER BY WarehouseName",
                        new SqlParameter("@SystemID", sysId)).ToList();
                }
                catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("GetFilterOptions locations: {0}", ex.Message); }

                try
                {
                    classes = db.Database.SqlQuery<ClassFilterItem>(@"
                        SELECT ClassID, ClassName
                        FROM   dbo.TblClass WITH (NOLOCK)
                        WHERE  (@SystemID = 0 OR SystemID = @SystemID OR SystemID IS NULL)
                        ORDER BY ClassName",
                        new SqlParameter("@SystemID", sysId)).ToList();
                }
                catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("GetFilterOptions classes: {0}", ex.Message); }

                try
                {
                    users = db.Database.SqlQuery<UserFilterItem>(@"
                        SELECT DISTINCT u.UserLoginID AS UserID, 
                                COALESCE(NULLIF(u.FullName, ''), NULLIF(u.UserName, ''), NULLIF(u.TelegramUserName, ''), 'User ' + CAST(u.UserLoginID AS VARCHAR)) AS FullName, 
                                CASE 
                                    WHEN NULLIF(emp.Gender, '') IS NOT NULL THEN 
                                        CASE 
                                            WHEN emp.Gender IN ('M', 'Male', 'male', 'm', '1') THEN 'Male'
                                            WHEN emp.Gender IN ('F', 'Female', 'female', 'f', '2') THEN 'Female'
                                            ELSE emp.Gender 
                                        END
                                    WHEN NULLIF(v.Gender, '') IS NOT NULL THEN 
                                        CASE 
                                            WHEN v.Gender IN ('M', 'Male', 'male', 'm', '1') THEN 'Male'
                                            WHEN v.Gender IN ('F', 'Female', 'female', 'f', '2') THEN 'Female'
                                            ELSE v.Gender 
                                        END
                                    WHEN LOWER(u.FullName) LIKE 'mr.%' OR LOWER(u.FullName) LIKE 'mr %' OR LOWER(u.FullName) LIKE N'លោក %' OR LOWER(u.UserName) LIKE 'mr.%' THEN 'Male'
                                    WHEN LOWER(u.FullName) LIKE 'ms.%' OR LOWER(u.FullName) LIKE 'mrs.%' OR LOWER(u.FullName) LIKE 'miss%' OR LOWER(u.FullName) LIKE N'អ្នកស្រី %' OR LOWER(u.FullName) LIKE N'កញ្ញា %' THEN 'Female'
                                    ELSE ''
                                END AS Gender
                        FROM   dbo.TblUserLogin u WITH (NOLOCK)
                        LEFT JOIN dbo.TblEmployees emp WITH (NOLOCK) ON (
                            emp.EmployeeName = u.FullName 
                            OR emp.EmployeeName = u.UserName 
                            OR CHARINDEX(emp.EmployeeName, u.FullName) > 0
                            OR CHARINDEX(u.FullName, emp.EmployeeName) > 0
                        )
                        LEFT JOIN dbo.TblVendor v WITH (NOLOCK) ON (
                            (v.RefID = u.UserLoginID AND v.RefType IN ('User', 'Employee')) 
                            OR v.VendorName = u.FullName
                        )
                        WHERE  u.StatusID = 3
                        ORDER BY FullName").ToList();
                }
                catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("GetFilterOptions users: {0}", ex.Message); }

                return Json(new { success = true, accounts, locations, classes, users });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Invoice drill-down (Fully synchronized with SP logic)
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        public ActionResult GetAccountInvoices(
      string accountNo,
      string accountName,
      string toDate,
      string fromDate = null, // <--- Add fromDate parameter
      string systemID = null,
      string warehouseID = null,
      string classId = null,
      string createdBy = null,
      string txType = null,
      string refNo = null)
        {
            try
            {
                if (string.IsNullOrEmpty(accountNo) || string.IsNullOrEmpty(toDate))
                    return Json(new { success = false, message = "Missing required parameters." });

                DateTime dtFrom, dtTo;

                // Support both date range string ("dd/MM/yyyy - dd/MM/yyyy") or separate fromDate/toDate
                if (toDate.Contains("-") && toDate.Contains("/"))
                {
                    var parts = toDate.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                    DateTime.TryParseExact(parts[0].Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dtFrom);
                    DateTime.TryParseExact(parts[1].Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dtTo);
                }
                else
                {
                    DateTime.TryParseExact(toDate.Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dtTo);
                    if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParseExact(fromDate.Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dtFrom))
                    {
                        // dtFrom parsed successfully
                    }
                    else
                    {
                        // Default to first day of month if no fromDate provided
                        dtFrom = new DateTime(dtTo.Year, dtTo.Month, 1);
                    }
                }

                using (var db = new DBContext())
                {
                    HTML html = new HTML();
                    int sysId = 13;
                    if (!string.IsNullOrEmpty(systemID)) int.TryParse(systemID, out sysId);
                    else if (!string.IsNullOrEmpty(Request["SystemID"])) int.TryParse(Request["SystemID"], out sysId);
                    else if (!string.IsNullOrEmpty(html.SystemID)) int.TryParse(html.SystemID, out sysId);

                    int defaultWhId = GetDefaultWarehouseIdForSystem(sysId);
                    int whId = defaultWhId;
                    if (!string.IsNullOrEmpty(warehouseID))
                    {
                        int parsedWh;
                        if (int.TryParse(warehouseID, out parsedWh) && parsedWh > 0)
                        {
                            if (parsedWh == 16 && sysId != 13) whId = defaultWhId;
                            else whId = parsedWh;
                        }
                    }
                    else if (!string.IsNullOrEmpty(Request["WarehouseID"]))
                    {
                        int parsedWh;
                        if (int.TryParse(Request["WarehouseID"], out parsedWh) && parsedWh > 0)
                        {
                            if (parsedWh == 16 && sysId != 13) whId = defaultWhId;
                            else whId = parsedWh;
                        }
                    }
                    else if (!string.IsNullOrEmpty(html.WarehouseID))
                    {
                        int parsedWh;
                        if (int.TryParse(html.WarehouseID, out parsedWh) && parsedWh > 0)
                        {
                            if (parsedWh == 16 && sysId != 13) whId = defaultWhId;
                            else whId = parsedWh;
                        }
                    }

                    string domId = "%" + (html.DomainID ?? "001") + "%";
                    int? createdByUserId = !string.IsNullOrEmpty(createdBy) ? Convert.ToInt32(createdBy) : (int?)null;
                    int? filterClassId = !string.IsNullOrEmpty(classId) ? Convert.ToInt32(classId) : (int?)null;

                    // 1. Calculate Opening Balance before dtFrom (Opening Balance Table + Prior Journal Entries)
                    string sqlOB = @"
                        SELECT 
                            (
                                ISNULL((
                                    SELECT SUM(ob.OpeningBalance) 
                                    FROM dbo.TblAccount_OpeningBalance ob WITH (NOLOCK)
                                    JOIN dbo.TblChartOfAccount c WITH (NOLOCK) ON ob.ChartOfAccountID = c.ChartOfAccountID
                                    WHERE c.AccountNo = @AccountNo
                                      AND (@WarehouseID = 0 OR ob.WarehouseID = @WarehouseID OR ob.WarehouseID IS NULL)
                                      AND (@SystemID = 0 OR ob.SystemID = @SystemID OR ob.SystemID IS NULL)
                                      AND CAST(ob.OpeningBalanceDate AS DATE) < CAST(@FromDate AS DATE)
                                      AND NOT EXISTS (
                                          SELECT 1 FROM dbo.TblAccount_JournalEntries jEx WITH (NOLOCK)
                                          LEFT JOIN dbo.TblAccount_Transaction txEx WITH (NOLOCK) ON jEx.TransactionID = txEx.TransactionID
                                          WHERE jEx.OpeningBalanceID = ob.OpeningBalanceID OR txEx.OpeningBalanceID = ob.OpeningBalanceID
                                      )
                                ), 0)
                                +
                                ISNULL((
                                    SELECT SUM(ISNULL(j.Debit, 0) - ISNULL(j.Credit, 0))
                                    FROM dbo.TblAccount_JournalEntries j WITH (NOLOCK)
                                    JOIN dbo.TblChartOfAccount c WITH (NOLOCK) ON j.AccountID = c.ChartOfAccountID
                                    LEFT JOIN dbo.TblAccount_Transaction tx WITH (NOLOCK) ON j.TransactionID = tx.TransactionID
                                    LEFT JOIN dbo.TblCustomerInvoice ci WITH (NOLOCK) ON ci.CustomerInvoiceID = j.CustomerInvoiceID
                                    LEFT JOIN dbo.TblAccount_OpeningBalance ob WITH (NOLOCK) ON COALESCE(j.OpeningBalanceID, tx.OpeningBalanceID) = ob.OpeningBalanceID
                                    WHERE c.AccountNo = @AccountNo
                                      AND (@SystemID = 0 OR c.SystemID = @SystemID OR c.SystemID IS NULL)
                                      AND (c.DomainID IS NULL OR c.DomainID LIKE @DomainID)
                                      AND (COALESCE(j.InvoiceNo, tx.InvoiceNo) IS NULL OR COALESCE(j.InvoiceNo, tx.InvoiceNo) NOT LIKE 'Del[_]%')
                                      AND (
                                          COALESCE(NULLIF(tx.Type, ''), NULLIF(j.Type, '')) IN ('OPENING', 'Opening Balance')
                                          OR j.OpeningBalanceID IS NOT NULL
                                          OR tx.OpeningBalanceID IS NOT NULL
                                      )
                                      AND CAST(COALESCE(
                                          ob.OpeningBalanceDate,
                                          NULLIF(tx.TransactionDate, '1900-01-01'),
                                          NULLIF(j.TransactionDate, '1900-01-01'),
                                          NULLIF(ci.InvoiceDate, '1900-01-01'),
                                          NULLIF(tx.InvoiceDate, '1900-01-01'),
                                          j.EntryDate,
                                          ci.CreateDate,
                                          j.CreateDate
                                      ) AS DATE) < CAST(@FromDate AS DATE)
                                ), 0)
                            ) AS OpeningDebit,
                            (
                                SELECT TOP 1 ob.OpeningBalanceDate
                                FROM dbo.TblAccount_OpeningBalance ob WITH (NOLOCK)
                                JOIN dbo.TblChartOfAccount c WITH (NOLOCK) ON ob.ChartOfAccountID = c.ChartOfAccountID
                                WHERE c.AccountNo = @AccountNo
                                  AND (@WarehouseID = 0 OR ob.WarehouseID = @WarehouseID OR ob.WarehouseID IS NULL)
                                  AND (@SystemID = 0 OR ob.SystemID = @SystemID OR ob.SystemID IS NULL)
                                  AND CAST(ob.OpeningBalanceDate AS DATE) < CAST(@FromDate AS DATE)
                                ORDER BY ob.OpeningBalanceDate DESC
                            ) AS OpeningBalanceDate";
                    var obParams = new SqlParameter[]
                    {
                        new SqlParameter("@AccountNo", accountNo),
                        new SqlParameter("@WarehouseID", whId),
                        new SqlParameter("@SystemID", sysId),
                        new SqlParameter("@DomainID", domId),
                        new SqlParameter("@FromDate", dtFrom.ToString("yyyy-MM-dd 00:00:00")),
                        new SqlParameter("@ToDate", dtTo.ToString("yyyy-MM-dd 23:59:59"))
                    };

                    var obResult = db.Database.SqlQuery<OpeningBalanceResultDto>(sqlOB, obParams).FirstOrDefault();
                    decimal openingBal = obResult != null ? obResult.OpeningDebit : 0;

                    // 2. Fetch Journal Entries strictly BETWEEN dtFrom and dtTo
                    string sql = @"
						SELECT
						    u.InvoiceNo,
						    CONVERT(VARCHAR(10), CAST(u.InvoiceDate AS date), 105) AS InvoiceDate,
						    u.InvoiceType,
						    u.AccountNo,
						    u.AccountName,
						    u.AccountTypes,
						    u.Debit,
						    u.Credit,
						    u.Description
						FROM (
						    SELECT
						        x.InvoiceNo,
						        CONVERT(VARCHAR(10), x.InvoiceDate, 120) AS InvoiceDate,
						        x.InvoiceType,
						        c.AccountNo,
						        c.AccountName,
						        t.AccountTypes,
						        ISNULL(j.Debit, 0) AS Debit,
						        ISNULL(j.Credit, 0) AS Credit,
					
			        COALESCE(
			            NULLIF(LTRIM(RTRIM(exp.Description)), ''),
			            NULLIF(LTRIM(RTRIM(
			                CASE
			                    WHEN NULLIF(LTRIM(RTRIM(j.Description)), '') IS NULL
			                      OR LTRIM(RTRIM(j.Description)) = LTRIM(RTRIM(c.AccountName))
			                      OR LTRIM(RTRIM(j.Description)) = (LTRIM(RTRIM(c.AccountNo)) + '-' + LTRIM(RTRIM(c.AccountName)))
			                      OR LTRIM(RTRIM(j.Description)) = (LTRIM(RTRIM(c.AccountNo)) + ' - ' + LTRIM(RTRIM(c.AccountName)))
			                    THEN counterpart.Description

			                    WHEN j.Description LIKE '[0-9][0-9][0-9][0-9][0-9]%-%'
			                     AND CHARINDEX(' - ', j.Description) > 0
			                    THEN LTRIM(SUBSTRING(j.Description, CHARINDEX(' - ', j.Description) + 3, LEN(j.Description)))

			                    WHEN j.Description LIKE '[0-9][0-9][0-9][0-9][0-9]%-%'
			                     AND CHARINDEX(' - ', j.Description) = 0
			                    THEN counterpart.Description

			                    ELSE j.Description
			                END
			            )), ''),
			            ''
			        ) AS Description
						    FROM dbo.TblAccount_JournalEntries j WITH (NOLOCK)
						    JOIN dbo.TblChartOfAccount c WITH (NOLOCK)
						        ON j.AccountID = c.ChartOfAccountID
						    JOIN dbo.TblAccountCategories cat WITH (NOLOCK)
						        ON c.AccountCategoryID = cat.AccountCategoryID
						    JOIN dbo.TblAccountTypes t WITH (NOLOCK)
						        ON cat.AccountTypesID = t.AccountTypesID
						    LEFT JOIN dbo.TblAccount_Transaction tx WITH (NOLOCK)
						        ON j.TransactionID = tx.TransactionID
						    LEFT JOIN dbo.TblCustomerInvoice ci WITH (NOLOCK)
						        ON ci.CustomerInvoiceID = j.CustomerInvoiceID
						    LEFT JOIN dbo.TblAccount_OpeningBalance ob WITH (NOLOCK)
						        ON j.OpeningBalanceID = ob.OpeningBalanceID
						    CROSS APPLY (
						        SELECT
						            COALESCE(
						                NULLIF(j.InvoiceNo, ''), 
						                NULLIF(tx.InvoiceNo, ''), 
						                NULLIF(tx.TransactionNo, ''), 
						                CASE 
						                    WHEN COALESCE(NULLIF(tx.Type, ''), NULLIF(j.Type, '')) IN ('OPENING', 'Opening Balance') OR j.OpeningBalanceID IS NOT NULL 
						                    THEN 'OPENING' 
						                    ELSE '-' 
						                END
						            ) AS InvoiceNo,
						            COALESCE(
						                ob.OpeningBalanceDate,
						                NULLIF(tx.TransactionDate, '1900-01-01'),
						                NULLIF(j.TransactionDate, '1900-01-01'),
						                ci.InvoiceDate,
						                tx.InvoiceDate,
						                j.EntryDate,
						                j.CreateDate
						            ) AS InvoiceDate,
						            COALESCE(
						                NULLIF(tx.Type, ''),
						                NULLIF(j.Type, ''),
						                NULLIF(ci.Type, ''),
						                'Journal Entry'
						            ) AS InvoiceType
						    ) x
						    OUTER APPLY (
						        SELECT TOP 1 e.Description
						        FROM dbo.Expenses e WITH (NOLOCK)
						        WHERE (
						            x.InvoiceNo IS NOT NULL AND x.InvoiceNo <> ''
						            AND (e.ExpenseNo = x.InvoiceNo OR e.[No] = x.InvoiceNo)
						        )
						          AND (@SystemID = 0 OR e.SystemID = @SystemID OR c.SystemID = 0 OR e.SystemID = c.SystemID)
						        ORDER BY e.ExpenseID DESC
						    ) exp
						    OUTER APPLY (
						        SELECT 
						            STRING_AGG(CAST(sub.ItemText AS NVARCHAR(MAX)), ', ') AS Description
						        FROM (
						            SELECT DISTINCT
						                LTRIM(RTRIM(
						                    CASE 	
						                        WHEN CHARINDEX(', Being exp', 
						                            CASE 
						                                WHEN j2.Description LIKE '[0-9][0-9][0-9][0-9][0-9]%-%' AND CHARINDEX(' - ', j2.Description) > 0
						                                THEN LTRIM(SUBSTRING(j2.Description, CHARINDEX(' - ', j2.Description) + 3, LEN(j2.Description)))
						                                ELSE LTRIM(RTRIM(j2.Description))
						                            END
						                        ) > 0
						                        THEN SUBSTRING(
						                            CASE 
						                                WHEN j2.Description LIKE '[0-9][0-9][0-9][0-9][0-9]%-%' AND CHARINDEX(' - ', j2.Description) > 0
						                                THEN LTRIM(SUBSTRING(j2.Description, CHARINDEX(' - ', j2.Description) + 3, LEN(j2.Description)))
						                                ELSE LTRIM(RTRIM(j2.Description))
						                            END,
						                            1,
						                            CHARINDEX(', Being exp', 
						                                CASE 
						                                    WHEN j2.Description LIKE '[0-9][0-9][0-9][0-9][0-9]%-%' AND CHARINDEX(' - ', j2.Description) > 0
						                                    THEN LTRIM(SUBSTRING(j2.Description, CHARINDEX(' - ', j2.Description) + 3, LEN(j2.Description)))
						                                    ELSE LTRIM(RTRIM(j2.Description))
						                                END
						                            ) - 1
						                        )
						                        WHEN j2.Description LIKE '[0-9][0-9][0-9][0-9][0-9]%-%' AND CHARINDEX(' - ', j2.Description) > 0
						                        THEN LTRIM(SUBSTRING(j2.Description, CHARINDEX(' - ', j2.Description) + 3, LEN(j2.Description)))
						                        ELSE LTRIM(RTRIM(j2.Description))
						                    END
						                )) AS ItemText
						            FROM dbo.TblAccount_JournalEntries j2 WITH (NOLOCK)
						            JOIN dbo.TblChartOfAccount c2 WITH (NOLOCK)
						                ON j2.AccountID = c2.ChartOfAccountID
						            WHERE x.InvoiceNo IS NOT NULL AND x.InvoiceNo <> ''
						              AND COALESCE(NULLIF(j2.InvoiceNo, ''), NULLIF(tx.InvoiceNo, '')) = x.InvoiceNo
						              AND j2.AccountID <> j.AccountID
						              AND c2.SystemID = c.SystemID
						              AND NULLIF(LTRIM(RTRIM(j2.Description)), '') IS NOT NULL
						              AND LTRIM(RTRIM(j2.Description)) <> LTRIM(RTRIM(c2.AccountName))
						              AND LTRIM(RTRIM(j2.Description)) <> (LTRIM(RTRIM(c2.AccountNo)) + '-' + LTRIM(RTRIM(c2.AccountName)))
						              AND LTRIM(RTRIM(j2.Description)) <> (LTRIM(RTRIM(c2.AccountNo)) + ' - ' + LTRIM(RTRIM(c2.AccountName)))
						        ) sub
						        WHERE NULLIF(sub.ItemText, '') IS NOT NULL
						    ) counterpart
						    WHERE (@SystemID = 0 OR c.SystemID = @SystemID)
						      AND c.AccountNo = @AccountNo
						      AND c.DomainID LIKE @DomainID
						      AND (x.InvoiceNo IS NULL OR x.InvoiceNo NOT LIKE 'Del[_]%')
                                AND NOT (
                                        ISNULL(j.TransactionID, 0) = 0
                                    AND NULLIF(j.Type, '') IS NULL
                                    AND j.CustomerInvoiceID IS NULL
                                    AND j.OpeningBalanceID IS NULL
                                )
						      AND x.InvoiceDate >= @FromDate
						      AND x.InvoiceDate <= @ToDate
						      AND (@TxType IS NULL OR x.InvoiceType = @TxType OR j.Type = @TxType OR tx.Type = @TxType)
						      AND (@RefNo IS NULL OR x.InvoiceNo LIKE '%' + @RefNo + '%')
						      AND (@CreatedBy IS NULL OR j.CreateByUserID = @CreatedBy)
						      AND (@ClassID IS NULL OR COALESCE(NULLIF(j.ClassNameID, 0), NULLIF(tx.ClassNameID, 0)) = @ClassID)

						    UNION ALL

						    SELECT
						        'OPENING' AS InvoiceNo,
						        CONVERT(VARCHAR(10), ob.OpeningBalanceDate, 120) AS InvoiceDate,
						        'Opening Balance' AS InvoiceType,
						        c.AccountNo,
						        c.AccountName,
						        t.AccountTypes,
						        CASE 
						            WHEN (t.AccountTypes = 'Asset' AND cat.AccountCategory NOT LIKE '%liability%' AND cat.AccountCategory NOT LIKE '%payable%')
						                 OR t.AccountTypes = 'Expense'
						            THEN ISNULL(ob.OpeningBalance, 0)
						            ELSE 0 
						        END AS Debit,
						        CASE 
						            WHEN t.AccountTypes IN ('Liability', 'Equity', 'Revenue', 'Income')
						                 OR (t.AccountTypes = 'Asset' AND (cat.AccountCategory LIKE '%liability%' OR cat.AccountCategory LIKE '%payable%'))
						            THEN ISNULL(ob.OpeningBalance, 0)
						            ELSE 0 
						        END AS Credit,
						        ISNULL(NULLIF(ob.Memo, ''), 'Opening Balance') AS Description
						    FROM dbo.TblAccount_OpeningBalance ob WITH (NOLOCK)
						    JOIN dbo.TblChartOfAccount c WITH (NOLOCK) ON ob.ChartOfAccountID = c.ChartOfAccountID
						    JOIN dbo.TblAccountCategories cat WITH (NOLOCK) ON c.AccountCategoryID = cat.AccountCategoryID
						    JOIN dbo.TblAccountTypes t WITH (NOLOCK) ON cat.AccountTypesID = t.AccountTypesID
						    WHERE (@SystemID = 0 OR ob.SystemID = @SystemID)
						      AND (@WarehouseID = 0 OR ob.WarehouseID = @WarehouseID OR ob.WarehouseID IS NULL)
						      AND c.AccountNo = @AccountNo
						      AND (c.DomainID LIKE @DomainID OR ob.DomainID LIKE @DomainID)
						      AND ob.OpeningBalanceDate >= @FromDate
						      AND ob.OpeningBalanceDate <= @ToDate
						      AND (@TxType IS NULL OR @TxType IN ('OpeningBalance', 'OPENING', 'Opening Balance'))
						      AND NOT EXISTS (
						          SELECT 1 FROM dbo.TblAccount_JournalEntries jEx WITH (NOLOCK)
						          LEFT JOIN dbo.TblAccount_Transaction txEx WITH (NOLOCK) ON jEx.TransactionID = txEx.TransactionID
						          WHERE jEx.OpeningBalanceID = ob.OpeningBalanceID OR txEx.OpeningBalanceID = ob.OpeningBalanceID
						      )
						) u
						ORDER BY
						    CAST(u.InvoiceDate AS DATE) ASC,
						    CASE
						        WHEN u.InvoiceType IN ('OPENING', 'Opening Balance') THEN 0
						        WHEN u.InvoiceType IN ('Journal Entry') THEN 1
						        ELSE 2
						    END ASC,
						    CASE
						        WHEN ISNULL(u.Debit, 0) > 0 THEN 0
						        ELSE 1
						    END ASC,
						    u.InvoiceNo ASC;";
                    var jParams = new SqlParameter[]
                    {
                        new SqlParameter("@SystemID", sysId),
                        new SqlParameter("@WarehouseID", whId),
                        new SqlParameter("@DomainID", domId),
                        new SqlParameter("@AccountNo", accountNo),
                        new SqlParameter("@FromDate", dtFrom.ToString("yyyy-MM-dd 00:00:00")),
                        new SqlParameter("@ToDate", dtTo.ToString("yyyy-MM-dd 23:59:59")),
                        new SqlParameter("@TxType", string.IsNullOrEmpty(txType) ? (object)DBNull.Value : txType),
                        new SqlParameter("@RefNo", string.IsNullOrEmpty(refNo) ? (object)DBNull.Value : refNo),
                        new SqlParameter("@CreatedBy", createdByUserId.HasValue ? (object)createdByUserId.Value : DBNull.Value),
                        new SqlParameter("@ClassID", filterClassId.HasValue ? (object)filterClassId.Value : DBNull.Value)
                    };

                    var rows = db.Database.SqlQuery<AccountInvoiceRow>(sql, jParams).ToList();

                    for (int i = 0; i < rows.Count; i++)
                    {
                        rows[i].No = i + 1;
                    }

                    return Json(new
                    {
                        success = true,
                        data = rows,
                        accountNo,
                        accountName,
                        toDate = dtTo.ToString("dd/MM/yyyy"),
                        recordCount = rows.Count
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
    public class AccountCategoryDto
    {
        public int AccountCategoryID { get; set; }
        public string AccountCategory { get; set; }
        public int AccountTypesID { get; set; }
        public string AccountTypes { get; set; }
        public int? OrderBy { get; set; }
    }

    public class OpeningBalanceResultDto
    {
        public decimal OpeningDebit { get; set; }
        public DateTime? OpeningBalanceDate { get; set; }
        public string MinDate { get; set; }
    }

    public class AccountDailyDelta
    {
        public int AccountID { get; set; }
        public DateTime TxDate { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }
}