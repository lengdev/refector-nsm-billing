using System;

namespace NSMSOLUTION.Models.EFCore
{
    public class AlertMessage
    {
        public int AlertID { get; set; }
        public string AlertType { get; set; }
        public int? ReferenceID { get; set; }
        public int? CustomerID { get; set; }
        public string Comment { get; set; }
        public string Reason { get; set; }
        public string PostComment { get; set; }
        public string PostCommentBy { get; set; }
        public bool IsRead { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsDismissed { get; set; }
        public string TelegramGroupID { get; set; }
        public int? MessageID { get; set; }
        public string SentBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}