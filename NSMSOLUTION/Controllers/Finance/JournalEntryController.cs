using MVC;
using NSMSOLUTION.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers.Finance
{

    public class JournalEntryController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "Journal Entries"
            };

            if (html.HasPermission("Add Journal Entry") || html.HasPermission("Report General Journal"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Journal Entry") || html.HasPermission("Report General Journal"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Journal Entry") || html.HasPermission("Report General Journal"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add Journal Entry";
            html.AddURL = "JournalEntry/Create?systemId=" + html.SystemID + "&warehouseId=" + html.WarehouseID;
            html.TableName = "dbo.TblAccount_Transaction";
            html.PrimaryField = "TransactionID";
            html.OrderBy = "TransactionID desc";
            html.TblAlign = "auto";
            html.Cellpadding = 4;

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date Range:",
                LabelAlign = "left",
                DBField = "t.TransactionDate",
                DateTimePickerWithRanges = true
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtJVNo",
                ISAutoComplete = true,
                Label = "Entry No:",
                LabelAlign = "left",
                DBField = "t.InvoiceNo",
                SQLAutoComplete = "SELECT DISTINCT TOP 10 InvoiceNo FROM dbo.TblAccount_Transaction WHERE [Type] = 'Journal Entry' AND LOWER(InvoiceNo) LIKE @SearchText AND SystemID = " + html.SystemID + " AND WarehouseID = " + html.WarehouseID + " ORDER BY InvoiceNo DESC"
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Status:",
                LabelAlign = "left",
                DBField = "t.StatusID",
                SQL_CBO = "SELECT DISTINCT StatusID, [Status] FROM dbo.TblStatus WHERE StatusID IN (60, 90, 91, 92, 102)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                ID = "CboSearchText",
                OPTLabel = "Class",
                SQL = @"SELECT ClassID, ClassName FROM dbo.TblClass WHERE SystemID = " + html.SystemID + " AND WarehouseID = " + html.WarehouseID,
                DisplayField = "ClassName",
                SelectValue = "ClassID",
                RadioValue = EncryAndDecry.Encryp("t.ClassNameID"),
                DefaultCheckBok = true
            });

            // EnsureSchema(html);

            string sql = @"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY t.TransactionDate DESC, t.TransactionID DESC) AS [No],
                    t.TransactionID,
                    '<a class=""text-primary"" style=""cursor: pointer; text-decoration: none;"" onclick=""showJournalForm(' + CONVERT(VARCHAR, t.TransactionID) + ', ''' + t.InvoiceNo + ''')"">' + t.InvoiceNo + '</a>' AS EntryNoLink,
                    t.InvoiceNo AS EntryNo,
                    FORMAT(t.TransactionDate, 'dd/MM/yyyy') AS TransactionDate,
                    ISNULL(c.ClassName, '') AS ClassName,
                    t.Description AS Memo,
                    t.Amount AS TotalAmount,
                    CASE 
                        WHEN t.StatusID = 90 THEN '<span class=""badge badge-primary"">Approved</span>'
                        WHEN t.StatusID = 91 THEN '<span class=""badge badge-danger"">Disapproved</span>'
                        WHEN t.StatusID = 92 THEN '<span class=""badge badge-info"">Checked</span>'
                        WHEN t.StatusID = 102 THEN '<span class=""badge badge-success"">Verified</span>'
                        ELSE '<span class=""badge badge-warning"">Draft</span>'
                    END AS [Status],
                    t.StatusID,
                    FORMAT(t.CreateDate, 'dd/MM/yyyy') AS CreatedDateFormatted,
                    u.FullName AS CreatedBy
                FROM dbo.TblAccount_Transaction t
                LEFT JOIN dbo.TblClass c ON c.ClassID = t.ClassNameID
                LEFT JOIN dbo.TblUserLogin u ON u.UserLoginID = t.CreateByUserID
                WHERE t.Type = 'Journal Entry' 
                  AND t.SystemID = " + html.SystemID + @" 
                  AND t.WarehouseID = " + html.WarehouseID;

            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "center", Width = "50px" },
                new GridField() { Header = "Entry No", FieldName = "EntryNoLink", TextAlign = "left", Width = "150px" },
                new GridField() { Header = "Date", FieldName = "TransactionDate", TextAlign = "center", Width = "110px" },            
                new GridField(){ Header = "Memo", FieldName = "Memo", TextAlign = "left", Width = "150px"},
                new GridField() { Header = "Amount", FieldName = "TotalAmount", Format = "#,##0.00;-#,##0.00;-", TextAlign = "right", Width = "130px", IsTotalFooter = true },
                new GridField() { Header = "Created Date", FieldName = "CreatedDateFormatted", TextAlign = "center", Width = "120px" },
                new GridField() { Header = "Created By", FieldName = "CreatedBy", Width = "140px" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign = "center", Width = "110px" },
                new GridField() { Header = "Action", FieldName = "TransactionID", TextAlign = "center", Width = "90px", IsModifyField = true, DeleteWithTableName = "TblAccount_JournalEntries" }
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View("~/Views/JournalEntry/Index.cshtml");
        }

        [HttpGet]
        public ActionResult Create(string PrimaryFieldValue = "", int? id = null, string copyFrom = "", int? systemId = null, int? warehouseId = null)
        {
            HTML html = new HTML();
            var model = new JournalEntryVM();

            string sysID = GetSystemID(html, systemId);
            string whID = GetWarehouseID(html, warehouseId);

            int targetId = 0;
            if (id.HasValue && id.Value > 0)
            {
                targetId = id.Value;
            }
            else if (!string.IsNullOrEmpty(PrimaryFieldValue) && int.TryParse(PrimaryFieldValue, out int parsedId) && parsedId > 0)
            {
                targetId = parsedId;
            }

            if (targetId > 0)
            {
                model = LoadJournalEntry(targetId, html);
                if (copyFrom == "duplicate")
                {
                    model.JournalEntryID = 0;
                    model.JVNo = GetNextJVNumber(html, model.ClassID, model.SystemID, model.WarehouseID);
                    model.EntryDate = DateTime.Today;
                }
            }
            else
            {
                model.SystemID = int.TryParse(sysID, out int sId) ? (int?)sId : null;
                model.WarehouseID = int.TryParse(whID, out int wId) ? (int?)wId : null;
                model.JVNo = (model.ClassID.HasValue && model.ClassID.Value > 0) ? GetNextJVNumber(html, model.ClassID.Value, model.SystemID, model.WarehouseID) : "";
                model.EntryDate = DateTime.Today;
                model.Lines = new List<JournalEntryDetailVM>()
                {
                    new JournalEntryDetailVM { LineOrder = 1, Debit = 0, Credit = 0 },
                    new JournalEntryDetailVM { LineOrder = 2, Debit = 0, Credit = 0 }
                };
            }

            model.AvailableAccounts = GetChartOfAccounts(html, model.SystemID, model.WarehouseID);
            model.AvailableContacts = GetContacts(html, model.SystemID, model.WarehouseID);
            model.AvailableClasses = GetClasses(html, model.SystemID, model.WarehouseID);
            model.AvailableUsers = GetUsers(html, model.SystemID, model.WarehouseID);

            ViewBag.SystemID = sysID;
            ViewBag.WarehouseID = whID;
            ViewBag.CurrentUserID = html.UserID ?? "0";
            ViewBag.CurrentUserFullName = html.UserFullName ?? "";

            return View("~/Views/JournalEntry/Create.cshtml", model);
        }

        [HttpGet]
        public JsonResult GetNextJVNumberByClass(int? classId, int? systemId = null, int? warehouseId = null, string date = null)
        {
            HTML html = new HTML();
            DateTime? entryDate = null;
            if (!string.IsNullOrEmpty(date))
            {
                if (DateTime.TryParseExact(date, new[] { "dd-MM-yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDt))
                {
                    entryDate = parsedDt;
                }
                else if (DateTime.TryParse(date, out DateTime fallbackDt))
                {
                    entryDate = fallbackDt;
                }
            }
            string jvNo = GetNextJVNumber(html, classId, systemId, warehouseId, entryDate);
            return Json(new { success = true, JVNo = jvNo }, JsonRequestBehavior.AllowGet);
        }

        private bool ValidateJournalModel(JournalEntryVM model, out string errorMessage)
        {
            errorMessage = "";
            if (model == null)
            {
                errorMessage = "Invalid data payload.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(model.JVNo))
            {
                errorMessage = "Entry No is required.";
                return false;
            }

            HTML html = new HTML();
            string sysID = GetSystemID(html, model.SystemID);
            string whID = GetWarehouseID(html, model.WarehouseID);
            string duplicateQuery = @"
                SELECT COUNT(1) 
                FROM dbo.TblAccount_Transaction 
                WHERE InvoiceNo = '" + model.JVNo.Replace("'", "''") + @"' 
                  AND Type = 'Journal Entry'
                  AND SystemID = " + sysID + @" 
                  AND WarehouseID = " + whID + @"
                  AND TransactionID <> " + model.JournalEntryID;

            DataSet ds = html.LoadData(duplicateQuery, false, null);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                int count = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                if (count > 0)
                {
                    errorMessage = "Entry No '" + model.JVNo + "' already exists. Please use a unique Entry No.";
                    return false;
                }
            }

            if (model.EntryDate == DateTime.MinValue || model.EntryDate == default(DateTime))
            {
                errorMessage = "Valid Entry Date is required.";
                return false;
            }

            if (model.Lines == null || model.Lines.Count < 2)
            {
                errorMessage = "Journal Entry must contain at least 2 line items.";
                return false;
            }

            decimal totalDebit = 0;
            decimal totalCredit = 0;

            for (int i = 0; i < model.Lines.Count; i++)
            {
                var line = model.Lines[i];
                int lineNum = i + 1;

                if (line.ChartOfAccountID <= 0)
                {
                    errorMessage = "Line #" + lineNum + ": Please select a valid Account.";
                    return false;
                }

                if (line.Debit < 0 || line.Credit < 0)
                {
                    errorMessage = "Line #" + lineNum + ": Negative amounts are not allowed.";
                    return false;
                }

                if (line.Debit > 0 && line.Credit > 0)
                {
                    errorMessage = "Line #" + lineNum + ": A line cannot contain both Debit and Credit amounts.";
                    return false;
                }

                if (line.Debit == 0 && line.Credit == 0)
                {
                    errorMessage = "Line #" + lineNum + ": Please enter a Debit or Credit amount.";
                    return false;
                }

                totalDebit += line.Debit;
                totalCredit += line.Credit;
            }

            if (totalDebit <= 0 || totalCredit <= 0)
            {
                errorMessage = "Total Debit and Credit must be greater than $0.00.";
                return false;
            }

            if (Math.Abs(totalDebit - totalCredit) > 0.001m)
            {
                errorMessage = "Journal Entry is out of balance: Total Debit ($" + totalDebit.ToString("#,##0.00") + ") must equal Total Credit ($" + totalCredit.ToString("#,##0.00") + ").";
                return false;
            }

            return true;
        }

        [HttpPost]
        public async Task<JsonResult> SaveJournalEntry(JournalEntryVM model)
        {
            HTML html = new HTML();
            try
            {
                if (!ValidateJournalModel(model, out string errorMsg))
                {
                    return Json(new { Result = 1, success = false, Message = errorMsg });
                }

                int transId = SaveTransactionToDatabase(model, html);

                // Automatically send to Telegram check group
                var tgResult = await SendJournalEntryTelegram(transId);
                dynamic tgData = tgResult.Data;
                string extraMessage = tgData.success ? "" : (" (Telegram: " + tgData.message + ")");

                return Json(new { Result = 2, success = true, Message = "Journal Entry saved successfully!" + extraMessage, TransactionID = transId });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 1, success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> SaveAndReverse(JournalEntryVM model)
        {
            HTML html = new HTML();
            try
            {
                if (!ValidateJournalModel(model, out string errorMsg))
                {
                    return Json(new { Result = 1, success = false, Message = errorMsg });
                }

                int transId = SaveTransactionToDatabase(model, html);

                var reverseModel = new JournalEntryVM
                {
                    JournalEntryID = 0,
                    JVNo = GetNextJVNumber(html, model.ClassID),
                    EntryDate = model.EntryDate.AddDays(1),
                    Memo = "Reversal of " + model.JVNo + (!string.IsNullOrEmpty(model.Memo) ? ": " + model.Memo : ""),
                    IsAdjustment = true,
                    ClassID = model.ClassID,
                    TotalAmount = model.TotalAmount
                };

                int order = 1;
                foreach (var line in model.Lines)
                {
                    reverseModel.Lines.Add(new JournalEntryDetailVM
                    {
                        ChartOfAccountID = line.ChartOfAccountID,
                        Debit = line.Credit,
                        Credit = line.Debit,
                        LineMemo = "Reversal: " + line.LineMemo,
                        ClassID = line.ClassID,
                        NameID = line.NameID,
                        LineOrder = order++
                    });
                }

                int revTransId = SaveTransactionToDatabase(reverseModel, html);

                var tgResult = await SendJournalEntryTelegram(transId);
                dynamic tgData = tgResult.Data;
                string extraMessage = tgData.success ? "" : (" (Telegram: " + tgData.message + ")");

                return Json(new { Result = 2, success = true, Message = "Journal Entry and Reversal saved successfully!" + extraMessage, TransactionID = transId, ReversalID = revTransId });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 1, success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UpdateStatus(int id, int statusId, string clientUserFullName = "", int clientUserId = 0, long editChatId = 0, int editMessageId = 0)
        {
            HTML html = new HTML();
            try
            {
                string userFullName = !string.IsNullOrEmpty(html.UserFullName) ? html.UserFullName : (!string.IsNullOrEmpty(clientUserFullName) ? clientUserFullName : "Admin");
                string userId = !string.IsNullOrEmpty(html.UserID) && html.UserID != "0" ? html.UserID : (clientUserId > 0 ? clientUserId.ToString() : "1");
                int currentUid = 0;
                int.TryParse(userId, out currentUid);

                // Fetch transaction approvers to validate authority
                string query = "SELECT StatusID, CheckedByUserID, CheckedBy, VerifiedByUserID, VerifiedBy, ApprovedByUserID, ApprovedBy, TelegramMessageID, TelegramChatID FROM dbo.TblAccount_Transaction WHERE TransactionID = " + id;
                DataSet ds = html.LoadData(query, false, null);
                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                {
                    return Json(new { Result = 1, success = false, Message = "Transaction not found." });
                }

                DataRow row = ds.Tables[0].Rows[0];
                int assignedCheckedId = row["CheckedByUserID"] != DBNull.Value ? Convert.ToInt32(row["CheckedByUserID"]) : 0;
                string assignedCheckedName = row["CheckedBy"] != DBNull.Value ? row["CheckedBy"].ToString() : "";
                int assignedVerifiedId = row["VerifiedByUserID"] != DBNull.Value ? Convert.ToInt32(row["VerifiedByUserID"]) : 0;
                string assignedVerifiedName = row["VerifiedBy"] != DBNull.Value ? row["VerifiedBy"].ToString() : "";
                int assignedApprovedId = row["ApprovedByUserID"] != DBNull.Value ? Convert.ToInt32(row["ApprovedByUserID"]) : 0;
                string assignedApprovedName = row["ApprovedBy"] != DBNull.Value ? row["ApprovedBy"].ToString() : "";

                // If called from the web UI, the edit IDs will be 0. Populate them from the DB so we can sync back to Telegram.
                if (editChatId == 0 && row.Table.Columns.Contains("TelegramChatID") && row["TelegramChatID"] != DBNull.Value)
                {
                    long.TryParse(row["TelegramChatID"].ToString(), out editChatId);
                }
                if (editMessageId == 0 && row.Table.Columns.Contains("TelegramMessageID") && row["TelegramMessageID"] != DBNull.Value)
                {
                    int.TryParse(row["TelegramMessageID"].ToString(), out editMessageId);
                }

                // Validate authority: ONLY the user selected can take this action (check by ID or FullName)
                bool isOverrideUid = currentUid == 38 || currentUid == 126 || currentUid == 201;
                if (statusId == 92) // Check
                {
                    bool isMatched = isOverrideUid || (assignedCheckedId > 0 && currentUid == assignedCheckedId) ||
                                     (!string.IsNullOrEmpty(assignedCheckedName) && (assignedCheckedName.IndexOf(userFullName, StringComparison.OrdinalIgnoreCase) >= 0 || userFullName.IndexOf(assignedCheckedName, StringComparison.OrdinalIgnoreCase) >= 0));
                    if (assignedCheckedId > 0 && !isMatched && currentUid > 1)
                    {
                        return Json(new { Result = 1, success = false, Message = "Validation Failed: Only the selected user (" + assignedCheckedName + ") can Check this entry." });
                    }
                }
                else if (statusId == 102) // Verify
                {
                    bool isMatched = isOverrideUid || (assignedVerifiedId > 0 && currentUid == assignedVerifiedId) ||
                                     (!string.IsNullOrEmpty(assignedVerifiedName) && (assignedVerifiedName.IndexOf(userFullName, StringComparison.OrdinalIgnoreCase) >= 0 || userFullName.IndexOf(assignedVerifiedName, StringComparison.OrdinalIgnoreCase) >= 0));
                    if (assignedVerifiedId > 0 && !isMatched && currentUid > 1)
                    {
                        return Json(new { Result = 1, success = false, Message = "Validation Failed: Only the selected user (" + assignedVerifiedName + ") can Verify this entry." });
                    }
                }
                else if (statusId == 90) // Approve
                {
                    bool isMatched = isOverrideUid || (assignedApprovedId > 0 && currentUid == assignedApprovedId) ||
                                     (!string.IsNullOrEmpty(assignedApprovedName) && (assignedApprovedName.IndexOf(userFullName, StringComparison.OrdinalIgnoreCase) >= 0 || userFullName.IndexOf(assignedApprovedName, StringComparison.OrdinalIgnoreCase) >= 0));
                    if (assignedApprovedId > 0 && !isMatched && currentUid > 1)
                    {
                        return Json(new { Result = 1, success = false, Message = "Validation Failed: Only the selected user (" + assignedApprovedName + ") can Approve this entry." });
                    }
                }
                else if (statusId == 91) // Disapprove
                {
                    int currentStatus = row["StatusID"] != DBNull.Value ? Convert.ToInt32(row["StatusID"]) : 60;
                    int targetAssignedId = 0;
                    string targetAssignedName = "";

                    if (currentStatus == 92) // Checked stage -> Verifier or Approver can disapprove
                    {
                        targetAssignedId = assignedVerifiedId > 0 ? assignedVerifiedId : assignedApprovedId;
                        targetAssignedName = !string.IsNullOrEmpty(assignedVerifiedName) ? assignedVerifiedName : assignedApprovedName;
                    }
                    else if (currentStatus == 102) // Verified stage -> Approver can disapprove
                    {
                        targetAssignedId = assignedApprovedId;
                        targetAssignedName = assignedApprovedName;
                    }
                    else
                    {
                        targetAssignedId = assignedCheckedId > 0 ? assignedCheckedId : (assignedVerifiedId > 0 ? assignedVerifiedId : assignedApprovedId);
                        targetAssignedName = !string.IsNullOrEmpty(assignedCheckedName) ? assignedCheckedName : (!string.IsNullOrEmpty(assignedVerifiedName) ? assignedVerifiedName : assignedApprovedName);
                    }

                    bool isMatched = isOverrideUid || (targetAssignedId > 0 && currentUid == targetAssignedId) ||
                                     (!string.IsNullOrEmpty(targetAssignedName) && (targetAssignedName.IndexOf(userFullName, StringComparison.OrdinalIgnoreCase) >= 0 || userFullName.IndexOf(targetAssignedName, StringComparison.OrdinalIgnoreCase) >= 0));

                    if (targetAssignedId > 0 && !isMatched && currentUid > 1)
                    {
                        return Json(new { Result = 1, success = false, Message = "Validation Failed: Only the assigned reviewer (" + targetAssignedName + ") can Disapprove this entry." });
                    }
                }

                string sql = statusId == 92
                    ? @"UPDATE dbo.TblAccount_Transaction 
                            SET StatusID = 92, CheckedBy = '" + userFullName.Replace("'", "''") + @"', CheckedByUserID = " + userId + @", CheckedDate = GETDATE()
                            WHERE TransactionID = " + id + ";"
                    : statusId == 102
                        ? @"UPDATE dbo.TblAccount_Transaction 
                            SET StatusID = 102, VerifiedBy = '" + userFullName.Replace("'", "''") + @"', VerifiedByUserID = " + userId + @", VerifiedDate = GETDATE()
                            WHERE TransactionID = " + id + ";"
                        : statusId == 90
                        ? @"UPDATE dbo.TblAccount_Transaction 
                            SET StatusID = 90, ApprovedBy = '" + userFullName.Replace("'", "''") + @"', ApprovedByUserID = " + userId + @", ApprovalDate = GETDATE(), IsPosted = 1
                            WHERE TransactionID = " + id + ";"
                        : statusId == 91
                        ? @"UPDATE dbo.TblAccount_Transaction 
                            SET StatusID = 91, DisapprovedBy = '" + userFullName.Replace("'", "''") + @"', DisapprovedDate = GETDATE(), IsPosted = 0
                            WHERE TransactionID = " + id + ";"
                        : @"UPDATE dbo.TblAccount_Transaction 
                            SET StatusID = " + statusId + @", IsPosted = 0
                            WHERE TransactionID = " + id + ";";
                html.SaveChangeData(sql, false, null);
                await SendNotificationToTelegram(id, true, editChatId, editMessageId);
                return Json(new { Result = 2, success = true, Message = "Status updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 1, success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteEntry(int id)
        {
            HTML html = new HTML();
            try
            {
                string sql = @"
                    DELETE FROM dbo.TblAccount_JournalEntries WHERE TransactionID = " + id + @";
                    DELETE FROM dbo.TblAccount_Transaction WHERE TransactionID = " + id + @";
                    SELECT 2 AS Result, 'Journal Entry deleted successfully' AS Message;
                ";
                var res = html.SaveChangeData(sql, false, null);
                return Json(new { Result = 2, Message = "Deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 1, Message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult Invoice(int id)
        {
            HTML html = new HTML();
            var model = LoadJournalEntry(id, html);
            return View("~/Views/JournalEntry/Invoice.cshtml", model);
        }

        public (string BotToken, long ChatId, int TopicId, bool IsTopic) GetTelegramConfig(HTML html, string module = "JournalEntry", int? customSystemId = null, int? customWarehouseId = null, int? targetStatusId = null)
        {

            int systemId = customSystemId ?? Convert.ToInt32(html.SystemID);
            int warehouseId = customWarehouseId ?? Convert.ToInt32(html.WarehouseID);

            string sql;
            if (targetStatusId == 90)
            {
                // Looking for dedicated Approved topic (StatusID = 90)
                sql = @"
SELECT TOP 1 
    b.BotToken, 
    t.TelegramGroupID, 
    ISNULL(t.TelegramGroupTopicID, 0) AS TelegramGroupTopicID, 
    t.TelegramGroupType
FROM dbo.TblTelegramGroupNotification t
LEFT JOIN dbo.TblTelegramBots b 
    ON b.BotID = t.BotID
WHERE 
    t.Module = @Module
    AND t.SystemID = @SystemID
    AND (t.WarehouseID = @WarehouseID OR ISNULL(@WarehouseID, 0) = 0 OR ISNULL(t.WarehouseID, 0) = 0)
ORDER BY 
    CASE WHEN t.StatusID = 90 THEN 0 ELSE 1 END,
    CASE WHEN t.WarehouseID = @WarehouseID THEN 0 ELSE 1 END";
            }
            else
            {
                // Looking for Review/Draft topic (StatusID = 60, NULL, 0, or any review status, NOT 90)
                sql = @"
SELECT TOP 1 
    b.BotToken, 
    t.TelegramGroupID, 
    ISNULL(t.TelegramGroupTopicID, 0) AS TelegramGroupTopicID, 
    t.TelegramGroupType
FROM dbo.TblTelegramGroupNotification t
LEFT JOIN dbo.TblTelegramBots b 
    ON b.BotID = t.BotID
WHERE 
    t.Module = @Module
    AND t.SystemID = @SystemID
    AND (t.WarehouseID = @WarehouseID OR ISNULL(@WarehouseID, 0) = 0 OR ISNULL(t.WarehouseID, 0) = 0)
ORDER BY 
    CASE 
        WHEN t.StatusID = 60 THEN 0
        WHEN t.StatusID IS NULL OR t.StatusID = 0 THEN 1
        WHEN ISNULL(t.StatusID, 0) <> 90 THEN 2
        ELSE 3
    END,
    CASE WHEN t.WarehouseID = @WarehouseID THEN 0 ELSE 1 END";
            }

            var para = new Dictionary<string, string>
            {
                { "Module", module },
                { "SystemID", systemId.ToString() },
                { "WarehouseID", warehouseId.ToString() }
            };

            DataSet ds = html.LoadData(sql, false, para);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                string token = row["BotToken"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(token))
                {
                    token = html.GetBotTokenBySystemID(systemId);
                }
                long groupChatId = long.TryParse(row["TelegramGroupID"]?.ToString(), out var cid) ? cid : 0;
                int groupTopicId = int.TryParse(row["TelegramGroupTopicID"]?.ToString(), out var tid) ? tid : 0;
                bool isTopic = (row["TelegramGroupType"]?.ToString() == "Topic") || (groupTopicId > 0);

                return (token, groupChatId, groupTopicId, isTopic);
            }

            string fallbackBotToken = html.GetBotTokenBySystemID(systemId);
            return (fallbackBotToken, 0, 0, false); // No config found for this System/Warehouse
        }

        [HttpPost]
        public async Task<JsonResult> SendJournalEntryTelegram(int id)
        {
            return await SendNotificationToTelegram(id, false);
        }

        public async Task<JsonResult> SendNotificationToTelegram(int id, bool isUpdate = false, long editChatId = 0, int editMessageId = 0)
        {
            HTML html = new HTML();
            try
            {
                // ==============================================================================
                // BLOCK 1: LOAD DATA & INITIALIZE
                // Load the Journal Entry from the database.
                // ==============================================================================
                var model = LoadJournalEntry(id, html);
                if (model == null)
                    return Json(new { success = false, message = "Journal Entry not found." });

                int statusId = model.StatusID > 0 ? model.StatusID : 60;
                string dateStr = model.EntryDate.ToString("dd-MMM-yyyy");
                string memo = System.Net.WebUtility.HtmlEncode(model.Memo ?? "");
                string totalStr = model.TotalAmount.ToString("N2");

                // ==============================================================================
                // BLOCK 2: DETERMINE TARGET TELEGRAM GROUP
                // Dynamically look up the correct Telegram Group in the database based on System, 
                // ==============================================================================
                // BLOCK 2: DETERMINE TARGET TELEGRAM GROUP
                // Dynamically look up the correct Telegram Group in the database based on System, 
                // Warehouse, and Status. Status 102 (Verified) & 90 (Approved) route to Approve topic.
                // ==============================================================================
                string targetModule = "JournalEntry";
                int? filterStatusId = (statusId == 102 || statusId == 90) ? 90 : (int?)null;

                var tgConfig = GetTelegramConfig(html, targetModule, model.SystemID, model.WarehouseID, filterStatusId);

                // Fallback: If a specific Approval group (Status 90) isn't configured, send to the generic group to prevent crashing.
                if (string.IsNullOrEmpty(tgConfig.BotToken) || tgConfig.ChatId == 0)
                {
                    tgConfig = GetTelegramConfig(html, "JournalEntry", model.SystemID, model.WarehouseID, null);
                }

                if (string.IsNullOrEmpty(tgConfig.BotToken) || tgConfig.ChatId == 0)
                {
                    return Json(new { success = false, message = $"Telegram config missing in TblTelegramGroupNotification for Module '{targetModule}', SystemID {model.SystemID}, WarehouseID {model.WarehouseID}" });
                }

                // ==============================================================================
                // BLOCK 3: FETCH ASSIGNED USER FOR MENTIONS
                // Find the specific user who is supposed to action this step so we can tag them.
                // ==============================================================================
                string nextUserMention = "";
                string nextUserFullName = "";
                int? targetUserId = null;

                if (statusId == 60) targetUserId = model.CheckedByUserID; // Pending Check
                else if (statusId == 92) targetUserId = model.VerifiedByUserID; // Pending Verify
                else if (statusId == 102) targetUserId = model.ApprovedByUserID; // Pending Approve

                if (targetUserId.HasValue && targetUserId.Value > 0)
                {
                    DataSet uDs = html.LoadData("SELECT TelegramUserName, FullName FROM dbo.TblUserLogin WHERE UserLoginID = " + targetUserId.Value, false, null);
                    if (uDs != null && uDs.Tables.Count > 0 && uDs.Tables[0].Rows.Count > 0)
                    {
                        nextUserMention = uDs.Tables[0].Rows[0]["TelegramUserName"]?.ToString() ?? "";
                        nextUserFullName = uDs.Tables[0].Rows[0]["FullName"]?.ToString() ?? "";
                    }
                }

                if (!string.IsNullOrEmpty(nextUserMention) && !nextUserMention.StartsWith("@"))
                    nextUserMention = "@" + nextUserMention;

                // ==============================================================================
                // BLOCK 4: BUILD THE TELEGRAM MESSAGE CONTENT (COMPACT & CLEAN VIEW)
                // ==============================================================================
                Func<string, bool, string> buildMessage = (customHeader, isReviewFinal) =>
                {
                    var msg = new System.Text.StringBuilder();

                    string hTitle = customHeader;
                    if (string.IsNullOrEmpty(hTitle))
                    {
                        if (statusId == 60) hTitle = isUpdate ? "⚠️ <b>Updated Journal Entry</b>" : "🔔 <b>Journal Entry Review Request</b>";
                        else if (statusId == 92) hTitle = "🔔 <b>Journal Entry Review Request</b>";
                        else if (statusId == 102) hTitle = "🔔 <b>Journal Entry Approval Request</b>";
                        else if (statusId == 90) hTitle = "✅ <b>Journal Entry Approved</b>";
                        else if (statusId == 91) hTitle = "❌ <b>Journal Entry Disapproved</b>";
                        else hTitle = "🔔 <b>Journal Entry Notification</b>";
                    }

                    msg.AppendLine(hTitle);

                    if (!isReviewFinal && (statusId == 60 || statusId == 92 || statusId == 102))
                    {
                        if (!string.IsNullOrEmpty(nextUserFullName) || !string.IsNullOrEmpty(nextUserMention))
                        {
                            string safeName = System.Net.WebUtility.HtmlEncode(string.IsNullOrEmpty(nextUserFullName) ? nextUserMention : nextUserFullName);
                            msg.AppendLine($"👤 Assigned to: <b>{safeName}</b> {nextUserMention}".Trim());
                        }
                    }

                    msg.AppendLine();
                    msg.AppendLine($"📋 <b>JV No:</b> <code>{System.Net.WebUtility.HtmlEncode(model.JVNo)}</code> | 📅 {dateStr}");
                    msg.AppendLine($"💰 <b>Total Amount:</b> <b>${totalStr}</b>");
                    if (!string.IsNullOrEmpty(memo)) msg.AppendLine($"📝 <b>Memo:</b> <i>{memo}</i>");

                    msg.AppendLine();
                    msg.AppendLine("📒 <b>Accounting Lines:</b>");
                    var sortedLines = model.Lines.OrderByDescending(l => l.Debit).ThenByDescending(l => l.Credit);
                    foreach (var line in sortedLines)
                    {
                        string safeAccountName = System.Net.WebUtility.HtmlEncode(line.AccountName ?? "");
                        if (line.Debit > 0)
                            msg.AppendLine($"🟢 <b>DR:</b> ${line.Debit:N2} | <code>{System.Net.WebUtility.HtmlEncode(line.AccountNo)}</code> - {safeAccountName}");
                        else
                            msg.AppendLine($"🔴 <b>CR:</b> ${line.Credit:N2} | <code>{System.Net.WebUtility.HtmlEncode(line.AccountNo)}</code> - {safeAccountName}");
                    }

                    msg.AppendLine();
                    msg.AppendLine("✍️ <b>Signatures:</b>");
                    msg.AppendLine($"• Prepared: <i>{System.Net.WebUtility.HtmlEncode(model.CreatedBy)}</i>");

                    if (!string.IsNullOrEmpty(model.CheckedBy))
                    {
                        string cDate = model.CheckedDate.HasValue ? model.CheckedDate.Value.ToString("dd-MM-yyyy HH:mm") : "";
                        msg.AppendLine($"• Checked: <i>{model.CheckedBy}</i>" + (!string.IsNullOrEmpty(cDate) ? $" ({cDate})" : ""));
                    }
                    if (!string.IsNullOrEmpty(model.VerifiedBy))
                    {
                        string vDate = model.VerifiedDate.HasValue ? model.VerifiedDate.Value.ToString("dd-MM-yyyy HH:mm") : "";
                        msg.AppendLine($"• Verified: <i>{model.VerifiedBy}</i>" + (!string.IsNullOrEmpty(vDate) ? $" ({vDate})" : ""));
                    }
                    if (!string.IsNullOrEmpty(model.ApprovedBy))
                    {
                        string aDate = model.ApprovalDate.HasValue ? model.ApprovalDate.Value.ToString("dd-MM-yyyy HH:mm") : "";
                        msg.AppendLine($"• Approved: <i>{model.ApprovedBy}</i>" + (!string.IsNullOrEmpty(aDate) ? $" ({aDate})" : ""));
                    }
                    if (!string.IsNullOrEmpty(model.DisapprovedBy))
                    {
                        string dDate = model.DisapprovedDate.HasValue ? model.DisapprovedDate.Value.ToString("dd-MM-yyyy HH:mm") : "";
                        msg.AppendLine($"• Disapproved: <i>{model.DisapprovedBy}</i>" + (!string.IsNullOrEmpty(dDate) ? $" ({dDate})" : ""));
                    }

                    return msg.ToString().TrimEnd();
                };

                string mainMessageText = buildMessage(null, false);

                // ==============================================================================
                // BLOCK 5: BUILD INLINE KEYBOARD
                // Attach the correct action buttons based on the current step in the workflow.
                // ==============================================================================
                InlineKeyboardMarkup inlineKeyboard = null;
                if (statusId == 60)
                {
                    inlineKeyboard = new InlineKeyboardMarkup(new[] { new[] {
                        InlineKeyboardButton.WithCallbackData("✅ Check", $"CheckJE~{id}~{tgConfig.ChatId}~{nextUserMention}"),
                        InlineKeyboardButton.WithCallbackData("❌ Disapprove", $"DisapproveJE~{id}~{tgConfig.ChatId}~{nextUserMention}")
                    }});
                }
                else if (statusId == 92)
                {
                    inlineKeyboard = new InlineKeyboardMarkup(new[] { new[] {
                        InlineKeyboardButton.WithCallbackData("🔍 Verify", $"VerifyJE~{id}~{tgConfig.ChatId}~{nextUserMention}"),
                        InlineKeyboardButton.WithCallbackData("❌ Disapprove", $"DisapproveJE~{id}~{tgConfig.ChatId}~{nextUserMention}")
                    }});
                }
                else if (statusId == 102)
                {
                    inlineKeyboard = new InlineKeyboardMarkup(new[] { new[] {
                        InlineKeyboardButton.WithCallbackData("✅ Approve", $"ApproveJE~{id}~{tgConfig.ChatId}~{nextUserMention}"),
                        InlineKeyboardButton.WithCallbackData("❌ Disapprove", $"DisapproveJE~{id}~{tgConfig.ChatId}~{nextUserMention}")
                    }});
                }

                NotificationTelegram notification = new NotificationTelegram();

                // If this is a status update, look up the existing Telegram message ID / Chat ID if not provided
                if (isUpdate && (editMessageId == 0 || editChatId == 0))
                {
                    DataSet dsPrev = html.LoadData($"SELECT TelegramMessageID, TelegramChatID FROM dbo.TblAccount_Transaction WHERE TransactionID = {id}", false, null);
                    if (dsPrev != null && dsPrev.Tables.Count > 0 && dsPrev.Tables[0].Rows.Count > 0)
                    {
                        var rPrev = dsPrev.Tables[0].Rows[0];
                        if (editMessageId == 0 && rPrev["TelegramMessageID"] != DBNull.Value) int.TryParse(rPrev["TelegramMessageID"].ToString(), out editMessageId);
                        if (editChatId == 0 && rPrev["TelegramChatID"] != DBNull.Value) long.TryParse(rPrev["TelegramChatID"].ToString(), out editChatId);
                    }
                }

                // ==============================================================================
                // CASE 1: UPDATE EXISTING MESSAGE IN-PLACE (NO DUPLICATE MESSAGES)
                // When status changes, edit the original message directly in-place:
                // - Updates the status text and signatures
                // - Updates the buttons to the next workflow step (or removes buttons when Verified/Approved/Disapproved)
                // ==============================================================================
                if (isUpdate && editMessageId > 0)
                {
                    long targetEditChatId = editChatId != 0 ? editChatId : tgConfig.ChatId;
                    try
                    {
                        if (statusId == 102)
                        {
                            // In the review topic (Journal Entry NSM):
                            // Update review message to show it has been Verified and remove action buttons.
                            string reviewVerifiedMsg = buildMessage("🔍 <b>Journal Entry Verified</b>", true);
                            await notification.EditTelegramMessage(
                                messageId: editMessageId,
                                msgText: reviewVerifiedMsg,
                                inlineKeyboard: null, // removes buttons from review topic
                                chatId: targetEditChatId,
                                topicID: 0,
                                newBotClient: tgConfig.BotToken
                            );
                        }
                        else
                        {
                            await notification.EditTelegramMessage(
                                messageId: editMessageId,
                                msgText: mainMessageText,
                                inlineKeyboard: inlineKeyboard, // updates to next action buttons, or null if approved/disapproved
                                chatId: targetEditChatId,
                                topicID: 0,
                                newBotClient: tgConfig.BotToken
                            );
                        }
                    }
                    catch { }

                    // If NOT status 102 (Verified), we're done (the original message was updated).
                    // If it IS status 102 (Verified), proceed below to send the approval request to the dedicated Approve topic!
                    if (statusId != 102)
                    {
                        return Json(new { success = true, message = "Notification updated successfully in Telegram." });
                    }
                }

                // ==============================================================================
                // CASE 2: SEND INITIAL NEW MESSAGE (Initial Send or moving to Approve topic at status 102)
                // ==============================================================================
                if (tgConfig.IsTopic && tgConfig.TopicId > 0)
                {
                    var resTopic = await notification.NotificationTelegramBotToTopic(
                        MsgText: mainMessageText,
                        chatId: tgConfig.ChatId,
                        topicID: tgConfig.TopicId,
                        inlineKeyboard: inlineKeyboard,
                        MessageID: 0,
                        TelegramMessage: "",
                        Files: "",
                        Caption: "",
                        EditMessage: false,
                        newBotClient: tgConfig.BotToken
                    );

                    if (resTopic != null && resTopic.Result == ResultType.Successful && resTopic.Value != null)
                    {
                        long messageId = 0;
                        if (long.TryParse(resTopic.Value.ToString(), out messageId) && messageId > 0)
                        {
                            string updateSql = $@"UPDATE dbo.TblAccount_Transaction 
                                                SET TelegramMessageID = {messageId},
                                                    TelegramChatID = {tgConfig.ChatId},
                                                    TelegramSentDate = GETDATE()
                                                WHERE TransactionID = {id};";
                            html.SaveChangeData(updateSql, false, null);
                        }
                    }
                }
                else
                {
                    var resNormal = await notification.NotificationTelegramBot(
                        MsgText: mainMessageText,
                        chatId: tgConfig.ChatId,
                        DestinationFolder: "",
                        PDFFile: "",
                        Caption: "",
                        IsSendFileWithCaption: false,
                        replyMarkupInlineKeyboardMarkup: inlineKeyboard != null,
                        DBID: id,
                        MessageID: 0,
                        TelegramMessage: "",
                        inlineKeyboard: inlineKeyboard,
                        edit: false,
                        messageThreadId: tgConfig.TopicId,
                        DeleteMessageID: "",
                        newBotClient: tgConfig.BotToken
                    );

                    if (resNormal != null && resNormal.Result == ResultType.Successful && resNormal.Value != null)
                    {
                        long messageId = 0;
                        if (long.TryParse(resNormal.Value.ToString(), out messageId) && messageId > 0)
                        {
                            string updateSql = $@"UPDATE dbo.TblAccount_Transaction 
                                                SET TelegramMessageID = {messageId},
                                                    TelegramChatID = {tgConfig.ChatId},
                                                    TelegramSentDate = GETDATE()
                                                WHERE TransactionID = {id};";
                            html.SaveChangeData(updateSql, false, null);
                        }
                    }
                }

                return Json(new { success = true, message = "Notification sent successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Telegram error: " + ex.Message + (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "") });
            }
        }

        private string GetSystemID(HTML html, int? modelSysId = null)
        {
            if (modelSysId.HasValue && modelSysId.Value > 0)
                return modelSysId.Value.ToString();

            if (!string.IsNullOrEmpty(html.SystemID) && html.SystemID != "0")
                return html.SystemID;

            var ctx = System.Web.HttpContext.Current;
            if (ctx != null)
            {
                if (ctx.Session != null && ctx.Session["SystemID"] != null && !string.IsNullOrEmpty(ctx.Session["SystemID"].ToString()) && ctx.Session["SystemID"].ToString() != "0")
                    return ctx.Session["SystemID"].ToString();

                if (ctx.Request != null)
                {
                    if (!string.IsNullOrEmpty(ctx.Request["SystemID"]) && ctx.Request["SystemID"] != "0")
                        return ctx.Request["SystemID"];

                    if (ctx.Request.Cookies["SystemID"] != null && !string.IsNullOrEmpty(ctx.Request.Cookies["SystemID"].Value) && ctx.Request.Cookies["SystemID"].Value != "0")
                        return ctx.Request.Cookies["SystemID"].Value;

                    if (ctx.Request.UrlReferrer != null)
                    {
                        var q = System.Web.HttpUtility.ParseQueryString(ctx.Request.UrlReferrer.Query);
                        if (!string.IsNullOrEmpty(q["SystemID"]) && q["SystemID"] != "0")
                            return q["SystemID"];
                    }
                }
            }
            return "0";
        }

        private string GetWarehouseID(HTML html, int? modelWhId = null)
        {
            if (modelWhId.HasValue && modelWhId.Value > 0)
                return modelWhId.Value.ToString();

            if (!string.IsNullOrEmpty(html.WarehouseID) && html.WarehouseID != "0")
                return html.WarehouseID;

            var ctx = System.Web.HttpContext.Current;
            if (ctx != null)
            {
                if (ctx.Session != null && ctx.Session["WarehouseID"] != null && !string.IsNullOrEmpty(ctx.Session["WarehouseID"].ToString()) && ctx.Session["WarehouseID"].ToString() != "0")
                    return ctx.Session["WarehouseID"].ToString();

                if (ctx.Request != null)
                {
                    if (!string.IsNullOrEmpty(ctx.Request["WarehouseID"]) && ctx.Request["WarehouseID"] != "0")
                        return ctx.Request["WarehouseID"];

                    if (ctx.Request.Cookies["WarehouseID"] != null && !string.IsNullOrEmpty(ctx.Request.Cookies["WarehouseID"].Value) && ctx.Request.Cookies["WarehouseID"].Value != "0")
                        return ctx.Request.Cookies["WarehouseID"].Value;

                    if (ctx.Request.UrlReferrer != null)
                    {
                        var q = System.Web.HttpUtility.ParseQueryString(ctx.Request.UrlReferrer.Query);
                        if (!string.IsNullOrEmpty(q["WarehouseID"]) && q["WarehouseID"] != "0")
                            return q["WarehouseID"];
                    }
                }
            }
            return "0";
        }

        private string IncrementNumberSuffix(string jvNo)
        {
            var match = System.Text.RegularExpressions.Regex.Match(jvNo, @"\d+$");
            if (match.Success)
            {
                string numStr = match.Value;
                int length = numStr.Length;
                int nextNum = int.Parse(numStr) + 1;
                string newNumStr = nextNum.ToString().PadLeft(length, '0');
                return jvNo.Substring(0, match.Index) + newNumStr;
            }
            return jvNo + "-1";
        }

        private string GetNextJVNumber(HTML html, int? classId = null, int? systemId = null, int? warehouseId = null, DateTime? entryDate = null)
        {
            if (!classId.HasValue || classId.Value <= 0)
            {
                return "";
            }

            try
            {
                string sysID = GetSystemID(html, systemId);
                string whID = GetWarehouseID(html, warehouseId);
                string domainID = html.DomainID ?? "";

                DateTime targetDate = entryDate.HasValue && entryDate.Value != default(DateTime) ? entryDate.Value : DateTime.Today;
                string yyyy = targetDate.ToString("yyyy");
                string yy = targetDate.ToString("yy");
                string mm = targetDate.ToString("MM");
                string dd = targetDate.ToString("dd");

                string sql = @"
                    DECLARE @Prefix NVARCHAR(100) = '';
                    DECLARE @Module NVARCHAR(50) = 'JE';
                    
                    SELECT @Prefix = ISNULL(PrefixJE, ''),
                           @Module = 'JE - ' + REPLACE(ClassName, ' ', '')
                    FROM dbo.TblClass 
                    WHERE ClassID = " + classId.Value + @";

                    IF (@Prefix = '' OR @Prefix IS NULL)
                    BEGIN
                        SELECT TOP 1 @Prefix = Prefix, @Module = Module 
                        FROM dbo.TblPrefix 
                        WHERE Module IN ('JE', 'Journal Entry', 'JournalEntry', 'JV') 
                          AND (SystemID = " + sysID + @" OR " + sysID + @" = 0)
                          AND (WarehouseID = " + whID + @" OR " + whID + @" = 0)
                        ORDER BY PrefixID DESC;
                    END

                    IF (@Prefix = '' OR @Prefix IS NULL)
                    BEGIN
                        SET @Prefix = 'JE-{yy}{MM}-{000}';
                    END

                    SET @Prefix = REPLACE(@Prefix, '{yyyy}', '" + yyyy + @"');
                    SET @Prefix = REPLACE(@Prefix, '{yy}', '" + yy + @"');
                    SET @Prefix = REPLACE(@Prefix, '{MM}', '" + mm + @"');
                    SET @Prefix = REPLACE(@Prefix, '{dd}', '" + dd + @"');

                    EXEC [SP_SHOW_PREFIX] @Prefix, @Module, " + sysID + @", " + whID + @", '" + domainID + @"';
                ";
                DataSet ds = html.LoadData(sql, false, null);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    string jvNo = ds.Tables[0].Rows[0][0].ToString();

                    // Check for duplicate number collisions in TblAccount_Transaction and auto-increment suffix
                    while (true)
                    {
                        string checkSql = "SELECT COUNT(1) FROM dbo.TblAccount_Transaction WHERE InvoiceNo = '" + jvNo.Replace("'", "''") + "' AND SystemID = " + sysID + " AND WarehouseID = " + whID;
                        DataSet dsCheck = html.LoadData(checkSql, false, null);
                        if (dsCheck != null && dsCheck.Tables.Count > 0 && dsCheck.Tables[0].Rows.Count > 0 && Convert.ToInt32(dsCheck.Tables[0].Rows[0][0]) > 0)
                        {
                            jvNo = IncrementNumberSuffix(jvNo);
                        }
                        else
                        {
                            break;
                        }
                    }
                    return jvNo;
                }
            }
            catch { }

            return "";
        }

        private List<AccountDropdownItem> GetChartOfAccounts(HTML html, int? systemId = null, int? warehouseId = null)
        {
            List<AccountDropdownItem> list = new List<AccountDropdownItem>();
            string sysID = GetSystemID(html, systemId);
            string whID = GetWarehouseID(html, warehouseId);
            string domainID = html.DomainID ?? "";

            string sql = @"
                SELECT 
                    ChartOfAccountID, 
                    AccountNo, 
                    AccountName, 
                    AccountNo + ' - ' + AccountName AS DisplayName 
                FROM dbo.TblChartOfAccount 
                WHERE (SystemID = " + sysID + @" OR " + sysID + @" = 0) 
                  AND (WarehouseID = " + whID + @" OR " + whID + @" = 0)
                  AND ('" + domainID + @"' = '' OR DomainID LIKE '%" + domainID + @"%')
                ORDER BY AccountNo, AccountName";

            DataSet ds = html.LoadData(sql, false, null);
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    list.Add(new AccountDropdownItem
                    {
                        ChartOfAccountID = Convert.ToInt32(r["ChartOfAccountID"]),
                        AccountNo = r["AccountNo"].ToString(),
                        AccountName = r["AccountName"].ToString(),
                        DisplayName = r["DisplayName"].ToString()
                    });
                }
            }
            return list;
        }

        private List<ContactDropdownItem> GetContacts(HTML html, int? systemId = null, int? warehouseId = null)
        {
            List<ContactDropdownItem> list = new List<ContactDropdownItem>();
            string sysID = GetSystemID(html, systemId);
            string whID = GetWarehouseID(html, warehouseId);
            string domainID = html.DomainID ?? "";

            string sql = @"
                SELECT CustomerID AS ContactID, COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin, 'Customer') AS ContactName, 'Customer' AS ContactType
                FROM dbo.TblCustomer 
                WHERE (SystemID = " + sysID + @" OR " + sysID + @" = 0) 
                  AND (WarehouseID = " + whID + @" OR " + whID + @" = 0)
                  AND ('" + domainID + @"' = '' OR DomainID LIKE '%" + domainID + @"%')
                UNION ALL
                SELECT VendorID AS ContactID, VendorName AS ContactName, 'Vendor' AS ContactType
                FROM dbo.TblVendor 
                WHERE (SystemID = " + sysID + @" OR " + sysID + @" = 0) 
                  AND (WarehouseID = " + whID + @" OR " + whID + @" = 0)
                  AND ('" + domainID + @"' = '' OR DomainID LIKE '%" + domainID + @"%')
                ORDER BY ContactName";

            DataSet ds = html.LoadData(sql, false, null);
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    list.Add(new ContactDropdownItem
                    {
                        ContactID = Convert.ToInt32(r["ContactID"]),
                        ContactName = r["ContactName"].ToString(),
                        ContactType = r["ContactType"].ToString()
                    });
                }
            }
            return list;
        }

        private List<ClassDropdownItem> GetClasses(HTML html, int? systemId = null, int? warehouseId = null)
        {
            List<ClassDropdownItem> list = new List<ClassDropdownItem>();
            string sysID = GetSystemID(html, systemId);
            string whID = GetWarehouseID(html, warehouseId);

            string sql = @"
                SELECT ClassID, ClassName 
                FROM dbo.TblClass 
                WHERE (SystemID = " + sysID + @" OR " + sysID + @" = 0) 
                  AND (WarehouseID = " + whID + @" OR " + whID + @" = 0)
                ORDER BY ClassName";

            DataSet ds = html.LoadData(sql, false, null);
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    list.Add(new ClassDropdownItem
                    {
                        ClassID = Convert.ToInt32(r["ClassID"]),
                        ClassName = r["ClassName"].ToString()
                    });
                }
            }
            return list;
        }

        private List<UserDropdownItem> GetUsers(HTML html, int? systemId = null, int? warehouseId = null)
        {
            List<UserDropdownItem> list = new List<UserDropdownItem>();
            string sysID = GetSystemID(html, systemId);
            string whID = GetWarehouseID(html, warehouseId);

            string sql = @"
                SELECT DISTINCT u.UserLoginID, u.FullName, ISNULL(u.Job, '') AS Job
                FROM dbo.TblUserLogin u 
                INNER JOIN dbo.TblSetUserAccessSystemAndWarehouse ts ON ts.UserLoginID = u.UserLoginID 
                WHERE u.StatusID = 3 AND (ts.SystemID = " + sysID + @" OR " + sysID + @" = 0) AND (ts.WarehouseID = " + whID + @" OR " + whID + @" = 0)
                ORDER BY u.FullName";

            DataSet ds = html.LoadData(sql, false, null);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    list.Add(new UserDropdownItem
                    {
                        UserLoginID = Convert.ToInt32(r["UserLoginID"]),
                        FullName = r["FullName"].ToString(),
                        Job = r["Job"].ToString()
                    });
                }
            }
            else
            {
                string fallbackSql = "SELECT UserLoginID, FullName, ISNULL(Job, '') AS Job FROM dbo.TblUserLogin WHERE StatusID = 3 ORDER BY FullName";
                DataSet dsFb = html.LoadData(fallbackSql, false, null);
                if (dsFb != null && dsFb.Tables.Count > 0)
                {
                    foreach (DataRow r in dsFb.Tables[0].Rows)
                    {
                        list.Add(new UserDropdownItem
                        {
                            UserLoginID = Convert.ToInt32(r["UserLoginID"]),
                            FullName = r["FullName"].ToString(),
                            Job = r["Job"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        private JournalEntryVM LoadJournalEntry(int transactionId, HTML html)
        {
            var vm = new JournalEntryVM();
            string sqlHeader = @"
                SELECT 
                    t.TransactionID, 
                    t.InvoiceNo, 
                    t.TransactionDate, 
                    t.Description, 
                    t.Amount,
                    t.ClassNameID,
                    t.SystemID,
                    t.WarehouseID,
                    ISNULL(t.StatusID, 60) AS StatusID,
                    ISNULL(t.IsAdjustment, 0) AS IsAdjustment,
                    t.CheckedByUserID,
                    t.CheckedBy,
                    t.CheckedDate,
                    t.VerifiedByUserID,
                    t.VerifiedBy,
                    t.VerifiedDate,
                    t.ApprovedByUserID,
                    t.ApprovedBy,
                    t.ApprovalDate,
                    t.DisapprovedBy,
                    t.DisapprovedDate,
                    u.FullName AS CreatedBy
                FROM dbo.TblAccount_Transaction t
                LEFT JOIN dbo.TblUserLogin u ON u.UserLoginID = t.CreateByUserID
                WHERE t.TransactionID = " + transactionId;

            DataSet dsHeader = html.LoadData(sqlHeader, false, null);
            if (dsHeader != null && dsHeader.Tables.Count > 0 && dsHeader.Tables[0].Rows.Count > 0)
            {
                DataRow r = dsHeader.Tables[0].Rows[0];
                vm.CreatedBy = r.Table.Columns.Contains("CreatedBy") && r["CreatedBy"] != DBNull.Value ? r["CreatedBy"].ToString() : null;
                vm.JournalEntryID = Convert.ToInt32(r["TransactionID"]);
                vm.JVNo = r["InvoiceNo"].ToString();
                vm.EntryDate = Convert.ToDateTime(r["TransactionDate"]);
                vm.Memo = r["Description"] != DBNull.Value ? r["Description"].ToString() : "";
                vm.TotalAmount = r["Amount"] != DBNull.Value ? Convert.ToDecimal(r["Amount"]) : 0;
                vm.IsAdjustment = r.Table.Columns.Contains("IsAdjustment") && r["IsAdjustment"] != DBNull.Value && Convert.ToBoolean(r["IsAdjustment"]);
                if (r.Table.Columns.Contains("ClassNameID") && r["ClassNameID"] != DBNull.Value)
                {
                    vm.ClassID = Convert.ToInt32(r["ClassNameID"]);
                }
                if (r.Table.Columns.Contains("SystemID") && r["SystemID"] != DBNull.Value)
                {
                    vm.SystemID = Convert.ToInt32(r["SystemID"]);
                }
                if (r.Table.Columns.Contains("WarehouseID") && r["WarehouseID"] != DBNull.Value)
                {
                    vm.WarehouseID = Convert.ToInt32(r["WarehouseID"]);
                }
                if (r.Table.Columns.Contains("StatusID") && r["StatusID"] != DBNull.Value)
                {
                    vm.StatusID = Convert.ToInt32(r["StatusID"]);
                }
                vm.CheckedByUserID = r.Table.Columns.Contains("CheckedByUserID") && r["CheckedByUserID"] != DBNull.Value ? (int?)Convert.ToInt32(r["CheckedByUserID"]) : null;
                vm.CheckedBy = r.Table.Columns.Contains("CheckedBy") && r["CheckedBy"] != DBNull.Value ? r["CheckedBy"].ToString() : null;
                vm.CheckedDate = r.Table.Columns.Contains("CheckedDate") && r["CheckedDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["CheckedDate"]) : null;
                vm.VerifiedByUserID = r.Table.Columns.Contains("VerifiedByUserID") && r["VerifiedByUserID"] != DBNull.Value ? (int?)Convert.ToInt32(r["VerifiedByUserID"]) : null;
                vm.VerifiedBy = r.Table.Columns.Contains("VerifiedBy") && r["VerifiedBy"] != DBNull.Value ? r["VerifiedBy"].ToString() : null;
                vm.VerifiedDate = r.Table.Columns.Contains("VerifiedDate") && r["VerifiedDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["VerifiedDate"]) : null;
                vm.ApprovedByUserID = r.Table.Columns.Contains("ApprovedByUserID") && r["ApprovedByUserID"] != DBNull.Value ? (int?)Convert.ToInt32(r["ApprovedByUserID"]) : null;
                vm.ApprovedBy = r.Table.Columns.Contains("ApprovedBy") && r["ApprovedBy"] != DBNull.Value ? r["ApprovedBy"].ToString() : null;
                vm.ApprovalDate = r.Table.Columns.Contains("ApprovalDate") && r["ApprovalDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["ApprovalDate"]) : null;
                vm.DisapprovedBy = r.Table.Columns.Contains("DisapprovedBy") && r["DisapprovedBy"] != DBNull.Value ? r["DisapprovedBy"].ToString() : null;
                vm.DisapprovedDate = r.Table.Columns.Contains("DisapprovedDate") && r["DisapprovedDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["DisapprovedDate"]) : null;
            }

            string sqlDetails = @"
                SELECT 
                    je.EntryID,
                    je.AccountID,
                    coa.AccountNo,
                    coa.AccountName,
                    je.Description AS LineMemo,
                    ISNULL(je.Debit, 0) AS Debit,
                    ISNULL(je.Credit, 0) AS Credit,
                    je.CustomerID,
                    COALESCE(NULLIF(c.CustomerName, ''), NULLIF(c.CustomerNameLatin, ''), v.VendorName, '') AS ContactName
                FROM dbo.TblAccount_JournalEntries je
                LEFT JOIN dbo.TblChartOfAccount coa ON coa.ChartOfAccountID = je.AccountID
                LEFT JOIN dbo.TblCustomer c ON c.CustomerID = je.CustomerID
                LEFT JOIN dbo.TblVendor v ON v.VendorID = je.CustomerID
                WHERE je.TransactionID = " + transactionId + @"
                ORDER BY je.EntryID ASC";

            DataSet dsDetails = html.LoadData(sqlDetails, false, null);
            if (dsDetails != null && dsDetails.Tables.Count > 0)
            {
                int order = 1;
                foreach (DataRow dr in dsDetails.Tables[0].Rows)
                {
                    vm.Lines.Add(new JournalEntryDetailVM
                    {
                        DetailID = Convert.ToInt32(dr["EntryID"]),
                        JournalEntryID = transactionId,
                        ChartOfAccountID = Convert.ToInt32(dr["AccountID"]),
                        AccountNo = dr["AccountNo"] != DBNull.Value ? dr["AccountNo"].ToString() : "",
                        AccountName = dr["AccountName"] != DBNull.Value ? dr["AccountName"].ToString() : "",
                        LineMemo = dr["LineMemo"] != DBNull.Value ? dr["LineMemo"].ToString() : "",
                        Debit = Convert.ToDecimal(dr["Debit"]),
                        Credit = Convert.ToDecimal(dr["Credit"]),
                        NameID = dr["CustomerID"] != DBNull.Value ? (int?)Convert.ToInt32(dr["CustomerID"]) : null,
                        Name = dr["ContactName"] != DBNull.Value ? dr["ContactName"].ToString() : "",
                        LineOrder = order++
                    });
                }
            }

            return vm;
        }

        private int SaveTransactionToDatabase(JournalEntryVM model, HTML html)
        {
            int transactionID = model.JournalEntryID;
            string dateStr = model.EntryDate.ToString("yyyy-MM-dd");
            string safeMemo = (model.Memo ?? "").Replace("'", "''");
            string safeJVNo = (model.JVNo ?? "").Replace("'", "''");

            string sysID = GetSystemID(html, model.SystemID);
            string whID = GetWarehouseID(html, model.WarehouseID);
            string userID = !string.IsNullOrEmpty(html.UserID) && html.UserID != "0" ? html.UserID : "1";

            decimal totalAmt = model.TotalAmount > 0 ? model.TotalAmount : model.Lines.Sum(l => l.Debit);
            string totalAmtStr = totalAmt.ToString(System.Globalization.CultureInfo.InvariantCulture);
            int? classId = model.ClassID ?? model.Lines.FirstOrDefault(l => l.ClassID.HasValue && l.ClassID.Value > 0)?.ClassID;
            string classIdVal = classId.HasValue ? classId.Value.ToString() : "NULL";

            string checkedByIdVal = model.CheckedByUserID.HasValue && model.CheckedByUserID.Value > 0 ? model.CheckedByUserID.Value.ToString() : "NULL";
            string safeCheckedBy = !string.IsNullOrEmpty(model.CheckedBy) ? "'" + model.CheckedBy.Replace("'", "''") + "'" : "NULL";

            string verifiedByIdVal = model.VerifiedByUserID.HasValue && model.VerifiedByUserID.Value > 0 ? model.VerifiedByUserID.Value.ToString() : "NULL";
            string safeVerifiedBy = !string.IsNullOrEmpty(model.VerifiedBy) ? "'" + model.VerifiedBy.Replace("'", "''") + "'" : "NULL";

            string approvedByIdVal = model.ApprovedByUserID.HasValue && model.ApprovedByUserID.Value > 0 ? model.ApprovedByUserID.Value.ToString() : "NULL";
            string safeApprovedBy = !string.IsNullOrEmpty(model.ApprovedBy) ? "'" + model.ApprovedBy.Replace("'", "''") + "'" : "NULL";

            int statusVal = model.StatusID > 0 ? model.StatusID : 60;
            string isAdjustmentVal = model.IsAdjustment ? "1" : "0";

            if (transactionID > 0)
            {
                // STANDARD ACCOUNTING PRACTICE: If a transaction is edited, 
                // it MUST be reverted to Pending Check (60) and un-posted.
                string sqlUpdate = @"
                    UPDATE dbo.TblAccount_Transaction 
                    SET 
                        InvoiceNo = '" + safeJVNo + @"',
                        TransactionDate = '" + dateStr + @"',
                        Description = '" + safeMemo + @"',
                        Amount = " + totalAmtStr + @",
                        ClassNameID = " + classIdVal + @",
                        IsAdjustment = " + isAdjustmentVal + @",
                        StatusID = 60,
                        IsPosted = 0,
                        CheckedByUserID = " + checkedByIdVal + @",
                        VerifiedByUserID = " + verifiedByIdVal + @",
                        ApprovedByUserID = " + approvedByIdVal + @",
                        CheckedBy = NULL,
                        CheckedDate = NULL,
                        VerifiedBy = NULL,
                        VerifiedDate = NULL,
                        ApprovedBy = NULL,
                        ApprovalDate = NULL,
                        DisapprovedBy = NULL,
                        DisapprovedDate = NULL
                    WHERE TransactionID = " + transactionID + @";

                    DELETE FROM dbo.TblAccount_JournalEntries WHERE TransactionID = " + transactionID + @";
                ";
                html.SaveChangeData(sqlUpdate, false, null);
            }
            else
            {
                string domainID = html.DomainID ?? "";
                string sqlInsertHeader = @"
                    INSERT INTO dbo.TblAccount_Transaction 
                    (InvoiceNo, Type, Amount, TransactionDate, Description, ClassNameID, SystemID, WarehouseID, CreateByUserID, CreateDate, StatusID, IsAdjustment, CheckedByUserID, CheckedBy, VerifiedByUserID, VerifiedBy, ApprovedByUserID, ApprovedBy, DomainID)
                    VALUES 
                    ('" + safeJVNo + @"', 'Journal Entry', " + totalAmtStr + @", '" + dateStr + @"', '" + safeMemo + @"', " + classIdVal + @", " + sysID + @", " + whID + @", " + userID + @", GETDATE(), 60, " + isAdjustmentVal + @", " + checkedByIdVal + @", " + safeCheckedBy + @", " + verifiedByIdVal + @", " + safeVerifiedBy + @", " + approvedByIdVal + @", " + safeApprovedBy + @", '" + domainID + @"');
                    SELECT SCOPE_IDENTITY() AS NewID;

                    DECLARE @Prefix NVARCHAR(100) = '';
                    DECLARE @Module NVARCHAR(50) = 'JE';

                    IF (" + (classId.HasValue && classId.Value > 0 ? classId.Value.ToString() : "0") + @" > 0)
                    BEGIN
                        SELECT @Prefix = ISNULL(PrefixJE, ''),
                               @Module = 'JE - ' + REPLACE(ClassName, ' ', '')
                        FROM dbo.TblClass 
                        WHERE ClassID = " + (classId ?? 0) + @";
                    END

                    IF (@Prefix = '' OR @Prefix IS NULL)
                    BEGIN
                        SELECT TOP 1 @Prefix = Prefix, @Module = Module 
                        FROM dbo.TblPrefix 
                        WHERE Module IN ('JE', 'Journal Entry', 'JournalEntry', 'JV') 
                          AND SystemID = " + sysID + @" AND WarehouseID = " + whID + @"
                        ORDER BY PrefixID DESC;
                    END

                    IF (@Prefix = '' OR @Prefix IS NULL)
                    BEGIN
                        SET @Prefix = 'JE-{yy}{MM}-{000}';
                    END

                    IF EXISTS (SELECT 1 FROM dbo.TblPrefix WHERE Module = @Module AND SystemID = " + sysID + @" AND WarehouseID = " + whID + @")
                    BEGIN
                        UPDATE dbo.TblPrefix 
                        SET CurrentNumber = ISNULL(CurrentNumber, 0) + 1, CurrentPrefix = '" + safeJVNo + @"' 
                        WHERE Module = @Module AND SystemID = " + sysID + @" AND WarehouseID = " + whID + @";
                    END
                    ELSE
                    BEGIN
                        INSERT INTO dbo.TblPrefix (StartNumber, CurrentNumber, Prefix, CurrentPrefix, Module, SystemID, WarehouseID, DomainID)
                        VALUES (1, 1, @Prefix, '" + safeJVNo + @"', @Module, " + sysID + @", " + whID + @", '" + domainID + @"');
                    END
                ";
                DataSet ds = html.LoadData(sqlInsertHeader, false, null);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    transactionID = Convert.ToInt32(ds.Tables[0].Rows[0]["NewID"]);
                }
            }

            // Insert Details
            foreach (var line in model.Lines)
            {
                string lineMemo = (string.IsNullOrWhiteSpace(line.LineMemo) ? safeMemo : line.LineMemo).Replace("'", "''");
                string customerIdVal = line.NameID.HasValue ? line.NameID.Value.ToString() : "NULL";
                string debitStr = line.Debit.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string creditStr = line.Credit.ToString(System.Globalization.CultureInfo.InvariantCulture);

                string sqlInsertDetail = @"
                    INSERT INTO dbo.TblAccount_JournalEntries 
                    (TransactionID, InvoiceNo, AccountID, Debit, Credit, TransactionDate, Description, CustomerID, SystemID, WarehouseID, CreateByUserID, CreateDate)
                    VALUES 
                    (" + transactionID + @", '" + safeJVNo + @"', " + line.ChartOfAccountID + @", " + debitStr + @", " + creditStr + @", '" + dateStr + @"', '" + lineMemo + @"', " + customerIdVal + @", " + sysID + @", " + whID + @", " + userID + @", GETDATE());
                ";
                html.SaveChangeData(sqlInsertDetail, false, null);
            }

            return transactionID;
        }
    }
}
