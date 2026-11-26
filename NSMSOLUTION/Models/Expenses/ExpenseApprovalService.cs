using MVC;
using NSMSOLUTION.Helper_Code;
using NSMSOLUTION.Services;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Models.Expenses
{
    public class ExpenseApprovalService
    {
        private readonly TelegramBotClient _botClient;
        private readonly ExpenseRepository _repo;

        public ExpenseApprovalService(string botToken, ExpenseRepository repo)
        {
            //   if (!string.IsNullOrWhiteSpace(botToken))
            // {
            _botClient = new TelegramBotClient(botToken);
            // }
            _repo = repo;
        }

        public (bool success, string message) PostExpense(int expenseId)
        {
            HTML html = new HTML();

            string checkSql = $@"
        SELECT Status, IsPosted 
        FROM Expenses 
        WHERE ExpenseID = {expenseId}";

            DataTable dt = html.LoadData(checkSql).Tables[0];

            if (dt.Rows.Count == 0)
                return (false, "Expense not found");

            string status = dt.Rows[0]["Status"].ToString();
            bool isPosted = Convert.ToBoolean(dt.Rows[0]["IsPosted"]);

            if (isPosted)
                return (false, "Already posted");

            if (status != "Checked")
                return (false, "Only checked expense can be posted");

            string sql = $@"
        UPDATE Expenses
        SET 
            IsPosted = 1,
            PaymentStatus = 'Paid',
            PostedDate = GETDATE(),
            PostedBy = '{html.UserFullName}',
            PostedByUserID = {html.UserID}
        WHERE ExpenseID = {expenseId}
        AND ISNULL(IsPosted, 0) = 0;
        
        UPDATE TblAccount_JournalEntries SET IsPosted = 1 WHERE ExpenseID = {expenseId}
    ";

            SqlResult sqlResult = new SqlResult();
            sqlResult = html.SaveChangeData(sql, false, false);

            return sqlResult.Result == ResultType.Successful
                ? (true, "Posted successfully")
                : (false, "Post failed");
        }
        public async Task<(bool success, string message)> ApprovalPaymentVoucherAsync(
            int expenseId,
            string status,
            string UserFullName,
            string PaymentNo,
            ControllerContext controllerContext)
        {
            HTML html = new HTML();

            string sql = $@"
                SELECT e.CheckedByUserID,
                       ISNULL(e.TelegramMessageID,0) TelegramMessageID,
                       ISNULL(e.TelegramChatID,0) TelegramChatID,
                       e.[No],
                       e.[Status], ISNULL(ul.FullName, '') FullName, ISNULL(ulv.FullName, '') VerifiedBy
                FROM dbo.Expense_PaymentTransactions e
                LEFT JOIN dbo.TblUserLogin ul ON ul.UserLoginID = e.CheckedByUserID
                LEFT JOIN dbo.TblUserLogin ulv ON ulv.UserLoginID = e.VerifiedByUserID
                WHERE ExpenseID = {expenseId} AND (PaymentNo = '{PaymentNo}' OR '{PaymentNo}' = '')";

            DataSet ds = html.LoadData(sql);

            if (ds == null ||
                ds.Tables.Count == 0 ||
                ds.Tables[0].Rows.Count == 0)
            {
                return (false, "Expense not found");
            }

            var row = ds.Tables[0].Rows[0];

            string FullName = row["FullName"].ToString();
            string VerifiedBy = row["VerifiedBy"].ToString();
            string checkedByUserID = row["CheckedByUserID"].ToString();
            string currentStatus = row["Status"].ToString();
            string creatorFullName = row["FullName"].ToString();

            bool isOverrideUser = html.UserID == "38" || html.UserID == "126";

            if (!isOverrideUser)
            {
                if ((status == "Checked" || status == "Rejected") && FullName != UserFullName && !string.IsNullOrEmpty(FullName))
                    return (false, "You are not authorized to perform this action");
                if ((status == "Confirm Verified" || status == "Confirm Rejected") && VerifiedBy != UserFullName && !string.IsNullOrEmpty(VerifiedBy))
                    return (false, "You are not authorized to perform this action");
            }
            // ✅ Prevent double processing
            //if (currentStatus == "Checked" || currentStatus == "Rejected" || currentStatus == "Confirm Verified" || currentStatus == "Confirm Rejected")
            //  return (false, "This voucher was already processed");

            int messageId = Convert.ToInt32(row["TelegramMessageID"]);
            long chatId = Convert.ToInt64(row["TelegramChatID"]);
            string voucherNo = row["No"].ToString();

            // ✅ Update Status (safe update)
            string updateSql = status == "Checked"
                ? $@"
                UPDATE dbo.Expenses
                SET [Status] = '{status}',
                    IsChecked = 1,
                    CheckedDate = GETDATE()
                WHERE ExpenseID = {expenseId} AND PaymentNo = '{PaymentNo}';

                UPDATE dbo.Expense_PaymentTransactions
                SET [Status] = '{status}',
                    IsChecked = 1,
                    CheckedDate = GETDATE()
                WHERE ExpenseID = {expenseId} AND PaymentNo = '{PaymentNo}'"
                : status == "Confirm Verified"
                    ? $@"
                UPDATE dbo.Expenses
                SET [Status] = '{status}',
                    IsVerified = 1,
                    VerifiedDate = GETDATE()
                WHERE ExpenseID = {expenseId} AND PaymentNo = '{PaymentNo}';

                UPDATE dbo.Expense_PaymentTransactions
                SET [Status] = '{status}',
                    IsVerified = 1,
                    VerifiedDate = GETDATE()
                WHERE ExpenseID = {expenseId} AND PaymentNo = '{PaymentNo}'"
                    : $@"
                UPDATE dbo.Expenses
                SET [Status] = '{status}',
                    VerifiedDate = NULL, CheckedDate = NULL, IsChecked = 0
                WHERE ExpenseID = {expenseId} AND PaymentNo = '{PaymentNo}';

                UPDATE dbo.Expense_PaymentTransactions
                SET [Status] = '{status}',
                    VerifiedDate = NULL, CheckedDate = NULL, IsChecked = 0
                WHERE ExpenseID = {expenseId} AND PaymentNo = '{PaymentNo}'";
            SqlResult sqlResult = html.SaveChangeData(updateSql, false, false);

            if (_botClient != null)
            {
                // Pre-render voucher HTML while ControllerContext is still active on the request thread (~15ms)
                string htmlContent = null;
                string baseUrl = null;
                try
                {
                    var model = _repo.GetVoucherByExpenseId(expenseId, PaymentNo);
                    if (model != null && controllerContext != null)
                    {
                        htmlContent = ViewRenderHelper.RenderViewToString(controllerContext, "PaymentVoucher", model);
                    }
                    baseUrl = HttpContext.Current != null && HttpContext.Current.Request != null
                        ? HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)
                        : null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Pre-render voucher HTML error: " + ex.Message);
                }

                // Run heavy Chromium PDF generation and Telegram API calls in background without blocking UI
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(htmlContent))
                            return;

                        var pdfService = new PuppeteerPdfService();
                        byte[] pdfBytes = await pdfService.GeneratePdfAsync(htmlContent, baseUrl);

                        if (pdfBytes != null && pdfBytes.Length > 0)
                        {
                            // Remove Inline Keyboard (only if exists)
                            if (messageId > 0)
                            {
                                try
                                {
                                    await _botClient.EditMessageReplyMarkup(
                                        chatId: chatId,
                                        messageId: messageId,
                                        replyMarkup: null
                                    );
                                }
                                catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                                {
                                    if (!ex.Message.Contains("message is not modified"))
                                        throw;
                                }
                            }

                            string caption = BuildCaption(status, voucherNo, UserFullName, VerifiedBy);

                            using (var stream = new MemoryStream(pdfBytes))
                            {
                                InlineKeyboardMarkup keyboard = null;

                                // Only show Verify buttons after Checker approved
                                if (status == "Checked")
                                {
                                    keyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(
                                                "✅ Confirm Verified",
                                                $"verify_confirm~{expenseId}~{PaymentNo}")
                                            ,
                                            InlineKeyboardButton.WithCallbackData(
                                                "❌ Reject",
                                                $"verify_reject~{expenseId}~{PaymentNo}")
                                        }
                                    });
                                }
                                var sentMessage = await _botClient.SendDocument(
                                    chatId: chatId,
                                    document: new InputFileStream(stream, $"PaymentVoucher_{PaymentNo}.pdf"),
                                    caption: caption,
                                    parseMode: ParseMode.Html,
                                    replyMarkup: keyboard,
                                    replyParameters: messageId
                                );

                                if (sentMessage != null)
                                {
                                    HTML bgHtml = new HTML();
                                    string updateReplySql = $@"
                                        UPDATE dbo.Expenses
                                        SET TelegramMessageID = {sentMessage.MessageId},
                                         TelegramReplyMessageID = {sentMessage.MessageId}
                                        WHERE ExpenseID = {expenseId} AND PaymentNo = '{PaymentNo}';

                                        UPDATE dbo.Expense_PaymentTransactions
                                        SET TelegramMessageID = {sentMessage.MessageId},
                                         TelegramReplyMessageID = {sentMessage.MessageId}
                                        WHERE ExpenseID = {expenseId} AND PaymentNo = '{PaymentNo}'
                                    ";

                                    bgHtml.SaveChangeData(updateReplySql, false, false);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Telegram update error: " + ex.Message);
                    }
                });
            }

            return (true, "Approval processed successfully");
        }

        private string BuildCaption(string status, string voucherNo, string userFullName, string VerifiedBy = "")
        {
            switch (status)
            {
                case "Checked":
                    {
                        var safeVoucherNo = WebUtility.HtmlEncode(voucherNo);
                        var safeUserFullName = WebUtility.HtmlEncode(userFullName);
                        var safeVerifiedBy = WebUtility.HtmlEncode(VerifiedBy);

                        var nextApprover = string.IsNullOrWhiteSpace(VerifiedBy)
                            ? string.Empty
                            : $"\n<b>Next Approver:</b> {safeVerifiedBy}";

                        return $@"
<b>🧾 PAYMENT VOUCHER APPROVAL UPDATE</b>

<b>Voucher No:</b> #{safeVoucherNo}

────────────────────────
✅ <b>STATUS: CHECKED</b>
────────────────────────

<b>Checked By:</b> {safeUserFullName}{nextApprover}

<b>Date:</b> {DateTime.Now:dd-MMM-yyyy HH:mm}

The payment voucher has been successfully checked and forwarded to the verification stage for further approval.

This is an automated system notification. Please do not reply to this message.
";
                    }

                case "Confirm Verified":
                    return $@"
<b>🧾 PAYMENT VOUCHER VERIFICATION RESULT</b>

<b>Voucher No:</b> #{voucherNo}

────────────────────────
✅ <b>CURRENT STATUS: VERIFIED</b>
────────────────────────

<b>Verified By:</b> {userFullName}
<b>Date:</b> {DateTime.Now:dd-MMM-yyyy HH:mm}

The voucher has been verified successfully and is ready for the next process.

This is an automated system notification.
";

                case "Rejected":
                    return $@"
<b>🧾 PAYMENT VOUCHER REVIEW RESULT</b>

<b>Voucher No:</b> #{voucherNo}

────────────────────────
❌ <b>CURRENT STATUS: REJECTED</b>
────────────────────────

<b>Rejected By:</b> {userFullName}
<b>Date:</b> {DateTime.Now:dd-MMM-yyyy HH:mm}

The voucher has been rejected and returned for correction.

Please review, make the necessary changes, and resubmit.

This is an automated system notification.
";

                case "Confirm Rejected":
                    return $@"
<b>🧾 PAYMENT VOUCHER VERIFICATION RESULT</b>

<b>Voucher No:</b> #{voucherNo}

────────────────────────
❌ <b>CURRENT STATUS: VERIFICATION REJECTED</b>
────────────────────────

<b>Verified By:</b> {userFullName}
<b>Date:</b> {DateTime.Now:dd-MMM-yyyy HH:mm}

The voucher was reviewed during the verification stage but was not approved.

Please correct the required information and resubmit for approval.

This is an automated system notification.
";

                default:
                    return string.Empty;
            }
        }
        public async Task<byte[]> GenerateVoucherPdfBytes(int id, string PaymentNo, ControllerContext context)
        {
            var model = _repo.GetVoucherByExpenseId(id, PaymentNo);

            if (model == null)
                return null;

            // Render PaymentVoucher.cshtml
            var html = ViewRenderHelper.RenderViewToString(context, "PaymentVoucher", model);

            // Current application URL
            var baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

            var pdfService = new PuppeteerPdfService();

            return await pdfService.GeneratePdfAsync(html, baseUrl);
        }
        //private byte[] GenerateVoucherPdfBytes(int id, string PaymentNo, ControllerContext context)
        //{
        //    var model = _repo.GetVoucherByExpenseId(id, PaymentNo);

        //    if (model == null)
        //        return null;

        //    var pdf = new ViewAsPdf("PaymentVoucher", model)
        //    {
        //        PageSize = Size.A4,
        //        PageOrientation = Orientation.Portrait,
        //        //PageMargins = new Margins(0, 0, 0, 0),
        //        CustomSwitches =
        //"--print-media-type " +
        //"--enable-local-file-access " +
        //"--background " +
        ////"--footer-line " +
        //"--footer-center \"Page [page] of [toPage]\" " +
        //"--footer-font-size 7 " +
        //"--footer-spacing 5"
        //    };

        //    return pdf.BuildFile(context);
        //}
    }
}