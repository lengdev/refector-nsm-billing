using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NSMSOLUTION.Controllers.API
{
    [Authorize]
    public class NotificationTelegramController : ApiController
    {

        public NotificationTelegramController()
        {
        }
        // POST api/<controller>
        [Authorize]
        [Route("api/NotificationTelegram/Notification")]
        public async Task<HttpResponseMessage> Notification()
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            StringBuilder MsgBody = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            DataSet ds = new DataSet();

            //send notification for new customer register.
            #region send notification to customer service will expired in next 3 days.
            //send notification to customer service will expired in next 3 days.
            string sql = @"select c.CustomerID, c.CustomerCode, c.CustomerName LastName, ta.PPPoEUserName, ta.BillingInvoiceID, bi.CompanyEmail [Email],
(c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM +
case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName,
format(ta.StartDate, 'dd-MMM-yyyy') StartDate, format(ta.EndDate, 'dd-MMM-yyyy') EndDate, b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MMM-yyyy') PaymentDueDate
, ta.ConnectionType, ta.VLanID, bi.CompanyContactMobileNumber, b.DestinationFolder, b.PDFFileName, c.ATTName
from dbo.TblActivateCustomer ta
inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID
inner join dbo.TblServices s on s.ServiceID = ta.ServiceID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ta.PackageDetailID
inner join dbo.TblCustomerBillingInvoice b on b.BillingInvoiceID = ta.BillingInvoiceID
inner join dbo.TblCustomerBillInformation bi on bi.CustomerBillInformationID = b.CustomerBillInformationID
where b.StatusID not in(12,13,14,20) and format(dateadd(d,-3,ta.EndDate), 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')";
            ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string ATTName = dr["ATTName"].ToString();
                    string CustomerID = dr["CustomerID"].ToString();
                    string ConnectionType = dr["ConnectionType"].ToString();
                    string PPPoEUser = dr["PPPoEUserName"].ToString();
                    string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                    string Email = dr["Email"].ToString();
                    string CustomerName = dr["CustomerName"].ToString();
                    string FirstName = dr["CustomerCode"].ToString();
                    string LastName = dr["LastName"].ToString();
                    string ServiceName = dr["ServiceName"].ToString();
                    string PackageName = dr["PackageName"].ToString();
                    string StartDate = dr["StartDate"].ToString();
                    string EndDate = dr["EndDate"].ToString();
                    string Amount = dr["Amount"].ToString();
                    string InvoiceNo = dr["InvoiceNo"].ToString();
                    string PaymentDueDate = dr["PaymentDueDate"].ToString();
                    string VLanID = dr["VLanID"].ToString();
                    string CustomerPhone = dr["CompanyContactMobileNumber"].ToString();
                    string PDFFileName = dr["PDFFileName"].ToString();
                    string DestinationFolder = dr["DestinationFolder"].ToString();
                    string Subject = "YOUR " + ServiceName.ToUpper() + " WILL BE EXPIRED ON DATE: " + EndDate;
                    //MsgBody.AppendLine(Subject);
                    MsgBody.AppendLine("We would like confirm your service will be expired on date: " + EndDate + ".");
                    MsgBody.AppendLine("<br />Your current service: " + ServiceName);
                    MsgBody.AppendLine("<br />Package Name: " + PackageName);
                    //MsgBody.AppendLine("<br />Payment Due Date: " + PaymentDueDate);
                    MsgBody.AppendLine("<br />Start Date: " + StartDate);
                    MsgBody.AppendLine("<br />Expired Date: " + EndDate);
                    MsgBody.AppendLine("<br />Amount USD: " + Amount);
                    MsgBody.AppendLine("<br />Phone Number: " + CustomerPhone);
                    MsgBody.AppendLine("<br />ATT Name: " + ATTName);
                    if (Email != "")
                        MsgBody.AppendLine("<br />Email: " + Email);

                    sb.AppendLine(Subject);
                    sb.AppendLine("\nDear " + CustomerName + ",\n");
                    sb.AppendLine("Greeting from NSM SOLUTION CO., LTD and thanks for using our services.\n");
                    sb.AppendLine("We would like confirm your service will be expired on date: " + EndDate + ".");
                    sb.AppendLine("");
                    sb.AppendLine("Your current service: " + ServiceName);
                    //sb.AppendLine("\n");
                    sb.AppendLine("Package Name: " + PackageName);
                    //MsgBody.AppendLine("<br />Payment Due Date: " + PaymentDueDate);
                    sb.AppendLine("");
                    sb.AppendLine("Start Date: " + StartDate);
                    //sb.AppendLine("\n");
                    sb.AppendLine("Expired Date: " + EndDate);
                    //sb.AppendLine("\n");
                    sb.AppendLine("Amount USD: " + Amount);
                    //sb.AppendLine("Customer Phone Number: " + CustomerPhone);
                    if (Email != "")
                        sb.AppendLine("Email: " + Email);
                    sb.AppendLine("");
                    sb.AppendLine("Thank you and best regards,\n");
                    //sb.AppendLine("<a href=\"https://db.nsmisp.com.kh/Content/NSMLogo.png\">&#8204;</a>");
                    //sb.AppendLine("<a href=\"https://db.nsmisp.com.kh/Content/NSMISPLogo.png\">&#8204;</a>");

                    sb.AppendLine("NSM SOLUTION AND MAINTENANCE CO.,LTD");
                    sb.AppendLine("#1159No1, Street Number 2, Phum Prek Tanu, Sangkat Chak Angre Leu, Khan Mean Chey, Phnom Penh.");
                    sb.AppendLine("Customer Service: 086 588 777, 078 588 777");
                    //sb.AppendLine("Website: nsmisp.com.kh");
                    //sb.AppendLine("Website: https://nsmisp.com.kh/");
                    //sb.AppendLine("https://db.nsmisp.com.kh/Content/NSMISPLogo.png");
                    //sb.AppendLine("<a href=\"https://nsmisp.com.kh/NSM_LOGO/NSM_ISP_Logo.png\">&#8204;</a>");
                    //sb.AppendLine("https://nsmisp.com.kh");
                    //sqlResult = notificationTelegram.NotificationWillBeLockUser(sb.ToString());
                    CustomerPhone = CustomerPhone.Trim().Replace(" ", string.Empty);
                    var regex = new Regex(Regex.Escape("0"));
                    CustomerPhone = regex.Replace(CustomerPhone, "+855", 1);

                    //sqlResult = notificationTelegram.NotificationToCustomer(CustomerPhone, FirstName, LastName, int.Parse(CustomerID), sb.ToString(), DestinationFolder, PDFFileName);

                    //html.SendEmailToCustomer(Subject, CustomerName, Email, MsgBody.ToString());
                    //html.SendEmailToBillingAndTech(Subject, CustomerName, Email, MsgBody.ToString());
                    sb.Clear();
                }
            }
            #endregion

            #region send notification due to system locked
            //set notification due to system locked    
            sql = @"select distinct c.CustomerCode CID, c.CustomerName, cb.CompanyContactMobileNumber, ta.[SID], ta.ConnectionID,
bi.PackageName, 'Start Date: ' + convert(varchar, format (ta.StartDate, 'dd-MMM-yyyy')) + ' End Date: ' + convert(varchar, format (ta.EndDate, 'dd-MMM-yyyy')) + '<br>' +
convert(varchar, DATEDIFF(d, ta.StartDate, ta.EndDate)) + ' Days, ' + convert(varchar, DATEDIFF(m, ta.StartDate, ta.EndDate)) + ' Months '+
case when DATEDIFF(d, getdate(), ta.EndDate) > 0 then case when DATEDIFF(d, getdate(), ta.StartDate) <= 0 then 'used ' + convert(varchar, case when DATEDIFF(day, ta.StartDate, GETDATE()) > 0 then convert(varchar, DATEDIFF(day, ta.StartDate, GETDATE())) + ' days,' + convert(varchar, DATEDIFF(d, getdate(), ta.EndDate)) + ' day left' else '0' end) else '(Expired)' end else '(Expired)' end Duration,
ta.Comment, c.ATTName
from dbo.TblActivateCustomer ta
inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID
inner join dbo.TblCustomerBillInformation cb on cb.CustomerID = ta.CustomerID
inner join dbo.TblCustomerBillingInvoice bi on bi.InvoiceNo = ta.InvoiceNo and bi.CustomerID = ta.CustomerID
where ta.ConnectivityStatus = 41 and bi.StatusID not in(12,13,14,20)
and format(ta.LockedDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')";
            Dictionary<string, string> para = new Dictionary<string, string>();
            ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string TelegramComment = "Dear Team, <br><br>";
                TelegramComment += "Please check list below that is customer locked lists.";
                int i = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    i++;
                    string ConnectionID = dr["ConnectionID"].ToString();
                    string SID = dr["SID"].ToString();
                    string CID = dr["CID"].ToString();
                    string CName = dr["CustomerName"].ToString();
                    string CompanyContactMobileNumber = dr["CompanyContactMobileNumber"].ToString();
                    string CustomerName = dr["CustomerName"].ToString();
                    string PackageName = dr["PackageName"].ToString();
                    string Duration = dr["Duration"].ToString();
                    string Comment = dr["Comment"].ToString();
                    string ATTName = dr["ATTName"].ToString();


                    TelegramComment += "<br><br><b>" + i + ". CID: " + CID + "</b><br>";
                    TelegramComment += "<b>Customer Name: " + CName + "</b><br>";
                    TelegramComment += "<b>Phone: " + CompanyContactMobileNumber + "</b><br><br>";
                    TelegramComment += "<b>ATT Name: " + ATTName + "</b><br><br>";
                    TelegramComment += "<b>Package Name: " + PackageName + "</b><br><br>";
                    TelegramComment += "Duration: " + Duration + "<br><br>";
                    TelegramComment += "Comment: <b>" + Comment + "</b>";
                }
                TelegramComment += "<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO SEND NOTIFICATION. " + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("dd/MM/yyyy hh:mm:ss tt");
                await notificationTelegram.NotificationTelegramBotToTopic(TelegramComment.Replace("<br>", "\n"), -1545132903, 517);
                await notificationTelegram.NotificationTelegramBotToTopic(TelegramComment.Replace("<br>", "\n"), -1903890245, 4);

                //string Subject = "Please check list below that is customer locked lists.";
                //sb.Clear();
                //sb.AppendLine("CUSTOMER SERVICES LOCKED!");
                //sb.AppendLine("\nDear TEAM,");
                //sb.AppendLine("\n" + Subject);
                //int i = 0;
                //StringBuilder sbcust = new StringBuilder();
                //foreach (DataRow dr in ds.Tables[0].Rows)
                //{
                //    i++;
                //    string Email = dr["Email"].ToString();
                //    string CustomerID = dr["CustomerID"].ToString();
                //    string CustomerName = dr["CustomerName"].ToString();
                //    string PhoneNumber = dr["PhoneNumber"].ToString();
                //    string ServiceName = dr["ServiceName"].ToString();
                //    string PackageName = dr["PackageName"].ToString();
                //    string StartDate = dr["StartDate"].ToString();
                //    string EndDate = dr["EndDate"].ToString();
                //    string Amount = dr["GrandTotal"].ToString();
                //    string Comment = dr["Comment"].ToString();
                //    string LockedBy = dr["LockedBy"].ToString();
                //    string LockedDate = dr["LockedDate"].ToString();
                //    string PDFFileName = dr["PDFFileName"].ToString();
                //    string DestinationFolder = dr["DestinationFolder"].ToString();
                //    string FirstName = dr["CustomerCode"].ToString();
                //    string LastName = dr["LastName"].ToString();
                //    sb.AppendLine("\n------------------------");
                //    sb.AppendLine(i + ". Customer Name: " + CustomerName);
                //    sb.AppendLine("Customer Phone: " + PhoneNumber);
                //    sb.AppendLine("\nService Name: " + ServiceName);
                //    sb.AppendLine("Package Name: " + PackageName);
                //    sb.AppendLine("\nStart Date: " + StartDate);
                //    sb.AppendLine("Expired Date: " + EndDate);
                //    sb.AppendLine("Amount USD: " + Amount);
                //    if (Email != "")
                //        sb.AppendLine("Email: " + Email);
                //    sb.AppendLine("\nLocked By: " + LockedBy);
                //    sb.AppendLine("Locked Date: " + LockedDate);
                //    //sb.AppendLine("Comment: " + Comment);

                //    sbcust.Clear();
                //    sbcust.AppendLine("YOUR " + ServiceName.ToUpper() + " IS LOCKED.");
                //    sbcust.AppendLine("\nDear " + CustomerName + ",\n");
                //    sbcust.AppendLine("Greeting from NSM SOLUTION CO., LTD.\n");
                //    sbcust.AppendLine("We would like info you, your service is locked due to expired and not yet renew. Please kindly contact us for renew.\n");
                //    sbcust.AppendLine("Your current service: " + ServiceName);
                //    sbcust.AppendLine("Package Name: " + PackageName + "\n");
                //    sbcust.AppendLine("Start Date: " + StartDate);
                //    sbcust.AppendLine("Expired Date: " + EndDate);
                //    sbcust.AppendLine("Amount USD: " + Amount);
                //    if (Email != "")
                //        sbcust.AppendLine("Email: " + Email);
                //    sbcust.AppendLine("\nLocked By: " + LockedBy);
                //    sbcust.AppendLine("Locked Date: " + LockedDate);
                //    sbcust.AppendLine("Comment: " + Comment + "\n");
                //    sbcust.AppendLine("Thank you and best regards,\n");
                //    sbcust.AppendLine("NSM SOLUTION AND MAINTENANCE CO.,LTD");
                //    sbcust.AppendLine("#1159No1, Street Number 2, Phum Prek Tanu, Sangkat Chak Angre Leu, Khan Mean Chey, Phnom Penh.");
                //    sbcust.AppendLine("Customer Service: 086 588 777, 078 588 777");
                //    sbcust.AppendLine("Website: nsmisp.com.kh");
                //    PhoneNumber = PhoneNumber.Trim().Replace(" ", string.Empty);
                //    var regex = new Regex(Regex.Escape("0"));
                //    PhoneNumber = regex.Replace(PhoneNumber, "+855", 1);
                //    sqlResult = notificationTelegram.NotificationToCustomer(PhoneNumber, FirstName, LastName, int.Parse(CustomerID), sbcust.ToString(), DestinationFolder, PDFFileName);
                //}
                //sqlResult = notificationTelegram.NotificationTelegramBot(sb.ToString());
                //html.SendEmailToBillingAndTech(Subject, "TEAM", "", sb.ToString());
                //sb.Clear();
            }
            #endregion

            #region set notification due to system down speed
            //set notification due to system down speed
            sql = @"select ta.RouterID, ta.PPPoEUserName, ta.PPPoEPassword, ta.BillingInvoiceID, ta.ActivateCustomerID, c.CustomerCode CID, c.CustomerName CName, ta.ConnectionID, ta.SID, bf.CompanyContactMobileNumber,
(c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM +
case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName, ta.VLanName, ta.VLanIPAddress, ta.IPAddress CustomerIPAddress, ta.GetwayIP, ta.TicketNumber,
format(b.StartDate, 'dd-MM-yyyy') StartDate, format(b.EndDate, 'dd-MM-yyyy') EndDate, b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate
, ta.ConnectionType, ta.VLanID, c.ATTName from dbo.TblActivateCustomer ta
inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID
inner join dbo.TblServices s on s.ServiceID = ta.ServiceID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ta.PackageDetailID
inner join dbo.TblCustomerBillingInvoice b on b.ActivateCustomerID = ta.ActivateCustomerID inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = b.CustomerBillInformationID
where ta.ConnectivityStatus = 50 and b.StatusID not in(12,13)";
            para = new Dictionary<string, string>();
            ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string TelegramComment = "Dear Team, <br><br>";
                TelegramComment += "Please check customers down speed list due to late payment and system will be locked in next 3 days @" + DateTime.Parse(html.GetCurrentDate().ToString()).AddDays(3).ToString("dd/MM/yyyyy 00:00:00") + ").";
                int i = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    i++;
                    string ATTName = dr["ATTName"].ToString();
                    string ConnectionID = dr["ConnectionID"].ToString();
                    string SID = dr["SID"].ToString();
                    string CID = dr["CID"].ToString();
                    string CName = dr["CName"].ToString();
                    string CompanyContactMobileNumber = dr["CompanyContactMobileNumber"].ToString();
                    string ConnectionType = dr["ConnectionType"].ToString();
                    string PPPoEUserName = dr["PPPoEUserName"].ToString();
                    string PPPoEPassword = dr["PPPoEPassword"].ToString();
                    string VLanName = dr["VLanName"].ToString();
                    string VLanIPAddress = dr["VLanIPAddress"].ToString();
                    string CustomerIPAddress = dr["CustomerIPAddress"].ToString();
                    string GetwayIP = dr["GetwayIP"].ToString();
                    string TicketNumber = dr["TicketNumber"].ToString();
                    string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                    string CustomerName = dr["CustomerName"].ToString();
                    string ServiceName = dr["ServiceName"].ToString();
                    string PackageName = dr["PackageName"].ToString();
                    string StartDate = dr["StartDate"].ToString();
                    string EndDate = dr["EndDate"].ToString();
                    string Amount = dr["Amount"].ToString();
                    string InvoiceNo = dr["InvoiceNo"].ToString();
                    string RouterID = dr["RouterID"].ToString();
                    string PaymentDueDate = dr["PaymentDueDate"].ToString();
                    string VLanID = dr["VLanID"].ToString();
                    string ActivateCustomerID = dr["ActivateCustomerID"].ToString();

                    TelegramComment += "<br><br><b>" + i + ". SID: " + SID + "</b><br>";
                    TelegramComment += "<b>Connection ID: " + ConnectionID + "</b><br>";
                    TelegramComment += "<b>CID: " + CID + "</b><br>";
                    TelegramComment += "Customer Name: " + CName + "<br>";
                    TelegramComment += "<b>Phone: " + CompanyContactMobileNumber + "</b><br><br>";
                    TelegramComment += "<b>Current Package Name: " + PackageName + "</b><br><br>";
                    TelegramComment += "<b>NOW SET TO PACKAGE NAME: ALTER 3 DAYS EXPIRED = 3 mbps.</b><br>";
                    TelegramComment += "Amount USD: " + Amount + "<br>";
                    TelegramComment += "Invoice No.: " + InvoiceNo + "<br><br>";
                    TelegramComment += "Start Date: " + StartDate + "<br>";
                    TelegramComment += "End Date: " + EndDate + "<br>";
                    //TelegramComment += "<b>Payment Due Date: " + PaymentDueDate + "</b><br>";
                    TelegramComment += "<b>ATT Name: " + ATTName + "</b><br>";
                }
                TelegramComment += "<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO SEND NOTIFICATION. " + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("dd/MM/yyyy hh:mm:ss tt");
                await notificationTelegram.NotificationTelegramBot(TelegramComment.Replace("<br>", "\n"));
                await notificationTelegram.NotificationTelegramBotToTopic(TelegramComment.Replace("<br>", "\n"), -1545132903, 516);//635916546 = test send finance group
            }
            #endregion

            #region send notification to telegram when customer services will be expire in next 7days
            //send notification to telegram when customer services will be expire in next 7days
            sql = @"select distinct ta.RouterID, ta.PPPoEUserName, ta.PPPoEPassword, ta.BillingInvoiceID, ta.ActivateCustomerID, c.CustomerCode CID, COALESCE(NULLIF(ISNULL(c.CustomerName, ''), ''), c.CustomerNameLatin) CName, ta.ConnectionID, ta.SID, bf.CompanyContactMobileNumber,
