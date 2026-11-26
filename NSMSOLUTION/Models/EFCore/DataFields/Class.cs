using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class Class
    {
        public int? ClassID { get; set; }
        public string ClassName { get; set; }
        public string FullName { get; set; }
        public string PrefixInvoice { get; set; }
        public int? StartNumberInvoice { get; set; }
        public string PrefixSO { get; set; }
        public int? StartNumberSO { get; set; }
        public string Memo { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string FileLogo { get; set; }
        public int? StatusID { get; set; }
        public string PrefixCommercial { get; set; }
        public int? StartNumberCM { get; set; }
        public string PrefixCreditNote { get; set; }
        public int? StartNumberCreditNote { get; set; }
        public string PrefixPV { get; set; }
        public int? StartPrefixPV { get; set; }
        public string JournalEntryNo { get; set; }
        public int? StartJournalEntry { get; set; }
        public string AdvanceExpenseNo { get; set; }
        public int? StartAdvanceExpenseNo { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
        public string PrefixPO { get; set; }
        public int? StartNumberPO { get; set; }
        public string PrefixRequestForm { get; set; }
        public int? StartRF { get; set; }
        public string PrefixQuote { get; set; }
        public int? StartQuote { get; set; }
        public int? StartNumberQuote { get; set; }
        public string PrefixQuotation { get; set; }
        public int? StartQuoteNumber { get; set; }
        public string KHName { get; set; }
        public string PrefixFT { get; set; }
        public string PrefixJE { get; set; }
        public string PrefixExpense { get; set; }
        public int? StartPrefixEx { get; set; }
        public string PrefixCollection { get; set; }
        public int? StartCollectionNumber { get; set; }
    }
}