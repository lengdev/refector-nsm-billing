using MVC;
using NSMSOLUTION.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Report.AccountingReport
{
    public class GeneralLedgerController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Read(DateTime? fromDate, DateTime? toDate, string account)
        {
            try
            {
                var data = GetGeneralLedger(fromDate, toDate, account.Trim());

                return Json(new
                {
                    Success = true,
                    Total = data.Count,
                    Data = data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<GeneralLedgerViewModel> GetGeneralLedger(
            DateTime? fromDate,
            DateTime? toDate, string account)
        {
            List<GeneralLedgerViewModel> result =
                new List<GeneralLedgerViewModel>();
            HTML html = new HTML();
            string connStr = html.GetConnectionString();

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string sql = @";WITH GL AS
(
    SELECT
        JE.EntryID,
        JE.TransactionID,
        JE.AccountID,

        COA.AccountNo,
        COA.AccountName,

        ISNULL(PCOA.AccountNo,'')   AS ParentAccountNo,
        ISNULL(PCOA.AccountName,'') AS ParentAccountName,

        AT.AccountTypesID,
        AT.AccountTypes,
        AT.NormalSide AS EffectiveNormalSide,

        CONVERT(date, JE.TransactionDate) AS TransactionDate,
        case when TT.Type = 'Expense' OR ISNULL(TT.IsPosted,0) = 1 then
            '<a href=""javascript:void(0);"" onclick=""showPaymentVoucher('+convert(varchar, TT.ExpenseID)+', '''+TT.ExpenseNo+''', ''Checked'')"">'+TT.InvoiceNo+'</a>'
        else
            '<a href=""javascript:void(0);"" onclick=""PrintInvoice('+ convert(varchar, C.CustomerID) +', '''+JE.InvoiceNo+''', 0, 10, '''', false, this);"">'+JE.InvoiceNo+'</a>'
        end
        AS ReferenceNo,

        case when TT.Type = 'Expense' OR ISNULL(TT.IsPosted,0) = 1 then 
            '<a href=""#!/VendorCenter?id='+ convert(varchar, V.VendorID) +'"" target=""_blank"">' + V.VendorCode + '</a>'
        else
            '<a href=""#!/CustomerProfile?ID='+ convert(varchar, C.CustomerID) +'"" target=""_blank"">' + C.CustomerCode + '</a>'
        end
        AS CustomerCode,
        case when TT.Type = 'Expense' OR ISNULL(TT.IsPosted,0) = 1 then 
            '<a href=""#!/VendorCenter?id='+ convert(varchar, V.VendorID) +'"" target=""_blank"">' + V.VendorName + '</a>'
        else
            '<a href=""#!/CustomerProfile?ID='+ convert(varchar, C.CustomerID) +'"" target=""_blank"">' + case when isnull(C.CustomerName, '') <> '' then C.CustomerName else C.CustomerNameLatin end + '</a>'
        end
        AS CustomerName,

        ISNULL(JE.Description,'') AS Description,

        ISNULL(JE.Debit,0) AS Debit,
        ISNULL(JE.Credit,0) AS Credit,

        JE.CustomerInvoiceID,
        JE.PackageDetailID,
        JE.ItemID,
        JE.OtherChargeID,

        TT.Type,

        ISNULL(CA.ContraAccount,'') AS ContraAccount,
        CL.ClassName
    FROM dbo.TblAccount_JournalEntries JE

    INNER JOIN dbo.TblChartOfAccount COA
        ON COA.ChartOfAccountID = JE.AccountID

    LEFT JOIN dbo.TblChartOfAccount PCOA
        ON PCOA.ChartOfAccountID = COA.ParentAccountID

    INNER JOIN dbo.TblAccountTypes AT
        ON AT.AccountTypesID = COA.AccountTypesID

    LEFT JOIN dbo.TblCustomer C
        ON C.CustomerID = JE.CustomerID

    INNER JOIN dbo.TblAccount_Transaction TT
        ON TT.TransactionID = JE.TransactionID
    
    LEFT JOIN dbo.TblVendor V
        ON V.VendorID = TT.VendorID

    INNER JOIN dbo.TblClass CL ON CL.ClassID = TT.ClassNameID
    OUTER APPLY
    (
        SELECT TOP 1
            COA2.AccountName AS ContraAccount
        FROM dbo.TblAccount_JournalEntries JE2
        INNER JOIN dbo.TblChartOfAccount COA2
            ON COA2.ChartOfAccountID = JE2.AccountID
        WHERE
            JE2.TransactionID = JE.TransactionID
            AND JE2.AccountID <> JE.AccountID

            AND
            (
                (
                    ISNULL(JE2.CustomerInvoiceID,0) = ISNULL(JE.CustomerInvoiceID,0)
                    AND ISNULL(JE2.PackageDetailID,0) = ISNULL(JE.PackageDetailID,0)
                    AND ISNULL(JE2.ItemID,0) = ISNULL(JE.ItemID,0)
                    AND ISNULL(JE2.OtherChargeID,0) = ISNULL(JE.OtherChargeID,0)
                )

                OR

                (
                    JE.Debit > 0
                    AND JE2.Credit = JE.Debit
                )

                OR

                (
                    JE.Credit > 0
                    AND JE2.Debit = JE.Credit
                )
            )

        ORDER BY
            CASE
                WHEN
                    ISNULL(JE2.CustomerInvoiceID,0) = ISNULL(JE.CustomerInvoiceID,0)
                    AND ISNULL(JE2.PackageDetailID,0) = ISNULL(JE.PackageDetailID,0)
                    AND ISNULL(JE2.ItemID,0) = ISNULL(JE.ItemID,0)
                    AND ISNULL(JE2.OtherChargeID,0) = ISNULL(JE.OtherChargeID,0)
                THEN 0
                ELSE 1
            END,
            JE2.EntryID
    ) CA

    WHERE
    (
        TT.Type <> 'Expense'
        OR ISNULL(TT.IsPosted,0) = 1
    )
    AND
    (@FromDate IS NULL OR JE.TransactionDate >= @FromDate)
    AND
    (@ToDate IS NULL OR JE.TransactionDate < DATEADD(DAY,1,@ToDate))
    AND
    (
        ISNULL(@Account,'') = ''
        OR COA.AccountNo LIKE '%' + @Account + '%'
        OR COA.AccountName LIKE '%' + @Account + '%'
        OR PCOA.AccountNo LIKE '%' + @Account + '%'
        OR PCOA.AccountName LIKE '%' + @Account + '%'
    )
)

SELECT
    AccountTypes,

    ParentAccountNo,
    ParentAccountName,

    CASE
        WHEN ISNULL(ParentAccountNo,'') <> ''
        THEN ParentAccountNo
        ELSE AccountNo
    END AS GroupAccountNo,

    CASE
        WHEN ISNULL(ParentAccountName,'') <> ''
        THEN ParentAccountName
        ELSE AccountName
    END AS GroupAccountName,

    CASE
        WHEN ISNULL(ParentAccountNo,'') <> ''
        THEN ParentAccountNo + ' - ' + ParentAccountName
        ELSE AccountNo + ' - ' + AccountName
    END AS GroupAccount,

    CASE
        WHEN ParentAccountNo <> ''
        THEN ParentAccountNo + ' - ' + ParentAccountName
        ELSE ''
    END AS ParentAccount,

    AccountNo,
    AccountName,

    TransactionDate,
    Type,
    ReferenceNo,

    CustomerCode,
    CustomerName,

    Description,

    ContraAccount,

    Debit,
    Credit,

    CASE
        WHEN EffectiveNormalSide = 'DR'
        THEN
            SUM(Debit - Credit)
            OVER
            (
                PARTITION BY AccountID
                ORDER BY TransactionDate, EntryID
                ROWS UNBOUNDED PRECEDING
            )
        ELSE
            SUM(Credit - Debit)
            OVER
            (
                PARTITION BY AccountID
                ORDER BY TransactionDate, EntryID
                ROWS UNBOUNDED PRECEDING
            )
    END AS RunningBalance,

    SUM(Debit)
    OVER
    (
        PARTITION BY
        CASE
            WHEN ISNULL(ParentAccountNo,'') <> ''
            THEN ParentAccountNo
            ELSE AccountNo
        END
    ) AS GroupDebitTotal,

    SUM(Credit)
    OVER
    (
        PARTITION BY
        CASE
            WHEN ISNULL(ParentAccountNo,'') <> ''
            THEN ParentAccountNo
            ELSE AccountNo
        END
    ) AS GroupCreditTotal,
    ClassName
FROM GL

ORDER BY
    AccountTypesID,

    CASE
        WHEN ISNULL(ParentAccountNo,'') <> ''
        THEN ParentAccountNo
        ELSE AccountNo
    END,

    AccountNo,
    TransactionDate,
    EntryID;
";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue(
    "@FromDate",
    fromDate.HasValue
        ? (object)fromDate.Value
        : DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@ToDate",
                        toDate.HasValue
                            ? (object)toDate.Value
                            : DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@Account",
                        string.IsNullOrWhiteSpace(account)
                            ? (object)DBNull.Value
                            : account);

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new GeneralLedgerViewModel
                            {
                                AccountNo = dr["AccountNo"].ToString(),
                                AccountName = dr["AccountName"].ToString(),
                                TransactionDate = Convert.ToDateTime(dr["TransactionDate"]),
                                Type = dr["Type"]?.ToString(),
                                ReferenceNo = dr["ReferenceNo"]?.ToString(),
                                CustomerCode = dr["CustomerCode"]?.ToString(),
                                CustomerName = dr["CustomerName"]?.ToString(),
                                Description = dr["Description"]?.ToString(),
                                ContraAccount = dr["ContraAccount"]?.ToString(),
                                Debit = Convert.ToDecimal(dr["Debit"]),
                                Credit = Convert.ToDecimal(dr["Credit"]),
                                RunningBalance = Convert.ToDecimal(dr["RunningBalance"]),
                                ParentAccountNo = dr["ParentAccountNo"]?.ToString(),
                                ParentAccountName = dr["ParentAccountName"]?.ToString(),
                                ParentAccount = dr["ParentAccount"]?.ToString(),
                                GroupAccountNo = dr["GroupAccountNo"]?.ToString(),
                                GroupAccountName = dr["GroupAccountName"]?.ToString(),
                                GroupAccount = dr["GroupAccount"]?.ToString(),
                                ClassName = dr["ClassName"]?.ToString(),

                                GroupDebitTotal = Convert.ToDecimal(dr["GroupDebitTotal"]),
                                GroupCreditTotal = Convert.ToDecimal(dr["GroupCreditTotal"]),
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}