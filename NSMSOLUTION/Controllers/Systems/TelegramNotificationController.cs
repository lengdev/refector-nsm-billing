using MVC;
using Newtonsoft.Json;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Systems
{
    public class TelegramNotificationController : Controller
    {
        // GET: TelegramNotification
        public ActionResult Index()
        {
            HTML html = new HTML();
            string sql = "select ROW_NUMBER() over(order by TelegramNotificationID) as [No], TelegramNotificationID, ActionName, IsEnable from dbo.TblTelegramNotification where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            html.ShowPrintOnTopTable = false;
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.GH.SQL = sql;
            html.OrderBy = "TelegramNotificationID";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "TelegramNotificationID",FieldName = "TelegramNotificationID", IsHiddenField = true },
                new GridField() { Header = "Action Name",FieldName = "ActionName" },
                new GridField() { Header = "Enabled", FieldName = "IsEnable", DisplayCheckBox= true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public JsonResult SaveChangeData(string Data)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            dynamic dt = JsonConvert.DeserializeObject(Data);
            string sql = "";
            foreach (dynamic item in dt)
            {
                string ID = item["ID"];
                string IsEnable = item["IsEnable"];
                sql += "update dbo.TblTelegramNotification set IsEnable ='" + IsEnable + "' where TelegramNotificationID = " + ID + " and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + ";";
            }
            sqlResult = html.SaveChangeData(sql, false, false);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}