using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class CustomerInvoice
    {
        // Primary / Relations
        public int CustomerInvoiceID { get; set; }
        public int? CustomerBillInformationID { get; set; }
        public int? BillingInvoiceID { get; set; }
        public int? OldBillingInvoiceID { get; set; }
        public int? ActivateCustomerID { get; set; }
        public int? FromActivateCustomerID { get; set; }

        // Invoice Info
        public string TypeOfInvoice { get; set; }
        public string Type { get; set; }
        public string InvoiceNo { get; set; }
        public string CreditNoteNo { get; set; }
        public string ProformaInvoiceNo { get; set; }
        public string QuotationNo { get; set; }
        public string ReceiptNo { get; set; }
        public string OldInvoiceNo { get; set; }
        public int? InvoiceTmpID { get; set; }
        public int? invTmp { get; set; }
        public bool? IsChangeFlow { get; set; }

        // Customer
        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public int? CustomerTypeID { get; set; }
        public string ATTName { get; set; }

        // Employee / Sales
        public int? EmployeeID { get; set; }
        public int? SalesID { get; set; }

        // Service / Package
        public int? ServiceID { get; set; }
        public int? PackageID { get; set; }
        public int? PackageDetailID { get; set; }
        public string PackageCode { get; set; }
        public string ServiceName { get; set; }
        public string PID { get; set; }
        public int? OrderBy { get; set; }

        // Item / Stock
        public int? ItemID { get; set; }
        public int? StockInID { get; set; }
        public int? OtherChargeID { get; set; }
        public int? UomID { get; set; }
        public string UM { get; set; }
        public string ItemDescription { get; set; }
        public string ItemName { get; set; }
        public string ItemStatus { get; set; }
        public string Dimension { get; set; }
        public decimal? QOH { get; set; }
        public DateTime? StockExpireDate { get; set; }
        public decimal? AvailableStock { get; set; }

        // Pricing
        public decimal? QTY { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? DiscountP { get; set; }
        public decimal? DiscountUSD { get; set; }
        public decimal? Amount { get; set; }
        public decimal? DepositFee { get; set; }
        public decimal? ExchangeRate { get; set; }

        // Period / Dates
        public DateTime? InstallDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextInvoiceDate { get; set; }
        public DateTime? EndNextInvoiceDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public DateTime? PaymentDueDate { get; set; }

        // Duration
        public int? Duration { get; set; }
        public int? FreeDuration { get; set; }
        public bool? IsShowPeriod { get; set; }

        // Network / Connectivity
        public string SID { get; set; }
        public string ConnectionID { get; set; }
        public int? DowSpeed { get; set; }
        public int? UpSpeed { get; set; }
        public int? Bandwidth { get; set; }
        public string Feature { get; set; }
        public int? ConnectivityStatus { get; set; }
        public string ConnectivityStatusName { get; set; }
        public string PPPoEUserName { get; set; }

        // Status / Flags
        public int? StatusID { get; set; }
        public int? InvoiceStatusID { get; set; }
        public string InvoiceStatus { get; set; }
        public string InvoiceNote { get; set; }
        public string InvoiceType { get; set; }
        public bool? IsIncludedVAT { get; set; }
        public bool? IsNextInvoice { get; set; }
        public bool? IsRenew { get; set; }
        public bool? IsGenerated { get; set; }
        public bool? IsChangePlan { get; set; }
        public bool? IsApplyChangePlan { get; set; }
        public bool? IsApplyDelayPayment { get; set; }
        public bool? IsToDMCTAX { get; set; }

        // Accounting
        public int? ChartOfAccountID { get; set; }
        public int? PaymentID { get; set; }
        public string ReferenceNo { get; set; }

        // Agency Commission
        public int? AgencyCommissionID { get; set; }
        public string AgencyCommissionTypeID { get; set; }

        // DMCTAX Approval
        public int? RequestedByUserIDToDMCTAX { get; set; }
        public string RequestedByUserToDMCTAX { get; set; }
        public string RequestApprovalToDMCTAX { get; set; }
        public int? RequestApprovalToDMCTAXID { get; set; }
        public string RequestApprovalToDMCTAXTgUser { get; set; }
        public DateTime? RequestToDMCTAXDate { get; set; }
        public DateTime? ApproveRequestToDMCTAXDate { get; set; }
        public string ApproveRequestToDMCTAXTGMID { get; set; }
        public string ApproveRequestToDMCTAXStatus { get; set; }
        public string ApproveRequestToDMCTAXMsg { get; set; }

        // Approval
        public string ApproveBy { get; set; }
        public int? ApproveByUserID { get; set; }
        public DateTime? ApproveDate { get; set; }

        // Comments
        public string Comment { get; set; }
        public int? CommentByID { get; set; }
        public string CommentBy { get; set; }
        public DateTime? CommentDate { get; set; }

        // Cancel / Renew / Delete
        public string CreateBy { get; set; }
        public string ModifyBy { get; set; }
        public string RenewBy { get; set; }
        public string CancelBy { get; set; }
        public string DeleteBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public DateTime? RenewDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string CancelComment { get; set; }

        // Proforma Cancel
        public string CancelPIBy { get; set; }
        public DateTime? CancelPIDate { get; set; }
        public string CancelPIComment { get; set; }

        // Misc
        public string Warehouse { get; set; }
        public int? LocationID { get; set; }
        public string ClassName { get; set; }
        public int? ClassNameID { get; set; }
        public int? RegisterByUserLoginID { get; set; }
        public int? InvoiceTelegramMessageID { get; set; }
        public int? ReviseCount { get; set; }
        public string Remark { get; set; }

        // System
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
