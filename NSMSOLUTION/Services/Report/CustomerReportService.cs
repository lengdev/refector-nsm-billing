using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Data.Entity;
using System.Linq;

namespace AllReportAPi.Services
{
    public interface ICustomerReportService
    {
        IQueryable<CustomerListDto> CustomerList(ReportFilter filter);
        IQueryable<CustomerByBlockDto> CustomerByBlock(ReportFilter filter);
        IQueryable<CustomerByBlockSummaryDto> CustomerByBlockSummary(ReportFilter filter);
        IQueryable<ConnectionSummaryDto> ConnectionSummary(ReportFilter filter);
        IQueryable<CustomerByConnectionDto> CustomerByConnection(ReportFilter filter);
        IQueryable<CustomerFeedbackDto> CustomerFeedback(ReportFilter filter);
        IQueryable<FeedbackAnalysisDto> CustomerFeedbackAnalysis(ReportFilter filter);
    }

    public class CustomerReportService : ICustomerReportService
    {
        private static readonly string[] NoComplaint =
            { "-", "no", "none", "n/a", "na", "nil", "no complaint" };

        private readonly DBContext db;
        private readonly ReportScope scope;

        public CustomerReportService(DBContext db, ReportScope scope)
        {
            this.db = db;
            this.scope = scope;
        }

        public IQueryable<CustomerListDto> CustomerList(ReportFilter filter)
        {
            var firstBillIds =
                db.TblCustomerBillInformation.AsNoTracking()
                    .GroupBy(x => x.CustomerID)
                    .Select(g => g.Min(x => x.CustomerBillInformationID));

            var source =
                from c in db.TblCustomer.AsNoTracking().WhereScope(scope)
                join bi in db.TblCustomerBillInformation.AsNoTracking()
                        .Where(x => firstBillIds.Contains(x.CustomerBillInformationID))
                    on (int?)c.CustomerID equals bi.CustomerID into billInfo
                from bi in billInfo.DefaultIfEmpty()
                join b in db.TblCustomerBlock.AsNoTracking()
                    on c.CustomerTypeID equals (int?)b.CustomerTypeID into blocks
                from b in blocks.DefaultIfEmpty()
                select new
                {
                    Customer = c,
                    Block = b.CustomerType,
                    bi.GoogleMap_Latitude,
                    bi.GoogleMap_Longitude,
                    bi.GoogleMap_FullAddress
                };

            return source
                .WhereEquals(r => r.Customer.CustomerID, filter.CustomerID)
                .WhereEquals(r => r.Customer.ClassName, filter.Class)
                .WhereEquals(r => r.Block, filter.Block)
                .WherePeriod(r => (DateTime?)r.Customer.RegisterDate, filter)
                .OrderByDescending(r => r.Customer.RegisterDate)
                .Select(r => new CustomerListDto
                {
                    CustomerID = r.Customer.CustomerID,
                    RegisterDate = r.Customer.RegisterDate,
                    CustomerCode = r.Customer.CustomerCode,
                    CustomerName = r.Customer.CustomerName,
                    CustomerNameLatin = r.Customer.CustomerNameLatin,
                    Phone = r.Customer.CustomerMobile,
                    Class = r.Customer.ClassName,
                    Status = r.Customer.CustomerStatus,
                    Block = r.Block,
                    Address = r.Customer.CustomerAddress,
                    Commune = r.Customer.CustomerCommune,
                    District = r.Customer.CustomerDistrict,
                    Province = r.Customer.CustomerProvinceCity,
                    Lat = r.GoogleMap_Latitude,
                    Lng = r.GoogleMap_Longitude,
                    MapAddress = r.GoogleMap_FullAddress,
                });
        }

