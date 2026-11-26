using MVC;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NSMSOLUTION.Controllers.Vendor
{
    public class VendorCenterController : Controller
    {
        // GET: VendorCenter
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadVendor()
        {
            HTML html = new HTML();
            string sql = @"SELECT DISTINCT
    v.VendorID,
    v.VendorName,
    v.VendorCode
    FROM dbo.TblVendor v
    inner join dbo.Expenses e on e.PayeeID = v.VendorID and e.Status = 'Confirm Verified'
    where v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " and v.DomainID like '%" + html.DomainID + @"%'
ORDER BY v.VendorName;";

            var Data = html.JsonData(sql);
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadVendorInfo(int ID)
        {
            HTML html = new HTML();
            string SQL = @"SELECT [VendorID], [VendorType], [VendorCode], [VendorName], [Gender], [Address], [Phone], [Email]
      ,[Website], format([RegisterDate], 'dd-MM-yyyy') RegisterDate, [CreateBy], [VendorPicture], [ContactName], [ContactPhoneNumber]
      ,[PaymentTerms],[NumberofDays],[ContactPersonName],[Position],[VATTIN]
      ,[CompanyAddress], [CompanyName], [PrimaryFieldValue], [VendorProfilePicture]
  FROM [dbo].[TblVendor] WHERE VendorID = @VendorID;

SELECT 
    COUNT(*) AS TotalInvoices,

    SUM(ISNULL(TotalAmount, 0)) AS TotalAmount,

    SUM(ISNULL(TotalAmount, 0) - ISNULL(BalanceAmount, 0)) AS PaidAmount,

    SUM(ISNULL(BalanceAmount, 0)) AS BalanceAmount,

    SUM(
        CASE 
            WHEN (ISNULL(TotalAmount, 0) - ISNULL(BalanceAmount, 0)) <= 0
                THEN 1
            ELSE 0
        END
    ) AS UnpaidInvoices,

    SUM(
        CASE 
            WHEN (ISNULL(TotalAmount, 0) - ISNULL(BalanceAmount, 0)) > 0
             AND ISNULL(BalanceAmount, 0) > 0
                THEN 1
            ELSE 0
        END
    ) AS PartialPaidInvoices,

    SUM(
        CASE 
            WHEN ISNULL(BalanceAmount, 0) <= 0
                THEN 1
            ELSE 0
        END
    ) AS PaidInvoices,

    ISNULL((
        SELECT COUNT(DISTINCT COALESCE(NULLIF(ep.PaymentNo, ''), NULLIF(ep.PVNo, ''), CONVERT(nvarchar, ep.ExpensePaymentID)))
        FROM dbo.Expense_PaymentTransactions ep
        WHERE ep.PayeeID = @VendorID
          AND ep.[Status] = 'Confirm Verified'
          AND (ep.PaidAmount > 0 OR ep.PaymentStatus IN ('Paid', 'Partial Paid', 'Partial'))
    ), 0) AS TotalPayments

FROM dbo.Expenses
WHERE Status = 'Confirm Verified'
AND PayeeID = @VendorID;

-- ============================================
-- VENDOR DEPOSIT / ADVANCE SUMMARY
-- Available amount = RemainingAmount
-- ============================================

SELECT
    @VendorID AS VendorID,

    COUNT(*) AS OpenDepositCount,

    ISNULL(SUM(ISNULL(Amount, 0)), 0)
        AS TotalDepositAmount,

    ISNULL(SUM(ISNULL(AppliedAmount, 0)), 0)
        AS TotalAppliedAmount,

    ISNULL(SUM(ISNULL(RemainingAmount, 0)), 0)
        AS VendorAdvanceAvailable

FROM dbo.TblVendorDeposit
WHERE VendorID = @VendorID
  AND ISNULL(RemainingAmount, 0) > 0
  AND ISNULL(Status, '') NOT IN
      ('Cancelled', 'Canceled', 'Void', 'Voided',
       'Deleted', 'Closed', 'Fully Applied');
";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "VendorID", ID.ToString() }
            };
            return Json(new { data = html.LoadData(SQL, false, Para).ToJsonString() }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveChangeVendorProfilePicture(object[] Data)
        {
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(Data[0].ToString());
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            string VendorName = HttpUtility.UrlDecode(DataInfo["VendorName"]);
            string VendorCode = HttpUtility.UrlDecode(DataInfo["VendorCode"]);
            VendorName = $"{html.ToUrlSlug($"{VendorName + " [" + VendorCode + "]"}")}";
            string DestinationFolder = "/Uploads/Vendors/" + VendorName + "/Profile Picture/";
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
                    Data = sqlresult.Value
                }, JsonRequestBehavior.AllowGet);
            }
            string SQL = "update dbo.TblVendor set VendorPicture= @VendorPicture where VendorID = @VendorID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "VendorID", DataInfo["VendorID"].ToString() }
            };
            //para.Add("DestinationFile", DestinationFolder);
            if (AttachmentFile != "")
                para.Add("VendorPicture", DestinationFolder + AttachmentFile);
            else
                para.Add("VendorPicture", "");
            sqlresult = html.SaveChangeData(SQL, false, para);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result,
                Data = sqlresult.Value
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadVendorProfilePicture(int ID)
        {
            HTML html = new HTML();
            string SQL = "select VendorPicture from dbo.TblVendor ";
            SQL += "where VendorID=@VendorID ";
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "VendorID", ID.ToString() }
            };
            return Json(new { data = html.JsonData(SQL, false, Para) }, JsonRequestBehavior.AllowGet);
        }

        public string GetTransactions(int PayeeID, string PaymentStatus = "")
        {
            HTML html = new HTML();

            string sql = $@"SELECT 
ROW_NUMBER() over (order by ExpenseID desc) AS [No],
    ExpenseID,
    '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher('+convert(varchar, ExpenseID)+', '''+No+''', '''+Status+''')"">' + No + '</a>' AS InvoiceNo,
case when isnull(IsPosted, 0) = 1 then    
    PVNo
else
    ''
end PVNo,
    CreatedDate,
    PayeeID,
    p.PaymentMethod,
    case 
        when isnull(ChequeBankerName, '') <> '' then ChequeBankerName
    else BankerName end BankerName,
    case
        when isnull(ChequeTo, '') <> '' then ChequeTo
    else BankAccountName end BankAccountName,
    case
        when isnull(ChequeNo, '') <> '' then ChequeNo
    else BankAccountNo end BankAccountNo,
    TotalAmount Total,
    [Status],
    case when PaymentStatus= 'Unpaid' then '<span class=""badge bg-danger"">Unpaid</span>'
    when PaymentStatus= 'Paid' then '<span class=""badge bg-success"">Paid</span>'
    else '<span class=""badge bg-warning"">Partial Paid</span>' end PaymentStatus, PaidOnDate, 
    
    CASE
    -- Expense is already posted
    WHEN ISNULL(IsPosted, 0) = 1 THEN

        CASE
            -- Still has remaining balance
            WHEN ISNULL(BalanceAmount, TotalAmount) > 0 and ExpenseTransactionType <> 'Vendor Deposit' THEN
                CASE
                    -- No payment request yet
                    WHEN ISNULL(PostedPaymentNo, '') = '' THEN
                        '<button type=""button"" class=""btn btn-success btn-sm me-1""
                            onclick=""requestPayment('
                            + CONVERT(varchar, ExpenseID)
                            + ', '''
                            + No
                            + ''', '''
                            + PaymentNo
                            + ''', '''
                            + ExpenseNo
                            + ''', '''
                            + ISNULL(PostedPaymentNo,'')
                            + ''')"">
                            <i class=""fa fa-dollar-sign""></i> Request Payment
                        </button>'

                    -- Payment request can still be edited
                    WHEN ex.PStatus IN ('Draft', 'Confirm Rejected') THEN
                        '<button type=""button"" class=""btn btn-success btn-sm me-1""
                            onclick=""requestPayment('
                            + CONVERT(varchar, ExpenseID)
                            + ', '''
                            + No
                            + ''', '''
                            + PaymentNo
                            + ''', '''
                            + ExpenseNo
                            + ''', '''
                            + ISNULL(PostedPaymentNo,'')
                            + ''', '''
                            + ISNULL(convert(varchar, ExpensePaymentID),'')
                            + ''')"">
                            <i class=""bi bi-pencil-square""></i> Edit Payment
                        </button>'

                    -- Payment verified, waiting for posting
                    WHEN ex.PStatus = 'Confirm Verified'
                         AND ISNULL(IsPosted, 0) = 0 THEN
                        '<button type=""button"" class=""btn btn-primary btn-sm""
                            onclick=""postingPaymentVoucher('
                            + CONVERT(varchar, ExpenseID)
                            + ', '''
                            + No
                            + ''', '''
                            + Status
                            + ''', '''
                            + PaymentNo
                            + ''')"">
                            Post
                        </button>'
                     WHEN ex.PStatus = 'Confirm Verified' AND ISNULL(IsPosted, 0) = 1 THEN
                         '<span class=""badge bg-success"">Posted</span>'
                    -- Other payment status
                    ELSE
                        '<span class=""badge bg-warning text-uppercase"">'
                        + ISNULL(ex.PStatus,'Pending')
                        + '</span>'
                END

            -- Fully paid
            ELSE
                '<span class=""badge bg-success"">Posted</span>'
        END

    -- Expense not yet posted
    ELSE
        CASE
            WHEN ISNULL(PostedByUserID,'') = '' THEN
                '<button type=""button"" class=""btn btn-primary btn-sm""
                    onclick=""postingPaymentVoucher('
                    + CONVERT(varchar, ExpenseID)
                    + ', '''
                    + No
                    + ''', '''
                    + Status
                    + ''', '''
                    + PaymentNo
                    + ''')"">
                    Post
                </button>'

            ELSE
                 '<button type=""button"" class=""btn btn-primary btn-sm"" onclick=""postingPaymentVoucher('
                + CONVERT(varchar, ExpenseID)
                + ', '''
                + No
                + ''', '''
                + Status
                + ''', '''
                + PaymentNo
                + ''')"">
                Post
            </button>'
        END
END AS BTN

    , TotalAmount, PaidAmount, BalanceAmount, e.PostedDate,
case when (PStatus = 'Draft' OR PStatus = 'Checked') then
    '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher('+convert(nvarchar, ex.PExpenseID) +','''+ ex.PNo+''','''+ ex.PStatus+''','''+ ex.PPaymentNo+''');"">' + e.ExpenseNo +'</a>'
    else
    '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher('+convert(nvarchar, e.ExpenseID) +','''+e.[No]+''','''+e.[Status]+''','''+e.[PaymentNo]+''');"">' + e.ExpenseNo +'</a>'
    end
    ExpenseNo, ExpensePaymentID
FROM dbo.Expense_PaymentTransactions e
INNER JOIN dbo.TblPaymentMethod p on p.PaymentMethodID = e.PaymentMethodID
OUTER APPLY
(
    SELECT TOP (1)
        ep.ExpenseID AS PExpenseID,
        ep.No AS PNo,
        ep.Status AS PStatus,
        ep.PostedPaymentNo AS PPostedPaymentNo,
        ep.PaymentNo AS PPaymentNo
    FROM Expense_PaymentTransactions ep
    WHERE ep.PaymentNo = e.PostedPaymentNo
    ORDER BY ep.ExpenseID DESC
) ex
WHERE
    PaymentStatus = CASE
                        WHEN '{PaymentStatus}' IN ('Paid', 'Unpaid')
                            THEN '{PaymentStatus}'
                        ELSE PaymentStatus
                    END
    AND [Status] = 'Confirm Verified'
    AND PayeeID = " + PayeeID + @"";

            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.OrderBy = "ExpenseID desc";

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Expense No", FieldName = "ExpenseNo", TextAlign = "center" },
                new GridField() { Header = "PV", FieldName = "PVNo", TextAlign = "center" },
                new GridField() { Header = "Created Date", FieldName = "CreatedDate", Format = "dd-MM-yyyy", TextAlign = "center" },
                new GridField() { Header = "Posted Date", FieldName = "PostedDate", Format = "dd-MM-yyyy", TextAlign = "center" },
                new GridField() { Header = "Paid On Date", FieldName = "PaidOnDate", Format = "dd-MM-yyyy", TextAlign = "center" },
                new GridField() { Header = "Payment Method", FieldName = "PaymentMethod" },
                new GridField() { Header = "Banker Name", FieldName = "BankerName" },
                new GridField() { Header = "Acc.Name/Cheque To", FieldName = "BankAccountName" },
                new GridField() { Header = "Acc.No/Cheque No", FieldName = "BankAccountNo" },

                new GridField() { Header = "Total", FieldName = "TotalAmount", Format = "#,0#.00", TextAlign = "right" },
                new GridField() { Header = "Paid", FieldName = "PaidAmount", Format = "#,0#.00", TextAlign = "right" },
                new GridField() { Header = "Balance", FieldName = "BalanceAmount", Format = "#,0#.00", TextAlign = "right" },
                new GridField() { Header = "Status", FieldName = "PaymentStatus", TextAlign ="center" },
                new GridField() { Header = "Action", FieldName = "BTN", TextAlign ="center" },
                //new GridField() { Header = "Open Blance", FieldName = "OpenBlance", Format = "#,0#.00", TextAlign = "right",
                //IsTotalFooter = true, Colspan = 4},

            };
            html.ShowPrintOnTopTable = false;
            //return html.DrawHTMLSearchForm(1, "LoadCustomerInvoicesTransactions", true, html.GH.GridTable.ToJsonString());
            return html.DrawHTMLSearchForm(1, "LoadCustomerInvoicesTransactions");
        }


        public JsonResult LoadVendorHistory(int PayeeID, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "Transactions")
            {
                string sqlTrans = $@"SELECT
			    ROW_NUMBER() OVER (ORDER BY e.ExpenseID DESC) AS [No],
			    e.ExpenseID,
			    e.[No]                                                   AS BillNo,
			    case when (PStatus = 'Draft' OR PStatus = 'Checked') then
			    '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher('+convert(nvarchar, ex.PExpenseID) +','''+ ex.PNo+''','''+ ex.PStatus+''','''+ ex.PPaymentNo+''');"">' + e.ExpenseNo +'</a>'
			    else
			    '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher('+convert(nvarchar, e.ExpenseID) +','''+e.[No]+''','''+e.[Status]+''','''+e.[PaymentNo]+''');"">' + e.ExpenseNo +'</a>'
			    end
			    ExpenseNo,
			    format(e.CreatedDate,  'dd-MM-yyyy')                     AS CreatedDate,
			    format(e.PostedDate,   'dd-MM-yyyy')                     AS PostedDate,
			    format(e.PaidOnDate,   'dd-MM-yyyy')                     AS PaidOnDate,
			    e.PVNo,
			    pm.PaymentMethod,
			    CASE WHEN ISNULL(e.ChequeBankerName,'') <> '' THEN e.ChequeBankerName ELSE e.BankerName     END AS BankerName,
			    CASE WHEN ISNULL(e.ChequeTo,'')         <> '' THEN e.ChequeTo         ELSE e.BankAccountName END AS BankAccountName,
			    CASE WHEN ISNULL(e.ChequeNo,'')         <> '' THEN e.ChequeNo         ELSE e.BankAccountNo   END AS BankAccountNo,
			    e.PaidAmount                                             AS Debit,
			    e.TotalAmount                                            AS Credit,
			    e.BalanceAmount,
			    CASE
			        WHEN e.PaymentStatus = 'Unpaid'       THEN '<span class=""badge bg-danger"">Unpaid</span>'
			        WHEN e.PaymentStatus = 'Paid'         THEN '<span class=""badge bg-success"">Paid</span>'
			        ELSE                                       '<span class=""badge bg-warning"">Partial Paid</span>'
			    END AS PaymentStatus,
			    e.[Status]
			FROM dbo.Expense_PaymentTransactions e
			INNER JOIN dbo.TblPaymentMethod pm ON pm.PaymentMethodID = e.PaymentMethodID
			OUTER APPLY
			(
			    SELECT TOP (1)
			        ep.ExpenseID  AS PExpenseID,
			        ep.No         AS PNo,
			        ep.Status     AS PStatus,
			        ep.PaymentNo  AS PPaymentNo
			    FROM dbo.Expense_PaymentTransactions ep
			    WHERE ep.PaymentNo = e.PostedPaymentNo
			    ORDER BY ep.ExpenseID DESC
			) ex
			WHERE e.[Status] = 'Confirm Verified'
			  AND e.PayeeID  = {PayeeID}";

                html.GH.SQL = sqlTrans;
                html.OrderBy = "e.ExpenseID desc";
                html.ShowPaging = false;
                html.GH.GridTable = new GridField[]
                {
                    new GridField() { Header = "#",                  FieldName = "No",            TextAlign = "right"  },
                    new GridField() { Header = "Bill No",            FieldName = "BillNo",        TextAlign = "center" },
                    new GridField() { Header = "Expense No",         FieldName = "ExpenseNo",     TextAlign = "center" },
                    new GridField() { Header = "PV No",              FieldName = "PVNo",          TextAlign = "center" },
                    new GridField() { Header = "Created Date",       FieldName = "CreatedDate",   TextAlign = "center" },
                    new GridField() { Header = "Posted Date",        FieldName = "PostedDate",    TextAlign = "center" },
                    new GridField() { Header = "Paid On Date",       FieldName = "PaidOnDate",    TextAlign = "center" },
                    new GridField() { Header = "Payment Method",     FieldName = "PaymentMethod"                       },
                    new GridField() { Header = "Banker Name",        FieldName = "BankerName"                          },
                    new GridField() { Header = "Acc.Name/Cheque To", FieldName = "BankAccountName"                     },
                    new GridField() { Header = "Acc.No/Cheque No",   FieldName = "BankAccountNo"                       },
                    new GridField() { Header = "Debit (Payment)",    FieldName = "Debit",         TextAlign = "right", Format = "#,0#.00" },
                    new GridField() { Header = "Credit (Billed)",    FieldName = "Credit",        TextAlign = "right", Format = "#,0#.00" },
                    new GridField() { Header = "Balance",            FieldName = "BalanceAmount", TextAlign = "right", Format = "#,0#.00" },
                    new GridField() { Header = "Status",             FieldName = "PaymentStatus", TextAlign = "center" },
                };
                html.ShowPrintOnTopTable = false;
                return Json(new { Data = html.DrawHTMLSearchForm(1, "LoadVendorTransactionsGrid") }, JsonRequestBehavior.AllowGet);
            }

            string sql = $@"SELECT
			    ROW_NUMBER() OVER (ORDER BY e.ExpenseID DESC) AS [No],
			    e.ExpenseID,
			    e.[No]                                         AS BillNo,
			    e.ExpenseNo,
			    e.PVNo,
			    e.[Status],
			    ISNULL(u.FullName, '')                         AS CreatedUser,
			    FORMAT(e.CreatedDate, 'dd-MM-yyyy HH:mm')      AS CreatedDate,
			    ISNULL(e.CheckedBy, '')                        AS CheckedBy,
			    FORMAT(e.CheckedDate, 'dd-MM-yyyy HH:mm')      AS CheckedDate,
			    ISNULL(e.VerifiedBy, '')                       AS VerifiedBy,
			    FORMAT(e.VerifiedDate, 'dd-MM-yyyy HH:mm')     AS VerifiedDate,
			    ISNULL(e.ApprovedBy, '')                       AS ApprovedBy,
			    ISNULL(e.PostedBy, '')                         AS PostedBy,
			    FORMAT(e.PostedDate, 'dd-MM-yyyy HH:mm')       AS PostedDate
			FROM dbo.Expense_PaymentTransactions e
			LEFT JOIN dbo.TblUserLogin u ON u.UserLoginID = ISNULL(e.ModifyByUserID, e.CreateByUserID)
			WHERE e.PayeeID  = {PayeeID}";

            html.GH.SQL = sql;
            html.OrderBy = "e.ExpenseID desc";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#",             FieldName = "No",           TextAlign = "right"  },
                new GridField() { Header = "Bill No",       FieldName = "BillNo",       TextAlign = "center" },
                new GridField() { Header = "Expense No",    FieldName = "ExpenseNo",    TextAlign = "center" },
                new GridField() { Header = "PV No",         FieldName = "PVNo",         TextAlign = "center" },
                new GridField() { Header = "Status",        FieldName = "Status",       TextAlign = "center" },
                new GridField() { Header = "Created By",    FieldName = "CreatedUser"                        },
                new GridField() { Header = "Created Date",  FieldName = "CreatedDate",  TextAlign = "center" },
                new GridField() { Header = "Checked By",    FieldName = "CheckedBy"                          },
                new GridField() { Header = "Checked Date",  FieldName = "CheckedDate",  TextAlign = "center" },
                new GridField() { Header = "Verified By",   FieldName = "VerifiedBy"                         },
                new GridField() { Header = "Verified Date", FieldName = "VerifiedDate", TextAlign = "center" },
                new GridField() { Header = "Approved By",   FieldName = "ApprovedBy"                         },
                new GridField() { Header = "Posted By",     FieldName = "PostedBy"                           },
                new GridField() { Header = "Posted Date",   FieldName = "PostedDate",   TextAlign = "center" },
            };
            html.ShowPrintOnTopTable = false;
            return Json(new { Data = html.DrawHTMLSearchForm(1, "LoadVendorHistoryGrid") }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadVendorAttachmentFile(int ID)
        {
            HTML html = new HTML();
            string sql = @"
			SELECT 
			    ep.ExpensePaymentID                 AS ID,
			    ep.PayeeID                          AS VendorID,
			    '/Uploads/Expenses/'                AS DestinationFolder,
			    ep.FileName                         AS AttachmentFileName,
			    'Expense Bill'                      AS TypeFor,
			    ISNULL(NULLIF(ep.ExpenseNo, ''), ep.[No]) AS InvoiceNo,
			    ISNULL(ep.PVNo, '')                 AS PVNo,
			    ISNULL(ep.ExpenseNo, '')            AS ExpenseNo,
			    FORMAT(ep.CreatedDate, 'dd-MM-yyyy') AS FileDate
			FROM dbo.Expense_PaymentTransactions ep
			WHERE ep.PayeeID = @VendorID
			  AND ISNULL(ep.FileName, '') <> ''

			UNION ALL

			SELECT 
			    ep.ExpensePaymentID                 AS ID,
			    ep.PayeeID                          AS VendorID,
			    '/Uploads/Expenses/PostingDOC/'    AS DestinationFolder,
			    ep.PostingDOC                       AS AttachmentFileName,
			    'Posting Document'                  AS TypeFor,
			    ISNULL(NULLIF(ep.PVNo, ''), ep.[No]) AS InvoiceNo,
			    ISNULL(ep.PVNo, '')                 AS PVNo,
			    ISNULL(ep.ExpenseNo, '')            AS ExpenseNo,
			    FORMAT(ISNULL(ep.PostedDate, ep.CreatedDate), 'dd-MM-yyyy') AS FileDate
			FROM dbo.Expense_PaymentTransactions ep
			WHERE ep.PayeeID = @VendorID
			  AND ISNULL(ep.PostingDOC, '') <> ''

			ORDER BY FileDate DESC";

            var para = new Dictionary<string, string>
            {
                { "VendorID", ID.ToString() }
            };
            var ds = html.JsonData(sql, false, para);
            return Json(new { data = ds }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadVendorStatement(int PayeeID = 0, string FromDate = "", string ToDate = "", int APAccountID = 464, int VendorID = 0)
        {
            if (PayeeID == 0 && VendorID != 0) PayeeID = VendorID;
            HTML html = new HTML();

            DateTime now = DateTime.Now;
            DateTime dtFromDate = new DateTime(now.Year, now.Month, 1);
            DateTime dtToDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            string[] dateFormats = new[] { "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy/MM/dd", "dd/MM/yyyy" };

            if (!string.IsNullOrEmpty(FromDate))
            {
                DateTime parsedFrom;
                if (DateTime.TryParseExact(FromDate, dateFormats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedFrom) || DateTime.TryParse(FromDate, out parsedFrom))
                {
                    dtFromDate = parsedFrom;
                }
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                DateTime parsedTo;
                if (DateTime.TryParseExact(ToDate, dateFormats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedTo) || DateTime.TryParse(ToDate, out parsedTo))
                {
                    dtToDate = parsedTo;
                }
            }

            if (APAccountID <= 0)
            {
                APAccountID = 464;
            }

            string sql = $@";WITH Opening AS
			(
			    SELECT
			        ISNULL(SUM(JE.Credit - JE.Debit), 0) AS OpeningBalance
			    FROM dbo.TblAccount_JournalEntries JE
			    INNER JOIN dbo.TblAccount_Transaction AT
			        ON AT.TransactionID = JE.TransactionID
			    WHERE
			        AT.VendorID = {PayeeID}
			        AND JE.AccountID = {APAccountID}
			        AND JE.IsPosted = 1
			        AND JE.TransactionDate < '{dtFromDate:yyyy-MM-dd}'
			),
			StatementData AS
			(
			    -- Opening Balance
			    SELECT
			        0 AS SortOrder,
			        NULL AS EntryID,
			        NULL AS TransactionID,
			        CAST('{dtFromDate:yyyy-MM-dd}' AS DATETIME) AS TransactionDate,
			        '<span class=""badge bg-info"">Opening</span>' AS [Type],
			        'BF' AS Reference,
			        'Opening Balance' AS Description,
			        CAST(0 AS DECIMAL(18,2)) AS Debit,
			        CAST(0 AS DECIMAL(18,2)) AS Credit,
			        OpeningBalance AS BalanceChange
			    FROM Opening

			    UNION ALL

			    -- Current period transactions
			    SELECT
			        1 AS SortOrder,
			        JE.EntryID,
			        JE.TransactionID,
			        JE.TransactionDate,
			        CASE
			            WHEN COALESCE(NULLIF(JE.Type, ''), AT.Type) = 'Expense' THEN '<span class=""badge bg-danger"">Expense</span>'
			            WHEN COALESCE(NULLIF(JE.Type, ''), AT.Type) IN ('Expense Payment', 'Payment') THEN '<span class=""badge bg-success"">Payment</span>'
			            WHEN COALESCE(NULLIF(JE.Type, ''), AT.Type) = 'Journal' THEN '<span class=""badge bg-primary"">Journal</span>'
			            ELSE '<span class=""badge bg-secondary"">' + ISNULL(COALESCE(NULLIF(JE.Type, ''), AT.Type), 'Transaction') + '</span>'
			        END AS [Type],
			        CASE
			            WHEN ex.ExpenseID IS NOT NULL THEN
			                '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher(' + CONVERT(nvarchar, ex.ExpenseID) + ', ''' + ISNULL(ex.[No], '') + ''', ''' + ISNULL(ex.[Status], '') + ''', ''' + ISNULL(ex.PaymentNo, '') + ''');"">' + COALESCE(JE.ExpenseNo, JE.PaymentNo, JE.JournalNo) + '</a>'
			            ELSE
			                COALESCE(JE.ExpenseNo, JE.PaymentNo, JE.JournalNo)
			        END AS Reference,
			        AT.Description,
			        JE.Debit,
			        JE.Credit,
			        JE.Credit - JE.Debit AS BalanceChange
			    FROM dbo.TblAccount_JournalEntries JE
			    INNER JOIN dbo.TblAccount_Transaction AT
			        ON AT.TransactionID = JE.TransactionID
			    OUTER APPLY
			    (
			        SELECT TOP 1 
			            exp.ExpenseID,
			            exp.[No],
			            exp.[Status],
			            exp.PaymentNo
			        FROM dbo.Expenses exp
			        WHERE exp.ExpenseID = ISNULL(JE.ExpenseID, AT.ExpenseID)
			           OR (NULLIF(JE.ExpenseNo, '') IS NOT NULL AND exp.ExpenseNo = JE.ExpenseNo)
			           OR (NULLIF(AT.ExpenseNo, '') IS NOT NULL AND exp.ExpenseNo = AT.ExpenseNo)
			    ) ex
			    WHERE
			        AT.VendorID = {PayeeID}
			        AND JE.AccountID = {APAccountID}
			        AND JE.IsPosted = 1
			        AND JE.TransactionDate >= '{dtFromDate:yyyy-MM-dd}'
			        AND JE.TransactionDate < '{dtToDate.AddDays(1):yyyy-MM-dd}'
			),
			RunningBalance AS
			(
			    SELECT
			        *,
			        ROW_NUMBER() OVER (ORDER BY TransactionDate, SortOrder, EntryID) AS [No],
			        SUM(BalanceChange) OVER
			        (
			            ORDER BY
			                TransactionDate,
			                SortOrder,
			                EntryID
			            ROWS BETWEEN UNBOUNDED PRECEDING
			            AND CURRENT ROW
			        ) AS Balance
			    FROM StatementData
			)
			SELECT
			    [No],
			    EntryID,
			    SortOrder,
			    FORMAT(TransactionDate, 'dd-MM-yyyy') AS TransactionDate,
			    [Type],
			    Reference,
			    Description,
			    Debit,
			    Credit,
			    Balance
			FROM RunningBalance";

            html.GH.SQL = sql;
            html.OrderBy = "[No] asc";
            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#",           FieldName = "No",              TextAlign = "right"  },
                new GridField() { Header = "Date",        FieldName = "TransactionDate", TextAlign = "center" },
                new GridField() { Header = "Type",        FieldName = "Type",            TextAlign = "center" },
                new GridField() { Header = "Reference",   FieldName = "Reference",       TextAlign = "center" },
                new GridField() { Header = "Description", FieldName = "Description"                           },
                new GridField() { Header = "Debit",       FieldName = "Debit",           TextAlign = "right", Format = "#,##0.00" },
                new GridField() { Header = "Credit",      FieldName = "Credit",          TextAlign = "right", Format = "#,##0.00" },
                new GridField() { Header = "Balance",     FieldName = "Balance",         TextAlign = "right", Format = "#,##0.00" },
            };
            html.ShowPrintOnTopTable = false;
            return Json(new { Data = html.DrawHTMLSearchForm(1, "LoadVendorStatementGrid") }, JsonRequestBehavior.AllowGet);
        }

    }
}