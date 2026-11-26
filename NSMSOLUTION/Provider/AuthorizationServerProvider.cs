using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using MVC;
using Newtonsoft.Json;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NSMSOLUTION.Provider
{
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        //private AuthContext ctx;
        private string DomainID = "";

        AuthRepository authRepository = new AuthRepository();
        IdentityUser user = new IdentityUser();
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {

            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            var userAgent = HttpContext.Current.Request.Headers["User-Agent"];
            var WebBrowserName = HttpContext.Current.Request.Browser.Browser;
            try
            {
                user = await authRepository.FindUser(context.UserName, context.Password, userAgent, WebBrowserName);
                if (user.UserName != null)
                {
                    HTML html = new HTML();
                    IdentityUserRole identityUserRole = new IdentityUserRole
                    {
                        RoleId = user.Roles.FirstOrDefault().RoleId
                    };
                    string SQL = "";
                    string sql = @"select a.Code, u.UserLoginID, u.UserName from dbo.TblAccessDomain a
CROSS APPLY STRING_SPLIT(isnull(a.DomainID,''), ',')
inner join (select value DomainID, u.UserLoginID, u.UserName from dbo.TblUserLogin u cross apply string_split(u.DomainID, ',')) u on u.DomainID = a.Code
where value = (select DomainID from dbo.TblDomain where DomainName ='" + html.HostName + @"')
and u.UserLoginID=" + user.Id;
                    DataSet ds2 = html.LoadData(sql);
                    if (ds2.Tables[0].Rows.Count == 0)
                    {
                        string message = "You cannot login this domain name: " + html.HostName + ". Please contact administrator.";
                        context.SetCustomError("invalid_grant", message);
                        context.Rejected();
                        return;
                    }
                    sql = @"declare @DomainID nvarchar(50)
select @DomainID = (STUFF((SELECT ',' + a.Code from dbo.TblAccessDomain a
CROSS APPLY STRING_SPLIT(isnull(a.DomainID,''), ',')
where value = (select DomainID from dbo.TblDomain where DomainName ='" + html.HostName + @"')
ORDER BY a.Code FOR XML PATH('')), 1, 1, ''));
select @DomainID DomainID";
                    ds2 = html.LoadData(sql);
                    if (ds2.Tables.Count > 0)
                    {
                        if (ds2.Tables[0].Rows.Count > 0)
                            DomainID = ds2.Tables[0].Rows[0][0].ToString();
                    }

                    SQL = @"select distinct tu.UserName, tp.PermissionPageCode, tp.PermissionPageName, tu.UserTypeID, 
tu.UserLoginID, tsw.SystemID, tsw.WarehouseID , tp.ParentPageCode
, tu.ISChangePasswordOnNextLogon, tsw.DefaultDashboardPage, tu.DomainID, tu.FullName
from dbo.TblSetUserAccessSystemAndWarehouse tsw
inner join dbo.TblUserRolePagePermission up on up.UserRoleID = tsw.UserRoleID 
inner join dbo.TblUserLogin tu on tu.UserLoginID = tsw.UserLoginID
inner join dbo.TblPagePermission tp on tp.PermissionPageID = up.PermissionPageID
inner join dbo.TblSetPagePermission sp on sp.PermissionPageID = up.PermissionPageID and tsw.UserLoginID = sp.UserLoginID
where tu.UserLoginID = " + user.Id + " order by tp.ParentPageCode";


                    DataSet ds = html.LoadData(SQL);
                    string FullName = ds.Tables[0].Rows[0]["FullName"].ToString();
                    ClaimsIdentity identity = new ClaimsIdentity(context.Options.AuthenticationType)
                    {
                        Label = "NSM BILLING ISP"
                    };
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        user.Roles.Add(new IdentityUserRole
                        {
                            RoleId = ds.Tables[0].Rows[0]["UserTypeID"].ToString(),
                            UserId = ds.Tables[0].Rows[0]["UserLoginID"].ToString()
                        });
                        identity.AddClaim(new Claim("LoggedOn", DateTime.Now.ToString()));
                        identity.AddClaim(new Claim("SystemID", ds.Tables[0].Rows[0]["SystemID"].ToString()));
                        identity.AddClaim(new Claim("WarehouseID", ds.Tables[0].Rows[0]["WarehouseID"].ToString()));
                        identity.AddClaim(new Claim("DomainID", DomainID));
                        // Use the database UserLoginID as the stable identity key.
                        // ASP.NET MVC anti-forgery validation requires NameIdentifier
                        // unless AntiForgeryConfig is configured to use another claim.
                        string userId = ds.Tables[0].Rows[0]["UserLoginID"].ToString();
                        string userName = ds.Tables[0].Rows[0]["UserName"].ToString();

                        identity.AddClaim(new Claim("UserID", userId));
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
                        identity.AddClaim(new Claim(ClaimTypes.Sid, userId));
                        identity.AddClaim(new Claim(ClaimTypes.Name, userName));
                        identity.AddClaim(new Claim(ClaimTypes.System, ds.Tables[0].Rows[0]["UserTypeID"].ToString()));
                        user.Claims.Add(new IdentityUserClaim()
                        {
                            ClaimType = "UserName",
                            ClaimValue = ds.Tables[0].Rows[0]["UserName"].ToString(),
                            UserId = ds.Tables[0].Rows[0]["UserLoginID"].ToString()
                        });

                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            user.Claims.Add(new IdentityUserClaim()
                            {
                                ClaimType = dr["PermissionPageCode"].ToString(),
                                ClaimValue = dr["PermissionPageName"].ToString(),
                                UserId = dr["UserLoginID"].ToString()
                            });
                            identity.AddClaim(new Claim(ClaimTypes.Role, dr["PermissionPageCode"].ToString()));
                            identity.AddClaim(new Claim(ClaimTypes.Role, dr["PermissionPageName"].ToString()));
                        }
                    }
                    AuthenticationTicket ticket = new AuthenticationTicket(identity, null);

                    context.Validated(ticket);
                    CookieStore.SetCookie("UserID_" + html.HostName, user.Id, TimeSpan.FromDays(30));
                    CookieStore.SetCookie("UserName_" + html.HostName, user.UserName, TimeSpan.FromDays(30));
                    CookieStore.SetCookie("UserFullName_" + html.HostName, FullName, TimeSpan.FromDays(30));
                    return;
                }
                else
                {
                    string message = "Invalid User Name or Password.";
                    if (user.LockoutEnabled)
                    {
                        int m = DateTime.Parse(user.LockoutEndDateUtc.ToString()).Minute;
                        int now = easternTimeNow.Minute;
                        m = m - now;
                        if (m < 1)
                        {
                            m = 1;
                        }
                        message = "User Locked " + m.ToString() + " Minutes due to invalid password many times.";

                    }
                    context.SetCustomError("invalid_grant", message);
                    context.Rejected();
                    return;
                }
            }
            catch
            {
                // Could not retrieve the user due to error.
                context.SetError("server_error");
                context.Rejected();
                return;
            }
        }
        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            string sid = context.Ticket.Identity.Claims
                .Where(c => c.Type == ClaimTypes.Sid)
                .Select(c => c.Value)
                .SingleOrDefault();

            if (string.IsNullOrEmpty(sid))
            {
                context.SetError("invalid_grant", "Refresh token is not associated with a valid user.");
                context.Rejected();
                return Task.FromResult<object>(null);
            }

            HTML html = new HTML();
            string sql = @"select distinct tu.UserName, tp.PermissionPageCode, tp.PermissionPageName, tu.UserTypeID, 
