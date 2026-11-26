using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class BankSlip
    {
        public int ID { get; set; }
        public int CustomerID { get; set; }
        public string InvoiceNo { get; set; }
        public string Destination { get; set; }
        public string FileName { get; set; }
        public DateTime CreateDate { get; set; }
        public int? NeedApproveByID { get; set; }
        public string ApprovedBy { get; set; }
        public int? ApprovedByID { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int? StatusID { get; set; }
        public long? TelegramGroupID { get; set; }
        public int? TelegramMessageID { get; set; }
        public int? TelegramTopicID { get; set; }
        public string TelegramCaption { get; set; }
        public bool? IsDoneLayCable { get; set; }
        public DateTime? DoneLayDate { get; set; }
        public bool? IsDoneInstall { get; set; }
        public DateTime? DoneInstallDate { get; set; }
        public string UCID { get; set; }
        public string USID { get; set; }
        public string ConnectionID { get; set; }
        public int? FinishInstallMessageID { get; set; }
        public int UserID { get; set; }
        public decimal? Amount { get; set; }
        public string Memo { get; set; }
    }
}
