using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Report.AccountingReport
{
    public class TrialBalanceController : Controller
    {
        [Authorize(Roles = "Report Trial Balance")]
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
                    using (var con = new SqlConnection(connStr))
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
                        using (var cmd = new SqlCommand(sqlComp, con))
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

                ViewBag.SystemID = sysId;
                ViewBag.WarehouseID = whId;

                ViewBag.CompanyNameKh = compKh;
                ViewBag.CompanyNameEn = compEn;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Trial Balance report: failed to load company header: {0}", ex.Message);
            }

            return View("~/Views/TrialBalance/index.cshtml");
        }

        [HttpPost]
        public JsonResult Read(int? SystemID, int? WarehouseID, DateTime? toDate, string subOfAccount = "", decimal? fromBalance = null, decimal? toBalance = null, string createdBy = "", string sortBy = "", string orderBy = "asc")
        {
            try
            {
                HTML html = new HTML();
                string connStr = html.GetConnectionString();
                var data = new List<TrialBalanceRow>();

                int sysId = SystemID ?? 0;
                if (sysId == 0 && !string.IsNullOrEmpty(Request["SystemID"])) int.TryParse(Request["SystemID"], out sysId);
                if (sysId == 0 && !string.IsNullOrEmpty(html.SystemID)) int.TryParse(html.SystemID, out sysId);

                int whId = WarehouseID ?? 0;
                if (whId == 0 && !string.IsNullOrEmpty(Request["WarehouseID"])) int.TryParse(Request["WarehouseID"], out whId);
                if (whId == 0 && !string.IsNullOrEmpty(html.WarehouseID)) int.TryParse(html.WarehouseID, out whId);

                string domId = html.DomainID ?? "";

                bool isMultiAccount = !string.IsNullOrEmpty(subOfAccount) && subOfAccount.Contains(",");
                bool isMultiCreatedBy = !string.IsNullOrEmpty(createdBy) && createdBy.Contains(",");

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = new SqlCommand("dbo.sp_GetTrialBalance", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@SystemID", sysId > 0 ? (object)sysId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@WarehouseID", whId > 0 ? (object)whId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@DomainID", !string.IsNullOrEmpty(domId) ? (object)domId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToDate", toDate.HasValue ? (object)toDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@SubOfAccount", (!string.IsNullOrEmpty(subOfAccount) && !isMultiAccount) ? (object)subOfAccount : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromBalance", fromBalance.HasValue ? (object)fromBalance.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToBalance", toBalance.HasValue ? (object)toBalance.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", (!string.IsNullOrEmpty(createdBy) && !isMultiCreatedBy) ? (object)createdBy : DBNull.Value);
                        cmd.Parameters.AddWithValue("@SortBy", !string.IsNullOrEmpty(sortBy) ? (object)sortBy : DBNull.Value);
                        cmd.Parameters.AddWithValue("@OrderBy", !string.IsNullOrEmpty(orderBy) ? orderBy : "asc");

                        con.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                decimal totalDebit = Convert.ToDecimal(dr["Debit"]);
                                decimal totalCredit = Convert.ToDecimal(dr["Credit"]);

                                data.Add(new TrialBalanceRow
                                {
                                    AccountNo = dr["AccountNo"].ToString(),
                                    AccountType = dr["AccountType"].ToString(),
                                    AccountCategory = dr["AccountCategory"].ToString(),
                                    AccountName = dr["AccountName"].ToString(),
                                    Status = dr["Status"].ToString(),
                                    NormalSide = dr["NormalSide"].ToString(),
                                    Debit = totalDebit,
                                    Credit = totalCredit
                                });
                            }
                        }
                    }
                }

                if (isMultiAccount)
                {
                    var accTokens = subOfAccount.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim().ToLower())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();

                    if (accTokens.Count > 0)
                    {
                        data = data.FindAll(d => accTokens.Exists(t =>
                            (!string.IsNullOrEmpty(d.AccountNo) && d.AccountNo.ToLower().Contains(t)) ||
                            (!string.IsNullOrEmpty(d.AccountName) && d.AccountName.ToLower().Contains(t))));
                    }
                }

                string compKh = "";
                string compEn = "";

                int defaultClassId;
                switch (sysId)
                {
                    case 13: defaultClassId = 5; break;
                    case 28: defaultClassId = 14; break;
                    case 25: defaultClassId = 10; break;
                    case 27: defaultClassId = 12; break;
                    default: defaultClassId = 0; break;
                }

                if (sysId > 0)
                {
                    try
                    {
                        using (var con = new SqlConnection(connStr))
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
                            using (var cmdComp = new SqlCommand(sqlComp, con))
                            {
                                cmdComp.Parameters.AddWithValue("@DefaultClassID", defaultClassId);
                                cmdComp.Parameters.AddWithValue("@SysID", sysId);
                                cmdComp.Parameters.AddWithValue("@WarehouseID", whId);
                                using (var drComp = cmdComp.ExecuteReader())
                                {
                                    if (drComp.Read())
                                    {
                                        for (int i = 0; i < drComp.FieldCount; i++)
                                        {
                                            string name = drComp.GetName(i);
                                            if (string.Equals(name, "CompanyNameKH", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "KHName", StringComparison.OrdinalIgnoreCase))
                                                compKh = drComp[i] != DBNull.Value ? drComp[i].ToString().Trim() : compKh;
                                            if (string.Equals(name, "FullName", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "CompanyName", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "CompanyNameEN", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "ClassName", StringComparison.OrdinalIgnoreCase))
                                                compEn = drComp[i] != DBNull.Value ? drComp[i].ToString().Trim() : compEn;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }

                if (!string.IsNullOrWhiteSpace(compKh))
                {
                    string clean = compKh.Trim(' ', '\t', '\r', '\n', '\u200B', '\u00A0');
                    if (!clean.StartsWith("ក្រុមហ៊ុន")) compKh = "ក្រុមហ៊ុន " + clean;
                    else compKh = clean;
                }

                return Json(new { Success = true, Data = data, Company = new { CompanyNameKh = compKh, CompanyNameEn = compEn } }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

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
                var users = new List<object>();

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    string sqlAccounts = @"
SELECT ChartOfAccountID, AccountNo, AccountName 
FROM dbo.TblChartOfAccount 
WHERE (@SysID = 0 OR SystemID = @SysID) AND (@WhID = 0 OR WarehouseID = @WhID) 
ORDER BY AccountNo, AccountName";
                    using (SqlCommand cmd = new SqlCommand(sqlAccounts, con))
                    {
                        cmd.Parameters.AddWithValue("@SysID", sysId);
                        cmd.Parameters.AddWithValue("@WhID", whId);
                        con.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                accounts.Add(new
                                {
                                    ChartOfAccountID = Convert.ToInt32(dr["ChartOfAccountID"]),
                                    AccountNo = dr["AccountNo"].ToString(),
                                    AccountName = dr["AccountName"].ToString()
                                });
                            }
                        }
                        con.Close();
                    }

                    string sqlUsers = @"
SELECT UserLoginID, FullName, UserName 
FROM dbo.TblUserLogin 
WHERE StatusID = 3 
ORDER BY FullName, UserName";
                    using (SqlCommand cmd = new SqlCommand(sqlUsers, con))
                    {
                        con.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                users.Add(new
                                {
                                    UserLoginID = Convert.ToInt32(dr["UserLoginID"]),
                                    FullName = dr["FullName"].ToString(),
                                    UserName = dr["UserName"].ToString()
                                });
                            }
                        }
                    }
                }

                return Json(new { success = true, accounts = accounts, users = users }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetClassOptions(int? SystemID = null, int? WarehouseID = null)
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

                int defaultClassId;
                switch (sysId)
                {
                    case 13: defaultClassId = 5; break;
                    case 28: defaultClassId = 14; break;
                    case 25: defaultClassId = 10; break;
                    case 27: defaultClassId = 12; break;
                    default: defaultClassId = 0; break;
                }

                var classes = new List<object>();
                const string sql = @"
SELECT ClassID, ClassName, FullName, KHName
FROM dbo.TblClass WITH (NOLOCK)
WHERE (@SysID = 0 OR SystemID = @SysID OR SystemID IS NULL)
  AND (@WhID = 0 OR WarehouseID = @WhID OR WarehouseID IS NULL)
  AND (@DomID = '' OR DomainID LIKE '%' + @DomID + '%' OR DomainID IS NULL)
ORDER BY
    CASE WHEN @DefaultClassID > 0 AND ClassID = @DefaultClassID THEN 0 ELSE 1 END,
    CASE WHEN StatusID = 40 THEN 0 ELSE 1 END,
    CASE WHEN @WhID > 0 AND WarehouseID = @WhID THEN 0 ELSE 1 END,
    ClassID";

                using (SqlConnection con = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@SysID", sysId);
                    cmd.Parameters.AddWithValue("@WhID", whId);
                    cmd.Parameters.AddWithValue("@DomID", html.DomainID ?? "");
                    cmd.Parameters.AddWithValue("@DefaultClassID", defaultClassId);
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            classes.Add(new
                            {
                                ClassID = dr["ClassID"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ClassID"]),
                                ClassName = dr["ClassName"] == DBNull.Value ? "" : dr["ClassName"].ToString(),
                                FullName = dr["FullName"] == DBNull.Value ? "" : dr["FullName"].ToString(),
                                KHName = dr["KHName"] == DBNull.Value ? "" : dr["KHName"].ToString()
                            });
                        }
                    }
                }

                return Json(new { success = true, classes = classes, defaultClassId = defaultClassId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Trial Balance GetClassOptions error: {0}", ex.Message);
                return Json(new { success = false, message = "Could not load Trial Balance classes." }, JsonRequestBehavior.AllowGet);
            }
        }

        
        public class TrialBalanceRow
        {
            public string AccountNo { get; set; }
            public string AccountType { get; set; }
            public string AccountCategory { get; set; }
            public string AccountName { get; set; }
            public string Status { get; set; }
            public string NormalSide { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
        }
    }
}
