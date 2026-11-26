using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using Telegram.Bot.Extensions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ILogger<UpdateHandler> _logger;
    private readonly TelegramConfig _telegramConfig;
    private readonly ITelegramBotClient _botClient;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;
    private readonly string _authUser;
    private readonly string _authPwd;
    private static readonly char[] MarkdownV2SpecialChars =
    "_*[]()~`>#+-=|{}.!".ToCharArray();
    private readonly ConcurrentDictionary<string, DateTime> _processedMessages = new();
    private static readonly TimeSpan DedupTtl = TimeSpan.FromMinutes(30);

    public UpdateHandler(
        ILogger<UpdateHandler> logger,
        TelegramConfig telegramConfig,
        ITelegramBotClient botClient
    )
    {
        _logger = logger;
        _telegramConfig = telegramConfig;
        _botClient = botClient
        ?? throw new ArgumentNullException(nameof(botClient), "A bot-specific ITelegramBotClient must be supplied.");
        _apiBaseUrl = _telegramConfig.GetSection("BASE_URL").Value ?? "";
        _apiKey = _telegramConfig.GetSection("API_KEY").Value ?? "";
        _authUser = _telegramConfig.GetSection("AUTH_USER").Value ?? "";
        _authPwd = _telegramConfig.GetSection("AUTH_PWD").Value ?? "";
        _httpClient = CreateHttpClient();
    }

    private static string EscapeMarkdownV2(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (MarkdownV2SpecialChars.Contains(c))
            {
                sb.Append('\\');
            }
            sb.Append(c);
        }
        return sb.ToString();
    }
    private async Task BotOnMessageReceived(Message message, string? messageText, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Get message: {messageText}");
        if (!string.IsNullOrEmpty(messageText) && messageText.StartsWith("/check", StringComparison.OrdinalIgnoreCase))
        {
            await SendGroupInfo(_botClient, message);
        }
    }
    public void LogInfo(string message, [CallerMemberName] string callerName = "")
    {
        _logger.LogInformation($"{callerName}: {message}");
    }
    private record UserInfo(string FirstName, string LastName, string UserName);
    private record MessageInfo(ChatId ChatId, int? MessageThreadId, int MessageId, string Caption, string Text, ChatType Type, bool IsTopicMessage, string CallbackQueryId)
    {
        public string GroupName => ChatId?.ToString() ?? string.Empty;
    };
    private HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("NSMISP-ApiKey", _apiKey);

        var credentials = Convert.ToBase64String(Encoding.Default.GetBytes($"{_authUser}:{_authPwd}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        return client;
    }
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var handler = update switch
            {
                { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
                // Handle text messages
                { Message: { Text: var messageText } message } => BotOnMessageReceived(message, messageText, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update, cancellationToken)
            };

            await handler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error while processing update {UpdateId}", update.Id);
        }
    }
    public Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Polling error from {Source}", source);
        return Task.CompletedTask;
    }
    private async Task<(bool Success, string Message)> PostAsync(string endpoint, Dictionary<string, string> data, CancellationToken cancellationToken)
    {
        string body = string.Empty;

        try
        {
            var content = new FormUrlEncodedContent(data);
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/{endpoint}", content, cancellationToken);

            body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (!_apiBaseUrl.Contains("localhost") && !_apiBaseUrl.Contains("127.0.0.1"))
                {
                    try
                    {
                        var localResp = await _httpClient.PostAsync($"http://localhost:62248/api/{endpoint}", new FormUrlEncodedContent(data), cancellationToken);
                        if (localResp.IsSuccessStatusCode)
                        {
                            var localBody = await localResp.Content.ReadAsStringAsync(cancellationToken);
                            return (true, localBody);
                        }
                    }
                    catch { }
                }

                _logger.LogError("POST {Endpoint} failed: {StatusCode} {Body}", endpoint, response.StatusCode, body);
                await _telegramConfig.ReportErrorAsync(
                    $"POST '{endpoint}' failed: '{response.StatusCode}' '{body}'",
                    new Exception(body),
                    CancellationToken.None
                );

                return (false, body);
            }

            return (true, body);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (!_apiBaseUrl.Contains("localhost") && !_apiBaseUrl.Contains("127.0.0.1"))
            {
                try
                {
                    var localResp = await _httpClient.PostAsync($"http://localhost:62248/api/{endpoint}", new FormUrlEncodedContent(data), cancellationToken);
                    if (localResp.IsSuccessStatusCode)
                    {
                        var localBody = await localResp.Content.ReadAsStringAsync(cancellationToken);
                        return (true, localBody);
                    }
                }
                catch { }
            }

            _logger.LogError(ex, "POST {Endpoint} threw an exception", endpoint);
            await _telegramConfig.ReportErrorAsync(
                $"POST '{endpoint}' threw an exception: '{ex.Message}'",
                ex,
                CancellationToken.None
            );

            return (false, ex.Message);
    }
    }
    private async Task SendGroupInfo(ITelegramBotClient bot, Telegram.Bot.Types.Message message)
    {
        var chat = message.Chat;
        var chatIdStr = EscapeMarkdownV2(chat.Id.ToString());
        var chatTitle = EscapeMarkdownV2(chat.Title ?? string.Empty);

        var lines = new List<string>
        {
            "`📂 Group Info`",
            $"`|- id:`{chatIdStr}",
            $"`|- type:` {chat.Type}",
            $"`|- title:` {chatTitle}",
            message.IsTopicMessage == true
                ? $"`|- IsTopic:` true\n`|- TopicId:` {message.MessageThreadId}"
                : "`|- IsTopic:` false"
        };

        try
        {
            int membersCount = await bot.GetChatMemberCount(chat.Id);
            lines.Add($"`|- members_count:` {membersCount}");
        }
        catch (Exception e)
        {
            lines.Add($"`|- members_count:` error \\({EscapeMarkdownV2(e.Message)}\\)");
            _logger.LogWarning(e, "Failed to get member count for chat {ChatId}", chat.Id);
        }

        await bot.SendMessage(
            chatId: chat.Id,
            text: string.Join("\n", lines),
            parseMode: ParseMode.MarkdownV2
        );
    }

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery?.Message == null || callbackQuery.From == null) return;

        string actionUsername = "@" + (callbackQuery.From.Username ?? "unknown");

        try
        {
            var rawData = callbackQuery.Data ?? string.Empty;
            var data = rawData.Split('~');

            if (data.Length == 0) return;

            var status = data[0].Trim();
            if (string.IsNullOrEmpty(status)) return;

            var messageKey = $"{callbackQuery.Message.Chat.Id}:{callbackQuery.Message.MessageId}:{rawData}";

            if (!_processedMessages.TryAdd(messageKey, DateTime.UtcNow))
            {
                try
                {
                    await _botClient.AnswerCallbackQuery(
                        callbackQueryId: callbackQuery.Id,
                        text: "✅ This request was already actioned.",
                        showAlert: false, // false = toast, true = blocking popup
                        cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AnswerCallbackQuery (dedup toast) failed for {CallbackQueryId} by {User}", callbackQuery.Id, actionUsername);
                    await _telegramConfig.ReportErrorAsync($"AnswerCallbackQuery (dedup toast) failed for {callbackQuery.Id} by {actionUsername}", ex, CancellationToken.None);
                }
                return;
            }

            try
            {
                await _botClient.AnswerCallbackQuery(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AnswerCallbackQuery (ack) failed for {CallbackQueryId} by {User}", callbackQuery.Id, actionUsername);
                await _telegramConfig.ReportErrorAsync($"AnswerCallbackQuery (ack) failed for {callbackQuery.Id} by {actionUsername}", ex, CancellationToken.None);
            }

            var authUsername = data.Length >= 3 ? data[2].Trim() : "";

            if (!string.IsNullOrEmpty(authUsername)
                && authUsername.Contains('@')
                && !string.Equals(authUsername, actionUsername, StringComparison.OrdinalIgnoreCase))
            {
                var allowUsernames = await GetAllowlistUsernamesAsync(cancellationToken);

                if (!allowUsernames.Any(u => string.Equals(u, actionUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    _processedMessages.TryRemove(messageKey, out _); // wasn't actually actioned, allow retry
                    await _botClient.SendMessage(
                        chatId: callbackQuery.Message.Chat.Id,
                        replyParameters: new ReplyParameters { MessageId = callbackQuery.Message.MessageId },
                        text: $"❌ {actionUsername}, You can't take this action due to no permission.",
                        cancellationToken: cancellationToken);
                    return;
                }
            }

            var processingMessage = await _botClient.SendMessage(
                chatId: callbackQuery.Message.Chat.Id,
                replyParameters: new ReplyParameters { MessageId = callbackQuery.Message.MessageId },
                text: "⏳ Please wait a moment...",
                cancellationToken: cancellationToken);

            var userInfo = new UserInfo(
                callbackQuery.From.FirstName,
                callbackQuery.From.LastName ?? string.Empty,
                callbackQuery.From.Username ?? string.Empty);

            var messageInfo = new MessageInfo(
                callbackQuery.Message.Chat.Id,
                callbackQuery.Message.MessageThreadId,
                callbackQuery.Message.MessageId,
                callbackQuery.Message.ToHtml() ?? callbackQuery.Message.Caption ?? string.Empty,
                callbackQuery.Message.Text ?? string.Empty,
                callbackQuery.Message.Chat.Type,
                callbackQuery.Message.IsTopicMessage,
                callbackQuery.Id);

            bool success = await ProcessCallback(status, data, userInfo, messageInfo, cancellationToken);

            try
            {
                await _botClient.DeleteMessage(
                    callbackQuery.Message.Chat.Id,
                    processingMessage.MessageId,
                    cancellationToken);
            }
            catch (Exception deleteEx)
            {
                _logger.LogWarning(deleteEx, "Failed to delete processing message {MessageId}", processingMessage.MessageId);
            }

            if (!success)
            {
                _processedMessages.TryRemove(messageKey, out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing callback query");
            await _telegramConfig.ReportErrorAsync($"Unhandled exception in BotOnCallbackQueryReceived by {actionUsername}", ex, CancellationToken.None);
        }
        finally
        {
            PurgeExpiredDedupEntries();
        }
    }

    private void PurgeExpiredDedupEntries()
    {
        var cutoff = DateTime.UtcNow - DedupTtl;
        foreach (var kvp in _processedMessages)
        {
            if (kvp.Value < cutoff)
                _processedMessages.TryRemove(kvp.Key, out _);
        }
    }

    private async Task<List<string>> GetAllowlistUsernamesAsync(CancellationToken ct)
    {
        var allowUsernames = new List<string>();
        try
        {
            await using var conn = _telegramConfig.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = "SELECT TelegramUserName FROM TblUserLogin WHERE UserLoginID IN (38, 126);";
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                var userName = reader["TelegramUserName"] as string;
                if (!string.IsNullOrWhiteSpace(userName))
                    allowUsernames.Add("@" + userName.TrimStart('@'));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking override username allowlist");
        }
        return allowUsernames;
    }
    private async Task<bool> ProcessCallback(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        switch (status)
        {
            case "ApprovePO":
            case "ApproveDraftPO":
            case "DisapproveDraftPO":
            case "DisapprovePO":
            case "DisCheckPO":
            case "CheckPO":
            case "ApproveEditPO":
            case "RejectEditPO":
                return await HandlePOApproval(status, data, userInfo, messageInfo, cancellationToken);


            case "Start Lay Cable":
            case "Start Install":
            case "Finish Lay Cable":
            case "Finish Install":
                return await HandleTicketStatusUpdate(status, data, userInfo, messageInfo, cancellationToken);


            case "ApproveInvDMC":
            case "DisapproveInvDMC":
            case "CheckedInvDMC":
            case "DisapproveCheckedInvDMC":
                return await HandleInvoiceDMCApproval(status, data, userInfo, messageInfo, cancellationToken);


            case "ApproveInv":
            case "CheckInv":
            case "DisapproveInv":
                return await HandleInvoiceApproval(status, data, userInfo, messageInfo, cancellationToken);


            case "ApproveItemRequest":
            case "DisapproveItemRequest":
            case "RejectItemRequest":
            case "ConfirmReceivedItemRequest":
                return await HandleItemRequestApproval(status, data, userInfo, messageInfo, cancellationToken);

            case "ApproveStockReturn":
            case "DisapproveStockReturn":
                return await HandleStockReturnApproval(status, data, userInfo, messageInfo, cancellationToken);

            case "Check":
            case "Approve":
            case "DisCheck":
            case "Disapprove":
            case "RejectCheck":
            case "Reject":
            case "NextApprove":
            case "verify_confirm":
            case "NextDisapprove":
                return await HandleRequestFormApproval(status, data, userInfo, messageInfo, cancellationToken);


            case "ApproveSales":
            case "DisapproveSales":
            case "CheckSales":
            case "DisCheckSales":
            case "ApproveRequestEdit":
            case "DisapproveRequestEdit":
                return await HandleSalesApproval(status, data, userInfo, messageInfo, cancellationToken);


            case "CheckExpense":
            case "DisapproveExpense":
                return await HandleExpenseApproval(status, data, userInfo, messageInfo, cancellationToken);

            case "ApprovePV":
            case "RejectPV":
                return await HandlePaymentVoucherApproval(status, data, userInfo, messageInfo, cancellationToken);


            case "CheckAdvExpense":
            case "DisCheckAdvExpense":
            case "DisapproveAdvExpense":
            case "ApproveAdvExpense":
                return await HandleAdvanceExpenseApproval(status, data, userInfo, messageInfo, cancellationToken);


            case "ApproveQuotation":
            case "DisapproveQuotation":
            case "DoneLayCable":
            case "NotDoneLayCable":
            case "FinishInstall":
                return await HandleQuotationApproval(status, data, userInfo, messageInfo, cancellationToken);


            case "ApproveInvoice":
            case "CheckInvoice":
            case "DisapproveInvoice":
                return await HandleInvoiceApproval2(status, data, userInfo, messageInfo, cancellationToken);


            // Fund Transfer
            case "ApproveFT":
            case "VerifyFT":
            case "CheckFT":
            case "DisapproveFT":
                return await HandleFundTransferApproval(status, data, userInfo, messageInfo, cancellationToken);


            // Journal Entry Approval
            case "ApproveJE":
            case "VerifyJE":
            case "CheckJE":
            case "DisapproveJE":
                return await HandleJournalEntryApproval(status, data, userInfo, messageInfo, cancellationToken);


            // Notification Payment Approval
            case "CheckNP":
            case "ApproveNP":
            case "RejectNP":
            case "DisapproveNP":
                return await HandleNotificationPaymentApproval(status, data, userInfo, messageInfo, cancellationToken);

            default:
                _logger.LogWarning("Unknown callback status: {Status}", status);
                return false;
        }
    }
    private async Task<bool> HandleJournalEntryApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        var ftStatus = status switch
        {
            "CheckJE" => "92",
            "VerifyJE" => "102",
            "ApproveJE" => "90",
            "DisapproveJE" => "91",
            _ => "0"
        };

        var (success, _) = await PostAsync("JournalEntryApproval/UpdateStatus", new()
        {
            ["TelegramGroupID"] = data[2].Trim(),
            ["JournalEntryID"] = data[1].Trim(),
            ["Status"] = ftStatus,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName ?? "",
            ["ChatID"] = messageInfo.ChatId.ToString(),
            ["MessageID"] = messageInfo.MessageId.ToString(),
            ["Comment"] = ""
        }, cancellationToken);

        return success;

    }
    private async Task<bool> HandleFundTransferApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        var ftStatus = status switch
        {
            "CheckFT" => "92",
            "VerifyFT" => "102",
            "ApproveFT" => "90",
            "DisapproveFT" => "91",
            _ => "0"
        };

        var (success, _) = await PostAsync("FundTransferApproval/ApprovalFT", new()
        {
            ["TelegramGroupID"] = data[2].Trim(),
            ["FTID"] = data[1].Trim(),
            ["Status"] = ftStatus,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName ?? "",
            ["ChatID"] = messageInfo.ChatId.ToString(),
            ["MessageID"] = messageInfo.MessageId.ToString(),
            ["Comment"] = ""
        }, cancellationToken);

        return success;
    }
    private async Task<bool> HandlePOApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        var poStatus = status switch
        {
            "CheckPO"
                or "DisCheckPO"
                => "CheckPO",
            "ApprovePO"
                or "DisapprovePO"
                or "ApproveDraftPO"
                or "DisapproveDraftPO"
                or "ApproveEditPO"
                or "RejectEditPO"
                => "ApprovePO",
            _ => ""
        };

        var requestData = new Dictionary<string, string>
        {
            ["POID"] = data[1].Trim(),
            ["CallBackStatus"] = status,
            ["POStatus"] = poStatus,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName,
            ["ChatID"] = messageInfo.ChatId.ToString(),
            ["MessageID"] = messageInfo.MessageId.ToString(),
            ["Message"] = messageInfo.Caption.ToString() ?? ""

        };

        var (success, _) = await PostAsync("POApproval", requestData, cancellationToken);
        return success;
    }
    private async Task<bool> HandleTicketStatusUpdate(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        if (data.Length < 5) return false;

        var requestData = new Dictionary<string, string>
        {
            ["CustomerID"] = data[1].Trim(),
            ["TicketNumber"] = data[2].Trim(),
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName,
            ["Status"] = status,
            ["GroupName"] = messageInfo.GroupName,
            ["TelegramGroupID"] = data[3].Trim(),
            ["ChooseStatus"] = data[4].Trim(),
            ["MessageId"] = messageInfo.MessageId.ToString(),
            ["ChatId"] = messageInfo.ChatId.ToString(),
            ["Caption"] = messageInfo.Caption,
            ["MessageText"] = messageInfo.Text
        };

        var (success, _) = await PostAsync("UpdateTicketStatus/UpdateStatus", requestData, cancellationToken);

        return success;
    }
    private async Task<bool> HandleInvoiceDMCApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        if (data.Length < 4) return false;

        if (data[3] != "@" + userInfo.UserName)
        {
            await _botClient.AnswerCallbackQuery(
                messageInfo.CallbackQueryId,
                $"You cannot do this action. Only user name: {data[3]} can do this action.",
                cancellationToken: cancellationToken
            );
            return false;
        }

        var approvalStatus = status switch
        {
            "ApproveInvDMC" or "CheckedInvDMC" => "Approve",
            "DisapproveInvDMC" or "DisapproveCheckedInvDMC" => "Disapprove",
            _ => string.Empty
        };

        var requestData = new Dictionary<string, string>
        {
            ["CustomerID"] = data[1].Trim(),
            ["InvoiceNo"] = data[2].Trim(),
            ["Status"] = approvalStatus,
            ["MessageId"] = messageInfo.MessageId.ToString(),
            ["MessageText"] = messageInfo.Text,
            ["From"] = userInfo.UserName
        };

        if (status == "DisapproveCheckedInvDMC" || status == "CheckedInvDMC")
        {
            await HandleSpecialInvoiceDMCApproval(status, data, userInfo, messageInfo, cancellationToken);
            var (success, _) = await PostAsync("InvoiceToDMCTAXApproval/SendApprovalInvoice", requestData, cancellationToken);
            return success;
        }
        else
        {
            var (success, _) = await PostAsync("InvoiceToDMCTAXApproval/Approval", requestData, cancellationToken);
            return success;
        }
    }
    private async Task<bool> HandleSpecialInvoiceDMCApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        await _botClient.SendChatAction(
            chatId: messageInfo.ChatId,
            action: ChatAction.Typing,
            cancellationToken: cancellationToken
        );

        await Task<bool>.Delay(500, cancellationToken);

        await _botClient.EditMessageCaption(
            chatId: messageInfo.ChatId,
            replyMarkup: null,
            messageId: messageInfo.MessageId,
            caption: messageInfo.Caption,
            cancellationToken: cancellationToken
        );

        var actionText = status == "DisapproveCheckedInvDMC" ? "disapproved" : "checked";
        var message = $"Dear Team,\n\nNow, this invoice is {actionText} by: @{userInfo.UserName}.\n\n" +
                     (status == "DisapproveCheckedInvDMC" ? "Please revise this invoice.\n\n" : "") +
                     "Thank you and best regards,\n\nNSM ISP BILLING SYSTEM AUTO SEND Request approve Invoice to DMC & TAX System.#" + data[2].Trim();

        await _botClient.SendMessage(
            chatId: messageInfo.ChatId,
            replyParameters: new ReplyParameters { MessageId = messageInfo.MessageId },
            text: message,
            cancellationToken: cancellationToken
        );

        await _botClient.AnswerCallbackQuery(
            messageInfo.CallbackQueryId,
            $"You {actionText} this.",
            cancellationToken: cancellationToken
        );

        return true;
    }
    private async Task<bool> HandleInvoiceApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        if (data.Length < 3) return false;

        var statusId = status switch
        {
            "ApproveInv" => "90",
            "CheckInv" => "92",
            "DisapproveInv" => "91",
            _ => "0"
        };

        var requestData = new Dictionary<string, string>
        {
            ["CustomerID"] = data[1].Trim(),
            ["InvoiceNo"] = data[2].Trim(),
            ["StatusID"] = statusId,
            ["UserName"] = userInfo.UserName
        };

        var (success, _) = await PostAsync("InvoiceToDMCTAXApproval/SendApprovalInvoiceCISP", requestData, cancellationToken);

        return success;
    }
    private async Task<bool> HandleItemRequestApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        var approvalStatus = status switch
        {
            "ApproveItemRequest" => "Approved",
            "DisapproveItemRequest" => "Disapproved",
            "RejectItemRequest" => "Rejected",
            "ConfirmReceivedItemRequest" => "ConfirmReceived",
            _ => string.Empty
        };

        var requestData = new Dictionary<string, string>
        {
            ["RequestItemID"] = data[1].Trim(),
            ["Status"] = approvalStatus,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName,
            ["TelegramGroupID"] = messageInfo.ChatId.ToString(),
            ["TelegramGroupTopicID"] = messageInfo.MessageThreadId?.ToString() ?? "",
            ["TelegramMessageID"] = messageInfo.MessageId.ToString() ?? "",
            ["TelegramMessage"] = messageInfo.Caption.ToString() ?? "",
            ["TelegramGroupType"] = messageInfo.Type.ToString() ?? "",
            ["TelegramMessageType"] = messageInfo.IsTopicMessage ? "Topic" : "",
        };

        var (success, _) = await PostAsync("ItemRequestApproval", requestData, cancellationToken);

        return success;
    }
    private async Task<bool> HandleStockReturnApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        var approvalStatus = status switch
        {
            "ApproveStockReturn" => "Approved",
            "DisapproveStockReturn" => "Disapproved",
            _ => string.Empty
        };

        var requestData = new Dictionary<string, string>
        {
            ["RequestItemID"] = data[1].Trim(),
            ["Status"] = approvalStatus,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName,
            ["TelegramGroupID"] = messageInfo.ChatId.ToString(),
            ["TelegramGroupTopicID"] = messageInfo.MessageThreadId?.ToString() ?? "",
            ["TelegramMessageID"] = messageInfo.MessageId.ToString() ?? "",
            ["TelegramMessage"] = messageInfo.Caption.ToString() ?? "",
            ["TelegramGroupType"] = messageInfo.Type.ToString() ?? "",
            ["TelegramMessageType"] = messageInfo.IsTopicMessage ? "Topic" : "",
        };

        var (success, _) = await PostAsync("StockReturnApproval", requestData, cancellationToken);

        return success;
    }
    private async Task<bool> HandleRequestFormApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        LogInfo("processing...");
        var statusReq = status switch
        {
            "Check"
                or "DisCheck"
                or "RejectCheck"
                => "CheckRequest",
            "Approve"
                or "Disapprove"
                or "Reject"
                or "NextApprove"
                or "NextDisapprove"
                => "ApprovalRequest",
            _ => ""
        };

        var requestData = new Dictionary<string, string>
        {
            ["RequestFormID"] = data[1].Trim(),
            ["RequestStatus"] = statusReq,
            ["CallBackStatus"] = status,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName,
            ["TelegramGroupID"] = messageInfo.ChatId.ToString(),
            ["TelegramMessageID"] = messageInfo.MessageId.ToString() ?? "",
            ["TelegramMessage"] = messageInfo.Caption.ToString() ?? ""
        };

        var endpoint = "RequestFormApproval";

        var (success, _) = await PostAsync(endpoint, requestData, cancellationToken);

        return success;
    }
    private async Task<bool> HandleSalesApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        LogInfo("processing...");

        var actionCallBack = status switch
        {
            "CheckSales"
                or "DisCheckSales"
                => "Check",
            "ApproveSales"
                or "DisapproveSales"
                => "Approve",
            "ApproveRequestEdit"
                or "DisapproveRequestEdit"
                => "RequestEdit",
            _ => ""
        };

        var requestData = new Dictionary<string, string>
        {
            ["SalesID"] = data[1].Trim(),
            ["ActionCallBack"] = actionCallBack,
            ["StatusKey"] = status,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName,
            ["TelegramGroupID"] = messageInfo.ChatId.ToString(),
            ["TelegramGroupTopicID"] = messageInfo.MessageThreadId?.ToString() ?? "",
            ["TelegramMessageID"] = messageInfo.MessageId.ToString() ?? "",
            ["TelegramMessage"] = messageInfo.Caption.ToString() ?? "",
            ["TelegramGroupType"] = messageInfo.Type.ToString() ?? "",
            ["TelegramMessageType"] = messageInfo.IsTopicMessage ? "Topic" : "",
        };

        var (success, _) = await PostAsync("SalesApproval", requestData, cancellationToken);

        return success;
    }
    private async Task<bool> HandleExpenseApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        if (data.Length < 4) return false;

        var allowUsernames = await GetAllowlistUsernamesAsync(cancellationToken);
        bool isAllowUser = allowUsernames.Any(u => string.Equals(u, "@" + userInfo.UserName, StringComparison.OrdinalIgnoreCase)
                                                || string.Equals(u, userInfo.UserName, StringComparison.OrdinalIgnoreCase));

        if (!isAllowUser && data[3] != "@" + userInfo.UserName)
        {
            await _botClient.AnswerCallbackQuery(
                messageInfo.CallbackQueryId,
                $"You cannot do this action. Only user name: {data[3]} can do this action.",
                cancellationToken: cancellationToken
            );
            return false;
        }

        var requestData = new Dictionary<string, string>
        {
            ["ExpenseID"] = data[1].Trim(),
            ["ExpenseType"] = data[2].Trim(),
            ["Status"] = status,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName,
            ["ChatID"] = messageInfo.ChatId.ToString(),
            ["MessageID"] = messageInfo.MessageId.ToString(),
            ["ExpensesType"] = "ExpensesCheck"
        };

        var (success, _) = await PostAsync("ExpensesApproval", requestData, cancellationToken);

        return success;
    }
    private async Task<bool> HandlePaymentVoucherApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        if (data.Length < 3) return false;

        var allowUsernames = await GetAllowlistUsernamesAsync(cancellationToken);
        bool isAllowUser = allowUsernames.Any(u => string.Equals(u, "@" + userInfo.UserName, StringComparison.OrdinalIgnoreCase)
                                                || string.Equals(u, userInfo.UserName, StringComparison.OrdinalIgnoreCase));

        if (!isAllowUser && data[2] != "@" + userInfo.UserName)
        {
            await _botClient.AnswerCallbackQuery(
                messageInfo.CallbackQueryId,
                $"You cannot do this action. Only user name: {data[2]} can do this action.",
                cancellationToken: cancellationToken
            );
            return false;
        }

        var requestData = new Dictionary<string, string>
        {
            ["ExpenseID"] = data[1].Trim(),
            ["Status"] = status
        };

        var (success, _) = await PostAsync("ApprovalPaymentVoucher", requestData, cancellationToken);

        return success;
    }
    private async Task<bool> HandleAdvanceExpenseApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        var expensesType = status switch
        {
            "CheckAdvExpense" or "DisCheckAdvExpense" => "ExpensesAdvCheck",
            "ApproveAdvExpense" or "DisapproveAdvExpense" => "ExpensesAdvApproval",
            _ => "ExpensesAdvCheck"
        };

        var requestData = new Dictionary<string, string>
        {
            ["ExpenseID"] = data[1].Trim(),
            ["ExpenseType"] = string.Empty,
            ["Status"] = status,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName,
            ["ChatID"] = messageInfo.ChatId.ToString(),
            ["MessageID"] = messageInfo.MessageId.ToString(),
            ["TopicID"] = messageInfo.MessageThreadId?.ToString() ?? "0",
            ["Message"] = messageInfo.Caption,
            ["ExpensesType"] = expensesType
        };

        var (success, _) = await PostAsync("ExpensesAdvApproval", requestData, cancellationToken);

        return success;
    }
    private async Task<bool> HandleQuotationApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        string endpoint = status switch
        {
            "DoneLayCable" or "NotDoneLayCable" => "DoneLayCable",
            "FinishInstall" => "FinishInstallService",
            _ => "QuotationApproval"
        };

        var (success, _) = await PostAsync(endpoint, new()
        {
            ["InvoiceNo"] = data[1].Trim(),
            ["CustomerID"] = data[2],
            ["Status"] = status,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName,
            ["ChatID"] = messageInfo.ChatId.ToString(),
            ["MessageID"] = messageInfo.MessageId.ToString(),
            ["Message"] = messageInfo.Caption
        }, cancellationToken);

        return success;
    }
    private async Task<bool> HandleInvoiceApproval2(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        if (data.Length < 2) return false;

        var (success, _) = await PostAsync("InvoiceApproval", new()
        {
            ["InvoiceNo"] = data[1].Trim(),
            ["CustomerID"] = data[2],
            ["Status"] = status,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName,
            ["ChatID"] = messageInfo.ChatId.ToString(),
            ["TopicID"] = messageInfo.MessageThreadId?.ToString() ?? "0",
            ["MessageID"] = messageInfo.MessageId.ToString(),
            ["Message"] = messageInfo.Caption
        }, cancellationToken);

        return success;
    }
    private async Task<bool> HandleNotificationPaymentApproval(string status, string[] data, UserInfo userInfo, MessageInfo messageInfo, CancellationToken cancellationToken)
    {
        if (data.Length < 2) return false;

        var (success, _) = await PostAsync("NotificationPaymentApproval/UpdateStatus", new Dictionary<string, string>()
        {
            ["NotificationPaymentID"] = data[1].Trim(),
            ["Status"] = status,
            ["FirstName"] = userInfo.FirstName,
            ["LastName"] = userInfo.LastName,
            ["UserName"] = userInfo.UserName ?? "",
            ["ChatID"] = messageInfo.ChatId.ToString(),
            ["TopicID"] = messageInfo.MessageThreadId?.ToString() ?? "0",
            ["MessageID"] = messageInfo.MessageId.ToString(),
            ["Message"] = messageInfo.Caption ?? "",
            ["Comment"] = ""
        }, cancellationToken);

        return success;
    }

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
