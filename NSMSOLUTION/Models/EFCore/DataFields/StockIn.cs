namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class StockIn
    {
        public int StockInID { get; set; }
        public int? BillID { get; set; }
        public string ReferenceNo { get; set; }
        public string BillNumber { get; set; }
        public System.DateTime? BillDate { get; set; }
        public string PONumber { get; set; }
        public System.DateTime? PODate { get; set; }
        public int? PurchaseOrderID { get; set; }
        public string VendorName { get; set; }
        public int? VendorID { get; set; }
        public string VendorDONumber { get; set; }
        public string VendorInvoiceNumber { get; set; }
        public string ReceiveNoteNumber { get; set; }
        public System.DateTime? ReceiveDate { get; set; }
        public int? ClassID { get; set; }
        public string ClassName { get; set; }
        public int? WID { get; set; }
        public string Warehouse { get; set; }
        public int? FromWarehouseID { get; set; }
        public string FromWarehouse { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string CustomerNotes { get; set; }
        public string TermsConditions { get; set; }
        public int? ChartOfAccountID { get; set; }
        public string AccountName { get; set; }
        public int? ReasonID { get; set; }
        public string Description { get; set; }
        public int? StatusID { get; set; }
        public string ItemType { get; set; }
        public string IssuedBy { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateDate { get; set; }
        public string StockTransfersNumber { get; set; }
        public int? CheckVerifyByUserID { get; set; }
        public int? IssuedByUserID { get; set; }
        public int? ApproveByUserID { get; set; }
        public int? DisapproveByUserID { get; set; }
        public int? UserID { get; set; }
        public string Reference_VendorName { get; set; }
        public int? ItemOpeningStockID { get; set; }
        public string Type { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
        public double? TotalOrderQty { get; set; }
        public double? TotalReceivedQty { get; set; }
        public int? PreparedUserID { get; set; }
        public string PreparedBy { get; set; }
        public int? DeliveredByID { get; set; }
        public string DeliveredBy { get; set; }
        public int? ReceivedByID { get; set; }
        public string ReceivedBy { get; set; }
        public System.DateTime? DeliveredDate { get; set; }
        public string PreparedPhone { get; set; }
        public string CheckBy { get; set; }
        public int? CheckByID { get; set; }
    }
}