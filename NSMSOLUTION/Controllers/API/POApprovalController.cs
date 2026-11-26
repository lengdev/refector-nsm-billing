using MVC;
using NSMSOLUTION.Controllers.Vendor;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    [Authorize]
    [RoutePrefix("api")]
    public class POApprovalController : ApiController
    {
        [HttpPost]
        [Route("POApproval")]
        public async Task<HttpResponseMessage> POApproval()
        {
            try
            {
                HTML html = new HTML();
                int poId = int.Parse(HttpContext.Current.Request.Form["POID"]?.ToString());
                string userName = HttpContext.Current.Request.Form["UserName"]?.ToString();
                string reqStatus = HttpContext.Current.Request.Form["POStatus"]?.ToString();
                string cbStatus = HttpContext.Current.Request.Form["CallBackStatus"]?.ToString();
                long chatID = long.Parse(HttpContext.Current.Request.Form["ChatID"]?.ToString());
                int messageID = int.Parse(HttpContext.Current.Request.Form["MessageID"]?.ToString());
                string message = HttpContext.Current.Request.Form["Message"]?.ToString();

                string resolverSql = "";

                if (reqStatus == "CheckPO")
                {
                    resolverSql = @"select s.*, s.NeedToCheckUserID AS ApproverID, s.NeedToCheckBy AS ApproverName, s.CheckUsername AS ApproverUsername,
                        s.ApproveByUserID as RecipientID, s.NeedToApproval AS Recipient, s.TelegramUserName as RecipientUsername
                        from dbo.TblPurchaseOrder s
                        where s.PurchaseOrderID = @PurchaseOrderID";
                }
                else if (reqStatus == "ApprovePO")
                {
                    resolverSql = @"select s.*, s.ApproveByUserID AS ApproverID, s.NeedToApproval AS ApproverName, 
                        case when (s.DraftPOStatusID = 90) then s.TelegramUserName else s.DraftCheckUsername end AS ApproverUsername,
                        case when (s.DraftPOStatusID = 90 or s.NeedToCheckUserID = 0) then s.ApproveByUserID else s.NeedToCheckUserID end as RecipientID, 
                        case when (s.DraftPOStatusID = 90 or s.NeedToCheckUserID = 0) then  s.NeedToApproval else s.NeedToCheckBy end AS Recipient, 
                        case when (s.DraftPOStatusID = 90 or s.NeedToCheckUserID = 0) then s.TelegramUserName else s.CheckUsername end as RecipientUsername
                        from dbo.TblPurchaseOrder s 
                        where s.PurchaseOrderID = @PurchaseOrderID";
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest,
                        new { success = false, message = "Unknown Status." });
                }

                var para = new Dictionary<string, string> { { "@PurchaseOrderID", poId.ToString() } };
                DataSet ds = html.LoadData(resolverSql, false, para);

                if (ds.Tables[0].Rows.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.NotFound,
                        new { success = false, message = "PO record not found." });

                DataRow row = ds.Tables[0].Rows[0];

                string PONo = row["PONumber"].ToString();
                int statusId = int.Parse(row["StatusID"].ToString());
                bool isReqEdit = statusId == 90 && !string.IsNullOrEmpty(row["RequestEditApproveBy"].ToString());
                bool isNeedCheck = !string.IsNullOrEmpty(row["NeedToCheckBy"]?.ToString());
                int approverId = int.Parse(row["ApproverID"].ToString());
                string preparedBy = row["CreateBy"].ToString();
                string approver = row["ApproverName"].ToString();
                int recipientId = int.Parse(row["RecipientID"].ToString());
                string recipient = row["Recipient"].ToString();
                string approverUserName = isReqEdit
                    ? row["RequestEditByUsername"].ToString()
                    : row["ApproverUsername"].ToString();
                string recipientUsername = row["RecipientUsername"].ToString();
                string systemID = row["SystemID"].ToString();
                string vendorId = row["VendorID"].ToString();

                string botToken = html.GetBotTokenBySystemID(int.Parse(systemID));

                approver = approverId == 38 ? "Boss" : approver;

                bool isCheck = false;
                bool isApprove = false;
                bool isApproveDraft = false;
                bool isApproveEdit = false;
                bool isDisApprove = false;
                bool isDisApproveDraft = false;

                int newStatusId;
                string resolvedStatus;
                string sql = "";

                if (reqStatus == "CheckPO")
                {
                    isCheck = cbStatus == "CheckPO";
                    isDisApprove = cbStatus == "DisCheckPO";

                    newStatusId = isCheck ? 92 : 91;
                    resolvedStatus = isCheck ? "Checked" : "Disapproved";
                }
                else
                {
                    isApprove = cbStatus == "ApprovePO";
                    isApproveDraft = cbStatus == "ApproveDraftPO";
                    isApproveEdit = cbStatus == "ApproveEditPO";

                    isDisApprove = cbStatus == "DisapprovePO";
                    isDisApproveDraft = cbStatus == "DisapproveDraftPO";

                    bool isApprovedGroup = isApprove || isApproveDraft || isApproveEdit;

                    newStatusId = isApprovedGroup ? 90 : 91;
                    resolvedStatus = isApprovedGroup ? "Approved" : "Disapproved";
                }

                if (chatID < 0 && messageID > 0)
                {
                    string actionIcon = newStatusId == 90 ? "✅" : newStatusId == 92 ? "✍️" : "❌";

                    const int BossUserId = 38;

                    bool showAsBoss = recipientId == BossUserId
                                      && reqStatus != "CheckPO"
                                      && (isApprove || isDisApprove);

                    string actor = showAsBoss ? "Boss" : $"@{userName}";
                    string timestamp = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

                    string updatedCaption =
                        $"{message}\n\n" +
                        $"{actionIcon} <i><b>{resolvedStatus} by: {actor} at {timestamp}</b></i>";

                    var notification = new NotificationTelegram();

                    para.Clear();
                    para.Add("@ID", poId.ToString());
                    para.Add("@ApproverID", approverId.ToString());
                    para.Add("@Approver", approver);
                    para.Add("@StatusID", newStatusId.ToString());
                    para.Add("@POStatus", newStatusId.ToString());
                    para.Add("@Date", DateTime.Now.ToString());
                    para.Add("@AprrovalFrom", "Telegram");

                    var poCtrl = new PurchaseOrderController();
                    var msg = new StringBuilder();
                    var result = new SqlResult();
                    var sentResult = new SqlResult();
                    var pdfResult = new SqlResult();

                    if (!isDisApprove)
                    {
                        sql = @"SELECT * 
                FROM dbo.TblTelegramGroupNotification
                WHERE [Module] = 'PO' AND StatusID = @SID AND SystemID = @SystemID";
                        para.Add("@SID", isNeedCheck && isApproveDraft ? "92" : "90");
                        para.Add("@SystemID", systemID);
                        ds = html.LoadData(sql, false, para);
                        var telRow = ds.Tables[0].Rows[0];

                        if (telRow == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError,
                                new { success = false, message = "Telegram group for this action is not configured." });
                        }

                        long newChatID = long.Parse(telRow["TelegramGroupID"].ToString());

                        if (!isCheck)
                        {
                            var inlineKeyboard = new InlineKeyboardMarkup();

                            inlineKeyboard = isNeedCheck
                                ? new InlineKeyboardMarkup(new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Check ✍️", $"CheckPO~{poId}~{recipientUsername}"),
                                        InlineKeyboardButton.WithCallbackData("Disapprove ❌", $"DisCheckPO~{poId}~{recipientUsername}")
                                    }
                                })
                                : new InlineKeyboardMarkup(new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"ApprovePO~{poId}~{recipientUsername}"),
                                        InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisapprovePO~{poId}~{recipientUsername}"),
                                    }
                                });

                            if (isApprove)
                            {
                                sql = @"UPDATE dbo.TblPurchaseOrder 
                                            SET StatusID = @StatusID, POStatus = @POStatus, 
                                                ApproveBy = @Approver,
                                                ApprovalDate = @Date, AprrovalFrom = @AprrovalFrom
                                        WHERE PurchaseOrderID = @ID;";
                            }
                            else if (isApproveDraft || isDisApproveDraft)
                            {
                                sql = @"UPDATE dbo.TblPurchaseOrder 
                                        SET DraftPOStatusID = @StatusID
                                    WHERE PurchaseOrderID = @ID;";
                            }
                            else
                            {
                                sql = @"UPDATE dbo.TblPurchaseOrder 
                                        SET RequestEditStatusID = @StatusID ";
                                if (isApproveEdit)
                                    sql += ",StatusID = 60, POStatus = 60 ";
                                sql += "WHERE PurchaseOrderID = @ID;";
                            }

                            result = html.SaveChangeData(sql, false, para);
                            if(result.Result == ResultType.Successful) 
                                pdfResult = await poCtrl.GeneratePDF(vendorId, poId, PONo);

                            // Update and edit message to show approved status with PDF for approver and recipient
                            sentResult = await notification.EditTelegramMessage(
                                messageId: messageID,
                                caption: updatedCaption,
                                inlineKeyboard: null,
                                chatId: chatID,
                                topicID: 0,
                                filePath: isApprove ? pdfResult?.OutPutFile : "",
                                newBotClient: botToken
                            );

                            if (isApprove)
                            {
                                // Alert to stock in group
                                sql = @"select case when exists(
                                          select 1 from dbo.TblPurchaseOrderDetail pd
                                          inner join dbo.TblItem i on i.ItemID = pd.ItemID and i.ItemTypeID = 4
                                          where pd.PurchaseOrderID = @ID
                                        ) then 1 else 0 end as IsHasInventory;";
                                ds = html.LoadData(sql, false, para);
                                row = ds.Tables[0].Rows[0];
                                bool isInventory = row["IsHasInventory"].ToString() == "1";

                                if (isInventory)
                                {
                                    msg.Clear();
                                    msg.AppendLine($"✅ <b>NEW PO APPROVED</b>\n");
                                    msg.AppendLine($"<i>PO#: {PONo}</i>\n");
                                    msg.AppendLine($"Please proceed with stock entry.\n");

                                    long stockChatID = systemID == "13" ? -4019202888 : -5123119304;
                                    sentResult = await notification.NotificationTelegramBot(
                                        "",
                                        chatId: stockChatID,
                                        PDFFile: pdfResult?.OutPutFile,
                                        Caption: msg.ToString(),
                                        edit: false,
                                        newBotClient: botToken
                                    );
                                }
                            }
                            else if (isApproveDraft)
                            {
                                msg.Clear();
                                msg.AppendLine($"{recipientUsername}\n");
                                msg.AppendLine($"Dear {(recipientId == 38 ? "Boss" : recipient)},\n");
                                msg.AppendLine($"Now you have a new po need to {(isNeedCheck ? "check" : "approve")}.\n");
                                msg.AppendLine($"<i><b>Prepared by: {preparedBy}</b></i>.\n");
                                msg.AppendLine($"Please kindly review and take action, Thanks you.");

                                chatID = newChatID;

                                sentResult = await notification.NotificationTelegramBot(
                                    "",
                                    chatId: chatID,
                                    PDFFile: pdfResult?.OutPutFile,
                                    Caption: msg.ToString(),
                                    replyMarkupInlineKeyboardMarkup: true,
                                    inlineKeyboard: inlineKeyboard,
                                    newBotClient: botToken
                                );
                            }

                        }
                        else
                        {
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"ApprovePO~{poId}~{recipientUsername}"),
                                    InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisapprovePO~{poId}~{recipientUsername}"),
                                }
                            });

                            sql = @"UPDATE dbo.TblPurchaseOrder 
                                            SET StatusID = @StatusID, POStatus = @POStatus, 
                                                CheckVerifyBy = @Approver, CheckVerifyByUserID = @ApproverID, 
                                                CheckVerifyDate = @Date, AprrovalFrom = @AprrovalFrom
                                        WHERE PurchaseOrderID = @ID;";

                            result = html.SaveChangeData(sql, false, para);
                            if (result.Result == ResultType.Successful)
                                pdfResult = await poCtrl.GeneratePDF(vendorId, poId, PONo);

                            await notification.EditTelegramMessage(
                                messageId: messageID,
                                caption: updatedCaption,
                                inlineKeyboard: null,
                                chatId: chatID,
                                topicID: 0,
                                filePath: pdfResult?.OutPutFile,
                                newBotClient: botToken
                            );

                            msg.Clear();
                            msg.AppendLine($"{recipientUsername}\n");
                            msg.AppendLine($"Dear {(recipientId == 38 ? "Boss" : recipient)},\n");
                            msg.AppendLine($"Now you have a new po need to approve.\n");
                            msg.AppendLine($"<i><b>Prepared by: {preparedBy}</b></i>.\n");
                            msg.AppendLine($"Please kindly review and take action, Thanks you.");

                            chatID = newChatID;

                            sentResult = await notification.NotificationTelegramBot(
                                "",
                                chatId: chatID,
                                PDFFile: pdfResult?.OutPutFile,
                                Caption: msg.ToString(),
                                replyMarkupInlineKeyboardMarkup: true,
                                inlineKeyboard: inlineKeyboard,
                                newBotClient: botToken
                            );
                        }

                        sql = @"update dbo.TblPurchaseOrder set TelegramMessageID = @TelegramMessageID, TelegramGroupID = @TelegramGroupID, TelegramMessage = @TelegramMessage where PurchaseOrderID = @ID;";
                        para.Clear();
                        para.Add("@ID", poId.ToString());
                        para.Add("@TelegramGroupID", chatID.ToString());
                        para.Add("@TelegramMessage", sentResult.Caption);
                        para.Add("@TelegramMessageID", sentResult.Value);
                        html.SaveChangeData(sql, false, para);
                    }
                    else
                    {
                        await notification.EditTelegramMessage(
                            messageId: messageID,
                            caption: updatedCaption,
                            inlineKeyboard: null,
                            chatId: chatID,
                            topicID: 0,
                            newBotClient: botToken
                        );

                        if (isDisApprove)
                        {
                            sql = @"UPDATE dbo.TblPurchaseOrder 
                                    SET StatusID = @StatusID, POStatus = @POStatus, 
                                        DisapproveBy = @Approver, DisapproveByUserID = @Approver, 
                                        ApprovalDate = @Date, AprrovalFrom = @AprrovalFrom
                                WHERE PurchaseOrderID = @ID;";
                        }

                        html.SaveChangeData(sql, false, para);
                    }

                    if (sentResult.Result == ResultType.Successful)
                    {
                        File.Delete(pdfResult?.OutPutFile);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { success = false, message = ex.Message });
            }
        }
    }
}