(c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM +
case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName, ta.VLanName, ta.VLanIPAddress, ta.IPAddress CustomerIPAddress, ta.GetwayIP, ta.TicketNumber,
format(b.StartDate, 'dd-MM-yyyy') StartDate, format(b.EndDate, 'dd-MM-yyyy') EndDate, b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate
, ta.ConnectionType, ta.VLanID, c.ATTName from dbo.TblActivateCustomer ta
inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID
inner join dbo.TblServices s on s.ServiceID = ta.ServiceID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ta.PackageDetailID
inner join dbo.TblCustomerBillingInvoice b on b.ActivateCustomerID = ta.ActivateCustomerID
inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = b.CustomerBillInformationID
where ta.ConnectivityStatus = 40 and b.StatusID not in(12,13,14,20) and 
format(dateadd(d,-7, b.EndDate), 'yyyyMMdd') = format(dateadd(d,-7, ta.EndDate), 'yyyyMMdd') and
format(dateadd(d,-7, ta.EndDate), 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')
order by c.CustomerCode";
            //para = new Dictionary<string, string>();
            //para.Add("DateNow", easternTimeNow.ToString("yyyyMMdd"));
            ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count == 0)
            {
                sql = @"select b.RouterID, c.CustomerCode CID, COALESCE(NULLIF(ISNULL(c.CustomerName, ''), ''), c.CustomerNameLatin) CName, b.ConnectionID, b.[SID], 
bf.CompanyContactMobileNumber,
(c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + 
convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM +
case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName,
b.TicketNumber, format(b.StartDate, 'dd-MM-yyyy') StartDate, format(b.EndDate, 'dd-MM-yyyy') EndDate, 
b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, c.ATTName
from dbo.TblCustomer c
inner join dbo.TblCustomerBillingInvoice b on b.CustomerID = c.CustomerID 
inner join dbo.TblServices s on s.ServiceID = b.ServiceID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = b.PackageDetailID
inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = b.CustomerBillInformationID
where b.StatusID not in(12,13) and format(dateadd(d,-7,b.EndDate), 'yyyyMMdd') = @DateNow";
                ds = html.LoadData(sql, false, para);
            }

            if (ds.Tables[0].Rows.Count > 0)
            {
                string TelegramComment = "Dear Team, <br><br>";
                TelegramComment += "Please check list below that is customer sevices will be expired in next 7 days.";
                int i = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    i++;
                    string ATTName = dr["ATTName"].ToString();
                    string ConnectionID = dr["ConnectionID"].ToString();
                    string SID = dr["SID"].ToString();
                    string CID = dr["CID"].ToString();
                    string CName = dr["CName"].ToString();
                    string CompanyContactMobileNumber = dr["CompanyContactMobileNumber"].ToString();
                    //string ConnectionType = dr["ConnectionType"].ToString();
                    //string PPPoEUserName = dr["PPPoEUserName"].ToString();
                    //string PPPoEPassword = dr["PPPoEPassword"].ToString();
                    //string VLanName = dr["VLanName"].ToString();
                    //string VLanIPAddress = dr["VLanIPAddress"].ToString();
                    //string CustomerIPAddress = dr["CustomerIPAddress"].ToString();
                    //string GetwayIP = dr["GetwayIP"].ToString();
                    string TicketNumber = dr["TicketNumber"].ToString();
                    //string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                    //string Email = dr["Email"].ToString();
                    string CustomerName = dr["CustomerName"].ToString();
                    string ServiceName = dr["ServiceName"].ToString();
                    string PackageName = dr["PackageName"].ToString();
                    string StartDate = dr["StartDate"].ToString();
                    string EndDate = dr["EndDate"].ToString();
                    string Amount = dr["Amount"].ToString();
                    string InvoiceNo = dr["InvoiceNo"].ToString();
                    string RouterID = dr["RouterID"].ToString();
                    string PaymentDueDate = dr["PaymentDueDate"].ToString();
                    //string VLanID = dr["VLanID"].ToString();
                    //string ActivateCustomerID = dr["ActivateCustomerID"].ToString();

                    TelegramComment += "<br><br><b>" + i + ". CID: " + CID + "</b><br>";
                    TelegramComment += "Customer Name: " + CName + "<br>";
                    TelegramComment += "<b>Phone: " + CompanyContactMobileNumber + "</b><br>";
                    TelegramComment += "<b>Connection ID: " + ConnectionID + "</b><br>";
                    TelegramComment += "<b>SID: " + SID + "</b><br><br>";
                    TelegramComment += "<b>Package Name: " + PackageName + "</b><br>";
                    //TelegramComment += "Amount USD: " + Amount + "<br>";
                    //TelegramComment += "Invoice No.: " + InvoiceNo + "<br><br>";
                    TelegramComment += "Start Date: " + StartDate + "<br>";
                    TelegramComment += "End Date: <b>" + EndDate + "</b><br>";
                    TelegramComment += "ATT Name: <b>" + ATTName + "</b><br>";
                    //TelegramComment += "<b>Payment Due Date: " + PaymentDueDate + "</b><br>";
                }
                TelegramComment += "<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO SEND NOTIFICATION. " + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("dd/MM/yyyy hh:mm:ss tt");
                await notificationTelegram.NotificationTelegramBotToTopic(TelegramComment.Replace("<br>", "\n"), -1545132903, 516);//635916546 = test send finance group
            }
            #endregion

            #region send notification to telegram when generated invoice is done.            
            sql = @"select c.CustomerCode CID, COALESCE(NULLIF(ISNULL(c.CustomerName, ''), ''), c.CustomerNameLatin) CName, b.ConnectionID, b.SID, bf.CompanyContactMobileNumber,
            (c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + 
            ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM +
            case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + 
            ' Month(s)' else '' end PackageName, 
            format(b.StartDate, 'dd-MM-yyyy') StartDate, format(b.EndDate, 'dd-MM-yyyy') EndDate, 
            b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, b.TicketNumber
            from TblCustomerBillingInvoice b
            inner join dbo.TblCustomer c on c.CustomerID = b.CustomerID
            inner join dbo.TblServices s on s.ServiceID = b.ServiceID
            inner join dbo.TblPackageDetail pd on pd.PackageDetailID = b.PackageDetailID
            inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = b.CustomerBillInformationID
            where b.StatusID not in(12,13) and 
            b.IsGenerated = 1 and format(b.GeneratedDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')";
            //para = new Dictionary<string, string>();
            //para.Add("DateNow", easternTimeNow.ToString("yyyyMMdd"));
            ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string TelegramComment = "Dear Team, <br><br>";
                TelegramComment += "Please check generated invoice lists before 7 days.";
                int i = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    i++;
                    string ConnectionID = dr["ConnectionID"].ToString();
                    string SID = dr["SID"].ToString();
                    string CID = dr["CID"].ToString();
                    string CName = dr["CName"].ToString();
                    string CompanyContactMobileNumber = dr["CompanyContactMobileNumber"].ToString();
                    //string ConnectionType = dr["ConnectionType"].ToString();
                    //string PPPoEUserName = dr["PPPoEUserName"].ToString();
                    //string PPPoEPassword = dr["PPPoEPassword"].ToString();
                    //string VLanName = dr["VLanName"].ToString();
                    //string VLanIPAddress = dr["VLanIPAddress"].ToString();
                    //string CustomerIPAddress = dr["CustomerIPAddress"].ToString();
                    //string GetwayIP = dr["GetwayIP"].ToString();
                    string TicketNumber = dr["TicketNumber"].ToString();
                    //string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                    //string Email = dr["Email"].ToString();
                    string CustomerName = dr["CustomerName"].ToString();
                    string ServiceName = dr["ServiceName"].ToString();
                    string PackageName = dr["PackageName"].ToString();
                    string StartDate = dr["StartDate"].ToString();
                    string EndDate = dr["EndDate"].ToString();
                    string Amount = dr["Amount"].ToString();
                    string InvoiceNo = dr["InvoiceNo"].ToString();
                    //string RouterID = dr["RouterID"].ToString();
                    string PaymentDueDate = dr["PaymentDueDate"].ToString();
                    //string VLanID = dr["VLanID"].ToString();
                    //string ActivateCustomerID = dr["ActivateCustomerID"].ToString();

                    TelegramComment += "<br><br><b>" + i + ". SID: " + SID + "</b><br>";
                    TelegramComment += "<b>Connection ID: " + ConnectionID + "</b><br>";
                    TelegramComment += "<b>CID: " + CID + "</b><br>";
                    TelegramComment += "Customer Name: " + CName + "<br>";
                    TelegramComment += "<b>Phone: " + CompanyContactMobileNumber + "</b><br><br>";
                    TelegramComment += "<b>Package Name: " + PackageName + "</b><br>";
                    TelegramComment += "<b>Amount USD: " + Amount + "</b><br>";
                    TelegramComment += "<b>Invoice No.: " + InvoiceNo + "</b><br><br>";
                    TelegramComment += "Start Date: " + StartDate + "<br>";
                    TelegramComment += "End Date: <b>" + EndDate + "</b><br>";
                    TelegramComment += "<b>Payment Due Date: " + PaymentDueDate + "</b><br>";
                }
                TelegramComment += "<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO SEND NOTIFICATION. " + DateTime.Parse(html.GetCurrentDate().ToString()).ToString("dd/MM/yyyy hh:mm:ss tt");
                await notificationTelegram.NotificationTelegramBot(TelegramComment.Replace("<br>", "\n"), -635916546);//635916546 = test send finance group
            }
            #endregion
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }


        [Route("api/NotificationTelegram/NotificationNewCustomer")]

        [HttpPost]
        public async Task<HttpResponseMessage> NotificationNewCustomer()
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            StringBuilder sb = new StringBuilder();
            //send notification for new customer register.
            string sql = @"select c.CustomerName + ' [' + c.CustomerCode + ']' CustomerName, format(c.RegisterDate, 'dd-MMM-yyyy') RegisterDate,
