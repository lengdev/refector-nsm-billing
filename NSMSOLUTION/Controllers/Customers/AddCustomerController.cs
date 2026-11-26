using MVC;
using NewInventory.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSMSOLUTION.Controllers.Customers.Reports;
using NSMSOLUTION.Helper_Code;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;
using WebMarkupMin.AspNet4.Mvc;
using static NSMSOLUTION.Models.UserDTOs;

namespace NewInventorySystem.Controllers
{
    public class AddCustomerController : Controller
    {
        // GET: AddCustomer
        [Authorize(Roles = "Add Customer, Edit Customer, Delete Customer")]

        public ActionResult Index()
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

            //html.DialogBoxWidth = "Auto";
            //html.DialogBoxHeight = 600;
            //html.AddButton = "Register New Customer";
            //html.AddURL = "AddCustomer/CreateCustomer";
            //html.AddURL = "AddCustomer/CreateCustomerV2";
            html.GH.GridProcedureName = "SP_SEARCH_CUSTOMER";
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                //DateTimePickerWithRanges = true,
                DateRangePicker = true,
                LabelFromDate = "Register Date:"
            });
            string sqlLocation = "select distinct CustomerProvinceCity from dbo.TblCustomer where lower(CustomerProvinceCity) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerProvinceCity";
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtLocation",
                Label = "Location:",
                DBField = "Location",
                SQL_CBO = "select distinct ProvinceCityID Location, CustomerProvinceCity from dbo.TblCustomer where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerProvinceCity",
                DisplayText = "CustomerProvinceCity",
                Value = "Location",
                ComboBox = true
            });
            string sql = "select distinct s.StatusID, s.[Status] from dbo.TblCustomer c ";
            sql += "inner join dbo.TblStatus s on s.StatusID = c.StatusID ";
            sql += "where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%' order by s.StatusID";
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                Label = "Status:",
                DBField = "Status",
                SQL_CBO = sql,
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Customer Group:",
                DBField = "CustomerGroupID",
                SQL_CBO = "select CustomerGroupID, CustomerGroup from dbo.TblCustomerGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroupID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType\r\nfrom dbo.TblCustomer c inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID where c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%' order by ct.CustomerType",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtATTName",
                Label = "ATT Name:",
                DBField = "ATTName",
                SQLAutoComplete = @"select distinct top 10 bi.ATTName from dbo.TblCustomerBillInformation bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where lower(bi.ATTName) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'",
                ISAutoComplete = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("PhoneNumber") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerName",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("PhoneNumber") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Phone Number",
                SQL = "select distinct top 10 CompanyContactMobileNumber from dbo.TblCustomerCompany where lower(CompanyContactMobileNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CompanyContactMobileNumber",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("PhoneNumber") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Identification",
                SQL = "select distinct top 10 CustomerIdentification from dbo.TblCustomer where lower(CustomerIdentification) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerIdentification",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("PhoneNumber") + "," + EncryAndDecry.Encryp("CustomerIdentification"),
                ISAutoComplete = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GridEventClick = "onGridClick(this);";
            //html.GridHeight = "400px";
            //html.DefaultSearch = false;
            html.TableName = "dbo.TblCustomer";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
				//new GridField() { Header = "CustomerID", FieldName = "CustomerID", IsHiddenField= true },
				new GridField() { Header = "CustomerGroup", FieldName = "CustomerGroup" }, //GroupLabel="Group Name: ", IsGroupByField= true, Colspan=9 },
				new GridField() { Header = "Block", FieldName = "CustomerType" },//GroupLabel="Block: ", IsGroupByField= true, Colspan=9 },
				new GridField() { Header = "CID", FieldName = "CustomerCode", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
                new GridField() { Header = "ATT Name", FieldName = "ATTName" },
                new GridField() { Header = "Phone Number", FieldName = "PhoneNumber" },
                //new GridField() { Header = "Inv.Date", FieldName = "InvoiceDate", TextAlign="center" },
                //new GridField() { Header = "Inv.No", FieldName = "InvoiceNo", TextAlign="center" },
                //new GridField() { Header = "Total", FieldName = "GrandTotal" },
                new GridField() { Header = "Register Date", FieldName = "CreateDate", ClassName="text-danger" },
                new GridField() { Header = "Register By", FieldName = "RegisterBy" },
                new GridField() { Header = "Location", FieldName = "Location" },
                //new GridField() { Header = "Status", FieldName = "Status", TextAlign="center" },
                new GridField() { Header = "Sale Name", FieldName = "SaleName" },
				//new GridField() { Header = "Action", FieldName = "CustomerID", TextAlign = "Center",  IsModifyField = true,
    //                DeleteWithTableName="TblCustomerBalance, TblCustomerBillInformation, TblCustomerBillingInvoice, TblCustomerCompany, TblCustomerDetail, TblCustomerInvoice, TblCustomerPayment",
    //            }
			};
            html.ShowPaging = false;
            //html.ShowPrintOnTopTable = false;
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                filterable = false,
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            //ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();

            return View();
        }
        public ActionResult CustomerInformation()
        {
            HTML html = new HTML();
            bool roleEditCustomer = HttpContext.GetOwinContext().Request.User.IsInRole("Edit Customer");
            ViewBag.roleEditCustomer = roleEditCustomer;
            string sql = "select CustomerGroupID, CustomerGroup from dbo.TblCustomerGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            ViewBag.CustomerGroup = html.JsonData(sql);
            return View();
        }
        public ActionResult CompanyInformation()
        {
            return View();
        }

        [Authorize(Roles = "Add Customer, Edit Customer, Create Quotation, Edit Quotation, Add Sale Order, Edit Sale Order")]
        [HttpPost]
        [MinifyHtml]
        public ActionResult CreateCustomer(object[] PrimaryFieldValue, int ISSO = 0)
        {
            HTML html = new HTML();
            ViewBag.RegisterBy = html.UserFullName;
            ViewBag.PrimaryFieldValue = 0;
            ViewBag.SQLCustomerType = EncryAndDecry.Encryp("select CustomerTypeID, CustomerType, PrefixVAT, PrefixCM, isnull(ISVAT, 'false') ISVAT from dbo.TblCustomerType where [Status] = 'Enable' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by isnull(OrderBy,9), CustomerType");//CustomerGroupID =@CustomerGroupID
            ViewBag.TblCustomerInvoice = EncryAndDecry.Encryp("TblCustomerInvoice");
            ViewBag.TblCustomer = EncryAndDecry.Encryp("TblCustomer");
            //if (ISSO == 0)
            //{
            ViewBag.SQLCID = EncryAndDecry.Encryp("select distinct top 10 CustomerCode CID from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID =" + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode");
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
where lower(c.CustomerCode) = @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'";
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
where c.CustomerID = @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'";
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
            string sql = "select CustomerTypeID, CustomerType from dbo.TblCustomerType where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            ViewBag.CustomerType = html.JsonData(sql);

            //sql = "select uw.WID LocationID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName, uw.ContactName, uw.ContactPhone, uw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + case when uw.District <> '' then ', ' + uw.District end + case when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end DeliverTo, uw.WarehouseName WName from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by (b.BranchName + ' - ' + uw.WarehouseName)";
            //ViewBag.SQLWarehouseName = EncryAndDecry.Encryp(sql);
            sql = "select uw.WID LocationID, uw.WarehouseName from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and uw.DomainID like '%" + html.DomainID + "%' order by uw.WarehouseName";
            ViewBag.SQLWarehouseName = html.JsonData(sql);

            sql = "select LeadID EmployeeID, Salutation + ' ' + FullName EmployeeName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            ViewBag.EmployeeData = html.JsonData(sql);
            sql = "select CustomerGroupID, CustomerGroup from dbo.TblCustomerGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            ViewBag.CustomerGroup = html.JsonData(sql);
            sql = "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ClassName";
            ViewBag.ClassName = html.JsonData(sql);
            sql = "select uw.WID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName, uw.ContactName, uw.ContactPhone, uw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + case when uw.District <> '' then ', ' + uw.District end + case when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end Warehouse from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and uw.DomainID like '%" + html.DomainID + "%' order by (b.BranchName + ' - ' + uw.WarehouseName)";
            ViewBag.LoadLocation = html.JsonData(sql);

            sql = $"select * from dbo.TblIspLink where SystemID = {html.SystemID} and WarehouseID = {html.WarehouseID} and DomainID like '%{html.DomainID}%';";
            ViewBag.IspLink = html.JsonData(sql);

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
                                pd.Bandwidth, bi.UM, pd.DowSpeed, pd.UpSpeed, pd.Price, format(bi.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, format(bi.StartDate, 'dd-MM-yyyy') StartDate, format(bi.EndDate, 'dd-MM-yyyy') EndDate,
                                case when (DATEDIFF(D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate))) < 0 then 0 else DATEDIFF (D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate)) end Remain, 
                                isnull(ac.ServiceID,0) ServiceID, isnull(bi.PackageDetailID, 0) PackageDetailID, isnull(bi.FreeDuration,0) FreeDuration, isnull(bi.Duration,0) Duration, 
                                bi.InvoiceNo, bi.ConnectionID
                                from dbo.TblActivateCustomer ac
                                inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
                                inner join dbo.TblServices s on s.ServiceID = ac.ServiceID
                                inner join dbo.TblCustomerBillingInvoice bi on bi.ActivateCustomerID = ac.ActivateCustomerID
                                inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
                                inner join dbo.TblPackages p on p.PackageID = pd.PackageID
                                inner join dbo.TblPackageType pt on p.PackageTypeID = pt.PackageTypeID
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
                if (value == "1")
                {
                    FormStatus = "Create Quotation";
                    html.AllowAdd = html.HasPermission("Create Quotation");
                    if (!html.AllowAdd)
                        return RedirectToAction("Index", "NoPermission");
                    if (InvoiceNo != "0")
                    {
                        html.AllowEdit = html.HasPermission("Edit Quotation");
                        if (!html.AllowEdit)
                            return RedirectToAction("Index", "NoPermission");
                    }
                }

                if (CustomerID != "0")
                {
                    if (FormStatus == "CreateInvoice")
                    {
                        html.AllowAdd = html.HasPermission("Create Invoice") || html.HasPermission("Edit Sale Order");
                        if (!html.AllowAdd)
                            return RedirectToAction("Index", "NoPermission");
                    }
                    sql = $@"select (e.Salutation + e.FullName) Employee, c.EmployeeID, c.CustomerID, 
isnull(ty.CustomerType, '') CustomerType, c.CustomerTypeID, c.CustomerName, isnull(c.CustomerNameLatin, '') CustomerNameLatin, isnull(c.CustomerNote, '') CustomerNote, replace(isnull(c.CustomerCode, ''), 'null', '') CustomerCode, 
c.CustomerGender, c.CustomerGender CustomerGenderID, c.CustomerAddress,
c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity, l.IspLinkID, l.IspLinkName,
c.CustomerMobile, c.CustomerEmail, c.CustomerNationality, c.CustomerIdentification,
c.CustomerContactName, c.CustomerContactMobileNumber, c.CustomerContactEmail, c.CustomerGroupID, c.CustomerGroup, isnull(c.AgencyCommissionID,0) AgencyCommissionID, isnull(c.CustomerNoted,'') CustomerNoted, isnull(TypeOfInvoice,'None VAT') SetCID, CustomerType_PrefixVAT, CustomerType_PrefixCM, c.ClassName, c.ClassNameID, c.CommuneID, c.DistrictID, c.ProvinceCityID, c.ISApplyCID
from dbo.TblCustomer c
left join dbo.TblCustomerType ty on ty.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblLeads e on e.LeadID = c.EmployeeID
left join dbo.TblIspLink l on l.IspLinkID = (select top 1 IspLinkID from dbo.TblCustomerBillingInvoice 
where CustomerID = {CustomerID} and InvoiceNo = '{InvoiceNo}')
where c.CustomerID = " + CustomerID + ";";
                    ViewBag.CustInfo = html.JsonData(sql);
                    sql = @"select distinct cb.CustomerBillInformationID, cb.CompanyName, cb.CompanyNameKH, cb.CompanyDeskPhone, cb.CompanyMobile, 
cb.CompanyEmail, cb.VATNumber, cb.CompanyAddress, cb.CompanyCommune, isnull(ci.PID, NEWID()) PID,
cb.CompanyDistrict, cb.CompanyProvinceCity, cb.CompanyContactName,
cb.CompanyContactPosition, cb.CompanyContactMobileNumber, cb.ATTName,
cb.BillToAddress, cb.BillToCommune, cb.BillToDistrict, cb.BillToProvinceCity,
cb.InstallAddress, cb.InstallCommune, cb.InstallDistrict, cb.InstallProvinceCity, it.IndustryTypeID, it.IndustryType, cb.GoogleMap_FullAddress, cb.GoogleMap_Latitude, cb.GoogleMap_Longitude, cb.InvoiceType, cb.ClassNameID, cb.ClassName, cb.LocationID, cb.Warehouse, cb.InstallDate
from dbo.TblCustomerBillInformation cb
inner join TblCustomer c on c.CustomerID = cb.CustomerID
left join dbo.TblIndustryType it on it.IndustryTypeID = c.IndustryTypeID
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
                                pd.Bandwidth, bi.UM, pd.DowSpeed, pd.UpSpeed, pd.Price, format(bi.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, format(bi.StartDate, 'dd-MM-yyyy') StartDate, format(bi.EndDate, 'dd-MM-yyyy') EndDate,
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
ci.ItemID, format(ci.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, format(ci.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, 
isnull(ci.ConnectionID, '') ConnectionID, ci.[Type],ci.ReferenceNo, ci.QuotationNo, ci.IsIncludedVAT, isnull(ci.InvoiceStatusID, '') InvoiceStatusID,
ci.LocationID, ci.Warehouse, isnull(ci.ServiceID, 0) ServiceID, isnull(ci.[SID], 0) [SID], ci.ChartOfAccountID from dbo.TblCustomerInvoice ci 
where '" + ViewBag.InvoiceType + "' <> 'Renew Invoice' and ci.CustomerID = " + CustomerID + " and InvoiceNo = N'" + InvoiceNo + @"' 
and (ci.OtherChargeID <> 0 or ci.ItemID <> 0) 
order by ci.OrderBy";
                    //}
                    //               else
                    //               {
                    //	sql = "select  ci.ItemDescription, ci.UM, ci.QTY, ci.UnitPrice, ci.DiscountP, ci.DiscountUSD, ci.Amount, ";
                    //	sql += "ci.OtherChargeID, ci.StockInID, ci.ItemID, format(ci.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, format(ci.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, ci.ConnectionID, ci.[Type],ci.ReferenceNo, ci.QuotationNo, ci.IsIncludedVAT, ci.InvoiceStatusID, ci.LocationID, ci.Warehouse from dbo.TblCustomerInvoice ci ";
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
                    sql = "select (e.Salutation + e.FullName) Employee, c.EmployeeID, c.CustomerID, ty.CustomerType, c.CustomerTypeID, c.CustomerName, isnull(c.CustomerNameLatin, '') CustomerNameLatin, isnull(c.CustomerNote, '') CustomerNote, c.CustomerCode, c.CustomerGender, c.CustomerGender CustomerGenderID, c.CustomerAddress, ";
                    sql += "c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity, l.IspLinkID, l.IspLinkID, ";
                    sql += "c.CustomerMobile, c.CustomerEmail, c.CustomerNationality, c.CustomerIdentification, ";
                    sql += "c.CustomerContactName, c.CustomerContactMobileNumber, c.CustomerContactEmail, c.CustomerGroupID, c.CustomerGroup, isnull(c.AgencyCommissionID,0) AgencyCommissionID, isnull(c.TypeOfInvoice,'None VAT') SetCID, c.ClassName, c.ClassNameID, c.CommuneID, c.DistrictID, c.ProvinceCityID, c.ISApplyCID ";
                    sql += "from dbo.TblCustomer c ";
                    sql += "left join dbo.TblCustomerType ty on ty.CustomerTypeID = c.CustomerTypeID ";
                    sql += "inner join dbo.TblLeads e on e.LeadID = c.EmployeeID ";
                    sql += $"left join dbo.TblIspLink l on l.IspLinkID = (select top 1 IspLinkID from dbo.TblCustomerBillingInvoice \r\nwhere CustomerID = {CustomerID} and InvoiceNo = '{InvoiceNo}')";
                    sql += "where c.CustomerID = " + CustomerID + "; ";

                    ViewBag.CustInfo = html.JsonData(sql);
                    sql = @"select distinct cb.CustomerBillInformationID, cb.CompanyName, cb.CompanyNameKH, cb.CompanyDeskPhone, cb.CompanyMobile, 
cb.CompanyEmail, cb.VATNumber, cb.CompanyAddress, cb.CompanyCommune,
cb.CompanyDistrict, cb.CompanyProvinceCity, cb.CompanyContactName,
cb.CompanyContactPosition, cb.CompanyContactMobileNumber, cb.ATTName,
cb.BillToAddress, cb.BillToCommune, cb.BillToDistrict, cb.BillToProvinceCity,
cb.InstallAddress, cb.InstallCommune, cb.InstallDistrict, cb.InstallProvinceCity, it.IndustryTypeID, it.IndustryType, cb.GoogleMap_FullAddress, cb.GoogleMap_Latitude, cb.GoogleMap_Longitude, cb.InvoiceType, cb.ClassNameID, cb.ClassName, cb.LocationID, cb.Warehouse
from dbo.TblCustomerBillInformation cb
inner join TblCustomer c on c.CustomerID = cb.CustomerID
inner join dbo.TblIndustryType it on it.IndustryTypeID = c.IndustryTypeID
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
        public ActionResult CreateCustomerV2(string PrimaryFieldValue, string FormType, bool IsToInvoice = false, bool IsRenew = false, bool IsChangePlan = false)
        {
            HTML html = new HTML();
            ViewBag.FormType = FormType;
            ViewBag.IsToInvoice = IsToInvoice;
            ViewBag.IsRenew = IsRenew;
            ViewBag.IsChangePlan = IsChangePlan;
            ViewBag.CustomerID = PrimaryFieldValue;
            ViewBag.IsEdit = PrimaryFieldValue != "0";

            html.FormData = new HTMLControl[]
            {
                new FormData() {
                    FormTitle = "ព័ត៌មានអ្នកលក់/ Seller Information",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="ភ្នាក់ងារលក់/Sale's Name:", ID="SallerName",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="SellerName", isTypeahead = true,
                            SQL="select LeadID SellerID, Salutation + ' ' + FullName SellerName from dbo.TblLeads where Status = 'Enable' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                            HiddenField="SellerID",
                            placeholder="Select Sale Name"
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Group:", ID="CustomerGroup",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerGroup", isTypeahead = true,
                            SQL="select CustomerGroupID, CustomerGroup from dbo.TblCustomerGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                            HiddenField="CustomerGroupID",
                        },
                        new HtmlTextBox()
                        {
                            Label="តំបន់/Block", ID="CustomerType",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerType", isTypeahead = true, DivGroup = true,
                            SQL="select CustomerTypeID, CustomerType from dbo.TblCustomerType where Status='Enable' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerType",
                            HiddenField="CustomerTypeID",
                            placeholder="Enter Block"
                        },
                    }
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Class Name", ID="ClassName", Required = true, IsReadOnly = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="ClassName", isTypeahead = true,
                            SQL="select ClassID, ClassName from dbo.TblClass where ClassName <> 'NSM no VAT' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName",
                            HiddenField="ClassID",
                            HiddenFieldValue="0",
                            SQLCallBack=$"declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = case when '{FormType}' = 'Quotation' then PrefixQuotation else PrefixInvoice end, @Module = 'Customer Billing Prefix' from dbo.TblClass where ClassID = @ClassID; exec [SP_SHOW_PREFIX] @Prefix, @Module, "+html.SystemID+", "+html.WarehouseID+", '"+html.DomainID+"';",
                            CallBackToControlID="InvoiceNo, PrefixInvoice", UpdatePrefixModule="Customer Billing Prefix", Prefix="PrefixInvoice", SetPrefixToDbFieldName="InvoiceNo",
                        },
                        new HtmlTextBox()
                        {
                            Label="Reference No.", ID="ReferenceNo",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="ReferenceNo", isTypeahead = true,
                            HiddenField="CustomerID",
                            SQL = $"select distinct CustomerID, InvoiceNo as ReferenceNo from dbo.TblCustomerInvoice where InvoiceNo like '%Q/%' and DeleteDate IS NULL and SystemID = {html.SystemID} order by InvoiceNo desc",
                            placeholder = "Enter Ref."
                        },
                        new HtmlTextBox()
                        {
                            Label=$"{(FormType == "Quotation" ? "Quotation No." : "Invoice No.")}",
                            ID="InvoiceNo", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="InvoiceNo", IsReadOnly = true,
                        },
                    }
                },
                new FormData() {
                    FormTitle="ព័ត៌មានអតិថិជន NSM / Customer Information NSM",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="ឈ្មោះខ្មែរ/Name KH:", ID="CustomerName",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerName",
                            placeholder="Enter Name Khmer",
                            HiddenField="CustomerID,AUTOFill,CustomerInvoiceID,BillingInvoiceID,CustomerBillInformationID",
                            HiddenFieldValue= $"{PrimaryFieldValue},0,0,0,0"
                        },
                        new HtmlTextBox()
                        {
                            Label="ឈ្មោះឡាតាំង/Name Latin:", ID="CustomerNameLatin",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerNameLatin",
                            placeholder="Enter Name Latin"
                        },
                        new HtmlTextBox()
                        {
                            Label="ATT Name:", ID="ATTName",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="ATTName",
                            placeholder="Enter ATTName"
                        },
                        new HtmlTextBox()
                        {
                            Label="ភេទ/Gender:", ID="CustomerGender",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerGender", isTypeahead = true,
                            SQL="select 'Male' CustomerGenderID, 'Male' CustomerGender union all select 'Female' CustomerGenderID, 'Female' CustomerGender union all select 'Other' CustomerGenderID, 'Other' CustomerGender",
                            HiddenField="CustomerGenderID",
                            placeholder="Enter Gender"
                        },
                    }
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="លេខសម្គាល់/CID:", ID="CustomerCode",
                            Class="input2 CustInfo text-danger", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerCode", IsReadOnly = true,
                            HiddenField="Prefix",
                            placeholder="Enter Customer Code",
                        },
                        new HtmlTextBox()
                        {
                            Label="SID:", ID="SID", //Required = true,
							Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="SID", IsReadOnly = true,
                            placeholder="Enter SID",
                        },
                        new HtmlTextBox()
                        {
                            Label="Connection ID:", ID="ConnectionID",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="ConnectionID", IsReadOnly = true,
                            placeholder="Enter Connection ID",
                        },
                    }
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="អាសយដ្ឋាន/Address", ID="CustomerAddress",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerAddress",
                            placeholder="Enter Address"
                        },
                        new HtmlTextBox()
                        {
                            Label="សង្កាត់/Commune:", ID="CustomerCommune", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerCommune", isTypeahead = true,
                            SQL=@"SELECT DISTINCT t.CommuneID, c.CustomerCommune, d.DistrictID, CASE WHEN c.Lang = 'KH' THEN d.DistrictKH ELSE d.District END AS District, pc.ProvinceCityID, CASE WHEN c.Lang = 'KH' THEN pc.ProvinceCityKH ELSE pc.ProvinceCity END AS ProvinceCity FROM dbo.TblCommune t CROSS APPLY (VALUES (t.Commune, 'EN'), (t.CommuneKH, 'KH')) c(CustomerCommune, Lang) LEFT JOIN dbo.TblDistrict d ON t.DistrictID = d.DistrictID LEFT JOIN dbo.TblProvinceCity pc ON pc.ProvinceCityID = d.ProvinceCityID WHERE ISNULL(c.CustomerCommune, '') <> '' AND t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%" + html.DomainID + "%'",
                            HiddenField="CommuneID",
                            DisplayOnFields= JsonConvert.SerializeObject(new {
                                CustomerDistrict="District",
                                CustomerProvinceCity="ProvinceCity",
                                CustomerDistrict_DistrictID="DistrictID",
                                CustomerProvinceCity_ProvinceCityID="ProvinceCityID",
                            }),
                            placeholder="Enter Commune"
                        },
                        new HtmlTextBox()
                        {
                            Label="ខណ្ឌ/District:", ID="CustomerDistrict",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerDistrict", IsReadOnly = true,
                            placeholder="Enter District",
                            HiddenField="DistrictID"
                        },
                        new HtmlTextBox()
                        {
                            Label="ខេត្ត/ក្រុង/Province/City:", ID="CustomerProvinceCity",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerProvinceCity", IsReadOnly = true,
                            placeholder="Enter Province City",
                            HiddenField="ProvinceCityID"
                        },
                    }
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="ទូរស័ព្ទដៃ/H/P:", ID="CustomerMobile",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerMobile",
                            placeholder="Enter Phone Number"
                        },
                        new HtmlTextBox()
                        {
                            Label="សញ្ជាតិ/Nationality:", ID="CustomerNationality",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerNationality", isTypeahead = true,
                            SQL=$"select NationalityKH CustomerNationality from dbo.TblNationality where SystemID = {html.SystemID} and WarehouseID = {html.WarehouseID} and DomainID like '%{html.DomainID}%' order by NationalityKH",
                            placeholder="Enter Nationality"
                        },
                    }
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="អត្តសញ្ញាណប័ណ្ណ/លិខិតឆ្លងដែន/Identification/Passport No:", ID="CustomerIdentification",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerIdentification",
                            placeholder="Enter Identification"
                        },
                        new HtmlTextBox()
                        {
                            Label="Identification Expire Date", ID="IdentificationExpireDate",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="IdentificationExpireDate",
                        },
                        new HtmlTextBox()
                        {
                            Label="Status", ID="CustomerStatus",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerStatus", isTypeahead = true,
                            SQL = "select Status as CustomerStatus from dbo.TblStatus where StatusID in (3,4)",
                            Value = "Enable", Hide = FormType != "NewRegister"
                        },
                    }
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="ISP Link:", ID="IspLinkName", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="IspLinkName", isTypeahead = true,
                            SQL=$"select IspLinkID, IspLinkName from dbo.TblIspLink where SystemID = {html.SystemID} and WarehouseID = {html.WarehouseID} and DomainID like '%{html.DomainID}%' order by IspLinkID desc",
                            HiddenField = "IspLinkID",
                            placeholder="Enter ISP Link"
                        },
                        new HtmlTextBox()
                        {
                            Label="ប្រភេទអាជីវកម្ម/Business Type:", ID="IndustryType",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="IndustryType", isTypeahead = true,
                            SQL = $"select IndustryTypeID, IndustryType from dbo.TblIndustryType where IndustryType <> 'Personal' and SystemID = {html.SystemID} and WarehouseID = {html.WarehouseID} and DomainID like '%{html.DomainID}%' order by IndustryTypeID desc",
                            HiddenField = "IndustryTypeID",
                            placeholder="Enter Industry Type"
                        },
                    }
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Customer Note:", ID="CustomerNote",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerNote",
                            placeholder="Enter Customer Note"
                        },
                    }
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="អាសយដ្ឋានតម្លើង/Installation Add.:", ID="InstallAddress",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="InstallAddress",
                            placeholder="Enter Install Address"
                        },
                        new HtmlTextBox()
                        {
                            Label="សង្កាត់/Commune:", ID="InstallCommune", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="InstallCommune", isTypeahead = true,
                            SQL=@"SELECT DISTINCT t.CommuneID, c.CustomerCommune InstallCommune, d.DistrictID, CASE WHEN c.Lang = 'KH' THEN d.DistrictKH ELSE d.District END AS District, pc.ProvinceCityID, CASE WHEN c.Lang = 'KH' THEN pc.ProvinceCityKH ELSE pc.ProvinceCity END AS ProvinceCity FROM dbo.TblCommune t CROSS APPLY (VALUES (t.Commune, 'EN'), (t.CommuneKH, 'KH')) c(CustomerCommune, Lang) LEFT JOIN dbo.TblDistrict d ON t.DistrictID = d.DistrictID LEFT JOIN dbo.TblProvinceCity pc ON pc.ProvinceCityID = d.ProvinceCityID WHERE ISNULL(c.CustomerCommune, '') <> '' AND c.Lang = 'KH' AND t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%" + html.DomainID + "%'",
                            HiddenField="CommuneID",
                            DisplayOnFields= JsonConvert.SerializeObject(new {
                                InstallDistrict="District",
                                InstallProvinceCity="ProvinceCity",
                            }),
                            placeholder="Enter Install Commune"
                        },
                        new HtmlTextBox()
                        {
                            Label="ខណ្ឌ/District:", ID="InstallDistrict",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="InstallDistrict", IsReadOnly = true,
                            placeholder="Enter Install District"
                        },
                        new HtmlTextBox()
                        {
                            Label="ខេត្ត/ក្រុង/Province/City:", ID="InstallProvinceCity",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="InstallProvinceCity", IsReadOnly = true,
                            placeholder="Enter Install Province City"
                        },
                    }
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Google Map:", ID="GoogleMap_FullAddress",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="GoogleMap_FullAddress",
                            placeholder="Enter Google Map"
                        },
                        new HtmlTextBox()
                        {
                            Label="Latitude:", ID="GoogleMap_Latitude",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="GoogleMap_Latitude",
                            placeholder="Enter Latitude"
                        },
                        new HtmlTextBox()
                        {
                            Label="Longitude:", ID="GoogleMap_Longitude",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="GoogleMap_Longitude",
                            placeholder="Enter Longitude",
                            HiddenField = "CurrentLocation"
                        },
                    }
                },
                new FormData() {
                    FormTitle="ព័ត៌មានអតិថិជន​ Infra / Customer Information Infrastructure",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Infra Provider:", ID="InfraProviderName",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="InfraProviderName", isTypeahead = true,
                            SQL="select InfraProviderID, InfraProviderName from dbo.TblInfraProvider where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by InfraProviderID desc",
                            HiddenField="InfraProviderID",
                            placeholder="Enter Infra Provider"
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Infra Name:", ID="CustomerInfraName",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerInfraName",
                            placeholder="Enter Name"
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Infra Survey ID:", ID="CustomerInfraSurveyID",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerInfraSurveyID",
                            placeholder="Enter Survey ID"
                        },
                    },
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Customer Infra CID:", ID="CustomerInfraCID",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerInfraCID",
                            placeholder="Enter CID"
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Infra SID:", ID="CustomerInfraSID",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerInfraSID",
                            placeholder="Enter SID"
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Infra AID:", ID="CustomerInfraAID",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CustomerInfraAID",
                            placeholder="Enter AID"
                        },
                    },
                },
                new FormData() {
                    FormTitle="ព័ត៌មានទូទាត់វិក្កយបត្រ / Customer Billing Information",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="ឈ្មោះក្រុមហ៊ុនជាភាសាខ្មែរ/Company Name KH:", ID="CompanyNameKH",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyNameKH",
                            placeholder="Enter Company Name KH"
                        },
                        new HtmlTextBox()
                        {
                            Label="ឈ្មោះក្រុមហ៊ុន/Company Name:", ID="CompanyName",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyName",
                            placeholder="Enter Company Name"
                        },
                        new HtmlTextBox()
                        {
                            Label="លេខ VAT/VAT TIN:", ID="VATNumber",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="VATNumber",
                            placeholder="Enter VAT Number"
                        },
                    },
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="ទូរស័ព្ទលើតុ/Company Desk Phone:", ID="CompanyDeskPhone",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyDeskPhone",
                            placeholder="Enter Company Desk Phone"
                        },
                        new HtmlTextBox()
                        {
                            Label="ទូរស័ព្ទដៃ/Company H/P:", ID="CompanyMobile",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyMobile",
                            placeholder="Enter Company Mobile"
                        },
                        new HtmlTextBox()
                        {
                            Label="អ៊ីមែលទូទាត់វិក្កយបត្រ/Billing Email:", ID="CompanyEmail",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyEmail",
                            placeholder="Enter Company Email"
                        },
                    },
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="អាសយដ្ឋានក្រុមហ៊ុន/Company Add.:", ID="CompanyAddress",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyAddress",
                            placeholder="Enter Company Address"
                        },
                        new HtmlTextBox()
                        {
                            Label="សង្កាត់/Commune:", ID="CompanyCommune", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyCommune", isTypeahead = true,
                            SQL=@"SELECT DISTINCT t.CommuneID, c.CustomerCommune CompanyCommune, d.DistrictID, CASE WHEN c.Lang = 'KH' THEN d.DistrictKH ELSE d.District END AS District, pc.ProvinceCityID, CASE WHEN c.Lang = 'KH' THEN pc.ProvinceCityKH ELSE pc.ProvinceCity END AS ProvinceCity FROM dbo.TblCommune t CROSS APPLY (VALUES (t.Commune, 'EN'), (t.CommuneKH, 'KH')) c(CustomerCommune, Lang) LEFT JOIN dbo.TblDistrict d ON t.DistrictID = d.DistrictID LEFT JOIN dbo.TblProvinceCity pc ON pc.ProvinceCityID = d.ProvinceCityID WHERE ISNULL(c.CustomerCommune, '') <> '' AND t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%" + html.DomainID + "%'",
                            DisplayOnFields= JsonConvert.SerializeObject(new {
                                CompanyDistrict="District",
                                CompanyProvinceCity="ProvinceCity",
                            }),
                            placeholder="Enter Company Commune"
                        },
                        new HtmlTextBox()
                        {
                            Label="ខណ្ឌ/District:", ID="CompanyDistrict",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyDistrict", IsReadOnly = true,
                            placeholder="Enter Company District"
                        },
                        new HtmlTextBox()
                        {
                            Label="ខេត្ត/ក្រុង/Province/City:", ID="CompanyProvinceCity",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyProvinceCity", IsReadOnly = true,
                            placeholder="Enter Company Province City"
                        },
                    },
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="ឈ្មោះទំនាក់ទំនង/Contact Person:", ID="CompanyContactName",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyContactName",
                            placeholder="Enter Company Contact Name"
                        },
                        new HtmlTextBox()
                        {
                            Label="តំណែង/Position:", ID="CompanyContactPosition",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyContactPosition",
                            placeholder="Enter Company Contact Position"
                        },
                        new HtmlTextBox()
                        {
                            Label="លេខទំនាក់ទំនង/Contact Number:", ID="CompanyContactMobileNumber",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CompanyContactMobileNumber",
                            placeholder="Enter Company Contact Mobile Number"
                        },
                    },
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="អាសយដ្ឋានទូទាត់វិក្កយបត្រ/Bill Add.:", ID="BillToAddress", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="BillToAddress",
                            placeholder="Enter Bill To Address"
                        },
                        new HtmlTextBox()
                        {
                            Label="សង្កាត់/Commune:", ID="BillToCommune", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="BillToCommune", isTypeahead = true,
                            SQL=@"SELECT DISTINCT t.CommuneID, c.CustomerCommune BillToCommune, d.DistrictID, CASE WHEN c.Lang = 'KH' THEN d.DistrictKH ELSE d.District END AS District, pc.ProvinceCityID, CASE WHEN c.Lang = 'KH' THEN pc.ProvinceCityKH ELSE pc.ProvinceCity END AS ProvinceCity FROM dbo.TblCommune t CROSS APPLY (VALUES (t.Commune, 'EN'), (t.CommuneKH, 'KH')) c(CustomerCommune, Lang) LEFT JOIN dbo.TblDistrict d ON t.DistrictID = d.DistrictID LEFT JOIN dbo.TblProvinceCity pc ON pc.ProvinceCityID = d.ProvinceCityID WHERE ISNULL(c.CustomerCommune, '') <> '' AND t.SystemID = " + html.SystemID + " and t.WarehouseID = " + html.WarehouseID + " and t.DomainID like '%" + html.DomainID + "%'",
                            DisplayOnFields= JsonConvert.SerializeObject(new {
                                BillToDistrict="District",
                                BillToProvinceCity="ProvinceCity",
                            }),
                            placeholder="Enter Bill To Commune"
                        },
                        new HtmlTextBox()
                        {
                            Label="ខណ្ឌ/District:", ID="BillToDistrict",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="BillToDistrict", IsReadOnly = true,
                            placeholder="Enter Bill To District"
                        },
                        new HtmlTextBox()
                        {
                            Label="ខេត្ត/ក្រុង/Province/City:", ID="BillToProvinceCity",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="BillToProvinceCity", IsReadOnly = true,
                            placeholder="Enter Bill To Province City"
                        },
                    },
                },
                new FormData() {
                    FormTitle="ភ្នាក់ងារ ទទួលកម្រៃជើងសារ / Agency Commission Info",
                    FormTitleID = "AgencyCommission",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlComboBox()
                        {
                            Label="ប្រភេទ កម្រៃជើងសារ / Commission Type:", ID="AgencyCommissionType",
                            Class="input2 CustInfo", IsCustomForm = true, DivClassCol = "col-sm-auto",
                            DBField="CommissionTypeID",
                            SQL_CBO = $@"select 1 CommissionTypeID, 'Once Time' CommissionType union all select 2 CommissionTypeID, 'On Package' CommissionType",
                            DisplayText="CommissionType",
                            Value="CommissionTypeID",
                            //Multiple = true,
                            placeholder="Enter Commission Type"
                        },
                        new HtmlTextBox()
                        {
                            Label="កម្រៃជើងសារ / Commission Amount USD:", ID="CommissionAmountUSD",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CommissionAmountUSD", Value = "0",
                            placeholder="Enter Commission Amount USD"
                        },
                        new HtmlTextBox()
                        {
                            Label="កម្រៃជើងសារ / Commission Amount %:", ID="CommissionAmountP",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="CommissionAmountP", Value = "0",
                            placeholder="Enter Commission Amount %"
                        },
                    },
                },
                new FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="កំណត់សំគាល់ / Noted:", ID="Memo",
                            Class="input2 CustInfo", IsCustomForm = true, DivGroup = true,
                            DBField="Memo",
                            placeholder="Enter Noted"
                        },
                    },
                },
                new  FormData() {
                    FormTitle = "សេវាកម្ម / Service Plan",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ScriptCallBackFunction= "calculateTotals()",
                            FooterColspan=2,
                            SubTotalColspan= 5,
                            ShowDeposit = false,
                            ShowBalance = false,
                            ISInputVAT = true,
                            CustomerNotesLabel="កំណត់សំគាល់ / Invoice Noted",
                            HtmlTblID="TableItemInformation",
                            InputName="OtherCharge",
                            TRName="TRNo",
                            TDName="TDNo",
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="ល.រ"
                                },
                                new TblTHead()
                                {
                                    Label="បរិយាយ", Class="col-6"
                                },
                                new TblTHead()
                                {
                                     Label="ខ្នាត", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="បរិមាណ", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="ថ្លៃឯកតា", Class="col-1"
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
                                     Label="Description", Class="col-6"
                                },
                                new TblTHead()
                                {
                                     Label="UOM", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="QTY", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="Unit Price", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="Dis%", Class="col-1"
                                },
                                new TblTHead()
                                {
                                     Label="Dis$", Class="col-1"
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
                                            ID="OtherCharge",Name="OtherCharge", //Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="OtherCharge", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','OtherCharge','calculateTotals');",
                                            HiddenField = "OtherChargeID"
                                        },
                                        new HtmlTextBox()
                                        {
                                            ID="CustomerInvoiceID",Name="CustomerInvoiceID",
                                            DBField="CustomerInvoiceID", Hide=true
                                        },
                                        new HtmlTextBox()
                                        {
                                            ID="ItemID",Name="ItemID",
                                            DBField="ItemID", Hide=true
                                        },
                                        new HtmlTextBox()
                                        {
                                            ID="ServiceID",Name="ServiceID",
                                            DBField="ServiceID", Hide=true
                                        },
                                        new HtmlTextBox()
                                        {
                                            ID="UM",Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM", //Required = true,
                                            SQL = html.SystemID == "27" ? "select UomID, [Name] UM from dbo.TblUom where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by [Name]" : "",
                                            isTypeahead= html.SystemID == "27",
                                            HiddenField="UomID",
                                        },
                                        new HtmlTextBox()
                                        {
                                            ID="QTY",Name="QTY", DivClass="wrap-input3 text-center", //Required=true,
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                            Digit="0"
                                        },
                                        new HtmlTextBox()
                                        {
                                            ID="UnitPrice",Name="UnitPrice", //Required=true, 
											DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UnitPrice", IsAllowInputOnlyNumber =true,
                                            Digit="0.000", Value = "0",
                                        },
                                        new HtmlTextBox()
                                        {
                                            ID="DisP",Name="DisP", DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="DiscountP", IsAllowInputOnlyNumber =true,
                                            Digit="0", Value = "0",
                                        },
                                        new HtmlTextBox()
                                        {
                                            ID="DisUSD",Name="DisUSD", DivClass="wrap-input3",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DBField="DiscountUSD", Value = "0", IsAllowInputOnlyNumber =true,
                                            HiddenField="DisP", Digit="0.00"
                                        },
                                        new HtmlTextBox()
                                        {
                                            ID="Amount",Name="Amount", //Required=true, 
											DivClass="wrap-input3",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Amount", Value = "0", Digit="0.000",
                                            IsAllowInputOnlyNumber =true,
                                        }
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            html.SaveChangeFunction = "SaveChangeCustomer()";
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLFormV1 = html.DrawHTMLFormV1();
            return View();
        }

        public JsonResult LoadInvoicePrefix(string formType, string invoiceType)
        {
            HTML html = new HTML();

            string prefix = formType == "Quotation" && invoiceType != "None VAT" ? "Q/{yy}-{000000}"
                : formType == "Quotation" && invoiceType == "None VAT" ? "{yy}-{000000}"
                : invoiceType == "VAT" ? "NSM/TI/{yy}-{000000}"
                : invoiceType == "CM" ? "NSM/CM/{yy}-{000000}"
                : "CISP{yy}-{000000}";

            string SQL = @"
                DECLARE @Module nvarchar(50) = @pModule;
                EXEC [SP_SHOW_PREFIX] @pPrefix, @Module, @SystemID, @WarehouseID, @DomainID;
            ";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "pPrefix", prefix },
                { "pModule", "Customer Billing Prefix" },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };

            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomerBilling(int CustomerID, string InvoiceNo = "", bool IsRenew = false, bool IsChangePlan = false)
        {
            HTML html = new HTML();
            var para = new Dictionary<string, string>
            {
                { "@CustomerID", CustomerID.ToString() },
                { "@InvoiceNo", InvoiceNo },
                { "@IsRenew", IsRenew ? "1" : "0" },
                { "@IsChangePlan", IsChangePlan ? "1" : "0" },
            };
            var customerData = html.JsonData("SP_GET_CUSTOMER_DETAIL", true, para, true);

            return Json(new { data = customerData }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomerName()
        {
            using (var ctx = new DBContext())
            {
                var html = new HTML();
                var data = ctx.TblCustomer
                    .Where(c => c.SystemID == 13
                        && c.DomainID.Contains(html.DomainID)
                        && (string.IsNullOrEmpty(c.CustomerStatus) || c.CustomerStatus != "Disable"))
                    .Select(c => new
                    {
                        c.CustomerID,
                        c.CustomerCode,
                        c.CustomerName,
                        c.CustomerNameLatin
                    })
                    .ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<(bool IsValid, string Message)> ValidateCustomerNameAsync(DBContext ctx, string nameKh, string nameLatin)
        {
            bool checkKh = !string.IsNullOrEmpty(nameKh);
            bool checkLatin = !string.IsNullOrEmpty(nameLatin);

            if (!checkKh && !checkLatin)
                return (true, "");

            var existCustomer = await ctx.TblCustomer
                .Where(c =>
                    (checkKh && c.CustomerName == nameKh) ||
                    (checkLatin && c.CustomerNameLatin == nameLatin)
                )
                .Select(c => new
                {
                    c.CustomerName,
                    c.CustomerNameLatin
                })
                .FirstOrDefaultAsync();

            if (existCustomer == null)
                return (true, "");

            bool khDuplicated = checkKh && existCustomer.CustomerName == nameKh;
            bool latinDuplicated = checkLatin && existCustomer.CustomerNameLatin == nameLatin;

            if (khDuplicated && latinDuplicated)
                return (false, "Customer Khmer name and Latin name already exist");

            if (khDuplicated)
                return (false, "Customer Khmer name already exists");

            return (false, "Customer Latin name already exists");
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> SaveChangeCustomerV2(string data)
        {
            HTML html = new HTML();
            JObject obj = JObject.Parse(data);

            int customerId = Utility.TryParse(obj["CustomerName_CustomerID"]?.ToString(), "int", 0);
            int autoFillId = Utility.TryParse(obj["CustomerName_AUTOFill"]?.ToString(), "int", 0);
            bool isRenew = Utility.TryParse(obj["IsRenew"]?.ToString(), "bool", false);
            bool isChangePlan = Utility.TryParse(obj["IsChangePlan"]?.ToString(), "bool", false);
            bool isAutoFill = autoFillId > 0;

            string nameKH = obj["CustomerName"]?.ToString()?.Trim();
            string nameLatin = obj["CustomerNameLatin"]?.ToString()?.Trim();
            string gender = obj["CustomerGender"]?.ToString();
            string nationality = obj["CustomerNationality"]?.ToString();
            string typeOfInvoice = obj["SetCID"]?.ToString();
            string formType = obj["FormType"]?.ToString();
            string invoiceNo = obj["InvoiceNo"]?.ToString();
            bool isNewRegister = formType == "NewRegister";
            string salutation = nationality == "ខ្មែរ"
                ? (gender == "Male" ? "លោក" : "អ្នកស្រី")
                : (gender == "Male" ? "Mr." : "Ms.");

            int SystemID = Utility.TryParse(html.SystemID, "int", 13);
            int WarehouseID = Utility.TryParse(html.WarehouseID, "int", 16);
            string domainId = (typeOfInvoice == "VAT" || typeOfInvoice == "CM") ? "001,002" : "001";

            using (var ctx = new DBContext())
            using (var tran = ctx.Database.BeginTransaction())
            {
                try
                {
                    bool isEdit = false;
                    var existInvoiceNo = await ctx.TblCustomerInvoice
                            .Where(ci => ci.CustomerID == customerId && ci.InvoiceNo == invoiceNo)
                            .FirstOrDefaultAsync();

                    isEdit = existInvoiceNo != null;

                    #region GET EXCHANGE RATE
                    var invoiceDate = DateTime.TryParse(obj["InvoiceDate"]?.ToString(), out var d) ? d : DateTime.Now;
                    var ex = await ctx.Database.SqlQuery<ExchangeRateDTO>(
                            @"SELECT TOP 1 ex.ExchangeRate
					FROM dbo.TblExchangeRate ex
					INNER JOIN dbo.TblCurrency c ON c.CurrencyID = ex.CurrencyID
					WHERE c.CurrencyName = 'Riel'
					AND ex.SystemID = @SystemID
					ORDER BY ex.DateEntry DESC",
                            new SqlParameter("@SystemID", html.SystemID)
                        ).FirstOrDefaultAsync();
                    #endregion

                    #region CUSTOMER (UPSERT)
                    var customer = await ctx.TblCustomer.FindAsync(customerId);
                    customer.CustomerCode = customer.CustomerCode ?? obj["CustomerCode"]?.ToString();
                    customer.DomainID = customer.DomainID ?? domainId;
                    customer.IndustryTypeID = Utility.TryParse(obj["IndustryType_IndustryTypeID"]?.ToString(), "int", 0);
                    
                    await ctx.SaveChangesAsync();

                    // FIX: refresh customerId so every later query uses the real (new) ID
                    customerId = customer.CustomerID;
                    #endregion

                    #region EDIT CLEANUP
                    if (isEdit && !isNewRegister)
                    {
                        ctx.TblCustomerBillInformation.RemoveRange(
                            ctx.TblCustomerBillInformation.Where(x =>
                                x.CustomerID == customer.CustomerID && x.InvoiceNo == invoiceNo)
                        );

                        ctx.TblCommission.RemoveRange(
                            ctx.TblCommission.Where(x =>
                                x.CustomerID == customer.CustomerID && x.InvoiceNo == invoiceNo)
                        );

                        ctx.TblCustomerBalance.RemoveRange(
                            ctx.TblCustomerBalance.Where(x =>
                                x.CustomerID == customer.CustomerID && x.InvoiceNo == invoiceNo)
                        );

                        ctx.TblCustomerInvoice.RemoveRange(
                            ctx.TblCustomerInvoice.Where(x =>
                                x.CustomerID == customer.CustomerID 
                                && x.InvoiceNo == invoiceNo
                                && (x.ServiceID == 0 || x.PackageDetailID == null || x.PackageDetailID == 0)
                                && x.OtherChargeID != null
                            )
                        );

                        ctx.TblCustomerPayment.RemoveRange(
                            ctx.TblCustomerPayment.Where(x =>
                                x.CustomerID == customer.CustomerID && x.InvoiceNo == invoiceNo)
                        );

                        ctx.TblCustomerCompany.RemoveRange(
                            ctx.TblCustomerCompany.Where(x => x.CustomerID == customer.CustomerID)
                        );

                        ctx.TblCustomerInfra.RemoveRange(
                            ctx.TblCustomerInfra.Where(x =>
                                x.CustomerID == customer.CustomerID && x.InvoiceNo == invoiceNo)
                        );

                        ctx.TblCustomerBankSlip.RemoveRange(
                            ctx.TblCustomerBankSlip.Where(x =>
                                x.CustomerID == customer.CustomerID && x.InvoiceNo == invoiceNo)
                        );

                        ctx.TblCustomerAttachmentFile.RemoveRange(
                            ctx.TblCustomerAttachmentFile.Where(x =>
                                x.CustomerID == customer.CustomerID && x.InvoiceNo == invoiceNo)
                        );

                        await ctx.SaveChangesAsync();
                    }
                    #endregion

                    #region BILL INFO (UPSERT)
                    var billInfo = await ctx.TblCustomerBillInformation
                        .Where(x =>
                            x.CustomerID == customer.CustomerID &&
                            (string.IsNullOrEmpty(invoiceNo) || x.InvoiceNo == invoiceNo)
                        )
                        .FirstOrDefaultAsync();

                    if (billInfo == null)
                    {
                        billInfo = new BillInfo
                        {
                            CustomerID = customer.CustomerID,
                            CreateBy = html.UserFullName,
                            CreateDate = DateTime.Now
                        };
                        ctx.TblCustomerBillInformation.Add(billInfo);
                    }
                    billInfo.ATTName = obj["ATTName"]?.ToString();
                    billInfo.CustomerCode = customer.CustomerCode;
                    billInfo.CustomerName = customer.CustomerName;
                    billInfo.CompanyName = obj["CompanyName"]?.ToString();
                    billInfo.CompanyNameKH = obj["CompanyNameKH"]?.ToString();
                    billInfo.VATNumber = obj["VATNumber"]?.ToString();
                    billInfo.CompanyDeskPhone = obj["CompanyDeskPhone"]?.ToString();
                    billInfo.CompanyMobile = obj["CompanyMobile"]?.ToString();
                    billInfo.CompanyEmail = obj["CompanyEmail"]?.ToString();
                    billInfo.CompanyAddress = obj["CompanyAddress"]?.ToString();
                    billInfo.CompanyCommune = obj["CompanyCommune"]?.ToString();
                    billInfo.CompanyDistrict = obj["CompanyDistrict"]?.ToString();
                    billInfo.CompanyProvinceCity = obj["CompanyProvinceCity"]?.ToString();
                    billInfo.CompanyContactName = obj["CompanyContactName"]?.ToString();
                    billInfo.CompanyContactPosition = obj["CompanyContactPosition"]?.ToString();
                    billInfo.CompanyContactMobileNumber = obj["CompanyContactMobileNumber"]?.ToString();
                    billInfo.InvoiceNo = obj["InvoiceNo"]?.ToString();
                    billInfo.InstallDate = Utility.TryParse(obj["InstallDate"]?.ToString(), "datetime", null);
                    billInfo.InvoiceDate = Utility.TryParse(obj["InvoiceDate"]?.ToString(), "datetime", null);
                    billInfo.StartDate = Utility.TryParse(obj["StartDate"]?.ToString(), "datetime", null);
                    billInfo.EndDate = Utility.TryParse(obj["EndDate"]?.ToString(), "datetime", null);
                    billInfo.NextInvoiceDate = Utility.TryParse(obj["NextInvoiceDate"]?.ToString(), "datetime", null);
                    billInfo.PaymentDueDate = Utility.TryParse(obj["PaymentDueDate"]?.ToString(), "datetime", null);
                    billInfo.TermCondition = obj["TermsConditions"]?.ToString();
                    billInfo.IsIncludedVAT = Utility.TryParse(obj["IsIncludedVAT"]?.ToString(), "bool", false);
                    billInfo.BillToAddress = obj["BillToAddress"]?.ToString();
                    billInfo.BillToCommune = obj["BillToCommune"]?.ToString();
                    billInfo.BillToDistrict = obj["BillToDistrict"]?.ToString();
                    billInfo.BillToProvinceCity = obj["BillToProvinceCity"]?.ToString();
                    billInfo.InstallAddress = obj["InstallAddress"]?.ToString();
                    billInfo.InstallCommune = obj["InstallCommune"]?.ToString();
                    billInfo.InstallDistrict = obj["InstallDistrict"]?.ToString();
                    billInfo.InstallProvinceCity = obj["InstallProvinceCity"]?.ToString();
                    billInfo.GoogleMap_FullAddress = obj["GoogleMap_FullAddress"]?.ToString();
                    billInfo.GoogleMap_Latitude = obj["GoogleMap_Latitude"]?.ToString();
                    billInfo.GoogleMap_Longitude = obj["GoogleMap_Longitude"]?.ToString();
                    billInfo.ExchangeRate = ex?.ExchangeRate ?? 0;
                    billInfo.Type = formType == "Quotation" ? "Quotation Invoice" : "Official Invoice";
                    billInfo.InvoiceType = isRenew || isChangePlan ? "Renew Invoice" : "Created Invoice";
                    billInfo.IspLinkID = Utility.TryParse(obj["IspLinkName_IspLinkID"]?.ToString(), "int", 0);
                    billInfo.ConnectivityStatus = 43;
                    billInfo.invTmp = 1;
                    billInfo.StatusID = 40;
                    billInfo.InvoiceTmpID = 62;
                    billInfo.ConnectionID = obj["ConnectionID"]?.ToString();
                    billInfo.ClassName = obj["ClassName"]?.ToString();
                    billInfo.ClassNameID = Utility.TryParse(obj["ClassName_ClassID"]?.ToString(), "int", 0);
                    billInfo.EmployeeID = customer.EmployeeID;
                    billInfo.IndustryTypeID = customer.IndustryTypeID;
                    billInfo.SystemID = SystemID;
                    billInfo.WarehouseID = WarehouseID;
                    billInfo.DomainID = domainId;

                    await ctx.SaveChangesAsync();
                    #endregion

                    #region CUSTOMER INFRASTRUCTURE (UPSERT)
                    var infraName = obj["CustomerInfraName"]?.ToString();
                    var surveyId = obj["CustomerInfraSurveyID"]?.ToString();
                    var cid = obj["CustomerInfraCID"]?.ToString();
                    var sid = obj["CustomerInfraSID"]?.ToString();
                    var aid = obj["CustomerInfraAID"]?.ToString();
                    var infraProviderId = Utility.TryParse(obj["InfraProviderName_InfraProviderID"]?.ToString(), "int", 0);
                    bool hasInfraData =
                        !string.IsNullOrWhiteSpace(infraName) ||
                        !string.IsNullOrWhiteSpace(surveyId) ||
                        !string.IsNullOrWhiteSpace(cid) ||
                        !string.IsNullOrWhiteSpace(sid) ||
                        !string.IsNullOrWhiteSpace(aid) ||
                        infraProviderId != 0;

                    if (!isNewRegister && hasInfraData)
                    {
                        var customerInfra = await ctx.TblCustomerInfra
                            .Where(x => x.CustomerID == customer.CustomerID && x.InvoiceNo == invoiceNo)
                            .FirstOrDefaultAsync();

                        if (customerInfra == null)
                        {
                            customerInfra = new CustomerInfra
                            {
                                CustomerID = customer.CustomerID,
                                CreateBy = html.UserFullName,
                                CreateDate = DateTime.Now,
                                UserID = Utility.TryParse(html.UserID, "int", 0)
                            };

                            ctx.TblCustomerInfra.Add(customerInfra);
                        }
                        else
                        {
                            customerInfra.ModifyDate = DateTime.Now;
                            customerInfra.ModifyBy = html.UserFullName;
                        }

                        customerInfra.InvoiceNo = invoiceNo;
                        customerInfra.InfraProviderID = infraProviderId;
                        customerInfra.CustomerInfraName = infraName;
                        customerInfra.CustomerInfraSurveyID = surveyId;
                        customerInfra.CustomerInfraCID = cid;
                        customerInfra.CustomerInfraSID = sid;
                        customerInfra.CustomerInfraAID = aid;
                        customerInfra.CID = customer?.CustomerCode;
                        customerInfra.SID = obj["SID"]?.ToString();
                        customerInfra.SystemID = SystemID;
                        customerInfra.WarehouseID = WarehouseID;
                        customerInfra.DomainID = domainId;
                        await ctx.SaveChangesAsync();
                    }
                    #endregion

                    #region CUSTOMER COMPANY (UPSERT)
                    var customerCompany = await ctx.TblCustomerCompany
                        .FirstOrDefaultAsync(x => x.CustomerID == customer.CustomerID);

                    if (customerCompany == null)
                    {
                        customerCompany = new CustomerCompany
                        {
                            CustomerID = customer.CustomerID,
                        };

                        ctx.TblCustomerCompany.Add(customerCompany);
                    }
                    customerCompany.IndustryTypeID = customer.IndustryTypeID;
                    customerCompany.CompanyName = obj["CompanyName"]?.ToString();
                    customerCompany.CompanyNameKH = obj["CompanyNameKH"]?.ToString();
                    customerCompany.VATNumber = obj["VATNumber"]?.ToString();
                    customerCompany.CompanyDeskPhone = obj["CompanyDeskPhone"]?.ToString();
                    customerCompany.CompanyMobile = obj["CompanyMobile"]?.ToString();
                    customerCompany.CompanyEmail = obj["CompanyEmail"]?.ToString();
                    customerCompany.CompanyAddress = obj["CompanyAddress"]?.ToString();
                    customerCompany.CompanyCommune = obj["CompanyCommune"]?.ToString();
                    customerCompany.CompanyDistrict = obj["CompanyDistrict"]?.ToString();
                    customerCompany.CompanyProvinceCity = obj["CompanyProvinceCity"]?.ToString();
                    customerCompany.CompanyContactName = obj["CompanyContactName"]?.ToString();
                    customerCompany.CompanyContactPosition = obj["CompanyContactPosition"]?.ToString();
                    customerCompany.CompanyContactMobileNumber = obj["CompanyContactMobileNumber"]?.ToString();
                    customerCompany.IsIncludedVAT = Utility.TryParse(obj["IsIncludedVAT"]?.ToString(), "bool", false);
                    customerCompany.SystemID = SystemID;
                    customerCompany.WarehouseID = WarehouseID;
                    customerCompany.DomainID = domainId;
                    await ctx.SaveChangesAsync();
                    #endregion

                    #region CUSTOMER COMMISSION
                    List<Commission> agencyCommission = new List<Commission>();

                    foreach (JObject commission in (obj["AgencyCommissionType"] as JArray ?? new JArray()).Cast<JObject>())
                    {
                        agencyCommission.Add(new Commission
                        {
                            CustomerID = customer.CustomerID,
                            CommissionTypeID = commission["CommissionTypeID"]?.ToString(),
                            CommissionAmountUSD = Utility.TryParse(commission["CommissionAmountUSD"]?.ToString(), "decimal", 0),
                            CommissionAmountP = commission["CommissionAmountP"]?.ToString(),
                            InvoiceNo = obj["InvoiceNo"]?.ToString(),
                            Memo = commission["Memo"]?.ToString(),
                            SystemID = SystemID,
                            WarehouseID = WarehouseID,
                            DomainID = domainId,
                            UserID = Utility.TryParse(html.UserID, "int", 0),
                            CreateBy = html.UserFullName,
                            CreateDate = DateTime.Now
                        });
                    }

                    ctx.TblCommission.AddRange(agencyCommission);
                    await ctx.SaveChangesAsync();
                    #endregion

                    #region Renew Connection (LOAD OLD)
                    var oldInvoice = new CustomerInvoice();
                    var oldBillingInvoice = new CustomerBillingInvoice();
                    string refNo = obj["ReferenceNo"]?.ToString();
                    string SID = obj["SID"]?.ToString();
                    string ConnectionID = obj["ConnectionID"]?.ToString();

                    oldInvoice = await ctx.TblCustomerInvoice
                        .Where(i => i.CustomerID == customerId && i.InvoiceNo == refNo
                            && i.ServiceID != null && i.ServiceID != 0)
                        .FirstOrDefaultAsync();

                    oldBillingInvoice = await ctx.TblCustomerBillingInvoice
                        .Where(i => i.CustomerID == customerId && i.InvoiceNo == refNo
                            && i.ServiceID != null && i.ServiceID != 0)
                        .FirstOrDefaultAsync();
                    #endregion

                    #region CUSTOMER INVOICE (SERVICE PLAN)
                    CustomerInvoice lastInvoice = null;
                    var billingInvoices = new List<CustomerBillingInvoice>();
                    var serviceInvoices = new List<CustomerInvoice>();

                    foreach (JObject plan in (obj["CustomerServicePlan"] as JArray ?? new JArray()).Cast<JObject>())
                    {
                        // ====== BILLING UPSERT ======
                        var billing = await ctx.TblCustomerBillingInvoice
                            .Where(x =>
                                x.CustomerID == customer.CustomerID && x.InvoiceNo == invoiceNo
                            )
                            .FirstOrDefaultAsync();

                        bool isNewBilling = billing == null;
                        if (isNewBilling)
                        {
                            billing = new CustomerBillingInvoice
                            {
                                CreateBy = html.UserFullName,
                                CreateDate = DateTime.Now
                            };
                            ctx.TblCustomerBillingInvoice.Add(billing);
                        }
                        else
                        {
                            billing.ModifyBy = html.UserFullName;
                            billing.ModifyDate = DateTime.Now;
                        }

                        billing.CustomerID = customer.CustomerID;
                        billing.CustomerBillInformationID = billInfo?.CustomerBillInformationID;
                        billing.SID = obj["SID"]?.ToString();
                        billing.ConnectionID = obj["ConnectionID"]?.ToString();
                        billing.EmployeeID = customer.EmployeeID;
                        billing.ServiceID = Utility.TryParse(plan["ServiceID"]?.ToString(), "int", 0);
                        billing.ServiceName = plan["ServicePlan"]?.ToString();
                        billing.PackageName = plan["PackageName"]?.ToString();
                        billing.PackageID = Utility.TryParse(plan["PackageID"]?.ToString(), "int", 0);
                        billing.PackageDetailID = Utility.TryParse(plan["PackageDetailID"]?.ToString(), "int", 0);
                        billing.PID = plan["PID"]?.ToString();
                        billing.Duration = Utility.TryParse(plan["Duration"]?.ToString(), "int", 0);
                        billing.QTY = plan["QTY"]?.Value<int>() ?? 0;
                        billing.UM = plan["UM"]?.ToString();
                        billing.UnitPrice = plan["UnitPrice"]?.Value<decimal>() ?? 0;
                        billing.DiscountP = plan["DisP"]?.Value<decimal>() ?? 0;
                        billing.DiscountUSD = plan["DisUSD"]?.Value<decimal>() ?? 0;
                        billing.Amount = plan["Amount"]?.Value<decimal>() ?? 0;
                        billing.InvoiceNo = obj["InvoiceNo"]?.ToString();
                        billing.OldInvoiceNo = refNo;
                        billing.InstallDate = Utility.TryParse(obj["InstallDate"]?.ToString(), "datetime", null);
                        billing.InvoiceDate = Utility.TryParse(obj["InvoiceDate"]?.ToString(), "datetime", null);
                        billing.PaymentDueDate = Utility.TryParse(obj["PaymentDueDate"]?.ToString(), "datetime", null);
                        billing.StartDate = Utility.TryParse(obj["StartDate"]?.ToString(), "datetime", null);
                        billing.EndDate = Utility.TryParse(obj["EndDate"]?.ToString(), "datetime", null);
                        billing.NextInvoiceDate = Utility.TryParse(obj["NextInvoiceDate"]?.ToString(), "datetime", null);
                        billing.EndNextInvoiceDate = Utility.TryParse(obj["EndNextInvoiceDate"]?.ToString(), "datetime", null);
                        billing.IsIncludedVAT = Utility.TryParse(obj["IsIncludedVAT"]?.ToString(), "bool", false);
                        billing.StatusID = 11;
                        billing.invTmp = 1;
                        billing.IsRenew = false;
                        billing.IsChangePlan = isChangePlan;
                        billing.InvoiceType = isRenew || isChangePlan ? "Renew Invoice" : "Created Invoice";
                        billing.Type = formType == "Quotation" ? "Quotation Invoice" : "Official Invoice";
                        billing.TypeOfInvoice = typeOfInvoice;
                        billing.ConnectivityStatus = 43;
                        billing.ConnectivityStatusName = "Not yet activate";
                        billing.RouterID = isRenew || isChangePlan ? oldBillingInvoice?.RouterID : billing?.RouterID;
                        billing.ActivateCustomerID = isRenew || isChangePlan ? oldBillingInvoice?.ActivateCustomerID : billing?.ActivateCustomerID;
                        billing.ActivateDate = isRenew || isChangePlan ? oldBillingInvoice?.ActivateDate : billing?.ActivateDate;
                        billing.EndServiceDate = isRenew || isChangePlan ? oldBillingInvoice?.EndServiceDate : billing?.EndServiceDate;
                        billing.ExpireDate = isRenew || isChangePlan ? oldBillingInvoice?.ExpireDate : billing?.ExpireDate;
                        billing.PPPoEUserName = isRenew || isChangePlan ? oldBillingInvoice?.PPPoEUserName : billing?.PPPoEUserName;
                        billing.PPPoEPassword = isRenew || isChangePlan ? oldBillingInvoice?.PPPoEPassword : billing?.PPPoEPassword;
                        billing.ConnectionType = isRenew || isChangePlan ? oldBillingInvoice?.ConnectionType : billing?.ConnectionType;
                        billing.OldBillingInvoiceID = isRenew || isChangePlan ? oldBillingInvoice?.BillingInvoiceID : billing?.BillingInvoiceID;
                        billing.ExchangeRate = billInfo.ExchangeRate;
                        billing.ChartOfAccountID = Utility.TryParse(plan["ChartOfAccountID"]?.ToString(), "int", 0);
                        billing.ClassName = obj["ClassName"]?.ToString();
                        billing.ClassNameID = Utility.TryParse(obj["ClassName_ClassID"]?.ToString(), "int", 0);
                        billing.SystemID = SystemID;
                        billing.WarehouseID = WarehouseID;
                        billing.DomainID = domainId;

                        billingInvoices.Add(billing);
                    }

                    await ctx.SaveChangesAsync(); // Save billing first to get BillingInvoiceIDs

                    // ====== SERVICE INVOICE UPSERT ======
                    // Rebuild lookup by index to avoid duplicate ServiceID collision
                    var planList = (obj["CustomerServicePlan"] as JArray ?? new JArray()).ToList();

                    for (int i = 0; i < billingInvoices.Count; i++)
                    {
                        var billing = billingInvoices[i];
                        var plan = i < planList.Count ? planList[i] as JObject : null;
                        if (plan == null) continue;

                        var servicePlan = await ctx.TblCustomerInvoice
                             .Where(x =>
                                 x.CustomerID == customer.CustomerID
                                 && x.InvoiceNo == invoiceNo
                                 && x.BillingInvoiceID == billing.BillingInvoiceID
                                 && x.ServiceID == billing.ServiceID
                                 && x.InvoiceType != null
                             )
                             .FirstOrDefaultAsync();

                        bool isNewInvoice = servicePlan == null;
                        if (isNewInvoice)
                        {
                            servicePlan = new CustomerInvoice
                            {
                                CreateBy = html.UserFullName,
                                CreateDate = DateTime.Now
                            };
                            ctx.TblCustomerInvoice.Add(servicePlan);
                        }
                        else
                        {
                            servicePlan.ModifyBy = html.UserFullName;
                            servicePlan.ModifyDate = DateTime.Now;
                        }

                        servicePlan.CustomerID = customer.CustomerID;
                        servicePlan.CustomerCode = customer.CustomerCode;
                        servicePlan.SID = obj["SID"]?.ToString();
                        servicePlan.ConnectionID = obj["ConnectionID"]?.ToString();
                        servicePlan.CustomerName = customer.CustomerName;
                        servicePlan.CustomerTypeID = customer.CustomerTypeID;
                        servicePlan.ATTName = obj["ATTName"]?.ToString();
                        servicePlan.ServiceID = billing.ServiceID;
                        servicePlan.ServiceName = billing.ServiceName;
                        servicePlan.PackageID = billing.PackageID;
                        servicePlan.PackageDetailID = billing.PackageDetailID;
                        servicePlan.PID = billing.PID;
                        servicePlan.Duration = billing.Duration;
                        servicePlan.ItemDescription = plan["PackageName"]?.ToString();
                        servicePlan.Feature = plan["Feature"]?.ToString();
                        servicePlan.DowSpeed = Utility.TryParse(plan["DownSpeed"]?.ToString(), "int", 0);
                        servicePlan.UpSpeed = Utility.TryParse(plan["UpSpeed"]?.ToString(), "int", 0);
                        servicePlan.Bandwidth = Utility.TryParse(plan["Bandwidth"]?.ToString(), "int", 0);
                        servicePlan.QTY = billing.QTY;
                        servicePlan.UM = billing.UM;
                        servicePlan.UnitPrice = billing.UnitPrice;
                        servicePlan.DiscountP = billing.DiscountP;
                        servicePlan.DiscountUSD = billing.DiscountUSD;
                        servicePlan.Amount = billing.Amount;
                        servicePlan.InstallDate = billing.InstallDate;
                        servicePlan.InvoiceNo = billing.InvoiceNo;
                        servicePlan.ReferenceNo = obj["ReferenceNo"]?.ToString();
                        servicePlan.OldInvoiceNo = refNo;
                        servicePlan.InvoiceDate = billing.InvoiceDate;
                        servicePlan.StartDate = billing.StartDate;
                        servicePlan.EndDate = billing.EndDate;
                        servicePlan.NextInvoiceDate = billing.NextInvoiceDate;
                        servicePlan.EndNextInvoiceDate = billing.EndNextInvoiceDate;
                        servicePlan.PaymentDueDate = billing.PaymentDueDate;
                        servicePlan.ExpireDate = billing.ExpireDate;
                        servicePlan.IsIncludedVAT = Utility.TryParse(obj["IsIncludedVAT"]?.ToString(), "bool", false);
                        servicePlan.StatusID = 11;
                        servicePlan.IsRenew = false;
                        servicePlan.IsChangePlan = billing.IsChangePlan;
                        servicePlan.InvoiceType = billing.InvoiceType;
                        servicePlan.Type = formType == "Quotation" ? "Quotation Invoice" : "Official Invoice";
                        servicePlan.CustomerBillInformationID = billInfo.CustomerBillInformationID;
                        servicePlan.BillingInvoiceID = billing.BillingInvoiceID;
                        servicePlan.EmployeeID = customer.EmployeeID;
                        servicePlan.TypeOfInvoice = typeOfInvoice;
                        servicePlan.ConnectivityStatus = billing.ConnectivityStatus;
                        servicePlan.ConnectivityStatusName = billing.ConnectivityStatusName;
                        servicePlan.invTmp = 1;
                        servicePlan.InvoiceStatusID = 101;
                        servicePlan.InvoiceTmpID = 62;
                        servicePlan.OrderBy = 1;
                        servicePlan.InvoiceNote = obj["Customer_Notes"]?.ToString();
                        servicePlan.IsChangeFlow = true;
                        servicePlan.ExchangeRate = billInfo.ExchangeRate;
                        servicePlan.ChartOfAccountID = billing.ChartOfAccountID;
                        servicePlan.ClassNameID = billInfo.ClassNameID;
                        servicePlan.ClassName = billInfo.ClassName;
                        servicePlan.RegisterByUserLoginID = Utility.TryParse(html.UserID, "int", 0);
                        servicePlan.RequestedByUserIDToDMCTAX = Utility.TryParse(html.UserID, "int", 0);
                        servicePlan.RequestedByUserToDMCTAX = html.UserFullName;
                        servicePlan.RequestApprovalToDMCTAXID = Utility.TryParse(obj["RequestApprovalNameID"]?.ToString(), "int", 0);
                        servicePlan.RequestApprovalToDMCTAX = obj["RequestApprovalName"]?.ToString();
                        servicePlan.RequestApprovalToDMCTAXTgUser = obj["RequestApprovalTgUser"]?.ToString();
                        servicePlan.RequestToDMCTAXDate = DateTime.Now;
                        servicePlan.PPPoEUserName = billing.PPPoEUserName;
                        servicePlan.ActivateCustomerID = billing.ActivateCustomerID;
                        servicePlan.SystemID = SystemID;
                        servicePlan.WarehouseID = WarehouseID;
                        servicePlan.DomainID = domainId;

                        serviceInvoices.Add(servicePlan);
                        lastInvoice = servicePlan;
                    }

                    // ====== OTHER CHARGES UPSERT ======
                    int orderBy = 1;

                    var otherChargeList = (obj["CustomerOtherCharge"] as JArray ?? new JArray())
                        .Cast<JObject>();

                    foreach (JObject charge in otherChargeList)
                    {
                        var otherCharge = new CustomerInvoice
                        {
                            CreateBy = html.UserFullName,
                            CreateDate = DateTime.Now,

                            CustomerID = customer.CustomerID,
                            CustomerCode = customer.CustomerCode,
                            CustomerName = customer.CustomerName,
                            SID = obj["SID"]?.ToString(),
                            ConnectionID = obj["ConnectionID"]?.ToString(),
                            ATTName = obj["ATTName"]?.ToString(),
                            CustomerTypeID = customer.CustomerTypeID,
                            EmployeeID = customer.EmployeeID,
                            TypeOfInvoice = typeOfInvoice,

                            ServiceID = Utility.TryParse(charge["ServiceID"]?.ToString(), "int", 0),
                            ItemID = Utility.TryParse(charge["ItemID"]?.ToString(), "int", 0),
                            OtherChargeID = Utility.TryParse(charge["OtherChargeID"]?.ToString(), "int", 0),
                            ItemDescription = charge["OtherCharge"]?.ToString(),
                            UM = charge["UM"]?.ToString(),
                            QTY = charge["QTY"]?.Value<int>() ?? 0,
                            UnitPrice = charge["UnitPrice"]?.Value<decimal>() ?? 0,
                            DiscountP = charge["DisP"]?.Value<decimal>() ?? 0,
                            DiscountUSD = charge["DisUSD"]?.Value<decimal>() ?? 0,
                            Amount = charge["Amount"]?.Value<decimal>() ?? 0,

                            InvoiceNo = obj["InvoiceNo"]?.ToString(),
                            ReferenceNo = obj["ReferenceNo"]?.ToString(),
                            OldInvoiceNo = refNo,

                            InvoiceDate = Utility.TryParse(obj["InvoiceDate"]?.ToString(), "datetime", null),
                            PaymentDueDate = Utility.TryParse(obj["PaymentDueDate"]?.ToString(), "datetime", null),

                            StartDate = lastInvoice?.StartDate ?? DateTime.Now,
                            EndDate = lastInvoice?.EndDate ?? DateTime.Now,
                            ExpireDate = lastInvoice?.ExpireDate,

                            IsIncludedVAT = Utility.TryParse(obj["IsIncludedVAT"]?.ToString(), "bool", false),
                            CustomerBillInformationID = billInfo.CustomerBillInformationID,

                            DowSpeed = Utility.TryParse(charge["DownSpeed"]?.ToString(), "int", 0),
                            UpSpeed = Utility.TryParse(charge["UpSpeed"]?.ToString(), "int", 0),
                            Bandwidth = Utility.TryParse(charge["Bandwidth"]?.ToString(), "int", 0),
                            Feature = charge["Feature"]?.ToString(),

                            StatusID = 11,
                            IsRenew = false,
                            IsChangePlan = isChangePlan,
                            InvoiceStatusID = 101,
                            InvoiceTmpID = 62,
                            invTmp = 1,
                            OrderBy = orderBy,

                            IsChangeFlow = true,
                            ConnectivityStatus = 43,
                            ConnectivityStatusName = "Not yet activate",

                            InvoiceType = isRenew || isChangePlan ? "Renew Invoice" : "Created Invoice",
                            Type = formType == "Quotation" ? "Quotation Invoice" : "Official Invoice",

                            ExchangeRate = billInfo.ExchangeRate,
                            ChartOfAccountID = Utility.TryParse(charge["ChartOfAccountID"]?.ToString(), "int", 0),

                            ClassName = billInfo.ClassName,
                            ClassNameID = billInfo.ClassNameID,

                            RegisterByUserLoginID = Utility.TryParse(html.UserID, "int", 0),
                            RequestedByUserIDToDMCTAX = Utility.TryParse(html.UserID, "int", 0),
                            RequestedByUserToDMCTAX = html.UserFullName,

                            RequestApprovalToDMCTAXID = Utility.TryParse(obj["RequestApprovalNameID"]?.ToString(), "int", 0),
                            RequestApprovalToDMCTAX = obj["RequestApprovalName"]?.ToString(),
                            RequestApprovalToDMCTAXTgUser = obj["RequestApprovalTgUser"]?.ToString(),
                            RequestToDMCTAXDate = DateTime.Now,

                            PPPoEUserName = oldInvoice?.PPPoEUserName,
                            ActivateCustomerID = oldInvoice?.ActivateCustomerID,

                            SystemID = SystemID,
                            WarehouseID = WarehouseID,
                            DomainID = domainId
                        };

                        ctx.TblCustomerInvoice.Add(otherCharge);

                        serviceInvoices.Add(otherCharge);
                        lastInvoice = otherCharge;

                        orderBy++;
                    }

                    // ===================== CUSTOMER INVOICE HISTORY =====================
                    var invoiceHistory = await ctx.TblCustomerInvoiceHistory
                        .Where(i => i.CustomerID == customerId && i.InvoiceNo == invoiceNo)
                        .FirstOrDefaultAsync();

                    var invoiceHistories = serviceInvoices
                        .Where(invoice => invoice.Type == "Official Invoice")
                        .Select(invoice => new CustomerInvoiceHistory
                        {
                            CustomerBillInformationID = invoice.CustomerBillInformationID,
                            BillingInvoiceID = invoice.BillingInvoiceID,
                            TypeOfInvoice = invoice.TypeOfInvoice,
                            InvoiceNo = invoice.InvoiceNo,
                            CreditNoteNo = invoice.CreditNoteNo,
                            EmployeeID = invoice.EmployeeID,
                            SID = invoice.SID,
                            ConnectionID = invoice.ConnectionID,
                            CustomerID = invoice.CustomerID,
                            CustomerCode = invoice.CustomerCode,
                            CustomerName = invoice.CustomerName,
                            CustomerTypeID = invoice.CustomerTypeID,
                            ServiceID = invoice.ServiceID,
                            PackageID = invoice.PackageID,
                            PackageDetailID = invoice.PackageDetailID,
                            PID = invoice.PID,
                            OrderBy = invoice.OrderBy,
                            PackageCode = invoice.PackageCode,
                            ServiceName = invoice.ServiceName,
                            ItemDescription = invoice.ItemDescription,
                            Duration = invoice.Duration,
                            FreeDuration = invoice.FreeDuration,
                            DepositFee = invoice.DepositFee,
                            OtherChargeID = invoice.OtherChargeID,
                            StockInID = invoice.StockInID,
                            ItemID = invoice.ItemID,
                            UM = invoice.UM,
                            QTY = invoice.QTY,
                            UnitPrice = invoice.UnitPrice,
                            DiscountP = invoice.DiscountP,
                            DiscountUSD = invoice.DiscountUSD,
                            Amount = invoice.Amount,
                            InvoiceDate = invoice.InvoiceDate,
                            StartDate = invoice.StartDate,
                            EndDate = invoice.EndDate,
                            NextInvoiceDate = invoice.NextInvoiceDate,
                            EndNextInvoiceDate = invoice.EndNextInvoiceDate,
                            StatusID = invoice.StatusID,
                            PaymentDueDate = invoice.PaymentDueDate,
                            IsIncludedVAT = invoice.IsIncludedVAT,
                            invTmp = invoice.invTmp,
                            DowSpeed = invoice.DowSpeed,
                            UpSpeed = invoice.UpSpeed,
                            Bandwidth = invoice.Bandwidth,
                            Feature = invoice.Feature,
                            IsNextInvoice = invoice.IsNextInvoice,
                            ConnectivityStatus = invoice.ConnectivityStatus,
                            ConnectivityStatusName = invoice.ConnectivityStatusName,
                            InvoiceType = invoice.InvoiceType,
                            ExpireDate = invoice.ExpireDate,
                            IsRenew = invoice.IsRenew,
                            IsGenerated = invoice.IsGenerated,
                            GeneratedBy = html.UserFullName,
                            GeneratedDate = DateTime.Now,
                            OldBillingInvoiceID = invoice.OldBillingInvoiceID,
                            PPPoEUserName = invoice.PPPoEUserName,
                            ActivateCustomerID = invoice.ActivateCustomerID,
                            QuotationNo = invoice.QuotationNo,
                            InvoiceTmpID = invoice.InvoiceTmpID,
                            OldInvoiceNo = invoice.OldInvoiceNo,
                            IsChangePlan = invoice.IsChangePlan,
                            FromActivateCustomerID = invoice.FromActivateCustomerID,
                            IsApplyChangePlan = invoice.IsApplyChangePlan,
                            ProformaInvoiceNo = invoice.ProformaInvoiceNo,
                            ReceiptNo = invoice.ReceiptNo,
                            Type = invoice.Type,
                            ExchangeRate = invoice.ExchangeRate,
                            ChartOfAccountID = invoice.ChartOfAccountID,
                            CreateBy = invoice.CreateBy,
                            RenewBy = invoice.RenewBy,
                            CancelBy = invoice.CancelBy,
                            DeleteBy = invoice.DeleteBy,
                            CreateDate = invoice.CreateDate,
                            RenewDate = invoice.RenewDate,
                            CancelDate = invoice.CancelDate,
                            DeleteDate = invoice.DeleteDate,
                            CancelComment = invoice.CancelComment,
                            PaymentID = invoice.PaymentID,
                            ReferenceNo = invoice.ReferenceNo,
                            Warehouse = invoice.Warehouse,
                            LocationID = invoice.LocationID,
                            ClassName = invoice.ClassName,
                            ClassNameID = invoice.ClassNameID,
                            IsShowPeriod = invoice.IsShowPeriod,
                            InvoiceStatusID = invoice.InvoiceStatusID,
                            InvoiceStatus = invoice.InvoiceStatus,
                            ApproveBy = invoice.ApproveBy,
                            ApproveByUserID = invoice.ApproveByUserID,
                            ApproveDate = invoice.ApproveDate,
                            IsApplyDelayPayment = invoice.IsApplyDelayPayment,
                            IsToDMCTAX = invoice.IsToDMCTAX,
                            AgencyCommissionID = invoice.AgencyCommissionID,
                            AgencyCommissionTypeID = invoice.AgencyCommissionTypeID,
                            SystemID = invoice.SystemID,
                            WarehouseID = invoice.WarehouseID,
                            Remark = invoice.Remark,
                            RegisterByUserLoginID = invoice.RegisterByUserLoginID,
                            DomainID = invoice.DomainID,
                            InvoiceTelegramMessageID = invoice.InvoiceTelegramMessageID,
                            // ---- History-specific fields ----
                            InvoiceHistoryStatus = invoiceHistory != null && isEdit ? "Revise Invoice" : "Create Invoice",
                            ModifyDate = DateTime.Now,
                            ModifyBy = html.UserFullName,
                            ReviseCount = 0
                        }).ToList();

                    ctx.TblCustomerInvoiceHistory.AddRange(invoiceHistories);
                    await ctx.SaveChangesAsync();

                    // ===================== Accounting Transaction =====================
                    if (!isNewRegister && formType != "Quotation")
                    {
                        await ctx.Database.ExecuteSqlCommandAsync(
                            @"
                        delete from dbo.TblAccount_Transaction where CustomerID = @CustomerID and InvoiceNo = @InvoiceNumber;
                        delete from dbo.TblAccount_JournalEntries where CustomerID = @CustomerID and InvoiceNo = @InvoiceNumber;
                        delete from dbo.TblDeferredRevenueSchedule where CustomerID = @CustomerID and InvoiceNo = @InvoiceNumber;
                    ",
                            new SqlParameter("@CustomerID", customerId),
                            new SqlParameter("@InvoiceNumber", invoiceNo)
                        );
                        await ctx.Database.ExecuteSqlCommandAsync(
                            "EXEC [SP_InsertFullInvoiceAccounting] @CustomerID, @InvoiceNumber, @UserLoginID, @SystemID, @WarehouseID, @DomainID",
                            new SqlParameter("@CustomerID", customerId),
                            new SqlParameter("@InvoiceNumber", invoiceNo),
                            new SqlParameter("@UserLoginID", html.UserID),
                            new SqlParameter("@SystemID", html.SystemID),
                            new SqlParameter("@WarehouseID", html.WarehouseID),
                            new SqlParameter("@DomainID", "001")
                        );
                    }

                    // ===================== BALANCE & PAYMENT =====================
                    if (lastInvoice != null)
                    {
                        await HandleBalanceAndPayment(ctx, lastInvoice, customer, obj);
                    }

                    // ===================== PREFIX =====================
                    // FIX: guard each prefix separately so an empty one never calls the SP
                    if (!isEdit)
                    {
                        string prefixInvoice = obj["PrefixInvoice"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(prefixInvoice))
                        {
                            await ctx.Database.ExecuteSqlCommandAsync(
                                "EXEC [dbo].[SP_INSERT_PREFIX] @Prefix, @Module, @SystemID, @WarehouseID, '', @StartNumber, 0, @DomainID",
                                new SqlParameter("@Prefix", prefixInvoice),
                                new SqlParameter("@Module", "Customer Billing Prefix"),
                                new SqlParameter("@SystemID", html.SystemID),
                                new SqlParameter("@WarehouseID", html.WarehouseID),
                                new SqlParameter("@StartNumber", 1),
                                new SqlParameter("@DomainID", "001")
                            );
                        }

                        string customerCodePrefix = obj["CustomerCode_Prefix"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(customerCodePrefix))
                        {
                            await ctx.Database.ExecuteSqlCommandAsync(
                                "EXEC [dbo].[SP_INSERT_PREFIX] @Prefix, @Module, @SystemID, @WarehouseID, '', @StartNumber, 0, @DomainID",
                                new SqlParameter("@Prefix", customerCodePrefix),
                                new SqlParameter("@Module", "Customer Quote"),
                                new SqlParameter("@SystemID", html.SystemID),
                                new SqlParameter("@WarehouseID", html.WarehouseID),
                                new SqlParameter("@StartNumber", 1),
                                new SqlParameter("@DomainID", "001")
                            );
                        }
                    }
                    #endregion

                    #region Attachment File
                    if (!isNewRegister)
                    {
                        string strCustomer = html.ToUrlSlug($"{customer.CustomerName} [{customer.CustomerCode}]");
                        string destinationFolder = $"/Uploads/CustomerAttFiles/{strCustomer}";
                        var result = html.UploadFile(destinationFolder, "CustomerAttFiles");

                        if (result.Result == ResultType.Successful)
                        {
                            string filePath = result.Value;
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                var customerAtt = new AttachmentFile
                                {
                                    CustomerID = customer.CustomerID,
                                    InvoiceNo = lastInvoice?.InvoiceNo,
                                    DestinationFolder = destinationFolder,
                                    AttachmentFileName = filePath,
                                    AddDate = lastInvoice?.CreateDate,
                                    UploadBy = lastInvoice?.CreateBy,
                                    TypeFor = "Attachment File",
                                    SystemID = SystemID,
                                    WarehouseID = WarehouseID,
                                    DomainID = domainId
                                };

                                ctx.TblCustomerAttachmentFile.Add(customerAtt);
                            }
                        }
                    }
                    #endregion

                    #region Renew Connection (MARK OLD)
                    if (isRenew || isChangePlan)
                    {
                        // Mark old invoice as renewed
                        if (oldInvoice != null)
                        {
                            oldInvoice.IsRenew = true;
                        }

                        // Mark old billing invoice as renewed
                        if (oldBillingInvoice != null)
                        {
                            oldBillingInvoice.IsRenew = true;
                            oldBillingInvoice.RenewStatus = "Done";
                            oldBillingInvoice.RenewDate = DateTime.Now;
                        }
                    }
                    #endregion

                    // Final save + COMMIT
                    await ctx.SaveChangesAsync();
                    tran.Commit();

                    // FIX: return real data for NewRegister (no invoice exists yet)
                    // and don't serialize the EF entity (circular reference risk)
                    return Json(new
                    {
                        success = true,
                        message = isEdit ? "Updated successfully" : "Created successfully",
                        data = new
                        {
                            CustomerID = customer.CustomerID,
                            CustomerCode = customer.CustomerCode,
                            CustomerName = customer.CustomerName,
                            CustomerMobile = customer.CustomerMobile,
                            CustomerAddress = customer.CustomerAddress,
                            InvoiceNo = lastInvoice?.InvoiceNo ?? (isNewRegister ? null : invoiceNo),
                            CustomerBillInformationID = billInfo.CustomerBillInformationID,
                            BillingInvoiceID = lastInvoice?.BillingInvoiceID,
                            InvoiceID = lastInvoice?.CustomerInvoiceID,
                            InvoiceDate = lastInvoice?.InvoiceDate ?? billInfo.InvoiceDate,
                            Type = lastInvoice?.Type ?? billInfo.Type,
                            PaymentDueDate = lastInvoice?.PaymentDueDate ?? billInfo.PaymentDueDate,
                            TotalAmount = lastInvoice?.Amount ?? 0,
                            ExchangeRate = billInfo.ExchangeRate,
                            IsEdit = isEdit,
                            IsAutoFill = isAutoFill,
                            IsRenew = isRenew,
                            IsChangePlan = isChangePlan,
                            CreateBy = lastInvoice?.CreateBy ?? html.UserFullName,
                            FormType = formType,
                            IsIncludedVAT = lastInvoice?.IsIncludedVAT ?? billInfo.IsIncludedVAT,
                            NeedCheckByUsername = obj["RequestCheckByTgUser"]?.ToString()
                        }
                    });
                }
                catch (Exception exc)
                {
                    try { tran.Rollback(); } catch { }

                    // Show the real cause (EF hides it in InnerException)
                    var root = exc;
                    while (root.InnerException != null) root = root.InnerException;

                    return Json(new { success = false, message = root.Message, stackTrace = exc.StackTrace });
                }
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> GenerateCustomerTelegramContact(string data)
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    JObject json = JObject.Parse(data);
                    int customerId = json["CustomerID"]?.Value<int>() ?? 0;
                    string customerMobile = json["CustomerMobile"]?.ToString();
                    string customerName = json["CustomerName"]?.ToString();
                    string customerCode = json["CustomerCode"]?.ToString();

                    var doActionData = new DoActionDataController();
                    var result = await doActionData.AddUserToTelegram(customerId.ToString(), customerMobile, customerName, customerCode);

                    return Json(new
                    {
                        success = true,
                        message = result.Message ?? "Unknown response",
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task HandleBalanceAndPayment(DBContext ctx, CustomerInvoice plan, Customer customer, JObject obj)
        {
            HTML html = new HTML();
            JObject totals = (obj["InvoiceTotalAmount"] as JArray)?.FirstOrDefault() as JObject;
            decimal total = totals["Total"]?.Value<decimal>() ?? 0;
            decimal subtotal = totals["Subtotal"]?.Value<decimal>() ?? 0;
            decimal vat = totals["VAT"]?.Value<decimal>() ?? 0;
            decimal totalDisP = totals["TotalDiscountP"]?.Value<decimal>() ?? 0;
            decimal totalDisUSD = totals["TotalDiscountUSD"]?.Value<decimal>() ?? 0;
            string typeOfInvoice = obj["SetCID"]?.ToString();
            decimal grandTotal = totals?["GrandTotal"]?.Value<decimal>() ?? 0;
            string formType = obj["FormType"]?.ToString();
            int systemID = Utility.TryParse(html.SystemID, "int", 0);
            int warehouseID = Utility.TryParse(html.WarehouseID, "int", 0);
            string domainID = (typeOfInvoice == "VAT" || typeOfInvoice == "CM") ? "001,002" : "001";
            bool isCreditNote = plan.Type == "Credit Notes";

            // -------- BALANCE --------
            var customerBalance = await ctx.TblCustomerBalance
                .FirstOrDefaultAsync(x =>
                    x.CustomerID == customer.CustomerID &&
                    x.InvoiceNo == plan.InvoiceNo);

            if (customerBalance == null)
            {
                customerBalance = new CustomerBalance
                {
                    CustomerID = customer.CustomerID,
                    BillingInvoiceID = plan.BillingInvoiceID,
                    CreateBy = html.UserFullName,
                    CreateDate = DateTime.Now
                };

                ctx.TblCustomerBalance.Add(customerBalance);
            }

            customerBalance.InvoiceNo = plan.InvoiceNo;
            customerBalance.GrandTotal = grandTotal;
            customerBalance.Unpaid = grandTotal;
            customerBalance.Paid = 0;
            customerBalance.StatusID = formType == "Quotation" ? 20 : 11;
            customerBalance.InvoiceStatusID = 101;
            customerBalance.SystemID = systemID;
            customerBalance.WarehouseID = warehouseID;
            customerBalance.DomainID = domainID;
            customerBalance.CustomerName = customer.CustomerName;
            customerBalance.CompanyName = obj["CompanyName"]?.ToString();
            customerBalance.ConnectionID = obj["ConnectionID"]?.ToString();
            customerBalance.TypeOfInvoice = plan.TypeOfInvoice;
            customerBalance.SubTotal = subtotal;
            customerBalance.VAT = vat;
            customerBalance.Total = total;
            customerBalance.DiscountP = totalDisP;
            customerBalance.DiscountUSD = totalDisUSD;
            customerBalance.Balance = 0;
            customerBalance.StartDate = plan.StartDate;
            customerBalance.EndDate = plan.EndDate;
            customerBalance.InvoiceDate = plan.InvoiceDate;
            customerBalance.PaymentDueDate = plan.PaymentDueDate;
            customerBalance.IsIncludedVAT = plan.IsIncludedVAT;
            customerBalance.EmployeeID = customer.EmployeeID;
            customerBalance.ExchangeRate = plan.ExchangeRate;
            customerBalance.ChartOfAccountID = plan.ChartOfAccountID;
            customerBalance.Type = plan.Type;
            customerBalance.InvoiceType = plan.InvoiceType;
            customerBalance.ClassName = plan.ClassName;
            customerBalance.ClassNameID = plan.ClassNameID;
            customerBalance.IsServicesOrProduct = "customer services";
            await ctx.SaveChangesAsync();
            // -------- PAYMENT --------
            var customerPayment = await ctx.TblCustomerPayment
                .FirstOrDefaultAsync(x =>
                    x.CustomerID == customer.CustomerID &&
                    x.InvoiceNo == plan.InvoiceNo);

            if (customerPayment == null)
            {
                customerPayment = new CustomerPayment
                {
                    CustomerID = customer.CustomerID,
                    BillingInvoiceID = plan.BillingInvoiceID,
                    InvoiceNo = plan.InvoiceNo,
                    CreateDate = DateTime.Now
                };

                ctx.TblCustomerPayment.Add(customerPayment);
            }

            customerPayment.BillingInvoiceID = plan.BillingInvoiceID;
            customerPayment.InvoiceNo = plan.InvoiceNo;
            customerPayment.Deposit = 0;
            customerPayment.Total = grandTotal;
            customerPayment.Paid = 0;
            customerPayment.Unpaid = grandTotal;
            customerPayment.StatusID = 11;
            customerPayment.Type = plan.InvoiceType;
            customerPayment.InvoiceDate = plan.InvoiceDate;
            customerPayment.PaymentDueDate = plan.PaymentDueDate;
            customerPayment.CreateDate = DateTime.Now;
            customerPayment.SystemID = systemID;
            customerPayment.WarehouseID = warehouseID;
            customerPayment.DomainID = domainID;

            // -------- CUSTOMER ACCOUNT --------
            // Delete existing record then re-insert based on latest balance data
            if (formType != "Quotation")
            {
                var existingAccounts = await ctx.TblCustomerAccount
                .Where(x =>
                    x.CustomerID == customer.CustomerID &&
                    x.InvoiceNo == plan.InvoiceNo)
                .ToListAsync();

                if (existingAccounts.Any())
                    ctx.TblCustomerAccount.RemoveRange(existingAccounts);

                // Fetch the balance record to map into CustomerAccount
                // (using the same condition logic from the SP)
                var balanceForAccount = await ctx.TblCustomerBalance
                    .FirstOrDefaultAsync(x =>
                        x.CustomerID == customer.CustomerID &&
                        x.InvoiceNo == plan.InvoiceNo &&
                        x.SystemID == systemID &&
                        x.WarehouseID == warehouseID &&
                        x.Type == plan.Type);

                if (balanceForAccount != null)
                {
                    var newCustomerAccount = new CustomerAccount
                    {
                        ChartOfAccountID = 0,
                        CustomerID = balanceForAccount.CustomerID,
                        InvoiceNo = isCreditNote ? "" : balanceForAccount.InvoiceNo,
                        CreditNoteNo = isCreditNote ? plan.InvoiceNo : "",
                        InvoiceDate = balanceForAccount.InvoiceDate,
                        InvoiceDueDate = balanceForAccount.PaymentDueDate,
                        InvoiceBy = balanceForAccount.CreateBy,
                        DepositFee = balanceForAccount.DepositFee,
                        InvoiceAmount = balanceForAccount.GrandTotal,
                        PaidAmount = balanceForAccount.Paid,
                        UnpaidAmount = balanceForAccount.Unpaid,
                        Type = plan.Type,
                        SystemID = balanceForAccount.SystemID,
                        WarehouseID = balanceForAccount.WarehouseID,
                        IsIncludedVAT = balanceForAccount.IsIncludedVAT,
                        StatusID = balanceForAccount.StatusID,
                        DomainID = domainID
                    };

                    ctx.TblCustomerAccount.Add(newCustomerAccount);
                }

                // -------- CUSTOMER DEPOSIT --------
                // Delete existing deposit records for this invoice
                var existingDeposits = await ctx.TblCustomerDeposit
                    .Where(x =>
                        x.CustomerID == customer.CustomerID &&
                        x.InvoiceNo == plan.InvoiceNo &&
                        x.SystemID == systemID &&
                        x.WarehouseID == warehouseID)
                    .ToListAsync();

                if (existingDeposits.Any())
                    ctx.TblCustomerDeposit.RemoveRange(existingDeposits);

                // Re-insert deposit records from invoice lines where ItemDescription = 'Deposit Fee'
                var depositInvoiceLines = await ctx.TblCustomerInvoice
                    .Where(x =>
                        x.CustomerID == customer.CustomerID &&
                        x.InvoiceNo == plan.InvoiceNo &&
                        x.SystemID == systemID &&
                        x.WarehouseID == warehouseID &&
                        x.ItemDescription == "Deposit Fee")
                    .ToListAsync();

                foreach (var line in depositInvoiceLines)
                {
                    var newDeposit = new CustomerDeposit
                    {
                        CustomerID = line.CustomerID,
                        ConnectionID = line.ConnectionID,
                        DepositFee = line.Amount,
                        InvoiceNo = line.InvoiceNo,
                        Memo = $" Deposit Fee: {line.Amount} For Invoice No.: {line.InvoiceNo}",
                        CreateBy = line.CreateBy,
                        CreateDate = line.CreateDate,
                        SystemID = line.SystemID,
                        WarehouseID = line.WarehouseID,
                        IsIncludedVAT = line.IsIncludedVAT,
                        DomainID = domainID
                    };

                    ctx.TblCustomerDeposit.Add(newDeposit);
                }
            }

            await ctx.SaveChangesAsync();
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> HandleTicketMessage(string data)
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    NotificationTelegram notification = new NotificationTelegram();
                    HTML html = new HTML();
                    var pdfResult = new SqlResult();
                    var notifyResult = new SqlResult();

                    JObject json = JObject.Parse(data);
                    int customerId = json["CustomerID"]?.Value<int>() ?? 0;
                    string invoiceNo = json["InvoiceNo"]?.ToString();
                    string connectionID = json["ConnectionID"]?.ToString();

                    var customer = ctx.TblCustomer.Find(customerId);

                    var billInfo = await ctx.TblCustomerBillInformation
                        .Where(x => x.CustomerID == customerId && x.InvoiceNo == invoiceNo)
                        .Select(x => new
                        {
                            x.GoogleMap_FullAddress,
                            x.GoogleMap_Latitude,
                            x.GoogleMap_Longitude,
                            x.InstallAddress,
                            x.InstallCommune,
                            x.InstallDistrict,
                            x.InstallProvinceCity
                        })
                        .FirstOrDefaultAsync();

                    // ── Existing fields ──────────────────────────────────────────────
                    string customerName = json["CustomerName"]?.ToString();
                    string customerCode = json["CustomerCode"]?.ToString();
                    string address = json["CustomerAddress"]?.ToString();
                    string invoiceDate = json["InvoiceDate"]?.ToString();
                    string phone = customer?.CustomerMobile;
                    decimal totalAmount = json["TotalAmount"]?.Value<decimal>() ?? 0;
                    string createdBy = json["CreateBy"]?.ToString();
                    string registerBy = json["RegisterBy"]?.ToString();
                    string registerDate = json["RegisterDate"]?.ToString();
                    string formType = json["FormType"]?.ToString();
                    int isIncludedVAT = Utility.TryParse(json["IsIncludedVAT"]?.ToString(), "int", 0);

                    // ── New fields matching SP parameters ────────────────────────────
                    string domainID = html.DomainID;
                    int systemID = json["SystemID"]?.Value<int>() ?? int.Parse(html.SystemID.ToString());
                    int warehouseID = json["WarehouseID"]?.Value<int>() ?? int.Parse(html.WarehouseID.ToString());
                    string typeOfInvoice = json["TypeOfInvoice"]?.ToString();   // "VAT" | "CM" | "None VAT"
                    string invoiceType = json["InvoiceType"]?.ToString();     // "Created Invoice" | "Renew Invoice"
                    string sid = json["SID"]?.ToString();
                    string userName = html.UserFullName;
                    string userFullName = json["UserFullName"]?.ToString();
                    string userProfilePicture = json["UserProfilePicture"]?.ToString();
                    int userLoginID = json["RegisterByUserLoginID"]?.Value<int>() ?? 0;
                    int supportProjectID = 1;
                    string projectName = "Customer Services";
                    string assigneeID = "110";
                    string assigneeName = "Technical Support";
                    int serviceID = json["ServiceID"]?.Value<int>() ?? 0;
                    string serviceName = json["ServiceName"]?.ToString();
                    int packageDetailID = json["PackageDetailID"]?.Value<int>() ?? 0;
                    string packageName = json["ItemDescription"]?.ToString();
                    DateTime? startDate = json["StartDate"]?.Value<DateTime?>();
                    DateTime? endDate = json["EndDate"]?.Value<DateTime?>();
                    string installAddress = billInfo?.InstallAddress;
                    string installCommune = billInfo?.InstallCommune;
                    string installDistrict = billInfo?.InstallDistrict;
                    string installProvince = billInfo?.InstallProvinceCity;
                    string descriptionTicket = json["DescriptionTicket"]?.ToString();
                    string oldInvoiceNo = json["OldInvoiceNo"]?.ToString();
                    int countConnection = json["CountConnection"]?.Value<int>() ?? 1;

                    // ── Determine prefix / module (mirrors SP logic) ─────────────────
                    string prefix = json["Prefix"]?.ToString();
                    string module = json["Module"]?.ToString();

                    if (string.IsNullOrWhiteSpace(prefix)) prefix = "TK#{000000}";
                    if (string.IsNullOrWhiteSpace(module)) module = "Support Ticket";

                    if (typeOfInvoice == "VAT" || typeOfInvoice == "CM")
                    {
                        prefix = "TK#{000000}";
                        module = "VAT Support Ticket";
                    }

                    // ── Resolve user info when missing (mirrors SP lookup) ───────────
                    if (string.IsNullOrWhiteSpace(userFullName))
                    {
                        var userInfo = await ctx.TblUserLogin
                            .Where(u => u.FullName.ToLower() == userName.ToLower())
                            .Select(u => new { u.UserLoginID, u.FullName })
                            .FirstOrDefaultAsync();

                        if (userInfo != null)
                        {
                            userLoginID = userInfo.UserLoginID;
                            userFullName = userInfo.FullName;
                        }
                    }

                    // ── Ticket existence check ───────────────────────────────────────
                    var ticket = await ctx.TblSupportOpenTicket
                        .Where(t =>
                            t.InvoiceNo == invoiceNo &&
                            t.ConnectionID == connectionID &&
                            t.SID == sid
                        )
                        .FirstOrDefaultAsync();

                    string ticketNumber = null;
                    int openTicketID = 0;
                    var telegramDesc = new StringBuilder();

                    if (ticket == null)
                    {
                        // ── Generate ticket number via SP_INSERT_PREFIX ──────────────
                        var prefixParam = new SqlParameter("@Prefix", prefix);
                        var moduleParam = new SqlParameter("@Module", module);
                        var sysParam = new SqlParameter("@SystemID", systemID);
                        var whParam = new SqlParameter("@WarehouseID", warehouseID);
                        var domainParam = new SqlParameter("@DomainID", domainID);
                        var ticketOutParam = new SqlParameter("@TicketNumber", SqlDbType.NVarChar, 50)
                        { Direction = ParameterDirection.Output };

                        await ctx.Database.ExecuteSqlCommandAsync(
                            "EXEC SP_INSERT_PREFIX @Prefix, @Module, @SystemID, @WarehouseID, @TicketNumber OUT, 1, 0, @DomainID",
                            prefixParam, moduleParam, sysParam, whParam, ticketOutParam, domainParam);

                        ticketNumber = ticketOutParam.Value?.ToString();

                        if (invoiceType == "Created Invoice")
                        {
                            // ── Insert new ticket ────────────────────────────────────
                            var newTicket = new SupportOpenTicket
                            {
                                TicketNumber = ticketNumber,
                                SupportProjectID = supportProjectID,
                                ProjectName = projectName,
                                PostAudienceID = 20,
                                PostAudience = "Public",
                                Subject = "New Customer",
                                CID = customerCode,
                                StatusID = 1,
                                Status = "Open",
                                PriorityID = 1,
                                Priority = "Normal",
                                SupportDoneID = 1,
                                SupportDone = "0 %",
                                AssigneeID = assigneeID,
                                AssigneeName = assigneeName,
                                SID = string.IsNullOrWhiteSpace(sid) ? "Pending" : sid,
                                ConnectionID = connectionID,
                                CustomerID = customerId,
                                CustomerName = $"{customerCode} - {customerName}",
                                Phone = phone,
                                ServiceID = serviceID,
                                ServiceName = serviceName,
                                PackageDetailID = packageDetailID,
                                PackageName = packageName,
                                StartDate = startDate,
                                EndDate = endDate,
                                Address = installAddress,
                                Commune = installCommune,
                                District = installDistrict,
                                ProvinceCity = installProvince,
                                Description = descriptionTicket,
                                CreateByFullName = userFullName,
                                ProblemType = "New Installation",
                                UserLoginID = userLoginID,
                                CreateBy = userName,
                                CreateDate = DateTime.Now,
                                SystemID = systemID,
                                WarehouseID = warehouseID,
                                CoWorkerName = "",
                                CoWorkerID = "0",
                                InvoiceNo = invoiceNo,
                                DomainID = domainID
                            };

                            ctx.TblSupportOpenTicket.Add(newTicket);
                            await ctx.SaveChangesAsync();
                            openTicketID = newTicket.OpenTicketID;

                            // ── Build Telegram description (mirrors SP string builder) 
                            if (countConnection == 1)
                            {
                                string mapLink = $"https://www.google.com/maps/place/{billInfo?.GoogleMap_Latitude}," +
                                                 $"{billInfo?.GoogleMap_Longitude}/@{billInfo?.GoogleMap_Latitude}," +
                                                 $"{billInfo?.GoogleMap_Longitude},15z";

                                string linkAddress = $"<a target=\"_blank\" href=\"{mapLink}\">" +
                                                     $"{billInfo?.GoogleMap_FullAddress}</a>";

                                telegramDesc.Append($"\n\nCID: <b>{customerCode}</b>\n");
                                telegramDesc.Append($"Customer Name: <b>{customerName}</b>\n");
                                telegramDesc.Append($"Phone: <b>{phone}</b>\n");
                                telegramDesc.Append($"Address: <b>{installAddress}");
                                telegramDesc.Append($", {installCommune}");
                                telegramDesc.Append($", {installDistrict}");
                                telegramDesc.Append($", {installProvince}.</b>\n");
                                telegramDesc.Append($"Google Map: {linkAddress}\n");
                                telegramDesc.Append($"Latitude: {billInfo?.GoogleMap_Latitude}\n");
                                telegramDesc.Append($"Longitude: {billInfo?.GoogleMap_Longitude}\n");
                            }

                            // Per-connection info (always appended)
                            telegramDesc.Append($"\n{countConnection}. Connection ID: <b>{connectionID}</b>\n");

                            // invTmp == 1 means it's a service-based invoice
                            if (serviceID != 0)
                            {
                                telegramDesc.Append($"Service Name: <b>{serviceName}</b>\n");
                                telegramDesc.Append($"Package Name: <b>{packageName}</b>\n");
                            }
                            else
                            {
                                telegramDesc.Append($"Service Name: <b>{packageName}</b>");
                            }
                        }
                        else // Renew Invoice — fetch existing ticket
                        {
                            var existing = await ctx.TblSupportOpenTicket
                                .Where(t => t.ConnectionID == connectionID &&
                                            t.SystemID == systemID &&
                                            t.WarehouseID == warehouseID &&
                                            t.DomainID.Contains(domainID))
                                .Select(t => new { t.OpenTicketID, t.TicketNumber })
                                .FirstOrDefaultAsync();

                            openTicketID = existing?.OpenTicketID ?? 0;
                            ticketNumber = existing?.TicketNumber;
                        }
                    }
                    else // Ticket already exists — UPDATE
                    {
                        var ticketToUpdate = await ctx.TblSupportOpenTicket
                            .FirstOrDefaultAsync(t =>
                                t.ConnectionID == connectionID &&
                                t.InvoiceNo == invoiceNo &&
                                t.SystemID == systemID &&
                                t.WarehouseID == warehouseID &&
                                t.DomainID.Contains(domainID));

                        if (ticketToUpdate != null)
                        {
                            ticketToUpdate.SupportProjectID = supportProjectID;
                            ticketToUpdate.ProjectName = projectName;
                            ticketToUpdate.Subject = "New Customer";
                            ticketToUpdate.CID = customerCode;
                            ticketToUpdate.SID = string.IsNullOrWhiteSpace(sid) ? "Pending" : sid;
                            ticketToUpdate.ConnectionID = connectionID;
                            ticketToUpdate.CustomerID = customerId;
                            ticketToUpdate.CustomerName = customerName;
                            ticketToUpdate.Phone = phone;
                            ticketToUpdate.ServiceID = serviceID;
                            ticketToUpdate.ServiceName = serviceName;
                            ticketToUpdate.PackageDetailID = packageDetailID;
                            ticketToUpdate.PackageName = packageName;
                            ticketToUpdate.StartDate = startDate;
                            ticketToUpdate.EndDate = endDate;
                            ticketToUpdate.Address = installAddress;
                            ticketToUpdate.Commune = installCommune;
                            ticketToUpdate.District = installDistrict;
                            ticketToUpdate.ProvinceCity = installProvince;
                            ticketToUpdate.DomainID = domainID;

                            await ctx.SaveChangesAsync();
                        }

                        var existing = await ctx.TblSupportOpenTicket
                            .Where(t => t.ConnectionID == connectionID &&
                                        t.SystemID == systemID &&
                                        t.WarehouseID == warehouseID &&
                                        t.DomainID.Contains(domainID))
                            .Select(t => new { t.OpenTicketID, t.TicketNumber })
                            .FirstOrDefaultAsync();

                        openTicketID = existing?.OpenTicketID ?? 0;
                        ticketNumber = existing?.TicketNumber;
                    }

                    var billInvoice = await ctx.TblCustomerBillingInvoice
                        .Where(b => b.CustomerID == customerId && b.InvoiceNo == invoiceNo)
                        .FirstOrDefaultAsync();

                    if (billInvoice != null)
                    {
                        billInvoice.TicketNumber = ticketNumber;
                        await ctx.SaveChangesAsync();
                    }

                    notifyResult = await notification.NotificationTelegramBotToTopic(telegramDesc.ToString());

                    if (notifyResult.Result == ResultType.Successful)
                    {
                        if (System.IO.File.Exists(pdfResult?.OutPutFile))
                            System.IO.File.Delete(pdfResult.OutPutFile);
                    }

                    return Json(new
                    {
                        success = notifyResult.Result == ResultType.Successful,
                        message = notifyResult.Message ?? "Unknown response",
                        ticketNumber = ticketNumber,
                        openTicketID = openTicketID,
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> SentNotification(string data)
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    NotificationTelegram notification = new NotificationTelegram();
                    HTML html = new HTML();
                    var pdfResult = new SqlResult();
                    var notifyResult = new SqlResult();

                    // Convert to JObject
                    JObject json = JObject.Parse(data);

                    int customerId = json["CustomerID"]?.Value<int>() ?? 0;
                    string customerName = json["CustomerName"]?.ToString();
                    string customerCode = json["CustomerCode"]?.ToString();
                    string address = json["CustomerAddress"]?.ToString();
                    string invoiceNo = json["InvoiceNo"]?.ToString();
                    string phone = json["CustomerMobile"]?.ToString();
                    decimal totalAmount = json["TotalAmount"]?.Value<decimal>() ?? 0;
                    string createdBy = json["CreateBy"]?.ToString();
                    string formType = json["FormType"]?.ToString();
                    int IsIncludedVAT = Utility.TryParse(json["IsIncludedVAT"]?.ToString(), "int", 0);
                    string needCheckBy = json["NeedByCheck"]?.ToString();
                    bool isNeedCheck = Utility.TryParse(json["NeedByCheckID"]?.ToString(), "int", 0) > 0;
                    bool isNeedApprove = Utility.TryParse(json["NeedByApproveID"]?.ToString(), "int", 0) > 0;
                    bool IsRenew = Utility.TryParse(json["IsRenew"]?.ToString(), "bool", false);
                    bool IsChangePlan = Utility.TryParse(json["IsChangePlan"]?.ToString(), "bool", false);
                    bool IsEdit = Utility.TryParse(json["IsEdit"]?.ToString(), "bool", false);
                    int StatusID = 11;

                    //Get customer info
                    var customer = await ctx.TblCustomer
                        .Where(x => x.CustomerID == customerId)
                        .Join(
                            ctx.TblCustomerBlock,
                            c => c.CustomerTypeID,
                            b => b.CustomerTypeID,
                            (c, b) => new
                            {
                                c.RegisterDate,
                                c.CustomerCode,
                                c.RegisterBy,
                                c.CustomerGender,
                                CustomerName = string.IsNullOrEmpty(c.CustomerName) ?
                                    c.CustomerNameLatin : c.CustomerName,
                                b.CustomerType
                            })
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

                    customerName = customer?.CustomerName;

                    var customerInvoice = await ctx.TblCustomerInvoice
                        .Where(x => x.CustomerID == customerId && x.InvoiceNo == invoiceNo)
                        .Select(x => new
                        {
                            PackageName = x.ItemDescription,
                            ServiceSort = (x.ServiceID != null && x.ServiceID != 0) ? 1 : 0,
                            x.ServiceID,
                            x.ATTName,
                            x.InvoiceDate,
                            x.StartDate,
                            x.EndDate,
                            x.InstallDate,
                            x.ClassName,
                            x.TypeOfInvoice,
                            x.InvoiceType,
                            x.OldInvoiceNo,
                            x.InvoiceStatusID,
                            x.RequestApprovalToDMCTAX,
                            x.RequestApprovalToDMCTAXTgUser,
                            x.RequestedByUserToDMCTAX,
                            x.SID,
                            x.SystemID,
                            x.InvoiceNote
                        })
                        .OrderByDescending(x => x.ServiceSort)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

                    string remark = "";
                    string invNote = customerInvoice?.InvoiceNote ?? "";

                    if (!string.IsNullOrWhiteSpace(customerInvoice?.PackageName))
                    {
                        const string marker = ";Remark:";

                        int idx = customerInvoice.PackageName.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

                        if (idx >= 0)
                        {
                            remark = "<b>Remark:</b> " +
                                     customerInvoice.PackageName
                                         .Substring(idx + marker.Length)
                                         .Trim();
                        }
                    }

                    var billInfo = await ctx.TblCustomerBillInformation
                        .Where(x => x.CustomerID == customerId && x.InvoiceNo == invoiceNo)
                        .Join(
                            ctx.TblIspLink,
                            bill => bill.IspLinkID,
                            isp => isp.IspLinkID,
                            (bill, isp) => new
                            {
                                bill.GoogleMap_FullAddress,
                                bill.GoogleMap_Latitude,
                                bill.GoogleMap_Longitude,
                                isp.IspLinkName,
                            })
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

                    var customerAtt = await ctx.TblCustomerAttachmentFile
                        .Where(x => x.CustomerID == customerId && x.InvoiceNo == invoiceNo)
                        .Select(x => new
                        {
                            x.DestinationFolder,
                            x.AttachmentFileName,
                            x.TypeFor
                        })
                        .AsNoTracking()
                        .ToListAsync();

                    var infra = await ctx.TblCustomerInfra
                        .Where(x => x.CustomerID == customerId && x.InvoiceNo == invoiceNo)
                        .Join(ctx.TblInfraProvider,
                            c => c.InfraProviderID,
                            i => i.InfraProviderID,
                            (c, i) => new
                            {
                                c.CustomerInfraName,
                                c.CustomerInfraSurveyID,
                                c.CustomerInfraCID,
                                c.CustomerInfraSID,
                                c.CustomerInfraAID,
                                i.InfraProviderName
                            })
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

                    string sid = customerInvoice.SID;
                    var bankSlip = await ctx.TblCustomerBankSlip
                        .Where(
                            x => x.CustomerID == customerId
                            && x.USID == sid
                        )
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

                    // Init message
                    var message = new StringBuilder();
                    var mapLink = $"https://www.google.com/maps?q={billInfo?.GoogleMap_Latitude},{billInfo?.GoogleMap_Longitude}";
                    string captions = "";
                    string outPutFiles = "";
                    string ATTName = customerInvoice?.ATTName;
                    bool isService = customerInvoice?.ServiceID != 0 && customerInvoice?.ServiceID != null;
                    var keyboard = new InlineKeyboardMarkup();
                    string type = customerInvoice?.ClassName == "NSM" ? "NSM" : "ISP";

                    //handle pdf file
                    pdfResult = await html.InvoiceToPDFandUpload(
                                customerName, customerCode, customerId, invoiceNo,
                                customerInvoice.InvoiceDate.ToString(), IsIncludedVAT, StatusID, int.Parse(html.SystemID.ToString()),
                                int.Parse(html.WarehouseID.ToString()), false
                            );

                    if (formType == "Quotation" && isService && customerInvoice?.ServiceID == 1)
                    {
                        string attPath = "";
                        string typeForStr = "";

                        if (isService && customerAtt != null && customerAtt.Any())
                        {
                            List<string> pathList = new List<string>();
                            List<string> typeList = new List<string>();

                            foreach (var item in customerAtt)
                            {
                                if (!string.IsNullOrEmpty(item.AttachmentFileName))
                                {
                                    var fileNames = item.AttachmentFileName
                                        .Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

                                    foreach (var name in fileNames)
                                    {
                                        pathList.Add($"{item.DestinationFolder}/{name}");
                                        typeList.Add(item.TypeFor ?? "");
                                    }
                                }
                            }

                            attPath = string.Join("~", pathList);
                            typeForStr = string.Join("~", typeList);
                        }

                        outPutFiles = !string.IsNullOrEmpty(attPath) ? pdfResult?.OutPutFile + "~" + attPath : pdfResult?.OutPutFile;
                        bool isKhmer = customerName?.Any(c => c >= '\u1780' && c <= '\u17FF') ?? false;
                        string genderPrefix = "";

                        if (customer?.CustomerGender == "Male")
                            genderPrefix = isKhmer ? "លោក" : "Mr.";
                        else if (customer?.CustomerGender == "Female")
                            genderPrefix = isKhmer ? "អ្នកស្រី" : "Ms.";

                        message.AppendLine($"<b>📋 {type} QUOTATION</b>\n");
                        message.AppendLine($"<i><b>No.</b>: {invoiceNo}</i>");
                        message.AppendLine("<b>────────────────────</b>");

                        message.AppendLine("<b><i>👤 CUSTOMER INFO</i></b>");
                        message.AppendLine($"‣ <b>Name:</b> {genderPrefix} {customerName}");
                        message.AppendLine($"‣ <b>Phone:</b> {phone}");
                        message.AppendLine($"‣ <b>Register Date:</b> {customer?.RegisterDate}");
                        message.AppendLine($"‣ <b>Install Date:</b> {customerInvoice?.InstallDate}\n");

                        message.AppendLine("<b><i>⚙️ INFRASTRUCTURE</i></b>");
                        message.AppendLine($"‣ <b>Infra:</b> {infra?.InfraProviderName}");
                        message.AppendLine($"‣ <b>Name:</b> {infra?.CustomerInfraName}");
                        message.AppendLine($"‣ <b>Survey ID:</b> {infra?.CustomerInfraSurveyID}");
                        message.AppendLine($"‣ <b>CID:</b> {infra?.CustomerInfraCID}, <b>SID:</b> {infra?.CustomerInfraSID}, <b>AID:</b> {infra?.CustomerInfraAID}\n");

                        message.AppendLine("<b><i>🛜 PACKAGE INFO</i></b>");
                        message.AppendLine($"‣ <i><b>Plan:</b> {customerInvoice?.PackageName}</i>");
                        message.AppendLine($"‣ <i><b>Start Date:</b> {customerInvoice?.StartDate?.ToString("dd-MM-yyyy")}</i>");
                        message.AppendLine($"‣ <i><b>End Date:</b> {customerInvoice?.EndDate?.ToString("dd-MM-yyyy")}</i>\n");

                        message.AppendLine("<b><i>📍 LOCATION</i></b>");
                        message.AppendLine($"‣ <b>Map:</b> <a href='{mapLink}'>{billInfo?.GoogleMap_FullAddress}</a>");
                        message.AppendLine($"‣ <b>Lat / Long:</b> {billInfo?.GoogleMap_Latitude}, {billInfo?.GoogleMap_Longitude}");

                        captions = typeForStr + "~" + message.ToString();
                        notifyResult = await notification.NotificationTelegramBot("", -1003550433125, PDFFile: outPutFiles, Caption: captions, inlineKeyboard: keyboard ?? null);
                    }
                    else if (!string.IsNullOrEmpty(customerInvoice?.SID) || (formType == "Invoice" && !isService))
                    {
                        string requester = customerInvoice?.RequestedByUserToDMCTAX;
                        string approver = customerInvoice?.RequestApprovalToDMCTAX;
                        string needCheckByUsername = json["NeedCheckByUsername"]?.ToString();
                        string typeOfInvoice = customerInvoice?.TypeOfInvoice;
                        string invoiceType = customerInvoice?.InvoiceType;
                        string oldInvoice = customerInvoice?.OldInvoiceNo;
                        int? statusId = customerInvoice?.InvoiceStatusID;
                        string username = isNeedCheck && statusId == 101 ? needCheckByUsername
                            : customerInvoice?.RequestApprovalToDMCTAXTgUser;
                        string userFullName = isNeedCheck && statusId == 101 ? needCheckBy : approver;

                        if (!string.IsNullOrEmpty(approver) && (isNeedCheck || isNeedApprove))
                        {
                            if (isNeedApprove || isNeedCheck)
                            {
                                keyboard = new InlineKeyboardMarkup(new[]
                                {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData($"{(isNeedCheck ? "Check ✍️" : "Approve ✅")}", $"{(isNeedCheck ? "CheckInvoice" : "ApproveInvoice")}~{invoiceNo}~{customerId}"),
                                InlineKeyboardButton.WithCallbackData($"Disapprove ❌", $"DisapproveInvoice~{invoiceNo}~{customerId}")
                            }
                        });
                            }

                            message.Clear();

                            if (typeOfInvoice == "None VAT")
                            {
                                message.AppendLine($"{username}\n");
                                message.AppendLine($"Dear {userFullName},\n");
                                message.AppendLine($"Now, you have invoice need to check.\n");
                                message.AppendLine($"<i>ISP LINK: {billInfo?.IspLinkName}.</i>\n");
                                if (!string.IsNullOrEmpty(remark))
                                    message.AppendLine($"<i>{remark}.</i>\n");
                                if (!string.IsNullOrEmpty(invNote))
                                    message.AppendLine($"<i>Invoice Note: {invNote}.</i>\n");
                                message.AppendLine($"Request by: {requester}.\n");
                                message.AppendLine($"<b><i>ISP No.: #{invoiceNo}.</i></b>");
                                message.AppendLine($"<b><i>Status: {(invoiceType == "Created Invoice" ? "New Register" : "Renew Invoice")}.</i></b>\n");
                            }
                            else
                            {
                                message.AppendLine($"{username}\n");
                                message.AppendLine($"Dear {userFullName},\n");
                                message.AppendLine($"Now, you have invoice for DMC and TAX System need to check.\n");
                                message.AppendLine($"<i>ISP LINK: {billInfo?.IspLinkName}.</i>\n");
                                if (!string.IsNullOrEmpty(remark))
                                    message.AppendLine($"<i>{remark}.</i>\n");
                                if (!string.IsNullOrEmpty(invNote))
                                    message.AppendLine($"<i>Invoice Note: {invNote}.</i>\n");
                                message.AppendLine($"Request by: {requester}.\n");
                                message.AppendLine($"<b><i>NSM No.: #{invoiceNo}.</i></b>");
                                message.AppendLine($"<b><i>Status: {(invoiceType == "Created Invoice" ? "New Register" : "Renew Invoice")}.</i></b>\n");
                            }
                            outPutFiles = pdfResult?.OutPutFile;
                            captions = message.ToString();

                            if (isNeedApprove && !isNeedCheck)
                            {
                                long chatId = type == "NSM" ? -1002042086477 : -5009096861;
                                notifyResult = await notification.NotificationTelegramBot("", chatId, PDFFile: outPutFiles, Caption: captions, replyMarkupInlineKeyboardMarkup: true, inlineKeyboard: keyboard ?? null);
                            }
                            else
                            {
                                long chatId = -1001545132903;
                                int topicId = type == "NSM" ? 7167 : 23002;

                                notifyResult = await notification.NotificationTelegramBotToTopic("", chatId, topicId, inlineKeyboard: keyboard ?? null, Files: outPutFiles, Caption: captions);
                            }
                        }
                        else
                        {
                            message.AppendLine($"Dear Team,\n");
                            message.AppendLine($"Now we have new customer services that's {(invoiceType == "Created Invoice" ? "registered" : "renew")} by {requester}\n");
                            message.AppendLine($"<i>ISP LINK: {billInfo?.IspLinkName}.</i>\n");
                            message.AppendLine($"<b><i>NSM No.: #{invoiceNo}.</i></b>");
                            message.AppendLine($"<b><i>Status: {(invoiceType == "Created Invoice" ? "New Register" : "Renew Invoice")}.</i></b>\n");

                            outPutFiles = pdfResult?.OutPutFile;
                            captions = message.ToString();
                            long chatId = -1001545132903;
                            int topicId = invoiceType == "Created Invoice" ? 2 : 8;

                            notifyResult = await notification.NotificationTelegramBotToTopic("", chatId, topicId, Files: outPutFiles, Caption: captions);
                        }

                        if (isService && !IsRenew && !IsEdit && !IsChangePlan)
                        {
                            var technicalMessage = new StringBuilder();
                            technicalMessage.AppendLine($"✅ Invoice ready created, please be inform and prepare installation service for new customer.\n");
                            technicalMessage.AppendLine($"👉 <b>Customer Name: {customer?.CustomerName}</b>");
                            technicalMessage.AppendLine($"👉 <b>CID: {customer?.CustomerCode}</b>");
                            technicalMessage.AppendLine($"👉 <b>SID: {customerInvoice?.SID}</b>");
                            captions = technicalMessage.ToString();

                            int finishMsgId = Utility.TryParse(bankSlip?.FinishInstallMessageID, "int", 0);

                            await notification.NotificationTelegramBot(
                                "",
                                -1003778849283,
                                Caption: captions,
                                replyMarkupInlineKeyboardMarkup: true,
                                MessageID: finishMsgId,
                                edit: false
                            );
                        }
                    }

                    if (notifyResult.Result == ResultType.Successful)
                    {
                        System.IO.File.Delete(pdfResult.OutPutFile);
                    }

                    return Json(new
                    {
                        success = notifyResult.Result == ResultType.Successful,
                        message = notifyResult.Message ?? "Unknown response",
                        data = new
                        {
                            customerId,
                            invoiceNo
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadEployee()
        {
            HTML html = new HTML();
            //string SQL = "select EmployeeID, (EmployeeCode + ' - ' + EmployeeName) EmployeeName ";
            //SQL += "from dbo.TblEmployees ";
            //SQL += "where PositionID = 6 and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " ";
            //SQL += "order by(EmployeeCode +' - ' + EmployeeName)";
            string SQL = @"select LeadID EmployeeID, Salutation + ' ' + FullName EmployeeName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadTermCondition(int TermCondition = 0)
        {
            HTML html = new HTML();
            string SQL = "select top 1 Descp ";
            SQL += "from dbo.TblTermCondition ";
            SQL += "where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult CheckCustCode(string CustCode, string NewValue = "")
        {
            HTML html = new HTML();
            string SQL = "";
            SqlResult result = new SqlResult();
            if (NewValue == "")
            {
                SQL = "select CustomerCode from dbo.TblCustomer ";
                SQL += "where lower(CustomerCode) = N'" + CustCode.ToLower() + "' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            }
            else if (CustCode != NewValue)
            {
                SQL = "select CustomerCode from dbo.TblCustomer ";
                SQL += "where lower(CustomerCode) = N'" + NewValue.ToLower() + "' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            }
            result.Data = html.LoadData(SQL);
            result.Result = ResultType.Successful;
            if (result.Data.Tables.Count > 0)
            {
                if (result.Data.Tables[0].Rows.Count > 0)
                {
                    result.Message = "CID is exsiting in database.";
                    result.Result = ResultType.Failed;
                }
            }
            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadCustType()
        {
            HTML html = new HTML();
            string SQL = "select CustomerTypeID, CustomerType from dbo.TblCustomerType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerType";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadCustOrderType()
        {
            HTML html = new HTML();
            string SQL = "select OrderTypeID, OrderType from dbo.TblOrderType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by OrderType";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadIndustryType()
        {
            HTML html = new HTML();
            string SQL = "select IndustryTypeID, IndustryType from dbo.TblIndustryType where IndustryType <> 'Personal' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by IndustryType";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadServicePlan()
        {
            HTML html = new HTML();
            string SQL = "select distinct ts.ServiceID, ts.ServiceName from dbo.TblServices ts inner join dbo.TblPackages tp on tp.ServiceID = ts.ServiceID where ts.SystemID = " + html.SystemID + " and ts.WarehouseID=" + html.WarehouseID + " and ts.DomainID like '%" + html.DomainID + "%' order by ts.ServiceName";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadServicePlanAndOtherCharge(int LocationID = 0, int isRegisterForm = 0)
        {
            HTML html = new HTML();
            string SQL = @"select '0' StockInID, '0' ItemID, o.OtherChargeItem ItemName, isnull(o.Fee,0) SalePrice, o.UM, o.OtherChargeID,
'0' PackageDetailID, '0' PackageID, o.ServiceID, '0' Duration, '0' FreeDuration, '' PackageName, '0' Bandwidth,
'0' DowSpeed, '0' UpSpeed, '0' DepositFee, '' Feature, '3' OrderBy, NEWID() PID,
0 LocationID,  '' WarehouseName, s.ServiceName, '' PackageCode, coa.ChartOfAccountID
from dbo.TblOtherCharge o
inner join dbo.TblServices s on s.ServiceID = o.ServiceID
left join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = o.IncomeAccountID
where o.[Status] = 'Enable' and o.SystemID = @SystemID and o.WarehouseID = @WarehouseID and o.DomainID like '%'+@DomainID+'%'
union all
select distinct 0 StockInID, s.ItemID, i.ItemName + case when isnull(i.ItemCode,'') <> '' then ' - ' + i.ItemCode else case when isnull(i.SerialNumber,'') <> '' then ' - ' + i.SerialNumber else '' end end ItemName, 
isnull(iv.SellingPrice,0) SalePrice, iv.UM,
'0' OtherChargeID, '0' PackageDetailID, '0' PackageID, i.ItemTypeID ServiceID, '0' Duration, '0' FreeDuration, '' PackageName,
'0' Bandwidth, '0' DowSpeed, '0' UpSpeed, '0' DepositFee, '' Feature, '2' OrderBy, NEWID() PID,
s.LocationID, uw.WarehouseName, ss.ServiceName, '' PackageCode, coa.ChartOfAccountID
from dbo.TblStockInDetail s
inner join dbo.TblItem i on i.ItemID = s.ItemID
inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID
inner join dbo.TblServices ss on ss.ServiceID = i.ItemTypeID
inner join dbo.TblUserWarehouse uw on uw.WID = s.LocationID
left join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = i.SaleChartOfAccountID
where s.LocationID = case when @LocationID = 0 then LocationID else @LocationID end and i.SystemID = @SystemID and i.WarehouseID = @WarehouseID and i.DomainID like '%'+@DomainID+'%'
group by s.ItemID, i.ItemName, iv.SellingPrice, iv.UM, i.SerialNumber, i.ItemCode,s.LocationID, uw.WarehouseName, i.ItemTypeID, ss.ServiceName, coa.ChartOfAccountID
having (sum(isnull(s.QTY,0))-sum(isnull(s.QTYOUT,0))) > 0
union all
select '0' StockInID, '0' ItemID, trim(pd.PackageName) + ' - ' +
convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM AS ItemName,
pd.Price, (convert(varchar, pd.Duration) + ' ' + pd.UM) UM,
'0' OtherChargeID, isnull(pd.PackageDetailID, 0) PackageDetailID, 
isnull(pd.PackageID, 0) PackageID, isnull(pd.ServiceID, 0) ServiceID, 
isnull(pd.Duration, 0) Duration, isnull(pd.FreeDuration, 0) FreeDuration, 
isnull(pd.PackageName, 0) PackageName, isnull(pd.Bandwidth,0) Bandwidth, 
isnull(pd.DowSpeed,0) DowSpeed, isnull(pd.UpSpeed,0) UpSpeed, isnull(pd.DepositFee,0) DepositFee, pd.Feature, '1' OrderBy, NEWID() PID,
0 LocationID,  '' WarehouseName, pd.ServiceName, pd.PackageCode, coa.ChartOfAccountID
from dbo.TblPackageDetail pd
inner join dbo.TblPackages p on p.PackageID = pd.PackageID
left join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = pd.IncomeAccountID
where pd.Status='Enable' and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID and pd.DomainID like '%'+@DomainID+'%'
--union all
--select 0 StockInID, i.ItemID, i.ItemName, isnull(iv.SellingPrice,0) SellingPrice, isnull(iv.UM,0) UM, 0 OtherChargeID, 0 PackageDetailID, 0 PackageID, i.ItemTypeID ServiceID, 0 Duration, 0 FreeDuration,
--'' PackageName, 0 Bandwidth, 0 DowSpeed, 0 UpSeed, 0 DepositFee, '' Feature, 4 OrderBy, NEWID() PID, @LocationID LocationID, '' WarehouseName, 
--s.ServiceName, '' PackageCode, coa.ChartOfAccountID
--from dbo.TblItem i 
--inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID
--inner join dbo.TblServices s on s.ServiceID = i.ItemTypeID
--inner join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = i.SaleChartOfAccountID
--where i.ItemID not in (select ItemID from dbo.TblStockInDetail) and i.SystemID = @SystemID and i.WarehouseID = @WarehouseID and i.DomainID like '%'+@DomainID+'%'
--and @isRegisterForm = 1
union all
select 0 StockInID, i.ItemID, i.ItemName, i.SellingPrice, i.UM, 0 OtherChargeID, 0 PackageDetailID, 0 PackageID, i.ItemTypeID ServiceID, 0 Duration,
0 FreeDuration, '' PackageName, 0 Bandwidth, 0 DowSpeed, 0 UpSpeed, 0 DepositFee, '' Feature, 5 OrderBy, NEWID() PID, @LocationID LocationID,
'' WarehouseName, s.ServiceName, '' PackageCode, coa.ChartOfAccountID
from dbo.TblItem i
inner join dbo.TblServices s on s.ServiceID = i.ItemTypeID
left join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = i.SaleChartOfAccountID
where s.ServiceName <> 'Inventory' and i.SystemID = @SystemID and i.WarehouseID = @WarehouseID and i.DomainID like '%'+@DomainID+'%'
order by OrderBy, ItemName";

            if (isRegisterForm == 1)
            {
                SQL = @"select '0' StockInID, '0' ItemID, o.OtherChargeItem ItemName, isnull(o.Fee,0) SalePrice, o.UM, o.OtherChargeID,
'0' PackageDetailID, '0' PackageID, o.ServiceID, '0' Duration, '0' FreeDuration, '' PackageName, '0' Bandwidth,
'0' DowSpeed, '0' UpSpeed, '0' DepositFee, '' Feature, '3' OrderBy, NEWID() PID,
0 LocationID,  '' WarehouseName, s.ServiceName, '' PackageCode, coa.ChartOfAccountID
from dbo.TblOtherCharge o
inner join dbo.TblServices s on s.ServiceID = o.ServiceID
left join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = o.IncomeAccountID
where o.[Status] = 'Enable' and o.SystemID = @SystemID and o.WarehouseID = @WarehouseID and o.DomainID like '%'+@DomainID+'%'
union all
select distinct 0 StockInID, i.ItemID, i.ItemName + case when isnull(i.ItemCode,'') <> '' then ' - ' + i.ItemCode else case when isnull(i.SerialNumber,'') <> '' then ' - ' + i.SerialNumber else '' end end ItemName, 
isnull(iv.SellingPrice,0) SalePrice, iv.UM,
'0' OtherChargeID, '0' PackageDetailID, '0' PackageID, i.ItemTypeID ServiceID, '0' Duration, '0' FreeDuration, '' PackageName,
'0' Bandwidth, '0' DowSpeed, '0' UpSpeed, '0' DepositFee, '' Feature, '2' OrderBy, NEWID() PID,
s.LocationID, uw.WarehouseName, ss.ServiceName, '' PackageCode, coa.ChartOfAccountID
from dbo.TblItem i
left join dbo.TblStockInDetail s on i.ItemID = s.ItemID
inner join dbo.TblItemVariant iv on iv.ItemID = i.ItemID
inner join dbo.TblServices ss on ss.ServiceID = i.ItemTypeID
left join dbo.TblUserWarehouse uw on uw.WID = s.LocationID
left join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = i.SaleChartOfAccountID
where i.SystemID = @SystemID and i.WarehouseID = @WarehouseID and i.DomainID like '%'+@DomainID+'%'
group by i.ItemID, i.ItemName, iv.SellingPrice, iv.UM, i.SerialNumber, i.ItemCode,s.LocationID, uw.WarehouseName, i.ItemTypeID, ss.ServiceName, coa.ChartOfAccountID
union all
select '0' StockInID, '0' ItemID, (trim(pd.ServiceName) + ' - ' + case when p.PackageCode <> '' then p.PackageCode + ' - ' else '' end + trim(pd.PackageName) + ' - ' +
convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM) ItemName,
pd.Price, (convert(varchar, pd.Duration) + ' ' + pd.UM) UM,
'0' OtherChargeID, isnull(pd.PackageDetailID, 0) PackageDetailID, 
isnull(pd.PackageID, 0) PackageID, isnull(pd.ServiceID, 0) ServiceID, 
isnull(pd.Duration, 0) Duration, isnull(pd.FreeDuration, 0) FreeDuration, 
isnull(pd.PackageName, 0) PackageName, isnull(pd.Bandwidth,0) Bandwidth, 
isnull(pd.DowSpeed,0) DowSpeed, isnull(pd.UpSpeed,0) UpSpeed, isnull(pd.DepositFee,0) DepositFee, pd.Feature, '1' OrderBy, NEWID() PID,
0 LocationID,  '' WarehouseName, pd.ServiceName, pd.PackageCode, coa.ChartOfAccountID
from dbo.TblPackageDetail pd
inner join dbo.TblPackages p on p.PackageID = pd.PackageID
inner join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = pd.IncomeAccountID
where pd.Status='Enable' and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID and pd.DomainID like '%'+@DomainID+'%'
union all
select 0 StockInID, i.ItemID, i.ItemName, i.SellingPrice, i.UM, 0 OtherChargeID, 0 PackageDetailID, 0 PackageID, i.ItemTypeID ServiceID, 0 Duration,
0 FreeDuration, '' PackageName, 0 Bandwidth, 0 DowSpeed, 0 UpSpeed, 0 DepositFee, '' Feature, 5 OrderBy, NEWID() PID, @LocationID LocationID,
'' WarehouseName, s.ServiceName, '' PackageCode, coa.ChartOfAccountID
from dbo.TblItem i
inner join dbo.TblServices s on s.ServiceID = i.ItemTypeID
left join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = i.SaleChartOfAccountID
where s.ServiceName <> 'Inventory' and i.SystemID = @SystemID and i.WarehouseID = @WarehouseID and i.DomainID like '%'+@DomainID+'%'
order by OrderBy, ItemName";
            }

            Dictionary<string, string> para = new Dictionary<string, string>{
                { "LocationID", LocationID.ToString() },
                { "SystemID", html.SystemID},
                { "WarehouseID", html.WarehouseID},
                { "isRegisterForm", isRegisterForm.ToString() },
                { "DomainID", html.DomainID},
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadPackages(int ServiceID, string InvoiceType, int CustomerID = 0, string InvoiceNo = "")
        {
            HTML html = new HTML();
            string SQL = @"select pd.PackageID, pd.PackageDetailID, 
concat(p.PackageName, ' - ', pd.Bandwidth, ' Mbps', ' - ', pd.Duration, ' ', pd.UM) as PackageName, pd.Price, pd.Feature, isnull(pd.DepositFee, 0) DepositFee, isnull(pd.FreeDuration,0) FreeDuration, pd.Duration, pd.UM, p.SpecialNote,
pd.Bandwidth, pd.DowSpeed, pd.UpSpeed, p.PackageName OriPackageName, NEWID() PID, pd.PackageCode, p.ServiceID, s.ServiceName, coa.ChartOfAccountID, (convert(varchar, pd.Duration) + ' ' + pd.UM) as UM2
from dbo.TblPackages p 
inner join dbo.TblPackageDetail pd on pd.PackageID = p.PackageID 
inner join dbo.TblServices s on s.ServiceID = p.ServiceID
left join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = pd.IncomeAccountID
where p.PackageStatus = 'Enable' and pd.Status='Enable' and p.ServiceID = @ServiceID and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID order by p.PackageName, pd.PackageDetailID;";

            string SQL2 = $@"select ConnectionID, SID from dbo.TblCustomerInvoice where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo and StatusID <> 12;";

            var para = new Dictionary<string, string>()
            {
                { "CustomerID", CustomerID.ToString() },
                { "InvoiceNo", InvoiceNo },
                { "ServiceID", ServiceID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
            };

            return Json(new { data = html.JsonData(SQL, false, para), data2 = html.JsonData(SQL2, false, para) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadPackages2(int ServiceID, string InvoiceType, int CustomerID = 0, string InvoiceNo = "")
        {
            HTML html = new HTML();
            string SQL = @"select pd.PackageID, pd.PackageDetailID, 
concat(p.PackageName, ' - ', pd.Bandwidth, ' Mbps', ' - ', pd.Duration, ' ', pd.UM, '(', pd.Price, ')') as PackageName, pd.Price, pd.Feature, isnull(pd.DepositFee, 0) DepositFee, isnull(pd.FreeDuration,0) FreeDuration, pd.Duration, pd.UM, p.SpecialNote,
pd.Bandwidth, pd.DowSpeed, pd.UpSpeed, p.PackageName OriPackageName, NEWID() PID, pd.PackageCode, p.ServiceID, s.ServiceName, coa.ChartOfAccountID, (convert(varchar, pd.Duration) + ' ' + pd.UM) as UM2
from dbo.TblPackages p 
inner join dbo.TblPackageDetail pd on pd.PackageID = p.PackageID 
inner join dbo.TblServices s on s.ServiceID = p.ServiceID
left join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = pd.IncomeAccountID
where p.PackageStatus = 'Enable' and pd.Status='Enable' and p.ServiceID = @ServiceID and pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID order by p.PackageName, pd.PackageDetailID;";

            string SQL2 = $@"select ConnectionID, SID from dbo.TblCustomerInvoice where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo and StatusID <> 12;";

            var para = new Dictionary<string, string>()
            {
                { "CustomerID", CustomerID.ToString() },
                { "InvoiceNo", InvoiceNo },
                { "ServiceID", ServiceID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
            };

            return Json(new { data = html.JsonData(SQL, false, para), data2 = html.JsonData(SQL2, false, para) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadOtherCharge()
        {
            HTML html = new HTML();
            string SQL = @"
                    select 
                        0 ItemID,
                        tc.OtherChargeID,
                        tc.OtherChargeItem as ItemName,
                        tc.UM,
                        tc.CostPrice as SalePrice
                    from dbo.TblOtherCharge tc 
                    where tc.SystemID = 13";

            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadClassName()
        {
            HTML html = new HTML();
            string SQL = "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ClassName";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadLocation()
        {
            HTML html = new HTML();
            string SQL = "select uw.WID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName, uw.ContactName, uw.ContactPhone, uw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + case when uw.District <> '' then ', ' + uw.District end + case when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end Warehouse from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " and uw.DomainID like '%" + html.DomainID + "%' order by (b.BranchName + ' - ' + uw.WarehouseName)";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadCompanyLogo()
        {
            HTML html = new HTML();
            string SQL = "select top 1 CompanyID, Logo, TermConditionVAT, TermConditionNonVAT, CompanyAddress, Phone, Email, Website, QuoteTermConditionNonVAT, QuoteTermConditionVAT from dbo.TblCompany where ISDefault = 1 and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadCommune()
        {
            HTML html = new HTML();
            string SQL = @"select c.CommuneID, c.Commune, d.DistrictID, d.District, p.ProvinceCity, p.ProvinceCityID, 0 OrderBy
from dbo.TblCommune c
inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID
inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + @"%'
union
select c.CommuneID, c.CommuneKH Commune, d.DistrictID, d.DistrictKH District, p.ProvinceCityKH ProvinceCity, p.ProvinceCityID, 1 OrderBy
from dbo.TblCommune c
inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID
inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + @"%'
order by c.Commune, OrderBy";
            return Json(new { data = HttpUtility.HtmlDecode(html.JsonData(SQL)) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadDistrict(int DistrictID)
        {
            HTML html = new HTML();
            string SQL = "select DistrictID, District from dbo.TblDistrict where DistrictID=" + DistrictID + " and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by District";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadProvinceCity()
        {
            HTML html = new HTML();
            string SQL = "select ProvinceCityID, ProvinceCity from dbo.TblProvinceCity where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ProvinceCity";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Customer, Edit Customer, Add Sale Order, Edit Sale Order")]
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
            sb.AppendLine("<UserID>" + html.UserID + "</UserID>");
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
                                    StockOnHand += (int)decimal.Parse(ds2.Tables[0].Rows[0]["QTY"].ToString());
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
                string strD = GetPreviewInvoice(sb.ToString());
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
            if (CustomerID == "0")
            {
                RegisterDto dto = new RegisterDto();
                var dt = sqlresult.Data.Tables[0];
                dto.Username = dt.Rows[0]["CustomerCode"].ToString();
                dto.CustomerID = int.Parse(dt.Rows[0]["CustomerID"].ToString());
                dto.PhoneNumber = CustomerDataInfo[0]["CustomerMobile"].ToString();
                dto.Password = "NSMISP@2025";
                var rst = await AuthService.RegisterAsync(dto);
            }
            string InvoiceData = html.JsonConvertDataTable(sqlresult.Data.Tables[0]);
            string SupportTicketData = "";
            if (sqlresult.Data.Tables.Count > 1)
            {
                SupportTicketData = html.JsonConvertDataTable(sqlresult.Data.Tables[1]);
            }
            DoActionDataController doActionData = new DoActionDataController();
            await doActionData.AddUserToTelegram(sqlresult.Data.Tables[0].Rows[0]["CustomerID"].ToString(), HttpUtility.HtmlDecode(HttpUtility.UrlDecode(CustContactPhone)), sqlresult.Data.Tables[0].Rows[0]["CustomerName"].ToString(), sqlresult.Data.Tables[0].Rows[0]["CustomerCode"].ToString());
            NotificationTelegram notificationTelegram = new NotificationTelegram();

            //await notificationTelegram.AddCustomerTelegram(int.Parse(sqlresult.Data.Tables[0].Rows[0]["CustomerID"].ToString()), HttpUtility.HtmlDecode(HttpUtility.UrlDecode(CustContactPhone)), HttpUtility.UrlDecode(CustomerDataInfo[0]["CustomerName"]), CustomerDataInfo[0]["CustomerCode"], "", html.GetCurrentDate(), html.UserFullName, html.UserID, html.SystemID, html.WarehouseID, html.DomainID);

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
        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> SendNotificationToTelegram(string CustomerName, string CustomerCode,
            int CustomerID, string InvoiceNo, string InvoiceDate, int IsIncludedVAT, string DescriptionTicketTelegram,
            string DescriptionTicketTelegramSupportTeam, int StatusID = 11, int ISSO = 0, bool ApprovalPO = false,
            string TicketNumber = "", string InvoiceType = "", bool IsRequestApprovalSO = false, bool IsNoNeedApproval = false, bool isInternetServices = true, bool IsNeedToCheckApproval = false, int statusIdApproveInvCISP = 0, string Username = "")
        {
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            SqlResult sqlresult = new SqlResult();
            SqlResult result = new SqlResult();
            HTML html = new HTML();
            dynamic rst;
            CustomerReportsController customerReportsController = new CustomerReportsController();
            if (IsNoNeedApproval)
            {
                rst = await customerReportsController.ApprovalInvoiceToDMCTAX(CustomerID, InvoiceNo, "", "Approve", "", true, isInternetServices);
                //var ds = rst.Data.DataResult;
                //DataRow dr = ds.Tables[0].Rows[0];
                //CustomerName = dr["CustomerName"].ToString();
                //CustomerCode = dr["CustomerCode"].ToString();
            }
            else
            {
                if (statusIdApproveInvCISP != 0)
                {
                    rst = await this.ApprovalInvCISP(CustomerID, InvoiceNo, statusIdApproveInvCISP, Username);
                    if (statusIdApproveInvCISP != 90)
                        return Json(new { Result = rst.Result, Messsage = rst.Message });
                }
            }
            if (!IsNeedToCheckApproval)
            {
                result = await html.InvoiceToPDFandUpload(CustomerName, CustomerCode, CustomerID, InvoiceNo, InvoiceDate, IsIncludedVAT, StatusID, int.Parse(html.SystemID.ToString()), int.Parse(html.WarehouseID.ToString()), false, "", "/ISP_Invoice", ISSO == 0, InvoiceType);
                if (result.Result == ResultType.Successful)
                {
                    string outPutFile = result.OutPutFile;
                    //send notification to finance -635916546 = finance // -1801296528 = bill notification //
                    //sqlresult = await notificationTelegram.NotificationTelegramBot("", -635916546, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegram), true);
                    //TopticID = 2 // new install
                    //NotificationTelegramBotToTopic(string MsgText, int chatId = 0, int messageThreadId = 0, InlineKeyboardMarkup inlineKeyboard = null, int MessageID = 0, string TelegramMessage = "", string Files="", string Caption="")
                    //1545132903 = finance
                    if (InvoiceType == "Renew Invoice")
                        sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 8, null, 0, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegram)); // send notification to finance renew 1545132903_8
                    else if (ISSO == 2)
                    {
                        string sql = @"SELECT DestinationFolder + '/'+ value AS [FileName], isnull(TypeFor, '') TypeFor
FROM dbo.TblCustomerAttachmentFile
CROSS APPLY STRING_SPLIT(AttachmentFileName, '~') where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo";
                        Dictionary<string, string> para = new Dictionary<string, string>
                        {
                            { "CustomerID", CustomerID.ToString() },
                            { "InvoiceNo", InvoiceNo },
                        };

                        DataSet ds = html.LoadData(sql, false, para);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string attfile = "", TypeFor = "";
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                if (attfile == "")
                                {
                                    attfile = dr["FileName"].ToString();
                                    TypeFor = dr["TypeFor"].ToString();
                                }
                                else
                                {
                                    attfile += "~" + dr["FileName"].ToString();
                                    TypeFor += "~" + dr["TypeFor"].ToString();
                                }
                            }
                            string outPutFiles = outPutFile + "~" + attfile;
                            string Captions = TypeFor + "~" + HttpUtility.UrlDecode(DescriptionTicketTelegram);
                            sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 54, null, 0, "", outPutFiles, Captions);
                            // -986646049 = check po // -967102140 = approval po // -844276850 = test							
                        }
                        else
                        {
                            sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 54, null, 0, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegram));//1545132903_54 PI
                        }
                        sql = "update TblCustomerBalance set TelegramMessageID = '" + sqlresult.Value + "' where CustomerID=" + CustomerID + " and InvoiceNo='" + InvoiceNo + "'";
                        sqlresult = html.SaveChangeData(sql, false, false);
                    }
                    else if (StatusID == 20) //for quotation
                        sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 261, null, 0, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegram));
                    else if (!IsRequestApprovalSO)
                        sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 2, null, 0, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegram));
                    if (!IsRequestApprovalSO && isInternetServices)
                    {
                        if (ApprovalPO && DescriptionTicketTelegramSupportTeam != "")
                        {
                            //send notification to osp team
                            long TelegramGroupIDOSP = -1002161346708;

                            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                                {
                                new []
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "▶️ Start Lay Cable", callbackData: "Start Lay Cable ~ "+ CustomerID.ToString()+"~"+TicketNumber+"~"+TelegramGroupIDOSP.ToString()+"~0~0"),
                                }
                            });

                            string sql = @"SELECT af.DestinationFolder + '/'+ value AS [FileName], isnull(af.TypeFor, '') TypeFor, af.InvoiceNo
FROM dbo.TblCustomerAttachmentFile af
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = af.CustomerID
CROSS APPLY STRING_SPLIT(AttachmentFileName, '~') where af.CustomerID = @CustomerID 
and ci.InvoiceNo = @InvoiceNo and isnull(af.TypeFor, '') in('Attachment File','AttachmentFile')";
                            Dictionary<string, string> para = new Dictionary<string, string>
                            {
                                { "CustomerID", CustomerID.ToString() },
                                { "InvoiceNo", InvoiceNo },
                            };

                            DataSet ds = html.LoadData(sql, false, para);
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                string attfile = "", TypeFor = "";
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    if (attfile == "")
                                    {
                                        attfile = dr["FileName"].ToString();
                                        TypeFor = dr["TypeFor"].ToString();
                                    }
                                    else
                                    {
                                        attfile += "~" + dr["FileName"].ToString();
                                        TypeFor += "~" + dr["TypeFor"].ToString();
                                    }
                                }
                                string outPutFiles = attfile;
                                string Captions = TypeFor;

                                sqlresult = await notificationTelegram.NotificationTelegramBot(HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam), TelegramGroupIDOSP, "", outPutFiles, Captions, false, true, 0, 0, "", inlineKeyboard);
                            }
                            else
                                sqlresult = await notificationTelegram.NotificationTelegramBot(HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam), TelegramGroupIDOSP, "", "", "", false, true, 0, 0, "", inlineKeyboard);
                            //TelegramGroupID = -2161346708 /OSP Team New Install Internet Services
                            //TelegramGroupID = -844276850 =test/-827559974=Technical Support
                            string TelegramGroupNameAndMessageID = TelegramGroupIDOSP.ToString() + "|" + sqlresult.Value;
                            ///OSP Team New Install Internet Services
                            //send notification to technical support
                            int TelegramGroupIDTS = -1903890245;
                            int TopicID = 7;
                            inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                    new[]
                                {
                                        InlineKeyboardButton.WithCallbackData(text: "▶️ Start Install", callbackData: "Start Install ~ " + CustomerID.ToString() + "~" + TicketNumber + "~" + TelegramGroupIDTS.ToString() + "~Start Install~TECH"),
                                    },
                                });
                            //sqlresult = await notificationTelegram.NotificationTelegramBot(HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam), TelegramGroupIDTS, "", "", "", false, true, 0, 0, "", inlineKeyboard);                   
                            //NotificationTelegramBotToTopic(string MsgText, int chatId = 0, int topicID = 0, InlineKeyboardMarkup inlineKeyboard = null, int MessageID = 0, string TelegramMessage = "", string Files="", string Caption="")
                            sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", TelegramGroupIDTS, TopicID, inlineKeyboard, 0, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam));
                            TelegramGroupNameAndMessageID += "~" + TelegramGroupIDTS.ToString() + "|" + sqlresult.Value;
                            sql = "update dbo.TblSupportOpenTicket set TelegramMessageID=@TelegramMessageID, TelegramGroupNameAndMessageID=@TelegramGroupNameAndMessageID, TelegramMessage = @TelegramMessage where TicketNumber = @TicketNumber and CustomerID = @CustomerID";
                            para = new Dictionary<string, string>
                            {
                                {"TelegramMessageID",TelegramGroupNameAndMessageID },
                                {"TelegramGroupNameAndMessageID",TelegramGroupNameAndMessageID },
                                {"TelegramMessage",HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam) },
                                {"TicketNumber",TicketNumber },
                                {"CustomerID",CustomerID.ToString() },
                            };
                            sqlresult = html.SaveChangeData(sql, false, para);
                        }
                        else if (ISSO == 0)
                        {
                            //notifcation to technical team =1903890245_7 technical =-1689604171_2 test
                            //sqlresult = await notificationTelegram.NotificationTelegramBot(HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam), -827559974);//-827559974 = NSM ISP BILLING TECHNICAL
                            if (DescriptionTicketTelegramSupportTeam != "" && InvoiceType != "Credit Notes")
                            {
                                if (InvoiceType != "Renew Invoice")
                                {
                                    string TelegramMessageID = "";
                                    string sql = @"select isnull(TelegramMessageID, '') TelegramMessageID from TblCustomerBalance where CustomerID = " + CustomerID + " and InvoiceNo = '" + InvoiceNo + "'";
                                    DataSet ds = html.LoadData(sql);
                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        TelegramMessageID = ds.Tables[0].Rows[0][0].ToString();
                                    }
                                    sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", -1903890245, 7, null, 0, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam), false, TelegramMessageID);
                                    sql = @"update dbo.TblCustomerBalance set TelegramMessageID = '" + sqlresult.Value + "' where CustomerID = " + CustomerID + " and InvoiceNo = '" + InvoiceNo + "'";
                                    html.SaveChangeData(sql, false, false);
                                }
                                //else if (InvoiceType == "Renew Invoice")
                                //  sqlresult = await notificationTelegram.NotificationTelegramBotToTopic("", -1903890245, 13, null, 0, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam));
                            }
                        }
                        else if (ISSO == 2)
                        {
                            //send notification to sale 954558193 = sale // 977486427 = test
                            string sql = @"SELECT DestinationFolder + '/'+ value AS [FileName], isnull(TypeFor, '') TypeFor
FROM dbo.TblCustomerAttachmentFile
CROSS APPLY STRING_SPLIT(AttachmentFileName, '~') where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo";
                            Dictionary<string, string> para = new Dictionary<string, string>
                            {
                                { "CustomerID", CustomerID.ToString() },
                                { "InvoiceNo", InvoiceNo },
                            };

                            DataSet ds = html.LoadData(sql, false, para);
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                string attfile = "", TypeFor = "", outPutFiles;
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    if (attfile == "")
                                    {
                                        attfile = dr["FileName"].ToString();
                                        TypeFor = dr["TypeFor"].ToString();
                                    }
                                    else
                                    {
                                        attfile += "~" + dr["FileName"].ToString();
                                        TypeFor += "~" + dr["TypeFor"].ToString();
                                    }
                                }
                                outPutFiles = outPutFile + "~" + attfile;
                                string Captions = TypeFor + "~" + HttpUtility.UrlDecode(DescriptionTicketTelegram);
                                sqlresult = await notificationTelegram.NotificationTelegramBot("", -954558193, "", outPutFiles, Captions, true); // -954558193=NSM ISP NOTIFICATION FOR SALE
                                                                                                                                                 // -986646049 = check po // -967102140 = approval po // -844276850 = test
                            }
                            else
                            {
                                sqlresult = await notificationTelegram.NotificationTelegramBot("", -954558193, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegram), true); // -954558193=NSM ISP NOTIFICATION FOR SALE
                            }

                        }
                    }

                    if (System.IO.File.Exists(result.OutPutFile))
                    {
                        System.IO.File.Delete(result.OutPutFile);
                    }
                }
            }
            //dynamic obj = new ExpandoObject();
            //obj.InvoiceNo = InvoiceNo;
            //obj.CustomerName = CustomerName;
            //obj.CustomerID = CustomerID;

            var objList = new List<object>
            {
                new
                {
                    InvoiceNo = InvoiceNo,
                    CustomerName = CustomerName,
                    CustomerID = CustomerID
                }
            };
            return Json(new { Data = objList, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult CustomerContractToPDF(string Data, string CustomerName, string CustomerCode, string InvoiceNo)
        {
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            sqlResult = html.URLtoPDF("AddCustomer/CreateCustomerImg", Data, CustomerName, CustomerCode, InvoiceNo).Result;
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> InvoiceToPDFandUpload(string CustomerName, string CustomerCode, int CustomerID, string InvoiceNo, string InvoiceDate, string IsIncludedVAT, int StatusID, bool UploadToServer = false, string OfficialReceiptNo = "")
        {
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            try
            {
                string[] dtInvoiceNo = InvoiceNo.Split('~');
                if (dtInvoiceNo.Length > 0)
                {
                    string[] dtInvoiceDate = InvoiceDate.Split('~');
                    string[] dtIsIncludedVAT = IsIncludedVAT.Split('~');
                    for (int i = 0; i < dtInvoiceNo.Length; i++)
                    {
                        sqlResult = await html.InvoiceToPDFandUpload(
                            CustomerName,
                            CustomerCode,
                            CustomerID,
                            dtInvoiceNo[i],
                            dtInvoiceDate[i],
                            int.Parse(dtIsIncludedVAT[i].ToString()),
                            StatusID,
                            int.Parse(html.SystemID),
                            int.Parse(html.WarehouseID),
                            UploadToServer,
                            OfficialReceiptNo
                            );
                    }
                }
                else
                {
                    sqlResult = await html.InvoiceToPDFandUpload(
                        CustomerName,
                        CustomerCode,
                        CustomerID,
                        InvoiceNo,
                        InvoiceDate,
                        int.Parse(IsIncludedVAT),
                        StatusID,
                        int.Parse(html.SystemID),
                        int.Parse(html.WarehouseID),
                        UploadToServer,
                        OfficialReceiptNo
                        );
                }
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Customer, Edit Customer")]
        public ActionResult AddQuotation(object[] PrimaryFieldValue)
        {
            ViewBag.CustQuotationInfo = "";
            ViewBag.CustQuotationDetail = "";
            ViewBag.InvData = "";
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(PrimaryFieldValue[0].ToString());

            string InvoiceNo = DataInfo["InvoiceNo"].ToString();
            string CustomerID = DataInfo["CustomerID"].ToString();

            if (CustomerID != "0")
            {
                string sql = "select (e.Salutation + e.FullName) Employee, c.EmployeeID, c.CustomerID, ty.CustomerType, c.CustomerTypeID, c.CustomerName, c.CustomerCode, c.CustomerGender, c.CustomerAddress, ";
                sql += "c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity, ";
                sql += "c.CustomerMobile, c.CustomerEmail, c.CustomerNationality, c.CustomerIdentification, ";
                sql += "c.CustomerContactName, c.CustomerContactMobileNumber, c.CustomerContactEmail ";
                sql += "from dbo.TblCustomer c ";
                sql += "inner join dbo.TblCustomerType ty on ty.CustomerTypeID = c.CustomerTypeID ";
                sql += "inner join dbo.TblLeads e on e.LeadID = c.EmployeeID ";
                sql += "where CustomerID = " + CustomerID + "; ";
                HTML html = new HTML();
                ViewBag.CustQuotationInfo = html.JsonData(sql);
                sql = "select cb.CustomerBillInformationID, cb.CompanyName, cb.CompanyNameKH, cb.CompanyDeskPhone, cb.CompanyMobile, ";
                sql += "cb.CompanyEmail, cb.VATNumber, cb.CompanyAddress, cb.CompanyCommune, ";
                sql += "cb.CompanyDistrict, cb.CompanyProvinceCity, cb.CompanyContactName, ";
                sql += "cb.CompanyContactPosition, cb.CompanyContactMobileNumber, ";
                sql += "cb.BillToAddress, cb.BillToCommune, cb.BillToDistrict, cb.BillToProvinceCity, ";
                sql += "cb.InstallAddress, cb.InstallCommune, cb.InstallDistrict, cb.InstallProvinceCity ";
                sql += "from dbo.TblCustomerBillInformation cb ";
                sql += "where cb.CustomerID = " + CustomerID;
                ViewBag.CustQuotationDetail = html.JsonData(sql);
            }

            if (InvoiceNo != "0")
            {
                Dictionary<string, string> Para = new Dictionary<string, string>()
                {
                    { "CustID", CustomerID },
                    { "InvoiceNo", InvoiceNo }
                };
                DataSet ds = new DataSet();
                HTML html = new HTML();
                ds = html.LoadData("SP_GET_CUSTOMER_INVOICE", true, Para);
                ViewBag.InvData = JsonConvert.SerializeObject(ds);
            }
            return View();
        }

        [Authorize(Roles = "Add Customer, Edit Customer")]
        public ActionResult SaveChangeQuotation(object[] CustomerData, object[] CustInvoice, object[] CustomerOtherChargeInvoice)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            dynamic CustomerDataInfo = new JavaScriptSerializer().DeserializeObject(CustomerData[0].ToString());
            dynamic CustomerInvoice = new JavaScriptSerializer().DeserializeObject(CustInvoice[0].ToString());
            dynamic CustomerInvoiceOtherCharge = new JavaScriptSerializer().DeserializeObject(CustomerOtherChargeInvoice[0].ToString());
            HTML html = new HTML();
            string SystemID = html.SystemID;
            string WarehouseID = html.WarehouseID;
            string DomainID = html.DomainID;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<Data>");
            sb.AppendLine("<CustomerInfo>");
            foreach (dynamic item in CustomerDataInfo[0])
            {
                string key = item.Key;
                string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                sb.AppendLine("<" + key + ">" + html.EscapeXml(value) + "</" + key + ">");
            }
            sb.AppendLine("<StatusID>20</StatusID>");
            sb.AppendLine("<RegisterDate>" + easternTimeNow + "</RegisterDate>");
            sb.AppendLine("<SystemID>" + SystemID + "</SystemID>");
            sb.AppendLine("<WarehouseID>" + WarehouseID + "</WarehouseID>");
            sb.AppendLine("<DomainID>" + DomainID + "</DomainID>");
            sb.AppendLine("</CustomerInfo>");

            sb.AppendLine("<CustomerInvoice>");
            foreach (dynamic item in CustomerInvoice[0])
            {
                string key = item.Key;
                string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                sb.AppendLine("<" + key + ">" + html.EscapeXml(value) + "</" + key + ">");
            }
            sb.AppendLine("<StatusID>20</StatusID>");
            sb.AppendLine("<InvoiceDate>" + easternTimeNow + "</InvoiceDate>");
            sb.AppendLine("</CustomerInvoice>");

            foreach (dynamic data in CustomerInvoiceOtherCharge)
            {
                sb.AppendLine("<CustomerOtherCharge>");
                foreach (dynamic item in data)
                {
                    string key = item.Key;
                    string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                    sb.AppendLine("<" + key + ">" + html.EscapeXml(value) + "</" + key + ">");
                }
                sb.AppendLine("<StatusID>20</StatusID>");
                sb.AppendLine("<OtherChargeDate>" + easternTimeNow + "</OtherChargeDate>");
                sb.AppendLine("</CustomerOtherCharge>");
            }

            sb.AppendLine("</Data>");
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "Data", sb.ToString() }
            };
            SqlResult sqlresult = html.SaveChangeData("SP_INSERT_CUSTOMER", true, Para, true);
            if (sqlresult.Result == ResultType.Successful)
            {
                string sql = "select distinct CustomerID, InvoiceNo, IsIncludedVAT, StatusID ";
                sql += "from dbo.TblCustomerInvoice where InvoiceNo = N'" + sqlresult.Value + "'";
                sqlresult.Value = html.JsonData(sql);
            }
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value
            });
        }

        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadContactName()
        {
            HTML html = new HTML();
            string SQL = "select Distinct ContactName from dbo.TblCustomer where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and ContactName is not null order by ContactName";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadNationality()
        {
            HTML html = new HTML();
            string SQL = @"select NationalityKH CustomerNationality, OrderBy, 0 neworder from dbo.TblNationality
where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + @"
--union all
--select Nationality CustomerNationality, OrderBy, 1 neworder from dbo.TblNationality
--where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + @"
order by OrderBy, neworder, CustomerNationality";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveChangeCustInfo(object[] CustomerData)
        {
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            dynamic CustomerDataInfo = new JavaScriptSerializer().DeserializeObject(CustomerData[0].ToString());
            Dictionary<string, string> Para = new Dictionary<string, string>();
            string SQL = "update dbo.TblCustomer set ";
            string updata = "";
            foreach (dynamic item in CustomerDataInfo[0])
            {
                string key = item.Key;
                string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                Para.Add(key, value);
                if (key != "CustomerID")
                {
                    if (updata == "")
                        SQL += key + " = @" + key;
                    else
                        SQL += ", " + key + " = @" + key;
                    updata = key;
                }
            }
            SQL += " where CustomerID = @CustomerID";
            sqlresult = html.SaveChangeData(SQL, false, Para);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveChangeCompanyInfo(object[] CompanyData)
        {
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            dynamic CustomerDataInfo = new JavaScriptSerializer().DeserializeObject(CompanyData[0].ToString());
            Dictionary<string, string> Para = new Dictionary<string, string>();
            string SQL = "update dbo.TblCustomerCompany set ";
            string updata = "";
            foreach (dynamic item in CustomerDataInfo[0])
            {
                string key = item.Key;
                string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                Para.Add(key, value);
                if (key != "CustomerID")
                {
                    if (updata == "")
                        SQL += key + " = @" + key;
                    else
                        SQL += ", " + key + " = @" + key;
                    updata = key;
                }
            }
            SQL += " where CustomerID = @CustomerID;";
            SQL += "update dbo.TblCustomer set IndustryTypeID = @IndustryTypeID where CustomerID = @CustomerID;";
            sqlresult = html.SaveChangeData(SQL, false, Para);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveChangeCustomerBillInfo(object[] CustomerBillData)
        {
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            dynamic CustomerDataInfo = new JavaScriptSerializer().DeserializeObject(CustomerBillData[0].ToString());
            Dictionary<string, string> Para = new Dictionary<string, string>();
            string SQL = "update dbo.TblCustomerBillInformation set ";
            string updata = "";
            foreach (dynamic item in CustomerDataInfo)
            {
                string key = item.Key;
                string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                Para.Add(key, value);
                if (key != "CustomerBillInformationID")
                {
                    if (updata == "")
                        SQL += key + " = @" + key;
                    else
                        SQL += ", " + key + " = @" + key;
                    updata = key;
                }
            }
            SQL += " where CustomerBillInformationID = @CustomerBillInformationID;";
            SQL += "update TblCustomerInvoice set CustomerName = @CustomerName where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo;";
            SQL += "update TblCustomer set CustomerName = @CustomerName where CustomerID = @CustomerID;";
            sqlresult = html.SaveChangeData(SQL, false, Para);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value
            }, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadInvoiceTmp()
        {
            HTML html = new HTML();
            string SQL = "select CompanyID, InvoiceTemplateName from dbo.TblCompany where SystemID =" + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by case when ISDefault = 1 then 0 else OrderNumber end";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetCustomerLocationByGoogleMap(string lat = "", string lng = "")
        {
            ViewBag.Latitude = lat;
            ViewBag.Longitude = lng;
            return View();
        }

        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadCustPrefix()
        {
            HTML html = new HTML();
            string SQL = @" select case when (isnull(PrefixYear, 0)) <> 0 then Prefix + convert(varchar, format(CurrentNumber+1, FormatMask))
                            else Prefix + convert(varchar, format(StartNumber, FormatMask)) end Display, PrefixID from dbo.TblPrefix 
                            where Module = 'Customer' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CurrentNumber desc";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustomerQuoteCID(string CustomerType, string FormType)
        {
            HTML html = new HTML();

            string SQL = @"
				DECLARE 
					@Prefix NVARCHAR(50),
					@Module NVARCHAR(50);

				SELECT 
					@Prefix = CONCAT('Q-', @CustomerType, '-{000}'),
					@Module = @ModuleParam
				FROM dbo.TblCustomerType
				WHERE CustomerType = @CustomerType;

				EXEC [SP_SHOW_PREFIX] 
					@Prefix, 
					@Module, 
					@SystemID, 
					@WarehouseID, 
					@DomainID;
			";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "@CustomerType", CustomerType },
                { "@ModuleParam", "Customer Quote" },
                { "@SystemID", html.SystemID },
                { "@WarehouseID", html.WarehouseID },
                { "@DomainID", html.DomainID }
            };

            return Json(
                new { data = html.JsonData(SQL, false, para) },
                JsonRequestBehavior.AllowGet
            );
        }

        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadCustTypePrefix(string CustomerType, bool ToVAT = false, string ClassName = "", string PrefixType = "")
        {
            HTML html = new HTML();
            string sql = @"declare @Prefix nvarchar(50), @Module nvarchar(50), @StartNumber int, @PrefixType nvarchar(50) = '" + PrefixType + @"';
select @Prefix = case when @PrefixType = 'VAT' then PrefixVAT when @PrefixType = 'CM' then PrefixCM else Prefix end, 
@Module = 'Customer', @StartNumber = case when @PrefixType = 'VAT' then isnull(StartNumberPrefixVAT, 1) when @PrefixType = 'CM' then isnull(StartNumberPrefixCM, 1) else isnull(StartNumber,1) end
from dbo.TblCustomerType where Status='Enable' and CustomerType = '" + CustomerType + @"';
exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ",'" + html.DomainID + "'," + @"@StartNumber;
select @Prefix";
            string con = "";
            string InvVAT = "";
            string CustomerGroup = "";
            string CustBlock = "";
            string newcusttype = "";
            if (ToVAT)
            {
                string sqlinsert = @"select CustomerType, DistrictCode, Prefix, StartNumber, Memo, CreateBy, CreateDate, OrderBy, [Status], SystemID, WarehouseID from dbo.TblCustomerType where Status='Enable' and CustomerType = '" + CustomerType + "'";
                DataRow dr = html.LoadData(sqlinsert).Tables[0].Rows[0];
                sqlinsert = @"if(not exists (select CustomerType from TblCustomerType where CustomerType = N'" + dr["CustomerType"] + @"' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + @"))
begin
insert into dbo.TblCustomerType (CustomerType, DistrictCode, Prefix, StartNumber, Memo, CreateBy, CreateDate, OrderBy, [Status], SystemID, WarehouseID)
values(N'" + dr["CustomerType"] + "',N'" + dr["DistrictCode"] + "',N'" + dr["Prefix"] + "',N'" + dr["StartNumber"] + "',N'" + dr["Memo"] + "','" + dr["CreateBy"] + "','" + dr["CreateDate"] + "'," + dr["OrderBy"] + ",'" + dr["Status"] + "'," + dr["SystemID"] + "," + dr["WarehouseID"] + ") end";
                html.SaveChangeData(sqlinsert, false, null, false, false, html.GetDMCConnectionString());

                con = html.GetDMCConnectionString();
                string sqlcusttype = "select CustomerType from dbo.TblCustomerType";
                DataSet ds = html.LoadData(sqlcusttype, false, null, con);
                object[] objs = new object[] { };
                var objList = objs.ToList();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr2 in ds.Tables[0].Rows)
                    {
                        string custtype = dr2[0].ToString();
                        sql = @"declare @Prefix nvarchar(50), @Module nvarchar(50), @StartNumber int;
select @Prefix = Prefix, @Module = 'Customer', @StartNumber = isnull(StartNumber,1) from dbo.TblCustomerType where Status='Enable' and CustomerType = '" + custtype + @"';
exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + @", '" + html.DomainID + "',@StartNumber;";
                        DataSet dss = html.LoadData(sql, false, null, con);
                        if (dss.Tables[0].Rows.Count > 0)
                        {
                            DataRow dtt = dss.Tables[0].Rows[0];
                            objList.Add(new
                            {
                                PrefixNumber = dtt["PrefixNumber"],
                                Prefix = dtt["Prefix"],
                                Module = dtt["Module"],
                            });
                        }
                    }
                }
                newcusttype = objList.ToList().ToJsonString();

                string sqlclass = @"if(not exists (select ClassID from dbo.TblClass where ClassName = '" + ClassName + @"'))
begin
	SET IDENTITY_INSERT dbo.TblClass ON
	insert into dbo.TblClass (ClassID, ClassName, PrefixInvoice, StartNumberInvoice,
			PrefixSO, StartNumberSO, Memo, CreateBy, CreateDate, PrefixCommercial, StartNumberCM, StatusID, SystemID, WarehouseID)
	select	ClassID, ClassName, PrefixInvoice, 1 StartNubmerInvoice,
			0 PrefixSO, 0 StartNumberSO, Memo, CreateBy, CreateDate, PrefixCommercial, 1 StartNumberCM, StatusID, SystemID, WarehouseID
	from NSM_BILLING_DB.dbo.TblClass where ClassName = '" + ClassName + "' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + @"
	SET IDENTITY_INSERT dbo.TblClass OFF
end";
                html.SaveChangeData(sqlclass, false, null, false, false, html.GetDMCConnectionString());

                string sqlinfo = @"declare @Prefix nvarchar(50), @ClassID int, @StartNumber int;
select @Prefix = PrefixInvoice, @ClassID = ClassID from dbo.TblClass where ClassName = '" + ClassName + "' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + @";
exec SP_GET_PREFIX_BY_CLASS @ClassID,@Prefix;";
                InvVAT = html.JsonData(sqlinfo, false, null, false, con);
                sqlinfo = "select CustomerGroupID, CustomerGroup from dbo.TblCustomerGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
                CustomerGroup = html.JsonData(sqlinfo, false, null, false, con);
                sqlinfo = "select CustomerTypeID, CustomerType from dbo.TblCustomerType where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
                CustBlock = html.JsonData(sqlinfo, false, null, false, con);
            }
            return Json(new { data = html.JsonData(sql, false, null, false, con), InvVAT = InvVAT, CustomerGroup = CustomerGroup, CustBlock = CustBlock, NewCustType = newcusttype }, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadChartOfAccount()
        {
            HTML html = new HTML();
            string sqlacc = "select ca.ChartOfAccountID, (ca.AccountNo + ' - ' + ca.AccountName + ' (' + ca.AccountType + ')') AccountName ";
            sqlacc += "from dbo.TblChartOfAccount ca ";
            sqlacc += "where ca.SystemID = @SystemID and ca.WarehouseID = @WarehouseID";
            sqlacc += ";select FormName, ChartOfAccountID from dbo.TblRemember where FormName='CustomerRegisterForm' and SystemID = @SystemID and WarehouseID = @WarehouseID";
            Dictionary<string, string> Para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            return Json(new { data = html.JsonData(sqlacc, false, Para, true) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustomerGroup()
        {
            HTML html = new HTML();
            string sql = "select CustomerGroupID, CustomerGroup from dbo.TblCustomerGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            SqlResult sqlresult = new SqlResult();
            string data = html.JsonData(sql);
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustInfo(string CID)
        {
            HTML html = new HTML();
            string sql = "select (e.Salutation + e.FullName) Employee, c.EmployeeID, c.CustomerID, ty.CustomerType, c.CustomerTypeID, c.CustomerName, c.CustomerCode, c.CustomerGender, c.CustomerGender CustomerGenderID, c.CustomerAddress, ";
            sql += "c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity, ";
            sql += "c.CustomerMobile, c.CustomerEmail, c.CustomerNationality, c.CustomerIdentification, ";
            sql += "c.CustomerContactName, c.CustomerContactMobileNumber, c.CustomerContactEmail, c.CustomerGroupID, c.CustomerGroup ";
            sql += "from dbo.TblCustomer c ";
            sql += "inner join dbo.TblCustomerType ty on ty.CustomerTypeID = c.CustomerTypeID ";
            sql += "inner join dbo.TblLeads e on e.LeadID = c.EmployeeID ";
            sql += "where CustomerCode = " + CID + "; ";
            string CustInfo = html.JsonData(sql);
            dynamic CustID = new JavaScriptSerializer().DeserializeObject(ViewBag.CustInfo);
            string CustomerID = CustID[0]["CustomerID"].ToString();
            sql = @"select distinct cb.CustomerBillInformationID, cb.CompanyName, cb.CompanyNameKH, cb.CompanyDeskPhone, cb.CompanyMobile, 
cb.CompanyEmail, cb.VATNumber, cb.CompanyAddress, cb.CompanyCommune, ci.PID,
cb.CompanyDistrict, cb.CompanyProvinceCity, cb.CompanyContactName,
cb.CompanyContactPosition, cb.CompanyContactMobileNumber, cb.ATTName,
cb.BillToAddress, cb.BillToCommune, cb.BillToDistrict, cb.BillToProvinceCity,
cb.InstallAddress, cb.InstallCommune, cb.InstallDistrict, cb.InstallProvinceCity, cb.IndustryTypeID, it.IndustryType, cb.GoogleMap_FullAddress, cb.GoogleMap_Latitude, cb.GoogleMap_Longitude, cb.InvoiceType, cb.ClassNameID, cb.ClassName, cb.LocationID, cb.Warehouse
from dbo.TblCustomerBillInformation cb
inner join dbo.TblIndustryType it on it.IndustryTypeID = cb.IndustryTypeID
inner join dbo.TblCustomerInvoice ci on ci.CustomerBillInformationID = cb.CustomerBillInformationID
where cb.CustomerID = " + CustomerID;
            string CustDetail = html.JsonData(sql);

            return Json(new { CustInfo = CustInfo, CustDetail = CustDetail }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadUserApprovalForDMCTAX()
        {
            HTML html = new HTML();
            string sql = @"select * from (select ul.UserLoginID, ul.FullName, ul.TelegramUserName, 3 OrderBy
from dbo.TblUserLogin ul
inner join dbo.TblSetUserAccessSystemAndWarehouse ua on ua.UserLoginID = ul.UserLoginID
where ua.SystemID = " + html.SystemID + @" and ul.TelegramUserName <> '') as aa
order by aa.OrderBy, case when aa.FullName = 'Ms. Meng Daneth' then '2' else aa.FullName end asc";
            DataSet ds = html.LoadData(sql);
            return Json(new { Data = ds.Tables[0].Rows.ToJsonString() }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<JsonResult> RequestApproveInvToDMCTAX(string CustomerCode, string CustomerName, int CustomerID, string InvoiceNo, string InvoiceDate, string RequestApprovalTgUser, int RequestApprovalNameID, string RequestApprovalName, string RequestCheckByTgUser = "", string RequestCheckByNameID = "", string RequestCheckByName = "", string TypeOfInvoice = "")
        {
            HTML html = new HTML();
            string sql = @"update dbo.TblCustomerInvoice set RequestedByUserIDToDMCTAX = @RequestedByUserIDToDMCTAX,
RequestedByUserToDMCTAX = @RequestedByUserToDMCTAX, RequestApprovalToDMCTAX = @RequestApprovalToDMCTAX,
RequestApprovalToDMCTAXID = @RequestApprovalToDMCTAXID, RequestApprovalToDMCTAXTgUser = @RequestApprovalToDMCTAXTgUser,
RequestToDMCTAXDate = @RequestToDMCTAXDate, ApproveRequestToDMCTAXStatus = 'Pending'
where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo;
select CreateBy, CreateDate, TypeOfInvoice, ClassNameID from dbo.TblCustomerInvoice
where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo;
select l.IspLinkName from dbo.TblCustomerBillingInvoice bi
left join dbo.TblIspLink l on l.IspLinkID = bi.IspLinkID
where bi.CustomerID = @CustomerID and bi.InvoiceNo = @InvoiceNo;";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"RequestedByUserIDToDMCTAX", html.UserID},
                {"RequestedByUserToDMCTAX", html.UserFullName},
                {"RequestApprovalToDMCTAX", RequestApprovalName},
                {"RequestApprovalToDMCTAXID", RequestApprovalNameID.ToString()},
                {"RequestApprovalToDMCTAXTgUser", RequestApprovalTgUser},
                {"RequestToDMCTAXDate", html.GetCurrentDate()},
                {"CustomerID", CustomerID.ToString()},
                {"InvoiceNo", InvoiceNo},
            };

            SqlResult sqlResult = new SqlResult();
            int invoiceStatusId = 0;
            sqlResult = html.SaveChangeData(sql, false, para, true, true);
            string CreateBy = sqlResult.Data.Tables[0].Rows[0]["CreateBy"].ToString(), CreateDate = DateTime.Parse(sqlResult.Data.Tables[0].Rows[0]["CreateDate"].ToString()).ToString("dd-MMM-yyyy hh:mm:ss tt");
            string IspLinkName = sqlResult.Data.Tables[1].Rows.Count > 0 ? sqlResult.Data.Tables[1].Rows[0]["IspLinkName"].ToString() : "Non";
            TypeOfInvoice = sqlResult.Data.Tables[0].Rows[0]["TypeOfInvoice"].ToString();
            string ClassID = sqlResult.Data.Tables[0].Rows[0]["ClassNameID"].ToString();

            sqlResult = await html.InvoiceToPDFandUpload(CustomerName, CustomerCode, CustomerID, InvoiceNo, InvoiceDate, 0, 11, int.Parse(html.SystemID.ToString()), int.Parse(html.WarehouseID.ToString()), false, "", "/ISP_Invoice", true);
            if (sqlResult.Result == ResultType.Successful)
            {
                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Approve ✅", callbackData: "ApproveInvDMC~"+CustomerID+"~"+InvoiceNo+"~"+RequestApprovalTgUser),
                        InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: "DisapproveInvDMC~"+CustomerID+"~"+InvoiceNo+"~"+RequestApprovalTgUser)
                    }
                });

                NotificationTelegram notificationTelegram = new NotificationTelegram();
                string invoiceType = "";
                string sql2 = @"select InvoiceType from dbo.TblCustomerInvoice where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo";
                DataSet ds = html.LoadData(sql2, false, para);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        invoiceType = ds.Tables[0].Rows[0]["InvoiceType"].ToString();
                        invoiceType = invoiceType == "Renew Invoice" ? invoiceType : "";
                    }
                }

                string msg;
                string ApproveRequestToDMCTAXTGMID;
                if (RequestCheckByNameID != "0" && RequestCheckByNameID != "")
                {
                    if (TypeOfInvoice == "None VAT" && ClassID == "6")
                    {
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData(text: "Check 📝", callbackData: "CheckInv~"+CustomerID+"~"+InvoiceNo+"~"+RequestCheckByTgUser),
                                InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: "DisapproveInv~"+CustomerID+"~"+InvoiceNo+"~"+RequestCheckByTgUser)
                            }
                        });

                        msg = RequestCheckByTgUser + "<br><br>Dear " + RequestCheckByName + ",<br><br>";
                        msg += @"Now, you have invoice need to check.";
                        //msg += "<br><br>Created by: " + CreateBy + "<br>Created Date: " + CreateDate;
                        if (!string.IsNullOrWhiteSpace(IspLinkName))
                            msg += "<br><br><b>🌐 ISP LINK: </b>" + IspLinkName;
                        msg += "<br><br>Requested by: " + html.UserFullName;
                        msg += "<br><br>Thank you and best regards,<br><br>";
                        msg += "NSM ISP BILLING SYSTEM AUTO SEND Request approve Invoice. #" + InvoiceNo;
                        sqlResult = await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 23002, inlineKeyboard, 0, "", sqlResult.OutPutFile, msg, false);
                        ApproveRequestToDMCTAXTGMID = sqlResult.Value;
                        invoiceStatusId = 101;
                    }
                    else
                    {
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData(text: "Check 📝", callbackData: "CheckedInvDMC~"+CustomerID+"~"+InvoiceNo+"~"+RequestCheckByTgUser),
                                InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: "DisapproveCheckedInvDMC~"+CustomerID+"~"+InvoiceNo+"~"+RequestCheckByTgUser)
                            }
                        });
                        msg = RequestCheckByTgUser + "<br><br>Dear " + RequestCheckByName + ",<br><br>";
                        msg += @"Now, you have invoice for DMC and TAX System need to check.";
                        if (!string.IsNullOrWhiteSpace(IspLinkName))
                            msg += "<br><br><b>🌐 ISP LINK: </b>" + IspLinkName;
                        msg += "<br><br>Created by: " + CreateBy + "<br>Created Date: " + CreateDate;
                        msg += "<br>Requested by: " + html.UserFullName;
                        msg += "<br><br>Thank you and best regards,<br><br>";
                        msg += "NSM ISP BILLING SYSTEM AUTO SEND Request approve Invoice to DMC & TAX System. #" + InvoiceNo;
                        ApproveRequestToDMCTAXTGMID = "-1545132903/7167";
                        sqlResult = await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 7167, inlineKeyboard, 0, "", sqlResult.OutPutFile, msg, false);
                    }
                }
                else
                {
                    msg = RequestApprovalTgUser + "<br><br>Dear " + RequestApprovalName + ",";
                    //msg += @"<br><br>Please check attachment file for detail and APPROVE or DISAPPROVE.";
                    msg += "<br><br>This is invoice for DMC and TAX System need to approve.";
                    if (invoiceType != "")
                        msg += $"<br><br>Type: <b>{invoiceType}</b>";
                    if (!string.IsNullOrWhiteSpace(IspLinkName))
                        msg += "<br><br><b>🌐 ISP LINK: </b>" + IspLinkName;
                    msg += "<br><br>Created by: " + CreateBy + "<br>Created Date: " + CreateDate;
                    //msg += "<br><br>Requested by: " + html.UserFullName;
                    //msg += "<br><br>Thank you and best regards,<br><br>";
                    msg += "<br><br>NSM ISP BILLING SYSTEM AUTO SEND Request approve Invoice to DMC & TAX System. #" + InvoiceNo;
                    //ApproveRequestToDMCTAXTGMID = "-1002042086477";
                    sqlResult = await notificationTelegram.NotificationTelegramBot("", -1002042086477, "", sqlResult.OutPutFile, msg, true, true, 0, 0, "", inlineKeyboard, false);
                    ApproveRequestToDMCTAXTGMID = sqlResult.Value;
                }
                //sql = "update dbo.TblCustomerInvoice set InvoiceTelegramMessageID = '" + sqlResult.Value + "' where CustomerID = " + CustomerID + " and InvoiceNo='" + InvoiceNo + "'";
                //sqlResult = html.SaveChangeData(sql, false, false);

                sql = @"declare @ActivateCustomerID int, @ConnectionID nvarchar(50), @SID nvarchar(50);
select @ActivateCustomerID = ActivateCustomerID, @ConnectionID = ConnectionID, @SID = [SID] from TblCustomerInvoice where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo;

update dbo.TblCustomerInvoice set ApproveRequestToDMCTAXTGMID = @ApproveRequestToDMCTAXTGMID, InvoiceStatusID = @InvoiceStatusID,
ApproveRequestToDMCTAXMsg = @ApproveRequestToDMCTAXMsg, InvoiceTelegramMessageID = @InvoiceTelegramMessageID
where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo;
update TblActivateCustomer set InvoiceNo = @InvoiceNo, ConnectionID=@ConnectionID, [SID]=@SID where CustomerID = @CustomerID and ActivateCustomerID=@ActivateCustomerID";
                para = new Dictionary<string, string>
                {
                    {"ApproveRequestToDMCTAXTGMID", ApproveRequestToDMCTAXTGMID },
                    {"ApproveRequestToDMCTAXMsg", msg },
                    {"CustomerID", CustomerID.ToString()},
                    {"InvoiceStatusID", invoiceStatusId.ToString()},
                    {"InvoiceNo", InvoiceNo},
                    {"InvoiceTelegramMessageID", sqlResult.Value},
                };
                html.SaveChangeData(sql, false, para);
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Test()
        {
            return View();
        }

        public string GetPreviewInvoice(string Data)
        {
            HTML html = new HTML();

            string sql = @"declare @Data xml, @olddelim nvarchar(32) = N'&#013;&#010;', @newdelim nchar(1) = NCHAR(9999);";
            sql += "select @Data = N'" + Data + "';";
            sql += @"declare @PaymentDueDate datetime, @IsIncludedVAT bit, @ExchangeRate decimal(18,2), @InvoiceDate datetime,
@TermCondition nvarchar(max), @InvoiceTmpID int, @ServiceName nvarchar(50)

select @PaymentDueDate = i.N.value('PaymentDueDate[1]', 'datetime'),
@IsIncludedVAT = i.N.value('IsIncludedVAT[1]', 'bit'),
@InvoiceDate = i.N.value('InvoiceDate[1]', 'datetime'), @InvoiceTmpID = i.N.value('InvoiceTmpID[1]', 'int'),
@ServiceName =i.N.value('ServiceName[1]', 'nvarchar(50)')
FROM @Data.nodes('/Data/CustomerInvoice') i(N)

select top 1 @ExchangeRate = ExchangeRate from dbo.TblExchangeRate er
	inner join dbo.TblCurrency c on c.CurrencyID = er.CurrencyID
	where c.CurrencyName='Riel' and er.SystemID = @SystemID and er.WarehouseID = @WarehouseID and er.DomainID like '%'+@DomainID+'%'
	and format(DateEntry,'yyyyMMdd') <= format(@InvoiceDate, 'yyyyMMdd')
	order by format(DateEntry,'yyyyMMdd') desc

select CompanyName, CompanyAddress [Address], CompanyNameKH, PhoneKH,​​ AddressKH,
	Phone, Email, Website, Logo, Facebook, VATNumber, @TermCondition TermCondition, InvoiceTemplateID,
	QuoteTermConditionVAT, QuoteTermConditionNonVAT
	from dbo.TblCompany where SystemID = @SystemID and WarehouseID = @WarehouseID and DomainID like '%'+@DomainID+'%'
	and CompanyID = @InvoiceTmpID

select
i.N.value('CustomerContactName[1]', 'nvarchar(max)') ContactName,
case when lower(i.N.value('CustomerName[1]', 'nvarchar(max)')) not like '%[a-z]%' and i.N.value('CustomerGender[1]', 'nvarchar(50)') = 'Male' then N'លោក ' + replace(i.N.value('CustomerName[1]', 'nvarchar(max)'), N'លោក','')
when lower(i.N.value('CustomerName[1]', 'nvarchar(max)')) not like '%[a-z]%' and i.N.value('CustomerGender[1]', 'nvarchar(50)') = 'Female' then N'អ្នកស្រី ' + replace(replace(i.N.value('CustomerName[1]', 'nvarchar(max)'), N'លោកស្រី​',''), N'អ្នកស្រី','')
when i.N.value('CustomerGender[1]', 'nvarchar(50)') = 'Male' then 'Mr. ' + replace(replace(i.N.value('CustomerName[1]', 'nvarchar(max)'), 'Mr. ', ''), 'Mr.','')
when i.N.value('CustomerGender[1]', 'nvarchar(50)') = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(i.N.value('CustomerName[1]', 'nvarchar(max)'), 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
else i.N.value('CustomerName[1]', 'nvarchar(max)') end CustomerName,
i.N.value('CustomerContactMobileNumber[1]', 'nvarchar(max)') Phone,
i.N.value('BillToAddress[1]', 'nvarchar(max)') +' ,' + 
i.N.value('BillToCommune[1]', 'nvarchar(max)') +', ' + 
i.N.value('BillToDistrict[1]', 'nvarchar(max)') + ', ' +
i.N.value('BillToProvinceCity[1]', 'nvarchar(max)') [Address],
'' InvoiceNo,
i.N.value('InvoiceDate[1]', 'datetime') InvoiceDate,
i.N.value('Employee[1]', 'nvarchar(max)') SaleName,
l.Phone SalePhone, l.LeadCode, i.N.value('CustomerEmail[1]', 'nvarchar(50)') Email,
i.N.value('CustomerCode[1]', 'nvarchar(max)') CustomerCode,
i.N.value('CustomerID[1]', 'nvarchar(max)') CustomerID,
@PaymentDueDate PaymentDueDate, @IsIncludedVAT IsIncludedVAT,
i.N.value('CompanyName[1]', 'nvarchar(max)') CompanyName,
i.N.value('CompanyNameKH[1]', 'nvarchar(max)') CompanyNameKH,
i.N.value('VATNumber[1]', 'nvarchar(50)') VATNumber,
i.N.value('CompanyDeskPhone[1]', 'nvarchar(50)') CompanyDeskPhone,
i.N.value('CustomerContactEmail[1]', 'nvarchar(50)') CustomerEmail,
i.N.value('ATTName[1]', 'nvarchar(max)') ATTName,
i.N.value('QuotationNo[1]', 'nvarchar(50)') QuotationNo,
i.N.value('InvoiceTmpID[1]', 'nvarchar(50)') InvoiceTmpID,
'Official Invoice' [Type],
i.N.value('invTmp[1]', 'nvarchar(50)') invTmp,
@ExchangeRate ExchangeRate,
i.N.value('TypeOfInvoice[1]', 'nvarchar(50)') TypeOfInvoice,
i.N.value('TermCondition[1]', 'nvarchar(max)') TermCondition,
'' CreditNoteNo
FROM @Data.nodes('/Data/CustomerInfo') i(N)
inner join dbo.TblLeads l on l.LeadID = i.N.value('EmployeeID[1]', 'int')

select @TermCondition = i.N.value('TermCondition[1]', 'nvarchar(max)')
FROM @Data.nodes('/Data/CustomerInfo') i(N)

select
'Service Plan: '+ i.N.value('ServiceName[1]', 'nvarchar(50)') + '<br>Package Name: ' +
i.N.value('ItemDescription[1]', 'nvarchar(max)') + 
case when isnull(pd.Feature, '') <> '' then
'<br/><br/><b>Feature:</b><br/>' + 
(SELECT 
STUFF((SELECT ' ' + ('<div class=""row""><div class=""col-auto ml-2"">'+
'<svg xmlns=""http://www.w3.org/2000/svg"" width=""16"" height=""16"" fill=""currentColor"" class=""bi bi-caret-right-fill"" viewBox=""0 0 16 16"">' +
'<path d=""m12.14 8.753-5.482 4.796c-.646.566-1.658.106-1.658-.753V3.204a1 1 0 0 1 1.659-.753l5.48 4.796a1 1 0 0 1 0 1.506z""/>' +
'</svg></div><div class=""col""><p class=""text-justify"">'
+ value + '</p></div></div>') 
FROM STRING_SPLIT(REPLACE(pd.Feature, @olddelim, @newdelim), @newdelim)
FOR XML PATH('')), 1, 1, '')) else '' end +

'<div class=""row""><div class=""col-auto""><label class=""text-primary text-nowrap"">Period ' + i.N.value('UM[1]', 'nvarchar(50)') + ' From ' +
FORMAT(i.N.value('StartDate[1]', 'datetime'), 'dd-MMM-yyyy') + ' To ' + 
case when convert(varchar, i.N.value('EndDate[1]', 'datetime'), 111) <> convert(varchar, i.N.value('NextInvoiceDate[1]', 'datetime'), 111) then
format(DATEAdd(d,-1, i.N.value('EndDate[1]', 'datetime')), 'dd-MMM-yyyy')
else
FORMAT(DATEAdd(d,-1, DATEADD(MONTH, i.N.value('Duration[1]', 'int'), i.N.value('StartDate[1]', 'datetime'))), 'dd-MMM-yyyy') 
end
+
'</label></div></div>'
+
case when DATEDIFF(M, convert(varchar, i.N.value('StartDate[1]', 'datetime'), 111) ,convert(varchar, i.N.value('EndDate[1]', 'datetime'), 111)) >= i.N.value('Duration[1]', 'int') then
case when convert(varchar, i.N.value('FreeDuration[1]', 'int')) <> 0 then
'<div class=""row""><div class=""col-auto""><label class=""text-danger text-nowrap"">FREE ' + convert(varchar, i.N.value('FreeDuration[1]', 'int')) + ' ' + 
' Period From ' + FORMAT(DATEADD(MONTH, i.N.value('Duration[1]', 'int'), i.N.value('StartDate[1]', 'datetime')), 'dd-MMM-yyyy') + ' To ' +
FORMAT(DATEAdd(d,-1, DATEADD(MONTH, (i.N.value('Duration[1]', 'int') + i.N.value('FreeDuration[1]', 'int')), i.N.value('StartDate[1]', 'datetime'))), 'dd-MMM-yyyy')  +
'</label></div></div>' else '' end 
else '' end
[Description],
i.N.value('UM[1]', 'nvarchar(50)') UM,
i.N.value('QTY[1]', 'nvarchar(50)') QTY,
i.N.value('UnitPrice[1]', 'decimal(18,2)') UnitPrice,
i.N.value('DiscountP[1]', 'decimal(18,2)') DiscountP,
i.N.value('DiscountUSD[1]', 'decimal(18,2)') DiscountUSD,
i.N.value('Amount[1]', 'decimal(18,2)') Amount,
i.N.value('PackageID[1]', 'int') PackageID,
i.N.value('PackageDetailID[1]', 'int') PackageDetailID,
i.N.value('ServiceID[1]', 'int') ServiceID,
i.N.value('OrderBy[1]', 'int') OrderBy
FROM @Data.nodes('/Data/CustomerInvoice') i(N)
left join dbo.TblPackageDetail pd on pd.PackageDetailID = i.N.value('PackageDetailID[1]', 'int')
union all
select '<b>Deposit</b>' [Description],
i.N.value('UM[1]', 'nvarchar(50)') UM, 
i.N.value('QTY[1]', 'int') QTY,
i.N.value('DepositFee[1]', 'decimal(18,2)') UnitPrice,
i.N.value('DiscountP[1]', 'decimal(18,2)') DiscountP, 
i.N.value('DiscountUSD[1]', 'decimal(18,2)') DiscountUSD, 
i.N.value('Amount[1]', 'decimal(18,2)') Amount,
0 PackageID, 0 PackageDetailID, 0 ServiceID, i.N.value('OrderBy[1]', 'int') OrderBy
FROM @Data.nodes('/Data/CustomerOtherCharge') i(N)
where isnull(i.N.value('DepositFee[1]', 'int'),0) <> 0
union all
select i.N.value('ItemDescription[1]', 'nvarchar(max)') [Description],
i.N.value('UM[1]', 'nvarchar(50)') UM, 
i.N.value('QTY[1]', 'int') QTY,
i.N.value('UnitPrice[1]', 'decimal(18,2)') UnitPrice,
i.N.value('DiscountP[1]', 'decimal(18,2)') DiscountP, 
i.N.value('DiscountUSD[1]', 'decimal(18,2)') DiscountUSD, 
i.N.value('Amount[1]', 'decimal(18,2)') Amount,
0 PackageID, 0 PackageDetailID, 0 ServiceID, i.N.value('OrderBy[1]', 'int') OrderBy
FROM @Data.nodes('/Data/CustomerOtherCharge') i(N)
inner join dbo.TblItem im on im.ItemID = i.N.value('ItemID[1]', 'int')
union all
select i.N.value('ItemDescription[1]', 'nvarchar(max)') [Description],
i.N.value('UM[1]', 'nvarchar(50)') UM, 
i.N.value('QTY[1]', 'int') QTY,
i.N.value('UnitPrice[1]', 'decimal(18,2)') UnitPrice,
i.N.value('DiscountP[1]', 'decimal(18,2)') DiscountP, 
i.N.value('DiscountUSD[1]', 'decimal(18,2)') DiscountUSD, 
i.N.value('Amount[1]', 'decimal(18,2)') Amount,
0 PackageID, 0 PackageDetailID, 0 ServiceID, i.N.value('OrderBy[1]', 'int') OrderBy
FROM @Data.nodes('/Data/CustomerOtherCharge') i(N)
inner join dbo.TblOtherCharge ot on ot.OtherChargeID = i.N.value('OtherChargeID[1]', 'int')

	select '' InvoiceNoted
	select @TermCondition TermCondition

	select i.N.value('SubTotal[1]', 'decimal(18,2)') SubTotal,
	i.N.value('VAT[1]', 'decimal(18,2)') VAT,
	i.N.value('Total[1]', 'decimal(18,2)') Total,
	i.N.value('DiscountP[1]', 'decimal(18,2)') DiscountP,
	i.N.value('DiscountUSD[1]', 'decimal(18,2)') DiscountUSD,
	i.N.value('GrandTotal[1]', 'decimal(18,2)') GrandTotal,
	0 Paid,
	i.N.value('GrandTotal[1]', 'decimal(18,2)') Unpaid,
	'" + html.UserFullName + @"' CreateBy, '" + DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy") + @"' CreateDate
	FROM @Data.nodes('/Data/CustomerInvoiceDetail') i(N)

	select @ServiceName ServiceName
	select 0 DepositFee

	declare @Signature nvarchar(max)
	select @Signature = [Signature] from dbo.TblUserLogin where UserLoginID = " + html.UserID + @"

	select '" + html.UserFullName + "' CreateBy, @Signature [Signature]";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID },
            };
            DataSet ds = html.LoadData(sql, false, para);
            return html.DrawInvoice(ds, false, 0, 0, false);
        }

        public string GetAgencyInfo(int CustomerID, string InvoiceNo = "", bool IsEditInvoice = false, bool IsRenew = false)
        {
            HTML html = new HTML();
            string sql = "select ac.AgencyCommissionID, ac.FullName AgencyName, ac.HP AgencyPhone, ac.CommissionType AgencyCommissionType,\r\nac.CommissionAmountUSD, ac.CommissionAmountP, ac.[Address] AgencyAddress, ac.BankName AgencyBankName,\r\nac.AccountName AgencyAccountName, ac.AccountNumber AgencyAccountNumber, ac.Memo\r\nfrom dbo.TblAgencyCommission ac ";

            if (IsEditInvoice || (IsRenew && InvoiceNo != ""))
            {
                sql += @" inner join dbo.TblAgencyInvoice ai on ai.AgencyCommissionID = ac.AgencyCommissionID and ai.CustomerID = ac.CustomerID";
                sql += @" where ac.StatusID = 3 and ac.CustomerID = @CustomerID and ai.InvoiceNo = @InvoiceNo";
                if (IsRenew)
                {
                    sql = @"select ac.AgencyCommissionID, ac.FullName AgencyName, ac.HP AgencyPhone,
ac.CommissionAmountUSD, ac.CommissionAmountP, ac.[Address] AgencyAddress, ac.BankName AgencyBankName,
ac.AccountName AgencyAccountName, ac.AccountNumber AgencyAccountNumber, ac.Memo
,(select STUFF((select ',' + value
from dbo.TblAgencyInvoice aii
CROSS APPLY STRING_SPLIT(isnull(aii.CommissionType,''), ',')
where aii.AgencyCommissionID = ac.AgencyCommissionID and aii.InvoiceNo = ai.InvoiceNo
and aii.CustomerID= ac.CustomerID and value <> 1 FOR XML PATH('')), 1, 1, '')) AgencyCommissionType
from dbo.TblAgencyCommission ac
inner join dbo.TblAgencyInvoice ai on ai.AgencyCommissionID = ac.AgencyCommissionID
CROSS APPLY STRING_SPLIT(isnull(ai.CommissionType,''), ',')
where ac.StatusID = 3 and value <> 1 and ac.CustomerID = @CustomerID and ai.InvoiceNo = @InvoiceNo";
                }
            }
            else
                sql += " where ac.StatusID = 3 and ac.CustomerID = @CustomerID";

            Dictionary<string, string> para = new Dictionary<string, string> {
                { "CustomerID", CustomerID.ToString() },
                { "InvoiceNo", InvoiceNo }
            };
            return html.JsonData(sql, false, para);
        }
        [HttpPost]
        public ActionResult RemoveAgency(int AgencyCommissionID)
        {
            HTML html = new HTML();
            string sql = "update TblAgencyCommission set StatusID = 4 where AgencyCommissionID = @AgencyCommissionID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "AgencyCommissionID", AgencyCommissionID.ToString() }
            };
            SqlResult rst = html.SaveChangeData(sql, false, para);
            return Json(new { Result = rst.Result, Messsage = rst.Message }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> ApprovalInvCISP(int CustomerID, string InvoiceNo, int StatusID, string Username)
        {
            HTML html = new HTML();
            string sql = @"select top 1 ci.*, l.IspLinkName from dbo.TblCustomerInvoice ci
				left join dbo.TblIspLink l on l.IspLinkID = (select top 1 IspLinkID from dbo.TblCustomerBillingInvoice where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo)
				where ci.InvoiceNo = @InvoiceNo and ci.CustomerID = @CustomerID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "CustomerID", CustomerID.ToString() },
                { "InvoiceNo", InvoiceNo }
            };
            DataSet ds = html.LoadData(sql, false, para);
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return Json(new { Result = ResultType.Failed, Message = "Invoice not found." }, JsonRequestBehavior.AllowGet);
            string messageId = ds.Tables[0].Rows[0]["ApproveRequestToDMCTAXTGMID"].ToString();
            string telegramMessage = ds.Tables[0].Rows[0]["ApproveRequestToDMCTAXMsg"].ToString();
            string approverName = ds.Tables[0].Rows[0]["RequestApprovalToDMCTAX"].ToString();
            string approverUsername = ds.Tables[0].Rows[0]["RequestApprovalToDMCTAXTgUser"].ToString();
            string requestBy = ds.Tables[0].Rows[0]["RequestedByUserToDMCTAX"].ToString();
            string InvoiceStatusID = ds.Tables[0].Rows[0]["InvoiceStatusID"].ToString();
            string invoiceType = ds.Tables[0].Rows[0]["InvoiceType"].ToString();
            string CustomerName = ds.Tables[0].Rows[0]["CustomerName"].ToString();
            string CustomerCode = ds.Tables[0].Rows[0]["CustomerCode"].ToString();
            string InvoiceDate = ds.Tables[0].Rows[0]["InvoiceDate"].ToString();
            string IspLinkName = ds.Tables[0].Rows[0]["IspLinkName"].ToString();

            SqlResult result = new SqlResult();
            result = await html.InvoiceToPDFandUpload(CustomerName, CustomerCode, CustomerID, InvoiceNo, InvoiceDate, 0, 11, int.Parse(html.SystemID.ToString()), int.Parse(html.WarehouseID.ToString()), false, "", "/ISP_Invoice", true);
            if (result.Result == ResultType.Successful)
            {
                NotificationTelegram notificationTelegram = new NotificationTelegram();
                string userAction = Username;
                InlineKeyboardMarkup inlineKeyboard = null;

                string strStatus;
                if (StatusID == 92)
                {
                    strStatus = "check ✍️";
                    inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Approve ✅", callbackData: "ApproveInv~"+CustomerID+"~"+InvoiceNo),
                            InlineKeyboardButton.WithCallbackData(text: "Disapprove ❌", callbackData: "DisapproveInv~"+CustomerID+"~"+InvoiceNo)
                        }
                    });
                }
                else
                {
                    strStatus = StatusID == 90 ? "approve ✅" : "disapprove ❌";
                }

                string msg = $"<br><br>Dear Team,<br><br>";
                msg += $"Now, Invoice is {strStatus} by @{userAction}.";
                if (StatusID == 91)
                    msg += "<br>Please revise this invoice.<br>";
                msg += "<br>Thank you and best regards,<br><br>";
                msg += "NSM ISP BILLING SYSTEM AUTO SEND Invoice. #" + InvoiceNo;

                if (StatusID == 90 || (StatusID == 91 && InvoiceStatusID == "92"))
                {
                    result = await notificationTelegram.NotificationTelegramBot("", -5009096861, "", result.OutPutFile, msg, false, true, 0, int.Parse(messageId), telegramMessage);
                }
                else
                {
                    await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 23002, null, int.Parse(messageId), telegramMessage, result.OutPutFile, msg);
                }

                if (StatusID == 92)
                {
                    msg = $"{approverUsername}";
                    msg += $"<br><br>Dear {approverName},<br><br>";
                    msg += $"Now, You have Invoice need to approve.";
                    if (!string.IsNullOrWhiteSpace(IspLinkName))
                        msg += "<br><b>🌐 ISP LINK: </b>" + IspLinkName;
                    if (invoiceType == "Renew Invoice")
                        msg += $"<br>Type: {invoiceType}";
                    msg += $"<br>Request by: {requestBy}.";
                    msg += "<br><br>Thank you and best regards,<br><br>";
                    msg += "NSM ISP BILLING SYSTEM AUTO SEND Invoice. #" + InvoiceNo;
                    result = await notificationTelegram.NotificationTelegramBot("", -5009096861, "", result.OutPutFile, msg, false, true, 0, 0, "", inlineKeyboard, false);
                }

                if (System.IO.File.Exists(result.OutPutFile))
                {
                    System.IO.File.Delete(result.OutPutFile);
                }

                if (result.Result == ResultType.Successful)
                {
                    sql = @"update dbo.TblCustomerInvoice set InvoiceStatusID = @InvoiceStatusID, ApproveRequestToDMCTAXTGMID = @MessageID, ApproveRequestToDMCTAXMsg = @Message
					where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo";
                    para.Add("InvoiceStatusID", StatusID == 91 ? "101" : (StatusID == 92) ? "92" : "90");
                    para.Add("MessageID", result.Value);
                    para.Add("Message", msg);
                    result = html.SaveChangeData(sql, false, para);
                }
            }

            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}