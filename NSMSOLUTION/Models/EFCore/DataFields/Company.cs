namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class Company
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyNameKH { get; set; }
        public string AddressKH { get; set; }
        public string Commune { get; set; }
        public string District { get; set; }
        public string ProvinceCity { get; set; }
        public string Phone { get; set; }
        public string PhoneKH { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Facebook { get; set; }
        public string VATNumber { get; set; }
        public string Logo { get; set; }
        public bool? UseWithInvoiceVAT { get; set; }
        public string TermConditionVAT { get; set; }
        public string TermConditionNonVAT { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public bool ISDefault { get; set; }
        public int? UserID { get; set; }
        public int? InvoiceTemplateID { get; set; }
        public string InvoiceTemplateName { get; set; }
        public int? OrderNumber { get; set; }
        public string CustomerServicePhonNo { get; set; }
        public string AccountingPhonNo { get; set; }
        public string PrefixVAT { get; set; }
        public string PrefixNonVAT { get; set; }
        public string PrefixProformaInvoiceVAT { get; set; }
        public string PrefixProformaInvoiceNonVAT { get; set; }
        public string PrefixQuotationInvoiceVAT { get; set; }
        public string PrefixQuotationInvoiceNonVAT { get; set; }
        public string PrefixReceiptVAT { get; set; }
        public string PrefixReceiptNonVAT { get; set; }
        public string PrefixCreditNote { get; set; }
        public string QuoteTermConditionVAT { get; set; }
        public string QuoteTermConditionNonVAT { get; set; }
        public string PrefixItemRequestNo { get; set; }
        public string PrefixRequestApprovalItemOut { get; set; }
        public int? StartNumberPrefixVAT { get; set; }
        public int? StartNumberPrefixNonVAT { get; set; }
        public int? StartNumberPrefixProformaInvoiceVAT { get; set; }
        public int? StartNumberPrefixProformaInvoiceNonVAT { get; set; }
        public int? StartNumberPrefixQuotationInvoiceVAT { get; set; }
        public int? StartNumberPrefixQuotationInvoiceNonVAT { get; set; }
        public int? StartNumberPrefixReceiptVAT { get; set; }
        public int? StartNumberPrefixReceiptNonVAT { get; set; }
        public int? StartNumberPrefixCreditNote { get; set; }
        public int? StartNumberPrefixItemRequestNo { get; set; }
        public int? StartNumberPrefixRequestApprovalItemOut { get; set; }
        public string DomainID { get; set; }
    }
}