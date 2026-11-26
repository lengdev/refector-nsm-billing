using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;
using TL;
using TLSharp.Core;
using TLSharp.Core.Utils;
using Utilities;
using Message = Telegram.Bot.Types.Message;

namespace MVC
{
    public class NotificationTelegram
    {
        private static WTelegram.Client client;
        private readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG", ".JPE", ".BMP", ".GIF", ".PNG" };
        private readonly static TelegramBotClient defualtBotClient = 
            new TelegramBotClient("9085979546:AAE7m7hRxIk5qJLMrJiD8sUm90H5v5OU_UQ");

        [Authorize, ValidateInput(false)]
        public async Task<SqlResult> NotificationTelegramBot(
            string MsgText,
            Int64 chatId = -827559974,
            string DestinationFolder = "",
            string PDFFile = "",
            string Caption = "",
            bool IsSendFileWithCaption = false,
            bool replyMarkupInlineKeyboardMarkup = false,
            int DBID = 0,
            int MessageID = 0,
            string TelegramMessage = "",
            InlineKeyboardMarkup inlineKeyboard = null,
            bool edit = true,
            int messageThreadId = 0,
            string DeleteMessageID = "",
            string newBotClient = "")
        {
            SqlResult sqlResult = new SqlResult();
            var botClient = newBotClient == "" 
                ? defualtBotClient
                : new TelegramBotClient(newBotClient);
            try
            {
                HTML html = new HTML();
                CallbackQuery callbackQuery = new CallbackQuery();
                ReceiverOptions receiverOptions = new ReceiverOptions()
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                };
                TelegramMessage = html.HtmlDecode(TelegramMessage);
                MsgText = html.HtmlDecode(MsgText);
                Caption = html.HtmlDecode(Caption);
                var cts = new CancellationTokenSource();

                //override here for local test
                chatId = -4510188155;

                if (DeleteMessageID != "")
                {
                    await botClient.DeleteMessage(
                        chatId: chatId,
                        messageId: int.Parse(DeleteMessageID),
                        cancellationToken: cts.Token);
                }

                if (MsgText != "")
                {
                    if (MessageID != 0 && edit == true)
                    {
                        await botClient.EditMessageText(
                            chatId: chatId,
                            messageId: MessageID,
                            cancellationToken: cts.Token,
                            text: TelegramMessage.Replace("\\n", "\n").Replace("<br>", "\n"),
                            parseMode: ParseMode.Html
                         );
                    }
                    MsgText = MsgText.Replace("\\n", "\n").Replace("<br>", "\n");
                    if (replyMarkupInlineKeyboardMarkup)
                    {
                        if (PDFFile != "")
                        {
                            string[] pdfs = PDFFile.Split('~');
                            if (pdfs.Length == 1)
                            {
                                string[] Captions = Caption.Split('~');
                                string desf = GetFullPath(PDFFile);
                                using (var fileStream = new FileStream(desf, FileMode.Open, FileAccess.Read))
                                {
                                    string filename = Path.GetFileName(desf);
                                    InputFileStream inputFile = new InputFileStream(fileStream, filename);
                                    try
                                    {

                                        var ext = Path.GetExtension(desf);
                                        if (ImageExtensions.Contains(ext.ToUpperInvariant()))
                                        {
                                            var messageSent = await botClient.SendPhoto(
                                                chatId: chatId,
                                                photo: inputFile,
                                                replyParameters: MessageID,
                                                caption: Captions[0].Replace("\\n", "\n").Replace("<br>", "\n") + "\n\n" + MsgText,
                                                parseMode: ParseMode.Html,
                                                replyMarkup: replyMarkupInlineKeyboardMarkup ? inlineKeyboard : null,
                                                cancellationToken: cts.Token
                                            );
                                            sqlResult.Value = messageSent.MessageId.ToString();
                                        }
                                        else
                                        {
                                            var messageSent = await botClient.SendDocument(
                                                chatId: chatId,
                                                document: inputFile,
                                                replyParameters: MessageID,
                                                caption: Captions[0].Replace("\\n", "\n").Replace("<br>", "\n") + "\n\n" + MsgText,
                                                parseMode: ParseMode.Html,
                                                replyMarkup: replyMarkupInlineKeyboardMarkup ? inlineKeyboard : null,
                                                cancellationToken: cts.Token
                                            );
                                            sqlResult.Value = messageSent.MessageId.ToString();
                                        }
                                    }
                                    catch (ApiRequestException ex)
                                    {
                                        if (ex.Message.Contains("group chat was upgraded to a supergroup chat"))
                                        {
                                            Console.WriteLine("The group chat was upgraded to a supergroup, and the chat ID needs to be updated.");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"An API request error occurred: {ex.Message}");
                                        }
                                    }
                                }
                                ;
                            }
                            else
                            {
                                IAlbumInputMedia[] inputMedia = new IAlbumInputMedia[pdfs.Length];
                                int i = 0;
                                string[] Captions = Caption.Split('~');
                                foreach (string str in pdfs)
                                {
                                    string desf = GetFullPath(str);
                                    Stream stream = System.IO.File.OpenRead(desf);
                                    string filename = Path.GetFileName(desf);
                                    inputMedia[i] = (i + 1) == pdfs.Length
                                        ? new Telegram.Bot.Types.InputMediaDocument(new InputFileStream(stream, filename))
                                        {
                                            Caption = Captions[i].Replace("\\n", "\n").Replace("<br>", "\n"),//+"\n\n"+ MsgText,
                                            ParseMode = ParseMode.Html
                                        }
                                        : new Telegram.Bot.Types.InputMediaDocument(new InputFileStream(stream, filename))
                                        {
                                            Caption = Captions[i].Replace("\\n", "\n").Replace("<br>", "\n"),
                                            ParseMode = ParseMode.Html
                                        };
                                    i++;
                                }
                                await botClient.SendMediaGroup(
                                    chatId: chatId,
                                    media: inputMedia,
                                    replyParameters: MessageID,
                                    cancellationToken: cts.Token
                                );
                                await botClient.SendMessage(
                                    chatId: chatId,
                                    parseMode: ParseMode.Html,
                                    text: MsgText,
                                    replyParameters: MessageID,
                                    replyMarkup: inlineKeyboard,
                                    cancellationToken: cts.Token
                                );
                            }
                            PDFFile = ""; Caption = "";
                        }
                        else
                        {
                            var message = botClient.SendMessage(
                            chatId: chatId,
                            parseMode: ParseMode.Html,
                            text: MsgText,
                            replyParameters: MessageID,
                            replyMarkup: inlineKeyboard,
                            cancellationToken: cts.Token);
                            message.Wait();
                            message.Dispose();
                            sqlResult.Value = message.Result.MessageId.ToString();
                        }
                    }
                    else
                    {
                        Task<Message> message = null;
                        if (messageThreadId != 0)
                        {
                            string cid = "-100" + chatId.ToString().Replace("-", string.Empty);
                            Int64 cids = Int64.Parse(cid);
                            message = botClient.SendMessage(
                            chatId: cids,
                            parseMode: ParseMode.Html,
                            text: MsgText,
                            messageThreadId: messageThreadId,
                            cancellationToken: cts.Token);
                        }
                        else
                        {
                            message = botClient.SendMessage(
                            chatId: chatId,
                            parseMode: ParseMode.Html,
                            text: MsgText,
                            cancellationToken: cts.Token);
                        }
                        message.Wait();
                        message.Dispose();
                        sqlResult.Value = message.Result.MessageId.ToString();
                    }

                }
                if (PDFFile != "")
                {
                    string[] pdfs = PDFFile.Split('~');
                    if (pdfs.Length == 1)
                    {
                        string desf = GetFullPath(PDFFile);
                        Stream stream = System.IO.File.OpenRead(desf);
                        string filename = Path.GetFileName(desf);
                        var inputFile = new InputFileStream(stream, filename);

                        if (MessageID != 0 && edit == true && TelegramMessage != "")
                        {
                            await botClient.EditMessageCaption(
                                chatId: chatId,
                                messageId: MessageID,
                                cancellationToken: cts.Token,
                                caption: TelegramMessage.Replace("\\n", "\n").Replace("<br>", "\n"),
                                parseMode: ParseMode.Html
                             );
                        }
                        var message = botClient.SendDocument(chatId: chatId,
                            document: inputFile,
                            replyParameters: MessageID,
                            caption: Caption == "" ? "វិក្កយបត្រ" : Caption.Replace("\\n", "\n").Replace("<br>", "\n"),
                            parseMode: ParseMode.Html,
                            replyMarkup: replyMarkupInlineKeyboardMarkup ? inlineKeyboard : null,
                            cancellationToken: cts.Token
                            );
                        message.Wait();
                        message.Dispose();
                        sqlResult.Value = message.Result.MessageId.ToString();
                        stream.Dispose();
                    }
                    else if (pdfs.Length > 1)
                    {
                        if (MessageID != 0 && edit == true)
                        {
                            await botClient.EditMessageCaption(
                                chatId: chatId,
                                messageId: MessageID,
                                cancellationToken: cts.Token,
                                caption: TelegramMessage.Replace("\\n", "\n").Replace("<br>", "\n"),
                                parseMode: ParseMode.Html
                             );
                        }
                        Message[] messages;
                        IAlbumInputMedia[] inputMedia = new IAlbumInputMedia[pdfs.Length];
                        int i = 0;
                        string[] Captions = Caption.Split('~');
                        foreach (string str in pdfs)
                        {
                            string localPath = html.ServerRoot;
                            string desf = GetFullPath(str);
                            Stream stream = System.IO.File.OpenRead(desf);
                            string filename = Path.GetFileName(desf);
                            inputMedia[i] = (i + 1) == pdfs.Length
                                ? new Telegram.Bot.Types.InputMediaDocument(new InputFileStream(stream, filename))
                                {
                                    Caption = Captions[i].Replace("\\n", "\n").Replace("<br>", "\n"),
                                    ParseMode = ParseMode.Html
                                }
                                : new Telegram.Bot.Types.InputMediaDocument(new InputFileStream(stream, filename))
                                {
                                    Caption = Captions[i].Replace("\\n", "\n").Replace("<br>", "\n"),
                                    ParseMode = ParseMode.Html
                                };
                            i++;
                        }
                        messages = await botClient.SendMediaGroup(chatId: chatId,
                            media: inputMedia,
                            replyParameters: MessageID,
                            cancellationToken: cts.Token
                        );
                    }
                }
                else if (Caption != "")
                {
                    try
                    {
                        if (MessageID != 0 && edit == true)
                        {
                            await botClient.EditMessageCaption(
                                chatId: chatId,
                                messageId: MessageID,
                                cancellationToken: cts.Token,
                                caption: TelegramMessage.Replace("\\n", "\n").Replace("<br>", "\n"),
                                parseMode: ParseMode.Html
                             );
                        }
                    }
                    catch (Exception)
                    {
                        if (MessageID != 0 && edit == true)
                        {
                            await botClient.EditMessageText(
                                chatId: chatId,
                                messageId: MessageID,
                                cancellationToken: cts.Token,
                                text: TelegramMessage.Replace("\\n", "\n").Replace("<br>", "\n"),
                                parseMode: ParseMode.Html
                             );
                        }
                    }
                    Caption = Caption.Replace("\\n", "\n").Replace("<br>", "\n");
                    if (replyMarkupInlineKeyboardMarkup)
                    {
                        var message = botClient.SendMessage(
                             chatId: chatId,
                             text: Caption,
                             replyParameters: MessageID,
                             replyMarkup: inlineKeyboard,
                             cancellationToken: cts.Token,
                             parseMode: ParseMode.Html
                        );
                        message.Wait();
                        message.Dispose();
                        sqlResult.Value = message.Result.MessageId.ToString();
                    }
                    else
                    {
                        Task<Message> message = null;
                        if (messageThreadId != 0)
                        {
                            string cid = "-100" + chatId.ToString().Replace("-", string.Empty);
                            Int64 cids = Int64.Parse(cid);
                            message = botClient.SendMessage(
                            chatId: cids,
                            parseMode: ParseMode.Html,
                            text: Caption,
                            messageThreadId: messageThreadId,
                            cancellationToken: cts.Token);
                        }
                        else
                        {
                            message = botClient.SendMessage(
                            chatId: chatId,
                            parseMode: ParseMode.Html,
                            text: Caption,
                            cancellationToken: cts.Token);
                        }
                        message.Wait();
                        message.Dispose();
                        sqlResult.Value = message.Result.MessageId.ToString();
                    }

                }
                cts.Cancel();
                sqlResult.Result = ResultType.Successful;
                sqlResult.Message = "Send Message Successful.";
                sqlResult.ChatID = chatId;
                sqlResult.TopicID = 0;
                sqlResult.Caption = Caption;
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;

