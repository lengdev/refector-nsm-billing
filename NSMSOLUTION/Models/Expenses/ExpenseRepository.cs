using MVC;
using NSMSOLUTION.Controllers.Expenses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace NSMSOLUTION.Models.Expenses
{
    public class ExpenseRepository
    {
        private readonly string _connectionString;
        public ExpenseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ExpensePostingResult InsertExpense(Expense expense)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        //string[] vendorDepositTypes =
                        //    {
                        //        "VendorDeposit",
                        //        "Vendor Deposit",
                        //        "Supplier Deposit",
                        //        "Vendor Advance",
                        //        "Supplier Advance"
                        //    };
                        string[] vendorDepositTypes =
                            {
                                
                            };

                        string ExpenseTransactionType =
                            expense.Details?.Any(a =>
                                !string.IsNullOrEmpty(a.AccountName) &&
                                vendorDepositTypes.Any(x =>
                                    a.AccountName.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0)
                            ) == true
                                ? "Vendor Deposit"
                                : "Expense";
                        //if (ExpenseTransactionType != "Vendor Deposit")
                        //{
                        if (expense.BalanceAmount == 0 && !expense.IsFromBills)
                        {
                            ExpenseTransactionType = "Direct Payment";
                        }
                        else
                        {
                            ExpenseTransactionType = "Bill";
                        }
                        //}
                        if (expense.PaymentNo == "")
                        {
                            int existingExpense = expense.ExpenseID;
                            expense.ExistingExpenseID = existingExpense;
                            
                            expense.ExpenseTransactionType = ExpenseTransactionType;
                            if (ExpenseTransactionType == "Bill")
                            {
                                if (expense.VendorDepositID > 0)
                                {
                                    expense.BalanceAmount = (expense.AmountToApply - expense.TotalAmount);
                                    expense.PaidAmount = expense.AmountToApply;
                                }
                                else
                                {
                                    expense.BalanceAmount = expense.PaidAmount;
                                    expense.PaidAmount = 0;
                                }
                            }
                                ExpensePostingResult postingResult = InsertExpenseHeader(expense, conn, tran);
                            int TransactionID = postingResult.TransactionID;
                            decimal ItemGrandTotal = expense.Details?.Sum(x => x.ItemGrandTotal) ?? 0m;
                            //expense.TotalAmount = ItemGrandTotal;
                            // ============================================================
                            // 1. Create Expense Transaction
                            // ============================================================

                            //int expenseTransactionId = CreateExpenseTransaction(
                            //    expense,
                            //    postingResult,
                            //    conn,
                            //    tran);


                            // ============================================================
                            // 2. Create Expense Journal Entries
                            // ============================================================
                            foreach (var d in expense.Details)
                            {
                                InsertExpenseDetail(
                                    expense,
                                    postingResult,
                                    d,
                                    TransactionID,
                                    conn,
                                    tran);
                            }
                            InsertCombinedVATJournalEntry(expense, postingResult, conn, tran);
                            expense.ExpenseID = postingResult.ExpenseID;
                            expense.PayeeID = postingResult.PayeeID;
                            expense.APAccountID = postingResult.APAccountID;
                            expense.PaymentAccountID = postingResult.PaymentAccountID;
                            expense.ExpenseNo = postingResult.ExpenseNo;
                            //InsertExpensePaymentJournalEntries(expense, transactionId, expense.PaidAmount, postingResult.JournalNo, postingResult.PaymentNo, conn, tran);
                            decimal wholeExpenseDiscount = expense.Discount;

                            //if (wholeExpenseDiscount > 0 && ExpenseTransactionType != "Vendor Deposit")
                            //{
                                InsertWholeExpenseDiscountJournal(
                                    expense,
                                    postingResult,
                                    wholeExpenseDiscount,
                                    conn,
                                    tran
                                );
                            //}
                            //if (ExpenseTransactionType != "Direct Payment")// && ExpenseTransactionType != "Bill")
                            //{
                            //    expense.BalanceAmount = expense.PaidAmount;
                            //    expense.PaidAmount = 0;                                
                            //    InsertExpensePayment(expense, postingResult, expense.PaymentAmount, conn, tran);
                            //}
                            if (ExpenseTransactionType == "Bill")
                            {
                                if (expense.VendorDepositID > 0)
                                {
                                    expense.BalanceAmount = (expense.AmountToApply - expense.TotalAmount);
                                    expense.PaidAmount = expense.AmountToApply;
                                }
                                else
                                {
                                    expense.BalanceAmount = expense.PaidAmount;
                                    expense.PaidAmount = 0;
                                }
                                expense.BillExpenseID = expense.ExpenseID;
                                InsertVendorBillAPEntry(expense, conn, tran);
                                if (expense.VendorDepositID > 0)
                                {
                                    decimal appliedAmount = ApplyVendorDeposit(
                                        APAccountID: expense.PaymentAccountID,
                                        vendorDepositID: expense.VendorDepositID,
                                        billExpenseID: expense.ExpenseID,
                                        amountToApply: expense.AmountToApply,
                                        systemID: expense.SystemID.Value,
                                        warehouseID: expense.WarehouseID.Value,
                                        domainID: expense.DomainID,
                                        createByUserID: int.Parse(expense.CreateByUserID),
                                        conn: conn,
                                        tran: tran,
                                        existingExpense: existingExpense
                                    );
                                }
                            }
                            vendorDepositTypes = new[]
                            {
                                    "VendorDeposit",
                                    "Vendor Deposit",
                                    "Vendor Deposits",
                                    "Supplier Deposit",
                                    "Vendor Advance",
                                    "Supplier Advance",
                                    "Supplier / Vendor Deposits"
                            };
                            if (!expense.IsFromBills)
                            {
                                var vendorDeposits = expense.Details?
        .Where(a =>
            !string.IsNullOrWhiteSpace(a.AccountName) &&
            vendorDepositTypes.Contains(
                a.AccountName.Trim(),
                StringComparer.OrdinalIgnoreCase))
        .ToList();

                                if (vendorDeposits != null && vendorDeposits.Count > 0)
                                {
                                    var depositAmount = vendorDeposits.Sum(x => x.ItemGrandTotal);
                                    var description = string.Join(Environment.NewLine,
                                        vendorDeposits
                                            .Where(x => !string.IsNullOrWhiteSpace(x.ItemName))
                                            .Select(x => x.ItemName.Trim()));
                                    if (depositAmount > 0)
                                    {
                                        var DepositAccountID = vendorDeposits
                                            .Select(x => x.AccountID)
                                            .FirstOrDefault();
                                        const string sql = @"

DECLARE @depositNo NVARCHAR(50) = '';

DECLARE @ResultVendorDepositID INT = 0;

BEGIN TRY
    BEGIN TRANSACTION;

    IF ISNULL(@VendorDepositID, 0) = 0
    BEGIN
        -- ==========================================
        -- NEW VENDOR DEPOSIT
        -- ==========================================

        EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR]
            'VD{yy}{MM}_{00000}',
            'Vendor Deposit',
            @SystemID,
            @WarehouseID,
            @depositNo OUT,
            1,
            0,
            @DomainID;

        INSERT INTO dbo.TblVendorDeposit
        (
            VendorID,
            TransactionID,
            ExpenseID,
            DepositNo,
            DepositDate,
            Amount,
            AppliedAmount,
            AccountID,
            BankAccountID,
            Status,
            DomainID,
            SystemID,
            WarehouseID,
            CreateByUserID,
            CreateDate,
            Description,
            PONo,
            DepositAccountID
        )
        VALUES
        (
            @VendorID,
            @TransactionID,
            @ExpenseID,
            @depositNo,
            @DepositDate,
            @Amount,
            0,
            @AccountID,
            @BankAccountID,
            'Open',
            @DomainID,
            @SystemID,
            @WarehouseID,
            @CreateByUserID,
            GETDATE(),
            @Description,
            @PONo,
            @DepositAccountID
        );

        SET @ResultVendorDepositID =
            CAST(SCOPE_IDENTITY() AS INT);
    END
    ELSE
    BEGIN
        -- ==========================================
        -- UPDATE EXISTING VENDOR DEPOSIT
        -- ==========================================

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.TblVendorDeposit
            WHERE VendorDepositID = @VendorDepositID
              AND VendorID = @VendorID
              AND SystemID = @SystemID
              AND WarehouseID = @WarehouseID
              AND DomainID = @DomainID
        )
        BEGIN
            THROW 50001, 'Vendor deposit not found.', 1;
        END;

        -- Prevent deposit amount from being lower
        -- than the amount already applied.
        IF @Amount <
        (
            SELECT ISNULL(AppliedAmount, 0)
            FROM dbo.TblVendorDeposit
            WHERE VendorDepositID = @VendorDepositID
        )
        BEGIN
            THROW 50002,
                'Deposit amount cannot be less than applied amount.',
                1;
        END;

        UPDATE dbo.TblVendorDeposit
        SET
            VendorID = @VendorID,
            TransactionID = @TransactionID,
            ExpenseID = @ExpenseID,
            DepositDate = @DepositDate,
            Amount = @Amount,
            AccountID = @AccountID,
            BankAccountID = @BankAccountID,

            -- Preserve existing AppliedAmount.
            AppliedAmount = ISNULL(AppliedAmount, 0),

            Status =
                CASE
                    WHEN ISNULL(AppliedAmount, 0) <= 0
                        THEN 'Open'

                    WHEN ISNULL(AppliedAmount, 0) < @Amount
                        THEN 'Partially Applied'

                    ELSE Status
                END,

            Description = @Description,
            PONo = @PONo,
            DepositAccountID = @DepositAccountID

        WHERE VendorDepositID = @VendorDepositID
          AND VendorID = @VendorID
          AND SystemID = @SystemID
          AND WarehouseID = @WarehouseID
          AND DomainID = @DomainID;

        SET @ResultVendorDepositID = @VendorDepositID;

        SELECT @depositNo = DepositNo
        FROM dbo.TblVendorDeposit
        WHERE VendorDepositID = @VendorDepositID;
    END;

    COMMIT TRANSACTION;

    -- Return result for both New and Update.
    SELECT
        @ResultVendorDepositID AS VendorDepositID,
        @depositNo AS DepositNo,
        CASE
            WHEN @VendorDepositID = 0 THEN 'INSERT'
            ELSE 'UPDATE'
        END AS ActionType,
        CAST(1 AS BIT) AS Success;

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
";


                                        using (var cmd = new SqlCommand(sql, conn, tran))
                                        {
                                            // Existing VendorDepositID:
                                            // 0 = New
                                            // > 0 = Update
                                            cmd.Parameters.Add("@VendorDepositID", SqlDbType.Int).Value = expense.VendorDepositID;

                                            cmd.Parameters.Add("@DepositAccountID", SqlDbType.Int).Value =
                                                DepositAccountID;

                                            cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value =
                                                expense.PayeeID;

                                            cmd.Parameters.Add("@TransactionID", SqlDbType.Int).Value =
                                                postingResult.TransactionID;

                                            cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value =
                                                postingResult.ExpenseID;

                                            cmd.Parameters.Add("@DepositDate", SqlDbType.DateTime).Value =
                                                expense.TransactionDate;

                                            cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value =
                                                depositAmount;

                                            cmd.Parameters.Add("@AccountID", SqlDbType.Int).Value =
                                                DepositAccountID;

                                            // Nullable BankAccountID
                                            cmd.Parameters.Add("@BankAccountID", SqlDbType.Int).Value =
                                                expense.PaymentAccountID > 0
                                                    ? (object)expense.PaymentAccountID
                                                    : DBNull.Value;

                                            cmd.Parameters.Add("@PONo", SqlDbType.NVarChar, 50).Value =
                                                string.IsNullOrWhiteSpace(expense.Reference)
                                                    ? (object)DBNull.Value
                                                    : expense.Reference.Trim();

                                            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value =
                                                string.IsNullOrWhiteSpace(description)
                                                    ? (object)DBNull.Value
                                                    : description.Trim();

                                            cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar, 50).Value =
                                                expense.DomainID;

                                            cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value =
                                                expense.SystemID;

                                            cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value =
                                                expense.WarehouseID;

                                            cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int).Value =
                                                expense.CreateByUserID;

                                            // Execute INSERT or UPDATE
                                            var result = cmd.ExecuteScalar();

                                            if (result == null || result == DBNull.Value)
                                            {
                                                throw new Exception(
                                                    "Unable to create or update Vendor Deposit.");
                                            }

                                            var vendorDepositID = Convert.ToInt32(result);

                                            // Keep the ID for subsequent processing.
                                            expense.VendorDepositID = vendorDepositID;
                                        }
                                    }
                                }
                            }
                            if (existingExpense != 0)
                                DeleteExpenseDetails(existingExpense, conn, tran);
                            tran.Commit();

                            return postingResult;
                        }
                        else
                        {
                            HTML html = new HTML();
                            string sql = @"
DECLARE
    @PaymentStatus NVARCHAR(50),
    @TPaidAmount DECIMAL(18,2),
    @BalanceAmount DECIMAL(18,2),
    @OutPutPaymentNo NVARCHAR(50),
    @PaymentNo NVARCHAR(50),
    @IsRequestedPayment BIT = 0,
    @CurrentPaymentNo NVARCHAR(50),

    /* =========================================================
       ACCOUNTING VARIABLES
       ========================================================= */
    @AccountingAmount DECIMAL(18,2),
    @OriginalAP DECIMAL(18,2),
    @PreviousPaidAP DECIMAL(18,2),
    @RemainingAP DECIMAL(18,2),
    @NewRemainingAP DECIMAL(18,2),
    @APAccountID INT,
    @APAccountNo NVARCHAR(50),
    @APAccountName NVARCHAR(50),
    @PaymentAccountName NVARCHAR(MAX),
    @JournalNo NVARCHAR(100),
    @NewTransactionID INT;


/* =============================================================
   CURRENT PAYMENT NUMBER
   ============================================================= */

SET @CurrentPaymentNo = '" + expense.PostedPaymentNo + @"';


/* =============================================================
   CHECK EXISTING REQUESTED PAYMENT
   ============================================================= */

IF NULLIF(@CurrentPaymentNo, '') IS NOT NULL
BEGIN

    SELECT TOP 1
        @IsRequestedPayment = ISNULL(IsRequestedPayment, 0)
    FROM dbo.Expense_PaymentTransactions
    WHERE PaymentNo = @CurrentPaymentNo;

END;


/* =============================================================
   CALCULATE PREVIOUS PAID AMOUNT
   ============================================================= */

IF @IsRequestedPayment = 1
BEGIN

    SELECT
        @TPaidAmount =
            ISNULL(
                SUM(ISNULL(PaidAmount, 0)),
                0
            )
    FROM dbo.Expense_PaymentTransactions
    WHERE ExpenseID = @ExpenseID
      AND PaymentNo <> @CurrentPaymentNo;

END
ELSE
BEGIN

    SELECT
        @TPaidAmount =
            ISNULL(
                SUM(ISNULL(PaidAmount, 0)),
                0
            )
    FROM dbo.Expense_PaymentTransactions
    WHERE ExpenseID = @ExpenseID;

END;


/* =============================================================
   ADD CURRENT PAYMENT
   ============================================================= */

SET @TPaidAmount =
    ROUND(
        @TPaidAmount + ISNULL(@PaidAmount, 0),
        2
    );


/* =============================================================
   PREVENT OVER PAYMENT
   ============================================================= */

IF @TPaidAmount > @TotalAmount
BEGIN

    RAISERROR(
        'Payment amount exceeds the remaining Accounts Payable balance.',
        16,
        1
    );

    RETURN;

END;


/* =============================================================
   BALANCE
   ============================================================= */

SET @BalanceAmount =
    ROUND(
        @TotalAmount - @TPaidAmount,
        2
    );


IF @BalanceAmount < 0
    SET @BalanceAmount = 0;


/* =============================================================
   PAYMENT STATUS
   ============================================================= */

IF @TPaidAmount <= 0
BEGIN

    SET @PaymentStatus = 'Unpaid';

END
ELSE IF @TPaidAmount < @TotalAmount
BEGIN

    SET @PaymentStatus = 'Partial';

END
ELSE
BEGIN

    SET @PaymentStatus = 'Paid';

END;


/* =============================================================
   NEW PAYMENT
   ============================================================= */

IF @IsRequestedPayment = 0
BEGIN

    EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR]
        'PAY{yy}{MM}{dd}-{00000}',
        'PaymentNo',
        @SystemID,
        @WarehouseID,
        @PaymentNo OUT,
        1,
        0,
        @DomainID;


    IF NULLIF(@PaymentNo, '') IS NULL
    BEGIN

        RAISERROR(
            'Failed to generate Payment Number.',
            16,
            1
        );

        RETURN;

    END;


    /* ---------------------------------------------------------
       Update requested payment if applicable
       --------------------------------------------------------- */

    UPDATE dbo.Expense_PaymentTransactions
    SET
        PostedPaymentNo = @PaymentNo,
        IsRequestedPayment = 0
    WHERE PaymentNo = @PostedPaymentNo;


    /* ---------------------------------------------------------
       Update Expense
       --------------------------------------------------------- */

    UPDATE dbo.Expenses
    SET
        PostedPaymentNo = @PaymentNo,
        IsRequestedPayment = 0
    WHERE ExpenseID = @ExpenseID;


    /* ---------------------------------------------------------
       Insert Posted Payment
       --------------------------------------------------------- */

    INSERT INTO dbo.Expense_PaymentTransactions
    (
        ExpenseID,
        PaymentNo,
        ExpenseNo,
        ClassName,
        PaymentMethodID,
        PayeeID,
        PaymentAccountID,
        PaymentTeam,
        PaidOnDate,

        IsChecked,
        CheckedBy,
        CheckedByUserID,
        CheckedByTelegramUserName,

        IsVerified,
        VerifiedBy,
        VerifiedByUserID,
        VerifiedByTelegramUserName,

        IsPosted,

        Reference,
        ExchangeRate,
        Description,

        BankerName,
        BankAccountName,
        BankAccountNo,

        ChequeBankerName,
        ChequeTo,
        ChequeNo,

        Status,

        CreatedDate,
        CreateByUserID,

        FileName,

        TotalAmount,
        PaidAmount,
        BalanceAmount,

        PaymentStatus,

        SystemID,
        WarehouseID,
        DomainID,
        No,

        IsRequestedPayment,

        Total,
        TotalPaidAmount,
        TotalBalanceAmount,
        PaymentDate,
        GrantTotalAsRiel,
        Discount,
        ExpenseTransactionType
    )
    VALUES
    (
        @ExpenseID,
        @PaymentNo,
        @ExpenseNo,
        @ClassID,
        @PaymentMethodID,
        @VendorID,
        @PaymentAccountID,
        @PaymentTermsID,
        @PaidOnDate,

        0,
        @CheckedBy,
        @CheckedByUserID,
        @CheckedByTelegramUserName,

        0,
        @VerifiedBy,
        @VerifiedByUserID,
        @VerifiedByTelegramUserName,

        0,

        @Reference,
        @ExchangeRate,
        @Description,

        @BankerName,
        @BankAccountName,
        @BankAccountNo,

        @ChequeBankerName,
        @ChequeTo,
        @ChequeNo,

        @Status,

        @CreateDate,
        @CreateByUserID,

        @FileName,

        @TotalAmount,
        @PaidAmount,
        @BalanceAmount,

        @PaymentStatus,

        @SystemID,
        @WarehouseID,
        @DomainID,
        @No,

        1,

        @TotalAmount,
        @TPaidAmount,
        @BalanceAmount,
        @PaymentDate,
        @GrantTotalAsRiel,
        @Discount,
        @ExpenseTransactionType
    );


    SET @OutPutPaymentNo = @PaymentNo;

END
ELSE
BEGIN

    /* =========================================================
       UPDATE EXISTING REQUESTED PAYMENT
       ========================================================= */

    UPDATE dbo.Expense_PaymentTransactions
    SET

        ExpenseID = @ExpenseID,
        ExpenseNo = @ExpenseNo,
        ClassName = @ClassID,
        PaymentMethodID = @PaymentMethodID,
        PayeeID = @VendorID,
        PaymentAccountID = @PaymentAccountID,
        PaymentTeam = @PaymentTermsID,
        PaidOnDate = @PaidOnDate,
        PaymentDate = @PaymentDate,

        IsChecked = 0,
        CheckedBy = @CheckedBy,
        CheckedByUserID = @CheckedByUserID,
        CheckedByTelegramUserName =
            @CheckedByTelegramUserName,

        IsVerified = 0,
        VerifiedBy = @VerifiedBy,
        VerifiedByUserID = @VerifiedByUserID,
        VerifiedByTelegramUserName =
            @VerifiedByTelegramUserName,

        IsPosted = 0,

        Reference = @Reference,
        ExchangeRate = @ExchangeRate,
        Description = @Description,

        BankerName = @BankerName,
        BankAccountName = @BankAccountName,
        BankAccountNo = @BankAccountNo,

        ChequeBankerName = @ChequeBankerName,
        ChequeTo = @ChequeTo,
        ChequeNo = @ChequeNo,

        Status = @Status,

        CreatedDate = @CreateDate,
        CreateByUserID = @CreateByUserID,

        FileName = @FileName,

        Total = @TotalAmount,
        TotalAmount = @TotalAmount,

        PaidAmount = @PaidAmount,
        BalanceAmount = @BalanceAmount,

        PaymentStatus = @PaymentStatus,

        SystemID = @SystemID,
        WarehouseID = @WarehouseID,
        DomainID = @DomainID,
        No = @No,

        IsRequestedPayment = 1,

        ModifyByUserID = @ModifyByUserID,
        ModifyDate = @ModifyDate,

        PostedByTelegramUserName = @PostedByTelegramUserName,
        PostedByUserID = @PostedByUserID,
        PostedBy = @PostedBy,

        TotalPaidAmount = @TPaidAmount,
        TotalBalanceAmount = @BalanceAmount,

        GrantTotalAsRiel = @GrantTotalAsRiel,
        Discount = @Discount,
        ExpenseTransactionType = @ExpenseTransactionType

    WHERE PaymentNo = @CurrentPaymentNo;


    SET @OutPutPaymentNo = @CurrentPaymentNo;

END;


/* =============================================================
   ACCOUNTING POSTING
   ============================================================= */

/*
    IMPORTANT:

    Only delete/recreate the accounting entries belonging
    to THIS payment.

    Do NOT delete the original Purchase entries.
*/

DELETE FROM dbo.TblAccount_Transaction
WHERE ExpenseID = @ExpenseID
  AND PaymentNo = @OutPutPaymentNo;


DELETE FROM dbo.TblAccount_JournalEntries
WHERE ExpenseID = @ExpenseID
  AND PaymentNo = @OutPutPaymentNo;


/* =============================================================
   ONLY POST WHEN PAYMENT > 0
   ============================================================= */

IF ISNULL(@PaidAmount, 0) > 0
BEGIN

    SET @AccountingAmount =
        ROUND(
            ISNULL(@PaidAmount, 0),
            2
        );


    /* =========================================================
       GENERATE / SET JOURNAL NUMBER

       Use a real JV number if your system has one.

       If PaymentNo is intentionally your journal reference,
       this is also safe.
       ========================================================= */

    EXEC    [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR]
            'JV{yy}{MM}_{00000}',
            'JournalNo',
            @SystemID,
            @WarehouseID,
            @JournalNo OUTPUT,
            1,
            0,
            @DomainID;
    --SET @JournalNo = @OutPutPaymentNo;


    /* =========================================================
       FIND ACCOUNTS PAYABLE

       A/P must normally have Credit normal side.
       ========================================================= */

    SELECT TOP 1
        @APAccountID = ChartOfAccountID, @APAccountNo = AccountNo, @APAccountName = AccountName
    FROM dbo.TblChartOfAccount
    WHERE NormalSide = 'Credit'
      AND AccountName LIKE '%Payable%'
      AND ISNULL(ParentAccountID, 0) = 0
      AND SystemID = @SystemID
      AND WarehouseID = @WarehouseID
      AND DomainID = @DomainID
      AND ISNULL(StatusID, 1) <> 0
    ORDER BY

        CASE
            WHEN AccountName =
                'Accounts Payable (A/P)'
                THEN 0

            WHEN AccountName =
                'Account Payable (A/P)'
                THEN 1

            WHEN AccountName LIKE
                '%Accounts Payable%'
                THEN 2

            WHEN AccountName LIKE
                '%Account Payable%'
                THEN 3

            ELSE 4
        END,

        ChartOfAccountID;

    SELECT @PaymentAccountName = AccountNo +'-'+ AccountName
    FROM dbo.TblChartOfAccount
    WHERE ChartOfAccountID = @PaymentAccountID;
    
    IF @APAccountID IS NULL
    BEGIN

        RAISERROR(
            'Accounts Payable account could not be found.',
            16,
            1
        );

        RETURN;

    END;


    /* =========================================================
       PAYMENT ACCOUNT
       ========================================================= */

    IF ISNULL(@PaymentAccountID, 0) = 0
    BEGIN

        RAISERROR(
            'Payment Account could not be found.',
            16,
            1
        );

        RETURN;

    END;


    /* =========================================================
       CALCULATE ORIGINAL A/P
       
       Example:
       
       Purchase:
           CR A/P = 1000

       Original A/P = 1000
       ========================================================= */

    SELECT
        @OriginalAP =
            ISNULL(
                SUM(
                    CASE
                        WHEN Type = 'Expense'
                        THEN
                            ISNULL(Credit, 0)
                            -
                            ISNULL(Debit, 0)

                        ELSE 0
                    END
                ),
                0
            )
    FROM dbo.TblAccount_JournalEntries
    WHERE ExpenseID = @ExpenseID
      AND AccountID = @APAccountID;


    /* =========================================================
       CALCULATE PREVIOUS PAYMENTS

       Example:

       Payment #1 = 50

       PreviousPaidAP = 50
       ========================================================= */

    SELECT
        @PreviousPaidAP =
            ISNULL(
                SUM(
                    CASE
                        WHEN Type = 'Bill Payment'
                        THEN
                            ISNULL(Debit, 0)
                            -
                            ISNULL(Credit, 0)

                        ELSE 0
                    END
                ),
                0
            )
    FROM dbo.TblAccount_JournalEntries
    WHERE ExpenseID = @ExpenseID
      AND AccountID = @APAccountID
      AND PaymentNo <> @OutPutPaymentNo;


    /* =========================================================
       CURRENT REMAINING A/P

       Example:

       Original A/P = 1000
       Previous     = 50

       Remaining = 950
       ========================================================= */

    SET @RemainingAP =
        ROUND(
            @OriginalAP - @PreviousPaidAP,
            2
        );


    IF @RemainingAP < 0
        SET @RemainingAP = 0;


    /* =========================================================
       PREVENT OVER-PAYMENT
       ========================================================= */

    IF @AccountingAmount > @RemainingAP
    BEGIN

        RAISERROR(
            'Payment amount exceeds the remaining Accounts Payable balance.',
            16,
            1
        );

        RETURN;

    END;


    /* =========================================================
       NEW REMAINING A/P

       Example:

       Remaining before payment = 950
       Current payment          = 60

       New remaining = 890
       ========================================================= */

    SET @NewRemainingAP =
        ROUND(
            @RemainingAP - @AccountingAmount,
            2
        );


    IF @NewRemainingAP < 0
        SET @NewRemainingAP = 0;


    /* =========================================================
       INSERT PAYMENT TRANSACTION
       ========================================================= */

    INSERT INTO dbo.TblAccount_Transaction
    (
        ExpensesID,
        PayeeID,
        VendorID,
        TransactionType,
        Type,
        InvoiceDate,
        PVNo,
        TransactionDate,
        Description,
        Discount,
        VAT,
        Amount,
        Currency,
        SystemID,
        WarehouseID,
        DomainID,
        CreateByUserID,
        CreateDate,
        AccountID,
        TransactionNo,
        ExpenseNo,
        PostingNo,
        ExpenseID,
        FromAccountID,
        ToAccountID,
        ClassNameID,
        IsPosted,
        JournalNo,
        StatusID,
        PaymentNo,
        InvoiceNo,
        ExpenseTransactionType
    )
    VALUES
    (
        @ExpenseID,
        @VendorID,
        @VendorID,

        'Bill Payment',
        'Bill Payment',

        @PaymentDate,
        @PaymentNo,

        @PaymentDate,

        'Payment - ' + @ExpenseNo,

        0,
        0,

        @AccountingAmount,

        'USD',

        @SystemID,
        @WarehouseID,
        @DomainID,

        @CreateByUserID,
        @CreateDate,

        @PaymentAccountID,

        @OutPutPaymentNo,
        @ExpenseNo,
        @OutPutPaymentNo,

        @ExpenseID,

        @APAccountID,
        @PaymentAccountID,

        @ClassID,

        0,

        @JournalNo,

        60,

        @OutPutPaymentNo,
        @ExpenseNo,
        @ExpenseTransactionType
    );


    SET @NewTransactionID =
        SCOPE_IDENTITY();


    /* =========================================================
       JOURNAL 1
       
       DEBIT ACCOUNTS PAYABLE

       Payment $60:

           DR A/P     60
       ========================================================= */

    INSERT INTO dbo.TblAccount_JournalEntries
    (
        Type,
        TransactionID,
        AccountID,
        Description,
        Debit,
        Credit,
        EntryDate,
        DomainID,
        TransactionDate,
        SystemID,
        WarehouseID,
        CreateByUserID,
        CreateDate,
        ExpenseID,
        ExpenseAmount,
        ItemTAX,
        ClassNameID,
        IsPosted,
        JournalNo,
        ExpenseNo,
        PaymentNo,
        InvoiceNo,
        ExpenseTransactionType
    )
    VALUES
    (
        'Bill Payment',

        @NewTransactionID,

        @APAccountID,

        @APAccountNo + '-' + @APAccountName,

        @AccountingAmount,
        0,

        @PaymentDate,

        @DomainID,

        @PaymentDate,

        @SystemID,
        @WarehouseID,

        @CreateByUserID,
        @CreateDate,

        @ExpenseID,

        @AccountingAmount,

        0,

        @ClassID,

        0,

        @JournalNo,

        @ExpenseNo,

        @OutPutPaymentNo,
        @ExpenseNo,
        @ExpenseTransactionType
    );


    /* =========================================================
       JOURNAL 2
       
       CREDIT BANK / CASH

       Payment $60:

           CR Bank     60
       ========================================================= */

    INSERT INTO dbo.TblAccount_JournalEntries
    (
        Type,
        TransactionID,
        AccountID,
        Description,
        Debit,
        Credit,
        EntryDate,
        DomainID,
        TransactionDate,
        SystemID,
        WarehouseID,
        CreateByUserID,
        CreateDate,
        ExpenseID,
        ExpenseAmount,
        ItemTAX,
        ClassNameID,
        IsPosted,
        JournalNo,
        ExpenseNo,
        PaymentNo,
        InvoiceNo,
        ExpenseTransactionType
    )
    VALUES
    (
        'Bill Payment',

        @NewTransactionID,

        @PaymentAccountID,

        @PaymentAccountName,

        0,

        @AccountingAmount,

        @PaymentDate,

        @DomainID,

        @PaymentDate,

        @SystemID,
        @WarehouseID,

        @CreateByUserID,
        @CreateDate,

        @ExpenseID,

        @AccountingAmount,

        0,

        @ClassID,

        0,

        @JournalNo,

        @ExpenseNo,

        @OutPutPaymentNo,
        @ExpenseNo,
        @ExpenseTransactionType
    );

END;


/* =============================================================
   RETURN RESULT
   ============================================================= */

SELECT

    @OutPutPaymentNo AS PaymentNo,

    @TPaidAmount AS TotalPaidAmount,

    @BalanceAmount AS TotalBalanceAmount,

    @BalanceAmount AS BalanceAmount,

    @PaymentStatus AS PaymentStatus;
