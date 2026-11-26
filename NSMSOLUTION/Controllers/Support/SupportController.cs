using MVC;
using System.Web.Mvc;

namespace NewInventorySystem.Controllers
{
    public class SupportController : Controller
    {
        // GET: Support
        [Authorize(Roles = "Show Support Page")]
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult LoadProject()
        {
            HTML html = new HTML();
            string sql = @"select SupportProjectID, ProjectName, Memo from dbo.TblSupportProject
            where SystemID =" + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            return Json(new { Data = html.JsonData(sql) }, JsonRequestBehavior.AllowGet);
        }
    }
}