using HtmlAgilityPack;
using MVC;
using Newtonsoft.Json;
using NSMSOLUTION.Controllers.Expenses.Models;
using NSMSOLUTION.Helper_Code;
using NSMSOLUTION.Models.Expenses;
using NSMSOLUTION.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;
using static NSMSOLUTION.Models.Expenses.ExpenseRepository;

namespace NSMSOLUTION.Controllers.Expenses
{
    [Authorize]
    public class ExpenseTransactionsController : Controller
    {
        private readonly ExpenseRepository _repo;
        private string botToken = "";
        private long chatId = 0;
        private int? TelegramGroupTopicID = 0;
        private TelegramBotClient botClient;
        //private readonly PuppeteerPdfService _pdfService;
        public ExpenseTransactionsController()
        {
            _repo = new ExpenseRepository(
                ConfigurationManager.ConnectionStrings["AuthContextNSM"].ConnectionString
            );

            HTML html = new HTML();

            string sql = @"SELECT b.BotToken, t.TelegramGroupID, isnull(t.TelegramGroupTopicID, 0) TelegramGroupTopicID
                       FROM TblTelegramGroupNotification t
                       INNER JOIN dbo.TblTelegramBots b ON b.BotID = t.BotID
                       WHERE t.Module = 'Expense'
                       AND t.SystemID = " + html.SystemID + @"
                       AND t.WarehouseID = " + html.WarehouseID;

            DataSet ds = html.LoadData(sql);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                botToken = ds.Tables[0].Rows[0]["BotToken"].ToString();
                chatId = long.Parse(ds.Tables[0].Rows[0]["TelegramGroupID"].ToString());
                TelegramGroupTopicID = int.Parse(ds.Tables[0].Rows[0]["TelegramGroupTopicID"].ToString());
                botClient = new TelegramBotClient(botToken);
            }
        }
        // GET: Expenses
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> SendVoucherToTelegram(int id, string PaymentNo)
        {
            bool result = await SendVoucherInternal(id, PaymentNo);

            if (result)
                return Json(new { success = true, message = "Voucher sent to Telegram successfully." });

            return Json(new { success = false, message = "Failed to send voucher." });
        }

        public async Task<byte[]> GenerateVoucherPdfBytes(int id, string PaymentNo)
        {
            var model = _repo.GetVoucherByExpenseId(id, PaymentNo);

            if (model == null)
                return null;

            // Render PaymentVoucher.cshtml
            var html = ViewRenderHelper.RenderViewToString(ControllerContext, "~/Views/Expenses/PaymentVoucher.cshtml", model);

            // Current application URL
            var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);

            var pdfService = new PuppeteerPdfService();

