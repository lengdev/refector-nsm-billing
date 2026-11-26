using AllReportAPi.Services;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System.Web.Http;

namespace AllReportAPi.Controllers
{
    [RoutePrefix("api/AllReport")]
    public class PurchaseReportApiController : ReportApiControllerBase
    {
        [HttpGet, Route("purchase-order-list")]
        public IHttpActionResult PurchaseOrderList([FromUri] ReportFilter f) =>
            RunService<POListResponseDto>(f, (s, filter) => s.Purchase.PurchaseOrderList(filter));

        [HttpGet, Route("purchase-order-detail")]
        public IHttpActionResult PurchaseOrderDetail([FromUri] ReportFilter f) =>
            RunService<PODetailResponseDto>(f, (s, filter) => s.Purchase.PurchaseOrderDetail(filter));

        [HttpGet, Route("expense-summary")]
        public IHttpActionResult ExpenseSummary([FromUri] ReportFilter f) =>
            RunService<ExpsenseSummaryDto>(
                f,
                (s, filter) => s.Purchase.ExpsenseSummary(filter),
                (s, rows) => s.Purchase.FillExpenseMemo(rows));


        [HttpGet, Route("expense-detail")]
        public IHttpActionResult ExpenseDetail([FromUri] ReportFilter f) =>
            RunService<ExpenseDetailDto>(
                f,
                (s, filter) => s.Purchase.ExpenseDetail(filter),
                (s, rows) => s.Purchase.FillExpenseMemo(rows));

        [HttpGet, Route("payment-summary")]
        public IHttpActionResult PaymentSummary([FromUri] ReportFilter f) =>
            RunService<PaymentSummaryDto>(
                f,
                (s, filter) => s.Purchase.PaymentSummary(filter));

        [HttpGet, Route("payment-detail")]
        public IHttpActionResult PaymentDetail([FromUri] ReportFilter f) =>
            RunService<PaymentDetailDto>(
                f,
                (s, filter) => s.Purchase.PaymentDetail(filter));

    }
}