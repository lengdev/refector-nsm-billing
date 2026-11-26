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

namespace NSMSOLUTION.Controllers
{
    [Authorize]
    public class CustomerCenterController : Controller
    {
        // GET: CustomerCenter
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadCustomer()
        {
            HTML html = new HTML();
            string sql = @"select 
    c.CustomerID, 
    c.CustomerCode + ' - ' + c.CustomerName as CustomerName, c.CustomerCode, c.CustomerName CustName
from 
    dbo.TblCustomer c
inner join 
    dbo.TblCustomerInvoice ci 
    on ci.CustomerID = c.CustomerID
where ci.[Type] = 'Official Invoice'
	and ci.StatusID in (10,11)
    and c.SystemID = " + html.SystemID + @" 
    and c.WarehouseID = " + html.WarehouseID + @"
    and c.DomainID like '%" + html.DomainID + @"%'
group by 
    c.CustomerID, 
    c.CustomerCode, 
    c.CustomerName
order by 
    c.CustomerID desc, c.CustomerCode;
";

            var Data = html.JsonData(sql);
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCustomerBalance(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select isnull(sum(GrandTotal),0) Total, count(distinct InvoiceNo) Invoice, isnull(sum(Paid),0) Paid, isnull(sum(Unpaid),0) AR, 
(isnull(sum(DepositFee),0) + isnull((select sum(Paid) from dbo.TblCustomerPayment where CustomerID = @CustomerID and (InvoiceNo is null or InvoiceNo = '')),0)) DepositFee 
from dbo.TblCustomerBalance where CustomerID = @CustomerID and StatusID not in (12,13,14,20) and [Type]='Official Invoice'";
            Dictionary<string, string> para = new Dictionary<string, string> {
                { "CustomerID", CustomerID.ToString() }
            };
            var Data = html.JsonData(sql, false, para);
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }

        public string GetTransactions(int CustomerID)
        {
            HTML html = new HTML();

            string sql = $@"
SELECT 
    ROW_NUMBER() OVER (ORDER BY CustomerBalanceID DESC) AS [No],
    (
        '<a href=""javascript:void(0);"" onclick=""PrintInvoice(' + 
        CAST({CustomerID} AS varchar) + ', ''' + InvoiceNo + ''', ' + 
        CONVERT(varchar, IsIncludedVAT) + ', ' + CONVERT(varchar, StatusID) + ', '''', false, this);"">' +
        InvoiceNo + '</a>'
    ) AS InvoiceNo,
    InvoiceDate, 
    PaidDate, 
    GrandTotal, 
    Paid, 
    Unpaid AS OpenBlance
FROM dbo.TblCustomerBalance 
WHERE 
    CustomerID = {CustomerID}
    AND Type IN ('Official Invoice', 'Income')
    AND StatusID IN (10,11)
    AND DomainID LIKE '%{html.DomainID}%'";

            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "CustomerBalanceID desc";

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice#", FieldName = "InvoiceNo", TextAlign = "center" },
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", Format = "dd-MM-yyyy", TextAlign = "center" },
                new GridField() { Header = "Paid Date", FieldName = "PaidDate", Format = "dd-MM-yyyy", TextAlign = "center" },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", Format = "#,0#.00", TextAlign = "right" },
                new GridField() { Header = "Paid", FieldName = "Paid", Format = "#,0#.00", TextAlign = "right",
                IsTotalFooter = true, Colspan = 4, TotalLabel = "Total:"},
                new GridField() { Header = "Open Blance", FieldName = "OpenBlance", Format = "#,0#.00", TextAlign = "right",
                IsTotalFooter = true, Colspan = 4},

            };
            html.ShowPrintOnTopTable = false;
            //return html.DrawHTMLSearchForm(1, "LoadCustomerInvoicesTransactions", true, html.GH.GridTable.ToJsonString());
            return html.DrawHTMLSearchForm(1, "LoadCustomerInvoicesTransactions");
            //var results = new List<object>();
            //int total = 0;

            //string connectionString = ConfigurationManager.ConnectionStrings["AuthContextNSM"].ConnectionString;
            //using (SqlConnection conn = new SqlConnection(connectionString))
            //{
            //    conn.Open();

            //    string countQuery = @"
            //SELECT COUNT(*) 
            //FROM [dbo].[TblAccount_Transaction] 
            //WHERE CustomerID = @CustomerID 
            //AND (@SearchTerm = '' OR InvoiceNo LIKE '%' + @SearchTerm + '%')";

            //    using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
            //    {
            //        countCmd.Parameters.AddWithValue("@CustomerID", CustomerID);
            //        countCmd.Parameters.AddWithValue("@SearchTerm", searchTerm ?? "");
            //        total = (int)countCmd.ExecuteScalar();
            //    }

            //    string dataQuery = @"
            //SELECT [Type] AS TranType, TransactionDate AS TranDate, InvoiceNo AS Number, Amount 
            //FROM [dbo].[TblAccount_Transaction]
            //WHERE CustomerID = @CustomerID
            //AND (@SearchTerm = '' OR InvoiceNo LIKE '%' + @SearchTerm + '%')
            //ORDER BY InvoiceNo
            //OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            //    using (SqlCommand cmd = new SqlCommand(dataQuery, conn))
            //    {
            //        cmd.Parameters.AddWithValue("@CustomerID", CustomerID);
            //        cmd.Parameters.AddWithValue("@SearchTerm", searchTerm ?? "");
            //        cmd.Parameters.AddWithValue("@Skip", skip);
            //        cmd.Parameters.AddWithValue("@Take", take);

            //        using (SqlDataReader reader = cmd.ExecuteReader())
            //        {
            //            while (reader.Read())
            //            {
            //                results.Add(new
            //                {
            //                    TranType = reader["TranType"].ToString(),
            //                    TranDate = Convert.ToDateTime(reader["TranDate"]).ToString("dd-MM-yyyy hh:mm tt"),
            //                    Number = reader["Number"].ToString(),
            //                    Amount = Convert.ToDecimal(reader["Amount"])
            //                });
            //            }
            //        }
            //    }
            //}

            //return Json(new { data = results, total = total }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddCollection(string PrimaryFieldValue)
        {
            HTML html = new HTML();
            ViewBag.CustomerID = PrimaryFieldValue;

            ViewBag.UserFullName = html.UserFullName;
            ViewBag.UserID = html.UserID;

            string sql = "select CustomerID, CustomerCode + ' - ' + CustomerName CustomerName, CustomerCode from dbo.TblCustomerInvoice where CustomerID = @CustomerID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "CustomerID",  PrimaryFieldValue}
            };
            DataSet custData = html.LoadData(sql, false, para);
            ViewBag.CustomerCode = custData.Tables[0].Rows[0]["CustomerCode"].ToString();

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
            string sqlCustomer = @"select distinct top 10 c.CustomerID, c.CustomerCode + ' - ' + c.CustomerName CustomerName,
c.CustomerName CustName, c.CustomerCode CID from dbo.TblCustomer c
inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + @" and ci.Type = 'Official Invoice' and ci.StatusID not in (12,13,14,20)
and lower(c.CustomerCode + ' - ' + c.CustomerName) like @SearchText
order by c.CustomerCode + ' - ' + c.CustomerName";

            html.FormID = "FrmAddCollection";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Invoice Type:",
                ID = "InvoiceType",
                SQL_CBO = "select 'Receipt NSM' ID, 'Receipt NSM' Name union select 'Receipt ISP' ID, 'Receipt ISP' Name order by ID desc",
                DisplayText = "Name",
                Value = "ID",
                LabelAlign = "left",
                CallBackFunction = "ChangeInvoiceType(this)"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Number:",
                ID = "RVNumber",
                Required = true,
                LabelAlign = "left",
                SQLPreFix = "declare @Prefix nvarchar(50), @Module nvarchar(50) = 'Receipt NSM'; select @Prefix = Prefix from dbo.TblPrefix where Module = 'Receipt NSM' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ",'" + html.DomainID + "', 1, '', 0, ''",
                CallBackToControlID = "RVNumber, RVNumber_PrefixOR",
                UpdatePrefixModule = "Receipt NSM",
                Prefix = "PrefixOR",
                SetPrefixToDbFieldName = "RVNumber",
                HiddenField = "PrefixOR",
                IsReadOnly = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                ID = "txtCustomerName",
                Label = "Customer Name:",
                SQL_AutoComplete = sqlCustomer,
                ISAutoComplete = true,
                LabelAlign = "left",
                HiddenField = "CustomerID, CustName, CID",
                HiddenFieldValue = "0,0,0",
                DisplayOnFields = "CustomerID,CustName, CID",
                Value = custData.Tables[0].Rows[0]["CustomerName"].ToString(),
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                SingleDatePicker = true,
                Label = "Payment Date:",
                ID = "PaymentDate",
                Required = true,
                LabelAlign = "left"
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Payment Method:",
                ID = "PaymentMenthod",
                Required = true,
                SQL_CBO = "select PaymentMethodID, PaymentMethod from dbo.TblPaymentMethod " +
                "where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by case when PaymentMethod = 'Cash' then 0 else 1 end, PaymentMethod",
                DisplayText = "PaymentMethod",
                Value = "PaymentMethodID",
                LabelAlign = "left"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Cheque No.:",
                ID = "ChequeNumber",
                DBField = "ChequeNumber",
                LabelAlign = "left"
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Deposit To:",
                ID = "ChartOfAccountID",
                SQL_CBO = sql_deposit,
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
                LabelAlign = "left"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "PaymentDescription",
                Required = true,
                LabelAlign = "left"
            });


