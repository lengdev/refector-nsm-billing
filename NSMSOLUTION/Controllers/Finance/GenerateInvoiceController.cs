using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NewInventorySystem.Controllers
{
    //[LoggedOrAuthorizedAttribute(Roles = "Generate Invoice")]
    public class GenerateInvoiceController : Controller
    {
        // GET: GenerateInvoice
        [Authorize(Roles = "Generate Invoice")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Customers Invoice"
            };
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            HtmlAddControls htmlAddControl = new HtmlAddControls()
            {
                ID = "txtCustomerName",
                Label = "Customer Name:",
                DBField = "CustomerName",
                SQLAutoComplete = "select top 10 CustomerName from dbo.TblCustomer where lower((CustomerCode + ' - ' + CustomerName)) Like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                ISAutoComplete = true
            };
            htmlAddControls.Add(htmlAddControl);
            htmlAddControl = new HtmlAddControls()
            {
                ID = "txtPhoneNumber",
                Label = "Phone Number:",
                DBField = "CompanyContactMobileNumber",
                SQLAutoComplete = "select distinct top 10 CompanyContactMobileNumber from dbo.TblCustomerCompany where lower(CompanyContactMobileNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CompanyContactMobileNumber",
                ISAutoComplete = true
            };
            htmlAddControls.Add(htmlAddControl);
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," + EncryAndDecry.Encryp("c.CustomerName") + "," + EncryAndDecry.Encryp("cc.CompanyContactMobileNumber"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            //html.ShowPaging = false;
            //html.ShowGridOnly = true;
            //html.ShowSearchButton = false;
            html.TblID = "TblCustInfo";
            string sql = "select ROW_NUMBER() OVER(ORDER BY bi.BillingInvoiceID desc) AS[No], ";
            sql += "'<a href=\"javascript:void(0);\" onclick=\"OpenNewTap(''#!/CustomerProfile?ID='+convert(varchar, bi.CustomerID)+''')\">' + (c.CustomerCode + ' - ' + c.CustomerName) + '</a>' CustomerName, ";
            sql += "s.ServiceName, pt.PackageTypeName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, bi.UM) PackageName, ";
            sql += "bi.Amount, format(bi.InvoiceDate, 'dd-MM-yy') InvoiceDate, format(bi.StartDate, 'dd-MM-yy') StartDate, format(bi.EndDate, 'dd-MM-yy') EndDate, format(bi.NextInvoiceDate, 'dd-MM-yy') NextInvoiceDate, bi.InvoiceType, case when isnull(IsGenerated,0) = 0 then 'Not yet Generate' else 'Generated' end 'Generated', ";
            sql += "bi.GeneratedBy, '<a href=\"javascript:void(0);\" onclick=\"PrintInvoice('+convert(varchar, bi.CustomerID) +', ''' + bi.InvoiceNo +''','+ convert(varchar, bi.IsIncludedVAT) +','+ convert(varchar, bi.StatusID) +','''',false);\">' + bi.InvoiceNo + '</a>' InvoiceNo ";
            sql += ",(case when format(bi.EndDate, 'yyMMdd') < format(getdate(), 'yyMMdd') then 'Expired' else 'Remain: ' + convert(varchar, DATEDIFF(d, getdate(), bi.EndDate)) + ' days' end) InvoiceStatus ";
            sql += ",'<button type=\"button\" id=\"Renew\" onclick=\"Renew('''+bi.InvoiceNo+''','''+convert(varchar, bi.NextInvoiceDate, 111)+''','+convert(varchar, bi.CustomerID)+');\" class=\"btn btn-primary btn-sm\"> Renew</button>'";
            sql += "+ ' <button type=\"button\" id=\"NewInvoice\" onclick=\"CreateCustNewInvoice('+convert(varchar, bi.CustomerID)+','''');\" class=\"btn btn-info btn-sm\"> New Invoice</button>' ";
            sql += " [Action] ";
            sql += "from dbo.TblCustomerBillingInvoice bi ";
            sql += "inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID ";
            sql += "inner join dbo.TblServices s on s.ServiceID = bi.ServiceID ";
            sql += "inner join dbo.TblPackages p on p.PackageID = bi.PackageID ";
            sql += "inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID ";
            sql += "inner join dbo.TblPackageType pt on pt.PackageTypeID = p.PackageTypeID ";
            sql += "inner join dbo.TblCustomerCompany cc on cc.CustomerID = bi.CustomerID ";
            sql += "where bi.IsRenew = 0 and bi.StatusID not in(12, 13) and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%' "; //and bi.StatusID <> 12
            html.OrderBy = "bi.BillingInvoiceID desc";
            html.IsHaveWhereCondition = true;
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerName", FieldName = "CustomerName" },
                //new GridField() { Header = "ServiceName", FieldName = "ServiceName" },
                //new GridField() { Header = "ConnectionType", FieldName = "PackageTypeName" },
                new GridField() { Header = "PackageName", FieldName = "PackageName", IsGroupByField = true, Colspan = 8 },
                new GridField() { Header = "InvoiceNo", FieldName = "InvoiceNo", TextAlign="center" },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign = "right" },
                new GridField() { Header = "InvoiceDate", FieldName = "InvoiceDate", IsGroupByField = true, Colspan = 8, GroupLabel="Inv.Date: " },
                new GridField() { Header = "StartDate", FieldName = "StartDate", IsGroupByField = true, Colspan = 8, GroupLabel="Start Date: " },
                new GridField() { Header = "EndDate", FieldName = "EndDate", IsGroupByField = true, Colspan = 8, GroupLabel="End Date: " },
                new GridField() { Header = "NextInvoiceDate", FieldName = "NextInvoiceDate", TextAlign="center" },
                new GridField() { Header = "Status", FieldName = "InvoiceStatus", ClassName="text-danger" },
                //new GridField() { Header = "InvoiceType", FieldName = "InvoiceType" },
                new GridField() { Header = "Action", FieldName = "Action", TextAlign="center", HideOnPrint = true },
            };
            html.DefaultSearch = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();

            html = new HTML
            {
                DefaultSearch = false,
                TblWidth = ""
            };
            html.HTMLControls.Add(new HtmlDateTimePicker()
            {
                Label = "Invoice Date:",
                ID = "TxtInvoiceDate",
                Hide = true
            });
            html.HTMLControls.Add(new HtmlDateTimePicker()
            {
                Label = "Generate Date:",
                ID = "TxtGenerateDate",
            });
            ViewBag.AutoInvoice = html.DrawHTMLForm();
            return View();
        }
        [Authorize(Roles = "Generate Invoice")]
        public JsonResult ShowInvoiceWillGenerate(string GenerateInvoiceDate)
        {
            DateTime dt = new DateTime();
            dt = DateTime.Parse(GenerateInvoiceDate);
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false,
                ShowGridOnly = true,
                isShowLoadingGrid = false,
                ShowSearchButton = false,
                //html.JqueryDataTable = true;
                TblID = "TblShowInvWillGenerate"
            };
            string sql = "select ROW_NUMBER() OVER(ORDER BY bi.BillingInvoiceID desc) AS[No], ";
            sql += "'<a href=\"javascript:void(0);\" onclick=\"OpenNewTap(''#!/CustomerProfile?ID='+convert(varchar, bi.CustomerID)+''')\">' + (c.CustomerCode + ' - ' + c.CustomerName) + '</a>' CustomerName, ";
            //sql += "(c.CustomerCode + ' - ' + c.CustomerName) CustomerName, ";
            sql += "s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, bi.UM) PackageName, ";
            sql += "format(bi.InvoiceDate, 'dd-MM-yy') InvoiceDate, format(bi.StartDate, 'dd-MM-yy') StartDate, format(bi.EndDate, 'dd-MM-yy') EndDate, format(bi.NextInvoiceDate, 'dd-MM-yy') NextInvoiceDate, bi.Amount, ";
            sql += "(case when format(bi.EndDate, 'yyMMdd') < format(getdate(), 'yyMMdd') then 'Expired' else 'Remain: ' + convert(varchar, DATEDIFF(d, getdate(), bi.EndDate)) + ' days' end) InvoiceStatus, ";
            sql += "bi.GeneratedBy, '<a href=\"javascript:void(0);\" onclick=\"PrintInvoice('+convert(varchar, bi.CustomerID) +', ''' + bi.InvoiceNo +''','+ convert(varchar, bi.IsIncludedVAT) +','+ convert(varchar, bi.StatusID) +','''',false);\">' + bi.InvoiceNo + '</a>' InvoiceNo ";
            sql += "from dbo.TblCustomerBillingInvoice bi ";
            sql += "inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID ";
            sql += "inner join dbo.TblPackages p on p.PackageID = bi.PackageID ";
            sql += "inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID ";
            sql += "inner join dbo.TblServices s on s.ServiceID = bi.ServiceID ";
            sql += "where bi.StatusID not in (12,13) and bi.IsNextInvoice = 1 and bi.IsRenew = 0 and isnull(bi.IsGenerated,0) <> 1 ";
            sql += "and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'";
            sql += " and format(bi.EndDate, 'yyyyMMdd') = '" + dt.ToString("yyyyMMdd") + "'";
            html.GH.SQL = sql;
            html.OrderBy = "bi.BillingInvoiceID desc";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerName", FieldName = "CustomerName" },
                //new GridField() { Header = "ServiceName", FieldName = "ServiceName" },
                //new GridField() { Header = "ConnectionType", FieldName = "PackageTypeName" },
                new GridField() { Header = "PackageName", FieldName = "PackageName", IsGroupByField = true, Colspan = 8 },
                new GridField() { Header = "InvoiceNo", FieldName = "InvoiceNo", TextAlign="center" },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign = "right" },
                new GridField() { Header = "InvoiceDate", FieldName = "InvoiceDate", IsGroupByField = true, Colspan = 8, GroupLabel="Inv.Date: " },
                new GridField() { Header = "StartDate", FieldName = "StartDate", IsGroupByField = true, Colspan = 8, GroupLabel="Start Date: " },
                new GridField() { Header = "EndDate", FieldName = "EndDate", IsGroupByField = true, Colspan = 8, GroupLabel="End Date: " },
                new GridField() { Header = "NextInvoiceDate", FieldName = "NextInvoiceDate", TextAlign="center" },
                new GridField() { Header = "Status", FieldName = "InvoiceStatus", ClassName="text-danger" },
                //new GridField() { Header = "InvoiceType", FieldName = "InvoiceType" },
                //new GridField() { Header = "Action", FieldName = "Action", TextAlign="center" },
            };

            return Json(new { data = html.DrawHTMLSearchForm(1, "LoadInvWillGenerate"), InvGenerated = ShowGeneratedInvoice(GenerateInvoiceDate) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Generate Invoice")]
        public string ShowGeneratedInvoice(string GenerateInvoiceDate)
        {
            DateTime dt = new DateTime();
            dt = DateTime.Parse(GenerateInvoiceDate);
            HTML html = new HTML
            {
                ShowPrintOnTopTable = false,
                ShowGridOnly = true,
                ShowSearchButton = false,
                TblID = "TblShowGeneratedInv"
            };
            //html.isShowLoadingGrid = false;
            //html.JqueryDataTable = true;
            string sql = "select ROW_NUMBER() OVER(ORDER BY bi.CustomerID, p.PackageName) AS[No], ";
            sql += "'<a href=\"javascript:void(0);\" onclick=\"OpenNewTap(''#!/CustomerProfile?ID='+convert(varchar, bi.CustomerID)+''')\">' + (c.CustomerCode + ' - ' + c.CustomerName) + '</a>' CustomerName, ";
            //sql += "(c.CustomerCode + ' - ' + c.CustomerName) CustomerName, ";
            sql += "s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, bi.UM) PackageName, ";
            sql += "format(bi.InvoiceDate, 'dd-MM-yy') InvoiceDate, format(bi.StartDate, 'dd-MM-yy') StartDate, format(bi.EndDate, 'dd-MM-yy') EndDate, format(bi.NextInvoiceDate, 'dd-MM-yy') NextInvoiceDate, bi.Amount, ";
            sql += "(case when format(bi.EndDate, 'yyMMdd') < format(getdate(), 'yyMMdd') then 'Expired' else 'Remain: ' + convert(varchar, DATEDIFF(d, getdate(), bi.EndDate)) + ' days' end) InvoiceStatus, ";
            sql += "bi.GeneratedBy, '<a href=\"javascript:void(0);\" onclick=\"PrintInvoice('+convert(varchar, bi.CustomerID) +', ''' + bi.InvoiceNo +''','+ convert(varchar, bi.IsIncludedVAT) +','+ convert(varchar, bi.StatusID) +','''',false);\">' + bi.InvoiceNo + '</a>' InvoiceNo ";
            sql += " from dbo.TblCustomerBillingInvoice bi ";
            sql += "inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID ";
            sql += "inner join dbo.TblPackages p on p.PackageID = bi.PackageID ";
            sql += "inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID ";
            sql += "inner join dbo.TblServices s on s.ServiceID = bi.ServiceID ";
            sql += "where bi.StatusID not in (12,13) and bi.IsNextInvoice = 1 and isnull(bi.IsGenerated,0) = 1 ";
            sql += "and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'";
            sql += " and format(bi.StartDate, 'yyyyMMdd') = '" + dt.ToString("yyyyMMdd") + "'";

            html.GH.SQL = sql;
            html.OrderBy = "bi.CustomerID, p.PackageName";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerName", FieldName = "CustomerName" },
                new GridField() { Header = "PackageName", FieldName = "PackageName", IsGroupByField = true, Colspan = 8 },
                new GridField() { Header = "InvoiceNo", FieldName = "InvoiceNo", TextAlign="center" },
                new GridField() { Header = "Amount", FieldName = "Amount", TextAlign = "right" },
                new GridField() { Header = "InvoiceDate", FieldName = "InvoiceDate", IsGroupByField = true, Colspan = 8, GroupLabel="Inv.Date: " },
                new GridField() { Header = "StartDate", FieldName = "StartDate", IsGroupByField = true, Colspan = 8, GroupLabel="Start Date: " },
                new GridField() { Header = "EndDate", FieldName = "EndDate", IsGroupByField = true, Colspan = 8, GroupLabel="End Date: " },
                new GridField() { Header = "NextInvoiceDate", FieldName = "NextInvoiceDate", TextAlign="center" },
                new GridField() { Header = "Status", FieldName = "InvoiceStatus", ClassName="text-danger" },
                new GridField() { Header = "GeneratedBy", FieldName = "GeneratedBy" },
            };
            return html.DrawHTMLSearchForm(1, "LoadInvGenerated");
        }

        [Authorize(Roles = "Generate Invoice")]
        public async Task<JsonResult> GenerateInvoice(object[] Data)
        {
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(Data[0].ToString());
            string GenerateInvoiceDate = DataInfo["GenerateInvoiceDate"];
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            var result = await html.GenerateInvoice(0, GenerateInvoiceDate, html.UserName, int.Parse(html.SystemID), int.Parse(html.WarehouseID), false, html.DomainID);
            sqlResult = result;
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlString http(SqlString url)
        {
            var wc = new WebClient();
            var html = wc.DownloadString(url.Value);
            return new SqlString(html);
        }

        public ActionResult DoGenerateInv()
        {
            return View();
        }
        //protected override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    string str = @"<script>
        //    $(function () {
        //        DoGenerateInvoice('GenerateInvoice/GenerateInvoices', function (respone) {
        //        }, 'true');
        //    });
        //    </script>
        //    ";
        //    sb.AppendLine(str);
        //    filterContext.HttpContext.Response.Write(sb.ToString());
        //}
        [HttpPost]
        public JsonResult GenerateInvoices()
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);

            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = "SP_GENERATE_INVOICE";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "InvoiceDate", easternTimeNow.ToString() },
                { "GenerateInvoiceDate", easternTimeNow.ToString() },
                { "GenerateBeforeDay", "7" }
            };
            sqlResult = html.SaveChangeData(sql, true, para, true, true);
            string DataList = "";
            string Number = "";
            StringBuilder sb = new StringBuilder();
            if (sqlResult.Data.Tables.Count > 0)
            {
                if (sqlResult.Data.Tables[0].Rows.Count > 0)
                {
                    DataList = JsonConvert.SerializeObject(sqlResult.Data.Tables);
                }
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = DataList, Number = Number }, JsonRequestBehavior.AllowGet);
        }

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }
    }
}