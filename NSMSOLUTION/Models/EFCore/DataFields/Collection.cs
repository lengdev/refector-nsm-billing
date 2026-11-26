using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblCollection")]
    public class Collection
    {
        [Key] public int CollectionID { get; set; }
        public string Number { get; set; }
        public DateTime CollectionDate { get; set; }
        public int CustomerID { get; set; }
        public int MethodID { get; set; }
        public int? ClassID { get; set; }
        public int DepositAccountID { get; set; }
        public string Currency { get; set; }
        public string Memo { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AppliedAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal OtherCharge { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal UnappliedAmount { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SlipPath { get; set; }   // slip image paths, separated by ";"
    }

    [Table("TblCollectionDetail")]
    public class CollectionDetail
    {
        [Key] public int CollectionDetailID { get; set; }
        public int CollectionID { get; set; }
        public int? InvoiceID { get; set; }
        public int CustomerID { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal OpenBalance { get; set; }
        public decimal Discount { get; set; }
        public decimal Payment { get; set; }
        public int? PrevStatusID { get; set; }
        public int? PaymentID { get; set; }
        public string ReceiptNo { get; set; }
    }

    // Audit trail of a receipt: money held as a deposit, paid to invoices, reversed, rolled back.
    [Table("TblCollectionHistory")]
    public class CollectionHistory
    {
        [Key] public int HistoryID { get; set; }
        public int CollectionID { get; set; }
        public int CustomerID { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; }     // Deposit, Payment, Reversal, Rollback
        public string InvoiceNo { get; set; }
        public decimal Amount { get; set; }
        public decimal DepositAfter { get; set; } // deposit still held on the receipt after this event
        public string Note { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    [Table("TblCollectionCharge")]
    public class CollectionCharge
    {
        [Key] public int CollectionChargeID { get; set; }
        public int CollectionID { get; set; }
        public int CustomerID { get; set; }
        public string ChargeType { get; set; }
        public string Note { get; set; }
        public decimal Amount { get; set; }
    }

}

namespace NSMSOLUTION.Models.Collections
{
    public class CreditDto
    {
        public int id { get; set; }
        public string no { get; set; }
        public string date { get; set; }
        public string methodId { get; set; }
        public string method { get; set; }
        public string depositId { get; set; }
        public string deposit { get; set; }
        public decimal original { get; set; }
        public decimal balance { get; set; }
    }

    public class CollectionSaveRequest
    {
        public int CustomerId { get; set; }
        public string DateIso { get; set; }
        public string Number { get; set; }
        public int? ClassId { get; set; }
        public string MethodId { get; set; }
        public int DepositId { get; set; }
        public int? SalesId { get; set; }
        public string Currency { get; set; }
        public string Memo { get; set; }
        public decimal AmountPaid { get; set; }
        public List<CollectionLineDto> Lines { get; set; }
        public List<CollectionChargeDto> Charges { get; set; }
    }
    public class CollectionLineDto
    {
        public string InvoiceNo { get; set; }
        public decimal Discount { get; set; }
        public decimal Payment { get; set; }
    }
    public class CollectionChargeDto
    {
        public string Type { get; set; }
        public string Note { get; set; }
        public decimal Amount { get; set; }
    }

    public class LookupItem
    {
        public string value { get; set; }
        public string label { get; set; }
        public string symbol { get; set; }
    }

    public class DepositItem
    {
        public string type { get; set; }
        public string label { get; set; }
        public int level { get; set; }
        public string value { get; set; }
    }
    public class AccountRow
    {
        public int id { get; set; }
        public string accountNo { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int? parentId { get; set; }
    }

    public class CustomerDto
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string latin { get; set; }
        public int count { get; set; }
        public decimal balance { get; set; }
        public decimal credit { get; set; }
    }
    public class OpenInvoiceDto
    {
        public int? id { get; set; }
        public string no { get; set; }
        public string type { get; set; }
        public string kind { get; set; }
        public string className { get; set; }
        public string date { get; set; }
        public decimal total { get; set; }
        public decimal open { get; set; }
    }
    public class CollectionListRow
    {
        public int id { get; set; }
        public string no { get; set; }
        public string date { get; set; }
        public string customer { get; set; }
        public string method { get; set; }
        public string deposit { get; set; }
        public string sym { get; set; }
        public decimal grand { get; set; }
        public decimal credit { get; set; }
    }
}