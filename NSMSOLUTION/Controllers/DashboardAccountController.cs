using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class DashboardAccountController : Controller
    {
        public ActionResult Index(DateTime? fromDate = null, DateTime? toDate = null, string mode = null, int? year = null)
        {
            HTML html = new HTML();
            string connectionString = html.GetConnectionString();

            // ---------------- Account search filter (Account Code / Account Name) ----------------
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Account Code",
                SQL = "select top 10 AccountNo from dbo.TblChartOfAccount where lower(AccountNo) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountNo",
                RadioValue = EncryAndDecry.Encryp("ChartOfAccountID") + "," + EncryAndDecry.Encryp("AccountNo"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Account Name",
                SQL = "select top 10 AccountName from dbo.TblChartOfAccount where lower(AccountName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountName",
                RadioValue = EncryAndDecry.Encryp("ChartOfAccountID") + "," + EncryAndDecry.Encryp("AccountName"),
                ISAutoComplete = true,
            });

            // ---------------- Recent Transactions grid (your existing GH grid engine) ----------------
            string sql = @"SP_GET_DASHBOARD_RECENT_TRANSACTIONS";
            html.GH.GridProcedureName = sql;
            html.OrderBy = "[TxDate] DESC";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Description", FieldName = "Description", TextAlign = "left" },
                new GridField() { Header = "Category", FieldName = "CategoryName", TextAlign = "center" },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "TxDate", TextAlign = "center" },
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo", TextAlign = "center" },
            };
            html.ShowPaging = false;

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());

            // ---------------- Stat cards + charts (combined SP - not a grid, so plain ADO.NET) ----------------
            int currentYear = DateTime.Today.Year;
            bool isMonthly = false;
            int selYear = year ?? (fromDate.HasValue ? fromDate.Value.Year : currentYear);

            DateTime to;
            DateTime from;

            if (mode == "monthly")
            {
                isMonthly = true;
                to = DateTime.Today;
                from = to.AddDays(-30);
            }
            else if (year.HasValue)
            {
                from = new DateTime(selYear, 1, 1);
                to = selYear == currentYear ? DateTime.Today : new DateTime(selYear, 12, 31);
            }
            else if (fromDate.HasValue || toDate.HasValue)
            {
                to = toDate ?? DateTime.Today;
                from = fromDate ?? to.AddDays(-30);
            }
            else
            {
                // Default initial view: Last 30 Days (monthly)
                isMonthly = true;
                to = DateTime.Today;
                from = to.AddDays(-30);
            }

            DateTime prevTo = from.AddDays(-1);
            DateTime prevFrom = prevTo.AddDays(-(to - from).Days);

            var availableYears = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                availableYears.Add(currentYear - i);
            }

            DashboardSummary summary;
            List<TrendPoint> trend;
            List<QuarterPoint> quarterly;

            LoadDashboardData(connectionString,
                Convert.ToInt32(html.SystemID), Convert.ToInt32(html.WarehouseID), html.DomainID,
                from, to, prevFrom, prevTo, selYear,
                out summary, out trend, out quarterly);

            // Recent transactions: loaded via direct SP call (shown in the custom HTML table)
            var recent = LoadRecentTransactions(
                connectionString,
                Convert.ToInt32(html.SystemID),
                Convert.ToInt32(html.WarehouseID),
                html.DomainID);

            ViewBag.Summary = summary;
            ViewBag.TrendJson = Newtonsoft.Json.JsonConvert.SerializeObject(trend);
            ViewBag.QuarterlyJson = Newtonsoft.Json.JsonConvert.SerializeObject(quarterly);
            ViewBag.RecentTransactionsJson = Newtonsoft.Json.JsonConvert.SerializeObject(recent);
            ViewBag.FromDate = from;
            ViewBag.ToDate = to;
            ViewBag.SelectedYear = selYear;
            ViewBag.AvailableYears = availableYears;
            ViewBag.IsMonthly = isMonthly;

            return View();
        }

        // ------------------------------------------------------------
        // Calls SP_GetDashboardData and reads its 3 result sets in order:
        // Summary (1 row) -> Trend (N rows) -> Quarterly (4 rows)
        // ------------------------------------------------------------
        private void LoadDashboardData(
            string connectionString,
            int systemId, int warehouseId, string domainId,
            DateTime from, DateTime to, DateTime prevFrom, DateTime prevTo, int year,
            out DashboardSummary summary, out List<TrendPoint> trend, out List<QuarterPoint> quarterly)
        {
            summary = new DashboardSummary();
            trend = new List<TrendPoint>();
            quarterly = new List<QuarterPoint>();

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SP_GetDashboardData", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30          // 30 s max – fail fast rather than hang
            })
            {
                cmd.Parameters.AddWithValue("@SystemID", systemId);
                cmd.Parameters.AddWithValue("@WarehouseID", warehouseId);
                cmd.Parameters.AddWithValue("@DomainID", "%" + domainId + "%");
                cmd.Parameters.AddWithValue("@FromDate", from);
                cmd.Parameters.AddWithValue("@ToDate", to);
                cmd.Parameters.AddWithValue("@PrevFromDate", prevFrom);
                cmd.Parameters.AddWithValue("@PrevToDate", prevTo);
                cmd.Parameters.AddWithValue("@Year", year);

                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    // ---- Result set 1: Summary ----
                    if (r.Read())
                    {
                        summary = new DashboardSummary
                        {
                            CashBalance = r.GetDecimalSafe("CashBalance"),
                            PrevCashBalance = r.GetDecimalSafe("PrevCashBalance"),
                            TotalIncome = r.GetDecimalSafe("TotalIncome"),
                            PrevTotalIncome = r.GetDecimalSafe("PrevTotalIncome"),
                            TotalSpent = r.GetDecimalSafe("TotalSpent"),
                            PrevTotalSpent = r.GetDecimalSafe("PrevTotalSpent"),
                            TotalTransactions = r.GetInt32Safe("TotalTransactions"),
                            PrevTotalTransactions = r.GetInt32Safe("PrevTotalTransactions"),
                            PendingAmount = r.GetDecimalSafe("PendingAmount")
                        };
                    }

                    // ---- Result set 2: Trend ----
                    r.NextResult();
                    while (r.Read())
                    {
                        trend.Add(new TrendPoint
                        {
                            Label = Convert.ToDateTime(r["TxDate"]).ToString("MMM d"),
                            Income = r.GetDecimalSafe("Income"),
                            Expense = r.GetDecimalSafe("Expense")
                        });
                    }

                    // ---- Result set 3: Quarterly ----
                    r.NextResult();
                    while (r.Read())
                    {
                        quarterly.Add(new QuarterPoint
                        {
                            Label = "Q" + r.GetInt32Safe("QuarterNo"),
                            Income = r.GetDecimalSafe("Income"),
                            Expense = r.GetDecimalSafe("Expense")
                        });
                    }
                }
            }
        }

        private List<RecentTransaction> LoadRecentTransactions(string connectionString, int systemId, int warehouseId, string domainId)
        {
            var list = new List<RecentTransaction>();
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SP_GET_DASHBOARD_RECENT_TRANSACTIONS", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            })
            {
                cmd.Parameters.AddWithValue("@SystemID", systemId);
                cmd.Parameters.AddWithValue("@WarehouseID", warehouseId);
                cmd.Parameters.AddWithValue("@DomainID", "%" + domainId + "%");

                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new RecentTransaction
                        {
                            Description = r["Description"] == DBNull.Value ? "" : r["Description"].ToString(),
                            CategoryName = r["CategoryName"] == DBNull.Value ? "General" : r["CategoryName"].ToString(),
                            Amount = r.GetDecimalSafe("Amount"),
                            TxDate = Convert.ToDateTime(r["TxDate"]),
                            InvoiceNo = r["InvoiceNo"] == DBNull.Value ? "" : r["InvoiceNo"].ToString()
                        });
                    }
                }
            }
            return list;
        }
    }

    public class RecentTransaction
    {
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }
        public DateTime TxDate { get; set; }
        public string InvoiceNo { get; set; }
    }

    // ---------------- Small POCOs for the stat cards / charts ----------------

    public class DashboardSummary
    {
        public decimal CashBalance { get; set; }
        public decimal PrevCashBalance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal PrevTotalIncome { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal PrevTotalSpent { get; set; }
        public int TotalTransactions { get; set; }
        public int PrevTotalTransactions { get; set; }
        public decimal PendingAmount { get; set; }

        public decimal NetIncome => TotalIncome - TotalSpent;
        public decimal PrevNetIncome => PrevTotalIncome - PrevTotalSpent;
        public decimal CashDelta => CashBalance - PrevCashBalance;
        public decimal IncomeDelta => TotalIncome - PrevTotalIncome;
        public decimal SpentDelta => TotalSpent - PrevTotalSpent;
        public decimal NetIncomeDelta => NetIncome - PrevNetIncome;
        public int TransactionsDelta => TotalTransactions - PrevTotalTransactions;
    }

    public class TrendPoint
    {
        public string Label { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }

    public class QuarterPoint
    {
        public string Label { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }

    // ---------------- Reader helpers (avoid DBNull crashes) ----------------
    internal static class DataReaderExtensions
    {
        public static decimal GetDecimalSafe(this IDataRecord r, string column)
        {
            var v = r[column];
            return v == DBNull.Value ? 0m : Convert.ToDecimal(v);
        }

        public static int GetInt32Safe(this IDataRecord r, string column)
        {
            var v = r[column];
            return v == DBNull.Value ? 0 : Convert.ToInt32(v);
        }
    }
}