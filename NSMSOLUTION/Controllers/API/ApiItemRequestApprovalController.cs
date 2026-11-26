using MVC;
using NSMSOLUTION.Controllers.Inventory;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    [Authorize]
    [RoutePrefix("api")]
    public class ApiItemRequestApprovalController : ApiController
    {
        [Route("ItemRequestApproval")]
        public async Task<HttpResponseMessage> ItemRequestApproval()
        {
            HTML html = new HTML();
            NotificationTelegram notification = new NotificationTelegram();
            var formData = HttpContext.Current.Request.Form;

            try
            {
                int reqId = int.Parse(formData["RequestItemID"]);
                string actionStatus = formData["Status"];
                string tlgUsername = formData["Username"];
                long groupId = Utility.TryParse(formData["TelegramGroupID"], "long", 0);
                int groupTopicId = Utility.TryParse(formData["TelegramGroupTopicID"], "int", 0); ;
                string tglMsg = formData["TelegramMessage"];
                int tlgMsgId = Utility.TryParse(formData["TelegramMessageID"], "int", 0);

                //** Upate & Get the data item request
                string sql = "select * from dbo.TblItem_Request where RequestItemID = @RequestItemID;";
                var dicPara = new Dictionary<string, string>()
                {
                    { "RequestItemID", reqId.ToString() },
                };

                DataSet ds = html.LoadData(sql, false, dicPara);
                if (ds != null && ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Request item not found.");

                DataRow row = ds.Tables[0].Rows[0];
                int systemId = int.Parse(row["SystemID"].ToString());
                string botToken = html.GetBotTokenBySystemID(systemId);
                string reqType = row["RequestMode"].ToString();
                string receiver = row["ReceivedTelegaramUserName"].ToString();

                string refNo = reqType == "Item Request"
                    ? row["RequestNo"].ToString()
                    : row["RequestApprovalNo"].ToString();

                bool isApproved = actionStatus == "Approved";
                bool isConfirmReceived = actionStatus == "ConfirmReceived";

                if (isConfirmReceived)
                {
                    sql = @"
                        update dbo.TblItem_Request set 
                            ReceivedDate = GETDATE(), 
                            ReceivedTelegaramUserName = @ReceivedTelegaramUserName, 
                            IsReceived = 1, 
                            IsWaitingReceived = 0, 
                            IsWaitingConfirmedReceived = 0, 
                            WaitingConfirmedReceivedStatus = 'Received' 
                        where RequestItemID = @RequestItemID;
                    ";

                    dicPara["ReceivedTelegaramUserName"] = tlgUsername;
                }
                else
                {
                    if (reqType == "Item Request")
                    {
                        sql = @"
                        update dbo.TblItem_Request set 
                            Status = 'Approved', 
                            ApproveDisapproveDate = GETDATE(), 
                            IsRequestApprovalItemOut = IsRequestApprovalItemOut, 
                            RequestApprovalItemOutStatus = @RequestApprovalItemOutStatus, 
                            IsWaitingConfirmedReceived = @IsWaitingConfirmedReceived, 
                            WaitingConfirmedReceivedStatus = @WaitingConfirmedReceivedStatus 
                        where RequestItemID = @RequestItemID;
                    ";

                        dicPara["IsRequestApprovalItemOut"] = isApproved ? "1" : "0";
                        dicPara["RequestApprovalItemOutStatus"] = isApproved ? "Pending" : null;
                        dicPara["IsWaitingConfirmedReceived"] = isApproved ? "1" : "0";
                        dicPara["WaitingConfirmedReceivedStatus"] = isApproved
                            ? "Waiting Stock Confirm"
                            : null;
                    }
                    else
                    {
                        sql = $@"declare @ReferenceRequestItemID int, @TelegramGroupID nvarchar(50), @TelegramGroupMessageID int, @FromTelegramGroupID nvarchar(50), @FromTelegramGroupMessageID int;
select @ReferenceRequestItemID = ReferenceRequestItemID, @TelegramGroupID = TelegramGroupID,@TelegramGroupMessageID=TelegramGroupMessageID, @FromTelegramGroupID = FromTelegramGroupID,@FromTelegramGroupMessageID=FromTelegramGroupMessageID  from dbo.TblItem_Request where RequestItemID = @RequestItemID;
select @TelegramGroupID = TelegramGroupID,@TelegramGroupMessageID=TelegramGroupMessageID, @FromTelegramGroupID = FromTelegramGroupID,@FromTelegramGroupMessageID=FromTelegramGroupMessageID  from dbo.TblItem_Request where RequestItemID = @ReferenceRequestItemID;
update TblItem_Request set IsWaitingReceived = {(isApproved ? 1 : 0)}, WaitingConfirmedReceivedStatus='Waiting Confirm Received' where RequestItemID = @ReferenceRequestItemID;
update dbo.TblItem_Request set [Status] = '{actionStatus}', ApprovedRequestDate = getdate()";
                        if (isApproved)
                        {
                            sql += ", IsWaitingConfirmedReceived = 1, IsRequestApprovalItemOut = 1, RequestApprovalItemOutStatus = 'Approved', IsWaitingReceived = 1, WaitingConfirmedReceivedStatus='Waiting Confirm Received', ApproveDisapproveDate = getdate()";
                            sql += ", TelegramGroupID = @TelegramGroupID, TelegramGroupMessageID = @TelegramGroupMessageID, FromTelegramGroupID = @FromTelegramGroupID, FromTelegramGroupMessageID = @FromTelegramGroupMessageID ";
                        }
                        else
                        {
                            sql += ", RequestApprovalItemOutStatus = 'Disapproved', ApproveDisapproveDate = getdate()";
                        }
                        sql += " where RequestItemID = @RequestItemID";

                        dicPara["ApprovedRequestDate"] = html.GetCurrentDate();
                    }
                }

                var result = html.SaveChangeData(sql, false, dicPara);

                if (result.Result == ResultType.Failed)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError,
                        new { success = false, message = result.Message });
                }

                string actionIcon = isApproved || isConfirmReceived ? "✅" : "❌";
                string updatedCaption = tglMsg
                    + $"\n\n{actionIcon} <i><b>{(isConfirmReceived ? "Confirmed Received" : actionStatus)} by: @{tlgUsername} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}</b></i>";

                var itemCtl = new ItemsController();
                var pdfResult = await itemCtl.GeneratePDF(reqId, reqType, refNo);

                var editMessag = await notification.EditTelegramMessage(
                    messageId: tlgMsgId,
                    caption: updatedCaption,
                    inlineKeyboard: null,
                    chatId: groupId,
                    topicID: groupTopicId,
                    filePath: pdfResult?.OutPutFile,
                    newBotClient: botToken
                );

                if (editMessag.Result == ResultType.Successful)
                {
                    System.IO.File.Delete(pdfResult.OutPutFile);

                    if (isConfirmReceived)
                    {
                        await itemCtl.SaveChangeConfirmItemRequest(reqId);
                    }
                    else
                    {
                        if (reqType != "Item Request")
                        {
                            var msgReceived = new StringBuilder();
                            msgReceived.AppendLine($"📦 Item Request Issued\n");
                            msgReceived.AppendLine($"Hello {receiver},\n");
                            msgReceived.AppendLine($"Your requested item has been issued by the Stock Department.\n");
                            msgReceived.AppendLine($"Please confirm that you have received the item by clicking the button below. 👇\n");

                            var inlineBtn = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(
                                        text: "✅ Confirm Item Received",
                                        callbackData: $"ConfirmReceivedItemRequest~{reqId}~{receiver}"
                                    )
                                }
                            });
                            var notificationTelegram = new NotificationTelegram();
                            result = await notificationTelegram.NotificationTelegramBotToTopic(
                                msgReceived.ToString(),
                                chatId: groupId,
                                topicID: groupTopicId,
                                inlineKeyboard: inlineBtn,
                                MessageID: int.Parse(editMessag.Value),
                                EditMessage: false,
                                IsReplyMsg: true,
                                newBotClient: botToken
                            );

                        }
                    }

                    sql = @"
                        UPDATE dbo.TblItem_Request 
                        SET
                            TelegramGroupID = @TelegramGroupID, 
                            TelegramGroupTopicID = @TelegramGroupTopicID, 
                            TelegramMessage = @TelegramMessage, 
                            TelegramMessage = @TelegramMessage 
                        WHERE RequestItemID = @RequestItemID;
                    ";

                    dicPara.Add("@TelegramGroupID", groupId.ToString());
                    dicPara.Add("@TelegramGroupTopicID", groupTopicId.ToString());
                    dicPara.Add("@TelegramMessageID", editMessag.Value.ToString());
                    dicPara.Add("@TelegramMessage", updatedCaption);

                    html.SaveChangeData(sql, false, dicPara);
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { success = false, message = ex.Message });
            }
        }
        [Route("StockReturnApproval")]
        public async Task<HttpResponseMessage> StockReturnApproval()
        {
            HTML html = new HTML();
            NotificationTelegram notification = new NotificationTelegram();
            var formData = HttpContext.Current.Request.Form;

            try
            {
                int reqId = int.Parse(formData["RequestItemID"]);
                string actionStatus = formData["Status"];
                string tlgUsername = formData["Username"];
                long groupId = Utility.TryParse(formData["TelegramGroupID"], "long", 0);
                int groupTopicId = Utility.TryParse(formData["TelegramGroupTopicID"], "int", 0); ;
                string tglMsg = formData["TelegramMessage"];
                int tlgMsgId = Utility.TryParse(formData["TelegramMessageID"], "int", 0);

                //** Upate & Get the data item request
                string sql = "select * from dbo.TblItem_Request where RequestItemID = @RequestItemID;";
                var dicPara = new Dictionary<string, string>()
                {
                    { "RequestItemID", reqId.ToString() },
                };

                DataSet ds = html.LoadData(sql, false, dicPara);
                if (ds != null && ds.Tables[0].Rows.Count == 0)
                    throw new Exception("Request item not found.");

                DataRow row = ds.Tables[0].Rows[0];
                int systemId = int.Parse(row["SystemID"].ToString());
                string botToken = html.GetBotTokenBySystemID(systemId);
                string reqType = row["RequestMode"].ToString();
                string refNo = reqType == "Request Stock Return"
                    ? row["RequestReturnNo"].ToString()
                    : row["RequestApprovalNo"].ToString();
                string receiver = row["ReceivedTelegaramUserName"].ToString();

                string reqMode = reqType == "Request Stock Return"
                    ? "request return"
                    : "issue stock return";

                bool isApproved = actionStatus == "Approved";
              
                if (reqType == "Request Stock Return")
                {
                    sql = @"
                        update dbo.TblItem_Request set 
                            Status = 'Approved', 
                            ApproveDisapproveDate = GETDATE(), 
                            IsWaitingConfirmedReceived = @IsWaitingConfirmedReceived, 
                            WaitingConfirmedReceivedStatus = @WaitingConfirmedReceivedStatus 
                        where RequestItemID = @RequestItemID;
                    ";

                    dicPara["IsWaitingConfirmedReceived"] = isApproved ? "1" : "0";
                    dicPara["WaitingConfirmedReceivedStatus"] = isApproved
                        ? "Waiting Stock Confirm"
                        : null;
                }
                else
                {
                    sql = $@"declare @ReferenceRequestItemID int, @TelegramGroupID nvarchar(50), @TelegramGroupMessageID int, @FromTelegramGroupID nvarchar(50), @FromTelegramGroupMessageID int;
select @ReferenceRequestItemID = ReferenceRequestItemID, @TelegramGroupID = TelegramGroupID,@TelegramGroupMessageID=TelegramGroupMessageID, @FromTelegramGroupID = FromTelegramGroupID,@FromTelegramGroupMessageID=FromTelegramGroupMessageID  from dbo.TblItem_Request where RequestItemID = @RequestItemID;
select @TelegramGroupID = TelegramGroupID,@TelegramGroupMessageID=TelegramGroupMessageID, @FromTelegramGroupID = FromTelegramGroupID,@FromTelegramGroupMessageID=FromTelegramGroupMessageID  from dbo.TblItem_Request where RequestItemID = @ReferenceRequestItemID;
update TblItem_Request set IsWaitingReceived = {(isApproved ? 1 : 0)}, WaitingConfirmedReceivedStatus='Waiting Confirm Received' where RequestItemID = @ReferenceRequestItemID;
update dbo.TblItem_Request set [Status] = '{actionStatus}', ApprovedRequestDate = getdate()";
                    if (isApproved)
                    {
                        sql += ", IsWaitingConfirmedReceived = 1, IsRequestApprovalItemOut = 1, RequestApprovalItemOutStatus = 'Approved', IsWaitingReceived = 1, WaitingConfirmedReceivedStatus='Waiting Confirm Received', ApproveDisapproveDate = getdate()";
                        sql += ", TelegramGroupID = @TelegramGroupID, TelegramGroupMessageID = @TelegramGroupMessageID, FromTelegramGroupID = @FromTelegramGroupID, FromTelegramGroupMessageID = @FromTelegramGroupMessageID ";
                    }
                    else
                    {
                        sql += ", RequestApprovalItemOutStatus = 'Disapproved', ApproveDisapproveDate = getdate()";
                    }
                    sql += " where RequestItemID = @RequestItemID";

                    dicPara["ApprovedRequestDate"] = html.GetCurrentDate();
                }

                var result = html.SaveChangeData(sql, false, dicPara);

                if (result.Result == ResultType.Failed)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError,
                        new { success = false, message = result.Message });
                }

                string actionIcon = isApproved ? "✅" : "❌";
                string updatedCaption = tglMsg
                    + $"\n\n{actionIcon} <i><b>{(actionStatus)} by: @{tlgUsername} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}</b></i>";

                var itemCtl = new ItemsController();
                var pdfResult = await itemCtl.GeneratePDF(reqId, reqType, refNo);

                var editMessag = await notification.EditTelegramMessage(
                    messageId: tlgMsgId,
                    caption: updatedCaption,
                    inlineKeyboard: null,
                    chatId: groupId,
                    topicID: 0,
                    filePath: pdfResult?.OutPutFile,
                    newBotClient: botToken
                );

                if (isApproved && editMessag.Result == ResultType.Successful)
                {
                    System.IO.File.Delete(pdfResult.OutPutFile);

                    if (reqType == "Stock Return")
                    {
                        itemCtl.SaveItemReturnStockIn(reqId);
                    }

                    sql = @"
                        UPDATE dbo.TblItem_Request 
                        SET
                            TelegramGroupID = @TelegramGroupID, 
                            TelegramGroupTopicID = @TelegramGroupTopicID, 
                            TelegramMessage = @TelegramMessage, 
                            TelegramMessage = @TelegramMessage 
                        WHERE RequestItemID = @RequestItemID;
                    ";

                    dicPara.Add("@TelegramGroupID", groupId.ToString());
                    dicPara.Add("@TelegramGroupTopicID", groupTopicId.ToString());
                    dicPara.Add("@TelegramMessageID", editMessag.Value.ToString());
                    dicPara.Add("@TelegramMessage", updatedCaption);

                    html.SaveChangeData(sql, false, dicPara);
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { success = false, message = ex.Message });
            }
        }
    }
}