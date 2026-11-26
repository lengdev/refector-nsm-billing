using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace NSMSOLUTION.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int UserLoginID { get; set; }
        public
        override string UserName
        { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string FullName { get; set; }
        public string Telephone { get; set; }
        public string Address { get; set; }
        public
        override string Email
        { get; set; }
        public string UserPicture { get; set; }
        public string Job { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public int StatusID { get; set; }
        public int UserTypeID { get; set; }
        public bool IsLogged { get; set; }
        public DateTime LoginTime { get; set; }
        public string IP { get; set; }
        public string HostName { get; set; }
        public int AttemptFailedCount { get; set; }
        public bool ISLocked { get; set; }
        public DateTime LockOutEnd { get; set; }
        public
        override bool LockoutEnabled
        { get; set; }
        public DateTime ChangePasswordDate { get; set; }
        public string WebBrowserName { get; set; }
        public string UserAgent { get; set; }
        public bool ISSupportAdministrator { get; set; }
        public bool ISChangePasswordOnNextLogon { get; set; }
        public string Signature { get; set; }
        public string TelegramUserName { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
        public int TemplateID { get; set; }
    }
}