tu.UserLoginID, tsw.SystemID, tsw.WarehouseID, tp.ParentPageCode
, tu.ISChangePasswordOnNextLogon, tsw.DefaultDashboardPage, tu.DomainID, tu.FullName
from dbo.TblSetUserAccessSystemAndWarehouse tsw
inner join dbo.TblUserRolePagePermission up on up.UserRoleID = tsw.UserRoleID 
inner join dbo.TblUserLogin tu on tu.UserLoginID = tsw.UserLoginID
inner join dbo.TblPagePermission tp on tp.PermissionPageID = up.PermissionPageID
inner join dbo.TblSetPagePermission sp on sp.PermissionPageID = up.PermissionPageID and tsw.UserLoginID = sp.UserLoginID
where tu.UserLoginID = " + sid + " order by tp.ParentPageCode";

            DataSet ds = html.LoadData(sql);

            if (ds.Tables[0].Rows.Count == 0)
            {
                context.SetError("invalid_grant", "User no longer has access.");
                context.Rejected();
                return Task.FromResult<object>(null);
            }

            string sqlDomain = @"declare @DomainID nvarchar(50)
select @DomainID = (STUFF((SELECT ',' + a.Code from dbo.TblAccessDomain a
CROSS APPLY STRING_SPLIT(isnull(a.DomainID,''), ',')
where value = (select DomainID from dbo.TblDomain where DomainName ='" + html.HostName + @"')
ORDER BY a.Code FOR XML PATH('')), 1, 1, ''));
select @DomainID DomainID";
            DataSet dsDomain = html.LoadData(sqlDomain);
            if (dsDomain.Tables.Count > 0 && dsDomain.Tables[0].Rows.Count > 0)
            {
                DomainID = dsDomain.Tables[0].Rows[0][0].ToString();
            }

            string userName = ds.Tables[0].Rows[0]["UserName"].ToString();
            string fullName = ds.Tables[0].Rows[0]["FullName"].ToString();
            string systemId = ds.Tables[0].Rows[0]["SystemID"].ToString();
            string warehouseId = ds.Tables[0].Rows[0]["WarehouseID"].ToString();

            var newIdentity = new ClaimsIdentity(context.Options.AuthenticationType)
            {
                Label = "NSM BILLING ISP"
            };
            newIdentity.AddClaim(new Claim("LoggedOn", DateTime.Now.ToString()));
            newIdentity.AddClaim(new Claim("SystemID", systemId));
            newIdentity.AddClaim(new Claim("WarehouseID", warehouseId));
            newIdentity.AddClaim(new Claim("DomainID", DomainID));
            // Keep the same stable identity claims when issuing a refreshed token.
            string userId = ds.Tables[0].Rows[0]["UserLoginID"].ToString();

            newIdentity.AddClaim(new Claim("UserID", userId));
            newIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            newIdentity.AddClaim(new Claim(ClaimTypes.Sid, userId));
            newIdentity.AddClaim(new Claim(ClaimTypes.Name, userName));
            newIdentity.AddClaim(new Claim(ClaimTypes.System, ds.Tables[0].Rows[0]["UserTypeID"].ToString()));

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                newIdentity.AddClaim(new Claim(ClaimTypes.Role, dr["PermissionPageCode"].ToString()));
                newIdentity.AddClaim(new Claim(ClaimTypes.Role, dr["PermissionPageName"].ToString()));
            }

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            // Fire-and-forget Telegram notification — don't let it block or fail the actual refresh
            try
            {
                var ipAddress = context.OwinContext?.Request?.RemoteIpAddress ?? "unknown";
                string accessTokenFingerprint = ShortHash(newTicket.GetHashCode().ToString() + sid + DateTime.UtcNow.Ticks);

                string message = $"🔄 Token Refreshed\n" +
                                  $"User: {fullName} ({userName})\n" +
                                  $"UserID: {sid}\n" +
                                  $"System/Warehouse: {systemId}/{warehouseId}\n" +
                                  $"Domain: {DomainID}\n" +
                                  $"IP: {ipAddress}\n" +
                                  $"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                var notification = new NotificationTelegram();
                notification.NotificationTelegramBot(message, -4510188155);
            }
            catch { }

            return Task.FromResult<object>(null);
        }
        private static string ShortHash(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(bytes).Substring(0, 12);
            }
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML();
            string sid = context.Identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            string sql = @"select ts.SystemID, ts.WarehouseID, s.SystemName, w.WarehouseName, ts.DefaultDashboardPage, 
