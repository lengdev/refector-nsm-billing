using CsvHelper;
using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Utilities;

namespace NewInventorySystem.Controllers.API
{
    //[Authorize]
    public class CustInfoController : ApiController
    {
        // GET api/<controller>

        public string GET(string id)
        {
            HTML html = new HTML();
            string SQL = @"select c.CustomerID, case when c.CustomerCode is null then c.CustomerName else (c.CustomerName + ' [' + c.CustomerCode + ']') end + ' (' + c.CustomerGender + ')' FullCustomerName,
            c.CustomerAddress + case when c.CustomerCommune <> '' then + ', ' + c.CustomerCommune else '' end + case when c.CustomerDistrict <> '' then + ', ' + c.CustomerDistrict else '' end + case when c.CustomerProvinceCity <> '' then + ', ' + c.CustomerProvinceCity else '' end [Address],
            c.CustomerContactMobileNumber PhoneNumber, c.CustomerContactEmail, c.CustomerIdentification, convert(varchar, c.RegisterDate, 103) RegisterDate,
            c.CustomerNationality, c.CustomerIdentification, (e.FullName) EmployeeName, ct.CustomerType, c.CustomerTypeID, c.CustomerName,
            i.IndustryType, c.CustomerID, c.CustomerAddress, c.CustomerCommune, c.CustomerDistrict, c.CustomerProvinceCity, c.CustomerMobile, c.CustomerEmail, c.CustomerIdentification, c.CustomerCode, c.CustomerGender, c.CustomerName Cust_Name
            from dbo.TblCustomer c
            inner join dbo.TblLeads e on e.LeadID = c.EmployeeID
            inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
            inner join dbo.TblIndustryType i on i.IndustryTypeID = c.IndustryTypeID
            where c.CustomerContactMobileNumber = @PhoneNumber;


            select ROW_NUMBER() OVER(ORDER BY b.CustomerBalanceID desc) AS[No], b.InvoiceNo, b.InvoiceDate,
            DATEDIFF(day, b.InvoiceDate, GETDATE())[Day], b.PaymentDueDate, b.SubTotal, b.VAT, b.Total, b.DiscountP, 
            b.DiscountUSD, b.GrandTotal, b.Paid, b.Unpaid, b.IsIncludedVAT, b.StatusID,
            format(b.PaidDate, 'dd-MM-yy HH:mm:ss tt') PaidDate, b.[Type]
            from dbo.TblCustomerBalance b
            inner join dbo.TblCustomer c on c.CustomerID = b.CustomerID
            where c.CustomerContactMobileNumber = @PhoneNumber;

            select ROW_NUMBER() OVER(ORDER BY c.CustomerID desc, bi.ConnectionID desc) AS [No], 
            format(c.RegisterDate, 'dd-MM-yyyy') RegisterDate,
            c.CustomerCode CustomerID,
            c.CustomerName, bf.CompanyContactMobileNumber ContactPhone, 
            id.IndustryType,
            (bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) [Address],
            convert(varchar, pd.Bandwidth) + ' Mbps' Bandwidth,
            pt.PackageTypeName [Connection Type],
            case when bi.invTmp = 1 then se.ServiceName + ' - ' + bi.PackageName
            else bi.PackageName end PackageName,
            format(bi.StartDate, 'dd-MM-yyyy') StartDate, 
            format(bi.EndDate, 'dd-MM-yyyy') EndDate,
            [Status] , bi.ConnectionID, bi.[SID]           
            from dbo.TblCustomer c
            inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
            inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = bi.CustomerBillInformationID
            inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
            inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
            inner join dbo.TblPackages p on p.PackageID = bi.PackageID
            inner join dbo.TblServices se on se.ServiceID = bi.ServiceID
            inner join dbo.TblIndustryType id on id.IndustryTypeID = bf.IndustryTypeID
            inner join dbo.TblPackageType pt on pt.PackageTypeID = p.PackageTypeID
            where c.CustomerContactMobileNumber = @PhoneNumber;";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "PhoneNumber", id }
            };
            return html.JsonData(SQL, false, Para, true);
        }

        public async Task<SqlResult> POST()
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() OVER(ORDER BY c.CustomerID desc, bi.ConnectionID desc) AS [No], 
            format(c.RegisterDate, 'MM/dd/yyyy') RegisterDate,
            c.CustomerCode CustomerID,
            c.CustomerName, bf.CompanyContactMobileNumber ContactPhone, 
            id.IndustryType,
            (bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) [Address],
            convert(varchar, pd.Bandwidth) + ' Mbps' Bandwidth,
            pt.PackageTypeName [Connection Type], p.PackageCode,
            case when bi.invTmp = 1 then se.ServiceName + ' - ' + bi.PackageName
            else bi.PackageName end PackageName,
            format(bi.StartDate, 'MM/dd/yyyy') StartDate, 
            format(bi.EndDate, 'MM/dd/yyyy') EndDate,
            [Status]            
            from dbo.TblCustomer c
            inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
            inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = bi.CustomerBillInformationID
            inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
            inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
            inner join dbo.TblPackages p on p.PackageID = bi.PackageID
            inner join dbo.TblServices se on se.ServiceID = bi.ServiceID
            inner join dbo.TblIndustryType id on id.IndustryTypeID = bf.IndustryTypeID
            inner join dbo.TblPackageType pt on pt.PackageTypeID = p.PackageTypeID
            where format(bi.StartDate, 'yyyyMMdd') between format(dateadd(mm, -1,dateadd(dd, +1, eomonth(getdate()))), 'yyyyMMdd') and format(eomonth(getdate()),'yyyyMMdd')";
            string csv = html.JsonToCsv(html.JsonData(sql), ",");

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(csv)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "export.csv" };
            string fname = DateTime.Parse(html.GetCurrentDate()).ToString("yyyyMM") + "_NSM_ISP_Cust_Info.csv";
            string file = HttpContext.Current.Server.MapPath(@"\Uploads\CSV_File_For_DMC\\");
            if (!Directory.Exists(file))
                Directory.CreateDirectory(file);
            System.IO.File.WriteAllText(file + fname, csv);
            SqlResult sqlResult = new SqlResult();
            sqlResult = await html.UploadFileToServer(file.ToString(), fname.ToString(), "/ISP_Customer_Info_List/");

            return sqlResult;
            //return html.JsonData(sql);
        }
        [Authorize]
        [HttpPost]
        [Route("api/CustInfo/Monthly")]
        public async Task<HttpResponseMessage> Monthly()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            string sql = @"select ROW_NUMBER() over(order by c.RegisterDate desc) as [No],
