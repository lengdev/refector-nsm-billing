using MVC;
using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class ReminderPOController : ApiController
    {
        // GET api/<controller>
        [HttpGet]
        [Route("api/ReminderPO")]
        public async Task<HttpResponseMessage> GET()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult
            {
                Result = ResultType.Successful
            };
            var now = DateTime.Now.TimeOfDay;
            var day = DateTime.Now.DayOfWeek;
            var PM5 = new TimeSpan(17, 0, 0);
            var AM8 = new TimeSpan(8, 0, 0);

            //Do nothing if current time < 8am or > 5pm and except Sunday 
            if ((now > PM5) || (now < AM8) || (day == DayOfWeek.Sunday))
            {
                sqlResult.Result = ResultType.Successful;
                return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
            }

            NotificationTelegram notificationTelegram = new NotificationTelegram();
            string sql = @"select * from dbo.TblPurchaseOrder where StatusID in (60, 92) and DraftPOStatusID = 90";
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
                        bool isPendingCheck = dr["DraftPOStatusID"]?.ToString() == "90"
                            && !string.IsNullOrEmpty(dr["NeedToCheckBy"]?.ToString());
                        bool isPendingApprove = dr["StatusID"]?.ToString() == "92" || (string.IsNullOrEmpty(dr["NeedToCheckBy"]?.ToString()) && dr["StatusID"]?.ToString() == "60");
                        string fullName = isPendingApprove ? dr["NeedToApproval"]?.ToString()
                            : dr["NeedToCheckBy"]?.ToString();
                        string userName = isPendingApprove ? dr["TelegramUserName"]?.ToString()
                            : dr["CheckUsername"]?.ToString();
                        string approverId = isPendingApprove ? dr["ApproveByUserID"]?.ToString()
                            : dr["NeedToCheckUserID"]?.ToString();
                        string groupId = dr["TelegramGroupID"]?.ToString();
                        string messageId = dr["TelegramMessageID"]?.ToString();
                        string PONumber = dr["PONumber"]?.ToString();
                        string systemId = dr["SystemID"]?.ToString();
                        string botToken = html.GetBotTokenBySystemID(int.Parse(systemId));

                        var msg = new StringBuilder();
                        msg.AppendLine("📣 <b>Alert Pending PO</b>\n");
                        msg.AppendLine($"<i>{userName}</i>\n");
                        msg.AppendLine($"Dear {(approverId == "38" ? "Boss" : fullName)}, PO: <b>{PONumber}</b> is still pending.\n");
                        msg.AppendLine($"Please take action as soon as possible. Thank you!");
                        sqlResult = await notificationTelegram.NotificationTelegramBot(
                                msg.ToString(),
                                long.Parse(groupId),
                                replyMarkupInlineKeyboardMarkup: true,
                                MessageID: int.Parse(messageId),
                                edit: false,
                                newBotClient: botToken
                            );
                    }
                }
            }
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
    }
}