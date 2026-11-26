using AllReportAPi.Services;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System.Web.Http;

namespace AllReportAPi.Controllers
{
    [RoutePrefix("api/AllReport")]
    public class ReceivableReportApiController : ReportApiControllerBase
    {
        [HttpGet, Route("a-r-aging-summary")]
        public IHttpActionResult ARAgingSummary([FromUri] ReportFilter f) =>
            RunService<ArAgingDto>(f, (s, filter) => s.Receivable.ARAgingSummary(filter));

        [HttpGet, Route("a-r-aging-detail")]
        public IHttpActionResult ARAgingDetail([FromUri] ReportFilter f) =>
            RunService<ArAgingDetailDto>(f, (s, filter) => s.Receivable.ARAgingDetail(filter));

        [HttpGet, Route("a-r-aging-by-saleman")]
        public IHttpActionResult ARAgingBySaleman([FromUri] ReportFilter f) =>
            RunService<ArAgingBySalesmanDto>(f, (s, filter) => s.Receivable.ARAgingBySaleman(filter));

        [HttpGet, Route("a-r-aging-by-customer")]
        public IHttpActionResult ARAgingByCustomer([FromUri] ReportFilter f) =>
            RunService<ArAgingDto>(f, (s, filter) => s.Receivable.ARAgingByCustomer(filter));

        [HttpGet, Route("collection")]
        public IHttpActionResult Collection([FromUri] ReportFilter f) =>
            RunService<CollectionDto>(f, (s, filter) => s.Receivable.Collection(filter));

        [HttpGet, Route("daily-income")]
        public IHttpActionResult DailyIncome([FromUri] ReportFilter f) =>
            RunService<CollectionDto>(f, (s, filter) => s.Receivable.DailyIncome(filter));

        [HttpGet, Route("revenue-estimate")]
        public IHttpActionResult RevenueEstimate([FromUri] ReportFilter f) =>
            RunService<RevenueEstimate>(f, (s, filter) => s.Receivable.RevenueEstimate(filter));
    }
}