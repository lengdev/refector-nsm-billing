using MVC;
using NSMSOLUTION.Controllers.Report;
using NSMSOLUTION.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class ReportARController : ApiController
    {
        private const int TargetSystemId = 13;
        private const int ExcludedClassId = 15;
        private const long TelegramChatId = -1003973197414;

        // =========================================================
        // AUTOMATIC REPORT SCHEDULER (run every day)
        // =========================================================

        [HttpGet, Route("api/ReportARDaily")]
        public async Task<HttpResponseMessage> Auto(DateTime? date = null)
        {
            try
            {
                DateTime today = (date ?? DateTime.Today).Date;

                if (!ReportScheduleService.IsWorkingDay(today))
                {
                    return Request.CreateResponse(
                        HttpStatusCode.OK,
                        "Skipped: Sunday is a non-working day.");
                }

                var messages = new List<string>();
                bool anyFailed = false;

                foreach (ReportPeriod period in ReportScheduleService.GetScheduledPeriods(today))
                {
                    ReportDateRange range = ReportScheduleService.GetRange(period, today);

                    Tuple<bool, string> result = await SendPeriod(period, range);

                    messages.Add(result.Item2);

                    if (!result.Item1)
                        anyFailed = true;
                }

                return Request.CreateResponse(
                    anyFailed ? HttpStatusCode.BadRequest : HttpStatusCode.OK,
                    string.Join(" || ", messages));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // =========================================================
        // MANUAL REPORT ENDPOINTS
        // Example: api/ReportARWeekly?date=2026-10-30
        // =========================================================

        [HttpGet, Route("api/ReportARWeekly")]
        public Task<HttpResponseMessage> Weekly(DateTime? date = null)
        {
            return Manual(ReportPeriod.Weekly, date);
        }

        [HttpGet, Route("api/ReportARMonthly")]
        public Task<HttpResponseMessage> Monthly(DateTime? date = null)
        {
            return Manual(ReportPeriod.Monthly, date);
        }

        private async Task<HttpResponseMessage> Manual(ReportPeriod period, DateTime? date)
        {
            try
            {
                DateTime referenceDate = (date ?? DateTime.Today).Date;

                ReportDateRange range = ReportScheduleService.GetRange(period, referenceDate);

                Tuple<bool, string> result = await SendPeriod(period, range);

                return Request.CreateResponse(
                    result.Item1 ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                    result.Item2);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // =========================================================
        // REPORT GENERATION AND TELEGRAM SENDER
        // Item1 = Success, Item2 = Result message
        // =========================================================

        private async Task<Tuple<bool, string>> SendPeriod(ReportPeriod period, ReportDateRange range)
        {
            var html = new HTML();

            const string sql = @"
                SELECT
                    ClassID,
                    ClassName
                FROM dbo.TblClass
                WHERE ClassID <> @ExcludedClassId
                  AND SystemID = @SystemId";

            var para = new Dictionary<string, string>
            {
                { "@ExcludedClassId", ExcludedClassId.ToString() },
                { "@SystemId", TargetSystemId.ToString() }
            };

            using (DataSet ds = html.LoadData(sql, false, para))
            {
                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                {
                    return Tuple.Create(false, range.Title + ": no class found");
                }

                string botToken = html.GetBotTokenBySystemID(TargetSystemId);

                if (string.IsNullOrWhiteSpace(botToken))
                {
                    return Tuple.Create(
                        false,
                        range.Title + ": no bot token configured for SystemID " + TargetSystemId);
                }

                var report = new ReportController();
                var notification = new NotificationTelegram();
                var failures = new List<string>();
                var successes = new List<string>();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int classId = Convert.ToInt32(dr["ClassID"]);
                    string className = dr["ClassName"].ToString();

                    try
                    {
                        var pdfResult = await report.RenderReportAR(
                            classId,
                            className,
                            range.From,
                            range.To,
                            range.Title);

                        if (pdfResult == null || string.IsNullOrWhiteSpace(pdfResult.OutPutFile))
                        {
                            failures.Add(className + ": report generation returned no file");
                            continue;
                        }

                        var msg = new StringBuilder();
                        msg.AppendLine($"<b>💰 Report Income {range.Title} {className}</b>\n");
                        msg.AppendLine($"<i>{range.Label}</i>\n");
                        msg.AppendLine("<i>BILLING SYSTEM AUTO GENERATED REPORT.</i>");

                        var result = await notification.NotificationTelegramBot(
                            "",
                            TelegramChatId,
                            PDFFile: pdfResult.OutPutFile,
                            Caption: msg.ToString(),
                            newBotClient: botToken);

                        if (result.Result == ResultType.Successful)
                        {
                            successes.Add(className);
                            TryDeleteFile(pdfResult.OutPutFile);
                        }
                        else
                        {
                            failures.Add($"{className}: {result.Message}");
                        }
                    }
                    catch (Exception exClass)
                    {
                        // Continue with other classes.
                        failures.Add($"{className}: {exClass.Message}");
                    }
                }

                bool hasError = failures.Count > 0;

                string message = hasError
                    ? $"{range.Title}: {successes.Count} sent, " +
                      $"{failures.Count} failed: {string.Join(" | ", failures)}"
                    : $"{range.Title}: all reports sent successfully ({successes.Count} classes)";

                return Tuple.Create(!hasError, message);
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch
            {
                // Cleanup failure should not affect report status.
            }
        }
    }
}