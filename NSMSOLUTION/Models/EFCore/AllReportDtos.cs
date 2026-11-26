using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NSMSOLUTION.Models.EFCore
{
    // ═════════════════════════ Common ═════════════════════════

    public class ReportStatus
    {
        public int StatusID { get; set; }
        public string Status { get; set; }
    }

    public class ReportFilter
    {
        // purchase
        public string Supplier { get; set; }
        public string PONumber { get; set; }

        // period
        public string Month { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        // general
        public string User { get; set; }
        public string Category { get; set; }
        public int ItemID { get; set; }
        public string Product { get; set; }
        public string GroupName { get; set; }
        public string Warehouse { get; set; }
        public string Branch { get; set; }
        public string Brand { get; set; }
        public string Class { get; set; }
        public string InvoiceNo { get; set; }

        // customer / connection
        public int? CustomerID { get; set; }
        public string ServiceStatus { get; set; }
        public string SID { get; set; }
        public string ConnectionType { get; set; }
        public string Block { get; set; }

        // feedback
        public string InternetSpeed { get; set; }
        public string CustomerServices { get; set; }
        public string Complaint { get; set; }     // "Complaint" | "No Complaint" | empty


        // purchase & paymen
        public string Payee { get; set; }
        public string Account { get; set; }
        public string PVNumber { get; set; }
        public string SortOption { get; set; }
        // accounting
        public string TransactionType { get; set; }
        public string Customer { get; set; }
        public bool GroupByParentAccount { get; set; }
        public bool ShowAllAccounts { get; set; }


        // scope
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }

        // paging / sorting
        public string SortBy { get; set; }
        public string SortDir { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public bool AllRows { get; set; }
        public string TotalFields { get; set; }
    }

    public sealed class ReportScope
    {
        public int SystemId { get; set; }
        public int WarehouseId { get; set; }

        // same value, spelled like the SystemID columns and ReportFilter.SystemID
        public int SystemID { get { return SystemId; } }
    }

    // ═════════════════════════ Customer & Connection ═════════════════════════

    public class CustomerListDto
    {
        public DateTime? RegisterDate { get; set; }
        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNameLatin { get; set; }
        public string Phone { get; set; }
        public string Block { get; set; }
        public string Class { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string Commune { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public string MapAddress { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }

        public string FullAddress =>
            string.Join(", ", new[] { Address, Commune, District, Province }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim()));

        public string GoogleMap
        {
            get
            {
                string query = null;

                if (!string.IsNullOrWhiteSpace(Lat) && !string.IsNullOrWhiteSpace(Lng))
                    query = Lat.Trim() + "," + Lng.Trim();
                else if (!string.IsNullOrWhiteSpace(MapAddress))
                    query = MapAddress.Trim();

                return query == null
                    ? null
                    : "https://www.google.com/maps?q=" + Uri.EscapeDataString(query);
            }
        }
    }

    public class CustomerByBlockDto
    {
        public string Block { get; set; }
        public int CustomerID { get; set; }
        public DateTime? RegisterDate { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Class { get; set; }
        public string SID { get; set; }
        public string Type { get; set; }      // connection type
        public string Status { get; set; }
    }

    public class CustomerByBlockSummaryDto
    {
        public string Block { get; set; }
        public int Customers { get; set; }
        public int Connections { get; set; }
        public int ActiveConnections { get; set; }
    }

    public class CustomerByConnectionDto
    {
        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string SID { get; set; }
        public string ConnectionID { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Block { get; set; }
        public string Class { get; set; }
        public string Remark { get; set; }
        public int? DaysLeft { get; set; }
        public string Vlan { get; set; }
        public string PPPoE { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ActivateDate { get; set; }
    }

    public class ConnectionSummaryDto
    {
        public string Type { get; set; }
        public int Active { get; set; }
        public int Locked { get; set; }
        public int Terminate { get; set; }
        public int Inactive { get; set; }
        public int Total { get; set; }
    }

    public class CustomerFeedbackDto
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public DateTime DateFeedback { get; set; }
        public string InternetSpeed { get; set; }
        public string Pricing { get; set; }
        public string CustomerServices { get; set; }
        public int Ratingus { get; set; }
        public string CustomerComment { get; set; }
        public string Complaint { get; set; }
        public string ContactBy { get; set; }
        public string UserContact { get; set; }
        public bool IsNoAnswer { get; set; }
        public bool IsBusy { get; set; }
        public string CommentNoAnswerOrBusy { get; set; }
        public string FollowUpComment { get; set; }
        public string FollowUpBy { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class FeedbackAnalysisDto
    {
        public string Topic { get; set; }
        public string Rating { get; set; }
        public int Feedbacks { get; set; }
        public decimal SharePct { get; set; }
        public int Complaints { get; set; }
        public decimal ComplaintPct { get; set; }
    }

    public class FeedbackRow
    {
        public string Rating { get; set; }
        public bool Complained { get; set; }
    }

    // ═════════════════════════ Quotation & Sale ═════════════════════════

    public class QuotationListDto
    {
        public DateTime? Date { get; set; }
        public string QuotationNo { get; set; }
        public string Customer { get; set; }
        public string Salesman { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
    }

    public class PiListDto
    {
        public int CustomerID { get; set; }
        public DateTime? Date { get; set; }
        public string PiNo { get; set; }
        public string CustomerName { get; set; }
        public string Salesman { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
    }

    public class QuotationDetailDto
    {
        public int CustomerID { get; set; }
        public DateTime? Date { get; set; }
        public string QuotationNo { get; set; }
        public string CustomerName { get; set; }
        public string Description { get; set; }
        public decimal? Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Amount { get; set; }
    }

    public class SaleByCustomerSummaryDto
    {
        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public int Invoices { get; set; }
        public decimal Amount { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
    }

    public class SaleInvoiceDto
    {
        public int CustomerID { get; set; }
        public DateTime? Date { get; set; }
        public string InvoiceNo { get; set; }
        public string CustomerName { get; set; }
        public string Block { get; set; }
        public string Salesman { get; set; }
        public decimal Amount { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
    }

    public class SaleByBlockSummaryDto
    {
        public string Block { get; set; }
        public int Invoices { get; set; }
        public int Customers { get; set; }
        public decimal Amount { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
    }

    public class SaleBySalemanSummaryDto
    {
        public string Salesman { get; set; }
        public int Invoices { get; set; }
        public int Customers { get; set; }
        public decimal Amount { get; set; }
    }

    public class SaleByMonthDto
    {
        public string Month { get; set; }
        public int Invoices { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
    }

    public class SaleItemLineDto
    {
        public int CustomerID { get; set; }
        public DateTime? Date { get; set; }
        public string InvoiceNo { get; set; }
        public string CustomerName { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }

    public class SaleByItemSummaryDto
    {
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal Amount { get; set; }
    }

    public class SaleDeliveryDto
    {
        public int CustomerID { get; set; }
        public DateTime? Date { get; set; }
        public string DeliveryNo { get; set; }
        public string CustomerName { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public string Status { get; set; }
    }

    public class SaleOrderDto
    {
        public int CustomerID { get; set; }
        public DateTime? Date { get; set; }
        public string OrderNo { get; set; }
        public string CustomerName { get; set; }
        public string Salesman { get; set; }
        public string Status { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
    }

    public class OpenOrderDto
    {
        public string Name { get; set; }
        public int Orders { get; set; }
        public decimal OrderedQty { get; set; }
        public decimal OpenAmount { get; set; }
    }

    // ═════════════════════════ Receivable ═════════════════════════

    public class ArAgingDto
    {
        public int? CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public decimal Current { get; set; }
        public decimal Days1To30 { get; set; }
        public decimal Days31To60 { get; set; }
        public decimal Days61To90 { get; set; }
        public decimal Over90 { get; set; }
        public decimal Total { get; set; }
    }

    public class ArAgingBySalesmanDto
    {
        public string Saleman { get; set; }
        public decimal Current { get; set; }
        public decimal Days1To30 { get; set; }
        public decimal Days31To60 { get; set; }
        public decimal Days61To90 { get; set; }
        public decimal Over90 { get; set; }
        public decimal Total { get; set; }
    }

    public class ArAgingDetailDto
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNo { get; set; }
        public string Class { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int AgeDays { get; set; }
        public decimal Balance { get; set; }
    }

    public class CollectionDto
    {
        public DateTime? InvoiceDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string InvoiceNo { get; set; }
        public string ReceiptNo { get; set; }
        public string CustomerCode { get; set; }
        public int? CustomerID { get; set; }
        public string Customer { get; set; }
        public string Block { get; set; }
        public string Class { get; set; }
        public string Account { get; set; }
        public string Memo { get; set; }
        public decimal Balance { get; set; }
        public decimal Amount { get; set; }
        public string ReceivedBy { get; set; }
    }

    public class RevenueEstimate
    {
        public DateTime? Date { get; set; }
        public DateTime? EndDate { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Class { get; set; }
        public string Package { get; set; }
        public decimal? Amount { get; set; }
        public string Remark { get; set; }
    }

    // ═════════════════════════ Inventory ═════════════════════════
    public class StockSummaryDto
    {
        public int ItemID { get; set; }
        public string ItemCode { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public string ItemGroup { get; set; }
        public string UM { get; set; }
        public string Warehouse { get; set; }
        public string SerialNumber { get; set; }
        public decimal QtyIn { get; set; }
        public decimal QtyOut { get; set; }
        public decimal StockOnHand { get; set; }
        public decimal StockReturn { get; set; }
        public string Status { get; set; }
    }

    public class StockMovementDto
    {
        public DateTime? Date { get; set; }
        public string MoveGroup { get; set; }     // Receive / Request / Issue / Transfer / Return / Other
        public string Type { get; set; }          // the raw TblStockInDetail.Type
        public int? StockInID { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string SerialNumber { get; set; }
        public string Warehouse { get; set; }
        public decimal QtyIn { get; set; }
        public decimal QtyOut { get; set; }
    }

    public class StockValueDto
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Warehouse { get; set; }
        public string UM { get; set; }
        public decimal Qty { get; set; }
        public decimal AvgCost { get; set; }
        public decimal Value { get; set; }
    }

    public class StockMoveDto
    {
        public DateTime? Date { get; set; }
        public string RefNo { get; set; }
        public string MoveType { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Warehouse { get; set; }
        public string UM { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Value { get; set; }
    }

    public class StockReconDto
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal LedgerQty { get; set; }
        public decimal ItemQty { get; set; }
        public decimal Difference { get; set; }
    }

    public class CategoryValueDto
    {
        public string Category { get; set; }
        public decimal Qty { get; set; }
        public decimal Value { get; set; }
    }

    public class FifoLayerDto
    {
        public DateTime? Date { get; set; }
        public string RefNo { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Warehouse { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal RemainingQty { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Value { get; set; }
    }

    public class TransferSummaryDto
    {
        public DateTime? Date { get; set; }
        public string TransferNo { get; set; }
        public string FromWarehouse { get; set; }
        public string ToWarehouse { get; set; }
        public int Items { get; set; }
        public decimal Qty { get; set; }
    }

    public class TransferDto
    {
        public DateTime? Date { get; set; }
        public string TransferNo { get; set; }
        public string FromWarehouse { get; set; }
        public string ToWarehouse { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
    }

    public class ConsignmentDto
    {
        public string Customer { get; set; }
        public string ItemName { get; set; }
        public decimal QtyOut { get; set; }
        public decimal QtyReturned { get; set; }
        public decimal QtyBalance { get; set; }
        public decimal Value { get; set; }
    }

    public class ConsignmentMoveDto
    {
        public DateTime? Date { get; set; }
        public string RefNo { get; set; }
        public string Customer { get; set; }
        public string ItemName { get; set; }
        public string MoveType { get; set; }
        public decimal Qty { get; set; }
    }

    public class ConsignmentAgingDto
    {
        public string Customer { get; set; }
        public string ItemName { get; set; }
        public decimal Current { get; set; }
        public decimal Days1To30 { get; set; }
        public decimal Days31To60 { get; set; }
        public decimal Days61To90 { get; set; }
        public decimal Over90 { get; set; }
        public decimal Total { get; set; }
    }

    public class LotDto
    {
        public string ItemName { get; set; }
        public string LotNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Warehouse { get; set; }
        public decimal Qty { get; set; }
    }

    public class LotAgingDto
    {
        public string ItemName { get; set; }
        public string LotNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Expired { get; set; }
        public decimal Days30 { get; set; }
        public decimal Days60 { get; set; }
        public decimal Days90 { get; set; }
        public decimal Over90 { get; set; }
        public decimal Total { get; set; }
    }

    public class LotMoveDto
    {
        public DateTime? Date { get; set; }
        public string RefNo { get; set; }
        public string MoveType { get; set; }
        public string ItemName { get; set; }
        public string LotNo { get; set; }
        public string Warehouse { get; set; }
        public string Customer { get; set; }
        public decimal Qty { get; set; }
    }

    public class LotReconDto
    {
        public string ItemName { get; set; }
        public string LotNo { get; set; }
        public decimal LedgerQty { get; set; }
        public decimal SerialQty { get; set; }
        public decimal Difference { get; set; }
    }

    public class SerialDto
    {
        public string ItemName { get; set; }
        public string SerialNo { get; set; }
        public string LotNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Warehouse { get; set; }
        public string Status { get; set; }
    }

    public class ReorderDto
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal OnHand { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal MinQty { get; set; }
        public decimal SuggestedQty { get; set; }
    }

    // ═════════════════════════ Purchase & Payable ═════════════════════════

    public class POListResponseDto
    {
        public DateTime? Date { get; set; }
        public string PoNo { get; set; }
        public string Vendor { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
    }

    public class PODetailResponseDto
    {
        public DateTime? Date { get; set; }
        public string PoNo { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public double? Qty { get; set; }
        public double? Cost { get; set; }
        public double? Amount { get; set; }
    }

    public class PurchaseReceiveDto
    {
        public DateTime? Date { get; set; }
        public string ReceiveNo { get; set; }
        public string Vendor { get; set; }
        public string Warehouse { get; set; }
        public string Item { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
    }

    public class ExpsenseSummaryDto
    {
        public DateTime? ExpenseDate { get; set; }
        public int ExpenseID { get; set; }
        public string TranType { get; set; }
        public string PVNumber { get; set; }
        public string PaymentNo { get; set; }
        public string Status { get; set; }
        public string ClassName { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string Payee { get; set; }
        public string PaymentMethod { get; set; }
        public string Memo { get; set; }
        public decimal Amount { get; set; }
    }

    public class ExpenseDetailDto
    {
        public DateTime? ExpenseDate { get; set; }
        public int ExpenseID { get; set; }
        public string TranType { get; set; }
        public string PVNumber { get; set; }
        public string PaymentNo { get; set; }
        public string Status { get; set; }
        public string ClassName { get; set; }
        public string Payee { get; set; }
        public string Account { get; set; }
        public string Memo { get; set; }
        public decimal Amount { get; set; }
        public decimal Vat { get; set; }
        public string AccountName { get; set; }
        public string AccountCode { get; set; }
    }

    public class PaymentSummaryDto
    {
        public DateTime? PaymentDate { get; set; }
        public string TransactionType { get; set; }
        public string Number { get; set; }
        public string Vendor { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public string ClassName { get; set; }
        public string Memo { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public int ExpenseID { get; set; }
        public string PaymentNo { get; set; }
        public string Status { get; set; }

    }

    public class PaymentDetailDto
    {
        public string Vendor { get; set; }
        public string TransactionType { get; set; }
        public string BillNo { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentNo { get; set; }
        public decimal PayAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Currency { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrandAmount { get; set; }
        public string ClassName { get; set; }
        public string Memo { get; set; }
        public string MemoDetail { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public int ExpenseID { get; set; }
        public string Status { get; set; }
        public string RowType { get; set; }   // "detail" or "total"
        public int SortKey { get; set; }      // 0 = detail, 1 = total (keeps total below its rows)
        [Newtonsoft.Json.JsonIgnore]
        public bool ExcludeFromTotals { get; set; }
    }

    public class PaymentDetailGroupDto
    {
        public string Vendor { get; set; }
        public decimal TotalPayAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalGrandAmount { get; set; }
        public List<PaymentDetailDto> Rows { get; set; }
    }

    public class PurchaseByVendorDto
    {
        public string Vendor { get; set; }
        public string Item { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
    }

    public class PurchaseByItemDto
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public string UM { get; set; }
        public decimal AvgCost { get; set; }
        public decimal Amount { get; set; }
    }

    public class PurchaseInvoiceDto
    {
        public DateTime? Date { get; set; }
        public string BillNo { get; set; }
        public string Vendor { get; set; }
        public decimal Amount { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
    }

    public class ExpenseDto
    {
        public DateTime? Date { get; set; }
        public string Account { get; set; }
        public string Payee { get; set; }
        public string Memo { get; set; }
        public decimal Amount { get; set; }
    }

    public class ApAgingDto
    {
        public string VendorCode { get; set; }
        public string Vendor { get; set; }
        public decimal Current { get; set; }
        public decimal Days1To30 { get; set; }
        public decimal Days31To60 { get; set; }
        public decimal Days61To90 { get; set; }
        public decimal Over90 { get; set; }
        public decimal Total { get; set; }
    }

    public class ApAgingDetailDto
    {
        public string Vendor { get; set; }
        public string BillNo { get; set; }
        public DateTime? BillDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int AgeDays { get; set; }
        public decimal Balance { get; set; }
    }

    public class ApReconDto
    {
        public string Vendor { get; set; }
        public string BillNo { get; set; }
        public decimal BillAmount { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
    }

    public class PaymentDto
    {
        public DateTime? Date { get; set; }
        public string PaymentNo { get; set; }
        public string Vendor { get; set; }
        public string Method { get; set; }
        public int Level { get; set; }
        public string Display { get; set; }
        public string Account { get; set; }
        public decimal Amount { get; set; }
    }

    // ═════════════════════════ Accounting ═════════════════════════

    public class GeneralJournalDto
    {
        public DateTime? Date { get; set; }
        public string JournalNo { get; set; }
        public int? TransactionID { get; set; }
        public string TransactionType { get; set; }
        public string ItemName { get; set; }
        public int? CustomerID { get; set; }
        public int? AccountID { get; set; }
        public string ParentCode { get; set; }
        public string ParentName { get; set; }
        public int? ParentAccountID { get; set; }
        public string ParentAccountCode { get; set; }
        public string ParentAccountName { get; set; }
        public string JournalType { get; set; }     // "General" | "Adjusting"
        public string AccountCategory { get; set; } // NEW
        public string Status { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }     // Asset, Liability, Equity, Income, COGS, Expense
        public string Memo { get; set; }
        public string PartyName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Class { get; set; }
        public string Customer { get; set; }
        public string Branch { get; set; }
        public string Account { get { return AccountCode + " - " + AccountName; } }

    }

    public class LedgerDto
    {
        public string Transaction { get; set; }
        public DateTime? Date { get; set; }
        public string ItemName { get; set; }
        public string Number { get; set; }
        public string AccountName { get; set; }
        public string AccountCode { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public decimal Balance { get; set; }
        public string GroupAccount { get; set; }   // parent, e.g. "111000-Cash and Cash Equivalents"
        public string Account { get; set; }        // e.g. "111101-Cash in Bank"

        public int? TransactionID { get; set; }
        public string TypeLabel { get; set; }
        public int? ExpenseID { get; set; }
        public string Status { get; set; }       // expense status
        public string PaymentNo { get; set; }
        public int? CustomerID { get; set; }
        public string RowType { get; set; }      // "line" | "subtotal"
        public int Level { get; set; }
        public string Display { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public bool ExcludeFromTotals { get; set; }
    }

    public class TransactionLogDto
    {
        public int TransactionID { get; set; }
        public string TransactionType { get; set; }
        public string CreateOrModifyBy { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? CreateOrModifyDate { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
        public List<TransactionLogLineDto> Details { get; set; }
    }

    public class TransactionLogLineDto
    {
        public string ItemName { get; set; }
        public string Account { get; set; }
        public string Memo { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public class BalanceSheetDto
    {
        public string RowType { get; set; }       // header | account | total | grand
        public string Section { get; set; }
        public string Subsection { get; set; }
        public string Category { get; set; }
        public string AccountCategory { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public int Level { get; set; }
        public decimal Amount { get; set; }
    }

    public class TrialBalanceDto
    {
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string AccountType { get; set; }
        public string AccountCategory { get; set; }
        public string Status { get; set; }
        public bool ExcludeFromTotals { get; set; }
    }

    public class GenericReportDto
    {
        public DateTime? Date { get; set; }
        public string ReferenceNo { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
    }

    public class ProfitLossDto
    {
        public string Group { get; set; }      
        public string Section { get; set; }     
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public string RowType { get; set; }  
        public int Level { get; set; }      
        public decimal Percent { get; set; } 
        public string Display { get; set; }  
        public decimal YearToDate { get; set; }
        public decimal PreviousMonth { get; set; }
        public decimal ThisMonth { get; set; }
        public decimal ThisMonthLastYear { get; set; }
    }

    public class ProfitLossByMonthDto : ProfitLossDto
    {
        [Newtonsoft.Json.JsonExtensionData]
        public IDictionary<string, object> MonthlyValues { get; set; }
    }

    public class ProfitLossByYearDto : ProfitLossDto
    {
        [Newtonsoft.Json.JsonExtensionData]
        public IDictionary<string, object> YearlyValues { get; set; }
    }

    public class ProfitLossByClassCustomerDto : ProfitLossDto
    {
        [Newtonsoft.Json.JsonExtensionData]
        public IDictionary<string, object> GroupValues { get; set; }
    }

    public class WorkingTrialBalanceDto
    {
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal Begin { get; set; }      // NEW
        public decimal Change { get; set; }     // NEW
        public decimal Ending { get; set; }     // NEW
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal AdjDebit { get; set; }
        public decimal AdjCredit { get; set; }
        public decimal AdjustedDebit { get; set; }
        public decimal AdjustedCredit { get; set; }
    }

    // ═════════════════════════ Helpers ═════════════════════════

    public sealed class JournalDisplayLine
    {
        public int TransactionID { get; set; }
        public int AccountID { get; set; }
        public string AccountCategory { get; set; }
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public static class ReportHelpers
    {
        public static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static bool TryGetDisplayOnlyVatReceivableCorrection(
            IEnumerable<JournalDisplayLine> entries,
            out int receivableAccountId,
            out decimal adjustment)
        {
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            receivableAccountId = 0;
            adjustment = 0m;
            var lines = entries.ToList();
            Func<JournalDisplayLine, bool> isReceivable = line =>
                (line.AccountCategory ?? "").IndexOf("Receivable", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (line.AccountName ?? "").IndexOf("Receivable", StringComparison.OrdinalIgnoreCase) >= 0;
            Func<JournalDisplayLine, bool> isOutputVat = line =>
                (line.AccountName ?? "").IndexOf("Output VAT", StringComparison.OrdinalIgnoreCase) >= 0;

            var receivableDebits = lines
                .Where(line => line.Debit > 0m && isReceivable(line))
                .ToList();
            var vatCredits = lines
                .Where(line => line.Credit > 0m && isOutputVat(line))
                .ToList();
            var receivableDebit = receivableDebits.Sum(line => line.Debit);
            var outputVatCredit = vatCredits.Sum(line => line.Credit);
            var nonReceivableDebit = lines
                .Where(line => line.Debit > 0m && !isReceivable(line))
                .Sum(line => line.Debit);
            var nonVatCredit = lines
                .Where(line => line.Credit > 0m && !isOutputVat(line))
                .Sum(line => line.Credit);
            var missingReceivableDebit = lines.Sum(line => line.Credit - line.Debit);

            if (receivableDebits.Count != 1 ||
                vatCredits.Count == 0 ||
                outputVatCredit <= 0m ||
                nonReceivableDebit != 0m ||
                nonVatCredit != receivableDebit ||
                missingReceivableDebit != outputVatCredit)
                return false;

            receivableAccountId = receivableDebits[0].AccountID;
            adjustment = missingReceivableDebit;
            return true;
        }

        public static bool TryGetPeriod(string value, out DateTime start, out DateTime endExclusive)
        {
            endExclusive = default(DateTime);
            value = Clean(value);
            if (value == null)
            {
                start = default(DateTime);
                return false;
            }

            if (DateTime.TryParseExact(value, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
            {
                endExclusive = start.AddMonths(1);
                return true;
            }

            if (DateTime.TryParseExact(value, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
            {
                endExclusive = start.AddYears(1);
                return true;
            }

            return false;
        }

        public static bool TryGetMonthOnly(string value, out int month)
        {
            month = 0;
            value = Clean(value);
            return value != null
                && value.Length == 2
                && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out month)
                && month >= 1
                && month <= 12;
        }
    }

    public static class ReportQueryExtensions
    {
        private static readonly MethodInfo StringContains =
            typeof(string).GetMethod("Contains", new[] { typeof(string) });

        private static readonly Type[] NumericTypes =
        {
            typeof(decimal), typeof(double), typeof(float),
            typeof(int), typeof(long), typeof(short),
        };

        public static IQueryable<T> WhereContains<T>(
            this IQueryable<T> query,
            Expression<Func<T, string>> selector,
            string value)
        {
            var text = ReportHelpers.Clean(value);
            if (text == null) return query;

            var body = Expression.Call(selector.Body, StringContains, Expression.Constant(text));
            return query.Where(Expression.Lambda<Func<T, bool>>(body, selector.Parameters));
        }

        public static IQueryable<T> WhereEquals<T, TValue>(
            this IQueryable<T> query,
            Expression<Func<T, TValue>> selector,
            TValue value)
        {
            object boxed = value;
            if (boxed == null) return query;

            ConstantExpression constant;
            var str = boxed as string;
            if (str != null)
            {
                var text = ReportHelpers.Clean(str);
                if (text == null) return query;
                constant = Expression.Constant(text, typeof(string));
            }
            else
            {
                constant = Expression.Constant(value, typeof(TValue));
            }

            var body = Expression.Equal(selector.Body, constant);
            return query.Where(Expression.Lambda<Func<T, bool>>(body, selector.Parameters));
        }

        // Matches an answer; "(Not answered)" matches null, blank and "-".
        public static IQueryable<T> WhereAnswer<T>(
            this IQueryable<T> source, Expression<Func<T, string>> selector, string value)
        {
            value = (value ?? "").Trim();
            if (value == "") return source;

            var field = selector.Body;
            var trimmed = Expression.Call(field, typeof(string).GetMethod("Trim", Type.EmptyTypes));
            Expression test = value == "(Not answered)"
                ? Expression.OrElse(
                    Expression.OrElse(
                        Expression.Equal(field, Expression.Constant(null, typeof(string))),
                        Expression.Equal(trimmed, Expression.Constant(""))),
                    Expression.Equal(trimmed, Expression.Constant("-")))
                : Expression.Equal(trimmed, Expression.Constant(value));

            return source.Where(Expression.Lambda<Func<T, bool>>(test, selector.Parameters));
        }

        public static IQueryable<T> WherePeriod<T>(
            this IQueryable<T> query,
            Expression<Func<T, DateTime?>> date,
            ReportFilter f)
        {
            DateTime start = default(DateTime);
            DateTime endExclusive = default(DateTime);
            int month = 0;

            if (ReportHelpers.TryGetPeriod(f.Month, out start, out endExclusive))
                return WhereRange(query, date, start, endExclusive);

            if (ReportHelpers.TryGetMonthOnly(f.Month, out month))
                return WhereMonth(query, date, month);

            DateTime? from = f.From.HasValue ? f.From.Value.Date : (DateTime?)null;
            DateTime? toExclusive = f.To.HasValue ? f.To.Value.Date.AddDays(1) : (DateTime?)null;
            return WhereRange(query, date, from, toExclusive);
        }

        public static IQueryable<T> WhereNonZero<T>(
            this IQueryable<T> query,
            Expression<Func<T, decimal?>> amount,
            ReportFilter f)
        {
            var notNull = Expression.NotEqual(amount.Body, Expression.Constant(null, typeof(decimal?)));
            var notZero = Expression.NotEqual(amount.Body, Expression.Constant(0m, typeof(decimal?)));
            var body = Expression.AndAlso(notNull, notZero);
            return query.Where(Expression.Lambda<Func<T, bool>>(body, amount.Parameters));
        }

        public static Dictionary<string, decimal> SumFields<T>(this IQueryable<T> query, string fields)
        {
            var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var excludeFromTotals = typeof(T).GetProperty(
                nameof(LedgerDto.ExcludeFromTotals),
                BindingFlags.Public | BindingFlags.Instance);
            if (excludeFromTotals != null && excludeFromTotals.PropertyType == typeof(bool))
            {
                var row = Expression.Parameter(typeof(T), "row");
                var excluded = Expression.Property(row, excludeFromTotals);
                var included = Expression.Lambda<Func<T, bool>>(
                    Expression.Not(excluded),
                    row);
                query = query.Where(included);
            }

            var specs = (fields ?? "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var spec in specs)
            {
                var parts = spec.Split(':');
                var name = parts[0].Trim();
                var agg = parts.Length > 1 ? parts[1].Trim().ToLowerInvariant() : "sum";

                var property = typeof(T).GetProperty(
                    name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null) continue;

                var row = Expression.Parameter(typeof(T), "row");
                var member = Expression.Property(row, property);

                if (agg == "count")
                {
                    totals[property.Name] = CountNonNull(query, row, member, property.PropertyType);
                    continue;
                }

                if (!IsNumeric(property.PropertyType)) continue;
                var underlying = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if (underlying == typeof(decimal))
                {
                    var q = query.Select(Expression.Lambda<Func<T, decimal?>>(
                        Expression.Convert(member, typeof(decimal?)), row));
                    decimal? r = agg == "avg" ? q.Average()
                               : agg == "min" ? q.Min()
                               : agg == "max" ? q.Max()
                               : q.Sum();
                    totals[property.Name] = r ?? 0m;
                }
                else
                {
                    // cast to double? (EF supports it), convert the single result in memory
                    var q = query.Select(Expression.Lambda<Func<T, double?>>(
                        Expression.Convert(member, typeof(double?)), row));
                    double? r = agg == "avg" ? q.Average()
                              : agg == "min" ? q.Min()
                              : agg == "max" ? q.Max()
                              : q.Sum();
                    totals[property.Name] = Convert.ToDecimal(r ?? 0d);
                }
            }

            return totals;
        }

        private static decimal CountNonNull<T>(
            IQueryable<T> query, ParameterExpression row, MemberExpression member, Type type)
        {
            var nullable = !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
            if (!nullable) return query.Count();
            var test = Expression.NotEqual(member, Expression.Constant(null, type));
            return query.Count(Expression.Lambda<Func<T, bool>>(test, row));
        }

        private static bool IsNumeric(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            return NumericTypes.Contains(underlying);
        }

        private static IQueryable<T> WhereRange<T>(
            IQueryable<T> query,
            Expression<Func<T, DateTime?>> date,
            DateTime? from,
            DateTime? toExclusive)
        {
            Expression condition = null;

            if (from.HasValue)
            {
                condition = Expression.GreaterThanOrEqual(
                    date.Body,
                    Expression.Constant(from.Value, typeof(DateTime?)));
            }

            if (toExclusive.HasValue)
            {
                var upper = Expression.LessThan(
                    date.Body,
                    Expression.Constant(toExclusive.Value, typeof(DateTime?)));
                condition = condition == null ? upper : Expression.AndAlso(condition, upper);
            }

            if (condition == null) return query;
            return query.Where(Expression.Lambda<Func<T, bool>>(condition, date.Parameters));
        }

        private static IQueryable<T> WhereMonth<T>(
            IQueryable<T> query,
            Expression<Func<T, DateTime?>> date,
            int month)
        {
            var hasValue = Expression.Property(date.Body, "HasValue");
            var monthOf = Expression.Property(Expression.Property(date.Body, "Value"), "Month");
            var body = Expression.AndAlso(hasValue, Expression.Equal(monthOf, Expression.Constant(month)));
            return query.Where(Expression.Lambda<Func<T, bool>>(body, date.Parameters));
        }

        public static IQueryable<T> OrderByName<T>(this IQueryable<T> query, string name, bool desc)
        {
            if (string.IsNullOrWhiteSpace(name)) return query;

            var property = typeof(T).GetProperty(
                name.Trim(),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null) return query;

            var row = Expression.Parameter(typeof(T), "row");
            var lambda = Expression.Lambda(Expression.Property(row, property), row);
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == (desc ? "OrderByDescending" : "OrderBy")
                            && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.PropertyType);

            return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda });
        }

        public static IQueryable<T> WhereScope<T>(this IQueryable<T> query, ReportScope scope)
        {
            query = WhereIntProperty(query, "SystemID", scope.SystemId);
            return WhereIntProperty(query, "WarehouseID", scope.WarehouseId);
        }

        private static IQueryable<T> WhereIntProperty<T>(IQueryable<T> query, string name, int value)
        {
            var property = typeof(T).GetProperty(
                name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null) return query;

            var underlying = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (underlying != typeof(int)) return query;

            var row = Expression.Parameter(typeof(T), "row");
            var body = Expression.Equal(
                Expression.Property(row, property),
                Expression.Constant(value, property.PropertyType));
            return query.Where(Expression.Lambda<Func<T, bool>>(body, row));
        }
    }
}