using MVC;
using NSMSOLUTION.Controllers.API;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using NSMSOLUTION.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers
{
    [RoutePrefix("api/activation")]
    public class ActivationController : ApiController
    {
        private static readonly MikrotikService Mk = new MikrotikService();

        public class MovementRow
        {
            public long MovementID { get; set; }
            public string ActionType { get; set; }
            public int? OldStatus { get; set; }
            public int? NewStatus { get; set; }
            public string Comment { get; set; }
            public string Source { get; set; }
            public string RouterResult { get; set; }
            public string Changes { get; set; }
            public string ConnectionType { get; set; }
            public string RouterName { get; set; }
            public string PPPoEUserName { get; set; }
            public string VLanName { get; set; }
            public string VLanIPAddress { get; set; }
            public string IPAddress { get; set; }
            public string UserGroup { get; set; }
            public string CreateBy { get; set; }
            public DateTime CreateDate { get; set; }
        }

        public class SetConnectivityRequest
        {
            public int ActivateCustomerID { get; set; }
            public int Status { get; set; }
            public string Comment { get; set; } = "";
        }

        public class CheckPPPoEUserRequest
        {
            public bool CheckPPPoEUserName { get; set; }
            public int RouterID { get; set; }
            public int ActivateCustomerID { get; set; }
            public string OldPPPoEUserName { get; set; }
            public string OldPPPoEPassword { get; set; }
            public string PPPoEUserName { get; set; }
            public string PPPoEPassword { get; set; }
        }

        public class SaveChangeActivateRequest
        {
            public CheckPPPoEUserRequest CheckPPPoEUserName { get; set; }
            public List<CustomerDeviceInfo> DeviceInformation { get; set; }
            public List<CustomerDeviceInfoDetail> DeviceInformationDetail { get; set; }
            public List<ActivateCustomer> ActivateCustomerData { get; set; }
            public List<object> ApplyCons { get; set; }
        }

        private static string Fmt(DateTime? d) =>
            d.HasValue ? d.Value.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) : null;

        [HttpGet, Route("movements")]
        public IHttpActionResult Movements(int activateCustomerId, int top = 100)
        {
            using (var ctx = new DBContext())
            {
                var rows = ctx.TblActivateCustomerMovement
                    .AsNoTracking()
                    .Where(m => m.ActivateCustomerID == activateCustomerId)
                    .OrderByDescending(m => m.MovementID)
                    .Take(top)
                    .Select(m => new MovementRow
                    {
                        MovementID = m.MovementID,
                        ActionType = m.ActionType,
                        OldStatus = m.OldStatus,
                        NewStatus = m.NewStatus,
                        Comment = m.Comment,
                        Source = m.Source,
                        RouterResult = m.RouterResult,
                        Changes = m.Changes,
                        ConnectionType = m.ConnectionType,
                        RouterName = m.RouterName,
                        PPPoEUserName = m.PPPoEUserName,
                        VLanName = m.VLanName,
                        VLanIPAddress = m.VLanIPAddress,
                        IPAddress = m.IPAddress,
                        UserGroup = m.UserGroup,
                        CreateBy = m.CreateBy,
                        CreateDate = m.CreateDate
                    })
                    .ToList();
                return Ok(rows);
            }
        }

        [HttpGet, Route("context")]
        public IHttpActionResult Context(int billingInvoiceId, int activateCustomerId = 0)
        {
            var html = new HTML();
            int sys = Convert.ToInt32(html.SystemID);
            int wh = Convert.ToInt32(html.WarehouseID);

            using (var ctx = new DBContext())
            {
                var p = (from bi in ctx.TblCustomerBillingInvoice
                         join c in ctx.TblCustomer on bi.CustomerID equals (int?)c.CustomerID
                         join s in ctx.TblService on bi.ServiceID equals (int?)s.ServiceID
                         join pd in ctx.TblPackageDetail on bi.PackageDetailID equals (int?)pd.PackageDetailID
                         join i in ctx.TblCustomerBillInformation on bi.CustomerID equals (int?)i.CustomerID
                         join ct in ctx.TblCustomerBlock on (int?)c.CustomerTypeID equals (int?)ct.CustomerTypeID
                         where bi.BillingInvoiceID == billingInvoiceId && i.InvoiceNo == bi.InvoiceNo
                         select new
                         {
                             bi.BillingInvoiceID,
                             bi.CustomerID,
                             c.CustomerCode,
                             c.CustomerName,
                             c.CustomerNameLatin,
                             CustomerBlock = ct.CustomerType,
                             s.ServiceName,
                             pd.Bandwidth,
                             pd.DowSpeed,
                             pd.UpSpeed,
                             pd.PackageName,
                             pd.Duration,
                             PackageUM = pd.UM,
                             bi.UM,
                             bi.FreeDuration,
                             bi.StartDate,
                             bi.EndDate,
                             bi.InvoiceNo,
                             bi.SID,
                             bi.ConnectionID,
                             bi.TicketNumber,
                             bi.IsChangePlan,
                             bi.IsAlreadyApplyProfile,
                             i.CompanyContactMobileNumber,
                             i.InstallAddress,
                             i.InstallCommune,
                             i.InstallDistrict,
                             i.InstallProvinceCity
                         }).FirstOrDefault();

                if (p == null) return NotFound();

                var plan = new
                {
                    p.BillingInvoiceID,
                    p.CustomerID,
                    p.CustomerCode,
                    CustomerName = string.IsNullOrEmpty(p.CustomerName) ? p.CustomerNameLatin : p.CustomerName,
                    p.CustomerBlock,
                    p.ServiceName,
                    p.Bandwidth,
                    p.DowSpeed,
                    p.UpSpeed,
                    PackageLabel = p.PackageName + " - " + p.Bandwidth + " Mbps - " + p.Duration + " " + p.PackageUM,
                    p.UM,
                    p.FreeDuration,
                    p.StartDate,
                    p.EndDate,
                    p.InvoiceNo,
                    p.SID,
                    p.ConnectionID,
                    p.TicketNumber,
                    IsChangePlan = p.IsChangePlan ?? false,
                    IsAlreadyApplyProfile = p.IsAlreadyApplyProfile ?? false,
                    Phone = p.CompanyContactMobileNumber,
                    Location = string.Join(",",
                        new[] { p.InstallAddress, p.InstallCommune, p.InstallDistrict, p.InstallProvinceCity }
                            .Where(x => !string.IsNullOrEmpty(x)))
                };

                object activate = null, device = null;
                object items = new List<object>();

                if (activateCustomerId > 0)
                {
                    var ac = ctx.TblActivateCustomer.AsNoTracking()
                        .FirstOrDefault(x => x.ActivateCustomerID == activateCustomerId);

                    if (ac != null)
                    {
                        activate = new
                        {
                            ac.ActivateCustomerID,
                            ac.ConnectionType,
                            ac.PPPoEProfileName,
                            ac.PPPoEUserName,
                            ac.PPPoEPassword,
                            ac.PPPoEIPAddress,
                            ac.IPAddress,
                            ac.GetwayIP,
                            ac.SubnetIP,
                            ac.DNS1,
                            ac.DNS2,
                            ac.WifiIP,
                            ac.WifiName,
                            ac.WifiKey,
                            ac.WifiLoginName,
                            ac.WifiLoginPassword,
                            ac.RouterID,
                            ac.ConnectivityStatus,
                            ac.VLanID,
                            ac.VLanName,
                            ac.VLanIPAddress,
                            CustDeviceName = ac.DeviceName,
                            ac.SID,
                            ac.ActivateBy,
                            ActivateDate = Fmt(ac.ActivateDate),
                            ac.LockedBy,
                            LockedDate = Fmt(ac.LockedDate),
                            UserGroup = ac.UserGroup ?? ""
                        };
                    }

                    device = (from di in ctx.TblCustomerDeviceInfo
                              join dn in ctx.TblDeviceName on di.DeviceNameID equals (int?)dn.DeviceNameID
                              join dsn in ctx.TblDeviceSplitterName on di.SplitterNameID equals (int?)dsn.SplitterNameID
                              from uw in ctx.TblUserWarehouse.Where(w => w.WID == di.WID).DefaultIfEmpty()
                              where di.ActivateCustomerID == activateCustomerId
                              select new
                              {
                                  di.DeviceNameID,
                                  dn.DeviceName,
                                  di.PONPort,
                                  di.OLTID,
                                  dsn.SplitterName,
                                  dsn.SplitterNameID,
                                  di.Port,
                                  di.Length,
                                  di.LossDB,
                                  di.GoogleMap,
                                  di.WID,
                                  Warehouse = uw.WarehouseName,
                                  di.CustomerNotes,
                                  di.InstallBy,
                                  di.CompleteInstallDate
                              }).FirstOrDefault();

                    items = (from ci in ctx.TblCustomerDeviceInfo
                             join cd in ctx.TblCustomerDeviceInfoDetail
                                 on (int?)ci.CustomerDeviceInfoID equals cd.CustomerDeviceInfoID
                             where ci.ActivateCustomerID == activateCustomerId
                             select new
                             {
                                 cd.CustomerDeviceInfoDetailID,
                                 cd.ItemID,
                                 cd.ItemSerialDetailID,
                                 cd.ItemName,
                                 cd.UM,
                                 cd.Type,
                                 cd.QTY,
                                 SerialNumber = cd.SerialNo,
                                 cd.MACAddress,
                                 Reason = cd.Note,
                                 cd.LocationID,
                                 cd.WarehouseName,
                                 Connection = ci.ConnectionID
                             }).ToList();
                }

                bool bulk = ctx.TblCustomerInvoice.Any(x =>
                    x.CustomerID == p.CustomerID &&
                    x.InvoiceNo == p.InvoiceNo &&
                    x.ItemDescription.Contains("CON:"));

                var routers = ctx.TblRouter
                    .AsNoTracking()
                    .Where(r => (r.StatusID ?? 0) != 4 && r.SystemID == sys && r.WarehouseID == wh)
                    .Select(r => new { r.RouterID, r.RouterName })
                    .ToList();

                return Ok(new
                {
                    plan,
                    activate,
                    device,
                    items,
                    routers,
                    bulk,
                    recommendedUser = plan.CustomerBlock + "-" + plan.CustomerCode,
                    canCreatePppoe = User.IsInRole("Allow Create PPPoE User")
                });
            }
        }

        [HttpGet, Route("router-options")]
        public IHttpActionResult RouterOptions(int routerId, int activateCustomerId = 0)
        {
            object vlans = new List<object>(), users = new List<object>(), profiles = new List<object>(), groups = new List<string>();
            try { vlans = Mk.GetVlans(routerId); } catch { }
            try { users = Mk.GetPPPoEUsers(routerId); } catch { }
            try { profiles = Mk.GetProfiles(routerId); } catch { }
            try { groups = Mk.GetUserGroup(routerId); } catch { }

            List<string> used;
            using (var ctx = new DBContext())
            {
                used = ctx.TblActivateCustomer
                    .AsNoTracking()
                    .Where(x => x.ConnectionType == "Static"
                             && x.RouterID == routerId
                             && x.ConnectivityStatus == 40
                             && x.ActivateCustomerID != activateCustomerId)
                    .Select(x => x.VLanID)
                    .ToList()
                    .Select(v => Convert.ToString(v))
                    .ToList();
            }

            return Ok(new
            {
                used,
                vlans = ((IEnumerable<object>)vlans).Select(v => new { id = (string)((dynamic)v).id, name = (string)((dynamic)v).name }),
                users = ((IEnumerable<object>)users).Select(u => new { name = (string)((dynamic)u).name, password = (string)((dynamic)u).password }),
                groups = ((IEnumerable<object>)groups).Select(u => new { name = (string)((dynamic)u).name }),
                profiles = ((IEnumerable<object>)profiles).Select(p => new { name = (string)((dynamic)p).name }),
            });
        }

        [HttpGet, Route("vlan-address")]
        public IHttpActionResult VlanAddress(int routerId, string vlanName, string vlanId = null)
        {
            var rows = Mk.GetVlanAddress(vlanName, routerId, vlanId);
            dynamic first = rows.FirstOrDefault();
            return Ok(new { address = first == null ? "" : (string)first.address, network = first == null ? "" : (string)first.network });
        }

        [HttpGet, Route("lookup")]
        public IHttpActionResult Lookup(string kind, string q = "", string loc = "")
        {
            var html = new HTML();
            int sys = Convert.ToInt32(html.SystemID);
            int wh = Convert.ToInt32(html.WarehouseID);
            var term = (q ?? "").ToLower();

            using (var ctx = new DBContext())
            {
                switch (kind)
                {
                    case "device":
                        return Ok(ctx.TblDeviceName.AsNoTracking()
                            .Where(x => x.SystemID == sys && x.WarehouseID == wh && x.DeviceName.ToLower().Contains(term))
                            .OrderBy(x => x.DeviceName)
                            .Take(10)
                            .Select(x => new { id = x.DeviceNameID, name = x.DeviceName, meta = "" })
                            .ToList());

                    case "splitter":
                        return Ok(ctx.TblDeviceSplitterName.AsNoTracking()
                            .Where(x => x.SystemID == sys && x.WarehouseID == wh && x.SplitterName.ToLower().Contains(term))
                            .OrderBy(x => x.SplitterName)
                            .Take(10)
                            .Select(x => new { id = x.SplitterNameID, name = x.SplitterName, meta = "" })
                            .ToList());

                    case "warehouse":
                        return Ok(ctx.TblUserWarehouse.AsNoTracking()
                            .Where(x => x.SystemID == sys && x.WarehouseID == wh && x.WarehouseName.ToLower().Contains(term))
                            .OrderBy(x => x.WarehouseName)
                            .Take(10)
                            .Select(x => new { id = x.WID, name = x.WarehouseName, meta = "" })
                            .ToList());

                    case "installer":
                        return Ok(ctx.TblUserLogin.AsNoTracking()
                            .Join(
                                ctx.TblUserRole.AsNoTracking(),
                                u => u.UserRoleID,
                                r => r.UserRoleID,
                                (u, r) => new { u, r }
                            )
                            .Where(x =>
                                x.r.SystemID == sys &&
                                x.r.UserRoleID == 23 &&
                                x.u.FullName.Contains(term)
                            )
                            .Select(x => x.u.FullName)
                            .Distinct()
                            .OrderBy(n => n)
                            .Take(10)
                            .Select(n => new
                            {
                                id = n,
                                name = n,
                                meta = ""
                            })
                            .ToList());

                    case "item":
                        {
                            int locId;
                            if (!int.TryParse(loc, out locId)) return Ok(new object[0]);

                            term = (term ?? "").Trim().ToLower();

                            var rows = (from i in ctx.TblItem.AsNoTracking()
                                    from srd in ctx.TblItemSerialDetail
                                        .Where(s => s.ItemID == i.ItemID)
                                        .DefaultIfEmpty()
                                    join iv in ctx.TblItemVariant on i.ItemID equals iv.ItemID
                                    from sd in ctx.TblStockInDetail
                                        .Where(s => s.ItemID == i.ItemID &&
                                            (s.SerialNumber == null || s.SerialNumber == "" ||
                                                s.SerialNumber == srd.SerialNumber))
                                        .DefaultIfEmpty()
                                    from uw in ctx.TblUserWarehouse
                                        .Where(w => w.WID == sd.LocationID)
                                        .DefaultIfEmpty()
                                    join sn in ctx.TblCategory on i.StockNameID equals sn.StockNameID
                                    let fullName = i.ItemName
                                        + ((i.ItemCode ?? "") != "" ? " - " + i.ItemCode : "")
                                        + ((srd.SerialNumber ?? "") != "" ? " - " + srd.SerialNumber : "")
                                    where fullName.ToLower().Contains(term)
                                        && sd.LocationID == locId
                                        && i.SystemID == sys
                                        && i.WarehouseID == wh
                                        && ((srd.SerialNumber ?? "") == "" ||
                                            ctx.TblStockInDetail.Any(z => z.StockInID == srd.StockInID &&
                                                (z.Type == "Stock In" || z.Type == "Opening Stock")))
                                    select new { i, iv, sn, srd, sd, uw, fullName })
                            .GroupBy(x => new
                            {
                                x.i.ItemID,
                                x.fullName,
                                WID = (int?)x.uw.WID,
                                SerialId = (int?)x.srd.ItemSerialDetailID,
                                x.srd.SerialNumber,
                                x.iv.SellingPrice,
                                x.iv.UM,
                                x.sn.StockName,
                                x.sd.LocationID,
                                x.uw.WarehouseName
                            })
                            .Select(g => new
                            {
                                g.Key.ItemID,
                                Name = g.Key.fullName,
                                SalePrice = g.Key.SellingPrice,
                                g.Key.UM,
                                g.Key.SerialNumber,
                                g.Key.StockName,
                                g.Key.LocationID,
                                g.Key.WarehouseName,
                                g.Key.SerialId,
                                QTY = g.Sum(x =>
                                    ((x.srd.SerialNumber ?? "") == "" || x.srd.SerialNumber == "N/A")
                                        ? ((double?)x.sd.QTY ?? 0) - ((double?)x.sd.QTYOUT ?? 0)
                                    : (x.i.ItemID == x.sd.ItemID && x.sd.SerialNumber == x.srd.SerialNumber)
                                        ? ((double?)x.sd.QTY ?? 0) - ((double?)x.sd.QTYOUT ?? 0)
                                    : (x.srd.StockInID == x.sd.StockInID &&
                                        (x.sd.Type == "Stock In" || x.sd.Type == "Opening Stock"))
                                        ? 1.0
                                        : 0.0)
                            })
                            .Where(r => r.QTY > 0)      
                            .OrderBy(r => r.Name)
                            .Take(20)
                            .ToList();

                        return Ok(rows.Select(r => new
                        {
                            id = r.ItemID,
                            name = r.Name,
                            r.SalePrice,
                            r.UM,
                            r.SerialNumber,
                            r.StockName,
                            r.LocationID,
                            r.WarehouseName,
                            ItemSerialDetailID = r.SerialId ?? 0,
                            r.QTY,
                            meta = r.UM + " · " + r.StockName + " · stock " + r.QTY
                        }).ToList());
                    }

                    default:
                        return BadRequest("Unknown lookup.");
                }
            }
        }

        [HttpPost, Route("set-connectivity")]
        public async Task<IHttpActionResult> SetConnectivity(SetConnectivityRequest req)
        {
            var html = new HTML();
            var o = await ConnectivityWorkflow.RunAsync(html, req.ActivateCustomerID, req.Status, req.Comment);
            return Ok(new { success = o.Success, message = o.Message, newStatus = o.NewStatus, dbWasWrong = o.DbWasWrong });
        }

        [HttpPost, Route("save-change-activate")]
        public async Task<IHttpActionResult> SaveChangeActivate(SaveChangeActivateRequest req)
        {
            var error = ActivationValidator.Validate(req);
            if (error != null) return BadRequest(error);

            var a = req.ActivateCustomerData.First();
            var dev = req.DeviceInformation.First();
            var items = req.DeviceInformationDetail ?? new List<CustomerDeviceInfoDetail>();
            var user = new HTML().UserFullName;
            var now = AppTime.Now;
            var notes = string.IsNullOrEmpty(dev.CustomerNotes) ? "" : Uri.UnescapeDataString(dev.CustomerNotes);

            bool isInsert;
            int activateId;
            string packageName = null;
            DateTime? startDate = null;
            DateTime? endDate = null;

            try
            {
                using (var ctx = new DBContext())
                using (var tx = ctx.Database.BeginTransaction())
                {
                    var existing = a.ActivateCustomerID > 0
                        ? ctx.TblActivateCustomer.Find(a.ActivateCustomerID)
                        : null;
                    isInsert = existing == null;

                    var usernameError = ActivationSaveService.ValidatePPPoEUsername(ctx, a);
                    if (usernameError != null) return BadRequest(usernameError);

                    var before = isInsert ? null : ActivationSnapshot.Load(ctx, a.ActivateCustomerID);

                    var inv = ActivationSaveService.LoadInvoice(ctx, a.BillingInvoiceID);
                    if (inv == null) return BadRequest("Billing invoice not found.");

                    string comment = ActivationSaveService.EnsureDeviceEnabledOnRouter(a);

                    existing = ActivationSaveService.UpsertActivation(ctx, existing, a, inv, user, now, comment);
                    ActivationSaveService.SyncBilling(ctx, inv.Bill, existing, isInsert, user, now);

                    var info = ActivationSaveService.UpsertDeviceInfo(ctx, existing, dev, notes, inv.Bill.DomainID, user, now);
                    ActivationSaveService.ReplaceEquipment(ctx, info, existing, items, dev, user, now);

                    var changes = ActivationChangeLog.Build(before, a, dev, notes, items);

                    if (isInsert || changes.Any)
                    {
                        ConnectivityRepository.InsertMovement(
                            ctx,
                            existing.ActivateCustomerID,
                            isInsert ? "Activate" : "Edit",
                            isInsert ? (int?)null : existing.ConnectivityStatus,
                            existing.ConnectivityStatus,
                            isInsert ? "New activation" : "Activation edited",
                            "SaveChangeActivate",
                            null,
                            user,
                            now,
                            extraChanges: changes?.Extra
                        );
                    }

                    tx.Commit();
                    activateId = existing.ActivateCustomerID;

                    if (isInsert)
                    {
                        packageName = inv.Bill.PackageName;      
                        startDate = inv.Bill.StartDate;
                        endDate = inv.Bill.EndDate; 
                    }
                }

                if (isInsert)
                    await ConnectivityWorkflow.ActivateMessage(
                        new HTML(),
                        activateId,
                        user,
                        now,
                        notes,
                        packageName,
                        startDate,
                        endDate);

                return Ok(new
                {
                    Result = 2,
                    Message = isInsert ? "Activation saved successfully." : "Activation updated successfully.",
                    ActivateCustomerID = activateId
                });
            }
            catch (Exception ex)
            {
                return Ok(new { Result = 0, Message = ex.GetBaseException().Message });
            }
        }
    }
}