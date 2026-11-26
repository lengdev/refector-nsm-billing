using System;

public class ActivateCustomer
{
    public int ActivateCustomerID { get; set; }
    public int RouterID { get; set; }
    public int? BillingInvoiceID { get; set; }
    public int CustomerID { get; set; }
    public int? CustomerTypeID { get; set; }
    public int? ServiceID { get; set; }

    public string ConnectionID { get; set; }
    public string SID { get; set; }
    public string InvoiceNo { get; set; }

    public int? PackageDetailID { get; set; }
    public int? Bandwidth { get; set; }

    public string ID { get; set; }
    public string ActualProfile { get; set; }
    public string UserGroup { get; set; }
    public string PPPoEProfileName { get; set; }
    public string ConnectionType { get; set; }
    public string PPPoEUserName { get; set; }
    public string PPPoEPassword { get; set; }
    public string PPPoEIPAddress { get; set; }

    public string VLanID { get; set; }
    public string VLanName { get; set; }
    public string VLanIPAddress { get; set; }

    public string IPAddress { get; set; }
    public string GetwayIP { get; set; }
    public string SubnetIP { get; set; }
    public string DNS1 { get; set; }
    public string DNS2 { get; set; }

    public string WifiIP { get; set; }
    public string WifiName { get; set; }
    public string WifiKey { get; set; }
    public string WifiLoginName { get; set; }
    public string WifiLoginPassword { get; set; }

    public string ActivateBy { get; set; }
    public DateTime? ActivateDate { get; set; }

    public string LockedBy { get; set; }
    public DateTime? LockedDate { get; set; }

    public int ConnectivityStatus { get; set; }
    public string ConnectivityStatusName { get; set; }

    public DateTime? StartDate { get; set; }

    public string RenewBy { get; set; }
    public DateTime? RenewDate { get; set; }
    public string RenewStatus { get; set; }

    public DateTime? PaidDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Duration { get; set; }

    public bool? IsApplyChangePlan { get; set; }

    public string DeviceName { get; set; }
    public string TicketNumber { get; set; }

    public DateTime? CreateDate { get; set; }

    public bool? IsOneService { get; set; }

    public string ConnectionName { get; set; }
    public string ConnectionOfConnectionID { get; set; }

    public string Comment { get; set; }

    public int? PaymentStatus { get; set; }
    public DateTime? PaymentDueDate { get; set; }

    public string ModifyBy { get; set; }
    public DateTime? ModifyDate { get; set; }

    public int? SystemID { get; set; }
    public int? WarehouseID { get; set; }

    public string DomainID { get; set; }

    public DateTime? Date { get; set; }

    public DateTime? ReactivateDate { get; set; }
    public string ReactivateBy { get; set; }

    public bool? AutoLock { get; set; }
    public string SetAutoLockBy { get; set; }
    public DateTime? SetAutoLockDate { get; set; }

    public DateTime? TerminatDate { get; set; }
}