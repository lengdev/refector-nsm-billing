using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System.Web.Http;

namespace AllReportAPi.Controllers
{
    [RoutePrefix("api/AllReport")]
    public class CustoemrReportApiController : ReportApiControllerBase
    {
        [HttpGet, Route("customer-list")]
        public IHttpActionResult CustomerList([FromUri] ReportFilter f) =>
            RunService<CustomerListDto>(f, (s, filter) => s.Customer.CustomerList(filter));

        [HttpGet, Route("customer-by-block")]
        public IHttpActionResult CustomerByBlock([FromUri] ReportFilter f) =>
            RunService<CustomerByBlockDto>(f, (s, filter) => s.Customer.CustomerByBlock(filter));

        [HttpGet, Route("customer-by-block-summary")]
        public IHttpActionResult CustomerByBlockSummary([FromUri] ReportFilter f) =>
            RunService<CustomerByBlockSummaryDto>(f, (s, filter) => s.Customer.CustomerByBlockSummary(filter));

        [HttpGet, Route("connection-summary")]
        public IHttpActionResult ConnectionSummary([FromUri] ReportFilter f) =>
            RunService<ConnectionSummaryDto>(f, (s, filter) => s.Customer.ConnectionSummary(filter));

        [HttpGet, Route("customer-by-connection")]
        public IHttpActionResult CustomerByConnection([FromUri] ReportFilter f) =>
            RunService<CustomerByConnectionDto>(f, (s, filter) => s.Customer.CustomerByConnection(filter));

        [HttpGet, Route("customer-feedback")]
        public IHttpActionResult CustomerFeedback([FromUri] ReportFilter f) =>
            RunService<CustomerFeedbackDto>(f, (s, filter) => s.Customer.CustomerFeedback(filter));

        [HttpGet, Route("customer-feedback-analysis")]
        public IHttpActionResult CustomerFeedbackAnalysis([FromUri] ReportFilter f) =>
            RunService<FeedbackAnalysisDto>(f, (s, filter) => s.Customer.CustomerFeedbackAnalysis(filter));
    }
}