";
                            // ======================================
                            // AUTO CALCULATE PAYMENT
                            // ======================================
                            expense.TotalAmount = expense.Total;

                            if (expense.PaidAmount < 0)
                                expense.PaidAmount = 0;

                            //                            string sqlTP = $@"
                            //update Expense_PaymentTransactions set PaidAmount = {expense.PaidAmount} where PaymentNo = '{expense.PostedPaymentNo}';

                            //select sum(PaidAmount) PaidAmount from Expense_PaymentTransactions where ExpenseID = " + expense.ExpenseID;
                            //                            SqlResult rst = html.SaveChangeData(sqlTP, false, true);

                            //decimal TPaidAmount = decimal.Parse(rst.Value.ToString());// decimal.Parse(dsTP.Tables[0].Rows[0]["PaidAmount"].ToString());

                            //if (expense.PostedPaymentNo == "")
                            //    TPaidAmount += expense.PaidAmount;

                            //expense.BalanceAmount = expense.TotalAmount - TPaidAmount;

                            //if (expense.BalanceAmount < 0)
                            //{
                            //    expense.PaidAmount = expense.TotalAmount;
                            //    expense.BalanceAmount = 0;
                            //}

                            //if (expense.BalanceAmount == 0)
                            //    expense.PaymentStatus = "Unpaid";
                            //else if (TPaidAmount < expense.BalanceAmount)
                            //    expense.PaymentStatus = "Partial";
                            //else
                            //    expense.PaymentStatus = "Paid";
                            ExpensePostingResult result = null;
                            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                            {

                                //==========================================
                                // Required
                                //==========================================
                                cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = expense.ExpenseID;
                                cmd.Parameters.Add("@ExpenseNo", SqlDbType.NVarChar).Value = expense.ExpenseNo;
                                cmd.Parameters.Add("@PostedPaymentNo", SqlDbType.NVarChar).Value = expense.PaymentNo;
                                cmd.Parameters.Add("@ExpenseTransactionType", SqlDbType.NVarChar).Value = expense.ExpenseTransactionType;
                                cmd.Parameters.Add("@PaymentDate", SqlDbType.DateTime).Value = expense.PaymentDate;
                                if (expense.PostedPaymentNo != "")
                                {
                                    cmd.Parameters.Add("@ModifyDate", SqlDbType.DateTime).Value =
                                    html.GetCurrentDate();

                                    cmd.Parameters.Add("@ModifyByUserID", SqlDbType.Int).Value =
                                        html.UserID;
                                }
                                else
                                {
                                    cmd.Parameters.Add("@ModifyDate", SqlDbType.DateTime).Value = DBNull.Value;

                                    cmd.Parameters.Add("@ModifyByUserID", SqlDbType.Int).Value = DBNull.Value;
                                }

                                cmd.Parameters.Add("@No", SqlDbType.NVarChar).Value = expense.No;

                                cmd.Parameters.Add("@ClassID", SqlDbType.Int).Value = expense.ClassName;
                                cmd.Parameters.Add("@ClassNameID", SqlDbType.Int).Value = expense.ClassName;
                                //cmd.Parameters.Add("@PaymentAccountName", SqlDbType.Int).Value = res.ClassName;

                                cmd.Parameters.Add("@PaymentMethodID", SqlDbType.Int).Value = expense.PaymentMethodID;

                                cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value = expense.VendorID;

                                cmd.Parameters.Add("@PaymentAccountID", SqlDbType.Int).Value = expense.PaymentAccountID;

                                cmd.Parameters.Add("@PaymentTermsID", SqlDbType.DateTime).Value = expense.PaymentTermsID.HasValue
                                        ? (object)expense.PaymentTermsID.Value
                                        : DBNull.Value;

                                cmd.Parameters.Add("@PaidOnDate", SqlDbType.DateTime).Value =
                                    expense.PaidOnDate.HasValue
                                        ? (object)expense.PaidOnDate.Value
                                        : DBNull.Value;

                                //==========================================
                                // Description
                                //==========================================

                                cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = expense.Description ?? "";

                                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar).Value = expense.Reference ?? "";

                                //==========================================
                                // Checked
                                //==========================================

                                cmd.Parameters.Add("@CheckedBy", SqlDbType.NVarChar).Value = expense.CheckedBy ?? "";

                                cmd.Parameters.Add("@CheckedByUserID", SqlDbType.Int).Value = expense.CheckedByUserID;

                                cmd.Parameters.Add("@CheckedByTelegramUserName", SqlDbType.NVarChar).Value =
                                    expense.CheckedByTelegramUserName ?? "";

                                //==========================================
                                // Verified
                                //==========================================

                                cmd.Parameters.Add("@VerifiedBy", SqlDbType.NVarChar).Value = expense.VerifiedBy ?? "";

                                cmd.Parameters.Add("@VerifiedByUserID", SqlDbType.Int).Value = expense.VerifiedByUserID;

                                cmd.Parameters.Add("@VerifiedByTelegramUserName", SqlDbType.NVarChar).Value =
                                    expense.VerifiedByTelegramUserName ?? "";

                                cmd.Parameters.Add("@PostedBy", SqlDbType.NVarChar).Value = expense.PostedBy ?? "";

                                cmd.Parameters.Add("@PostedByUserID", SqlDbType.Int).Value =
                                string.IsNullOrWhiteSpace(expense.PostedByUserID)
                                    ? DBNull.Value
                                    : (object)Convert.ToInt32(expense.PostedByUserID);

                                cmd.Parameters.Add("@PostedByTelegramUserName", SqlDbType.NVarChar).Value =
                                    expense.PostedByTelegramUserName ?? "";

                                //==========================================
                                // Bank
                                //==========================================

                                cmd.Parameters.Add("@BankerName", SqlDbType.NVarChar).Value =
                                    expense.BankerName ?? "";

                                cmd.Parameters.Add("@BankAccountName", SqlDbType.NVarChar).Value =
                                    expense.BankAccountName ?? "";

                                cmd.Parameters.Add("@BankAccountNo", SqlDbType.NVarChar).Value =
                                    expense.BankAccountNo ?? "";

                                cmd.Parameters.Add("@ChequeBankerName", SqlDbType.NVarChar).Value =
                                    expense.ChequeBankerName ?? "";

                                cmd.Parameters.Add("@ChequeTo", SqlDbType.NVarChar).Value =
                                    expense.ChequeTo ?? "";

                                cmd.Parameters.Add("@ChequeNo", SqlDbType.NVarChar).Value =
                                    expense.ChequeNo ?? "";

                                //==========================================
                                // Amount
                                //==========================================

                                cmd.Parameters.Add("@ExchangeRate", SqlDbType.Decimal).Value =
                                    expense.ExchangeRate;

                                cmd.Parameters.Add("@GrantTotalAsRiel", SqlDbType.Decimal).Value = expense.GrantTotalAsRiel;

                                cmd.Parameters.Add("@Discount", SqlDbType.Decimal).Value = expense.Discount;

                                cmd.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value =
                                    expense.TotalAmount;

                                cmd.Parameters.Add("@PaidAmount", SqlDbType.Decimal).Value = expense.PaidAmount;


                                //cmd.Parameters.Add("@TotalPaidAmount", SqlDbType.Decimal).Value = TPaidAmount;
                                //cmd.Parameters.Add("@TotalBalanceAmount", SqlDbType.Decimal).Value = expense.BalanceAmount;

                                //cmd.Parameters.Add("@BalanceAmount", SqlDbType.Decimal).Value =
                                //    expense.BalanceAmount;

                                //cmd.Parameters.Add("@PaymentStatus", SqlDbType.NVarChar).Value =
                                //    expense.PaymentStatus;

                                //==========================================
                                // Other
                                //==========================================

                                cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value =
                                    expense.Status;

                                cmd.Parameters.Add("@FileName", SqlDbType.NVarChar).Value =
                                    expense.FileName ?? "";

                                cmd.Parameters.Add("@CreateDate", SqlDbType.DateTime).Value =
                                    html.GetCurrentDate();

                                cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int).Value =
                                    html.UserID;

                                //==========================================
                                // Company
                                //==========================================

                                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value =
                                    html.SystemID;

                                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value =
                                    html.WarehouseID;

                                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar).Value =
                                    html.DomainID;

                                //==========================================                                

                                //cmd.ExecuteNonQuery();
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        result = new ExpensePostingResult
                                        {
                                            ExpenseID = expense.ExpenseID,
                                            PaymentNo = reader["PaymentNo"].ToString()
                                        };
                                    }
                                }
                            }
                            tran.Commit();
                            return result;
                        }
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        // ================= HEADER =================
        private ExpensePostingResult InsertExpenseHeader(
            Expense e,
            SqlConnection conn,
            SqlTransaction tran)
        {
            //HTML html = new HTML();
            //string[] vendorDepositTypes =
            //{
            //    "VendorDeposit",
            //    "Vendor Deposit",
            //    "Supplier Deposit",
            //    "Vendor Advance",
            //    "Supplier Advance"
            //};
            string[] vendorDepositTypes =
            {
            };

            //string ExpenseTransactionType =
            //    e.Details?.Any(a =>
            //        !string.IsNullOrEmpty(a.AccountName) &&
            //        vendorDepositTypes.Any(x =>
            //            a.AccountName.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0)
            //    ) == true
            //        ? "Vendor Deposit"
            //        : "Expense";
            //if (ExpenseTransactionType != "Vendor Deposit")
            //{
            //    if (e.BalanceAmount == 0)
            //    {
            //        ExpenseTransactionType = "Direct Payment";
            //    }
            //}
            //e.ExpenseTransactionType = ExpenseTransactionType;

            string sql = @"
declare @JournalNo nvarchar(50), @ExpenseID int, @PaymentAccountNo nvarchar(50), @PaymentAccountName nvarchar(50), @AccountNo nvarchar(50), @AccountName nvarchar(50), @PaymentNo nvarchar(50), @OutPrefixExpenseNo nvarchar(50), @OutPrefix nvarchar(50), @OutPrefixPV nvarchar(50), @IDENTITY int, @TransactionID  int, @APAccountID int, @NewVendorID int, @OutVD nvarchar(50), @PrefixVD nvarchar(50), @PayeeID int;
select @PrefixVD = Prefix from dbo.TblPrefix where Module = 'VD' and SystemID = @SystemID and WarehouseID = @WarehouseID;

SELECT @PaymentAccountNo = AccountNo, @PaymentAccountName = AccountName
FROM TblChartOfAccount WHERE ChartOfAccountID = @PaymentAccountID;

select @VendorID = " + e.PayeeID + ", @PayeeID = " + e.PayeeID + @";

-- If Customer selected
IF (@TransactionType = 'Customer')
BEGIN
    SELECT TOP 1 @NewVendorID = VendorID
    FROM TblVendor 
    WHERE RefID = @CustomerID
          AND RefType = 'Customer'
          AND SystemID = @SystemID
          AND WarehouseID = @WarehouseID
          AND DomainID = @DomainID;

    IF (@NewVendorID IS NULL)
    BEGIN
        exec [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR] @PrefixVD, 'VD', @SystemID, @WarehouseID, @OutVD OUT, 1, 0, @DomainID
        INSERT INTO TblVendor
        (
            VendorName,
            VendorCode,
            RefID,
            RefType,
            SystemID,
            WarehouseID,
            DomainID,
            UserID,
            CreateDate, StatusID, Status, BankerName, AccountNo, AccountName, Gender, Phone, RegisterDate
        )
        SELECT 
            CustomerName,
            @OutVD,
            CustomerID,
            'Customer',
            @SystemID,
            @WarehouseID,
            @DomainID,
            @CreateByUserID,
            @CreateDate, 3, 'Enable', @BankerName, @BankAccountNo, @BankAccountName, @Gender, @PhoneNumber, @RegisterDate
        FROM TblCustomer
        WHERE CustomerID = @CustomerID;

        SET @NewVendorID = SCOPE_IDENTITY();
    END
    select @VendorID = @NewVendorID, @PayeeID = @NewVendorID;
END

-- If Employee selected
IF (@TransactionType = 'Employee')
BEGIN
    SELECT TOP 1 @NewVendorID = VendorID
    FROM TblVendor
    WHERE RefID = @EmployeeID
          AND RefType = 'Employee'
          AND SystemID = @SystemID
          AND WarehouseID = @WarehouseID
          AND DomainID = @DomainID;

    IF (@NewVendorID IS NULL)
    BEGIN
        exec [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR] @PrefixVD, 'VD', @SystemID, @WarehouseID, @OutVD OUT, 1, 0, @DomainID
        INSERT INTO TblVendor
        (
            VendorName,
            VendorCode,
            RefID,
            RefType,
            SystemID,
            WarehouseID,
            DomainID,
            UserID,
            CreateDate, StatusID, Status, BankerName, AccountNo, AccountName, Gender, Phone, RegisterDate
        )
        SELECT 
            FullName,
            @OutVD,
            EmployeeID,
            'Employee',
            @SystemID,
            @WarehouseID,
            @DomainID,
            @CreateByUserID,
            @CreateDate, 3, 'Enable', @BankerName, @BankAccountNo, @BankAccountName, @Gender, @PhoneNumber, @RegisterDate
        FROM HRMS_DB.dbo.Employees
        WHERE EmployeeID = @EmployeeID;

        SET @NewVendorID = SCOPE_IDENTITY();
    END

    select @VendorID = @NewVendorID, @PayeeID = @NewVendorID;
END";
            sql += $@"
IF ({e.ExpenseID} = 0)
";
            sql += @"
BEGIN
    DECLARE @PrefixPV NVARCHAR(50) = '', @PrefixExpense NVARCHAR(50) = '';
    SELECT @PrefixPV = PrefixPV, @PrefixExpense = PrefixExpense FROM dbo.TblClass WHERE ClassID = " + e.ClassNameID + @";
    
    -- EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR] @NoPrefix, 'PrefixPV', @SystemID, @WarehouseID, @OutPrefix OUT, 1, 0, @DomainID, @PaymentDate
  --  EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR] @PrefixPV, 'Expense', @SystemID, @WarehouseID, @OutPrefixPV OUT, 1, 0, @DomainID, @PaymentDate 
    -- Dedicated Expense Prefix Procedure (Completely isolated from other modules)
    EXEC [dbo].[SP_GENERATE_EXPENSE_PREFIX] @NoPrefix, 'PrefixPV', @SystemID, @WarehouseID, @OutPrefix OUT, @DomainID, @PaymentDate;
    EXEC [dbo].[SP_GENERATE_EXPENSE_PREFIX] @PrefixPV, 'Expense', @SystemID, @WarehouseID, @OutPrefixPV OUT, @DomainID, @PaymentDate;
    
    IF(@ExpenseTransactionType <> 'Bill')
    BEGIN
        EXEC [dbo].[SP_GENERATE_EXPENSE_PREFIX] @PrefixExpense, 'ExpenseNo', @SystemID, @WarehouseID, @OutPrefixExpenseNo OUT, @DomainID, @PaymentDate;
    END
    ELSE
    BEGIN
    
        EXEC [dbo].[SP_GENERATE_EXPENSE_PREFIX] 'BL{yy}{MM}-{00000}', 'Bill', @SystemID, @WarehouseID, @OutPrefixExpenseNo OUT, @DomainID, @PaymentDate;
    END
    
    EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR] 'PAY{yy}{MM}{dd}-{00000}', 'PaymentNo', @SystemID, @WarehouseID, @PaymentNo OUT, 1, 0, @DomainID, @PaymentDate
    EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR] 'JV{yy}{MM}_{00000}', 'JournalNo', @SystemID, @WarehouseID, @JournalNo OUTPUT, 1, 0, @DomainID, @PaymentDate
END
ELSE
BEGIN";
            sql += $@"
    SELECT  @OutPrefix = e.[No], @OutPrefixPV = e.PVNo, @OutPrefixExpenseNo = e.ExpenseNo,
            @PaymentNo = e.PaymentNo, @JournalNo = t.JournalNo
    FROM [dbo].[Expenses] e
    INNER JOIN TblAccount_Transaction t ON t.ExpenseID = e.ExpenseID
    WHERE e.ExpenseID = {e.ExpenseID} AND t.Type in ('Expense', 'Bill', 'Vendor Deposit');

    IF (@InputExpenseNo IS NOT NULL AND @InputExpenseNo <> '')
    BEGIN
        SET @OutPrefixExpenseNo = @InputExpenseNo;
    END
    ELSE
    BEGIN
        DECLARE @OldClassNameID INT, @OldPaymentDate DATETIME;
        SELECT @OldClassNameID = TRY_CAST(ClassName AS INT), @OldPaymentDate = PaymentDate FROM dbo.Expenses WHERE ExpenseID = {e.ExpenseID};
        IF (@OldClassNameID <> @ClassNameID OR CONVERT(VARCHAR(7), @OldPaymentDate, 120) <> CONVERT(VARCHAR(7), @PaymentDate, 120))
        BEGIN
            DECLARE @PrefixExpenseEdit NVARCHAR(50) = '';
            SELECT @PrefixExpenseEdit = PrefixExpense FROM dbo.TblClass WHERE ClassID = @ClassNameID;
            IF (@ExpenseTransactionType <> 'Bill')
            BEGIN
                EXEC [dbo].[SP_GENERATE_EXPENSE_PREFIX] @PrefixExpenseEdit, 'ExpenseNo', @SystemID, @WarehouseID, @OutPrefixExpenseNo OUT, @DomainID, @PaymentDate;
            END
            ELSE
            BEGIN
                EXEC [dbo].[SP_GENERATE_EXPENSE_PREFIX] 'BL{{yy}}{{MM}}-{{00000}}', 'Bill', @SystemID, @WarehouseID, @OutPrefixExpenseNo OUT, @DomainID, @PaymentDate;
            END
        END
    END";
            sql += @"
END

INSERT INTO Expenses
(
    PaymentNo, ClassName, PaymentMethodID, PayeeID,
    No,NoPrefix, PaymentAccountID,
    PaymentDate, PaidOnDate,
    PVNo, PVNoPrefix,
    CheckedBy, CheckedByTelegramUserName, CheckedByUserID,
    VerifiedBy, VerifiedByTelegramUserName, VerifiedByUserID,
    ApprovedBy, ApprovedByTelegramUserName, ApprovedByUserID,
    PostingNumber, PostingPrefix,
    Reference, BankInfo,BankerName,BankAccountName,BankAccountNo,ChequeTo,ChequeNo,Total, Status, PayeeType, CreateByUserID, SystemID, WarehouseID, DomainID,
    PostedBy, PostedByUserID, PostedByTelegramUserName, ChequeBankerName, ExchangeRate, FileName, PaymentTeam, TermName, TotalAmount, PaidAmount, BalanceAmount, 
    PaymentStatus, ExpenseNo, Description, GrantTotalAsRiel, Discount, DiscountItem, SubTotal, VAT, ExpenseTransactionType, HeaderTax, VATRemark, Type, VendorDepositID, AppliedAmount
)
VALUES
(
    @PaymentNo, @ClassName, @PaymentMethodID, @VendorID,
    @OutPrefix, @NoPrefix, @PaymentAccountID,
    @PaymentDate, @PaidOnDate,
    @OutPrefixPV, @PVNoPrefix,
    @CheckedBy, @CheckedByTelegramUserName, @CheckedByUserID,
    @VerifiedBy, @VerifiedByTelegramUserName, @VerifiedByUserID,
    @ApprovedBy, @ApprovedByTelegramUserName, @ApprovedByUserID,
    @PostingNumber, @PostingPrefix,
    @Reference, @BankInfo, @BankerName,@BankAccountName,@BankAccountNo,@ChequeTo,@ChequeNo,@Total, @Status, 
    @TransactionType, @CreateByUserID, @SystemID, @WarehouseID, @DomainID,
    @PostedBy, @PostedByUserID, @PostedByTelegramUserName, @ChequeBankerName, @ExchangeRate, @FileName, 
    @PaymentTermsID, @TermName, @TotalAmount, @PaidAmount, @BalanceAmount, @PaymentStatus, @OutPrefixExpenseNo,
    @Description, @GrantTotalAsRiel, @Discount, @DiscountItem, @SubTotal, @VAT, @ExpenseTransactionType, @HeaderTax, @VATRemark, @ExpenseFormType, @VendorDepositID, @AppliedAmount
);
SET @IDENTITY = SCOPE_IDENTITY();
SET @ExpenseID = @IDENTITY;

INSERT INTO dbo.Expense_PaymentTransactions
(
    ExpenseID,
    PaymentNo,
    ExpenseNo,
    No,
    ClassName,
    PaymentMethodID,
    PayeeID,
    PaymentAccountID,
    PaymentTeam,
    PaidOnDate,

    IsChecked,
    CheckedBy,
    CheckedByUserID,
    CheckedByTelegramUserName,

    IsVerified,
    VerifiedBy,
    VerifiedByUserID,
    VerifiedByTelegramUserName,

    IsPosted,
    
    Reference,
    ExchangeRate,
    Description,

    BankerName,
    BankAccountName,
    BankAccountNo,

    ChequeBankerName,
    ChequeTo,
    ChequeNo,

    Status,

    CreatedDate,
    CreateByUserID,

    FileName,

    TotalAmount,
    PaidAmount,
    BalanceAmount,

    PaymentStatus,

    SystemID,
    WarehouseID,
    DomainID,
    PostedByTelegramUserName,
    PostedByUserID,
    Total,
    IsRequestedPayment,
    TotalPaidAmount,
    TotalBalanceAmount,
    PaymentDate, GrantTotalAsRiel, Discount, DiscountItem, SubTotal, VAT, ExpenseTransactionType, HeaderTax, VATRemark, Type, VendorDepositID, AppliedAmount
)
VALUES
(
    @IDENTITY,
    @PaymentNo,
    @OutPrefixExpenseNo,
    @OutPrefix,
    @ClassName,
    @PaymentMethodID,
    @VendorID,
    @PaymentAccountID,
    @PaymentTermsID,
    @PaidOnDate,

    0,
    @CheckedBy,
    @CheckedByUserID,
    @CheckedByTelegramUserName,

    0,
    @VerifiedBy,
    @VerifiedByUserID,
    @VerifiedByTelegramUserName,

    0,

    @Reference,
    @ExchangeRate,
    @Description,

    @BankerName,
    @BankAccountName,
    @BankAccountNo,

    @ChequeBankerName,
    @ChequeTo,
    @ChequeNo,

    @Status,

    @CreateDate,
    @CreateByUserID,
    @FileName,

    @TotalAmount,
    @PaidAmount,
    @BalanceAmount,

    @PaymentStatus,

    @SystemID,
    @WarehouseID,
    @DomainID,
    @PostedByTelegramUserName,
    @PostedByUserID,
    @Total, 0,
    @TotalPaidAmount,
    @TotalBalanceAmount,
    @PaymentDate, @GrantTotalAsRiel, @Discount, @DiscountItem, @SubTotal, @VAT, @ExpenseTransactionType, @HeaderTax, @VATRemark, @ExpenseFormType, @VendorDepositID, @AppliedAmount
);

insert into TblAccount_Transaction 
(ExpensesID, PayeeID, Type, TransactionTypeID, ExpenseNo, Amount, TransactionDate, 
CreateByUserID, SystemID, WarehouseID, DomainID, CreateDate, PVNo, [Description], InvoiceNo, InvoiceDate, AccountID,
VendorID, EmployeeID, CustomerID, TransactionType, ExpenseID, ClassNameID, PaymentNo, JournalNo, ExpenseTransactionType, VAT, Discount, VendorDepositID, AppliedAmount)
values (@IDENTITY, @PayeeID, @Type, @TransactionTypeID, @OutPrefixExpenseNo, @Total, @EntryDate, 
@CreateByUserID, @SystemID, @WarehouseID, @DomainID, @CreateDate, NULL,";
            if (e.ExpenseTransactionType == "Vendor Deposit")
            {
                sql += @"N'Posting Vendor Deposit for " + e.VendorName + "'";
            }
            else if (e.ExpenseTransactionType == "Direct Payment")
            {
                sql += @"N'Direct Payment for " + e.VendorName + "'";
            }
            else if (e.ExpenseTransactionType == "Bill")
            {
                sql += @"N'Posting Vendor Bill for " + e.VendorName + "'";
            }
            else
            {
                sql += @"N'Posting Expense for " + e.VendorName + "'";
            }
            sql += @",@OutPrefixExpenseNo, @PaymentDate, @PaymentAccountID,
@VendorID, @EmployeeID, @CustomerID, @TransactionType, @IDENTITY, @ClassNameID, @PaymentNo, @JournalNo, @ExpenseTransactionType, @HeaderTax, @TDiscount, @VendorDepositID, @AppliedAmount);

SET @TransactionID = SCOPE_IDENTITY();

UPDATE dbo.Expenses SET TransactionID = @TransactionID
WHERE ExpenseID = @IDENTITY;

UPDATE dbo.Expense_PaymentTransactions SET TransactionID = @TransactionID
WHERE ExpenseID = @IDENTITY;

/* =========================================================
   DETERMINE CREDIT ACCOUNT
   ========================================================= */

SET @APAccountID = NULL;
SET @AccountNo = NULL;
SET @AccountName = NULL;


/* =========================================================
   NORMAL EXPENSE
   ========================================================= */

IF @ExpenseTransactionType = 'Bill'
BEGIN

    SELECT TOP 1
        @APAccountID = ChartOfAccountID,
        @AccountNo = AccountNo,
        @AccountName = AccountName
    FROM TblChartOfAccount
    WHERE NormalSide = 'Credit'
      AND ISNULL(ParentAccountID, 0) = 0
      AND
      (
            AccountName = 'Accounts Payable (A/P)'
         OR AccountName = 'Account Payable (A/P)'
         OR AccountName LIKE '%Accounts Payable%'
         OR AccountName LIKE '%Account Payable%'
         OR AccountName LIKE '%Payable%'
      )
      AND SystemID = @SystemID
      AND WarehouseID = @WarehouseID
      AND DomainID = @DomainID
    ORDER BY
        CASE
            WHEN AccountName = 'Accounts Payable (A/P)' THEN 1
            WHEN AccountName = 'Account Payable (A/P)' THEN 2
            WHEN AccountName LIKE '%Accounts Payable%' THEN 3
            WHEN AccountName LIKE '%Account Payable%' THEN 4
            WHEN AccountName LIKE '%Payable%' THEN 5
            ELSE 6
        END;

    IF @APAccountID IS NULL
    BEGIN
        THROW 50002,
            'Accounts Payable (A/P) account was not found.',
            1;
    END;

END;


/* =========================================================
   DIRECT PAYMENT
   ========================================================= */

ELSE IF @ExpenseTransactionType = 'Direct Payment'
BEGIN

    /*
        Direct Payment DOES NOT use A/P.

        Example:

            @PaymentAccountID = 410
            110000-Petty Cash

        Journal:

            Dr Expense       500
                Cr Petty Cash    500
    */

    IF @PaymentAccountID IS NULL
       OR @PaymentAccountID = 0
    BEGIN
        THROW 50009,
            'Direct Payment requires a PaymentAccountID.',
            1;
    END;


    SELECT TOP 1
        @AccountNo = AccountNo,
        @AccountName = AccountName
    FROM TblChartOfAccount
    WHERE ChartOfAccountID = @PaymentAccountID
      AND SystemID = @SystemID
      AND WarehouseID = @WarehouseID
      AND DomainID = @DomainID;


    IF @AccountNo IS NULL
    BEGIN
        THROW 50010,
            'Direct Payment payment account was not found.',
            1;
    END;

END;

/* =====================================================
   CREATE CREDIT JOURNAL
   ===================================================== */

IF (@Credit >= 0)
BEGIN

    /* =================================================
       NORMAL EXPENSE
       ================================================= */

    IF @ExpenseTransactionType = 'Expense'
    BEGIN

        INSERT INTO TblAccount_JournalEntries
        (
            TransactionID,
            AccountID,
            Debit,
            Credit,
            EntryDate,
            TransactionDate,
            SystemID,
            WarehouseID,
            DomainID,
            CreateByUserID,
            CreateDate,
            Description,
            VATAmount,
            DiscountAmount,
            ExpenseID,
            ExpenseAmount,
            ItemTAX,
            ClassNameID,
            PaymentNo,
            ExpenseNo,
            Type,
            JournalNo,
            InvoiceNo,
            ExpenseTransactionType,
            Account
        )
        VALUES
        (
            @TransactionID,
            @APAccountID,
            0,
            @Credit,
            @EntryDate,
            @EntryDate,
            @SystemID,
            @WarehouseID,
            @DomainID,
            @CreateByUserID,
            GETDATE(),
            case when @Description <> '' then @Description else
                'Expense on Credit-' +
                ISNULL(@OutPrefixExpenseNo, '')
            end,
            0,
            0,
            @ExpenseID,
            @ItemGrandTotal,
            0,
            @ClassNameID,
            @PaymentNo,
            @OutPrefixExpenseNo,

            'Expense',

            @JournalNo,
            @OutPrefixExpenseNo,
            'Expense',
            'Expense-' + @AccountNo + '-' + @AccountName
        );

    END


    /* =================================================
       DIRECT PAYMENT
       ================================================= */

    ELSE IF @ExpenseTransactionType = 'Direct Payment'
    BEGIN

        /*
            IMPORTANT:

            Do NOT use @APAccountID here.

            Use @PaymentAccountID.

            Example:

                Expense = $500
                PaymentAccountID = 410
                410 = Petty Cash

            Result:

                Dr Expense       500
                    Cr Petty Cash    500
        */

        INSERT INTO TblAccount_JournalEntries
        (
            TransactionID,
            AccountID,
            Debit,
            Credit,
            EntryDate,
            TransactionDate,
            SystemID,
            WarehouseID,
            DomainID,
            CreateByUserID,
            CreateDate,
            Description,
            VATAmount,
            DiscountAmount,
            ExpenseID,
            ExpenseAmount,
            ItemTAX,
            ClassNameID,
            PaymentNo,
            ExpenseNo,
            Type,
            JournalNo,
            InvoiceNo,
            ExpenseTransactionType,
            Account
        )
        VALUES
        (
            @TransactionID,
            @PaymentAccountID,
            0,
            @Credit,
            @EntryDate,
            @EntryDate,
            @SystemID,
            @WarehouseID,
            @DomainID,
            @CreateByUserID,
            GETDATE(),
            case when @Description <> '' then @Description else
                'Direct Payment - ' +
                ISNULL(@OutPrefixExpenseNo, '')
            end,

            0,
            0,
            @ExpenseID,
            @ItemGrandTotal,
            0,
            @ClassNameID,
            @PaymentNo,
            @OutPrefixExpenseNo,

            'Expense',

            @JournalNo,
            @OutPrefixExpenseNo,
            'Direct Payment',
            'Direct Payment-' +
            @AccountNo + '-' + @AccountName
        );

    END

END;              

SELECT 
    @IDENTITY AS ExpenseID,
    @TransactionID AS TransactionID,
    @PaymentAccountID AS PaymentAccountID,
    @APAccountID AS APAccountID,
    @PaymentNo AS PaymentNo,
    @OutPrefixExpenseNo AS ExpenseNo,
    @JournalNo AS JournalNo,
    @AccountNo AS APAccountNo,
    @AccountName AS APAccountName,
    @PaymentAccountName AS PaymentAccountName,
    @PaymentAccountNo AS PaymentAccountNo,
    @PayeeID AS PayeeID;
