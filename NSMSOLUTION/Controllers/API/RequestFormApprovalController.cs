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
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
  [Authorize]
  [RoutePrefix("api")]
  public class RequestFormApprovalController : ApiController
  {
    [HttpPost]
    [Route("RequestFormApproval")]
    public async Task<HttpResponseMessage> RequestFormApproval()
    {
      try
      {
        HTML html = new HTML();
        int reqID = int.Parse(HttpContext.Current.Request.Form["RequestFormID"]?.ToString());
        string userName = HttpContext.Current.Request.Form["UserName"]?.ToString();
        string reqStatus = HttpContext.Current.Request.Form["RequestStatus"]?.ToString();
        string cbStatus = HttpContext.Current.Request.Form["CallBackStatus"]?.ToString();
        long chatID = long.Parse(HttpContext.Current.Request.Form["TelegramGroupID"]?.ToString());
        int messageID = int.Parse(HttpContext.Current.Request.Form["TelegramMessageID"]?.ToString());
        string message = HttpContext.Current.Request.Form["TelegramMessage"]?.ToString();

        string resolverSql = "";

        if (reqStatus == "CheckRequest")
        {
          resolverSql = @"select s.NeedNextApprovalBy, s.NeedNextApprovalByUsername, s.StatusID, s.ReferenceNumber,  s.PaymentMean, s.NeedToCheckUserID AS ApproverID, s.NeedToCheckBy AS ApproverName, s.CheckedByUsername AS ApproverUsername,
                        s.NeedApprovalByUserID as RecipientID, s.NeedApprovalBy AS Recipient, s.TelegramUserName as RecipientUsername, s.CreateBy, s.SystemID
                        from dbo.TblRequestForm s
                        where s.RequestFormID = @RequestFormID";
        }
        else if (reqStatus == "ApprovalRequest")
        {
          resolverSql = @"select s.NeedNextApprovalBy, s.NeedNextApprovalByUsername, s.StatusID, s.ReferenceNumber, s.PaymentMean, case when isnull(s.NeedNextApprovalBy, '') <> '' and s.StatusID = 90 then s.NeedNextApprovalByUserID else s.NeedApprovalByUserID end AS ApproverID, case when isnull(s.NeedNextApprovalBy, '') <> '' and s.StatusID = 90 then s.NeedNextApprovalBy else s.NeedApprovalBy end AS ApproverName, s.TelegramUserName AS ApproverUsername,
                        s.NeedApprovalByUserID as RecipientID, s.NeedApprovalBy AS Recipient, s.TelegramUserName as RecipientUsername, s.CreateBy, s.SystemID
                        from dbo.TblRequestForm s where s.RequestFormID = @RequestFormID";
        }
        else
        {
          return Request.CreateResponse(HttpStatusCode.BadRequest,
              new { success = false, message = "Unknown RequestStatus." });
        }

        var para = new Dictionary<string, string> { { "@RequestFormID", reqID.ToString() } };
        DataSet ds = html.LoadData(resolverSql, false, para);

        if (ds.Tables[0].Rows.Count == 0)
          return Request.CreateResponse(HttpStatusCode.NotFound,
              new { success = false, message = "Request form record not found." });

        DataRow row = ds.Tables[0].Rows[0];
        string reqNo = row["ReferenceNumber"].ToString();
        string payemtnMean = row["PaymentMean"].ToString();
        int approverId = int.Parse(row["ApproverID"].ToString());
        int statusId = int.Parse(row["StatusID"].ToString());
        string preparedBy = row["CreateBy"].ToString();
        string approver = row["ApproverName"].ToString();
        string nextApprover = row["NeedNextApprovalBy"].ToString();
        bool isHasNextApprove = !string.IsNullOrEmpty(nextApprover);
        int recipientId = int.Parse(row["RecipientID"].ToString());
        string recipient = row["Recipient"].ToString();
        string nextApproverUsername = row["NeedNextApprovalByUsername"].ToString();
        string approverUserName = isHasNextApprove && statusId == 90
            ? nextApproverUsername
            : row["ApproverUsername"].ToString();
        string recipientUsername = row["RecipientUsername"].ToString();
        string systemID = row["SystemID"].ToString();

        string botToken = html.GetBotTokenBySystemID(int.Parse(systemID));

        approver = approverId == 38 ? "Boss" : approver;

        bool isApprove = false;
        bool isCheck = false;
        bool isDisApprove = false;
        bool isReject = false;
        bool isNextApprove = false;
        bool isNextDisapprove = false;
        string resolvedStatus;
        string sql = "";

        if (reqStatus == "CheckRequest")
        {
          isCheck = cbStatus == "Check";
          isDisApprove = cbStatus == "DisCheck";
          isReject = cbStatus == "RejectCheck";
          resolvedStatus = isCheck ? "Checked" : isReject ? "Rejected" : "Disapproved";
        }
        else
        {
          isApprove = cbStatus == "Approve";
          isNextApprove = cbStatus == "NextApprove";
          isDisApprove = cbStatus == "Disapprove";
          isNextDisapprove = cbStatus == "NextDisapprove";
          isReject = cbStatus == "Reject";
          resolvedStatus = isApprove ? "Approved"
              : isNextApprove ? "Next Approved"
              : isReject ? "Rejected"
              : "Disapproved";
        }

        if (chatID < 0 && messageID > 0)
        {
          string actionIcon = isApprove || isNextApprove ? "✅" : isCheck ? "✍️" : isReject ? "🚫" : "❌";

          string updatedCaption = message
              + $"\n\n{actionIcon} <i><b>{resolvedStatus} by: {(!isNextApprove && isApprove && recipientId == 38 ? "Boss" : "@" + userName)} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}</b></i>";

          var notification = new NotificationTelegram();

          para.Clear();
          para.Add("@ID", reqID.ToString());
          para.Add("@ApproverID", approverId.ToString());
          para.Add("@Approver", approver);
          para.Add("@StatusID", isCheck ? "92"
              : isApprove || isNextApprove ? "90"
              : isDisApprove || isNextDisapprove ? "91"
              : "93");
          para.Add("@RequestStatusID", isCheck ? "92" : isApprove ? "90" : "");
          para.Add("@Date", DateTime.Now.ToString());

          if (!isDisApprove && !isReject)
          {
            var controller = new ProfileController();
            var msg = new StringBuilder();
            var sentResult = new SqlResult();

            if (isApprove || isNextApprove || isNextDisapprove)
            {
              var inlineKeyboard = new InlineKeyboardMarkup(new[]
              {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "Next Approve ✅",    callbackData: $"NextApprove~{reqID}~{nextApproverUsername}"),
                                    InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"NextDisapprove~{reqID}~{nextApproverUsername}"),
                                }
                            });

              sql = isNextApprove
                  ? @"update dbo.TblRequestForm set NextApproveStatusID = @StatusID, NextApprovedByID = @ApproverID, NextApprovedBy = @Approver, NextApprovedByDate = @Date where RequestFormID = @ID;"
                  : isNextDisapprove
                  ? @"update dbo.TblRequestForm set NextApproveStatusID = @StatusID, NextDisapproveByID = @ApproverID, NextDisapproveBy = @Approver, NextDisapproveDate = @Date where RequestFormID = @ID;"
                  : @"update dbo.TblRequestForm set StatusID = @StatusID, RequestStatusID = @RequestStatusID, ApprovedByUserID = @ApproverID, ApprovedBy = @Approver, ApprovalDate = @Date where RequestFormID = @ID;";

              html.SaveChangeData(sql, false, para);

              dynamic frm = controller.InvoiceTMPRequestForm(0, reqID, payemtnMean, ReturnAllHtmlData: true);
              string fileName = html.ToUrlSlug(reqNo);
              string destination = $"Uploads/RequestForm";
              var pdfResult = await html.HTMLToPDF(frm.Data.Data, destination, fileName);

              updatedCaption = isHasNextApprove && isApprove
                  ? updatedCaption + $"\n\n👤 <i><b>Next Approver: {nextApproverUsername}</b></i>"
                  : updatedCaption;

              sentResult = await notification.EditTelegramMessage(
                  messageId: messageID,
                  caption: updatedCaption,
                  inlineKeyboard: isHasNextApprove && isApprove ? inlineKeyboard : null,
                  chatId: chatID,
                  topicID: 0,
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
                                    InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"Approve~{reqID}~{recipientUsername}"),
                                    InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"Disapprove~{reqID}~{recipientUsername}"),
                                    InlineKeyboardButton.WithCallbackData(text: "Reject ❌",     callbackData: $"Reject~{reqID}~{recipientUsername}")
                                }
                            });

              sql = @"update dbo.TblRequestForm set StatusID = @StatusID, RequestStatusID = @RequestStatusID, CheckedByUserID = @ApproverID, CheckedBy = @Approver, CheckedDate = @Date where RequestFormID = @ID;";
              html.SaveChangeData(sql, false, para);

              dynamic frm = controller.InvoiceTMPRequestForm(0, reqID, payemtnMean, ReturnAllHtmlData: true);
              string fileName = html.ToUrlSlug(reqNo);
              string destination = $"Uploads/RequestForm";
              var pdfResult = await html.HTMLToPDF(frm.Data.Data, destination, fileName);

              await notification.EditTelegramMessage(
                  messageId: messageID,
                  caption: updatedCaption,
                  inlineKeyboard: null,
                  chatId: chatID,
                  topicID: 0,
                  filePath: pdfResult.OutPutFile,
                  newBotClient: botToken
              );

              msg.Clear();
              msg.AppendLine($"{recipientUsername}\n");
              msg.AppendLine($"Dear {(recipientId == 38 ? "Boss" : recipient)},\n");
              msg.AppendLine($"Now you have a request need to approve.\n");
              msg.AppendLine($"<i><b>Prepared by: {preparedBy}</b></i>.\n");
              msg.AppendLine($"Please kindly review and take action, Thanks you.");

              sentResult = await notification.NotificationTelegramBot(
                  "",
                  chatId: chatID,
                  PDFFile: pdfResult?.OutPutFile,
                  Caption: msg.ToString(),
                  replyMarkupInlineKeyboardMarkup: true,
                  MessageID: messageID,
                  inlineKeyboard: inlineKeyboard,
                  newBotClient: botToken
              );

              if (sentResult.Result == ResultType.Successful)
              {
                System.IO.File.Delete(pdfResult.OutPutFile);
              }
            }

            sql = @"update dbo.TblRequestForm set TelegramMessageID = @TelegramMessageID, TelegramMessage = @TelegramMessage where RequestFormID = @ID;";
            para.Clear();
            para.Add("@ID", reqID.ToString());
            para.Add("@TelegramMessage", msg.ToString());
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

            sql = isDisApprove
                ? @"update dbo.TblRequestForm set StatusID = @StatusID, DisapproveByUserID = @ApproverID, DisapproveBy = @Approver, DisapproveDate = @Date where RequestFormID = @ID;"
                : @"update dbo.TblRequestForm set StatusID = @StatusID, RejectByUserID = @ApproverID, RejectBy = @Approver, RejectDate = @Date where RequestFormID = @ID;";

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