c.RegisterBy, case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end PackageName,
bi.UM, bi.FreeDuration, bi.DiscountUSD, bi.Amount, sp.[Status], s.[Status] Connectivity, 
format(bi.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate,
bi.InvoiceNo, bi.ServiceID, sr.ServiceName,
format(bi.StartDate, 'dd-MMM-yyyy') StartDate, format(bi.EndDate, 'dd-MMM-yyyy') EndDate
from dbo.TblCustomer c
inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
inner join dbo.TblStatus sp on sp.StatusID = bi.StatusID
inner join dbo.TblServices sr on sr.ServiceID = bi.ServiceID
where bi.[Type] = 'Official Invoice' and  bi.StatusID not in(12,13) and format(c.RegisterDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')";
            DataSet ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string ServiceID2 = "";
                string Subject = "New Customer Register for ";
                int i = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    i++;
                    string ServiceName = dr["ServiceName"].ToString();
                    string CustomerName = dr["CustomerName"].ToString();
                    string RegisterDate = dr["RegisterDate"].ToString();
                    string RegisterBy = dr["RegisterBy"].ToString();
                    string PackageName = dr["PackageName"].ToString();
                    string UM = dr["UM"].ToString();
                    string FreeDuration = dr["FreeDuration"].ToString();
                    string DiscountUSD = dr["DiscountUSD"].ToString();
                    string Amount = dr["Amount"].ToString();
                    string Status = dr["Status"].ToString();
                    string Connectivity = dr["Connectivity"].ToString();
                    string InvoiceDate = dr["InvoiceDate"].ToString();
                    string InvoiceNo = dr["InvoiceNo"].ToString();
                    string StartDate = dr["StartDate"].ToString();
                    string EndDate = dr["EndDate"].ToString();
                    string ServiceID = dr["ServiceID"].ToString();
                    if (ServiceID != ServiceID2)
                    {
                        Subject += ServiceName + ".";
                        sb.AppendLine(Subject.ToUpper());
                        sb.AppendLine("--------------------------------------------------------");
                        sb.AppendLine("\nDear TEAM,");
                        sb.AppendLine("\nPlease check list below that is new customer register.\n");
                    }
                    ServiceID2 = dr["ServiceID"].ToString();
                    sb.AppendLine(i + ". Customer Name: " + CustomerName);
                    sb.AppendLine("Register Date: " + RegisterDate);
                    sb.AppendLine("Register By: " + RegisterBy);
                    sb.AppendLine("Service Plan: " + PackageName);
                    sb.AppendLine("Start Date: " + StartDate);
                    sb.AppendLine("End Date: " + EndDate);
                    sb.AppendLine("Discount USD: " + DiscountUSD);
                    sb.AppendLine("Amount USD: " + Amount);
                    sb.AppendLine("Status: " + Status);
                    sb.AppendLine("Connectivity: " + Connectivity);
                    sb.AppendLine("Invoice Date: " + InvoiceDate);
                    sb.AppendLine("InvoiceNo: " + InvoiceNo);
                    sb.AppendLine("\n--------------------------------------------------------");
                }
                sqlResult = await notificationTelegram.NotificationTelegramBot(sb.ToString());
                html.SendEmailToBillingAndTech(Subject, "TEAM", "", sb.ToString());
            }
            sb.Clear();
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }

        [Route("api/NotificationTelegram/NotificationToCustomer")]

        [HttpPost]
        public async Task<HttpResponseMessage> NotificationToCustomer()
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult
            {
                Result = ResultType.Successful
            };
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            Dictionary<string, string> para = new Dictionary<string, string>();
            DataSet ds = new DataSet();

            //send notification to telegram when customer services will be expire in next 7days
            string sql = "select IsEnable from TblTelegramNotification where IsEnable = 1 and StatusID = 46 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                sql = @"select c.CustomerID, ta.RouterID, ta.PPPoEUserName, ta.PPPoEPassword, ta.BillingInvoiceID, ta.ActivateCustomerID, c.CustomerCode CID, 
