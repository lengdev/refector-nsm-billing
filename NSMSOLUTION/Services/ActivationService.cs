using NSMSOLUTION.Models.EFCore;
using System;

namespace NSMSOLUTION.Services
{
    public enum ConnectivityAction { Activate, Deactivate }
    public enum RouterState { NotFound, Enabled, Disabled }

    public class ConnectivityResult
    {
        public bool Skipped { get; set; }
        public bool RealEnabled { get; set; }
        public bool DbWasWrong { get; set; }
        public int NewStatus { get; set; }
        public string Message { get; set; }
    }

    public static class ConnectivityStatusCodes
    {
        public const int Active = 40;
        public const int Locked = 41;
        public const int Terminated = 42;
        public const int Inactive = 43;
        public const int Expired = 50;   // enabled on router but profile cleared
    }

    public static class ConnectionTypes
    {
        public const string PPPoE = "pppoe";
        public const string Static = "static";
        public const string Manual = "manual";
        public static string Normalize(string v) => (v ?? "").Trim().ToLowerInvariant();
    }

    public static class ConnectivityText
    {
        public static string Past(int status)
        {
            switch (status)
            {
                case ConnectivityStatusCodes.Active: return "reactivated";
                case ConnectivityStatusCodes.Locked: return "locked";
                case ConnectivityStatusCodes.Terminated: return "terminated";
                case ConnectivityStatusCodes.Inactive: return "inactivated";
                default: return "updated";
            }
        }
    }

    public static class AppTime
    {
        // Phnom Penh. One place instead of FindSystemTimeZoneById in every action.
        private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        public static DateTime Now => TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, Zone);
    }

    public static class ActivationService
    {
        private static readonly MikrotikService Mikrotik = new MikrotikService();

        public static ConnectivityResult Activate(ActivateCustomer c, string comment = "", bool allowFromTerminated = false)
            => Apply(c, ConnectivityAction.Activate, ConnectivityStatusCodes.Inactive, comment, allowFromTerminated);

        public static ConnectivityResult Deactivate(ActivateCustomer c, string comment = "",
            int offStatus = ConnectivityStatusCodes.Inactive)
            => Apply(c, ConnectivityAction.Deactivate, offStatus, comment, false);

        public static ConnectivityResult Apply(ActivateCustomer c, ConnectivityAction action,
            int offStatus, string comment = "", bool allowFromTerminated = false)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));

            bool wantEnabled = action == ConnectivityAction.Activate;
            bool dbSaysEnabled = c.ConnectivityStatus == ConnectivityStatusCodes.Active;
            string type = ConnectionTypes.Normalize(c.ConnectionType);
            int newStatus = wantEnabled ? ConnectivityStatusCodes.Active : offStatus;

            if (wantEnabled && !allowFromTerminated && c.ConnectivityStatus == ConnectivityStatusCodes.Terminated)
                throw new SoftFailureException("Customer is terminated and cannot be activated.");

            // Manual connections have nothing on the router
            if (type == ConnectionTypes.Manual)
                return new ConnectivityResult
                {
                    RealEnabled = wantEnabled,
                    NewStatus = newStatus,
                    DbWasWrong = false,
                    Message = "Manual connection: status updated only."
                };

            try
            {
                var real = CheckDeviceStatus(c.RouterID, type, c.PPPoEUserName, c.VLanName);

                if (real == RouterState.NotFound)
                    throw new NotFoundException(type == ConnectionTypes.PPPoE
                        ? $"User '{c.PPPoEUserName}' not found on router."
                        : $"VLAN '{c.VLanName}' not found on router.");

                bool realEnabled = real == RouterState.Enabled;
                bool dbWasWrong = realEnabled != dbSaysEnabled;

                // Status 50: user is enabled but profile was cleared -> must re-apply profile
                bool reapplyProfile = wantEnabled && type == ConnectionTypes.PPPoE
                                      && c.ConnectivityStatus == ConnectivityStatusCodes.Expired;

                if (realEnabled == wantEnabled && !reapplyProfile)
                    return new ConnectivityResult
                    {
                        Skipped = true,
                        RealEnabled = realEnabled,
                        DbWasWrong = dbWasWrong,
                        NewStatus = realEnabled ? ConnectivityStatusCodes.Active : offStatus,
                        Message = wantEnabled ? "Already active on router." : "Already inactive on router."
                    };

                if (type == ConnectionTypes.PPPoE)
                {
                    var req = new UserActionRequest { RouterID = c.RouterID, Comment = comment };
                    if (wantEnabled)
                    {
                        if (reapplyProfile)
                        {
                            Mikrotik.ClearUserProfile(c.PPPoEUserName, c.RouterID);
                            Mikrotik.AppendProfile(new AppendProfileRequest
                            {
                                RouterID = c.RouterID,
                                PPPoEUserName = c.PPPoEUserName,
                                PPPoEProfileName = c.PPPoEProfileName,
                                Comment = comment
                            });
                        }
                        if (real == RouterState.Disabled) Mikrotik.ReactivateUser(c.PPPoEUserName, req);
                    }
                    else Mikrotik.LockUser(c.PPPoEUserName, req);
                }
                else
                {
                    var req = new VlanActionRequest { RouterID = c.RouterID, VlanId = c.VLanID, Comment = comment };
                    if (wantEnabled) Mikrotik.EnableVlan(c.VLanName, req);
                    else Mikrotik.DisableVlan(c.VLanName, req);
                }

                return new ConnectivityResult
                {
                    RealEnabled = wantEnabled,
                    DbWasWrong = dbWasWrong,
                    NewStatus = newStatus,
                    Message = wantEnabled ? "Activated successfully." : "Deactivated successfully."
                };
            }
            catch (NotFoundException) { throw; }
            catch (SoftFailureException) { throw; }
            catch (ConflictException) { throw; }
            catch (ArgumentException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"{(wantEnabled ? "Activation" : "Deactivation")} failed: {ex.Message}", ex);
            }
        }

        public static RouterState CheckDeviceStatus(int routerId, string type, string pppoeUserName, string vlanName)
        {
            type = ConnectionTypes.Normalize(type);
            if (type == ConnectionTypes.PPPoE)
                return Mikrotik.GetUserState(pppoeUserName, routerId);
            else if (type == ConnectionTypes.Static)
                return Mikrotik.GetVlanState(vlanName, routerId, "");
            else
                throw new ArgumentException($"Unknown connection type '{type}'.");
        }
    }
}