using MVC;
using NSMSOLUTION.Controllers.Expenses;
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
    public class ExpensesApprovalController : ApiController
    {
        [HttpPost]
        [Route("api/ExpensesApproval")]
        public async Task<HttpResponseMessage> POST()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            int ExpensesID = int.Parse(HttpContext.Current.Request.Form["ExpenseID"].ToString());
            string ExpenseType = HttpContext.Current.Request.Form["ExpenseType"].ToString();
            string Status = HttpContext.Current.Request.Form["Status"].ToString();
            string FirstName = HttpContext.Current.Request.Form["FirstName"].ToString();
            string LastName = HttpContext.Current.Request.Form["LastName"].ToString();
            string UserName = HttpContext.Current.Request.Form["UserName"].ToString();
            string ExpensesType = HttpContext.Current.Request.Form["ExpensesType"].ToString();

            if (ExpensesType == "ExpensesCheck")
            {
                Status = Status == "CheckExpense" ? "Checked" : "Disapproved";

                string sql = @"select distinct u.UserLoginID, u.FullName, u.TelegramUserName from dbo.TblAccount_Expenses ae
inner join dbo.TblUserLogin u on u.UserLoginID = ae.CheckedByUserID where ae.ExpensesID = @ExpensesID";
                Dictionary<string, string> para = new Dictionary<string, string> {
                {"ExpensesID",ExpensesID.ToString() }
            };
                DataSet ds = html.LoadData(sql, false, para);
                int UserLoginID = int.Parse(ds.Tables[0].Rows[0]["UserLoginID"].ToString());
                string TelegramUserName = ds.Tables[0].Rows[0]["TelegramUserName"].ToString();
                string UserFullName = UserLoginID == 38 ? "Boss" : ds.Tables[0].Rows[0]["FullName"].ToString();
                if ("@" + UserName == TelegramUserName)
                {
                    ExpensesController request = new ExpensesController();
                    var result = await request.Check(ExpensesID, ExpenseType, Status, UserFullName);
                }
            }
            else if (ExpensesType == "ExpensesAdvCheck")
            {
                Status = Status == "CheckAdvExpense" ? "Checked" : "Disapproved";

                string sql = @"select distinct u.UserLoginID, u.FullName, u.TelegramUserName, ae.SystemID from dbo.TblAdvanceExpense ae
inner join dbo.TblUserLogin u on u.UserLoginID = ae.CheckedByUserID where ae.AdvanceExpenseID = @AdvanceExpenseID";
                Dictionary<string, string> para = new Dictionary<string, string> {
                {"AdvanceExpenseID",ExpensesID.ToString() }
            };
                DataSet ds = html.LoadData(sql, false, para);
                int UserLoginID = int.Parse(ds.Tables[0].Rows[0]["UserLoginID"].ToString());
                string TelegramUserName = ds.Tables[0].Rows[0]["TelegramUserName"].ToString();
                string SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                string UserFullName = UserLoginID == 38 ? "Boss" : ds.Tables[0].Rows[0]["FullName"].ToString();
                if ("@" + UserName == TelegramUserName)
                {
                    ExpensesController request = new ExpensesController();
                    var result = await request.CheckAdvExpense(ExpensesID, Status, UserFullName, SystemID);
                }
            }
            else if (ExpensesType == "ExpensesAdvApproval")
            {
                Status = Status == "ApproveAdvExpense" ? "Approved" : "Disapproved";

                string sql = @"select distinct u.UserLoginID, u.FullName, u.TelegramUserName, ae.SystemID from dbo.TblAdvanceExpense ae
inner join dbo.TblUserLogin u on u.UserLoginID = ae.ApprovedByUserID where ae.AdvanceExpenseID = @AdvanceExpenseID";
                Dictionary<string, string> para = new Dictionary<string, string> {
                {"AdvanceExpenseID",ExpensesID.ToString() }
            };
                DataSet ds = html.LoadData(sql, false, para);
                int UserLoginID = int.Parse(ds.Tables[0].Rows[0]["UserLoginID"].ToString());
                string TelegramUserName = ds.Tables[0].Rows[0]["TelegramUserName"].ToString();
                string SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                string UserFullName = UserLoginID == 38 ? "Boss" : ds.Tables[0].Rows[0]["FullName"].ToString();
                if ("@" + UserName == TelegramUserName)
                {
                    ExpensesController request = new ExpensesController();
                    var result = await request.ApproveDisapproveExpense(ExpensesID, Status, UserFullName, SystemID);
                }
            }
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
        [HttpPost]
        [Route("api/ExpensesAdvApproval")]
        public async Task<HttpResponseMessage> ExpensesAdvApproval()
        {
            try
            {
                int expensesID = int.Parse(HttpContext.Current.Request.Form["ExpenseID"]?.ToString());
                string status = HttpContext.Current.Request.Form["Status"]?.ToString();
                string userName = HttpContext.Current.Request.Form["UserName"]?.ToString();
                string expensesType = HttpContext.Current.Request.Form["ExpensesType"]?.ToString();
                string chatID = HttpContext.Current.Request.Form["ChatID"]?.ToString();
                string messageID = HttpContext.Current.Request.Form["MessageID"]?.ToString();
                string topicID = HttpContext.Current.Request.Form["TopicID"]?.ToString();
                string message = HttpContext.Current.Request.Form["Message"]?.ToString();

                HTML html = new HTML();

                // ── Resolve authorizer from DB based on ExpensesType ──────────────
                string resolverSql = "";

                if (expensesType == "ExpensesAdvCheck")
                {
                    resolverSql = @"select distinct ae.AdvNo, u.UserLoginID, u.FullName, u.TelegramUserName, ae.ApprovedBy, ae.ApprovedByTelegramUserName, ae.SystemID, ae.PreparedBy
                                    from dbo.TblAdvanceExpense ae
                                    inner join dbo.TblUserLogin u on u.UserLoginID = ae.CheckedByUserID
                                    where ae.AdvanceExpenseID = @AdvanceExpenseID";
                }
                else if (expensesType == "ExpensesAdvApproval")
                {
                    resolverSql = @"select distinct ae.AdvNo, u.UserLoginID, u.FullName, u.TelegramUserName, ae.ApprovedBy, ae.ApprovedByTelegramUserName, ae.SystemID, ae.PreparedBy
                                    from dbo.TblAdvanceExpense ae
                                    inner join dbo.TblUserLogin u on u.UserLoginID = ae.ApprovedByUserID
                                    where ae.AdvanceExpenseID = @AdvanceExpenseID";
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest,
                        new { success = false, message = "Unknown ExpensesType." });
                }

                var para = new Dictionary<string, string> { { "AdvanceExpenseID", expensesID.ToString() } };
                DataSet ds = html.LoadData(resolverSql, false, para);

                if (ds.Tables[0].Rows.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.NotFound,
                        new { success = false, message = "Expense record not found." });

                DataRow row = ds.Tables[0].Rows[0];
                string advNo = row["AdvNo"].ToString();
                int userLoginID = int.Parse(row["UserLoginID"].ToString());
                string telegramUserName = row["TelegramUserName"].ToString();
                string approverUserName = row["ApprovedByTelegramUserName"].ToString();
                string approver = row["ApprovedBy"].ToString();
                string preparedBy = row["PreparedBy"].ToString();
                string systemID = row["SystemID"].ToString();
                string userFullName = userLoginID == 38 ? "Boss" : row["FullName"].ToString();
                string botToken = html.GetBotTokenBySystemID(int.Parse(systemID));

                // ── Resolve final status ──────────────────────────────────────────
                bool isApprove = false;
                bool isCheck = false;
                bool isDisApprove = false;
                string resolvedStatus;

                if (expensesType == "ExpensesAdvCheck")
                {
                    isCheck = status == "CheckAdvExpense";
                    isDisApprove = status == "DisCheckAdvExpense";
                    resolvedStatus = isCheck ? "Checked" : "Disapproved";
                }
                else // ExpensesAdvApproval
                {
                    isApprove = status == "ApproveAdvExpense";
                    isDisApprove = status == "DisapproveAdvExpense";
                    resolvedStatus = isApprove ? "Approved" : "Disapproved";
                }

                // ── Edit original Telegram message ────────────────────────────────
                if (!string.IsNullOrEmpty(chatID) && !string.IsNullOrEmpty(messageID))
                {
                    string actionIcon = isApprove ? "✅" : isCheck ? "✍️" : "❌";

                    string updatedCaption = message
                        + $"\n\n{actionIcon} <i><b>{resolvedStatus} by: @{userName} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}</b></i>";

                    var notification = new NotificationTelegram();

                    if (!isDisApprove)
                    {
                        string sql = @"update dbo.TblAdvanceExpense set CheckStatus = @Status, CheckedDate = @Date where AdvanceExpenseID = @ID;";

                        if (isApprove)
                        {
                            sql = @"update dbo.TblAdvanceExpense set ApprovedStatus = @Status, ApprovedDate = @Date where AdvanceExpenseID = @ID;";

                            para.Clear();
                            para.Add("@ID", expensesID.ToString());
                            para.Add("@Status", "Approved");
                            para.Add("@Date", DateTime.Now.ToString());
                            html.SaveChangeData(sql, false, para);

                            // ── Re-render PDF and edit the message with new caption ───
                            var controller = new ExpensesController();
                            var textResult = controller.ShowAdvExpense(expensesID, true) as System.Web.Mvc.ContentResult;
                            var pdfResult = await html.HTMLToPDF(textResult.Content, $"Uploads/AdvExpenses", advNo, false, true, "A5");

                            // Edit original message: replace PDF + update caption
                            var sent = await notification.EditTelegramMessage(
                                messageId: int.Parse(messageID),
                                caption: updatedCaption,
                                inlineKeyboard: null,
                                chatId: long.Parse(chatID),
                                topicID: int.Parse(topicID),
                                filePath: pdfResult.OutPutFile,
                                newBotClient: botToken
                            );

                            if (sent.Result == ResultType.Successful)
                            {
                                System.IO.File.Delete(pdfResult.OutPutFile);
                            }
                        }
                        else // isCheck
                        {
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"ApproveAdvExpense~{expensesID}~{approverUserName}"),
                                    InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisapproveAdvExpense~{expensesID}~{approverUserName}"),
                                }
                            });

                            para.Clear();
                            para.Add("@ID", expensesID.ToString());
                            para.Add("@Status", "Checked");
                            para.Add("@Date", DateTime.Now.ToString());
                            html.SaveChangeData(sql, false, para);

                            var controller = new ExpensesController();
                            var textResult = controller.ShowAdvExpense(expensesID, true) as System.Web.Mvc.ContentResult;
                            var pdfResult = await html.HTMLToPDF(textResult.Content, $"Uploads/AdvExpenses", advNo, false, true, "A5");

                            // Edit original message: replace PDF + update caption + keep approval buttons
                            await notification.EditTelegramMessage(
                                messageId: int.Parse(messageID),
                                caption: updatedCaption,
                                inlineKeyboard: null,
                                chatId: long.Parse(chatID),
                                topicID: int.Parse(topicID),
                                filePath: pdfResult.OutPutFile,
                                newBotClient: botToken
                            );

                            var msgApproval = new StringBuilder();

                            msgApproval.AppendLine($"{approverUserName}\n");
                            msgApproval.AppendLine($"Dear {approver}.\n");
                            msgApproval.AppendLine($"Now you have new advance expense need to check that's created by {preparedBy}.");

                            var sent = await notification.NotificationTelegramBotToTopic(
                                "",
                                chatId: long.Parse(chatID),
                                topicID: int.Parse(topicID),
                                inlineKeyboard: inlineKeyboard,
                                MessageID: int.Parse(messageID),
                                Files: pdfResult.OutPutFile,
                                Caption: msgApproval.ToString(),
                                EditMessage: false,
                                newBotClient: botToken
                            );

                            if (sent.Result == ResultType.Successful)
                            {
                                System.IO.File.Delete(pdfResult.OutPutFile);

                                sql = @"update dbo.TblAdvanceExpense set TelegramMessageID = @TelegramMessageID, TelegramMessage = @TelegramMessage where AdvanceExpenseID = @ID;";
                                para.Clear();
                                para.Add("@ID", expensesID.ToString());
                                para.Add("@TelegramMessage", msgApproval.ToString());
                                para.Add("@TelegramMessageID", sent.Value);
                                html.SaveChangeData(sql, false, para);
                            }
                        }
                    }
                    else // isDisApprove — just update the caption text, no new PDF needed
                    {
                        await notification.EditTelegramMessage(
                            messageId: int.Parse(messageID),
                            caption: updatedCaption,
                            inlineKeyboard: null,
                            chatId: long.Parse(chatID),
                            topicID: int.Parse(topicID),
                            newBotClient: botToken
                        );
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
