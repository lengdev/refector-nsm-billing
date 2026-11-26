using System.Web.Mvc;

namespace NewInventorySystem.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            Session.Add("UserID", "1");
            Session.Add("WarehouseID", "1");
            return View();
        }
    }
}