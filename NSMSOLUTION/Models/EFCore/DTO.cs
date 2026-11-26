using System;
using System.Collections.Generic;

namespace NSMSOLUTION.Models.EFCore
{
    public class TelegramGroupConfigDTO
    {
        public string TelegramGroupID { get; set; }
    }

    public class CompanyDto
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyNameKH { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyWebsite { get; set; }
        public string CompanyLogo { get; set; }
    }

    public class UserLoginDTO
    {
        public int UserLoginID { get; set; }
        public string FullName { get; set; }
    }

    public class InvoiceSummaryDTO
    {
        public int No { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceNo { get; set; }
        public string CID { get; set; }
        public string SID { get; set; }
        public string CustomerName { get; set; }
        public string PackageName { get; set; }
        public string UM { get; set; }
        public decimal Amount { get; set; }
    }
    public class ItemExpiredDTO
    {
        public DateTime ExpireDate { get; set; }
        public string WarehouseName { get; set; }
        public int CurrentStockExpired { get; set; }
    }
    public class ItemProfileDetailDTO
    {
        //Detail
        public int ItemID { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemGroup { get; set; }
        public string ItemDescription { get; set; }
        public string Barcode { get; set; }
        public string UM { get; set; }
        public string ItemSize { get; set; }
        public string ItemModel { get; set; }
        public string ItemBrand { get; set; }
        public int ReorderPoint { get; set; }
        public string StockName { get; set; }
        public string ItemImage { get; set; }
        public string CreateBy { get; set; }
        public string CreateDate { get; set; }

        //Pricing
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }

        //Stock Summary
        public double? CurrentStock { get; set; }
        public double? InStock { get; set; }
        public double? OutStock { get; set; }

        //Stock Expiration
        public string ExpireDate { get; set; }
        public string WarehouseName { get; set; }
        public double CurrentStockExpired { get; set; }
        public int? TelegramAlert { get; set; }


        //Supplier
        public string SupplierName { get; set; }
        public string SupplierPhone { get; set; }
    }
    public class MikrotikItem
    {
        public Dictionary<string, string> Words { get; set; }
        public string Tag { get; set; }
    }
    public class ExchangeRateDTO
    {
        public decimal? ExchangeRate { get; set; }
    }
    public class CustomerExpireDto
    {
        public int CustomerID { get; set; }
        public string CustomerIdentification { get; set; }
        public DateTime? IdentificationExpireDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerGender { get; set; }
        public string CustomerMobile { get; set; }
        public DateTime? RegisterDate { get; set; }
        public int ExpirePeriod { get; set; }
    }

    // add new dto for api /api/Customer/CustomerRenew
    public class CustomerRenewRequest
    {
        public int CustomerID { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextInvoiceDate { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string RequestApprovalTgUser { get; set; }
        public int? RequestApprovalNameID { get; set; }
        public string RequestApprovalName { get; set; }
        public string RequestCheckByTgUser { get; set; }
        public int? RequestCheckByNameID { get; set; }
        public string RequestCheckByName { get; set; }
    }

    public class PackageDetailDto
    {
        public int PackageDetailID { get; set; }
        public int Bandwidth { get; set; }
        public int DowSpeed { get; set; }
        public int UpSpeed { get; set; }
        public string Feature { get; set; }
    }

    public class CustomerPaymentRequest
    {
        public int PaymentID { get; set; }
        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNo { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentDate { get; set; }
        public int PaymentMenthod { get; set; }
        public string ChequeNumber { get; set; }
        public string ReferenceNo { get; set; }
        public int ChartOfAccountID { get; set; }
        public string ToAccountNumber { get; set; }
        public string Category { get; set; }
        public string PaymentDescription { get; set; }
        public string ReceivedFromName { get; set; }
        public int StatusID { get; set; }
        public int BillingInvoiceID { get; set; }
        public string Payer { get; set; }

        // Optional system/user overrides
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public int? UserID { get; set; }
        public string UserFullName { get; set; }
        public string DomainID { get; set; }

        // Attachment file support
        public string AttachmentFile { get; set; }
        public string Base64Attachment { get; set; }
        public string AttachmentFileName { get; set; }

        // Dynamic Bank / Mobile Transfer details
        public string FromBankName { get; set; }
        public string FromAccountNumber { get; set; }
        public string FromAccountName { get; set; }
        public string FromPhoneNumber { get; set; }
        public string ToPhoneNumber { get; set; }
    }

    public class CustomerCollectionDto
    {
        public long CollectionID { get; set; }
        public string CollectionNo { get; set; }

        public int CustomerID { get; set; }
        public DateTime CollectionDate { get; set; }

        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }

        // 1 = Invoice Collection
        // 2 = Advance Collection
        public int CollectionType { get; set; }

        public int StatusID { get; set; }

        public string ReferenceNo { get; set; }
        public string Remark { get; set; }

        public List<CustomerCollectionPaymentDto> Payments { get; set; }
        public List<CustomerCollectionAllocationDto> Allocations { get; set; }
    }

    public class CustomerCollectionPaymentDto
    {
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNo { get; set; }
        public string BankName { get; set; }
    }

    public class CustomerCollectionAllocationDto
    {
        public int? InvoiceID { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
    }
}