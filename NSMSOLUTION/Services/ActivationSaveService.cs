using MVC;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSMSOLUTION.Services
{
    // result of the billing/package/customer join
    public class InvoiceInfo
    {
        public CustomerBillingInvoice Bill;   // <- use your real entity class for TblCustomerBillingInvoice
        public int? Bandwidth;                // <- match the column type
        public int? CustomerTypeID;           // <- match the column type
    }

    public static class ActivationSaveService
    {
        public static InvoiceInfo LoadInvoice(DBContext ctx, int? billingInvoiceId)
        {
            return (from bi in ctx.TblCustomerBillingInvoice
                    join pd in ctx.TblPackageDetail on bi.PackageDetailID equals (int?)pd.PackageDetailID
                    join c in ctx.TblCustomer on bi.CustomerID equals (int?)c.CustomerID
                    where bi.BillingInvoiceID == billingInvoiceId
                    select new InvoiceInfo
                    {
                        Bill = bi,
                        Bandwidth = pd.Bandwidth,
                        CustomerTypeID = c.CustomerTypeID
                    }).FirstOrDefault();
        }

        public static string ValidatePPPoEUsername(
            DBContext ctx,
            ActivateCustomer a)
        {
            if (ConnectionTypes.Normalize(a.ConnectionType) != ConnectionTypes.PPPoE)
                return null;

            var username = (a.PPPoEUserName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(username))
                return "PPPoE username is required.";

            // Exclude the current activation when editing.
            var duplicate = ctx.TblActivateCustomer.Any(x =>
                x.RouterID == a.RouterID &&
                x.PPPoEUserName == username &&
                x.ActivateCustomerID != a.ActivateCustomerID);

            if (duplicate)
                return "This PPPoE username is already assigned to another activation on this router.";

            return null;
        }

        // I/O to the router only, no database access
        public static string EnsureDeviceEnabledOnRouter(ActivateCustomer a)
        {
            string type = ConnectionTypes.Normalize(a.ConnectionType);
            var status = ActivationService.CheckDeviceStatus(a.RouterID, type, a.PPPoEUserName, a.VLanName);

            var mikrotik = new MikrotikService();

            string comment = "";

            if (type == ConnectionTypes.PPPoE)
            {
                comment = $"New connection with pppoe: {a.PPPoEUserName}";
                mikrotik.ReactivateUser(a.PPPoEUserName,
                    new UserActionRequest { RouterID = a.RouterID, Comment = comment });
            }
            else if (type == ConnectionTypes.Static)
            {
                comment = $"New connection with vlan: {a.VLanName}";
                mikrotik.EnableVlan(a.VLanName,
                    new VlanActionRequest { RouterID = a.RouterID, VlanId = a.VLanID, Comment = comment });
            }

            return comment;
        }

        // insert a new activation, or update the existing one; returns the saved row
        public static ActivateCustomer UpsertActivation(DBContext ctx, ActivateCustomer existing,
            ActivateCustomer a, InvoiceInfo inv, string user, DateTime now, string comment)
        {
            if (existing == null)
            {
                var billing = inv.Bill;
                var row = new ActivateCustomer
                {
                    RouterID = a.RouterID,
                    CustomerID = a.CustomerID,
                    ConnectionID = a.ConnectionID,
                    ActualProfile = a.ActualProfile,
                    ConnectionType = a.ConnectionType,

                    PPPoEProfileName = a.PPPoEProfileName,
                    PPPoEUserName = a.PPPoEUserName,
                    PPPoEPassword = a.PPPoEPassword,
                    PPPoEIPAddress = a.PPPoEIPAddress,
                    UserGroup = a.UserGroup,

                    VLanID = a.VLanID,
                    VLanName = a.VLanName,
                    VLanIPAddress = a.VLanIPAddress,

                    IPAddress = a.IPAddress,
                    GetwayIP = a.GetwayIP,
                    SubnetIP = a.SubnetIP,
                    DNS1 = a.DNS1,
                    DNS2 = a.DNS2,

                    WifiIP = a.WifiIP,
                    WifiName = a.WifiName,
                    WifiKey = a.WifiKey,
                    WifiLoginName = a.WifiLoginName,
                    WifiLoginPassword = a.WifiLoginPassword,

                    ActivateBy = user,
                    ActivateDate = a.ActivateDate,
                    ConnectivityStatus = ConnectivityStatusCodes.Active,
                    ConnectivityStatusName = "Actived",
                    Comment = comment,

                    ServiceID = billing.ServiceID,
                    PackageDetailID = billing.PackageDetailID,
                    StartDate = billing.StartDate,
                    EndDate = billing.EndDate,
                    Duration = billing.Duration,
                    Bandwidth = inv.Bandwidth,
                    CustomerTypeID = inv.CustomerTypeID,
                    Date = billing.StartDate,

                    SID = a.SID,
                    SystemID = a.SystemID,
                    WarehouseID = a.WarehouseID,
                    DomainID = billing.DomainID,
                    BillingInvoiceID = a.BillingInvoiceID,
                    InvoiceNo = a.InvoiceNo,

                    DeviceName = a.DeviceName,
                    IsOneService = a.IsOneService,
                    ConnectionName = a.ConnectionName,
                    ConnectionOfConnectionID = a.ConnectionOfConnectionID,
                    TicketNumber = a.TicketNumber,

                    PaymentStatus = a.PaymentStatus,
                    PaymentDueDate = a.PaymentDueDate,

                    CreateDate = now,
                };

                ctx.TblActivateCustomer.Add(row);
                ctx.SaveChanges();

                var history = new HistoryCustomerActivate
                {
                    ConnectionType = row.ConnectionType,
                    CustomerID = row.CustomerID,
                    ServiceID = row.ServiceID,
                    PackageDetailID = row.PackageDetailID,
                    ActivateDate = row.ActivateDate,
                    ActivateBy = row.ActivateBy,
                    Comment = row.Comment,
                    StartDate = row.StartDate,
                    EndDate = row.EndDate,
                    Duration = row.Duration,
                    ConnectivityStatus = row.ConnectivityStatus,
                    SystemID = row.SystemID,
                    WarehouseID = row.WarehouseID,
                    DomainID = row.DomainID,
                    ActivateCustomerID = row.ActivateCustomerID,
                    LockedBy = row.LockedBy,
                    LockedDate = row.LockedDate,
                    ConnectionID = row.ConnectionID,
                    Date = DateTime.Now
                };

                ctx.TblHistoryCustomerActivate.Add(history);
                ctx.SaveChanges();

                return row;
            }

            existing.RouterID = a.RouterID;
            existing.ActivateDate = a.ActivateDate;
            existing.ConnectionType = a.ConnectionType;
            existing.PPPoEProfileName = a.PPPoEProfileName;
            existing.PPPoEUserName = a.PPPoEUserName;
            existing.PPPoEPassword = a.PPPoEPassword;
            existing.PPPoEIPAddress = a.PPPoEIPAddress;
            existing.UserGroup = a.UserGroup;

            existing.VLanID = a.VLanID;
            existing.VLanName = a.VLanName;
            existing.VLanIPAddress = a.VLanIPAddress;
            existing.IPAddress = a.IPAddress;
            existing.GetwayIP = a.GetwayIP;
            existing.SubnetIP = a.SubnetIP;
            existing.DNS1 = a.DNS1;
            existing.DNS2 = a.DNS2;

            existing.DeviceName = a.DeviceName;
            existing.WifiIP = a.WifiIP;
            existing.WifiName = a.WifiName;
            existing.WifiKey = a.WifiKey;
            existing.WifiLoginName = a.WifiLoginName;
            existing.WifiLoginPassword = a.WifiLoginPassword;

            existing.ModifyBy = user;
            existing.ModifyDate = now;
            ctx.SaveChanges();
            return existing;
        }

        // keep billing invoice and customer invoice in step with the activation
        public static void SyncBilling(DBContext ctx, CustomerBillingInvoice billing,
            ActivateCustomer existing, bool isInsert, string user, DateTime now)
        {
            billing.ActivateCustomerID = existing.ActivateCustomerID;
            billing.SID = existing.SID;
            billing.RouterID = existing.RouterID;
            billing.ConnectionType = existing.ConnectionType;
            billing.PPPoEUserName = existing.PPPoEUserName;
            billing.PPPoEPassword = existing.PPPoEPassword;
            billing.ModifyBy = user;
            billing.ModifyDate = now;

            if (isInsert)
            {
                billing.ActivateDate = existing.ActivateDate;
                billing.ConnectivityStatus = existing.ConnectivityStatus;
                billing.ConnectivityStatusName = existing.ConnectivityStatusName;
            }

            var custInv = ctx.TblCustomerInvoice
                .FirstOrDefault(i => i.BillingInvoiceID == billing.BillingInvoiceID &&
                                     i.InvoiceNo == billing.InvoiceNo);
            if (custInv != null)
            {
                custInv.ActivateCustomerID = existing.ActivateCustomerID;
                custInv.ConnectivityStatus = existing.ConnectivityStatus;
                custInv.ConnectivityStatusName = existing.ConnectivityStatusName;
            }

            ctx.SaveChanges();
        }

        public static CustomerDeviceInfo UpsertDeviceInfo(DBContext ctx, ActivateCustomer existing,
            CustomerDeviceInfo dev, string notes, string domainId, string user, DateTime now)
        {
            var info = ctx.TblCustomerDeviceInfo
                .FirstOrDefault(x => x.ActivateCustomerID == existing.ActivateCustomerID);

            if (info == null)
            {
                info = new CustomerDeviceInfo
                {
                    ActivateCustomerID = existing.ActivateCustomerID,
                    CreateBy = user,
                    CreateDate = now
                };
                ctx.TblCustomerDeviceInfo.Add(info);
            }
            else
            {
                info.ModifyBy = user;
                info.ModifyDate = now;
            }

            var html = new HTML();

            info.CustomerID = existing.CustomerID;
            info.InvoiceNo = existing.InvoiceNo;
            info.ConnectionID = Convert.ToString(existing.ConnectionID);
            info.SID = existing.SID;
            info.DeviceNameID = dev.DeviceNameID;
            info.DeviceName = dev.DeviceName;
            info.PONPort = dev.PONPort;
            info.OLTID = dev.OLTID ?? "";
            info.SplitterNameID = dev.SplitterNameID;
            info.SplitterName = dev.SplitterName;
            info.Port = dev.Port;
            info.Length = dev.Length ?? "";
            info.LossDB = dev.LossDB ?? "";
            info.GoogleMap = dev.GoogleMap ?? "";
            info.WID = dev.WID;
            info.Warehouse = dev.Warehouse ?? "";
            info.CustomerNotes = notes;
            info.InstallBy = dev.InstallBy ?? "";
            info.CompleteInstallDate = dev.CompleteInstallDate;
            info.SystemID = Convert.ToInt32(html.SystemID);
            info.WarehouseID = Convert.ToInt32(html.WarehouseID);
            info.DomainID = domainId;

            ctx.SaveChanges();     // info.CustomerDeviceInfoID is needed by ReplaceEquipment
            return info;
        }

        public static void ReplaceEquipment(DBContext ctx, CustomerDeviceInfo info, ActivateCustomer existing,
            IEnumerable<CustomerDeviceInfoDetail> items, CustomerDeviceInfo dev, string user, DateTime now)
        {
            var oldDetails = ctx.TblCustomerDeviceInfoDetail
                .Where(x => x.CustomerDeviceInfoID == info.CustomerDeviceInfoID)
                .ToList();
            ctx.TblCustomerDeviceInfoDetail.RemoveRange(oldDetails);

            foreach (var i in items)
            {
                ctx.TblCustomerDeviceInfoDetail.Add(new CustomerDeviceInfoDetail
                {
                    CustomerDeviceInfoID = info.CustomerDeviceInfoID,
                    CustomerID = existing.CustomerID,
                    ActivateCustomerID = existing.ActivateCustomerID,
                    InvoiceNo = existing.InvoiceNo,
                    ItemID = i.ItemID,
                    ItemSerialDetailID = i.ItemSerialDetailID,
                    ItemName = i.ItemName ?? "",
                    CostPrice = i.CostPrice,
                    UM = i.UM ?? "",
                    Type = i.Type ?? "",
                    QTY = i.QTY,
                    SerialNo = i.SerialNo ?? "",
                    MACAddress = i.MACAddress ?? "",
                    WarehouseName = i.WarehouseName ?? "",
                    LocationID = i.LocationID,
                    Connection = i.Connection,
                    Status = "New Install",
                    Note = i.Reason ?? "",
                    InstallBy = dev.InstallBy ?? "",
                    CompleteInstallDate = dev.CompleteInstallDate,
                    Date = now,
                    CreateBy = user,
                    CreateDate = now,
                    DomainID = info.DomainID
                });
            }
            ctx.SaveChanges();
        }
    }
}