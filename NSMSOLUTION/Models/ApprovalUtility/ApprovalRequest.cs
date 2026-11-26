using MVC;
using System.Collections.Generic;

namespace NSMSOLUTION.Models.ApprovalUtility
{
    public class ApprovalRequest
    {
        public HTML html { get; set; } = new HTML();
        public int SystemID
        {
            get => int.Parse(html.SystemID.ToString());
            set => html.SystemID = value.ToString();
        }

        public Dictionary<string, string> para { get; set; } = null;
        public string sql { get; set; } = "";
        public int EntryID { get; set; }
        public string EntryType { get; set; } = "";
        public int StatusID { get; set; } = 0;
        public string Comment { get; set; } = "";
        public string ApprovalFrom { get; set; } = ApprovalSource.System;
        public string ReferenceNo { get; set; } = "";
        public bool IsMsgApproval { get; set; } = false;
        //user 
        public string UserFullName { get; set; } = "";
        public string DBUsername { get; set; } = "";
        public int TakeActionByID { get; set; } = 0;
        public string TakeActionBy { get; set; } = "";
        public int? UserApprovalID { get; set; } = 0;
        public int NeedApproveByUserID { get; set; } = 0;
        public int UserBossID { get; set; } = 38;
        public string BossPrefix { get; set; } = "Boss";
        public string NeedApproveByUser { get; set; } = "";
        public string NeedApproveByUsername { get; set; } = "";
        //pdf 
        public string HtmlData { get; set; } = "";
        public string Destination { get; set; } = "";
        public string FileName { get; set; } = "";
        //telegram
        public string Message { get; set; } = "";
        public int MessageId { get; set; } = 0;
        public long GroupId { get; set; } = 0;
        public long GroupId2 { get; set; } = 0;
        public int TopicId { get; set; } = 0;
        public string BotID { get; set; } = null;
        public string GroupType { get; set; } = "";
        public string MessageType { get; set; } = "Normal";
        public string Msg { get; set; } = "";
        public string Msg2 { get; set; } = "";
        public bool IsRequestEdit { get; set; } = false;
        public bool IsNeedCheck { get; set; } = false;
        public bool AllowReject { get; set; } = false;
        public string InlineKeyModule { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string UserName { get; set; } = "";
        public bool TakeActionFromTelegram { get; set; } = false;
        public string BotToken { get; set; } = "";
        public bool IsApprovalDifferentGroup { get; set; } = false;
        public bool AllowButtonMarkup { get; set; } = true;
        public bool ShowButtonCallback { get; set; } = true;
        public bool EditMessage { get; set; } = true;
        public int LastTelegramMessageId { get; set; } = 0;
        public string LastTelegramMessage { get; set; } = "";
        public bool IsSentWithFilePdf { get; set; } = true;
    }
}