using MVC;
using NSMSOLUTION.Controllers.Report;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class CustomerInvoiceDailyAlertController : ApiController
    {
        [HttpGet]
        [Route("api/CustomerInvoiceDailyAlert")]
        public async Task<HttpResponseMessage> GET()
        {
            SqlResult sqlResult = new SqlResult { Result = ResultType.Successful };

            try
            {
                var day = DateTime.Now.DayOfWeek;
                if (day == DayOfWeek.Sunday)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Sunday - no alerts.");
                }

                var report = new ReportController();
                var pdfResult = await report.RenderCustomerInvoiceDailyAlert();

                if (pdfResult.Result != ResultType.Successful)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, pdfResult.Message);
                }

                var notification = new NotificationTelegram();
                string buildCaption(string reportName)
                {
                    var msg = new StringBuilder();
                    msg.AppendLine("<b>\uD83D\uDCB0 " + reportName + "</b>\n");
                    msg.AppendLine($"<i>Date: {DateTime.Now:dd/MM/yyyy}</i>\n");
                    msg.AppendLine("<i>BILLING SYSTEM AUTO GENERATED REPORT.</i>");
                    return msg.ToString();
                }

                var files = (pdfResult.OutPutFile ?? "").Split('~').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                var captions = (pdfResult.Caption ?? "").Split('~').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                // Send each PDF as a separate Telegram message (one alert = one PDF)
                for (int i = 0; i < files.Count; i++)
                {
                    var html = new HTML();
                    string captionText = i < captions.Count ? buildCaption(captions[i]) : buildCaption("Daily Unpaid Invoice Report");
                    string botToken = html.GetBotTokenBySystemID(13);
                    sqlResult = await notification.NotificationTelegramBot("", -1003973197414, PDFFile: files[i], Caption: captionText, newBotClient: botToken);
                }

                // Clean up temp files
                foreach (var file in files)
                {
                    if (System.IO.File.Exists(file))
                        System.IO.File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
    }
}