            ViewBag.DrawHTMLForm = html.DrawHTMLForm();

            html = new HTML();

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Sub Total:",
                ID = "SubTotal",
                Required = true,
                LabelAlign = "left",
                //IsReadOnly = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Discount:",
                ID = "Discount",
                LabelAlign = "left",
                Value = "0",
                //IsReadOnly = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Other Charge:",
                ID = "OtherCharge",
                LabelAlign = "left",
                Value = "0",
                //IsReadOnly = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Grand Total:",
                ID = "GrandTotal",
                LabelAlign = "left",
                Value = "0",
                //IsReadOnly = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Amount Received:",
                ID = "PaymentAmount",
                Required = true,
                LabelAlign = "left",
                //IsReadOnly = true,
                Class = "text-danger form-control text-lg",
            });

            ViewBag.DrawHTMLFormTotal = html.DrawHTMLForm();



            html = new HTML();
            html.FormData = new HTMLControl[]
            {
            new FormData()
            {
                htmlControl = new HTMLControl[]
                                {
                        new HtmlMyTable()
                        {
                            ShowFooter=false,
                            PrimaryFieldName="ExpensesItemID",
                            TableName="TblAccount_ExpensesCategory",
                            GetIDENTITYByFiled="ExpensesID",
                            HtmlTblID="TblExpensesCategory",
                            InputName="ExpenseDescription",
                            TRName="TRNo",
                            TDName="TDNo",
                            ScriptCallBackFunction="Reload_ExpenseCategory",
                            ApplyTblTHBackground=false,
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>", Label="#"
                                },
                                new TblTHead()
                                {
                                    Label="Expense Account", Class="col-3",
                                },
                                new TblTHead()
                                {
                                    Label="Description", Class="col-6",
                                },
                                new TblTHead()
                                {
                                     Label="Amount",
                                     Class="col-auto",
                                     CloseTR="</tr>"
                                }
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TRNo\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TDNo"
                                        },
                                        new HtmlComboBox()
                                        {
                                            Name="ExpenseCategory",
                                            Class="input2", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Category",
                                            SQL_CBO=@"select distinct coa.ChartOfAccountID ExpensesAccountID, ac.AccountCategory, act.AccountTypes, coa.AccountNo, coa.AccountName
from dbo.TblAccountTypes act
inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID
inner join dbo.TblChartOfAccount coa on coa.AccountType = act.AccountTypesID
where act.SystemID = " + html.SystemID + " and act.WarehouseID = " + html.WarehouseID,
                                            DBField = "ExpensesAccountID",
                                            Value = "ExpensesAccountID",
                                            DisplayText = "AccountName",
                                            GroupByField = "AccountTypes",
                                            IsKendoMultiColumnComboBox = true,
                                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                                            {
                                                new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                                                new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "550" }
                                            },
                                            KendoMultiColumnComboBoxHeight = 350,
                                            //RequiredInputIfOtherFieldIsNotNull = true,
                                        },
                                        new HtmlTextarea()
                                        {
                                            Name="ExpenseDescription",
                                            Class="input2", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Description",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ExpenseDescription','Reload_ExpenseCategory');",
                                            DBField="ExpenseDescription",
                                            //RequiredInputIfOtherFieldIsNotNull = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ExpenseAmount",
                                            Class="input2 text-center", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Amount",
                                            DBField="ExpenseAmount", ISNumeric = true,
                                            OnKeyupFunction="SumOtherCharge()"
                                        }
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                                }
            }
            };
            html.FormData = new HTMLControl[]
                {
               new FormData()
               {
                   htmlControl = new HTMLControl[]
                   {
                       new HtmlFile()
                       {

                           ID = "AttachmentFile",
                           Class = "input2 CustInfo",
                           IsCustomForm = true,
                           DBField = "AttachmentFile",
                           AcceptImageOnly = true,  // Allow all files (not just images) for payment slips/receipts
                           DestinationFile = $"Uploads/CustomerAttFiles/{ViewBag.CustomerCode}/BankSlip",
                           Multiple = true,
                           UploadFilesV2 = true,
                           TableName = "TblPayment",  // Adjust table name as needed
                           PrimaryFieldName = "PaymentID",  // Adjust field name as needed
                           PrimaryFieldValue = PrimaryFieldValue
                       },
                   }
               }
                };
            ViewBag.DrawHTMLFormAttachment = html.DrawHTMLFormV1();
            //ViewBag.DrawHTMLFormOtherCharge = html.DrawHTMLFormV1();
            return View();
        }

        public ActionResult LoadAllCustInvoice(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select CustomerID, InvoiceNo, InvoiceDate, GrandTotal, Unpaid OpenBalance, DiscountUSD
from dbo.TblCustomerBalance where CustomerID = @CustomerID
and Type='Official Invoice' and StatusID =11 order by CustomerBalanceID desc";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "CustomerID", CustomerID.ToString()}
            };
            var Data = html.JsonData(sql, false, para);
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }



        [Authorize(Roles = "Customer Payments")]
        public async Task<ActionResult> SaveChangePayment(object[] PaymentData, object[] OtherChargeData)
        {
            SqlResult sqlresult = new SqlResult();
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML();
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(PaymentData[0].ToString());
            dynamic OtherChargeDataInfo = new JavaScriptSerializer().DeserializeObject(OtherChargeData[0].ToString());
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            // bank slip 
            // File Upload Handling
            IDictionary<string, object> firstItem = (IDictionary<string, object>)(((object[])DataInfo).Length > 0 ? ((object[])DataInfo)[0] : ((object[])OtherChargeDataInfo)[0]);
            string CustomerID = firstItem["CustomerID"].ToString();
            string InvoiceNo = firstItem.ContainsKey("InvoiceNo") ? firstItem["InvoiceNo"].ToString() : "";
            string CustomerName = HttpUtility.UrlDecode(firstItem["CustomerName"].ToString()); // Changed from CustName
            string memo = HttpUtility.UrlDecode(firstItem["PaymentDescription"].ToString());
            string CID = HttpUtility.UrlDecode(firstItem["CustomerCode"].ToString()); // Changed from CID
            CustomerName = $"{html.ToUrlSlug($"{CustomerName} [{CID}]")}";
            string DestinationFolder = "/Uploads/CustomerAttFiles/" + CustomerName + "/BankSlip/";
            string newbots = "";
            try
            {
                string sqls = @"SELECT top 1 BotToken FROM dbo.TblTelegramBots WHERE SystemID = @SystemID AND IsActive = 1 ORDER BY CreateDate DESC";
                Dictionary<string, string> botParams = new Dictionary<string, string> { { "SystemID", html.SystemID } };
                DataSet newbotData = html.LoadData(sqls, false, botParams);
                if (newbotData != null && newbotData.Tables.Count > 0 && newbotData.Tables[0].Rows.Count > 0)
                {
                    newbots = newbotData.Tables[0].Rows[0]["BotToken"]?.ToString() ?? "";
                }
            }
            catch { }

            string sqlGuest = @"SELECT 
                                CASE 
                                    WHEN COALESCE(NULLIF(CustomerName, ''),CustomerNameLatin) NOT LIKE '%[a-z]%' AND CustomerGender = 'Male' 
                                        THEN N'លោក ' + COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin)

                                    WHEN COALESCE(NULLIF(CustomerName, ''), CustomerNameLatin) NOT LIKE '%[a-z]%' AND CustomerGender = 'Female' 
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
            sqlresult = html.UploadFile(DestinationFolder);
            string AttachmentFile;
            if (sqlresult.Result == ResultType.Successful)
            {
                AttachmentFile = sqlresult.Value;

                List<string> invoiceList = new List<string>();
                decimal totalPaymentAmount = 0;

                foreach (IDictionary<string, object> item in (object[])DataInfo)
                {
                    string inv = item.ContainsKey("InvoiceNo") ? item["InvoiceNo"]?.ToString() : "";
                    if (!string.IsNullOrEmpty(inv) && !invoiceList.Contains(inv))
                        invoiceList.Add(inv);

                    if (item.ContainsKey("PaymentAmount") && item["PaymentAmount"] != null)
                        totalPaymentAmount += Convert.ToDecimal(item["PaymentAmount"]);
                }
                string RVNumber = firstItem.ContainsKey("RVNumber") ? firstItem["RVNumber"]?.ToString() : "";
                string allInvoices = invoiceList.Count > 0 ? string.Join(", ", invoiceList) : (string.IsNullOrEmpty(RVNumber) ? "N/A" : RVNumber);
                string payment = $"$ {totalPaymentAmount}";
                string ChartOfAccountName = "";
                string sql_acc = "select AccountName from dbo.TblChartOfAccount where ChartOfAccountID = " + firstItem["ChartOfAccountID"] + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
                DataSet ds_acc = html.LoadData(sql_acc);
                if (ds_acc.Tables[0].Rows.Count > 0)
                {
                    ChartOfAccountName = ds_acc.Tables[0].Rows[0]["AccountName"].ToString();
                    var match = Regex.Match(ChartOfAccountName, @"\(([^)]+)\)");
                    if (match.Success)
                        ChartOfAccountName = match.Groups[1].Value.Trim();
                }
                string PaymentAttachFolder = "/Uploads/CustomerAttFiles/" + CustomerName + "/BankSlip/";

                StringBuilder payMsg = new StringBuilder();
                payMsg.AppendLine("");
                payMsg.AppendLine("<b>We recieved payment from</b>");   
                payMsg.AppendLine("<b>Customer:</b> " + guest);
                payMsg.AppendLine("<b>Receipt/Invoice No:</b> " + allInvoices);
                payMsg.AppendLine("<b>Deposit To:</b> " + ChartOfAccountName);
                payMsg.AppendLine("<b>Amount :</b> " + payment);
                payMsg.AppendLine("<b>Received By:</b> " + html.UserFullName);
                payMsg.AppendLine("<b>Memo :</b> " + memo);

                string TelegramMessage = payMsg.ToString();

                try
                {
                    long paymentChatId = -1002999742690;
                    if (!string.IsNullOrEmpty(AttachmentFile))
                    {
                        string[] files = AttachmentFile.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

                        //Send message onced
                        await notificationTelegram.NotificationTelegramBot(payMsg.ToString(), paymentChatId);

                        //Send each as full inline photo
                        foreach (string file in files)
                        {
                            string fullFilePath = Server.MapPath(PaymentAttachFolder + file.Trim());
                            if (System.IO.File.Exists(fullFilePath))
                                await notificationTelegram.SendPhotoToGroup(fullFilePath, "", paymentChatId);
                        }
                    }
                    else
                    {
                        await notificationTelegram.NotificationTelegramBot(payMsg.ToString(), paymentChatId);
                    }
                }
                catch { }
            }
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

            //NotificationTelegram notificationTelegram = new NotificationTelegram();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<Data>");
            //sb.AppendLine("<AttachmentFile>" + html.EscapeXml(AttachmentFile) + "</AttachmentFile>"); // Added EscapeXml
            //sb.AppendLine("<DestinationFolder>" + html.EscapeXml(DestinationFolder) + "</DestinationFolder>"); // Added EscapeXml

            foreach (IDictionary<string, object> items in (object[])DataInfo)
            {
                sb.AppendLine("<PaymentInfo>");
                foreach (var item in items)
                {
                    string key = item.Key;
                    string value = item.Value == null ? "" : HttpUtility.UrlDecode(item.Value.ToString().Trim());
                      if (key == "PaymentDate")
                        value = html.StringToDate(value);
                    sb.AppendLine("<" + key + ">" + html.EscapeXml(value) + "</" + key + ">");
                }
                sb.AppendLine("<AttachmentFile>" + html.EscapeXml(AttachmentFile) + "</AttachmentFile>"); // Added EscapeXml
                sb.AppendLine("<DestinationFolder>" + html.EscapeXml(DestinationFolder) + "</DestinationFolder>"); // Added EscapeXml
                sb.AppendLine("</PaymentInfo>");
            }

            foreach (IDictionary<string, object> items in (object[])OtherChargeDataInfo)
            {
                sb.AppendLine("<OtherChargeInfo>");
                foreach (var item in items)
                {
                    string key = item.Key;
                    string value = item.Value == null ? "" : HttpUtility.UrlDecode(item.Value.ToString().Trim());
                    if (key == "PaymentDate")
                        value = html.StringToDate(value);
                    sb.AppendLine("<" + key + ">" + html.EscapeXml(value) + "</" + key + ">");
                }
                sb.AppendLine("</OtherChargeInfo>");
            }

            sb.AppendLine("</Data>");
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "Data", "N'"+sb.ToString()+"'" }
            };
            sqlresult = html.SaveChangeData("SP_INSERT_PAYMENT_ALLs", true, Para, true, true);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value,
                OfficialReceiptData = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNextReceiptNumber(string Module)
        {
            HTML html = new HTML();
            string safeModule = Module.Replace("'", "''");
            string sql = "declare @Prefix nvarchar(50); select @Prefix = Prefix from dbo.TblPrefix where Module = '" + safeModule + "' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, '" + safeModule + "', " + html.SystemID + ", " + html.WarehouseID + ",'" + html.DomainID + "', 1, '', 0, ''";
            DataSet ds = html.LoadData(sql, false, null);
            string number = "";
            string prefix = "";
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                number = ds.Tables[0].Rows[0][0].ToString();
                prefix = ds.Tables[0].Rows[0][1].ToString();
            }
            return Json(new { Number = number, Prefix = prefix }, JsonRequestBehavior.AllowGet);
        }

    }
}