            return await pdfService.GeneratePdfAsync(html, baseUrl);
        }
        //public byte[] GenerateVoucherPdfBytes(int id, string PaymentNo)
        //{
        //    var model = _repo.GetVoucherByExpenseId(id, PaymentNo);

        //    if (model == null)
        //        return null;

        //    var pdf = new ViewAsPdf("PaymentVoucher", model)
        //    {
        //        FileName = $"PaymentVoucher_{id}.pdf",
        //        PageSize = Size.A4,
        //        PageOrientation = Orientation.Portrait,
        //        PageMargins = new Margins(5, 5, 10, 5),
        //        CustomSwitches =
        //            "--print-media-type " +
        //            "--enable-local-file-access " +
        //            "--encoding utf-8 " +
        //            "--load-error-handling ignore " +
        //            "--no-stop-slow-scripts " +
        //            "--background " +
        //            //"--footer-line " +
        //            "--footer-center \"Page [page] of [toPage]\" " +
        //            "--footer-font-size 7 " +
        //            "--footer-spacing 2"
        //    };

        //    return pdf.BuildFile(ControllerContext);
        //}
        [AllowAnonymous]
        public async Task<ActionResult> PrintVoucherPdf(int id)
        {
            var model = _repo.GetVoucherByExpenseId(id);

            if (model == null)
                return HttpNotFound();

            // Render PaymentVoucher.cshtml
            var html = ViewRenderHelper.RenderViewToString(ControllerContext, "PaymentVoucher", model);

            // Current application URL
            var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);

            var pdfService = new PuppeteerPdfService();

            var pdfBytes = await pdfService.GeneratePdfAsync(html, baseUrl);

            return File(pdfBytes, "application/pdf", $"PaymentVoucher_{id}.pdf");
        }
        //public ActionResult PrintVoucherPdf(int id)
        //{
        //    var model = _repo.GetVoucherByExpenseId(id);

        //    if (model == null)
        //        return HttpNotFound();

        //    return new ViewAsPdf("PaymentVoucher", model)
        //    {
        //        FileName = $"PaymentVoucher_{id}.pdf",
        //        PageSize = Size.A4,
        //        PageOrientation = Orientation.Portrait,
        //        PageMargins = new Margins(5, 5, 10, 5),
        //        CustomSwitches =
        //            "--print-media-type " +
        //            "--enable-local-file-access " +
        //            "--encoding utf-8 " +
        //            "--load-error-handling ignore " +
        //            "--no-stop-slow-scripts " +
        //            "--background " +
        //            //"--footer-line " +
        //            "--footer-center \"Page [page] of [toPage]\" " +
        //            "--footer-font-size 7 " +
        //            "--footer-spacing 2"
        //    };
        //}
        public ActionResult PrintVoucher(int id, string PaymentNo = "")
        {
            var model = _repo.GetVoucherByExpenseId(id, PaymentNo);

            return model == null ? HttpNotFound() : (ActionResult)View("PaymentVoucher", model);
        }

        // ============================================================
        // TELEGRAM TOPIC CONFIGURATION (OPTION 2: FROM TblTelegramGroupNotification & TblTelegramBots)
        // ============================================================
        private (string BotToken, long ChatId, int TopicId, bool IsTopic) GetStaticTelegramConfig(int systemId, int warehouseId = 0, string module = "Expense")
        {
            HTML html = new HTML();
            string sql = @"
SELECT TOP 1 
    b.BotToken, 
    t.TelegramGroupID, 
    ISNULL(t.TelegramGroupTopicID, 0) AS TelegramGroupTopicID, 
    t.TelegramGroupType
FROM dbo.TblTelegramGroupNotification t
LEFT JOIN dbo.TblTelegramBots b 
    ON b.BotID = t.BotID
WHERE 
    t.Module = @Module
    AND t.SystemID = @SystemID
    AND (t.WarehouseID = @WarehouseID OR ISNULL(@WarehouseID, 0) = 0 OR ISNULL(t.WarehouseID, 0) = 0)
ORDER BY 
    CASE WHEN t.WarehouseID = @WarehouseID THEN 0 ELSE 1 END";

            var para = new Dictionary<string, string>
            {
                { "Module", module },
                { "SystemID", systemId.ToString() },
                { "WarehouseID", warehouseId.ToString() }
            };

            DataSet ds = html.LoadData(sql, false, para);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                string token = row["BotToken"]?.ToString() ?? "";
                long groupChatId = long.TryParse(row["TelegramGroupID"]?.ToString(), out var cid) ? cid : 0;
                int groupTopicId = int.TryParse(row["TelegramGroupTopicID"]?.ToString(), out var tid) ? tid : 0;
                bool isTopic = (row["TelegramGroupType"]?.ToString() == "Topic") || (groupTopicId > 0);

                return (token, groupChatId, groupTopicId, isTopic);
            }

            // Fallback: Default static mappings if database configuration is missing
            long fallbackChatId;
            int fallbackTopicId;
            switch (systemId)
            {
                case 13: // NSM & NSMVAT
                    fallbackChatId = -1002410808979;
                    fallbackTopicId = module == "AdvanceExpense" ? 1 : 4;
                    break;
                //case 25: // BSM
                //    fallbackChatId = -1002410808979;
                //    fallbackTopicId = module == "AdvanceExpense" ? 1 : 1242;
                //    break;
                case 27: // BS
                case 28: // NSMTech
                default:
                    fallbackChatId = -1002410808979;
                    fallbackTopicId = 1061;
                    break;
            }
            string fallbackBotToken = html.GetBotTokenBySystemID(systemId);

            return (fallbackBotToken, fallbackChatId, fallbackTopicId, true);
        }

        private async Task<bool> SendVoucherInternal(int id, string PaymentNo)
        {
            try
            {
                // ============================================================
                // STEP 1: PRE-RENDER RAZOR VIEW ON REQUEST THREAD (FAST ~10ms)
                // ============================================================
                var model = _repo.GetVoucherByExpenseId(id, PaymentNo);

                if (model == null)
                {
                    Console.WriteLine(
                        $"Payment voucher data not found for ExpenseID={id}, PaymentNo={PaymentNo}");

                    return false;
                }

                // Render PaymentVoucher.cshtml while ControllerContext is still active
                string htmlContent = ViewRenderHelper.RenderViewToString(ControllerContext, "~/Views/Expenses/PaymentVoucher.cshtml", model);

                // Current application URL while Request is active
                string baseUrl = Request != null && Request.Url != null ? Request.Url.GetLeftPart(UriPartial.Authority) : null;

                // Capture user info and system config before switching to background thread
                string submittedBy = "";
                int currentSystemId = 0;
                int currentWarehouseId = 0;
                try
                {
                    HTML userHtml = new HTML();
                    submittedBy = userHtml.UserFullName ?? "";
                    int.TryParse(userHtml.SystemID, out currentSystemId);
                    int.TryParse(userHtml.WarehouseID, out currentWarehouseId);
                }
                catch { }

                // ============================================================
                // RUN PDF GENERATION, TELEGRAM SEND & DB UPDATE IN BACKGROUND TASK
                // ============================================================
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Generate PDF in background without blocking the UI response (~2.5-3.5s)
                        var pdfService = new PuppeteerPdfService();
                        byte[] pdfBytes = await pdfService.GeneratePdfAsync(htmlContent, baseUrl);

                        if (pdfBytes == null || pdfBytes.Length == 0)
                        {
                            Console.WriteLine(
                                "Payment voucher PDF generation failed in background.");

                            return;
                        }

                        // ============================================================
                        // STEP 2: LOAD PAYMENT VOUCHER INFORMATION
                        // ============================================================

                        HTML html = new HTML();

                        string safePaymentNo =
                            (PaymentNo ?? string.Empty)
                                .Replace("'", "''");

                        // Query expense voucher data without restricting by caller's session SystemID for multi-system support
                        string sql = $@"
SELECT 
    e.CheckedByTelegramUserName,
    u.FullName,
    e.[No],
    ISNULL(e.TelegramMessageID, 0) AS TelegramMessageID,
    ISNULL(e.TelegramChatID, 0) AS TelegramChatID,
    e.PaymentNo,
    e.SystemID,
    e.WarehouseID
FROM dbo.Expense_PaymentTransactions e
INNER JOIN dbo.TblUserLogin u
    ON u.UserLoginID = e.CheckedByUserID
WHERE
    e.ExpenseID = {id}
    AND (
        '{safePaymentNo}' = ''
        OR e.PaymentNo = '{safePaymentNo}'
    )";

                        DataSet ds =
                            html.LoadData(sql);

                        if (ds == null ||
                            ds.Tables.Count == 0 ||
                            ds.Tables[0].Rows.Count == 0)
                        {
                            Console.WriteLine(
                                "Payment voucher information not found.");

                            return;
                        }

                        DataRow row =
                            ds.Tables[0].Rows[0];


                        // ============================================================
                        // STEP 3: READ DATA & RESOLVE STATIC TELEGRAM TOPIC CONFIG
                        // ============================================================

                        string No =
                            row["No"] == DBNull.Value
                                ? string.Empty
                                : row["No"].ToString();

                        int oldMessageId =
                            row["TelegramMessageID"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(
                                    row["TelegramMessageID"]);

                        long oldChatId =
                            row["TelegramChatID"] == DBNull.Value
                                ? 0
                                : Convert.ToInt64(
                                    row["TelegramChatID"]);

                        string checkedByTelegramUserName =
                            row["CheckedByTelegramUserName"] == DBNull.Value
                                ? string.Empty
                                : row["CheckedByTelegramUserName"].ToString();

                        string checkedByFullName =
                            row["FullName"] == DBNull.Value
                                ? string.Empty
                                : row["FullName"].ToString();

                        int systemId =
                            row["SystemID"] == DBNull.Value
                                ? currentSystemId
                                : Convert.ToInt32(row["SystemID"]);

                        int warehouseId =
                            row["WarehouseID"] == DBNull.Value
                                ? currentWarehouseId
                                : Convert.ToInt32(row["WarehouseID"]);

                        // Resolve static multi-system Telegram Topic configuration
                        var tgConfig = GetStaticTelegramConfig(systemId, warehouseId, "Expense");

                        if (string.IsNullOrEmpty(tgConfig.BotToken) || tgConfig.ChatId == 0)
                        {
                            Console.WriteLine(
                                $"Static Telegram config not found for SystemID={systemId}");

                            return;
                        }

                        TelegramBotClient activeBotClient = new TelegramBotClient(tgConfig.BotToken);


                        // ============================================================
                        // STEP 4: INLINE KEYBOARD
                        // ============================================================

                        var inlineKeyboard =
                            new InlineKeyboardMarkup(
                                new[]
                                {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(
                                    "✅ Check",
                                    $"ApprovePV~{id}~{checkedByTelegramUserName}~{PaymentNo}"
                                ),

                                InlineKeyboardButton.WithCallbackData(
                                    "❌ Disapprove",
                                    $"RejectPV~{id}~{checkedByTelegramUserName}~{PaymentNo}"
                                )
                            }
                                });


                        // ============================================================
                        // STEP 5: TELEGRAM CAPTION
                        // ============================================================

                        string caption = $@"
<b>🧾 PAYMENT VOUCHER APPROVAL REQUEST</b>

<b>Voucher No:</b> #{No}
<b>Payment No:</b> {PaymentNo}
<b>Submitted By:</b> {submittedBy}
<b>Checker:</b> {checkedByTelegramUserName}

Dear {checkedByFullName},

A new payment voucher has been submitted for your review and approval.

Please review the attached document carefully and select one of the actions below.

Thank you for your prompt attention.
";


                        // ============================================================
                        // STEP 6: DELETE OLD TELEGRAM MESSAGE
                        // ============================================================

                        if (oldMessageId != 0)
                        {
                            try
                            {
                                long targetDeleteChatId = oldChatId != 0 ? oldChatId : tgConfig.ChatId;
                                await activeBotClient.DeleteMessage(
                                    chatId: targetDeleteChatId,
                                    messageId: oldMessageId);
                            }
                            catch (
                                Telegram.Bot.Exceptions.ApiRequestException ex)
                            {
                                // Message may already have been deleted.
                                Console.WriteLine(
                                    "Delete warning: " +
                                    ex.Message);
                            }
                            catch (Exception ex)
                            {
                                // Do not stop sending the new voucher
                                Console.WriteLine(
                                    "Delete old message warning: " +
                                    ex.Message);
                            }
                        }


                        // ============================================================
                        // STEP 7: SEND NEW PDF TO TELEGRAM TOPIC
                        // ============================================================

                        Message sentMessage;

                        string fileName =
                            string.IsNullOrWhiteSpace(PaymentNo)
                                ? $"PaymentVoucher_{id}.pdf"
                                : $"PaymentVoucher_{PaymentNo}.pdf";

                        using (var stream =
                            new MemoryStream(pdfBytes))
                        {
                            sentMessage =
                                await activeBotClient.SendDocument(
                                    chatId: tgConfig.ChatId,

                                    document:
                                        new InputFileStream(
                                            stream,
                                            fileName),

                                    caption: caption,

                                    parseMode:
                                        ParseMode.Html,

                                    replyMarkup:
                                        inlineKeyboard,

                                    messageThreadId:
                                        tgConfig.IsTopic && tgConfig.TopicId > 0
                                            ? tgConfig.TopicId
                                            : (int?)null);
                        }


                        // ============================================================
                        // STEP 8: SAVE TELEGRAM MESSAGE ID
                        // ============================================================

                        if (sentMessage == null)
                        {
                            Console.WriteLine(
                                "Telegram message was not sent.");

                            return;
                        }


                        int messageId =
                            sentMessage.MessageId;


                        // Escape PaymentNo for SQL
                        string sqlPaymentNo =
                            (PaymentNo ?? string.Empty)
                                .Replace("'", "''");


                        string updateSql = $@"

UPDATE dbo.Expenses
SET
    TelegramMessageID = {messageId},
    TelegramChatID = {tgConfig.ChatId},
    TelegramSentDate = GETDATE()
WHERE
    ExpenseID = {id};


UPDATE dbo.Expense_PaymentTransactions
SET
    TelegramMessageID = {messageId},
    TelegramChatID = {tgConfig.ChatId},
    TelegramSentDate = GETDATE()
WHERE
    ExpenseID = {id}
    AND (
        '{sqlPaymentNo}' = ''
        OR PaymentNo = '{sqlPaymentNo}'
    );

";


                        SqlResult sqlResult =
                            html.SaveChangeData(
                                updateSql,
                                false,
                                false);


                        // ============================================================
                        // STEP 9: SUCCESS
                        // ============================================================

                        Console.WriteLine(
                            $"Payment Voucher sent successfully in background. " +
                            $"ExpenseID={id}, " +
                            $"PaymentNo={PaymentNo}, " +
                            $"TelegramMessageID={messageId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            "Background Telegram Send ERROR: " +
                            ex.Message);
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                // ============================================================
                // ERROR
                // ============================================================

                Console.WriteLine(
                    "SendVoucherInternal ERROR: " +
                    ex.Message);

                return false;
            }
        }
        //        private async Task<bool> SendVoucherInternal(int id, string PaymentNo)
        //        {
        //            try
        //            {
        //                var pdfBytes = GenerateVoucherPdfBytes(id, PaymentNo);
        //                if (pdfBytes == null)
        //                    return false;

        //                HTML html = new HTML();

        //                string sql = $@"
        //SELECT 
        //    e.CheckedByTelegramUserName, 
        //    u.FullName, 
        //    e.[No], 
        //    ISNULL(e.TelegramMessageID, 0) AS TelegramMessageID,
        //    e.PaymentNo
        //FROM dbo.Expense_PaymentTransactions e
        //INNER JOIN dbo.TblUserLogin u 
        //    ON u.UserLoginID = e.CheckedByUserID
        //WHERE 
        //    e.ExpenseID = {id}
        //    AND ('{PaymentNo}' IS NULL OR '{PaymentNo}' = '' OR e.PaymentNo = '{PaymentNo}')
        //    AND e.SystemID = {html.SystemID}
        //    AND e.WarehouseID = {html.WarehouseID}";

        //                DataSet ds = html.LoadData(sql);

        //                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //                    return false;

        //                var row = ds.Tables[0].Rows[0];

        //                string No = row["No"].ToString();
        //                //string PaymentNo = row["PaymentNo"].ToString();
        //                int oldMessageId = Convert.ToInt32(row["TelegramMessageID"]);
        //                string checkedByTelegramUserName = row["CheckedByTelegramUserName"].ToString();
        //                string checkedByFullName = row["FullName"].ToString();

        //                // ✅ Inline Keyboard
        //                var inlineKeyboard = new InlineKeyboardMarkup(new[]
        //                {
        //                    new []
        //                    {
        //                        InlineKeyboardButton.WithCallbackData("✅ Check", $"ApprovePV~{id}~{checkedByTelegramUserName}~{PaymentNo}"),
        //                        InlineKeyboardButton.WithCallbackData("❌ Disapprove", $"RejectPV~{id}~{checkedByTelegramUserName}~{PaymentNo}")
        //                    }
        //                });

        //                // ✅ Caption
        //                string caption = $@"
        //<b>🧾 PAYMENT VOUCHER APPROVAL REQUEST</b>

        //<b>Voucher No:</b> #{No}
        //<b>Submitted By:</b> {html.UserFullName}
        //{checkedByTelegramUserName}

        //Dear {checkedByFullName},

        //A new payment voucher has been submitted for your review and approval.

        //Please review the attached document carefully and select one of the actions below.

        //Thank you for your prompt attention.
        //";

        //                // ==============================
        //                // 🔴 STEP 1: DELETE OLD MESSAGE
        //                // ==============================
        //                if (oldMessageId != 0)
        //                {
        //                    try
        //                    {
        //                        await botClient.DeleteMessage(chatId, oldMessageId);
        //                    }
        //                    catch (Telegram.Bot.Exceptions.ApiRequestException ex)
        //                    {
        //                        // Ignore if already deleted or not found
        //                        Console.WriteLine("Delete warning: " + ex.Message);
        //                    }
        //                }

        //                // ==============================
        //                // 🟢 STEP 2: SEND NEW MESSAGE
        //                // ==============================
        //                Message sentMessage;

        //                using (var stream = new MemoryStream(pdfBytes))
        //                {
        //                    sentMessage = await botClient.SendDocument(
        //                        chatId: chatId,
        //                        document: new InputFileStream(stream, $"PaymentVoucher_{PaymentNo}.pdf"),
        //                        caption: caption,
        //                        parseMode: ParseMode.Html,
        //                        replyMarkup: inlineKeyboard,
        //                        messageThreadId: TelegramGroupTopicID == 0 ? null : TelegramGroupTopicID
        //                    );
        //                }

        //                // ==============================
        //                // 💾 STEP 3: SAVE NEW MESSAGE ID
        //                // ==============================
        //                if (sentMessage != null)
        //                {
        //                    string updateSql = $@"
        //UPDATE dbo.Expenses
        //SET
        //    TelegramMessageID = {sentMessage.MessageId},
        //    TelegramChatID = {chatId},
        //    TelegramSentDate = GETDATE()
        //WHERE ExpenseID = {id};

        //UPDATE dbo.Expense_PaymentTransactions
        //SET
        //    TelegramMessageID = {sentMessage.MessageId},
        //    TelegramChatID = {chatId},
        //    TelegramSentDate = GETDATE()
        //WHERE
        //    ExpenseID = {id}
        //    AND ('{PaymentNo}' IS NULL OR '{PaymentNo}' = '' OR PaymentNo = '{PaymentNo}');
        //";

        //                    SqlResult sqlResult = html.SaveChangeData(updateSql, false, false);
        //                }

        //                return true;
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("ERROR: " + ex.Message);
        //                return false;
        //            }
        //        }
        //        private async Task<bool> SendVoucherInternal(int id)
        //        {
        //            try
        //            {
        //                // Ensure Supergroup format
        //                long NormalizeTelegramChatId(long chatId)
        //                {
        //                    if (chatId <= -1000000000000)
        //                        return chatId;

        //                    return -1000000000000 - Math.Abs(chatId);
        //                }
        //                //chatId = chatId.ToString().StartsWith("-100") ? chatId : long.Parse("-100" + Math.Abs(chatId));
        //                var pdfBytes = GenerateVoucherPdfBytes(id);
        //                if (pdfBytes == null)
        //                    return false;

        //                HTML html = new HTML();
        //                string sql = $@"select e.CheckedByTelegramUserName, u.FullName, e.[No], isnull(e.TelegramMessageID,0) TelegramMessageID, isnull(e.TelegramReplyMessageID, 0) TelegramReplyMessageID from dbo.Expenses e
        //inner join dbo.TblUserLogin u on u.UserLoginID = e.CheckedByUserID where e.ExpenseID ={id} and e.SystemID = { html.SystemID} and e.WarehouseID = {html.WarehouseID}";
        //                DataSet ds = html.LoadData(sql);

        //                if (ds == null ||
        //                    ds.Tables.Count == 0 ||
        //                    ds.Tables[0].Rows.Count == 0)
        //                    return false;

        //                string No = ds.Tables[0].Rows[0]["No"].ToString();
        //                int TelegramMessageID = int.Parse(ds.Tables[0].Rows[0]["TelegramMessageID"].ToString());
        //                int TelegramReplyMessageID = int.Parse(ds.Tables[0].Rows[0]["TelegramReplyMessageID"].ToString());
        //                string checkedByTelegramUserName = ds.Tables[0].Rows[0]["CheckedByTelegramUserName"].ToString();
        //                string checkedByFullName = ds.Tables[0].Rows[0]["FullName"].ToString();

        //                // ✅ Create Inline Buttons
        //                var inlineKeyboard = new InlineKeyboardMarkup(new[]
        //                {
        //                    new []
        //                    {
        //                        InlineKeyboardButton.WithCallbackData("✅ Check", $"ApprovePV~{id}~{checkedByTelegramUserName}"),
        //                        InlineKeyboardButton.WithCallbackData("❌ Disapprove", $"RejectPV~{id}~{checkedByTelegramUserName}")
        //                    }
        //                });

        //                // ✅ Delete old message if exists
        //                if (TelegramMessageID != 0 && TelegramReplyMessageID == 0)
        //                {
        //                    try
        //                    {
        //                        await botClient.DeleteMessage(chatId, TelegramMessageID);
        //                    }
        //                    catch (Telegram.Bot.Exceptions.ApiRequestException ex)
        //                    {
        //                        //if (!ex.Message.Contains("message to delete not found"))
        //                        //    throw;
        //                    }
        //                }
        //                string caption = $@"
        //<b>🧾 PAYMENT VOUCHER APPROVAL REQUEST</b>

        //<b>Voucher No:</b> #{No}
        //<b>Submitted By:</b> {html.UserFullName}
        //{checkedByTelegramUserName}

        //Dear {checkedByFullName},

        //A new payment voucher has been submitted for your review and approval.

        //Please review the attached document carefully and select one of the actions below.

        //Thank you for your prompt attention.
        //";
        //                //using (var stream = new MemoryStream(pdfBytes))
        //                //{
        //                //    await botClient.SendDocument(
        //                //        chatId: chatId,
        //                //        document: new InputFileStream(stream, $"PaymentVoucher_{id}.pdf"),
        //                //        caption: caption,
        //                //        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
        //                //        replyMarkup: inlineKeyboard
        //                //    );
        //                //}
        //                using (var stream = new MemoryStream(pdfBytes))
        //                {
        //                    var sentMessage = await botClient.SendDocument(
        //                        chatId: chatId,
        //                        document: new InputFileStream(stream, $"PaymentVoucher_{id}.pdf"),
        //                        caption: caption,
        //                        parseMode: ParseMode.Html,
        //                        replyMarkup: inlineKeyboard,
        //                        replyParameters:TelegramGroupTopicID
        //                    );

        //                    if (sentMessage != null)
        //                    {
        //                        string updateSql = $@"UPDATE dbo.Expenses SET TelegramMessageID = {sentMessage.MessageId},
        //                            TelegramChatID = {chatId},
        //                            TelegramSentDate = GETDATE()
        //                            WHERE ExpenseID = {id}";

        //                        html.SaveChangeData(updateSql, false, false);
        //                    }
        //                }

        //                return true;
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine(ex.Message);
        //                return false;
        //            }
        //        }

        [HttpPost]
        public async Task<ActionResult> ApprovalPaymentVoucherAsync(int ExpenseID, string Status, string PaymentNo)
        {
            HTML html = new HTML();
            string sql = $@"select CheckedByUserID, VerifiedByUserID, TelegramMessageID, TelegramChatID, [No], SystemID, WarehouseID, DomainID 
                           from [dbo].[Expense_PaymentTransactions] 
                           where (PaymentNo = '{PaymentNo}' and '{PaymentNo}' <> '') or ExpenseID = {ExpenseID}";
            DataSet ds = html.LoadData(sql);
            if (ds == null ||
                    ds.Tables.Count == 0 ||
                    ds.Tables[0].Rows.Count == 0)
                return Json(new { Result = 2, success = false, message = "Expense not found" });

            string checkedByUserId = ds.Tables[0].Rows[0]["CheckedByUserID"]?.ToString() ?? "";
            string verifiedByUserId = ds.Tables[0].Columns.Contains("VerifiedByUserID") ? (ds.Tables[0].Rows[0]["VerifiedByUserID"]?.ToString() ?? "") : "";

            bool isOverrideUser = html.UserID == "38" || html.UserID == "126";

            bool isAuthorized = isOverrideUser;
            if (!isAuthorized)
            {
                if (Status == "Checked" || Status == "Rejected")
                {
                    isAuthorized = (checkedByUserId == html.UserID || string.IsNullOrEmpty(checkedByUserId) || checkedByUserId == "0");
                }
                else if (Status == "Confirm Verified" || Status == "Confirm Rejected")
                {
                    isAuthorized = (verifiedByUserId == html.UserID || string.IsNullOrEmpty(verifiedByUserId) || verifiedByUserId == "0");
                }
            }

            if (isAuthorized)
            {
                int recordSystemId = Convert.ToInt32(ds.Tables[0].Rows[0]["SystemID"]);
                int recordWarehouseId = ds.Tables[0].Rows[0]["WarehouseID"] != DBNull.Value ? Convert.ToInt32(ds.Tables[0].Rows[0]["WarehouseID"]) : 0;
                var tgConfig = GetStaticTelegramConfig(recordSystemId, recordWarehouseId, "Expense");
                string targetBotToken = !string.IsNullOrEmpty(tgConfig.BotToken) ? tgConfig.BotToken : botToken;

                var service = new ExpenseApprovalService(targetBotToken, _repo);

                var result = await service.ApprovalPaymentVoucherAsync(
                    ExpenseID,
                    Status, html.UserFullName,
                    PaymentNo,
                    this.ControllerContext
                );

                return Json(new
                {
                    Result = 2,
                    success = result.success,
                    message = result.message
                });
                //                int messageId = Convert.ToInt32(ds.Tables[0].Rows[0]["TelegramMessageID"]);
                //                long chatId = Convert.ToInt64(ds.Tables[0].Rows[0]["TelegramChatID"]);
                //                string voucherNo = ds.Tables[0].Rows[0]["No"].ToString();

                //                sql = $@"update [dbo].[Expenses] set [Status] = '{Status}', CheckedDate = GETDATE() where ExpenseID = {ExpenseID}";
                //                html.SaveChangeData(sql, false, false);

                //                var pdfBytes = GenerateVoucherPdfBytes(ExpenseID);
                //                if (pdfBytes != null)
                //                {
                //                    if (Status == "Rejected" || Status == "Checked")
                //                    {
                //                        try
                //                        {
                //                            await botClient.EditMessageReplyMarkup(
                //                                chatId: chatId,
                //                                messageId: messageId,
                //                                replyMarkup: null
                //                            );
                //                        }
                //                        catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                //                        {
                //                            if (!ex.Message.Contains("message is not modified"))
                //                                throw;
                //                        }
                //                    }

                //                    string newCaption = "";
                //                    string dbStatus = Status;

                //                    if (Status == "Rejected")
                //                    {
                //                        newCaption = $@"
                //<b>🧾 PAYMENT VOUCHER REVIEW RESULT</b>

                //<b>Voucher No:</b> #{voucherNo}

                //────────────────────────
                //❌ <b>CURRENT STATUS: REJECTED</b>
                //────────────────────────

                //After careful review, this payment voucher has not been approved at the checking level.

                //<b>Reviewed By:</b> {html.UserFullName}
                //<b>Date:</b> {DateTime.Now:dd-MMM-yyyy HH:mm}

                //Please review the voucher details and make the necessary corrections before resubmission.

                //This is an automated system notification.
                //";
                //                    }
                //                    else if (Status == "Checked")
                //                    {
                //                        newCaption = $@"
                //<b>🧾 PAYMENT VOUCHER APPROVAL REQUEST</b>

                //<b>Voucher No:</b> #{voucherNo}

                //────────────────────────
                //✅ <b>CURRENT STATUS: CHECKED</b>
                //────────────────────────

                //This payment voucher has been reviewed and formally checked.

                //<b>Checked By:</b> {html.UserFullName}
                //<b>Date:</b> {DateTime.Now:dd-MMM-yyyy HH:mm}

                //The document is now forwarded to the next stage of approval.

                //This is an automated system notification.
                //";
                //                    }
                //                    using (var stream = new MemoryStream(pdfBytes))
                //                    {
                //                        var sentMessage = await botClient.SendDocument(
                //                           chatId: chatId,
                //                           document: new InputFileStream(stream, $"PaymentVoucher_{ExpenseID}.pdf"),
                //                           caption: newCaption,
                //                           parseMode: ParseMode.Html,
                //                           replyParameters: messageId // reply under original voucher
                //                       );
                //                        if (sentMessage != null)
                //                        {
                //                            // Save new reply message ID (optional but recommended)
                //                            string updateSql = $@"
                //            UPDATE dbo.Expenses
                //            SET TelegramReplyMessageID = {sentMessage.MessageId}
                //            WHERE ExpenseID = {ExpenseID}";

                //                            html.SaveChangeData(updateSql, false, false);
                //                        }
                //                    }
                //                }


                //await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, "Approved successfully");

                //return Json(new { Result=2, success = true, message = "Your approval has been recorded successfully" });
            }
            else
            {
                return Json(new { Result = 2, success = false, message = "You are not authorized to perform this action" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostingPaymentVoucherAsync(int ExpenseID, string Status)
        {
            HTML html = new HTML();

            if (Status != "Checked")
            {
                return Json(new
                {
                    Result = 0,
                    success = false,
                    message = "Only checked voucher can be posted"
                });
            }
            string checkSql = $@"SELECT SystemID, WarehouseID FROM Expenses WHERE ExpenseID = {ExpenseID}";
            DataSet ds = html.LoadData(checkSql);
            int expSysId = (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Rows[0]["SystemID"] != DBNull.Value)
                ? Convert.ToInt32(ds.Tables[0].Rows[0]["SystemID"])
                : int.Parse(html.SystemID);
            int expWhId = (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Rows[0]["WarehouseID"] != DBNull.Value)
                ? Convert.ToInt32(ds.Tables[0].Rows[0]["WarehouseID"])
                : 0;

            var tgConfig = GetStaticTelegramConfig(expSysId, expWhId, "Expense");
            string targetBotToken = !string.IsNullOrEmpty(tgConfig.BotToken) ? tgConfig.BotToken : botToken;

            var service = new ExpenseApprovalService(targetBotToken, _repo);
            var result = service.PostExpense(
                expenseId: ExpenseID
            );

            //return Json(new
            //{
            //    Result = result.success ? 1 : 0,
            //    success = result.success,
            //    message = result.message
            //});
            return Json(new
            {
                Result = 2,
                success = true,
                ExpenseID,
                message = result.message
            });
        }

        // POST: /Expense/Create
        [HttpPost]
        public async Task<ActionResult> CreateAsync()
        {
            try
            {
                HTML html = new HTML();
                string json = Request.Form["DATA"];
                SqlResult sqlResult = new SqlResult();
                string FileName = "";
                if (Request.Files.Count > 0)
                {
                    sqlResult = html.UploadFile("Uploads/Expenses", "", false, true);
                    FileName = sqlResult.Value;
                }
                if (string.IsNullOrWhiteSpace(json))
                    return Json(new { success = false, message = "DATA is empty" });

                var wrapper = JsonConvert.DeserializeObject<ExpensePostWrapper>(
                    json,
                    new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Include
                    });

                if (wrapper?.ExpenseData == null || !wrapper.ExpenseData.Any())
                    return Json(new { success = false, message = "Expense header not found" });
                if (wrapper.ExpenseData[0].PaymentNo == "")
                {
                    if (wrapper.Details == null || !wrapper.Details.Any())
                        return Json(new { success = false, message = "Expense details are required" });
                }
                Expense expense = MapExpenseSafe(
                    wrapper.ExpenseData.First(),
                    wrapper.Details, FileName
                );

                //int expenseId = _repo.InsertExpense(expense);
                int expenseId = 0;
                ExpensePostingResult expensePostingResult = null;
                //if (expense.ExpenseID != 0 && wrapper.ExpenseData[0].PaymentNo == "")
                //{
                //    expensePostingResult = _repo.UpdateExpense(expense);
                //}
                //else
                //{
                expense.SystemID = int.Parse(html.SystemID);
                expense.WarehouseID = int.Parse(html.WarehouseID); 
                expense.DomainID = html.DomainID;
                expense.CreateByUserID = html.UserID;
                expense.CreateDate = html.GetCurrentDate();

                if (expense.ExpenseID == 0)
                {
                    int userId = 0;
                    if (!int.TryParse(expense.CreateByUserID, out userId))
                    {
                        int.TryParse(html.UserID, out userId);
                    }

                    // Guard against rapid duplicate submissions (e.g. fast double-clicks or concurrent network requests)
                    string dupCheckSql = @"
                        SELECT TOP 1 e.ExpenseID, e.ExpenseNo, e.PaymentNo 
                        FROM dbo.Expense_PaymentTransactions e
                        WHERE e.SystemID = " + expense.SystemID + @"
                          AND e.WarehouseID = " + expense.WarehouseID + @"
                          " + (userId > 0 ? "AND e.CreateByUserID = " + userId : "") + @"
                          AND e.PayeeID = " + expense.PayeeID + @"
                          AND e.TotalAmount = " + expense.TotalAmount + @"
                          AND e.CreatedDate >= DATEADD(SECOND, -30, GETDATE())
                        ORDER BY e.ExpenseID DESC";
                    DataSet dsDup = html.LoadData(dupCheckSql, false, null);
                    if (dsDup != null && dsDup.Tables.Count > 0 && dsDup.Tables[0].Rows.Count > 0)
                    {
                        int existingExpenseId = Convert.ToInt32(dsDup.Tables[0].Rows[0]["ExpenseID"]);
                        return Json(new
                        {
                            Result = 2,
                            success = true,
                            expenseId = existingExpenseId,
                            message = "Expense saved successfully"
                        });
                    }
                }

                expensePostingResult = _repo.InsertExpense(expense);
                //}
                expenseId = expensePostingResult.ExpenseID;
                // 🔥 AUTO SEND TO TELEGRAM
                //_ = Task.Run(async () =>
                //{
                //    try
                //    {
                await SendVoucherInternal(expenseId, expensePostingResult.PaymentNo);
                //    }
                //    {
                //        // optionally log error here
                //    catch
                //    }
                //});
                return Json(new
                {
                    Result = 2,
                    success = true,
                    expenseId,
                    message = "Expense saved successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    // message = "System error while saving expense",
                    message = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
                    debug = ex.ToString()
                });
            }
        }

        private Expense MapExpenseSafe(
            ExpenseViewModel m,
            List<ExpenseDetailViewModel> details, string FileName)
        {
            int ToInt(string v) => int.TryParse(v, out var x) ? x : 0;

            HTML html = new HTML();
            return new Expense
            {
                ExpenseID = m.ExpenseID,
                ClassName = m.ClassName,
                PaymentMethodID = m.PaymentMethodID.HasValue ? m.PaymentMethodID.Value : 0,
                PayeeID = m.PayeeID,
                Description = m.Description,
                VATRemark = m.VATRemark,
                No = m.No,
                PostedPaymentNo = m.PostedPaymentNo,
                ExpenseNo = m.ExpenseNo,
                NoPrefix = m.NoPrefix,

                PaymentAccountID = ToInt(m.PaymentAccountID),
                PaymentDate = m.PaymentDate,
                TransactionDate = m.PaymentDate,
                PaidOnDate = m.PaidOnDate,

                PVNo = m.PVNo,
                PVNoPrefix = m.PVNoPrefix,

                CheckedBy = m.CheckedBy,
                CheckedByTelegramUserName = m.CheckedByTelegramUserName,
                CheckedByUserID = ToInt(m.CheckedByUserID),

                VerifiedBy = m.VerifiedBy,
                VerifiedByTelegramUserName = m.VerifiedByTelegramUserName,
                VerifiedByUserID = ToInt(m.VerifiedByUserID),

                ApprovedBy = m.ApprovedBy,
                ApprovedByTelegramUserName = m.ApprovedByTelegramUserName,
                ApprovedByUserID = ToInt(m.ApprovedByUserID),

                PostingNumber = m.PostingNumber,
                PostingPrefix = m.PostingPrefix,

                Reference = m.Reference ?? "",
                BankInfo = m.BankInfo ?? "",
                BankerName = m.BankerName ?? "",
                ChequeBankerName = m.ChequeBankerName ?? "",
                BankAccountName = m.BankAccountName ?? "",
                BankAccountNo = m.BankAccountNo ?? "",
                ChequeTo = m.ChequeTo ?? "",
                ChequeNo = m.ChequeNo ?? "",
                Total = m.Total,
                Status = m.Status,
                CustomerID = m.CustomerID,
                EmployeeID = m.EmployeeID,
                VendorID = m.VendorID,
                TransactionType = m.TransactionType,
                PayeeType = m.PayeeType,
                Gender = m.Gender,
                PhoneNumber = m.PhoneNumber,
                SystemID = m.SystemID,
                WarehouseID = m.WarehouseID,
                DomainID = m.DomainID,
                PostedByUserID = m.PostedByUserID,
                PostedBy = m.PostedBy,
                PostedByTelegramUserName = m.PostedByTelegramUserName,
                ExchangeRate = m.ExchangeRate,
                FileName = FileName,
                PaymentTermsID = m.PaymentTermsID,
                TermName = m.TermName,
                TotalAmount = m.TotalAmount,
                // GrantTotalAsRiel = m.GrantTotalAsRiel,
                // New logic: 
                GrantTotalAsRiel = m.GrantTotalAsRiel > 0
                    ? m.GrantTotalAsRiel
                    : Math.Round((m.TotalAmount * m.ExchangeRate) / 100m, MidpointRounding.AwayFromZero) * 100m,
                Discount = m.Discount,
                PaidAmount = m.PaidAmount,
                PaymentAmount = m.PaidAmount,
                PaymentNo = m.PaymentNo,
                BalanceAmount = m.TotalAmount - m.PaidAmount,
                ExpensePaymentID = m.ExpensePaymentID.HasValue ? m.ExpensePaymentID.Value : 0,
                VendorName = m.VendorName,
                ItemVAT = m.ItemVAT.HasValue ? m.ItemVAT.Value : 0,
                CreateByUserID = html.UserID,
                ClassNameID = m.ClassName,
                ExpenseTransactionType = m.ExpenseTransactionType,
                HeaderTax = m.HeaderTax,
                Type = m.Type,
                IsFromBills = m.IsFromBills,
                VendorDepositID = m.VendorDepositID.HasValue ? m.VendorDepositID.Value : 0,
                AmountToApply = m.AmountToApply.HasValue ? m.AmountToApply.Value : 0,
                Details = details.Select(d => new ExpenseDetail
                {
                    ExpenseTransactionType = m.ExpenseTransactionType,
                    ExpenseDetailID = d.ExpenseDetailID,
                    ItemTypeID = d.ItemTypeID,
                    ExpenseCategory = d.ExpenseCategory,
                    AccountName = d.AccountName,
                    AccountID = d.AccountID,
                    ItemID = d.ItemID.HasValue ? d.ItemID.Value : 0,
                    ItemName = html.HtmlDecode(d.ItemName),
                    ItemName_Description = html.HtmlDecode(d.ItemName_Description),
                    Description = d.Description,
                    UM = d.UM,
                    QTY = d.QTY,
                    UnitPrice = d.UnitPrice,
                    DisUSD = d.DisUSD,
                    Total = d.Total,
                    ItemTAX = d.ItemTAX.HasValue ? d.ItemTAX.Value : 0,
                    ItemGrandTotal = d.ItemGrandTotal,
                    RefNo = d.RefNo,
                    SourceType = d.SourceType,
                    ItemVAT = d.ItemVAT.HasValue ? d.ItemVAT.Value : 0,
                    ExpenseCategoryType = d.ExpenseCategoryType
                }).ToList()
            };
        }

        public string Vendors()
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over (order by v.VendorID) as [No],
v.VendorID, v.VendorName, v.CompanyName, v.Phone, v.ContactPersonName, v.ContactPhoneNumber,
po.[Type], sum(po.GrandTotal) Total
from dbo.TblVendor v
inner join dbo.TblPurchaseOrder po on po.VendorID = v.VendorID
where v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " and v.DomainID like '%" + html.DomainID + @"%'
group by v.VendorID, v.VendorName, v.CompanyName, v.Phone, v.ContactPersonName, v.ContactPhoneNumber, po.[Type]";
            html.IsHaveWhereCondition = true;
            html.GH.SQL = sql;
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Vendor Name", FieldName = "VendorName", Link ="#!/Profile?ID=", LinkParameterField="VendorID" },
                new GridField() { Header = "Company Name", FieldName = "CompanyName" },
                new GridField() { Header = "Phone", FieldName = "Phone" },
                new GridField() { Header = "Contact Name", FieldName = "ContactPersonName" },
                new GridField() { Header = "Type", FieldName = "Type" },
                new GridField() { Header = "Total", FieldName = "Total", TextAlign = "right", Format="#,#0.00" },
            };
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }


        [Authorize(Roles = "Add Expenses, Edit Expenses, Delete Expenses")]
        public ActionResult LoadExpenses()
        {
            HTML html = new HTML();
            //html.ReportTitle = "List Purchase Order";
            if (html.HasPermission("Add Expenses"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Expenses"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Expenses"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New";
            html.AddURL = "ExpenseTransactions/AddExpenses";
            
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date Range:",
                LabelAlign = "left",
                DBField = "e.PaymentDate",
                DateTimePickerWithRanges = true
            });

            // html.HtmlAddControls.Add(new HtmlAddControls()
            // {
            //     ID = "txtExpenseNo",
            //     ISAutoComplete = true,
            //     Label = "Expense No:",
            //     LabelAlign = "left",
            //     DBField = "exp.ExpenseNo",
            //     SQLAutoComplete = "SELECT DISTINCT TOP 10 ExpenseNo FROM dbo.Expenses WHERE ExpenseNo IS NOT NULL AND LOWER(ExpenseNo) LIKE @SearchText AND SystemID = " + html.SystemID + " AND WarehouseID = " + html.WarehouseID + " ORDER BY ExpenseNo DESC"
            // });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtVendorName",
                ISAutoComplete = true,
                Label = "Vendor / Payee:",
                LabelAlign = "left",
                DBField = "py.VendorName",
                SQLAutoComplete = "SELECT DISTINCT TOP 10 py.VendorName FROM dbo.TblVendor py WHERE LOWER(py.VendorName) LIKE @SearchText AND py.SystemID = " + html.SystemID + " AND py.WarehouseID = " + html.WarehouseID + " ORDER BY py.VendorName",
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                Label = "Status:",
                LabelAlign = "left",
                DBField = "e.Status",
                SQL_CBO = "SELECT DISTINCT Status, Status AS StatusText FROM dbo.Expense_PaymentTransactions WHERE Status IS NOT NULL AND SystemID = " + html.SystemID + " AND WarehouseID = " + html.WarehouseID,
                DisplayText = "StatusText",
                Value = "Status",
                ComboBox = true
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtPaymentStatus",
                Label = "Payment Status:",
                LabelAlign = "left",
                DBField = "e.PaymentStatus",
                SQL_CBO = "SELECT DISTINCT PaymentStatus, PaymentStatus AS PaymentStatusText FROM dbo.Expense_PaymentTransactions WHERE PaymentStatus IS NOT NULL AND SystemID = " + html.SystemID + " AND WarehouseID = " + html.WarehouseID,
                DisplayText = "PaymentStatusText",
                Value = "PaymentStatus",
                ComboBox = true,
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Expense No",
                SQL = "SELECT DISTINCT TOP 10 exp.ExpenseNo FROM dbo.Expenses exp WHERE exp.ExpenseNo IS NOT NULL AND LOWER(exp.ExpenseNo) LIKE @SearchText AND exp.SystemID = " + html.SystemID + " AND exp.WarehouseID = " + html.WarehouseID + " ORDER BY exp.ExpenseNo DESC",
                RadioValue = EncryAndDecry.Encryp("exp.ExpenseNo"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });


            html.TableName = "Expenses";
            html.PrimaryField = "ExpenseID";
            //html.OrderBy = "ExpensesID desc";
            //html.ShowSearchButton = false;
            html.ShowPaging = false;
            html.GH.SQL = @"SELECT
        row_number() over(order by e.PaymentDate desc, ISNULL(exp.ExpenseNo, e.ExpenseNo) desc, e.ExpenseID desc) as [RowNo],
        e.ExpenseID,
        '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher('+convert(nvarchar, e.ExpenseID) +','''+e.[No]+''','''+e.[Status]+''','''+e.[PaymentNo]+''');"">' + e.[No] +'</a>' [No],
        e.PaymentTeam,
        e.PaidOnDate,
        '<a href=""#!/vendorcenter?id='+convert(varchar, e.PayeeID)+'"" target=""_blank"">' + ISNULL(py.VendorName, '') + '</a>' PayeeName,
        CASE
            WHEN COALESCE(NULLIF(e.Description, ''), NULLIF(exp.Description, '')) IS NOT NULL 
                 AND NULLIF(items.ItemNames, '') IS NOT NULL
                 AND COALESCE(NULLIF(e.Description, ''), exp.Description) != items.ItemNames
            THEN COALESCE(NULLIF(e.Description, ''), exp.Description) + ' - ' + items.ItemNames
            WHEN COALESCE(NULLIF(e.Description, ''), NULLIF(exp.Description, '')) IS NOT NULL
            THEN COALESCE(NULLIF(e.Description, ''), exp.Description)
            ELSE items.ItemNames
        END AS Description,
        e.[Status],
        e.TotalAmount,
        e.PaidAmount,
        e.BalanceAmount,
        e.PaymentStatus,
        pm.PaymentMethod AS PaymentMethod,
        e.PaymentNo,
        '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher('+convert(nvarchar, e.ExpenseID) +','''+e.[No]+''','''+e.[Status]+''','''+e.[PaymentNo]+''');"">' + ISNULL(exp.ExpenseNo, e.ExpenseNo) +'</a>' ExpenseNo,
        e.PaymentDate
    FROM Expense_PaymentTransactions e
    LEFT JOIN dbo.Expenses exp ON exp.ExpenseID = e.ExpenseID
    INNER JOIN dbo.TblVendor py ON py.VendorID = e.PayeeID
    LEFT JOIN dbo.TblPaymentMethod pm
    ON pm.PaymentMethodID = e.PaymentMethodID
    OUTER APPLY (
        SELECT STUFF((
            SELECT ', ' + CASE 
                WHEN NULLIF(ed.ItemName, '') IS NOT NULL AND NULLIF(ed.ItemName_Description, '') IS NOT NULL 
                     AND ed.ItemName != ed.ItemName_Description
                    THEN ed.ItemName + ' - ' + ed.ItemName_Description
                ELSE COALESCE(NULLIF(ed.ItemName_Description, ''), ed.ItemName, '')
            END
            FROM dbo.ExpenseDetails ed WITH (NOLOCK)
            WHERE ed.ExpenseID = ISNULL(e.ExpenseID, exp.ExpenseID) 
              AND COALESCE(NULLIF(ed.ItemName_Description, ''), ed.ItemName, '') <> ''
            FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ItemNames
    ) items
    WHERE e.Type='Expense' and e.SystemID = " + html.SystemID + " and e.WarehouseID = " + html.WarehouseID;

            html.OrderBy = "e.PaymentDate DESC, ISNULL(exp.ExpenseNo, e.ExpenseNo) DESC, e.ExpenseID DESC";
            html.IsHaveWhereCondition = true;
            //html.IsUseHavingCondition = true;
            html.GH.GridTable = new GridField[]
{
    new GridField()
    {
        Header = "#",
        FieldName = "RowNo",
        TextAlign = "right"
    },

    new GridField()
    {
        Header = "Expense#",
        FieldName = "ExpenseNo",
        TextAlign = "center"
    },
    new GridField()
    {
        Header = "PV.Date",
        FieldName = "PaymentDate",
        TextAlign = "center",
        FormatDateTime = "dd-MM-yyyy"
    },
    new GridField()
    {
        Header = "Paid On Date",
        FieldName = "PaidOnDate",
        TextAlign = "center",
        FormatDateTime = "dd-MM-yyyy"
    },

    new GridField()
    {
        Header = "Payee Name",
        FieldName = "PayeeName"
    },

    new GridField()
    {
        Header = "Description",
        FieldName = "Description",
        Width = "500px",
        ClassName = "text-truncate"
    },

    new GridField()
    {
        Header = "Total",
        FieldName = "TotalAmount",
        TextAlign = "right",
        Format = "#,#0.00"
    },
    new GridField()
    {
        Header = "Paid",
        FieldName = "PaidAmount",
        TextAlign = "right",
        Format = "#,#0.00",
        IsHiddenField= true
    },
    new GridField()
    {
        Header = "Balance",
        FieldName = "BalanceAmount",
        TextAlign = "right",
        Format = "#,#0.00",
        IsHiddenField= true
    },
    new GridField()
    {
        Header = "Payment Status",
        FieldName = "PaymentStatus",
        TextAlign = "center",
        IsHiddenField= true
    },
    new GridField()
    {
        Header = "Payment Method",
        FieldName = "PaymentMethod",
        TextAlign = "center"
        
    },

    new GridField()
    {
        Header = "Status",
        FieldName = "Status",
        TextAlign = "center"
    },
    new GridField()
    {
        Header = "Action",
        FieldName = "ExpenseID",
        TextAlign = "Center",
        IsModifyField = true,
        DeleteWithTableName = "Expenses, ExpenseDetails, Expense_PaymentTransactions, TblAccount_Transaction, TblAccount_JournalEntries",
        HideEditOnActionNameNotMatch = true,
        ShowActionNameOnConditionField = "Status",
        ShowActionNameOnConditionValue = "Draft,Rejected,Confirm Rejected"
    }
};

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Expenses, Edit Expenses, Add Bills, Edit Bills")]
        public ActionResult AddExpenses(string PrimaryFieldValue, string Status = "Draft", string No = "", string PaymentNo = "", string ExpenseNo = "", string PostedPaymentNo = "", string ExpensePaymentID = "", bool IsFromBills = false)
        {
            if (PostedPaymentNo == "" && PaymentNo != "")
                PostedPaymentNo = PaymentNo;
            ViewBag.ItemDetails = "";
            ViewBag.GrantTotalAsRiel = "";
            ViewBag.dtSummary = "";
            ViewBag.dtPaymentHistory = "";
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.PaymentNo = PaymentNo;
            ViewBag.ExpenseNo = ExpenseNo;
            ViewBag.No = No;
            ViewBag.Status = Status;
            ViewBag.ExpensePaymentID = ExpensePaymentID;
            ViewBag.VendorDepositID = 0;
            ViewBag.AppliedAmount = 0;
            HTML html = new HTML
            {
                PrimaryFieldValue = PrimaryFieldValue,
                PrimaryField = "ExpenseID",
                FormID = "FrmAddExpense",
            };
            string sqlPayee = @"SELECT 
    a.PayeeID, 
    a.Code, 
    a.[Name], 
    a.[Type], 
    a.OrderBy, 
    ISNULL(a.Gender, '') AS Gender,
    ISNULL(a.PhoneNumber, '') PhoneNumber
FROM (
    
    -- ========================
    -- Vendor
    -- ========================
    SELECT 
        v.VendorID AS PayeeID, 
        CAST(v.VendorCode AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code, 
        CAST(v.VendorName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name], 
        CAST('Vendor' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type], 
        1 AS OrderBy, 
        CAST(v.Gender AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
        CAST(v.ContactPhoneNumber AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
    FROM dbo.TblVendor v
    WHERE v.SystemID = " + html.SystemID + " AND v.WarehouseID = " + html.WarehouseID + @"
    UNION ALL

    -- ========================
    -- Customer
    -- ========================
    SELECT 
        c.CustomerID AS PayeeID, 
        CAST(c.CustomerCode AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code, 
        CAST(c.CustomerName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name], 
        CAST('Customer' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type], 
        3 AS OrderBy, 
        CAST(c.CustomerGender AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
        CAST(c.CustomerContactMobileNumber AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
    FROM dbo.TblCustomer c
    WHERE ISNULL(c.CustomerCode, '') <> '' AND c.SystemID = " + html.SystemID + " AND c.WarehouseID = " + html.WarehouseID + @"
    UNION ALL

    -- ========================
    -- Employee (Different DB)
    -- ========================
    SELECT 
        e.EmployeeID AS PayeeID, 
        CAST('' AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code, 
        CAST(e.FullName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name], 
        CAST('Employee' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type], 
        2 AS OrderBy, 
        CAST(g.GenderName AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
        CAST(p.PhoneNumber1 AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
    FROM HRMS_DB.dbo.Employees e
    INNER JOIN HRMS_DB.dbo.Gender g 
        ON g.GenderID = e.GenderID
    INNER JOIN HRMS_DB.dbo.Staffing_EmployeePhones p 
        ON p.EmployeeID = e.EmployeeID

) AS a
ORDER BY 
    a.OrderBy, 
    a.[Name] ASC;";
            string ExpenseAccount = @"SELECT
    coa.ChartOfAccountID,
    cat.AccountCategory,
    ct.AccountTypes,
    coa.AccountNo,
    coa.AccountName,
    coa.ParentAccountID
FROM TblChartOfAccount coa
INNER JOIN dbo.TblAccountTypes ct 
    ON ct.AccountTypesID = coa.AccountTypesID
INNER JOIN dbo.TblAccountCategories cat 
    ON cat.AccountCategoryID = coa.AccountCategoryID
WHERE 
    coa.SystemID = " + html.SystemID + @"
    AND coa.WarehouseID = " + html.WarehouseID + @"

    -- Allow Payment Accounts (Asset) OR A/P (Liability)
    --AND (
      --  (
            --ct.AccountTypes = 'Asset' AND coa.AccountName NOT LIKE '%Discount%'
        --    AND coa.AccountName NOT LIKE '%Advance%'
          --  AND coa.AccountName NOT LIKE '%Receivable%'
            --AND coa.AccountName NOT LIKE '%Prepaid%'
            --AND coa.AccountName NOT LIKE '%Cash out%'

            --AND (
              --  coa.ParentAccountID IN (
                --    SELECT ChartOfAccountID
                  --  FROM TblChartOfAccount
                    --WHERE AccountName IN (
                      --  'Petty Cash',
                        --'Cash in Bank',
                        --'Cash on Hand'
                    --)
                --)
                --OR coa.AccountName IN (
                  --  'Petty Cash',
                    --'Cash in Bank',
                    --'Cash on Hand'
                --)
            --)
        --)

        -- ✅ Accounts Payable
        --OR (
          --  ct.AccountTypes = 'Liability'
            --AND coa.AccountName LIKE '%Payable%'
        --)
    --)
ORDER BY
    coa.AccountNo;

";

            string PaidAmount = "0", HeaderTax = "0", VATRemark = "";
            if (PrimaryFieldValue != "0")
            {
                // string sql = @"SELECT isnull(PaidAmount, 0) AS PaidAmount, HeaderTax, VATRemark, Type FROM dbo.Expenses WHERE ExpenseID=" + PrimaryFieldValue;
                // PaidAmount = html.LoadData(sql).Tables[0].Rows[0]["PaidAmount"].ToString();
                // HeaderTax = html.LoadData(sql).Tables[0].Rows[0]["HeaderTax"].ToString();
                // IsFromBills = html.LoadData(sql).Tables[0].Rows[0]["Type"].ToString() == "Bill" ? true : false;
                // VATRemark = HttpUtility.JavaScriptStringEncode(Convert.ToString(html.LoadData(sql).Tables[0].Rows[0]["VATRemark"].ToString()));
                string sql = @"SELECT isnull(PaidAmount, 0) AS PaidAmount, HeaderTax, VATRemark, Type, isnull(ExpenseNo, '') AS ExpenseNo, VendorDepositID, AppliedAmount, GrantTotalAsRiel FROM dbo.Expenses WHERE ExpenseID=" + PrimaryFieldValue;
                var dtExp = html.LoadData(sql).Tables[0];
                PaidAmount = dtExp.Rows[0]["PaidAmount"].ToString();
                HeaderTax = dtExp.Rows[0]["HeaderTax"].ToString();       
                ViewBag.VendorDepositID = dtExp.Rows[0]["VendorDepositID"].ToString();
                ViewBag.AppliedAmount = dtExp.Rows[0]["AppliedAmount"].ToString();
                ViewBag.GrantTotalAsRiel = dtExp.Rows[0]["GrantTotalAsRiel"].ToString();
                IsFromBills = dtExp.Rows[0]["Type"].ToString() == "Bill" ? true : false;
                VATRemark = HttpUtility.JavaScriptStringEncode(Convert.ToString(dtExp.Rows[0]["VATRemark"].ToString()));
                if (string.IsNullOrEmpty(ExpenseNo) && dtExp.Rows[0]["ExpenseNo"] != DBNull.Value)
                {
                    ExpenseNo = dtExp.Rows[0]["ExpenseNo"].ToString();
                }
            }
            ViewBag.ExpenseNo = ExpenseNo;
            ViewBag.PaidAmount = PaidAmount;
            ViewBag.HeaderTax = HeaderTax;
            ViewBag.VATRemark = VATRemark;
            ViewBag.IsFromBills = IsFromBills;
            html.IsCustomForm = true;
            html.TableColumn = 3;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    SQLEdit= PostedPaymentNo!=""? $"select * from Expense_PaymentTransactions where PaymentNo =N'{PostedPaymentNo}'":"",
                    TableName="Expense_PaymentTransactions",
                    FormTitle="Expense",
                    GetIDENTITYByFiled="ExpenseID",
                    PrimaryFieldName="ExpenseID",
                    ISAddAttachmentFile = true,
                    DestinationFile = "Uploads/Expenses",
                    //SQLEdit="select * from dbo.TblAccount_Expenses where ExpensesID="+ PrimaryFieldValue,
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label="Transaction Type:", ID="ExpenseTransactionType",
                            Required=false,
                            Class="input2",
                            IsCustomForm = true,
                            DBField="ExpenseTransactionType",
                            SQL_CBO= "select 'Expense' TransactionType union select 'Vendor Deposit' TransactionType",
                            Value = "TransactionType",
                            DisplayText = "TransactionType",
                            IsKendoMultiColumnComboBox = true,
                            CallBackFunction="OnChangeExpenseTransactionType(this);",
                            ReadOnly = No != "",
                            CboValue = "Expense",
                            Hide = true
                        },
                        new HtmlComboBox()
                        {
                            Label="Class Name:", ID="ClassName",
                            Required=true,
                            Class="input2",
                            IsCustomForm = true,
                            DBField="ClassName",
                            SQL_CBO= "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID ="+html.WarehouseID + " and DomainID like '%"+html.DomainID+"%'",
                            Value = "ClassID",
                            DisplayText = "ClassName",
                            IsKendoMultiColumnComboBox = true,
                            CallBackFunction="PaymentMothod(this);",
                            ReadOnly = No != ""
                        },
                        new HtmlComboBox()
                        {
                            Label="Payment Method:", ID="PaymentMethodID",
                            Class="input2", IsCustomForm = true,
                            DBField="PaymentMethodID",
                            SQL_CBO="select PaymentMethodID, PaymentMethod from dbo.TblPaymentMethod where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and PaymentMethod in ('Bank Transfer', 'Cheque', 'Petty Cash') order by PaymentMethod",
                            Value="PaymentMethodID",
                            DisplayText="PaymentMethod",
                            IsKendoMultiColumnComboBox = true,
                            CallBackFunction ="PaymentMothod(this);",
                            ReadOnly = No != "",
                            Required = IsFromBills ? false : true,
                            Hide = IsFromBills
                        },

                        new HtmlComboBox()
                        {
                            Label="Payee", ID="PayeeID",
                            Required=true,
                            Class="input2",
                            IsCustomForm = true,
                            DBField="PayeeID",
                            SQL_CBO= sqlPayee,
                            Value = "PayeeID",
                            DisplayText = "Name",
                            //GroupByField = "Type",
                            IsKendoMultiColumnComboBox = true,
                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                            {
                                new KendoComboBoxColumns() { field = "Name", title = "Name", width = "550" },
                                new KendoComboBoxColumns() { field = "Type", title = "Type", width = "120" }
                            },
                            KendoMultiColumnComboBoxHeight = 350,
                            CallBackFunction="LoadVendorBankInfo()",
                            AddNewFuntionName="AddNewPayee()",
                            //ConditionField="PayeeType",
                            ReadOnly = No != ""
                        },
                        new HtmlTextBox()
                        {
                            Label="No.:", ID="No",
                            Class="input2", IsCustomForm = true,
                            DBField="No", IsReadOnly = true, //Required = true,
                            HiddenField="NoPrefix",
                            Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label=IsFromBills ==true?"Bill No:":"Expense No:", ID="ExpenseNo",
                            Class="input2", IsCustomForm = true,
                            DBField="ExpenseNo", IsReadOnly = false,
                        },
                    },
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label= IsFromBills?"A/P Account:": "Payment Account:", 
                            ID="PaymentAccountID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-auto",
                            SQL_CBO = ExpenseAccount,
                            DBField = "PaymentAccountID",
                            Value = "ChartOfAccountID",
                            DisplayText = "AccountName",
                            GroupByField = "AccountCategory",
                            IsKendoMultiColumnComboBox = true,
                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                            {
                                new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                                new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                            },
                            KendoMultiColumnComboBoxHeight = 300,
                            Required = true,
                            //Hide = IsFromBills
                        },
                        new HtmlTextBox()
                        {
                            Label="Date:", ID="PaymentDate",
                            Class="input2", IsCustomForm = true,
                            SingleDatePicker=true,
                            DBField="PaymentDate",
                            //DivClassCol="col-auto",
                            Required = true,
                            //Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Terms:", ID="PaymentTerms",
                            Class="input2", IsCustomForm = true,
                            DBField="PaymentTermsID",
                            DivClassCol="col-sm",
                            SingleDatePicker=true,
                            DefaultShowDateValue = false,
                            //IsReadOnly = No != ""
                            //ISNumeric = true,
                            //SQL_CBO= "select PaymentTermsID, TermName from dbo.TblPaymentTerms where SystemID = " + html.SystemID + " and WarehouseID ="+html.WarehouseID + " and DomainID like '%"+html.DomainID+"%'",
                            //Value = "PaymentTermsID",
                            //DisplayText = "TermName",
                            //IsKendoMultiColumnComboBox = true,
                            //Required = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Paid On Date:", ID="PaidOnDate",
                            Class="input2", IsCustomForm = true,
                            SingleDatePicker=true,
                            DBField="PaidOnDate",
                            DefaultShowDateValue = false,
                            Hide = IsFromBills
                            //DivClassCol="col-auto",
                            //Required = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="PV No.:", ID="PVNo",
                            Class="input2", IsCustomForm = true,
                            DBField="PVNo", IsReadOnly = true, //Required = true,
                            HiddenField="PVNoPrefix", Hide = true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Banker's Name:", ID="ChequeBankerName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ChequeBankerName",
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 BankerName from TblVendor where isnull(BankerName, '') <> '' and lower(BankerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by BankerName",
                            //IsReadOnly = No != ""
                        },
                        new HtmlTextBox()
                        {
                            Label="Cheque To:", ID="ChequeTo",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ChequeTo",
                            //IsReadOnly = No != ""
                        },
                        new HtmlTextBox()
                        {
                            Label="Cheque No:", ID="ChequeNo",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ChequeNo",
                            //IsReadOnly = No != ""
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Banker's Name:", ID="BankerName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BankerName",
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 BankerName from TblVendor where isnull(BankerName, '') <> '' and lower(BankerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by BankerName",
                            //IsReadOnly = No != ""
                        },
                        new HtmlTextBox()
                        {
                            Label="Account Name:", ID="BankAccountName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BankAccountName",
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 AccountNo BankAccountNo, AccountName BankAccountName from TblVendor where isnull(AccountName, '') <> '' and lower(AccountName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountName",
                            DisplayOnFields ="BankAccountNo",
                            //IsReadOnly = No != ""
                        },
                        new HtmlTextBox()
                        {
                            Label="Account No:", ID="BankAccountNo",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BankAccountNo",
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 AccountNo from TblVendor where isnull(AccountNo, '') <> '' and lower(AccountNo) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountNo",
                            //IsReadOnly = No != ""
                        }
                    }
                },

                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Checked By", ID="CheckedBy",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CheckedBy", isTypeahead = true,
                            //SQL="select distinct isnull(RequestBy, '') RequestBy from dbo.TblPurchaseOrder \r\nwhere isnull(RequestBy, '') <> '' and SystemID = "+html.SystemID+" and WarehouseID = "+ html.WarehouseID + " order by isnull(RequestBy, '')",
                            SQL="select u.UserLoginID CheckedByUserID, u.TelegramUserName CheckedByTelegramUserName, u.FullName CheckedBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="CheckedByTelegramUserName, CheckedByUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Verified By", ID="VerifiedBy",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="VerifiedBy", isTypeahead = true,
                            SQL="select u.UserLoginID VerifiedByUserID, u.TelegramUserName VerifiedByTelegramUserName, u.FullName VerifiedBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="VerifiedByTelegramUserName, VerifiedByUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto",
                            Required = true,
                        },
                        //new HtmlTextBox()
                        //{
                        //    Label="Approved By", ID="ApprovedBy",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="ApprovedBy", isTypeahead = true,
                        //    SQL="select u.UserLoginID ApprovedByUserID, u.TelegramUserName ApprovedByTelegramUserName, u.FullName ApprovedBy, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                        //    HiddenField="ApprovedByTelegramUserName, ApprovedByUserID",
                        //    HiddenFieldValue="0,0",
                        //    //DivClassCol="col-sm-auto",
                        //    Required = true,
                        //},
                        new HtmlTextBox()
                        {
                            Label="Posted By", ID="PostedBy",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PostedBy", isTypeahead = true,
                            SQL="select u.UserLoginID PostedByUserID, u.TelegramUserName PostedByTelegramUserName, u.FullName PostedBy, u.Job PostedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="PostedByTelegramUserName, PostedByUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto",
                            //Required = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Posting Number", ID="PostingNumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PostingNumber",
                            HiddenField="PostingPrefix",
                            Hide = true,
                            //DivClassCol="col-sm-auto",
                            //Required = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="PO/RF/ADV No.:", ID="Reference",
                            Class="input2", IsCustomForm = true,
                            DBField="Reference",
                            ISAutoComplete=true,
                            SQL_AutoComplete = @"
                SELECT top 20 *
                FROM (
                    SELECT 'PO' AS Source, PONumber AS ReferenceNumber
                    FROM dbo.TblPurchaseOrder
                    WHERE StatusID = 90 AND SystemID = "+html.SystemID+" AND WarehouseID = "+html.WarehouseID+@"
                    AND convert(varchar, VendorID) = case when '@SearchText2' <> '' then '@SearchText2' else VendorID end 

                    UNION ALL

                    SELECT 'RF', ReferenceNumber
                    FROM dbo.TblRequestForm
                    WHERE StatusID = 90 AND SystemID = "+html.SystemID+" AND WarehouseID = "+html.WarehouseID+@"

                    UNION ALL

                    SELECT 'ADV', AdvNo ReferenceNumber
                    FROM dbo.TblAdvanceExpense
                    WHERE ApprovedStatus = 'Approved' AND SystemID = "+html.SystemID+" AND WarehouseID = "+html.WarehouseID+@"
                ) U
                WHERE lower(U.ReferenceNumber) LIKE @SearchText
                ORDER BY U.ReferenceNumber",
                            HiddenField="Source",
                            isTagsInput=true,
                            OnChangeFunction="LoadDetail(this)",
                            IsReadOnly = No != "", ControlNameID = "PayeeID"
                        },
                        //new HtmlTextBox()
                        //{
                        //    Label="Payment Terms:", ID="PaymentTerms",
                        //    Class="input2", IsCustomForm = true,
                        //    DBField="PaymentTermsID",
                        //    DivClassCol="col-sm-auto",
                        //    SingleDatePicker=true,
                        //    DefaultShowDateValue = false,
                        //    //ISNumeric = true,
                        //    //SQL_CBO= "select PaymentTermsID, TermName from dbo.TblPaymentTerms where SystemID = " + html.SystemID + " and WarehouseID ="+html.WarehouseID + " and DomainID like '%"+html.DomainID+"%'",
                        //    //Value = "PaymentTermsID",
                        //    //DisplayText = "TermName",
                        //    //IsKendoMultiColumnComboBox = true,
                        //    //Required = true
                        //},
                        new HtmlTextBox()
                        {
                            Label="Bill/InvoiceNo:", ID="BillInvoiceNo",
                            Class="input2", IsCustomForm = true,
                            DBField="BillInvoiceNo",
                            //DivClassCol="col",
                            //IsReadOnly = No != "",
                            //isTagsInput = true,
                            //HiddenField="SourceBillInvoiceNo",
                        },
                        new HtmlTextBox()
                        {
                            Label="Exchange Rate:", ID="ExchangeRate",
                            Class="input2", IsCustomForm = true,
                            DBField="ExchangeRate",
                            DivClassCol="col-sm-auto",
                            ISNumeric = true,
                            IsReadOnly = No != "",
                            Hide = IsFromBills
                        },
                        new HtmlTextBox()
                        {
                            Label="GrantTotalAsRiel:", ID="GrantTotalAsRiel",
                            Class="input2", IsCustomForm = true,
                            DBField="GrantTotalAsRiel",
                            DivClassCol="col-sm-auto",
                            //ISNumeric = true,
                            Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Discount:", ID="Discount",
                            Class="input2", IsCustomForm = true,
                            DBField="Discount",
                            DivClassCol="col-sm-auto",
                            //ISNumeric = true,
                            Hide = true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextarea()
                        {
                            Label="Description:", ID="Description",
                            Class="input2", IsCustomForm = true,
                            DBField="Description",
                        }
                    }
                },

                new  FormData() {
                    NotRender = No != "",
                    FormTitle="Item Details",
                    TableName="ExpenseDetails",
                    GetIDENTITYByFiled="ExpenseID",
                    PrimaryFieldName="ExpenseID",
                    ShowBtnExpans=true,
                    AutoHideCollapse=false,
                    //SQLEdit="select * from dbo.ExpenseDetails where ExpenseID="+ PrimaryFieldValue + " AND ExpenseCategoryType = 'Item Details'",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            //UserClassTableResponsive = true,
                            //ShowFooter =true,
                            ShowBalance=false,
                            ShowDeposit=false,
                            ShowBankInfo=false,
                            //FooterColspan=10,
                            ShowRemark=false,
                            ShowIncluceVAT=false,
                            ShowRowTotal=false,
                            //AllowEditVAT=true,
                            ShowTotal = false,
                            //ShowTotalOnly = false,
                            //CustomerNotesLabel="Remark",
                            ShowCustomerNotes=false,
                            ShowTermsConditions=false,
                            SubTotalColspan=9,

                            GetIDENTITYByFiled="ExpenseID",
                            PrimaryFieldName="ExpenseID",
                            TableName="ExpenseDetails",
                            //OrderBy="ExpensesID",
                            //ReturnFieldNameValue="PurchaseOrderID, VendorID, PONumber, VendorName, NeedToApproval, TelegramUserName",
                            //ReturnFieldNameValueFromTblName = "TblPurchaseOrder",
                            HtmlTblID="TblExpensesItem",
                            InputName="ItemName, ItemName_Description",
                            TRName="TR_No",
                            TDName="TD_No",
                            ApplyTblTHBackground=false,
                            AutoTotalByRow=true,
                            TotalByInputName="ItemName",
                            TotalByInputNameQTY="QTY",
                            TotalByInputNameAmount="UnitPrice",
                            TotalDisplayOnInputName="Total",
                            TotalByInputNameDiscountUSD="DisUSD",
                            TotalByInputNameTAX="ItemTAX",
                            TotalByInputNameGrandTotal="ItemGrandTotal",
                            ScriptCallBackFunction="Reload_ExpenseCategory,Reload_ItemTAX,Reload_UM(), CallSetAutoComplete_ItemName()",//,BindItemTAXChange()
                            //ScriptCallBackFunction="Reload_ExpenseCategory,Reload_ItemTypeID,Reload_ItemTAX,Reload_UM(), CallSetAutoComplete_ItemName()",//,BindItemTAXChange()
                            // SQL = "select * from dbo.ExpenseDetails where ExpenseID="+ PrimaryFieldValue + " AND ExpenseCategoryType = 'Item Details'",
                            SQL = @"select *, ExpenseCategory IExpenseAccountID
from dbo.ExpenseDetails
where ExpenseID=" + PrimaryFieldValue + @"
  AND (
        ExpenseCategoryType = 'Item Details'
        OR (
            NULLIF(LTRIM(RTRIM(ISNULL(ExpenseCategoryType, ''))), '') IS NULL
            AND (
                ISNULL(ItemID, 0) <> 0
                OR ISNULL(ItemTypeID, 0) <> 0
                OR NULLIF(LTRIM(RTRIM(ISNULL(ItemName, ''))), '') IS NOT NULL
                OR NULLIF(LTRIM(RTRIM(ISNULL(UM, ''))), '') IS NOT NULL
                OR NULLIF(LTRIM(RTRIM(ISNULL(ItemName_Description, ''))), '') IS NOT NULL
            )
        )
  )",

                            dbField=new DBField[]
                            {
                                //new DBField()
                                //{
                                //    DBFieldName="ItemID", Value="ItemID", GetValueFromInputName = true
                                //},
                                new DBField()
                                {
                                    DBFieldName="ItemName_Description", Value="ItemName_Description", GetValueFromTextAreaName = true,
                                },
                                new DBField()
                                {
                                    DBFieldName="ExpenseDetailID", Value="ExpenseDetailID", GetValueFromInputName = true
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>", Label="#"
                                },
                                //new TblTHead()
                                //{
                                //    Label="Type"
                                //},
                                new TblTHead()
                                {
                                     Label="Account", Class="col-2", Hide = true
                                },
                                new TblTHead()
                                {
                                    Label="Item/Service", Class="col-5",
                                },
                                new TblTHead()
                                {
                                     Label="UOM"
                                },
                                new TblTHead()
                                {
                                     Label="QTY"
                                },
                                new TblTHead()
                                {
                                     Label="Unit Price",
                                },
                                new TblTHead()
                                {
                                     Label="Dis$",
                                },
                                new TblTHead()
                                {
                                     Label="Total"
                                },
                                new TblTHead()
                                {
                                     Label="TAX", Class="col-1 d-none"
                                },
                                new TblTHead()
                                {
                                     Label="G.Total",
                                     Class="d-none",
                                     CloseTR="</tr>"
                                }
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TR_No\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TD_No"
                                        },
                                        //new HtmlComboBox()
                                        //{
                                        //    Name="ItemTypeID",
                                        //    Class="input2", IsCustomForm = true,
                                        //    DivClass="wrap-input3",
                                        //    placeholder="Enter Type",
                                        //    SQL_CBO=@"select ServiceID ItemTypeID, ServiceName from dbo.TblServices where SystemID =" + html.SystemID + " and WarehouseID = " +html.WarehouseID,
                                        //    Value="ItemTypeID",
                                        //    DisplayText="ServiceName",
                                        //    DBField="ItemTypeID",
                                        //    IsKendoMultiColumnComboBox=true,
                                        //    Required = true,
                                        //},                                        
                                        new HtmlComboBox()
                                        {
                                            Name="ExpenseCategory",
                                            Class="input2", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Category",
                                            SQL_CBO=@"SELECT
    coa.ChartOfAccountID ExpensesAccountID,
    cat.AccountCategory,
    ct.AccountTypes,
    coa.AccountNo,
    coa.AccountName,
    coa.ParentAccountID
FROM TblChartOfAccount coa
JOIN TblAccountTypes ct ON ct.AccountTypesID = coa.AccountTypesID
JOIN TblAccountCategories cat ON cat.AccountCategoryID = coa.AccountCategoryID
WHERE
    coa.SystemID = "+html.SystemID+@"
    AND coa.WarehouseID = "+html.WarehouseID+@"
ORDER BY AccountNo;

",
                                            DBField = "ExpenseCategory",
                                            Value = "ExpensesAccountID",
                                            DisplayText = "AccountName",
                                            GroupByField = "AccountCategory",
                                            IsKendoMultiColumnComboBox = true,
                                            Hide = true,
                                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                                            {
                                                new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                                                new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "550" }
                                            },
                                            KendoMultiColumnComboBoxHeight = 350,
                                            //Required = true,
                                            //RequiredInputIfOtherFieldIsNotNull = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", 
                                            //Required=true,
                                            DBField = "ItemName",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            //DeleteTDRowFunction="RemoveTableRow('TR_No', 'TD_No', this,'','ItemName','Reload_ExpenseCategory,Reload_ItemTypeID,Reload_ItemTAX,Reload_UM(), CallSetAutoComplete_ItemName()');",
                                            DeleteTDRowFunction="RemoveTableRow('TR_No', 'TD_No', this,'','ItemName');",
                                            //isTypeahead = true,
                                            ISAutoComplete = true,
                                            HiddenField="ItemID", //AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete=@"select distinct top 10 iv.ItemID, i.ItemCode + '-' + i.ItemName ItemName, i.ItemDescription ItemName_Description
from dbo.TblItem i 
left join dbo.TblItemVariant iv on iv.ItemID = i.ItemID
left join dbo.TblItemSerialDetail s on s.ItemID = i.ItemID
where lower(i.ItemCode + '-' + iv.ItemName + case when isnull(iv.Model,'') <> '' then ' - ' + isnull(iv.Model,'') else '' end) like @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID +" and i.DomainID like '%"+html.DomainID+@"%'
order by i.ItemCode + '-' + i.ItemName",
                                            SQLCallBack ="select distinct iv.UM,iv.UomID UomIDi, iv.CostPrice UnitPrice, iv.SellingPrice SalePrice, iv.ItemDescription ItemName_Description, iv.ExpenseAccountID IExpenseAccountID, coa.AccountName IAccountName, coa.AccountNo IAccountNo from dbo.TblItemVariant iv left join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = iv.ExpenseAccountID where iv.ItemID = @SearchText",
                                            DisplayOnFields="UM, UomIDi, UnitPrice, SalePrice, ItemName_Description, IExpenseAccountID, IAccountName, IAccountNo",
                                            SQLTypeaheadCallBack = "select distinct ItemID, UomID UomIDi, UM, CostPrice UnitPrice, SellingPrice SalePrice from dbo.TblItemVariant where ItemID = @ItemID ",
                                            DisplayOnFieldsTypeahead="UnitPrice, SalePrice, UomIDi",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="UM", FieldName = "ItemID",
                                            CallBackDisplayText="UM",
                                            //TypeaheadCallBackFunction="TotalAll()",
                                            InputDescription = true,
                                            placeholder="Item/Service",
                                            RequiredInputIfOtherFieldIsNotNull = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter UOM", 
                                            //Required = true,
											HiddenField="UomID, IExpenseAccountID, IAccountName, IAccountNo",
                                            HiddenFieldValue = "0,0,0,0",
                                            DBField = "UM",
                                            //RequiredInputIfOtherFieldIsNotNull = true,
                                            isTypeahead=true,
                                            SQL="select UomID UomID, [Name] UM from dbo.TblUom where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID,

                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="QTY", IsAllowInputOnlyNumber =true,
                                            placeholder="Enter QTY",
                                            //Required = true,
                                            //RequiredInputIfOtherFieldIsNotNull = true,
                                            //OnChangeFunction="TotalAll();",OnKeyupFunction="TotalAll();",
                                            HiddenField="RefNo,SourceType,ExpenseDetailID",
                                            HiddenFieldValue="0,0,0"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UnitPrice",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter UnitPrice",
                                            DBField="UnitPrice",
                                            IsAllowInputOnlyNumber =true,
                                            //Required = true,
                                            RequiredInputIfOtherFieldIsNotNull = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="DisUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                            //Required = true,
                                            //OnKeyupFunction="onChangeDisUSD(this);",
                                            HiddenField="DisP"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Total", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="Total", IsAllowInputOnlyNumber =true,
                                            IsReadOnly=true,
                                            placeholder="Total",
                                            //Hide = true
                                            //OnKeyupFunction="AmountKeyup(this);"
                                        },
                                        new HtmlComboBox()
                                        {
                                            Name="ItemTAX", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="ItemTAX", IsAllowInputOnlyNumber =true,
                                            placeholder="TAX",
                                            SQL_CBO=@"select Rate, TaxName ItemTAX from dbo.TblTax where SystemID =" + html.SystemID + " and WarehouseID = " +html.WarehouseID,
                                            Value="Rate",
                                            DisplayText="ItemTAX",
                                            IsKendoMultiColumnComboBox = true,
                                            //Required = true, 
                                            CallBackFunction="ItemTAX_Change(this);",
                                            Hide = true
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemGrandTotal", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="ItemGrandTotal", IsAllowInputOnlyNumber =true,
                                            IsReadOnly=true, placeholder="Grand Total",
                                            Hide = true
                                            //OnKeyupFunction="AmountKeyup(this);"
                                        }
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },
            //for vendor deposit
                new  FormData() {
                    NotRender = No != "",
                    FormTitle="Expenses",
                    TableName="ExpenseDetails",
                    GetIDENTITYByFiled="ExpenseID",
                    PrimaryFieldName="ExpenseID",
                    ShowBtnExpans=true,
                    AutoHideCollapse=false,
                    //SQLEdit="select * from dbo.ExpenseDetails where ExpenseID="+ PrimaryFieldValue + " AND ExpenseCategoryType = 'Expenses'",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            UserClassTableResponsive = false,
                            ShowFooter =false,
                            ShowBalance=false,
                            ShowDeposit=false,
                            ShowBankInfo=false,
                            //FooterColspan=10,
                            ShowRemark=false,
                            ShowIncluceVAT=false,
                            ShowTotal=false,
                            //AllowEditVAT=true,
                            //ShowTotal = false,
                            //ShowTotalOnly = true,
                           
                            //CustomerNotesLabel="Remark",
                            ShowCustomerNotes=false,
                            ShowTermsConditions=false,
                            //SubTotalColspan=9,

                            GetIDENTITYByFiled="ExpenseID",
                            PrimaryFieldName="ExpenseID",
                            TableName="ExpenseDetails",
                            //OrderBy="ExpensesID",
                            //ReturnFieldNameValue="PurchaseOrderID, VendorID, PONumber, VendorName, NeedToApproval, TelegramUserName",
                            //ReturnFieldNameValueFromTblName = "TblPurchaseOrder",
                            HtmlTblID="ExpenseDetails",
                            InputName="ExpenseDetailsDescription",
                            TRName="VTR_No",
                            TDName="VTD_No",
                            ApplyTblTHBackground=false,
                            AutoTotalByRow=true,
                            TotalByTextAreaName="ExpenseDetailsDescription",
                            TotalByInputNameQTY="ExpenseDetailsQTY",
                            TotalByInputNameAmount="ExpenseDetailsAmount",
                            TotalDisplayOnInputName="ExpenseDetailsTotal",
                            //TotalByInputNameDiscountUSD="VDisUSD",
                            //TotalByInputNameTAX="VItemTAX",
                            //TotalByInputNameGrandTotal="VItemGrandTotal",
                            
                            ScriptCallBackFunction="Reload_AccountVendorDeposit,Reload_ExpenseDetailsUM()",//,BindItemTAXChange()
                            SQL = "select * from dbo.ExpenseDetails where ExpenseID="+ PrimaryFieldValue + " AND ExpenseCategoryType = 'Expenses'",
                    
                            //dbField=new DBField[]
                            //{
                            //    new DBField()
                            //    {
                            //        DBFieldName="ItemName_Description", Value="ItemName_Description", GetValueFromTextAreaName = true,
                            //    },
                            //    new DBField()
                            //    {
                            //        DBFieldName="ExpenseDetailID", Value="ExpenseDetailID", GetValueFromInputName = true
                            //    },
                            //},
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>", Label="#"
                                },
                                new TblTHead()
                                {
                                     Label="Account",
                                },
                                new TblTHead()
                                {
                                    Label="Description", Class="col-4",
                                },
                                new TblTHead()
                                {
                                     Label="UOM",
                                },
                                new TblTHead()
                                {
                                     Label="QTY",
                                },
                                new TblTHead()
                                {
                                     Label="Unit Price",
                                },
                                new TblTHead()
                                {
                                     Label="Total",
                                     CloseTR="</tr>"
                                }
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"VTR_No\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="VTD_No"
                                        },
                                        new HtmlComboBox()
                                        {
                                            Name="AccountVendorDeposit",
                                            Class="input2", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Account",
                                            SQL_CBO=@"SELECT
    coa.ChartOfAccountID ExpensesAccountID,
    cat.AccountCategory,
    ct.AccountTypes,
    coa.AccountNo,
    coa.AccountName,
    coa.ParentAccountID
FROM TblChartOfAccount coa
JOIN TblAccountTypes ct ON ct.AccountTypesID = coa.AccountTypesID
JOIN TblAccountCategories cat ON cat.AccountCategoryID = coa.AccountCategoryID
WHERE
    coa.SystemID = "+html.SystemID+@"
    AND coa.WarehouseID = "+html.WarehouseID+@"
ORDER BY AccountNo;

",
                                            DBField = "ExpenseCategory",
                                            Value = "ExpensesAccountID",
                                            DisplayText = "AccountName",
                                            GroupByField = "AccountCategory",
                                            IsKendoMultiColumnComboBox = true,
                                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                                            {
                                                new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                                                new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "550" }
                                            },
                                            KendoMultiColumnComboBoxHeight = 350,
                                            //Required = true,
                                            //RequiredInputIfOtherFieldIsNotNull = true,
                                        },
                                        new HtmlTextarea()
                                        {
                                            Name="ExpenseDetailsDescription",
                                            //Required=true,
                                            DBField = "ItemName",
                                            GetValueFromTextAreaName = true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            //DeleteTDRowFunction="RemoveTableRow('TR_No', 'TD_No', this,'','ItemName','Reload_ExpenseCategory,Reload_ItemTypeID,Reload_ItemTAX,Reload_UM(), CallSetAutoComplete_ItemName()');",
                                            DeleteTDRowFunction="RemoveTableRow('VTR_No', 'VTD_No', this,'','ExpenseDetailsDescription','Reload_AccountVendorDeposit,Reload_ExpenseDetailsUM(),ExpenseDetails_TotalAll');",
                                            placeholder="Description",
                                            //RequiredInputIfOtherFieldIsNotNull = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ExpenseDetailsUM",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter UOM",
                                            HiddenField="ExpenseDetailsUomID",
                                            HiddenFieldValue = "0",
                                            DBField = "UM",
                                            isTypeahead=true,
                                            SQL="select UomID ExpenseDetailsUomID, [Name] UM from dbo.TblUom where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ExpenseDetailsQTY", DivClass="wrap-input3 text-center",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="QTY", IsAllowInputOnlyNumber =true,
                                            placeholder="Enter QTY",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ExpenseDetailsAmount", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="UnitPrice", IsAllowInputOnlyNumber =true,
                                            placeholder="Enter Amount",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ExpenseDetailsTotal", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="Total", IsAllowInputOnlyNumber =true, IsReadOnly=true,
                                            placeholder="Total",
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },

            };
            html.ShowDialogTotalLabelNameFooter = true;
            html.SaveChangeFunction = "SaveChangeExpense()";
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();

            if (PaymentNo != "")
            {
                string detailSql = $@"
declare @PreviousPaidAmount decimal(18,2);
select @PreviousPaidAmount = PaidAmount from Expense_PaymentTransactions where PaymentNo ='{PostedPaymentNo}'
SELECT 
    ed.ExpenseCategory,
    coa.AccountName AS ExpenseAccount,
    coa.AccountNo,
    ed.ItemName,
    ed.ItemName_Description,
    ed.QTY,
    ed.UnitPrice,
    ed.Total,
    ed.ItemTAX,
    ed.DisUSD,
    ed.UM
FROM ExpenseDetails ed
INNER JOIN TblChartOfAccount coa
    ON ed.ExpenseCategory = coa.ChartOfAccountID
WHERE ed.ExpenseID = @ExpenseID
ORDER BY ed.ExpenseDetailID;

SELECT
    TotalAmount = MAX(TotalAmount),
    PaidAmount = SUM(PaidAmount),
    BalanceAmount = MAX(TotalAmount) - SUM(PaidAmount),
    PaymentStatus =
        CASE
            WHEN SUM(PaidAmount) = 0 THEN 'Unpaid'
            WHEN SUM(PaidAmount) >= MAX(TotalAmount) THEN 'Paid'
            ELSE 'Partial'
        END,
    PostedPaymentNo = MAX(PostedPaymentNo),
    IsRequestedPayment = CAST(MAX(CAST(IsRequestedPayment AS INT)) AS BIT),
    PreviousPaidAmount = @PreviousPaidAmount
FROM Expense_PaymentTransactions
WHERE ExpenseID = @ExpenseID;

SELECT
    ROW_NUMBER() OVER(ORDER BY CreatedDate) AS No,
    PostedDate, IsPosted,
    PaymentNo,
    pm.PaymentMethod,
    PaidAmount,
    BalanceAmount,
    u.FullName AS PaidBy,
    PaymentStatus,
    Description AS Remark, pt.Status, pt.CreatedDate, pt.PostedPaymentNo, pt.IsRequestedPayment
FROM Expense_PaymentTransactions pt
INNER JOIN TblPaymentMethod pm
    ON pm.PaymentMethodID = pt.PaymentMethodID
LEFT JOIN dbo.TblUserLogin u ON u.UserLoginID = pt.PostedByUserID
WHERE ExpenseID = @ExpenseID
ORDER BY CreatedDate;
";

                DataTable dtDetail = new DataTable();
                using (SqlConnection conn = new SqlConnection(html.GetConnectionString()))
                {
                    conn.Open();
                    DataSet ds = new DataSet();

                    using (SqlCommand cmd = new SqlCommand(detailSql, conn))
                    {
                        cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = PrimaryFieldValue;
                        cmd.Parameters.Add("@PaymentNo", SqlDbType.NVarChar).Value = PaymentNo;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                    }

                    DataTable dtItem = ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
                    DataTable dtSummary = ds.Tables.Count > 1 ? ds.Tables[1] : new DataTable();
                    DataTable dtPaymentHistory = ds.Tables.Count > 2 ? ds.Tables[2] : new DataTable();

                    ViewBag.ItemDetails = dtItem;
                    ViewBag.dtSummary = dtSummary;
                    ViewBag.dtPaymentHistory = dtPaymentHistory;
                }
            }

            return View();
        }

        public ActionResult LoadDetail(string PO, string RF, string ADV)
        {
            HTML html = new HTML();
            List<string> conditions = new List<string>();

            if (!string.IsNullOrEmpty(PO))
                conditions.Add($"(SourceType = 'PO' AND RefNo IN ('{PO.Replace(",", "','")}'))");

            if (!string.IsNullOrEmpty(RF))
                conditions.Add($"(SourceType = 'RF' AND RefNo IN ('{RF.Replace(",", "','")}'))");

            if (!string.IsNullOrEmpty(ADV))
                conditions.Add($"(SourceType = 'ADV' AND RefNo IN ('{ADV.Replace(",", "','")}'))");

            #region Old SQL
            /*
            string baseSql_Old = @"SELECT *
FROM (
    select 
        pd.ItemID, 
        i.ItemName, 
        isnull(pd.ItemDescription, '') ItemName_Description, 
        pd.UomID, 
        pd.UM UM, 
        pd.QTY, 
        pd.UnitPrice,
        po.Tax, 
        po.Rate, 
        pd.Amount Total, 
        po.VAT, 
        po.VendorID, 
        v.VendorName, 
        0 DisUSD, 
        po.Rate ItemTAX, 
        pd.Amount ItemGrandTotal, 
        coa.ChartOfAccountID ExpensesAccountID, 
        i.ItemTypeID, 
        'PO' AS SourceType, 
        po.PONumber AS RefNo, 
        po.SystemID, 
        po.WarehouseID, 
        isnull(v.BankerName, '') BankerName, isnull(v.SwiftCode, '') SwiftCode, isnull(v.AccountNo, '') AccountNo, isnull(v.AccountName, '') AccountName, 
        po.ClassID, po.ClassName
    from dbo.TblPurchaseOrderDetail pd
    inner join dbo.TblPurchaseOrder po on po.PurchaseOrderID = pd.PurchaseOrderID
    inner join dbo.TblVendor v on v.VendorID = po.VendorID
    inner join dbo.TblItem i on i.ItemID = pd.ItemID
    inner join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = i.ExpenseAccountID

    union all

    SELECT 
        fd.RequestFormDetailID AS ItemID,
        fd.ItemDescription AS ItemName,
        '' AS ItemName_Description,
        0 AS UomID,
        ISNULL(fd.UM,'') AS UM,
        fd.QTY,
        fd.UnitPrice,
        NULL AS Tax,
        0 AS Rate,
        fd.Amount AS Total,
        0 AS VAT,
        f.VendorID,
        ISNULL(v.VendorName, '') AS VendorName,
        0 AS DisUSD,
        0 AS ItemTAX,
        fd.Amount AS ItemGrandTotal,
        NULL AS ExpensesAccountID,
        NULL AS ItemTypeID,
        'RF' AS SourceType,
        f.ReferenceNumber AS RefNo,
        f.SystemID,
        f.WarehouseID,
        isnull(v.BankerName, '') BankerName, isnull(v.SwiftCode, '') SwiftCode, isnull(v.AccountNo, '') AccountNo, isnull(v.AccountName, '') AccountName, 
        f.ClassID, f.ClassName
    FROM dbo.TblRequestFormDetail fd
    INNER JOIN dbo.TblRequestForm f ON f.RequestFormID = fd.RequestFormID
    LEFT JOIN dbo.TblVendor v ON v.VendorID = f.VendorID
    
union all

    SELECT 
    fd.AdvanceExpenseDetailID AS ItemID,
    fd.Description AS ItemName,
    '' AS ItemName_Description,
    0 AS UomID,
    '' AS UM,
    1 AS QTY,
    amt.Amount AS UnitPrice,
    NULL AS Tax,
    0 AS Rate,
    amt.Amount AS Total,
    0 AS VAT,
    f.VendorID,
    ISNULL(v.VendorName,'') AS VendorName,
    0 AS DisUSD,
    0 AS ItemTAX,
    amt.Amount AS ItemGrandTotal,
    NULL  AS ExpensesAccountID,
    NULL  AS ItemTypeID,
    'ADV' AS SourceType,
    f.AdvNo AS RefNo,
    f.SystemID,
    f.WarehouseID,
    isnull(v.BankerName, '') BankerName, isnull(v.SwiftCode, '') SwiftCode, isnull(v.AccountNo, '') AccountNo, isnull(v.AccountName, '') AccountName, 
    f.ClassNameID ClassID, f.ClassName
FROM dbo.TblAdvanceExpenseDetail fd
INNER JOIN dbo.TblAdvanceExpense f 
    ON f.AdvanceExpenseID = fd.AdvanceExpenseID
LEFT JOIN dbo.TblVendor v 
    ON v.VendorID = f.VendorID
CROSS APPLY (
    SELECT 
        CASE 
            WHEN ISNULL(fd.AmountBank,0) = 0 
            THEN ISNULL(fd.AmountCase,0) 
            ELSE ISNULL(fd.AmountBank,0) 
        END AS Amount
) amt

) X;
            */
            #endregion

            string baseSql = @"SELECT *
FROM (
    select 
        pd.ItemID, 
        i.ItemName, 
        isnull(pd.ItemDescription, '') ItemName_Description, 
        pd.UomID, 
        pd.UM UM, 
        pd.QTY, 
        pd.UnitPrice,
        po.Tax, 
        po.Rate, 
        pd.Amount Total, 
        po.VAT,
        po.VendorID, 
        v.VendorName,
        0 DisUSD, 
        po.Rate ItemTAX, 
        pd.Amount ItemGrandTotal, 
        coa.ChartOfAccountID ExpensesAccountID, 
        i.ItemTypeID,
        'PO' AS SourceType,
        po.PONumber AS RefNo,
        po.SystemID,
        po.WarehouseID,
    isnull(v.BankerName, '') BankerName, isnull(v.SwiftCode, '') SwiftCode, isnull(v.AccountNo, '') AccountNo, isnull(v.AccountName, '') AccountName,
    po.ClassID, po.ClassName,
    '' AS ExpenseDetailsDescription,
    '' AS ExpenseDetailsUM,
    CONVERT(decimal(18,3), 0) AS ExpenseDetailsQTY,
    CONVERT(decimal(18,4), 0) AS ExpenseDetailsAmount,
    CONVERT(decimal(18,2), 0) AS ExpenseDetailsTotal,
    coa.ChartOfAccountID AS ExpenseCategory
    from dbo.TblPurchaseOrderDetail pd
    inner join dbo.TblPurchaseOrder po on po.PurchaseOrderID = pd.PurchaseOrderID
    inner join dbo.TblVendor v on v.VendorID = po.VendorID
    inner join dbo.TblItem i on i.ItemID = pd.ItemID
    inner join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = i.ExpenseAccountID

    union all

    SELECT 
        0 AS ItemID,
        fd.ItemDescription AS ItemName,
        '' AS ItemName_Description,
        0 AS UomID,
        ISNULL(fd.UM,'') AS UM,
        fd.QTY,
        fd.UnitPrice,
        NULL AS Tax,
        0 AS Rate,
        fd.Amount AS Total,
        0 AS VAT,
        f.VendorID,
        ISNULL(v.VendorName, '') AS VendorName,
        0 AS DisUSD,
        0 AS ItemTAX,
        fd.Amount AS ItemGrandTotal,
        NULL AS ExpensesAccountID,
        NULL AS ItemTypeID,
        'RF' AS SourceType,
        f.ReferenceNumber AS RefNo,
        f.SystemID,
        f.WarehouseID,
        isnull(v.BankerName, '') BankerName, isnull(v.SwiftCode, '') SwiftCode, isnull(v.AccountNo, '') AccountNo, isnull(v.AccountName, '') AccountName,
        f.ClassID, f.ClassName,
        fd.ItemDescription AS ExpenseDetailsDescription,
        ISNULL(fd.UM, '') AS ExpenseDetailsUM,
        fd.QTY AS ExpenseDetailsQTY,
        fd.UnitPrice AS ExpenseDetailsAmount,
        fd.Amount AS ExpenseDetailsTotal,
        CONVERT(int, NULL) AS ExpenseCategory
    FROM dbo.TblRequestFormDetail fd
    INNER JOIN dbo.TblRequestForm f ON f.RequestFormID = fd.RequestFormID
    LEFT JOIN dbo.TblVendor v ON v.VendorID = f.VendorID
    
union all

    SELECT 
    0 AS ItemID,
    fd.Description AS ItemName,
    '' AS ItemName_Description,
    0 AS UomID,
    '' AS UM,
    CONVERT(decimal(18,3), 1) AS QTY,
    amt.Amount AS UnitPrice,
    NULL AS Tax,
    0 AS Rate,
    amt.Amount AS Total,
    0 AS VAT,
    f.VendorID,
    ISNULL(v.VendorName,'') AS VendorName,
    0 AS DisUSD,
    0 AS ItemTAX,
    amt.Amount AS ItemGrandTotal,
    NULL  AS ExpensesAccountID,
    NULL  AS ItemTypeID,
    'ADV' AS SourceType,
    f.AdvNo AS RefNo,
    f.SystemID,
    f.WarehouseID,
    isnull(v.BankerName, '') BankerName, isnull(v.SwiftCode, '') SwiftCode, isnull(v.AccountNo, '') AccountNo, isnull(v.AccountName, '') AccountName,
    f.ClassNameID ClassID, f.ClassName,
    fd.Description AS ExpenseDetailsDescription,
    '' AS ExpenseDetailsUM,
    CONVERT(decimal(18,3), 1) AS ExpenseDetailsQTY,
    amt.Amount AS ExpenseDetailsAmount,
    amt.Amount AS ExpenseDetailsTotal,
    CONVERT(int, NULL) AS ExpenseCategory
FROM dbo.TblAdvanceExpenseDetail fd
INNER JOIN dbo.TblAdvanceExpense f 
    ON f.AdvanceExpenseID = fd.AdvanceExpenseID
LEFT JOIN dbo.TblVendor v 
    ON v.VendorID = f.VendorID
CROSS APPLY (
    SELECT 
        CASE 
            WHEN ISNULL(fd.AmountBank,0) = 0 
            THEN ISNULL(fd.AmountCase,0) 
            ELSE ISNULL(fd.AmountBank,0) 
        END AS Amount
) amt

) X
";

            if (conditions.Any())
            {
                baseSql += " WHERE (" + string.Join(" OR ", conditions) + ")";
            }

            baseSql += $" AND SystemID = {html.SystemID} AND WarehouseID = {html.WarehouseID}";

            string data = html.JsonData(baseSql);

            return Json(new
            {
                Data = data
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadDetailRG(string Reference)
        {
            HTML html = new HTML();
            string sql = @"select sd.ItemID, sd.ItemName, sd.ItemDescription ItemDescription, pd.UomID, pd.UM Uom, sd.QTY, pd.UnitPrice,
po.CustomerNotes Remark, po.BankInformation BankInfo, po.TermsConditions, po.Tax, po.Rate, pd.Amount Total, po.VAT, pd.DisUSD,
po.VendorID, v.VendorName
from dbo.TblStockInDetail sd
inner join dbo.TblStockIn si on si.StockInID = sd.StockInID
inner join dbo.TblVendor v on v.VendorID = si.VendorID
left join dbo.TblPurchaseOrder po on po.PurchaseOrderID = si.PurchaseOrderID
left join dbo.TblPurchaseOrderDetail pd on pd.PurchaseOrderID = po.PurchaseOrderID and sd.ItemID = pd.ItemID
where sd.[Type] = 'Stock In' and si.StockInID in (" + Reference + ") and si.SystemID = " + html.SystemID + " and si.WarehouseID = " + html.WarehouseID;


            string Data = html.JsonData(sql);
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Expenses, Edit Expenses")]
        [HttpPost, ValidateInput(false)]
        public JsonResult SaveChangeExpense(dynamic DATA)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string DestinationFolder = "/Uploads/Expenses/";//$"{html.ToUrlSlug($"{CustomerName + " [" + CustomerCode + "]"}")}";
            SqlResult result = new SqlResult();
            result = html.UploadFile(DestinationFolder);
            string AttachmentFile;
            if (result.Result == ResultType.Successful)
                AttachmentFile = result.Value;
            else
            {
                return Json(new
                {
                    Message = result.Message,
                    Result = result.Result,
                }, JsonRequestBehavior.AllowGet);
            }
            dynamic DataInfos = new JavaScriptSerializer().DeserializeObject(DATA[0].ToString());
            dynamic ExpenseData = DataInfos["ExpenseData"];
            dynamic ExpensesCategoryData = DataInfos["ExpensesCategoryData"];
            dynamic ExpensesItemData = DataInfos["ExpensesItemData"];
            StringBuilder sb = new StringBuilder();

            string DraftNo = ExpenseData[0]["DraftNo"].ToString();
            sb.AppendLine("<Data>");

            foreach (dynamic item in ExpenseData)
            {
                sb.AppendLine("<ExpenseData>");
                sb.AppendLine($"<ExpensesID>{item["ExpensesID"]}</ExpensesID>");
                sb.AppendLine($"<TransactionID>{item["TransactionID"]}</TransactionID>");
                sb.AppendLine($"<Type>{item["Type"]}</Type>");
                sb.AppendLine($"<TransactionTypeID>{item["TransactionTypeID"]}</TransactionTypeID>");
                sb.AppendLine($"<TransactionType>{item["TransactionType"]}</TransactionType>");
                sb.AppendLine($"<UserID>{html.UserID}</UserID>");
                sb.AppendLine($"<PayeeID>{item["PayeeID"]}</PayeeID>");
                sb.AppendLine($"<PayeeType>{item["PayeeType"]}</PayeeType>");
                sb.AppendLine($"<PaymentAccountID>{item["PaymentAccountID"]}</PaymentAccountID>");
                sb.AppendLine($"<PaymentDate>{item["PaymentDate"]}</PaymentDate>");
                sb.AppendLine($"<PaymentMethodID>{item["PaymentMethodID"]}</PaymentMethodID>");
                sb.AppendLine($"<ClassNameID>{item["ClassNameID"]}</ClassNameID>");
                sb.AppendLine($"<AccountExpenseTypeID>{item["AccountExpenseTypeID"]}</AccountExpenseTypeID>");
                sb.AppendLine($"<InvoiceDate>{item["InvoiceDate"]}</InvoiceDate>");
                sb.AppendLine($"<InvoiceNo>{item["InvoiceNo"]}</InvoiceNo>");
                sb.AppendLine($"<PaymentTerms>{item["PaymentTerms"]}</PaymentTerms>");
                sb.AppendLine($"<NumberofDays>{item["NumberofDays"]}</NumberofDays>");
                sb.AppendLine($"<BillNo>{item["BillNo"]}</BillNo>");
                sb.AppendLine($"<PONo>{item["PONo"]}</PONo>");
                sb.AppendLine($"<PurchaseOrderID>{item["PurchaseOrderID"]}</PurchaseOrderID>");
                sb.AppendLine($"<GRN>{item["GRN"]}</GRN>");
                sb.AppendLine($"<ATTName>{item["ATTName"]}</ATTName>");
                sb.AppendLine($"<StockInID>{item["StockInID"]}</StockInID>");
                sb.AppendLine($"<Warehouse>{item["WarehouseName"]}</Warehouse>");
                sb.AppendLine($"<ReferenceNo>{item["ReferenceNo"]}</ReferenceNo>");
                sb.AppendLine($"<RequestFormID>{item["RequestFormID"]}</RequestFormID>");
                sb.AppendLine($"<ReferenceType>{item["ReferenceType"]}</ReferenceType>");
                sb.AppendLine($"<CheckNo>{item["CheckNo"]}</CheckNo>");

                sb.AppendLine($"<CheckTo>{item["CheckTo"]}</CheckTo>");

                sb.AppendLine($"<CheckedByUserID>{item["CheckedByUserID"]}</CheckedByUserID>");
                sb.AppendLine($"<CheckedBy>{item["CheckedBy"]}</CheckedBy>");
                sb.AppendLine($"<CheckedByTelegramUserName>{item["CheckedByTelegramUserName"]}</CheckedByTelegramUserName>");

                sb.AppendLine($"<VerifiedByUserID>{item["VerifiedByUserID"]}</VerifiedByUserID>");
                sb.AppendLine($"<VerifiedBy>{item["VerifiedBy"]}</VerifiedBy>");
                sb.AppendLine($"<VerifiedByTelegramUserName>{item["VerifiedByTelegramUserName"]}</VerifiedByTelegramUserName>");

                sb.AppendLine($"<ApprovedByUserID>{item["ApprovedByUserID"]}</ApprovedByUserID>");
                sb.AppendLine($"<ApprovedBy>{item["ApprovedBy"]}</ApprovedBy>");
                sb.AppendLine($"<ApprovedByTelegramUserName>{item["ApprovedByTelegramUserName"]}</ApprovedByTelegramUserName>");

                sb.AppendLine($"<DraftNo>{item["DraftNo"]}</DraftNo>");
                sb.AppendLine($"<CreateBy>{html.UserFullName}</CreateBy>");
                sb.AppendLine($"<CreateDate>{html.GetCurrentDate()}</CreateDate>");
                sb.AppendLine($"<Total>{item["Total"]}</Total>");
                sb.AppendLine($"<AttachmentFile>{AttachmentFile}</AttachmentFile>");
                sb.AppendLine($"<SystemID>{html.SystemID}</SystemID>");
                sb.AppendLine($"<WarehouseID>{html.WarehouseID}</WarehouseID>");
                sb.AppendLine($"<DomainID>{html.DomainID}</DomainID>");

                sb.AppendLine($"<ExpenseDetailSubTotal>{item["ExpenseDetailSubTotal"]}</ExpenseDetailSubTotal>");
                sb.AppendLine($"<ExpenseDetailVAT>{item["ExpenseDetailVAT"]}</ExpenseDetailVAT>");
                sb.AppendLine($"<ExpenseDetailTotal>{item["ExpenseDetailTotal"]}</ExpenseDetailTotal>");
                sb.AppendLine($"<ExpenseDetailDiscount>{item["ExpenseDetailDiscount"]}</ExpenseDetailDiscount>");
                sb.AppendLine($"<ExpenseDetailGrandTotal>{item["ExpenseDetailGrandTotal"]}</ExpenseDetailGrandTotal>");
                sb.AppendLine($"<ItemDetailSubTotal>{item["ItemDetailSubTotal"]}</ItemDetailSubTotal>");
                sb.AppendLine($"<ItemDetailVAT>{item["ItemDetailVAT"]}</ItemDetailVAT>");
                sb.AppendLine($"<ItemDetailTotal>{item["ItemDetailTotal"]}</ItemDetailTotal>");
                sb.AppendLine($"<ItemDetailDiscount>{item["ItemDetailDiscount"]}</ItemDetailDiscount>");
                sb.AppendLine($"<ItemDetailGrandTotal>{item["ItemDetailGrandTotal"]}</ItemDetailGrandTotal>");
                sb.AppendLine("</ExpenseData>");
            }

            foreach (dynamic item in ExpensesCategoryData)
            {
                sb.AppendLine("<ExpensesCategoryData>");
                sb.AppendLine($"<DraftNo>{DraftNo}</DraftNo>");
                sb.AppendLine($"<ExpensesAccountID>{item["ExpensesAccountID"]}</ExpensesAccountID>");
                sb.AppendLine($"<ExpenseDescription>{item["ExpenseDescription"]}</ExpenseDescription>");
                sb.AppendLine($"<UomID>{item["UomID"]}</UomID>");
                sb.AppendLine($"<ExpenseUOM>{item["ExpenseUOM"]}</ExpenseUOM>");
                sb.AppendLine($"<ExpenseQTY>{item["ExpenseQTY"]}</ExpenseQTY>");
                sb.AppendLine($"<ExpenseAmount>{item["ExpenseAmount"]}</ExpenseAmount>");
                sb.AppendLine($"<DisUSD>{item["DisUSD"]}</DisUSD>");
                sb.AppendLine($"<ExpenseTotal>{item["ExpenseTotal"]}</ExpenseTotal>");
                sb.AppendLine("</ExpensesCategoryData>");
            }

            foreach (dynamic item in ExpensesItemData)
            {
                sb.AppendLine("<ExpensesItemData>");
                sb.AppendLine($"<DraftNo>{DraftNo}</DraftNo>");
                sb.AppendLine($"<ItemID>{item["ItemID"]}</ItemID>");
                sb.AppendLine($"<ItemName>{item["ItemName"]}</ItemName>");
                sb.AppendLine($"<ItemNameDescription>{item["ItemNameDescription"]}</ItemNameDescription>");
                sb.AppendLine($"<UomID>{item["UomID"]}</UomID>");
                sb.AppendLine($"<UM>{item["UM"]}</UM>");
                sb.AppendLine($"<QTY>{item["QTY"]}</QTY>");
                sb.AppendLine($"<UnitPrice>{item["UnitPrice"]}</UnitPrice>");
                sb.AppendLine($"<DisUSD>{item["DisUSD"]}</DisUSD>");
                sb.AppendLine($"<Total>{item["Total"]}</Total>");
                sb.AppendLine("</ExpensesItemData>");
            }

            sb.AppendLine("</Data>");

            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "Data", "N'"+sb.ToString()+"'" },
            };

            sqlResult = html.SaveChangeData("SP_INSERT_EXPENSES", true, Para, true, true);

            return Json(new { Data = sqlResult.Data.Tables[0].Rows[0].ToJsonString(), Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public class DocumentLookupVM
        {
            public string DocumentNo { get; set; }
            public string Source { get; set; }
        }
        [AllowAnonymous]
        public ActionResult Search(string text)
        {
            var list = new List<DocumentLookupVM>();
            DBHelper dB = new DBHelper();
            using (var con = new SqlConnection(
                dB.ConnectionString))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandText = @"
                SELECT *
                FROM (
                    SELECT PONumber AS DocumentNo, 'PO' AS Source
                    FROM dbo.TblPurchaseOrder
                    WHERE StatusID = 90 AND SystemID = 13 AND WarehouseID = 16

                    UNION ALL

                    SELECT ReferenceNumber, 'RF'
                    FROM dbo.TblRequestForm
                    WHERE StatusID = 90 AND SystemID = 13 AND WarehouseID = 16

                    UNION ALL

                    SELECT AdvNo, 'ADV'
                    FROM dbo.TblAdvanceExpense
                    WHERE ApprovedStatus = 'Approved' AND SystemID = 13 AND WarehouseID = 16
                ) U
                WHERE U.DocumentNo LIKE '%' + @filter + '%'
                ORDER BY U.DocumentNo";

                cmd.Parameters.AddWithValue("@filter", text ?? "");

                con.Open();
                using (var r = cmd.ExecuteReader())

                {
                    while (r.Read())
                    {
                        list.Add(new DocumentLookupVM
                        {
                            DocumentNo = r["DocumentNo"].ToString(),
                            Source = r["Source"].ToString()
                        });
                    }
                }
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }
//         public JsonResult GetPVNumber(int ClassID, string paymentDate = "")
//         {
//             string PVNo = "", Prefix = "", PrefixExpense = "";
//             HTML html = new HTML();
//             if (!string.IsNullOrEmpty(paymentDate) && DateTime.TryParse(paymentDate, out DateTime pDate))
//             {
//                 string yyMM = pDate.ToString("yyMM");
//                 string sqlCustom = @"
//                     DECLARE @PrefixPV NVARCHAR(50) = '', @PrefixExpense NVARCHAR(50) = '';
//                     SELECT @PrefixPV = PrefixPV, @PrefixExpense = PrefixExpense FROM dbo.TblClass WHERE ClassID = " + ClassID + @";
//                     IF @PrefixPV IS NULL OR @PrefixPV = '' SET @PrefixPV = 'PV';
//                     DECLARE @BasePref VARCHAR(20) = REPLACE(REPLACE(REPLACE(REPLACE(@PrefixPV, '{yy}', ''), '{MM}', ''), '{dd}', ''), '{000000}', '');
//                     SET @BasePref = REPLACE(REPLACE(@BasePref, '{00000}', ''), '-', '');
//                     IF @BasePref = '' SET @BasePref = 'PV';
//                     DECLARE @Pat VARCHAR(30) = @BasePref + '" + yyMM + @"-';
//                     DECLARE @MaxNo INT = 0;
//                     SELECT @MaxNo = ISNULL(MAX(TRY_CAST(SUBSTRING(PVNo, LEN(@Pat) + 1, 10) AS INT)), 0)
//                     FROM dbo.Expenses WITH (NOLOCK)
//                     WHERE PVNo LIKE @Pat + '%' AND (SystemID = " + html.SystemID + @" OR " + html.SystemID + @" = '0');
//                     SELECT @Pat + RIGHT('000000' + CAST(@MaxNo + 1 AS VARCHAR), 6) AS PVNo, @PrefixPV AS Prefix, @PrefixExpense PrefixExpense;
//                 ";
//                 DataSet dsCustom = html.LoadData(sqlCustom, false, null);
//                 if (dsCustom != null && dsCustom.Tables.Count > 0 && dsCustom.Tables[0].Rows.Count > 0)
//                 {
//                     PVNo = dsCustom.Tables[0].Rows[0][0].ToString();
//                     Prefix = dsCustom.Tables[0].Rows[0][1].ToString();
//                     PrefixExpense = dsCustom.Tables[0].Rows[0][2].ToString();
//                     return Json(new { PVNo = PVNo, Prefix = Prefix, PrefixExpense= PrefixExpense }, JsonRequestBehavior.AllowGet);
//                 }
//             }

//             string sql = @"declare @Prefix nvarchar(50)
// select @Prefix = PrefixPV from dbo.TblClass where ClassID = @ClassID
// exec [SP_SHOW_PREFIX] @Prefix, 'Expense', @SystemID, @WarehouseID, @DomainID;";
//             Dictionary<string, string> para = new Dictionary<string, string>
//             {
//                 { "ClassID", ClassID.ToString() },
//                 { "SystemID", html.SystemID },
//                 { "WarehouseID", html.WarehouseID },
//                 { "DomainID", html.DomainID }
//             };
//             DataSet ds = html.LoadData(sql, false, para);

//             if (ds.Tables[0].Rows.Count > 0)
//             {
//                 PVNo = ds.Tables[0].Rows[0][0].ToString();
//                 Prefix = ds.Tables[0].Rows[0][1].ToString();
//             }
//             return Json(new { PVNo = PVNo, Prefix = Prefix }, JsonRequestBehavior.AllowGet);
//         }

        public JsonResult GetPVNumber(int ClassID, string paymentDate = "", bool IsFromBills = false)
        {
            string PVNo = "", Prefix = "", PrefixExpense = "";
            HTML html = new HTML();
            if (!string.IsNullOrEmpty(paymentDate) && DateTime.TryParse(paymentDate, out DateTime pDate))
            {
                string yyMM = pDate.ToString("yyMM");
                string sqlCustom = @"
                    DECLARE @PrefixPV NVARCHAR(50) = '', @PrefixExpense NVARCHAR(50) = '', @ExpenseNo NVARCHAR(50) = '';
                    SELECT @PrefixPV = PrefixPV, @PrefixExpense = PrefixExpense FROM dbo.TblClass WHERE ClassID = " + ClassID + @";
                    IF @PrefixPV IS NULL OR @PrefixPV = '' SET @PrefixPV = 'PV';
                    DECLARE @BasePref VARCHAR(20) = REPLACE(REPLACE(REPLACE(REPLACE(@PrefixPV, '{yy}', ''), '{MM}', ''), '{dd}', ''), '{000000}', '');
                    SET @BasePref = REPLACE(REPLACE(@BasePref, '{00000}', ''), '-', '');
                    IF @BasePref = '' SET @BasePref = 'PV';
                    DECLARE @Pat VARCHAR(30) = @BasePref + '" + yyMM + @"-';
                    DECLARE @MaxNo INT = 0;
                    SELECT @MaxNo = ISNULL(MAX(TRY_CAST(SUBSTRING(PVNo, LEN(@Pat) + 1, 10) AS INT)), 0)
                    FROM dbo.Expenses WITH (NOLOCK)
                    WHERE PVNo LIKE @Pat + '%' AND (SystemID = " + html.SystemID + @" OR " + html.SystemID + @" = '0');
                    IF (" + (IsFromBills ? "1" : "0") + @" = 1)
                        EXEC [dbo].[SP_GENERATE_EXPENSE_PREFIX] 'BL{yy}{MM}-{00000}', 'Bill', " + html.SystemID + @", " + html.WarehouseID + @", @ExpenseNo OUT, '" + html.DomainID + @"', '" + pDate.ToString("yyyy-MM-dd") + @"';
                    ELSE
                        EXEC [dbo].[SP_GENERATE_EXPENSE_PREFIX] @PrefixExpense, 'ExpenseNo', " + html.SystemID + @", " + html.WarehouseID + @", @ExpenseNo OUT, '" + html.DomainID + @"', '" + pDate.ToString("yyyy-MM-dd") + @"';
                    SELECT @Pat + RIGHT('000000' + CAST(@MaxNo + 1 AS VARCHAR), 6) AS PVNo, @PrefixPV AS Prefix, @PrefixExpense PrefixExpense, @ExpenseNo AS ExpenseNo;
                ";
                DataSet dsCustom = html.LoadData(sqlCustom, false, null);
                if (dsCustom != null && dsCustom.Tables.Count > 0 && dsCustom.Tables[0].Rows.Count > 0)
                {
                    PVNo = dsCustom.Tables[0].Rows[0][0].ToString();
                    Prefix = dsCustom.Tables[0].Rows[0][1].ToString();
                    PrefixExpense = dsCustom.Tables[0].Rows[0][2].ToString();
                    string ExpenseNo = dsCustom.Tables[0].Rows[0]["ExpenseNo"].ToString();
                    return Json(new { PVNo = PVNo, Prefix = Prefix, PrefixExpense = PrefixExpense, ExpenseNo = ExpenseNo }, JsonRequestBehavior.AllowGet);
                }
            }

            string sql = @"declare @Prefix nvarchar(50)
select @Prefix = PrefixPV from dbo.TblClass where ClassID = @ClassID
exec [SP_SHOW_PREFIX] @Prefix, 'Expense', @SystemID, @WarehouseID, @DomainID;";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ClassID", ClassID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            DataSet ds = html.LoadData(sql, false, para);

            if (ds.Tables[0].Rows.Count > 0)
            {
                PVNo = ds.Tables[0].Rows[0][0].ToString();
                Prefix = ds.Tables[0].Rows[0][1].ToString();
            }
            return Json(new { PVNo = PVNo, Prefix = Prefix, ExpenseNo = "" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPostingNumber(int PaymentMethodID, int ClassID, string Type = "PrefixPosting")
        {
            string PostingNo = "", Prefix = "";
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = $@"declare @Prefix nvarchar(50)
select @Prefix = {Type} from dbo.TblPostingPrefix where PaymentMethodID = @PaymentMethodID and ClassID = @ClassID
exec [SP_SHOW_PREFIX] @Prefix, '{Type}', @SystemID, @WarehouseID, @DomainID;";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "PaymentMethodID", PaymentMethodID.ToString() },
                { "ClassID", ClassID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            DataSet ds = html.LoadData(sql, false, para);

            if (ds.Tables[0].Rows.Count > 0)
            {
                PostingNo = ds.Tables[0].Rows[0][0].ToString();
                Prefix = ds.Tables[0].Rows[0][1].ToString();
            }
            return Json(new { PostingNo = PostingNo, Prefix = Prefix }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowPaymentVoucher(int ExpensesID, bool ShowFullPage = false)
        {
            HTML html = new HTML();
            string sql = @"select FileLogo from dbo.TblClass c
inner join dbo.TblAccount_Expenses ae on ae.ClassNameID = c.ClassID
where ae.ExpensesID = " + ExpensesID;

            DataSet ds = html.LoadData(sql);
            string WarehouseLogo = html.ServerRoot + "Uploads/ClassLogo/" + ds.Tables[0].Rows[0]["FileLogo"].ToString();

            var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);

            string FilePath = httpContext.Server.MapPath("~/Templates/Expenses/index.html");

            StreamReader str = new StreamReader(FilePath);
            string Text = str.ReadToEnd();
            str.Close();

            sql = @"declare @ExpenseID int
select @ExpenseID = " + ExpensesID + @";

DECLARE @TblExpense TABLE(ID smallint Primary Key IDENTITY(1,1), AccountNo nvarchar(50), [Description] nvarchar(max), AccountNo1 nvarchar(50), 
QTY decimal(18,0), UnitPrice decimal(18,4), Amount decimal(18,2))

SELECT
    format(te.PaymentDate, 'dd-MMM-yyyy') PaymentDate, 
    CASE 
        WHEN te.PayeeType = 'Vendor' THEN v.VendorName COLLATE SQL_Latin1_General_CP1_CI_AS
        WHEN te.PayeeType = 'Customer' THEN c.CustomerName COLLATE SQL_Latin1_General_CP1_CI_AS
        ELSE e.FullName COLLATE SQL_Latin1_General_CP1_CI_AS 
    END AS PayeeName,
    ISNULL(CASE 
        WHEN te.PayeeType = 'Vendor' THEN v.CompanyName 
        WHEN te.PayeeType = 'Customer' THEN c.CompanyName 
        ELSE '' 
    END, '') AS CompanyName, 
    ISNULL(CASE 
        WHEN te.PayeeType = 'Vendor' THEN v.CompanyAddress 
        WHEN te.PayeeType = 'Customer' THEN c.CompanyAddress 
        ELSE '' 
    END, '') AS CompanyAddress, 
    ISNULL(v.AccountNo, '') AS AccountNo, 
    ISNULL(v.AccountName, '') AS AccountName,
	ISNULL(v.BankerName, '') AS BankerName,
	ISNULL(te.ATTName, '') AS ATTName,
    te.PVNo,
    ISNULL(te.PaymentTerms, '') PaymentTerms,
    format(te.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate,
    et.ExpenseType, tp.PaymentMethod, te.CheckNo, te.DraftNo, isnull(te.CheckStatus, 'Pending') CheckStatus, te.CheckedBy, te.ExpensesID,
    isnull(te.PostStatus, 'Pending') PostStatus
FROM [dbo].[TblAccount_Expenses] te
inner join dbo.TblAccount_ExpenseType et on et.AccountExpenseTypeID = te.AccountExpenseTypeID
inner join dbo.TblPaymentMethod tp on tp.PaymentMethodID = te.PaymentMethodID
OUTER APPLY (
    SELECT TOP 1 
        v1.VendorName, 
        v1.CompanyName, 
        v1.CompanyAddress, 
        v1.AccountNo, 
        v1.AccountName,
		v1.BankerName
    FROM dbo.TblVendor v1 
    WHERE v1.VendorID = te.PayeeID AND te.PayeeType = 'Vendor'
) AS v
OUTER APPLY (
    SELECT TOP 1 
        c.CustomerName, 
        bi.CompanyName, 
        bi.CompanyAddress
    FROM dbo.TblCustomer c
    INNER JOIN dbo.TblCustomerBillInformation bi 
        ON bi.CustomerID = c.CustomerID
    WHERE c.CustomerID = te.PayeeID AND te.PayeeType = 'Customer'
) AS c
OUTER APPLY (
    SELECT TOP 1 
        ep.FullName
    FROM HRMS_DB.dbo.Employees ep 
    WHERE ep.EmployeeID = te.PayeeID AND te.PayeeType = 'Employee'
) AS e

where te.ExpensesID = @ExpenseID

declare @TransactionType nvarchar(50), @PVNo nvarchar(50)

select @TransactionType = TransactionType, @PVNo = DraftNo from dbo.TblAccount_Expenses where ExpensesID = @ExpenseID

select 'Being expenses on '+@TransactionType+' for ' + @PVNo ExDescription

insert into @TblExpense (AccountNo, [Description], AccountNo1, QTY, UnitPrice, Amount)
select coa.AccountNo, ei.ExpenseDescription [Description],
coa1.AccountNo, ei.ExpenseQTY QTY, ei.ExpenseAmount UniPrice, ei.ExpenseTotal Amount
from TblAccount_ExpensesCategory ei
inner join dbo.TblAccount_Expenses ae on ae.ExpensesID = ei.ExpensesID
inner join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = ei.ExpensesAccountID
inner join dbo.TblChartOfAccount coa1 on coa1.ChartOfAccountID = ae.PaymentAccountID
where ei.ExpensesID = @ExpenseID
union all
select coa.AccountNo, ei.ItemName + case when ei.ItemDescription <> '' then '<br>' + ei.ItemDescription else '' end [Description],
coa1.AccountNo, ei.QTY, ei.UnitPrice UniPrice, ei.Total Amount
from TblAccount_ExpensesItem ei
inner join dbo.TblAccount_Expenses ae on ae.ExpensesID = ei.ExpensesID
inner join dbo.TblItem i on i.ItemID = ei.ItemID
inner join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = i.PurchaseChartOfAccountID
inner join dbo.TblChartOfAccount coa1 on coa1.ChartOfAccountID = ae.PaymentAccountID
where ei.ExpensesID = @ExpenseID

select * from @TblExpense

declare @SumAmount decimal(18, 2)

select @SumAmount = sum(Amount) from @TblExpense

select @SumAmount SumAmount, dbo.AmountInWords(@SumAmount) AmountInWords

select ae.CreateBy, format(ae.CreateDate, 'dd-MM-yyyy hh:mm tt') CreateDate, 

--case when isnull(ae.CheckStatus, '') = 'Checked' then ae.CheckedBy else '' end CheckedBy, 
ae.CheckedBy,
--case when isnull(ae.VerifiedStatus, '') = 'Verified' then ae.VerifiedBy else '' end VerifiedBy,
ae.VerifiedBy,
--case when isnull(ae.ApprovedStatus, '') = 'Approved' then ae.ApprovedBy else '' end ApprovedBy, 
ae.ApprovedBy,

case when isnull(ae.CheckStatus, '') = 'Checked' then ul.[Signature] else '' end CheckedSignature,
case when isnull(ae.VerifiedStatus, '') = 'Verified' then ul1.[Signature] else '' end VerifiedSignature,
case when isnull(ae.ApprovedStatus, '') = 'Approved' then ul2.[Signature] else '' end ApprovedSignature,
case when isnull(ul3.[Signature], '') <> '' then ul3.[Signature] else '' end CreateSignature,
case when isnull(ae.CheckStatus, '') = 'Checked' then format(ae.CheckedDate, 'dd-MM-yyyy hh:mm tt') else '..............................' end CheckedDate, 
case when isnull(ae.VerifiedStatus, '') = 'Verified' then format(ae.VerifiedDate, 'dd-MM-yyyy hh:mm tt') else '..............................' end VerifiedDate,
case when isnull(ae.ApprovedStatus, '') = 'Approved' then format(ae.ApprovedDate, 'dd-MM-yyyy hh:mm tt') else '..............................' end ApprovedDate
from dbo.TblAccount_Expenses ae 
inner join dbo.TblUserLogin ul on ul.UserLoginID = ae.CheckedByUserID
inner join dbo.TblUserLogin ul1 on ul1.UserLoginID = ae.VerifiedByUserID
inner join dbo.TblUserLogin ul2 on ul2.UserLoginID = ae.ApprovedByUserID
inner join dbo.TblUserLogin ul3 on ul3.UserLoginID = ae.UserID
where ae.ExpensesID = @ExpenseID

declare @TransactionID int

select @TransactionID = TransactionID from dbo.TblAccount_Transaction where ExpensesID = @ExpenseID

select ac.AccountNo AccountCode, ac.AccountName, ae.Debit, ae.Credit from dbo.TblAccount_JournalEntries ae
inner join dbo.TblChartOfAccount ac on ac.ChartOfAccountID = ae.AccountID
where TransactionID = @TransactionID

declare @Prefix nvarchar(50), @PrefixNumber nvarchar(50)

select @Prefix = c.PrefixPV from dbo.TblAccount_Expenses ae
inner join dbo.TblClass c on c.ClassID = ae.ClassNameID
where ae.ExpensesID = @ExpenseID


exec [SP_SHOW_PREFIX] @Prefix, 'ExpenseDraft', " + html.SystemID + ", " + html.WarehouseID + ", '" + html.DomainID + @"', 0, @PrefixNumber OUT, 1;

SELECT LEFT(@PrefixNumber, LEN(@PrefixNumber) - 1) AS PrefixNumber;";

            ds = html.LoadData(sql);
            DataRow drPayment = ds.Tables[0].Rows[0];

            Text = Text.Replace("{LOGO}", WarehouseLogo);
            Text = Text.Replace("{Header}", drPayment["ExpenseType"].ToString().Replace("Petty ", ""));
            Text = Text.Replace("{PayTo}", drPayment["PayeeName"].ToString());
            Text = Text.Replace("{Company}", drPayment["CompanyName"].ToString());
            Text = Text.Replace("{Address}", drPayment["CompanyAddress"].ToString());
            Text = Text.Replace("{ABAACC}", drPayment["AccountNo"].ToString());
            Text = Text.Replace("{ABAName}", drPayment["AccountName"].ToString());

            string CheckStatus = drPayment["CheckStatus"].ToString();
            string CheckedBy = drPayment["CheckedBy"].ToString();

            ViewBag.ExpensesID = drPayment["ExpensesID"].ToString();
            ViewBag.PostStatus = drPayment["PostStatus"].ToString();

            ViewBag.ShowBtnCheckApproval = false;
            ViewBag.ShowBtnPost = false;

            if (CheckedBy == html.UserFullName && CheckStatus == "Pending")
            {
                ViewBag.ShowBtnCheckApproval = true;
            }

            if (CheckStatus == "Checked" && ViewBag.PostStatus == "Pending")
            {
                ViewBag.ShowBtnPost = true;
            }

            ViewBag.PaymentMethod = drPayment["PaymentMethod"].ToString();
            ViewBag.CheckNo = drPayment["CheckNo"].ToString();

            ViewBag.PVNo = drPayment["DraftNo"].ToString();
            ViewBag.ExpenseType = drPayment["ExpenseType"].ToString();

            Text = Text.Replace("{Title}", drPayment["DraftNo"].ToString());
            Text = Text.Replace("{Chqno}", drPayment["CheckNo"].ToString());

            Text = Text.Replace("{Date}", drPayment["PaymentDate"].ToString());
            Text = Text.Replace("{Voucher}", ds.Tables[6].Rows[0]["PrefixNumber"].ToString());
            Text = Text.Replace("{Bank}", drPayment["BankerName"].ToString());
            Text = Text.Replace("{TTOn}", "..............................");
            Text = Text.Replace("{PaymentTerm}", drPayment["PaymentTerms"].ToString());
            Text = Text.Replace("{PaymentDueDate}", drPayment["PaymentDueDate"].ToString());

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<tr><td></td><td></td><td colspan=\"4\">" + ds.Tables[1].Rows[0]["ExDescription"].ToString() + "</td></tr>");

            foreach (DataRow row in ds.Tables[2].Rows)
            {
                sb.AppendLine($"<tr><td align=\"right\">{row["ID"].ToString()}</td><td>{row["AccountNo"].ToString()}</td><td>{row["Description"].ToString()}</td><td>{row["AccountNo1"].ToString()}</td><td align=\"right\">{row["QTY"].ToString()}</td><td align=\"right\">{decimal.Parse(row["UnitPrice"].ToString()).ToString("N4")}</td><td align=\"right\">{decimal.Parse(row["Amount"].ToString()).ToString("C2")}</td></tr>");
            }
            sb.AppendLine($"<tr><td colspan=\"6\" align=\"right\">Total</td><td align=\"right\">{decimal.Parse(ds.Tables[3].Rows[0]["SumAmount"].ToString()).ToString("C2")}</td></tr>");
            sb.AppendLine($"<tr><td colspan=\"7\"><b>Amount in Word (USD): {ds.Tables[3].Rows[0]["AmountInWords"].ToString()}</b></td></tr>");

            sb.AppendLine(@"<tr><td colspan=""7"">
<div class=""row"" style=""height:90px"">
<div class=""col-3"">Prepared by:</div><div class=""col-3"">Checked by:</div><div class=""col-3"">Verified by:</div><div class=""col-3"">Approved By:</div>
</div>
<div class=""row"">
<div class=""col-3"">");
            string[] Signature = ds.Tables[4].Rows[0]["CreateSignature"].ToString().Split('~');
            if (Signature[0].ToString() != "")
            {
                sb.AppendLine("<img id=\"Signature\" src = \"../Uploads/Users/" + Signature[0].ToString() + "\" height=\"90\" />");
            }

            sb.AppendLine(@"
</div>
<div class=""col-3"">");
            Signature = ds.Tables[4].Rows[0]["CheckedSignature"].ToString().Split('~');
            if (Signature[0].ToString() != "")
            {
                sb.AppendLine("<img id=\"Signature\" src = \"../Uploads/Users/" + Signature[0].ToString() + "\" height=\"90\" />");
            }
            sb.AppendLine(@"</div><div class=""col-3""></div><div class=""col-3""></div>
</div>");

            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine($"<div class=\"col-3\">{ds.Tables[4].Rows[0]["CreateBy"].ToString()}</div><div class=\"col-3\">{ds.Tables[4].Rows[0]["CheckedBy"].ToString()}</div><div class=\"col-3\">{ds.Tables[4].Rows[0]["VerifiedBy"].ToString()}</div><div class=\"col-3\">{ds.Tables[4].Rows[0]["ApprovedBy"].ToString()}</div>");
            sb.AppendLine($"</div><div class=\"row\"><div class=\"col-3\">Date: {ds.Tables[4].Rows[0]["CreateDate"].ToString()}</div><div class=\"col-3\">Date: {ds.Tables[4].Rows[0]["CheckedDate"].ToString()}</div><div class=\"col-3\">Date: {ds.Tables[4].Rows[0]["VerifiedDate"].ToString()}</div><div class=\"col-3\">Date: {ds.Tables[4].Rows[0]["ApprovedDate"].ToString()}</div>");
            sb.AppendLine("</div></td></tr>");

            sb.AppendLine(@"<tr>
<td colspan=""3"">
<div class=""row""><div class=""col"">Posted by</div></div>
<div class=""row""><div class=""col"">Name:............................................................</div></div>
</td>
<td colspan=""4"" align=""right"">
<table>
<thead><tr><th class=""text-nowrap"">Account Code</th><th class=""text-nowrap"">Account Name</th><th>Debit</th><th>Credit</th></tr></thead>
<tbody>");
            foreach (DataRow row in ds.Tables[5].Rows)
            {
                sb.AppendLine($"<tr><td>{row["AccountCode"].ToString()}</td><td>{row["AccountName"].ToString()}</td><td>{decimal.Parse(row["Debit"].ToString()).ToString("N2")}</td><td>{decimal.Parse(row["Credit"].ToString()).ToString("N2")}</td></tr>");
            }
            sb.AppendLine("</tbody></table></td></tr>");

            Text = Text.Replace("{TBody}", sb.ToString());
            if (!ShowFullPage)
            {
                var doc = new HtmlDocument
                {
                    OptionDefaultStreamEncoding = Encoding.UTF8
                };
                doc.LoadHtml(Text);
                HtmlNode node = doc.DocumentNode.SelectNodes("//div[@id='Div_Content']")[0];
                ViewBag.TMPData = node.OuterHtml;
            }
            else
            {
                return Content(Text, "text/html"); // Return full HTML
            }
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public async Task<JsonResult> SendNotificationToTelegram(int ExpensesID, string ExpenseType, string DraftNo, string CheckedByTelegramUserName, string UserFullName)
        {
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            var Text = ShowPaymentVoucher(ExpensesID, true) as ContentResult;

            sqlresult = await html.HTMLToPDF(Text.Content, $"Uploads/Expenses/{ExpenseType}", DraftNo);
            string OutPutFile = sqlresult.OutPutFile;
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
{
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Check ✍️", callbackData: "CheckExpense~"+ExpensesID.ToString()+"~"+ExpenseType+"~"+CheckedByTelegramUserName),
                    InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: "DisapproveExpense~"+ExpensesID.ToString()+"~"+ExpenseType+"~"+CheckedByTelegramUserName),
                }
            });
            long chatID = -1002999742690;
            int topicID = 1242;
            string msg = "";
            msg += CheckedByTelegramUserName + "<br><br>Dear <b>" + UserFullName + $"</b>,<br><br>Now you have new {ExpenseType} Need to Check that's created by " + html.UserFullName + ".";
            msg += "<br><br>Please check attachment file for detail.<br><br>Thank you and best regards,<br><br>";
            msg += $"BILLING SYSTEM AUTO GENERATED {ExpenseType}. #{DraftNo}";

            sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", chatID, topicID, inlineKeyboard, 0, "", OutPutFile, msg);

            string sql = "update dbo.TblAccount_Expenses set TelegramMessageID = @TelegramMessageID, TelegramMessage=@TelegramMessage where ExpensesID = @ExpensesID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "TelegramMessageID", sqlresult.Value },
                { "TelegramMessage", msg },
                { "ExpensesID", ExpensesID.ToString() },
            };

            sqlresult = html.SaveChangeData(sql, false, para);

            return Json(new { Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> Check(int ExpenseID, string ExpenseType, string Status, string UserName = "", string PaymentNo = "")
        {
            HTML html = new HTML();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            SqlResult sqlresult = new SqlResult();
            long chatID = -1002999742690;
            int topicID = 1242;

            string sql = "select TelegramMessageID, DraftNo, TelegramMessage from dbo.TblAccount_Expenses where ExpensesID = " + ExpenseID;
            int TelegramMessageID = int.Parse(html.LoadData(sql).Tables[0].Rows[0]["TelegramMessageID"].ToString());
            string DraftNo = html.LoadData(sql).Tables[0].Rows[0]["DraftNo"].ToString();
            string TelegramMessage = html.LoadData(sql).Tables[0].Rows[0]["TelegramMessage"].ToString();

            string msg = "Dear Team,<br><br>Now this Expense is Checked By " + (UserName != "" ? UserName : html.UserFullName) + ".<br><br>";
            if (Status == "Disapproved")
            {
                msg = "Dear Team,<br><br>Now this Expense is Disapproved By " + (UserName != "" ? UserName : html.UserFullName) + ".<br><br>Please revise this Expense.<br><br>";
            }
            //else
            //{

            sql = @"update dbo.TblAccount_Expenses set CheckStatus = '" + Status + "', CheckedDate = '" + html.GetCurrentDate() + "' where ExpensesID =" + ExpenseID;
            sqlresult = html.SaveChangeData(sql, false, false);

            var Text = ShowPaymentVoucher(ExpenseID, true) as ContentResult;

            sqlresult = await html.HTMLToPDF(Text.Content, $"Uploads/Expenses/{ExpenseType}", DraftNo);
            string OutPutFile = sqlresult.OutPutFile;
            //}
            msg += "Thank you and best regards,<br><br>";
            msg += "BILLING SYSTEM AUTO SEND Expense NOTIFICATION. #" + DraftNo;

            sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", chatID, topicID, null, TelegramMessageID, TelegramMessage, OutPutFile, msg, true);

            if (System.IO.File.Exists(OutPutFile))
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.IO.File.Delete(OutPutFile);
            }
            return Json(new { Data = ExpenseID, Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PostNow(int ExpenseID, string ExpenseType)
        {
            HTML html = new HTML();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            SqlResult sqlresult = new SqlResult();
            string sql = "update dbo.TblAccount_Expenses set PostStatus = 'Posted', PostDate = '" + html.GetCurrentDate() + "', PostBy='" + html.UserFullName + "', PostByUserID=" + html.UserID + " where ExpensesID=" + ExpenseID;
            sqlresult = html.SaveChangeData(sql, false, false);
            return Json(new { Data = ExpenseID, Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Advance Expenses, Edit Advance Expenses, Delete Advance Expenses")]
        public ActionResult LoadAdvanceExpenses()
        {
            HTML html = new HTML();
            //html.ReportTitle = "List Purchase Order";
            if (html.HasPermission("Add Advance Expenses"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Advance Expenses"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Advance Expenses"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New";
            html.AddURL = "Expenses/AddAdvanceExpenses";

            string sqlPayee = @"SELECT distinct a.ID, a.Code, a.[Name], a.[Type], a.OrderBy
FROM (
    SELECT 
        v.VendorID AS ID, 
        v.VendorCode AS Code, 
        v.VendorName COLLATE SQL_Latin1_General_CP1_CI_AS AS [Name], 
        'Vendor' AS [Type], 
        1 AS OrderBy
    FROM dbo.TblVendor v
    WHERE v.SystemID = " + html.SystemID + " AND v.WarehouseID = " + html.WarehouseID + @"

    UNION ALL

    SELECT 
        c.CustomerID AS ID, 
        c.CustomerCode AS Code, 
        c.CustomerName COLLATE SQL_Latin1_General_CP1_CI_AS AS [Name], 
        'Customer' AS [Type], 
        3 AS OrderBy
    FROM dbo.TblCustomer c
    WHERE ISNULL(c.CustomerCode, '') <> '' AND c.SystemID = " + html.SystemID + " AND c.WarehouseID = " + html.WarehouseID + @"

    UNION ALL

    SELECT 
        EmployeeID AS ID, 
        '' AS Code, 
        FullName COLLATE SQL_Latin1_General_CP1_CI_AS AS [Name], 
        'Employee' AS [Type], 
        2 AS OrderBy 
    FROM HRMS_DB.[dbo].[Employees]
) AS a
inner join dbo.TblAdvanceExpense ae on ae.VendorType = a.[Type] and ae.VendorID = a.ID
ORDER BY OrderBy, [Name] ASC;
";

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "ae.Date"
            });
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtVendorID",
                Label = "Vendor Name:",
                DBField = "VendorID",
                SQL_CBO = sqlPayee,
                DisplayText = "Name",
                Value = "ID",
                ComboBox = true,
                IsKendoMultiColumnComboBox = true,
                //KendoComboBoxColumn = new List<KendoComboBoxColumns>
                //            {
                //                //new KendoComboBoxColumns() { field = "Code", title = "Code", width = "120" },
                //                new KendoComboBoxColumns() { field = "Name", title = "Name", width = "550" },
                //                new KendoComboBoxColumns() { field = "Type", title = "Type", width = "120" }
                //            },
                //KendoMultiColumnComboBoxHeight = 350,
                //ConditionField = "VendorType"
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "ClassNameID",
                Label = "Class Name:",
                DBField = "ClassNameID",
                SQL_CBO = $"select distinct ClassNameID, ClassName from [dbo].[TblAdvanceExpense] where SystemID = {html.SystemID} and WarehouseID = {html.WarehouseID}",
                DisplayText = "ClassName",
                Value = "ClassNameID",
                ComboBox = true
            });
            //new HtmlComboBox()
            //{
            //    Label = "Vendor Name",
            //    ID = "VendorID",
            //    Required = true,
            //    Class = "input2",
            //    IsCustomForm = true,
            //    DBField = "VendorID",
            //    SQL_CBO = sqlPayee,
            //    Value = "ID",
            //    DisplayText = "Name",
            //    //GroupByField = "Type",
            //    IsKendoMultiColumnComboBox = true,
            //KendoComboBoxColumn = new List<KendoComboBoxColumns>
            //                {
            //                    //new KendoComboBoxColumns() { field = "Code", title = "Code", width = "120" },
            //                    new KendoComboBoxColumns() { field = "Name", title = "Name", width = "550" },
            //                    new KendoComboBoxColumns() { field = "Type", title = "Type", width = "120" }
            //                },
            //    KendoMultiColumnComboBoxHeight = 350,
            //    ConditionField = "VendorType"
            //},

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Adv No",
                SQL = "select top 10 AdvNo from dbo.TblAdvanceExpense where lower(AdvNo) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AdvNo",
                RadioValue = EncryAndDecry.Encryp("ae.AdvNo"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });

            html.TableName = "TblAdvanceExpense";
            //html.ShowSearchButton = false;
            html.ShowPaging = false;
            //             html.GH.SQL = @"select row_number() over (order by ae.AdvanceExpenseID desc) as [No], ae.AdvanceExpenseID, a.PayeeName, ae.PayeeType, 
            // b.VendorName, ae.VendorType, d.DepartmentName, c.ClassName, 
            // '<a href=""javascript:void(0);"" onclick=""ShowDialogBox2(''Preview Adv No.: '+ae.AdvNo+''', ''Auto'', ''500'', ''Expenses/ShowAdvExpense?AdvanceExpenseID='+convert(varchar, ae.AdvanceExpenseID)+''');"">' + ae.AdvNo + '</a>' AdvNo, ae.[Date] Dates, 
            // case when isnull(ae.ApprovedStatus, '') <> '' and isnull(ae.ApprovedStatus, '') <> 'Cancel' then '<span class=""badge bg-primary"">Approved</span>'
            // when isnull(ae.ApprovedStatus, '') = 'Cancel' then '<span class=""badge bg-success"">Cancel</span>'
            // else case when ae.CheckedBy = '" + html.UserFullName + @"' then case when isnull(ae.CheckStatus, '') = '' then '<a href=""javascript:void(0);"" onclick=""Check('+convert(varchar, ae.AdvanceExpenseID) +', ''Checked'');"" class=""btn btn-sm btn-primary text-white"">Check</a> <a href=""javascript:void(0);"" onclick=""Check('+convert(varchar, ae.AdvanceExpenseID) +', ''Disapproved'');"" class=""btn btn-sm btn-danger text-white"">Disapprove</a>' else 
            // case when ae.CheckStatus = 'Checked' then
            // 	case when ae.ApprovedStatus = 'Approved' then '<span class=""badge bg-primary"">' + ae.ApprovedStatus + '</span>' 
            // 	else 
            // 		case when ae.ApprovedBy = '" + html.UserFullName + @"' then 
            // 			case when isnull(ae.ApprovedStatus, '') = '' then '<a href=""javascript:void(0);"" onclick=""ApproveDisapprove('+convert(varchar, ae.AdvanceExpenseID) +', ''Approved'');"" class=""btn btn-sm btn-primary text-white"">Approve</a> <a href=""javascript:void(0);"" onclick=""ApproveDisapprove('+convert(varchar, ae.AdvanceExpenseID) +', ''Disapproved'');"" class=""btn btn-sm btn-danger text-white"">Disapprove</a>' else '<span class=""badge bg-danger"">' + isnull(ae.ApprovedStatus, 'Pending') + '</span>' end
            // 		else 
            // 		    '<span class=""badge bg-danger"">' + isnull(ae.ApprovedStatus, 'Pending') + '</span>'
            // 		end
            // 	end
            // else '<span class=""badge bg-danger"">' + ae.CheckStatus + '</span>' end
            // end 
            // else isnull(ae.CheckStatus, '<span class=""badge bg-danger"">Pending</span>') end end [Status], 
            // ae.TotalAmountBank, ae.TotalAmountCase, isnull(ae.CheckStatus, 'Pending') CheckStatus, isnull(ae.ApprovedStatus, 'Pending') ApprovedStatus
            // from dbo.TblAdvanceExpense ae
            // inner join dbo.TblClass c on c.ClassID = ae.ClassNameID
            // inner join dbo.TblDepartment d on d.DepartmentID = ae.DepartmentID
            // INNER JOIN (
            // SELECT 
            // 	te.AdvanceExpenseID,
            //     CASE 
            //         WHEN te.PayeeType = 'Vendor' THEN v.VendorName COLLATE SQL_Latin1_General_CP1_CI_AS
            //         WHEN te.PayeeType = 'Customer' THEN c.CustomerName COLLATE SQL_Latin1_General_CP1_CI_AS
            //         WHEN te.PayeeType = 'Employee' and exists(select 1 from dbo.TblUserLogin u where u.UserLoginID = te.PayeeID) THEN te.PayTo COLLATE SQL_Latin1_General_CP1_CI_AS
            //         ELSE e.FullName COLLATE SQL_Latin1_General_CP1_CI_AS 
            //     END AS PayeeName
            // FROM 
            //     dbo.TblAdvanceExpense te
            // OUTER APPLY (
            //     SELECT TOP 1 
            //         v1.VendorName
            //     FROM dbo.TblVendor v1 
            //     WHERE v1.VendorID = te.PayeeID AND te.PayeeType = 'Vendor'
            // ) AS v
            // OUTER APPLY (
            //     SELECT TOP 1 
            //         c.CustomerName
            //     FROM dbo.TblCustomer c
            //     INNER JOIN dbo.TblCustomerBillInformation bi 
            //         ON bi.CustomerID = c.CustomerID
            //     WHERE c.CustomerID = te.PayeeID AND te.PayeeType = 'Customer'
            // ) AS c
            // OUTER APPLY (
            //     SELECT TOP 1 
            //         ep.FullName
            //     FROM HRMS_DB.dbo.Employees ep 
            //     WHERE ep.EmployeeID = te.PayeeID AND te.PayeeType = 'Employee'
            // ) AS e
            // ) a 
            // ON a.AdvanceExpenseID = ae.AdvanceExpenseID
            // INNER JOIN (
            // SELECT 
            // 	te.AdvanceExpenseID,
            //     CASE 
            //         WHEN te.VendorType = 'Vendor' THEN v.VendorName COLLATE SQL_Latin1_General_CP1_CI_AS
            //         WHEN te.VendorType = 'Customer' THEN c.CustomerName COLLATE SQL_Latin1_General_CP1_CI_AS
            //         ELSE e.FullName COLLATE SQL_Latin1_General_CP1_CI_AS 
            //     END AS VendorName
            // FROM 
            //     dbo.TblAdvanceExpense te
            // OUTER APPLY (
            //     SELECT TOP 1 
            //         v1.VendorName
            //     FROM dbo.TblVendor v1 
            //     WHERE v1.VendorID = te.VendorID AND te.VendorType = 'Vendor'
            // ) AS v
            // OUTER APPLY (
            //     SELECT TOP 1 
            //         c.CustomerName
            //     FROM dbo.TblCustomer c
            //     INNER JOIN dbo.TblCustomerBillInformation bi 
            //         ON bi.CustomerID = c.CustomerID
            //     WHERE c.CustomerID = te.VendorID AND te.VendorType = 'Customer'
            // ) AS c
            // OUTER APPLY (
            //     SELECT TOP 1 
            //         ep.FullName
            //     FROM HRMS_DB.dbo.Employees ep 
            //     WHERE ep.EmployeeID = te.VendorID AND te.VendorType = 'Employee'
            // ) AS e
            // ) b 
            // ON b.AdvanceExpenseID = ae.AdvanceExpenseID
            //  where ae.SystemID = " + html.SystemID + " and ae.WarehouseID = " + html.WarehouseID;

            html.GH.SQL = @"select row_number() over (order by ae.AdvanceExpenseID desc) as [No], ae.AdvanceExpenseID, a.PayeeName, ae.PayeeType, 
