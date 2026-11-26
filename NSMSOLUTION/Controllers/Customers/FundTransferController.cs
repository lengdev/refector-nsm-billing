using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Utilities;

namespace NSMSOLUTION.Controllers.Customers
{
    [Authorize]
    public class FundTransferController : Controller
    {
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "Fund Transfers"
            };

            if (html.HasPermission("Add Fund Transfer"))
            {
                html.AllowAdd = true;
            }
            if (html.HasPermission("Edit Fund Transfer"))
            {
                html.AllowEdit = true;
            }
            if (html.HasPermission("Delete Fund Transfer"))
            {
                html.AllowDelete = true;
            }


            // html.LoadData(alterSql, false);

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New";
            html.AddURL = "FundTransfer/CreateFundTransfer";
            html.TableName = "dbo.TblFundTransfer";
            html.PrimaryField = "FundTransferID";
            html.OrderBy = "FundTransferID desc";

            // 1. Search Filters
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DateRangePicker = true,
                LabelFromDate = "Date Range:",
                DBField = "ft.TransferDate"
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Status:",
                DBField = "ft.StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90, 91, 92, 102)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtPaymentMethod",
                Label = "Payment Method:",
                DBField = "ft.PaymentMethod",
                SQL_CBO = "SELECT DISTINCT PaymentMethod FROM dbo.TblFundTransfer",
                DisplayText = "PaymentMethod",
                Value = "PaymentMethod",
                ComboBox = true
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtRefNo",
                Label = "Ref No:",
                DBField = "ft.RefNo",
                SQL_CBO = "SELECT DISTINCT RefNo FROM dbo.TblFundTransfer",
                DisplayText = "RefNo",
                Value = "RefNo",
                ComboBox = true
            });

            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Accounts Name",
                SQL = @"select ChartOfAccountID, AccountName from TblChartOfAccount where DomainID = '" + html.DomainID + "' and AccountTypesID = 1",
                DisplayField = "AccountName",
                SelectValue = "ChartOfAccountID",
                RadioValue = EncryAndDecry.Encryp("ft.FromAccountID"),
                DefaultCheckBok = true,
            });

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ft.ClassName",
                SQL_CBO = "SELECT DISTINCT ClassName FROM dbo.TblFundTransfer",
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true,

            });

            string sql = @"select ROW_NUMBER() Over(order by ft.FundTransferID desc) as [No], 
