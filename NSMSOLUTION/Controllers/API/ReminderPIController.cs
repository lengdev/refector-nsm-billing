using System.Web.Http;

namespace NSMSOLUTION.Controllers.API
{
    public class ReminderPIController : ApiController
    {
        // GET api/<controller>
        //[HttpGet]
        //[Route("api/ReminderPI")]
        //public async Task<HttpResponseMessage> GET()
        //{
        //    HTML html = new HTML();
        //    SqlResult sqlResult = new SqlResult();
        //    sqlResult.Result = ResultType.Successful;
        //    var now = DateTime.Now.TimeOfDay;
        //    var day = DateTime.Now.DayOfWeek;
        //    var PM5 = new TimeSpan(17, 0, 0);
        //    var AM8 = new TimeSpan(8, 0, 0);

        //    //Do nothing if current time < 8am or > 5pm and except Sunday 
        //    if ((now > PM5) || (now < AM8) || (day == DayOfWeek.Sunday))
        //    {
        //        sqlResult.Result = ResultType.Successful;
        //        return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        //    }

        //    NotificationTelegram notificationTelegram = new NotificationTelegram();
        //    string sql = @"select TelegramMessageID,InvoiceNo,CreateDate from TblCustomerBalance where InvoiceStatus = 'Pending' and isnull(TelegramMessageID, '') <> ''";

        //    DataSet ds = html.LoadData(sql, false);
        //    if (ds.Tables[0].Rows.Count > 0)
        //    {
        //        foreach (DataRow dr in ds.Tables[0].Rows)
        //        {
        //            DateTime createDate = DateTime.Parse(dr["CreateDate"].ToString());
        //            TimeSpan timeDifference = DateTime.Now - createDate;
        //            double pendingMinutes = timeDifference.TotalMinutes;
        //            /*
        //             Fetch only data created > (2h = 120mn)
        //             */
        //            if (pendingMinutes > 15)
        //            {
        //                var TelegramMessageID = dr["TelegramMessageID"].ToString();
        //                var InvoiceNo = dr["InvoiceNo"].ToString();
        //                string msg = $"📣 <b>Alert Pending Check PI!</b><br><br>Hello, This PI number <b>{InvoiceNo}</b> is still pending.<br>Please take action as soon as possible. Thank you!";
        //                sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(msg, 1545132903, 54, null, int.Parse(TelegramMessageID), "", "", "", false, "", true);
        //            }
        //        }
        //    }

        //    sql = @"select InvoiceTelegramMessageID TelegramMessageID, CreateDate, isnull(ApproveRequestToDMCTAXTGMID,'') ApproveRequestToDMCTAXTGMID, InvoiceNo from TblCustomerInvoice where isnull(ApproveRequestToDMCTAXStatus, '')='Pending'";
        //    ds = html.LoadData(sql, false);
        //    if (ds.Tables[0].Rows.Count > 0)
        //    {
        //        foreach (DataRow dr in ds.Tables[0].Rows)
        //        {
        //            DateTime createDate = DateTime.Parse(dr["CreateDate"].ToString());
        //            TimeSpan timeDifference = DateTime.Now - createDate;
        //            double pendingMinutes = timeDifference.TotalMinutes;
        //            /*
        //             Fetch only data created > (2h = 120mn)
        //             */
        //            if (pendingMinutes > 15)
        //            {
        //                var TelegramMessageID = dr["TelegramMessageID"].ToString();
        //                string[] ApproveRequestToDMCTAXTGMID = dr["ApproveRequestToDMCTAXTGMID"].ToString().Split('/');
        //                var InvoiceNo = dr["InvoiceNo"].ToString();
        //                string msg = $"📣 <b>Alert Pending Approval Invoice!</b><br><br>Hello, This Invoice number <b>{InvoiceNo}</b> is still pending.<br>Please take action as soon as possible. Thank you!";
        //                if (ApproveRequestToDMCTAXTGMID.Length > 1)
        //                    sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(msg, long.Parse(ApproveRequestToDMCTAXTGMID[0]), int.Parse(ApproveRequestToDMCTAXTGMID[1]), null, int.Parse(TelegramMessageID), "", "", "", false, "", true);
        //                else if (ApproveRequestToDMCTAXTGMID[0].ToString() != "0" && ApproveRequestToDMCTAXTGMID[0].ToString() != "" && TelegramMessageID != "" && TelegramMessageID != "0")
        //                    sqlResult = await notificationTelegram.NotificationTelegramBot("", long.Parse(ApproveRequestToDMCTAXTGMID[0]), "", "", msg, false, true, 0, int.Parse(TelegramMessageID), "", null, false);
        //            }
        //        }
        //    }
        //    return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        //}
    }
}