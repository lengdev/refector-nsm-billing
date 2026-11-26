using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblCustomerDeviceInfoDetail")]
    public class CustomerDeviceInfoDetail
    {
        [Key] public int CustomerDeviceInfoDetailID { get; set; }
        public int? CustomerID { get; set; }
        public int? ActivateCustomerID { get; set; }
        public int? CustomerDeviceInfoID { get; set; }
        public string InvoiceNo { get; set; }
        public int? ItemID { get; set; }
        public int? ItemSerialDetailID { get; set; }
        public string ItemName { get; set; }
        public decimal? CostPrice { get; set; }
        public string UM { get; set; }
        public string Type { get; set; }
        public int? QTY { get; set; }
        public string SerialNo { get; set; }
        public string MACAddress { get; set; }
        public string WarehouseName { get; set; }
        public int? LocationID { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Remark { get; set; }
        public System.DateTime? Date { get; set; }
        public string Connection { get; set; }
        public string Note { get; set; }
        public string InstallBy { get; set; }
        public System.DateTime? CompleteInstallDate { get; set; }
        public string DomainID { get; set; }
        public int? UserID { get; set; }
        public System.DateTime? CreateDate { get; set; }
        public string CreateBy { get; set; }
    }
}