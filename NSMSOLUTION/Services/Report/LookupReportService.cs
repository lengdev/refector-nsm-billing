using NSMSOLUTION.Models.EFCore;
using System.Collections.Generic;
using System.Linq;

namespace AllReportAPi.Services
{
    // named types replace the anonymous objects so the service can be used from other code
    public class ProductLookupDto
    {
        public int ItemID { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
    }

    public class CustomerLookupDto
    {
        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }

    public class CompanyLookupDto
    {
        public string CompanyName { get; set; }
        public string CompanyNameKH { get; set; }
    }

    public interface ILookupService
    {
        IQueryable<ProductLookupDto> Products();
        IQueryable<string> ItemGroups();
        IQueryable<string> Categories();
        IQueryable<string> Suppliers();
        IQueryable<string> Warehouses();
        IQueryable<string> PONumbers();
        IQueryable<string> Classes();
        IQueryable<CustomerLookupDto> Customers();
        IQueryable<string> CustomerBlocks();
        IQueryable<string> SIDs();
        IQueryable<string> ConnectionTypes();
        IQueryable<string> Payees();
        IQueryable<string> Account();
        IQueryable<string> SortOption();
        IQueryable<string> TransactionType();
        IQueryable<CompanyLookupDto> GetCompanyName(string className);
    }

    public class LookupService : ILookupService
    {
        private readonly DBContext db;
        private readonly ReportScope scope;

        public LookupService(DBContext db, ReportScope scope)
        {
            this.db = db;
            this.scope = scope;
        }

        public IQueryable<ProductLookupDto> Products() =>
            db.TblItem.AsNoTracking()
                .WhereScope(scope)
                .Where(i => i.ItemName != null && i.ItemName != "")
                .Select(c => new ProductLookupDto
                {
                    ItemID = c.ItemID,
                    ItemCode = c.ItemCode,
                    ItemName = c.ItemName
                })
                .Distinct()
                .OrderByDescending(c => c.ItemID);

        public IQueryable<string> ItemGroups() =>
            db.TblItemGroup.AsNoTracking()
                .WhereScope(scope)
                .Where(i => i.GroupName != null && i.GroupName != "")
                .Select(i => i.GroupName)
                .Distinct()
                .OrderByDescending(name => name);

        public IQueryable<string> Categories() =>
            db.TblCategory.AsNoTracking()
                .WhereScope(scope)
                .Select(i => i.StockName)
                .Distinct()
                .OrderByDescending(name => name);

        public IQueryable<string> Suppliers() =>
            db.TblVendor.AsNoTracking()
                .WhereScope(scope)
                .Where(c => c.VendorName != null && c.VendorName != "")
                .Select(c => c.VendorName)
                .Distinct()
                .OrderBy(name => name);

        public IQueryable<string> Warehouses() =>
            db.TblWarehouse.AsNoTracking()
                .WhereScope(scope)
                .Where(c => c.WarehouseName != null && c.WarehouseName != "")
                .Select(c => c.WarehouseName)
                .Distinct()
                .OrderBy(name => name);

        public IQueryable<string> PONumbers() =>
            db.TblPurchaseOrder.AsNoTracking()
                .WhereScope(scope)
                .Where(c => c.PONumber != null && c.PONumber != "")
                .Select(c => c.PONumber)
                .Distinct()
                .OrderBy(name => name);

        public IQueryable<string> Classes() =>
            db.TblClass.AsNoTracking()
                .WhereScope(scope)
                .Where(c => c.ClassName != null && c.ClassName != "")
                .Select(c => c.ClassName)
                .Distinct()
                .OrderBy(name => name);

        public IQueryable<CustomerLookupDto> Customers() =>
            db.TblCustomer.AsNoTracking()
                .WhereScope(scope)
                .Where(c => c.CustomerCode != null
                    && c.CustomerCode != ""
                    && c.CustomerStatus != "Disable")
                .Select(c => new CustomerLookupDto
                {
                    CustomerID = c.CustomerID,
                    CustomerCode = c.CustomerCode,
                    CustomerName = string.IsNullOrEmpty(c.CustomerName)
                        ? c.CustomerNameLatin : c.CustomerName
                })
                .Distinct()
                .OrderByDescending(c => c.CustomerID);

