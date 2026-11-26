using Microsoft.AspNet.Identity;
using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class HomeController : Controller
    {
        //Home
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                HostName = "localhost:83"
            };
            ViewBag.TMP = "_LayoutPageUser";
            if (html.UserID != "")
            {
                string sql = @"select d.DomainName, ut.NamedTemplates
from dbo.TblDomain d 
inner join (select value, t.NamedTemplates from dbo.TblUserTemplate ut cross apply string_split(ut.ForDomainID,',')
inner join dbo.TblTemplate t on t.TemplateID = ut.TemplateID where ut.UserLoginID = " + html.UserID + @") as ut on ut.value = d.DomainID
where d.DomainName = '" + html.HostName + "'";
                DataSet ds = html.LoadData(sql);
                
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        ViewBag.TMP = ds.Tables[0].Rows[0]["NamedTemplates"].ToString();
                }
            }
            return View();
        }
        public ActionResult Dashboard()
        {
            return View();
        }
        public async Task<ActionResult> Test()
        {
            SqlResult sqlResult = new SqlResult();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData(text: "▶️ Start Installing", callbackData: "Test"),
                            },
                    });
            //await notificationTelegram.NotificationTelegramBotToTopic(
            //    "hello this is testing...", -1689604171, 2);//1903890245/7
            sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(
                "hello this is testing...", -1689604171, 2, inlineKeyboard);//1903890245/7
            sqlResult = await notificationTelegram.NotificationTelegramBotToTopic(
                "hello this is testing...", -1689604171, 2, null, 90, "hello");//1903890245/7
            return View();
        }
        public class UserClaim
        {
            //public string Type { get; set; }
            public string UserRole { get; set; }
        }
        public ActionResult GetUserRole()
        {
            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;
            List<UserClaim> userClaim = new List<UserClaim>();
            foreach (Claim str in claims)
            {
                userClaim.Add(new UserClaim { UserRole = str.Value });
            }
            //var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //string strclaims = serializer.Serialize(claims);
            var serializedIdentity = JsonConvert.SerializeObject(userClaim);
            return Json(new { responseText = serializedIdentity }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCustConnectivity()
        {
            HTML html = new HTML();
            string sql = @"SQ_GET_DASHBOARD_DATA";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            string data = html.JsonData(sql, true, para, true);
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetCustPayment()
        {
            string sql = @"select 
sum(InvoiceAmount) InvoiceAmount,
sum(PaidAmount) PaidAmount,
((sum(InvoiceAmount) - sum(PaidAmount))) UnpaidAmount,
sum(PartiallyPaid) PartiallyPaid,
sum(AdvancePaymentAmount) AdvancePaymentAmount,
sum(InvoiceCancelledAmount) InvoiceCancelledAmount,
sum(isnull(InvoiceCancelledAmount,0)) InvoiceCancelledAmount,
sum(isnull(DepositFee,0)) DepositFee,
count(distinct CustomerID) Customer
from dbo.TblCustomerAccount 
where [Type] = 'Official Invoice' and StatusID not in (12,13,14,20) and
SystemID = @SystemID and WarehouseID = @WarehouseID and DomainID like '%'+@DomainID+'%';";
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            string data = html.JsonData(sql, false, para);
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult ShowRecentInvoice()
        {
            HTML html = new HTML
            {
                GridEventClick = "onGridClickShowCustProfile(this)",
                ShowPrintOnTopTable = false
            };
            //html.DrawTotalAllHeader = true;
            //html.DrawTotalAllHeaderDataTableNumber = 2;
            string sql = @"select top 10 row_number() over (order by format(ca.InvoiceDate, 'yyyy/MM/dd') desc) as [No],
            c.CustomerID, c.CustomerCode, 
            c.CustomerName, 
            format(ca.InvoiceDate, 'yyyy/MM/dd') InvoiceDate, 
            format(ca.PaymentDueDate, 'yyyy/MM/dd') PaymentDueDate,
            case when ca.StatusID = 10 then '<span class=""badge bg-primary"">' + s.[Status] + '</span>' else
            '<span class=""badge bg-danger"">' + s.[Status] + '</span>'
            end [Status], format(sum(ca.GrandTotal), '#,#.00') Amount
            from TblCustomerBalance ca
            inner join dbo.TblCustomer c on c.CustomerID = ca.CustomerID
            inner join dbo.TblStatus s on s.StatusID = ca.StatusID
            where format(ca.InvoiceDate, 'yyyy/MM/dd') between '" + DateTime.Parse(html.GetCurrentDate()).AddDays(-1).ToString("yyyy/MM/dd") + "' and '" + DateTime.Parse(html.GetCurrentDate()).ToString("yyyy/MM/dd") + "' and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " " +
            "group by c.CustomerID, c.CustomerCode, c.CustomerName, format(ca.InvoiceDate, 'yyyy/MM/dd'), format(ca.PaymentDueDate, 'yyyy/MM/dd'), case when ca.StatusID = 10 then '<span class=\"badge bg-primary\">' + s.[Status] + '</span>' else\r\n'<span class=\"badge bg-danger\">' + s.[Status] + '</span>'\r\nend order by [No], format(ca.InvoiceDate, 'yyyy/MM/dd') desc";
            html.GH.SQL = sql;
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.isShowLoadingGrid = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerID", FieldName = "CustomerID", IsHiddenField= true },
                //new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Date", FieldName = "InvoiceDate", Format="dd-MM-yyyy", TextAlign="center" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", Format="dd-MM-yyyy", TextAlign="center" },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign="right" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center", ClassName="text-info" }

            };
            string GData = html.DrawHTMLSearchForm();
            return Json(new { Data = GData }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult ShowLastIncome()
        {
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false,
                TblID = "tblshowlastincome",
                BunSearch = "btnsearchlastincome"
            };
            //html.GridEventClick = "onGridClickShowCustProfile(this)";
            string sql = @"select  top 10 ROW_NUMBER() over(order by p.PaymentDate desc) as [No],
            p.CustomerID, c.CustomerCode, c.CustomerName, 
            p.PaymentDate, p.PaymentDescription,
            ct.AccountName, format(p.PaymentAmount, '#,#.00') Amount
            from dbo.TblPayment p
            inner join dbo.TblCustomer c on c.CustomerID = p.CustomerID
            inner join dbo.TblChartOfAccount ct on ct.ChartOfAccountID = p.ChartOfAccountID
            where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by [No]";
            html.GH.SQL = sql;
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.isShowLoadingGrid = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerID", FieldName = "CustomerID", IsHiddenField= true },
                //new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Date", FieldName = "PaymentDate", Format="dd-MM-yyyy hh:mm:ss tt", TextAlign="center" },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign="center", ClassName="text-primary" },
                new GridField() { Header = "Description", FieldName = "PaymentDescription" },
                new GridField() { Header = "Account Name", FieldName = "AccountName" },
            };
            string GData = html.DrawHTMLSearchForm(1, "loadlastincome");
            return Json(new { Data = GData }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LoadIncomeByCategory()
        {
            HTML html = new HTML();
            string sql = @"select pc.Category, sum(p.PaymentAmount) Amount, '4' OrderBy
            from dbo.TblPayment p
            inner join dbo.TblPaymentCategory pc on pc.PaymentCategoryID = p.Category
            where p.SystemID = @SystemID and p.WarehouseID = @WarehouseID and p.DomainID like '%'+@DomainID+'%'
            group by pc.Category;
            select 'Total All' Category, sum(p.PaymentAmount) Amount, '0' OrderBy
            from dbo.TblPayment p
            inner join dbo.TblPaymentCategory pc on pc.PaymentCategoryID = p.Category
            where p.SystemID = @SystemID and p.WarehouseID = @WarehouseID and p.DomainID like '%'+@DomainID+'%'
            order by OrderBy, Category";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            string Data = html.JsonData(sql, false, para, true);
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LoadCustLocation()
        {
            HTML html = new HTML();
            string sql = @"select co.CommuneKH, COUNT(distinct c.CustomerID) Customer, co.CommuneID from dbo.TblCustomer c
inner join dbo.TblCommune co on co.CommuneID = c.CommuneID
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
where ci.StatusID not in (12,13,14,20) and ci.[Type] = 'Official Invoice' and
c.SystemID = @SystemID and c.WarehouseID = @WarehouseID and c.DomainID like '%'+@DomainID+'%'
group by co.CommuneKH, co.CommuneID order by Customer desc;

select co.DistrictKH, COUNT(distinct c.CustomerID) Customer, co.DistrictID from dbo.TblCustomer c
inner join dbo.TblDistrict co on co.DistrictID = c.DistrictID
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
where ci.StatusID not in (12,13,14,20) and ci.[Type] = 'Official Invoice' and
c.SystemID = @SystemID and c.WarehouseID = @WarehouseID and c.DomainID like '%'+@DomainID+'%'
group by co.DistrictKH, co.DistrictID order by Customer desc;

select co.ProvinceCityKH, COUNT(distinct c.CustomerID) Customer, co.ProvinceCityID from dbo.TblCustomer c
inner join dbo.TblProvinceCity co on co.ProvinceCityID = c.ProvinceCityID
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
where ci.StatusID not in (12,13,14,20) and ci.[Type] = 'Official Invoice' and
c.SystemID = @SystemID and c.WarehouseID = @WarehouseID and c.DomainID like '%'+@DomainID+'%'
group by co.ProvinceCityKH, co.ProvinceCityID order by Customer desc;";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            string Data = html.JsonData(sql, false, para, true);
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LoadCustConnection()
        {
            HTML html = new HTML();
            string sql = @"select count(ac.ConnectionType) Connection
            from dbo.TblActivateCustomer ac
            where ac.SystemID = @SystemID and ac.WarehouseID = @WarehouseID and ac.DomainID like '%'+@DomainID+'%';
            select ac.ConnectionType, count(ac.ConnectionType) Connection
            from dbo.TblActivateCustomer ac
            where ac.SystemID = @SystemID and ac.WarehouseID = @WarehouseID and ac.DomainID like '%'+@DomainID+'%'
            group by ac.ConnectionType;

            select count(distinct bi.CustomerID) Customer, 
(p.PackageName +' - ' + convert(varchar, pd.Bandwidth) + ' Mpbs - ' + bi.UM) PackageName
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
inner join dbo.TblPackages p on p.PackageID = bi.PackageID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
where bi.SystemID = @SystemID and bi.WarehouseID = @WarehouseID and bi.DomainID like '%'+@DomainID+'%'
group by (p.PackageName +' - ' + convert(varchar, pd.Bandwidth) + ' Mpbs - ' + bi.UM)
order by (p.PackageName +' - ' + convert(varchar, pd.Bandwidth) + ' Mpbs - ' + bi.UM);

            select count(distinct CustomerID) Customer
            from TblCustomerBillingInvoice
            where SystemID = @SystemID and WarehouseID = @WarehouseID and DomainID like '%'+@DomainID+'%'
            and format(EndDate, 'yyMMdd') >= format(getdate(), 'yyMMdd')
            ";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            string Data = html.JsonData(sql, false, para, true);
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult LoadCustConnectionType()
        {
            HTML html = new HTML();
            string sql = @"select top 10 ROW_NUMBER() over(order by ac.ActivateCustomerID desc) as [No], ac.CustomerID, c.CustomerCode, c.CustomerName, ac.ConnectionType, 
            ac.ActivateDate, ac.EndDate,
            convert(varchar, DATEDIFF(d, getdate(), ac.EndDate)) RemainDay
            from dbo.TblActivateCustomer ac
            inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
            where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " order by [No]";
            html.TblID = "tblshowcustcontype";
            html.GH.SQL = sql;
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.isShowLoadingGrid = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerID", FieldName = "CustomerID", IsHiddenField= true },
                //new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Connection Type", FieldName = "ConnectionType" },
                new GridField() { Header = "Activate Date", FieldName = "ActivateDate", Format="dd-MM-yy", TextAlign="center" },
                new GridField() { Header = "End Date", FieldName = "EndDate", Format="dd-MM-yy", TextAlign="center" },
                new GridField() { Header = "Remain Day", FieldName = "RemainDay", TextAlign="right", ClassName="text-red" }
            };
            string GData = html.DrawHTMLSearchForm(1, "loadcustcontype");
            return Json(new { Data = GData }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetPPPeEUserOnline(bool ShowDetail)
        {
            //Mikrotik mikrotik = new Mikrotik();
            //return Json(new { Data = mikrotik.GetPPPeEUserOnline(ShowDetail) }, JsonRequestBehavior.AllowGet);
            return Json(new { Data = "" }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult RemovePPPeEUserSession(int RouterID, string ID)
        {
            Mikrotik mikrotik = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            sqlResult = mikrotik.RemovePPPeEUserSession(RouterID, ID);
            return Json(new { Message = sqlResult.Message, Result = sqlResult.Result }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Test2()
        {
            var a = HttpContext.Request.GetOwinContext().Authentication.User.Identity.GetUserId();
            HTML html = new HTML();
            return View();
        }


        [Authorize]
        public string LoadDashboardInfo(int? year)
        {
            HTML html = new HTML();
            int selectedYear = year ?? DateTime.Now.Year;

            string sql = @"SP_DASHBOARD_INFO";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID },
                { "SelectedYear", selectedYear.ToString() },
            };
            return html.JsonData(sql, true, para, true);
        }
    }
}