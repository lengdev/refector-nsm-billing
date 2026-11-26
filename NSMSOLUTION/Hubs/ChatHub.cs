using Microsoft.AspNet.SignalR;
using MVC;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace NSMSOLUTION.Hubs
{
    public class ChatHub : Hub
    {
        // Role IDs allowed to receive SentAlertToTeamSale notifications.
        // 6  = Administrator For Billing System
        // 16 = Finance
        // ** Replace "YOUR_SALES_ROLE_ID" with the actual Sales role ID from TblUserRoles **
        private static readonly List<string> _alertRoleIDs = new List<string>
        {
            "6", "16", "7"
        };

        public void JoinRoleGroup(string roleID)
        {
            if (_alertRoleIDs.Contains(roleID))
            {
                Groups.Add(Context.ConnectionId, "AlertRole_" + roleID);
            }
        }

        public void SendMessage(
            string user,
            string UserProfilePicture,
            string message,
            string projectName,
            string CommentText,
            string TicketID,
            string TicketNumber,
            string SupportProjectID,
            string System_ID,
            string Warehouse_ID)
        {
            HTML html = new HTML();

            string sql = @"
                SELECT STUFF((
                    SELECT ',' + UserLoginID FROM (
                        SELECT CONVERT(varchar, ul.UserLoginID) UserLoginID
                        FROM   dbo.TblUserLogin ul
                        INNER JOIN dbo.TblSupportUserProject up
                            ON up.SupportProjectID = @SupportProjectID
                            AND up.UserLoginID = ul.UserLoginID
                        INNER JOIN dbo.TblSetUserAccessSystemAndWarehouse sw
                            ON sw.UserLoginID = ul.UserLoginID
                        WHERE  sw.SystemID = @SystemID
                          AND  sw.WarehouseID = @WarehouseID

                        UNION

                        SELECT CONVERT(varchar, ul.UserLoginID) UserLoginID
                        FROM   dbo.TblUserLogin ul
                        INNER JOIN dbo.TblSetUserAccessSystemAndWarehouse sw
                            ON sw.UserLoginID = ul.UserLoginID
                        INNER JOIN dbo.TblSupportGroupProject gp
                            ON gp.SupportProjectID = @SupportProjectID
                        INNER JOIN dbo.TblSupportUserGroup ug
                            ON ug.GroupID = gp.GroupID
                            AND ug.UserLoginID = ul.UserLoginID
                        WHERE  sw.SystemID = @SystemID
                          AND  sw.WarehouseID = @WarehouseID
                    ) A
                    FOR XML PATH('')
                ), 1, 1, '') UserLoginID";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SupportProjectID", SupportProjectID },
                { "SystemID",         System_ID        },
                { "WarehouseID",      Warehouse_ID     }
            };

            string Data = html.JsonData(sql, false, para);
            Clients.All.ReceiveMessage(user, UserProfilePicture, message, projectName, CommentText, TicketID, TicketNumber, Data);
        }

        public void SendNewCustomerNotification(string CustomerID, string CustomerName, string CustomerCode)
        {
            Clients.All.ReceiveCustomerNotification(CustomerID, CustomerName, CustomerCode);
        }

        // Broadcasts only to connections that belong to an allowed role group.
        public async Task SentAlertToTeamSale(string ReferenceID,
            string CustomerID, string CustomerName, string CustomerCode,
            string CustomerMobile, string ExpireDate, string Comment, string Reason, string SentBy)
        {
            string telegramMsg =
                $"🔔 <b>Customer Alert</b>\n" +
                $"CID   : <code>{CustomerCode}</code>\n" +
                $"Name  : <code>{CustomerName}</code>\n" +
                $"Phone : <b>{CustomerMobile}</b>\n" +
                $"Expire Date : <b>{DateTime.Parse(ExpireDate).ToString("dd-MMM-yyyy")}</b>\n" +
                $"Remark : {(string.IsNullOrEmpty(Comment) ? "-" : Comment)}\n\n" +
                $"<i>Reason : {(string.IsNullOrEmpty(Reason) ? "-" : Reason)} ({SentBy})</i>";

            var notification = new NotificationTelegram();
            var sent = await notification.NotificationTelegramBot(telegramMsg, -5081672205);

            int.TryParse(sent.Value, out var telegramMessageID);
            var telegramGroupID = sent.ChatID != 0 ? sent.ChatID : -5081672205;
            int alertID = await SaveAlertAsync(
                "customer", CustomerID, Comment, Reason, SentBy,
                telegramMessageID, telegramGroupID, Utility.TryParse(ReferenceID, "int", 0), isUrgent: false);

            foreach (string roleID in _alertRoleIDs)
            {
                Clients.Group("AlertRole_" + roleID)
                       .ReceiveAlertCustomer(ReferenceID, alertID, CustomerID, CustomerName, CustomerCode, CustomerMobile, ExpireDate, Comment, Reason, SentBy);
            }
        }

        private async Task<int> SaveAlertAsync(
            string alertType,
            string customerID,
            string comment,
            string reason,
            string sentBy,
            int messageId,
            long groupId,
            int referenceID,
            bool isUrgent = false)
        {
            try
            {
                int? custID = int.TryParse(customerID, out var parsed) ? parsed : (int?)null;
                var html = new HTML();
                using (var db = new DBContext())
                {
                    var existing = await db.TblAlertMessages
                        .FirstOrDefaultAsync(x => x.CustomerID == custID && !x.IsDismissed);

                    if (existing != null)
                    {
                        existing.AlertType = alertType;
                        existing.Comment = comment ?? "";
                        existing.Reason = reason ?? "";
                        existing.SentBy = sentBy;
                        existing.IsUrgent = isUrgent;
                        existing.ReferenceID = referenceID;
                        existing.IsRead = false;
                        existing.MessageID = messageId > 0 ? messageId : (int?)null;
                        existing.TelegramGroupID = groupId.ToString();
                        existing.CreatedAt = DateTime.Now;
                        existing.CreatedAt = DateTime.Now;
                        existing.SystemID = int.Parse(html.SystemID);
                        existing.WarehouseID = int.Parse(html.WarehouseID);
                        existing.DomainID = html.DomainID;

                        await db.SaveChangesAsync();
                        return existing.AlertID;
                    }
                    else
                    {
                        var newAlert = new AlertMessage
                        {
                            AlertType = alertType,
                            CustomerID = custID,
                            Comment = comment ?? "",
                            Reason = reason ?? "",
                            SentBy = sentBy,
                            IsUrgent = isUrgent,
                            ReferenceID = referenceID,
                            IsRead = false,
                            IsDismissed = false,
                            MessageID = messageId > 0 ? messageId : (int?)null,
                            TelegramGroupID = groupId.ToString(),
                            CreatedAt = DateTime.Now,
                            SystemID = int.Parse(html.SystemID),
                            WarehouseID = int.Parse(html.WarehouseID),
                            DomainID = html.DomainID
                        };

                        db.TblAlertMessages.Add(newAlert);
                        await db.SaveChangesAsync();

                        return newAlert.AlertID;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[SaveAlertAsync] {ex.Message}");
                return 0;
            }
        }
    }
}