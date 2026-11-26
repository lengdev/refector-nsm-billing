using NSMSOLUTION.Services;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using tik4net;

namespace Api.Controllers
{
    [RoutePrefix("api/mikrotik")]
    public class APIMikrotikController : ApiController
    {
        private readonly MikrotikService _service = new MikrotikService();

        // Central exception → HTTP mapping so every action doesn't repeat it.
        private IHttpActionResult HandleError(Exception ex)
        {
            switch (ex)
            {
                case SoftFailureException sf:
                    return Ok(new { success = false, message = sf.Message });
                case NotFoundException nf:
                    return Content(HttpStatusCode.NotFound, new { success = false, message = nf.Message });
                case ConflictException cf:
                    return Content(HttpStatusCode.Conflict, new { success = false, message = cf.Message });
                case System.Net.Sockets.SocketException se:
                    return Content(HttpStatusCode.BadGateway, new { success = false, message = "Cannot reach router: " + se.Message });
                case TikConnectionLoginException le:
                    return Content(HttpStatusCode.Unauthorized, new { success = false, message = "Login failed: " + le.Message });
                case TikCommandTrapException te:
                    return Content(HttpStatusCode.BadRequest, new { success = false, message = te.Message });
                default:
                    return InternalServerError(ex);
            }
        }

