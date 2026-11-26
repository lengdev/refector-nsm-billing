using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class Feedback
    {
        public int FeedbackID { get; set; }
        public string FeedbackNo { get; set; }
        public int CustomerID { get; set; }
        public string CID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime DateFeedback { get; set; }
        public string InternetSpeed { get; set; }
        public string Pricing { get; set; }
        public string CustomerServices { get; set; }
        public int Ratingus { get; set; }
        public string CustomerComment { get; set; }
        public string ContactBy { get; set; }
        public string CreateBy { get; set; }
        public int UserID { get; set; }
        public bool IsNoAnswer { get; set; }
        public bool IsBusy { get; set; }
        public string CommentNoAnswerOrBusy { get; set; }
        public DateTime CreateDate { get; set; }
        public string Complaint { get; set; }
        public string UserContact { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
        public bool? IsDelete { get; set; }
        public string ModifyBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public int TelegramMessageID { get; set; }
        public string FollowUpComment { get; set; }
        public DateTime FollowUpDate { get; set; }
        public string FollowUpStatus { get; set; }
        public string FollowUpByTeam { get; set; }
        public string FollowUpBy { get; set; }
    }
}