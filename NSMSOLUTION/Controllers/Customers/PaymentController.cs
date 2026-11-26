using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NewInventorySystem.Controllers
{
    //[Authorize]
    public class PaymentController : Controller
    {
        // GET: Payment
        [Authorize(Roles = "Customer Payments")]
        public ActionResult Index(string InvoiceNo, string CustomerName, string Amount, string Status, int CustomerID, string Type)
        {
            HTML html = new HTML();
            string sql = "select t.PaymentID, c.Category, t.ToAccountNumber, t.ChartOfAccountID, b.Paid, b.Unpaid, t.ReferenceNo from dbo.TblPayment t ";
            sql += "left join dbo.TblPaymentCategory c on c.PaymentCategoryID = t.Category ";
            sql += "inner join dbo.TblCustomerBalance b on b.PaymentID = t.PaymentID ";
            sql += "where b.CustomerID=" + CustomerID + " and b.InvoiceNo = N'" + InvoiceNo + "' and b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID;
            string Category = "", ChartOfAccountID = "", Unpaid = "";
            string PayTo = "";
            ViewBag.Type = Type;
            ViewBag.Status = Status;
            ViewBag.PaymentID = "0";

            string sql_deposit = @"SELECT
    coa.ChartOfAccountID,
    cat.AccountCategory,
    ct.AccountTypes,
    coa.AccountNo,
    coa.AccountName,
    coa.ParentAccountID
FROM TblChartOfAccount coa
INNER JOIN dbo.TblAccountTypes ct 
    ON ct.AccountTypesID = coa.AccountTypesID
INNER JOIN dbo.TblAccountCategories cat 
    ON cat.AccountCategoryID = coa.AccountCategoryID
WHERE 
    coa.SystemID = " + html.SystemID + @" 
    AND coa.WarehouseID = " + html.WarehouseID + @"
    AND coa.AccountName NOT LIKE '%Discount%'
    AND (
        -- 1. Revenue accounts, exclude 
        (ct.AccountTypes = 'Revenue')
        
        -- 2. Deposit accounts (Cash / Bank / Petty Cash)
        OR (
            ct.AccountTypes = 'Asset'
            AND coa.AccountName NOT LIKE '%Cash out%'  -- exclude Cash out
            AND (
                    coa.AccountName LIKE '%Cash%' 
                 OR coa.AccountName LIKE '%Bank%' 
                 OR coa.AccountNo LIKE '111%'  
                 OR coa.AccountNo LIKE '112%'  
                 OR coa.AccountNo LIKE '1121%'   -- ABA bank account
                 OR coa.ParentAccountID IN (
                        SELECT ChartOfAccountID 
                        FROM TblChartOfAccount 
                        WHERE AccountName IN ('Cash on Hand', 'Bank Account', 'Petty Cash')
                   )
                )           
        )
    )
ORDER BY 
    CASE WHEN ct.AccountTypes = 'Asset' THEN 0 ELSE 1 END,
    coa.AccountNo;

";

            DataSet ds = new DataSet();
            ds = html.LoadData(sql);
            string existingRefNo = "";
            if (Status != "10")
            {
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        Unpaid = ds.Tables[0].Rows[0]["Paid"].ToString();
                        Category = ds.Tables[0].Rows[0]["Category"].ToString();
                        ChartOfAccountID = ds.Tables[0].Rows[0]["ChartOfAccountID"].ToString();
                        PayTo = ds.Tables[0].Rows[0]["ToAccountNumber"].ToString();
                        existingRefNo = ds.Tables[0].Rows[0]["ReferenceNo"].ToString();
                        ViewBag.PaymentID = ds.Tables[0].Rows[0]["PaymentID"].ToString();
                    }
                }
            }
            else
            {
                Unpaid = ds.Tables[0].Rows.Count > 0 ? ds.Tables[0].Rows[0]["Unpaid"].ToString() : Amount;
                //sql = "select FormName, ChartOfAccountID from dbo.TblRemember where FormName='PaymentForm' and SystemID = @SystemID and WarehouseID = @WarehouseID";
                //Dictionary<string, string> Para = new Dictionary<string, string>();
                //Para.Add("SystemID", html.SystemID);
                //Para.Add("WarehouseID", html.WarehouseID);
                //ds = html.LoadData(sql, false, Para);
                //if (ds.Tables.Count > 0)
                //{
                //	if (ds.Tables[0].Rows.Count > 0)
                //	{
                //		ChartOfAccountID = ds.Tables[0].Rows[0]["ChartOfAccountID"].ToString();
                //	}
                //}
                //ChartOfAccountID = "92";

            }

            ViewBag.Unpaid = Unpaid;


            html.SaveChangeFunction = "SaveChangePayment()";
            html.FormID = "PaymentInvoice";
            html.FieldIDFocus = "ReferenceNo";
            html.TableName = "TblDepartment";
            html.PrimaryField = "DepartmentID";
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "InvoiceNo:",
                ID = "InvoiceNo",
                DBField = "InvoiceNo",
                Required = true,
                IsReadOnly = true,
                Value = InvoiceNo,
                Hide = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Customer:",
                ID = "Payer",
                DBField = "Payer",
                Required = true,
                IsReadOnly = true,
                Value = CustomerName
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                SingleDatePicker = true,
                Label = "Payment Date:",
                ID = "PaymentDate",
                DBField = "PaymentDate",
                Required = true
            });
            // Filter receipt prefix: CISP invoices use "I26-" prefix, others use "N26-"
            string receiptPrefixFilter = InvoiceNo.StartsWith("CISP", StringComparison.OrdinalIgnoreCase) ? "I%" : "N%";

            string sql_ref = @"
                SELECT p.OfficialReceiptNo as ReferenceNo, p.ChartOfAccountID, p.PaymentMenthod, p.ChequeNumber,
                       (p.PaymentAmount - ISNULL((SELECT SUM(PaymentAmount) FROM dbo.TblPayment WHERE ReferenceNo = p.OfficialReceiptNo AND SystemID = p.SystemID AND WarehouseID = p.WarehouseID), 0)) as PaymentAmount,
                       (p.OfficialReceiptNo + ' (Avail: $' + convert(varchar, cast((p.PaymentAmount - ISNULL((SELECT SUM(PaymentAmount) FROM dbo.TblPayment WHERE ReferenceNo = p.OfficialReceiptNo AND SystemID = p.SystemID AND WarehouseID = p.WarehouseID), 0)) as decimal(18,2))) + ')') as ReceiptDisplay
                FROM dbo.TblPayment p
                WHERE p.CustomerID = " + CustomerID + @" 
                  AND (p.InvoiceNo = '' OR p.InvoiceNo IS NULL)     
                  AND p.Type = 'Income' 
                  AND p.SystemID = " + html.SystemID + @" 
                  AND p.WarehouseID = " + html.WarehouseID + @"
                  AND ISNULL(p.OfficialReceiptNo, '') <> ''
                  AND p.OfficialReceiptNo LIKE '" + receiptPrefixFilter + @"'
                  AND (p.PaymentAmount - ISNULL((SELECT SUM(PaymentAmount) FROM dbo.TblPayment WHERE ReferenceNo = p.OfficialReceiptNo AND SystemID = p.SystemID AND WarehouseID = p.WarehouseID), 0)) > 0
                ORDER BY p.PaymentDate DESC";

            DataSet dsRef = html.LoadData(sql_ref);
            string defaultReceipt = "";
            decimal receiptAmount = 0;
            string defaultPaymentMethodId = "";
            bool hasUnappliedReceipt = false;

            if (dsRef != null && dsRef.Tables.Count > 0 && dsRef.Tables[0].Rows.Count > 0)
            {
                hasUnappliedReceipt = true;
                defaultReceipt = dsRef.Tables[0].Rows[0]["ReferenceNo"].ToString();
                string receiptChartOfAccountID = dsRef.Tables[0].Rows[0]["ChartOfAccountID"].ToString();
                defaultPaymentMethodId = dsRef.Tables[0].Rows[0]["PaymentMenthod"].ToString();
                decimal.TryParse(dsRef.Tables[0].Rows[0]["PaymentAmount"].ToString(), out receiptAmount);

                if (Status == "10" && !string.IsNullOrEmpty(receiptChartOfAccountID))
                {
                    ChartOfAccountID = receiptChartOfAccountID;
                }
            }

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Payment Method:",
                ID = "PaymentMenthod",
                DBField = "PaymentMenthod",
                Required = true,
                SQL_CBO = "select PaymentMethodID, PaymentMethod from dbo.TblPaymentMethod " +
                "where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by case when PaymentMethod = 'Bank Transfer' then 0 else 1 end, PaymentMethod",
                DisplayText = "PaymentMethod",
                Value = "PaymentMethodID",
                CboValue = defaultPaymentMethodId
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Cheque No.:",
                ID = "ChequeNumber",
                DBField = "ChequeNumber",
                IsReadOnly = true
            });

            List<string> entriesList = new List<string>();
            if (dsRef != null && dsRef.Tables.Count > 0 && dsRef.Tables[0].Rows.Count > 0)
            {
                hasUnappliedReceipt = true;
                foreach (System.Data.DataRow row in dsRef.Tables[0].Rows)
                {
                    string refNo = row["ReferenceNo"]?.ToString();
                    string acc = row["ChartOfAccountID"]?.ToString();
                    string payMethodId = row["PaymentMenthod"]?.ToString();
                    string chequeNo = row["ChequeNumber"]?.ToString();
                    string display = row["ReceiptDisplay"]?.ToString().Replace("\"", "\\\"");
                    entriesList.Add($"{{ \"refNo\": \"{refNo}\", \"account\": \"{acc}\", \"methodId\": \"{payMethodId}\", \"cheque\": \"{chequeNo}\", \"display\": \"{display}\" }}");
                }
            }
            ViewBag.refDataJson = "[" + string.Join(",", entriesList) + "]";
            ViewBag.StatusID = Status;
            ViewBag.InvoiceNo = InvoiceNo;



            if (Status != "10")
            {
                html.HTMLControls.Add(new HtmlTextBox()
                {
                    Label = "Reference No.:",
                    ID = "ReferenceNo",
                    DBField = "ReferenceNo",
                    Value = existingRefNo,
                    IsReadOnly = true
                });
            }
            else if (hasUnappliedReceipt)
            {
                decimal unpaidAmt;
                decimal.TryParse(Unpaid, out unpaidAmt);

                if (receiptAmount > 0 && receiptAmount < unpaidAmt)
                {
                    Unpaid = receiptAmount.ToString("0.00");
                }

                html.HTMLControls.Add(new HtmlComboBox()
                {
                    Label = "Reference No.:",
                    ID = "ReferenceNo",
                    DBField = "ReferenceNo",
                    SQL_CBO = sql_ref,
                    DisplayText = "ReceiptDisplay",
                    Value = "ReferenceNo",
                    CboValue = defaultReceipt
                });
            }
            else
            {
                html.HTMLControls.Add(new HtmlTextBox()
                {
                    Label = "Reference No.:",
                    ID = "ReferenceNo",
                    DBField = "ReferenceNo",
                    //ExLabel= "e.g. Transaction ID, Check No."
                });
            }

            string IncomeAccount = @"select distinct coa.ChartOfAccountID, ac.AccountCategory, act.AccountTypes, coa.AccountNo, coa.AccountName
