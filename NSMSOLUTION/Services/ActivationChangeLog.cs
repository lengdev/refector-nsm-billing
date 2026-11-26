using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSMSOLUTION.Services
{
    public class ChangeResult
    {
        public bool Any;        // anything changed at all (connection fields included)
        public string Extra;    // text for fields the SQL in InsertMovement does not track
    }

    public static class ActivationChangeLog
    {
        public static ChangeResult Build(
            ActivationSnapshot before,
            ActivateCustomer a,
            CustomerDeviceInfo dev,
            string notes,
            IList<CustomerDeviceInfoDetail> items)
        {
            var conn = new ChangeBuilder();
            var extra = new ChangeBuilder();

            // NEW ACTIVATION OR EDIT ACTIVATION
            bool isNew = before == null;

            var oldAct = before?.Act;
            var oldInfo = before?.Info;

            notes = notes ?? "";
            items = items ?? new List<CustomerDeviceInfoDetail>();

            // ==========================================
            // 1. CONNECTION CONFIGURATION
            // ==========================================

            // For new activation, use empty old values.
            // For edit, compare against previous values.

            conn.Field("Type",
                oldAct?.ConnectionType, a.ConnectionType);

            conn.Field("Router",
                oldAct?.RouterID, a.RouterID);

            conn.Field("PPPoE user",
                oldAct?.PPPoEUserName, a.PPPoEUserName);

            conn.Field("PPPoE profile",
                oldAct?.PPPoEProfileName, a.PPPoEProfileName);

            conn.Field("Group",
                oldAct?.UserGroup, a.UserGroup);

            conn.Field("VLAN",
                oldAct?.VLanName, a.VLanName);

            conn.Field("VLAN IP",
                oldAct?.VLanIPAddress, a.VLanIPAddress);

            conn.Field("IP",
                oldAct?.IPAddress, a.IPAddress);

            conn.Field("Gateway",
                oldAct?.GetwayIP, a.GetwayIP);

            conn.Field("Subnet",
                oldAct?.SubnetIP, a.SubnetIP);

            conn.Field("DNS1",
                oldAct?.DNS1, a.DNS1);

            conn.Field("DNS2",
                oldAct?.DNS2, a.DNS2);


            // ==========================================
            // 2. INITIAL CONNECTION CONFIGURATION
            // ==========================================

            // SQL InsertMovement already handles connection
            // fields for existing edits.
            //
            // For NEW activation, explicitly append all
            // initial connection values to Extra.

            if (isNew)
            {
                extra.Field("Type", null, a.ConnectionType);
                extra.Field("Router", null, a.RouterID);

                extra.Field("PPPoE user", null, a.PPPoEUserName);
                extra.Field("PPPoE profile", null, a.PPPoEProfileName);
                extra.Field("PPPoE password", null, a.PPPoEPassword, true);
                extra.Field("Fixed IP", null, a.PPPoEIPAddress);
                extra.Field("Group", null, a.UserGroup);

                extra.Field("VLAN ID", null, a.VLanID);
                extra.Field("VLAN", null, a.VLanName);
                extra.Field("VLAN IP", null, a.VLanIPAddress);

                extra.Field("IP", null, a.IPAddress);
                extra.Field("Gateway", null, a.GetwayIP);
                extra.Field("Subnet", null, a.SubnetIP);
                extra.Field("DNS1", null, a.DNS1);
                extra.Field("DNS2", null, a.DNS2);

                extra.Field("Activate date", null, a.ActivateDate);
                extra.Field("Customer router", null, a.DeviceName);

                extra.Field("Router IP", null, a.WifiIP);
                extra.Field("WiFi name", null, a.WifiName);
                extra.Field("WiFi key", null, a.WifiKey, true);
                extra.Field("Router login", null, a.WifiLoginName);
                extra.Field("Router password", null, a.WifiLoginPassword, true);
            }
            else
            {
                // ==========================================
                // 3. EDIT CONNECTION EXTRAS
                // ==========================================

                extra.Field("PPPoE password",
                    oldAct?.PPPoEPassword, a.PPPoEPassword, true);

                extra.Field("Fixed IP",
                    oldAct?.PPPoEIPAddress, a.PPPoEIPAddress);

                extra.Field("Activate date",
                    oldAct?.ActivateDate, a.ActivateDate);

                extra.Field("Customer router",
                    oldAct?.DeviceName, a.DeviceName);

                extra.Field("Router IP",
                    oldAct?.WifiIP, a.WifiIP);

                extra.Field("WiFi name",
                    oldAct?.WifiName, a.WifiName);

                extra.Field("WiFi key",
                    oldAct?.WifiKey, a.WifiKey, true);

                extra.Field("Router login",
                    oldAct?.WifiLoginName, a.WifiLoginName);

                extra.Field("Router password",
                    oldAct?.WifiLoginPassword, a.WifiLoginPassword, true);
            }


            // ==========================================
            // 4. DEVICE INFORMATION
            // ==========================================

            if (dev != null)
            {
                extra.Field("OLT / device",
                    oldInfo?.DeviceName, dev.DeviceName);

                extra.Field("PON",
                    oldInfo?.PONPort, dev.PONPort);

                extra.Field("ONT ID",
                    oldInfo?.OLTID, dev.OLTID);

                extra.Field("Splitter",
                    oldInfo?.SplitterName, dev.SplitterName);

                extra.Field("Splitter port",
                    oldInfo?.Port, dev.Port);

                extra.Field("Length",
                    oldInfo?.Length, dev.Length);

                extra.Field("Loss dB",
                    oldInfo?.LossDB, dev.LossDB);

                extra.Field("Google map",
                    oldInfo?.GoogleMap, dev.GoogleMap);

                extra.Field("Warehouse",
                    oldInfo?.Warehouse, dev.Warehouse);

                extra.Field("Installed by",
                    oldInfo?.InstallBy, dev.InstallBy);

                extra.Field("Installed date",
                    oldInfo?.CompleteInstallDate, dev.CompleteInstallDate);

                extra.Field("Notes",
                    oldInfo?.CustomerNotes, notes);
            }


            // ==========================================
            // 5. EQUIPMENT
            // ==========================================

            Func<object, object, string> key = (sid, iid) =>
                Convert.ToInt32(sid ?? 0) > 0
                    ? "s" + sid
                    : "i" + iid;

            var oldItems = before?.Items
                ?? new List<CustomerDeviceInfoDetail>();

            var oldMap = oldItems
                .GroupBy(x => key(x.ItemSerialDetailID, x.ItemID))
                .ToDictionary(g => g.Key, g => g.First());

            var newMap = items
                .GroupBy(x => key(x.ItemSerialDetailID, x.ItemID))
                .ToDictionary(g => g.Key, g => g.First());


            // Newly added equipment
            foreach (var kv in newMap.Where(x => !oldMap.ContainsKey(x.Key)))
            {
                extra.Add(
                    "Equipment",
                    "+ " + kv.Value.ItemName + " " + kv.Value.SerialNo
                );
            }


            // Removed equipment
            foreach (var kv in oldMap.Where(x => !newMap.ContainsKey(x.Key)))
            {
                extra.Add(
                    "Equipment",
                    "− " + kv.Value.ItemName + " " + kv.Value.SerialNo
                );
            }


            // Modified equipment
            foreach (var kv in newMap.Where(x => oldMap.ContainsKey(x.Key)))
            {
                var oldItem = oldMap[kv.Key];
                var newItem = kv.Value;

                extra.Field(
                    newItem.ItemName + " serial",
                    oldItem.SerialNo,
                    newItem.SerialNo
                );

                extra.Field(
                    newItem.ItemName + " MAC",
                    oldItem.MACAddress,
                    newItem.MACAddress
                );

                extra.Field(
                    newItem.ItemName + " qty",
                    oldItem.QTY,
                    newItem.QTY
                );
            }


            // ==========================================
            // 6. RESULT
            // ==========================================

            return new ChangeResult
            {
                Any = isNew || conn.Any || extra.Any,
                Extra = extra.ToString()
            };
        }
    }
}