s.Logo SystemLogo, w.Logo WarehouseLogo, s.AsideLightSidebar, s.Body, u.DomainID, u.Job Position, c.CompanyName, c.CompanyNameKH, c.CompanyAddress, c.Phone, c.Email, c.Website
from dbo.TblSetUserAccessSystemAndWarehouse ts
inner join dbo.TblSystem s on s.SystemID = ts.SystemID
inner join dbo.TblWarehouseSystem ws on ws.WarehouseID = ts.WarehouseID and ws.SystemID = s.SystemID
inner join dbo.TblWarehouse w on w.WarehouseID = ws.WarehouseID
inner join dbo.TblUserLogin u on u.UserLoginID = ts.UserLoginID
left join dbo.TblCompany c on c.SystemID = ts.SystemID and c.ISDefault = 1    
where ts.UserLoginID = @UserLoginID order by s.SystemName, w.OrderNumber, w.WarehouseName;";

            var paras = new Dictionary<string, string>
            {
                ["@UserLoginID"] = sid,
                ["@SystemID"] = html.SystemID
            };

            string UserRolesID = context.Identity.Claims.Where(c => c.Type == ClaimTypes.System).Select(c => c.Value).SingleOrDefault();
            DataSet ds = html.LoadData(sql, false, paras);
            List<SysAndWH> SysAndWH = new List<SysAndWH>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                SysAndWH.Add(new SysAndWH
                {
                    UserID = sid,
                    SystemID = dr["SystemID"].ToString(),
                    WarehouseID = dr["WarehouseID"].ToString(),
                    SystemName = dr["SystemName"].ToString(),
                    WarehouseName = dr["WarehouseName"].ToString(),
                    DefaultDashboardPage = dr["DefaultDashboardPage"].ToString(),
                    SystemLogo = dr["SystemLogo"].ToString(),
                    WarehouseLogo = dr["WarehouseLogo"].ToString(),
                    Body = dr["Body"].ToString(),
                    AsideLightSidebar = dr["AsideLightSidebar"].ToString(),
                    DomainID = DomainID,
                    Position = dr["Position"].ToString(),
                    Company = new CompanyDto
                    {
                        CompanyName = dr["CompanyName"].ToString(),
                        CompanyNameKH = dr["CompanyNameKH"].ToString(),
                        CompanyAddress = dr["CompanyAddress"].ToString(),
                        CompanyPhone = dr["Phone"].ToString(),
                        CompanyEmail = dr["Email"].ToString(),
                        CompanyWebsite = dr["Website"].ToString()
                    }
                });
            }
            sql = @"select distinct tu.FullName, tp.OrderNumber, tp.NAVICON, tp.PermissionPageCode, 
