using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Utilities;

namespace MVC
{
    public class ApprovPO
    {
        public async Task<SqlResult> ApprovalPO(int POID, int POStatus)
        {
            HTML html = new HTML();
            string sql = @"update dbo.TblPurchaseOrder set POStatus = @POStatus, ApproveBy = @ApproveBy, AprrovalFrom = @AprrovalFrom,
ApprovalDate = @ApprovalDate, StatusID = @StatusID where PurchaseOrderID = @POID;
select VendorID, VendorName, PONumber, TelegramMessageID, TelegramMessage from dbo.TblPurchaseOrder where PurchaseOrderID = @POID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"POID", POID.ToString() },
                {"StatusID", POStatus.ToString()},
                {"POStatus", POStatus.ToString()},
                {"ApproveBy", html.UserName},
                {"AprrovalFrom", "Billing System"},
                {"ApprovalDate", html.GetCurrentDate()},
            };
            SqlResult result = html.SaveChangeData(sql, false, para, true, true);
            DataRow dr = result.Data.Tables[0].Rows[0];
            string VendorID = dr["VendorID"].ToString();
            string VendorName = html.ToUrlSlug(dr["VendorName"].ToString());
            string PONumber = html.ToUrlSlug(dr["PONumber"].ToString());
            string TelegramMessageID = dr["TelegramMessageID"].ToString();
            string TelegramMessage = dr["TelegramMessage"].ToString();
            result = await html.URLtoPDF("Profile/InvoiceTMP?VendorID=" + VendorID + "&POID=" + POID + "&Type=PO&ReturnHtmlData=false&ReturnAllHtmlData=false&RedirectToPOView=true", "", "Uploads/Vendors/" + VendorName, PONumber);

            if (result.Result == ResultType.Successful)
            {
                NotificationTelegram notificationTelegram = new NotificationTelegram();
                string msg = "Dear Team,<br><br>Now this PO " + (POStatus == 90 ? "Approved By " : "Disapproved By ") + html.UserName + ".";
                msg += "<br><br>Please check attachment file for detail.<br><br>Thank you and best regards,<br><br>";
                msg += "NSM ISP BILLING SYSTEM AUTO SEND PO NOTIFICATION. #" + PONumber;
                result = await notificationTelegram.NotificationTelegramBot("", -967102140, "", result.OutPutFile, msg, false, false, POID, int.Parse(TelegramMessageID), TelegramMessage);
            }
            return result;
        }
    }
}
