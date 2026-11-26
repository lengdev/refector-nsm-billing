using MVC;
using NSMSOLUTION.Models.ApprovalUtility;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Services
{
    public class TelegramNotifier : ITelegramNotifier
    {
        private readonly NotificationTelegram notificationTelegram;
        private SqlResult result;
        public TelegramNotifier()
        {
            notificationTelegram = new NotificationTelegram();
            result = new SqlResult();
        }
        public async Task<ApprovalRequest> SendApprovalNotificationAsync(ApprovalRequest request, string pdfPath = "")
        {
            // TODO: Send message with dynamic buttons
            try
            {
                var inlineKeyboard = CreateApprovalButtons(request);

                result = request.MessageType == "Topic"
                    ? await notificationTelegram.NotificationTelegramBotToTopic(
                        MsgText: "",
                        chatId: request.GroupId,
                        topicID: request.TopicId,
                        inlineKeyboard: request.ShowButtonCallback ? inlineKeyboard : null,
                        MessageID: request.MessageId,
                        TelegramMessage: request.Message,
                        Files: pdfPath,
                        Caption: request.Msg,
                        EditMessage: request.EditMessage,
                        DeleteTelegramMessageID: "",
                        IsReplyMsg: false,
                        newBotClient: request.BotToken
                    )
                    : await notificationTelegram.NotificationTelegramBot(
                        MsgText: "",
                        chatId: request.GroupId,
                        DestinationFolder: "",
                        PDFFile: pdfPath,
                        Caption: request.Msg,
                        IsSendFileWithCaption: false,
                        replyMarkupInlineKeyboardMarkup: true,
                        DBID: request.EntryID,
                        MessageID: request.MessageId,
                        TelegramMessage: request.Message,
                        inlineKeyboard: request.ShowButtonCallback ? inlineKeyboard : null,
                        edit: request.EditMessage,
                        messageThreadId: 0,
                        DeleteMessageID: "",
                        newBotClient: request.BotToken
                    );

                if (request.IsApprovalDifferentGroup)
                {
                    result = await notificationTelegram.NotificationTelegramBot(
                        MsgText: "",
                        chatId: request.GroupId2,
                        PDFFile: pdfPath,
                        Caption: request.Msg2,
                        DBID: request.EntryID,
                        replyMarkupInlineKeyboardMarkup: true,
                        inlineKeyboard: inlineKeyboard,
                        newBotClient: request.BotToken
                    );
                }

                request.LastTelegramMessageId = Utility.TryParse(result.Value, "int", 0);
                request.LastTelegramMessage = result.Caption;
                request.GroupId = result.ChatID;
                request.TopicId = result.TopicID;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return request;
        }

        public async Task SendErrorAsync(ApprovalRequest request, string errorMessage)
        {
            if (TelegramGroupType.Topic == request.MessageType)
            {
                await notificationTelegram.NotificationTelegramBotToTopic(
                    MsgText: errorMessage,
                    chatId: request.GroupId,
                    topicID: request.TopicId,
                    MessageID: request.MessageId,
                    TelegramMessage: request.Message,
                    IsReplyMsg: true,
                    newBotClient: request.BotToken
                );
            }
            else
            {
                await notificationTelegram.NotificationTelegramBot(
                    MsgText: errorMessage,
                    chatId: request.GroupId,
                    replyMarkupInlineKeyboardMarkup: true,
                    MessageID: request.MessageId,
                    TelegramMessage: request.Message,
                    edit: false,
                    newBotClient: request.BotToken
                );
            }
        }

        public InlineKeyboardMarkup CreateApprovalButtons(ApprovalRequest request)
        {
            var buttons = new List<InlineKeyboardButton>();

            if (request.AllowButtonMarkup)
            {
                // Add Approve only if not need check
                if (!request.IsNeedCheck)
                {
                    buttons.Add(InlineKeyboardButton.WithCallbackData(
                        text: $"Approve ✅",
                        callbackData: $"Approve{request.InlineKeyModule}~{request.EntryID}~{request.GroupId}"
                    ));
                }
                else
                {
                    buttons.Add(InlineKeyboardButton.WithCallbackData(
                        text: $"Check 📝",
                        callbackData: $"Check{request.InlineKeyModule}~{request.EntryID}~{request.GroupId}"
                    ));
                }

                // Always add Disapprove
                buttons.Add(InlineKeyboardButton.WithCallbackData(
                    text: $"Disapprove ❌",
                    callbackData: $"Disapprove{request.InlineKeyModule}~{request.EntryID}~{request.GroupId}"
                ));

                // Optional Reject button
                if (request.AllowReject)
                {
                    buttons.Add(InlineKeyboardButton.WithCallbackData(
                        text: $"Reject ❌",
                        callbackData: $"Reject{request.InlineKeyModule}~{request.EntryID}~{request.GroupId}"
                    ));
                }
            }

            return new InlineKeyboardMarkup(new[] { buttons.ToArray() });
        }

    }

    public class TelegramGroupType
    {
        public const string Topic = "Topic";
        public const string Normal = "Normal";
    }
}
