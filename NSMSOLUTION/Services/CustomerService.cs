using AllReportAPi.Controllers;
using MVC;
using NSMSOLUTION.Controllers.API;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace NSMSOLUTION.Services
{
    public class CustomerService
    {
        public static async Task<Customer> GetCustomerByIdAsync(int id)
        {
            try
            {
                using (var db = new DBContext())
                {
                    return await db.TblCustomer.FindAsync(id);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving customer by ID: " + ex.Message, ex);
            }
        }

        private static bool ContainsKhmer(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return text.Any(c => c >= '\u1780' && c <= '\u17FF');
        }

        public static string GetCustomerNamePrefix(int customerId)
        {
            using (var ctx = new DBContext())
            {
                var customer = ctx.TblCustomer.Find(customerId);

                if (customer == null)
                    return "";

                string name = !string.IsNullOrWhiteSpace(customer.CustomerName)
                    ? customer.CustomerName
                    : customer.CustomerNameLatin ?? "";

                bool isKhmer = ContainsKhmer(name);

                string prefix = isKhmer
                    ? customer.CustomerGender == "Female" ? "អ្នកស្រី "
                    : customer.CustomerGender == "Male" ? "លោក "
                    : ""
                    : customer.CustomerGender == "Female" ? "Ms. "
                    : customer.CustomerGender == "Male" ? "Mr. "
                    : "";

                return prefix + name;
            }
        }

        public async Task<List<ActivateCustomer>> GetConnectionAsync(int customerId)
        {
            using (var ctx = new DBContext())
            {
                return await ctx.TblActivateCustomer
                    .AsNoTracking()
                    .Where(c => c.CustomerID == customerId)
                    .ToListAsync();
            }
        }

        public static async Task<int> RegisterCustomerAsync(CustomerRegisterRequest req)
        {
            if (req?.Customer == null)
                throw new ArgumentException("Customer information is required.");

            using (var ctx = new DBContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                try
                {
                    var c = req.Customer;
                    var b = req.BillInfo;

                    var customer = await ctx.TblCustomer
                        .FirstOrDefaultAsync(x => x.CustomerID == c.CustomerID);

                    bool isNew = customer == null;

                    if (isNew)
                    {
                        customer = new Customer
                        {
                            RegisterDate = DateTime.Now,
                            RegisterBy = c.RegisterBy,
                            RegisterByUserLoginID = c.RegisterByUserLoginID,
                            CustomerCode = c.CustomerCode,
                            StatusID = 1,
                            DomainID = c.DomainID,
                            SystemID = c.SystemID,
                            WarehouseID = c.WarehouseID
                        };

                        ctx.TblCustomer.Add(customer);
                    }
                    else
                    {
                        customer.ModifyBy = c.ModifyBy;
                        customer.ModifyDate = DateTime.Now;
                    }

                    // CUSTOMER INFORMATION
                    customer.IndustryTypeID = c.IndustryTypeID;
                    customer.CustomerName = c.CustomerName;
                    customer.CustomerNameLatin = c.CustomerNameLatin;
                    customer.CustomerGender = c.CustomerGender;
                    customer.CustomerNationality = c.CustomerNationality;
                    customer.Salutation = c.Salutation;
                    customer.CustomerAddress = c.CustomerAddress;
                    customer.CustomerMobile = c.CustomerMobile;
                    customer.CustomerContactMobileNumber = c.CustomerMobile;
                    customer.CustomerNote = c.CustomerNote;
                    customer.CustomerStatus = c.CustomerStatus;

                    customer.CustomerTypeID = c.CustomerTypeID;
                    customer.ClassName = c.ClassName;
                    customer.ClassNameID = c.ClassNameID;
                    customer.ATTName = c.ATTName;

                    customer.CustomerGroup = c.CustomerGroup;
                    customer.CustomerGroupID = c.CustomerGroupID;
                    customer.EmployeeID = c.EmployeeID;

                    customer.CustomerCommune = c.CustomerCommune;
                    customer.CommuneID = c.CommuneID;
                    customer.CustomerDistrict = c.CustomerDistrict;
                    customer.DistrictID = c.DistrictID;
                    customer.CustomerProvinceCity = c.CustomerProvinceCity;
                    customer.ProvinceCityID = c.ProvinceCityID;

                    customer.CustomerIdentification = c.CustomerIdentification;
                    customer.IdentificationExpireDate = c.IdentificationExpireDate;

                    // Save customer first to generate CustomerID
                    await ctx.SaveChangesAsync();

                    // BILL INFORMATION
                    if (b != null)
                    {
                        await UpsertBillInfo(ctx, customer, b, c);
                    }

                    await ctx.SaveChangesAsync();

                    tx.Commit();

                    return customer.CustomerID;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static async Task UpsertBillInfo(
            DBContext ctx,
            Customer customer,
            BillInfo b,
            Customer c)
        {
            string invoiceNo = b.InvoiceNo;

            var billInfo = await ctx.TblCustomerBillInformation
                .FirstOrDefaultAsync(x =>
                    x.CustomerID == customer.CustomerID &&
                    (string.IsNullOrEmpty(invoiceNo)
                        ? string.IsNullOrEmpty(x.InvoiceNo)
                        : x.InvoiceNo == invoiceNo));

            bool isNew = billInfo == null;

            if (isNew)
            {
                billInfo = new BillInfo
                {
                    CustomerID = customer.CustomerID,
                    InvoiceNo = invoiceNo,
                    CreateBy = c.RegisterBy,
                    CreateDate = DateTime.Now
                };

                ctx.TblCustomerBillInformation.Add(billInfo);
            }

            // CUSTOMER INFORMATION
            billInfo.CustomerCode = customer.CustomerCode;
            billInfo.CustomerName = customer.CustomerName;
            billInfo.ATTName = b.ATTName;

            // COMPANY INFORMATION
            billInfo.CompanyName = b.CompanyName;
            billInfo.CompanyNameKH = b.CompanyNameKH;
            billInfo.VATNumber = b.VATNumber;
            billInfo.CompanyDeskPhone = b.CompanyDeskPhone;
            billInfo.CompanyMobile = b.CompanyMobile;
            billInfo.CompanyEmail = b.CompanyEmail;
            billInfo.CompanyAddress = b.CompanyAddress;
            billInfo.CompanyCommune = b.CompanyCommune;
            billInfo.CompanyDistrict = b.CompanyDistrict;
            billInfo.CompanyProvinceCity = b.CompanyProvinceCity;
            billInfo.CompanyContactName = b.CompanyContactName;
            billInfo.CompanyContactPosition = b.CompanyContactPosition;
            billInfo.CompanyContactMobileNumber = b.CompanyContactMobileNumber;

            // BILLING ADDRESS
            billInfo.BillToAddress = b.BillToAddress;
            billInfo.BillToCommune = b.BillToCommune;
            billInfo.BillToDistrict = b.BillToDistrict;
            billInfo.BillToProvinceCity = b.BillToProvinceCity;

            // INSTALLATION ADDRESS
            billInfo.InstallAddress = b.InstallAddress;
            billInfo.InstallCommune = b.InstallCommune;
            billInfo.InstallDistrict = b.InstallDistrict;
            billInfo.InstallProvinceCity = b.InstallProvinceCity;

            // GOOGLE MAP
            billInfo.GoogleMap_FullAddress = b.GoogleMap_FullAddress;
            billInfo.GoogleMap_Latitude = b.GoogleMap_Latitude;
            billInfo.GoogleMap_Longitude = b.GoogleMap_Longitude;

            // OTHER INFORMATION
            billInfo.TermCondition = b.TermCondition;
            billInfo.IsIncludedVAT = b.IsIncludedVAT;

            // SYSTEM INFORMATION
            billInfo.SystemID = customer.SystemID;
            billInfo.WarehouseID = customer.WarehouseID;
            billInfo.DomainID = customer.DomainID;
        }
    }
}