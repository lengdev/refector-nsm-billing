using MVC;
using NSMSOLUTION.Controllers.Inventory;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class ApproveDisapproveItemRequestController : ApiController
    {
        // GET api/<controller>
        [HttpPost]
        [Route("api/ApproveDisapproveItemRequest/ApproveDisapprove")]
        public async Task<HttpResponseMessage> ApproveDisapprove()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            int ID = int.Parse(HttpContext.Current.Request.Form["ID"].ToString());
            string Status = HttpContext.Current.Request.Form["Status"].ToString();
            string From = HttpContext.Current.Request.Form["From"].ToString();
            string RequestMode = HttpContext.Current.Request.Form["RequestMode"].ToString();
            string ApprovalType = HttpContext.Current.Request.Form["ApprovalType"].ToString();
            ItemsController itemsController = new ItemsController();

            if (RequestMode == "Request Stock Return" || RequestMode == "Stock Return")
            {
                await itemsController.ApproveDisapproveRequestReturn(ID, Status, RequestMode, "@" + From);
            }
            else
            {
                await itemsController.ApproveDisapproveItemRequest(ID, Status, "@" + From, RequestMode, ApprovalType);
            }

            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
        [HttpPost]
        [Route("api/ApproveDisapproveItemRequest/ConfirmReceivedItemRequest")]
        public async Task<HttpResponseMessage> ConfirmReceivedItemRequest()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            int ID = int.Parse(HttpContext.Current.Request.Form["ID"].ToString());
            string From = HttpContext.Current.Request.Form["From"].ToString();
            string MessageId = HttpContext.Current.Request.Form["MessageId"].ToString();
            string MessageText = HttpContext.Current.Request.Form["MessageText"].ToString();
            ItemsController itemsController = new ItemsController();
            await itemsController.SaveChangeConfirmItemRequest("", "@" + From, ID, MessageId, MessageText);

            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
    }
}