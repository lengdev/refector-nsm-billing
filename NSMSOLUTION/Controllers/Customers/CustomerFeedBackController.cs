using MVC;
using Newtonsoft.Json.Linq;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.Customers
{
    [RoutePrefix("api/CustomerFeedback")]
    public class CustomerFeedbackController : ApiController
    {
        private static readonly string[] FlowTeams = { "Sales Team", "Account Team", "Technical Team" };
        private static readonly string[] FlowStatuses = { "Pending", "In Progress", "Done" };
        private const long TelegramChatID = -5154190382;

        public class FeedbackDto
        {
            public int? CustomerID { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public string CustomerPhone { get; set; }
            public string InternetSpeed { get; set; }
            public string Pricing { get; set; }
            public string CustomerServices { get; set; }
            public int? Ratingus { get; set; }
            public string ContactBy { get; set; }
            public string Complaint { get; set; }
            public string CustomerComment { get; set; }
            public bool? IsNoAnswer { get; set; }
            public bool? IsBusy { get; set; }
            public string CommentNoAnswerOrBusy { get; set; }
            public string AssignTo { get; set; }
        }

        public class FollowUpDto
        {
            public string AssignTo { get; set; }
            public string FollowUpStatus { get; set; }
            public string CommentText { get; set; }
        }

        private IHttpActionResult Fail(string message)
        {
            return Ok(new { Result = 0, Message = message });
        }

        private static JToken ParseJson(string json)
        {
            return string.IsNullOrWhiteSpace(json) ? new JArray() : JToken.Parse(json);
        }

        private static string E(string s)
        {
            return HttpUtility.HtmlEncode(s ?? "");
        }

        private string GetUserTeam(HTML html)
        {
            int uid = Utility.TryParse(Convert.ToString(html.UserID), "int", 0);
            if (uid == 0) return "";
            var dt = html.LoadData("select top 1 Team from dbo.TblCustomerFeedbackTeamMember where UserID = " + uid).Tables[0];
            return dt.Rows.Count > 0 ? dt.Rows[0]["Team"].ToString() : "";
        }

        [HttpGet, Route("Context")]
        public IHttpActionResult Context()
        {
            var html = new HTML();
            return Ok(new
            {
                Result = (int)ResultType.Successful,
                UserName = html.UserFullName,
                Team = GetUserTeam(html),
                Teams = FlowTeams,
                Statuses = FlowStatuses
            });
        }

        [HttpGet, Route("")]
        public IHttpActionResult List(int customerId = 0)
        {
            var html = new HTML();
            string sql = @"select 
ROW_NUMBER() over(order by cf.FeedbackID desc) as [No], cf.*, c.CustomerCode as CID, coalesce(nullif(c.CustomerName, ''), c.CustomerNameLatin) as Customer,
(select count(*) from dbo.TblCustomerFeedbackLog l where l.FeedbackID = cf.FeedbackID) as StepCount
from dbo.TblCustomerFeedback cf inner join dbo.TblCustomer c on c.CustomerID = cf.CustomerID
where isnull(cf.IsDelete, 0) <> 1 and cf.SystemID = " + html.SystemID + " and cf.WarehouseID = " + html.WarehouseID
                + (customerId > 0 ? " and cf.CustomerID = " + customerId : "");

            return Ok(new { Result = (int)ResultType.Successful, Data = ParseJson(html.JsonData(sql)) });
        }

        [HttpGet, Route("{id:int}/Log")]
        public IHttpActionResult Log(int id)
        {
            var html = new HTML();
            string sql = @"select l.LogID, l.FeedbackID, l.StepNo, l.ActionType, l.Team, l.AssignedTeam, l.Status, l.Comment, l.CreatedBy, l.CreatedDate
from dbo.TblCustomerFeedbackLog l
inner join dbo.TblCustomerFeedback cf on cf.FeedbackID = l.FeedbackID
where l.FeedbackID = " + id + " and cf.SystemID = " + html.SystemID + " and cf.WarehouseID = " + html.WarehouseID + @"
order by l.StepNo desc";

            return Ok(new { Result = (int)ResultType.Successful, Data = ParseJson(html.JsonData(sql)) });
        }

        [HttpPost, Route("")]
        public Task<IHttpActionResult> Create([FromBody] FeedbackDto d)
        {
            return SaveInternal(0, d);
        }

        [HttpPut, Route("{id:int}")]
        public Task<IHttpActionResult> Update(int id, [FromBody] FeedbackDto d)
        {
            return SaveInternal(id, d);
        }

        private async Task<IHttpActionResult> SaveInternal(int feedbackID, FeedbackDto d)
        {
            if (d == null) return Fail("Invalid request.");
            if ((d.CustomerID ?? 0) <= 0 || string.IsNullOrWhiteSpace(d.CustomerName)) return Fail("Customer is required.");
            if (string.IsNullOrWhiteSpace(d.CustomerComment)) return Fail("Customer comment is required.");

            var html = new HTML();

            string assignTo = (d.AssignTo ?? "").Trim();
            if (feedbackID != 0 || Array.IndexOf(FlowTeams, assignTo) < 0) assignTo = "";
            string userTeam = GetUserTeam(html);
            if (userTeam == "") userTeam = "Sales Team";

            string customerComment = d.CustomerComment.Trim();
            string commentNoAnswerOrBusy = (d.CommentNoAnswerOrBusy ?? "").Trim();
            string rating = (d.Ratingus ?? 0).ToString();

            var para = new Dictionary<string, string>
            {
                { "CustomerID", d.CustomerID.ToString() },
                { "CustomerCode", d.CustomerCode ?? "" },
                { "CustomerName", d.CustomerName },
                { "DateFeedback", html.GetCurrentDate() },
                { "InternetSpeed", d.InternetSpeed ?? "" },
                { "Pricing", d.Pricing ?? "" },
                { "CustomerServices", d.CustomerServices ?? "" },
                { "Ratingus", rating },
                { "CustomerComment", customerComment },
                { "ContactBy", d.ContactBy ?? "" },
                { "CreateBy", html.UserFullName },
                { "UserID", html.UserID },
                { "IsNoAnswer", (d.IsNoAnswer ?? false) ? "1" : "0" },
                { "IsBusy", (d.IsBusy ?? false) ? "1" : "0" },
                { "CommentNoAnswerOrBusy", commentNoAnswerOrBusy },
                { "CreateDate", html.GetCurrentDate() },
                { "UserContact", html.UserFullName },
                { "Complaint", d.Complaint ?? "" },
                { "AssignTo", assignTo },
                { "UserTeam", userTeam },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };

            string sql;
            string feedbackNo = "";
            SqlResult result;

            if (feedbackID == 0)
            {
                var prefixPara = new Dictionary<string, string>
                {
                    { "Prefix", "FB-{yy}{MM}{dd}-{000}" },
                    { "Module", "Customer Feedback" },
                    { "SystemID", html.SystemID.ToString() },
                    { "WarehouseID", html.WarehouseID.ToString() }
                };
                result = html.SaveChangeData("SP_INSERT_PREFIX", true, prefixPara, true);
                feedbackNo = result.Value;
                para.Add("FeedbackNo", feedbackNo);

                sql = @"
                    INSERT INTO TblCustomerFeedback
                    (FeedbackNo, CID, CustomerID, CustomerName, DateFeedback, InternetSpeed, Pricing, CustomerServices, Ratingus,
                     CustomerComment, ContactBy, CreateBy, UserID, IsNoAnswer, IsBusy, CommentNoAnswerOrBusy, CreateDate,
                     UserContact, Complaint, SystemID, WarehouseID)
                    VALUES
                    (@FeedbackNo, @CustomerCode, @CustomerID, @CustomerName, @DateFeedback, @InternetSpeed, @Pricing, @CustomerServices, @Ratingus,
                     @CustomerComment, @ContactBy, @CreateBy, @UserID, @IsNoAnswer, @IsBusy, @CommentNoAnswerOrBusy, @CreateDate,
                     @UserContact, @Complaint, @SystemID, @WarehouseID);

                    DECLARE @NewID INT = SCOPE_IDENTITY();

                    UPDATE TblCustomer SET IsFeedback = 1 WHERE CustomerID = @CustomerID;

                    INSERT INTO TblCustomerFeedbackLog (FeedbackID, StepNo, ActionType, Team, CreatedBy, CreatedDate)
                    VALUES (@NewID, 1, 'Created', @UserTeam, @CreateBy, @CreateDate);

                    IF @AssignTo <> ''
                    BEGIN
                        INSERT INTO TblCustomerFeedbackLog (FeedbackID, StepNo, ActionType, Team, AssignedTeam, Status, Comment, CreatedBy, CreatedDate)
                        VALUES (@NewID, 2, 'Handover', @UserTeam, @AssignTo, 'Pending', 'Assigned when the feedback was logged.', @CreateBy, @CreateDate);

                        UPDATE TblCustomerFeedback
                        SET FollowUpByTeam = @AssignTo, FollowUpStatus = 'Pending', FollowUpBy = @CreateBy, FollowUpDate = @CreateDate
                        WHERE FeedbackID = @NewID;
                    END

                    SELECT @NewID;";
            }
            else
            {
                para.Add("FeedbackID", feedbackID.ToString());
                para.Add("ModifyBy", html.UserFullName);
                para.Add("ModifyDate", html.GetCurrentDate());

                sql = @"
                    UPDATE TblCustomerFeedback
                    SET CustomerID = @CustomerID,
                        CID = ISNULL(NULLIF(@CustomerCode, ''), CID),
                        CustomerName = @CustomerName,
                        InternetSpeed = @InternetSpeed,
                        Pricing = @Pricing,
                        CustomerServices = @CustomerServices,
                        Ratingus = @Ratingus,
                        CustomerComment = @CustomerComment,
                        ContactBy = @ContactBy,
                        IsNoAnswer = @IsNoAnswer,
                        IsBusy = @IsBusy,
                        CommentNoAnswerOrBusy = @CommentNoAnswerOrBusy,
                        UserContact = @UserContact,
                        Complaint = @Complaint,
                        ModifyBy = @ModifyBy,
                        ModifyDate = @ModifyDate
                    WHERE FeedbackID = @FeedbackID AND SystemID = @SystemID AND WarehouseID = @WarehouseID;

                    SELECT @FeedbackID;";
            }

            result = html.SaveChangeData(sql, false, para, true);

            if (result.Result == ResultType.Successful && feedbackID == 0)
                await NotifyCreated(html, result.Value, d, customerComment, commentNoAnswerOrBusy, rating, assignTo);

            return Ok(new
            {
                Result = (int)result.Result,
                Message = result.Message,
                FeedbackID = feedbackID == 0 ? result.Value : feedbackID.ToString(),
                FeedbackNo = feedbackNo
            });
        }

        private async Task NotifyCreated(HTML html, string newFeedbackID, FeedbackDto d, string customerComment,
                                         string commentNoAnswerOrBusy, string rating, string assignTo)
        {
            try
            {
                var message = new StringBuilder();
                message.AppendLine("📣 <b>Customer Feedback Received</b>\n");
                message.AppendLine($"👤 Customer: {E(d.CustomerName)}");
                if (!string.IsNullOrWhiteSpace(d.CustomerPhone)) message.AppendLine($"📞 Phone: {E(d.CustomerPhone)}");
                if (!string.IsNullOrWhiteSpace(d.InternetSpeed)) message.AppendLine($"📶 Internet Speed: {E(d.InternetSpeed)}");
                if (!string.IsNullOrWhiteSpace(d.Pricing)) message.AppendLine($"💲 Pricing: {E(d.Pricing)}");
                if (!string.IsNullOrWhiteSpace(d.CustomerServices)) message.AppendLine($"🤝 Customer Services: {E(d.CustomerServices)}");
                message.AppendLine($"⭐ Rating: {rating}");
                if (!string.IsNullOrWhiteSpace(d.ContactBy)) message.AppendLine($"📩 Contacted By: {E(d.ContactBy)}");
                if (!string.IsNullOrWhiteSpace(commentNoAnswerOrBusy)) message.AppendLine($"📝 Note (NoAnswer/Busy): {E(commentNoAnswerOrBusy)}");
                if (!string.IsNullOrWhiteSpace(d.Complaint)) message.AppendLine($"⚠️ Complaint: {E(d.Complaint)}");
                if (!string.IsNullOrWhiteSpace(assignTo)) message.AppendLine($"➡️ Follow-up team: <b>{E(assignTo)}</b>");
                message.AppendLine($"\n📝Customer Feedback: {E(customerComment)}");
                message.AppendLine($"\nReceived By: {E(html.UserFullName)}");
                message.AppendLine();
                message.AppendLine("Please check the system for more details. Thank you.");

                var notify = new NotificationTelegram();
                var telResult = await notify.NotificationTelegramBot(message.ToString(), TelegramChatID);
                if (telResult.Result == ResultType.Successful)
                {
                    var para = new Dictionary<string, string>
                    {
                        { "FeedbackID", newFeedbackID },
                        { "TelegramMessageID", telResult.Value.ToString() }
                    };
                    html.SaveChangeData("UPDATE TblCustomerFeedback SET TelegramMessageID = @TelegramMessageID WHERE FeedbackID = @FeedbackID;", false, para);
                }
            }
            catch { }
        }

        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var html = new HTML();
            var para = new Dictionary<string, string>
            {
                { "FeedbackID", id.ToString() },
                { "ModifyBy", html.UserFullName },
                { "ModifyDate", html.GetCurrentDate() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            string sql = @"UPDATE TblCustomerFeedback
                           SET IsDelete = 1, ModifyBy = @ModifyBy, ModifyDate = @ModifyDate
                           WHERE FeedbackID = @FeedbackID AND SystemID = @SystemID AND WarehouseID = @WarehouseID;
                           SELECT @FeedbackID;";
            var result = html.SaveChangeData(sql, false, para, true);
            return Ok(new { Result = (int)result.Result, Message = result.Message });
        }

        [HttpPost, Route("{id:int}/FollowUp")]
        public async Task<IHttpActionResult> FollowUp(int id, [FromBody] FollowUpDto d)
        {
            if (d == null || id <= 0) return Fail("Feedback not found.");
            var html = new HTML();

            string assignTo = (d.AssignTo ?? "").Trim();
            string reqStatus = (d.FollowUpStatus ?? "").Trim();
            string comment = (d.CommentText ?? "").Trim();

            string userTeam = GetUserTeam(html);
            if (userTeam == "") return Fail("Your account is not assigned to a feedback team.");
            if (assignTo != "" && Array.IndexOf(FlowTeams, assignTo) < 0) return Fail("Unknown team.");
            if (reqStatus != "" && Array.IndexOf(FlowStatuses, reqStatus) < 0) return Fail("Unknown status.");

            var cur = html.LoadData(
                "select FollowUpByTeam, FollowUpStatus from dbo.TblCustomerFeedback where isnull(IsDelete, 0) <> 1 and FeedbackID = " + id +
                " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID).Tables[0];
            if (cur.Rows.Count == 0) return Fail("Feedback not found.");

            string curTeam = Convert.ToString(cur.Rows[0]["FollowUpByTeam"]);
            string curStatus = Convert.ToString(cur.Rows[0]["FollowUpStatus"]);

            if (curTeam != "" && curTeam != userTeam && userTeam != "Sales Team")
                return Fail("Only " + curTeam + " (or Sales) can update this follow-up.");

            string action, newTeam, newStatus;
            if (assignTo != "" && assignTo != curTeam)
            {
                action = "Handover"; newTeam = assignTo; newStatus = "Pending";
                if (comment == "") return Fail("Please explain what " + assignTo + " needs to do.");
            }
            else if (reqStatus != "" && reqStatus != curStatus)
            {
                action = "Status"; newTeam = curTeam != "" ? curTeam : userTeam; newStatus = reqStatus;
                if (reqStatus == "Done" && comment == "") return Fail("A comment is required to close this follow-up.");
            }
            else
            {
                action = "Comment"; newTeam = curTeam != "" ? curTeam : userTeam; newStatus = curStatus != "" ? curStatus : "Pending";
                if (comment == "") return Fail("Please write a comment.");
            }

            var para = new Dictionary<string, string>
            {
                { "FeedbackID", id.ToString() },
                { "Team", userTeam },
                { "Action", action },
                { "NewTeam", newTeam },
                { "NewStatus", newStatus },
                { "Comment", comment },
                { "ExpectedTeam", curTeam ?? "" },
                { "ExpectedStatus", curStatus ?? "" },
                { "FollowUpBy", html.UserFullName },
                { "FollowUpDate", html.GetCurrentDate() }
            };

            string sql = @"
                SET XACT_ABORT ON;
                BEGIN TRAN;

                UPDATE TblCustomerFeedback
                SET FollowUpByTeam  = @NewTeam,
                    FollowUpStatus  = @NewStatus,
                    FollowUpComment = ISNULL(NULLIF(@Comment, ''), FollowUpComment),
                    FollowUpBy      = @FollowUpBy,
                    FollowUpDate    = @FollowUpDate
                WHERE FeedbackID = @FeedbackID
                  AND ISNULL(FollowUpByTeam, '') = @ExpectedTeam
                  AND ISNULL(FollowUpStatus, '') = @ExpectedStatus;

                IF @@ROWCOUNT = 0
                BEGIN
                    ROLLBACK;
                    SELECT 0 AS Applied, 0 AS TelegramMessageID;
                    RETURN;
                END

                DECLARE @Step INT = (SELECT ISNULL(MAX(StepNo), 0) + 1
                                     FROM TblCustomerFeedbackLog WITH (UPDLOCK, HOLDLOCK)
                                     WHERE FeedbackID = @FeedbackID);

                INSERT INTO TblCustomerFeedbackLog
                    (FeedbackID, StepNo, ActionType, Team, AssignedTeam, Status, Comment, CreatedBy, CreatedDate)
                VALUES
                    (@FeedbackID, @Step, @Action, @Team, @NewTeam, @NewStatus, NULLIF(@Comment, ''), @FollowUpBy, @FollowUpDate);

                COMMIT;

                SELECT 1 AS Applied, ISNULL(TelegramMessageID, 0) AS TelegramMessageID
                FROM TblCustomerFeedback WHERE FeedbackID = @FeedbackID;";

            var result = html.SaveChangeData(sql, false, para, true, true);
            if (result.Result != ResultType.Successful)
                return Ok(new { Result = (int)result.Result, Message = result.Message });

            var dr = result.Data.Tables[0].Rows[0];
            if (Convert.ToInt32(dr["Applied"]) == 0)
                return Fail("This follow-up was just changed by someone else. Please reopen it.");

            try
            {
                int prevMessageId = Utility.TryParse(dr["TelegramMessageID"].ToString(), "int", 0);
                var message = new StringBuilder();
                message.AppendLine(action == "Handover" ? "🔁 <b>Follow-up handed over</b>\n"
                                 : action == "Status" ? "🔄 <b>Follow-up status updated</b>\n"
                                 : "💬 <b>Follow-up comment</b>\n");
                message.AppendLine($"From: {E(userTeam)} ({E(html.UserFullName)})");
                if (action == "Handover") message.AppendLine($"To: <b>{E(newTeam)}</b>");
                message.AppendLine($"Status: {E(newStatus)}");
                if (comment != "") message.AppendLine($"Comment: {E(comment)}");

                var notify = new NotificationTelegram();
                await notify.NotificationTelegramBot("", TelegramChatID, Caption: message.ToString(),
                    replyMarkupInlineKeyboardMarkup: true, MessageID: prevMessageId, edit: false);
            }
            catch { }

            return Ok(new { Result = (int)result.Result, Message = result.Message, Action = action });
        }

        [HttpGet, Route("Report")]
        public IHttpActionResult Report(string startDate = "", string endDate = "")
        {
            var html = new HTML();
            string sql = @"select
    isnull(sum(case when InternetSpeed = 'Smoothly'   then 1 else 0 end), 0) Smoothly,
    isnull(sum(case when InternetSpeed = 'Slow'       then 1 else 0 end), 0) Slow,
    isnull(sum(case when InternetSpeed = 'Not Stable' then 1 else 0 end), 0) NotStable,
    isnull(sum(case when Pricing = 'Reasonable' then 1 else 0 end), 0) Reasonable,
    isnull(sum(case when Pricing = 'Cheap'      then 1 else 0 end), 0) Cheap,
    isnull(sum(case when Pricing = 'Expensive'  then 1 else 0 end), 0) Expensive,
    isnull(sum(case when CustomerServices = 'Very Good' then 1 else 0 end), 0) VeryGood,
    isnull(sum(case when CustomerServices = 'Good'      then 1 else 0 end), 0) Good,
    isnull(sum(case when CustomerServices = 'Bad'       then 1 else 0 end), 0) Bad,
    isnull(sum(Ratingus), 0) RatingTotal,
    isnull(sum(case when Ratingus > 0 then 1 else 0 end), 0) RatedCount,
    isnull(sum(case when IsNoAnswer = 1 then 1 else 0 end), 0) NoAnswer,
    isnull(sum(case when IsBusy = 1 then 1 else 0 end), 0) Busy,
    isnull(sum(case when FollowUpStatus = 'Pending'     then 1 else 0 end), 0) FollowUpPending,
    isnull(sum(case when FollowUpStatus = 'In Progress' then 1 else 0 end), 0) FollowUpInProgress,
    isnull(sum(case when FollowUpStatus = 'Done'        then 1 else 0 end), 0) FollowUpDone
from dbo.TblCustomerFeedback
where isnull(IsDelete, 0) <> 1 and SystemID = @SystemID and WarehouseID = @WarehouseID
  and (@startDate = '' or format(DateFeedback, 'yyMMdd') between @startDate and @endDate)";

            var para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "startDate", startDate ?? "" },
                { "endDate", endDate ?? "" }
            };

            var rows = ParseJson(html.JsonData(sql, false, para)) as JArray;
            return Ok(new
            {
                Result = (int)ResultType.Successful,
                Data = (rows != null && rows.Count > 0) ? rows[0] : new JObject()
            });
        }
    }
}