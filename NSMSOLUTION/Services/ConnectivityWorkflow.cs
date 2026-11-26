using MVC;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Utilities;

namespace NSMSOLUTION.Services
{
    public class ConnectivityOutcome
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int NewStatus { get; set; }
        public bool DbWasWrong { get; set; }
    }

    public enum AlertType
    {
        Activate,
        Reactivate,
        Lock,
        Terminate,
        Inactive
    }

    public static class ConnectivityWorkflow
    {
        private struct Target
        {
            public long ChatId;
            public int Topic;
            public Target(long chatId, int topic) { ChatId = chatId; Topic = topic; }
        }

        private static readonly Dictionary<AlertType, Target[]> Routes = new Dictionary<AlertType, Target[]>
        {
            [AlertType.Activate] = new[] { new Target(-1545132903, 2), new Target(-5399466732, 0) },
            [AlertType.Reactivate] = new[] { new Target(-1903890245, 4), new Target(-1545132903, 4), new Target(-5399466732, 0) },
            [AlertType.Lock] = new[] { new Target(-1903890245, 4), new Target(-1545132903, 4), new Target(-5399466732, 0) },
            [AlertType.Terminate] = new[] { new Target(-1903890245, 4), new Target(-1545132903, 4), new Target(-5399466732, 0) },
            [AlertType.Inactive] = new[] { new Target(-1903890245, 4), new Target(-1545132903, 4), new Target(-5399466732, 0) }
        };

        private static AlertType FromStatus(int status)
        {
            return status == ConnectivityStatusCodes.Active ? AlertType.Reactivate
                 : status == ConnectivityStatusCodes.Locked ? AlertType.Lock
                 : status == ConnectivityStatusCodes.Terminated ? AlertType.Terminate
                 : AlertType.Inactive;
        }

        private static string H(string s)
        {
            return WebUtility.HtmlEncode(s ?? "");
        }

        private static async Task SendAlertAsync(AlertType type, string message)
        {
            Target[] targets;
            if (!Routes.TryGetValue(type, out targets)) return;

            var tg = new NotificationTelegram();
            foreach (var t in targets)
            {
                try { 
                    if(t.Topic != 0) await tg.NotificationTelegramBotToTopic(message, t.ChatId, t.Topic); 
                    else await tg.NotificationTelegramBot(message, t.ChatId); 
                }
                catch { }
            }
        }

        public static async Task<ConnectivityOutcome> RunAsync(HTML html, int id, int status, string userComment)
        {
            var c = ConnectivityRepository.Load(id);
            if (c == null) return new ConnectivityOutcome { Message = "Connection not found." };

            DateTime now = AppTime.Now;
            string stamp = $"{userComment}; {ConnectivityText.Past(status)} by:{html.UserFullName}; {now:dd-MM-yyyy HH:mm:ss tt}";

            ConnectivityResult r;
            try
            {
                r = status == ConnectivityStatusCodes.Active
                    ? ActivationService.Activate(c, stamp, allowFromTerminated: true)
                    : ActivationService.Deactivate(c, stamp, status);
            }
            catch (Exception ex)
            {
                return new ConnectivityOutcome { Message = ex.Message };
            }

            var customer = await CustomerService.GetCustomerByIdAsync(c.CustomerID);
            string customerName = CustomerService.GetCustomerNamePrefix(c.CustomerID);

            string message =
                $"<b>{(status == ConnectivityStatusCodes.Active ? "🟢 RE-ACTIVATED" : "🔴 " + H(ConnectivityText.Past(status).ToUpper()))} SERVICE</b>\n\n" +
                $"Customer: <b>{H(customerName)}</b>\n" +
                $"CID: <code>{H(customer?.CustomerCode ?? "N/A")}</code>\n" +
                $"SID: <code>{H(c.SID)}</code>\n" +
                $"ConnectionID: <code>{H(c.ConnectionID)}</code>\n\n" +
                $"Reason: {H(userComment)}\n\n" +
                $"Date: <b>{now:dd-MM-yyyy hh:mm:ss tt}</b>\n" +
                $"By: <b>{H(html.UserFullName)}</b>";

            string actionType = status == ConnectivityStatusCodes.Active ? "Reactivate"
                  : status == ConnectivityStatusCodes.Locked ? "Lock"
                  : status == ConnectivityStatusCodes.Terminated ? "Terminate" : "Inactive";

            var save = ConnectivityRepository.SaveStatus(html.UserFullName, id, c.ConnectivityStatus, r.NewStatus,
                message, stamp, actionType, "SaveStatus",
                r.Skipped ? "Router already in state; " + r.Message : r.Message, now);
            if (save.Result != ResultType.Successful)
                return new ConnectivityOutcome { Message = save.Message };

            try { ConnectivityRepository.AddTicketComment(html, c.TicketNumber, message); }
            catch { }

            await SendAlertAsync(FromStatus(status), message);

            return new ConnectivityOutcome
            {
                Success = true,
                Message = r.Message,
                NewStatus = r.NewStatus,
                DbWasWrong = r.DbWasWrong
            };
        }

        public static async Task ActivateMessage(
            HTML html,
            int activateCustomerId,
            string user,
            DateTime now,
            string remark,
            string packageName,
            DateTime? startDate,
            DateTime? endDate)
        {
            try
            {
                var c = ConnectivityRepository.Load(activateCustomerId);
                if (c == null) return;

                var customerService = new CustomerService();
                var customer = await CustomerService.GetCustomerByIdAsync(c.CustomerID);
                string customerName = customer == null ? "N/A" : CustomerService.GetCustomerNamePrefix(c.CustomerID);

                string package = H((packageName ?? "N/A")
                    .Replace("<br>", " ").Replace("<br/>", " ").Replace("<br />", " "));

                string message =
                    "<b>🟢 NEW SERVICE ACTIVATED</b>\n\n" +
                    "Dear Team,\n\n" +
                    $"Customer <b>{H(customerName)}</b> is now <b>ACTIVATED</b>\n" +
                    $"Date: <b>{now:dd-MMM-yyyy hh:mm:ss tt}</b>\n" +
                    $"By: <b>{H(user)}</b>\n\n" +
                    $"Package: <b>{package}</b>\n\n" +
                    $"CID: <code>{H(customer?.CustomerCode ?? "N/A")}</code>\n" +
                    $"SID: <code>{H(c.SID)}</code>\n" +
                    $"Connection ID: <code>{H(c.ConnectionID)}</code>\n\n" +
                    $"Start Date: <b>{(startDate.HasValue ? startDate.Value.ToString("dd/MMM/yyyy") : "N/A")}</b>\n" +
                    $"Expired Date: <b>{(endDate.HasValue ? endDate.Value.ToString("dd/MMM/yyyy") : "N/A")}</b>\n" +
                    "\nThank you and best regards,\n" +
                    "NSM ISP BILLING SYSTEM";

                await SendAlertAsync(AlertType.Activate, message);

                if (customer == null) return;

                await CustomerNotifier.SendAsync(
                    html,
                    CustomerAlert.Activate,
                    customerId: c.CustomerID,
                    customerCode: customer.CustomerCode,
                    customerName: customerName,
                    nationality: customer.CustomerNationality,
                    connectionId: c.ConnectionID,
                    activateCustomerId: activateCustomerId,
                    packageName: packageName,
                    startDate: startDate?.ToString("dd-MM-yyyy"),
                    endDate: endDate?.ToString("dd-MM-yyyy"));
            }
            catch { }
        }
    }
}