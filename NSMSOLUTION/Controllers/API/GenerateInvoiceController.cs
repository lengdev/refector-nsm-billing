using MVC;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NewInventorySystem.Controllers.API
{
    [Authorize]
    public class GenerateInvoiceController : ApiController
    {
        //[Authorize]
        //[Microsoft.SqlServer.Server.SqlFunction]
        //public static SqlString http(SqlString url)
        //{
        //    var wc = new WebClient();
        //    var html = wc.DownloadString(url.Value);
        //    return new SqlString(html);
        //}

        public GenerateInvoiceController()
        {
        }
        [HttpPost]
        //[Route("api/GenerateInvoice/InvoiceAsync")]
        public async Task<HttpResponseMessage> InvoiceAsync()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            sqlResult = await html.GenerateInvoice();
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
    }
}