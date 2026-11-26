using MVC;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers
{
    [Authorize]
    public class KendoPDFExportController : Controller
    {
        // GET: KendoPDFExport
        public ActionResult Index(int systemId = 13)
        {
            var html = new HTML();
            string sql = @"
                SELECT * FROM dbo.TblClass WHERE StatusID = 40 and SystemID = @SystemID;
            ";

            var para = new Dictionary<string, string>
            {
                { "@SystemID", systemId.ToString() }
            };

            var ds = html.LoadData(sql, false, para);
            ViewBag.ClassInfo = ds.Tables[0].Rows;
            bool ChangePDFExportTitle = HttpContext.GetOwinContext().Request.User.IsInRole("Change PDF Export Title");
            ViewBag.ChangePDFExportTitle = ChangePDFExportTitle;
            return View();
        }
    }
}