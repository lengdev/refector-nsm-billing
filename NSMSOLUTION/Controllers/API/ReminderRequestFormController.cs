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
    public class ReminderRequestFormController : ApiController
    {
        // GET api/<controller>
        [HttpGet]
        [Route("api/ReminderRequestForm")]
        public async Task<HttpResponseMessage> GET()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();

            var now = DateTime.Now.TimeOfDay;
            var day = DateTime.Now.DayOfWeek;
            var PM5 = new TimeSpan(17, 0, 0);
            var AM8 = new TimeSpan(8, 0, 0);

            //Do nothing if current time < 8am or > 5pm
            if ((now > PM5) || (now < AM8) || (day == DayOfWeek.Sunday))
            {
                sqlResult.Result = ResultType.Successful;
                return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
            }

            NotificationTelegram notificationTelegram = new NotificationTelegram();
            string sql = @"select r.RequestFormID, r.ReferenceNumber, r.TelegramMessageID, r.TelegramMessage, isnull(r.TelegramGroupID,0) TelegramGroupID, case when isnull(r.NeedNextApprovalByUserID, 0) <> 0 and isnull(r.NextApproveStatusID, 0) = 0 and r.StatusID = 90 then r.NeedNextApprovalByUsername else r.TelegramUserName end [ApproveByUserName], r.CreateDate,
r.PaymentMean, isnull(r.CheckedByUserID, 0) [CheckedByUserID], r.NeedToCheckUserID, case when r.NeedToCheckUserID <> 0 then u.TelegramUserName  else '' end [CheckByUserName], r.SystemID
from dbo.TblRequestForm r
left join dbo.TblUserLogin u on u.UserLoginID = r.NeedToCheckUserID
where r.StatusID in (60, 92) or (isnull(r.NeedNextApprovalByUserID, 0) <> 0 and isnull(r.NextApproveStatusID, 0) = 0 and r.StatusID <> 93);";

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
                        int RequestFormID = int.Parse(dr["RequestFormID"].ToString());
                        var TelegramMessageID = dr["TelegramMessageID"].ToString();
                        var TelegramMessage = dr["TelegramMessage"].ToString();
                        var PaymentMean = dr["PaymentMean"].ToString();
                        int NeedToCheckByID = int.Parse(dr["NeedToCheckUserID"].ToString());
                        int CheckedByUserID = int.Parse(dr["CheckedByUserID"].ToString());
                        int sysID = int.Parse(dr["SystemID"].ToString());
                        string bt = html.GetBotTokenBySystemID(sysID);

                        string TelegramUserName = NeedToCheckByID != 0 && CheckedByUserID == 0 ? dr["CheckByUserName"].ToString() : dr["ApproveByUserName"].ToString();
                        var ReferenceNumber = dr["ReferenceNumber"].ToString();
                        long TelegramGroupID = long.Parse(dr["TelegramGroupID"].ToString());
                        string msg = $"📣 Alert Pending Request Form!<br><br>Hello {TelegramUserName}, Request Form {ReferenceNumber} is still pending.<br>Please take action as soon as possible. Thank you!";
                        sqlResult = TelegramGroupID != 0 && TelegramGroupID != 1 && TelegramMessageID != ""
                            ? await notificationTelegram.NotificationTelegramBot("", TelegramGroupID, "", "", msg, false, true, RequestFormID, int.Parse(TelegramMessageID), TelegramMessage, null, false, 0, "", bt)
                            : await notificationTelegram.NotificationTelegramBot("", TelegramGroupID, "", "", msg, false, false, RequestFormID, 0, "", null, false, 0, "", bt);
                    }
                }
            }
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
    }
}