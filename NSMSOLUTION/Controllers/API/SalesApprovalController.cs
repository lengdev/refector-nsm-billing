using MVC;
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
    public class SalesApprovalController : ApiController
    {
        [HttpPost]
        [Route("SalesApproval")]
        public async Task<HttpResponseMessage> SalesApproval()
        {
            try
            {
                var html = new HTML();
                var form = HttpContext.Current.Request.Form;

                int salesId = Utility.TryParse(form["SalesID"], "int", 0);
                string userName = form["UserName"];
                string action = form["ActionCallBack"];
                string statusKey = form["StatusKey"];
                long chatId = Utility.TryParse(form["TelegramGroupID"], "long", 0);
                int messageId = Utility.TryParse(form["TelegramMessageID"], "int", 0);
                int topicId = Utility.TryParse(form["TelegramGroupTopicID"], "int", 0);
                string message = form["TelegramMessage"];
                string approvalFrom = form["ApprovalFrom"];

                string resolverSql = action == "Check"
                    ? @"select *, s.NeedCheckByID AS ApproverID, s.NeedCheckBy AS ApproverName, s.CheckByUsername AS ApproverUsername,
                        s.NeedApproveByID as RecipientID, s.NeedApproveBy AS Recipient, s.ApproveUsername as RecipientUsername
                        from dbo.TblSales s 
                        where s.SalesID = @SalesID"
                    : @"select *, s.NeedApproveByID AS ApproverID, s.NeedApproveBy AS ApproverName, s.ApproveUsername AS ApproverUsername,
                        s.NeedApproveByID as RecipientID, s.NeedApproveBy AS Recipient, s.ApproveUsername as RecipientUsername
                        from dbo.TblSales s
                        where s.SalesID = @SalesID";
                var para = new Dictionary<string, string> { { "@SalesID", salesId.ToString() } };
                DataSet ds = html.LoadData(resolverSql, false, para);

                if (ds.Tables[0].Rows.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.NotFound,
                        new { success = false, message = "Sales record not found." });

                DataRow row = ds.Tables[0].Rows[0];
                string saleType = row["Type"]?.ToString();
                string referenceNo = saleType == "SalesOrder" ? row["SONumber"]?.ToString() :
                                saleType == "Quotation" ? row["QuoteNumber"]?.ToString() :
                                saleType == "Invoices" ? row["InvoiceNumber"]?.ToString() :
                                row["DNNumber"]?.ToString();
                string requestType = saleType == "SalesOrder" ? "sales order" :
                                saleType == "Quotation" ? "quotation" :
                                saleType == "Invoices" ? "invoice" :
                                "delivery note";

                int approverId = int.Parse(row["ApproverID"].ToString());
                int statusId = int.Parse(row["StatusID"].ToString());
                string preparedBy = row["CreateBy"].ToString();
                string customerName = row["CustomerName"].ToString();
                string approver = row["ApproverName"].ToString();
                int recipientId = int.Parse(row["RecipientID"].ToString());
                string recipient = row["Recipient"].ToString();
                string approverUsername = action == "RequestEdit"
                    ? row["ReqEditApproveByUsername"].ToString()
                    : row["ApproverUsername"].ToString();
                string recipientUsername = row["RecipientUsername"].ToString();
                string systemID = row["SystemID"].ToString();
                string className = row["ClassName"].ToString();
                string note = row["SaleNote"].ToString();
                bool isNonVAT = Utility.TryParse(row["IsNonVAT"]?.ToString(), "bool", false);

                string botToken = html.GetBotTokenBySystemID(int.Parse(systemID));

                approver = approverId == 38 ? "Boss" : approver;

                if (approvalFrom == "System")
                {
                    chatId = Utility.TryParse(row["TelegramGroupID"]?.ToString(), "long", 0);
                    topicId = Utility.TryParse(row["TelegramGroupTopicID"]?.ToString(), "int", 0);
                    messageId = Utility.TryParse(row["TelegramMessageID"]?.ToString(), "int", 0);
                    message = row["TelegramMessage"]?.ToString();
                }

                bool isApprove = false;
                bool isCheck = false;
                bool isRequestEdit = false;
                string resolvedStatus = "";
                string sql = "";

                if (action == "Check")
                {
                    isCheck = statusKey == "CheckSales";
                    resolvedStatus = isCheck ? "Checked" : "Disapproved";
                }
                else if (action == "Approve")
                {
                    isApprove = statusKey == "ApproveSales";
                    resolvedStatus = isApprove ? "Approved" : "Disapproved";
                }
                else if (action == "RequestEdit")
                {
                    isRequestEdit = statusKey == "ApproveRequestEdit";
                    resolvedStatus = isRequestEdit ? "Approved" : "Disapproved";
                }


                if (chatId < 0 && messageId > 0)
                {
                    string actionIcon = isApprove || isRequestEdit ? "✅" : isCheck ? "✍️" : "❌";

                    string updatedCaption = message
                        + $"\n\n{actionIcon} <i><b>{resolvedStatus} by: {(isApprove && recipientId == 38 ? "Boss" : "@" + userName)} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}</b></i>";

                    var notification = new NotificationTelegram();

                    para.Add("StatusID", isCheck ? "92" : isApprove ? "90" : "91");
                    para.Add("Type", saleType);
                    para.Add("ApprovalByID", approverId.ToString());
                    para.Add("ApprovalBy", approver);
                    para.Add("ConfirmApproval", "1");
                    para.Add("ApprovalDate", DateTime.Now.ToString());
                    para.Add("Comment", "");
                    para.Add("ApprovalFrom", "");

                    var controller = new ProfileController();
                    var msg = new StringBuilder();
                    var sentResult = new SqlResult();

                    if (isApprove || isCheck)
                    {
                        if (isApprove)
                        {
                            sql = @"update dbo.TblSales set StatusID = @StatusID, ApproveBy = @ApprovalBy, 
                        ApprovalFrom = @ApprovalFrom, ApproveByID = @ApprovalByID, ApproveDate = @ApprovalDate, IsApprove = @ConfirmApproval
                        where SalesID = @SalesID and Type = @Type;";

                            html.SaveChangeData(sql, false, para);

                            dynamic frm = controller.InvoiceSaleOrderTMP(salesId, saleType, false, true, approvalFrom: approvalFrom);
                            string fileName = html.ToUrlSlug(referenceNo);
                            string destination = $"Uploads/Sales/{saleType}/" + html.ToUrlSlug(customerName);
                            var pdfResult = await html.HTMLToPDF(frm.Data.Data, destination, fileName);

                            sentResult = await notification.EditTelegramMessage(
                                messageId: messageId,
                                caption: updatedCaption,
                                inlineKeyboard: null,
                                chatId: chatId,
                                topicID: topicId,
                                filePath: pdfResult.OutPutFile,
                                newBotClient: botToken
                            );

                            if (sentResult.Result == ResultType.Successful)
                            {
                                System.IO.File.Delete(pdfResult.OutPutFile);
                            }
                        }
                        else if (isCheck)
                        {
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"ApproveSales~{salesId}~{recipientUsername}"),
                                    InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisapproveSales~{salesId}~{recipientUsername}"),
                                }
                            });

                            sql = @"update dbo.TblSales set StatusID = @StatusID, CheckBy = @ApprovalBy, 
                        ApprovalFrom = @ApprovalFrom, CheckByID = @ApprovalByID, CheckDate = @ApprovalDate, IsCheck = @ConfirmApproval
                        where SalesID = @SalesID and Type = @Type;";
                            html.SaveChangeData(sql, false, para);

                            dynamic frm = controller.InvoiceSaleOrderTMP(salesId, saleType, false, true, approvalFrom: approvalFrom);
                            string fileName = html.ToUrlSlug(referenceNo);
                            string destination = $"Uploads/Sales/{saleType}/" + html.ToUrlSlug(customerName);
                            var pdfResult = await html.HTMLToPDF(frm.Data.Data, destination, fileName);

                            await notification.EditTelegramMessage(
                                messageId: messageId,
                                caption: updatedCaption,
                                inlineKeyboard: null,
                                chatId: chatId,
                                topicID: topicId,
                                filePath: pdfResult.OutPutFile,
                                newBotClient: botToken
                            );

                            msg.Clear();
                            msg.AppendLine($"{recipientUsername}\n");
                            msg.AppendLine($"Dear {(recipientId == 38 ? "Boss" : recipient)}, you have an {requestType}{(!isNonVAT && className != "BSM" ? " tax" : "")} need to approve.\n");
                            if (!string.IsNullOrEmpty(note) && saleType == "SalesOrder")
                                msg.AppendLine($"<i>Note: {note}</i>\n");

                            msg.AppendLine($"<i><b>Prepared by: {preparedBy}</b></i>.\n");
                            msg.Append($"Please kindly review and take action, Thanks you.");

                            sql = @"SELECT TOP 1 * 
                        FROM dbo.TblTelegramGroupNotification
                        WHERE [Module] = @Module AND TelegramGroupType = @Type AND SystemID = @SystemID";

                            para.Clear();
                            para.Add("@Module", saleType);
                            para.Add("@Type", "Normal");
                            para.Add("@SystemID", systemID);

                            ds = html.LoadData(sql, false, para);
                            var dr = ds?.Tables[0]?.Rows[0];

                            chatId = long.Parse(dr["TelegramGroupID"]?.ToString());

                            sentResult = await notification.NotificationTelegramBot(
                                "",
                                chatId: chatId,
                                PDFFile: pdfResult?.OutPutFile,
                                Caption: msg.ToString(),
                                replyMarkupInlineKeyboardMarkup: true,
                                inlineKeyboard: inlineKeyboard,
                                newBotClient: botToken
                            );

                            if (sentResult.Result == ResultType.Successful)
                            {
                                System.IO.File.Delete(pdfResult.OutPutFile);
                            }
                        }

                        sql = @"update dbo.TblSales set TelegramMessageID = @TelegramMessageID, TelegramMessage = @TelegramMessage, TelegramGroupTopicID = @TelegramGroupTopicID where SalesID = @ID;";
                        para.Clear();
                        para.Add("@ID", salesId.ToString());
                        para.Add("@TelegramMessage", !string.IsNullOrEmpty(msg.ToString()) ? msg.ToString() : updatedCaption);
                        para.Add("@TelegramMessageID", sentResult.Value);
                        para.Add("@TelegramGroupTopicID", "");
                        html.SaveChangeData(sql, false, para);

                    }
                    else
                    {
                        if (isRequestEdit)
                        {
                            await notification.EditTelegramMessage(
                                messageId: messageId,
                                msgText: updatedCaption,
                                inlineKeyboard: null,
                                chatId: chatId,
                                topicID: topicId,
                                newBotClient: botToken
                            );

                            sql = @"update dbo.TblSales set ReqEditStatusID = 90, IsToInvoice = 0 where SalesID = @SalesID and Type = @Type";
                        }
                        else
                        {
                            await notification.EditTelegramMessage(
                                messageId: messageId,
                                caption: updatedCaption,
                                inlineKeyboard: null,
                                chatId: chatId,
                                topicID: topicId,
                                newBotClient: botToken
                            );

                            sql = @"update dbo.TblSales set StatusID = @StatusID, DisapproveBy = @ApprovalBy, Comment = @Comment,
                                ApprovalFrom = @ApprovalFrom, DisapproveByID = @ApprovalByID, DisapproveDate = @ApprovalDate, IsDisapprove = @ConfirmApproval
                                where SalesID = @SalesID and Type = @Type;";
                        }

                        html.SaveChangeData(sql, false, para);
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