        public IQueryable<CustomerByBlockDto> CustomerByBlock(ReportFilter filter)
        {
            filter.SortBy = null;

            var lastBillIds =
                db.TblCustomerBillInformation.AsNoTracking()
                    .WhereScope(scope)
                    .GroupBy(x => x.CustomerID)
                    .Select(g => g.Max(x => x.CustomerBillInformationID));

            var source =
                from ac in db.TblActivateCustomer.AsNoTracking()
                join c in db.TblCustomer.AsNoTracking().WhereScope(scope)
                    on (int?)ac.CustomerID equals (int?)c.CustomerID
                join b in db.TblCustomerBlock.AsNoTracking()
                    on c.CustomerTypeID equals (int?)b.CustomerTypeID
                join bi in db.TblCustomerBillInformation.AsNoTracking()
                        .Where(x => lastBillIds.Contains(x.CustomerBillInformationID))
                    on (int?)c.CustomerID equals bi.CustomerID into billInfo
                from bi in billInfo.DefaultIfEmpty()
                select new
                {
                    Customer = c,
                    Connection = ac,
                    Block = b.CustomerType,
                    bi.ClassName,
                    CustomerName = string.IsNullOrEmpty(c.CustomerName)
                        ? c.CustomerNameLatin : c.CustomerName,
                    Status = ac.ConnectivityStatus == 40 ? "Active"
                        : ac.ConnectivityStatus == 41 ? "Locked"
                        : ac.ConnectivityStatus == 42 ? "Terminate"
                        : "Inactive",
                };

            var status = (filter.ServiceStatus ?? "").Trim();

            var q = source
                .WhereEquals(r => r.Customer.CustomerID, filter.CustomerID)
                .WhereEquals(r => r.ClassName, filter.Class)
                .WhereEquals(r => r.Block, filter.Block)
                .WherePeriod(r => (DateTime?)r.Customer.RegisterDate, filter);

            if (status == "Active") q = q.Where(r => r.Connection.ConnectivityStatus == 40);
            else if (status == "Locked") q = q.Where(r => r.Connection.ConnectivityStatus == 41);
            else if (status == "Terminate") q = q.Where(r => r.Connection.ConnectivityStatus == 42);
            else if (status == "Inactive") q = q.Where(r => r.Connection.ConnectivityStatus != 40
                                                          && r.Connection.ConnectivityStatus != 41
                                                          && r.Connection.ConnectivityStatus != 42);

            return q
                .OrderBy(r => r.Block)
                .ThenBy(r => r.CustomerName)
                .ThenBy(r => r.Customer.CustomerID)
                .ThenBy(r => r.Connection.SID)
                .Select(r => new CustomerByBlockDto
                {
                    Block = r.Block,
                    CustomerID = r.Customer.CustomerID,
                    RegisterDate = r.Customer.RegisterDate,
                    CustomerCode = r.Customer.CustomerCode,
                    CustomerName = r.CustomerName,
                    Phone = r.Customer.CustomerMobile,
                    Class = r.ClassName,
                    SID = r.Connection.SID,
                    Type = r.Connection.ConnectionType,
                    Status = r.Status,
                });
        }

        public IQueryable<CustomerByBlockSummaryDto> CustomerByBlockSummary(ReportFilter filter)
        {
            filter.SortBy = null;

            var customers =
                from c in db.TblCustomer.AsNoTracking().WhereScope(scope)
                join b in db.TblCustomerBlock.AsNoTracking()
                    on c.CustomerTypeID equals (int?)b.CustomerTypeID
                select new { c.CustomerID, c.RegisterDate, Block = b.CustomerType };

            customers = customers
                .WhereEquals(r => r.Block, filter.Block)
                .WherePeriod(r => (DateTime?)r.RegisterDate, filter);

            var customerCounts = customers
                .GroupBy(r => r.Block ?? "(No block)")
                .Select(g => new { Block = g.Key, Customers = g.Count() })
                .ToList();

            var connectionCounts =
                (from r in customers
                 join ac in db.TblActivateCustomer.AsNoTracking()
                    on (int?)r.CustomerID equals (int?)ac.CustomerID
                 group ac by (r.Block ?? "(No block)") into g
                 select new
                 {
                     Block = g.Key,
                     Connections = g.Count(),
                     Active = g.Count(x => x.ConnectivityStatus == 40)
                 }).ToList();

            return customerCounts
                .Select(cc =>
                {
                    var cn = connectionCounts.FirstOrDefault(x => x.Block == cc.Block);
                    return new CustomerByBlockSummaryDto
                    {
                        Block = cc.Block,
                        Customers = cc.Customers,
                        Connections = cn == null ? 0 : cn.Connections,
                        ActiveConnections = cn == null ? 0 : cn.Active
                    };
                })
                .OrderBy(x => x.Block)
                .AsQueryable();
        }

        public IQueryable<ConnectionSummaryDto> ConnectionSummary(ReportFilter filter)
        {
            var source =
                from c in db.TblCustomer.AsNoTracking().WhereScope(scope)
                join ac in db.TblActivateCustomer.AsNoTracking()
                    on (int?)c.CustomerID equals (int?)ac.CustomerID
                join b in db.TblCustomerBlock.AsNoTracking()
                    on c.CustomerTypeID equals (int?)b.CustomerTypeID
                select new
                {
                    CustomerID = (int?)ac.CustomerID,
                    ac.SID,
                    ConnType = ac.ConnectionType,
                    Code = ac.ConnectivityStatus,
                    Block = b.CustomerType
                };

            return source
                .GroupBy(r => r.ConnType ?? "(No type)")
                .Select(g => new ConnectionSummaryDto
                {
                    Type = g.Key,
                    Active = g.Count(x => x.Code == 40),
                    Locked = g.Count(x => x.Code == 41),
                    Terminate = g.Count(x => x.Code == 42),
                    Inactive = g.Count(x => x.Code != 40 && x.Code != 41 && x.Code != 42),
                    Total = g.Count()
                })
                .OrderBy(x => x.Type);
        }

