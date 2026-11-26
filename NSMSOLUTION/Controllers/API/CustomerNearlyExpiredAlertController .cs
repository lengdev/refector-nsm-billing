using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class CustomerNearlyExpiredAlertController : ApiController
    {
        private const int SystemId = 13;
        private const int WarehouseId = 16;
        private const string DomainId = "001";
        private const long TelegramChatId = -1003973197414;

        private string StripHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            var match = System.Text.RegularExpressions.Regex.Match(input, @">([^<]+)<\/");
            return match.Success ? match.Groups[1].Value.Trim() : System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", "").Trim();
        }

        [HttpGet]
        [Route("api/CustomerNearlyExpiredAlert")]
        public async Task<HttpResponseMessage> GET()
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                return Request.CreateResponse(HttpStatusCode.OK, "Outside working hours or Sunday - no alerts.");

            string tempFilePath = null;
            bool sendSucceeded = false;

            try
            {
                var html = new HTML();

                var param = new Dictionary<string, string>
                {
                    { "@FromDate", DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd") },
                    { "@ToDate",   DateTime.Now.AddDays(+3).ToString("yyyy-MM-dd") },
                    { "@SystemID", SystemId.ToString() },
                    { "@WarehouseID", WarehouseId.ToString() },
                    { "@DomainID", DomainId },
                    { "@Page", "1" },
                    { "@RecsPerPage", "100" }
                };

                using (DataSet ds = html.LoadData("SP_SEARCH_CUSTOMER_NEARLY_EXPIRRE", true, param))
                {
                    if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                        return Request.CreateResponse(HttpStatusCode.OK, "No nearly-expired customers found.");

                    DataTable dt = ds.Tables[0];

                    string fileName = "CustomerNearlyExpired";
                    string destinationFolder = "Uploads/CustomerNearlyExpired";

                    var sbHtml = new StringBuilder();

                    sbHtml.Append(@"<!DOCTYPE html>
                    <html>
                    <head>
                    <meta charset='utf-8' />
                    <link href='https://fonts.googleapis.com/css2?family=Khmer&family=Moul&display=swap' rel='stylesheet' crossorigin />
                    <link href='https://fonts.googleapis.com/css2?family=Inter:ital,opsz,wght@0,14..32,100..900;1,14..32,100..900&display=swap' rel='stylesheet' crossorigin />
                    <style>
                    @page {
                        size: A4 landscape;
                        margin: 8mm 5mm 10mm 5mm;
                    }

                    @font-face {
                        font-family: 'KhmerOS';
                        src: url('../Content/fonts/KhmerOSSiemreap.ttf') format('truetype');
                        font-weight: normal;
                        font-style: normal;
                    }
                    @font-face {
                        font-family: 'KhmerOSContent';
                        src: url('../Content/fonts/KhmerOScontent.ttf') format('truetype');
                        font-weight: normal;
                        font-style: normal;
                    }

                    body {
                        font-family: 'KhmerOS', Arial, sans-serif;
                        font-size: 10pt;
                    }

                    .header-table { width: 100%; border-collapse: collapse; margin-bottom: 12px; }
                    .header-table td { border: none !important; vertical-align: middle !important; }
                    .logo-cell { width: 100px; text-align: center; }
                    .title-cell { text-align: left; padding-left: 40px !important; }

                    .company-kh { font-family: 'Moul', serif !important; }
                    .company-en { font-family: Arial, sans-serif; }
                    .report-title { font-family: Arial, sans-serif; }

                    .print-date {
                        font-family: Arial, sans-serif;
                        font-size: 9pt;
                        color: #666;
                        display: block;
                    }

                    table {
                        width: 100%;
                        border-collapse: collapse;
                        margin-top: 10px;
                    }

                    th, td {
                        border: 1px solid #000;
                        font-size: 8pt;
                        padding: 7px;
                    }

                    th {
                        background-color: #f89c39;
                        color: white;
                        font-size:bold;
                        font-size: 9pt;
                        text-align: center;
                    }
                    .invoiceNO{ width:100px; }

                    td.center { text-align: center; }
                    td.right  { text-align: right; }
                    td.remark { font-size: 7pt; width: 210px; }
                    td.invoice { width: 100px; font-size: 7pt; }

                    tr     { page-break-inside: avoid; }
                    thead  { display: table-header-group; }
                    </style>
                    </head>
                    <body>");

                    string logoUrl = "../Content/NSMLogo.png";

                    sbHtml.Append($@"
                    <table class='header-table'>
                        <tr>
                            <td class='logo-cell'>
                                <img src='{logoUrl}' style='height:100px;  display:block; margin:20px auto;' onerror=""this.style.display='none'"" />
                            </td>
                            <td class='title-cell' >
                                <div class='company-kh' style='font-size:18px; display:block; padding-bottom:8px;margin-left:60px;' >ក្រុមហ៊ុន អិនអេសអិម សឹលូសិន អេន មេថឹណិន ឯ.ក</div>
                                <div style='font-family:Arial,sans-serif; font-size:11pt; display:block; padding-bottom:8px;margin-left:125px;' class='company-en'>NSM SOLUTION AND MAINTENANCE CO., LTD</div>
                                <div style='font-family:Arial,sans-serif; font-size:11pt; display:block; padding-bottom:8px;margin-left:125px;' class='report-title'>Report Customer Nearly Expired on {DateTime.Now:dd-MMM-yyyy}</div>
                                <div style='font-family:Arial,sans-serif; font-size:9pt; color:#666; display:block;margin-left:225px;'>Date: {DateTime.Now:dd-MMM-yyyy}</div>
                            </td>
                        </tr>
                    </table>");

                    string[] fieldNames = { "No","CustomerCode","CustomerName","InvoiceDate","InvoiceNo",
                        "Amount","EndDate","DaysLeft","Status","Comment", "PostComment" };

                    string[] headers = { "No.","CID","Customer Name","Invoice Date","Invoice No",
                        "Amount","Expire Date","Days Left","Status","Remark", "Sale Remark" };

                    // Fields that may contain HTML/markup from the DB and should be stripped down
                    // to plain text before display. NOTE: original code targeted "InvoiceNoLink"/
                    // "ServiceStatus", which don't exist in fieldNames, so it never actually ran —
                    // confirm which columns really need stripping; "Status" is a guess.
                    var fieldsNeedingStrip = new HashSet<string> { "Status" };

                    sbHtml.Append("<table>");
                    sbHtml.Append("<thead style='font-family: Inter !important;'><tr>");
                    foreach (var h in headers)
                        sbHtml.Append($"<th>{WebUtility.HtmlEncode(h)}</th>");
                    sbHtml.Append("</tr></thead><tbody>");

                    foreach (DataRow dr in dt.Rows)
                    {
                        sbHtml.Append("<tr>");

                        for (int col = 0; col < fieldNames.Length; col++)
                        {
                            string field = fieldNames[col];
                            string val = dt.Columns.Contains(field) ? (dr[field]?.ToString() ?? "") : "";

                            if (fieldsNeedingStrip.Contains(field))
                                val = StripHtml(val);

                            if (field == "InvoiceDate" || field == "EndDate")
                            {
                                val = DateTime.TryParse(val, out var parsed)
                                    ? parsed.ToString("dd-MMM-yyyy")
                                    : "-";
                            }

                            if (field == "Amount")
                                val = "$ " + val;

                            // Encode first, then re-introduce our own <br/> markup so user data
                            // can't break out of the table structure.
                            val = WebUtility.HtmlEncode(val).Replace("\n", "<br/>");

                            string cls = "";
                            if (col == 0 || col == 3 || col == 6 || col == 8) cls = "center";
                            if (col == 5 || col == 7) cls = "right";
                            if (col == 9) cls = "remark";
                            if (col == 4) cls = "invoiceNO";

                            sbHtml.Append($"<td class='{cls}'>{val}</td>");
                        }

                        sbHtml.Append("</tr>");
                    }

                    sbHtml.Append("</tbody></table></body></html>");

                    var pdfGenResult = await html.HTMLToPDF(sbHtml.ToString(), destinationFolder, fileName, ShowPageNumber: true);

                    if (pdfGenResult.Result != ResultType.Successful)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"PDF Failed: {pdfGenResult.Message}");

                    // File exists on disk from here on — track it so `finally` can clean up
                    // even if something below throws.
                    tempFilePath = pdfGenResult.OutPutFile;

                    string botToken = html.GetBotTokenBySystemID(SystemId);
                    if (string.IsNullOrWhiteSpace(botToken))
                        return Request.CreateResponse(HttpStatusCode.InternalServerError,
                            $"No bot token configured for SystemID {SystemId}");

                    var caption = new StringBuilder();
                    caption.AppendLine("<b>📅 Customer Nearly Expired Report</b>\n");
                    caption.AppendLine($"<i>Date: {DateTime.Now:dd-MM-yyyy}</i>\n");
                    caption.AppendLine("<i>BILLING SYSTEM AUTO GENERATED REPORT.</i>");

                    var notificationTelegram = new NotificationTelegram();
                    var sendResult = await notificationTelegram.NotificationTelegramBot(
                        MsgText: "",
                        chatId: TelegramChatId,
                        PDFFile: tempFilePath,
                        Caption: caption.ToString(),
                        newBotClient: botToken
                    );

                    sendSucceeded = sendResult.Result == ResultType.Successful;

                    return Request.CreateResponse(
                        sendSucceeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                        sendSucceeded ? "Report sent successfully" : sendResult.Message
                    );
                }
            }
            catch (Exception ex)
            {
                // TODO: log ex
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                // Only delete once we know the send actually succeeded — a failed send
                // leaves the file for retry/inspection, same as the other alert controllers.
                if (sendSucceeded && !string.IsNullOrWhiteSpace(tempFilePath))
                {
                    try
                    {
                        if (File.Exists(tempFilePath))
                            File.Delete(tempFilePath);
                    }
                    catch
                    {
                        // Non-fatal — leave for manual cleanup.
                    }
                }
            }
        }
    }
}