b.VendorName, ae.VendorType, d.DepartmentName, c.ClassName, 
'<a href=""javascript:void(0);"" onclick=""ShowDialogBox2(''Preview Adv No.: '+ae.AdvNo+''', ''Auto'', ''500'', ''Expenses/ShowAdvExpense?AdvanceExpenseID='+convert(varchar, ae.AdvanceExpenseID)+''');"">' + ae.AdvNo + '</a>' AdvNo, ae.[Date] Dates, 
case when isnull(ae.ApprovedStatus, '') <> '' and isnull(ae.ApprovedStatus, '') <> 'Cancel' then '<span class=""badge bg-primary"">Approved</span>'
when isnull(ae.ApprovedStatus, '') = 'Cancel' then '<span class=""badge bg-success"">Cancel</span>'
else case when (isnull(ae.CheckedBy, '') = '' or ae.CheckedBy = '" + html.UserFullName + @"' or '" + html.UserID + @"' in ('38', '126')) then case when isnull(ae.CheckStatus, '') = '' then '<a href=""javascript:void(0);"" onclick=""Check('+convert(varchar, ae.AdvanceExpenseID) +', ''Checked'');"" class=""btn btn-sm btn-primary text-white"">Check</a> <a href=""javascript:void(0);"" onclick=""Check('+convert(varchar, ae.AdvanceExpenseID) +', ''Disapproved'');"" class=""btn btn-sm btn-danger text-white"">Disapprove</a>' else 
case when ae.CheckStatus = 'Checked' then
	case when ae.ApprovedStatus = 'Approved' then '<span class=""badge bg-primary"">' + ae.ApprovedStatus + '</span>' 
	else 
		case when (isnull(ae.ApprovedBy, '') = '' or ae.ApprovedBy = '" + html.UserFullName + @"' or '" + html.UserID + @"' in ('38', '126')) then 
			case when isnull(ae.ApprovedStatus, '') = '' then '<a href=""javascript:void(0);"" onclick=""ApproveDisapprove('+convert(varchar, ae.AdvanceExpenseID) +', ''Approved'');"" class=""btn btn-sm btn-primary text-white"">Approve</a> <a href=""javascript:void(0);"" onclick=""ApproveDisapprove('+convert(varchar, ae.AdvanceExpenseID) +', ''Disapproved'');"" class=""btn btn-sm btn-danger text-white"">Disapprove</a>' else '<span class=""badge bg-danger"">' + isnull(ae.ApprovedStatus, 'Pending') + '</span>' end
		else 
		    '<span class=""badge bg-danger"">' + isnull(ae.ApprovedStatus, 'Pending') + '</span>'
		end
	end
