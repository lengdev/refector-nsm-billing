using System;
using System.Collections.Generic;

namespace NSMSOLUTION.Models.ViewModels
{
    /// <summary>
    /// Raw flat row from SQL query.
    /// </summary>
    public class BalanceSheetFlatNode
    {
        public int ChartOfAccountID { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public int ParentAccountID { get; set; }
        public int AccountCategoryID { get; set; }
        public string AccountCategory { get; set; }
        public string AccountTypes { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Balance { get; set; }
    }

    /// <summary>
    /// Definition of a period column (Day, Week, Month, Quarter, Year, or Default).
    /// </summary>
    public class BalanceSheetColumnDef
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string SubLabel { get; set; } = "Amount";
        public string DateStr { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// A single account node in the parent-child tree.
    /// </summary>
    public class BalanceSheetNode
    {
        public int ChartOfAccountID { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public int ParentAccountID { get; set; }
        public int AccountCategoryID { get; set; }
        public string AccountCategory { get; set; }
        public string AccountTypes { get; set; }
        public decimal Balance { get; set; }
        public List<decimal> ColBalances { get; set; } = new List<decimal>();
        public Dictionary<string, decimal> PeriodBalances { get; set; } = new Dictionary<string, decimal>();
        public bool IsLeaf { get; set; }
        public int Level { get; set; }
        public List<BalanceSheetNode> Children { get; set; } = new List<BalanceSheetNode>();
    }

    /// <summary>
    /// Level 3: A category (e.g. "Bank", "Account Receivable") with its account tree.
    /// </summary>
    public class BalanceSheetCategory
    {
        public int AccountCategoryID { get; set; }
        public string CategoryName { get; set; }
        public List<BalanceSheetNode> Accounts { get; set; } = new List<BalanceSheetNode>();
        public decimal Total { get; set; }
        public List<decimal> ColTotals { get; set; } = new List<decimal>();
        public Dictionary<string, decimal> PeriodTotals { get; set; } = new Dictionary<string, decimal>();
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Level 2: A category group (e.g. "Current Assets", "Non Current Assets").
    /// Groups multiple AccountCategories together under one header.
    /// </summary>
    public class BalanceSheetCategoryGroup
    {
        public string GroupName { get; set; }          // e.g. "Current Assets"
        public List<BalanceSheetCategory> Categories { get; set; } = new List<BalanceSheetCategory>();
        public decimal Total { get; set; }
        public List<decimal> ColTotals { get; set; } = new List<decimal>();
        public Dictionary<string, decimal> PeriodTotals { get; set; } = new Dictionary<string, decimal>();
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Level 1: Top-level section (ASSETS, LIABILITIES, EQUITY).
    /// </summary>
    public class BalanceSheetSection
    {
        public string SectionName { get; set; }
        public List<BalanceSheetCategoryGroup> Groups { get; set; } = new List<BalanceSheetCategoryGroup>();
        public decimal Total { get; set; }
        public List<decimal> ColTotals { get; set; } = new List<decimal>();
        public Dictionary<string, decimal> PeriodTotals { get; set; } = new Dictionary<string, decimal>();
    }

    /// <summary>
    /// Full balance sheet response returned to the view.
    /// </summary>
    public class BalanceSheetResponse
    {
        public BalanceSheetSection Assets { get; set; } = new BalanceSheetSection { SectionName = "ASSETS" };
        public BalanceSheetSection Liabilities { get; set; } = new BalanceSheetSection { SectionName = "LIABILITIES" };
        public BalanceSheetSection Equity { get; set; } = new BalanceSheetSection { SectionName = "EQUITY" };
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public List<decimal> TotalAssetsByPeriod { get; set; } = new List<decimal>();
        public List<decimal> TotalLiabilitiesByPeriod { get; set; } = new List<decimal>();
        public List<decimal> TotalEquityByPeriod { get; set; } = new List<decimal>();
        public List<decimal> TotalLiabAndEquityByPeriod { get; set; } = new List<decimal>();
        public List<BalanceSheetColumnDef> Columns { get; set; } = new List<BalanceSheetColumnDef>();
        public string DisplayColumns { get; set; } = "default";
    }

    /// <summary>
    /// A single journal entry row returned by the invoice drill-down query.
    /// Property names match the SQL column aliases exactly so EF can map them.
    /// </summary>
    public class AccountInvoiceRow
    {
        public int No { get; set; }           // CAST to INT in SQL
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceType { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string AccountTypes { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Description { get; set; }
    }

    // ── Advance Filter dropdown projection types ──────────────────

    public class AccountFilterItem
    {
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string AccountCategory { get; set; }
    }

    public class LocationFilterItem
    {
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
    }

    public class ClassFilterItem
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
    }

    public class UserFilterItem
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
    }
}
