using System;

namespace NSMSOLUTION.Models.EFCore
{
    public class Sale
    {
        public int SalesID { get; set; }

        public string InvoiceNumber { get; set; }
        public string SONumber { get; set; }
        public string QuoteNumber { get; set; }

        public DateTime? TranDate { get; set; }
        public DateTime? DueDate { get; set; }

        public string Type { get; set; }

        public int? SellerID { get; set; }
        public string SellerName { get; set; }

        public int? CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string CustomerMobile { get; set; }

        public string SOID { get; set; }

        public int? ClassID { get; set; }
        public string ClassName { get; set; }

        public int? LocationID { get; set; }
        public string WarehouseName { get; set; }

        public string TermCondition { get; set; }
        public string Remark { get; set; }
        public string SaleNote { get; set; }

        public int? TaxID { get; set; }
        public string TaxName { get; set; }

        public bool? IsIncludedVAT { get; set; }
        public bool? IsCommercialINV { get; set; }

        public int? ChartOfAccountID { get; set; }
        public int? ExchangeRate { get; set; }
        public string AttachmentFile { get; set; }

        public decimal? SubTotal { get; set; }
        public decimal? DiscountP { get; set; }
        public decimal? DiscountUSD { get; set; }
        public decimal? Total { get; set; }
        public decimal? VAT { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal? Deposit { get; set; }
        public decimal? Paid { get; set; }
        public decimal? Balance { get; set; }

        public int? StatusID { get; set; }

        public int? NeedCheckByID { get; set; }
        public string NeedCheckBy { get; set; }

        public int? NeedApproveByID { get; set; }
        public string NeedApproveBy { get; set; }

        public int? CheckByID { get; set; }
        public string CheckBy { get; set; }
        public string CheckByUsername { get; set; }
        public DateTime? CheckDate { get; set; }

        public int? ApproveByID { get; set; }
        public string ApproveBy { get; set; }
        public string ApproveUsername { get; set; }
        public DateTime? ApproveDate { get; set; }

        public int? DisapproveByID { get; set; }
        public string DisapproveBy { get; set; }
        public string DisapproveUsername { get; set; }
        public DateTime? DisapproveDate { get; set; }

        public bool? IsCheck { get; set; }
        public bool? IsApprove { get; set; }
        public bool? IsDisapprove { get; set; }

        public int? IsToInvoice { get; set; }

        public int? TelegramMessageID { get; set; }
        public string TelegramMessage { get; set; }
        public string TelegramGroupID { get; set; }
        public int? TelegramGroupTopicID { get; set; }

        public string ApprovalFrom { get; set; }
        public string Comment { get; set; }

        public int? UserID { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }

        public bool? IsClone { get; set; }
        public bool? IsUpdate { get; set; }
        public string PaymentStatus { get; set; }

        public string NewInvoiceNumber { get; set; }
        public int? INVID { get; set; }
        public string DNNumber { get; set; }

        public bool? IsCancel { get; set; }
        public string CancelBy { get; set; }
        public int? CancelByID { get; set; }
        public DateTime? CancelDate { get; set; }

        public string DestinationFolder { get; set; }

        public int? PreparedByID { get; set; }
        public string PreparedBy { get; set; }
        public string PreparedByUsername { get; set; }
        public DateTime? PreparedDate { get; set; }

        public int? DeliveryByID { get; set; }
        public string DeliveryBy { get; set; }
        public string DeliveryByUsername { get; set; }
        public DateTime? DeliveryDate { get; set; }

        public string DNID { get; set; }

        public int? ReqEditApproveByID { get; set; }
        public string ReqEditApproveBy { get; set; }
        public int? ReqEditStatusID { get; set; }
        public DateTime? ReqEditDate { get; set; }
        public string ReqEditApproveByUsername { get; set; }
        public string ReqEditComment { get; set; }

        public bool? IsToDN { get; set; }

        public string TelegramBotID { get; set; }

        public string VATRate { get; set; }
        public bool? IsNonVAT { get; set; }
    }
}
