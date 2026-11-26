using MVC;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class StockExpireReorderAlertController : ApiController
    {
        [HttpGet]
        [Route("api/ExpiredPDF")]
        public async Task<HttpResponseMessage> GET()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult { Result = ResultType.Successful };


            var now = DateTime.Now.TimeOfDay;
            var day = DateTime.Now.DayOfWeek;

            if (now < new TimeSpan(8, 0, 0) || now > new TimeSpan(17, 0, 0) || day == DayOfWeek.Sunday)
                return Request.CreateResponse(HttpStatusCode.OK, "Outside working hours or Sunday - no alerts.");

            try
            {
                // Only send on Monday
                if (day != DayOfWeek.Monday)
                    return Request.CreateResponse(HttpStatusCode.OK, "Not Monday - no alerts sent.");

                NotificationTelegram notificationTelegram = new NotificationTelegram();


                string msg = $"<b>📦 Stock Expire Report</b><br><b>Date:</b> {DateTime.Now:dd-MM-yyyy}";


                var result = await html.URLtoPDF(
                    "Profile/ExpirePdf",   // 👉 Change to your actual page
                    "",
                    "Uploads/Sales/ExpirePdf",
                    "Report-StockExpire"
                );

                if (result.Result != ResultType.Successful)
                    throw new Exception("Failed to generate PDF: " + result.Message);

                string outFile = result.OutPutFile;




                sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(
                    MsgText: "",
                    //chatId: -1002999742690,   
                    //topicID: 115,
                    chatId: -1003110947397,
                    topicID: 396,
                    inlineKeyboard: null,
                    MessageID: 0,
                    TelegramMessage: "",
                    Files: outFile,
                    Caption: "Stock Expire Report",
                    EditMessage: false,
                    newBotClient: html.GetBotTokenBySystemID(25)
                );


                if (System.IO.File.Exists(outFile))
                {
                    System.IO.File.Delete(outFile);
                }
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }


            return Request.CreateResponse(
                sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                sqlResult.Message
            );
        }
        [HttpGet]
        [Route("api/ReorderPDF")]

        public async Task<HttpResponseMessage> ReorderPDF()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult { Result = ResultType.Successful };


            var now = DateTime.Now.TimeOfDay;
            var day = DateTime.Now.DayOfWeek;

            if (now < new TimeSpan(8, 0, 0) || now > new TimeSpan(17, 0, 0) || day == DayOfWeek.Sunday)
                return Request.CreateResponse(HttpStatusCode.OK, "Outside working hours or Sunday - no alerts.");

            try
            {
                // Only send on Monday
                if (day != DayOfWeek.Monday)
                    return Request.CreateResponse(HttpStatusCode.OK, "Not Monday - no alerts sent.");

                NotificationTelegram notificationTelegram = new NotificationTelegram();


                string msg = $"<b>📦 ReorderStock Report</b><br><b>Date:</b> {DateTime.Now:dd-MM-yyyy}";


                var result = await html.URLtoPDF(
                    "Profile/ReorderStock",   // 👉 Change to your actual page
                    "",
                    "Uploads/Sales/ReorderStock",
                    "Report-ReorderStock"
                );

                if (result.Result != ResultType.Successful)
                    throw new Exception("Failed to generate PDF: " + result.Message);

                string outFile = result.OutPutFile;

                sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(
                    MsgText: "",
                    chatId: -1003110947397,
                    topicID: 399,
                    inlineKeyboard: null,
                    MessageID: 0,
                    TelegramMessage: "",
                    Files: outFile,
                    Caption: "Reorder Stock Report",
                    EditMessage: false,
                    newBotClient: html.GetBotTokenBySystemID(25)
                );


                if (System.IO.File.Exists(outFile))
                {
                    System.IO.File.Delete(outFile);
                }
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }


            return Request.CreateResponse(
                sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                sqlResult.Message
            );
        }
    }
}