ft.FundTransferID, 
ISNULL(facc.AccountName, 'N/A') as FromAccount, 
ISNULL(tacc.AccountName, 'N/A') as ToAccount, 
'<a class=""text-primary"" style=""cursor: pointer;"" onclick=""showFundTransferForm('+convert(varchar, ft.FundTransferID)+', '''+ ft.RefNo +''')"">'+ft.RefNo+'</a>' as RefNo, 
ft.TransferAmount, ft.TransferDate, ft.Memo, ft.AttachmentFile,
case when ft.StatusID = 90 then '<span class=""badge badge-primary"">Approve</span>' 
  when ft.StatusID = 91 then '<span class=""badge badge-danger"">Disapprove</span>'
  when ft.StatusID = 92 then '<span class=""badge badge-info"">Check</span>'
  when ft.StatusID = 102 then '<span class=""badge badge-success"">Verify</span>'
  else '<span class=""badge badge-warning"">Pending</span>'
end as [Status], ft.StatusID
from dbo.TblFundTransfer ft
left join dbo.TblChartOfAccount facc on facc.ChartOfAccountID = ft.FromAccountID
left join dbo.TblChartOfAccount tacc on tacc.ChartOfAccountID = ft.ToAccountID
where ft.SystemID = " + html.SystemID + " and ft.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;

            html.GH.GridPara = new GridPara[] { };
            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.GridEventClick = "onGridClick(this);";
            html.ShowPaging = false;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#",            FieldName = "No",             TextAlign = "right" },
                new GridField() { Header = "Ref No",       FieldName = "RefNo", TextAlign = "center" },
                new GridField() { Header = "From Account", FieldName = "FromAccount" },
                new GridField() { Header = "To Account",   FieldName = "ToAccount" },
                new GridField() { Header = "Amount",       FieldName = "TransferAmount",  TextAlign = "right",  Format = "#,#0.00" },
                new GridField() { Header = "Date",         FieldName = "TransferDate",    FormatDateTime = "dd-MM-yyyy", TextAlign = "center" },
                new GridField() { Header = "Memo",         FieldName = "Memo" },
                new GridField() { Header = "Status",       FieldName = "Status", TextAlign = "center" },
                new GridField() { Header = "Action",       FieldName = "FundTransferID", TextAlign = "Center", IsModifyField = true, DeleteFunction = "DeleteFundTransfer(this)" }
            };

            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = false
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(
                1,
                "",
                true,
                html.GH.GridTable.ToList().ToJsonString(),
                kendoGridSetting.ToJsonString()
            );

            return View();
        }

        [Authorize(Roles = "Add Fund Transfer, Edit Fund Transfer")]
        public ActionResult CreateFundTransfer(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            try
            {
                bool isClone = Type == "Clone";
                html.isCloned = isClone;
                html.SaveChangeFunction = "SaveChangeFundTransfer()";
                html.SaveChangeCallbackFunction = "SaveChangeCallbackFunction(response);";
                html.FormID = "FundTransferForm";
                // Schema check consolidated in Index action
                html.TableName = "dbo.TblFundTransfer";
                html.PrimaryField = "FundTransferID";
                html.PrimaryFieldValue = PrimaryFieldValue;
                html.UseTable = true;
                html.TableColumn = 2;

                string sysID = html.SystemID ?? "0";
                string whID = html.WarehouseID ?? "0";

                string sql_acc = @"SELECT 
                        coa.ChartOfAccountID, 
                        coa.AccountNo,
                        coa.AccountName AS AccountNameOnly,
                        at.AccountTypes,
                        '$' + CONVERT(varchar, CAST(
                            CASE WHEN at.NormalSide = 'DR' 
                                THEN ISNULL(coa.DebitBalance,0) - ISNULL(coa.CreditBalance,0)
                                ELSE ISNULL(coa.CreditBalance,0) - ISNULL(coa.DebitBalance,0)
                            END AS money), 1) AS Balance,
                        coa.AccountNo + ' ' + coa.AccountName AS AccountName
                    FROM dbo.TblChartOfAccount coa
                    INNER JOIN dbo.TblAccountTypes at ON at.AccountTypesID = coa.AccountTypesID
                    WHERE coa.SystemID = " + sysID + " AND coa.WarehouseID = " + whID + @"
                    AND coa.StatusID = 40
                    AND NOT EXISTS (
                        SELECT 1 FROM dbo.TblChartOfAccount child 
                        WHERE child.ParentAccountID = coa.ChartOfAccountID
                    )
                    ORDER BY at.AccountTypes, coa.AccountNo, coa.AccountName";

                // Row 1: From Account | Large Balance
                html.HTMLControls.Add(new HtmlComboBox()
                {
                    Label = "Transfer Funds From",
                    ID = "FromAccountID",
                    DBField = "FromAccountID",
                    SQL_CBO = sql_acc,
                    Value = "ChartOfAccountID",
                    DisplayText = "AccountName",
                    Required = true,
                    IsKendoMultiColumnComboBox = true,
                    KendoComboBoxColumn = new List<KendoComboBoxColumns>
                    {
                        new KendoComboBoxColumns() { field = "AccountNo", title = "Acc No", width = "100" },
                        new KendoComboBoxColumns() { field = "AccountNameOnly", title = "Account Name", width = "250" },
                        new KendoComboBoxColumns() { field = "AccountTypes", title = "Type", width = "100" },
                        new KendoComboBoxColumns() { field = "Balance", title = "Balance", width = "100" }
                    },
                    KendoMultiColumnComboBoxHeight = 400,
                    ColSpan = 1,
                    placeholder = "Select Account..."
                });
                html.HTMLControls.Add(new HtmlTextBox()
                {
                    Label = "Balance",
                    ID = "FromBalance",
                    IsReadOnly = true,
                    Value = "$0.00",
                    ColSpan = 1,
                    Class = "form-control-plaintext text-end balance-small"
                });

                // Row 2: To Account | Balance
                html.HTMLControls.Add(new HtmlComboBox()
                {
                    Label = "Transfer Funds To",
                    ID = "ToAccountID",
                    DBField = "ToAccountID",
                    SQL_CBO = sql_acc,
                    Value = "ChartOfAccountID",
                    DisplayText = "AccountName",
                    Required = true,
                    IsKendoMultiColumnComboBox = true,
                    KendoComboBoxColumn = new List<KendoComboBoxColumns>
                    {
                        new KendoComboBoxColumns() { field = "AccountNo", title = "Acc No", width = "100" },
                        new KendoComboBoxColumns() { field = "AccountNameOnly", title = "Account Name", width = "250" },
                        new KendoComboBoxColumns() { field = "AccountTypes", title = "Type", width = "100" },
                        new KendoComboBoxColumns() { field = "Balance", title = "Balance", width = "100" }
                    },
                    KendoMultiColumnComboBoxHeight = 400,
                    ColSpan = 1,
                    placeholder = "Select Account..."
                });
                html.HTMLControls.Add(new HtmlTextBox()
                {
                    Label = "Balance",
                    ID = "ToBalance",
                    IsReadOnly = true,
                    Value = "$0.00",
                    ColSpan = 1,
                    Class = "form-control-plaintext text-end balance-small"
                });

                // Row 3: Amount | Date
                html.HTMLControls.Add(new HtmlTextBox()
                {
                    Label = "Transfer Amount",
                    ID = "TransferAmount",
                    DBField = "TransferAmount",
                    ISNumeric = true,
                    Required = true,
                    Value = "0.00",
                    ColSpan = 1
                });
                html.HTMLControls.Add(new HtmlTextBox()
                {
                    SingleDatePicker = true,
                    Label = "Date",
                    ID = "TransferDate",
                    DBField = "TransferDate",
                    Required = true,
                    ColSpan = 1,
                    MaxLength = 10,
                });

                // New QuickBooks-style fields
                html.HTMLControls.Add(new HtmlTextBox()
                {
                    Label = "Class Name",
                    ID = "ClassName",
                    Required = true,
                    Class = "form-control",
                    DBField = "ClassName",
                    isTypeahead = true,
                    SQL = "select ClassID, ClassName, PrefixFT from dbo.TblClass where SystemID = " + sysID + " and WarehouseID = " + whID + " order by ClassName",
                    HiddenField = "ClassID",
                    SQLCallBack = "declare @Prefix nvarchar(100), @Module nvarchar(50); select @Prefix = ISNULL(PrefixFT,'FT-{yy}{MM}{dd}-{0000}'), @Module = 'Fund Transfer' from dbo.TblClass where ClassID = TRY_CAST(@ClassID AS int); exec [SP_SHOW_PREFIX] @Prefix, @Module, " + sysID + ", " + whID + ", '" + html.DomainID + "';",
                    CallBackToControlID = "RefNo, PrefixRefNo",
                    UpdatePrefixModule = "Fund Transfer",
                    Prefix = "PrefixRefNo",
                    SetPrefixToDbFieldName = "RefNo",
                    ColSpan = 1
                });

                html.HTMLControls.Add(new HtmlTextBox()
                {
                    Label = "No.",
                    ID = "RefNo",
                    DBField = "RefNo",
                    Required = true,
                    IsReadOnly = true,
                    isCloned = isClone,
                    placeholder = "Enter No.",
                    ColSpan = 1,
                    CallBackToControlID = "RefNo, PrefixRefNo",
                    UpdatePrefixModule = "Fund Transfer",
                    Prefix = "PrefixRefNo",
                    SetPrefixToDbFieldName = "RefNo"
                });

                html.HTMLControls.Add(new HtmlComboBox()
                {
                    Label = "Payment Method",
                    ID = "PaymentMethodID",
                    Class = "form-control",
                    DBField = "PaymentMethodID",
                    SQL_CBO = "select PaymentMethodID, PaymentMethod from dbo.TblPaymentMethod where SystemID = " + sysID + " and WarehouseID = " + whID + " order by PaymentMethod",
                    Value = "PaymentMethodID",
                    DisplayText = "PaymentMethod",
                    Required = true,
                    placeholder = "Select Method...",
                    ColSpan = 1
                });

                html.HTMLControls.Add(new HtmlTextBox()
                {
                    Label = "Checked By",
                    ID = "CheckedBy",
                    Required = true,
                    DBField = "CheckedBy",
                    isTypeahead = true,
                    SQL = "select u.UserLoginID CheckedByUserID, u.FullName CheckedBy from dbo.TblUserLogin u inner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID where u.StatusID = 3 and ts.SystemID = " + sysID + " and ts.WarehouseID = " + whID + " order by u.FullName",
                    HiddenField = "CheckedByUserID",
                    HiddenFieldValue = "0",
                    ColSpan = 1
                });

                html.HTMLControls.Add(new HtmlTextBox()
                {
                    Label = "Verified By",
                    ID = "VerifiedBy",
                    Required = true,
                    DBField = "VerifiedBy",
                    isTypeahead = true,
                    SQL = "select u.UserLoginID VerifiedByUserID, u.FullName VerifiedBy from dbo.TblUserLogin u inner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID where u.StatusID = 3 and ts.SystemID = " + sysID + " and ts.WarehouseID = " + whID + " order by u.FullName",
                    HiddenField = "VerifiedByUserID",
                    HiddenFieldValue = "0",
                    ColSpan = 1
                });
                html.HTMLControls.Add(new HtmlTextBox()
                {
                    Label = "Approved By",
                    ID = "ApprovedBy",
                    Required = true,
                    DBField = "ApprovedBy",
                    isTypeahead = true,
                    SQL = "select u.UserLoginID ApprovedByUserID, u.FullName ApprovedBy from dbo.TblUserLogin u inner join dbo.TblSetUserAccessSystemAndWarehouse ts on ts.UserLoginID = u.UserLoginID where u.StatusID = 3 and ts.SystemID = " + sysID + " and ts.WarehouseID = " + whID + " order by u.FullName",
                    HiddenField = "ApprovedByUserID",
                    HiddenFieldValue = "0",
                    ColSpan = 1
                });

                // Row 4: Memo
                html.HTMLControls.Add(new HtmlTextarea()
                {
                    Label = "Memo",
                    ID = "Memo",
                    DBField = "Memo",
                    Required = true,
                    ColSpan = 2,

                });

                html.HTMLControls.Add(new HtmlFile()
                {
                    Label = "Attachments",
                    ID = "AttachmentFile",
                    DBField = "AttachmentFile",
                    Multiple = true,
                    DestinationFile = "Uploads/FundTransferAttachments",
                    ColSpan = 2,
                    Hide = true
                });

                html.HTMLControls.Add(new HtmlTextBox()
                {
                    Label = "FundTransferID",
                    ID = "FundTransferID",
                    Value = isClone ? "0" : PrimaryFieldValue.ToString(),
                    IsReadOnly = true,
                    Hide = true
                });
                ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View("Index");
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        //[Authorize]
        [AllowAnonymous]
        public ActionResult InvoiceTMP(int FundTransferID, string RefNo = "")
        {
            ViewBag.FundTransferID = FundTransferID;
            ViewBag.RefNo = RefNo;
            return View();
        }

        [AllowAnonymous]

        public ActionResult Invoice(int FundTransferID, string RefNo = "")
        {
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>();

            try
            {
                string sql = "SELECT SystemID, WarehouseID FROM TblFundTransfer WHERE FundTransferID = " + FundTransferID;
                if (FundTransferID == 0 && !string.IsNullOrEmpty(RefNo))
                {
                    sql = "SELECT SystemID, WarehouseID FROM TblFundTransfer WHERE RefNo = '" + RefNo.Replace("'", "''") + "'";
                }

                DataSet dsSystem = new DataSet();
                try
                {
                    dsSystem = html.LoadData(sql, false, para);
                }
                catch { }

                string systid = (dsSystem != null && dsSystem.Tables.Count > 0 && dsSystem.Tables[0].Rows.Count > 0)
                    ? dsSystem.Tables[0].Rows[0]["SystemID"].ToString()
                    : html.SystemID;

                string warehouseid = (dsSystem != null && dsSystem.Tables.Count > 0 && dsSystem.Tables[0].Rows.Count > 0)
                    ? dsSystem.Tables[0].Rows[0]["WarehouseID"].ToString()
                    : html.WarehouseID;

                string whereClause = " WHERE FundTransferID = " + FundTransferID;
                if (FundTransferID == 0 && !string.IsNullOrEmpty(RefNo))
                {
                    whereClause = " WHERE RefNo = '" + RefNo.Replace("'", "''") + "'";
                }

                // 2. Fetch Data in separate calls for maximum reliability
                string debugInfo = "";

                // Table 0: Company Info
                DataSet ds0 = new DataSet();
                try
                {
                    string sql0 = "select top 1 CompanyName, CompanyAddress, Phone, Email, FileLogo from TblCompany where SystemID = " + systid + " and WarehouseID = " + warehouseid + " order by ISNULL(IsDefault, 0) desc, CompanyID";
                    ds0 = html.LoadData(sql0, false, new Dictionary<string, string>());
                }
                catch (Exception ex) { debugInfo += "T0:" + ex.Message + "; "; }

                // Table 1: Account Info
                DataSet ds1 = new DataSet();
                try
                {
                    string sql1 = "select ISNULL(facc.AccountName,'N/A') as FromAccount, facc.AccountNo as FromAccountNo, ISNULL(tacc.AccountName,'N/A') as ToAccount, tacc.AccountNo as ToAccountNo from TblFundTransfer ft left join TblChartOfAccount facc on facc.ChartOfAccountID = ft.FromAccountID left join TblChartOfAccount tacc on tacc.ChartOfAccountID = ft.ToAccountID " + whereClause;
                    ds1 = html.LoadData(sql1, false, new Dictionary<string, string>());
                }
                catch (Exception ex) { debugInfo += "T1:" + ex.Message + "; "; }

                // Table 2: Master Info (Simplified)
                DataSet ds2 = new DataSet();
                try
                {
                    string sql2 = @"select ft.*, pm.PaymentMethod as PaymentMethodName, 
                                           ISNULL(NULLIF(ft.PostedBy, ''), (select top 1 u.FullName from TblAccount_Transaction t inner join TblUserLogin u on u.UserLoginID = t.CreateByUserID where t.InvoiceNo = CONCAT('FT-', ft.FundTransferID))) as PostedByText,
                                           (select top 1 t.TransactionID from TblAccount_Transaction t where t.InvoiceNo = CONCAT('FT-', ft.FundTransferID)) as JournalEntryNo
                                    from TblFundTransfer ft 
                                    left join TblPaymentMethod pm on pm.PaymentMethodID = ft.PaymentMethodID " + whereClause.Replace("WHERE", "WHERE ft.");
                    ds2 = html.LoadData(sql2, false, new Dictionary<string, string>());
                }
                catch (Exception ex) { debugInfo += "T2:" + ex.Message + "; "; }

                // Table 3: Details for Grid compatibility
                DataSet ds3 = new DataSet();
                try
                {
                    string sql3 = "select 1 as No, 'Fund Transfer' as Description, 1 as Qty, ISNULL(ft.TransferAmount, 0) as UnitPrice, ISNULL(ft.TransferAmount, 0) as Amount from TblFundTransfer ft " + whereClause;
                    ds3 = html.LoadData(sql3, false, new Dictionary<string, string>());
                }
                catch (Exception ex) { debugInfo += "T3:" + ex.Message + "; "; }

                if (ds2 == null || ds2.Tables.Count == 0 || ds2.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Error = "Fund transfer master record not found. ID: " + FundTransferID + ", Debug: " + debugInfo + (ds2 == null ? "DS2 NULL" : "Tables: " + ds2.Tables.Count);
                    return View();
                }

                // If the class name matches a specific company, prefer that company record over the default one
                string className = ds2.Tables[0].Rows[0].Table.Columns.Contains("ClassName")
                    ? ds2.Tables[0].Rows[0]["ClassName"]?.ToString()
                    : "";
                if (!string.IsNullOrEmpty(className))
                {
                    try
                    {
                        string safeClassName = className.Replace("'", "''");
                        string normalizedClassName = safeClassName.ToUpper()
                            .Replace(" ", "")
                            .Replace("-", "")
                            .Replace(",", "")
                            .Replace(".", "")
                            .Replace("(", "")
                            .Replace(")", "");
                        string aliasClassName = normalizedClassName;
                        if (normalizedClassName == "NSMISP" || normalizedClassName == "NSM")
                        {
                            aliasClassName = "NSMINTERNETSERVICEPROVIDER";
                        }
                        string sqlCompanyByClass = "select top 1 CompanyName, CompanyAddress, Phone, Email, FileLogo from TblCompany "
                            + "where SystemID = " + systid + " and WarehouseID = " + warehouseid
                            + " and (UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ClassName, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) = '" + normalizedClassName + "'"
                            + " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyName, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) = '" + normalizedClassName + "'"
                            + " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyNameEN, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) = '" + normalizedClassName + "'"
                            + " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyNameKH, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) = '" + normalizedClassName + "'"
                            + (aliasClassName != normalizedClassName ? " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ClassName, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) = '" + aliasClassName + "'" : "")
                            + (aliasClassName != normalizedClassName ? " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyName, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) = '" + aliasClassName + "'" : "")
                            + (aliasClassName != normalizedClassName ? " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyNameEN, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) = '" + aliasClassName + "'" : "")
                            + (aliasClassName != normalizedClassName ? " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyNameKH, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) = '" + aliasClassName + "'" : "")
                            + " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ClassName, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) like '%" + normalizedClassName + "%'"
                            + " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyName, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) like '%" + normalizedClassName + "%'"
                            + " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyNameEN, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) like '%" + normalizedClassName + "%'"
                            + " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyNameKH, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) like '%" + normalizedClassName + "%'"
                            + (aliasClassName != normalizedClassName ? " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ClassName, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) like '%" + aliasClassName + "%'" : "")
                            + (aliasClassName != normalizedClassName ? " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyName, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) like '%" + aliasClassName + "%'" : "")
                            + (aliasClassName != normalizedClassName ? " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyNameEN, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) like '%" + aliasClassName + "%'" : "")
                            + (aliasClassName != normalizedClassName ? " or UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(CompanyNameKH, ' ', ''), '-', ''), ',', ''), '.', ''), '(', ''), ')', '')) like '%" + aliasClassName + "%'" : "")
                            + " ) order by ISNULL(IsDefault, 0) desc, CompanyID";
                        DataSet dsCompanyByClass = html.LoadData(sqlCompanyByClass, false, new Dictionary<string, string>());
                        if (dsCompanyByClass != null && dsCompanyByClass.Tables.Count > 0 && dsCompanyByClass.Tables[0].Rows.Count > 0)
                        {
                            ds0 = dsCompanyByClass;
                        }
                    }
                    catch { /* ignore and keep default company record */ }
                }

                // Construct a virtual DataSet to match the expected view structure
                DataSet dsVirtual = new DataSet();

                DataTable dt0 = (ds0 != null && ds0.Tables.Count > 0) ? ds0.Tables[0].Copy() : new DataTable();
                dt0.TableName = "Table0";
                dsVirtual.Tables.Add(dt0);

                DataTable dt1 = (ds1 != null && ds1.Tables.Count > 0) ? ds1.Tables[0].Copy() : new DataTable();
                dt1.TableName = "Table1";
                dsVirtual.Tables.Add(dt1);

                DataTable dt2 = (ds2 != null && ds2.Tables.Count > 0) ? ds2.Tables[0].Copy() : new DataTable();
                dt2.TableName = "Table2";
                dsVirtual.Tables.Add(dt2);

                DataTable dt3 = (ds3 != null && ds3.Tables.Count > 0) ? ds3.Tables[0].Copy() : new DataTable();
                dt3.TableName = "Table3";
                dsVirtual.Tables.Add(dt3);

                ViewBag.Data = dsVirtual.Tables;
                DataRow drMaster = dsVirtual.Tables[2].Rows[0];
                decimal amount = 0m;
                if (drMaster.Table.Columns.Contains("TransferAmount") && drMaster["TransferAmount"] != DBNull.Value)
                {
                    amount = Convert.ToDecimal(drMaster["TransferAmount"]);
                }
                ViewBag.AmountInWords = NumberToWords(amount);
                ViewBag.TransferAmount = amount;
                ViewBag.FundTransferID = drMaster["FundTransferID"];
                ViewBag.PrintDate = DateTime.Now.ToString("dd/MM/yyyy");
                ViewBag.systemID = systid;
                ViewBag.UserFullName = html.UserFullName;

                // 3. Fetch Journal Entries (PO Flow)
                string ftRef = "FT-" + drMaster["FundTransferID"];
                string sqlJournal = "SELECT coa.AccountName, coa.AccountNo, ISNULL(je.Debit, 0) as Debit, ISNULL(je.Credit, 0) as Credit FROM TblAccount_JournalEntries je INNER JOIN TblChartOfAccount coa ON coa.ChartOfAccountID = je.AccountID WHERE je.InvoiceNo = '" + ftRef + "' AND je.SystemID = " + systid;

                DataSet dsJournal = html.LoadData(sqlJournal, false, new Dictionary<string, string>());
                ViewBag.JournalData = (dsJournal != null && dsJournal.Tables.Count > 0) ? dsJournal.Tables[0] : new DataTable();

                // 4. Fetch Signatures
                ViewBag.PreparedBySig = "";
                ViewBag.CheckedBySig = "";
                ViewBag.VerifiedBySig = "";
                ViewBag.ApprovedBySig = "";

                try
                {
                    // Extract user names from FundTransfer master record
                    string postedByName = ds2.Tables[0].Rows[0].Table.Columns.Contains("PostedBy") ? ds2.Tables[0].Rows[0]["PostedBy"]?.ToString() : "";
                    string checkedByName = ds2.Tables[0].Rows[0].Table.Columns.Contains("CheckedBy") ? ds2.Tables[0].Rows[0]["CheckedBy"]?.ToString() : "";
                    string verifiedByName = ds2.Tables[0].Rows[0].Table.Columns.Contains("VerifiedBy") ? ds2.Tables[0].Rows[0]["VerifiedBy"]?.ToString() : "";
                    string approvedByName = ds2.Tables[0].Rows[0].Table.Columns.Contains("ApprovedBy") ? ds2.Tables[0].Rows[0]["ApprovedBy"]?.ToString() : "";

                    Dictionary<string, string> sigParams = new Dictionary<string, string>
                    {
                        { "PostedByName", postedByName ?? "" },
                        { "CheckedByName", checkedByName ?? "" },
                        { "VerifiedByName", verifiedByName ?? "" },
                        { "ApprovedByName", approvedByName ?? "" }
                    };

                    string sqlSig = @"select 
    (select top 1 [Signature] from dbo.TblUserLogin where ISNULL(@PostedByName, '') != '' and (FullName = @PostedByName or [UserName] like '%' + @PostedByName + '%') and [Signature] is not null and [Signature] != '') as PreparedBySig,
    (select top 1 [Signature] from dbo.TblUserLogin where ISNULL(@CheckedByName, '') != '' and (FullName = @CheckedByName or [UserName] like '%' + @CheckedByName + '%') and [Signature] is not null and [Signature] != '') as CheckedBySig,
    (select top 1 [Signature] from dbo.TblUserLogin where ISNULL(@VerifiedByName, '') != '' and (FullName = @VerifiedByName or [UserName] like '%' + @VerifiedByName + '%') and [Signature] is not null and [Signature] != '') as VerifiedBySig,
    (select top 1 [Signature] from dbo.TblUserLogin where ISNULL(@ApprovedByName, '') != '' and (FullName = @ApprovedByName or [UserName] like '%' + @ApprovedByName + '%') and [Signature] is not null and [Signature] != '') as ApprovedBySig";

                    DataSet dsSig = html.LoadData(sqlSig, false, sigParams);
                    if (dsSig != null && dsSig.Tables.Count > 0 && dsSig.Tables[0].Rows.Count > 0)
                    {
                        string preparedSig = dsSig.Tables[0].Rows[0]["PreparedBySig"]?.ToString() ?? "";
                        string checkedSig = dsSig.Tables[0].Rows[0]["CheckedBySig"]?.ToString() ?? "";
                        string verifiedSig = dsSig.Tables[0].Rows[0]["VerifiedBySig"]?.ToString() ?? "";
                        string approvedSig = dsSig.Tables[0].Rows[0]["ApprovedBySig"]?.ToString() ?? "";

                        if (!string.IsNullOrWhiteSpace(preparedSig))
                            ViewBag.PreparedBySig = preparedSig.Split('~')[0].Trim();
                        if (!string.IsNullOrWhiteSpace(checkedSig))
                            ViewBag.CheckedBySig = checkedSig.Split('~')[0].Trim();
                        if (!string.IsNullOrWhiteSpace(verifiedSig))
                            ViewBag.VerifiedBySig = verifiedSig.Split('~')[0].Trim();
                        if (!string.IsNullOrWhiteSpace(approvedSig))
                            ViewBag.ApprovedBySig = approvedSig.Split('~')[0].Trim();
                    }
                }
                catch { /* silently ignore signature errors */ }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Exception: " + ex.Message;
                return View();
            }
        }
        private static string NumberToWords(decimal amount)
        {
            amount = Math.Round(amount, 2);

            if (amount == 0)
                return "Zero Dollars Only";

            long dollars = (long)amount;
            int cents = (int)((amount - dollars) * 100);

            string dollarWords = ConvertIntToWords(dollars);
            string result = "";

            if (dollars > 0)
            {
                result += dollarWords + (dollars == 1 ? " Dollar" : " Dollars");
            }

            if (cents > 0)
            {
                if (dollars > 0)
                    result += " and ";

                result += ConvertIntToWords(cents) + (cents == 1 ? " Cent" : " Cents");
            }

            return result + " Only";
        }
        private static string ConvertIntToWords(long number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + ConvertIntToWords(Math.Abs(number));

            string words = "";

            if (number / 1000000000 > 0)
            {
                words += ConvertIntToWords(number / 1000000000) + " Billion ";
                number %= 1000000000;
            }

            if (number / 1000000 > 0)
            {
                words += ConvertIntToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }

            if (number / 1000 > 0)
            {
                words += ConvertIntToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if (number / 100 > 0)
            {
                words += ConvertIntToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                string[] unitsMap =
                {
                    "Zero","One","Two","Three","Four","Five","Six","Seven","Eight","Nine",
                    "Ten","Eleven","Twelve","Thirteen","Fourteen","Fifteen",
                    "Sixteen","Seventeen","Eighteen","Nineteen"
                };

                string[] tensMap =
                {
                    "Zero","Ten","Twenty","Thirty","Forty",
                    "Fifty","Sixty","Seventy","Eighty","Ninety"
                };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if (number % 10 > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }

        [HttpPost]
        public async Task<JsonResult> SaveChangeFundTransfer(string FundTransferData)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            try
            {
                if (string.IsNullOrEmpty(FundTransferData))
                    return Json(new { Result = ResultType.Failed, Message = "No data received." }, JsonRequestBehavior.AllowGet);

                var dataObj = Newtonsoft.Json.Linq.JObject.Parse(FundTransferData);

                // Extract values with fallback to empty string to avoid "parameter not supplied" errors
                string ftID = dataObj["FundTransferID"]?.ToString();
                if (string.IsNullOrEmpty(ftID)) ftID = "0";

                string fromAccID = dataObj["FromAccountID"]?.ToString() ?? "";
                string toAccID = dataObj["ToAccountID"]?.ToString() ?? "";
                string amount = dataObj["TransferAmount"]?.ToString() ?? "0";
                amount = amount.Replace(",", ""); // Remove commas for numeric conversion in SQL

                // Validate numeric IDs to prevent SQL conversio errors
                int parsedFTID = 0;
                int parsedFromAccID = 0;
                int parsedToAccID = 0;

                if (!int.TryParse(ftID, out parsedFTID)) parsedFTID = 0;

                if (!int.TryParse(fromAccID, out parsedFromAccID))
                    return Json(new { Result = ResultType.Failed, Message = "Please select a valid Source Account." }, JsonRequestBehavior.AllowGet);

                if (!int.TryParse(toAccID, out parsedToAccID))
                    return Json(new { Result = ResultType.Failed, Message = "Please select a valid Destination Account." }, JsonRequestBehavior.AllowGet);

                // --- 1. BASIC VALIDATIONS ---
                if (parsedFromAccID == parsedToAccID)
                {
                    return Json(new { Result = ResultType.Failed, Message = "Source and Destination accounts cannot be the same." }, JsonRequestBehavior.AllowGet);
                }

                if (!decimal.TryParse(amount, out decimal amt_check) || amt_check <= 0)
                {
                    return Json(new { Result = ResultType.Failed, Message = "Transfer amount must be greater than zero." }, JsonRequestBehavior.AllowGet);
                }

                string transferDate = dataObj["TransferDate"]?.ToString();
                string formattedDate = "";
                if (!string.IsNullOrEmpty(transferDate))
                {
                    try
                    {
                        // Try standard system conversion first
                        formattedDate = html.ConvertStringToDate(transferDate);
                    }
                    catch
                    {
                        // If that fails, try generic parsing
                        DateTime dt;
                        formattedDate = DateTime.TryParse(transferDate, out dt) ? dt.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
                else
                {
                    formattedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                // --- 2. DUPLICATE SUBMISSION GUARD ---
                if (parsedFTID == 0) // Only for new records
                {
                    string dupCheckSql = @"
                        SELECT TOP 1 FundTransferID FROM dbo.TblFundTransfer 
                        WHERE FromAccountID = @FromAccID AND ToAccountID = @ToAccID AND TransferAmount = @Amt 
                        AND ABS(DATEDIFF(SECOND, CreateDate, GETDATE())) < 15 AND SystemID = @SysID";

                    DataSet dsDup = html.LoadData(dupCheckSql, false, new Dictionary<string, string> {
                        { "FromAccID", parsedFromAccID.ToString() },
                        { "ToAccID", parsedToAccID.ToString() },
                        { "Amt", amount },
                        { "SysID", html.SystemID }
                    });

                    if (dsDup != null && dsDup.Tables.Count > 0 && dsDup.Tables[0].Rows.Count > 0)
                    {
                        return Json(new { Result = ResultType.Failed, Message = "Duplicate transfer detected. Please wait a few seconds before saving again." }, JsonRequestBehavior.AllowGet);
                    }
                }

                string memo = dataObj["Memo"]?.ToString() ?? "";

                string classID = dataObj["ClassName"]?.ToString() ?? "0";
                string className = "";
                string prefixFT = "";
                int parsedClassID = 0;
                if (int.TryParse(classID, out parsedClassID) && parsedClassID != 0)
                {
                    DataSet dsClass = html.LoadData("SELECT ClassName, PrefixFT FROM dbo.TblClass WHERE ClassID = @CID", false, new Dictionary<string, string> { { "CID", parsedClassID.ToString() } });
                    if (dsClass != null && dsClass.Tables.Count > 0 && dsClass.Tables[0].Rows.Count > 0)
                    {
                        className = dsClass.Tables[0].Rows[0]["ClassName"].ToString();
                        prefixFT = dsClass.Tables[0].Rows[0]["PrefixFT"] != DBNull.Value ? dsClass.Tables[0].Rows[0]["PrefixFT"].ToString() : "";
                    }
                }
                else if (classID != "0" && classID != "")
                {
                    // If it's a string like 'NSM', try to lookup by ClassName instead of ClassID
                    DataSet dsClass = html.LoadData("SELECT ClassID, ClassName, PrefixFT FROM dbo.TblClass WHERE ClassName = @CName AND SystemID = @SysID", false,
                        new Dictionary<string, string> { { "CName", classID }, { "SysID", html.SystemID } });
                    if (dsClass != null && dsClass.Tables.Count > 0 && dsClass.Tables[0].Rows.Count > 0)
                    {
                        className = dsClass.Tables[0].Rows[0]["ClassName"].ToString();
                        classID = dsClass.Tables[0].Rows[0]["ClassID"].ToString();
                        prefixFT = dsClass.Tables[0].Rows[0]["PrefixFT"] != DBNull.Value ? dsClass.Tables[0].Rows[0]["PrefixFT"].ToString() : "";
                    }
                    else
                    {
                        return Json(new { Result = ResultType.Failed, Message = "Please select a valid Class Name from the list." }, JsonRequestBehavior.AllowGet);
                    }
                }

                string paymentMethodID = dataObj["PaymentMethodID"]?.ToString() ?? "0";
                string paymentMethod = "";
                int parsedPMID = 0;
                if (int.TryParse(paymentMethodID, out parsedPMID) && parsedPMID != 0)
                {
                    DataSet dsPM = html.LoadData("SELECT PaymentMethod FROM dbo.TblPaymentMethod WHERE PaymentMethodID = @PMID", false, new Dictionary<string, string> { { "PMID", parsedPMID.ToString() } });
                    if (dsPM != null && dsPM.Tables.Count > 0 && dsPM.Tables[0].Rows.Count > 0)
                        paymentMethod = dsPM.Tables[0].Rows[0]["PaymentMethod"].ToString();
                }
                else if (paymentMethodID != "0" && paymentMethodID != "")
                {
                    // If it's a string, try to lookup by PaymentMethod name
                    DataSet dsPM = html.LoadData("SELECT PaymentMethodID, PaymentMethod FROM dbo.TblPaymentMethod WHERE PaymentMethod = @PMName AND SystemID = @SysID", false,
                        new Dictionary<string, string> { { "PMName", paymentMethodID }, { "SysID", html.SystemID } });
                    if (dsPM != null && dsPM.Tables.Count > 0 && dsPM.Tables[0].Rows.Count > 0)
                    {
                        paymentMethod = dsPM.Tables[0].Rows[0]["PaymentMethod"].ToString();
                        paymentMethodID = dsPM.Tables[0].Rows[0]["PaymentMethodID"].ToString();
                    }
                    else
                    {
                        return Json(new { Result = ResultType.Failed, Message = "Please select a valid Payment Method from the list." }, JsonRequestBehavior.AllowGet);
                    }
                }

                string refNo = dataObj["RefNo"]?.ToString() ?? "";
                string checkedBy = dataObj["CheckedBy"]?.ToString() ?? "";
                string verifiedBy = dataObj["VerifiedBy"]?.ToString() ?? "";
                string postedBy = dataObj["PostedBy"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(postedBy))
                {
                    postedBy = html.UserFullName;
                }
                string approvedBy = dataObj["ApprovedBy"]?.ToString() ?? "";

                int.TryParse(dataObj["CheckedByUserID"]?.ToString(), out int parsedCheckedByUserID);
                int.TryParse(dataObj["VerifiedByUserID"]?.ToString(), out int parsedVerifiedByUserID);
                int.TryParse(dataObj["ApprovedByUserID"]?.ToString(), out int parsedApprovedByUserID);

                // ─── ACCOUNTING TRANSFER RULE VALIDATION ────────────────────────
                // Fetch both account types in one query
                string acctTypeSql = @"
                    SELECT coa.ChartOfAccountID, at.AccountTypesID, at.AccountTypes, at.NormalSide
                    FROM dbo.TblChartOfAccount coa
                    INNER JOIN dbo.TblAccountTypes at ON at.AccountTypesID = coa.AccountTypesID
                    WHERE coa.ChartOfAccountID IN (@FromAccID, @ToAccID)";
                DataSet dsAccTypes = html.LoadData(acctTypeSql, false,
                    new Dictionary<string, string> { { "FromAccID", fromAccID }, { "ToAccID", toAccID } });

                int fromTypeID = 0; string fromTypeName = "";
                int toTypeID = 0; string toTypeName = "";
                if (dsAccTypes?.Tables.Count > 0)
                {
                    foreach (System.Data.DataRow row in dsAccTypes.Tables[0].Rows)
                    {
                        int id = Convert.ToInt32(row["ChartOfAccountID"]);
                        if (id == parsedFromAccID) { fromTypeID = Convert.ToInt32(row["AccountTypesID"]); fromTypeName = row["AccountTypes"].ToString(); }
                        if (id == parsedToAccID) { toTypeID = Convert.ToInt32(row["AccountTypesID"]); toTypeName = row["AccountTypes"].ToString(); }
                    }
                }

                // Valid FROM → TO pairs (real-world accounting rules)
                // Asset=1, Equity=2, Expense=3, Liability=4, Revenue=5
                var validPairs = new HashSet<(int, int)>
                {
                    (1, 1), // Asset → Asset         (Bank to Bank, Cash to AR)
                    (1, 4), // Asset → Liability      (Pay supplier / debt)
                    (1, 2), // Asset → Equity         (Owner withdrawal)
                    (1, 3), // Asset → Expense        (Pay rent, salary)
                    (1, 5), // Asset → Revenue        (Receive income → deposit)
                    (4, 1), // Liability → Asset      (Take loan → bank)
                    (2, 1), // Equity → Asset         (Owner invests)
                    (5, 1), // Revenue → Asset        (Transfer income to bank)
                    (3, 4), // Expense → Liability    (Record unpaid expense → Payable)
                    (4, 4), // Liability → Liability  (Refinancing / reclassify debt)
                };

                if (fromTypeID > 0 && toTypeID > 0 && !validPairs.Contains((fromTypeID, toTypeID)))
                {
                    return Json(new
                    {
                        Result = ResultType.Failed,
                        Message = $"Invalid transfer: [{fromTypeName}] → [{toTypeName}] is not allowed in accounting. " +
                                  "Common valid transfers: Asset→Asset, Asset→Expense, Asset→Liability, Liability→Asset."
                    }, JsonRequestBehavior.AllowGet);
                }

                // ─── C# VALIDATIONS ──────────────────────────────────────────────
                string checkSql = @"
                SELECT 
                    coa.ChartOfAccountID,
                    coa.AccountName,
                    CASE 
                        WHEN ISNULL(coa.NormalSide, at.NormalSide) IN ('Debit', 'DR')
                            THEN ISNULL(coa.DebitBalance, 0) - ISNULL(coa.CreditBalance, 0)
                        ELSE 
                            ISNULL(coa.CreditBalance, 0) - ISNULL(coa.DebitBalance, 0)
                    END AS Balance,
                    (SELECT MAX(OpeningBalanceDate) FROM TblAccount_OpeningBalance WHERE ChartOfAccountID = coa.ChartOfAccountID) as OBDate
                FROM dbo.TblChartOfAccount coa
                INNER JOIN dbo.TblAccountTypes at ON at.AccountTypesID = coa.AccountTypesID
                WHERE coa.ChartOfAccountID IN (@FromAccID, @ToAccID)";

                Dictionary<string, string> checkPara = new Dictionary<string, string> { { "FromAccID", fromAccID }, { "ToAccID", toAccID } };
                DataSet dsCheck = html.LoadData(checkSql, false, checkPara);

                decimal currentBalance = 0;

                if (dsCheck != null && dsCheck.Tables.Count > 0)
                {
                    decimal amt_val = decimal.Parse(amount);
                    DateTime transDate_val = DateTime.Parse(formattedDate);

                    foreach (DataRow row in dsCheck.Tables[0].Rows)
                    {
                        int rowAccID = Convert.ToInt32(row["ChartOfAccountID"]);

                        // Check Opening Balance Date for each account
                        if (row["OBDate"] != DBNull.Value)
                        {
                            DateTime obDate = Convert.ToDateTime(row["OBDate"]);
                            if (transDate_val < obDate)
                            {
                                string accLabel = (rowAccID == parsedFromAccID) ? "Source" : "Destination";
                                return Json(new { Result = ResultType.Failed, Message = $"Transaction date cannot be before the {accLabel} account's opening balance date ({obDate:dd/MM/yyyy}). Account: {row["AccountName"]}" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        // Store Source Balance for next check
                        if (rowAccID == parsedFromAccID)
                        {
                            currentBalance = row["Balance"] != DBNull.Value ? Convert.ToDecimal(row["Balance"]) : 0;
                        }
                    }
                }

                decimal amt = decimal.Parse(amount);
                DateTime transDate = DateTime.Parse(formattedDate);

                // If update, add old amount to effective balance
                if (ftID != "0")
                {
                    string getOldSql = "SELECT FromAccountID, TransferAmount FROM dbo.TblFundTransfer WHERE FundTransferID = @FTID";
                    DataSet dsOld = html.LoadData(getOldSql, false, new Dictionary<string, string> { { "FTID", ftID } });
                    if (dsOld != null && dsOld.Tables.Count > 0 && dsOld.Tables[0].Rows.Count > 0)
                    {
                        string oldFromAccID = dsOld.Tables[0].Rows[0]["FromAccountID"].ToString();
                        decimal oldAmount = Convert.ToDecimal(dsOld.Tables[0].Rows[0]["TransferAmount"]);
                        if (fromAccID == oldFromAccID)
                        {
                            currentBalance += oldAmount;
                        }
                    }
                }

                // Balance validation
                if (currentBalance < amt)
                {
                    return Json(new { Result = ResultType.Failed, Message = "Insufficient funds in the source account. Available Balance: $" + currentBalance.ToString("N2") }, JsonRequestBehavior.AllowGet);
                }

                // ─── HANDLE FILE UPLOAD (optional) ───────────────────────────────────
                string attachmentFileName = "";
                SqlResult uploadResult = html.UploadFile("Uploads/FundTransferAttachments", "AttachmentFile");
                // Attachment is optional — only update path if a new file was actually uploaded
                if (uploadResult.Result == ResultType.Successful && !string.IsNullOrEmpty(uploadResult.Value))
                {
                    attachmentFileName = "Uploads/FundTransferAttachments/" + uploadResult.Value;
                }

                // Build inline SQL — SaveChangeData forces StoredProcedure mode when Para != null,
                // so we inline the already-validated C# values and pass null for Para.
                string sql = @"
              
                DECLARE @OldFromAccountID int, @OldToAccountID int, @OldAmount decimal(18,2), @TransactionID int, @FTReference nvarchar(50);
                
                DECLARE @FTID int = " + parsedFTID + @";
                DECLARE @FromAccID int = " + parsedFromAccID + @";
                DECLARE @ToAccID int = " + parsedToAccID + @";
                DECLARE @Amt decimal(18,2) = " + amt.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + @";
                DECLARE @TransDate datetime = '" + formattedDate + @"';
                DECLARE @SysID int = " + html.SystemID + @";
                DECLARE @WhID int = " + html.WarehouseID + @";
                DECLARE @UID int = " + html.UserID + @";
                DECLARE @Memo nvarchar(max) = N'" + memo.Replace("'", "''") + @"';
                DECLARE @AttachmentFile nvarchar(500) = N'" + attachmentFileName.Replace("'", "''") + @"';
                DECLARE @ClassIDVal int = " + (parsedClassID != 0 ? parsedClassID.ToString() : "0") + @";
                DECLARE @ClassNameVal nvarchar(250) = N'" + className.Replace("'", "''") + @"';
                DECLARE @PMIDVal int = " + (parsedPMID != 0 ? parsedPMID.ToString() : "0") + @";
                DECLARE @PMVal nvarchar(250) = N'" + paymentMethod.Replace("'", "''") + @"';
                DECLARE @RefNoVal nvarchar(250) = N'" + refNo.Replace("'", "''") + @"';
                DECLARE @CheckedByVal nvarchar(250) = N'" + checkedBy.Replace("'", "''") + @"';
                DECLARE @VerifiedByVal nvarchar(250) = N'" + verifiedBy.Replace("'", "''") + @"';
                DECLARE @PostedByVal nvarchar(250) = N'" + postedBy.Replace("'", "''") + @"';
                DECLARE @ApprovedByVal nvarchar(250) = N'" + approvedBy.Replace("'", "''") + @"';
                DECLARE @CheckedByUserID int = " + (parsedCheckedByUserID != 0 ? parsedCheckedByUserID.ToString() : "0") + @";
                DECLARE @VerifiedByUserID int = " + (parsedVerifiedByUserID != 0 ? parsedVerifiedByUserID.ToString() : "0") + @";
                DECLARE @ApprovedByUserID int = " + (parsedApprovedByUserID != 0 ? parsedApprovedByUserID.ToString() : "0") + @";
                DECLARE @CurrentPrefix nvarchar(100) = N'" + prefixFT.Replace("'", "''") + @"';

                -- 0. Get old record details if updating
                IF @FTID <> 0
                BEGIN
                    SELECT @OldFromAccountID = FromAccountID, @OldToAccountID = ToAccountID, @OldAmount = TransferAmount 
                    FROM dbo.TblFundTransfer WHERE FundTransferID = @FTID;
                END

-- 2. IF UPDATE: Reverse old account balances (Updating only Debit/Credit)
IF @FTID <> 0
BEGIN
    SET @FTReference = CONCAT('FT-', @FTID);

    -- Reverse old FROM account credit entry
    UPDATE dbo.TblChartOfAccount SET CreditBalance = ISNULL(CreditBalance, 0) - @OldAmount WHERE ChartOfAccountID = @OldFromAccountID;
    -- Reverse old TO account debit entry
    UPDATE dbo.TblChartOfAccount SET DebitBalance = ISNULL(DebitBalance, 0) - @OldAmount WHERE ChartOfAccountID = @OldToAccountID;
    
    DELETE FROM dbo.TblAccount_JournalEntries WHERE InvoiceNo = @FTReference;
    DELETE FROM dbo.TblAccount_Transaction WHERE InvoiceNo = @FTReference;
END

-- 2. Save/Update the Fund Transfer Record
IF @FTID = 0
BEGIN
    INSERT INTO dbo.TblFundTransfer (FromAccountID, ToAccountID, TransferAmount, TransferDate, Memo, AttachmentFile, SystemID, WarehouseID, ClassID, ClassName, PaymentMethodID, PaymentMethod, RefNo, CheckedBy, VerifiedBy, PostedBy, ApprovedBy, CheckedByUserID, VerifiedByUserID, ApprovedByUserID, StatusID)
    VALUES (@FromAccID, @ToAccID, @Amt, @TransDate, @Memo, @AttachmentFile, @SysID, @WhID, @ClassIDVal, @ClassNameVal, @PMIDVal, @PMVal, @RefNoVal, @CheckedByVal, @VerifiedByVal, @PostedByVal, @ApprovedByVal, @CheckedByUserID, @VerifiedByUserID, @ApprovedByUserID, 60);
    SET @FTID = SCOPE_IDENTITY();

    IF EXISTS (SELECT 1 FROM dbo.TblPrefix WHERE Module = 'Fund Transfer' AND Prefix = @CurrentPrefix AND SystemID = @SysID AND WarehouseID = @WhID)
    BEGIN
        UPDATE dbo.TblPrefix 
        SET CurrentNumber = ISNULL(CurrentNumber, 0) + 1 
        WHERE Module = 'Fund Transfer' AND Prefix = @CurrentPrefix AND SystemID = @SysID AND WarehouseID = @WhID;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.TblPrefix (StartNumber, CurrentNumber, Prefix, CurrentPrefix, Module, SystemID, WarehouseID, DomainID)
        VALUES (1, 1, @CurrentPrefix, @RefNoVal, 'Fund Transfer', @SysID, @WhID, (SELECT TOP 1 DomainID FROM dbo.TblSystem WHERE SystemID = @SysID));
    END
END
ELSE
BEGIN
    UPDATE dbo.TblFundTransfer SET 
        FromAccountID = @FromAccID, 
        ToAccountID = @ToAccID, 
        TransferAmount = @Amt, 
        TransferDate = @TransDate, 
        Memo = @Memo,
        AttachmentFile = (CASE WHEN @AttachmentFile <> '' THEN @AttachmentFile ELSE AttachmentFile END),
        ClassID = @ClassIDVal,
        ClassName = @ClassNameVal,
        PaymentMethodID = @PMIDVal,
        PaymentMethod = @PMVal,
        RefNo = @RefNoVal,
        CheckedBy = @CheckedByVal,
        VerifiedBy = @VerifiedByVal,
        PostedBy = @PostedByVal,
        ApprovedBy = @ApprovedByVal,
        CheckedByUserID = @CheckedByUserID,
        VerifiedByUserID = @VerifiedByUserID,
        ApprovedByUserID = @ApprovedByUserID,
        -- Reset approval status so it re-enters the workflow
        StatusID = 60,
        CheckedDate = NULL,
        VerifiedDate = NULL,
        ApprovalDate = NULL,
        DisapproveDate = NULL,
        DisapproveBy = NULL
    WHERE FundTransferID = @FTID;
END

SET @FTReference = CONCAT('FT-', @FTID);

-- 3. Apply New Balances using proper double-entry (only Debit/Credit, NOT TotalAmount)
-- FROM account: always CREDIT (reduces debit-normal like Asset; increases credit-normal like Liability)
UPDATE dbo.TblChartOfAccount 
SET CreditBalance = ISNULL(CreditBalance, 0) + @Amt
WHERE ChartOfAccountID = @FromAccID;

-- TO account: always DEBIT (increases debit-normal like Asset; reduces credit-normal like Liability)
UPDATE dbo.TblChartOfAccount 
SET DebitBalance = ISNULL(DebitBalance, 0) + @Amt
WHERE ChartOfAccountID = @ToAccID;


                    -- 4. Insert Accounting Transaction Logs
                    INSERT INTO dbo.TblAccount_Transaction (InvoiceNo, Type, Amount, TransactionDate, Description, FromAccountID, ToAccountID, SystemID, WarehouseID, CreateByUserID, CreateDate)
                    VALUES (@FTReference, 'Fund Transfer', @Amt, @TransDate, @Memo, @FromAccID, @ToAccID, @SysID, @WhID, @UID, GETDATE());
                    SET @TransactionID = SCOPE_IDENTITY();

                    -- 5. Journal Entries (Double Entry)
                    -- Credit the Source Account (Decrease Asset)
                    INSERT INTO dbo.TblAccount_JournalEntries (TransactionID, InvoiceNo, AccountID, Debit, Credit, TransactionDate, SystemID, WarehouseID, CreateByUserID, CreateDate, Description)
                    VALUES (@TransactionID, @FTReference, @FromAccID, 0, @Amt, @TransDate, @SysID, @WhID, @UID, GETDATE(), CONCAT('Transfer Out: ', @Memo));

                    -- Debit the Destination Account (Increase Asset)
                    INSERT INTO dbo.TblAccount_JournalEntries (TransactionID, InvoiceNo, AccountID, Debit, Credit, TransactionDate, SystemID, WarehouseID, CreateByUserID, CreateDate, Description)
                    VALUES (@TransactionID, @FTReference, @ToAccID, @Amt, 0, @TransDate, @SysID, @WhID, @UID, GETDATE(), CONCAT('Transfer In: ', @Memo));

                    SELECT 2 as Result, 'Transfer saved and balances updated successfully' as Message;
                ";

                result = html.SaveChangeData(sql, false, null);

                if (result.Result == ResultType.Successful)
                {
                    try
                    {
                        int ftId = parsedFTID;

                        if (ftId == 0)
                        {
                            string getFTIdSql = @"SELECT TOP 1 FundTransferID FROM dbo.TblFundTransfer 
                                                 WHERE FromAccountID = @FromAccountID AND ToAccountID = @ToAccountID 
                                                 AND TransferAmount = @TransferAmount 
                                                 ORDER BY FundTransferID DESC";
                            Dictionary<string, string> getFTParams = new Dictionary<string, string>
                            {
                                { "FromAccountID", parsedFromAccID.ToString() },
                                { "ToAccountID", parsedToAccID.ToString() },
                                { "TransferAmount", amount }
                            };
                            DataSet ftIdDs = html.LoadData(getFTIdSql, false, getFTParams);
                            if (ftIdDs != null && ftIdDs.Tables.Count > 0 && ftIdDs.Tables[0].Rows.Count > 0)
                            {
                                ftId = Convert.ToInt32(ftIdDs.Tables[0].Rows[0]["FundTransferID"]);
                            }
                        }

                        // For edit, use parsedFTID directly
                        if (ftId == 0 && parsedFTID > 0)
                            ftId = parsedFTID;

                        // Send Telegram notification directly from C# (reliable for both new & edit)
                        if (ftId > 0)
                        {
                            try
                            {
                                var teleResult = await SendNotificationToTelegram(ftId, parsedFTID > 0);
                                if (teleResult != null && teleResult.Data != null && teleResult.Data.ToString().Contains("Failed"))
                                {
                                    result.Message += " (Note: Telegram notification failed)";
                                }
                            }
                            catch { result.Message += " (Note: Telegram notification error)"; }
                        }

                        // Return the ID in Data field for the callback function
                        return Json(new { Result = result.Result, Message = result.Message, Data = Newtonsoft.Json.JsonConvert.SerializeObject(new { FundTransferID = ftId }) }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        // Fallback
                        // show error message in toast
                        return Json(new { Result = ResultType.Failed, Message = ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Result = ResultType.Failed;
                result.Message = ex.Message;
            }

            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetAccountBalance(string AccountID)
        {
            HTML html = new HTML();
            // Real net balance formula based on NormalSide:
            //   Debit-normal  (Asset, Expense)           → DebitBalance - CreditBalance
            //   Credit-normal (Liability, Equity, Revenue) → CreditBalance - DebitBalance
            string sql = @"
                SELECT 
                    CASE 
                        WHEN ISNULL(coa.NormalSide, at.NormalSide) IN ('Debit', 'DR')
                            THEN ISNULL(coa.DebitBalance, 0) - ISNULL(coa.CreditBalance, 0)
                        ELSE 
                            ISNULL(coa.CreditBalance, 0) - ISNULL(coa.DebitBalance, 0)
                    END AS Balance,
                    ISNULL(coa.NormalSide, at.NormalSide) AS NormalSide,
                    ISNULL(coa.DebitBalance, 0)  AS DebitBalance,
                    ISNULL(coa.CreditBalance, 0) AS CreditBalance
                FROM dbo.TblChartOfAccount coa
                INNER JOIN dbo.TblAccountTypes at ON at.AccountTypesID = coa.AccountTypesID
                WHERE coa.ChartOfAccountID = @AccountID";

            Dictionary<string, string> Para = new Dictionary<string, string> { { "AccountID", AccountID ?? "" } };
            DataSet ds = html.LoadData(sql, false, Para);

            decimal balance = 0;
            decimal debitBalance = 0;
            decimal creditBalance = 0;
            string normalSide = "";
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                balance = Convert.ToDecimal(ds.Tables[0].Rows[0]["Balance"]);
                debitBalance = Convert.ToDecimal(ds.Tables[0].Rows[0]["DebitBalance"]);
                creditBalance = Convert.ToDecimal(ds.Tables[0].Rows[0]["CreditBalance"]);
                normalSide = ds.Tables[0].Rows[0]["NormalSide"]?.ToString() ?? "";
            }

            return Json(new
            {
                Balance = balance.ToString("N2"),
                DebitBalance = debitBalance.ToString("N2"),
                CreditBalance = creditBalance.ToString("N2"),
                NormalSide = normalSide
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Delete Fund Transfer")]
        [HttpPost]
        public JsonResult DeleteFundTransfer(int ID)
        {
            HTML html = new HTML();
            string sql = @"
                DECLARE @OldFromAccountID int, @OldToAccountID int, @OldAmount decimal(18,2), @FTReference nvarchar(50);
                SET @FTReference = CONCAT('FT-', @ID);

                -- 1. Get old record details
                SELECT @OldFromAccountID = FromAccountID, @OldToAccountID = ToAccountID, @OldAmount = TransferAmount 
                FROM dbo.TblFundTransfer WHERE FundTransferID = @ID;

                IF @OldFromAccountID IS NOT NULL
                BEGIN
                    -- 2. Reverse Balances (only Debit/Credit, no TotalAmount)
                        UPDATE dbo.TblChartOfAccount SET CreditBalance = ISNULL(CreditBalance, 0) - @OldAmount WHERE ChartOfAccountID = @OldFromAccountID;
                        UPDATE dbo.TblChartOfAccount SET DebitBalance  = ISNULL(DebitBalance,  0) - @OldAmount WHERE ChartOfAccountID = @OldToAccountID;
                    
                    -- 3. Delete Accounting Logs
                    DELETE FROM dbo.TblAccount_JournalEntries WHERE InvoiceNo = @FTReference;
                    DELETE FROM dbo.TblAccount_Transaction WHERE InvoiceNo = @FTReference;
                    
                    -- 4. Delete the Record
                    DELETE FROM dbo.TblFundTransfer WHERE FundTransferID = @ID;
                END
                SELECT 1 as Result, 'Transfer deleted successfully' as Message;";

            Dictionary<string, string> Para = new Dictionary<string, string> { { "ID", ID.ToString() } };
            SqlResult result = html.SaveChangeData(sql, false, Para);
            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult GetFundTransferStatus(int FundTransferID)
        {
            HTML html = new HTML();
            string sql = "SELECT StatusID FROM dbo.TblFundTransfer WHERE FundTransferID = @FTID";
            Dictionary<string, string> para = new Dictionary<string, string> { { "FTID", FundTransferID.ToString() } };
            DataSet ds = html.LoadData(sql, false, para);
            int statusID = 0;
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                int.TryParse(ds.Tables[0].Rows[0]["StatusID"]?.ToString(), out statusID);
            }
            return Json(new { StatusID = statusID }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDomains()
        {
            HTML html = new HTML();
            string sql = "SELECT DISTINCT DomainID FROM dbo.TblPrefix UNION SELECT DISTINCT DomainID FROM dbo.TblSystem";
            DataSet ds = html.LoadData(sql, false);
            var domains = new List<string>();
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    domains.Add(row["DomainID"].ToString());
                }
            }
            return Json(domains, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ApproveFundTransfer(int FundTransferID, int StatusID, string Comment = "", long TelegramGroupID = 0, string FirstName = "", string LastName = "", string UserName = "", string UserFullName = "", string NeedToCheckUserID = "", string ChatID = "", string MessageID = "", bool IsDisFromSystem = false)
        {
            HTML html = new HTML();
            if (StatusID == 0) StatusID = 90;

            Dictionary<string, string> para = new Dictionary<string, string>
    {
        { "FTID", FundTransferID.ToString() },
        { "StatusID", StatusID.ToString() }
    };

            int actingUserID;
            if (!string.IsNullOrEmpty(NeedToCheckUserID) && NeedToCheckUserID != "0")
                int.TryParse(NeedToCheckUserID, out actingUserID);
            else
                int.TryParse(html.UserID, out actingUserID); // ← web UI session user

            // Determine which column holds the authorized user for this workflow step
            string authColumn = StatusID == 92 ? "CheckedByUserID"
                              : StatusID == 102 ? "VerifiedByUserID"
                              : (StatusID == 90 || StatusID == 91) ? "ApprovedByUserID"
                              : null;

            // ── Run authorization check for ALL flows (not just Telegram) ─────────
            if (!IsDisFromSystem && authColumn != null && actingUserID > 0)
            {
                string validationSql = $@"
            SELECT {authColumn},
                   {authColumn.Replace("UserID", "")} AS AuthName
            FROM dbo.TblFundTransfer
            WHERE FundTransferID = @FTID";

                DataSet validationDs = html.LoadData(validationSql, false,
                    new Dictionary<string, string> { { "FTID", FundTransferID.ToString() } });

                if (validationDs != null && validationDs.Tables.Count > 0 && validationDs.Tables[0].Rows.Count > 0)
                {
                    string authNameText = validationDs.Tables[0].Rows[0]["AuthName"]?.ToString() ?? "";
                    int authorizedUserID;
                    int.TryParse(validationDs.Tables[0].Rows[0][authColumn]?.ToString(), out authorizedUserID);

                    // Fallback: look up by name if ID is missing
                    if (authorizedUserID == 0 && !string.IsNullOrEmpty(authNameText))
                    {
                        DataSet findUserDs = html.LoadData(
                            "SELECT UserLoginID FROM dbo.TblUserLogin WHERE FullName = @FullName",
                            false,
                            new Dictionary<string, string> { { "FullName", authNameText } });

                        if (findUserDs?.Tables.Count > 0 && findUserDs.Tables[0].Rows.Count > 0)
                            int.TryParse(findUserDs.Tables[0].Rows[0]["UserLoginID"]?.ToString(), out authorizedUserID);
                    }
                    // No authorized user configured at all — block
                    if (authorizedUserID == 0)
                    {
                        return Json(new
                        {
                            Result = ResultType.Failed,
                            Message = "Unauthorized. No authorized user is configured for this step. Please re-create the fund transfer with the correct approvers."
                        }, JsonRequestBehavior.AllowGet);
                    }

                    // Wrong user attempting the action — block with a clear message
                    if (actingUserID != authorizedUserID)
                    {
                        // Fetch the authorized user's display name for the error message
                        string authorizedName = authNameText;
                        DataSet authNameDs = html.LoadData(
                            "SELECT FullName FROM dbo.TblUserLogin WHERE UserLoginID = @UID",
                            false,
                            new Dictionary<string, string> { { "UID", authorizedUserID.ToString() } });

                        if (authNameDs?.Tables.Count > 0 && authNameDs.Tables[0].Rows.Count > 0)
                            authorizedName = authNameDs.Tables[0].Rows[0]["FullName"].ToString();

                        string actionLabel = StatusID == 92 ? "Check"
                                           : StatusID == 102 ? "Verify"
                                           : StatusID == 90 ? "Approve"
                                           : StatusID == 91 ? "Disapprove"
                                           : "perform this action";

                        string msgErr = $"You are not authorized. Only '{authorizedName}' is allowed to {actionLabel} this Fund Transfer.";

                        // Telegram Alert flow
                        if (!string.IsNullOrEmpty(ChatID) && !string.IsNullOrEmpty(MessageID))
                        {
                            string botSql = "SELECT BotToken, SystemID FROM dbo.TblFundTransfer ft JOIN dbo.TblTelegramBots b ON b.SystemID = ft.SystemID WHERE ft.FundTransferID = @FTID";
                            DataSet botDs = html.LoadData(botSql, false, new Dictionary<string, string> { { "FTID", FundTransferID.ToString() } });
                            string botToken = (botDs != null && botDs.Tables.Count > 0 && botDs.Tables[0].Rows.Count > 0)
                                              ? botDs.Tables[0].Rows[0]["BotToken"]?.ToString()
                                              : "";

                            _ = SendTelegramAuthAlert(botToken, long.Parse(ChatID), int.Parse(MessageID), $"@{UserName} attempted an unauthorized action.\n\nOnly <b>{authorizedName}</b> is authorized.");
                        }

                        return Json(new
                        {
                            Result = ResultType.Failed,
                            Message = $"Unauthorized. Only '{authorizedName}' is allowed to {actionLabel} this Fund Transfer."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            string sql;
            // ─────────────────────────────────────────────────────────────────────

            if (StatusID == 92) // Check
            {
                sql = @"UPDATE dbo.TblFundTransfer 
                        SET StatusID = @StatusID,
                            CheckedBy = @CheckedBy,
                            CheckedDate = GETDATE()
                        WHERE FundTransferID = @FTID;
                        SELECT 1 as Result, 'Fund Transfer Checked' as Message;";
                para.Add("CheckedBy", !string.IsNullOrEmpty(UserFullName) ? UserFullName : html.UserFullName);
            }
            else if (StatusID == 102) // Verify
            {
                sql = @"UPDATE dbo.TblFundTransfer 
                        SET StatusID = @StatusID,
                            VerifiedBy = @VerifiedBy,
                            VerifiedDate = GETDATE()
                        WHERE FundTransferID = @FTID;
                        SELECT 1 as Result, 'Fund Transfer Verified' as Message;";
                para.Add("VerifiedBy", !string.IsNullOrEmpty(UserFullName) ? UserFullName : html.UserFullName);
            }
            else if (StatusID == 90) // Approve
            {
                sql = @"UPDATE dbo.TblFundTransfer 
                        SET StatusID = @StatusID,
                            ApprovedBy = @ApprovedBy,
                            ApprovalDate = GETDATE()
                        WHERE FundTransferID = @FTID;
                        SELECT 1 as Result, 'Fund Transfer Approved' as Message;";
                para.Add("ApprovedBy", !string.IsNullOrEmpty(UserFullName) ? UserFullName : html.UserFullName);
            }
            else if (StatusID == 91) // Disapprove
            {
                sql = @"UPDATE dbo.TblFundTransfer 
                        SET StatusID = @StatusID,
                            DisapproveBy = @DisapproveBy,
                            DisapproveDate = GETDATE()
                        WHERE FundTransferID = @FTID;
                        SELECT 1 as Result, 'Fund Transfer Disapproved' as Message;";
                para.Add("DisapproveBy", !string.IsNullOrEmpty(UserFullName) ? UserFullName : html.UserFullName);
            }
            else
            {
                sql = @"UPDATE dbo.TblFundTransfer 
                        SET StatusID = @StatusID
                        WHERE FundTransferID = @FTID;
                        SELECT 1 as Result, 'Fund Transfer Updated' as Message;";
            }

            SqlResult result = html.SaveChangeData(sql, false, para);

            if (result.Result == ResultType.Successful)
            {
                result.Message = StatusID == 92
                    ? "Fund Transfer Checked"
                    : StatusID == 102
                    ? "Fund Transfer Verified"
                    : StatusID == 90 ? "Fund Transfer Approved" : StatusID == 91 ? "Fund Transfer Disapproved" : "Fund Transfer Updated";
            }

            if (result.Result == ResultType.Successful && !string.IsNullOrEmpty(ChatID) && !string.IsNullOrEmpty(MessageID))
            {
                try
                {
                    string sysSql = "SELECT SystemID FROM dbo.TblFundTransfer WHERE FundTransferID = @FTID";
                    Dictionary<string, string> sysParams = new Dictionary<string, string> { { "FTID", FundTransferID.ToString() } };
                    DataSet sysDs = html.LoadData(sysSql, false, sysParams);
                    string sysId = "0";
                    if (sysDs != null && sysDs.Tables.Count > 0 && sysDs.Tables[0].Rows.Count > 0)
                    {
                        sysId = sysDs.Tables[0].Rows[0]["SystemID"]?.ToString() ?? "0";
                    }

                    string botSql = "SELECT BotToken FROM dbo.TblTelegramBots WHERE SystemID = @SystemID";
                    Dictionary<string, string> botParams = new Dictionary<string, string> { { "SystemID", sysId } };
                    DataSet botDs = html.LoadData(botSql, false, botParams);
                    string botToken = "";
                    if (botDs != null && botDs.Tables.Count > 0 && botDs.Tables[0].Rows.Count > 0)
                    {
                        botToken = botDs.Tables[0].Rows[0]["BotToken"]?.ToString() ?? "";
                    }

                    if (string.IsNullOrEmpty(botToken))
                        botToken = ""; // Rely on service defaults if empty

                    var botClient = new Telegram.Bot.TelegramBotClient(botToken);
                    long chatId = long.Parse(ChatID);
                    int messageId = int.Parse(MessageID);

                    string ftSql = @"SELECT ft.FundTransferID, ft.RefNo, ft.TransferAmount, ft.TransferDate, ft.Memo, ft.AttachmentFile, ft.ClassName, ft.PaymentMethod,
                                             (coa1.AccountName + ISNULL(' (' + coa1.AccountNo + ')', '')) as FromAccount, 
                                             (coa2.AccountName + ISNULL(' (' + coa2.AccountNo + ')', '')) as ToAccount,
                                             ISNULL(u_checked.TelegramUserName, u_checked_fb.TelegramUserName) as CheckedByTelegramUserName,
                                             ISNULL(u_verified.TelegramUserName, u_verified_fb.TelegramUserName) as VerifiedByTelegramUserName,
                                             ISNULL(u_approved.TelegramUserName, u_approved_fb.TelegramUserName) as ApprovedByTelegramUserName
                                      FROM dbo.TblFundTransfer ft
                                      LEFT JOIN dbo.TblChartOfAccount coa1 ON coa1.ChartOfAccountID = ft.FromAccountID
                                      LEFT JOIN dbo.TblChartOfAccount coa2 ON coa2.ChartOfAccountID = ft.ToAccountID
                                      LEFT JOIN dbo.TblUserLogin u_checked ON u_checked.UserLoginID = ft.CheckedByUserID
                                      LEFT JOIN dbo.TblUserLogin u_checked_fb ON u_checked_fb.FullName = ft.CheckedBy
                                      LEFT JOIN dbo.TblUserLogin u_verified ON u_verified.UserLoginID = ft.VerifiedByUserID
                                      LEFT JOIN dbo.TblUserLogin u_verified_fb ON u_verified_fb.FullName = ft.VerifiedBy
                                      LEFT JOIN dbo.TblUserLogin u_approved ON u_approved.UserLoginID = ft.ApprovedByUserID
                                      LEFT JOIN dbo.TblUserLogin u_approved_fb ON u_approved_fb.FullName = ft.ApprovedBy
                                      WHERE ft.FundTransferID = @FTID";

                    Dictionary<string, string> ftParams = new Dictionary<string, string> { { "FTID", FundTransferID.ToString() } };
                    DataSet ftDs = html.LoadData(ftSql, false, ftParams);

                    string msg = "";

                    if (ftDs != null && ftDs.Tables.Count > 0 && ftDs.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = ftDs.Tables[0].Rows[0];
                        decimal amount = Convert.ToDecimal(dr["TransferAmount"]);
                        string fromAccount = dr["FromAccount"].ToString();
                        string toAccount = dr["ToAccount"].ToString();
                        string memo = dr["Memo"].ToString();
                        string dateStr = Convert.ToDateTime(dr["TransferDate"]).ToString("dd-MMM-yyyy");
                        string refNo = dr["RefNo"]?.ToString() ?? FundTransferID.ToString();

                        msg = $@"<b>💸 FUND TRANSFER REQUEST #{refNo}</b>

<b>Source:</b> {fromAccount}
<b>Destination:</b> {toAccount}
<b>Amount:</b> ${amount:N2}
<b>Date:</b> {dateStr}
<b>Class:</b> {dr["ClassName"]}
<b>Method:</b> {dr["PaymentMethod"]}
<b>Memo:</b> {memo}
{(string.IsNullOrEmpty(dr["AttachmentFile"]?.ToString()) ? "" : "<b>Attachment:</b> Provided")}";
                        msg += $@"

────────────────────────
Created By: {(string.IsNullOrEmpty(UserFullName) ? html.UserFullName : UserFullName)}";
                    }
                    else
                    {
                        msg = $@"<b>💸 FUND TRANSFER REQUEST #{FundTransferID}</b>";
                    }
                    string refNoFallback = FundTransferID.ToString();
                    if (ftDs != null && ftDs.Tables.Count > 0 && ftDs.Tables[0].Rows.Count > 0)
                    {
                        refNoFallback = ftDs.Tables[0].Rows[0]["RefNo"]?.ToString() ?? FundTransferID.ToString();
                    }

                    string statusText = "";
                    string nextInstruction = "";
                    if (StatusID == 92)
                    {
                        statusText = "Checked ✍️";
                        nextInstruction = "Please check attachment file for detail and VERIFY or DISAPPROVE.";
                    }
                    else if (StatusID == 102)
                    {
                        statusText = "Verified 🔍";
                        nextInstruction = "Please check attachment file for detail and APPROVE or DISAPPROVE.";
                    }
                    else if (StatusID == 90)
                    {
                        statusText = "Approved ✅";
                    }
                    else if (StatusID == 91)
                    {
                        statusText = "Disapproved ❌";
                    }

                    string creator = "Team";
                    if (msg.Contains("Created By: "))
                    {
                        int startIndex = msg.IndexOf("Created By: ") + 12;
                        int endIndex = msg.IndexOf("\n", startIndex);
                        if (endIndex == -1) endIndex = msg.Length;
                        creator = msg.Substring(startIndex, endIndex - startIndex).Trim();
                        // Remove HTML tags if any
                        creator = System.Text.RegularExpressions.Regex.Replace(creator, "<.*?>", string.Empty);
                    }

                    string nextUserMention = "";
                    string nextUserCallback = "";
                    if (StatusID == 92) // Currently Checked, next is Verify
                    {
                        nextUserMention = ftDs.Tables[0].Rows[0]["VerifiedByTelegramUserName"]?.ToString() ?? "";
                    }
                    else if (StatusID == 102) // Currently Verified, next is Approve
                    {
                        nextUserMention = ftDs.Tables[0].Rows[0]["ApprovedByTelegramUserName"]?.ToString() ?? "";
                    }
                    else if (StatusID == 91) // Disapproved, mention creator
                    {
                        // In a real scenario, you might want to fetch the creator's Telegram username
                        // For now, we'll just leave it empty or use the creator's name from the message
                    }

                    if (!string.IsNullOrEmpty(nextUserMention))
                    {
                        if (!nextUserMention.StartsWith("@")) nextUserMention = "@" + nextUserMention;
                        nextUserCallback = nextUserMention;
                    }

                    msg = StatusID == 90 || StatusID == 91
                        ? $@"<b>Dear Team,</b>

Now this FundTransfer {statusText} by {(string.IsNullOrEmpty(UserFullName) ? html.UserFullName : UserFullName)}.

Thank you and best regards,

BILLING SYSTEM AUTO SEND FUND TRANSFER NOTIFICATION. #{refNoFallback}"
                        : $@"<b>Dear {creator},</b>

Now this FundTransfer {statusText} By {(string.IsNullOrEmpty(UserFullName) ? html.UserFullName : UserFullName)}.

{nextInstruction}

Thank you and best regards,

BILLING SYSTEM AUTO SEND FUND TRANSFER NOTIFICATION. #{refNoFallback}";

                    if (!string.IsNullOrEmpty(nextUserMention))
                    {
                        msg = nextUserMention + "\n\n" + msg;
                    }

                    if (!string.IsNullOrEmpty(Comment))
                    {
                        msg += $"\n<b>Comment:</b> {Comment}";
                    }

                    Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup nextButtons = null;
                    if (StatusID == 92) // Checked, next is Verify
                    {
                        nextButtons = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Verify 🔍", $"VerifyFT~{FundTransferID}~{chatId}~{nextUserCallback}"),
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Disapprove ❌", $"DisapproveFT~{FundTransferID}~{chatId}~{nextUserCallback}")
                            }
                        });
                    }
                    else if (StatusID == 102) // Verified, next is Approve
                    {
                        nextButtons = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Approve ✅", $"ApproveFT~{FundTransferID}~{chatId}~{nextUserCallback}"),
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Disapprove ❌", $"DisapproveFT~{FundTransferID}~{chatId}~{nextUserCallback}")
                            }
                        });
                    }

                    try
                    {
                        await botClient.EditMessageText(
                            chatId: chatId,
                            messageId: messageId,
                            text: msg.Replace("\\n", "\n").Replace("<br>", "\n"),
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                            replyMarkup: nextButtons
                        );
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                    {
                        if (ex.Message.Contains("message can't be edited") || ex.Message.Contains("there is no text in the message"))
                        {
                            try
                            {
                                await botClient.EditMessageCaption(
                                    chatId: chatId,
                                    messageId: messageId,
                                    caption: msg.Replace("\\n", "\n").Replace("<br>", "\n"),
                                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                                    replyMarkup: nextButtons
                                );
                            }
                            catch { }
                        }
                        else if (!ex.Message.Contains("message is not modified"))
                        {
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {

                    return Json(new { Result = ResultType.Failed, Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> SendNotificationToTelegram(int FundTransferID, bool isUpdate = false)
        {
            HTML html = new HTML();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            try
            {
                if (FundTransferID > 0)
                {
                    string ftDetailsSql = @"SELECT ft.FundTransferID, ft.RefNo, ft.TransferAmount, ft.TransferDate, ft.Memo, ft.AttachmentFile, ft.ClassName, ft.PaymentMethod,
                                             (coa1.AccountName + ISNULL(' (' + coa1.AccountNo + ')', '')) as FromAccount, 
                                             (coa2.AccountName + ISNULL(' (' + coa2.AccountNo + ')', '')) as ToAccount, ft.SystemID,
                                             ISNULL(u.TelegramUserName, u_fallback.TelegramUserName) as CheckedByTelegramUserName
                                             FROM dbo.TblFundTransfer ft
                                             LEFT JOIN dbo.TblChartOfAccount coa1 ON coa1.ChartOfAccountID = ft.FromAccountID
                                             LEFT JOIN dbo.TblChartOfAccount coa2 ON coa2.ChartOfAccountID = ft.ToAccountID
                                             LEFT JOIN dbo.TblUserLogin u ON u.UserLoginID = ft.CheckedByUserID
                                             LEFT JOIN dbo.TblUserLogin u_fallback ON u_fallback.FullName = ft.CheckedBy
                                             WHERE ft.FundTransferID = @FTID";

                    Dictionary<string, string> detailsParams = new Dictionary<string, string> { { "FTID", FundTransferID.ToString() } };
                    DataSet detailsDs = html.LoadData(ftDetailsSql, false, detailsParams);

                    if (detailsDs != null && detailsDs.Tables.Count > 0 && detailsDs.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = detailsDs.Tables[0].Rows[0];
                        decimal ftAmount = Convert.ToDecimal(dr["TransferAmount"]);
                        string fromAccount = dr["FromAccount"].ToString();
                        string toAccount = dr["ToAccount"].ToString();
                        string ftMemo = dr["Memo"].ToString();
                        string dateStr = Convert.ToDateTime(dr["TransferDate"]).ToString("dd-MMM-yyyy");
                        string sysId = dr["SystemID"]?.ToString() ?? html.SystemID;
                        string checkedByTelegram = dr["CheckedByTelegramUserName"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(checkedByTelegram) && !checkedByTelegram.StartsWith("@")) checkedByTelegram = "@" + checkedByTelegram;

                        var inlineKeyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Check ✍️", $"CheckFT~{FundTransferID}~-5114516641~{checkedByTelegram}"),
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Disapprove ❌", $"DisapproveFT~{FundTransferID}~-5114516641~{checkedByTelegram}")
                            }
                        });

                        string msg = "";
                        if (!string.IsNullOrEmpty(checkedByTelegram))
                        {
                            msg += checkedByTelegram + "\n\n";
                        }

                        string header = isUpdate ? "<b>⚠️ UPDATED FUND TRANSFER REQUEST</b>" : "<b>💸 NEW FUND TRANSFER REQUEST</b>";
                        msg += $@"{header}

<b>No:</b> {dr["RefNo"]}
<b>Source:</b> {fromAccount}
<b>Destination:</b> {toAccount}
<b>Amount:</b> ${ftAmount:N2}
<b>Date:</b> {dateStr}
<b>Class:</b> {dr["ClassName"]}
<b>Method:</b> {dr["PaymentMethod"]}
<b>Memo:</b> {ftMemo}
{(string.IsNullOrEmpty(dr["AttachmentFile"]?.ToString()) ? "" : "<b>Attachment:</b> Attached Above")}

────────────────────────
Created By: {html.UserFullName}
Please review and CHECK or DISAPPROVE.
────────────────────────";

                        string sysSql = "SELECT TOP 1 BotToken FROM dbo.TblTelegramBots WHERE SystemID = @SystemID";
                        Dictionary<string, string> sysParams = new Dictionary<string, string> { { "SystemID", sysId } };
                        DataSet sysDs = html.LoadData(sysSql, false, sysParams);
                        string botToken = "";
                        if (sysDs != null && sysDs.Tables.Count > 0 && sysDs.Tables[0].Rows.Count > 0)
                        {
                            botToken = sysDs.Tables[0].Rows[0]["BotToken"]?.ToString() ?? "";
                        }

                        // Use token from database
                        if (string.IsNullOrEmpty(botToken))
                        {
                            // notificationTelegram.NotificationTelegramBot likely uses a default bot if newBotClient is empty
                        }

                        string attachmentFile = dr["AttachmentFile"]?.ToString() ?? "";
                        bool hasAttachment = !string.IsNullOrEmpty(attachmentFile);

                        SqlResult sqlResult;
                        if (hasAttachment)
                        {
                            // ── Send as photo/document with message as caption ─────────────
                            // MsgText must be non-empty so NotificationTelegramBot enters the
                            // image-extension check branch (SendPhoto for .jpg/.png, SendDocument for others).
                            // Caption="" → smart caption logic uses MsgText directly (no ugly \n\n prefix).
                            sqlResult = await notificationTelegram.NotificationTelegramBot(
                                MsgText: msg,          // non-empty → enters the image-check branch
                                chatId: -5114516641,
                                DestinationFolder: "",
                                PDFFile: attachmentFile,
                                Caption: "",           // empty → caption = MsgText (no double text)
                                IsSendFileWithCaption: false,
                                replyMarkupInlineKeyboardMarkup: true,
                                DBID: FundTransferID,
                                MessageID: 0,
                                TelegramMessage: "",
                                inlineKeyboard: inlineKeyboard,
                                edit: false,
                                messageThreadId: 0,
                                DeleteMessageID: "",
                                newBotClient: botToken
                            );
                        }
                        else
                        {
                            // ── No attachment — send text message only ─────────────────────
                            sqlResult = await notificationTelegram.NotificationTelegramBot(
                                MsgText: msg,
                                chatId: -5114516641,
                                DestinationFolder: "",
                                PDFFile: "",           // no file
                                Caption: "",           // no caption (avoids second message)
                                IsSendFileWithCaption: false,
                                replyMarkupInlineKeyboardMarkup: true,
                                DBID: FundTransferID,
                                MessageID: 0,
                                TelegramMessage: "",
                                inlineKeyboard: inlineKeyboard,
                                edit: false,
                                messageThreadId: 0,
                                DeleteMessageID: "",
                                newBotClient: botToken
                            );
                        }

                        return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { Result = ResultType.Failed, Message = "Invalid Fund Transfer ID" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = ResultType.Failed, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task SendTelegramAuthAlert(string botToken, long chatId, int messageId, string message)
        {
            try
            {
                var botClient = new Telegram.Bot.TelegramBotClient(botToken);

                // Send a new message tagging the user with the reason
                var sent = await botClient.SendMessage(
                    chatId: chatId,
                    text: $"⛔ <b>Access Denied</b>\n\n{message}",
                    parseMode: ParseMode.Html,
                    replyParameters: new ReplyParameters { MessageId = messageId }
                );

                // Auto-delete the alert after 5 seconds so the chat stays clean
                _ = Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    try { await botClient.DeleteMessage(chatId, sent.MessageId); }
                    catch { }
                });
            }
            catch { /* fail silently — don't block the main flow */ }
        }
    }
}