";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                // =========================================================
                // 1. CALCULATE SUBTOTAL FROM EXPENSE DETAILS
                // =========================================================

                decimal subtotal = e.Details?
                    .Sum(x => x.ItemGrandTotal) ?? 0m;

                decimal VAT = e.Details?
                    .Sum(x => x.ItemVAT) ?? 0m;

                decimal DiscountItem = e.Details?
                    .Sum(x => x.DisUSD) ?? 0m;

                if (subtotal < 0)
                    subtotal = 0m;


                // =========================================================
                // 2. WHOLE EXPENSE DISCOUNT
                // =========================================================

                decimal discount = e.Discount + DiscountItem;

                // Prevent negative discount
                if (discount < 0)
                    discount = 0m;

                // Discount cannot be greater than subtotal
                if (discount > subtotal)
                    discount = subtotal;

                // Keep normalized discount
                //e.Discount = discount;


                // =========================================================
                // 3. FINAL TOTAL AFTER WHOLE EXPENSE DISCOUNT
                // =========================================================

                //decimal finalTotal = subtotal - discount;

                //if (finalTotal < 0)
                //    finalTotal = 0m;

                //e.TotalAmount = finalTotal;


                // =========================================================
                // 4. KEEP ORIGINAL SUBTOTAL IN e.Total
                // =========================================================
                //
                // Example:
                //
                // e.Total       = 560
                // e.Discount    = 60
                // e.TotalAmount = 500
                //
                // This allows the UI/report to show:
                //
                // Sub Total     $560
                // Discount       $60
                // Total          $500
                //
                // =========================================================

                //e.Total = subtotal;


                // =========================================================
                // 5. ACTUAL AMOUNT PAID
                // =========================================================

                decimal paidAmount = e.PaidAmount;

                // Prevent negative payment
                if (paidAmount < 0)
                    paidAmount = 0m;

                // Payment cannot be greater than FINAL TOTAL
                if (paidAmount > e.TotalAmount)
                    paidAmount = e.TotalAmount;

                e.PaidAmount = paidAmount;


                // =========================================================
                // 6. PAID PORTION
                // =========================================================

                e.PaidPortion = e.PaidAmount;


                // =========================================================
                // 7. BALANCE
                // =========================================================

                decimal balanceAmount =
                    e.TotalAmount - e.PaidAmount;

                if (balanceAmount < 0)
                    balanceAmount = 0m;

                e.BalanceAmount = balanceAmount;


                // =========================================================
                // 8. PAYMENT STATUS
                // =========================================================

                if (e.PaidAmount <= 0)
                {
                    e.PaymentStatus = "Unpaid";
                }
                else if (e.PaidAmount < e.TotalAmount)
                {
                    e.PaymentStatus = "Partial";
                }
                else
                {
                    e.PaymentStatus = "Paid";
                }


                // =========================================================
                // 9. SQL PARAMETERS
                // =========================================================

                cmd.Parameters.Add("@Credit", SqlDbType.Decimal).Value = e.TotalAmount;
                cmd.Parameters.Add("@ItemGrandTotal", SqlDbType.Decimal).Value = e.TotalAmount;
                cmd.Parameters.Add("@EntryDate", SqlDbType.DateTime).Value = e.PaymentDate;


                cmd.Parameters.Add("@VendorDepositID", SqlDbType.Int).Value = e.VendorDepositID;
                cmd.Parameters.Add("@AppliedAmount", SqlDbType.Decimal).Value = e.AmountToApply;
                cmd.Parameters.Add("@ClassName", SqlDbType.Int).Value = e.ClassName;

                cmd.Parameters.Add("@ClassNameID", SqlDbType.Int).Value = e.ClassName;
                cmd.Parameters.Add("@PaymentMethodID", SqlDbType.Int).Value = e.PaymentMethodID;
                //cmd.Parameters.Add("@SetVendorID", SqlDbType.Int).Value = e.PayeeID;

                cmd.Parameters.Add("@ExpenseTransactionType", SqlDbType.NVarChar).Value = e.ExpenseTransactionType ?? "";
                cmd.Parameters.Add("@VATRemark", SqlDbType.NVarChar).Value = e.VATRemark ?? "";
                cmd.Parameters.Add("@NoPrefix", SqlDbType.NVarChar).Value = e.NoPrefix ?? "";
                cmd.Parameters.Add("@PVNoPrefix", SqlDbType.NVarChar).Value = e.PVNoPrefix ?? "";
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = e.Description ?? "";
                cmd.Parameters.Add("@InputExpenseNo", SqlDbType.NVarChar, 50).Value = (object)e.ExpenseNo ?? DBNull.Value;

                cmd.Parameters.Add("@PaymentAccountID", SqlDbType.Int).Value = e.PaymentAccountID;
                cmd.Parameters.Add("@PaymentDate", SqlDbType.DateTime).Value = e.PaymentDate;


                cmd.Parameters.Add("@PaidOnDate", SqlDbType.DateTime).Value = e.PaidOnDate.HasValue ? (object)e.PaidOnDate.Value : DBNull.Value;

                cmd.Parameters.Add("@CheckedBy", SqlDbType.NVarChar).Value = e.CheckedBy ?? "";
                cmd.Parameters.Add("@CheckedByTelegramUserName", SqlDbType.NVarChar).Value = e.CheckedByTelegramUserName ?? "";
                cmd.Parameters.Add("@CheckedByUserID", SqlDbType.Int).Value = e.CheckedByUserID;

                cmd.Parameters.Add("@VerifiedBy", SqlDbType.NVarChar).Value = e.VerifiedBy ?? "";
                cmd.Parameters.Add("@VerifiedByTelegramUserName", SqlDbType.NVarChar).Value = e.VerifiedByTelegramUserName ?? "";
                cmd.Parameters.Add("@VerifiedByUserID", SqlDbType.Int).Value = e.VerifiedByUserID;

                cmd.Parameters.Add("@ApprovedBy", SqlDbType.NVarChar).Value = e.ApprovedBy ?? "";
                cmd.Parameters.Add("@ApprovedByTelegramUserName", SqlDbType.NVarChar).Value = e.ApprovedByTelegramUserName ?? "";
                cmd.Parameters.Add("@ApprovedByUserID", SqlDbType.Int).Value = e.ApprovedByUserID;

                cmd.Parameters.Add("@ExpenseFormType", SqlDbType.NVarChar).Value = e.Type ?? "";
                cmd.Parameters.Add("@PostedBy", SqlDbType.NVarChar).Value = e.PostedBy ?? "";
                cmd.Parameters.Add("@PostedByTelegramUserName", SqlDbType.NVarChar).Value = e.PostedByTelegramUserName ?? "";
                cmd.Parameters.Add("@PostedByUserID", SqlDbType.Int).Value = e.PostedByUserID;

                cmd.Parameters.Add("@PostingNumber", SqlDbType.NVarChar).Value = e.PostingNumber ?? "";
                cmd.Parameters.Add("@PostingPrefix", SqlDbType.NVarChar).Value = e.PostingPrefix ?? "";

                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar).Value = e.Reference ?? "";
                cmd.Parameters.Add("@BankInfo", SqlDbType.NVarChar).Value = e.BankInfo ?? "";
                cmd.Parameters.Add("@ChequeBankerName", SqlDbType.NVarChar).Value = e.ChequeBankerName ?? "";
                cmd.Parameters.Add("@BankerName", SqlDbType.NVarChar).Value = e.BankerName ?? "";
                cmd.Parameters.Add("@BankAccountName", SqlDbType.NVarChar).Value = e.BankAccountName ?? "";
                cmd.Parameters.Add("@BankAccountNo", SqlDbType.NVarChar).Value = e.BankAccountNo ?? "";
                cmd.Parameters.Add("@ChequeTo", SqlDbType.NVarChar).Value = e.ChequeTo ?? "";
                cmd.Parameters.Add("@ChequeNo", SqlDbType.NVarChar).Value = e.ChequeNo ?? "";

                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value = e.SystemID;
                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value = e.WarehouseID;
                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar).Value = e.DomainID;
                cmd.Parameters.Add("@FileName", SqlDbType.NVarChar).Value = e.FileName;                
                cmd.Parameters.Add("@SubTotal", SqlDbType.Decimal).Value = subtotal;
                var a = subtotal - e.Discount;
                decimal totalVAT = Math.Round((e.HeaderTax * a) / 100m, 2, MidpointRounding.AwayFromZero);
                cmd.Parameters.Add("@VAT", SqlDbType.Decimal).Value = totalVAT;
                cmd.Parameters.Add("@HeaderTax", SqlDbType.Decimal).Value = e.HeaderTax;
                cmd.Parameters.Add("@Discount", SqlDbType.Decimal).Value = e.Discount;
                cmd.Parameters.Add("@TDiscount", SqlDbType.Decimal).Value = discount;
                cmd.Parameters.Add("@DiscountItem", SqlDbType.Decimal).Value = DiscountItem;
                cmd.Parameters.Add("@GrantTotalAsRiel", SqlDbType.Decimal).Value = e.GrantTotalAsRiel;
                cmd.Parameters.Add("@Total", SqlDbType.Decimal).Value = e.TotalAmount;
                cmd.Parameters.Add("@TotalPaidAmount", SqlDbType.Decimal).Value = e.PaidAmount;
                cmd.Parameters.Add("@TotalBalanceAmount", SqlDbType.Decimal).Value = e.BalanceAmount;
                cmd.Parameters.Add("@ExchangeRate", SqlDbType.Decimal).Value = e.ExchangeRate;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = e.Status;

                cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value = e.IsFromBills ? "Bill" : "Expense"; //e.ExpenseTransactionType == "Vendor Deposit" ? "Vendor Deposit" : "Expense";
                cmd.Parameters.Add("@TransactionTypeID", SqlDbType.Int).Value = e.PaymentMethodID;
                cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int).Value = e.CreateByUserID;
                cmd.Parameters.Add("@CreateDate", SqlDbType.DateTime).Value = e.CreateDate;

                cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value = e.VendorID;
                cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = e.EmployeeID;
                cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = e.CustomerID;
                cmd.Parameters.Add("@TransactionType", SqlDbType.NVarChar).Value = e.TransactionType;
                cmd.Parameters.Add("@Gender", SqlDbType.NVarChar).Value = e.Gender;
                cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = e.PhoneNumber;
                cmd.Parameters.Add("@RegisterDate", SqlDbType.NVarChar).Value = e.CreateDate;
                cmd.Parameters.Add("@CheckedDate", SqlDbType.DateTime).Value = e.CreateDate;

                cmd.Parameters.Add("@PaymentTermsID", SqlDbType.DateTime).Value = e.PaymentTermsID.HasValue ? (object)e.PaymentTermsID.Value : DBNull.Value;
                cmd.Parameters.Add("@TermName", SqlDbType.NVarChar).Value = e.TermName;
                cmd.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = e.TotalAmount;
                cmd.Parameters.Add("@PaidAmount", SqlDbType.Decimal).Value = e.PaidAmount;
                cmd.Parameters.Add("@BalanceAmount", SqlDbType.Decimal).Value = e.BalanceAmount;
                cmd.Parameters.Add("@PaymentStatus", SqlDbType.NVarChar).Value = e.PaymentStatus;

                //e.SystemID = int.Parse(html.SystemID);
                //e.WarehouseID = int.Parse(html.WarehouseID);
                //e.DomainID = html.DomainID;

                //object result = cmd.ExecuteScalar();
                //return Convert.ToInt32(result);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new ExpensePostingResult
                        {
                            ExpenseID = Convert.ToInt32(reader["ExpenseID"]),
                            PayeeID = Convert.ToInt32(reader["PayeeID"]),
                            TransactionID = Convert.ToInt32(reader["TransactionID"]),
                            PaymentAccountID = reader["PaymentAccountID"] == DBNull.Value
                                            ? 0 : Convert.ToInt32(reader["PaymentAccountID"]),
                            APAccountID = reader["APAccountID"] == DBNull.Value
                                            ? 0
                                            : Convert.ToInt32(reader["APAccountID"]),
                            PaymentNo = reader["PaymentNo"].ToString(),
                            ExpenseNo = reader["ExpenseNo"].ToString(),
                            JournalNo = reader["JournalNo"].ToString(),
                            APAccountNo = reader["APAccountNo"].ToString(),
                            APAccountName = reader["APAccountName"].ToString(),
                            PaymentAccountNo = reader["PaymentAccountNo"].ToString(),
                            PaymentAccountName = reader["PaymentAccountName"].ToString(),
                            ExpenseAmount = e.TotalAmount,
                            //APAccountID = reader["APAccountID"] == DBNull.Value
                            //                ? 0
                            //                : Convert.ToInt32(reader["APAccountID"])
                        };
                    }
                }
                throw new Exception("Failed to insert expense header.");
            }
        }

        public class ExpensePostingResult
        {
            public int PayeeID { get; set; }
            public int ExpenseID { get; set; }
            public int TransactionID { get; set; }
            public int APAccountID { get; set; }
            public string APAccountNo { get; set; }
            public string APAccountName { get; set; }
            public string PaymentAccountName { get; set; }
            public string PaymentAccountNo { get; set; }
            public int PaymentAccountID { get; set; }
            public int ClassNameID { get; set; }
            public string PaymentNo { get; set; }
            public string JournalNo { get; set; }
            public string ExpenseNo { get; set; }
            public decimal ExpenseAmount { get; set; }
            public decimal PaidAmount { get; set; } = 0;
            public decimal RemainingAP { get; set; }
            public string PaymentJournalNo { get; set; }
        }

        // ================= DETAILS =================     
        private void InsertExpenseDetail(
    Expense e,
    ExpensePostingResult expensePostingResult,
    ExpenseDetail d,
    int TransactionID,
    SqlConnection conn,
    SqlTransaction tran)
        {
            //HTML html = new HTML();

            //string[] vendorDepositTypes =
            //{
            //    "VendorDeposit",
            //    "Vendor Deposit",
            //    "Supplier Deposit",
            //    "Vendor Advance",
            //    "Supplier Advance"
            //};
            string[] vendorDepositTypes =
            {
                
            };

            //string expenseTransactionType =
            //    !string.IsNullOrEmpty(d.AccountName) &&
            //    vendorDepositTypes.Any(x =>
            //        d.AccountName.IndexOf(
            //            x,
            //            StringComparison.OrdinalIgnoreCase) >= 0)
            //        ? "Vendor Deposit"
            //        : "Expense";

            //if (expenseTransactionType != "Vendor Deposit")
            //{
            //    if (e.BalanceAmount == 0)
            //    {
            //        expenseTransactionType = "Direct Payment";
            //    }
            //}

            //e.ExpenseTransactionType = expenseTransactionType;
            d.ExpenseTransactionType = e.ExpenseTransactionType;//expenseTransactionType;

            // =========================================================
            // 1. EXPENSE ACCOUNT NAME
            // =========================================================

            string expenseCategoryAccName =
                d.AccountName + "-" + d.ItemName;


            // =========================================================
            // 2. ORIGINAL TOTAL
            // =========================================================

            decimal originalExpenseTotal =
                e.Details?.Sum(x => Math.Max(0m, x.ItemGrandTotal)) ?? 0m;

            originalExpenseTotal = Math.Round(
                originalExpenseTotal,
                2,
                MidpointRounding.AwayFromZero
            );

            if (originalExpenseTotal < 0m)
                originalExpenseTotal = 0m;


            // =========================================================
            // 3. WHOLE EXPENSE DISCOUNT
            // =========================================================

            decimal wholeDiscount = e.Discount;

            if (wholeDiscount < 0m)
                wholeDiscount = 0m;

            if (wholeDiscount > originalExpenseTotal)
                wholeDiscount = originalExpenseTotal;

            wholeDiscount = Math.Round(
                wholeDiscount,
                2,
                MidpointRounding.AwayFromZero
            );


            // =========================================================
            // 4. NET EXPENSE
            // =========================================================

            decimal netExpenseTotal =
                Math.Round(
                    originalExpenseTotal - wholeDiscount,
                    2,
                    MidpointRounding.AwayFromZero
                );

            if (netExpenseTotal < 0m)
                netExpenseTotal = 0m;


            // =========================================================
            // 5. ACTUAL PAID AMOUNT
            // =========================================================

            decimal paidAmount = e.PaidAmount;

            if (paidAmount < 0m)
                paidAmount = 0m;

            if (paidAmount > netExpenseTotal)
                paidAmount = netExpenseTotal;

            paidAmount = Math.Round(
                paidAmount,
                2,
                MidpointRounding.AwayFromZero
            );


            // =========================================================
            // 6. PAYMENT RATIO
            //
            // IMPORTANT:
            //
            // Payment ratio is based on NET amount after discount.
            //
            // Example:
            //
            // Original = 1,520
            // Discount =   520
            // Net      = 1,000
            // Paid     =   500
            //
            // Ratio = 500 / 1,000 = 50%
            // =========================================================

            decimal paymentRatio =
                netExpenseTotal == 0m
                    ? 0m
                    : paidAmount / netExpenseTotal;


            // =========================================================
            // 7. CURRENT ITEM TOTAL
            // =========================================================

            decimal itemTotal =
                Math.Max(0m, d.ItemGrandTotal);

            itemTotal = Math.Round(
                itemTotal,
                2,
                MidpointRounding.AwayFromZero
            );


            // =========================================================
            // 8. DETERMINE CURRENT ITEM INDEX
            // =========================================================
            //
            // We need this to make the LAST item receive the
            // remaining rounding amount.
            //
            // This assumes ExpenseDetail objects are reference-equal
            // to the objects inside e.Details.
            //
            // If your list is not reference-equal, use the overload
            // shown below that passes itemIndex/isLastItem.
            // =========================================================

            int itemIndex = -1;

            if (e.Details != null)
            {
                for (int i = 0; i < e.Details.Count; i++)
                {
                    if (ReferenceEquals(e.Details[i], d))
                    {
                        itemIndex = i;
                        break;
                    }
                }
            }

            bool isLastItem =
                e.Details != null &&
                itemIndex >= 0 &&
                itemIndex == e.Details.Count - 1;


            // =========================================================
            // 9. ITEM DISCOUNT
            // =========================================================

            decimal itemDiscount = 0m;

            if (originalExpenseTotal > 0m &&
                wholeDiscount > 0m)
            {
                if (isLastItem)
                {
                    // -------------------------------------------------
                    // LAST ITEM:
                    // Give it the remaining discount so that:
                    //
                    // Sum(ItemDiscount) = WholeDiscount
                    // -------------------------------------------------

                    decimal previousAllocatedDiscount = 0m;

                    for (int i = 0; i < itemIndex; i++)
                    {
                        decimal previousItem =
                            Math.Max(
                                0m,
                                e.Details[i].ItemGrandTotal
                            );

                        decimal previousDiscount =
                            Math.Round(
                                wholeDiscount *
                                (previousItem / originalExpenseTotal),
                                2,
                                MidpointRounding.AwayFromZero
                            );

                        if (previousDiscount > previousItem)
                            previousDiscount = previousItem;

                        previousAllocatedDiscount +=
                            previousDiscount;
                    }

                    itemDiscount =
                        Math.Round(
                            wholeDiscount -
                            previousAllocatedDiscount,
                            2,
                            MidpointRounding.AwayFromZero
                        );
                }
                else
                {
                    itemDiscount =
                        Math.Round(
                            wholeDiscount *
                            (itemTotal / originalExpenseTotal),
                            2,
                            MidpointRounding.AwayFromZero
                        );
                }
            }

            if (itemDiscount < 0m)
                itemDiscount = 0m;

            if (itemDiscount > itemTotal)
                itemDiscount = itemTotal;


            // =========================================================
            // 10. NET ITEM
            // =========================================================

            decimal netItemTotal =
                Math.Round(
                    itemTotal - itemDiscount,
                    2,
                    MidpointRounding.AwayFromZero
                );

            if (netItemTotal < 0m)
                netItemTotal = 0m;


            // =========================================================
            // 11. PAID PORTION
            // =========================================================
            //
            // IMPORTANT:
            //
            // We allocate the ACTUAL $500 payment against the
            // NET $1,000 expense.
            //
            // Example:
            //
            // Item 1:
            //   1,000 - 333.33 discount = 666.67
            //
            // Item 2:
            //     520 - 186.67 discount = 333.33
            //
            // Net total = 1,000
            //
            // Paid = 500
            //
            // Item 1 paid = 333.34
            // Item 2 paid = 166.66
            //
            // Total paid = 500.00
            // =========================================================

            decimal paidPortion = 0m;

            if (netExpenseTotal > 0m)
            {
                if (isLastItem)
                {
                    // -------------------------------------------------
                    // LAST ITEM:
                    // Calculate previous paid amounts and give the
                    // remaining payment to the last item.
                    // -------------------------------------------------

                    decimal previousPaid = 0m;

                    for (int i = 0; i < itemIndex; i++)
                    {
                        decimal previousItem =
                            Math.Max(
                                0m,
                                e.Details[i].ItemGrandTotal
                            );

                        decimal previousDiscount = 0m;

                        if (originalExpenseTotal > 0m &&
                            wholeDiscount > 0m)
                        {
                            previousDiscount =
                                Math.Round(
                                    wholeDiscount *
                                    (previousItem / originalExpenseTotal),
                                    2,
                                    MidpointRounding.AwayFromZero
                                );

                            if (previousDiscount > previousItem)
                                previousDiscount = previousItem;
                        }

                        decimal previousNet =
                            Math.Round(
                                previousItem - previousDiscount,
                                2,
                                MidpointRounding.AwayFromZero
                            );

                        if (previousNet < 0m)
                            previousNet = 0m;


                        previousPaid =
                             Math.Round(
                                 previousNet * paymentRatio,
                                 2,
                                 MidpointRounding.AwayFromZero
                             );

                        if (previousPaid < 0m)
                            previousPaid = 0m;

                        if (previousPaid > previousNet)
                            previousPaid = previousNet;

                        previousPaid += previousPaid;
                    }

                    // -------------------------------------------------
                    // IMPORTANT:
                    //
                    // The above loop should calculate previous item
                    // payments separately.
                    // -------------------------------------------------

                    previousPaid = 0m;

                    for (int i = 0; i < itemIndex; i++)
                    {
                        decimal previousItem =
                            Math.Max(
                                0m,
                                e.Details[i].ItemGrandTotal
                            );

                        decimal previousDiscount = 0m;

                        if (originalExpenseTotal > 0m &&
                            wholeDiscount > 0m)
                        {
                            previousDiscount =
                                Math.Round(
                                    wholeDiscount *
                                    (previousItem / originalExpenseTotal),
                                    2,
                                    MidpointRounding.AwayFromZero
                                );

                            if (previousDiscount > previousItem)
                                previousDiscount = previousItem;
                        }

                        decimal previousNet =
                            Math.Round(
                                previousItem - previousDiscount,
                                2,
                                MidpointRounding.AwayFromZero
                            );

                        if (previousNet < 0m)
                            previousNet = 0m;

                        decimal previousPaidPart =
                            Math.Round(
                                previousNet * paymentRatio,
                                2,
                                MidpointRounding.AwayFromZero
                            );

                        if (previousPaidPart < 0m)
                            previousPaidPart = 0m;

                        if (previousPaidPart > previousNet)
                            previousPaidPart = previousNet;

                        previousPaid += previousPaidPart;
                    }

                    paidPortion =
                        Math.Round(
                            paidAmount - previousPaid,
                            2,
                            MidpointRounding.AwayFromZero
                        );
                }
                else
                {
                    paidPortion =
                        Math.Round(
                            netItemTotal * paymentRatio,
                            2,
                            MidpointRounding.AwayFromZero
                        );
                }
            }

            if (paidPortion < 0m)
                paidPortion = 0m;

            if (paidPortion > netItemTotal)
                paidPortion = netItemTotal;


            // =========================================================
            // 12. AP PORTION
            // =========================================================

            decimal payablePortion =
                Math.Round(
                    netItemTotal - paidPortion,
                    2,
                    MidpointRounding.AwayFromZero
                );

            if (payablePortion < 0m)
                payablePortion = 0m;


            // =========================================================
            // 13. SQL
            // =========================================================

            string sql = @"

DECLARE
    @ExpenseDetailID INT,
    @VATAccountID INT,
    @APAccountID INT,
    @ExpenseAccountNo NVARCHAR(50),
    @ExpenseCategoryName NVARCHAR(MAX),
    @TransactionType NVARCHAR(100),

    @GrossAmount DECIMAL(18,2),
    @DiscountAmount DECIMAL(18,2),
    @VATAmountLocal DECIMAL(18,2),
    @GrandTotal DECIMAL(18,2);


-- =========================================================
-- 1. TRANSACTION TYPE
-- =========================================================

SET @TransactionType =
    LTRIM(TRIM(ISNULL(@ExpenseTransactionType, 'Expense')));

IF @TransactionType IN
(
    'VendorDeposit',
    'Vendor Deposit',
    'Supplier Deposit',
    'Vendor Advance',
    'Supplier Advance'
)
BEGIN
    SET @TransactionType = 'Vendor Deposit';
END;


-- =========================================================
-- 2. INSERT EXPENSE DETAIL
-- =========================================================

INSERT INTO ExpenseDetails
(
    ExpenseID,
    ItemTypeID,
    ExpenseCategory,
    ItemID,
    ItemName,
    ItemName_Description,
    UM,
    QTY,
    UnitPrice,
    DisUSD,
    Total,
    ItemTAX,
    ItemGrandTotal,
    RefNo,
    SourceType,
    ExpenseCategoryType
)
VALUES
(
    @ExpenseID,
    @ItemTypeID,
    @ExpenseCategory,
    @ItemID,
    @ItemName,
    @ItemName_Description,
    @UM,
    @QTY,
    @UnitPrice,
    ISNULL(@DisUSD, 0),
    ISNULL(@Total, 0),
    ISNULL(@ItemTAX, 0),
    ISNULL(@ItemGrandTotal, 0),
    @RefNo,
    @SourceType,
    @ExpenseCategoryType
);

SET @ExpenseDetailID = CONVERT(INT, SCOPE_IDENTITY());


-- =========================================================
-- 3. AMOUNTS
-- =========================================================
--
-- IMPORTANT:
--
-- @Total             = amount BEFORE discount
-- @ItemDiscountAmount = purchase discount
-- @VATAmount          = VAT amount
-- @ItemGrandTotal     = final payable amount
--
-- Example:
--
-- Total       = 520
-- Discount    = 20
-- VAT         = 50
-- GrandTotal  = 550
--
-- Journal:
--
-- DR Expense              520
-- DR VAT Input             50
--     CR Purchase Discount      20
--     CR Accounts Payable      550
--
-- =========================================================

SET @GrossAmount =
    ROUND(ISNULL(@Total, 0), 2);

SET @DiscountAmount =
    ROUND(
        CASE
            WHEN ISNULL(@ItemDiscountAmount, 0) < 0
                THEN 0
            WHEN ISNULL(@ItemDiscountAmount, 0) > @GrossAmount
                THEN @GrossAmount
            ELSE ISNULL(@ItemDiscountAmount, 0)
        END,
        2
    );

SET @VATAmountLocal =
    ROUND(
        CASE
            WHEN ISNULL(@VATAmount, 0) < 0
                THEN 0
            ELSE ISNULL(@VATAmount, 0)
        END,
        2
    );

SET @GrandTotal =
    ROUND(
        ISNULL(@ItemGrandTotal, 0),
        2
    );


-- =========================================================
-- 4. GET SELECTED GL ACCOUNT
-- =========================================================

SELECT TOP 1
    @ExpenseAccountNo = AccountNo,
    @ExpenseCategoryName = AccountName
FROM TblChartOfAccount
WHERE ChartOfAccountID = @ExpenseCategory;


IF @ExpenseCategoryName IS NULL
BEGIN
    THROW 50001,
        'Selected GL Account was not found.',
        1;
END;


-- =========================================================
-- 5. FIND ACCOUNTS PAYABLE
-- =========================================================

SELECT TOP 1
    @APAccountID = ChartOfAccountID
FROM TblChartOfAccount
WHERE NormalSide = 'Credit'
  AND ISNULL(ParentAccountID, 0) = 0
  AND
  (
        AccountName = 'Accounts Payable (A/P)'
     OR AccountName = 'Account Payable (A/P)'
     OR AccountName LIKE '%Accounts Payable%'
     OR AccountName LIKE '%Account Payable%'
     OR AccountName LIKE '%Payable%'
  )
  AND SystemID = @SystemID
  AND WarehouseID = @WarehouseID
  AND DomainID = @DomainID
ORDER BY
    CASE
        WHEN AccountName = 'Accounts Payable (A/P)' THEN 1
        WHEN AccountName = 'Account Payable (A/P)' THEN 2
        WHEN AccountName LIKE '%Accounts Payable%' THEN 3
        WHEN AccountName LIKE '%Account Payable%' THEN 4
        WHEN AccountName LIKE '%Payable%' THEN 5
        ELSE 6
    END;


IF @APAccountID IS NULL
BEGIN
    THROW 50002,
        'Accounts Payable (A/P) account was not found.',
        1;
END;


-- =========================================================
-- 6. VENDOR DEPOSIT
-- =========================================================

IF @ExpenseTransactionType = 'Vendor Deposit'
BEGIN

    -- -----------------------------------------------------
    -- Vendor Deposit must use a Vendor Deposit / Advance
    -- account as the debit account
    -- -----------------------------------------------------

    IF
    (
       @ExpenseCategoryName NOT LIKE '%Vendor Deposit%'
       AND @ExpenseCategoryName NOT LIKE '%Supplier Deposit%'
       AND @ExpenseCategoryName NOT LIKE '%Vendor Advance%'
       AND @ExpenseCategoryName NOT LIKE '%Supplier Advance%'
    )
    BEGIN
        THROW 50003,
            'Vendor Deposit transaction requires a Vendor Deposit / Supplier Deposit / Vendor Advance account.',
            1;
    END;


    -- =====================================================
    -- DR VENDOR DEPOSIT
    -- =====================================================

    --INSERT INTO TblAccount_JournalEntries
--    (
--        ExpenseDetailID,
--        TransactionID,
--        AccountID,
--        Debit,
--        Credit,
--        EntryDate,
--        TransactionDate,
--        SystemID,
--        WarehouseID,
--        DomainID,
--        CreateByUserID,
--        CreateDate,
--        Description,
--        VATAmount,
--        DiscountAmount,
--        ExpenseID,
--        ExpenseAmount,
--        ItemTAX,
--        ClassNameID,
--        PaymentNo,
--        ExpenseNo,
--        Type,
--        JournalNo,
--        InvoiceNo,
--        ExpenseTransactionType
--    )
--    VALUES
--    (
--        @ExpenseDetailID,
--        @TransactionID,
--        @ExpenseCategory,
--        @GrandTotal,
--        0,
--        @EntryDate,
--        @EntryDate,
--        @SystemID,
--        @WarehouseID,
--        @DomainID,
--        @CreateByUserID,
--        GETDATE(),

--        'Vendor Deposit - ' +
--        ISNULL(@ItemName, ''),

--        0,
--        0,
--        @ExpenseID,
--        @GrandTotal,
--        0,
--        @ClassNameID,
--        @PaymentNo,
--        @ExpenseNo,
--        'Vendor Deposit',
--        @JournalNo,
--        @ExpenseNo,
--        'Vendor Deposit'
--    );
    
END


-- =========================================================
-- 7. NORMAL EXPENSE
-- =========================================================

ELSE
BEGIN

    -- =====================================================
    -- 7.1 VAT INPUT ACCOUNT
    -- =====================================================

    IF @VATAmountLocal > 0
    BEGIN

        SELECT TOP 1
            @VATAccountID = ChartOfAccountID
        FROM TblChartOfAccount
        WHERE AccountName = 'VAT Input'
          AND DomainID = @DomainID
          AND SystemID = @SystemID
          AND WarehouseID = @WarehouseID;


        IF @VATAccountID IS NULL
        BEGIN
            THROW 50004,
                'VAT Input account was not found.',
                1;
        END;

    END;


    -- =====================================================
    -- 7.2 DR EXPENSE
    -- =====================================================
    --
    -- IMPORTANT:
    --
    -- Debit the ORIGINAL amount.
    --
    -- Discount is separately credited to
    -- Purchase Discount account.
    --
    -- =====================================================

    IF @GrossAmount >= 0
    BEGIN

        INSERT INTO TblAccount_JournalEntries
        (
            ExpenseDetailID,
            TransactionID,
            AccountID,
            Debit,
            Credit,
            EntryDate,
            TransactionDate,
            SystemID,
            WarehouseID,
            DomainID,
            CreateByUserID,
            CreateDate,
            Description,
            VATAmount,
            DiscountAmount,
            ExpenseID,
            ExpenseAmount,
            ItemTAX,
            ClassNameID,
            PaymentNo,
            ExpenseNo,
            Type,
            JournalNo,
            InvoiceNo,
            ExpenseTransactionType,
            Account
        )
        VALUES
        (
            @ExpenseDetailID,
            @TransactionID,
            @ExpenseCategory,

            @GrossAmount,

            0,

            @EntryDate,
            @EntryDate,
            @SystemID,
            @WarehouseID,
            @DomainID,
            @CreateByUserID,
            GETDATE(),

            @ItemName,

            @VATAmountLocal,
            @DiscountAmount,
            @ExpenseID,
            @GrossAmount,
            0,
            @ClassNameID,
            @PaymentNo,
            @ExpenseNo,
            @Type,
            @JournalNo,
            @ExpenseNo,
            @ExpenseTransactionType,
            @ExpenseTransactionType + '-' + 
            ISNULL(@ExpenseAccountNo, '') +
                CASE
                    WHEN ISNULL(@expenseCategoryAccName, '') <> ''
                    THEN '-' + @expenseCategoryAccName
                    ELSE ''
                END
        );

    END;


    -- =====================================================
    -- 7.3 PURCHASE DISCOUNT
    -- =====================================================
    --
    -- Example:
    --
    -- Original Expense = 520
    -- Purchase Discount = 20
    --
    -- DR Expense          520
    --     CR Purchase Discount   20
    --
    -- =====================================================

    IF @DiscountAmount > 0
    BEGIN

        IF @PurchaseDiscountAccountID IS NULL
        BEGIN
            THROW 50005,
                'Purchase Discount account was not provided.',
                1;
        END;
        
        DECLARE @DiscountAccountNo NVARCHAR(50), @DiscountAccountName NVARCHAR(50)

        SELECT TOP 1
            @DiscountAccountNo = AccountNo,
            @DiscountAccountName = AccountName
        FROM TblChartOfAccount
        WHERE ChartOfAccountID = @PurchaseDiscountAccountID;

        INSERT INTO TblAccount_JournalEntries
        (
            ExpenseDetailID,
            TransactionID,
            AccountID,
            Debit,
            Credit,
            EntryDate,
            TransactionDate,
            SystemID,
            WarehouseID,
            DomainID,
            CreateByUserID,
            CreateDate,
            Description,
            VATAmount,
            DiscountAmount,
            ExpenseID,
            ExpenseAmount,
            ItemTAX,
            ClassNameID,
            PaymentNo,
            ExpenseNo,
            Type,
            JournalNo,
            InvoiceNo,
            ExpenseTransactionType,
            Account
        )
        VALUES
        (
            @ExpenseDetailID,
            @TransactionID,

            @PurchaseDiscountAccountID,

            0,
            @DiscountAmount,

            @EntryDate,
            @EntryDate,
            @SystemID,
            @WarehouseID,
            @DomainID,
            @CreateByUserID,
            GETDATE(),
            --@ItemName,
            --'Purchase Discount - ' +
            --ISNULL(@ExpenseAccountNo, ''),
            'Discount-' + CONVERT(VARCHAR, @DiscountAmount) + ', ' + @ItemName,
            0,
            @DiscountAmount,
            @ExpenseID,
            @DiscountAmount,
            0,
            @ClassNameID,
            @PaymentNo,
            @ExpenseNo,
            @Type,
            @JournalNo,
            @ExpenseNo,
            @ExpenseTransactionType,
            @ExpenseTransactionType + '-' + @DiscountAccountNo + '-' + @DiscountAccountName
        );

    END;


    -- =====================================================
    -- 7.4 VAT INPUT (Disabled: Single combined VAT Input row posted per expense)
    -- =====================================================

    /*
    IF @VATAmountLocal > 0
    BEGIN
        DECLARE @VATAccountNo NVARCHAR(50), @VATAccountName NVARCHAR(50)

        SELECT TOP 1
            @VATAccountNo = AccountNo,
            @VATAccountName = AccountName
        FROM TblChartOfAccount
        WHERE ChartOfAccountID = @VATAccountID;

        INSERT INTO TblAccount_JournalEntries
        (
            ExpenseDetailID,
            TransactionID,
            AccountID,
            Debit,
            Credit,
            EntryDate,
            TransactionDate,
            SystemID,
            WarehouseID,
            DomainID,
            CreateByUserID,
            CreateDate,
            Description,
            VATAmount,
            DiscountAmount,
            ExpenseID,
            ExpenseAmount,
            ItemTAX,
            ClassNameID,
            PaymentNo,
            ExpenseNo,
            Type,
            JournalNo,
            InvoiceNo,
            ExpenseTransactionType,
            Account
        )
        VALUES
        (
            @ExpenseDetailID,
            @TransactionID,

            @VATAccountID,

            @VATAmountLocal,
            0,

            @EntryDate,
            @EntryDate,
            @SystemID,
            @WarehouseID,
            @DomainID,
            @CreateByUserID,
            GETDATE(),

            --'VAT Input - ' +
            --CONVERT(VARCHAR(20), ISNULL(@ItemTAX, 0)) +
            --'% - ' +
            --ISNULL(@ExpenseAccountNo, ''),
            'VAT-' + CONVERT(VARCHAR(20), ISNULL(@ItemTAX, 0)) + '%, ' + @ItemName,    
            @VATAmountLocal,
            0,
            @ExpenseID,
            @VATAmountLocal,
            @ItemTAX,
            @ClassNameID,
            @PaymentNo,
            @ExpenseNo,
            @Type,
            @JournalNo,
            @ExpenseNo,
            @ExpenseTransactionType,
            @ExpenseTransactionType + '-' + @VATAccountNo + '-' + @VATAccountName
        );

    END;
    */

END;


-- =========================================================
-- 8. FINAL JOURNAL BALANCE CHECK
-- =========================================================
--
-- Expense:
--
-- Debit:
--     Gross + VAT
--
-- Credit:
--     Discount + Grand Total
--
-- =========================================================

IF @TransactionType = 'Expense'
BEGIN

    IF ABS(
        (
            @GrossAmount
            + @VATAmountLocal
        )
        -
        (
            @DiscountAmount
            + @GrandTotal
        )
    ) > 0.01
    BEGIN

        THROW 50006,
            'Expense journal is not balanced. Please check Total, Discount, VAT and Grand Total.',
            1;

    END;

END;

";

            int purchaseDiscountAccountID = 0;

            if (d.DisUSD > 0)
            {
                string purchaseDiscountSql = @"
        SELECT TOP 1
            ChartOfAccountID
        FROM TblChartOfAccount
        WHERE (
        --LTRIM(RTRIM(AccountName)) = 'Purchase Discount'
           -- OR LTRIM(RTRIM(AccountName)) = 'Purchase Discounts'
           -- OR AccountName LIKE '%Purchase Discount%'
            AccountName = 'Purchase Discount'
            OR AccountName LIKE 'Purchase Discount%'
        )
          AND SystemID = @SystemID
          AND (WarehouseID = @WarehouseID OR ISNULL(WarehouseID, 0) = 0)
        ORDER BY
            CASE WHEN WarehouseID = @WarehouseID THEN 0 ELSE 1 END,
            -- CASE WHEN LTRIM(RTRIM(AccountName)) = 'Purchase Discount' THEN 0 ELSE 1 END;
            CASE WHEN AccountName = 'Purchase Discount' THEN 0 ELSE 1 END,
            ChartOfAccountID;
        ";

                using (SqlCommand discountCmd =
                    new SqlCommand(
                        purchaseDiscountSql,
                        conn,
                        tran))
                {
                    discountCmd.Parameters.Add(
                        "@DomainID",
                        SqlDbType.NVarChar
                    ).Value = e.DomainID;

                    discountCmd.Parameters.Add(
                        "@SystemID",
                        SqlDbType.Int
                    ).Value = e.SystemID;

                    discountCmd.Parameters.Add(
                        "@WarehouseID",
                        SqlDbType.Int
                    ).Value = e.WarehouseID;

                    object result =
                        discountCmd.ExecuteScalar();

                    if (result == null ||
                        result == DBNull.Value)
                    {
                        throw new Exception(
                            "Purchase Discount account was not found in Chart of Accounts."
                        );
                    }

                    purchaseDiscountAccountID =
                        Convert.ToInt32(result);
                }
            }
            // =========================================================
            // 14. SQL COMMAND
            // =========================================================

            using (SqlCommand cmd = new SqlCommand(
                sql,
                conn,
                tran))
            {

                // =====================================================
                // COMMON
                // =====================================================
                // =====================================================
                // ITEM DISCOUNT
                // =====================================================

                cmd.Parameters.Add(
                    "@ItemDiscountAmount",
                    SqlDbType.Decimal
                ).Value =
                    d.DisUSD;
                cmd.Parameters.Add(
                    "@PurchaseDiscountAccountID",
                    SqlDbType.Int
                ).Value =
                    purchaseDiscountAccountID > 0 ? (object)purchaseDiscountAccountID : DBNull.Value;

                cmd.Parameters.Add(
                    "@ExpenseID",
                    SqlDbType.Int
                ).Value =
                    expensePostingResult.ExpenseID;

                cmd.Parameters.Add(
                    "@TransactionID",
                    SqlDbType.Int
                ).Value =
                    expensePostingResult.TransactionID;


                // =====================================================
                // PAYMENT ACCOUNT
                // =====================================================
                //
                // This is the user's selected Cash / Bank account.
                //
                // Example:
                // ABA Bank = 111001
                // Petty Cash = 110000
                //

                cmd.Parameters.Add(
                    "@PaymentAccountID",
                    SqlDbType.Int
                ).Value =
                    expensePostingResult.PaymentAccountID;


                // =====================================================
                // CLASS
                // =====================================================

                cmd.Parameters.Add(
                    "@ClassNameID",
                    SqlDbType.Int
                ).Value =
                    expensePostingResult.ClassNameID;


                // =====================================================
                // EXPENSE DEBIT
                // =====================================================

                cmd.Parameters.Add(
                    "@Debit",
                    SqlDbType.Decimal
                ).Value =
                    d.Total + d.DisUSD;


                // =====================================================
                // AP CREDIT
                // =====================================================

                cmd.Parameters.Add(
                    "@Credit",
                    SqlDbType.Decimal
                ).Value =
                    payablePortion;


                // =====================================================
                // PAID PORTION
                // =====================================================

                cmd.Parameters.Add(
                    "@PaidPortion",
                    SqlDbType.Decimal
                ).Value =
                    paidPortion;


                // =====================================================
                // DETAIL
                // =====================================================

                cmd.Parameters.Add(
                    "@ItemTypeID",
                    SqlDbType.Int
                ).Value =
                    d.ItemTypeID;

                cmd.Parameters.Add(
                    "@ExpenseCategory",
                    SqlDbType.Int
                ).Value =
                    d.ExpenseCategory;

                cmd.Parameters.Add(
                    "@RefNo",
                    SqlDbType.NVarChar
                ).Value =
                    d.RefNo ?? "";
                cmd.Parameters.Add(
                    "@Type",
                    SqlDbType.NVarChar
                ).Value = e.IsFromBills ? "Bill" : "Expense";
                cmd.Parameters.Add(
                    "@ExpenseTransactionType",
                    SqlDbType.NVarChar
                ).Value =
                    d.ExpenseTransactionType ?? "";

                cmd.Parameters.Add(
                    "@SourceType",
                    SqlDbType.NVarChar
                ).Value =
                    d.SourceType ?? "";
                cmd.Parameters.Add(
                    "@ExpenseCategoryType",
                    SqlDbType.NVarChar
                ).Value =
                    d.ExpenseCategoryType ?? "";

                cmd.Parameters.Add(
                    "@ItemID",
                    SqlDbType.Int
                ).Value =
                    d.ItemID;

                cmd.Parameters.Add(
                    "@ItemName",
                    SqlDbType.NVarChar
                ).Value =
                    d.ItemName ?? "";

                cmd.Parameters.Add(
                    "@expenseCategoryAccName",
                    SqlDbType.NVarChar
                ).Value =
                    expenseCategoryAccName ?? "";

                cmd.Parameters.Add(
                    "@ItemName_Description",
                    SqlDbType.NVarChar
                ).Value =
                    d.ItemName_Description ?? "";

                cmd.Parameters.Add(
                    "@UM",
                    SqlDbType.NVarChar
                ).Value = d.UM;
                //html.HtmlDecode(d.UM) ?? "";


                // =====================================================
                // QUANTITY / PRICE
                // =====================================================

                cmd.Parameters.Add(
                    "@QTY",
                    SqlDbType.Decimal
                ).Value =
                    d.QTY;

                cmd.Parameters.Add(
                    "@UnitPrice",
                    SqlDbType.Decimal
                ).Value =
                    d.UnitPrice;

                cmd.Parameters.Add(
                    "@DisUSD",
                    SqlDbType.Decimal
                ).Value =
                    d.DisUSD;

                cmd.Parameters.Add(
                    "@Total",
                    SqlDbType.Decimal
                ).Value =
                    d.Total + d.DisUSD;

                cmd.Parameters.Add(
                    "@ItemTAX",
                    SqlDbType.Decimal
                ).Value =
                    d.ItemTAX;

                cmd.Parameters.Add(
                    "@ItemGrandTotal",
                    SqlDbType.Decimal
                ).Value =
                    d.ItemGrandTotal;


                // =====================================================
                // VAT
                // =====================================================

                decimal total = d.Total;
                decimal tax = d.ItemTAX;

                decimal vatAmount =
                    Math.Round(
                        (total * tax) / 100m,
                        2,
                        MidpointRounding.AwayFromZero
                    );

                if (vatAmount < 0m)
                    vatAmount = 0m;

                cmd.Parameters.Add(
                    "@VATAmount",
                    SqlDbType.Decimal
                ).Value =
                    vatAmount;


                // =====================================================
                // COMMON INFORMATION
                // =====================================================

                cmd.Parameters.Add(
                    "@EntryDate",
                    SqlDbType.DateTime
                ).Value =
                    e.PaymentDate;

                cmd.Parameters.Add(
                    "@SystemID",
                    SqlDbType.Int
                ).Value =
                    e.SystemID;

                cmd.Parameters.Add(
                    "@WarehouseID",
                    SqlDbType.Int
                ).Value =
                    e.WarehouseID;

                cmd.Parameters.Add(
                    "@DomainID",
                    SqlDbType.NVarChar
                ).Value =
                    e.DomainID;
                cmd.Parameters.Add(
                    "@PaymentNo",
                    SqlDbType.NVarChar
                ).Value = expensePostingResult.PaymentNo ?? "";

                cmd.Parameters.Add(
                    "@ExpenseNo",
                    SqlDbType.NVarChar
                ).Value = expensePostingResult.ExpenseNo ?? "";
                cmd.Parameters.Add(
                    "@JournalNo",
                    SqlDbType.NVarChar
                ).Value = expensePostingResult.JournalNo ?? "";

                cmd.Parameters.Add(
                    "@CreateByUserID",
                    SqlDbType.Int
                ).Value =
                   e.CreateByUserID;


                // =====================================================
                // EXECUTE
                // =====================================================

                cmd.ExecuteNonQuery();
            }
        }

        private void InsertCombinedVATJournalEntry(
            Expense expense,
            ExpensePostingResult postingResult,
            SqlConnection conn,
            SqlTransaction tran)
        {
            if (expense.Details == null || !expense.Details.Any())
                return;

            //decimal totalVAT = expense.Details
            //    .Where(d => d.ItemTAX > 0)
            //    .Sum(d => Math.Round((d.Total * d.ItemTAX) / 100m, 2, MidpointRounding.AwayFromZero));

            decimal subtotal = expense.Details?
    .Sum(x => x.ItemGrandTotal) ?? 0m;

            decimal VAT = expense.Details?
                .Sum(x => x.ItemVAT) ?? 0m;

            decimal DiscountItem = expense.Details?
                .Sum(x => x.DisUSD) ?? 0m;

            if (subtotal < 0)
                subtotal = 0m;
            var a = subtotal - expense.Discount;
            decimal totalVAT = Math.Round((expense.HeaderTax * a) / 100m, 2, MidpointRounding.AwayFromZero);

            //decimal totalVAT = Math.Round((expense.HeaderTax * expense.PaymentAmount) / 100m, 2, MidpointRounding.AwayFromZero);
            if (totalVAT <= 0)
                return;

            decimal taxRate = expense.HeaderTax;// expense.Details.FirstOrDefault(d => d.ItemTAX > 0)?.ItemTAX ?? 0m;
            string taxRateStr = taxRate > 0 ? (taxRate % 1 == 0 ? taxRate.ToString("0") : taxRate.ToString("0.##")) : "";

            string remarkText = "";
            if (!string.IsNullOrWhiteSpace(expense.VATRemark))
            {
                remarkText = expense.VATRemark.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(expense.Description))
            {
                remarkText = expense.Description.Trim();
            }
            else
            {
                var vatItems = expense.Details.Where(d => d.ItemTAX > 0).ToList();
                List<string> itemDescs = new List<string>();
                foreach (var item in vatItems)
                {
                    string itemDesc = !string.IsNullOrWhiteSpace(item.ItemName_Description)
                        ? item.ItemName_Description.Trim()
                        : (!string.IsNullOrWhiteSpace(item.Description)
                            ? item.Description.Trim()
                            : item.ItemName?.Trim());

                    if (!string.IsNullOrEmpty(itemDesc) && !itemDescs.Contains(itemDesc))
                    {
                        itemDescs.Add(itemDesc);
                    }
                }
                remarkText = string.Join(", ", itemDescs);
            }

            string vatDescription = "";
            if (remarkText.StartsWith("VAT-", StringComparison.OrdinalIgnoreCase) || remarkText.StartsWith("VAT Input", StringComparison.OrdinalIgnoreCase))
            {
                vatDescription = remarkText;
            }
            else if (!string.IsNullOrEmpty(taxRateStr))
            {
                vatDescription = !string.IsNullOrEmpty(remarkText) ? $"{remarkText}" : $"VAT-{taxRateStr}%";
            }
            else
            {
                vatDescription = !string.IsNullOrEmpty(remarkText) ? $"{remarkText}" : "VAT Input";
            }

            HTML html = new HTML();
            int sysId = (expense.SystemID.HasValue && expense.SystemID.Value > 0) ? expense.SystemID.Value : (int.TryParse(html.SystemID, out int sId) ? sId : 1);
            int whId = (expense.WarehouseID.HasValue && expense.WarehouseID.Value > 0) ? expense.WarehouseID.Value : (int.TryParse(html.WarehouseID, out int wId) ? wId : 1);
            int userId = int.TryParse(expense.CreateByUserID, out int uId) ? uId : (int.TryParse(html.UserID, out uId) ? uId : 1);

            string sql = @"
                DECLARE @VATAccountID INT;
                DECLARE @VATAccountNo NVARCHAR(50), @VATAccountName NVARCHAR(50);

                SELECT TOP 1
                    @VATAccountID = ChartOfAccountID,
                    @VATAccountNo = AccountNo,
                    @VATAccountName = AccountName
                FROM TblChartOfAccount
                WHERE (AccountName = 'VAT Input' OR AccountName LIKE '%VAT Input%' OR AccountNo = '140001' OR AccountNo LIKE '1400%' OR AccountNo = '130101')
                ORDER BY CASE WHEN AccountName = 'VAT Input' THEN 1 WHEN AccountNo = '140001' THEN 2 ELSE 3 END;

                IF @VATAccountID IS NOT NULL
                BEGIN
                    INSERT INTO TblAccount_JournalEntries
                    (
                        TransactionID, AccountID, Debit, Credit, EntryDate, TransactionDate,
                        SystemID, WarehouseID, DomainID, CreateByUserID, CreateDate,
                        Description, VATAmount, DiscountAmount, ExpenseID, ExpenseAmount,
                        ItemTAX, ClassNameID, PaymentNo, ExpenseNo, Type, JournalNo,
                        InvoiceNo, ExpenseTransactionType, Account
                    )
                    VALUES
                    (
                        @TransactionID, @VATAccountID, @VATAmount, 0, @EntryDate, @EntryDate,
                        @SystemID, @WarehouseID, @DomainID, @CreateByUserID, GETDATE(),
                        @Description, @VATAmount, 0, @ExpenseID, @VATAmount,
                        @ItemTAX, @ClassNameID, @PaymentNo, @ExpenseNo, @Type, @JournalNo,
                        @ExpenseNo, @ExpenseTransactionType,
                        @ExpenseTransactionType + '-' + ISNULL(@VATAccountNo, '') + '-' + ISNULL(@VATAccountName, '') + ' "+ taxRate + @"%'
                    );
                END;";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.Add("@TransactionID", SqlDbType.Int).Value = postingResult.TransactionID;
                cmd.Parameters.Add("@VATAmount", SqlDbType.Decimal).Value = totalVAT;
                cmd.Parameters.Add("@EntryDate", SqlDbType.DateTime).Value = expense.PaymentDate != DateTime.MinValue ? expense.PaymentDate : DateTime.Now;
                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value = sysId;
                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value = whId;
                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar).Value = (object)html.DomainID ?? DBNull.Value;
                cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = (object)vatDescription ?? DBNull.Value;
                cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = postingResult.ExpenseID;
                cmd.Parameters.Add("@ItemTAX", SqlDbType.Decimal).Value = taxRate;
                cmd.Parameters.Add("@ClassNameID", SqlDbType.Int).Value = postingResult.ClassNameID;
                cmd.Parameters.Add("@PaymentNo", SqlDbType.NVarChar).Value = (object)postingResult.PaymentNo ?? "";
                cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value = expense.Type;
                cmd.Parameters.Add("@ExpenseNo", SqlDbType.NVarChar).Value = (object)postingResult.ExpenseNo ?? "";
                cmd.Parameters.Add("@JournalNo", SqlDbType.NVarChar).Value = (object)postingResult.JournalNo ?? "";
                cmd.Parameters.Add("@ExpenseTransactionType", SqlDbType.NVarChar).Value = (object)expense.ExpenseTransactionType ?? "Expense";
                cmd.ExecuteNonQuery();
            }
        }

        //    private void InsertExpenseDetail(
        //Expense e,
        //ExpensePostingResult expensePostingResult,
        //ExpenseDetail d,
        //int TransactionID,
        //SqlConnection conn,
        //SqlTransaction tran)
        //    {
        //        HTML html = new HTML();

        //        // =========================================================
        //        // 1. EXPENSE ACCOUNT NAME
        //        // =========================================================

        //        string expenseCategoryAccName =
        //            d.AccountName + " - " + d.ItemName;


        //        // =========================================================
        //        // 2. ORIGINAL TOTAL
        //        // =========================================================

        //        decimal originalExpenseTotal =
        //            e.Details?.Sum(x => Math.Max(0m, x.ItemGrandTotal)) ?? 0m;

        //        originalExpenseTotal = Math.Round(
        //            originalExpenseTotal,
        //            2,
        //            MidpointRounding.AwayFromZero
        //        );

        //        if (originalExpenseTotal < 0m)
        //            originalExpenseTotal = 0m;


        //        // =========================================================
        //        // 3. WHOLE EXPENSE DISCOUNT
        //        // =========================================================

        //        decimal wholeDiscount = e.Discount;

        //        if (wholeDiscount < 0m)
        //            wholeDiscount = 0m;

        //        if (wholeDiscount > originalExpenseTotal)
        //            wholeDiscount = originalExpenseTotal;

        //        wholeDiscount = Math.Round(
        //            wholeDiscount,
        //            2,
        //            MidpointRounding.AwayFromZero
        //        );


        //        // =========================================================
        //        // 4. NET EXPENSE
        //        // =========================================================

        //        decimal netExpenseTotal =
        //            Math.Round(
        //                originalExpenseTotal - wholeDiscount,
        //                2,
        //                MidpointRounding.AwayFromZero
        //            );

        //        if (netExpenseTotal < 0m)
        //            netExpenseTotal = 0m;


        //        // =========================================================
        //        // 5. ACTUAL PAID AMOUNT
        //        // =========================================================

        //        decimal paidAmount = e.PaidAmount;

        //        if (paidAmount < 0m)
        //            paidAmount = 0m;

        //        if (paidAmount > netExpenseTotal)
        //            paidAmount = netExpenseTotal;

        //        paidAmount = Math.Round(
        //            paidAmount,
        //            2,
        //            MidpointRounding.AwayFromZero
        //        );


        //        // =========================================================
        //        // 6. PAYMENT RATIO
        //        //
        //        // IMPORTANT:
        //        //
        //        // Payment ratio is based on NET amount after discount.
        //        //
        //        // Example:
        //        //
        //        // Original = 1,520
        //        // Discount =   520
        //        // Net      = 1,000
        //        // Paid     =   500
        //        //
        //        // Ratio = 500 / 1,000 = 50%
        //        // =========================================================

        //        decimal paymentRatio =
        //            netExpenseTotal == 0m
        //                ? 0m
        //                : paidAmount / netExpenseTotal;


        //        // =========================================================
        //        // 7. CURRENT ITEM TOTAL
        //        // =========================================================

        //        decimal itemTotal =
        //            Math.Max(0m, d.ItemGrandTotal);

        //        itemTotal = Math.Round(
        //            itemTotal,
        //            2,
        //            MidpointRounding.AwayFromZero
        //        );


        //        // =========================================================
        //        // 8. DETERMINE CURRENT ITEM INDEX
        //        // =========================================================
        //        //
        //        // We need this to make the LAST item receive the
        //        // remaining rounding amount.
        //        //
        //        // This assumes ExpenseDetail objects are reference-equal
        //        // to the objects inside e.Details.
        //        //
        //        // If your list is not reference-equal, use the overload
        //        // shown below that passes itemIndex/isLastItem.
        //        // =========================================================

        //        int itemIndex = -1;

        //        if (e.Details != null)
        //        {
        //            for (int i = 0; i < e.Details.Count; i++)
        //            {
        //                if (ReferenceEquals(e.Details[i], d))
        //                {
        //                    itemIndex = i;
        //                    break;
        //                }
        //            }
        //        }

        //        bool isLastItem =
        //            e.Details != null &&
        //            itemIndex >= 0 &&
        //            itemIndex == e.Details.Count - 1;


        //        // =========================================================
        //        // 9. ITEM DISCOUNT
        //        // =========================================================

        //        decimal itemDiscount = 0m;

        //        if (originalExpenseTotal > 0m &&
        //            wholeDiscount > 0m)
        //        {
        //            if (isLastItem)
        //            {
        //                // -------------------------------------------------
        //                // LAST ITEM:
        //                // Give it the remaining discount so that:
        //                //
        //                // Sum(ItemDiscount) = WholeDiscount
        //                // -------------------------------------------------

        //                decimal previousAllocatedDiscount = 0m;

        //                for (int i = 0; i < itemIndex; i++)
        //                {
        //                    decimal previousItem =
        //                        Math.Max(
        //                            0m,
        //                            e.Details[i].ItemGrandTotal
        //                        );

        //                    decimal previousDiscount =
        //                        Math.Round(
        //                            wholeDiscount *
        //                            (previousItem / originalExpenseTotal),
        //                            2,
        //                            MidpointRounding.AwayFromZero
        //                        );

        //                    if (previousDiscount > previousItem)
        //                        previousDiscount = previousItem;

        //                    previousAllocatedDiscount +=
        //                        previousDiscount;
        //                }

        //                itemDiscount =
        //                    Math.Round(
        //                        wholeDiscount -
        //                        previousAllocatedDiscount,
        //                        2,
        //                        MidpointRounding.AwayFromZero
        //                    );
        //            }
        //            else
        //            {
        //                itemDiscount =
        //                    Math.Round(
        //                        wholeDiscount *
        //                        (itemTotal / originalExpenseTotal),
        //                        2,
        //                        MidpointRounding.AwayFromZero
        //                    );
        //            }
        //        }

        //        if (itemDiscount < 0m)
        //            itemDiscount = 0m;

        //        if (itemDiscount > itemTotal)
        //            itemDiscount = itemTotal;


        //        // =========================================================
        //        // 10. NET ITEM
        //        // =========================================================

        //        decimal netItemTotal =
        //            Math.Round(
        //                itemTotal - itemDiscount,
        //                2,
        //                MidpointRounding.AwayFromZero
        //            );

        //        if (netItemTotal < 0m)
        //            netItemTotal = 0m;


        //        // =========================================================
        //        // 11. PAID PORTION
        //        // =========================================================
        //        //
        //        // IMPORTANT:
        //        //
        //        // We allocate the ACTUAL $500 payment against the
        //        // NET $1,000 expense.
        //        //
        //        // Example:
        //        //
        //        // Item 1:
        //        //   1,000 - 333.33 discount = 666.67
        //        //
        //        // Item 2:
        //        //     520 - 186.67 discount = 333.33
        //        //
        //        // Net total = 1,000
        //        //
        //        // Paid = 500
        //        //
        //        // Item 1 paid = 333.34
        //        // Item 2 paid = 166.66
        //        //
        //        // Total paid = 500.00
        //        // =========================================================

        //        decimal paidPortion = 0m;

        //        if (netExpenseTotal > 0m)
        //        {
        //            if (isLastItem)
        //            {
        //                // -------------------------------------------------
        //                // LAST ITEM:
        //                // Calculate previous paid amounts and give the
        //                // remaining payment to the last item.
        //                // -------------------------------------------------

        //                decimal previousPaid = 0m;

        //                for (int i = 0; i < itemIndex; i++)
        //                {
        //                    decimal previousItem =
        //                        Math.Max(
        //                            0m,
        //                            e.Details[i].ItemGrandTotal
        //                        );

        //                    decimal previousDiscount = 0m;

        //                    if (originalExpenseTotal > 0m &&
        //                        wholeDiscount > 0m)
        //                    {
        //                        previousDiscount =
        //                            Math.Round(
        //                                wholeDiscount *
        //                                (previousItem / originalExpenseTotal),
        //                                2,
        //                                MidpointRounding.AwayFromZero
        //                            );

        //                        if (previousDiscount > previousItem)
        //                            previousDiscount = previousItem;
        //                    }

        //                    decimal previousNet =
        //                        Math.Round(
        //                            previousItem - previousDiscount,
        //                            2,
        //                            MidpointRounding.AwayFromZero
        //                        );

        //                    if (previousNet < 0m)
        //                        previousNet = 0m;


        //                    previousPaid =
        //                         Math.Round(
        //                             previousNet * paymentRatio,
        //                             2,
        //                             MidpointRounding.AwayFromZero
        //                         );

        //                    if (previousPaid < 0m)
        //                        previousPaid = 0m;

        //                    if (previousPaid > previousNet)
        //                        previousPaid = previousNet;

        //                    previousPaid += previousPaid;
        //                }

        //                // -------------------------------------------------
        //                // IMPORTANT:
        //                //
        //                // The above loop should calculate previous item
        //                // payments separately.
        //                // -------------------------------------------------

        //                previousPaid = 0m;

        //                for (int i = 0; i < itemIndex; i++)
        //                {
        //                    decimal previousItem =
        //                        Math.Max(
        //                            0m,
        //                            e.Details[i].ItemGrandTotal
        //                        );

        //                    decimal previousDiscount = 0m;

        //                    if (originalExpenseTotal > 0m &&
        //                        wholeDiscount > 0m)
        //                    {
        //                        previousDiscount =
        //                            Math.Round(
        //                                wholeDiscount *
        //                                (previousItem / originalExpenseTotal),
        //                                2,
        //                                MidpointRounding.AwayFromZero
        //                            );

        //                        if (previousDiscount > previousItem)
        //                            previousDiscount = previousItem;
        //                    }

        //                    decimal previousNet =
        //                        Math.Round(
        //                            previousItem - previousDiscount,
        //                            2,
        //                            MidpointRounding.AwayFromZero
        //                        );

        //                    if (previousNet < 0m)
        //                        previousNet = 0m;

        //                    decimal previousPaidPart =
        //                        Math.Round(
        //                            previousNet * paymentRatio,
        //                            2,
        //                            MidpointRounding.AwayFromZero
        //                        );

        //                    if (previousPaidPart < 0m)
        //                        previousPaidPart = 0m;

        //                    if (previousPaidPart > previousNet)
        //                        previousPaidPart = previousNet;

        //                    previousPaid += previousPaidPart;
        //                }

        //                paidPortion =
        //                    Math.Round(
        //                        paidAmount - previousPaid,
        //                        2,
        //                        MidpointRounding.AwayFromZero
        //                    );
        //            }
        //            else
        //            {
        //                paidPortion =
        //                    Math.Round(
        //                        netItemTotal * paymentRatio,
        //                        2,
        //                        MidpointRounding.AwayFromZero
        //                    );
        //            }
        //        }

        //        if (paidPortion < 0m)
        //            paidPortion = 0m;

        //        if (paidPortion > netItemTotal)
        //            paidPortion = netItemTotal;


        //        // =========================================================
        //        // 12. AP PORTION
        //        // =========================================================

        //        decimal payablePortion =
        //            Math.Round(
        //                netItemTotal - paidPortion,
        //                2,
        //                MidpointRounding.AwayFromZero
        //            );

        //        if (payablePortion < 0m)
        //            payablePortion = 0m;


        //        // =========================================================
        //        // 13. SQL
        //        // =========================================================

        //        string sql = @"

        //    -- =========================================================
        //    -- 1. VAT INPUT ACCOUNT
        //    -- =========================================================

        //    DECLARE @VATAccountID INT;

        //    SELECT TOP 1
        //        @VATAccountID = ChartOfAccountID
        //    FROM TblChartOfAccount
        //    WHERE AccountName = 'VAT Input'
        //      AND DomainID = @DomainID
        //      AND SystemID = @SystemID
        //      AND WarehouseID = @WarehouseID;


        //    -- =========================================================
        //    -- 2. AUTOMATICALLY FIND ACCOUNTS PAYABLE
        //    -- =========================================================

        //    DECLARE @APAccountID INT;

        //    SELECT TOP 1
        //        @APAccountID = ChartOfAccountID
        //    FROM TblChartOfAccount
        //    WHERE NormalSide = 'Credit'
        //      AND ParentAccountID = 0
        //      AND
        //      (
        //            AccountName = 'Accounts Payable (A/P)'
        //         OR AccountName = 'Account Payable (A/P)'
        //         OR AccountName LIKE '%Accounts Payable%'
        //         OR AccountName LIKE '%Account Payable%'
        //         OR AccountName LIKE '%Payable%'
        //      )
        //      AND SystemID = @SystemID
        //      AND WarehouseID = @WarehouseID
        //      AND DomainID = @DomainID
        //    ORDER BY
        //        CASE
        //            WHEN AccountName = 'Accounts Payable (A/P)' THEN 1
        //            WHEN AccountName = 'Account Payable (A/P)' THEN 2
        //            WHEN AccountName LIKE '%Accounts Payable%' THEN 3
        //            WHEN AccountName LIKE '%Account Payable%' THEN 4
        //            WHEN AccountName LIKE '%Payable%' THEN 5
        //            ELSE 6
        //        END;


        //    IF @APAccountID IS NULL
        //    BEGIN
        //        THROW 50001,
        //            'Accounts Payable (A/P) account was not found.',
        //            1;
        //    END;


        //    -- =========================================================
        //    -- 3. INSERT EXPENSE DETAIL
        //    -- =========================================================

        //    INSERT INTO ExpenseDetails
        //    (
        //        ExpenseID,
        //        ItemTypeID,
        //        ExpenseCategory,
        //        ItemID,
        //        ItemName,
        //        ItemName_Description,
        //        UM,
        //        QTY,
        //        UnitPrice,
        //        DisUSD,
        //        Total,
        //        ItemTAX,
        //        ItemGrandTotal,
        //        RefNo,
        //        SourceType
        //    )
        //    VALUES
        //    (
        //        @ExpenseID,
        //        @ItemTypeID,
        //        @ExpenseCategory,
        //        @ItemID,
        //        @ItemName,
        //        @ItemName_Description,
        //        @UM,
        //        @QTY,
        //        @UnitPrice,
        //        @DisUSD,
        //        @Total,
        //        @ItemTAX,
        //        @ItemGrandTotal,
        //        @RefNo,
        //        @SourceType
        //    );


        //    -- =========================================================
        //    -- 4. GET EXPENSE ACCOUNT NAME
        //    -- =========================================================

        //    DECLARE @ExpenseCategoryName NVARCHAR(200);

        //    SELECT TOP 1
        //        @ExpenseCategoryName = AccountName
        //    FROM TblChartOfAccount
        //    WHERE ChartOfAccountID = @ExpenseCategory
        //      AND DomainID = @DomainID
        //      AND SystemID = @SystemID
        //      AND WarehouseID = @WarehouseID;


        //    -- =========================================================
        //    -- 5. VENDOR / SUPPLIER DEPOSIT OR ADVANCE
        //    -- =========================================================

        //    IF
        //    (
        //           @ExpenseCategoryName = 'Vendor Deposit'
        //        OR @ExpenseCategoryName = 'Supplier Deposit'
        //        OR @ExpenseCategoryName = 'Vendor Advance'
        //        OR @ExpenseCategoryName = 'Supplier Advance'
        //    )
        //    BEGIN

        //        -- DR Vendor / Supplier Deposit
        //        INSERT INTO TblAccount_JournalEntries
        //        (
        //            TransactionID,
        //            AccountID,
        //            Debit,
        //            Credit,
        //            EntryDate,
        //            TransactionDate,
        //            SystemID,
        //            WarehouseID,
        //            DomainID,
        //            CreateByUserID,
        //            CreateDate,
        //            Description,
        //            VATAmount,
        //            DiscountAmount,
        //            ExpenseID,
        //            ExpenseAmount,
        //            ItemTAX,
        //            ClassNameID
        //        )
        //        VALUES
        //        (
        //            @TransactionID,
        //            @ExpenseCategory,
        //            @PaidPortion,
        //            0,
        //            @EntryDate,
        //            @EntryDate,
        //            @SystemID,
        //            @WarehouseID,
        //            @DomainID,
        //            @CreateByUserID,
        //            GETDATE(),
        //            @ExpenseCategoryName + ' - ' + @ItemName,
        //            0,
        //            @DisUSD,
        //            @ExpenseID,
        //            @ItemGrandTotal,
        //            0,
        //            @ClassNameID
        //        );


        //        -- CR Cash / Bank
        //        IF(@PaidPortion > 0)
        //        BEGIN

        //            INSERT INTO TblAccount_JournalEntries
        //            (
        //                TransactionID,
        //                AccountID,
        //                Debit,
        //                Credit,
        //                EntryDate,
        //                TransactionDate,
        //                SystemID,
        //                WarehouseID,
        //                DomainID,
        //                CreateByUserID,
        //                CreateDate,
        //                Description,
        //                VATAmount,
        //                DiscountAmount,
        //                ExpenseID,
        //                ExpenseAmount,
        //                ItemTAX,
        //                ClassNameID
        //            )
        //            VALUES
        //            (
        //                @TransactionID,
        //                @PaymentAccountID,
        //                0,
        //                @PaidPortion,
        //                @EntryDate,
        //                @EntryDate,
        //                @SystemID,
        //                @WarehouseID,
        //                @DomainID,
        //                @CreateByUserID,
        //                GETDATE(),
        //                'Paid ' + @ExpenseCategoryName + ' - ' + @ItemName,
        //                0,
        //                @DisUSD,
        //                @ExpenseID,
        //                @ItemGrandTotal,
        //                0,
        //                @ClassNameID
        //            );

        //        END

        //    END
        //    ELSE
        //    BEGIN

        //        -- =====================================================
        //        -- 6. EXPENSE DEBIT
        //        -- =====================================================

        //        INSERT INTO TblAccount_JournalEntries
        //        (
        //            TransactionID,
        //            AccountID,
        //            Debit,
        //            Credit,
        //            EntryDate,
        //            TransactionDate,
        //            SystemID,
        //            WarehouseID,
        //            DomainID,
        //            CreateByUserID,
        //            CreateDate,
        //            Description,
        //            VATAmount,
        //            DiscountAmount,
        //            ExpenseID,
        //            ExpenseAmount,
        //            ItemTAX,
        //            ClassNameID,
        //            PaymentNo,
        //            ExpenseNo
        //        )
        //        VALUES
        //        (
        //            @TransactionID,
        //            @ExpenseCategory,
        //            @Debit,
        //            0,
        //            @EntryDate,
        //            @EntryDate,
        //            @SystemID,
        //            @WarehouseID,
        //            @DomainID,
        //            @CreateByUserID,
        //            GETDATE(),
        //            @expenseCategoryAccName,
        //            @VATAmount,
        //            @DisUSD,
        //            @ExpenseID,
        //            @ItemGrandTotal,
        //            0,
        //            @ClassNameID,
        //            @PaymentNo,
        //            @ExpenseNo
        //        );

        //    -- =========================================================
        //    -- 6. ITEM-LEVEL PURCHASE DISCOUNT
        //    --
        //    -- This is the important new part.
        //    --
        //    -- Example:
        //    --
        //    -- Item price       = 520
        //    -- Item discount    = 20
        //    --
        //    -- DR Expense       = 520
        //    -- CR Discount      = 20
        //    --
        //    -- =========================================================

        //    IF ISNULL(@ItemDiscountAmount, 0) > 0
        //    BEGIN

        //        INSERT INTO TblAccount_JournalEntries
        //        (
        //            TransactionID,
        //            AccountID,
        //            Debit,
        //            Credit,
        //            EntryDate,
        //            TransactionDate,
        //            SystemID,
        //            WarehouseID,
        //            DomainID,
        //            CreateByUserID,
        //            CreateDate,
        //            Description,
        //            VATAmount,
        //            DiscountAmount,
        //            ExpenseID,
        //            ExpenseAmount,
        //            ItemTAX,
        //            ClassNameID,
        //            PaymentNo,
        //            ExpenseNo
        //        )
        //        VALUES
        //        (
        //            @TransactionID,
        //            @PurchaseDiscountAccountID,
        //            0,
        //            @ItemDiscountAmount,
        //            @EntryDate,
        //            @EntryDate,
        //            @SystemID,
        //            @WarehouseID,
        //            @DomainID,
        //            @CreateByUserID,
        //            GETDATE(),
        //            'Item Discount - ' + @expenseCategoryAccName,
        //            0,
        //            @ItemDiscountAmount,
        //            @ExpenseID,
        //            @ItemDiscountAmount,
        //            0,
        //            @ClassNameID,
        //            @PaymentNo,
        //            @ExpenseNo
        //        );

        //    END;
        //        -- =====================================================
        //        -- 7. ACCOUNTS PAYABLE
        //        -- =====================================================

        //        IF(@Credit > 0)
        //        BEGIN

        //            INSERT INTO TblAccount_JournalEntries
        //            (
        //                TransactionID,
        //                AccountID,
        //                Debit,
        //                Credit,
        //                EntryDate,
        //                TransactionDate,
        //                SystemID,
        //                WarehouseID,
        //                DomainID,
        //                CreateByUserID,
        //                CreateDate,
        //                Description,
        //                VATAmount,
        //                DiscountAmount,
        //                ExpenseID,
        //                ExpenseAmount,
        //                ItemTAX,
        //                ClassNameID,
        //                PaymentNo,
        //                ExpenseNo
        //            )
        //            VALUES
        //            (
        //                @TransactionID,
        //                @APAccountID,
        //                0,
        //                @Credit,
        //                @EntryDate,
        //                @EntryDate,
        //                @SystemID,
        //                @WarehouseID,
        //                @DomainID,
        //                @CreateByUserID,
        //                GETDATE(),
        //                'Accounts Payable (A/P) - ' + @expenseCategoryAccName,
        //                @VATAmount,
        //                @DisUSD,
        //                @ExpenseID,
        //                @ItemGrandTotal,
        //                0,
        //                @ClassNameID,
        //                @PaymentNo,
        //                @ExpenseNo
        //            );

        //        END


        //        -- =====================================================
        //        -- 8. CASH / BANK
        //        -- =====================================================

        //        IF(@PaidPortion > 0)
        //        BEGIN

        //            INSERT INTO TblAccount_JournalEntries
        //            (
        //                TransactionID,
        //                AccountID,
        //                Debit,
        //                Credit,
        //                EntryDate,
        //                TransactionDate,
        //                SystemID,
        //                WarehouseID,
        //                DomainID,
        //                CreateByUserID,
        //                CreateDate,
        //                Description,
        //                VATAmount,
        //                DiscountAmount,
        //                ExpenseID,
        //                ExpenseAmount,
        //                ItemTAX,
        //                ClassNameID,
        //                PaymentNo,
        //                ExpenseNo
        //            )
        //            VALUES
        //            (
        //                @TransactionID,
        //                @PaymentAccountID,
        //                0,
        //                @PaidPortion,
        //                @EntryDate,
        //                @EntryDate,
        //                @SystemID,
        //                @WarehouseID,
        //                @DomainID,
        //                @CreateByUserID,
        //                GETDATE(),
        //                'Paid - ' + @expenseCategoryAccName,
        //                @VATAmount,
        //                @DisUSD,
        //                @ExpenseID,
        //                @ItemGrandTotal,
        //                0,
        //                @ClassNameID,
        //                @PaymentNo,
        //                @ExpenseNo
        //            );

        //        END


        //        -- =====================================================
        //        -- 9. VAT INPUT
        //        -- =====================================================

        //        IF(@VATAmount > 0)
        //        BEGIN

        //            INSERT INTO TblAccount_JournalEntries
        //            (
        //                TransactionID,
        //                AccountID,
        //                Debit,
        //                Credit,
        //                EntryDate,
        //                TransactionDate,
        //                SystemID,
        //                WarehouseID,
        //                DomainID,
        //                CreateByUserID,
        //                CreateDate,
        //                Description,
        //                VATAmount,
        //                DiscountAmount,
        //                ExpenseID,
        //                ExpenseAmount,
        //                ItemTAX,
        //                ClassNameID,
        //                PaymentNo,
        //                ExpenseNo
        //            )
        //            VALUES
        //            (
        //                @TransactionID,
        //                @VATAccountID,
        //                @VATAmount,
        //                0,
        //                @EntryDate,
        //                @EntryDate,
        //                @SystemID,
        //                @WarehouseID,
        //                @DomainID,
        //                @CreateByUserID,
        //                GETDATE(),
        //                'VAT - ' + @expenseCategoryAccName,
        //                @VATAmount,
        //                0,
        //                @ExpenseID,
        //                @VATAmount,
        //                @ItemTAX,
        //                @ClassNameID,
        //                @PaymentNo,
        //                @ExpenseNo
        //            );

        //        END

        //    END
        //    ";

        //        int purchaseDiscountAccountID;

        //        string purchaseDiscountSql = @"
        //    SELECT TOP 1
        //        ChartOfAccountID
        //    FROM TblChartOfAccount
        //    WHERE AccountName = 'Purchase Discount'
        //      AND DomainID = @DomainID
        //      AND SystemID = @SystemID
        //      AND WarehouseID = @WarehouseID;
        //    ";

        //        using (SqlCommand discountCmd =
        //            new SqlCommand(
        //                purchaseDiscountSql,
        //                conn,
        //                tran))
        //        {
        //            discountCmd.Parameters.Add(
        //                "@DomainID",
        //                SqlDbType.NVarChar
        //            ).Value = html.DomainID;

        //            discountCmd.Parameters.Add(
        //                "@SystemID",
        //                SqlDbType.Int
        //            ).Value = html.SystemID;

        //            discountCmd.Parameters.Add(
        //                "@WarehouseID",
        //                SqlDbType.Int
        //            ).Value = html.WarehouseID;

        //            object result =
        //                discountCmd.ExecuteScalar();

        //            if (result == null ||
        //                result == DBNull.Value)
        //            {
        //                throw new Exception(
        //                    "Purchase Discount account was not found."
        //                );
        //            }

        //            purchaseDiscountAccountID =
        //                Convert.ToInt32(result);
        //        }
        //        // =========================================================
        //        // 14. SQL COMMAND
        //        // =========================================================

        //        using (SqlCommand cmd = new SqlCommand(
        //            sql,
        //            conn,
        //            tran))
        //        {

        //            // =====================================================
        //            // COMMON
        //            // =====================================================
        //            // =====================================================
        //            // ITEM DISCOUNT
        //            // =====================================================

        //            cmd.Parameters.Add(
        //                "@ItemDiscountAmount",
        //                SqlDbType.Decimal
        //            ).Value =
        //                d.DisUSD;
        //            cmd.Parameters.Add(
        //            "@PurchaseDiscountAccountID",
        //            SqlDbType.Int
        //        ).Value =
        //            purchaseDiscountAccountID;

        //            cmd.Parameters.Add(
        //                "@ExpenseID",
        //                SqlDbType.Int
        //            ).Value =
        //                expensePostingResult.ExpenseID;

        //            cmd.Parameters.Add(
        //                "@TransactionID",
        //                SqlDbType.Int
        //            ).Value =
        //                expensePostingResult.TransactionID;


        //            // =====================================================
        //            // PAYMENT ACCOUNT
        //            // =====================================================
        //            //
        //            // This is the user's selected Cash / Bank account.
        //            //
        //            // Example:
        //            // ABA Bank = 111001
        //            // Petty Cash = 110000
        //            //

        //            cmd.Parameters.Add(
        //                "@PaymentAccountID",
        //                SqlDbType.Int
        //            ).Value =
        //                expensePostingResult.PaymentAccountID;


        //            // =====================================================
        //            // CLASS
        //            // =====================================================

        //            cmd.Parameters.Add(
        //                "@ClassNameID",
        //                SqlDbType.Int
        //            ).Value =
        //                expensePostingResult.ClassNameID;


        //            // =====================================================
        //            // EXPENSE DEBIT
        //            // =====================================================

        //            cmd.Parameters.Add(
        //                "@Debit",
        //                SqlDbType.Decimal
        //            ).Value =
        //                d.Total + d.DisUSD;


        //            // =====================================================
        //            // AP CREDIT
        //            // =====================================================

        //            cmd.Parameters.Add(
        //                "@Credit",
        //                SqlDbType.Decimal
        //            ).Value =
        //                payablePortion;


        //            // =====================================================
        //            // PAID PORTION
        //            // =====================================================

        //            cmd.Parameters.Add(
        //                "@PaidPortion",
        //                SqlDbType.Decimal
        //            ).Value =
        //                paidPortion;


        //            // =====================================================
        //            // DETAIL
        //            // =====================================================

        //            cmd.Parameters.Add(
        //                "@ItemTypeID",
        //                SqlDbType.Int
        //            ).Value =
        //                d.ItemTypeID;

        //            cmd.Parameters.Add(
        //                "@ExpenseCategory",
        //                SqlDbType.Int
        //            ).Value =
        //                d.ExpenseCategory;

        //            cmd.Parameters.Add(
        //                "@RefNo",
        //                SqlDbType.NVarChar
        //            ).Value =
        //                d.RefNo ?? "";

        //            cmd.Parameters.Add(
        //                "@SourceType",
        //                SqlDbType.NVarChar
        //            ).Value =
        //                d.SourceType ?? "";

        //            cmd.Parameters.Add(
        //                "@ItemID",
        //                SqlDbType.Int
        //            ).Value =
        //                d.ItemID;

        //            cmd.Parameters.Add(
        //                "@ItemName",
        //                SqlDbType.NVarChar
        //            ).Value =
        //                d.ItemName ?? "";

        //            cmd.Parameters.Add(
        //                "@expenseCategoryAccName",
        //                SqlDbType.NVarChar
        //            ).Value =
        //                expenseCategoryAccName ?? "";

        //            cmd.Parameters.Add(
        //                "@ItemName_Description",
        //                SqlDbType.NVarChar
        //            ).Value =
        //                d.ItemName_Description ?? "";

        //            cmd.Parameters.Add(
        //                "@UM",
        //                SqlDbType.NVarChar
        //            ).Value =
        //                html.HtmlDecode(d.UM) ?? "";


        //            // =====================================================
        //            // QUANTITY / PRICE
        //            // =====================================================

        //            cmd.Parameters.Add(
        //                "@QTY",
        //                SqlDbType.Decimal
        //            ).Value =
        //                d.QTY;

        //            cmd.Parameters.Add(
        //                "@UnitPrice",
        //                SqlDbType.Decimal
        //            ).Value =
        //                d.UnitPrice;

        //            cmd.Parameters.Add(
        //                "@DisUSD",
        //                SqlDbType.Decimal
        //            ).Value =
        //                d.DisUSD;

        //            cmd.Parameters.Add(
        //                "@Total",
        //                SqlDbType.Decimal
        //            ).Value =
        //                d.Total;

        //            cmd.Parameters.Add(
        //                "@ItemTAX",
        //                SqlDbType.Decimal
        //            ).Value =
        //                d.ItemTAX;

        //            cmd.Parameters.Add(
        //                "@ItemGrandTotal",
        //                SqlDbType.Decimal
        //            ).Value =
        //                d.ItemGrandTotal;


        //            // =====================================================
        //            // VAT
        //            // =====================================================

        //            decimal total = d.Total;
        //            decimal tax = d.ItemTAX;

        //            decimal vatAmount =
        //                Math.Round(
        //                    (total * tax) / 100m,
        //                    2,
        //                    MidpointRounding.AwayFromZero
        //                );

        //            if (vatAmount < 0m)
        //                vatAmount = 0m;

        //            cmd.Parameters.Add(
        //                "@VATAmount",
        //                SqlDbType.Decimal
        //            ).Value =
        //                vatAmount;


        //            // =====================================================
        //            // COMMON INFORMATION
        //            // =====================================================

        //            cmd.Parameters.Add(
        //                "@EntryDate",
        //                SqlDbType.DateTime
        //            ).Value =
        //                e.PaymentDate;

        //            cmd.Parameters.Add(
        //                "@SystemID",
        //                SqlDbType.Int
        //            ).Value =
        //                html.SystemID;

        //            cmd.Parameters.Add(
        //                "@WarehouseID",
        //                SqlDbType.Int
        //            ).Value =
        //                html.WarehouseID;

        //            cmd.Parameters.Add(
        //                "@DomainID",
        //                SqlDbType.NVarChar
        //            ).Value =
        //                html.DomainID;
        //            cmd.Parameters.Add(
        //                "@PaymentNo",
        //                SqlDbType.NVarChar
        //            ).Value =
        //                expensePostingResult.PaymentNo;
        //            cmd.Parameters.Add(
        //                "@ExpenseNo",
        //                SqlDbType.NVarChar
        //            ).Value = expensePostingResult.ExpenseNo;

        //            cmd.Parameters.Add(
        //                "@CreateByUserID",
        //                SqlDbType.Int
        //            ).Value =
        //                html.UserID;


        //            // =====================================================
        //            // EXECUTE
        //            // =====================================================

        //            cmd.ExecuteNonQuery();
        //        }
        //    }

        private void InsertWholeExpenseDiscountJournal(
    Expense expense,
    ExpensePostingResult expensePostingResult,
    decimal wholeExpenseDiscount,
    SqlConnection conn,
    SqlTransaction tran)
        {
            // ============================================================
            // VALIDATION
            // ============================================================

            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            if (expensePostingResult == null)
                throw new ArgumentNullException(nameof(expensePostingResult));

            if (conn == null)
                throw new ArgumentNullException(nameof(conn));

            if (tran == null)
                throw new ArgumentNullException(nameof(tran));

            // No whole-expense discount -> nothing to insert
            if (wholeExpenseDiscount <= 0m)
                return;


            // ============================================================
            // HTML / SYSTEM INFORMATION
            // ============================================================

            //HTML html = new HTML();

            int expenseId = expensePostingResult.ExpenseID;
            int transactionId = expensePostingResult.TransactionID;

            if (expenseId <= 0)
                throw new Exception("Invalid ExpenseID.");

            if (transactionId <= 0)
                throw new Exception("Invalid TransactionID.");


            // ------------------------------------------------------------
            // Safely convert system information
            // ------------------------------------------------------------

            int systemId;

            if (!int.TryParse(Convert.ToString(expense.SystemID), out systemId))
            {
                throw new Exception(
                    "Invalid SystemID: " + Convert.ToString(expense.SystemID)
                );
            }


            int warehouseId;

            if (!int.TryParse(Convert.ToString(expense.WarehouseID), out warehouseId))
            {
                throw new Exception(
                    "Invalid WarehouseID: " + Convert.ToString(expense.WarehouseID)
                );
            }


            int createByUserId;

            if (!int.TryParse(Convert.ToString(expense.CreateByUserID), out createByUserId))
            {
                throw new Exception(
                    "Invalid CreateByUserID: " + Convert.ToString(expense.CreateByUserID)
                );
            }


            string domainId =
                Convert.ToString(expense.DomainID);


            if (string.IsNullOrWhiteSpace(domainId))
                throw new Exception("Invalid DomainID.");


            int classNameId =
                expense.ClassName;


            DateTime entryDate =
                expense.PaymentDate;


            // ============================================================
            // NORMALIZE DECIMAL
            // ============================================================

            wholeExpenseDiscount =
                Math.Round(
                    wholeExpenseDiscount,
                    2,
                    MidpointRounding.AwayFromZero
                );


            if (wholeExpenseDiscount <= 0m)
                return;


            // ============================================================
            // FIND PURCHASE DISCOUNT ACCOUNT
            // ============================================================

            const string getPurchaseDiscountAccountSql = @"
SELECT TOP 1
       ChartOfAccountID
FROM TblChartOfAccount
WHERE (
    LTRIM(RTRIM(AccountName)) = 'Purchase Discount'
    OR LTRIM(RTRIM(AccountName)) = 'Purchase Discounts'
    OR AccountName LIKE '%Purchase Discount%'
)
  AND SystemID = @SystemID
  AND (WarehouseID = @WarehouseID OR ISNULL(WarehouseID, 0) = 0)
ORDER BY
    CASE WHEN WarehouseID = @WarehouseID THEN 0 ELSE 1 END,
    CASE WHEN LTRIM(RTRIM(AccountName)) = 'Purchase Discount' THEN 0 ELSE 1 END,
    ChartOfAccountID;
";


            int purchaseDiscountAccountId;


            using (SqlCommand cmd = new SqlCommand(
                getPurchaseDiscountAccountSql,
                conn,
                tran))
            {
                cmd.Parameters.Add("@AccountName", SqlDbType.NVarChar, 200)
                    .Value = "Purchase Discount";

                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar, 50)
                    .Value = domainId;

                cmd.Parameters.Add("@SystemID", SqlDbType.Int)
                    .Value = systemId;

                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int)
                    .Value = warehouseId;


                object result =
                    cmd.ExecuteScalar();


                if (result == null || result == DBNull.Value)
                {
                    throw new Exception(
                        "Purchase Discount account was not found."
                    );
                }


                purchaseDiscountAccountId =
                    Convert.ToInt32(result);


                if (purchaseDiscountAccountId <= 0)
                {
                    throw new Exception(
                        "Invalid Purchase Discount AccountID."
                    );
                }
            }


            // ============================================================
            // GET A/P ACCOUNT NAME
            //
            // We use APAccountID only to make the description meaningful.
            //
            // Example:
            //
            // Whole Expense Discount - Account Payable (A/P)
            // ============================================================

            string expenseCategoryName = "Account Payable (A/P)";
            string expenseCategoryAccountNo = "";

            const string getExpenseAccountNameSql = @"