else '<span class=""badge bg-danger"">' + ae.CheckStatus + '</span>' end
end 
else isnull(ae.CheckStatus, '<span class=""badge bg-danger"">Pending</span>') end end [Status], 
ae.TotalAmountBank, ae.TotalAmountCase, isnull(ae.CheckStatus, 'Pending') CheckStatus, isnull(ae.ApprovedStatus, 'Pending') ApprovedStatus
from dbo.TblAdvanceExpense ae
inner join dbo.TblClass c on c.ClassID = ae.ClassNameID
inner join dbo.TblDepartment d on d.DepartmentID = ae.DepartmentID
INNER JOIN (
SELECT 
	te.AdvanceExpenseID,
    CASE 
        WHEN te.PayeeType = 'Vendor' THEN v.VendorName COLLATE SQL_Latin1_General_CP1_CI_AS
        WHEN te.PayeeType = 'Customer' THEN c.CustomerName COLLATE SQL_Latin1_General_CP1_CI_AS
        WHEN te.PayeeType = 'Employee' and exists(select 1 from dbo.TblUserLogin u where u.UserLoginID = te.PayeeID) THEN te.PayTo COLLATE SQL_Latin1_General_CP1_CI_AS
        ELSE e.FullName COLLATE SQL_Latin1_General_CP1_CI_AS 
    END AS PayeeName
FROM 
    dbo.TblAdvanceExpense te
OUTER APPLY (
    SELECT TOP 1 
        v1.VendorName
    FROM dbo.TblVendor v1 
    WHERE v1.VendorID = te.PayeeID AND te.PayeeType = 'Vendor'
) AS v
OUTER APPLY (
    SELECT TOP 1 
        c.CustomerName
    FROM dbo.TblCustomer c
    INNER JOIN dbo.TblCustomerBillInformation bi 
        ON bi.CustomerID = c.CustomerID
    WHERE c.CustomerID = te.PayeeID AND te.PayeeType = 'Customer'
) AS c
OUTER APPLY (
    SELECT TOP 1 
        ep.FullName
    FROM HRMS_DB.dbo.Employees ep 
    WHERE ep.EmployeeID = te.PayeeID AND te.PayeeType = 'Employee'
) AS e
) a 
ON a.AdvanceExpenseID = ae.AdvanceExpenseID
INNER JOIN (
SELECT 
	te.AdvanceExpenseID,
    CASE 
        WHEN te.VendorType = 'Vendor' THEN v.VendorName COLLATE SQL_Latin1_General_CP1_CI_AS
        WHEN te.VendorType = 'Customer' THEN c.CustomerName COLLATE SQL_Latin1_General_CP1_CI_AS
        ELSE e.FullName COLLATE SQL_Latin1_General_CP1_CI_AS 
    END AS VendorName
FROM 
    dbo.TblAdvanceExpense te
OUTER APPLY (
    SELECT TOP 1 
        v1.VendorName
    FROM dbo.TblVendor v1 
    WHERE v1.VendorID = te.VendorID AND te.VendorType = 'Vendor'
) AS v
OUTER APPLY (
    SELECT TOP 1 
        c.CustomerName
    FROM dbo.TblCustomer c
    INNER JOIN dbo.TblCustomerBillInformation bi 
        ON bi.CustomerID = c.CustomerID
    WHERE c.CustomerID = te.VendorID AND te.VendorType = 'Customer'
) AS c
OUTER APPLY (
    SELECT TOP 1 
        ep.FullName
    FROM HRMS_DB.dbo.Employees ep 
    WHERE ep.EmployeeID = te.VendorID AND te.VendorType = 'Employee'
) AS e
) b 
ON b.AdvanceExpenseID = ae.AdvanceExpenseID
 where ae.SystemID = " + html.SystemID + " and ae.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "ae.AdvanceExpenseID desc";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Dates", TextAlign = "center", FormatDateTime="dd-MM-yyyy" },
                new GridField() { Header = "Adv No", FieldName = "AdvNo", TextAlign="center" },
                new GridField() { Header = "Class", FieldName = "ClassName", TextAlign = "center" },
                new GridField() { Header = "Vendor Name", FieldName = "VendorName" },
                //new GridField() { Header = "Vendor Type", FieldName = "VendorType" },
                new GridField() { Header = "Pay To", FieldName = "PayeeName" },
                //new GridField() { Header = "Payee Type", FieldName = "PayeeType" },                
                new GridField() { Header = "Department", FieldName = "DepartmentName" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign = "center" },
                new GridField() { Header = "Total Amount Bank", FieldName = "TotalAmountBank", TextAlign = "right" },
                new GridField() { Header = "Total Amount Cash", FieldName = "TotalAmountCase", TextAlign = "right" },
                new GridField() { Header = "Action", FieldName = "AdvanceExpenseID", TextAlign = "Center",  IsModifyField = true,
                    DeleteWithTableName="TblAdvanceExpense, TblAdvanceExpenseDetail",
                    HideEditOnActionNameNotMatch = true,
                    ShowActionNameOnConditionField="ApprovedStatus",
                    ShowActionNameOnConditionValue="Disapproved, Pending"
                }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Advance Expenses, Edit Advance Expenses")]
        public ActionResult AddAdvanceExpenses(string PrimaryFieldValue, bool IsClone = false)
        {
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.IsClone = IsClone;
            HTML html = new HTML
            {
                PrimaryFieldValue = PrimaryFieldValue,
                isCloned = IsClone,
                PrimaryField = "AdvanceExpenseID",
                FormID = "FrmAddExpense"
            };
            string sqlPayee = @"SELECT a.ID, a.Code, a.[Name], a.[Type], a.OrderBy
FROM (
    SELECT v.VendorID AS ID, v.VendorCode AS Code, v.VendorName AS [Name], 'Vendor' AS [Type], 1 AS OrderBy
    FROM dbo.TblVendor v
    WHERE v.SystemID = " + html.SystemID + " AND v.WarehouseID = " + html.WarehouseID + @"

    UNION ALL

    SELECT c.CustomerID AS ID, c.CustomerCode AS Code, c.CustomerName AS [Name], 'Customer' AS [Type], 3 AS OrderBy
    FROM dbo.TblCustomer c
    WHERE ISNULL(c.CustomerCode, '') <> '' AND c.SystemID = " + html.SystemID + " AND c.WarehouseID = " + html.WarehouseID + @"

    UNION ALL
    
    select EmployeeID AS ID, '' AS Code, FullName AS [Name], 'Employee' AS [Type], 2 AS OrderBy from HRMS_DB.[dbo].[Employees]

    --SELECT DISTINCT u.UserLoginID AS ID, '' AS Code, u.FullName AS [Name], 'Employee' AS [Type], 2 AS OrderBy
    --FROM dbo.TblUserLogin u
    --WHERE u.SystemID = " + html.SystemID + " AND u.WarehouseID = " + html.WarehouseID + @"
) AS a
ORDER BY OrderBy, [Name] COLLATE SQL_Latin1_General_CP1_CI_AS ASC;
";
            sqlPayee = @"SELECT a.ID, a.Code, a.[Name], a.[Type], a.OrderBy
FROM (
    SELECT 
        v.VendorID AS ID, 
        v.VendorCode AS Code, 
        v.VendorName COLLATE SQL_Latin1_General_CP1_CI_AS AS [Name], 
        'Vendor' AS [Type], 
        1 AS OrderBy
    FROM dbo.TblVendor v
    WHERE v.SystemID = " + html.SystemID + " AND v.WarehouseID = " + html.WarehouseID + @"

    UNION ALL

    SELECT 
        c.CustomerID AS ID, 
        c.CustomerCode AS Code, 
        c.CustomerName COLLATE SQL_Latin1_General_CP1_CI_AS AS [Name], 
        'Customer' AS [Type], 
        3 AS OrderBy
    FROM dbo.TblCustomer c
    WHERE ISNULL(c.CustomerCode, '') <> '' AND c.SystemID = " + html.SystemID + " AND c.WarehouseID = " + html.WarehouseID + @"

    UNION ALL

    SELECT 
        EmployeeID AS ID, 
        '' AS Code, 
        FullName COLLATE SQL_Latin1_General_CP1_CI_AS AS [Name], 
        'Employee' AS [Type], 
        2 AS OrderBy 
    FROM HRMS_DB.[dbo].[Employees]
    UNION ALL
	SELECT DISTINCT
			UserLoginID as ID,
			'' as Code, 
			FullName COLLATE SQL_Latin1_General_CP1_CI_AS AS [Name],
			'Employee' AS [Type],
			2 AS OrderBy 
	FROM dbo.TblUserLogin 
) AS a
ORDER BY OrderBy, [Name] ASC;
";
            string ExpenseAccount = @"select distinct coa.ChartOfAccountID PaymentAccountID, ac.AccountCategory, act.AccountTypes, coa.AccountNo, coa.AccountName
from dbo.TblAccountTypes act
inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID
inner join dbo.TblChartOfAccount coa on coa.AccountType = act.AccountTypesID
where ac.AccountCategoryID = 1 and act.AccountTypesID not in(3) and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by ac.AccountCategory, act.AccountTypes";
            string sql = @"select * from dbo.TblAccount_Expenses ae
inner join dbo.TblAccount_ExpensesCategory aec on aec.ExpensesID = ae.ExpensesID
inner join dbo.TblAccount_ExpensesItem aet on aet.ExpensesID = ae.ExpensesID
where ae.ExpensesID=" + PrimaryFieldValue;

            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblAdvanceExpense",
                    FormTitle="Advance Expense",
                    GetIDENTITYByFiled="AdvanceExpenseID",
                    PrimaryFieldName="AdvanceExpenseID",
                    //ISAddAttachmentFile = true,
                    //DestinationFile = "Uploads/Expenses",
                    //SQLEdit="select * from dbo.TblAccount_Expenses where ExpensesID="+ PrimaryFieldValue,
                    SQL=@"update dbo.TblAdvanceExpense set CheckStatus = NULL, CheckedDate = NULL, ApprovedDate = NULL, ApprovedStatus = NULL, CheckedBy = NULL, ApprovedBy = NULL
where AdvanceExpenseID = @AdvExpID",
                    UpdateFiledName="AdvExpID",
                    htmlControl = new HTMLControl[]
                    {
                         new HtmlTextBox()
                        {
                            Label="AdvExpID", ID="AdvExpID", Required=true,
                            Value=PrimaryFieldValue.ToString(), IsReadOnly = true, Hide = true
                        },
                        new HtmlComboBox()
                        {
                            Label="Vendor Name", ID="VendorID",
                            //Required=true,
                            Class="input2",
                            IsCustomForm = true,
                            DBField="VendorID",
                            SQL_CBO= sqlPayee,
                            Value = "ID",
                            DisplayText = "Name",
                            //GroupByField = "Type",
                            IsKendoMultiColumnComboBox = true,
                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                            {
                                //new KendoComboBoxColumns() { field = "Code", title = "Code", width = "120" },
                                new KendoComboBoxColumns() { field = "Name", title = "Name", width = "550" },
                                new KendoComboBoxColumns() { field = "Type", title = "Type", width = "120" }
                            },
                            KendoMultiColumnComboBoxHeight = 350,
                            ConditionField="VendorType"
                        },
                        new HtmlComboBox()
                        {
                            Label="Pay To", ID="PayeeID",
                            Required=true,
                            Class="input2",
                            IsCustomForm = true,
                            DBField="PayeeID",
                            SQL_CBO= sqlPayee,
                            Value = "ID",
                            DisplayText = "Name",
                            //GroupByField = "Type",
                            IsKendoMultiColumnComboBox = true,
                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                            {
                                //new KendoComboBoxColumns() { field = "Code", title = "Code", width = "120" },
                                new KendoComboBoxColumns() { field = "Name", title = "Name", width = "550" },
                                new KendoComboBoxColumns() { field = "Type", title = "Type", width = "120" }
                            },
                            KendoMultiColumnComboBoxHeight = 350,
                            ConditionField="PayeeType"
                        },
                        new HtmlTextBox()
                        {
                            Label="Under Operation:", ID="UnderOperation",
                            Class="input2", IsCustomForm = true,
                            DBField="UnderOperation",
                            Required = true,
                            isTypeahead = true,
                            SQL="select DepartmentID, DepartmentName UnderOperation from dbo.TblDepartment where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' order by DepartmentName",
                            HiddenField ="DepartmentID",
                            AddValueToTblNameIfNotExistingData="TblDepartment",
                            AddValueToTblNameIfNotExistingDataFieldName="DepartmentName",
                            //DivClassCol="col-sm-auto",
                            AddButtonFunction="AddNewDepartment()",
                            TypeAheadAddNewFunctionName="AddNewDepartment()"
                        },
                        new HtmlTextBox()
                        {
                            Label="Date:", ID="PaymentDate",
                            Class="input2", IsCustomForm = true,
                            SingleDatePicker=true,
                            DBField="Date",
                            Required = true,
                        },
                        //new HtmlComboBox()
                        //{
                        //    Label="Class Name:", ID="ClassName",
                        //    Required=true,
                        //    Class="input2",
                        //    IsCustomForm = true,
                        //    DBField="ClassNameID",
                        //    SQL_CBO= "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID ="+html.WarehouseID + " and DomainID like '%"+html.DomainID+"%'",
                        //    Value = "ClassID",
                        //    DisplayText = "ClassName",
                        //    IsKendoMultiColumnComboBox = true,
                        //    CallBackFunction="ClassChange()"
                        //},
                        new HtmlTextBox()
                        {
                            Label="Class Name", ID="ClassName", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ClassName", isTypeahead = true,
                            SQL="select ClassID as ClassNameID, ClassName, AdvanceExpenseNo as PrefixAdvExpense from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            HiddenField="ClassNameID",
                            SQLCallBack="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = AdvanceExpenseNo, @Module = 'AdvExpenseNo' from dbo.TblClass where ClassID = @ClassNameID;	exec [SP_SHOW_PREFIX] @Prefix, @Module, "+html.SystemID+", "+html.WarehouseID+", '"+html.DomainID+"';",
                            CallBackToControlID="AdvNo, PrefixAdvExpense", UpdatePrefixModule="AdvExpenseNo", Prefix="PrefixAdvExpense", SetPrefixToDbFieldName="AdvNo"
                        },
                    },
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Adv No", ID="AdvNo", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="AdvNo", IsReadOnly = true,
                            isCloned = IsClone,
                            //SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'AdvExpenseNo'; select @Prefix = Prefix from dbo.TblPrefix where Module = 'AdvNo' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'NSMVAT-{yy}{MM}-{0000}'",
                            //CallBackToControlID="AdvNo, PrefixAdvNo", UpdatePrefixModule="AdvExpenseNo", Prefix="PrefixAdvNo", SetPrefixToDbFieldName="AdvNo"
                        },
                        new HtmlTextBox()
                        {
                            Label="Checked By", ID="CheckedBy", //Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CheckedBy", isTypeahead = true,
                            //SQL="select distinct isnull(RequestBy, '') RequestBy from dbo.TblPurchaseOrder \r\nwhere isnull(RequestBy, '') <> '' and SystemID = "+html.SystemID+" and WarehouseID = "+ html.WarehouseID + " order by isnull(RequestBy, '')",
                            SQL="select u.UserLoginID CheckedByUserID, u.TelegramUserName CheckedByTelegramUserName, u.FullName CheckedBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="CheckedByTelegramUserName, CheckedByUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Verified By", ID="VerifiedBy",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="VerifiedBy", isTypeahead = true,
                            SQL="select u.UserLoginID VerifiedByUserID, u.TelegramUserName VerifiedByTelegramUserName, u.FullName VerifiedBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="VerifiedByTelegramUserName, VerifiedByUserID",
                            HiddenFieldValue="0,0", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Approved By", ID="ApprovedBy",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ApprovedBy", isTypeahead = true,
                            SQL="select u.UserLoginID ApprovedByUserID, u.TelegramUserName ApprovedByTelegramUserName, u.FullName ApprovedBy, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="ApprovedByTelegramUserName, ApprovedByUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto",
                            Required = true,
                        },
                    }
                },
                new  FormData() {
                    FormTitle="Item Details",
                    TableName="TblAdvanceExpenseDetail",
                    GetIDENTITYByFiled="AdvanceExpenseID",
                    PrimaryFieldName="AdvanceExpenseID",
                    ShowBtnExpans=true,
                    AutoHideCollapse=false,
                    //SQLEdit="select * from dbo.TblAccount_ExpensesItem where ExpensesID="+ PrimaryFieldValue,
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ShowBalance=false,
                            ShowDeposit=false,
                            ShowBankInfo=false,
                            ShowRemark=false,
                            ShowCustomerNotes=false,
                            ShowTermsConditions=false,
                            SubTotalColspan=1,
                            ShowTotal = false,
                            ShowTotalOnly=true,

                            ShowFooter=false,

                            GetIDENTITYByFiled="AdvanceExpenseID",
                            PrimaryFieldName="AdvanceExpenseID",
                            TableName="TblAdvanceExpenseDetail",
                            HtmlTblID="TblExpensesItem",
                            InputName="ItemName",
                            TRName="TR_No",
                            TDName="TD_No",
                            ApplyTblTHBackground=false,
                            AutoTotalByRow=true,
                            TotalByInputName="ItemName",
                            //TotalByInputNameQTY="QTY",
                            //TotalByInputNameAmount="UnitPrice",
                            TotalDisplayOnInputName="AmountCase",
                            //TotalByInputNameDiscountUSD="DisUSD",
                            //ScriptCallBackFunction="Reload_UM(), CallSetAutoComplete_ItemName()",
                            //dbField=new DBField[]
                            //{
                            //    new DBField()
                            //    {
                            //        DBFieldName="ItemID", Value="ItemID", GetValueFromInputName = true
                            //    },
                            //    new DBField()
                            //    {
                            //        DBFieldName="ItemDescription", Value="ItemName_Description", GetValueFromTextareaName = true,
                            //    },
                            //    new DBField()
                            //    {
                            //        DBFieldName="UomID", Value="UomID", GetValueFromInputName = true
                            //    },
                            //},
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>", Label="#"
                                },
                                new TblTHead()
                                {
                                     Class="col-5", Label="Description"
                                },
                                new TblTHead()
                                {
                                     Label="Amount-Bank",
                                },
                                new TblTHead()
                                {
                                     Label="Amount-Cash",
                                     CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TR_No\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TD_No"
                                        },
                                        new HtmlTextarea()
                                        {
                                            Name="ItemName",
                                            DBField = "Description",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TR_No', 'TD_No', this,'','ItemName');",
                                            placeholder="Description",
                                            RequiredInputIfOtherFieldIsNotNull = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="AmountBank", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="AmountBank", Value = "0", IsAllowInputOnlyNumber =true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="AmountCase", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="AmountCase", Value = "0", IsAllowInputOnlyNumber =true,
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },
            };
            html.SaveChangeFunction = "SaveChangeAdvanceExpense()";
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        public string GetAdvNumber(int ClassID)
        {
            string PVNo = "";
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = @"declare @Prefix nvarchar(50)
select @Prefix = AdvanceExpenseNo from dbo.TblClass where ClassID = @ClassID
exec [SP_SHOW_PREFIX] @Prefix, 'AdvExpenseNo', @SystemID, @WarehouseID, @DomainID;";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ClassID", ClassID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            DataSet ds = html.LoadData(sql, false, para);

            if (ds.Tables[0].Rows.Count > 0)
            {
                PVNo = ds.Tables[0].Rows[0][0].ToString();
            }
            return PVNo;
        }

        [Authorize(Roles = "Add Advance Expenses, Edit Advance Expenses")]
        [HttpPost, ValidateInput(false)]
        public JsonResult SaveChangeAdvanceExpense(dynamic DATA)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string DestinationFolder = "/Uploads/Expenses/";//$"{html.ToUrlSlug($"{CustomerName + " [" + CustomerCode + "]"}")}";
            SqlResult result = new SqlResult();
            result = html.UploadFile(DestinationFolder);
            string AttachmentFile;
            if (result.Result == ResultType.Successful)
                AttachmentFile = result.Value;
            else
            {
                return Json(new
                {
                    Message = result.Message,
                    Result = result.Result,
                }, JsonRequestBehavior.AllowGet);
            }
            dynamic DataInfos = new JavaScriptSerializer().DeserializeObject(DATA[0].ToString());
            dynamic AdvExpenseData = DataInfos["AdvExpenseData"];
            dynamic AdvExpenseDataDetail = DataInfos["AdvExpenseDataDetail"];
            StringBuilder sb = new StringBuilder();

            string AdvNo = AdvExpenseData[0]["AdvNo"].ToString();
            sb.AppendLine("<Data>");

            foreach (dynamic item in AdvExpenseData)
            {
                sb.AppendLine("<AdvExpenseData>");
                sb.AppendLine($"<AdvanceExpenseID>{item["AdvanceExpenseID"]}</AdvanceExpenseID>");
                sb.AppendLine($"<IsClone>{(item["IsClone"] == "True" ? 1 : 0)}</IsClone>");
                sb.AppendLine($"<PayTo>{item["PayeeName"]}</PayTo>");
                sb.AppendLine($"<PayeeType>{item["PayeeType"]}</PayeeType>");
                sb.AppendLine($"<PayeeID>{item["PayeeID"]}</PayeeID>");
                sb.AppendLine($"<VendorName>{item["VendorName"]}</VendorName>");
                sb.AppendLine($"<VendorType>{item["VendorType"]}</VendorType>");
                sb.AppendLine($"<VendorID>{item["VendorID"]}</VendorID>");
                sb.AppendLine($"<DepartmentID>{item["DepartmentID"]}</DepartmentID>");
                sb.AppendLine($"<UnderOperation>{item["UnderOperation"]}</UnderOperation>");
                sb.AppendLine($"<ClassNameID>{item["ClassNameID"]}</ClassNameID>");
                sb.AppendLine($"<ClassName>{item["ClassName"]}</ClassName>");
                sb.AppendLine($"<AdvNo>{item["AdvNo"]}</AdvNo>");
                sb.AppendLine($"<Date>{item["AdvDate"]}</Date>");
                sb.AppendLine($"<PreparedBy>{html.UserFullName}</PreparedBy>");
                sb.AppendLine($"<PreparedID>{html.UserID}</PreparedID>");
                sb.AppendLine($"<CheckedByUserID>{item["CheckedByUserID"]}</CheckedByUserID>");
                sb.AppendLine($"<CheckedBy>{item["CheckedBy"]}</CheckedBy>");
                sb.AppendLine($"<CheckedByTelegramUserName>{item["CheckedByTelegramUserName"]}</CheckedByTelegramUserName>");
                sb.AppendLine($"<ApprovedByUserID>{item["ApprovedByUserID"]}</ApprovedByUserID>");
                sb.AppendLine($"<ApprovedBy>{item["ApprovedBy"]}</ApprovedBy>");
                sb.AppendLine($"<ApprovedByTelegramUserName>{item["ApprovedByTelegramUserName"]}</ApprovedByTelegramUserName>");
                sb.AppendLine($"<SystemID>{html.SystemID}</SystemID>");
                sb.AppendLine($"<WarehouseID>{html.WarehouseID}</WarehouseID>");
                sb.AppendLine($"<DomainID>{html.DomainID}</DomainID>");

                sb.AppendLine("</AdvExpenseData>");
            }

            foreach (dynamic item in AdvExpenseDataDetail)
            {
                sb.AppendLine("<AdvExpenseDataDetail>");
                sb.AppendLine($"<Description>{System.Security.SecurityElement.Escape(item["Description"].ToString())}</Description>");
                sb.AppendLine($"<AmountBank>{item["AmountBank"]}</AmountBank>");
                sb.AppendLine($"<AmountCase>{item["AmountCase"]}</AmountCase>");
                sb.AppendLine("</AdvExpenseDataDetail>");
            }

            sb.AppendLine("</Data>");

            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "Data", "N'"+sb.ToString()+"'" },
            };

            sqlResult = html.SaveChangeData("SP_INSERT_ADV_EXPENSES", true, Para, true, true);

            return Json(new { Data = sqlResult.Data.Tables[0].Rows[0].ToJsonString(), Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowAdvExpense(int AdvanceExpenseID, bool ShowFullPage = false)
        {
            HTML html = new HTML();
            string sql = @"select row_number() over (order by ae.AdvNo desc) as [No], ae.AdvanceExpenseID, a.PayeeName, ae.PayeeType, d.DepartmentName, c.ClassName, ae.AdvNo, format(ae.[Date], 'dd-MM-yyyy') [Date], 
case when ae.CheckedBy = '' then case when isnull(ae.CheckStatus, '') = '' then '<a href=""javascript:void(0);"" onclick=""Check('+convert(varchar, ae.AdvanceExpenseID) +', ''Checked'');"" class=""btn btn-sm btn-primary text-white"">Check</a> <a href=""javascript:void(0);"" onclick=""Check('+convert(varchar, ae.AdvanceExpenseID) +', ''Disapproved'');"" class=""btn btn-sm btn-danger text-white"">Disapprove</a>' else 
case when ae.CheckStatus = 'Checked' then '<span class=""badge bg-primary"">' + ae.CheckStatus + '</span>' 
else '<span class=""badge bg-danger"">' + ae.CheckStatus + '</span>' end
end 
else isnull(ae.CheckStatus, '<span class=""badge bg-danger"">Pending</span>') end
[Status], ae.TotalAmountBank, ae.TotalAmountCase, c.FileLogo, ul.[Signature] PreparedBySignature, ae.PreparedBy,
ae.CheckedBy, format(ae.CheckedDate, 'dd-MM-yyyy hh:mm:ss tt') CheckedDate,
case when isnull(ae.CheckStatus, '') = 'Checked' then uc.[Signature] else '' end CheckedBySignature,
ae.ApprovedBy, format(ae.ApprovedDate, 'dd-MM-yyyy hh:mm:ss tt') ApprovedDate, 
case when isnull(ae.ApprovedStatus, '') = 'Approved' then ua.[Signature] else '' end ApprovedSignature,
b.VendorName, format(ae.CreateDate, 'dd-MM-yyyy hh:mm:ss tt') CreateDate, ae.SystemID, ae.ClassNameID, ae.CheckStatus, ae.ApprovedStatus
from dbo.TblAdvanceExpense ae
inner join dbo.TblClass c on c.ClassID = ae.ClassNameID
inner join dbo.TblDepartment d on d.DepartmentID = ae.DepartmentID
inner join dbo.TblUserLogin ul on ul.UserLoginID = ae.PreparedID
left join dbo.TblUserLogin uc on uc.UserLoginID = ae.CheckedByUserID
inner join dbo.TblUserLogin ua on ua.UserLoginID = ae.ApprovedByUserID
INNER JOIN (
SELECT 
	te.AdvanceExpenseID,
    CASE 
        WHEN te.PayeeType = 'Vendor' THEN v.VendorName COLLATE SQL_Latin1_General_CP1_CI_AS
        WHEN te.PayeeType = 'Customer' THEN c.CustomerName COLLATE SQL_Latin1_General_CP1_CI_AS
        when te.PayeeType = 'Employee' and exists(select 1 from dbo.TblUserLogin u where te.PayeeID = u.UserLoginID) THEN te.PayTo COLLATE SQL_Latin1_General_CP1_CI_AS
        ELSE e.FullName COLLATE SQL_Latin1_General_CP1_CI_AS 
    END AS PayeeName
FROM 
    dbo.TblAdvanceExpense te
OUTER APPLY (
    SELECT TOP 1 
        v1.VendorName
    FROM dbo.TblVendor v1 
    WHERE v1.VendorID = te.PayeeID AND te.PayeeType = 'Vendor'
) AS v
OUTER APPLY (
    SELECT TOP 1 
        c.CustomerName
    FROM dbo.TblCustomer c
    INNER JOIN dbo.TblCustomerBillInformation bi 
        ON bi.CustomerID = c.CustomerID
    WHERE c.CustomerID = te.PayeeID AND te.PayeeType = 'Customer'
) AS c
OUTER APPLY (
    SELECT TOP 1 
        ep.FullName
    FROM HRMS_DB.dbo.Employees ep 
    WHERE ep.EmployeeID = te.PayeeID AND te.PayeeType = 'Employee'
) AS e
) a 
ON a.AdvanceExpenseID = ae.AdvanceExpenseID
INNER JOIN (
SELECT 
	te.AdvanceExpenseID,
    CASE 
        WHEN te.VendorType = 'Vendor' THEN v.VendorName COLLATE SQL_Latin1_General_CP1_CI_AS
        WHEN te.VendorType = 'Customer' THEN c.CustomerName COLLATE SQL_Latin1_General_CP1_CI_AS
        ELSE e.FullName COLLATE SQL_Latin1_General_CP1_CI_AS 
    END AS VendorName
FROM 
    dbo.TblAdvanceExpense te
OUTER APPLY (
    SELECT TOP 1 
        v1.VendorName
    FROM dbo.TblVendor v1 
    WHERE v1.VendorID = te.VendorID AND te.VendorType = 'Vendor'
) AS v
OUTER APPLY (
    SELECT TOP 1 
        c.CustomerName
    FROM dbo.TblCustomer c
    INNER JOIN dbo.TblCustomerBillInformation bi 
        ON bi.CustomerID = c.CustomerID
    WHERE c.CustomerID = te.VendorID AND te.VendorType = 'Customer'
) AS c
OUTER APPLY (
    SELECT TOP 1 
        ep.FullName
    FROM HRMS_DB.dbo.Employees ep 
    WHERE ep.EmployeeID = te.VendorID AND te.VendorType = 'Employee'
) AS e
) b 
ON b.AdvanceExpenseID = ae.AdvanceExpenseID
where ae.AdvanceExpenseID = @AdvanceExpenseID

select row_number() over (order by AdvanceExpenseDetailID) as [No], 
[Description], AmountBank, AmountCase from dbo.TblAdvanceExpenseDetail
where AdvanceExpenseID = @AdvanceExpenseID";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "AdvanceExpenseID", AdvanceExpenseID.ToString() }
            };

            DataSet ds = html.LoadData(sql, false, para);
            string WarehouseLogo = html.ServerRoot + "Uploads/ClassLogo/" + ds.Tables[0].Rows[0]["FileLogo"].ToString();

            var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);

            string FilePath = httpContext.Server.MapPath("~/Templates/AdvExpenses/index.html");

            StreamReader str = new StreamReader(FilePath);
            string Text = str.ReadToEnd();
            str.Close();
            DataRow drPayment = ds.Tables[0].Rows[0];

            string approvedStatus = drPayment["ApprovedStatus"].ToString();
            int advID = int.Parse(drPayment["AdvanceExpenseID"].ToString());
            ViewBag.AdvStatus = approvedStatus;
            ViewBag.AdvExpenseID = advID;

            //Text = Text.Replace("{LOGO}", WarehouseLogo);
            Text = Text.Replace("{Header}", "Advance Expense");
            Text = Text.Replace("{Title}", "Advance Expense_" + drPayment["AdvNo"].ToString());
            Text = Text.Replace("{VendorName}", drPayment["VendorName"]?.ToString() ?? "");
            Text = Text.Replace("{PayTo}", drPayment["PayeeName"].ToString());
            Text = Text.Replace("{DepartmentName}", drPayment["DepartmentName"].ToString());
            Text = Text.Replace("{Date}", drPayment["Date"].ToString());
            Text = Text.Replace("{AdvNo}", drPayment["AdvNo"].ToString());
            Text = Text.Replace("{PreparedBy}", drPayment["PreparedBy"].ToString());
            Text = Text.Replace("{PreparedDate}", drPayment["CreateDate"].ToString());

            if (!String.IsNullOrEmpty(drPayment["CheckStatus"].ToString()) && drPayment["CheckStatus"].ToString() == "Checked" && drPayment["ApprovedStatus"].ToString() != "Cancel")
            {
                Text = Text.Replace("{CheckedBy}", drPayment["CheckedBy"].ToString());
                Text = Text.Replace("{CheckedDate}", drPayment["CheckedDate"].ToString());
            }
            else
            {
                Text = Text.Replace("{CheckedBy}", "");
                Text = Text.Replace("{CheckedDate}", "");
            }

            if (!String.IsNullOrEmpty(drPayment["ApprovedStatus"].ToString()) && drPayment["ApprovedStatus"].ToString() == "Approved")
            {
                Text = Text.Replace("{ApprovedBy}", drPayment["ApprovedBy"].ToString());
                Text = Text.Replace("{ApprovedDate}", drPayment["ApprovedDate"].ToString());
            }
            else
            {
                Text = Text.Replace("{ApprovedBy}", "");
                Text = Text.Replace("{ApprovedDate}", "");
            }

            int systemID = int.Parse(drPayment["SystemID"].ToString());
            int classID = int.Parse(drPayment["ClassNameID"].ToString());
            string theadColorStyle = "";
            switch (systemID)
            {
                case 13: // NSM&NSMVAT
                    theadColorStyle = "background-color: #f89c39; color: #fff;";
                    break;
                case 25: // BSM
                    theadColorStyle = "background-color: #c5d9f1; color: #000;";
                    break;
                case 27: // BS
                    if (classID == 11) // BS 
                        theadColorStyle = "background-color: #ffde16; color: #000;";
                    else if (classID == 12) // BS Coltd
                        theadColorStyle = "background-color: #ffccff; color: #000;";
                    break;
                case 28: // NSMTech
                    theadColorStyle = "background-color: #2275B9; color: #fff;";
                    break;
                default:
                    theadColorStyle = "background-color: #c7c7c7ff; color: #fff;";
                    break;
            }

            Text = Text.Replace("{theadColorStyle}", $"style=\"{theadColorStyle}\"");

            ViewBag.AdvNo = drPayment["AdvNo"].ToString();

            if (drPayment["PreparedBySignature"].ToString() != "")
            {
                string[] PreparedBySignature = drPayment["PreparedBySignature"].ToString().Split('~');
                Text = Text.Replace("{PreparedBySignature}", "<img id=\"Signature\" src = \"../Uploads/Users/" + PreparedBySignature[0] + "\" height=\"90\" />");
            }
            else
                Text = Text.Replace("{PreparedBySignature}", "");

            if (drPayment["CheckedBySignature"].ToString() != "" && drPayment["ApprovedStatus"].ToString() != "Cancel")
            {
                string[] CheckedBySignature = drPayment["CheckedBySignature"].ToString().Split('~');
                Text = Text.Replace("{CheckedBySignature}", "<img id=\"Signature\" src = \"../Uploads/Users/" + CheckedBySignature[0] + "\" height=\"90\" />");
            }
            else
                Text = Text.Replace("{CheckedBySignature}", "");

            if (drPayment["ApprovedSignature"].ToString() != "")
            {
                string[] ApprovedSignature = drPayment["ApprovedSignature"].ToString().Split('~');
                Text = Text.Replace("{ApprovedSignature}", "<img id=\"Signature\" src = \"../Uploads/Users/" + ApprovedSignature[0] + "\" height=\"90\" />");
            }
            else
                Text = Text.Replace("{ApprovedSignature}", "");

            StringBuilder sb = new StringBuilder();

            foreach (DataRow row in ds.Tables[1].Rows)
            {
                string description = row["Description"].ToString()
                    .Replace("&#013;&#010;", "<br/>")
                    .Replace("\r\n", "<br/>")
                    .Replace("\n", "<br/>")
                    .Replace("\r", "<br/>");
                sb.AppendLine($"<tr><td align=\"right\">{row["No"].ToString()}</td><td class=\"text-nowrap-pre\">{description}</td><td align=\"right\">{decimal.Parse(row["AmountBank"].ToString()).ToString("C2")}</td><td align=\"right\">{decimal.Parse(row["AmountCase"].ToString()).ToString("C2")}</td></tr>");
            }
            sb.AppendLine($"<tr style=\"font-weight:bold\"><td></td><td align=\"right\">Total</td><td align=\"right\">{decimal.Parse(ds.Tables[0].Rows[0]["TotalAmountBank"].ToString()).ToString("C2")}</td><td align=\"right\">{decimal.Parse(ds.Tables[0].Rows[0]["TotalAmountCase"].ToString()).ToString("C2")}</td></tr>");
            Text = Text.Replace("{TBody}", sb.ToString());
            if (!ShowFullPage)
            {
                var doc = new HtmlDocument
                {
                    OptionDefaultStreamEncoding = Encoding.UTF8
                };
                doc.LoadHtml(Text);
                HtmlNode node = doc.DocumentNode.SelectNodes("//div[@id='Div_Content']")[0];
                ViewBag.TMPData = node.OuterHtml;
            }
            else
            {
                return Content(Text, "text/html"); // Return full HTML
            }
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public async Task<JsonResult> SendNotificationAdvExpense(int AdvanceExpenseID, string AdvNo, string CheckedByTelegramUserName, string UserFullName = "")
        {
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            var Text = ShowAdvExpense(AdvanceExpenseID, true) as ContentResult;
            Dictionary<string, string> para = new Dictionary<string, string>();

            // ============================================================
            // STEP 1: LOAD ADVANCE EXPENSE DATA & RESOLVE STATIC TELEGRAM TOPIC CONFIG
            // ============================================================
            string sql = "select * from dbo.TblAdvanceExpense where AdvanceExpenseID = @AdvanceExpenseID";
            para = new Dictionary<string, string>
            {
                { "AdvanceExpenseID", AdvanceExpenseID.ToString() }
            };

            DataSet ds = html.LoadData(sql, false, para);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                sqlresult.Result = ResultType.Failed;
                sqlresult.Message = "Advance Expense not found.";
                return Json(new { Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
            }

            DataRow row = ds.Tables[0].Rows[0];
            int sysID = Convert.ToInt32(row["SystemID"]);
            int warehouseId = row["WarehouseID"] == DBNull.Value ? 0 : Convert.ToInt32(row["WarehouseID"]);

            // Resolve static multi-system Telegram Topic configuration
            var tgConfig = GetStaticTelegramConfig(sysID, warehouseId, "AdvanceExpense");

            long chatID = tgConfig.ChatId;
            int topicID = tgConfig.TopicId;
            string bt = html.GetBotTokenBySystemID(sysID);
            bool isNeedCheck = true;

            if (String.IsNullOrEmpty(CheckedByTelegramUserName))
            {
                CheckedByTelegramUserName = row["ApprovedByTelegramUserName"]?.ToString() ?? "";
                UserFullName = row["ApprovedBy"]?.ToString() ?? "";
                isNeedCheck = false;
            }

            // ============================================================
            // STEP 2: GENERATE PDF & INLINE BUTTONS
            // ============================================================
            sqlresult = await html.HTMLToPDF(Text.Content, $"Uploads/AdvExpenses", AdvNo, false, true, "A5");
            string OutPutFile = sqlresult.OutPutFile;
            InlineKeyboardMarkup inlineKeyboard = !isNeedCheck
                ? new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Approve ✅", callbackData: $"ApproveAdvExpense~{AdvanceExpenseID}~{CheckedByTelegramUserName}"),
                        InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisapproveAdvExpense~{AdvanceExpenseID}~{CheckedByTelegramUserName}"),
                    }
                })
                : new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Check ✍️", callbackData: $"CheckAdvExpense~{AdvanceExpenseID}~{CheckedByTelegramUserName}"),
                        InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisCheckAdvExpense~{AdvanceExpenseID}~{CheckedByTelegramUserName}"),
                    }
                });

            // ============================================================
            // STEP 3: SEND TO TELEGRAM TOPIC
            // ============================================================
            string msg = "";
            msg += CheckedByTelegramUserName + "<br><br>Dear <b>" + UserFullName + $"</b>,<br><br>Now you have new advance expense need to {(isNeedCheck ? "check" : "approve")} that's created by " + html.UserFullName + ".";

            sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", chatID, topicID, inlineKeyboard, 0, "", OutPutFile, msg, true, "", false, bt);

            if (sqlresult.Result == ResultType.Successful)
            {
                System.IO.File.Delete(OutPutFile);

                sql = @"update dbo.TblAdvanceExpense set CheckStatus = null, CheckedDate = null, ApprovedDate = null, ApprovedStatus = null,
TelegramMessageID = @TelegramMessageID, TelegramMessage=@TelegramMessage, TelegramGroupTopicID = @TelegramGroupTopicID, TelegramGroupID = @TelegramGroupID where AdvanceExpenseID = @AdvanceExpenseID";
                para.Clear();
                para = new Dictionary<string, string>
                {
                    { "TelegramGroupID", chatID.ToString() },
                    { "TelegramMessageID", sqlresult.Value },
                    { "TelegramMessage", msg },
                    { "TelegramGroupTopicID", topicID.ToString() },
                    { "AdvanceExpenseID", AdvanceExpenseID.ToString() },
                };

                sqlresult = html.SaveChangeData(sql, false, para);
            }

            return Json(new { Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> CheckAdvExpense(int AdvanceExpenseID, string Status, string UserName = "", string SystemID = "")
        {
            HTML html = new HTML();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            SqlResult sqlresult = new SqlResult();
            string sql = "select TelegramMessageID, TelegramGroupID, TelegramGroupTopicID, AdvNo, TelegramMessage, ApprovedByTelegramUserName, ApprovedBy, SystemID, WarehouseID from dbo.TblAdvanceExpense where AdvanceExpenseID = " + AdvanceExpenseID;
            DataSet ds = html.LoadData(sql);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                return Json(new { Data = AdvanceExpenseID, Result = ResultType.Failed, Message = "Advance Expense not found." }, JsonRequestBehavior.AllowGet);
            }

            DataRow row = ds.Tables[0].Rows[0];
            int sysID = row["SystemID"] != DBNull.Value ? Convert.ToInt32(row["SystemID"]) : (SystemID != "" ? int.Parse(SystemID) : int.Parse(html.SystemID));
            int warehouseId = row["WarehouseID"] != DBNull.Value ? Convert.ToInt32(row["WarehouseID"]) : 0;

            // Resolve static multi-system Telegram Topic configuration
            var tgConfig = GetStaticTelegramConfig(sysID, warehouseId, "AdvanceExpense");
            string bt = tgConfig.BotToken;

            int TelegramMessageID = row["TelegramMessageID"] != DBNull.Value && row["TelegramMessageID"].ToString() != "" ? int.Parse(row["TelegramMessageID"].ToString()) : 0;
            string AdvNo = row["AdvNo"]?.ToString() ?? "";
            string TelegramMessage = row["TelegramMessage"]?.ToString() ?? "";
            string ApprovedByTelegramUserName = row["ApprovedByTelegramUserName"]?.ToString() ?? "";
            string ApprovedBy = row["ApprovedBy"]?.ToString() ?? "";

            long chatID = row["TelegramGroupID"] != DBNull.Value && row["TelegramGroupID"].ToString() != "" ? long.Parse(row["TelegramGroupID"].ToString()) : tgConfig.ChatId;
            int topicID = row["TelegramGroupTopicID"] != DBNull.Value && row["TelegramGroupTopicID"].ToString() != "" ? int.Parse(row["TelegramGroupTopicID"].ToString()) : tgConfig.TopicId;

            string msg = $"{ApprovedByTelegramUserName}<br><br> Dear {ApprovedBy}, <br><br>Now this Expense is Checked ✍️ By " + (UserName != "" ? UserName : html.UserFullName) + " and waiting approval.";
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
{
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Approve ✅", callbackData: "ApproveAdvExpense~"+AdvanceExpenseID.ToString()+"~ApprovedAdv~"+ApprovedByTelegramUserName),
                    InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: "DisapproveAdvExpense~"+AdvanceExpenseID.ToString()+"~DisapprovedAdv~"+ApprovedByTelegramUserName),
                }
            });
            if (Status == "Disapproved")
            {
                inlineKeyboard = null;
                msg = "Dear Team,<br><br>Now this Expense is Disapproved ❌ By " + (UserName != "" ? UserName : html.UserFullName) + ".<br><br>Please revise this Expense.<br><br>";
            }
            //else
            //{

            sql = @"update dbo.TblAdvanceExpense set CheckStatus = '" + Status + "', CheckedDate = '" + html.GetCurrentDate() + "' where AdvanceExpenseID =" + AdvanceExpenseID;
            sqlresult = html.SaveChangeData(sql, false, false);

            var Text = ShowAdvExpense(AdvanceExpenseID, true) as ContentResult;

            sqlresult = await html.HTMLToPDF(Text.Content, $"Uploads/Expenses", AdvNo, false, true, "A5");
            string OutPutFile = sqlresult.OutPutFile;
            //}
            //msg += "Thank you and best regards,<br><br>";
            //msg += "BILLING SYSTEM AUTO SEND Expense NOTIFICATION. #" + AdvNo;

            sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", chatID, topicID, inlineKeyboard, TelegramMessageID, TelegramMessage, OutPutFile, msg, true, "", false, bt);

            if (System.IO.File.Exists(OutPutFile))
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.IO.File.Delete(OutPutFile);
            }
            sql = "update dbo.TblAdvanceExpense set TelegramMessageID = @TelegramMessageID, TelegramMessage=@TelegramMessage, TelegramGroupTopicID = @TelegramGroupTopicID, TelegramGroupID = @TelegramGroupID where AdvanceExpenseID = @AdvanceExpenseID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "TelegramGroupID", chatID.ToString() },
                { "TelegramGroupTopicID", topicID.ToString() },
                { "TelegramMessageID", sqlresult.Value },
                { "TelegramMessage", msg },
                { "AdvanceExpenseID", AdvanceExpenseID.ToString() },
            };

            sqlresult = html.SaveChangeData(sql, false, para);
            return Json(new { Data = AdvanceExpenseID, Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ApproveDisapproveExpense(int AdvanceExpenseID, string Status, string UserName = "", string SystemID = "")
        {
            HTML html = new HTML();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            SqlResult sqlresult = new SqlResult();
            string sql = "select TelegramMessageID,TelegramGroupID,TelegramGroupTopicID, AdvNo, TelegramMessage, SystemID, WarehouseID from dbo.TblAdvanceExpense where AdvanceExpenseID = " + AdvanceExpenseID;
            DataSet ds = html.LoadData(sql);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                return Json(new { Data = AdvanceExpenseID, Result = ResultType.Failed, Message = "Advance Expense not found." }, JsonRequestBehavior.AllowGet);
            }

            DataRow row = ds.Tables[0].Rows[0];
            int sysID = row["SystemID"] != DBNull.Value ? Convert.ToInt32(row["SystemID"]) : (SystemID != "" ? int.Parse(SystemID) : int.Parse(html.SystemID));
            int warehouseId = row["WarehouseID"] != DBNull.Value ? Convert.ToInt32(row["WarehouseID"]) : 0;

            // Resolve static multi-system Telegram Topic configuration
            var tgConfig = GetStaticTelegramConfig(sysID, warehouseId, "AdvanceExpense");
            string bt = tgConfig.BotToken;

            int TelegramMessageID = row["TelegramMessageID"] != DBNull.Value && row["TelegramMessageID"].ToString() != "" ? int.Parse(row["TelegramMessageID"].ToString()) : 0;
            string AdvNo = row["AdvNo"]?.ToString() ?? "";
            string TelegramMessage = row["TelegramMessage"]?.ToString() ?? "";

            long chatID = row["TelegramGroupID"] != DBNull.Value && row["TelegramGroupID"].ToString() != "" ? long.Parse(row["TelegramGroupID"].ToString()) : tgConfig.ChatId;
            int topicID = row["TelegramGroupTopicID"] != DBNull.Value && row["TelegramGroupTopicID"].ToString() != "" ? int.Parse(row["TelegramGroupTopicID"].ToString()) : tgConfig.TopicId;

            string msg = "Dear Team,<br><br>Now this Expense is Approved ✅ By " + (UserName != "" ? UserName : html.UserFullName) + ".<br><br>";
            if (Status == "Disapproved")
            {
                msg = "Dear Team,<br><br>Now this Expense is Disapproved ❌ By " + (UserName != "" ? UserName : html.UserFullName) + ".<br><br>Please revise this Expense.<br><br>";
            }
            //else
            //{

            sql = @"update dbo.TblAdvanceExpense set ApprovedStatus = '" + Status + "', ApprovedDate = '" + html.GetCurrentDate() + "' where AdvanceExpenseID =" + AdvanceExpenseID;
            sqlresult = html.SaveChangeData(sql, false, false);

            var Text = ShowAdvExpense(AdvanceExpenseID, true) as ContentResult;

            sqlresult = await html.HTMLToPDF(Text.Content, $"Uploads/Expenses", AdvNo, false, true, "A5");
            string OutPutFile = sqlresult.OutPutFile;
            //}
            //msg += "Thank you and best regards,<br><br>";
            //msg += "BILLING SYSTEM AUTO SEND Expense NOTIFICATION. #" + AdvNo;

            sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", chatID, topicID, null, TelegramMessageID, TelegramMessage, OutPutFile, msg, true, "", false, bt);

            if (System.IO.File.Exists(OutPutFile))
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.IO.File.Delete(OutPutFile);
            }

            sql = "update dbo.TblAdvanceExpense set TelegramMessageID = @TelegramMessageID, TelegramMessage=@TelegramMessage, TelegramGroupTopicID = @TelegramGroupTopicID, TelegramGroupID = @TelegramGroupID where AdvanceExpenseID = @AdvanceExpenseID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "TelegramGroupID", chatID.ToString() },
                { "TelegramGroupTopicID", topicID.ToString() },
                { "TelegramMessageID", sqlresult.Value },
                { "TelegramMessage", msg },
                { "AdvanceExpenseID", AdvanceExpenseID.ToString() },
            };
            sqlresult = html.SaveChangeData(sql, false, para);
            return Json(new { Data = AdvanceExpenseID, Result = sqlresult.Result, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> CancelAdv(int AdvExpenseID, string Comment)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            string sql = @"update dbo.TblAdvanceExpense set ApprovedStatus = 'Cancel', CancelComment = @CancelComment where AdvanceExpenseID = @AdvanceExpenseID";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "CancelComment", Comment },
                { "AdvanceExpenseID", AdvExpenseID.ToString() }
            };

            sqlResult = html.SaveChangeData(sql, false, para);

            if (sqlResult.Result == ResultType.Successful)
            {
                // Notify Telegram Group
                sql = "select TelegramMessageID, TelegramGroupID, TelegramGroupTopicID, AdvNo, TelegramMessage, SystemID from dbo.TblAdvanceExpense where AdvanceExpenseID = @AdvanceExpenseID";
                DataSet ds = html.LoadData(sql, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    long chatID = long.Parse(ds.Tables[0].Rows[0]["TelegramGroupID"].ToString());
                    int topicID = int.Parse(ds.Tables[0].Rows[0]["TelegramGroupTopicID"].ToString());
                    string advNo = ds.Tables[0].Rows[0]["AdvNo"].ToString();
                    int tmsgID = int.Parse(ds.Tables[0].Rows[0]["TelegramMessageID"].ToString());
                    string tmsg = ds.Tables[0].Rows[0]["TelegramMessage"].ToString();
                    int sysID = int.Parse(ds.Tables[0].Rows[0]["SystemID"].ToString());
                    string bt = html.GetBotTokenBySystemID(sysID);

                    string msg = "";
                    msg += "<br><br>Dear Team,<br><br>Now this Advance Expense is cancelled ❌.<br>";
                    msg += "Adv No: [" + advNo + "].<br>";
                    msg += "Cancel By: " + html.UserFullName + ".<br><br>";
                    msg += "Cancel Comment: " + Comment;
                    sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(msg, chatID, topicID, null, tmsgID, tmsg, "", "", false, "", false, bt);
                }
            }

            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "Add Expenses, Edit Expenses")]
        public ActionResult PostExpensesForm(string PrimaryFieldValue, string PaymentNo)
        {
            HTML html = new HTML
            {
                FormID = "frmPostExpenses",
                SaveChangeFunction = "SaveChangePostingExpense();"
            };
            ViewBag.ExpenseID = PrimaryFieldValue;
            ViewBag.PaymentNo = PaymentNo;
            string sqlex = $@"select e.ClassName, e.PaidAmount, v.VendorName ReceivedFrom, pm.PaymentMethod, 
ac.AccountNo + ' - ' +  ac.AccountName PaymentAccount, isnull(e.ExchangeRate, '0') ExchangeRate, e.Reference,
ex.PostingPrefix, e.[No]
from dbo.Expense_PaymentTransactions e
inner join dbo.Expenses ex on ex.ExpenseID = e.ExpenseID
inner join dbo.TblVendor v on v.VendorID = e.PayeeID
inner join dbo.TblPaymentMethod pm on pm.PaymentMethodID = e.PaymentMethodID
inner join dbo.TblChartOfAccount ac on ac.ChartOfAccountID = e.PaymentAccountID
where e.ExpenseID = @ExpenseID and e.PaymentNo = '{PaymentNo}'";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ExpenseID", PrimaryFieldValue },
            };
            DataSet ds = html.LoadData(sqlex, false, para);
            string ClassID = ds.Tables[0].Rows[0]["ClassName"].ToString();
            string PaidAmount = ds.Tables[0].Rows[0]["PaidAmount"].ToString();
            string ReceivedFrom = ds.Tables[0].Rows[0]["ReceivedFrom"].ToString();
            string PaymentMethod = ds.Tables[0].Rows[0]["PaymentMethod"].ToString();
            string PaymentAccount = ds.Tables[0].Rows[0]["PaymentAccount"].ToString();
            string ExchangeRate = ds.Tables[0].Rows[0]["ExchangeRate"].ToString();
            string Reference = ds.Tables[0].Rows[0]["Reference"].ToString();
            string PostingPrefix = ds.Tables[0].Rows[0]["PostingPrefix"].ToString();
            string No = ds.Tables[0].Rows[0]["No"].ToString();
            string PrefixPVNO = "";
            SqlResult sqlResult = new SqlResult();
            string sql = @"declare @Prefix nvarchar(50)
