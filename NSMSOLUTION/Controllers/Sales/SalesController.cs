using MVC;
using NewInventory.Controllers;
using NewInventorySystem.Controllers;
using Newtonsoft.Json;
using NSMSOLUTION.Controllers.Inventory;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers.Sales
{
    [Authorize]
    public class SalesController : Controller
    {
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
        [Authorize(Roles = "Add Customer Profile, Edit Customer Profile, Delete Customer Profile")]
        public ActionResult CustomerProfileInfo()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            html.ReportTitle = "List Customer Profile";
            if (html.HasPermission("Add Customer Profile"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Customer Profile"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Customer Profile"))
                html.AllowDelete = true;
            html.TableName = "TblCustomer";
            html.AddButton = "Add New";
            html.AddURL = "Sales/CreateCustomerProfileInfo/?ISSO=1";

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Create Date:",
                DBField = "c.RegisterDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "CustomerType",
                //ISAutoComplete = true,
                Label = "Customer Type:",
                DBField = "CustomerType",
                SQL_CBO = "select distinct CustomerType from dbo.TblCustomerType where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerType",
                DisplayText = "CustomerType",
                Value = "CustomerType",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Code",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
            });

            string sql = @"select ROW_NUMBER() Over(order by ct.CustomerType, c.CustomerCode desc) as [No], c.CustomerCode, '<a href=""javascript:void(0);"" onclick=""showCustomerProfile('+convert(varchar, c.CustomerID)+')"">'+c.CustomerName+'</a>' as CustomerName, ct.CustomerType, c.CustomerMobile, c.CustomerAddress,
c.CustomerGender, c.RegisterBy, c.CustomerID, format(c.RegisterDate, 'dd-MMM-yyyy') RegisterDate, case when isnull(c.CustomerStatus, 'Enable') = 'Enable' then '<span class=""badge badge-primary"">'+isnull(c.CustomerStatus, 'Enable')+'</span>' else '<span class=""badge badge-danger"">'+isnull(c.CustomerStatus, 'Enable')+'</span>' end CustomerStatus
from dbo.TblCustomer c 
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID";
            html.OrderBy = "[No], c.CustomerCode desc";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="c.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="c.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="c.DomainID", Value =html.DomainID },
            };
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Customer Code", FieldName = "CustomerCode", TextAlign="center"},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                new GridField() { Header = "Customer Type", FieldName = "CustomerType", IsGroupByField=true},
                new GridField() { Header = "Gender", FieldName = "CustomerGender", },
                new GridField() { Header = "Phone", FieldName = "CustomerMobile", },
                new GridField() { Header = "Address", FieldName = "CustomerAddress"},
                new GridField() { Header = "Status", FieldName = "CustomerStatus", TextAlign="center"},
                new GridField() { Header = "Created By", FieldName = "RegisterBy", TextAlign ="center" },
                new GridField() { Header = "Created Date", FieldName = "RegisterDate", TextAlign ="center" },
                new GridField() { Header = "Action", FieldName = "CustomerID", TextAlign = "Center",
                    IsModifyField = true, //IsHiddenField = (FormStatus.ToString().ToLower()=="quotation"?true:false),
					//HideDelete =(FormStatus.ToString().ToLower()=="quotation"?true:false),
					//HideEdit=(FormStatus.ToString().ToLower()=="quotation"?true:false),
					DeleteWithTableName="TblCustomerBillInformation, TblCustomerCompany, TblCustomerDetail, TblAgencyCommission"
                }
            };
            html.ShowPaging = false;
            html.DialogBoxWidth = "auto";

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Customer Profile, Edit Customer Profile, Delete Customer Profile")]
        public ActionResult CreateCustomerProfileInfo(object[] PrimaryFieldValue, int ISSO = 0)
        {
            HTML html = new HTML();
            ViewBag.RegisterBy = html.UserFullName;
            ViewBag.PrimaryFieldValue = 0;
            ViewBag.SQLCustomerType = EncryAndDecry.Encryp("select CustomerTypeID, CustomerType, PrefixVAT, PrefixCM, isnull(ISVAT, 'false') ISVAT from dbo.TblCustomerType where [Status] = 'Enable' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by isnull(OrderBy,9), CustomerType");//CustomerGroupID =@CustomerGroupID
            ViewBag.TblCustomerInvoice = EncryAndDecry.Encryp("TblCustomerInvoice");
            ViewBag.TblCustomer = EncryAndDecry.Encryp("TblCustomer");
            //if (ISSO == 0)
            //{
            ViewBag.SQLCID = EncryAndDecry.Encryp("select distinct top 10 CustomerCode CID from dbo.TblCustomer where lower(CustomerCode) like @SearchText order by CustomerCode");
            string sqlcallbackcid = @"select distinct c.EmployeeID, (l.Salutation + ' ' + l.FullName) Employee,
cg.CustomerGroup, c.CustomerGroupID, ct.CustomerType, c.CustomerTypeID,
c.CustomerName, c.CustomerGender, c.CustomerGender CustomerGenderID,
c.CustomerAddress, c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity,
c.CustomerMobile, c.CustomerEmail, c.CustomerNationality, c.CustomerIdentification,
c.ATTName, cb.CompanyName, cb.CompanyNameKH, cb.CompanyDeskPhone, cb.CompanyMobile,
cb.CompanyEmail, cb.VATNumber, id.IndustryType, cb.CompanyAddress,
cb.CompanyCommune, cb.CompanyDistrict, cb.CompanyProvinceCity,
case when isnull(cb.CompanyContactName, '') = '' then c.CustomerName else cb.CompanyContactName end CompanyContactName,
case when isnull(cb.CompanyContactName, '') = '' then c.CustomerName else cb.CompanyContactName end CustomerContactName,
cb.CompanyContactPosition,
case when isnull(cb.CompanyContactMobileNumber, '') = '' then c.CustomerMobile else cb.CompanyContactMobileNumber end CompanyContactMobileNumber,
case when isnull(cb.CompanyContactMobileNumber, '') = '' then c.CustomerMobile else cb.CompanyContactMobileNumber end CustomerContactMobileNumber,
cb.BillToAddress, cb.BillToCommune, cb.BillToDistrict, cb.BillToProvinceCity,
cb.InstallAddress, cb.InstallCommune, cb.InstallDistrict, cb.InstallProvinceCity,
cb.GoogleMap_FullAddress, cb.GoogleMap_Latitude, cb.GoogleMap_Longitude,c.CustomerID, cb.CustomerBillInformationID, c.IndustryTypeID,
ag.AgencyCommissionID, ag.FullName AgencyName, ag.HP AgencyPhone, ag.CommissionType AgencyCommissionType, ag.CommissionAmountUSD, ag.CommissionAmountP,
ag.[Address] AgencyAddress, ag.BankName AgencyBankName, ag.AccountName AgencyAccountName, 
ag.AccountNumber AgencyAccountNumber, ag.Memo, isnull(c.CustomerNoted, '') CustomerNoted, c.ClassNameID, c.ClassName, c.CustomerType_PrefixVAT, c.CustomerType_PrefixCM, c.CommuneID, c.DistrictID, c.ProvinceCityID, isnull(c.TypeOfInvoice,'None VAT') SetCID
from dbo.TblCustomer c
inner join dbo.TblLeads l on l.LeadID = c.EmployeeID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblCustomerBillInformation cb on cb.CustomerID = c.CustomerID
left join dbo.TblIndustryType id on id.IndustryTypeID = c.IndustryTypeID
left join dbo.TblAgencyCommission ag on ag.AgencyCommissionID = c.AgencyCommissionID
where lower(c.CustomerCode) = @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
            ViewBag.SQLCallBackCID = EncryAndDecry.Encryp(sqlcallbackcid);
            //}
            ViewBag.SQLCustName = EncryAndDecry.Encryp("select distinct top 10 CustomerID CustomerNameID, (case when CustomerCode <> '' then CustomerCode + ' - ' else '' end + CustomerName) CustName from dbo.TblCustomer where lower((case when CustomerCode <> '' then CustomerCode + ' - ' else '' end + CustomerName)) like @SearchText and SystemID=" + html.SystemID + " and WarehouseID =" + html.WarehouseID);
            string SQLCallBackCustName = @"select distinct c.CustomerName, c.EmployeeID, (l.Salutation + ' ' + l.FullName) Employee,
cg.CustomerGroup, c.CustomerGroupID, ct.CustomerType, c.CustomerTypeID CustomerType_CustomerTypeID,
c.CustomerGender, c.CustomerGender CustomerGenderID,
c.CustomerAddress, c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity,
c.CustomerMobile, c.CustomerEmail, c.CustomerNationality, c.CustomerIdentification,
c.ATTName, cb.CompanyName, cb.CompanyNameKH, cb.CompanyDeskPhone, cb.CompanyMobile,
cb.CompanyEmail, cb.VATNumber, id.IndustryType, cb.CompanyAddress,
cb.CompanyCommune, cb.CompanyDistrict, cb.CompanyProvinceCity,
case when isnull(cb.CompanyContactName, '') = '' then c.CustomerName else cb.CompanyContactName end CompanyContactName,
case when isnull(cb.CompanyContactName, '') = '' then c.CustomerName else cb.CompanyContactName end CustomerContactName,
cb.CompanyContactPosition,
case when isnull(cb.CompanyContactMobileNumber, '') = '' then c.CustomerMobile else cb.CompanyContactMobileNumber end CompanyContactMobileNumber,
case when isnull(cb.CompanyContactMobileNumber, '') = '' then c.CustomerMobile else cb.CompanyContactMobileNumber end CustomerContactMobileNumber,
cb.BillToAddress, cb.BillToCommune, cb.BillToDistrict, cb.BillToProvinceCity,
cb.InstallAddress, cb.InstallCommune, cb.InstallDistrict, cb.InstallProvinceCity,
cb.GoogleMap_FullAddress, isnull(cb.GoogleMap_Latitude, 0) GoogleMap_Latitude, 
isnull(cb.GoogleMap_Longitude, 0) GoogleMap_Longitude,c.CustomerID, cb.CustomerBillInformationID, c.IndustryTypeID, c.CustomerCode,
c.ClassNameID, c.ClassName, c.CustomerType_PrefixVAT, c.CustomerType_PrefixCM, c.CommuneID, c.DistrictID, c.ProvinceCityID,isnull(c.TypeOfInvoice,'None VAT') SetCID
from dbo.TblCustomer c
inner join dbo.TblLeads l on l.LeadID = c.EmployeeID
left join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
left join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblCustomerBillInformation cb on cb.CustomerID = c.CustomerID
left join dbo.TblIndustryType id on id.IndustryTypeID = c.IndustryTypeID
where c.CustomerID = @SearchText";
            //where lower((case when c.CustomerCode <> '' then c.CustomerCode + ' - ' else '' end + c.CustomerName)) = @SearchText
            ViewBag.SQLCallBackCustName = EncryAndDecry.Encryp(SQLCallBackCustName);

            ViewBag.AgencyCommissionData = "[]";
            ViewBag.ISSO = ISSO.ToString();

            html.AllowAdd = html.HasPermission("Add Customer");

            if (html.HasPermission("Edit Customer"))
                html.AllowEdit = true;
            else if (html.HasPermission("Edit Sale Order"))
                html.AllowEdit = true;
            else
                html.AllowAdd = false;
            ViewBag.ChangePlan = false;
            ViewBag.InvoiceNo = "";
            ViewBag.CustInfo = "";
            ViewBag.CustDetail = "";
            ViewBag.CustInvoice = "";
            ViewBag.CustInvoiceDeposit = "";
            ViewBag.CustInvoiceDetail = "";
            ViewBag.CustInvoiceTotal = "";
            ViewBag.CloneInvoice = "false";
            ViewBag.IsRenew = "false";
            ViewBag.NextInvoiceDate = "";
            ViewBag.InvoiceType = "Created Invoice";
            ViewBag.OldBillingInvoiceID = "0";
            ViewBag.ConnectivityStatus = 43; //Inactive service
            ViewBag.PPPoEUserName = "";
            ViewBag.IsPreviewContract = "False";
            ViewBag.CustomerCurrentPlan = "";
            ViewBag.ConnectionID = "";
            string ConnectionID = "";
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(PrimaryFieldValue[0].ToString());
            string FormStatus = "";
            //if (PrimaryFieldValue[0].ToString().Contains("ConnectionID"))
            //{
            //    ViewBag.ConnectionID = DataInfo["ConnectionID"].ToString();
            //}
            string CustomerID = "", InvoiceNo = "";
            //sql = "select uw.WID LocationID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName, uw.ContactName, uw.ContactPhone, uw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + case when uw.District <> '' then ', ' + uw.District end + case when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end DeliverTo, uw.WarehouseName WName from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by (b.BranchName + ' - ' + uw.WarehouseName)";
            //ViewBag.SQLWarehouseName = EncryAndDecry.Encryp(sql);
            string sql = "select uw.WID LocationID, uw.WarehouseName from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and uw.DomainID like '%" + html.DomainID + "%' order by uw.WarehouseName";
            ViewBag.SQLWarehouseName = html.JsonData(sql);

            sql = "select LeadID EmployeeID, Salutation + ' ' + FullName EmployeeName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            ViewBag.EmployeeData = html.JsonData(sql);
            sql = "select CustomerGroupID, CustomerGroup from dbo.TblCustomerGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            ViewBag.CustomerGroup = html.JsonData(sql);
            sql = "select CustomerTypeID, CustomerType from dbo.TblCustomerType where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            ViewBag.CustomerType = html.JsonData(sql);
            sql = "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ClassName";
            ViewBag.ClassName = html.JsonData(sql);
            sql = "select uw.WID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName, uw.ContactName, uw.ContactPhone, uw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + case when uw.District <> '' then ', ' + uw.District end + case when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end Warehouse from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and uw.DomainID like '%" + html.DomainID + "%' order by (b.BranchName + ' - ' + uw.WarehouseName)";
            ViewBag.LoadLocation = html.JsonData(sql);

            DataSet ds = new DataSet();
            ViewBag.ApprovalPO = false;
            if (PrimaryFieldValue[0].ToString().Contains("ISSO"))
                ViewBag.ISSO = DataInfo["ISSO"].ToString();
            if (PrimaryFieldValue[0].ToString().Contains("ApprovalPO"))
                ViewBag.ApprovalPO = bool.Parse(DataInfo["ApprovalPO"].ToString());
            ViewBag.ToSaleOrder = false;
            if (PrimaryFieldValue[0].ToString().Contains("ToSaleOrder"))
            {
                ViewBag.ToSaleOrder = bool.Parse(DataInfo["ToSaleOrder"].ToString());
            }
            if (PrimaryFieldValue[0].ToString().Contains("PrimaryFieldValue"))
            {
                ViewBag.PrimaryFieldValue = DataInfo["PrimaryFieldValue"].ToString();
                CustomerID = DataInfo["CustomerID"].ToString();
                InvoiceNo = DataInfo["InvoiceNo"].ToString();
                sql = "select BillingInvoiceID, ConnectivityStatus, PPPoEUserName, ConnectionID from dbo.TblCustomerBillingInvoice where CustomerID=" + CustomerID + " and InvoiceNo=N'" + InvoiceNo + "'";
                ds = html.LoadData(sql);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        ViewBag.ConnectionID = ds.Tables[0].Rows[0]["ConnectionID"].ToString();
                    }
                }
            }
            ViewBag.EditInvoice = false;
            if (PrimaryFieldValue[0].ToString().Contains("EditInvoice"))
            {
                ViewBag.EditInvoice = DataInfo["EditInvoice"].ToString();
                bool EditInvoice = bool.Parse(DataInfo["EditInvoice"].ToString());
            }
            if (PrimaryFieldValue[0].ToString().Contains("IsRenew"))
            {
                bool IsRenew = bool.Parse(DataInfo["IsRenew"].ToString());
                if (IsRenew)
                    ViewBag.EditInvoice = false;
            }
            ViewBag.KeepBillingInvoiceID = 0;
            ViewBag.KeepConnectionID = 0;
            if (PrimaryFieldValue[0].ToString().Contains("ChangePlan"))
            {
                sql = @" select ac.ActivateCustomerID, s.ServiceName, pt.PackageTypeName, 
                                p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + bi.UM PackageName,
                                pd.Bandwidth, bi.UM, pd.DowSpeed, pd.UpSpeed, pd.Price, format(bi.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate, format(bi.StartDate, 'dd-MMM-yyyy') StartDate, format(bi.EndDate, 'dd-MMM-yyyy') EndDate,
                                case when (DATEDIFF(D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate))) < 0 then 0 else DATEDIFF (D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate)) end Remain, 
                                isnull(ac.ServiceID,0) ServiceID, isnull(bi.PackageDetailID, 0) PackageDetailID, isnull(bi.FreeDuration,0) FreeDuration, isnull(bi.Duration,0) Duration, 
                                bi.InvoiceNo, bi.ConnectionID
                                from dbo.TblActivateCustomer ac
                                inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
                                inner join dbo.TblServices s on s.ServiceID = ac.ServiceID
                                inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
                                inner join dbo.TblPackages p on p.PackageID = pd.PackageID
                                inner join dbo.TblPackageType pt on p.PackageTypeID = pt.PackageTypeID
                                inner join dbo.TblCustomerBillingInvoice bi on bi.ActivateCustomerID = ac.ActivateCustomerID
                                where bi.BillingInvoiceID = @BillingInvoiceID";
                Dictionary<string, string> Para = new Dictionary<string, string>
                {
                    { "BillingInvoiceID", DataInfo["BillingInvoiceID"].ToString() }
                };
                ViewBag.CustomerCurrentPlan = html.JsonData(sql, false, Para);
                ViewBag.ChangePlan = true;
                ViewBag.KeepBillingInvoiceID = DataInfo["BillingInvoiceID"].ToString();
                ViewBag.KeepConnectionID = DataInfo["ConnectionID"].ToString();
            }
            if (PrimaryFieldValue[0].ToString().Contains("IsPreviewContract"))
                ViewBag.IsPreviewContract = DataInfo["IsPreviewContract"].ToString();
            if (PrimaryFieldValue[0].ToString().Contains("CloneInvoice"))
                ViewBag.CloneInvoice = DataInfo["CloneInvoice"].ToString();
            if (PrimaryFieldValue[0].ToString().Contains("IsRenew"))
            {
                ViewBag.IsRenew = DataInfo["IsRenew"].ToString();
                if (bool.Parse(DataInfo["IsRenew"].ToString()))
                    ViewBag.InvoiceType = "Renew Invoice";
                //string CustomerID = "";
                if (PrimaryFieldValue[0].ToString().Contains("PrimaryFieldValue"))
                {
                    CustomerID = DataInfo["CustomerID"].ToString();
                }

                sql = "select BillingInvoiceID, ConnectivityStatus, PPPoEUserName, ConnectionID from dbo.TblCustomerInvoice where CustomerID=" + CustomerID + " and InvoiceNo=N'" + InvoiceNo + "'";
                ds = html.LoadData(sql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    ConnectionID = ds.Tables[0].Rows[0]["ConnectionID"].ToString();
                    ViewBag.ConnectivityStatus = ds.Tables[0].Rows[0]["ConnectivityStatus"].ToString();
                    ViewBag.OldBillingInvoiceID = ds.Tables[0].Rows[0]["BillingInvoiceID"].ToString();
                    ViewBag.PPPoEUserName = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
                }
                else
                {
                    ConnectionID = "";
                    ViewBag.ConnectivityStatus = "";
                    ViewBag.OldBillingInvoiceID = "";
                    ViewBag.PPPoEUserName = "";
                }

            }
            if (PrimaryFieldValue[0].ToString().Contains("InvoiceType"))
            {
                ViewBag.InvoiceType = DataInfo["InvoiceType"].ToString();
            }
            if (PrimaryFieldValue[0].ToString().Contains("NextInvoiceDate"))
            {
                ViewBag.NextInvoiceDate = DataInfo["NextInvoiceDate"].ToString() != ""
                    ? DataInfo["NextInvoiceDate"].ToString()
                    : DateTime.Parse(html.GetCurrentDate()).ToString("yyyy-MM-dd");
            }
            string value;
            if (PrimaryFieldValue[0].ToString().Contains("PrimaryFieldValue"))
            {
                value = DataInfo["PrimaryFieldValue"].ToString();
                InvoiceNo = DataInfo["InvoiceNo"].ToString();
                CustomerID = DataInfo["CustomerID"].ToString();
                FormStatus = DataInfo["FormStatus"].ToString();
                //if (value == "1")
                //{
                //    FormStatus = "Create Quotation";
                //    if (html.HasPermission("Create Quotation"))
                //        html.AllowAdd = true;
                //    else
                //        html.AllowAdd = false;
                //    if (!html.AllowAdd)
                //        return RedirectToAction("Index", "NoPermission");
                //    if (InvoiceNo != "0")
                //    {
                //        if (html.HasPermission("Edit Quotation"))
                //            html.AllowEdit = true;
                //        else
                //            html.AllowEdit = false;
                //        if (!html.AllowEdit)
                //            return RedirectToAction("Index", "NoPermission");
                //    }
                //}
                if (CustomerID != "0")
                {
                    if (FormStatus == "CreateInvoice")
                    {
                        html.AllowAdd = html.HasPermission("Create Invoice") || html.HasPermission("Edit Sale Order");
                        if (!html.AllowAdd)
                            return RedirectToAction("Index", "NoPermission");
                    }
                    sql = @"select (e.Salutation + e.FullName) Employee, c.EmployeeID, c.CustomerID, 
isnull(ty.CustomerType, '') CustomerType, c.CustomerTypeID, c.CustomerName, replace(isnull(c.CustomerCode, ''), 'null', '') CustomerCode, 
c.CustomerGender, c.CustomerGender CustomerGenderID, c.CustomerAddress,
c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity,c.CustomerStatus,
c.CustomerMobile, c.CustomerEmail, c.CustomerNationality, c.CustomerIdentification,
c.CustomerContactName, c.CustomerContactMobileNumber, c.CustomerContactEmail, c.CustomerGroupID, c.CustomerGroup, isnull(c.AgencyCommissionID,0) AgencyCommissionID, isnull(c.CustomerNoted,'') CustomerNoted, isnull(TypeOfInvoice,'None VAT') SetCID, CustomerType_PrefixVAT, CustomerType_PrefixCM, c.ClassName, c.ClassNameID, c.CommuneID, c.DistrictID, c.ProvinceCityID, c.ISApplyCID
from dbo.TblCustomer c
left join dbo.TblCustomerType ty on ty.CustomerTypeID = c.CustomerTypeID
left join dbo.TblLeads e on e.LeadID = c.EmployeeID
where CustomerID = " + CustomerID + ";";
                    ViewBag.CustInfo = html.JsonData(sql);
                    sql = @"select distinct top 1 cb.CustomerBillInformationID, cb.CompanyName, cb.CompanyNameKH, cb.CompanyDeskPhone, cb.CompanyMobile, 
cb.CompanyEmail, cb.VATNumber, cb.CompanyAddress, cb.CompanyCommune, isnull(ci.PID, NEWID()) PID,
cb.CompanyDistrict, cb.CompanyProvinceCity, cb.CompanyContactName,
cb.CompanyContactPosition, cb.CompanyContactMobileNumber, cb.ATTName,
cb.BillToAddress, cb.BillToCommune, cb.BillToDistrict, cb.BillToProvinceCity,
cb.InstallAddress, cb.InstallCommune, cb.InstallDistrict, cb.InstallProvinceCity, cb.IndustryTypeID, it.IndustryType, cb.GoogleMap_FullAddress, cb.GoogleMap_Latitude, cb.GoogleMap_Longitude, cb.InvoiceType, cb.ClassNameID, cb.ClassName, cb.LocationID, cb.Warehouse, cb.InstallDate
from dbo.TblCustomerBillInformation cb
left join dbo.TblIndustryType it on it.IndustryTypeID = cb.IndustryTypeID
left join dbo.TblCustomerInvoice ci on ci.CustomerBillInformationID = cb.CustomerBillInformationID
where cb.CustomerID = " + CustomerID + " and " +
"case when isnull(ci.ConnectionID,'') <> '' then isnull(ci.ConnectionID,'') else '" + ConnectionID + "' end = case when '" + ConnectionID + "' <> '' then '" + ConnectionID + "' else isnull(ci.ConnectionID,'') end";
                    //if (InvoiceNo != "0")
                    //{
                    //    sql += " and ci.InvoiceNo = '" + InvoiceNo + "'";
                    //}
                    ViewBag.CustDetail = html.JsonData(sql);
                    //					dynamic AgencyID = new JavaScriptSerializer().DeserializeObject(ViewBag.CustInfo);
                    //					string AgencyCommissionID = AgencyID[0]["AgencyCommissionID"].ToString();
                    //					sql = @"select AgencyCommissionID, FullName AgencyName, HP AgencyPhone,
                    //CommissionType AgencyCommissionType, [Address] AgencyAddress,
                    //BankName AgencyBankName, AccountName AgencyAccountName, 
                    //AccountNumber AgencyAccountNumber, Memo, CommissionAmountUSD, CommissionAmountP
                    //from dbo.TblAgencyCommission where AgencyCommissionID = " + AgencyCommissionID;
                    //					ViewBag.AgencyCommissionData = html.JsonData(sql);
                }
                if (InvoiceNo != "0")
                {
                    ViewBag.InvoiceNo = InvoiceNo;

                    sql = "SP_GET_INVOICE_INFO";
                    Dictionary<string, string> Para = new Dictionary<string, string>()
                    {
                        { "InvoiceNumber", InvoiceNo },
                        { "InvoiceType", ViewBag.InvoiceType },
                        { "ConnectionID", ViewBag.ConnectionID },
                        { "CustomerID", CustomerID==""?"0":CustomerID },
                        { "SystemID", html.SystemID },
                        { "WarehouseID", html.WarehouseID }
                    };
                    //ds = html.LoadData(sql, true, Para);
                    var dd = html.JsonData(sql, true, Para);
                    ViewBag.CustInvoice = dd.ToJsonString(); //ds.Tables[0].Rows.ToJsonString();
                                                             //dynamic AgencyID = null;
                                                             //string AgencyCommissionID = "0";
                                                             //if (dd != "[]")
                                                             //{
                                                             //	AgencyID = new JavaScriptSerializer().DeserializeObject(dd);
                                                             //	AgencyCommissionID = AgencyID[0]["AgencyCommissionID"].ToString();
                                                             //}

                    //DataRow[] AgencyID = ds.Tables[0].Select("AgencyCommissionID <> 0");
                    //					sql = @"select AgencyCommissionID, FullName AgencyName, HP AgencyPhone,
                    //CommissionType AgencyCommissionType, [Address] AgencyAddress,
                    //BankName AgencyBankName, AccountName AgencyAccountName, 
                    //AccountNumber AgencyAccountNumber, Memo, CommissionAmountUSD, CommissionAmountP
                    //from dbo.TblAgencyCommission where AgencyCommissionID = " + AgencyCommissionID;//AgencyID[0]["AgencyCommissionID"].ToString();
                    //					ViewBag.AgencyCommissionData = html.JsonData(sql);

                    //string result = ViewBag.CustInvoice.Substring(1, ViewBag.CustInvoice.Length - 2);
                    //Dictionary<string, string> Datainfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                    //string IsChangePlan = Datainfo["IsChangePlan"], FromActivateCustomerID = Datainfo["FromActivateCustomerID"];
                    if (ViewBag.IsRenew == "false")
                    {
                        if (!PrimaryFieldValue[0].ToString().Contains("ChangePlan"))
                        {
                            sql = "select distinct IsChangePlan, FromActivateCustomerID from dbo.TblCustomerInvoice where InvoiceNo=@InvoiceNo and CusotmerID = @CustomerID";
                            Para = new Dictionary<string, string>()
                            {
                                { "InvoiceNo", InvoiceNo },
                                { "CustomerID", CustomerID }
                            };
                            ds = html.LoadData(sql, false, Para);
                            string IsChangePlan = "", FromActivateCustomerID = "";
                            if (ds.Tables.Count > 0)
                            {
                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    IsChangePlan = ds.Tables[0].Rows[0]["IsChangePlan"].ToString();
                                    FromActivateCustomerID = ds.Tables[0].Rows[0]["FromActivateCustomerID"].ToString();
                                }
                            }
                            if (IsChangePlan == "True")
                            {
                                sql = @" select ac.ActivateCustomerID, s.ServiceName, pt.PackageTypeName, 
                                p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + bi.UM PackageName,
                                pd.Bandwidth, bi.UM, pd.DowSpeed, pd.UpSpeed, pd.Price, format(bi.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate, format(bi.StartDate, 'dd-MMM-yyyy') StartDate, format(bi.EndDate, 'dd-MMM-yyyy') EndDate,
                                case when (DATEDIFF(D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate))) < 0 then 0 else DATEDIFF (D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate)) end Remain, 
                                isnull(ac.ServiceID,0)ServiceID, isnull(bi.PackageDetailID,0) PackageDetailID, 
                                isnull(bi.FreeDuration,0)FreeDuration, isnull(bi.Duration,0) Duration, bi.InvoiceNo, bi.PID, bi.ConnectionID
                                from dbo.TblActivateCustomer ac
                                inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
                                inner join dbo.TblServices s on s.ServiceID = ac.ServiceID
                                inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
                                inner join dbo.TblPackages p on p.PackageID = pd.PackageID
                                inner join dbo.TblPackageType pt on p.PackageTypeID = pt.PackageTypeID
                                inner join dbo.TblCustomerBillingInvoice bi on bi.ActivateCustomerID = ac.ActivateCustomerID
                                where ac.ActivateCustomerID = @ActivateCustomerID";
                                Para = new Dictionary<string, string>
                                {
                                    { "ActivateCustomerID", FromActivateCustomerID }
                                };
                                ViewBag.CustomerCurrentPlan = html.JsonData(sql, false, Para);
                                ViewBag.ChangePlan = true;
                            }
                        }
                    }
                    sql = "select ci.UM, ci.QTY, ci.UnitPrice, ci.DiscountP, ";
                    sql += "ci.DiscountUSD, ci.Amount, ";
                    if (ViewBag.InvoiceType == "Renew Invoice")
                        sql += "0 DepositFee ";
                    else
                        sql += "ci.DepositFee, ci.PID, ci.ConnectionID, ci.[Type],ci.ReferenceNo, ci.QuotationNo, ci.IsIncludedVAT, ci.InvoiceStatusID, ci.LocationID, ci.Warehouse ";
                    sql += "from dbo.TblCustomerInvoice ci ";
                    sql += "inner join dbo.TblPackageDetail p on p.PackageDetailID = ci.PackageDetailID ";
                    sql += "where ci.CustomerID = " + CustomerID + " and InvoiceNo = N'" + InvoiceNo + "' and ci.DepositFee <> 0";
                    ViewBag.CustInvoiceDeposit = html.JsonData(sql);
                    //if (!EditInvoice)
                    //{
                    sql = @"select  ci.ItemDescription, ci.UM, ci.QTY, ci.UnitPrice, ci.DiscountP, ci.DiscountUSD, ci.Amount, ci.OtherChargeID, ci.StockInID, 
