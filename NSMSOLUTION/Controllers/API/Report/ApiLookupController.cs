using AllReportAPi.Services;
using System.Linq;
using System.Web.Http;

namespace AllReportAPi.Controllers
{
    [RoutePrefix("api/AllReport")]
    public class ApiLookupController : ReportApiControllerBase
    {
        [HttpGet, Route("get-products")]
        public IHttpActionResult GetProducts() =>
            RunService<ProductLookupDto>(null, (s, f) => s.Lookup.Products());

        [HttpGet, Route("get-item-groups")]
        public IHttpActionResult GetItemGroups() =>
            RunService<string>(null, (s, f) => s.Lookup.ItemGroups());

        [HttpGet, Route("get-categories")]
        public IHttpActionResult GetCategories() =>
            RunService<string>(null, (s, f) => s.Lookup.Categories());

        [HttpGet, Route("get-suppliers")]
        public IHttpActionResult GetSuppliers() =>
            RunService<string>(null, (s, f) => s.Lookup.Suppliers());

        [HttpGet, Route("get-warehouses")]
        public IHttpActionResult GetWarehouses() =>
            RunService<string>(null, (s, f) => s.Lookup.Warehouses());

        [HttpGet, Route("get-po-number")]
        public IHttpActionResult GetPONumber() =>
            RunService<string>(null, (s, f) => s.Lookup.PONumbers());

        [HttpGet, Route("get-classes")]
        public IHttpActionResult GetClasses() =>
            RunService<string>(null, (s, f) => s.Lookup.Classes());

        [HttpGet, Route("get-customers")]
        public IHttpActionResult GetCustomers() =>
            RunService<CustomerLookupDto>(null, (s, f) => s.Lookup.Customers());

        [HttpGet, Route("get-customer-block")]
        public IHttpActionResult GetCustomerBlock() =>
            RunService<string>(null, (s, f) => s.Lookup.CustomerBlocks());

        [HttpGet, Route("get-sid")]
        public IHttpActionResult GetSID() =>
            RunService<string>(null, (s, f) => s.Lookup.SIDs());

        [HttpGet, Route("get-connection-type")]
        public IHttpActionResult GetConnectionType() =>
            RunService<string>(null, (s, f) => s.Lookup.ConnectionTypes());

        [HttpGet, Route("get-payee")]
        public IHttpActionResult GetPayee() =>
            RunService<string>(null, (s, f) => s.Lookup.Payees());

        [HttpGet, Route("get-account")]
        public IHttpActionResult GetAccount() =>
            RunService<string>(null, (s, f) => s.Lookup.Account());

        [HttpGet, Route("get-sort-option")]
        public IHttpActionResult GetSortOption() =>
            RunService<string>(null, (s, f) => s.Lookup.SortOption().AsQueryable());

        [HttpGet, Route("get-transaction-type")]
        public IHttpActionResult GetTransactionType() =>
            RunService<string>(null, (s, f) => s.Lookup.TransactionType().AsQueryable());

        [HttpGet, Route("get-company-name")]
        public IHttpActionResult GetCompanyName(string className) =>
            RunService<CompanyLookupDto>(null, (s, f) => s.Lookup.GetCompanyName(className));
    }
}