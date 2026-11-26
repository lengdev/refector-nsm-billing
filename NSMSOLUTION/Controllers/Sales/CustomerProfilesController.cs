using HtmlAgilityPack;
using MVC;
using NewInventory.Controllers;
using Newtonsoft.Json;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;
using Telegram.Bot;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class CustomerProfilesController : Controller
    {
        // GET: CustomerProfiles
        public ActionResult Index(int ID, int ISSO = 0, bool ISPaidInvoice = false, string InvoiceNo = "")
        {
            bool roleEditCustomer = HttpContext.GetOwinContext().Request.User.IsInRole("Edit Customer");
            ViewBag.roleEditCustomer = roleEditCustomer;
            ViewBag.CustomerID = ID;
            ViewBag.ISSO = ISSO;
            HTML html = new HTML();
            bool roleManager = HttpContext.GetOwinContext().Request.User.IsInRole("Show Customer Invoice Profile");
            ViewBag.ShowCustomerInvoiceProfile = roleManager;
            bool roleAddAttachmentFile = HttpContext.GetOwinContext().Request.User.IsInRole("Add Attachment File");
            ViewBag.roleAddAttachmentFile = roleAddAttachmentFile;
            bool roleCustomerProfileAttachment = HttpContext.GetOwinContext().Request.User.IsInRole("Customer Profile Attachment File");
            ViewBag.roleCustomerProfileAttachment = roleCustomerProfileAttachment;
            bool NewServiceProduct = HttpContext.GetOwinContext().Request.User.IsInRole("New Service & Product");
            ViewBag.NewServiceProduct = NewServiceProduct;
            bool ApprovalSaleOrder = HttpContext.GetOwinContext().Request.User.IsInRole("Approval Sale Order");
            ViewBag.ApprovalSaleOrder = ApprovalSaleOrder;
            bool ShowQuotationinCustProfile = HttpContext.GetOwinContext().Request.User.IsInRole("Show Quotation in Cust Profile");
            ViewBag.ShowQuotationinCustProfile = ShowQuotationinCustProfile;
            bool ShowDeviceInfoinCustProfile = HttpContext.GetOwinContext().Request.User.IsInRole("Show Device Info in Cust Profile");
            ViewBag.ShowDeviceInfoinCustProfile = ShowDeviceInfoinCustProfile;
            bool ShowTicketinCustProfile = HttpContext.GetOwinContext().Request.User.IsInRole("Show Ticket in Cust Profile");
            ViewBag.ShowTicketinCustProfile = ShowTicketinCustProfile;
            bool ShowServicesProductsinCustProfile = HttpContext.GetOwinContext().Request.User.IsInRole("Show Services & Products in Cust Profile");
            ViewBag.ShowServicesProductsinCustProfile = ShowServicesProductsinCustProfile;
            bool AgencyCommissionTap = HttpContext.GetOwinContext().Request.User.IsInRole("Agency Commission Tap");
            ViewBag.AgencyCommissionTap = AgencyCommissionTap;
            bool CustomerFeedback = HttpContext.GetOwinContext().Request.User.IsInRole("Customer Feedback");
            ViewBag.CustomerFeedback = CustomerFeedback;

            bool showDelayPayment = HttpContext.GetOwinContext().Request.User.IsInRole("Add Delay Payment");
            ViewBag.showDelayPayment = showDelayPayment;

            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            //			SQL = @"select c.CustomerCode + ' - ' + case when lower(c.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Male' then N'លោក ' + replace(c.CustomerName, N'លោក','')
            //when lower(c.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Female' then N'អ្នកស្រី ' + replace(replace(c.CustomerName, N'លោកស្រី​',''), N'អ្នកស្រី','')
            //when c.CustomerGender = 'Male' then 'Mr. ' + replace(replace(c.CustomerName, 'Mr. ', ''), 'Mr.','')
            //when c.CustomerGender = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(c.CustomerName, 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
            //else c.CustomerName end  CustomerName, 
            //c.CustomerAddress + case when c.CustomerCommune <> '' then + ', ' + c.CustomerCommune else '' end + case when c.CustomerDistrict <> '' then + ', ' + c.CustomerDistrict else '' end + case when c.CustomerProvinceCity <> '' then + ', ' + c.CustomerProvinceCity else '' end [Address],
            //c.CustomerContactMobileNumber, c.CustomerContactEmail, c.CustomerIdentification, case when '" + html.DomainID + @"' = '001,002' and c.NewRegisterDate is not null then format(c.NewRegisterDate, 'dd/MM/yyyy') else format(c.RegisterDate, 'dd/MM/yyyy') end RegisterDate,
            //c.CustomerNationality, c.CustomerIdentification, (e.Salutation + e.FullName) EmployeeName, ct.CustomerType,
            //i.IndustryType, o.OrderType, c.CustomerID, isnull(c.AgencyCommissionID,0) AgencyCommissionID, c.ATTName
            //from dbo.TblCustomer c
            //inner join dbo.TblLeads e on e.LeadID = c.EmployeeID
            //inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
            //inner join dbo.TblIndustryType i on i.IndustryTypeID = c.IndustryTypeID
            //left join dbo.TblOrderType o on o.OrderTypeID = c.CustomerTypeID
            //where c.CustomerID = " + ID + " and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%' ";
            //			ViewBag.CustInfo = html.JsonData(SQL);
            string SQL = "select s.ServiceName, (p.PackageName  + ' - ' + CONVERT(varchar, cd.Bandwidth) + ' Mbps - ' + cd.UM) PackageName, ";
            SQL += "cd.Duration, cd.FreeDuration, cd.Bandwidth, cd.DowSpeed, cd.UpSpeed, cd.DepositFee, cd.BillCycle ";
            SQL += ", convert(varchar, CompletedDate, 103) CompletedDate, convert(varchar, StartChargingDate, 103) StartChargingDate, StartChargingDate InvoiceDate ";
            SQL += "from dbo.TblCustomerDetail cd ";
            SQL += "inner join dbo.TblServices s on cd.ServiceID = s.ServiceID ";
            SQL += "inner join dbo.TblPackages p on p.PackageID = cd.PackageID ";
            SQL += "where cd.CustomerID = " + ID;
            ViewBag.CustDetail = html.JsonData(SQL);
            SQL = "select sum(cb.DepositFee) Deposit, sum(cb.GrandTotal) Total, sum(cb.Unpaid) Unpaid, convert(varchar, NextInvoiceDate, 103) NextInvoiceDate from dbo.TblCustomerBalance cb ";
            SQL += "where cb.CustomerID = " + ID + " and cb.StatusID = 11 group by convert(varchar, NextInvoiceDate, 103)";
            ViewBag.CustDeposit = html.JsonData(SQL);
            SQL = "select top 1 format(e.ExchangeRate, '#,#') ExchangeRate, format(e.DateEntry, 'dd-MM-yyyy hh:mm tt') DateEntry from dbo.TblExchangeRate e inner join dbo.TblCurrency c on c.CurrencyID = e.CurrencyID where c.CurrencyName = 'Riel' and e.SystemID = " + html.SystemID + " and e.WarehouseID = " + html.WarehouseID + " and e.DomainID like '%" + html.DomainID + "%' order by format(e.DateEntry, 'yyyyMMdd') desc";
            ViewBag.ExchangeRate = html.JsonConvertDataTable(html.LoadData(SQL).Tables[0]);

            SQL = @"select cb.InvoiceNo, c.CustomerName, cb.GrandTotal, c.CustomerID, cb.[Type], '" + ISPaidInvoice + @"' ISPaidInvoice
			from dbo.TblCustomerBalance cb
			inner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID
			where cb.StatusID = 11 and cb.CustomerID = " + ID + " and cb.InvoiceNo=N'" + InvoiceNo + "'";
            ViewBag.ISPaidInvoice = html.JsonData(SQL);

            //dynamic AgencyID = new JavaScriptSerializer().DeserializeObject(ViewBag.CustInfo);
            //string AgencyCommissionID = AgencyID[0]["CustomerID"].ToString();
            //string sql = @"select AgencyCommissionID, FullName AgencyName, HP AgencyPhone,
            //CommissionType AgencyCommissionType, [Address] AgencyAddress,
            //BankName AgencyBankName, AccountName AgencyAccountName, 
            //AccountNumber AgencyAccountNumber, Memo CustomerNoted, CommissionAmountUSD, CommissionAmountP
            //from dbo.TblAgencyCommission where CustomerID = " + ID;
            //ViewBag.AgencyCommissionData = html.JsonData(sql);
            //sql = "select distinct tc.AgencyCommissionID, tc.FullName, tc.HP, tc.[Address], tc.BankName,\r\ntc.AccountName, tc.AccountNumber\r\nfrom dbo.TblAgencyCommission tc where tc.FullName <> '' and tc.SystemID = " + html.SystemID + " and tc.WarehouseID = " + html.WarehouseID + " and tc.DomainID like '%" + html.DomainID + "%' order by FullName";
            //ViewBag.AgencyCommission = html.JsonData(sql);
            return View();
        }
        public JsonResult LoadCustInfo(int ID)
        {
            HTML html = new HTML();
            string SQL = @"select case when isnull(c.CustomerCode,'') <> '' then  isnull(c.CustomerCode,'') + ' - ' else '' end + case when lower(c.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Male' then N'លោក ' + replace(c.CustomerName, N'លោក','')
when lower(c.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Female' then N'អ្នកស្រី ' + replace(replace(c.CustomerName, N'លោកស្រី​',''), N'អ្នកស្រី','')
when c.CustomerGender = 'Male' then 'Mr. ' + replace(replace(c.CustomerName, 'Mr. ', ''), 'Mr.','')
when c.CustomerGender = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(c.CustomerName, 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
else c.CustomerName end  CustomerName, 
c.CustomerAddress + case when c.CustomerCommune <> '' then + ', ' + c.CustomerCommune else '' end + case when c.CustomerDistrict <> '' then + ', ' + c.CustomerDistrict else '' end + case when c.CustomerProvinceCity <> '' then + ', ' + c.CustomerProvinceCity else '' end [Address],
c.CustomerContactMobileNumber CustomerMobile, c.CustomerContactEmail CustomerEmail, c.CustomerIdentification, case when '" + html.DomainID + @"' = '001,002' and c.NewRegisterDate is not null then format(c.NewRegisterDate, 'dd/MM/yyyy') else format(c.RegisterDate, 'dd/MM/yyyy') end RegisterDate,
c.CustomerNationality, c.CustomerIdentification, (e.Salutation + e.FullName) EmployeeName, isnull(ct.CustomerType,'') CustomerType,
i.IndustryType, isnull(o.OrderType,'') OrderType, c.CustomerID, isnull(c.AgencyCommissionID,0) AgencyCommissionID, c.ATTName, c.RegisterBy, isnull(c.CustomerCode,'') CustomerCode, c.CustomerGroupID, c.CustomerGroup, c.CustomerName CustName, c.CustomerGender,
c.CustomerAddress, c.CustomerCommune, c.CommuneID, c.CustomerDistrict, c.DistrictID, c.CustomerProvinceCity, c.ProvinceCityID
from dbo.TblCustomer c
inner join dbo.TblLeads e on e.LeadID = c.EmployeeID
left join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
left join dbo.TblIndustryType i on i.IndustryTypeID = c.IndustryTypeID
left join dbo.TblOrderType o on o.OrderTypeID = c.CustomerTypeID
where c.CustomerID = @CustomerID";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult LoadCustNoted(int ID)
        {
            HTML html = new HTML();
            string SQL = "select isnull(CustomerNoted, '') CustomerNoted from dbo.TblCustomer ";
            SQL += "where CustomerID = @CustomerID";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveCustNoted(int ID, string CustomerNoted)
        {
            HTML html = new HTML();
            string SQL = "update dbo.TblCustomer set CustomerNoted = @CustomerNoted where CustomerID = @CustomerID";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() },
                { "CustomerNoted", HttpUtility.UrlDecode(CustomerNoted) }
            };
            SqlResult result = html.SaveChangeData(SQL, false, Para);
            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCompanyInfo(int ID)
        {
            HTML html = new HTML();
            string SQL = "select CompanyName, CompanyNameKH, CompanyDeskPhone, CompanyMobile, ";
            SQL += "VATNumber, t.IndustryType, c.IndustryTypeID BusinessTypeID, c.CompanyAddress, c.CompanyCommune, ";
            SQL += "c.CompanyDistrict, c.CompanyProvinceCity, c.CompanyContactName, ";
            SQL += "c.CompanyContactPosition, c.CompanyContactMobileNumber ";
            SQL += "from dbo.TblCustomerCompany c ";
            SQL += "inner join dbo.TblIndustryType t on t.IndustryTypeID = c.IndustryTypeID ";
            SQL += "where c.CustomerID = @CustomerID";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustInv(int ID)
        {
            HTML html = new HTML();
            string SQL = "select ROW_NUMBER() OVER(ORDER BY CustomerBalanceID desc) AS[No], ('<a href=\"javascript:void(0);\" onclick=\"PrintInvoice(" + ID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +','''',false,this);\">' + InvoiceNo + '</a>') InvNo, convert(varchar, InvoiceDate, 103) InvoiceDate, ";
            SQL += "DATEDIFF(day, InvoiceDate, GETDATE())[Day], SubTotal, VAT, Total, DiscountP, ";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, '<a name=\"Payment\" href=\"javascript:void(0);\" onclick=\"Payment(''' + InvoiceNo + ''','''+ case when CompanyName <> '' then CompanyName else CustomerName end +''','''+format(Unpaid,'#,#.00')+''',10, '+ convert(varchar, CustomerID) +');\">Pay Now</a>' Payment ";
            SQL += ", format(PaidDate, 'dd/MM/yyyy HH:mm:ss') PaidDate from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 11"; //11 = unpaid invoice
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustInvoice(int ID)
        {
            HTML html = new HTML();
            string SQL = "select ROW_NUMBER() OVER(ORDER BY CustomerBalanceID desc) AS[No], ('<a name=\"APreviewInv\" href=\"CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "&ID=" + ID + "&InvNo='+InvoiceNo+'&ISVAT='+ convert(varchar, IsIncludedVAT) +'&StatusID='+ convert(varchar, StatusID) +'&AutoPrint=false&index=0&InvoiceType=UnpaidInvoice\" onclick=\"PrintInvoice(" + ID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +','''',false,this);\">' + InvoiceNo + '</a>') InvNo, InvoiceDate, ";
            SQL += "case when DATEDIFF(day, InvoiceDate, GETDATE()) > 0 then DATEDIFF(day, InvoiceDate, GETDATE()) else 0 end [Day], PaymentDueDate, SubTotal, VAT, Total, DiscountP, [Type],";
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, '<a name=\"Payment\" href=\"javascript:void(0);\" onclick=\"Payment(''' + InvoiceNo + ''',`'+ case when CompanyName <> '' then CompanyName when isnull(CustomerName, '') <> '' then CustomerName else (select top 1 c.CustomerNameLatin from dbo.TblCustomer c where c.CustomerID = dbo.TblCustomerBalance.CustomerID) end +'`,'''+format(Unpaid,'#,#.00')+''',10, '+ convert(varchar, CustomerID) +','''+[Type]+''');\">Pay Now</a>' Payment ";
            SQL += ", format(PaidDate, 'dd-MM-yy HH:mm:ss tt') PaidDate, ConnectionID, CommentInvoice from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 11 and [Type] <> 'Sale Order Invoice' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'"; //11 = unpaid invoice
            html.GH.SQL = SQL;
            html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustInvoice";
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            html.ShowPrintOnTopTable = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                //new GridField() { Header = "Connection ID", FieldName = "ConnectionID", ClassName="text-nowrap" },
                new GridField() { Header = "Invoice#", FieldName = "InvNo", ClassName="text-nowrap" },
                //new GridField() { Header = "IsVAT", FieldName = "IsIncludedVAT", TextAlign = "center" },
                new GridField() { Header = "Date", FieldName = "InvoiceDate", TextAlign = "center", Format="dd-MM-yy" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", TextAlign = "center", Format="dd-MM-yy" },
                new GridField() { Header = "Paid Date", FieldName = "PaidDate", TextAlign = "center", ClassName="text-nowrap" },
                new GridField() { Header = "Day", FieldName = "Day", TextAlign = "center" },
                //new GridField() { Header = "Dis. %", FieldName = "DiscountP", TextAlign = "center" },
                //new GridField() { Header = "Dis. $", FieldName = "DiscountUSD", TextAlign = "center" },
                new GridField() { Header = "Total", FieldName = "GrandTotal", TextAlign = "center", Format="#,#.00" },
                new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center", Format="#,#.00" },
                new GridField() { Header = "Unpaid", FieldName = "Unpaid", TextAlign = "center",
                                  IsTotalFooter=true, Colspan = 7, TotalLabel="Total:", Format="#,#.00"
                },
                new GridField() { Header = "Memo", FieldName = "CommentInvoice", TextAlign = "center" },
                new GridField() { Header = "Payment", FieldName = "Payment", TextAlign = "center" },
                new GridField() { Header = "Action", FieldName = "InvoiceNo", TextAlign = "Center",
                    ActionName="Proforma To Official Invoice",
                    ActionIcon="fas fa-file-invoice",
                    ActionClickFunction="OfficialInvoice(this)",
                    ShowActionNameOnConditionValue="ProformaInvoice",
                    ShowActionNameOnConditionField="Type",
                    //HideEditOnActionNameNotMatch=true,
                    IsModifyField = true, EditFunction ="Edit_TblRow(this);",
                    DeletedCallBackFunction="LoadCustInvoice(" + ID + ")",
                    DeleteUseProcedureName="SP_DELETE_CUSTOMER_INVOICE",
                }
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadCustomerInvoices") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustSaleOrderInvoice(int ID)
        {
            HTML html = new HTML();
            bool ApprovalSaleOrder = HttpContext.GetOwinContext().Request.User.IsInRole("Approval Sale Order");
            string SQL = "select ROW_NUMBER() OVER(ORDER BY CustomerBalanceID desc) AS[No], ('<a name=\"APreviewInv\" href=\"CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "&ID=" + ID + "&InvNo='+InvoiceNo+'&ISVAT='+ convert(varchar, IsIncludedVAT) +'&StatusID='+ convert(varchar, StatusID) +'&AutoPrint=false&index=0&InvoiceType=UnpaidInvoice\" onclick=\"PrintInvoice(" + ID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +','''',false,this);\">' + InvoiceNo + '</a>') InvNo, InvoiceDate, ";
            SQL += "case when DATEDIFF(day, InvoiceDate, GETDATE()) > 0 then DATEDIFF(day, InvoiceDate, GETDATE()) else 0 end [Day], PaymentDueDate, SubTotal, VAT, Total, DiscountP, [Type],";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, '<a name=\"Payment\" href=\"javascript:void(0);\" onclick=\"Payment(''' + InvoiceNo + ''','''+ case when CompanyName <> '' then CompanyName else CustomerName end +''','''+format(Unpaid,'#,#.00')+''',10, '+ convert(varchar, CustomerID) +','''+[Type]+''');\">Pay Now</a>' Payment ";
            SQL += ", format(PaidDate, 'dd-MM-yy HH:mm:ss tt') PaidDate, ConnectionID, InvoiceStatusID, case when InvoiceStatusID = 90 then '<span class=\"badge bg-primary\">' + InvoiceStatus +'</span>' else '<span class=\"badge bg-danger\">' + InvoiceStatus + '</span>' end InvoiceStatus, CreateBy, ApproveBy, ApproveDate from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 11 and [Type] = 'Sale Order Invoice'"; //11 = unpaid invoice
            html.GH.SQL = SQL;
            html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustInvoice";
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Sale Order Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Sale Order Invoice"))
                html.AllowDelete = true;
            html.ShowPrintOnTopTable = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                //new GridField() { Header = "Connection ID", FieldName = "ConnectionID", ClassName="text-nowrap" },
                new GridField() { Header = "Invoice#", FieldName = "InvNo", ClassName="text-nowrap" },
                new GridField() { Header = "Create By", FieldName = "CreateBy", ClassName="text-nowrap" },
                //new GridField() { Header = "IsVAT", FieldName = "IsIncludedVAT", TextAlign = "center" },
                new GridField() { Header = "Date", FieldName = "InvoiceDate", TextAlign = "center", Format="dd-MM-yy" },
                //new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", TextAlign = "center", Format="dd-MM-yy" },
                //new GridField() { Header = "Paid Date", FieldName = "PaidDate", TextAlign = "center", ClassName="text-nowrap" },
                new GridField() { Header = "Day", FieldName = "Day", TextAlign = "center" },
                //new GridField() { Header = "Dis. %", FieldName = "DiscountP", TextAlign = "center" },
                //new GridField() { Header = "Dis. $", FieldName = "DiscountUSD", TextAlign = "center" },
                new GridField() { Header = "Total", FieldName = "GrandTotal", TextAlign = "center", Format="#,#.00" },
                new GridField() { Header = "Status", FieldName = "InvoiceStatus", TextAlign = "center" },
                new GridField() { Header = "Approve By", FieldName = "ApproveBy" },
                new GridField() { Header = "ApproveDate", FieldName = "ApproveDate", TextAlign = "center", Format="dd-MM-yy HH:mm:ss tt" },
                //new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center", Format="#,#.00" },
                //new GridField() { Header = "Unpaid", FieldName = "Unpaid", TextAlign = "center",
                  //                IsTotalFooter=true, Colspan = 7, TotalLabel="Total:", Format="#,#.00"
                //},
                //new GridField() { Header = "Payment", FieldName = "Payment", TextAlign = "center" },
                new GridField() { Header = "Action", FieldName = "InvoiceNo", TextAlign = "Center",
                    ActionName= ApprovalSaleOrder==true?"Approve":"",
                    ActionIcon=ApprovalSaleOrder==true?"fa-sharp fa-regular fa-ballot-check":"",
                    ActionClickFunction=ApprovalSaleOrder==true?"ApproveSaleOrder(this)":"",
                    ShowActionNameOnConditionValue=ApprovalSaleOrder==true?"101":"",
                    ShowActionNameOnConditionField=ApprovalSaleOrder==true?"InvoiceStatusID":"",
                    //HideEditOnActionNameNotMatch=true,
                    IsModifyField = true, EditFunction ="Edit_Tbl_Row(this);",
                    DeletedCallBackFunction="LoadCustInvoice("+ID+",'loadpi');",
                    DeleteUseProcedureName="SP_DELETE_CUSTOMER_INVOICE",
                }
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadCustomerInvoices") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustRollBackInvoice(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            string SQL = "select ROW_NUMBER() OVER(ORDER BY format(PaidDate, 'yyyy/MM/dd HH:mm:ss') desc) AS[No], ('<a href=\"javascript:void(0);\" onclick=\"PrintInvoice(" + ID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +');\">' + InvoiceNo + '</a>') InvNo, convert(varchar, InvoiceDate, 103) InvoiceDate, ";
            SQL += "DATEDIFF(day, InvoiceDate, GETDATE())[Day], SubTotal, VAT, Total, DiscountP, ";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, '<a href=\"javascript:void(0);\" onclick=\"Payment(''' + InvoiceNo + ''',`'+ case when CompanyName <> '' then CompanyName else CustomerName end +'`,'''+format(GrandTotal,'#,#.00')+''',11," + ID + ");\">Rollback</a>' Payment ";
            SQL += ", format(PaidDate, 'dd/MM/yyyy HH:mm:ss') PaidDate, PaidBy from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 10 and DomainID like '%" + html.DomainID + "%'"; //10 = paid invoice
            html.OrderBy = "format(PaidDate, 'yyyy/MM/dd HH:mm:ss') desc";
            html.GH.SQL = SQL;
            //html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustRollback_Invoice";
            //html.JqueryDataTable = true;
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                new GridField() { Header = "Inv.No", FieldName = "InvNo", ClassName="text-nowrap" },
                new GridField() { Header = "Inv.Date", FieldName = "InvoiceDate", TextAlign = "center", ClassName="text-nowrap" },
                new GridField() { Header = "PaidDate", FieldName = "PaidDate", TextAlign = "center", ClassName="text-nowrap" },
                new GridField() { Header = "Day", FieldName = "Day", TextAlign = "center" },
                new GridField() { Header = "PaidBy", FieldName = "PaidBy" },
                new GridField() { Header = "Total", FieldName = "GrandTotal", TextAlign = "center", Format="#,#.00" },
                //new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center",
                    //IsTotalFooter=true, Colspan = 6, TotalLabel="Total:"
                //},
                new GridField() { Header = "Payment", FieldName = "Payment", TextAlign = "center" }
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadRollback_Inv") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustPaidInvoice(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            string SQL = "select ROW_NUMBER() OVER(ORDER BY format(PaidDate, 'yyyy/MM/dd HH:mm:ss') desc) AS[No], ConnectionID, ('<a name=\"APreviewInv\" href=\"CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "&ID=" + ID + "&InvNo='+InvoiceNo+'&ISVAT='+ convert(varchar, IsIncludedVAT) +'&StatusID='+ convert(varchar, StatusID) +'&AutoPrint=false&index=0&InvoiceType=PaidInvoice&DomainID=" + html.DomainID + "\" onclick=\"PrintInvoice(" + ID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +','''',false, this,''PaidInvoice'');\">' + InvoiceNo + '</a>') InvNo, convert(varchar, InvoiceDate, 103) InvoiceDate, ";
            SQL += "DATEDIFF(day, InvoiceDate, GETDATE())[Day], SubTotal, VAT, Total, DiscountP, ";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, '<a href=\"javascript:void(0);\" onclick=\"Payment(''' + InvoiceNo + ''','''+ case when CompanyName <> '' then CompanyName else CustomerName end +''','''+format(GrandTotal,'#,#.00')+''',11);\">Roll Back</a>' Payment ";
            SQL += ", format(PaidDate, 'dd/MM/yyyy HH:mm:ss') PaidDate, PaidBy, '<a name=\"APreviewInv\" href=\"Payment/OfficialReceipt/?PaymentID=' + convert(varchar, PaymentID) + '&CompanyID=' + convert(varchar, InvoiceTmpID) + '&OfficialReceiptNo=' + ReceiptNo + '&InvoiceNo=' + InvoiceNo + '&SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "\" onclick=\"showOfficialReceipt('+ convert(varchar, PaymentID) + ',' + convert(varchar, InvoiceTmpID) + ', ''' + ReceiptNo +''', '''+ InvoiceNo + ''');\">'+ ReceiptNo + '</a>' ReceiptNo from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 10 and DomainID like '%" + html.DomainID + "%'";//10 = paid invoice
            html.GH.SQL = SQL;
            html.OrderBy = "format(PaidDate, 'yyyy/MM/dd HH:mm:ss') desc";
            //html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustPaidInvoice";
            //html.JqueryDataTable = true;
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                //new GridField() { Header = "Connection ID", FieldName = "ConnectionID", ClassName="text-nowrap" },
                new GridField() { Header = "Invoice#", FieldName = "InvNo", ClassName="text-nowrap" },
                new GridField() { Header = "Receipt#", FieldName = "ReceiptNo", ClassName="text-nowrap" },
                //new GridField() { Header = "IsVAT", FieldName = "IsIncludedVAT", TextAlign = "center" },
                new GridField() { Header = "Date", FieldName = "InvoiceDate", TextAlign = "center" },
                new GridField() { Header = "Paid Date", FieldName = "PaidDate", TextAlign = "center", ClassName="text-nowrap" },
                new GridField() { Header = "Day", FieldName = "Day", TextAlign = "center" },
                //new GridField() { Header = "Dis. %", FieldName = "DiscountP", TextAlign = "center" },
                //new GridField() { Header = "Dis. $", FieldName = "DiscountUSD", TextAlign = "center" },
                new GridField() { Header = "Paid By", FieldName = "PaidBy" },
                new GridField() { Header = "Total", FieldName = "GrandTotal",
                    TextAlign = "center", Format = "#,#.00",
                    IsTotalFooter=true, Colspan = 6, TotalLabel="Total:"},
                //new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center",
                //    IsTotalFooter=true, Colspan = 6, TotalLabel="Total:"
                //},
                //new GridField() { Header = "Unpaid", FieldName = "Unpaid", TextAlign = "center",
                //                  IsTotalFooter=true, Colspan = 7, TotalLabel="Total:"
                //},
                //new GridField() { Header = "Payment", FieldName = "Payment", TextAlign = "center" },
                //new GridField() { Header = "Action", FieldName = "InvoiceNo", TextAlign = "Center",
                //    IsModifyField = true, EditFunction ="Edit_Tbl_Row(this);",
                //    DeletedCallBackFunction="LoadCustInvoice(" + ID + ")",
                //    DeleteUseProcedureName="SP_DELETE_CUSTOMER_INVOICE"
                //}
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadPaidInv") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustCancelInvoice(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            string SQL = "select ROW_NUMBER() OVER(ORDER BY format(CancelDate, 'yyyy/MM/dd HH:mm:ss') desc) AS[No], ('<a target=\"_blank\" href=\"CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "&ID=" + ID + "&InvNo='+InvoiceNo+'&ISVAT='+ convert(varchar, IsIncludedVAT) +'&StatusID='+ convert(varchar, StatusID) +'&AutoPrint=false&index=0&InvoiceType=PaidInvoice\">' + InvoiceNo + '</a>') InvNo, convert(varchar, InvoiceDate, 103) InvoiceDate, ";
            SQL += "SubTotal, CancelComment, VAT, Total, DiscountP, ";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo ";
            SQL += ", format(CancelDate, 'dd/MM/yyyy HH:mm:ss') CancelDate, CancelBy from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 13 and DomainID like '%" + html.DomainID + "%'";//13 = cancel invoice
            html.GH.SQL = SQL;
            html.OrderBy = "format(CancelDate, 'yyyy/MM/dd HH:mm:ss') desc";
            //html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustCancelInvoice";
            //html.JqueryDataTable = true;
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                new GridField() { Header = "Inv.No", FieldName = "InvNo", ClassName="text-nowrap" },
                new GridField() { Header = "Cancel Date", FieldName = "CancelDate", TextAlign = "center" },
                new GridField() { Header = "Cancel By", FieldName = "CancelBy", TextAlign = "center", ClassName="text-nowrap" },
                new GridField() { Header = "Comment", FieldName = "CancelComment", ClassName="text-nowrap" },
                new GridField() { Header = "Total", FieldName = "GrandTotal",
                    TextAlign = "right", Format = "#,#.00",
                    IsTotalFooter=true, Colspan = 4, TotalLabel="Total:"},
                //new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center",
                //    IsTotalFooter=true, Colspan = 6, TotalLabel="Total:"
                //},
                //new GridField() { Header = "Unpaid", FieldName = "Unpaid", TextAlign = "center",
                //                  IsTotalFooter=true, Colspan = 7, TotalLabel="Total:"
                //},
                //new GridField() { Header = "Payment", FieldName = "Payment", TextAlign = "center" },
                //new GridField() { Header = "Action", FieldName = "InvoiceNo", TextAlign = "Center",
                //    IsModifyField = true, EditFunction ="Edit_Tbl_Row(this);",
                //    DeletedCallBackFunction="LoadCustInvoice(" + ID + ")",
                //    DeleteUseProcedureName="SP_DELETE_CUSTOMER_INVOICE"
                //}
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadPaidInv") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustPaidInvoiceHistory(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            //string SQL = "select ROW_NUMBER() OVER(ORDER BY format(p.PaymentDate, 'yyyy/MM/dd HH:mm:ss') desc) AS[No], ";
            //SQL += "p.Payer, format(p.PaymentDate, 'dd/MM/yyyy HH:mm:ss') PaymentDate, m.PaymentMethod, p.FromBankName, p.FromAccountNumber, p.FromAccountName, ";
            //SQL += "p.FromPhoneNumber, p.ToPhoneNumber, p.ChequeNumber, (ac.AccountTitle + case when ac.AccountNumber <> '' then ' - ' + ";
            //SQL += "ac.AccountNumber else '' end + case when ac.AccountName <> '' then ' - ' + ac.AccountName else '' end) AccountName, ";
            //SQL += "c.Category, p.PaymentDescription, p.ReceivedFromName, p.InvoiceNo, p.PaymentAmount, p.PaidBy ";
            //SQL += "from dbo.TblPayment p ";
            //SQL += "inner join dbo.TblPaymentMethod m on m.PaymentMethodID = p.PaymentMenthod ";
            //SQL += "inner join dbo.TblAccount ac on ac.AccountID = p.ToAccountNumber ";
            //SQL += "inner join dbo.TblPaymentCategory c on c.PaymentCategoryID = p.Category ";
            //SQL += "where p.CustomerID = " + ID + " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID;
            //html.OrderBy = "format(p.PaymentDate, 'yyyy/MM/dd HH:mm:ss') desc";

            string SQL = "select ROW_NUMBER() OVER(ORDER BY format(p.PayDate, 'yyyy/MM/dd HH:mm:ss') desc) AS[No], ('<a href=\"javascript:void(0);\" onclick=\"PrintInvoice('+ convert(varchar, p.CustomerID) +', '''+ p.InvoiceNo +''','+ convert(varchar, i.IsIncludedVAT) +','+ convert(varchar, i.StatusID) +');\">' + p.InvoiceNo + '</a>') InvoiceNo, ";
            SQL += "format(p.PayDate, 'dd/MM/yyyy HH:mm:ss') PayDate, PayBy, PaymentDescription, PayTo, PayAmount, Category, PayDescription, ReceivedFromName ";
            SQL += "from dbo.TblPaymentHistory p ";
            SQL += "inner join dbo.TblCustomerInvoice i on i.InvoiceNo = p.InvoiceNo ";
            SQL += "where p.CustomerID = " + ID + " and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " and i.DomainID like '%" + html.DomainID + "%'";
            SQL += "group by ('<a href=\"javascript:void(0);\" onclick=\"PrintInvoice('+ convert(varchar, p.CustomerID) +', '''+ p.InvoiceNo +''','+ convert(varchar, i.IsIncludedVAT) +','+ convert(varchar, i.StatusID) +');\">' + p.InvoiceNo + '</a>') ";
            SQL += ", format(p.PayDate, 'dd/MM/yyyy HH:mm:ss'), PayBy, PaymentDescription, PayTo, PayAmount, Category, PayDescription, ReceivedFromName ";
            SQL += ", format(p.PayDate, 'yyyy/MM/dd HH:mm:ss')";

            html.OrderBy = "format(p.PayDate, 'yyyy/MM/dd HH:mm:ss') desc";
            html.GH.SQL = SQL;
            //html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustPaidInvoiceHistory";
            //html.JqueryDataTable = true;
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Invoice"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice"))
                html.AllowDelete = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                new GridField() { Header = "InvoiceNo", FieldName = "InvoiceNo", ClassName="text-nowrap" },
                new GridField() { Header = "PayDate", FieldName = "PayDate", ClassName="text-nowrap" },
                new GridField() { Header = "PayBy", FieldName = "PayBy", ClassName="text-nowrap" },
                new GridField() { Header = "Description", FieldName = "PaymentDescription", ClassName="text-nowrap" },
                new GridField() { Header = "PayTo", FieldName = "PayTo", ClassName="text-nowrap" },
                new GridField() { Header = "PayAmount", FieldName = "PayAmount", TextAlign="right", Format="#,#.00" },
                new GridField() { Header = "Category", FieldName = "Category", ClassName="text-nowrap" },
                new GridField() { Header = "PayDescription", FieldName = "PayDescription", ClassName="text-nowrap" },
                new GridField() { Header = "ReceivedFromName", FieldName = "ReceivedFromName", ClassName="text-nowrap" },
            };
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadInvHistory") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustQuotation(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            string SQL = "select ROW_NUMBER() OVER(ORDER BY CustomerBalanceID desc) AS[No], ('<a href=\"javascript:void(0);\" onclick=\"PrintInvoice(" + ID + ", '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +','+ convert(varchar, StatusID) +','''',false,this);\">' + InvoiceNo + '</a>') InvNo, convert(varchar, InvoiceDate, 103) InvoiceDate, ";
            SQL += "DATEDIFF(day, InvoiceDate, GETDATE())[Day], SubTotal, VAT, Total, DiscountP, ";//GETUTCDATE
            SQL += "DiscountUSD, GrandTotal, Paid, Unpaid, BillCycle, IsIncludedVAT, StatusID, InvoiceNo, CustomerID ";
            SQL += "from dbo.TblCustomerBalance where CustomerID = " + ID + " and StatusID = 20 and DomainID like '%" + html.DomainID + "%' order by CustomerBalanceID desc";
            html.GH.SQL = SQL;
            html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.TblID = "tblCustQuotation";
            html.isShowLoadingGrid = false;
            if (html.HasPermission("Edit Quotation"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Quotation"))
                html.AllowDelete = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center" },
                new GridField() { Header = "Quotation No.", FieldName = "InvNo", TextAlign = "center" },
                //new GridField() { Header = "IsVAT", FieldName = "IsIncludedVAT", TextAlign = "center" },
                new GridField() { Header = "Inv.Date", FieldName = "InvoiceDate", TextAlign = "center" },
                new GridField() { Header = "Day", FieldName = "Day", TextAlign = "center" },
                new GridField() { Header = "Dis. %", FieldName = "DiscountP", TextAlign = "center" },
                new GridField() { Header = "Dis. $", FieldName = "DiscountUSD", TextAlign = "center" },
                new GridField() { Header = "GrandTotal", FieldName = "GrandTotal", TextAlign = "center", Format="#,#.00" },
                //new GridField() { Header = "Paid", FieldName = "Paid", TextAlign = "center" },
                //new GridField() { Header = "Unpaid", FieldName = "Unpaid", TextAlign = "center" },
                new GridField() { Header = "Action", FieldName = "InvoiceNo", TextAlign = "Center",
                    IsModifyField = true, EditFunction ="Edit_Tbl_Row(this, 1);",
                    DeleteWithTableName="TblCustomerInvoice, TblCustomerBillInformation, TblCustomerDetail, TblCustomerBalance, TblCustomerBillingInvoice",
                    DeletedCallBackFunction="LoadCustQuotation(" + ID + ")", DeletePrimaryTblName="TblCustomer",
                    DeletePrimaryFiledName="CustomerID", DeletePrimaryValue=ID.ToString(), DeletePrimaryTblNameDependOnTblName="TblCustomerBalance"
                }
            };
            //string data = html.DrawHTMLSearchForm(1, "LoadCustomerQT");
            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadCustomerQT") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveChangeExchageRate(string Value)
        {
            HTML html = new HTML();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("if (not EXISTS(select ExchangeRateID from dbo.TblExchangeRate where convert(varchar, ExchangeRate) <> ''))");
            sb.AppendLine("begin");
            sb.AppendLine("insert into dbo.TblExchangeRate(ExchangeRate) values(" + Value.ToString() + ");");
            sb.AppendLine("end");
            sb.AppendLine("else");
            sb.AppendLine("begin");
            sb.AppendLine("update dbo.TblExchangeRate set ExchangeRate = " + Value.ToString() + "");
            sb.AppendLine("end");
            ;
            return Json(new { data = html.SaveChangeData(sb.ToString(), false, false) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustomerServices(int CustomerID)
        {
            HTML html = new HTML();
            //			string SQL = @"select distinct isnull(ac.[SID], 'Pending') [SID], tb.OpenTicketID, tb.TicketNumber, 
            //isnull(ac.RouterID, 0) RouterID, s.ServiceName + case when isnull(ac.ActivateDate,'')<>'' then '; <span class=""text-primary"">Start Service: ' + CONVERT(VARCHAR, format(ac.ActivateDate, 'dd-MMM-yy hh:mm:ss tt')) + '; By: ' + ac.ActivateBy +'</span>' else '' end ServiceName, 
            //p.PackageName, pd.Bandwidth, tb.UM, tb.StatusID, isnull(tb.IsChangePlan, 0) IsChangePlan, isnull(tb.IsApplyChangePlan, 0) IsApplyChangePlan, 
            //tb.ConnectionID, tb.StartDate, tb.Duration, case when DATEDIFF(M, convert(varchar, tb.StartDate, 111),
            //convert(varchar, tb.EndDate, 111)) >= tb.Duration then
            //tb.FreeDuration else 0 end FreeDuration, isnull(tb.ConnectivityStatus, 43) ConnectivityStatus, ss.[Status], isnull(ac.ConnectionType, '') ConnectionType, isnull(ac.IPAddress, 'Manual') IPAddress, isnull(ac.VLanName, 'Manual') VLanName,
            //tb.NextInvoiceDate, isnull(tb.IsRenew, 0) IsRenew,
            //case when DATEDIFF(day, tb.StartDate, GETDATE()) > 0 then DATEDIFF(day, tb.StartDate, GETDATE()) else 0 end [Day], tb.EndDate, tb.ServiceID, p.PackageTypeID, tb.PackageDetailID, tb.BillingInvoiceID
            //, tb.InvoiceNo, (case when DATEDIFF(d, getdate(), tb.EndDate) >=0 then case when DATEDIFF(d, getdate(), tb.StartDate) <= 0 then convert(varchar, DATEDIFF(d, getdate(), tb.EndDate)) + ' days'
            //when DATEDIFF(d, getdate(), tb.StartDate) > 0 then convert(varchar, DATEDIFF(d, tb.StartDate, tb.EndDate)) + ' days'
            //end else 'Expired' end) RemainDay, isnull(ac.VLanID, '') VLanID
            //, tb.PaymentDueDate, isnull(ac.PPPoEUserName, '') PPPoEUserName, isnull(ac.ActivateCustomerID, 0) ActivateCustomerID,
            //isnull((select top 1 case when bi.IsApplyChangePlan =1 then case when isnull(bi.invTmp, 1) = 1 then bi.PackageName else bi.ServiceName end end
            //from dbo.TblCustomerBillingInvoice bi where bi.InvoiceNo = tb.OldInvoiceNo),'') OldPlanName, tb.[Type], tb.InvoiceDate, isnull(ci.ApproveRequestToDMCTAXStatus, '') ApproveRequestToDMCTAXStatus, tb.CustomerID, isnull(ac.AutoLock, 1) AutoLock, isnull(ac.ConnectionOfConnectionID, '') ConnectionOfConnectionID
            //from dbo.TblCustomerBillingInvoice tb
            //left join dbo.TblActivateCustomer ac on ac.ConnectionID = tb.ConnectionID and ac.ActivateCustomerID = tb.ActivateCustomerID
            //inner join dbo.TblServices s on s.ServiceID = tb.ServiceID
            //inner join dbo.TblPackageDetail pd on pd.PackageDetailID = tb.PackageDetailID
            //inner join dbo.TblPackages p on p.PackageID = tb.PackageID
            //inner join dbo.TblStatus ss on ss.StatusID = tb.ConnectivityStatus
            //inner join dbo.TblCustomerInvoice ci on ci.CustomerID = " + CustomerID + @" and ci.InvoiceNo = tb.InvoiceNo and isnull(ci.ItemID,0) = 0
            //inner join (select cbi.CustomerID, count(*) as [Count] from TblCustomerBillingInvoice cbi where cbi.CustomerID = " + CustomerID + @" group by cbi.CustomerID) gc on gc.CustomerID = tb.CustomerID
            //where ci.CustomerCode is not null and isnull(tb.IsApplyChangePlan, 0) = 0 and tb.DomainID like '%" + html.DomainID + "%' and tb.CustomerID = " + CustomerID + @" and (tb.StatusID not in (12,13, 14, 20))
            //and format(GETDATE(), 'yyyy-MM') <= case when gc.[Count] > 1 then format(tb.NextInvoiceDate, 'yyyy-MM') else format(GETDATE(), 'yyyy-MM') end
            //and 1 = case when gc.[Count] = 1 and tb.[Type] in ('Sale Order Invoice', 'Official Invoice', 'ProformaInvoice') then 1 when gc.[Count] > 1 and (tb.[Type] in ('Official Invoice', 'ProformaInvoice')) then 1 else 0 end-- or ci.InvoiceStatusID<>90
            //and (case when DATEDIFF(d, getdate(), tb.EndDate) >=0 then case when DATEDIFF(d, getdate(), tb.StartDate) <= 0 then convert(varchar, DATEDIFF(d, getdate(), tb.EndDate)) + ' days'
            //when DATEDIFF(d, getdate(), tb.StartDate) > 0 then convert(varchar, DATEDIFF(d, tb.StartDate, tb.EndDate)) + ' days' end else 'Expired' end) <> 'Expired'
            //order by tb.ConnectionID, tb.InvoiceDate desc, tb.BillingInvoiceID";
            string SQL = @"SELECT tb.EndDate,
    ISNULL(tb.[SID], 'Pending') AS [SID],
    ISNULL(a.OpenTicketID, 0) OpenTicketID,
    ISNULL(a.TicketNumber, '') TicketNumber,
    ISNULL(a.RouterID, 0) AS RouterID,
	case when isnull(ac.ConnectionOfConnectionID,'') <> '' then sr.ServiceName else sr.ServiceName + case when isnull(ac.ActivateDate,'')<>'' then '; <span class=""""text-primary"""">Start Service: ' + CONVERT(VARCHAR, format(ac.ActivateDate, 'dd-MMM-yy hh:mm:ss tt')) + '; By: ' + ac.ActivateBy +'</span>' else '' end end ServiceName,
    ISNULL(a.PackageName, '') AS PackageName,
    ISNULL(ac.Bandwidth, 0) AS Bandwidth,
    ISNULL(a.UM, '') AS UM,
    ISNULL(a.StatusID, 0) AS StatusID,
    ISNULL(a.IsChangePlan, 0) AS IsChangePlan,
    ISNULL(a.IsApplyChangePlan, 0) AS IsApplyChangePlan,
    ISNULL(a.ConnectionID, 0) AS ConnectionID,
    ISNULL(a.StartDate, '') AS StartDate,
    ISNULL(a.Duration, 0) AS Duration,
    ISNULL(a.FreeDuration, 0) AS FreeDuration,
    ISNULL(a.ConnectivityStatus, 0) AS ConnectivityStatus,
    ISNULL(s.[Status], '') AS [Status],
    ISNULL(ac.ConnectionType, '') AS ConnectionType,
	ISNULL(ac.IPAddress, '') AS IPAddress,
	ISNULL(ac.VLanName, '') AS VLanName,
	a.NextInvoiceDate,
	a.IsRenew,
	a.[Day],
	a.EndDate,
	a.ServiceID,
	a.PackageTypeID,
	a.PackageDetailID,
	a.BillingInvoiceID,
	a.InvoiceNo,
	a.RemainDay,
	ac.VLanID,
	a.PaymentDueDate,
	ac.PPPoEUserName,
	isnull(ac.ActivateCustomerID, 0) ActivateCustomerID,
	a.OldPlanName,
	a.[Type],
	a.CustomerID,
	isnull(ac.AutoLock, 0) AutoLock,
	ac.ConnectionOfConnectionID
FROM TblCustomerBillingInvoice tb
OUTER APPLY (
    SELECT TOP 1
		ci.OpenTicketID,
		ci.TicketNumber,
		ci.ActivateCustomerID,
		ci.RouterID,
        pd.PackageName,
        ci.UM,
        ci.StatusID,
        ci.IsChangePlan,
        ci.IsApplyChangePlan,
        ci.ConnectionID,
        ci.StartDate,
        ci.Duration,
        ci.FreeDuration,
        ci.ConnectivityStatus,
		ci.NextInvoiceDate,
		ci.IsRenew,		
		case when DATEDIFF(day, ci.StartDate, GETDATE()) > 0 then DATEDIFF(day, ci.StartDate, GETDATE()) else 0 end [Day],
		ci.EndDate,
		ci.ServiceID,
		p.PackageTypeID,
		ci.PackageDetailID,
		ci.BillingInvoiceID,
		ci.InvoiceNo,
		(case when DATEDIFF(d, getdate(), ci.EndDate) >=0 then case when DATEDIFF(d, getdate(), ci.StartDate) <= 0 then convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + ' days'
		when DATEDIFF(d, getdate(), ci.StartDate) > 0 then convert(varchar, DATEDIFF(d, ci.StartDate, ci.EndDate)) + ' days'
		end else 'Expired' end) RemainDay,
		ci.PaymentDueDate,
		isnull((select top 1 case when bi.IsApplyChangePlan =1 then case when isnull(bi.invTmp, 1) = 1 then bi.PackageName else bi.ServiceName end end
		from dbo.TblCustomerBillingInvoice bi where bi.InvoiceNo = ci.OldInvoiceNo),'') OldPlanName,
		ci.[Type],
		ci.CustomerID
    FROM TblCustomerBillingInvoice ci
    INNER JOIN dbo.TblPackageDetail pd ON pd.PackageDetailID = ci.PackageDetailID
	inner join dbo.TblPackages p on p.PackageID = ci.PackageID	
    WHERE ci.StatusID NOT IN (12, 13, 14, 20) 
        AND ci.CustomerID = tb.CustomerID 
        AND isnull(ci.[SID], 0) = isnull(tb.[SID], 0)
		AND ci.EndDate = tb.EndDate
    ORDER BY ci.StartDate desc, ci.BillingInvoiceID DESC
) AS a
inner JOIN dbo.TblStatus s ON s.StatusID = a.ConnectivityStatus
inner join dbo.TblServices sr on sr.ServiceID = tb.ServiceID
left JOIN dbo.TblActivateCustomer ac ON ac.ActivateCustomerID = a.ActivateCustomerID
WHERE tb.[Type] = 'Official Invoice' and tb.StatusID NOT IN (12, 13, 14, 20) AND tb.CustomerID = " + CustomerID + " and tb.DomainID like '%" + html.DomainID + @"%'
and DATEDIFF(d, getdate(), tb.EndDate) >= 0
GROUP BY tb.EndDate, tb.[SID], tb.CustomerID, a.OpenTicketID, a.TicketNumber, a.RouterID, sr.ServiceName, a.PackageName,
ac.Bandwidth, a.UM, a.StatusID, a.IsChangePlan, a.IsApplyChangePlan, a.ConnectionID,
a.StartDate, a.Duration, a.FreeDuration, a.ConnectivityStatus, s.[Status], ac.ConnectionType, ac.IPAddress, ac.VLanName,
a.NextInvoiceDate, a.IsRenew, a.[Day], a.EndDate, a.ServiceID, a.PackageTypeID, a.PackageDetailID, a.BillingInvoiceID,
a.InvoiceNo, a.RemainDay, ac.VLanID, a.PaymentDueDate, ac.PPPoEUserName, ac.ActivateCustomerID, a.OldPlanName,
a.[Type], a.CustomerID, ac.AutoLock, ac.ConnectionOfConnectionID, ac.ActivateDate, ac.ActivateBy
order by a.StartDate desc";
            string data = html.JsonData(SQL);
            if (data == "[]")
            {
                SQL = @"select bi.[SID],
(select top 1 OpenTicketID from dbo.TblCustomerBillingInvoice where [SID] = bi.[SID] and isnull(OpenTicketID, 0) <> 0 and CustomerID = bi.CustomerID order by BillingInvoiceID desc) OpenTicketID,
(select top 1 TicketNumber from dbo.TblCustomerBillingInvoice where [SID] = bi.[SID] and isnull(OpenTicketID, 0) <> 0 and CustomerID = bi.CustomerID order by BillingInvoiceID desc) TicketNumber,
(select top 1 RouterID from dbo.TblCustomerBillingInvoice where [SID] = bi.[SID] and isnull(OpenTicketID, 0) <> 0 and CustomerID = bi.CustomerID order by BillingInvoiceID desc) RouterID,
s.ServiceName, pd.PackageName, 
isnull((select top 1 Bandwidth from dbo.TblActivateCustomer where [SID] = bi.[SID] and CustomerID = bi.CustomerID), 0) Bandwidth,
(select top 1 UM from dbo.TblCustomerBillingInvoice where [SID] = bi.[SID] and CustomerID = bi.CustomerID order by BillingInvoiceID desc) UM,
(select top 1 StatusID from dbo.TblCustomerBillingInvoice where [SID] = bi.[SID] and isnull(OpenTicketID, 0) <> 0 and CustomerID = bi.CustomerID order by BillingInvoiceID desc) StatusID,
isnull(bi.IsApplyChangePlan, 0) IsApplyChangePlan, bi.ConnectionID,
(select top 1 StartDate from dbo.TblCustomerBillingInvoice where [SID] = bi.[SID] and isnull(OpenTicketID, 0) <> 0 and CustomerID = bi.CustomerID order by BillingInvoiceID desc) StartDate,
(select top 1 Duration from dbo.TblCustomerBillingInvoice where [SID] = bi.[SID] and isnull(OpenTicketID, 0) <> 0 and CustomerID = bi.CustomerID order by BillingInvoiceID desc) Duration,
(select top 1 FreeDuration from dbo.TblCustomerBillingInvoice where [SID] = bi.[SID] and isnull(OpenTicketID, 0) <> 0 and CustomerID = bi.CustomerID order by BillingInvoiceID desc) FreeDuration,
(select top 1 bii.ConnectivityStatus from dbo.TblCustomerBillingInvoice bii where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) ConnectivityStatus,
(select top 1 s.[Status] from dbo.TblCustomerBillingInvoice bii inner join dbo.TblStatus s on s.StatusID = bii.ConnectivityStatus
where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) [Status],
isnull(ac.ConnectionType, '') ConnectionType, isnull(ac.IPAddress, '') IPAddress, isnull(ac.VLanName, '') VLanName,
(select top 1 bii.NextInvoiceDate from dbo.TblCustomerBillingInvoice bii where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) NextInvoiceDate,
(select top 1 bii.IsRenew from dbo.TblCustomerBillingInvoice bii where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) IsRenew,
(select top 1 case when DATEDIFF(day, bii.StartDate, GETDATE()) > 0 then DATEDIFF(day, bii.StartDate, GETDATE()) else 0 end from dbo.TblCustomerBillingInvoice bii where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) [Day],
(select top 1 bii.EndDate from dbo.TblCustomerBillingInvoice bii where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) EndDate,
(select top 1 bii.ServiceID from dbo.TblCustomerBillingInvoice bii where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) ServiceID,
(select top 1 p.PackageTypeID from dbo.TblCustomerBillingInvoice bii inner join dbo.TblPackages p on p.PackageID = bii.PackageID where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) PackageTypeID,
(select top 1 bii.PackageDetailID from dbo.TblCustomerBillingInvoice bii where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) PackageDetailID,
(select top 1 bii.BillingInvoiceID from dbo.TblCustomerBillingInvoice bii where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) BillingInvoiceID,
(select top 1 bii.InvoiceNo from dbo.TblCustomerBillingInvoice bii where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) InvoiceNo,
(select top 1 (case when DATEDIFF(d, getdate(), bii.EndDate) >=0 then case when DATEDIFF(d, getdate(), bii.StartDate) <= 0 then convert(varchar, DATEDIFF(d, getdate(), bii.EndDate)) + ' days' when DATEDIFF(d, getdate(), bii.StartDate) > 0 then convert(varchar, DATEDIFF(d, bii.StartDate, bii.EndDate)) + ' days' end else 'Expired' end) from dbo.TblCustomerBillingInvoice bii where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) RemainDay,
isnull(ac.VLanID, '') VLanID, (select top 1 bii.PaymentDueDate from dbo.TblCustomerBillingInvoice bii where bii.[SID] = bi.[SID] and isnull(bii.OpenTicketID, 0) <> 0 and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc) PaymentDueDate,
isnull(ac.PPPoEUserName, '') PPPoEUserName, isnull(bi.ActivateCustomerID, 0) ActivateCustomerID,
isnull((select top 1 case when bii.IsApplyChangePlan =1 then case when isnull(bii.invTmp, 1) = 1 then bii.PackageName else bii.ServiceName end end from dbo.TblCustomerBillingInvoice bii where bii.SID = bi.SID and bii.CustomerID = bi.CustomerID order by bii.BillingInvoiceID desc),'') OldPlanName,
bi.[Type], bi.CustomerID, isnull(ac.AutoLock, 0) AutoLock, isnull(ac.ConnectionOfConnectionID, '') ConectionOfConnectionID
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblServices s on s.ServiceID = bi.ServiceID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
left join dbo.TblActivateCustomer ac on ac.CustomerID = bi.CustomerID and ac.[SID] = bi.[SID]
where bi.[Type] = 'Official Invoice' and bi.StatusID NOT IN (12, 13, 14, 20)
and bi.CustomerID = " + CustomerID + " and bi.DomainID like '%" + html.DomainID + @"%'
group by bi.[SID], bi.CustomerID, s.ServiceName, pd.PackageName, bi.IsApplyChangePlan, bi.ConnectionID, ac.ConnectionType, ac.IPAddress,
ac.VLanName, ac.VLanID, ac.PPPoEUserName, bi.ActivateCustomerID, bi.[Type], ac.AutoLock, ac.ConnectionOfConnectionID
order by bi.[SID]";
                data = html.JsonData(SQL);
            }
            //SQL = @" select PPPoEUserName, IPAddress, s.[Status], ac.ConnectionType, ac.VLanName 
            //, ac.ConnectivityStatus
            //from dbo.TblActivateCustomer ac
            //inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
            //where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID
            //+ " and ac.CustomerID = " + CustomerID;
            //string ConnectivityData = html.JsonData(SQL);
            SQL = @"select bi.BillingInvoiceID, case when bi.invTmp = 1 then '<div class=""row""><div class=""col text-danger"">This package: ' + bi.PackageName + ' not yet upgrade due to not yet payment. </div></div>' else '<div class=""row""><div class=""col text-danger"">This package: ' + bi.ServiceName + ' not yet upgrade due to not yet payment. </div></div>' end NewPackageName
            from dbo.TblCustomerBillingInvoice bi where bi.DomainID like '%" + html.DomainID + "%' and CustomerID = " + CustomerID + " and StatusID = 11 and isnull(bi.IsChangePlan,0) = 1 and isnull(bi.IsAlreadyApplyProfile,0) = 0";
            string UpgradeP = html.JsonData(SQL);

            SQL = @"Select REPLACE(ci.ItemDescription, s.ServiceName+ ' - ', '') ItemDescription, ci.ConnectionID, s.ServiceName from TblCustomerInvoice ci
inner join dbo.TblServices s on s.ServiceID = ci.ServiceID
where ci.DomainID like '%" + html.DomainID + @"%' and ci.CustomerID = @CustomerID 
and (CHARINDEX('CON:',ItemDescription) <> 0 or CHARINDEX('CON: ',ItemDescription) <> 0 or CHARINDEX(' CON: ',ItemDescription) <> 0)";
            Dictionary<string, string> Para = new Dictionary<string, string>
            {
                {"CustomerID", CustomerID.ToString() }
            };
            string[] delimiterChars = { "CON:" };
            DataSet ds = html.LoadData(SQL, false, Para);
            List<object> list = new List<object>();
            string listConnectionName = "";
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        string ServiceName = row["ItemDescription"].ToString().Replace(";", string.Empty).Trim();
                        string[] Connection = ServiceName.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                        list.Add(new { Connection = row["ConnectionID"].ToString(), Data = Connection.ToList().Skip(1).ToList() });
                    }
                    SQL = "select ConnectionName, ConnectivityStatus,ConnectionOfConnectionID, ActivateCustomerID, VLanID, ActivateDate, BillingInvoiceID,PPPoEUserName,PackageDetailID,ServiceID,RouterID from dbo.TblActivateCustomer where CustomerID = " + CustomerID;
                    listConnectionName = JsonConvert.SerializeObject(html.LoadData(SQL).Tables[0]);
                }
            }

            SQL = "select VLanID, EndDate, RouterID,ConnectionType from dbo.TblActivateCustomer where CustomerID = " + CustomerID;
            ds = html.LoadData(SQL);
            SqlResult sqlResult = new SqlResult();
            object[] objs = new object[] { };
            var objList = objs.ToList();
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    Mikrotik mk = new Mikrotik();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        int RouterID = int.Parse(row["RouterID"].ToString());
                        string VLanID = row["VLanID"].ToString();
                        if (VLanID != "")
                        {
                            string ConnectionType = row["ConnectionType"].ToString();
                            mk.SetConnection(RouterID);
                            if (ConnectionType == "Static")
                            {
                                sqlResult = mk.GetVlanIDStatus(VLanID);
                                if (sqlResult.Result == ResultType.Successful)
                                {
                                    if (sqlResult.MikrotikExecuteList.Count() > 0)
                                    {
                                        objList.Add(new
                                        {
                                            VLanID = VLanID.ToString(),
                                            Status = sqlResult.Value.ToString(),
                                            VLanName = sqlResult.MikrotikExecuteList.ToList()[0].Words["name"].ToString(),
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Json(new { data = data, UpgradeP = UpgradeP, Connections = list.ToList(), ConnectionName = listConnectionName, RealConnectionStatus = objList }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustomerBalance(int ID)
        {
            HTML html = new HTML();
            string SQL = "select sum(DepositFee) DepositFee, sum(GrandTotal) Total, sum(Paid) Paid, sum(Unpaid) Unpaid ";
            SQL += "from dbo.TblCustomerBalance where CustomerID = @CustomerID and DomainID like '%" + html.DomainID + "%' and (StatusID not in (12,13,20)) ";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustServices(int ID)
        {
            HTML html = new HTML();
            bool roleEditCustomer = HttpContext.GetOwinContext().Request.User.IsInRole("Edit Customer");
            ViewBag.roleEditCustomer = roleEditCustomer;
            //			string SQL = @"select c.CustomerName, bf.CustomerID, bf.InvoiceNo, (case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end) ServiceName,
            //bf.CompanyName,bf.CompanyNameKH, bf.CompanyDeskPhone, bf.CompanyMobile, bf.VATNumber, bf.IndustryTypeID, it.IndustryType,
            //bi.ConnectionID, '<span class=""text-red"">' + isnull(bi.[SID], 'Pending') + '</span>' [SID], s.[Status], bf.CompanyEmail, bf.CompanyContactMobileNumber, bf.BillToAddress, bf.IndustryTypeID,
            //bf.BillToCommune, bf.BillToDistrict, bf.BillToProvinceCity, bf.InstallAddress, bf.InstallCommune, bf.CompanyAddress, bf.CompanyCommune, bf.CompanyDistrict, bf.ATTName, 
            //bf.InstallDistrict, bf.InstallProvinceCity, bf.GoogleMap_FullAddress, bf.GoogleMap_Latitude, bf.GoogleMap_Longitude, bf.CompanyProvinceCity, bf.CompanyContactName, bf.CompanyContactPosition, bf.InstallDistrict, bf.InstallProvinceCity, bf.CustomerBillInformationID,
            //case when sum(DATEDIFF(D, getdate(), bi.EndDate)) >=0 then convert(varchar, sum(DATEDIFF(D, getdate(), bi.EndDate))) + ' days' else 'Expired' end RemainDay
            //from dbo.TblCustomerBillInformation bf
            //left join dbo.TblCustomerBillingInvoice bi on bi.CustomerBillInformationID = bf.CustomerBillInformationID
            //left join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
            //inner join dbo.TblIndustryType it on it.IndustryTypeID = bf.IndustryTypeID
            //inner join dbo.TblCustomer c on c.CustomerID = bf.CustomerID
            //where bi.DomainID like '%"+html.DomainID+@"%' and bi.[Type] = 'Official Invoice' and bf.CustomerID = @CustomerID and isnull(bi.StatusID, 0) not in (12,13,14,20)
            //and bi.ServiceName is not null
            //--isnull(bi.StatusID,0) <> 20 and isnull(bf.StatusID,0) <> 12 
            //and bf.SystemID = @SystemID and bf.WarehouseID = @WarehouseID
            //group by c.CustomerName, bf.CustomerID, bf.InvoiceNo,
            //(case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end),
            //bf.CompanyName,bf.CompanyNameKH, bf.CompanyDeskPhone, bf.CompanyMobile, bf.VATNumber, bf.IndustryTypeID, it.IndustryType,
            //bi.ConnectionID, '<span class=""text-red"">' + isnull(bi.[SID], 'Pending') + '</span>', s.[Status], bf.CompanyEmail, bf.CompanyContactMobileNumber, bf.BillToAddress, bf.IndustryTypeID,
            //bf.BillToCommune, bf.BillToDistrict, bf.BillToProvinceCity, bf.InstallAddress, bf.InstallCommune, bf.CompanyAddress, bf.CompanyCommune, bf.CompanyDistrict, bf.ATTName, 
            //bf.InstallDistrict, bf.InstallProvinceCity, bf.GoogleMap_FullAddress, bf.GoogleMap_Latitude, bf.GoogleMap_Longitude, bf.CompanyProvinceCity, bf.CompanyContactName, bf.CompanyContactPosition,
            //bf.InstallDistrict, bf.InstallProvinceCity, bf.CustomerBillInformationID
            //order by bi.ConnectionID desc";
            string SQL = @"select distinct ci.InvoiceDate, ci.CustomerID, ci.CustomerBillInformationID, bi.IndustryTypeID, ci.InvoiceNo, ci.ConnectionID, ci.[SID], s.[Status],
ci.CustomerName, bi.CompanyName, bi.CompanyNameKH, bi.CompanyDeskPhone, bi.CompanyMobile, bi.CompanyEmail,
bi.VATNumber, it.IndustryType, bi.ATTName, bi.CompanyAddress, bi.CompanyCommune,
bi.CompanyDistrict, bi.CompanyProvinceCity, bi.CompanyContactName, bi.CompanyContactPosition,
bi.CompanyContactMobileNumber, bi.BillToAddress, bi.BillToCommune, bi.BillToDistrict, bi.BillToProvinceCity,
bi.InstallAddress, bi.InstallCommune, bi.InstallDistrict, bi.InstallProvinceCity,
bi.GoogleMap_FullAddress, bi.GoogleMap_Latitude, bi.GoogleMap_Longitude,
case when (DATEDIFF(D, getdate(), ci.EndDate)) >=0 then convert(varchar, (DATEDIFF(D, getdate(), ci.EndDate))) + ' days' else 'Expired' end RemainDay
from dbo.TblCustomerInvoice ci
inner join dbo.TblCustomerBillInformation bi on bi.CustomerBillInformationID = ci.CustomerBillInformationID
inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
inner join dbo.TblIndustryType it on it.IndustryTypeID = bi.IndustryTypeID
where ci.[Type] = 'Official Invoice' and ci.CustomerID = @CustomerID
order by ci.InvoiceDate desc";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() },
				//{ "SystemID", html.SystemID },
				//{ "WarehouseID", html.WarehouseID }
			};
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadAttachmentFile(int ID)
        {
            HTML html = new HTML();
            string sql = "select PaymentID, AttachmentFile, (PaymentDescription + '<br> Pay Date: ' + format(PaymentDate, 'dd/MM/yyyy HH:mm:ss')) Title, DestinationFolder from dbo.TblPayment ";
            sql += "where AttachmentFile <> '' and CustomerID = " + ID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            sql += "order by format(PaymentDate, 'yyyy/MM/dd HH:mm:ss') desc";
            DataSet ds = html.LoadData(sql);
            string data = "";
            dynamic FileName = new List<dynamic>();
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string DestinationFolder = ds.Tables[0].Rows[0]["DestinationFolder"].ToString();
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        dynamic row = new ExpandoObject();
                        row.FileName = dr["AttachmentFile"].ToString();
                        row.Title = dr["Title"].ToString();
                        row.ID = dr["PaymentID"].ToString();
                        FileName.Add(row);
                        //if (FileName == "")
                        //{
                        //    FileName = dr["AttachmentFile"].ToString();
                        //    Title = dr["Title"].ToString();
                        //}
                        //else
                        //{
                        //    FileName += "~" + dr["AttachmentFile"].ToString();
                        //    Title += "~" + dr["Title"].ToString();
                        //}
                    }
                    //object FName = Json(new
                    //{
                    //    FileName = fileName,
                    //    Title = Title
                    //}, JsonRequestBehavior.AllowGet);

                    //var obj = new { FileName = FileName, Title = Title };
                    data = html.ShowAttachmentFile(DestinationFolder, FileName);
                }
            }
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Attachment File, Show Attachment File, Delete Attachment File, Upload Service Form")]
        public JsonResult LoadCustAttachmentFile(int ID, string TypeFor = "AttachmentFile", string InvoiceNo = "")
        {
            HTML html = new HTML();
            string sql = @"select AttachmentFileID, ('InvoiceNo.: ' + InvoiceNo) Title, DestinationFolder, 
AttachmentFileName AttachmentFile, format(AddDate, 'dd/MM/yyyy') AddDate
from TblCustomerAttachmentFile where CustomerID = " + ID + " and isnull(InvoiceNo,'')=case when '" + InvoiceNo + "' <> '' then '" + InvoiceNo + "' else isnull(InvoiceNo,'') end and isnull(TypeFor, '')";
            if (TypeFor == "AttachmentFile")
                sql += @" in ('AttachmentFile', 'Attachment File', 'Customer Bank Slip')";
            else
                sql += @"= '" + TypeFor + "' ";
            sql += @" and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by InvoiceNo, format(AddDate, 'dd/MM/yyyy') desc";
            DataSet ds = html.LoadData(sql);
            string data = "";
            dynamic FileName = new List<dynamic>();
            string desc = "";
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //string localPath = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);					
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if (desc == "")
                            desc = "/" + dr["DestinationFolder"].ToString() + "/";
                        else
                            desc += "~" + "/" + dr["DestinationFolder"].ToString() + "/";
                        dynamic row = new ExpandoObject();
                        if (dr["AttachmentFile"].ToString() != "")
                        {
                            row.FileName = dr["AttachmentFile"].ToString();
                            row.Title = dr["Title"].ToString() + "<br />" + dr["AddDate"].ToString();
                            row.ID = dr["AttachmentFileID"].ToString();
                            FileName.Add(row);
                        }
                    }
                    bool roleManager = HttpContext.GetOwinContext().Request.User.IsInRole("Delete Attachment File");
                    data = html.ShowAttachmentFile(desc, FileName, true, roleManager, "CustomerProfile/DeletCustomerAttFile");
                }
            }
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeletCustomerAttFile(string DestinationFile, string DeleteFileName, dynamic KeepFileName, int ID)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string SQL = "update dbo.TblCustomerAttachmentFile set AttachmentFileName = @AttachmentFileName where AttachmentFileID = @AttachmentFileID";
            var FileName = "";
            foreach (string fname in KeepFileName)
            {
                if (FileName == "")
                    FileName = fname;
                else
                    FileName += "~" + fname;
            }
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "AttachmentFileName", FileName },
                { "AttachmentFileID", ID.ToString() }
            };
            sqlResult = html.DeleteAttachmentFile(DestinationFile, DeleteFileName, KeepFileName, SQL, para);

            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public ActionResult SaveChangeCustomerUploadFile(object[] Data)
        {
            SqlResult sqlresult = new SqlResult();
            dynamic CustomerDataInfo = new JavaScriptSerializer().DeserializeObject(Data[0].ToString());
            HTML html = new HTML();
            string TypeFor = "AttachmentFile";
            if (CustomerDataInfo.ContainsKey("TypeFor"))
                TypeFor = HttpUtility.UrlDecode(CustomerDataInfo["TypeFor"]);
            string CustomerName = HttpUtility.UrlDecode(CustomerDataInfo["CustomerName"]);
            string CID = HttpUtility.UrlDecode(CustomerDataInfo["CustomerCode"]);
            CustomerName = $"{html.ToUrlSlug($"{CustomerName + " [" + CID + "]"}")}";
            string DestinationFolder = "/Uploads/CustomerAttFiles/" + CustomerName;
            sqlresult = html.UploadFile(DestinationFolder, "", false, true);
            string AttachmentFile;
            if (sqlresult.Result == ResultType.Successful)
                AttachmentFile = sqlresult.Value;
            else
            {
                return Json(new
                {
                    Message = sqlresult.Message,
                    Result = sqlresult.Result,
                    Data = sqlresult.Value
                });
            }
            string SystemID = html.SystemID;
            string WarehouseID = html.WarehouseID;
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);

            string sql = "insert into TblCustomerAttachmentFile (CustomerID, AttachmentFileName, DestinationFolder, SystemID, WarehouseID, AddDate, UploadBy, TypeFor, DomainID)";
            sql += " values (@CustomerID, @AttachmentFileName, @DestinationFolder, @SystemID, @WarehouseID, @AddDate, @UploadBy,@TypeFor, @DomainID);";
            Dictionary<string, string> para = new Dictionary<string, string>{
                { "CustomerID", CustomerDataInfo["CustomerID"].ToString()},
                { "AttachmentFileName", AttachmentFile },
                { "DestinationFolder", DestinationFolder },
                { "SystemID", SystemID.ToString() },
                {"WarehouseID", WarehouseID.ToString() },
                {"AddDate", easternTimeNow.ToString("yyyy/MM/dd HH:mm:ss tt") },
                {"UploadBy", html.UserFullName },
                {"TypeFor", TypeFor },
                {"DomainID", html.DomainID }
            };

            sqlresult = html.SaveChangeData(sql, false, para);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value
            });
        }
        public ActionResult SaveChangeCustomerProfilePicture(object[] Data)
        {
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(Data[0].ToString());
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            string CustomerName = HttpUtility.UrlDecode(DataInfo["CustomerName"]);
            string CID = HttpUtility.UrlDecode(DataInfo["CustomerCode"]);
            CustomerName = $"{html.ToUrlSlug($"{CustomerName + " [" + CID + "]"}")}";
            string DestinationFolder = "/Uploads/CustomerAttFiles/" + CustomerName + "/Profile Picture/";
            sqlresult = html.UploadFile(DestinationFolder);
            string AttachmentFile;
            if (sqlresult.Result == ResultType.Successful)
                AttachmentFile = sqlresult.Value;
            else
            {
                return Json(new
                {
                    Message = sqlresult.Message,
                    Result = sqlresult.Result,
                    Data = sqlresult.Value
                }, JsonRequestBehavior.AllowGet);
            }
            string SQL = "update dbo.TblCustomer set DestinationFile = @DestinationFile, CustomerProfilePicture= @CustomerProfilePicture where CustomerID = @CustomerID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "CustomerID", DataInfo["CustomerID"].ToString() },
                { "DestinationFile", DestinationFolder },
                { "CustomerProfilePicture", AttachmentFile }
            };
            sqlresult = html.SaveChangeData(SQL, false, para);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustContractFile(int ID)
        {
            HTML html = new HTML();
            string sql = "select c.CustomerBillInformationID, c.DestinationFolder, c.imgFileName, pdfFileName, format(c.InvoiceDate, 'dd/MM/yyyy') InvoiceDate ";
            sql += "from dbo.TblCustomerBillInformation c where c.CustomerID = " + ID;
            sql += " and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'";
            sql += " order by format(c.InvoiceDate, 'dd/MM/yyyy')";
            DataSet ds = html.LoadData(sql);
            string data = "";
            dynamic FileName = new List<dynamic>();
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //string localPath = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
                    string desc = "/" + ds.Tables[0].Rows[0]["DestinationFolder"].ToString() + "/";
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        dynamic row = new ExpandoObject();
                        if (dr["imgFileName"].ToString() != "")
                        {
                            row.FileName = dr["imgFileName"].ToString();
                            row.Title = "Date: " + dr["InvoiceDate"].ToString();
                            row.ID = dr["CustomerBillInformationID"].ToString();
                            row.PDFFileName = dr["pdfFileName"].ToString();
                            FileName.Add(row);
                        }
                    }
                    data = html.ShowAttachmentFile(desc, FileName, true, false, "", true, false, true);
                }
            }
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustMap(int ID)
        {
            HTML html = new HTML();
            string SQL = "select distinct cb.GoogleMap_FullAddress, cb.GoogleMap_Latitude, cb.GoogleMap_Longitude ";
            SQL += "from dbo.TblCustomerBillInformation cb ";
            //SQL += "inner join dbo.TblCustomerInvoice c on c.InvoiceNo = cb.InvoiceNo ";
            SQL += "where cb.CustomerID=@CustomerID ";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadCustProfilePicture(int ID)
        {
            HTML html = new HTML();
            string SQL = "select DestinationFile, CustomerProfilePicture from dbo.TblCustomer ";
            SQL += "where CustomerID=@CustomerID ";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "CustomerID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckConnectivityStatus(int CustomerID)
        {
            HTML html = new HTML();
            StringBuilder sb = new StringBuilder();
            string sql = @" select PPPoEUserName, IPAddress, s.[Status], ac.ConnectionType, ac.VLanName 
                            , ac.ConnectivityStatus
                            from dbo.TblActivateCustomer ac
                            inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
                            where ac.DomainID like '%" + html.DomainID + "%' and ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID
                            + " and ac.CustomerID = " + CustomerID;
            string Data = html.JsonData(sql);
            sql = @"select case when bi.invTmp = 1 then '<div class=""row""><div class=""col text-danger"">This package not yet upgrade due to not yet payment. </div></div><div class=""row""><div class=""col text-danger"">' + bi.PackageName + '</div></div>' else '<div class=""row""><div class=""col text-danger"">This package not yet upgrade due to not yet payment. </div></div><div class=""row""><div class=""col text-danger"">' + bi.ServiceName + '</div></div>' end NewPackageName
                    from dbo.TblCustomerBillingInvoice bi where bi.DomainID like '%" + html.DomainID + "%' and CustomerID = " + CustomerID + " and ConnectivityStatus = 43 and StatusID = 11 and bi.IsChangePlan = 1";
            string UpgradeP = html.JsonData(sql);
            return Json(new { Data = Data, UpgradeP = UpgradeP }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CustomerChangePlan(string PrimaryFieldValue)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            Dictionary<string, string> Datainfo = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(PrimaryFieldValue);
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = @" select ac.ActivateCustomerID, c.CustomerName + ' [' + c.CustomerCode + ']' CustomerName,
                            s.ServiceName, pt.PackageTypeName, p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + bi.UM PackageName,
                            pd.Bandwidth, bi.UM, pd.DowSpeed, pd.UpSpeed, pd.Price, format(bi.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, format(bi.StartDate, 'dd-MM-yyyy') StartDate, format(bi.EndDate, 'dd-MM-yyyy') EndDate,
                            case when (DATEDIFF(D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate))) < 0 then 0 else DATEDIFF (D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate)) end Remain, ac.ServiceID, bi.PackageDetailID, bi.FreeDuration, bi.Duration
                            from dbo.TblActivateCustomer ac
                            inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
                            inner join dbo.TblServices s on s.ServiceID = ac.ServiceID
                            inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
                            inner join dbo.TblPackages p on p.PackageID = pd.PackageID
                            inner join dbo.TblPackageType pt on p.PackageTypeID = pt.PackageTypeID
                            inner join dbo.TblCustomerBillingInvoice bi on bi.ActivateCustomerID = ac.ActivateCustomerID
                            where ac.ActivateCustomerID = @ActivateCustomerID";
            Dictionary<string, string> Para = new Dictionary<string, string>
            {
                { "ActivateCustomerID", Datainfo["ActivateCustomerID"] }
            };
            DataSet ds = html.LoadData(sql, false, Para);
            DataRow dr = ds.Tables[0].Rows[0];
            html.TableColumn = 2;
            html.UseTable = true;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Customer Name:",
                ID = "CustomerName",
                DBField = "CustomerName",
                IsReadOnly = true,
                Value = dr["CustomerName"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Service Name:",
                ID = "ServiceName",
                DBField = "ServiceName",
                IsReadOnly = true,
                Value = dr["ServiceName"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Connection Type:",
                ID = "PackageTypeName",
                DBField = "PackageTypeName",
                IsReadOnly = true,
                Value = dr["PackageTypeName"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Name:",
                ID = "PackageName",
                DBField = "PackageName",
                IsReadOnly = true,
                Value = dr["PackageName"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Bandwidth:",
                ID = "Bandwidth",
                DBField = "Bandwidth",
                IsReadOnly = true,
                Value = dr["Bandwidth"].ToString()
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "UM:",
            //    ID = "UM",
            //    DBField = "UM",
            //    IsReadOnly = true,
            //    Value = dr["UM"].ToString()
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Download:",
                ID = "DowSpeed",
                DBField = "DowSpeed",
                IsReadOnly = true,
                Value = dr["DowSpeed"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Upload:",
                ID = "UpSpeed",
                DBField = "UpSpeed",
                IsReadOnly = true,
                Value = dr["UpSpeed"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Amount:",
                ID = "Price",
                DBField = "Price",
                IsReadOnly = true,
                Value = dr["Price"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Duration:",
                ID = "Txt_Duration",
                DBField = "Duration",
                IsReadOnly = true,
                Value = dr["UM"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Free Duration:",
                ID = "Txt_FreeDuration",
                DBField = "FreeDuration",
                IsReadOnly = true,
                Value = html.Pluralize("Month", int.Parse(dr["FreeDuration"].ToString()))
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Invoice Date:",
                ID = "InvoiceDate",
                DBField = "InvoiceDate",
                IsReadOnly = true,
                Value = dr["InvoiceDate"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Start Date:",
                ID = "StartDate",
                DBField = "StartDate",
                IsReadOnly = true,
                Value = dr["StartDate"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "End Date:",
                ID = "EndDate",
                DBField = "EndDate",
                IsReadOnly = true,
                Value = dr["EndDate"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Remain:",
                ID = "Remain",
                DBField = "Remain",
                IsReadOnly = true,
                Value = html.Pluralize("Day", int.Parse(dr["Remain"].ToString()))
            });
            html.FormID = "FrmCustChange_Plan";
            html.SaveChangeFunction = "SaveChangeCustomerChangePlan()";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            Para.Clear();
            html = new HTML();
            sql = @"select distinct pd.PackageDetailID, s.ServiceID,
                    s.ServiceName + ' - ' + p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' +  + pd.UM PackageName 
                    from dbo.TblServices s
                    inner join dbo.TblPackages p on p.ServiceID = s.ServiceID
                    inner join dbo.TblPackageDetail pd on pd.PackageID = p.PackageID
                    where s.ServiceID = " + dr["ServiceID"].ToString()
                    + " order by s.ServiceName + ' - ' + p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' +  + pd.UM";
            string sql_callback = @"select distinct pd.Price, pd.Duration, pd.FreeDuration
                    from dbo.TblServices s
                    inner join dbo.TblPackages p on p.ServiceID = s.ServiceID
                    inner join dbo.TblPackageDetail pd on pd.PackageID = p.PackageID
                    where pd.PackageDetailID = @SearchText";
            Para.Add("ServiceID", dr["ServiceID"].ToString());
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Package Name:",
                ID = "Cbo_PackageName",
                DBField = "PackageName",
                SQL_CBO = sql,
                DisplayText = "PackageName",
                Value = "PackageDetailID",
                Infinity = false,
                SQLCallBack = sql_callback,
                CallBackWithControlValueID = "Cbo_PackageName",
                DisplayOnFields = "TxtPackagePrice, TxtPackageDuration, TxtPackageFreeDuration",
                IsCallBack = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Duration:",
                ID = "TxtPackageDuration",
                ISNumeric = true,
                ISInteger = true,
                IsReadOnly = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Free Duration:",
                ID = "TxtPackageFreeDuration",
                ISNumeric = true,
                ISInteger = true,
                IsReadOnly = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Price:",
                ID = "TxtPackagePrice",
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlDateTimePicker()
            {
                Label = "Change Date:",
                ID = "ChangeDate",
                DBField = "ChangeDate"
            });
            html.HTMLControls.Add(new HtmlDateTimePicker()
            {
                Label = "End Date:",
                ID = "TxtEndDate",
                DBField = "EndDate",
                //Value = easternTimeNow.AddDays(int.Parse(dr["Remain"].ToString())).ToString("dd/MM/yyyy"),
                //IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Duration:",
                ID = "TxtRemainDuration",
                DBField = "RemainDuration",
                //Value= html.Pluralize("Day", int.Parse(dr["Remain"].ToString())),
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Amount to Pay:",
                ID = "TxtAmount",
                DBField = "Amount",
                ISNumeric = true
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Reason For Change:",
                ID = "ReasonForChagne",
                DBField = "ReasonForChagne"
            });
            ViewBag.Remain = dr["Remain"].ToString();
            html.FormID = "FrmCustChangePlan";
            html.SaveChangeFunction = "SaveChangeCustomerChangePlan()";
            ViewBag.NewPlaneForm = html.DrawHTMLForm();
            return View();
        }

        public ActionResult ProformatInvoiceToOfficialInvoice(object[] Data)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            Dictionary<string, string> paraData = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(Data[0].ToString());
            Dictionary<string, string> para = new Dictionary<string, string>();
            string InvoiceDate = html.GetCurrentDate();
            if (Data[0].ToString().Contains("InvoiceDate"))
            {
                InvoiceDate = paraData["InvoiceDate"];
            }
            para.Add("InvoiceDate", InvoiceDate);
            para.Add("InvoiceNo", paraData["InvoiceNo"]);
            para.Add("SystemID", html.SystemID);
            para.Add("WarehouseID", html.WarehouseID);
            sqlResult = html.SaveChangeData("SP_CONVERT_PI_TO_IN", true, para, true, true);
            DataSet ds = sqlResult.Data;
            DataRow dr = ds.Tables[0].Rows[0];
            string DataCallBack = html.JsonConvertDataTable(ds.Tables[0]);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, DataCallBack = DataCallBack }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadCustPieChart(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select isnull(format(sum(isnull(c.InvoiceAmount, 0)), '#,#0.00'), 0) InvoiceAmount,
isnull(format(sum(isnull(c.DepositFee,0)), '#,#0.00'), 0) DepositFee,
isnull(format(sum(isnull(c.PaidAmount,0)), '#,#0.00'), 0) PaidAmount, 
isnull(format(sum(isnull(c.InvoiceAmount, 0)) - sum(isnull(PaidAmount,0)), '#,#0.00'), 0) UnpaidAmount,
isnull(format(sum(isnull(c.PartiallyPaid,0)), '#,#0.00'), 0) PartiallyPaid,
format(
isnull(sum(isnull(c.AdvancePaymentAmount,0)), 0) 
+ (select isnull(sum(isnull(ca.InvoiceAmount, 0)) - sum(isnull(ca.PaidAmount,0)), 0) from dbo.TblCustomerAccount ca where ca.StatusID = 14 and ca.CustomerID = c.CustomerID)
,'#,#0.00') AdvancePaymentAmount,
isnull(format(sum(isnull(c.InputAmount,0)), '#,#0.00'), 0) InputAmount,
isnull(format(count(distinct c.InvoiceNo), '#,#0.00'), 0) CountOfficailInvoice
,(select isnull(format(sum(isnull(ca.InvoiceAmount, 0)) - sum(isnull(ca.PaidAmount,0)), '#,#0.00'), 0) from dbo.TblCustomerAccount ca where ca.StatusID = 14 and ca.CustomerID = c.CustomerID) CreditNote
from dbo.TblCustomerAccount c where CustomerID = @CustomerID and c.DomainID like '%" + html.DomainID + @"%'
and isnull(c.StatusID, 0) not in (12, 13, 14, 20) group by c.CustomerID";//[Type] = 'Official Invoice' and

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "CustomerID", CustomerID.ToString() }
            };
            SqlResult sqlResult = new SqlResult();
            DataSet ds = html.LoadData(sql, false, para);
            ViewBag.CustBalanceData = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                ViewBag.CustBalanceData = html.JsonConvertDataTable(ds.Tables[0]).ToString();
            }
            return View();
        }
        public JsonResult LoadCustConnection(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() OVER(ORDER BY c.CustomerID desc, bi.BillingInvoiceID desc, bi.ConnectionID desc) AS [No], c.CustomerCode, 
            c.CustomerName, case when bi.IsRenew = 1 then bi.[SID] + ' (Renew)' else isnull(bi.[SID], '<span class=""text-red"">Pending</span>') end [SID], bi.InvoiceNo,
            (bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) InstallAddress,
            '<a class=""text-dark"" href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, bi.CustomerID) + ', '''+ bi.InvoiceNo +''','+convert(varchar, bi.IsIncludedVAT)+','+convert(varchar, bi.StatusID)+','''',false,this);"">' + bi.ConnectionID + '</a>' ConnectionID,
            '<a href=""javascript:void(0);"" onclick=""ShowDialogBox(''Activation Customer'', ''Auto'', ''500'', ''ActivateCustomer/Activate'', { CID: ''' + c.CustomerCode + ''', CustomerID:'+convert(varchar, bi.CustomerID)+', CustomerName:'''+(c.CustomerName + ' [' + c.CustomerCode + ']')+''', ServiceID: '+convert(varchar, bi.ServiceID) +', ConnectionTypeID:'+convert(varchar, p.PackageTypeID)+',PackageDetailID:' + convert(varchar, bi.PackageDetailID) + ', BillingInvoiceID:'+convert(varchar, bi.BillingInvoiceID)+', IsOnCustProfile:true, ActivateCustomerID: '+convert(varchar, isnull(bi.ActivateCustomerID,0))+', RouterID:'+convert(varchar, isnull(bi.RouterID,0)) + '});"">' + s.[Status] + '</a>' [Status], (case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end) PackageName,
            format(c.RegisterDate, 'dd-MM-yyyy') RegisterDate, format(bi.StartDate, 'dd-MM-yyyy') StartDate, 
            format(bi.EndDate, 'dd-MM-yyyy') EndDate, (c.CustomerName + ' [' + c.CustomerCode + ']') CustName, c.RegisterBy,
            '<span style=""color:red"">' + convert(varchar, (select count(distinct _bi.ConnectionID) from dbo.TblCustomerBillingInvoice _bi where _bi.CustomerID = bi.CustomerID and _bi.StatusID not in (12,13,20))) + '</span>' CountConnection,
            convert(varchar, DATEDIFF(D, getdate(), bi.EndDate))+ ' days' RemainDay
            from dbo.TblCustomer c
            inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
            inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = bi.CustomerBillInformationID
            inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
            inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
            inner join dbo.TblPackages p on p.PackageID = bi.PackageID
            where bi.DomainID like '%" + html.DomainID + "%' and bi.CustomerID = " + CustomerID + " and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.StatusID not in(12,13,20)";

            sql += ";select (select count(distinct bi.ConnectionID) ";
            sql += "from dbo.TblCustomerBillingInvoice bi ";
            sql += "where bi.ConnectivityStatus = 40 and bi.CustomerID = " + CustomerID + " and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.StatusID not in(12,13,20)) Active, ";
            sql += "(select count(distinct bi.ConnectionID) ";
            sql += "from dbo.TblCustomerBillingInvoice bi ";
            sql += "where bi.ConnectivityStatus = 41 and bi.CustomerID = " + CustomerID + " and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.StatusID not in(12,13,20)) Lock, ";
            sql += "(select count(distinct bi.ConnectionID) ";
            sql += "from dbo.TblCustomerBillingInvoice bi ";
            sql += "where bi.DomainID like '%" + html.DomainID + "%' and bi.ConnectivityStatus = 43 and bi.CustomerID = " + CustomerID + " and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.StatusID not in(12,13,20)) Inactive;";
            html.GH.SQL = sql;
            html.TableName = "tblcustconnection";
            html.TblID = "tblcustconnection";
            html.TableClassName = "table table-bordered table-head-fixed text-nowrap table-responsive";
            html.isShowLoadingGrid = false;
            html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 1;
            html.GridEventClick = "ShowCustomer_ConnectionInv(this)";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", IsGroupByField = true, GroupLabel="CID: ", Colspan=10 },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", IsGroupByField = true, GroupLabel="Name: ", Colspan=10 },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", IsGroupByField = true, GroupLabel="Reg.Date: ", Colspan=10 },
                new GridField() { Header = "Register By", FieldName = "RegisterBy", IsGroupByField = true, GroupLabel="Reg.By: ", Colspan=10 },
                new GridField() { Header = "Count Connection", FieldName = "CountConnection", IsGroupByField = true, GroupLabel="Connection: ", Colspan=10 },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Service ID", FieldName = "SID" },
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo" },
                new GridField() { Header = "Status", FieldName = "Status", ClassName="text-red", TextAlign="center" },
                new GridField() { Header = "Package Name", FieldName = "PackageName", ClassName="text-info" },
                new GridField() { Header = "Start Date", FieldName = "StartDate" },
                new GridField() { Header = "End Date", FieldName = "EndDate", ClassName="text-red" },
                new GridField() { Header = "Remain Day", FieldName = "RemainDay", ClassName="text-red" },
                new GridField() { Header = "Install Address", FieldName = "InstallAddress" }
            };
            //html.DefaultSearch = false;
            //html.DrawTotalAllHeader = true;
            //html.DrawTotalAllHeaderDataTableNumber = 2;
            //html.GridEventClick = "ShowCustomerInfo(this)";
            //ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return Json(new { Data = html.DrawHTMLSearchForm(1, "LoadCustConnection") }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadTransaction(int CustomerID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false
            };
            //string sql = "select ROW_NUMBER() OVER(ORDER BY p.ReceiptNo desc, p.InvoiceNo) as [No], ";
            //sql += "p.ReceiptDate, p.ReceiptBy, ";
            //sql += "'<a href=\"Payment/OfficialReceipt/?PaymentID='+convert(varchar, p.PaymentID) +'&CompanyID='+convert(varchar, p.CompanyID) +'&OfficialReceiptNo='+p.ReceiptNo+'&InvoiceNo='+p.InvoiceNo+'&SystemID='+convert(varchar, p.SystemID)+'&WarehouseID='+convert(varchar, p.WarehouseID)+'\" target=\"_blank\">' + p.ReceiptNo + '</a>' ReceiptNo, ";
            //sql += "('<a name=\"APreviewInv\" href=\"CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + "&ID='+convert(varchar, p.CustomerID)+'&InvNo='+p.InvoiceNo+'&ISVAT='+ convert(varchar, p.IsIncludedVAT) +'&StatusID='+ convert(varchar, p.StatusID) +'&AutoPrint=false&index=0&InvoiceType=UnpaidInvoice\" onclick=\"PrintInvoice(' + convert(varchar, p.CustomerID) +', '''+ p.InvoiceNo +''','+ convert(varchar, p.IsIncludedVAT) +','+ convert(varchar, p.StatusID) +','''',false,this);\" target=\"_blank\">' + p.InvoiceNo + '</a>') InvoiceNo, ";
            //sql += "(ca.AccountNo + ' - ' + ca.AccountName + ' (' + ca.AccountType + ')')  AccountName, ";
            //sql += "'Income' [Type], p.[Description], p.GrandTotal, p.TotalPaid, p.UnpaidAmount ";
            //sql += "from dbo.TblCustomerAccount p ";
            //sql += "inner join dbo.TblChartOfAccount ca on ca.ChartOfAccountID = p.ChartOfAccountID ";
            //sql += "where isnull(p.ReceiptNo, '') <> '' and ";
            //sql += "p.DomainID like '%"+html.DomainID+"%' and p.CustomerID = " + CustomerID + " and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID + " and isnull(p.StatusID,0) not in(12,13,20)";
            string sql = @"select ROW_NUMBER() OVER(ORDER BY cp.OfficialReceiptNo desc, cp.InvoiceNo) as [No],
'<a href=""Payment/OfficialReceipt/?PaymentID='+convert(varchar, cp.PaymentID) +'&CompanyID='+convert(varchar, cp.CompanyID) +'&OfficialReceiptNo='+cp.OfficialReceiptNo+'&InvoiceNo='+cp.InvoiceNo+'&SystemID='+convert(varchar, cp.SystemID)+'&WarehouseID='+convert(varchar, cp.WarehouseID)+'"" target=""_blank"">' + cp.OfficialReceiptNo + '</a>' ReceiptNo,
'<a name=""APreviewInv"" href=""CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + @"&ID='+convert(varchar, cp.CustomerID)+'&InvNo='+cp.InvoiceNo+'&ISVAT=0&StatusID=10&AutoPrint=false&index=0&InvoiceType=UnpaidInvoice"" onclick=""PrintInvoice(' + convert(varchar, cp.CustomerID) +', '''+ cp.InvoiceNo +''',0,10,'''',false,this);"" target=""_blank"">' + cp.InvoiceNo + '</a>' 
InvoiceNo, cp.InvoiceDate, cp.Total, cp.PaymentDate, cp.Paid PaymentAmount,  cp.Unpaid RemainingBalance, coa.AccountNo + ' - '+ coa.AccountName 'DepositToAccount',
cp.[Description], cp.PaidBy
from dbo.TblCustomerPayment cp
inner join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = cp.ChartOfAccountID
where PaymentID is not null and CustomerID = " + CustomerID;
            html.GH.SQL = sql;
            html.OrderBy = "cp.OfficialReceiptNo desc";
            html.TblID = "tbltransaction";
            html.TableClassName = "table table-bordered table-head-fixed text-nowrap table-responsive";
            html.isShowLoadingGrid = false;
            html.ShowPaging = true;
            html.ShowGridOnly = true;
            html.ShowSearchButton = false;
            //html.JqueryDataTable = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice No.", FieldName = "InvoiceNo" },
                new GridField() { Header = "Receipt No.", FieldName = "ReceiptNo" },
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", Format="dd-MM-yy" },// HH:mm:ss tt
				new GridField() { Header = "Total", FieldName = "Total", TextAlign = "right" },
                new GridField() { Header = "Payment Date", FieldName = "PaymentDate", Format="dd-MM-yy HH:mm:ss tt" },
                new GridField() { Header = "Payment Amount", FieldName = "PaymentAmount", TextAlign = "right" },
                new GridField() { Header = "Remaining Balance", FieldName = "RemainingBalance", TextAlign = "right" },
                new GridField() { Header = "Deposit To Account", FieldName = "DepositToAccount" },
                new GridField() { Header = "Description", FieldName = "Description" },
                new GridField() { Header = "Paid By", FieldName = "PaidBy" },
            };
            return Json(new { Data = html.DrawHTMLSearchForm(1, "LoadCustTransaction") }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadInvoiceForCreditNotes(int ID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false,
                AllowAdd = true,
                AllowDelete = true,
                AllowEdit = true,
                AddButton = "Create Credit Notes",
                AddFUNCTION = "AddCreditNotes(" + ID.ToString() + ")"
            };

            string sqla = "select distinct top 10 CreditNoteNo from dbo.TblCustomerBalance where lower(CreditNoteNo) like @SearchText and StatusID = 14 and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID;
            if (ID != 0)
                sqla += " and CustomerID = " + ID;
            sqla += " order by CreditNoteNo";
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Credit Note#",
                SQL = sqla,
                RadioValue = EncryAndDecry.Encryp("CreditNoteNo"),
                DefaultCheckBok = true,
                ISAutoComplete = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string SQL = @"select ROW_NUMBER() OVER(ORDER BY cb.CreditNoteNo desc) as [No],
c.CustomerCode CID, c.CustomerName, cb.CustomerID,
('<a name=""APreviewInv"" href=""CustomerInvoice/?SystemID=" + html.SystemID + "&WarehouseID=" + html.WarehouseID + @"&ID='+convert(varchar, cb.CustomerID)+'&InvNo='+CreditNoteNo+'&ISVAT='+ convert(varchar, cb.IsIncludedVAT) +'&StatusID='+ convert(varchar, cb.StatusID) +'&AutoPrint=false&index=0&InvoiceType=PaidInvoice&Type=Credit Notes"" onclick=""PrintInvoice('+convert(varchar, cb.CustomerID)+', '''+ cb.CreditNoteNo +''','+ convert(varchar, cb.IsIncludedVAT) +','+ convert(varchar, cb.StatusID) +','''',false, this,''PaidInvoice'',false,false,'''','''',''Credit Notes'');"">' + cb.CreditNoteNo + '</a>') CreditNoteNo,
cb.CreditNoteNo CreditNoteNumber, cb.InvoiceDate, cb.PaymentDueDate DueDate, ReferenceNo,
s.[Status], cb.GrandTotal, cb.Unpaid, cb.CancelComment, case when cb.Unpaid > 0 then '<a href=""javascript:void(0);"" onclick=""ApplyToInvoices('''+cb.CreditNoteNo+''','+convert(varchar, cb.CustomerID)+', '''+c.CustomerName+''');"">Apply To Invoices</a>' else 'Closed' end ApplyToInvoices
from dbo.TblCustomerBalance cb
inner join dbo.TblStatus s on s.StatusID = cb.StatusID
inner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID
where cb.DomainID like '%" + html.DomainID + "%' and cb.StatusID = 14 and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID;
            if (ID != 0)
                SQL += " and c.CustomerID = " + ID;
            html.GH.SQL = SQL;
            html.OrderBy = "cb.CreditNoteNo desc";
            html.IsHaveWhereCondition = true;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },                
                //new GridField() { Header = "Account", FieldName = "AccountName" },
				new GridField() { Header = "Date", FieldName = "InvoiceDate", Format="dd-MM-yyyy" },
                new GridField() { Header = "Credit Note#", FieldName = "CreditNoteNo" },
                new GridField() { Header = "Ref.No", FieldName = "ReferenceNo" },
                new GridField() { Header = "CID", FieldName = "CID", Link = "#!/CustomerProfile?ID=", LinkParameterField = "CustomerID", NewPage = true },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", Link = "#!/CustomerProfile?ID=", LinkParameterField = "CustomerID", NewPage = true },				
				//new GridField() { Header = "Inv.Date", FieldName = "InvoiceDate", Format="dd-MM-yy", TextAlign="center" },
				//new GridField() { Header = "Due Date", FieldName = "DueDate", Format="dd-MM-yy", TextAlign="center" },
				new GridField() { Header = "Amount", FieldName = "Unpaid", TextAlign="right", Format = "#,#.00",
                    IsTotalFooter=true, Colspan = 5, TotalLabel="Total:" },
                new GridField() { Header = "Apply To Invoices", FieldName = "ApplyToInvoices", TextAlign="center" },
                new GridField() { Header = "Comment", FieldName = "CancelComment" },
                new GridField() { Header = "Action", FieldName = "CreditNoteNumber", TextAlign = "Center",
                    IsModifyField = true,
                    EditFunction ="Edit_Tbl_Row(this);",
					//DeletedCallBackFunction="LoadCustInvoice(" + ID + ")",
					DeleteUseProcedureName="SP_DELETE_CUSTOMER_CREDITNOTE",
                }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadCreditNotes");
            return View();
        }
        public ActionResult ApplyToInvoices(string CreditNoteNo, int CustomerID, string CustomerName)
        {
            ViewBag.CreditNoteNo = CreditNoteNo;
            ViewBag.CustomerID = CustomerID;
            ViewBag.CustomerName = CustomerName;
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over(order by cb.InvoiceNo) as [No],
cb.CustomerID, cb.InvoiceNo, cb.InvoiceDate, cb.GrandTotal, cb.Unpaid, '' CreditstoApply
from dbo.TblCustomerBalance cb
where cb.DomainID like '%" + html.DomainID + @"%' and cb.StatusID = 11 and cb.StatusID not in (12,13,14,20) and [Type] = 'Official Invoice'
and cb.CustomerID = " + CustomerID;
            html.OrderBy = "cb.InvoiceNo";
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.ShowPrintOnTopTable = false;
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo", },
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", Format = "dd-MM-yyyy" },
                new GridField() { Header = "Invoice Amount", FieldName = "GrandTotal", Format ="#,#0.00", TextAlign = "right" },
                new GridField() { Header = "Invoice Balance", FieldName = "Unpaid", Format ="#,#0.00", TextAlign = "right" },
                new GridField() { Header = "Credits to Apply", FieldName = "CreditstoApply", TextAlign="center", DisplayTextBox = true, },
            };
            html.TblID = "TblInvoiceList";

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "InvoiceList");

            sql = @"select ca.Unpaid 
from dbo.TblCustomerBalance ca where ca.DomainID like '%" + html.DomainID + "%' and ca.StatusID = 14 and ca.CustomerID = " + CustomerID + " and ca.CreditNoteNo = '" + CreditNoteNo + "'";
            ViewBag.InvoiceAmount = double.Parse(html.LoadData(sql).Tables[0].Rows[0][0].ToString()).ToString("N2");
            return View();
        }

        public ActionResult SaveChangeApplyCreditToInvoice(string Data)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(Data.ToString());
            foreach (dynamic item in DataInfo)
            {
                string CreditNoteNo = item["CreditNoteNo"].ToString();
                string CustomerID = item["CustomerID"].ToString();
                string Payer = item["CustomerName"].ToString();
                string ReceivedFromName = item["CustomerName"].ToString();
                string InvoiceNo = item["InvoiceNo"].ToString();
                string PaymentAmount = item["PaymentAmount"].ToString();
                string InvoiceAmount = item["InvoiceAmount"].ToString();
                string InvoiceBalance = item["InvoiceBalance"].ToString();
                string PaymentDate = html.GetCurrentDate();
                string PaidBy = html.UserFullName;
                string Type = "Income";
                string SystemID = html.SystemID.ToString();
                string WarehouseID = html.WarehouseID.ToString();

                string sql = @"insert into dbo.TblPayment (CustomerID, Payer, ReceivedFromName, InvoiceNo, PaymentAmount,
PaymentDate, PaidBy, [Type], SystemID, WarehouseID, DomainID) values (@CustomerID, @Payer, @ReceivedFromName, @InvoiceNo, @PaymentAmount,
@PaymentDate, @PaidBy, @Type, @SystemID, @WarehouseID, @DomainID); select @@IDENTITY;";
                Dictionary<string, string> para = new Dictionary<string, string>
                {
                    {"CustomerID",CustomerID },
                    {"Payer",Payer },
                    {"ReceivedFromName",ReceivedFromName },
                    {"InvoiceNo",InvoiceNo },
                    {"PaymentAmount",PaymentAmount },
                    {"PaymentDate",PaymentDate },
                    {"PaidBy",PaidBy },
                    {"Type",Type },
                    {"SystemID",SystemID },
                    {"WarehouseID",WarehouseID },
                    {"DomainID",html.DomainID },
                };
                result = html.SaveChangeData(sql, false, para, true);
                int StatusID = 10; double PartiallyPaid = 0;
                if (PaymentAmount == InvoiceBalance)
                {
                    PartiallyPaid = 0;
                    StatusID = 10;
                    sql = @"update dbo.TblCustomerBalance set PaymentID = " + result.Value + ", StatusID = 10, PaidDate = '" + PaymentDate + @"',
Paid = " + PaymentAmount + ", Unpaid = 0, PaidBy = '" + PaidBy + @"'
where InvoiceNo = '" + InvoiceNo + "' and CustomerID = " + CustomerID + " and SystemID = " + SystemID + " and WarehouseID = " + WarehouseID + @";
update dbo.TblCustomerAccount set PartiallyPaid = 0 where CustomerID = " + CustomerID + " and InvoiceNo = '" + InvoiceNo + "' and SystemID = " + SystemID + " and WarehouseID = " + WarehouseID + @";";
                }
                else if (double.Parse(PaymentAmount) < double.Parse(InvoiceBalance))
                {
                    PartiallyPaid = double.Parse(PaymentAmount);
                    StatusID = 11;
                    sql = @"update dbo.TblCustomerBalance set PaymentID = " + result.Value + ", StatusID = 11, PaidDate = '" + PaymentDate + @"',
Paid = (Paid + " + PaymentAmount + @"), Unpaid = " + (double.Parse(InvoiceBalance) - double.Parse(PaymentAmount)) + @", Balance = 0, PaidBy = '" + PaidBy + @"' 
where CustomerID = " + CustomerID + " and InvoiceNo = '" + InvoiceNo + "' and SystemID = " + SystemID + " and WarehouseID = " + WarehouseID + ";";
                }

                sql += @"insert into TblCustomerPayment (CustomerID, InvoiceNo, Deposit, Total, Paid, Unpaid, 
PaymentDate, PaidBy, StatusID, [Type], SystemID, WarehouseID, DomainID)
values (" + CustomerID + ",'" + InvoiceNo + "',0," + InvoiceAmount + "," + PaymentAmount + "," + (double.Parse(InvoiceBalance) - double.Parse(PaymentAmount)) + @",
'" + PaymentDate + "','" + PaidBy + "'," + StatusID + ",'Income', " + SystemID + "," + WarehouseID + ", " + html.DomainID + @");
insert into dbo.TblPaymentHistory (PaymentID, Payer, PayDate, PayBy, PayAmount, ReceivedFromName,
CustomerID, InvoiceNo, SystemID, WarehouseID, DomainID)
values (" + result.Value + ",'" + Payer + "','" + PaymentDate + "','" + PaidBy + "'," + PaymentAmount + ",'" + ReceivedFromName + "', " + CustomerID + ",'" + InvoiceNo + @"',
" + SystemID + "," + WarehouseID + ",'" + html.DomainID + @"');
update dbo.TblCustomerBillingInvoice set PaymentID = " + result.Value + ", StatusID = " + StatusID + " where CustomerID = " + CustomerID + " and InvoiceNo = '" + InvoiceNo + "' and SystemID = " + SystemID + " and WarehouseID = " + WarehouseID + @";
update dbo.TblCustomerInvoice set PaymentID = " + result.Value + ", StatusID = " + StatusID + " where CustomerID = " + CustomerID + " and InvoiceNo = '" + InvoiceNo + "' and SystemID = " + SystemID + " and WarehouseID = " + WarehouseID + @";

insert into TblCustomerAccount (CreditNoteNo, CustomerID, InvoiceNo, InvoiceDate, InvoiceDueDate, InvoiceBy,
DepositFee, InvoiceAmount, PaidAmount, UnpaidAmount, PartiallyPaid, AdvancePaymentAmount, [Type], InputAmount, 
PaymentID, GrandTotal, TotalPaid, IsIncludedVAT, StatusID, SystemID, WarehouseID, ReceiptDate, DomainID)
select '" + CreditNoteNo + @"',CustomerID, InvoiceNo, InvoiceDate, PaymentDueDate, CreateBy,
0 DepositFee, 0, " + PaymentAmount + @" PaidAmount, Unpaid,
" + PartiallyPaid + ", 0, [Type], " + PaymentAmount + @", " + result.Value + ", GrandTotal, " + PaymentAmount + @", IsIncludedVAT, 
StatusID, " + SystemID + "," + WarehouseID + ", '" + PaymentDate + "', '" + html.DomainID + @"' from TblCustomerBalance
where InvoiceNo = '" + InvoiceNo + "' and SystemID = " + SystemID + " and WarehouseID = " + WarehouseID + " and DomainID like '%" + html.DomainID + "%'" + @";

insert into TblCustomerAccount (CreditNoteNo, CustomerID, InvoiceNo, InvoiceAmount, PaidAmount, InputAmount, 
PaymentID, StatusID, ReceiptDate, SystemID, WarehouseID, DomainID)
values ('" + CreditNoteNo + "', " + CustomerID + ", '" + InvoiceNo + "',0," + PaymentAmount + "," + PaymentAmount + @",
" + result.Value + ",14,'" + PaymentDate + "'," + SystemID + "," + WarehouseID + ",'" + html.DomainID + @"');
update dbo.TblCustomerBalance set Paid=" + PaymentAmount + ", Unpaid = (Unpaid - " + PaymentAmount + @") 
where CreditNoteNo = '" + CreditNoteNo + "' and CustomerID = " + CustomerID + @";

update dbo.TblCustomerInvoice set InvoiceNo ='" + InvoiceNo + "', PaymentID = " + result.Value + @"
where CustomerID = " + CustomerID + " and CreditNoteNo = '" + CreditNoteNo + @"';
update dbo.TblCustomerBillingInvoice set InvoiceNo ='" + InvoiceNo + "', PaymentID = " + result.Value + @"
where CustomerID = " + CustomerID + " and CreditNoteNo = '" + CreditNoteNo + @"';";
                result = html.SaveChangeData(sql, false, false);
            }

            return Json(new { Result = result.Result, Message = result.Message, Data = "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowCustActivateLog(int CustomerID, bool ShowDetail, string PPPoEUserName, string ConnectionID)
        {
            HTML html = new HTML();
            ViewBag.CustomerID = CustomerID;
            ViewBag.ShowDetail = ShowDetail;
            ViewBag.PPPoEUserName = PPPoEUserName;
            html.ReportTitle = "list Customers Activity Connection Log";
            //html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            //{
            //    DefaultValueDate = false,
            //    DateRangePicker = true,
            //    LabelFromDate = "Start Date:",
            //    DBField = "bi.StartDate"
            //});
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Connection ID",
                SQL = "select ConnectionID from dbo.TblActivateCustomer where CustomerID =" + CustomerID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                DisplayField = "ConnectionID",
                SelectValue = "ConnectionID",
                RadioValue = EncryAndDecry.Encryp("ac.ConnectionID"),
                DefaultCheckBok = true,
                Value = ConnectionID
            });
            string sql = @"select ROW_NUMBER() over(order by ac.ConnectionID) as[No], ha.[Date], ac.ConnectionID, 
            ac.[SID], ac.PPPoEProfileName ProfileName, ha.ActivateBy, ac.ActivateDate, ha.Comment, s.[Status]
            from dbo.TblHistoryActivateCustomer ha
            inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ha.ActivateCustomerID
            inner join dbo.TblStatus s on s.StatusID = ha.ConnectivityStatus
            where ha.DomainID like '%" + html.DomainID + "%' and ha.CustomerID = " + CustomerID + " and ha.SystemID = " + html.SystemID + " and ha.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "[No], ac.ConnectionID desc";
            html.IsHaveWhereCondition = true;
            html.GH.SQL = sql;
            html.isShowLoadingGrid = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Date", Format="dd-MM-yy hh:mm:ss tt" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Service ID", FieldName = "SID" },
                new GridField() { Header = "Profile Name", FieldName = "ProfileName" },
                new GridField() { Header = "Action By", FieldName = "ActivateBy" },
                new GridField() { Header = "Activate Date", FieldName = "ActivateDate", Format="dd-MM-yy hh:mm:ss tt" },
                new GridField() { Header = "Comment", FieldName = "Comment" },
                new GridField() { Header = "Status", FieldName = "Status" }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public JsonResult GetPPPeEUserOnline(bool ShowDetail, string PPPoEUserName)
        {
            Mikrotik mikrotik = new Mikrotik();
            return Json(new { Data = mikrotik.GetPPPeEUserOnline(ShowDetail, true, PPPoEUserName) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Change Device")]
        public ActionResult ChangeDevice(int CustomerID, string PrimaryFieldValue = "0")
        {
            HTML html = new HTML
            {
                FormID = "CreateChangeDevice",
                //html.FieldIDFocus = "TxtDepartmentName";
                //html.TableName = "TblDepartment";
                //html.PrimaryField = "DepartmentID";
                SaveChangeCallbackFunction = "CallBackSaveChangeDevice();"
            };
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Current Device Information",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Item Name:", ID="Cbo_CurrentItemName",
                            Required=true,
                            isTypeahead=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            SQL="select distinct di.ActivateCustomerID, di.CustomerDeviceInfoID, di.CustomerDeviceInfoDetailID, ac.BillingInvoiceID, i.ItemID, i.ItemName + case when isnull(i.ItemCode, '') <> '' then ' - ' + i.ItemCode else case when isnull(i.SerialNumber,'') <> '' then ' - ' + i.SerialNumber else '' end end ItemName, di.SerialNo SerialNumber, di.MACAddress, ac.ConnectionID, ac.[SID], di.InvoiceNo\r\nfrom dbo.TblItem i\r\ninner join dbo.TblCustomerDeviceInfoDetail di on di.ItemID = i.ItemID\r\ninner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = di.ActivateCustomerID\r\nwhere (di.[Status] = 'New Install' or di.[Status] = 'Active') and di.CustomerID="+CustomerID+" and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " and i.DomainID like '%"+html.DomainID+"%' order by i.ItemName + case when isnull(i.ItemCode, '') <> '' then ' - ' + i.ItemCode else case when isnull(i.SerialNumber,'') <> '' then ' - ' + i.SerialNumber else '' end end",
                            FieldName="ItemName",
                            HiddenField="ActivateCustomerID, CustomerDeviceInfoID, CustomerDeviceInfoDetailID, BillingInvoiceID",
                            HiddenFieldValue="0,0,0,0",
                            DisplayOnFields=JsonConvert.SerializeObject(new {
                                ActivateCustomerID="ActivateCustomerID",
                                CustomerDeviceInfoID="CustomerDeviceInfoID",
                                CustomerDeviceInfoDetailID="CustomerDeviceInfoDetailID",
                                BillingInvoiceID="BillingInvoiceID",
                                IName="ItemName",
                                CurrentMACAddress="MACAddress",
                                CurrentSerialNo="SerialNumber",
                                CurrentConnectionID="ConnectionID",
                                CurrentSID="SID",
                                CurrentInvoiceNo="InvoiceNo"
                            })
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Serial No:", ID="CurrentSerialNo",
                            Class="input2 CustInfo", IsCustomForm = true, IsReadOnly = true
                        },
                        new HtmlTextBox()
                        {
                            Label="MAC Address:", ID="CurrentMACAddress",
                            Class="input2 CustInfo", IsCustomForm = true, IsReadOnly = true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="ConnectionID:", ID="CurrentConnectionID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm", IsReadOnly=true,
                        },
                        new HtmlTextBox()
                        {
                            Label="SID:", ID="CurrentSID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm", IsReadOnly = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Invoice#:", ID="CurrentInvoiceNo",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm", IsReadOnly = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="CustomerID:", ID="Txt_CustomerID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            IsReadOnly = true, Hide=true, Value=CustomerID.ToString()
                        },
                    }
                },
                new  FormData() {
                    FormTitle="Choose New Device",
                    TableName="TblCustomerDeviceInfoDetail",
                    GetIDENTITYByFiled="CustomerDeviceInfoDetailID",
                    PrimaryFieldName="CustomerDeviceInfoDetailID",
                    SQL="update dbo.TblCustomerDeviceInfoDetail set [Status]='Changed', Reason = @Txt_Reason + '; Changed Device From: <span class=''text-danger''>'+ @Cbo_CurrentItemName + '</span> To: <span class=''text-primary''>' + @cboItemName + '</span>' where CustomerDeviceInfoDetailID = @Cbo_CurrentItemName_CustomerDeviceInfoDetailID",
                    UpdateFiledName="Cbo_CurrentItemName_CustomerDeviceInfoDetailID, Txt_Reason, Cbo_CurrentItemName, cboItemName",
                    dbField = new DBField[]
                    {
                        new DBField{DBFieldName="ActivateCustomerID", Value="Cbo_CurrentItemName_ActivateCustomerID", GetValueFromInputName = true},
                        new DBField{DBFieldName="CustomerID", Value="Txt_CustomerID", GetValueFromInputName = true},
                        new DBField{DBFieldName="ItemID", Value="ItemID", GetValueFromInputName = true},
                        new DBField{DBFieldName="Type", Value="UM_StockName", GetValueFromInputName = true},
                        new DBField{DBFieldName="QTY", Value="UM_QTY", GetValueFromInputName = true},
                        new DBField{DBFieldName="LocationID", Value="cbo_Warehouse_Name_LocationID", GetValueFromInputName = true},
                        //new DBField{DBFieldName="Reason", Value="Txt_Reason", GetValueFromInputName = true, }//SetValue="$(\"#Txt_Reason\").val()+'; Change Router from:'+$(\"#Cbo_CurrentItemName\").val()+' TO: ' + $(\"#cboItemName\").val()"},
                        new DBField{DBFieldName="CustomerDeviceInfoID", Value="Cbo_CurrentItemName_CustomerDeviceInfoID", GetValueFromInputName = true},
                        new DBField{DBFieldName="InvoiceNo", Value="CurrentInvoiceNo", GetValueFromInputName = true},
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Warehouse Name:",
                            ID = "cbo_Warehouse_Name",
                            SQL = "select uw.WID LocationID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName from dbo.TblUserWarehouse uw\r\ninner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " order by (b.BranchName + ' - ' + uw.WarehouseName)",
                            DBField = "WarehouseName",
                            HiddenField = "LocationID",
                            isTypeahead = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            Required = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Item Name:",
                            ID = "cboItemName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ItemName",
                            Required = true,
                            HiddenField="ItemID",
                            ISAutoComplete = true,
                            SQL_AutoComplete=@"select distinct s.ItemID, i.ItemName + case when isnull(i.ItemCode, '') <> '' then ' - ' + i.ItemCode else case when isnull(i.SerialNumber,'') <> '' then ' - ' + i.SerialNumber else '' end end ItemName, i.SellingPrice SalePrice, 
i.UM, i.SerialNumber, sn.StockName, s.LocationID, uw.WarehouseName
from dbo.TblStockInDetail s
inner join dbo.TblItem i on i.ItemID = s.ItemID
inner join dbo.TblStockName sn on sn.StockNameID = i.StockNameID
inner join dbo.TblUserWarehouse uw on uw.WID = s.LocationID
where lower(i.ItemName + (case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else case when isnull(i.SerialNumber,'') <> '' then ' - ' + isnull(i.SerialNumber,'') else '' end end)) like @SearchText and s.LocationID= @SearchText2 and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID+@"
group by s.ItemID, i.ItemName, i.SellingPrice, i.UM, i.SerialNumber, i.ItemCode, sn.StockName, s.LocationID, uw.WarehouseName
having (sum(isnull(s.QTY,0))-sum(isnull(s.QTYOUT,0))) > 0",
                            SQLCallBack=@"select distinct i.UM, i.SerialNumber TxtSerialNo, sn.StockName UM_StockName, sd.LocationID , uw.WarehouseName,
isnull(sd.CostPrice,0) UM_CostPrice, sd.StockInID UM_StockInID
from dbo.TblItem 
i inner join dbo.TblStockName sn on sn.StockNameID = i.StockNameID
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblUserWarehouse uw on uw.WID = sd.LocationID
where i.ItemID = @SearchText and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID,
                            DisplayOnFields="UM, TxtSerialNo, UM_StockName, LocationID,WarehouseName, UM_CostPrice, UM_StockInID",
                            CboValue="cbo_Warehouse_Name_LocationID",
                        },
                        new HtmlTextBox()
                        {
                            Label = "UM:",
                            ID="UM",
                            Hide = true,
                            DBField="UM",
                            HiddenFieldValue="0,1,0,0",
                            HiddenField="StockName,QTY,CostPrice,StockInID"
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Serial No:",
                            ID = "TxtSerialNo",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SerialNo"
                        },
                        new HtmlTextBox()
                        {
                            Label = "MAC Address:",
                            ID = "TxtMACAddress",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="MACAddress"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        //new HtmlComboBox()
                        //{
                        //    Label = "Status:",
                        //    ID = "cboStatus",
                        //    SQL_CBO="select StatusID, [Status] from dbo.TblRouterDeviceStatus where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Status]",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DisplayText="Status",
                        //    Value="Status",
                        //    DBField="Status",
                        //    DivClassCol="col-sm-2", AddNewFuntionName="AddNewStatus();",
                        //    Required = true
                        //},
                        new HtmlTextBox()
                        {
                            Label = "Status:",
                            ID = "TxtStatus",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Status",
                            Required = true, Value="Active", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label = "Remark:",
                            ID = "Txt_Reason",
                            Class="input2 CustInfo", IsCustomForm = true,
                            SQL_AutoComplete="select Reason from dbo.TblStockReason\r\nwhere lower(Reason) like @SearchText and SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID +" order by Reason",
                            //DBField="Reason",
                            Required = true, ISAutoComplete = true
                        },
                        new HtmlTextBox()
                        {
                            Label = "Remark:",
                            ID = "Txt_Remark",
                            Class="input2 CustInfo", IsCustomForm = true,
                            //DBField="Remark",
                            //Required = true,
                            Hide= true,
                            //Value="Change Router from"
                        },new HtmlTextBox()
                        {
                            Label = "Date:",
                            ID = "Txt_Date",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Date",
                            SingleDatePicker=true,
                            //Value=html.GetCurrentDate(),
                            //Hide= true,
                        },
                        //new HtmlComboBox()
                        //{
                        //    Label = "Reason:",
                        //    ID = "Txt_Reason",
                        //    SQL_CBO = "select ReasonID, Reason from dbo.TblStockReason where SystemID = "+html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by Reason",
                        //    DisplayText = "Reason",
                        //    Value = "Reason",
                        //    DBField="Reason",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    AddNewFuntionName="AddNewStockReason()",
                        //    Required = true
                        //},

                        //new HtmlTextBox()
                        //{
                        //    Label="NSM:", ID="NSM", DivClass="noborder", DivClassCol="col-sm-auto",
                        //    IsCustomForm = true,
                        //    DBField="NSM", Type="checkbox", DefaultCheckBoxChecked = true, Value="1"
                        //},
                        //new HtmlTextBox()
                        //{
                        //    Label="Customer:", ID="Customer", DivClass="noborder", DivClassCol="col-sm-auto",
                        //    IsCustomForm = true,
                        //    DBField="Customer", Type="checkbox", Value="0", DefaultCheckBoxChecked = false
                        //},
                    }
                },
                //new  FormData() {
                //    htmlControl = new HTMLControl[]
                //    {

                //        new HtmlTextBox()
                //        {
                //            Hide= true,
                //            Label = "ItemName:",
                //            ID = "TxtItemName",
                //            Class="input2 CustInfo", IsCustomForm = true,
                //            DBField="ItemName"
                //        },
                //        new HtmlTextBox()
                //        {
                //            Hide= true,
                //            Label = "StockInID:",
                //            ID = "Txt_Stock_In_ID",
                //            Class="input2 CustInfo", IsCustomForm = true,
                //        },
                //        new HtmlTextBox()
                //        {
                //            Hide= true,
                //            Label = "UM:",
                //            ID = "Txt_UM",
                //            Class="input2 CustInfo", IsCustomForm = true,
                //        },
                //        new HtmlTextBox()
                //        {
                //            Hide= true,
                //            Label = "CostPrice:",
                //            ID = "Txt_Cost_Price",
                //            Class="input2 CustInfo", IsCustomForm = true,
                //        },new HtmlTextBox()
                //        {
                //            Label = "UnitPrice:",
                //            ID = "Txt_Unit_Price",
                //            Class="input2 CustInfo", IsCustomForm = true,
                //            DivClassCol="col-sm-auto", Hide = true
                //        },
                //        //new HtmlTextBox()
                //        //{
                //        //    Label = "Barcode:",
                //        //    ID = "TxtBarcode",
                //        //    Class="input2 CustInfo", IsCustomForm = true,
                //        //    DBField="Barcode"
                //        //}
                //    }
                //},
            };

            html.insTable.Add(
                new InsertTable()
                {
                    TableName = "TblStockOut",
                    //GetIndetityFiled = "StockOutID",
                    PrimaryFieldValue = "0",
                    PrimaryField = "StockOutID",
                    Fields = new
                    {
                        SystemID = html.SystemID,
                        WarehouseID = html.WarehouseID,
                        ActivateCustomerID = "$(\"#Cbo_CurrentItemName_ActivateCustomerID\").val()",
                        BillingInvoiceID = "$(\"#Cbo_CurrentItemName_BillingInvoiceID\").val()",
                        CustomerID = "$(\"#Txt_CustomerID\").val()",
                        StockInID = "$(\"#UM_StockInID\").val()",
                        ItemID = "$(\"#ItemID\").val()",
                        UM = "$(\"#UM\").val()",
                        QTY = "1",
                        CostPrice = "$(\"#UM_CostPrice\").val()",
                        UnitPrice = "0",//"$(\"#Txt_Unit_Price\").val()",
                        DiscountP = "0",
                        DiscountUSD = "0",
                        Amount = "0",//"$(\"#Txt_Unit_Price\").val()",
                        StatusID = "80",
                        IsUsed = "1",
                        InvoiceNumber = "''",
                        ConnectionID = "$(\"#CurrentConnectionID\").val()",
                        LocationID = "$(\"#cbo_Warehouse_Name_LocationID\").val()",
                        Type = "'Stock Out'",
                        ClassNameID = "''",
                        IsIncludedVAT = "0",
                        DateOut = "'" + html.GetCurrentDate() + "'",
                    },
                });

            html.insTable.Add(
                new InsertTable()
                {
                    TableName = "TblStockInDetail",
                    Fields = new
                    {
                        SystemID = html.SystemID,
                        WarehouseID = html.WarehouseID,
                        ActivateCustomerID = "$(\"#Cbo_CurrentItemName_ActivateCustomerID\").val()",
                        StockInID = "$(\"#UM_StockInID\").val()",
                        ItemID = "$(\"#ItemID\").val()",
                        ItemName = "$(\"#cboItemName\").val()",
                        UM = "$(\"#UM\").val()",
                        CostPrice = "$(\"#UM_CostPrice\").val()",
                        QTYOUT = "1",
                        Type = "'Stock Out'",
                        Reason = "$(\"#Txt_Reason\").val().toString()",
                        StockInDate = "'" + html.GetCurrentDate() + "'",
                        StockOutDate = "'" + html.GetCurrentDate() + "'",
                        CustomerID = "$(\"#Txt_CustomerID\").val()",
                        StatusID = "80",
                        LocationID = "$(\"#cbo_Warehouse_Name_LocationID\").val()",
                        UnitPrice = "0",
                        ClassNameID = "''",
                    },
                    GetIdentityField = "StockOutID",
                    PrimaryField = "StockInDetailID",
                    PrimaryFieldValue = "0",
                });
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        [Authorize(Roles = "Receive Return Device")]
        public ActionResult ReceivedDevice(int CustomerID, string PrimaryFieldValue = "0")
        {
            HTML html = new HTML
            {
                FormID = "CreateReceivedDevice",
                SaveChangeCallbackFunction = "CallBackSaveChangeReceivedDevice();"
            };
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Current Device Information",
                    TableName="TblCustomerDeviceInfoDetail",
                    PrimaryFieldName="CustomerDeviceInfoDetailID",
                    GetIDENTITYByFiled="CustomerDeviceInfoDetailID",
                    PrimaryFieldValue = PrimaryFieldValue,
                    SQL="update dbo.TblCustomerDeviceInfoDetail set [Status]='Return' where CustomerDeviceInfoDetailID = @Cbo_CurrentItemName_CustomerDeviceInfoDetailID",
                    UpdateFiledName="Cbo_CurrentItemName_CustomerDeviceInfoDetailID",
                    dbField = new DBField[]
                    {
                        new DBField{DBFieldName="ActivateCustomerID", Value="Cbo_CurrentItemName_ActivateCustomerID", GetValueFromInputName = true},
                        //new DBField{DBFieldName="CustomerID", Value="Txt_CustomerID", GetValueFromInputName = true},
                        new DBField{DBFieldName="ItemID", Value="Cbo_CurrentItemName_ItemID", GetValueFromInputName = true},
                        new DBField{DBFieldName="ItemName", Value="Cbo_CurrentItemName_ItemName", GetValueFromInputName = true},
                        new DBField{DBFieldName="UM", Value="Cbo_CurrentItemName_UM", GetValueFromInputName = true},
                        new DBField{DBFieldName="Type", Value="Cbo_CurrentItemName_StockName", GetValueFromInputName = true},
                        new DBField{DBFieldName="QTY", Value="Cbo_CurrentItemName_QTY", GetValueFromInputName = true},
                        new DBField{DBFieldName="Status", Value="Cbo_CurrentItemName_Status", GetValueFromInputName = true},
                        new DBField{DBFieldName="CostPrice", Value="Cbo_CurrentItemName_TxtCostPrice", GetValueFromInputName = true, IsNumber=true },
                        new DBField{DBFieldName="CustomerDeviceInfoID", Value="Cbo_CurrentItemName_CustomerDeviceInfoID", GetValueFromInputName = true},
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Receive Date:", ID="ReceiveDate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Date", SingleDatePicker = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Item Name:", ID="Cbo_CurrentItemName",
                            Required=true,
                            isTypeahead=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            SQL=@"select distinct di.ActivateCustomerID, 
di.CustomerDeviceInfoID, di.CustomerDeviceInfoDetailID, ac.BillingInvoiceID, i.ItemID, i.ItemName +
case when isnull(i.ItemCode, '') <> '' then ' - ' + i.ItemCode else case when isnull(i.SerialNumber,'') <> '' then ' - ' 
+ i.SerialNumber else '' end end ItemName, i.UM, sn.StockName, 1 QTY, 'Received' [Status],di.SerialNo, 
di.MACAddress, ac.ConnectionID, ac.[SID], sd.StockOutID, sd.StockInID, format(isnull(di.CostPrice,0), '#0.00') TxtCostPrice, di.LocationID, di.WarehouseName
from dbo.TblItem i
inner join dbo.TblCustomerDeviceInfoDetail di on di.ItemID = i.ItemID
inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = di.ActivateCustomerID
inner join dbo.TblStockName sn on sn.StockNameID = i.StockNameID
inner join dbo.TblStockInDetail sd on sd.ActivateCustomerID = ac.ActivateCustomerID and sd.ItemID = di.ItemID
where di.[Status] = 'Active' and di.CustomerID="+CustomerID+" and i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " order by i.ItemName + case when isnull(i.ItemCode, '') <> '' then ' - ' + i.ItemCode else case when isnull(i.SerialNumber,'') <> '' then ' - ' + i.SerialNumber else '' end end",
                            FieldName="ItemName",
                            HiddenField="ActivateCustomerID, CustomerDeviceInfoID, CustomerDeviceInfoDetailID, BillingInvoiceID, ItemID, ItemName, UM, StockName, QTY, Status, StockOutID,StockInID,TxtCostPrice",
                            HiddenFieldValue="0,0,0,0,0,0,0,0,1,Return,0,0,0",
                            DisplayOnFields=JsonConvert.SerializeObject(new {
                                ActivateCustomerID="ActivateCustomerID",
                                CustomerDeviceInfoID="CustomerDeviceInfoID",
                                CustomerDeviceInfoDetailID="CustomerDeviceInfoDetailID",
                                BillingInvoiceID="BillingInvoiceID",
                                ItemID="ItemID",
                                ItemName="ItemName",
                                UM="UM",
                                StockName="StockName",
                                QTY="QTY",
                                Status="Status",
                                CurrentSerialNo="SerialNo",
                                CurrentMACAddress="MACAddress",
                                CurrentConnectionID="ConnectionID",
                                CurrentSID="SID",
                                StockOutID="StockOutID",
                                StockInID="StockInID",
                                TxtCostPrice="TxtCostPrice",
                                cbo_Warehouse_Name_LocationID="LocationID",
                                cbo_Warehouse_Name="WarehouseName",
                            })
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Serial No:", ID="CurrentSerialNo",
                            Class="input2 CustInfo", IsCustomForm = true, IsReadOnly = true,
                            DBField="SerialNo"
                        },
                        new HtmlTextBox()
                        {
                            Label="MAC Address:", ID="CurrentMACAddress",
                            Class="input2 CustInfo", IsCustomForm = true, IsReadOnly = true,
                            DBField="MACAddress"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        //new HtmlTextBox()
                        //{
                        //    Label="TxtCostPrice:", ID="TxtCostPrice",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DivClassCol="col-sm", IsReadOnly=true,
                        //},
                        new HtmlTextBox()
                        {
                            Label="ConnectionID:", ID="CurrentConnectionID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm", IsReadOnly=true,
                        },
                        new HtmlTextBox()
                        {
                            Label="SID:", ID="CurrentSID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm", IsReadOnly = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="CustomerID:", ID="Txt_CustomerID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            IsReadOnly = true, Hide=true, Value=CustomerID.ToString(),
                            DBField = "CustomerID"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Warehouse Name:",
                            ID = "cbo_Warehouse_Name",
                            SQL = "select uw.WID LocationID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName from dbo.TblUserWarehouse uw\r\ninner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " order by (b.BranchName + ' - ' + uw.WarehouseName)",
                            DBField = "WarehouseName",
                            HiddenField = "LocationID",
                            isTypeahead = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            Required = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Remark:",
                            ID = "Txt_Reason",
                            Class="input2 CustInfo", IsCustomForm = true,
                            SQL_AutoComplete="select Reason from dbo.TblStockReason\r\nwhere lower(Reason) like @SearchText and SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID +" order by Reason",
                            DBField="Reason",
                            Required = true, ISAutoComplete = true
                        },
                    }
                },
            };
            html.insTable.Add(
                new InsertTable()
                {
                    TableName = "TblStockInDetail",
                    Fields = new
                    {
                        SystemID = html.SystemID,
                        WarehouseID = html.WarehouseID,
                        ActivateCustomerID = "$(\"#Cbo_CurrentItemName_ActivateCustomerID\").val()",
                        StockInID = "$(\"#Cbo_CurrentItemName_StockInID\").val()",
                        StockOutID = "$(\"#Cbo_CurrentItemName_StockOutID\").val()",
                        ItemID = "$(\"#Cbo_CurrentItemName_ItemID\").val()",
                        ItemName = "$(\"#Cbo_CurrentItemName_ItemName\").val()",
                        UM = "$(\"#Cbo_CurrentItemName_UM\").val()",
                        CostPrice = "$(\"#Cbo_CurrentItemName_TxtCostPrice\").val()",
                        QTY = "1",
                        QTYOUT = "0",
                        Type = "'Stock In'",
                        Reason = "$(\"#Txt_Reason\").val().toString()",
                        StockInDate = "StringToDate($(\"#ReceiveDate\").val())",
                        StockOutDate = "StringToDate($(\"#ReceiveDate\").val())",
                        CustomerID = "$(\"#Txt_CustomerID\").val()",
                        StatusID = "83",
                        LocationID = "$(\"#cbo_Warehouse_Name_LocationID\").val()",
                        UnitPrice = "0",
                        ClassNameID = "''",
                    }
                });
            //html.SaveChangeAndClose = true;
            //html.FormData = new HTMLControl[]
            //{
            //    new  FormData() {
            //        FormTitle="Current Device Information",
            //        TableName="TblStockIn",
            //        PrimaryFieldName="StockInID",
            //        GetIDENTITYByFiled="StockInID",
            //        PrimaryFieldValue = PrimaryFieldValue,
            //        SQL="update dbo.TblCustomerEquipment set Barcode = @Barcode, Status='Return', Reason = 'Received Date: ' + @ReceiveDate + '; ' + @Reason where EquipmentID = @EquipmentID; update dbo.TblStockOut set IsReturn = 1 where ConnectionID = @CurrentConnectionID",
            //        UpdateFiledName="Barcode, EquipmentID, ReceiveDate, Reason, CurrentConnectionID",
            //        htmlControl = new HTMLControl[]
            //        {
            //            new HtmlTextBox()
            //            {
            //                Label="Receive Date:", ID="ReceiveDate",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //                DBField="ReceiveDate", SingleDatePicker = true
            //            },
            //            new HtmlComboBox()
            //            {
            //                Label="Item Name:", ID="Cbo_CurrentItemName",
            //                Required=true,
            //                Class="input2 CustInfo", IsCustomForm = true,//case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end
            //                SQL_CBO="select distinct i.ItemName iName, ce.EquipmentID, ce.ItemID,\r\nce.MACAddress, ce.SerialNo, i.Barcode, i.UM, so.CostPrice, ce.ConnectionID, ce.[SID],\r\n'' PackageName, \r\ncase when i.Barcode <> '' then i.Barcode + ' - ' else '' end + ce.ItemName ItemName\r\nfrom dbo.TblCustomerEquipment ce\r\ninner join dbo.TblItem i on i.ItemID = ce.ItemID inner join dbo.TblStockOut so on so.ItemID = ce.ItemID inner join dbo.TblCustomerBillingInvoice bi on bi.ConnectionID = ce.ConnectionID where ce.[Status]='Active' and ce.CustomerID = " + CustomerID + " and ce.SystemID = " + html.SystemID + " and ce.WarehouseID = " + html.WarehouseID + " order by case when i.Barcode <> '' then i.Barcode + ' - ' else '' end + ce.ItemName",
            //                DisplayText="ItemName",
            //                Value="ItemID", DisplayOnFields=JsonConvert.SerializeObject(new {iName="iName",EquipmentID="EquipmentID",ItemID="ItemID",CurrentMACAddress="MACAddress",CurrentSerialNo="SerialNo",Barcode="Barcode",Txt_UM="UM",Txt_Cost_Price="CostPrice",CurrentConnectionID="ConnectionID", CurrentSID="SID",CurrentPackageName="PackageName"})
            //            },
            //        }
            //    },
            //    new  FormData() {
            //        htmlControl = new HTMLControl[]
            //        {
            //            new HtmlTextBox()
            //            {
            //                Label="StatusID:", ID="TxtStatusID",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //                DBField="StatusID", Value="83",
            //                Hide = true,
            //            },
            //            new HtmlTextBox()
            //            {
            //                Label="iName:", ID="iName",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //                Hide = true,
            //            },
            //            new HtmlTextBox()
            //            {
            //                Label="EquipmentID:", ID="EquipmentID",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //                Hide = true,
            //            },
            //            new HtmlTextBox()
            //            {
            //                Label="ItemID:", ID="ItemID",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //                Hide = true,
            //            },
            //            new HtmlTextBox()
            //            {
            //                Label="Barcode:", ID="Barcode",
            //                Class="input2 CustInfo", IsCustomForm = true, Hide = true
            //            },
            //            new HtmlTextBox()
            //            {
            //                Label="UM:", ID="Txt_UM",
            //                Class="input2 CustInfo", IsCustomForm = true, Hide = true
            //            },
            //            new HtmlTextBox()
            //            {
            //                Label="CostPrice:", ID="Txt_Cost_Price",
            //                Class="input2 CustInfo", IsCustomForm = true, Hide = true
            //            },
            //            new HtmlTextBox()
            //            {
            //                Label="MAC Address:", ID="CurrentMACAddress",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //            },
            //            new HtmlTextBox()
            //            {
            //                Label="Serial No:", ID="CurrentSerialNo",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //            },                        
            //            new HtmlTextBox()
            //            {
            //                Label="CustomerID:", ID="Txt_Customer_ID",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //                Hide = true, Value=CustomerID.ToString(),
            //            },new HtmlTextBox()
            //            {
            //                Label="Return:", ID="Return",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //                Hide = true, Value="Return",
            //            }
            //        }
            //    },
            //    new  FormData() {
            //        htmlControl = new HTMLControl[]
            //        {
            //            new HtmlTextBox()
            //            {
            //                Label="ConnectionID:", ID="CurrentConnectionID",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //                DivClassCol="col-sm", IsReadOnly=true,
            //            },
            //            new HtmlTextBox()
            //            {
            //                Label="SID:", ID="CurrentSID",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //                DivClassCol="col-sm", IsReadOnly = true,
            //            },
            //            new HtmlTextBox()
            //            {
            //                Label="Package Name:", ID="CurrentPackageName",
            //                Class="input2 CustInfo", IsCustomForm = true, Hide = true
            //            },
            //        }
            //    },
            //    new  FormData() {
            //        htmlControl = new HTMLControl[]
            //        {
            //            new HtmlComboBox()
            //            {
            //                Label = "Class Name:",
            //                ID = "cbo_ClassName",
            //                SQL_CBO = "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
            //                DisplayText = "ClassName",
            //                Value = "ClassID", Class="input2 CustInfo", IsCustomForm = true,
            //                Required = true, DivClassCol="col-sm-2",
            //                DBField="ClassID"
            //            },
            //            new HtmlComboBox()
            //            {
            //                Label = "Warehouse Name:",
            //                ID = "cbo_Warehouse_Name",
            //                SQL_CBO = "select uw.WID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName from dbo.TblUserWarehouse uw\r\ninner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " order by (b.BranchName + ' - ' + uw.WarehouseName)",
            //                DisplayText = "WarehouseName",
            //                Value = "WID",                            
            //                Class="input2 CustInfo", IsCustomForm = true,
            //                Required = true, DBField="WID"
            //            },
            //            new HtmlComboBox()
            //            {
            //                Label = "Stock Reason:",
            //                ID = "Reason",
            //                SQL_CBO = "select ReasonID, Reason from dbo.TblStockReason where SystemID = "+html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by Reason",
            //                DisplayText = "Reason",
            //                Value = "Reason",
            //                Class="input2 CustInfo", IsCustomForm = true,
            //                AddNewFuntionName="AddNewStockReason()",
            //                Required = true
            //            }
            //        }
            //    }
            //};

            //html.insTable.Add(
            //    new InsertTable()
            //    {
            //        GetIndetityFiled = "StockInID",
            //        PrimaryField = "StockInID",
            //        PrimaryFieldValue = PrimaryFieldValue.ToString(),
            //        TableName = "TblStockInDetail",
            //        Fields = new
            //        {
            //            SystemID = html.SystemID,
            //            WarehouseID = html.WarehouseID,
            //            ItemID = "$(\"#ItemID\").val()",
            //            ItemName = "$(\"#iName\").val()",
            //            //StockInID = "0",
            //            UM = "$(\"#Txt_UM\").val()",
            //            CostPrice = "$(\"#Txt_Cost_Price\").val()",
            //            QTY = "1",
            //            Type = "'Stock In'",
            //            Reason = "$(\"#Reason\").val().toString()",
            //            StockInDate = "'" + html.GetCurrentDate() + "'",
            //            //StockOutDate = "'" + html.GetCurrentDate() + "'",
            //            CustomerID = "$(\"#Txt_Customer_ID\").val()",
            //            StatusID = "83", //Return Stock Status
            //            LocationID = "$(\"#cbo_Warehouse_Name\").val().toString()",
            //            UnitPrice = "0",//"$(\"#Txt_Unit_Price\").val()",
            //            ClassNameID = "$(\"#cbo_ClassName\").val().toString()",
            //        },
            //    });
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        public ActionResult Products(int CustomerID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false,
                ShowGridOnly = true
            };
            html.GH.GridPara = new GridPara[]
            {
                new GridPara() { ParaName="FromDate", Value="" },
                new GridPara() { ParaName="ToDate", Value="" },
                new GridPara() { ParaName="CustomerName", Value="" },
                new GridPara() { ParaName="SystemID", Value=html.SystemID },
                new GridPara() { ParaName="WarehouseID", Value=html.WarehouseID },
                new GridPara() { ParaName="WarehouseName", Value="" },
                new GridPara() { ParaName="ItemName", Value="" },
                new GridPara() { ParaName="CustomerID", Value=CustomerID.ToString() },
            };
            html.GH.GridProcedureName = "SP_SHOW_STOCK_OUT";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "WarehouseName", FieldName = "WarehouseName", IsGroupByField = true, Colspan = 7, DisplayOneRow= true },
                new GridField() { Header = "Date", FieldName = "DateOut", Format="dd-MM-yyyy"  },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNumber" },
                new GridField() { Header = "Item Name", FieldName = "ItemName" },
                new GridField() { Header = "QTY", FieldName = "QTY", TextAlign="right", Format="#,#0" },
                new GridField() { Header = "Amount USD", FieldName = "SalePrice", TextAlign="right", Format="#,#0.00" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public ActionResult DeviceInfo(int CustomerID)
        {
            HTML html = new HTML();
            bool ChangeDevice = HttpContext.GetOwinContext().Request.User.IsInRole("Change Device");
            ViewBag.ChangeDevice = ChangeDevice;
            bool ReceiveReturnDevice = HttpContext.GetOwinContext().Request.User.IsInRole("Receive Return Device");
            ViewBag.ReceiveReturnDevice = ReceiveReturnDevice;
            string sql = @"select ac.ConnectionID, ac.[SID], ac.ConnectionType, case when isnull(convert(varchar, ac.CreateDate), '') <> '' then ac.CreateDate else ac.ActivateDate end CreateDate, 
isnull(i.ItemName, '') ItemName, isnull(cd.MACAddress, '') MACAddress, 
isnull(cd.SerialNo, '') SerialNo, isnull(cd.[Status], '') [Status], cd.Reason, cd.Remark, ce.CustomerNotes,
r.RouterName, ac.ConnectionType, ac.VLanName, ac.VLanIPAddress,
ac.IPAddress, ac.GetwayIP, ac.SubnetIP, ac.DNS1, ac.DNS2, ac.PPPoEProfileName,
ac.PPPoEUserName, ac.PPPoEPassword, ac.DeviceName, ac.WifiIP, ac.WifiName,
ac.WifiKey, ac.WifiLoginName, ac.WifiLoginPassword, ce.CreateBy,
dv.DeviceName DeviceNameOLT, ce.PONPort, ce.OLTID, dsv.SplitterName, ce.[Port], ce.[Length], ce.LossDB,
ce.GoogleMap, ac.ActivateDate, ac.ActivateBy, ce.ModifyBy, ce.ModifyDate, ce.InstallBy
from dbo.TblCustomerDeviceInfo ce
left join dbo.TblCustomerDeviceInfoDetail cd on cd.CustomerDeviceInfoID = ce.CustomerDeviceInfoID
left join dbo.TblItem i on i.ItemID = cd.ItemID
inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ce.ActivateCustomerID
inner join dbo.TblRouter r on r.RouterID = ac.RouterID
inner join dbo.TblDeviceName dv on dv.DeviceNameID = ce.DeviceNameID
inner join dbo.TblDeviceSplitterName dsv on dsv.SplitterNameID = ce.SplitterNameID
where ac.ConnectionID in (select distinct bi.ConnectionID from dbo.TblCustomerBillingInvoice bi where isnull(bi.StatusID,0) <> 12 
and bi.CustomerID=@CustomerID)
and ac.CustomerID = @CustomerID order by ac.ConnectionID desc;";
            Dictionary<string, string> para = new Dictionary<string, string> {
                {"CustomerID",CustomerID.ToString() }
            };
            ViewBag.Data = JsonConvert.DeserializeObject(html.JsonData(sql, false, para, true)).ToString().Replace("`", string.Empty);
            return View();
        }

        public ActionResult CustomerService(int CustomerID)
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false,
                ShowGridOnly = true
            };
            html.GH.GridProcedureName = "SP_SEARCH_CUSTOMER_CONNECTION";
            html.GH.GridPara = new GridPara[]
            {
                new GridPara() { ParaName="FromDate", Value="" },
                new GridPara() { ParaName="ToDate", Value="" },
                new GridPara() { ParaName="ConnectivityStatus", Value="" },
                new GridPara() { ParaName="PackageName", Value="" },
                new GridPara() { ParaName="CustomerCode", Value="" },
                new GridPara() { ParaName="CustomerName", Value="" },
                new GridPara() { ParaName="CompanyContactMobileNumber", Value="" },
                new GridPara() { ParaName="SID", Value="" },
                new GridPara() { ParaName="InstallAddress", Value="" },
                new GridPara() { ParaName="ConnectionID", Value="" },
                new GridPara() { ParaName="SystemID", Value=html.SystemID },
                new GridPara() { ParaName="WarehouseID", Value=html.WarehouseID },
                new GridPara() { ParaName="CustomerID", Value=CustomerID.ToString() },
            };
            html.isShowLoadingGrid = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                //new GridField() { Header = "CID", FieldName = "CustomerCode", IsGroupByField = true, GroupLabel="CID: ", Colspan=9 },
                //new GridField() { Header = "Customer Name", FieldName = "CustomerName", IsGroupByField = true, GroupLabel="Name: ", Colspan=9 },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", IsGroupByField = true, GroupLabel="Register Date: ", Colspan=10 },
                new GridField() { Header = "Register By", FieldName = "RegisterBy", IsGroupByField = true, GroupLabel="Register By: ", Colspan=9 },
                new GridField() { Header = "Count Connection", FieldName = "CountConnection", IsGroupByField = true, GroupLabel="Connection: ", Colspan=10 },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Service ID", FieldName = "SID" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo" },
                new GridField() { Header = "Status", FieldName = "Status", ClassName="text-red", TextAlign="center" },
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                new GridField() { Header = "Start Date", FieldName = "StartDate" },
                new GridField() { Header = "End Date", FieldName = "EndDate", ClassName="text-red" },
                new GridField() { Header = "Remain Day", FieldName = "RemainDay", ClassName="text-red" },
                new GridField() { Header = "InstallAddress", FieldName = "InstallAddress" }
            };
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 2;
            //html.GridEventClick = "ShowCustomerInfo(this)";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public JsonResult AlertCustConInfo(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select BillingInvoiceID,
            ConnectionID, [SID], PaymentDueDate,
            'Connection ID: ' + ConnectionID + ' will be down speed to 3M on date: '+ format(dateadd(d,-3, PaymentDueDate), 'dd-MM-yyyy') + ' due to not yet payment.' Comment
            from dbo.TblCustomerBillingInvoice
            where isnull([SID], '') <> '' and (DATEDIFF(D, getdate(), PaymentDueDate)-3) = 1 and CustomerID = " + CustomerID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            return Json(new { Data = html.JsonData(sql) }, JsonRequestBehavior.AllowGet); ;
        }

        [Authorize(Roles = "Add Customer, Edit Customer, Delete Customer")]

        public ActionResult CustomerInfo()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Customers",
                isShowLoadingGrid = false
            };
            if (html.HasPermission("Add Customer"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Customer"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Customer"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.DialogBoxHeight = 600;
            html.AddButton = "Register New Customer";
            html.AddURL = "AddCustomer/CreateCustomer";
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                //DateTimePickerWithRanges = true,
                DateRangePicker = true,
                LabelFromDate = "Register Date:",
                DBField = "c.RegisterDate",
                DefaultValueDate = false

            });
            //sql = "select distinct s.StatusID, s.[Status] from dbo.TblCustomer c ";
            //sql += "inner join dbo.TblStatus s on s.StatusID = c.StatusID ";
            //sql += "where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by s.StatusID";

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Customer Group:",
                DBField = "c.CustomerGroupID",
                SQL_CBO = @"select distinct g.CustomerGroupID, g.CustomerGroup from dbo.TblCustomerGroup g
inner join dbo.TblCustomer c on c.CustomerGroupID = g.CustomerGroupID where g.SystemID = " + html.SystemID + " and g.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%' order by g.CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroupID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerType",
                Label = "Block:",
                DBField = "c.CustomerTypeID",
                SQL_CBO = @"select distinct ct.CustomerTypeID, ct.CustomerType from dbo.TblCustomerType ct
inner join dbo.TblCustomer c on c.CustomerTypeID = ct.CustomerTypeID
where ct.SystemID = " + html.SystemID + " and ct.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%' " + @"
order by ct.CustomerType",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerName",
                Label = "Customer Name:",
                DBField = "c.CustomerName",
                SQLAutoComplete = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName",
                ISAutoComplete = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            //radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            //{
            //    OPTLabel = "Customer Name",
            //    SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerName",
            //    RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
            //    ISAutoComplete = true
            //};
            //htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Phone Number",
                SQL = "select distinct top 10 CompanyContactMobileNumber from dbo.TblCustomerCompany where lower(CompanyContactMobileNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CompanyContactMobileNumber",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Address",
                SQL = "select distinct top 10 CustomerAddress from dbo.TblCustomer where lower(CustomerAddress) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerAddress",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Commune",
                SQL = "select distinct top 10 CustomerCommune from dbo.TblCustomer where lower(CustomerCommune) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCommune",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "District",
                SQL = "select distinct top 10 CustomerDistrict from dbo.TblCustomer where lower(CustomerDistrict) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '" + html.DomainID + "' order by CustomerDistrict",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Province/City",
                SQL = "select distinct top 10 CustomerProvinceCity from dbo.TblCustomer where lower(CustomerProvinceCity) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerProvinceCity",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerContactMobileNumber") + "," + EncryAndDecry.Encryp("c.CustomerAddress") + "," + EncryAndDecry.Encryp("c.CustomerCommune") + "," + EncryAndDecry.Encryp("c.CustomerDistrict") + "," + EncryAndDecry.Encryp("c.CustomerProvinceCity"),
                ISAutoComplete = true
            });
            html.GridEventClick = "onGridClick_CustInfo(this);";
            //html.GridHeight = "400px";
            //html.DefaultSearch = false;
            html.ReportTitle = "List Customers Information";
            html.TableName = "TblCustomer";

            //string sqlLocation = "select distinct CustomerProvinceCity from dbo.TblCustomer where lower(CustomerProvinceCity) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by CustomerProvinceCity";
            //HtmlAddControls htmlAddControl = new HtmlAddControls()
            //{
            //    ID = "txtLocation",
            //    Label = "Location:",
            //    DBField = "Location",
            //    SQLAutoComplete = sqlLocation,
            //    ISAutoComplete = true
            //};
            //htmlAddControls.Add(htmlAddControl);
            string sql = @"select ROW_NUMBER() over(order by cg.CustomerGroup) as [No], c.CustomerID, 
            c.CustomerCode, c.CustomerName, c.CustomerNationality, c.CustomerContactMobileNumber PhoneNumber,
            c.RegisterDate, c.RegisterBy, c.CustomerAddress, c.CustomerCommune, c.CustomerDistrict, 
            c.CustomerProvinceCity, cg.CustomerGroup + ' (<span class=""text-danger"">' +
convert(varchar, (select distinct count(cc.CustomerGroupID) from dbo.TblCustomer cc where cc.CustomerGroupID = cg.CustomerGroupID and cc.SystemID = " + html.SystemID + " and cc.WarehouseID = " + html.WarehouseID + " and cc.DomainID like '%" + html.DomainID + @"%'))+'</span>)' CustomerGroup,
ct.CustomerType + ' (<span class=""text-danger"">' +
convert(varchar, (select distinct count(cc.CustomerTypeID) from dbo.TblCustomer cc where cc.CustomerTypeID = ct.CustomerTypeID and cc.SystemID = " + html.SystemID + " and cc.WarehouseID = " + html.WarehouseID + " and cc.DomainID like '%" + html.DomainID + @"%'))+'</span>)' CustomerType, c.ATTName
from dbo.TblCustomer c
            inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblCustomerType ct on c.CustomerTypeID = ct.CustomerTypeID
            where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%' ";
            html.GH.SQL = sql;
            html.OrderBy = "cg.CustomerGroup, ct.CustomerType, c.CustomerCode";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerID", FieldName = "CustomerID", IsHiddenField= true },
                new GridField() { Header = "CustomerGroup", FieldName = "CustomerGroup", GroupLabel="Group Name: ", IsGroupByField= true, Colspan=11, DisplayOneRow = true },
                new GridField() { Header = "Customer Type", FieldName = "CustomerType", GroupLabel="Customer Type: ", IsGroupByField= true, Colspan=11 },
                new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Phone Number", FieldName = "PhoneNumber" },
                new GridField() { Header = "ATT Name", FieldName = "ATTName" },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", ClassName="text-danger", Format="dd/MM/yyyy" },
                new GridField() { Header = "Register By", FieldName = "RegisterBy" },
                new GridField() { Header = "Address", FieldName = "CustomerAddress" },
                new GridField() { Header = "Commune", FieldName = "CustomerCommune", },
                new GridField() { Header = "District", FieldName = "CustomerDistrict" },
                new GridField() { Header = "Province/City", FieldName = "CustomerProvinceCity" }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        public ActionResult DepositList(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over(order by ConnectionID) as [No], DepositFeeID,
ConnectionID, '<a href=""javascript:void(0);"" onclick=""PrintInvoice('+ convert(varchar, CustomerID) +', '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +',10,'''',false,this);"">'+ InvoiceNo+'</a>' InvoiceNo, DepositFee, CreateDate [Date], Memo, '<a href=""javascript:void(0);"" onclick=""ReturnDeposit('+convert(varchar, CustomerID)+');"">Return Deposit</a>' Action
from dbo.TblCustomerDeposit
where CustomerID = " + CustomerID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.ReportTitle = "Deposit Fee List";
            html.ShowGridOnly = true;
            html.OrderBy = "ConnectionID";
            html.TblID = "TableDepositFee";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Date", Format="dd-MM-yyyy"  },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Deposit Fee", FieldName = "DepositFee",
                    TextAlign="right", Format="#,#0.00",
                IsTotalFooter=true, Colspan = 4, TotalLabel="Total:"},
                new GridField() { Header = "Action", FieldName = "Action", HideOnPrint = true },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadDepositFee");
            return View();
        }

        public ActionResult ReturnDeposit(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select c.CustomerID, c.CustomerCode + ' - ' + c.CustomerName CustomerName
from dbo.TblCustomer c where c.CustomerID = @CustomerID and c.SystemID = @SystemID and c.WarehouseID = @WarehouseID";
            Dictionary<string, string> para = new Dictionary<string, string>()
            {
                { "CustomerID",CustomerID.ToString() },
                { "SystemID",html.SystemID },
                { "WarehouseID",html.WarehouseID },
            };
            DataSet ds = html.LoadData(sql, false, para);
            string CustomerName = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                CustomerName = ds.Tables[0].Rows[0]["CustomerName"].ToString();
            }
            html.FormID = "FrmCustomerReturnDeposit";
            html.SaveChangeFunction = "SaveChangeVendorPayment()";
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Return Deposit",
                    //ISAddAttachmentFile = true,
                    //DestinationFile = "Uploads/Vendors/Payment",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Pay To:",
                            ID = "Payee",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            Required = true,
                            ISAutoComplete = true,
                            SQL_AutoComplete="select distinct top 10 c.CustomerID, c.CustomerCode + ' - ' + c.CustomerName CustomerName from dbo.TblCustomerInvoice bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where isnull(bi.DepositFee,0) <> 0 and bi.SystemID = "+ html.SystemID + " and bi.WarehouseID = " + html.WarehouseID,
                            HiddenField="CustomerID",
                            Value =CustomerName,
                            HiddenFieldValue = CustomerID.ToString(),
                            //ISCallBack = true,
                            //SQLCallBack="select v.VendorName ReceivedFromName from dbo.TblBills b inner join dbo.TblVendor v on v.VendorID = b.VendorID where lower(v.VendorCode + ' - ' + v.VendorName) = @SearchText and b.SystemID =" + html.SystemID + " and b.WarehouseID = " + html.WarehouseID,
                            //DisplayOnFields="ReceivedFromName"
                        },
                        new HtmlTextBox()
                        {
                            Label = "Reference#:",
                            ID = "Reference",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Return Date:",
                            ID = "ReturnDate",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            SingleDatePicker = true,
                            Required = true
                        },
                    }
                },
                new  FormData() {

                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label="Payment Account:", ID="ChartOfAccountID",
                            Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            AddNewFuntionName="AddNewChartOfAccount('ChartOfAccountID')",
                            SQL="select ChartOfAccountID, AccountType, (AccountNo + ' - ' + AccountName) AccountName from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by AccountType",
                            DisplayText="AccountName",
                            Value="ChartOfAccountID",
                            GroupByField="AccountType",
                        },
                        new HtmlComboBox()
                        {
                            Label = "Payment Method:",
                            ID = "PaymentModeID",
                            Required = true,
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            SQL_CBO = "select PaymentMethodID, PaymentMethod from dbo.TblPaymentMethod where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by case when PaymentMethod = 'Cash' then 0 else 1 end, PaymentMethod",
                            DisplayText = "PaymentMethod",
                            Value = "PaymentMethodID", AddNewFuntionName="AddPaymentMethod('PaymentModeID')"
                        },
                        new HtmlComboBox()
                        {
                            Label = "Category:",
                            ID = "PaymentCategoryID",
                            Required = true,
                            IsCustomForm = true,
                            SQL_CBO = "select PaymentCategoryID, Category from dbo.TblPaymentCategory " +
                            "where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by Category",
                            DisplayText = "Category", Value = "PaymentCategoryID",
                            AddNewFuntionName="AddPaymentCategory('PaymentCategoryID')"
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "From Bank Name:",
                            ID = "FromBankName",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Account Number:",
                            ID = "FromAccountNumber",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Account Name:",
                            ID = "FromAccountName",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        }

                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Cheque Number:",
                            ID = "ChequeNumber",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "From Phone Number:",
                            ID = "FromPhoneNumber",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "To Phone Number:",
                            ID = "ToPhoneNumber",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Description:",
                            ID = "Description",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Received From Name:",
                            ID = "ReceivedFromName",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            //Value = ReceivedFromName,
                            Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label = "Paid By:",
                            ID = "PaidBy",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            Value = html.UserName,
                            Required = true
                        }
                    }
                },
                new  FormData() {
                    FormTitle="Transaction Details",
                    htmlControl = new HTMLControl[]
                    {
                       new HtmlLabel()
                       {
                           Label="<div id=\"divTblTransactionDetails\" class=\"col-12 table-responsive\"></div>"
                       }
                    }
                }
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        public JsonResult LoadDepositList(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over(order by ConnectionID) as [No], DepositFeeID,
ConnectionID, '<a href=""javascript:void(0);"" onclick=""PrintInvoice('+ convert(varchar, CustomerID) +', '''+ InvoiceNo +''','+ convert(varchar, IsIncludedVAT) +',10,'''',false,this);"">'+ InvoiceNo+'</a>' InvoiceNo, DepositFee, CreateDate [Date], Memo, '<a href=""javascript:void(0);"" onclick=""ReturnDeposit('+convert(varchar, CustomerID)+');"">Return Deposit</a>' Action
from dbo.TblCustomerDeposit
where CustomerID = " + CustomerID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            html.GH.SQL = sql;
            html.ShowPrintOnTopTable = false;
            //html.ReportTitle = "Deposit Fee List";
            html.ShowGridOnly = true;
            html.OrderBy = "ConnectionID";
            html.TblID = "TableLoadDepositFee";
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Date", Format="dd-MM-yyyy"  },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Deposit Fee", FieldName = "DepositFee",
                    TextAlign="right", Format="#,#0.00",
                IsTotalFooter=true, Colspan = 4, TotalLabel="Total:"},
                new GridField() { Header = "Action", FieldName = "ConnectionID", DisplayCheckBox = true },
            };
            string DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "Load_Deposit_Fee");
            return Json(new { Data = DrawHTMLSearchForm }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Delay Payment, Edit Delay Payment, Delete Delay Payment")]
        public ActionResult DeplayPayment(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "FrmDeplayPayment",
                FieldIDFocus = "TxtCustInvoice_No",
                TableName = "TblDelayPayment",
                PrimaryField = "DelayPaymentID",
                BtnSaveChangeDialog = "SaveChange2",
                BtnCloseDialog = "CloseModal2",
                SaveChangeAndClose = true,
                SaveChangeCallbackFunction = "LoadCustomerServices(" + PrimaryFieldValue + ");"
            };
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Invoice No:",
                ID = "TxtCustInvoice_No",
                DBField = "InvoiceNo",
                Required = true,
                isTypeahead = true,
                SQL_CBO = "select distinct InvoiceNo\r\nfrom dbo.TblCustomerInvoice where StatusID = 11 and InvoiceType = 'Renew Invoice' and CustomerID = " + PrimaryFieldValue,
                Value = "InvoiceNo",
                DisplayText = "InvoiceNo"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Payment Due Date:",
                ID = "TxtCustPaymentDueDate",
                DBField = "DelayPaymentDueDate",
                Required = true,
                SingleDatePicker = true,
                ISDateTime = true,
                LimitDateRangePickerByDay = 2
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "UserFullName",
                ID = "TxtUserFullName",
                DBField = "UserFullName",
                Required = true,
                Hide = true,
                Value = html.UserFullName
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "UserLoginID:",
                ID = "TxtUserLoginID",
                DBField = "UserLoginID",
                Required = true,
                Hide = true,
                Value = html.UserID
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtCustPaymentMemo",
                DBField = "Memo",
                Required = true,
            });

            html.insTable.Add(new InsertTable()
            {
                TableName = "TblCustomerInvoice",
                Fields = new
                {
                    IsApplyDelayPayment = "'1'",
                    PaymentDueDate = "StringToDate($(\"#TxtCustPaymentDueDate\").val())"
                },
                UpdateDateConditionFields = new
                {
                    CustomerID = "'" + PrimaryFieldValue.ToString() + "'",
                    InvoiceNo = "$(\"#TxtCustInvoice_No\").val()",
                }
            });
            html.insTable.Add(new InsertTable()
            {
                TableName = "TblCustomerBillingInvoice",
                Fields = new
                {
                    PaymentDueDate = "StringToDate($(\"#TxtCustPaymentDueDate\").val())"
                },
                UpdateDateConditionFields = new
                {
                    CustomerID = "'" + PrimaryFieldValue.ToString() + "'",
                    InvoiceNo = "$(\"#TxtCustInvoice_No\").val()",
                }
            });
            html.insTable.Add(new InsertTable()
            {
                TableName = "TblCustomerBalance",
                Fields = new
                {
                    PaymentDueDate = "StringToDate($(\"#TxtCustPaymentDueDate\").val())"
                },
                UpdateDateConditionFields = new
                {
                    CustomerID = "'" + PrimaryFieldValue.ToString() + "'",
                    InvoiceNo = "$(\"#TxtCustInvoice_No\").val()",
                }
            });
            html.insTable.Add(new InsertTable()
            {
                TableName = "TblActivateCustomer",
                Fields = new
                {
                    PaymentStatus = "11",
                    PaymentDueDate = "StringToDate($(\"#TxtCustPaymentDueDate\").val())"
                },
                UpdateDateConditionFields = new
                {
                    CustomerID = "'" + PrimaryFieldValue.ToString() + "'",
                    InvoiceNo = "$(\"#TxtCustInvoice_No\").val()",
                }
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        public JsonResult ShowUserProfile(int CustomerID)
        {
            HTML html = new HTML();
            string sql = "select distinct ta.ConnectionID, ta.RouterID, ta.PPPoEUserName, ta.ConnectionType, r.MikroTikVersionID Version from dbo.TblActivateCustomer ta\r\ninner join dbo.TblRouter r on r.RouterID = ta.RouterID where ta.CustomerID =" + CustomerID + " and ta.SystemID = " + html.SystemID + " and ta.WarehouseID = " + html.WarehouseID;
            string CboData = html.LoadData(sql).Tables[0].Rows.ToJsonString();
            return Json(new { CboData = CboData }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetUserProfile(string PPPoEUserName, int RouterID)
        {
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            Mikrotik MK = new Mikrotik();
            MK.SetConnection(RouterID);
            sqlResult = MK.GetUserProfileName(PPPoEUserName);
            string json = JsonConvert.SerializeObject(sqlResult.Parameters);
            return Json(new { Result = sqlResult.Result, Messge = sqlResult.Message, Data = json }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RemoveUserPPPoEProfile(int RouterID, string ProfileID, string ProfileName, string PPPoEUserName)
        {
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            Mikrotik MK = new Mikrotik();
            MK.SetConnection(RouterID);
            sqlResult = MK.RemoveUserProfile(ProfileID, ProfileName, PPPoEUserName);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProfile(int RouterID, string PPPoEUserName)
        {
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            Mikrotik MK = new Mikrotik();
            MK.SetConnection(RouterID);
            sqlResult = MK.GetProfileName("", 0, "", "", false, RouterID);
            ViewBag.PPPoEUserName = PPPoEUserName;
            ViewBag.Cbo = sqlResult.Value;
            return View();
        }
        public JsonResult AppendProfileToUser(int RouterID, string PPPoEUserName, string PPPoEProfileName, string Comment = "")
        {
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            Mikrotik MK = new Mikrotik();
            MK.SetConnection(RouterID);
            sqlResult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public async Task<JsonResult> GetUserONUInfo(string SearchInfo, int key = 0, bool getContent = true, bool Reboot = false, bool Deactivate = false)
        {
            //         SearchInfo = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(SearchInfo)).Trim();
            //StringBuilder sb = new StringBuilder();
            //dynamic parameters = new ExpandoObject();
            //         string Data = "";
            //         string ExecutablePath = "Chrome\\Win64-115.0.5790.102\\chrome-win64\\chrome.exe";
            //var Path = HttpContext.Server.MapPath(@"/Chromium/");
            //SqlResult sqlResult = new SqlResult();

            //         var browserFetcherOptions = new BrowserFetcherOptions
            //         {
            //             Path = Path,
            //             Browser = SupportedBrowser.Chrome,
            //             Platform = Platform.Win64
            //         };
            //         var browserFetcher = new BrowserFetcher(browserFetcherOptions);
            //         await browserFetcher.DownloadAsync();
            //         var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            //         {
            //             Headless = true,
            //             ExecutablePath = Path + ExecutablePath,
            //             Timeout = 0,
            //             IgnoreHTTPSErrors = true,
            //             //Devtools = true,
            //             //Args = new[]
            //             //{
            //             //    "--disable-web-security",
            //             //    "--disable-features=IsolateOrigins,site-per-process",
            //             //},
            //         });
            //         var page = await browser.NewPageAsync();
            //         try
            //         {
            //             string url = "https://192.168.59.159";
            //             string userName = "NoC";
            //             string password = "Noc@OLT";
            //             await page.GoToAsync(url);
            //             await page.TypeAsync("#user", userName);
            //             await page.TypeAsync("#pass", password);
            //             await page.ClickAsync("#loginBtn");

            //             //await page.ClickAsync("#menu/a/a/a");
            //             Thread.Sleep(1000);
            //             var element = await page.WaitForXPathAsync("//div[@id=\"menu\"]/a[3]");
            //             await element.ClickAsync();
            //             Thread.Sleep(1000);
            //             url = url + "/action/onuauthinfo.html";
            //             await page.GoToAsync(url);
            //             Thread.Sleep(1000);
            //             await page.TypeAsync("#searchONU", SearchInfo);
            //             await page.Keyboard.PressAsync("Enter");
            //             Thread.Sleep(1000);
            //             //var pageHeaderHandle = await page.WaitForXPathAsync("//form/table[3]");

            //             string text = await page.GetContentAsync();
            //	var doc = new HtmlDocument
            //	{
            //		OptionDefaultStreamEncoding = Encoding.UTF8
            //	};
            //	doc.LoadHtml(text.ToString());
            //             if (getNode(text.ToString(), "//form/table[3]/tbody/tr[2]/td[2]") != "")
            //             {
            //                 if (getContent)
            //                 {
            //                     HtmlNode node = doc.DocumentNode.SelectNodes("//form/table[3]/tbody/tr[2]/td[2]")[0];
            //                     parameters.Status = node.InnerText;
            //                     string Status = node.InnerText;
            //                     node = doc.DocumentNode.SelectNodes("//form/table[3]/tbody/tr[2]/td[1]")[0];
            //                     parameters.ONUID = node.InnerText;
            //                     string ONUID = node.InnerText;
            //			node = doc.DocumentNode.SelectNodes("//form/table[3]/tbody/tr[2]/td[8]/a[2]")[0];
            //			string ConnectivityStatus = node.InnerText;
            //			sb.AppendLine("<div class=\"row\">");
            //                     sb.Append("<div class=\"col-auto\"><span class=\"badge badge-info\">" + ONUID + "</span></div>");
            //                     if (Status == "Offline")
            //                         sb.Append("<div class=\"col-auto\"><span name=\"ONUStatus\" class=\"badge badge-danger\">" + Status + "</span></div>");
            //                     else
            //                         sb.Append("<div class=\"col-auto\"><span name=\"ONUStatus\" class=\"badge badge-primary\">" + Status + "</span></div>");
            //                     sb.Append("<div class=\"col-auto\"><button onclick=\"DeactivateONUDevice('" + SearchInfo + "','"+ key + "');\" type=\"button\" class=\"btn btn-block btn-outline-danger btn-xs\" name=\"ONUConStatus\">" + ConnectivityStatus + "</button></div>");
            //                     sb.Append("<div class=\"col-auto\"><button onclick=\"RebootONUDevice('" + SearchInfo + "','"+ key + "');\" type=\"button\" class=\"btn btn-block btn-outline-success btn-xs\">Reboot</button></div>");
            //                     sb.AppendLine("</div>");
            //                 }
            //                 else if (Reboot)
            //                 {
            //			element = await page.WaitForXPathAsync("//form/table[3]/tbody/tr[2]/td[8]/a[7]");
            //			Thread.Sleep(1000);
            //			await element.ClickAsync();
            //			Thread.Sleep(5000);
            //			sqlResult.Message = "Device "+ SearchInfo + " is rebooting.";
            //                     sqlResult.Result = ResultType.Successful;
            //		}
            //		else if (Deactivate)
            //		{
            //			element = await page.WaitForXPathAsync("//form/table[3]/tbody/tr[2]/td[8]/a[2]");
            //			Thread.Sleep(1000);
            //			await element.ClickAsync();
            //			Thread.Sleep(3000);
            //			sqlResult.Message = "Device " + SearchInfo + " is deactivated.";
            //			sqlResult.Result = ResultType.Successful;
            //		}
            //		//var pageHeaderHandle = await page.QuerySelectorAsync("h1");
            //		//var innerTextHandle = await pageHeaderHandle.GetPropertyAsync("outerText");
            //		//var innerText = innerTextHandle.RemoteObject.Value.ToString();
            //	}
            //             await page.CloseAsync();
            //             await browser.CloseAsync();
            //         }
            //         catch (Exception ex)
            //         {
            //             sqlResult.Result = ResultType.Failed;
            //             sqlResult.Message = ex.Message;
            //         }
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            sqlResult = await html.GetUserONUInfo(SearchInfo, key, getContent, Reboot, Deactivate);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = sqlResult.Value }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult GetPPPoEUserStatus(int RouterID, string PPPoEUserName)
        {
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            Mikrotik MK = new Mikrotik();
            sqlResult = MK.GetPPPoEUserStatus(RouterID, PPPoEUserName);
            return Json(new { Data = sqlResult.DataRowRouter.ToJsonString() }, JsonRequestBehavior.AllowGet);
        }

        public string getNode(string text, string node)
        {
            try
            {
                var doc = new HtmlDocument
                {
                    OptionDefaultStreamEncoding = Encoding.UTF8
                };
                doc.LoadHtml(text.ToString());
                HtmlNode nodes = doc.DocumentNode.SelectNodes(node)[0];
                return nodes.OuterHtml;
            }
            catch (Exception)
            {
                return "";
            }
        }

        [Authorize(Roles = "Add Customer Telegram, Edit Customer Telegram, Delete Customer Telegram")]
        public ActionResult TelegramNotification(string PrimaryFieldValue, string CustomerName, string CustomerCode)
        {
            ViewBag.CustomerName = CustomerName;
            ViewBag.CustomerCode = CustomerCode;
            ViewBag.CustomerID = PrimaryFieldValue;
            HTML html = new HTML();
            if (html.HasPermission("Add Customer Telegram"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Customer Telegram"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Customer Telegram"))
                html.AllowDelete = true;
            html.AddButton = "Add Cust Telegram";
            html.AddURL = "CustomerProfile/CreateCustTelegram";
            html.TableName = "TblCustomerTelegram";
            html.PrimaryField = "TelegramCustomerID";
            html.OrderBy = "TelegramCustomerID desc";
            html.IsHaveWhereCondition = true;
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Phone",
                SQL = "select TelegramCustomerID, CustomerPhone from dbo.TblCustomerTelegram where CustomerID =" + PrimaryFieldValue + " and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerPhone",
                SelectValue = "TelegramCustomerID",
                DisplayField = "CustomerPhone",
                RadioValue = EncryAndDecry.Encryp("TelegramCustomerID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = "select ROW_NUMBER() over(order by TelegramCustomerID desc) as [No], * from TblCustomerTelegram where CustomerID = " + PrimaryFieldValue;
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "FirstName", FieldName = "FirstName" },
                new GridField() { Header = "LastName", FieldName = "LastName" },
                new GridField() { Header = "Phone Number", FieldName = "CustomerPhone" },
                new GridField() { Header = "Is Telegram", FieldName = "IsUsingTelegram" },
                new GridField() { Header = "Created Date", FieldName = "CreateDate", Format="dd-MM-yyyy" },
                new GridField() { Header = "Created By", FieldName = "CreateBy" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Action", FieldName = "TelegramCustomerID", TextAlign = "Center",  IsModifyField = true }
            };
            html.OnDialogCloseCallbackFunction = "setTimeout(function() { $(\"#btnCustNotification\").click();}, 100);";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();

            sql = "select distinct TelegramCustomerID, CustomerPhone, isnull(TelegramNotificationID, '') TelegramNotificationID from dbo.TblCustomerTelegram\r\nwhere IsUsingTelegram = 'Yes' and CustomerID = " + PrimaryFieldValue;
            ViewBag.CustPhone = html.JsonConvertDataTable(html.LoadData(sql).Tables[0]);

            sql = "select * from dbo.TblTelegramNotificationSchedule where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by Description";
            ViewBag.NotificationList = html.JsonConvertDataTable(html.LoadData(sql).Tables[0]);

            sql = @"select distinct InvoiceNo from dbo.TblCustomerInvoice where StatusID not in (12,13,14,20) and [Type] = 'Official Invoice' and CustomerID = " + PrimaryFieldValue + @"
order by InvoiceNo desc";
            ViewBag.CustInvoice = html.JsonConvertDataTable(html.LoadData(sql).Tables[0]);

            sql = @"select distinct u.UserLoginID ApprovedByUserID, u.TelegramUserName ApprovedByTelegramUserName, u.FullName ApprovedBy, u.Job CheckVerifyByPosition
from dbo.TblUserLogin u
inner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID
where u.StatusID = 3 and ts.SystemID = " + html.SystemID + " and ts.WarehouseID = " + html.WarehouseID + " order by u.FullName";
            var dtApprovedBy = html.LoadData(sql).Tables[0];
            ViewBag.ApprovedByList = dtApprovedBy;
            ViewBag.ApprovedByJson = html.JsonConvertDataTable(dtApprovedBy);
            ViewBag.CheckedByJson = html.JsonConvertDataTable(dtApprovedBy);

            html = new HTML
            {
                UseTable = true,
                TableColumn = 5
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Start Date",
                ID = "TxtStartDate",
                ISDateTime = true,
                ISFloatingLabel = true,
                SingleDatePicker = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "End Date",
                ID = "TxtEndDate",
                ISDateTime = true,
                ISFloatingLabel = true,
                SingleDatePicker = true,
                DefaultShowDateValue = false
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Invoice Date",
                ID = "TxtInvoiceDate",
                ISDateTime = true,
                ISFloatingLabel = true,
                SingleDatePicker = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Payment Due Date",
                ID = "TxtPaymentDueDate",
                ISDateTime = true,
                ISFloatingLabel = true,
                SingleDatePicker = true,
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                SQL_CBO = "select CustomerPhone from dbo.TblCustomerTelegram where IsUsingTelegram = 'Yes' and CustomerID = " + PrimaryFieldValue,
                Label = "Customer Phone",
                ID = "TxtCustPhone",
                ISFloatingLabel = true,
                DisplayText = "CustomerPhone",
                Value = "CustomerPhone",
                IsUseSelect2 = false
            });
            ViewBag.DrawInvDate = html.DrawHTMLForm();

            return View();
        }
        [Authorize(Roles = "Add Customer Telegram, Edit Customer Telegram")]
        public ActionResult CreateCustTelegram(string PrimaryFieldValue)
        {
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            HTML html = new HTML
            {
                FormID = "CreateCustTelegram",
                FieldIDFocus = "TxtFirstName",
                TableName = "TblCustomerTelegram",
                PrimaryField = "TelegramCustomerID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "First Name:",
                ID = "TxtFirstName",
                DBField = "FirstName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Last Name:",
                ID = "TxtLastName",
                DBField = "LastName"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Customer Phone:",
                ID = "TxtCustomerPhone",
                DBField = "CustomerPhone",
                Required = true,
                //ISUniqueFiled = true
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo"
            });
            html.SaveChangeFunction = "saveChangeCustTelegram()";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AddCustomerTelegram(string PrimaryFieldValue, int CustomerID, string CustomerPhone, string FirstName, string LastName, string Memo)
        {
            SqlResult sqlResult = new SqlResult
            {
                Result = ResultType.Successful
            };
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            HTML html = new HTML();
            //await notificationTelegram.AddCustomerTelegram(CustomerID, CustomerPhone, FirstName, LastName, Memo, html.GetCurrentDate(), html.UserFullName, html.UserID, html.SystemID, html.WarehouseID, html.DomainID, false, int.Parse(PrimaryFieldValue));
            DoActionDataController doActionData = new DoActionDataController();
            await doActionData.AddUserToTelegram(CustomerID.ToString(), CustomerPhone, FirstName, LastName, Memo, false, int.Parse(PrimaryFieldValue));
            return Json(new { Result = sqlResult.Result, Messge = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveChangeCustTelegramNotification(int TelegramCustomerID, string TelegramNotificationIDs)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = "update dbo.TblCustomerTelegram set TelegramNotificationID = @TelegramNotificationID where TelegramCustomerID = @TelegramCustomerID";
            Dictionary<string, string> para = new Dictionary<string, string> {
                {"TelegramCustomerID", TelegramCustomerID.ToString() },
                {"TelegramNotificationID", TelegramNotificationIDs },
            };
            sqlResult = html.SaveChangeData(sql, false, para);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowCustomerTelegramNotification(int CustomerID)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = @"select ROW_NUMBER() over(order by TelegramMessageID desc) as [No], 
tg.DateSend, tg.SendBy, tg.ConnectionID, ts.[Description], '<p class=""d-inline-block text-truncate"" style=""max-width: 150px;"" title=""'+tg.MessageText+'"">' + tg.MessageText + '</p>' MessageText, tg.MessageStatus, tg.CustomerPhone
from TblCustomerTelegramMessage tg
inner join dbo.TblTelegramNotificationSchedule ts on ts.TelegramNotificationID = tg.TelegramNotificationID
where tg.CustomerID = " + CustomerID;
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Description", FieldName = "Description", IsGroupByField=true, Colspan = 7 },
                new GridField() { Header = "Date", FieldName = "DateSend", Format = "dd-MM-yyyy hh:mm:ss tt" },
                new GridField() { Header = "Send By", FieldName = "SendBy" },
                new GridField() { Header = "Customer Phone", FieldName = "CustomerPhone" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Message", FieldName = "MessageText", ClassName="text-truncate" },
                new GridField() { Header = "Status", FieldName = "MessageStatus" }
            };
            html.OrderBy = "TelegramMessageID desc";
            html.IsHaveWhereCondition = true;
            html.ShowGridOnly = true;
            html.TblID = "TblCustTelNot";
            //html.ShowPaging = false;
            //html.UsingJqueryDataTable = true;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [ValidateInput(false)]
        public async Task<ActionResult> NotificationPayment(string CustomerName, string CustomerCode, int CustomerID, string InvoiceNo, string InvoiceDate, string InvoiceDueDate, string StartDate, bool Download = false, bool SendNotificationPaymentToCustomer = false, string CustPhone = "", string MessageText = "", string EndDate = "",
            string PackageName = "", decimal? UnitPrice = null, int? Duration = null, string UM = "", string Feature = "", decimal? ExchangeRate = null, bool HideSignature = false, string ReviseNPNo = "", string ExtraRowsJson = "")
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string> {
                { "CustID", CustomerID.ToString() },
                { "InvoiceNo", InvoiceNo },
                { "StartDate", StartDate },
                { "EndDate", EndDate },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID },
            };
            string sql = "SP_NOTIFICATION_PAYMENT";
            DataSet ds = html.LoadData(sql, true, para);

            // Filter Table 2 rows for Notification Payment
            if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
            {
                DataRow primaryRow = null;
                foreach (DataRow r in ds.Tables[2].Rows)
                {
                    if (r.RowState != DataRowState.Deleted)
                    {
                        string desc = ds.Tables[2].Columns.Contains("Description") ? r["Description"]?.ToString()?.ToLower() ?? "" : "";
                        if (ds.Tables[2].Columns.Contains("ServiceID") && r["ServiceID"]?.ToString() == "1")
                        {
                            primaryRow = r;
                            break;
                        }
                        if (primaryRow == null && !desc.Contains("router") && !desc.Contains("deposit") && !desc.Contains("installation"))
                        {
                            primaryRow = r;
                        }
                    }
                }

                if (primaryRow == null)
                    primaryRow = ds.Tables[2].Rows[0];

                if (!string.IsNullOrEmpty(ExtraRowsJson))
                {
                    for (int ri = ds.Tables[2].Rows.Count - 1; ri >= 0; ri--)
                    {
                        if (ds.Tables[2].Rows[ri] != primaryRow)
                        {
                            ds.Tables[2].Rows[ri].Delete();
                        }
                    }
                    ds.Tables[2].AcceptChanges();
                    ApplyExtraRowsToInvoice(ds, ExtraRowsJson);
                }
                else
                {
                    for (int ri = ds.Tables[2].Rows.Count - 1; ri >= 0; ri--)
                    {
                        DataRow r = ds.Tables[2].Rows[ri];
                        if (r != primaryRow)
                        {
                            string desc = ds.Tables[2].Columns.Contains("Description") ? r["Description"]?.ToString()?.ToLower() ?? "" : "";
                            if (desc.Contains("router") || desc.Contains("deposit") || desc.Contains("installation"))
                            {
                                r.Delete();
                            }
                        }
                    }
                    ds.Tables[2].AcceptChanges();
                }
            }
            else if (!string.IsNullOrEmpty(ExtraRowsJson))
            {
                ApplyExtraRowsToInvoice(ds, ExtraRowsJson);
            }

            // ✅ Apply changed package/pricing if specified
            ApplyPackageChangesToInvoice(ds, PackageName, UnitPrice, Duration, UM, Feature, ExchangeRate, StartDate, EndDate);

            // ✅ Recalculate Table 5 totals based on remaining rows in Table 2
            if (ds.Tables.Count > 2 && ds.Tables.Count > 5 && ds.Tables[5].Rows.Count > 0)
            {
                decimal newSubTotal = 0;
                foreach (DataRow r in ds.Tables[2].Rows)
                {
                    if (r.RowState != DataRowState.Deleted && ds.Tables[2].Columns.Contains("Amount"))
                    {
                        decimal amt = 0;
                        if (decimal.TryParse(r["Amount"].ToString(), out amt))
                            newSubTotal += amt;
                    }
                }
                ds.Tables[5].Rows[0]["SubTotal"] = newSubTotal;

                decimal vat = 0;
                if (ds.Tables[5].Columns.Contains("VAT") && ds.Tables[5].Rows[0]["VAT"].ToString() != "0.00")
                {
                    vat = Math.Round(newSubTotal * 0.10m, 2);
                    ds.Tables[5].Rows[0]["VAT"] = vat.ToString("0.00");
                }

                decimal discount = 0;
                if (ds.Tables[5].Columns.Contains("DiscountUSD"))
                {
                    decimal.TryParse(ds.Tables[5].Rows[0]["DiscountUSD"].ToString(), out discount);
                }

                decimal grandTotal = newSubTotal + vat - discount;
                if (ds.Tables[5].Columns.Contains("GrandTotal"))
                {
                    ds.Tables[5].Rows[0]["GrandTotal"] = grandTotal;
                }
            }

            string invType = "";
            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0 && ds.Tables[1].Columns.Contains("TypeOfInvoice"))
            {
                invType = ds.Tables[1].Rows[0]["TypeOfInvoice"]?.ToString()?.Trim() ?? "";
            }
            else if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Columns.Contains("TypeOfInvoice"))
            {
                invType = ds.Tables[0].Rows[0]["TypeOfInvoice"]?.ToString()?.Trim() ?? "";
            }

            bool isVAT = invType.Equals("VAT", StringComparison.OrdinalIgnoreCase) || invType.Equals("CM", StringComparison.OrdinalIgnoreCase);

            // ✅ Override TermCondition only for VAT & CM from Default Company in SystemID = 13 (ISDefault = 1)
            if (isVAT)
            {
                try
                {
                    string sqlTerm = "SELECT TOP 1 TermConditionVAT FROM dbo.TblCompany WHERE SystemID = 13 AND ISDefault = 1";
                    var dtTerm = html.LoadData(sqlTerm).Tables[0];
                    if (dtTerm.Rows.Count > 0)
                    {
                        string termCondition = dtTerm.Rows[0]["TermConditionVAT"]?.ToString();
                        if (!string.IsNullOrEmpty(termCondition))
                        {
                            termCondition = termCondition.Replace("&#013;&#010;", "\r\n").Replace("&#013;", "\r").Replace("&#010;", "\n");
                            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0 && ds.Tables[1].Columns.Contains("TermCondition"))
                                ds.Tables[1].Rows[0]["TermCondition"] = termCondition;
                            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Columns.Contains("TermCondition"))
                                ds.Tables[0].Rows[0]["TermCondition"] = termCondition;
                        }
                    }
                }
                catch { }
            }
            else
            {
                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0 && ds.Tables[1].Columns.Contains("TermCondition"))
                    ds.Tables[1].Rows[0]["TermCondition"] = "";
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Columns.Contains("TermCondition"))
                    ds.Tables[0].Rows[0]["TermCondition"] = "";
            }

            decimal exchangeRate = 4200;
            if (isVAT)
            {
                string sqlEx = "select top 1 format(e.ExchangeRate, '#,#') ExchangeRate from dbo.TblExchangeRate e inner join dbo.TblCurrency c on c.CurrencyID = e.CurrencyID where c.CurrencyName = 'Riel' and e.SystemID = " + html.SystemID + " and e.WarehouseID = " + html.WarehouseID + " and e.DomainID like '%" + html.DomainID + "%' order by format(e.DateEntry, 'yyyyMMdd') desc";
                var dtExchangeRate = html.LoadData(sqlEx).Tables[0];
                if (dtExchangeRate.Rows.Count > 0)
                {
                    string exchangeRateStr = dtExchangeRate.Rows[0]["ExchangeRate"].ToString();
                    exchangeRate = Convert.ToDecimal(exchangeRateStr.Replace(",", ""));
                }
            }
            else
            {
                exchangeRate = 4200;
            }

            if (ExchangeRate.HasValue && ExchangeRate.Value > 0)
            {
                exchangeRate = ExchangeRate.Value;
            }

            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0 && ds.Tables[1].Columns.Contains("ExchangeRate"))
            {
                ds.Tables[1].Rows[0]["ExchangeRate"] = exchangeRate;
            }
            ViewBag.ExchangeRate = exchangeRate.ToString("#,0");

            // Check duplicate Notification Payment for this Invoice (for informational notice banner)
            string checkDupSql = @"
                SELECT TOP 1 NPNo, StatusID,
                       CASE WHEN StatusID = 60 THEN 'Pending'
                            WHEN StatusID = 90 THEN 'Approved'
                            WHEN StatusID = 91 THEN 'Disapproved'
                            ELSE CAST(StatusID AS varchar) END AS StatusName
                FROM dbo.TblNotificationPayment
                WHERE InvoiceNo = @InvoiceNo AND (@CustomerID = 0 OR CustomerID = @CustomerID) AND StatusID IN (60, 90)
                ORDER BY NotificationPaymentID DESC;";
            var dtDup = html.LoadData(checkDupSql, false, new Dictionary<string, string> {
                { "@InvoiceNo", InvoiceNo ?? "" },
                { "@CustomerID", CustomerID.ToString() }
            }).Tables[0];
            string existNP = "";
            string existStatus = "";
            if (dtDup.Rows.Count > 0)
            {
                existNP = dtDup.Rows[0]["NPNo"].ToString();
                existStatus = dtDup.Rows[0]["StatusName"].ToString();
            }

            // Query unapproved Notification Payments across all customers/invoices (StatusID = 60, not yet approved 90)
            // Prioritize unapproved records for the current invoice first
            string uncheckedSql = @"
                SELECT TOP 100 NotificationPaymentID, NPNo, CustomerID, CustomerName, InvoiceNo, CreateDate, StatusID, ISNULL(CheckStatusID, 60) AS CheckStatusID,
                       CASE WHEN StatusID = 60 AND ISNULL(CheckStatusID, 60) = 70 THEN 'Checked'
                            WHEN StatusID = 60 THEN 'Pending'
                            WHEN StatusID = 90 THEN 'Approved'
                            WHEN StatusID = 91 THEN 'Disapproved'
                            ELSE CAST(StatusID AS varchar) END AS StatusName
                FROM dbo.TblNotificationPayment
                WHERE (SystemID = " + html.SystemID + @" OR " + html.SystemID + @" = 0 OR SystemID IS NULL)
                  AND (WarehouseID = " + html.WarehouseID + @" OR " + html.WarehouseID + @" = 0 OR WarehouseID IS NULL)
                  AND StatusID = 60
                ORDER BY CASE WHEN InvoiceNo = @InvoiceNo THEN 0 ELSE 1 END, NotificationPaymentID DESC;";
            var dtUnchecked = html.LoadData(uncheckedSql, false, new Dictionary<string, string> {
                { "@InvoiceNo", InvoiceNo ?? "" }
            }).Tables[0];
            string uncheckedJson = html.JsonConvertDataTable(dtUnchecked);

            // Preview NPNo: if revising, show ReviseNPNo; otherwise preview next NPNo from TblPrefix
            string displayNP = (ReviseNPNo ?? "").Trim();
            if (string.IsNullOrEmpty(displayNP))
            {
                try
                {
                    string prefixSql = @"
                        DECLARE @Prefix nvarchar(100), @Module nvarchar(50);
                        SELECT TOP 1 @Prefix = Prefix, @Module = Module 
                        FROM dbo.TblPrefix 
                        WHERE (Module = 'NotificationPayment' OR Module = 'NP' OR Prefix LIKE 'NP%')
                          AND SystemID = " + html.SystemID + @" AND WarehouseID = " + html.WarehouseID + @"
                        ORDER BY CASE WHEN Module = 'NotificationPayment' THEN 0 ELSE 1 END, PrefixID DESC;

                        IF @Prefix IS NULL
                        BEGIN
                            SELECT TOP 1 @Prefix = Prefix, @Module = Module 
                            FROM dbo.TblPrefix 
                            WHERE (Module = 'NotificationPayment' OR Module = 'NP' OR Prefix LIKE 'NP%')
                            ORDER BY CASE WHEN Module = 'NotificationPayment' THEN 0 ELSE 1 END, PrefixID DESC;
                        END

                        IF @Prefix IS NOT NULL
                            EXEC [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + @", " + html.WarehouseID + @", '" + html.DomainID + @"';
                    ";
                    DataSet dsPrefix = html.LoadData(prefixSql, false, null);
                    if (dsPrefix != null && dsPrefix.Tables.Count > 0 && dsPrefix.Tables[0].Rows.Count > 0)
                    {
                        if (dsPrefix.Tables[0].Columns.Contains("PrefixNumber"))
                            displayNP = dsPrefix.Tables[0].Rows[0]["PrefixNumber"].ToString();
                        else
                            displayNP = dsPrefix.Tables[0].Rows[0][0].ToString();
                    }
                }
                catch { }

                if (string.IsNullOrEmpty(displayNP))
                {
                    try
                    {
                        string getPrefix = @"
                            SELECT TOP 1 PrefixID, Prefix, ISNULL(CurrentNumber, 0) + 1 AS NextNum
                            FROM dbo.TblPrefix 
                            WHERE (Module = 'NotificationPayment' OR Module = 'NP' OR Prefix LIKE 'NP%')
                              AND SystemID = " + html.SystemID + @" AND WarehouseID = " + html.WarehouseID + @"
                            ORDER BY CASE WHEN Module = 'NotificationPayment' THEN 0 ELSE 1 END, PrefixID DESC;";
                        var dtP = html.LoadData(getPrefix).Tables[0];
                        if (dtP.Rows.Count > 0)
                        {
                            int nextVal = Convert.ToInt32(dtP.Rows[0]["NextNum"]);
                            string pfx = dtP.Rows[0]["Prefix"].ToString();
                            if (pfx.Contains("{") && pfx.Contains("}"))
                            {
                                displayNP = pfx.Replace("{yy}", DateTime.Now.ToString("yy"))
                                               .Replace("{MM}", DateTime.Now.ToString("MM"))
                                               .Replace("{dd}", DateTime.Now.ToString("dd"))
                                               .Replace("{000000}", nextVal.ToString("D6"))
                                               .Replace("{00000}", nextVal.ToString("D5"))
                                               .Replace("{0000}", nextVal.ToString("D4"))
                                               .Replace("{000}", nextVal.ToString("D3"));
                            }
                            else
                            {
                                displayNP = $"{pfx}-{DateTime.Now:yyMM}-{nextVal:D5}";
                            }
                        }
                    }
                    catch { }
                }
            }

            if (Download || SendNotificationPaymentToCustomer)
            {
                string dt = html.DrawInvoice(ds, false, (int)exchangeRate, 0, false, true, true, InvoiceDate, InvoiceDueDate);
                if (HideSignature)
                {
                    dt = dt.Replace("id=\"Signature\"", "id=\"Signature\" style=\"visibility:hidden;\"");
                }
                dt = dt.Replace("សរុបរួមទាំងអាករ / Total Included VAT 10%:", "សរុបទឹកប្រាក់រួម / Grand Total USD:");
                dt = dt.Replace("Total Included VAT", "Grand Total USD");
                dt = dt.Replace("សរុបរួមទាំងអាករ", "សរុបទឹកប្រាក់រួម");
                dt = dt.Replace("Discount %", "Discount $");
                dt = dt.Replace("Sub Total", "Total");
                dt = dt.Replace("Grand Total KHR:", "Grand Total KHR:"); // Ensure it stays if it exists
                dt = dt.Replace("សរុបរួមទាំងអាករជាលុយរៀល", "សរុបទឹកប្រាក់រួមជាលុយរៀល");
                dt = dt.Replace(">រ", ">&#x17DB;");
                dt = dt.Replace(" រ", " &#x17DB;");
                dt = dt.Replace("ស&#x17DB;ុប", "សរុប");
                dt = dt.Replace("អាក&#x17DB;", "អាករ");
                dt = dt.Replace("ប្តូ&#x17DB;", "ប្តូរ");
                dt = dt.Replace("ប្ដូ&#x17DB;", "ប្ដូរ");
                dt = dt.Replace(" &#x17DB;ាជធានី", " រាជធានី");
                dt = dt.Replace(">&#x17DB;ាជធានី", ">រាជធានី");

                // ✅ Restore Khmer 'Ro' character if it was incorrectly replaced with the Riel symbol (e.g. inside Customer Names like ពេជ្រ រ៉ាហ្សង់)
                dt = System.Text.RegularExpressions.Regex.Replace(dt, @"(?:&#x17DB;|៛)(?=[\u1780-\u17DA\u17DC-\u17FF])", "រ");

                if (!isVAT)
                {
                    dt = System.Text.RegularExpressions.Regex.Replace(dt, @"<tr>\s*<td[^>]*>[^<]*Sale's Name[^<]*</td>.*?</tr>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                    dt = System.Text.RegularExpressions.Regex.Replace(dt, @"<tr>\s*<td[^>]*>[^<]*Sale's Phone[^<]*</td>.*?</tr>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                }

                // ✅ Remove Note from invoice Vat and Cm (សំគាល់៖ ច្បាប់ដើមសម្រាប់អ្នកទិញ ច្បាប់ចម្លងសម្រាប់អ្នកលក់ / Note: Original invoice for customer and copied invoice for seller)
                dt = System.Text.RegularExpressions.Regex.Replace(dt, @"<div class=""row"">\s*<div class=""col text-sm"">.*?</div>\s*</div>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                dt = System.Text.RegularExpressions.Regex.Replace(dt, @"<div class=""row"">\s*<div class=""col"">\s*<label[^>]*>(?:សំគាល់|សម្គាល់|Original invoice).*?</label>.*?</div>\s*</div>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);


                // ✅ Inject Notification Payment No.
                if (!string.IsNullOrEmpty(displayNP))
                {
                    if (!dt.Contains(displayNP))
                    {
                        string npRow = "<tr><td class=\"text-nowrap\">លេខរៀង/Notification Payment No.៖ </td><td class=\"text-nowrap\">" + displayNP + "</td></tr>\n";
                        if (dt.Contains("<tr><td>កាលបរិច្ឆេទ/Date"))
                        {
                            dt = dt.Replace("<tr><td>កាលបរិច្ឆេទ/Date", npRow + "<tr><td>កាលបរិច្ឆេទ/Date");
                        }
                        else
                        {
                            var regex = new System.Text.RegularExpressions.Regex(@"(<tr>\s*<td[^>]*>[^<]*Date[^<]*</td>)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            dt = regex.Replace(dt, npRow + "$1", 1);
                        }
                    }
                }

                string FilePath = HttpContext.Server.MapPath("~/Templates/HtmlTemplate.html");
                StreamReader str = new StreamReader(FilePath);
                string Text = str.ReadToEnd();
                str.Close();
                //Text = Text.Replace("/", html.GetLocalIP());
                Text = Text.Replace("{Title}", "Notification Payment");
                Text = Text.Replace("{BodyMessage}", dt);
                result = await html.HTMLToPDF(Text.ToString(), "/Uploads/CustomerAttFiles/" + CustomerName + "-" + CustomerCode, "Notification Payment");
                if (Download)
                    return Json(new { Result = result.Result, Message = result.Message, OutPutFile = result.OutPutUrlFile, FileName = Path.GetFileName(result.OutPutFile) }, JsonRequestBehavior.AllowGet);
                else
                {
                    NotificationTelegram notificationTelegram = new NotificationTelegram();
                    result = await notificationTelegram.NotificationToCustomer(CustPhone, CustomerName, "", CustomerID, MessageText, "", result.OutPutFile, "", "", "Notification Payment");
                    return Json(new { Result = result.Result, Message = result.Message, FileName = HttpUtility.HtmlEncode(result.OutPutFile) }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                string dt = html.DrawInvoice(ds, false, (int)exchangeRate, 0, false, true, true, InvoiceDate, InvoiceDueDate);
                if (HideSignature)
                {
                    dt = dt.Replace("id=\"Signature\"", "id=\"Signature\" style=\"visibility:hidden;\"");
                }
                dt = dt.Replace("សរុបរួមទាំងអាករ / Total Included VAT 10%:", "សរុបទឹកប្រាក់រួម / Grand Total USD:");
                dt = dt.Replace("Total Included VAT", "Grand Total USD");
                dt = dt.Replace("សរុបរួមទាំងអាករ", "សរុបទឹកប្រាក់រួម");
                dt = dt.Replace("Discount %", "Discount $");
                dt = dt.Replace("Sub Total", "Total");
                dt = dt.Replace("សរុបរួមទាំងអាករជាលុយរៀល", "សរុបទឹកប្រាក់រួមជាលុយរៀល");
                dt = dt.Replace(">រ", ">&#x17DB;");
                dt = dt.Replace(" រ", " &#x17DB;");
                dt = dt.Replace("ស&#x17DB;ុប", "សរុប");
                dt = dt.Replace("អាក&#x17DB;", "អាករ");
                dt = dt.Replace("ប្តូ&#x17DB;", "ប្តូរ");
                dt = dt.Replace("ប្ដូ&#x17DB;", "ប្ដូរ");
                dt = dt.Replace(" &#x17DB;ាជធានី", " រាជធានី");
                dt = dt.Replace(">&#x17DB;ាជធានី", ">រាជធានី");

                dt = dt.Replace("លេខរៀងវិក្កយបត្រ/Invoice No.៖", "លេខរៀង/Notification Payment No.៖");
                dt = dt.Replace("លេខរៀងវិក្កយបត្រ/Invoice No. ", "លេខរៀង/Notification Payment No.៖ ");
                dt = dt.Replace("លេខរៀងវិក្កយបត្រ/Invoice No.", "លេខរៀង/Notification Payment No.៖");

                // ✅ Restore Khmer 'Ro' character if it was incorrectly replaced with the Riel symbol (e.g. inside Customer Names like ពេជ្រ រ៉ាហ្សង់)
                dt = System.Text.RegularExpressions.Regex.Replace(dt, @"(?:&#x17DB;|៛)(?=[\u1780-\u17DA\u17DC-\u17FF])", "រ");

                if (!isVAT)
                {
                    dt = System.Text.RegularExpressions.Regex.Replace(dt, @"<tr>\s*<td[^>]*>[^<]*Sale's Name[^<]*</td>.*?</tr>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                    dt = System.Text.RegularExpressions.Regex.Replace(dt, @"<tr>\s*<td[^>]*>[^<]*Sale's Phone[^<]*</td>.*?</tr>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                }

                // ✅ Remove Note from invoice Vat and Cm (សំគាល់៖ ច្បាប់ដើមសម្រាប់អ្នកទិញ ច្បាប់ចម្លងសម្រាប់អ្នកលក់ / Note: Original invoice for customer and copied invoice for seller)
                dt = System.Text.RegularExpressions.Regex.Replace(dt, @"<div class=""row"">\s*<div class=""col text-sm"">.*?</div>\s*</div>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                dt = System.Text.RegularExpressions.Regex.Replace(dt, @"<div class=""row"">\s*<div class=""col"">\s*<label[^>]*>(?:សំគាល់|សម្គាល់|Original invoice).*?</label>.*?</div>\s*</div>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);


                if (!string.IsNullOrEmpty(ReviseNPNo))
                {
                    string origInfo = "";
                    try
                    {
                        string checkOrigSql = @"
                            SELECT TOP 1 CustomerName, InvoiceNo 
                            FROM dbo.TblNotificationPayment 
                            WHERE NPNo = @ReviseNPNo AND StatusID = 60";
                        var dtOrig = html.LoadData(checkOrigSql, false, new Dictionary<string, string> { { "@ReviseNPNo", ReviseNPNo } }).Tables[0];
                        if (dtOrig.Rows.Count > 0)
                        {
                            string origCust = dtOrig.Rows[0]["CustomerName"]?.ToString() ?? "";
                            string origInv = dtOrig.Rows[0]["InvoiceNo"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(origInv) && !origInv.Equals(InvoiceNo, StringComparison.OrdinalIgnoreCase))
                            {
                                origInfo = $" (Originally from {origCust} - {origInv})";
                            }
                        }
                    }
                    catch { }

                    dt = $"<div class=\"alert alert-info mb-2\" id=\"alertExistingNP\" style=\"font-size:14px;\"><i class=\"fa fa-pen-to-square mr-1\"></i> <b>Revising Notification Payment:</b> <span class=\"badge bg-warning text-dark\">{ReviseNPNo}</span>{origInfo} (Status: <b>Not Approved</b> - sequence number will not increase)</div>" + dt;
                }
                else if (!string.IsNullOrEmpty(existNP))
                {
                    dt = $"<div class=\"alert alert-warning mb-2\" id=\"alertExistingNP\" style=\"font-size:14px;\"><i class=\"fa fa-exclamation-triangle mr-1\"></i> <b>Notice:</b> This invoice already has a Notification Payment: <span class=\"badge bg-danger\">{existNP}</span> (Status: <b>{existStatus}</b>)</div>" + dt;
                }

                if (!string.IsNullOrEmpty(displayNP))
                {
                    if (!dt.Contains(displayNP))
                    {
                        string npRow = "<tr><td class=\"text-nowrap\">លេខរៀង/Notification Payment No.៖ </td><td class=\"text-nowrap\">" + displayNP + "</td></tr>\n";
                        if (dt.Contains("<tr><td>កាលបរិច្ឆេទ/Date"))
                        {
                            dt = dt.Replace("<tr><td>កាលបរិច្ឆេទ/Date", npRow + "<tr><td>កាលបរិច្ឆេទ/Date");
                        }
                        else
                        {
                            var regex = new System.Text.RegularExpressions.Regex(@"(<tr>\s*<td[^>]*>[^<]*Date[^<]*</td>)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            dt = regex.Replace(dt, npRow + "$1", 1);
                        }
                    }
                }
                dt += $"<input type=\"hidden\" id=\"hdnInvoiceExchangeRate\" value=\"{exchangeRate}\" />";
                dt += $"<input type=\"hidden\" id=\"hdnExistingNPNo\" value=\"{existNP}\" />";
                dt += $"<input type=\"hidden\" id=\"hdnExistingNPStatus\" value=\"{existStatus}\" />";
                dt += $"<input type=\"hidden\" id=\"hdnUncheckedNPJson\" value='{HttpUtility.HtmlAttributeEncode(uncheckedJson)}' />";
                dt += $"<input type=\"hidden\" id=\"hdnDisplayNP\" value=\"{displayNP}\" />";

                ViewBag.invTmp = dt;
                return View();
            }
        }

        [HttpGet]
        public JsonResult GetUncheckedNotificationPayments(string InvoiceNo = "", int CustomerID = 0)
        {
            HTML html = new HTML();
            string uncheckedSql = @"
                SELECT TOP 100 NotificationPaymentID, NPNo, CustomerID, CustomerName, InvoiceNo, CreateDate, StatusID, ISNULL(CheckStatusID, 60) AS CheckStatusID,
                       CASE WHEN StatusID = 60 AND ISNULL(CheckStatusID, 60) = 70 THEN 'Checked'
                            WHEN StatusID = 60 THEN 'Pending'
                            WHEN StatusID = 90 THEN 'Approved'
                            WHEN StatusID = 91 THEN 'Disapproved'
                            ELSE CAST(StatusID AS varchar) END AS StatusName
                FROM dbo.TblNotificationPayment
                WHERE (SystemID = " + html.SystemID + @" OR " + html.SystemID + @" = 0 OR SystemID IS NULL)
                  AND (WarehouseID = " + html.WarehouseID + @" OR " + html.WarehouseID + @" = 0 OR WarehouseID IS NULL)
                  AND StatusID = 60
                ORDER BY CASE WHEN InvoiceNo = @InvoiceNo THEN 0 ELSE 1 END, NotificationPaymentID DESC;";
            var dt = html.LoadData(uncheckedSql, false, new Dictionary<string, string> {
                { "@InvoiceNo", InvoiceNo ?? "" }
            }).Tables[0];
            return Json(html.JsonConvertDataTable(dt), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> SendNotificationPayment(string CustomerName, string CustomerCode, string CustPhone, int CustomerID, string MessageText, string InvoiceNo, string InvoiceDate, string InvoiceDueDate)
        {
            MessageText = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(MessageText));
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            result = await html.URLtoPDF("CustomerInvoice/Invoice?CustomerID=" + CustomerID + "&InvoiceNo=" + InvoiceNo + "&InvoiceDate=" + InvoiceDate +
                "&InvoiceDueDate=" + InvoiceDueDate
                , "", "/Uploads/CustomerAttFiles/" + CustomerName + "-" + CustomerCode, "NotificationPayment");

            NotificationTelegram notificationTelegram = new NotificationTelegram();
            result = await notificationTelegram.NotificationToCustomer(CustPhone, CustomerName, "", CustomerID, MessageText, "", result.OutPutFile, "", "", "Notification Payment");

            return Json(new { Result = result.Result, Message = result.Message, FileName = result.OutPutFile }, JsonRequestBehavior.AllowGet);
        }

        private void ApplyPackageChangesToInvoice(
            DataSet dsInv,
            string packageName,
            decimal? unitPrice,
            int? duration,
            string um,
            string feature,
            decimal? customExchangeRate,
            string startDate,
            string endDate)
        {
            packageName = HttpUtility.UrlDecode(packageName ?? "").Trim();
            um = HttpUtility.UrlDecode(um ?? "").Trim();
            feature = HttpUtility.UrlDecode(feature ?? "").Trim();
            feature = HttpUtility.HtmlDecode(feature);

            if (string.IsNullOrWhiteSpace(packageName) && !unitPrice.HasValue)
                return;

            if (dsInv == null || dsInv.Tables.Count <= 2 || dsInv.Tables[2].Rows.Count == 0)
                return;

            // Look up price, duration, UM, feature from DB if not provided
            if ((!unitPrice.HasValue || unitPrice.Value <= 0) && !string.IsNullOrWhiteSpace(packageName))
            {
                try
                {
                    HTML h = new HTML();
                    string sqlPkg = @"
                        SELECT TOP 1 pd.Price, pd.Duration, pd.UM, pd.Feature
                        FROM dbo.TblPackageDetail pd
                        INNER JOIN dbo.TblPackages p ON p.PackageID = pd.PackageID
                        WHERE concat(p.PackageName, ' - ', pd.Bandwidth, ' Mbps', ' - ', pd.Duration, ' ', pd.UM) = @PkgName
                           OR p.PackageName = @PkgName";
                    var dtPkg = h.LoadData(sqlPkg, false, new Dictionary<string, string> { { "@PkgName", packageName } }).Tables[0];
                    if (dtPkg.Rows.Count > 0)
                    {
                        if (!unitPrice.HasValue || unitPrice.Value <= 0)
                            unitPrice = Convert.ToDecimal(dtPkg.Rows[0]["Price"]);
                        if (!duration.HasValue || duration.Value <= 0)
                            duration = Convert.ToInt32(dtPkg.Rows[0]["Duration"]);
                        if (string.IsNullOrWhiteSpace(um))
                            um = dtPkg.Rows[0]["UM"].ToString();
                        if (string.IsNullOrWhiteSpace(feature))
                            feature = dtPkg.Rows[0]["Feature"].ToString();
                    }
                }
                catch { }
            }

            DataRow pkgRow = null;
            foreach (DataRow r in dsInv.Tables[2].Rows)
            {
                if (r.RowState != DataRowState.Deleted)
                {
                    string desc = r.Table.Columns.Contains("Description") ? r["Description"]?.ToString() ?? "" : "";
                    if (!desc.ToLower().Contains("deposit"))
                    {
                        pkgRow = r;
                        break;
                    }
                }
            }
            if (pkgRow == null && dsInv.Tables[2].Rows.Count > 0)
            {
                pkgRow = dsInv.Tables[2].Rows[0];
            }
            if (pkgRow == null) return;

            int dur = duration.HasValue && duration.Value > 0 ? duration.Value : 1;
            string umUnit = string.IsNullOrWhiteSpace(um) ? "Month" : um;
            string umDisplay = dur + " " + (dur > 1 ? (umUnit.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? umUnit : umUnit + "s") : umUnit.TrimEnd('s', 'S'));

            if (!string.IsNullOrWhiteSpace(packageName))
            {
                if (pkgRow.Table.Columns.Contains("UM"))
                {
                    pkgRow["UM"] = umDisplay;
                }
                if (pkgRow.Table.Columns.Contains("QTY"))
                {
                    pkgRow["QTY"] = 1;
                }
            }

            if (unitPrice.HasValue && unitPrice.Value >= 0)
            {
                decimal price = unitPrice.Value;
                if (pkgRow.Table.Columns.Contains("UnitPrice"))
                {
                    pkgRow["UnitPrice"] = price;
                }
                decimal discount = 0;
                if (pkgRow.Table.Columns.Contains("DiscountUSD") && pkgRow["DiscountUSD"] != DBNull.Value)
                {
                    decimal.TryParse(pkgRow["DiscountUSD"].ToString(), out discount);
                }
                decimal amt = price - discount;
                if (pkgRow.Table.Columns.Contains("Amount"))
                {
                    pkgRow["Amount"] = amt;
                }
            }

            if (!string.IsNullOrWhiteSpace(packageName))
            {
                StringBuilder sbDesc = new StringBuilder();
                string currentDesc = pkgRow.Table.Columns.Contains("Description") ? pkgRow["Description"]?.ToString() ?? "" : "";
                string servicePlan = "Service Plan: Internet Service";
                if (currentDesc.Contains("Service Plan:"))
                {
                    var m = System.Text.RegularExpressions.Regex.Match(currentDesc, @"(Service Plan:[^<]+)");
                    if (m.Success) servicePlan = m.Groups[1].Value.Trim();
                }
                sbDesc.Append(servicePlan + "<br/>");
                sbDesc.Append("Package Name: " + packageName + " <br/>");

                if (!string.IsNullOrWhiteSpace(feature))
                {
                    sbDesc.Append("<br/><b>Feature:</b><br/>");
                    string decoded = feature.Replace("&#013;", "\r").Replace("&#010;", "\n");
                    string[] fLines = decoded.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string fLine in fLines)
                    {
                        string trimmed = fLine.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            sbDesc.Append("<div class=\"row\"><div class=\"col-auto ml-2\">" +
                                "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-caret-right-fill\" viewBox=\"0 0 16 16\">" +
                                "<path d=\"m12.14 8.753-5.482 4.796c-.646.566-1.658.106-1.658-.753V3.204a1 1 0 0 1 1.659-.753l5.48 4.796a1 1 0 0 1 0 1.506z\"/></svg></div>" +
                                "<div class=\"col\"><p class=\"text-justify\">" + trimmed + "</p></div></div>");
                        }
                    }
                }

                DateTime startDt;
                if (!DateTime.TryParse(startDate, out startDt))
                {
                    startDt = DateTime.Now;
                }

                DateTime endDt = startDt;
                string uLower = umUnit.ToLower();
                if (uLower.Contains("month")) endDt = startDt.AddMonths(dur).AddDays(-1);
                else if (uLower.Contains("day")) endDt = startDt.AddDays(dur).AddDays(-1);
                else if (uLower.Contains("year")) endDt = startDt.AddYears(dur).AddDays(-1);

                if (!string.IsNullOrWhiteSpace(endDate))
                {
                    DateTime parsedEnd;
                    if (DateTime.TryParse(endDate, out parsedEnd))
                        endDt = parsedEnd;
                }

                sbDesc.Append("<div class=\"row\"><div class=\"col-auto\"><label class=\"text-primary text-nowrap\">");
                sbDesc.Append($"Period {umDisplay} From {startDt:dd-MMM-yyyy} To {endDt:dd-MMM-yyyy}");
                sbDesc.Append("</label></div></div>");

                if (currentDesc.Contains("FREE") && currentDesc.Contains("Period From"))
                {
                    var mFree = System.Text.RegularExpressions.Regex.Match(currentDesc, @"(<div class=""row""><div class=""col-auto""><label class=""text-danger text-nowrap"">FREE [^<]+</label></div></div>)");
                    if (mFree.Success)
                    {
                        sbDesc.Append(mFree.Groups[1].Value);
                    }
                }

                pkgRow["Description"] = sbDesc.ToString();
            }

            if (customExchangeRate.HasValue && customExchangeRate.Value > 0)
            {
                if (dsInv.Tables.Count > 1 && dsInv.Tables[1].Rows.Count > 0 && dsInv.Tables[1].Columns.Contains("ExchangeRate"))
                {
                    dsInv.Tables[1].Rows[0]["ExchangeRate"] = customExchangeRate.Value;
                }
            }
        }

        public class ExtraServiceRowDTO
        {
            public string Description { get; set; }
            public string UM { get; set; }
            public decimal QTY { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal DiscountUSD { get; set; }
            public decimal Amount { get; set; }
            public int ServiceID { get; set; }
            public int OtherChargeID { get; set; }
        }

        [HttpGet]
        public ActionResult GetServiceTypes()
        {
            HTML html = new HTML();
            string sql = @"
                SELECT * FROM (
                    SELECT 
                        s.ServiceID,
                        s.ServiceName,
                        ISNULL(c.Price, 0) AS Price,
                        ISNULL(NULLIF(c.UM, ''), 'Month') AS UM,
                        ISNULL(c.OtherChargeID, 0) AS OtherChargeID,
                        'Services' AS TypeGroup,
                        CASE 
                            WHEN s.ServiceName = 'Public IP' THEN 1 
                            WHEN s.ServiceName = 'Maintenance' THEN 2 
                            WHEN s.ServiceName = 'Services' THEN 3 
                            WHEN s.ServiceName = 'Internet Service' THEN 4
                            WHEN s.ServiceName = 'Other Charge' THEN 5
                            ELSE 6 
                        END AS SortOrder
                    FROM dbo.TblServices s
                    OUTER APPLY (
                        SELECT TOP 1 
                            o.OtherChargeID,
                            o.OtherChargeItem,
                            o.Fee AS Price,
                            o.UM
                        FROM dbo.TblOtherCharge o
                        WHERE o.SystemID = 13 
                          AND o.Status = 'Enable' 
                          AND o.Fee > 0
                          AND (
                              o.OtherChargeItem = s.ServiceName 
                              OR (o.ServiceID = s.ServiceID AND s.ServiceID NOT IN (1, 8, 20))
                          )
                        ORDER BY 
                            CASE WHEN o.OtherChargeItem = s.ServiceName THEN 0 ELSE 1 END,
                            CASE WHEN o.OtherChargeItem = 'Public IP' THEN 0 ELSE 1 END,
                            o.Fee ASC
                    ) c
                    WHERE s.SystemID = 13

                    UNION ALL

                    SELECT 
                        ISNULL(o.ServiceID, 8) AS ServiceID,
                        o.OtherChargeItem AS ServiceName,
                        ISNULL(o.Fee, 0) AS Price,
                        ISNULL(NULLIF(o.UM, ''), 'Month') AS UM,
                        o.OtherChargeID,
                        'Other Charges' AS TypeGroup,
                        10 AS SortOrder
                    FROM dbo.TblOtherCharge o
                    WHERE o.SystemID = 13 
                      AND o.Status = 'Enable' 
                      AND o.Fee > 0
                      AND o.OtherChargeItem NOT IN (SELECT ServiceName FROM dbo.TblServices WHERE SystemID = 13)
                      AND o.OtherChargeItem NOT LIKE '%Note:%'
                      AND o.OtherChargeItem NOT LIKE '%Lending Router%'
                      AND o.OtherChargeItem NOT LIKE '%Phase%'
                      AND o.OtherChargeItem NOT LIKE '%VAT%'
                      AND o.OtherChargeItem NOT IN ('Free', 'Cancel', 'Item')
                ) AS tbl
                ORDER BY 
                    CASE WHEN TypeGroup = 'Services' THEN 1 ELSE 2 END,
                    SortOrder,
                    Price DESC,
                    ServiceName;";
            DataSet ds = html.LoadData(sql);
            var list = new List<Dictionary<string, object>>();
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in ds.Tables[0].Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    list.Add(dict);
                }
            }
            return Content(JsonConvert.SerializeObject(new { data = list }), "application/json");
        }

        private void ApplyExtraRowsToInvoice(DataSet ds, string extraRowsJson)
        {
            if (ds == null || ds.Tables.Count <= 2 || string.IsNullOrWhiteSpace(extraRowsJson))
                return;

            try
            {
                var extraRows = JsonConvert.DeserializeObject<List<ExtraServiceRowDTO>>(extraRowsJson);
                if (extraRows != null && extraRows.Count > 0)
                {
                    int orderIdx = 2;
                    foreach (var item in extraRows)
                    {
                        if (string.IsNullOrWhiteSpace(item.Description) && item.Amount == 0)
                            continue;

                        DataRow newRow = ds.Tables[2].NewRow();
                        if (ds.Tables[2].Columns.Contains("Description")) newRow["Description"] = item.Description ?? "";
                        if (ds.Tables[2].Columns.Contains("UM")) newRow["UM"] = item.UM ?? "";
                        if (ds.Tables[2].Columns.Contains("QTY")) newRow["QTY"] = item.QTY > 0 ? item.QTY : 1;
                        if (ds.Tables[2].Columns.Contains("UnitPrice")) newRow["UnitPrice"] = item.UnitPrice;
                        if (ds.Tables[2].Columns.Contains("DiscountUSD")) newRow["DiscountUSD"] = item.DiscountUSD;
                        if (ds.Tables[2].Columns.Contains("Amount")) newRow["Amount"] = item.Amount;
                        
                        int sId = item.ServiceID > 0 ? item.ServiceID : 8;
                        if (sId == 8 && (item.Description ?? "").IndexOf("Public IP", StringComparison.OrdinalIgnoreCase) >= 0) sId = 3;
                        else if (sId == 8 && (item.Description ?? "").IndexOf("Maintenance", StringComparison.OrdinalIgnoreCase) >= 0) sId = 5;
                        else if (sId == 8 && (item.Description ?? "").IndexOf("Services", StringComparison.OrdinalIgnoreCase) >= 0) sId = 6;
                        if (ds.Tables[2].Columns.Contains("ServiceID")) newRow["ServiceID"] = sId;

                        if (ds.Tables[2].Columns.Contains("OtherChargeID"))
                        {
                            int ocId = item.OtherChargeID;
                            if (ocId == 0 && (item.Description ?? "").IndexOf("Public IP", StringComparison.OrdinalIgnoreCase) >= 0) ocId = 1089;
                            newRow["OtherChargeID"] = ocId;
                        }
                        if (ds.Tables[2].Columns.Contains("OrderBy")) newRow["OrderBy"] = orderIdx++;
                        ds.Tables[2].Rows.Add(newRow);
                    }
                    ds.Tables[2].AcceptChanges();
                }
            }
            catch { }
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> SaveNotificationPayment(
            int CustomerID, string CustomerName, string CustomerCode, string InvoiceNo,
            string InvoiceDate, string InvoiceDueDate, string StartDate,
            int ApprovedByUserID, string ApprovedBy, string ApprovedByTelegramUserName, string EndDate = "",
            string PackageName = "", decimal? UnitPrice = null, int? Duration = null, string UM = "",
            string Feature = "", decimal? ExchangeRate = null, bool HideSignature = false,
            int CheckedByUserID = 0, string CheckedBy = "", string CheckedByTelegramUserName = "",
            string ReviseNPNo = "", string ExtraRowsJson = "")
        {
            HTML html = new HTML();
            CustomerName = HttpUtility.UrlDecode(CustomerName ?? "");
            ApprovedBy = HttpUtility.UrlDecode(ApprovedBy ?? "");
            ApprovedByTelegramUserName = (ApprovedByTelegramUserName ?? "").Trim();
            CheckedBy = HttpUtility.UrlDecode(CheckedBy ?? "");
            CheckedByTelegramUserName = (CheckedByTelegramUserName ?? "").Trim();

            try
            {
                // Identify Requester (logged-in user)
                int requestByUserID = 0;
                int.TryParse(html.UserID, out requestByUserID);
                string requestBy = !string.IsNullOrEmpty(html.UserFullName) ? html.UserFullName : html.UserName;
                string requestByTgUser = "";

                if (requestByUserID > 0 || !string.IsNullOrEmpty(html.UserName))
                {
                    string sqlUser = "SELECT TOP 1 UserLoginID, FullName, TelegramUserName FROM dbo.TblUserLogin WHERE UserLoginID = " + requestByUserID + " OR UserName = '" + html.UserName.Replace("'", "''") + "'";
                    var dtUser = html.LoadData(sqlUser).Tables[0];
                    if (dtUser.Rows.Count > 0)
                    {
                        requestByUserID = Convert.ToInt32(dtUser.Rows[0]["UserLoginID"]);
                        requestBy = dtUser.Rows[0]["FullName"].ToString();
                        requestByTgUser = dtUser.Rows[0]["TelegramUserName"]?.ToString() ?? "";
                    }
                }

                bool isRevise = !string.IsNullOrEmpty(ReviseNPNo);
                string npNo = "";
                int notificationPaymentID = 0;
                int oldCheckTelegramMessageId = 0;

                if (isRevise)
                {
                    npNo = ReviseNPNo.Trim();
                    string findSql = @"
                        SELECT TOP 1 NotificationPaymentID, CheckTelegramMessageID, TelegramGroupID, TelegramGroupTopicID, CustomerID, CustomerName, InvoiceNo
                        FROM dbo.TblNotificationPayment
                        WHERE NPNo = @ReviseNPNo 
                          AND StatusID = 60
                          AND (SystemID = " + html.SystemID + @" OR " + html.SystemID + @" = 0 OR SystemID IS NULL)
                          AND (WarehouseID = " + html.WarehouseID + @" OR " + html.WarehouseID + @" = 0 OR WarehouseID IS NULL)
                        ORDER BY NotificationPaymentID DESC;";
                    var dtRevise = html.LoadData(findSql, false, new Dictionary<string, string> {
                        { "@ReviseNPNo", npNo }
                    }).Tables[0];

                    if (dtRevise.Rows.Count > 0)
                    {
                        notificationPaymentID = Convert.ToInt32(dtRevise.Rows[0]["NotificationPaymentID"]);
                        oldCheckTelegramMessageId = Utility.TryParse(dtRevise.Rows[0]["CheckTelegramMessageID"]?.ToString(), "int", 0);

                        // Update existing record (re-assigning to current Customer and Invoice, resetting check status so it gets checked again)
                        string updateSql = @"
                            UPDATE dbo.TblNotificationPayment
                            SET CustomerID = @CustomerID,
                                CustomerName = @CustomerName,
                                InvoiceNo = @InvoiceNo,
                                RequestByUserID = @RequestByUserID,
                                RequestBy = @RequestBy,
                                RequestByTgUser = @RequestByTgUser,
                                CheckByUserID = @CheckByUserID,
                                CheckBy = @CheckBy,
                                CheckByTgUser = @CheckByTgUser,
                                CheckStatusID = 60,
                                CheckDate = NULL,
                                ApproveByUserID = @ApproveByUserID,
                                ApproveBy = @ApproveBy,
                                ApproveByTgUser = @ApproveByTgUser,
                                StatusID = 60,
                                ApproveDate = NULL,
                                CreateDate = GETDATE()
                            WHERE NotificationPaymentID = @NotificationPaymentID;";
                        var paraUpdate = new Dictionary<string, string> {
                            { "@NotificationPaymentID", notificationPaymentID.ToString() },
                            { "@CustomerID", CustomerID.ToString() },
                            { "@CustomerName", CustomerName ?? "" },
                            { "@InvoiceNo", InvoiceNo ?? "" },
                            { "@RequestByUserID", requestByUserID.ToString() },
                            { "@RequestBy", requestBy ?? "" },
                            { "@RequestByTgUser", requestByTgUser ?? "" },
                            { "@CheckByUserID", CheckedByUserID.ToString() },
                            { "@CheckBy", CheckedBy ?? "" },
                            { "@CheckByTgUser", CheckedByTelegramUserName ?? "" },
                            { "@ApproveByUserID", ApprovedByUserID.ToString() },
                            { "@ApproveBy", ApprovedBy ?? "" },
                            { "@ApproveByTgUser", ApprovedByTelegramUserName ?? "" }
                        };
                        html.SaveChangeData(updateSql, false, paraUpdate);
                    }
                    else
                    {
                        return Json(new { Result = ResultType.Failed, Message = $"Notification Payment {npNo} has already been approved and cannot be revised." }, JsonRequestBehavior.AllowGet);
                    }
                }

                if (!isRevise)
                {
                    // Generate NPNo from TblPrefix
                    try
                    {
                        string prefixSql = @"
                            DECLARE @Prefix nvarchar(100), @Module nvarchar(50);
                            SELECT TOP 1 @Prefix = Prefix, @Module = Module 
                            FROM dbo.TblPrefix 
                            WHERE (Module = 'NotificationPayment' OR Module = 'NP' OR Prefix LIKE 'NP%')
                              AND SystemID = " + html.SystemID + @" AND WarehouseID = " + html.WarehouseID + @"
                            ORDER BY CASE WHEN Module = 'NotificationPayment' THEN 0 ELSE 1 END, PrefixID DESC;

                            IF @Prefix IS NOT NULL
                                EXEC [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + @", " + html.WarehouseID + @", '" + html.DomainID + @"';
                        ";
                        DataSet dsPrefix = html.LoadData(prefixSql, false, null);
                        if (dsPrefix != null && dsPrefix.Tables.Count > 0 && dsPrefix.Tables[0].Rows.Count > 0)
                        {
                            if (dsPrefix.Tables[0].Columns.Contains("PrefixNumber"))
                                npNo = dsPrefix.Tables[0].Rows[0]["PrefixNumber"].ToString();
                            else
                                npNo = dsPrefix.Tables[0].Rows[0][0].ToString();
                        }
                    }
                    catch { }

                    if (string.IsNullOrEmpty(npNo))
                    {
                        string getPrefix = @"
                            SELECT TOP 1 PrefixID, Prefix, ISNULL(CurrentNumber, 0) + 1 AS NextNum
                            FROM dbo.TblPrefix 
                            WHERE (Module = 'NotificationPayment' OR Module = 'NP' OR Prefix LIKE 'NP%')
                              AND SystemID = " + html.SystemID + @" AND WarehouseID = " + html.WarehouseID + @"
                            ORDER BY CASE WHEN Module = 'NotificationPayment' THEN 0 ELSE 1 END, PrefixID DESC;";
                        var dtP = html.LoadData(getPrefix).Tables[0];
                        if (dtP.Rows.Count > 0)
                        {
                            int pId = Convert.ToInt32(dtP.Rows[0]["PrefixID"]);
                            int nextVal = Convert.ToInt32(dtP.Rows[0]["NextNum"]);
                            string pfx = dtP.Rows[0]["Prefix"].ToString();
                            if (pfx.Contains("{") && pfx.Contains("}"))
                            {
                                npNo = pfx.Replace("{yy}", DateTime.Now.ToString("yy"))
                                          .Replace("{MM}", DateTime.Now.ToString("MM"))
                                          .Replace("{dd}", DateTime.Now.ToString("dd"))
                                          .Replace("{000000}", nextVal.ToString("D6"))
                                          .Replace("{00000}", nextVal.ToString("D5"))
                                          .Replace("{0000}", nextVal.ToString("D4"))
                                          .Replace("{000}", nextVal.ToString("D3"));
                            }
                            else
                            {
                                npNo = $"{pfx}-{DateTime.Now:yyMM}-{nextVal:D5}";
                            }

                            string updatePrefix = "UPDATE dbo.TblPrefix SET CurrentNumber = " + nextVal + ", CurrentPrefix = '" + npNo + "' WHERE PrefixID = " + pId;
                            html.SaveChangeData(updatePrefix, false, null);
                        }
                        else
                        {
                            npNo = $"NP-{DateTime.Now:yyMM}-{new Random().Next(1000, 9999)}";
                        }
                    }

                    // Increment CurrentNumber and update CurrentPrefix in dbo.TblPrefix
                    try
                    {
                        string updatePrefixSql = @"
                            UPDATE dbo.TblPrefix 
                            SET CurrentNumber = ISNULL(CurrentNumber, 0) + 1,
                                CurrentPrefix = '" + npNo.Replace("'", "''") + @"'
                            WHERE (Module = 'NotificationPayment' OR Module = 'NP' OR Prefix LIKE 'NP%')
                              AND SystemID = " + html.SystemID + @" AND WarehouseID = " + html.WarehouseID;
                        html.SaveChangeData(updatePrefixSql, false, null);
                    }
                    catch { }

                    // Insert record into dbo.TblNotificationPayment with StatusID = 60 (Pending) and CheckStatusID = 60 (Pending Check)
                    string insertSql = @"
                        INSERT INTO dbo.TblNotificationPayment (
                            NPNo, InvoiceNo, CustomerID, CustomerName,
                            SystemID, WarehouseID, DomainID,
                            RequestByUserID, RequestBy, RequestByTgUser, CreateDate,
                            CheckByUserID, CheckBy, CheckByTgUser, CheckStatusID,
                            ApproveByUserID, ApproveBy, ApproveByTgUser,
                            StatusID
                        ) VALUES (
                            @NPNo, @InvoiceNo, @CustomerID, @CustomerName,
                            @SystemID, @WarehouseID, @DomainID,
                            @RequestByUserID, @RequestBy, @RequestByTgUser, GETDATE(),
                            @CheckByUserID, @CheckBy, @CheckByTgUser, 60,
                            @ApproveByUserID, @ApproveBy, @ApproveByTgUser,
                            60
                        );
                        SELECT CAST(SCOPE_IDENTITY() AS INT);
                    ";
                    var paraInsert = new Dictionary<string, string>
                    {
                        { "@NPNo", npNo },
                        { "@InvoiceNo", InvoiceNo ?? "" },
                        { "@CustomerID", CustomerID.ToString() },
                        { "@CustomerName", CustomerName ?? "" },
                        { "@SystemID", html.SystemID },
                        { "@WarehouseID", html.WarehouseID },
                        { "@DomainID", html.DomainID },
                        { "@RequestByUserID", requestByUserID.ToString() },
                        { "@RequestBy", requestBy ?? "" },
                        { "@RequestByTgUser", requestByTgUser ?? "" },
                        { "@CheckByUserID", CheckedByUserID.ToString() },
                        { "@CheckBy", CheckedBy ?? "" },
                        { "@CheckByTgUser", CheckedByTelegramUserName ?? "" },
                        { "@ApproveByUserID", ApprovedByUserID.ToString() },
                        { "@ApproveBy", ApprovedBy ?? "" },
                        { "@ApproveByTgUser", ApprovedByTelegramUserName ?? "" }
                    };
                    var dsInsert = html.LoadData(insertSql, false, paraInsert);
                    if (dsInsert != null && dsInsert.Tables.Count > 0 && dsInsert.Tables[0].Rows.Count > 0)
                    {
                        notificationPaymentID = Convert.ToInt32(dsInsert.Tables[0].Rows[0][0]);
                    }
                }

                // 4. Render Invoice PDF using SP_NOTIFICATION_PAYMENT
                Dictionary<string, string> paraSP = new Dictionary<string, string> {
                    { "CustID", CustomerID.ToString() },
                    { "InvoiceNo", InvoiceNo },
                    { "StartDate", StartDate },
                    { "EndDate", EndDate },
                    { "SystemID", html.SystemID },
                    { "WarehouseID", html.WarehouseID },
                    { "DomainID", html.DomainID },
                };
                DataSet dsInv = html.LoadData("SP_NOTIFICATION_PAYMENT", true, paraSP);

                // Filter Table 2 rows for Notification Payment
                if (dsInv.Tables.Count > 2 && dsInv.Tables[2].Rows.Count > 0)
                {
                    DataRow primaryRow = null;
                    foreach (DataRow r in dsInv.Tables[2].Rows)
                    {
                        if (r.RowState != DataRowState.Deleted)
                        {
                            string desc = dsInv.Tables[2].Columns.Contains("Description") ? r["Description"]?.ToString()?.ToLower() ?? "" : "";
                            if (dsInv.Tables[2].Columns.Contains("ServiceID") && r["ServiceID"]?.ToString() == "1")
                            {
                                primaryRow = r;
                                break;
                            }
                            if (primaryRow == null && !desc.Contains("router") && !desc.Contains("deposit") && !desc.Contains("installation"))
                            {
                                primaryRow = r;
                            }
                        }
                    }

                    if (primaryRow == null)
                        primaryRow = dsInv.Tables[2].Rows[0];

                    if (!string.IsNullOrEmpty(ExtraRowsJson))
                    {
                        for (int ri = dsInv.Tables[2].Rows.Count - 1; ri >= 0; ri--)
                        {
                            if (dsInv.Tables[2].Rows[ri] != primaryRow)
                            {
                                dsInv.Tables[2].Rows[ri].Delete();
                            }
                        }
                        dsInv.Tables[2].AcceptChanges();
                        ApplyExtraRowsToInvoice(dsInv, ExtraRowsJson);
                    }
                    else
                    {
                        for (int ri = dsInv.Tables[2].Rows.Count - 1; ri >= 0; ri--)
                        {
                            DataRow r = dsInv.Tables[2].Rows[ri];
                            if (r != primaryRow)
                            {
                                string desc = dsInv.Tables[2].Columns.Contains("Description") ? r["Description"]?.ToString()?.ToLower() ?? "" : "";
                                if (desc.Contains("router") || desc.Contains("deposit") || desc.Contains("installation"))
                                {
                                    r.Delete();
                                }
                            }
                        }
                        dsInv.Tables[2].AcceptChanges();
                    }
                }
                else if (!string.IsNullOrEmpty(ExtraRowsJson))
                {
                    ApplyExtraRowsToInvoice(dsInv, ExtraRowsJson);
                }

                // ✅ Apply changed package/pricing if specified
                ApplyPackageChangesToInvoice(dsInv, PackageName, UnitPrice, Duration, UM, Feature, ExchangeRate, StartDate, EndDate);

                if (dsInv.Tables.Count > 2 && dsInv.Tables.Count > 5 && dsInv.Tables[5].Rows.Count > 0)
                {
                    decimal newSubTotal = 0;
                    foreach (DataRow r in dsInv.Tables[2].Rows)
                    {
                        if (r.RowState != DataRowState.Deleted && dsInv.Tables[2].Columns.Contains("Amount"))
                        {
                            decimal amt = 0;
                            if (decimal.TryParse(r["Amount"].ToString(), out amt))
                                newSubTotal += amt;
                        }
                    }
                    dsInv.Tables[5].Rows[0]["SubTotal"] = newSubTotal;

                    decimal vat = 0;
                    if (dsInv.Tables[5].Columns.Contains("VAT") && dsInv.Tables[5].Rows[0]["VAT"].ToString() != "0.00")
                    {
                        vat = Math.Round(newSubTotal * 0.10m, 2);
                        dsInv.Tables[5].Rows[0]["VAT"] = vat.ToString("0.00");
                    }

                    decimal discount = 0;
                    if (dsInv.Tables[5].Columns.Contains("DiscountUSD"))
                    {
                        decimal.TryParse(dsInv.Tables[5].Rows[0]["DiscountUSD"].ToString(), out discount);
                    }

                    decimal grandTotal = newSubTotal + vat - discount;
                    if (dsInv.Tables[5].Columns.Contains("GrandTotal"))
                    {
                        dsInv.Tables[5].Rows[0]["GrandTotal"] = grandTotal;
                    }
                }

                // ✅ Override signature, IssuedBy, and IssuedDate with current logged-in user
                if (dsInv.Tables.Count > 8 && dsInv.Tables[8].Rows.Count > 0)
                {
                    string sqlSign = "SELECT Signature, FullName FROM dbo.TblUserLogin WHERE UserLoginID = " + html.UserID;
                    var dtSign = html.LoadData(sqlSign).Tables[0];
                    if (dtSign.Rows.Count > 0)
                    {
                        string fullName = dtSign.Rows[0]["FullName"].ToString();

                        if (dtSign.Rows[0]["Signature"] != DBNull.Value && dsInv.Tables[8].Columns.Contains("Signature"))
                        {
                            dsInv.Tables[8].Rows[0]["Signature"] = dtSign.Rows[0]["Signature"].ToString();
                        }

                        // Force replace Name and Date in all tables just in case
                        foreach (DataTable table in dsInv.Tables)
                        {
                            if (table.Rows.Count > 0)
                            {
                                if (table.Columns.Contains("UserLoginID")) table.Rows[0]["UserLoginID"] = html.UserID;
                                if (table.Columns.Contains("UserID")) table.Rows[0]["UserID"] = html.UserID;
                                if (table.Columns.Contains("CreateBy")) table.Rows[0]["CreateBy"] = fullName;

                                if (table.Columns.Contains("IssuedBy")) table.Rows[0]["IssuedBy"] = fullName;
                                if (table.Columns.Contains("IssuedByName")) table.Rows[0]["IssuedByName"] = fullName;
                                if (table.Columns.Contains("IssuedDate")) table.Rows[0]["IssuedDate"] = DateTime.Now.ToString("dd/MM/yyyy");
                                if (table.Columns.Contains("IssuedByDate")) table.Rows[0]["IssuedByDate"] = DateTime.Now.ToString("dd/MM/yyyy");
                                if (table.Columns.Contains("FullName")) table.Rows[0]["FullName"] = fullName;
                            }
                        }
                    }
                }

                // ✅ Set 'ប្រាក់កក់ / Deposit' to default $0.00 by overriding the 'Paid' column in Table 5
                if (dsInv.Tables.Count > 5 && dsInv.Tables[5].Rows.Count > 0)
                {
                    if (dsInv.Tables[5].Columns.Contains("Paid"))
                    {
                        dsInv.Tables[5].Rows[0]["Paid"] = 0;
                    }
                }

                string invType = "";
                if (dsInv != null && dsInv.Tables.Count > 1 && dsInv.Tables[1].Rows.Count > 0 && dsInv.Tables[1].Columns.Contains("TypeOfInvoice"))
                {
                    invType = dsInv.Tables[1].Rows[0]["TypeOfInvoice"]?.ToString()?.Trim() ?? "";
                }
                else if (dsInv != null && dsInv.Tables.Count > 0 && dsInv.Tables[0].Rows.Count > 0 && dsInv.Tables[0].Columns.Contains("TypeOfInvoice"))
                {
                    invType = dsInv.Tables[0].Rows[0]["TypeOfInvoice"]?.ToString()?.Trim() ?? "";
                }

                bool isVAT = invType.Equals("VAT", StringComparison.OrdinalIgnoreCase) || invType.Equals("CM", StringComparison.OrdinalIgnoreCase);

                // ✅ Override TermCondition only for VAT & CM from Default Company in SystemID = 13 (ISDefault = 1)
                if (isVAT)
                {
                    try
                    {
                        string sqlActiveTerm = "SELECT TOP 1 TermConditionVAT FROM dbo.TblCompany WHERE SystemID = 13 AND ISDefault = 1";
                        var dtActiveTerm = html.LoadData(sqlActiveTerm).Tables[0];
                        if (dtActiveTerm.Rows.Count > 0)
                        {
                            string termCondition = dtActiveTerm.Rows[0]["TermConditionVAT"]?.ToString();
                            if (!string.IsNullOrEmpty(termCondition))
                            {
                                termCondition = termCondition.Replace("&#013;&#010;", "\r\n").Replace("&#013;", "\r").Replace("&#010;", "\n");
                                if (dsInv.Tables.Count > 1 && dsInv.Tables[1].Rows.Count > 0 && dsInv.Tables[1].Columns.Contains("TermCondition"))
                                    dsInv.Tables[1].Rows[0]["TermCondition"] = termCondition;
                                if (dsInv.Tables.Count > 0 && dsInv.Tables[0].Rows.Count > 0 && dsInv.Tables[0].Columns.Contains("TermCondition"))
                                    dsInv.Tables[0].Rows[0]["TermCondition"] = termCondition;
                            }
                        }
                    }
                    catch { }
                }
                else
                {
                    if (dsInv.Tables.Count > 1 && dsInv.Tables[1].Rows.Count > 0 && dsInv.Tables[1].Columns.Contains("TermCondition"))
                        dsInv.Tables[1].Rows[0]["TermCondition"] = "";
                    if (dsInv.Tables.Count > 0 && dsInv.Tables[0].Rows.Count > 0 && dsInv.Tables[0].Columns.Contains("TermCondition"))
                        dsInv.Tables[0].Rows[0]["TermCondition"] = "";
                }

                decimal exchangeRate = 4200;
                if (isVAT)
                {
                    string sqlEx = "select top 1 format(e.ExchangeRate, '#,#') ExchangeRate from dbo.TblExchangeRate e inner join dbo.TblCurrency c on c.CurrencyID = e.CurrencyID where c.CurrencyName = 'Riel' and e.SystemID = " + html.SystemID + " and e.WarehouseID = " + html.WarehouseID + " and e.DomainID like '%" + html.DomainID + "%' order by format(e.DateEntry, 'yyyyMMdd') desc";
                    var dtExchangeRate = html.LoadData(sqlEx).Tables[0];
                    if (dtExchangeRate.Rows.Count > 0)
                    {
                        string exchangeRateStr = dtExchangeRate.Rows[0]["ExchangeRate"].ToString();
                        exchangeRate = Convert.ToDecimal(exchangeRateStr.Replace(",", ""));
                    }
                }
                else
                {
                    exchangeRate = 4200;
                }

                if (ExchangeRate.HasValue && ExchangeRate.Value > 0)
                {
                    exchangeRate = ExchangeRate.Value;
                }

                if (dsInv != null && dsInv.Tables.Count > 1 && dsInv.Tables[1].Rows.Count > 0)
                {
                    if (dsInv.Tables[1].Columns.Contains("InvoiceNo"))
                        dsInv.Tables[1].Rows[0]["InvoiceNo"] = npNo;
                    if (dsInv.Tables[1].Columns.Contains("ExchangeRate"))
                        dsInv.Tables[1].Rows[0]["ExchangeRate"] = exchangeRate;
                }

                string invHtml = html.DrawInvoice(dsInv, false, (int)exchangeRate, 0, false, true, true, InvoiceDate, InvoiceDueDate);
                if (HideSignature)
                {
                    invHtml = invHtml.Replace("id=\"Signature\"", "id=\"Signature\" style=\"visibility:hidden;\"");
                }
                if (!invHtml.Contains(npNo))
                {
                    string invNoRow = "<tr><td class=\"text-nowrap\">លេខរៀង/Notification Payment No.៖ </td><td class=\"text-nowrap\">" + npNo + "</td></tr>\n";
                    if (invHtml.Contains("<tr><td>កាលបរិច្ឆេទ/Date"))
                    {
                        invHtml = invHtml.Replace("<tr><td>កាលបរិច្ឆេទ/Date", invNoRow + "<tr><td>កាលបរិច្ឆេទ/Date");
                    }
                    else
                    {
                        var regex = new System.Text.RegularExpressions.Regex(@"(<tr>\s*<td[^>]*>[^<]*Date[^<]*</td>)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        invHtml = regex.Replace(invHtml, invNoRow + "$1", 1);
                    }
                }

                invHtml = invHtml.Replace("សរុបរួមទាំងអាករ / Total Included VAT 10%:", "សរុបទឹកប្រាក់រួម / Grand Total USD:");
                invHtml = invHtml.Replace("Total Included VAT", "Grand Total USD");
                invHtml = invHtml.Replace("សរុបរួមទាំងអាករ", "សរុបទឹកប្រាក់រួម");
                invHtml = invHtml.Replace("Discount %", "Discount $");
                invHtml = invHtml.Replace("Sub Total", "Total");
                invHtml = invHtml.Replace("សរុបរួមទាំងអាករជាលុយរៀល", "សរុបទឹកប្រាក់រួមជាលុយរៀល");
                invHtml = invHtml.Replace(">រ", ">&#x17DB;");
                invHtml = invHtml.Replace(" រ", " &#x17DB;");
                invHtml = invHtml.Replace("ស&#x17DB;ុប", "សរុប");
                invHtml = invHtml.Replace("អាក&#x17DB;", "អាករ");
                invHtml = invHtml.Replace("ប្តូ&#x17DB;", "ប្តូរ");
                invHtml = invHtml.Replace("ប្ដូ&#x17DB;", "ប្ដូរ");
                invHtml = invHtml.Replace(" &#x17DB;ាជធានី", " រាជធានី");
                invHtml = invHtml.Replace(">&#x17DB;ាជធានី", ">រាជធានី");
                invHtml = invHtml.Replace("លេខរៀងវិក្កយបត្រ/Invoice No.៖", "លេខរៀង/Notification Payment No.៖");
                invHtml = invHtml.Replace("លេខរៀងវិក្កយបត្រ/Invoice No. ", "លេខរៀង/Notification Payment No.៖ ");
                invHtml = invHtml.Replace("លេខរៀងវិក្កយបត្រ/Invoice No.", "លេខរៀង/Notification Payment No.៖");

                if (!isVAT)
                {
                    invHtml = System.Text.RegularExpressions.Regex.Replace(invHtml, @"<tr>\s*<td[^>]*>[^<]*Sale's Name[^<]*</td>.*?</tr>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                    invHtml = System.Text.RegularExpressions.Regex.Replace(invHtml, @"<tr>\s*<td[^>]*>[^<]*Sale's Phone[^<]*</td>.*?</tr>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                }

                // ✅ Remove Note from invoice Vat and Cm (សំគាល់៖ ច្បាប់ដើមសម្រាប់អ្នកទិញ ច្បាប់ចម្លងសម្រាប់អ្នកលក់ / Note: Original invoice for customer and copied invoice for seller)
                invHtml = System.Text.RegularExpressions.Regex.Replace(invHtml, @"<div class=""row"">\s*<div class=""col text-sm"">.*?</div>\s*</div>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                invHtml = System.Text.RegularExpressions.Regex.Replace(invHtml, @"<div class=""row"">\s*<div class=""col"">\s*<label[^>]*>(?:សំគាល់|សម្គាល់|Original invoice).*?</label>.*?</div>\s*</div>\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

                invHtml = System.Text.RegularExpressions.Regex.Replace(invHtml, @"(?:&#x17DB;|៛)(?=[\u1780-\u17DA\u17DC-\u17FF])", "រ");

                string FilePath = HttpContext.Server.MapPath("~/Templates/HtmlTemplate.html");
                StreamReader str = new StreamReader(FilePath);
                string Text = str.ReadToEnd();
                str.Close();
                Text = Text.Replace("{Title}", "Notification Payment");
                Text = Text.Replace("{BodyMessage}", invHtml);
                var pdfResult = await html.HTMLToPDF(Text.ToString(), "/Uploads/CustomerAttFiles/" + CustomerName + "-" + CustomerCode, "Notification Payment - " + npNo);

                // 5. Query Telegram Group Notification (TID 2039)
                string sqlTG = @"
                    SELECT TOP 1 t.TID, t.BotID, t.TelegramGroupID, t.TelegramGroupName, t.Module, 
                                 t.TelegramGroupType, t.TelegramGroupTopicID, t.SystemID, t.WarehouseID, b.BotToken
                    FROM dbo.TblTelegramGroupNotification t
                    LEFT JOIN dbo.TblTelegramBots b ON b.BotID = t.BotID
                    WHERE t.TID = 2039";
                var dtTG = html.LoadData(sqlTG).Tables[0];
                if (dtTG.Rows.Count == 0)
                {
                    sqlTG = @"
                        SELECT TOP 1 t.TID, t.BotID, t.TelegramGroupID, t.TelegramGroupName, t.Module, 
                                     t.TelegramGroupType, t.TelegramGroupTopicID, t.SystemID, t.WarehouseID, b.BotToken
                        FROM dbo.TblTelegramGroupNotification t
                        LEFT JOIN dbo.TblTelegramBots b ON b.BotID = t.BotID
                        WHERE (t.Module = 'NotificationPayment' OR t.Module = 'CustomerProfile') AND t.SystemID = " + html.SystemID;
                    dtTG = html.LoadData(sqlTG).Tables[0];
                }

                if (dtTG.Rows.Count > 0)
                {
                    long tgChatId = Convert.ToInt64(dtTG.Rows[0]["TelegramGroupID"]);
                    int tgTopicId = 0;
                    if (dtTG.Rows[0]["TelegramGroupTopicID"] != DBNull.Value)
                        int.TryParse(dtTG.Rows[0]["TelegramGroupTopicID"].ToString(), out tgTopicId);
                    string botToken = dtTG.Rows[0]["BotToken"]?.ToString() ?? "";

                    string cleanCheckTg = CheckedByTelegramUserName;
                    if (!string.IsNullOrEmpty(cleanCheckTg) && !cleanCheckTg.StartsWith("@"))
                        cleanCheckTg = "@" + cleanCheckTg;

                    var sbMsg = new StringBuilder();
                    if (!string.IsNullOrEmpty(cleanCheckTg))
                        sbMsg.AppendLine(cleanCheckTg);
                    if (isRevise)
                        sbMsg.AppendLine("Now, you have Notification Payment need to check\n");
                    else
                        sbMsg.AppendLine("Now, you have Notification Payment need to check\n");
                    sbMsg.AppendLine($"Request by: {requestBy}\n");
                    sbMsg.AppendLine($"NP No.: {npNo}");
                    sbMsg.AppendLine("Please kindly review and take action, Thanks you.");
                    string caption = sbMsg.ToString();

                    var inlineKeyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Check ✍️", $"CheckNP~{notificationPaymentID}~{cleanCheckTg}"),
                            Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Reject ❌", $"RejectNP~{notificationPaymentID}~{cleanCheckTg}")
                        }
                    });

                    // Best-effort delete old Telegram message if revising (isolated so errors never block sending the new message)
                    if (oldCheckTelegramMessageId > 0 && !string.IsNullOrEmpty(botToken))
                    {
                        try
                        {
                            var botClient = new Telegram.Bot.TelegramBotClient(botToken);
                            await botClient.DeleteMessage(tgChatId, oldCheckTelegramMessageId);
                        }
                        catch
                        {
                            // Ignore deletion errors (e.g. message > 48h old, already deleted, or missing delete permission)
                        }
                    }

                    NotificationTelegram notification = new NotificationTelegram();
                    SqlResult sent = null;
                    if (tgTopicId > 0)
                    {
                        sent = await notification.NotificationTelegramBotToTopic(
                            MsgText: "",
                            chatId: tgChatId,
                            topicID: tgTopicId,
                            inlineKeyboard: inlineKeyboard,
                            MessageID: 0,
                            TelegramMessage: caption,
                            Files: pdfResult.OutPutFile,
                            Caption: caption,
                            EditMessage: false,
                            DeleteTelegramMessageID: "",
                            IsReplyMsg: false,
                            newBotClient: botToken
                        );
                    }
                    else
                    {
                        sent = await notification.NotificationTelegramBot(
                            MsgText: "",
                            chatId: tgChatId,
                            DestinationFolder: "",
                            PDFFile: pdfResult.OutPutFile,
                            Caption: caption,
                            IsSendFileWithCaption: false,
                            replyMarkupInlineKeyboardMarkup: true,
                            DBID: notificationPaymentID,
                            MessageID: 0,
                            TelegramMessage: caption,
                            inlineKeyboard: inlineKeyboard,
                            edit: false,
                            messageThreadId: 0,
                            DeleteMessageID: "",
                            newBotClient: botToken
                        );
                    }

                    // 6. Update dbo.TblNotificationPayment with Telegram info
                    int telegramMessageId = Utility.TryParse(sent?.Value, "int", 0);
                    if (telegramMessageId > 0)
                    {
                        string updateSql = @"
                            UPDATE dbo.TblNotificationPayment
                            SET CheckTelegramMessageID = @CheckTelegramMessageID,
                                TelegramGroupID = @TelegramGroupID,
                                TelegramGroupTopicID = @TelegramGroupTopicID
                            WHERE NotificationPaymentID = @ID;
                        ";
                        var paraUpdate = new Dictionary<string, string>
                        {
                            { "@ID", notificationPaymentID.ToString() },
                            { "@CheckTelegramMessageID", telegramMessageId.ToString() },
                            { "@TelegramGroupID", tgChatId.ToString() },
                            { "@TelegramGroupTopicID", tgTopicId.ToString() }
                        };
                        html.SaveChangeData(updateSql, false, paraUpdate);
                    }
                }

                // 7. Retain PDF file for Step 2 (Approval) dispatch

                return Json(new { Result = ResultType.Successful, NPNo = npNo, Message = (isRevise ? $"Notification Payment {npNo} revised and updated to Telegram successfully!" : $"Notification Payment {npNo} saved and sent to Telegram successfully!") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = ResultType.Failed, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CustomerConnectionLog()
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over (order by ac.ConnectionID, c.CustomerName) as [No], 
ac.CustomerID, c.CustomerCode, c.CustomerName, ac.ConnectionID, ac.[SID], format(ac.ActivateDate, 'dd/MM/yyyy') ActivateDate,
format(ac.StartDate,'dd/MM/yyyy') StartDate, format(ac.EndDate, 'dd/MM/yyyy') EndDate, s.[Status]
from dbo.TblActivateCustomer ac
inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = ac.CustomerID
inner join dbo.TblClass ca on ca.ClassID = ci.ClassNameID
where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + @"%' 
group by ac.CustomerID, c.CustomerCode, c.CustomerName, ac.ConnectionID, ac.[SID], ac.ActivateDate,
ac.StartDate, ac.EndDate, s.[Status],ci.ClassNameID";

            html.IsUseHavingCondition = true;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Activate Date:",
                DBField = "ac.ActivateDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "TxtClassName",
                Label = "Class Name:",
                DBField = "ci.ClassNameID",
                SQL_CBO = @"select distinct ca.ClassID, ca.ClassName
from dbo.TblClass ca 
inner join dbo.TblCustomerInvoice ci on ci.ClassNameID = ca.ClassID where ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "ClassName",
                Value = "ClassID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select distinct c.CustomerName from dbo.TblActivateCustomer ac inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + "%' order by c.CustomerName",
                SelectValue = "CustomerName",
                DisplayField = "CustomerName",
                RadioValue = EncryAndDecry.Encryp("c.CustomerName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.SQL = sql;
            html.OrderBy = "ac.ConnectionID, c.CustomerName";
            html.ReportTitle = "Customer Connection Log";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField = "CustomerID", NewPage = true },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField = "CustomerID", NewPage = true },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Service ID", FieldName = "SID" },
                new GridField() { Header = "Activate Date", FieldName = "ActivateDate", },
                new GridField() { Header = "Start Date", FieldName = "StartDate", },
                new GridField() { Header = "End Date", FieldName = "EndDate", },
                new GridField() { Header = "Status", FieldName = "Status" },
            };
            //html.UsingJqueryDataTable = true;
            html.ShowPaging = false;
            //ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        public string ShowAgencyInfo(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select row_number() over(order by isnull(LastInvoiceDate, '') desc) as [No],
ac.AgencyCommissionID, ac.CustomerID, ac.FullName, ac.HP, 
(select STUFF((select (',' + agt.AgencyCommissionType)
from dbo.TblAgencyCommission acc
CROSS APPLY STRING_SPLIT(isnull(acc.CommissionType,''), ',')
inner join dbo.TblAgencyCommissionType agt on agt.AgencyCommissionTypeID = value
where acc.AgencyCommissionID = ac.AgencyCommissionID
FOR XML PATH('')), 1, 1, '')) CommissionType, 
ac.[Address], ac.BankName, ac.AccountName, ac.AccountNumber, ac.CommissionAmountUSD, 
ac.CommissionAmountP, ai.InvoiceNo, ai.InvoiceDate
from dbo.TblAgencyCommission ac
left join dbo.TblAgencyInvoice ai on ai.AgencyCommissionID = ac.AgencyCommissionID";
            html.GH.SQL = sql;
            html.TableName = "TblAgencyCommission";
            html.AllowEdit = true;
            html.AllowDelete = true;
            html.GH.GridPara = new GridPara[]
            {
                new GridPara { ParaName="ac.CustomerID", Value = CustomerID.ToString() }
            };
            html.AddURL = "CustomerProfile/EditAgencyInof";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo" },
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", FormatDateTime="dd-MM-yy", TextAlign = "center" },
                new GridField() { Header = "Name", FieldName = "FullName" },
                new GridField() { Header = "Phone", FieldName = "HP" },
                new GridField() { Header = "Commission Type", FieldName = "CommissionType" },
                new GridField() { Header = "Commission USD", FieldName = "CommissionAmountUSD", TextAlign = "right" },
                new GridField() { Header = "Commission %", FieldName = "CommissionAmountP", TextAlign = "right" },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Bank Name", FieldName = "BankName" },
                new GridField() { Header = "Account Name", FieldName = "AccountName" },
                new GridField() { Header = "Account Number", FieldName = "AccountNumber" },
                new GridField() { Header = "Action", FieldName = "AgencyCommissionID", TextAlign = "Center",  IsModifyField = true, DeleteWithTableName="TblAgencyInvoice" }
            };
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            //html.IsHaveWhereCondition = true;
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }

        public string EditAgencyInof(int PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                //string sql = @"select * from TblAgencyCommission where AgencyCommissionID = @AgencyCommissionID";

                FormID = "CreateAgencyCommission",
                FieldIDFocus = "TxtAFullName",
                TableName = "TblAgencyCommission",
                PrimaryField = "AgencyCommissionID",
                PrimaryFieldValue = PrimaryFieldValue.ToString()
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Name:",
                ID = "TxtAFullName",
                DBField = "FullName",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Phone:",
                ID = "TxtAHP",
                DBField = "HP",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Commission Type:",
                ID = "TxtACommissionType",
                DBField = "CommissionType",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Address:",
                ID = "TxtAAddress",
                DBField = "Address",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Bank Name:",
                ID = "TxtABankName",
                DBField = "BankName",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Account Name:",
                ID = "TxtAAccountName",
                DBField = "AccountName",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Account Number:",
                ID = "TxtAAccountNumber",
                DBField = "AccountNumber",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Memo:",
                ID = "TxtAMemo",
                DBField = "Memo",
            });
            return html.DrawHTMLForm();
        }

        [HttpPost]
        public ActionResult SetAutoLockOnOff(int CustomerID, string InvoiceNo, int AutoLock)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = @"update TblActivateCustomer set AutoLock = @AutoLock, SetAutoLockBy = @SetAutoLockBy, SetAutoLockDate = @SetAutoLockDate
where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "AutoLock", AutoLock.ToString() },
                { "SetAutoLockBy", html.UserFullName },
                { "SetAutoLockDate", html.GetCurrentDate() },
                { "CustomerID", CustomerID.ToString() },
                { "InvoiceNo", InvoiceNo },
            };
            sqlResult = html.SaveChangeData(sql, false, para);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

    }
}