CASE WHEN ISNULL(c.CustomerName, '') <> '' AND c.CustomerNationality = N'ខ្មែរ' THEN c.CustomerName WHEN ISNULL(c.CustomerNameLatin, '') = '' THEN c.CustomerName ELSE c.CustomerNameLatin END AS CName, ta.ConnectionID, ta.SID, bf.CompanyContactMobileNumber,
(c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM +
case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName, ta.VLanName, ta.VLanIPAddress, ta.IPAddress CustomerIPAddress, ta.GetwayIP, ta.TicketNumber,
format(b.StartDate, 'dd-MM-yyyy') StartDate, format(b.EndDate, 'dd-MM-yyyy') EndDate, b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate
, ta.ConnectionType, ta.VLanID, c.CustomerNationality, bf.ATTName from dbo.TblActivateCustomer ta
inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID
inner join dbo.TblServices s on s.ServiceID = ta.ServiceID
inner join dbo.TblCustomerBillingInvoice b on b.ActivateCustomerID = ta.ActivateCustomerID 
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = b.PackageDetailID
inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = b.CustomerBillInformationID
where b.Type = 'Official Invoice' and ta.ConnectivityStatus = 40 and b.StatusID not in(12,13,14,20) and b.IsRenew = 0 and format(dateadd(d,-7, b.EndDate), 'yyyyMMdd') = @DateNow";
                para = new Dictionary<string, string>
                {
                    { "DateNow", easternTimeNow.ToString("yyyyMMdd") }
                };
                ds = html.LoadData(sql, false, para);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        string CustomerNationality = dr["CustomerNationality"].ToString();
                        string CustomerID = dr["CustomerID"].ToString();
                        string ConnectionID = dr["ConnectionID"].ToString();
                        string SID = dr["SID"].ToString();
                        string CID = dr["CID"].ToString();
                        string CName = dr["CName"].ToString();
                        string CompanyContactMobileNumber = dr["CompanyContactMobileNumber"].ToString();
                        //string ConnectionType = dr["ConnectionType"].ToString();
                        //string PPPoEUserName = dr["PPPoEUserName"].ToString();
                        //string PPPoEPassword = dr["PPPoEPassword"].ToString();
                        //string VLanName = dr["VLanName"].ToString();
                        //string VLanIPAddress = dr["VLanIPAddress"].ToString();
                        //string CustomerIPAddress = dr["CustomerIPAddress"].ToString();
                        //string GetwayIP = dr["GetwayIP"].ToString();
                        string TicketNumber = dr["TicketNumber"].ToString();
                        //string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                        //string Email = dr["Email"].ToString();
                        string CustomerName = dr["CustomerName"].ToString();
                        string ServiceName = dr["ServiceName"].ToString();
                        string PackageName = dr["PackageName"].ToString();
                        string StartDate = dr["StartDate"].ToString();
                        string EndDate = dr["EndDate"].ToString();
                        string Amount = dr["Amount"].ToString();
                        string InvoiceNo = dr["InvoiceNo"].ToString();
                        string RouterID = dr["RouterID"].ToString();
                        string PaymentDueDate = dr["PaymentDueDate"].ToString();
                        //string VLanID = dr["VLanID"].ToString();
                        string ActivateCustomerID = dr["ActivateCustomerID"].ToString();
                        string ATTName = dr["ATTName"].ToString();


                        string msg = "";
                        string cleanNationality = Regex.Replace(CustomerNationality ?? "", @"[\u200B\u200C\u200D\uFEFF\s]+", "").ToLower();
                        if (cleanNationality == "khmer" || cleanNationality == "ខ្មែរ")
                        {
                            msg = "«សេចក្តីជូនដំណឹងអំពី អ្នកជិតផុតសុពលភាពនៃការប្រើប្រាស់កញ្ចប់សេវាកម្មអ៊ិនធឺណិត»\n\n";
                            msg += "លេខសំគាល់អតិថិជន: " + CID + "\n";
                            msg += "ឈ្មោះអតិថិជន: " + CName + "\n";
                            msg += "ឈ្មោះកញ្ចប់សេវាកម្មៈ <b>" + PackageName + "</b>\n\n";
                            msg += "ថ្ងៃចាប់ផ្តើមៈ <b>" + StartDate + "</b>\n";
                            msg += "ថ្ងៃផុតសុពលភាពៈ <b>" + EndDate + "</b>\n";
                            msg += "ចំនួនថ្ងៃនៅសល់ៈ <b>7 ថ្ងៃ</b>\n\n";
                            if (ATTName != "")
                                msg += "ATT Name: <b>" + ATTName + "</b>\n\n";
                            msg += "ដើម្បីជៀសវាងការផ្អាកសេវាកម្មអ៊ិនធឺណិត សូមអតិថិជនអញ្ជើញបង់ប្រាក់អោយបានមុនថ្ងៃផុតកំណត់។\n\n";
                            //msg += "<b>ចំណាំ៖</b> ការបង់ប្រាក់ថ្លៃសេវាកម្មអ៊ិនធឺណិត អាចតាមរយៈធនាគារ <b>ABA</b>។\n";
                            //msg += "<b>ឈ្មោះគណនីៈ \"NSM SOLUTION AND MAINTENANCE CO., LTD\"</b> \n";
                            //msg += "<b>លេខគណនីៈ \"000504499\"</b> \n";
                            msg += "សូមមេត្តាបង់ប្រាក់ជាដុល្លារ (USD)។\n\n";
                            msg += "សម្រាប់ពត៌មានបន្ថែមសូមទាក់ទង\nផ្នែកបម្រើអតិថិជន: +85586588777, +85578588777\n";
                            msg += "ផ្នែកបច្ចេកទេសៈ +85561876777\n";
                            msg += "គេហទំព័រ: www.nsmisp.com.kh\n";
                            msg += "\nសូមអរគុណ!";
                        }
                        else
                        {
                            msg = "«Notification of expiration of Internet Services»\n\n";
                            msg += "CID: " + CID + "\n";
                            msg += "Customer Name: " + CName + "\n";
                            msg += "Package Name: <b>" + PackageName + "</b>\n\n";
                            msg += "Start Date: <b>" + StartDate + "</b>\n";
                            msg += "End Date <b>" + EndDate + "</b>\n";
                            msg += "Number of days left: <b>7 Days</b>\n\n";
                            if (ATTName != "")
                                msg += "ATT Name: <b>" + ATTName + "</b>\n\n";

                            msg += "Please pay before the due date. In case you are of failure to pay not on time, our company will temporarily suspend the use of the service.\n";
                            msg += "The service will be reconnected after payment.\n\n";
                            msg += "Please make the payment in US dollars.\n\n";
                            msg += "Contact Information\nCustomer Services: +85586588777, +85578588777\n";
                            msg += "Technical: +85561876777\n";
                            msg += "Website: www.nsmisp.com.kh\n";
                            msg += "\nThank you!";
                        }
                        CompanyContactMobileNumber = CompanyContactMobileNumber.Trim().Replace(" ", string.Empty);
                        var regex = new Regex(Regex.Escape("0"));
                        CompanyContactMobileNumber = regex.Replace(CompanyContactMobileNumber, "+855", 1);
                        //await notificationTelegram.NotificationToCustomer(CompanyContactMobileNumber, CustomerName, CID, int.Parse(CustomerID), msg, "", "", InvoiceNo, "Customer Services Will Be Expire In Next 7days");
                        string TelegramNotificationID = "7";
                        sql = @"select ct.TelegramNotificationID, ct.CustomerPhone, ct.FirstName, ct.LastName, ct.CustomerID
from dbo.TblCustomerTelegram ct
where isnull(ct.TelegramNotificationID,'') <> ''
and " + TelegramNotificationID + @" = (select [Data] from dbo.Split(ct.TelegramNotificationID, ',') where [Data] = " + TelegramNotificationID + @")
and ct.CustomerID = " + CustomerID + " and ct.IsUsingTelegram = 'Yes'";
                        ds = html.LoadData(sql);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                foreach (DataRow drs in ds.Tables[0].Rows)
                                {
                                    string CustPhone = drs["CustomerPhone"].ToString();
                                    string CustFirstName = drs["FirstName"].ToString();
                                    string CustLastName = drs["LastName"].ToString();
                                    string CustID = drs["CustomerID"].ToString();
                                    SqlResult result = await notificationTelegram.NotificationToCustomer(CustPhone, CustFirstName, CustLastName, int.Parse(CustID), msg);
                                    sql = @"insert into dbo.TblCustomerTelegramMessage (DateSend, SendBy, CustomerID, ConnectionID, 
ActivateCustomerID, TelegramNotificationID, MessgeID, MessageText, MessageStatus, CustomerPhone)
values ('" + html.GetCurrentDate() + "','System', " + CustomerID + ",'" + ConnectionID + "'," + ActivateCustomerID + "," + TelegramNotificationID + ",'" + result.Value + "'," +
"N'" + msg + "', '" + result.Message + "', '" + CustPhone + "')";
                                    result = html.SaveChangeData(sql, false, false);
                                }
                            }
                        }
                    }
                }
            }
            //end send notification to telegram when customer services will be expire in next 7days

            //send notification to telegram when customer services will be lock in next 3days
            sql = "select IsEnable from TblTelegramNotification where IsEnable = 1 and StatusID = 45 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                sql = @"select c.CustomerID, ta.RouterID, ta.PPPoEUserName, ta.PPPoEPassword, ta.BillingInvoiceID, ta.ActivateCustomerID, c.CustomerCode CID, CASE WHEN ISNULL(c.CustomerName, '') <> '' AND c.CustomerNationality = N'ខ្មែរ' THEN c.CustomerName WHEN ISNULL(c.CustomerNameLatin, '') = '' THEN c.CustomerName ELSE c.CustomerNameLatin END AS CName, ta.ConnectionID, ta.SID, bf.CompanyContactMobileNumber,
