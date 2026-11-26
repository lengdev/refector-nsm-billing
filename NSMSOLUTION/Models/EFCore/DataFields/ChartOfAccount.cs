using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class ChartOfAccount
    {
        public int ChartOfAccountID { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public string SubOfAccountType { get; set; }
        public int? ParentAccountID { get; set; }
        public int OrderBy { get; set; }
        public int StatusID { get; set; }
        public string Memo { get; set; }
        public DateTime ActivateAccountAsOf { get; set; }
        public decimal OpeningBalance { get; set; }
        public DateTime OpeningBalanceDate { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
        public bool DefaultForInvoice { get; set; }
        public bool DefaultForBill { get; set; }
        public int ClassID { get; set; }
        public bool IsDefault { get; set; }
        public int CompanyID { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
        public string NormalSide { get; set; }
        public int AccountCategoryID { get; set; }
        public int AccountTypesID { get; set; }
    }
}