namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class Uom
    {
        public int UomID { get; set; }
        public string Name { get; set; }
        public string UoMCommercialUnitFormat { get; set; }
        public string Memo { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}