tp.PageLink, tp.MenuID, tp.ParentPageCode, tp.PageTypeID, pt.PageType, sw.SystemID, 
sw.WarehouseID, isnull(tu.ISChangePasswordOnNextLogon, 0) ISChangePasswordOnNextLogon,
sw.DefaultDashboardPage, sw.UserRoleID, ur.RoleName, tu.ImageLink,
case when isnull(tu.UserPicture, '') = '' then 'Content/images/avatar/user.png' else '" + html.ServerRoot + "Uploads/UserProfilePicture/'+tu.FullName+'" + @"/'+ tu.UserPicture end UserPicture, tu.DomainID
, tu.Job
from TblUserLogin tu
inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = tu.UserLoginID
inner join dbo.TblUserRolePagePermission up on up.UserRoleID = sw.UserRoleID
inner join dbo.TblUserRole ur on ur.UserRoleID = sw.UserRoleID
inner join dbo.[TblPagePermission] tp on tp.PermissionPageID = up.PermissionPageID
inner join dbo.TblPageType pt on pt.PageTypeID = tp.PageTypeID
inner join dbo.TblSetPagePermission sp on sp.PermissionPageID = up.PermissionPageID and tu.UserLoginID = sp.UserLoginID";
            sql += " where tu.UserLoginID = " + sid;
            sql += " order by pt.PageType, tp.OrderNumber";
            ds = html.LoadData(sql);

            var serializedIdentity = JsonConvert.SerializeObject(SysAndWH);
            context.AdditionalResponseParameters.Add("UserID", sid);
            context.AdditionalResponseParameters.Add("UserRolesID", ds.Tables[0].Rows[0]["UserRoleID"].ToString());
            context.AdditionalResponseParameters.Add("UserRolesName", ds.Tables[0].Rows[0]["RoleName"].ToString());
            context.AdditionalResponseParameters.Add("ImageLink", ds.Tables[0].Rows[0]["ImageLink"].ToString());
            context.AdditionalResponseParameters.Add("DefaultDashboardPage", ds.Tables[0].Rows[0]["DefaultDashboardPage"].ToString());
            context.AdditionalResponseParameters.Add("ISChangePasswordOnNextLogon", ds.Tables[0].Rows[0]["ISChangePasswordOnNextLogon"].ToString());
            context.AdditionalResponseParameters.Add("SysAndWH", serializedIdentity);

            if (context.TokenIssued)
            {
                // client information
                //var accessExpiration = easternTimeNow.AddMinutes(15);
                var accessExpiration = easternTimeNow.AddDays(7);
                context.Properties.ExpiresUtc = accessExpiration;
                context.AdditionalResponseParameters.Add("TokenExpires", accessExpiration);
                context.AdditionalResponseParameters.Add("LoginTime", easternTimeNow.ToString());
                context.AdditionalResponseParameters.Add("UserFullName", ds.Tables[0].Rows[0]["FullName"].ToString());
                context.AdditionalResponseParameters.Add("Poistion", ds.Tables[0].Rows[0]["Job"].ToString());
                context.AdditionalResponseParameters.Add("UserProfilePicture", ds.Tables[0].Rows[0]["UserPicture"].ToString());
                context.AdditionalResponseParameters.Add("UserName", context.Identity.Name);
            }

            List<Menu> Menu = new List<Menu>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                Menu.Add(new Menu
                {
                    NAVICON = dr["NAVICON"].ToString(),
                    PermissionPageCode = dr["PermissionPageCode"].ToString(),
                    PageLink = dr["PageLink"].ToString(),
                    MenuID = dr["MenuID"].ToString(),
                    ParentPageCode = dr["ParentPageCode"].ToString(),
                    PageTypeID = dr["PageTypeID"].ToString(),
                    PageType = dr["PageType"].ToString(),
                    SystemID = dr["SystemID"].ToString(),
                    WarehouseID = dr["WarehouseID"].ToString(),
                    DomainID = DomainID,
                });
            }
            //context.AdditionalResponseParameters.Add("ISChangePasswordOnNextLogon", ds.Tables[0].Rows[0]["ISChangePasswordOnNextLogon"].ToString());
            serializedIdentity = JsonConvert.SerializeObject(Menu);
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }
            context.AdditionalResponseParameters.Add("Menu", serializedIdentity);

            sql = @"select distinct pc.PageCategoryID, pc.PageCategory, pc.PageLink, pc.NAVICON, pc.OrderNumber,
            sw.SystemID, sw.WarehouseID
            from dbo.TblUserRolePagePermission sp
            inner join dbo.TblPagePermission pp on pp.PermissionPageID = sp.PermissionPageID
            inner join dbo.TblPageCategory pc on pc.PageCategoryID = pp.PageCategoryID
            inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserRoleID = sp.UserRoleID
			inner join dbo.TblSetPagePermission sp1 on sp1.PermissionPageID = sp.PermissionPageID and sw.UserLoginID = sp1.UserLoginID
            where sw.UserLoginID = @UID 
            order by pc.OrderNumber;

            SELECT DISTINCT
                pp.PageCategoryID,
                pp.PermissionPageCode,
                pp.PageLink,
                pp.ParentPageCode,
                pp.NAVICON,
                pp.MenuID,
                sw.SystemID,
                sw.WarehouseID,
                pp.OrderNumber,
                ISNULL(pp.PageSubCategory, '') AS PageSubCategory
            FROM dbo.TblSetUserAccessSystemAndWarehouse sw
            INNER JOIN dbo.TblAdminAddPageToSystem ap ON ap.SystemID = sw.SystemID
            INNER JOIN dbo.TblUserRolePagePermission up ON up.UserRoleID = sw.UserRoleID
                                                           AND up.PermissionPageID = ap.PermissionPageID
            INNER JOIN dbo.TblSetPagePermission sp ON sp.PermissionPageID = ap.PermissionPageID
                                                           AND sp.UserLoginID = sw.UserLoginID
            INNER JOIN dbo.TblPagePermission pp ON pp.PermissionPageID = ap.PermissionPageID
            WHERE sw.UserLoginID = @UID
            order by pp.OrderNumber