        public IQueryable<string> CustomerBlocks() =>
            db.TblCustomerBlock.AsNoTracking()
                .WhereScope(scope)
                .Select(c => c.CustomerType)
                .Distinct()
                .OrderBy(name => name);

        public IQueryable<string> SIDs() =>
            db.TblActivateCustomer.AsNoTracking()
                .WhereScope(scope)
                .Select(c => c.SID)
                .Distinct()
                .OrderBy(name => name);

        public IQueryable<string> ConnectionTypes() =>
            db.TblActivateCustomer.AsNoTracking()
                .WhereScope(scope)
                .Select(c => c.ConnectionType)
                .Distinct()
                .OrderBy(name => name);

        public IQueryable<string> Account() =>
           db.TblChartOfAccount.AsNoTracking()
               .WhereScope(scope)
               .Select(coa => coa.AccountNo + "-" + coa.AccountName)
               .Distinct()
               .OrderBy(name => name);

        public IQueryable<string> SortOption() =>
     new[] { "Number", "Date", "Vendor Name" }.AsQueryable();
        public IQueryable<string> Payees()
        {
            var expsense = db.TblExpense.AsNoTracking().WhereScope(scope);

            var source =
                from expense in expsense
                join vendor in db.TblVendor.AsNoTracking()
                    on new { VendorID = expense.PayeeID, PayeeType = expense.PayeeType }
                    equals new { VendorID = (int?)vendor.VendorID, PayeeType = "Vendor" }
                    into vendors
                from v in vendors.DefaultIfEmpty()
                join vendor in db.TblEmployee.AsNoTracking()
                    on new { VendorID = expense.PayeeID, PayeeType = expense.PayeeType }
                    equals new { VendorID = (int?)vendor.LeadID, PayeeType = "Employee" }
                    into employees
                from e in employees.DefaultIfEmpty()
                select new
                {
                    Payee = expense.PayeeType == "Vendor"
                    ? v.VendorName : e.FullName,
                };

            return source
                .Where(row => row.Payee != null)
                .Select(row => row.Payee)
                .Distinct()
                .OrderBy(name => name);
        }

        public IQueryable<string> TransactionType() =>
            db.TblAccountTransaction
                // .WhereScope(scope)
                .Select(t => t.Type)
                .Distinct()
                .OrderBy(name => name);

        public IQueryable<CompanyLookupDto> GetCompanyName(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                var defaultClassId = GetDefaultClassIdForSystem(scope.SystemId);
                var classes = defaultClassId > 0
                    ? db.TblClass.AsNoTracking()
                        .Where(company => company.ClassID == defaultClassId)
                    : db.TblClass.AsNoTracking().WhereScope(scope);

                return classes
                    .Where(company =>
                        (company.FullName != null && company.FullName != "")
                        || (company.KHName != null && company.KHName != ""))
                    .OrderByDescending(company => company.StatusID == 40)
                    .ThenBy(company => company.ClassID)
                    .Select(company => new CompanyLookupDto
                    {
                        CompanyName = company.FullName,
                        CompanyNameKH = company.KHName
                    })
                    .Take(1);
            }

            var selectedClass = className.Trim();
            return db.TblClass.AsNoTracking()
                .WhereScope(scope)
                .Where(company =>
                    company.ClassName == selectedClass
                    && ((company.FullName != null && company.FullName != "")
                        || (company.KHName != null && company.KHName != "")))
                .OrderByDescending(company => company.ClassID)
                .ThenBy(company => company.FullName)
                .Select(company => new CompanyLookupDto
                {
                    CompanyName = company.FullName,
                    CompanyNameKH = company.KHName
                })
                .Take(1);
        }

        private static int GetDefaultClassIdForSystem(int systemId)
        {
            switch (systemId)
            {
                case 13: return 5;
                case 28: return 14;
                case 25: return 10;
                case 27: return 12;
                default: return 0;
            }
        }
    }
}