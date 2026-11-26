using MVC;
using Newtonsoft.Json;
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
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class SaleOrderController : Controller
    {
        // GET: SaleOrder
        [Authorize(Roles = "Add Sale Order, Edit Sale Order, Delete Sale Order")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            bool ApprovalSaleOrder = HttpContext.GetOwinContext().Request.User.IsInRole("Approval Sale Order");
            if (html.HasPermission("Add Sale Order"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Sale Order"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Sale Order"))
                html.AllowDelete = true;
            html.AddButton = "New Sale Order";
            html.AddURL = "AddCustomer/CreateCustomer/?ISSO=2";
            html.DialogBoxWidth = "Auto";
            html.DialogBoxHeight = 600;
            html.ReportTitle = "List Sale Order";
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "PI Date:",
                DBField = "ci.CreateDate"
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                Label = "PI Status:",
                DBField = "ci.InvoiceStatus",
                SQL_CBO = "select distinct bi.InvoiceStatus from dbo.TblCustomerInvoice bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where bi.[Type]= 'Sale Order Invoice' and isnull(bi.InvoiceStatus,'') <> '' and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "InvoiceStatus",
                Value = "InvoiceStatus",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoiceNo",
                Label = "PI No:",
                DBField = "ci.InvoiceNo",
                SQLAutoComplete = "select distinct top 10 ci.InvoiceNo from dbo.TblCustomerInvoice ci where ci.InvoiceNo like '%PI%' and lower(ci.InvoiceNo) like @SearchText and ci.SystemID = " + html.SystemID + " and ci.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true,
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerName",
                Label = "Customer Name:",
                DBField = "c.CustomerName",
                SQLAutoComplete = "select distinct top 10 c.CustomerName from dbo.TblCustomerInvoice bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where bi.[Type]= 'Sale Order Invoice' and lower(c.CustomerName) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'",
                ISAutoComplete = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode from dbo.TblCustomer c inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID where ci.[Type]= 'Sale Order Invoice' and lower(c.CustomerCode) like @SearchText and ci.SystemID = " + html.SystemID + " and ci.WarehouseID=" + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + "%'",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerMobile"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Phone",
                SQL = "select distinct top 10 REPLACE(c.CustomerMobile, ' ','') CustomerMobile from dbo.TblCustomer c inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID where ci.[Type]= 'Sale Order Invoice' and  REPLACE(c.CustomerMobile, ' ','') like @SearchText and ci.SystemID = " + html.SystemID + " and ci.WarehouseID=" + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + "%'",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerMobile"),
                ISAutoComplete = true
            });
            html.isShowLoadingGrid = false;
            //			string sql = @"select ROW_NUMBER() over (order by c.CustomerID desc) as [No], c.CustomerID, 
            //'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a>' CustomerCode, 
            //'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerName + '</a>' CustomerName, 
            //c.CustomerMobile, format(c.RegisterDate,'dd/MM/yyyy') + '<br>By: '+ c.RegisterBy RegisterDate, c.RegisterBy, p.PackageName, ci.Bandwidth, ci.Duration, ci.FreeDuration, ci.UM,
            //ci.StartDate, ci.EndDate, case when ci.InvoiceStatusID = 90 then '<span class=""badge bg-primary"">' + ci.InvoiceStatus + '</span>' else '<span class=""badge bg-danger"">' + ci.InvoiceStatus + '</span>' end InvoiceStatus, 
            //p.PackageName + '<br>' + convert(varchar,ci.Bandwidth) + ' Mbps ' + ci.UM + ' Free: ' + convert(varchar, ci.FreeDuration)+ ' Month' + '<br><span class=""text-primary"">' + format(ci.StartDate, 'dd/MM/yyyy') + '</span> - <span class=""text-danger"">' + format(ci.EndDate, 'dd/MM/yyyy') + '</span>' ItemDescription,
            //(bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) InstallAddress, ci.InvoiceNo,
            //case when ci.InvoiceStatusID <> 90 then '<a href=""javascript:void(0);"" onclick=""Edit_Tbl_Row('''', '''', '''+ci.InvoiceNo+''', false, '''', '+convert(varchar, ci.CustomerID)+', true,'''','''+ci.[Type]+''');"" class=""btn btn-sm btn-primary"">Approve</a>' else ci.ApproveBy + '<br>' + format(ci.ApproveDate, 'dd-MM-yyyy HH:mm:ss tt') end Approval
            //from dbo.TblCustomer c
            //inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
            //left join dbo.TblPackageDetail pd on pd.PackageDetailID = ci.PackageDetailID
            //left join dbo.TblPackages p on p.PackageID = ci.PackageID
            //inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = ci.CustomerBillInformationID
            //where ci.[Type] = 'Sale Order Invoice' and isnull(ci.ItemDescription,'') <> 'Deposit Fee' and c.SystemID = " + html.SystemID + " and c.WarehouseID =" + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'";
            string sql = @"select ROW_NUMBER() over (order by ci.InvoiceNo desc) as [No], c.CustomerID, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a>' CustomerCode, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerName + '</a><br>' + c.CustomerMobile CustomerName, 
