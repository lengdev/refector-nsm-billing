using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities;

public enum CustomerAlert
{
    Activate,
    Renew,
    ExpiryNear,
    ExpirySoon
}

public static class CustomerTemplates
{
    private class Template
    {
        public string TitleEn;
        public string TitleKh;
        public bool NameFirst;
        public bool ShowDaysLeft;
        public bool ShowPayNotice;
    }

    private static readonly Dictionary<CustomerAlert, Template> Templates = new Dictionary<CustomerAlert, Template>
    {
        [CustomerAlert.Activate] = new Template
        {
            TitleEn = "«Welcome to NSM ISP! Now your connection is activated.»",
            TitleKh = "«សេចក្តីជូនដំណឹងអំពី អ្នកបានភ្ជាប់កញ្ចប់សេវាកម្មអ៊ិនធឺណិត»",
            NameFirst = true
        },
        [CustomerAlert.Renew] = new Template
        {
            TitleEn = "«Notification: your Internet service has been renewed»",
            TitleKh = "«សេចក្តីជូនដំណឹងអំពី អ្នកបានបន្តការប្រើប្រាស់កញ្ចប់សេវាកម្មអ៊ិនធឺណិត»",
            NameFirst = true
        },
        [CustomerAlert.ExpiryNear] = new Template
        {
            TitleEn = "«Notification of expiration of Internet Services»",
            TitleKh = "«សេចក្តីជូនដំណឹងអំពី អ្នកជិតផុតសុពលភាពនៃការប្រើប្រាស់កញ្ចប់សេវាកម្មអ៊ិនធឺណិត»",
            ShowDaysLeft = true,
            ShowPayNotice = true
        },
        [CustomerAlert.ExpirySoon] = new Template
        {
            TitleEn = "«Notification of expiration of Internet Services»",
            TitleKh = "«សេចក្តីជូនដំណឹងអំពី អ្នកនឹងត្រូវផុតសុពលភាពការប្រើប្រាស់សេវាកម្មអ៊ិនធឺណិត»",
            ShowDaysLeft = true,
            ShowPayNotice = true
        }
    };

    public static CustomerAlert ExpiryFor(int daysLeft)
    {
        return daysLeft <= 3 ? CustomerAlert.ExpirySoon : CustomerAlert.ExpiryNear;
    }

    private static bool IsKhmer(string nationality)
    {
        var n = (nationality ?? "").Trim();
        return n.ToLower() == "khmer" || n == "ខ្មែរ";
    }

    private static string CleanBr(string s)
    {
        return (s ?? "").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n");
    }

    private static string H(string s)
    {
        return WebUtility.HtmlEncode(s ?? "");
    }

    public static string Build(
        CustomerAlert type,
        string customerName,
        string customerCode,
        string nationality,
        string packageName,
        string startDate,
        string endDate,
        int daysLeft = 0,
        string attName = null)
    {
        Template t;
        if (!Templates.TryGetValue(type, out t)) return null;

        bool kh = IsKhmer(nationality);
        var sb = new StringBuilder();

        sb.Append(kh ? t.TitleKh : t.TitleEn).Append("\n\n");

        string nameLine = (kh ? "ឈ្មោះអតិថិជន: " : "Customer Name: ") + H(customerName) + "\n";
        string cidLine = (kh ? "លេខសំគាល់អតិថិជន: " : "CID: ") + H(customerCode) + "\n";
        sb.Append(t.NameFirst ? nameLine + cidLine : cidLine + nameLine);

        sb.Append(kh ? "ឈ្មោះកញ្ចប់សេវាកម្មៈ " : "Package Name: ")
          .Append("<b>").Append(H(CleanBr(packageName))).Append("</b>\n\n");

        sb.Append(kh ? "ថ្ងៃចាប់ផ្តើមៈ " : "Start Date: ")
          .Append("<b>").Append(H(startDate)).Append("</b>\n");
        sb.Append(kh ? "ថ្ងៃផុតសុពលភាពៈ " : "End Date: ")
          .Append("<b>").Append(H(endDate)).Append("</b>\n");

        if (t.ShowDaysLeft)
        {
            string unit = kh ? " ថ្ងៃ" : (daysLeft == 1 ? " Day" : " Days");
            sb.Append(kh ? "ចំនួនថ្ងៃនៅសល់ៈ " : "Number of days left: ")
              .Append("<b>").Append(daysLeft).Append(unit).Append("</b>\n");

            if (!string.IsNullOrWhiteSpace(attName))
                sb.Append("\nATT Name: ").Append(H(attName)).Append("\n");
        }

        sb.Append("\n");

        if (t.ShowPayNotice)
        {
            sb.Append(kh
                ? "ដើម្បីជៀសវាងការផ្អាកសេវាកម្មអ៊ិនធឺណិត សូមអតិថិជនអញ្ជើញបង់ប្រាក់អោយបានមុនថ្ងៃផុតកំណត់។\n\nសូមមេត្តាបង់ប្រាក់ជាដុល្លារ (USD)។\n\n"
                : "Please pay before the due date. If payment is not made on time, our company will temporarily suspend the service.\nThe service will be reconnected after payment.\n\nPlease make the payment in US dollars.\n\n");
        }

        sb.Append(kh
            ? "សម្រាប់ពត៌មានបន្ថែមសូមទាក់ទង\nផ្នែកបម្រើអតិថិជន: 85586588777, 85578588777\nផ្នែកបច្ចេកទេសៈ 85561876777\nគេហទំព័រ: www.nsmisp.com.kh\n\nសូមអរគុណ!"
            : "Contact Information\nCustomer Services: 85586588777, 85578588777\nTechnical: 85561876777\nWebsite: www.nsmisp.com.kh\n\nThank you!");

        return sb.ToString();
    }
}