";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "UID", sid.ToString() }
            };
            string UserMenu = html.JsonData(sql, false, para, true);
            context.AdditionalResponseParameters.Add("UserMenu", UserMenu);
            return Task.CompletedTask;
        }
        public static AuthenticationProperties CreateProperties(OAuthGrantResourceOwnerCredentialsContext context)
        {

            IDictionary<string, string> data = new Dictionary<string, string>
        {
            { "UserName", context.UserName }
        };

            var response = new AuthenticationProperties(data)
            {
                ExpiresUtc = DateTime.Now.AddMonths(1)
            };

            return response;
        }
    }
    public class SysAndWH
    {
        public string UserID = "";
        public string SystemID = "";
        public string WarehouseID = "";
        public string SystemName = "";
        public string WarehouseName = "";
        public string DefaultDashboardPage = "";
        public string SystemLogo = "";
        public string WarehouseLogo = "";
        public string AsideLightSidebar = "";
        public string Body = "";
        public string DomainID = "";
        public string Position = "";
        public CompanyDto Company { get; set; }
    }
    public class Menu
    {
        public string NAVICON { get; set; }
        public string PermissionPageCode { get; set; }
        public string PageLink { get; set; }
        public string MenuID { get; set; }
        public string ParentPageCode { get; set; }
        public string PageTypeID { get; set; }
        public string PageType { get; set; }
        public string SystemID { get; set; }
        public string WarehouseID { get; set; }
        public string DomainID = "";

    }

    public static class ContextHelper
    {
        public static void SetCustomError(this OAuthGrantResourceOwnerCredentialsContext context, string error, string errorMessage)
        {
            var json = new ResponseMessage
            { Data = error, Message = errorMessage, IsError = true }.ToJsonString();
            context.SetError(json);
            context.Response.Write(json);
            Invoke(context).Wait();
        }
        static async Task Invoke(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var owinResponseStream = new MemoryStream();
            var customResponseBody = new System.Net.Http.StringContent(JsonConvert.SerializeObject(new ResponseMessage()));
            var customResponseStream = await customResponseBody.ReadAsStreamAsync();
            await customResponseStream.CopyToAsync(owinResponseStream);
            context.Response.ContentType = "application/json";
            context.Response.ContentLength = customResponseStream.Length;
            context.Response.Body = owinResponseStream;
        }
    }
    public class ResponseMessage
    {
        public bool IsError { get; set; }
        public string Data { get; set; }
        public string Message { get; set; }
    }
}