(COALESCE(NULLIF(c.CustomerName, ''), c.CustomerNameLatin) + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM +
case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName, ta.VLanName, ta.VLanIPAddress, ta.IPAddress CustomerIPAddress, ta.GetwayIP, ta.TicketNumber,
format(b.StartDate, 'dd-MM-yyyy') StartDate, format(b.EndDate, 'dd-MM-yyyy') EndDate, b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate
, ta.ConnectionType, ta.VLanID, c.CustomerNationality, bf.ATTName from dbo.TblActivateCustomer ta
inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID
inner join dbo.TblServices s on s.ServiceID = ta.ServiceID
inner join dbo.TblCustomerBillingInvoice b on b.ActivateCustomerID = ta.ActivateCustomerID 
inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = b.CustomerBillInformationID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = b.PackageDetailID
where b.Type = 'Official Invoice' and ta.ConnectivityStatus = 40 and b.StatusID not in(12,13,14,20)
and b.IsRenew = 0
and format(dateadd(d,-3, b.EndDate), 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')
order by c.CustomerCode";
                ds = html.LoadData(sql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        string ATTName = dr["ATTName"].ToString();
                        string CustomerNationality = dr["CustomerNationality"].ToString();
                        string CustomerID = dr["CustomerID"].ToString();
                        string ConnectionID = dr["ConnectionID"].ToString();
                        string SID = dr["SID"].ToString();
                        string CID = dr["CID"].ToString();
                        string CName = dr["CName"].ToString();
                        string CompanyContactMobileNumber = dr["CompanyContactMobileNumber"].ToString();
                        //string ConnectionType = dr["ConnectionType"].ToString();
                        //string PPPoEUserName = dr["PPPoEUserName"].ToString();
                        //string PPPoEPassword = dr["PPPoEPassword"].ToString();
                        //string VLanName = dr["VLanName"].ToString();
                        //string VLanIPAddress = dr["VLanIPAddress"].ToString();
                        //string CustomerIPAddress = dr["CustomerIPAddress"].ToString();
                        //string GetwayIP = dr["GetwayIP"].ToString();
                        string TicketNumber = dr["TicketNumber"].ToString();
                        //string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                        //string Email = dr["Email"].ToString();
                        string CustomerName = dr["CustomerName"].ToString();
                        string ServiceName = dr["ServiceName"].ToString();
                        string PackageName = dr["PackageName"].ToString();
                        string StartDate = dr["StartDate"].ToString();
                        string EndDate = dr["EndDate"].ToString();
                        string Amount = dr["Amount"].ToString();
                        string InvoiceNo = dr["InvoiceNo"].ToString();
                        string RouterID = dr["RouterID"].ToString();
                        string PaymentDueDate = dr["PaymentDueDate"].ToString();
                        //string VLanID = dr["VLanID"].ToString();
                        string ActivateCustomerID = dr["ActivateCustomerID"].ToString();
                        string msg = "";
                        string cleanNationality = Regex.Replace(CustomerNationality ?? "", @"[\u200B\u200C\u200D\uFEFF\s]+", "").ToLower();
                        if (cleanNationality == "khmer" || cleanNationality == "ខ្មែរ")
                        {
                            msg = "«សេចក្តីជូនដំណឹងអំពី អ្នកនឹងត្រូវផុតសុពលភាពការប្រើប្រាស់សេវាកម្មអ៊ិនធឺណិត»\n\n";
                            msg += "លេខសំគាល់អតិថិជន: " + CID + "\n";
                            msg += "ឈ្មោះអតិថិជន: " + CName + "\n";
                            msg += "ឈ្មោះកញ្ចប់សេវាកម្មៈ <b>" + PackageName + "</b>\n\n";
                            msg += "ថ្ងៃចាប់ផ្តើមៈ <b>" + StartDate + "</b>\n";
                            msg += "ថ្ងៃផុតសុពលភាពៈ <b>" + EndDate + "</b>\n";
                            msg += "ចំនួនថ្ងៃនៅសល់ៈ <b>3 ថ្ងៃ</b>\n\n";
                            if (ATTName != "")
                                msg += "ATT Name: <b>" + ATTName + "</b>\n\n";
                            msg += "ដើម្បីជៀសវាងការផ្អាកសេវាកម្មអ៊ិនធឺណិត សូមអតិថិជនអញ្ជើញបង់ប្រាក់អោយបានមុនថ្ងៃផុតកំណត់។\n\n";
                            msg += "សូមមេត្តាបង់ប្រាក់ជាដុល្លារ (USD)។\n\n";
                            //msg += "<b>ចំណាំ៖</b> ការបង់ប្រាក់ថ្លៃសេវាកម្មអ៊ិនធឺណិត អាចតាមរយៈធនាគារ <b>ABA</b>។\n";
                            //msg += "<b>ឈ្មោះគណនីៈ \"NSM SOLUTION AND MAINTENANCE CO., LTD\"</b> \n";
                            //msg += "<b>លេខគណនីៈ \"000504499\"</b> \n";
                            msg += "\nសម្រាប់ពត៌មានបន្ថែមសូមទាក់ទង\nផ្នែកបម្រើអតិថិជន: +85586588777, +85578588777\n";
                            msg += "ផ្នែកបច្ចេកទេសៈ +85561876777\n";
                            msg += "គេហទំព័រ: www.nsmisp.com.kh\n";
                            msg += "\nសូមអរគុណ!";
                        }
                        else
                        {
                            msg = "«Notice that you will have to suspend your Internet service for the next 3 days»\n\n";
                            msg += "CID: " + CID + "\n";
                            msg += "Customer Name: " + CName + "\n";
                            msg += "package Name: <b>" + PackageName + "</b>\n\n";
                            msg += "Start Date: <b>" + StartDate + "</b>\n";
                            msg += "End Date: <b>" + EndDate + "</b>\n";
                            msg += "Number of days left: <b>3 Days</b>\n\n";
                            if (ATTName != "")
                                msg += "ATT Name: <b>" + ATTName + "</b>\n\n";
                            msg += "Please pay before the due date. In case you are of failure to pay not on time, our company will temporarily suspend the use of the service.\n";
                            msg += "The service will be reconnected after payment.\n\n";
                            msg += "Please make the payment in US dollars.\n\n";
                            msg += "Contact Information\nCustomer Services: +85586588777, +85578588777\n";
                            msg += "Technical: +85561876777\n";
                            msg += "Website: www.nsmisp.com.kh\n";
                            msg += "\nThank you!";
                        }
                        CompanyContactMobileNumber = CompanyContactMobileNumber.Trim().Replace(" ", string.Empty);
                        var regex = new Regex(Regex.Escape("0"));
                        CompanyContactMobileNumber = regex.Replace(CompanyContactMobileNumber, "+855", 1);
                        //await notificationTelegram.NotificationToCustomer(CompanyContactMobileNumber, CustomerName, CID, int.Parse(CustomerID), msg, "", "", InvoiceNo, "Customer Services Will Be Lock In Next 3days");
                        string TelegramNotificationID = "3";
                        sql = @"select ct.TelegramNotificationID, ct.CustomerPhone, ct.FirstName, ct.LastName, ct.CustomerID
from dbo.TblCustomerTelegram ct
where isnull(ct.TelegramNotificationID,'') <> ''
and " + TelegramNotificationID + @" = (select [Data] from dbo.Split(ct.TelegramNotificationID, ',') where [Data] = " + TelegramNotificationID + @")
and ct.CustomerID = " + CustomerID + " and ct.IsUsingTelegram = 'Yes'";
                        ds = html.LoadData(sql);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                foreach (DataRow drs in ds.Tables[0].Rows)
                                {
                                    string CustPhone = drs["CustomerPhone"].ToString();
                                    string CustFirstName = drs["FirstName"].ToString();
                                    string CustLastName = drs["LastName"].ToString();
                                    string CustID = drs["CustomerID"].ToString();
                                    SqlResult result = await notificationTelegram.NotificationToCustomer(CustPhone, CustFirstName, CustLastName, int.Parse(CustID), msg);
                                    sql = @"insert into dbo.TblCustomerTelegramMessage (DateSend, SendBy, CustomerID, ConnectionID, 
ActivateCustomerID, TelegramNotificationID, MessgeID, MessageText, MessageStatus, CustomerPhone)
values ('" + html.GetCurrentDate() + "','System', " + CustomerID + ",'" + ConnectionID + "'," + ActivateCustomerID + "," + TelegramNotificationID + ",'" + result.Value + "'," +
"N'" + msg + "', '" + result.Message + "', '" + CustPhone + "')";
                                    result = html.SaveChangeData(sql, false, false);
                                }
                            }
                        }
                    }
                }
            }
            //end send notification to telegram when customer services will be lock in next 3days

            //send notification to telegram when customer services is locked
            sql = "select IsEnable from TblTelegramNotification where IsEnable = 1 and StatusID = 41 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                sql = @"select c.CustomerID, ta.RouterID, ta.PPPoEUserName, ta.PPPoEPassword, ta.BillingInvoiceID, ta.ActivateCustomerID, c.CustomerCode CID, CASE WHEN ISNULL(c.CustomerName, '') <> '' AND c.CustomerNationality = N'ខ្មែរ' THEN c.CustomerName WHEN ISNULL(c.CustomerNameLatin, '') = '' THEN c.CustomerName ELSE c.CustomerNameLatin END AS CName, ta.ConnectionID, ta.SID, bf.CompanyContactMobileNumber,
