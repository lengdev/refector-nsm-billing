namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class CustomerCompany
    {
        public int CustomerCompanyID { get; set; }
        public int CustomerID { get; set; }
        public int? IndustryTypeID { get; set; }

        public string CompanyName { get; set; }
        public string CompanyNameKH { get; set; }

        public string CompanyDeskPhone { get; set; }
        public string CompanyMobile { get; set; }
        public string CompanyEmail { get; set; }

        public string VATNumber { get; set; }

        public string CompanyAddress { get; set; }
        public string CompanyCommune { get; set; }
        public string CompanyDistrict { get; set; }
        public string CompanyProvinceCity { get; set; }

        public string CompanyContactName { get; set; }
        public string CompanyContactPosition { get; set; }
        public string CompanyContactMobileNumber { get; set; }

        public bool IsIncludedVAT { get; set; }

        // System Fields
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
