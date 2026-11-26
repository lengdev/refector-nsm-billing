
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("TblAccount_JournalEntries")]
public class TblAccountJournalEntries
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TransactionID { get; set; }

    public int? ExpensesID { get; set; }
    public int EntryID { get; set; }
    public string Account { get; set; }

    public int? PayeeID { get; set; }
    public int? VendorID { get; set; }
    public int? EmployeeID { get; set; }
    public int? CustomerID { get; set; }
    public int? TransactionTypeID { get; set; }

    public int ItemID { get; set; }

    [StringLength(50)]
    public string TransactionType { get; set; }

    [StringLength(50)]
    public string Type { get; set; }

    public DateTime? InvoiceDate { get; set; }

    [StringLength(50)]
    public string InvoiceNo { get; set; }

    [StringLength(50)]
    public string BillNo { get; set; }

    [StringLength(50)]
    public string PONo { get; set; }

    [StringLength(50)]
    public string GRN { get; set; }

    [StringLength(50)]
    public string DraftNo { get; set; }

    [StringLength(50)]
    public string PVNo { get; set; }

    [StringLength(50)]
    public string ReceiptNo { get; set; }

    [StringLength(50)]
    public string OfficialReceiptNo { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string Description { get; set; }

    // Decimal(18,2) - Configure precision in DBContext
    public decimal? Discount { get; set; }

    // Decimal(18,2) - Configure precision in DBContext
    public decimal? VAT { get; set; }

    // Decimal(18,2) - Configure precision in DBContext
    public decimal? Amount { get; set; }

    [StringLength(50)]
    public string Currency { get; set; }

    public int? SystemID { get; set; }
    public int? WarehouseID { get; set; }

    [StringLength(50)]
    public string DomainID { get; set; }

    public int? CreateByUserID { get; set; }
    public DateTime? CreateDate { get; set; }
    public int? ModifyByUserID { get; set; }
    public DateTime? ModifyDate { get; set; }

    public int? AccountID { get; set; }

    [StringLength(50)]
    public string TransactionNo { get; set; }

    public int? UpdateByUserID { get; set; }
    public DateTime? UpdateDate { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime? TransactionDateOnly { get; private set; }

    public int? OpeningBalanceID { get; set; }

    [StringLength(50)]
    public string ExpenseNo { get; set; }

    [StringLength(50)]
    public string PostingNo { get; set; }

    public int? ExpenseID { get; set; }
    public int? FromAccountID { get; set; }
    public int? ToAccountID { get; set; }
    public int? ClassNameID { get; set; }

    public bool? IsPosted { get; set; }

    [StringLength(50)]
    public string JournalNo { get; set; }

    public int? StatusID { get; set; } = 60;

    [StringLength(100)]
    public string CheckedBy { get; set; }

    public DateTime? CheckedDate { get; set; }
    public int? CheckedByUserID { get; set; }

    [StringLength(100)]
    public string VerifiedBy { get; set; }

    public DateTime? VerifiedDate { get; set; }
    public int? VerifiedByUserID { get; set; }

    [StringLength(100)]
    public string ApprovedBy { get; set; }

    public DateTime? ApprovalDate { get; set; }
    public int? ApprovedByUserID { get; set; }

    [StringLength(100)]
    public string DisapprovedBy { get; set; }

    public DateTime? DisapprovedDate { get; set; }

    public bool? IsAdjustment { get; set; } = false;

    [StringLength(50)]

    public string PaymentNo { get; set; }
    public long? TelegramMessageID { get; set; }
    public long? TelegramChatID { get; set; }
    public DateTime? TelegramSentDate { get; set; }

    [StringLength(50)]
    public string ExpenseTransactionType { get; set; }

    public int? VendorDepositID { get; set; }
    public decimal? Credit { get; set; }
    public decimal? Debit { get; set; }

    // Decimal(18,2) - Configure precision in DBContext
    public decimal? AppliedAmount { get; set; }
}
