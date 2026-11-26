using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers.Customers.Reports
{
    //[Authorize]
    public class CustomerReportsController : Controller
    {
        //public CustomerReportsController(IPdfService pdfService)
        //{
        //	_pdfService = pdfService;
        //}
        //public CustomerReportsController(){ }
        // GET: CustomerReports
        [Authorize(Roles = "Customer Reports")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "cb.InvoiceDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "cc.ClassID",
                SQL_CBO = "select distinct cc.ClassID, cc.ClassName\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblLeads l on l.LeadID = cb.EmployeeID\r\ninner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.StatusID not in (12,13,14,20) and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "ClassName",
                Value = "ClassID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Invoice No",
                SQL = "select distinct top 10 cb.InvoiceNo\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblLeads l on l.LeadID = cb.EmployeeID\r\ninner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.StatusID not in(12,13,14,20) and lower(cb.InvoiceNo) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%'",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("cb.InvoiceNo"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = @"select ROW_NUMBER() over(order by cb.InvoiceNo) as [No],
('<a class=""text-primary"" href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ cb.InvoiceNo +''','''+ convert(varchar, cb.IsIncludedVAT) +''','''+ convert(varchar, cb.StatusID) +''','''',false,this);"">' + cb.InvoiceNo + '</a>') InvoiceNo,
l.FullName, cc.ClassName,
'<a class=""text-primary"" href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a>' CustomerCode, c.CustomerName, format(cb.InvoiceDate, 'dd-MM-yy') InvoiceDate, format(cb.PaymentDueDate, 'dd-MM-yy') PaymentDueDate, cb.GrandTotal, isnull(cb.CommentInvoice,'') Memo, isnull(c.CustomerNoted, '') Remark,
c.CustomerID, format(cb.InvoiceDate, 'yyyyMMdd') + '-'+ c.CustomerCode +'_'+ REPLACE(cb.InvoiceNo,'/','') + '.pdf' InvoiceFileName
from dbo.TblCustomer c
inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID
inner join dbo.TblLeads l on l.LeadID = cb.EmployeeID
inner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.StatusID not in(12,13,14,20)";
            html.OrderBy = "cb.InvoiceNo";
            html.GH.SQL = sql;

            html.GH.GridPara = new GridPara[]{
                new GridPara() { ParaName="cb.SystemID", Value=html.SystemID},
                new GridPara() { ParaName="cb.WarehouseID", Value=html.WarehouseID},
                new GridPara() { ParaName="cb.DomainID", Value=html.DomainID}
            };

            html.IsHaveWhereCondition = true;
            html.ShowPaging = false;
            html.ShowPrintOnTopTable = false;
            //html.UsingJqueryDataTable = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo" },
                new GridField() { Header = "Sale​​", FieldName = "FullName" },
                new GridField() { Header = "Class", FieldName = "ClassName" },
                new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
                new GridField() { Header = "Inv. Date", FieldName = "InvoiceDate" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", TotalLabel="Total:" },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", TextAlign = "right", ClassName="text-danger text-right", Format="#,#0.00", IsTotalFooter = true },
                new GridField() { Header = "Invoice File Name", FieldName = "InvoiceFileName" },
				//new GridField() { Header = "Memo", FieldName = "Memo" },
				//new GridField() { Header = "Remark", FieldName = "Remark" },
			};
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                filterable = false,
                ShowToolbarKendoGrid = false
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }

        [Authorize(Roles = "Connection Reports")]
        public ActionResult Connection()
        {
            HTML html = new HTML();
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "cb.InvoiceDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class:",
                DBField = "cc.ClassID",
                SQL_CBO = "select distinct cc.ClassID, cc.ClassName\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblLeads l on l.LeadID = cb.EmployeeID\r\ninner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "ClassName",
                Value = "ClassID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Group:",
                DBField = "c.CustomerGroupID",
                SQL_CBO = "select distinct cg.CustomerGroupID, cg.CustomerGroup\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID where cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroupID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "c.CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID where cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Invoice No",
                SQL = "select distinct top 10 cb.InvoiceNo\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblLeads l on l.LeadID = cb.EmployeeID\r\ninner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.StatusID not in(12,13,14,20) and lower(cb.InvoiceNo) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%'",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("cb.InvoiceNo") + "," + EncryAndDecry.Encryp("c.CustomerCode"),
                DefaultCheckBok = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("cb.InvoiceNo") + "," + EncryAndDecry.Encryp("c.CustomerCode"),
                ISAutoComplete = true,
            });

            string sql = @"select ROW_NUMBER() over(order by cb.CustomerBalanceID desc, cc.ClassName, cg.CustomerGroup, ct.CustomerType) as [No],
('<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ cb.InvoiceNo +''','''+ convert(varchar, cb.IsIncludedVAT) +''','''+ convert(varchar, cb.StatusID) +''','''',false,this);"">' + cb.InvoiceNo + '</a>') + 
case when isnull(ci.ApproveRequestToDMCTAXStatus,'') = '' then '<br><button type=""button"" name=""BTNTODMC"" value=""' + cb.InvoiceNo + '"" class=""btn btn-warning btn-sm text-white mt-3"" onclick=""AddQuotation({ PrimaryFieldValue: 0, InvoiceNo: '''+cb.InvoiceNo+''', CustomerID: '+convert(varchar, c.CustomerID) +', FormStatus: ''ToDMC'' }, ''Invoice To DMC & TAX System'');"">TO DMC & TAX SYS.</button>'
when ci.ApproveRequestToDMCTAXStatus ='Pending' then '<br><span class=""btn btn-info btn-sm mt-3"">Waiting Approval</span>'
else '<br><span onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ cb.InvoiceNo +''','''+ convert(varchar, cb.IsIncludedVAT) +''','''+ convert(varchar, cb.StatusID) +''','''',false,this,'''',0,'''','+convert(varchar, isnull(cb.CustIDToDMC, 0))+','''+ isnull(cb.InvoiceToDMC,'') +''');"" class=""btn btn-primary"">DONE TO DMC</span>'
end
InvoiceNo,
cc.ClassName, cg.CustomerGroup, ct.CustomerType,
c.CustomerCode + '<br>'+ c.CustomerName CustomerCode, c.CustomerName, 
ac.ConnectionID, ac.ConnectionID + '<br>' + ac.[SID] + '<br>'+ case when ss.StatusID <> 40 then '<span class=""badge bg-danger"">' + ss.[Status]  + '</span>' else '<span class=""badge bg-primary"">' + ss.[Status]  + '</span>' end [SID], ac.PPPoEProfileName PackageName,
case when ac.PPPoEProfileName <> '' then ac.PPPoEProfileName + '<br>' else '' end + format(ac.StartDate, 'dd-MMM-yy') +' - '+
format(ac.EndDate, 'dd-MMM-yy') + '<br>' + convert(varchar, DATEDIFF(d, ac.StartDate, ac.EndDate)) + ' Days, ' + convert(varchar, DATEDIFF(m, ac.StartDate, ac.EndDate)) + ' Months <br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), ac.EndDate) > 0 then case when DATEDIFF(d, getdate(), ac.StartDate) <= 0 then 'used ' + convert(varchar, case when DATEDIFF(day, ac.StartDate, GETDATE()) > 0 then convert(varchar, DATEDIFF(day, ac.StartDate, GETDATE())) + ' days, ' + convert(varchar, DATEDIFF(d, getdate(), ac.EndDate)) + ' day left' else '0' end) else 'Expired' end else 'Expired' end + '</b></span>'
[Date], cb.InvoiceNo InvNo,
cb.InvoiceDate, cb.PaymentDueDate, cb.GrandTotal, case when s.StatusID <> 10 then '<span class=""badge bg-danger"">' + s.[Status]  + '</span>' else '<span class=""badge bg-primary"">' + s.[Status]  + '</span>' end [Status],
isnull(cb.CommentInvoice,'') Memo, isnull(c.CustomerNoted, '') Remark,
c.CustomerID, ss.[Status] ConnectivityStatus
from dbo.TblCustomer c
inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = cb.CustomerID and ci.InvoiceNo = cb.InvoiceNo
inner join dbo.TblLeads l on l.LeadID = cb.EmployeeID
inner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID 
inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID and ac.InvoiceNo = cb.InvoiceNo
inner join dbo.TblStatus s on s.StatusID = cb.StatusID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblStatus ss on ss.StatusID = ac.ConnectivityStatus
where cb.[Type]='Official Invoice' and cb.StatusID not in(12,13,14,20) and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + @"
group by cb.InvoiceNo, cc.ClassName, cg.CustomerGroup, ct.CustomerType, c.CustomerCode, c.CustomerName,
cb.ConnectionID, ac.[SID], ac.PPPoEProfileName, cb.CustomerBalanceID, c.CustomerID,
cb.IsIncludedVAT,cb.StatusID,ci.ApproveRequestToDMCTAXStatus,cb.CustIDToDMC,
cb.InvoiceToDMC, ac.ConnectionID, ss.[Status],s.StatusID, ss.StatusID,
ac.StartDate, ac.EndDate, cb.InvoiceDate, cb.PaymentDueDate,
cb.GrandTotal, s.[Status], cb.CommentInvoice, c.CustomerNoted, cc.ClassID, c.CustomerGroupID,
c.CustomerTypeID";

            html.OrderBy = "cb.CustomerBalanceID desc, cc.ClassName, cg.CustomerGroup, ct.CustomerType";
            html.GH.SQL = sql;
            //html.IsHaveWhereCondition = true;
            html.IsUseHavingCondition = true;
            //html.ShowPaging = false;
            //html.ShowPrintOnTopTable = false;
            //html.UsingJqueryDataTable = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Group", FieldName = "CustomerGroup", IsGroupByField = true, GroupLabel="Group: ", Colspan=15 },
                new GridField() { Header = "Block", FieldName = "CustomerType", IsGroupByField = true, GroupLabel="Block: ", Colspan=15 },
                new GridField() { Header = "Class", FieldName = "ClassName", IsGroupByField = true, GroupLabel="Class: ", Colspan=15 },
				//new GridField() { Header = "Check", FieldName = "InvNo", DisplayCheckBox=true },
				new GridField() { Header = "Invoice#", FieldName = "InvoiceNo", TextAlign = "center" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
				//new GridField() { Header = "Customer Name", FieldName = "CustomerName", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
				//new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
				new GridField() { Header = "Connection", FieldName = "SID" },
				//new GridField() { Header = "Package Name", FieldName = "PackageName" },
				new GridField() { Header = "Package Name", FieldName = "Date" },
                new GridField() { Header = "Inv. Date", FieldName = "InvoiceDate", Format="dd-MM-yy" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", Format = "dd-MM-yy" },
                new GridField() { Header = "Total", FieldName = "GrandTotal", TextAlign = "right", ClassName="text-danger" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Remark", FieldName = "Remark" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Connection By Group")]
        public ActionResult ConnectionByGroup()
        {
            HTML html = new HTML();
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "cb.InvoiceDate"
            });
            return View();
        }

        [Authorize(Roles = "Customer Commission")]
        public ActionResult CustomerCommission()
        {
            HTML html = new HTML();
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "ci.InvoiceDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ci.ClassNameID",
                SQL_CBO = "select distinct cc.ClassID, cc.ClassName\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblLeads l on l.LeadID = cb.EmployeeID\r\ninner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "ClassName",
                Value = "ClassID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBlock",
                Label = "Block:",
                DBField = "c.CustomerTypeID",
                SQL_CBO = @"select distinct ct.CustomerTypeID, CustomerType from dbo.TblCustomer c
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + @"%'
order by CustomerType",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Invoice No",
                SQL = "select distinct top 10 cb.InvoiceNo\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblLeads l on l.LeadID = cb.EmployeeID\r\ninner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.StatusID not in(12,13,14,20) and lower(cb.InvoiceNo) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%'",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ci.InvoiceNo"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = @"select row_number() over(order by isnull(ai.InvoiceDate, '') desc) as [No],
ai.InvoiceNo, ct.CustomerType Block, c.CustomerCode CID, case when isnull(c.CustomerName, '') <> '' then c.CustomerName else c.CustomerNameLatin end as CustomerName, ai.InvoiceDate, ai.InvoiceAmount,
ac.FullName AgencyName, ac.HP, ac.CommissionAmountUSD, ac.CommissionAmountP,
(select STUFF((select (',' + agt.AgencyCommissionType)
from dbo.TblAgencyCommission acc
CROSS APPLY STRING_SPLIT(isnull(acc.CommissionType,''), ',')
inner join dbo.TblAgencyCommissionType agt on agt.AgencyCommissionTypeID = value
where acc.AgencyCommissionID = ac.AgencyCommissionID
FOR XML PATH('')), 1, 1, '')) AgencyCommissionType, l.FullName, cl.ClassName, ac.CustomerID
from dbo.TblAgencyCommission ac
inner join dbo.TblAgencyInvoice ai on ai.AgencyCommissionID = ac.AgencyCommissionID
inner join dbo.TblCustomer c on c.CustomerID = ai.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID and cb.InvoiceNo = ai.InvoiceNo and cb.[Type] = 'Official Invoice'
inner join dbo.TblLeads l on l.LeadID = c.EmployeeID
inner join dbo.TblClass cl on cl.ClassID = cb.ClassNameID
where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + "%'";
            html.OrderBy = "ai.InvoiceDate desc";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            //html.ShowPaging = false;
            //html.ShowPrintOnTopTable = false;
            //html.UsingJqueryDataTable = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo", OnClickFunction="PrintInvoice(#=CustomerID#,'#=InvoiceNo#','1','10','',false,this);" },
                new GridField() { Header = "Block", FieldName = "Block" },
                new GridField() { Header = "CID", FieldName = "CID", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
                new GridField() { Header = "Sale​​", FieldName = "FullName" },
                new GridField() { Header = "Class", FieldName = "ClassName" },
                new GridField() { Header = "Inv. Date", FieldName = "InvoiceDate", FormatDateTime="dd-MM-yy", TextAlign = "center" },
				//new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", FormatDateTime = "dd-MM-yy", TotalLabel = "Total", TextAlign = "center" },
				new GridField() { Header = "Invoice Amount", FieldName = "InvoiceAmount", TextAlign = "right", ClassName="text-danger text-right", IsTotalFooter = true, Format="#,#.00" },
                new GridField() { Header = "Commission $", FieldName = "CommissionAmountUSD", TextAlign ="right", ClassName="text-primary text-right", IsTotalFooter = true, Format="#,#.00" },
                new GridField() { Header = "Commission %", FieldName = "CommissionAmountP", TextAlign ="right", ClassName="text-primary text-right", IsTotalFooter = true, Format="#,#.00" },
                new GridField() { Header = "Agency Name", FieldName = "AgencyName" },
                new GridField() { Header = "Agency Phone", FieldName = "HP" },

                new GridField() { Header = "Type", FieldName = "AgencyCommissionType" },
            };
            //html.UsingJqueryDataTable = true;
            html.ShowPaging = false;
            html.ShowGridOnly = true;
            //html.JqueryDataTableResponsive = false;
            KendoGridSetting kendoGridSetting = new KendoGridSetting()
            {
                filterable = true,
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }

        [Authorize(Roles = "Invoice To DMC TAX")]
        public ActionResult InvoiceToDMCTAX()
        {
            HTML html = new HTML();
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "ci.InvoiceDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class:",
                DBField = "cs.ClassID",
                SQL_CBO = "select distinct cc.ClassID, cc.ClassName\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblLeads l on l.LeadID = cb.EmployeeID\r\ninner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID,
                DisplayText = "ClassName",
                Value = "ClassID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Group:",
                DBField = "c.CustomerGroupID",
                SQL_CBO = "select distinct cg.CustomerGroupID, cg.CustomerGroup\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID where cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID,
                DisplayText = "CustomerGroup",
                Value = "CustomerGroupID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "c.CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID where cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID,
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Invoice No",
                SQL = "select distinct top 10 cb.InvoiceNo\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblLeads l on l.LeadID = cb.EmployeeID\r\ninner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.StatusID not in(12,13,14,20) and lower(cb.InvoiceNo) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID,
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("ci.InvoiceNo") + "," + EncryAndDecry.Encryp("c.CustomerCode"),
                DefaultCheckBok = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("ci.InvoiceNo") + "," + EncryAndDecry.Encryp("c.CustomerCode"),
                ISAutoComplete = true,
            });

            string sql = @"select ROW_NUMBER() over (order by ci.CustomerInvoiceID desc) as [No],
ci.CustomerID, c.CustomerCode, c.CustomerName, c.CustomerMobile,
cg.CustomerGroup, cs.ClassName, ct.CustomerType,
('<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ cb.InvoiceNo +''','''+ convert(varchar, cb.IsIncludedVAT) +''','''+ convert(varchar, cb.StatusID) +''','''',false,this);"">' + ci.InvoiceNo + '</a>') +
case when isnull(ci.ApproveRequestToDMCTAXStatus,'') = '' then '<br><button type=""button"" name=""BTNTODMC"" value=""' + cb.InvoiceNo + '"" class=""btn btn-warning btn-sm text-white mt-3"" onclick=""AddQuotation({ PrimaryFieldValue: 0, InvoiceNo: '''+cb.InvoiceNo+''', CustomerID: '+convert(varchar, c.CustomerID) +', FormStatus: ''ToDMC'' }, ''Invoice To DMC & TAX System'');"">TO DMC & TAX SYS.</button>'
when ci.ApproveRequestToDMCTAXStatus ='Pending' then '<br><span class=""btn btn-primary btn-sm mt-3"" onclick=""ApproveInvoiceToDMC('+convert(varchar, c.CustomerID)+', '''+ci.InvoiceNo+''', ''Approve'');"" >Approve</span> <span class=""btn btn-danger btn-sm mt-3"" onclick=""ApproveInvoiceToDMC('+convert(varchar, c.CustomerID)+', '''+ci.InvoiceNo+''', ''Disapprove'');"" >Disapprove</span>'
when isnull(ci.ApproveRequestToDMCTAXStatus,'') = 'Disapprove' then
'<br><span class=""btn btn-primary btn-sm mt-3"" onclick=""ApproveInvoiceToDMC('+convert(varchar, c.CustomerID)+', '''+ci.InvoiceNo+''', ''Approve'');"" >Disapproved</span>'
else '<br><span onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ cb.InvoiceNo +''','''+ convert(varchar, cb.IsIncludedVAT) +''','''+ convert(varchar, cb.StatusID) +''','''',false,this,'''',0,'''','+convert(varchar, isnull(cb.CustIDToDMC, 0))+','''+ isnull(cb.InvoiceToDMC,'') +''');"" class=""btn btn-sm btn-primary mt-3"">DONE TO DMC & TAX</span>'
end InvoiceNo,
case when isnull(ci.[SID], '') <> '' then isnull(ci.[SID], '') + '<br>' else '' end + isnull(ci.ConnectionID,'') + 
'<br>' + case when ci.ConnectivityStatus = 40 then '<span class=""badge bg-primary"">' else '<span class=""badge bg-danger"">' end  + sc.[Status] + '</span>'
Connection,
replace(ci.ItemDescription, 'Internet Service - ','') + '<br>'+ format(ci.StartDate, 'dd-MMM-yy') +' - '+
format(ci.EndDate, 'dd-MMM-yy') + '<br>' + convert(varchar, DATEDIFF(d, ci.StartDate, ci.EndDate)) + ' Days, ' + convert(varchar, DATEDIFF(m, ci.StartDate, ci.EndDate)) + ' Months <br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), ci.EndDate) > 0 then case when DATEDIFF(d, getdate(), ci.StartDate) <= 0 then 'used ' + convert(varchar, case when DATEDIFF(day, ci.StartDate, GETDATE()) > 0 then convert(varchar, DATEDIFF(day, ci.StartDate, GETDATE())) + ' days, ' + convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + ' day left' else '0' end) else 'Expired' end else 'Expired' end + '</b></span>'
[Date], ci.InvoiceDate, ci.PaymentDueDate, cb.GrandTotal, case when cb.StatusID <> 10 then '<span class=""badge bg-danger"">' + s.[Status]  + '</span>' else '<span class=""badge bg-primary"">' + s.[Status]  + '</span>' end [Status],
sc.[Status] ConnectivityStatus
from dbo.TblCustomerInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblClass cs on cs.ClassID = ci.ClassNameID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblCustomerBalance cb on cb.CustomerID = ci.CustomerID and cb.InvoiceNo = ci.InvoiceNo
inner join dbo.TblStatus s on s.StatusID = cb.StatusID
inner join dbo.TblStatus sc on sc.StatusID = ci.ConnectivityStatus
where ci.ServiceName <> 'Deposit Fee' and ci.StatusID not in (12,13,14,20) 
and ci.CustomerCode is not null";//and ci.ServiceID is not null//and isnull(ci.ItemID,0) = 0

            html.OrderBy = "ci.CustomerInvoiceID desc";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            //html.ShowPaging = false;
            //html.ShowPrintOnTopTable = false;
            //html.UsingJqueryDataTable = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Group", FieldName = "CustomerGroup", IsGroupByField = true, GroupLabel="Group: ", Colspan=15 },
                new GridField() { Header = "Block", FieldName = "CustomerType", IsGroupByField = true, GroupLabel="Block: ", Colspan=15 },
                new GridField() { Header = "Class", FieldName = "ClassName", IsGroupByField = true, GroupLabel="Class: ", Colspan=15 },
				//new GridField() { Header = "Check", FieldName = "InvNo", DisplayCheckBox=true },
				new GridField() { Header = "Invoice#", FieldName = "InvoiceNo", TextAlign = "center" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
				//new GridField() { Header = "Customer Name", FieldName = "CustomerName", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
				//new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
				new GridField() { Header = "Connection", FieldName = "Connection" },
				//new GridField() { Header = "Package Name", FieldName = "PackageName" },
				new GridField() { Header = "Package Name", FieldName = "Date" },
                new GridField() { Header = "Inv. Date", FieldName = "InvoiceDate", Format="dd-MM-yy" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", Format = "dd-MM-yy" },
                new GridField() { Header = "Total", FieldName = "GrandTotal", TextAlign = "right", ClassName="text-danger" },
                new GridField() { Header = "Status", FieldName = "Status" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public async Task<JsonResult> ApprovalInvoiceToDMCTAX(int CustomerID, string InvoiceNo, string From, string Status, string Comment = "", bool IsNoNeedApproval = false, bool isInternetServices = true, HttpContext httpContext = null, int MessageID = 0)
        {
            SqlResult sqlResult = new SqlResult();
            SqlResult DataResult = new SqlResult();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            string ApproveRequestToDMCTAXTGMID = "0";
            string ApproveRequestToDMCTAXMsg = "";
            string msg = "Dear Team,<br><br>";
            HTML html = new HTML();
            string sql = @"select c.CustomerCode, c.CustomerName, c.CustomerID, b.InvoiceNo, b.InvoiceDate, b.IsIncludedVAT, b.StatusID, b.SystemID, b.WarehouseID, b.DomainID
from dbo.TblCustomerBalance b
inner join dbo.TblCustomer c on c.CustomerID = b.CustomerID
where b.CustomerID = @CustomerID and b.InvoiceNo = @InvoiceNo";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"CustomerID",CustomerID.ToString() },
                {"InvoiceNo",InvoiceNo },
            };
            DataSet ds = new DataSet();
            sqlResult.Data = html.LoadData(sql, false, para);
            bool isMatchApproval = false;
            if (IsNoNeedApproval)
                isMatchApproval = true;

            if (!IsNoNeedApproval)
            {
                sql = @"select RequestApprovalToDMCTAXTgUser, u.FullName, ci.ApproveRequestToDMCTAXMsg, ci.ApproveRequestToDMCTAXTGMID from dbo.TblCustomerInvoice ci
inner join dbo.TblUserLogin u on u.UserLoginID = ci.RequestApprovalToDMCTAXID
where ci.CustomerID = @CustomerID and ci.InvoiceNo = @InvoiceNo and 
ci.RequestApprovalToDMCTAXTgUser = case when @RequestApprovalToDMCTAXTgUser <> '' then '@'+@RequestApprovalToDMCTAXTgUser else RequestApprovalToDMCTAXTgUser end";
                From = From.Replace("@", string.Empty);
                //From = "DaNeth077";
                para = new Dictionary<string, string>
                {
                    {"RequestApprovalToDMCTAXTgUser",From },
                    {"CustomerID",CustomerID.ToString() },
                    {"InvoiceNo",InvoiceNo },
                };
                ds = new DataSet();
                ds = html.LoadData(sql, false, para);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    isMatchApproval = true;
                    ApproveRequestToDMCTAXTGMID = ds.Tables[0].Rows[0]["ApproveRequestToDMCTAXTGMID"].ToString();
                    ApproveRequestToDMCTAXMsg = ds.Tables[0].Rows[0]["ApproveRequestToDMCTAXMsg"].ToString();
                    string approvalusername = From == "" ? html.UserFullName : ds.Tables[0].Rows[0]["FullName"].ToString();
                    if (Status == "Approve")
                    {
                        msg += "Now, Invoice#" + InvoiceNo + " for DMC and TAX System is approved ✅ by " + approvalusername;
                        msg += "<br>And uploaded to DMC FTP already.";
                    }
                    else
                        msg += "Now, Invoice#" + InvoiceNo + " for DMC and TAX System is disapproved ❌ by " + approvalusername + ". Please revise this invoice.";
                    if (Comment != "")
                        msg += "<br><br>Comment: " + Comment;
                    msg += "<br><br>Thank you and best regards,<br><br>";
                    msg += "NSM ISP BILLING SYSTEM AUTO SEND Request approve Invoice to DMC & TAX System. #" + InvoiceNo;

                    sql = @"update dbo.TblCustomerInvoice set ApproveRequestToDMCTAXTGMID = case when @ApproveRequestToDMCTAXTGMID = 0 then ApproveRequestToDMCTAXTGMID else @ApproveRequestToDMCTAXTGMID end,
ApproveRequestToDMCTAXStatus = @ApproveRequestToDMCTAXStatus, RequestApprovalToDMCTAX = @RequestApprovalToDMCTAX,
ApproveRequestToDMCTAXMsg = @ApproveRequestToDMCTAXMsg
where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo;
update dbo.TblCustomerBalance set ApproveRequestToDMCTAXTGMID = case when @ApproveRequestToDMCTAXTGMID = 0 then ApproveRequestToDMCTAXTGMID else @ApproveRequestToDMCTAXTGMID end,
ApproveRequestToDMCTAXStatus = @ApproveRequestToDMCTAXStatus, RequestApprovalToDMCTAX = @RequestApprovalToDMCTAX,
ApproveRequestToDMCTAXMsg = @ApproveRequestToDMCTAXMsg
where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo;
select c.CustomerCode, c.CustomerName, c.CustomerID, b.InvoiceNo, b.InvoiceDate, b.IsIncludedVAT, b.StatusID, b.SystemID, b.WarehouseID, b.DomainID
from dbo.TblCustomerBalance b
inner join dbo.TblCustomer c on c.CustomerID = b.CustomerID
where b.CustomerID = @CustomerID and b.InvoiceNo = @InvoiceNo
";
                    if (Comment != "")
                        sql += @"update dbo.TblCustomerBalance set CommentInvoice  = @CommentInvoice where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo;";
                    para = new Dictionary<string, string>
                    {
                        {"RequestApprovalToDMCTAX",From == "" ? html.UserFullName : From },
                        {"ApproveRequestToDMCTAXTGMID",ApproveRequestToDMCTAXTGMID },
                        {"ApproveRequestToDMCTAXStatus",Status },
                        {"CustomerID",CustomerID.ToString() },
                        {"InvoiceNo",InvoiceNo },
                        {"ApproveRequestToDMCTAXMsg",msg },
                        {"CommentInvoice",Comment },
                    };
                    sqlResult = html.SaveChangeData(sql, false, para, true, true);
                }
            }
            if (isMatchApproval)
            {
                if (Status == "Approve")
                {
                    sql = "select c.CustomerCode, c.CustomerName, c.CustomerID, b.InvoiceNo, b.InvoiceDate, b.IsIncludedVAT, b.StatusID, b.SystemID, b.WarehouseID, b.DomainID\r\nfrom dbo.TblCustomerBalance b\r\ninner join dbo.TblCustomer c on c.CustomerID = b.CustomerID\r\nwhere b.CustomerID = " + CustomerID + " and b.InvoiceNo = '" + InvoiceNo + "'";
                    DataRow dr = html.LoadData(sql).Tables[0].Rows[0];
                    string CustomerName = dr["CustomerName"].ToString();
                    string CustomerCode = dr["CustomerCode"].ToString();
                    string InvoiceDate = dr["InvoiceDate"].ToString();
                    int IsIncludedVAT = dr["IsIncludedVAT"].ToString() == "True" ? 1 : 0;
                    int StatusID = int.Parse(dr["StatusID"].ToString());
                    string SystemID = dr["SystemID"].ToString();
                    string WarehouseID = dr["WarehouseID"].ToString();
                    string DomainID = dr["DomainID"].ToString();
                    sql = @"declare @SID nvarchar(50), @ConnectionID nvarchar(50), @ActivateCustomerID int, @EndDate datetime, @RenewBy nvarchar(50), @RenewDate datetime, @OldInvoiceNo nvarchar(50), @ConnectivityStatus nvarchar(50) = '43', @TicketNumber nvarchar(50);
select @ActivateCustomerID = ActivateCustomerID, @SID = [SID], @ConnectionID = ConnectionID, @EndDate = EndDate, @RenewBy = RenewBy, @RenewDate = RenewDate, @OldInvoiceNo = OldInvoiceNo from dbo.TblCustomerBillingInvoice where CustomerID = " + CustomerID + " and InvoiceNo = '" + dr["InvoiceNo"].ToString() + @"';
select @TicketNumber = TicketNumber from dbo.TblCustomerBillingInvoice where CustomerID = " + CustomerID + @" and InvoiceNo = @OldInvoiceNo;
select @ConnectivityStatus = ConnectivityStatus from dbo.TblActivateCustomer where ActivateCustomerID = @ActivateCustomerID;
update dbo.TblCustomerBalance set TicketNumber = case when isnull(@TicketNumber, '') <> '' then @TicketNumber else TicketNumber end, DomainID='001,002', IsToDMCTAX = 1, CustIDToDMC=" + CustomerID + ", InvoiceToDMC='" + InvoiceNo + "' where CustomerID = " + CustomerID + " and InvoiceNo='" + dr["InvoiceNo"].ToString() + @"';
update dbo.TblCustomerInvoice set DomainID='001,002', IsToDMCTAX = 1 where CustomerID = " + CustomerID + " and InvoiceNo='" + dr["InvoiceNo"].ToString() + @"';
update dbo.TblCustomerBillingInvoice set TicketNumber = case when isnull(@TicketNumber, '') <> '' then @TicketNumber else TicketNumber end, ConnectivityStatus = @ConnectivityStatus, DomainID='001,002', IsToDMCTAX = 1 where CustomerID = " + CustomerID + " and InvoiceNo='" + dr["InvoiceNo"].ToString() + @"';
update dbo.TblCustomerPayment set DomainID='001,002' where CustomerID = " + CustomerID + " and InvoiceNo='" + dr["InvoiceNo"].ToString() + @"';
update dbo.TblCustomer set DomainID='001,002', IsToDMCTAX = 1 where CustomerID = " + CustomerID + @";
update dbo.TblCustomerBillInformation set DomainID='001,002' where CustomerID = " + CustomerID + " and InvoiceNo='" + dr["InvoiceNo"].ToString() + @"';
update dbo.TblCustomerAccount set DomainID='001,002' where CustomerID = " + CustomerID + " and InvoiceNo='" + dr["InvoiceNo"].ToString() + @"';
update dbo.TblCustomerCompany set DomainID='001,002' where CustomerID = " + CustomerID + @";

update dbo.TblActivateCustomer set [SID] = @SID, ConnectionID = @ConnectionID, DomainID = '001,002', InvoiceNo = '" + dr["InvoiceNo"].ToString() + @"', EndDate = @EndDate, RenewBy = @RenewBy, RenewDate = @RenewDate where ActivateCustomerID = @ActivateCustomerID;
update dbo.TblCustomerInvoice set [SID] = @SID, ConnectionID = @ConnectionID where InvoiceNo = @OldInvoiceNo and CustomerID = " + CustomerID + @"
update dbo.TblCustomerBillingInvoice set [SID] = @SID, ConnectionID = @ConnectionID where InvoiceNo = @OldInvoiceNo and CustomerID = " + CustomerID + "";
                    html.SaveChangeData(sql, false, null, false, false, html.GetConnectionString());

                    CustomerName = dr["CustomerName"].ToString();
                    CustomerCode = dr["CustomerCode"].ToString();
                    sqlResult = await html.InvoiceToPDFandUpload(CustomerName, CustomerCode, CustomerID, InvoiceNo, InvoiceDate, IsIncludedVAT, StatusID, int.Parse(SystemID.ToString()), int.Parse(WarehouseID.ToString()), IsNoNeedApproval == false, "", "/ISP_Invoice");
                    string outPutFile = sqlResult.OutPutFile;

                    if (IsNoNeedApproval)
                    {
                        msg = "Dear Team,<br><br>";
                        msg += "Please check attachment file that is invoice for TAX System and NO NEED APPROVAL.";
                        msg += "<br><br>Thank you and best regards,<br><br>";
                        msg += "NSM ISP BILLING SYSTEM AUTO SEND Request approve Invoice to DMC & TAX System. #" + InvoiceNo;
                        sqlResult = await notificationTelegram.NotificationTelegramBot("", -1002042086477, "", outPutFile, msg, true, true, 0, MessageID, ApproveRequestToDMCTAXMsg, null, true);
                    }
                    else
                        sqlResult = await notificationTelegram.NotificationTelegramBot("", -1002042086477, "", outPutFile, msg, true, true, 0, MessageID, ApproveRequestToDMCTAXMsg, null, true);

                    if (!IsNoNeedApproval)
                    {
                        sql = @"select DescriptionTicketTelegram, TicketNumber, InvoiceType, IsServicesOrProduct, CreateBy from dbo.TblCustomerBalance where CustomerID = " + CustomerID + " and InvoiceNo = '" + InvoiceNo + "'";
                        DataSet ds2 = html.LoadData(sql);
                        if (ds2.Tables[0].Rows.Count > 0)
                        {
                            DataRow drs = ds2.Tables[0].Rows[0];
                            string CreateBy = drs["CreateBy"].ToString();
                            string InvoiceType = drs["InvoiceType"].ToString();
                            string IsServicesOrProduct = drs["IsServicesOrProduct"].ToString();
                            string DescriptionTicketTelegramSupportTeam = ds2.Tables[0].Rows[0]["DescriptionTicketTelegram"].ToString();
                            if (InvoiceType != "Renew Invoice")
                            {
                                if (IsServicesOrProduct != "customer products")
                                {
                                    //dr["DescriptionTicketTelegram"].ToString();
                                    string TicketNumber = ds2.Tables[0].Rows[0]["TicketNumber"].ToString();
                                    //send notification to osp team
                                    long TelegramGroupIDOSP = -1002161346708;

                                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new []
                                        {
                                            InlineKeyboardButton.WithCallbackData(text: "▶️ Start Lay Cable", callbackData: "Start Lay Cable ~ "+ CustomerID.ToString()+"~"+TicketNumber+"~"+TelegramGroupIDOSP.ToString()+"~0~0"),
                                        }
                                    });
                                    sql = @"SELECT af.DestinationFolder + '/'+ value AS [FileName], isnull(af.TypeFor, '') TypeFor, af.InvoiceNo
FROM dbo.TblCustomerAttachmentFile af
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = af.CustomerID and af.InvoiceNo = @InvoiceNo
CROSS APPLY STRING_SPLIT(AttachmentFileName, '~') where af.CustomerID = @CustomerID 
and ci.InvoiceNo = @InvoiceNo and isnull(af.TypeFor, '') in('Attachment File','AttachmentFile')";
                                    para = new Dictionary<string, string>
                                    {
                                        { "CustomerID", CustomerID.ToString() },
                                        { "InvoiceNo", InvoiceNo },
                                    };

                                    ds = html.LoadData(sql, false, para);
                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        string attfile = "", TypeFor = "";
                                        foreach (DataRow dr1 in ds.Tables[0].Rows)
                                        {
                                            if (attfile == "")
                                            {
                                                attfile = dr1["FileName"].ToString();
                                                TypeFor = dr1["TypeFor"].ToString();
                                            }
                                            else
                                            {
                                                attfile += "~" + dr1["FileName"].ToString();
                                                TypeFor += "~" + dr1["TypeFor"].ToString();
                                            }
                                        }
                                        string outPutFiles = attfile;
                                        string Captions = TypeFor;

                                        sqlResult = await notificationTelegram.NotificationTelegramBot(HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam), TelegramGroupIDOSP, "", outPutFiles, Captions, false, true, 0, 0, "", inlineKeyboard);
                                    }
                                    else
                                        sqlResult = await notificationTelegram.NotificationTelegramBot(HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam), TelegramGroupIDOSP, "", "", "", false, true, 0, 0, "", inlineKeyboard);
                                    string TelegramGroupNameAndMessageID = TelegramGroupIDOSP.ToString() + "|" + sqlResult.Value;
                                    ///OSP Team New Install Internet Services
                                    //send notification to technical support
                                    int TelegramGroupIDTS = -1903890245;
                                    int TopicID = 7;
                                    inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new []
                                        {
                                            InlineKeyboardButton.WithCallbackData(text: "▶️ Start Install", callbackData: "Start Install ~ "+ CustomerID.ToString()+"~"+TicketNumber+"~"+TelegramGroupIDTS.ToString()+"~Start Install~TECH"),
                                        },
                                    });
                                    //sqlresult = await notificationTelegram.NotificationTelegramBot(HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam), TelegramGroupIDTS, "", "", "", false, true, 0, 0, "", inlineKeyboard);                   
                                    //NotificationTelegramBotToTopic(string MsgText, int chatId = 0, int topicID = 0, InlineKeyboardMarkup inlineKeyboard = null, int MessageID = 0, string TelegramMessage = "", string Files="", string Caption="")
                                    sqlResult = await notificationTelegram.NotificationTelegramBotToTopic("", TelegramGroupIDTS, TopicID, inlineKeyboard, 0, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam), false);
                                    TelegramGroupNameAndMessageID += "~" + TelegramGroupIDTS.ToString() + "|" + sqlResult.Value;
                                    sql = "update dbo.TblSupportOpenTicket set TelegramMessageID=@TelegramMessageID, TelegramGroupNameAndMessageID=@TelegramGroupNameAndMessageID, TelegramMessage = @TelegramMessage, DomainID ='001,002' where TicketNumber = @TicketNumber and CustomerID = @CustomerID";
                                    para = new Dictionary<string, string>
                                    {
                                        {"TelegramMessageID",TelegramGroupNameAndMessageID },
                                        {"TelegramGroupNameAndMessageID",TelegramGroupNameAndMessageID },
                                        {"TelegramMessage",HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam) },
                                        {"TicketNumber",TicketNumber },
                                        {"CustomerID",CustomerID.ToString() },
                                    };
                                    sqlResult = html.SaveChangeData(sql, false, para);
                                    DescriptionTicketTelegramSupportTeam = @"Dear Team,\n\nNow we have new customer services that's registered by " + CreateBy + ".\n\nPlease check invoice for detail.\n\nThank you and best regards\n\nNSM ISP BILLING SYSTEM AUTO GENERATE INVOICE. Invoice#" + InvoiceNo;
                                }
                                await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 2, null, 0, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegramSupportTeam));
                            }
                            else if (InvoiceType == "Renew Invoice")
                            {
                                string smg = @"Dear Team,\n\n
Now we have customer " + (IsServicesOrProduct != "customer products" ? "services" : "product") + " that's renew by " + CreateBy + @".\n\n
Please check invoice for detail.\n\n
Thank you and best regards\n\n
NSM ISP BILLING SYSTEM AUTO GENERATE INVOICE. Invoice#" + InvoiceNo;
                                await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 8, null, 0, "", outPutFile, HttpUtility.UrlDecode(smg)); // send notification to finance renew 1545132903_8
                            }
                        }
                        //}
                        //}
                    }
                    dynamic dt = null;
                    if (IsNoNeedApproval)
                        dt = DataResult.Data;
                    string desf = html.GetFullPath(outPutFile);
                    if (System.IO.File.Exists(desf))
                    {
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        FileInfo file = new FileInfo(desf);
                        file.Delete();
                        //System.IO.File.Delete(desf);
                    }
                    return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, InvoiceNo = InvoiceNo, DataResult = dt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    sqlResult = await notificationTelegram.NotificationTelegramBot("", -1002042086477, "", "", msg, true, true, 0, int.Parse(ApproveRequestToDMCTAXTGMID), ApproveRequestToDMCTAXMsg, null, true);
                }

            }
            //else
            //{
            //	sqlResult = await notificationTelegram.NotificationTelegramBot("", -1002042086477, "", "", msg, true, true, 0, int.Parse(ApproveRequestToDMCTAXTGMID), ApproveRequestToDMCTAXMsg, null, true);
            //}
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Test()
        {
            return View();
        }

        public ActionResult Test2()
        {
            HTML html = new HTML();
            string dbName = "NSM_DB_DMC_VAT.dbo.";
            string tblName = "TblCustomerInvoice";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"CustomerID","1947" },
                {"InvoiceNo","NSM/VT/23-000031" },
            };
            InsertToOtherDBIfNotExists(dbName, tblName, para);
            return View();
        }

        public string GetColumnsName(string tableName)
        {
            HTML html = new HTML();
            string sql = @"select name from sys.columns where object_id = object_id('" + tableName + "') and is_computed = 0";
            DataSet ds = html.LoadData(sql);
            string ColumnsName = "";
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                if (ColumnsName == "")
                    ColumnsName = dr["name"].ToString();
                else
                    ColumnsName += ", " + dr["name"].ToString();
            }
            return ColumnsName;
        }

        public SqlResult InsertToOtherDBIfNotExists(string dbName, string tblName, Dictionary<string, string> ParaCondition)
        {
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            //string SQL = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tblName + "'";
            string SQL = @"select name from sys.columns where object_id = object_id('" + tblName + @"') and is_computed = 0";
            DataSet ds = html.LoadData(SQL);
            StringBuilder sb = new StringBuilder();
            string IfNotExists = "";
            string condition = "";
            string insertintoColumn = "";
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string COLUMN_NAME = dr["name"].ToString();
                if (IfNotExists == "")
                {
                    IfNotExists = @"if (not exists (select " + COLUMN_NAME + " from " + dbName + tblName + " where ";
                    foreach (KeyValuePair<string, string> item in ParaCondition)
                    {
                        if (condition == "")
                            condition = item.Key + " = '" + item.Value + "'";
                        else
                            condition += " and " + item.Key + " = '" + item.Value + "'";
                    }
                    IfNotExists += condition + "))";
                    IfNotExists += @" begin ";
                    sb.AppendLine(IfNotExists);
                    sb.AppendLine("SET IDENTITY_INSERT " + dbName + tblName + " ON");
                    sb.AppendLine("insert into " + dbName + tblName + " (" + COLUMN_NAME);
                    insertintoColumn = COLUMN_NAME;
                }
                else
                {
                    insertintoColumn += ", " + COLUMN_NAME;
                    sb.Append("," + COLUMN_NAME);
                }
            }
            sb.Append(")");
            sb.AppendLine("select " + insertintoColumn + " from " + tblName + " where " + condition);
            sb.AppendLine("SET IDENTITY_INSERT " + dbName + tblName + " OFF");
            sb.AppendLine("end");
            sqlResult = html.SaveChangeData(sb.ToString(), false, false);
            return sqlResult;
        }

        public ActionResult CustomerBlock()
        {
            HTML html = new HTML();
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "c.CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType from dbo.TblCustomerType ct where ct.SystemID = " + html.SystemID + " and ct.WarehouseID=" + html.WarehouseID + " order by ct.CustomerType",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                Label = "Connectivity Status:",
                DBField = "ConnectivityStatus",
                SQL_CBO = "select distinct s.[Status], ac.ConnectivityStatus from dbo.TblStatus s inner join dbo.TblActivateCustomer ac on ac.ConnectivityStatus = s.StatusID where ac.SystemID = " + html.SystemID + " and ac.WarehouseID=" + html.WarehouseID + " order by s.[Status]",
                DisplayText = "Status",
                Value = "ConnectivityStatus",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode from dbo.TblCustomer c inner join dbo.TblCustomerDeviceInfoDetail cd on cd.CustomerID = c.CustomerID where lower(c.CustomerCode) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " and c.DomainID like '%'+@DomainID+'%' order by c.CustomerCode",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select distinct top 10 c.CustomerName from dbo.TblCustomer c inner join dbo.TblCustomerDeviceInfoDetail cd on cd.CustomerID = c.CustomerID where lower(c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " and c.DomainID like '%'+@DomainID+'%' order by c.CustomerName",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName"),
                ISAutoComplete = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = @"select distinct row_number() over(partition by ct.CustomerTypeID order by ct.CustomerTypeID) as [No], c.CustomerID, c.CustomerCode, coalesce(nullif(isnull(c.CustomerName,''), ''), c.CustomerNameLatin) as CustomerName, c.CustomerMobile, ct.CustomerTypeID, ct.CustomerType, s.Status, ac.ConnectivityStatus, ac.SID, ac.ConnectionID, format(ac.StartDate, 'dd-MM-yyyy') StartDate, format(ac.EndDate, 'dd-MM-yyyy') EndDate, bi.GoogleMap_FullAddress, bi.GoogleMap_Latitude, bi.GoogleMap_Longitude
from dbo.TblCustomer c 
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID 
inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID
outer apply (
  select top 1 bi.GoogleMap_FullAddress, bi.GoogleMap_Latitude, bi.GoogleMap_Longitude 
  from dbo.TblCustomerBillInformation bi 
  where bi.CustomerID = c.CustomerID and bi.InvoiceNo = ac.InvoiceNo
  order by bi.CustomerBillInformationID desc
) as bi 
inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus where c.SystemID = 13 and c.WarehouseID = 16 and c.DomainID like '%" + html.DomainID + "%'";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true },
                new GridField() { Header = "Customer", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true },
                new GridField() { Header = "Phone", FieldName = "CustomerMobile" },
                new GridField() { Header = "Block", FieldName = "CustomerType", IsGroupByField = true },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "ConnectionID", FieldName = "ConnectionID" },
                new GridField() { Header = "Start Date", FieldName = "StartDate" },
                new GridField() { Header = "End Date", FieldName = "EndDate" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Map adress", FieldName = "GoogleMap_FullAddress" },
                new GridField() { Header = "Lat", FieldName = "GoogleMap_Latitude" },
                new GridField() { Header = "Long", FieldName = "GoogleMap_Longitude" },
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        public JsonResult GetCustomerBlock(string Year, string YearMonth, string Month, string CustomerTypeID)
        {
            HTML html = new HTML();
            //			string sql = @"select ct.CustomerType, ct.CustomerTypeID, format(ac.StartDate, 'yyMM') StartDate
            //, (select format(sum(acc.Bandwidth), '#,0# Mbps') from dbo.TblActivateCustomer acc
            //inner join dbo.TblCustomerBillingInvoice bi on bi.ActivateCustomerID = acc.ActivateCustomerID and bi.StatusID not in (12,13,14,20) and bi.[Type] = 'Official Invoice'
            //where acc.CustomerTypeID = ct.CustomerTypeID
            //and acc.ConnectivityStatus = 40
            //and format(acc.StartDate, 'yyMM') <= format(ac.StartDate, 'yyMM')
            //and format(acc.EndDate, 'yyMM') >= format(ac.StartDate, 'yyMM')
            //and acc.SystemID = @SystemID and acc.WarehouseID = @WarehouseID
            //) Bandwidth,
            //(select count(distinct acc.ConnectionID) from dbo.TblActivateCustomer acc
            //where acc.CustomerTypeID = ct.CustomerTypeID
            //and acc.ConnectivityStatus = 40
            //and format(acc.StartDate, 'yyMM') <= format(ac.StartDate, 'yyMM')
            //and format(acc.EndDate, 'yyMM') >= format(ac.StartDate, 'yyMM')
            //and acc.SystemID = @SystemID and acc.WarehouseID = @WarehouseID
            //) Customer,
            //(select count(distinct acc.ConnectionID) from dbo.TblActivateCustomer acc
            //where acc.CustomerTypeID = ct.CustomerTypeID
            //and acc.ConnectivityStatus = 40
            //and format(acc.StartDate, 'yyMM') = format(ac.StartDate, 'yyMM')
            //and acc.SystemID = @SystemID and acc.WarehouseID = @WarehouseID
            //) NewCustomer,
            //(select count(distinct acc.ConnectionID) from dbo.TblActivateCustomer acc
            //where acc.CustomerTypeID = ct.CustomerTypeID
            //and acc.ConnectivityStatus = 41
            //and format(acc.LockedDate, 'yyMM') = format(ac.StartDate, 'yyMM')
            //and acc.SystemID = @SystemID and acc.WarehouseID = @WarehouseID
            //) LockCustomer,
            //(select count(distinct acc.ConnectionID) from dbo.TblActivateCustomer acc
            //where acc.CustomerTypeID = ct.CustomerTypeID
            //and acc.ConnectivityStatus = 40
            //and format(acc.RenewDate, 'yyMM') = format(ac.StartDate, 'yyMM')
            //and acc.SystemID = @SystemID and acc.WarehouseID = @WarehouseID
            //) CustomerRenew
            //from dbo.TblActivateCustomer ac
            //inner join dbo.TblCustomerType ct on ct.CustomerTypeID = ac.CustomerTypeID
            //where ac.SystemID = @SystemID and ac.WarehouseID = @WarehouseID
            //and format(ac.StartDate, 'yy') = @Date
            //group by ct.CustomerType, ct.CustomerTypeID, format(ac.StartDate, 'yyMM')
            //order by ct.CustomerType, format(ac.StartDate, 'yyMM')";
            //			Dictionary<string, string> para = new Dictionary<string, string>
            //			{
            //				{ "SystemID", html.SystemID},
            //				{ "WarehouseID", html.WarehouseID },
            //				{ "Date", Date }
            //			};
            string sql = @"declare @year int = " + Year + @", @count int = 1, @countCustBlock int, @i int = 1,
@SystemID int = " + html.SystemID + ", @WarehouseID int = " + html.WarehouseID + @", @CustomerTypeID int,
@Date nvarchar(50), @FromDate nvarchar(50) = '" + Year + "01', @ToDate nvarchar(50) = '" + Year + @"12'

DECLARE @TblCustomerBlock TABLE (ID smallint Primary Key IDENTITY(1,1), CustomerTypeID INT)

DECLARE @TblCustomerBlocks TABLE (ID smallint Primary Key IDENTITY(1,1), CustomerTypeID INT, StartDate nvarchar(50),
TotalConnection int, NewConnection int, ExpireConnection int, CurrentConnection int, RenewConnection int, LockConnection int,
TotalBandwidth int, CurrentBandwidth int)

select @CustomerTypeID = " + CustomerTypeID + @"
select @count = " + Month + @"
select @Date = convert(varchar, @year) + format(@count,'0#')
insert into @TblCustomerBlocks (CustomerTypeID, StartDate, TotalConnection, NewConnection,
ExpireConnection, CurrentConnection, RenewConnection, LockConnection, TotalBandwidth, CurrentBandwidth)
select @CustomerTypeID CustomerTypeID, @Date StartDate,
(	select count(distinct acc.ConnectionID)
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblActivateCustomer acc on acc.CustomerID = bi.CustomerID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where bi.[Type] = 'Official Invoice' and bi.StatusID not in (12,13,14,20)
and bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID
and c.CustomerTypeID = @CustomerTypeID
and format(acc.StartDate, 'yyMM') <= @Date
) TotalConnection,
(	select count(distinct acc.ConnectionID)
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblActivateCustomer acc on acc.CustomerID = bi.CustomerID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where bi.[Type] = 'Official Invoice' and bi.StatusID not in (12,13,14,20)
and bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID
and c.CustomerTypeID = @CustomerTypeID
and format(acc.StartDate, 'yyMM') = @Date
) NewConnection,
(	select count(distinct acc.ConnectionID)
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblActivateCustomer acc on acc.CustomerID = bi.CustomerID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where bi.[Type] = 'Official Invoice' and bi.StatusID not in (12,13,14,20)
and bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID
and c.CustomerTypeID = @CustomerTypeID
and format(acc.StartDate, 'yyMM') <= @Date
and format(acc.EndDate, 'yyMM') < @Date
) ExpireConnection,
(	select count(distinct acc.ConnectionID)
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblActivateCustomer acc on acc.CustomerID = bi.CustomerID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where bi.[Type] = 'Official Invoice' and bi.StatusID not in (12,13,14,20)
and bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID
and c.CustomerTypeID = @CustomerTypeID
and format(acc.StartDate, 'yyMM') <= @Date
and format(acc.EndDate, 'yyMM') >= @Date
) CurrentConnection,
(	select count(distinct acc.ConnectionID)
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblActivateCustomer acc on acc.CustomerID = bi.CustomerID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where bi.[Type] = 'Official Invoice' and bi.StatusID not in (12,13,14,20)
and bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID
and c.CustomerTypeID = @CustomerTypeID
and format(acc.RenewDate, 'yyMM') = @Date
) RenewConnection,
(	select count(distinct acc.ConnectionID)
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblActivateCustomer acc on acc.CustomerID = bi.CustomerID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where bi.[Type] = 'Official Invoice' and bi.StatusID not in (12,13,14,20)
and bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID
and c.CustomerTypeID = @CustomerTypeID and acc.ConnectivityStatus = 41
and format(acc.LockedDate, 'yyMM') = @Date
) LockConnection,
(	select isnull(sum(acc.Bandwidth), 0)
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblActivateCustomer acc on acc.CustomerID = bi.CustomerID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where bi.[Type] = 'Official Invoice' and bi.StatusID not in (12,13,14,20)
and bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID
and c.CustomerTypeID = @CustomerTypeID
and format(acc.StartDate, 'yyMM') <= @Date
) TotalBandwidth,
(	select isnull(sum(acc.Bandwidth), 0)
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblActivateCustomer acc on acc.CustomerID = bi.CustomerID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where bi.[Type] = 'Official Invoice' and bi.StatusID not in (12,13,14,20)
and bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID
and c.CustomerTypeID = @CustomerTypeID
and format(acc.StartDate, 'yyMM') <= @Date
and format(acc.EndDate, 'yyMM') >= @Date
) CurrentBandwidth

select * from @TblCustomerBlocks";
            //var jsonResult = html.JsonConvertDataTable(sqlResult.Data.Tables[0]);
            return Json(new { Data = html.JsonData(sql) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CustomerStatus()
        {
            HTML html = new HTML();

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Date:",
                ID = "DateSearch",
                ISDateTime = true,
                DateRangePicker = true,
                //DateTimePickerWithRanges = true,
                //SingleDatePicker = true,
                OnChangeFunction = "SearchByDate()"
            });
            html.SaveChangeFunction = "SearchByDate()";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public ActionResult GetCustomerStatus(string startDate, string endDate)
        {
            HTML html = new HTML();
            string sql = @"
declare @CustomerPendingInstall int, @NewCustomerConnection int, @CustomerRenewConnection int, @CustomerLockConnection int, 
@CustomerExpiredConnection int, @Completed int, @Pending int, @CustomerActiveConnection int, @CustomerTerminate int

select @CustomerPendingInstall = count(distinct ci.ConnectionID) from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where ci.[Type] = 'Official Invoice' and ci.InvoiceType = 'Created Invoice' and ci.[SID] = 'Pending' and ci.StatusID not in (12,13,14,20)
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID

select @NewCustomerConnection = count(distinct ci.ConnectionID) from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where ci.[Type] = 'Official Invoice' and ci.InvoiceType = 'Created Invoice'
and ci.StatusID not in (12,13,14,20)
and format(ci.CreateDate,'yyMMdd') between @startDate and @endDate
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID

select @CustomerRenewConnection = count(distinct ci.ConnectionID)  from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = ci.CustomerID and ac.ConnectionID = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
where ci.[Type] = 'Official Invoice' and ci.InvoiceType = 'Renew Invoice' and ci.ActivateCustomerID <> 0 and ci.StatusID in (10)
and format(ci.RenewDate,'yyMMdd') between @startDate and @endDate
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID

select @CustomerLockConnection  = count(distinct ci.ConnectionID) from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ci.ActivateCustomerID and ac.ConnectionID = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
where ci.[Type] = 'Official Invoice' and ci.ConnectivityStatus = 41 and ci.StatusID not in(12,13,14,20) 
and format(ac.LockedDate,'yyMMdd') <= @endDate
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID

select @CustomerExpiredConnection = count(distinct ci.ConnectionID) from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ci.ActivateCustomerID and ac.ConnectionID = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
where ci.[Type] = 'Official Invoice' and ci.ActivateCustomerID <> 0 and ci.StatusID in (10)
and format(ac.EndDate,'yyMMdd') <= @endDate
and ac.ConnectivityStatus <> 42
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID

select @CustomerTerminate = count(distinct ci.ConnectionID) from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ci.ActivateCustomerID and ac.ConnectionID = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
where ci.[Type] = 'Official Invoice'
and format(ac.EndDate,'yyMMdd') <= @endDate
and ac.ConnectivityStatus = 42
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID

select @Completed = count(distinct ci.ConnectionID) from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where ci.[Type] = 'Official Invoice' and ci.InvoiceType = 'Created Invoice' 
and ci.[SID] <> 'Pending' 
and ci.StatusID not in (12,13,14,20)
and format(ci.CreateDate,'yyMMdd')  between @startDate and @endDate
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID

select @Pending = count(distinct ci.ConnectionID) from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where ci.[Type] = 'Official Invoice' and ci.InvoiceType = 'Created Invoice' 
and ci.[SID] = 'Pending' 
and ci.StatusID not in (12,13,14,20)
and format(ci.CreateDate,'yyMMdd')  between @startDate and @endDate
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID

select @CustomerActiveConnection = count(distinct ci.ConnectionID) from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ci.ActivateCustomerID and ac.ConnectionID = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
where ci.[Type] = 'Official Invoice' and ci.ConnectivityStatus = 40 and ci.ActivateCustomerID <> 0 and ci.StatusID not in (12,13,14,20)
and format(ac.StartDate,'yyMMdd')  <= @endDate and format(ac.EndDate,'yyMMdd')  >= @endDate
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID

select	@CustomerPendingInstall CustomerPendingInstall, @NewCustomerConnection NewCustomerConnection, 
		@CustomerRenewConnection CustomerRenewConnection, @CustomerLockConnection CustomerLockConnection, 
		@CustomerExpiredConnection CustomerExpiredConnection, @Completed Completed, @Pending Pending,
		@CustomerActiveConnection CustomerActiveConnection, @CustomerTerminate CustomerTerminate";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "startDate", startDate},
                { "endDate", endDate}
            };

            ViewBag.CustomerStatus = html.LoadData(sql, false, para).Tables[0].Rows[0].ToJsonString();


            sql = @"select row_number() over(order by format(ci.InvoiceDate, 'yyMMdd') desc, ct.CustomerType, c.CustomerCode) as [No], 
(format(ci.InvoiceDate, 'dd-MM-yyyy') + '<br>' + 
'<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ ci.InvoiceNo +''','''+ convert(varchar, ci.IsIncludedVAT) +''','''+ convert(varchar, ci.StatusID) +''','''',false,this);"">' + ci.InvoiceNo + '</a>') +'<br>' + ct.CustomerType Invoice,
ct.CustomerType, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a><br>' + c.CustomerName + '<br>Reg.By: '+ c.RegisterBy + '<br>Reg.Date: ' + format(c.RegisterDate, 'dd-MM-yy') CID, 
ci.ConnectionID + '<br>' +
format(ci.StartDate, 'dd-MMM-yy') + ' - ' + format(ci.EndDate, 'dd-MMM-yy') + '<br>' +
convert(varchar, DATEDIFF(d, ci.StartDate, ci.EndDate)) + ' Days, ' + convert(varchar, DATEDIFF(m, ci.StartDate, ci.EndDate)) + 
' Months Free:'+convert(varchar, ci.FreeDuration)+'<br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), ci.EndDate) > 0 then case when DATEDIFF(d, getdate(), ci.StartDate) <= 0 then 'used ' +
convert(varchar, case when DATEDIFF(day, ci.StartDate, GETDATE()) > 0 then convert(varchar, DATEDIFF(day, ci.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + ' day left' else '0' end) else convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + 
' day left' end else 'Expired' end + '</b></span>' ConnectionID, REPLACE(ci.PackageName, 'Internet Service - ','') PackageName
from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where ci.[Type] = 'Official Invoice' and ci.InvoiceType = 'Created Invoice' and ci.[SID] = 'Pending' 
and ci.StatusID not in (12,13,14,20)
and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "format(ci.InvoiceDate, 'yyMMdd') desc, ct.CustomerType, c.CustomerCode";
            html.ShowPrintOnTopTable = false;
            //html.GridHeight = "300px";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Invoice" },
				//new GridField() { Header = "Block", FieldName = "CustomerType" },
				new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "PackageName", FieldName = "PackageName" },
            };
            html.GH.SQL = sql;
            html.UsingJqueryDataTable = true;
            html.TblID = "TblPendingInstall";
            html.ShowPaging = false;
            ViewBag.TblPendingInstall = html.DrawHTMLSearchForm(1, "PendingInstall");

            html = new HTML();
            sql = @"select row_number() over(order by format(ci.CreateDate, 'dd-MM-yyyy')) as [No], format(ci.CreateDate, 'dd-MM-yyyy') [Date],
(format(ci.InvoiceDate, 'dd-MM-yyyy') + '<br>' + 
'<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ ci.InvoiceNo +''','''+ convert(varchar, ci.IsIncludedVAT) +''','''+ convert(varchar, ci.StatusID) +''','''',false,this);"">' + ci.InvoiceNo + '</a>')
+ '<br>' + case when ci.ConnectivityStatus =40 then '<span class=""badge badge-primary"">Completed</span>' else '<span class=""badge badge-warning"">Pending</span>' end  + '<br>' + ct.CustomerType Invoice,
ct.CustomerType, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a><br>' + c.CustomerName + '<br>Reg.By: '+ c.RegisterBy + '<br>Reg.Date: ' + format(c.RegisterDate, 'dd-MM-yy') CID,
ci.ConnectionID + '<br>' +
format(ci.StartDate, 'dd-MMM-yy') + ' - ' + format(ci.EndDate, 'dd-MMM-yy') + '<br>' +
convert(varchar, DATEDIFF(d, ci.StartDate, ci.EndDate)) + ' Days, ' + convert(varchar, DATEDIFF(m, ci.StartDate, ci.EndDate)) + 
' Months Free:'+convert(varchar, ci.FreeDuration)+'<br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), ci.EndDate) > 0 then case when DATEDIFF(d, getdate(), ci.StartDate) <= 0 then 'used ' +
convert(varchar, case when DATEDIFF(day, ci.StartDate, GETDATE()) > 0 then convert(varchar, DATEDIFF(day, ci.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + ' day left' else '0' end) else convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + 
' day left' end else 'Expired' end + '</b></span>' ConnectionID,
REPLACE(ci.PackageName, 'Internet Service - ','') PackageName
from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where ci.[Type] = 'Official Invoice' and ci.InvoiceType = 'Created Invoice'
and ci.StatusID not in (12,13,14,20)
and format(ci.CreateDate,'yyMMdd') between '" + startDate + "' and '" + endDate + @"'
and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "format(ci.InvoiceDate, 'dd-MM-yyyy') desc, ct.CustomerType, c.CustomerCode";
            html.ShowPrintOnTopTable = false;
            //html.GridHeight = "300px";
            html.TblID = "TblNewInstall";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Invoice" },
				//new GridField() { Header = "Block", FieldName = "CustomerType" },
				new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "PackageName", FieldName = "PackageName" },
            };
            html.GH.SQL = sql;
            html.UsingJqueryDataTable = true;
            html.ShowPaging = false;
            ViewBag.TblNewInstall = html.DrawHTMLSearchForm(1, "NewInstall");

            html = new HTML();
            sql = @"select row_number() over(order by format(ac.RenewDate, 'yyMMdd') desc) as [No], format(ci.RenewDate, 'dd-MM-yyyy') [Date],
(format(ci.RenewDate, 'dd-MM-yyyy') + '<br>' + 
'<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ ci.InvoiceNo +''','''+ convert(varchar, ci.IsIncludedVAT) +''','''+ convert(varchar, ci.StatusID) +''','''',false,this);"">' + ci.InvoiceNo + '</a>') +'<br>' + ct.CustomerType Invoice,
ct.CustomerType, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a><br>' + c.CustomerName + '<br>Reg.By: '+ c.RegisterBy + '<br>Reg.Date: ' + format(c.RegisterDate, 'dd-MM-yy') CID,
ci.ConnectionID + '<br>' +
format(ci.StartDate, 'dd-MMM-yy') + ' - ' + format(ci.EndDate, 'dd-MMM-yy') + '<br>' +
convert(varchar, DATEDIFF(d, ci.StartDate, ci.EndDate)) + ' Days, ' + convert(varchar, DATEDIFF(m, ci.StartDate, ci.EndDate)) + 
' Months Free:'+convert(varchar, ci.FreeDuration)+'<br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), ci.EndDate) > 0 then case when DATEDIFF(d, getdate(), ci.StartDate) <= 0 then 'used ' +
convert(varchar, case when DATEDIFF(day, ci.StartDate, GETDATE()) > 0 then convert(varchar, DATEDIFF(day, ci.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + ' day left' else '0' end) else convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + 
' day left' end else 'Expired' end + '</b></span>' ConnectionID,
REPLACE(ci.PackageName, 'Internet Service - ','') PackageName
from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = ci.CustomerID and ac.ConnectionID = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
where ci.[Type] = 'Official Invoice' and ci.InvoiceType = 'Renew Invoice' and ci.ActivateCustomerID <> 0 and ci.StatusID in (10)
and format(ci.RenewDate,'yyMMdd') between '" + startDate + "' and '" + endDate + @"'
and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "format(ac.RenewDate, 'yyMMdd') desc, ct.CustomerType, c.CustomerCode";
            html.ShowPrintOnTopTable = false;
            //html.GridHeight = "300px";
            html.TblID = "TblRenew";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Invoice" },
				//new GridField() { Header = "Block", FieldName = "CustomerType" },
				new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "PackageName", FieldName = "PackageName" },
            };
            html.GH.SQL = sql;
            html.UsingJqueryDataTable = true;
            html.ShowPaging = false;
            ViewBag.TblRenew = html.DrawHTMLSearchForm(1, "TblRenew");

            html = new HTML();
            sql = @"select row_number() over(order by format(ac.LockedDate, 'yyMMdd') desc, ct.CustomerType, c.CustomerID) as [No], 
format(ac.LockedDate, 'dd-MM-yyyy') [Date],
(format(ac.LockedDate, 'dd-MM-yyyy') + '<br>' + '<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ ci.InvoiceNo +''','''+ convert(varchar, ci.IsIncludedVAT) +''','''+ convert(varchar, ci.StatusID) +''','''',false,this);"">' + ci.InvoiceNo + '</a>') +'<br>' + ct.CustomerType Invoice,
--ct.CustomerType, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a><br>' + c.CustomerName + '<br>Reg.By: '+ c.RegisterBy + '<br>Reg.Date: ' + format(c.RegisterDate, 'dd-MM-yy') CID,
ac.ConnectionID + '<br>' + format(ci.StartDate, 'dd-MMM-yy') + ' - ' + format(ci.EndDate, 'dd-MMM-yy') + '<br>' + convert(varchar, DATEDIFF(d, format(ci.StartDate,'yyyy-MM-dd'), format(ci.EndDate,'yyyy-MM-dd'))) + ' Days, ' + convert(varchar, DATEDIFF(m, format(ci.StartDate,'yyyy-MM-dd'), format(ci.EndDate,'yyyy-MM-dd'))) + ' Months Free:'+convert(varchar, ci.FreeDuration)+'<br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), format(ci.EndDate,'yyyy-MM-dd')) > 0 then case when DATEDIFF(d, getdate(), format(ci.StartDate,'yyyy-MM-dd')) <= 0 then 'used ' + convert(varchar, case when DATEDIFF(day, format(ci.StartDate,'yyyy-MM-dd'), GETDATE()) > 0 then convert(varchar, DATEDIFF(day, format(ci.StartDate,'yyyy-MM-dd'), GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), format(ci.EndDate,'yyyy-MM-dd'))) + ' day left' else '0' end) else convert(varchar, DATEDIFF(d, getdate(), format(ci.EndDate,'yyyy-MM-dd'))) + ' day left' end else 'Expired' end + '</b></span>' ConnectionID,
REPLACE(ci.PackageName, 'Internet Service - ','') 
+ case when isnull(ac.Comment,'') <> '' then '<br><b>' + replace(isnull(ac.Comment,''),'.','<br>') + '</b>' else '' end PackageName
from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ci.ActivateCustomerID and ac.ConnectionID = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
where ci.[Type] = 'Official Invoice' and ci.ConnectivityStatus = 41 and ci.StatusID not in(12,13,14,20)
and format(ac.LockedDate,'yyMMdd') <= '" + endDate + @"'
and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + @"
group by format(ac.LockedDate, 'dd-MM-yyyy'), format(ac.LockedDate, 'yyMMdd'), c.CustomerID, ci.InvoiceNo, ci.IsIncludedVAT,
ci.StatusID, ct.CustomerType, c.CustomerCode, c.CustomerName, c.RegisterBy, c.RegisterDate,
ac.ConnectionID, format(ci.StartDate,'yyyy-MM-dd'), format(ci.EndDate,'yyyy-MM-dd'), ci.FreeDuration, ci.PackageName, ac.Comment
,format(ci.StartDate, 'dd-MMM-yy'), format(ci.EndDate, 'dd-MMM-yy')";
            html.OrderBy = "format(ac.LockedDate, 'yyMMdd') desc, ct.CustomerType, c.CustomerID";
            html.ShowPrintOnTopTable = false;
            //html.GridHeight = "300px";
            html.TblID = "TblLocked";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Invoice" },
				//new GridField() { Header = "Block", FieldName = "CustomerType" },
				new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "PackageName", FieldName = "PackageName" },
            };
            html.GH.SQL = sql;
            html.UsingJqueryDataTable = true;
            html.ShowPaging = false;
            ViewBag.TblLocked = html.DrawHTMLSearchForm(1, "TblLocked");

            html = new HTML();
            sql = @"select row_number() over(order by format(ac.StartDate, 'yyMMdd') desc, ct.CustomerType, c.CustomerID) as [No], 
format(ac.StartDate, 'dd-MM-yyyy') [Date],
(format(ac.StartDate, 'dd-MM-yyyy') + '<br>' + '<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ ci.InvoiceNo +''','''+ convert(varchar, ci.IsIncludedVAT) +''','''+ convert(varchar, ci.StatusID) +''','''',false,this);"">' + ci.InvoiceNo + '</a>') +'<br>' + ct.CustomerType Invoice,
--ct.CustomerType, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a><br>' + c.CustomerName + '<br>Reg.By: '+ c.RegisterBy + '<br>Reg.Date: ' + format(c.RegisterDate, 'dd-MM-yy') CID,
ac.ConnectionID + '<br>' + format(ci.StartDate, 'dd-MMM-yy') + ' - ' + format(ci.EndDate, 'dd-MMM-yy') + '<br>' + convert(varchar, DATEDIFF(d, format(ci.StartDate,'yyyy-MM-dd'), format(ci.EndDate,'yyyy-MM-dd'))) + ' Days, ' + convert(varchar, DATEDIFF(m, format(ci.StartDate,'yyyy-MM-dd'), format(ci.EndDate,'yyyy-MM-dd'))) + ' Months Free:'+convert(varchar, ci.FreeDuration)+'<br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), format(ci.EndDate,'yyyy-MM-dd')) > 0 then case when DATEDIFF(d, getdate(), format(ci.StartDate,'yyyy-MM-dd')) <= 0 then 'used ' + convert(varchar, case when DATEDIFF(day, format(ci.StartDate,'yyyy-MM-dd'), GETDATE()) > 0 then convert(varchar, DATEDIFF(day, format(ci.StartDate,'yyyy-MM-dd'), GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), format(ci.EndDate,'yyyy-MM-dd'))) + ' day left' else '0' end) else convert(varchar, DATEDIFF(d, getdate(), format(ci.EndDate,'yyyy-MM-dd'))) + ' day left' end else 'Expired' end + '</b></span>' ConnectionID,
REPLACE(ci.PackageName, 'Internet Service - ','') 
+ '<br>In.Add.: ' + cb.InstallAddress + ', ' + cb.InstallCommune + ', ' + cb.InstallDistrict + ', ' + cb.InstallProvinceCity
+ case when isnull(cb.InstallDate,'') <> '' then '<br>Installed Date:' + format(cb.InstallDate, 'dd-MMM-yyyy HH:mm:ss tt') else '' end PackageName
from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ci.ActivateCustomerID and ac.ConnectionID = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
inner join dbo.TblCustomerBillInformation cb on cb.CustomerID = c.CustomerID and cb.ConnectionID = ci.ConnectionID
where ci.[Type] = 'Official Invoice' and ci.ConnectivityStatus = 40 and ci.ActivateCustomerID <> 0 and ci.StatusID not in (12,13,14,20)
and format(ac.StartDate,'yyMMdd')  <= '" + endDate + "' and format(ac.EndDate,'yyMMdd')  >= '" + endDate + @"'
and ci.SystemID = " + html.SystemID + " and ci.WarehouseID =" + html.WarehouseID;
            html.OrderBy = "format(ac.StartDate, 'yyMMdd') desc, ct.CustomerType, c.CustomerID";
            html.ShowPrintOnTopTable = false;
            //html.GridHeight = "300px";
            html.TblID = "TblActived";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Invoice" },
				//new GridField() { Header = "Block", FieldName = "CustomerType" },
				new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "PackageName", FieldName = "PackageName" },
            };
            html.GH.SQL = sql;
            html.UsingJqueryDataTable = true;
            html.ShowPaging = false;
            ViewBag.TblActived = html.DrawHTMLSearchForm(1, "TblActived");

            html = new HTML();
            sql = @"select row_number() over(order by format(ac.EndDate, 'yyMMdd') desc) as [No], format(ac.EndDate, 'dd-MM-yyyy') [Date],
(format(ac.EndDate, 'dd-MM-yyyy') + '<br>' + 
'<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ ci.InvoiceNo +''','''+ convert(varchar, ci.IsIncludedVAT) +''','''+ convert(varchar, ci.StatusID) +''','''',false,this);"">' + ci.InvoiceNo + '</a>') +'<br>' + ct.CustomerType Invoice,
ct.CustomerType, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a><br>' + c.CustomerName + '<br>Reg.By: '+ c.RegisterBy + '<br>Reg.Date: ' + format(c.RegisterDate, 'dd-MM-yy') CID,
ci.ConnectionID + '<br>' +
format(ci.StartDate, 'dd-MMM-yy') + ' - ' + format(ci.EndDate, 'dd-MMM-yy') + '<br>' +
convert(varchar, DATEDIFF(d, ci.StartDate, ci.EndDate)) + ' Days, ' + convert(varchar, DATEDIFF(m, ci.StartDate, ci.EndDate)) + 
' Months Free:'+convert(varchar, ci.FreeDuration)+'<br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), ci.EndDate) > 0 then case when DATEDIFF(d, getdate(), ci.StartDate) <= 0 then 'used ' +
convert(varchar, case when DATEDIFF(day, ci.StartDate, GETDATE()) > 0 then convert(varchar, DATEDIFF(day, ci.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + ' day left' else '0' end) else convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + 
' day left' end else 'Expired' end + '</b></span>' ConnectionID,
REPLACE(ci.PackageName, 'Internet Service - ','')
+ case when isnull(ac.Comment,'') <> '' then '<br><b>' + replace(isnull(ac.Comment,''),'.','<br>') + '</b>' else '' end PackageName
from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ci.ActivateCustomerID and ac.ConnectionID = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
where ci.[Type] = 'Official Invoice' and ci.ActivateCustomerID <> 0 and ci.StatusID in (10)
and ac.ConnectivityStatus <> 42
and format(ac.EndDate,'yyMMdd') <= '" + endDate + @"'
and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "format(ac.EndDate, 'yyMMdd') desc, ct.CustomerType, c.CustomerCode";
            html.ShowPrintOnTopTable = false;
            //html.GridHeight = "300px";
            html.TblID = "TblExpired";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Invoice" },
				//new GridField() { Header = "Block", FieldName = "CustomerType" },
				new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "PackageName", FieldName = "PackageName" },
            };
            html.GH.SQL = sql;
            html.UsingJqueryDataTable = true;
            html.ShowPaging = false;
            ViewBag.TblExpired = html.DrawHTMLSearchForm(1, "TblExpired");

            html = new HTML();
            sql = @"select row_number() over(order by format(ac.EndDate, 'yyMMdd') desc) as [No], format(ac.EndDate, 'dd-MM-yyyy') [Date],
(format(ac.EndDate, 'dd-MM-yyyy') + '<br>' + 
'<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ ci.InvoiceNo +''','''+ convert(varchar, ci.IsIncludedVAT) +''','''+ convert(varchar, ci.StatusID) +''','''',false,this);"">' + ci.InvoiceNo + '</a>') +'<br>' + ct.CustomerType Invoice,
ct.CustomerType, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a><br>' + c.CustomerName + '<br>Reg.By: '+ c.RegisterBy + '<br>Reg.Date: ' + format(c.RegisterDate, 'dd-MM-yy') CID,
ci.ConnectionID + '<br>' +
format(ci.StartDate, 'dd-MMM-yy') + ' - ' + format(ci.EndDate, 'dd-MMM-yy') + '<br>' +
convert(varchar, DATEDIFF(d, ci.StartDate, ci.EndDate)) + ' Days, ' + convert(varchar, DATEDIFF(m, ci.StartDate, ci.EndDate)) + 
' Months Free:'+convert(varchar, ci.FreeDuration)+'<br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), ci.EndDate) > 0 then case when DATEDIFF(d, getdate(), ci.StartDate) <= 0 then 'used ' +
convert(varchar, case when DATEDIFF(day, ci.StartDate, GETDATE()) > 0 then convert(varchar, DATEDIFF(day, ci.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + ' day left' else '0' end) else convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + 
' day left' end else 'Expired' end + '</b></span>' ConnectionID,
REPLACE(ci.PackageName, 'Internet Service - ','') 
+ case when isnull(ac.Comment,'') <> '' then '<br><b>' + replace(isnull(ac.Comment,''),'.','<br>') + '</b>' else '' end PackageName
from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = ci.ActivateCustomerID and ac.ConnectionID = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
where ci.[Type] = 'Official Invoice' and ac.ConnectivityStatus = 42
and format(ac.EndDate,'yyMMdd') <= '" + endDate + @"'
and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "format(ac.EndDate, 'yyMMdd') desc, ct.CustomerType, c.CustomerCode";
            html.ShowPrintOnTopTable = false;
            //html.GridHeight = "300px";
            html.TblID = "TblTerminated";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Invoice" },
				//new GridField() { Header = "Block", FieldName = "CustomerType" },
				new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "PackageName", FieldName = "PackageName" },
            };
            html.GH.SQL = sql;
            html.UsingJqueryDataTable = true;
            html.ShowPaging = false;
            ViewBag.TblTerminated = html.DrawHTMLSearchForm(1, "TblTerminated");
            return View();
        }

        public ActionResult Customer()
        {
            HTML html = new HTML
            {
                TblAlign = "left"
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Date:",
                ID = "DateSearch",
                ISDateTime = true,
                DateRangePicker = true,
                ToDay = false,
                //DefaultValueDate=false,
                OnChangeFunction = "CustSearchByDate()"
            });
            ViewBag.CustSearchByDate = html.DrawHTMLForm();
            string sql = @"
--Start New Cust Last Month and this month
DECLARE @StartDate  DATE,
        @EndDate    DATE;

SET @StartDate  = DATEFROMPARTS ( DATEPART(yyyy, GETDATE()) - 1, 1, 1 );
SET @EndDate    = DATEADD(yy, DATEDIFF(yy, 0, GETDATE()) + 1, -1);

WITH CTE_Months
AS
(
    SELECT @StartDate dates
    UNION ALL
    SELECT DATEADD(MONTH,1,dates)
    FROM CTE_Months
    WHERE DATEADD(MONTH,1,dates) < @EndDate
)
SELECT  YEAR(B.RegisterDate) AS yr,
		month(B.RegisterDate) as Monthnumber,
        count(distinct ISNULL(B.CustomerID,0)) AS Customer
FROM CTE_Months A
LEFT JOIN dbo.TblCustomer B
ON  YEAR(A.dates) = YEAR(B.RegisterDate)
    AND MONTH(A.dates) = MONTH(B.RegisterDate)
inner join dbo.TblCustomerInvoice bi on bi.CustomerID = B.CustomerID
	where YEAR(B.RegisterDate) is not null and 
	bi.[Type] = 'Official Invoice' and B.StatusID not in (12,13,14,20)
	and YEAR(B.RegisterDate) = format(@StartDate,'yyyy')
	and B.SystemID = @SystemID and B.WarehouseID = @WarehouseID and B.DomainID like '%'+@DomainID+'%'
GROUP BY YEAR(B.RegisterDate),month(B.RegisterDate)
order by Monthnumber;

WITH CTE_Months
AS
(
    SELECT @StartDate dates
    UNION ALL
    SELECT DATEADD(MONTH,1,dates)
    FROM CTE_Months
    WHERE DATEADD(MONTH,1,dates) < @EndDate
)
SELECT  YEAR(B.RegisterDate) AS yr,
		month(B.RegisterDate) as Monthnumber,
        count(distinct ISNULL(B.CustomerID,0)) AS Customer
FROM CTE_Months A
LEFT JOIN dbo.TblCustomer B
ON  YEAR(A.dates) = YEAR(B.RegisterDate)
    AND MONTH(A.dates) = MONTH(B.RegisterDate)
inner join dbo.TblCustomerInvoice bi on bi.CustomerID = B.CustomerID
	where YEAR(B.RegisterDate) is not null and 
	bi.[Type] = 'Official Invoice' and B.StatusID not in (12,13,14,20)
	and YEAR(B.RegisterDate) = format(@EndDate,'yyyy')
	and B.SystemID = @SystemID and B.WarehouseID = @WarehouseID and B.DomainID like '%'+@DomainID+'%'
GROUP BY YEAR(B.RegisterDate),month(B.RegisterDate)
order by Monthnumber;

-- End Start New Cust Last Month and this month
--Start Customer Group
select cg.CustomerGroup, count(distinct c.CustomerID) CustomerID from dbo.TblCustomer c
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
where ci.[Type] = 'Official Invoice' and ci.StatusID not in (12,13,14,20)
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID and ci.DomainID like '%'+@DomainID+'%'
group by cg.CustomerGroup order by CustomerID desc;

select count(distinct c.CustomerID) CustomerID from dbo.TblCustomer c
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
where ci.[Type] = 'Official Invoice' and ci.StatusID not in (12,13,14,20)
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID and ci.DomainID like '%'+@DomainID+'%'
--End Customer Group";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };

            ViewBag.NewCustomer = html.JsonData(sql, false, para, true);
            //			ViewBag.DrawHTMLForm = html.DrawHTMLForm();

            sql = @"select distinct a.[Date], a.y from (
select distinct format(ac.ActivateDate,'MMM/yyyy') [Date], format(ac.ActivateDate,'yyyyMM') y from dbo.TblActivateCustomer ac 
inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = ac.CustomerID
where bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + @"%'
union all
select distinct format(ac.LockedDate,'MMM/yyyy') [Date], format(ac.LockedDate,'yyyyMM') y from dbo.TblActivateCustomer ac 
inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = ac.CustomerID
where ac.LockedDate is not null and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + @"%'
) as a order by a.y desc";
            html = new HTML();
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Date:",
                ID = "TxtDate",
                SQL_CBO = sql,
                DisplayText = "Date",
                Value = "y",
            });
            html.TblAlign = "left";
            ViewBag.DrawHTMLFormDate = html.DrawHTMLForm();
            ViewBag.Date = DateTime.Parse(html.GetCurrentDate()).ToString("yyyyMM01");

            html = new HTML
            {
                TblAlign = "left"
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Date:",
                ID = "CustSearchDate",
                ISDateTime = true,
                DateRangePicker = true,
                ToDay = false,
                //DefaultValueDate=false,
                OnChangeFunction = "CustSearchDate()"
            });
            ViewBag.CustSearchDate = html.DrawHTMLForm();
            return View();
        }
        public ActionResult ShowCustomerConnectivityStatus(string Date)
        {
            HTML html = new HTML();
            string sql = @"DECLARE @StartDate DATE, @EndDate DATE;
SET @StartDate  = DATEADD(M, DATEDIFF(m, 0, @Date), 0);
SET @EndDate    = DATEADD(M, DATEDIFF(m, 0, @Date) + 1, -1);

WITH CTE_Months
AS
(
    SELECT @StartDate dates
    UNION ALL
    SELECT DATEADD(day,1,dates)
    FROM CTE_Months
    WHERE DATEADD(day,1,dates) <= @EndDate
)
select format(A.dates, 'dd-MMM') [Day],
(select count(distinct ac.ConnectionID) from dbo.TblActivateCustomer ac
where format(ac.ActivateDate, 'yyyyMMdd') = format(A.dates, 'yyyyMMdd') and ac.ConnectivityStatus = 40
and ac.SystemID = @SystemID and ac.WarehouseID = @WarehouseID and ac.DomainID  like '%' + @DomainID + '%') Activated,
(select count(distinct ac.ConnectionID) from dbo.TblActivateCustomer ac
where format(ac.LockedDate, 'yyyyMMdd') = format(A.dates, 'yyyyMMdd') and ac.ConnectivityStatus = 41
and ac.SystemID = @SystemID and ac.WarehouseID = @WarehouseID and ac.DomainID  like '%' + @DomainID + '%') Locked,
(select count(distinct ac.ConnectionID) from dbo.TblActivateCustomer ac
where format(ac.LockedDate, 'yyyyMMdd') = format(A.dates, 'yyyyMMdd') and ac.ConnectivityStatus = 42
and ac.SystemID = @SystemID and ac.WarehouseID = @WarehouseID and ac.DomainID  like '%' + @DomainID + '%') Terminated
from CTE_Months A;";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID },
                { "Date", Date }
            };

            ViewBag.Customer = html.JsonData(sql, false, para, true);
            return View();
        }

        public JsonResult ShowCustomer(int Bandwidth = 0)
        {
            var html = new HTML();
            string sql = @"select c.CustomerGroupID, cg.CustomerGroup, count(distinct ac.ConnectionID) Customer
from dbo.TblCustomer c
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblCustomerBillingInvoice ci on ci.CustomerID = c.CustomerID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID and ac.ConnectionID = ci.ConnectionID
where ci.[Type] = 'Official Invoice'
and ci.StatusID not in (12,13,14,20) 
and isnull(ac.Bandwidth,0) = @Bandwidth
group by c.CustomerGroupID, cg.CustomerGroup";

            Dictionary<string, string> para = new Dictionary<string, string> {
                { "Bandwidth", Bandwidth.ToString() },
            };

            return Json(new { data = html.JsonData(sql, false, para) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ShowCustomerDetail(int CustomerGroupID, int Bandwidth)
        {
            HTML html = new HTML();
            string sql = @"select c.CustomerID, c.CustomerCode, coalesce(nullif(c.CustomerName, ''), c.CustomerNameLatin) as CustomerName, case when ac.VLanName is not null then ac.VLanName else ac.PPPoEUserName end as vp, ac.Bandwidth, ac.ConnectivityStatus
from dbo.TblActivateCustomer ac 
inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
where c.CustomerGroupID = @CustomerGroupID and ac.Bandwidth = @Bandwidth";
            Dictionary<string, string> para = new Dictionary<string, string> {
                {"CustomerGroupID",CustomerGroupID.ToString() },
                {"Bandwidth",Bandwidth.ToString() },
            };
            return Json(new { data = html.JsonData(sql, false, para) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowCustomerByBlock(int CustomerGroupID, int CustomerTypeID, string startDate = "", string endDate = "", string Bandwidth = "0", string CustomerID = "", string RouterID = "", string ConnectionType = "")
        {
            HTML html = new HTML();
            string sql = @"select row_number() over(order by format(ci.InvoiceDate, 'yyMMdd') desc, c.CustomerCode, ct.CustomerType) as [No], 
(format(ci.InvoiceDate, 'dd-MM-yyyy') + '<br>' + 
'<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ ci.InvoiceNo +''','''+ convert(varchar, ci.IsIncludedVAT) +''','''+ convert(varchar, ci.StatusID) +''','''',false,this);"">' + ci.InvoiceNo + '</a>') +'<br>' + ct.CustomerType Invoice,
ct.CustomerType, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a><br>' + c.CustomerName + '<br>Reg.By: '+ c.RegisterBy + '<br>Reg.Date: ' + format(c.RegisterDate, 'dd-MM-yy') CID, 
ci.ConnectionID + '<br>' +
format(ci.StartDate, 'dd-MMM-yy') + ' - ' + format(ci.EndDate, 'dd-MMM-yy') + '<br>' +
convert(varchar, DATEDIFF(d, ci.StartDate, ci.EndDate)) + ' Days, ' + convert(varchar, DATEDIFF(m, ci.StartDate, ci.EndDate)) + 
' Months Free:'+convert(varchar, ci.FreeDuration)+'<br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), ci.EndDate) > 0 then case when DATEDIFF(d, getdate(), ci.StartDate) <= 0 then 'used ' +
convert(varchar, case when DATEDIFF(day, ci.StartDate, GETDATE()) > 0 then convert(varchar, DATEDIFF(day, ci.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + ' day left' else '0' end) else convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + 
' day left' end else 'Expired' end + '</b></span>' ConnectionID, REPLACE(ci.ItemDescription, 'Internet Service - ','') 
+ '<br>In.Add.: ' + cb.InstallAddress + ', ' + cb.InstallCommune + ', ' + cb.InstallDistrict + ', ' + cb.InstallProvinceCity
+ case when isnull(cb.InstallDate,'') <> '' then '<br>Installed Date:' + format(cb.InstallDate, 'dd-MMM-yyyy HH:mm:ss tt') else '' end PackageName
from dbo.TblCustomerInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblCustomerBillInformation cb on cb.CustomerID = c.CustomerID and cb.ConnectionID = ci.ConnectionID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID and case when isnull(ac.ConnectionOfConnectionID,'') <> '' then ac.ConnectionOfConnectionID else ac.ConnectionID end = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
where ci.[Type] = 'Official Invoice' ";
            if (CustomerID == "")
                sql += @" and ci.InvoiceType = 'Created Invoice' ";
            sql += @" and ci.StatusID not in (12,13,14,20) and ci.ServiceName <> 'Deposit Fee' and isnull(ci.OtherChargeID, 0) = 0 and isnull(ci.ItemID, 0) = 0
and c.CustomerTypeID = " + CustomerTypeID + " and c.CustomerGroupID = " + CustomerGroupID;
            if (startDate != "" && endDate != "")
                sql += @"and format(ci.CreateDate,'yyMMdd') between '" + startDate + "' and '" + endDate + @"'";
            else if (CustomerID != "")
                sql += @" and c.CustomerID IN (" + CustomerID + ") and ac.ConnectionType = '" + ConnectionType + "' and ac.RouterID = " + RouterID;
            sql += @"and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + "%'";

            if (Bandwidth != "0")
            {
                sql = @"select row_number() over(order by format(ac.StartDate, 'yyMMdd') desc, c.CustomerCode, ct.CustomerType) as [No], 
(format(ac.StartDate, 'dd-MM-yyyy') + '<br>' + 
'<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ ci.InvoiceNo +''','''+ convert(varchar, ci.IsIncludedVAT) +''','''+ convert(varchar, ci.StatusID) +''','''',false,this);"">' + ci.InvoiceNo + '</a>') +'<br>' + ct.CustomerType Invoice,
ct.CustomerType, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a><br>' + c.CustomerName + '<br>Reg.By: '+ c.RegisterBy + '<br>Reg.Date: ' + format(c.RegisterDate, 'dd-MM-yy') CID, 
ci.ConnectionID + '<br>' +
format(ci.StartDate, 'dd-MMM-yy') + ' - ' + format(ci.EndDate, 'dd-MMM-yy') + '<br>' +
convert(varchar, DATEDIFF(d, ci.StartDate, ci.EndDate)) + ' Days, ' + convert(varchar, DATEDIFF(m, ci.StartDate, ci.EndDate)) + 
' Months Free:'+convert(varchar, ci.FreeDuration)+'<br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), ci.EndDate) > 0 then case when DATEDIFF(d, getdate(), ci.StartDate) <= 0 then 'used ' +
convert(varchar, case when DATEDIFF(day, ci.StartDate, GETDATE()) > 0 then convert(varchar, DATEDIFF(day, ci.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + ' day left' else '0' end) else convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + 
' day left' end else 'Expired' end + '</b></span>' ConnectionID, REPLACE(ci.ServiceName, 'Internet Service - ','') 
+ '<br>In.Add.: ' + cb.InstallAddress + ', ' + cb.InstallCommune + ', ' + cb.InstallDistrict + ', ' + cb.InstallProvinceCity
+ case when isnull(cb.InstallDate,'') <> '' then '<br>Installed Date:' + format(cb.InstallDate, 'dd-MMM-yyyy HH:mm:ss tt') else '' end PackageName
from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblCustomerBillInformation cb on cb.CustomerID = c.CustomerID and cb.CustomerBillInformationID = ci.CustomerBillInformationID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID and ac.ConnectionID = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
where ci.[Type] = 'Official Invoice'
and ci.StatusID not in (12,13,14,20) --and ci.ServiceName <> 'Deposit Fee' and isnull(ci.OtherChargeID, 0) = 0 and isnull(ci.ItemID, 0) = 0
and c.CustomerTypeID = " + CustomerTypeID + " and c.CustomerGroupID = " + CustomerGroupID + @"
and format(ac.StartDate, 'yyMMdd') <= '" + endDate + "' and format(ac.EndDate, 'yyMMdd') >= '" + endDate + @"'
and ci.ConnectivityStatus = 40
and isnull(ac.Bandwidth,0) = " + Bandwidth + @"
and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + @"%'
group by format(ac.StartDate, 'yyMMdd'), c.CustomerCode,
(format(ac.StartDate, 'dd-MM-yyyy') + '<br>' + 
'<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ ci.InvoiceNo +''','''+ convert(varchar, ci.IsIncludedVAT) +''','''+ convert(varchar, ci.StatusID) +''','''',false,this);"">' + ci.InvoiceNo + '</a>') +'<br>' + ct.CustomerType,
ct.CustomerType, 
'<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a><br>' + c.CustomerName + '<br>Reg.By: '+ c.RegisterBy + '<br>Reg.Date: ' + format(c.RegisterDate, 'dd-MM-yy'), 
ci.ConnectionID + '<br>' +
format(ci.StartDate, 'dd-MMM-yy') + ' - ' + format(ci.EndDate, 'dd-MMM-yy') + '<br>' +
convert(varchar, DATEDIFF(d, ci.StartDate, ci.EndDate)) + ' Days, ' + convert(varchar, DATEDIFF(m, ci.StartDate, ci.EndDate)) + 
' Months Free:'+convert(varchar, ci.FreeDuration)+'<br><span class=""text-red""><b>' + 
case when DATEDIFF(d, getdate(), ci.EndDate) > 0 then case when DATEDIFF(d, getdate(), ci.StartDate) <= 0 then 'used ' +
convert(varchar, case when DATEDIFF(day, ci.StartDate, GETDATE()) > 0 then convert(varchar, DATEDIFF(day, ci.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + ' day left' else '0' end) else convert(varchar, DATEDIFF(d, getdate(), ci.EndDate)) + 
' day left' end else 'Expired' end + '</b></span>', REPLACE(ci.ServiceName, 'Internet Service - ','') 
+ '<br>In.Add.: ' + cb.InstallAddress + ', ' + cb.InstallCommune + ', ' + cb.InstallDistrict + ', ' + cb.InstallProvinceCity
+ case when isnull(cb.InstallDate,'') <> '' then '<br>Installed Date:' + format(cb.InstallDate, 'dd-MMM-yyyy HH:mm:ss tt') else '' end";
                html.IsUseHavingCondition = true;
                html.OrderBy = "format(ac.StartDate, 'yyMMdd') desc, c.CustomerCode, ct.CustomerType";
            }
            else
            {
                html.IsHaveWhereCondition = true;
                html.OrderBy = "format(ci.InvoiceDate, 'yyMMdd') desc, c.CustomerCode, ct.CustomerType";
            }

            html.ShowPrintOnTopTable = false;
            //html.GridHeight = "300px";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Invoice" },
                new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "PackageName", FieldName = "PackageName" },
            };
            html.GH.SQL = sql;
            html.UsingJqueryDataTable = true;
            html.TblID = "TblCustBlock";
            html.ShowPaging = false;
            ViewBag.TblCustBlock = html.DrawHTMLSearchForm(1, "TblCustBlock");
            return View();
        }

        public ActionResult Bandwidth()
        {
            HTML html = new HTML();
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Date:",
                ID = "DateSearch",
                ISDateTime = true,
                //DateRangePicker = true,
                SingleDatePicker = true,
                //ToDay = false,
                //DefaultValueDate=false,
                OnChangeFunction = "SearchByDate()"
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public JsonResult ShowBandwidth()
        {
            HTML html = new HTML();
            string sql = @"select ac.Bandwidth, count(distinct ac.ConnectionID) Connection
from dbo.TblActivateCustomer ac
inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
inner join dbo.TblCustomerBillingInvoice ci on ci.ConnectionID = ac.ConnectionID and ci.CustomerID = ac.CustomerID 
where ci.[Type] = 'Official Invoice'
and ci.StatusID not in (12,13,14,20) 
group by ac.Bandwidth
order by ac.Bandwidth";
            return Json(new { data = html.JsonData(sql) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Router()
        {
            HTML html = new HTML();
            string sql = @"select ac.RouterID, tr.RouterName, count(distinct ac.ConnectionID) Connection,
(select count(distinct acc.ConnectionID) from dbo.TblActivateCustomer acc 
inner join dbo.TblCustomer cc on cc.CustomerID = acc.CustomerID
where acc.ConnectivityStatus = 40 and acc.RouterID = ac.RouterID
and acc.SystemID = @SystemID and acc.WarehouseID = @WarehouseID and acc.DomainID like '%'+@DomainID+'%') Actived,
(select count(distinct acc.ConnectionID) from dbo.TblActivateCustomer acc
inner join dbo.TblCustomer cc on cc.CustomerID = acc.CustomerID
where acc.ConnectivityStatus <> 40 and acc.RouterID = ac.RouterID
and acc.SystemID = @SystemID and acc.WarehouseID = @WarehouseID and acc.DomainID like '%'+@DomainID+'%') Inactived
from dbo.TblRouter tr
inner join dbo.TblActivateCustomer ac on ac.RouterID = tr.RouterID
inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
where ac.SystemID = @SystemID and ac.WarehouseID = @WarehouseID and ac.DomainID like '%'+@DomainID+'%' and isnull(ac.ConnectionID, '') <> ''
group by ac.RouterID, tr.RouterName
order by tr.RouterName";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID },
            };

            return Json(new { data = html.JsonData(sql, false, para) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRouterInfoByID(int RouterID)
        {
            HTML html = new HTML();
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            var DataPPPoE = MK.GetDataRowFromRouterPPPoE(RouterID.ToString()).DataRowRouter.ToJsonString();
            var DataVlan = MK.GetDataRowFromRouterVlan(RouterID.ToString()).DataRowRouter.ToJsonString();

            string sql = @"select distinct ac.RouterID, ac.PPPoEUserName, isnull(ac.VLanID,'') VLanID, ac.ConnectionType, ac.CustomerID
from dbo.TblActivateCustomer ac
inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
inner join dbo.TblCustomerBillInformation cb on cb.CustomerID = c.CustomerID and cb.ConnectionID = ci.ConnectionID
where ac.RouterID = @RouterID and ac.SystemID = @SystemID and ac.WarehouseID = @WarehouseID and ac.DomainID like '%'+@DomainID+'%'
and case when isnull(ac.ConnectionOfConnectionID,'') <> '' then ac.ConnectionOfConnectionID else ac.ConnectionID end = ci.ConnectionID and ac.InvoiceNo = ci.InvoiceNo
and ci.[Type] = 'Official Invoice'
and ci.StatusID not in (12,13,14,20) and ci.ServiceName <> 'Deposit Fee' and isnull(ci.OtherChargeID, 0) = 0 and isnull(ci.ItemID, 0) = 0
order by ac.CustomerID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "RouterID", RouterID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            string DataInDatabase = html.dataSetToJsonSt(html.LoadData(sql, false, para));

            return Json(new { DataPPPoE = DataPPPoE, DataVlan = DataVlan, DataInDatabase = DataInDatabase }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddPPPoEComment(int RouterID, string PPPoEUserName, string Comment)
        {
            Comment = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(Comment));
            HTML html = new HTML();
            Mikrotik mk = new Mikrotik();
            mk.SetConnection(RouterID);
            SqlResult result = mk.SetPPPoEUserComment(PPPoEUserName, Comment);
            return Json(new { Result = result.Result, Message = result.Message, Comment = Comment }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddVlanComment(int RouterID, string ID, string Comment)
        {
            Comment = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(Comment));
            HTML html = new HTML();
            Mikrotik mk = new Mikrotik();
            mk.SetConnection(RouterID);
            SqlResult result = mk.SetVlanComment(ID, Comment);
            return Json(new { Result = result.Result, Message = result.Message, Comment = Comment }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RenameVlan(int RouterID, string ID, string Comment)
        {
            Comment = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(Comment));
            HTML html = new HTML();
            Mikrotik mk = new Mikrotik();
            mk.SetConnection(RouterID);
            SqlResult result = mk.RenameVlan(ID, Comment);
            return Json(new { Result = result.Result, Message = result.Message, NewName = Comment }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ChangStatus(int RouterID, string ID, string Comment, string Status)
        {
            Comment = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(Comment));
            HTML html = new HTML();
            Mikrotik mk = new Mikrotik();
            mk.SetConnection(RouterID);
            SqlResult result = new SqlResult();
            result = Status == "Enable" ? mk.EnableVlanID(ID, Comment) : mk.DisableVlanID(ID, Comment);
            return Json(new { Result = result.Result, Message = result.Message, Comment = Comment }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ChangStatusPPPoE(int RouterID, string ID, string Comment, string Status)
        {
            Comment = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(Comment));
            HTML html = new HTML();
            Mikrotik mk = new Mikrotik();
            mk.SetConnection(RouterID);
            SqlResult result = new SqlResult();
            result = Status == "Enable" ? mk.EnablePPPoE(ID, Comment) : mk.DisablePPPoE(ID, Comment);
            return Json(new { Result = result.Result, Message = result.Message, Comment = Comment }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportedInvoiceToDMCTAX()
        {
            HTML html = new HTML();
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "cb.InvoiceDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class:",
                DBField = "cb.ClassNameID",
                SQL_CBO = "select distinct c.ClassID, c.ClassName from dbo.TblCustomerBalance cb inner join dbo.TblClass c on c.ClassID = cb.ClassNameID where cb.IsToDMCTAX = 1 and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " order by c.ClassName",
                DisplayText = "ClassName",
                Value = "ClassID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Group:",
                DBField = "c.CustomerGroupID",
                SQL_CBO = "select distinct cg.CustomerGroupID, cg.CustomerGroup from dbo.TblCustomerBalance cb inner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID\r\ninner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID where cb.IsToDMCTAX = 1 and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " order by cg.CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroupID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "c.CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType from dbo.TblCustomerBalance cb\r\ninner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID\r\ninner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID\r\nwhere cb.IsToDMCTAX = 1 and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " order by ct.CustomerType",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Invoice No",
                SQL = "select distinct top 10 cb.InvoiceNo from dbo.TblCustomerBalance cb\r\ninner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID\r\nwhere cb.IsToDMCTAX = 1 and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " order by cb.InvoiceNo",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("cb.InvoiceNo") + "," + EncryAndDecry.Encryp("c.CustomerCode"),
                DefaultCheckBok = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode from dbo.TblCustomerBalance cb\r\ninner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID\r\nwhere cb.IsToDMCTAX = 1 and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " order by c.CustomerCode",
                RadioValue = EncryAndDecry.Encryp("cb.InvoiceNo") + "," + EncryAndDecry.Encryp("c.CustomerCode"),
                ISAutoComplete = true,
            });

            string sql = @"select ROW_NUMBER() over (order by cb.InvoiceNo desc) as [No],
('<a href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+', '''+ cb.InvoiceNo +''','+ convert(varchar, cb.IsIncludedVAT) +','+ convert(varchar, cb.StatusID) +','''',false,this);"">' + cb.InvoiceNo + '</a>') InvoiceNo,
cb.InvoiceDate, cb.PaymentDueDate, cb.GrandTotal,
c.CustomerID, c.CustomerCode, c.CustomerName,
cg.CustomerGroup, l.Salutation+' '+ l.FullName SaleName, cc.ClassName, ct.CustomerType,
case when cb.StatusID = 10 then '<span class=""badge bg-primary"">' + s.[Status] + '</span>' else '<span class=""badge bg-danger"">' + 
case when format(cb.PaymentDueDate, 'yyMMdd') <= format(getdate(), 'yyMMdd') then 'Over Due Payment' else s.[Status] end + '</span>' end [Status]
from dbo.TblCustomerBalance cb
inner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblLeads l on l.LeadID = c.EmployeeID
inner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID
inner join dbo.TblStatus s on s.StatusID = cb.StatusID
where cb.IsToDMCTAX =1 and cb.SystemID =" + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " and cb.StatusID not in (12,13,14,20) and cb.[Type]='Official Invoice'";

            html.OrderBy = "cb.InvoiceNo desc";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            //html.ShowPaging = false;
            //html.ShowPrintOnTopTable = false;
            //html.UsingJqueryDataTable = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
				//new GridField() { Header = "Group", FieldName = "CustomerGroup", IsGroupByField = true, GroupLabel = "Group: ", Colspan = 10 },
				//new GridField() { Header = "Block", FieldName = "CustomerType", IsGroupByField = true, GroupLabel = "Block: ", Colspan = 10 },
				new GridField() { Header = "Invoice#", FieldName = "InvoiceNo" },
                new GridField() { Header = "Inv. Date", FieldName = "InvoiceDate", Format="dd-MM-yy" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", Format = "dd-MM-yy" },
                new GridField() { Header = "Amount", FieldName = "GrandTotal", TextAlign = "right", ClassName="text-danger" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
                new GridField() { Header = "Group", FieldName = "CustomerGroup" },
                new GridField() { Header = "Block", FieldName = "CustomerType" },
                new GridField() { Header = "Sale Name", FieldName = "SaleName" },
                new GridField() { Header = "Class", FieldName = "ClassName" },
                new GridField() { Header = "Status", FieldName = "Status" },
            };
            html.UsingJqueryDataTable = true;
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public ActionResult CustInfoWillSendToDMC()
        {
            HTML html = new HTML();
            ViewBag.DomainID = html.DomainID;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoice Date:",
                DBField = "c.RegisterDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                SQL_CBO = "select distinct cc.ClassID, cc.ClassName\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblLeads l on l.LeadID = cb.EmployeeID\r\ninner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.StatusID not in (12,13,14,20) and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "ClassName",
                Value = "ClassID",
                //ComboBox = true,
                Hide = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Invoice No",
                SQL = "select distinct top 10 cb.InvoiceNo from dbo.TblCustomerBalance cb\r\ninner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID\r\ninner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID where lower(cb.InvoiceNo) like @SearchText and cb.IsToDMCTAX = 1 and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " order by cb.InvoiceNo desc",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("cb.InvoiceNo") + "," + EncryAndDecry.Encryp("c.CustomerCode"),
                DefaultCheckBok = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode from dbo.TblCustomerBalance cb\r\ninner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID\r\ninner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID where lower(c.CustomerCode) like @SearchText and cb.IsToDMCTAX = 1 and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " order by c.CustomerCode",
                RadioValue = EncryAndDecry.Encryp("cb.InvoiceNo") + "," + EncryAndDecry.Encryp("c.CustomerCode"),
                ISAutoComplete = true,
            });
            html.BunSearch = "btns";
            html.OverrideSearch = "ShowCustWillSendToDMC()";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public string ShowCustInfoWillSendToDMC(string FromDate, string ToDate, string InvoiceNo, string CID, bool isOnDMCTax = false, string ClassName = "")
        {
            FromDate = HttpUtility.UrlDecode(FromDate);
            ToDate = HttpUtility.UrlDecode(ToDate);
            InvoiceNo = HttpUtility.UrlDecode(InvoiceNo);
            CID = HttpUtility.UrlDecode(CID);

            HTML html = new HTML();

            string sql = @"select row_number() over (order by c.CustomerCode) as [No], c.CustomerID ID, 
case when c.NewRegisterDate is null then format(c.RegisterDate, 'dd-MMM-yyyy') else format(c.NewRegisterDate, 'dd-MMM-yyyy') end RegisterDate, 
'<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" class=""text-primary"" target=""_blank"">' + c.CustomerCode + '</a>' CustomerCode, isnull(ci.[SID], '') [SID], 
'<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" class=""text-primary"" target=""_blank"">' + c.CustomerName + '</a>' CustomerName,
it.IndustryType,
i.BillToAddress + ', ' + i.BillToCommune + ', ' + i.BillToDistrict + ', ' + i.BillToProvinceCity [Address],
convert(varchar, isnull(ci.Bandwidth, 0)) + ' Mbps' Bandwidth,
case when upper(isnull(pd.PackageName, '')) = '' then 
sr.ServiceName
else upper(isnull(pd.PackageName, '')) end 'ServicePackage',
isnull(ac.ConnectionType, '') 'ConnectionType', 'Unlimited' 'DataUsage',
isnull(s.[Status],'Actived') 'Status', 
case when isnull(ac.LockedDate,'') <> '' and ac.ConnectivityStatus <> 40 then format(ac.LockedDate, 'dd-MMM-yyyy') else '' end 'DeactivateDate'
from dbo.TblCustomerInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblIndustryType it on it.IndustryTypeID = c.IndustryTypeID
inner join dbo.TblCustomerBillInformation i on i.CustomerBillInformationID = ci.CustomerBillInformationID and i.DomainID like '%001,002%'
left join dbo.TblPackageDetail pd on pd.PackageDetailID = ci.PackageDetailID
left join dbo.TblServices sr on sr.ServiceID = ci.ServiceID
left join dbo.TblActivateCustomer ac on ac.CustomerID = ci.CustomerID and ac.DomainID like '%001,002%'
left join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
where ci.DomainID like '%001,002%' and ci.StatusID not in(12,13,14,20) and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID;
            if (ClassName != "")
                sql += @" and ci.ClassNameID = " + ClassName + " ";
            if (FromDate != "")
                sql += @" and format(ci.InvoiceDate, 'yyyy-MM-dd') between '" + FromDate + "' and '" + ToDate + "'";
            if (InvoiceNo != "")
                sql += @" and ci.InvoiceNo='" + InvoiceNo + "' ";
            if (CID != "")
                sql += @" and c.CustomerCode = '" + CID + "' ";
            sql += @" group by c.CustomerID, format(c.RegisterDate, 'dd-MMM-yyyy'), 
c.CustomerCode, isnull(ci.[SID], ''), 
c.CustomerName, it.IndustryType, i.BillToAddress + ', ' + i.BillToCommune + ', ' + i.BillToDistrict + ', ' + i.BillToProvinceCity,
ci.ConnectionID,convert(varchar, isnull(ci.Bandwidth, 0)) + ' Mbps',
case when upper(isnull(pd.PackageName, '')) = '' then 
sr.ServiceName
else upper(isnull(pd.PackageName, '')) end,
isnull(ac.ConnectionType, ''),
isnull(s.[Status],'Actived'),
ac.LockedDate,c.NewRegisterDate,ac.ConnectivityStatus";
            html.OrderBy = "c.CustomerCode, isnull(ci.[SID], '')";
            html.GH.SQL = sql;
            html.IsUseHavingCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Reg.Date", FieldName = "RegisterDate" },
                new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Industry Type", FieldName = "IndustryType" },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Bandwidth", FieldName = "Bandwidth" },
                new GridField() { Header = "Service", FieldName = "ServicePackage" },
                new GridField() { Header = "Connection Type", FieldName = "ConnectionType" },
                new GridField() { Header = "Data Usage", FieldName = "DataUsage" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Deactivate Date", FieldName = "DeactivateDate" },
            };
            //html.UsingJqueryDataTable = true;
            html.ShowPaging = false;
            html.ShowGridOnly = true;
            html.TblID = "tblCurrent";
            //if (isOnDMCTax)
            //{
            //	html.GridUseConnection2 = true;
            //	html.TblID = "tblDMXTax";
            //	ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "loadDMCTAX");
            //}
            //else
            //{
            return html.DrawKendoGrid(); //html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
                                         //}
                                         //return View();
        }

        [HttpGet]
        public async Task<FileResult> PreviewExcel(string FromDate, string ToDate, string InvoiceNo, string CID, bool Upload = false, bool IsToDMCFormat = false)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = @"select '' 'Operator Name: NSM ISP';
select row_number() over (order by isnull(a.[Service ID/Account ID], ''), a.[Customer ID]) as [No],
	a.[Register Date (DD-MMM-YYYY)],
    case when isnull(a.CreateDate, '') = '' then a.[Bill Date (DD-MMM-YYYY)] else a.CreateDate end 'Register Account IDs',
    a.[Bill Date (DD-MMM-YYYY)],    
    a.[Customer ID],
    a.[Service ID/Account ID],
    a.[Name],
    a.[Customer Type (Firm or Individual)],
    a.[Address] 'Installation Address',
    a.[Location],
    a.[Bandwidth],
    a.[Service/Package Type (Tariff)],
    --a.[Connection Type],
    case when a.ServiceID = 1 then case when a.CustomerGroup = 'SME' then 'FTTB' else 'FTTH' end else '' end 'Connection Type',
    a.[Data Usage (Limited or Unlimited)],
    a.[Status (Active or Deactive)],
    a.[Deactivate Date (DD-MMM-YYYY)],
    a.GoogleMap_FullAddress,
a.GoogleMap_Latitude,
a.GoogleMap_Longitude

from(
select 
case when c.NewRegisterDate is null then format(c.RegisterDate, 'dd-MMM-yyyy') else format(c.NewRegisterDate, 'dd-MMM-yyyy') end 'Register Date (DD-MMM-YYYY)', 
(select top 1 format(cci.StartDate, 'dd-MMM-yyyy') from dbo.TblCustomerInvoice cci where cci.CustomerID = c.CustomerID and cci.[SID] = isnull(ci.[SID],'') and cci.DomainID = '001,002' 
and format(cci.InvoiceDate,'yyyy-MM-dd') between '" + FromDate + "' and '" + ToDate + @"'
order by cci.CustomerInvoiceID) 'Bill Date (DD-MMM-YYYY)',
ci.CustomerCode 'Customer ID', isnull(ci.[SID], '') 'Service ID/Account ID', 
trim(REPLACE(
case when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Male' then N'លោក ' + replace(ci.CustomerName, N'លោក','')
when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Female' then N'អ្នកស្រី ' + replace(replace(ci.CustomerName, N'លោកស្រី​',''), N'អ្នកស្រី','')
when c.CustomerGender = 'Male' then 'Mr. ' + replace(replace(c.CustomerName, 'Mr. ', ''), 'Mr.','')
when c.CustomerGender = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(ci.CustomerName, 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
else ci.CustomerName end, ' ', '')) [Name],
case when ci.TypeOfInvoice = 'CM' then 'Individual' else 'Firm' end  'Customer Type (Firm or Individual)',
(select top 1 i.InstallAddress + ' ' + i.InstallCommune + ' ' + i.InstallDistrict + ' ' + i.InstallProvinceCity from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = c.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc) [Address],
(select top 1 replace(i.InstallProvinceCity, N'រាជធានីភ្នំពេញ', N'ភ្នំពេញ') from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = c.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc) [Location],
isnull((select top 1 convert(varchar, isnull(cci.Bandwidth, 0)) + ' Mbps' from dbo.TblCustomerInvoice cci where cci.ServiceID = ci.ServiceID and cci.DomainID = '001,002' and  isnull(cci.Bandwidth, 0) <> 0 and cci.CustomerID = c.CustomerID and cci.ConnectionID = ci.ConnectionID order by cci.CustomerInvoiceID desc),0) Bandwidth,
--isnull(case when upper(isnull(pd.PackageName, '')) = '' then sr.ServiceName else upper(isnull(pd.PackageName, '')) end,'Services') 'Service/Package Type (Tariff)',

upper(isnull((select top 1 pd.PackageName from dbo.TblPackageDetail pd where pd.PackageDetailID = ci.PackageDetailID), (select top 1 s.ServiceName from dbo.TblServices s where s.ServiceID = ci.ServiceID))) 'Service/Package Type (Tariff)',

--case when upper(isnull(pd.PackageName, '')) <> '' then case when c.CustomerGroup = 'SME' then 'FTTB' when c.CustomerGroup='FTTH' then 'FTTH' end else '' end 'Connection Type',
c.CustomerGroup, ci.ServiceID,
'Unlimited' 'Data Usage (Limited or Unlimited)',
isnull(s.[Status],'Actived') 'Status (Active or Deactive)', 
case when isnull(ac.LockedDate,'') <> '' and ac.ConnectivityStatus <> 40 then format(ac.LockedDate, 'dd-MMM-yyyy') else '' end 'Deactivate Date (DD-MMM-YYYY)',
(select top 1 i.GoogleMap_FullAddress from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = ci.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc) GoogleMap_FullAddress,
(select top 1 i.GoogleMap_Latitude from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = ci.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc) GoogleMap_Latitude,
(select top 1 i.GoogleMap_Longitude from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = ci.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc) GoogleMap_Longitude
, format(ac.CreateDate, 'dd-MMM-yyyy') CreateDate
from dbo.TblCustomerInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblIndustryType it on it.IndustryTypeID = c.IndustryTypeID
left join dbo.TblActivateCustomer ac on ac.CustomerID = ci.CustomerID and ac.DomainID = '001,002' and ac.ConnectionID = ci.ConnectionID
left join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
left join dbo.TblServices sr on sr.ServiceID = ci.ServiceID
left join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
where ci.DomainID = '001,002' and c.DomainID = '001,002' and ci.StatusID not in(12,13,14,20) and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID;
            if (FromDate != "")
                sql += @" and format(ci.InvoiceDate,'yyyy-MM-dd') between '" + FromDate + "' and '" + ToDate + "' ";
            if (CID != "")
                sql += " and c.CustomerCode = '" + CID + "' ";
            if (InvoiceNo != "")
                sql += " and ci.InvoiceNo = '" + InvoiceNo + "' ";
            sql = !IsToDMCFormat
                ? @"select ROW_NUMBER() over (order by ci.[SID]) as [No],
case when c.NewRegisterDate is null then format(c.RegisterDate, 'dd-MMM-yyyy') else format(c.NewRegisterDate, 'dd-MMM-yyyy') end 'Register Date (DD-MMM-YYYY)'
, ci.CustomerCode 'Customer ID', 
isnull((select top 1 format(ac.CreateDate, 'dd-MMM-yyyy') from dbo.TblActivateCustomer ac where ac.CustomerID = ci.CustomerID and ac.SID = ci.SID),
case when c.NewRegisterDate is null then format(c.RegisterDate, 'dd-MMM-yyyy') else format(c.NewRegisterDate, 'dd-MMM-yyyy') end) 'Register Account IDs',
ci.[SID] 'Service ID/Account ID',
(select top 1 format(cci.StartDate, 'dd-MMM-yyyy') from dbo.TblCustomerInvoice cci where cci.CustomerID = ci.CustomerID and cci.SID = ci.SID and cci.DomainID = '001,002' order by cci.CustomerInvoiceID asc) 'Bill Date (DD-MMM-YYYY)',
upper(replace(trim(REPLACE(case when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Male' then 'Mr.' + isnull(c.CustomerNameLatin, '')
when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Female' then 'Ms.' + isnull(c.CustomerNameLatin, '')
when c.CustomerGender = 'Male' then 'Mr. ' + replace(replace(ci.CustomerName, 'Mr. ', ''), 'Mr.','')
when c.CustomerGender = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(ci.CustomerName, 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Other' then isnull(c.CustomerNameLatin, '')
else ci.CustomerName end, ' ', '')),',',' ')) [Name],
--case when ci.TypeOfInvoice = 'CM' then 'Individual' else 'Firm' end 'Customer Type (Firm or Individual)',
it.IndustryType 'Customer Type (Firm or Individual)',
replace((select top 1 i.InstallAddress + ', ' + i.InstallCommune + ', ' + i.InstallDistrict + ', ' + i.InstallProvinceCity from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = ci.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc),',',' ') 'Installation Address',
(select top 1 replace(i.InstallProvinceCity, N'រាជធានីភ្នំពេញ', N'ភ្នំពេញ') from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = ci.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc) [Location]
, isnull((select top 1 convert(varchar, isnull(cci.Bandwidth, 0)) + ' Mbps' from dbo.TblCustomerInvoice cci where cci.ServiceID = ci.ServiceID and cci.DomainID = '001,002' and  isnull(cci.Bandwidth, 0) <> 0 and cci.CustomerID = ci.CustomerID and cci.ConnectionID = ci.ConnectionID order by cci.CustomerInvoiceID desc),0) Bandwidth
, upper(isnull((select top 1 pd.PackageName from dbo.TblCustomerInvoice cci inner join dbo.TblPackageDetail pd on pd.PackageDetailID = cci.PackageDetailID where cci.CustomerID = ci.CustomerID and cci.SID = ci.SID and cci.[Type] ='Official Invoice' order by cci.CustomerInvoiceID desc), (select top 1 s.ServiceName from dbo.TblServices s where s.ServiceID = ci.ServiceID))) PackageName
, case when ci.ServiceID = 1 then case when c.CustomerGroup = 'SME' then 'FTTB' else 'FTTH' end else '' end 'Connection Type'
, 'Unlimited' 'Data Usage (Limited or Unlimited)'
, isnull((select top 1 s.[Status] from dbo.TblActivateCustomer ac inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus where ac.CustomerID = ci.CustomerID and ac.[SID] = ci.[SID]), 'Actived') 'Status (Active or Deactive)'
, isnull((select top 1 case when isnull(ac.LockedDate,'') <> '' and ac.ConnectivityStatus <> 40 then format(ac.LockedDate, 'dd-MMM-yyyy') else '' end from dbo.TblActivateCustomer ac where ac.CustomerID = ci.CustomerID and ac.[SID] = ci.[SID]), '') 'Deactivate Date (DD-MMM-YYYY)',
(select top 1 i.GoogleMap_FullAddress from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = ci.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc) GoogleMap_FullAddress,
'=HYPERLINK(""https://www.google.com/maps?q=' + (SELECT TOP 1 convert(varchar(50), i.GoogleMap_Latitude, 128) FROM dbo.TblCustomerBillInformation i WHERE i.DomainID = '001,002' AND i.CustomerID = ci.CustomerID AND i.ConnectionID = ci.ConnectionID ORDER BY CustomerBillInformationID DESC) + ',' + (SELECT TOP 1 convert(varchar(50), i.GoogleMap_Longitude, 128) FROM dbo.TblCustomerBillInformation i WHERE i.DomainID = '001,002' AND i.CustomerID = ci.CustomerID AND i.ConnectionID = ci.ConnectionID ORDER BY CustomerBillInformationID DESC) + '"",""' + (SELECT TOP 1 convert(varchar(50), i.GoogleMap_Latitude, 128) FROM dbo.TblCustomerBillInformation i WHERE i.DomainID = '001,002' AND i.CustomerID = ci.CustomerID AND i.ConnectionID = ci.ConnectionID ORDER BY CustomerBillInformationID DESC) + '"")' AS GoogleMap_Latitude,
'=HYPERLINK(""https://www.google.com/maps?q=' + (SELECT TOP 1 convert(varchar(50), i.GoogleMap_Latitude, 128) FROM dbo.TblCustomerBillInformation i WHERE i.DomainID = '001,002' AND i.CustomerID = ci.CustomerID AND i.ConnectionID = ci.ConnectionID ORDER BY CustomerBillInformationID DESC) + ',' + (SELECT TOP 1 convert(varchar(50), i.GoogleMap_Longitude, 128) FROM dbo.TblCustomerBillInformation i WHERE i.DomainID = '001,002' AND i.CustomerID = ci.CustomerID AND i.ConnectionID = ci.ConnectionID ORDER BY CustomerBillInformationID DESC) + '"",""' + (SELECT TOP 1 convert(varchar(50), i.GoogleMap_Longitude, 128) FROM dbo.TblCustomerBillInformation i WHERE i.DomainID = '001,002' AND i.CustomerID = ci.CustomerID AND i.ConnectionID = ci.ConnectionID ORDER BY CustomerBillInformationID DESC) + '"")' AS GoogleMap_Longitude
from dbo.TblCustomerInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblIndustryType it on it.IndustryTypeID = c.IndustryTypeID
where isnull(ci.[SID], '') <> '' and ci.StatusID not in(12,13,14,20) and c.DomainID = '001,002' and ci.DomainID = '001,002'
and ci.[Type] ='Official Invoice'
and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + @""
                : @"select ROW_NUMBER() over (order by ci.[SID]) as [No],
case when c.NewRegisterDate is null then format(c.RegisterDate, 'dd-MMM-yyyy') else format(c.NewRegisterDate, 'dd-MMM-yyyy') end 'Register Date (DD-MMM-YYYY)'
, ci.CustomerCode 'Customer ID', 
isnull((select top 1 format(ac.CreateDate, 'dd-MMM-yyyy') from dbo.TblActivateCustomer ac where ac.CustomerID = ci.CustomerID and ac.SID = ci.SID),
case when c.NewRegisterDate is null then format(c.RegisterDate, 'dd-MMM-yyyy') else format(c.NewRegisterDate, 'dd-MMM-yyyy') end) 'Register Account IDs',
ci.[SID] 'Service ID/Account ID',
(select top 1 format(cci.StartDate, 'dd-MMM-yyyy') from dbo.TblCustomerInvoice cci where cci.CustomerID = ci.CustomerID and cci.SID = ci.SID and cci.DomainID = '001,002' order by cci.CustomerInvoiceID asc) 'Bill Date (DD-MMM-YYYY)',
upper(replace(trim(REPLACE(case when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Male' then 'Mr.' + isnull(c.CustomerNameLatin, '')
when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Female' then 'Ms.' + isnull(c.CustomerNameLatin, '')
when c.CustomerGender = 'Male' then 'Mr. ' + replace(replace(ci.CustomerName, 'Mr. ', ''), 'Mr.','')
when c.CustomerGender = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(ci.CustomerName, 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Other' then isnull(c.CustomerNameLatin, '')
else ci.CustomerName end, ' ', '')),',',' ')) [Name],
--case when ci.TypeOfInvoice = 'CM' then 'Individual' else 'Firm' end 'Customer Type (Firm or Individual)',
it.IndustryType 'Customer Type (Firm or Individual)',
replace((select top 1 i.InstallAddress + ', ' + i.InstallCommune + ', ' + i.InstallDistrict + ', ' + i.InstallProvinceCity from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = ci.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc),',',' ') 'Installation Address',
(select top 1 replace(i.InstallProvinceCity, N'រាជធានីភ្នំពេញ', N'ភ្នំពេញ') from dbo.TblCustomerBillInformation i where i.DomainID ='001,002' and i.CustomerID = ci.CustomerID and i.ConnectionID = ci.ConnectionID order by CustomerBillInformationID desc) [Location]
, isnull((select top 1 convert(varchar, isnull(cci.Bandwidth, 0)) + ' Mbps' from dbo.TblCustomerInvoice cci where cci.ServiceID = ci.ServiceID and cci.DomainID = '001,002' and  isnull(cci.Bandwidth, 0) <> 0 and cci.CustomerID = ci.CustomerID and cci.ConnectionID = ci.ConnectionID order by cci.CustomerInvoiceID desc),0) Bandwidth
, upper(isnull((select top 1 pd.PackageName from dbo.TblCustomerInvoice cci inner join dbo.TblPackageDetail pd on pd.PackageDetailID = cci.PackageDetailID where cci.CustomerID = ci.CustomerID and cci.SID = ci.SID and cci.[Type] ='Official Invoice' order by cci.CustomerInvoiceID desc), (select top 1 s.ServiceName from dbo.TblServices s where s.ServiceID = ci.ServiceID))) PackageName
, case when ci.ServiceID = 1 then case when c.CustomerGroup = 'SME' then 'FTTB' else 'FTTH' end else '' end 'Connection Type'
, 'Unlimited' 'Data Usage (Limited or Unlimited)'
, isnull((select top 1 s.[Status] from dbo.TblActivateCustomer ac inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus where ac.CustomerID = ci.CustomerID and ac.[SID] = ci.[SID]), 'Actived') 'Status (Active or Deactive)'
, isnull((select top 1 case when isnull(ac.LockedDate,'') <> '' and ac.ConnectivityStatus <> 40 then format(ac.LockedDate, 'dd-MMM-yyyy') else '' end from dbo.TblActivateCustomer ac where ac.CustomerID = ci.CustomerID and ac.[SID] = ci.[SID]), '') 'Deactivate Date (DD-MMM-YYYY)'
from dbo.TblCustomerInvoice ci
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
inner join dbo.TblIndustryType it on it.IndustryTypeID = c.IndustryTypeID
where isnull(ci.[SID], '') <> '' and ci.StatusID not in(12,13,14,20) and c.DomainID = '001,002' and ci.DomainID = '001,002'
and ci.[Type] ='Official Invoice'
and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + @"";
            if (FromDate != "")
                sql += @" and format(ci.InvoiceDate,'yyyy-MM-dd') between '" + FromDate + "' and '" + ToDate + "' ";
            if (CID != "")
                sql += " and ci.CustomerCode = '" + CID + "' ";
            if (InvoiceNo != "")
                sql += " and ci.InvoiceNo = '" + InvoiceNo + "' ";
            sql += @"group by ci.CustomerID,
case when c.NewRegisterDate is null then format(c.RegisterDate, 'dd-MMM-yyyy') else format(c.NewRegisterDate, 'dd-MMM-yyyy') end
, ci.CustomerCode, ci.[SID] 
, replace(trim(REPLACE(case when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Male' then 'Mr.' + isnull(c.CustomerNameLatin, '')
when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Female' then 'Ms.' + isnull(c.CustomerNameLatin, '')
when c.CustomerGender = 'Male' then 'Mr. ' + replace(replace(ci.CustomerName, 'Mr. ', ''), 'Mr.','')
when c.CustomerGender = 'Female' then 'Ms. ' + replace(replace(replace(replace(replace(replace(ci.CustomerName, 'Mrs. ', ''), 'Mrs.',''),'Miss. ',''),'Miss.',''), 'Ms. ',''),'Ms.','')
when lower(ci.CustomerName) not like '%[a-z]%' and c.CustomerGender = 'Other' then isnull(c.CustomerNameLatin, '')
else ci.CustomerName end, ' ', '')),',',' '), ci.TypeOfInvoice, ci.ConnectionID, ci.ServiceID, c.CustomerGroup, it.IndustryType
order by ci.SID, ci.CustomerCode";

            string filepath = HttpContext.Server.MapPath(@"\Uploads\CSV_File_For_DMC\\");
            string fname = DateTime.Parse(html.GetCurrentDate()).ToString("yyyyMM") + "_NSM_ISP_Cust_Info.csv";
            string csv = "";
            if (IsToDMCFormat)
            {
                html.JsonToCustomCsv(html.LoadData(sql), filepath + fname);
            }
            else
            {
                csv = html.JsonToCsv(html.LoadData(sql));//html.JsonToCsv(html.JsonData(sql, false, null, false, html.GetDMCConnectionString()), ",");
            }

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(filepath + fname, Encoding.Unicode)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fname };


            if (!Directory.Exists(filepath))
                Directory.CreateDirectory(filepath);
            if (!IsToDMCFormat)
            {
                System.IO.File.WriteAllText(filepath + fname, csv);

                using (StreamWriter sw = new StreamWriter(new FileStream(filepath + fname, FileMode.Create), Encoding.UTF8))
                {
                    //sw.WriteLine(string.Join(",", csv));
                    sw.WriteLine(csv);
                }
            }

            FileInfo file = new FileInfo(filepath + fname);
            if (!Upload)
            {
                System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = file.Name,
                    Inline = false
                };
                Response.Headers.Add("Content-Disposition", cd.ToString());
                Response.Headers.Add("X-Content-Type-Options", "nosniff");

                string contentType = MimeMapping.GetMimeMapping(file.Name);
                var readStream = System.IO.File.ReadAllBytes(filepath + fname);
                return File(readStream, contentType);
            }
            else
            {
                await html.UploadFileToServer(filepath, fname.ToString(), "/ISP_Customer_Info_List/");
                return File(filepath + fname, MimeMapping.GetMimeMapping(fname));
            }
        }

        public ActionResult CustomerDevice()
        {
            HTML html = new HTML();
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                //DateTimePickerWithRanges = true,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "cd.Date"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "c.CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType\r\nfrom dbo.TblCustomer c inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID inner join dbo.TblCustomerDeviceInfoDetail cd on cd.CustomerID = c.CustomerID where c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " order by ct.CustomerType",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode from dbo.TblCustomer c inner join dbo.TblCustomerDeviceInfoDetail cd on cd.CustomerID = c.CustomerID where lower(c.CustomerCode) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " and c.DomainID like '%'+@DomainID+'%' order by c.CustomerCode",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select distinct top 10 c.CustomerName from dbo.TblCustomer c inner join dbo.TblCustomerDeviceInfoDetail cd on cd.CustomerID = c.CustomerID where lower(c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " and c.DomainID like '%'+@DomainID+'%' order by c.CustomerName",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName"),
                ISAutoComplete = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = @"select ROW_NUMBER() over(order by cd.CustomerDeviceInfoDetailID desc) as [No], 
format(cd.[Date],'dd-MM-yyyy') + '<br><a href=""javascript:void(0)"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+isnull(cd.InvoiceNo,'')+''',''1'',''10'','''',false,this)"">' + isnull(cd.InvoiceNo,'') + '</a>' [Date], 
c.CustomerID, ct.CustomerType + '<br><a href=""javascript:void(0)"" onclick=""OpenNewTap(''#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+''')"">' + c.CustomerCode + '</a><br>' + 
'<a href=""javascript:void(0)"" onclick=""OpenNewTap(''#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+''')"">' + c.CustomerName + '</a>' Customer, 
cd.[Type] + '<br>' +  
cd.ItemName + '<br>' + case when cd.MACAddress <> '' then cd.MACAddress + ' <br>' else '' end + uw.WarehouseName [Item],
cd.ItemName, cd.[Status] + '<br>' + isnull(cd.Reason,'') Remark, cd.InstallBy + '<br>' + format(cd.CompleteInstallDate,'dd-MM-yy HH:mm:ss tt') Installed, cd.InvoiceNo
from dbo.TblCustomerDeviceInfoDetail cd
inner join dbo.TblCustomer c on c.CustomerID = cd.CustomerID
inner join dbo.TblUserWarehouse uw on uw.WID = cd.LocationID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "cd.CustomerDeviceInfoDetailID desc";
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "Date" },
				//new GridField() { Header = "Block", FieldName = "Block" },
				new GridField() { Header = "Customer", FieldName = "Customer" },
				//new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
				//new GridField() { Header = "Type", FieldName = "Type" },
				//new GridField() { Header = "Warehouse", FieldName = "WarehouseName" },
				new GridField() { Header = "Item", FieldName = "Item" },
				//new GridField() { Header = "Serial#", FieldName = "SerialNo" },
				//new GridField() { Header = "MAC Address", FieldName = "MACAddress" },
				//new GridField() { Header = "Status", FieldName = "Status" },
				new GridField() { Header = "Remark", FieldName = "Remark" },
                new GridField() { Header = "Installed", FieldName = "Installed" },
				//new GridField() { Header = "Installed Date", FieldName = "CompleteInstallDate", Format="dd-MM-yy HH:mm:ss tt" },
			};
            html.UsingJqueryDataTable = true;
            html.ShowPaging = false;
            html.JqueryDataTableResponsive = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public ActionResult CustomerInvoiceUploaded()
        {
            HTML html = new MVC.HTML();
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                //DateTimePickerWithRanges = true,
                DateRangePicker = true,
                LabelFromDate = "Date:"
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Invoice No",
                SQL = "select distinct top 10 InvoiceNo from dbo.TblCustomerBalance where Type = 'Official Invoice' and StatusID not in (12,13,14,20) and InvoiceNo like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by InvoiceNo",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("cb.InvoiceNo"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            string sql = @"select Row_Number() over(order by c.CustomerCode) as [No], 
c.CustomerID, c.CustomerCode CID, cb.InvoiceDate, cb.InvoiceNo, cb.IsIncludedVAT, cb.IsUploadToFTP,
cb.UploadDate, cb.UploadMessage
from dbo.TblCustomerBalance cb
inner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID
where cb.SystemID= " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " and isnull(cb.IsUploadToFTP, 0) = 1";
            html.IsHaveWhereCondition = true;
            html.OrderBy = "c.CustomerCode";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate" },
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo" },
                new GridField() { Header = "Upload Message", FieldName = "UploadMessage" },
                new GridField() { Header = "CID", FieldName = "CID" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public ActionResult CustomerPrefixByBlock()
        {
            HTML html = new HTML();
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "c.RegisterDate",
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Block",
                SQL = "select distinct ct.CustomerTypeID, ct.CustomerType from dbo.TblCustomerType ct inner join dbo.TblCustomer c on c.CustomerTypeID = ct.CustomerTypeID where c.SystemID =" + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + "  order by ct.CustomerType",
                SelectValue = "CustomerTypeID",
                DisplayField = "CustomerType",
                RadioValue = EncryAndDecry.Encryp("c.CustomerTypeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = @"select ROW_NUMBER() over(order by ct.CustomerType, c.CustomerCode) as [No] , c.CustomerID, 
ct.CustomerType, c.CustomerCode, TypeOfInvoice, format(c.RegisterDate,'dd-MM-yyyy') as RegisterDate , c.RegisterBy, ct.Prefix, ct.PrefixVAT, ct.PrefixCM
from dbo.TblCustomer c
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "ct.CustomerType, c.CustomerCode";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Block", FieldName = "CustomerType", IsGroupByField = true, GroupLabel = "Block: ", Colspan = 8 },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate" },
                new GridField() { Header = "Register By", FieldName = "RegisterBy",Width="100%"},
                new GridField() { Header = "VAT", FieldName = "TypeOfInvoice" },
                new GridField() { Header = "Prefix", FieldName = "Prefix", TextAlign ="center" },
                new GridField() { Header = "Prefix VAT", FieldName = "PrefixVAT", TextAlign ="center" },
                new GridField() { Header = "Prefix CM", FieldName = "PrefixCM", TextAlign ="center" },
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        public string CusotmerInvoice(string FromDate, string ToDate, string InvoiceNo, string CID, bool isOnDMCTax = false, string ClassName = "", bool isNoSerial = false)
        {
            HTML html = new HTML();
            string sql = $@"select ROW_NUMBER() over(order by format(cb.InvoiceDate, 'yyyyMMdd') desc, cb.InvoiceNo desc) as [No],
('<a class=""text-primary"" href=""javascript:void(0);"" onclick=""PrintInvoice('+convert(varchar, c.CustomerID)+','''+ cb.InvoiceNo +''','''+ convert(varchar, cb.IsIncludedVAT) +''','''+ convert(varchar, cb.StatusID) +''','''',false,this, '''', '''', '''', '''', '''', '''', {isNoSerial.ToString().ToLower()});"">' + cb.InvoiceNo + '</a>') InvoiceNo,
l.FullName, cc.ClassName,
'<a class=""text-primary"" href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a>' CustomerCode, case when isnull(c.CustomerName, '') <> '' then c.CustomerName else c.CustomerNameLatin end as CustomerName, format(cb.InvoiceDate, 'dd-MM-yy') InvoiceDate, format(cb.PaymentDueDate, 'dd-MM-yy') PaymentDueDate, cb.GrandTotal, isnull(cb.CommentInvoice,'') Memo, isnull(c.CustomerNoted, '') Remark,
c.CustomerID, format(cb.InvoiceDate, 'yyyyMMdd') + '-'+ c.CustomerCode +'_'+ REPLACE(cb.InvoiceNo,'/','') + '.pdf' InvoiceFileName,
case when cb.IsUploadToFTP = 1 then '<span class=""badge badge-primary"">Uploaded</span>' else '<span class=""badge badge-danger"">Pending</span>' end UploadStatus,
'<button onclick=""UploadInvoiceTODMC('+convert(varchar, c.CustomerID)+','''+ cb.InvoiceNo +''', '''+ format(cb.InvoiceDate, 'yyyyMMdd') + '-'+ c.CustomerCode +'_'+ REPLACE(cb.InvoiceNo,'/','') + '.pdf'', '''+ cb.TypeOfInvoice +''', '+convert(varchar, isnull(cb.ReviseCount, 0))+');"" class=""btn btn-danger btn-sm"">Upload</button>' Upload,
case when '" + html.DomainID + @"' = '001,002' then case when isnull(c.NewRegisterDate,'') <> '' then isnull(c.NewRegisterDate,'') else c.RegisterDate end else c.RegisterDate end RegisterDate
from dbo.TblCustomer c
inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID
inner join dbo.TblLeads l on l.LeadID = cb.EmployeeID
inner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.StatusID not in(12,13,14,20) and not (cb.InvoiceNo like '%PI-%' or cb.InvoiceNo like '%CISP%') and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " and cb.DomainID like '%001,002%' ";
            if (ClassName != "")
                sql += @" and cb.ClassNameID = " + ClassName + " ";
            if (FromDate != "")
                sql += @" and format(cb.InvoiceDate, 'yyyy-MM-dd') between '" + FromDate + "' and '" + ToDate + "'";
            if (InvoiceNo != "")
                sql += @" and cb.InvoiceNo='" + InvoiceNo + "' ";
            if (CID != "")
                sql += @" and c.CustomerCode = '" + CID + "' ";
            html.OrderBy = "format(cb.InvoiceDate, 'yyyyMMdd') desc, cb.InvoiceNo desc";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.ShowPaging = false;
            html.ShowPrintOnTopTable = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo" },
                new GridField() { Header = "Sale​​", FieldName = "FullName" },
                new GridField() { Header = "Class", FieldName = "ClassName" },
                new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID=" },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", TextAlign="center", FormatDateTime="dd-MM-yy" },
                new GridField() { Header = "Inv. Date", FieldName = "InvoiceDate", TextAlign="center" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", TextAlign="center", TotalLabel = "Total" },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", Format="#,#0.00", TextAlign = "right", ClassName="text-danger text-right", IsTotalFooter = true },
                new GridField() { Header = "Invoice File Name", FieldName = "InvoiceFileName" },
                new GridField() { Header = "Status", FieldName = "UploadStatus", TextAlign="center" },
                new GridField() { Header = "Upload", FieldName = "Upload", TextAlign="center" },
            };
            html.ShowGridOnly = true;
            return html.DrawKendoGrid();
            //return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());

            //return View();
        }

        public async Task<ActionResult> UploadInvoiceTODMC(int CustomerID, string InvoiceNo, string FileName = "", string TypeOfInvoice = "", int ReviseCount = 0, bool UploadToDMC = false)
        {
            SqlResult sqlResult = new SqlResult();
            DataSet ds = new DataSet();
            HTML html = new HTML();
            if (FileName == "")
            {
                string sql = @"select format(bi.InvoiceDate, 'yyyyMMdd') + '-'+ c.CustomerCode +'_'+ REPLACE(bi.InvoiceNo,'/','') + '.pdf' [FileName],
bi.TypeOfInvoice, isnull(bi.ReviseCount,0) ReviseCount
from dbo.TblCustomerBalance bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where bi.CustomerID = " + CustomerID + " and bi.InvoiceNo = '" + InvoiceNo + "'";
                ds = html.LoadData(sql);
                DataRow dr = ds.Tables[0].Rows[0];
                FileName = dr["FileName"].ToString();
                TypeOfInvoice = dr["TypeOfInvoice"].ToString();
                ReviseCount = int.Parse(dr["ReviseCount"].ToString());
            }
            Dictionary<string, string> para = new Dictionary<string, string>()
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "CustID", CustomerID.ToString() },
                {"InvoiceNo", InvoiceNo },
                { "ISVAT", "0" },
                { "DomainID", html.DomainID }
            };
            ds = html.LoadData("SP_CUSTOMER_INVOICE", true, para);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(html.DrawInvoice(ds, false, 0, 0, false));
            string FilePaths = html.httpContext.Server.MapPath("~/Templates/HtmlTemplate.html");
            StreamReader str = new StreamReader(FilePaths);
            string Text = str.ReadToEnd();
            str.Close();

            Text = Text.Replace("{Title}", FileName == "" ? "Customer Invoice" : FileName);
            Text = Text.Replace("{BodyMessage}", sb.ToString());

            //byte[] pdfBytes = pdfs.CreatePdfFromHtml(Text, FileName);

            //var a = File(pdfBytes, "application/pdf", FileName);
            //var fpath = html.httpContext.Server.MapPath("~/Uploads/InvoiceToDMC");
            //fpath = System.IO.Path.Combine(fpath, FileName);
            //html.SaveByteArrayToFileWithFileStream(pdfBytes, fpath);
            //sqlResult.OutPutFile = fpath;
            sqlResult = await html.HTMLToPDF(Text, "Uploads/InvoiceToDMC", FileName, false, false);
            if (sqlResult.Result == ResultType.Successful)
            {
                string f = sqlResult.OutPutFile.Replace(FileName, "");

                FileInfo fileInfo = new FileInfo(f);
                string uploadFolder = "";
                await html.UploadFileToServer(f, "", "/ISP_Invoice/", UploadToDMC, UploadToDMC);
                string filePath;
                string newFileName;
                if (TypeOfInvoice == "CM" && ReviseCount == 0)
                {
                    uploadFolder = "Com_Invoice";
                    await html.UploadFileToServer(f, FileName, "/Com_Invoice/", UploadToDMC, UploadToDMC);
                }
                else if (TypeOfInvoice == "CM" && ReviseCount > 0)
                {
                    uploadFolder = "Com_Invoice_Adjustment";
                    filePath = fileInfo.DirectoryName;
                    newFileName = fileInfo.Name.Replace(".pdf", "_Rev" + ReviseCount + ".pdf");
                    filePath = System.IO.Path.Combine(filePath, newFileName);
                    System.IO.File.Move(f, filePath);
                    f = filePath;
                    await html.UploadFileToServer(f, FileName, "/Com_Invoice_Adjustment/", UploadToDMC, UploadToDMC);
                }
                else if (TypeOfInvoice == "VAT" && ReviseCount == 0)
                {
                    uploadFolder = "Tax_Invoice";
                    await html.UploadFileToServer(f, FileName, "/Tax_Invoice/", UploadToDMC, UploadToDMC);
                }
                else if (TypeOfInvoice == "VAT" && ReviseCount > 0)
                {
                    uploadFolder = "Tax_Invoice_Adjustment";
                    filePath = fileInfo.DirectoryName;
                    newFileName = fileInfo.Name.Replace(".pdf", "_Rev" + ReviseCount + ".pdf");
                    filePath = System.IO.Path.Combine(filePath, newFileName);
                    System.IO.File.Move(f, filePath);
                    f = filePath;
                    await html.UploadFileToServer(f, FileName, "/Tax_Invoice_Adjustment/", UploadToDMC, UploadToDMC);
                }
                if (sqlResult.Result == ResultType.Successful)
                {
                    string sql = @"update dbo.TblCustomerBalance set IsUploadToFTP = 1, ReviseCount = isnull(ReviseCount,0) + 1, UploadDate='" + html.GetCurrentDate() + "', UploadBy='" + html.UserFullName + "'  where InvoiceNo = '" + InvoiceNo + "' and CustomerID = " + CustomerID;
                    sqlResult = html.SaveChangeData(sql, false, false);
                    sqlResult.Message = "File Upload to folder " + uploadFolder + " successfully.";
                    //NotificationTelegram telegram = new NotificationTelegram();
                    //await telegram.NotificationTelegramBot("", -4032348170, "", f, sqlResult.Message);
                }
            }


            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowCustomerByBlockAndConnection(string FromDate, string ToDate)
        {
            HTML html = new HTML();
            string sql = @"select ct.CustomerType, ct.DistrictCode, count(distinct acc.CustomerID) Customer, count(distinct acc.ConnectionID) Connection,
(select count(distinct ac.ConnectionID) NewConnectionID
from dbo.TblActivateCustomer ac
inner join dbo.TblCustomer cc on cc.CustomerID = ac.CustomerID and ac.DomainID like '%'+@DomainID+'%'
where cc.CustomerTypeID = c.CustomerTypeID and ac.ConnectivityStatus = 40 and format(ac.ActivateDate, 'yyyyMMdd') between @FromDate and @ToDate and cc.SystemID = @SystemID and cc.WarehouseID = @WarehouseID) NewConnectionID,
(select count(distinct ac.ConnectionID) LockedConnectionID
from dbo.TblActivateCustomer ac
inner join dbo.TblCustomer cc on cc.CustomerID = ac.CustomerID and ac.DomainID like '%'+@DomainID+'%'
where cc.CustomerTypeID = c.CustomerTypeID  and ac.ConnectivityStatus = 41
and format(ac.LockedDate, 'yyyyMMdd') between @FromDate and @ToDate and cc.SystemID = @SystemID and cc.WarehouseID = @WarehouseID) LockedConnectionID,
(select count(distinct ac.ConnectionID) TerminatedConnectionID
from dbo.TblActivateCustomer ac
inner join dbo.TblCustomer cc on cc.CustomerID = ac.CustomerID and ac.DomainID like '%'+@DomainID+'%'
where cc.CustomerTypeID = c.CustomerTypeID  and ac.ConnectivityStatus = 42
and format(ac.LockedDate, 'yyyyMMdd') between @FromDate and @ToDate and cc.SystemID = @SystemID and cc.WarehouseID = @WarehouseID) TerminatedConnectionID,
(select count(distinct ac.ConnectionID)
from dbo.TblActivateCustomer ac
inner join dbo.TblCustomer cc on cc.CustomerID = ac.CustomerID and ac.DomainID like '%'+@DomainID+'%'
where cc.CustomerTypeID = c.CustomerTypeID  and ac.ConnectivityStatus = 40
and format(ac.RenewDate, 'yyyyMMdd') between @FromDate and @ToDate and cc.SystemID = @SystemID and cc.WarehouseID = @WarehouseID) Renew

from dbo.TblCustomer c
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer acc on acc.CustomerID = c.CustomerID
where acc.DomainID like '%'+@DomainID+'%' and c.SystemID = @SystemID and c.WarehouseID = @WarehouseID
and case when @DomainID = '001,002' then format(c.NewRegisterDate,'yyyyMMdd') else  format(c.RegisterDate,'yyyyMMdd') end <= @ToDate
group by ct.CustomerType, ct.DistrictCode, c.CustomerTypeID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID },
                { "FromDate", FromDate },
                { "ToDate", ToDate },
            };
            ViewBag.Data = html.JsonData(sql, false, para);

            return View();
        }

        public string CustSearchDate(string FromDate, string ToDate)
        {
            HTML html = new HTML();
            string sql = @"declare @FromDate datetime = '" + FromDate + "', @ToDate datetime = '" + ToDate + "', @DomainID nvarchar(50) = '" + html.DomainID + @"',
@SystemID int =" + html.SystemID + ", @WarehouseID int = " + html.WarehouseID + @";

select row_number() over(order by a.CustomerGroup, a.DistrictCode, a.Block) as [No], a.*, (a.NewConnectionAmount + a.RenewAmount) Total from (
select c.CustomerTypeID,
cg.CustomerGroup,
ct.CustomerType + ' - ' + ct.DistrictCode [Block], ct.CustomerType,ct.DistrictCode,
count(distinct acc.CustomerID) Customer, 
count(acc.ConnectionID) Connection,
isnull((select count(distinct ac.ConnectionID) from dbo.TblCustomerBalance ci
inner join dbo.TblCustomer cc on cc.CustomerID = ci.CustomerID and cc.DomainID like '%'+@DomainID+'%' and ci.DomainID like '%'+@DomainID+'%'
inner join dbo.TblActivateCustomer ac on ac.CustomerID = cc.CustomerID
where format(ac.ActivateDate, 'yyyyMMdd') between @FromDate and @ToDate and format(ci.StartDate, 'yyyyMMdd') between @FromDate and @ToDate and ci.InvoiceType='Created Invoice' and ci.[Type]='Official Invoice'
and cc.CustomerTypeID = c.CustomerTypeID and cc.CustomerGroupID=c.CustomerGroupID and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID),0) NewConnectionID,
(select count(distinct ac.ConnectionID) from dbo.TblActivateCustomer ac
inner join dbo.TblCustomer cc on cc.CustomerID = ac.CustomerID and ac.DomainID like '%'+@DomainID+'%'
where cc.CustomerTypeID = c.CustomerTypeID  and ac.ConnectivityStatus = 41 and cc.CustomerGroupID = c.CustomerGroupID
and format(ac.LockedDate, 'yyyyMMdd') between @FromDate and @ToDate and cc.SystemID = @SystemID and cc.WarehouseID = @WarehouseID) LockedConnectionID,
(select count(distinct ac.ConnectionID) TerminatedConnectionID
from dbo.TblActivateCustomer ac
inner join dbo.TblCustomer cc on cc.CustomerID = ac.CustomerID and ac.DomainID like '%'+@DomainID+'%'
where cc.CustomerTypeID = c.CustomerTypeID  and ac.ConnectivityStatus = 42
and format(ac.LockedDate, 'yyyyMMdd') between @FromDate and @ToDate and cc.SystemID = @SystemID and cc.WarehouseID = @WarehouseID) TerminatedConnectionID,
isnull((select count(distinct ac.ConnectionID) from dbo.TblCustomerBalance ci
inner join dbo.TblCustomer cc on cc.CustomerID = ci.CustomerID and cc.DomainID like '%'+@DomainID+'%' and ci.DomainID like '%'+@DomainID+'%'
inner join dbo.TblActivateCustomer ac on ac.CustomerID = cc.CustomerID
where format(ci.StartDate, 'yyyyMMdd') between @FromDate and @ToDate  and ci.InvoiceType='Renew Invoice' and ci.[Type]='Official Invoice'
and cc.CustomerTypeID = c.CustomerTypeID and cc.CustomerGroupID=c.CustomerGroupID and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID),0) RenewConnection,

isnull((select SUM(ci.GrandTotal) from dbo.TblCustomerBalance ci
inner join dbo.TblCustomer cc on cc.CustomerID = ci.CustomerID and cc.DomainID like '%'+@DomainID+'%' and ci.DomainID like '%'+@DomainID+'%'
where format(ci.StartDate, 'yyyyMMdd') between @FromDate and @ToDate  and ci.InvoiceType='Created Invoice' and ci.[Type]='Official Invoice'
and cc.CustomerTypeID = c.CustomerTypeID and cc.CustomerGroupID=c.CustomerGroupID and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID),0) NewConnectionAmount,

isnull((select SUM(ci.GrandTotal) from dbo.TblCustomerBalance ci
inner join dbo.TblCustomer cc on cc.CustomerID = ci.CustomerID and cc.DomainID like '%'+@DomainID+'%' and ci.DomainID like '%'+@DomainID+'%'
inner join dbo.TblActivateCustomer ac on ac.CustomerID = cc.CustomerID
where format(ci.StartDate, 'yyyyMMdd') between @FromDate and @ToDate  and ci.InvoiceType='Renew Invoice' and ci.[Type]='Official Invoice'
and cc.CustomerTypeID = c.CustomerTypeID and cc.CustomerGroupID=c.CustomerGroupID and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID),0) RenewAmount

from dbo.TblCustomer c
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer acc on acc.CustomerID = c.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
where acc.ConnectivityStatus <> 42 and acc.DomainID like '%'+@DomainID+'%' and c.SystemID = @SystemID and c.WarehouseID = @WarehouseID
and case when @DomainID = '001,002' then format(c.NewRegisterDate,'yyyyMMdd') else  format(c.RegisterDate,'yyyyMMdd') end <= @ToDate
group by cg.CustomerGroup, ct.CustomerType, ct.DistrictCode, c.CustomerTypeID, c.CustomerGroupID
) as a";
            html.GH.SQL = sql;
            //Dictionary<string, string> para = new Dictionary<string, string>
            //{
            //    { "SystemID", html.SystemID },
            //    { "WarehouseID", html.WarehouseID },
            //    { "DomainID", html.DomainID },
            //    { "FromDate", FromDate },
            //    { "ToDate", ToDate },
            //};
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.KendoGridExportExcelByManualCode = false;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerTypeID", FieldName = "CustomerTypeID", IsHiddenField = true },
                new GridField() { Header = "Group", FieldName = "CustomerGroup", IsGroupByField = true, SumGroupByFieldName="Customer" },
                new GridField() { Header = "Block", FieldName = "Block", TotalLabel = "Total:" },
                new GridField() { Header = "Customer", FieldName = "Customer", TextAlign = "center", Format="n0",
                    OnClickFunction="ShowCustByBlock(this,'#=CustomerTypeID#','#=Block#', 'Customer','#=CustomerGroup#');",
                    IsTotalFooter = true, },
                new GridField() { Header = "Connection", FieldName = "Connection", TextAlign = "center", Format="n0",
                    OnClickFunction="ShowCustByBlock(this,'#=CustomerTypeID#','#=Block#', 'Connection','#=CustomerGroup#');",
                    IsTotalFooter = true },
                new GridField() { Header = "New Connection", FieldName = "NewConnectionID", TextAlign = "center", Format="n0",
                    OnClickFunction="ShowCustByBlock(this,'#=CustomerTypeID#','#=Block#', 'NewConnection','#=CustomerGroup#');",
                    IsTotalFooter = true },
                new GridField() { Header = "Renew", FieldName = "RenewConnection", TextAlign = "center", Format="n0",
                    OnClickFunction="ShowCustByBlock(this,'#=CustomerTypeID#','#=Block#', 'Renew','#=CustomerGroup#');",
                    IsTotalFooter = true },
                new GridField() { Header = "Locked Connection", FieldName = "LockedConnectionID", TextAlign = "center", Format="n0",
                    OnClickFunction="ShowCustByBlock(this,'#=CustomerTypeID#','#=Block#', 'Locked','#=CustomerGroup#');",
                    IsTotalFooter = true },

                new GridField() { Header = "New Conn.Amount", FieldName = "NewConnectionAmount", TextAlign = "right", Format="c",
                    IsTotalFooter = true },
                new GridField() { Header = "Renew Amount", FieldName = "RenewAmount", TextAlign = "right", Format="c",
                    IsTotalFooter = true },
                new GridField() { Header = "Total", FieldName = "Total", TextAlign = "right", Format="c",
                    IsTotalFooter = true },
				//new GridField() { Header = "Terminated Connection", FieldName = "TerminatedConnectionID", TextAlign = "right", Format="n0",
				//	OnClickFunction="ShowCustByBlock(this,'#=CustomerTypeID#','#=Block#', 'Terminated','#=CustomerGroup#');",
				//	IsTotalFooter = true },
			};
            //html.KendoGridAutoFitColumn = false;
            html.ReportTitle = "Customer Connection By Block";
            html.KendoGridAutoFitColumn = false;
            return html.DrawKendoGrid();
        }

        public string ShowCustByBlock(int CustomerTypeID, string Show, string FromDate, string ToDate, string CustomerGroup)
        {
            HTML html = new HTML();
            string sql = "";
            if (Show == "Customer")
            {
                sql = @"select row_number() over(order by c.CustomerCode) as [No], cg.CustomerGroup, ct.CustomerType + ' - ' + ct.DistrictCode [Block], c.CustomerID,
'<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" target=""_blank"">' + c.CustomerCode + '</a>' CustomerCode, 
'<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" target=""_blank"">' + c.CustomerName + '</a>' CustomerName, c.CustomerAddress + ', ' + c.CustomerCommune + ', ' + c.CustomerDistrict + ', ' + c.CustomerProvinceCity [Address]
, c.CustomerContactMobileNumber Phone from dbo.TblCustomer c
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
where ac.ConnectivityStatus <> 42 and cg.CustomerGroup = N'" + CustomerGroup + "' and c.CustomerTypeID = " + CustomerTypeID + " and ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + @"%'
group by ct.CustomerType + ' - ' + ct.DistrictCode, c.CustomerID,
c.CustomerCode, c.CustomerName, c.CustomerAddress + ', ' + c.CustomerCommune + ', ' + c.CustomerDistrict + ', ' + c.CustomerProvinceCity
, c.CustomerContactMobileNumber, cg.CustomerGroup";
                html.OrderBy = "c.CustomerCode";
                html.GH.GridTable = new GridField[]
                {
                new GridField() { Header = "No", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Group", FieldName = "CustomerGroup" },
                new GridField() { Header = "Block", FieldName = "Block" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Phone", FieldName = "Phone" },
                };
            }
            else if (Show == "Connection")
            {
                sql = @"select row_number() over(order by c.CustomerCode) as [No],cg.CustomerGroup, ct.CustomerType + ' - ' + ct.DistrictCode [Block], c.CustomerID,
'<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" target=""_blank"">' + c.CustomerCode + '</a>' CustomerCode, 
'<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" target=""_blank"">' + c.CustomerName + '</a>' CustomerName, c.CustomerAddress + ', ' + c.CustomerCommune + ', ' + c.CustomerDistrict + ', ' + c.CustomerProvinceCity [Address]
, c.CustomerContactMobileNumber Phone, 
pd.PackageName + ' - ' + convert(varchar, ac.Bandwidth) + ' Mpbs'
+ '<br>' +
format(ac.StartDate, 'dd-MM-yy') +' - '+
format(ac.EndDate, 'dd-MM-yy') + '<br>' + convert(varchar, DATEDIFF(d, ac.StartDate, ac.EndDate)) + ' Days, ' + 
convert(varchar, DATEDIFF(m, ac.StartDate, ac.EndDate)) + ' Months <br><span class=""text-red""><b>' +
case when DATEDIFF(d, getdate(), ac.EndDate) > 0 then case when DATEDIFF(d, getdate(), ac.StartDate) <= 0 then 
'used ' +
convert(varchar, case when DATEDIFF(day, ac.StartDate, GETDATE()) > 0 then 
convert(varchar, DATEDIFF(day, ac.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), ac.EndDate)) + ' day left '
else '0' end) 
else 'Expired' end else 'Expired' end + '</b></span>' Package,
ac.ConnectionID, case when s.[Status] = 'Actived' then '<span class=""badge bg-primary"">' + s.[Status] + '</span>' else '<span class=""badge bg-danger"">' + s.[Status] + '</span>' end [Status]
from dbo.TblCustomer c
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID
inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
where ac.ConnectivityStatus <> 42 and c.CustomerTypeID = " + CustomerTypeID + " and cg.CustomerGroup = N'" + CustomerGroup + "' and ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + @"%'
and case when '" + html.DomainID + "' = '001,002' then format(c.NewRegisterDate,'yyyyMMdd') else  format(c.RegisterDate,'yyyyMMdd') end <= '" + ToDate + @"'
group by ct.CustomerType + ' - ' + ct.DistrictCode, c.CustomerID,
c.CustomerCode, c.CustomerName, c.CustomerAddress + ', ' + c.CustomerCommune + ', ' + c.CustomerDistrict + ', ' + c.CustomerProvinceCity
, c.CustomerContactMobileNumber, s.[Status],pd.PackageName,ac.Bandwidth,ac.StartDate,ac.EndDate,ac.ConnectionID,cg.CustomerGroup";
                html.OrderBy = "c.CustomerCode";
            }
            else if (Show == "NewConnection")
            {
                sql = @"select row_number() over (order by cc.CustomerCode) as [No], cg.CustomerGroup, ct.CustomerType Block, 
'<a href=""#!/CustomerProfile?ID='+convert(varchar, cc.CustomerID)+'"" target=""_blank"">' + cc.CustomerCode + '</a>' CustomerCode,
'<a href=""#!/CustomerProfile?ID='+convert(varchar, cc.CustomerID)+'"" target=""_blank"">' + cc.CustomerName + '</a>' CustomerName,
cc.CustomerAddress + ', ' + cc.CustomerCommune + ', ' + cc.CustomerDistrict + ', ' + cc.CustomerAddress [Address], cc.CustomerContactMobileNumber Phone,
bii.PackageName + ' - ' + convert(varchar, ac.Bandwidth) + ' Mpbs'
+ '<br>' +
format(bii.StartDate, 'dd-MM-yy') +' - '+
format(bii.EndDate, 'dd-MM-yy') + '<br>' + convert(varchar, DATEDIFF(d, bii.StartDate, bii.EndDate)) + ' Days, ' + 
convert(varchar, DATEDIFF(m, bii.StartDate, bii.EndDate)) + ' Months <br><span class=""text-red""><b>' +
case when DATEDIFF(d, getdate(), bii.EndDate) > 0 then case when DATEDIFF(d, getdate(), bii.StartDate) <= 0 then 
'used ' +
convert(varchar, case when DATEDIFF(day, bii.StartDate, GETDATE()) > 0 then 
convert(varchar, DATEDIFF(day, bii.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), bii.EndDate)) + ' day left '
else '0' end) 
else '' end else 'Expired' end + '</b></span>' Package, s.[Status], ac.ConnectionID
from dbo.TblCustomerBalance ci
inner join dbo.TblCustomer cc on cc.CustomerID = ci.CustomerID and cc.DomainID like '%" + html.DomainID + "%' and ci.DomainID like '%" + html.DomainID + @"%'
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = cc.CustomerGroupID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = cc.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = cc.CustomerID
inner join dbo.TblCustomerBillInformation bi on bi.CustomerID = cc.CustomerID
inner join dbo.TblCustomerBillingInvoice bii on bii.CustomerID = cc.CustomerID and bii.InvoiceNo = ci.InvoiceNo
inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
where format(ac.ActivateDate, 'yyyyMMdd') between '" + FromDate + "' and '" + ToDate + @"' 
and format(ci.StartDate, 'yyyyMMdd') between '" + FromDate + "' and '" + ToDate + @"' 
and ci.InvoiceType='Created Invoice' and ci.[Type]='Official Invoice'
and cc.CustomerTypeID = " + CustomerTypeID + " and cg.CustomerGroup=N'" + CustomerGroup + "' and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + @"
group by cc.CustomerID, cg.CustomerGroup, ct.CustomerType, cc.CustomerCode, cc.CustomerName,
cc.CustomerAddress + ', ' + cc.CustomerCommune + ', ' + cc.CustomerDistrict + ', ' + cc.CustomerAddress, cc.CustomerContactMobileNumber,
bii.PackageName + ' - ' + convert(varchar, ac.Bandwidth) + ' Mpbs'
+ '<br>' +
format(bii.StartDate, 'dd-MM-yy') +' - '+
format(bii.EndDate, 'dd-MM-yy') + '<br>' + convert(varchar, DATEDIFF(d, bii.StartDate, bii.EndDate)) + ' Days, ' + 
convert(varchar, DATEDIFF(m, bii.StartDate, bii.EndDate)) + ' Months <br><span class=""text-red""><b>' +
case when DATEDIFF(d, getdate(), bii.EndDate) > 0 then case when DATEDIFF(d, getdate(), bii.StartDate) <= 0 then 
'used ' +
convert(varchar, case when DATEDIFF(day, bii.StartDate, GETDATE()) > 0 then 
convert(varchar, DATEDIFF(day, bii.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), bii.EndDate)) + ' day left '
else '0' end) 
else '' end else 'Expired' end + '</b></span>', s.[Status], ac.ConnectionID";
                html.OrderBy = "c.CustomerCode";
            }
            else if (Show == "Renew")
            {
                sql = @"select row_number() over (order by cc.CustomerCode) as [No], cg.CustomerGroup, ct.CustomerType Block, 
'<a href=""#!/CustomerProfile?ID='+convert(varchar, cc.CustomerID)+'"" target=""_blank"">' + cc.CustomerCode + '</a>' CustomerCode,
'<a href=""#!/CustomerProfile?ID='+convert(varchar, cc.CustomerID)+'"" target=""_blank"">' + cc.CustomerName + '</a>' CustomerName,
cc.CustomerAddress + ', ' + cc.CustomerCommune + ', ' + cc.CustomerDistrict + ', ' + cc.CustomerAddress [Address], cc.CustomerContactMobileNumber Phone,
bii.PackageName + ' - ' + convert(varchar, ac.Bandwidth) + ' Mpbs'
+ '<br>' +
format(bii.StartDate, 'dd-MM-yy') +' - '+
format(bii.EndDate, 'dd-MM-yy') + '<br>' + convert(varchar, DATEDIFF(d, bii.StartDate, bii.EndDate)) + ' Days, ' + 
convert(varchar, DATEDIFF(m, bii.StartDate, bii.EndDate)) + ' Months <br><span class=""text-red""><b>' +
case when DATEDIFF(d, getdate(), bii.EndDate) > 0 then case when DATEDIFF(d, getdate(), bii.StartDate) <= 0 then 
'used ' +
convert(varchar, case when DATEDIFF(day, bii.StartDate, GETDATE()) > 0 then 
convert(varchar, DATEDIFF(day, bii.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), bii.EndDate)) + ' day left '
else '0' end) 
else '' end else 'Expired' end + '</b></span>' Package, s.[Status], ac.ConnectionID
from dbo.TblCustomerBalance ci
inner join dbo.TblCustomer cc on cc.CustomerID = ci.CustomerID and cc.DomainID like '%" + html.DomainID + "%' and ci.DomainID like '%" + html.DomainID + @"%'
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = cc.CustomerGroupID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = cc.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = cc.CustomerID
inner join dbo.TblCustomerBillInformation bi on bi.CustomerID = cc.CustomerID
inner join dbo.TblCustomerBillingInvoice bii on bii.CustomerID = cc.CustomerID and bii.InvoiceNo = ci.InvoiceNo
inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
where format(ci.StartDate, 'yyyyMMdd') between '" + FromDate + "' and '" + ToDate + @"'  and ci.InvoiceType='Renew Invoice' and ci.[Type]='Official Invoice'
and cc.CustomerTypeID = " + CustomerTypeID + " and cg.CustomerGroup=N'" + CustomerGroup + "' and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + @"
group by cc.CustomerID, cg.CustomerGroup, ct.CustomerType, cc.CustomerCode, cc.CustomerName,
cc.CustomerAddress + ', ' + cc.CustomerCommune + ', ' + cc.CustomerDistrict + ', ' + cc.CustomerAddress, cc.CustomerContactMobileNumber,
bii.PackageName + ' - ' + convert(varchar, ac.Bandwidth) + ' Mpbs'
+ '<br>' +
format(bii.StartDate, 'dd-MM-yy') +' - '+
format(bii.EndDate, 'dd-MM-yy') + '<br>' + convert(varchar, DATEDIFF(d, bii.StartDate, bii.EndDate)) + ' Days, ' + 
convert(varchar, DATEDIFF(m, bii.StartDate, bii.EndDate)) + ' Months <br><span class=""text-red""><b>' +
case when DATEDIFF(d, getdate(), bii.EndDate) > 0 then case when DATEDIFF(d, getdate(), bii.StartDate) <= 0 then 
'used ' +
convert(varchar, case when DATEDIFF(day, bii.StartDate, GETDATE()) > 0 then 
convert(varchar, DATEDIFF(day, bii.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), bii.EndDate)) + ' day left '
else '0' end) 
else '' end else 'Expired' end + '</b></span>', s.[Status], ac.ConnectionID";
                html.OrderBy = "cc.CustomerCode";
            }
            else if (Show == "Locked")
            {
                sql = @"select row_number() over(order by c.CustomerCode) as [No],cg.CustomerGroup, ct.CustomerType + ' - ' + ct.DistrictCode [Block], c.CustomerID,
'<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" target=""_blank"">' + c.CustomerCode + '</a>' CustomerCode, 
'<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" target=""_blank"">' + c.CustomerName + '</a>' CustomerName, c.CustomerAddress + ', ' + c.CustomerCommune + ', ' + c.CustomerDistrict + ', ' + c.CustomerProvinceCity [Address]
, c.CustomerContactMobileNumber Phone, 
pd.PackageName + ' - ' + convert(varchar, ac.Bandwidth) + ' Mpbs'
+ '<br>' +
format(ac.StartDate, 'dd-MM-yy') +' - '+
format(ac.EndDate, 'dd-MM-yy') + '<br>' + convert(varchar, DATEDIFF(d, ac.StartDate, ac.EndDate)) + ' Days, ' + 
convert(varchar, DATEDIFF(m, ac.StartDate, ac.EndDate)) + ' Months <br><span class=""text-red""><b>' +
case when DATEDIFF(d, getdate(), ac.EndDate) > 0 then case when DATEDIFF(d, getdate(), ac.StartDate) <= 0 then 
'used ' +
convert(varchar, case when DATEDIFF(day, ac.StartDate, GETDATE()) > 0 then 
convert(varchar, DATEDIFF(day, ac.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), ac.EndDate)) + ' day left '
else '0' end) 
else 'Expired' end else 'Expired' end + '</b></span>' Package,
ac.ConnectionID, case when s.[Status] = 'Actived' then '<span class=""badge bg-primary"">' + s.[Status] + '</span>' else '<span class=""badge bg-danger"">' + s.[Status] + '</span>' end [Status]
from dbo.TblCustomer c
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID
inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
where c.CustomerTypeID = " + CustomerTypeID + " and cg.CustomerGroup = N'" + CustomerGroup + "' and ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + @"%'
and ac.ConnectivityStatus = 41 and format(ac.LockedDate, 'yyyyMMdd') between '" + FromDate + "' and '" + ToDate + @"'
group by ct.CustomerType + ' - ' + ct.DistrictCode, c.CustomerID,
c.CustomerCode, c.CustomerName, c.CustomerAddress + ', ' + c.CustomerCommune + ', ' + c.CustomerDistrict + ', ' + c.CustomerProvinceCity
, c.CustomerContactMobileNumber, s.[Status],pd.PackageName,ac.Bandwidth,ac.StartDate,ac.EndDate,ac.ConnectionID,cg.CustomerGroup";
                html.OrderBy = "c.CustomerCode";
            }
            else if (Show == "Terminated")
            {
                sql = @"select row_number() over(order by c.CustomerCode) as [No],cg.CustomerGroup, ct.CustomerType + ' - ' + ct.DistrictCode [Block], c.CustomerID,
'<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" target=""_blank"">' + c.CustomerCode + '</a>' CustomerCode, 
'<a href=""#!/CustomerProfile?ID='+convert(varchar, c.CustomerID)+'"" target=""_blank"">' + c.CustomerName + '</a>' CustomerName, c.CustomerAddress + ', ' + c.CustomerCommune + ', ' + c.CustomerDistrict + ', ' + c.CustomerProvinceCity [Address]
, c.CustomerContactMobileNumber Phone, 
pd.PackageName + ' - ' + convert(varchar, ac.Bandwidth) + ' Mpbs'
+ '<br>' +
format(ac.StartDate, 'dd-MM-yy') +' - '+
format(ac.EndDate, 'dd-MM-yy') + '<br>' + convert(varchar, DATEDIFF(d, ac.StartDate, ac.EndDate)) + ' Days, ' + 
convert(varchar, DATEDIFF(m, ac.StartDate, ac.EndDate)) + ' Months <br><span class=""text-red""><b>' +
case when DATEDIFF(d, getdate(), ac.EndDate) > 0 then case when DATEDIFF(d, getdate(), ac.StartDate) <= 0 then 
'used ' +
convert(varchar, case when DATEDIFF(day, ac.StartDate, GETDATE()) > 0 then 
convert(varchar, DATEDIFF(day, ac.StartDate, GETDATE())) + ' days, ' +
convert(varchar, DATEDIFF(d, getdate(), ac.EndDate)) + ' day left '
else '0' end) 
else 'Expired' end else 'Expired' end + '</b></span>' Package,
ac.ConnectionID, case when s.[Status] = 'Actived' then '<span class=""badge bg-primary"">' + s.[Status] + '</span>' else '<span class=""badge bg-danger"">' + s.[Status] + '</span>' end [Status]
from dbo.TblCustomer c
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID
inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
where c.CustomerTypeID = " + CustomerTypeID + " and cg.CustomerGroup = N'" + CustomerGroup + "' and ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + @"%'
and ac.ConnectivityStatus = 42 and format(ac.LockedDate, 'yyyyMMdd') between '" + FromDate + "' and '" + ToDate + @"'
group by ct.CustomerType + ' - ' + ct.DistrictCode, c.CustomerID,
c.CustomerCode, c.CustomerName, c.CustomerAddress + ', ' + c.CustomerCommune + ', ' + c.CustomerDistrict + ', ' + c.CustomerProvinceCity
, c.CustomerContactMobileNumber, s.[Status],pd.PackageName,ac.Bandwidth,ac.StartDate,ac.EndDate,ac.ConnectionID,cg.CustomerGroup";
                html.OrderBy = "c.CustomerCode";
            }

            if (Show == "Connection" || Show == "NewConnection" || Show == "Renew" || Show == "Locked" || Show == "Terminated")
            {
                html.GH.GridTable = new GridField[]
                {
                new GridField() { Header = "No", FieldName = "No", TextAlign = "right" },
                    new GridField() { Header = "Group", FieldName = "CustomerGroup"},
                new GridField() { Header = "Block", FieldName = "Block"},
                new GridField() { Header = "CID", FieldName = "CustomerCode", },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Phone", FieldName = "Phone", TextAlign="center" },
                    new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Package", FieldName = "Package" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign = "center" },
                };
            }
            html.GH.SQL = sql;
            html.TblID = "tblcustblock";
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            //html.KendoGridAutoFitColumn = false;
            html.KendoGridStickyColumn = false;
            return html.DrawKendoGrid();
        }

        public string ShowCustBlockWithConnection()
        {
            HTML html = new HTML();
            string sql = @"-- Start Customer Block with Connection

declare @FromDate datetime, @ToDate datetime;

SET @FromDate  = DATEFROMPARTS ( DATEPART(yyyy, GETDATE()), 1, 1 );
SET @ToDate    = DATEADD(yy, DATEDIFF(yy, 0, GETDATE()) + 1, -1);

WITH CTE_Months
AS
(
	SELECT @FromDate StartDate
	UNION ALL
	SELECT DATEADD(MONTH,1,StartDate)
	FROM CTE_Months
	WHERE DATEADD(MONTH,1,StartDate) < @ToDate
)
select cg.CustomerGroup
,(select count(distinct ci.CustomerID) from dbo.TblCustomerInvoice ci 
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
where c.CustomerGroupID = cg.CustomerGroupID and ci.StatusID not in (12,13,14,20)
and case when @DomainID = '001,002' and c.NewRegisterDate is not null then format(c.NewRegisterDate, 'yyyyMM') else format(c.RegisterDate, 'yyyyMM') end
= format(A.StartDate,'yyyyMM')
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID and ci.DomainID like '%'+@DomainID+'%'
) NewCustomer,
(select count(distinct ci.ConnectionID) from dbo.TblActivateCustomer ci 
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
where c.CustomerGroupID = cg.CustomerGroupID
and format(ci.ActivateDate, 'yyyyMM') = format(A.StartDate,'yyyyMM')
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID and ci.DomainID like '%'+@DomainID+'%'
) NewConnection,
(select count(distinct ci.ConnectionID) from dbo.TblActivateCustomer ci 
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
where c.CustomerGroupID = cg.CustomerGroupID
and format(ci.RenewDate, 'yyyyMM') = format(A.StartDate,'yyyyMM')
and ci.ConnectivityStatus = 40
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID and ci.DomainID like '%'+@DomainID+'%'
) Renew,
(select count(distinct ci.ConnectionID) from dbo.TblActivateCustomer ci 
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
where c.CustomerGroupID = cg.CustomerGroupID
and format(ci.LockedDate, 'yyyyMM') = format(A.StartDate,'yyyyMM')
and ci.ConnectivityStatus = 41
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID and ci.DomainID like '%'+@DomainID+'%'
) Lock,
(select count(distinct ci.ConnectionID) from dbo.TblActivateCustomer ci 
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID
where c.CustomerGroupID = cg.CustomerGroupID
and format(ci.LockedDate, 'yyyyMM') = format(A.StartDate,'yyyyMM')
and ci.ConnectivityStatus = 42
and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID and ci.DomainID like '%'+@DomainID+'%'
) Terminate
from dbo.TblCustomerInvoice cb
inner join dbo.TblCustomer cc on cc.CustomerID = cb.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = cc.CustomerGroupID
inner join CTE_Months A on format(A.StartDate,'yyyy') = case when @DomainID = '001,002' and cc.NewRegisterDate is not null then format(cc.NewRegisterDate, 'yyyy') else format(cc.RegisterDate,'yyyy') end
where cb.StatusID not in (12,13,14,20) and cb.[Type] = 'Official Invoice'
and cb.SystemID = @SystemID and cb.WarehouseID = @WarehouseID and cb.DomainID like '%'+@DomainID+'%'
group by cg.CustomerGroup, cg.CustomerGroupID,format(A.StartDate,'yyyyMM')
order by format(A.StartDate,'yyyyMM'), cg.CustomerGroup
-- End Customer Block with Connection";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID },
            };
            return html.JsonData(sql, false, para, true);
        }
        [Authorize(Roles = "List Report Customer By Block")]
        public ActionResult ReportCustomerByBlock()
        {
            HTML html = new HTML
            {
                ReportTitle = "Report Customer By Block"
            };
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "InvoiceDate",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID where cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Invoice No",
                SQL = "select distinct top 10 cb.InvoiceNo\r\nfrom dbo.TblCustomer c\r\ninner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID\r\ninner join dbo.TblLeads l on l.LeadID = cb.EmployeeID\r\ninner join dbo.TblClass cc on cc.ClassID = cb.ClassNameID where cb.StatusID not in(12,13,14,20) and lower(cb.InvoiceNo) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%'",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("InvoiceNo") + "," + EncryAndDecry.Encryp("CustomerCode"),
                DefaultCheckBok = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("InvoiceNo") + "," + EncryAndDecry.Encryp("CustomerCode"),
                ISAutoComplete = true,
            });


            string sql = @"SP_CUSTOMER_PACKAGE_BY_BLOCK";
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GH.GridProcedureName = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", TextAlign="center", FormatDateTime="dd-MM-yyyy" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", ClassName="text-center" },
                new GridField() { Header = "SID", FieldName = "SID", ClassName="text-center"},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", },
                new GridField() { Header = "Customer Block", FieldName = "Block", ClassName="text-center" },
                new GridField() { Header = "InvoiceNo", FieldName = "InvoiceNo" },
                new GridField() { Header = "Package Name", FieldName = "ItemDescription" },
                new GridField() { Header = "Amount", FieldName = "Amount", ClassName="text-right", Format="n0" },
                new GridField() { Header = "Contract", FieldName = "Contract" },
                new GridField() { Header = "StartDate", FieldName = "StartDate", FormatDateTime="dd-MM-yyyy",ClassName="text-center" },
                new GridField() { Header = "EndDate", FieldName = "EndDate", FormatDateTime="dd-MM-yyyy", ClassName="text-center" },
                new GridField() { Header = "Remark", FieldName = "Remark" },
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}