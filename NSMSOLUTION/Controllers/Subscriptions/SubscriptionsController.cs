using MVC;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Subscriptions
{
    public class SubscriptionsController : Controller
    {
        // GET: Subscriptions
        [Authorize(Roles = "Show Subscriptions")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Subscriptions"
            };
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            DataSet ds = new DataSet();//html.LoadData(sql, false, para);
                                       //string sql = @"select (s.ServiceName + ' - ' + p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + bi.UM) PackageName, pd.Price,
                                       //isnull((select count(distinct (_bi.[SID])) from dbo.TblCustomerBillingInvoice _bi 
                                       //where _bi.ConnectivityStatus = 40 and _bi.PackageDetailID = pd.PackageDetailID and _bi.UM = bi.UM and _bi.SystemID = @SystemID and _bi.WarehouseID = @WarehouseID and _bi.StatusID not in (12,13,20)), 0) Active,
                                       //isnull((select count(distinct (_bi.[SID])) from dbo.TblCustomerBillingInvoice _bi 
                                       //where _bi.ConnectivityStatus = 41 and _bi.PackageDetailID = pd.PackageDetailID and _bi.UM = bi.UM and _bi.SystemID = @SystemID and _bi.WarehouseID = @WarehouseID and _bi.StatusID not in (12,13,20)), 0) Lock,
                                       //isnull((select count(distinct (_bi.ConnectionID)) from dbo.TblCustomerBillingInvoice _bi 
                                       //where _bi.ConnectivityStatus = 43 and _bi.PackageDetailID = pd.PackageDetailID and _bi.UM = bi.UM and _bi.SystemID = @SystemID and _bi.WarehouseID = @WarehouseID and _bi.StatusID not in (12,13,20)), 0) Inactive
                                       //from dbo.TblCustomerBillingInvoice bi
                                       //inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
                                       //inner join dbo.TblPackages p on p.PackageID = bi.PackageID
                                       //inner join dbo.TblServices s on s.ServiceID = bi.ServiceID
                                       //where bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID and bi.StatusID not in(12,13,20)
                                       //group by (s.ServiceName + ' - ' + p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + bi.UM), pd.Price, bi.UM, pd.PackageDetailID";
                                       //ViewBag.Data = html.JsonConvertDataTable(ds.Tables[0]);
            string sql = @"select p.PackageName
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblPackages p on p.PackageID = bi.PackageID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
where bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID and bi.DomainID like '%'+@DomainID+'%'
group by p.PackageName
order by p.PackageName

select p.PackageName, convert(varchar, pd.Bandwidth) + ' Mbps' Bandwidth, pd.Bandwidth Band
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblPackages p on p.PackageID = bi.PackageID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
where bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID and bi.DomainID like '%'+@DomainID+'%'
group by p.PackageName, convert(varchar, pd.Bandwidth) + ' Mbps', pd.Bandwidth
order by p.PackageName, pd.Bandwidth

select p.PackageName, convert(varchar, pd.Bandwidth) + ' Mbps' Bandwidth, bi.UM, 
bi.FreeDuration, pd.Bandwidth Band, format(bi.UnitPrice, '#,#0.00') UnitPrice,
count(distinct bi.CustomerID) Customer,
(p.PackageName +' - ' + convert(varchar, pd.Bandwidth) + ' Mpbs - ' + bi.UM) PName
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblPackages p on p.PackageID = bi.PackageID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
where bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID and bi.DomainID like '%'+@DomainID+'%'
group by p.PackageName, convert(varchar, pd.Bandwidth) + ' Mbps', bi.UM, 
bi.FreeDuration, pd.Bandwidth, format(bi.UnitPrice, '#,#0.00'),
(p.PackageName +' - ' + convert(varchar, pd.Bandwidth) + ' Mpbs - ' + bi.UM)
order by (p.PackageName +' - ' + convert(varchar, pd.Bandwidth) + ' Mpbs - ' + bi.UM)";
            ViewBag.PackageInfo = html.JsonData(sql, false, para, true);
            return View();
        }

        [Authorize(Roles = "Show Subscriptions")]
        public ActionResult SubscriptionsList()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Subscriptions"
            };
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                ComboBox = true,
                Label = "Connectivity Status:",
                DBField = "bi.ConnectivityStatus",
                Value = "ConnectivityStatus",
                DisplayText = "Status",
                SQL_CBO = "select distinct bi.ConnectivityStatus, s.[Status] from dbo.TblCustomerBillingInvoice bi inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus where bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'"
            });

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                DBField = "CustomerCode",
                SQL = "select distinct top 10 c.CustomerCode from dbo.TblCustomerBillingInvoice bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where lower(c.CustomerCode) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%' order by c.CustomerCode",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName") + "," + EncryAndDecry.Encryp("bi.SID"),
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                DBField = "CustomerName",
                SQL = "select distinct top 10 c.CustomerName from dbo.TblCustomerBillingInvoice bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where lower(c.CustomerName) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%' order by c.CustomerName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName") + "," + EncryAndDecry.Encryp("bi.SID"),
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "SID",
                DBField = "SID",
                SQL = "select distinct top 10 bi.[SID] from dbo.TblCustomerBillingInvoice bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where lower(bi.[SID]) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%' order by bi.[SID]",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName") + "," + EncryAndDecry.Encryp("bi.SID"),
            });
            html.isShowLoadingGrid = false;
            html.GH.SQL = @"select ROW_NUMBER() OVER(ORDER BY c.CustomerCode desc) as [No], c.CustomerID, c.CustomerCode, c.CustomerName, format(c.RegisterDate,'dd-MM-yyyy') as DateSubscribed,
            format(bi.NextInvoiceDate,'dd-MM-yyyy') as NextRenewalDate, case when bi.ConnectivityStatus = 43 then '<span class=""badge bg-danger"">' + s.[Status] + '</span>' when bi.ConnectivityStatus = 40 then '<span class=""badge bg-primary"">' + s.[Status] + '</span>' else '<span class=""badge bg-warning"">' + s.[Status] + '</span>' end [Status], bi.UM [Type], isnull(bi.[SID], '<span class=""text-red"">Pending</span>') [SID], bi.ConnectionID
            ,bi.FreeDuration from dbo.TblCustomerBillingInvoice bi
            inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
            inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
            where bi.DomainID like '%" + html.DomainID + "%' and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.StatusID not in(12,13,20) " +
            "group by c.CustomerID, c.CustomerCode, c.CustomerName, c.RegisterDate," +
            "bi.NextInvoiceDate, case when bi.ConnectivityStatus = 43 then '<span class=\"badge bg-danger\">' + s.[Status] + '</span>' when bi.ConnectivityStatus = 40 then '<span class=\"badge bg-primary\">' + s.[Status] + '</span>' else '<span class=\"badge bg-warning\">' + s.[Status] + '</span>' end, bi.UM, bi.[SID], bi.ConnectivityStatus, bi.ConnectionID,bi.FreeDuration";
            html.OrderBy = "c.CustomerCode desc";
            html.IsUseHavingCondition = true;
            //html.IsHaveWhereCondition = true;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", NewPage = true, LinkParameterField="CustomerID" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "ConnectionID", FieldName = "ConnectionID" },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "Date Subscribed", FieldName = "DateSubscribed",  ClassName="text-info" },
                new GridField() { Header = "Next Renewal Date", FieldName = "NextRenewalDate",  ClassName="text-red" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center" },
                new GridField() { Header = "Type", FieldName = "Type" },
                new GridField() { Header = "Free Duration", FieldName = "FreeDuration", TextAlign="right" }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}