ci.ItemID, format(ci.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate, format(ci.PaymentDueDate, 'dd-MMM-yyyy') PaymentDueDate, 
isnull(ci.ConnectionID, '') ConnectionID, ci.[Type],ci.ReferenceNo, ci.QuotationNo, ci.IsIncludedVAT, isnull(ci.InvoiceStatusID, '') InvoiceStatusID,
ci.LocationID, ci.Warehouse, isnull(ci.ServiceID, 0) ServiceID, isnull(ci.[SID], 0) [SID], ci.ChartOfAccountID from dbo.TblCustomerInvoice ci 
where '" + ViewBag.InvoiceType + "' <> 'Renew Invoice' and ci.CustomerID = " + CustomerID + " and InvoiceNo = N'" + InvoiceNo + @"' 
and (ci.OtherChargeID <> 0 or ci.ItemID <> 0) 
order by ci.OrderBy";
                    //}
                    //               else
                    //               {
                    //	sql = "select  ci.ItemDescription, ci.UM, ci.QTY, ci.UnitPrice, ci.DiscountP, ci.DiscountUSD, ci.Amount, ";
                    //	sql += "ci.OtherChargeID, ci.StockInID, ci.ItemID, format(ci.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate, format(ci.PaymentDueDate, 'dd-MMM-yyyy') PaymentDueDate, ci.ConnectionID, ci.[Type],ci.ReferenceNo, ci.QuotationNo, ci.IsIncludedVAT, ci.InvoiceStatusID, ci.LocationID, ci.Warehouse from dbo.TblCustomerInvoice ci ";
                    //                   sql += "where '" + ViewBag.InvoiceType + "' <> 'Renew Invoice' and ci.CustomerID = " + CustomerID + " and InvoiceNo = N'" + InvoiceNo + "' and ItemDescription <> ''";
                    //}
                    ViewBag.CustInvoiceDetail = html.JsonData(sql);
                    sql = @"select SubTotal, VAT, Total, DiscountP, DiscountUSD, GrandTotal, IsIncludedVAT ";
                    sql += "from dbo.TblCustomerBalance where '" + ViewBag.InvoiceType + "' <> 'Renew Invoice' and CustomerID = " + CustomerID + " and InvoiceNo = N'" + InvoiceNo + "'";
                    ViewBag.CustInvoiceTotal = html.JsonData(sql);
                }
            }
            else
            {
                value = PrimaryFieldValue[0].ToString();
                if (value == "1")
                {

                    html.AllowAdd = html.HasPermission("Create Quotation");

                    //if (html.HasPermission("Edit Quotation"))
                    //    html.AllowEdit = true;
                    //else
                    //    html.AllowEdit = false;
                    FormStatus = "Create Quotation";
                    if (!html.AllowAdd)
                        return RedirectToAction("Index", "NoPermission");
                }
                else if (value != "0")
                {
                    CustomerID = value;
                    sql = "select (e.Salutation + e.FullName) Employee, c.EmployeeID, c.CustomerID, ty.CustomerType, c.CustomerTypeID, c.CustomerName, c.CustomerCode, c.CustomerGender, c.CustomerGender CustomerGenderID, c.CustomerAddress, ";
                    sql += "c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity,isnull(c.CustomerStatus, 'Enable') as CustomerStatus, ";
                    sql += "c.CustomerMobile, c.CustomerEmail, c.CustomerNationality, c.CustomerIdentification, ";
                    sql += "c.CustomerContactName, c.CustomerContactMobileNumber, c.CustomerContactEmail, c.CustomerGroupID, c.CustomerGroup, isnull(c.AgencyCommissionID,0) AgencyCommissionID, isnull(c.TypeOfInvoice,'None VAT') SetCID, c.ClassName, c.ClassNameID, c.CommuneID, c.DistrictID, c.ProvinceCityID, c.ISApplyCID ";
                    sql += "from dbo.TblCustomer c ";
                    sql += "left join dbo.TblCustomerType ty on ty.CustomerTypeID = c.CustomerTypeID ";
                    sql += "left join dbo.TblLeads e on e.LeadID = c.EmployeeID ";
                    sql += "where CustomerID = " + CustomerID + "; ";
                    ViewBag.CustInfo = html.JsonData(sql);
                    sql = @"select distinct cb.CustomerBillInformationID, cb.CompanyName, cb.CompanyNameKH, cb.CompanyDeskPhone, cb.CompanyMobile, 
cb.CompanyEmail, cb.VATNumber, cb.CompanyAddress, cb.CompanyCommune,
cb.CompanyDistrict, cb.CompanyProvinceCity, cb.CompanyContactName,
cb.CompanyContactPosition, cb.CompanyContactMobileNumber, cb.ATTName,
cb.BillToAddress, cb.BillToCommune, cb.BillToDistrict, cb.BillToProvinceCity,
cb.InstallAddress, cb.InstallCommune, cb.InstallDistrict, cb.InstallProvinceCity, cb.IndustryTypeID, it.IndustryType, cb.GoogleMap_FullAddress, cb.GoogleMap_Latitude, cb.GoogleMap_Longitude, cb.InvoiceType, cb.ClassNameID, cb.ClassName, cb.LocationID, cb.Warehouse
from dbo.TblCustomerBillInformation cb
left join dbo.TblIndustryType it on it.IndustryTypeID = cb.IndustryTypeID
where cb.CustomerID = " + CustomerID;
                    ViewBag.CustDetail = html.JsonData(sql);

                    //dynamic AgencyID = new JavaScriptSerializer().DeserializeObject(ViewBag.CustInfo);
                    //string AgencyCommissionID = AgencyID[0]["AgencyCommissionID"].ToString();
                    //					sql = @"select AgencyCommissionID, FullName AgencyName, HP AgencyPhone,
                    //CommissionType AgencyCommissionType, [Address] AgencyAddress,
                    //BankName AgencyBankName, AccountName AgencyAccountName, 
                    //AccountNumber AgencyAccountNumber, Memo, CommissionAmountUSD, CommissionAmountP
                    //from dbo.TblAgencyCommission where AgencyCommissionID = " + AgencyCommissionID;
                    //					ViewBag.AgencyCommissionData = html.JsonData(sql);

                    sql = @"select top 1 InvoiceNo from dbo.TblCustomerInvoice where CustomerID = " + CustomerID + " order by CustomerInvoiceID desc";
                    if (html.LoadData(sql).Tables[0].Rows.Count > 0)
                        ViewBag.InvoiceNo = html.LoadData(sql).Tables[0].Rows[0]["InvoiceNo"].ToString();
                    InvoiceNo = ViewBag.InvoiceNo;
                }
            }
            ViewBag.isRegisterForm = value;
            ViewBag.FormStatus = FormStatus;
            ViewBag.OldValueInvoiceNo = ViewBag.IsRenew == "false" ? InvoiceNo : "";
            return View();
        }
        [Authorize(Roles = "Add Customer Profile, Edit Customer Profile, Delete Customer Profile")]
        public async Task<ActionResult> SaveChangeCustomer(object[] CustomerData, object[] CustInvoice, object[] CustInvoiceDetail, object[] CustomerOtherChargeInvoice, int ISSO = 0, bool PreviewInvoice = false, object[] objAgencyCommission = null)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            dynamic CustomerDataInfo = new JavaScriptSerializer().DeserializeObject(CustomerData[0].ToString());
            dynamic CustomerInvoice = new JavaScriptSerializer().DeserializeObject(CustInvoice[0].ToString());
            dynamic CustInvDetail = new JavaScriptSerializer().DeserializeObject(CustInvoiceDetail[0].ToString());
            dynamic CustomerInvoiceOtherCharge = new JavaScriptSerializer().DeserializeObject(CustomerOtherChargeInvoice[0].ToString());
            dynamic AgencyCommissionObj = new JavaScriptSerializer().DeserializeObject(objAgencyCommission[0].ToString());
            JavaScriptSerializer jsJson = new JavaScriptSerializer
            {
                MaxJsonLength = int.MaxValue
            };
            //dynamic ImgPdfFiles = jsJson.DeserializeObject(ImgPdfFile[0].ToString());
            //string imgData = ImgPdfFiles["imgData"];
            //string pdfData = ImgPdfFiles["pdfData"];
            string CustomerName = HttpUtility.UrlDecode(CustomerDataInfo[0]["CustomerName"]);
            string CustomerStatus = HttpUtility.UrlDecode(CustomerDataInfo[0]["CustomerStatus"]);
            string CustIDToDMC = CustomerDataInfo[0]["CustIDToDMC"].ToString();
            string CustContactPhone = CustomerDataInfo[0]["CompanyContactMobileNumber"].ToString();
            string CustomerID = CustomerDataInfo[0]["CustomerID"].ToString();
            string InvoiceToDMC = CustomerDataInfo[0]["InvoiceToDMC"].ToString();
            string CID = HttpUtility.UrlDecode(CustomerDataInfo[0]["CustomerCode"]);
            string FormStatus = HttpUtility.UrlDecode(CustomerDataInfo[0]["FormStatus"]);
            bool ISToVATSystem = bool.Parse(CustomerDataInfo[0]["ISToVATSystem"].ToString());
            bool ISSameInvoiceNo = bool.Parse(CustomerDataInfo[0]["ISSameInvoiceNo"].ToString());
            bool ISSameCID = bool.Parse(CustomerDataInfo[0]["ISSameCID"].ToString());
            bool IsEditInvoice = bool.Parse(CustomerDataInfo[0]["IsEditInvoice"].ToString());
            var aa = CustomerDataInfo[0]["QuotationNo"];
            string RequestApprovalSOInvoiceNo;
            if (aa != null)
                RequestApprovalSOInvoiceNo = CustomerDataInfo[0]["QuotationNo"].ToString();
            bool IsRequestApprovalSO = bool.Parse(CustomerDataInfo[0]["IsRequestApprovalSO"].ToString());
            bool isInternetServices = bool.Parse(CustomerDataInfo[0]["isInternetServices"].ToString());
            string InvoiceNo = CustomerDataInfo[0]["CustomerInvoiceNo"].ToString();
            HTML html = new HTML();
            CustomerName = $"{html.ToUrlSlug($"{CustomerName + " [" + CID + "]"}")}";
            Regex regex = new Regex(@"^[\w/\:.-]+;base64,");
            //string base64 = "";
            //byte[] bitmapData = null;

            //if (pdfData != "")
            //{
            //    base64 = regex.Replace(pdfData, string.Empty);
            //    bitmapData = Convert.FromBase64String(html.FixBase64ForImage(base64));
            //    pdfData = html.UploadToServer(bitmapData, "Uploads/CustomerAttFiles/" + CustomerName, CustomerName, true, false);
            //}
            //if (imgData != "")
            //{
            //    base64 = regex.Replace(imgData, string.Empty);
            //    bitmapData = Convert.FromBase64String(html.FixBase64ForImage(base64));
            //    imgData = html.UploadToServer(bitmapData, "Uploads/CustomerAttFiles", CustomerName, false, false);
            //}
            string DestinationFolder = "/Uploads/CustomerAttFiles/" + CustomerName;//$"{html.ToUrlSlug($"{CustomerName + " [" + CustomerCode + "]"}")}";
            SqlResult result = new SqlResult();
            string SystemID = html.SystemID;
            string WarehouseID = html.WarehouseID;
            string DomainID = html.DomainID;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<Data>");

            result = html.UploadFile(DestinationFolder, "CustomerAttFiles");
            string AttachmentFile;
            if (result.Result == ResultType.Successful && result.Value != "")
            {
                AttachmentFile = result.Value;
                sb.AppendLine("<CustomerAttachmentFileName>");
                sb.AppendLine("<AttachmentFileName>" + AttachmentFile + "</AttachmentFileName>");
                sb.AppendLine("<DestinationFolder>" + DestinationFolder + "</DestinationFolder>");
                sb.AppendLine("<TypeFor>Attachment File</TypeFor>");
                sb.AppendLine("</CustomerAttachmentFileName>");
            }
            else if (result.Result == ResultType.Failed)
            {
                return Json(new
                {
                    Message = result.Message,
                    Result = result.Result,
                }, JsonRequestBehavior.AllowGet);
            }
            result = html.UploadFile(DestinationFolder, "CustomerBankSlip");
            if (result.Result == ResultType.Successful && result.Value != "")
            {
                AttachmentFile = result.Value;
                sb.AppendLine("<CustomerAttachmentFileName>");
                sb.AppendLine("<AttachmentFileName>" + AttachmentFile + "</AttachmentFileName>");
                sb.AppendLine("<DestinationFolder>" + DestinationFolder + "</DestinationFolder>");
                sb.AppendLine("<TypeFor>Customer Bank Slip</TypeFor>");
                sb.AppendLine("</CustomerAttachmentFileName>");
            }
            else if (result.Result == ResultType.Failed)
            {
                return Json(new
                {
                    Message = result.Message,
                    Result = result.Result,
                }, JsonRequestBehavior.AllowGet);
            }


            sb.AppendLine("<CustomerInfo>");
            foreach (dynamic item in CustomerDataInfo[0])
            {
                if (item.Value != null)
                {
                    string key = item.Key;
                    string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                    sb.AppendLine("<" + key + ">" + html.EscapeXml(value).ToString().Trim() + "</" + key + ">");
                }
            }
            sb.AppendLine("<RegisterDate>" + easternTimeNow.ToString("yyy-MM-dd hh:mm:ss tt") + "</RegisterDate>");
            sb.AppendLine("<SystemID>" + SystemID + "</SystemID>");
            sb.AppendLine("<WarehouseID>" + WarehouseID + "</WarehouseID>");
            sb.AppendLine("<DomainID>" + DomainID + "</DomainID>");
            sb.AppendLine("<UserName>" + html.UserFullName + "</UserName>");
            sb.AppendLine("<Destination>Uploads/CustomerAttFiles/" + CustomerName + "</Destination>");
            //sb.AppendLine("<imgFileName>" + imgData + "</imgFileName>");
            //sb.AppendLine("<pdfFileName>" + pdfData + "</pdfFileName>");
            sb.AppendLine("</CustomerInfo>");
            if (CustomerInvoice.Length > 0)
            {
                foreach (dynamic data in CustomerInvoice)
                {
                    sb.AppendLine("<CustomerInvoice>");
                    foreach (dynamic item in data)
                    {
                        if (item.Value != null)
                        {
                            string key = item.Key;
                            string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                            sb.AppendLine("<" + key + ">" + html.EscapeXml(value).ToString().Trim() + "</" + key + ">");
                        }
                    }
                    sb.AppendLine("<SystemID>" + SystemID + "</SystemID>");
                    sb.AppendLine("<WarehouseID>" + WarehouseID + "</WarehouseID>");
                    sb.AppendLine("<DomainID>" + DomainID + "</DomainID>");
                    sb.AppendLine("</CustomerInvoice>");
                }
            }
            if (CustInvDetail.Length > 0)
            {
                sb.AppendLine("<CustomerInvoiceDetail>");
                foreach (dynamic item in CustInvDetail[0])
                {
                    if (item.Value != null)
                    {
                        string key = item.Key;
                        string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                        sb.AppendLine("<" + key + ">" + html.EscapeXml(value).ToString().Trim() + "</" + key + ">");
                    }
                }
                sb.AppendLine("<SystemID>" + SystemID + "</SystemID>");
                sb.AppendLine("<WarehouseID>" + WarehouseID + "</WarehouseID>");
                sb.AppendLine("<DomainID>" + DomainID + "</DomainID>");
                sb.AppendLine("</CustomerInvoiceDetail>");
            }
            foreach (dynamic data in CustomerInvoiceOtherCharge)
            {
                sb.AppendLine("<CustomerOtherCharge>");
                string ItemID = data["ItemID"].ToString().Trim();
                string LocationID = data["LocationID"].ToString().Trim();
                string sql = @"select (sum(isnull(sd.QTY,0)) - sum(isnull(sd.QTYOUT,0))) StockOnHand, i.ItemName
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblUserWarehouse w on w.WID = sd.LocationID
where i.ItemID = " + ItemID + " and w.WID = " + LocationID + " group by i.ItemName";
                DataSet ds = html.LoadData(sql);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        int StockOnHand = int.Parse(ds.Tables[0].Rows[0]["StockOnHand"].ToString());
                        if (IsEditInvoice)
                        {
                            sql = @"select QTY from dbo.TblCustomerInvoice 
where InvoiceNo = '" + InvoiceNo + "' and CustomerID =" + CustomerID + " and ItemID = " + ItemID;
                            DataSet ds2 = html.LoadData(sql);
                            if (ds2.Tables.Count > 0)
                            {
                                if (ds2.Tables[0].Rows.Count > 0)
                                {
                                    StockOnHand += int.Parse(ds2.Tables[0].Rows[0]["QTY"].ToString());
                                }
                            }
                        }
                        int QTY = int.Parse(data["QTY"].ToString().Trim());
                        //if (QTY > StockOnHand)
                        //{
                        //	return Json(new
                        //	{
                        //		Message = "You cannot Stock Out Item Name: " + ds.Tables[0].Rows[0]["ItemName"].ToString() + " due to Stock On Hand (" + StockOnHand + ") < Stock Out (" + QTY + ").",
                        //		Result = 1
                        //	}, JsonRequestBehavior.AllowGet);
                        //}
                    }
                }
                foreach (dynamic item in data)
                {
                    if (item.Value != null)
                    {
                        string key = item.Key;
                        string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                        sb.AppendLine("<" + key + ">" + html.EscapeXml(value).ToString().Trim() + "</" + key + ">");
                    }
                }
                sb.AppendLine("<SystemID>" + SystemID + "</SystemID>");
                sb.AppendLine("<WarehouseID>" + WarehouseID + "</WarehouseID>");
                sb.AppendLine("<DomainID>" + DomainID + "</DomainID>");
                //sb.AppendLine("<OtherChargeDate>" + easternTimeNow + "</OtherChargeDate>");
                sb.AppendLine("</CustomerOtherCharge>");
            }
            if (AgencyCommissionObj.Length > 0)
            {
                foreach (dynamic data in AgencyCommissionObj)
                {
                    sb.AppendLine("<AgencyCommission>");
                    foreach (dynamic item in data)
                    {
                        if (item.Value != null)
                        {
                            string key = item.Key;
                            string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                            sb.AppendLine("<" + key + ">" + html.EscapeXml(value).ToString().Trim() + "</" + key + ">");
                        }
                    }
                    sb.AppendLine("<CreateDate>" + html.GetCurrentDate() + "</CreateDate>");
                    sb.AppendLine("<CreateBy>" + html.UserFullName + "</CreateBy>");
                    sb.AppendLine("<SystemID>" + SystemID + "</SystemID>");
                    sb.AppendLine("<WarehouseID>" + WarehouseID + "</WarehouseID>");
                    sb.AppendLine("<DomainID>" + DomainID + "</DomainID>");
                    sb.AppendLine("</AgencyCommission>");
                }
            }
            sb.AppendLine("</Data>");

            if (PreviewInvoice)
            {
                AddCustomerController addCustomerController = new AddCustomerController();
                string strD = addCustomerController.GetPreviewInvoice(sb.ToString());
                result.Result = ResultType.Successful;
                return Json(new { Result = result.Result, Data = strD }, JsonRequestBehavior.AllowGet);
            }

            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "Data", "N'"+sb.ToString()+"'" },
            };
            SqlResult sqlresult = new SqlResult();
            string dbcon = "";
            if (FormStatus == "ToDMC")
            {
                dbcon = html.GetDMCConnectionString(); //"Data Source=192.168.69.83;Initial Catalog=NSM_DB_DMC_Publish;user id=sothea;password=Thea8989; Connection Timeout=900; Pooling=true; Max Pool Size=200; MultipleActiveResultSets=True";
                string sql = "select InvoiceNo from dbo.TblCustomerInvoice where InvoiceNo ='" + InvoiceNo + "'";
                DataSet ds = html.LoadData(sql, false, null, dbcon);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    sqlresult.Result = ResultType.Failed;
                    return Json(new
                    {
                        Message = "Invoice No: " + InvoiceNo + " is exsiting in database.",
                        Result = sqlresult.Result
                    }, JsonRequestBehavior.AllowGet);
                }
                sql = "update dbo.TblCustomerBalance set IsToDMCTAX = 1, CustIDToDMC=" + CustIDToDMC + ", InvoiceToDMC='" + InvoiceToDMC + "' where CustomerID = " + CustIDToDMC + " and InvoiceNo='" + InvoiceToDMC + "'";
                html.SaveChangeData(sql, false, null, false, false, html.GetConnectionString());
            }
            if (ISSO == 0)
            {
                //if (!IsRequestApprovalSO)
                //{
                sqlresult = html.SaveChangeData("SP_INSERT_CUSTOMER", true, Para, true, true, dbcon);
                //}
                //                else
                //                {
                //                    string sql = @"update dbo.TblCustomerInvoice set InvoiceStatusID = 93, 
                //InvoiceStatus = 'Waitting Confirm' where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo;
                //select distinct ci.InvoiceDate, c.CustomerCode, c.CustomerName, c.CustomerID, ci.InvoiceNo
                //from dbo.TblCustomerInvoice ci
                //inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
                //where ci.CustomerID = @CustomerID and ci.InvoiceNo = @InvoiceNo";
                //                    Para = new Dictionary<string, string>
                //                    {
                //                        {"CustomerID", CustomerID},
                //						{"InvoiceNo", RequestApprovalSOInvoiceNo}
                //					};
                //                    sqlresult = html.SaveChangeData(sql, false, Para, true, true);
                //					InvoiceData = html.JsonConvertDataTable(sqlresult.Data.Tables[0]);
                //					return Json(new
                //					{
                //						Message = sqlresult.Message,
                //						Result = sqlresult.Result,
                //						Data = sqlresult.Value,
                //						InvoiceData = InvoiceData,
                //						CustomerName = CustomerName,
                //						InvoiceNo = RequestApprovalSOInvoiceNo,
                //						SupportTicketData = ""
                //					});
                //				}
                if (ISToVATSystem)
                {
                    Para = new Dictionary<string, string>()
                    {
                        { "Data", "N'"+sb.ToString()+"'" },
                        { "NewCID", sqlresult.Data.Tables[0].Rows[0]["CustomerCode"].ToString() },
                        { "NewInvNo", sqlresult.Data.Tables[0].Rows[0]["InvoiceNo"].ToString() },
                    };
                    if (ISSameCID)
                    {
                        string sqlinsert = @"select CustomerType, DistrictCode, Prefix, StartNumber, Memo, CreateBy, CreateDate, OrderBy, [Status], SystemID, WarehouseID, DomainID from dbo.TblCustomerType where CustomerTypeID = " + CustomerDataInfo[0]["CustomerType_CustomerTypeID"].ToString();
                        DataRow dr = html.LoadData(sqlinsert).Tables[0].Rows[0];
                        sqlinsert = @"if(not exists (select CustomerType from TblCustomerType where CustomerType = N'" + dr["CustomerType"] + @"' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + @"%'))
begin
insert into dbo.TblCustomerType (CustomerType, DistrictCode, Prefix, StartNumber, Memo, CreateBy, CreateDate, OrderBy, [Status], SystemID, WarehouseID, DomainID)
values(N'" + dr["CustomerType"] + "',N'" + dr["DistrictCode"] + "',N'" + dr["Prefix"] + "',N'" + dr["StartNumber"] + "',N'" + dr["Memo"] + "','" + dr["CreateBy"] + "','" + dr["CreateDate"] + "'," + dr["OrderBy"] + ",'" + dr["Status"] + "'," + dr["SystemID"] + "," + dr["WarehouseID"] + "," + dr["DomainID"] + ") end";
                        html.SaveChangeData(sqlinsert, false, null, false, false, html.GetDMCConnectionString());
                    }
                    dbcon = html.GetDMCConnectionString();

                    html.SaveChangeData("SP_INSERT_CUSTOMER_TO_VAT", true, Para, true, true, dbcon);
                }
            }
            else if (ISSO == 2)
            {
                Para = new Dictionary<string, string>()
                {
                    { "Data", "N'"+sb.ToString()+"'" },
                    { "ISSO", ISSO.ToString() },
                };
                sqlresult = html.SaveChangeData("SP_INSERT_CUSTOMER", true, Para, true, true);
            }
            else
            {
                Para = new Dictionary<string, string>()
                {
                    { "Data", "N'"+sb.ToString()+"'" },
                    { "ISSO", ISSO.ToString() },
                };
                sqlresult = html.SaveChangeData("SP_INSERT_CUSTOMER_SALE_ORDER", true, Para, true, true);
            }
            if (sqlresult.Result == ResultType.Failed)
            {
                return Json(new
                {
                    Message = sqlresult.Message,
                    Result = sqlresult.Result
                }, JsonRequestBehavior.AllowGet);
            }
            string InvoiceData = html.JsonConvertDataTable(sqlresult.Data.Tables[0]);
            string SupportTicketData = "";
            if (sqlresult.Data.Tables.Count > 1)
            {
                SupportTicketData = html.JsonConvertDataTable(sqlresult.Data.Tables[1]);
            }
            DoActionDataController doActionData = new DoActionDataController();
            await doActionData.AddUserToTelegram(sqlresult.Data.Tables[0].Rows[0]["CustomerID"].ToString(), HttpUtility.HtmlDecode(HttpUtility.UrlDecode(CustContactPhone)), sqlresult.Data.Tables[0].Rows[0]["CustomerName"].ToString(), sqlresult.Data.Tables[0].Rows[0]["CustomerCode"].ToString());
            //NotificationTelegram notificationTelegram = new NotificationTelegram();
            //notificationTelegram.AddCustomerTelegram(int.Parse(sqlresult.Data.Tables[0].Rows[0]["CustomerID"].ToString()), HttpUtility.HtmlDecode(HttpUtility.UrlDecode(CustContactPhone)), HttpUtility.UrlDecode(CustomerDataInfo[0]["CustomerName"]), CustomerDataInfo[0]["CustomerCode"], "", html.GetCurrentDate(), html.UserFullName, html.UserID, html.SystemID, html.WarehouseID, html.DomainID);
            this.SaveChangeCustomerStatus(int.Parse(sqlresult.Data.Tables[0].Rows[0]["CustomerID"].ToString()), CustomerStatus);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value,
                InvoiceData = InvoiceData,
                CustomerName = CustomerName,
                InvoiceNo = InvoiceNo,
                SupportTicketData = SupportTicketData,
                isInternetServices = isInternetServices
            });
        }
        public JsonResult SaveChangeCustomerStatus(int CustomerID, string CustomerStatus)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = @"update dbo.TblCustomer set CustomerStatus = '" + CustomerStatus + "' where CustomerID = " + CustomerID + " and SystemID=" + html.SystemID + " and WarehouseID =" + html.WarehouseID;
            sqlResult = html.SaveChangeData(sql, false, null);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Quotations()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            html.ReportTitle = "List Quotation";
            if (html.HasPermission("Add Quotations"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Quotations"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Quotations"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            // html.AddButton = "Add New";
            // html.AddURL = "Sales/CreateQuotation";
            html.AddButton = "Add New";
            html.AddURL = "Sales/CreateQuotation";
            html.TableName = "TblSales";
            html.PrimaryField = "SalesID";
            html.OrderBy = "PurchaseOrderID desc";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Quote Date:",
                DBField = "s.TranDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Quote Status:",
                DBField = "s.StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });


            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtQuoteNumber",
                ISAutoComplete = true,
                Label = "Quote Number:",
                DBField = "QuoteNumber",
                SQLAutoComplete = "select distinct top 10 QuoteNumber from dbo.TblSales where Type='Quotation' and lower(QuoteNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by QuoteNumber"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerCode",
                ISAutoComplete = true,
                Label = "Customer Code:",
                DBField = "s.CustomerCode",
                SQLAutoComplete = "select distinct top 10 CustomerCode from dbo.TblSales where Type='SaleOrder' and lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerName",
                ISAutoComplete = true,
                Label = "Customer Name:",
                DBField = "s.CustomerName",
                SQLAutoComplete = "select distinct top 10 CustomerName from dbo.TblSales where Type='SaleOrder' and lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName"
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Class Name",
                SQL = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("ClassID"),
                DefaultCheckBok = true,
            });



            string sql = @"select ROW_NUMBER() Over(order by s.ClassID desc, s.QuoteNumber desc) as [No], format(s.TranDate, 'dd-MMM-yyyy') QuoteDate, s.SalesID, '<a href=""javascript:void(0);"" onclick=""showQuoteForm('+convert(varchar, s.SalesID)+', '''+ s.QuoteNumber +''', '''+ s.Type +''')"">'+s.QuoteNumber+'</a>' as QuoteNumber, 
