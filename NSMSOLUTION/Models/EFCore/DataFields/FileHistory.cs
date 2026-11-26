using System;

namespace NSMSOLUTION.Models.EFCore
{
    public class FileHistory
    {
        public int ID { get; set; }
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
        public string Status { get; set; }
        public string Server { get; set; }
        public string TelegramUserName { get; set; }
    }
}