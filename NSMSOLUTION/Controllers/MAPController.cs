using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace NewInventorySystem.Controllers
{
    public class MAPController : Controller
    {
        // GET: MAP
        [ValidateInput(false)]
        public ActionResult Index(object[] Data)
        {
            //SqlResult sqlResult = new SqlResult();
            //sqlResult.Result = ResultType.Successful;
            //sqlResult.Message = "Load Map Data Successful.";
            dynamic DataInfo = new JavaScriptSerializer().Serialize(Data[0].ToString());
            ViewBag.Data = DataInfo;//Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
            return View();// Json(new { Result = sqlResult.Result, Message= sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult NewVersion()
        {
            return View();
        }
        public ActionResult Draw()
        {
            return View();
        }
    }
}