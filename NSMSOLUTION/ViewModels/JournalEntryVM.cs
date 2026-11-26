using System;
using System.Collections.Generic;

namespace NSMSOLUTION.ViewModels
{
    public class JournalEntryVM
    {
        public int JournalEntryID { get; set; }
        public string JVNo { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.Today;
        public string Memo { get; set; }
        public bool IsAdjustment { get; set; }
        public decimal TotalAmount { get; set; }
        public int StatusID { get; set; } = 60;
        public string StatusName { get; set; } = "Draft";
        public int? ClassID { get; set; }
        public string ClassName { get; set; }
        public string CreatedBy { get; set; }
        public int? CheckedByUserID { get; set; }
        public string CheckedBy { get; set; }
        public DateTime? CheckedDate { get; set; }
        public int? VerifiedByUserID { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public int? ApprovedByUserID { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DisapprovedBy { get; set; }
        public DateTime? DisapprovedDate { get; set; }
        public List<JournalEntryDetailVM> Lines { get; set; } = new List<JournalEntryDetailVM>();
        public List<AccountDropdownItem> AvailableAccounts { get; set; } = new List<AccountDropdownItem>();
        public List<ContactDropdownItem> AvailableContacts { get; set; } = new List<ContactDropdownItem>();
        public List<ClassDropdownItem> AvailableClasses { get; set; } = new List<ClassDropdownItem>();
        public List<UserDropdownItem> AvailableUsers { get; set; } = new List<UserDropdownItem>();
    }

    public class UserDropdownItem
    {
        public int UserLoginID { get; set; }
        public string FullName { get; set; }
        public string Job { get; set; }
    }

    public class JournalEntryDetailVM
    {
        public int DetailID { get; set; }
        public int JournalEntryID { get; set; }
        public int ChartOfAccountID { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public string LineMemo { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public int? ClassID { get; set; }
        public string ClassName { get; set; }
        public int? NameID { get; set; }
        public string Name { get; set; }
        public string NameType { get; set; } // Customer, Vendor, Employee
        public int LineOrder { get; set; } = 1;
    }

    public class AccountDropdownItem
    {
        public int ChartOfAccountID { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string DisplayName { get; set; }
    }

    public class ContactDropdownItem
    {
        public int ContactID { get; set; }
        public string ContactName { get; set; }
        public string ContactType { get; set; }
    }

    public class ClassDropdownItem
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
    }
}
