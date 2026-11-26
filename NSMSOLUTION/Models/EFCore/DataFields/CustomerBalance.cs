using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class CustomerBalance
    {
        public int CustomerBalanceID { get; set; }
        public int? BillingInvoiceID { get; set; }
        public int CustomerID { get; set; }
        public string ConnectionID { get; set; }

        public string CustomerName { get; set; }
        public string CompanyName { get; set; }
        public string TypeOfInvoice { get; set; }
        public string InvoiceNo { get; set; }
        public string CreditNoteNo { get; set; }

        public decimal? SubTotal { get; set; }
        public decimal? VAT { get; set; }
        public decimal? Total { get; set; }
        public decimal? DiscountP { get; set; }
        public decimal? DiscountUSD { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal? DepositFee { get; set; }
        public decimal? Balance { get; set; }
        public decimal? Paid { get; set; }
        public decimal? Unpaid { get; set; }

        public int? ServiceID { get; set; }
        public int? PackageID { get; set; }

        public int? Duration { get; set; }
        public int? FreeDuration { get; set; }
        public string BillCycle { get; set; }

        public int? StatusID { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? NextInvoiceDate { get; set; }
        public DateTime? PaymentDueDate { get; set; }

        public bool? IsIncludedVAT { get; set; }

        public int? EmployeeID { get; set; }

        public DateTime? PaidDate { get; set; }
        public int? PaidByUserID { get; set; }
        public string PaidBy { get; set; }

        public string Type { get; set; }
        public string ReceiptNo { get; set; }
        public int? PaymentID { get; set; }
        public int? InvoiceTmpID { get; set; }

        public decimal? ExchangeRate { get; set; }
        public int? ChartOfAccountID { get; set; }

        public string CreateBy { get; set; }
        public string RenewBy { get; set; }
        public string CancelBy { get; set; }
        public string DeleteBy { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? RenewDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime? DeleteDate { get; set; }

        public string CancelComment { get; set; }
        public string InvoiceType { get; set; }
        public string ReferenceNo { get; set; }

        public string ClassName { get; set; }
        public int? ClassNameID { get; set; }

        public int? InvoiceStatusID { get; set; }
        public string InvoiceStatus { get; set; }

        public string ApproveBy { get; set; }
        public int? ApproveByUserID { get; set; }
        public DateTime? ApproveDate { get; set; }

        public string CommentInvoice { get; set; }

        public bool? IsToDMCTAX { get; set; }
        public int? CustIDToDMC { get; set; }
        public string InvoiceToDMC { get; set; }

        public int? AgencyCommissionID { get; set; }
        public string AgencyCommissionTypeID { get; set; }

        public int? RequestedByUserIDToDMCTAX { get; set; }
        public string RequestedByUserToDMCTAX { get; set; }

        public string RequestApprovalToDMCTAX { get; set; }
        public int? RequestApprovalToDMCTAXID { get; set; }
        public string RequestApprovalToDMCTAXTgUser { get; set; }

        public DateTime? RequestToDMCTAXDate { get; set; }
        public DateTime? ApproveRequestToDMCTAXDate { get; set; }
        public int? ApproveRequestToDMCTAXTGMID { get; set; }
        public string ApproveRequestToDMCTAXStatus { get; set; }
        public string ApproveRequestToDMCTAXMsg { get; set; }

        public string DescriptionTicketTelegram { get; set; }
        public string TicketNumber { get; set; }

        public string KeepXMLData { get; set; }
        public string Remark { get; set; }

        public bool? IsUploadToFTP { get; set; }
        public string UploadMessage { get; set; }
        public DateTime? UploadDate { get; set; }

        public long? TelegramMessageID { get; set; }

        public string IsServicesOrProduct { get; set; }
        public int? ReviseCount { get; set; }

        public int? UploadBy { get; set; }

        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
