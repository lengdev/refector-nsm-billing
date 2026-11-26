using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class NotificationPaymentApprovalController : ApiController
    {
        [HttpPost]
        [Route("api/NotificationPaymentApproval/UpdateStatus")]
        [Route("api/NotificationPaymentApproval")]
        public async Task<HttpResponseMessage> NotificationPaymentApproval()
        {
            try
            {
                var formData = HttpContext.Current.Request.Form;
                string rawId = formData["NotificationPaymentID"] ?? formData["NPID"] ?? formData["ID"] ?? formData["SalesID"] ?? formData["JournalEntryID"];
                if (string.IsNullOrEmpty(rawId))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = "Missing required parameters: NotificationPaymentID." });
                }

                int npId = int.Parse(rawId);
                string status = formData["Status"] ?? formData["StatusKey"] ?? formData["ActionCallBack"] ?? "";
                string userName = (formData["UserName"] ?? "").Trim().TrimStart('@');
                string chatID = formData["ChatID"] ?? formData["TelegramGroupID"] ?? "";
                string messageID = formData["MessageID"] ?? formData["TelegramMessageID"] ?? "";
                string topicID = formData["TopicID"] ?? formData["TelegramGroupTopicID"] ?? "";
                string message = formData["TelegramMessage"] ?? formData["Message"] ?? "";

                HTML html = new HTML();

                string resolverSql = @"
                    SELECT np.*, 
                           u.UserLoginID, u.FullName, u.TelegramUserName,
                           ISNULL(b.BotToken, '') AS BotToken
                    FROM dbo.TblNotificationPayment np
                    LEFT JOIN dbo.TblUserLogin u ON u.TelegramUserName = '@' + @UserName OR u.TelegramUserName = @UserName
                    LEFT JOIN dbo.TblTelegramGroupNotification t ON t.TID = 2039
                    LEFT JOIN dbo.TblTelegramBots b ON b.BotID = t.BotID
                    WHERE np.NotificationPaymentID = @NotificationPaymentID";

                var para = new Dictionary<string, string> {
                    { "NotificationPaymentID", npId.ToString() },
                    { "UserName", userName }
                };
                DataSet ds = html.LoadData(resolverSql, false, para);

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound,
                        new { success = false, message = "Notification Payment record not found." });
                }

                DataRow row = ds.Tables[0].Rows[0];
                int currentStatus = row["StatusID"] != DBNull.Value ? Convert.ToInt32(row["StatusID"]) : 60;
                string botToken = row["BotToken"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(botToken))
                {
                    int systemId = row["SystemID"] != DBNull.Value ? Convert.ToInt32(row["SystemID"]) : 0;
                    botToken = html.GetBotTokenBySystemID(systemId);
                }

                var notification = new NotificationTelegram();

                // ── Normalize Status & Determine Workflow Phase ────────────────────────────
                string normalizedStatus = status;
                if (status == "CheckNP" || status == "70" || status.IndexOf("Check", StringComparison.OrdinalIgnoreCase) >= 0)
                    normalizedStatus = "CheckNP";
                else if (status == "ApproveNP" || status == "90" || status.IndexOf("Approve", StringComparison.OrdinalIgnoreCase) >= 0)
                    normalizedStatus = "ApproveNP";
                else if (status == "RejectNP" || status == "DisapproveNP" || status == "91" || status.IndexOf("Reject", StringComparison.OrdinalIgnoreCase) >= 0 || status.IndexOf("Disapprove", StringComparison.OrdinalIgnoreCase) >= 0)
                    normalizedStatus = "RejectNP";

                bool hasCheckColumns = row.Table.Columns.Contains("CheckDate");
                bool isCheckPhase = normalizedStatus == "CheckNP"
                    || (hasCheckColumns && (row["CheckDate"] == DBNull.Value || Convert.ToInt32(row["CheckStatusID"] ?? 60) == 60) && normalizedStatus != "ApproveNP");

                bool isDisapprove = normalizedStatus == "RejectNP";

                // ── User Authorization Check (similar to JournalEntryApprovalController) ───
                int targetAssignedId = 0;
                string targetAssignedName = "";

                if (isCheckPhase)
                {
                    targetAssignedId = row.Table.Columns.Contains("CheckByUserID") && row["CheckByUserID"] != DBNull.Value ? Convert.ToInt32(row["CheckByUserID"]) : 0;
                    targetAssignedName = row.Table.Columns.Contains("CheckBy") && row["CheckBy"] != DBNull.Value ? row["CheckBy"].ToString() : "";
                }
                else
                {
                    targetAssignedId = row["ApproveByUserID"] != DBNull.Value ? Convert.ToInt32(row["ApproveByUserID"]) : 0;
                    targetAssignedName = row["ApproveBy"] != DBNull.Value ? row["ApproveBy"].ToString() : "";
                }

                string actionUserFullName = !string.IsNullOrEmpty(userName) ? "@" + userName : "Telegram User";
                int actionUserID = 0;

                if (row["UserLoginID"] != DBNull.Value)
                {
                    actionUserID = Convert.ToInt32(row["UserLoginID"]);
                    actionUserFullName = row["FullName"] != DBNull.Value ? row["FullName"].ToString() : actionUserFullName;
                }

                bool isAuthorized = (targetAssignedId == 0) || (actionUserID != 0 && targetAssignedId == actionUserID);
                if (!isAuthorized && actionUserID != 0)
                {
                    string actionPhase = isCheckPhase ? "check" : "approve";
                    return Request.CreateResponse(HttpStatusCode.Forbidden,
                        new { success = false, message = $"Only {targetAssignedName} can {actionPhase} this Notification Payment." });
                }

                if (isCheckPhase)
                {
                    // ─────────────────────────────────────────────────────────────────────────
                    // STEP 1: CHECK ACTION (In TID 2039 NP Check - Topic 1242)
                    // ─────────────────────────────────────────────────────────────────────────
                    long editChatId = !string.IsNullOrEmpty(chatID) ? long.Parse(chatID) : (row["TelegramGroupID"] != DBNull.Value ? Convert.ToInt64(row["TelegramGroupID"]) : 0);
                    int editMessageId = !string.IsNullOrEmpty(messageID) ? int.Parse(messageID) : (row.Table.Columns.Contains("CheckTelegramMessageID") && row["CheckTelegramMessageID"] != DBNull.Value ? Convert.ToInt32(row["CheckTelegramMessageID"]) : 0);
                    int editTopicId = !string.IsNullOrEmpty(topicID) ? int.Parse(topicID) : (row["TelegramGroupTopicID"] != DBNull.Value ? Convert.ToInt32(row["TelegramGroupTopicID"]) : 0);

                    string userTgTag = !string.IsNullOrEmpty(userName) ? "@" + userName : (row.Table.Columns.Contains("CheckByTgUser") && row["CheckByTgUser"] != DBNull.Value && !string.IsNullOrEmpty(row["CheckByTgUser"].ToString()) ? row["CheckByTgUser"].ToString() : "@user");
                    if (!userTgTag.StartsWith("@")) userTgTag = "@" + userTgTag;

                    if (isDisapprove)
                    {
                        // Check Rejected
                        string updateSql = @"
                            UPDATE dbo.TblNotificationPayment
                            SET CheckStatusID = 91,
                                StatusID = 91,
                                CheckDate = GETDATE(),
                                CheckByTgUser = CASE WHEN ISNULL(@CheckByTgUser, '') <> '' THEN @CheckByTgUser ELSE CheckByTgUser END
                            WHERE NotificationPaymentID = @ID;";
                        var paraUpdate = new Dictionary<string, string>
                        {
                            { "ID", npId.ToString() },
                            { "CheckByTgUser", userTgTag }
                        };
                        html.SaveChangeData(updateSql, false, paraUpdate);

                        if (editChatId != 0 && editMessageId > 0)
                        {
                            string updatedCaption = (message + $"\n\n❌ Rejected by: {userTgTag} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}").TrimEnd();
                            await notification.EditTelegramMessage(
                                messageId: editMessageId,
                                caption: updatedCaption,
                                inlineKeyboard: null,
                                chatId: editChatId,
                                topicID: editTopicId,
                                newBotClient: botToken
                            );
                        }

                        return Request.CreateResponse(HttpStatusCode.OK,
                            new { success = true, message = "Notification Payment Rejected at Check stage." });
                    }
                    else
                    {
                        // Check Approved / Verified
                        string updateSql = @"
                            UPDATE dbo.TblNotificationPayment
                            SET CheckStatusID = 70,
                                CheckDate = GETDATE(),
                                CheckByTgUser = CASE WHEN ISNULL(@CheckByTgUser, '') <> '' THEN @CheckByTgUser ELSE CheckByTgUser END
                            WHERE NotificationPaymentID = @ID;";
                        var paraUpdate = new Dictionary<string, string>
                        {
                            { "ID", npId.ToString() },
                            { "CheckByTgUser", userTgTag }
                        };
                        html.SaveChangeData(updateSql, false, paraUpdate);

                        // Edit original Check message in TID 2039: append Checked and remove buttons
                        if (editChatId != 0 && editMessageId > 0)
                        {
                            string updatedCaption = (message + $"\n\n✍️ Checked by: {userTgTag} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}").TrimEnd();
                            await notification.EditTelegramMessage(
                                messageId: editMessageId,
                                caption: updatedCaption,
                                inlineKeyboard: null,
                                chatId: editChatId,
                                topicID: editTopicId,
                                newBotClient: botToken
                            );
                        }

                        // ── Automatically Dispatch Step 2: Send to TID 2040 (NP Approve - Normal Group) ──
                        string sqlTGApprove = @"
                            SELECT TOP 1 t.TID, t.BotID, t.TelegramGroupID, t.TelegramGroupName, t.TelegramGroupTopicID, b.BotToken
                            FROM dbo.TblTelegramGroupNotification t
                            LEFT JOIN dbo.TblTelegramBots b ON b.BotID = t.BotID
                            WHERE t.TID = 2040";
                        var dtTGApprove = html.LoadData(sqlTGApprove).Tables[0];

                        if (dtTGApprove.Rows.Count > 0)
                        {
                            long tgChatId2 = Convert.ToInt64(dtTGApprove.Rows[0]["TelegramGroupID"]);
                            int tgTopicId2 = 0;
                            if (dtTGApprove.Rows[0]["TelegramGroupTopicID"] != DBNull.Value)
                                int.TryParse(dtTGApprove.Rows[0]["TelegramGroupTopicID"].ToString(), out tgTopicId2);
                            string botToken2 = dtTGApprove.Rows[0]["BotToken"]?.ToString() ?? botToken;

                            string approverTg = row["ApproveByTgUser"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(approverTg) && !approverTg.StartsWith("@"))
                                approverTg = "@" + approverTg;
                            string approvedByName = row["ApproveBy"]?.ToString() ?? "";
                            string requestBy = row["RequestBy"]?.ToString() ?? "";
                            string checkedBy = row.Table.Columns.Contains("CheckBy") && row["CheckBy"] != DBNull.Value ? row["CheckBy"].ToString() : (!string.IsNullOrEmpty(userName) ? "@" + userName : "");
                            string npNo = row["NPNo"]?.ToString() ?? "";

                            var sbMsg2 = new StringBuilder();
                            if (!string.IsNullOrEmpty(approverTg))
                                sbMsg2.AppendLine(approverTg);
                            sbMsg2.AppendLine($"Dear {approvedByName},\n");
                            sbMsg2.AppendLine("Now you have a new Notification Payment need to approve.\n");
                            sbMsg2.AppendLine($"Prepared by: {requestBy}.");
                            sbMsg2.AppendLine($"Checked by: {checkedBy}.\n");
                            if (!string.IsNullOrEmpty(npNo))
                                sbMsg2.AppendLine($"NP No.: {npNo}\n");
                            sbMsg2.AppendLine("Please kindly review and take action, Thanks you.");
                            string caption2 = sbMsg2.ToString().TrimEnd();

                            var inlineKeyboardApprove = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Approve ✅", $"ApproveNP~{npId}~{approverTg}"),
                                    Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Reject ❌", $"RejectNP~{npId}~{approverTg}")
                                }
                            });

                            // Locate generated PDF file for this NP
                            string pdfPath = "";
                            try
                            {
                                string uploadDir = HttpContext.Current.Server.MapPath("~/Uploads/CustomerAttFiles");
                                if (System.IO.Directory.Exists(uploadDir))
                                {
                                    var files = System.IO.Directory.GetFiles(uploadDir, $"*{npNo}*.pdf", System.IO.SearchOption.AllDirectories);
                                    if (files.Length > 0)
                                    {
                                        pdfPath = files[0];
                                    }
                                }
                            }
                            catch { }

                            SqlResult sent2 = null;
                            if (tgTopicId2 > 0)
                            {
                                sent2 = await notification.NotificationTelegramBotToTopic(
                                    MsgText: "",
                                    chatId: tgChatId2,
                                    topicID: tgTopicId2,
                                    inlineKeyboard: inlineKeyboardApprove,
                                    MessageID: 0,
                                    TelegramMessage: caption2,
                                    Files: pdfPath,
                                    Caption: caption2,
                                    EditMessage: false,
                                    DeleteTelegramMessageID: "",
                                    IsReplyMsg: false,
                                    newBotClient: botToken2
                                );
                            }
                            else
                            {
                                sent2 = await notification.NotificationTelegramBot(
                                    MsgText: "",
                                    chatId: tgChatId2,
                                    DestinationFolder: "",
                                    PDFFile: pdfPath,
                                    Caption: caption2,
                                    IsSendFileWithCaption: false,
                                    replyMarkupInlineKeyboardMarkup: true,
                                    DBID: npId,
                                    MessageID: 0,
                                    TelegramMessage: caption2,
                                    inlineKeyboard: inlineKeyboardApprove,
                                    edit: false,
                                    messageThreadId: 0,
                                    DeleteMessageID: "",
                                    newBotClient: botToken2
                                );
                            }

                            int approveMsgId = 0;
                            if (sent2 != null && sent2.Value != null)
                            {
                                int.TryParse(sent2.Value.ToString(), out approveMsgId);
                            }
                            string updateSql2 = @"
                                UPDATE dbo.TblNotificationPayment
                                SET TelegramMessageID = @TelegramMessageID,
                                    TelegramGroupID = @TelegramGroupID,
                                    TelegramGroupTopicID = @TelegramGroupTopicID
                                WHERE NotificationPaymentID = @ID;
                            ";
                            var paraUpdate2 = new Dictionary<string, string>
                            {
                                { "ID", npId.ToString() },
                                { "TelegramMessageID", approveMsgId.ToString() },
                                { "TelegramGroupID", tgChatId2.ToString() },
                                { "TelegramGroupTopicID", tgTopicId2.ToString() }
                            };
                            html.SaveChangeData(updateSql2, false, paraUpdate2);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK,
                            new { success = true, message = "Notification Payment Checked and forwarded to Approver successfully." });
                    }
                }
                else
                {
                   
                    bool isApprove = !isDisapprove;
                    int targetStatusID = isApprove ? 90 : 91;
                    string statusText = isApprove ? "Approved" : "Rejected";
                    string actionIcon = isApprove ? "✅" : "❌";

                    string userTgTag = !string.IsNullOrEmpty(userName) ? "@" + userName : (row["ApproveByTgUser"] != DBNull.Value && !string.IsNullOrEmpty(row["ApproveByTgUser"].ToString()) ? row["ApproveByTgUser"].ToString() : "@user");
                    if (!userTgTag.StartsWith("@")) userTgTag = "@" + userTgTag;

                    // Update dbo.TblNotificationPayment
                    string updateSql = @"
                        UPDATE dbo.TblNotificationPayment
                        SET StatusID = @StatusID,
                            ApproveDate = GETDATE(),
                            ApproveByTgUser = CASE WHEN ISNULL(@ApproveByTgUser, '') <> '' THEN @ApproveByTgUser ELSE ApproveByTgUser END
                        WHERE NotificationPaymentID = @ID;";
                    var paraUpdate = new Dictionary<string, string>
                    {
                        { "ID", npId.ToString() },
                        { "StatusID", targetStatusID.ToString() },
                        { "ApproveByTgUser", userTgTag }
                    };
                    html.SaveChangeData(updateSql, false, paraUpdate);

                    long editChatId = !string.IsNullOrEmpty(chatID) ? long.Parse(chatID) : (row["TelegramGroupID"] != DBNull.Value ? Convert.ToInt64(row["TelegramGroupID"]) : 0);
                    int editMessageId = !string.IsNullOrEmpty(messageID) ? int.Parse(messageID) : (row["TelegramMessageID"] != DBNull.Value ? Convert.ToInt32(row["TelegramMessageID"]) : 0);
                    int editTopicId = !string.IsNullOrEmpty(topicID) ? int.Parse(topicID) : (row["TelegramGroupTopicID"] != DBNull.Value ? Convert.ToInt32(row["TelegramGroupTopicID"]) : 0);

                    // Edit Approval Telegram message: update caption & remove buttons
                    if (editChatId != 0 && editMessageId > 0)
                    {
                        string updatedCaption = (message + $"\n\n{actionIcon} {statusText} by: {userTgTag} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}").TrimEnd();

                        await notification.EditTelegramMessage(
                            messageId: editMessageId,
                            caption: updatedCaption,
                            inlineKeyboard: null,
                            chatId: editChatId,
                            topicID: editTopicId,
                            newBotClient: botToken
                        );
                    }

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = $"Notification Payment {statusText} successfully." });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { success = false, message = ex.Message });
            }
        }
    }
}
