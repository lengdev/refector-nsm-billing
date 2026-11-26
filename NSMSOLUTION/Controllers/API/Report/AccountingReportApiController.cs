using AllReportAPi.Services;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System.Web.Http;

namespace AllReportAPi.Controllers
{
    [RoutePrefix("api/AllReport")]
    public class AccountingReportApiController : ReportApiControllerBase
    {
        // ───────────── ledger style ─────────────
        [HttpGet, Route("general-ledger")]
        public IHttpActionResult GeneralLedger([FromUri] ReportFilter f) =>
            RunService<LedgerDto>(f, (s, filter) => s.Accounting.GeneralLedger(filter));

        [HttpGet, Route("journal-report")]
        public IHttpActionResult JournalReport([FromUri] ReportFilter f) =>
            RunService<LedgerDto>(
                f,
                (s, filter) => s.Accounting.JournalReport(filter));

        [HttpGet, Route("transaction-detail-by-account")]
        public IHttpActionResult TransactionDetailByAccount([FromUri] ReportFilter f) =>
            RunService<LedgerDto>(f, (s, filter) => s.Accounting.TransactionDetailByAccount(filter));

        [HttpGet, Route("transaction-log")]
        public IHttpActionResult TransactionLog(int transactionId)
        {
            if (transactionId <= 0)
                return BadRequest("A valid transaction ID is required.");

            var current = Scope;
            if (current == null)
                return BadRequest("The current system or warehouse could not be resolved.");

            try
            {
                using (var services = new ReportServices(current))
                {
                    var result = services.Accounting.TransactionLog(transactionId);
                    return result == null ? (IHttpActionResult)NotFound() : Ok(result);
                }
            }
            catch (System.Exception ex)
            {
                return Fail(ex);
            }
        }

        [HttpGet, Route("balance-sheet")]   
        public IHttpActionResult BalanceSheet([FromUri] ReportFilter f) =>
            RunService<BalanceSheetDto>(f, (s, filter) => s.Accounting.BalanceSheet(filter));

        // ───────────── trial balance ─────────────
        [HttpGet, Route("trial-balance")]
        public IHttpActionResult TrialBalance([FromUri] ReportFilter f) =>
            RunService<TrialBalanceDto>(f, (s, filter) => s.Accounting.TrialBalance(filter));

        [HttpGet, Route("adjusted-trial-balance")]
        public IHttpActionResult AdjustedTrialBalance([FromUri] ReportFilter f) =>
            RunService<WorkingTrialBalanceDto>(f, (s, filter) => s.Accounting.AdjustedTrialBalance(filter));

        [HttpGet, Route("working-trial-balance")]
        public IHttpActionResult WorkingTrialBalance([FromUri] ReportFilter f) =>
            RunService<WorkingTrialBalanceDto>(f, (s, filter) => s.Accounting.WorkingTrialBalance(filter));

        // ───────────── profit / loss ─────────────
        [HttpGet, Route("profit-loss-standard")]
        public IHttpActionResult ProfitLossStandard([FromUri] ReportFilter f) =>
            RunService<ProfitLossDto>(f, (s, filter) => s.Accounting.ProfitLossStandard(filter));

        [HttpGet, Route("profit-loss-detail")]
        public IHttpActionResult ProfitLossDetail([FromUri] ReportFilter f) =>
            RunService<LedgerDto>(f, (s, filter) => s.Accounting.ProfitLossDetail(filter));

        [HttpGet, Route("profit-loss-year-to-date")]
        public IHttpActionResult ProfitLossYearToDate([FromUri] ReportFilter f) =>
            RunService<ProfitLossDto>(f, (s, filter) => s.Accounting.ProfitLossYearToDate(filter));

        [HttpGet, Route("profit-loss-by-month")]
        public IHttpActionResult ProfitLossByMonth([FromUri] ReportFilter f) =>
            RunService<ProfitLossByMonthDto>(f, (s, filter) => s.Accounting.ProfitLossByMonth(filter));

        [HttpGet, Route("profit-loss-by-year")]
        public IHttpActionResult ProfitLossByYear([FromUri] ReportFilter f) =>
            RunService<ProfitLossByYearDto>(f, (s, filter) => s.Accounting.ProfitLossByYear(filter));

        [HttpGet, Route("profit-loss-by-class")]
        public IHttpActionResult ProfitLossByClass([FromUri] ReportFilter f) =>
            RunService<ProfitLossByClassCustomerDto>(f, (s, filter) => s.Accounting.ProfitLossByClass(filter));

        [HttpGet, Route("profit-loss-by-customer")]
        public IHttpActionResult ProfitLossByCustomer([FromUri] ReportFilter f) =>
            RunService<ProfitLossByClassCustomerDto>(f, (s, filter) => s.Accounting.ProfitLossByCustomer(filter));
    }
}