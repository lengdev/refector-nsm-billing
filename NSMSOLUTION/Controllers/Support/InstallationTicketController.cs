using NSMSOLUTION.Services;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Support
{
    [Authorize]
    public class InstallationTicketController : Controller
    {

        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> CreateTicket(InstallationTicketRequest request)
        {
            if (request == null)
            {
                return Json(new { success = false, message = "Request body is empty" }, JsonRequestBehavior.AllowGet);
            }

            var result = await InstallationTicketService.CreateTicketAsync(request);

            return Json(new
            {
                success = result.Success,
                statusCode = result.StatusCode,
                message = result.Message,
                data = result.Data,
                raw = result.RawResponse
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// POST: InstallationTicket/CreateNewInstallation
        /// Creates a ticket specifically for new installation with titleId: 99e3ae08-b782-4b4f-b53d-a6065e57b524
        /// </summary>
        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> CreateNewInstallation(
            int customerId,
            string cid,
            string sid,
            string description,
            string title = "New Installation",
            string telegramId = null,
            string priority = "HIGH",
            string titleId = null)
        {
            var result = await InstallationTicketService.CreateNewInstallationTicketAsync(
                customerId: customerId,
                cid: cid,
                sid: sid,
                description: description,
                title: title,
                telegramId: telegramId,
                priority: priority,
                titleId: titleId
            );

            return Json(new
            {
                success = result.Success,
                statusCode = result.StatusCode,
                message = result.Message,
                data = result.Data
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// POST: InstallationTicket/CreateFromInvoice
        /// Pulls customer and billing details from database to construct and send the ticket
        /// </summary>
        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> CreateFromInvoice(
            int customerId,
            string invoiceNo = null,
            string connectionId = null,
            string title = "New Installation",
            string customDescription = null,
            string telegramId = null,
            string priority = "HIGH")
        {
            var result = await InstallationTicketService.CreateFromCustomerInvoiceAsync(
                customerId: customerId,
                invoiceNo: invoiceNo,
                connectionId: connectionId,
                title: title,
                customDescription: customDescription,
                telegramId: telegramId,
                priority: priority
            );

            return Json(new
            {
                success = result.Success,
                statusCode = result.StatusCode,
                message = result.Message,
                data = result.Data
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
