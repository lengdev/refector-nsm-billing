using MVC;
using NSMSOLUTION.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class ReportSaleDailyAlertController : ApiController
    {
        private const long TelegramChatId = -1002504403736;

        // System 13 (NSM-ISP) is split per class, like the AR report.
        private const string SplitByClassSystemId = "13";
        private const int ExcludedClassId = 15;

        private static readonly Dictionary<string, (string FileName, int BotTokenSystemId)> Systems =
            new Dictionary<string, (string, int)>
            {
                ["13"] = ("NSM-ISP", 13),
                ["25"] = ("BSM", 13),
                ["27"] = ("BS-BSCo.,ltd", 13)
            };

        private class ReportJob
        {
            public string SysId { get; set; }
            public string DisplayName { get; set; }   // used in messages / file name
            public int BotTokenSystemId { get; set; }
            public int? ClassId { get; set; }          // null = whole system
            public string ClassName { get; set; }
        }

        // =========================================================
        // AUTOMATIC SCHEDULER (run every day)
        // Example: api/ReportSaleDailyAlert?date=2026-10-30
        // =========================================================

        [HttpGet]
        [Route("api/ReportSaleDailyAlert")]
        public async Task<HttpResponseMessage> GET(DateTime? date = null)
        {
            try
            {
                DateTime today = (date ?? DateTime.Today).Date;

                if (!ReportScheduleService.IsWorkingDay(today))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Skipped on Sunday");
                }

                var messages = new List<string>();
                bool anyFailed = false;

                foreach (ReportPeriod period in ReportScheduleService.GetScheduledPeriods(today))
                {
                    ReportDateRange range = ReportScheduleService.GetRange(period, today);

                    Tuple<bool, string> result = await SendPeriod(range);

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
                // TODO: log ex
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // =========================================================
        // MANUAL ENDPOINTS
        // Example: api/ReportSaleWeekly?date=2026-10-30
        // =========================================================

        [HttpGet, Route("api/ReportSaleWeekly")]
        public Task<HttpResponseMessage> Weekly(DateTime? date = null)
        {
            return Manual(ReportPeriod.Weekly, date);
        }

        [HttpGet, Route("api/ReportSaleMonthly")]
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

                Tuple<bool, string> result = await SendPeriod(range);

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
        // JOB LIST: one job per system, system 13 expands to one job per class
        // =========================================================

        private List<ReportJob> BuildJobs(HTML html, List<string> failures)
        {
            var jobs = new List<ReportJob>();

            foreach (var kvp in Systems)
            {
                string sysId = kvp.Key;
                string displayName = kvp.Value.FileName;
                int botSysId = kvp.Value.BotTokenSystemId;

                if (sysId != SplitByClassSystemId)
                {
                    jobs.Add(new ReportJob
                    {
                        SysId = sysId,
                        DisplayName = displayName,
                        BotTokenSystemId = botSysId
                    });
                    continue;
                }

                const string sql = @"
                    SELECT ClassID, ClassName
                    FROM dbo.TblClass
                    WHERE ClassID <> @ExcludedClassId
                      AND SystemID = @SystemId";

                var para = new Dictionary<string, string>
                {
                    { "@ExcludedClassId", ExcludedClassId.ToString() },
                    { "@SystemId", sysId }
                };

                using (DataSet ds = html.LoadData(sql, false, para))
                {
                    if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    {
                        failures.Add($"{displayName}: no class found");
                        continue;
                    }

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        jobs.Add(new ReportJob
                        {
                            SysId = sysId,
                            DisplayName = displayName,
                            BotTokenSystemId = botSysId,
                            ClassId = Convert.ToInt32(dr["ClassID"]),
                            ClassName = dr["ClassName"].ToString()
                        });
                    }
                }
            }

            return jobs;
        }

        // =========================================================
        // SHARED GENERATION + TELEGRAM SENDER
        // Item1 = Success, Item2 = Result message
        // =========================================================

        private async Task<Tuple<bool, string>> SendPeriod(ReportDateRange range)
        {
            var html = new HTML();
            var notificationTelegram = new NotificationTelegram();
            var failures = new List<string>();
            var successes = new List<string>();

            CultureInfo ci = CultureInfo.InvariantCulture;

            string from = range.From.ToString("yyyy-MM-dd", ci);
            string to = range.To.ToString("yyyy-MM-dd", ci);

            foreach (ReportJob job in BuildJobs(html, failures))
            {
                bool perClass = job.ClassId.HasValue;
                string label = perClass ? $"{job.DisplayName}/{job.ClassName}" : job.DisplayName;

                try
                {
                    string url =
                        $"Profile/DailySaleReportForm?sysId={job.SysId}&period={range.Title}&from={from}&to={to}";

                    if (perClass)
                        url += $"&classId={job.ClassId.Value}";

                    string fileName = $"Report-{range.Title}-{job.DisplayName}";
                    if (perClass)
                        fileName += "-" + SafeFileName(job.ClassName);

                    var pdfResult = await html.URLtoPDF(
                        url,
                        "",
                        "Uploads/Sales/DailyReport",
                        fileName);

                    if (pdfResult == null || string.IsNullOrWhiteSpace(pdfResult.OutPutFile))
                    {
                        failures.Add($"{label}: PDF generation returned no file");
                        continue;
                    }

                    string botToken = html.GetBotTokenBySystemID(job.BotTokenSystemId);
                    if (string.IsNullOrWhiteSpace(botToken))
                    {
                        failures.Add($"{label}: no bot token for SystemID {job.BotTokenSystemId}");
                        continue;
                    }

                    string caption =
                        $"<b>{range.Title} Sale Report" +
                        (perClass ? " " + WebUtility.HtmlEncode(job.ClassName) : "") + "</b>\n" +
                        $"<b>{range.Label}</b>\n\n" +
                        "BILLING SYSTEM AUTO GENERATED REPORT.";

                    var sendResult = await notificationTelegram.NotificationTelegramBot(
                        MsgText: "",
                        chatId: TelegramChatId,
                        PDFFile: pdfResult.OutPutFile,
                        Caption: caption,
                        newBotClient: botToken);

                    if (sendResult.Result == ResultType.Successful)
                    {
                        successes.Add(label);
                        TryDeleteFile(pdfResult.OutPutFile);
                    }
                    else
                    {
                        // File left on disk when send fails, for retry/debugging.
                        failures.Add($"{label}: {sendResult.Message}");
                    }
                }
                catch (Exception exJob)
                {
                    // Continue with the remaining systems / classes.
                    failures.Add($"{label}: {exJob.Message}");
                }
            }

            bool hasError = failures.Count > 0;

            string message = hasError
                ? $"{range.Title}: {successes.Count} sent, {failures.Count} failed: {string.Join(" | ", failures)}"
                : $"{range.Title}: all reports sent successfully ({successes.Count} reports)";

            return Tuple.Create(!hasError, message);
        }

        private static string SafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            return name.Replace(' ', '_');
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
                // Non-fatal — leave for manual cleanup.
            }
        }
    }
}