SELECT TOP 1
       AccountNo,
       AccountName
FROM TblChartOfAccount
WHERE ChartOfAccountID = @AccountID;
";

            using (SqlCommand cmd = new SqlCommand(
                getExpenseAccountNameSql,
                conn,
                tran))
            {
                cmd.Parameters.Add("@AccountID", SqlDbType.Int)
                    .Value = expensePostingResult.APAccountID;

                //cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar, 50)
                //    .Value = domainId;

                //cmd.Parameters.Add("@SystemID", SqlDbType.Int)
                //    .Value = systemId;

                //cmd.Parameters.Add("@WarehouseID", SqlDbType.Int)
                //    .Value = warehouseId;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("AccountNo")))
                        {
                            expenseCategoryAccountNo =
                                reader["AccountNo"].ToString().Trim();
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("AccountName")))
                        {
                            expenseCategoryName =
                                reader["AccountName"].ToString().Trim();
                        }
                    }
                }
            }


            // ============================================================
            // DESCRIPTION
            // ============================================================

            //        string description =
            //"Whole Expense Discount - "
            //+ expenseCategoryAccountNo
            //+ "-"
            //+ expenseCategoryName;
            string description =
    "Whole Expense Discount - "
    + expense.Discount + "%";

            // ============================================================
            // CHECK EXISTING WHOLE EXPENSE DISCOUNT
            //
            // This prevents accidental duplicate insertion if this method
            // is called more than once for the same ExpenseID/TransactionID.
            //
            // IMPORTANT:
            // If your UPDATE logic deletes all old journal entries before
            // inserting again, this check will normally find nothing.
            // ============================================================

            const string checkExistingSql = @"
