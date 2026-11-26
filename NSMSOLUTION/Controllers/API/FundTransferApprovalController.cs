using MVC;
using NSMSOLUTION.Controllers.Customers;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Utilities;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;

namespace NSMSOLUTION.Controllers.API
{
    [System.Web.Http.Authorize]
    public class FundTransferApprovalController : ApiController
    {
        [HttpPost]
        [Route("api/FundTransferApproval/ApprovalFT")]
        public async Task<HttpResponseMessage> ApprovalFT()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();

            int FTID = 0;
            if (HttpContext.Current.Request.Form["FTID"] != null)
                FTID = int.Parse(HttpContext.Current.Request.Form["FTID"].ToString());

            int StatusID = 90;
            if (HttpContext.Current.Request.Form["Status"] != null)
                StatusID = int.Parse(HttpContext.Current.Request.Form["Status"].ToString());

            string FirstName = HttpContext.Current.Request.Form["FirstName"]?.ToString() ?? "";
            string LastName = HttpContext.Current.Request.Form["LastName"]?.ToString() ?? "";
            string UserName = HttpContext.Current.Request.Form["UserName"]?.ToString() ?? "";
            string TelegramGroupID = HttpContext.Current.Request.Form["TelegramGroupID"]?.ToString() ?? "";
            string ChatID = HttpContext.Current.Request.Form["ChatID"]?.ToString() ?? "";
            string MessageID = HttpContext.Current.Request.Form["MessageID"]?.ToString() ?? "";
            string Comment = HttpContext.Current.Request.Form["Comment"]?.ToString() ?? "";

            string UserFullName = FirstName + " " + LastName;
            string NeedToCheckUserID = "0";

            // Query user info by Telegram UserName
            string sql = @"SELECT UserLoginID, FullName, TelegramUserName FROM dbo.TblUserLogin WHERE TelegramUserName = @TelegramUserName OR TelegramUserName = @UserName";
            Dictionary<string, string> para = new Dictionary<string, string> {
                {"TelegramUserName", "@" + UserName },
                {"UserName", UserName }
            };

            DataSet ds = html.LoadData(sql, false, para);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                int UserLoginID = int.Parse(ds.Tables[0].Rows[0]["UserLoginID"].ToString());
                NeedToCheckUserID = UserLoginID.ToString();
                UserFullName = UserLoginID == 38 ? "Boss" : ds.Tables[0].Rows[0]["FullName"].ToString();
            }

            FundTransferController ftController = new FundTransferController();
            JsonResult jsonResult = await ftController.ApproveFundTransfer(FTID, StatusID, Comment, !string.IsNullOrEmpty(TelegramGroupID) ? long.Parse(TelegramGroupID) : 0, FirstName, LastName, UserName, UserFullName, NeedToCheckUserID, ChatID, MessageID);

            // Extract the message from the JsonResult
            var dynamicResult = (dynamic)jsonResult.Data;
            string finalMessage = dynamicResult.Message;

            return Request.CreateResponse(HttpStatusCode.OK, finalMessage);
        }
    }
}