s.ClassName, s.ClassID, s.StatusID, isnull(s.IsToInvoice, 0) IsToInvoice, case when isnull(s.IsToInvoice, 0) <> 0 then '<span class=""text-primary"">Complete</span>' else '<span class=""text-danger"">Pending</span>' end StatusToInvoice , c.CustomerCode, c.CustomerName, case when s.StatusID = 92 then '<span class=""badge badge-pill badge-success text-uppercase"">' + st.[Status] + '</span>' when s.StatusID = 90 then '<span class=""badge badge-pill badge-primary text-white text-uppercase"">' + st.[Status] + '</span>' else '<span class=""badge badge-pill text-uppercase badge-danger text-white"">' + st.[Status] + '</span>' end [Status], 
s.GrandTotal, s.Total, s.DiscountUSD, s.SaleNote, case when isnull(s.NeedCheckByID, 0) <> 0 then s.NeedCheckBy else '<span class=""text-info"">No need check</span>' end NeedCheckBy, s.NeedApproveBy, s.SellerName
from dbo.TblSales s
inner join dbo.TblCustomer c on c.CustomerID = s.CustomerID
inner join dbo.TblStatus st on st.StatusID = s.StatusID
where s.Type = 'Quotation'";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;

            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="s.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="s.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="s.DomainID", Value =html.DomainID },
            };


            //html.ShowSearchButton = false;
            html.GH.SQL = sql;
            html.GridEventClick = "onGridClick(this);";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Quote Date", FieldName = "QuoteDate", TextAlign="center" },
                new GridField() { Header = "Quote Number", FieldName = "QuoteNumber", TextAlign="center"},
                //new GridField() { Header = "Class Name", FieldName = "ClassName" },
                //new GridField() { Header = "Customer Code", FieldName = "CustomerCode", TextAlign="center"},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                //new GridField() { Header = "Sale Name", FieldName = "SellerName"},
                //new GridField() { Header = "Need Check By", FieldName = "NeedCheckBy", TextAlign ="center" },
                //new GridField() { Header = "Need Approve By", FieldName = "NeedApproveBy", TextAlign ="center" },
                //new GridField() { Header = "Status Approval", FieldName = "Status", TextAlign ="center" },
                new GridField() { Header = "Status To Invoice", FieldName = "StatusToInvoice", TextAlign ="center" },
                //new GridField() { Header = "Total", FieldName = "Total", ClassName="text-right", Format="#,#0.00" },
                //new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right", Format="#,#0.00" },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", ClassName="text-right", Format="#,#0.00" },
                //new GridField() { Header = "Balance", FieldName = "RemainAmount", ClassName="text-right text-danger", Format="#,#0.00" },			
            	new GridField() { Header = "Action", FieldName = "SalesID", TextAlign = "Center",  IsModifyField = true,
                    DeleteWithTableName="TblCustomerInvoice",
                    HideEditOnActionNameNotMatch = AdminEdit!=false,
                    //HideEditOnActionNameNotMatch = true,
                    ShowActionNameOnConditionField ="IsToInvoice",
                    ShowActionNameOnConditionValue="0",
                    HideDeteleOnNotMatch=true
                }
            };
            // ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = false
            };

            // html.GH.SQL = sql;
            html.OrderBy = "[No]";

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(
                1,
                "",
                true,
                html.GH.GridTable.ToList().ToJsonString(),
                kendoGridSetting.ToJsonString()
            );

            return View();
        }
        public ActionResult SalesOrder()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            html.ReportTitle = "List Sales Order";
            if (html.HasPermission("Add Sales Order"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Sales Order"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Sales Order"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New";
            html.AddURL = "Sales/CreateSalesOrder";
            html.TableName = "TblSales";
            html.PrimaryField = "SalesID";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "SO Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "SO Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSOType",
                Label = "SO Type:",
                DBField = "SOType",
                SQL_CBO = "select 'VAT' SOType union all select 'CM' SOType union all select 'Non-VAT' SOType",
                DisplayText = "SOType",
                Value = "SOType",
                ComboBox = true,
                Hide = html.SystemID == "25"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSONumber",
                Label = "SO Number:",
                DBField = "SONumber",
                SQLAutoComplete = "select distinct top 10 SONumber from dbo.TblSales where Type='SalesOrder' and lower(SONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by SONumber",
                ISAutoComplete = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ClassName",
                SQL_CBO = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Code",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });

            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
            });

            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "SO Date", FieldName = "SODate", TextAlign="center", FormatDateTime = "dd-MMM-yyyy" },
                new GridField() { Header = "SO Number", FieldName = "SONumber", OnClickFunction = "showSOForm(#=SalesID#, '#=SONumber#', '#=Type#')"},
                new GridField() { Header = "Class Name", FieldName = "ClassName",TextAlign="center", IsHiddenField=true },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID="},
                new GridField() { Header = "Sale Name", FieldName = "SellerName", IsHiddenField=true},
                new GridField() { Header = "Status Approval", FieldName = "Status", TextAlign ="center",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string>
                    {
                        { "Approve", "badge-primary" },
                        { "Draft", "badge-danger" },
                        { "Checked", "badge-warning" },
                        { "Disapprove", "badge-danger" },
                        { "Cancel", "badge-warning" },
                    }
                },
                new GridField() { Header = "DN Status", FieldName = "DNStatus", TextAlign ="center", IsHiddenField= html.SystemID=="27",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string>
                    {
                        { "Complete", "badge-primary" },
                        { "Pending", "badge-danger" },
                    }
                },
                new GridField() { Header = "Status To Invoice", FieldName = "StatusToInvoice", TextAlign ="center",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string>
                    {
                        { "Complete", "badge-primary" },
                        { "Pending", "badge-danger" },
                    }
                },
                new GridField() { Header = "Total", FieldName = "Total", ClassName="text-right", Format="#,#0.00", IsHiddenField=true },
                new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right", Format="#,#0.00", IsHiddenField=true },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", ClassName="text-right", Format="#,#0.00" },
                new GridField() { Header = "Remark", FieldName = "Remark", Width="200px", ClassName="truncate-cell"},
                new GridField() { Header = "Action", FieldName = "SalesID", TextAlign = "Center",  IsModifyField = true,
                    DeleteWithTableName="TblCustomerInvoice",
                    ShowActionNameOnConditionValue="0",
                    ShowActionNameOnConditionField ="IsToInvoice",
                    HideDeteleOnNotMatch=true
                }
            };

            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_SALES_SO_LIST"
            };

            html.OrderBy = "[No]";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        public ActionResult Invoices()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            html.ReportTitle = "List Invoices";
            if (html.HasPermission("Add Invoices"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Invoices"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoices"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New";
            html.AddURL = "Sales/CreateInvoices";
            html.TableName = "TblSales";
            html.PrimaryField = "SalesID";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoices Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Invoice Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoiceType",
                Label = "Invoices Type:",
                DBField = "InvoicesType",
                SQL_CBO = "select 'VAT' InvoicesType union all select 'CM' InvoicesType union all select 'Non-VAT' InvoicesType",
                DisplayText = "InvoicesType",
                Value = "InvoicesType",
                ComboBox = true,
                Hide = html.SystemID == "25"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoicesNumber",
                ISAutoComplete = true,
                Label = "Invoices Number:",
                DBField = "InvoiceNumber",
                SQLAutoComplete = "select distinct top 10 InvoiceNumber from dbo.TblSales where Type='Invoices' and lower(InvoiceNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by InvoiceNumber"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSONumber",
                Label = "SO Number:",
                DBField = "SONumber",
                SQLAutoComplete = "select distinct top 10 SONumber from dbo.TblSales where Type='SalesOrder' and lower(SONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by SONumber",
                ISAutoComplete = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ClassName",
                SQL_CBO = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Code",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
            });
            html.GH.GridPara = new GridPara[]
            {
                new GridPara(){ ParaName="TranType", Value =  "Invoices"},
            };

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoices Date", FieldName = "InvoiceDate", TextAlign="center" },
                new GridField() { Header = "Invoices Number", FieldName = "InvoiceNumber", TextAlign="center"},
                new GridField() { Header = "SO Number", FieldName = "SONumber", },
                new GridField() { Header = "Class Name", FieldName = "ClassName", TextAlign="center" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                new GridField() { Header = "Status Approval", FieldName = "Status", TextAlign ="center" },
                new GridField() { Header = "Paid", FieldName = "Paid", ClassName="text-right text-primary", Format="#,#0.00" },
                new GridField() { Header = "Balance", FieldName = "Balance", ClassName="text-right text-danger", Format="#,#0.00" },
                new GridField() { Header = "Payment Status", FieldName = "PaymentStatus", TextAlign="center" },
                new GridField() { Header = "Payment", FieldName = "Payment", ClassName="text-center" },
                new GridField() { Header = "Action", FieldName = "SalesID", TextAlign = "Center",  IsModifyField = true,
                    DeleteWithTableName="TblCustomerInvoice, TblStockOut, TblStockInDetail, TblSalesPayment",
                    //HideEditOnActionNameNotMatch = true,//(AdminEdit==false? true:false),
                    ShowActionNameOnConditionField ="StatusID",
                    //ShowActionNameOnConditionValue="60",
                    HideDeteleOnNotMatch=true
                }
            };

            KendoGridSetting gridSetting = new KendoGridSetting()
            {
                ServerPaging = true,
                ProcedureName = "SP_SALES_INVOICE_LIST"
            };
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.ShowPaging = false;

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), gridSetting.ToJsonString());
            return View();
        }
        public ActionResult ConvertInvoices()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            html.ReportTitle = "List Convert Invoices";
            if (html.HasPermission("Add Convert Invoices"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Convert Invoices"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Convert Invoices"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New";
            html.AddURL = "Sales/CreateConvertInvoices";
            html.TableName = "TblSales";
            html.PrimaryField = "SalesID";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoices Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Invoices Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true,
                Hide = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCMInvoicesNumber",
                ISAutoComplete = true,
                Label = "CM Invoices Number:",
                DBField = "InvoiceNumber",
                SQLAutoComplete = "select distinct top 10 NewInvoiceNumber InvoiceNumber from dbo.TblSales where Type='ConvertInvoices' and lower(NewInvoiceNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by NewInvoiceNumber"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ClassName",
                SQL_CBO = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Code",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
            });
            html.GH.GridPara = new GridPara[]
            {
                new GridPara(){ ParaName="TranType", Value =  "ConvertInvoices"},
                new GridPara(){ ParaName="InvoicesType", Value =  ""}
            };
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoices Date", FieldName = "InvoiceDate", TextAlign="center" },
                new GridField() { Header = "CM Invoice Number", FieldName = "NewInvoiceNumber", TextAlign="center"},
                new GridField() { Header = "Invoice Number", FieldName = "InvoiceNumber", },
                new GridField() { Header = "Class Name", FieldName = "ClassName", TextAlign="center" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", ClassName="text-right", Format="#,#0.00" },
                new GridField() { Header = "Create By", FieldName = "CreateBy", ClassName="text-center" },
                new GridField() { Header = "Action", FieldName = "SalesID", TextAlign = "Center",  IsModifyField = true,
                    DeleteWithTableName="TblCustomerInvoice",
                    HideEditOnActionNameNotMatch = true,//(AdminEdit==false? true:false),
                    ShowActionNameOnConditionField ="StatusID",
                    //ShowActionNameOnConditionValue="60",
                    HideDeteleOnNotMatch=true
                }
            };

            KendoGridSetting gridSetting = new KendoGridSetting()
            {
                ServerPaging = true,
                ProcedureName = "SP_SALES_INVOICE_LIST"
            };
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), gridSetting.ToJsonString());
            return View();
        }
        public ActionResult DeliveryNote()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            html.ReportTitle = "List Delivery Note";
            if (html.HasPermission("Add Delivery Note"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Delivery Note"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Delivery Note"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New";
            html.AddURL = "Sales/CreateDeliveryNote";
            html.TableName = "TblSales";
            html.PrimaryField = "SalesID";
            //html.OrderBy = "PurchaseOrderID desc";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "DN Date:",
                DBField = "s.TranDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "DN Status:",
                DBField = "s.StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtDNNumber",
                ISAutoComplete = true,
                Label = "DN Number:",
                DBField = "DNNumber",
                SQLAutoComplete = "select distinct top 10 DNNumber from dbo.TblSales where Type='DeliveryNote' and lower(DNNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by DNNumber"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerCode",
                ISAutoComplete = true,
                Label = "Customer Code:",
                DBField = "s.CustomerCode",
                SQLAutoComplete = "select distinct top 10 CustomerCode from dbo.TblSales where Type='SaleOrder' and lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerName",
                ISAutoComplete = true,
                Label = "Customer Name:",
                DBField = "s.CustomerName",
                SQLAutoComplete = "select distinct top 10 CustomerName from dbo.TblSales where Type='SaleOrder' and lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName"
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Class Name",
                SQL = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("ClassID"),
                DefaultCheckBok = true,
            });

            string sql = @"
