using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Customers
{
    public class CustomersController : Controller
    {
        // GET: Customers
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CustomerProfile()
        {
            return View();
        }
    }
}