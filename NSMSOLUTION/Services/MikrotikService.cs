using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tik4net;

namespace NSMSOLUTION.Services
{
    // ── Exceptions used to carry HTTP-shape intent up to the controller ────
    // (the service stays HTTP-agnostic; these just say *what kind* of failure)
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

    // Original code returned 200 OK with { success:false, message } for these
    // rather than a real error status — preserved as-is rather than "fixed",
    // since callers may depend on the 200.
    public class SoftFailureException : Exception
    {
        public SoftFailureException(string message) : base(message) { }
    }

    // ── Result DTOs for endpoints returning structured (not list) data ─────
    public class ExistsResult
    {
        public bool Exists { get; set; }
        public string Id { get; set; }
    }

    public class UserStatusResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Disabled { get; set; }
        public string ActualProfile { get; set; }
        public string LastSeen { get; set; }
        public string Comment { get; set; }
    }

    public class UserProfileResult
    {
        public string ProfileId { get; set; }
        public string ProfileName { get; set; }
        public string User { get; set; }
        public List<object> Limitations { get; set; }
    }

    public class VlanStatusResult
    {
        public string InternalId { get; set; }
        public string Name { get; set; }
        public string Disabled { get; set; }
        public string VlanId { get; set; }
        public string Iface { get; set; }
        public string Comment { get; set; }
    }

    // ── Request models (unchanged from controller) ──────────────────────
    public class RouterRef
    {
        public int RouterID { get; set; }
    }

    public class CreateUserRequest : RouterRef
    {
        public string PPPoEUserName { get; set; }
        public string PPPoEPassword { get; set; }
        public string PPPoEProfileName { get; set; }
        public string Comment { get; set; } = "";
        public string IPPOOL { get; set; } = "";
        public string PPPoEIPAddress { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Location { get; set; } = "";
    }

    public class UpdateUserRequest : RouterRef
    {
        public string OldPPPoEUserName { get; set; }
        public string PPPoEUserName { get; set; }
        public string PPPoEPassword { get; set; }
    }

    public class AppendProfileRequest : RouterRef
    {
        public string PPPoEUserName { get; set; }
        public string PPPoEProfileName { get; set; }
        public string Comment { get; set; } = "";
    }

    public class UserActionRequest : RouterRef
    {
        public string Comment { get; set; } = "";
    }

    public class CreateProfileRequest : RouterRef
    {
        public string ProfileName { get; set; }
        public string Validity { get; set; }
        public string Price { get; set; } = "0";
    }

    public class UpdateProfileRequest : RouterRef
    {
        public string Validity { get; set; }
        public string Price { get; set; } = "0";
    }

    public class CreateLimitationRequest : RouterRef
    {
        public string LimitationName { get; set; }
        public string Download { get; set; }
        public string Upload { get; set; }
        public string IPPOOL { get; set; } = "";
        public string Prefix { get; set; } = "M";
    }

    public class UpdateLimitationRequest : RouterRef
    {
        public int RX { get; set; }
        public int TX { get; set; }
        public int MinRX { get; set; }
        public int MinTX { get; set; }
        public string IPPOOL { get; set; } = "";
    }

    public class ProfileLimitationRequest : RouterRef
    {
        public string ProfileName { get; set; }
        public string LimitationName { get; set; }
        public string FromTime { get; set; } = "0s";
        public string ToTime { get; set; } = "23h59m59s";
    }

    public class VlanActionRequest
    {
        public int RouterID { get; set; }
        public string Comment { get; set; }
        public string VlanId { get; set; }
    }

    public class VlanRenameRequest
    {
        public int RouterID { get; set; }
        public string NewName { get; set; }
        public string VlanId { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════
    // SERVICE — all MikroTik logic. HTTP-agnostic: throws plain/custom
    // exceptions on failure, returns plain data on success. The controller
    // owns turning that into IHttpActionResult.
    // ══════════════════════════════════════════════════════════════════
    public class MikrotikService
    {
        private class RouterInfo
        {
            public int RouterID { get; set; }
            public string RouterName { get; set; }
            public string RouterIp { get; set; }
            public int RouterPort { get; set; }
            public string RouterUser { get; set; }
            public string RouterPass { get; set; }
            public int? MikroTikVersionID { get; set; }
        }

        // ── Connection helpers ──────────────────────────────────────────

        private RouterInfo GetRouterInfo(int routerId)
        {
            using (var ctx = new DBContext())
            {
                var router = ctx.Database.SqlQuery<RouterInfo>(@"
                    SELECT RouterID, RouterName,
                           IPAddress       AS RouterIp,
                           Port            AS RouterPort,
                           UserName        AS RouterUser,
                           Password        AS RouterPass,
                           MikroTikVersionID
                    FROM   dbo.TblRouter
                    WHERE  RouterID = @p0
                ", routerId).FirstOrDefault();

                return router == null ? throw new Exception($"Router with ID {routerId} not found in database.") : router;
            }
        }

        private ITikConnection OpenConnection(RouterInfo router)
        {
            ITikConnection conn = null;
            Exception captured = null;

            var task = Task.Run(() =>
            {
                try
                {
                    conn = ConnectionFactory.CreateConnection(TikConnectionType.Api);
                    conn.Open(router.RouterIp, router.RouterPort, router.RouterUser, router.RouterPass);
                }
                catch (Exception ex)
                {
                    captured = ex;
                }
            });

            bool completed = task.Wait(TimeSpan.FromSeconds(2));

            if (!completed)
            {
                conn?.Dispose();
                throw new TimeoutException($"Connection to router {router.RouterIp}:{router.RouterPort} timed out.");
            }

            if (captured != null)
            {
                conn?.Dispose();
                throw captured;
            }

            return conn;
        }

        private void SafeClose(ITikConnection conn)
        {
            if (conn == null) return;
            try { conn.Close(); } catch { }
            try { (conn as IDisposable)?.Dispose(); } catch { }
        }

        private string W(ITikReSentence s, string key)
            => s.Words.ContainsKey(key) ? s.Words[key] : "";

        // ── Version-aware command helpers ───────────────────────────────

        private bool IsV1(RouterInfo router) => router.MikroTikVersionID == 1;

        private string UmPath(RouterInfo router, string suffix)
            => IsV1(router) ? $"/tool/user-manager{suffix}" : $"/user-manager{suffix}";

        private string UserKey(RouterInfo router) => IsV1(router) ? "username" : "name";

        private IEnumerable<ITikReSentence> PrintUsers(ITikConnection conn, RouterInfo router, string username = null)
        {
            try
            {
                var cmd = username != null
                    ? conn.CreateCommandAndParameters(UmPath(router, "/user/print"), UserKey(router), username)
                    : conn.CreateCommandAndParameters(UmPath(router, "/user/print"));
                return cmd.ExecuteList();
            }
            catch (NotImplementedException) { return Enumerable.Empty<ITikReSentence>(); }
        }

        private IEnumerable<ITikReSentence> PrintUserProfiles(ITikConnection conn, RouterInfo router,
            string filterKey = null, string filterValue = null)
        {
            try
            {
                var cmd = filterKey != null
                    ? conn.CreateCommandAndParameters(UmPath(router, "/user-profile/print"), filterKey, filterValue)
                    : conn.CreateCommandAndParameters(UmPath(router, "/user-profile/print"));
                return cmd.ExecuteList();
            }
            catch (NotImplementedException) { return Enumerable.Empty<ITikReSentence>(); }
        }

        private ITikReSentence FindVlanByID(ITikConnection conn, string vlanId = null)
        {
            IEnumerable<ITikReSentence> rowsById;
            try { rowsById = conn.CreateCommandAndParameters("/interface/vlan/print", ".id", vlanId).ExecuteList(); }
            catch (NotImplementedException) { rowsById = Enumerable.Empty<ITikReSentence>(); }
            var match = rowsById.FirstOrDefault();
            return match;
        }

        private ITikReSentence FindVlanByName(ITikConnection conn, string vlanName)
        {
            IEnumerable<ITikReSentence> rows;
            try { rows = conn.CreateCommandAndParameters("/interface/vlan/print", "name", vlanName).ExecuteList(); }
            catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }
            var match = rows.FirstOrDefault();
            return match;
        }

        // ── Connection test ──────────────────────────────────────────────

        public void TestConnection(int routerId)
        {
            var router = GetRouterInfo(routerId);
            var conn = OpenConnection(router);
            SafeClose(conn);
        }

        // ── PPPoE users ──────────────────────────────────────────────────

        public List<object> GetPPPoEUsers(int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var rows = PrintUsers(conn, router);
                return rows.Select(s => (object)new
                {
                    id = W(s, ".id"),
                    name = W(s, IsV1(router) ? "username" : "name"),
                    password = W(s, "password"),
                    disabled = W(s, "disabled"),
                    comment = W(s, "comment"),
                    group = W(s, "group"),
                    actualProfile = W(s, "actual-profile"),
                    lastSeen = W(s, "last-seen")
                }).ToList();
            }
            finally { SafeClose(conn); }
        }

        public ExistsResult CheckUserExists(int routerId, string username)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = PrintUsers(conn, router, username).FirstOrDefault();
                return new ExistsResult { Exists = match != null, Id = match != null ? W(match, ".id") : "" };
            }
            finally { SafeClose(conn); }
        }

        public UserStatusResult GetPPPoEUserStatus(int routerId, string username)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = PrintUsers(conn, router, username).FirstOrDefault();
                return match == null
                    ? throw new SoftFailureException($"User '{username}' not found.")
                    : new UserStatusResult
                    {
                        Id = W(match, ".id"),
                        Name = W(match, IsV1(router) ? "username" : "name"),
                        Disabled = W(match, "disabled"),
                        ActualProfile = W(match, "actual-profile"),
                        LastSeen = W(match, "last-seen"),
                        Comment = W(match, "comment")
                    };
            }
            finally { SafeClose(conn); }
        }

        public string CreateOrUpdateUser(CreateUserRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var existing = PrintUsers(conn, router, req.PPPoEUserName).FirstOrDefault();

                if (existing == null)
                {
                    try
                    {
                        conn.CreateCommandAndParameters(UmPath(router, "/user/add"),
                            UserKey(router), req.PPPoEUserName,
                            "password", req.PPPoEPassword,
                            "comment", req.Comment,
                            "group", req.IPPOOL,
                            "shared-users", "1"
                        ).ExecuteNonQuery();

                        conn.CreateCommandAndParameters(UmPath(router, "/user-profile/add"),
                            "profile", req.PPPoEProfileName,
                            "user", req.PPPoEUserName
                        ).ExecuteNonQuery();
                    }
                    catch (NotImplementedException) { /* !empty on add = OK */ }

                    return "User created successfully.";
                }
                else
                {
                    string userId = W(existing, ".id");
                    bool maskPassword = req.PPPoEPassword.Contains("*");

                    if (maskPassword)
                        conn.CreateCommandAndParameters(UmPath(router, "/user/set"),
                            ".id", userId,
                            UserKey(router), req.PPPoEUserName,
                            "comment", req.Comment
                        ).ExecuteNonQuery();
                    else
                        conn.CreateCommandAndParameters(UmPath(router, "/user/set"),
                            ".id", userId,
                            UserKey(router), req.PPPoEUserName,
                            "password", req.PPPoEPassword,
                            "comment", req.Comment
                        ).ExecuteNonQuery();

                    conn.CreateCommandAndParameters(UmPath(router, "/user-profile/add"),
                        "profile", req.PPPoEProfileName,
                        "user", req.PPPoEUserName
                    ).ExecuteNonQuery();

                    return "User updated successfully.";
                }
            }
            finally { SafeClose(conn); }
        }

        public void UpdateUser(UpdateUserRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);

                if (!string.Equals(req.OldPPPoEUserName, req.PPPoEUserName, StringComparison.OrdinalIgnoreCase))
                {
                    var conflict = PrintUsers(conn, router, req.PPPoEUserName);
                    if (conflict.Any())
                        throw new ConflictException($"Username '{req.PPPoEUserName}' is already in use.");
                }

                var match = PrintUsers(conn, router, req.OldPPPoEUserName).FirstOrDefault();
                if (match == null)
                    throw new NotFoundException($"User '{req.OldPPPoEUserName}' not found.");

                bool maskPassword = req.PPPoEPassword.Contains("*");

                if (maskPassword)
                    conn.CreateCommandAndParameters(UmPath(router, "/user/set"),
                        ".id", W(match, ".id"),
                        UserKey(router), req.PPPoEUserName
                    ).ExecuteNonQuery();
                else
                    conn.CreateCommandAndParameters(UmPath(router, "/user/set"),
                        ".id", W(match, ".id"),
                        UserKey(router), req.PPPoEUserName,
                        "password", req.PPPoEPassword
                    ).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void RemoveUser(string username, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = PrintUsers(conn, router, username).FirstOrDefault();
                if (match == null)
                    throw new NotFoundException($"User '{username}' not found.");

                conn.CreateCommandAndParameters(UmPath(router, "/user/remove"), ".id", W(match, ".id"))
                    .ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void RemoveUserById(string userId, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                conn.CreateCommandAndParameters(UmPath(router, "/user/remove"), ".id", userId).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void EnableUser(string userId, UserActionRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                conn.CreateCommandAndParameters(UmPath(router, "/user/set"),
                    ".id", userId, "disabled", "no", "comment", req.Comment
                ).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void DisableUser(string userId, UserActionRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                conn.CreateCommandAndParameters(UmPath(router, "/user/set"),
                    ".id", userId, "disabled", "yes", "comment", req.Comment
                ).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void LockUser(string username, UserActionRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = PrintUsers(conn, router, username).FirstOrDefault();
                if (match == null)
                    throw new NotFoundException($"User '{username}' not found.");

                conn.CreateCommandAndParameters(UmPath(router, "/user/set"),
                    ".id", W(match, ".id"), UserKey(router), username,
                    "disabled", "yes", "comment", req.Comment
                ).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void ReactivateUser(string username, UserActionRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = PrintUsers(conn, router, username).FirstOrDefault();
                if (match == null)
                    throw new NotFoundException($"User '{username}' not found.");

                conn.CreateCommandAndParameters(UmPath(router, "/user/set"),
                    ".id", W(match, ".id"), UserKey(router), username,
                    "disabled", "no", "comment", req.Comment
                ).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void SetUserComment(string username, UserActionRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = PrintUsers(conn, router, username).FirstOrDefault();
                if (match == null)
                    throw new NotFoundException($"User '{username}' not found.");

                conn.CreateCommandAndParameters(UmPath(router, "/user/set"),
                    ".id", W(match, ".id"), "comment", req.Comment
                ).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void AppendProfile(AppendProfileRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                conn.CreateCommandAndParameters(UmPath(router, "/user-profile/add"),
                    "profile", req.PPPoEProfileName, "user", req.PPPoEUserName
                ).ExecuteNonQuery();

                if (!string.IsNullOrEmpty(req.Comment))
                {
                    var match = PrintUsers(conn, router, req.PPPoEUserName).FirstOrDefault();
                    if (match != null)
                        conn.CreateCommandAndParameters(UmPath(router, "/user/set"),
                            ".id", W(match, ".id"), "comment", req.Comment
                        ).ExecuteNonQuery();
                }
            }
            finally { SafeClose(conn); }
        }

        public void ClearUserProfile(string username, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = PrintUserProfiles(conn, router, "user", username).FirstOrDefault();
                if (match != null)
                    conn.CreateCommandAndParameters(UmPath(router, "/user-profile/remove"), ".id", W(match, ".id"))
                        .ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void RemoveUserProfile(string profileId, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                conn.CreateCommandAndParameters(UmPath(router, "/user-profile/remove"), ".id", profileId)
                    .ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public UserProfileResult GetUserProfile(string username, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var profile = PrintUserProfiles(conn, router, "user", username).FirstOrDefault();
                if (profile == null)
                    throw new NotFoundException($"No active profile found for '{username}'.");

                string profileName = W(profile, "profile");

                IEnumerable<ITikReSentence> limitRows;
                try
                {
                    limitRows = conn.CreateCommandAndParameters(
                        UmPath(router, "/profile-limitation/print"), "profile", profileName).ExecuteList();
                }
                catch (NotImplementedException) { limitRows = Enumerable.Empty<ITikReSentence>(); }

                var limitations = limitRows.Select(s => (object)new
                {
                    id = W(s, ".id"),
                    limitation = W(s, "limitation"),
                    fromTime = W(s, "from-time"),
                    tillTime = W(s, "till-time"),
                    weekdays = W(s, "weekdays")
                }).ToList();

                return new UserProfileResult
                {
                    ProfileId = W(profile, ".id"),
                    ProfileName = profileName,
                    User = W(profile, "user"),
                    Limitations = limitations
                };
            }
            finally { SafeClose(conn); }
        }

        // ── Sessions ─────────────────────────────────────────────────────

        public List<Dictionary<string, string>> GetOnlineSessions(int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                IEnumerable<ITikReSentence> rows;
                try { rows = conn.CreateCommandAndParameters(UmPath(router, "/session/print"), "active", "yes").ExecuteList(); }
                catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }
                return rows.Select(s => s.Words.ToDictionary(k => k.Key, v => v.Value)).ToList();
            }
            finally { SafeClose(conn); }
        }

        public List<Dictionary<string, string>> GetSessionHistory(string username, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                IEnumerable<ITikReSentence> rows;
                try { rows = conn.CreateCommandAndParameters(UmPath(router, "/session/print"), "user", username).ExecuteList(); }
                catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }
                return rows.Select(s => s.Words.ToDictionary(k => k.Key, v => v.Value)).ToList();
            }
            finally { SafeClose(conn); }
        }

        public void CloseSession(string sessionId, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                conn.CreateCommandAndParameters(UmPath(router, "/session/close-session"), ".id", sessionId)
                    .ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        // ── RADIUS profiles ──────────────────────────────────────────────

        public List<object> GetProfiles(int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                IEnumerable<ITikReSentence> rows;
                try { rows = conn.CreateCommandAndParameters(UmPath(router, "/profile/print")).ExecuteList(); }
                catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }

                return rows.Select(s => (object)new
                {
                    id = W(s, ".id"),
                    name = W(s, "name"),
                    nameForUsers = W(s, "name-for-users"),
                    validity = W(s, "validity"),
                    startsWhen = W(s, "starts-when"),
                    price = W(s, "price"),
                    comment = W(s, "comment")
                }).ToList();
            }
            finally { SafeClose(conn); }
        }

        public List<object> GetUserGroup(int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                IEnumerable<ITikReSentence> rows;
                try { rows = conn.CreateCommandAndParameters(UmPath(router, "/user/group/print")).ExecuteList(); }
                catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }

                return rows.Select(s => (object)new
                {
                    id = W(s, ".id"),
                    name = W(s, "name"),
                    comment = W(s, "comment")
                }).ToList();
            }
            finally { SafeClose(conn); }
        }

        public string CreateProfile(CreateProfileRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                try
                {
                    conn.CreateCommandAndParameters(UmPath(router, "/profile/add"),
                        "name", req.ProfileName,
                        "name-for-users", req.ProfileName,
                        "validity", req.Validity,
                        "starts-when", "first-auth",
                        "price", req.Price
                    ).ExecuteNonQuery();
                    return "Profile created successfully.";
                }
                catch (TikCommandTrapException ex) when (ex.Message.ToLower().Contains("already have such name"))
                {
                    return "Profile already exists.";
                }
            }
            finally { SafeClose(conn); }
        }

        public void UpdateProfile(string profileName, UpdateProfileRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                IEnumerable<ITikReSentence> rows;
                try { rows = conn.CreateCommandAndParameters(UmPath(router, "/profile/print"), "name", profileName).ExecuteList(); }
                catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }

                var match = rows.FirstOrDefault();
                if (match == null)
                    throw new NotFoundException($"Profile '{profileName}' not found.");

                conn.CreateCommandAndParameters(UmPath(router, "/profile/set"),
                    ".id", W(match, ".id"), "name", profileName,
                    "validity", req.Validity, "price", req.Price
                ).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void DeleteProfile(string profileId, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                conn.CreateCommandAndParameters(UmPath(router, "/profile/remove"), ".id", profileId).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        // ── Limitations ──────────────────────────────────────────────────

        public List<object> GetLimitations(int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                IEnumerable<ITikReSentence> rows;
                try { rows = conn.CreateCommandAndParameters(UmPath(router, "/limitation/print")).ExecuteList(); }
                catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }

                return rows.Select(s => (object)new
                {
                    id = W(s, ".id"),
                    name = W(s, "name"),
                    rateLimitRx = W(s, "rate-limit-rx"),
                    rateLimitTx = W(s, "rate-limit-tx"),
                    rateLimitMinRx = W(s, "rate-limit-min-rx"),
                    rateLimitMinTx = W(s, "rate-limit-min-tx"),
                    comment = W(s, "comment")
                }).ToList();
            }
            finally { SafeClose(conn); }
        }

        public string CreateLimitation(CreateLimitationRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                try
                {
                    conn.CreateCommandAndParameters(UmPath(router, "/limitation/add"),
                        "name", req.LimitationName,
                        "rate-limit-rx", req.Download + req.Prefix,
                        "rate-limit-tx", req.Upload + req.Prefix,
                        "rate-limit-min-rx", req.Upload + req.Prefix,
                        "rate-limit-min-tx", req.Download + req.Prefix
                    ).ExecuteNonQuery();
                    return "Limitation created successfully.";
                }
                catch (TikCommandTrapException ex) when (ex.Message.ToLower().Contains("already have such name"))
                {
                    return "Limitation already exists.";
                }
            }
            finally { SafeClose(conn); }
        }

        public void UpdateLimitation(string limitationName, UpdateLimitationRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                IEnumerable<ITikReSentence> rows;
                try { rows = conn.CreateCommandAndParameters(UmPath(router, "/limitation/print"), "name", limitationName).ExecuteList(); }
                catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }

                var match = rows.FirstOrDefault();
                if (match == null)
                    throw new NotFoundException($"Limitation '{limitationName}' not found.");

                conn.CreateCommandAndParameters(UmPath(router, "/limitation/set"),
                    ".id", W(match, ".id"), "name", limitationName,
                    "rate-limit-rx", req.RX.ToString(),
                    "rate-limit-tx", req.TX.ToString(),
                    "rate-limit-min-rx", req.MinRX.ToString(),
                    "rate-limit-min-tx", req.MinTX.ToString()
                ).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void DeleteLimitation(string limitationId, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                conn.CreateCommandAndParameters(UmPath(router, "/limitation/remove"), ".id", limitationId).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        // ── Profile-limitations ──────────────────────────────────────────

        public string ApplyProfileLimitation(ProfileLimitationRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                IEnumerable<ITikReSentence> rows;
                try { rows = conn.CreateCommandAndParameters(UmPath(router, "/profile-limitation/print"), "limitation", req.LimitationName).ExecuteList(); }
                catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }

                if (rows.Any())
                    return "Profile limitation already applied.";

                try
                {
                    conn.CreateCommandAndParameters(UmPath(router, "/profile-limitation/add"),
                        "limitation", req.LimitationName,
                        "profile", req.ProfileName,
                        "weekdays", "sunday,monday,tuesday,wednesday,thursday,friday,saturday",
                        "from-time", req.FromTime,
                        "till-time", req.ToTime
                    ).ExecuteNonQuery();
                }
                catch (NotImplementedException) { /* !empty = OK */ }

                return "Profile limitation applied successfully.";
            }
            finally { SafeClose(conn); }
        }

        public void RemoveProfileLimitation(string profileName, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                IEnumerable<ITikReSentence> rows;
                try { rows = conn.CreateCommandAndParameters(UmPath(router, "/profile-limitation/print"), "profile", profileName).ExecuteList(); }
                catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }

                foreach (var s in rows)
                    conn.CreateCommandAndParameters(UmPath(router, "/profile-limitation/remove"), ".id", W(s, ".id"))
                        .ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        public void DeleteProfileLimitation(string limitationId, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                conn.CreateCommandAndParameters(UmPath(router, "/limitation/remove"), ".id", limitationId).ExecuteNonQuery();
            }
            finally { SafeClose(conn); }
        }

        // ── VLANs ────────────────────────────────────────────────────────

        public List<object> GetVlans(int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var rows = conn.CreateCommand("/interface/vlan/print").ExecuteList();
                return rows.Select(s => (object)new
                {
                    id = W(s, ".id"),
                    name = W(s, "name"),
                    vlanId = W(s, "vlan-id"),
                    disabled = W(s, "disabled"),
                    iface = W(s, "interface"),
                    mtu = W(s, "mtu"),
                    comment = W(s, "comment"),
                    //raw = s.Words.ToDictionary(k => k.Key, v => v.Value)
                }).ToList();
            }
            finally { SafeClose(conn); }
        }

        public VlanStatusResult GetVlanStatus(string vlanName, int routerId, string vlanId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = FindVlanByName(conn, vlanName);
                return match == null
                    ? throw new SoftFailureException($"VLAN '{vlanName}' not found in router '{router.RouterName}'.")
                    : new VlanStatusResult
                    {
                        InternalId = W(match, ".id"),
                        Name = W(match, "name"),
                        Disabled = W(match, "disabled"),
                        VlanId = W(match, "vlan-id"),
                        Iface = W(match, "interface"),
                        Comment = W(match, "comment")
                    };
            }
            finally { SafeClose(conn); }
        }

        public List<object> GetVlanAddress(string vlanName, int routerId, string vlanId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = FindVlanByName(conn, vlanName);
                var resolvedName = match != null ? W(match, "name") : vlanName;

                IEnumerable<ITikReSentence> rows;
                try { rows = conn.CreateCommandAndParameters("/ip/address/print", "interface", resolvedName).ExecuteList(); }
                catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }

                return rows.Select(s => (object)new
                {
                    address = W(s, "address"),
                    network = W(s, "network")
                }).ToList();
            }
            finally { SafeClose(conn); }
        }

        public List<object> GetVlanLogs(string vlanName, int routerId, string vlanId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = FindVlanByName(conn, vlanName);
                var resolvedName = match != null ? W(match, "name") : vlanName;

                IEnumerable<ITikReSentence> rows;
                try { rows = conn.CreateCommand("/log/print").ExecuteList(); }
                catch (NotImplementedException) { rows = Enumerable.Empty<ITikReSentence>(); }

                return rows
                    .Where(x => W(x, "message").IndexOf(resolvedName, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(x => (object)new
                    {
                        internalId = W(x, ".id"),
                        time = W(x, "time"),
                        topics = W(x, "topics"),
                        message = W(x, "message")
                    })
                    .OrderByDescending(x => ((dynamic)x).time)
                    .ToList();
            }
            finally { SafeClose(conn); }
        }

        public string EnableVlan(string vlanName, VlanActionRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = FindVlanByName(conn, vlanName);
                if (match == null)
                    throw new SoftFailureException($"VLAN '{vlanName}' not found.");

                conn.CreateCommandAndParameters("/interface/vlan/set",
                    ".id", W(match, ".id"), "disabled", "no", "comment", req.Comment
                ).ExecuteNonQuery();

                return $"VLAN '{W(match, "name")}' enabled successfully.";
            }
            finally { SafeClose(conn); }
        }

        public string DisableVlan(string vlanName, VlanActionRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = FindVlanByName(conn, vlanName);
                if (match == null)
                    throw new SoftFailureException($"VLAN '{vlanName}' not found.");

                conn.CreateCommandAndParameters("/interface/vlan/set",
                    ".id", W(match, ".id"), "disabled", "yes", "comment", req.Comment
                ).ExecuteNonQuery();

                return $"VLAN '{W(match, "name")}' disabled successfully.";
            }
            finally { SafeClose(conn); }
        }

        public string SetVlanComment(string vlanName, VlanActionRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = FindVlanByName(conn, vlanName);
                if (match == null)
                    throw new SoftFailureException($"VLAN '{vlanName}' not found.");

                conn.CreateCommandAndParameters("/interface/vlan/set",
                    ".id", W(match, ".id"), "comment", req.Comment
                ).ExecuteNonQuery();

                return $"VLAN '{W(match, "name")}' comment updated successfully.";
            }
            finally { SafeClose(conn); }
        }

        public string RenameVlan(string vlanName, VlanRenameRequest req)
        {
            var router = GetRouterInfo(req.RouterID);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = FindVlanByName(conn, vlanName);
                if (match == null)
                    throw new SoftFailureException($"VLAN '{vlanName}' not found.");

                conn.CreateCommandAndParameters("/interface/vlan/set",
                    ".id", W(match, ".id"), "name", req.NewName
                ).ExecuteNonQuery();

                return $"VLAN renamed to '{req.NewName}' successfully.";
            }
            finally { SafeClose(conn); }
        }

        private static bool IsTrue(string v)
            => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "yes", StringComparison.OrdinalIgnoreCase);

        public RouterState GetUserState(string username, int routerId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = PrintUsers(conn, router, username).FirstOrDefault();
                if (match == null) return RouterState.NotFound;
                return IsTrue(W(match, "disabled")) ? RouterState.Disabled : RouterState.Enabled;
            }
            finally { SafeClose(conn); }
        }

        public RouterState GetVlanState(string vlanName, int routerId, string vlanId)
        {
            var router = GetRouterInfo(routerId);
            ITikConnection conn = null;
            try
            {
                conn = OpenConnection(router);
                var match = FindVlanByName(conn, vlanName);
                if (match == null) return RouterState.NotFound;
                return IsTrue(W(match, "disabled")) ? RouterState.Disabled : RouterState.Enabled;
            }
            finally { SafeClose(conn); }
        }
    }
}