with tempTotalAMT as(
	select s.SalesID, sum((ci.QTY * ci.UnitPrice)) as TotalNoDis, sum(ci.DiscountUSD) + s.DiscountUSD as tempDis
	from dbo.TblSales s
	inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
	where s.Type = 'DeliveryNote'
	group by s.SalesID, s.DiscountUSD
)
select ROW_NUMBER() Over(order by s.ClassID desc, s.DNNumber desc) as [No], format(s.TranDate, 'dd-MMM-yyyy') DNDate, s.SalesID, '<a href=""javascript:void(0);"" onclick=""showDNForm('+convert(varchar, s.SalesID)+', '+convert(varchar, s.StatusID)+', '''+ s.DNNumber +''', '''+ s.Type +''')"">'+s.DNNumber+'</a>' as DNNumber, s.SONumber, 
case when s.StatusID = 94 then 0 else s.GrandTotal end GrandTotal, tmp.TotalNoDis Total, tmp.tempDis DiscountUSD, s.CreateBy,c.CustomerName, s.SellerName, (select case when count(SalesID)>=1 and s.StatusID=90 then '<span class=""badge badge-primary"">Complete</span>' when count(SalesID)>=1 and s.StatusID=94 then '<span class=""badge badge-warning"">Cancel</span>' else '<span class=""badge badge-danger"">Pending</span>' end from dbo.TblSales where Type='Invoices' and DNNumber = s.DNNumber) StatusToInvoice, 
(select case when count(SalesID)>=1 and s.StatusID=90 then 'Complete' when count(SalesID)>=1 and s.StatusID=94 then 'Cancel' else  'Pending' end from dbo.TblSales where Type='Invoices' and DNNumber = s.DNNumber) IsToInvoice, case when s.StatusID = 90 then '<span class=""badge badge-primary"">Complete</span>' else '<span class=""badge badge-warning"">Cancel</span>' end [Status]
from tempTotalAMT tmp
inner join dbo.TblSales s on s.SalesID = tmp.SalesID
inner join dbo.TblCustomer c on c.CustomerID = s.CustomerID
inner join dbo.TblStatus st on st.StatusID = s.StatusID
where s.Type = 'DeliveryNote'";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="s.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="s.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="s.DomainID", Value =html.DomainID },
            };
            //html.ShowSearchButton = false;
            html.GH.SQL = sql;
            //html.GridEventClick = "onGridClick(this);";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "DN Date", FieldName = "DNDate", TextAlign="center" },
                new GridField() { Header = "DN Number", FieldName = "DNNumber", TextAlign="center"},
                new GridField() { Header = "SO Number", FieldName = "SONumber", TextAlign="center"},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName"},
                new GridField() { Header = "DN Status", FieldName = "Status", TextAlign="center"},
                new GridField() { Header = "Status To Invoice", FieldName = "StatusToInvoice", TextAlign="center"},
                new GridField() { Header = "Total", FieldName = "Total", ClassName="text-right", Format="#,#0.00" },
                new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right", Format="#,#0.00" },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", ClassName="text-right", Format="#,#0.00" },
                new GridField() { Header = "Issue By", FieldName = "CreateBy", TextAlign ="center" },
                new GridField() { Header = "Action", FieldName = "SalesID", TextAlign = "Center",  IsModifyField = true,
                    DeleteWithTableName="TblCustomerInvoice, TblStockOut, TblStockInDetail",
                    //HideEditOnActionNameNotMatch = (AdminEdit==false? false:true),
                    //HideEditOnActionNameNotMatch = true,
                    ShowActionNameOnConditionField ="IsToInvoice",
                    ShowActionNameOnConditionValue="Pending",
                    //HideDeteleOnNotMatch=true
                }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        public ActionResult CreateQuotation(string PrimaryFieldValue, bool Clone = false)
        {
            HTML html = new HTML();
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            html.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.SaleNote = "";
            string sql;
            if (PrimaryFieldValue != "0")
            {
                sql = @"select SaleNote from dbo.TblSales where SalesID = " + PrimaryFieldValue;
                var ds = html.LoadData(sql);
                ViewBag.SaleNote = ds.Tables[0].Rows[0]["SaleNote"].ToString();
            }
            html.isCloned = Clone;
            ViewBag.Clone = Clone;
            html.FormID = "CreateSalesOrder";
            //html.FieldIDFocus = "VendorName";
            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";

            sql = @"select distinct top 10 oc.OtherChargeID ItemID, oc.OtherChargeItem ItemName, sv.ServiceID ItemTypeID, sv.ServiceName ItemType, 'OtherCharge' ItemStatus 
from dbo.TblOtherCharge oc
left join dbo.TblUom um on um.Name = oc.UM and um.SystemID = oc.SystemID
inner join dbo.TblServices sv on sv.ServiceID = oc.ServiceID
where oc.Status = 'Enable' and oc.SystemID = " + html.SystemID + @" 
and oc.WarehouseID = " + html.WarehouseID + " and oc.DomainID like '%" + html.DomainID + @"%' 
and lower(oc.OtherChargeItem) like @SearchText 
union all 
select distinct top 10 i.ItemID, i.ItemName, sv.ServiceID ItemTypeID, sv.ServiceName ItemType, 'NonOtherCharge' ItemStatus
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblServices sv on sv.ServiceID = i.ItemTypeID
where i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " and i.DomainID like '%" + html.DomainID + @"%'
and lower(i.ItemName + case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end) like @SearchText ";
            if (html.SystemID == "27")
            {
                sql += @"and sv.ServiceName = 'Services' ";
            }
            else if (html.SystemID == "25")
            {
                sql += @"and sd.LocationID = @SearchText2
group by i.ItemID, i.ItemName,sv.ServiceID, sv.ServiceName
having (sum(isnull(sd.QTY, 0)) - sum(isnull(sd.QTYOUT, 0))) > 0";
            }

            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblSales",
                    FormTitle="Sales Order Information",
                    GetIDENTITYByFiled="SalesID",
                    PrimaryFieldName="SalesID",
                    ISAddAttachmentFile = false,
                    DestinationFile = "Uploads/Sales/Quotation",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="NeedCheckByID", Value="NeedCheckBy_NeedCheckByID", GetValueFromInputName= true,
                        },
                        new DBField()
                        {
                            DBFieldName="WarehouseName", Value="TxtContactName_WarehouseName", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SubTotal", Value="SubTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VAT", Value="VAT", GetValueFromInputName= true, //IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Total", Value="Total", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountP", Value="TxtDisP", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountUSD", Value="TxtDisUSD", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="GrandTotal", Value="GrandTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                           DBFieldName="Deposit", Value="TxtDeposit", GetValueFromInputName= true, IsNumber = true
                        },
                        //new DBField()
                        //{
                        //    DBFieldName="Balance", Value="TxtBalance", GetValueFromInputName= true, IsNumber = true
                        //},
                        new DBField()
                        {
                            DBFieldName="Remark", Value="Customer_Notes", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="TermCondition", Value="TermsConditions", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SaleNote", Value="SaleNote", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Type", SetValue="Quotation"
                        },
                        new DBField()
                        {
                            DBFieldName="StatusID", SetValue="60"
                        },
                        new DBField()
                        {
                            DBFieldName="IsClone", SetValue=Clone.ToString()
                        },
                        new DBField()
                        {
                            DBFieldName="IsIncludedVAT", SetValue="`+$('#IncludedVAT').val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="IsCommercialINV", SetValue="`+$('#CommercialINV').val()+`"
                        },
                    },
                    SQL=@"update dbo.TblSales set CheckDate = NULL, CheckBy = NULL, CheckByID=NULL, ApproveDate = NULL, ApproveBy=NULL, ApproveByID=NULL, DisapproveBy=NULL, 
DisapproveByID=NULL, DisapproveDate=NULL, DisapproveUsername=NULL, IsCheck=NULL, IsApprove=NULL, IsDisapprove=NULL
where SalesID = @SalesID",
                    UpdateFiledName="SalesID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="SalesID", ID="SalesID", Required=true,
                           Value=PrimaryFieldValue.ToString(), IsReadOnly = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Quote Date", ID="QuoteDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="TranDate", //DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Class Name", ID="ClassName", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ClassName", isTypeahead = true,
                            SQL="select ClassID, ClassName, PrefixQuote from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            HiddenField="ClassID",
                            SQLCallBack="declare @Prefix nvarchar(100), @Module nvarchar(50); select @Prefix = ISNULL(PrefixQuote,'Q-{0000}'), @Module = 'Quote' from dbo.TblClass where ClassID = @ClassID;	exec [SP_SHOW_PREFIX] @Prefix, @Module, "+html.SystemID+", "+html.WarehouseID+", '"+html.DomainID+"';",
                            CallBackToControlID="QuoteNumber, PrefixQuote", UpdatePrefixModule="Quote", Prefix="PrefixQuote", SetPrefixToDbFieldName="QuoteNumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="Quote Number", ID="QuoteNumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            isCloned=Clone,
                            DBField="QuoteNumber", IsReadOnly = true,
                            //SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'SalesOrder'; select @Prefix = Prefix from dbo.TblPrefix where Module = 'SalesOrder' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'BS/SO{yy}{MM}-{0000}'",
                            //CallBackToControlID="InvoiceNumber, PrefixSO", UpdatePrefixModule="SO", Prefix="PrefixSO", SetPrefixToDbFieldName="InvoiceNumber"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                         new HtmlTextBox()
                        {
                            Label="Sale's Name:", ID="SallerName",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SellerName", isTypeahead = true,
                            SQL="select LeadID SellerID, Salutation + ' ' + FullName SellerName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                            HiddenField="SellerID",
                            placeholder=""
                        },
                       new HtmlTextBox()
                        {
                            Label="Customer Name", ID="CustomerName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerName", isTypeahead = true,
                            SQL="select c.CustomerID, c.CustomerCode, ct.CustomerType, c.CustomerName, c.CustomerGender, c.CustomerAddress, c.CustomerMobile, c.CustomerEmail, cm.Commune, ds.District, pc.ProvinceCity from dbo.TblCustomer c inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID left join dbo.TblCommune cm on cm.CommuneID = c.CommuneID\r\nleft join dbo.TblDistrict ds on ds.DistrictID = c.DistrictID\r\nleft join dbo.TblProvinceCity pc on pc.ProvinceCityID = c.ProvinceCityID\r\nwhere c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'",
                            HiddenField="CustomerID",
                            DisplayOnFields= JsonConvert.SerializeObject(new {
                                CustomerCode="CustomerCode",
                                CustomerMobile="CustomerMobile",
                                CustomerType="CustomerType",
                            })
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Code", ID="CustomerCode", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerCode", IsReadOnly=true
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Type", ID="CustomerType", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerType", IsReadOnly=true
                        },
                        
                        //new HtmlTextBox()
                        //{
                        //    Label="Tax", ID="Tax",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    DBField="Tax", isTypeahead = true,
                        //    SQL="select Rate, TaxName + ' [' + convert(varchar, format(Rate, '#')) + '%]' Tax from dbo.TblTax where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by TaxName",
                        //    HiddenFeild="Rate", HidenFieldValue="0", DivClassCol="col-sm"
                        //},
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Phone", ID="CustomerMobile", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerMobile", IsReadOnly=true
                        },
                       new HtmlTextBox()
                        {
                            Label="Location:", ID="WarehouseName",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="WarehouseName", isTypeahead = true,
                            SQL="select uw.WID LocationID, uw.WarehouseName from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and uw.DomainID like '%" + html.DomainID + "%' order by uw.WarehouseName",
                            HiddenField="LocationID",
                            placeholder=""
                        },
                       new HtmlTextBox()
                        {
                            Label="Check By", ID="NeedCheckBy", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedCheckBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedCheckByID, u.TelegramUserName CheckByUsername, u.FullName NeedCheckBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="CheckByUsername, NeedCheckByID",
                            HiddenFieldValue="0,0",
                        },

                        new HtmlTextBox()
                        {
                            Label="Approve By", ID="NeedApproveBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedApproveBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedApproveByID, u.TelegramUserName ApproveUsername, u.FullName NeedApproveBy, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="ApproveUsername, NeedApproveByID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                         new HtmlTextBox()
                         {
                             Label="Exchange Rate", ID="ExchangeRate",
                             Class="input2 CustInfo", IsCustomForm=true,
                             DBField="ExchangeRate",Value="4200",
                             DivClassCol="col-sm",
                         },
                    }
                },
                new  FormData() {
                    //TableName="TblCustomerInvoice",
                    //GetIDENTITYByFiled="CustomerInvoiceID",
                    //PrimaryFieldName="CustomerInvoiceID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            // ScriptCallBackFunction="CallSetAutoComplete_ItemName,TotalAll()",//,ApplySummernote('summernote'),Reload_UM,Reload_AccountName
                            // FooterColspan=2,
                            // SubTotalColspan=5,
                            //ScriptCallBackFunction= html.SystemID == "27"?"CallSetAutoComplete_ItemName, Reload_UM(), calculateTotals()":"CallSetAutoComplete_ItemName, calculateTotals()",
                            ScriptCallBackFunction= html.SystemID == "27"
    ?"CallSetAutoComplete_ItemName, Reload_UM(), TotalAll()"
    :"CallSetAutoComplete_ItemName, TotalAll()",
                            FooterColspan=2,
                            SubTotalColspan= html.SystemID == "27" ? 6:6,
                            CustomerNotesLabel="Remark",
                            //ShowTermsConditions=false,
                            ShowDeposit = true,
                            ShowBalance = false,
                            ShowBankInfo = true,
                            GetIDENTITYByFiled="SalesID",
                            PrimaryFieldName="SalesID",
                            TableName="TblCustomerInvoice",
                            OrderBy="CustomerInvoiceID",
                            ReturnFieldNameValue="SalesID, Type",
                            ReturnFieldNameValueFromTblName = "TblSales",
                            
                            //UpdateValueFiledName="ItemDescription",
                            //UpdateValueFiledNameToTblName="TblPurchaseInfo",
                            //UpdateValueFiledNameConditionFiledName="ItemID",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
                            SQL=@"select cast(ci.QTY as varchar(50)) QTY, cast(ci.UnitPrice as varchar(50)) UnitPrice, 
    cast(ci.DiscountUSD as varchar(50)) DiscountUSD,
    cast(ci.Amount as varchar(50)) Amount,*
    from dbo.TblSales s 
    inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
    where s.SalesID = " + PrimaryFieldValue,
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", Value="ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", Value="UomID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemDescription", Value="ItemName_Description", GetValueFromTextAreaName = true //ISSummerNote = true
                                },
                                new DBField()
                                {
                                    DBFieldName="DiscountP", Value="DisP", GetValueFromInputName= true, IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="DiscountUSD", Value="DisUSD", GetValueFromInputName= true, IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemStatus", Value="ItemStatus", GetValueFromInputName= true,
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>", Label="ល.រ"
                                },
                                new TblTHead()
                                {
                                     Class="col-5", Label="បរិយាយ"
                                },
                                 new TblTHead()
                                 {
                                     Class="col-1", Label="វិមាត្រ", Hide= html.SystemID != "27"
                                 },
                                new TblTHead()
                                {
                                     Label="ខ្នាត", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="បរិមាណ", Class="col-1 text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="ថ្លៃឯកតា"
                                },
                                new TblTHead()
                                {
                                     Class="col-2", Label="បញ្ចុះតម្លៃ", colspan=2
                                },
                                new TblTHead()
                                {
                                     Label="សរុប$", Class="col-1",
                                     CloseTR="</tr>"
                                },
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="Description"
                                },
                                new TblTHead()
                                {
                                    Label="Dimension", Class="col-1"
                                },
                                new TblTHead()
                                {
                                    Label="UOM", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="QTY"
                                },
                                new TblTHead()
                                {
                                     Label="Unit Price", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Dis%"
                                },
                                new TblTHead()
                                {
                                     Label="Dis$"
                                },
                                new TblTHead()
                                {
                                     Label="Amount$",
                                     CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TRNo\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TDNo"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemName", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName','TotalAll');",
                                            //isTypeahead = true,
                                            ISAutoComplete = true,
                                            HiddenField="ItemID", //AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete= sql,
                                            CboValue="WarehouseName_LocationID",
                                            //SQLCallBack ="select distinct iv.UM,iv.UomID, iv.SellingPrice UnitPrice, iv.SellingPrice SalePrice, iv.ItemDescription ItemName_Description from dbo.TblItemVariant iv where iv.ItemID = @SearchText",
                                            DisplayOnFields="ItemStatus",
                                            //SQLTypeaheadCallBack = "select distinct ItemID, UomID, UM, CostPrice UnitPrice, SellingPrice SalePrice from dbo.TblItemVariant where ItemID = @ItemID ",
                                            //DisplayOnFieldsTypeahead="UnitPrice, SalePrice, UomID",
                                            //isApplyTypeaheadOnCallBack=true,
                                            //CallBackToControlID="UM", FieldName = "ItemID",
                                            //CallBackDisplayText="UM",
                                            TypeaheadCallBackFunction="TotalAll",
                                            InputDescription = true, placeholder="Item Name"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="HiddenField", Hide=true,
                                            HiddenField="ItemStatus"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Dimension", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Dimension",
                                            placeholder="E.g., m * m * pcs", IsReadOnly=true,
                                            Hide= html.SystemID=="25"
                                        },
                                        // new HtmlTextBox()
                                        // {
                                        //     Name="UM",
                                        //     Class="input2 CustInfo text-center", IsCustomForm = true,
                                        //     DBField="UM",
                                        //     DivClass="wrap-input3",
                                        //     placeholder="Enter UM", Required = true,
                                        //     HiddenField="UomID",
                                        // },
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM", Required = true,
                                            SQL = html.SystemID == "27" ? "select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]" : "",
                                            isTypeahead= html.SystemID == "27",
                                            HiddenField="UomID",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                            OnKeyupFunction="TotalAll();", Digit="0.000"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UnitPrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UnitPrice", IsAllowInputOnlyNumber =true, Digit="0.00",
                                            OnChangeFunction="TotalAll();",OnKeyupFunction="TotalAll();"
                                        },
                                                                                new HtmlTextBox()
                                        {
                                            Name="DisP", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="DiscountP", Value = "0", IsAllowInputOnlyNumber =true,
                                            OnKeyupFunction="onChangeDisP(this);"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="DiscountUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                            OnKeyupFunction="onChangeDisUSD(this);",
                                            HiddenField="DisP"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Amount", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Amount", IsAllowInputOnlyNumber =true,
                                            OnKeyupFunction="AmountKeyup(this);"
                                        }
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult CreateSalesOrder(string PrimaryFieldValue, bool Clone = false)
        {
            HTML html = new HTML();
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            html.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.SaleNote = "";
            ViewBag.IsCommercialINV = 0;
            ViewBag.IsIncludedVAT = 0;
            ViewBag.IsNonVAT = 0;
            bool IsEdit = false;
            string sql;
            if (PrimaryFieldValue != "0")
            {
                //  sql = @"select SaleNote, isnull(IsCommercialINV, 0) IsCommercialINV from dbo.TblSales where SalesID = " + PrimaryFieldValue;
                sql = @"select SaleNote, isnull(IsIncludedVAT, 0) IsIncludedVAT, isnull(IsCommercialINV, 0) IsCommercialINV, isnull(IsNonVAT, 0) IsNonVAT, isnull(SOID, 0) SOID from dbo.TblSales where SalesID = " + PrimaryFieldValue;
                var ds = html.LoadData(sql);
                ViewBag.SaleNote = ds.Tables[0].Rows[0]["SaleNote"].ToString();
                ViewBag.IsIncludedVAT = ds.Tables[0].Rows[0]["IsIncludedVAT"].ToString();
                ViewBag.IsCommercialINV = ds.Tables[0].Rows[0]["IsCommercialINV"].ToString();
                ViewBag.IsNonVAT = ds.Tables[0].Rows[0]["IsNonVAT"].ToString();
                IsEdit = !Clone;
            }
            html.isCloned = Clone;
            ViewBag.Clone = Clone;
            ViewBag.SystemID = html.SystemID;
            html.FormID = "CreateSalesOrder";
            //html.FieldIDFocus = "VendorName";
            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";

            string defaultExchangeRate = "4200";
            if (html.SystemID == "25" || html.SystemID == "27")
            {
                defaultExchangeRate = "4200";
            }
            else
            {
                string sqlExchange = @"select top 1 ExchangeRate from dbo.TblExchangeRate 
where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ExchangeRateID desc";
                var dtExchange = html.LoadData(sqlExchange);
                if (dtExchange != null && dtExchange.Tables.Count > 0 && dtExchange.Tables[0].Rows.Count > 0)
                {
                    string exRate = dtExchange.Tables[0].Rows[0]["ExchangeRate"]?.ToString();
                    if (!string.IsNullOrEmpty(exRate))
                    {
                        defaultExchangeRate = exRate;
                    }
                }
            }

            sql = @"select distinct top 10 
                    oc.OtherChargeID ItemID, 
                    oc.OtherChargeItem ItemName, 
                    sv.ServiceID ItemTypeID, 
                    sv.ServiceName ItemType, 
                    'OtherCharge' ItemStatus,
                    CAST(0 AS INT) AS Instock  -- Make sure it's same type
                from dbo.TblOtherCharge oc
                left join dbo.TblUom um on um.Name = oc.UM and um.SystemID = oc.SystemID
                inner join dbo.TblServices sv on sv.ServiceID = oc.ServiceID
                where oc.Status = 'Enable' and oc.SystemID = " + html.SystemID + @" 
                and oc.WarehouseID = " + html.WarehouseID + " and oc.DomainID like '%" + html.DomainID + @"%' 
                and lower(oc.OtherChargeItem) like @SearchText 

                union all 

                select distinct top 10 
                    i.ItemID, 
                    i.ItemName, 
                    sv.ServiceID ItemTypeID, 
                    sv.ServiceName ItemType, 
                    'NonOtherCharge' ItemStatus,
                    CAST(SUM(ISNULL(sd.QTY, 0) - ISNULL(sd.QTYOUT, 0)) AS INT) AS Instock
                from dbo.TblItem i
                inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
                inner join dbo.TblServices sv on sv.ServiceID = i.ItemTypeID
                where i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " and i.DomainID like '%" + html.DomainID + @"%'
                and lower(i.ItemName + case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end) like @SearchText ";

            if (html.SystemID == "27")
            {
                sql += @"and sv.ServiceName = 'Services' 
            GROUP BY i.ItemID, i.ItemName, sv.ServiceID, sv.ServiceName";
            }
            else if (html.SystemID == "25")
            {
                sql += @"and sd.LocationID = @SearchText2
            GROUP BY i.ItemID, i.ItemName, sv.ServiceID, sv.ServiceName
            having (sum(isnull(sd.QTY, 0)) - sum(isnull(sd.QTYOUT, 0))) > 0";
            }

            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblSales",
                    FormTitle="Sales Order Information",
                    GetIDENTITYByFiled="SalesID",
                    PrimaryFieldName="SalesID",
                    //ISAddAttachmentFile = true,
                    //DestinationFile = "/Uploads/Sales/SO",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="SubTotal", Value="SubTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VATRate", Value="VATRate", GetValueFromInputName= true,
                        },
                        new DBField()
                        {
                            DBFieldName="VAT", Value="VATAmount", GetValueFromLabelName = true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Total", Value="Total", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountP", Value="TxtDisP", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountUSD", Value="TxtDisUSD", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="GrandTotal", Value="GrandTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                           DBFieldName="Deposit", Value="TxtDeposit", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                           DBFieldName="Balance", Value="TxtBalance", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Remark", Value="Customer_Notes", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="TermCondition", Value="TermsConditions", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SaleNote", Value="SaleNote", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Type", SetValue="SalesOrder"
                        },
                        new DBField()
                        {
                            DBFieldName="StatusID", SetValue="60"
                        },
                        new DBField()
                        {
                            DBFieldName="IsClone", SetValue=Clone.ToString()
                        },
                        new DBField()
                        {
                            DBFieldName="IsUpdate", SetValue=IsEdit.ToString()
                        },
                        new DBField()
                        {
                            DBFieldName="IsIncludedVAT", SetValue="`+$('#IncludedVAT').val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="IsCommercialINV", SetValue="`+$('#CommercialINV').val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="IsNonVAT", SetValue="`+$('#NonVAT').val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="IsToInvoice", SetValue="0"
                        },
                    },
                    SQL=@"update dbo.TblSales set CheckDate = NULL, CheckBy = NULL, CheckByID=NULL, ApproveDate = NULL, ApproveBy=NULL, ApproveByID=NULL, DisapproveBy=NULL, 
                        DisapproveByID=NULL, DisapproveDate=NULL, DisapproveUsername=NULL, IsCheck=NULL, IsApprove=NULL, IsDisapprove=NULL
                        where SalesID = @SalesID",
                    UpdateFiledName="SalesID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="SalesID", ID="SalesID", Required=true,
                           Value=PrimaryFieldValue.ToString(), IsReadOnly = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="SO Date", ID="SODate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="TranDate",
                            SingleDatePicker = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Class Name", ID="ClassName", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ClassName", isTypeahead = true,
                            SQL="select ClassID, ClassName, PrefixSO from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            HiddenField="ClassID",
                            SQLCallBack="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = PrefixSO, @Module = 'SO' from dbo.TblClass where ClassID = @ClassID;	exec [SP_SHOW_PREFIX] @Prefix, @Module, "+html.SystemID+", "+html.WarehouseID+", '"+html.DomainID+"';",
                            CallBackToControlID="SONumber, PrefixSO", UpdatePrefixModule="SO", Prefix="PrefixSO", SetPrefixToDbFieldName="SONumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="SO Number", ID="SONumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            isCloned=Clone,
                            DBField="SONumber", IsReadOnly = true,
                        },
                        // add 
                        new HtmlComboBox()
                        {
                            Label="Quote Number:", ID="SOID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SOID",
                            SQL_CBO = $@"
                                SELECT DISTINCT SalesID AS SOID, QuoteNumber
                                FROM dbo.TblSales
                                WHERE 
                                    Type = 'Quotation' AND StatusID = 90 AND IsApprove = 1
                                    AND SystemID = {html.SystemID}
                                    AND WarehouseID = {html.WarehouseID}
                                ORDER BY QuoteNumber DESC",
                            DisplayText="QuoteNumber",
                            Value="SOID",
                            Multiple = true,
                            DivClass = "pt-1",
                        },
                        new HtmlTextBox()
                        {
                            Label="Quote Number", ID="QuoteNumber", Hide=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="QuoteNumber"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                         new HtmlTextBox()
                        {
                            Label="Sale's Name:", ID="SallerName",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SellerName", isTypeahead = true,
                            SQL="select LeadID SellerID, Salutation + ' ' + FullName SellerName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                            HiddenField="SellerID",
                            placeholder=""
                        },
                       new HtmlTextBox()
                        {
                            Label="Customer Name", ID="CustomerName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerName", isTypeahead = true,
                            SQL=@"select c.CustomerID, c.CustomerCode, ct.CustomerType, c.CustomerName, c.CustomerGender, c.CustomerAddress, c.CustomerMobile, c.CustomerEmail, cm.Commune, ds.District, pc.ProvinceCity 
from dbo.TblCustomer c inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID 
left join dbo.TblCommune cm on cm.CommuneID = c.CommuneID
left join dbo.TblDistrict ds on ds.DistrictID = c.DistrictID
left join dbo.TblProvinceCity pc on pc.ProvinceCityID = c.ProvinceCityID
where (c.CustomerStatus = 'Enable' or isnull(c.CustomerStatus, '') = '') and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'",
                            HiddenField="CustomerID",
                            DisplayOnFields= JsonConvert.SerializeObject(new {
                                CustomerCode="CustomerCode",
                                CustomerMobile="CustomerMobile",
                                CustomerType="CustomerType",
                            })
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Code", ID="CustomerCode", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerCode", IsReadOnly=true
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Type", ID="CustomerType", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerType", IsReadOnly=true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Phone", ID="CustomerMobile",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerMobile", IsReadOnly=true
                        },
                       new HtmlTextBox()
                        {
                            Label="Location:", ID="WarehouseName",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="WarehouseName", isTypeahead = true,
                            SQL="select uw.WID LocationID, uw.WarehouseName from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and uw.DomainID like '%" + html.DomainID + "%' order by uw.WarehouseName",
                            HiddenField="LocationID",
                            placeholder=""
                        },
                       new HtmlTextBox()
                        {
                            Label="Check By", ID="NeedCheckBy",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedCheckBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedCheckByID, u.TelegramUserName CheckByUsername, u.FullName NeedCheckBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="CheckByUsername, NeedCheckByID",
                            HiddenFieldValue="0,0",
                        },

                        new HtmlTextBox()
                        {
                            Label="Approve By", ID="NeedApproveBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedApproveBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedApproveByID, u.TelegramUserName ApproveUsername, u.FullName NeedApproveBy, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="ApproveUsername, NeedApproveByID",
                            HiddenFieldValue="0,0",
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                         new HtmlTextBox()
                         {
                             Label="Exchange Rate", ID="ExchangeRate",
                             Class="input2 CustInfo", IsCustomForm=true,
                             DBField="ExchangeRate", Value = defaultExchangeRate,
                             DivClassCol="col-sm",
                         },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ScriptCallBackFunction= html.SystemID == "27"?"CallSetAutoComplete_ItemName, Reload_UM(), calculateTotals()":"CallSetAutoComplete_ItemName, calculateTotals()",
                            FooterColspan=2,
                            SubTotalColspan= html.SystemID == "27" ? 6:6,
                            CustomerNotesLabel="Remark",
                            ShowBankInfo = true,
                            GetIDENTITYByFiled="SalesID",
                            PrimaryFieldName="SalesID",
                            TableName="TblCustomerInvoice",
                            OrderBy="CustomerInvoiceID",
                            ReturnFieldNameValue="SalesID, Type",
                            ReturnFieldNameValueFromTblName = "TblSales",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
                            SQL=@"select cast(ci.QTY as varchar(50)) QTY, cast(ci.UnitPrice as varchar(50)) UnitPrice, 
                                cast(ci.DiscountUSD as varchar(50)) DiscountUSD,
                                cast(ci.Amount as varchar(50)) Amount,*
                                from dbo.TblSales s 
                                inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
                                where s.SalesID = " + PrimaryFieldValue,
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", Value="ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", Value="UomID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemDescription", Value="ItemName_Description", GetValueFromTextAreaName = true //ISSummerNote = true
                                },
                                new DBField()
                                {
                                    DBFieldName="DiscountP", Value="DisP", GetValueFromInputName= true, IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="DiscountUSD", Value="DisUSD", GetValueFromInputName= true, IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemStatus", Value="ItemStatus", GetValueFromInputName= true,
                                },
                            },
                                tblTHead= new List<TblTHead>()
                                {
                                    new TblTHead()
                                    {
                                        OpenTR="<tr>", Label="ល.រ"
                                    },
                                    new TblTHead()
                                    {
                                         Class="col-5", Label="បរិយាយ"
                                    },
                                    new TblTHead()
                                    {
                                         Class="col-1", Label="វិមាត្រ", Hide= html.SystemID != "27"
                                    },
                                    new TblTHead()
                                    {
                                         Label="ខ្នាត", Class="col-1"
                                    },
                                    new TblTHead()
                                    {
                                         Label="ស្តុកនៅសល់", Class="col-1", Hide= html.SystemID=="27"
                                    },
                                    new TblTHead()
                                    {
                                         Label="បរិមាណ", Class="col-1 text-nowrap",

                                    },
                                    new TblTHead()
                                    {
                                         Label="ថ្លៃឯកតា"
                                    },
                                    new TblTHead()
                                    {
                                         Class="col-2", Label="បញ្ចុះតម្លៃ", colspan=2
                                    },
                                    new TblTHead()
                                    {
                                         Label="សរុប$", Class="col-1",
                                         CloseTR="</tr>"
                                    },
                                    new TblTHead()
                                    {
                                        OpenTR="<tr>",
                                        Label="No."
                                    },
                                    new TblTHead()
                                    {
                                         Label="Description"
                                    },
                                    new TblTHead()
                                    {
                                         Label="Dimension", Hide= html.SystemID!="27"
                                    },
                                    new TblTHead()
                                    {
                                         Label="UOM", Class="text-nowrap"
                                    },
                                         new TblTHead()
                                    {
                                         Label="Available Stock", Class="text-nowrap",Hide= html.SystemID=="27"
                                    },
                                    new TblTHead()
                                    {
                                         Label="QTY"
                                    },
                                    new TblTHead()
                                    {
                                         Label="Unit Price", Class="text-nowrap"
                                    },
                                    new TblTHead()
                                    {
                                         Label="Dis%"
                                    },
                                    new TblTHead()
                                    {
                                         Label="Dis$"
                                    },
                                    new TblTHead()
                                    {
                                         Label="Amount$",
                                         CloseTR="</tr>"
                                    },
                                },
                                tblTBody= new List<TblTBody>()
                                {
                                    new TblTBody()
                                    {
                                        OpenTR="<tr name=\"TRNo\">",
                                        html_Control = new HTMLControl[]
                                        {
                                            new HtmlLabel()
                                            {
                                                Label="1", Align="right", TDName="TDNo"
                                            },
                                            new HtmlTextBox()
                                            {
                                                Name="ItemName", Required=true,
                                                Class="input2 CustInfo", IsCustomForm = true,
                                                DBField="ItemName", DivClass="wrap-input3",
                                                DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName', 'calculateTotals');",
                                                ISAutoComplete = true,
                                                HiddenField="ItemID",
                                                SQL_AutoComplete= sql,
                                                CboValue="WarehouseName_LocationID",
                                                DisplayOnFields="ItemStatus,Instock",
                                                InputDescription = true, placeholder="Item Name"

                                            },
                                            new HtmlTextBox()
                                            {
                                                Name="HiddenField", Hide=true,
                                                HiddenField="ItemStatus"
                                            },
                                            new HtmlTextBox()
                                            {
                                                Name="Dimension", DivClass="wrap-input3 text-center",
                                                Class="input2 CustInfo text-center", IsCustomForm = true,
                                                DBField="Dimension",
                                                placeholder="E.g., m * m * pcs", IsReadOnly=true,
                                                Hide= html.SystemID=="25"
                                            },
                                            new HtmlTextBox()
                                            {
                                                Name="UM",
                                                Class="input2 CustInfo text-center", IsCustomForm = true,
                                                DBField="UM",
                                                DivClass="wrap-input3",
                                                placeholder="Enter UM", Required = true,
                                                SQL = html.SystemID == "27" ? "select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]" : "",
                                                isTypeahead= html.SystemID == "27",
                                                HiddenField="UomID",
                                            },
                                            new HtmlTextBox()
                                            {
                                               Name="Instock",
                                               Class="input2 CustInfo text-center text-danger", IsCustomForm = true,
                                               DBField = "AvailableStock",
                                               DivClass="wrap-input3",
                                               IsReadOnly= true,
                                               Digit="0", Value = "0",
                                               Hide = html.SystemID=="27"

                                            },
                                            new HtmlTextBox()
                                            {
                                                Name="QTY", DivClass="wrap-input3 text-center",
                                                Class="input2 CustInfo text-center", IsCustomForm = true,
                                                DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                                Digit="0.000",
                                            },
                                            new HtmlTextBox()
                                            {
                                                Name="UnitPrice", Required=true, DivClass="wrap-input3",
                                                Class="input2 CustInfo text-center", IsCustomForm = true,
                                                DBField="UnitPrice", IsAllowInputOnlyNumber =true,
                                                Digit="0.00", Value = "0"
                                            },
                                            new HtmlTextBox()
                                            {
                                                Name="DisP", DivClass="wrap-input3",
                                                Class="input2 CustInfo text-center", IsCustomForm = true,
                                                DBField="DiscountP", Value = "0", IsAllowInputOnlyNumber =true,
                                                Digit="0"
                                            },
                                            new HtmlTextBox()
                                            {
                                                Name="DisUSD", DivClass="wrap-input3",
                                                Class="input2 text-center", IsCustomForm = true,
                                                DBField="DiscountUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                                HiddenField="DisP", Digit="0.00"
                                            },
                                            new HtmlTextBox()
                                            {
                                                Name="Amount", DivClass="wrap-input3",
                                                Class="input2 CustInfo text-center", IsCustomForm = true,
                                                DBField="Amount", IsAllowInputOnlyNumber =true,
                                                Digit="0.00", Value = "0"
                                            }
                                        },
                                        CloseTR="</tr>"
                                    }
                                }
                            }
                        }
                    }
                };
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult CreateInvoices(string PrimaryFieldValue, bool Clone = false, string SONumber = "", string SOID = "0")
        {
            HTML html = new HTML();
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.SaleNote = "";
            ViewBag.IsCommercialINV = 0;
            ViewBag.IsEdit = false;
            bool Edit = false;
            string sql;
            if (PrimaryFieldValue != "0")
            {
                html.PrimaryFieldValue = PrimaryFieldValue;
                sql = @"select SaleNote, isnull(IsCommercialINV, 0) IsCommercialINV from dbo.TblSales where SalesID = " + PrimaryFieldValue;
                var ds = html.LoadData(sql);
                ViewBag.SaleNote = ds.Tables[0].Rows[0]["SaleNote"].ToString();
                ViewBag.IsCommercialINV = ds.Tables[0].Rows[0]["IsCommercialINV"].ToString();
                ViewBag.IsEdit = true;
                Edit = !Clone;
            }
            html.isCloned = Clone;
            ViewBag.Clone = Clone;
            ViewBag.SONumber = SONumber;
            ViewBag.SOID = SOID;
            ViewBag.SystemID = html.SystemID;
            html.FormID = "CreateSalesOrder";
            //html.FieldIDFocus = "VendorName";
            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";

            sql = @"select distinct top 10 
                    oc.OtherChargeID ItemID, 
                    oc.OtherChargeItem ItemName, 
                    sv.ServiceID ItemTypeID, 
                    sv.ServiceName ItemType, 
                    'OtherCharge' ItemStatus,
                    0 AS Instock
                from dbo.TblOtherCharge oc
                left join dbo.TblUom um on um.Name = oc.UM and um.SystemID = oc.SystemID
                inner join dbo.TblServices sv on sv.ServiceID = oc.ServiceID
                where oc.Status = 'Enable' and oc.SystemID = " + html.SystemID + @" 
                and oc.WarehouseID = " + html.WarehouseID + " and oc.DomainID like '%" + html.DomainID + @"%' 
                and lower(oc.OtherChargeItem) like @SearchText 

                union all 

                select distinct top 10 
                    i.ItemID, 
                    i.ItemName, 
                    sv.ServiceID ItemTypeID, 
                    sv.ServiceName ItemType, 
                    'NonOtherCharge' ItemStatus,
                    SUM(ISNULL(sd.QTY, 0) - ISNULL(sd.QTYOUT, 0)) AS Instock
                from dbo.TblItem i
                inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
                inner join dbo.TblServices sv on sv.ServiceID = i.ItemTypeID
                where i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " and i.DomainID like '%" + html.DomainID + @"%'
                and lower(i.ItemName + case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end) like @SearchText ";

            if (html.SystemID == "27")
            {
                sql += @"and sv.ServiceName = 'Services' 
                    GROUP BY i.ItemID, i.ItemName, sv.ServiceID, sv.ServiceName";
            }
            else if (html.SystemID == "25")
            {
                sql += @"and sd.LocationID = @SearchText2
                    GROUP BY i.ItemID, i.ItemName, sv.ServiceID, sv.ServiceName
                    having (sum(isnull(sd.QTY, 0)) - sum(isnull(sd.QTYOUT, 0))) > 0";
            }
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblSales",
                    FormTitle="Invoices Information",
                    GetIDENTITYByFiled="SalesID",
                    PrimaryFieldName="SalesID",
                    //ISAddAttachmentFile = true,
                    //DestinationFile = "Uploads/Sales/Invoices",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="SubTotal", Value="SubTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VATRate", Value="VATRate", GetValueFromInputName= true,
                        },
                        new DBField()
                        {
                            DBFieldName="VAT", Value="VATAmount", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Total", Value="Total", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountP", Value="TxtDisP", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountUSD", Value="TxtDisUSD", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="GrandTotal", Value="GrandTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Deposit", Value="TxtDeposit", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Balance", Value="TxtBalance", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Remark", Value="Customer_Notes", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="TermCondition", Value="TermsConditions", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SaleNote", Value="SaleNote", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Type", SetValue="Invoices"
                        },
                        new DBField()
                        {
                            DBFieldName="StatusID", SetValue="60"
                        },
                        new DBField()
                        {
                            DBFieldName="IsClone", SetValue=Clone.ToString()
                        },
                        new DBField()
                        {
                            DBFieldName="IsUpdate", SetValue=Edit.ToString()
                        },
                        new DBField()
                        {
                            DBFieldName="DNID", Value="DNNumber_DNID", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="IsIncludedVAT", SetValue="`+$('#IncludedVAT').val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="IsCommercialINV", SetValue="`+$('#CommercialINV').val()+`"
                        },
                        new DBField()
                        {
                            DBFieldName="IsNonVAT", SetValue="`+$('#NonVAT').val()+`"
                        },
                    },
                    SQL=@"update dbo.TblSales set CheckDate = NULL, CheckBy = NULL, CheckByID=NULL, ApproveDate = NULL, ApproveBy=NULL, ApproveByID=NULL, DisapproveBy=NULL, 
DisapproveByID=NULL, DisapproveDate=NULL, DisapproveUsername=NULL, IsCheck=NULL, IsApprove=NULL, IsDisapprove=NULL
where SalesID = @SalesID",
                    UpdateFiledName="SalesID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="SalesID", ID="SalesID", Required=true,
                           Value=PrimaryFieldValue.ToString(), IsReadOnly = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Invoices Date", ID="InvoicesDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="TranDate", //DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Class Name", ID="ClassName", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ClassName", isTypeahead = true,
                            SQL="select ClassID, ClassName, PrefixInvoice PrefixINV from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            HiddenField="ClassID",
                            SQLCallBack="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = PrefixInvoice, @Module = 'INV' from dbo.TblClass where ClassID = @ClassID;exec [SP_SHOW_PREFIX] @Prefix, @Module, "+html.SystemID+", "+html.WarehouseID+", '"+html.DomainID+"';",
                            CallBackToControlID="InvoiceNumber, PrefixINV", UpdatePrefixModule="INV", Prefix="PrefixINV", SetPrefixToDbFieldName="InvoiceNumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="Invoices Number", ID="InvoiceNumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            isCloned=Clone,
                            DBField="InvoiceNumber", IsReadOnly = true,
                            //SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'SalesOrder'; select @Prefix = Prefix from dbo.TblPrefix where Module = 'SalesOrder' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'BS/SO{yy}{MM}-{0000}'",
                            //CallBackToControlID="InvoiceNumber, PrefixSO", UpdatePrefixModule="SO", Prefix="PrefixSO", SetPrefixToDbFieldName="InvoiceNumber"
                        },
                        new HtmlComboBox()
                        {
                            Label="SO Number:", ID="SOID", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SOID",
                            SQL_CBO = $@"
    SELECT DISTINCT SalesID AS SOID, SONumber
    FROM dbo.TblSales
    WHERE 
        Type = 'SalesOrder' AND StatusID = 90 
        AND (
            ({(html.SystemID == "25" ? "1=1" : "1=0")})
            AND IsToDN = 1
            OR ({(html.SystemID == "27" ? "1=1" : "1=0")})
            AND (IsToDN IS NULL OR IsToDN = 0)
        ) 
        AND (
            ({(Edit ? "1=1" : "1=0")}) 
            AND ISNULL(IsToInvoice, 0) IN (0,1)
            OR ({(Edit ? "1=0" : "1=1")}) 
            AND ISNULL(IsToInvoice, 0) = 0
        )
        AND SystemID = {html.SystemID}
        AND WarehouseID = {html.WarehouseID}
    ORDER BY SONumber DESC",

                            DivClass="pt-1",
                            DisplayText="SONumber",
                            Value="SOID",
                            Multiple = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="SO Number", ID="SONumber", Hide=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SONumber"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                         new HtmlTextBox()
                        {
                            Label="Sale's Name:", ID="SallerName",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SellerName", isTypeahead = true,
                            SQL="select LeadID SellerID, Salutation + ' ' + FullName SellerName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                            HiddenField="SellerID",
                            placeholder=""
                        },
                       new HtmlTextBox()
                        {
                            Label="Customer Name", ID="CustomerName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerName", isTypeahead = true,
                            SQL=@"select c.CustomerID, c.CustomerCode, ct.CustomerType, c.CustomerName, c.CustomerGender, c.CustomerAddress, c.CustomerMobile, c.CustomerEmail, cm.Commune, ds.District, pc.ProvinceCity 
from dbo.TblCustomer c 
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID 
left join dbo.TblCommune cm on cm.CommuneID = c.CommuneID
left join dbo.TblDistrict ds on ds.DistrictID = c.DistrictID
left join dbo.TblProvinceCity pc on pc.ProvinceCityID = c.ProvinceCityID
where (c.CustomerStatus = 'Enable' or isnull(c.CustomerStatus, '') = '') and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'",
                            HiddenField="CustomerID",
                            DisplayOnFields= JsonConvert.SerializeObject(new {
                                CustomerCode="CustomerCode",
                                CustomerMobile="CustomerMobile",
                                CustomerType="CustomerType",
                            })
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Code", ID="CustomerCode", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerCode", IsReadOnly=true
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Type", ID="CustomerType", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerType", IsReadOnly=true
                        },

                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Phone", ID="CustomerMobile", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerMobile", IsReadOnly=true
                        },
                       new HtmlTextBox()
                        {
                            Label="Location:", ID="WarehouseName",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="WarehouseName", isTypeahead = true,
                            SQL="select uw.WID LocationID, uw.WarehouseName from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and uw.DomainID like '%" + html.DomainID + "%' order by uw.WarehouseName",
                            HiddenField="LocationID",
                            placeholder=""
                        },
                       new HtmlTextBox()
                        {
                            Label="Check By", ID="NeedCheckBy", Required = html.SystemID == "27",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedCheckBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedCheckByID, u.TelegramUserName CheckByUsername, u.FullName NeedCheckBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="CheckByUsername, NeedCheckByID",
                            HiddenFieldValue="0,0",
                        },

                        new HtmlTextBox()
                        {
                            Label="Approve By", ID="NeedApproveBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedApproveBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedApproveByID, u.TelegramUserName ApproveUsername, u.FullName NeedApproveBy, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="ApproveUsername, NeedApproveByID",
                            HiddenFieldValue="0,0",
                            //DivClassCol="col-sm"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Exchange Rate", ID="ExchangeRate",
                            Class="input2 CustInfo", IsCustomForm=true,
                            DBField="ExchangeRate", Value="4200"
                            //DivClassCol="col-sm",
                        },
                        new HtmlTextBox()
                        {
                            Label="DN Number", ID="DNNumber", Required = html.SystemID == "25",
                            Class="input2 CustInfo", IsCustomForm=true,
                            DBField="DNNumber",
                            HiddenField="DNID", IsReadOnly = html.SystemID == "25",
                            //DivClassCol="col-sm",
                        },
                        new HtmlTextBox()
                        {
                            Label="Due Date", ID="DueDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="DueDate", //DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                            DefaultShowDateValue=false
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ScriptCallBackFunction= html.SystemID == "27"?"CallSetAutoComplete_ItemName,calculateTotals(), Reload_UM()":"CallSetAutoComplete_ItemName,calculateTotals()",
                            FooterColspan=2,
                            SubTotalColspan= html.SystemID == "27" ? 6:5,
                            CustomerNotesLabel="Remark",
                            ShowBankInfo = true,
                            GetIDENTITYByFiled="SalesID",
                            PrimaryFieldName="SalesID",
                            TableName="TblCustomerInvoice",
                            OrderBy="CustomerInvoiceID",
                            ReturnFieldNameValue="SalesID, Type",
                            ReturnFieldNameValueFromTblName = "TblSales",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
                            SQL=@"select cast(ci.QTY as varchar(50)) QTY, cast(ci.UnitPrice as varchar(50)) UnitPrice, 
                                cast(ci.DiscountUSD as varchar(50)) DiscountUSD,
                                cast(ci.Amount as varchar(50)) Amount,*
                                from dbo.TblSales s 
                                inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
                                where s.SalesID = " + PrimaryFieldValue,
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", Value="ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", Value="UomID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemDescription", Value="ItemName_Description", GetValueFromTextAreaName = true //ISSummerNote = true
                                },
                                new DBField()
                                {
                                    DBFieldName="DiscountP", Value="DisP", GetValueFromInputName= true, IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="DiscountUSD", Value="DisUSD", GetValueFromInputName= true, IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemStatus", Value="ItemStatus", GetValueFromInputName= true,
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>", Label="ល.រ"
                                },
                                new TblTHead()
                                {
                                     Class="col-5", Label="បរិយាយ"
                                },
                                new TblTHead()
                                {
                                     Class="col-1", Label="វិមាត្រ", Hide= html.SystemID!="27"
                                },
                                new TblTHead()
                                {
                                     Label="ខ្នាត", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="បរិមាណ", Class="col-1 text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="ថ្លៃឯកតា"
                                },
                                new TblTHead()
                                {
                                     Class="col-2", Label="បញ្ចុះតម្លៃ", colspan=2
                                },
                                new TblTHead()
                                {
                                     Label="សរុប$", Class="col-1",
                                     CloseTR="</tr>"
                                },
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="Description"
                                },
                                new TblTHead()
                                {
                                     Label="Dimension", Hide= html.SystemID!="27"
                                },
                                new TblTHead()
                                {
                                     Label="UOM", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="QTY"
                                },
                                new TblTHead()
                                {
                                     Label="Unit Price", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Dis%"
                                },
                                new TblTHead()
                                {
                                     Label="Dis$"
                                },
                                new TblTHead()
                                {
                                     Label="Amount$",
                                     CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TRNo\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TDNo"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemName", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName','calculateTotals');",
                                            ISAutoComplete = true,
                                            HiddenField="ItemID", //AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete= sql,
                                            CboValue="WarehouseName_LocationID",
                                            DisplayOnFields="ItemStatus",
                                            TypeaheadCallBackFunction="TotalAll",
                                            InputDescription = true, placeholder="Item Name"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="HiddenField", Hide=true,
                                            HiddenField="ItemStatus"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Dimension", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Dimension",
                                            placeholder="E.g., m * m * pcs",
                                            Hide= html.SystemID != "27"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM", Required = true,
                                            SQL = html.SystemID == "27" ? "select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]" : "",
                                            isTypeahead= html.SystemID == "27",
                                            HiddenField="UomID",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                            Digit="0.000"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UnitPrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UnitPrice", IsAllowInputOnlyNumber =true,
                                            Digit="0.00", Value = "0",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisP", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="DiscountP", IsAllowInputOnlyNumber =true,
                                            Digit="0", Value = "0",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="DiscountUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                            HiddenField="DisP", Digit="0.00"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Amount", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Amount", IsAllowInputOnlyNumber =true,
                                        }
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult CreateConvertInvoices(string PrimaryFieldValue, bool Clone = false, string SONumber = "", string SOID = "0")
        {
            HTML html = new HTML();
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.SaleNote = "";
            ViewBag.IsCommercialINV = 0;
            bool Edit = false;
            string sql;
            if (PrimaryFieldValue != "0")
            {
                html.PrimaryFieldValue = PrimaryFieldValue;
                sql = @"select SaleNote, isnull(IsCommercialINV, 0) IsCommercialINV from dbo.TblSales where SalesID = " + PrimaryFieldValue;
                var ds = html.LoadData(sql);
                ViewBag.SaleNote = ds.Tables[0].Rows[0]["SaleNote"].ToString();
                ViewBag.IsCommercialINV = ds.Tables[0].Rows[0]["IsCommercialINV"].ToString();
                ViewBag.IsEdit = true;
                Edit = !Clone;
            }
            html.isCloned = Clone;
            ViewBag.Clone = Clone;
            ViewBag.SONumber = SONumber;
            ViewBag.SOID = SOID;
            html.FormID = "CreateSalesOrder";
            //html.FieldIDFocus = "VendorName";
            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";

            sql = @"select oc.OtherChargeID ItemID, oc.OtherChargeItem ItemName, sv.ServiceID ItemTypeID, sv.ServiceName ItemType, 'OtherCharge' ItemStatus 
from dbo.TblOtherCharge oc
left join dbo.TblUom um on um.Name = oc.UM and um.SystemID = oc.SystemID
inner join dbo.TblServices sv on sv.ServiceID = oc.ServiceID
where oc.Status = 'Enable' and oc.SystemID = " + html.SystemID + @" 
and oc.WarehouseID = " + html.WarehouseID + " and oc.DomainID like '%" + html.DomainID + @"%' 
and lower(oc.OtherChargeItem) like @SearchText 
union all 
select i.ItemID, i.ItemName, sv.ServiceID ItemTypeID, sv.ServiceName ItemType, 'NonOtherCharge' ItemStatus
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblServices sv on sv.ServiceID = i.ItemTypeID
where i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " and i.DomainID like '%" + html.DomainID + @"%'
and lower(i.ItemName + case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end) like @SearchText ";
            if (html.SystemID == "27")
            {
                sql += @"and sv.ServiceName = 'Services' ";
            }
            else if (html.SystemID == "25")
            {
                sql += @"and sd.LocationID = @SearchText2
group by i.ItemID, i.ItemName,sv.ServiceID, sv.ServiceName
having (sum(isnull(sd.QTY, 0)) - sum(isnull(sd.QTYOUT, 0))) > 0";
            }

            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblSales",
                    FormTitle="Invoices Information",
                    GetIDENTITYByFiled="SalesID",
                    PrimaryFieldName="SalesID",
                    //ISAddAttachmentFile = true,
                    //DestinationFile = "Uploads/Sales/Invoices",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="SubTotal", Value="SubTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VATRate", Value="VATRate", GetValueFromInputName= true,
                        },
                        new DBField()
                        {
                            DBFieldName="VAT", Value="VATAmount", GetValueFromLabelName = true, IsNumber = true,
                        },
                        new DBField()
                        {
                            DBFieldName="Total", Value="Total", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountP", Value="TxtDisP", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountUSD", Value="TxtDisUSD", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="GrandTotal", Value="GrandTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Deposit", Value="TxtDeposit", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Balance", Value="TxtBalance", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Remark", Value="Customer_Notes", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="TermCondition", Value="TermsConditions", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SaleNote", Value="SaleNote", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Type", SetValue="ConvertInvoices"
                        },
                        new DBField()
                        {
                            DBFieldName="StatusID", SetValue="60"
                        },
                        new DBField()
                        {
                            DBFieldName="IsClone", SetValue=Clone.ToString()
                        },
                        new DBField()
                        {
                            DBFieldName="IsUpdate", SetValue=Edit.ToString()
                        },
                    },
                    SQL=@"update dbo.TblSales set CheckDate = NULL, CheckBy = NULL, CheckByID=NULL, ApproveDate = NULL, ApproveBy=NULL, ApproveByID=NULL, DisapproveBy=NULL, 
DisapproveByID=NULL, DisapproveDate=NULL, DisapproveUsername=NULL, IsCheck=NULL, IsApprove=NULL, IsDisapprove=NULL
where SalesID = @SalesID",
                    UpdateFiledName="SalesID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="SalesID", ID="SalesID", Required=true,
                           Value=PrimaryFieldValue.ToString(), IsReadOnly = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Invoices Date", ID="InvoicesDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="TranDate", //DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Class Name", ID="ClassName", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ClassName", isTypeahead = true,
                            SQL="select ClassID, ClassName, PrefixCommercial PrefixFINV from dbo.TblClass where ClassID in (11, 10) and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            HiddenField="ClassID",
                            SQLCallBack="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = PrefixCommercial, @Module = 'FINV' from dbo.TblClass where ClassID = @ClassID;exec [SP_SHOW_PREFIX] @Prefix, @Module, "+html.SystemID+", "+html.WarehouseID+", '"+html.DomainID+"';",
                            CallBackToControlID="NewInvoiceNumber, PrefixFINV", UpdatePrefixModule="FINV", Prefix="PrefixFINV", SetPrefixToDbFieldName="NewInvoiceNumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="CM Invoices Number", ID="NewInvoiceNumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            isCloned=Clone,
                            DBField="NewInvoiceNumber", IsReadOnly = true,
                            //SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'SalesOrder'; select @Prefix = Prefix from dbo.TblPrefix where Module = 'SalesOrder' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'BS/SO{yy}{MM}-{0000}'",
                            //CallBackToControlID="InvoiceNumber, PrefixSO", UpdatePrefixModule="SO", Prefix="PrefixSO", SetPrefixToDbFieldName="InvoiceNumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="Invoices Number", ID="InvoiceNumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="InvoiceNumber", //Value=SONumber,
                            isTypeahead = true,
                            SQL="select distinct SalesID INVID, InvoiceNumber from dbo.TblSales where Type = 'Invoices' and ClassID in (11, 10) and StatusID = 90 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID +" order by InvoiceNumber desc",
                            CallBackFunction="changeInvoiceNumber",
                            HiddenField="INVID, SONumber",
                            HiddenFieldValue="0,0"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                         new HtmlTextBox()
                        {
                            Label="Sale's Name:", ID="SallerName",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SellerName", isTypeahead = true,
                            SQL="select LeadID SellerID, Salutation + ' ' + FullName SellerName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                            HiddenField="SellerID",
                            placeholder=""
                        },
                       new HtmlTextBox()
                        {
                            Label="Customer Name", ID="CustomerName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerName", isTypeahead = true,
                            SQL=@"select c.CustomerID, c.CustomerCode, ct.CustomerType, c.CustomerName, c.CustomerGender, c.CustomerAddress, c.CustomerMobile, c.CustomerEmail, cm.Commune, ds.District, pc.ProvinceCity 
from dbo.TblCustomer c 
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID 
left join dbo.TblCommune cm on cm.CommuneID = c.CommuneID
left join dbo.TblDistrict ds on ds.DistrictID = c.DistrictID
left join dbo.TblProvinceCity pc on pc.ProvinceCityID = c.ProvinceCityID
where (c.CustomerStatus = 'Enable' or isnull(c.CustomerStatus, '') = '') and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'",
                            HiddenField="CustomerID",
                            DisplayOnFields= JsonConvert.SerializeObject(new {
                                CustomerCode="CustomerCode",
                                CustomerMobile="CustomerMobile",
                                CustomerType="CustomerType",
                            })
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Code", ID="CustomerCode", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerCode", IsReadOnly=true
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Type", ID="CustomerType", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerType", IsReadOnly=true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Phone", ID="CustomerMobile", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerMobile", IsReadOnly=true
                        },
                       new HtmlTextBox()
                        {
                            Label="Location:", ID="WarehouseName",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="WarehouseName", isTypeahead = true,
                            SQL="select uw.WID LocationID, uw.WarehouseName from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and uw.DomainID like '%" + html.DomainID + "%' order by uw.WarehouseName",
                            HiddenField="LocationID",
                            placeholder=""
                        },
                       new HtmlTextBox()
                        {
                            Label="Check By", ID="NeedCheckBy", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedCheckBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedCheckByID, u.TelegramUserName CheckByUsername, u.FullName NeedCheckBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="CheckByUsername, NeedCheckByID",
                            HiddenFieldValue="0,0", Hide=true
                        },

                        new HtmlTextBox()
                        {
                            Label="Approve By", ID="NeedApproveBy", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedApproveBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedApproveByID, u.TelegramUserName ApproveUsername, u.FullName NeedApproveBy, u.Job ApprovedByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="ApproveUsername, NeedApproveByID",
                            HiddenFieldValue="0,0", Hide=true
                            //DivClassCol="col-sm"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                         new HtmlTextBox()
                         {
                             Label="Exchange Rate", ID="ExchangeRate",
                             Class="input2 CustInfo", IsCustomForm=true,
                             DBField="ExchangeRate", Value="4200"
                             //DivClassCol="col-sm",
                         },
                         new HtmlTextBox()
                        {
                            Label="Due Date", ID="DueDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="DueDate", //DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                            DefaultShowDateValue=false
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName,calculateTotals()",
                            FooterColspan=2,
                            SubTotalColspan= html.SystemID == "27" ? 6 : 5,
                            CustomerNotesLabel="Remark",
                            ShowBankInfo = true,
                            GetIDENTITYByFiled="SalesID",
                            PrimaryFieldName="SalesID",
                            TableName="TblCustomerInvoice",
                            OrderBy="CustomerInvoiceID",
                            ReturnFieldNameValue="SalesID, Type",
                            ReturnFieldNameValueFromTblName = "TblSales",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
                            SQL=@"select cast(ci.QTY as varchar(50)) QTY, cast(ci.UnitPrice as varchar(50)) UnitPrice, 
                                cast(ci.DiscountUSD as varchar(50)) DiscountUSD,
                                cast(ci.Amount as varchar(50)) Amount,*
                                from dbo.TblSales s 
                                inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
                                where s.SalesID = " + PrimaryFieldValue,
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", Value="ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", Value="UomID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemDescription", Value="ItemName_Description", GetValueFromTextAreaName = true //ISSummerNote = true
                                },
                                new DBField()
                                {
                                    DBFieldName="DiscountP", Value="DisP", GetValueFromInputName= true, IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="DiscountUSD", Value="DisUSD", GetValueFromInputName= true, IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemStatus", Value="ItemStatus", GetValueFromInputName= true,
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>", Label="ល.រ"
                                },
                                new TblTHead()
                                {
                                     Class="col-5", Label="បរិយាយ"
                                },
                                new TblTHead()
                                {
                                     Class="col-1", Label="វិមាត្រ", Hide= html.SystemID!="27"
                                },
                                new TblTHead()
                                {
                                     Label="ខ្នាត", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="បរិមាណ", Class="col-1 text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="ថ្លៃឯកតា"
                                },
                                new TblTHead()
                                {
                                     Class="col-2", Label="បញ្ចុះតម្លៃ", colspan=2
                                },
                                new TblTHead()
                                {
                                     Label="សរុប$", Class="col-1",
                                     CloseTR="</tr>"
                                },
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="Description"
                                },
                                new TblTHead()
                                {
                                        Label="Dimension", Hide= html.SystemID!="27"
                                },
                                new TblTHead()
                                {
                                     Label="UOM", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="QTY"
                                },
                                new TblTHead()
                                {
                                     Label="Unit Price", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Dis%"
                                },
                                new TblTHead()
                                {
                                     Label="Dis$"
                                },
                                new TblTHead()
                                {
                                     Label="Amount$",
                                     CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TRNo\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TDNo"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemName", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName','calculateTotals');",
                                            ISAutoComplete = true,
                                            HiddenField="ItemID", //AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete= sql,
                                            CboValue="WarehouseName_LocationID",
                                            DisplayOnFields="ItemStatus",
                                            TypeaheadCallBackFunction="TotalAll",
                                            InputDescription = true, placeholder="Item Name"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="HiddenField", Hide=true,
                                            HiddenField="ItemStatus"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Dimension", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Dimension",
                                            placeholder="E.g., m * m * pcs", IsReadOnly=true,
                                            Hide= html.SystemID != "27"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM", Required = true,
                                            HiddenField="UomID",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                            Digit="0.000"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UnitPrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UnitPrice", IsAllowInputOnlyNumber =true,
                                            Digit="0.00", Value = "0"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisP", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="DiscountP", Value = "0", IsAllowInputOnlyNumber =true,
                                            Digit="0"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="DiscountUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                            HiddenField="DisP", Digit="0.00"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Amount", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Amount", IsAllowInputOnlyNumber =true,
                                            Digit="0.00"
                                        }
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public ActionResult CreateDeliveryNote(string PrimaryFieldValue, bool Clone = false)
        {
            HTML html = new HTML();
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            html.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.SaleNote = "";
            bool IsEdit = false;
            string sql;
            if (PrimaryFieldValue != "0")
            {
                sql = @"select SaleNote from dbo.TblSales where SalesID = " + PrimaryFieldValue;
                var ds = html.LoadData(sql);
                ViewBag.SaleNote = ds.Tables[0].Rows[0]["SaleNote"].ToString();
                IsEdit = !Clone;
            }
            html.isCloned = Clone;
            ViewBag.Clone = Clone;
            html.FormID = "CreateSalesOrder";
            //html.FieldIDFocus = "VendorName";
            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";

            sql = @"select distinct top 10 oc.OtherChargeID ItemID, oc.OtherChargeItem ItemName, sv.ServiceID ItemTypeID, sv.ServiceName ItemType, 'OtherCharge' ItemStatus, 0 as QOH
from dbo.TblOtherCharge oc
left join dbo.TblUom um on um.Name = oc.UM and um.SystemID = oc.SystemID
inner join dbo.TblServices sv on sv.ServiceID = oc.ServiceID
where oc.Status = 'Enable' and oc.SystemID = " + html.SystemID + @" 
and oc.WarehouseID = " + html.WarehouseID + " and oc.DomainID like '%" + html.DomainID + @"%' 
and lower(oc.OtherChargeItem) like @SearchText 
union all 
select distinct top 10 i.ItemID, i.ItemName, sv.ServiceID ItemTypeID, sv.ServiceName ItemType, 'NonOtherCharge' ItemStatus, sum(isnull(sd.QTY, 0)) - sum(isnull(sd.QTYOUT, 0)) as QOH
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblServices sv on sv.ServiceID = i.ItemTypeID
where i.SystemID = " + html.SystemID + " and i.WarehouseID = " + html.WarehouseID + " and i.DomainID like '%" + html.DomainID + @"%'
and lower(i.ItemName + case when isnull(i.ItemCode,'') <> '' then ' - ' + isnull(i.ItemCode,'') else '' end) like @SearchText ";
            if (html.SystemID == "27")
            {
                sql += @"and sv.ServiceName = 'Services' ";
            }
            else if (html.SystemID == "25")
            {
                sql += @"and sd.LocationID = @SearchText2
group by i.ItemID, i.ItemName,sv.ServiceID, sv.ServiceName
having (sum(isnull(sd.QTY, 0)) - sum(isnull(sd.QTYOUT, 0))) > 0";
            }

            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblSales",
                    FormTitle="Sales Order Information",
                    GetIDENTITYByFiled="SalesID",
                    PrimaryFieldName="SalesID",
                    //ISAddAttachmentFile = true,
                    //DestinationFile = "/Uploads/Sales/SO",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="NeedCheckByID", Value="NeedCheckBy_NeedCheckByID", GetValueFromInputName= true,
                        },
                        new DBField()
                        {
                            DBFieldName="WarehouseName", Value="TxtContactName_WarehouseName", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SubTotal", Value="SubTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="VAT", Value="VAT", GetValueFromLabelName= true, //IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Total", Value="Total", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountP", Value="TxtDisP", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="DiscountUSD", Value="TxtDisUSD", GetValueFromInputName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="GrandTotal", Value="GrandTotal", GetValueFromLabelName= true, IsNumber = true
                        },
                        new DBField()
                        {
                            DBFieldName="Remark", Value="Customer_Notes", GetValueFromTextAreaName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Type", SetValue="DeliveryNote"
                        },
                        new DBField()
                        {
                            DBFieldName="StatusID", SetValue="90"
                        },
                        new DBField()
                        {
                            DBFieldName="IsClone", SetValue=Clone.ToString()
                        },
                        new DBField()
                        {
                            DBFieldName="IsUpdate", SetValue=IsEdit.ToString()
                        },
                    },
                    SQL=@"update dbo.TblSales set CheckDate = NULL, CheckBy = NULL, CheckByID=NULL, ApproveDate = NULL, ApproveBy=NULL, ApproveByID=NULL, DisapproveBy=NULL, 
DisapproveByID=NULL, DisapproveDate=NULL, DisapproveUsername=NULL, IsCheck=NULL, IsApprove=NULL, IsDisapprove=NULL
where SalesID = @SalesID",
                    UpdateFiledName="SalesID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="SalesID", ID="SalesID", Required=true,
                           Value=PrimaryFieldValue.ToString(), IsReadOnly = true, Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="DN Date", ID="DNDate", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="TranDate", //DivClassCol="col-sm-auto",
                            SingleDatePicker = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Class Name", ID="ClassName", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ClassName", isTypeahead = true, IsReadOnly = true,
                            HiddenField="ClassID",
                            Value="BSM", HiddenFieldValue="10"
                        },
                        new HtmlTextBox()
                        {
                            Label="DN Number", ID="DNNumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            isCloned=Clone,
                            DBField="DNNumber", IsReadOnly = true,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50) = 'DN'; select @Prefix = Prefix from dbo.TblPrefix where Module = 'DN' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"', 1, '', 0, 'DN{yy}{MM}-{00000}'",
                            CallBackToControlID="DNNumber, PrefixDN", UpdatePrefixModule="DN", Prefix="PrefixDN", SetPrefixToDbFieldName="DNNumber"
                        },
                        new HtmlTextBox()
                        {
                            Label="SO Number.", ID="SONumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SONumber", //Value=SONumber,
                            isTypeahead = true, Required=true,
                            SQL="select distinct SalesID SOID, SONumber from dbo.TblSales where Type = 'SalesOrder' and isnull(IsToDN, 0) = 0  and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID +" order by SalesID desc",
                            CallBackFunction="changeSONumber", HiddenField="SOID"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Customer Name", ID="CustomerName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerName", isTypeahead = true,
                            SQL=@"select c.CustomerID, c.CustomerCode, ct.CustomerType, c.CustomerName, c.CustomerGender, c.CustomerAddress, c.CustomerMobile, c.CustomerEmail, cm.Commune, ds.District, pc.ProvinceCity 
from dbo.TblCustomer c inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID 
left join dbo.TblCommune cm on cm.CommuneID = c.CommuneID
left join dbo.TblDistrict ds on ds.DistrictID = c.DistrictID
left join dbo.TblProvinceCity pc on pc.ProvinceCityID = c.ProvinceCityID
where (c.CustomerStatus = 'Enable' or isnull(c.CustomerStatus, '') = '') and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'",
                            HiddenField="CustomerID",
                            DisplayOnFields= JsonConvert.SerializeObject(new {
                                CustomerCode="CustomerCode",
                                CustomerMobile="CustomerMobile",
                                CustomerType="CustomerType",
                            })
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Code", ID="CustomerCode", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerCode", IsReadOnly=true
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Type", ID="CustomerType", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerType", IsReadOnly=true
                        },
                        new HtmlTextBox()
                        {
                            Label="Phone", ID="CustomerMobile", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerMobile", IsReadOnly=true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Location:", ID="WarehouseName",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="WarehouseName", isTypeahead = true,
                            SQL="select uw.WID LocationID, uw.WarehouseName from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and uw.DomainID like '%" + html.DomainID + "%' order by uw.WarehouseName",
                            HiddenField="LocationID",
                            placeholder=""
                        },
                        new HtmlTextBox()
                        {
                            Label="Prepared By", ID="PreparedBy", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PreparedBy", isTypeahead = true,
                            SQL="select u.UserLoginID PreparedByID, u.TelegramUserName PreparedByUsername, u.FullName PreparedBy\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="PreparedByUsername, PreparedByID",
                            HiddenFieldValue="0,0",
                        },
                        new HtmlTextBox()
                        {
                            Label="Delivery By", ID="DeliveryBy", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="DeliveryBy", isTypeahead = true,
                            SQL="select u.UserLoginID DeliveryByID, u.TelegramUserName DeliveryByUsername, u.FullName DeliveryBy\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="DeliveryByUsername, DeliveryByID",
                            HiddenFieldValue="0,0",Hide=true
                        },
                        new HtmlTextBox()
                        {
                            Label="Check By", ID="NeedCheckBy", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="NeedCheckBy", isTypeahead = true,
                            SQL="select u.UserLoginID NeedCheckByID, u.TelegramUserName CheckByUsername, u.FullName NeedCheckBy, u.Job CheckVerifyByPosition\r\nfrom dbo.TblUserLogin u\r\ninner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID\r\nwhere u.StatusID = 3 and ts.SystemID = "+html.SystemID+" and ts.WarehouseID = "+ html.WarehouseID + " order by u.FullName",
                            HiddenField="CheckByUsername, NeedCheckByID",
                            HiddenFieldValue="0,0",Hide=true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ScriptCallBackFunction= "CallSetAutoComplete_ItemName,calculateTotals()",
                            FooterColspan=2,
                            SubTotalColspan= 7,
                            CustomerNotesLabel="Remark",
                            ShowTermsConditions=false,
                            ShowDeposit = false,
                            ShowBalance = false,
                            ShowBankInfo = false,
                            GetIDENTITYByFiled="SalesID",
                            PrimaryFieldName="SalesID",
                            TableName="TblCustomerInvoice",
                            OrderBy="CustomerInvoiceID",
                            ReturnFieldNameValue="SalesID, Type",
                            ReturnFieldNameValueFromTblName = "TblSales",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo",
                            TDName="TDNo",
                            SQL=@"select cast(ci.QTY as varchar(50)) QTY, cast(ci.UnitPrice as varchar(50)) UnitPrice, 
                                cast(ci.DiscountUSD as varchar(50)) DiscountUSD,
                                cast(ci.Amount as varchar(50)) Amount,*
                                from dbo.TblSales s 
                                inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
                                where s.SalesID = " + PrimaryFieldValue,
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="ItemID", Value="ItemID"
                                },
                                new DBField()
                                {
                                    DBFieldName="UomID", Value="UomID"
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemDescription", Value="ItemName_Description", GetValueFromTextAreaName = true //ISSummerNote = true
                                },
                                new DBField()
                                {
                                    DBFieldName="DiscountP", Value="DisP", GetValueFromInputName= true, IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="DiscountUSD", Value="DisUSD", GetValueFromInputName= true, IsNumber = true
                                },
                                new DBField()
                                {
                                    DBFieldName="ItemStatus", Value="ItemStatus", GetValueFromInputName= true,
                                },
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="Description", Class="col-5",
                                },
                                new TblTHead()
                                {
                                     Label="Expire Date", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="QTY Available", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="UOM", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="QTY OUT"
                                },
                                new TblTHead()
                                {
                                     Label="Unit Price", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Dis%"
                                },
                                new TblTHead()
                                {
                                     Label="Dis$"
                                },
                                new TblTHead()
                                {
                                     Label="Amount$",
                                     CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TRNo\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TDNo"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemName", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ItemName','calculateTotals');",
                                            //isTypeahead = true,
                                            ISAutoComplete = true,
                                            HiddenField="ItemID", //AddButtonFunction="AddNewItem()",
                                            SQL_AutoComplete= sql,
                                            CboValue="WarehouseName_LocationID",
                                            DisplayOnFields="ItemStatus, QOH",
                                            InputDescription = true, placeholder="Item Name"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="HiddenField", Hide=true,
                                            HiddenField="ItemStatus"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ExpireDate",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="StockExpireDate",placeholder="Enter Expire Date",
                                            DivClass="wrap-input3"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QOH",Class="input2 CustInfo text-center text-red",
                                            DBField="QOH", IsCustomForm = true,
                                            DivClass="wrap-input3", IsReadOnly=true, Value="0",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM", Required = true,
                                            SQL = html.SystemID == "27" ? "select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]" : "",
                                            isTypeahead= html.SystemID == "27",
                                            HiddenField="UomID",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                            Digit="0.000",
                                            placeholder="Enter QTY OUT",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UnitPrice", Required=true, DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UnitPrice", IsAllowInputOnlyNumber =true,
                                            Digit="0.00", Value = "0",
                                            placeholder="Enter Unit Price",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisP", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="DiscountP", Value = "0", IsAllowInputOnlyNumber =true,
                                            placeholder="Enter Discount(%)", Digit="0"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="DiscountUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                            HiddenField="DisP", placeholder="Enter Discount($)", Digit="0.00"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Amount", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Amount", IsAllowInputOnlyNumber =true,
                                            placeholder="Enter Amount", Digit="0.00"
                                        }
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public JsonResult LoadItemInfo(int ItemID, string ItemStatus = "")
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ItemID", ItemID.ToString() }
            };
            string sql;
            if (ItemStatus == "OtherCharge")
            {
                sql = @"select oc.OtherChargeID ItemID, oc.OtherChargeItem ItemName, '' ItemSize, um.UomID, oc.UM, oc.Fee SalePrice, '' OtherField, 0 as QOH
from dbo.TblOtherCharge oc
left join dbo.TblUom um on um.Name = oc.UM and um.SystemID = oc.SystemID
where oc.OtherChargeID = @ItemID";
            }
            else if (ItemStatus == "NonOtherCharge")
            {
                sql = @"
select i.ItemID, i.ItemName, i.ItemSize, iv.UomID, iv.UM, iv.SellingPrice SalePrice, i.OtherField, sum(isnull(sd.QTY, 0)) - sum(isnull(sd.QTYOUT, 0)) as QOH
from dbo.TblItem i
inner join dbo.TblStockInDetail sd on sd.ItemID = i.ItemID
inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID
where i.ItemID = @ItemID
group by i.ItemID, i.ItemName, i.ItemSize, iv.UomID, iv.UM, iv.SellingPrice, i.OtherField";
            }
            else
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }

            var result = html.JsonData(sql, false, para);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadPrefix(int ClassID, string ClassName)
        {
            HTML html = new HTML();
            string sql = @"declare @Prefix nvarchar(50), @Module nvarchar(50), @StartNumber int;
select @Prefix = PrefixSO, @Module = 'SO', @StartNumber = 1
from dbo.TblClass where StatusID = 40 and ClassID = " + ClassID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '" + html.DomainID + @"';
if(exists(select * from dbo.TblPrefix where Prefix = @Prefix and Module = @Module and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '" + html.DomainID + @"'))
	begin 
		select @Prefix = Prefix, @Module = Module, @StartNumber = isnull(CurrentNumber, 1) from dbo.TblPrefix where Prefix = @Prefix and Module = @Module and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '" + html.DomainID + @"'
	end;
exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ",'" + html.DomainID + "'," + @"@StartNumber
select @Prefix";
            return Json(new { data = html.JsonData(sql, false, null, false, "") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCompanyInfo()
        {
            HTML html = new HTML();
            string SQL = @"select top 1 CompanyID, Logo, TermConditionVAT, TermConditionNonVAT, CompanyAddress, Phone, Email, Website, QuoteTermConditionNonVAT, QuoteTermConditionVAT 
from dbo.TblCompany 
where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadExchangeRate()
        {
            HTML html = new HTML();
            string SQL = (html.SystemID == "25" || html.SystemID == "27")
                ? "select 4200 as ExchangeRate"
                : @"select top 1 ExchangeRate from dbo.TblExchangeRate 
where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ExchangeRateID desc";
            var data = html.JsonData(SQL);
            if (string.IsNullOrEmpty(data) || data == "[]")
            {
                string fallbackRate = "4200";
                data = "[{\"ExchangeRate\":\"" + fallbackRate + "\"}]";
            }
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadSO(string SONumber, string Type)
        {
            HTML html = new HTML();
            // Split the incoming SONumber into an array of values
            var soNumbers = SONumber.Split(',').Select(x => $"'{x}'").ToList();
            // Join the array into a string for the SQL IN clause
            string formattedSONumber = string.Join(",", soNumbers);
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SONumber", SONumber},
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            string SQL = Type == "DeliveryNote"
                ? $@"select s.SalesID, s.ClassID, s.ClassName, s.SONumber, s.SOID, s.SellerID, s.SellerName, s.CustomerID, s.CustomerName, s.CustomerCode, 
s.CustomerType, s.CustomerMobile, s.LocationID, s.WarehouseName, s.NeedCheckBy, s.NeedCheckByID, s.CheckByUsername, s.NeedApproveBy, s.NeedApproveByID, s.ApproveUsername, s.ExchangeRate, 
s.DueDate, s.Remark, s.SaleNote, s.TermCondition, ci.UM, ci.UomID, ci.ItemName, ci.ItemDescription, ci.ItemID, ci.ItemStatus, format(ci.QTY, 'N3') QTY, cast(ci.UnitPrice as varchar(50)) UnitPrice, 
cast(ci.DiscountUSD as varchar(50)) DiscountUSD, ci.DiscountP,
cast(ci.Amount as varchar(50)) Amount, 
case when ci.ItemStatus <> 'OtherCharge' then sum(isnull(sd.QTY, 0) - isnull(sd.QTYOUT, 0)) else 0 end as QOH
from dbo.TblSales s 
inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
left join dbo.TblStockInDetail sd on sd.ItemID = ci.ItemID
where s.SONumber in ({formattedSONumber}) and s.Type='SalesOrder' and s.SystemID = @SystemID and s.WarehouseID = @WarehouseID and s.DomainID = @DomainID
group by s.SalesID, s.ClassID, s.ClassName, s.SONumber, s.SOID, s.SellerID, s.SellerName, s.CustomerID, s.CustomerName, s.CustomerCode, ci.DiscountP,
s.CustomerType, s.CustomerMobile, s.LocationID, s.WarehouseName, s.NeedCheckBy, s.NeedCheckByID, s.CheckByUsername, s.NeedApproveBy, s.NeedApproveByID, s.ApproveUsername, s.ExchangeRate, 
s.DueDate, s.Remark, s.SaleNote, s.TermCondition, ci.UM, ci.UomID, ci.ItemName, ci.ItemDescription, ci.ItemID, ci.ItemStatus, ci.QTY, ci.UnitPrice, ci.DiscountP, ci.DiscountUSD, ci.Amount;"
                : $@"select s.SalesID, s.ClassID, s.ClassName, s.SONumber, s.SOID, s.SellerID, s.SellerName, s.CustomerID, s.CustomerName, s.CustomerCode, 
s.CustomerType, s.CustomerMobile, s.LocationID, s.WarehouseName, s.NeedCheckBy, s.NeedCheckByID, s.CheckByUsername, s.NeedApproveBy, s.NeedApproveByID, s.ApproveUsername, s.ExchangeRate, 
s.DueDate, s.Remark, s.SaleNote, s.TermCondition, format(sd.QTY, 'N3') QTY, cast(sd.UnitPrice as varchar(50)) UnitPrice, 
cast(sd.DiscountUSD as varchar(50)) DiscountUSD, s.IsCommercialINV,
cast(sd.Amount as varchar(50)) Amount, sd.*
from dbo.TblSales s 
inner join dbo.TblCustomerInvoice sd on sd.SalesID = s.SalesID
where s.SONumber in ({formattedSONumber}) and s.Type='SalesOrder' and s.SystemID = @SystemID and s.WarehouseID = @WarehouseID and s.DomainID = @DomainID;";
            string sql = $@"select SalesID, SONumber, DNNumber from dbo.TblSales 
where SONumber in ({formattedSONumber}) and Type = 'DeliveryNote' and SystemID = @SystemID and WarehouseID = @WarehouseID and DomainID = @DomainID";

            return Json(new { data = html.JsonData(SQL, false, para), data2 = html.JsonData(sql, false, para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadQuotations(string SalesIDs)
        {
            HTML html = new HTML();
            if (string.IsNullOrEmpty(SalesIDs))
                return Json(new { data = "[]" }, JsonRequestBehavior.AllowGet);

            // Validate and format to prevent SQL injection
            var ids = SalesIDs.Split(',')
                              .Select(x => x.Trim())
                              .Where(x => int.TryParse(x, out _))
                              .Select(x => int.Parse(x))
                              .ToList();

            if (ids.Count == 0)
                return Json(new { data = "[]" }, JsonRequestBehavior.AllowGet);

            string formattedIDs = string.Join(",", ids);

            string SQL = $@"select s.SalesID, s.ClassID, format(s.TranDate, 'dd/MM/yyyy') InvoiceDate, s.ClassName, s.SONumber, s.SOID, s.SellerID, s.SellerName, s.CustomerID, s.CustomerName, s.CustomerCode, 
s.CustomerType, s.CustomerMobile, s.LocationID, s.WarehouseName, s.NeedCheckBy, s.NeedCheckByID, s.CheckByUsername, s.NeedApproveBy, s.NeedApproveByID, s.ApproveUsername, s.ExchangeRate, s.Remark, s.SaleNote, s.TermCondition, format(s.DueDate, 'dd/MM/yyyy') DueDate, 
s.IsIncludedVAT, s.IsCommercialINV, s.IsNonVAT,
format(sd.QTY, 'N3') QTY, cast(sd.UnitPrice as varchar(50)) UnitPrice, 
cast(sd.DiscountUSD as varchar(50)) DiscountUSD,
cast(sd.Amount as varchar(50)) Amount, sd.*
from dbo.TblSales s 
inner join dbo.TblCustomerInvoice sd on sd.SalesID = s.SalesID
where s.SalesID in ({formattedIDs}) and s.SystemID = @SystemID and s.WarehouseID = @WarehouseID and s.DomainID = @DomainID";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadInvoices(int SalesID)
        {
            HTML html = new HTML();
            string SQL = @"select s.SalesID, s.ClassID, format(s.TranDate, 'dd/MM/yyyy') InvoiceDate, s.ClassName, s.SONumber, s.SOID, s.SellerID, s.SellerName, s.CustomerID, s.CustomerName, s.CustomerCode, 
s.CustomerType, s.CustomerMobile, s.LocationID, s.WarehouseName, s.NeedCheckBy, s.NeedCheckByID, s.CheckByUsername, s.NeedApproveBy, s.NeedApproveByID, s.ApproveUsername, s.ExchangeRate, s.Remark, s.SaleNote, s.TermCondition, format(s.DueDate, 'dd/MM/yyyy') DueDate, 
s.IsIncludedVAT, s.IsCommercialINV, s.IsNonVAT,
format(sd.QTY, 'N3') QTY, cast(sd.UnitPrice as varchar(50)) UnitPrice, 
cast(sd.DiscountUSD as varchar(50)) DiscountUSD,
cast(sd.Amount as varchar(50)) Amount, sd.*
from dbo.TblSales s 
inner join dbo.TblCustomerInvoice sd on sd.SalesID = s.SalesID
where s.SalesID = @SalesID and s.SystemID = @SystemID and s.WarehouseID = @WarehouseID and s.DomainID = @DomainID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SalesID", SalesID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPrefixCallBack(int ClassID, string Module, bool IsCM = false)
        {
            HTML html = new HTML();
            // Use parameterized SQL to avoid SQL injection
            string SQL = @"
        DECLARE @Prefix nvarchar(50), @Module nvarchar(50);
        SELECT 
            @Prefix = CASE 
                WHEN @ModuleParam = 'INV' AND @IsCM = 1 THEN 'BS/CM/{yy}-{000000}' 
                WHEN @ModuleParam = 'INV' THEN PrefixInvoice 
                WHEN @ModuleParam = 'FINV' THEN PrefixCommercial 
                WHEN @ModuleParam = 'SO' AND @ClassIDParam = 12 AND @IsCM = 1 THEN 'BS/{yy}{MM}-{000000}'
                WHEN @ModuleParam = 'SO' AND @ClassIDParam = 11 THEN '{yy}{MM}-{0000}'
                WHEN @ModuleParam = 'SO' AND @IsCM = 1 THEN 'BS/{yy}{MM}-{000000}' 
                WHEN @ModuleParam = 'SO' THEN PrefixSO 
                WHEN @ModuleParam = 'Quote' AND @ClassIDParam = 11 THEN 'Q-{0000}'
                 ELSE 'Q/{yy}-{000000}' 
            END, 
            @Module = @ModuleParam
        FROM dbo.TblClass 
        WHERE ClassID = @ClassIDParam;
        EXEC [SP_SHOW_PREFIX] @Prefix, @Module, @SystemID, @WarehouseID, @DomainID;
    ";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ClassIDParam", ClassID.ToString() },
                { "ModuleParam", Module },
                { "IsCM", IsCM ? "1" : "0" },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };

            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveChangeStock(int SalesID)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            string Type = "DeliveryNote";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SalesID", SalesID.ToString() },
                { "Type", Type}
            };
            string sql = @"
if(exists(select * from dbo.TblStockOut where SalesID = @SalesID))
begin
delete from dbo.TblStockOut where SalesID = @SalesID
delete from dbo.TblStockInDetail where SalesID = @SalesID
end
insert into dbo.TblStockOut (SalesID, CustomerID, ItemID, DateOut, UomID, UM, QTY, UnitPrice, InvoiceNumber, [Type], LocationID, StockOutType, SystemID, WarehouseID)
select s.SalesID, s.CustomerID, sd.ItemID, '" + html.GetCurrentDate() + @"' DateOut, sd.UomID, sd.UM, sd.QTY, sd.UnitPrice, s.InvoiceNumber, 'Stock Out' [Type], s.LocationID, 'Sales Out' StockOutType, s.SystemID, s.WarehouseID
from dbo.TblSales s
inner join dbo.TblCustomerInvoice sd on sd.SalesID = s.SalesID
where s.SalesID = @SalesID and s.Type = @Type and sd.ItemStatus = 'NonOtherCharge';

insert into dbo.TblStockInDetail(SalesID, StockOutID, ItemID, ItemName, ItemDescription, [Type], StockOutDate, CustomerID, InvoiceNumber, LocationID, StockOutType, SystemID, WarehouseID, UomID, UM, UnitPrice, QTYOUT)
select so.SalesID, so.StockOutID, so.ItemID, i.ItemName, i.ItemDescription, 'Stock Out' [Type], '" + html.GetCurrentDate() + @"' StockOutDate, so.CustomerID, so.InvoiceNumber, so.LocationID, so.StockOutType, so.SystemID, so.WarehouseID, so.UomID, so.UM, so.UnitPrice, so.QTY QTYOUT
from dbo.TblStockOut so
inner join dbo.TblItem i on i.ItemID = so.ItemID
where so.SalesID = @SalesID;
";
            result = html.SaveChangeData(sql, false, para);

            UpdateQtyCalStockInDetail(SalesID);
            InventoryExpirationDateController expireDate = new InventoryExpirationDateController();
            expireDate.StockExpireOut(SalesID, Type);

            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateQtyCalStockInDetail(int SalesID)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            SqlResult result = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string sql = @"";

            sql += @"select * from dbo.TblCustomerInvoice
where SalesID = @SalesID and ItemStatus = 'NonOtherCharge'";
            para.Add("SalesID", SalesID.ToString());

            ds = html.LoadData(sql, false, para);
            if (ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int ItemID = int.Parse(dr["ItemID"].ToString());
                    int UomID = int.Parse(dr["UomID"].ToString());
                    double QTY = double.Parse(dr["QTY"].ToString());

                    var data = html.GetUnitCal(ItemID, UomID, QTY, 0);

                    para.Clear();
                    para.Add("QtyCal", data["QtyCal"]);
                    para.Add("QtyCalOut", data["QtyCal"]);
                    para.Add("SalesID", SalesID.ToString());
                    para.Add("ItemID", ItemID.ToString());

                    sql = @"update dbo.TblStockOut set QtyCalOut = @QtyCalOut
where SalesID =  @SalesID and ItemID = @ItemID;
update dbo.TblStockInDetail set QtyCalOut = @QtyCalOut
where SalesID = @SalesID and ItemID = @ItemID;";

                    html.SaveChangeData(sql, false, para);
                }

            }
            return Json(new { Result = ResultType.Successful, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustTypePrefix(string CustomerType, bool ToVAT = false, string ClassName = "", string PrefixType = "")
        {
            HTML html = new HTML();
            string sql = @"declare @Prefix nvarchar(50), @Module nvarchar(50), @StartNumber int, @PrefixType nvarchar(50) = '" + PrefixType + @"';
select @Prefix = case when @PrefixType = 'VAT' then PrefixVAT when @PrefixType = 'CM' then PrefixCM else Prefix end, 
@Module = 'Customer', @StartNumber = case when @PrefixType = 'VAT' then isnull(StartNumberPrefixVAT, 1) when @PrefixType = 'CM' then isnull(StartNumberPrefixCM, 1) else isnull(StartNumber,1) end
from dbo.TblCustomerType where Status='Enable' and CustomerType = '" + CustomerType + @"' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + @";
exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ",'" + html.DomainID + "'," + @"@StartNumber;
select @Prefix";
            return Json(new { data = html.JsonData(sql, false, null, false, "") }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustSO(int ID)
        {
            HTML html = new HTML
            {
                DialogBoxWidth = "Auto",
                TableName = "TblSales",
                PrimaryField = "SalesID"
            };
            //html.OrderBy = "PurchaseOrderID desc";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "SO Date:",
                DBField = "s.TranDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "SO Status:",
                DBField = "s.StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSONumber",
                ISAutoComplete = true,
                Label = "SO Number:",
                DBField = "SONumber",
                SQLAutoComplete = "select distinct top 10 SONumber from dbo.TblSales where Type='SaleOrder' and lower(InvoiceNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by InvoiceNumber"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "s.ClassName",
                SQL_CBO = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Code",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName"),
                ISAutoComplete = true,
            });

            string sql = $@"select ROW_NUMBER() Over(order by s.ClassID desc, s.SONumber desc) as [No], format(s.TranDate, 'dd-MMM-yyyy') SODate, s.SalesID, '<a href=""javascript:void(0);"" onclick=""showSOForm('+convert(varchar, s.SalesID)+', '''+ s.SONumber +''', '''+ s.Type +''')"">'+s.SONumber+'</a>' as SONumber, s.QuoteNumber, 
s.ClassName, s.ClassID, s.StatusID, isnull(s.IsToInvoice, 0) IsToInvoice, case when isnull(s.IsToInvoice, 0) <> 0 then '<span class=""text-primary"">Complete</span>' else '<span class=""text-danger"">Pending</span>' end StatusToInvoice , c.CustomerCode, c.CustomerName, 
case when s.StatusID = 92 then '<span class=""badge badge-pill badge-success text-uppercase"">' + st.[Status] + '</span>' when s.StatusID = 90 then '<span class=""badge badge-pill badge-primary text-white text-uppercase"">' + st.[Status] + '</span>' when s.StatusID = 94 then '<span class=""badge badge-pill badge-success text-uppercase"">' + st.[Status] + '</span>' else '<span class=""badge badge-pill text-uppercase badge-danger text-white"">' + st.[Status] + '</span>' end [Status], 
s.GrandTotal, s.Total, s.DiscountUSD, s.SaleNote, case when isnull(s.NeedCheckByID, 0) <> 0 then s.NeedCheckBy else '<span class=""text-info"">No need check</span>' end NeedCheckBy, s.NeedApproveBy, s.SellerName, s.CreateBy
from dbo.TblSales s
inner join dbo.TblCustomer c on c.CustomerID = s.CustomerID
inner join dbo.TblStatus st on st.StatusID = s.StatusID
where s.Type = 'SalesOrder' and s.CustomerID = {ID}";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="s.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="s.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="s.DomainID", Value =html.DomainID },
            };
            //html.ShowSearchButton = false;
            html.GH.SQL = sql;
            //html.GridEventClick = "onGridClick(this);";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "SO Date", FieldName = "SODate", TextAlign="center" },
                new GridField() { Header = "SO Number", FieldName = "SONumber", TextAlign="center"},
                //new GridField() { Header = "Quote Number", FieldName = "QuoteNumber", },
                new GridField() { Header = "Class Name", FieldName = "ClassName",TextAlign="center" },
                //new GridField() { Header = "Customer Code", FieldName = "CustomerCode", TextAlign="center"},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                new GridField() { Header = "Sale Name", FieldName = "SellerName"},
                //new GridField() { Header = "Need Check By", FieldName = "NeedCheckBy", TextAlign ="center" },
                //new GridField() { Header = "Need Approve By", FieldName = "NeedApproveBy", TextAlign ="center" },
                new GridField() { Header = "Status Approval", FieldName = "Status", TextAlign ="center" },
                new GridField() { Header = "Status To Invoice", FieldName = "StatusToInvoice", TextAlign ="center" },
                new GridField() { Header = "Total", FieldName = "Total", ClassName="text-right", Format="#,#0.00" },
                new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right", Format="#,#0.00" },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", ClassName="text-right", Format="#,#0.00" },
                //new GridField() { Header = "Balance", FieldName = "RemainAmount", ClassName="text-right text-danger", Format="#,#0.00" },		
                new GridField() { Header = "Issue By", FieldName = "CreateBy", TextAlign ="center" },
            };
            html.TblID = "TblSO";
            html.ShowGridOnly = true;
            return Json(new { data = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString()) }, JsonRequestBehavior.AllowGet);
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
left join dbo.TblLeads e on e.LeadID = c.EmployeeID
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
        public JsonResult LoadUserApproval()
        {
            HTML html = new HTML();
            string SQL = @"select u.UserLoginID NeedApproveByID, u.TelegramUserName ApproveUsername, u.FullName NeedApproveBy, u.Job ApprovedByPosition
from dbo.TblUserLogin u
inner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID
where u.StatusID = 3 and ts.SystemID = " + html.SystemID + " and ts.WarehouseID = " + html.WarehouseID + " order by u.FullName";
            return Json(new { data = html.JsonData(SQL, false, null) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustInvoice(int ID, string Type)
        {
            HTML html = new HTML
            {
                DialogBoxWidth = "Auto",
                TableName = "TblSales",
                PrimaryField = "SalesID"
            };
            //html.OrderBy = "PurchaseOrderID desc";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoices Date:",
                DBField = "s.TranDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Invoices Status:",
                DBField = "s.StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoicesNumber",
                ISAutoComplete = true,
                Label = "Invoices Number:",
                DBField = "InvoiceNumber",
                SQLAutoComplete = "select distinct top 10 InvoiceNumber from dbo.TblSales where Type='Invoices' and lower(InvoiceNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by InvoiceNumber"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "s.ClassName",
                SQL_CBO = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Code",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName"),
                ISAutoComplete = true,
            });
            string sql = $@"select ROW_NUMBER() Over(order by s.ClassID desc, s.InvoiceNumber desc) as [No], format(s.TranDate, 'dd-MMM-yyyy') InvoicesDate, s.SalesID, '<a href=""javascript:void(0);"" onclick=""showInvoicesForm('+convert(varchar, s.SalesID)+', '''+ s.InvoiceNumber +''', '''+ s.Type +''')"">'+s.InvoiceNumber+'</a>' as InvoicesNumber, 
'<a href=""javascript:void(0);"" onclick=""showInvoicesForm('+convert(varchar, (select top 1 SalesID from dbo.TblSales where SONumber = s.SONumber))+', '''+ s.SONumber +''', '''+ (select top 1 Type from dbo.TblSales where SONumber = s.SONumber) +''')"">'+s.SONumber+'</a>' as SONumber, s.CreateBy,
s.ClassName, s.StatusID, s.ClassID, '<a href=""javascript:void(0);"" onclick=""showCustomerProfile('+convert(varchar, s.CustomerID)+')"">'+c.CustomerName+'</a>' as CustomerName, s.CustomerCode , case when s.StatusID = 92 then '<span class=""badge badge-pill badge-success text-uppercase"">' + st.[Status] + '</span>' when s.StatusID = 90 then '<span class=""badge badge-pill badge-primary text-white text-uppercase"">' + st.[Status] + '</span>' else '<span class=""badge badge-pill text-uppercase badge-danger text-white"">' + st.[Status] + '</span>' end [Status], 
s.GrandTotal, s.Total, s.DiscountUSD, s.SaleNote, case when isnull(s.NeedCheckByID, 0) <> 0 then s.NeedCheckBy else '<span class=""text-info"">No need check</span>' end NeedCheckBy,case when isnull(s.NeedApproveByID, 0) <> 0 then s.NeedApproveBy else '<span class=""text-info"">No need approval</span>' end NeedApproveBy, s.SellerName, format(s.DueDate, 'dd-MMM-yyyy') DueDate, isnull(s.Balance, 0) Balance, isnull(s.Paid, 0) Paid, isnull(s.Deposit, 0) Deposit, 
case when isnull(s.PaymentStatus, '') <> 'paid' then '<a name=""Payment"" href=""javascript:void(0);"" onclick=""Payment('+ convert(varchar, s.SalesID) +', ''' + s.InvoiceNumber + ''','+ convert(varchar, s.CustomerID) +','''+s.Type+''');"">Pay Now</a>' else '' end Payment, case when s.PaymentStatus = 'paid' then '<span class=""badge badge-pill badge-primary"">Paid</span>' when s.PaymentStatus = 'partial' then '<span class=""badge badge-pill badge-info"">Partial</span>' else '<span class=""badge badge-pill badge-danger"">Unpaid</span>' end PaymentStatus
from dbo.TblSales s
inner join dbo.TblCustomer c on c.CustomerID = s.CustomerID
inner join dbo.TblStatus st on st.StatusID = s.StatusID
where s.Type = 'Invoices' and s.CustomerID = {ID}";
            if (Type == "paid")
                sql += $@" and isnull(s.PaymentStatus, '') = 'paid'";
            else if (Type == "unpaid")
                sql += $@" and isnull(s.PaymentStatus, '') = ''";
            else if (Type == "partial")
                sql += $@" and isnull(s.PaymentStatus, '') = 'partial'";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="s.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="s.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="s.DomainID", Value =html.DomainID },
            };
            //html.ShowSearchButton = false;
            html.GH.SQL = sql;
            //html.GridEventClick = "onGridClick(this);";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoices Date", FieldName = "InvoicesDate", TextAlign="center" },
                new GridField() { Header = "Invoices Number", FieldName = "InvoicesNumber", TextAlign="center"},
                //new GridField() { Header = "SO Number", FieldName = "SONumber", },
                new GridField() { Header = "Class Name", FieldName = "ClassName", TextAlign="center" },
                //new GridField() { Header = "Customer Code", FieldName = "CustomerCode", TextAlign="center"},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                //new GridField() { Header = "Sale Name", FieldName = "SellerName"},
                //new GridField() { Header = "Need Check By", FieldName = "NeedCheckBy", TextAlign ="center" },
                //new GridField() { Header = "Need Approve By", FieldName = "NeedApproveBy", TextAlign ="center" },
                new GridField() { Header = "Status Approval", FieldName = "Status", TextAlign ="center" },
                //new GridField() { Header = "Total", FieldName = "Total", ClassName="text-right" , Format="#,#0.00"},
                //new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right" , Format="#,#0.00"},
                //new GridField() { Header = "Grand Total", FieldName = "GrandTotal", ClassName="text-right", Format="#,#0.00" },
                //new GridField() { Header = "Deposit", FieldName = "Deposit", ClassName="text-right", Format="#,#0.00" },
                new GridField() { Header = "Paid", FieldName = "Paid", ClassName="text-right text-primary", Format="#,#0.00" },
                new GridField() { Header = "Balance", FieldName = "Balance", ClassName="text-right text-danger", Format="#,#0.00" },
                new GridField() { Header = "Payment Status", FieldName = "PaymentStatus", TextAlign="center" },
                new GridField() { Header = "Payment", FieldName = "Payment", ClassName="text-center" },
                new GridField() { Header = "Due Date", FieldName = "DueDate", ClassName="text-center text-danger" },
                new GridField() { Header = "Issue By", FieldName = "CreateBy", TextAlign ="center" },
            };
            html.TblID = "TblInvoices";
            html.ShowGridOnly = true;
            return Json(new { data = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString()) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSOIDEdit(int ID)
        {
            HTML html = new HTML();
            string sql = @"select SOID from dbo.TblSales where SalesID=" + ID;
            return Json(new { data = html.JsonData(sql, false) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Customer Payments")]
        public ActionResult Payment(int SalesID, string InvoiceNo, int CustomerID, string Type)
        {
            HTML html = new HTML
            {
                FormID = "CreateNewPayment",
                FieldIDFocus = "Txt",
                TableName = "TblSalesPayment",
                PrimaryField = "PaymentID"
            };
            string CustomerName = "";
            string Balance = "";
            string sql = @"select format(s.Balance, '#,#0.00') TotalBalance,* from dbo.TblSales s
where s.SalesID = " + SalesID + " and s.CustomerID = " + CustomerID + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            DataSet ds = html.LoadData(sql, false, null);
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    CustomerName = dr["CustomerName"].ToString();
                    Balance = dr["TotalBalance"].ToString();
                }
            }
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Customer:",
                ID = "CustomerName",
                DBField = "CustomerName",
                Required = true,
                IsReadOnly = true,
                Value = CustomerName,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "CustomerID:",
                ID = "CustomerID",
                DBField = "CustomerID",
                Required = true,
                IsReadOnly = true,
                Value = CustomerID.ToString(),
                Hide = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "SalesID:",
                ID = "SalesID",
                DBField = "SalesID",
                Value = SalesID.ToString(),
                Hide = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                SingleDatePicker = true,
                Label = "Payment Date:",
                ID = "PaymentDate",
                DBField = "PaymentDate",
                Required = true
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Payment Method:",
                ID = "PaymentMethod",
                DBField = "PaymentMethod",
                Required = true,
                SQL_CBO = "select PaymentMethodID, PaymentMethod from dbo.TblPaymentMethod " +
                "where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by case when PaymentMethod = 'Cash' then 0 else 1 end, PaymentMethod",
                DisplayText = "PaymentMethod",
                Value = "PaymentMethodID",
            });

            string IncomeAccount = @"select distinct coa.ChartOfAccountID, ac.AccountCategory, act.AccountTypes, coa.AccountNo, coa.AccountName
from dbo.TblAccountTypes act
inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID
inner join dbo.TblChartOfAccount coa on coa.AccountType = act.AccountTypesID
where ac.AccountCategoryID = 3 order by ac.AccountCategory, act.AccountTypes";
            string ChartOfAccountID = "85";

            html.HTMLControls.Add(new HtmlComboBox()
            {
                CboValue = ChartOfAccountID,
                Label = "Deposit To:",
                ID = "ChartOfAccountID",
                SQL_CBO = IncomeAccount,
                DBField = "ChartOfAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "150" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "DueAmount:",
                ID = "DueAmount",
                DBField = "DueAmount",
                Value = Balance,
                Hide = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Amount Received:",
                ID = "PaidAmount",
                DBField = "PaidAmount",
                Required = true,
                Value = Balance
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "Memo",
                DBField = "Memo",
                Required = true,
                Value = "Payment for Invoice No.: " + InvoiceNo
            });

            html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(" + SalesID + ");";
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        public JsonResult SaveChangeCustomerBalance(int SalesID)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SalesID", SalesID.ToString() }
            };
            string sql = @"declare @Paid decimal(18, 2), @Balance decimal(18, 2);
select top 1 @Paid = (select sum(PaidAmount) from dbo.TblSalesPayment where SalesID = sp.SalesID), 
@Balance = sp.DueAmount - sp.PaidAmount
from dbo.TblSalesPayment sp 
where SalesID = @SalesID
order by sp.PaymentID desc
select @Balance, @Paid
update dbo.TblSales set Paid = @Paid, Balance = @Balance,
PaymentStatus = case when @Balance > 0 then 'partial' else 'paid' end
where SalesID = @SalesID";
            result = html.SaveChangeData(sql, false, para);
            return Json(new { data = result.Message });
        }
        public async Task<JsonResult> ValidationCallBackAlert(int SalesID, int StatusID, string TelegramUserName, string UserName)
        {
            HTML html = new HTML();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            SqlResult result = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>();
            string sql = "";
            para.Add("SalesID", SalesID.ToString());

            if (StatusID == 92)
            {
                sql = @"select distinct s.Type, u.UserLoginID, u.FullName, u.TelegramUserName, s.TelegramGroupID,
s.TelegramGroupTopicID, s.TelegramMessageID, s.TelegramMessage, isnull(s.NeedCheckByID, 0) NeedCheckByID, isnull(s.IsCheck, 0) IsCheck
from dbo.TblSales s
inner join dbo.TblUserLogin u on u.UserLoginID = s.NeedCheckByID
where s.SalesID = @SalesID";
            }
            else if (StatusID == 90)
            {
                sql = @"select distinct s.Type, u.UserLoginID, u.FullName, u.TelegramUserName, s.TelegramGroupID,
s.TelegramGroupTopicID, s.TelegramMessageID, s.TelegramMessage,isnull(s.NeedCheckByID, 0) NeedCheckByID, isnull(s.IsCheck, 0) IsCheck
from dbo.TblSales s
inner join dbo.TblUserLogin u on u.UserLoginID = s.NeedApproveByID
where s.SalesID = @SalesID";
            }
            else if (StatusID == 91 || StatusID == 93)
            {
                sql = @"select distinct s.Type,case when isnull(s.NeedCheckByID, 0) <> 0 and isnull(s.IsCheck, 0) = 0 then (select top 1 UserLoginID from dbo.TblUserLogin where UserLoginID = s.NeedCheckByID) else u.UserLoginID end as UserLoginID, 
case when isnull(s.NeedCheckByID, 0) <> 0 and isnull(s.IsCheck, 0) = 0 then (select top 1 FullName from dbo.TblUserLogin where UserLoginID = s.NeedCheckByID) else u.FullName end as FullName,
case when isnull(s.NeedCheckByID, 0) <> 0 and isnull(s.IsCheck, 0) = 0 then (select top 1 TelegramUserName from dbo.TblUserLogin where UserLoginID = s.NeedCheckByID) else u.TelegramUserName end as TelegramUserName,s.TelegramGroupID,
s.TelegramGroupTopicID, s.TelegramMessageID, s.TelegramMessage,isnull(s.NeedCheckByID, 0) NeedCheckByID, isnull(s.IsCheck, 0) IsCheck
from dbo.TblSales s
inner join dbo.TblUserLogin u on u.UserLoginID = s.NeedApproveByID
where s.SalesID = @SalesID";
            }

            DataSet ds = html.LoadData(sql, false, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string TelegramGroupID = ds.Tables[0].Rows[0]["TelegramGroupID"].ToString();
                string TelegramMessageID = ds.Tables[0].Rows[0]["TelegramMessageID"].ToString();
                string TelegramMessage = ds.Tables[0].Rows[0]["TelegramMessage"].ToString();
                string TelegramGroupTopicID = ds.Tables[0].Rows[0]["TelegramGroupTopicID"].ToString();
                string ActionUserName = ds.Tables[0].Rows[0]["TelegramUserName"].ToString();
                string NeedCheckByID = ds.Tables[0].Rows[0]["NeedCheckByID"].ToString();
                string Type = ds.Tables[0].Rows[0]["Type"].ToString();
                bool IsCheck = bool.Parse(ds.Tables[0].Rows[0]["IsCheck"].ToString());
                string status = StatusID == 92 ? "check" : (StatusID == 91 ? "disapprove" : "approve");
                string type = Type == "Invoices" ? "Invoice" : "SO";

                string msg = $"@{UserName}<br>You are not allowed to {status} this {type}.<br>";
                msg += "Need to take action by " + ActionUserName + ".";

                if (Type == "Invoices")
                {
                    result = await notificationTelegram.NotificationTelegramBotToTopic(msg, long.Parse(TelegramGroupID), int.Parse(TelegramGroupTopicID), null, int.Parse(TelegramMessageID), TelegramMessage, "", "", false);
                }
                else if (Type == "SalesOrder")
                {
                    result = NeedCheckByID != "0" && !IsCheck
                        ? await notificationTelegram.NotificationTelegramBotToTopic(msg, long.Parse(TelegramGroupID), int.Parse(TelegramGroupTopicID), null, int.Parse(TelegramMessageID), TelegramMessage, "", "", false)
                        : await notificationTelegram.NotificationTelegramBot("", long.Parse(TelegramGroupID), "", "", msg, false, true, SalesID, int.Parse(TelegramMessageID), TelegramMessage, null, false);
                }
            }

            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveFileAttachment(int SalesID)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SalesID", SalesID.ToString() }
            };
            string sql = @"select SalesID, CustomerID, value as Filenamae, DestinationFolder
from dbo.TblSales 
cross apply string_split(AttachmentFile, '~')
where SalesID = @SalesID and AttachmentFile <> 'AttachmentFile'";
            DataSet ds = html.LoadData(sql, false, para);
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    sql = @"if exists(select * from dbo.TblSalesAttachmentFile where SalesID = @SalesID)
begin
delete from dbo.TblSalesAttachmentFile where SalesID = @SalesID
end";
                    html.SaveChangeData(sql, false, para);
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        string DestinationFolder = dr["DestinationFolder"].ToString();
                        string Filename = dr["Filenamae"].ToString();
                        string CustomerID = dr["CustomerID"].ToString();
                        para.Clear();
                        para.Add("SalesID", SalesID.ToString());
                        para.Add("DestinationFolder", DestinationFolder);
                        para.Add("Filename", Filename);
                        para.Add("CustomerID", CustomerID);
                        sql = @"insert into dbo.TblSalesAttachmentFile (SalesID, CustomerID, DestinationFolder, AttachmentFile, TypeFor)
    values (@SalesID, @CustomerID, @DestinationFolder, @FileName, 'AttachmentFile')";
                        result = html.SaveChangeData(sql, false, para);
                    }
                }

            }
            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveChangeRequestEdit(int SalesID, int ReqApproveID, string ReqApproveBy, string Username, string Type, string ReqEditComment)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SalesID", SalesID.ToString() },
                { "ReqApproveID", ReqApproveID.ToString() },
                { "ReqApproveBy", ReqApproveBy },
                { "ReqEditApproveByUsername", Username },
                { "ReqEditComment", ReqEditComment },
                { "ReqEditDate", html.GetCurrentDate() },
                { "Type", Type },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            string sql = $@"update dbo.TblSales set ReqEditApproveByID = @ReqApproveID, ReqEditApproveBy = @ReqApproveBy, ReqEditStatusID= 60 , ReqEditDate=@ReqEditDate, 
                            ReqEditApproveByUsername=@ReqEditApproveByUsername, ReqEditComment=@ReqEditComment
                            where SalesID = @SalesID and Type = @Type and SystemID = @SystemID and WarehouseID = @WarehouseID;";
            result = html.SaveChangeData(sql, false, para);
            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStockExpireDate(int ItemID, int LocationID, string SearchTerm)
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ItemID", ItemID.ToString() },
                { "LocationID", LocationID.ToString() },
                { "SearchTerm", "%" + SearchTerm.ToLower() + "%" }
            };
            string sql = @"select format(ExpireDate, 'dd-MM-yyyy') StockExpireDate, sum(isnull(QTYIN, 0) - isnull(QTYOUT, 0)) as QOH 
from dbo.TblStockExpireDate 
where ItemID = @ItemID and LocationID = @LocationID
and (@SearchTerm = '%%' or format(ExpireDate, 'dd-MM-yyyy') like @SearchTerm)
group by ExpireDate 
having sum(isnull(QTYIN, 0) - isnull(QTYOUT, 0)) > 0";
            return Json(new { data = html.JsonData(sql, false, para) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "List SO Report")]
        public ActionResult SOReport()
        {
            HTML html = new HTML
            {
                DialogBoxWidth = "Auto",
                TableName = "TblSales",
                PrimaryField = "SalesID"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "SO Date:",
                DateTimePickerWithRanges = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtYear",
                Label = "Year:",
                SQL_CBO = $"select '2024' as [Year] union all select distinct format(TranDate, 'yyyy') [Year] from dbo.TblSales where Type = 'SalesOrder' and SystemID = {html.SystemID}",
                DisplayText = "Year",
                DBField = "TranYear",
                Value = "Year",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "SO Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSOType",
                Label = "SO Type:",
                DBField = "SOType",
                SQL_CBO = "select 'VAT' SOType union all select 'CM' SOType union all select 'Non-VAT' SOType",
                DisplayText = "SOType",
                Value = "SOType",
                ComboBox = true,
                Hide = html.SystemID == "25"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSONumber",
                Label = "SO Number:",
                DBField = "SONumber",
                SQL_CBO = "select distinct SONumber from dbo.TblSales where Type='SalesOrder' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "order by SONumber desc",
                DisplayText = "SONumber",
                Value = "SONumber",
                ComboBox = true,
                Multiple = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerCode",
                ISAutoComplete = true,
                Label = "Customer Code:",
                DBField = "CustomerCode",
                SQLAutoComplete = "select distinct CustomerCode from dbo.TblSales where Type='SalesOrder' and lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerName",
                ISAutoComplete = true,
                Label = "Customer Name:",
                DBField = "CustomerName",
                SQLAutoComplete = "select distinct CustomerName from dbo.TblSales where Type='SalesOrder' and lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName"
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Class Name",
                SQL = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("ClassID"),
                DefaultCheckBok = true,
            });

            html.GH.GridPara = new GridPara[]
            {
                new GridPara(){ ParaName="TranType", Value =  "SalesOrder"},
                new GridPara(){ ParaName="ReportType", Value =  "ReportSO"},
            };

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "SO Date", FieldName = "TranDate", TextAlign="center", FormatDateTime = "dd-MMM-yyyy" },
                new GridField() { Header = "SO Number", FieldName = "SONumber", OnClickFunction = "showSOForm(#=SalesID#, '#=SONumber#', '#=Type#')"},
                new GridField() { Header = "Quote Number", FieldName = "QuoteNumber", IsHiddenField = true},
                new GridField() { Header = "Description", FieldName = "ItemName", Width="500px", ClassName="truncate-cell" },
                new GridField() { Header = "Class Name", FieldName = "ClassName", ClassName="text-center"},
                new GridField() { Header = "Customer Code", FieldName = "CustomerCode", TextAlign="center"},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                new GridField() { Header = "Customer Phone", FieldName = "CustomerMobile", },
                new GridField() { Header = "Sale Name", FieldName = "SellerName"},
                new GridField() { Header = "Need Check By", FieldName = "NeedCheckBy", TextAlign ="center", IsHiddenField = true},
                new GridField() { Header = "Need Approve By", FieldName = "NeedApproveBy", TextAlign ="center", IsHiddenField = true },
                new GridField() { Header = "Status Approval", FieldName = "Status", TextAlign ="center",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string>
                    {
                        { "Approve", "badge-primary" },
                        { "Draft", "badge-danger" },
                        { "Checked", "badge-warning" },
                        { "Disapprove", "badge-danger" },
                        { "Cancel", "badge-warning" },
                    }
                },
                new GridField() { Header = "Status To Invoice", FieldName = "StatusToInvoice", TextAlign ="center", TotalLabel="Total:",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string>
                    {
                        { "Complete", "badge-primary" },
                        { "Pending", "badge-danger" },
                    }
                },
                new GridField() { Header = "Total", FieldName = "Total", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Issue By", FieldName = "CreateBy", TextAlign ="center" },
            };
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_SALES_REPORT",
            };

            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        [Authorize(Roles = "List Invoices Report")]
        public ActionResult InvoicesReport()
        {
            HTML html = new HTML
            {
                DialogBoxWidth = "Auto",
                TableName = "TblSales",
                PrimaryField = "SalesID"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoices Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Invoices Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoicesType",
                Label = "Invoices Type:",
                DBField = "InvoicesType",
                SQL_CBO = "select 'VAT' InvoicesType union all select 'CM' InvoicesType union all select 'Non-VAT' InvoicesType",
                DisplayText = "InvoicesType",
                Value = "InvoicesType",
                ComboBox = true,
                Hide = html.SystemID == "25"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoicesNumber",
                ISAutoComplete = true,
                Label = "Invoices Number:",
                DBField = "InvoiceNumber",
                SQLAutoComplete = "select distinct top 10 InvoiceNumber from dbo.TblSales where Type='Invoices' and lower(InvoiceNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by InvoiceNumber"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSONumber",
                Label = "SO Number:",
                DBField = "SONumber",
                SQLAutoComplete = "select distinct top 10 SONumber from dbo.TblSales where Type='SalesOrder' and lower(SONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by SONumber",
                ISAutoComplete = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerCode",
                ISAutoComplete = true,
                Label = "Customer Code:",
                DBField = "CustomerCode",
                SQLAutoComplete = "select distinct top 10 CustomerCode from dbo.TblSales where Type='Invoices' and lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerName",
                ISAutoComplete = true,
                Label = "Customer Name:",
                DBField = "CustomerName",
                SQLAutoComplete = "select distinct top 10 CustomerName from dbo.TblSales where Type='Invoices' and lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName"
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Class Name",
                SQL = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("ClassID"),
                DefaultCheckBok = true,
            });

            html.GH.GridPara = new GridPara[]
            {
                new GridPara(){ ParaName="TranType", Value =  "Invoices"},
                new GridPara(){ ParaName="ReportType", Value =  "ReportInvoice"},
            };
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoices Date", FieldName = "InvoicesDate", TextAlign="center" },
                new GridField() { Header = "Invoices Number", FieldName = "InvoicesNumber", TextAlign="center"},
                new GridField() { Header = "SO Number", FieldName = "SONumber", },
                new GridField() { Header = "DN Number", FieldName = "DNNumber", },
                new GridField() { Header = "Description", FieldName = "ItemName", Width="500px", ClassName="truncate-cell"},
                new GridField() { Header = "Remark", FieldName = "Remark", Width="200px", ClassName="truncate-cell"},
                new GridField() { Header = "Class Name", FieldName = "ClassName", TextAlign="center" },
                new GridField() { Header = "Customer Code", FieldName = "CustomerCode", TextAlign="center", IsHiddenField = true},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                new GridField() { Header = "Customer Phone", FieldName = "CustomerMobile", },
                new GridField() { Header = "VAT TIN", FieldName = "VATNumber", TotalLabel="Total:"},
                new GridField() { Header = "Sale Name", FieldName = "SellerName", IsHiddenField = true},
                new GridField() { Header = "Sub Total", FieldName = "SubTotal", ClassName="text-right" , Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "VAT", FieldName = "VAT", ClassName="text-right" , Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Total", FieldName = "Total", ClassName="text-right" , Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right" , Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Deposit", FieldName = "Deposit", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Balance", FieldName = "Balance", ClassName="text-right text-danger", Format="#,#0.00",IsTotalFooter=true },
                new GridField() { Header = "Paid", FieldName = "Paid", ClassName="text-right text-primary", Format="#,#0.00",IsTotalFooter=true },
                new GridField() { Header = "Exchange Rate", FieldName = "ExchangeRate", ClassName="text-right"},
                new GridField() { Header = "Payment Status", FieldName = "PaymentStatus", TextAlign="center", IsHiddenField = true},
                new GridField() { Header = "Payment", FieldName = "Payment", ClassName="text-center", IsHiddenField = true },
                new GridField() { Header = "Due Date", FieldName = "DueDate", ClassName="text-center text-danger" },
                new GridField() { Header = "Issue By", FieldName = "CreateBy", TextAlign ="center", IsHiddenField = true },
            };
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_SALES_REPORT"
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        [Authorize(Roles = "List CMInvoices Report")]
        public ActionResult CMInvoicesReport()
        {
            HTML html = new HTML
            {
                TableName = "TblSales",
                PrimaryField = "SalesID"
            };
            //html.OrderBy = "PurchaseOrderID desc";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoices Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Invoices Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true,
                Hide = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCMInvoicesNumber",
                ISAutoComplete = true,
                Label = "CM Invoices Number:",
                DBField = "NewInvoiceNumber",
                SQLAutoComplete = "select distinct top 10 NewInvoiceNumber from dbo.TblSales where Type='ConvertInvoices' and lower(NewInvoiceNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by NewInvoiceNumber"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerCode",
                ISAutoComplete = true,
                Label = "Customer Code:",
                DBField = "CustomerCode",
                SQLAutoComplete = "select distinct top 10 CustomerCode from dbo.TblSales where Type='ConvertInvoices' and lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerName",
                ISAutoComplete = true,
                Label = "Customer Name:",
                DBField = "CustomerName",
                SQLAutoComplete = "select distinct top 10 CustomerName from dbo.TblSales where Type='ConvertInvoices' and lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName"
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Class Name",
                SQL = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("ClassID"),
                DefaultCheckBok = true,
            });

            html.GH.GridPara = new GridPara[]
            {
                new GridPara(){ ParaName="TranType", Value =  "ConvertInvoices"},
                new GridPara(){ ParaName="ReportType", Value =  "ReportCMInvoice"},
            };
            html.ShowPaging = false;
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoices Date", FieldName = "InvoicesDate", TextAlign="center" },
                new GridField() { Header = "CM Invoice Number", FieldName = "NewInvoiceNumber", TextAlign="center"},
                new GridField() { Header = "Invoice Number", FieldName = "InvoiceNumber", },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                new GridField() { Header = "Description", FieldName = "ItemName", Width="500px", ClassName="truncate-cell" },
                new GridField() { Header = "Class Name", FieldName = "ClassName", TextAlign="center" },
                new GridField() { Header = "Customer Code", FieldName = "CustomerCode", TextAlign="center"},
                new GridField() { Header = "Customer Phone", FieldName = "CustomerMobile", },
                new GridField() { Header = "Sale Name", FieldName = "SellerName", TotalLabel="Total:"},
                new GridField() { Header = "Sub Total", FieldName = "SubTotal", ClassName="text-right" , Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "VAT", FieldName = "VAT", ClassName="text-right" , Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Total", FieldName = "Total", ClassName="text-right" , Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right" , Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Deposit", FieldName = "Deposit", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Paid", FieldName = "Paid", ClassName="text-right text-primary", Format="#,#0.00", IsTotalFooter=true },
                new GridField() { Header = "Balance", FieldName = "Balance", ClassName="text-right text-danger", Format="#,#0.00", IsTotalFooter=true},
                new GridField() { Header = "Payment Status", FieldName = "PaymentStatus", TextAlign="center" },
                new GridField() { Header = "Due Date", FieldName = "DueDate", ClassName="text-center text-danger" },
                new GridField() { Header = "Create By", FieldName = "CreateBy", ClassName="text-center" },
            };
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_SALES_REPORT"
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        [Authorize(Roles = "List SO Detail Report")]
        public ActionResult SODetailReport()
        {
            HTML html = new HTML
            {
                DialogBoxWidth = "Auto",
                TableName = "TblSales",
                PrimaryField = "SalesID"
            };
            //html.OrderBy = "PurchaseOrderID desc";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "SO Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "SO Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSONumber",
                Label = "SO Number:",
                DBField = "SONumber",
                SQLAutoComplete = "select distinct top 10 SONumber from dbo.TblSales where Type='SalesOrder' and lower(SONumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by SONumber",
                ISAutoComplete = true,
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Class Name",
                SQL = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("ClassID"),
                DefaultCheckBok = true,
            });

            html.GH.GridPara = new GridPara[]
            {
                new GridPara(){ ParaName="TranType", Value =  "SalesOrder"},
                new GridPara(){ ParaName="ReportType", Value =  "ReportSODetail"},
            };

            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "SO Date", FieldName = "SODate", TextAlign="center" },
                new GridField() { Header = "SO Number", FieldName = "SONumber"},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", TextAlign="left"},
                new GridField() { Header = "Item Code", FieldName = "ItemCode", ClassName="text-center"},
                new GridField() { Header = "Item Name", FieldName = "ItemName", },
                new GridField() { Header = "Description", FieldName = "ItemDescription", Width="500px", ClassName="truncate-cell" },
                new GridField() { Header = "Dimension", FieldName = "Dimension", ClassName="text-center",},
                new GridField() { Header = "QTY", FieldName = "QTY", ClassName="text-right text-danger"},
                new GridField() { Header = "UOM", FieldName = "UM", ClassName="text-center", TotalLabel="Total:"},
                new GridField() { Header = "Unit Price", FieldName = "UnitPrice", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Amount", FieldName = "Amount", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
            };
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_SALES_REPORT"
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        [Authorize(Roles = "List Quote Detail Report")]
        public ActionResult QuoteDetailReport()
        {
            HTML html = new HTML
            {
                DialogBoxWidth = "Auto",
                TableName = "TblSales",
                PrimaryField = "SalesID"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Quote Date:",
                DBField = "s.TranDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Quote Status:",
                DBField = "s.StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSONumber",
                Label = "Quote Number:",
                DBField = "s.QuoteNumber",
                SQLAutoComplete = "select distinct top 10 QuoteNumber from dbo.TblSales where Type='Quotation' and lower(QuoteNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by QuoteNumber",
                ISAutoComplete = true,
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Class Name",
                SQL = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("ClassID"),
                DefaultCheckBok = true,
            });

            string sql = @"select row_number() Over(order by s.TranDate desc, s.QuoteNumber desc) as [No], 
    format(s.TranDate, 'dd-MMM-yyyy') QuoteDate, '<a href=""javascript:void(0);"" onclick=""showQuoteForm('+convert(varchar, s.SalesID)+', '''+ s.QuoteNumber +''', '''+ s.Type +''')"">'+s.QuoteNumber+'</a>' as QuoteNumber, c.CustomerName,
    case when ci.ItemStatus = 'OtherCharge' then '' else (select top 1 ItemCode from dbo.TblItem where ItemID = ci.ItemID) end as ItemCode, 
    case when ci.ItemStatus = 'OtherCharge' then (select top 1 OtherChargeItem from dbo.TblOtherCharge where OtherChargeID = ci.ItemID) else (select top 1 ItemName from dbo.TblItem where ItemID = ci.ItemID) end as ItemName , 
    ci.ItemDescription, ci.Dimension, ci.QTY, ci.UM, ci.UnitPrice, ci.DiscountUSD, ci.Amount
    from dbo.TblSales s
    inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
    inner join dbo.TblCustomer c on c.CustomerID = s.CustomerID
    where s.Type = 'Quotation'";

            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "[No]";

            html.GH.GridPara = new GridPara[]
            {
        new GridPara(){ ParaName="s.SystemID", Value = html.SystemID},
        new GridPara(){ ParaName="s.WarehouseID", Value = html.WarehouseID},
        new GridPara(){ ParaName="s.DomainID", Value = html.DomainID},
            };

            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
        new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
        new GridField() { Header = "Quote Date", FieldName = "QuoteDate", TextAlign="center" },
        new GridField() { Header = "Quote Number", FieldName = "QuoteNumber"},
        new GridField() { Header = "Customer Name", FieldName = "CustomerName", TextAlign="left"},
        new GridField() { Header = "Item Code", FieldName = "ItemCode", ClassName="text-center"},
        new GridField() { Header = "Item Name", FieldName = "ItemName", },
        new GridField() { Header = "Description", FieldName = "ItemDescription" }, // Removed width and truncate
        new GridField() { Header = "Dimension", FieldName = "Dimension", ClassName="text-center",},
        new GridField() { Header = "QTY", FieldName = "QTY", ClassName="text-right text-danger"},
        new GridField() { Header = "UOM", FieldName = "UM", ClassName="text-center", TotalLabel="Total:"},
        new GridField() { Header = "Unit Price", FieldName = "UnitPrice", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
        new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
        new GridField() { Header = "Amount", FieldName = "Amount", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
            };
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = false
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        [Authorize(Roles = "List Invoices Detail Report")]
        public ActionResult InvoicesDetailReport()
        {
            HTML html = new HTML
            {
                DialogBoxWidth = "Auto",
                TableName = "TblSales",
                PrimaryField = "SalesID"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoices Date:",
                DBField = "s.TranDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Invoices Status:",
                DBField = "s.StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoicesNumber",
                ISAutoComplete = true,
                Label = "Invoices Number:",
                DBField = "InvoiceNumber",
                SQLAutoComplete = "select distinct top 10 InvoiceNumber from dbo.TblSales where Type='Invoices' and lower(InvoiceNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by InvoiceNumber"
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Class Name",
                SQL = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("ClassID"),
                DefaultCheckBok = true,
            });

            string sql = @"select row_number() Over(order by s.ClassID desc, case when s.InvoiceNumber not like '%BS%' then 0 else 1 end asc, s.InvoiceNumber desc) as [No], s.InvoiceNumber, format(s.TranDate, 'dd-MMM-yyyy') InvoiceDate,
case when ci.ItemStatus = 'OtherCharge' then '' else (select top 1 ItemCode from dbo.TblItem where ItemID = ci.ItemID) end as ItemCode, 
case when ci.ItemStatus = 'OtherCharge' then (select top 1 OtherChargeItem from dbo.TblOtherCharge where OtherChargeID = ci.ItemID) else (select top 1 ItemName from dbo.TblItem where ItemID = ci.ItemID) end as ItemName , 
ci.QTY, ci.UM, ci.Dimension, ci.UnitPrice, ci.DiscountUSD, ci.Amount
from dbo.TblSales s
inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
where s.Type = 'Invoices'";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="s.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="s.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="s.DomainID", Value =html.DomainID },
            };
            //html.ShowSearchButton = false;
            html.GH.SQL = sql;
            //html.GridEventClick = "onGridClick(this);";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoices Date", FieldName = "InvoiceDate", TextAlign="center" },
                new GridField() { Header = "Invoices Number", FieldName = "InvoiceNumber", TextAlign="center"},
                new GridField() { Header = "Item Code", FieldName = "ItemCode", ClassName="text-center"},
                new GridField() { Header = "Item Name", FieldName = "ItemName", Width="500px", ClassName="truncate-cell"},
                new GridField() { Header = "Dimension", FieldName = "Dimension", ClassName="text-center"},
                new GridField() { Header = "QTY", FieldName = "QTY", ClassName="text-right text-danger"},
                new GridField() { Header = "UOM", FieldName = "UM", ClassName="text-center", TotalLabel="Total:"},
                new GridField() { Header = "Unit Price", FieldName = "UnitPrice", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Amount", FieldName = "Amount", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
            };
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        [Authorize(Roles = "List CM Invoices Detail Report")]
        public ActionResult CMInvoicesDetailReport()
        {
            HTML html = new HTML
            {
                TableName = "TblSales",
                PrimaryField = "SalesID"
            };
            //html.OrderBy = "PurchaseOrderID desc";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoices Date:",
                DBField = "s.TranDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Invoices Status:",
                DBField = "s.StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 92, 94)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true,
                Hide = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCMInvoicesNumber",
                ISAutoComplete = true,
                Label = "CM Invoices Number:",
                DBField = "NewInvoiceNumber",
                SQLAutoComplete = "select distinct top 10 NewInvoiceNumber from dbo.TblSales where Type='ConvertInvoices' and lower(NewInvoiceNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by NewInvoiceNumber"
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Class Name",
                SQL = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("ClassID"),
                DefaultCheckBok = true,
            });

            string sql = @"select row_number() over(order by s.NewInvoiceNumber desc) as [No], s.NewInvoiceNumber, s.InvoiceNumber, format(s.TranDate, 'dd-MMM-yyyy') InvoiceDate, 
case when ci.ItemStatus = 'OtherCharge' then '' else (select top 1 ItemCode from dbo.TblItem where ItemID = ci.ItemID) end as ItemCode, 
case when ci.ItemStatus = 'OtherCharge' then (select top 1 OtherChargeItem from dbo.TblOtherCharge where OtherChargeID = ci.ItemID) else (select top 1 ItemName from dbo.TblItem where ItemID = ci.ItemID) end as ItemName , 
ci.QTY, ci.UM, ci.Dimension, ci.UnitPrice, s.SubTotal, ci.DiscountUSD, ci.Amount
from dbo.TblSales s
inner join dbo.TblCustomerInvoice ci on ci.SalesID = s.SalesID
where s.Type = 'ConvertInvoices'";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridPara = new GridPara[] {
              new GridPara() { ParaName="s.SystemID", Value =html.SystemID },
              new GridPara() { ParaName="s.WarehouseID", Value =html.WarehouseID },
              new GridPara() { ParaName="s.DomainID", Value =html.DomainID },
            };
            //html.ShowSearchButton = false;
            html.GH.SQL = sql;
            //html.GridEventClick = "onGridClick(this);";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoices Date", FieldName = "InvoiceDate", TextAlign="center" },
                new GridField() { Header = "CM Invoice Number", FieldName = "NewInvoiceNumber", TextAlign="center"},
                new GridField() { Header = "Invoice Number", FieldName = "InvoiceNumber",TextAlign="center" },
                new GridField() { Header = "Item Code", FieldName = "ItemCode", ClassName="text-center"},
                new GridField() { Header = "Item Name", FieldName = "ItemName", Width="500px", ClassName="truncate-cell"},
                new GridField() { Header = "Dimension", FieldName = "Dimension", ClassName="text-center"},
                new GridField() { Header = "UOM", FieldName = "UM", ClassName="text-center", TotalLabel="Total:"},
                new GridField() { Header = "QTY", FieldName = "QTY", ClassName="text-right text-danger"},
                new GridField() { Header = "Unit Price", FieldName = "UnitPrice", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Discount$", FieldName = "DiscountUSD", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
                new GridField() { Header = "Amount", FieldName = "Amount", ClassName="text-right", Format="#,#0.00", IsTotalFooter = true},
            };
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        public async Task<JsonResult> SendNotificationToTelegram(int SalesID, bool isReqEdit = false, bool isCancel = false)
        {
            var html = new HTML();
            var result = new SqlResult();

            string sql = "SELECT *, isnull(IsNonVAT, 0) as IsNanVAT FROM dbo.TblSales WHERE SalesID = @SalesID";
            var para = new Dictionary<string, string> { { "@SalesID", SalesID.ToString() } };
            var ds = html.LoadData(sql, false, para);
            var dr = ds.Tables[0].Rows[0];

            bool isNeedCheck = !string.IsNullOrEmpty(dr["NeedCheckBy"]?.ToString());
            bool isNonVAT = bool.Parse(dr["IsNanVAT"]?.ToString());
            string approverId = isNeedCheck ? dr["NeedCheckByID"]?.ToString() : dr["NeedApproveByID"]?.ToString();
            string fullName = isNeedCheck ? dr["NeedCheckBy"]?.ToString() : dr["NeedApproveBy"]?.ToString();
            string username = isNeedCheck ? dr["CheckByUsername"]?.ToString() : dr["ApproveUsername"]?.ToString();
            string preparedBy = dr["CreateBy"]?.ToString();
            string saleType = dr["Type"]?.ToString();
            string referenceNo = saleType == "SalesOrder" ? dr["SONumber"]?.ToString() :
                                saleType == "Quotation" ? dr["QuoteNumber"]?.ToString() :
                                saleType == "ConvertInvoices" ? dr["NewInvoiceNumber"]?.ToString() :
                                saleType == "Invoices" ? dr["InvoiceNumber"]?.ToString() :
                                dr["DNNumber"]?.ToString();
            string systemID = dr["SystemID"]?.ToString();
            string customerName = dr["CustomerName"]?.ToString();
            string className = dr["ClassName"]?.ToString();
            string inputComment = HttpUtility.UrlDecode(Request.Form["Comment"] ?? "").Trim();
            string comment = !string.IsNullOrEmpty(inputComment) ? inputComment : (dr["Comment"]?.ToString() ?? "");
            string reqEditComment = dr.Table.Columns.Contains("ReqEditComment") ? dr["ReqEditComment"]?.ToString() : "";
            string messageId = dr["TelegramMessageID"]?.ToString();
            string action = isNeedCheck ? "check" : "approve";
            string recipient = Utility.TryParse(approverId, "int", 0) == 38 ? "Boss" : fullName;
            string botToken = html.GetBotTokenBySystemID(int.Parse(systemID));

            string requestType = saleType == "SalesOrder" ? "sales order" :
                                saleType == "Quotation" ? "quotation" :
                                saleType == "Invoices" ? "invoice" :
                                saleType == "ConvertInvoices" ? "cm invoice" :
                                "delivery note";

            string note = dr["SaleNote"]?.ToString();

            if (saleType == "Invoices" || saleType == "DeliveryNote")
            {
                await UpdateSORef(dr, saleType);
            }

            var msg = new StringBuilder();
            var sent = new SqlResult();
            var inlineKeyboard = new InlineKeyboardMarkup();
            var notification = new NotificationTelegram();
            long chatId = Utility.TryParse(dr["TelegramGroupID"]?.ToString(), "long", 0);

            if (isCancel)
            {
                sql = @"update dbo.TblSales set StatusID = 94, Comment = @Comment where SalesID = @SalesID";
                para.Add("@Comment", comment);
                html.SaveChangeData(sql, false, para);

                msg.AppendLine($"Dear Team, a <b>{requestType}</b> has been <b>cancelled</b>.\n");
                msg.AppendLine($"━━━━━━━━━━━━━━━━━━━━");
                msg.AppendLine($"📋 <b>Ref. No</b>      : <code>{referenceNo}</code>");
                msg.AppendLine($"👤 <b>Cancelled by</b>  : {html.UserFullName}");
                if (!string.IsNullOrEmpty(comment))
                {
                    msg.AppendLine($"💬 <b>Reason</b>        : <i>{comment}</i>");
                }
                msg.AppendLine($"━━━━━━━━━━━━━━━━━━━━\n");
                msg.AppendLine("This document has been cancelled and requires no further action. Thank you.");

                sent = await notification.NotificationTelegramBot(
                    msg.ToString(),
                    chatId: chatId,
                    replyMarkupInlineKeyboardMarkup: true,
                    MessageID: int.Parse(messageId),
                    edit: false,
                    newBotClient: botToken
                );

            }
            else // Request approval action
            {
                if (!isReqEdit)
                {
                    if (requestType != "delivery note")
                    {
                        msg.AppendLine($"{username}\n");
                        msg.AppendLine($"Dear {recipient}, you have a {requestType}{(!isNonVAT && className != "BSM" ? " tax" : "")} need to {action}.\n");
                        if (!string.IsNullOrEmpty(note) && saleType == "SalesOrder")
                            msg.AppendLine($"<i>Note: {note}</i>\n");
                    }
                    else
                    {
                        msg.AppendLine($"Dear Team, we have a new {requestType}.\n");
                    }

                    msg.AppendLine($"<i><b>Prepared by: {preparedBy}</b></i>.\n");
                    msg.Append($"Please kindly review {(requestType == "delivery note" ? "" : "and take action")}, Thanks you.");

                    inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"ApproveSales~{SalesID}~{username}"),
                            InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisapproveSales~{SalesID}~{username}"),
                        }
                    });

                    if (isNeedCheck)
                    {
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(text: "Check ✍️",    callbackData: $"CheckSales~{SalesID}~{username}"),
                                InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisCheckSales~{SalesID}~{username}"),
                            }
                        });
                    }

                    sql = @"SELECT TOP 1 * 
                        FROM dbo.TblTelegramGroupNotification
                        WHERE [Module] = @Module AND TelegramGroupType = @Type AND SystemID = @SystemID";

                    string type = !isReqEdit &&
                        (isNeedCheck || saleType == "DeliveryNote"
                        || (saleType == "Invoices" && isNonVAT)
                        || (className == "BSM" && saleType == "Invoices"))
                        ? "Topic" : "Normal";

                    para.Clear();
                    para.Add("@Module", saleType);
                    para.Add("@Type", type);
                    para.Add("@SystemID", html.SystemID);
                    ds = html.LoadData(sql, false, para);
                    dr = ds?.Tables[0]?.Rows[0];

                    bool isGroupTopic = dr["TelegramGroupType"]?.ToString() == "Topic";
                    chatId = long.Parse(dr["TelegramGroupID"]?.ToString());
                    int topId = Utility.TryParse(dr["TelegramGroupTopicID"]?.ToString(), "int", 0);

                    ProfileController controller = new ProfileController();
                    dynamic frm = controller.InvoiceSaleOrderTMP(SalesID, saleType, false, true);
                    string fileName = html.ToUrlSlug(referenceNo);
                    string destination = $"Uploads/Sales/{saleType}/" + html.ToUrlSlug(customerName);
                    var pdfResult = await html.HTMLToPDF(frm.Data.Data, destination, fileName);
                    sent = isGroupTopic
                        ? (SqlResult)await notification.NotificationTelegramBotToTopic(
                            "",
                            chatId: chatId,
                            topicID: topId,
                            inlineKeyboard: saleType == "DeliveryNote" || saleType == "ConvertInvoices" ? null : inlineKeyboard,
                            Files: pdfResult?.OutPutFile,
                            Caption: msg.ToString(),
                            newBotClient: botToken
                        )
                        : (SqlResult)await notification.NotificationTelegramBot(
                            "",
                            chatId: chatId,
                            PDFFile: pdfResult?.OutPutFile,
                            Caption: saleType != "ConvertInvoices" ? msg.ToString() : "",
                            replyMarkupInlineKeyboardMarkup: true,
                            inlineKeyboard: saleType == "DeliveryNote" || saleType == "ConvertInvoices" ? null : inlineKeyboard,
                            newBotClient: botToken
                        );

                    if (sent.Result == ResultType.Successful)
                    {
                        System.IO.File.Delete(pdfResult.OutPutFile);

                        sql = @"update dbo.TblSales set TelegramMessageID = @TelegramMessageID, TelegramMessage = @TelegramMessage, TelegramGroupID = @TelegramGroupID, TelegramGroupTopicID = @TelegramGroupTopicID where SalesID = @SalesID";
                        para.Clear();
                        para.Add("@TelegramMessageID", sent.Value.ToString());
                        para.Add("@TelegramGroupTopicID", topId.ToString());
                        para.Add("@TelegramMessage", msg.ToString());
                        para.Add("@TelegramGroupID", chatId.ToString());
                        para.Add("@SalesID", SalesID.ToString());
                        html.SaveChangeData(sql, false, para);
                    }
                }
                else
                {
                    username = dr["ReqEditApproveByUsername"]?.ToString();
                    recipient = dr["ReqEditApproveBy"]?.ToString();
                    preparedBy = html.UserFullName;

                    msg.AppendLine($"{username}\n");
                    msg.AppendLine($"Dear <b>{recipient}</b>, you have a new <b>Edit Request</b> pending your approval.\n");
                    msg.AppendLine($"━━━━━━━━━━━━━━━━━━━━");
                    msg.AppendLine($"📋 <b>Ref. No</b>     : <code>{referenceNo}</code>");
                    msg.AppendLine($"👤 <b>Requested by</b> : {preparedBy}");
                    msg.AppendLine($"💬 <b>Reason</b>       : <i>{reqEditComment}</i>");
                    msg.AppendLine($"━━━━━━━━━━━━━━━━━━━━\n");
                    msg.AppendLine($"Please review and take action, Thank you.");

                    inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Approve ✅",    callbackData: $"ApproveRequestEdit~{SalesID}~{username}"),
                            InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: $"DisapproveRequestEdit~{SalesID}~{username}"),
                        }
                    });

                    sent = await notification.NotificationTelegramBot(
                        "",
                        chatId: chatId,
                        Caption: msg.ToString(),
                        replyMarkupInlineKeyboardMarkup: true,
                        MessageID: int.Parse(messageId),
                        inlineKeyboard: inlineKeyboard,
                        edit: false,
                        newBotClient: botToken
                    );
                }
            }

            if (saleType == "DeliveryNote")
            {
                SaveChangeStock(SalesID);
            }

            return Json(new
            {
                Result = sent.Result,
                Message = sent.Message
            }, JsonRequestBehavior.AllowGet);
        }
        public Task UpdateSORef(DataRow row, string saleType = "")
        {
            try
            {
                var html = new HTML();

                if (!row.Table.Columns.Contains("SOID") || row["SOID"] == DBNull.Value)
                    return Task.CompletedTask;

                var salesIds = row["SOID"]
                    .ToString()
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                string columnName = null;

                if (saleType == "Invoices")
                {
                    columnName = "IsToInvoice";
                }
                else if (saleType == "DeliveryNote")
                {
                    columnName = "IsToDN";
                }

                if (string.IsNullOrEmpty(columnName))
                    return Task.CompletedTask;

                foreach (var salesId in salesIds)
                {
                    string sql = $@"
                        UPDATE dbo.TblSales
                        SET {columnName} = 1
                        WHERE SalesID = @SalesID";

                    var parameters = new Dictionary<string, string>
                    {
                        ["SalesID"] = salesId.Trim()
                    };

                    html.SaveChangeData(sql, false, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update sales status.", ex);
            }

            return Task.CompletedTask;
        }
    }
}