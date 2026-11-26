using MVC;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Support
{
    [Authorize]
    public class SupportUserProfileController : Controller
    {
        // GET: SupportUserProfile
        public ActionResult Index()
        {
            //HTML html = new HTML();
            //string sql = "select isnull(UserPicture,'') UserPicture from dbo.TblUserLogin where UserLoginID=@UserLoginID";
            //Dictionary<string, string> para = new Dictionary<string, string>();
            //para.Add("UserLoginID", html.UserID);
            //DataSet ds = html.LoadData(sql, false, para);
            //ViewBag.HasUserProfilePicture = ds.Tables[0].Rows[0]["UserPicture"];
            //ViewBag.UserPicture = html.ServerRoot + "Uploads/UserProfilePicture/" + html.UserFullName + "/" + ds.Tables[0].Rows[0]["UserPicture"];
            return View();
        }
        public JsonResult LoadUserProfilePic()
        {
            HTML html = new HTML();
            string sql = @"select isnull(u.UserPicture,'') UserPicture, u.FullName, u.Job, u.Telephone, u.Signature, u.TelegramUserName, u.Password, u.UserLoginID,
            u.Email, u.CreateDate, u.LoginTime, ur.RoleName, u.ImageLink from dbo.TblUserLogin u
            inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = u.UserLoginID
            inner join dbo.TblUserRole ur on ur.UserRoleID = sw.UserRoleID
            where u.UserLoginID = @UserLoginID and sw.SystemID = @SystemID and sw.WarehouseID = @WarehouseID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"UserLoginID",html.UserID },
                {"SystemID",html.SystemID },
                {"WarehouseID",html.WarehouseID },
            };
            DataSet ds = html.LoadData(sql, false, para);
            string HasUserProfilePicture = ds.Tables[0].Rows[0]["UserPicture"].ToString();
            string ImageLink = ds.Tables[0].Rows[0]["ImageLink"].ToString();
            string UserPicture = !string.IsNullOrEmpty(ImageLink) ? ImageLink : html.ServerRoot + "Uploads/UserProfilePicture/" + html.UserFullName + "/" + ds.Tables[0].Rows[0]["UserPicture"].ToString();
            return Json(new { HasUserProfilePicture = HasUserProfilePicture, UserPicture = UserPicture, DataSet = ds.Tables[0].Rows[0].ToJsonString() }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveChangePhotoProfile()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            sqlResult = html.UploadFile("Uploads/UserProfilePicture/" + html.UserFullName);
            string strfile;
            if (sqlResult.Result == ResultType.Successful)
                strfile = sqlResult.Value;
            else
            {
                return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
            }
            string sql = @"update dbo.TblUserLogin set UserPicture = @UserPicture 
            where UserLoginID = @UserLoginID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "UserPicture", strfile },
                { "UserLoginID", html.UserID }
            };

            sqlResult = html.SaveChangeData(sql, false, para);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UserInforForm()
        {
            HTML html = new HTML();
            string PrimaryFieldValue = html.UserID;
            html.FormID = "CreateUser";
            html.DrawBtnSaveChange = true;
            html.FieldIDFocus = "TxtFullName";
            //html.UseTable = true;
            //html.TableColumn = 2;
            html.TblWidth = "auto";
            html.TblAlign = "center";
            html.TableName = "TblUserLogin";
            html.PrimaryField = "UserLoginID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.SaveChangeCallbackFunction = "LoadUserProfilePic();UserInforForm();";
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Full Name:",
                ID = "TxtFullName",
                DBField = "FullName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Password:",
                ID = "TxtPassword",
                DBField = "Password",
                Required = true,
                Type = "password",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Confirm Password:",
                ID = "TxtConfirmPassword",
                Required = true,
                Type = "password",
                EqualTo = "TxtPassword",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Telephone:",
                ID = "TxtTelephone",
                DBField = "Telephone",
                Required = true,
                Type = "tel"
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Address:",
                ID = "TxtAddress",
                DBField = "Address"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Email:",
                ID = "TxtEmail",
                DBField = "Email",
                Type = "email",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Job:",
                ID = "TxtJob",
                DBField = "Job",
                Required = true
            });
            html.HTMLControls.Add(new HtmlFile()
            {
                Label = "Signature:",
                ID = "TxtSignature",
                DBField = "Signature",
                ISAddAttachmentFile = true,
                AcceptImageOnly = true,
                UploadMultiple = false,
                IsDragDropFile = true,
                DestinationFile = "Uploads/Users",
                Type = "file",
            });

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> SaveUserInfo(UserLogin model)
        {
            try
            {
                using (var db = new DBContext())
                {
                    var user = await db.TblUserLogin
                                 .FirstOrDefaultAsync(x => x.UserLoginID == model.UserLoginID);

                    if (user == null)
                        return Json(new { success = false, message = "User not found." });

                    user.FullName = model.FullName;
                    user.Email = model.Email;
                    user.Telephone = model.Telephone;
                    user.Job = model.Job;
                    user.TelegramUserName = model.TelegramUserName;
                    user.ImageLink = model.ImageLink;

                    db.SaveChanges();

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> UploadSignature(HttpPostedFileBase SignatureFile, int UserLoginID)
        {
            // ── Server-side validation ──────────────────────────────
            if (SignatureFile == null || SignatureFile.ContentLength == 0)
                return Json(new { success = false, message = "No file received." });

            var allowedTypes = new[] { "image/png", "image/jpeg", "image/gif", "image/webp", "application/pdf" };
            if (!allowedTypes.Contains(SignatureFile.ContentType))
                return Json(new { success = false, message = "File type not allowed." });

            const int maxBytes = 5 * 1024 * 1024;
            if (SignatureFile.ContentLength > maxBytes)
                return Json(new { success = false, message = "File exceeds 5 MB limit." });

            // ── Build a unique filename to avoid collisions ─────────
            string ext = Path.GetExtension(SignatureFile.FileName);
            string fileName = $"{UserLoginID}_{DateTime.Now:yyyyMMddHHmmssfff}{ext}";
            string savePath = Path.Combine(Server.MapPath("~/Uploads/Users"), fileName);

            // ── Save file to disk ───────────────────────────────────
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)); // ensure folder exists
            SignatureFile.SaveAs(savePath);

            // ── Append to user's Signature column ──────────────────
            using (var db = new DBContext())
            {
                var user = await db.TblUserLogin
                                   .FirstOrDefaultAsync(x => x.UserLoginID == UserLoginID);

                if (user == null)
                    return Json(new { success = false, message = "User not found." });

                user.Signature = string.IsNullOrWhiteSpace(user.Signature)
                    ? fileName
                    : user.Signature + "~" + fileName;

                await db.SaveChangesAsync(); // must be awaited
            }

            return Json(new { success = true });
        }
    }
}