using MVC;
using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities;

namespace NewInventorySystem.Controllers.API
{
    public class LockUserController : ApiController
    {
        // GET api/<controller>
        [Authorize]
        public async Task<HttpResponseMessage> Post()
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            DataSet ds = new DataSet();

            #region Set Status To Teminate If > 1 month
            string sql = @"update TblActivateCustomer set [TerminatDate]='" + html.GetCurrentDate() + @"', ConnectivityStatus = 42 where ConnectivityStatus <> 42 and DATEDIFF(m, EndDate, getdate()) > 1;
update dbo.TblCustomerInvoice set ConnectivityStatus = 42 where ConnectivityStatus <> 42 and DATEDIFF(m, EndDate, getdate()) > 1;
update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 42 where ConnectivityStatus <> 42 and DATEDIFF(m, EndDate, getdate()) > 1;";
            sqlResult = html.SaveChangeData(sql, false, false);
            #endregion

            #region Lock user if PaymentDueDate < now and ConnectivityStatus is actviated or downspeed and invoice unpaid            
            sql = @"select distinct ta.ActivateCustomerID, ta.RouterID, ta.PPPoEUserName, ta.PPPoEPassword, ta.BillingInvoiceID, ta.ActivateCustomerID, c.CustomerCode CID, c.CustomerName CName, ta.ConnectionID, ta.[SID], bf.CompanyContactMobileNumber,
(c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + bi.UM +
case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName, ta.VLanName, ta.VLanIPAddress, ta.IPAddress CustomerIPAddress, ta.GetwayIP, ta.TicketNumber,
format(b.StartDate, 'dd-MM-yyyy') StartDate, format(b.EndDate, 'dd-MM-yyyy') EndDate, b.GrandTotal, b.InvoiceNo, format(ta.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate
, ta.ConnectionType, ta.VLanID, c.CustomerID, b.StatusID, b.GrandTotal Amount, isnull(AutoLock,1) AutoLock 
from dbo.TblActivateCustomer ta
inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID
inner join dbo.TblServices s on s.ServiceID = ta.ServiceID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ta.PackageDetailID
inner join dbo.TblCustomerBalance b on b.CustomerID = c.CustomerID and b.[Type] = 'Official Invoice' and b.InvoiceNo = ta.InvoiceNo and b.StatusID = 11
inner join dbo.TblCustomerBillingInvoice bi on bi.InvoiceNo = ta.InvoiceNo and bi.[Type] = 'Official Invoice'
inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = bi.CustomerBillInformationID
where (ta.ConnectivityStatus = 40 or ta.ConnectivityStatus = 50)
and format(b.PaymentDueDate, 'yyyyMMdd') < format(getdate(),'yyyyMMdd')
and ta.ConnectionID not in (isnull((select distinct cii.ConnectionID from dbo.TblCustomerBillingInvoice cii 
where cii.StatusID not in(12,13,14, 20)
and DATEDIFF(d, getdate(), cii.EndDate) > 0 and
cii.CustomerID = c.CustomerID and cii.ActivateCustomerID = ta.ActivateCustomerID and cii.IsRenew = 1),0))";
            ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string TelegramComment = "Dear Team, <br><br>";
                TelegramComment += "We would like confirm customer information as below is locked due to not yet payment.<br><br>";
                int i = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    i++;
                    string CommentTicket = "Dear Team, <br><br>";
                    CommentTicket += "We would like confirm customer information as below that is locked due to not yet payment.";

                    bool AutoLock = bool.Parse(dr["AutoLock"].ToString());

                    string comment = "System Locked due to not yet payment. Locked Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
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
                    string VLanID = dr["VLanID"].ToString();
                    string ActivateCustomerID = dr["ActivateCustomerID"].ToString();
                    string Subject = "YOUR " + ServiceName.ToUpper() + " LOCKED DUE TO NOT YET PAYMENT";
                    StringBuilder MsgBody = new StringBuilder();
                    MsgBody.AppendLine("We would like confirm your service is locked due to not yet payment.");
                    MsgBody.AppendLine("<br><br>");
                    MsgBody.AppendLine("You current service: " + ServiceName);
                    MsgBody.AppendLine("<br />Invoice No.: " + InvoiceNo);
                    MsgBody.AppendLine("<br />Package Name: " + PackageName);
                    MsgBody.AppendLine("<br />Payment Due Date: " + PaymentDueDate);
                    MsgBody.AppendLine("<br />Start Date: " + StartDate);
                    MsgBody.AppendLine("<br />Expired Date: " + EndDate);
                    MsgBody.AppendLine("<br />Amount USD: " + Amount);
                    //if (Email != "")
                    //    MsgBody.AppendLine("<br />Customer Email: " + Email);
                    TelegramComment += "<br><br>" + i + ". Customer Information<br>______________________________<br><br>";
                    TelegramComment += "SID: " + SID + "<br>";
                    TelegramComment += "Connection ID: " + ConnectionID + "<br>";
                    TelegramComment += "CID: " + CID + "<br>";
                    TelegramComment += "Customer Name: " + CName + "<br>";
                    TelegramComment += "Phone: " + CompanyContactMobileNumber + "<br><br>";
                    TelegramComment += "Service Name: " + ServiceName + "<br>";
                    TelegramComment += "Package Name: " + PackageName + "<br><br>";
                    TelegramComment += "Payment Due Date: " + PaymentDueDate + "<br>";
                    TelegramComment += "Start Date: " + StartDate + "<br>";
                    TelegramComment += "End Date: " + EndDate + "<br>";
                    TelegramComment += "Amount USD: " + Amount + "<br>";
                    TelegramComment += "Invoice No.: " + InvoiceNo + "<br>";
                    if (ConnectionType == "PPPoE")
                    {
                        TelegramComment += @"<br>Connection Type: " + ConnectionType + @"<br>PPPoE User Name: " + PPPoEUserName + @"<br>PPPoE Password: " + PPPoEPassword;
                    }
                    else if (ConnectionType == "Static")
                    {
                        TelegramComment += @"<br>Connection Type: " + ConnectionType + @"<br>VLan Name: " + VLanName + @"<br>VLan IP Address: " + VLanIPAddress + @"<br>Customer IP Address: " + CustomerIPAddress + @"<br>Getway IP: " + GetwayIP;
                    }
                    else
                    {
                        TelegramComment += @"<br>Connection Type: " + ConnectionType;
                    }
                    if (AutoLock)
                    {
                        Mikrotik MK = new Mikrotik();
                        MK.SetConnection(int.Parse(RouterID));
                        if (ConnectionType == "PPPoE")
                        {
                            sqlResult = MK.LockUser(PPPoEUserName, comment);
                            if (sqlResult.Result == ResultType.Successful)
                            {
                                await html.SendNotificationTelegramBotTopic("Locked PPPoE service successful. CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + comment, -1849499041, 2);
                                //await html.GetUserONUInfo(PPPoEUserName, 0, false, false, true);
                            }
                            else
                                await html.SendNotificationTelegramBotTopic("Locked PPPoE service failed. CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + sqlResult.Message + "<br><br>" + comment, -1849499041, 4);

                        }
                        else if (ConnectionType == "Static")
                        {
                            sqlResult = MK.DisableVlanID(VLanID, comment);
                            if (sqlResult.Result == ResultType.Successful)
                                await html.SendNotificationTelegramBotTopic("Locked Vlan service successful. CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + comment, -1849499041, 2);
                            else
                                await html.SendNotificationTelegramBotTopic("Locked Vlan service failed. CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + sqlResult.Message + "<br><br>" + comment, -1849499041, 4);
                        }
                        else
                        {
                            sqlResult.Result = ResultType.Successful;
                        }
                        if (sqlResult.Result == ResultType.Successful)
                        {
                            sql = "update dbo.TblActivateCustomer set [Date]='" + html.GetCurrentDate() + @"', ConnectivityStatus = 41, LockedBy='System', LockedDate='" + easternTimeNow.ToString() + "', Comment = '" + comment + "' where ActivateCustomerID = " + ActivateCustomerID;
                            sql += ";";
                            sql += "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 41, ConnectivityStatusName = '" + comment + "' where ActivateCustomerID = " + ActivateCustomerID;
                            sql += ";";
                            sql += "update dbo.TblCustomerInvoice set ConnectivityStatus = 41, ConnectivityStatusName = '" + comment + "' where ActivateCustomerID = " + ActivateCustomerID;
                            sql += ";";
                            sql += "insert into dbo.TblHistoryActivateCustomer ([Date],ActivateCustomerID, ConnectionType, ";
                            sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                            sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID) ";
                            sql += "select '" + easternTimeNow.ToString() + "', ac.ActivateCustomerID, ac.ConnectionType, ac.CustomerID, ";
                            sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, 'System' ActivateBy, '" + comment + "' Comment, '" + easternTimeNow.ToString() + "' LockedDate, ";
                            sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 41 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                            sql += "from dbo.TblActivateCustomer ac where ac.ActivateCustomerID = " + ActivateCustomerID;
                            sqlResult = html.SaveChangeData(sql, false, false);
                            //CommentTicket += TelegramComment + "<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO GENERATE TICKET.";
                            //sql =@"insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID, [Status], PriorityID, [Priority], OpenTicketID, FromUserLoginID, FromUserName,
                            //ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
                            //select SupportProjectID, 4 StatusID, 'Feedback' [Status], 1 PriorityID, 'Normal' [Priority], 
                            //OpenTicketID, UserLoginID FromUserLoginID, CreateBy FromUserName, " + html.UserID + @" ReplyFromUserLoginID, '" + html.UserFullName + @"' ReplyFromUserName, 
                            //N'" + CommentTicket + "' Comment, '" + html.GetCurrentDate() + @"' [Date], '" + html.UserName + @"' CreateBy, '" + html.GetCurrentDate() + @"'CreateDate, " + html.SystemID + " SystemID, " + html.WarehouseID + @" WarehouseID, 
                            //'Change Status to Feedback by " + html.UserFullName + @"' DescriptionChangeStatus
                            //from dbo.TblSupportOpenTicket
                            //where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;
                            //update dbo.TblSupportOpenTicket set SID = @SID, StatusID = 4, Status= 'Feedback', PriorityID = 1, Priority = 'Normal' where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID";
                            //Dictionary<string, string> para = new Dictionary<string, string>();
                            //para.Add("SID", SID);
                            //para.Add("TicketNumber", TicketNumber);
                            //para.Add("SystemID", html.SystemID);
                            //para.Add("WarehouseID", html.WarehouseID);
                            //sqlResult = html.SaveChangeData(sql, false, para);

                            //if (sqlResult.Result == ResultType.Successful)
                            //{
                            //    html.SendEmailToCustomer(Subject, CustomerName, Email, MsgBody.ToString());
                            //    html.SendEmailToBillingAndTech(Subject, CustomerName, Email, MsgBody.ToString());
                            //}
                        }
                        else
                        {
                            MsgBody.AppendLine(sqlResult.Message);
                            //html.SendEmailToBillingAndTech("Locked service faild for customer:" + CustomerName, CustomerName, "technical@nsmsolution.com.kh", MsgBody.ToString());
                        }
                    }
                    else
                    {
                        await html.SendNotificationTelegramBotTopic("NOT Locked CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + comment + "<br><br>NOT LOCKED due to set auto lock off.", -1849499041, 2);
                        await html.SendNotificationTelegramBotTopic("NOT Locked CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + comment + "<br><br>NOT LOCKED due to set auto lock off.", -1545132903, 9428);//1545132903/9428
                    }
                }
                //notificationTelegram.NotificationTelegramBot((TelegramComment).Replace("<br>", "\n"));
            }
            #endregion
            #region Down Speed if ConnectivityStatus is active and invoice unpaid and PaymentDueDate < 3days
            //Down Speed if ConnectivityStatus is active and invoice unpaid and PaymentDueDate < 3days
            //ds = html.LoadData(sql);
            //if (ds.Tables[0].Rows.Count > 0)
            //{
            //	foreach (DataRow dr in ds.Tables[0].Rows)
            //	{
            //		string comment = "System down speed due to not yet payment. Down Speed Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
            //		string RouterID = dr["RouterID"].ToString();
            //		string CID = dr["CID"].ToString();
            //		string ConnectionID = dr["ConnectionID"].ToString();
            //		string ConnectionType = dr["ConnectionType"].ToString();
            //		string PPPoEUser = dr["PPPoEUserName"].ToString();
            //		string PPPoEProfileName = dr["PPPoEProfileName"].ToString();
            //		string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
            //		//string Email = dr["Email"].ToString();
            //		string CustomerName = dr["CustomerName"].ToString();
            //		string ServiceName = dr["ServiceName"].ToString();
            //		string PackageName = dr["PackageName"].ToString();
            //		string StartDate = dr["StartDate"].ToString();
            //		string EndDate = dr["EndDate"].ToString();
            //		string Amount = dr["Amount"].ToString();
            //		string InvoiceNo = dr["InvoiceNo"].ToString();
            //		string PaymentDueDate = dr["PaymentDueDate"].ToString();
            //		string VLanID = dr["VLanID"].ToString();
            //		string ActivateCustomerID = dr["ActivateCustomerID"].ToString();
            //		string Subject = "NSM ISP DOWN SPEED TO CUSTOMER NAME: " + CustomerName;
            //		StringBuilder MsgBody = new StringBuilder();
            //		MsgBody.AppendLine("We would like confirm your service will be lock due to not yet payment.");
            //		MsgBody.AppendLine("<br /><br />");
            //		MsgBody.AppendLine("You current service: " + ServiceName);
            //		MsgBody.AppendLine("<br />Invoice No.: " + InvoiceNo);
            //		MsgBody.AppendLine("<br />Package Name: " + PackageName);
            //		MsgBody.AppendLine("<br />Payment Due Date: " + PaymentDueDate);
            //		MsgBody.AppendLine("<br />Start Date: " + StartDate);
            //		MsgBody.AppendLine("<br />Expired Date: " + EndDate);
            //		MsgBody.AppendLine("<br />Amount USD: " + Amount);
            //		Mikrotik MK = new Mikrotik();
            //		MK.SetConnection(int.Parse(RouterID));
            //		int ConnStatus = 50;
            //		bool forpppoeuser = false;
            //		if (ConnectionType == "PPPoE")
            //		{
            //			forpppoeuser = true;
            //			//sqlResult = MK.ReActivateUser(PPPoEUser);
            //			//sqlResult = MK.DownSpeedUser3M(PPPoEUser, comment);
            //			//if (sqlResult.Result == ResultType.Successful)
            //			//await html.SendNotificationTelegramBotTopic("Down Speed PPPoE service successful. CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + comment, -1849499041, 2);
            //			await html.SendNotificationTelegramBotTopic("Tempory disable Down Speed PPPoE service. CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + comment, -1903890245, 656);//1001903890245_656
            //			await html.SendNotificationTelegramBotTopic("Tempory disable Down Speed PPPoE service Please manual. CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + comment, -1849499041, 2);
            //			//else
            //			//await html.SendNotificationTelegramBotTopic("Down Speed PPPoE service failed. CID: " + CID + "<br>Connection ID: "+ ConnectionID + "<br><br>" + sqlResult.Message + "<br><br>" + comment, -1849499041, 4);
            //		}
            //		//else if (ConnectionType =="Static")
            //		//{
            //		//    comment = "System locked due to not yet payment. Locked Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt")+". Locked By System.";
            //		//    ConnStatus = 41;
            //		//    sqlResult = MK.DisableVlanID(VLanID, comment);
            //		//}
            //		else
            //		{
            //			sqlResult.Result = ResultType.Successful;
            //		}
            //		if (sqlResult.Result == ResultType.Successful && forpppoeuser)
            //		{
            //			sql = "update dbo.TblActivateCustomer set [Date]='"+html.GetCurrentDate()+@"', ConnectivityStatus = " + ConnStatus + ", LockedBy ='System', LockedDate = '" + easternTimeNow.ToString() + "', Comment = '" + comment + "' where ActivateCustomerID = " + ActivateCustomerID;
            //			sql += ";";
            //			sql += "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = " + ConnStatus + ", ConnectivityStatusName = '" + comment + "' where ActivateCustomerID = " + ActivateCustomerID;
            //			sql += ";";
            //			sql += "update dbo.TblCustomerInvoice set ConnectivityStatus = " + ConnStatus + ", ConnectivityStatusName = '" + comment + "' where ActivateCustomerID = " + ActivateCustomerID;
            //			sql += ";";
            //			sql += "insert into dbo.TblHistoryActivateCustomer ([Date],ActivateCustomerID, ConnectionType, ";
            //			sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
            //			sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID) ";
            //			sql += "select '" + easternTimeNow.ToString() + "', ac.ActivateCustomerID, ac.ConnectionType, ac.CustomerID, ";
            //			sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, 'System' ActivateBy, '" + comment + "' Comment, '" + easternTimeNow.ToString() + "' LockedDate, ";
            //			sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, " + ConnStatus + " ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
            //			sql += "from dbo.TblActivateCustomer ac where ac.ActivateCustomerID = " + ActivateCustomerID;
            //			sqlResult = html.SaveChangeData(sql, false, false);
            //		}
            //		else
            //		{
            //			MsgBody.AppendLine(sqlResult.Message);
            //			//html.SendEmailToBillingAndTech("Locked service faild for customer:" + CustomerName, CustomerName, "technical@nsmsolution.com.kh", MsgBody.ToString());
            //		}
            //	}
            //}
            #endregion

            #region System Locked due to expired
            //System Locked due to expired
            sql = @"select distinct c.CustomerID, ta.ActivateCustomerID, ta.ConnectionID, c.CustomerCode CID, ta.RouterID, ta.PPPoEUserName, ta.PPPoEProfileName, ta.BillingInvoiceID, ta.ActivateCustomerID,
(c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM +
case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName,
format(b.StartDate, 'dd-MM-yyyy') StartDate, format(ta.EndDate, 'dd-MM-yyyy') EndDate, b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate
, ta.ConnectionType, ta.VLanID, ta.VLanName, isnull(ta.AutoLock, 1) AutoLock from dbo.TblActivateCustomer ta
inner join dbo.TblCustomerBillingInvoice b on b.InvoiceNo = ta.InvoiceNo or b.ConnectionID = ta.ConnectionID
inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID
inner join dbo.TblServices s on s.ServiceID = ta.ServiceID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ta.PackageDetailID
where ta.ConnectivityStatus = 40 and b.IsRenew = 0 and ta.InvoiceNo = b.InvoiceNo
and format(ta.EndDate, 'yyyyMMdd') <= format(getdate(), 'yyyyMMdd')
and format(ta.EndDate, 'yyyyMMdd') = format(b.EndDate, 'yyyyMMdd')
order by c.CustomerCode";
            ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    bool AutoLock = bool.Parse(dr["AutoLock"].ToString());

                    string comment = "System Locked due to expired. Please contact us for renew. Locked Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                    string RouterID = dr["RouterID"].ToString();
                    string CID = dr["CID"].ToString();
                    string ConnectionID = dr["ConnectionID"].ToString();
                    string VLan = dr["VLanName"].ToString();
                    string ConnectionType = dr["ConnectionType"].ToString();
                    string PPPoEUser = dr["PPPoEUserName"].ToString();
                    string PPPoEProfileName = dr["PPPoEProfileName"].ToString();
                    string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                    //string Email = dr["Email"].ToString();
                    string CustomerName = dr["CustomerName"].ToString();
                    string ServiceName = dr["ServiceName"].ToString();
                    string PackageName = dr["PackageName"].ToString();
                    string StartDate = dr["StartDate"].ToString();
                    string EndDate = dr["EndDate"].ToString();
                    string Amount = dr["Amount"].ToString();
                    string InvoiceNo = dr["InvoiceNo"].ToString();
                    string PaymentDueDate = dr["PaymentDueDate"].ToString();
                    string VLanID = dr["VLanID"].ToString();
                    string ActivateCustomerID = dr["ActivateCustomerID"].ToString();
                    string Subject = "YOUR " + ServiceName.ToUpper() + " IS EXPIRED AND LOCKED DUE TO NOT YET PAYMENT";
                    StringBuilder MsgBody = new StringBuilder();
                    MsgBody.AppendLine("We would like confirm your service is locked due to expired. Please contact us for renew.");
                    MsgBody.AppendLine("<br /><br />");
                    MsgBody.AppendLine("You current service: " + ServiceName);
                    MsgBody.AppendLine("<br />Invoice No.: " + InvoiceNo);
                    MsgBody.AppendLine("<br />Package Name: " + PackageName);
                    MsgBody.AppendLine("<br />Payment Due Date: " + PaymentDueDate);
                    MsgBody.AppendLine("<br />Start Date: " + StartDate);
                    MsgBody.AppendLine("<br />Expired Date: " + EndDate);
                    MsgBody.AppendLine("<br />Amount USD: " + Amount);
                    //if (Email != "")
                    //    MsgBody.AppendLine("<br />Customer Email: " + Email);
                    if (AutoLock)
                    {
                        Mikrotik MK = new Mikrotik();
                        MK.SetConnection(int.Parse(RouterID));
                        if (ConnectionType == "PPPoE")
                        {
                            sqlResult = MK.LockUser(PPPoEUser, comment);
                            if (sqlResult.Result == ResultType.Successful)
                            {
                                await html.SendNotificationTelegramBotTopic("Locked PPPoE service successful. CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + comment, -1849499041, 2);
                                //await html.GetUserONUInfo(PPPoEUser, 0, false, false, true);
                            }
                            else
                                await html.SendNotificationTelegramBotTopic("Locked PPPoE service failed. CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + sqlResult.Message + "<br><br>" + comment, -1849499041, 4);
                        }
                        else if (ConnectionType == "Static")
                        {
                            sqlResult = MK.DisableVlanID(VLanID, comment);
                            if (sqlResult.Result == ResultType.Successful)
                                await html.SendNotificationTelegramBotTopic("Disable Vlan service successful. CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + comment, -1849499041, 2);
                            else
                                await html.SendNotificationTelegramBotTopic("Disable Vlan service failed. CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + sqlResult.Message + "<br><br>" + comment, -1849499041, 4);
                        }
                        else
                        {
                            sqlResult.Result = ResultType.Successful;
                        }
                        if (sqlResult.Result == ResultType.Successful)
                        {
                            VLan = VLan.ToString().Replace("null", "") == "" ? "PPPoE Profile Name: " + PPPoEProfileName : "VLanName: " + VLan;
                            //await html.SendNotificationTelegramBotTopic("Locked service successful for customer: " + CustomerName + "<br><br>CID: " + CID + "<br><br>" + VLan + "<br>" + MsgBody.ToString(), -1849499041, 2);

                            sql = "update dbo.TblActivateCustomer set [Date]='" + html.GetCurrentDate() + @"', ConnectivityStatus = 41, LockedBy='System', LockedDate='" + easternTimeNow.ToString() + "', Comment = '" + comment + "' where ActivateCustomerID = " + ActivateCustomerID;
                            sql += ";";
                            sql += "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 41, ConnectivityStatusName = '" + comment + "' where ActivateCustomerID = " + ActivateCustomerID;
                            sql += ";";
                            sql += "update dbo.TblCustomerInvoice set ConnectivityStatus = 41, ConnectivityStatusName = '" + comment + "' where ActivateCustomerID = " + ActivateCustomerID;
                            sql += ";";
                            sql += "insert into dbo.TblHistoryActivateCustomer ([Date], ActivateCustomerID, ConnectionType, ";
                            sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                            sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID) ";
                            sql += "select '" + easternTimeNow.ToString() + "', ac.ActivateCustomerID, ac.ConnectionType, ac.CustomerID, ";
                            sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, 'System' ActivateBy, '" + comment + "' Comment, '" + easternTimeNow.ToString() + "' LockedDate, ";
                            sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 41 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                            sql += "from dbo.TblActivateCustomer ac where ac.ActivateCustomerID = " + ActivateCustomerID;
                            sqlResult = html.SaveChangeData(sql, false, false);
                        }
                        else
                        {
                            MsgBody.AppendLine(sqlResult.Message);
                            //await html.SendNotificationTelegramBotTopic("Locked service failed for customer: " + CustomerName + "<br><br>CID: " + CID + "<br><br>" + VLan + "<br>" + MsgBody.ToString(), -1849499041, 4);

                            //html.SendEmailToBillingAndTech("Locked service faild for customer:" + CustomerName, CustomerName, "technical@nsmsolution.com.kh", MsgBody.ToString());                        
                        }
                    }
                    else
                    {
                        await html.SendNotificationTelegramBotTopic("NOT Locked CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + comment + "<br><br>NOT LOCKED due to set auto lock off.", -1849499041, 2);
                        await html.SendNotificationTelegramBotTopic("NOT Locked CID: " + CID + "<br>Connection ID: " + ConnectionID + "<br><br>" + comment + "<br><br>NOT LOCKED due to set auto lock off.", -1545132903, 9428);//1545132903/9428
                    }
                }
            }
            #endregion

            #region do customer change plan
            //do customer change plan
            sql = @"select bi.ActivateCustomerID, bi.CustomerID, bi.BillingInvoiceID, bi.InvoiceType, bi.PPPoEUserName, 
                    bi.ConnectivityStatus, bi.StartDate, bi.EndDate, bi.Duration, isnull(bi.IsAlreadyApplyProfile, 0) IsAlreadyApplyProfile, 
                    bi.IsChangePlan, bi.PackageName 
                    from dbo.TblCustomerBillingInvoice bi 
                    inner join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = bi.ActivateCustomerID
                    where bi.StatusID = 10 and isnull(ac.IsApplyChangePlan, 0) = 0
                    and bi.IsChangePlan = 1 and format(bi.StartDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd')";
            ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string ActivateCustomerID = dr["ActivateCustomerID"].ToString();
                    string CustomerID = dr["CustomerID"].ToString();
                    string PPPoEProfileName = dr["PackageName"].ToString();
                    string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                    string StartDate = dr["StartDate"].ToString();
                    string EndDate = dr["EndDate"].ToString();
                    string Duration = dr["Duration"].ToString();

                    sql = @"select RouterID, ActivateCustomerID, PPPoEUserName, ConnectionType, PPPoEProfileName OldPPPoEProfileName, VLanID from dbo.TblActivateCustomer 
                            where CustomerID = " + CustomerID + " and ActivateCustomerID = " + ActivateCustomerID +
                            " and isnull(IsApplyChangePlan, 0) = 0";
                    DataSet ds2 = html.LoadData(sql);
                    if (ds2.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr2 = ds2.Tables[0].Rows[0];
                        string RouterID = dr2["RouterID"].ToString();
                        string PPPoEUserName = dr2["PPPoEUserName"].ToString();
                        string ConnectionType = dr2["ConnectionType"].ToString();
                        string OldPPPoEProfileName = dr2["OldPPPoEProfileName"].ToString();
                        string VLanID = dr2["VLanID"].ToString();
                        Mikrotik MK = new Mikrotik();
                        MK.SetConnection(int.Parse(RouterID));
                        SqlResult sqlresult = new SqlResult();
                        if (ConnectionType == "PPPoE")
                        {
                            string Comment = "Upgrade from plan: " + OldPPPoEProfileName + " to " + PPPoEProfileName + "; Upgrade plan to Customer by: System; Upgrade Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                            MK.ClearProfileName(PPPoEUserName);
                            sqlresult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                            if (sqlresult.Result == ResultType.Successful)
                            {
                                sql += "update dbo.TblActivateCustomer set [Date]='" + html.GetCurrentDate() + @"', IsApplyChangePlan = 1, BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', StartDate = '" + StartDate + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='System', Comment = '" + Comment + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                sql += ";";
                                sql += "insert into dbo.TblHistoryActivateCustomer ([Date], ActivateCustomerID, ConnectionType, ";
                                sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate) ";
                                sql += "select '" + easternTimeNow.ToString() + "', ac.ActivateCustomerID, ac.ConnectionType, ac.CustomerID, ";
                                sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, 'System' ActivateBy, '" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                                sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                                //sql += "inner join dbo.TblCustomerBillingInvoice bi on bi.BillingInvoiceID = ac.BillingInvoiceID ";
                                sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                sqlresult = html.SaveChangeData(sql, false, false);
                            }
                        }
                        else
                        {
                            string Comment = "Upgrade from plan: " + OldPPPoEProfileName + " to " + PPPoEProfileName + "; Upgrade plan to Customer by: System; Upgrade Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                            sqlresult = MK.EnableVlanID(VLanID, Comment);
                            if (sqlresult.Result == ResultType.Successful)
                            {
                                sql += "update dbo.TblActivateCustomer set [Date]='" + html.GetCurrentDate() + @"', IsApplyChangePlan = 1, BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', StartDate = '" + StartDate + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='System', Comment = '" + Comment + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                sql += ";";
                                sql += "insert into dbo.TblHistoryActivateCustomer ([Date], ActivateCustomerID, ConnectionType, ";
                                sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate) ";
                                sql += "select '" + easternTimeNow.ToString() + "', ac.ActivateCustomerID, ac.ConnectionType, ac.CustomerID, ";
                                sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, 'System' ActivateBy, '" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                                sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                                //sql += "inner join dbo.TblCustomerBillingInvoice bi on bi.BillingInvoiceID = ac.BillingInvoiceID ";
                                sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                sqlresult = html.SaveChangeData(sql, false, false);
                            }
                        }
                    }
                }
            }
            #endregion
            return Request.CreateResponse(sqlResult.Result == ResultType.Successful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, sqlResult.Message);
        }
    }
}