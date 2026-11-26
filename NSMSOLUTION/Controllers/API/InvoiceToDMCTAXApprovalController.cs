using MVC;
using NewInventorySystem.Controllers;
using NSMSOLUTION.Controllers.Customers.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    [Authorize]
    public class InvoiceToDMCTAXApprovalController : ApiController
    {
        // GET api/<controller>
        [HttpPost]
        [Route("api/InvoiceToDMCTAXApproval/Approval")]
        public async Task<HttpResponseMessage> Approval()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            int CustomerID = int.Parse(HttpContext.Current.Request.Form["CustomerID"].ToString());
            string InvoiceNo = HttpContext.Current.Request.Form["InvoiceNo"].ToString();
            string Status = HttpContext.Current.Request.Form["Status"].ToString();
            string From = HttpContext.Current.Request.Form["From"].ToString();
            string ApproveRequestToDMCTAXTGMID = HttpContext.Current.Request.Form["MessageId"].ToString();
            string MessageText = HttpContext.Current.Request.Form["MessageText"].ToString();
            CustomerReportsController customerReports = new CustomerReportsController();
            HttpContext httpContext = HttpContext.Current;
            await customerReports.ApprovalInvoiceToDMCTAX(CustomerID, InvoiceNo, From, Status, "", false, true, httpContext, int.Parse(ApproveRequestToDMCTAXTGMID));
            //			string sql = @"select RequestApprovalToDMCTAXTgUser, u.FullName, ApproveRequestToDMCTAXMsg from dbo.TblCustomerInvoice ci
            //inner join dbo.TblUserLogin u on u.UserLoginID = ci.RequestApprovalToDMCTAXID
            //where ci.CustomerID = @CustomerID and ci.InvoiceNo = @InvoiceNo and ci.RequestApprovalToDMCTAXTgUser='@'+@RequestApprovalToDMCTAXTgUser";
            //			Dictionary<string, string> para = new Dictionary<string, string>
            //			{
            //				{"RequestApprovalToDMCTAXTgUser",From },
            //				{"CustomerID",CustomerID.ToString() },
            //				{"InvoiceNo",InvoiceNo },
            //			};
            //			DataSet ds = html.LoadData(sql, false, para);
            //			if (ds.Tables[0].Rows.Count > 0)
            //			{
            //				string ApproveRequestToDMCTAXMsg = ds.Tables[0].Rows[0]["ApproveRequestToDMCTAXMsg"].ToString();
            //				string approvalusername = ds.Tables[0].Rows[0]["FullName"].ToString();
            //				sql = @"update dbo.TblCustomerInvoice set ApproveRequestToDMCTAXTGMID = @ApproveRequestToDMCTAXTGMID,
            //ApproveRequestToDMCTAXStatus = @ApproveRequestToDMCTAXStatus
            //where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo";
            //				para = new Dictionary<string, string>
            //			{
            //				{"ApproveRequestToDMCTAXTGMID",ApproveRequestToDMCTAXTGMID },
            //				{"ApproveRequestToDMCTAXStatus",Status },
            //				{"CustomerID",CustomerID.ToString() },
            //				{"InvoiceNo",InvoiceNo },
            //			};
            //				sqlResult = html.SaveChangeData(sql, false, para);
            //				NotificationTelegram notificationTelegram = new NotificationTelegram();
            //				string msg = "Dear Team,<br><br>";
            //				if (Status == "Approve")
            //					msg += "Now, Invoice#" + InvoiceNo + " for DMC and TAX System is approved by " + approvalusername;
            //				else
            //					msg += "Now, Invoice#" + InvoiceNo + " for DMC and TAX System is disapproved by " + approvalusername;
            //				msg += "<br><br>Thank you and best regards,<br><br>";
            //				msg += "NSM ISP BILLING SYSTEM AUTO SEND Request approve Invoice to DMC & TAX System. #" + InvoiceNo;
            //				sqlResult = await notificationTelegram.NotificationTelegramBot("", -2042086477, "", "", msg, true, true, 0, int.Parse(ApproveRequestToDMCTAXTGMID), MessageText, null, true);
            //			}
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }

        [HttpPost]
        [Route("api/InvoiceToDMCTAXApproval/SendApprovalInvoice")]
        public async Task<HttpResponseMessage> SendApprovalInvoice()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            int CustomerID = int.Parse(HttpContext.Current.Request.Form["CustomerID"].ToString());
            string InvoiceNo = HttpContext.Current.Request.Form["InvoiceNo"].ToString();
            string Status = HttpContext.Current.Request.Form["Status"].ToString();
            HttpContext httpContext = HttpContext.Current;
            AddCustomerController addCustomerController = new AddCustomerController();

            string sql = @"select CustomerCode, CustomerName, CustomerID, InvoiceNo, InvoiceDate,