                if (ex is AggregateException aggEx)
                {
                    var realEx = aggEx.Flatten().InnerException ?? aggEx;
                    sqlResult.Message = realEx.Message;
                }
                else
                {
                    sqlResult.Message = ex.Message;
                }
            }
            return sqlResult;
        }
        [Authorize, ValidateInput(false)]
        public async Task<SqlResult> NotificationTelegramBotToTopic(
            string MsgText,
            Int64 chatId = 0,
            int topicID = 0,
            InlineKeyboardMarkup inlineKeyboard = null,
            int MessageID = 0,
            string TelegramMessage = "",
            string Files = "",
            string Caption = "",
            bool EditMessage = true,
            string DeleteTelegramMessageID = "",
            bool IsReplyMsg = false,
            string newBotClient = "")
        {
            HTML html = new HTML();
            var botClient = newBotClient == "" 
                ? defualtBotClient 
                : new TelegramBotClient(newBotClient);

            TelegramMessage = html.HtmlDecode(TelegramMessage);
            MsgText = html.HtmlDecode(MsgText);
            Caption = html.HtmlDecode(Caption);

            //override here for local test
            chatId = -1002377280117; topicID = 2;

            SqlResult sqlResult = new SqlResult();
            int? messageThreadId = null;
            if (topicID != 0) messageThreadId = topicID;
            try
            {
                string cid = "-100" + chatId.ToString().Replace("-", string.Empty).Replace("100", string.Empty);
                Int64 cids = Int64.Parse(cid);

                CallbackQuery callbackQuery = new CallbackQuery();
                ReceiverOptions receiverOptions = new ReceiverOptions()
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                };
                var cts = new CancellationTokenSource();

                if (DeleteTelegramMessageID != "")
                {
                    await botClient.DeleteMessage(
                        chatId: topicID != 0 ? cids : chatId,
                        messageId: int.Parse(DeleteTelegramMessageID),
                        cancellationToken: cts.Token
                    );
                }
                Task<Message> message = null;

                if (IsReplyMsg)
                {
                    message = botClient.SendMessage(
                        chatId: topicID != 0 ? cids : chatId,
                        parseMode: ParseMode.Html,
                        text: MsgText.Replace("\\n", "\n").Replace("<br>", "\n").Replace("<br />", "\n"),
                    messageThreadId: topicID,
                        replyParameters: MessageID,
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cts.Token
                        );
                    await message;
                    message.Dispose();
                    sqlResult.Value = message.Result.MessageId.ToString();
                    sqlResult.Result = ResultType.Successful;
                    return sqlResult;
                }


                if (Files != "")
                {
                    string[] files = Files.Split('~');
                    if (files.Length == 1)
                    {
                        string localPath = html.ServerRoot;
                        string desf = GetFullPath(Files);
                        Stream stream = System.IO.File.OpenRead(desf);
                        string filename = Path.GetFileName(desf);
                        var inputFile = new InputFileStream(stream, filename);
                        if (MessageID != 0 && EditMessage)
                        {
                            await botClient.EditMessageCaption(
                                chatId: topicID != 0 ? cids : chatId,
                                messageId: MessageID,
                                cancellationToken: cts.Token,
                                caption: TelegramMessage.Replace("\\n", "\n").Replace("<br>", "\n").Replace("<br />", "\n"),
                                parseMode: ParseMode.Html
                             );
                        }

                        message = botClient.SendDocument(
                            chatId: topicID != 0 ? cids : chatId,
                            messageThreadId: messageThreadId,
                            document: inputFile,
                            replyParameters: MessageID,
                            caption: (Caption == "" ? "វិក្កយបត្រ" : Caption.Replace("\\n", "\n").Replace("<br>", "\n")).Replace("<br />", "\n"),
                            parseMode: ParseMode.Html,
                            replyMarkup: inlineKeyboard,
                            cancellationToken: cts.Token
                            );
                        message.Wait();
                        message.Dispose();
                        sqlResult.Value = message.Result.MessageId.ToString();
                    }
                    else if (files.Length > 1)
                    {
                        if (MessageID != 0 && EditMessage)
                        {
                            await botClient.EditMessageCaption(
                                chatId: topicID != 0 ? cids : chatId,
                                messageId: MessageID,
                                cancellationToken: cts.Token,
                                caption: TelegramMessage.Replace("\\n", "\n").Replace("<br>", "\n").Replace("<br />", "\n"),
                                parseMode: ParseMode.Html
                             );
                        }
                        Message[] messages;
                        IAlbumInputMedia[] inputMedia = new IAlbumInputMedia[files.Length];
                        int i = 0;
                        string[] Captions = Caption.Split('~');

                        foreach (string str in files)
                        {
                            string desf = GetFullPath(str);
                            Stream stream = System.IO.File.OpenRead(desf);
                            string filename = Path.GetFileName(desf);
                            inputMedia[i] = (i + 1) == files.Length
                                ? new Telegram.Bot.Types.InputMediaDocument(new InputFileStream(stream, filename))
                                {
                                    Caption = Captions[i].Replace("\\n", "\n").Replace("<br>", "\n").Replace("<br />", "\n"),
                                    ParseMode = ParseMode.Html
                                }
                                : new Telegram.Bot.Types.InputMediaDocument(new InputFileStream(stream, filename))
                                {
                                    Caption = Captions[i].Replace("\\n", "\n").Replace("<br>", "\n").Replace("<br />", "\n"),
                                    ParseMode = ParseMode.Html
                                };
                            i++;
                        }
                        messages = await botClient.SendMediaGroup(
                            chatId: topicID != 0 ? cids : chatId,
                            messageThreadId: messageThreadId,
                            media: inputMedia,
                            replyParameters: MessageID,
                            cancellationToken: cts.Token
                        );
                        sqlResult.Value = messages.Last().MessageId.ToString();
                    }
                }
                if (MsgText != "")
                {
                    MsgText = MsgText.Replace("\\n", "\n").Replace("<br>", "\n").Replace("<br />", "\n");
                    if (MessageID != 0 && EditMessage)
                    {
                        if (Caption != "")
                        {
                            Caption = Caption.Replace("\\n", "\n").Replace("<br>", "\n").Replace("<br />", "\n");

                            await botClient.EditMessageCaption(
                                chatId: topicID != 0 ? cids : chatId,
                                messageId: MessageID,
                                caption: Caption,
                                parseMode: ParseMode.Html,
                                replyMarkup: null,
                                cancellationToken: cts.Token
                            );
                        }
                        else
                        {
                            await botClient.EditMessageText(
                                chatId: topicID != 0 ? cids : chatId,
                                messageId: MessageID,
                                text: TelegramMessage.Replace("\\n", "\n").Replace("<br>", "\n").Replace("<br />", "\n"),
                                parseMode: ParseMode.Html,
                                replyMarkup: null,
                                cancellationToken: cts.Token
                            );
                        }
                    }
                    int? MsgID = null;
                    if (MessageID != 0) MsgID = MessageID;

                    message = botClient.SendMessage(
                        chatId: topicID != 0 ? cids : chatId,
                        parseMode: ParseMode.Html,
                        text: MsgText,
                        messageThreadId: topicID,
                        replyParameters: MsgID,
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cts.Token
                    );

                    await message;
                    message.Dispose();
                    sqlResult.Value = message.Result.MessageId.ToString();
                }
                cts.Cancel();
                sqlResult.Result = ResultType.Successful;
                sqlResult.Message = "Send Message Successful.";
                sqlResult.ChatID = chatId;
                sqlResult.TopicID = topicID;
                sqlResult.Caption = Caption;
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;

                if (ex is AggregateException aggEx)
                {
                    var realEx = aggEx.Flatten().InnerException ?? aggEx;
                    sqlResult.Message = realEx.Message;
                }
                else
                {
                    sqlResult.Message = ex.Message;
                }
            }
            return sqlResult;
        }
        public async Task<SqlResult> SendPhotoToGroup(IList<string> filePaths, string caption, long chatId, string newBotClient = "")
        {
            SqlResult sqlResult = new SqlResult();
            chatId = -4510188155;
            try
            {
                var bot = string.IsNullOrEmpty(newBotClient)
                    ? defualtBotClient
                    : new TelegramBotClient(newBotClient);

                var cts = new CancellationTokenSource();

                var paths = (filePaths ?? new List<string>())
                    .Where(p => !string.IsNullOrWhiteSpace(p) && System.IO.File.Exists(p))
                    .ToList();

                // 1. receipt text
                var textMsg = await bot.SendMessage(
                    chatId: chatId,
                    text: string.IsNullOrWhiteSpace(caption) ? "" : caption,
                    parseMode: ParseMode.Html,
                    cancellationToken: cts.Token);

                sqlResult.Value = textMsg.MessageId.ToString();

                // 2. each slip as its own photo, replying to the text
                foreach (var path in paths)
                {
                    using (var stream = System.IO.File.OpenRead(path))
                    {
                        await bot.SendPhoto(
                            chatId: chatId,
                            photo: new InputFileStream(stream, Path.GetFileName(path)),
                            cancellationToken: cts.Token);
                    }
                }

                sqlResult.Result = ResultType.Successful;
                sqlResult.Message = paths.Count == 0
                    ? "Message sent successfully."
                    : "Send Photos Successful.";
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                var real = ex is AggregateException agg ? (agg.Flatten().InnerException ?? agg) : ex;
                sqlResult.Message = real.Message;
            }
            return sqlResult;
        }

        public Task<SqlResult> SendPhotoToGroup(string filePath, string caption, long chatId, string newBotClient = "")
        {
            return SendPhotoToGroup(new List<string> { filePath }, caption, chatId, newBotClient);
        }
        public async Task<SqlResult> EditTelegramMessage(int messageId, string caption = "", string msgText = "", InlineKeyboardMarkup inlineKeyboard = null, Int64? chatId = null, int? topicID = null, string filePath = "", string newBotClient = "")
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            var botClient = newBotClient == "" 
                ? defualtBotClient 
                : new TelegramBotClient(newBotClient);

            long effectiveChatId = chatId ?? -1002377280117;
            int effectiveTopicID = topicID ?? 2;

            try
            {
                // Handle target ID logic
                long targetChatId = effectiveChatId;
                if (effectiveTopicID > 0)
                {
                    string cidStr = "-100" + effectiveChatId.ToString().Replace("-", "").Replace("100", "");
                    targetChatId = long.Parse(cidStr);
                }

                // Clean formatting
                string cleanCaption = html.HtmlDecode(caption).Replace("\\n", "\n").Replace("<br>", "\n").Replace("<br />", "\n");
                string cleanMsgText = html.HtmlDecode(msgText).Replace("\\n", "\n").Replace("<br>", "\n").Replace("<br />", "\n");

                if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                {
                    using (var fileStream = System.IO.File.OpenRead(filePath))
                    {
                        var fileName = Path.GetFileName(filePath);
                        var inputFile = Telegram.Bot.Types.InputFile.FromStream(fileStream, fileName);

                        // Explicitly use fully qualified name to solve CS0104 from image_5bbbbd.png
                        var media = new Telegram.Bot.Types.InputMediaDocument(inputFile)
                        {
                            Caption = string.IsNullOrEmpty(caption) ? null : cleanCaption,
                            ParseMode = Telegram.Bot.Types.Enums.ParseMode.Html
                        };

                        await botClient.EditMessageMedia(
                            chatId: targetChatId,
                            messageId: messageId,
                            media: media,
                            replyMarkup: inlineKeyboard);
                    }
                }
                else if (!string.IsNullOrEmpty(caption))
                {
                    await botClient.EditMessageCaption(
                        chatId: targetChatId,
                        messageId: messageId,
                        caption: cleanCaption,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                        replyMarkup: inlineKeyboard);
                }
                else if (!string.IsNullOrEmpty(msgText))
                {
                    await botClient.EditMessageText(
                        chatId: targetChatId,
                        messageId: messageId,
                        text: cleanMsgText,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                        replyMarkup: inlineKeyboard);
                }
                else
                {
                    sqlResult.Result = ResultType.Failed;
                    sqlResult.Message = "Either filePath, caption, or msgText must be provided.";
                    return sqlResult;
                }

                sqlResult.Result = ResultType.Successful;
                sqlResult.Message = "Edit Message Successful.";
                sqlResult.Value = messageId.ToString();
                sqlResult.ChatID = effectiveChatId;
                sqlResult.TopicID = effectiveTopicID;
                sqlResult.Caption = caption;
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;

                if (ex is AggregateException aggEx)
                {
                    var realEx = aggEx.Flatten().InnerException ?? aggEx;
                    sqlResult.Message = realEx.Message;
                }
                else
                {
                    sqlResult.Message = ex.Message;
                }
            }

            return sqlResult;
        }
        public string GetFullPath(string path)
        {
            try
            {
                return path = HttpContext.Current.Server.MapPath(@"/" + path);
            }
            catch
            {
                return path;
            }
        }
        [Authorize]
        public async Task<SqlResult> NotificationTelegramBotSurveyCustomer(string MsgText, int chatId = -827559974, int SurveyCustomerID = 0, string newBotClient = "")
        {
            HTML html = new HTML();
            MsgText = html.HtmlDecode(MsgText);
            SqlResult sqlResult = new SqlResult();
            var botClient = newBotClient == ""
                ? defualtBotClient
                : new TelegramBotClient(newBotClient);

            try
            {

                CallbackQuery callbackQuery = new CallbackQuery();
                var cts = new CancellationTokenSource();
                //chatId = -809264553; // 809264553 = for test

                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "✅ Can Install", callbackData: "Can Install ~ "+ SurveyCustomerID.ToString()),
                        InlineKeyboardButton.WithCallbackData(text: "❎ Cannot Install", callbackData: "Cannot Install ~ "+ SurveyCustomerID.ToString()),
                    }
                });

                if (MsgText != "")
                {
                    MsgText = MsgText.Replace("\\n", "\n").Replace("<br>", "\n");
                    await botClient.SendMessage(
                        chatId: chatId,
                        parseMode: ParseMode.Html,
                        text: MsgText,
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cts.Token
                     );
                }
                cts.Cancel();
                sqlResult.Result = ResultType.Successful;
                sqlResult.Message = "Send Message Successful.";
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        [Authorize]
        public async Task<SqlResult> NotificationToCustomer(string PhoneNumber, string FirstName, string LastName, int CustomerID, string MsgText, string DestinationFolder = "", string PDFFile = "", string LastInvoiceNo = "", string Subject = "", string Caption = "")
        {
            HTML html = new HTML();
            MsgText = html.HtmlDecode(MsgText);
            //PhoneNumber = "+855965565118";
            string FilePath = HttpContext.Current.Server.MapPath(@"/Uploads/");
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);
            DirectoryInfo dinfo = new DirectoryInfo(FilePath);
            FileSessionStore store = new FileSessionStore(dinfo);

            var cts = new CancellationTokenSource();

            SqlResult sqlResult = new SqlResult();
            //string FromPhoneNumber = "+855965565118"; //"+85561876777";//855965565118//85561588777 = finance team //85585715555 = laroth
            string FromPhoneNumber = "+85561876777";
            string sent = "Sent";
            try
            {
                var client = new TelegramClient(27294688, "708a468fb6811978a5ad88088a1a31ea", dcIpVersion: DataCenterIPVersion.OnlyIPv4, store: store, sessionUserId: "NSM_ISP_Telegram_SessionID_Finance");//, sessionUserId: "NSM_ISP_Telegram_SessionID"
                                                                                                                                                                                                            //var client = new TelegramClient(10369643, "98a5c16a746f18a7eda29bb08d428502", dcIpVersion: DataCenterIPVersion.OnlyIPv4, store: store, sessionUserId: "sothea");//, sessionUserId: "NSM_ISP_Telegram_SessionID"
                client.ConnectAsync().Wait();
                if (!client.IsUserAuthorized())
                {
                    var hash = await client.SendCodeRequestAsync(FromPhoneNumber, cts.Token);
                    var code = "42183";
                    await client.MakeAuthAsync(FromPhoneNumber, hash, code);
                    var passwordSettings = await client.GetPasswordSetting();
                    await client.MakeAuthWithPasswordAsync(passwordSettings, "Allaccount@NSM");//NSM@admin2023
                                                                                               //TLPassword settings = client.GetPasswordSetting().Result;
                                                                                               //TLPassword settings = client.GetPasswordSetting().Result;
                                                                                               //client.MakeAuthWithPasswordAsync(settings, "Thea8989", cts.Token).Wait();

                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Login to telegram successful.";
                }
                string desf = HttpContext.Current.Server.MapPath(@"~/Content/CompanyLogo/NSM-ISPLogo.png");
                //string desf = HttpContext.Current.Server.MapPath(@"https://nsmisp.com.kh/NSM_LOGO/NSM_ISP_Logo.png");
                TeleSharp.TL.Contacts.TLRequestImportContacts requestImportContacts = new TeleSharp.TL.Contacts.TLRequestImportContacts
                {
                    Contacts = new TLVector<TLInputPhoneContact>()
                };
                int NewUserId = 0;
                requestImportContacts.Contacts.Add(new TLInputPhoneContact()
                {
                    Phone = PhoneNumber,
                    FirstName = FirstName,//(LastName == "" ? FirstName : LastName),
                    LastName = "",
                    ClientId = CustomerID
                });
                var o = client.SendRequestAsync<TeleSharp.TL.Contacts.TLImportedContacts>(requestImportContacts);
                o.Wait();

                if (o.Result.Users.Count > 0)
                {
                    NewUserId = (o.Result.Users.First() as TLUser).Id;
                    o.Dispose();
                    //client.SendMessageAsync(new TLInputPeerUser() { UserId = NewUserId }, MsgText).Wait();
                    //client.SendUploadedPhoto(new TLInputPeerUser() { UserId = NewUserId }, fileResult.Result, MsgText).Wait();
                }
                else
                {
                    var result = client.GetContactsAsync();
                    result.Wait();
                    var user = result.Result.Users
                    .Where(x => x.GetType() == typeof(TLUser))
                    .Cast<TLUser>()
                    .FirstOrDefault(x => x.Phone == PhoneNumber);
                    result.Dispose();
                    if (user == null)
                    {
                        var contact = client.ImportContactsAsync(requestImportContacts.Contacts.ToList());
                        contact.Wait();
                        if (contact.Result.Users.Count > 0)
                        {
                            NewUserId = (contact.Result.Users.First() as TLUser).Id;
                        }
                        else
                        {
                            o = client.SendRequestAsync<TeleSharp.TL.Contacts.TLImportedContacts>(requestImportContacts);
                            o.Wait();
                            if (o.Result.Users.Count > 0)
                            {
                                NewUserId = (o.Result.Users.First() as TLUser).Id;
                            }
                            o.Dispose();
                        }
                        contact.Dispose();
                    }
                    else
                    {
                        NewUserId = user.Id;
                    }
                }
                if (NewUserId != 0)
                {
                    MsgText = MsgText.Replace("<b>", string.Empty).Replace("</b>", string.Empty);
                    sqlResult = PDFFile != ""
                        ? await NotificationTelegramBot("", NewUserId, "", PDFFile, MsgText)
                        : await NotificationTelegramBot(MsgText, NewUserId);
                    if (sqlResult.Result == ResultType.Failed)
                    {
                        Task<TLAbsUpdates> result = null;
                        if (PDFFile != "")
                        {
                            desf = PDFFile;
                            TLDocumentAttributeFilename f = new TLDocumentAttributeFilename
                            {
                                FileName = Path.GetFileName(PDFFile)
                            };
                            TLVector<TLAbsDocumentAttribute> attrs = new TLVector<TLAbsDocumentAttribute>
                            {
                                f
                            };

                            TLInputFile fileup = new TLInputFile
                            {
                                Name = PDFFile
                            };
                            var fileResult = client.UploadFile(PDFFile, new StreamReader(desf));
                            fileResult.Wait();
                            fileup = (TLInputFile)fileResult.Result;

                            result = client.SendUploadedDocument(new TLInputPeerUser() { UserId = NewUserId }, fileup, MsgText == "" ? "វិក្កយបត្រ" : MsgText, "application/pdf", attrs);
                            result.Wait();
                        }
                        else
                        {
                            result = client.SendMessageAsync(new TLInputPeerUser() { UserId = NewUserId }, MsgText);
                            result.Wait();
                        }
                        sqlResult.Value = result.Id.ToString();
                    }
                }
                else
                    sent = "Not Sent";
                string sql = @"insert into dbo.TblTelegramNotificationLog (NotificationDate, FromPhone, ToPhone, Message, MessageSatus, CustomerID, LastInvoiceNo, CreateDate, CreateBy, SystemID, WarehouseID,MessageID,Subject)
values (@NotificationDate, @FromPhone, @ToPhone, @Message, @MessageSatus, @CustomerID, @LastInvoiceNo,@CreateDate,@CreateBy,@SystemID,@WarehouseID,@MessageID,@Subject)";
                Dictionary<string, string> para = new Dictionary<string, string>
                {
                    {"NotificationDate", html.GetCurrentDate()},
                    {"FromPhone", FromPhoneNumber},
                    {"ToPhone", PhoneNumber},
                    {"Message", MsgText},
                    {"MessageSatus", sent},
                    {"CustomerID", CustomerID.ToString()},
                    {"LastInvoiceNo", LastInvoiceNo},
                    {"CreateDate", html.GetCurrentDate()},
                    {"CreateBy", html.UserFullName==""?"System":html.UserFullName},
                    {"SystemID", html.SystemID},
                    {"WarehouseID", html.WarehouseID},
                    {"MessageID", sqlResult.Value},
                    {"Subject", Subject},
                };
                html.SaveChangeData(sql, false, para);
                if (sent == "Not Sent")
                {
                    if (PDFFile != "")
                        await NotificationTelegramBotToTopic("", -1545132903, 494, null, 0, "", PDFFile, "Send Fail to Phone:" + PhoneNumber + "\n\n" + MsgText);
                    else
                        await NotificationTelegramBotToTopic("Send Fail to Phone:" + PhoneNumber + "\n\n" + MsgText, -1545132903, 494);
                }
                else
                {
                    if (PDFFile != "")
                        await NotificationTelegramBotToTopic("", -1849499041, 2, null, 0, "", PDFFile, "Send Successful to Phone:" + PhoneNumber + "\n\n" + MsgText);
                    else
                        await NotificationTelegramBotToTopic("Send Successful to Phone:" + PhoneNumber + "\n\n" + MsgText, -1849499041, 2, null, 0, "", PDFFile, MsgText);
                }

                client.Dispose();
                cts.Cancel();
                sqlResult.Result = ResultType.Successful;
                sqlResult.Message = "Send Message Successful.";
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
                cts.Cancel();
                sent = "Not Sent";
                string sql = @"insert into dbo.TblTelegramNotificationLog (NotificationDate, FromPhone, ToPhone, Message, MessageSatus, CustomerID, LastInvoiceNo, CreateDate, CreateBy, SystemID, WarehouseID,MessageID,Subject)
values (@NotificationDate, @FromPhone, @ToPhone, @Message, @MessageSatus, @CustomerID, @LastInvoiceNo,@CreateDate,@CreateBy,@SystemID,@WarehouseID,@MessageID,@Subject)";
                Dictionary<string, string> para = new Dictionary<string, string>
                {
                    {"NotificationDate", html.GetCurrentDate()},
                    {"FromPhone", FromPhoneNumber},
                    {"ToPhone", PhoneNumber},
                    {"Message", MsgText},
                    {"MessageSatus", sent},
                    {"CustomerID", CustomerID.ToString()},
                    {"LastInvoiceNo", LastInvoiceNo},
                    {"CreateDate", html.GetCurrentDate()},
                    {"CreateBy", html.UserFullName==""?"System":html.UserFullName},
                    {"SystemID", html.SystemID},
                    {"WarehouseID", html.WarehouseID},
                    {"MessageID", sqlResult.Value},
                    {"Subject", Subject},
                };
                html.SaveChangeData(sql, false, para);
                sqlResult.Message = "Send Fail to Phone:" + PhoneNumber;
                await NotificationTelegramBotToTopic("Send Fail to Phone:" + PhoneNumber + "\n\n" + MsgText, -1545132903, 494);
            }
            return sqlResult;
        }
        public string ConfigSothea(string what)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                switch (what)
                {
                    case "session_pathname": return $"{path}\\Uploads\\TelegramSession\\0965565118.session";// "D:\\NSM PROJECT\\NSM_ISP_BILLING\\NSMSOLUTION\\Uploads\\TelegramSession\\0965565118.session";
                    case "api_id": return "10369643";
                    case "api_hash": return "98a5c16a746f18a7eda29bb08d428502";
                    case "phone_number": return "+855965565118";
                    case "verification_code": Console.Write("Code: "); return Console.ReadLine();
                    //case "first_name": return "John";      // if sign-up is required
                    //case "last_name": return "Doe";        // if sign-up is required
                    case "password": return "Thea8989";     // if user has enabled 2FA
                    default: return null;                  // let WTelegramClient decide the default config
                }
            }
            catch (Exception)
            {
                client = null;
                client.Dispose();
                return null;
            }
        }
        public string ConfigFinance(string what)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                switch (what)
                {
                    case "session_pathname": return $"{path}\\Uploads\\TelegramSession\\061588777.session";
                    case "api_id": return "27294688";
                    case "api_hash": return "708a468fb6811978a5ad88088a1a31ea";
                    case "phone_number": return "+85561588777";
                    case "verification_code": Console.Write("Code: "); return Console.ReadLine();
                    //case "first_name": return "John";      // if sign-up is required
                    //case "last_name": return "Doe";        // if sign-up is required
                    case "password": return "Acc@Team@NSM";     // Allaccount@NSMif user has enabled 2FA
                    default: return null;                  // let WTelegramClient decide the default config
                }
            }
            catch (Exception)
            {
                client = null;
                client.Dispose();
                return null;
            }
        }
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            // Regular expression to check if the phone number contains only digits
            string pattern = @"^\d+$";
            return Regex.IsMatch(phoneNumber, pattern);
        }
        [Authorize]
        public async Task<SqlResult> AddCustomerTelegram(int CustomerID, string CustomerPhone, string FirstName, string LastName, string Memo, string CreateDate, string CreateBy, string UserID, string SystemID, string WarehouseID, string DomainID, bool AutoCreate = true, int TelegramCustomerID = 0)
        {

            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            try
            {
                var regex = new Regex(Regex.Escape("0"));
                string phoneNumber = CustomerPhone.Replace(" ", "").Replace("+", "").Replace("-", "");

                if (phoneNumber.StartsWith("0"))
                {
                    phoneNumber = "855" + phoneNumber.Substring(1);
                }
                if (html.IsValidPhoneNumber(phoneNumber))
                {
                    // Sothea Account
                    using (var clientSothea = new WTelegram.Client(ConfigSothea))
                    {
                        await clientSothea.LoginUserIfNeeded();

                        await clientSothea.Contacts_ImportContacts(new[]
                        {
                            new TL.InputPhoneContact
                            {
                                phone = phoneNumber,
                                first_name = FirstName,
                                last_name = LastName
                            }
                        });
                    }
                    // Finance Account
                    using (var clientFinance = new WTelegram.Client(ConfigFinance))
                    {
                        await clientFinance.LoginUserIfNeeded();

                        await clientFinance.Contacts_ImportContacts(new[]
                        {
                            new TL.InputPhoneContact
                            {
                                phone = phoneNumber,
                                first_name = FirstName,
                                last_name = LastName
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                client.Dispose();
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return sqlResult;
        }
        [Authorize]
        public async Task<SqlResult> CheckIsCustomerTelegram(string CustomerPhone)
        {
            CustomerPhone = CustomerPhone.Trim().Replace(" ", string.Empty).Replace("+855", "0");
            var regex = new Regex(Regex.Escape("0"));
            CustomerPhone = regex.Replace(CustomerPhone, "+855", 1);
            string FilePath = HttpContext.Current.Server.MapPath(@"/Uploads/");
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);
            DirectoryInfo dinfo = new DirectoryInfo(FilePath);
            FileSessionStore store = new FileSessionStore(dinfo);
            var cts = new CancellationTokenSource();
            SqlResult sqlResult = new SqlResult();
            //string FromPhoneNumber = "+855965565118";
            string IsUsingTelegram = "No";
            string FromPhoneNumber = "+85561876777";
            //string FromPhoneNumber = "+855965565118";
            try
            {
                //85561876777
                var client = new TelegramClient(27294688, "708a468fb6811978a5ad88088a1a31ea", dcIpVersion: DataCenterIPVersion.OnlyIPv4, store: store, sessionUserId: "NSM_ISP_Telegram_SessionID_Finance");//, sessionUserId: "NSM_ISP_Telegram_SessionID"
                //855965565118
                //var client = new TelegramClient(10369643, "98a5c16a746f18a7eda29bb08d428502", dcIpVersion: DataCenterIPVersion.OnlyIPv4, store: store, sessionUserId: "sothea");
                client.ConnectAsync().Wait();
                if (!client.IsUserAuthorized())
                {
                    var hash = await client.SendCodeRequestAsync(FromPhoneNumber, cts.Token);
                    var code = "75584";
                    await client.MakeAuthAsync(FromPhoneNumber, hash, code);
                    var passwordSettings = await client.GetPasswordSetting();
                    await client.MakeAuthWithPasswordAsync(passwordSettings, "Allaccount@NSM");//NSM@admin2023
                                                                                               //TLPassword settings = client.GetPasswordSetting().Result;
                                                                                               //TLPassword settings = client.GetPasswordSetting().Result;
                                                                                               //client.MakeAuthWithPasswordAsync(settings, "Thea8989", cts.Token).Wait();

                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Login to telegram successful.";
                }

                var result = await client.GetContactsAsync();
                var user = result.Users.Where(x => x.GetType() == typeof(TLUser)).Cast<TLUser>().FirstOrDefault(x => x.Phone == CustomerPhone);

                IsUsingTelegram = user == null ? "No" : "Yes";
                sqlResult.Value = IsUsingTelegram;
                client.Dispose();
                cts.Cancel();
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
                cts.Cancel();
            }
            return sqlResult;
        }
        [Authorize]
        public async Task<SqlResult> SendMsgCustomerTelegram(string CustomerPhone, string MsgText)
        {
            CustomerPhone = CustomerPhone.Trim().Replace(" ", string.Empty).Replace("+855", "0");
            var regex = new Regex(Regex.Escape("0"));
            CustomerPhone = regex.Replace(CustomerPhone, "855", 1);
            //CustomerPhone = CustomerPhone.Replace("+", string.Empty);
            string FilePath = HttpContext.Current.Server.MapPath(@"/Uploads/");
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);
            DirectoryInfo dinfo = new DirectoryInfo(FilePath);
            FileSessionStore store = new FileSessionStore(dinfo);
            var cts = new CancellationTokenSource();
            SqlResult sqlResult = new SqlResult();
            //string FromPhoneNumber = "+855965565118";
            string IsUsingTelegram = "No";
            string FromPhoneNumber = "+85561876777";
            //string FromPhoneNumber = "+855965565118";
            try
            {
                //85561876777
                var client = new TelegramClient(27294688, "708a468fb6811978a5ad88088a1a31ea", dcIpVersion: DataCenterIPVersion.OnlyIPv4, store: store, sessionUserId: "NSM_ISP_Telegram_SessionID_Finance");//, sessionUserId: "NSM_ISP_Telegram_SessionID"
                //855965565118
                //var client = new TelegramClient(10369643, "98a5c16a746f18a7eda29bb08d428502", dcIpVersion: DataCenterIPVersion.OnlyIPv4, store: store, sessionUserId: "sothea");
                client.ConnectAsync().Wait();
                if (!client.IsUserAuthorized())
                {
                    var hash = await client.SendCodeRequestAsync(FromPhoneNumber, cts.Token);
                    var code = "75584";
                    await client.MakeAuthAsync(FromPhoneNumber, hash, code);
                    var passwordSettings = await client.GetPasswordSetting();
                    await client.MakeAuthWithPasswordAsync(passwordSettings, "Allaccount@NSM");//NSM@admin2023
                                                                                               //TLPassword settings = client.GetPasswordSetting().Result;
                                                                                               //TLPassword settings = client.GetPasswordSetting().Result;
                                                                                               //client.MakeAuthWithPasswordAsync(settings, "Thea8989", cts.Token).Wait();

                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Login to telegram successful.";
                }
                var result = await client.GetContactsAsync();
                var user = result.Users.Where(x => x.GetType() == typeof(TLUser)).Cast<TLUser>().FirstOrDefault(x => x.Phone == CustomerPhone);

                if (user == null)
                {
                    IsUsingTelegram = "No";

                }
                else
                {
                    var NewUserId = (result.Users.First() as TLUser).Id;
                    IsUsingTelegram = "Yes";
                    HTML html = new HTML();
                    MsgText = html.HtmlDecode(MsgText);
                    sqlResult = await NotificationTelegramBot(MsgText, NewUserId);
                }
                sqlResult.Value = IsUsingTelegram;
                client.Dispose();
                cts.Cancel();
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
                cts.Cancel();
            }
            return sqlResult;
        }
    }
}
