using MVC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace NSMSOLUTION.Controllers.API
{
    [RoutePrefix("api/dmc-report")]
    public class DMCReportController : ApiController
    {
        private async Task<string> GenerateCsvAndUpload(string storedProcedure, Dictionary<string, string> para, string fileName, string uploadFolder)
        {
            HTML html = new HTML();

            var dataSet = html.LoadData(storedProcedure, true, para);
            string csv = html.JsonToCsv(dataSet);

            string folderPath = HttpContext.Current.Server.MapPath("~/Uploads/CSV_File_For_DMC/");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, fileName);

            File.WriteAllText(fullPath, csv, new UTF8Encoding(true));
            await html.UploadFileToServer(folderPath, fileName, uploadFolder);

            return fullPath;
        }
        private IHttpActionResult DownloadCsv(string fullPath, string fileName)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(File.ReadAllBytes(fullPath))
            };
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            return ResponseMessage(result);
        }
        [HttpGet]
        [Route("invoice-summary")]
        public async Task<IHttpActionResult> ReportInvoiceSummary()
        {
            DateTime today = DateTime.Today;
            DateTime firstDay = new DateTime(today.Year, today.Month, 1);
            DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);

            var para = new Dictionary<string, string> {
                { "@FromDate", firstDay.ToString("yyyy-MM-dd") },
                { "@ToDate", lastDay.ToString("yyyy-MM-dd") },
                { "@IsUploadToDMC", "1" }
            };

            string fileName = $"{DateTime.Now:yyyyMM}_Invoice_Summary_Report.csv";
            string fullPath = await GenerateCsvAndUpload("SP_NSM_INVOICE_SUMMARY", para, fileName, "/Invoice_Summary/");
            return DownloadCsv(fullPath, fileName);
        }
        [HttpGet]
        [Route("customer-info-list")]
        public async Task<IHttpActionResult> ReportCustomerListInfo(bool upload = false)
        {
            HTML html = new HTML();
            var para = new Dictionary<string, string> {
                { "@IsUploadToDMC", "1" }
            };
            string sql = "SP_NSM_CUSTOMER_INFO_LIST";
            string fileName = $"{DateTime.Now:yyyyMM}_NSM_ISP_Cust_Info.csv";
            string fullPath = await GenerateCsvAndUpload(sql, para, fileName, "/ISP_Customer_Info_List/");

            if (upload)
            {
                string folderPath = HttpContext.Current.Server.MapPath("~/Uploads/CSV_File_For_DMC/");
                html.JsonToCustomCsv(html.LoadData(sql, true, para), folderPath + fileName);
            }

            return DownloadCsv(fullPath, fileName);
        }
    }
}
