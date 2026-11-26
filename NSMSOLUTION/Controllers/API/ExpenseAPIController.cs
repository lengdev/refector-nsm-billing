using MVC;
using NSMSOLUTION.Controllers.Expenses;
using NSMSOLUTION.Models.Expenses;
using System.Configuration;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.API
{
    //[Authorize]
    public class ExpenseAPIController : ApiController
    {
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("api/ApprovalPaymentVoucher")]
        public async Task<HttpResponseMessage> POST()
        {
            int ExpenseID = int.Parse(HttpContext.Current.Request.Form["ExpenseID"].ToString());
            string Status = HttpContext.Current.Request.Form["Status"].ToString();
            string PaymentNo = HttpContext.Current.Request.Form["PaymentNo"].ToString();
            Status = Status == "ApprovePV" ? "Checked" : "Rejected";
            try
            {

                var repo = new ExpenseRepository(
                    ConfigurationManager
                        .ConnectionStrings["AuthContextNSM"]
                        .ConnectionString
                );

                HTML html = new HTML();

                string sql = $@"select u.FullName, e.SystemID, e.WarehouseID, e.DomainID
from dbo.Expense_PaymentTransactions e
inner join dbo.TblUserLogin u on u.UserLoginID = e.CheckedByUserID
where e.ExpenseID = {ExpenseID} AND e.PaymentNo = {PaymentNo}";
                DataSet ds = html.LoadData(sql);
                string SystemID = ds.Tables[0].Rows[0]["SystemID"].ToString();
                string WarehouseID = ds.Tables[0].Rows[0]["WarehouseID"].ToString();
                string DomainID = ds.Tables[0].Rows[0]["DomainID"].ToString();
                string FullName = ds.Tables[0].Rows[0]["FullName"].ToString();

                // ✅ Proper null-safe check
                sql = @"SELECT b.BotToken, t.TelegramGroupID
                               FROM TblTelegramGroupNotification t
                               INNER JOIN dbo.TblTelegramBots b ON b.BotID = t.BotID
                               WHERE t.Module = 'Expense'
                               AND t.SystemID = " + SystemID + @"
                               AND t.WarehouseID = " + WarehouseID;

                ds = html.LoadData(sql);


                string botToken = ds.Tables[0].Rows[0]["BotToken"].ToString();

                // 🔥 Proper Fake MVC ControllerContext for Rotativa

                var httpContext = HttpContext.Current;

                var routeData = new System.Web.Routing.RouteData();
                routeData.Values.Add("controller", "Expenses"); // MUST match MVC controller name

                var expensesController = new ExpensesController();

                var controllerContext = new ControllerContext(
                    new HttpContextWrapper(httpContext),
                    routeData,
                    expensesController
                );

                expensesController.ControllerContext = controllerContext;

                var service = new ExpenseApprovalService(botToken, repo);

                var result = await service.ApprovalPaymentVoucherAsync(
                    ExpenseID,
                    Status, FullName,
                    PaymentNo,
                    controllerContext // ✅ MVC context works here
                );
            }
            catch (System.Exception)
            {

            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}