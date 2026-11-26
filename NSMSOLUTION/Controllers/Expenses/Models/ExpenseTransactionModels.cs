using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NSMSOLUTION.Controllers.Expenses.Models
{
    public class ExpenseCreateVm
    {
        public int? Id { get; set; }

        [Required]
        public int? ClassId { get; set; }

        [Required]
        public int? PaymentMethodId { get; set; }

        [Required]
        public int? PaymentAccountId { get; set; }

        [Required]
        public int? PayeeId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string Terms { get; set; }
        public DateTime? PaidOnDate { get; set; }
        public string CheckedBy { get; set; }
        public string VerifiedBy { get; set; }
        public string PostedBy { get; set; }
        public string PoRfAdvNo { get; set; }
        public string BillInvoiceNo { get; set; }
        public string ExpenseNo { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Description { get; set; }

        public decimal HeaderDiscount { get; set; }
        public decimal HeaderTax { get; set; }

        public List<ExpenseItemVm> Items { get; set; }
        public List<AdditionalExpenseVm> AdditionalExpenses { get; set; }

        public ExpenseCreateVm()
        {
            Items = new List<ExpenseItemVm>();
            AdditionalExpenses = new List<AdditionalExpenseVm>();
            ExchangeRate = 1m;
            Date = DateTime.Today;
        }
    }

    public class ExpenseItemVm
    {
        public int AccountId { get; set; }
        public string ItemService { get; set; }
        public string Uom { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; }
        public int? TaxId { get; set; }

        public decimal Total
        {
            get
            {
                var gross = Qty * UnitPrice;
                return Math.Max(0m, gross - Discount);
            }
        }
    }

    public class AdditionalExpenseVm
    {
        public int AccountId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class SelectOptionVm
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public SelectOptionVm() { }

        public SelectOptionVm(int id, string text)
        {
            Id = id;
            Text = text;
        }
    }

    public class ExpenseLookupVm
    {
        public List<SelectOptionVm> Classes { get; set; }
        public List<SelectOptionVm> PaymentMethods { get; set; }
        public List<SelectOptionVm> Accounts { get; set; }
        public List<SelectOptionVm> Payees { get; set; }
        public List<SelectOptionVm> Taxes { get; set; }

        public ExpenseLookupVm()
        {
            Classes = new List<SelectOptionVm>();
            PaymentMethods = new List<SelectOptionVm>();
            Accounts = new List<SelectOptionVm>();
            Payees = new List<SelectOptionVm>();
            Taxes = new List<SelectOptionVm>();
        }
    }
}
