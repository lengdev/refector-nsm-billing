using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class MasterDataRequest
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 30;
        public string SP { get; set; }
        public bool IsRawSql { get; set; } = false;
        public string OrderBy { get; set; }
        public string FilterParam { get; set; }
        public string SystemID { get; set; }
        public string WarehouseID { get; set; }
        public string DomainID { get; set; }
    }

    public class ApiGetMasterDataController : ApiController
    {
        private const int MaxPageSize = 99999;

        [HttpPost]
        [Route("ApiGetMasterData/GetListData")]
        public IHttpActionResult GetListData([FromBody] MasterDataRequest req)
        {
            try
            {
                if (req == null) return BadRequest("Empty request.");
                if (string.IsNullOrWhiteSpace(req.SP)) return BadRequest("Missing SP.");

                int page = req.page < 1 ? 1 : req.page;
                int pageSize = req.pageSize < 1 ? 30 : Math.Min(req.pageSize, MaxPageSize);

                var para = new Dictionary<string, object>();

                if (req.IsRawSql && string.IsNullOrWhiteSpace(req.OrderBy))
                    return BadRequest("OrderBy is required when IsRawSql true.");

                string decryptedText = EncryAndDecry.Decryp(req.SP);

                string sql;
                CommandType commandType;

                if (!string.IsNullOrEmpty(req.FilterParam))
                {
                    foreach (var f in req.FilterParam.Split('^'))
                    {
                        if (string.IsNullOrWhiteSpace(f)) continue;

                        var p = f.Split('|');
                        if (p.Length < 2) continue;

                        string paramName = "@" + EncryAndDecry.Decryp(p[0]);
                        object paramValue = p[1] == "All" ? "" : p[1];

                        if (!para.ContainsKey(paramName))
                            para.Add(paramName, paramValue);
                    }
                }

                if (req.IsRawSql)
                {
                    sql =
                       "SET NOCOUNT ON;\n" +
                       ";WITH __RawQuery__ AS (\n" + decryptedText + "\n)\n" +
                       "SELECT *, COUNT(*) OVER() AS TotalRecord\n" +
                       "FROM __RawQuery__\n" +
                       "ORDER BY " + req.OrderBy + "\n" +
                       "OFFSET @__Offset__ ROWS FETCH NEXT @__PageSize__ ROWS ONLY\n" +
                       "OPTION (RECOMPILE);";
                    commandType = CommandType.Text;

                    para.Add("@__Offset__", (page - 1) * pageSize);
                    para.Add("@__PageSize__", pageSize);
                }
                else
                {
                    sql = decryptedText;
                    commandType = CommandType.StoredProcedure;

                    para.Add("@Page", page);
                    para.Add("@RecsPerPage", pageSize);
                }

                if (!string.IsNullOrEmpty(req.SystemID))
                {
                    if (!int.TryParse(req.SystemID, out int systemId))
                        return BadRequest("SystemID must be numeric.");
                    para.Add("@SystemID", systemId);
                }
                if (!string.IsNullOrEmpty(req.WarehouseID))
                {
                    if (!int.TryParse(req.WarehouseID, out int warehouseId))
                        return BadRequest("WarehouseID must be numeric.");
                    para.Add("@WarehouseID", warehouseId);
                }
                if (req.DomainID != null)
                {
                    para.Add("@DomainID", req.DomainID);
                }

                HTML html = new HTML();
                DataSet ds = html.GetData(sql, commandType == CommandType.StoredProcedure, para);

                var data = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(ds.Tables[0]));

                int total = 0;
                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0 && ds.Tables[1].Columns.Contains("TotalRecord"))
                {
                    total = Convert.ToInt32(ds.Tables[1].Rows[0]["TotalRecord"]);
                }
                else if (ds.Tables[0].Columns.Contains("TotalRecord") && ds.Tables[0].Rows.Count > 0)
                {
                    total = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalRecord"]);
                }

                return Ok(new { data, total });
            }
            catch (SqlException)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "A database error occurred."
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("ApiGetMasterData/GetListDetail")]
        public IHttpActionResult GetListDetail([FromBody] dynamic req)
        {
            try
            {
                if (req == null)
                    return BadRequest("Empty request.");

                string spValue = Convert.ToString(req.SP);
                string sql = EncryAndDecry.Decryp(spValue);

                var para = new Dictionary<string, object>
                {
                    { "@Page", Convert.ToInt32(req.page) },
                    { "@RecsPerPage", Convert.ToInt32(req.pageSize) },
                    { "@LinkFieldValue", Convert.ToString(req.LinkFieldValue) ?? string.Empty },
                };

                HTML html = new HTML();
                DataSet ds = html.GetData(sql, true, para);

                var data = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(ds.Tables[0]));

                int total = 0;
                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0 && ds.Tables[1].Columns.Contains("TotalRecord"))
                {
                    total = Convert.ToInt32(ds.Tables[1].Rows[0]["TotalRecord"]);
                }
                else if (ds.Tables[0].Columns.Contains("TotalRecord") && ds.Tables[0].Rows.Count > 0)
                {
                    total = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalRecord"]);
                }

                return Ok(new { data, total });
            }
            catch (SqlException)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "A database error occurred."
                });
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "An unexpected error occurred."
                });
            }
        }

        [HttpPost]
        [Route("ApiGetMasterData/UpdateInvoiceRemark")]
        public IHttpActionResult UpdateInvoiceRemark([FromBody] dynamic request)
        {
            try
            {
                HTML html = new HTML();
                string sql = @"UPDATE dbo.TblCustomerInvoice SET Comment = @Comment, CommentBy = @CommentBy, CommentByID = @CommentByID, CommentDate = @CommentDate
WHERE InvoiceNo = @InvoiceNo and CustomerID = @CustomerID";

                var para = new Dictionary<string, string>
                {
                    { "@InvoiceNo", request.InvoiceNo.ToString() },
                    { "@CustomerID", request.CustomerID.ToString() },
                    { "@Comment", request.Comment.ToString() },
                    { "@CommentBy",  html.UserFullName},
                    { "@CommentByID",  html.UserID},
                    { "@CommentDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                SqlResult result = html.SaveChangeData(sql, false, para);

                return Ok(new { msg = result.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("ApiGetMasterData/UpdateInvoiceRemarkUnpaid")]
        public IHttpActionResult UpdateInvoiceRemarkUnpaid([FromBody] dynamic request)
        {
            try
            {
                HTML html = new HTML();
                string sql = @"UPDATE dbo.TblCustomerInvoice SET CommentUnpaid = @CommentUnpaid, CommentBy = @CommentBy, CommentByID = @CommentByID, CommentDate = @CommentDate
WHERE InvoiceNo = @InvoiceNo and CustomerID = @CustomerID";

                var para = new Dictionary<string, string>
                {
                    { "@InvoiceNo", request.InvoiceNo.ToString() },
                    { "@CustomerID", request.CustomerID.ToString() },
                    { "@CommentUnpaid", request.CommentUnpaid.ToString() },
                    { "@CommentBy",  html.UserFullName},
                    { "@CommentByID",  html.UserID},
                    { "@CommentDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                SqlResult result = html.SaveChangeData(sql, false, para);

                return Ok(new { msg = result.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static List<Dictionary<string, object>> DataTableToList(DataTable table)
        {
            var rows = new List<Dictionary<string, object>>(table.Rows.Count);
            var columnNames = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>(columnNames.Length);
                foreach (var col in columnNames)
                {
                    var val = row[col];
                    dict[col] = (val == DBNull.Value) ? null : val;
                }
                rows.Add(dict);
            }

            return rows;
        }
    }
}