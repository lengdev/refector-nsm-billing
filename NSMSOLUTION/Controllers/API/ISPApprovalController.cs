using MVC;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
    public class ISPApprovalController : ApiController
    {
        // ── OSP Group Constants ────────────────────────────────────────────
        private const long OSP_CHAT_ID = -1002377280117;  // ← your OSP group
        private const int OSP_TOPIC_ID = 5;               // ← your OSP topic

        [HttpPost]
        [AllowAnonymous]
        [Route("api/QuotationApproval")]
        public async Task<HttpResponseMessage> QuotationApproval()
        {
            try
            {
                var request = HttpContext.Current.Request;

                string invoiceNo = request.Form["InvoiceNo"];
                string status = request.Form["Status"];
                string chatID = request.Form["ChatID"];
                string messageID = request.Form["MessageID"];
                string message = request.Form["Message"];

                if (string.IsNullOrWhiteSpace(invoiceNo) ||
                    string.IsNullOrWhiteSpace(status))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest,
                        new { success = false, message = "InvoiceNo and Status are required." });
                }

                bool isApproved;

                if (status == "ApproveQuotation")
                    isApproved = true;
                else if (status == "DisapproveQuotation")
                    isApproved = false;
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest,
                        new { success = false, message = "Invalid approval status." });
                }

                using (var ctx = new DBContext())
                {
                    // 1. Get bank slip/payment record
                    var bankSlip = await ctx.TblCustomerBankSlip
                        .FirstOrDefaultAsync(x => x.InvoiceNo == invoiceNo);

                    if (bankSlip == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound,
                            new { success = false, message = "Payment record not found." });
                    }

                    // 2. Get approver
                    var approver = await ctx.TblUserLogin
                        .Where(x => x.UserLoginID == bankSlip.NeedApproveByID)
                        .Select(x => new
                        {
                            x.UserLoginID,
                            x.FullName,
                            x.TelegramUserName
                        })
                        .FirstOrDefaultAsync();

                    if (approver == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound,
                            new { success = false, message = "Approver not found." });
                    }

                    // 3. Get customer invoice
                    var customerInvoice = await ctx.TblCustomerInvoice
                        .Where(x => x.InvoiceNo == invoiceNo &&
                                    x.CustomerID == bankSlip.CustomerID)
                        .Join(ctx.TblCustomer,
                            inv => inv.CustomerID,
                            cus => cus.CustomerID,
                            (inv, cus) => new
                            {
                                inv.IsIncludedVAT,
                                inv.InvoiceDate,
                                inv.CustomerID,
                                cus.CustomerName,
                                cus.CustomerCode
                            })
                        .FirstOrDefaultAsync();

                    if (customerInvoice == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound,
                            new { success = false, message = "Customer invoice not found." });
                    }

                    // 4. Build approval caption
                    string actionText = isApproved ? "Approved" : "Disapproved";
                    string actionIcon = isApproved ? "✅" : "❌";

                    string originalCaption = !string.IsNullOrWhiteSpace(message)
                        ? message
                        : bankSlip.TelegramCaption ?? string.Empty;

                    // Avoid adding the approval line more than once
                    string cleanCaption = string.Join("\n",
                        originalCaption
                            .Split('\n')
                            .Where(line =>
                                !line.Contains("Approved by:") &&
                                !line.Contains("Disapproved by:")));

                    string updatedCaption = cleanCaption +
                        $"\n\n{actionIcon} <i><b>{actionText} by: " +
                        $"{approver.FullName} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}</b></i>";

                    // 5. Update payment approval
                    bankSlip.StatusID = isApproved ? 90 : 91;
                    bankSlip.ApprovedBy = isApproved ? approver.FullName : null;
                    bankSlip.ApprovedByID = isApproved ? approver.UserLoginID : (int?)null;
                    bankSlip.ApprovedDate = isApproved ? DateTime.Now : (DateTime?)null;
                    bankSlip.TelegramCaption = updatedCaption;

                    // 6. Update invoice approval independently of Telegram
                    if (isApproved)
                    {
                        var invoice = await ctx.TblCustomerInvoice
                            .FirstOrDefaultAsync(x =>
                                x.InvoiceNo == invoiceNo &&
                                x.CustomerID == bankSlip.CustomerID);

                        if (invoice != null)
                        {
                            invoice.InvoiceStatusID = 90;
                            invoice.ApproveBy = approver.FullName;
                            invoice.ApproveDate = DateTime.Now;
                        }
                    }

                    await ctx.SaveChangesAsync();

                    // 7. Edit original Telegram message
                    // Use saved Telegram IDs if callback values are not supplied.
                    string savedChatID = !string.IsNullOrWhiteSpace(chatID)
                        ? chatID
                        : bankSlip.TelegramGroupID?.ToString();

                    string savedMessageID = !string.IsNullOrWhiteSpace(messageID)
                        ? messageID
                        : bankSlip.TelegramMessageID?.ToString();

                    string warning = null;

                    if (!string.IsNullOrWhiteSpace(savedChatID) &&
                        !string.IsNullOrWhiteSpace(savedMessageID))
                    {
                        try
                        {
                            var notification = new NotificationTelegram();

                            var editResult = await notification.EditTelegramMessage(
                                messageId: int.Parse(savedMessageID),
                                caption: updatedCaption,
                                inlineKeyboard: null,
                                chatId: long.Parse(savedChatID),
                                topicID: bankSlip.TelegramTopicID ?? 0
                            );

                            if (editResult.Result != ResultType.Successful)
                            {
                                warning = "Telegram message could not be updated: " +
                                          editResult.Message;
                            }
                        }
                        catch (Exception ex)
                        {
                            warning = "Telegram message update failed: " + ex.Message;
                        }
                    }

                    // 8. Notify OSP group after approval
                    // This is no longer dependent on editing the original Telegram message.
                    if (isApproved)
                    {
                        try
                        {
                            string ospCaption = cleanCaption
                                .Replace("📋 ISP QUOTATION (Bank Slip)", "🚀 ISP New Installation")
                                .Replace("📋 NSM QUOTATION (Bank Slip)", "🚀 NSM New Installation")
                                .Replace("📋 ISP QUOTATION (Cash Payment)", "🚀 ISP New Installation")
                                .Replace("📋 NSM QUOTATION (Cash Payment)", "🚀 NSM New Installation");

                            // Remove approval instructions from the OSP message
                            ospCaption = string.Join("\n",
                                ospCaption
                                    .Split('\n')
                                    .Where(line =>
                                        !line.Contains("Need approve by:") &&
                                        !line.Contains("Approved by:") &&
                                        !line.Contains("Disapproved by:")));

                            var html = new HTML();

                            var pdfResult = await html.InvoiceToPDFandUpload(
                                customerInvoice.CustomerName,
                                customerInvoice.CustomerCode,
                                customerInvoice.CustomerID,
                                invoiceNo,
                                customerInvoice.InvoiceDate.ToString(),
                                Utility.TryParse(customerInvoice.IsIncludedVAT, "int", 0),
                                11,
                                int.Parse(html.SystemID.ToString()),
                                int.Parse(html.WarehouseID.ToString()),
                                false
                            );

                            if (pdfResult == null ||
                                string.IsNullOrWhiteSpace(pdfResult.OutPutFile))
                            {
                                throw new Exception("PDF generation failed.");
                            }

                            try
                            {
                                var ospKeyboard = new InlineKeyboardMarkup(new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData(
                                            "Done Lay Cable ✅",
                                            $"DoneLayCable~{invoiceNo}~{customerInvoice.CustomerID}"),

                                        InlineKeyboardButton.WithCallbackData(
                                            "Can't Lay Cable ❌",
                                            $"NotDoneLayCable~{invoiceNo}~{customerInvoice.CustomerID}")
                                    }
                                });

                                var ospNotification = new NotificationTelegram();

                                var alertResult = await ospNotification.NotificationTelegramBot(
                                    MsgText: "",
                                    chatId: -1003610309586,
                                    replyMarkupInlineKeyboardMarkup: true,
                                    inlineKeyboard: ospKeyboard,
                                    PDFFile: pdfResult.OutPutFile,
                                    Caption: ospCaption
                                );

                                if (alertResult.Result != ResultType.Successful)
                                {
                                    warning = (warning == null ? "" : warning + " | ") +
                                              "OSP notification failed: " + alertResult.Message;
                                }
                            }
                            finally
                            {
                                // Always delete temporary PDF
                                if (System.IO.File.Exists(pdfResult.OutPutFile))
                                    System.IO.File.Delete(pdfResult.OutPutFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            warning = (warning == null ? "" : warning + " | ") +
                                      "OSP notification failed: " + ex.Message;
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = true,
                        message = $"{actionText} successfully.",
                        warning
                    });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/DoneLayCable")]
        public async Task<HttpResponseMessage> DoneLayCable()
        {
            try
            {
                // ── Parse form fields 
                int customerId = Utility.TryParse(HttpContext.Current.Request.Form["customerId"]?.ToString(), "int", 0);
                string invoiceNo = HttpContext.Current.Request.Form["InvoiceNo"]?.ToString();
                string status = HttpContext.Current.Request.Form["Status"]?.ToString();
                string userName = HttpContext.Current.Request.Form["UserName"]?.ToString();
                string chatID = HttpContext.Current.Request.Form["ChatID"]?.ToString();
                string messageID = HttpContext.Current.Request.Form["MessageID"]?.ToString();
                string message = HttpContext.Current.Request.Form["Message"]?.ToString();

                if (string.IsNullOrEmpty(invoiceNo) || string.IsNullOrEmpty(status))
                    return Request.CreateResponse(HttpStatusCode.BadRequest,
                        new { success = false, message = "InvoiceNo and Status are required" });

                using (var ctx = new DBContext())
                {
                    // ── Fetch bank slip + approver 
                    var result = await ctx.TblCustomerBankSlip
                        .Where(x => x.InvoiceNo == invoiceNo)
                        .Join(ctx.TblUserLogin,
                            slip => slip.NeedApproveByID,
                            user => user.UserLoginID,
                            (slip, user) => new
                            {
                                BankSlip = slip,
                                ApproverID = user.UserLoginID,
                                ApproverFullName = user.FullName,
                                ApproverTelegram = user.TelegramUserName
                            })
                        .FirstOrDefaultAsync();

                    if (result == null)
                        return Request.CreateResponse(HttpStatusCode.NotFound,
                            new { success = false, message = "Bank slip not found" });

                    // ── Fetch customer invoice 
                    var customerInvoice = await ctx.TblCustomerInvoice
                        .Where(x => x.CustomerID == customerId && x.InvoiceNo == invoiceNo)
                        .Join(ctx.TblCustomer,
                            inv => inv.CustomerID,
                            cus => cus.CustomerID,
                            (inv, cus) => new
                            {
                                inv.IsIncludedVAT,
                                inv.TypeOfInvoice,
                                inv.InvoiceDate,
                                inv.InstallDate,
                                inv.SID,
                                inv.SystemID,
                                inv.WarehouseID,
                                inv.DomainID,
                                cus.CustomerID,
                                cus.CustomerName,
                                cus.CustomerCode,
                                cus.CustomerTypeID
                            })
                        .FirstOrDefaultAsync();

                    // ── Build captions 
                    bool isDone = status == "DoneLayCable";
                    string actionIcon = isDone ? "✅" : "❌";
                    string actionText = isDone ? "Done lay cable" : "Can't lay cable at this location.";

                    // FIX: keep originalCaption BEFORE updating TelegramCaption
                    // so ospCaption is built from clean original (no approval line)
                    string originalCaption = message ?? string.Empty;

                    string updatedCaption = originalCaption
                        + $"\n\n{actionIcon} <i><b>{actionText} by: @{userName} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}</b></i>";

                    // ── Update DB 
                    result.BankSlip.IsDoneLayCable = isDone;
                    result.BankSlip.DoneLayDate = DateTime.Now;
                    result.BankSlip.TelegramCaption = updatedCaption;

                    await ctx.SaveChangesAsync();

                    // ── Edit original Telegram message 
                    if (!string.IsNullOrEmpty(chatID) && !string.IsNullOrEmpty(messageID))
                    {
                        try
                        {
                            var notification = new NotificationTelegram();

                            // FIX: use EditTelegramMessage with correct signature
                            var sentEdit = await notification.EditTelegramMessage(
                                messageId: int.Parse(messageID),
                                caption: updatedCaption,
                                inlineKeyboard: null,  // ← removes Approve/Disapprove buttons
                                chatId: long.Parse(chatID),
                                topicID: result.BankSlip.TelegramTopicID ?? 0
                            );

                            // ── Notify OSP group on approval 
                            if (isDone && sentEdit.Result == ResultType.Successful)
                            {
                                // ── Fetch prefixes from TblCustomerType (mirrors your SQL pattern) 
                                var customerType = await ctx.TblCustomer
                                    .Where(c => c.CustomerID == customerId)
                                    .Join(ctx.TblCustomerBlock,
                                        c => c.CustomerTypeID,
                                        ct => ct.CustomerTypeID,
                                        (c, ct) => new
                                        {
                                            c.CustomerCode,
                                            ct.Prefix,
                                            ct.PrefixVAT,
                                            ct.PrefixCM,
                                            ct.StartNumber,
                                            ct.StartNumberPrefixVAT,
                                            ct.StartNumberPrefixCM
                                        })
                                    .FirstOrDefaultAsync();

                                if (customerType == null)
                                    throw new Exception("CustomerType not found — cannot generate CID/SID.");

                                // ── Resolve prefix & startNumber based on SetCID type 
                                // Determine PrefixType from customer's VAT/CM flags (same logic as SP)
                                string customerCode = customerType.CustomerCode;
                                string prefixCID = customerInvoice.TypeOfInvoice == "VAT" ? customerType.PrefixVAT
                                    : customerInvoice.TypeOfInvoice == "CM" ? customerType.PrefixCM
                                    : customerType.Prefix;

                                // Prevent new generate CID from old customer
                                bool isFirstConnection = string.IsNullOrEmpty(customerCode) || customerCode.Contains("Q-");

                                // ── Generate CID, SID, ConnectionID 
                                (string generatedCID, string generatedSID, string generatedConnectionID) = await GenerateID(
                                    cidPrefix: prefixCID,
                                    systemID: customerInvoice.SystemID,
                                    warehouseID: customerInvoice.WarehouseID,
                                    domainID: customerInvoice.DomainID,
                                    prefixType: customerInvoice.TypeOfInvoice,
                                    isFirstConnection: isFirstConnection
                                );

                                // ── Save CID, SID, ConnectionID 
                                generatedCID = isFirstConnection ? generatedCID : customerCode;
                                result.BankSlip.UCID = generatedCID;
                                result.BankSlip.USID = generatedSID;
                                result.BankSlip.ConnectionID = generatedConnectionID;

                                await SaveGeneratedIdsAsync(ctx, invoiceNo, customerInvoice.CustomerID, generatedCID, generatedSID, generatedConnectionID, customerInvoice.DomainID);

                                // FIX: build techCaption from originalCaption (clean, no approval line)
                                var lines = originalCaption.Split('\n').ToList();

                                int registerDateIndex = lines.FindIndex(line => line.Contains("Register Date"));
                                if (registerDateIndex >= 0)
                                {
                                    lines.Insert(registerDateIndex + 2, $"👉 <i><b>CID: {generatedCID}</b></i>");
                                    lines.Insert(registerDateIndex + 3, $"👉 <i><b>SID: {generatedSID}</b></i>");
                                }

                                string technicalCaption = string.Join("\n", lines);

                                // FIX: null check before using customerInvoice
                                if (customerInvoice == null)
                                    throw new Exception("Customer invoice not found — cannot generate PDF.");

                                var html = new HTML();
                                var pdfResult = await html.InvoiceToPDFandUpload(
                                    customerInvoice.CustomerName,
                                    generatedCID,
                                    customerInvoice.CustomerID,
                                    invoiceNo,
                                    customerInvoice.InvoiceDate.ToString(),
                                    Utility.TryParse(customerInvoice.IsIncludedVAT, "int", 0),
                                    11,
                                    int.Parse(html.SystemID.ToString()),
                                    int.Parse(html.WarehouseID.ToString()),
                                    false
                                );

                                var ospKeyboard = new InlineKeyboardMarkup(new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Finish Install ✅", $"FinishInstall~{invoiceNo}~{customerId}")
                                    }
                                });

                                var ospNotification = new NotificationTelegram();
                                var alertResult = await ospNotification.NotificationTelegramBot(
                                    MsgText: "",
                                    chatId: -1003778849283,
                                    replyMarkupInlineKeyboardMarkup: true,
                                    inlineKeyboard: ospKeyboard,
                                    PDFFile: pdfResult.OutPutFile,
                                    Caption: technicalCaption
                                );

                                if (alertResult.Result == ResultType.Successful)
                                {
                                    if (System.IO.File.Exists(pdfResult.OutPutFile))
                                        System.IO.File.Delete(pdfResult.OutPutFile);

                                    result.BankSlip.TelegramCaption = technicalCaption;
                                    await ctx.SaveChangesAsync();
                                }
                            }
                        }
                        catch (Exception telegramEx)
                        {
                            // DB already saved — don't fail the whole request
                            return Request.CreateResponse(HttpStatusCode.OK,
                                new { success = true, warning = "Saved but Telegram failed: " + telegramEx.Message });
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = $"{actionText} successfully" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/FinishInstallService")]
        public async Task<HttpResponseMessage> FinishInstallService()
        {
            try
            {
                // ── Parse form fields 
                string invoiceNo = HttpContext.Current.Request.Form["InvoiceNo"]?.ToString();
                string status = HttpContext.Current.Request.Form["Status"]?.ToString();
                string userName = HttpContext.Current.Request.Form["UserName"]?.ToString();
                string chatID = HttpContext.Current.Request.Form["ChatID"]?.ToString();
                string messageID = HttpContext.Current.Request.Form["MessageID"]?.ToString();
                string message = HttpContext.Current.Request.Form["Message"]?.ToString();

                if (string.IsNullOrEmpty(invoiceNo) || string.IsNullOrEmpty(status))
                    return Request.CreateResponse(HttpStatusCode.BadRequest,
                        new { success = false, message = "InvoiceNo and Status are required" });

                using (var ctx = new DBContext())
                {
                    // ── Fetch bank slip + approver 
                    var result = await ctx.TblCustomerBankSlip
                        .Where(x => x.InvoiceNo == invoiceNo)
                        .Join(ctx.TblUserLogin,
                            slip => slip.NeedApproveByID,
                            user => user.UserLoginID,
                            (slip, user) => new
                            {
                                BankSlip = slip,
                                ApproverID = user.UserLoginID,
                                ApproverFullName = user.FullName,
                                ApproverTelegram = user.TelegramUserName
                            })
                        .FirstOrDefaultAsync();

                    var customer = await ctx.TblCustomer
                        .Where(c => c.CustomerID == result.BankSlip.CustomerID)
                        .FirstOrDefaultAsync();

                    if (result == null)
                        return Request.CreateResponse(HttpStatusCode.NotFound,
                            new { success = false, message = "Bank slip not found" });

                    // ── Build captions 
                    bool isDone = status == "FinishInstall";
                    string actionIcon = isDone ? "✅" : "❌";
                    string actionText = isDone ? "Finish Install" : "";

                    string originalCaption = message ?? string.Empty;

                    string updatedCaption = originalCaption
                        + $"\n\n{actionIcon} <i><b>{actionText} by: @{userName} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}</b></i>";


                    // ── Edit original Telegram message ─────────────────────
                    if (!string.IsNullOrEmpty(chatID) && !string.IsNullOrEmpty(messageID))
                    {
                        try
                        {
                            var notification = new NotificationTelegram();

                            // FIX: use EditTelegramMessage with correct signature
                            var sentEdit = await notification.EditTelegramMessage(
                                messageId: int.Parse(messageID),
                                caption: updatedCaption,
                                inlineKeyboard: null,  // ← removes Approve/Disapprove buttons
                                chatId: long.Parse(chatID),
                                topicID: result.BankSlip.TelegramTopicID ?? 0
                            );

                            if (isDone && sentEdit.Result == ResultType.Successful)
                            {
                                //Re-alert to group New Install
                                await notification.NotificationTelegramBotToTopic(
                                    MsgText: updatedCaption,
                                    chatId: -1001545132903,
                                    topicID: 2
                                );

                                result.BankSlip.IsDoneInstall = true;
                                result.BankSlip.DoneInstallDate = DateTime.Now;
                                result.BankSlip.FinishInstallMessageID = int.Parse(messageID);
                                await ctx.SaveChangesAsync();

                                //For finance team
                                string customerName =
                                    !string.IsNullOrWhiteSpace(customer?.CustomerName)
                                        ? customer.CustomerName
                                        : customer?.CustomerNameLatin ?? "-";
                                var msg = new StringBuilder();
                                msg.AppendLine("✅ <b>NEW INSTALLATION COMPLETED</b>\n");
                                msg.AppendLine($"<b><i>👉 Name: </i> {customerName}</b>");
                                msg.AppendLine($"<i>👉 CID: <b>{result.BankSlip.UCID}</b></i>");
                                msg.AppendLine($"<i>👉 SID: <b>{result.BankSlip.USID}</b></i>");
                                msg.AppendLine($"<i>👉 Installed Date: <b>{result.BankSlip.DoneInstallDate:dd-MM-yyyy HH:mm}</b></i>\n");
                                msg.AppendLine("Please prepare and issue the invoice.");

                                await notification.NotificationTelegramBot(
                                    MsgText: msg.ToString(),
                                    chatId: -5484785634
                                );
                            }
                        }
                        catch (Exception telegramEx)
                        {
                            // DB already saved — don't fail the whole request
                            return Request.CreateResponse(HttpStatusCode.OK,
                                new { success = true, warning = "Saved but Telegram failed: " + telegramEx.Message });
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = $"{actionText} successfully" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/InvoiceApproval")]
        public async Task<HttpResponseMessage> InvoiceApproval()
        {
            try
            {
                var para = new Dictionary<string, string>();
                var html = new HTML();
                var sent = new SqlResult();

                // ── Parse form fields 
                string invoiceNo = HttpContext.Current.Request.Form["InvoiceNo"]?.ToString();
                int customerId = Utility.TryParse(HttpContext.Current.Request.Form["CustomerID"]?.ToString(), "int", 0);
                string status = HttpContext.Current.Request.Form["Status"]?.ToString();
                string userName = HttpContext.Current.Request.Form["UserName"]?.ToString();
                string chatID = HttpContext.Current.Request.Form["ChatID"]?.ToString();
                int topicID = Utility.TryParse(HttpContext.Current.Request.Form["TopicID"]?.ToString(), "int", 0);
                string messageID = HttpContext.Current.Request.Form["MessageID"]?.ToString();
                string message = HttpContext.Current.Request.Form["Message"]?.ToString();

                if (string.IsNullOrEmpty(invoiceNo) || string.IsNullOrEmpty(status))
                    return Request.CreateResponse(HttpStatusCode.BadRequest,
                        new { success = false, message = "InvoiceNo and Status are required" });

                using (var ctx = new DBContext())
                {
                    // ── Build captions 
                    bool isApprove = status == "ApproveInvoice";
                    bool isCheck = status == "CheckInvoice";
                    string actionIcon = isApprove ? "✅"
                        : isCheck ? "✍️"
                        : "❌";
                    string actionText = isApprove ? "Approved"
                        : isCheck ? "Checked"
                        : "Disapproved";

                    string originalCaption = message ?? string.Empty;

                    string updatedCaption = originalCaption
                        + $"\n\n{actionIcon} <i><b>{actionText} by: @{userName} at {DateTime.Now:dd-MM-yyyy HH:mm:ss}</b></i>";


                    // ── Edit original Telegram message ─────────────────────
                    if (!string.IsNullOrEmpty(chatID) && !string.IsNullOrEmpty(messageID))
                    {
                        try
                        {
                            var notification = new NotificationTelegram();

                            // FIX: use EditTelegramMessage with correct signature
                            sent = await notification.EditTelegramMessage(
                                messageId: int.Parse(messageID),
                                caption: updatedCaption,
                                inlineKeyboard: null,
                                chatId: long.Parse(chatID),
                                topicID: topicID
                            );

                            // ── Notify group on approval ───────────────
                            if (isCheck && sent.Result == ResultType.Successful)
                            {
                                // Re-alert to group DMC approval
                                var keyboard = new InlineKeyboardMarkup(new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData($"Approve ✅", $"ApproveInvoice~{invoiceNo}~{customerId}"),
                                        InlineKeyboardButton.WithCallbackData($"Disapprove ❌", $"DisapproveInvoice~{invoiceNo}~{customerId}")
                                    }
                                });

                                var customer = await (
                                    from c in ctx.TblCustomer
                                    join ci in ctx.TblCustomerInvoice
                                        on c.CustomerID equals ci.CustomerID
                                    where c.CustomerID == customerId
                                       && ci.InvoiceNo == invoiceNo
                                    select new
                                    {
                                        c.CustomerID,
                                        c.CustomerName,
                                        c.CustomerCode,
                                        ci.InvoiceNo,
                                        ci.InvoiceDate,
                                        ci.IsIncludedVAT,
                                        ci.ClassName,
                                        ci.RequestApprovalToDMCTAX,
                                        ci.RequestApprovalToDMCTAXTgUser,
                                        ci.RequestedByUserToDMCTAX,
                                        ci.TypeOfInvoice,
                                        ci.InvoiceType,
                                        ci.OldInvoiceNo,
                                        ci.InvoiceStatusID,
                                        ci.ItemDescription,
                                        ci.InvoiceNote
                                    }
                                ).FirstOrDefaultAsync();

                                var billInfo = await ctx.TblCustomerBillInformation
                                    .Where(x => x.CustomerID == customerId && x.InvoiceNo == invoiceNo)
                                    .Join(
                                        ctx.TblIspLink,
                                        bill => bill.IspLinkID,
                                        isp => isp.IspLinkID,
                                        (bill, isp) => new
                                        {
                                            bill.GoogleMap_FullAddress,
                                            bill.GoogleMap_Latitude,
                                            bill.GoogleMap_Longitude,
                                            isp.IspLinkName,
                                        })
                                    .FirstOrDefaultAsync();
                                //handle pdf file
                                var pdfResult = await html.InvoiceToPDFandUpload(
                                            customer?.CustomerName, customer?.CustomerCode, customerId, invoiceNo,
                                            customer?.InvoiceDate.ToString(), Utility.TryParse(customer?.IsIncludedVAT.ToString(), "int", 0), 11,
                                            int.Parse(html.SystemID.ToString()),
                                            int.Parse(html.WarehouseID.ToString()), false
                                        );

                                long chatId = customer?.ClassName == "NSM" ? -1002042086477 : -5009096861;
                                string approver = customer?.RequestApprovalToDMCTAX;
                                string username = customer?.RequestApprovalToDMCTAXTgUser;
                                //string msgApproval = message;
                                var msgApproval = new StringBuilder();
                                string requester = customer?.RequestedByUserToDMCTAX;
                                string typeOfInvoice = customer?.TypeOfInvoice;
                                string invoiceType = customer?.InvoiceType;
                                string oldInvoice = customer?.OldInvoiceNo;
                                int? statusId = customer?.InvoiceStatusID;

                                string remark = "";
                                string invNote = customer?.InvoiceNote ?? "";

                                if (!string.IsNullOrWhiteSpace(customer?.ItemDescription))
                                {
                                    const string marker = ";Remark:";

                                    int idx = customer.ItemDescription.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

                                    if (idx >= 0)
                                    {
                                        remark = "<b>Remark:</b> " +
                                                 customer.ItemDescription
                                                     .Substring(idx + marker.Length)
                                                     .Trim();
                                    }
                                }

                                if (typeOfInvoice == "None VAT")
                                {
                                    msgApproval.AppendLine($"{username}\n");
                                    msgApproval.AppendLine($"Dear {approver},\n");
                                    msgApproval.AppendLine($"Now, you have invoice need to check.\n");
                                    msgApproval.AppendLine($"<i>ISP LINK: {billInfo?.IspLinkName}.</i>\n");
                                    if (!string.IsNullOrEmpty(remark))
                                        msgApproval.AppendLine($"<i>{remark}.</i>\n");
                                    if (!string.IsNullOrEmpty(invNote))
                                        msgApproval.AppendLine($"<i>Invoice Note: {invNote}.</i>\n");
                                    msgApproval.AppendLine($"Request by: {requester}.\n");
                                    msgApproval.AppendLine($"<b><i>ISP No.: #{invoiceNo}.</i></b>");
                                    msgApproval.AppendLine($"<b><i>Status: {(invoiceType == "Created Invoice" ? "New Register" : "Renew Invoice")}.</i></b>\n");
                                }
                                else
                                {
                                    msgApproval.AppendLine($"{username}\n");
                                    msgApproval.AppendLine($"Dear {approver},\n");
                                    msgApproval.AppendLine($"Now, you have invoice for DMC and TAX System need to check.\n");
                                    msgApproval.AppendLine($"<i>ISP LINK: {billInfo?.IspLinkName}.</i>\n");
                                    if (!string.IsNullOrEmpty(remark))
                                        msgApproval.AppendLine($"<i>{remark}.</i>\n");
                                    if (!string.IsNullOrEmpty(invNote))
                                        msgApproval.AppendLine($"<i>Invoice Note: {invNote}.</i>\n");
                                    msgApproval.AppendLine($"Request by: {requester}.\n");
                                    msgApproval.AppendLine($"<b><i>NSM No.: #{invoiceNo}.</i></b>");
                                    msgApproval.AppendLine($"<b><i>Status: {(invoiceType == "Created Invoice" ? "New Register" : "Renew Invoice")}.</i></b>\n");
                                }
                                message = msgApproval.ToString();

                                await notification.NotificationTelegramBot(
                                     "",
                                     chatId,
                                     PDFFile: pdfResult?.OutPutFile,
                                     Caption: message,
                                     replyMarkupInlineKeyboardMarkup: true,
                                     inlineKeyboard: keyboard,
                                     edit: false
                                 );
                                System.IO.File.Delete(pdfResult.OutPutFile);
                            }

                            string sql = @"update dbo.TblCustomerInvoice set InvoiceStatusID = @InvoiceStatusID, ApproveRequestToDMCTAXTGMID = @MessageID, ApproveRequestToDMCTAXMsg = @Message
					where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo";
                            para.Add("InvoiceStatusID", isApprove ? "90" : isCheck ? "92" : "101");
                            para.Add("CustomerID", customerId.ToString());
                            para.Add("InvoiceNo", invoiceNo);
                            para.Add("MessageID", sent.Value);
                            para.Add("Message", updatedCaption);
                            html.SaveChangeData(sql, false, para);
                        }
                        catch (Exception telegramEx)
                        {
                            // DB already saved — don't fail the whole request
                            return Request.CreateResponse(HttpStatusCode.OK,
                                new { success = true, warning = "Saved but Telegram failed: " + telegramEx.Message });
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = $"{actionText} successfully" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { success = false, message = ex.Message });
            }
        }

        public async Task<(string CID, string SID, string ConnectionID)> GenerateID(string cidPrefix, int systemID, int warehouseID, string domainID, string prefixType, bool isFirstConnection = false)
        {
            using (var ctx = new DBContext())
            {
                string cid = "";
                string connectionID;
                string sid;

                // ── CID always checks TblCustomer
                if (isFirstConnection)
                {
                    cid = await GetNextPrefix(
                        ctx, cidPrefix, "Customer",
                        systemID, warehouseID, domainID, prefixType,
                        checkTable: "TblCustomer",
                        checkColumn: "CustomerCode"
                    );
                }

                if (prefixType == "VAT" || prefixType == "CM")
                {
                    // ── VAT/CM: CON{000000} + SID{000000}
                    connectionID = await GetNextPrefix(
                        ctx, "CON{000000}", "VATConnection",
                        systemID, warehouseID, domainID, prefixType,
                        checkTable: "TblCustomerBillingInvoice",
                        checkColumn: "ConnectionID");

                    sid = await GetNextPrefix(
                        ctx, "SID{000000}", "VAT Service ID",
                        systemID, warehouseID, domainID, prefixType,
                        checkTable: "TblCustomerBillingInvoice",
                        checkColumn: "SID");
                }
                else
                {
                    // ── None VAT: CISP{000000} + ISP{000000}
                    connectionID = await GetNextPrefix(
                        ctx, "CISP{000000}", "NoneVATConnectionID",
                        systemID, warehouseID, domainID, prefixType,
                        checkTable: "TblCustomerBillingInvoice",
                        checkColumn: "ConnectionID");

                    sid = await GetNextPrefix(
                        ctx, "ISP{000000}", "NoneVATServiceID",
                        systemID, warehouseID, domainID, prefixType,
                        checkTable: "TblCustomerBillingInvoice",
                        checkColumn: "SID");
                }

                return (cid, sid, connectionID);
            }
        }

        private async Task<string> GetNextPrefix(DBContext ctx, string prefix, string module, int systemID, int warehouseID, string domainID, string prefixType, string checkTable = "TblCustomer", string checkColumn = "CustomerCode")
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(ctx.Database.Connection.ConnectionString))
            {
                await conn.OpenAsync();

                // ── Get current number and go back 5 to fill gaps
                int tmpsNum = 1;
                using (var cmdStart = new System.Data.SqlClient.SqlCommand(
                    @"SELECT ISNULL(CurrentNumber, 0) + 1 
                      FROM dbo.TblPrefix 
                      WHERE Prefix      = @Prefix 
                      AND   Module      = @Module 
                      AND   SystemID    = @SystemID 
                      AND   WarehouseID = @WarehouseID 
                      AND   DomainID LIKE '%' + @DomainID + '%'", conn)
                )
                {
                    cmdStart.Parameters.AddWithValue("@Prefix", prefix);
                    cmdStart.Parameters.AddWithValue("@Module", module);
                    cmdStart.Parameters.AddWithValue("@SystemID", systemID);
                    cmdStart.Parameters.AddWithValue("@WarehouseID", warehouseID);
                    cmdStart.Parameters.AddWithValue("@DomainID", "001");

                    var obj = await cmdStart.ExecuteScalarAsync();
                    tmpsNum = obj != null ? Convert.ToInt32(obj) : 1;
                }

                // ── Go back 5 to reclaim any gaps (mirrors original SP) 
                if (tmpsNum > 6)
                    tmpsNum = tmpsNum - 5;

                for (int attempt = 0; attempt < 10; attempt++)
                {
                    // ── Step 1: Preview candidate 
                    string candidate = null;
                    using (var cmdShow = new System.Data.SqlClient.SqlCommand("dbo.SP_SHOW_PREFIX", conn))
                    {
                        cmdShow.CommandType = System.Data.CommandType.StoredProcedure;
                        cmdShow.Parameters.AddWithValue("@Prefix", prefix);
                        cmdShow.Parameters.AddWithValue("@Module", module);
                        cmdShow.Parameters.AddWithValue("@SystemID", systemID);
                        cmdShow.Parameters.AddWithValue("@WarehouseID", warehouseID);
                        cmdShow.Parameters.AddWithValue("@DomainID", domainID);
                        cmdShow.Parameters.AddWithValue("@StartNumber", tmpsNum);
                        cmdShow.Parameters.AddWithValue("@ShowSelectResult", 1);
                        cmdShow.Parameters.AddWithValue("@DefaultPrefix", prefix);
                        cmdShow.Parameters.AddWithValue("@GetPrefixOnly", 1);

                        var prefixOut = new System.Data.SqlClient.SqlParameter("@PrefixNumber", System.Data.SqlDbType.NVarChar, 50)
                        {
                            Direction = System.Data.ParameterDirection.Output,
                            Value = string.Empty
                        };
                        cmdShow.Parameters.Add(prefixOut);

                        await cmdShow.ExecuteNonQueryAsync();
                        candidate = prefixOut.Value?.ToString();
                    }

                    if (string.IsNullOrEmpty(candidate))
                        throw new Exception($"SP_SHOW_PREFIX returned empty for prefix '{prefix}' at attempt {attempt + 1}.");

                    // ── Step 2: Check uniqueness against correct table/column 
                    // CID  → TblCustomer.CustomerCode
                    // CON  → TblCustomerBillingInvoice.ConnectionID (exclude StatusID 12,13,14,20)
                    // SID  → TblCustomerBillingInvoice.SID          (exclude StatusID 12,13,14,20)
                    bool exists = false;

                    string checkSql = checkTable == "TblCustomerBillingInvoice"
                        ? $@"SELECT 1 FROM dbo.TblCustomerBillingInvoice
                     WHERE {checkColumn} = @Code
                     AND   SystemID      = @SystemID
                     AND   WarehouseID   = @WarehouseID
                     AND   DomainID LIKE '%' + @DomainID + '%'
                     AND   StatusID NOT IN (12, 13, 14, 20)"   // ← mirrors original SP
                        : $@"SELECT 1 FROM dbo.TblCustomer
                     WHERE CustomerCode = @Code
                     AND   SystemID     = @SystemID
                     AND   WarehouseID  = @WarehouseID
                     AND   DomainID LIKE '%' + @DomainID + '%'";

                    using (var cmdCheck = new System.Data.SqlClient.SqlCommand(checkSql, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@Code", candidate);
                        cmdCheck.Parameters.AddWithValue("@SystemID", systemID);
                        cmdCheck.Parameters.AddWithValue("@WarehouseID", warehouseID);
                        //cmdCheck.Parameters.AddWithValue("@DomainID", domainID);
                        cmdCheck.Parameters.AddWithValue("@DomainID", "001");
                        exists = await cmdCheck.ExecuteScalarAsync() != null;
                    }

                    if (!exists)
                    {
                        // ── Step 3: Reset CurrentNumber 
                        using (var cmdReset = new System.Data.SqlClient.SqlCommand(
                            @"UPDATE dbo.TblPrefix 
                      SET   CurrentNumber = @StartNumber - 1
                      WHERE Prefix        = @Prefix 
                      AND   Module        = @Module 
                      AND   SystemID      = @SystemID 
                      AND   WarehouseID   = @WarehouseID 
                      AND   DomainID LIKE '%' + @DomainID + '%'", conn))
                        {
                            cmdReset.Parameters.AddWithValue("@StartNumber", tmpsNum);
                            cmdReset.Parameters.AddWithValue("@Prefix", prefix);
                            cmdReset.Parameters.AddWithValue("@Module", module);
                            cmdReset.Parameters.AddWithValue("@SystemID", systemID);
                            cmdReset.Parameters.AddWithValue("@WarehouseID", warehouseID);
                            cmdReset.Parameters.AddWithValue("@DomainID", domainID);
                            await cmdReset.ExecuteNonQueryAsync();
                        }

                        // ── Step 4: Commit via SP_INSERT_PREFIX 
                        string generatedCode = null;
                        using (var cmdInsert = new System.Data.SqlClient.SqlCommand("dbo.SP_INSERT_PREFIX", conn))
                        {
                            cmdInsert.CommandType = System.Data.CommandType.StoredProcedure;
                            cmdInsert.Parameters.AddWithValue("@Prefix", prefix);
                            cmdInsert.Parameters.AddWithValue("@Module", module);
                            cmdInsert.Parameters.AddWithValue("@SystemID", systemID);
                            cmdInsert.Parameters.AddWithValue("@WarehouseID", warehouseID);
                            cmdInsert.Parameters.AddWithValue("@StartNumber", tmpsNum);
                            cmdInsert.Parameters.AddWithValue("@OutResutl", 0);
                            cmdInsert.Parameters.AddWithValue("@DomainID", domainID);

                            var outPrefix = new System.Data.SqlClient.SqlParameter("@OutPrefix", System.Data.SqlDbType.NVarChar, 50)
                            {
                                Direction = System.Data.ParameterDirection.Output,
                                Value = string.Empty
                            };
                            cmdInsert.Parameters.Add(outPrefix);

                            await cmdInsert.ExecuteNonQueryAsync();
                            generatedCode = outPrefix.Value?.ToString();
                        }

                        // ── Step 5: Update TblCustomerType StartNumber 
                        string updateSql = null;

                        if (prefixType == "None VAT")
                        {
                            updateSql = @"UPDATE dbo.TblCustomerType 
                                  SET StartNumber = @NextNumber
                                  WHERE [Status]    = 'Enable'
                                  AND   Prefix      = @Prefix
                                  AND   SystemID    = @SystemID
                                  AND   WarehouseID = @WarehouseID
                                  AND   DomainID LIKE '%' + @DomainID + '%'";
                        }
                        else if (prefixType == "VAT")
                        {
                            updateSql = @"UPDATE dbo.TblCustomerType 
                                  SET StartNumberPrefixVAT = @NextNumber,
                                      StartNumberPrefixCM  = @NextNumber
                                  WHERE [Status]    = 'Enable'
                                  AND   PrefixVAT   = @Prefix
                                  AND   SystemID    = @SystemID
                                  AND   WarehouseID = @WarehouseID
                                  AND   DomainID LIKE '%' + @DomainID + '%'";
                        }
                        else if (prefixType == "CM")
                        {
                            updateSql = @"UPDATE dbo.TblCustomerType 
                                  SET StartNumberPrefixVAT = @NextNumber,
                                      StartNumberPrefixCM  = @NextNumber
                                  WHERE [Status]    = 'Enable'
                                  AND   PrefixCM    = @Prefix
                                  AND   SystemID    = @SystemID
                                  AND   WarehouseID = @WarehouseID
                                  AND   DomainID LIKE '%' + @DomainID + '%'";
                        }

                        if (updateSql != null)
                        {
                            using (var cmdUpdateType = new System.Data.SqlClient.SqlCommand(updateSql, conn))
                            {
                                cmdUpdateType.Parameters.AddWithValue("@NextNumber", tmpsNum + 1);
                                cmdUpdateType.Parameters.AddWithValue("@Prefix", prefix);
                                cmdUpdateType.Parameters.AddWithValue("@SystemID", systemID);
                                cmdUpdateType.Parameters.AddWithValue("@WarehouseID", warehouseID);
                                cmdUpdateType.Parameters.AddWithValue("@DomainID", domainID);
                                await cmdUpdateType.ExecuteNonQueryAsync();
                            }
                        }

                        return generatedCode; // ✅
                    }

                    tmpsNum++;
                }

                throw new Exception($"Could not generate unique code for prefix '{prefix}' after 10 attempts.");
            }
        }

        private async Task SaveGeneratedIdsAsync(
            DBContext ctx,
            string invoiceNo,
            int customerId,
            string cid,
            string sid,
            string connectionId,
            string domainId)
        {
            // =========================
            // CUSTOMER
            // =========================
            var customer = await ctx.TblCustomer
                .FirstOrDefaultAsync(c => c.CustomerID == customerId);

            if (customer != null)
            {
                customer.CustomerCode = cid;
                customer.DomainID = domainId;
            }

            // =========================
            // CUSTOMER INVOICE
            // =========================
            var invoices = await ctx.TblCustomerInvoice
                .Where(i =>
                    i.CustomerID == customerId &&
                    i.InvoiceNo == invoiceNo)
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                invoice.CustomerCode = cid;
                invoice.SID = sid;
                invoice.ConnectionID = connectionId;
            }

            // =========================
            // BILLING INVOICE
            // =========================
            var billings = await ctx.TblCustomerBillingInvoice
                .Where(b =>
                    b.CustomerID == customerId &&
                    b.InvoiceNo == invoiceNo)
                .ToListAsync();

            foreach (var billing in billings)
            {
                billing.SID = sid;
                billing.ConnectionID = connectionId;
            }

            // =========================
            // BILL INFORMATION
            // =========================
            var billInfos = await ctx.TblCustomerBillInformation
                .Where(bi =>
                    bi.CustomerID == customerId &&
                    bi.InvoiceNo == invoiceNo)
                .ToListAsync();

            foreach (var billInfo in billInfos)
            {
                billInfo.CustomerCode = cid;
                billInfo.ConnectionID = connectionId;
            }

            // =========================
            // CUSTOMER INFRA
            // =========================
            var customerInfras = await ctx.TblCustomerInfra
                .Where(i =>
                    i.InvoiceNo == invoiceNo &&
                    i.CustomerID == customerId)
                .ToListAsync();

            foreach (var infra in customerInfras)
            {
                infra.CID = cid;
                infra.SID = sid;
                infra.ConnectionID = connectionId;
            }

            // =========================
            // SAVE
            // =========================
            await ctx.SaveChangesAsync();
        }
    }
}