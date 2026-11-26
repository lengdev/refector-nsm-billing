using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace NSMSOLUTION.Controllers.API
{
    public class JournalEntryApprovalController : ApiController
    {
        [HttpPost]
        [Route("api/JournalEntryApproval/UpdateStatus")]
        public async Task<HttpResponseMessage> JournalEntryApproval()
        {
            try
            {
                var formData = HttpContext.Current.Request.Form;
                if (string.IsNullOrEmpty(formData["JournalEntryID"]) || string.IsNullOrEmpty(formData["Status"]))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = "Missing required parameters." });
                }

                int transactionID = int.Parse(formData["JournalEntryID"]);
                string status = formData["Status"];
                string userName = (formData["UserName"] ?? "").Trim().TrimStart('@');
                string chatID = formData["ChatID"];
                string messageID = formData["MessageID"];

                HTML html = new HTML();

                string sql = @"
SELECT t.StatusID, 
       u.UserLoginID, u.FullName, u.TelegramUserName,
       t.CheckedByUserID, t.CheckedBy,
       t.VerifiedByUserID, t.VerifiedBy,
       t.ApprovedByUserID, t.ApprovedBy,
       t.SystemID, t.WarehouseID
FROM dbo.TblAccount_Transaction t
LEFT JOIN dbo.TblUserLogin u ON u.TelegramUserName = '@' + @UserName OR u.TelegramUserName = @UserName
WHERE t.TransactionID = @TransactionID";

                var para = new Dictionary<string, string> {
                    { "TransactionID", transactionID.ToString() },
                    { "UserName", userName }
                };
                DataSet ds = html.LoadData(sql, false, para);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { success = false, message = "Journal Entry not found." });
                }

                DataRow row = ds.Tables[0].Rows[0];
                int currentStatus = row["StatusID"] != DBNull.Value ? Convert.ToInt32(row["StatusID"]) : 60;
                int systemId = Convert.ToInt32(row["SystemID"]);
                int warehouseId = row["WarehouseID"] != DBNull.Value ? Convert.ToInt32(row["WarehouseID"]) : 0;
                string botToken = html.GetBotTokenBySystemID(systemId);

                int targetAssignedId = 0;
                string targetAssignedName = "";

                string normalizedStatus = status;
                if (status == "CheckJE" || status == "92") normalizedStatus = "CheckJournalEntry";
                else if (status == "VerifyJE" || status == "102") normalizedStatus = "VerifyJournalEntry";
                else if (status == "ApproveJE" || status == "90") normalizedStatus = "ApproveJournalEntry";
                else if (status == "DisapproveJE" || status == "91") normalizedStatus = "DisapproveJournalEntry";

                if (normalizedStatus == "CheckJournalEntry")
                {
                    targetAssignedId = row["CheckedByUserID"] != DBNull.Value ? Convert.ToInt32(row["CheckedByUserID"]) : 0;
                    targetAssignedName = row["CheckedBy"] != DBNull.Value ? row["CheckedBy"].ToString() : "";
                }
                else if (normalizedStatus == "VerifyJournalEntry")
                {
                    targetAssignedId = row["VerifiedByUserID"] != DBNull.Value ? Convert.ToInt32(row["VerifiedByUserID"]) : 0;
                    targetAssignedName = row["VerifiedBy"] != DBNull.Value ? row["VerifiedBy"].ToString() : "";
                }
                else if (normalizedStatus == "ApproveJournalEntry")
                {
                    targetAssignedId = row["ApprovedByUserID"] != DBNull.Value ? Convert.ToInt32(row["ApprovedByUserID"]) : 0;
                    targetAssignedName = row["ApprovedBy"] != DBNull.Value ? row["ApprovedBy"].ToString() : "";
                }
                else if (normalizedStatus == "DisapproveJournalEntry")
                {
                    if (currentStatus == 92)
                    {
                        targetAssignedId = row["VerifiedByUserID"] != DBNull.Value ? Convert.ToInt32(row["VerifiedByUserID"]) : (row["ApprovedByUserID"] != DBNull.Value ? Convert.ToInt32(row["ApprovedByUserID"]) : 0);
                        targetAssignedName = row["VerifiedBy"] != DBNull.Value && !string.IsNullOrEmpty(row["VerifiedBy"].ToString()) ? row["VerifiedBy"].ToString() : (row["ApprovedBy"] != DBNull.Value ? row["ApprovedBy"].ToString() : "");
                    }
                    else if (currentStatus == 102)
                    {
                        targetAssignedId = row["ApprovedByUserID"] != DBNull.Value ? Convert.ToInt32(row["ApprovedByUserID"]) : 0;
                        targetAssignedName = row["ApprovedBy"] != DBNull.Value ? row["ApprovedBy"].ToString() : "";
                    }
                    else
                    {
                        targetAssignedId = row["CheckedByUserID"] != DBNull.Value ? Convert.ToInt32(row["CheckedByUserID"]) : 0;
                        targetAssignedName = row["CheckedBy"] != DBNull.Value ? row["CheckedBy"].ToString() : "";
                    }
                }

                string actionUserFullName = "Telegram User";
                int actionUserID = 1;

                if (row["UserLoginID"] != DBNull.Value)
                {
                    actionUserID = Convert.ToInt32(row["UserLoginID"]);
                    actionUserFullName = row["FullName"].ToString();
                }

                bool isOverrideUser = actionUserID == 38 || actionUserID == 126 || actionUserID == 201;
                if (actionUserID <= 0)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { success = false, message = "User not found or authorized." });
                }

                // Validate authority: Only the assigned user or Boss/override user can take this action
                bool isAuthorized = isOverrideUser || (targetAssignedId > 0 && targetAssignedId == actionUserID);
                if (!isAuthorized)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { success = false, message = $"Only {targetAssignedName} can action this Journal Entry." });
                }

                var jeController = new NSMSOLUTION.Controllers.Finance.JournalEntryController();

                int targetStatusID = 60;

                if (normalizedStatus == "CheckJournalEntry") targetStatusID = 92;
                else if (normalizedStatus == "VerifyJournalEntry") targetStatusID = 102;
                else if (normalizedStatus == "ApproveJournalEntry") targetStatusID = 90;
                else if (normalizedStatus == "DisapproveJournalEntry") targetStatusID = 91;

                var jsonResult = await jeController.UpdateStatus(transactionID, targetStatusID, actionUserFullName, actionUserID,
                    !string.IsNullOrEmpty(chatID) ? long.Parse(chatID) : 0,
                    !string.IsNullOrEmpty(messageID) ? int.Parse(messageID) : 0);

                var data = jsonResult.Data;
                if (data != null)
                {
                    var propSuccess = data.GetType().GetProperty("success");
                    var propMessage = data.GetType().GetProperty("Message");
                    bool isSuccess = true;
                    string msg = "";
                    if (propSuccess != null)
                    {
                        isSuccess = Convert.ToBoolean(propSuccess.GetValue(data, null));
                    }
                    if (propMessage != null)
                    {
                        msg = Convert.ToString(propMessage.GetValue(data, null));
                    }

                    if (!isSuccess)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = msg });
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = ex.Message });
            }
        }

    }
}