(c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM +
case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName, ta.VLanName, ta.VLanIPAddress, ta.IPAddress CustomerIPAddress, ta.GetwayIP, ta.TicketNumber,
format(b.StartDate, 'dd-MM-yyyy') StartDate, format(b.EndDate, 'dd-MM-yyyy') EndDate, b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate
, ta.ConnectionType, ta.VLanID, c.CustomerNationality, bf.ATTName from dbo.TblActivateCustomer ta
inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID
inner join dbo.TblServices s on s.ServiceID = ta.ServiceID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ta.PackageDetailID
inner join dbo.TblCustomerBillingInvoice b on b.ActivateCustomerID = ta.ActivateCustomerID 
inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = b.CustomerBillInformationID
where ta.ConnectivityStatus = 41 and b.StatusID not in(12,13,14,20)
and ta.InvoiceNo = b.InvoiceNo
and format(ta.LockedDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')
order by c.CustomerCode";
                ds = html.LoadData(sql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        string ATTName = dr["ATTName"].ToString();
                        string CustomerNationality = dr["CustomerNationality"].ToString();
                        string CustomerID = dr["CustomerID"].ToString();
                        string ConnectionID = dr["ConnectionID"].ToString();
                        string SID = dr["SID"].ToString();
                        string CID = dr["CID"].ToString();
                        string CName = dr["CName"].ToString();
                        string CompanyContactMobileNumber = dr["CompanyContactMobileNumber"].ToString();
                        string TicketNumber = dr["TicketNumber"].ToString();
                        string CustomerName = dr["CustomerName"].ToString();
                        string ServiceName = dr["ServiceName"].ToString();
                        string PackageName = dr["PackageName"].ToString();
                        string StartDate = dr["StartDate"].ToString();
                        string EndDate = dr["EndDate"].ToString();
                        string Amount = dr["Amount"].ToString();
                        string InvoiceNo = dr["InvoiceNo"].ToString();
                        string RouterID = dr["RouterID"].ToString();
                        string PaymentDueDate = dr["PaymentDueDate"].ToString();

                        string msg = "";
                        string cleanNationality = Regex.Replace(CustomerNationality ?? "", @"[\u200B\u200C\u200D\uFEFF\s]+", "").ToLower();
                        if (cleanNationality == "khmer" || cleanNationality == "ខ្មែរ")
                        {
                            msg = "«សេចក្តីជូនដំណឹងអំពី អ្នកត្រូវផ្អាកការប្រើប្រាស់សេវាកម្មអ៊ិនធឺណិត»\n\n";
                            msg += "លេខសំគាល់អតិថិជន: " + CID + "\n";
                            msg += "ឈ្មោះអតិថិជន: " + CName + "\n";
                            msg += "ឈ្មោះកញ្ចប់សេវាកម្មៈ <b>" + PackageName + "</b>\n\n";
                            msg += "ថ្ងៃចាប់ផ្តើមៈ <b>" + StartDate + "</b>\n";
                            msg += "ថ្ងៃផុតសុពលភាពៈ <b>" + EndDate + "</b>\n";
                            msg += "ចំនួនថ្ងៃនៅសល់ៈ <b>0 ថ្ងៃ</b>\n\n";
                            if (ATTName != "")
                                msg += "ATT Name: <b>" + ATTName + "</b>\n\n";
                            //msg += "ដើម្បីធ្វើការតភ្ជាប់សេវាកម្មអ៊ិនធឺណិតឡើងវិញ សូមអតិថិជនអញ្ជើញមកបង់ប្រាក់នៅ\n";
                            //msg += "អាសយដ្ឋាន៖ #1159 អាគារ១ ផ្លូវជាតិលេខ២ ភូមិព្រែកតានូ សង្កាត់ចាក់អង្រែកលើ ខណ្ឌមានជ័យ រាជធានីភ្នំពេញ\n\n";
                            //msg += "<b>ឬ</b> តាមរយៈធនាគារ <b>ABA</b>\n";
                            //msg += "<b>ឈ្មោះគណនីៈ \"NSM SOLUTION AND MAINTENANCE CO., LTD\"</b> \n";
                            //msg += "<b>លេខគណនីៈ \"000504499\"</b> \n";
                            msg += "សូមមេត្តាបង់ប្រាក់ជាដុល្លារ (USD)។\n\n";
                            msg += "\nសម្រាប់ពត៌មានបន្ថែមសូមទាក់ទង\nផ្នែកបម្រើអតិថិជន: +85586588777, +85578588777\n";
                            msg += "ផ្នែកបច្ចេកទេសៈ +85561876777\n";
                            msg += "គេហទំព័រ: www.nsmisp.com.kh\n";
                            msg += "\nសូមអរគុណ!";
                        }
                        else
                        {
                            msg = "«Notice about your Internet Service is suspended»\n\n";
                            msg += "CID " + CID + "\n";
                            msg += "Customer Name: " + CName + "\n";
                            msg += "package Name: <b>" + PackageName + "</b>\n\n";
                            msg += "Start Date: <b>" + StartDate + "</b>\n";
                            msg += "End Date: <b>" + EndDate + "</b>\n";
                            msg += "Number of days left: <b>0 Day</b>\n\n";
                            if (ATTName != "")
                                msg += "ATT Name: <b>" + ATTName + "</b>\n\n";
                            //msg += "Please pay before the due date. In case you are of failure to pay not on time, our company will temporarily suspend the use of the service.\n";
                            msg += "Please make the payment in US dollars.\n\n";
                            msg += "The service will be reconnected after payment.\n\n";
                            msg += "Contact Information\nCustomer Services: +85586588777, +85578588777\n";
                            msg += "Technical: +85561876777\n";
                            msg += "Website: www.nsmisp.com.kh\n";
                            msg += "\nThank you!";
                        }

                        CompanyContactMobileNumber = CompanyContactMobileNumber.Trim().Replace(" ", string.Empty);
                        var regex = new Regex(Regex.Escape("0"));
                        CompanyContactMobileNumber = regex.Replace(CompanyContactMobileNumber, "+855", 1);
                        //await notificationTelegram.NotificationToCustomer(CompanyContactMobileNumber, CustomerName, CID, int.Parse(CustomerID), msg,"","", InvoiceNo, "Customer Services Is Locked");
                        string TelegramNotificationID = "0";
                        sql = @"select ct.TelegramNotificationID, ct.CustomerPhone, ct.FirstName, ct.LastName, ct.CustomerID
from dbo.TblCustomerTelegram ct
where isnull(ct.TelegramNotificationID,'') <> ''
and " + TelegramNotificationID + @" = (select [Data] from dbo.Split(ct.TelegramNotificationID, ',') where [Data] = " + TelegramNotificationID + @")
and ct.CustomerID = " + CustomerID + " and ct.IsUsingTelegram = 'Yes'";
                        ds = html.LoadData(sql);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                foreach (DataRow drs in ds.Tables[0].Rows)
                                {
                                    string CustPhone = drs["CustomerPhone"].ToString();
                                    string CustFirstName = drs["FirstName"].ToString();
                                    string CustLastName = drs["LastName"].ToString();
                                    string CustID = drs["CustomerID"].ToString();
                                    SqlResult result = await notificationTelegram.NotificationToCustomer(CustPhone, CustFirstName, CustLastName, int.Parse(CustID), msg);
                                    sql = @"insert into dbo.TblCustomerTelegramMessage (DateSend, SendBy, CustomerID, ConnectionID, 
ActivateCustomerID, TelegramNotificationID, MessgeID, MessageText, MessageStatus, CustomerPhone)
values ('" + html.GetCurrentDate() + "','System', " + CustomerID + ",'" + ConnectionID + "',0," + TelegramNotificationID + ",'" + result.Value + "'," +
"N'" + msg + "', '" + result.Message + "', '" + CustPhone + "')";
                                    result = html.SaveChangeData(sql, false, false);
                                }
                            }
                        }
                    }
                }
            }
            //end send notification to telegram when customer services is locked
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }

        [HttpPost]
        [Route("api/NotificationTelegram/NotificationDailyReport")]
        public async Task<HttpResponseMessage> NotificationDailyReport()
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            Dictionary<string, string> para = new Dictionary<string, string>();
            DataSet ds = new DataSet();
            StringBuilder sb = new StringBuilder();
            int i = 0;

            #region  Daily Report for New Customer
            string sql = @"select cc.ClassName, cg.CustomerGroup, c.CustomerCode CID, c.CustomerName, c.CustomerContactMobileNumber, 