SELECT TOP 1
       EntryID
FROM TblAccount_JournalEntries
WHERE TransactionID = @TransactionID
  AND ExpenseID = @ExpenseID
  AND AccountID = @AccountID
  AND Description LIKE 'Whole Expense Discount - %';
";


            object existingEntryId;


            using (SqlCommand cmd = new SqlCommand(
                checkExistingSql,
                conn,
                tran))
            {
                cmd.Parameters.Add("@TransactionID", SqlDbType.Int)
                    .Value = transactionId;

                cmd.Parameters.Add("@ExpenseID", SqlDbType.Int)
                    .Value = expenseId;

                cmd.Parameters.Add("@AccountID", SqlDbType.Int)
                    .Value = purchaseDiscountAccountId;


                existingEntryId =
                    cmd.ExecuteScalar();
            }


            // ============================================================
            // IF EXISTING -> UPDATE
            //
            // This makes the method safe if it is called again.
            // ============================================================

            if (existingEntryId != null &&
                existingEntryId != DBNull.Value)
            {
                const string updateSql = @"
UPDATE TblAccount_JournalEntries
SET
    Debit = 0,
    Credit = @Credit,
    EntryDate = @EntryDate,
    TransactionDate = @EntryDate,
    SystemID = @SystemID,
    WarehouseID = @WarehouseID,
    DomainID = @DomainID,
    CreateByUserID = @CreateByUserID,
    Description = @Description,
    VATAmount = 0,
    DiscountAmount = @DiscountAmount,
    ExpenseID = @ExpenseID,
    ExpenseAmount = @ExpenseAmount,
    ItemTAX = 0,
    ClassNameID = @ClassNameID
WHERE EntryID = @EntryID;
";


                using (SqlCommand cmd = new SqlCommand(
                    updateSql,
                    conn,
                    tran))
                {
                    cmd.Parameters.Add("@EntryID", SqlDbType.Int)
                        .Value = Convert.ToInt32(existingEntryId);


                    SqlParameter creditParameter =
                        cmd.Parameters.Add(
                            "@Credit",
                            SqlDbType.Decimal
                        );

                    creditParameter.Precision = 18;
                    creditParameter.Scale = 2;
                    creditParameter.Value =
                        wholeExpenseDiscount;


                    cmd.Parameters.Add("@EntryDate", SqlDbType.DateTime)
                        .Value = entryDate;

                    cmd.Parameters.Add("@SystemID", SqlDbType.Int)
                        .Value = systemId;

                    cmd.Parameters.Add("@WarehouseID", SqlDbType.Int)
                        .Value = warehouseId;

                    cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar, 50)
                        .Value = domainId;

                    cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int)
                        .Value = createByUserId;

                    cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500)
                        .Value = description;


                    SqlParameter discountParameter =
                        cmd.Parameters.Add(
                            "@DiscountAmount",
                            SqlDbType.Decimal
                        );

                    discountParameter.Precision = 18;
                    discountParameter.Scale = 2;
                    discountParameter.Value =
                        wholeExpenseDiscount;


                    cmd.Parameters.Add("@ExpenseID", SqlDbType.Int)
                        .Value = expenseId;


                    SqlParameter expenseAmountParameter =
                        cmd.Parameters.Add(
                            "@ExpenseAmount",
                            SqlDbType.Decimal
                        );

                    expenseAmountParameter.Precision = 18;
                    expenseAmountParameter.Scale = 2;
                    expenseAmountParameter.Value =
                        wholeExpenseDiscount;


                    cmd.Parameters.Add("@ClassNameID", SqlDbType.Int)
                        .Value = classNameId;


                    cmd.ExecuteNonQuery();
                }


                return;
            }


            // ============================================================
            // INSERT WHOLE EXPENSE DISCOUNT JOURNAL
            //
            // ACCOUNTING:
            //
            //      DR = 0
            //      CR = Whole Expense Discount
            //
            // Example:
            //
            //      Whole Expense Discount = $100
            //
            //      Purchase Discount
            //          Debit  = 0
            //          Credit = 100
            //
            // This is correct because the other expense accounts already
            // contain the original expense amounts.
            // ============================================================

            const string insertSql = @"
        DECLARE @DiscountAccountNo NVARCHAR(50), @DiscountAccountName NVARCHAR(50)

        SELECT TOP 1
            @DiscountAccountNo = AccountNo,
            @DiscountAccountName = AccountName
        FROM TblChartOfAccount
        WHERE ChartOfAccountID = @AccountID;


