using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class StockExpireAlertController : ApiController
    {
        [HttpGet]
        [Route("api/StockExpireAlert")]
        public async Task<HttpResponseMessage> GET()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();

            var now = DateTime.Now.TimeOfDay;
            var day = DateTime.Now.DayOfWeek;

            if (now < new TimeSpan(8, 0, 0) || now > new TimeSpan(17, 0, 0) || day == DayOfWeek.Sunday)
                return Request.CreateResponse(HttpStatusCode.OK, "Outside working hours or Sunday - no alerts.");

            NotificationTelegram notificationTelegram = new NotificationTelegram();
            DataSet ds = html.LoadData("SP_EXPIRE_STOCK_ALERT", true);
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return Request.CreateResponse(HttpStatusCode.OK, "No reorder items found.");

            Dictionary<string, List<string>> tokenMessages = new Dictionary<string, List<string>>();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                var telegramAlert = dr["TelegramAlert"];
                if (telegramAlert != DBNull.Value && Convert.ToInt32(telegramAlert) == 1)
                    continue;
                string systemId = dr["SystemID"]?.ToString() ?? "0";
                string botToken = html.GetBotTokenBySystemID(int.Parse(systemId));

                string rawHtml = dr["DaysUntilExpiry"]?.ToString()?.Trim() ?? "";
                string cleanText = System.Text.RegularExpressions.Regex.Replace(rawHtml, "<.*?>", "").Trim();
                string expiryMessage = $"<b >{cleanText}</b>";
                string spot = cleanText.StartsWith("-")
      ? "<b>🔴</b>"
      : "<b>🟢</b>";

                string systemName = systemId == "13" ? "🟠 NSMISP" : $"{spot} BSM";
                if (string.IsNullOrWhiteSpace(botToken)) continue;

                if (!tokenMessages.ContainsKey(botToken))
                    tokenMessages[botToken] = new List<string>();

                tokenMessages[botToken].Add(
                    //$"No: {dr["No"]}\n" +
                    $"Item Code: <b>{dr["ItemCode"]}</b>\n" +
                    $"Item Name: <b>{dr["ItemName"]}</b>\n" +
                    $"<b>Stock on Hand: {dr["StockOnHand"]} {dr["UM"]}</b>\n" +
                    $"Expired In: {expiryMessage}\n" +
                    $"📅 Expired Date: {dr["ItemExpirationDate"]}\n" +
                    $"System: [{systemName}]\n" +
                "==============================================\n\n"
                );
            }

            foreach (var entry in tokenMessages)
            {
                var botToken = entry.Key;
                var items = entry.Value;

                for (int i = 0; i < items.Count; i += 10)
                {
                    var batch = items.Skip(i).Take(10);
                    string msg = "📦 <b>Stock Expire</b>\n\n" + string.Join("", batch) +
                                 "⚠️ Please Check Expire item.";
                    //await NotificationTelegramBotToTopic("", -1545132903, 494, null, 0, "", PDFFile, "Send Fail to Phone:" + PhoneNumber + "\n\n" + MsgText);
                    sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(msg, -1003110947397, 4, null, 0, "", newBotClient: botToken);
                    //sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(msg, -1002999742690, 115, null, 0, "", newBotClient: botToken);
                    //chatId: -1002999742690, // PDF report chat
                    //         topicID: 115,
                }
            }

            return Request.CreateResponse(
                sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                sqlResult.Message
            );
        }

    }
}
