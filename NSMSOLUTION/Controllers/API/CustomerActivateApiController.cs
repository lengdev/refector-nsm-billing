using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using NSMSOLUTION.Helper_Code.Common;
using System.Text.RegularExpressions;
using MVC;

namespace NSMSOLUTION.Controllers.API
{
    /// <summary>

    [EnableCors(origins: "http://192.168.200.183:3001", headers: "*", methods: "*")]
    [Authorize]
    [RoutePrefix("api/CustomerActivate")]
    public class CustomerActivateApiController : ApiController
    {
        private const int STATUS_ACTIVE = 40;
        private const int STATUS_BLOCKED = 41;

        /// <summary>
        /// Select and filter activated customers with CID, CustomerName, PhoneNumber, SID, and Package details    
        /// </summary>
        [HttpGet]
        [Route("")]
        [Route("GetList")]
        public async Task<IHttpActionResult> GetList([FromUri] ActivateCustomerFilterDTO filter)
        {
            if (filter == null) filter = new ActivateCustomerFilterDTO();
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1 || filter.PageSize > 200) filter.PageSize = 20;

            try
            {
                using (var ctx = new DBContext())
                {
                    var today = DateTime.Today;

                    // Join TblActivateCustomer with TblCustomer and TblPackageDetail
                    // to fetch CID, CustomerName, PhoneNumber, SID, and Package info.
                    var query = from ac in ctx.TblActivateCustomer
                                join c in ctx.TblCustomer on ac.CustomerID equals c.CustomerID
                                join pd in ctx.TblPackageDetail on ac.PackageDetailID equals pd.PackageDetailID into pdGroup
                                from pd in pdGroup.DefaultIfEmpty()
                                select new
                                {
                                    ac.ActivateCustomerID,
                                    CustomerID = c.CustomerID,
                                    CID = c.CustomerCode,
                                    CustomerName = string.IsNullOrEmpty(c.CustomerName) ? c.CustomerNameLatin : c.CustomerName,
                                    CustomerNameLatin = c.CustomerNameLatin,
                                    CustomerMobile = c.CustomerMobile,
                                    CustomerContactMobileNumber = c.CustomerContactMobileNumber,
                                    ac.SID,
                                    ac.PackageDetailID,
                                    PackageName = pd.PackageName,
                                    Bandwidth = (int?)pd.Bandwidth,
                                    ac.PaymentDueDate,
                                    ac.EndDate,
                                    ac.ConnectivityStatus,
                                    ac.ConnectivityStatusName
                                };

                    // 1. General search across CID, CustomerName, Phone, SID, and Package (ignores spaces and special separators in phone/name)
                    if (!string.IsNullOrWhiteSpace(filter.Search))
                    {
                        string s = filter.Search.Trim().ToLower();
                        string sClean = s.Replace(" ", "").Replace("\u200B", "").Replace("-", "").Replace(".", "");
                        bool hasClean = !string.IsNullOrEmpty(sClean);

                        query = query.Where(x => (x.CID != null && (x.CID.ToLower().Contains(s) || (hasClean && x.CID.Replace(" ", "").ToLower().Contains(sClean))))
                                              || (x.CustomerName != null && (x.CustomerName.ToLower().Contains(s) || (hasClean && x.CustomerName.Replace(" ", "").Replace("\u200B", "").ToLower().Contains(sClean))))
                                              || (x.CustomerNameLatin != null && (x.CustomerNameLatin.ToLower().Contains(s) || (hasClean && x.CustomerNameLatin.Replace(" ", "").Replace("\u200B", "").ToLower().Contains(sClean))))
                                              || (x.CustomerMobile != null && (x.CustomerMobile.ToLower().Contains(s) || (hasClean && x.CustomerMobile.Replace(" ", "").Replace("\u200B", "").Replace("-", "").Replace(".", "").ToLower().Contains(sClean))))
                                              || (x.CustomerContactMobileNumber != null && (x.CustomerContactMobileNumber.ToLower().Contains(s) || (hasClean && x.CustomerContactMobileNumber.Replace(" ", "").Replace("\u200B", "").Replace("-", "").Replace(".", "").ToLower().Contains(sClean))))
                                              || (x.SID != null && (x.SID.ToLower().Contains(s) || (hasClean && x.SID.Replace(" ", "").ToLower().Contains(sClean))))
                                              || (x.PackageName != null && x.PackageName.ToLower().Contains(s)));
                    }

                    // 2. Filter specifically by CID (CustomerCode)
                    if (!string.IsNullOrWhiteSpace(filter.Cid))
                    {
                        string cid = filter.Cid.Trim().ToLower();
                        string cidClean = cid.Replace(" ", "");
                        bool hasCidClean = !string.IsNullOrEmpty(cidClean);
                        query = query.Where(x => x.CID != null && (x.CID.ToLower().Contains(cid) || (hasCidClean && x.CID.Replace(" ", "").ToLower().Contains(cidClean))));
                    }

                    // 3. Filter specifically by Customer Name
                    if (!string.IsNullOrWhiteSpace(filter.CustomerName))
                    {
                        string cname = filter.CustomerName.Trim().ToLower();
                        string cnameClean = cname.Replace(" ", "").Replace("\u200B", "");
                        bool hasCnameClean = !string.IsNullOrEmpty(cnameClean);
                        query = query.Where(x => (x.CustomerName != null && (x.CustomerName.ToLower().Contains(cname) || (hasCnameClean && x.CustomerName.Replace(" ", "").Replace("\u200B", "").ToLower().Contains(cnameClean))))
                                              || (x.CustomerNameLatin != null && (x.CustomerNameLatin.ToLower().Contains(cname) || (hasCnameClean && x.CustomerNameLatin.Replace(" ", "").Replace("\u200B", "").ToLower().Contains(cnameClean)))));
                    }

                    // 4. Filter specifically by SID
                    string sidFilter = !string.IsNullOrWhiteSpace(filter.Sid) ? filter.Sid : null;
                    if (!string.IsNullOrWhiteSpace(sidFilter))
                    {
                        string sid = sidFilter.Trim().ToLower();
                        string sidClean = sid.Replace(" ", "");
                        bool hasSidClean = !string.IsNullOrEmpty(sidClean);
                        query = query.Where(x => x.SID != null && (x.SID.ToLower().Contains(sid) || (hasSidClean && x.SID.Replace(" ", "").ToLower().Contains(sidClean))));
                    }

                    // 5. Filter specifically by Phone Number (ignores spaces, zero-width spaces, dashes, dots)
                    string phoneFilter = !string.IsNullOrWhiteSpace(filter.PhoneNumber) ? filter.PhoneNumber : filter.Phone;
                    if (!string.IsNullOrWhiteSpace(phoneFilter))
                    {
                        string phone = phoneFilter.Trim().ToLower();
                        string phoneClean = phone.Replace(" ", "").Replace("\u200B", "").Replace("-", "").Replace(".", "");
                        bool hasPhoneClean = !string.IsNullOrEmpty(phoneClean);
                        query = query.Where(x => (x.CustomerMobile != null && (x.CustomerMobile.ToLower().Contains(phone) || (hasPhoneClean && x.CustomerMobile.Replace(" ", "").Replace("\u200B", "").Replace("-", "").Replace(".", "").ToLower().Contains(phoneClean))))
                                              || (x.CustomerContactMobileNumber != null && (x.CustomerContactMobileNumber.ToLower().Contains(phone) || (hasPhoneClean && x.CustomerContactMobileNumber.Replace(" ", "").Replace("\u200B", "").Replace("-", "").Replace(".", "").ToLower().Contains(phoneClean)))));
                    }

                    // 6. Filter specifically by Package
                    string pkgFilter = !string.IsNullOrWhiteSpace(filter.PackageName) ? filter.PackageName : filter.Package;
                    if (!string.IsNullOrWhiteSpace(pkgFilter))
                    {
                        string pkg = pkgFilter.Trim().ToLower();
                        query = query.Where(x => x.PackageName != null && x.PackageName.ToLower().Contains(pkg));
                    }

                    // 7. Filter by Blocked status (ConnectivityStatus == 41)
                    if (filter.IsBlockedOnly.HasValue)
                    {
                        query = filter.IsBlockedOnly.Value
                            ? query.Where(x => x.ConnectivityStatus == STATUS_BLOCKED)
                            : query.Where(x => x.ConnectivityStatus != STATUS_BLOCKED);
                    }
                    else if (filter.ConnectivityStatus.HasValue)
                    {
                        query = query.Where(x => x.ConnectivityStatus == filter.ConnectivityStatus.Value);
                    }

                    // 8. Filter by Due Customers (PaymentDueDate < Today OR EndDate < Today)
                    if (filter.IsDueOnly.HasValue && filter.IsDueOnly.Value)
                    {
                        query = query.Where(x => (x.PaymentDueDate.HasValue && x.PaymentDueDate.Value < today)
                                              || (x.EndDate.HasValue && x.EndDate.Value < today));
                    }

                    int totalRecords = await query.CountAsync();

                    var rows = await query
                        .OrderByDescending(x => x.ActivateCustomerID)
                        .Skip((filter.Page - 1) * filter.PageSize)
                        .Take(filter.PageSize)
                        .ToListAsync();

                    var items = rows.Select(x =>
                    {
                        string phone = !string.IsNullOrWhiteSpace(x.CustomerMobile)
    ? NormalizePhone(x.CustomerMobile)
    : NormalizePhone(x.CustomerContactMobileNumber);
                        //string phone = !string.IsNullOrWhiteSpace(x.CustomerMobile)
                        //    ? x.CustomerMobile.Trim()
                        //    : (!string.IsNullOrWhiteSpace(x.CustomerContactMobileNumber) ? x.CustomerContactMobileNumber.Trim() : "");
                        string packageName = x.PackageName ?? "";

                        return new ActivateCustomerItemDTO
                        {
                            ActivateCustomerID = x.ActivateCustomerID,
                            CustomerID = x.CustomerID,
                            CID = x.CID,
                            CustomerName = x.CustomerName,
                            PhoneNumber = phone,        
                            SID = x.SID,
                            PackageDetailID = x.PackageDetailID,
                            PackageName = packageName,
                            Package = packageName,
                            Bandwidth = x.Bandwidth,
                            PaymentDueDate = x.PaymentDueDate,
                            EndDate = x.EndDate,
                            ConnectivityStatus = x.ConnectivityStatus,
                            StatusName = x.ConnectivityStatus == STATUS_BLOCKED
                                ? "Blocked"
                                : (x.ConnectivityStatus == STATUS_ACTIVE ? "Active" : x.ConnectivityStatusName),
                            IsBlocked = x.ConnectivityStatus == STATUS_BLOCKED
                        };
                    }).ToList();

                    return Ok(new
                    {
                        success = true,
                        total = totalRecords,
                        page = filter.Page,
                        pageSize = filter.PageSize,
                        data = items
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Fetch detailed customer activation record by ActivateCustomerID, SID, or CID.
        /// </summary>
        [HttpGet]
        [Route("GetDetail")]
        [Route("{activateCustomerId:int}")]
        public async Task<IHttpActionResult> GetDetail([FromUri] int? activateCustomerId = null, [FromUri] string sid = null, [FromUri] string cid = null)
        {
            if (!activateCustomerId.HasValue && string.IsNullOrWhiteSpace(sid) && string.IsNullOrWhiteSpace(cid))
            {
                return BadRequest("Please provide at least activateCustomerId, sid, or cid.");
            }

            try
            {
                using (var ctx = new DBContext())
                {
                    var query = from ac in ctx.TblActivateCustomer
                                join c in ctx.TblCustomer on ac.CustomerID equals c.CustomerID
                                join pd in ctx.TblPackageDetail on ac.PackageDetailID equals pd.PackageDetailID into pdGroup
                                from pd in pdGroup.DefaultIfEmpty()
                                select new
                                {
                                    ac.ActivateCustomerID,
                                    CustomerID = c.CustomerID,
                                    CID = c.CustomerCode,
                                    CustomerName = string.IsNullOrEmpty(c.CustomerName) ? c.CustomerNameLatin : c.CustomerName,
                                    CustomerMobile = c.CustomerMobile,
                                    CustomerContactMobileNumber = c.CustomerContactMobileNumber,
                                    ac.SID,
                                    ac.PackageDetailID,
                                    PackageName = pd.PackageName,
                                    Bandwidth = (int?)pd.Bandwidth,
                                    ac.PaymentDueDate,
                                    ac.EndDate,
                                    ac.ConnectivityStatus,
                                    ac.ConnectivityStatusName,
                                    ac.PPPoEUserName,
                                    ac.ConnectionType,
                                    ac.IPAddress,
                                    ac.VLanName,
                                    ac.RouterID,
                                    ac.ActivateDate
                                };

                    if (activateCustomerId.HasValue && activateCustomerId.Value > 0)
                    {
                        query = query.Where(x => x.ActivateCustomerID == activateCustomerId.Value);
                    }
                    else if (!string.IsNullOrWhiteSpace(sid))
                    {
                        string s = sid.Trim().ToLower();
                        string sClean = s.Replace(" ", "");
                        query = query.Where(x => x.SID != null && (x.SID.ToLower() == s || x.SID.Replace(" ", "").ToLower() == sClean));
                    }
                    else if (!string.IsNullOrWhiteSpace(cid))
                    {
                        string c = cid.Trim().ToLower();
                        string cClean = c.Replace(" ", "");
                        query = query.Where(x => x.CID != null && (x.CID.ToLower() == c || x.CID.Replace(" ", "").ToLower() == cClean));
                    }

                    var row = await query.OrderByDescending(x => x.ActivateCustomerID).FirstOrDefaultAsync();
                    if (row == null)
                    {
                        return NotFound();
                    }

                    string phone = !string.IsNullOrWhiteSpace(row.CustomerMobile)
     ? NormalizePhone(row.CustomerMobile)
     : NormalizePhone(row.CustomerContactMobileNumber);
                    string packageName = row.PackageName ?? "";

                    var detail = new ActivateCustomerDetailDTO
                    {
                        ActivateCustomerID = row.ActivateCustomerID,
                        CustomerID = row.CustomerID,
                        CID = row.CID,
                        CustomerName = row.CustomerName,
                        PhoneNumber = phone,
                        SID = row.SID,
                        PackageDetailID = row.PackageDetailID,
                        PackageName = packageName,
                        Package = packageName,
                        Bandwidth = row.Bandwidth,
                        PaymentDueDate = row.PaymentDueDate,
                        EndDate = row.EndDate,
                        ConnectivityStatus = row.ConnectivityStatus,
                        StatusName = row.ConnectivityStatus == STATUS_BLOCKED
                            ? "Blocked"
                            : (row.ConnectivityStatus == STATUS_ACTIVE ? "Active" : row.ConnectivityStatusName),
                        IsBlocked = row.ConnectivityStatus == STATUS_BLOCKED,
                        PPPoEUserName = row.PPPoEUserName,
                        ConnectionType = row.ConnectionType,
                        IPAddress = row.IPAddress,
                        VLanName = row.VLanName,
                        RouterID = row.RouterID,
                        ActivateDate = row.ActivateDate
                    };

                    return Ok(new { success = true, data = detail });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Shortcut to fetch customer activation by SID to verify correction.
        /// GET: api/CustomerActivate/GetBySid?sid=ISP000335
        /// </summary>
        [HttpGet]
        [Route("GetBySid")]
        public async Task<IHttpActionResult> GetBySid([FromUri] string sid)
        {
            return await GetDetail(null, sid, null);
        }

        /// <summary>
        /// Block or Unblock customer(s) - supports both single and bulk operations.
        /// Sets ConnectivityStatus to 41 (Locked/Blocked) or 40 (Active) and syncs associated invoice records.
        /// POST: api/CustomerActivate/Block
        /// </summary>
        [HttpPost]
        [Route("Block")]
        public async Task<IHttpActionResult> Block([FromBody] BlockCustomerRequestDTO req)
        {
            if (req == null)
                return BadRequest("Request body cannot be null.");

            var targetIds = new List<int>();
            if (req.ActivateCustomerIDs != null && req.ActivateCustomerIDs.Count > 0)
            {
                targetIds.AddRange(req.ActivateCustomerIDs.Where(id => id > 0).Distinct());
            }
            else if (req.ActivateCustomerID.HasValue && req.ActivateCustomerID.Value > 0)
            {
                targetIds.Add(req.ActivateCustomerID.Value);
            }

            if (targetIds.Count == 0)
                return BadRequest("At least one valid ActivateCustomerID must be provided.");

            try
            {
                using (var ctx = new DBContext())
                {
                    var records = await ctx.TblActivateCustomer
                        .Where(x => targetIds.Contains(x.ActivateCustomerID))
                        .ToListAsync();

                    if (records.Count == 0)
                        return NotFound();

                    int newStatus = req.Block ? STATUS_BLOCKED : STATUS_ACTIVE;
                    string statusName = req.Block ? "Locked" : "Active";
                    string currentUserName = User?.Identity?.Name;
                    if (string.IsNullOrWhiteSpace(currentUserName)) currentUserName = "System";
                    DateTime now = DateTime.Now;

                    foreach (var record in records)
                    {
                        record.ConnectivityStatus = newStatus;
                        record.ConnectivityStatusName = statusName;
                        record.LockedBy = currentUserName;
                        record.LockedDate = now;

                        if (!string.IsNullOrWhiteSpace(req.Reason))
                        {
                            record.Comment = req.Reason;
                        }
                    }

                    // Synchronize with TblCustomerBillingInvoice if present
                    var billingInvoices = await ctx.TblCustomerBillingInvoice
                        .Where(bi => bi.ActivateCustomerID.HasValue && targetIds.Contains(bi.ActivateCustomerID.Value))
                        .ToListAsync();

                    foreach (var bi in billingInvoices)
                    {
                        bi.ConnectivityStatus = newStatus;
                        bi.ConnectivityStatusName = statusName;
                    }

                    // Synchronize with TblCustomerInvoice if present
                    var customerInvoices = await ctx.TblCustomerInvoice
                        .Where(ci => ci.ActivateCustomerID.HasValue && targetIds.Contains(ci.ActivateCustomerID.Value))
                        .ToListAsync();

                    foreach (var ci in customerInvoices)
                    {
                        ci.ConnectivityStatus = newStatus;
                        ci.ConnectivityStatusName = statusName;
                    }

                    await ctx.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        count = records.Count,
                        isBlocked = req.Block,
                        status = newStatus,
                        message = req.Block
                            ? $"Successfully blocked {records.Count} customer connection(s)."
                            : $"Successfully unblocked {records.Count} customer connection(s)."
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Summary count of Active, Blocked, and Due customers for dashboard / quick badges.
        /// GET: api/CustomerActivate/GetDueSummary
        /// </summary>
        [HttpGet]
        [Route("GetDueSummary")]
        public async Task<IHttpActionResult> GetDueSummary()
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    var today = DateTime.Today;

                    int totalRecords = await ctx.TblActivateCustomer.CountAsync();
                    int totalBlocked = await ctx.TblActivateCustomer.CountAsync(x => x.ConnectivityStatus == STATUS_BLOCKED);
                    int totalActive = await ctx.TblActivateCustomer.CountAsync(x => x.ConnectivityStatus == STATUS_ACTIVE);
                    int totalDue = await ctx.TblActivateCustomer.CountAsync(x =>
                        (x.PaymentDueDate.HasValue && x.PaymentDueDate.Value < today) ||
                        (x.EndDate.HasValue && x.EndDate.Value < today));

                    var summary = new CustomerActivateSummaryDTO
                    {
                        TotalRecords = totalRecords,
                        TotalActive = totalActive,
                        TotalBlocked = totalBlocked,
                        TotalDue = totalDue
                    };

                    return Ok(new { success = true, data = summary });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        // Inside CustomerActivateApiController
        private static string NormalizePhone(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            // removes all spaces/tabs; add "-" and "." to the pattern if you want those removed too: @"[\s\-\.]+"
            return Regex.Replace(value, @"[\s\u200B\-\.]+", "");
        }

        /// <summary>
        /// GET Technical department received items. Filter by srNo, itemCode, itemName, snNumber.
        /// GET: api/ReceivedItems
        /// GET: api/CustomerActivate/ReceivedItems
        /// </summary>
        [HttpGet]
        [Authorize]
        //[AllowAnonymous]
        [Route("ReceivedItems")]
        [Route("~/api/ReceivedItems")]
        public async Task<IHttpActionResult> GetReceivedItems([FromUri] ReceivedItemsFilterDTO filter)
        {
            try
            {
                if (filter == null) filter = new ReceivedItemsFilterDTO();
                var result = await QueryReceivedItemsAsync(filter);
                return Ok(new
                {
                    success = true,
                    total = result.Total,
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static SqlParameter[] CloneParameters(IEnumerable<SqlParameter> source)
        {
            if (source == null) return new SqlParameter[0];
            return source.Select(p => new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value)).ToArray();
        }

        private static bool? ToNullableBool(object val)
        {
            if (val == null || val == DBNull.Value) return null;
            if (val is bool b) return b;
            if (int.TryParse(val.ToString(), out int i)) return i == 1;
            if (bool.TryParse(val.ToString(), out bool pb)) return pb;
            return null;
        }

        private static int? ToNullableInt(object val)
        {
            if (val == null || val == DBNull.Value) return null;
            if (int.TryParse(val.ToString(), out int i)) return i;
            return null;
        }

        private static DateTime? ToNullableDateTime(object val)
        {
            if (val == null || val == DBNull.Value) return null;
            if (val is DateTime dt) return dt;
            if (DateTime.TryParse(val.ToString(), out DateTime pdt)) return pdt;
            return null;
        }

        private async Task<(int Total, List<ReceivedItemDetailDTO> Data)> QueryReceivedItemsAsync(ReceivedItemsFilterDTO filter)
        {
            if (filter == null) filter = new ReceivedItemsFilterDTO();
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1 && filter.PageSize != -1) filter.PageSize = 20;

            // Only show Department=Technical records with strict "Received" status
            var whereClauses = new List<string>
            {
                "ir.Department = 'Technical'",
                "ir.RequestMode <> 'Stock Return'",
                "ir.RequestNo IS NOT NULL AND ir.RequestNo <> ''",
                "ir.WaitingConfirmedReceivedStatus = 'Received'"
            };

            var parameters = new List<SqlParameter>();

            // Filter: SR No
            if (!string.IsNullOrWhiteSpace(filter.SRNo))
            {
                whereClauses.Add("ir.RequestNo LIKE @SRNo");
                parameters.Add(new SqlParameter("@SRNo", "%" + filter.SRNo.Trim() + "%"));
            }

            // Filter: Item Code
            if (!string.IsNullOrWhiteSpace(filter.ItemCode))
            {
                whereClauses.Add("(ird.ItemCode LIKE @ItemCode OR i.ItemCode LIKE @ItemCode)");
                parameters.Add(new SqlParameter("@ItemCode", "%" + filter.ItemCode.Trim() + "%"));
            }

            // Filter: Item Name
            if (!string.IsNullOrWhiteSpace(filter.ItemName))
            {
                whereClauses.Add("(ird.ItemName LIKE @ItemName OR i.ItemName LIKE @ItemName)");
                parameters.Add(new SqlParameter("@ItemName", "%" + filter.ItemName.Trim() + "%"));
            }

            // Filter: Serial Number
            if (!string.IsNullOrWhiteSpace(filter.SNNumber))
            {
                whereClauses.Add("ird.SerialNumber LIKE @SNNumber");
                parameters.Add(new SqlParameter("@SNNumber", "%" + filter.SNNumber.Trim() + "%"));
            }

            string whereSql = string.Join(" AND ", whereClauses);

            string fromJoinSql = @"
FROM dbo.TblItem_Request ir
INNER JOIN dbo.TblItem_Request_Detail ird on ird.RequestItemID = ir.RequestItemID
LEFT JOIN dbo.TblItem i on i.ItemID = ird.ItemID
LEFT JOIN dbo.TblCustomerType t on t.CustomerTypeID = ird.BlockID
LEFT JOIN dbo.TblProject tp on tp.ProjectID = ird.ProjectID";

            string countSql = "SELECT COUNT(1) " + fromJoinSql + " WHERE " + whereSql;

            string selectColumns = @"
ROW_NUMBER() OVER(ORDER BY ir.RequestItemID DESC, ird.RequestItemDetailID DESC) AS [No],
ird.RequestItemDetailID,
ISNULL(ir.RequestNo, '') AS [SR No],
ISNULL(ir.Department, '') AS [Department],
ISNULL(ir.RequestName, '') AS [Name],
ISNULL(ir.[Address], '') AS [Address],
FORMAT(ISNULL(ir.PreparedDate, ir.RequestDate), 'dd-MM-yyyy') AS [Issue Date],
ISNULL(ird.ItemCode, ISNULL(i.ItemCode, '')) AS [Item Code],
ISNULL(ird.ItemName, ISNULL(i.ItemName, '')) AS [Item Name],
ISNULL(ird.SerialNumber, '') AS [SN Number],
ird.QTY AS [QTY],
ISNULL(ird.Unit, '') AS [Unit],
ISNULL(tp.ProjectName, '') AS [Project],
ISNULL(t.CustomerType, '') AS [Block],
ISNULL(ird.Remark, '') AS [Purpose or Reasons]";

            string dataSql = "SELECT " + selectColumns + " " + fromJoinSql + " WHERE " + whereSql;

            if (filter.PageSize > 0)
            {
                dataSql += " ORDER BY ir.RequestItemID DESC, ird.RequestItemDetailID DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            }
            else
            {
                dataSql += " ORDER BY ir.RequestItemID DESC, ird.RequestItemDetailID DESC";
            }

            using (var ctx = new DBContext())
            {
                var conn = ctx.Database.Connection;
                bool needClose = false;
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                    needClose = true;
                }

                try
                {
                    int totalRecords = 0;
                    using (var cmdCount = conn.CreateCommand())
                    {
                        cmdCount.CommandText = countSql;
                        foreach (var p in CloneParameters(parameters))
                        {
                            cmdCount.Parameters.Add(p);
                        }
                        var scalar = await cmdCount.ExecuteScalarAsync();
                        totalRecords = Convert.ToInt32(scalar ?? 0);
                    }

                    var dataParams = CloneParameters(parameters).ToList();
                    if (filter.PageSize > 0)
                    {
                        int offset = (filter.Page - 1) * filter.PageSize;
                        dataParams.Add(new SqlParameter("@Offset", offset));
                        dataParams.Add(new SqlParameter("@PageSize", filter.PageSize));
                    }

                    var list = new List<ReceivedItemDetailDTO>();
                    using (var cmdData = conn.CreateCommand())
                    {
                        cmdData.CommandText = dataSql;
                        foreach (var p in dataParams)
                        {
                            cmdData.Parameters.Add(p);
                        }

                        using (var reader = await cmdData.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new ReceivedItemDetailDTO
                                {
                                    No = reader["No"] != DBNull.Value ? Convert.ToInt64(reader["No"]) : (long?)null,
                                    RequestItemDetailID = reader["RequestItemDetailID"] != DBNull.Value ? Convert.ToInt32(reader["RequestItemDetailID"]) : 0,
                                    SRNo = reader["SR No"]?.ToString() ?? "",
                                    Department = reader["Department"]?.ToString() ?? "",
                                    Name = reader["Name"]?.ToString() ?? "",
                                    Address = reader["Address"]?.ToString() ?? "",
                                    IssueDate = reader["Issue Date"]?.ToString() ?? "",
                                    ItemCode = reader["Item Code"]?.ToString() ?? "",
                                    ItemName = reader["Item Name"]?.ToString() ?? "",
                                    SNNumber = reader["SN Number"]?.ToString() ?? "",
                                    QTY = reader["QTY"] != DBNull.Value ? Convert.ToDecimal(reader["QTY"]) : (decimal?)null,
                                    Unit = reader["Unit"]?.ToString() ?? "",
                                    Project = reader["Project"]?.ToString() ?? "",
                                    Block = reader["Block"]?.ToString() ?? "",
                                    PurposeOrReasons = reader["Purpose or Reasons"]?.ToString() ?? ""
                                };
                                list.Add(item);
                            }
                        }
                    }

                    return (totalRecords, list);
                }
                finally
                {
                    if (needClose && conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
    }
}
