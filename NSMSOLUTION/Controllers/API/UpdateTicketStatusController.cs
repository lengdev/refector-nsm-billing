using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    [Authorize]
    public class UpdateTicketStatusController : ApiController
    {
        // GET api/<controller>
        [HttpPost]
        [Route("api/UpdateTicketStatus/UpdateStatus")]
        public async Task<HttpResponseMessage> UpdateStatus()
        {
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            InlineKeyboardMarkup inlineKeyboard = null;
            HTML html = new HTML();
            //bool FinishLayCable = false;
            //bool FinishInstall=false;
            SqlResult sqlResult = new SqlResult();
            int CustomerID = int.Parse(HttpContext.Current.Request.Form["CustomerID"].ToString());
            string TicketNumber = HttpContext.Current.Request.Form["TicketNumber"].ToString();
            string FirstName = HttpContext.Current.Request.Form["FirstName"].ToString();
            string LastName = HttpContext.Current.Request.Form["LastName"].ToString();
            string UserName = HttpContext.Current.Request.Form["UserName"].ToString();
            string Status = HttpContext.Current.Request.Form["Status"].ToString();
            string GroupName = HttpContext.Current.Request.Form["GroupName"].ToString();
            string TelegramGroupID = HttpContext.Current.Request.Form["TelegramGroupID"].ToString();
            string ChooseStatus = HttpContext.Current.Request.Form["ChooseStatus"].ToString();
            int MessageId = int.Parse(HttpContext.Current.Request.Form["MessageId"].ToString());
            string CHID = HttpContext.Current.Request.Form["ChatId"].ToString();
            string Caption = HttpContext.Current.Request.Form["Caption"].ToString();
            string MessageText = HttpContext.Current.Request.Form["MessageText"].ToString();

            string getMessageID = @"select TelegramMessageID,TelegramMessage from TblSupportOpenTicket where CustomerID = " + CustomerID + " and TicketNumber='" + TicketNumber + "'";

            string mygetMessageID = "", myTelegramMessage = ""; ;

            DataSet myds = html.LoadData(getMessageID);
            if (myds.Tables[0].Rows.Count > 0)
            {
                mygetMessageID = myds.Tables[0].Rows[0]["TelegramMessageID"].ToString();
                myTelegramMessage = myds.Tables[0].Rows[0]["TelegramMessage"].ToString();

            }

            //int ChatId = int.Parse(CHID);
            //if (Status == "Test")
            //{
            //    inlineKeyboard = new InlineKeyboardMarkup(new[]
            //                {
            //                    new []
            //                    {
            //                        InlineKeyboardButton.WithCallbackData(text: "▶️ Start Install", callbackData: "Test"),
            //                        //InlineKeyboardButton.WithCallbackData(text: "🏁 Finish Lay Cable", callbackData: "Finish Lay Cable ~ "+ CustomerID.ToString()+"~"+TicketNumber+"~"+TelegramGroupID),
            //                    }
            //                });
            //    sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(
            //    "hello this is testing...", -1689604171, 4, null, int.Parse(MessageId),"new testing...");//1903890245/7
            //    return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
            //}


            //sqlResult = await html.ApprovPO(POID, POStatus, "", FirstName, LastName, "@" + UserName);
            string sql = "select UserLoginID, FullName from dbo.TblUserLogin where TelegramUserName = @TelegramUserName";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"TelegramUserName","@"+UserName }
            };
            string Comment = "";
            //if (Status == "Start Lay Cable" || Status == "Start Install")
            //{
            if (Status == "Start Lay Cable")
            {
                Comment = @"<br>Dear Team,<br><br>Now this Customer is starting lay cable and when done @" + UserName + " will feedback.<br><br>" +
                    "Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO NOTIFICATION TICKET. " + TicketNumber + " " + DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt");
            }
            else if (Status == "Start Install")
            {
                Comment = @"<br>Dear Team,<br><br>Now this Customer is starting install and when done install by @" + UserName + " will feedback.<br><br>" +
                    "Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO NOTIFICATION TICKET. " + TicketNumber + " " + DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt");
            }
            else if (Status == "Finish Lay Cable")
            {
                Comment = "<br>Dear Team,<br><br>Now this Customer is finish lay cable by @" + UserName + ".<br><br>";
                Comment += "Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO NOTIFICATION TICKET. " + TicketNumber + " " + DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt");
            }
            else if (Status == "Finish Install")
            {
                Comment = "<br>Dear Team,<br><br>Now this Customer is finish install by @" + UserName + ".<br><br>";
                Comment += "Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO NOTIFICATION TICKET. " + TicketNumber + " " + DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy HH:mm:ss tt");
            }
            DataSet ds = html.LoadData(sql, false, para);

            if (ds.Tables[0].Rows.Count > 0)
            {
                string UserLoginID = ds.Tables[0].Rows[0]["UserLoginID"].ToString();
                string FullName = ds.Tables[0].Rows[0]["FullName"].ToString();
                if (Status == "Start Lay Cable" || Status == "Start Install")
                {
                    sql = @"
insert into TblSupportOpenTicketComment (StatusID, Status, PriorityID, Priority, OpenTicketID, SupportProjectID,
FromUserLoginID, FromUserName, ReplyFromUserLoginID, ReplyFromUserName, Comment, [Date],CreateBy,CreateDate,DescriptionChangeStatus,SystemID,WarehouseID)
select 
2 StatusID, 'In Progress' [Status], 1 PriorityID, 'Normal' [Priority],
OpenTicketID, SupportProjectID,
UserLoginID FromUserLoginID, CreateByFullName FromUserName, " + UserLoginID + @" ReplyFromUserLoginID,'" + FullName + @"' ReplyFromUserName,
'" + Comment + @"' Comment, '" + html.GetCurrentDate() + @"' [Date],'" + (html.UserFullName == "" ? "System" : html.UserFullName) + @"' CreateBy,
'" + html.GetCurrentDate() + @"' CreateDate, 'Change Status to In Progress by " + FullName + @"',
SystemID, WarehouseID from dbo.TblSupportOpenTicket where TicketNumber = '" + TicketNumber + "';" +
"update dbo.TblSupportOpenTicket set StatusID = 2, [Status] = 'In Progress' where TicketNumber ='" + TicketNumber + "' and CustomerID =" + CustomerID + ";" +
"select @@IDENTITY; select TelegramMessageID, TelegramGroupNameAndMessageID,TelegramMessage, OpenTicketID from dbo.TblSupportOpenTicket where TicketNumber = '" + TicketNumber + "';";
                }
                else if (Status == "Finish Lay Cable")
                {
                    sql = @"
insert into TblSupportOpenTicketComment (StatusID, Status, PriorityID, Priority, OpenTicketID, SupportProjectID,
FromUserLoginID, FromUserName, ReplyFromUserLoginID, ReplyFromUserName, Comment, [Date],CreateBy,CreateDate,DescriptionChangeStatus,SystemID,WarehouseID)
select 
4 StatusID, 'Feedback' [Status], 1 PriorityID, 'Normal' [Priority],
OpenTicketID, SupportProjectID,
UserLoginID FromUserLoginID, CreateByFullName FromUserName, " + UserLoginID + @" ReplyFromUserLoginID,'" + FullName + @"' ReplyFromUserName,
'" + Comment + @"' Comment, '" + html.GetCurrentDate() + @"' [Date],'" + (html.UserFullName == "" ? "System" : html.UserFullName) + @"' CreateBy,
'" + html.GetCurrentDate() + @"' CreateDate, 'Change Status to Feedback by " + FullName + @"',
SystemID, WarehouseID from dbo.TblSupportOpenTicket where TicketNumber = '" + TicketNumber + "';" +
"update dbo.TblSupportOpenTicket set StatusID = 4, [Status] = 'Feedback' where TicketNumber ='" + TicketNumber + "' and CustomerID =" + CustomerID + ";" +
"select @@IDENTITY; select TelegramMessageID, TelegramGroupNameAndMessageID,TelegramMessage, OpenTicketID from dbo.TblSupportOpenTicket where TicketNumber = '" + TicketNumber + "';";
                }
                else if (Status == "Finish Install")
                {
                    sql = @"
insert into TblSupportOpenTicketComment (StatusID, Status, PriorityID, Priority, OpenTicketID, SupportProjectID,
FromUserLoginID, FromUserName, ReplyFromUserLoginID, ReplyFromUserName, Comment, [Date],CreateBy,CreateDate,DescriptionChangeStatus,SystemID,WarehouseID)
select 
5 StatusID, 'Closed' [Status], 1 PriorityID, 'Normal' [Priority],
OpenTicketID, SupportProjectID,
UserLoginID FromUserLoginID, CreateByFullName FromUserName, " + UserLoginID + @" ReplyFromUserLoginID,'" + FullName + @"' ReplyFromUserName,
'" + Comment + @"' Comment, '" + html.GetCurrentDate() + @"' [Date],'" + (html.UserFullName == "" ? "System" : html.UserFullName) + @"' CreateBy,
'" + html.GetCurrentDate() + @"' CreateDate, 'Change Status to Closed by " + FullName + @"',
SystemID, WarehouseID from dbo.TblSupportOpenTicket where TicketNumber = '" + TicketNumber + "';" +
"update dbo.TblSupportOpenTicket set StatusID = 5, [Status] = 'Closed' where TicketNumber ='" + TicketNumber + "' and CustomerID =" + CustomerID + ";" +
"select @@IDENTITY; select TelegramMessageID, TelegramGroupNameAndMessageID,TelegramMessage, OpenTicketID from dbo.TblSupportOpenTicket where TicketNumber = '" + TicketNumber + "';";
                }
                sqlResult = html.SaveChangeData(sql, false, null, true, true);
                string SupportOpenTicketCommentID = sqlResult.Data.Tables[0].Rows[0][0].ToString();
                string[] TelegramGroupNameAndMessageID = mygetMessageID.ToString().Split('~');//sqlResult.Data.Tables[1].Rows[0]["TelegramGroupNameAndMessageID"].ToString().Split('~');
                string TelegramMessage = myTelegramMessage;//sqlResult.Data.Tables[1].Rows[0]["TelegramMessage"].ToString();
                string[] TelegramMessageID = sqlResult.Data.Tables[1].Rows[0]["TelegramMessageID"].ToString().Split('~');
                string OpenTicketID = sqlResult.Data.Tables[1].Rows[0]["OpenTicketID"].ToString();

                string TelegramGroupNameAndMessageIDs = "";
                int TopicID = 7;// TopicID = 2 for test//1903890245 = technical
                if (Status == "Start Lay Cable")
                {
                    foreach (string str in TelegramGroupNameAndMessageID)
                    {
                        string[] strs = str.Split(new char[] { '|' });
                        if (strs[0].ToString() == "-1903890245")
                            sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(Comment, -1903890245, 7, null, int.Parse(strs[1].ToString()), "", "", TelegramMessage);
                        else if (strs[0].ToString() == "-1002161346708")
                        {
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new []
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "🏁 Finish Lay Cable", callbackData: "Finish Lay Cable ~ "+ CustomerID.ToString()+"~"+TicketNumber+"~"+TelegramGroupID+"~"+Status),
                                }
                            });
                            sqlResult = await notificationTelegram.NotificationTelegramBot("", -1002161346708, "", "", Comment, true, true, 0, MessageId, TelegramMessage, inlineKeyboard, true);

                        }
                        //if (TelegramGroupID == strs[0].ToString())
                        //{

                        //}
                        //else
                        //{
                        //sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, null, true);
                        //  sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(Comment, int.Parse(CHID), TopicID, null, MessageId, "", "", TelegramMessage);
                        //}
                        if (TelegramGroupNameAndMessageIDs == "")
                            TelegramGroupNameAndMessageIDs = strs[0].ToString() + "|" + sqlResult.Value + "|" + Status;
                        else
                            TelegramGroupNameAndMessageIDs += "~" + strs[0].ToString() + "|" + sqlResult.Value + "|" + Status;
                    }

                    sql = "update dbo.TblSupportOpenTicketComment set TelegramGroupNameAndMessageID = @TelegramGroupNameAndMessageID where SupportOpenTicketCommentID = @SupportOpenTicketCommentID";
                    para.Clear();
                    para.Add("TelegramGroupNameAndMessageID", TelegramGroupNameAndMessageIDs);
                    para.Add("SupportOpenTicketCommentID", SupportOpenTicketCommentID);
                    sqlResult = html.SaveChangeData(sql, false, para);
                    sql = "update dbo.TblSupportOpenTicket set TelegramGroupNameAndMessageID=@TelegramGroupNameAndMessageID, TelegramMessage = @TelegramMessage where OpenTicketID = @OpenTicketID";
                    para.Clear();
                    para.Add("TelegramGroupNameAndMessageID", TelegramGroupNameAndMessageIDs);
                    para.Add("TelegramMessage", Comment);
                    para.Add("OpenTicketID", OpenTicketID);
                    sqlResult = html.SaveChangeData(sql, false, para);
                }
                else if (Status == "Start Install")
                {
                    foreach (string str in TelegramGroupNameAndMessageID)
                    {
                        string[] strs = str.Split(new char[] { '|' });
                        string gname = GroupName;
                        if (strs[0].ToString() == "") strs[0] = "-1903890245";
                        if (TelegramGroupID == strs[0].ToString())
                        {
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new []
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "🏁 Finish Install", callbackData: "Finish Install ~ "+ CustomerID.ToString()+"~"+TicketNumber+"~"+strs[0].ToString()+"~"+Status),
                                }
                            });
                            //TelegramMessage = sqlResult.Data.Tables[1].Rows[0]["TelegramMessage"].ToString();
                            //sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(Comment, int.Parse(TelegramGroupID), TopicID, inlineKeyboard, int.Parse(strs[1].ToString()), TelegramMessage);
                            sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(Comment, int.Parse(TelegramGroupID), TopicID, inlineKeyboard, MessageId, MessageText, "", Caption);
                        }
                        else
                        {
                            if (strs.Length > 2 && strs[2].ToString() == "Finish Lay Cable")
                            {
                                //sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse((strs[1].ToString() == "" ? "0" : strs[1].ToString())), TelegramMessage, null, false);
                                sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, Int64.Parse(CHID), "", "", "", false, true, 0, MessageId, TelegramMessage, null, false);
                            }
                            else
                            {
                                sqlResult = await notificationTelegram.NotificationTelegramBot("", -1002161346708, "", "", Comment, false, true, 0, int.Parse(strs[1].ToString() == "" ? "0" : strs[1].ToString()), TelegramMessage, null, true);
                                if (sqlResult.Value == "")
                                    sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, -1002161346708, "", "", "", false, true, 0, MessageId, TelegramMessage, null, false);
                                if (sqlResult.Value == "")
                                    sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(Comment, Int64.Parse(CHID), TopicID, inlineKeyboard, MessageId, TelegramMessage == "" ? Comment : TelegramMessage, "", TelegramMessage);
                            }
                            //sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, null, false);
                        }
                        if (TelegramGroupNameAndMessageIDs == "")
                            TelegramGroupNameAndMessageIDs = strs[0].ToString() + "|" + sqlResult.Value + "|" + Status;
                        else
                            TelegramGroupNameAndMessageIDs += "~" + strs[0].ToString() + "|" + sqlResult.Value + "|" + Status;
                    }

                    sql = "update dbo.TblSupportOpenTicketComment set TelegramGroupNameAndMessageID = @TelegramGroupNameAndMessageID where SupportOpenTicketCommentID = @SupportOpenTicketCommentID";
                    para.Clear();
                    para.Add("TelegramGroupNameAndMessageID", TelegramGroupNameAndMessageIDs);
                    para.Add("SupportOpenTicketCommentID", SupportOpenTicketCommentID);
                    sqlResult = html.SaveChangeData(sql, false, para);
                    sql = "update dbo.TblSupportOpenTicket set TelegramGroupNameAndMessageID=@TelegramGroupNameAndMessageID, TelegramMessage = @TelegramMessage where OpenTicketID = @OpenTicketID";
                    para.Clear();
                    para.Add("TelegramGroupNameAndMessageID", TelegramGroupNameAndMessageIDs);
                    para.Add("TelegramMessage", Comment);
                    para.Add("OpenTicketID", OpenTicketID);
                    sqlResult = html.SaveChangeData(sql, false, para);
                }
                else if (Status == "Finish Lay Cable")
                {
                    //FinishLayCable = true;                    
                    foreach (string str in TelegramGroupNameAndMessageID)
                    {
                        string[] strs = str.Split(new char[] { '|' });
                        if (strs[0] == "") strs[0] = "-1002161346708";
                        if (TelegramGroupID == strs[0])
                        {
                            TelegramMessage = sqlResult.Data.Tables[1].Rows[0]["TelegramMessage"].ToString();
                            sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, Int64.Parse(CHID), "", "", "", false, true, 0, MessageId, TelegramMessage, null, true);
                        }
                        else
                        {
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new []
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "▶️ Start Install", callbackData: "Start Install ~ "+ CustomerID.ToString()+"~"+TicketNumber+"~"+strs[0].ToString()+"~"+Status),
                                    //InlineKeyboardButton.WithCallbackData(text: "🏁 Finish Lay Cable", callbackData: "Finish Lay Cable ~ "+ CustomerID.ToString()+"~"+TicketNumber+"~"+TelegramGroupID),
                                }
                            });
                            //sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, inlineKeyboard, false);
                            sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(Comment, -1903890245, TopicID, inlineKeyboard, int.Parse(strs[1].ToString() == "" ? "0" : strs[1].ToString()), TelegramMessage, "", "", false);
                        }
                        if (TelegramGroupNameAndMessageIDs == "")
                            TelegramGroupNameAndMessageIDs = strs[0].ToString() + "|" + sqlResult.Value + "|" + Status;
                        else
                            TelegramGroupNameAndMessageIDs += "~" + strs[0].ToString() + "|" + sqlResult.Value + "|" + Status;
                    }

                    sql = "update dbo.TblSupportOpenTicketComment set TelegramGroupNameAndMessageID = @TelegramGroupNameAndMessageID where SupportOpenTicketCommentID = @SupportOpenTicketCommentID";
                    para.Clear();
                    para.Add("TelegramGroupNameAndMessageID", TelegramGroupNameAndMessageIDs);
                    para.Add("SupportOpenTicketCommentID", SupportOpenTicketCommentID);
                    sqlResult = html.SaveChangeData(sql, false, para);
                    sql = "update dbo.TblSupportOpenTicket set TelegramGroupNameAndMessageID=@TelegramGroupNameAndMessageID, TelegramMessage = @TelegramMessage where OpenTicketID = @OpenTicketID";
                    para.Clear();
                    para.Add("TelegramGroupNameAndMessageID", TelegramGroupNameAndMessageIDs);
                    para.Add("TelegramMessage", Comment);
                    para.Add("OpenTicketID", OpenTicketID);
                    sqlResult = html.SaveChangeData(sql, false, para);
                }
                else if (Status == "Finish Install")
                {
                    //FinishInstall = true;
                    foreach (string str in TelegramGroupNameAndMessageID)
                    {
                        string[] strs = str.Split(new char[] { '|' });
                        if (TelegramGroupID == strs[0].ToString())
                        {
                            //sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, null, true);
                            sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(Comment, int.Parse(TelegramGroupID), TopicID, null, MessageId, TelegramMessage, "", Caption);
                        }
                        else
                        {
                            TelegramMessage = sqlResult.Data.Tables[1].Rows[0]["TelegramMessage"].ToString();
                            sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, long.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString() == "" ? "0" : strs[1].ToString()), TelegramMessage, null, false);
                        }
                        if (TelegramGroupNameAndMessageIDs == "")
                            TelegramGroupNameAndMessageIDs = strs[0].ToString() + "|" + sqlResult.Value + "|" + Status;
                        else
                            TelegramGroupNameAndMessageIDs += "~" + strs[0].ToString() + "|" + sqlResult.Value + "|" + Status;
                    }

                    sql = "update dbo.TblSupportOpenTicketComment set TelegramGroupNameAndMessageID = @TelegramGroupNameAndMessageID where SupportOpenTicketCommentID = @SupportOpenTicketCommentID";
                    para.Clear();
                    para.Add("TelegramGroupNameAndMessageID", TelegramGroupNameAndMessageIDs);
                    para.Add("SupportOpenTicketCommentID", SupportOpenTicketCommentID);
                    sqlResult = html.SaveChangeData(sql, false, para);
                    sql = "update dbo.TblSupportOpenTicket set TelegramGroupNameAndMessageID=@TelegramGroupNameAndMessageID, TelegramMessage = @TelegramMessage where OpenTicketID = @OpenTicketID";
                    para.Clear();
                    para.Add("TelegramGroupNameAndMessageID", TelegramGroupNameAndMessageIDs);
                    para.Add("TelegramMessage", Comment);
                    para.Add("OpenTicketID", OpenTicketID);
                    sqlResult = html.SaveChangeData(sql, false, para);
                }




                //foreach (string str in TelegramGroupNameAndMessageID)
                //{
                //    string[] strs = str.Split(new char[] { '|' });

                //    if (!FinishInstall)
                //    {
                //        if (Status == "Start Install")
                //        {
                //            if (TelegramGroupID != strs[0].ToString())
                //            {
                //                sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, null, false);
                //                if (sqlResult.Value != "")
                //                {
                //                    if (TelegramGroupNameAndMessageIDs == "")
                //                        TelegramGroupNameAndMessageIDs = strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //                    else
                //                        TelegramGroupNameAndMessageIDs += "~" + strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //                }
                //            }
                //            else if(TelegramGroupID == strs[0].ToString())
                //            {
                //                sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, inlineKeyboard, true);
                //                if (sqlResult.Value != "")
                //                {
                //                    if (TelegramGroupNameAndMessageIDs == "")
                //                        TelegramGroupNameAndMessageIDs = strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //                    else
                //                        TelegramGroupNameAndMessageIDs += "~" + strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //                }
                //            }
                //        }
                //        else
                //        {
                //            if (TelegramGroupID != strs[0].ToString() && !FinishLayCable)
                //            {
                //                sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, null, true);
                //                if (sqlResult.Value != "")
                //                {
                //                    if (TelegramGroupNameAndMessageIDs == "")
                //                        TelegramGroupNameAndMessageIDs = strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //                    else
                //                        TelegramGroupNameAndMessageIDs += "~" + strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //                }
                //            }
                //            else if (TelegramGroupID != strs[0].ToString() && FinishLayCable)
                //            {
                //                sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, inlineKeyboard, false);
                //                if (sqlResult.Value != "")
                //                {
                //                    if (TelegramGroupNameAndMessageIDs == "")
                //                        TelegramGroupNameAndMessageIDs = strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //                    else
                //                        TelegramGroupNameAndMessageIDs += "~" + strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //                }
                //            }
                //            else if (FinishLayCable && strs[2].ToString() == ChooseStatus)
                //            {
                //                sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, null, true);
                //            }
                //            else if (!FinishLayCable)
                //            {
                //                sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, inlineKeyboard, true);
                //                if (sqlResult.Value != "")
                //                {
                //                    if (TelegramGroupNameAndMessageIDs == "")
                //                        TelegramGroupNameAndMessageIDs = strs[0].ToString() + "|" + sqlResult.Value + "|" + Status;
                //                    else
                //                        TelegramGroupNameAndMessageIDs += "~" + strs[0].ToString() + "|" + sqlResult.Value + "|" + Status;
                //                }
                //            }
                //        }
                //    }
                //    else
                //    {
                //        if (TelegramGroupID != strs[0].ToString())
                //        {
                //            sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, null, false);
                //            if (sqlResult.Value != "")
                //            {
                //                if (TelegramGroupNameAndMessageIDs == "")
                //                    TelegramGroupNameAndMessageIDs = strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //                else
                //                    TelegramGroupNameAndMessageIDs += "~" + strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //            }
                //        }
                //        else
                //        {
                //            sqlResult = await notificationTelegram.NotificationTelegramBot(Comment, int.Parse(strs[0].ToString()), "", "", "", false, true, 0, int.Parse(strs[1].ToString()), TelegramMessage, null, true);
                //            if (sqlResult.Value != "")
                //            {
                //                if (TelegramGroupNameAndMessageIDs == "")
                //                    TelegramGroupNameAndMessageIDs = strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //                else
                //                    TelegramGroupNameAndMessageIDs += "~" + strs[0].ToString() + "|" + sqlResult.Value + "|" + "";
                //            }
                //        }
                //    }
                //}
                //sql = "update dbo.TblSupportOpenTicketComment set TelegramGroupNameAndMessageID = @TelegramGroupNameAndMessageID where SupportOpenTicketCommentID = @SupportOpenTicketCommentID";
                //para.Clear();
                //para.Add("TelegramGroupNameAndMessageID", TelegramGroupNameAndMessageIDs);
                //para.Add("SupportOpenTicketCommentID", SupportOpenTicketCommentID);
                //sqlResult = html.SaveChangeData(sql, false, para);
                //sql = "update dbo.TblSupportOpenTicket set TelegramGroupNameAndMessageID=@TelegramGroupNameAndMessageID, TelegramMessage = @TelegramMessage where OpenTicketID = @OpenTicketID";
                //para.Clear();
                //para.Add("TelegramGroupNameAndMessageID", TelegramGroupNameAndMessageIDs);
                //para.Add("TelegramMessage", Comment);
                //para.Add("OpenTicketID", OpenTicketID);
                //sqlResult = html.SaveChangeData(sql, false, para);
            }
            //}
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
    }
}