public static class CustomerNotifier
{
    private static readonly Dictionary<CustomerAlert, int> NotificationTypeId = new Dictionary<CustomerAlert, int>
    {
        [CustomerAlert.Activate] = 1
    };

    public static async Task SendAsync(
        HTML html,
        CustomerAlert type,
        int customerId,
        string customerCode,
        string customerName,
        string nationality,
        string connectionId,
        int activateCustomerId,
        string packageName,
        string startDate,
        string endDate,
        int daysLeft = 0,
        string attName = null)
    {
        try
        {
            int typeId;
            if (!NotificationTypeId.TryGetValue(type, out typeId)) return;

            string msg = CustomerTemplates.Build(type, customerName, customerCode, nationality,
                packageName, startDate, endDate, daysLeft, attName);
            if (msg == null) return;

            var selectParams = new Dictionary<string, string>
            {
                ["@TypeId"] = typeId.ToString(),
                ["@CustomerId"] = customerId.ToString()
            };

            string sql = @"select ct.TelegramNotificationID, ct.CustomerPhone, ct.FirstName, ct.LastName, ct.CustomerID
from dbo.TblCustomerTelegram ct
where isnull(ct.TelegramNotificationID,'') <> ''
and exists (select 1 from dbo.Split(ct.TelegramNotificationID, ',') where [Data] = @TypeId)
and ct.CustomerID = @CustomerId and ct.IsUsingTelegram = 'Yes'";

            DataSet ds = html.LoadData(sql, false, selectParams);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return;

            var tg = new NotificationTelegram();

            const string insertSql = @"insert into dbo.TblCustomerTelegramMessage
(DateSend, SendBy, CustomerID, ConnectionID, ActivateCustomerID, TelegramNotificationID,
 MessgeID, MessageText, MessageStatus, CustomerPhone)
values (@DateSend, 'System', @CustomerId, @ConnectionId, @ActivateCustomerId, @TypeId,
 @MessageId, @MessageText, @MessageStatus, @Phone)";

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string phone = dr["CustomerPhone"].ToString();
                string first = dr["FirstName"].ToString();
                string last = dr["LastName"].ToString();
                int custId = int.Parse(dr["CustomerID"].ToString());

                SqlResult sent;
                try { sent = await tg.NotificationToCustomer(phone, first, last, custId, msg); }
                catch (Exception ex) { sent = new SqlResult { Message = "Error: " + ex.Message }; }

                var logParams = new Dictionary<string, string>
                {
                    ["@DateSend"] = html.GetCurrentDate().ToString(),
                    ["@CustomerId"] = customerId.ToString(),
                    ["@ConnectionId"] = connectionId ?? "",
                    ["@ActivateCustomerId"] = activateCustomerId.ToString(),
                    ["@TypeId"] = typeId.ToString(),
                    ["@MessageId"] = Convert.ToString(sent.Value) ?? "",
                    ["@MessageText"] = msg,
                    ["@MessageStatus"] = sent.Message ?? "",
                    ["@Phone"] = phone
                };

                html.SaveChangeData(insertSql, false, logParams);
            }
        }
        catch { }
    }
}