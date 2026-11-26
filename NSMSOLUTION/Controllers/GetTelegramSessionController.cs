using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using TeleSharp.TL;
using TLSharp.Core;

namespace NSMSOLUTION.Controllers
{
    public class GetTelegramSessionController : Controller
    {
        // GET: GetTelegramSession

        public static dynamic client = null;
        public static dynamic registered = null;
        public static dynamic hash = null;
        TLUser user = null;
        //string phone = "+855965565118";
        string phone = "+85561588777";
        public async Task<ActionResult> Index()
        {
            string FilePath = HttpContext.Server.MapPath(@"/Uploads/");
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);
            DirectoryInfo dinfo = new DirectoryInfo(FilePath);
            FileSessionStore store = new FileSessionStore(dinfo);
            client = new TelegramClient(27294688, "708a468fb6811978a5ad88088a1a31ea", dcIpVersion: DataCenterIPVersion.OnlyIPv4, store: store, sessionUserId: "New_NSM_ISP_Telegram_SessionID_Finance");//, sessionUserId: "NSM_ISP_Telegram_SessionID"
                                                                                                                                                                                                        //client = new TelegramClient(10369643, "98a5c16a746f18a7eda29bb08d428502", dcIpVersion: DataCenterIPVersion.OnlyIPv4, store: store, sessionUserId: "Sothea");//, sessionUserId: "NSM_ISP_Telegram_SessionID"
            client.ConnectAsync().Wait();
            ViewBag.IsUserAuthorized = client.IsUserAuthorized();
            //Console.WriteLine($"Authorised: {client.IsUserAuthorized()}");			
            if (client.IsUserAuthorized())
                user = client.Session.TLUser;
            else
            {
                registered = await client.IsPhoneRegisteredAsync(phone);
                hash = await client.SendCodeRequestAsync(phone);
            }
            //else
            //{
            //	string phone = "+85561588777";
            //	var registered = await client.IsPhoneRegisteredAsync(phone);
            //	var hash = await client.SendCodeRequestAsync(phone);
            //	Console.Write("Code: ");
            //	var code = Console.ReadLine();
            //	//if (!registered)
            //	//{
            //	//	Console.WriteLine($"Sign up {phone}");
            //	//	user = await client.SignUpAsync(phone, hash, code, "First", "Last");
            //	//}
            //	Console.WriteLine($"Sign in {phone}");
            //	try
            //	{
            //		user = await client.MakeAuthAsync(phone, hash, code);
            //	}
            //	catch
            //	{
            //		var passwordSettings = await client.GetPasswordSetting();
            //		await client.MakeAuthWithPasswordAsync(passwordSettings, "Allaccount@NSM");
            //	}
            //}
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> Submit(string Code)
        {
            try
            {
                user = await client.MakeAuthAsync(phone, hash, Code.Trim());
            }
            catch
            {
                var passwordSettings = await client.GetPasswordSetting();
                //await client.MakeAuthWithPasswordAsync(passwordSettings, "Thea8989");
                await client.MakeAuthWithPasswordAsync(passwordSettings, "Allaccount@NSM");
            }
            return Json(new { Result = "ok" }, JsonRequestBehavior.AllowGet);
        }
    }
}