        [HttpPost, Route("connect")]
        public IHttpActionResult TestConnection([FromBody] RouterRef req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                _service.TestConnection(req.RouterID);
                return Ok(new { success = true, message = "Connection successful." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("pppoe/users")]
        public IHttpActionResult ListUsers([FromUri] int routerId)
        {
            try
            {
                var list = _service.GetPPPoEUsers(routerId);
                return Ok(new { success = true, count = list.Count, data = list });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("pppoe/users/exists")]
        public IHttpActionResult CheckUserExists([FromUri] string username, [FromUri] int routerId)
        {
            try
            {
                var result = _service.CheckUserExists(routerId, username);
                return Ok(new { success = true, exists = result.Exists, id = result.Id });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("pppoe/users/status")]
        public IHttpActionResult GetPPPoEUserStatus([FromUri] string username, [FromUri] int routerId)
        {
            try
            {
                var r = _service.GetPPPoEUserStatus(routerId, username);
                return Ok(new
                {
                    success = true,
                    id = r.Id,
                    name = r.Name,
                    disabled = r.Disabled,
                    actualProfile = r.ActualProfile,
                    lastSeen = r.LastSeen,
                    comment = r.Comment
                });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPost, Route("pppoe/users")]
        public IHttpActionResult CreateOrUpdateUser([FromBody] CreateUserRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                var message = _service.CreateOrUpdateUser(req);
                return Ok(new { success = true, message });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPut, Route("pppoe/users")]
        public IHttpActionResult UpdateUser([FromBody] UpdateUserRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                _service.UpdateUser(req);
                return Ok(new { success = true, message = "User updated successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpDelete, Route("pppoe/users")]
        public IHttpActionResult RemoveUser([FromUri] string username, [FromUri] int routerId)
        {
            try
            {
                _service.RemoveUser(username, routerId);
                return Ok(new { success = true, message = "User removed successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpDelete, Route("pppoe/users/by-id/{userId}")]
        public IHttpActionResult RemoveUserById(string userId, [FromUri] int routerId)
        {
            try
            {
                _service.RemoveUserById(userId, routerId);
                return Ok(new { success = true, message = "User removed successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPost, Route("pppoe/users/{userId}/enable")]
        public IHttpActionResult EnableUser(string userId, [FromBody] UserActionRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                _service.EnableUser(userId, req);
                return Ok(new { success = true, message = "User enabled successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPost, Route("pppoe/users/{userId}/disable")]
        public IHttpActionResult DisableUser(string userId, [FromBody] UserActionRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                _service.DisableUser(userId, req);
                return Ok(new { success = true, message = "User disabled successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPost, Route("pppoe/users/lock")]
        public IHttpActionResult LockUser([FromUri] string username, [FromBody] UserActionRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                _service.LockUser(username, req);
                return Ok(new { success = true, message = "User locked successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPost, Route("pppoe/users/reactivate")]
        public IHttpActionResult ReactivateUser([FromUri] string username, [FromBody] UserActionRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                _service.ReactivateUser(username, req);
                return Ok(new { success = true, message = "User reactivated successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPatch, Route("pppoe/users/comment")]
        public IHttpActionResult SetUserComment([FromUri] string username, [FromBody] UserActionRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                _service.SetUserComment(username, req);
                return Ok(new { success = true, message = "Comment updated successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPost, Route("pppoe/users/profiles/append")]
        public IHttpActionResult AppendProfile([FromBody] AppendProfileRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                _service.AppendProfile(req);
                return Ok(new { success = true, message = "Profile appended to user successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpDelete, Route("pppoe/users/profiles")]
        public IHttpActionResult ClearUserProfile([FromUri] string username, [FromUri] int routerId)
        {
            try
            {
                _service.ClearUserProfile(username, routerId);
                return Ok(new { success = true, message = "Profile cleared successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpDelete, Route("pppoe/users/profiles/{profileId}")]
        public IHttpActionResult RemoveUserProfile(string profileId, [FromUri] int routerId)
        {
            try
            {
                _service.RemoveUserProfile(profileId, routerId);
                return Ok(new { success = true, message = "User profile removed successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("pppoe/users/profiles/current")]
        public IHttpActionResult GetUserProfile([FromUri] string username, [FromUri] int routerId)
        {
            try
            {
                var r = _service.GetUserProfile(username, routerId);
                return Ok(new
                {
                    success = true,
                    profileId = r.ProfileId,
                    profileName = r.ProfileName,
                    user = r.User,
                    limitations = r.Limitations
                });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("sessions/online")]
        public IHttpActionResult GetOnlineSessions([FromUri] int routerId)
        {
            try
            {
                var list = _service.GetOnlineSessions(routerId);
                return Ok(new { success = true, count = list.Count, data = list });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("sessions/history")]
        public IHttpActionResult GetSessionHistory([FromUri] string username, [FromUri] int routerId)
        {
            try
            {
                var list = _service.GetSessionHistory(username, routerId);

                if (list == null || list.Count == 0)
                    return Ok(new { success = true, data = (object)null });

                var last = list.FirstOrDefault(s => s.ContainsKey("active") && s["active"] == "true")
                           ?? list.OrderByDescending(s => DateTime.Parse(s["started"])).First();

                return Ok(new { success = true, data = last });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpDelete, Route("sessions/{sessionId}")]
        public IHttpActionResult CloseSession(string sessionId, [FromUri] int routerId)
        {
            try
            {
                _service.CloseSession(sessionId, routerId);
                return Ok(new { success = true, message = "Session closed successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("profiles")]
        public IHttpActionResult GetProfiles([FromUri] int routerId)
        {
            try
            {
                var list = _service.GetProfiles(routerId);
                return Ok(new { success = true, data = list });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPost, Route("profiles")]
        public IHttpActionResult CreateProfile([FromBody] CreateProfileRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                var message = _service.CreateProfile(req);
                return Ok(new { success = true, message });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPut, Route("profiles/{profileName}")]
        public IHttpActionResult UpdateProfile(string profileName, [FromBody] UpdateProfileRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                _service.UpdateProfile(profileName, req);
                return Ok(new { success = true, message = "Profile updated successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpDelete, Route("profiles/{profileId}")]
        public IHttpActionResult DeleteProfile(string profileId, [FromUri] int routerId)
        {
            try
            {
                _service.DeleteProfile(profileId, routerId);
                return Ok(new { success = true, message = "Profile deleted successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("limitations")]
        public IHttpActionResult GetLimitations([FromUri] int routerId)
        {
            try
            {
                var list = _service.GetLimitations(routerId);
                return Ok(new { success = true, data = list });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPost, Route("limitations")]
        public IHttpActionResult CreateLimitation([FromBody] CreateLimitationRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                var message = _service.CreateLimitation(req);
                return Ok(new { success = true, message });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPut, Route("limitations/{limitationName}")]
        public IHttpActionResult UpdateLimitation(string limitationName, [FromBody] UpdateLimitationRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                _service.UpdateLimitation(limitationName, req);
                return Ok(new { success = true, message = "Limitation updated successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpDelete, Route("limitations/{limitationId}")]
        public IHttpActionResult DeleteLimitation(string limitationId, [FromUri] int routerId)
        {
            try
            {
                _service.DeleteLimitation(limitationId, routerId);
                return Ok(new { success = true, message = "Limitation deleted successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPost, Route("profile-limitations")]
        public IHttpActionResult ApplyProfileLimitation([FromBody] ProfileLimitationRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                var message = _service.ApplyProfileLimitation(req);
                return Ok(new { success = true, message });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpDelete, Route("profile-limitations/{profileName}")]
        public IHttpActionResult RemoveProfileLimitation(string profileName, [FromUri] int routerId)
        {
            try
            {
                _service.RemoveProfileLimitation(profileName, routerId);
                return Ok(new { success = true, message = "Profile limitation(s) removed successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpDelete, Route("profile-limitations/by-id/{limitationId}")]
        public IHttpActionResult DeleteProfileLimitation(string limitationId, [FromUri] int routerId)
        {
            try
            {
                _service.DeleteProfileLimitation(limitationId, routerId);
                return Ok(new { success = true, message = "Profile limitation deleted successfully." });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("vlans")]
        public IHttpActionResult GetVlans([FromUri] int routerId)
        {
            try
            {
                var list = _service.GetVlans(routerId);
                return Ok(new { success = true, count = list.Count, data = list });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("vlans/status")]
        public IHttpActionResult GetVlanStatus([FromUri] string vlanName, [FromUri] int routerId, [FromUri] string vlanId = null)
        {
            try
            {
                var r = _service.GetVlanStatus(vlanName, routerId, vlanId);
                return Ok(new
                {
                    success = true,
                    internalId = r.InternalId,
                    name = r.Name,
                    disabled = r.Disabled,
                    vlanId = r.VlanId,
                    iface = r.Iface,
                    comment = r.Comment
                });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("vlans/address")]
        public IHttpActionResult GetVlanAddress([FromUri] string vlanName, [FromUri] int routerId, [FromUri] string vlanId = null)
        {
            try
            {
                var list = _service.GetVlanAddress(vlanName, routerId, vlanId);
                return Ok(new { success = true, data = list });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpGet, Route("vlans/logs")]
        public IHttpActionResult GetVlanLogs([FromUri] string vlanName, [FromUri] int routerId, [FromUri] string vlanId = null)
        {
            try
            {
                var logs = _service.GetVlanLogs(vlanName, routerId, vlanId);
                return Ok(new { success = true, count = logs.Count, data = logs });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPost, Route("vlans/enable")]
        public IHttpActionResult EnableVlan([FromUri] string vlanName, [FromBody] VlanActionRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                var message = _service.EnableVlan(vlanName, req);
                return Ok(new { success = true, message });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPost, Route("vlans/disable")]
        public IHttpActionResult DisableVlan([FromUri] string vlanName, [FromBody] VlanActionRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                var message = _service.DisableVlan(vlanName, req);
                return Ok(new { success = true, message });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPatch, Route("vlans/comment")]
        public IHttpActionResult SetVlanComment([FromUri] string vlanName, [FromBody] VlanActionRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            try
            {
                var message = _service.SetVlanComment(vlanName, req);
                return Ok(new { success = true, message });
            }
            catch (Exception ex) { return HandleError(ex); }
        }

        [HttpPatch, Route("vlans/rename")]
        public IHttpActionResult RenameVlan([FromUri] string vlanName, [FromBody] VlanRenameRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.NewName)) return BadRequest("NewName is required.");
            try
            {
                var message = _service.RenameVlan(vlanName, req);
                return Ok(new { success = true, message });
            }
            catch (Exception ex) { return HandleError(ex); }
        }
    }
}