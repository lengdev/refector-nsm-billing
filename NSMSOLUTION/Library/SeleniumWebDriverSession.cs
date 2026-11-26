using System.Collections.Generic;

namespace NSMSOLUTION.Library
{
    public class SeleniumWebDriverSession
    {
        public string sessionId { get; set; }
        public int UserID { get; set; }
        public List<CompanyInfo> MyCompanyInfo { get; set; }
    }

    public class CompanyInfo
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string ComID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyNameEN { get; set; }
        public int ComType { get; set; }
        public string VatNo { get; set; }
        public string TaxType { get; set; }
        public string Location { get; set; }
        public string ComStatus { get; set; }
        public int TaxBranchID { get; set; }
        public string TaxBranchKH { get; set; }
    }
}