RequestApprovalToDMCTAXTgUser, RequestApprovalToDMCTAXID, RequestApprovalToDMCTAX, TypeOfInvoice
from dbo.TblCustomerInvoice where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo";
            Dictionary<string, string> Para = new Dictionary<string, string>
            {
                { "CustomerID", CustomerID.ToString() },
                { "InvoiceNo",  InvoiceNo }
            };
            if (Status == "Disapprove")
            {
                sql = @"update TblCustomerInvoice set ApproveRequestToDMCTAXStatus = 'DisapproveCheckedInvDMC' where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo";
                sqlResult = html.SaveChangeData(sql, false, Para);
            }
            else
            {
                DataSet ds = html.LoadData(sql, false, Para);
                DataRow dr = ds.Tables[0].Rows[0];
                string CustomerCode = dr["CustomerCode"].ToString();
                string CustomerName = dr["CustomerName"].ToString();
                string InvoiceDate = dr["InvoiceDate"].ToString();
                string RequestApprovalTgUser = dr["RequestApprovalToDMCTAXTgUser"].ToString();
                int RequestApprovalNameID = int.Parse(dr["RequestApprovalToDMCTAXID"].ToString());
                string RequestApprovalName = dr["RequestApprovalToDMCTAX"].ToString();
                string TypeOfInvoice = dr["TypeOfInvoice"].ToString();

                await addCustomerController.RequestApproveInvToDMCTAX(CustomerCode, CustomerName, CustomerID, InvoiceNo, InvoiceDate, RequestApprovalTgUser, RequestApprovalNameID, RequestApprovalName, "", "", "", TypeOfInvoice);
            }
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }

        [HttpPost]
        [Route("api/InvoiceToDMCTAXApproval/SendApprovalInvoiceCISP")]
        public async Task<HttpResponseMessage> SendApprovalInvoiceCISP()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            int CustomerID = int.Parse(HttpContext.Current.Request.Form["CustomerID"].ToString());
            string InvoiceNo = HttpContext.Current.Request.Form["InvoiceNo"].ToString();
            int StatusID = Convert.ToInt32(HttpContext.Current.Request.Form["StatusID"].ToString());
            string UserName = HttpContext.Current.Request.Form["UserName"].ToString();
            HttpContext httpContext = HttpContext.Current;
            AddCustomerController addCustomerController = new AddCustomerController();
            string sql = @"select * from TblCustomerBalance cb
inner join TblCustomerInvoice ci on ci.CustomerID = cb.CustomerID and ci.InvoiceNo = cb.InvoiceNo 
where cb.CustomerID = @CustomerID and cb.InvoiceNo = @InvoiceNo";
            Dictionary<string, string> Para = new Dictionary<string, string>
            {
                { "CustomerID", CustomerID.ToString() },
                { "InvoiceNo",  InvoiceNo }
            };
            DataSet ds = html.LoadData(sql, false, Para);
            string DescriptionTicketTelegramSupportTeam = "";
            string InvoiceDate = "";
            string CreateBy = "";
            string InvoiceType = "";
            string CustomerName = "";
            string CustomerCode = "";
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DescriptionTicketTelegramSupportTeam = ds.Tables[0].Rows[0]["DescriptionTicketTelegram"].ToString();
                InvoiceDate = ds.Tables[0].Rows[0]["InvoiceDate"].ToString();
                CreateBy = ds.Tables[0].Rows[0]["CreateBy"].ToString();
                InvoiceType = ds.Tables[0].Rows[0]["InvoiceType"].ToString();
                CustomerName = ds.Tables[0].Rows[0]["CustomerName"].ToString();
                CustomerCode = ds.Tables[0].Rows[0]["CustomerCode"].ToString();
            }

            string DescriptionTicketTelegram = $@"Dear Team,\n\nNow we have new customer services that's {(InvoiceType == "Renew Invoice" ? "renew" : "registered")} by " + CreateBy + ".\n\nPlease check invoice for detail.\n\nThank you and best regards\n\nNSM ISP BILLING SYSTEM AUTO GENERATE INVOICE. Invoice#" + InvoiceNo;

            //await addCustomerController.ApprovalInvCISP(CustomerID, InvoiceNo, StatusID, UserName);
            await addCustomerController.SendNotificationToTelegram(CustomerName, CustomerCode, CustomerID, InvoiceNo, InvoiceDate, 0, DescriptionTicketTelegram, DescriptionTicketTelegramSupportTeam, 1, 0, false, "", InvoiceType, false, false, true, false, StatusID, UserName);
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
    }
}