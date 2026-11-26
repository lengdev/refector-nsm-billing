using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System.Web.Http;

namespace AllReportAPi.Controllers
{
    [RoutePrefix("api/AllReport")]
    public class InventoryReportApiController : ReportApiControllerBase
    {
        [HttpGet, Route("stock-summary")]
        public IHttpActionResult StockSummary([FromUri] ReportFilter f) =>
            RunService<StockSummaryDto>(f, (s, filter) => s.Inventory.StockSummary(filter));

        [HttpGet, Route("stock-movement")]
        public IHttpActionResult StockMovement([FromUri] ReportFilter f) =>
            RunService<StockMovementDto>(f, (s, filter) => s.Inventory.StockMovement(filter));

        // ───────────── Stock value ─────────────
        [HttpGet, Route("stock-value-summary")]
        public IHttpActionResult StockValueSummary([FromUri] ReportFilter f) =>
            RunService<StockValueDto>(f, (s, filter) => s.Inventory.StockValueSummary(filter));

        [HttpGet, Route("stock-value-detail")]
        public IHttpActionResult StockValueDetail([FromUri] ReportFilter f) =>
            RunService<StockMoveDto>(f, (s, filter) => s.Inventory.StockValueDetail(filter));

        [HttpGet, Route("stock-work-sheet")]
        public IHttpActionResult StockWorkSheet([FromUri] ReportFilter f) =>
            RunService<StockValueDto>(f, (s, filter) => s.Inventory.StockValueSummary(filter));

        [HttpGet, Route("stock-reconciliation")]
        public IHttpActionResult StockReconciliation([FromUri] ReportFilter f) =>
            RunService<StockReconDto>(f, (s, filter) => s.Inventory.StockReconciliation(filter));

        [HttpGet, Route("stock-value-and-balance-sheet-account-variance")]
        public IHttpActionResult StockValueVariance([FromUri] ReportFilter f) =>
            RunService<CategoryValueDto>(f, (s, filter) => s.Inventory.StockValueVariance(filter));

        // ───────────── FIFO ─────────────
        [HttpGet, Route("fifo-layer-balance-detail")]
        public IHttpActionResult FifoLayerDetail([FromUri] ReportFilter f) =>
            RunService<FifoLayerDto>(f, (s, filter) => s.Inventory.FifoLayerDetail(filter));

        [HttpGet, Route("fifo-layer-balance")]
        public IHttpActionResult FifoLayerBalance([FromUri] ReportFilter f) =>
            RunService<StockValueDto>(f, (s, filter) => s.Inventory.FifoLayerBalance(filter));

        // ───────────── Transfers ─────────────
        [HttpGet, Route("stock-transfer-detail")]
        public IHttpActionResult StockTransferDetail([FromUri] ReportFilter f) =>
            RunService<TransferDto>(f, (s, filter) => s.Inventory.StockTransferDetail(filter));

        [HttpGet, Route("stock-transfer-summary")]
        public IHttpActionResult StockTransferSummary([FromUri] ReportFilter f) =>
            RunService<TransferSummaryDto>(f, (s, filter) => s.Inventory.StockTransferSummary(filter));

        // ───────────── Consignment ─────────────
        [HttpGet, Route("consignment-balance")]
        public IHttpActionResult ConsignmentBalance([FromUri] ReportFilter f) =>
            RunService<ConsignmentDto>(f, (s, filter) => s.Inventory.ConsignmentBalance(filter));

        [HttpGet, Route("consignment-movement")]
        public IHttpActionResult ConsignmentMovement([FromUri] ReportFilter f) =>
            RunService<ConsignmentMoveDto>(f, (s, filter) => s.Inventory.ConsignmentMovement(filter));

        [HttpGet, Route("consignment-settlement")]
        public IHttpActionResult ConsignmentSettlement([FromUri] ReportFilter f) =>
            RunService<ConsignmentMoveDto>(f, (s, filter) => s.Inventory.ConsignmentSettlement(filter));

        [HttpGet, Route("consignment-aging")]
        public IHttpActionResult ConsignmentAging([FromUri] ReportFilter f) =>
            RunService<ConsignmentAgingDto>(f, (s, filter) => s.Inventory.ConsignmentAging(filter));

        // ───────────── Lot / Serial ─────────────
        [HttpGet, Route("serial-register")]
        public IHttpActionResult SerialRegister([FromUri] ReportFilter f) =>
            RunService<SerialDto>(f, (s, filter) => s.Inventory.SerialRegister(filter));

        [HttpGet, Route("lot-balance")]
        public IHttpActionResult LotBalance([FromUri] ReportFilter f) =>
            RunService<LotDto>(f, (s, filter) => s.Inventory.LotBalance(filter));

        [HttpGet, Route("lot-expiry-aging")]
        public IHttpActionResult LotExpiryAging([FromUri] ReportFilter f) =>
            RunService<LotAgingDto>(f, (s, filter) => s.Inventory.LotExpiryAging(filter));

        [HttpGet, Route("lot-reconciliation")]
        public IHttpActionResult LotReconciliation([FromUri] ReportFilter f) =>
            RunService<LotReconDto>(f, (s, filter) => s.Inventory.LotReconciliation(filter));

        [HttpGet, Route("lot-movement-detail")]
        public IHttpActionResult LotMovementDetail([FromUri] ReportFilter f) =>
            RunService<LotMoveDto>(f, (s, filter) => s.Inventory.LotMovementDetail(filter));

        [HttpGet, Route("lot-serial-traceability")]
        public IHttpActionResult LotSerialTraceability([FromUri] ReportFilter f) =>
            RunService<LotMoveDto>(f, (s, filter) => s.Inventory.LotMovementDetail(filter));

        // ───────────── Reorder ─────────────
        [HttpGet, Route("reorder-list")]
        public IHttpActionResult ReorderList([FromUri] ReportFilter f) =>
            RunService<ReorderDto>(f, (s, filter) => s.Inventory.ReorderList(filter));
    }
}