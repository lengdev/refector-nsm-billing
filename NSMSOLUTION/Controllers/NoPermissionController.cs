using MVC;
using System.Web.Mvc;

namespace NewInventorySystem.Controllers
{
    public class NoPermissionController : Controller
    {
        // GET: NoPermission
        public ActionResult Index()
        {
            HTML html = new HTML();
            ViewBag.DrawHTMLForm = html.DrawAccessDeniedPage();
            return View();
        }
    }
}