c.CustomerMobile, format(c.RegisterDate,'dd/MM/yyyy') + '<br>By: '+ c.RegisterBy RGDate, c.RegisterBy, c.RegisterDate, ci.CreateDate,
ci.StartDate, ci.EndDate, case when ci.InvoiceStatusID = 90 then '<span class=""badge bg-primary"">' + ci.InvoiceStatus + '</span>' else '<span class=""badge bg-danger"">' + ci.InvoiceStatus + '</span>' end InvoiceStatus, 
case when (p.PackageName + '<br>' + convert(varchar,ci.Bandwidth) + ' Mbps ' + ci.UM + ' Free: ' + convert(varchar, ci.FreeDuration)+ ' Month' + '<br><span class=""text-primary"">' + format(ci.StartDate, 'dd/MM/yyyy') + '</span> - <span class=""text-danger"">' + format(ci.EndDate, 'dd/MM/yyyy') + '</span>') is null then 
STRING_AGG(ci.ItemDescription, ',')
else 
p.PackageName + '<br>' + convert(varchar,ci.Bandwidth) + ' Mbps ' + ci.UM + ' Free: ' + convert(varchar, ci.FreeDuration)+ ' Month' + '<br><span class=""text-primary"">' + format(ci.StartDate, 'dd/MM/yyyy') + '</span> - <span class=""text-danger"">' + format(ci.EndDate, 'dd/MM/yyyy') + '</span>' end ItemDescription
,(bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) InstallAddress, 
'<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+', '''+ ci.InvoiceNo +''',0,10,'''',false,this);"">' + ci.InvoiceNo + '</a>' PI, ci.InvoiceNo,
case when ci.InvoiceStatusID = 101 then '<a href=""javascript:void(0);"" onclick=""Edit_Tbl_Row('''', '''', '''+ci.InvoiceNo+''', false, '''', '+convert(varchar, c.CustomerID)+', true,'''','''+ci.[Type]+''');"" class=""btn btn-sm btn-primary text-white"">Approve</a>' 
when ci.InvoiceStatusID = 94 then ''
else ci.ApproveBy + '<br>' + format(ci.ApproveDate, 'dd-MM-yyyy HH:mm:ss tt') end Approval
from dbo.TblCustomer c
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
left join dbo.TblPackageDetail pd on pd.PackageDetailID = ci.PackageDetailID
left join dbo.TblPackages p on p.PackageID = ci.PackageID
inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = ci.CustomerBillInformationID
where ci.StatusID not in (12,13,14,20) and ci.[Type] = 'Sale Order Invoice' and isnull(ci.ItemDescription,'') <> 'Deposit Fee' and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + @"%'
group by c.CustomerID, c.CustomerCode,c.CustomerName, c.CustomerMobile, c.RegisterDate, c.RegisterBy,
(p.PackageName + '<br>' + convert(varchar,ci.Bandwidth) + ' Mbps ' + ci.UM + ' Free: ' + convert(varchar, ci.FreeDuration)+ ' Month' + '<br><span class=""text-primary"">' + format(ci.StartDate, 'dd/MM/yyyy') + '</span> - <span class=""text-danger"">' + format(ci.EndDate, 'dd/MM/yyyy') + '</span>'),
ci.InvoiceStatusID,ci.InvoiceStatus,bf.InstallAddress,bf.InstallCommune,bf.InstallDistrict,bf.InstallProvinceCity,ci.InvoiceNo,ci.[Type],
ci.ApproveBy,ci.ApproveDate, ci.StartDate, ci.EndDate, ci.CreateDate";
            html.GH.SQL = sql;
            html.IsUseHavingCondition = true;
            html.OrderBy = "[No]";//"case when isnull(ci.InvoiceStatus,'') = 'Pending' then 0 else 1 end, format(ci.CreateDate,'yyMMdd') desc";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo", ClassName="text-nowrap", IsHiddenField = true },
                new GridField() { Header = "PI#", FieldName = "PI", ClassName="text-nowrap" },
                new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
				//new GridField() { Header = "Phone", FieldName = "CustomerMobile" },
				new GridField() { Header = "Register Date", FieldName = "RGDate" },
                //new GridField() { Header = "Register By", FieldName = "RegisterBy" },
                new GridField() { Header = "Package Name", FieldName = "ItemDescription", Style="max-width: 400px;text-overflow: ellipsis;white-space: nowrap;overflow: hidden;" },
                //new GridField() { Header = "Bandwidth", FieldName = "Bandwidth" },
                //new GridField() { Header = "Duration", FieldName = "Duration" },
                //new GridField() { Header = "FreeDuration", FieldName = "FreeDuration" },
                //new GridField() { Header = "UM", FieldName = "UM" },
                //new GridField() { Header = "Start Date", FieldName = "StartDate", Format="dd/MM/yyyy" },
                //new GridField() { Header = "End Date", FieldName = "EndDate", Format="dd/MM/yyyy" },
                new GridField() { Header = "Status", FieldName = "InvoiceStatus", TextAlign="center" },
                new GridField() { Header = "Approve", FieldName = "Approval", IsHiddenField=ApprovalSaleOrder!=true, TextAlign="center" },
                new GridField() { Header = "Install Address", FieldName = "InstallAddress" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate", FormatDateTime = "dd-MM-yyyy", ClassName="text-center"},
                new GridField() { Header = "Action",
                EditFunction ="Edit_SaleOrder(#=CustomerID#,'#=InvoiceNo#');",
                FieldName = "CustomerID", TextAlign = "Center", IsModifyField = true, DeleteWithTableName="TblCustomerBalance, TblCustomerBillInformation, TblCustomerBillingInvoice, TblCustomerCompany, TblCustomerDetail, TblCustomerInvoice, TblCustomerPayment" }
            };
            //html.DefaultSearch = false;
            //html.DrawTotalAllHeader = true;
            //html.DrawTotalAllHeaderDataTableNumber = 2;
            //html.GridEventClick = "ShowCustomerInfo(this)";
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString()); //html.DrawHTMLSearchForm();

            return View();
        }
        //public ActionResult Index()
        //{
        //    HTML html = new HTML();
        //    html.ReportTitle = "List Customers";
        //    html.isShowLoadingGrid = false;
        //    if (html.HasPermission("Add Sale Order"))
        //        html.AllowAdd = true;
        //    if (html.HasPermission("Edit Sale Order"))
        //        html.AllowEdit = true;
        //    if (html.HasPermission("Delete Sale Order"))
        //        html.AllowDelete = true;

        //    html.DialogBoxWidth = "Auto";
        //    html.DialogBoxHeight = 600;
        //    html.AddButton = "New Sale Order";
        //    html.AddURL = "SaleOrder/CreateCustomer";
        //    html.GH.GridProcedureName = "SP_SEARCH_CUSTOMER_SALE_ORDER";
        //    List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
        //    html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
        //    {
        //        DefaultValueDate = false,
        //        //DateTimePickerWithRanges = true,
        //        DateRangePicker = true,
        //        LabelFromDate = "Register Date:"
        //    });
        //    string sqlLocation = "select distinct CustomerProvinceCity from dbo.TblCustomer_SaleOrder where lower(CustomerProvinceCity) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by CustomerProvinceCity";
        //    HtmlAddControls htmlAddControl = new HtmlAddControls()
        //    {
        //        ID = "txtLocation",
        //        Label = "Location:",
        //        DBField = "Location",
        //        SQLAutoComplete = sqlLocation,
        //        ISAutoComplete = true
        //    };
        //    htmlAddControls.Add(htmlAddControl);
        //    string sql = "select distinct s.StatusID, s.[Status] from dbo.TblCustomer_SaleOrder c ";
        //    sql += "inner join dbo.TblStatus s on s.StatusID = c.StatusID ";
        //    sql += "where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by s.StatusID";
        //    htmlAddControl = new HtmlAddControls()
        //    {
        //        ID = "txtStatus",
        //        Label = "Status:",
        //        DBField = "Status",
        //        SQL_CBO = sql,
        //        DisplayText = "Status",
        //        Value = "StatusID",
        //        ComboBox = true
        //    };
        //    htmlAddControls.Add(htmlAddControl);
        //    html.HtmlAddControls.Add(new HtmlAddControls()
        //    {
        //        ID = "txtCustomerGroup",
        //        Label = "Customer Group:",
        //        DBField = "CustomerGroupID",
        //        SQL_CBO = "select cg.CustomerGroupID, cg.CustomerGroup from dbo.TblCustomerGroup cg\r\ninner join dbo.TblCustomer_SaleOrder c on c.CustomerGroupID = cg.CustomerGroupID where cg.SystemID = " + html.SystemID + " and cg.WarehouseID = " + html.WarehouseID + " order by cg.CustomerGroup",
        //        DisplayText = "CustomerGroup",
        //        Value = "CustomerGroupID",
        //        ComboBox = true
        //    });
        //    List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
        //    RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
        //    {
        //        OPTLabel = "CID",
        //        SQL = "select top 10 CustomerCode from dbo.TblCustomer_SaleOrder where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerCode",
        //        RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("PhoneNumber"),
        //        ISAutoComplete = true,
        //        DefaultCheckBok = true
        //    };
        //    htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
        //    radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
        //    {
        //        OPTLabel = "Customer Name",
        //        SQL = "select top 10 CustomerName from dbo.TblCustomer_SaleOrder where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerName",
        //        RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("PhoneNumber"),
        //        ISAutoComplete = true
        //    };
        //    htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
        //    radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
        //    {
        //        OPTLabel = "Phone Number",
        //        SQL = "select distinct top 10 CompanyContactMobileNumber from dbo.TblCustomerCompany_SaleOrder where lower(CompanyContactMobileNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CompanyContactMobileNumber",
        //        RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("PhoneNumber"),
        //        ISAutoComplete = true
        //    };
        //    htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
        //    html.GridEventClick = "onGridClick(this);";
        //    //html.GridHeight = "400px";
        //    //html.DefaultSearch = false;
        //    html.TableName = "TblCustomer_SaleOrder";
        //    html.GH.GridTable = new GridField[]
        //    {
        //        new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
        //        new GridField() { Header = "CustomerID", FieldName = "CustomerID", IsHiddenField= true },
        //        new GridField() { Header = "CustomerGroup", FieldName = "CustomerGroup", GroupLabel="Group Name: ", IsGroupByField= true, Colspan=9 },
        //        new GridField() { Header = "CID", FieldName = "CustomerCode" },
        //        new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
        //        new GridField() { Header = "Phone Number", FieldName = "PhoneNumber" },
        //        //new GridField() { Header = "Inv.Date", FieldName = "InvoiceDate", TextAlign="center" },
        //        //new GridField() { Header = "Inv.No", FieldName = "InvoiceNo", TextAlign="center" },
        //        //new GridField() { Header = "Total", FieldName = "GrandTotal" },
        //        new GridField() { Header = "Register Date", FieldName = "RegisterDate", ClassName="text-danger" },
        //        new GridField() { Header = "Register By", FieldName = "RegisterBy" },
        //        new GridField() { Header = "Location", FieldName = "Location" },
        //        //new GridField() { Header = "Status", FieldName = "Status", TextAlign="center" },
        //        new GridField() { Header = "Sale Name", FieldName = "SaleName" },
        //        new GridField() { Header = "Action", FieldName = "CustomerID", TextAlign = "Center",  IsModifyField = true, HideEdit= true, DeleteWithTableName="TblCustomerBalance_SaleOrder, TblCustomerBillInformation_SaleOrder, TblCustomerBillingInvoice_SaleOrder, TblCustomerCompany_SaleOrder, TblCustomerDetail_SaleOrder, TblCustomerInvoice_SaleOrder, TblCustomerPayment_SaleOrder" }
        //    };
        //    ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
        //    return View();
        //}
        public ActionResult CustomerInformation()
        {
            return View();
        }
        public ActionResult CompanyInformation()
        {
            return View();
        }

        [Authorize(Roles = "Add Sale Order, Edit Sale Order")]
        [HttpPost]
        public ActionResult CreateCustomer(object[] PrimaryFieldValue)
        {
            HTML html = new HTML();
            html.AllowAdd = html.HasPermission("Add Sale Order");

            if (html.HasPermission("Edit Sale Order"))
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
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(PrimaryFieldValue[0].ToString());
            string FormStatus = "";
            //if (PrimaryFieldValue[0].ToString().Contains("ConnectionID"))
            //{
            //    ViewBag.ConnectionID = DataInfo["ConnectionID"].ToString();
            //}
            string CustomerID = "", InvoiceNo = "";
            DataSet ds = new DataSet();
            string sql;
            if (PrimaryFieldValue[0].ToString().Contains("PrimaryFieldValue"))
            {
                CustomerID = DataInfo["CustomerID"].ToString();
                InvoiceNo = DataInfo["InvoiceNo"].ToString();
                sql = "select BillingInvoiceID, ConnectivityStatus, PPPoEUserName, ConnectionID from dbo.TblCustomerBillingInvoice_SaleOrder where CustomerID=" + CustomerID + " and InvoiceNo=N'" + InvoiceNo + "'";
                ds = html.LoadData(sql);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        ViewBag.ConnectionID = ds.Tables[0].Rows[0]["ConnectionID"].ToString();
                }
            }
            ViewBag.EditInvoice = false;
            if (PrimaryFieldValue[0].ToString().Contains("EditInvoice"))
            {
                ViewBag.EditInvoice = DataInfo["EditInvoice"].ToString();
            }
            if (PrimaryFieldValue[0].ToString().Contains("IsRenew"))
            {
                bool IsRenew = bool.Parse(DataInfo["IsRenew"].ToString());
                if (IsRenew)
                    ViewBag.EditInvoice = false;
            }
            if (PrimaryFieldValue[0].ToString().Contains("ChangePlan"))
            {
                sql = @" select ac.ActivateCustomerID, s.ServiceName, pt.PackageTypeName, 
                                p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + bi.UM PackageName,
                                pd.Bandwidth, bi.UM, pd.DowSpeed, pd.UpSpeed, pd.Price, format(bi.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, format(bi.StartDate, 'dd-MM-yyyy') StartDate, format(bi.EndDate, 'dd-MM-yyyy') EndDate,
                                case when (DATEDIFF(D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate))) < 0 then 0 else DATEDIFF (D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate)) end Remain, 
                                isnull(ac.ServiceID,0) ServiceID, isnull(bi.PackageDetailID, 0) PackageDetailID, isnull(bi.FreeDuration,0) FreeDuration, isnull(bi.Duration,0) Duration, 
                                bi.InvoiceNo, bi.ConnectionID
                                from dbo.TblActivateCustomer_SaleOrder ac
                                inner join dbo.TblCustomer_SaleOrder c on c.CustomerID = ac.CustomerID
                                inner join dbo.TblServices s on s.ServiceID = ac.ServiceID
                                inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
                                inner join dbo.TblPackages p on p.PackageID = pd.PackageID
                                inner join dbo.TblPackageType pt on p.PackageTypeID = pt.PackageTypeID
                                inner join dbo.TblCustomerBillingInvoice_SaleOrder bi on bi.ActivateCustomerID = ac.ActivateCustomerID
                                where bi.BillingInvoiceID = @BillingInvoiceID";
                Dictionary<string, string> Para = new Dictionary<string, string>
                {
                    { "BillingInvoiceID", DataInfo["BillingInvoiceID"].ToString() }
                };
                ViewBag.CustomerCurrentPlan = html.JsonData(sql, false, Para);
                ViewBag.ChangePlan = true;
            }
            if (PrimaryFieldValue[0].ToString().Contains("IsPreviewContract"))
                ViewBag.IsPreviewContract = DataInfo["IsPreviewContract"].ToString();
            if (PrimaryFieldValue[0].ToString().Contains("CloneInvoice"))
                ViewBag.CloneInvoice = DataInfo["CloneInvoice"].ToString();
            if (PrimaryFieldValue[0].ToString().Contains("IsRenew"))
            {
                ViewBag.IsRenew = DataInfo["IsRenew"].ToString();
                ViewBag.InvoiceType = "Renew Invoice";
                //string CustomerID = "";
                if (PrimaryFieldValue[0].ToString().Contains("PrimaryFieldValue"))
                {
                    CustomerID = DataInfo["CustomerID"].ToString();
                }

                sql = "select BillingInvoiceID, ConnectivityStatus, PPPoEUserName, ConnectionID from dbo.TblCustomerInvoice_SaleOrder where CustomerID=" + CustomerID + " and InvoiceNo=N'" + InvoiceNo + "'";
                ds = html.LoadData(sql);
                ViewBag.ConnectivityStatus = ds.Tables[0].Rows[0]["ConnectivityStatus"].ToString();
                ViewBag.OldBillingInvoiceID = ds.Tables[0].Rows[0]["BillingInvoiceID"].ToString();
                ViewBag.PPPoEUserName = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
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
                        html.AllowAdd = html.HasPermission("Create Invoice");
                        if (!html.AllowAdd)
                            return RedirectToAction("Index", "NoPermission");
                    }
                    sql = "select (e.Salutation + e.FullName) Employee, c.EmployeeID, c.CustomerID, ty.CustomerType, c.CustomerTypeID, c.CustomerName, c.CustomerCode, c.CustomerGender, c.CustomerAddress, ";
                    sql += "c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity, ";
                    sql += "c.CustomerMobile, c.CustomerEmail, c.CustomerNationality, c.CustomerIdentification, ";
                    sql += "c.CustomerContactName, c.CustomerContactMobileNumber, c.CustomerContactEmail, c.CustomerGroupID, c.CustomerGroup ";
                    sql += "from dbo.TblCustomer_SaleOrder c ";
                    sql += "inner join dbo.TblCustomerType ty on ty.CustomerTypeID = c.CustomerTypeID ";
                    sql += "inner join dbo.TblLeads e on e.LeadID = c.EmployeeID ";
                    sql += "where CustomerID = " + CustomerID + "; ";
                    ViewBag.CustInfo = html.JsonData(sql);
                    sql = @"select distinct cb.CustomerBillInformationID, cb.CompanyName, cb.CompanyNameKH, cb.CompanyDeskPhone, cb.CompanyMobile, 
cb.CompanyEmail, cb.VATNumber, cb.CompanyAddress, cb.CompanyCommune, ci.PID,
cb.CompanyDistrict, cb.CompanyProvinceCity, cb.CompanyContactName,
cb.CompanyContactPosition, cb.CompanyContactMobileNumber, cb.ATTName,
cb.BillToAddress, cb.BillToCommune, cb.BillToDistrict, cb.BillToProvinceCity,
cb.InstallAddress, cb.InstallCommune, cb.InstallDistrict, cb.InstallProvinceCity, cb.IndustryTypeID, it.IndustryType, cb.GoogleMap_FullAddress, cb.GoogleMap_Latitude, cb.GoogleMap_Longitude, cb.InvoiceType, cb.ClassNameID, cb.ClassName, cb.LocationID, cb.Warehouse
from dbo.TblCustomerBillInformation_SaleOrder cb
inner join dbo.TblIndustryType it on it.IndustryTypeID = cb.IndustryTypeID
inner join dbo.TblCustomerInvoice_SaleOrder ci on ci.CustomerBillInformationID = cb.CustomerBillInformationID
where cb.CustomerID = " + CustomerID;
                    if (InvoiceNo != "0")
                    {
                        sql += " and ci.InvoiceNo = '" + InvoiceNo + "'";
                    }
                    ViewBag.CustDetail = html.JsonData(sql);
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
                    ViewBag.CustInvoice = html.JsonData(sql, true, Para).ToJsonString();
                    //string result = ViewBag.CustInvoice.Substring(1, ViewBag.CustInvoice.Length - 2);
                    //Dictionary<string, string> Datainfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                    //string IsChangePlan = Datainfo["IsChangePlan"], FromActivateCustomerID = Datainfo["FromActivateCustomerID"];
                    if (ViewBag.IsRenew == "false")
                    {
                        if (!PrimaryFieldValue[0].ToString().Contains("ChangePlan"))
                        {
                            sql = "select distinct IsChangePlan, FromActivateCustomerID from dbo.TblCustomerInvoice_SaleOrder where InvoiceNo=@InvoiceNo";
                            Para = new Dictionary<string, string>()
                            {
                                { "InvoiceNo", InvoiceNo }
                            };
                            ds = html.LoadData(sql, false, Para);
                            string IsChangePlan = ds.Tables[0].Rows[0]["IsChangePlan"].ToString();
                            string FromActivateCustomerID = ds.Tables[0].Rows[0]["FromActivateCustomerID"].ToString();
                            if (IsChangePlan == "True")
                            {
                                sql = @" select ac.ActivateCustomerID, s.ServiceName, pt.PackageTypeName, 
                                p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + bi.UM PackageName,
                                pd.Bandwidth, bi.UM, pd.DowSpeed, pd.UpSpeed, pd.Price, format(bi.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, format(bi.StartDate, 'dd-MM-yyyy') StartDate, format(bi.EndDate, 'dd-MM-yyyy') EndDate,
                                case when (DATEDIFF(D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate))) < 0 then 0 else DATEDIFF (D, GETDATE(), DateAdd(M, -bi.FreeDuration, bi.EndDate)) end Remain, 
                                isnull(ac.ServiceID,0)ServiceID, isnull(bi.PackageDetailID,0) PackageDetailID, 
                                isnull(bi.FreeDuration,0)FreeDuration, isnull(bi.Duration,0) Duration, bi.InvoiceNo, bi.PID, bi.ConnectionID
                                from dbo.TblActivateCustomer_SaleOrder ac
                                inner join dbo.TblCustomer_SaleOrder c on c.CustomerID = ac.CustomerID
                                inner join dbo.TblServices s on s.ServiceID = ac.ServiceID
                                inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
                                inner join dbo.TblPackages p on p.PackageID = pd.PackageID
                                inner join dbo.TblPackageType pt on p.PackageTypeID = pt.PackageTypeID
                                inner join dbo.TblCustomerBillingInvoice_SaleOrder bi on bi.ActivateCustomerID = ac.ActivateCustomerID
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
                        sql += "ci.DepositFee, ci.PID, ci.ConnectionID ";
                    sql += "from dbo.TblCustomerInvoice_SaleOrder ci ";
                    sql += "inner join dbo.TblPackageDetail p on p.PackageDetailID = ci.PackageDetailID ";
                    sql += "where InvoiceNo = N'" + InvoiceNo + "' and ci.DepositFee <> 0";
                    ViewBag.CustInvoiceDeposit = html.JsonData(sql);
                    sql = "select  ci.ItemDescription, ci.UM, ci.QTY, ci.UnitPrice, ci.DiscountP, ci.DiscountUSD, ci.Amount, ";
                    sql += "ci.OtherChargeID, ci.StockInID, ci.ItemID, format(ci.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, format(ci.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, ci.ConnectionID from dbo.TblCustomerInvoice_SaleOrder ci ";
                    sql += "where InvoiceNo = N'" + InvoiceNo + "' and (ci.OtherChargeID <> 0 or ci.ItemID <> 0)";
                    ViewBag.CustInvoiceDetail = html.JsonData(sql);
                    sql = "select SubTotal, VAT, Total, DiscountP, DiscountUSD, GrandTotal, IsIncludedVAT ";
                    sql += "from dbo.TblCustomerBalance_SaleOrder where InvoiceNo = N'" + InvoiceNo + "'";
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

                    if (!html.AllowAdd)
                        return RedirectToAction("Index", "NoPermission");
                }
            }
            ViewBag.isRegisterForm = value;
            ViewBag.FormStatus = FormStatus;
            return View();
        }
        public ActionResult CreateCustomerImg(object[] PrimaryFieldValue)
        {
            HTML html = new HTML();
            html.AllowAdd = html.HasPermission("Add Customer");

            if (html.HasPermission("Edit Customer"))
                html.AllowEdit = true;
            else
                html.AllowAdd = false;

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
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(PrimaryFieldValue[0].ToString());
            string FormStatus = "";
            if (PrimaryFieldValue[0].ToString().Contains("CloneInvoice"))
                ViewBag.CloneInvoice = DataInfo["CloneInvoice"].ToString();
            if (PrimaryFieldValue[0].ToString().Contains("IsRenew"))
            {
                ViewBag.IsRenew = DataInfo["IsRenew"].ToString();
                ViewBag.InvoiceType = "Renew Invoice";
                string InvoiceNo = DataInfo["InvoiceNo"].ToString();
                string sql = "select BillingInvoiceID, ConnectivityStatus, PPPoEUserName from dbo.TblCustomerInvoice_SaleOrder where InvoiceNo=N'" + InvoiceNo + "'";
                DataSet ds = html.LoadData(sql);
                ViewBag.ConnectivityStatus = ds.Tables[0].Rows[0]["ConnectivityStatus"].ToString();
                ViewBag.OldBillingInvoiceID = ds.Tables[0].Rows[0]["BillingInvoiceID"].ToString();
                ViewBag.PPPoEUserName = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
            }
            if (PrimaryFieldValue[0].ToString().Contains("NextInvoiceDate"))
                ViewBag.NextInvoiceDate = DataInfo["NextInvoiceDate"].ToString();
            string value;
            if (PrimaryFieldValue[0].ToString().Contains("PrimaryFieldValue"))
            {
                value = DataInfo["PrimaryFieldValue"].ToString();
                string InvoiceNo = DataInfo["InvoiceNo"].ToString();
                string CustomerID = DataInfo["CustomerID"].ToString();
                FormStatus = DataInfo["FormStatus"].ToString();
                if (value == "1")
                {
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
                    //if (FormStatus == "CreateInvoice")
                    //{
                    //    if (html.HasPermission("Create Invoice"))
                    //        html.AllowAdd = true;
                    //    else
                    //        html.AllowAdd = false;
                    //    if (!html.AllowAdd)
                    //        return RedirectToAction("Index", "NoPermission");
                    //}
                    string sql = "select (e.Salutation + e.FullName) Employee, c.EmployeeID, c.CustomerID, ty.CustomerType, c.CustomerTypeID, c.CustomerName, c.CustomerCode, c.CustomerGender, c.CustomerAddress, ";
                    sql += "c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity, ";
                    sql += "c.CustomerMobile, c.CustomerEmail, c.CustomerNationality, c.CustomerIdentification, ";
                    sql += "c.CustomerContactName, c.CustomerContactMobileNumber, c.CustomerContactEmail ";
                    sql += "from dbo.TblCustomer_SaleOrder c ";
                    sql += "inner join dbo.TblCustomerType ty on ty.CustomerTypeID = c.CustomerTypeID ";
                    sql += "inner join dbo.TblLeads e on e.LeadID = c.EmployeeID ";
                    sql += "where CustomerID = " + CustomerID + "; ";
                    ViewBag.CustInfo = html.JsonData(sql);
                    sql = "select cb.CustomerBillInformationID, cb.CompanyName, cb.CompanyNameKH, cb.CompanyDeskPhone, cb.CompanyMobile, ";
                    sql += "cb.CompanyEmail, cb.VATNumber, cb.CompanyAddress, cb.CompanyCommune, ";
                    sql += "cb.CompanyDistrict, cb.CompanyProvinceCity, cb.CompanyContactName, ";
                    sql += "cb.CompanyContactPosition, cb.CompanyContactMobileNumber, ";
                    sql += "cb.BillToAddress, cb.BillToCommune, cb.BillToDistrict, cb.BillToProvinceCity, ";
                    sql += "cb.InstallAddress, cb.InstallCommune, cb.InstallDistrict, cb.InstallProvinceCity, cb.IndustryTypeID, it.IndustryType, cb.GoogleMap_FullAddress, cb.GoogleMap_Latitude, cb.GoogleMap_Longitude, cb.InvoiceType ";
                    sql += "from dbo.TblCustomerBillInformation_SaleOrder cb ";
                    sql += "inner join dbo.TblIndustryType it on it.IndustryTypeID = cb.IndustryTypeID ";
                    sql += "where cb.CustomerID = " + CustomerID;
                    ViewBag.CustDetail = html.JsonData(sql);
                }
                if (InvoiceNo != "0")
                {
                    ViewBag.InvoiceNo = InvoiceNo;
                    string sql = "SP_GET_INVOICE_INFO";
                    Dictionary<string, string> Para = new Dictionary<string, string>()
                    {
                        { "InvoiceNumber", InvoiceNo },
                        { "InvoiceType", ViewBag.InvoiceType }
                    };
                    ViewBag.CustInvoice = html.JsonData(sql, true, Para);
                    sql = "select ci.UM, ci.QTY, ci.UnitPrice, ci.DiscountP, ";
                    sql += "ci.DiscountUSD, ci.Amount, ";
                    if (ViewBag.InvoiceType == "Renew Invoice")
                        sql += "0 DepositFee ";
                    else
                        sql += "ci.DepositFee ";
                    sql += "from dbo.TblCustomerInvoice_SaleOrder ci ";
                    sql += "inner join dbo.TblPackageDetail p on p.PackageDetailID = ci.PackageDetailID ";
                    sql += "where InvoiceNo = N'" + InvoiceNo + "' and ci.DepositFee <> 0";
                    ViewBag.CustInvoiceDeposit = html.JsonData(sql);
                    sql = "select  ci.ItemDescription, ci.UM, ci.QTY, ci.UnitPrice, ci.DiscountP, ci.DiscountUSD, ci.Amount, ";
                    sql += "ci.OtherChargeID, ci.StockInID, ci.ItemID, format(ci.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, format(ci.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate from dbo.TblCustomerInvoice_SaleOrder ci ";
                    sql += "where InvoiceNo = N'" + InvoiceNo + "' and (ci.OtherChargeID <> 0 or ci.StockInID <> 0)";
                    ViewBag.CustInvoiceDetail = html.JsonData(sql);
                    sql = "select SubTotal, VAT, Total, DiscountP, DiscountUSD, GrandTotal, IsIncludedVAT ";
                    sql += "from dbo.TblCustomerBalance_SaleOrder where InvoiceNo = N'" + InvoiceNo + "'";
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

                    if (!html.AllowAdd)
                        return RedirectToAction("Index", "NoPermission");
                }
            }
            ViewBag.isRegisterForm = value;
            ViewBag.FormStatus = FormStatus;
            return View();
        }
        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadEployee()
        {
            HTML html = new HTML();
            //string SQL = "select EmployeeID, (EmployeeCode + ' - ' + EmployeeName) EmployeeName ";
            //SQL += "from dbo.TblEmployees ";
            //SQL += "where PositionID = 6 and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " ";
            //SQL += "order by(EmployeeCode +' - ' + EmployeeName)";
            string SQL = @"select LeadID EmployeeID, Salutation + ' ' + FullName EmployeeName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadTermCondition(int TermCondition = 0)
        {
            HTML html = new HTML();
            string SQL = "select top 1 Descp ";
            SQL += "from dbo.TblTermCondition ";
            SQL += "where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID;
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult CheckCustCode(string CustCode, string NewValue = "")
        {
            HTML html = new HTML();
            string SQL = "";
            SqlResult result = new SqlResult();
            if (NewValue == "")
            {
                SQL = "select CustomerCode from dbo.TblCustomer_SaleOrder ";
                SQL += "where lower(CustomerCode) = N'" + CustCode.ToLower() + "' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID;
            }
            else if (CustCode != NewValue)
            {
                SQL = "select CustomerCode from dbo.TblCustomer_SaleOrder ";
                SQL += "where lower(CustomerCode) = N'" + NewValue.ToLower() + "' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID;
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

        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadCustOrderType()
        {
            HTML html = new HTML();
            string SQL = "select OrderTypeID, OrderType from dbo.TblOrderType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by OrderType";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadIndustryType()
        {
            HTML html = new HTML();
            string SQL = "select IndustryTypeID, IndustryType from dbo.TblIndustryType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by IndustryType";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadServicePlan()
        {
            HTML html = new HTML();
            string SQL = "select distinct ts.ServiceID, ts.ServiceName from dbo.TblServices ts inner join dbo.TblPackages tp on tp.ServiceID = ts.ServiceID where ts.SystemID = " + html.SystemID + " and ts.WarehouseID=" + html.WarehouseID + " order by ts.ServiceName";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadServicePlanAndOtherCharge(int LocationID)
        {
            HTML html = new HTML();
            string SQL = @"select '0' StockInID, '0' ItemID, OtherChargeItem ItemName, Fee SalePrice, UM, OtherChargeID,
            '0' PackageDetailID, '0' PackageID, '0' ServiceID, '0' Duration, '0' FreeDuration, '' PackageName, '0' Bandwidth,
            '0' DowSpeed, '0' UpSpeed, '0' DepositFee, '' Feature, '3' OrderBy, (ABS(CAST(CAST(NEWID() AS VARBINARY) AS INT)) % (100 - 70 + 1) + 70) PID
            from dbo.TblOtherCharge_SaleOrder
            where SystemID = @SystemID and WarehouseID = @WarehouseID
            union all
            select distinct 0 StockInID, s.ItemID, i.ItemName, i.SellingPrice SalePrice, i.UM,
            '0' OtherChargeID, '0' PackageDetailID, '0' PackageID, '0' ServiceID, '0' Duration, '0' FreeDuration, '' PackageName,
            '0' Bandwidth, '0' DowSpeed, '0' UpSpeed, '0' DepositFee, '' Feature, '2' OrderBy, (ABS(CAST(CAST(NEWID() AS VARBINARY) AS INT)) % (100 - 70 + 1) + 70) PID
            from dbo.TblStockInDetail_SaleOrder s
            inner join dbo.TblItem i on i.ItemID = s.ItemID
            --inner join dbo.TblSaleInfo si on si.ItemID = s.ItemID
            where s.LocationID = @LocationID and i.SystemID = @SystemID and i.WarehouseID = @WarehouseID
            group by s.ItemID, i.ItemName, i.SellingPrice, i.UM
            having (sum(isnull(s.QTY,0))-sum(isnull(s.QTYOUT,0))) > 0
            union all
            select '0' StockInID, '0' ItemID, (pd.ServiceName + ' - ' + pd.PackageName + ' - ' +
            convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM) ItemName,
            pd.Price, (convert(varchar, pd.Duration) + ' ' + pd.UM) UM,
            '0' OtherChargeID, isnull(pd.PackageDetailID, 0) PackageDetailID, 
            isnull(pd.PackageID, 0) PackageID, isnull(pd.ServiceID, 0) ServiceID, 
            isnull(pd.Duration, 0) Duration, isnull(pd.FreeDuration, 0) FreeDuration, 
            isnull(pd.PackageName, 0) PackageName, isnull(pd.Bandwidth,0) Bandwidth, 
            isnull(pd.DowSpeed,0) DowSpeed, isnull(pd.UpSpeed,0) UpSpeed, isnull(pd.DepositFee,0) DepositFee, pd.Feature, '1' OrderBy, (ABS(CAST(CAST(NEWID() AS VARBINARY) AS INT)) % (100 - 70 + 1) + 70) PID
            from dbo.TblPackageDetail pd
            inner join dbo.TblPackages p on p.PackageID = pd.PackageID
            where pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID
            order by OrderBy, ItemName";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "LocationID", LocationID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadPackages(int ServiceID, string InvoiceType)
        {
            HTML html = new HTML();
            string SQL = "select pd.PackageID, pd.PackageDetailID, (p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps' ";
            SQL += "+ ' - ' + convert(varchar, pd.Duration) + ' ' + pd.UM) PackageName, ";
            SQL += "pd.Price, pd.Feature, ";
            //if (InvoiceType == "Renew Invoice")
            //    SQL += "0 DepositFee, ";
            //else
            SQL += "pd.DepositFee, ";
            SQL += "pd.FreeDuration, pd.Duration, pd.UM, p.SpecialNote, ";
            SQL += "pd.Bandwidth, pd.DowSpeed, pd.UpSpeed, p.PackageName OriPackageName, (ABS(CAST(CAST(NEWID() AS VARBINARY) AS INT)) % (100 - 70 + 1) + 70) PID ";
            SQL += "from dbo.TblPackages p inner join dbo.TblPackageDetail pd on pd.PackageID = p.PackageID ";
            SQL += "where p.ServiceID = " + ServiceID + " and p.SystemID = " + html.SystemID + " and p.WarehouseID=" + html.WarehouseID + " order by p.PackageName, pd.PackageDetailID";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadOtherCharge(int LocationID)
        {
            HTML html = new HTML();
            string SQL = @"select '0' StockInID, '0' ItemID, OtherChargeItem ItemName, Fee SalePrice, UM, OtherChargeID,
            '0' PackageDetailID, '0' PackageID, '0' ServiceID, '0' Duration, '0' FreeDuration, '' PackageName, '0' Bandwidth,
            '0' DowSpeed, '0' UpSpeed, '0' DepositFee, '' Feature, '3' OrderBy
            from dbo.TblOtherCharge_SaleOrder
            where SystemID = @SystemID and WarehouseID = @WarehouseID
            union all
            select distinct 0 StockInID, s.ItemID, i.ItemName, i.SellingPrice SalePrice, i.UM,
            '0' OtherChargeID, '0' PackageDetailID, '0' PackageID, '0' ServiceID, '0' Duration, '0' FreeDuration, '' PackageName,
            '0' Bandwidth, '0' DowSpeed, '0' UpSpeed, '0' DepositFee, '' Feature, '2' OrderBy
            from dbo.TblStockInDetail_SaleOrder s
            inner join dbo.TblItem i on i.ItemID = s.ItemID
            --inner join dbo.TblSaleInfo si on si.ItemID = s.ItemID
            where s.LocationID=@LocationID and i.SystemID = @SystemID and i.WarehouseID = @WarehouseID
            group by s.ItemID, i.ItemName, i.SellingPrice, i.UM
            having (sum(isnull(s.QTY,0))-sum(isnull(s.QTYOUT,0))) > 0";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "LocationID", LocationID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            return Json(new { data = html.JsonData(SQL, false, para) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadClassName()
        {
            HTML html = new HTML();
            string SQL = "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadLocation()
        {
            HTML html = new HTML();
            string SQL = "select uw.WID, (b.BranchName + ' - ' + uw.WarehouseName) WarehouseName, uw.ContactName, uw.ContactPhone, uw.[Address] + case when uw.Commune <> '' then ', ' + uw.Commune end + case when uw.District <> '' then ', ' + uw.District end + case when uw.ProvinceCity <> '' then ', ' + uw.ProvinceCity end Warehouse from dbo.TblUserWarehouse uw inner join dbo.TblBranch b on b.BranchID = uw.BranchID where uw.SystemID = " + html.SystemID + " and uw.WarehouseID = " + html.WarehouseID + " order by (b.BranchName + ' - ' + uw.WarehouseName)";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadCompanyLogo()
        {
            HTML html = new HTML();
            string SQL = "select top 1 CompanyID, Logo, TermConditionVAT, TermConditionNonVAT, CompanyAddress, Phone, Email, Website from dbo.TblCompany where ISDefault = 1 and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + "";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadCommune()
        {
            HTML html = new HTML();
            string SQL = "select c.CommuneID, c.Commune, d.DistrictID, d.District, p.ProvinceCity, p.ProvinceCityID ";
            SQL += "from dbo.TblCommune c ";
            SQL += "inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID ";
            SQL += "inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID ";
            SQL += "where c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " order by c.Commune";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadDistrict(int DistrictID)
        {
            HTML html = new HTML();
            string SQL = "select DistrictID, District from dbo.TblDistrict where DistrictID=" + DistrictID + " and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by District";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadProvinceCity()
        {
            HTML html = new HTML();
            string SQL = "select ProvinceCityID, ProvinceCity from dbo.TblProvinceCity where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ProvinceCity";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Customer, Edit Customer")]
        public ActionResult SaveChangeCustomer(object[] CustomerData, object[] CustInvoice, object[] CustInvoiceDetail, object[] CustomerOtherChargeInvoice, object[] ImgPdfFile)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            dynamic CustomerDataInfo = new JavaScriptSerializer().DeserializeObject(CustomerData[0].ToString());
            dynamic CustomerInvoice = new JavaScriptSerializer().DeserializeObject(CustInvoice[0].ToString());
            dynamic CustInvDetail = new JavaScriptSerializer().DeserializeObject(CustInvoiceDetail[0].ToString());
            dynamic CustomerInvoiceOtherCharge = new JavaScriptSerializer().DeserializeObject(CustomerOtherChargeInvoice[0].ToString());
            JavaScriptSerializer jsJson = new JavaScriptSerializer
            {
                MaxJsonLength = int.MaxValue
            };
            //dynamic ImgPdfFiles = jsJson.DeserializeObject(ImgPdfFile[0].ToString());
            //string imgData = ImgPdfFiles["imgData"];
            //string pdfData = ImgPdfFiles["pdfData"];
            string CustomerName = HttpUtility.UrlDecode(CustomerDataInfo[0]["CustomerName"]);
            string CID = HttpUtility.UrlDecode(CustomerDataInfo[0]["CustomerCode"]);
            string InvoiceData = "";
            string InvoiceNo = "";
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
            result = html.UploadFile(DestinationFolder);
            string AttachmentFile;
            if (result.Result == ResultType.Successful)
                AttachmentFile = result.Value;
            else
            {
                return Json(new
                {
                    Message = result.Message,
                    Result = result.Result,
                    Data = result.Value,
                    InvoiceData = InvoiceData,
                    CustomerName = CustomerName,
                    InvoiceNo = InvoiceNo,
                    SupportTicketData = ""
                });
            }
            string SystemID = html.SystemID;
            string WarehouseID = html.WarehouseID;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<Data>");
            sb.AppendLine("<CustomerAttachmentFileName>");
            sb.AppendLine("<AttachmentFileName>" + AttachmentFile + "</AttachmentFileName>");
            sb.AppendLine("<DestinationFolder>" + DestinationFolder + "</DestinationFolder>");
            sb.AppendLine("</CustomerAttachmentFileName>");
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
            sb.AppendLine("<RegisterDate>" + easternTimeNow + "</RegisterDate>");
            sb.AppendLine("<SystemID>" + SystemID + "</SystemID>");
            sb.AppendLine("<WarehouseID>" + WarehouseID + "</WarehouseID>");
            sb.AppendLine("<UserName>" + html.UserName + "</UserName>");
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
                sb.AppendLine("</CustomerInvoiceDetail>");
            }
            foreach (dynamic data in CustomerInvoiceOtherCharge)
            {
                sb.AppendLine("<CustomerOtherCharge>");
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
                //sb.AppendLine("<OtherChargeDate>" + easternTimeNow + "</OtherChargeDate>");
                sb.AppendLine("</CustomerOtherCharge>");
            }
            sb.AppendLine("</Data>");
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "Data", "N'"+sb.ToString()+"'" },
                { "ISSO", "1" }
            };
            SqlResult sqlresult = html.SaveChangeData("SP_INSERT_CUSTOMER", true, Para, true, true);
            InvoiceData = html.JsonConvertDataTable(sqlresult.Data.Tables[0]);
            string SupportTicketData = "";
            if (sqlresult.Data.Tables.Count > 1)
            {
                SupportTicketData = html.JsonConvertDataTable(sqlresult.Data.Tables[1]);
            }

            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value,
                InvoiceData = InvoiceData,
                CustomerName = CustomerName,
                InvoiceNo = InvoiceNo,
                SupportTicketData = SupportTicketData
            });
        }
        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> SendNotificationToTelegram(string CustomerName, string CustomerCode,
            int CustomerID, string InvoiceNo, string InvoiceDate, int IsIncludedVAT, string DescriptionTicketTelegram, string DescriptionTicketTelegramSupportTeam, int StatusID = 11)
        {
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            SqlResult sqlresult = new SqlResult();
            SqlResult result = new SqlResult();
            HTML html = new HTML();
            result = await html.InvoiceToPDFandUpload(CustomerName, CustomerCode, CustomerID, InvoiceNo, InvoiceDate, IsIncludedVAT, StatusID, int.Parse(html.SystemID.ToString()), int.Parse(html.WarehouseID.ToString()), false);
            string outPutFile = result.OutPutFile;

            await notificationTelegram.NotificationTelegramBot("", -635916546, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegram), true);

            sqlresult = await notificationTelegram.NotificationTelegramBot(HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam));
            return Json(new { Data = sqlresult.Message }, JsonRequestBehavior.AllowGet);
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
                sql += "from dbo.TblCustomer_SaleOrder c ";
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
                sql += "from dbo.TblCustomerBillInformation_SaleOrder cb ";
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
                sql += "from dbo.TblCustomerInvoice_SaleOrder where InvoiceNo = N'" + sqlresult.Value + "'";
                sqlresult.Value = html.JsonData(sql);
            }
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value
            });
        }

        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadContactName()
        {
            HTML html = new HTML();
            string SQL = "select Distinct ContactName from dbo.TblCustomer_SaleOrder where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and ContactName is not null order by ContactName";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadNationality()
        {
            HTML html = new HTML();
            string SQL = "select distinct CustomerNationality ";
            SQL += "from dbo.TblCustomer_SaleOrder where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerNationality";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveChangeCustInfo(object[] CustomerData)
        {
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            dynamic CustomerDataInfo = new JavaScriptSerializer().DeserializeObject(CustomerData[0].ToString());
            Dictionary<string, string> Para = new Dictionary<string, string>();
            string SQL = "update dbo.TblCustomer_SaleOrder set ";
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
            string SQL = "update dbo.TblCustomerCompany_SaleOrder set ";
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
            SQL += "update dbo.TblCustomer_SaleOrder set IndustryTypeID = @IndustryTypeID where CustomerID = @CustomerID;";
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
            string SQL = "update dbo.TblCustomerBillInformation_SaleOrder set ";
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
            sqlresult = html.SaveChangeData(SQL, false, Para);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadInvoiceTmp()
        {
            HTML html = new HTML();
            string SQL = "select CompanyID, InvoiceTemplateName from dbo.TblCompany where SystemID =" + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by case when ISDefault = 1 then 0 else OrderNumber end";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetCustomerLocationByGoogleMap()
        {

            return View();
        }

        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadCustPrefix()
        {
            HTML html = new HTML();
            string SQL = @" select case when (isnull(PrefixYear, 0)) <> 0 then Prefix + convert(varchar, format(CurrentNumber+1, FormatMask))
                            else Prefix + convert(varchar, format(StartNumber, FormatMask)) end Display, PrefixID from dbo.TblPrefix 
                            where Module = 'Customer' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CurrentNumber desc";
            return Json(new { data = html.JsonData(SQL) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Add Customer, Edit Customer")]
        public JsonResult LoadCustTypePrefix(string CustomerType)
        {
            HTML html = new HTML();
            StringBuilder sb = new StringBuilder();
            //sb.AppendLine("if not exists(select Prefix from dbo.TblPrefix where Module = 'Customer' and Prefix='" + CustomerType + "' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + ")");
            //sb.AppendLine("begin");
            sb.AppendLine("declare @Prefix nvarchar(50), @Module nvarchar(50), @StartNumber int;");
            sb.AppendLine("select @Prefix = Prefix, @Module = 'Customer', @StartNumber = isnull(StartNumber,1) from dbo.TblCustomerType where CustomerType = '" + CustomerType + "';");
            sb.AppendLine("exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ",'" + html.DomainID + "', @StartNumber;");
            sb.AppendLine("select @Prefix");
            //sb.AppendLine("select '" + CustomerType + "100001' PrefixNumber");
            //sb.AppendLine("end");
            //sb.AppendLine("else");
            //sb.AppendLine("begin");
            //sb.AppendLine("select Prefix + convert(varchar, format(CurrentNumber + 1, FormatMask)) PrefixNumber");
            //sb.AppendLine("from dbo.TblPrefix where Prefix='" + CustomerType + "' and Module = 'Customer' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CurrentNumber desc");
            //sb.AppendLine("end");                            
            return Json(new { data = html.JsonData(sb.ToString()) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Add Customer, Edit Customer")]
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

        public async Task<JsonResult> CancelPI(string invoiceNo, int customerId, string comment)
        {
            HTML html = new HTML();
            NotificationTelegram bot = new NotificationTelegram();
            SqlResult result = new SqlResult();
            string cancelByUser = html.UserFullName;
            string msg = "Dear Team, ";
            msg += $"<br><br>Now PI number <b>{invoiceNo}</b> has been cancel ❌";
            msg += $"<br>Cancel By: {cancelByUser}.";
            msg += $"<br><br>Comment: {comment}.";
            msg += $"<br><br>Thank you.";

            //Parameter
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "InvoiceNo", invoiceNo },
                { "CustomerID", customerId.ToString() },
                { "CancelBy", cancelByUser },
                { "CancelDate", html.GetCurrentDate() },
                { "Comment", comment }
            };

            //Get messageId pi 
            string sql = @"select top 1 isnull(cb.TelegramMessageID, 0) MessageID
					from dbo.TblCustomerInvoice ci
					inner join dbo.TblCustomerBalance cb on cb.CustomerID = ci.CustomerID and cb.InvoiceNo = ci.InvoiceNo 
					where ci.InvoiceNo = @InvoiceNo and ci.CustomerID = @CustomerID and ci.Type = 'Sale Order Invoice' and isnull(ci.ItemDescription,'') <> 'Deposit Fee';";
            DataSet ds = html.LoadData(sql, false, para);
            int messageId = Convert.ToInt32(ds.Tables[0].Rows[0]["MessageID"].ToString());

            //Sent msg to group top PI
            result = await bot.NotificationTelegramBotToTopic(msg, 1545132903, 54, null, messageId, "", "", "", false);

            if (result.Result == ResultType.Successful)
            {
                //Update status 
                sql = @"update dbo.TblCustomerInvoice set InvoiceStatusID = 94, InvoiceStatus = 'Cancel', CancelPIBy = @CancelBy, CancelPIDate = @CancelDate, CancelPIComment = @Comment
						where InvoiceNo = @InvoiceNo and CustomerID = @CustomerID and StatusID not in (12,13,14,20) and Type = 'Sale Order Invoice' and isnull(ItemDescription,'') <> 'Deposit Fee';
						update dbo.TblCustomerBalance set InvoiceStatusID = 94, InvoiceStatus = 'Cancel' where InvoiceNo = @InvoiceNo and CustomerID = @CustomerID;";
                result = html.SaveChangeData(sql, false, para);
            }

            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}