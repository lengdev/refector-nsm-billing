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
    public class ReminderAdvExpenseController : ApiController
    {
        // GET api/<controller>
        [HttpGet]
        [Route("api/ReminderAdvExpense")]
        public async Task<HttpResponseMessage> GET()
        {
            try
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
                string sql = @"select AdvanceExpenseID, AdvNo, CheckStatus, ApprovedStatus, CheckedBy, CheckedByTelegramUserName, ApprovedBy, ApprovedByTelegramUserName, CreateDate,TelegramGroupID, TelegramGroupTopicID, TelegramMessageID, TelegramMessage, SystemID
                        from dbo.TblAdvanceExpense 
                        where isnull(CheckedByUserID, 0) <> 0 and isnull(CheckStatus, 'Pending') = 'Pending' or isnull(ApprovedStatus, 'Pending') = 'Pending'";

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
                            int advId = int.Parse(dr["AdvanceExpenseID"].ToString());
                            string advNo = dr["AdvNo"].ToString();
                            string checkStatus = dr["CheckStatus"].ToString();
                            string approveStatus = dr["ApprovedStatus"].ToString();
                            string checkBy = dr["CheckedBy"].ToString();
                            string checkByUsername = dr["CheckedByTelegramUserName"].ToString();
                            string approveBy = dr["ApprovedBy"].ToString();
                            string approveByUsername = dr["ApprovedByTelegramUserName"].ToString();
                            long telegramGroupId = long.Parse(dr["TelegramGroupID"].ToString());
                            int groupTopicId = int.Parse(dr["TelegramGroupTopicID"].ToString());
                            int telegramMessageId = int.Parse(dr["TelegramMessageID"].ToString());
                            string telegramMessage = dr["TelegramMessage"].ToString();

                            int sysID = int.Parse(dr["SystemID"].ToString());
                            string bt = html.GetBotTokenBySystemID(sysID);

                            string username = approveByUsername;
                            if (!string.IsNullOrEmpty(checkBy) && (checkStatus == "Pending" || string.IsNullOrEmpty(checkStatus)))
                            {
                                username = checkByUsername;
                            }

                            if (checkStatus != "Disapproved" || string.IsNullOrEmpty(checkStatus))
                            {
                                string msg = $"📣 Alert Pending Advance Expense!<br><br>Hello {username}, Advance Expense {advNo} is still pending.<br>Please take action as soon as possible. Thank you!";
                                sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(msg, telegramGroupId, groupTopicId, null, telegramMessageId, telegramMessage, "", "", false, "", true, bt);
                            }
                        }
                    }
                }

                return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, ex.Message);
            }
        }
    }
}