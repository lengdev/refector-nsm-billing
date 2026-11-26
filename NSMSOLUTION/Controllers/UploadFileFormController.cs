using MVC;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace NewInventorySystem.Controllers
{
    public class UploadFileFormController : Controller
    {
        // GET: UploadFileForm
        public ActionResult Index(object[] PrimaryFieldValue, bool AllowOnlyImage = false, bool ShowAvatar = false)
        {
            //dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(PrimaryFieldValue[0].ToString());
            bool SaveOnItsForm = false;
            string ContolName = "CustomerAttFiles";
            ViewBag.ContolName = ContolName;
            if (PrimaryFieldValue[0].ToString().Contains("SaveOnItsForm"))
            {
                SaveOnItsForm = true;
                ViewBag.DataInfo = PrimaryFieldValue[0];//new JavaScriptSerializer().Serialize(PrimaryFieldValue);
            }
            else if (PrimaryFieldValue[0].ToString().Contains("URL"))
            {
                ViewBag.DataInfo = HttpUtility.UrlDecode(PrimaryFieldValue[0].ToString());
            }
            if (PrimaryFieldValue[0].ToString().Contains("AllowOnlyImage"))
            {
                Dictionary<string, string> Datainfo = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(PrimaryFieldValue[0].ToString());
                AllowOnlyImage = bool.Parse(Datainfo["AllowOnlyImage"].ToString());
            }
            if (PrimaryFieldValue[0].ToString().Contains("ContolName"))
            {
                //dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(PrimaryFieldValue[0].ToString());
                Dictionary<string, string> Datainfo = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(PrimaryFieldValue[0].ToString());
                ContolName = Datainfo["ContolName"];
                if (ContolName == "")
                    ContolName = "CustomerAttFiles";
                ViewBag.ContolName = ContolName;
            }
            ViewBag.AllowOnlyImage = AllowOnlyImage;
            ViewBag.ShowAvatar = ShowAvatar;
            ViewBag.SaveOnItsForm = SaveOnItsForm;
            return View();
        }
        public ActionResult WebCam()
        {
            return View();
        }

        public ActionResult SaveUploadFile(object[] DataInfo)
        {
            HTML html = new HTML();
            dynamic Dtinfo = new JavaScriptSerializer().DeserializeObject(DataInfo[0].ToString());

            //string CustomerName = HttpUtility.UrlDecode(DataInfo[0]["CustomerName"]);
            //CustomerName = $"{html.ToUrlSlug($"{CustomerName}")}";
            //string AttachmentFile = html.UploadFile("CustomerAttFiles", CustomerName);
            //string DestinationFolder = "Uploads/CustomerAttFiles/" + CustomerName;
            //string SystemID = html.SystemID;
            //string WarehouseID = html.WarehouseID;
            return View();
        }
    }
}