from dbo.TblAccountTypes act
inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID
inner join dbo.TblChartOfAccount coa on coa.AccountType = act.AccountTypesID
where ac.AccountCategoryID = 3 and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " order by ac.AccountCategory, act.AccountTypes";

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Deposit To:",
                ID = "ChartOfAccountID",
                SQL_CBO = sql_deposit,
                CboValue = ChartOfAccountID,
                DBField = "ChartOfAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "150" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
                //Label = "Account:",
                //ID = "ChartOfAccountID",
                //DBField = "ChartOfAccountID",
                //Required = true,
                //SQL_CBO = "select ca.ChartOfAccountID, (ca.AccountNo + ' - ' + ca.AccountName + ' (' + ca.AccountType + ')') AccountName from dbo.TblChartOfAccount ca " +
                //"where ca.SystemID = " + html.SystemID + " and ca.WarehouseID = " + html.WarehouseID + " order by (ca.AccountNo + ' - ' + ca.AccountName + ' (' + ca.AccountType + ')')",
                //DisplayText = "AccountName",
                //Value = "ChartOfAccountID",
                //CboValue = ChartOfAccountID,
                //Infinity = false
            });


            sql = "select AccountID, (AccountTitle + case when AccountNumber <> '' then ' - ' + AccountNumber else '' end";
            sql += "+ case when AccountName <> '' then ' - ' + AccountName else '' end) AccountName from dbo.TblAccount ";
            sql += "where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by case when AccountTitle = 'Cash' then 0 else 1 end, AccountName";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Pay To:",
                ID = "ToAccountNumber",
                DBField = "ToAccountNumber",
                //Required = true,
                SQL_CBO = sql,
                Value = "AccountID",
                DisplayText = "AccountName",
                CboValue = PayTo,
                Hide = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Amount Received:",
                ID = "PaymentAmount",
                DBField = "PaymentAmount",
                Required = true,
                Value = Unpaid
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Hide = true,
                Label = "Category:",
                ID = "Category",
                DBField = "Category",
                Value = Category,
                //Required = (Status != "10" ? false : true),
                isTypeahead = true,
                SQL = "select Category from dbo.TblPaymentCategory " +
                "where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by Category"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "PaymentDescription",
                DBField = "PaymentDescription",
                //Required = true,
                //Value = (Status == "10" ? "Payment for Invoice No.: " + InvoiceNo : "Rollback Payment for Invoice No.: " + InvoiceNo)
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Received From Name:",
                ID = "ReceivedFromName",
                DBField = "ReceivedFromName",
                //Required = true,
                Value = CustomerName,
                Hide = true,
            });

            //html.HTMLControls.Add(new HtmlFile()
            //{
            //    Label = "Attachment File:",
            //    ID = "AttachmentFile",
            //    DBField = "AttachmentFile",
            //    DestinationFile = "Content/PaymentAttachmentFile",
            //    Multiple = true
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "StatusID:",
                ID = "StatusID",
                DBField = "StatusID",
                Required = true,
                Value = Status,
                Hide = true
            });
            sql = "select distinct ci.BillingInvoiceID, c.CustomerCode, c.CustomerName from dbo.TblCustomerInvoice ci inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID where ci.InvoiceNo='" + InvoiceNo + "'";
            ds = html.LoadData(sql);
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "BillingInvoiceID:",
                ID = "BillingInvoiceID",
                DBField = "BillingInvoiceID",
                Required = true,
                Value = ds.Tables[0].Rows[0]["BillingInvoiceID"].ToString(),
                Hide = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "CustomerCode:",
                ID = "CustomerCode",
                DBField = "CustomerCode",
                Required = true,
                Value = ds.Tables[0].Rows[0]["CustomerCode"].ToString(),
                Hide = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "CustomerName:",
                ID = "CustomerName",
                DBField = "CustomerName",
                Required = true,
                Value = ds.Tables[0].Rows[0]["CustomerName"].ToString(),
                Hide = true
            });
            sql = "select ac.CustomerID from dbo.TblActivateCustomer ac ";
            sql += "inner join dbo.TblCustomerBillingInvoice i on i.BillingInvoiceID = ac.BillingInvoiceID ";
            sql += "where ac.CustomerID = " + CustomerID;
            ds = html.LoadData(sql);
            ViewBag.CustomerID = CustomerID;
            ViewBag.CustIsActivate = false;
            if (ds.Tables[0].Rows.Count > 0)
                ViewBag.CustIsActivate = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        [Authorize(Roles = "Customer Payments")]
        public async Task<ActionResult> SaveChangePayment(object[] PaymentData)
        {
            SqlResult sqlresult = new SqlResult();
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML();
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(PaymentData[0].ToString());
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            StringBuilder sb = new StringBuilder();
            string InvoiceNo = DataInfo["InvoiceNo"];
            string memo = DataInfo["PaymentDescription"];
            string StatusID = DataInfo["StatusID"];
            string CustomerID = DataInfo["CustomerID"];
            string Payment = $"$ {DataInfo["PaymentAmount"]?.ToString()}";
            string CustomerName = HttpUtility.UrlDecode(DataInfo["CustomerName"]);
            string CID = HttpUtility.UrlDecode(DataInfo["CustomerCode"]);
            CustomerName = $"{html.ToUrlSlug($"{CustomerName + " [" + CID + "]"}")}";
            string DestinationFolder = "/Uploads/CustomerAttFiles/" + CustomerName + "/Payment AttFile/";
            sqlresult = html.UploadFile(DestinationFolder);
            string AttachmentFile;
            if (sqlresult.Result == ResultType.Successful)
                AttachmentFile = sqlresult.Value;
            else
            {
                return Json(new
                {
                    Message = sqlresult.Message,
                    Result = sqlresult.Result,
                    Data = sqlresult.Value,
                    OfficialReceiptData = ""
                }, JsonRequestBehavior.AllowGet);
            }
            // Generate Receipt Number (RV) using prefix system
            string ReceiptNo = "";
            string referenceNoStr = "";
            if (DataInfo.ContainsKey("ReferenceNo") && DataInfo["ReferenceNo"] != null)
            {
                referenceNoStr = DataInfo["ReferenceNo"].ToString().Trim();
            }

            if (!string.IsNullOrEmpty(referenceNoStr))
            {
                // Reuse the selected receipt instead of generating a new one
                ReceiptNo = referenceNoStr;
            }
            else
            {
                // Determine the receipt module based on the invoice type:
                // If InvoiceNo starts with "CISP" → use ISP module (prefix I26-XXXXX)
                // Otherwise → use NSM module (prefix N26-XXXXX)
                string rvModule = InvoiceNo.StartsWith("CISP", StringComparison.OrdinalIgnoreCase) ? "ISP" : "NSM";

                string rvSql = "declare @Prefix nvarchar(50), @Module nvarchar(50) = '" + rvModule + "'; " +
                    "select @Prefix = Prefix from dbo.TblPrefix where Module = '" + rvModule + "' and SystemID = " + html.SystemID +
                    " and WarehouseID = " + html.WarehouseID + "; " +
                    "exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID +
                    ",'" + html.DomainID + "', 1, '', 0, ''";
                DataSet dsRV = html.LoadData(rvSql);
                if (dsRV.Tables.Count > 0 && dsRV.Tables[0].Rows.Count > 0)
                {
                    ReceiptNo = dsRV.Tables[0].Rows[0][0].ToString();
                }
            }

            sb.AppendLine("<Data>");
            sb.AppendLine("<PaymentInfo>");
            foreach (dynamic item in DataInfo)
            {
                string key = item.Key;
                string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                if (key == "PaymentDate")
                    value = html.StringToDate(value);
                sb.AppendLine("<" + key + ">" + html.EscapeXml(value) + "</" + key + ">");
            }
            sb.AppendLine("<PaidByUserID>" + html.UserID + "</PaidByUserID>");
            sb.AppendLine("<PaidBy>" + html.UserFullName + "</PaidBy>");
            sb.AppendLine("<SystemID>" + html.SystemID + "</SystemID>");
            sb.AppendLine("<WarehouseID>" + html.WarehouseID + "</WarehouseID>");
            sb.AppendLine("<DomainID>" + html.DomainID + "</DomainID>");
            sb.AppendLine("<DestinationFolder>" + DestinationFolder + "</DestinationFolder>");
            sb.AppendLine("<AttachmentFile>" + AttachmentFile + "</AttachmentFile>");
            sb.AppendLine("<ReceiptNo>" + html.EscapeXml(ReceiptNo) + "</ReceiptNo>");
            sb.AppendLine("</PaymentInfo>");
            sb.AppendLine("</Data>");
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "Data", "N'"+sb.ToString()+"'" }
            };
            sqlresult = html.SaveChangeData("SP_INSERT_PAYMENT", true, Para, true, true);
            string OfficialReceiptData = html.JsonConvertDataTable(sqlresult.Data.Tables[0]);
            string sqls = @"SELECT BotToken FROM TblTelegramBots WHERE SystemID = @SystemID";
            Dictionary<string, string> botParams = new Dictionary<string, string> { { "SystemID", html.SystemID } };
            DataSet newbotData = html.LoadData(sqls, false, botParams);
            string newbots = newbotData.Tables[0].Rows[0]["BotToken"]?.ToString();
            string sqlGuest = @"SELECT 
                                CASE 
                                    WHEN COALESCE(NULLIF(CustomerName, ''),CustomerNameLatin) NOT LIKE '%[a-zA-Z]%' AND CustomerGender = 'Male' 
                                        THEN N'លោក ' + COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin)

                                    WHEN COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin) NOT LIKE '%[a-zA-Z]%' AND CustomerGender = 'Female' 
                                        THEN N'អ្នកស្រី ' + COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin)

                                    WHEN CustomerGender = 'Male' 
                                        THEN 'Mr. ' + COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin)

                                    WHEN CustomerGender = 'Female' 
                                        THEN 'Ms. ' + COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin)

                                    ELSE COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin)
                                END AS CustomerName
                            FROM TblCustomer  WHERE CustomerID  = @CustomerID";
            Dictionary<string, string> guestName = new Dictionary<string, string> { { "CustomerID", CustomerID } };
            DataSet customerName = html.LoadData(sqlGuest, false, guestName);
            string guest = customerName.Tables[0].Rows[0]["CustomerName"]?.ToString();
            // ===== END PAYMENT NOTIFICATION =====

            if (sqlresult.Result == ResultType.Successful)
            {

                if (StatusID != "11")
                {
                    string ChartOfAccountName = "";
                    string sql_acc = "select AccountName from dbo.TblChartOfAccount where ChartOfAccountID = " + DataInfo["ChartOfAccountID"] + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;

                    DataSet ds_acc = html.LoadData(sql_acc);
                    if (ds_acc.Tables[0].Rows.Count > 0)
                    {
                        ChartOfAccountName = ds_acc.Tables[0].Rows[0]["AccountName"].ToString();
                        //var match = Regex.Match(ChartOfAccountName, @"\(([^)]+)\)");
                        //if (match.Success)
                        //    ChartOfAccountName = match.Groups[1].Value.Trim();
                    }

                    string PaymentAttachFolder = "~/Uploads/CustomerAttFiles/" + CustomerName + "/Payment AttFile/";

                    StringBuilder payMsg = new StringBuilder();
                    payMsg.AppendLine("");
                    payMsg.AppendLine("<b>We recieved payment from</b>");
                    payMsg.AppendLine("<b>Customer:</b> " + guest);
                    payMsg.AppendLine("<b>Invoice No:</b> " + InvoiceNo);
                    payMsg.AppendLine("<b>Deposit To:</b> " + ChartOfAccountName);
                    payMsg.AppendLine("<b>Amount :</b> " + Payment);
                    payMsg.AppendLine("<b>Received By:</b> " + html.UserFullName);
                    payMsg.AppendLine("<b>Memo :</b> " + memo);

                    string TelegramMessage = payMsg.ToString();

                    if (!string.IsNullOrEmpty(AttachmentFile))
                    {
                        string[] files = AttachmentFile.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

                        //Send message once
                        await notificationTelegram.NotificationTelegramBot(payMsg.ToString(), -1003702719002);

                        //Send each as full inline photo
                        foreach (string file in files)
                        {
                            string fullFilePath = Server.MapPath(PaymentAttachFolder + file.Trim());
                            if (System.IO.File.Exists(fullFilePath))
                                await notificationTelegram.SendPhotoToGroup(fullFilePath, "", -1003702719002);
                        }
                    }
                    else
                    {
                        await notificationTelegram.NotificationTelegramBot(payMsg.ToString(), -1003702719002);
                    }
                    string sql = "select b.UM, b.CustomerID, b.BillingInvoiceID, b.InvoiceType, b.PPPoEUserName, b.ConnectivityStatus, b.StartDate, b.EndDate, b.Duration, isnull(b.IsAlreadyApplyProfile, 0) IsAlreadyApplyProfile, b.ActivateCustomerID, b.IsChangePlan, case when b.invTmp = 1 then b.PackageName else REPLACE(b.ServiceName, s.ServiceName+' - ', '') end PackageName  ";
                    sql += "from dbo.TblCustomerBillingInvoice b ";
                    sql += "inner join dbo.TblServices s on s.ServiceID = b.ServiceID ";
                    sql += "where b.CustomerID =" + CustomerID + " and b.InvoiceNo = N'" + InvoiceNo + "' and b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID;
                    DataSet ds = html.LoadData(sql);
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                //CustomerID = dr["CustomerID"].ToString();
                                string InvoiceType = dr["InvoiceType"].ToString();
                                string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                                string StartDate = dr["StartDate"].ToString();
                                string EndDate = dr["EndDate"].ToString();
                                string Duration = dr["Duration"].ToString();
                                string ActivateCustomerID = dr["ActivateCustomerID"].ToString();
                                string IsAlreadyApplyProfile = dr["IsAlreadyApplyProfile"].ToString();
                                string IsChangePlan = dr["IsChangePlan"].ToString();
                                string PackageName = dr["PackageName"].ToString();
                                string UM = dr["UM"].ToString();
                                if (InvoiceType == "Renew Invoice")
                                {
                                    sql = "select RouterID, PPPoEUserName, PPPoEPassword, PPPoEProfileName, ActualProfile, ConnectivityStatus, ConnectionType, VLanID, VLanName, format(RenewDate, 'yyyy-MM-dd') RenewDate, [SID], ActivateDate from dbo.TblActivateCustomer where ";
                                    sql += "ActivateCustomerID =" + ActivateCustomerID;
                                    ds = html.LoadData(sql);
                                    if (ds.Tables.Count > 0)
                                    {
                                        if (ds.Tables[0].Rows.Count > 0)
                                        {
                                            string ServiceID = ds.Tables[0].Rows[0]["SID"].ToString();
                                            string ActivateDate = ds.Tables[0].Rows[0]["ActivateDate"].ToString();
                                            string RouterID = ds.Tables[0].Rows[0]["RouterID"].ToString();
                                            string VLanID = ds.Tables[0].Rows[0]["VLanID"].ToString();
                                            string VLanName = ds.Tables[0].Rows[0]["VLanName"].ToString();
                                            string ConnectionType = ds.Tables[0].Rows[0]["ConnectionType"].ToString();
                                            string ConnectivityStatus = ds.Tables[0].Rows[0]["ConnectivityStatus"].ToString();
                                            string PPPoEProfileName = ds.Tables[0].Rows[0]["ActualProfile"].ToString();
                                            string RenewDate = ds.Tables[0].Rows[0]["RenewDate"].ToString();
                                            //string Comment = "Customer Renew Services With Profile Name: " + PPPoEProfileName;
                                            string Comment = "Renew " + UM + "; " + DateTime.Parse(StartDate).ToString("dd-MMM-yyyy") + " - " + DateTime.Parse(EndDate).ToString("dd-MMM-yyyy") + "; Renew Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserFullName;
                                            string PPPoEUserName = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
                                            string PPPoEPassword = ds.Tables[0].Rows[0]["PPPoEPassword"].ToString();
                                            //if (ConnectivityStatus != "42")
                                            //{
                                            Mikrotik MK = new Mikrotik();
                                            MK.SetConnection(int.Parse(RouterID));
                                            if (ConnectionType == "PPPoE")
                                            {
                                                //if (ConnectivityStatus == "50")
                                                //{
                                                //	MK.ClearProfileName(PPPoEUserName);
                                                //	Comment = "Customer Services was reverse to Profile Name: " + PPPoEProfileName + " Date:" + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserFullName;
                                                //}
                                                //else 
                                                if (ConnectivityStatus == "41")
                                                {
                                                    Comment = "Re-activated on " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserFullName;
                                                    MK.ReActivateUser(PPPoEUserName, Comment);
                                                }
                                                if (IsChangePlan == "False" || IsChangePlan == "")
                                                {
                                                    //disable temporay
                                                    //sqlresult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                                                    sqlresult.Result = ResultType.Successful;
                                                }
                                                else
                                                {
                                                    DateTime SDate = DateTime.Parse(StartDate);
                                                    if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                                    {
                                                        Comment = "Customer Change Plan From: <b>" + PPPoEProfileName + "</b> TO <b>" + PackageName + "</b>; Apply Changed by: <b>" + html.UserFullName + "</b>; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                                                        PPPoEProfileName = PackageName;
                                                        MK.ClearProfileName(PPPoEUserName);
                                                        sqlresult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                                                    }
                                                }
                                            }
                                            else if (ConnectionType == "Static")
                                            {
                                                //Comment = "Customer Renew Services With Vlan Name: " + VLanName;
                                                sqlresult = MK.EnableVlanID(VLanID, Comment);
                                            }
                                            else
                                            {
                                                sqlresult.Result = ResultType.Successful;
                                            }
                                            //}
                                            if (sqlresult.Result == ResultType.Successful)
                                            {
                                                //if (ConnectivityStatus != "42")
                                                //{
                                                sql = "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 40, ConnectionType = '" + ConnectionType + "', RenewBy = '" + html.UserFullName + "', RenewDate = '" + html.GetCurrentDate() + "', [ExpireDate] = '" + EndDate + "',";
                                                sql += "RouterID=" + RouterID + ", [SID] = '" + ServiceID + "', PPPoEPassword='" + PPPoEPassword + "', ActivateDate ='" + ActivateDate + "',ConnectivityStatusName = 'Active', IsAlreadyApplyProfile = 1 where InvoiceNo = '" + InvoiceNo + "'";
                                                sql += ";update dbo.TblCustomerInvoice set ConnectivityStatus = 40, ConnectivityStatusName = 'Active', RenewBy = '" + html.UserFullName + "', RenewDate = '" + html.GetCurrentDate() + "' ";
                                                sql += "where InvoiceNo = '" + InvoiceNo + "' and SystemID=" + html.SystemID + " and WarehouseID=" + html.WarehouseID + ";";
                                                string ConnectionID;
                                                if (IsChangePlan == "False" || IsChangePlan == "")
                                                {
                                                    sql += "update dbo.TblActivateCustomer set InvoiceNo = '" + InvoiceNo + "', BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                                    sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='" + html.UserFullName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                                    sql += ";";
                                                    sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, ConnectionType, ";
                                                    sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                                    sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                                    sql += "select ac.ActivateCustomerID, ac.ConnectionType, ac.CustomerID, ";
                                                    sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, '" + html.UserFullName + "' ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                                                    sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                                    sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                                                    //sql += "inner join dbo.TblCustomerBillingInvoice bi on bi.BillingInvoiceID = ac.BillingInvoiceID ";
                                                    sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                                    sqlresult = html.SaveChangeData(sql, false, false);
                                                    //send notification to telegram when customer services is renew
                                                    sql = "select IsEnable from TblTelegramNotification where IsEnable = 1 and StatusID = 44 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
                                                    ds = html.LoadData(sql);
                                                    if (ds.Tables[0].Rows.Count > 0)
                                                    {
                                                        sql = @"select top 1 c.CustomerID, ta.RouterID, ta.PPPoEUserName, ta.PPPoEPassword, ta.BillingInvoiceID, ta.ActivateCustomerID, c.CustomerCode CID,case when isnull(c.CustomerName, '') <> '' then c.CustomerName else c.CustomerNameLatin end CName, ta.ConnectionID, ta.SID, bf.CompanyContactMobileNumber,
                                                    (c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName, s.ServiceName, pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + b.UM +
                                                    case when b.FreeDuration <> 0 then ' Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName, ta.VLanName, ta.VLanIPAddress, ta.IPAddress CustomerIPAddress, ta.GetwayIP, ta.TicketNumber,
                                                    format(b.StartDate, 'dd-MM-yyyy') StartDate, format(b.EndDate, 'dd-MM-yyyy') EndDate, b.Amount, b.InvoiceNo, format(b.PaymentDueDate, 'dd-MM-yyyy') PaymentDueDate, ta.ConnectionType, ta.VLanID 
                                                    from dbo.TblActivateCustomer ta
                                                    inner join dbo.TblCustomer c on c.CustomerID = ta.CustomerID
                                                    inner join dbo.TblServices s on s.ServiceID = ta.ServiceID
                                                    inner join dbo.TblCustomerBillingInvoice b on b.ActivateCustomerID = ta.ActivateCustomerID 
                                                    inner join dbo.TblPackageDetail pd on pd.PackageDetailID = b.PackageDetailID
                                                    left join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = b.CustomerBillInformationID
                                                    where ta.ActivateCustomerID=@ActivateCustomerID and ta.ConnectivityStatus = 40 and b.StatusID not in(12,13) and format(b.RenewDate, 'yyyyMMdd') = format(getdate(), 'yyyyMMdd') order by b.BillingInvoiceID desc";
                                                        Dictionary<string, string> para = new Dictionary<string, string>
                                                        {
                                                            { "DateNow", easternTimeNow.ToString("yyyyMMdd") },
                                                            { "ActivateCustomerID", ActivateCustomerID }
                                                        };
                                                        ds = html.LoadData(sql, false, para);

                                                        if (ds.Tables[0].Rows.Count > 0)
                                                        {
                                                            foreach (DataRow drow in ds.Tables[0].Rows)
                                                            {
                                                                CustomerID = drow["CustomerID"].ToString();
                                                                ConnectionID = drow["ConnectionID"].ToString();
                                                                string SID = drow["SID"].ToString();
                                                                CID = drow["CID"].ToString();
                                                                string CName = drow["CName"].ToString();
                                                                string CompanyContactMobileNumber = drow["CompanyContactMobileNumber"].ToString();
                                                                string TicketNumber = drow["TicketNumber"].ToString();
                                                                CustomerName = drow["CustomerName"].ToString();
                                                                PackageName = drow["PackageName"].ToString();
                                                                StartDate = drow["StartDate"].ToString();
                                                                EndDate = drow["EndDate"].ToString();
                                                                InvoiceNo = drow["InvoiceNo"].ToString();
                                                                RouterID = drow["RouterID"].ToString();
                                                                string PaymentDueDate = drow["PaymentDueDate"].ToString();

                                                                string msg = "«សេចក្តីជូនដំណឹងអំពី អ្នកបានបន្តការប្រើប្រាស់កញ្ចប់សេវាកម្មអ៊ិនធឺណិត»\n\n";
                                                                msg += "ឈ្មោះអតិថិជន: " + CName + "\n";
                                                                msg += "លេខសំគាល់អតិថិជន: " + CID + "\n";
                                                                msg += "ឈ្មោះកញ្ចប់សេវាកម្មៈ <b>" + PackageName + "</b>\n\n";
                                                                msg += "ថ្ងៃចាប់ផ្តើមៈ <b>" + StartDate + "</b>\n";
                                                                msg += "ថ្ងៃផុតសុពលភាពៈ <b>" + EndDate + "</b>\n";
                                                                msg += "\nសម្រាប់ពត៌មានបន្ថែមសូមទាក់ទង\nផ្នែកបម្រើអតិថិជន: +85586588777, +85578588777\n";
                                                                msg += "ផ្នែកបច្ចេកទេសៈ +85561876777\n";
                                                                msg += "គេហទំព័រ: www.nsmisp.com.kh\n";
                                                                msg += "\nសូមអរគុណ!";
                                                                CompanyContactMobileNumber = CompanyContactMobileNumber.Trim().Replace(" ", string.Empty);
                                                                var rg_ex = new Regex(Regex.Escape("0"));
                                                                CompanyContactMobileNumber = rg_ex.Replace(CompanyContactMobileNumber, "+855", 1);
                                                                //await notificationTelegram.NotificationToCustomer(CompanyContactMobileNumber, CustomerName, CID, int.Parse(CustomerID), msg);
                                                                sql = @"select ct.TelegramNotificationID, ct.CustomerPhone, ct.FirstName, ct.LastName, ct.CustomerID
from dbo.TblCustomerTelegram ct
where isnull(ct.TelegramNotificationID,'') <> ''
and 2 = (select [Data] from dbo.Split(ct.TelegramNotificationID, ',') where [Data] = 2)
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
                                                                            string TelegramNotificationID = "2";
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
                                                    //end send notification to telegram when customer services is renew
                                                }
                                                else
                                                {
                                                    DateTime SDate = DateTime.Parse(StartDate);
                                                    if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                                    {
                                                        sql += "update dbo.TblActivateCustomer set IsApplyChangePlan = 1, InvoiceNo = '" + InvoiceNo + "', BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                                        sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='" + html.UserFullName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                                        sql += ";";
                                                        sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, BillingInvoiceID, ConnectionType, ";
                                                        sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                                        sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                                        sql += "select ac.ActivateCustomerID, ac.BillingInvoiceID, ac.ConnectionType, ac.CustomerID, ";
                                                        sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, '" + html.UserFullName + "' ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                                                        sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                                        sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                                                        sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                                        sqlresult = html.SaveChangeData(sql, false, false);
                                                        sql = "select * from dbo.TblActivateCustomer where ActivateCustomerID = " + ActivateCustomerID;
                                                        ds = html.LoadData(sql);
                                                        DataRow dr_data = ds.Tables[0].Rows[0];
                                                        string Ticket_Number = dr_data["TicketNumber"].ToString();
                                                        string Comment_s = @"Dear Team, <br><br>" + Comment + ".";

                                                        Comment_s += @"<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO GENERATE TICKET.";

                                                        sql = @"insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID,
                                                    Status, PriorityID, Priority, OpenTicketID, FromUserLoginID, FromUserName,
                                                    ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
                                                    select SupportProjectID, StatusID, [Status], PriorityID, [Priority], 
                                                    OpenTicketID, UserLoginID FromUserLoginID, CreateBy FromUserName, " + html.UserID + @" ReplyFromUserLoginID, '" + html.UserFullName + @"' ReplyFromUserName, 
                                                    N'" + Comment_s + "' Comment, '" + html.GetCurrentDate() + @"' [Date], '" + html.UserFullName + @"' CreateBy, '" + html.GetCurrentDate() + @"'CreateDate, " + html.SystemID + " SystemID, " + html.WarehouseID + @" WarehouseID, 
                                                    '' DescriptionChangeStatus
                                                    from dbo.TblSupportOpenTicket
                                                    where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;";
                                                        Dictionary<string, string> para = new Dictionary<string, string>
                                                        {
                                                            { "TicketNumber", Ticket_Number },
                                                            { "SystemID", html.SystemID },
                                                            { "WarehouseID", html.WarehouseID }
                                                        };
                                                        html.SaveChangeData(sql, false, para);
                                                        await notificationTelegram.NotificationTelegramBot((Comment_s + " " + Ticket_Number).Replace("<br>", "\n"));
                                                        await notificationTelegram.NotificationTelegramBot((Comment_s + " " + Ticket_Number).Replace("<br>", "\n"), -635916546);//635916546 = test send finance group                                                    
                                                    }
                                                }
                                                //}
                                                sql = "select top 1 cb.GrandTotal Amount, s.ServiceName, c.CustomerCode FirstName, c.CustomerName LastName, bi.CompanyEmail Email, bi.CompanyContactMobileNumber Phone, bi.InstallProvinceCity [Location], 'Activation by: " + html.UserFullName + "; Activate Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + "' Comment ";
                                                sql += ", b.PDFFileName, b.DestinationFolder, case when b.invTmp = 1 then b.ServiceName + ' - ' + b.PackageName else b.ServiceName end PackageName, ";
                                                sql += "format(b.StartDate, 'dd-MMM-yyyy') StartDate, format(b.EndDate, 'dd-MMM-yyyy') EndDate, b.ConnectionID, b.[SID] ";
                                                sql += "from dbo.TblCustomer c ";
                                                sql += "inner join dbo.TblCustomerBillInformation bi on bi.CustomerID = c.CustomerID ";
                                                sql += "inner join dbo.TblCustomerBillingInvoice b on b.CustomerID = bi.CustomerID and b.CustomerBillInformationID = bi.CustomerBillInformationID ";
                                                sql += "inner join dbo.TblServices s on s.ServiceID = b.ServiceID ";
                                                sql += "inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID and cb.InvoiceNo = b.InvoiceNo ";
                                                sql += "where b.InvoiceNo = '" + InvoiceNo + "' and c.CustomerID = " + CustomerID + " and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
                                                ds = html.LoadData(sql);
                                                ServiceID = ds.Tables[0].Rows[0]["SID"].ToString();
                                                ConnectionID = ds.Tables[0].Rows[0]["ConnectionID"].ToString();
                                                string ServiceName = ds.Tables[0].Rows[0]["ServiceName"].ToString();
                                                string FirstName = ds.Tables[0].Rows[0]["FirstName"].ToString();
                                                string LastName = ds.Tables[0].Rows[0]["LastName"].ToString();
                                                string Email = ds.Tables[0].Rows[0]["Email"].ToString();
                                                string Phone = ds.Tables[0].Rows[0]["Phone"].ToString();
                                                string Location = ds.Tables[0].Rows[0]["Location"].ToString();
                                                string PDFFileName = ds.Tables[0].Rows[0]["PDFFileName"].ToString();
                                                DestinationFolder = ds.Tables[0].Rows[0]["DestinationFolder"].ToString();
                                                PackageName = ds.Tables[0].Rows[0]["PackageName"].ToString();
                                                StartDate = ds.Tables[0].Rows[0]["StartDate"].ToString();
                                                EndDate = ds.Tables[0].Rows[0]["EndDate"].ToString();
                                                string Amount = ds.Tables[0].Rows[0]["Amount"].ToString();
                                                //Comment = ds.Tables[0].Rows[0]["Comment"].ToString();
                                                StringBuilder MsgBody = new StringBuilder();
                                                string Subject = "YOUR " + ServiceName.ToUpper() + " IS RENEW ON DATE: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt");
                                                MsgBody.AppendLine(Subject);
                                                MsgBody.AppendLine("\nDear " + LastName + " [" + FirstName + "],\n");
                                                MsgBody.AppendLine("Greeting from NSM SOLUTION CO., LTD and thanks for using our services.\n");
                                                MsgBody.AppendLine("We would like to confirm your service is renew already on date: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt"));
                                                MsgBody.AppendLine("\nYour current service: " + ServiceName);
                                                MsgBody.AppendLine("Package Name: " + PackageName);
                                                MsgBody.AppendLine("Service ID: " + ServiceID);
                                                MsgBody.AppendLine("Connection ID: " + ConnectionID);
                                                MsgBody.AppendLine("Start Date: " + StartDate);
                                                MsgBody.AppendLine("Expired Date: " + EndDate);
                                                MsgBody.AppendLine("Amount USD: " + Amount);
                                                MsgBody.AppendLine("Phone Number: " + Phone);
                                                if (Email != "")
                                                    MsgBody.AppendLine("Email: " + Email);
                                                MsgBody.AppendLine("Renew By: " + html.UserFullName);
                                                MsgBody.AppendLine("\nThank you and best regards,\n");
                                                MsgBody.AppendLine("NSM SOLUTION AND MAINTENANCE CO.,LTD");
                                                MsgBody.AppendLine("#1159No1, Street Number 2, Phum Prek Tanu, Sangkat Chak Angre Leu, Khan Mean Chey, Phnom Penh.");
                                                MsgBody.AppendLine("Customer Service: 086 588 777, 078 588 777");
                                                MsgBody.AppendLine("Website: nsmisp.com.kh");
                                                Phone = Phone.Trim().Replace(" ", string.Empty);
                                                var regex = new Regex(Regex.Escape("0"));
                                                Phone = regex.Replace(Phone, "+855", 1);

                                                //notificationTelegram.NotificationToCustomer(Phone, FirstName, LastName, int.Parse(CustomerID), MsgBody.ToString(), DestinationFolder, PDFFileName);
                                                MsgBody.Clear();

                                                //MsgBody.AppendLine("Customer Name: " + LastName.ToUpper() + " [" + FirstName.ToUpper() + "] IS RENEW SUCCESSFUL.");
                                                MsgBody.AppendLine("\nDear TEAM,\n");
                                                MsgBody.AppendLine("We would like to confirm Customer Name: <b>" + LastName.ToUpper() + "[" + FirstName.ToUpper() + "]</b> is renew: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt"));
                                                MsgBody.AppendLine("\n<b>Service ID: " + ServiceID + "</b>");
                                                MsgBody.AppendLine("<b>Connection ID: " + ConnectionID + "</b>");
                                                MsgBody.AppendLine("<b>Package Name: " + PackageName + "</b>\n");
                                                MsgBody.AppendLine("<b>Start Date: " + StartDate + "</b>");
                                                MsgBody.AppendLine("<b>Expired Date: " + EndDate + "</b>");
                                                //MsgBody.AppendLine("Amount USD: " + Amount);
                                                MsgBody.AppendLine("<b>Phone Number: " + Phone + "</b>");
                                                if (Email != "")
                                                    MsgBody.AppendLine("Email: " + Email);
                                                MsgBody.AppendLine("\n<b>Renew By: " + html.UserFullName + "</b> @" + DateTime.Parse(html.GetCurrentDate()).ToString("dd/MMM/yyyy hh:mm:ss tt"));
                                                await notificationTelegram.NotificationTelegramBotToTopic(MsgBody.ToString(), -1903890245, 13); // send notification to technical renew
                                            }

                                            //sql = "select * from dbo.TblActivateCustomer where ActivateCustomerID = " + ActivateCustomerID;
                                            //ds = html.LoadData(sql);
                                            //DataRow drdata = ds.Tables[0].Rows[0];
                                            ////string ConType = drdata["ConnectionType"].ToString();
                                            ////string PPPoEUserName = dr["PPPoEUserName"].ToString();
                                            ////string PPPoEPassword = dr["PPPoEPassword"].ToString();
                                            ////string VLanName = dr["VLanName"].ToString();
                                            ////string VLanIPAddress = dr["VLanIPAddress"].ToString();
                                            ////string CustomerIPAddress = dr["IPAddress"].ToString();
                                            ////string GetwayIP = dr["GetwayIP"].ToString();
                                            //string TicketNumber = drdata["TicketNumber"].ToString();
                                            //string Comments = @"Dear Team, <br><br>" + Comment + ".";

                                            //Comments += @"<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO GENERATE TICKET.";

                                            //sql = @"insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID,
                                            //Status, PriorityID, Priority, OpenTicketID, FromUserLoginID, FromUserName,
                                            //ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
                                            //select SupportProjectID, StatusID, [Status], PriorityID, [Priority], 
                                            //OpenTicketID, UserLoginID FromUserLoginID, CreateBy FromUserName, " + html.UserID + @" ReplyFromUserLoginID, '" + html.UserFullName + @"' ReplyFromUserName, 
                                            //N'" + Comments + "' Comment, '" + html.GetCurrentDate() + @"' [Date], '" + html.UserFullName + @"' CreateBy, '" + html.GetCurrentDate() + @"'CreateDate, " + html.SystemID + " SystemID, " + html.WarehouseID + @" WarehouseID, 
                                            //'' DescriptionChangeStatus
                                            //from dbo.TblSupportOpenTicket
                                            //where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;";
                                            //Dictionary<string, string> para = new Dictionary<string, string>();
                                            //para.Add("TicketNumber", TicketNumber);
                                            //para.Add("SystemID", html.SystemID);
                                            //para.Add("WarehouseID", html.WarehouseID);
                                            //html.SaveChangeData(sql, false, para);
                                            //notificationTelegram.NotificationTelegramBot((Comments + " " + TicketNumber).Replace("<br>", "\n"));
                                        }
                                    }
                                }
                                else if (InvoiceType == "Created Invoice")
                                {
                                    sql = "select RouterID, PPPoEUserName, PPPoEPassword, PPPoEProfileName, ConnectivityStatus, ConnectionType, VLanID, VLanName, format(RenewDate, 'yyyy-MM-dd') RenewDate from dbo.TblActivateCustomer where ";
                                    sql += "ActivateCustomerID =" + ActivateCustomerID;
                                    ds = html.LoadData(sql);
                                    if (ds.Tables.Count > 0)
                                    {
                                        if (ds.Tables[0].Rows.Count > 0)
                                        {
                                            string RouterID = ds.Tables[0].Rows[0]["RouterID"].ToString();
                                            string VLanID = ds.Tables[0].Rows[0]["VLanID"].ToString();
                                            string VLanName = ds.Tables[0].Rows[0]["VLanName"].ToString();
                                            string ConnectionType = ds.Tables[0].Rows[0]["ConnectionType"].ToString();
                                            string ConnectivityStatus = ds.Tables[0].Rows[0]["ConnectivityStatus"].ToString();
                                            string PPPoEProfileName = ds.Tables[0].Rows[0]["PPPoEProfileName"].ToString();
                                            string RenewDate = ds.Tables[0].Rows[0]["RenewDate"].ToString();
                                            string Comment = "New Customer With Profile Name: " + PPPoEProfileName;
                                            string PPPoEUserName = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
                                            string PPPoEPassword = ds.Tables[0].Rows[0]["PPPoEPassword"].ToString();
                                            //if (RenewDate != "")
                                            //{
                                            //    if (DateTime.Parse(RenewDate).ToString("yyyyMM") != easternTimeNow.ToString("yyyyMM"))
                                            //    {
                                            Mikrotik MK = new Mikrotik();
                                            MK.SetConnection(int.Parse(RouterID));
                                            if (ConnectionType == "PPPoE")
                                            {
                                                if (ConnectivityStatus == "50")
                                                {
                                                    sqlresult = MK.ClearProfileName(PPPoEUserName);
                                                    Comment = "New Customer With Profile Name: " + PPPoEProfileName + " Date:" + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserFullName;
                                                }
                                                else if (ConnectivityStatus == "41")
                                                {
                                                    Comment = "Your Services was re-activated on " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + " By:" + html.UserFullName;
                                                    sqlresult = MK.ReActivateUser(PPPoEUserName, Comment);

                                                }
                                                //if (IsChangePlan == "False" || IsChangePlan == "")
                                                //    sqlresult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                                                //else
                                                //{
                                                //    DateTime SDate = DateTime.Parse(StartDate);
                                                //    if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                                //    {
                                                //        Comment = "Change from plan: " + PPPoEProfileName + " to " + PackageName + "; Change plan to Customer by: " + html.UserFullName + "; Change Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                                                //        PPPoEProfileName = PackageName;
                                                //        MK.ClearProfileName(PPPoEUserName);
                                                //        sqlresult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                                                //    }
                                                //}
                                            }
                                            else if (ConnectionType == "Static")
                                            {
                                                Comment = "New Customer With Vlan Name: " + VLanName;
                                                sqlresult = MK.EnableVlanID(VLanID, Comment);
                                            }
                                            else
                                            {
                                                sqlresult.Result = ResultType.Successful;
                                            }

                                            if (sqlresult.Result == ResultType.Successful)
                                            {
                                                sql = "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 40, ";
                                                sql += "ConnectivityStatusName = 'Active', IsAlreadyApplyProfile = 1 where InvoiceNo = '" + InvoiceNo + "'";
                                                sql += ";update dbo.TblCustomerInvoice set ConnectivityStatus = 40, ConnectivityStatusName = 'Active' ";
                                                sql += "where InvoiceNo = '" + InvoiceNo + "' and SystemID=" + html.SystemID + " and WarehouseID=" + html.WarehouseID + ";";
                                                if (IsChangePlan == "False" || IsChangePlan == "")
                                                {
                                                    sql += "update dbo.TblActivateCustomer set InvoiceNo = '" + InvoiceNo + "', BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                                    sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='" + html.UserFullName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                                    sql += ";";
                                                    sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, ConnectionType, ";
                                                    sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                                    sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                                    sql += "select ac.ActivateCustomerID, ac.ConnectionType, ac.CustomerID, ";
                                                    sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, '" + html.UserFullName + "' ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                                                    sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                                    sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                                                    //sql += "inner join dbo.TblCustomerBillingInvoice bi on bi.BillingInvoiceID = ac.BillingInvoiceID ";
                                                    sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                                }
                                                else
                                                {
                                                    DateTime SDate = DateTime.Parse(StartDate);
                                                    if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                                    {
                                                        sql += "update dbo.TblActivateCustomer set IsApplyChangePlan = 1, InvoiceNo = '" + InvoiceNo + "' and BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                                        sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='" + html.UserFullName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                                        sql += ";";
                                                        sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, BillingInvoiceID, ConnectionType, ";
                                                        sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                                        sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                                        sql += "select ac.ActivateCustomerID, ac.BillingInvoiceID, ac.ConnectionType, ac.CustomerID, ";
                                                        sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, '" + html.UserFullName + "' ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                                                        sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                                        sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                                                        sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                                    }
                                                }
                                                sqlresult = html.SaveChangeData(sql, false, false);

                                                //for notificationTelegram

                                                //sql = "select top 1 cb.GrandTotal Amount, s.ServiceName, c.CustomerCode FirstName, c.CustomerName LastName, bi.CompanyEmail Email, bi.CompanyContactMobileNumber Phone, bi.InstallProvinceCity [Location], 'Activation by: " + html.UserFullName + "; Activate Date: " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + "' Comment ";
                                                //sql += ", b.PDFFileName, b.DestinationFolder, case when b.invTmp = 1 then b.ServiceName + ' - ' + b.PackageName else b.ServiceName end PackageName, ";
                                                //sql += "format(b.StartDate, 'dd-MMM-yyyy') StartDate, format(b.EndDate, 'dd-MMM-yyyy') EndDate ";
                                                //sql += "from dbo.TblCustomer c ";
                                                //sql += "inner join dbo.TblCustomerBillInformation bi on bi.CustomerID = c.CustomerID ";
                                                //sql += "inner join dbo.TblCustomerBillingInvoice b on b.CustomerID = bi.CustomerID and b.CustomerBillInformationID = bi.CustomerBillInformationID ";
                                                //sql += "inner join dbo.TblServices s on s.ServiceID = b.ServiceID ";
                                                //sql += "inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID and cb.InvoiceNo = b.InvoiceNo ";
                                                //sql += "where b.InvoiceNo = '" + InvoiceNo + "' and c.CustomerID = " + CustomerID + " and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
                                                //ds = html.LoadData(sql);
                                                //string ServiceName = ds.Tables[0].Rows[0]["ServiceName"].ToString();
                                                //string FirstName = ds.Tables[0].Rows[0]["FirstName"].ToString();
                                                //string LastName = ds.Tables[0].Rows[0]["LastName"].ToString();
                                                //string Email = ds.Tables[0].Rows[0]["Email"].ToString();
                                                //string Phone = ds.Tables[0].Rows[0]["Phone"].ToString();
                                                //string Location = ds.Tables[0].Rows[0]["Location"].ToString();
                                                //string PDFFileName = ds.Tables[0].Rows[0]["PDFFileName"].ToString();
                                                //DestinationFolder = ds.Tables[0].Rows[0]["DestinationFolder"].ToString();
                                                //PackageName = ds.Tables[0].Rows[0]["PackageName"].ToString();
                                                //StartDate = ds.Tables[0].Rows[0]["StartDate"].ToString();
                                                //EndDate = ds.Tables[0].Rows[0]["EndDate"].ToString();
                                                //string Amount = ds.Tables[0].Rows[0]["Amount"].ToString();
                                                //Comment = ds.Tables[0].Rows[0]["Comment"].ToString();
                                                //StringBuilder MsgBody = new StringBuilder();
                                                //string Subject = "YOUR " + ServiceName.ToUpper() + " IS ACTIVATED ON DATE: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt");
                                                //MsgBody.AppendLine(Subject);
                                                //MsgBody.AppendLine("\nDear " + LastName + " [" + FirstName + "],\n");
                                                //MsgBody.AppendLine("Greeting from NSM SOLUTION CO., LTD and thanks for using our services.\n");
                                                //MsgBody.AppendLine("We would like to confirm your service is activated on date: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt"));
                                                //MsgBody.AppendLine("\nYour current service: " + ServiceName);
                                                //MsgBody.AppendLine("Package Name: " + PackageName);
                                                //MsgBody.AppendLine("Start Date: " + StartDate);
                                                //MsgBody.AppendLine("Expired Date: " + EndDate);
                                                //MsgBody.AppendLine("Amount USD: " + Amount);
                                                //MsgBody.AppendLine("Phone Number: " + Phone);
                                                //if (Email != "")
                                                //    MsgBody.AppendLine("Email: " + Email);
                                                //MsgBody.AppendLine("Created By: " + html.UserFullName);
                                                //MsgBody.AppendLine("\nThank you and best regards,\n");
                                                //MsgBody.AppendLine("NSM SOLUTION AND MAINTENANCE CO.,LTD");
                                                //MsgBody.AppendLine("#1159No1, Street Number 2, Phum Prek Tanu, Sangkat Chak Angre Leu, Khan Mean Chey, Phnom Penh.");
                                                //MsgBody.AppendLine("Customer Service: 086 588 777, 078 588 777");
                                                //MsgBody.AppendLine("Website: nsmisp.com.kh");
                                                //Phone = Phone.Trim().Replace(" ", string.Empty);
                                                //var regex = new Regex(Regex.Escape("0"));
                                                //Phone = regex.Replace(Phone, "+855", 1);
                                                //NotificationTelegram notificationTelegram = new NotificationTelegram();
                                                //notificationTelegram.NotificationToCustomer(Phone, FirstName, LastName, int.Parse(CustomerID), MsgBody.ToString(), DestinationFolder, PDFFileName);
                                                //MsgBody.Clear();

                                                //MsgBody.AppendLine("Customer Name: " + LastName.ToUpper() + " [" + FirstName.ToUpper() + "] IS ACTIVATED SUCCESSFUL.");
                                                //MsgBody.AppendLine("\nDear TEAM,\n");
                                                //MsgBody.AppendLine("We would like to confirm Customer Name: " + LastName.ToUpper() + "[" + FirstName.ToUpper() + "] is renew on date: " + easternTimeNow.ToString("dd-MMM-yyyyy HH:mm:ss tt"));
                                                //MsgBody.AppendLine("\nServices: " + ServiceName);
                                                //MsgBody.AppendLine("Package Name: " + PackageName);
                                                //MsgBody.AppendLine("Start Date: " + StartDate);
                                                //MsgBody.AppendLine("Expired Date: " + EndDate);
                                                //MsgBody.AppendLine("Amount USD: " + Amount);
                                                //MsgBody.AppendLine("Phone Number: " + Phone);
                                                //if (Email != "")
                                                //    MsgBody.AppendLine("Email: " + Email);
                                                //MsgBody.AppendLine("Created By: " + html.UserFullName);
                                                //notificationTelegram.NotificationTelegramBot(MsgBody.ToString());

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value,
                OfficialReceiptData = OfficialReceiptData
            }, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "Customer Payments")]
        public ActionResult OfficialReceipt(int PaymentID, int CompanyID, string OfficialReceiptNo, string InvoiceNo, int SystemID = 0, int WarehouseID = 0, bool GetContainerSection = false)
        {
            HTML html = new HTML();
            if (SystemID == 0)
                SystemID = int.Parse(html.SystemID);
            if (WarehouseID == 0)
                WarehouseID = int.Parse(html.WarehouseID);

            // Get Company Data
            string sql = @" select CompanyNameKH, CompanyName, Logo
                            from dbo.TblCompany where SystemID = @SystemID and WarehouseID = @WarehouseID and CompanyID = @CompanyID";
            Dictionary<string, string> Para = new Dictionary<string, string>
            {
                { "CompanyID", CompanyID.ToString() },
                { "SystemID", SystemID.ToString() },
                { "WarehouseID", WarehouseID.ToString() }
            };
            DataSet dsCompany = html.LoadData(sql, false, Para);

            if (dsCompany.Tables[0].Rows.Count > 0)
            {
                DataRow drComp = dsCompany.Tables[0].Rows[0];
                // URL-encode the filename so spaces (e.g. "NSM Logo_803417095.png") don't break the img src
                string logoFileName = Uri.EscapeDataString(drComp["Logo"].ToString());
                ViewBag.CompanyLogo = html.ServerRoot + "/Content/CompanyLogo/" + logoFileName;
                ViewBag.CompanyKHName = drComp["CompanyNameKH"].ToString();
                ViewBag.CompanyName = drComp["CompanyName"].ToString();
            }

            // Get Receipt Data
            sql = @"SELECT TOP 1
                               CASE
                                   WHEN ISNULL(c.CustomerName, '') <> '' AND LOWER(c.CustomerName) NOT LIKE '%[a-z]%' AND cc.CustomerGender = 'Male'   THEN N'លោក '    + REPLACE(c.CustomerName, N'លោក', '')
                                   WHEN ISNULL(c.CustomerName, '') <> '' AND LOWER(c.CustomerName) NOT LIKE '%[a-z]%' AND cc.CustomerGender = 'Female' THEN N'អ្នកស្រី ' + REPLACE(REPLACE(c.CustomerName, N'លោកស្រី', ''), N'អ្នកស្រី', '')
                                   WHEN ISNULL(c.CustomerName, '') <> '' AND cc.CustomerGender = 'Male'   THEN 'Mr. ' + REPLACE(REPLACE(c.CustomerName, 'Mr. ', ''), 'Mr.', '')
                                   WHEN ISNULL(c.CustomerName, '') <> '' AND cc.CustomerGender = 'Female' THEN 'Ms. ' + REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(c.CustomerName, 'Mrs. ', ''), 'Mrs.', ''), 'Miss. ', ''), 'Miss.', ''), 'Ms. ', ''), 'Ms.', '')
                                   ELSE c.CustomerName
                               END AS Payer,
                               p.PaymentAmount,
                               p.PaymentDate,
                               co.AccountName,
                               p.OfficialReceiptNo,
                               p.InvoiceNo,
                               pm.PaymentMethod,
                               p.ChequeNumber,
                               p.PaymentDescription,
                               p.ReferenceNo,
                               p.PaidBy,
                               u.Signature AS ReceiverSignature,
                               c.CompanyContactMobileNumber AS Phone,
                               (c.BillToAddress + ', ' + c.BillToCommune + ', '
                                + c.BillToDistrict + ', ' + c.BillToProvinceCity) AS [Address]
                        FROM dbo.TblPayment p
                        LEFT JOIN dbo.TblPaymentMethod pm 
                               ON pm.PaymentMethodID = p.PaymentMenthod
                        LEFT JOIN dbo.TblChartOfAccount co 
                               ON co.ChartOfAccountID = p.ChartOfAccountID
                        LEFT JOIN dbo.TblUserLogin u 
                               ON u.FullName = p.PaidBy
                        LEFT JOIN dbo.TblCustomerBillInformation c 
                               ON c.CustomerID = p.CustomerID
                        LEFT JOIN dbo.TblCustomer cc 
                               ON cc.CustomerID = c.CustomerID
                        WHERE p.PaymentID = @PaymentID;";
            Para.Clear();
            Para.Add("PaymentID", PaymentID.ToString());
            DataSet dsReceipt = html.LoadData(sql, false, Para);

            if (dsReceipt.Tables[0].Rows.Count > 0)
            {
                DataRow dr = dsReceipt.Tables[0].Rows[0];
                ViewBag.ReceiptNo = dr["OfficialReceiptNo"].ToString();

                DateTime paymentDate = Convert.ToDateTime(dr["PaymentDate"]);
                DateTime receiptDate = paymentDate;
                string referenceNo = dr["ReferenceNo"].ToString();

                // If this is a settlement payment using an existing receipt, fetch original receipt date
                if (!string.IsNullOrEmpty(referenceNo))
                {
                    string sqlRef = "select top 1 PaymentDate from dbo.TblPayment where OfficialReceiptNo = @RefNo and (InvoiceNo = '' or InvoiceNo is null)";
                    Dictionary<string, string> refPara = new Dictionary<string, string> { { "RefNo", referenceNo } };
                    DataSet dsRef = html.LoadData(sqlRef, false, refPara);
                    if (dsRef.Tables.Count > 0 && dsRef.Tables[0].Rows.Count > 0)
                    {
                        receiptDate = Convert.ToDateTime(dsRef.Tables[0].Rows[0]["PaymentDate"]);
                    }
                }

                ViewBag.ReceiptDate = paymentDate.ToString("dd.MM.yyyy");
                ViewBag.OriginalReceiptDate = receiptDate.ToString("dd.MM.yyyy");
                ViewBag.PaymentDate = paymentDate.ToString("dd.MM.yyyy");

                ViewBag.Payer = dr["Payer"].ToString();
                ViewBag.InvoiceNo = dr["InvoiceNo"].ToString();

                double amount;
                double.TryParse(dr["PaymentAmount"].ToString(), out amount);
                ViewBag.Amount = amount.ToString("#,#.00");

                ConvertToWords convertToWords = new ConvertToWords();
                ViewBag.SumOfDollars = convertToWords.MoneyToWords(amount);
                ViewBag.PaymentMethod = dr["PaymentMethod"]?.ToString() ?? "";
                ViewBag.DepositAccount = dr["AccountName"]?.ToString() ?? "";
                ViewBag.ChequeNumber = dr["ChequeNumber"]?.ToString() ?? "";
                ViewBag.Comment = dr["PaymentDescription"]?.ToString() ?? "";
                ViewBag.Receiver = dr["PaidBy"]?.ToString() ?? "";
                ViewBag.Phone = dr["Phone"]?.ToString() ?? "";
                ViewBag.Telephone = dr["Phone"]?.ToString() ?? "";
                ViewBag.Address = dr["Address"]?.ToString() ?? "";

                string receiverSig = dr["ReceiverSignature"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(receiverSig))
                {
                    string[] s1 = receiverSig.Split('~');
                    ViewBag.ReceiverSignature = s1[0];
                }
                else
                {
                    ViewBag.ReceiverSignature = "";
                }
            }

            if (ViewBag.ReceiptNo != null && ViewBag.ReceiptNo.ToString().StartsWith("I"))
            {
                return View("OfficialReceiptISP");
            }

            return View();
        }
        //public async Task<JsonResult> OfficialReceiptToPDF(int PaymentID, int CompanyID, string OfficialReceiptNo, string InvoiceNo, int SystemID = 0, int WarehouseID = 0)
        //{
        //    HTML html = new HTML();
        //    if (SystemID == 0)
        //        SystemID = int.Parse(html.SystemID);
        //    if (WarehouseID == 0)
        //        WarehouseID = int.Parse(html.WarehouseID);
        //    SqlResult sqlResult = new SqlResult();
        //    string sql = @" select CompanyNameKH, CompanyName, VATNumber, AddressKH, CompanyAddress, 
        //                    CustomerServicePhonNo, AccountingPhonNo, Email, Website, Logo
        //                    from dbo.TblCompany where SystemID = @SystemID and WarehouseID = @WarehouseID and CompanyID = @CompanyID";
        //    Dictionary<string, string> Para = new Dictionary<string, string>();
        //    Para.Add("CompanyID", CompanyID.ToString());
        //    Para.Add("SystemID", SystemID.ToString());
        //    Para.Add("WarehouseID", WarehouseID.ToString());
        //    DataSet ds = html.LoadData(sql, false, Para);
        //    DataRow dr = ds.Tables[0].Rows[0];
        //    string FilePath = HttpContext.Server.MapPath("~/Templates/OfficialReceipt.html");
        //    StreamReader str = new StreamReader(FilePath);
        //    string Text = str.ReadToEnd();
        //    str.Close();
        //    Text = Text.Replace("/", html.ServerRoot);
        //    Text = Text.Replace("{CompanyLogo}", html.ServerRoot + "/Content/CompanyLogo/" + dr["Logo"].ToString());
        //    Text = Text.Replace("{CompanyKHName}", dr["CompanyNameKH"].ToString());
        //    Text = Text.Replace("{CompanyName}", dr["CompanyName"].ToString());
        //    Text = Text.Replace("{CompanyVAT}", dr["VATNumber"].ToString());
        //    Text = Text.Replace("{CompanyAddressKH}", dr["AddressKH"].ToString());
        //    Text = Text.Replace("{CompanyAddress}", dr["CompanyAddress"].ToString());
        //    Text = Text.Replace("{CustomerService}", dr["CustomerServicePhonNo"].ToString());
        //    Text = Text.Replace("{AccountingPhonNo}", dr["AccountingPhonNo"].ToString());
        //    Text = Text.Replace("{CompanyEmail}", dr["Email"].ToString());
        //    Text = Text.Replace("{CompanyWebsite}", dr["Website"].ToString());

        //    sql = @"select cc.CompanyName, cc.CompanyNameKH, cc.CompanyDeskPhone, (cc.CompanyAddress + ', ' + cc.CompanyCommune + ', ' + 
        //            cc.CompanyDistrict + ', ' + cc.CompanyProvinceCity + '.') CustomerAddress, c.ATTName, c.CustomerCode, 
        //            c.CustomerName, cc.CompanyContactMobileNumber, cc.VATNumber, p.OfficialReceiptNo ReceiptNo, 
        //            format(p.PaymentDate, 'dd-MM-yyyy HH:mm:ss tt') ReceiptDate, p.PaidBy, pm.PaymentMethod
        //            from dbo.TblCustomer c
        //            inner join dbo.TblPayment p on p.CustomerID = c.CustomerID
        //            inner join dbo.TblCustomerCompany cc on cc.CustomerID = p.CustomerID
        //            inner join dbo.TblPaymentMethod pm on pm.PaymentMethodID = p.PaymentMenthod
        //            where p.PaymentID = @PaymentID";
        //    Para.Clear();
        //    Para.Add("PaymentID", PaymentID.ToString());
        //    ds = html.LoadData(sql, false, Para);
        //    dr = ds.Tables[0].Rows[0];
        //    Text = Text.Replace("{Title}", "Official Receipt for Customer Name:" + dr["CustomerName"].ToString());
        //    Text = Text.Replace("{CustomerCompanyKH}", dr["CompanyNameKH"].ToString());
        //    Text = Text.Replace("{CustomerCompany}", dr["CompanyName"].ToString());
        //    Text = Text.Replace("{CustomerPhone}", dr["CompanyContactMobileNumber"].ToString());
        //    Text = Text.Replace("{CustomerAddress}", dr["CustomerAddress"].ToString());
        //    Text = Text.Replace("{CustomerVAT}", dr["VATNumber"].ToString());
        //    Text = Text.Replace("{ReceiptNo}", dr["ReceiptNo"].ToString());
        //    Text = Text.Replace("{ReceiptDate}", dr["ReceiptDate"].ToString());
        //    Text = Text.Replace("{PaymentMethod}", dr["PaymentMethod"].ToString());

        //    sql = @"select format(i.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, i.InvoiceNo,
        //            s.ServiceName, case when i.invTmp = 1 then i.ItemDescription else i.ServiceName end [Description],
        //            i.Amount, b.Paid, b.Unpaid from dbo.TblCustomerInvoice i
        //            left join dbo.TblServices s on s.ServiceID = i.ServiceID
        //            inner join dbo.TblCustomerBalance b on b.ReceiptNo = i.ReceiptNo and b.InvoiceNo = @InvoiceNo
        //            where i.ReceiptNo = @OfficialReceiptNo and i.SystemID = @SystemID and i.WarehouseID = @WarehouseID and i.InvoiceNo = @InvoiceNo
        //            union all
        //            select top 1 format(i.InvoiceDate, 'dd-MM-yyyy') InvoiceDate, i.InvoiceNo,
        //            s.ServiceName, 'VAT 10% = ' + convert(varchar, b.VAT) [Description], b.VAT, b.Paid, b.Unpaid
        //            from dbo.TblCustomerInvoice i
        //            left join dbo.TblServices s on s.ServiceID = i.ServiceID
        //            inner join dbo.TblCustomerBalance b on b.ReceiptNo = i.ReceiptNo and b.InvoiceNo = @InvoiceNo
        //            where i.ReceiptNo = @OfficialReceiptNo and isnull(b.VAT, 0) <> 0 and i.SystemID = @SystemID and i.WarehouseID = @WarehouseID and i.InvoiceNo = @InvoiceNo";
        //    Para.Clear();
        //    Para.Add("OfficialReceiptNo", OfficialReceiptNo);
        //    Para.Add("InvoiceNo", InvoiceNo);
        //    Para.Add("SystemID", SystemID.ToString());
        //    Para.Add("WarehouseID", WarehouseID.ToString());
        //    ds = html.LoadData(sql, false, Para);
        //    StringBuilder sb = new StringBuilder();
        //    int i = 0;
        //    string d = "", d2 = "";
        //    foreach (DataRow drow in ds.Tables[0].Rows)
        //    {
        //        d = drow["InvoiceDate"].ToString();
        //        i++;
        //        if (d != d2)
        //        {
        //            sb.AppendLine("<tr><td colspan=\"2\">");
        //            sb.Append("ថ្ងៃខែចេញវិក្កយបត្រ៖ " + drow["InvoiceDate"] + " លេខវិក្កយបត្រ៖ " + drow["InvoiceNo"]);
        //            sb.Append("</td></tr>");
        //            //sb.AppendLine("<td>" + drow["InvoiceDate"] +"</td>");
        //            //sb.Append("<td>" + drow["InvoiceNo"] +"</td>");
        //            //sb.Append("<td>" + drow["Description"] + "</td>");
        //            //sb.Append("<td align=\"right\">" + drow["Amount"] + "</td></tr>");
        //            sb.AppendLine("<tr><td align=\"right\">" + i + "</td>");
        //            //sb.AppendLine("<td>" + drow["InvoiceDate"] +"</td>");
        //            //sb.Append("<td>" + drow["InvoiceNo"] +"</td>");
        //            sb.Append("<td>" + drow["Description"] + "</td>");
        //            sb.Append("<td align=\"right\">" + drow["Amount"] + "</td></tr>");
        //        }
        //        else
        //        {
        //            sb.AppendLine("<tr><td align=\"right\">" + i + "</td>");
        //            //sb.AppendLine("<td>" + drow["InvoiceDate"] +"</td>");
        //            //sb.Append("<td>" + drow["InvoiceNo"] +"</td>");
        //            sb.Append("<td>" + drow["Description"] + "</td>");
        //            sb.Append("<td align=\"right\">" + drow["Amount"] + "</td></tr>");
        //        }

        //        d2 = drow["InvoiceDate"].ToString();
        //    }
        //    for (int j = i; j <= 8; j++)
        //    {
        //        sb.AppendLine("<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>");
        //    }
        //    Text = Text.Replace("{TRData}", sb.ToString());
        //    sb.Clear();
        //    ConvertToWords convertToWords = new ConvertToWords();
        //    sb.AppendLine("<tr><td colspan=\"2\" align=\"right\">ប្រាក់បានទទួល<br />Received Amount</td><td align=\"right\">" + ds.Tables[0].Rows[0]["Paid"] + "</td></tr>");
        //    sb.AppendLine("<tr><td colspan=\"2\" align=\"right\">ប្រាក់នៅខ្វះ<br />Unpaid Amount</td><td align=\"right\">" + ds.Tables[0].Rows[0]["Unpaid"] + "</td></tr>");
        //    sb.AppendLine("<tr><td align=\"right\">In Word:</td><td colspan=\"2\" align=\"center\" class=\"text-uppercase font-weight-bold text-primary\">" + convertToWords.MoneyToWords(double.Parse(ds.Tables[0].Rows[0]["Paid"].ToString())) + "</td></tr>");
        //    Text = Text.Replace("{TFData}", sb.ToString());

        //    sqlResult = await html.HTMLToPDF(Text.ToString(), "Uploads\\CustomerAttFiles", "Customername");
        //    return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        //}
    }
}