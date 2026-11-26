using System;
using System.Collections.Generic;

namespace NSMSOLUTION.Models.EFCore
{
    /// <summary>
    /// Filter parameters for querying customer activation records
    /// </summary>
    public class ActivateCustomerFilterDTO
    {
        public string Search { get; set; }
        public string Cid { get; set; }
        public string CustomerName { get; set; }
        public string Sid { get; set; }
        public string Phone { get; set; }
        public string PhoneNumber { get; set; }
        public string Package { get; set; }
        public string PackageName { get; set; }
        public bool? IsDueOnly { get; set; }
        public bool? IsBlockedOnly { get; set; }
        public int? ConnectivityStatus { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Customer activation record with CID, CustomerName, PhoneNumber, SID, and Package details
    /// </summary>
    public class ActivateCustomerItemDTO
    {
        public int ActivateCustomerID { get; set; }
        public int CustomerID { get; set; }
        public string CID { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string SID { get; set; }
        public int? PackageDetailID { get; set; }
        public string PackageName { get; set; }
        public string Package { get; set; }
        public int? Bandwidth { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ConnectivityStatus { get; set; }
        public string StatusName { get; set; }
        public bool IsBlocked { get; set; }
    }

    /// <summary>
    /// Detailed customer activation information for verification and correction
    /// </summary>
    public class ActivateCustomerDetailDTO : ActivateCustomerItemDTO
    {
        public string PPPoEUserName { get; set; }
        public string ConnectionType { get; set; }
        public string IPAddress { get; set; }
        public string VLanName { get; set; }
        public int? RouterID { get; set; }
        public DateTime? ActivateDate { get; set; }
    }

    /// <summary>
    /// Request payload for blocking or unblocking customer(s) (supports single & bulk)
    /// </summary>
    public class BlockCustomerRequestDTO
    {
        public int? ActivateCustomerID { get; set; }
        public List<int> ActivateCustomerIDs { get; set; }
        public bool Block { get; set; } = true; // true = Block (41), false = Unblock (40)
        public string Reason { get; set; }
    }

    /// <summary>
    /// Quick summary statistics for activated / due / blocked customers
    /// </summary>
    public class CustomerActivateSummaryDTO
    {
        public int TotalActive { get; set; }
        public int TotalBlocked { get; set; }
        public int TotalDue { get; set; }
        public int TotalRecords { get; set; }
    }

    /// <summary>
    /// Filter parameters for querying Request Approval Item Out
    /// </summary>
    public class RequestApprovalItemOutFilterDTO
    {
        public int? RequestItemID { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string Search { get; set; }
        public string RequestApprovalNo { get; set; }
        public string RequestNo { get; set; }
        public string RequestName { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Item Request Approval Out record containing both raw/clean and formatted fields
    /// </summary>
    public class RequestApprovalItemOutDTO
    {
        public long? No { get; set; }
        public int RequestItemID { get; set; }
        public string RequestApprovalNo { get; set; }
        public string RawRequestApprovalNo { get; set; }
        public string RequestNo { get; set; }
        public string Department { get; set; }
        public string RequestDate { get; set; }
        public DateTime? RequestDateRaw { get; set; }
        public string RequestName { get; set; }
        public string NeedToApproval { get; set; }
        public string Status { get; set; }
        public string StatusClean { get; set; }
        public string CStatus { get; set; }
        public string PreparedBy { get; set; }
        public string RequestApprovalBy { get; set; }
        public string PreparedDate { get; set; }
        public DateTime? PreparedDateRaw { get; set; }
        public string ApproveOrDisapprove { get; set; }
        public string CreateBy { get; set; }
        public bool? IsConfirmPrepared { get; set; }
        public string WaitingConfirmedReceivedStatus { get; set; }
        public bool? IsWaitingConfirmedReceived { get; set; }
        public bool? IsReceived { get; set; }
        public int? RequestNameUserID { get; set; }
        public string RequestNameTelegram { get; set; }
        public string ApproveDisapproveDate { get; set; }
        public DateTime? ApproveDisapproveDateRaw { get; set; }
        public string RequestMode { get; set; }
    }

    /// <summary>
    /// Filter parameters for querying Technical received items.
    /// Supports: srNo, itemCode, itemName, snNumber
    /// </summary>
    public class ReceivedItemsFilterDTO
    {
        public string SRNo { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string SNNumber { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Received item detail matching: SR No, Department, Name, Address, Issue Date, Item Code, Item Name, SN Number, QTY, Project, Block, Purpose or Reasons
    /// </summary>
    public class ReceivedItemDetailDTO
    {
        public long? No { get; set; }
        public int RequestItemDetailID { get; set; }
        public string SRNo { get; set; }
        public string Department { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string IssueDate { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string SNNumber { get; set; }
        public decimal? QTY { get; set; }
        public string Unit { get; set; }
        public string Project { get; set; }
        public string Block { get; set; }
        public string PurposeOrReasons { get; set; }
    }
}