        public IQueryable<CustomerByConnectionDto> CustomerByConnection(ReportFilter filter)
        {
            var today = DateTime.Today;
            var lastBillingIds =
                db.TblCustomerBillingInvoice.AsNoTracking()
                    .WhereScope(scope)
                    .GroupBy(x => new { x.CustomerID, x.SID })
                    .Select(g => g.Max(x => x.BillingInvoiceID));

            var lastHistoryIds =
                db.TblHistoryCustomerActivate.AsNoTracking()
                    .GroupBy(x => x.ActivateCustomerID)
                    .Select(g => g.Max(x => x.HistoryCustomerActivateID));

            var source =
                from c in db.TblCustomer.AsNoTracking().WhereScope(scope)
                join ac in db.TblActivateCustomer.AsNoTracking()
                    on (int?)c.CustomerID equals (int?)ac.CustomerID
                join ah in db.TblHistoryCustomerActivate.AsNoTracking()
                        .Where(a => lastHistoryIds.Contains(a.HistoryCustomerActivateID))
                    on new { CustomerID = (int?)ac.CustomerID, ActivateCustomerID = (int?)ac.ActivateCustomerID }
                    equals new { CustomerID = (int?)ah.CustomerID, ActivateCustomerID = (int?)ah.ActivateCustomerID }
                    into historyActivate
                from ah in historyActivate.DefaultIfEmpty()
                join bi in db.TblCustomerBillingInvoice.AsNoTracking()
                        .Where(x => lastBillingIds.Contains(x.BillingInvoiceID))
                    on new { CustomerID = (int?)ac.CustomerID, ac.SID }
                    equals new { CustomerID = (int?)bi.CustomerID, bi.SID } into billing
                from bi in billing.DefaultIfEmpty()
                join b in db.TblCustomerBlock.AsNoTracking()
                    on c.CustomerTypeID equals (int?)b.CustomerTypeID
                select new
                {
                    Customer = c,
                    Connection = ac,
                    bi.ClassName,
                    ah.Comment,
                    b.CustomerType,
                    CustomerName = string.IsNullOrEmpty(c.CustomerName)
                        ? c.CustomerNameLatin : c.CustomerName,
                    Status = ac.ConnectivityStatus == 40 ? "Active"
                        : ac.ConnectivityStatus == 41 ? "Locked"
                        : ac.ConnectivityStatus == 42 ? "Terminate"
                        : "Inactive",
                    bi.StartDate,
                    bi.EndDate,
                };

            var status = (filter.ServiceStatus ?? "").Trim();

            var q = source
                .WhereEquals(r => r.Customer.CustomerID, filter.CustomerID)
                .WhereEquals(r => r.Connection.SID, filter.SID)
                .WhereEquals(r => r.Connection.ConnectionType, filter.ConnectionType)
                .WhereEquals(r => r.CustomerType, filter.Block)
                .WherePeriod(r => (DateTime?)r.EndDate, filter);

            if (status == "Active") q = q.Where(r => r.Connection.ConnectivityStatus == 40);
            else if (status == "Locked") q = q.Where(r => r.Connection.ConnectivityStatus == 41);
            else if (status == "Terminate") q = q.Where(r => r.Connection.ConnectivityStatus == 42);
            else if (status == "Inactive") q = q.Where(r => r.Connection.ConnectivityStatus != 40
                                                          && r.Connection.ConnectivityStatus != 41
                                                          && r.Connection.ConnectivityStatus != 42);

            return q
                .OrderBy(r => r.CustomerName)
                .ThenBy(r => r.Customer.CustomerID)
                .ThenBy(r => r.Connection.SID)
                .Select(r => new CustomerByConnectionDto
                {
                    CustomerID = r.Customer.CustomerID,
                    CustomerCode = r.Customer.CustomerCode,
                    CustomerName = r.CustomerName,
                    SID = r.Connection.SID,
                    ConnectionID = r.Connection.ConnectionID,
                    Type = r.Connection.ConnectionType,
                    Status = r.Status,
                    Block = r.CustomerType,
                    Class = r.ClassName,
                    Vlan = r.Connection.ConnectionType == "Static"
                        ? r.Connection.VLanName : "",
                    PPPoE = r.Connection.ConnectionType == "PPPoE"
                        ? r.Connection.PPPoEUserName : "",
                    ActivateDate = r.Connection.ActivateDate,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    DaysLeft = r.EndDate == null
                        ? (int?)null
                        : DbFunctions.DiffDays(today, r.EndDate),
                    Remark = r.Comment
                });
        }

