using MVC;
using MVC.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Mvc;
using Utilities;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;

namespace NewInventorySystem.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            HTML html = new HTML();
            string sql = @"SELECT 
    t.*, 
    s.value AS DomainID,
    d.DomainName
FROM 
    TblTemplate t
CROSS APPLY 
    STRING_SPLIT(ISNULL(t.ForDomainID, ''), ',') AS s
INNER JOIN 
    TblDomain d ON d.DomainID = TRY_CAST(s.value AS uniqueidentifier)
where d.DomainName=@HostName";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "HostName", html.HostName }
            };
            DataSet ds = html.LoadData(sql, false, para);
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string HostName = ds.Tables[0].Rows[0]["DomainName"].ToString();
                    string LoginTmpName = ds.Tables[0].Rows[0]["LoginTmpName"].ToString();
                    ViewBag.PartialView = html.HostName == HostName
                        ? $"~/Views/LayoutTemplate/Login/{LoginTmpName}.cshtml"
                        : "~/Views/LayoutTemplate/Login/_PartialPageLoginNSMBilling.cshtml";
                }
                else
                {
                    ViewBag.PartialView = "~/Views/LayoutTemplate/Login/_PartialPageLoginNSMBilling.cshtml";
                }
            }
            else
            {
                ViewBag.PartialView = "~/Views/LayoutTemplate/Login/_PartialPageLoginNSMBilling.cshtml";
            }

            //string baseAddress = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
            //using (var client = new HttpClient())
            //{
            //    var form = new Dictionary<string, string>
            //   {
            //       {"grant_type", "password"},
            //       {"username", "sothea"},
            //       {"password", "Thea8989"},
            //   };
            //    var tokenResponse = client.PostAsync(baseAddress + "/token", new FormUrlEncodedContent(form)).Result;
            //    //var token = tokenResponse.Content.ReadAsStringAsync().Result;
            //    var token = tokenResponse.Content.ReadAsAsync<Token>(new[] { new JsonMediaTypeFormatter() }).Result;
            //    if (string.IsNullOrEmpty(token.Error))
            //    {
            //        Console.WriteLine("Token issued is: {0}", token.AccessToken);
            //    }
            //    else
            //    {
            //        Console.WriteLine("Error : {0}", token.Error);
            //    }
            //    Console.Read();
            //}

            return View();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            //Token token = new Token();
            //RefreshTokenProvider token = new RefreshTokenProvider();
            //var a = token.Create(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ExternalBearer);

            Request.GetOwinContext().Authentication.SignOut();

            Request.GetOwinContext().Authentication.SignOut(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ApplicationCookie);

            HttpContext.GetOwinContext().Authentication.SignOut(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Login");
        }
        //[HttpPost]
        //public HttpResponseMessage ReSetToken()
        //{
        //    var identity = ((ClaimsPrincipal)User).Identity as ClaimsIdentity;
        //    var ticket = new AuthenticationTicket(identity, new AuthenticationProperties());
        //    DateTimeOffset currentUtc = new SystemClock().UtcNow;
        //    ticket.Properties.IssuedUtc = currentUtc;
        //    ticket.Properties.ExpiresUtc = currentUtc.AddMinutes(3000);//different expiration times
        //    string token = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
        //    return new HttpResponseMessage(HttpStatusCode.OK)
        //    {
        //        Content = new ObjectContent<object>(new
        //        {
        //            accessToken = token,
        //        }, Configuration.Formatters.JsonFormatter)
        //    };
        //}
        [HttpPost]
        public JsonResult ValidatePassword(string UserName, string Password)
        {
            SqlResult sqlResult = new SqlResult
            {
                Result = ResultType.Failed,
                Message = "Invalid Password"
            };
            string SQL = "select UserLoginID, PasswordSalt from dbo.TblUserLogin ";
            SQL += "where isnull(ISLocked, 0) = 0 and lower(UserName)= @UserName and StatusID = 3";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "UserName", UserName.ToLower() }
            };
            HTML html = new HTML();
            DataSet ds = html.LoadData(SQL, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string PasswordSalt = ds.Tables[0].Rows[0]["PasswordSalt"].ToString();
                string passwordHash = PasswordHelper.HashUsingPbkdf2(Password, Convert.FromBase64String(PasswordSalt));
                para.Clear();
                para.Add("UserName", UserName.ToLower());
                para.Add("Password", passwordHash);
                SQL = "select UserLoginID, UserName, UserTypeID, Telephone, Email from dbo.TblUserLogin ";
                SQL += "where StatusID = 3 and isnull(ISLocked, 0) = 0 and lower(UserName)= @UserName and Password = @Password";
                ds = html.LoadData(SQL, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = "Valid Password.";
                }
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveChangeResetPassword(string UserName, string Phone, string Password, int UserID = 0, bool ISChangePasswordOnNextLogon = false)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            string SQL = "select UserName, UserLoginID from dbo.TblUserLogin where UserName = @UserName and Telephone = @Phone";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "UserName", UserName.Trim() },
                { "Phone", Phone.Trim().Replace(" ", string.Empty) }
            };
            if (ISChangePasswordOnNextLogon)
            {
                SQL = "select UserName, UserLoginID from dbo.TblUserLogin where UserLoginID = @UserID";
                para.Clear();
                para.Add("UserID", UserID.ToString());

            }
            DataSet ds = html.LoadData(SQL, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string UserLoginID = ds.Tables[0].Rows[0]["UserLoginID"].ToString();
                var UserAgent = HttpContext.Request.Headers["User-Agent"];
                var WebBrowserName = HttpContext.Request.Browser.Browser;
                SQL = "update dbo.TblUserLogin set ISChangePasswordOnNextLogon =0, UserAgent = @UserAgent, WebBrowserName= @WebBrowserName, AttemptFailedCount = 0, ISLocked = 0, LockoutEnabled = 0, Password = @Password, PasswordSalt = @PasswordSalt, ChangePasswordDate = @ChangePasswordDate where UserLoginID=@UserLoginID;";
                SQL += "insert into TblUserLog (UserLoginID, LockedInDate, IP, HostName, WebBrowserName, UserAgent, ISLocked, LockoutEnabled, Comment, ChangePasswordDate) ";
                SQL += "Values (" + UserLoginID + ", '" + easternTimeNow.ToString() + "', '" + html.GetLocalIPAddress(out string HostName) + "', '" + HostName + "', '" + WebBrowserName + "', '" + UserAgent + "', 0, 0, 'Change Password Successful.', '" + easternTimeNow.ToString() + "');";
                para.Clear();
                string PasswordHash = PasswordHelper.HashUsingPbkdf2(Password, out string PasswordSalt);
                para.Add("UserLoginID", UserLoginID);
                para.Add("Password", PasswordHash);
                para.Add("PasswordSalt", PasswordSalt);
                para.Add("ChangePasswordDate", easternTimeNow.ToString());
                para.Add("UserAgent", UserAgent);
                para.Add("WebBrowserName", WebBrowserName);
                sqlResult = html.SaveChangeData(SQL, false, para, false);
                if (sqlResult.Result == ResultType.Successful)
                {
                    sqlResult.Message = "Reset Password Successful.";
                }
            }
            else
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = "User Name or Registered Phone does not existing in database.";
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}