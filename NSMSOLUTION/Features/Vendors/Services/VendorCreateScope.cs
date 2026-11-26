namespace NSMSOLUTION.Features.Vendors.Services
{
    public class VendorCreateScope
    {
        public string CreateBy { get; set; }

        public int SystemID { get; set; }

        public int WarehouseID { get; set; }

        public string DomainID { get; set; }

        public int UserID { get; set; }

        public int? CreateByUserID { get; set; }
    }

    public class VendorCreateResult
    {
        public int VendorID { get; set; }
        public string VendorCode { get; set; }
        public int Seq { get; set; }
    }
}