select @Prefix = PrefixPV from dbo.TblClass where ClassID = @ClassID
exec [SP_SHOW_PREFIX] @Prefix, 'PostPVExpense', @SystemID, @WarehouseID, @DomainID;";
            para = new Dictionary<string, string>
            {
                { "ClassID", ClassID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            ds = html.LoadData(sql, false, para);

            if (ds.Tables[0].Rows.Count > 0)
            {
                string PVNo = ds.Tables[0].Rows[0][0].ToString();
                PrefixPVNO = ds.Tables[0].Rows[0][1].ToString();
            }
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "No:",
                ID = "No",
                //DBField = "PVNo",
                Value = No,
                IsReadOnly = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Posting Date:",
                ID = "PostingPaymentDate",
                DBField = "MakePostedData",
                SingleDatePicker = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Paid On Date:",
                ID = "PaidOnDate",
                DBField = "PaidOnDate",
                SingleDatePicker = true,
                DefaultShowDateValue = false
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "PV No:",
                ID = "PVNo",
                DBField = "PVNo",
                //Value = PVNo,
                //IsReadOnly = true,
                HiddenField = "PrefixPVNO, PostingPrefix",
                HiddenFieldValue = PrefixPVNO + "," + PostingPrefix,
                //Hide = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Posting No:",
                ID = "PostingNumber",
                DBField = "PostingNumber",
                //Value = PVNo,
                //IsReadOnly = true,
                //HiddenField = "PrefixPVNO, PostingPrefix",
                //HiddenFieldValue = PrefixPVNO + "," + PostingPrefix,
                //Hide = true
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Received From:",
                ID = "ReceivedFrom",
                DBField = "ReceivedFrom",
                Value = ReceivedFrom,
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Payment Method:",
                ID = "PaymentMethod",
                DBField = "PaymentMethod",
                Value = PaymentMethod,
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Payment Account:",
                ID = "PaymentAccount",
                DBField = "PaymentAccount",
                Value = PaymentAccount,
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Exchange Rate:",
                ID = "ExchangeRate",
                DBField = "ExchangeRate",
                Value = ExchangeRate,
                IsReadOnly = true
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Reference No.:",
                ID = "ReferenceNo",
                DBField = "ReferenceNo",
                Value = Reference,
                IsReadOnly = true
            });
            string IncomeAccount = @"select distinct coa.ChartOfAccountID, ac.AccountCategory, act.AccountTypes, coa.AccountNo, coa.AccountName
