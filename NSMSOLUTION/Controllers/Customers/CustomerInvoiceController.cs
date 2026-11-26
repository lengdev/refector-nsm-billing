using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class CustomerInvoiceController : Controller
    {
        // GET: CustomerInvoice
        //[HttpPost]
        public ActionResult Index(int SystemID, int WarehouseID, int ID, string InvNo, int ISVAT, int StatusID, bool AutoPrint = true, bool ISShowPeriod = true, int CustIDToDMC = 0, string InvoiceToDMC = "", string Type = "", string DomainID = "")
        {
            HTML html = new HTML();
            string dbCon = "";
            if (CustIDToDMC != 0 && InvoiceToDMC != "")
            {
                ID = CustIDToDMC;
                InvNo = InvoiceToDMC;
                dbCon = html.GetDMCConnectionString();
            }
            if (DomainID == "")
                DomainID = html.DomainID;
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "SystemID", SystemID.ToString() },
                { "WarehouseID", WarehouseID.ToString() },
                { "CustID", ID.ToString() },
                {"InvoiceNo", InvNo },
                { "ISVAT", ISVAT.ToString() },
                { "ISShowPeriod", ISShowPeriod.ToString() },
                { "Type", Type.ToString() },
                { "DomainID", DomainID }
            };
            DataSet ds = new DataSet();
            ds = html.LoadData("SP_CUSTOMER_INVOICE", true, Para, dbCon);
            ViewBag.InvoiceData = html.DrawInvoice(ds, false, ISVAT, StatusID, AutoPrint);
            //ViewBag.Data = html.JsonData("SP_CUSTOMER_INVOICE", true, Para, true);
            return View();
        }
        public ActionResult ShowInvoice(int SystemID, int WarehouseID, int ID, string InvNo, int ISVAT, int StatusID, bool AutoPrint = true, int index = 0, string InvoiceType = "", string ShowReceipt = "", int CustIDToDMC = 0, string InvoiceToDMC = "", string Type = "", string DomainID = "", bool IsHideSerialNumber = false)
        {
            ViewBag.ShowReceipt = ShowReceipt;
            ViewBag.InvoiceNumber = InvNo;
            ViewBag.ReceiptNo = "";
            ViewBag.PaymentID = "";
            ViewBag.CompanyID = "";
            HTML html = new HTML();
            if (DomainID == "")
                DomainID = html.DomainID;

            bool ApprovalSaleOrder = HttpContext.GetOwinContext().Request.User.IsInRole("Approval Sale Order");
            ViewBag.ApprovalSaleOrder = ApprovalSaleOrder;
            DataSet ds = new DataSet();
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "SystemID", SystemID.ToString() },
                { "WarehouseID", WarehouseID.ToString() },
                { "CustID", ID.ToString() },
                {"InvoiceNo", InvNo },
                { "ISVAT", ISVAT.ToString() },
                { "Type", Type.ToString() },
                { "DomainID", DomainID },
                { "IsHideSerialNumber", IsHideSerialNumber.ToString() },
            };
            string dbCon = "";
            if (CustIDToDMC != 0 && InvoiceToDMC != "")
            {
                ID = CustIDToDMC;
                InvNo = InvoiceToDMC;
                dbCon = html.GetDMCConnectionString();
                Para = new Dictionary<string, string>
                {
                    {"CustIDToDMC",CustIDToDMC.ToString() },
                    {"InvoiceToDMC",InvoiceToDMC }
                };
                string sql = @"select CustomerID, InvoiceNo, IsIncludedVAT, SystemID, WarehouseID
from dbo.TblCustomerBalance where CustIDToDMC =@CustIDToDMC and InvoiceToDMC = @InvoiceToDMC";
                ds = html.LoadData(sql, false, Para, dbCon);
                DataRow dr = ds.Tables[0].Rows[0];
                Para = new Dictionary<string, string>()
                {
                    { "SystemID", dr["SystemID"].ToString() },
                    { "WarehouseID", dr["WarehouseID"].ToString() },
                    { "CustID", dr["CustomerID"].ToString() },
                    {"InvoiceNo", dr["InvoiceNo"].ToString() },
                    { "ISVAT", dr["IsIncludedVAT"].ToString() },
                    { "DomainID", DomainID }
                };
                ISVAT = dr["IsIncludedVAT"].ToString() == "True" ? 1 : 0;
            }

            ds = html.LoadData("SP_CUSTOMER_INVOICE", true, Para, dbCon);
            ViewBag.InvoiceFileName = "";
            if (ds.Tables[9].Rows.Count > 0)
                ViewBag.InvoiceFileName = ds.Tables[9].Rows[0][0].ToString();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(html.DrawInvoice(ds, false, ISVAT, StatusID, AutoPrint, true));

            string SQL = "select InvoiceNo, format(InvoiceDate, 'dd/MM/yyy') InvoiceDate, [Type],";
            SQL += "format(PaymentDueDate, 'dd/MM/yyyy') PaymentDueDate, case when isnull(NextInvoiceDate,'') <> '' then format(NextInvoiceDate, 'yyyy-MM-dd') else '' end NextInvoiceDate, InvoiceStatusID from dbo.TblCustomerInvoice ";
            SQL += "where CustomerID = " + ID + " and case when '" + Type + "' = 'Credit Notes' then CreditNoteNo else InvoiceNo end = N'" + InvNo + "' and SystemID = " + SystemID + " and WarehouseID = " + WarehouseID + " ";
            DataSet dsc = html.LoadData(SQL);

            string PaymentID = ds.Tables[1].Rows[0]["PaymentID"].ToString();
            string CompanyID = ds.Tables[1].Rows[0]["InvoiceTmpID"].ToString();
            string ReceiptNo = ds.Tables[1].Rows[0]["ReceiptNo"].ToString();
            string NextInvoiceDate = dsc.Tables[0].Rows[0]["NextInvoiceDate"].ToString();
            string CompanyAddress = ds.Tables[0].Rows[0]["Address"].ToString();
            string CPhone = ds.Tables[0].Rows[0]["Phone"].ToString();
            string CEmail = ds.Tables[0].Rows[0]["Email"].ToString();
            string CWebsite = ds.Tables[0].Rows[0]["Website"].ToString();
            string CustomerName = ds.Tables[1].Rows[0]["TypeOfInvoice"].ToString();
            string TypeOfInvoice = ds.Tables[1].Rows[0]["TypeOfInvoice"].ToString();
            string CustomerCode = ds.Tables[1].Rows[0]["CustomerCode"].ToString();
            string Email = ds.Tables[0].Rows[0]["Email"].ToString();
            string InvoiceDate = ds.Tables[1].Rows[0]["InvoiceDate"].ToString();
            string EndDate = ds.Tables[1].Rows[0]["EndDate"].ToString();
            string ClassName = ds.Tables[1].Rows[0]["ClassName"].ToString();
            string PaymentDueDate = ds.Tables[1].Rows[0]["PaymentDueDate"].ToString();
            string GrandTotal = "0";
            if (ds.Tables[5].Rows.Count > 0)
                GrandTotal = ds.Tables[5].Rows[0]["GrandTotal"].ToString();
            string ServiceName = ds.Tables[6].Rows.Count > 0 ? ds.Tables[6].Rows[0]["ServiceName"].ToString() : "";
            string CustomerEmail = ds.Tables[1].Rows[0]["CustomerEmail"].ToString();
            string InvDate = DateTime.Parse(InvoiceDate).ToString("yyyyMMdd");
            Type = dsc.Tables[0].Rows[0]["Type"].ToString();
            string InvoiceStatusID = dsc.Tables[0].Rows[0]["InvoiceStatusID"].ToString();
            ViewBag.ReceiptNo = ReceiptNo;
            ViewBag.PaymentID = PaymentID;
            ViewBag.CompanyID = CompanyID;
            //CustomerName = $"{html.ToUrlSlug($"{CustomerCode}")}";
            //
            //string pdfFileName = InvDate + "-" + CustomerName + "-" + InvoiceNo + ".pdf";
            //string outputFile = FilePath + "\\" + pdfFileName;
            //SQL = @"select DestinationFolder, PDFFileName, PNGFileName from dbo.TblCustomerBillInformation where InvoiceNo='" + InvNo + "' and SystemID =" + SystemID + " and WarehouseID=" + WarehouseID;
            SQL = @"if(exists (select DestinationFolder from dbo.TblCustomerBillInformation where InvoiceNo='" + InvNo + "' and SystemID =" + SystemID + " and WarehouseID=" + WarehouseID + @"))
            select DestinationFolder, PDFFileName, PNGFileName from dbo.TblCustomerBillInformation where InvoiceNo='" + InvNo + "' and SystemID =" + SystemID + " and WarehouseID=" + WarehouseID + @"
            else
            select DestinationFolder, PDFFileName, PNGFileName from dbo.TblCustomerBillingInvoice where InvoiceNo='" + InvNo + "' and SystemID =" + SystemID + " and WarehouseID=" + WarehouseID;
            //ds = html.LoadData(SQL);
            string InvoiceFileName = "";//ds.Tables[0].Rows[0]["PDFFileName"].ToString();//ds.Tables[0].Rows[0]["DestinationFolder"].ToString() +//$"{html.ToUrlSlug($"{CustomerName + " [" + CustomerCode + "]"}")}";
                                        //string FilePath = "Uploads/CustomerAttFiles/" + InvoiceFileName + "/Invoices/";
                                        //HttpContext.Server.MapPath(@"/Uploads/CustomerAttFiles/" + InvoiceFileName + "/Invoices/");
                                        //InvoiceFileName = FilePath + InvDate + "-" + $"{html.ToUrlSlug($"{CustomerCode}")}";
                                        //InvoiceFileName += "_" + $"{html.ToUrlSlug($"{InvNo}")}" + ".pdf";
            string PDF_FileName = ""; //ds.Tables[0].Rows[0]["PDFFileName"].ToString();
            string PNG_FileName = "";// ds.Tables[0].Rows[0]["PNGFileName"].ToString();
            var Data = Json(new
            {
                CustomerID = ID,
                CustomerName = CustomerName,
                CustomerCode = CustomerCode,
                Email = Email,
                InvoiceNo = InvNo,
                InvoiceDate = InvoiceDate,
                EndDate = EndDate,
                ClassName = ClassName,
                PaymentDueDate = PaymentDueDate,
                GrandTotal = GrandTotal,
                ServiceName = ServiceName,
                CompanyAddress = CompanyAddress,
                CPhone = CPhone,
                CEmail = CEmail,
                CWebsite = CWebsite,
                CustomerEmail = CustomerEmail,
                InvoiceType = InvoiceType,
                NextInvoiceDate = NextInvoiceDate,
                StatusID = StatusID,
                InvoiceFileName = InvoiceFileName,
                PDF_FileName = PDF_FileName,
                PNG_FileName = PNG_FileName,
                FilePath = "",//ds.Tables[0].Rows[0]["DestinationFolder"].ToString(),
                Type = Type,
                TypeOfInvoice = TypeOfInvoice,
                InvoiceStatusID = InvoiceStatusID
            });
            sb.AppendLine("<script>");
            //sb.AppendLine("var exampleTriggerEl = document.getElementById('CloneInvoice');");
            //sb.AppendLine("var tooltip = new bootstrap.Tooltip(exampleTriggerEl);");
            //sb.AppendLine("$(\"#PrintInv\").click(function () {");
            //sb.AppendLine("PrintInvoice(" + ID + ", '" + InvNo + "', " + ISVAT + ", " + StatusID + ");");
            //sb.AppendLine("});");
            sb.AppendLine("$(\"#Make_Payment\").click(function () {");
            sb.AppendLine("Payment('" + InvNo + "','" + CustomerName + "','" + float.Parse(GrandTotal).ToString("#,#.00") + "',10, " + ID + ");");
            //sb.AppendLine("$(\"a[name=Payment]\").eq(\"" + index + "\").click();");
            sb.AppendLine("});");
            //sb.AppendLine("$(\"#CloneInvoice\").click(function () {");
            //sb.AppendLine("$('[role=\"tooltip\"]').remove();");
            //sb.AppendLine("EditTblRow(\"\", \"\", '" + InvNo + "', true, '" + NextInvoiceDate + "', "+ ID + ");");
            //sb.AppendLine("});");
            sb.AppendLine("</script>");

            ViewBag.InvoiceData = sb.ToString();
            ViewBag.Data = JsonConvert.SerializeObject(Data);
            //var Renderer = new IronPdf.ChromePdfRenderer();
            //var pdf = Renderer.RenderHtmlAsPdf(sb.ToString());
            //pdf.SaveAs("~/Content/output.pdf");
            return View();
        }

        public JsonResult CancelInvoice(string InvoiceNo, string Comment)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "InvoiceNo", InvoiceNo },
                { "Comment", Comment },
                { "UserName", html.UserFullName },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            //For DMC
            //            string sql = @"select c.CustomerName, c.CustomerCode, c.CustomerID, bi.InvoiceNo, bi.InvoiceDate, 
            //bi.IsIncludedVAT, bi.StatusID, bi.SystemID, bi.WarehouseID
            //from dbo.TblCustomerBillingInvoice bi
            //inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
            //where bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.InvoiceNo = '" + InvoiceNo + "'";
            //            DataRow dr = html.LoadData(sql).Tables[0].Rows[0];
            //            string CustomerName = dr["CustomerName"].ToString();
            //            string CustomerCode = dr["CustomerCode"].ToString();
            //            int CustomerID = int.Parse(dr["CustomerID"].ToString());
            //            string InvoiceDate = dr["InvoiceDate"].ToString();
            //            int IsIncludedVAT = (dr["IsIncludedVAT"].ToString() == "False" ? 0 : 1);
            //            string StatusID = dr["StatusID"].ToString();

            //await html.InvoiceToPDFandUpload(CustomerName, CustomerCode, CustomerID, InvoiceNo, InvoiceDate, IsIncludedVAT, int.Parse(StatusID), int.Parse(html.SystemID), int.Parse(html.WarehouseID), true, "", "/ISP_Cancel_Invoice/");
            sqlResult = html.SaveChangeData("SP_CANCEL_CUSTOMER_INVOICE", true, para);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult InvoiceToPDF(object[] Data)//int SystemID, int WarehouseID, int ID, string CustomerName, string InvNo, int ISVAT, int StatusID, bool AutoPrint = true
        {
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(Data[0].ToString());
            ////int CustomerID, string InvoiceNo, int IsIncludedVAT, int StatusID, string SystemID, string WarehouseID
            //ViewBag.InvoiceNumber = DataInfo["InvoiceNo"];
            //ViewBag.CustomerName = CustomerName;
            HTML html = new HTML();
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "SystemID", DataInfo["SystemID"].ToString() },
                { "WarehouseID", DataInfo["WarehouseID"].ToString() },
                { "CustID", DataInfo["CustomerID"].ToString() },
                {"InvoiceNo", DataInfo["InvoiceNo"] },
                { "ISVAT", DataInfo["IsIncludedVAT"].ToString() }
            };
            DataSet ds = new DataSet();
            ds = html.LoadData("SP_CUSTOMER_INVOICE", true, Para);
            StringBuilder sb = new StringBuilder();
            //sb.AppendLine("<div id=\"div_Cust_Invoice\" class=\"bg-white p-2\">");
            sb.AppendLine(html.DrawInvoice(ds, false, DataInfo["IsIncludedVAT"], DataInfo["StatusID"], false));
            //sb.AppendLine("</div>");
            //ViewBag.InvoiceData = sb.ToString();
            //return View();
            return Json(new { InvData = sb.ToString(), CustomerName = DataInfo["CustomerName"] }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveInvoiceToPDF(string CustomerName, string InvoiceNo, string pdfData)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            InvoiceNo = CustomerName + "_" + InvoiceNo;
            CustomerName = $"{html.ToUrlSlug($"{CustomerName}")}";
            InvoiceNo = $"{html.ToUrlSlug($"{InvoiceNo}")}";
            Regex regex = new Regex(@"^[\w/\:.-]+;base64,");
            string base64 = regex.Replace(pdfData, string.Empty);
            byte[] bitmapData = Convert.FromBase64String(html.FixBase64ForImage(base64));
            pdfData = html.UploadToServer(bitmapData, "Uploads/CustomerAttFiles/" + CustomerName + "/Invoices/", InvoiceNo, true, false);
            if (pdfData != "")
            {
                //WebClient client = new WebClient();
                //client.Credentials = new NetworkCredential("dmc-ftp", "P@ssword@DMC#2022");
                var url = "ftp://103.134.177.164:2022/" + pdfData;
                //Uploads/CustomerAttFiles/" + CustomerName + "/Invoices/", InvoiceNo
                string filepath = HttpContext.Server.MapPath(@"/Uploads/CustomerAttFiles/" + CustomerName + "/Invoices/" + pdfData);
                //client.UploadFile(url, filepath);
                //client.Dispose();
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Credentials = new NetworkCredential("dmc-ftp", "P@ssword@DMC#2022");
                request.Method = WebRequestMethods.Ftp.UploadFile;

                using (Stream fileStream = System.IO.File.OpenRead(filepath))
                using (Stream ftpStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(ftpStream);
                }
                sqlResult.Result = ResultType.Successful;
                sqlResult.Message = "Invoice to PDF file successful.";
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Invoice(int CustomerID, string InvoiceNo, string InvoiceDate, string InvoiceDueDate)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string> {
                { "CustID", CustomerID.ToString() },
                { "InvoiceNo", InvoiceNo },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
            };
            string sql = "SP_NOTIFICATION_PAYMENT";
            DataSet ds = html.LoadData(sql, true, para);
            if (ds != null && ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
            {
                string invType = ds.Tables[1].Columns.Contains("TypeOfInvoice") ? ds.Tables[1].Rows[0]["TypeOfInvoice"]?.ToString()?.Trim() ?? "" : "";
                bool isVAT = invType.Equals("VAT", StringComparison.OrdinalIgnoreCase) || invType.Equals("CM", StringComparison.OrdinalIgnoreCase);
                if (!isVAT && ds.Tables[1].Columns.Contains("ExchangeRate"))
                {
                    ds.Tables[1].Rows[0]["ExchangeRate"] = 4200;
                }
            }
            ViewBag.DrawInvoice = html.DrawInvoice(ds, false, 0, 0, false, false, true, InvoiceDate, InvoiceDueDate);
            return View();
        }
    }
}