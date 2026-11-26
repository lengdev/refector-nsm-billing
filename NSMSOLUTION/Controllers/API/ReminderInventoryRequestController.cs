using MVC;
using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class ReminderInventoryRequestController : ApiController
    {
        // GET api/<controller>
        [HttpGet]
        [Route("api/ReminderInventoryRequest")]
        public async Task<HttpResponseMessage> GET()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            var now = DateTime.Now.TimeOfDay;
            var PM5 = new TimeSpan(17, 0, 0);
            var AM8 = new TimeSpan(8, 0, 0);

            //Do nothing if current time < 8am or > 5pm
            if ((now > PM5) || (now < AM8))
            {
                sqlResult.Result = ResultType.Successful;
                return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
            }

            string sql = @"select RequestItemID, case when RequestMode='Item Request' then RequestNo else RequestReturnNo end as RequestNo, RequestMode, NeedToApprovalTelegram, isnull(TelegramMessageID, '') TelegramMessageID, 
isnull(TelegramMessage, '') TelegramMessage, CreateDate, TelegramGroupTopicID, TelegramGroupID
from dbo.TblItem_Request
where Status = 'Pending' and isnull(TelegramMessageID, '') <> '' and isnull(TelegramMessage, '') <> '' and NeedToApprovalUserID <> 0 and RequestMode in ('Item Request', 'Request Stock Return')";
            DataSet ds = html.LoadData(sql, false);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    DateTime createDate = DateTime.Parse(dr["CreateDate"].ToString());
                    TimeSpan timeDifference = DateTime.Now - createDate;
                    double pendingMinutes = timeDifference.TotalMinutes;
                    /*
                     Fetch only data created > (2h = 120mn)
                     */
                    if (pendingMinutes > 120)
                    {
                        int RequestItemID = int.Parse(dr["RequestItemID"].ToString());
                        long TelegramGroupID = long.Parse(dr["TelegramGroupID"].ToString());
                        string TelegramMessageID = dr["TelegramMessageID"].ToString();
                        string TelegramGroupTopicID = dr["TelegramGroupTopicID"].ToString();
                        string TelegramMessage = dr["TelegramMessage"].ToString();
                        string RequestNo = dr["RequestNo"].ToString();
                        string TelegramUserName = dr["NeedToApprovalTelegram"].ToString();
                        string RequestMode = dr["RequestMode"].ToString();
                        //long TelegramGroupID = -4117466359;
                        string msg = $"📣 Alert Pending {RequestMode}!<br><br>Hello {TelegramUserName}, {RequestMode} {RequestNo} is still pending.<br>Please take action as soon as possible. Thank you!";
                        if (RequestMode == "Request Stock Return")
                        {
                            TelegramGroupID = -4530227617;
                            sqlResult = await notificationTelegram.NotificationTelegramBot("", TelegramGroupID, "", "", msg, false, true, RequestItemID, int.Parse(TelegramMessageID), TelegramMessage, null, false);
                        }
                        else
                        {
                            sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(msg, TelegramGroupID, int.Parse(TelegramGroupTopicID), null, int.Parse(TelegramMessageID), TelegramMessage, "", "", false);
                        }

                    }
                }
            }
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
    }
}