        public IQueryable<CustomerFeedbackDto> CustomerFeedback(ReportFilter filter)
        {
            var source =
                from x in db.TblCustomerFeedback.AsNoTracking()
                    .WhereScope(scope)
                    .Where(x => x.IsDelete != true)
                join c in db.TblCustomer.AsNoTracking()
                    on x.CustomerID equals (int?)c.CustomerID into cs
                from c in cs.DefaultIfEmpty()
                select new
                {
                    Feedback = x,
                    Customer = c,
                    CustomerName = string.IsNullOrEmpty(c.CustomerName)
                        ? c.CustomerNameLatin : c.CustomerName
                };

            var q = source
                .WhereEquals(r => r.Customer.CustomerID, filter.CustomerID)
                .WherePeriod(r => r.Feedback.DateFeedback, filter)
                .WhereAnswer(r => r.Feedback.InternetSpeed, filter.InternetSpeed)
                .WhereAnswer(r => r.Feedback.CustomerServices, filter.CustomerServices);

            var complaint = (filter.Complaint ?? "").Trim();
            if (complaint == "Complaint")
                q = q.Where(r => r.Feedback.Complaint != null && r.Feedback.Complaint.Trim() == "Complaint");
            else if (complaint == "No Complaint")
                q = q.Where(r => r.Feedback.Complaint == null || r.Feedback.Complaint.Trim() != "Complaint");

            return q
                .OrderBy(r => r.Feedback.DateFeedback)
                .ThenBy(r => r.Feedback.FeedbackNo)
                .Select(r => new CustomerFeedbackDto
                {
                    CustomerID = r.Feedback.CustomerID,
                    CustomerName = r.CustomerName,
                    CustomerPhone = r.Customer.CustomerMobile,
                    InternetSpeed = r.Feedback.InternetSpeed,
                    Pricing = r.Feedback.Pricing,
                    CustomerServices = r.Feedback.CustomerServices,
                    Ratingus = r.Feedback.Ratingus,
                    CustomerComment = r.Feedback.CustomerComment,
                    Complaint = r.Feedback.Complaint,
                    ContactBy = r.Feedback.ContactBy,
                    CreateBy = r.Feedback.CreateBy,
                    CreateDate = r.Feedback.CreateDate
                });
        }

        // ───────────── feedback analysis ─────────────
        private static string RatingOf(string v)
        {
            v = (v ?? "").Trim();
            return v == "" || v == "-" ? "(Not answered)" : v;
        }

        private static IQueryable<FeedbackAnalysisDto> AnalyzeTopic(
            IQueryable<FeedbackRow> rows, string topic, int total)
        {
            return rows.GroupBy(r => r.Rating).Select(g => new FeedbackAnalysisDto
            {
                Topic = topic,
                Rating = g.Key,
                Feedbacks = g.Count(),
                SharePct = g.Count() * 100m / total,
                Complaints = g.Count(r => r.Complained),
                ComplaintPct = g.Count(r => r.Complained) * 100m / g.Count(),
            });
        }

        public IQueryable<FeedbackAnalysisDto> CustomerFeedbackAnalysis(ReportFilter filter)
        {
            var data = db.TblCustomerFeedback.AsNoTracking()
                .WhereScope(scope)
                .Where(x => x.IsDelete != true)
                .WhereEquals(x => x.CustomerID, filter.CustomerID)
                .WherePeriod(x => x.DateFeedback, filter)
                .Select(x => new { x.InternetSpeed, x.CustomerServices, x.Complaint })
                .ToList();                                   // the only database call

            var noComplaint = NoComplaint;
            Func<string, bool> complained = c =>
            {
                c = (c ?? "").Trim();
                return c != "" && !noComplaint.Contains(c.ToLower());
            };

            var speed = data.Select(x => new FeedbackRow
            { Rating = RatingOf(x.InternetSpeed), Complained = complained(x.Complaint) })
                .AsQueryable();

            var service = data.Select(x => new FeedbackRow
            { Rating = RatingOf(x.CustomerServices), Complained = complained(x.Complaint) })
                .AsQueryable();

            var count = data.Count;
            var complaints = data.Count(x => complained(x.Complaint));
            var total = Math.Max(count, 1);

            var overall = (count == 0
                ? new FeedbackAnalysisDto[0]
                : new[]
                {
                    new FeedbackAnalysisDto
                    {
                        Topic = "Overall",
                        Rating = "All feedback",
                        Feedbacks = count,
                        SharePct = 100m,
                        Complaints = complaints,
                        ComplaintPct = complaints * 100m / count
                    }
                }).AsQueryable();

            return AnalyzeTopic(speed, "Internet Speed", total)
                .Concat(AnalyzeTopic(service, "Customer Services", total))
                .Concat(overall)
                .OrderByDescending(d => d.Topic == "Overall")
                .ThenBy(d => d.Topic)
                .ThenByDescending(d => d.Feedbacks);
        }
    }
}