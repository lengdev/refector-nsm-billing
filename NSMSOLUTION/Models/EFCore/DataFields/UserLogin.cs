namespace NSMSOLUTION.Models.EFCore
{
    public class UserLogin
    {
        public int UserLoginID { get; set; }
        public int? UserRoleID { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Job { get; set; }
        public string TelegramUserName { get; set; }
        public string Signature { get; set; }
        public string ImageLink { get; set; }
        public int SystemID { get; set; }
    }
}