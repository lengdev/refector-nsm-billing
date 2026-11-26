using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Expenses")]
public class TblExpense
{
    [Key]
    public int ExpenseID { get; set; }

    [StringLength(50)]
    public string ExpenseNo { get; set; }

    [StringLength(50)]
    public string PaymentNo { get; set; }

    public int ClassName { get; set; }
    public int? PaymentMethodID { get; set; }
    public int? PayeeID { get; set; }

    [StringLength(50)]
    public string PayeeType { get; set; }

    public string No { get; set; }
    public string NoPrefix { get; set; }
    public int? PaymentAccountID { get; set; }

    public DateTime? PaymentDate { get; set; }
    public DateTime? PaidOnDate { get; set; }

    public string PVNo { get; set; }
    public string PVNoPrefix { get; set; }
    public string CheckedBy { get; set; }
    public string CheckedByTelegramUserName { get; set; }
    public int? CheckedByUserID { get; set; }

    public string VerifiedBy { get; set; }
    public string VerifiedByTelegramUserName { get; set; }
    public int? VerifiedByUserID { get; set; }
    public DateTime? VerifiedDate { get; set; }

    public string ApprovedBy { get; set; }
    public string ApprovedByTelegramUserName { get; set; }
    public int? ApprovedByUserID { get; set; }

    public string PostingNumber { get; set; }
    public string PostingPrefix { get; set; }
    public string Reference { get; set; }
    public string BankInfo { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string BankerName { get; set; }
    public string BankAccountName { get; set; }
    public string BankAccountNo { get; set; }
    public string ChequeBankerName { get; set; }
    public string ChequeTo { get; set; }
    public string ChequeNo { get; set; }

    public decimal? Total { get; set; }

    [StringLength(50)]
    public string Status { get; set; }

    public int? CreateByUserID { get; set; }
    public int? ModifyByUserID { get; set; }
    public DateTime? ModifyDate { get; set; }

    public int? SystemID { get; set; }
    public int? WarehouseID { get; set; }

    [StringLength(50)]
    public string DomainID { get; set; }

    public int? TelegramMessageID { get; set; }
    public long? TelegramChatID { get; set; }
    public DateTime? TelegramSentDate { get; set; }
    public int? TelegramReplyMessageID { get; set; }
    public DateTime? CheckedDate { get; set; }

    [Required]
    [StringLength(20)]
    public string PaymentStatus { get; set; }

    public bool? IsPosted { get; set; }
    public DateTime? PostedDate { get; set; }
    public string PostedBy { get; set; }
    public int? PostedByUserID { get; set; }
    public string PostedByTelegramUserName { get; set; }

    public decimal? ExchangeRate { get; set; }

    public string FileName { get; set; }
    public DateTime? PaymentTermsID { get; set; }

    [StringLength(50)]
    public string TermName { get; set; }

    public decimal? TotalAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public decimal? BalanceAmount { get; set; }

    public DateTime? MakePostedData { get; set; }
    public string PostingMemo { get; set; }
    public string PostingDOC { get; set; }

    public bool? IsChecked { get; set; }
    public bool? IsVerified { get; set; }

    [StringLength(50)]
    public string JournalNo { get; set; }

    public string Description { get; set; }
    public DateTime? PaymentTeam { get; set; }

    [StringLength(50)]
    public string PostedPaymentNo { get; set; }

    public bool? IsRequestedPayment { get; set; }

    [StringLength(50)]
    public string BillInvoiceNo { get; set; }

    public decimal? GrantTotalAsRiel { get; set; }
    public decimal? Discount { get; set; }
    public decimal? DiscountItem { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? VAT { get; set; }

    public int? TransactionID { get; set; }

    [StringLength(50)]
    public string ExpenseTransactionType { get; set; }

    public decimal? HeaderTax { get; set; }

    public string VATRemark { get; set; }

    [StringLength(50)]
    public string Type { get; set; }
}