using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class ItemExpireAutoAlertController : ApiController
    {
        // GET api/<controller>
        [HttpGet]
        [Route("api/ItemExpireAutoAlert")]
        public async Task<HttpResponseMessage> GET()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult { Result = ResultType.Successful };
            var now = DateTime.Now.TimeOfDay;
            var day = DateTime.Now.DayOfWeek;
            var PM5 = new TimeSpan(17, 0, 0);
            var AM8 = new TimeSpan(8, 0, 0);

            // Do nothing if current time < 8am or > 5pm and it's Sunday
            if ((now > PM5) || (now < AM8) || (day == DayOfWeek.Sunday))
            {
                return Request.CreateResponse(HttpStatusCode.OK, sqlResult.Message);
            }

            NotificationTelegram notificationTelegram = new NotificationTelegram();
            var sql = GetInitialQuery();
            DataSet ds = html.LoadData(sql, false);

            if (ds.Tables[0].Rows.Count > 0)
            {
                var expirationMessages = new Dictionary<int, (string message, List<string> itemIds)>();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (DateTime.TryParse(dr["ItemExpirationDate"].ToString(), out DateTime expireDate))
                    {
                        int totalDays = (expireDate.Date - DateTime.Now.Date).Days;
                        if (new[] { 180, 30, 15, 7 }.Contains(totalDays))
                        {
                            if (!expirationMessages.ContainsKey(totalDays))
                            {
                                expirationMessages[totalDays] = ($"📣 Alert Testing..{totalDays} days\n", new List<string>());
                            }
                            expirationMessages[totalDays].itemIds.Add(dr["ItemID"].ToString());
                        }
                    }
                }

                foreach (var entry in expirationMessages)
                {
                    int days = entry.Key;
                    var (message, itemIds) = entry.Value;
                    var expDate = ds.Tables[0].Rows
                        .Cast<DataRow>()
                        .FirstOrDefault(dr => DateTime.TryParse(dr["ItemExpirationDate"].ToString(), out DateTime expireDate) &&
                                              (expireDate.Date - DateTime.Now.Date).Days == days)?["ItemExpirationDate"];

                    if (expDate != null)
                    {
                        sqlResult = await ProcessExpirationNotification(html, notificationTelegram, itemIds, expDate.ToString(), message);
                    }
                }
            }

            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }

        private string GetInitialQuery()
        {
            return @"SELECT i.ItemID, i.ItemName, i.ItemDescription, iv.ItemCode, iv.UM, sid.LocationID, 
             w.WarehouseName, sid.ItemExpirationDate, SUM(ISNULL(sid.QTY, 0)) AS QTY 
             FROM dbo.TblStockInDetail sid 
             INNER JOIN dbo.TblItem i ON i.ItemID = sid.ItemID 
             LEFT JOIN dbo.TblItemVariant iv ON iv.ItemID = i.ItemID AND iv.UomID = sid.UomID
             INNER JOIN dbo.TblUserWarehouse w ON w.WID = sid.LocationID 
             WHERE sid.Type IN ('Stock In', 'Opening Stock')
             GROUP BY i.ItemID, i.ItemName, i.ItemDescription, iv.ItemCode, iv.UM, 
                      sid.LocationID, w.WarehouseName, sid.ItemExpirationDate";
        }

        private async Task<SqlResult> ProcessExpirationNotification(HTML html, NotificationTelegram notificationTelegram, List<string> itemIds, string expDate, string message)
        {
            string sql = $@"SELECT i.ItemID, i.ItemName, i.ItemDescription, iv.ItemCode, iv.UM, sid.LocationID, 
                    w.WarehouseName, sid.ItemExpirationDate, SUM(ISNULL(sid.QTY, 0)) AS QTY 
                    FROM dbo.TblStockInDetail sid 
                    INNER JOIN dbo.TblItem i ON i.ItemID = sid.ItemID 
                    LEFT JOIN dbo.TblItemVariant iv ON iv.ItemID = i.ItemID AND iv.UomID = sid.UomID
                    INNER JOIN dbo.TblUserWarehouse w ON w.WID = sid.LocationID
                    WHERE i.ItemID IN ('{string.Join("','", itemIds)}') AND sid.ItemExpirationDate = '{expDate}'
                    GROUP BY i.ItemID, i.ItemName, i.ItemDescription, iv.ItemCode, iv.UM, 
                             sid.LocationID, w.WarehouseName, sid.ItemExpirationDate";

            var result = await html.URLtoPDF("Items/ItemExpiredFormHtml?sql=" + sql, "", "Uploads/ItemExpirePdf/", "");
            return await notificationTelegram.NotificationTelegramBot("", -4510188155, "", result.OutPutFile, message, false, true, 0, 0, "", null, false);
        }


    }
}