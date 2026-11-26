using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class BillInfo
    {
        public int CustomerBillInformationID { get; set; }
        public int? BillingInvoiceID { get; set; }
        public string ATTName { get; set; }

        public int CustomerID { get; set; }
        public string ConnectionID { get; set; }
        public int? IspLinkID { get; set; }

        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }

        public string CompanyName { get; set; }
        public string CompanyNameKH { get; set; }
        public string CompanyDeskPhone { get; set; }
        public string CompanyMobile { get; set; }
        public string CompanyEmail { get; set; }
        public string VATNumber { get; set; }

        public string CompanyAddress { get; set; }
        public string CompanyCommune { get; set; }
        public string CompanyDistrict { get; set; }
        public string CompanyProvinceCity { get; set; }

        public string CompanyContactName { get; set; }
        public string CompanyContactPosition { get; set; }
        public string CompanyContactMobileNumber { get; set; }

        public string BillToAddress { get; set; }
        public string BillToCommune { get; set; }
        public string BillToDistrict { get; set; }
        public string BillToProvinceCity { get; set; }

        public string InstallAddress { get; set; }
        public string InstallCommune { get; set; }
        public string InstallDistrict { get; set; }
        public string InstallProvinceCity { get; set; }

        public int? EmployeeID { get; set; }

        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextInvoiceDate { get; set; }

        public bool? IsIncludedVAT { get; set; }
        public DateTime? PaymentDueDate { get; set; }

        public int? IndustryTypeID { get; set; }

        public string DestinationFolder { get; set; }
        public string PNGFileName { get; set; }
        public string PDFFileName { get; set; }
        public string ReceiptFileName { get; set; }

        public bool? IsUploaded { get; set; }

        public string GoogleMap_FullAddress { get; set; }
        public string GoogleMap_Latitude { get; set; }
        public string GoogleMap_Longitude { get; set; }

        public string InvoiceType { get; set; }
        public bool? IsRenew { get; set; }
        public bool? IsGenerated { get; set; }

        public int? StatusID { get; set; }

        public string QuotationNo { get; set; }
        public int? InvoiceTmpID { get; set; }

        public bool? IsChangePlan { get; set; }
        public int? FromActivateCustomerID { get; set; }
        public bool? IsApplyChangePlan { get; set; }
        public int? ActivateCustomerID { get; set; }

        public string ProformaInvoiceNo { get; set; }
        public string ReceiptNo { get; set; }
        public string Type { get; set; }

        public int? ConnectivityStatus { get; set; }
        public int invTmp { get; set; }

        public decimal? ExchangeRate { get; set; }
        public string TermCondition { get; set; }

        public bool? IsNextInvoice { get; set; }

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

        public int? PaymentID { get; set; }
        public string ReferenceNo { get; set; }

        public string ClassName { get; set; }
        public int? ClassNameID { get; set; }

        public string Warehouse { get; set; }
        public int? LocationID { get; set; }

        public int? AgencyCommissionID { get; set; }

        public int? ServiceID { get; set; }
        public string ServiceName { get; set; }

        public int? PackageID { get; set; }
        public int? PackageDetailID { get; set; }

        public DateTime? InstallDate { get; set; }
        public string SONumber { get; set; }

        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
