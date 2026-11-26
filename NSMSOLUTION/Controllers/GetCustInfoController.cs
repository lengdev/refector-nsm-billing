using System.Web.Http;
using AuthorizeAttribute = System.Web.Http.AuthorizeAttribute;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class GetCustInfoController : ApiController
    {

        // GET api/<controller>/5
        public string Get(int id)
        {
            //HTML html = new HTML();
            //string sql = "select * from dbo.TblCustomer";
            //Dictionary<string, string> para = null;
            //string data = html.JsonData(sql, false, para, true);
            //string jsonString = JsonSerializer.Serialize(data);
            //dynamic CustomerDataInfo = new JavaScriptSerializer().DeserializeObject(data);
            //data = "[{a:123, b:323}]";



            //SendTelegramToCust();


            return id.ToString();
        }


    }
}