INSERT INTO TblAccount_JournalEntries
(
    TransactionID,
    AccountID,
    Debit,
    Credit,
    EntryDate,
    TransactionDate,
    SystemID,
    WarehouseID,
    DomainID,
    CreateByUserID,
    CreateDate,
    Description,
    VATAmount,
    DiscountAmount,
    ExpenseID,
    ExpenseAmount,
    ItemTAX,
    ClassNameID, Type, InvoiceNo, JournalNo, ExpenseNo, ExpenseTransactionType, Account
)
VALUES
(
    @TransactionID,
    @AccountID,
    0,
    @Credit,
    @EntryDate,
    @EntryDate,
    @SystemID,
    @WarehouseID,
    @DomainID,
    @CreateByUserID,
    GETDATE(),
    @Description,
    0,
    @DiscountAmount,
    @ExpenseID,
    @ExpenseAmount,
    0,
    @ClassNameID, @Type, @ExpenseNo, @JournalNo, @ExpenseNo, @ExpenseTransactionType,
    @ExpenseTransactionType + '- ' + @DiscountAccountNo + '-' + @DiscountAccountName
);
";


            using (SqlCommand cmd = new SqlCommand(
                insertSql,
                conn,
                tran))
            {
                // --------------------------------------------------------
                // IDs
                // --------------------------------------------------------

                cmd.Parameters.Add("@TransactionID", SqlDbType.Int)
                    .Value = transactionId;

                cmd.Parameters.Add("@AccountID", SqlDbType.Int)
                    .Value = purchaseDiscountAccountId;


                // --------------------------------------------------------
                // Credit
                // --------------------------------------------------------

                SqlParameter creditParameter =
                    cmd.Parameters.Add(
                        "@Credit",
                        SqlDbType.Decimal
                    );

                creditParameter.Precision = 18;
                creditParameter.Scale = 2;
                creditParameter.Value =
                    wholeExpenseDiscount;


                // --------------------------------------------------------
                // Dates
                // --------------------------------------------------------

                cmd.Parameters.Add("@EntryDate", SqlDbType.DateTime)
                    .Value = entryDate;


                // --------------------------------------------------------
                // System information
                // --------------------------------------------------------

                cmd.Parameters.Add("@SystemID", SqlDbType.Int)
                    .Value = systemId;

                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int)
                    .Value = warehouseId;

                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar, 50)
                    .Value = domainId;
                cmd.Parameters.Add("@ExpenseTransactionType", SqlDbType.NVarChar, 50)
                    .Value = expense.ExpenseTransactionType;

                cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int)
                    .Value = createByUserId;

                cmd.Parameters.Add("@ExpenseNo", SqlDbType.NVarChar, 500).Value = expense.ExpenseNo;
                cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 500).Value = expense.Type;
                cmd.Parameters.Add("@JournalNo", SqlDbType.NVarChar, 500)
                    .Value = expensePostingResult.JournalNo;
                // --------------------------------------------------------
                // Description
                // --------------------------------------------------------

                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500)
                    .Value = description;


                // --------------------------------------------------------
                // Discount Amount
                // --------------------------------------------------------

                SqlParameter discountParameter =
                    cmd.Parameters.Add(
                        "@DiscountAmount",
                        SqlDbType.Decimal
                    );

                discountParameter.Precision = 18;
                discountParameter.Scale = 2;
                discountParameter.Value =
                    wholeExpenseDiscount;


                // --------------------------------------------------------
                // Expense
                // --------------------------------------------------------

                cmd.Parameters.Add("@ExpenseID", SqlDbType.Int)
                    .Value = expenseId;


                SqlParameter expenseAmountParameter =
                    cmd.Parameters.Add(
                        "@ExpenseAmount",
                        SqlDbType.Decimal
                    );

                expenseAmountParameter.Precision = 18;
                expenseAmountParameter.Scale = 2;
                expenseAmountParameter.Value =
                    wholeExpenseDiscount;


                // --------------------------------------------------------
                // Tax / Class
                // --------------------------------------------------------

                cmd.Parameters.Add("@ClassNameID", SqlDbType.Int)
                    .Value = classNameId;


                // --------------------------------------------------------
                // EXECUTE
                // --------------------------------------------------------

                cmd.ExecuteNonQuery();
            }
        }


        private ExpensePostingResult UpdateExpenseHeader(
    Expense e,
    SqlConnection conn,
    SqlTransaction tran)
        {
            HTML html = new HTML();

            string sql = @"
DECLARE
    @ExpenseID int,
    @TransactionID int,
    @APAccountID int,
    @AccountNo nvarchar(50),
    @AccountName nvarchar(50),
    @PaymentAccountNo nvarchar(50),
    @PaymentAccountName nvarchar(50),
    @VendorID int;

SET @ExpenseID = @InputExpenseID;

------------------------------------------------------------
-- 1. GET EXISTING EXPENSE
------------------------------------------------------------

IF NOT EXISTS
(
    SELECT 1
    FROM Expenses
    WHERE ExpenseID = @ExpenseID
)
BEGIN
    THROW 50001, 'ExpenseID does not exist.', 1;
END;


------------------------------------------------------------
-- 2. GET EXISTING TRANSACTION
------------------------------------------------------------

SELECT TOP 1
    @TransactionID = TransactionID
FROM Expenses
WHERE ExpenseID = @ExpenseID;


------------------------------------------------------------
-- 3. GET PAYEE / VENDOR
------------------------------------------------------------

SELECT
    @VendorID = PayeeID
FROM Expenses
WHERE ExpenseID = @ExpenseID;


------------------------------------------------------------
-- 4. GET PAYMENT ACCOUNT
------------------------------------------------------------

SELECT
    @PaymentAccountNo = AccountNo,
    @PaymentAccountName = AccountName
FROM TblChartOfAccount
WHERE ChartOfAccountID = @PaymentAccountID;


------------------------------------------------------------
-- 5. UPDATE EXPENSE HEADER
------------------------------------------------------------

UPDATE Expenses
SET
    ClassName = @ClassName,
    PaymentMethodID = @PaymentMethodID,
    PayeeID = @PayeeID,
    PaymentAccountID = @PaymentAccountID,

    PaymentDate = @PaymentDate,
    PaidOnDate = @PaidOnDate,

    CheckedBy = @CheckedBy,
    CheckedByTelegramUserName = @CheckedByTelegramUserName,
    CheckedByUserID = @CheckedByUserID,

    VerifiedBy = @VerifiedBy,
    VerifiedByTelegramUserName = @VerifiedByTelegramUserName,
    VerifiedByUserID = @VerifiedByUserID,

    ApprovedBy = @ApprovedBy,
    ApprovedByTelegramUserName = @ApprovedByTelegramUserName,
    ApprovedByUserID = @ApprovedByUserID,

    PostingNumber = @PostingNumber,
    PostingPrefix = @PostingPrefix,

    Reference = @Reference,
    BankInfo = @BankInfo,
    BankerName = @BankerName,
    BankAccountName = @BankAccountName,
    BankAccountNo = @BankAccountNo,
    ChequeTo = @ChequeTo,
    ChequeNo = @ChequeNo,

    Total = @Total,
    Status = @Status,

    PostedBy = @PostedBy,
    PostedByUserID = @PostedByUserID,
    PostedByTelegramUserName = @PostedByTelegramUserName,

    ChequeBankerName = @ChequeBankerName,
    ExchangeRate = @ExchangeRate,
    FileName = @FileName,

    PaymentTeam = @PaymentTermsID,
    TermName = @TermName,

    TotalAmount = @TotalAmount,
    PaidAmount = @PaidAmount,
    BalanceAmount = @BalanceAmount,
    PaymentStatus = @PaymentStatus,

    Description = @Description,

    GrantTotalAsRiel = @GrantTotalAsRiel,
    Discount = @Discount,
    DiscountItem = @DiscountItem,
    SubTotal = @SubTotal,
    VAT = @VAT
WHERE ExpenseID = @ExpenseID;


------------------------------------------------------------
-- 6. UPDATE PAYMENT TRANSACTION
------------------------------------------------------------

UPDATE dbo.Expense_PaymentTransactions
SET
    ClassName = @ClassName,
    PaymentMethodID = @PaymentMethodID,
    PayeeID = @PayeeID,
    PaymentAccountID = @PaymentAccountID,
    PaymentTeam = @PaymentTermsID,
    PaidOnDate = @PaidOnDate,

    IsChecked = @IsChecked,
    CheckedBy = @CheckedBy,
    CheckedByUserID = @CheckedByUserID,
    CheckedByTelegramUserName = @CheckedByTelegramUserName,

    IsVerified = @IsVerified,
    VerifiedBy = @VerifiedBy,
    VerifiedByUserID = @VerifiedByUserID,
    VerifiedByTelegramUserName = @VerifiedByTelegramUserName,

    Reference = @Reference,
    ExchangeRate = @ExchangeRate,
    Description = @Description,

    BankerName = @BankerName,
    BankAccountName = @BankAccountName,
    BankAccountNo = @BankAccountNo,

    ChequeBankerName = @ChequeBankerName,
    ChequeTo = @ChequeTo,
    ChequeNo = @ChequeNo,

    Status = @Status,

    FileName = @FileName,

    TotalAmount = @TotalAmount,
    PaidAmount = @PaidAmount,
    BalanceAmount = @BalanceAmount,

    PaymentStatus = @PaymentStatus,

    PostedByTelegramUserName = @PostedByTelegramUserName,
    PostedByUserID = @PostedByUserID,

    Total = @Total,
    TotalPaidAmount = @TotalPaidAmount,
    TotalBalanceAmount = @TotalBalanceAmount,

    PaymentDate = @PaymentDate,

    GrantTotalAsRiel = @GrantTotalAsRiel,
    Discount = @Discount,
    DiscountItem = @DiscountItem,
    SubTotal = @SubTotal,
    VAT = @VAT
WHERE ExpenseID = @ExpenseID;


------------------------------------------------------------
-- 7. UPDATE ACCOUNT TRANSACTION
------------------------------------------------------------

    UPDATE TblAccount_Transaction
    SET
        PayeeID = @PayeeID,
        TransactionTypeID = @TransactionTypeID,
        Amount = @TotalAmount,
        TransactionDate = @PaymentDate,

        CreateByUserID = @CreateByUserID,
        CreateDate = @CreateDate,

        [Description] = @Description,
        InvoiceDate = @PaymentDate,

        AccountID = @PaymentAccountID,

        VendorID = @VendorID,
        EmployeeID = @EmployeeID,
        CustomerID = @CustomerID,

        TransactionType = @TransactionType,
        ClassNameID = @ClassNameID,

    WHERE TransactionID = @TransactionID
      AND ExpenseID = @ExpenseID;

------------------------------------------------------------
-- 8. GET A/P ACCOUNT
------------------------------------------------------------

SELECT TOP 1
    @APAccountID = ChartOfAccountID,
    @AccountNo = AccountNo,
    @AccountName = AccountName
FROM TblChartOfAccount
WHERE NormalSide = 'Credit'
  AND ParentAccountID = 0
  AND
  (
        AccountName = 'Accounts Payable (A/P)'
        OR AccountName = 'Account Payable (A/P)'
        OR AccountName LIKE '%Accounts Payable%'
        OR AccountName LIKE '%Account Payable%'
        --OR AccountName LIKE '%Payable%'
  )
  AND SystemID = @SystemID
  AND WarehouseID = @WarehouseID
  AND DomainID = @DomainID;


------------------------------------------------------------
-- 9. UPDATE JOURNAL ENTRY
------------------------------------------------------------
    IF @Credit > 0
    BEGIN

        IF EXISTS
        (
            SELECT 1
            FROM TblAccount_JournalEntries
            WHERE TransactionID = @TransactionID
              AND ExpenseID = @ExpenseID
              AND Type = 'Expense'
        )
        BEGIN

            UPDATE TblAccount_JournalEntries
            SET
                AccountID = @APAccountID,
                Debit = 0,
                Credit = @Credit,

                EntryDate = @EntryDate,
                TransactionDate = @EntryDate,

                CreateByUserID = @CreateByUserID,
                Description = @AccountNo + '-' + @AccountName,

                VATAmount = 0,
                DiscountAmount = 0,

                ExpenseAmount = @ItemGrandTotal,
                ItemTAX = 0,

                ClassNameID = @ClassNameID,

                --PaymentNo = @PaymentNo,
                --ExpenseNo = @ExpenseNo,
                --InvoiceNo = @ExpenseNo

            WHERE TransactionID = @TransactionID
              AND ExpenseID = @ExpenseID
              AND Type = 'Expense';

        END
    END;


------------------------------------------------------------
-- 10. RESULT
------------------------------------------------------------

SELECT
    @ExpenseID AS ExpenseID,
    @TransactionID AS TransactionID,
    @PaymentAccountID AS PaymentAccountID,
    @APAccountID AS APAccountID,
    --@PaymentNo AS PaymentNo,
    --@ExpenseNo AS ExpenseNo,
    @AccountNo AS APAccountNo,
    @AccountName AS APAccountName,
    @PaymentAccountName AS PaymentAccountName,
    @PaymentAccountNo AS PaymentAccountNo,
    @PayeeID AS PayeeID;
";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                // =========================================================
                // 1. CALCULATE SUBTOTAL
                // =========================================================

                decimal subtotal = e.Details?
                    .Sum(x => x.ItemGrandTotal) ?? 0m;

                decimal VAT = e.Details?
                    .Sum(x => x.ItemVAT) ?? 0m;

                decimal DiscountItem = e.Details?
                    .Sum(x => x.DisUSD) ?? 0m;

                if (subtotal < 0)
                    subtotal = 0m;


                // =========================================================
                // 2. WHOLE EXPENSE DISCOUNT
                // =========================================================

                decimal discount = e.Discount + DiscountItem;

                if (discount < 0)
                    discount = 0m;

                if (discount > subtotal)
                    discount = subtotal;

                e.Discount = discount;


                // =========================================================
                // 3. FINAL TOTAL
                // =========================================================

                decimal finalTotal = subtotal - discount;

                if (finalTotal < 0)
                    finalTotal = 0m;

                e.TotalAmount = finalTotal;


                // =========================================================
                // 4. ORIGINAL SUBTOTAL
                // =========================================================

                e.Total = subtotal;


                // =========================================================
                // 5. PAID AMOUNT
                // =========================================================

                decimal paidAmount = e.PaidAmount;

                if (paidAmount < 0)
                    paidAmount = 0m;

                if (paidAmount > e.TotalAmount)
                    paidAmount = e.TotalAmount;

                e.PaidAmount = paidAmount;


                // =========================================================
                // 6. BALANCE
                // =========================================================

                decimal balanceAmount =
                    e.TotalAmount - e.PaidAmount;

                if (balanceAmount < 0)
                    balanceAmount = 0m;

                e.BalanceAmount = balanceAmount;


                // =========================================================
                // 7. PAYMENT STATUS
                // =========================================================

                if (e.PaidAmount <= 0)
                {
                    e.PaymentStatus = "Unpaid";
                }
                else if (e.PaidAmount < e.TotalAmount)
                {
                    e.PaymentStatus = "Partial";
                }
                else
                {
                    e.PaymentStatus = "Paid";
                }


                // =========================================================
                // 8. PARAMETERS
                // =========================================================

                cmd.Parameters.Add("@InputExpenseID", SqlDbType.Int)
                    .Value = e.ExpenseID;

                cmd.Parameters.Add("@Credit", SqlDbType.Decimal)
                    .Value = e.TotalAmount;

                cmd.Parameters.Add("@ItemGrandTotal", SqlDbType.Decimal)
                    .Value = e.TotalAmount;

                cmd.Parameters.Add("@EntryDate", SqlDbType.DateTime)
                    .Value = e.PaymentDate;

                cmd.Parameters.Add("@ClassName", SqlDbType.Int)
                    .Value = e.ClassName;

                cmd.Parameters.Add("@ClassNameID", SqlDbType.Int)
                    .Value = e.ClassName;

                cmd.Parameters.Add("@PaymentMethodID", SqlDbType.Int)
                    .Value = e.PaymentMethodID;

                cmd.Parameters.Add("@No", SqlDbType.NVarChar)
                    .Value = e.No ?? "";

                cmd.Parameters.Add("@NoPrefix", SqlDbType.NVarChar)
                    .Value = e.NoPrefix ?? "";

                //cmd.Parameters.Add("@PVNo", SqlDbType.NVarChar)
                //    .Value = e.PVNo ?? "";

                cmd.Parameters.Add("@PVNoPrefix", SqlDbType.NVarChar)
                    .Value = e.PVNoPrefix ?? "";

                cmd.Parameters.Add("@ExpenseNo", SqlDbType.NVarChar)
                    .Value = e.ExpenseNo ?? "";

                cmd.Parameters.Add("@PaymentNo", SqlDbType.NVarChar)
                    .Value = e.PaymentNo ?? "";

                cmd.Parameters.Add("@Description", SqlDbType.NVarChar)
                    .Value = e.Description ?? "";

                cmd.Parameters.Add("@PaymentAccountID", SqlDbType.Int)
                    .Value = e.PaymentAccountID;

                cmd.Parameters.Add("@PaidOnDate", SqlDbType.DateTime)
                    .Value = e.PaidOnDate.HasValue
                        ? (object)e.PaidOnDate.Value
                        : DBNull.Value;

                cmd.Parameters.Add("@CheckedBy", SqlDbType.NVarChar)
                    .Value = e.CheckedBy ?? "";

                cmd.Parameters.Add("@CheckedByTelegramUserName", SqlDbType.NVarChar)
                    .Value = e.CheckedByTelegramUserName ?? "";

                cmd.Parameters.Add("@CheckedByUserID", SqlDbType.Int)
                    .Value = e.CheckedByUserID;

                cmd.Parameters.Add("@VerifiedBy", SqlDbType.NVarChar)
                    .Value = e.VerifiedBy ?? "";

                cmd.Parameters.Add("@VerifiedByTelegramUserName", SqlDbType.NVarChar)
                    .Value = e.VerifiedByTelegramUserName ?? "";

                cmd.Parameters.Add("@VerifiedByUserID", SqlDbType.Int)
                    .Value = e.VerifiedByUserID;

                cmd.Parameters.Add("@ApprovedBy", SqlDbType.NVarChar)
                    .Value = e.ApprovedBy ?? "";

                cmd.Parameters.Add("@ApprovedByTelegramUserName", SqlDbType.NVarChar)
                    .Value = e.ApprovedByTelegramUserName ?? "";

                cmd.Parameters.Add("@ApprovedByUserID", SqlDbType.Int)
                    .Value = e.ApprovedByUserID;

                cmd.Parameters.Add("@PostedBy", SqlDbType.NVarChar)
                    .Value = e.PostedBy ?? "";

                cmd.Parameters.Add("@PostedByTelegramUserName", SqlDbType.NVarChar)
                    .Value = e.PostedByTelegramUserName ?? "";

                cmd.Parameters.Add("@PostedByUserID", SqlDbType.Int)
                    .Value = e.PostedByUserID;

                cmd.Parameters.Add("@PostingNumber", SqlDbType.NVarChar)
                    .Value = e.PostingNumber ?? "";

                cmd.Parameters.Add("@PostingPrefix", SqlDbType.NVarChar)
                    .Value = e.PostingPrefix ?? "";

                cmd.Parameters.Add("@Reference", SqlDbType.NVarChar)
                    .Value = e.Reference ?? "";

                cmd.Parameters.Add("@BankInfo", SqlDbType.NVarChar)
                    .Value = e.BankInfo ?? "";

                cmd.Parameters.Add("@ChequeBankerName", SqlDbType.NVarChar)
                    .Value = e.ChequeBankerName ?? "";

                cmd.Parameters.Add("@BankerName", SqlDbType.NVarChar)
                    .Value = e.BankerName ?? "";

                cmd.Parameters.Add("@BankAccountName", SqlDbType.NVarChar)
                    .Value = e.BankAccountName ?? "";

                cmd.Parameters.Add("@BankAccountNo", SqlDbType.NVarChar)
                    .Value = e.BankAccountNo ?? "";

                cmd.Parameters.Add("@ChequeTo", SqlDbType.NVarChar)
                    .Value = e.ChequeTo ?? "";

                cmd.Parameters.Add("@ChequeNo", SqlDbType.NVarChar)
                    .Value = e.ChequeNo ?? "";

                cmd.Parameters.Add("@SystemID", SqlDbType.Int)
                    .Value = html.SystemID;

                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int)
                    .Value = html.WarehouseID;

                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar)
                    .Value = html.DomainID;

                cmd.Parameters.Add("@FileName", SqlDbType.NVarChar)
                    .Value = e.FileName ?? "";

                cmd.Parameters.Add("@VAT", SqlDbType.Decimal)
                    .Value = VAT;

                cmd.Parameters.Add("@SubTotal", SqlDbType.Decimal)
                    .Value = subtotal;

                cmd.Parameters.Add("@Discount", SqlDbType.Decimal)
                    .Value = e.Discount;

                cmd.Parameters.Add("@DiscountItem", SqlDbType.Decimal)
                    .Value = DiscountItem;

                cmd.Parameters.Add("@GrantTotalAsRiel", SqlDbType.Decimal)
                    .Value = e.GrantTotalAsRiel;

                cmd.Parameters.Add("@Total", SqlDbType.Decimal)
                    .Value = e.Total;

                cmd.Parameters.Add("@TotalPaidAmount", SqlDbType.Decimal)
                    .Value = e.PaidAmount;

                cmd.Parameters.Add("@TotalBalanceAmount", SqlDbType.Decimal)
                    .Value = e.BalanceAmount;

                cmd.Parameters.Add("@ExchangeRate", SqlDbType.Decimal)
                    .Value = e.ExchangeRate;

                cmd.Parameters.Add("@Status", SqlDbType.NVarChar)
                    .Value = e.Status;

                cmd.Parameters.Add("@Type", SqlDbType.NVarChar)
                    .Value = "Expense";

                cmd.Parameters.Add("@TransactionTypeID", SqlDbType.Int)
                    .Value = e.PaymentMethodID;

                cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int)
                    .Value = html.UserID;

                cmd.Parameters.Add("@CreateDate", SqlDbType.DateTime)
                    .Value = html.GetCurrentDate();

                //cmd.Parameters.Add("@VendorID", SqlDbType.Int)
                //    .Value = e.VendorID;

                cmd.Parameters.Add("@EmployeeID", SqlDbType.Int)
                    .Value = e.EmployeeID;

                cmd.Parameters.Add("@CustomerID", SqlDbType.Int)
                    .Value = e.CustomerID;

                cmd.Parameters.Add("@PayeeID", SqlDbType.Int)
                    .Value = e.PayeeID;

                cmd.Parameters.Add("@TransactionType", SqlDbType.NVarChar)
                    .Value = e.TransactionType ?? "";

                cmd.Parameters.Add("@PaymentTermsID", SqlDbType.Int)
                    .Value = e.PaymentTermsID.HasValue
                        ? (object)e.PaymentTermsID.Value
                        : DBNull.Value;

                cmd.Parameters.Add("@TermName", SqlDbType.NVarChar)
                    .Value = e.TermName ?? "";

                cmd.Parameters.Add("@TotalAmount", SqlDbType.Decimal)
                    .Value = e.TotalAmount;

                cmd.Parameters.Add("@PaidAmount", SqlDbType.Decimal)
                    .Value = e.PaidAmount;

                cmd.Parameters.Add("@BalanceAmount", SqlDbType.Decimal)
                    .Value = e.BalanceAmount;

                cmd.Parameters.Add("@PaymentStatus", SqlDbType.NVarChar)
                    .Value = e.PaymentStatus;

                cmd.Parameters.Add("@PVNo", SqlDbType.NVarChar)
                    .Value = e.PVNo ?? "";

                cmd.Parameters.Add("@IsChecked", SqlDbType.Bit)
                    .Value = 0;

                cmd.Parameters.Add("@IsVerified", SqlDbType.Bit)
                    .Value = 0;

                cmd.Parameters.Add("@PaymentDate", SqlDbType.DateTime)
                    .Value = e.PaymentDate;
                // =========================================================
                // 9. EXECUTE
                // =========================================================

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new ExpensePostingResult
                        {
                            ExpenseID = Convert.ToInt32(reader["ExpenseID"]),
                            PayeeID = Convert.ToInt32(reader["PayeeID"]),
                            TransactionID = Convert.ToInt32(reader["TransactionID"]),

                            PaymentAccountID =
                                reader["PaymentAccountID"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(reader["PaymentAccountID"]),

                            APAccountID =
                                reader["APAccountID"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(reader["APAccountID"]),

                            PaymentNo = reader["PaymentNo"].ToString(),
                            ExpenseNo = reader["ExpenseNo"].ToString(),
                            //JournalNo = reader["JournalNo"].ToString(),

                            APAccountNo = reader["APAccountNo"].ToString(),
                            APAccountName = reader["APAccountName"].ToString(),

                            PaymentAccountNo =
                                reader["PaymentAccountNo"].ToString(),

                            PaymentAccountName =
                                reader["PaymentAccountName"].ToString(),

                            ExpenseAmount = e.TotalAmount
                        };
                    }
                }

                throw new Exception(
                    "Failed to update expense. ExpenseID = " +
                    e.ExpenseID);
            }
        }

        private void InsertExpenseDetailForUpdate(
    Expense e,
    ExpenseDetail d,
    SqlConnection conn,
    SqlTransaction tran)
        {
            // Get existing TransactionID + APAccountID
            string fetchSql = @"
SELECT t.TransactionID, t.PaymentNo, t.ExpenseNo,
       (SELECT TOP 1 ChartOfAccountID
FROM TblChartOfAccount
WHERE NormalSide = 'Credit'
  AND ISNULL(ParentAccountID, 0) = 0
  AND (
        AccountName = 'Accounts Payable (A/P)'
        OR AccountName = 'Account Payable (A/P)'
        OR AccountName LIKE '%Accounts Payable%'
        OR AccountName LIKE '%Account Payable%'
        OR AccountName LIKE '%Payable%'
      )
  AND SystemID = @SystemID
  AND WarehouseID = @WarehouseID
  AND DomainID = @DomainID) AS APAccountID
FROM TblAccount_Transaction t
WHERE t.ExpenseID = @ExpenseID";

            int transactionId = 0;
            int apAccountId = 0, PaymentAccountID = 0;
            string PaymentNo = "", ExpenseNo = "";
            HTML html = new HTML();
            using (SqlCommand fetchCmd = new SqlCommand(fetchSql, conn, tran))
            {
                fetchCmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = e.ExpenseID;
                fetchCmd.Parameters.Add("@SystemID", SqlDbType.Int).Value = html.SystemID;
                fetchCmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value = html.WarehouseID;
                fetchCmd.Parameters.Add("@DomainID", SqlDbType.Int).Value = html.DomainID;

                using (var reader = fetchCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        transactionId = Convert.ToInt32(reader["TransactionID"]);
                        ExpenseNo = reader["ExpenseNo"].ToString();
                        PaymentNo = reader["PaymentNo"].ToString();
                        PaymentAccountID = e.PaymentAccountID;
                        apAccountId = reader["APAccountID"] == DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(reader["APAccountID"]);
                        //apAccountId = reader["APAccountID"] == DBNull.Value
                        //                ? 0
                        //                : Convert.ToInt32(reader["APAccountID"]);
                    }
                }
            }

            ExpensePostingResult result = new ExpensePostingResult
            {
                ExpenseID = e.ExpenseID,
                TransactionID = transactionId,
                APAccountID = apAccountId,
                PaymentAccountID = PaymentAccountID,
                ClassNameID = e.ClassName,
                PaymentNo = PaymentNo,
                ExpenseNo = ExpenseNo
            };

            InsertExpenseDetail(e, result, d, transactionId, conn, tran);
        }

        private void UpdateExpenseDetail(
    Expense e,
    ExpensePostingResult expensePostingResult,
    ExpenseDetail d,
    int TransactionID,
    SqlConnection conn,
    SqlTransaction tran)
        {
            HTML html = new HTML();

            // =========================================================
            // 1. BASIC VALUES
            // =========================================================

            int expenseID = expensePostingResult.ExpenseID;

            int transactionID =
                expensePostingResult.TransactionID > 0
                    ? expensePostingResult.TransactionID
                    : TransactionID;

            int paymentAccountID =
                expensePostingResult.PaymentAccountID;

            int classNameID =
                expensePostingResult.ClassNameID;


            // =========================================================
            // 2. ACCOUNT DESCRIPTION
            // =========================================================

            string expenseCategoryAccName =
                (d.AccountName ?? "") +
                " - " +
                (d.ItemName ?? "");


            // =========================================================
            // 3. FULL ITEM AMOUNT
            //
            // IMPORTANT:
            //
            // We DO NOT calculate:
            //     paymentRatio
            //     paidPortion
            //     payablePortion
            //
            // The detail posts using its FULL amount.
            // =========================================================

            decimal itemTotal =
                Math.Round(
                    Math.Max(0m, d.Total),
                    2,
                    MidpointRounding.AwayFromZero
                );

            decimal itemDiscount =
                Math.Round(
                    Math.Max(0m, d.DisUSD),
                    2,
                    MidpointRounding.AwayFromZero
                );

            decimal itemGrandTotal =
                Math.Round(
                    Math.Max(0m, d.ItemGrandTotal),
                    2,
                    MidpointRounding.AwayFromZero
                );

            decimal itemTaxPercent =
                Math.Max(0m, d.ItemTAX);


            // =========================================================
            // 4. VAT
            //
            // Keep the same calculation from your existing code:
            //
            // VAT = Total × TAX%
            // =========================================================

            decimal vatAmount =
                Math.Round(
                    (itemTotal * itemTaxPercent) / 100m,
                    2,
                    MidpointRounding.AwayFromZero
                );

            if (vatAmount < 0m)
                vatAmount = 0m;


            // =========================================================
            // 5. GET PURCHASE DISCOUNT ACCOUNT
            // =========================================================

            int purchaseDiscountAccountID = 0;

            if (itemDiscount > 0)
            {
                string purchaseDiscountSql = @"
        SELECT TOP 1
            ChartOfAccountID
        FROM TblChartOfAccount
        WHERE (
            LTRIM(RTRIM(AccountName)) = 'Purchase Discount'
            OR LTRIM(RTRIM(AccountName)) = 'Purchase Discounts'
            OR AccountName LIKE '%Purchase Discount%'
        )
          AND SystemID = @SystemID
          AND (WarehouseID = @WarehouseID OR ISNULL(WarehouseID, 0) = 0)
        ORDER BY
            CASE WHEN WarehouseID = @WarehouseID THEN 0 ELSE 1 END,
            CASE WHEN LTRIM(RTRIM(AccountName)) = 'Purchase Discount' THEN 0 ELSE 1 END;
        ";

                using (SqlCommand discountCmd =
                    new SqlCommand(
                        purchaseDiscountSql,
                        conn,
                        tran))
                {
                    discountCmd.Parameters.Add(
                        "@DomainID",
                        SqlDbType.NVarChar
                    ).Value =
                        html.DomainID;

                    discountCmd.Parameters.Add(
                        "@SystemID",
                        SqlDbType.Int
                    ).Value =
                        html.SystemID;

                    discountCmd.Parameters.Add(
                        "@WarehouseID",
                        SqlDbType.Int
                    ).Value =
                        html.WarehouseID;

                    object result =
                        discountCmd.ExecuteScalar();

                    if (result == null ||
                        result == DBNull.Value)
                    {
                        throw new Exception(
                            "Purchase Discount account was not found in Chart of Accounts."
                        );
                    }

                    purchaseDiscountAccountID =
                        Convert.ToInt32(result);
                }
            }


            // =========================================================
            // 6. SQL
            // =========================================================

            string sql = @"

    ------------------------------------------------------------
    -- VARIABLES
    ------------------------------------------------------------

    DECLARE @VATAccountID INT;
    DECLARE @APAccountID INT;

    DECLARE
        @AccountExpenseNo NVARCHAR(50),
        @ExpenseCategoryName NVARCHAR(MAX);


    ------------------------------------------------------------
    -- 1. VAT INPUT ACCOUNT
    ------------------------------------------------------------

    SELECT TOP 1
        @VATAccountID = ChartOfAccountID
    FROM TblChartOfAccount
    WHERE AccountName = 'VAT Input'
      AND DomainID = @DomainID
      AND SystemID = @SystemID
      AND WarehouseID = @WarehouseID;


    ------------------------------------------------------------
    -- 2. ACCOUNTS PAYABLE
    ------------------------------------------------------------

    SELECT TOP 1
        @APAccountID = ChartOfAccountID
    FROM TblChartOfAccount
    WHERE NormalSide = 'Credit'
      AND ParentAccountID = 0
      AND
      (
            AccountName = 'Accounts Payable (A/P)'
         OR AccountName = 'Account Payable (A/P)'
         OR AccountName LIKE '%Accounts Payable%'
         OR AccountName LIKE '%Account Payable%'
         OR AccountName LIKE '%Payable%'
      )
      AND SystemID = @SystemID
      AND WarehouseID = @WarehouseID
      AND DomainID = @DomainID
    ORDER BY
        CASE
            WHEN AccountName = 'Accounts Payable (A/P)' THEN 1
            WHEN AccountName = 'Account Payable (A/P)' THEN 2
            WHEN AccountName LIKE '%Accounts Payable%' THEN 3
            WHEN AccountName LIKE '%Account Payable%' THEN 4
            WHEN AccountName LIKE '%Payable%' THEN 5
            ELSE 6
        END;


    IF @APAccountID IS NULL
    BEGIN
        THROW 50001,
            'Accounts Payable (A/P) account was not found.',
            1;
    END;


    ------------------------------------------------------------
    -- 3. CHECK EXPENSE
    ------------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM Expenses
        WHERE ExpenseID = @ExpenseID
          AND SystemID = @SystemID
          AND WarehouseID = @WarehouseID
          AND DomainID = @DomainID
    )
    BEGIN
        THROW 50002,
            'ExpenseID does not exist.',
            1;
    END;


    ------------------------------------------------------------
    -- 4. UPDATE EXPENSE DETAIL
    --
    -- IMPORTANT:
    -- This is UPDATE, NOT INSERT.
    --
    -- Existing:
    -- ExpenseID = 100
    -- ExpenseDetailID = 5
    --
    -- We update exactly that detail.
    ------------------------------------------------------------

    UPDATE ExpenseDetails
    SET
        ItemTypeID = @ItemTypeID,
        ExpenseCategory = @ExpenseCategory,
        ItemID = @ItemID,
        ItemName = @ItemName,
        ItemName_Description = @ItemName_Description,
        UM = @UM,
        QTY = @QTY,
        UnitPrice = @UnitPrice,
        DisUSD = @DisUSD,
        Total = @Total,
        ItemTAX = @ItemTAX,
        ItemGrandTotal = @ItemGrandTotal,
        RefNo = @RefNo,
        SourceType = @SourceType
    WHERE ExpenseDetailID = @ExpenseDetailID
      AND ExpenseID = @ExpenseID;


    IF @@ROWCOUNT = 0
    BEGIN
        THROW 50003,
            'ExpenseDetailID does not belong to this ExpenseID.',
            1;
    END;


    ------------------------------------------------------------
    -- 5. GET EXPENSE ACCOUNT
    ------------------------------------------------------------

    SELECT TOP 1
        @AccountExpenseNo = AccountNo,
        @ExpenseCategoryName = AccountName
    FROM TblChartOfAccount
    WHERE ChartOfAccountID = @ExpenseCategory
      AND DomainID = @DomainID
      AND SystemID = @SystemID
      AND WarehouseID = @WarehouseID;


    ------------------------------------------------------------
    -- 6. DELETE OLD JOURNAL ENTRIES
    --
    -- We are updating this detail.
    --
    -- First remove its old GL.
    -- Then insert the NEW GL using the new amount.
    ------------------------------------------------------------

    DELETE FROM TblAccount_JournalEntries
    WHERE ExpenseID = @ExpenseID
      AND TransactionID = @TransactionID
      AND ExpenseDetailID = @ExpenseDetailID
      AND Type = 'Expense'
      AND SystemID = @SystemID
      AND WarehouseID = @WarehouseID
      AND DomainID = @DomainID;


    ------------------------------------------------------------
    -- 7. VENDOR / SUPPLIER DEPOSIT
    --
    -- FULL AMOUNT
    ------------------------------------------------------------

    IF
    (
           @ExpenseCategoryName = 'Vendor Deposit'
        OR @ExpenseCategoryName = 'Supplier Deposit'
        OR @ExpenseCategoryName = 'Vendor Advance'
        OR @ExpenseCategoryName = 'Supplier Advance'
    )
    BEGIN

        --------------------------------------------------------
        -- DR Vendor / Supplier Deposit
        --
        -- FULL ITEM GRAND TOTAL
        --------------------------------------------------------

        INSERT INTO TblAccount_JournalEntries
        (
            TransactionID,
            AccountID,
            Debit,
            Credit,
            EntryDate,
            TransactionDate,
            SystemID,
            WarehouseID,
            DomainID,
            CreateByUserID,
            CreateDate,
            Description,
            VATAmount,
            DiscountAmount,
            ExpenseID,
            ExpenseDetailID,
            ExpenseAmount,
            ItemTAX,
            ClassNameID,
            Type,
            PaymentNo,
            ExpenseNo,
            JournalNo,
            InvoiceNo
        )
        VALUES
        (
            @TransactionID,
            @ExpenseCategory,

            @ItemGrandTotal,
            0,

            @EntryDate,
            @EntryDate,

            @SystemID,
            @WarehouseID,
            @DomainID,

            @CreateByUserID,
            GETDATE(),

            @ExpenseCategoryName
                + ' - '
                + @ItemName,

            @VATAmount,
            @DisUSD,

            @ExpenseID,
            @ExpenseDetailID,

            @ItemGrandTotal,

            @ItemTAX,
            @ClassNameID,

            'Expense',

            @PaymentNo,
            @ExpenseNo,
            @JournalNo,
            @ExpenseNo
        );


        --------------------------------------------------------
        -- CR CASH / BANK
        --
        -- FULL AMOUNT
        --------------------------------------------------------

        IF @ItemGrandTotal > 0
        BEGIN

            INSERT INTO TblAccount_JournalEntries
            (
                TransactionID,
                AccountID,
                Debit,
                Credit,
                EntryDate,
                TransactionDate,
                SystemID,
                WarehouseID,
                DomainID,
                CreateByUserID,
                CreateDate,
                Description,
                VATAmount,
                DiscountAmount,
                ExpenseID,
                ExpenseDetailID,
                ExpenseAmount,
                ItemTAX,
                ClassNameID,
                Type,
                PaymentNo,
                ExpenseNo,
                JournalNo,
                InvoiceNo
            )
            VALUES
            (
                @TransactionID,
                @PaymentAccountID,

                0,
                @ItemGrandTotal,

                @EntryDate,
                @EntryDate,

                @SystemID,
                @WarehouseID,
                @DomainID,

                @CreateByUserID,
                GETDATE(),

                'Paid '
                    + @ExpenseCategoryName
                    + ' - '
                    + @ItemName,

                0,
                @DisUSD,

                @ExpenseID,
                @ExpenseDetailID,

                @ItemGrandTotal,

                0,
                @ClassNameID,

                'Expense',

                @PaymentNo,
                @ExpenseNo,
                @JournalNo,
                @ExpenseNo
            );

        END;

    END
    ELSE
    BEGIN

        --------------------------------------------------------
        -- 8. EXPENSE DEBIT
        --
        -- FULL AMOUNT
        --
        -- DR Expense = ItemGrandTotal
        --------------------------------------------------------

        INSERT INTO TblAccount_JournalEntries
        (
            TransactionID,
            AccountID,
            Debit,
            Credit,
            EntryDate,
            TransactionDate,
            SystemID,
            WarehouseID,
            DomainID,
            CreateByUserID,
            CreateDate,
            Description,
            VATAmount,
            DiscountAmount,
            ExpenseID,
            ExpenseDetailID,
            ExpenseAmount,
            ItemTAX,
            ClassNameID,
            PaymentNo,
            ExpenseNo,
            Type,
            JournalNo,
            InvoiceNo
        )
        VALUES
        (
            @TransactionID,

            @ExpenseCategory,

            @ItemGrandTotal,
            0,

            @EntryDate,
            @EntryDate,

            @SystemID,
            @WarehouseID,
            @DomainID,

            @CreateByUserID,
            GETDATE(),

            @AccountExpenseNo
                + '-'
                + @expenseCategoryAccName,

            @VATAmount,
            @DisUSD,

            @ExpenseID,
            @ExpenseDetailID,

            @ItemGrandTotal,

            @ItemTAX,
            @ClassNameID,

            @PaymentNo,
            @ExpenseNo,

            'Expense',

            @JournalNo,
            @ExpenseNo
        );


        --------------------------------------------------------
        -- 9. PURCHASE DISCOUNT
        --
        -- CR Purchase Discount
        --
        -- FULL ITEM DISCOUNT
        --------------------------------------------------------

        IF @ItemDiscountAmount > 0
        BEGIN

            INSERT INTO TblAccount_JournalEntries
            (
                TransactionID,
                AccountID,
                Debit,
                Credit,
                EntryDate,
                TransactionDate,
                SystemID,
                WarehouseID,
                DomainID,
                CreateByUserID,
                CreateDate,
                Description,
                VATAmount,
                DiscountAmount,
                ExpenseID,
                ExpenseDetailID,
                ExpenseAmount,
                ItemTAX,
                ClassNameID,
                PaymentNo,
                ExpenseNo,
                Type,
                JournalNo,
                InvoiceNo
            )
            VALUES
            (
                @TransactionID,

                @PurchaseDiscountAccountID,

                0,
                @ItemDiscountAmount,

                @EntryDate,
                @EntryDate,

                @SystemID,
                @WarehouseID,
                @DomainID,

                @CreateByUserID,
                GETDATE(),

                'Purchase Discount - '
                    + @ItemName,

                0,
                @ItemDiscountAmount,

                @ExpenseID,
                @ExpenseDetailID,

                @ItemDiscountAmount,

                0,
                @ClassNameID,

                @PaymentNo,
                @ExpenseNo,

                'Expense',

                @JournalNo,
                @ExpenseNo
            );

        END;


        --------------------------------------------------------
        -- 10. CASH / BANK
        --
        -- FULL AMOUNT
        --
        -- CR Cash / Bank = ItemGrandTotal
        --------------------------------------------------------

        IF @PaymentAccountID IS NOT NULL
           AND @PaymentAccountID > 0
           AND @ItemGrandTotal > 0
        BEGIN

            INSERT INTO TblAccount_JournalEntries
            (
                TransactionID,
                AccountID,
                Debit,
                Credit,
                EntryDate,
                TransactionDate,
                SystemID,
                WarehouseID,
                DomainID,
                CreateByUserID,
                CreateDate,
                Description,
                VATAmount,
                DiscountAmount,
                ExpenseID,
                ExpenseDetailID,
                ExpenseAmount,
                ItemTAX,
                ClassNameID,
                PaymentNo,
                ExpenseNo,
                Type,
                JournalNo,
                InvoiceNo
            )
            VALUES
            (
                @TransactionID,

                @PaymentAccountID,

                0,
                @ItemGrandTotal,

                @EntryDate,
                @EntryDate,

                @SystemID,
                @WarehouseID,
                @DomainID,

                @CreateByUserID,
                GETDATE(),

                'Paid '
                    + @AccountExpenseNo
                    + '-'
                    + @expenseCategoryAccName,

                0,
                0,

                @ExpenseID,
                @ExpenseDetailID,

                @ItemGrandTotal,

                0,
                @ClassNameID,

                @PaymentNo,
                @ExpenseNo,

                'Expense',

                @JournalNo,
                @ExpenseNo
            );

        END
        ELSE
        BEGIN

            ----------------------------------------------------
            -- 11. ACCOUNTS PAYABLE
            --
            -- FULL AMOUNT
            --
            -- CR A/P = ItemGrandTotal
            ----------------------------------------------------

            INSERT INTO TblAccount_JournalEntries
            (
                TransactionID,
                AccountID,
                Debit,
                Credit,
                EntryDate,
                TransactionDate,
                SystemID,
                WarehouseID,
                DomainID,
                CreateByUserID,
                CreateDate,
                Description,
                VATAmount,
                DiscountAmount,
                ExpenseID,
                ExpenseDetailID,
                ExpenseAmount,
                ItemTAX,
                ClassNameID,
                PaymentNo,
                ExpenseNo,
                Type,
                JournalNo,
                InvoiceNo
            )
            VALUES
            (
                @TransactionID,

                @APAccountID,

                0,
                @ItemGrandTotal,

                @EntryDate,
                @EntryDate,

                @SystemID,
                @WarehouseID,
                @DomainID,

                @CreateByUserID,
                GETDATE(),

                'Payable '
                    + @AccountExpenseNo
                    + '-'
                    + @expenseCategoryAccName,

                0,
                0,

                @ExpenseID,
                @ExpenseDetailID,

                @ItemGrandTotal,

                0,
                @ClassNameID,

                @PaymentNo,
                @ExpenseNo,

                'Expense',

                @JournalNo,
                @ExpenseNo
            );

        END;


        --------------------------------------------------------
        -- 12. VAT INPUT
        --
        -- DR VAT Input
        --------------------------------------------------------

        IF @VATAmount > 0
        BEGIN

            IF @VATAccountID IS NULL
            BEGIN
                THROW 50004,
                    'VAT Input account was not found.',
                    1;
            END;


            INSERT INTO TblAccount_JournalEntries
            (
                TransactionID,
                AccountID,
                Debit,
                Credit,
                EntryDate,
                TransactionDate,
                SystemID,
                WarehouseID,
                DomainID,
                CreateByUserID,
                CreateDate,
                Description,
                VATAmount,
                DiscountAmount,
                ExpenseID,
                ExpenseDetailID,
                ExpenseAmount,
                ItemTAX,
                ClassNameID,
                PaymentNo,
                ExpenseNo,
                Type,
                JournalNo,
                InvoiceNo
            )
            VALUES
            (
                @TransactionID,

                @VATAccountID,

                @VATAmount,
                0,

                @EntryDate,
                @EntryDate,

                @SystemID,
                @WarehouseID,
                @DomainID,

                @CreateByUserID,
                GETDATE(),

                'VAT Input - '
                    + @ItemName,

                @VATAmount,
                0,

                @ExpenseID,
                @ExpenseDetailID,

                @VATAmount,

                @ItemTAX,
                @ClassNameID,

                @PaymentNo,
                @ExpenseNo,

                'Expense',

                @JournalNo,
                @ExpenseNo
            );

        END;

    END;
    ";


            // =========================================================
            // 7. EXECUTE SQL
            // =========================================================

            using (SqlCommand cmd =
                new SqlCommand(
                    sql,
                    conn,
                    tran))
            {

                // =====================================================
                // IDs
                // =====================================================

                cmd.Parameters.Add(
                    "@ExpenseID",
                    SqlDbType.Int
                ).Value =
                    expenseID;

                cmd.Parameters.Add(
                    "@ExpenseDetailID",
                    SqlDbType.Int
                ).Value =
                    d.ExpenseDetailID;

                cmd.Parameters.Add(
                    "@TransactionID",
                    SqlDbType.Int
                ).Value =
                    transactionID;


                // =====================================================
                // ITEM
                // =====================================================

                cmd.Parameters.Add(
                    "@ItemTypeID",
                    SqlDbType.Int
                ).Value =
                    d.ItemTypeID;

                cmd.Parameters.Add(
                    "@ExpenseCategory",
                    SqlDbType.Int
                ).Value =
                    d.ExpenseCategory;

                cmd.Parameters.Add(
                    "@ItemID",
                    SqlDbType.Int
                ).Value =
                    d.ItemID;

                cmd.Parameters.Add(
                    "@ItemName",
                    SqlDbType.NVarChar
                ).Value =
                    d.ItemName ?? "";

                cmd.Parameters.Add(
                    "@ItemName_Description",
                    SqlDbType.NVarChar
                ).Value =
                    d.ItemName_Description ?? "";

                cmd.Parameters.Add(
                    "@UM",
                    SqlDbType.NVarChar
                ).Value =
                    html.HtmlDecode(d.UM) ?? "";

                cmd.Parameters.Add(
                    "@QTY",
                    SqlDbType.Decimal
                ).Value =
                    d.QTY;

                cmd.Parameters.Add(
                    "@UnitPrice",
                    SqlDbType.Decimal
                ).Value =
                    d.UnitPrice;

                cmd.Parameters.Add(
                    "@DisUSD",
                    SqlDbType.Decimal
                ).Value =
                    d.DisUSD;

                cmd.Parameters.Add(
                    "@Total",
                    SqlDbType.Decimal
                ).Value =
                    d.Total;

                cmd.Parameters.Add(
                    "@ItemTAX",
                    SqlDbType.Decimal
                ).Value =
                    d.ItemTAX;

                cmd.Parameters.Add(
                    "@ItemGrandTotal",
                    SqlDbType.Decimal
                ).Value =
                    d.ItemGrandTotal;

                cmd.Parameters.Add(
                    "@RefNo",
                    SqlDbType.NVarChar
                ).Value =
                    d.RefNo ?? "";

                cmd.Parameters.Add(
                    "@SourceType",
                    SqlDbType.NVarChar
                ).Value =
                    d.SourceType ?? "";


                // =====================================================
                // DISCOUNT
                // =====================================================

                cmd.Parameters.Add(
                    "@ItemDiscountAmount",
                    SqlDbType.Decimal
                ).Value =
                    itemDiscount;

                cmd.Parameters.Add(
                    "@PurchaseDiscountAccountID",
                    SqlDbType.Int
                ).Value =
                    purchaseDiscountAccountID > 0 ? (object)purchaseDiscountAccountID : DBNull.Value;


                // =====================================================
                // PAYMENT ACCOUNT
                // =====================================================

                cmd.Parameters.Add(
                    "@PaymentAccountID",
                    SqlDbType.Int
                ).Value =
                    paymentAccountID;


                // =====================================================
                // CLASS
                // =====================================================

                cmd.Parameters.Add(
                    "@ClassNameID",
                    SqlDbType.Int
                ).Value =
                    classNameID;


                // =====================================================
                // VAT
                // =====================================================

                cmd.Parameters.Add(
                    "@VATAmount",
                    SqlDbType.Decimal
                ).Value =
                    vatAmount;


                // =====================================================
                // COMMON
                // =====================================================

                cmd.Parameters.Add(
                    "@EntryDate",
                    SqlDbType.DateTime
                ).Value =
                    e.PaymentDate;

                cmd.Parameters.Add(
                    "@SystemID",
                    SqlDbType.Int
                ).Value =
                    html.SystemID;

                cmd.Parameters.Add(
                    "@WarehouseID",
                    SqlDbType.Int
                ).Value =
                    html.WarehouseID;

                cmd.Parameters.Add(
                    "@DomainID",
                    SqlDbType.NVarChar
                ).Value =
                    html.DomainID;

                cmd.Parameters.Add(
                    "@CreateByUserID",
                    SqlDbType.Int
                ).Value =
                    html.UserID;


                // =====================================================
                // DOCUMENT NUMBERS
                // =====================================================

                cmd.Parameters.Add(
                    "@PaymentNo",
                    SqlDbType.NVarChar
                ).Value =
                    expensePostingResult.PaymentNo ?? "";

                cmd.Parameters.Add(
                    "@ExpenseNo",
                    SqlDbType.NVarChar
                ).Value =
                    expensePostingResult.ExpenseNo ?? "";

                cmd.Parameters.Add(
                    "@JournalNo",
                    SqlDbType.NVarChar
                ).Value =
                    expensePostingResult.JournalNo ?? "";


                // =====================================================
                // EXECUTE
                // =====================================================

                cmd.ExecuteNonQuery();
            }
        }

        private void DeleteExpenseDetails(int expenseId, SqlConnection conn, SqlTransaction tran)
        {
            string sql = @"
BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Delete applications linked to this Expense
    DELETE FROM dbo.TblVendorDepositApplication
    WHERE ExpenseID = @ExpenseID;

    -- 2. Delete applications linked to the deposits
    -- (including applications from other Bills)
    DELETE FROM dbo.TblVendorDepositApplication
    WHERE VendorDepositID IN
    (
        SELECT VendorDepositID
        FROM dbo.TblVendorDeposit
        WHERE ExpenseID = @ExpenseID
    );

    -- 3. Delete Vendor Deposits
    DELETE FROM dbo.TblVendorDeposit
    WHERE ExpenseID = @ExpenseID;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
DELETE FROM Expenses WHERE ExpenseID = @ExpenseID;
DELETE FROM Expense_PaymentTransactions WHERE ExpenseID = @ExpenseID;
DELETE FROM ExpenseDetails WHERE ExpenseID = @ExpenseID;
DELETE FROM TblAccount_JournalEntries WHERE ExpenseID = @ExpenseID;
DELETE FROM TblAccount_Transaction WHERE ExpenseID = @ExpenseID;
";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = expenseId;
                cmd.ExecuteNonQuery();
            }
        }

        public ExpensePostingResult UpdateExpense(Expense expense)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // If user inputs PaidAmount (example: 2000)
                        expense.BalanceAmount = expense.TotalAmount - expense.PaidAmount;

                        // 🔹 Determine status
                        if (expense.PaidAmount == 0)
                            expense.PaymentStatus = "Unpaid";
                        else if (expense.PaidAmount < expense.TotalAmount)
                            expense.PaymentStatus = "Partial";
                        else
                            expense.PaymentStatus = "Paid";
                        // 1️⃣ Update Header
                        ExpensePostingResult expensePostingResult = UpdateExpenseHeader(expense, conn, tran);

                        // 2️⃣ Delete Old Details
                        //DeleteExpenseDetails(expense.ExpenseID, conn, tran);

                        // 3️⃣ Reinsert Details
                        //foreach (var d in expense.Details)
                        //{
                        //    InsertExpenseDetailForUpdate(expense, d, conn, tran);
                        //}
                        //decimal wholeExpenseDiscount = expense.Discount;

                        //ExpensePostingResult expensePostingResult = new ExpensePostingResult
                        //{
                        //    ExpenseID = expense.ExpenseID,
                        //};
                        //string sql = "SELECT TransactionID, AccountID from dbo.TblAccount_Transaction where ExpenseID = @ExpenseID";

                        //using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                        //{
                        //    cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = expense.ExpenseID;

                        //    using (SqlDataReader reader = cmd.ExecuteReader())
                        //    {
                        //        if (reader.Read())
                        //        {
                        //            expensePostingResult.TransactionID =
                        //                reader["TransactionID"] == DBNull.Value
                        //                    ? 0
                        //                    : Convert.ToInt32(reader["TransactionID"]);

                        //            expensePostingResult.APAccountID =
                        //                reader["AccountID"] == DBNull.Value
                        //                    ? 0
                        //                    : Convert.ToInt32(reader["AccountID"]);
                        //            expensePostingResult.PaymentAccountID =
                        //                reader["AccountID"] == DBNull.Value
                        //                    ? 0
                        //                    : Convert.ToInt32(reader["AccountID"]);
                        //        }
                        //        else
                        //        {
                        //            throw new Exception(
                        //                $"Transaction not found for ExpenseID = {expense.ExpenseID}."
                        //            );
                        //        }
                        //    }
                        //}


                        //if (wholeExpenseDiscount > 0)
                        //{
                        //    InsertWholeExpenseDiscountJournal(
                        //        expense,
                        //        expensePostingResult,
                        //        wholeExpenseDiscount,
                        //        conn,
                        //        tran
                        //    );
                        //}
                        tran.Commit();
                        return new ExpensePostingResult
                        {
                            ExpenseID = expense.ExpenseID,
                            PaymentNo = expense.PaymentNo
                        };
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
        public string GetBase64Image(string fullPath)
        {
            if (!System.IO.File.Exists(fullPath))
                return null;

            byte[] imageBytes = System.IO.File.ReadAllBytes(fullPath);
            string base64 = Convert.ToBase64String(imageBytes);
            return "data:image/png;base64," + base64;
        }

        public PaymentVoucherViewModel GetVoucherByExpenseId(int expenseId, string PaymentNo = "")
        {
            PaymentVoucherViewModel model = new PaymentVoucherViewModel
            {
                Items = new List<PaymentVoucherItemVM>(),
                JournalEntries = new List<JournalEntryVM>()
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                #region 1️⃣ HEADER

                string headerSql = @"
SELECT 
    e.ExpenseID,
    e.PVNo,
    e.[No],
    e.PaymentDate,
    e.PaidOnDate,
    e.Reference,
    e.Total,
    e.BankInfo,
    e.BankerName,
    e.ChequeBankerName,
    e.BankAccountName,
    e.BankAccountNo,
    e.CheckedBy,
    e.VerifiedBy,
    e.ApprovedBy,
    e.ChequeTo,
    e.ChequeNo,

    bp.VendorName,
    pm.PaymentMethod,
    c.FileLogo,

    u.FullName AS PreparedBy,
    ISNULL(u.[Signature], '') AS [Signature],
    ISNULL(uc.[Signature], '') AS [CheckedSignature],

    e.[Status],
    FORMAT(e.CheckedDate, 'dd/MM/yyyy hh:mm:ss') AS CheckedDate,

    c.FullName,

    up.FullName AS PostedBy,

    ISNULL(e.ExchangeRate, 0) AS ExchangeRate,
    e.FileName,
    e.PaymentTermsID,
    e.TermName,

    -- =====================================================
    -- CURRENT PAYMENT
    -- =====================================================
    ISNULL(e.PaidAmount, 0) AS PaidAmount,

    -- =====================================================
    -- TOTAL PAID
    -- =====================================================
    ISNULL(TotalPaidAmount, 0) AS TotalPaid,

    -- =====================================================
    -- REMAINING BALANCE
    -- =====================================================
    ISNULL(TotalBalanceAmount, 0) AS BalanceAmount,

    ISNULL(e.PaymentStatus, 'Unpaid') AS PaymentStatus,

    -- =====================================================
    -- FINAL TOTAL
    -- =====================================================
    ISNULL(e.TotalAmount, 0) AS TotalAmount,

    bp.CompanyName,
    bp.CompanyAddress,

    e.IsPosted,
    FORMAT(e.PostedDate, 'dd/MM/yyyy hh:mm:ss') AS PostedDate,

    ISNULL(up.[Signature], '') AS PostedSignature,
    ISNULL(uv.[Signature], '') AS VerifiedSignature,
    ISNULL(uv.FullName, '') AS VerifiedName,

    ISNULL(e.IsChecked, 0) AS IsChecked,
    ISNULL(e.IsVerified, 0) AS IsVerified,

    FORMAT(e.VerifiedDate, 'dd/MM/yyyy hh:mm:ss') AS VerifiedDate,

    e.Description,
    e.CreatedDate,

    ISNULL(e.IsRequestedPayment, 0) AS IsRequestedPayment,
    e.GrantTotalAsRiel,

    -- =====================================================
    -- TOTAL DISCOUNT
    --
    -- Item Discount + Whole Expense Discount
    --
    -- Example:
    -- Item Discount          = 20
    -- Whole Expense Discount = 0
    -- Total Discount         = 20
    -- =====================================================
    --ISNULL(e.DiscountItem, 0) +
    ISNULL(e.Discount, 0) AS Discount,
    e.ExpenseTransactionType, e.VAT, e.Type

FROM Expense_PaymentTransactions e

INNER JOIN TblClass c
    ON c.ClassID = e.ClassName

INNER JOIN TblVendor bp
    ON e.PayeeID = bp.VendorID

LEFT JOIN TblPaymentMethod pm
    ON e.PaymentMethodID = pm.PaymentMethodID

INNER JOIN TblUserLogin u
    ON u.UserLoginID = ISNULL(e.ModifyByUserID, e.CreateByUserID)

INNER JOIN TblUserLogin uc
    ON uc.UserLoginID = e.CheckedByUserID

LEFT JOIN TblUserLogin up
    ON up.UserLoginID = e.PostedByUserID

LEFT JOIN TblUserLogin uv
    ON uv.UserLoginID = e.VerifiedByUserID

WHERE e.ExpenseID = @ExpenseID
";
                if (PaymentNo != "")
                {
                    headerSql += $@" AND PaymentNo = '{PaymentNo}'";
                }

                using (SqlCommand cmd = new SqlCommand(headerSql, conn))
                {
                    cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = expenseId;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exception("Voucher not found.");
                        HTML html = new HTML();
                        model.CompanyLogoUrl = GetBase64Image(HttpContext.Current.Server.MapPath("~/uploads/classlogo/" + reader["FileLogo"]?.ToString())); // or load from settings
                        model.CompanyName = reader["FullName"]?.ToString(); // or load from settings
                        model.VendorCompanyName = reader["CompanyName"]?.ToString(); // or load from settings
                        model.VendorCompanyAddress = reader["CompanyAddress"]?.ToString(); // or load from settings
                        model.ExpenseTransactionType = reader["ExpenseTransactionType"]?.ToString(); // or load from settings
                        model.PVNo = reader["PVNo"]?.ToString();
                        model.No = reader["No"]?.ToString();
                        model.PrintDate = DateTime.Now;
                        model.VoucherDate = reader["PaymentDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PaymentDate"]); //Convert.ToDateTime(reader["PaymentDate"]);
                        //model.DueDate = Convert.ToDateTime(reader["PaidOnDate"]);
                        model.DueDate = reader["PaidOnDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PaidOnDate"]);
                        model.PostedDate = reader["PostedDate"].ToString();
                        model.VerifiedDate = reader["VerifiedDate"].ToString();
                        model.Description = reader["Description"].ToString();

                        model.VAT = reader["VAT"] as decimal? ?? 0m;
                        model.VendorName = reader["VendorName"]?.ToString();
                        model.Reference = reader["Reference"]?.ToString();
                        model.PaymentMethod = reader["PaymentMethod"]?.ToString();
                        model.Currency = "USD ($)";

                        model.ChequeBankerName = reader["ChequeBankerName"]?.ToString();
                        model.BankName = reader["BankerName"]?.ToString();
                        model.Type = reader["Type"]?.ToString();
                        model.BankAccountName = reader["BankAccountName"]?.ToString();
                        model.BankAccountNo = reader["BankAccountNo"]?.ToString();
                        model.FileName = reader["FileName"]?.ToString();
                        model.PaymentTermsID = reader["PaymentTermsID"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PaymentTermsID"]);//reader["PaymentTermsID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["PaymentTermsID"]);
                        model.TermName = reader["TermName"]?.ToString();
                        model.IsPosted = Convert.ToBoolean(reader["IsPosted"]);
                        model.TotalPaid = Convert.ToDecimal(reader["TotalPaid"]);

                        decimal Discount = Convert.ToDecimal(reader["Discount"]);
                        decimal GrantTotalAsRiel = reader["GrantTotalAsRiel"] as decimal? ?? 0m;
                        decimal total = Convert.ToDecimal(reader["TotalAmount"]);
                        decimal paid = Convert.ToDecimal(reader["PaidAmount"]);
                        decimal balance = Convert.ToDecimal(reader["BalanceAmount"]);
                        string paymentStatus = reader["PaymentStatus"].ToString();
                        bool IsChecked = bool.Parse(reader["IsChecked"].ToString());
                        bool IsVerified = bool.Parse(reader["IsVerified"].ToString());
                        model.IsPartialPayment = paid > 0 && paid < total;
                        // 🔹 Assign to model
                        model.TotalAmount = total;
                        model.GrantTotalAsRiel = GrantTotalAsRiel;
                        model.PaidAmount = paid;
                        model.BalanceAmount = balance;
                        model.PaymentStatus = paymentStatus;
                        model.Discount = Discount;

                        model.ChequeTo = reader["ChequeTo"]?.ToString();
                        model.ChequeNo = reader["ChequeNo"]?.ToString();
                        model.ExchangeRate = Convert.ToDecimal(reader["ExchangeRate"]);

                        var fileName = reader["Signature"].ToString();
                        string Status = reader["Status"].ToString();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            var fileNames = fileName.Split('~');
                            var folderPath = HttpContext.Current.Server.MapPath("~/Uploads/Users");
                            var fullPath = System.IO.Path.Combine(folderPath, fileNames[0].Trim());

                            model.Signature =
                                $"<img src=\"{GetBase64Image(fullPath)}\" height=\"50\" />";
                        }
                        else
                        {
                            model.Signature = "";
                        }
                        if (IsChecked)
                        {
                            model.CheckedDate = reader["CheckedDate"].ToString();
                            //CheckedSignature
                            fileName = reader["CheckedSignature"].ToString();

                            if (!string.IsNullOrEmpty(fileName))
                            {
                                var fileNames = fileName.Split('~');
                                var folderPath = HttpContext.Current.Server.MapPath("~/Uploads/Users");
                                var fullPath = System.IO.Path.Combine(folderPath, fileNames[0].Trim());

                                model.CheckedSignature =
                                    $"<img src=\"{GetBase64Image(fullPath)}\" height=\"50\" />";
                            }
                            else
                            {
                                model.CheckedSignature = "";
                            }
                        }
                        else
                        {
                            model.CheckedSignature = "";
                        }

                        if (IsVerified)
                        {
                            //VerifiedSignature
                            fileName = reader["VerifiedSignature"].ToString();
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                var fileNames = fileName.Split('~');
                                var folderPath = HttpContext.Current.Server.MapPath("~/Uploads/Users");
                                var fullPath = System.IO.Path.Combine(folderPath, fileNames[0].Trim());
                                model.VerifiedSignature =
                                    $"<img src=\"{GetBase64Image(fullPath)}\" height=\"50\" />";
                            }
                            else
                            {
                                model.VerifiedSignature = "";
                            }
                        }
                        else
                        {
                            model.VerifiedSignature = "";
                        }

                        model.PreparedBy = reader["PreparedBy"]?.ToString();
                        model.CheckedBy = reader["CheckedBy"]?.ToString();
                        model.VerifiedBy = reader["VerifiedBy"]?.ToString();
                        model.ApprovedBy = reader["ApprovedBy"]?.ToString();
                        model.PostedBy = reader["PostedBy"]?.ToString();

                        if (model.IsPosted)
                        {
                            fileName = reader["PostedSignature"].ToString();

                            if (!string.IsNullOrEmpty(fileName))
                            {
                                var fileNames = fileName.Split('~');
                                var folderPath = HttpContext.Current.Server.MapPath("~/Uploads/Users");
                                var fullPath = System.IO.Path.Combine(folderPath, fileNames[0].Trim());

                                model.PostedSignature =
                                    $"<img src=\"{GetBase64Image(fullPath)}\" height=\"50\" />";
                            }
                            else
                            {
                                model.PostedSignature = "";
                            }
                        }
                        else
                        {
                            model.PostedSignature = "";
                        }

                        model.Status = Status;
                        model.GrandTotal = Convert.ToDecimal(reader["TotalAmount"]);
                        bool isRequestedPayment = Convert.ToBoolean(reader["IsRequestedPayment"]);
                        if (isRequestedPayment)
                        {
                            string paymentHistorySql = @"
SELECT
    ROW_NUMBER() OVER
    (
        PARTITION BY ExpenseID
        ORDER BY ExpensePaymentID
    ) AS PaymentNo,

    ExpensePaymentID,
    PaymentNo AS PaymentCode,
    PVNo,
    ExpenseNo,
    CreatedDate AS PaymentDate,
    TotalAmount,
    PaidAmount,
    BalanceAmount,
    PaymentStatus,
    Status,

    CASE
        WHEN ROW_NUMBER() OVER
        (
            PARTITION BY ExpenseID
            ORDER BY ExpensePaymentID DESC
        ) = 1
        THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS IsCurrentPayment

FROM Expense_PaymentTransactions
WHERE ExpenseID = @ExpenseID
ORDER BY ExpensePaymentID;";

                            using (SqlCommand paymentCmd = new SqlCommand(paymentHistorySql, conn))
                            {
                                paymentCmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = expenseId;

                                using (SqlDataReader paymentReader = paymentCmd.ExecuteReader())
                                {
                                    while (paymentReader.Read())
                                    {
                                        model.PaymentHistory.Add(new PaymentHistoryVM
                                        {
                                            PaymentNo = Convert.ToInt32(paymentReader["PaymentNo"]),
                                            ExpensePaymentID = Convert.ToInt32(paymentReader["ExpensePaymentID"]),

                                            PaymentCode = paymentReader["PaymentCode"]?.ToString(),

                                            PVNo = paymentReader["PVNo"]?.ToString(),

                                            ExpenseNo = paymentReader["ExpenseNo"]?.ToString(),

                                            PaymentDate = Convert.ToDateTime(paymentReader["PaymentDate"])
                                                .ToString("dd-MMM-yyyy HH:mm"),

                                            TotalAmount = paymentReader["TotalAmount"] == DBNull.Value
                                                ? 0
                                                : Convert.ToDecimal(paymentReader["TotalAmount"]),

                                            PaidAmount = paymentReader["PaidAmount"] == DBNull.Value
                                                ? 0
                                                : Convert.ToDecimal(paymentReader["PaidAmount"]),

                                            BalanceAmount = paymentReader["BalanceAmount"] == DBNull.Value
                                                ? 0
                                                : Convert.ToDecimal(paymentReader["BalanceAmount"]),

                                            PaymentStatus = paymentReader["PaymentStatus"]?.ToString(),

                                            Status = paymentReader["Status"]?.ToString(),

                                            IsCurrentPayment = Convert.ToBoolean(paymentReader["IsCurrentPayment"])
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion

                #region 2️⃣ DETAILS (ITEMS)

                string detailSql = @"
SELECT 
    ed.ExpenseCategory,
    coa.AccountNo,
    ed.ItemName,
    ed.ItemName_Description,
    ed.QTY,
    ed.UnitPrice,
    ed.Total,
    ed.ItemTAX,
    ed.DisUSD,
    ed.UM,
    isnull(ed.ItemVAT, 0) AS ItemVAT, ItemGrandTotal
FROM ExpenseDetails ed
INNER JOIN TblChartOfAccount coa ON ed.ExpenseCategory = coa.ChartOfAccountID
WHERE ed.ExpenseID = @ExpenseID
";

                using (SqlCommand cmd = new SqlCommand(detailSql, conn))
                {
                    cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = expenseId;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        decimal subTotal = 0;
                        decimal totalTax = 0;

                        while (reader.Read())
                        {
                            decimal amount = Convert.ToDecimal(reader["Total"]);
                            decimal ItemVAT = Convert.ToDecimal(reader["ItemVAT"]);
                            decimal taxPercent = reader["ItemTAX"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["ItemTAX"]);

                            decimal taxAmount = (amount * taxPercent) / 100;

                            model.Items.Add(new PaymentVoucherItemVM
                            {
                                AccountCode = reader["AccountNo"]?.ToString(),
                                Description = HttpUtility.HtmlDecode(Uri.UnescapeDataString(reader["ItemName"]?.ToString())),
                                ItemName_Description = HttpUtility.HtmlDecode(Uri.UnescapeDataString(reader["ItemName_Description"]?.ToString())),
                                Qty = Convert.ToDecimal(reader["QTY"]),
                                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                Discount = Convert.ToDecimal(reader["DisUSD"]),
                                UOM = reader["UM"]?.ToString(),
                                Amount = amount,
                                ItemVAT = ItemVAT,
                                ItemTAX = taxPercent,
                                ItemGrandTotal = Convert.ToDecimal(reader["ItemGrandTotal"])
                            });

                            subTotal += Convert.ToDecimal(reader["ItemGrandTotal"]);
                            totalTax += taxAmount;
                        }

                        model.SubTotal = subTotal;
                        model.Tax = totalTax;
                    }
                }

                #endregion

                #region 3️⃣ JOURNAL ENTRIES

                string journalSql = @"

;WITH

/* ===============================================================
   1. SELECTED PAYMENT CUTOFFS
   =============================================================== */
SelectedExpensePayment AS
(
    SELECT
        MIN(je.EntryID) AS SelectedFirstEntryID,
        MAX(je.EntryID) AS SelectedLastEntryID,
        MAX(je.TransactionID) AS SelectedTransactionID
    FROM dbo.TblAccount_JournalEntries AS je
    WHERE je.ExpenseID = @ExpenseID
      AND je.Type = 'Expense Payment'
      AND @PaymentNo IS NOT NULL
      AND LTRIM(RTRIM(@PaymentNo)) <> ''
      AND LTRIM(RTRIM(je.PaymentNo)) = LTRIM(RTRIM(@PaymentNo))
),

SelectedVendorDepositPayment AS
(
    SELECT
        MIN(je.EntryID) AS SelectedFirstEntryID,
        MAX(je.EntryID) AS SelectedLastEntryID,
        MAX(je.TransactionID) AS SelectedTransactionID
    FROM dbo.TblAccount_JournalEntries AS je
    WHERE je.ExpenseID = @ExpenseID
      AND je.Type = 'Vendor Deposit Payment'
      AND @PaymentNo IS NOT NULL
      AND LTRIM(RTRIM(@PaymentNo)) <> ''
      AND LTRIM(RTRIM(je.PaymentNo)) = LTRIM(RTRIM(@PaymentNo))
),

/* ===============================================================
   2. JOURNAL BASE
   =============================================================== */
JournalBase AS
(
    SELECT
        je.EntryID,
        je.Type,
        je.ExpenseTransactionType,
        je.TransactionID,
        je.AccountID,
        je.ExpenseID,
        je.JournalNo,

        coa.AccountNo,

        /* FIX: Direct Payment must not be classified as A/P */
        CASE
            WHEN ISNULL(je.ExpenseTransactionType, '') = 'Direct Payment'
            THEN coa.AccountName

            WHEN coa.AccountName = 'Accounts Payable (A/P)'
              OR coa.AccountName = 'Account Payable (A/P)'
              OR coa.AccountName LIKE '%Accounts Payable%'
              OR coa.AccountName LIKE '%Account Payable%'
            THEN 'Account Payable (A/P)'

            ELSE coa.AccountName
        END AS AccountName,

        ISNULL(je.Debit, 0) AS Debit,
        ISNULL(je.Credit, 0) AS Credit,
        ISNULL(je.DiscountAmount, 0) AS DiscountAmount,
        ISNULL(je.ItemTAX, 0) AS ItemTAX,

        /* FIX: Exclude Direct Payment from A/P classification */
        CASE
            WHEN ISNULL(je.ExpenseTransactionType, '') = 'Direct Payment'
            THEN 0

            WHEN coa.AccountName = 'Accounts Payable (A/P)'
              OR coa.AccountName = 'Account Payable (A/P)'
              OR coa.AccountName LIKE '%Accounts Payable%'
              OR coa.AccountName LIKE '%Account Payable%'
            THEN 1

            ELSE 0
        END AS IsAPAccount,

        CASE
            WHEN coa.AccountName LIKE '%VAT Input%'
            THEN 1
            ELSE 0
        END AS IsVATInput,

        je.PaymentNo,
        je.EntryDate,
        je.TransactionDate,
        je.CreateDate

    FROM dbo.TblAccount_JournalEntries AS je

    INNER JOIN dbo.TblChartOfAccount AS coa
        ON coa.ChartOfAccountID = je.AccountID

    WHERE je.ExpenseID = @ExpenseID
      AND
      (
          je.Type = 'Expense'
          OR je.Type = 'Bill'
          OR je.Type = 'Vendor Deposit'

          OR je.ExpenseTransactionType = 'Vendor Deposit Application'

          OR
          (
              je.Type = 'Expense Payment'
              AND
              (
                  @PaymentNo IS NULL
                  OR LTRIM(RTRIM(@PaymentNo)) = ''
                  OR
                  (
                      EXISTS
                      (
                          SELECT 1
                          FROM SelectedExpensePayment AS sp
                          WHERE sp.SelectedLastEntryID IS NOT NULL
                      )
                      AND je.EntryID <=
                      (
                          SELECT sp.SelectedLastEntryID
                          FROM SelectedExpensePayment AS sp
                      )
                  )
              )
          )

          OR
          (
              je.Type = 'Vendor Deposit Payment'
              AND
              (
                  @PaymentNo IS NULL
                  OR LTRIM(RTRIM(@PaymentNo)) = ''
                  OR
                  (
                      EXISTS
                      (
                          SELECT 1
                          FROM SelectedVendorDepositPayment AS sp
                          WHERE sp.SelectedLastEntryID IS NOT NULL
                      )
                      AND je.EntryID <=
                      (
                          SELECT sp.SelectedLastEntryID
                          FROM SelectedVendorDepositPayment AS sp
                      )
                  )
              )
          )
      )
),

/* ===============================================================
   3. PURCHASE / BILL DETAIL ACCOUNTS
   =============================================================== */
PurchaseEntries AS
(
    SELECT
        MAX(jb.TransactionID) AS TransactionID,
        'Purchase' AS TransactionType,
        jb.AccountID,
        MAX(jb.AccountNo) AS AccountNo,

        CASE
            WHEN MAX(jb.IsVATInput) = 1
            THEN 'VAT Input'
            ELSE MAX(jb.AccountName)
        END AS AccountName,

        CASE
            WHEN MAX(jb.IsVATInput) = 1
            THEN MAX(jb.ItemTAX)
            ELSE 0
        END AS ItemTAX,

        CASE
            WHEN MAX(jb.IsVATInput) = 0
                 AND SUM(jb.Debit) > 0
                 AND MAX(jb.AccountName) <> 'Purchase Discount'
            THEN SUM(jb.Debit) - SUM(jb.DiscountAmount)
            ELSE SUM(jb.Debit)
        END AS Debit,

        SUM(jb.Credit) AS Credit,
        MAX(jb.IsAPAccount) AS IsAPAccount,
        MAX(jb.IsVATInput) AS IsVATInput,
        CAST(NULL AS VARCHAR(100)) AS PaymentNo,
        MIN(jb.EntryID) AS FirstEntryID,
        CAST(NULL AS DATETIME) AS PaymentDate

    FROM JournalBase AS jb

    WHERE jb.Type IN ('Expense', 'Bill')
      AND jb.IsAPAccount = 0
      AND ISNULL(jb.ExpenseTransactionType, '')
          <> 'Vendor Deposit Application'

    GROUP BY
        jb.AccountID,
        CASE
            WHEN jb.IsVATInput = 1
            THEN jb.ItemTAX
            ELSE 0
        END
),

/* ===============================================================
   4. ORIGINAL A/P CREDIT
   =============================================================== */
OriginalAP AS
(
    SELECT
        ISNULL(SUM(jb.Credit), 0) AS OriginalAPAmount
    FROM JournalBase AS jb
    WHERE jb.Type IN ('Expense', 'Bill')
      AND jb.IsAPAccount = 1
      AND ISNULL(jb.ExpenseTransactionType, '')
          <> 'Vendor Deposit Application'
),

/* ===============================================================
   5. ACTUAL PAYMENT A/P DEBITS
   =============================================================== */
PaidAP AS
(
    SELECT
        ISNULL(SUM(jb.Debit), 0) AS PaidAPAmount
    FROM JournalBase AS jb
    WHERE jb.Type = 'Expense Payment'
      AND jb.IsAPAccount = 1
),

/* ===============================================================
   6. VENDOR DEPOSIT APPLICATION A/P DEBITS
   =============================================================== */
VendorDepositApplied AS
(
    SELECT
        ISNULL(SUM(jb.Debit), 0) AS AppliedAPAmount
    FROM JournalBase AS jb
    WHERE jb.ExpenseTransactionType = 'Vendor Deposit Application'
      AND jb.IsAPAccount = 1
),

/* ===============================================================
   7. REMAINING A/P
   =============================================================== */
PaymentStatus AS
(
    SELECT
        oa.OriginalAPAmount,
        pa.PaidAPAmount,
        va.AppliedAPAmount,

        CAST(
            CASE
                WHEN oa.OriginalAPAmount
                     - pa.PaidAPAmount
                     - va.AppliedAPAmount < 0
                THEN 0
                ELSE oa.OriginalAPAmount
                     - pa.PaidAPAmount
                     - va.AppliedAPAmount
            END
            AS DECIMAL(18,2)
        ) AS RemainingAP,

        CASE
            WHEN oa.OriginalAPAmount > 0
             AND oa.OriginalAPAmount
                 - pa.PaidAPAmount
                 - va.AppliedAPAmount <= 0.005
            THEN 1
            ELSE 0
        END AS IsFullyPaid

    FROM OriginalAP AS oa
    CROSS JOIN PaidAP AS pa
    CROSS JOIN VendorDepositApplied AS va
),

/* ===============================================================
   8. REMAINING A/P DISPLAY LINE
   =============================================================== */
PurchaseAPEntries AS
(
    SELECT
        MAX(jb.TransactionID) AS TransactionID,
        'Purchase' AS TransactionType,
        jb.AccountID,
        MAX(jb.AccountNo) AS AccountNo,
        'Account Payable (A/P)' AS AccountName,
        CAST(0 AS DECIMAL(18,2)) AS ItemTAX,
        CAST(0 AS DECIMAL(18,2)) AS Debit,
        ps.RemainingAP AS Credit,
        1 AS IsAPAccount,
        0 AS IsVATInput,
        CAST(NULL AS VARCHAR(100)) AS PaymentNo,
        MIN(jb.EntryID) AS FirstEntryID,
        CAST(NULL AS DATETIME) AS PaymentDate

    FROM JournalBase AS jb
    CROSS JOIN PaymentStatus AS ps

    WHERE jb.Type IN ('Expense', 'Bill')
      AND jb.IsAPAccount = 1
      AND ISNULL(jb.ExpenseTransactionType, '')
          <> 'Vendor Deposit Application'

    GROUP BY
        jb.AccountID,
        ps.RemainingAP

    HAVING ps.RemainingAP > 0.005
),

/* ===============================================================
   9. PAYMENT CASH / BANK ENTRIES
   =============================================================== */
PaymentEntries AS
(
    SELECT
        MAX(jb.TransactionID) AS TransactionID,
        'Payment' AS TransactionType,
        jb.AccountID,
        MAX(jb.AccountNo) AS AccountNo,
        MAX(jb.AccountName) AS AccountName,

        CASE
            WHEN MAX(jb.IsVATInput) = 1
            THEN MAX(jb.ItemTAX)
            ELSE 0
        END AS ItemTAX,

        SUM(jb.Debit) AS Debit,
        SUM(jb.Credit) AS Credit,
        MAX(jb.IsAPAccount) AS IsAPAccount,
        MAX(jb.IsVATInput) AS IsVATInput,
        jb.PaymentNo,
        MIN(jb.EntryID) AS FirstEntryID,

        MIN(
            COALESCE(
                jb.TransactionDate,
                jb.EntryDate,
                jb.CreateDate
            )
        ) AS PaymentDate

    FROM JournalBase AS jb

    WHERE jb.Type = 'Expense Payment'
      AND jb.IsAPAccount = 0

    GROUP BY
        jb.AccountID,
        jb.PaymentNo
),

/* ===============================================================
   10. VENDOR DEPOSIT ENTRIES
   =============================================================== */
VendorDepositEntries AS
(
    SELECT
        MAX(jb.TransactionID) AS TransactionID,
        'Vendor Deposit' AS TransactionType,
        jb.AccountID,
        MAX(jb.AccountNo) AS AccountNo,
        MAX(jb.AccountName) AS AccountName,
        CAST(0 AS DECIMAL(18,2)) AS ItemTAX,
        SUM(jb.Debit) AS Debit,
        SUM(jb.Credit) AS Credit,
        0 AS IsAPAccount,
        0 AS IsVATInput,
        CAST(NULL AS VARCHAR(100)) AS PaymentNo,
        MIN(jb.EntryID) AS FirstEntryID,

        MIN(
            COALESCE(
                jb.TransactionDate,
                jb.EntryDate,
                jb.CreateDate
            )
        ) AS PaymentDate

    FROM JournalBase AS jb
    WHERE jb.Type = 'Vendor Deposit'
    GROUP BY jb.AccountID
),

/* ===============================================================
   11. VENDOR DEPOSIT PAYMENT ENTRIES
   =============================================================== */
VendorDepositPaymentEntries AS
(
    SELECT
        MAX(jb.TransactionID) AS TransactionID,
        'Vendor Deposit Payment' AS TransactionType,
        jb.AccountID,
        MAX(jb.AccountNo) AS AccountNo,
        MAX(jb.AccountName) AS AccountName,
        CAST(0 AS DECIMAL(18,2)) AS ItemTAX,
        SUM(jb.Debit) AS Debit,
        SUM(jb.Credit) AS Credit,
        0 AS IsAPAccount,
        0 AS IsVATInput,
        jb.PaymentNo,
        MIN(jb.EntryID) AS FirstEntryID,

        MIN(
            COALESCE(
                jb.TransactionDate,
                jb.EntryDate,
                jb.CreateDate
            )
        ) AS PaymentDate

    FROM JournalBase AS jb
    WHERE jb.Type = 'Vendor Deposit Payment'
    GROUP BY
        jb.AccountID,
        jb.PaymentNo
),

/* ===============================================================
   12. VENDOR DEPOSIT APPLICATION ASSET CREDIT
   =============================================================== */
VendorDepositApplicationEntries AS
(
    SELECT
        MAX(jb.TransactionID) AS TransactionID,
        'Vendor Deposit Application' AS TransactionType,
        jb.AccountID,
        MAX(jb.AccountNo) AS AccountNo,
        MAX(jb.AccountName) AS AccountName,
        CAST(0 AS DECIMAL(18,2)) AS ItemTAX,
        CAST(0 AS DECIMAL(18,2)) AS Debit,
        SUM(jb.Credit) AS Credit,
        0 AS IsAPAccount,
        0 AS IsVATInput,
        CAST(NULL AS VARCHAR(100)) AS PaymentNo,
        MIN(jb.EntryID) AS FirstEntryID,

        MIN(
            COALESCE(
                jb.TransactionDate,
                jb.EntryDate,
                jb.CreateDate
            )
        ) AS PaymentDate

    FROM JournalBase AS jb

    WHERE jb.ExpenseTransactionType = 'Vendor Deposit Application'
      AND jb.IsAPAccount = 0
      AND jb.Credit > 0

    GROUP BY jb.AccountID
),

/* ===============================================================
   13. COMBINE DISPLAY ENTRIES
   =============================================================== */
NormalEntries AS
(
    SELECT
        TransactionID, TransactionType, AccountID, AccountNo,
        AccountName, ItemTAX, Debit, Credit, IsAPAccount,
        IsVATInput, PaymentNo, FirstEntryID, PaymentDate
    FROM PurchaseEntries

    UNION ALL

    SELECT
        TransactionID, TransactionType, AccountID, AccountNo,
        AccountName, ItemTAX, Debit, Credit, IsAPAccount,
        IsVATInput, PaymentNo, FirstEntryID, PaymentDate
    FROM PurchaseAPEntries

    UNION ALL

    SELECT
        TransactionID, TransactionType, AccountID, AccountNo,
        AccountName, ItemTAX, Debit, Credit, IsAPAccount,
        IsVATInput, PaymentNo, FirstEntryID, PaymentDate
    FROM PaymentEntries

    UNION ALL

    SELECT
        TransactionID, TransactionType, AccountID, AccountNo,
        AccountName, ItemTAX, Debit, Credit, IsAPAccount,
        IsVATInput, PaymentNo, FirstEntryID, PaymentDate
    FROM VendorDepositEntries

    UNION ALL

    SELECT
        TransactionID, TransactionType, AccountID, AccountNo,
        AccountName, ItemTAX, Debit, Credit, IsAPAccount,
        IsVATInput, PaymentNo, FirstEntryID, PaymentDate
    FROM VendorDepositPaymentEntries

    UNION ALL

    SELECT
        TransactionID, TransactionType, AccountID, AccountNo,
        AccountName, ItemTAX, Debit, Credit, IsAPAccount,
        IsVATInput, PaymentNo, FirstEntryID, PaymentDate
    FROM VendorDepositApplicationEntries
),

/* ===============================================================
   14. DISPLAY ORDER
   =============================================================== */
AllEntries AS
(
    SELECT
        ne.TransactionID,
        ne.TransactionType,
        ne.AccountID,
        ne.AccountNo,
        ne.AccountName,
        ne.ItemTAX,
        ne.Debit,
        ne.Credit,
        ne.IsAPAccount,
        ne.IsVATInput,
        ne.PaymentNo,
        ne.FirstEntryID,
        ne.PaymentDate,

        CASE
            WHEN ne.TransactionType = 'Purchase'
                 AND ne.Debit > 0
                 AND ne.IsVATInput = 0
                 AND ne.IsAPAccount = 0
                 AND ne.AccountName <> 'Purchase Discount'
            THEN 10

            WHEN ne.TransactionType = 'Purchase'
                 AND ne.Debit > 0
                 AND ne.IsVATInput = 1
            THEN 20

            WHEN ne.TransactionType = 'Purchase'
                 AND ne.Credit > 0
                 AND ne.AccountName = 'Purchase Discount'
            THEN 30

            WHEN ne.TransactionType = 'Purchase'
                 AND ne.IsAPAccount = 1
            THEN 40

            WHEN ne.TransactionType = 'Payment'
                 AND ne.Credit > 0
            THEN 50

            WHEN ne.TransactionType = 'Vendor Deposit'
            THEN 60

            WHEN ne.TransactionType = 'Vendor Deposit Payment'
            THEN 70

            WHEN ne.TransactionType = 'Vendor Deposit Application'
                 AND ne.Credit > 0
            THEN 80

            ELSE 99
        END AS DisplayOrder

    FROM NormalEntries AS ne
)

/* ===============================================================
   15. FINAL RESULT
   =============================================================== */
SELECT
    TransactionID,
    TransactionType,
    AccountID,
    AccountNo,
    AccountName,
    ItemTAX,
    Debit,
    Credit,
    IsAPAccount,
    IsVATInput,
    PaymentNo

FROM AllEntries

WHERE Debit <> 0
   OR Credit <> 0

ORDER BY
    CASE
        WHEN TransactionType = 'Purchase' THEN 1
        WHEN TransactionType = 'Payment' THEN 2
        WHEN TransactionType = 'Vendor Deposit' THEN 3
        WHEN TransactionType = 'Vendor Deposit Payment' THEN 4
        WHEN TransactionType = 'Vendor Deposit Application' THEN 5
        ELSE 99
    END,
    DisplayOrder,
    CASE
        WHEN IsVATInput = 1 THEN ItemTAX
        ELSE 0
    END,
    FirstEntryID,
    AccountNo;
";
                using (SqlCommand cmd = new SqlCommand(journalSql, conn))
                {
                    cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = expenseId;

                    cmd.Parameters.Add("@PaymentNo", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrWhiteSpace(PaymentNo)
                            ? (object)DBNull.Value
                            : PaymentNo.Trim();

                    decimal totalAPDebit = 0m;
                    decimal totalAPCredit = 0m;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal itemTax =
                                reader["ItemTAX"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(reader["ItemTAX"]);

                            decimal debit =
                                reader["Debit"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(reader["Debit"]);

                            decimal credit =
                                reader["Credit"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(reader["Credit"]);

                            string accountName =
                                reader["AccountName"]?.ToString() ?? "";

                            bool isAPAccount =
                                reader["IsAPAccount"] != DBNull.Value &&
                                Convert.ToInt32(reader["IsAPAccount"]) == 1;

                            /* =====================================================
                               TOTAL A/P

                               For selected payment:

                                   Credit 5350
                                   Debit  350

                               Remaining:

                                   5350 - 350 = 5000
                               ===================================================== */
                            if (isAPAccount)
                            {
                                totalAPDebit += debit;
                                totalAPCredit += credit;
                            }

                            model.JournalEntries.Add(new JournalEntryVM
                            {
                                ItemTAX = itemTax,

                                Transaction =
                                    reader["TransactionType"]?.ToString() ?? "",

                                AccountNo =
                                    reader["AccountNo"]?.ToString() ?? "",

                                AccountName =
                                    itemTax > 0
                                        ? $"{accountName} – {itemTax:0.##}%"
                                        : accountName,

                                Debit = debit,

                                Credit = credit
                            });
                        }
                    }

                    /* =============================================================
                       DO NOT calculate RemainingAP here from ONLY the displayed
                       journal rows.

                       RemainingAP is calculated below using ALL payments up to
                       the selected PaymentNo.
                       ============================================================= */
                }
                string remainingApSql = @"
;WITH ExpenseTotal AS
(
    /* =========================================================
       ORIGINAL EXPENSE A/P TOTAL
       ========================================================= */
    SELECT
        ISNULL(SUM(je.Credit), 0) AS TotalAP
    FROM TblAccount_JournalEntries AS je

    INNER JOIN TblChartOfAccount AS coa
        ON je.AccountID = coa.ChartOfAccountID

    WHERE je.ExpenseID = @ExpenseID
      AND je.Type IN ('Expense', 'Bill')

      AND
      (
            coa.AccountName = 'Accounts Payable (A/P)'
            OR coa.AccountName = 'Account Payable (A/P)'
            OR coa.AccountName LIKE '%Accounts Payable%'
            OR coa.AccountName LIKE '%Account Payable%'
            --OR coa.AccountName LIKE '%Payable%'
      )
),

/* =============================================================
   FIND SELECTED PAYMENT TRANSACTION

   IMPORTANT:
   DO NOT USE EntryID AS THE PAYMENT SEQUENCE.

   One payment contains multiple journal entries:

       Payment 28292
           Entry 51086 = A/P
           Entry 51087 = Bank

   Therefore we first find the TransactionID of the selected
   PaymentNo.
   ============================================================= */
SelectedPayment AS
(
    SELECT
        MIN(je.TransactionID) AS SelectedTransactionID
    FROM TblAccount_JournalEntries AS je

    WHERE je.ExpenseID = @ExpenseID
      AND je.Type = 'Bill Payment'
      AND je.PaymentNo = @PaymentNo
),

/* =============================================================
   ALL PAYMENTS UP TO SELECTED PAYMENT

   Example:

       PAY260830-00200
           TransactionID = 28290
           Paid = 50

       PAY260830-00201
           TransactionID = 28292
           Paid = 300

   Selected:
       PAY260830-00201

   Therefore:

       50 + 300 = 350
   ============================================================= */
PaidToSelected AS
(
    SELECT
        ISNULL(SUM(je.Debit), 0) AS PaidAmount

    FROM TblAccount_JournalEntries AS je

    INNER JOIN TblChartOfAccount AS coa
        ON je.AccountID = coa.ChartOfAccountID

    CROSS JOIN SelectedPayment AS sp

    WHERE je.ExpenseID = @ExpenseID
      AND je.Type = 'Bill Payment'

      /* =====================================================
         A/P ACCOUNT ONLY

         Payment has:

             A/P     Debit
             Bank    Credit

         We only calculate the A/P Debit as paid amount.
         ===================================================== */
      AND
      (
            coa.AccountName = 'Accounts Payable (A/P)'
            OR coa.AccountName = 'Account Payable (A/P)'
            OR coa.AccountName LIKE '%Accounts Payable%'
            OR coa.AccountName LIKE '%Account Payable%'
            --OR coa.AccountName LIKE '%Payable%'
      )

      /* =====================================================
         USE TRANSACTION ID

         This includes:

             Previous payments
             +
             Current selected payment

         Example for PAY260830-00201:

             28290 <= 28292
             28292 <= 28292

             50 + 300 = 350
         ===================================================== */
      AND je.TransactionID <= sp.SelectedTransactionID
)

/* =============================================================
   FINAL REMAINING A/P
   ============================================================= */
SELECT
    e.TotalAP,
    p.PaidAmount,

    CASE
        WHEN e.TotalAP - p.PaidAmount < 0
        THEN 0
        ELSE e.TotalAP - p.PaidAmount
    END AS RemainingAP

FROM ExpenseTotal AS e

CROSS JOIN PaidToSelected AS p;
";

                using (SqlCommand remainingCmd = new SqlCommand(remainingApSql, conn))
                {
                    remainingCmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value =
                        expenseId;

                    remainingCmd.Parameters.Add("@PaymentNo", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrWhiteSpace(PaymentNo)
                            ? (object)DBNull.Value
                            : PaymentNo.Trim();

                    using (SqlDataReader remainingReader = remainingCmd.ExecuteReader())
                    {
                        if (remainingReader.Read())
                        {
                            decimal totalAP =
                                remainingReader["TotalAP"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(remainingReader["TotalAP"]);

                            decimal paidAmount =
                                remainingReader["PaidAmount"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(remainingReader["PaidAmount"]);

                            decimal remainingAP =
                                remainingReader["RemainingAP"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(remainingReader["RemainingAP"]);

                            model.RemainingAP = remainingAP;
                        }
                        else
                        {
                            model.RemainingAP = 0m;
                        }
                    }
                }

                #endregion
            }

            #region 4️⃣ Amount in Words
            model.AmountInWords = NumberToWords(model.PaidAmount > 0 ? model.PaidAmount : model.GrandTotal);
            //model.AmountInWords = NumberToWords(model.GrandTotal);
            //model.AmountInWords = NumberToWords(model.GrandTotal);

            #endregion

            return model;
        }


        private static string NumberToWords(decimal amount)
        {
            amount = Math.Round(amount, 2);

            if (amount == 0)
                return "Zero Dollars Only";

            long dollars = (long)amount;
            int cents = (int)((amount - dollars) * 100);

            string dollarWords = ConvertIntToWords(dollars);
            string result = "";

            if (dollars > 0)
            {
                result += dollarWords + (dollars == 1 ? " Dollar" : " Dollars");
            }

            if (cents > 0)
            {
                if (dollars > 0)
                    result += " and ";

                result += ConvertIntToWords(cents) + (cents == 1 ? " Cent" : " Cents");
            }

            return result + " Only";
        }

        private static string ConvertIntToWords(long number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + ConvertIntToWords(Math.Abs(number));

            string words = "";

            if (number / 1000000000 > 0)
            {
                words += ConvertIntToWords(number / 1000000000) + " Billion ";
                number %= 1000000000;
            }

            if (number / 1000000 > 0)
            {
                words += ConvertIntToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }

            if (number / 1000 > 0)
            {
                words += ConvertIntToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if (number / 100 > 0)
            {
                words += ConvertIntToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "";

                string[] unitsMap =
                {
            "Zero","One","Two","Three","Four","Five","Six","Seven","Eight","Nine",
            "Ten","Eleven","Twelve","Thirteen","Fourteen","Fifteen",
            "Sixteen","Seventeen","Eighteen","Nineteen"
        };

                string[] tensMap =
                {
            "Zero","Ten","Twenty","Thirty","Forty",
            "Fifty","Sixty","Seventy","Eighty","Ninety"
        };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if (number % 10 > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }


        private void InsertVendorBillAPEntry(
            Expense expense,
            SqlConnection conn,
            SqlTransaction tran)
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            if (conn == null)
                throw new ArgumentNullException(nameof(conn));

            if (tran == null)
                throw new ArgumentNullException(nameof(tran));

            if (conn.State != ConnectionState.Open)
                throw new InvalidOperationException(
                    "The SQL connection must be open.");

            if (expense.ExpenseID <= 0)
                throw new InvalidOperationException(
                    "ExpenseID is required.");

            if (expense.APAccountID <= 0)
                throw new InvalidOperationException(
                    "A/P AccountID is required.");

            DateTime transactionDate =
                expense.TransactionDate == default(DateTime)
                    ? DateTime.Now
                    : expense.TransactionDate;

            const string sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE
    @TransactionID INT,
    @JournalNo NVARCHAR(50),
    @AccountNo NVARCHAR(50),
    @AccountName NVARCHAR(200),
    @BillAmount DECIMAL(18,2),
    @DetailDebit DECIMAL(18,2),
    @DetailCredit DECIMAL(18,2),
    @ExistingAPCount INT;

------------------------------------------------------------
-- 1. FIND THE EXISTING VENDOR BILL TRANSACTION
------------------------------------------------------------
SELECT TOP (1)
    @TransactionID = t.TransactionID,
    @JournalNo = t.JournalNo,
    @BillAmount = t.Amount
FROM dbo.TblAccount_Transaction t WITH (UPDLOCK, HOLDLOCK)
WHERE t.ExpenseID = @ExpenseID
  AND t.Type = 'Bill'
  AND t.ExpenseTransactionType = 'Bill'
  AND t.SystemID = @SystemID
  AND t.WarehouseID = @WarehouseID
  AND (
        t.DomainID = @DomainID
        OR (t.DomainID IS NULL AND @DomainID IS NULL)
      )
ORDER BY t.TransactionID DESC;

IF @TransactionID IS NULL
    THROW 50001,
        'Vendor Bill transaction was not found.',
        1;

IF ISNULL(@JournalNo, '') = ''
    THROW 50002,
        'Vendor Bill JournalNo is missing.',
        1;

IF ISNULL(@BillAmount, 0) <= 0
    THROW 50003,
        'Vendor Bill amount must be greater than zero.',
        1;

------------------------------------------------------------
-- 2. VALIDATE THE A/P ACCOUNT
------------------------------------------------------------
SELECT
    @AccountNo = coa.AccountNo,
    @AccountName = coa.AccountName
FROM dbo.TblChartOfAccount coa
WHERE coa.ChartOfAccountID = @APAccountID
  AND coa.SystemID = @SystemID
  AND coa.WarehouseID = @WarehouseID;

IF @AccountName IS NULL
    THROW 50004,
        'A/P account was not found for this system/warehouse.',
        1;

------------------------------------------------------------
-- 3. CHECK THE EXISTING BILL DETAIL JOURNAL ENTRIES
--
-- Expected expense details:
-- Debit 400 + Debit 900 = 1300
-- Credit = 0
------------------------------------------------------------
SELECT
    @DetailDebit = ISNULL(SUM(je.Debit), 0),
    @DetailCredit = ISNULL(SUM(je.Credit), 0)
FROM dbo.TblAccount_JournalEntries je
WHERE je.TransactionID = @TransactionID
  AND je.JournalNo = @JournalNo
  AND je.Type = 'Bill'
  AND je.ExpenseTransactionType = 'Bill'
  AND je.AccountID <> @APAccountID;

    IF ABS(@DetailDebit - @BillAmount) > 0.01
        THROW 50005,
            'Bill detail debit total does not match the bill amount.',
            1;

IF ABS(@DetailCredit) > 0.01
    THROW 50006,
        'Unexpected credit exists in the bill detail entries.',
        1;

------------------------------------------------------------
-- 4. PREVENT A DUPLICATE A/P ENTRY
------------------------------------------------------------
SELECT @ExistingAPCount = COUNT(*)
FROM dbo.TblAccount_JournalEntries je WITH (UPDLOCK, HOLDLOCK)
WHERE je.TransactionID = @TransactionID
  AND je.JournalNo = @JournalNo
  AND je.AccountID = @APAccountID
  AND je.Type = 'Bill'
  AND je.ExpenseTransactionType = 'Bill';

IF @ExistingAPCount > 0
    THROW 50007,
        'A/P entry already exists for this Vendor Bill.',
        1;

------------------------------------------------------------
-- 5. INSERT THE A/P CREDIT ENTRY
--
-- Dr Expense details: 1300
-- Cr Accounts Payable: 1300
--
-- No payment transaction.
-- No bank journal entry.
------------------------------------------------------------
INSERT INTO dbo.TblAccount_JournalEntries
(
    Type,
    TransactionID,
    AccountID,
    Description,
    Debit,
    Credit,
    EntryDate,
    DomainID,
    TransactionDate,
    SystemID,
    WarehouseID,
    CreateByUserID,
    CreateDate,
    ExpenseID,
    ExpenseAmount,
    ItemTAX,
    ClassNameID,
    IsPosted,
    JournalNo,
    ExpenseNo,
    PaymentNo,
    InvoiceNo,
    ExpenseTransactionType,
    Account
)
VALUES
(
    'Bill',
    @TransactionID,
    @APAccountID,
    @Description,
    0,
    @BillAmount,
    @TransactionDate,
    @DomainID,
    @TransactionDate,
    @SystemID,
    @WarehouseID,
    @CreateByUserID,
    GETDATE(),
    @ExpenseID,
    @BillAmount,
    0,
    @ClassNameID,
    0,
    @JournalNo,
    @ExpenseNo,
    NULL,
    @ExpenseNo,
    'Bill',
    'Bill-' + @AccountNo + '-' + @AccountName
);

------------------------------------------------------------
-- 6. VERIFY THE FINAL BILL JOURNAL BALANCES
------------------------------------------------------------
DECLARE
    @TotalDebit DECIMAL(18,2),
    @TotalCredit DECIMAL(18,2);

SELECT
    @TotalDebit = ISNULL(SUM(je.Debit), 0),
    @TotalCredit = ISNULL(SUM(je.Credit), 0)
FROM dbo.TblAccount_JournalEntries je
WHERE je.TransactionID = @TransactionID
  AND je.JournalNo = @JournalNo
  AND je.Type = 'Bill'
  AND je.ExpenseTransactionType = 'Bill';

IF ABS(@TotalDebit - @TotalCredit) > 0.01
    THROW 50008,
        'Vendor Bill journal entries are not balanced.',
        1;
";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value =
                    expense.ExpenseID;

                cmd.Parameters.Add("@APAccountID", SqlDbType.Int).Value =
                    expense.PaymentAccountID;

                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value =
                    expense.SystemID;

                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value =
                    expense.WarehouseID;

                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar).Value =
                    string.IsNullOrWhiteSpace(expense.DomainID)
                        ? (object)DBNull.Value
                        : expense.DomainID;

                cmd.Parameters.Add("@TransactionDate", SqlDbType.DateTime).Value =
                    transactionDate;

                cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int).Value =
                    expense.CreateByUserID;

                cmd.Parameters.Add("@ClassNameID", SqlDbType.Int).Value =
                    expense.ClassNameID;

                cmd.Parameters.Add("@ExpenseNo", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(expense.ExpenseNo)
                        ? (object)DBNull.Value
                        : expense.ExpenseNo;

                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value =
                    $"Accounts Payable - {expense.ExpenseNo}";

                cmd.ExecuteNonQuery();
            }
        }

        private decimal ApplyVendorDeposit(
    int APAccountID,
    int vendorDepositID,
    int billExpenseID,
    decimal amountToApply,
    int systemID,
    int warehouseID,
    string domainID,
    int createByUserID,
    SqlConnection conn,
    SqlTransaction tran,
    int existingExpense)
        {
            if (conn == null)
                throw new ArgumentNullException(nameof(conn));

            if (tran == null)
                throw new ArgumentNullException(nameof(tran));

            if (conn.State != ConnectionState.Open)
                throw new InvalidOperationException("SQL connection must be open.");

            if (APAccountID <= 0)
                throw new ArgumentException("A/P AccountID is required.");

            if (vendorDepositID <= 0)
                throw new ArgumentException("VendorDepositID is required.");

            if (billExpenseID <= 0)
                throw new ArgumentException("Bill ExpenseID is required.");

            if (amountToApply < 0)
                throw new ArgumentException(
                    "AmountToApply must be greater than zero.");

            if (systemID <= 0 || warehouseID <= 0)
                throw new ArgumentException(
                    "SystemID and WarehouseID are required.");

            const string sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE
    @DepositVendorID INT,
    @DepositAmount DECIMAL(18,2),
    @DepositAppliedAmount DECIMAL(18,2),
    @DepositRemainingAmount DECIMAL(18,2),
    @DepositAccountID INT,
    @DepositNo NVARCHAR(100),

    @BillTransactionID INT,
    @BillVendorID INT,
    @BillAmount DECIMAL(18,2),
    @BillJournalNo NVARCHAR(50),
    @BillInvoiceNo NVARCHAR(100),

    @AccountNo NVARCHAR(50),
    @AccountName NVARCHAR(200),

    @OutstandingAP DECIMAL(18,2),
    @ApplyAmount DECIMAL(18,2),

    @ApplicationJournalNo NVARCHAR(50),
    @ApplicationDate DATETIME,

    @NewAppliedAmount DECIMAL(18,2),
    @NewRemainingAmount DECIMAL(18,2),

    @OldApplicationCount INT;

------------------------------------------------------------
-- 0. PREVENT INVALID EDIT
------------------------------------------------------------

IF @AmountToApply < 0
    THROW 50100,
        'AmountToApply must be greater than zero.',
        1;

------------------------------------------------------------
-- 1. REVERSE PREVIOUS APPLICATIONS
--
-- Important:
-- Restore every previous deposit used by this bill.
-- This allows the user to change:
--   500 -> 300
--   500 -> 800
--   Deposit A -> Deposit B
------------------------------------------------------------

DECLARE @OldApplications TABLE
(
    VendorDepositID INT NOT NULL,
    AppliedAmount DECIMAL(18,2) NOT NULL,
    JournalNo NVARCHAR(50) NOT NULL
);

IF ISNULL(@ExistingExpenseID, 0) > 0
BEGIN

    INSERT INTO @OldApplications
    (
        VendorDepositID,
        AppliedAmount,
        JournalNo
    )
    SELECT
        VendorDepositID,
        AppliedAmount,
        JournalNo
    FROM dbo.TblVendorDepositApplication
        WITH (UPDLOCK, HOLDLOCK)
    WHERE ExpenseID = @ExistingExpenseID
      AND SystemID = @SystemID
      AND (
            DomainID = @DomainID
            OR (DomainID IS NULL AND @DomainID IS NULL)
          );

    SELECT @OldApplicationCount = COUNT(*)
    FROM @OldApplications;

    --------------------------------------------------------
    -- 1.1 RESTORE OLD DEPOSIT BALANCES
    --------------------------------------------------------

    ;WITH OldDepositTotals AS
    (
        SELECT
            VendorDepositID,
            SUM(AppliedAmount) AS TotalRestored
        FROM @OldApplications
        GROUP BY VendorDepositID
    ),
    Restored AS
    (
        SELECT
            D.VendorDepositID,
            D.AppliedAmount,
            D.Amount,
            O.TotalRestored,

            ISNULL(D.AppliedAmount, 0)
                - O.TotalRestored AS RestoredAppliedAmount

        FROM dbo.TblVendorDeposit D
            WITH (UPDLOCK, HOLDLOCK)

        INNER JOIN OldDepositTotals O
            ON O.VendorDepositID = D.VendorDepositID
    )
    UPDATE D
    SET
        AppliedAmount = R.RestoredAppliedAmount,

        Status =
            CASE
                WHEN R.RestoredAppliedAmount <= 0.005
                    THEN 'Open'

                WHEN R.RestoredAppliedAmount >= R.Amount - 0.005
                    THEN 'Applied'

                ELSE 'Partially Applied'
            END,

        UpdateByUserID = @CreateByUserID,
        UpdateDate = GETDATE()

    FROM dbo.TblVendorDeposit D
    INNER JOIN Restored R
        ON R.VendorDepositID = D.VendorDepositID;

    --------------------------------------------------------
    -- 1.2 DELETE OLD APPLICATION JOURNAL ENTRIES
    --------------------------------------------------------

    DELETE JE
    FROM dbo.TblAccount_JournalEntries JE
    INNER JOIN @OldApplications OA
        ON OA.JournalNo = JE.JournalNo
    WHERE JE.ExpenseID = @ExistingExpenseID
      AND JE.ExpenseTransactionType =
            'Vendor Deposit Application'
      AND JE.SystemID = @SystemID
      AND JE.WarehouseID = @WarehouseID
      AND (
            JE.DomainID = @DomainID
            OR (JE.DomainID IS NULL AND @DomainID IS NULL)
          );

    --------------------------------------------------------
    -- 1.3 DELETE OLD APPLICATION RECORDS
    --------------------------------------------------------

    DELETE DA
    FROM dbo.TblVendorDepositApplication DA
    WHERE DA.ExpenseID = @ExistingExpenseID
      AND DA.SystemID = @SystemID
      AND (
            DA.DomainID = @DomainID
            OR (DA.DomainID IS NULL AND @DomainID IS NULL)
          );

    --------------------------------------------------------
    -- 1.4 VERIFY REVERSAL
    --------------------------------------------------------

    IF EXISTS
    (
        SELECT 1
        FROM dbo.TblVendorDepositApplication
        WHERE ExpenseID = @ExistingExpenseID
          AND SystemID = @SystemID
          AND (
                DomainID = @DomainID
                OR (DomainID IS NULL AND @DomainID IS NULL)
              )
    )
        THROW 50121,
            'Previous vendor deposit applications could not be removed.',
            1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.TblAccount_JournalEntries
        WHERE ExpenseID = @ExistingExpenseID
          AND ExpenseTransactionType =
                'Vendor Deposit Application'
          AND SystemID = @SystemID
          AND WarehouseID = @WarehouseID
          AND (
                DomainID = @DomainID
                OR (DomainID IS NULL AND @DomainID IS NULL)
              )
    )
        THROW 50122,
            'Previous vendor deposit application journals could not be removed.',
            1;

END;

------------------------------------------------------------
-- 2. LOCK AND LOAD THE SELECTED DEPOSIT
------------------------------------------------------------

SELECT
    @DepositVendorID = D.VendorID,
    @DepositAmount = D.Amount,
    @DepositAppliedAmount = ISNULL(D.AppliedAmount, 0),
    @DepositRemainingAmount = D.RemainingAmount,
    @DepositAccountID = D.AccountID,
    @DepositNo = D.DepositNo

FROM dbo.TblVendorDeposit D
    WITH (UPDLOCK, HOLDLOCK)

WHERE D.VendorDepositID = @VendorDepositID
  AND D.SystemID = @SystemID
  AND D.WarehouseID = @WarehouseID
  AND (
        D.DomainID = @DomainID
        OR (D.DomainID IS NULL AND @DomainID IS NULL)
      );

IF @DepositVendorID IS NULL
    THROW 50101,
        'Vendor deposit was not found.',
        1;

------------------------------------------------------------
-- 3. VALIDATE DEPOSIT
------------------------------------------------------------

IF @DepositAmount <= 0
    THROW 50102,
        'Vendor deposit amount must be greater than zero.',
        1;

IF @DepositAppliedAmount < 0
   OR @DepositAppliedAmount > @DepositAmount
    THROW 50105,
        'Vendor deposit applied amount is invalid.',
        1;

SET @DepositRemainingAmount =
    @DepositAmount - @DepositAppliedAmount;

IF @DepositRemainingAmount <= 0
    THROW 50107,
        'Vendor deposit has no remaining amount.',
        1;

------------------------------------------------------------
-- 4. LOCK AND LOAD THE VENDOR BILL
------------------------------------------------------------

SELECT TOP (1)
    @BillTransactionID = T.TransactionID,

    @BillVendorID =
        COALESCE(NULLIF(T.VendorID, 0), NULLIF(T.PayeeID, 0)),

    @BillAmount = T.Amount,
    @BillJournalNo = T.JournalNo,
    @BillInvoiceNo = T.InvoiceNo

FROM dbo.TblAccount_Transaction T
    WITH (UPDLOCK, HOLDLOCK)

WHERE T.ExpenseID = @BillExpenseID
  AND T.Type = 'Bill'
  AND T.ExpenseTransactionType = 'Bill'
  AND T.AccountID = @APAccountID
  AND T.SystemID = @SystemID
  AND T.WarehouseID = @WarehouseID
  AND (
        T.DomainID = @DomainID
        OR (T.DomainID IS NULL AND @DomainID IS NULL)
      )

ORDER BY T.TransactionID DESC;

IF @BillTransactionID IS NULL
    THROW 50108,
        'Vendor bill transaction was not found.',
        1;

IF @BillVendorID IS NULL
    THROW 50109,
        'Vendor ID is missing from the bill.',
        1;

IF @BillVendorID <> @DepositVendorID
    THROW 50110,
        'Vendor deposit and bill belong to different vendors.',
        1;

IF ISNULL(@BillAmount, 0) <= 0
    THROW 50111,
        'Vendor bill amount must be greater than zero.',
        1;

IF ISNULL(@BillJournalNo, '') = ''
    THROW 50112,
        'Vendor bill JournalNo is missing.',
        1;

------------------------------------------------------------
-- 5. VALIDATE A/P ACCOUNT
------------------------------------------------------------

SELECT
    @AccountNo = COA.AccountNo,
    @AccountName = COA.AccountName

FROM dbo.TblChartOfAccount COA

WHERE COA.ChartOfAccountID = @APAccountID
  AND COA.SystemID = @SystemID
  AND COA.WarehouseID = @WarehouseID;

IF @AccountName IS NULL
    THROW 50114,
        'A/P account was not found.',
        1;

------------------------------------------------------------
-- 6. VALIDATE DEPOSIT ASSET ACCOUNT
------------------------------------------------------------

IF @DepositAccountID IS NULL
    THROW 50115,
        'Vendor deposit asset AccountID is missing.',
        1;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.TblChartOfAccount COA
    WHERE COA.ChartOfAccountID = @DepositAccountID
      AND COA.SystemID = @SystemID
      AND COA.WarehouseID = @WarehouseID
)
    THROW 50123,
        'Vendor deposit asset account was not found.',
        1;

------------------------------------------------------------
-- 7. RECALCULATE OUTSTANDING A/P
--
-- This happens AFTER previous application journals
-- have been reversed and deleted.
------------------------------------------------------------

SELECT
    @OutstandingAP =
        ISNULL(SUM(JE.Credit - JE.Debit), 0)

FROM dbo.TblAccount_JournalEntries JE

WHERE JE.TransactionID = @BillTransactionID
  AND JE.ExpenseID = @BillExpenseID
  AND JE.AccountID = @APAccountID
  AND JE.SystemID = @SystemID
  AND JE.WarehouseID = @WarehouseID
  AND (
        JE.DomainID = @DomainID
        OR (JE.DomainID IS NULL AND @DomainID IS NULL)
      );

IF @OutstandingAP <= 0
    THROW 50116,
        'This bill has no outstanding A/P balance.',
        1;

------------------------------------------------------------
-- 8. USE EXACT REQUESTED AMOUNT
--
-- Do not silently reduce AmountToApply.
------------------------------------------------------------

SET @ApplyAmount = ROUND(@AmountToApply, 2);

IF @ApplyAmount < 0
    THROW 50117,
        'Application amount must be greater than zero.',
        1;

IF @ApplyAmount > @DepositRemainingAmount + 0.005
    THROW 50124,
        'AmountToApply exceeds the available vendor deposit balance.',
        1;

IF @ApplyAmount > @OutstandingAP + 0.005
    THROW 50125,
        'AmountToApply exceeds the outstanding bill balance.',
        1;

------------------------------------------------------------
-- 9. GENERATE APPLICATION JOURNAL NUMBER
------------------------------------------------------------

SET @ApplicationDate = GETDATE();

SET @ApplicationJournalNo =
    'VDA'
    + CONVERT(CHAR(6), @ApplicationDate, 12)
    + '_'
    + RIGHT(
        REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''),
        10
      );

------------------------------------------------------------
-- 10. INSERT APPLICATION RECORD
------------------------------------------------------------

INSERT INTO dbo.TblVendorDepositApplication
(
    VendorDepositID,
    VendorBillTransactionID,
    AppliedAmount,
    ApplicationDate,
    JournalNo,
    DomainID,
    SystemID,
    CreateByUserID,
    CreateDate,
    ExpenseID
)
VALUES
(
    @VendorDepositID,
    @BillTransactionID,
    @ApplyAmount,
    @ApplicationDate,
    @ApplicationJournalNo,
    @DomainID,
    @SystemID,
    @CreateByUserID,
    GETDATE(),
    @BillExpenseID
);

------------------------------------------------------------
-- 11. DEBIT ACCOUNTS PAYABLE
------------------------------------------------------------

INSERT INTO dbo.TblAccount_JournalEntries
(
    Type,
    TransactionID,
    AccountID,
    Description,
    Debit,
    Credit,
    EntryDate,
    DomainID,
    TransactionDate,
    SystemID,
    WarehouseID,
    CreateByUserID,
    CreateDate,
    ExpenseID,
    ExpenseAmount,
    ItemTAX,
    ClassNameID,
    IsPosted,
    JournalNo,
    ExpenseNo,
    PaymentNo,
    InvoiceNo,
    ExpenseTransactionType,
    Account
)
SELECT
    'Bill',
    @BillTransactionID,
    @APAccountID,
    'Vendor Deposit Applied - ' + ISNULL(@DepositNo, ''),
    @ApplyAmount,
    0,
    @ApplicationDate,
    @DomainID,
    @ApplicationDate,
    @SystemID,
    @WarehouseID,
    @CreateByUserID,
    GETDATE(),
    @BillExpenseID,
    @BillAmount,
    0,
    T.ClassNameID,
    0,
    @ApplicationJournalNo,
    T.ExpenseNo,
    NULL,
    @BillInvoiceNo,
    'Vendor Deposit Application',
    'Bill-' + @AccountNo + '-' + @AccountName

FROM dbo.TblAccount_Transaction T
WHERE T.TransactionID = @BillTransactionID;

------------------------------------------------------------
-- 12. CREDIT VENDOR DEPOSIT ASSET
------------------------------------------------------------

INSERT INTO dbo.TblAccount_JournalEntries
(
    Type,
    TransactionID,
    AccountID,
    Description,
    Debit,
    Credit,
    EntryDate,
    DomainID,
    TransactionDate,
    SystemID,
    WarehouseID,
    CreateByUserID,
    CreateDate,
    ExpenseID,
    ExpenseAmount,
    ItemTAX,
    ClassNameID,
    IsPosted,
    JournalNo,
    ExpenseNo,
    PaymentNo,
    InvoiceNo,
    ExpenseTransactionType,
    Account
)
SELECT
    'Bill',
    @BillTransactionID,
    @DepositAccountID,
    'Vendor Deposit Used - ' + ISNULL(@DepositNo, ''),
    0,
    @ApplyAmount,
    @ApplicationDate,
    @DomainID,
    @ApplicationDate,
    @SystemID,
    @WarehouseID,
    @CreateByUserID,
    GETDATE(),
    @BillExpenseID,
    @BillAmount,
    0,
    T.ClassNameID,
    0,
    @ApplicationJournalNo,
    T.ExpenseNo,
    NULL,
    @BillInvoiceNo,
    'Vendor Deposit Application',
    'Vendor Deposit - ' + ISNULL(@DepositNo, '')

FROM dbo.TblAccount_Transaction T
WHERE T.TransactionID = @BillTransactionID;

------------------------------------------------------------
-- 13. UPDATE DEPOSIT APPLIED AMOUNT
--
-- RemainingAmount is calculated from Amount and
-- AppliedAmount. Do not update it directly.
------------------------------------------------------------

SET @NewAppliedAmount =
    @DepositAppliedAmount + @ApplyAmount;

SET @NewRemainingAmount =
    @DepositAmount - @NewAppliedAmount;

IF @NewAppliedAmount > @DepositAmount + 0.005
    THROW 50119,
        'Applied amount cannot exceed original deposit amount.',
        1;

UPDATE dbo.TblVendorDeposit
SET
    AppliedAmount = @NewAppliedAmount,

    Status =
        CASE
            WHEN @NewRemainingAmount <= 0.005
                THEN 'Applied'
            ELSE 'Partially Applied'
        END,

    UpdateByUserID = @CreateByUserID,
    UpdateDate = GETDATE()

WHERE VendorDepositID = @VendorDepositID
  AND SystemID = @SystemID
  AND WarehouseID = @WarehouseID
  AND (
        DomainID = @DomainID
        OR (DomainID IS NULL AND @DomainID IS NULL)
      );

IF @@ROWCOUNT <> 1
    THROW 50118,
        'Failed to update vendor deposit.',
        1;

------------------------------------------------------------
-- 14. VERIFY JOURNAL BALANCE
------------------------------------------------------------

DECLARE
    @ApplicationDebit DECIMAL(18,2),
    @ApplicationCredit DECIMAL(18,2);

SELECT
    @ApplicationDebit = ISNULL(SUM(Debit), 0),
    @ApplicationCredit = ISNULL(SUM(Credit), 0)

FROM dbo.TblAccount_JournalEntries

WHERE JournalNo = @ApplicationJournalNo
  AND ExpenseID = @BillExpenseID
  AND ExpenseTransactionType = 'Vendor Deposit Application';

IF ABS(@ApplicationDebit - @ApplicationCredit) > 0.01
    THROW 50126,
        'Vendor deposit application journal is not balanced.',
        1;

------------------------------------------------------------
-- 15. RETURN ACTUAL APPLIED AMOUNT
------------------------------------------------------------

SELECT @ApplyAmount AS AppliedAmount;
";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 120;

                cmd.Parameters.Add("@APAccountID", SqlDbType.Int).Value =
                    APAccountID;

                cmd.Parameters.Add("@VendorDepositID", SqlDbType.Int).Value =
                    vendorDepositID;

                cmd.Parameters.Add("@BillExpenseID", SqlDbType.Int).Value =
                    billExpenseID;

                cmd.Parameters.Add("@ExistingExpenseID", SqlDbType.Int).Value =
                    existingExpense;

                SqlParameter amountParameter =
                    cmd.Parameters.Add("@AmountToApply", SqlDbType.Decimal);

                amountParameter.Precision = 18;
                amountParameter.Scale = 2;
                amountParameter.Value = amountToApply;

                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value =
                    systemID;

                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value =
                    warehouseID;

                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(domainID)
                        ? (object)DBNull.Value
                        : domainID;

                cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int).Value =
                    createByUserID;

                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    throw new InvalidOperationException(
                        "Vendor deposit application did not return an amount.");

                return Convert.ToDecimal(result);
            }
        }

        private void InsertExpensePayment(
    Expense expense,
    ExpensePostingResult postingResult,
    decimal paymentAmount,
    SqlConnection conn,
    SqlTransaction tran)
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            if (postingResult == null)
                throw new ArgumentNullException(nameof(postingResult));

            if (paymentAmount <= 0)
                return;

            // ============================================================
            // 1. BASIC VALIDATION
            // ============================================================

            int expenseId = expense.ExpenseID;
            int payableAccountId = expense.APAccountID;
            int paymentAccountId = expense.PaymentAccountID;

            if (expenseId <= 0)
                throw new Exception("ExpenseID is required.");

            //if (payableAccountId <= 0)
              //  throw new Exception("A/P AccountID is required.");

            //if (paymentAccountId <= 0)
              //  throw new Exception("Payment AccountID is required.");

            decimal totalExpense = expense.Total;

            if (totalExpense <= 0)
                throw new Exception("Expense amount must be greater than zero.");

            // ============================================================
            // 2. GET PREVIOUS PAYMENTS
            //
            // IMPORTANT:
            // Only Expense Payment transactions should be counted here.
            // Do NOT count the original Expense transaction.
            // ============================================================

            decimal previousPayments = 0m;

            const string previousPaymentSql = @"
SELECT ISNULL(SUM(ISNULL(t.Amount, 0)), 0)
FROM TblAccount_Transaction t
WHERE t.ExpenseID = @ExpenseID
  AND t.Type = 'Bill Payment'
  AND ISNULL(t.IsAdjustment, 0) = 0;";

            using (SqlCommand cmd = new SqlCommand(
                previousPaymentSql,
                conn,
                tran))
            {
                cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value =
                    expenseId;

                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                    previousPayments = Convert.ToDecimal(result);
            }

            // ============================================================
            // 3. CALCULATE REMAINING A/P
            // ============================================================

            decimal remainingAP = totalExpense - previousPayments;

            if (remainingAP <= 0)
            {
                throw new Exception(
                    $"Expense {expense.ExpenseNo} is already fully paid.");
            }

            if (paymentAmount > remainingAP)
            {
                throw new Exception(
                    $"Payment amount {paymentAmount:N2} is greater than " +
                    $"remaining A/P {remainingAP:N2}.");
            }

            decimal newRemainingAP = remainingAP - paymentAmount;

            // ============================================================
            // 4. GENERATE PAYMENT NUMBER
            // ============================================================

            string paymentNo = postingResult.PaymentNo;

            if (string.IsNullOrWhiteSpace(paymentNo))
                throw new Exception("Unable to generate PaymentNo.");

            // ============================================================
            // 7. CREATE PAYMENT TRANSACTION
            //
            // One payment = one TransactionID
            //
            // Example:
            //
            // TransactionID = 28051
            // ExpenseID     = 129
            // Amount        = 751
            // Type          = Bill Payment
            //
            // TransactionDateOnly is NOT inserted because it is computed.
            // ============================================================

            DateTime transactionDate =
                expense.TransactionDate == default(DateTime)
                    ? DateTime.Now
                    : expense.TransactionDate;

            int paymentTransactionId;

            string transactionSql = @"
DECLARE @NewTransactionID INT, @JournalNo nvarchar(50)";
            transactionSql += $@"
IF ({expense.ExistingExpenseID} = 0 AND '{expense.ExpenseTransactionType}' <> 'Vendor Deposit')";
            transactionSql += @"
BEGIN
    EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR] 'JV{yy}{MM}_{00000}', 'JournalNo', @SystemID, @WarehouseID, @JournalNo OUTPUT, 1, 0, @DomainID;
END
ELSE
BEGIN";
            transactionSql += $@"
    SELECT TOP 1 @JournalNo = JournalNo FROM TblAccount_Transaction WHERE (ExpenseID = case when {expense.ExistingExpenseID} = 0 then {expense.ExpenseID} else {expense.ExistingExpenseID} end) AND ExpenseTransactionType in('Bill Payment', 'Vendor Deposit');";
            transactionSql += @"
    IF(ISNULL(@JournalNo, '') = '')
    BEGIN
        EXEC [dbo].[SP_INSERT_PREFIX_BASE_ON_YEAR] 'JV{yy}{MM}_{00000}', 'JournalNo', @SystemID, @WarehouseID, @JournalNo OUTPUT, 1, 0, @DomainID;
    END
";
transactionSql += $@"

END;
DECLARE @VDJournalNo nvarchar(50);
IF (@ExpenseTransactionType = 'Vendor Deposit')
BEGIN
    SELECT TOP 1 @VDJournalNo = JournalNo, @NewTransactionID = TransactionID FROM TblAccount_Transaction WHERE ExpenseID = {expense.ExpenseID} AND ExpenseTransactionType in('Vendor Deposit');
END
ELSE
BEGIN
    INSERT INTO TblAccount_Transaction
    (
        ExpensesID,
        PayeeID,
        VendorID,
        EmployeeID,
        CustomerID,

        TransactionTypeID,
        TransactionType,
        Type,

        InvoiceDate,
        TransactionDate,

        Description,
        Amount,
        Currency,

        SystemID,
        WarehouseID,
        DomainID,

        CreateByUserID,
        CreateDate,

        ExpenseNo,
        ExpenseID,

        FromAccountID,
        ToAccountID,

        ClassNameID,
        IsPosted,

        JournalNo,
        PaymentNo,
        InvoiceNo, ExpenseTransactionType
    )
    VALUES
    (
        @ExpensesID,
        @PayeeID,
        @VendorID,
        @EmployeeID,
        @CustomerID,

        @TransactionTypeID,
        @TransactionType,
        @Type,

        @InvoiceDate,
        @TransactionDate,

        @Description,
        @Amount,
        @Currency,

        @SystemID,
        @WarehouseID,
        @DomainID,

        @CreateByUserID,
        GETDATE(),

        @ExpenseNo,
        @ExpenseID,

        @FromAccountID,
        @ToAccountID,

        @ClassNameID,
        @IsPosted,

        @JournalNo,
        @PaymentNo,
        @ExpenseNo, @ExpenseTransactionType
    );
    SET @NewTransactionID = SCOPE_IDENTITY();
END

SELECT
    @NewTransactionID AS TransactionID,
    @JournalNo AS JournalNo,
    @VDJournalNo AS VDJournalNo;";
            string JournalNo = "", VDJournalNo = "";
            using (SqlCommand cmd = new SqlCommand(
                transactionSql,
                conn,
                tran))
            {
                // ========================================================
                // Expense reference
                // ========================================================

                cmd.Parameters.Add("@ExpensesID", SqlDbType.Int).Value =
                    expenseId;

                cmd.Parameters.Add("@PayeeID", SqlDbType.Int).Value =
                    expense.PayeeID;

                cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value =
                    expense.VendorID;

                cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value =
                    expense.EmployeeID;

                cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value =
                    expense.CustomerID;

                // ========================================================
                // Transaction Type
                // ========================================================

                cmd.Parameters.Add("@TransactionTypeID", SqlDbType.Int).Value =
                    43;

                cmd.Parameters.Add("@TransactionType", SqlDbType.NVarChar).Value =
                    expense.TransactionType;
                cmd.Parameters.Add("@ExpenseTransactionType", SqlDbType.NVarChar).Value =
                    expense.ExpenseTransactionType;

                cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value =
                    expense.ExpenseTransactionType == "Vendor Deposit" ? "Vendor Deposit Payment" : "Bill Payment";

                // ========================================================
                // Dates
                // ========================================================

                cmd.Parameters.Add("@InvoiceDate", SqlDbType.DateTime).Value =
                    transactionDate;

                cmd.Parameters.Add("@TransactionDate", SqlDbType.DateTime).Value =
                    transactionDate;

                // ========================================================
                // Description
                // ========================================================

                cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value =
                    $"Payment - {expense.ExpenseNo}";

                // ========================================================
                // Amount
                // ========================================================

                SqlParameter amountParameter =
                    cmd.Parameters.Add("@Amount", SqlDbType.Decimal);

                amountParameter.Precision = 18;
                amountParameter.Scale = 2;
                amountParameter.Value = paymentAmount;

                // ========================================================
                // System
                // ========================================================

                cmd.Parameters.Add("@Currency", SqlDbType.NVarChar).Value =
                    DBNull.Value;

                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value =
                    expense.SystemID;

                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value =
                    expense.WarehouseID;

                // IMPORTANT:
                // Do not use AddWithValue if DomainID can be null.
                // This prevents:
                // "expects parameter @DomainID, which was not supplied"
                // ========================================================

                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar).Value =
                    string.IsNullOrWhiteSpace(expense.DomainID)
                        ? (object)DBNull.Value
                        : expense.DomainID;

                cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int).Value =
                    expense.CreateByUserID;

                // ========================================================
                // Expense
                // ========================================================

                cmd.Parameters.Add("@ExpenseNo", SqlDbType.NVarChar).Value =
                    string.IsNullOrWhiteSpace(expense.ExpenseNo)
                        ? (object)DBNull.Value
                        : expense.ExpenseNo;

                cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value =
                    expenseId;

                // ========================================================
                // Payment relationship
                //
                // FromAccountID = Cash/Bank
                // ToAccountID   = A/P
                // ========================================================

                cmd.Parameters.Add("@FromAccountID", SqlDbType.Int).Value =
                    paymentAccountId;

                cmd.Parameters.Add("@ToAccountID", SqlDbType.Int).Value =
                    payableAccountId;

                // ========================================================
                // Class
                // ========================================================

                cmd.Parameters.Add("@ClassNameID", SqlDbType.Int).Value = expense.ClassNameID;

                cmd.Parameters.Add("@IsPosted", SqlDbType.Bit).Value = 0;

                cmd.Parameters.Add("@PaymentNo", SqlDbType.NVarChar).Value = postingResult.PaymentNo;

                int TransactionID = 0;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        TransactionID = Convert.ToInt32(reader["TransactionID"]);
                        if (reader["VDJournalNo"].ToString() != "")
                        {
                            JournalNo = reader["VDJournalNo"].ToString();
                            VDJournalNo = reader["JournalNo"].ToString();
                        }
                        else
                        {
                            JournalNo = reader["JournalNo"].ToString();
                            VDJournalNo = reader["JournalNo"].ToString();
                        }
                    }
                }
                //if (transactionResult == null ||
                //    transactionResult == DBNull.Value)
                //{
                //    throw new Exception(
                //        "Failed to create Expense Payment Transaction.");
                //}

                paymentTransactionId = TransactionID;
            }

            // ============================================================
            // 8. INSERT JOURNAL ENTRY #1
            //
            // DEBIT A/P
            //
            // Dr Accounts Payable       $751
            //     Cr Cash/Bank                  $751
            // ============================================================

            string journalEntrySql = $@"