from dbo.TblAccountTypes act
inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID
inner join dbo.TblChartOfAccount coa on coa.AccountType = act.AccountTypesID
where ac.AccountCategoryID = 3 and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " order by ac.AccountCategory, act.AccountTypes";

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Amount Received:",
                ID = "PaymentAmount",
                DBField = "PaymentAmount",
                Required = true,
                Value = PaidAmount,
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "PaymentDescription",
                DBField = "PaymentDescription",
                //Required = true,
                //Value = (Status == "10" ? "Payment for Invoice No.: " + InvoiceNo : "Rollback Payment for Invoice No.: " + InvoiceNo)
            });
            ViewBag.PaidAmount = PaidAmount;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public JsonResult SaveChangePostingExpense(
    int ExpenseID,
    DateTime Posting_Date,
    string PaidOn_Date,
    string Description,
    string PrefixPVNO,
    string PostingPrefix,
    string PVNo,
    string PostingNumber,
    string PaymentNo,
    string PaidAmount
    )
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = @"
DECLARE @TPaidAmount decimal(18,2), @PostedBy NVARCHAR(MAX), @PostedByUserID INT, @PostedByTelegramUserName NVARCHAR(MAX),
@PVNo NVARCHAR(50), @PostingNumber NVARCHAR(50), @JournalNo NVARCHAR(50), @OutExpensePaymentsNo NVARCHAR(50);

