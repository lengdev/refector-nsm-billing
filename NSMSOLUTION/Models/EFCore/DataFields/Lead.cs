namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class Lead
    {
        public int LeadID { get; set; }
        public int? DepartmentID { get; set; }
        public string LeadCode { get; set; }
        public string Salutation { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Memo { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
        public string Status { get; set; }
    }
}