using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class CustomerBillingInvoice
    {
        public int BillingInvoiceID { get; set; }
        public int? CustomerBillInformationID { get; set; }
        public int? OldBillingInvoiceID { get; set; }

        public string TypeOfInvoice { get; set; }
        public string InvoiceNo { get; set; }
        public string CreditNoteNo { get; set; }

        public int? EmployeeID { get; set; }
        public int? CustomerID { get; set; }

        public int? ServiceID { get; set; }
        public int? PackageID { get; set; }
        public int? PackageDetailID { get; set; }

        public string PID { get; set; }
        public string PackageCode { get; set; }

        public string ConnectionID { get; set; }
        public string SID { get; set; }
        public int? RouterID { get; set; }
        public int? ActivateCustomerID { get; set; }

        public string ServiceName { get; set; }
        public string PackageName { get; set; }

        public int? Duration { get; set; }
        public int? FreeDuration { get; set; }

        public decimal? DepositFee { get; set; }

        public string UM { get; set; }
        public int? QTY { get; set; }
        public decimal? UnitPrice { get; set; }

        public decimal? DiscountP { get; set; }
        public decimal? DiscountUSD { get; set; }
        public decimal? Amount { get; set; }

        public DateTime? InvoiceDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? InstallDate { get; set; }
        public DateTime? NextInvoiceDate { get; set; }
        public DateTime? EndNextInvoiceDate { get; set; }

        public int? StatusID { get; set; }
        public DateTime? PaymentDueDate { get; set; }

        public bool? IsIncludedVAT { get; set; }
        public bool? IsNextInvoice { get; set; }

        public int? ConnectivityStatus { get; set; }
        public string ConnectivityStatusName { get; set; }

        public DateTime? ActivateDate { get; set; }
        public DateTime? EndServiceDate { get; set; }

        public string InvoiceType { get; set; }
        public DateTime? ExpireDate { get; set; }

        public bool? IsRenew { get; set; }
        public bool? IsGenerated { get; set; }

        public string GeneratedBy { get; set; }
        public DateTime? GeneratedDate { get; set; }

        public string PPPoEUserName { get; set; }
        public string PPPoEPassword { get; set; }

        public bool? IsAlreadyApplyProfile { get; set; }

        public string ATTName { get; set; }

        public bool? IsUploaded { get; set; }
        public string DestinationFolder { get; set; }

        public string PDFFileName { get; set; }
        public string PNGFileName { get; set; }
        public string ReceiptFileName { get; set; }

        public string OldInvoiceNo { get; set; }
        public string QuotationNo { get; set; }

        public int? InvoiceTmpID { get; set; }

        public bool? IsChangePlan { get; set; }
        public int? FromActivateCustomerID { get; set; }
        public bool? IsApplyChangePlan { get; set; }

        public int? invTmp { get; set; }
        public string ConnectionType { get; set; }

        public string ProformaInvoiceNo { get; set; }
        public string ReceiptNo { get; set; }
        public string Type { get; set; }

        public decimal? ExchangeRate { get; set; }
        public int? ChartOfAccountID { get; set; }

        public string CreateBy { get; set; }
        public string ModifyBy { get; set; }
        public string RenewBy { get; set; }
        public string CancelBy { get; set; }
        public string DeleteBy { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public DateTime? RenewDate { get; set; }
        public string RenewStatus { get; set; }
        public DateTime? DoneRenewDate { get; set; }

        public DateTime? CancelDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string CancelComment { get; set; }

        public int? PaymentID { get; set; }
        public string ReferenceNo { get; set; }

        public int? OpenTicketID { get; set; }
        public string TicketNumber { get; set; }

        public string ClassName { get; set; }
        public int? ClassNameID { get; set; }

        public bool? IsToDMCTAX { get; set; }
        public int? IspLinkID { get; set; }

        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