DECLARE @AccountNo NVARCHAR(50), @AccountName NVARCHAR(50)

        SELECT TOP 1
            @AccountNo = AccountNo,
            @AccountName = AccountName
        FROM TblChartOfAccount
        WHERE ChartOfAccountID = @AccountID;
if(@Type = 'Vendor Deposit')
begin
    SELECT TOP 1
    @AccountID = case when @Type='Vendor Deposit' then ChartOfAccountID else @AccountID end,
    @AccountNo = AccountNo,
    @AccountName = AccountName
    FROM TblChartOfAccount
    WHERE (AccountName LIKE '%Vendor Deposit%'
       OR AccountName LIKE '%Supplier Deposit%'
       OR AccountName LIKE '%Vendor Advance%'
       OR AccountName LIKE '%Supplier Advance%')
    AND SystemID = @SystemID AND WarehouseID=@WarehouseID;
end

INSERT INTO TblAccount_JournalEntries
(
    Type,
    TransactionID,
    AccountID,

    Description,
    Debit,
    Credit,

    EntryDate,
    DomainID,

    TransactionDate,

    SystemID,
    WarehouseID,
    CreateByUserID,
    CreateDate,

    ExpenseID,
    ExpenseAmount,

    ItemTAX,
    ClassNameID,

    IsPosted,
    JournalNo,
    ExpenseNo,
    PaymentNo,
    InvoiceNo, ExpenseTransactionType, Account
)
VALUES
(
    @Type,
    @TransactionID,
    @AccountID,

    @Description,
    @Debit,
    @Credit,

    @EntryDate,
    @DomainID,

    @TransactionDate,

    @SystemID,
    @WarehouseID,
    @CreateByUserID,
    GETDATE(),

    @ExpenseID,
    @ExpenseAmount,

    @ItemTAX,
    @ClassNameID,

    @IsPosted,
    case when (@Type = 'Vendor Deposit Payment') then '{VDJournalNo}'
    else
        @JournalNo
    end,
    @ExpenseNo,
    @PaymentNo, @ExpenseNo, @ExpenseTransactionType,
    @ExpenseTransactionType + '-' + @AccountNo + '-' + @AccountName
);";

            // ============================================================
            // 9. INSERT DEBIT A/P
            // ============================================================

            using (SqlCommand cmd = new SqlCommand(
            journalEntrySql,
            conn,
            tran))
            {
                cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value =
                    expense.ExpenseTransactionType == "Vendor Deposit" ? "Vendor Deposit" : "Bill Payment";


                cmd.Parameters.Add("@TransactionID", SqlDbType.Int).Value =
                    paymentTransactionId;

                cmd.Parameters.Add("@AccountID", SqlDbType.Int).Value =
                    payableAccountId;

                cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = $"Payment of {postingResult.ExpenseNo}";
                    //$"{postingResult.APAccountNo}-{postingResult.APAccountName}";

                AddDecimalParameter(
                    cmd,
                    "@Debit",
                    paymentAmount);

                AddDecimalParameter(
                    cmd,
                    "@Credit",
                    0m);

                cmd.Parameters.Add("@EntryDate", SqlDbType.DateTime).Value =
                    transactionDate;

                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar).Value =
                    string.IsNullOrWhiteSpace(expense.DomainID)
                        ? (object)DBNull.Value
                        : expense.DomainID;

                cmd.Parameters.Add("@TransactionDate", SqlDbType.DateTime).Value =
                    transactionDate;

                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value =
                    expense.SystemID;

                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value =
                    expense.WarehouseID;

                cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int).Value =
                    expense.CreateByUserID;

                cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value =
                    expenseId;

                AddDecimalParameter(
                    cmd,
                    "@ExpenseAmount",
                    paymentAmount);

                cmd.Parameters.Add("@ItemTAX", SqlDbType.Int).Value =
                    0;

                cmd.Parameters.Add("@ClassNameID", SqlDbType.Int).Value = expense.ClassNameID;

                cmd.Parameters.Add("@IsPosted", SqlDbType.Bit).Value =
                    0;
                cmd.Parameters.Add("@ExpenseNo", SqlDbType.NVarChar).Value =
                    expense.ExpenseNo;

                cmd.Parameters.Add("@PaymentNo", SqlDbType.NVarChar).Value =
                    paymentNo;
                cmd.Parameters.Add("@JournalNo", SqlDbType.NVarChar).Value =
                    JournalNo;
                cmd.Parameters.Add("@ExpenseTransactionType", SqlDbType.NVarChar).Value =
                    expense.ExpenseTransactionType;

                cmd.ExecuteNonQuery();
            }

            // ============================================================
            // 10. INSERT CREDIT CASH / BANK
            // ============================================================

            using (SqlCommand cmd = new SqlCommand(
                journalEntrySql,
                conn,
                tran))
            {
                cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value =
                    expense.ExpenseTransactionType == "Vendor Deposit" ? "Vendor Deposit Payment" : "Bill Payment";

                cmd.Parameters.Add("@TransactionID", SqlDbType.Int).Value =
                    paymentTransactionId;

                cmd.Parameters.Add("@AccountID", SqlDbType.Int).Value =
                    paymentAccountId;

                cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = $"Payment of {postingResult.ExpenseNo}";
                //$"{postingResult.PaymentAccountNo}-{postingResult.PaymentAccountName}";

                AddDecimalParameter(
                    cmd,
                    "@Debit",
                    0m);

                AddDecimalParameter(
                    cmd,
                    "@Credit",
                    paymentAmount);

                cmd.Parameters.Add("@EntryDate", SqlDbType.DateTime).Value =
                    transactionDate;

                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar).Value =
                    string.IsNullOrWhiteSpace(expense.DomainID)
                        ? (object)DBNull.Value
                        : expense.DomainID;

                cmd.Parameters.Add("@TransactionDate", SqlDbType.DateTime).Value =
                    transactionDate;

                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value =
                    expense.SystemID;

                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value =
                    expense.WarehouseID;

                cmd.Parameters.Add("@CreateByUserID", SqlDbType.Int).Value =
                    expense.CreateByUserID;

                cmd.Parameters.Add("@ExpenseID", SqlDbType.Int).Value =
                    expenseId;

                AddDecimalParameter(
                    cmd,
                    "@ExpenseAmount",
                    paymentAmount);

                cmd.Parameters.Add("@ItemTAX", SqlDbType.Int).Value =
                    0;

                cmd.Parameters.Add("@ClassNameID", SqlDbType.Int).Value = expense.ClassNameID;

                cmd.Parameters.Add("@IsPosted", SqlDbType.Bit).Value =
                    0;

                cmd.Parameters.Add("@ExpenseNo", SqlDbType.NVarChar).Value =
                    expense.ExpenseNo;

                cmd.Parameters.Add("@PaymentNo", SqlDbType.NVarChar).Value =
                    paymentNo;
                cmd.Parameters.Add("@JournalNo", SqlDbType.NVarChar).Value =
                    JournalNo;
                cmd.Parameters.Add("@ExpenseTransactionType", SqlDbType.NVarChar).Value =
                    expense.ExpenseTransactionType;

                cmd.ExecuteNonQuery();
            }

            // ============================================================
            // 11. UPDATE RESULT
            // ============================================================

            postingResult.PaymentNo = paymentNo;
            //postingResult.PaymentJournalNo = journalNo;
            postingResult.PaidAmount = paymentAmount;
            postingResult.RemainingAP = newRemainingAP;
        }

        private void AddParameter(
    SqlCommand cmd,
    string parameterName,
    SqlDbType dbType,
    object value)
        {
            SqlParameter parameter =
                cmd.Parameters.Add(parameterName, dbType);

            parameter.Value =
                value ?? DBNull.Value;
        }
        private void AddDecimalParameter(
    SqlCommand cmd,
    string parameterName,
    decimal value)
        {
            SqlParameter parameter =
                cmd.Parameters.Add(
                    parameterName,
                    SqlDbType.Decimal);

            parameter.Precision = 18;
            parameter.Scale = 2;
            parameter.Value = value;
        }
    }
}
