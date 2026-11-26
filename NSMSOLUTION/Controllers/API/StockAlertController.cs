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
    public class StockAlertController : ApiController
    {
        [HttpGet]
        [Route("api/StockAlert")]
        public async Task<HttpResponseMessage> GET()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();

            var now = DateTime.Now.TimeOfDay;
            var day = DateTime.Now.DayOfWeek;


            //weekend or outside working hours
            if (now < new TimeSpan(8, 0, 0) || now > new TimeSpan(17, 0, 0) || day == DayOfWeek.Sunday)
                return Request.CreateResponse(HttpStatusCode.OK, "Outside working hours or Sunday - no alerts.");

            NotificationTelegram notificationTelegram = new NotificationTelegram();

            DataSet ds = html.LoadData("SP_ReorderPoint_Alert", true);
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return Request.CreateResponse(HttpStatusCode.OK, "No reorder items found.");

            Dictionary<string, List<string>> tokenMessages = new Dictionary<string, List<string>>();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string systemId = dr["SystemID"]?.ToString() ?? "0";
                string botToken = html.GetBotTokenBySystemID(int.Parse(systemId));
                if (string.IsNullOrWhiteSpace(botToken)) continue;

                if (!tokenMessages.ContainsKey(botToken))
                    tokenMessages[botToken] = new List<string>();

                tokenMessages[botToken].Add(
                    $"No: {dr["No"]}\n" +
                    $"Item Code: <b>{dr["ItemCode"]}</b>\n" +
                    $"Item Name: <b>{dr["ItemName"]}</b>\n" +
                    $"Reorder Level: <b>{dr["ReorderPoint"]} {dr["UM"]}</b>\n" +
                    $"<b>Stock on Hand: {dr["StockOnHand"]} {dr["UM"]}</b>\n" +
                    $"System: [{(systemId == "13" ? "🟠 NSMISP" : "🔴 BSM")}]\n" +
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
                    string msg = "📦 <b>Reorder Alert</b>\n\n" + string.Join("", batch) +
                                 "⚠️ Please restock to avoid shortage.";
                    sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(msg, -1003110947397, 2, null, 0, "", newBotClient: botToken);
                    //sqlResult = await notificationTelegram.NotificationTelegramBot(msg, -4984711194, edit: false, newBotClient: botToken);
                }
            }

            return Request.CreateResponse(
                sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                sqlResult.Message
            );
        }

    }
}
