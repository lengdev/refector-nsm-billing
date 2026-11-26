using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblCustomerDeviceInfo")]
    public class CustomerDeviceInfo
    {
        [Key] public int CustomerDeviceInfoID { get; set; }
        public int? CustomerID { get; set; }
        public int? ActivateCustomerID { get; set; }
        public string InvoiceNo { get; set; }
        public string ConnectionID { get; set; }
        public string SID { get; set; }
        public int? DeviceNameID { get; set; }
        public string DeviceName { get; set; }
        public int? PONPort { get; set; }
        public string OLTID { get; set; }
        public int? SplitterNameID { get; set; }
        public string SplitterName { get; set; }
        public int? Port { get; set; }
        public string Length { get; set; }
        public string LossDB { get; set; }
        public string GoogleMap { get; set; }
        public string Warehouse { get; set; }
        public int? WID { get; set; }
        public string CustomerNotes { get; set; }
        public System.DateTime? CreateDate { get; set; }
        public string CreateBy { get; set; }
        public string InstallBy { get; set; }
        public System.DateTime? CompleteInstallDate { get; set; }
        public string ModifyBy { get; set; }
        public System.DateTime? ModifyDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}