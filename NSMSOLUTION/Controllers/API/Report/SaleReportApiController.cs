using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System.Web.Http;

namespace AllReportAPi.Controllers
{
    [RoutePrefix("api/AllReport")]
    public class SaleReportApiController : ReportApiControllerBase
    {
        // ───────────── Quotation ─────────────
        [HttpGet, Route("quotation-list")]
        public IHttpActionResult QuotationList([FromUri] ReportFilter f) =>
            RunService<QuotationListDto>(f, (s, filter) => s.Sale.QuotationList(filter));

        [HttpGet, Route("pi-list")]
        public IHttpActionResult PiList([FromUri] ReportFilter f) =>
            RunService<PiListDto>(f, (s, filter) => s.Sale.PiList(filter));

        [HttpGet, Route("quotation-detail")]
        public IHttpActionResult QuotationDetail([FromUri] ReportFilter f) =>
            RunService<QuotationDetailDto>(f, (s, filter) => s.Sale.QuotationDetail(filter));

        // ───────────── Sale: by customer ─────────────
        [HttpGet, Route("sale-by-customer-summary")]
        public IHttpActionResult SaleByCustomerSummary([FromUri] ReportFilter f) =>
            RunService<SaleByCustomerSummaryDto>(f, (s, filter) => s.Sale.SaleByCustomerSummary(filter));

        [HttpGet, Route("sale-by-customer-detail")]
        public IHttpActionResult SaleByCustomerDetail([FromUri] ReportFilter f) =>
            RunService<SaleInvoiceDto>(f, (s, filter) => s.Sale.SaleByCustomerDetail(filter));

        [HttpGet, Route("sale-by-invoice")]
        public IHttpActionResult SaleByInvoice([FromUri] ReportFilter f) =>
            RunService<SaleInvoiceDto>(f, (s, filter) => s.Sale.SaleByInvoice(filter));

        [HttpGet, Route("sale-by-saleman-detail")]
        public IHttpActionResult SaleBySalemanDetail([FromUri] ReportFilter f) =>
            RunService<SaleInvoiceDto>(f, (s, filter) => s.Sale.SaleBySalemanDetail(filter));

        // ───────────── Sale: summaries ─────────────
        [HttpGet, Route("sale-by-block-summary")]
        public IHttpActionResult SaleByBlockSummary([FromUri] ReportFilter f) =>
            RunService<SaleByBlockSummaryDto>(f, (s, filter) => s.Sale.SaleByBlockSummary(filter));

        [HttpGet, Route("sale-by-saleman-summary")]
        public IHttpActionResult SaleBySalemanSummary([FromUri] ReportFilter f) =>
            RunService<SaleBySalemanSummaryDto>(f, (s, filter) => s.Sale.SaleBySalemanSummary(filter));

        [HttpGet, Route("sale-summary-by-month")]
        public IHttpActionResult SaleSummaryByMonth([FromUri] ReportFilter f) =>
            RunService<SaleByMonthDto>(f, (s, filter) => s.Sale.SaleSummaryByMonth(filter));

        // ───────────── Sale: items ─────────────
        [HttpGet, Route("sale-by-item-detail")]
        public IHttpActionResult SaleByItemDetail([FromUri] ReportFilter f) =>
            RunService<SaleItemLineDto>(f, (s, filter) => s.Sale.SaleByItemDetail(filter));

        [HttpGet, Route("sale-by-item-summary")]
        public IHttpActionResult SaleByItemSummary([FromUri] ReportFilter f) =>
            RunService<SaleByItemSummaryDto>(f, (s, filter) => s.Sale.SaleByItemSummary(filter));

        // ───────────── Delivery ─────────────
        [HttpGet, Route("sale-delivery-summary")]
        public IHttpActionResult SaleDeliverySummary([FromUri] ReportFilter f) =>
            RunService<SaleDeliveryDto>(f, (s, filter) => s.Sale.SaleDeliverySummary(filter));

        [HttpGet, Route("sale-delivery-detail")]
        public IHttpActionResult SaleDeliveryDetail([FromUri] ReportFilter f) =>
            RunService<SaleDeliveryDto>(f, (s, filter) => s.Sale.SaleDeliveryDetail(filter));

        [HttpGet, Route("open-sale-delivery")]
        public IHttpActionResult OpenSaleDelivery([FromUri] ReportFilter f) =>
            RunService<SaleDeliveryDto>(f, (s, filter) => s.Sale.OpenSaleDelivery(filter));

        // ───────────── Sale orders ─────────────
        [HttpGet, Route("sale-order-list")]
        public IHttpActionResult SaleOrderList([FromUri] ReportFilter f) =>
            RunService<SaleOrderDto>(f, (s, filter) => s.Sale.SaleOrderList(filter));

        [HttpGet, Route("sale-order-detail")]
        public IHttpActionResult SaleOrderDetail([FromUri] ReportFilter f) =>
            RunService<SaleOrderDto>(f, (s, filter) => s.Sale.SaleOrderDetail(filter));

        [HttpGet, Route("open-sale-order-by-customer")]
        public IHttpActionResult OpenSaleOrderByCustomer([FromUri] ReportFilter f) =>
            RunService<OpenOrderDto>(f, (s, filter) => s.Sale.OpenSaleOrderByCustomer(filter));

        [HttpGet, Route("open-sale-order-by-item")]
        public IHttpActionResult OpenSaleOrderByItem([FromUri] ReportFilter f) =>
            RunService<OpenOrderDto>(f, (s, filter) => s.Sale.OpenSaleOrderByItem(filter));
    }
}