format(c.RegisterDate, 'dd-MMMM-yyyy') RegisterDate, c.CustomerCode CustomerID, ac.[SID] ServiceID, c.CustomerName, ct.IndustryType,
bf.InstallAddress+ ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity [Address],
convert(varchar, ac.Bandwidth) + ' Mbps' Bandwidth, pd.PackageName, ac.ConnectionType,
'Unlimited' DateUsage, s.[Status], case when ac.ConnectivityStatus <> 40 then format(ac.LockedDate,'dd-MMMM-yyyy') else '' end LockedDate
from dbo.TblCustomerBalance cb
inner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID
inner join dbo.TblIndustryType ct on ct.IndustryTypeID = c.IndustryTypeID
inner join dbo.TblCustomerBillInformation bf on bf.CustomerID = cb.CustomerID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID
inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
where cb.IsToDMCTAX = 1 and cb.SystemID = 13 and cb.WarehouseID = 16
group by c.RegisterDate, c.CustomerCode, ac.[SID], c.CustomerName, ct.IndustryType,
bf.InstallAddress, bf.InstallCommune, + bf.InstallDistrict, + bf.InstallProvinceCity,
ac.Bandwidth, pd.PackageName, ac.ConnectionType, s.[Status],ac.ConnectivityStatus,ac.LockedDate, c.CustomerID,
cb.InvoiceNo";
            string csv = html.JsonToCsv(html.JsonData(sql), ",");

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(csv, Encoding.Unicode)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "export.csv" };
            string fname = DateTime.Parse(html.GetCurrentDate()).ToString("yyyyMM") + "_NSM_ISP_Cust_Info.csv";
            string file = HttpContext.Current.Server.MapPath(@"\Uploads\CSV_File_For_DMC\\");
            if (!Directory.Exists(file))
                Directory.CreateDirectory(file);
            //File.WriteAllText(file + fname, csv);

            using (StreamWriter sw = new StreamWriter(new FileStream(file + fname, FileMode.Create), Encoding.UTF8))
            {
                sw.WriteLine(string.Join(",", csv));
            }

            sqlResult = await html.UploadFileToServer(file.ToString(), fname.ToString(), "/ISP_Customer_Info_List/");

            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
        public static DataTable jsonStringToTable(string jsonContent)
        {
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(jsonContent);
            return dt;
        }
        
        [HttpGet]
        [Route("api/CustInfo/GETDataForInsert")]
        public HttpResponseMessage GETDataForInsert()
        {
            HTML html = new HTML();
            string sql = "select LeadID EmployeeID, Salutation + ' ' + FullName EmployeeName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            var SaleName = html.JsonData(sql);
            sql = "select ClassID, ClassName from dbo.TblClass where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ClassName";
            var ClassName = html.JsonData(sql);
            sql = "select CustomerGroupID, CustomerGroup from dbo.TblCustomerGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            var CustomerGroup = html.JsonData(sql);
            sql = "select CustomerTypeID, CustomerType from dbo.TblCustomerType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerType";
            var Block = html.JsonData(sql);
            sql = @"select c.CommuneID, c.Commune, d.DistrictID, d.District, p.ProvinceCity, p.ProvinceCityID, 0 OrderBy
from dbo.TblCommune c
inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID
inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + @"
union
select c.CommuneID, c.CommuneKH Commune, d.DistrictID, d.DistrictKH District, p.ProvinceCityKH ProvinceCity, p.ProvinceCityID, 1 OrderBy
from dbo.TblCommune c
inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID
inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + @"
order by c.Commune, OrderBy";
            var Commune = html.JsonData(sql);
            sql = @"select NationalityKH CustomerNationality, OrderBy, 0 neworder from dbo.TblNationality
where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + @"
union all
select Nationality CustomerNationality, OrderBy, 1 neworder from dbo.TblNationality
where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + @"
order by OrderBy, neworder, CustomerNationality";
            var Nationality = html.JsonData(sql);

            //            sql = @"select '0' StockInID, '0' ItemID, OtherChargeItem ItemName, Fee SalePrice, UM, OtherChargeID,
            //'0' PackageDetailID, '0' PackageID, '0' ServiceID, '0' Duration, '0' FreeDuration, '' PackageName, '0' Bandwidth,
            //'0' DowSpeed, '0' UpSpeed, '0' DepositFee, '' Feature, '3' OrderBy, (ABS(CAST(CAST(NEWID() AS VARBINARY) AS INT)) % (100 - 70 + 1) + 70) PID,
            //0 LocationID,  '' WarehouseName, '' ServiceName, '' PackageCode
            //from dbo.TblOtherCharge
            //where [Status] = 'Enable' and SystemID = @SystemID and WarehouseID = @WarehouseID
            //union all
            //select distinct 0 StockInID, s.ItemID, i.ItemName + case when isnull(i.ItemCode,'') <> '' then ' - ' + i.ItemCode else case when isnull(i.SerialNumber,'') <> '' then ' - ' + i.SerialNumber else '' end end ItemName, i.SellingPrice SalePrice, i.UM,
            //'0' OtherChargeID, '0' PackageDetailID, '0' PackageID, '0' ServiceID, '0' Duration, '0' FreeDuration, '' PackageName,
            //'0' Bandwidth, '0' DowSpeed, '0' UpSpeed, '0' DepositFee, '' Feature, '2' OrderBy, (ABS(CAST(CAST(NEWID() AS VARBINARY) AS INT)) % (100 - 70 + 1) + 70) PID,
            //s.LocationID, uw.WarehouseName, '' ServiceName, '' PackageCode
            //from dbo.TblStockInDetail s
            //inner join dbo.TblItem i on i.ItemID = s.ItemID
            //inner join dbo.TblUserWarehouse uw on uw.WID = s.LocationID
            //where s.LocationID = case when @LocationID = 0 then LocationID else @LocationID end and i.SystemID = @SystemID and i.WarehouseID = @WarehouseID
            //group by s.ItemID, i.ItemName, i.SellingPrice, i.UM, i.SerialNumber, i.ItemCode,s.LocationID, uw.WarehouseName
            //having (sum(isnull(s.QTY,0))-sum(isnull(s.QTYOUT,0))) > 0
            //union all
            //select '0' StockInID, '0' ItemID, (trim(pd.ServiceName) + ' - ' + trim(pd.PackageName) + ' - ' +
            //convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM) ItemName,
            //pd.Price, (convert(varchar, pd.Duration) + ' ' + pd.UM) UM,
            //'0' OtherChargeID, isnull(pd.PackageDetailID, 0) PackageDetailID, 
            //isnull(pd.PackageID, 0) PackageID, isnull(pd.ServiceID, 0) ServiceID, 
            //isnull(pd.Duration, 0) Duration, isnull(pd.FreeDuration, 0) FreeDuration, 
            //isnull(pd.PackageName, 0) PackageName, isnull(pd.Bandwidth,0) Bandwidth, 
            //isnull(pd.DowSpeed,0) DowSpeed, isnull(pd.UpSpeed,0) UpSpeed, isnull(pd.DepositFee,0) DepositFee, pd.Feature, '1' OrderBy, (ABS(CAST(CAST(NEWID() AS VARBINARY) AS INT)) % (100 - 70 + 1) + 70) PID,
            //0 LocationID,  '' WarehouseName, pd.ServiceName, pd.PackageCode
            //from dbo.TblPackageDetail pd
            //inner join dbo.TblPackages p on p.PackageID = pd.PackageID
            //where pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID
            //union all
            //select 0 StockInID, i.ItemID, i.ItemName, i.SellingPrice, i.UM, 0 OtherChargeID, 0 PackageDetailID, 0 PackageID, 0 ServiceID, 0 Duration, 0 FreeDuration,
            //'' PackageName, 0 Bandwidth, 0 DowSpeed, 0 UpSeed, 0 DepositFee, '' Feature, 4 OrderBy, 0 PID, @LocationID LocationID, '' WarehouseName, '' ServiceName, '' PackageCode
            //from dbo.TblItem i where i.ItemID not in (select ItemID from dbo.TblStockInDetail) and i.SystemID = @SystemID and i.WarehouseID = @WarehouseID
            //and @isRegisterForm = 1
            //union all
            //select 0 StockInID, i.ItemID, (s.ServiceName + ' - ' + i.ItemName) ItemName, i.SellingPrice, i.UM, 0 OtherChargeID, 0 PackageDetailID, 0 PackageID, 0 ServiceID, 0 Duration,
            //0 FreeDuration, '' PackageName, 0 Bandwidth, 0 DowSpeed, 0 UpSpeed, 0 DepositFee, '' Feature, 5 OrderBy, 0 PID, @LocationID LocationID, '' WarehouseName, '' ServiceName, '' PackageCode
            //from dbo.TblItem i
            //inner join dbo.TblServices s on s.ServiceID = i.ItemTypeID
            //where s.ServiceName <> 'Inventory' and i.SystemID = @SystemID and i.WarehouseID = @WarehouseID
            //order by OrderBy, ItemName";
            sql = @"select (trim(pd.ServiceName) + ' - ' + trim(pd.PackageName) + ' - ' +
convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM) ItemName,
pd.Price, (convert(varchar, pd.Duration) + ' ' + pd.UM) UM,
'0' OtherChargeID, isnull(pd.PackageDetailID, 0) PackageDetailID, 
isnull(pd.PackageID, 0) PackageID, isnull(pd.ServiceID, 0) ServiceID, 
isnull(pd.Duration, 0) Duration, isnull(pd.FreeDuration, 0) FreeDuration, 
isnull(pd.PackageName, 0) PackageName, isnull(pd.Bandwidth,0) Bandwidth, 
isnull(pd.DowSpeed,0) DowSpeed, isnull(pd.UpSpeed,0) UpSpeed, isnull(pd.DepositFee,0) DepositFee, pd.Feature, '1' OrderBy, (ABS(CAST(CAST(NEWID() AS VARBINARY) AS INT)) % (100 - 70 + 1) + 70) PID,
0 LocationID,  '' WarehouseName, pd.ServiceName, pd.PackageCode
from dbo.TblPackageDetail pd
inner join dbo.TblPackages p on p.PackageID = pd.PackageID
where pd.SystemID = @SystemID and pd.WarehouseID = @WarehouseID";
            Dictionary<string, string> para = new Dictionary<string, string>{
                { "LocationID", "0" },
                { "SystemID", html.SystemID},
                {"WarehouseID", html.WarehouseID},
                {"isRegisterForm", "0" }
            };
            var ServicePlan = html.JsonData(sql, false, para);

            sql = @"select IndustryTypeID, IndustryType, SystemID, WarehouseID from dbo.TblIndustryType where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            var industryType = html.JsonData(sql, false, para);

            AddCustomerController addCustomerController = new AddCustomerController();
            string CID = addCustomerController.LoadCustTypePrefix("Village").Data.ToString();

            object[] objs = new object[] { };
            var objList = objs.ToList();
            objList.Add(new
            {
                SaleName = SaleName,
                ClassName = ClassName,
                CustomerGroup = CustomerGroup,
                Block = Block,
                industryType = industryType,
                Commune = Commune,
                Nationality = Nationality,
                ServicePlan = ServicePlan,
                //CID = CID
            });

            return Request.CreateResponse(HttpStatusCode.OK, objList);
        }
        [HttpPost]
        //[Consumes("multipart/form-data")]
        [Route("api/CustInfo/InsertCustomer")]

        public async Task<HttpResponseMessage> InsertCustomer(object CustomerInfo, string image = "")
        {
            HTML html = new HTML();
            if (!Request.Content.IsMimeMultipartContent())
            {
                Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported Media Type");
            }
            dynamic value = HttpContext.Current.Request.Form[0];
            //dynamic value = CustomerInfo;
            int iFile = 0;
            string storeFileName = "";
            SqlResult sqlResult = new SqlResult();
            try
            {
                //string abv = @"{""CustomerInfo"":{""Employee"":""Ms. Pov Sreyroth"",""EmployeeID"":37,""ClassName"":""NSM ISP"",""ClassNameID"":6,""CustomerGroup"":""FTTH"",""CustomerGroupID"":26,""CustomerType"":""KDLTK"",""CustomerType_CustomerTypeID"":22,""CustomerID"":""081110"",""CustomerName"":""Demo Customer Name"",""CustomerGender"":""Male"",""CustomerAddress"":""Demo Customer Address"",""CustomerCommune"":""Boeung Prolit"",""CustomerDistrict"":""Prampi Makara"",""CustomerProvinceCity"":""Phnom Penh"",""CustomerMobile"":""1234567890"",""CustomerNationality"":""China"",""CustomerIdentification"":""Demo Identification"",""ATTName"":""Demo Att Name"",""CustomerContactName"":""Demo Contact Name"",""CustomerContactMobileNumber"":"""",""CustomerContactEmail"":""Demo Contact Email"",""CompanyName"":""Demo Company Name"",""CustomerBillInformationID"":""Demo Customer Bill Information ID"",""CompanyNameKH"":""Demo Company Name KH"",""CompanyDeskPhone"":""Demo Desk Phone"",""CompanyMobile"":""9876543210"",""CompanyEmail"":""Demo Company Email"",""VATNumber"":""0"",""IndustryType"":""Internet Café"",""CompanyAddress"":""Demo Company Address"",""CompanyCommune"":""Demo Company Commune"",""CompanyDistrict"":""Demo Company District"",""CompanyProvinceCity"":""Demo Company Province/City"",""CompanyContactName"":""Demo Company Contact Name"",""CompanyContactPosition"":""Demo Company Contact Position"",""CompanyContactMobileNumber"":"""",""BillToAddress"":""Demo Bill To Address"",""BillToCommune"":""Demo Bill To Commune"",""BillToDistrict"":""Demo Bill To District"",""BillToProvinceCity"":""Demo Bill To Province/City"",""InstallAddress"":""Demo Install Address"",""InstallCommune"":""Demo Install Commune"",""InstallDistrict"":""Toul Kork"",""InstallProvinceCity"":""Phnom Penh"",""GoogleMap_FullAddress"":""Demo Google Map Full Address"",""InputLatitude"":""11.543611857802848"",""InputLongitude"":"" 104.89496114140054"",""AgencyName"":""Demo Agency Account Name"",""AgencyCommissionID"":""Demo Agency Commission ID"",""AgencyPhone"":""555-1234567"",""AgencyCommissionType"":""Demo Agency Commission Type"",""CommissionAmountUSD"":""100.50"",""CommissionAmountP"":""10"",""AgencyAddress"":""Demo Agency Address"",""AgencyBankName"":""Demo Agency Bank Name"",""AgencyAccountName"":""Demo Agency Account Name"",""AgencyAccountNumber"":""1234567890"",""IsIncludedVAT"":""true"",""RegisterBy"":""012321125"",""PaymentDueDate"":""2023-12-4"",""StartDate"":""2023-12-4"",""InvoiceDate"":""2023-12-4"",""EndDate"":""2023-12-4"",""UserName"":""Keo Samnang""},""CustomerInvoice"":[{""ServiceID"":1,""PackageID"":2,""PackageCode"":"""",""PackageDetailID"":10,""ServiceName"":""Internet Service - Business Package-UV - 15 Mbps - 6 Months"",""UM"":""6 Months"",""QTY"":1,""UnitPrice"":180.0,""DiscountP"":0,""DiscountUSD"":0.0,""Amount"":180.0,""Duration"":6,""FreeDuration"":1,""Bandwidth"":15,""DowSpeed"":15,""UpSpeed"":15,""PID"":97,""DepositFee"":50.0,""ServDepositFee"":50.0,""PaymentDueDate"":""2023-12-4"",""StartDate"":""2023-12-4"",""InvoiceDate"":""2023-12-4"",""EndDate"":""2023-12-4"",""IsIncludedVAT"":true,""LocationID"":0,""Warehouse"":""""}],""CustomerInvoiceDetail"":{""SubTotal"":180.0,""VAT"":0,""Total"":198.0,""DiscountP"":0.0,""DiscountUSD"":0.0,""GrandTotal"":198.0},""CustomerOtherCharge"":{""Item"":""Deposit Fee"",""DepositFee"":50}}";
                dynamic CustomerDataInfo = JsonConvert.DeserializeObject(value);// new JavaScriptSerializer().Serialize(value.ToJsonString());
                                                                                //dynamic CustomerDataInfo = value;
                dynamic custinfo = CustomerDataInfo["CustomerInfo"];
                dynamic custinv = CustomerDataInfo["CustomerInvoice"];
                dynamic custdetail = CustomerDataInfo["CustomerInvoiceDetail"];
                dynamic custot = CustomerDataInfo["CustomerOtherCharge"];
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<Data>");
                sb.AppendLine("<CustomerAttachmentFileName>");
                sb.AppendLine("<AttachmentFileName></AttachmentFileName>");
                sb.AppendLine("<DestinationFolder>/Uploads/CustomerAttFiles/</DestinationFolder>");
                sb.AppendLine("</CustomerAttachmentFileName>");
                //sb.AppendLine("<CustomerInfo>");
                //foreach (dynamic item in custinfo)
                //{
                //	sb.AppendLine("<" + item.Key + ">" + item.Value.ToString() + "</" + item.Key + ">");
                //}
                //sb.AppendLine("</CustomerInfo>");
                //sb.AppendLine("</Data>");
                //string str = sb.ToString();

                sb.AppendLine("<CustomerInfo>");
                sb.AppendLine("<SetCID>None VAT</SetCID>");
                sb.AppendLine("<Employee>" + custinfo["Employee"] + "</Employee>");
                sb.AppendLine("<EmployeeID>" + custinfo["EmployeeID"] + "</EmployeeID>");
                sb.AppendLine("<ReferenceNo></ReferenceNo>");
                sb.AppendLine("<ClassName>" + custinfo["ClassName"] + "</ClassName>");
                sb.AppendLine("<ClassNameID>" + custinfo["ClassNameID"] + "</ClassNameID>");
                sb.AppendLine("<CustomerGroup>" + custinfo["CustomerGroup"] + "</CustomerGroup>");
                sb.AppendLine("<CustomerGroupID>" + custinfo["CustomerGroupID"] + "</CustomerGroupID>");
                sb.AppendLine("<CustomerInvoiceNo></CustomerInvoiceNo>");
                sb.AppendLine("<CustomerType>" + custinfo["CustomerType"] + "</CustomerType>");
                sb.AppendLine("<CustomerType_CustomerTypeID>" + custinfo["CustomerType_CustomerTypeID"] + "</CustomerType_CustomerTypeID>");
                sb.AppendLine("<CustomerCode></CustomerCode>");
                sb.AppendLine("<CustomerID>0</CustomerID>");
                sb.AppendLine("<CustomerName>" + custinfo["CustomerName"] + "</CustomerName>");
                sb.AppendLine("<CustomerGender>" + custinfo["CustomerGender"] + "</CustomerGender>");
                sb.AppendLine("<CustomerAddress>" + custinfo["CustomerAddress"] + "</CustomerAddress>");
                sb.AppendLine("<CustomerCommune>" + custinfo["CustomerCommune"] + "</CustomerCommune>");
                sb.AppendLine("<CustomerDistrict>" + custinfo["CustomerDistrict"] + "</CustomerDistrict>");
                sb.AppendLine("<CustomerProvinceCity>" + custinfo["CustomerProvinceCity"] + "</CustomerProvinceCity>");
                sb.AppendLine("<CustomerMobile>" + custinfo["CustomerMobile"] + "</CustomerMobile>");
                sb.AppendLine("<CustomerEmail></CustomerEmail>");
                sb.AppendLine("<CustomerNationality>" + custinfo["CustomerNationality"] + "</CustomerNationality>");
                sb.AppendLine("<CustomerIdentification>" + custinfo["CustomerIdentification"] + "</CustomerIdentification>");
                sb.AppendLine("<ATTName>" + custinfo["ATTName"] + "</ATTName>");
                sb.AppendLine("<CustomerContactName>" + custinfo["CustomerContactName"] + "</CustomerContactName>");
                sb.AppendLine("<CustomerContactMobileNumber>" + custinfo["CustomerContactMobileNumber"] + "</CustomerContactMobileNumber>");
                sb.AppendLine("<CustomerContactEmail>" + custinfo["CustomerContactEmail"] + "</CustomerContactEmail>");
                sb.AppendLine("<CompanyName>" + custinfo["CompanyName"] + "</CompanyName>");
                sb.AppendLine("<CustomerBillInformationID>0</CustomerBillInformationID>");
                sb.AppendLine("<CompanyNameKH>" + custinfo["CompanyNameKH"] + "</CompanyNameKH>");
                sb.AppendLine("<CompanyDeskPhone>" + custinfo["CompanyDeskPhone"] + "</CompanyDeskPhone>");
                sb.AppendLine("<CompanyMobile>" + custinfo["CompanyMobile"] + "</CompanyMobile>");
                sb.AppendLine("<CompanyEmail>" + custinfo["CompanyEmail"] + "</CompanyEmail>");
                sb.AppendLine("<VATNumber>" + custinfo["VATNumber"] + "</VATNumber>");
                sb.AppendLine("<IndustryType>" + custinfo["IndustryType"] + "</IndustryType>");
                sb.AppendLine("<CompanyAddress>" + custinfo["CompanyAddress"] + "</CompanyAddress>");
                sb.AppendLine("<CompanyCommune>" + custinfo["CompanyCommune"] + "</CompanyCommune>");
                sb.AppendLine("<CompanyDistrict>" + custinfo["CompanyDistrict"] + "</CompanyDistrict>");
                sb.AppendLine("<CompanyProvinceCity>" + custinfo["CompanyProvinceCity"] + "</CompanyProvinceCity>");
                sb.AppendLine("<CompanyContactName>" + custinfo["CompanyContactName"] + "</CompanyContactName>");
                sb.AppendLine("<CompanyContactPosition>" + custinfo["CompanyContactPosition"] + "</CompanyContactPosition>");
                sb.AppendLine("<CompanyContactMobileNumber>" + custinfo["CompanyContactMobileNumber"] + "</CompanyContactMobileNumber>");
                sb.AppendLine("<BillToAddress>" + custinfo["BillToAddress"] + "</BillToAddress>");
                sb.AppendLine("<BillToCommune>" + custinfo["BillToCommune"] + "</BillToCommune>");
                sb.AppendLine("<BillToDistrict>" + custinfo["BillToDistrict"] + "</BillToDistrict>");
                sb.AppendLine("<BillToProvinceCity>" + custinfo["BillToProvinceCity"] + "</BillToProvinceCity>");
                sb.AppendLine("<InstallAddress>" + custinfo["InstallAddress"] + "</InstallAddress>");
                sb.AppendLine("<InstallCommune>" + custinfo["InstallCommune"] + "</InstallCommune>");
                sb.AppendLine("<InstallDistrict>" + custinfo["InstallDistrict"] + "</InstallDistrict>");
                sb.AppendLine("<InstallProvinceCity>" + custinfo["InstallProvinceCity"] + "</InstallProvinceCity>");
                sb.AppendLine("<GoogleMap_FullAddress>" + custinfo["GoogleMap_FullAddress"] + "</GoogleMap_FullAddress>");
                sb.AppendLine("<InputLatitude>" + custinfo["InputLatitude"] + "</InputLatitude>");
                sb.AppendLine("<InputLongitude>" + custinfo["InputLongitude"] + "</InputLongitude>");
                sb.AppendLine("<AgencyName>" + custinfo["AgencyName"] + "</AgencyName>");
                sb.AppendLine("<AgencyCommissionID>0</AgencyCommissionID>");
                sb.AppendLine("<AgencyPhone>" + custinfo["AgencyPhone"] + "</AgencyPhone>");
                sb.AppendLine("<CommissionAmountUSD>" + custinfo["CommissionAmountUSD"] + "</CommissionAmountUSD>");
                sb.AppendLine("<CommissionAmountP>" + custinfo["CommissionAmountP"] + "</CommissionAmountP>");
                sb.AppendLine("<AgencyAddress>" + custinfo["AgencyAddress"] + "</AgencyAddress>");
                sb.AppendLine("<AgencyBankName>" + custinfo["AgencyBankName"] + "</AgencyBankName>");
                sb.AppendLine("<AgencyAccountName>" + custinfo["AgencyAccountName"] + "</AgencyAccountName>");
                sb.AppendLine("<AgencyAccountNumber>" + custinfo["AgencyAccountNumber"] + "</AgencyAccountNumber>");
                sb.AppendLine("<PreviousISP></PreviousISP>");
                sb.AppendLine("<IsEditInvoice>False</IsEditInvoice>");
                sb.AppendLine("<CustIDToDMC>0</CustIDToDMC>");
                sb.AppendLine("<InvoiceToDMC></InvoiceToDMC>");
                sb.AppendLine("<ContactName></ContactName>");
                sb.AppendLine("<OldCustomerID>0</OldCustomerID>");
                sb.AppendLine("<CustomerTypeID>47</CustomerTypeID>");
                sb.AppendLine("<IsIncludedVAT>false</IsIncludedVAT>");
                sb.AppendLine("<RegisterBy>" + custinfo["RegisterBy"] + "</RegisterBy>");
                sb.AppendLine("<PaymentDueDate>" + custinfo["PaymentDueDate"] + "</PaymentDueDate>");
                sb.AppendLine("<StartDate>" + custinfo["StartDate"] + "</StartDate>");
                sb.AppendLine("<InvoiceDate>" + custinfo["InvoiceDate"] + "</InvoiceDate>");
                sb.AppendLine("<EndDate>" + custinfo["EndDate"] + "</EndDate>");
                sb.AppendLine("<NextInvoiceDate>" + custinfo["EndDate"] + "</NextInvoiceDate>");
                sb.AppendLine("<CompletedDate>" + custinfo["EndDate"] + "</CompletedDate>");
                sb.AppendLine("<HaveServiceIDMoreThenOne>1</HaveServiceIDMoreThenOne>");
                sb.AppendLine("<StatusID>1</StatusID>");
                sb.AppendLine("<GoogleMap_Latitude>" + custinfo["InputLatitude"] + "</GoogleMap_Latitude>");
                sb.AppendLine("<GoogleMap_Longitude>" + custinfo["InputLongitude"] + "</GoogleMap_Longitude>");
                sb.AppendLine("<InvoiceType>Created Invoice</InvoiceType>");
                sb.AppendLine("<OldInvoiceNo></OldInvoiceNo>");
                sb.AppendLine("<BillingInvoiceID>0</BillingInvoiceID>");
                sb.AppendLine("<OldBillingInvoiceID>0</OldBillingInvoiceID>");
                sb.AppendLine("<ConnectivityStatus>43</ConnectivityStatus>");
                sb.AppendLine("<PPPoEUserName></PPPoEUserName>");
                sb.AppendLine("<ActivateCustomerID>0</ActivateCustomerID>");
                sb.AppendLine("<QuotationNo></QuotationNo>");
                sb.AppendLine("<InvoiceTmpID>20</InvoiceTmpID>");
                sb.AppendLine("<IsChangePlan>False</IsChangePlan>");
                sb.AppendLine("<FromActivateCustomerID>0</FromActivateCustomerID>");
                sb.AppendLine("<InvoiceStatusID>2</InvoiceStatusID>");
                sb.AppendLine("<invTmp>2</invTmp>");
                sb.AppendLine("<TermCondition>1. សូមបង់ប្រាក់មុនថ្ងៃកំណត់។ក្នុងករណីដែលលោកអ្នកខកខានក្នុងការបង់ប្រាក់មិនទាន់ពេលវេលា&amp;#013;&amp;#010;ក្រុមហ៊ុនយើងខ្ញុំ នឹងផ្អាកការប្រើប្រាស់ សេវាកម្មជាបណ្តោះអាសន្ន។&amp;#013;&amp;#010;សេវាកម្មនឹងត្រូវបានភ្ជាប់ឡើងវិញបន្ទាប់ពីការទូទាត់។&amp;#013;&amp;#010;- Please pay before the due date. In case you are of failure to pay not on time, &amp;#013;&amp;#010;our company will temporarily suspend the use of the service. &amp;#013;&amp;#010;The service will be reconnected after payment.&amp;#013;&amp;#010;2.ទឹកប្រាក់ដែលបានបង់រួចមិនដកត្រលប់វិញបានទេ។&amp;#013;&amp;#010;- The payment cannot refundable.&amp;#013;&amp;#010;3. មិនអាចយកទៅប្រកាសពន្ធបានទេ&amp;#013;&amp;#010;- Cannot take to tax declaration</TermCondition>");
                sb.AppendLine("<IsNextInvoice>False</IsNextInvoice>");
                sb.AppendLine("<ChartOfAccountID>177</ChartOfAccountID>");
                sb.AppendLine("<ConnectionID></ConnectionID>");
                sb.AppendLine("<ISChangeCustomerCode>0</ISChangeCustomerCode>");
                sb.AppendLine("<ChangeCustomerCode></ChangeCustomerCode>");
                sb.AppendLine("<AgencyCommissionType></AgencyCommissionType>");
                sb.AppendLine("<Memo></Memo>");
                sb.AppendLine("<LocationID></LocationID>");
                sb.AppendLine("<ApprovalPO>False</ApprovalPO>");
                sb.AppendLine("<CommentInvoice></CommentInvoice>");
                sb.AppendLine("<FormStatus></FormStatus>");
                sb.AppendLine("<CustomerNoted></CustomerNoted>");
                sb.AppendLine("<InvoiceNo></InvoiceNo>");
                sb.AppendLine("<NewCustIDToVAT>0</NewCustIDToVAT>");
                sb.AppendLine("<NewCustIDToVATType></NewCustIDToVATType>");
                sb.AppendLine("<NewCustIDToVATPreFix>0</NewCustIDToVATPreFix>");
                sb.AppendLine("<NewInvoiceToVAT></NewInvoiceToVAT>");
                sb.AppendLine("<NewInvoiceToVATClassID></NewInvoiceToVATClassID>");
                sb.AppendLine("<ISToVATSystem>False</ISToVATSystem>");
                sb.AppendLine("<ISSameInvoiceNo>False</ISSameInvoiceNo>");
                sb.AppendLine("<ISSameCID>False</ISSameCID>");
                sb.AppendLine("<RegisterDate>" + DateTime.Now + "</RegisterDate>");
                sb.AppendLine("<SystemID>13</SystemID>");
                sb.AppendLine("<WarehouseID>16</WarehouseID>");
                sb.AppendLine("<UserName>" + custinfo["UserName"] + "</UserName>");
                sb.AppendLine("<Destination>Uploads/CustomerAttFiles/" + custinfo["CustomerName"] + "</Destination>");
                sb.AppendLine("</CustomerInfo>");
                foreach (var item in custinv)
                {
                    sb.AppendLine("<CustomerInvoice>");
                    sb.AppendLine("<ServiceID>" + item["ServiceID"].ToString() + "</ServiceID>");
                    sb.AppendLine("<PackageID>" + item["PackageID"].ToString() + "</PackageID>");
                    sb.AppendLine("<PackageCode>" + item["PackageCode"].ToString() + "</PackageCode>");
                    sb.AppendLine("<PackageDetailID>" + item["PackageDetailID"].ToString() + "</PackageDetailID>");
                    sb.AppendLine("<ServiceName>" + item["ServiceName"].ToString() + "</ServiceName>");
                    sb.AppendLine("<ItemDescription>" + item["ServiceName"].ToString() + "</ItemDescription>");
                    sb.AppendLine("<UM>" + item["UM"].ToString() + "</UM>");
                    sb.AppendLine("<QTY>" + item["QTY"].ToString() + "</QTY>");
                    sb.AppendLine("<UnitPrice>" + item["UnitPrice"].ToString() + "</UnitPrice>");
                    sb.AppendLine("<DiscountP>" + item["DiscountP"].ToString() + "</DiscountP>");
                    sb.AppendLine("<DiscountUSD>" + item["DiscountUSD"].ToString() + "</DiscountUSD>");
                    sb.AppendLine("<Amount>" + item["Amount"].ToString() + "</Amount>");
                    sb.AppendLine("<Duration>" + item["Duration"].ToString() + "</Duration>");
                    sb.AppendLine("<FreeDuration>" + item["FreeDuration"].ToString() + "</FreeDuration>");
                    sb.AppendLine("<Bandwidth>" + item["Bandwidth"].ToString() + "</Bandwidth>");
                    sb.AppendLine("<DowSpeed>" + item["DowSpeed"].ToString() + "</DowSpeed>");
                    sb.AppendLine("<UpSpeed>" + item["UpSpeed"].ToString() + "</UpSpeed>");
                    sb.AppendLine("<CustomerInvoiceID></CustomerInvoiceID>");
                    sb.AppendLine("<Feature></Feature>");
                    sb.AppendLine("<ConnectionID></ConnectionID>");
                    sb.AppendLine("<PID>" + item["PID"].ToString() + "</PID>");
                    sb.AppendLine("<DepositFee>0</DepositFee>");
                    sb.AppendLine("<ServDepositFee>" + item["ServDepositFee"].ToString() + "</ServDepositFee>");
                    sb.AppendLine("<StartChargingDate>" + item["StartDate"].ToString() + "</StartChargingDate>");
                    sb.AppendLine("<CompletedDate>" + item["StartDate"].ToString() + "</CompletedDate>");
                    sb.AppendLine("<PaymentDueDate>" + item["PaymentDueDate"].ToString() + "</PaymentDueDate>");
                    sb.AppendLine("<StartDate>" + item["StartDate"].ToString() + "</StartDate>");
                    sb.AppendLine("<InvoiceDate>" + item["InvoiceDate"].ToString() + "</InvoiceDate>");
                    sb.AppendLine("<EndDate>" + item["EndDate"].ToString() + "</EndDate>");
                    sb.AppendLine("<HaveServiceIDMoreThenOne>1</HaveServiceIDMoreThenOne>");
                    sb.AppendLine("<EndChargingDate>" + item["EndDate"].ToString() + "</EndChargingDate>");
                    sb.AppendLine("<NextInvoiceDate>" + item["EndDate"].ToString() + "</NextInvoiceDate>");
                    sb.AppendLine("<EndNextInvoiceDate>" + item["EndDate"].ToString() + "</EndNextInvoiceDate>");
                    sb.AppendLine("<IsIncludedVAT>false</IsIncludedVAT>");
                    sb.AppendLine("<EmployeeID>" + custinfo["EmployeeID"].ToString() + "</EmployeeID>");
                    sb.AppendLine("<StatusID>11</StatusID>");
                    sb.AppendLine("<invTmp>2</invTmp>");
                    sb.AppendLine("<InvoiceNo></InvoiceNo>");
                    sb.AppendLine("<IsNextInvoice>False</IsNextInvoice>");
                    sb.AppendLine("<InvoiceType>Created Invoice</InvoiceType>");
                    sb.AppendLine("<OldInvoiceNo></OldInvoiceNo>");
                    sb.AppendLine("<BillingInvoiceID>0</BillingInvoiceID>");
                    sb.AppendLine("<OldBillingInvoiceID>0</OldBillingInvoiceID>");
                    sb.AppendLine("<ConnectivityStatus>43</ConnectivityStatus>");
                    sb.AppendLine("<PPPoEUserName></PPPoEUserName>");
                    sb.AppendLine("<ActivateCustomerID>0</ActivateCustomerID>");
                    sb.AppendLine("<QuotationNo></QuotationNo>");
                    sb.AppendLine("<InvoiceTmpID>20</InvoiceTmpID>");
                    sb.AppendLine("<IsChangePlan>False</IsChangePlan>");
                    sb.AppendLine("<FromActivateCustomerID>0</FromActivateCustomerID>");
                    sb.AppendLine("<InvoiceStatusID>2</InvoiceStatusID>");
                    sb.AppendLine("<ChartOfAccountID>177</ChartOfAccountID>");
                    sb.AppendLine("<LocationID>0</LocationID>");
                    sb.AppendLine("<Warehouse></Warehouse>");
                    sb.AppendLine("<CustomerBillInformationID>0</CustomerBillInformationID>");
                    sb.AppendLine("<NewCustIDToVAT>0</NewCustIDToVAT>");
                    sb.AppendLine("<NewInvoiceToVAT></NewInvoiceToVAT>");
                    sb.AppendLine("<SystemID>13</SystemID>");
                    sb.AppendLine("<WarehouseID>16</WarehouseID>");
                    sb.AppendLine("</CustomerInvoice>");
                }


                sb.AppendLine("<CustomerInvoiceDetail>");
                sb.AppendLine("<StartChargingDate>" + custinv[0]["StartDate"].ToString() + "</StartChargingDate>");
                sb.AppendLine("<CompletedDate>" + custinv[0]["StartDate"].ToString() + "</CompletedDate>");
                sb.AppendLine("<PaymentDueDate>" + custinv[0]["PaymentDueDate"].ToString() + "</PaymentDueDate>");
                sb.AppendLine("<StartDate>" + custinv[0]["StartDate"].ToString() + "</StartDate>");
                sb.AppendLine("<InvoiceDate>" + custinv[0]["InvoiceDate"].ToString() + "</InvoiceDate>");
                sb.AppendLine("<EndDate>" + custinv[0]["EndDate"].ToString() + "</EndDate>");
                sb.AppendLine("<HaveServiceIDMoreThenOne>1</HaveServiceIDMoreThenOne>");
                sb.AppendLine("<IsIncludedVAT>false</IsIncludedVAT>");
                sb.AppendLine("<SubTotal>" + custdetail["SubTotal"].ToString() + "</SubTotal>");
                sb.AppendLine("<VAT>" + custdetail["VAT"].ToString() + "</VAT>");
                sb.AppendLine("<Total>" + custdetail["Total"].ToString() + "</Total>");
                sb.AppendLine("<DiscountP>" + custdetail["DiscountP"].ToString() + "</DiscountP>");
                sb.AppendLine("<DiscountUSD>" + custdetail["DiscountUSD"].ToString() + "</DiscountUSD>");
                sb.AppendLine("<GrandTotal>" + custdetail["GrandTotal"].ToString() + "</GrandTotal>");
                sb.AppendLine("<InvoiceNoted></InvoiceNoted>");
                sb.AppendLine("<EmployeeID>" + custinfo["EmployeeID"].ToString() + "</EmployeeID>");
                sb.AppendLine("<StatusID>11</StatusID>");
                sb.AppendLine("<DepositFee>" + custot["DepositFee"].ToString() + "</DepositFee>");
                sb.AppendLine("<InvoiceNo></InvoiceNo>");
                sb.AppendLine("<OldInvoiceNo></OldInvoiceNo>");
                sb.AppendLine("<CustomerName>" + custinfo["CustomerName"].ToString() + "</CustomerName>");
                sb.AppendLine("<CompanyName></CompanyName>");
                sb.AppendLine("<InvoiceType>Created Invoice</InvoiceType>");
                sb.AppendLine("<BillingInvoiceID>0</BillingInvoiceID>");
                sb.AppendLine("<OldBillingInvoiceID>0</OldBillingInvoiceID>");
                sb.AppendLine("<ConnectivityStatus>43</ConnectivityStatus>");
                sb.AppendLine("<PPPoEUserName></PPPoEUserName>");
                sb.AppendLine("<ActivateCustomerID>0</ActivateCustomerID>");
                sb.AppendLine("<QuotationNo></QuotationNo>");
                sb.AppendLine("<InvoiceTmpID>20</InvoiceTmpID>");
                sb.AppendLine("<IsChangePlan>False</IsChangePlan>");
                sb.AppendLine("<FromActivateCustomerID>0</FromActivateCustomerID>");
                sb.AppendLine("<InvoiceStatusID>2</InvoiceStatusID>");
                sb.AppendLine("<CustomerBillInformationID>0</CustomerBillInformationID>");
                sb.AppendLine("<TermCondition>1. សូមបង់ប្រាក់មុនថ្ងៃកំណត់។ក្នុងករណីដែលលោកអ្នកខកខានក្នុងការបង់ប្រាក់មិនទាន់ពេលវេលា&amp;#013;&amp;#010;ក្រុមហ៊ុនយើងខ្ញុំ នឹងផ្អាកការប្រើប្រាស់ សេវាកម្មជាបណ្តោះអាសន្ន។&amp;#013;&amp;#010;សេវាកម្មនឹងត្រូវបានភ្ជាប់ឡើងវិញបន្ទាប់ពីការទូទាត់។&amp;#013;&amp;#010;- Please pay before the due date. In case you are of failure to pay not on time, &amp;#013;&amp;#010;our company will temporarily suspend the use of the service. &amp;#013;&amp;#010;The service will be reconnected after payment.&amp;#013;&amp;#010;2.ទឹកប្រាក់ដែលបានបង់រួចមិនដកត្រលប់វិញបានទេ។&amp;#013;&amp;#010;- The payment cannot refundable.&amp;#013;&amp;#010;3. មិនអាចយកទៅប្រកាសពន្ធបានទេ&amp;#013;&amp;#010;- Cannot take to tax declaration</TermCondition>");
                sb.AppendLine("<IsNextInvoice>False</IsNextInvoice>");
                sb.AppendLine("<ChartOfAccountID>177</ChartOfAccountID>");
                sb.AppendLine("<ConnectionID></ConnectionID>");
                sb.AppendLine("<SystemID>13</SystemID>");
                sb.AppendLine("<WarehouseID>16</WarehouseID>");
                sb.AppendLine("</CustomerInvoiceDetail>");

                sb.AppendLine("<CustomerOtherCharge>");
                sb.AppendLine("<StatusID>11</StatusID>");
                sb.AppendLine("<Item>Deposit Fee</Item>");
                sb.AppendLine("<PackageDetailID>" + custinv[0]["PackageDetailID"].ToString() + "</PackageDetailID>");
                sb.AppendLine("<PackageID>" + custinv[0]["PackageID"].ToString() + "</PackageID>");
                sb.AppendLine("<CustomerInvoiceID></CustomerInvoiceID>");
                sb.AppendLine("<UM></UM>");
                sb.AppendLine("<QTY>1</QTY>");
                sb.AppendLine("<UnitPrice>" + custot["DepositFee"].ToString() + "</UnitPrice>");
                sb.AppendLine("<DiscountP>0</DiscountP>");
                sb.AppendLine("<DiscountUSD>0</DiscountUSD>");
                sb.AppendLine("<Amount>" + custot["DepositFee"].ToString() + "</Amount>");
                sb.AppendLine("<DepositFee>" + custot["DepositFee"].ToString() + "</DepositFee>");
                sb.AppendLine("<StartDate>" + custinv[0]["StartDate"].ToString() + "</StartDate>");
                sb.AppendLine("<InvoiceDate>" + custinv[0]["InvoiceDate"].ToString() + "</InvoiceDate>");
                sb.AppendLine("<EndDate>" + custinv[0]["EndDate"] + "</EndDate>");
                sb.AppendLine("<IsIncludedVAT>false</IsIncludedVAT>");
                sb.AppendLine("<PaymentDueDate>" + custinv[0]["PaymentDueDate"].ToString() + "</PaymentDueDate>");
                sb.AppendLine("<HaveServiceIDMoreThenOne>1</HaveServiceIDMoreThenOne>");
                sb.AppendLine("<EmployeeID>" + custinfo["EmployeeID"].ToString() + "</EmployeeID>");
                sb.AppendLine("<ItemDescription>Deposit Fee</ItemDescription>");
                sb.AppendLine("<invTmp>2</invTmp>");
                sb.AppendLine("<InvoiceNo></InvoiceNo>");
                sb.AppendLine("<OldInvoiceNo></OldInvoiceNo>");
                sb.AppendLine("<IsNextInvoice>False</IsNextInvoice>");
                sb.AppendLine("<InvoiceType>Created Invoice</InvoiceType>");
                sb.AppendLine("<BillingInvoiceID>0</BillingInvoiceID>");
                sb.AppendLine("<OldBillingInvoiceID>0</OldBillingInvoiceID>");
                sb.AppendLine("<ConnectivityStatus>43</ConnectivityStatus>");
                sb.AppendLine("<PPPoEUserName></PPPoEUserName>");
                sb.AppendLine("<ActivateCustomerID>0</ActivateCustomerID>");
                sb.AppendLine("<QuotationNo></QuotationNo>");
                sb.AppendLine("<InvoiceTmpID>20</InvoiceTmpID>");
                sb.AppendLine("<IsChangePlan>False</IsChangePlan>");
                sb.AppendLine("<FromActivateCustomerID>0</FromActivateCustomerID>");
                sb.AppendLine("<InvoiceStatusID>2</InvoiceStatusID>");
                sb.AppendLine("<TermCondition>1. សូមបង់ប្រាក់មុនថ្ងៃកំណត់។ក្នុងករណីដែលលោកអ្នកខកខានក្នុងការបង់ប្រាក់មិនទាន់ពេលវេលា&amp;#013;&amp;#010;ក្រុមហ៊ុនយើងខ្ញុំ នឹងផ្អាកការប្រើប្រាស់ សេវាកម្មជាបណ្តោះអាសន្ន។&amp;#013;&amp;#010;សេវាកម្មនឹងត្រូវបានភ្ជាប់ឡើងវិញបន្ទាប់ពីការទូទាត់។&amp;#013;&amp;#010;- Please pay before the due date. In case you are of failure to pay not on time, &amp;#013;&amp;#010;our company will temporarily suspend the use of the service. &amp;#013;&amp;#010;The service will be reconnected after payment.&amp;#013;&amp;#010;2.ទឹកប្រាក់ដែលបានបង់រួចមិនដកត្រលប់វិញបានទេ។&amp;#013;&amp;#010;- The payment cannot refundable.&amp;#013;&amp;#010;3. មិនអាចយកទៅប្រកាសពន្ធបានទេ&amp;#013;&amp;#010;- Cannot take to tax declaration</TermCondition>");
                sb.AppendLine("<ChartOfAccountID>177</ChartOfAccountID>");
                sb.AppendLine("<ConnectionID></ConnectionID>");
                sb.AppendLine("<LocationID>0</LocationID>");
                sb.AppendLine("<Warehouse></Warehouse>");
                sb.AppendLine("<CustomerBillInformationID>0</CustomerBillInformationID>");
                sb.AppendLine("<SystemID>13</SystemID>");
                sb.AppendLine("<WarehouseID>16</WarehouseID>");
                sb.AppendLine("</CustomerOtherCharge>");

                sb.AppendLine("</Data>");

                string str = sb.ToString();
                //object[] objs = new object[] { };
                //var objList = objs.ToList();
                //objList.Add(new
                //{
                //	CustomerInfo = CustomerInfo,
                //	//CustomerInvoice = CustomerInvoice,
                //	//CustomerInvoiceDetail = CustomerInvoiceDetail,
                //	//CustomerOtherCharge = CustomerOtherCharge,
                //});
                Dictionary<string, string> Para = new Dictionary<string, string>()
                {
                    { "Data", "N'"+sb.ToString()+"'" },
                    { "ISSO", "2" },
                };

                //string con = "Data Source=192.168.69.83;Initial Catalog=NSM_BILLING_DB;user id=sothea;password=Thea8989; Connection Timeout=900; Pooling=true; Max Pool Size=200; MultipleActiveResultSets=True";
                sqlResult = html.SaveChangeData("SP_INSERT_CUSTOMER", true, Para, true, true);
                string custCode = sqlResult.Data.Tables[0].Rows[0]["CustomerCode"].ToString();
                string custname = sqlResult.Data.Tables[0].Rows[0]["CustomerName"].ToString();
                string CustomerID = sqlResult.Data.Tables[0].Rows[0]["CustomerID"].ToString();
                string InvoiceNo = sqlResult.Data.Tables[0].Rows[0]["InvoiceNo"].ToString();
                string InvoiceDate = sqlResult.Data.Tables[0].Rows[0]["InvoiceDate"].ToString();
                string IsIncludedVAT = sqlResult.Data.Tables[0].Rows[0]["ISVAT"].ToString();
                string StatusID = "11";//sqlResult.Data.Tables[0].Rows[0]["StatusID"].ToString();
                string DescriptionTicketTelegram = sqlResult.Data.Tables[0].Rows[0]["DescriptionTicketTelegram"].ToString();
                string desf = "";
                foreach (string fn in HttpContext.Current.Request.Files)
                {
                    HttpPostedFile file = HttpContext.Current.Request.Files[iFile];
                    string fileName = file.FileName.Replace("'", string.Empty).Replace(",", string.Empty).Trim();
                    Random rnd = new Random();
                    int rndnumber = rnd.Next();
                    string extension = Path.GetExtension(fileName);

                    if (storeFileName == "")
                        storeFileName = fileName;
                    else
                        storeFileName += "~" + fileName;
                    desf = "/Uploads/CustomerAttFiles/" + custname + "-" + custCode;

                    string root = @"" + HttpContext.Current.Request.PhysicalApplicationPath.ToString();
                    desf = root + desf;
                    //desf = HttpContext.Current.Server.MapPath(@"" + desf + "");
                    if (!Directory.Exists(desf))
                        Directory.CreateDirectory(desf);

                    string savedFileName = Path.Combine(desf, fileName);
                    file.SaveAs(savedFileName);
                    //WebImage img = new WebImage(HttpContext.Current.Request.Files[iFile].InputStream);
                    //img.Save(savedFileName);
                    iFile++;
                }
                if (storeFileName != "")
                {
                    string sql = @"insert into dbo.TblCustomerAttachmentFile (CustomerID, InvoiceNo, DestinationFolder, AttachmentFileName, AddDate, SystemID, WarehouseID)
values(@CustomerID, @InvoiceNo, @DestinationFolder, @AttachmentFileName, @AddDate, @SystemID, @WarehouseID)";
                    Para = new Dictionary<string, string>
                {
                    {"CustomerID", CustomerID },
                    {"InvoiceNo", InvoiceNo },
                    {"DestinationFolder", "/Uploads/CustomerAttFiles/" + custname + "-" + custCode  },
                    {"AttachmentFileName", storeFileName },
                    {"AddDate", DateTime.Now.ToString() },
                    {"SystemID", "13" },
                    {"WarehouseID", "16" },
                };
                    html.SaveChangeData(sql, false, Para, false, false);
                }
                NotificationTelegram notificationTelegram = new NotificationTelegram();
                SqlResult sqlResult2 = new SqlResult();
                sqlResult2 = await html.InvoiceToPDFandUpload(custname, custCode, int.Parse(CustomerID), InvoiceNo, InvoiceDate, int.Parse(IsIncludedVAT), int.Parse(StatusID), 13, 16, false, "", "/ISP_Invoice", false);
                string outPutFile = sqlResult2.OutPutFile;
                sqlResult2 = await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 54, null, 0, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegram));
                sqlResult2 = await notificationTelegram.NotificationTelegramBot("", -954558193, "", outPutFile, HttpUtility.UrlDecode(DescriptionTicketTelegram), true);
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return Request.CreateResponse(HttpStatusCode.OK, sqlResult.Message);
        }

        private string GetDeserializedFileName(MultipartFileData fileData)
        {
            var fileName = GetFileName(fileData);
            return JsonConvert.DeserializeObject(fileName).ToString();
        }

        public string GetFileName(MultipartFileData fileData)
        {
            return fileData.Headers.ContentDisposition.FileName;
        }
    }
}