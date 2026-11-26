using System;
using System.Collections.Generic;
using System.Web.Http;
using tik4net;

namespace NSMSOLUTION.Controllers.API
{
    [RoutePrefix("api/mikrotik")]
    public class MikrotikController : ApiController
    {
        // ─────────────────────────────────────────
        // ROUTER CONFIG — change these values
        // ─────────────────────────────────────────
        private string RouterIp => "103.134.176.2";
        private string RouterUser => "admin";
        private string RouterPass => "NSM@admin";

        // ─────────────────────────────────────────
        // GET: api/mikrotik/users/all
        // ─────────────────────────────────────────
        [HttpGet]
        [Route("users/all")]
        public IHttpActionResult GetAllUsers()
        {
            var users = new List<object>();
            try
            {
                using (var conn = OpenConnection(out int version))
                {
                    var rows = version == 1
                        ? conn.CreateCommand("/tool/user-manager/user/print").ExecuteList()
                        : conn.CreateCommand("/user-manager/user/print").ExecuteList();

                    foreach (var row in rows)
                        users.Add(MapUser(row, version, "All"));
                }
                return Ok(new { success = true, total = users.Count, data = users });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ─────────────────────────────────────────
        // GET: api/mikrotik/users/active
        // ─────────────────────────────────────────
        [HttpGet]
        [Route("users/active")]
        public IHttpActionResult GetActiveUsers()
        {
            var users = new List<object>();
            try
            {
                using (var conn = OpenConnection(out int version))
                {
                    // ✅ Correct path for active PPPoE sessions (both v6 and v7)
                    var rows = conn.CreateCommand("/ppp/active/print").ExecuteList();

                    foreach (var row in rows)
                    {
                        users.Add(new
                        {
                            Id = GetWord(row, ".id"),
                            Username = GetWord(row, "name"),
                            Service = GetWord(row, "service"),       // pppoe / pptp / l2tp
                            CallerIp = GetWord(row, "caller-id"),
                            Address = GetWord(row, "address"),
                            Uptime = GetWord(row, "uptime"),
                            Encoding = GetWord(row, "encoding"),
                            SessionId = GetWord(row, "session-id"),
                            Radius = GetWord(row, "radius"),
                            Status = "Active"
                        });
                    }
                }
                return Ok(new { success = true, total = users.Count, data = users });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ─────────────────────────────────────────
        // GET: api/mikrotik/users/locked
        // ─────────────────────────────────────────
        [HttpGet]
        [Route("users/locked")]
        public IHttpActionResult GetLockedUsers()
        {
            var users = new List<object>();
            try
            {
                using (var conn = OpenConnection(out int version))
                {
                    var rows = version == 1
                        ? conn.CreateCommand("/tool/user-manager/user/print").ExecuteList()
                        : conn.CreateCommand("/user-manager/user/print").ExecuteList();

                    foreach (var row in rows)
                    {
                        if (GetWord(row, "disabled") == "true")
                            users.Add(MapUser(row, version, "Locked"));
                    }
                }
                return Ok(new { success = true, total = users.Count, data = users });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ─────────────────────────────────────────
        // GET: api/mikrotik/users/terminated
        // ─────────────────────────────────────────
        [HttpGet]
        [Route("users/terminated")]
        public IHttpActionResult GetTerminatedUsers()
        {
            var users = new List<object>();
            try
            {
                using (var conn = OpenConnection(out int version))
                {
                    var rows = version == 1
                        ? conn.CreateCommand("/tool/user-manager/user/print").ExecuteList()
                        : conn.CreateCommand("/user-manager/user/print").ExecuteList();

                    foreach (var row in rows)
                    {
                        string expireDate = GetWord(row, "expire-date");
                        if (!string.IsNullOrEmpty(expireDate) &&
                            DateTime.TryParse(expireDate, out DateTime expDt) &&
                            expDt < DateTime.Now)
                        {
                            users.Add(MapUser(row, version, "Terminated"));
                        }
                    }
                }
                return Ok(new { success = true, total = users.Count, data = users });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ─────────────────────────────────────────
        // GET: api/mikrotik/users/search?username=john
        // ─────────────────────────────────────────
        [HttpGet]
        [Route("users/search")]
        public IHttpActionResult SearchUser(string username)
        {
            var users = new List<object>();
            try
            {
                if (string.IsNullOrEmpty(username))
                    return BadRequest("username parameter is required.");

                using (var conn = OpenConnection(out int version))
                {
                    IEnumerable<ITikReSentence> rows = version == 1
                        ? conn.CreateCommandAndParameters("/tool/user-manager/user/print", "username", username).ExecuteList()
                        : conn.CreateCommandAndParameters("/user-manager/user/print", "name", username).ExecuteList();
                    foreach (var row in rows)
                    {
                        string status = GetWord(row, "disabled") == "false" ? "Active" : "Disabled";
                        users.Add(MapUser(row, version, status));
                    }
                }

                if (users.Count == 0)
                    return Ok(new { success = false, message = $"User '{username}' not found." });

                return Ok(new { success = true, total = users.Count, data = users });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ─────────────────────────────────────────
        // GET: api/mikrotik/version
        // ─────────────────────────────────────────
        [HttpGet]
        [Route("version")]
        public IHttpActionResult GetVersion()
        {
            try
            {
                using (var conn = OpenConnection(out int version))
                {
                    var rows = conn.CreateCommand("/system/resource/print").ExecuteList();
                    string rosVersion = "";
                    foreach (var row in rows)
                        rosVersion = GetWord(row, "version");

                    return Ok(new
                    {
                        success = true,
                        routerOs = rosVersion,
                        apiVersion = version,
                        apiPath = version == 1 ? "/tool/user-manager" : "/user-manager"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ─────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────

        /// <summary>
        /// Opens connection and auto-detects RouterOS version.
        /// version = 1 → RouterOS v6 (/tool/user-manager)
        /// version = 2 → RouterOS v7 (/user-manager)
        /// </summary>
        private ITikConnection OpenConnection(out int version)
        {
            var conn = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            conn.Open(RouterIp, RouterUser, RouterPass);
            version = DetectVersion(conn);
            return conn;
        }

        private int DetectVersion(ITikConnection conn)
        {
            try
            {
                conn.CreateCommand("/user-manager/user/print").ExecuteList();
                return 2; // RouterOS v7+
            }
            catch
            {
                return 1; // RouterOS v6
            }
        }

        private object MapUser(ITikReSentence row, int version, string status = "Unknown")
        {
            return new
            {
                Id = GetWord(row, ".id"),
                Username = version == 1 ? GetWord(row, "username") : GetWord(row, "name"),
                Password = GetWord(row, "password"),
                Profile = GetWord(row, "profile"),
                Disabled = GetWord(row, "disabled"),
                ExpireDate = GetWord(row, "expire-date"),
                Comment = GetWord(row, "comment"),
                Status = status
            };
        }

        private string GetWord(ITikReSentence row, string key)
        {
            try { return row.Words.ContainsKey(key) ? row.Words[key] : ""; }
            catch { return ""; }
        }
    }
}