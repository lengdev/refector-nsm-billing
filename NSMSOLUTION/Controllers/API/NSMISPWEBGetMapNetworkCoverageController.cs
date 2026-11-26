using MVC;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    [Authorize]
    public class NSMISPWEBGetMapNetworkCoverageController : ApiController
    {
        // GET api/<controller>

        // GET api/<controller>/5
        public HttpResponseMessage Get()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = "select * from NSMISPWEB_MapNetworkCoverage";
            sqlResult.Value = html.JsonData(sql);
            sqlResult.Result = ResultType.Successful;
            sqlResult.Message = "Load Data Successful.";
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult);
        }
    }
}