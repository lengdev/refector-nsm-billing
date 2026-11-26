using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblActivateCustomerMovement")]
    public class Movement
    {
        [Key]public long MovementID { get; set; }
        public int ActivateCustomerID { get; set; }
        public int? CustomerID { get; set; }
        public string ConnectionID { get; set; }
        public string SID { get; set; }
        public string InvoiceNo { get; set; }
        public string ActionType { get; set; }
        public int? OldStatus { get; set; }
        public int? NewStatus { get; set; }
        public string Comment { get; set; }
        public string Source { get; set; }
        public string RouterResult { get; set; }
        public string Changes { get; set; }
        public string ConnectionType { get; set; }
        public int? RouterID { get; set; }
        public string RouterName { get; set; }
        public string PPPoEUserName { get; set; }
        public string PPPoEProfileName { get; set; }
        public string UserGroup { get; set; }
        public string ActualProfile { get; set; }
        public string VLanID { get; set; }
        public string VLanName { get; set; }
        public string VLanIPAddress { get; set; }
        public string IPAddress { get; set; }
        public string GetwayIP { get; set; }
        public string SubnetIP { get; set; }
        public string DNS1 { get; set; }
        public string DNS2 { get; set; }
        public string DeviceName { get; set; }
        public string WifiName { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime CreateDate { get; set; }
    }
}