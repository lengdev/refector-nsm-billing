using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.Http;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class SendNotificationController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            StringBuilder MessageBody = new StringBuilder();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            DataSet ds = html.LoadData("SP_CHECK_AND_SEND_NOTIFICATION", true, para);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string CustomerID = dr["CustomerID"].ToString();
                    string ServiceID = dr["ServiceID"].ToString();
                    string PackageDetailID = dr["PackageDetailID"].ToString();
                    string EmailAddress = dr["Email"].ToString();
                    string CustomerName = dr["CustomerName"].ToString();
                    string EndDate = dr["EndDate"].ToString();
                    string SeviceName = dr["ServiceName"].ToString();
                    string PackageName = dr["PackageName"].ToString();
                    string Bandwidth = dr["Bandwidth"].ToString();
                    string DowSpeed = dr["DowSpeed"].ToString();
                    string UpSpeed = dr["UpSpeed"].ToString();
                    string Duration = dr["Duration"].ToString();
                    string UM = dr["UM"].ToString();
                    string Amount = dr["Amount"].ToString();
                    string FreeDuration = dr["FreeDuration"].ToString();
                    string Day = dr["Day"].ToString();
                    string StartDate = dr["StartDate"].ToString();
                    string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                    PackageName = PackageName + " - " + Bandwidth + " Mbps - " + UM;
                    if (FreeDuration != "0" && FreeDuration != "")
                        PackageName += " FreeDuration:" + FreeDuration + " Month(s)";

                    MessageBody.AppendLine("We would like confirm your service will be lock in next 3days due to expire on " + EndDate);
                    MessageBody.AppendLine("<br /><br />");
                    MessageBody.AppendLine("You current service: " + SeviceName);
                    MessageBody.AppendLine("<br />Package Name: " + PackageName);
                    MessageBody.AppendLine("<br />Start Date: " + StartDate);
                    MessageBody.AppendLine("<br />Expired Date: " + EndDate);
                    MessageBody.AppendLine("<br />Amount USD: " + Amount);
                    string SQL = "insert into dbo.TblGenerateInvoice (CustomerID, ServiceID, PackageDetailID, Amount, [Message], GeneratedDate, GeneratedBy, StartDate, EndDate, BillingInvoiceID, NotificationType, NotificationStatus)";
                    SQL += " values (@CustomerID, @ServiceID, @PackageDetailID, @Amount, @Message, @GeneratedDate, @GeneratedBy, @StartDate, @EndDate, @BillingInvoiceID, @NotificationType, @NotificationStatus)";
                    para.Clear();
                    para.Add("CustomerID", CustomerID);
                    para.Add("ServiceID", ServiceID);
                    para.Add("PackageDetailID", PackageDetailID);
                    para.Add("Amount", Amount);
                    para.Add("Message", "This customer service will be lock in next 3days due to expire on " + EndDate);
                    para.Add("GeneratedDate", easternTimeNow.ToString());
                    para.Add("GeneratedBy", "System");
                    para.Add("StartDate", StartDate);
                    para.Add("EndDate", EndDate);
                    para.Add("BillingInvoiceID", BillingInvoiceID);

                    if (EmailAddress != "")
                    {
                        string Subject = "NSM ISP YOUR SERVICE WILL BE LOCK IN NEXT 3DAYS";
                        sqlResult = html.SendEmailToCustomer(Subject, CustomerName, EmailAddress, MessageBody.ToString());
                        if (sqlResult.Result == ResultType.Successful)
                        {
                            para.Add("NotificationType", "Email");
                            para.Add("NotificationStatus", "Success");
                        }
                        else
                        {
                            para.Add("NotificationType", "Email");
                            para.Add("NotificationStatus", sqlResult.Message);
                        }
                        //SQL = "SP_SEND_MAIL";
                        //para.Add("Subject", Subject);
                        //para.Add("EmailAddress", EmailAddress);
                        //para.Add("EmailBody", MessageBody.ToString());
                        //sqlResult = html.SaveChangeData(SQL, true, para);                        
                    }
                    else
                    {
                        para.Add("NotificationType", "");
                        para.Add("NotificationStatus", "");
                    }
                    sqlResult = html.SaveChangeData(SQL, false, para);
                    MessageBody.Clear();
                }
            }

            if (ds.Tables[1].Rows.Count > 0)
            {

            }

            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}