isnull(bi.[SID], 'Pending') [SID], bi.ConnectionID, ss.[Status] ConnectivityStatus,  bi.InvoiceNo, s.[Status],
bi.PackageName + ' - ' + bi.UM PackageName,
format(bi.StartDate, 'dd-MM-yyyy') StartDate, format(bi.EndDate, 'dd-MM-yyyy') EndDate, bi.CreateBy, format(isnull(bi.CreateDate, bi.InvoiceDate),'dd-MM-yyyy') CreateDate,
case when bi.InvoiceType = 'Created Invoice' and isnull(bi.IsChangePlan,0) = 0 then 'New Conntion' 
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 0 then 'Rewnew'
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 1 then 'Change Plan'
end 'ConntionType'
,case when bi.InvoiceType = 'Created Invoice' and isnull(bi.IsChangePlan,0) = 0 then 0 
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 0 then 1
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 1 then 2 end ConntionTypeID, bi.CustomerID,
l.FullName SaleName
from dbo.TblCustomer c
inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblClass cc on cc.ClassID = bi.ClassNameID
inner join dbo.TblStatus s on s.StatusID = bi.StatusID
inner join dbo.TblStatus ss on ss.StatusID = bi.ConnectivityStatus
inner join dbo.TblLeads l on l.LeadID = c.EmployeeID
where format(c.RegisterDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd') and bi.StatusID not in(12,13,14,20)
and bi.InvoiceType = 'Created Invoice'
order by cg.CustomerGroup, bi.ConnectionID";
            ds = html.LoadData(sql);
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    sb.AppendLine("Daily Report For New Customer<br>_________________________________<br>");
                    sb.AppendLine("Dear Team, <br>");
                    sb.AppendLine("Today, we have new customer: " + ds.Tables[0].Rows.Count.ToString());
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        i++;
                        sb.AppendLine("<br>" + i + ". CID: " + row["CID"].ToString());
                        sb.AppendLine("Customer Name: " + row["CustomerName"].ToString());
                        sb.AppendLine("Connectivity Status: " + row["ConnectivityStatus"].ToString());
                        sb.AppendLine("Package Name: " + row["PackageName"].ToString());
                        sb.AppendLine("Invoice No: " + row["InvoiceNo"].ToString());
                        sb.AppendLine("Payment Status: " + row["Status"].ToString());
                        sb.AppendLine("Start Date: " + row["StartDate"].ToString());
                        sb.AppendLine("End Date: " + row["EndDate"].ToString() + "<br>");
                        sb.AppendLine("Create By: " + row["CreateBy"].ToString());
                        sb.AppendLine("Sale Representativ: " + row["SaleName"].ToString());
                        sb.AppendLine("_________________________________________________");
                    }
                    await notificationTelegram.NotificationTelegramBotToTopic("<pre>" + sb.ToString().Replace("<br>", "\n") + "</pre>", -1903890245, 15); // technical dialy
                    await notificationTelegram.NotificationTelegramBotToTopic("<pre>" + sb.ToString().Replace("<br>", "\n") + "</pre>", -1545132903, 6); // finance dialy
                    sb.Clear();
                }
            }
            #endregion

            #region  Daily Report for Customer Renew Connection
            sql = @"select cc.ClassName, cg.CustomerGroup, c.CustomerCode CID, c.CustomerName, c.CustomerContactMobileNumber, 
isnull(bi.[SID], 'Pending') [SID], bi.ConnectionID, ss.[Status] ConnectivityStatus,  bi.InvoiceNo, s.[Status],
bi.PackageName + ' - ' + bi.UM PackageName,
format(bi.StartDate, 'dd-MM-yyyy') StartDate, format(bi.EndDate, 'dd-MM-yyyy') EndDate, bi.CreateBy, format(isnull(bi.CreateDate, bi.InvoiceDate),'dd-MM-yyyy') CreateDate,
case when bi.InvoiceType = 'Created Invoice' and isnull(bi.IsChangePlan,0) = 0 then 'New Conntion' 
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 0 then 'Rewnew'
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 1 then 'Change Plan'
end 'ConntionType'
,case when bi.InvoiceType = 'Created Invoice' and isnull(bi.IsChangePlan,0) = 0 then 0 
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 0 then 1
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 1 then 2 end ConntionTypeID, bi.CustomerID,
l.FullName SaleName
from dbo.TblCustomer c
inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblClass cc on cc.ClassID = bi.ClassNameID
inner join dbo.TblStatus s on s.StatusID = bi.StatusID
inner join dbo.TblStatus ss on ss.StatusID = bi.ConnectivityStatus
inner join dbo.TblLeads l on l.LeadID = c.EmployeeID
where format(bi.DoneRenewDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd') and bi.StatusID not in(12,13,14,20)
and bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 0
and bi.RenewStatus = 'Done'
order by cg.CustomerGroup, bi.InvoiceNo";
            ds = html.LoadData(sql);
            i = 0;
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    sb.AppendLine("Daily Report For Customer Renew Connection<br>_________________________________<br>");
                    sb.AppendLine("Dear Team, <br>");
                    sb.AppendLine("Today, we have customer renew connection: " + ds.Tables[0].Rows.Count.ToString());
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        i++;
                        sb.AppendLine("<br>" + i + ". CID: " + row["CID"].ToString());
                        sb.AppendLine("Customer Name: " + row["CustomerName"].ToString());
                        sb.AppendLine("Connectivity Status: " + row["ConnectivityStatus"].ToString());
                        sb.AppendLine("Package Name: " + row["PackageName"].ToString());
                        sb.AppendLine("Invoice No: " + row["InvoiceNo"].ToString());
                        sb.AppendLine("Payment Status: " + row["Status"].ToString());
                        sb.AppendLine("Start Date: " + row["StartDate"].ToString());
                        sb.AppendLine("End Date: " + row["EndDate"].ToString() + "<br>");
                        sb.AppendLine("Renew By: " + row["CreateBy"].ToString());
                        sb.AppendLine("Sale Representativ: " + row["SaleName"].ToString());
                        sb.AppendLine("_________________________________________________");
                    }
                    await notificationTelegram.NotificationTelegramBotToTopic("<pre>" + sb.ToString().Replace("<br>", "\n") + "</pre>", -1903890245, 15);
                    sqlResult = await notificationTelegram.NotificationTelegramBotToTopic("<pre>" + sb.ToString().Replace("<br>", "\n") + "</pre>", -1545132903, 6);
                    sb.Clear();
                }
            }
            #endregion

            #region  Daily Report for Customer Change Plan
            sql = @"select cc.ClassName, cg.CustomerGroup, c.CustomerCode CID, c.CustomerName, c.CustomerContactMobileNumber, 