select @TPaidAmount = sum(PaidAmount) from Expense_PaymentTransactions where ExpenseID = @ExpenseID;

select @PostedBy = u.FullName, @PostedByUserID = u.UserLoginID, @PostedByTelegramUserName = isnull(u.TelegramUserName, '')
from dbo.TblUserLogin u where u.UserLoginID = " + html.UserID + @";

--exec [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR] 'EXP{yy}{MM}{dd}-{00000}', 'ExpensePaymentsNo', @SystemID, @WarehouseID, @OutExpensePaymentsNo OUT, 1, 0, @DomainID
";

            if (PVNo == "")
            {
                sql += @"
EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR]
     @PrefixPVNO,
     'PrefixPV',
     @SystemID,
     @WarehouseID,
     @PVNo OUTPUT,
     1,
     0,
     @DomainID;";
            }
            else
            {
                sql += $@"set @PVNo=N'{PVNo}';";
            }
            if (PostingNumber == "")
            {
                sql += @"
EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR]
     @PostingPrefix,
     'Expense',
     @SystemID,
     @WarehouseID,
     @PostingNumber OUTPUT,
     1,
     0,
     @DomainID;";
            }
            else
            {
                sql += $@"set @PostingNumber=N'{PostingNumber}';";
            }
            sql += @"
--EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR]
  --   'JV{yy}{MM}_{00000}',
    -- 'JournalNo',
     --@SystemID,
     --@WarehouseID,
     --@JournalNo OUTPUT,
     --1,
     --0,
     --@DomainID;

UPDATE  TblAccount_Transaction SET IsPosted = 1, TransactionDate = @PostingDate, 
        InvoiceNo = @PVNo, PVNo = @PVNo,
        Amount = @TPaidAmount
WHERE ExpenseID = @ExpenseID AND PaymentNo = @PaymentNo;

UPDATE Expenses
SET
    IsPosted = 1,
    PostedDate = @PostingDate,
    PaidOnDate = @PaidOnDate,
    PostedBy = @PostedBy,
    PostedByUserID = @PostedByUserID,
    PostedByTelegramUserName = @PostedByTelegramUserName,
    PostingNumber = @PostingNumber,
    PVNo = @PVNo,
    PostingMemo = @PostingMemo,
    MakePostedData = @MakePostedData,
    PostingDOC = @PostingDOC,
    JournalNo = @JournalNo,
    PaidAmount = @TPaidAmount,
    IsRequestedPayment = 0,
    BalanceAmount = (TotalAmount - @TPaidAmount)
    --, PostedPaymentNo = @PaymentNo
WHERE ExpenseID = @ExpenseID --AND PaymentNo = @PaymentNo;

UPDATE Expense_PaymentTransactions
SET
    IsPosted = 1,
    PostedDate = @PostingDate,
    PaidOnDate = @PaidOnDate,
    PostedBy = @PostedBy,
    PostedByUserID = @PostedByUserID,
    PostedByTelegramUserName = @PostedByTelegramUserName,
    PostingNumber = @PostingNumber,
    PVNo = @PVNo,
    PostingMemo = @PostingMemo,
    MakePostedData = @MakePostedData,
    PostingDOC = @PostingDOC,
    JournalNo = @JournalNo,
    IsRequestedPayment = 0
    --, PostedPaymentNo = @PaymentNo
WHERE ExpenseID = @ExpenseID AND PaymentNo = @PaymentNo;

UPDATE TblAccount_JournalEntries SET IsPosted = 1, InvoiceNo = @PVNo WHERE ExpenseID = @ExpenseID AND PaymentNo = @PaymentNo;

SELECT
    @PVNo AS PVNo,
    @PostingNumber AS PostingNumber;
";

            try
            {
                string DestinationFolder = "/Uploads/Expenses/PostingDOC/";//$"{html.ToUrlSlug($"{CustomerName + " [" + CustomerCode + "]"}")}";
                string PostingDOC = "";
                SqlResult result = new SqlResult();
                result = html.UploadFile(DestinationFolder);
                if (result.Result == ResultType.Successful)
                    PostingDOC = result.Value;
                else
                {
                    return Json(new
                    {
                        Message = result.Message,
                        Result = result.Result,
                    }, JsonRequestBehavior.AllowGet);
                }
                using (SqlConnection conn = new SqlConnection(html.GetConnectionString()))
                {
                    conn.Open();

                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                            {
                                // Main Parameters
                                cmd.Parameters.Add("@PaidAmount", SqlDbType.Decimal).Value = PaidAmount;
                                cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = ExpenseID;
                                cmd.Parameters.Add("@PostingDate", SqlDbType.DateTime).Value = Posting_Date.Date + DateTime.Now.TimeOfDay;
                                cmd.Parameters.Add("@PaidOnDate", SqlDbType.DateTime).Value =
                                string.IsNullOrWhiteSpace(PaidOn_Date)
                                    ? (object)DBNull.Value
                                    : DateTime.Parse(PaidOn_Date);
                                cmd.Parameters.Add("@PostingDOC", SqlDbType.NVarChar).Value = PostingDOC ?? "";

                                // Prefix Parameters
                                cmd.Parameters.Add("@PrefixPVNO", SqlDbType.NVarChar).Value = PrefixPVNO ?? "";
                                cmd.Parameters.Add("@PostingPrefix", SqlDbType.NVarChar).Value = PostingPrefix ?? "";

                                // System Parameters
                                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value = html.SystemID;
                                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value = html.WarehouseID;
                                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar).Value = html.DomainID;
                                cmd.Parameters.Add("@PaymentNo", SqlDbType.NVarChar).Value = PaymentNo;

                                // Posting Data
                                cmd.Parameters.Add("@PostingMemo", SqlDbType.NVarChar).Value =
                                    Description ?? "";

                                cmd.Parameters.Add("@MakePostedData", SqlDbType.DateTime).Value =
                                    DateTime.Now;

                                string pvNo = "";
                                string postingNumber = "";
                                //cmd.ExecuteNonQuery();
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        pvNo = reader["PVNo"]?.ToString();
                                        postingNumber = reader["PostingNumber"]?.ToString();
                                    }
                                }

                                tran.Commit();

                                sqlResult.Result = ResultType.Successful;
                                sqlResult.Message = "Expense posted successfully.";

                                return Json(new
                                {
                                    Result = sqlResult.Result,
                                    Message = sqlResult.Message,
                                    ExpenseID = ExpenseID,
                                    PVNo = pvNo,
                                    PostingNumber = postingNumber
                                }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();

                            sqlResult.Result = ResultType.Failed;
                            sqlResult.Message = ex.Message;

                            return Json(new
                            {
                                Result = sqlResult.Result,
                                Message = ex.Message
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Result = sqlResult.Result,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Add Expenses, Edit Expenses")]
        public ActionResult RequestPayment(string PrimaryFieldValue, string Status = "Draft", string No = "", string PaymentNo = "")
        {
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.Status = Status;
            HTML html = new HTML
            {
                PrimaryFieldValue = PrimaryFieldValue,
                PrimaryField = "ExpenseID",
                FormID = "FrmAddExpense"
            };
            string sqlPayee = @"SELECT 
    a.PayeeID, 
    a.Code, 
    a.[Name], 
    a.[Type], 
    a.OrderBy, 
    ISNULL(a.Gender, '') AS Gender,
    ISNULL(a.PhoneNumber, '') PhoneNumber
FROM (
    
    -- ========================
    -- Vendor
    -- ========================
    SELECT 
        v.VendorID AS PayeeID, 
        CAST(v.VendorCode AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code, 
        CAST(v.VendorName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name], 
        CAST('Vendor' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type], 
        1 AS OrderBy, 
        CAST(v.Gender AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
        CAST(v.ContactPhoneNumber AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
    FROM dbo.TblVendor v
    WHERE v.SystemID = " + html.SystemID + " AND v.WarehouseID = " + html.WarehouseID + @"
    UNION ALL

    -- ========================
    -- Customer
    -- ========================
    SELECT 
        c.CustomerID AS PayeeID, 
        CAST(c.CustomerCode AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code, 
        CAST(c.CustomerName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name], 
        CAST('Customer' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type], 
        3 AS OrderBy, 
        CAST(c.CustomerGender AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
        CAST(c.CustomerContactMobileNumber AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
    FROM dbo.TblCustomer c
    WHERE ISNULL(c.CustomerCode, '') <> '' AND c.SystemID = " + html.SystemID + " AND c.WarehouseID = " + html.WarehouseID + @"
    UNION ALL

    -- ========================
    -- Employee (Different DB)
    -- ========================
    SELECT 
        e.EmployeeID AS PayeeID, 
        CAST('' AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code, 
        CAST(e.FullName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name], 
        CAST('Employee' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type], 
        2 AS OrderBy, 
        CAST(g.GenderName AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
        CAST(p.PhoneNumber1 AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
    FROM HRMS_DB.dbo.Employees e
    INNER JOIN HRMS_DB.dbo.Gender g 
        ON g.GenderID = e.GenderID
    INNER JOIN HRMS_DB.dbo.Staffing_EmployeePhones p 
        ON p.EmployeeID = e.EmployeeID

) AS a
ORDER BY 
    a.OrderBy, 
    a.[Name] ASC;";
            string ExpenseAccount = @"SELECT
    coa.ChartOfAccountID,
    cat.AccountCategory,
    ct.AccountTypes,
    coa.AccountNo,
    coa.AccountName,
    coa.ParentAccountID
FROM TblChartOfAccount coa
INNER JOIN dbo.TblAccountTypes ct 
    ON ct.AccountTypesID = coa.AccountTypesID
INNER JOIN dbo.TblAccountCategories cat 
    ON cat.AccountCategoryID = coa.AccountCategoryID
WHERE 
    coa.SystemID = " + html.SystemID + @"
    AND coa.WarehouseID = " + html.WarehouseID + @"

    -- Allow Payment Accounts (Asset) OR A/P (Liability)
    AND (
        (
            ct.AccountTypes = 'Asset'
            AND coa.AccountName NOT LIKE '%Discount%'
            AND coa.AccountName NOT LIKE '%Advance%'
            AND coa.AccountName NOT LIKE '%Receivable%'
            AND coa.AccountName NOT LIKE '%Prepaid%'
            AND coa.AccountName NOT LIKE '%Cash out%'

            AND (
                coa.ParentAccountID IN (
                    SELECT ChartOfAccountID
                    FROM TblChartOfAccount
                    WHERE AccountName IN (
                        'Petty Cash',
                        'Cash in Bank',
                        'Cash on Hand'
                    )
                )
                OR coa.AccountName IN (
                    'Petty Cash',
                    'Cash in Bank',
                    'Cash on Hand'
                )
            )
        )

        -- ✅ Accounts Payable
        OR (
            ct.AccountTypes = 'Liability'
            AND coa.AccountName LIKE '%Payable%'
        )
    )
ORDER BY
    coa.AccountNo;

";

            string PaidAmount = "0";
            if (PrimaryFieldValue != "0")
            {
                string sql = @"SELECT isnull(PaidAmount, 0) AS PaidAmount FROM dbo.Expenses WHERE ExpenseID=" + PrimaryFieldValue;
                PaidAmount = html.LoadData(sql).Tables[0].Rows[0]["PaidAmount"].ToString();
            }
            ViewBag.PaidAmount = PaidAmount;

            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="Expenses",
                    FormTitle="Expense",
                    GetIDENTITYByFiled="ExpenseID",
                    PrimaryFieldName="ExpenseID",
                    ISAddAttachmentFile = true,
                    DestinationFile = "Uploads/Expenses",
                    //SQLEdit="select * from dbo.TblAccount_Expenses where ExpensesID="+ PrimaryFieldValue,
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label="Class Name:", ID="ClassName",
                            Required=true,
                            Class="input2",
                            IsCustomForm = true,
                            DBField="ClassName",
                            SQL_CBO= "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID ="+html.WarehouseID + " and DomainID like '%"+html.DomainID+"%'",
                            Value = "ClassID",
                            DisplayText = "ClassName",
                            IsKendoMultiColumnComboBox = true,
                            CallBackFunction="PaymentMothod(this);",
                            ReadOnly = No != ""
                        },
                        new HtmlComboBox()
                        {
                            Label="Payment Method:", ID="PaymentMethodID",
                            Class="input2", IsCustomForm = true,
                            DBField="PaymentMethodID",
                            SQL_CBO="select PaymentMethodID, PaymentMethod from dbo.TblPaymentMethod where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and PaymentMethod in ('Bank Transfer', 'Cheque', 'Petty Cash') order by PaymentMethod",
                            Value="PaymentMethodID",
                            DisplayText="PaymentMethod",
                            IsKendoMultiColumnComboBox = true,
                            Required = true,
                            CallBackFunction ="PaymentMothod(this);",
                            ReadOnly = No != ""
                        },

                        new HtmlComboBox()
                        {
                            Label="Payee", ID="PayeeID",
                            Required=true,
                            Class="input2",
                            IsCustomForm = true,
                            DBField="PayeeID",
                            SQL_CBO= sqlPayee,
                            Value = "PayeeID",
                            DisplayText = "Name",
                            //GroupByField = "Type",
                            IsKendoMultiColumnComboBox = true,
                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                            {
                                //new KendoComboBoxColumns() { field = "Code", title = "Code", width = "120" },
                                new KendoComboBoxColumns() { field = "Name", title = "Name", width = "100%" }
                            },
                            KendoMultiColumnComboBoxHeight = 350,
                            //CallBackFunction="LoadVendorBankInfo()",
                            //AddNewFuntionName="AddNewPayee()",
                            //ConditionField="PayeeType",
                            ReadOnly = No != ""
                        },
                        new HtmlTextBox()
                        {
                            Label="No.:", ID="No",
                            Class="input2", IsCustomForm = true,
                            DBField="No", IsReadOnly = true, Required = true,
                            HiddenField="NoPrefix",
                            Hide = true
                        }
                    },
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label="Payment Account:", ID="PaymentAccountID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            SQL_CBO = ExpenseAccount,
                            DBField = "PaymentAccountID",
                            Value = "ChartOfAccountID",
                            DisplayText = "AccountName",
                            GroupByField = "AccountCategory",
                            IsKendoMultiColumnComboBox = true,
                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                            {
                                new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                                new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                            },
                            KendoMultiColumnComboBoxHeight = 300,
                            Required = true,
                            ReadOnly = No != ""
                        },
                        new HtmlTextBox()
                        {
                            Label="Payment Date:", ID="PaymentDate",
                            Class="input2", IsCustomForm = true,
                            SingleDatePicker=true,
                            DBField="PaymentDate",
                            //DivClassCol="col-auto",
                            Required = true,
                            Hide = true,

                        },
                        new HtmlTextBox()
                        {
                            Label="Terms:", ID="PaymentTerms",
                            Class="input2", IsCustomForm = true,
                            DBField="PaymentTermsID",
                            DivClassCol="col-sm-auto",
                            SingleDatePicker=true,
                            DefaultShowDateValue = false,
                            IsReadOnly = No != ""
                            //ISNumeric = true,
                            //SQL_CBO= "select PaymentTermsID, TermName from dbo.TblPaymentTerms where SystemID = " + html.SystemID + " and WarehouseID ="+html.WarehouseID + " and DomainID like '%"+html.DomainID+"%'",
                            //Value = "PaymentTermsID",
                            //DisplayText = "TermName",
                            //IsKendoMultiColumnComboBox = true,
                            //Required = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Paid On Date:", ID="PaidOnDate",
                            Class="input2", IsCustomForm = true,
                            SingleDatePicker=true,
                            DBField="PaidOnDate",
                            DefaultShowDateValue = false,
                            //DivClassCol="col-auto",
                            //Required = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="PV No.:", ID="PVNo",
                            Class="input2", IsCustomForm = true,
                            DBField="PVNo", IsReadOnly = true, Required = true,
                            HiddenField="PVNoPrefix", Hide = true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Banker's Name:", ID="ChequeBankerName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ChequeBankerName",
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 BankerName from TblVendor where isnull(BankerName, '') <> '' and lower(BankerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by BankerName",
                            IsReadOnly = No != ""
                        },
                        new HtmlTextBox()
                        {
                            Label="Cheque To:", ID="ChequeTo",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ChequeTo",
                            IsReadOnly = No != ""
                        },
                        new HtmlTextBox()
                        {
                            Label="Cheque No:", ID="ChequeNo",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ChequeNo",
                            IsReadOnly = No != ""
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Banker's Name:", ID="BankerName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BankerName",
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 BankerName from TblVendor where isnull(BankerName, '') <> '' and lower(BankerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by BankerName",
                            IsReadOnly = No != ""
                        },
                        new HtmlTextBox()
                        {
                            Label="Account Name:", ID="BankAccountName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BankAccountName",
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 AccountNo BankAccountNo, AccountName BankAccountName from TblVendor where isnull(AccountName, '') <> '' and lower(AccountName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountName",
                            DisplayOnFields ="BankAccountNo",
                            IsReadOnly = No != ""
                        },
                        new HtmlTextBox()
                        {
                            Label="Account No:", ID="BankAccountNo",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BankAccountNo",
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 AccountNo from TblVendor where isnull(AccountNo, '') <> '' and lower(AccountNo) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountNo",
                            IsReadOnly = No != ""
                        }
                    }
                },

                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Checked By", ID="CheckedBy",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CheckedBy", isTypeahead = true,
                            //SQL="select distinct isnull(RequestBy, '') RequestBy from dbo.TblPurchaseOrder \r\nwhere isnull(RequestBy, '') <> '' and SystemID = "+html.SystemID+" and WarehouseID = "+ html.WarehouseID + " order by isnull(RequestBy, '')",
                            SQL="select u.UserLoginID CheckedByUserID, u.TelegramUserName CheckedByTelegramUserName, u.FullName CheckedBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="CheckedByTelegramUserName, CheckedByUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Verified By", ID="VerifiedBy",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="VerifiedBy", isTypeahead = true,
                            SQL="select u.UserLoginID VerifiedByUserID, u.TelegramUserName VerifiedByTelegramUserName, u.FullName VerifiedBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="VerifiedByTelegramUserName, VerifiedByUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto",
                            Required = true,
                        },
                        //new HtmlTextBox()
                        //{
                        //    Label="Approved By", ID="ApprovedBy",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="ApprovedBy", isTypeahead = true,
                        //    SQL="select u.UserLoginID ApprovedByUserID, u.TelegramUserName ApprovedByTelegramUserName, u.FullName ApprovedBy, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                        //    HiddenField="ApprovedByTelegramUserName, ApprovedByUserID",
                        //    HiddenFieldValue="0,0",
                        //    //DivClassCol="col-sm-auto",
                        //    Required = true,
                        //},
                        new HtmlTextBox()
                        {
                            Label="Posted By", ID="PostedBy",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PostedBy", isTypeahead = true,
                            SQL="select u.UserLoginID PostedByUserID, u.TelegramUserName PostedByTelegramUserName, u.FullName PostedBy, u.Job PostedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="PostedByTelegramUserName, PostedByUserID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm-auto",
                            //Required = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Posting Number", ID="PostingNumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PostingNumber",
                            HiddenField="PostingPrefix",
                            Hide = true,
                            //DivClassCol="col-sm-auto",
                            Required = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Reference No.:", ID="Reference",
                            Class="input2", IsCustomForm = true,
                            DBField="Reference",
                            ISAutoComplete=true,
                            SQL_AutoComplete = @"
                SELECT top 20 *
                FROM (
                    SELECT 'PO' AS Source, PONumber AS ReferenceNumber
                    FROM dbo.TblPurchaseOrder
                    WHERE StatusID = 90 AND SystemID = "+html.SystemID+" AND WarehouseID = "+html.WarehouseID+@"

                    UNION ALL

                    SELECT 'RF', ReferenceNumber
                    FROM dbo.TblRequestForm
                    WHERE StatusID = 90 AND SystemID = "+html.SystemID+" AND WarehouseID = "+html.WarehouseID+@"

                    UNION ALL

                    SELECT 'ADV', AdvNo ReferenceNumber
                    FROM dbo.TblAdvanceExpense
                    WHERE ApprovedStatus = 'Approved' AND SystemID = "+html.SystemID+" AND WarehouseID = "+html.WarehouseID+@"
                ) U
                WHERE lower(U.ReferenceNumber) LIKE @SearchText
                ORDER BY U.ReferenceNumber",
                            HiddenField="Source",
                            isTagsInput=true,
                            OnChangeFunction="LoadDetail(this)",
                            IsReadOnly = No != ""
                        },
                        //new HtmlTextBox()
                        //{
                        //    Label="Payment Terms:", ID="PaymentTerms",
                        //    Class="input2", IsCustomForm = true,
                        //    DBField="PaymentTermsID",
                        //    DivClassCol="col-sm-auto",
                        //    SingleDatePicker=true,
                        //    DefaultShowDateValue = false,
                        //    //ISNumeric = true,
                        //    //SQL_CBO= "select PaymentTermsID, TermName from dbo.TblPaymentTerms where SystemID = " + html.SystemID + " and WarehouseID ="+html.WarehouseID + " and DomainID like '%"+html.DomainID+"%'",
                        //    //Value = "PaymentTermsID",
                        //    //DisplayText = "TermName",
                        //    //IsKendoMultiColumnComboBox = true,
                        //    //Required = true
                        //},
                        new HtmlTextBox()
                        {
                            Label="Exchange Rate:", ID="ExchangeRate",
                            Class="input2", IsCustomForm = true,
                            DBField="ExchangeRate",
                            DivClassCol="col-sm-auto",
                            ISNumeric = true,
                            IsReadOnly = No != ""
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextarea()
                        {
                            Label="Description:", ID="Description",
                            Class="input2", IsCustomForm = true,
                            DBField="Description",
                        }
                    }
                },

            };
            html.ShowDialogTotalLabelNameFooter = true;
            html.SaveChangeFunction = "SaveChangeExpense()";
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        // GET: /ExpenseTransactions/AddNew
        [HttpGet]
        public ActionResult AddNew()
        {
            var vm = new ExpenseCreateVm();

            ViewBag.Lookup = BuildLookups();

            // Optional: preload one empty row.
            vm.Items.Add(new ExpenseItemVm
            {
                Qty = 1m,
                UnitPrice = 0m,
                Discount = 0m,
                DiscountType = "AMOUNT"
            });

            vm.AdditionalExpenses.Add(new AdditionalExpenseVm());

            return View(vm);
        }

        // POST: /ExpenseTransactions/Save
        // Replace the TODO section with your existing EF6 save/journal-entry logic.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(ExpenseCreateVm model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request." });

            if (model.ExchangeRate <= 0)
                model.ExchangeRate = 1m;

            if (model.Items == null)
                model.Items = new List<ExpenseItemVm>();

            if (model.AdditionalExpenses == null)
                model.AdditionalExpenses = new List<AdditionalExpenseVm>();

            // Remove completely empty rows.
            model.Items = model.Items
                .Where(x => x != null &&
                            (x.AccountId > 0 ||
                             !string.IsNullOrWhiteSpace(x.ItemService) ||
                             x.Qty != 0 ||
                             x.UnitPrice != 0))
                .ToList();

            model.AdditionalExpenses = model.AdditionalExpenses
                .Where(x => x != null &&
                            (x.AccountId > 0 ||
                             !string.IsNullOrWhiteSpace(x.Description) ||
                             x.Amount != 0))
                .ToList();

            if (!model.ClassId.HasValue)
                return Json(new { success = false, message = "Please select Class Name." });

            if (!model.PaymentMethodId.HasValue)
                return Json(new { success = false, message = "Please select Payment Method." });

            if (!model.PaymentAccountId.HasValue)
                return Json(new { success = false, message = "Please select Payment Account." });

            if (!model.PayeeId.HasValue)
                return Json(new { success = false, message = "Please select Payee." });

            if (!model.Items.Any() && !model.AdditionalExpenses.Any())
                return Json(new { success = false, message = "Please add at least one expense line." });

            if (model.Items.Any(x => x.AccountId <= 0))
                return Json(new { success = false, message = "Every item line must have an account." });

            if (model.AdditionalExpenses.Any(x => x.AccountId <= 0))
                return Json(new { success = false, message = "Every additional expense line must have an account." });

            // TODO:
            // 1. Begin your EF6 transaction.
            // 2. Insert ExpenseTransaction header.
            // 3. Insert ExpenseTransactionDetail rows.
            // 4. Insert AdditionalExpense rows.
            // 5. Create the related journal entries / A/P entries according to
            //    your accounting flow.
            // 6. Commit transaction.
            //
            // Example:
            // using (var db = new AccountingDbContext())
            // using (var tx = db.Database.BeginTransaction())
            // {
            //     ...
            //     db.SaveChanges();
            //     tx.Commit();
            // }

            var itemTotal = model.Items.Sum(x =>
                Math.Max(0m, (x.Qty * x.UnitPrice) - x.Discount));

            var additionalTotal = model.AdditionalExpenses.Sum(x => x.Amount);
            var grandTotal = itemTotal + additionalTotal - model.HeaderDiscount + model.HeaderTax;

            return Json(new
            {
                success = true,
                id = 0,
                message = "Expense saved successfully.",
                total = grandTotal
            });
        }

        // These endpoints can later be replaced by your real DB lookup methods.
        [HttpGet]
        public JsonResult Lookup()
        {
            return Json(BuildLookups(), JsonRequestBehavior.AllowGet);
        }

        private ExpenseLookupVm BuildLookups()
        {
            // Replace these demo records with your existing account/class/payee queries.
            var vm = new ExpenseLookupVm();

            vm.Classes.Add(new SelectOptionVm(1, "Operating Expense"));
            vm.Classes.Add(new SelectOptionVm(2, "Administrative Expense"));
            vm.Classes.Add(new SelectOptionVm(3, "Selling Expense"));

            vm.PaymentMethods.Add(new SelectOptionVm(1, "Cash"));
            vm.PaymentMethods.Add(new SelectOptionVm(2, "Bank Transfer"));
            vm.PaymentMethods.Add(new SelectOptionVm(3, "Credit Card"));
            vm.PaymentMethods.Add(new SelectOptionVm(4, "Vendor Credit"));

            vm.Accounts.Add(new SelectOptionVm(6120, "6120 - Office Supplies"));
            vm.Accounts.Add(new SelectOptionVm(6150, "6150 - Fuel Expense"));
            vm.Accounts.Add(new SelectOptionVm(6160, "6160 - Bank Charge"));
            vm.Accounts.Add(new SelectOptionVm(6200, "6200 - Advertising Expense"));
            vm.Accounts.Add(new SelectOptionVm(6250, "6250 - Utilities"));
            vm.Accounts.Add(new SelectOptionVm(6300, "6300 - Repairs & Maintenance"));

            vm.Payees.Add(new SelectOptionVm(1, "ABC Trading Co., Ltd."));
            vm.Payees.Add(new SelectOptionVm(2, "K Lube Express 598"));
            vm.Payees.Add(new SelectOptionVm(3, "General Supplier"));

            vm.Taxes.Add(new SelectOptionVm(0, "None"));
            vm.Taxes.Add(new SelectOptionVm(1, "VAT 10%"));
            vm.Taxes.Add(new SelectOptionVm(2, "VAT 15%"));

            return vm;
        }

        [Authorize(Roles = "Add Expenses, Edit Expenses, Add Bills, Edit Bills")]
        public ActionResult ExpenseCreate(string PrimaryFieldValue, string Status = "Draft", string No = "", string PaymentNo = "", string ExpenseNo = "", string PostedPaymentNo = "", string ExpensePaymentID = "", bool IsFromBills = false)
        {
            HTML html = new HTML();
            string sqlClass = $"select ClassID, ClassName from dbo.TblClass where SystemID = {html.SystemID} and WarehouseID = {html.WarehouseID} and DomainID like '%{html.DomainID}%'";
            ViewBag.DataClass = html.LoadData(sqlClass).Tables[0].ToJsonString();

            string sqlPayee = $@"SELECT 
    a.PayeeID, 
    a.Code, 
    a.[Name], 
    a.[Type], 
    a.OrderBy, 
    ISNULL(a.Gender, '') AS Gender,
    ISNULL(a.PhoneNumber, '') PhoneNumber
FROM (
    
    -- ========================
    -- Vendor
    -- ========================
    SELECT 
        v.VendorID AS PayeeID, 
        CAST(v.VendorCode AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code, 
        CAST(v.VendorName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name], 
        CAST('Vendor' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type], 
        1 AS OrderBy, 
        CAST(v.Gender AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
        CAST(v.ContactPhoneNumber AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
    FROM dbo.TblVendor v
    WHERE v.SystemID = {html.SystemID} AND v.WarehouseID = {html.WarehouseID}
    UNION ALL

    -- ========================
    -- Customer
    -- ========================
    SELECT 
        c.CustomerID AS PayeeID, 
        CAST(c.CustomerCode AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code, 
        CAST(c.CustomerName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name], 
        CAST('Customer' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type], 
        3 AS OrderBy, 
        CAST(c.CustomerGender AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
        CAST(c.CustomerContactMobileNumber AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
    FROM dbo.TblCustomer c
    WHERE ISNULL(c.CustomerCode, '') <> '' AND c.SystemID = {html.SystemID} AND c.WarehouseID = {html.WarehouseID}
    UNION ALL

    -- ========================
    -- Employee (Different DB)
    -- ========================
    SELECT 
        e.EmployeeID AS PayeeID, 
        CAST('' AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code, 
        CAST(e.FullName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name], 
        CAST('Employee' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type], 
        2 AS OrderBy, 
        CAST(g.GenderName AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
        CAST(p.PhoneNumber1 AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
    FROM HRMS_DB.dbo.Employees e
    INNER JOIN HRMS_DB.dbo.Gender g 
        ON g.GenderID = e.GenderID
    INNER JOIN HRMS_DB.dbo.Staffing_EmployeePhones p 
        ON p.EmployeeID = e.EmployeeID

) AS a
ORDER BY 
    a.OrderBy, 
    a.[Name] ASC;";
            ViewBag.DataPayee = EncryAndDecry.Encryp(sqlPayee); //html.LoadData(sqlPayee).Tables[0].ToJsonString();

            return View();
        }
    }
}