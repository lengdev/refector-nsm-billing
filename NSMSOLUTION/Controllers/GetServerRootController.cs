using System;
using System.Web.Mvc;

namespace NewInventorySystem.Controllers
{
    public class GetServerRootController : Controller
    {
        // GET: GetServerRoot
        public JsonResult Index()
        {
            string localPath = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
            return Json(new { ServerRoot = localPath }, JsonRequestBehavior.AllowGet);
        }
    }
}