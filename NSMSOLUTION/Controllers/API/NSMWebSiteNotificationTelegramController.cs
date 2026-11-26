using MVC;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    [Authorize]
    public class NSMWebSiteNotificationTelegramController : ApiController
    {
        // POST api/<controller>
        //
        [HttpPost]
        [Route("api/NSMWebSiteNotificationTelegram/UserSubscript")]
        public async Task<HttpResponseMessage> UserSubscript()
        {
            string PhoneNumber = HttpContext.Current.Request.Form["PhoneNumber"].ToString();
            string FirstName = HttpContext.Current.Request.Form["FirstName"].ToString();
            string LastName = HttpContext.Current.Request.Form["LastName"].ToString();
            int CustomerID = int.Parse(HttpContext.Current.Request.Form["CustomerID"].ToString());
            string MessageText = HttpContext.Current.Request.Form["MessageText"].ToString();
            string PackageName = HttpContext.Current.Request.Form["PackageName"].ToString();
            string Price = HttpContext.Current.Request.Form["Price"].ToString();
            string GoogleMap_Location = HttpContext.Current.Request.Form["GoogleMap_Location"].ToString();
            string GoogleMap_Longitude = HttpContext.Current.Request.Form["GoogleMap_Longitude"].ToString();
            string GoogleMap_Latitude = HttpContext.Current.Request.Form["GoogleMap_Latitude"].ToString();

            SqlResult sqlResult = new SqlResult();
            NotificationTelegram telegram = new NotificationTelegram();
            sqlResult = await telegram.NotificationToCustomer(PhoneNumber, FirstName, LastName == null ? "" : LastName, CustomerID, MessageText);
            string msg = "Dear Team,\n\n";
            msg += "Now we have new customer subscript internet service.\n\n";
            msg += "Customer Name: " + FirstName + "\n";
            msg += "Customer's Phone: " + PhoneNumber + "\n";
            msg += "Package Name: " + PackageName + "\n";
            msg += "Price: " + Price + "\n\n";
            msg += "Location: <a href=\"https://www.google.com/maps/place/" + GoogleMap_Latitude + ',' + GoogleMap_Longitude + "/@" + GoogleMap_Latitude + ',' + GoogleMap_Longitude + ",15z\">" + GoogleMap_Location + "</a>\n\n";
            msg += "Please call back to Customer Name: " + FirstName + "\n\n";
            msg += "Thank you and best regards, \n\nAUTO SEND NOTIFICATION FROM NSM ISP WEB SITE.";
            sqlResult = await telegram.NotificationTelegramBot(msg, -998910135);
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }

        [HttpPost]
        [Route("api/NSMWebSiteNotificationTelegram/GetQuoteNow")]
        public HttpResponseMessage GetQuoteNow()
        {
            string PhoneNumber = HttpContext.Current.Request.Form["PhoneNumber"].ToString();
            string FirstName = HttpContext.Current.Request.Form["FirstName"].ToString();

            SqlResult sqlResult = new SqlResult();
            NotificationTelegram telegram = new NotificationTelegram();
            string msg = "អ្នកបានស្នើរសម្រង់តម្លៃ កញ្ចប់សេវាកម្មអ៊ិនធឺណិតអិនអេសអិម។";
            msg += "\n\nបុគ្គលិកយើងខ្ញុំនឹងធ្វើការទំនាក់ទំនងទៅអ្នកវិញក្នុងពេលឆាប់ៗនេះ។\n";
            msg += "\nសម្រាប់ពត៌មានបន្ថែមសូមទាក់ទង\nផ្នែកបម្រើអតិថិជន: +85586588777, +85578588777\n";
            msg += "ផ្នែកបច្ចេកទេសៈ +85561876777\n";
            msg += "គេហទំព័រ: www.nsmisp.com.kh\n";
            msg += "\nសូមអរគុណ!";
            telegram.NotificationToCustomer(PhoneNumber, FirstName, "", 0, msg);
            msg = "Dear Team,\n\n";
            msg += "Now we have new customer request free quote for internet service.\n\n";
            msg += "Customer Name: " + FirstName + "\n";
            msg += "Customer's Phone: " + PhoneNumber + "\n";
            msg += "Please call back to Customer Name: " + FirstName + "\n\n";
            msg += "Thank you and best regards, \n\nAUTO SEND NOTIFICATION FROM NSM ISP WEB SITE.";
            telegram.NotificationTelegramBot(msg, -998910135);
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }

        [HttpPost]
        [Route("api/NSMWebSiteNotificationTelegram/Contact")]
        public async Task<HttpResponseMessage> Contact()
        {
            SqlResult sqlResult = new SqlResult();
            NotificationTelegram telegram = new NotificationTelegram();
            var httpRequest = HttpContext.Current.Request;
            string name = httpRequest.Form["name"];
            string phone = httpRequest.Form["phone"];
            string email = httpRequest.Form["email"];
            string subject = httpRequest.Form["subject"];
            string message = httpRequest.Form["message"];
            phone = phone.Trim().Replace(" ", string.Empty);
            var regex = new Regex(Regex.Escape("0"));
            phone = regex.Replace(phone, "+855", 1);
            string msg = "អ្នកបានផ្ញើរសារមកក្រុមហ៊ុន អិនអេសអិម។";
            msg += "\n\nបុគ្គលិកយើងខ្ញុំនឹងធ្វើការទំនាក់ទំនងទៅអ្នកវិញក្នុងពេលឆាប់ៗនេះ។\n";
            msg += "\nសម្រាប់ពត៌មានបន្ថែមសូមទាក់ទង\nផ្នែកបម្រើអតិថិជន: +85586588777, +85578588777\n";
            msg += "ផ្នែកបច្ចេកទេសៈ +85561876777\n";
            msg += "គេហទំព័រ: www.nsmisp.com.kh\n";
            //await telegram.NotificationToCustomer(phone, name, "", 0, msg);
            msg = "Dear Team,\n\n";
            msg += "Now we have new customer message.\n\n";
            msg += "Customer Name: " + name + "\n";
            msg += "Customer's Phone: " + phone + "\n";
            msg += "Customer's Email: " + email + "\n";
            msg += "Subject: " + subject + "\n";
            msg += "Message: " + message + "\n\n";
            msg += "Please call back to Customer Name: " + name + "\n\n";
            msg += "Thank you and best regards, \n\nAUTO SEND NOTIFICATION FROM NSM ISP WEB SITE.";
            await telegram.NotificationTelegramBot(msg, -998910135);
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
    }
}