isnull(bi.[SID], 'Pending') [SID], bi.ConnectionID, ss.[Status] ConnectivityStatus,  bi.InvoiceNo, s.[Status],
bi.PackageName + ' - ' + bi.UM PackageName,
format(bi.StartDate, 'dd-MM-yyyy') StartDate, format(bi.EndDate, 'dd-MM-yyyy') EndDate, bi.CreateBy, format(isnull(bi.CreateDate, bi.InvoiceDate),'dd-MM-yyyy') CreateDate,
case when bi.InvoiceType = 'Created Invoice' and isnull(bi.IsChangePlan,0) = 0 then 'New Conntion' 
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 0 then 'Rewnew'
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 1 then 'Change Plan'
end 'ConntionType'
,case when bi.InvoiceType = 'Created Invoice' and isnull(bi.IsChangePlan,0) = 0 then 0 
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 0 then 1
when bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 1 then 2 end ConntionTypeID, bi.CustomerID,
l.FullName SaleName, hc.Comment
from dbo.TblCustomer c
inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblClass cc on cc.ClassID = bi.ClassNameID
inner join dbo.TblStatus s on s.StatusID = bi.StatusID
inner join dbo.TblStatus ss on ss.StatusID = bi.ConnectivityStatus
inner join dbo.TblLeads l on l.LeadID = c.EmployeeID
inner join dbo.TblHistoryActivateCustomer hc on hc.BillingInvoiceID = bi.BillingInvoiceID
where format(bi.CreateDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd') and bi.StatusID not in(12,13,14,20)
and bi.InvoiceType = 'Renew Invoice' and isnull(bi.IsChangePlan,0) = 1
order by cg.CustomerGroup, bi.InvoiceNo";
            ds = html.LoadData(sql);
            i = 0;
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    sb.AppendLine("Daily Report For Customer Change Connection<br>_________________________________<br>");
                    sb.AppendLine("Dear Team, <br>");
                    sb.AppendLine("Today, we have customer change connection: " + ds.Tables[0].Rows.Count.ToString());
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        i++;
                        sb.AppendLine("<br>" + i + ". CID: " + row["CID"].ToString());
                        sb.AppendLine("Customer Name: " + row["CustomerName"].ToString());
                        sb.AppendLine("Connectivity Status: " + row["ConnectivityStatus"].ToString());
                        sb.AppendLine("Package Name: " + row["PackageName"].ToString());
                        sb.AppendLine("Invoice No: " + row["InvoiceNo"].ToString());
                        sb.AppendLine("Payment Status: " + row["Status"].ToString());
                        sb.AppendLine("Start Date: " + row["StartDate"].ToString());
                        sb.AppendLine("End Date: " + row["EndDate"].ToString() + "<br>");
                        sb.AppendLine("Comment: " + row["Comment"].ToString() + "<br>");
                        sb.AppendLine("Change By: " + row["CreateBy"].ToString());
                        sb.AppendLine("Sale Representativ: " + row["SaleName"].ToString());
                        sb.AppendLine("_________________________________________________");
                    }
                    await notificationTelegram.NotificationTelegramBotToTopic("<pre>" + sb.ToString().Replace("<br>", "\n") + "</pre>", -1903890245, 15);
                    sqlResult = await notificationTelegram.NotificationTelegramBotToTopic("<pre>" + sb.ToString().Replace("<br>", "\n") + "</pre>", -1545132903, 6);
                    sb.Clear();
                }
            }
            #endregion

            #region  Daily Report for Customer Connection Expired
            sql = @"select cc.ClassName, cg.CustomerGroup, c.CustomerCode CID, c.CustomerName, c.CustomerContactMobileNumber, 
isnull(bi.[SID], 'Pending') [SID], bi.ConnectionID, ss.[Status] ConnectivityStatus, bi.InvoiceNo, s.[Status],
bi.PackageName, bi.UM,
format(bi.StartDate, 'dd-MM-yyyy') StartDate, format(bi.EndDate, 'dd-MM-yyyy') EndDate, bi.CreateBy, format(isnull(bi.CreateDate, bi.InvoiceDate),'dd-MM-yyyy') CreateDate,
'Expired' 'ConntionType'
,3 ConntionTypeID, bi.CustomerID, l.FullName SaleName
from dbo.TblCustomer c
inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblClass cc on cc.ClassID = bi.ClassNameID
inner join dbo.TblStatus s on s.StatusID = bi.StatusID
inner join dbo.TblStatus ss on ss.StatusID = bi.ConnectivityStatus
inner join dbo.TblLeads l on l.LeadID = c.EmployeeID
where format(bi.EndDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')
and bi.StatusID not in(12,13,14,20) and isnull(bi.IsRenew,0) = 0";
            ds = html.LoadData(sql);
            i = 0;
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    sb.AppendLine("Daily Report For Customer Connection Expired<br>_________________________________<br>");
                    sb.AppendLine("Dear Team, <br>");
                    sb.AppendLine("Today, we have customer connection expired: " + ds.Tables[0].Rows.Count.ToString());
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        i++;
                        sb.AppendLine("<br>" + i + ". CID: " + row["CID"].ToString());
                        sb.AppendLine("Customer Name: " + row["CustomerName"].ToString());
                        sb.AppendLine("Connectivity Status: " + row["ConnectivityStatus"].ToString());
                        sb.AppendLine("Package Name: " + row["PackageName"].ToString());
                        sb.AppendLine("Invoice No: " + row["InvoiceNo"].ToString());
                        sb.AppendLine("Payment Status: " + row["Status"].ToString());
                        sb.AppendLine("Start Date: " + row["StartDate"].ToString());
                        sb.AppendLine("End Date: " + row["EndDate"].ToString() + "<br>");
                        sb.AppendLine("Create By: " + row["CreateBy"].ToString());
                        sb.AppendLine("Sale Representativ: " + row["SaleName"].ToString());
                        sb.AppendLine("_________________________________________________");
                    }
                    await notificationTelegram.NotificationTelegramBotToTopic("<pre>" + sb.ToString().Replace("<br>", "\n") + "</pre>", -1903890245, 15);
                    sqlResult = await notificationTelegram.NotificationTelegramBotToTopic("<pre>" + sb.ToString().Replace("<br>", "\n") + "</pre>", -1545132903, 6);
                    sb.Clear();
                }
            }
            #endregion

            #region  Daily Report for Locked Customer Connection
            sql = @"select distinct cc.ClassName, cg.CustomerGroup, c.CustomerCode CID, c.CustomerName, c.CustomerContactMobileNumber, 
isnull(bi.[SID], 'Pending') [SID], bi.ConnectionID, ss.[Status] ConnectivityStatus, bi.InvoiceNo, s.[Status],
bi.PackageName, bi.UM,
format(bi.StartDate, 'dd-MM-yyyy') StartDate, format(bi.EndDate, 'dd-MM-yyyy') EndDate, bi.CreateBy, format(isnull(bi.CreateDate, bi.InvoiceDate),'dd-MM-yyyy') CreateDate,
ss.[Status] 'ConntionType',4 ConntionTypeID, bi.CustomerID, l.FullName SaleName
--,hc.Comment
from dbo.TblCustomer c
inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblClass cc on cc.ClassID = bi.ClassNameID
inner join dbo.TblStatus s on s.StatusID = bi.StatusID
inner join dbo.TblStatus ss on ss.StatusID = bi.ConnectivityStatus
inner join dbo.TblActivateCustomer ac on ac.ConnectionID = bi.ConnectionID
inner join dbo.TblLeads l on l.LeadID = c.EmployeeID
inner join dbo.TblHistoryActivateCustomer hc on hc.ActivateCustomerID = bi.ActivateCustomerID
and format(hc.[Date], 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')
where format(ac.LockedDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')
and bi.StatusID not in(12,13,14,20) and ac.ConnectivityStatus = 41
order by c.CustomerCode";
            ds = html.LoadData(sql);
            i = 0;
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    sb.AppendLine("Daily Report For Locked Customer Connection<br>_________________________________<br>");
                    sb.AppendLine("Dear Team, <br>");
                    sb.AppendLine("Today, we have locked customer connection: " + ds.Tables[0].Rows.Count.ToString());
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        i++;
                        sb.AppendLine("<br>" + i + ". CID: " + row["CID"].ToString());
                        sb.AppendLine("Customer Name: " + row["CustomerName"].ToString());
                        sb.AppendLine("Connectivity Status: " + row["ConnectivityStatus"].ToString());
                        sb.AppendLine("Package Name: " + row["PackageName"].ToString());
                        sb.AppendLine("Invoice No: " + row["InvoiceNo"].ToString());
                        sb.AppendLine("Payment Status: " + row["Status"].ToString());
                        sb.AppendLine("Start Date: " + row["StartDate"].ToString());
                        sb.AppendLine("End Date: " + row["EndDate"].ToString() + "<br>");
                        sb.AppendLine("Comment: " + row["Comment"].ToString() + "<br>");
                        sb.AppendLine("Create By: " + row["CreateBy"].ToString());
                        sb.AppendLine("Sale Representativ: " + row["SaleName"].ToString());
                        sb.AppendLine("_________________________________________________");
                    }
                    await notificationTelegram.NotificationTelegramBotToTopic("<pre>" + sb.ToString().Replace("<br>", "\n") + "</pre>", -1903890245, 15);
                    sqlResult = await notificationTelegram.NotificationTelegramBotToTopic("<pre>" + sb.ToString().Replace("<br>", "\n") + "</pre>", -1545132903, 6);
                    sb.Clear();
                }
            }
            #endregion

            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
    }
}