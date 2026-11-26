using MVC;
using NewInventorySystem.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using NSMSOLUTION.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    public class CustomerRegisterRequest
    {
        public Customer Customer { get; set; }
        public BillInfo BillInfo { get; set; }
    }

    [Authorize]
    [RoutePrefix("api/Customer")]
    public class APICustomerController : ApiController
    {
        private const string SP_EXC = "SP_GET_CUSTOMER_PROFILE";

        [HttpGet]
        [Route("Identification")]
        public async Task<IHttpActionResult> CustomerIdentificationExpire()
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    var today = DateTime.Today;

                    var data = ctx.Database
                        .SqlQuery<CustomerExpireDto>($"EXEC {SP_EXC}")
                        .ToList();

                    var hasAlert = data.Any(x =>
                        x.IdentificationExpireDate.HasValue &&
                        x.IdentificationExpireDate.Value.Date == today
                    );

                    if (!hasAlert)
                    {
                        return Ok("No expired customers");
                    }

                    var alerts = data
                        .Where(x => x.IdentificationExpireDate.HasValue &&
                                    x.IdentificationExpireDate.Value.Date <= today)
                        .ToList();

                    string message = "📢 <b>Customer Identification Expiry Alert</b>\n\n";

                    foreach (var item in alerts)
                    {
                        message += $"👤 <b>Name: {item.CustomerName}</b>\n" +
                                   $"📄 CID: {item.CustomerCode}\n" +
                                   $"📄 ID: {item.CustomerIdentification}\n" +
                                   $"📅 Expired: {item.IdentificationExpireDate:yyyy-MM-dd}\n" +
                                   $"⏱ Days: {item.ExpirePeriod}\n" +
                                   $"___________________________________\n\n";
                    }

                    var notification = new NotificationTelegram();
                    var result = await notification.NotificationTelegramBotToTopic(message, -1001545132903, 36562);

                    return Ok(result.Result == Utilities.ResultType.Successful
                        ? "Alert message successfully!" : result.Message);
                }
            }
            catch (Exception ex)
            {
                // Optional: log error here (recommended)
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Profile")]
        public async Task<IHttpActionResult> CustomerProfile(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "CustomerInfo" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("GetCustomerLists")]
        public async Task<IHttpActionResult> GetCustomerLists()
        {
            try
            {
                var html = new HTML();
                string query = "SP_GET_CUSTOMER_PROFILE";
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", "0" },
                    { "@RequestType", "GetCustomerLists" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("AllConnection")]
        public async Task<IHttpActionResult> AllConnection(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "AllConnection" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("CustomerTotalBalance")]
        public async Task<IHttpActionResult> CustomerTotalBalance(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "CustomerTotalBalance" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Unpaid")]
        public async Task<IHttpActionResult> Unpaid(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = "SP_GET_CUSTOMER_PROFILE";
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "Unpaid" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Paid")]
        public async Task<IHttpActionResult> Paid(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "Paid" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Quotation")]
        public async Task<IHttpActionResult> Quotation(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "Quotation" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("PI")]
        public async Task<IHttpActionResult> ProfomaInvoice(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "PI" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Transactions")]
        public async Task<IHttpActionResult> Transactions(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "Transactions" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("AttachmentFile")]
        public async Task<IHttpActionResult> AttachmentFile(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "AttachmentFile" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("IDServiceForm")]
        public async Task<IHttpActionResult> IDServiceForm(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "IDServiceForm" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Deposit")]
        public async Task<IHttpActionResult> Deposit(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "Deposit" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("CustomerProduct")]
        public async Task<IHttpActionResult> CustomerProduct(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "CustomerProduct" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("DeviceInfo")]
        public async Task<IHttpActionResult> DeviceInfo(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "DeviceInfo" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("BillInformation")]
        public async Task<IHttpActionResult> BillInformation(int CustomerID)
        {
            try
            {
                var html = new HTML();
                string query = SP_EXC;
                var para = new Dictionary<string, string>
                {
                    { "@CustomerID", CustomerID.ToString() },
                    { "@RequestType", "BillInformation" },
                    { "@DomainID", html.DomainID },
                };
                var data = html.JsonData(query, true, para);
                var obj = JsonConvert.DeserializeObject(data);
                return Json(obj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(object request)
        {
            try
            {
                var html = new HTML();
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.ToString());
                string query = @"update dbo.TblActivateCustomer set ConnectivityStatus = @ConnectivityStatus, 
                    ConnectivityStatusName = @ConnectivityStatusName,
                    LockedBy = @LockedBy,
                    LockedDate = @LockedDate
                    where ActivateCustomerID = @ActivateCustomerID";
                var para = new Dictionary<string, string>
                {
                    { "@ActivateCustomerID", data["ActivateCustomerID"] },
                    { "@LockedBy", html.UserFullName },
                    { "@LockedDate", DateTime.Parse(data["EndDate"].ToString()).ToString("yyyy-MM-dd HH:mm:ss") },
                    { "@ConnectivityStatus", data["StatusID"] },
                    { "@ConnectivityStatusName", data["StatusID"] == "40"
                        ? $"Active - Sync from system by {html.UserFullName}; Date: {DateTime.Now}"
                        : $"Locked - Sync from system by {html.UserFullName}; Date: {DateTime.Now}" }
                };

                html.SaveChangeData(query, false, para);

                return Ok("Status updated successfully");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("SaveBillInfo")]
        public async Task<IHttpActionResult> SaveBillInfo(object request)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.ToString());

                using (var ctx = new DBContext())
                {
                    var invoiceNo = data["InvoiceNo"];

                    var bill = await ctx.TblCustomerBillInformation
                        .Where(b => b.InvoiceNo == invoiceNo)
                        .FirstOrDefaultAsync();

                    if (bill == null)
                        return NotFound();

                    // Company
                    bill.CompanyName = data["CompanyName"];
                    bill.CompanyNameKH = data["CompanyNameKH"];
                    bill.VATNumber = data["VATNumber"];
                    bill.CompanyDeskPhone = data["CompanyDeskPhone"];
                    bill.CompanyContactMobileNumber = data["CompanyContactMobileNumber"];
                    bill.CompanyEmail = data["CompanyEmail"];
                    bill.ATTName = data["ATTName"];

                    // Company Address
                    bill.CompanyAddress = data["CompanyAddress"];
                    bill.CompanyCommune = data["CompanyCommune"];
                    bill.CompanyDistrict = data["CompanyDistrict"];
                    bill.CompanyProvinceCity = data["CompanyProvinceCity"];

                    // Bill To Address
                    bill.BillToAddress = data["BillToAddress"];
                    bill.BillToCommune = data["BillToCommune"];
                    bill.BillToDistrict = data["BillToDistrict"];
                    bill.BillToProvinceCity = data["BillToProvinceCity"];

                    // Installation Address
                    bill.InstallAddress = data["InstallAddress"];
                    bill.InstallCommune = data["InstallCommune"];
                    bill.InstallDistrict = data["InstallDistrict"];
                    bill.InstallProvinceCity = data["InstallProvinceCity"];

                    // Google Map
                    bill.GoogleMap_Latitude = data["GoogleMap_Latitude"];
                    bill.GoogleMap_Longitude = data["GoogleMap_Longitude"];
                    bill.GoogleMap_FullAddress = data["GoogleMap_FullAddress"];

                    // Contact Person
                    bill.CompanyContactName = data["CompanyContactName"];
                    bill.CompanyContactPosition = data["CompanyContactPosition"];

                    await ctx.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ApprovalUsers")]
        public IHttpActionResult GetApprovalUsers([FromUri] int? systemID = null, [FromUri] string search = null)
        {
            try
            {
                var conditions = new List<string>();

                // 1. Filter by SystemID if provided
                if (systemID.HasValue && systemID.Value > 0)
                {
                    conditions.Add($@"EXISTS (
                        SELECT 1 
                        FROM dbo.TblSetUserAccessSystemAndWarehouse ua WITH (NOLOCK) 
                        WHERE ua.UserLoginID = ul.UserLoginID 
                        AND ua.SystemID = {systemID.Value}
                    )");
                }

                // 2. Filter by Search term if provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string safeSearch = search.Trim().Replace("'", "''");
                    conditions.Add($@"(
                        ul.FullName LIKE N'%{safeSearch}%' 
                        OR ul.UserName LIKE N'%{safeSearch}%'
                        OR ISNULL(ul.TelegramUserName, '') LIKE N'%{safeSearch}%'
                    )");
                }

                // Build WHERE clause dynamically
                string whereClause = conditions.Count > 0 
                    ? "WHERE " + string.Join(" AND ", conditions) 
                    : "";

                string sql = $@"
                    SELECT 
                        ul.UserLoginID, 
                        ISNULL(ul.FullName, ul.UserName) AS FullName, 
                        ISNULL(ul.TelegramUserName, '') AS TelegramUserName
                    FROM dbo.TblUserLogin ul WITH (NOLOCK)
                    {whereClause}
                    ORDER BY 
                        CASE WHEN ul.FullName = 'Ms. Meng Daneth' THEN 1 ELSE 2 END, 
                        ul.FullName ASC";

                var html = new HTML();
                System.Data.DataSet ds = html.LoadData(sql);

                var userList = new List<object>();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (System.Data.DataRow row in ds.Tables[0].Rows)
                    {
                        userList.Add(new
                        {
                            UserLoginID = row["UserLoginID"]?.ToString(),
                            FullName = row["FullName"]?.ToString(),
                            TelegramUserName = row["TelegramUserName"]?.ToString()
                        });
                    }
                }

                return Json(new { success = true, count = userList.Count, data = userList });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route("Register")]
        public async Task<IHttpActionResult> Register(CustomerRegisterRequest req)
        {
            if (req?.Customer == null)
                return BadRequest("Customer information is required.");

            try
            {
                var result = await CustomerService.RegisterCustomerAsync(req);

                return Ok(new
                {
                    success = true,
                    message = req.Customer.CustomerID > 0 
                        ? "Update customer successfully." 
                        : "Customer registered successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        // add new api Renew
        [AllowAnonymous]
        [HttpPost]
        [Route("Renew")]
        public async Task<IHttpActionResult> Renew([FromBody] CustomerRenewRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null");
            }
            if (request.CustomerID <= 0)
            {
                return BadRequest("Invalid CustomerID");
            }
            if (string.IsNullOrEmpty(request.ReferenceNo))
            {
                return BadRequest("ReferenceNo is required");
            }

            try
            {
                ConfigureApiContext(request.SystemID, request.WarehouseID);
                using (var ctx = new DBContext())
                {
                    // 1. Fetch Customer
                    var customer = await ctx.TblCustomer.FindAsync(request.CustomerID);
                    if (customer == null)
                    {
                        return BadRequest($"Customer with ID {request.CustomerID} not found");
                    }

                    // 2. Fetch old invoice and bill info
                    var billInfo = await ctx.TblCustomerBillInformation
                        .Where(x => x.CustomerID == request.CustomerID && x.InvoiceNo == request.ReferenceNo)
                        .FirstOrDefaultAsync();

                    if (billInfo == null)
                    {
                        return BadRequest($"Billing Information not found for Invoice {request.ReferenceNo}");
                    }

                    // 3. Fetch old billing invoices (Service Plans)
                    var oldBillingInvoices = await ctx.TblCustomerBillingInvoice
                        .Where(x => x.CustomerID == request.CustomerID && x.InvoiceNo == request.ReferenceNo)
                        .ToListAsync();

                    if (!oldBillingInvoices.Any())
                    {
                        return BadRequest($"No service plans found for Invoice {request.ReferenceNo}");
                    }

                    // 4. Do not copy other charges (installation fee, etc.) on renew

                    // 5. Generate New Invoice Number dynamically using SP_SHOW_PREFIX
                    var html = new HTML();
                    string invoiceType = customer.TypeOfInvoice ?? "None VAT";

                    // Fetch prefix pattern dynamically from the customer's class in the database
                    string prefixPattern = "";
                    if (customer.ClassNameID.HasValue && customer.ClassNameID.Value > 0)
                    {
                        prefixPattern = await ctx.Database.SqlQuery<string>(
                            "SELECT TOP 1 PrefixInvoice FROM dbo.TblClass WHERE ClassID = @p0",
                            customer.ClassNameID.Value
                        ).FirstOrDefaultAsync();
                    }

                    // Fallback to default patterns if not found in database
                    if (string.IsNullOrEmpty(prefixPattern))
                    {
                        prefixPattern = invoiceType == "VAT" ? "NSM/TI/{yy}-{000000}"
                            : invoiceType == "CM" ? "NSM/CM/{yy}-{000000}"
                            : "CISP{yy}-{000000}";
                    }
                    else
                    {
                        // If it's a Credit Memo, ensure the prefix uses CM instead of TI
                        if (invoiceType == "CM")
                        {
                            prefixPattern = prefixPattern.Contains("/TI/")
                                ? prefixPattern.Replace("/TI/", "/CM/")
                                : prefixPattern.Contains("TI") ? prefixPattern.Replace("TI", "CM") : "NSM/CM/{yy}-{000000}";
                        }
                    }

                    string prefixSql = @"
                        DECLARE @Module nvarchar(50) = @pModule;
                        EXEC [SP_SHOW_PREFIX] @pPrefix, @Module, @SystemID, @WarehouseID, @DomainID;
                    ";

                    var para = new Dictionary<string, string>
                    {
                        { "pPrefix", prefixPattern },
                        { "pModule", "Customer Billing Prefix" },
                        { "SystemID", html.SystemID },
                        { "WarehouseID", html.WarehouseID },
                        { "DomainID", html.DomainID }
                    };

                    string prefixJson = html.JsonData(prefixSql, false, para);
                    var prefixObj = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(prefixJson);
                    string newInvoiceNo = prefixObj?.FirstOrDefault()?["PrefixNumber"]?.ToString();

                    if (string.IsNullOrEmpty(newInvoiceNo))
                    {
                        return InternalServerError(new Exception("Failed to generate a new Invoice Number."));
                    }

                    // 6. Build the payload JObject exactly as expected by SaveChangeCustomerV2
                    // 6. Build the payload JObject dynamically using reflection to synchronize with database changes automatically
                    var payload = new JObject();
                    payload.Merge(GetScalarPropertiesAsJObject(customer));
                    payload.Merge(GetScalarPropertiesAsJObject(billInfo));

                    payload["CustomerName_CustomerID"] = customer.CustomerID;
                    payload["CustomerName_AUTOFill"] = customer.CustomerID;
                    payload["IsRenew"] = true;
                    payload["IsChangePlan"] = false;
                    payload["SetCID"] = invoiceType;
                    payload["FormType"] = "Invoice";
                    payload["InvoiceNo"] = newInvoiceNo;
                    payload["PrefixInvoice"] = prefixPattern;
                    payload["ReferenceNo"] = request.ReferenceNo;
                    payload["InvoiceDate"] = DateTime.Now.ToString("yyyy-MM-dd");

                    // Map key mappings that differ from database column names
                    payload["IndustryType_IndustryTypeID"] = customer.IndustryTypeID;
                    payload["CustomerType_CustomerTypeID"] = customer.CustomerTypeID;
                    payload["ClassName_ClassID"] = customer.ClassNameID;
                    payload["CustomerGroup_CustomerGroupID"] = customer.CustomerGroupID;
                    payload["SallerName_SellerID"] = customer.EmployeeID;
                    payload["CustomerCommune_CommuneID"] = customer.CommuneID;
                    payload["CustomerDistrict_DistrictID"] = customer.DistrictID;
                    payload["CustomerProvinceCity_ProvinceCityID"] = customer.ProvinceCityID;
                    payload["TermsConditions"] = billInfo.TermCondition;
                    payload["IspLinkName_IspLinkID"] = billInfo.IspLinkID;
                    //    payload["SID"] = billInfo.ConnectionID;
                    //    payload["ConnectionID"] = billInfo.ConnectionID;
                    // Retrieve existing SID and ConnectionID from the previous invoice records
                    string existingSid = oldBillingInvoices.FirstOrDefault(x => !string.IsNullOrEmpty(x.SID))?.SID;
                    if (string.IsNullOrEmpty(existingSid))
                    {
                        var oldCustInv = await ctx.TblCustomerInvoice
                            .Where(x => x.CustomerID == request.CustomerID && x.InvoiceNo == request.ReferenceNo && !string.IsNullOrEmpty(x.SID))
                            .FirstOrDefaultAsync();
                        existingSid = oldCustInv?.SID;
                    }
                    if (string.IsNullOrEmpty(existingSid) && !string.IsNullOrEmpty(billInfo.ConnectionID))
                    {
                        var actCust = await ctx.TblActivateCustomer
                            .Where(x => x.CustomerID == request.CustomerID && (x.ConnectionID == billInfo.ConnectionID || x.ConnectionOfConnectionID == billInfo.ConnectionID) && !string.IsNullOrEmpty(x.SID))
                            .FirstOrDefaultAsync();
                        existingSid = actCust?.SID;
                    }

                    string existingConnectionId = oldBillingInvoices.FirstOrDefault(x => !string.IsNullOrEmpty(x.ConnectionID))?.ConnectionID ?? billInfo.ConnectionID;

                    payload["SID"] = existingSid ?? "";
                    payload["ConnectionID"] = existingConnectionId ?? "";

                    // Ensure format matches Web form expectation for date properties
                    if (customer.IdentificationExpireDate.HasValue)
                        payload["IdentificationExpireDate"] = customer.IdentificationExpireDate.Value.ToString("yyyy-MM-dd");

                    bool isIncludedVat = (invoiceType == "VAT") ? (billInfo.IsIncludedVAT ?? false) : (invoiceType == "CM");
                    payload["IsIncludedVAT"] = isIncludedVat;

                    // Automatically calculate billing dates if not supplied
                    DateTime defaultStartDate = (billInfo.EndDate.HasValue && billInfo.EndDate.Value >= DateTime.Today)
                        ? billInfo.EndDate.Value
                        : DateTime.Today;
                    DateTime startDate = request.StartDate ?? defaultStartDate;
                    int duration = oldBillingInvoices.FirstOrDefault()?.Duration ?? 1;
                    string um = oldBillingInvoices.FirstOrDefault()?.UM ?? "Month";
                    DateTime endDate = request.EndDate ?? (um.ToLower().Contains("month") ? startDate.AddMonths(duration) : startDate.AddMonths(1));
                    DateTime nextInvoiceDate = request.NextInvoiceDate ?? endDate;
                    DateTime paymentDueDate = request.PaymentDueDate ?? DateTime.Today;

                    payload["StartDate"] = startDate.ToString("yyyy-MM-dd");
                    payload["EndDate"] = endDate.ToString("yyyy-MM-dd");
                    payload["NextInvoiceDate"] = nextInvoiceDate.ToString("yyyy-MM-dd");
                    payload["PaymentDueDate"] = paymentDueDate.ToString("yyyy-MM-dd");

                    // Map Service Plan row(s)
                    var servicePlanArray = new JArray();
                    decimal subtotal = 0;
                    foreach (var item in oldBillingInvoices)
                    {
                        var planObj = new JObject
                        {
                            ["ServiceID"] = item.ServiceID,
                            ["ServicePlan"] = item.ServiceName,
                            ["PackageName"] = item.PackageName,
                            ["PackageID"] = item.PackageID,
                            ["PackageDetailID"] = item.PackageDetailID,
                            ["PID"] = Guid.NewGuid().ToString(), // Generate fresh PID
                            ["Duration"] = item.Duration,
                            ["QTY"] = item.QTY ?? 1,
                            ["UM"] = item.UM,
                            ["UnitPrice"] = item.UnitPrice ?? 0,
                            ["DisP"] = item.DiscountP ?? 0,
                            ["DisUSD"] = item.DiscountUSD ?? 0,
                            ["Amount"] = item.Amount ?? 0,
                            ["ChartOfAccountID"] = item.ChartOfAccountID
                        };

                        var packageDetail = item.PackageDetailID.HasValue
                            ? await ctx.Database.SqlQuery<PackageDetailDto>(
                                "SELECT PackageDetailID, Bandwidth, DowSpeed, UpSpeed, Feature FROM dbo.TblPackageDetail WHERE PackageDetailID = @p0",
                                item.PackageDetailID.Value
                              ).FirstOrDefaultAsync()
                            : null;

                        if (packageDetail != null)
                        {
                            planObj["DownSpeed"] = packageDetail.DowSpeed;
                            planObj["UpSpeed"] = packageDetail.UpSpeed;
                            planObj["Bandwidth"] = packageDetail.Bandwidth;
                            planObj["Feature"] = packageDetail.Feature;
                        }
                        else
                        {
                            planObj["DownSpeed"] = 0;
                            planObj["UpSpeed"] = 0;
                            planObj["Bandwidth"] = 0;
                            planObj["Feature"] = "";
                        }

                        servicePlanArray.Add(planObj);

                        subtotal += item.Amount ?? 0;
                    }
                    payload["CustomerServicePlan"] = servicePlanArray;

                    // Map Other Charge row(s) (Do not copy other charges like installation fees during renew)
                    var otherChargeArray = new JArray();
                    payload["CustomerOtherCharge"] = otherChargeArray;

                    // Calculate totals
                    decimal vatRate = (invoiceType == "VAT" && isIncludedVat) ? 0.1m : 0m;

                    // decimal vat = Math.Round(subtotal * vatRate, 2);
                    // decimal total = subtotal + vat;

                    // Mimic Javascript floating point rounding behavior to exactly match Web System totals
                    double jsSubtotal = (double)subtotal;
                    double jsVat = jsSubtotal * (double)vatRate;
                    decimal vat = Math.Round((decimal)jsVat, 2, MidpointRounding.AwayFromZero);
                    decimal total = Math.Round((decimal)(jsSubtotal + jsVat), 2, MidpointRounding.AwayFromZero);

                    var totalsArray = new JArray();
                    var totalsObj = new JObject
                    {
                        ["Subtotal"] = subtotal,
                        ["VAT"] = vat,
                        ["Total"] = total,
                        ["GrandTotal"] = total,
                        ["TotalDiscountP"] = 0,
                        ["TotalDiscountUSD"] = 0
                    };
                    totalsArray.Add(totalsObj);
                    payload["InvoiceTotalAmount"] = totalsArray;

                    // Add Approval fields
                    payload["RequestApprovalTgUser"] = request.RequestApprovalTgUser;
                    payload["RequestApprovalNameID"] = request.RequestApprovalNameID;
                    payload["RequestApprovalName"] = request.RequestApprovalName;
                    payload["RequestCheckByTgUser"] = request.RequestCheckByTgUser;
                    payload["RequestCheckByNameID"] = request.RequestCheckByNameID;
                    payload["RequestCheckByName"] = request.RequestCheckByName;

                    // 7. Invoke the AddCustomerController SaveChangeCustomerV2 method
                    var addCustomerController = new AddCustomerController();
                    var mvcResult = await addCustomerController.SaveChangeCustomerV2(payload.ToString());

                    // add new

                    var jsonResultObj = JObject.FromObject(mvcResult.Data);
                    if (jsonResultObj["success"]?.Value<bool>() == true)
                    {
                        var notificationData = jsonResultObj["data"] as JObject;
                        if (notificationData != null)
                        {
                            notificationData["NeedByCheck"] = request.RequestCheckByName;
                            notificationData["NeedByCheckID"] = request.RequestCheckByNameID;
                            notificationData["NeedByApproveID"] = request.RequestApprovalNameID;

                            await addCustomerController.SentNotification(notificationData.ToString());
                        }
                    }

                    return Json(mvcResult.Data);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SavePayment")]
        public async Task<IHttpActionResult> SavePayment([FromBody] CustomerPaymentRequest request)
        {
            try
            {
                var html = new HTML();

                // 1. Try to read from Form if JSON request body is empty
                if (request == null)
                {
                    var form = System.Web.HttpContext.Current.Request.Form;
                    if (form.Count > 0)
                    {
                        var paymentDataStr = form["PaymentData"];
                        request = !string.IsNullOrEmpty(paymentDataStr)
                            ? JsonConvert.DeserializeObject<CustomerPaymentRequest>(paymentDataStr)
                            : new CustomerPaymentRequest
                            {
                                CustomerID = Convert.ToInt32(form["CustomerID"] ?? "0"),
                                CustomerCode = form["CustomerCode"],
                                CustomerName = form["CustomerName"],
                                InvoiceNo = form["InvoiceNo"],
                                PaymentAmount = Convert.ToDecimal(form["PaymentAmount"] ?? "0"),
                                PaymentDate = form["PaymentDate"],
                                PaymentMenthod = Convert.ToInt32(form["PaymentMenthod"] ?? form["PaymentMethodID"] ?? "0"),
                                ReferenceNo = form["ReferenceNo"],
                                ChartOfAccountID = Convert.ToInt32(form["ChartOfAccountID"] ?? "0"),
                                ToAccountNumber = form["ToAccountNumber"],
                                Category = form["Category"],
                                PaymentDescription = form["PaymentDescription"],
                                ReceivedFromName = form["ReceivedFromName"],
                                StatusID = Convert.ToInt32(form["StatusID"] ?? "0"),
                                BillingInvoiceID = Convert.ToInt32(form["BillingInvoiceID"] ?? "0"),
                                Payer = form["Payer"],
                                SystemID = string.IsNullOrEmpty(form["SystemID"]) ? (int?)null : Convert.ToInt32(form["SystemID"]),
                                WarehouseID = string.IsNullOrEmpty(form["WarehouseID"]) ? (int?)null : Convert.ToInt32(form["WarehouseID"]),
                                UserID = string.IsNullOrEmpty(form["UserID"]) ? (int?)null : Convert.ToInt32(form["UserID"]),
                                UserFullName = form["UserFullName"],
                                DomainID = form["DomainID"]
                            };
                    }
                }

                if (request == null)
                {
                    return BadRequest("Invalid request payload.");
                }

                ConfigureApiContext(request.SystemID, request.WarehouseID, request.UserID, request.UserFullName, request.DomainID);

                // Apply System/User/Warehouse overrides if supplied in the request
                if (request.SystemID.HasValue) html.SystemID = request.SystemID.Value.ToString();
                if (request.WarehouseID.HasValue) html.WarehouseID = request.WarehouseID.Value.ToString();
                if (request.UserID.HasValue) html.UserID = request.UserID.Value.ToString();
                if (!string.IsNullOrEmpty(request.UserFullName)) html.UserFullName = request.UserFullName;
                if (!string.IsNullOrEmpty(request.DomainID)) html.DomainID = request.DomainID;

                // Ensure SystemID and WarehouseID are not empty (especially for stateless API requests)
                if (string.IsNullOrEmpty(html.SystemID)) html.SystemID = "1";
                if (string.IsNullOrEmpty(html.WarehouseID)) html.WarehouseID = "1";
                if (string.IsNullOrEmpty(html.UserID)) html.UserID = "1";
                if (string.IsNullOrEmpty(html.UserFullName)) html.UserFullName = "API User";
                if (string.IsNullOrEmpty(html.DomainID)) html.DomainID = "001";

                // Auto-fill missing customer details from database if they are not provided in the request
                if (request.CustomerID > 0 && (string.IsNullOrEmpty(request.CustomerCode) || string.IsNullOrEmpty(request.CustomerName)))
                {
                    string sqlCust = "select CustomerCode, CustomerName from dbo.TblCustomer where CustomerID = " + request.CustomerID;
                    DataSet dsCust = html.LoadData(sqlCust);
                    if (dsCust.Tables.Count > 0 && dsCust.Tables[0].Rows.Count > 0)
                    {
                        if (string.IsNullOrEmpty(request.CustomerCode))
                            request.CustomerCode = dsCust.Tables[0].Rows[0]["CustomerCode"]?.ToString();
                        if (string.IsNullOrEmpty(request.CustomerName))
                            request.CustomerName = dsCust.Tables[0].Rows[0]["CustomerName"]?.ToString();
                    }
                }

                if (request.BillingInvoiceID <= 0 && !string.IsNullOrEmpty(request.InvoiceNo))
                {
                    string sqlBillInv = "select distinct ci.BillingInvoiceID from dbo.TblCustomerInvoice ci where ci.InvoiceNo = '" + request.InvoiceNo + "'";
                    DataSet dsBillInv = html.LoadData(sqlBillInv);
                    if (dsBillInv.Tables.Count > 0 && dsBillInv.Tables[0].Rows.Count > 0)
                    {
                        var billingInvoiceIDObj = dsBillInv.Tables[0].Rows[0]["BillingInvoiceID"];
                        request.BillingInvoiceID = (billingInvoiceIDObj == DBNull.Value || billingInvoiceIDObj == null) ? 0 : Convert.ToInt32(billingInvoiceIDObj);
                    }
                }

                if (request.StatusID <= 0)
                {
                    request.StatusID = 10;
                }

                if (string.IsNullOrEmpty(request.Payer))
                {
                    request.Payer = request.CustomerName;
                }

                if (string.IsNullOrEmpty(request.ReceivedFromName))
                {
                    request.ReceivedFromName = request.CustomerName;
                }

                SqlResult sqlresult = new SqlResult();
                string ServiceID = "";
                string ConnectionID = "";
                DateTime timeNow = DateTime.Now;
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);

                NotificationTelegram notificationTelegram = new NotificationTelegram();
                StringBuilder sb = new StringBuilder();

                string InvoiceNo = request.InvoiceNo;
                string memo = request.PaymentDescription;
                string StatusID = request.StatusID.ToString();
                string CustomerID = request.CustomerID.ToString();
                // string Payment = $"$ {request.PaymentAmount}";
                string Payment = (request.PaymentAmount % 1 == 0)
    ? $"$ {request.PaymentAmount:N0}"
    : $"$ {request.PaymentAmount:N2}";

                string CustomerName = System.Web.HttpUtility.UrlDecode(request.CustomerName);
                string CID = System.Web.HttpUtility.UrlDecode(request.CustomerCode);
                CustomerName = $"{html.ToUrlSlug($"{CustomerName + " [" + CID + "]"}")}";

                string DestinationFolder = "/Uploads/CustomerAttFiles/" + CustomerName + "/Payment AttFile/";
                string AttachmentFile = "";

                // 2. Handle files uploaded via HttpContext
                var uploadedFiles = System.Web.HttpContext.Current.Request.Files;
                if (uploadedFiles.Count > 0)
                {
                    sqlresult = html.UploadFile(DestinationFolder);
                    if (sqlresult.Result == ResultType.Successful)
                    {
                        AttachmentFile = sqlresult.Value;
                    }
                    else
                    {
                        return Json(new
                        {
                            Message = sqlresult.Message,
                            Result = sqlresult.Result,
                            Data = sqlresult.Value,
                            OfficialReceiptData = ""
                        });
                    }
                }

                // 3. Handle base64 encoded attachment
                else if (!string.IsNullOrEmpty(request.Base64Attachment))
                {
                    try
                    {
                        string fileName = request.AttachmentFileName ?? "attachment.png";
                        string extension = System.IO.Path.GetExtension(fileName);
                        if (string.IsNullOrEmpty(extension)) extension = ".jpg";
                        fileName = fileName.Replace(extension, "");
                        string fname = $"{html.ToUrlSlug($"{fileName}-{html.GetUnixEpochMethod(DateTime.Now)}")}{extension}";

                        // Use Server.MapPath so the physical folder path matches Telegram's Server.MapPath exactly
                        string physicalFolder = System.Web.HttpContext.Current.Server.MapPath(DestinationFolder);
                        if (!System.IO.Directory.Exists(physicalFolder))
                        {
                            System.IO.Directory.CreateDirectory(physicalFolder);
                        }

                        string savedFileName = System.IO.Path.Combine(physicalFolder, fname);
                        string base64Data = request.Base64Attachment.Trim();
                        if (base64Data.Contains(","))
                        {
                            base64Data = base64Data.Substring(base64Data.IndexOf(",") + 1);
                        }
                        byte[] fileBytes = Convert.FromBase64String(base64Data);
                        System.IO.File.WriteAllBytes(savedFileName, fileBytes);
                        AttachmentFile = fname;
                    }
                    catch (Exception ex)
                    {
                        return Json(new
                        {
                            Message = "Failed to save base64 attachment: " + ex.Message,
                            Result = ResultType.Failed,
                            Data = "",
                            OfficialReceiptData = ""
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(request.AttachmentFile))
                {
                    AttachmentFile = request.AttachmentFile;
                }

                // Generate Receipt Number (RV) using prefix system
                string ReceiptNo = "";
                string referenceNoStr = request.ReferenceNo?.Trim();

                if (!string.IsNullOrEmpty(referenceNoStr))
                {
                    // Reuse the selected receipt instead of generating a new one
                    ReceiptNo = referenceNoStr;
                }
                else
                {
                    // Determine the receipt module based on the invoice type:
                    // If InvoiceNo starts with "CISP" → use ISP module (prefix I26-XXXXX)
                    // Otherwise → use NSM module (prefix N26-XXXXX)
                    string rvModule = InvoiceNo.StartsWith("CISP", StringComparison.OrdinalIgnoreCase) ? "ISP" : "NSM";

                    string rvSql = $@"declare @Prefix nvarchar(50), @Module nvarchar(50) = '{rvModule}';
 select @Prefix = Prefix from dbo.TblPrefix where Module = '{rvModule}' and SystemID = {html.SystemID} and WarehouseID = {html.WarehouseID};
                         exec [SP_SHOW_PREFIX] @Prefix, @Module,  {html.SystemID}, {html.WarehouseID}
                         ,'" + html.DomainID + "', 1, '', 0, ''";
                    DataSet dsRV = html.LoadData(rvSql);
                    if (dsRV.Tables.Count > 0 && dsRV.Tables[0].Rows.Count > 0)
                    {
                        ReceiptNo = dsRV.Tables[0].Rows[0][0].ToString();
                    }
                }

                sb.AppendLine("<Data>");
                sb.AppendLine("<PaymentInfo>");
                sb.AppendLine("<InvoiceNo>" + html.EscapeXml(request.InvoiceNo ?? "") + "</InvoiceNo>");
                sb.AppendLine("<CustomerID>" + request.CustomerID + "</CustomerID>");
                sb.AppendLine("<PaymentID>" + request.PaymentID + "</PaymentID>");
                sb.AppendLine("<PaymentAmount>" + request.PaymentAmount + "</PaymentAmount>");

                DateTime parsedDate;
                if (string.IsNullOrEmpty(request.PaymentDate) || !DateTime.TryParse(request.PaymentDate, out parsedDate))
                {
                    parsedDate = DateTime.Now;
                }
                string paymentDateVal = parsedDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) + "T" + easternTimeNow.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                sb.AppendLine("<PaymentDate>" + html.EscapeXml(paymentDateVal) + "</PaymentDate>");

                sb.AppendLine("<PaymentMenthod>" + request.PaymentMenthod + "</PaymentMenthod>");
                sb.AppendLine("<ChequeNumber>" + html.EscapeXml(request.ChequeNumber ?? "") + "</ChequeNumber>");
                sb.AppendLine("<ReferenceNo>" + html.EscapeXml(request.ReferenceNo ?? "") + "</ReferenceNo>");
                sb.AppendLine("<ChartOfAccountID>" + request.ChartOfAccountID + "</ChartOfAccountID>");
                sb.AppendLine("<ToAccountNumber>" + html.EscapeXml(request.ToAccountNumber ?? "") + "</ToAccountNumber>");
                sb.AppendLine("<Category>" + html.EscapeXml(request.Category ?? "") + "</Category>");
                sb.AppendLine("<PaymentDescription>" + html.EscapeXml(request.PaymentDescription ?? "") + "</PaymentDescription>");
                sb.AppendLine("<ReceivedFromName>" + html.EscapeXml(request.ReceivedFromName ?? "") + "</ReceivedFromName>");
                sb.AppendLine("<StatusID>" + request.StatusID + "</StatusID>");
                sb.AppendLine("<BillingInvoiceID>" + request.BillingInvoiceID + "</BillingInvoiceID>");
                sb.AppendLine("<CustomerCode>" + html.EscapeXml(request.CustomerCode ?? "") + "</CustomerCode>");
                sb.AppendLine("<CustomerName>" + html.EscapeXml(request.CustomerName ?? "") + "</CustomerName>");

                string payerName = !string.IsNullOrEmpty(request.Payer) ? request.Payer : request.CustomerName;
                sb.AppendLine("<Payer>" + html.EscapeXml(payerName ?? "") + "</Payer>");

                sb.AppendLine("<PaidByUserID>" + html.UserID + "</PaidByUserID>");
                sb.AppendLine("<PaidBy>" + html.UserFullName + "</PaidBy>");
                sb.AppendLine("<SystemID>" + html.SystemID + "</SystemID>");
                sb.AppendLine("<WarehouseID>" + html.WarehouseID + "</WarehouseID>");
                sb.AppendLine("<DomainID>" + html.EscapeXml(html.DomainID) + "</DomainID>");
                sb.AppendLine("<Domain>" + html.EscapeXml(html.DomainID) + "</Domain>");
                sb.AppendLine("<DestinationFolder>" + DestinationFolder + "</DestinationFolder>");
                sb.AppendLine("<AttachmentFile>" + AttachmentFile + "</AttachmentFile>");
                sb.AppendLine("<ReceiptNo>" + html.EscapeXml(ReceiptNo) + "</ReceiptNo>");

                // Dynamic Bank / Mobile Transfer details
                if (!string.IsNullOrEmpty(request.FromBankName))
                    sb.AppendLine("<FromBankName>" + html.EscapeXml(request.FromBankName) + "</FromBankName>");
                if (!string.IsNullOrEmpty(request.FromAccountNumber))
                    sb.AppendLine("<FromAccountNumber>" + html.EscapeXml(request.FromAccountNumber) + "</FromAccountNumber>");
                if (!string.IsNullOrEmpty(request.FromAccountName))
                    sb.AppendLine("<FromAccountName>" + html.EscapeXml(request.FromAccountName) + "</FromAccountName>");
                if (!string.IsNullOrEmpty(request.FromPhoneNumber))
                    sb.AppendLine("<FromPhoneNumber>" + html.EscapeXml(request.FromPhoneNumber) + "</FromPhoneNumber>");
                if (!string.IsNullOrEmpty(request.ToPhoneNumber))
                    sb.AppendLine("<ToPhoneNumber>" + html.EscapeXml(request.ToPhoneNumber) + "</ToPhoneNumber>");

                sb.AppendLine("</PaymentInfo>");
                sb.AppendLine("</Data>");

                Dictionary<string, string> Para = new Dictionary<string, string>()
                 {
                     { "Data", "N'"+sb.ToString()+"'" }
                 };
                sqlresult = html.SaveChangeData("SP_INSERT_PAYMENT", true, Para, true, true);

                string OfficialReceiptData = "";
                if (sqlresult.Data != null && sqlresult.Data.Tables.Count > 0)
                {
                    OfficialReceiptData = html.JsonConvertDataTable(sqlresult.Data.Tables[0]);
                }

                string sqls = @"SELECT BotToken FROM TblTelegramBots WHERE SystemID = @SystemID";
                Dictionary<string, string> botParams = new Dictionary<string, string> { { "SystemID", html.SystemID } };
                DataSet newbotData = html.LoadData(sqls, false, botParams);
                string newbots = "";
                if (newbotData != null && newbotData.Tables.Count > 0 && newbotData.Tables[0].Rows.Count > 0)
                {
                    newbots = newbotData.Tables[0].Rows[0]["BotToken"]?.ToString();
                }

                string sqlGuest = @"SELECT 
                                     CASE 
                                         WHEN COALESCE(NULLIF(CustomerName, ''),CustomerNameLatin) NOT LIKE '%[a-zA-Z]%' AND CustomerGender = 'Male' 
                                             THEN N'លោក ' + COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin)

                                         WHEN COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin) NOT LIKE '%[a-zA-Z]%' AND CustomerGender = 'Female' 
                                             THEN N'អ្នកស្រី ' + COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin)

                                         WHEN CustomerGender = 'Male' 
                                             THEN 'Mr. ' + COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin)

                                         WHEN CustomerGender = 'Female' 
                                             THEN 'Ms. ' + COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin)

                                         ELSE COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin)
                                     END AS CustomerName
                                 FROM TblCustomer  WHERE CustomerID  = @CustomerID";
                Dictionary<string, string> guestName = new Dictionary<string, string> { { "CustomerID", CustomerID } };
                DataSet customerName = html.LoadData(sqlGuest, false, guestName);
                string guest = "";
                if (customerName != null && customerName.Tables.Count > 0 && customerName.Tables[0].Rows.Count > 0)
                {
                    guest = customerName.Tables[0].Rows[0]["CustomerName"]?.ToString();
                }

                if (sqlresult.Result == ResultType.Successful)
                {
                    if (StatusID != "11")
                    {
                        string ChartOfAccountName = "";
                        string sql_acc = "select AccountName from dbo.TblChartOfAccount where ChartOfAccountID = @ChartOfAccountID and SystemID = @SystemID and WarehouseID = @WarehouseID";
                        Dictionary<string, string> accParams = new Dictionary<string, string>
                         {
                             { "ChartOfAccountID", request.ChartOfAccountID.ToString() },
                             { "SystemID", html.SystemID },
                             { "WarehouseID", html.WarehouseID }
                         };
                        DataSet ds_acc = html.LoadData(sql_acc, false, accParams);
                        if (ds_acc != null && ds_acc.Tables.Count > 0 && ds_acc.Tables[0].Rows.Count > 0)
                        {
                            ChartOfAccountName = ds_acc.Tables[0].Rows[0]["AccountName"].ToString();
                            var match = System.Text.RegularExpressions.Regex.Match(ChartOfAccountName, @"\(([^)]+)\)");
                            if (match.Success)
                                ChartOfAccountName = match.Groups[1].Value.Trim();
                        }

                        string PaymentAttachFolder = "/Uploads/CustomerAttFiles/" + CustomerName + "/Payment AttFile/";

                        //  long telegramChatId = -5354936745; // for test
                        long telegramChatId = -1003702719002; // production
                        string configChatId = System.Configuration.ConfigurationManager.AppSettings["TelegramPaymentChatId"];
                        if (!string.IsNullOrEmpty(configChatId) && long.TryParse(configChatId, out long parsedId))
                        {
                            telegramChatId = parsedId;
                        }

                        StringBuilder payMsg = new StringBuilder();
                        payMsg.AppendLine("");
                        payMsg.AppendLine("<b>We recieved payment from</b>");
                        payMsg.AppendLine("<b>Customer:</b> " + guest);
                        payMsg.AppendLine("<b>Invoice No:</b> " + InvoiceNo);
                        payMsg.AppendLine("<b>Deposit To:</b> " + ChartOfAccountName);
                        payMsg.AppendLine("<b>Amount :</b> " + Payment);
                        payMsg.AppendLine("<b>Received By:</b> " + html.UserFullName);
                        payMsg.AppendLine("<b>Memo :</b> " + memo);

                        if (!string.IsNullOrEmpty(AttachmentFile))
                        {
                            string[] files = AttachmentFile.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

                            //Send message once
                            await notificationTelegram.NotificationTelegramBot(payMsg.ToString(), telegramChatId);

                            //Send each as full inline photo
                            foreach (string file in files)
                            {
                                string fullFilePath = System.Web.HttpContext.Current.Server.MapPath(PaymentAttachFolder + file.Trim());
                                if (System.IO.File.Exists(fullFilePath))
                                    await notificationTelegram.SendPhotoToGroup(fullFilePath, "", telegramChatId);
                            }
                        }
                        else
                        {
                            await notificationTelegram.NotificationTelegramBot(payMsg.ToString(), telegramChatId);
                        }

                        string sql = "select b.UM, b.CustomerID, b.BillingInvoiceID, b.InvoiceType, b.PPPoEUserName, b.ConnectivityStatus, b.StartDate, b.EndDate, b.Duration, isnull(b.IsAlreadyApplyProfile, 0) IsAlreadyApplyProfile, b.ActivateCustomerID, b.IsChangePlan, case when b.invTmp = 1 then b.PackageName else REPLACE(b.ServiceName, s.ServiceName+' - ', '') end PackageName  ";
                        sql += "from dbo.TblCustomerBillingInvoice b ";
                        sql += "inner join dbo.TblServices s on s.ServiceID = b.ServiceID ";
                        sql += "where b.CustomerID =" + CustomerID + " and b.InvoiceNo = N'" + InvoiceNo + "' and b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID;
                        DataSet ds = html.LoadData(sql);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                string InvoiceType = dr["InvoiceType"].ToString();
                                string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                                string StartDate = dr["StartDate"] != DBNull.Value ? Convert.ToDateTime(dr["StartDate"]).ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) : "";
                                string EndDate = dr["EndDate"] != DBNull.Value ? Convert.ToDateTime(dr["EndDate"]).ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) : "";
                                string Duration = dr["Duration"].ToString();
                                string ActivateCustomerID = dr["ActivateCustomerID"].ToString();
                                string IsAlreadyApplyProfile = dr["IsAlreadyApplyProfile"].ToString();
                                string IsChangePlan = dr["IsChangePlan"].ToString();
                                string PackageName = dr["PackageName"].ToString();
                                string UM = dr["UM"].ToString();
                                if (InvoiceType == "Renew Invoice" && !string.IsNullOrEmpty(ActivateCustomerID) && ActivateCustomerID != "0")
                                {
                                    sql = "select RouterID, PPPoEUserName, PPPoEPassword, PPPoEProfileName, ActualProfile, ConnectivityStatus, ConnectionType, VLanID, VLanName, format(RenewDate, 'yyyy-MM-dd') RenewDate, [SID], ActivateDate from dbo.TblActivateCustomer where ";
                                    sql += "ActivateCustomerID =" + ActivateCustomerID;
                                    ds = html.LoadData(sql);
                                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                    {
                                        ServiceID = ds.Tables[0].Rows[0]["SID"].ToString();
                                        string ActivateDate = ds.Tables[0].Rows[0]["ActivateDate"] != DBNull.Value ? Convert.ToDateTime(ds.Tables[0].Rows[0]["ActivateDate"]).ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) : "";
                                        string RouterID = ds.Tables[0].Rows[0]["RouterID"].ToString();
                                        string VLanID = ds.Tables[0].Rows[0]["VLanID"].ToString();
                                        string VLanName = ds.Tables[0].Rows[0]["VLanName"].ToString();
                                        string ConnectionType = ds.Tables[0].Rows[0]["ConnectionType"].ToString();
                                        string ConnectivityStatus = ds.Tables[0].Rows[0]["ConnectivityStatus"].ToString();
                                        string PPPoEProfileName = ds.Tables[0].Rows[0]["ActualProfile"].ToString();
                                        string RenewDate = ds.Tables[0].Rows[0]["RenewDate"].ToString();
                                        string Comment = "Renew " + UM + "; " + DateTime.Parse(StartDate).ToString("dd-MMM-yyyy") + " - " + DateTime.Parse(EndDate).ToString("dd-MMM-yyyy") + "; Renew Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserFullName;
                                        string PPPoEUserName = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
                                        string PPPoEPassword = ds.Tables[0].Rows[0]["PPPoEPassword"].ToString();

                                        Mikrotik MK = new Mikrotik();
                                        MK.SetConnection(int.Parse(RouterID));
                                        if (ConnectionType == "PPPoE")
                                        {
                                            if (ConnectivityStatus == "41")
                                            {
                                                Comment = "Re-activated on " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserFullName;
                                                MK.ReActivateUser(PPPoEUserName, Comment);
                                            }
                                            if (IsChangePlan == "False" || IsChangePlan == "")
                                            {
                                                sqlresult.Result = ResultType.Successful;
                                            }
                                            else
                                            {
                                                DateTime SDate = DateTime.Parse(StartDate);
                                                if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                                {
                                                    Comment = "Customer Change Plan From: <b>" + PPPoEProfileName + "</b> TO <b>" + PackageName + "</b>; Apply Changed by: <b>" + html.UserFullName + "</b>; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                                                    PPPoEProfileName = PackageName;
                                                    MK.ClearProfileName(PPPoEUserName);
                                                    sqlresult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                                                }
                                            }
                                        }
                                        else if (ConnectionType == "Static")
                                        {
                                            sqlresult = MK.EnableVlanID(VLanID, Comment);
                                        }
                                        else
                                        {
                                            sqlresult.Result = ResultType.Successful;
                                        }

                                        if (sqlresult.Result == ResultType.Successful)
                                        {
                                            string invariantNow = easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                            sql = "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 40, ConnectionType = '" + ConnectionType + "', RenewBy = '" + html.UserFullName + "', RenewDate = '" + invariantNow + "', [ExpireDate] = '" + EndDate + "',";
                                            sql += "RouterID=" + RouterID + ", [SID] = '" + ServiceID + "', PPPoEPassword='" + PPPoEPassword + "', ActivateDate ='" + ActivateDate + "',ConnectivityStatusName = 'Active', IsAlreadyApplyProfile = 1 where InvoiceNo = '" + InvoiceNo + "'";
                                            sql += ";update dbo.TblCustomerInvoice set ConnectivityStatus = 40, ConnectivityStatusName = 'Active', RenewBy = '" + html.UserFullName + "', RenewDate = '" + invariantNow + "' ";
                                            sql += "where InvoiceNo = '" + InvoiceNo + "' and SystemID=" + html.SystemID + " and WarehouseID=" + html.WarehouseID + ";";
                                            if (IsChangePlan == "False" || IsChangePlan == "")
                                            {
                                                sql += "update dbo.TblActivateCustomer set InvoiceNo = '" + InvoiceNo + "', BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                                sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "', RenewBy='" + html.UserFullName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                                sql += ";";
                                                sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, ConnectionType, ";
                                                sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                                sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                                sql += "select ac.ActivateCustomerID, ac.ConnectionType, ac.CustomerID, ";
                                                sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, '" + html.UserFullName + "' ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "' LockedDate, ";
                                                sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                                sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "' from dbo.TblActivateCustomer ac ";
                                                sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                                sqlresult = html.SaveChangeData(sql, false, false);

                                                //send notification to telegram when customer services is renew
                                                sql = "select IsEnable from TblTelegramNotification where IsEnable = 1 and StatusID = 44 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
                                                ds = html.LoadData(sql);
                                                if (ds.Tables[0].Rows.Count > 0)
                                                {
                                                    sql = @"select top 1 c.CustomerID, ta.RouterID, ta.PPPoEUserName, ta.PPPoEPassword, ta.BillingInvoiceID, ta.ActivateCustomerID, c.CustomerCode CID,case when isnull(c.CustomerName, '') <> '' then c.CustomerName else c.CustomerNameLatin end CName, ta.ConnectionID, ta.SID, bf.CompanyContactMobileNumber,
                                                         (c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM +
                                                         case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName, ta.VLanName, ta.VLanIPAddress, ta.IPAddress CustomerIPAddress, ta.GetwayIP, ta.TicketNumber,
                                                         format(b.StartDate, 'dd-MM-yyyy') StartDate, format(b.EndDate, 'dd-MM-yyyy') EndDate, b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, ta.ConnectionType, ta.VLanID 
                                                         from dbo.TblActivateCustomer ta
                                                         inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID
                                                         inner join dbo.TblServices s on s.ServiceID = ta.ServiceID
                                                         inner join dbo.TblCustomerBillingInvoice b on b.ActivateCustomerID = ta.ActivateCustomerID 
                                                         inner join dbo.TblPackageDetail pd on pd.PackageDetailID = b.PackageDetailID
                                                         left join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = b.CustomerBillInformationID
                                                         where ta.ActivateCustomerID=@ActivateCustomerID and ta.ConnectivityStatus = 40 and b.StatusID not in(12,13) and format(b.RenewDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd') order by b.BillingInvoiceID desc";
                                                    Dictionary<string, string> para = new Dictionary<string, string>
                                                     {
                                                         { "DateNow", easternTimeNow.ToString("yyyyMMdd") },
                                                         { "ActivateCustomerID", ActivateCustomerID }
                                                     };
                                                    ds = html.LoadData(sql, false, para);

                                                    if (ds.Tables[0].Rows.Count > 0)
                                                    {
                                                        foreach (DataRow drow in ds.Tables[0].Rows)
                                                        {
                                                            CustomerID = drow["CustomerID"].ToString();
                                                            ConnectionID = drow["ConnectionID"].ToString();
                                                            string SID = drow["SID"].ToString();
                                                            CID = drow["CID"].ToString();
                                                            string CName = drow["CName"].ToString();
                                                            string CompanyContactMobileNumber = drow["CompanyContactMobileNumber"].ToString();
                                                            string TicketNumber = drow["TicketNumber"].ToString();
                                                            CustomerName = drow["CustomerName"].ToString();
                                                            PackageName = drow["PackageName"].ToString();
                                                            StartDate = drow["StartDate"].ToString();
                                                            EndDate = drow["EndDate"].ToString();
                                                            InvoiceNo = drow["InvoiceNo"].ToString();
                                                            RouterID = drow["RouterID"].ToString();
                                                            string PaymentDueDate = drow["PaymentDueDate"].ToString();

                                                            string msg = "«សេចក្តីជូនដំណឹងអំពី អ្នកបានបន្តការប្រើប្រាស់កញ្ចប់សេវាកម្មអ៊ិនធឺណិត»\n\n";
                                                            msg += "ឈ្មោះអតិថិជន: " + CName + "\n";
                                                            msg += "លេខសំគាល់អតិថិជន: " + CID + "\n";
                                                            msg += "ឈ្មោះកញ្ចប់សេវាកម្មៈ <b>" + PackageName + "</b>\n\n";
                                                            msg += "ថ្ងៃចាប់ផ្តើមៈ <b>" + StartDate + "</b>\n";
                                                            msg += "ថ្ងៃផុតសុពលភាពៈ <b>" + EndDate + "</b>\n";
                                                            msg += "\nសម្រាប់ពត៌មានបន្ថែមសូមទាក់ទង\nផ្នែកបម្រើអតិថិជន: +85586588777, +85578588777\n";
                                                            msg += "ផ្នែកបច្ចេកទេសៈ +85561876777\n";
                                                            msg += "គេហទំព័រ: www.nsmisp.com.kh\n";
                                                            msg += "\nសូមអរគុណ!";
                                                            CompanyContactMobileNumber = CompanyContactMobileNumber.Trim().Replace(" ", string.Empty);
                                                            var rg_ex = new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape("0"));
                                                            CompanyContactMobileNumber = rg_ex.Replace(CompanyContactMobileNumber, "+855", 1);

                                                            sql = @"select ct.TelegramNotificationID, ct.CustomerPhone, ct.FirstName, ct.LastName, ct.CustomerID
                                                                     from dbo.TblCustomerTelegram ct
                                                                     where isnull(ct.TelegramNotificationID,'') <> ''
                                                                     and 2 = (select [Data] from dbo.Split(ct.TelegramNotificationID, ',') where [Data] = 2)
                                                                     and ct.CustomerID = " + CustomerID + " and ct.IsUsingTelegram = 'Yes'";
                                                            ds = html.LoadData(sql);
                                                            if (ds.Tables.Count > 0)
                                                            {
                                                                if (ds.Tables[0].Rows.Count > 0)
                                                                {
                                                                    foreach (DataRow drs in ds.Tables[0].Rows)
                                                                    {
                                                                        string CustPhone = drs["CustomerPhone"].ToString();
                                                                        string CustFirstName = drs["FirstName"].ToString();
                                                                        string CustLastName = drs["LastName"].ToString();
                                                                        string CustID = drs["CustomerID"].ToString();
                                                                        SqlResult result = await notificationTelegram.NotificationToCustomer(CustPhone, CustFirstName, CustLastName, int.Parse(CustID), msg);
                                                                        string TelegramNotificationID = "2";
                                                                        sql = @"insert into dbo.TblCustomerTelegramMessage (DateSend, SendBy, CustomerID, ConnectionID, 
                                                                                 ActivateCustomerID, TelegramNotificationID, MessgeID, MessageText, MessageStatus, CustomerPhone)
                                                                                 values ('" + invariantNow + "','System', " + CustomerID + ",'" + ConnectionID + "'," + ActivateCustomerID + "," + TelegramNotificationID + ",'" + result.Value + "'," +
                                                                                                    "N'" + msg + "', '" + result.Message + "', '" + CustPhone + "')";
                                                                        result = html.SaveChangeData(sql, false, false);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                DateTime SDate = DateTime.Parse(StartDate);
                                                if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                                {
                                                    sql += "update dbo.TblActivateCustomer set IsApplyChangePlan = 1, InvoiceNo = '" + InvoiceNo + "' and BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                                    sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "', RenewBy='" + html.UserFullName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                                    sql += ";";
                                                    sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, BillingInvoiceID, ConnectionType, ";
                                                    sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                                    sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                                    sql += "select ac.ActivateCustomerID, ac.BillingInvoiceID, ac.ConnectionType, ac.CustomerID, ";
                                                    sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, '" + html.UserFullName + "' ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "' LockedDate, ";
                                                    sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                                    sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "' from dbo.TblActivateCustomer ac ";
                                                    sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                                    sqlresult = html.SaveChangeData(sql, false, false);

                                                    sql = "select * from dbo.TblActivateCustomer where ActivateCustomerID = " + ActivateCustomerID;
                                                    ds = html.LoadData(sql);
                                                    DataRow dr_data = ds.Tables[0].Rows[0];
                                                    string Ticket_Number = dr_data["TicketNumber"].ToString();
                                                    string Comment_s = @"Dear Team, <br><br>" + Comment + ".";
                                                    Comment_s += @"<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO GENERATE TICKET.";

                                                    sql = @"insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID,
                                                         Status, PriorityID, Priority, OpenTicketID, FromUserLoginID, FromUserName,
                                                         ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
                                                         select SupportProjectID, StatusID, [Status], PriorityID, [Priority], 
                                                         OpenTicketID, UserLoginID FromUserLoginID, CreateBy FromUserName, " + html.UserID + @" ReplyFromUserLoginID, '" + html.UserFullName + @"' ReplyFromUserName, 
                                                         N'" + Comment_s + "' Comment, '" + invariantNow + @"' [Date], '" + html.UserFullName + @"' CreateBy, '" + invariantNow + @"'CreateDate, " + html.SystemID + " SystemID, " + html.WarehouseID + @" WarehouseID, 
                                                         '' DescriptionChangeStatus
                                                         from dbo.TblSupportOpenTicket
                                                         where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;";
                                                    Dictionary<string, string> para = new Dictionary<string, string>
                                                     {
                                                         { "TicketNumber", Ticket_Number },
                                                         { "SystemID", html.SystemID },
                                                         { "WarehouseID", html.WarehouseID }
                                                     };
                                                    html.SaveChangeData(sql, false, para);

                                                    await notificationTelegram.NotificationTelegramBot((Comment_s + " " + Ticket_Number).Replace("<br>", "\n"));
                                                    await notificationTelegram.NotificationTelegramBot((Comment_s + " " + Ticket_Number).Replace("<br>", "\n"), -635916546);
                                                }
                                            }

                                            sql = "select top 1 cb.GrandTotal Amount, s.ServiceName, c.CustomerCode FirstName, c.CustomerName LastName, bi.CompanyEmail Email, bi.CompanyContactMobileNumber Phone, bi.InstallProvinceCity [Location], 'Activation by: " + html.UserFullName + "; Activate Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + "' Comment ";
                                            sql += ", b.PDFFileName, b.DestinationFolder, case when b.invTmp = 1 then b.ServiceName + ' - ' + b.PackageName else b.ServiceName end PackageName, ";
                                            sql += "format(b.StartDate, 'dd-MMM-yyyy') StartDate, format(b.EndDate, 'dd-MMM-yyyy') EndDate, b.ConnectionID, b.[SID] ";
                                            sql += "from dbo.TblCustomer c ";
                                            sql += "inner join dbo.TblCustomerBillInformation bi on bi.CustomerID = c.CustomerID ";
                                            sql += "inner join dbo.TblCustomerBillingInvoice b on b.CustomerID = bi.CustomerID and b.CustomerBillInformationID = bi.CustomerBillInformationID ";
                                            sql += "inner join dbo.TblServices s on s.ServiceID = b.ServiceID ";
                                            sql += "inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID and cb.InvoiceNo = b.InvoiceNo ";
                                            sql += "where b.InvoiceNo = '" + InvoiceNo + "' and c.CustomerID = " + CustomerID + " and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
                                            ds = html.LoadData(sql);
                                            ServiceID = ds.Tables[0].Rows[0]["SID"].ToString();
                                            ConnectionID = ds.Tables[0].Rows[0]["ConnectionID"].ToString();
                                            string ServiceName = ds.Tables[0].Rows[0]["ServiceName"].ToString();
                                            string FirstName = ds.Tables[0].Rows[0]["FirstName"].ToString();
                                            string LastName = ds.Tables[0].Rows[0]["LastName"].ToString();
                                            string Email = ds.Tables[0].Rows[0]["Email"].ToString();
                                            string Phone = ds.Tables[0].Rows[0]["Phone"].ToString();
                                            string Location = ds.Tables[0].Rows[0]["Location"].ToString();
                                            string PDFFileName = ds.Tables[0].Rows[0]["PDFFileName"].ToString();
                                            DestinationFolder = ds.Tables[0].Rows[0]["DestinationFolder"].ToString();
                                            PackageName = ds.Tables[0].Rows[0]["PackageName"].ToString();
                                            StartDate = ds.Tables[0].Rows[0]["StartDate"].ToString();
                                            EndDate = ds.Tables[0].Rows[0]["EndDate"].ToString();
                                            string Amount = ds.Tables[0].Rows[0]["Amount"].ToString();

                                            StringBuilder MsgBody = new StringBuilder();
                                            string Subject = "YOUR " + ServiceName.ToUpper() + " IS RENEW ON DATE: " + easternTimeNow.ToString("dd-MMM-yyyy HH:mm:ss tt");
                                            MsgBody.AppendLine(Subject);
                                            MsgBody.AppendLine("\nDear " + LastName + " [" + FirstName + "],\n");
                                            MsgBody.AppendLine("Greeting from NSM SOLUTION CO., LTD and thanks for using our services.\n");
                                            MsgBody.AppendLine("We would like to confirm your service is renew already on date: " + easternTimeNow.ToString("dd-MMM-yyyy HH:mm:ss tt"));
                                            MsgBody.AppendLine("\nYour current service: " + ServiceName);
                                            MsgBody.AppendLine("Package Name: " + PackageName);
                                            MsgBody.AppendLine("Service ID: " + ServiceID);
                                            MsgBody.AppendLine("Connection ID: " + ConnectionID);
                                            MsgBody.AppendLine("Start Date: " + StartDate);
                                            MsgBody.AppendLine("Expired Date: " + EndDate);
                                            MsgBody.AppendLine("Amount USD: " + Amount);
                                            MsgBody.AppendLine("Phone Number: " + Phone);
                                            if (Email != "")
                                                MsgBody.AppendLine("Email: " + Email);
                                            MsgBody.AppendLine("Renew By: " + html.UserFullName);
                                            MsgBody.AppendLine("\nThank you and best regards,\n");
                                            MsgBody.AppendLine("NSM SOLUTION AND MAINTENANCE CO.,LTD");
                                            MsgBody.AppendLine("#1159No1, Street Number 2, Phum Prek Tanu, Sangkat Chak Angre Leu, Khan Mean Chey, Phnom Penh.");
                                            MsgBody.AppendLine("Customer Service: 086 588 777, 078 588 777");
                                            MsgBody.AppendLine("Website: nsmisp.com.kh");
                                            Phone = Phone.Trim().Replace(" ", string.Empty);
                                            var regex = new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape("0"));
                                            Phone = regex.Replace(Phone, "+855", 1);

                                            MsgBody.Clear();

                                            MsgBody.AppendLine("\nDear TEAM,\n");
                                            MsgBody.AppendLine("We would like to confirm Customer Name: <b>" + LastName.ToUpper() + "[" + FirstName.ToUpper() + "]</b> is renew: " + easternTimeNow.ToString("dd-MMM-yyyy HH:mm:ss tt"));
                                            MsgBody.AppendLine("\n<b>Service ID: " + ServiceID + "</b>");
                                            MsgBody.AppendLine("<b>Connection ID: " + ConnectionID + "</b>");
                                            MsgBody.AppendLine("<b>Package Name: " + PackageName + "</b>\n");
                                            MsgBody.AppendLine("<b>Start Date: " + StartDate + "</b>");
                                            MsgBody.AppendLine("<b>Expired Date: " + EndDate + "</b>");
                                            MsgBody.AppendLine("<b>Phone Number: " + Phone + "</b>");
                                            if (Email != "")
                                                MsgBody.AppendLine("Email: " + Email);
                                            MsgBody.AppendLine("\n<b>Renew By: " + html.UserFullName + "</b> @" + DateTime.Parse(html.GetCurrentDate()).ToString("dd/MMM/yyyy hh:mm:ss tt"));
                                            //await notificationTelegram.NotificationTelegramBotToTopic(MsgBody.ToString(), -1004391832623, 0);
                                        }
                                    }
                                }
                                else if (InvoiceType == "Created Invoice")
                                {
                                    sql = "select RouterID, PPPoEUserName, PPPoEPassword, PPPoEProfileName, ConnectivityStatus, ConnectionType, VLanID, VLanName, format(RenewDate, 'yyyy-MM-dd') RenewDate from dbo.TblActivateCustomer where ";
                                    sql += "ActivateCustomerID =" + ActivateCustomerID;
                                    ds = html.LoadData(sql);
                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        string RouterID = ds.Tables[0].Rows[0]["RouterID"].ToString();
                                        string VLanID = ds.Tables[0].Rows[0]["VLanID"].ToString();
                                        string VLanName = ds.Tables[0].Rows[0]["VLanName"].ToString();
                                        string ConnectionType = ds.Tables[0].Rows[0]["ConnectionType"].ToString();
                                        string ConnectivityStatus = ds.Tables[0].Rows[0]["ConnectivityStatus"].ToString();
                                        string PPPoEProfileName = ds.Tables[0].Rows[0]["PPPoEProfileName"].ToString();
                                        string RenewDate = ds.Tables[0].Rows[0]["RenewDate"].ToString();
                                        string Comment = "New Customer With Profile Name: " + PPPoEProfileName;
                                        string PPPoEUserName = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
                                        string PPPoEPassword = ds.Tables[0].Rows[0]["PPPoEPassword"].ToString();

                                        Mikrotik MK = new Mikrotik();
                                        MK.SetConnection(int.Parse(RouterID));
                                        if (ConnectionType == "PPPoE")
                                        {
                                            if (ConnectivityStatus == "50")
                                            {
                                                sqlresult = MK.ClearProfileName(PPPoEUserName);
                                                Comment = "New Customer With Profile Name: " + PPPoEProfileName + " Date:" + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserFullName;
                                            }
                                            else if (ConnectivityStatus == "41")
                                            {
                                                Comment = "Your Services was re-activated on " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserFullName;
                                                sqlresult = MK.ReActivateUser(PPPoEUserName, Comment);
                                            }
                                        }
                                        else if (ConnectionType == "Static")
                                        {
                                            Comment = "New Customer With Vlan Name: " + VLanName;
                                            sqlresult = MK.EnableVlanID(VLanID, Comment);
                                        }
                                        else
                                        {
                                            sqlresult.Result = ResultType.Successful;
                                        }

                                        if (sqlresult.Result == ResultType.Successful)
                                        {
                                            sql = "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 40, ";
                                            sql += "ConnectivityStatusName = 'Active', IsAlreadyApplyProfile = 1 where InvoiceNo = '" + InvoiceNo + "'";
                                            sql += ";update dbo.TblCustomerInvoice set ConnectivityStatus = 40, ConnectivityStatusName = 'Active' ";
                                            sql += "where InvoiceNo = '" + InvoiceNo + "' and SystemID=" + html.SystemID + " and WarehouseID=" + html.WarehouseID + ";";
                                            if (IsChangePlan == "False" || IsChangePlan == "")
                                            {
                                                sql += "update dbo.TblActivateCustomer set InvoiceNo = '" + InvoiceNo + "', BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                                sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "', RenewBy='" + html.UserFullName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                                sql += ";";
                                                sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, ConnectionType, ";
                                                sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                                sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                                sql += "select ac.ActivateCustomerID, ac.ConnectionType, ac.CustomerID, ";
                                                sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, '" + html.UserFullName + "' ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "' LockedDate, ";
                                                sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                                sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "' from dbo.TblActivateCustomer ac ";
                                                sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                            }
                                            else
                                            {
                                                DateTime SDate = DateTime.Parse(StartDate);
                                                if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                                {
                                                    sql += "update dbo.TblActivateCustomer set IsApplyChangePlan = 1, InvoiceNo = '" + InvoiceNo + "' and BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                                    sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "', RenewBy='" + html.UserFullName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                                    sql += ";";
                                                    sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, BillingInvoiceID, ConnectionType, ";
                                                    sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                                    sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                                    sql += "select ac.ActivateCustomerID, ac.BillingInvoiceID, ac.ConnectionType, ac.CustomerID, ";
                                                    sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, '" + html.UserFullName + "' ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "' LockedDate, ";
                                                    sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                                    sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss") + "' from dbo.TblActivateCustomer ac ";
                                                    sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                                }
                                            }
                                            sqlresult = html.SaveChangeData(sql, false, false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return Json(new
                {
                    Message = sqlresult.Message,
                    Result = sqlresult.Result,
                    Data = sqlresult.Value,
                    OfficialReceiptData = OfficialReceiptData
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }

        private void ConfigureApiContext(int? systemID, int? warehouseID, int? userID = null, string userFullName = null, string domainID = null)
        {
            var context = System.Web.HttpContext.Current;
            if (context == null) return;

            var customHeader = context.Request.Headers["x-custom-header"];
            Dictionary<string, object> dict = null;

            if (!string.IsNullOrEmpty(customHeader))
            {
                try
                {
                    dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(customHeader);
                }
                catch { }
            }

            if (dict == null)
            {
                dict = new Dictionary<string, object>();
            }

            if (systemID.HasValue) dict["SystemID"] = systemID.Value.ToString();
            if (warehouseID.HasValue) dict["WarehouseID"] = warehouseID.Value.ToString();
            if (userID.HasValue) dict["UserID"] = userID.Value.ToString();
            if (!string.IsNullOrEmpty(userFullName)) dict["UserFullName"] = userFullName;
            if (!string.IsNullOrEmpty(domainID)) dict["DomainID"] = domainID;

            // Apply sensible defaults if not present
            if (!dict.ContainsKey("SystemID") || string.IsNullOrEmpty(dict["SystemID"]?.ToString())) dict["SystemID"] = "1";
            if (!dict.ContainsKey("WarehouseID") || string.IsNullOrEmpty(dict["WarehouseID"]?.ToString())) dict["WarehouseID"] = "1";
            if (!dict.ContainsKey("UserID") || string.IsNullOrEmpty(dict["UserID"]?.ToString())) dict["UserID"] = "1";
            if (!dict.ContainsKey("UserFullName") || string.IsNullOrEmpty(dict["UserFullName"]?.ToString())) dict["UserFullName"] = "API User";
            if (!dict.ContainsKey("DomainID") || string.IsNullOrEmpty(dict["DomainID"]?.ToString())) dict["DomainID"] = "001";

            string jsonHeader = JsonConvert.SerializeObject(dict);

            try
            {
                var headers = context.Request.Headers;
                var isReadOnlyField = typeof(System.Collections.Specialized.NameValueCollection)
                    .GetField("isReadOnly", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (isReadOnlyField != null)
                {
                    isReadOnlyField.SetValue(headers, false);
                    headers["x-custom-header"] = jsonHeader;
                    isReadOnlyField.SetValue(headers, true);
                }
            }
            catch (Exception)
            {

            }
        }

        private JObject GetScalarPropertiesAsJObject(object obj)
        {
            var jObj = new JObject();
            if (obj == null) return jObj;

            var properties = obj.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in properties)
            {
                // Skip virtual properties (navigation properties are virtual in Entity Framework)
                if (prop.GetAccessors()[0].IsVirtual) continue;

                var type = prop.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

                // Check if it's a scalar type (primitive, enum, string, decimal, datetime, guid)
                if (underlyingType.IsPrimitive ||
                    underlyingType.IsEnum ||
                    underlyingType == typeof(string) ||
                    underlyingType == typeof(decimal) ||
                    underlyingType == typeof(DateTime) ||
                    underlyingType == typeof(Guid))
                {
                    var value = prop.GetValue(obj);
                    jObj[prop.Name] = value != null ? JToken.FromObject(value) : null;
                }
            }
            return jObj;
        }
    }

}
