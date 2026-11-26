using System;

namespace NSMSOLUTION.ViewModels
{
    public class GeneralLedgerViewModel
    {
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string Description { get; set; }

        public DateTime TransactionDate { get; set; }

        public string Type { get; set; }

        public string ReferenceNo { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string ContraAccount { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public decimal RunningBalance { get; set; }
        public string ParentAccountNo { get; set; }
        public string ParentAccountName { get; set; }
        public string ParentAccount { get; set; }

        public string GroupAccountNo { get; set; }
        public string GroupAccountName { get; set; }
        public string GroupAccount { get; set; }
        public string ClassName { get; set; }

        public decimal GroupDebitTotal { get; set; }
        public decimal GroupCreditTotal { get; set; }
    }
}