using MVC;
using NSMSOLUTION.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Account
{
    public class AccountController : Controller
    {
        private readonly ReportsRepository _repo = new ReportsRepository();

        // GET: Account
        public ActionResult Index()
        {
            HTML html = new HTML();
            string d = html.GetCurrentDate();
            return View();
        }
        [Authorize(Roles = "Add Account Category, Edit Account Category, Delete Account Category")]
        public string AccountCategory()
        {
            HTML html = new HTML
            {
                DialogBoxHeight = 400
            };
            if (html.HasPermission("Add Account Category"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Account Category"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Account Category"))
                html.AllowDelete = true;

            html.AddButton = "Add Account Type";
            html.AddURL = "Account/AddAccountCategory";
            html.TableName = "TblAccountCategories";
            html.PrimaryField = "AccountCategoryID";
            html.OrderBy = "ct.AccountTypes, c.AccountCategory";

            //List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            //RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            //{
            //    OPTLabel = "Account Types",
            //    SQL = "select AccountCategoryID, AccountCategory from dbo.TblAccountCategories order by AccountCategory",
            //    SelectValue = "AccountCategoryID",
            //    DisplayField = "AccountCategory",
            //    RadioValue = EncryAndDecry.Encryp("AccountCategoryID"),
            //    DefaultCheckBok = true
            //};
            //htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            string sql = @"select ROW_NUMBER() over (order by ct.AccountTypes, c.AccountCategory) as [No], c.AccountCategoryID, ct.AccountTypes, c.AccountCategory, c.UsedFor
from dbo.TblAccountCategories c
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = c.AccountTypesID where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'";
            html.GH.SQL = sql;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Main Account Type", FieldName = "AccountTypes", IsGroupByField = true },
                new GridField() { Header = "Account Type", FieldName = "AccountCategory" },
                new GridField() { Header = "Used For", FieldName = "UsedFor" },
                //new GridField() { Header = "Debit", FieldName = "Debit", TextAlign="center" },
                //new GridField() { Header = "Credit", FieldName = "Credit", TextAlign="center" },
                //new GridField() { Header = "Statement", FieldName = "Statement", TextAlign="center" },
                //new GridField() { Header = "Common Number Sequences", FieldName = "CommonNumberSequences", TextAlign="center" },
                //new GridField() { Header = "Create Date", FieldName = "CreateDate", FormatDateTime="dd-MM-yyyy", TextAlign="center" },
                //new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Action", FieldName = "AccountCategoryID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowSearchButton = false;
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                KendoGridAutoFitColumn = false
            };
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
        }
        [Authorize(Roles = "Add Account Category, Edit Account Category")]
        public string AddAccountCategory(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "AccountCategory",
                FieldIDFocus = "TxtAccountCategory",
                TableName = "TblAccountCategories",
                PrimaryField = "AccountCategoryID",
                PrimaryFieldValue = PrimaryFieldValue
            };

            string sqlactype = @"select AccountTypesID, AccountTypes from dbo.TblAccountTypes where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Main Account Type:",
                ID = "TxtAccountTypes",
                DBField = "AccountTypesID",
                Required = true,
                SQL_CBO = sqlactype,
                Value = "AccountTypesID",
                DisplayText = "AccountTypes"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Account Type:",
                ID = "TxtAccountCategory",
                DBField = "AccountCategory",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "SystemID, WarehouseID, DomainID",
                CheckExistingWithDefaultValue = $"{html.SystemID}, {html.WarehouseID}, {html.DomainID}"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Used For:",
                ID = "TxtUsedFor",
                DBField = "UsedFor",
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Debit:",
            //    ID = "TxtDebit",
            //    DBField = "Debit",
            //    Required = true,
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Credit:",
            //    ID = "TxtCredit",
            //    DBField = "Credit",
            //    Required = true,
            //});
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Statement:",
            //    ID = "TxtStatement",
            //    DBField = "Statement",
            //    Required = true,
            //    SQL_CBO = "select 'Balance Sheet' [Statement]\r\nunion \r\nselect 'Income Statement' [Statement]",
            //    Value = "Statement",
            //    DisplayText = "Statement"
            //}); html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Common Number Sequences:",
            //    ID = "TxtCommonNumberSequences",
            //    DBField = "CommonNumberSequences",
            //});
            return html.DrawHTMLForm();
        }

        [Authorize(Roles = "Add Account Types, Edit Account Types, Delete Account Types")]
        public ActionResult AccountTypes()
        {
            HTML html = new HTML
            {
                DialogBoxHeight = 400
            };
            if (html.HasPermission("Add Account Types"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Account Types"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Account Types"))
                html.AllowDelete = true;

            html.AddButton = "Add Account Types";
            html.AddURL = "Account/AddAccountTypes";
            html.TableName = "TblAccountTypes";
            html.PrimaryField = "AccountTypesID";
            html.OrderBy = "AccountTypes asc";

            //List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            //RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            //{
            //    OPTLabel = "Account Types",
            //    SQL = "select AccountTypesID, AccountTypes from dbo.TblAccountTypes order by AccountTypes",
            //    SelectValue = "AccountTypesID",
            //    DisplayField = "AccountTypes",
            //    RadioValue = EncryAndDecry.Encryp("AccountTypesID"),
            //    DefaultCheckBok = true
            //};
            //htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            //            html.GH.SQL = @"select ROW_NUMBER() over(order by ac.AccountCategory asc) as [No], act.AccountCategoryID,
            //act.AccountTypesID, ac.AccountCategory, act.AccountTypes, ac.CreateBy, ac.CreateDate
            //from dbo.TblAccountTypes act
            //inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID
            //where act.SystemID = " + html.SystemID + " and act.WarehouseID = " + html.WarehouseID + " and act.DomainID like '%" + html.DomainID + "%'";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                //new GridField() { Header = "Account Category", FieldName = "AccountCategory", IsGroupByField= true },
                new GridField() { Header = "Account Types", FieldName = "AccountTypes", },
                new GridField() { Header = "Normal Side", FieldName = "NormalSide", },
                new GridField() { Header = "Used For", FieldName = "UsedFor", },                
                //new GridField() { Header = "Create Date", FieldName = "CreateDate", FormatDateTime="dd-MM-yyyy", TextAlign="center" },
                //new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Action", FieldName = "AccountTypesID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowSearchButton = false;
            html.ShowPaging = false;
            KendoGridSetting setting = new KendoGridSetting
            {
                KendoGridAutoFitColumn = false
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), setting.ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Account Types, Edit Account Types")]
        public string AddAccountTypes(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "AddAccountTypes",
                FieldIDFocus = "TxtAccountTypes",
                TableName = "TblAccountTypes",
                PrimaryField = "AccountTypesID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Account Category:",
            //    ID = "TxtAccountCategory",
            //    DBField = "AccountCategoryID",
            //    Required = true,
            //    SQL_CBO = "select AccountCategoryID, AccountCategory from dbo.TblAccountCategories where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountCategory",
            //    Value = "AccountCategoryID",
            //    DisplayText = "AccountCategory"
            //});

            string sqlcboNormalSide = @"select 'CR' NormalSide
union all 
select 'DR' NormalSide
";
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Account Types:",
                ID = "TxtAccountTypes",
                DBField = "AccountTypes",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Normal Side:",
                ID = "TxtNormalSide",
                DBField = "NormalSide",
                Required = true,
                SQL_CBO = sqlcboNormalSide,
                DisplayText = "NormalSide",
                Value = "NormalSide"
            });

            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Used For:",
                ID = "TxtUsedFor",
                DBField = "UsedFor",
            });

            //html.RestFields = "TxtAccountTypes";
            return html.DrawHTMLForm();
        }

        [Authorize(Roles = "Add Opening Balance, Edit Opening Balance, Delete Opening Balance")]
        public string OpeningBalance()
        {
            HTML html = new HTML
            {
                DialogBoxHeight = 500,
                DialogBoxWidth = "1000"
            };
            if (html.HasPermission("Add Opening Balance"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Opening Balance"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Opening Balance"))
                html.AllowDelete = true;

            html.AddButton = "Add Opening Balance";
            html.AddURL = "Account/AddOpeningBalance";
            html.TableName = "TblAccount_OpeningBalance";
            html.PrimaryField = "OpeningBalanceID";
            html.OrderBy = "ct.AccountTypes, cat.AccountCategory, c.AccountNo, c.AccountName";

            string sql = @"SELECT ROW_NUMBER() over (order by ct.AccountTypes, cat.AccountCategory, c.AccountNo, c.AccountName) as [No],
        o.OpeningBalanceID,
        c.AccountNo,
        c.AccountName,
        cl.ClassName,
        ct.AccountTypes,
        cat.AccountCategory,
        ct.NormalSide,
        o.OpeningBalanceDate,
        o.Memo,
        CASE 
            WHEN ct.NormalSide = 'DR' THEN o.OpeningBalance 
            ELSE 0 
        END AS DebitAmount,
        CASE 
            WHEN ct.NormalSide = 'CR' THEN o.OpeningBalance 
            ELSE 0 
        END AS CreditAmount
    FROM TblAccount_OpeningBalance o
    INNER JOIN TblChartOfAccount c ON o.ChartOfAccountID = c.ChartOfAccountID
    LEFT JOIN dbo.TblClass cl ON cl.ClassID = ISNULL(o.ClassID, c.ClassID)
	left join dbo.TblAccountCategories cat on cat.AccountCategoryID = c.AccountCategoryID
	inner join dbo.TblAccountTypes ct on ct.AccountTypesID = c.AccountTypesID
where o.SystemID = " + html.SystemID + " and o.WarehouseID = " + html.WarehouseID + " and o.DomainID like '%" + html.DomainID + "%'";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Account#", FieldName = "AccountNo" },
                new GridField() { Header = "Name", FieldName = "AccountName" },
                new GridField() { Header = "Class", FieldName = "ClassName" },
                new GridField() { Header = "Type", FieldName = "AccountTypes", IsGroupByField = true },
                new GridField() { Header = "Category", FieldName = "AccountCategory" },
                new GridField() { Header = "Date", FieldName = "OpeningBalanceDate", FormatDateTime="dd-MM-yyyy", TextAlign="center" },
                new GridField() { Header = "Debit", FieldName = "DebitAmount", TextAlign="right" },
                new GridField() { Header = "Credit", FieldName = "CreditAmount", TextAlign="right" },
                new GridField() { Header = "Normal Side", FieldName = "NormalSide", TextAlign="center" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                new GridField() { Header = "Action", FieldName = "OpeningBalanceID", TextAlign = "Center",  IsModifyField = true, DeleteWithTableName="TblAccount_JournalEntries, TblAccount_Transaction" }
            };
            html.ShowSearchButton = false;
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                KendoGridAutoFitColumn = true
            };
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
        }

        [Authorize(Roles = "Add Opening Balance, Opening Balance")]
        public ActionResult AddOpeningBalance(string PrimaryFieldValue)
        {
            HTML html = new HTML();
            DateTime myDate = DateTime.Parse(html.GetCurrentDate());
            DateTime firstDay = new DateTime(myDate.Year, myDate.Month, 1);
            html.FormID = "AddOpeningBalance";
            html.FieldIDFocus = "TxtClassName";
            html.TableName = "TblAccount_OpeningBalance";
            html.PrimaryField = "OpeningBalanceID";
            html.PrimaryFieldValue = PrimaryFieldValue;

            if (!string.IsNullOrEmpty(PrimaryFieldValue) && PrimaryFieldValue != "0")
            {
                html.SQLForEdit = @"SELECT o.*, ISNULL(o.ClassID, c.ClassID) AS ClassID 
                                    FROM dbo.TblAccount_OpeningBalance o
                                    LEFT JOIN dbo.TblChartOfAccount c ON o.ChartOfAccountID = c.ChartOfAccountID
                                    WHERE o.OpeningBalanceID = " + PrimaryFieldValue;
            }

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Class Name:",
                ID = "TxtClassName",
                DBField = "ClassID",
                Required = true,
                SQL_CBO = "select ClassID, ClassName from TblClass where SystemID = " + html.SystemID+" and WarehouseID = "+html.WarehouseID+" and DomainID like '%"+html.DomainID+ "%' order by ClassName",
                DisplayText = "ClassName",
                Value = "ClassID",
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Chart Of Account:",
                ID = "TxtChartOfAccount",
                DBField = "ChartOfAccountID",
                Required = true,
                SQL_CBO = @"SELECT DISTINCT
    a.ChartOfAccountID,
    a.AccountNo,
    a.AccountNo + ' - ' + a.AccountName AccountName,
    t.AccountTypes,
    CASE
        WHEN a.AccountName LIKE 'Accu.Depre%' THEN 'Credit'
        ELSE a.NormalSide
    END AS NormalSide,
    CASE t.AccountTypes
        WHEN 'Asset' THEN 1
        WHEN 'Liability' THEN 2
        WHEN 'Equity' THEN 3
    END AS AccountTypeOrder
FROM dbo.TblChartOfAccount a
JOIN dbo.TblAccountTypes t
    ON a.AccountTypesID = t.AccountTypesID
WHERE
    a.StatusID = 40
    AND t.AccountTypes IN ('Asset', 'Liability', 'Equity')
    AND a.AccountName NOT LIKE 'Owner%Drawing%'
    AND a.AccountName <> 'Retained Earnings'
     AND a.SystemID = " + html.SystemID + " AND a.WarehouseID = " + html.WarehouseID + " AND a.DomainID = " + html.DomainID + @"
ORDER BY
    AccountTypeOrder,
    a.AccountNo;
",
                DisplayText = "AccountName",
                Value = "ChartOfAccountID",
                GroupByField = "AccountTypes",
                //EventChange = "OnAccountTypeChange(this)"
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Account Name:",
            //    ID = "TxtAccountName",
            //    DBField = "AccountName",
            //    Required = true
            //});


            //string sqlactype = @"select AccountTypesID, AccountTypes from dbo.TblAccountTypes where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountTypes";
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Account Type:",
            //    ID = "TxtAccountTypes",
            //    DBField = "AccountTypesID",
            //    Required = true,
            //    SQL_CBO = sqlactype,
            //    Value = "AccountTypesID",
            //    DisplayText = "AccountTypes",
            //    ISCallBackToCbo = true,
            //    CallBackToControlID = "TxtAccountCategory",
            //    CallBackDisplayText = "AccountCategory",
            //    CallBackDisplayValue = "AccountCategoryID",
            //    SQLCallBack = "select AccountCategoryID, AccountCategory from dbo.TblAccountCategories where AccountTypesID = @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountCategory",
            //    placeholder = "Account Type"
            //});
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Account Category:",
            //    ID = "TxtAccountCategory",
            //    DBField = "AccountCategoryID",
            //    //Required = true,
            //    SQL_CBO = "select AccountCategoryID, AccountCategory from dbo.TblAccountCategories where 1=2",
            //    Value = "AccountCategoryID",
            //    DisplayText = "AccountCategory",
            //    placeholder = "Account Category"
            //});
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Account Type:",
            //    ID = "TxtAccountType",
            //    DBField = "AccountCategoryID",
            //    Required = true,
            //    SQL_CBO = "select act.AccountTypes, ac.AccountCategory, act.AccountTypesID, ac.AccountCategoryID from dbo.TblAccountTypes act\r\ninner join dbo.TblAccountCategories ac on ac.AccountTypesID = act.AccountTypesID where act.SystemID = " + html.SystemID + " and act.WarehouseID = " + html.WarehouseID + " and act.DomainID like '%" + html.DomainID + "%' order by act.AccountTypes",
            //    DisplayText = "AccountCategory",
            //    Value = "AccountTypesID, AccountCategoryID",
            //    GroupByField = "AccountTypes",
            //    //EventChange = "OnAccountTypeChange(this, e)",
            //    placeholder = "Parent Account"
            //});
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Sub Account:",
            //    ID = "TxtSubOfAccountType",
            //    DBField = "ParentAccountID",
            //    SQL_CBO = "select ChartOfAccountID ParentAccountID, AccountName SubOfAccountType from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by AccountName",
            //    //HiddenFeild= "ParentAccountID",
            //    DisplayText = "SubOfAccountType",
            //    Value = "ParentAccountID",
            //    placeholder = "Parent Account"
            //    //SQL = "select AccountName SubOfAccountType from dbo.TblChartOfAccount where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' union select SubOfAccountType from dbo.TblChartOfAccount where SubOfAccountType <> '' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' group by SubOfAccountType order by SubOfAccountType",
            //    //isTypeahead = true,
            //    //placeholder="None",
            //    //Infinity = false
            //});
            //            string sqlcboNormalSide = @"select 'CR' NormalSide
            //union all 
            //select 'DR' NormalSide
            //";
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Normal Side:",
            //    ID = "TxtNormalSide",
            //    DBField = "NormalSide",
            //    Required = true,
            //    SQL_CBO = sqlcboNormalSide,
            //    DisplayText = "NormalSide",
            //    Value = "NormalSide"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //	Label = "OrderBy:",
            //	ID = "TxtOrderBy",
            //	DBField = "OrderBy",
            //	ISNumeric = true,
            //	ISInteger = true,
            //	Value = "1"
            //});

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Opening Balance:",
                ID = "TxtOpeningBalance",
                DBField = "OpeningBalance",
                Value = "0",
                ISNumeric = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Opening Balance Date:",
                ID = "TxtOpeningBalanceDate",
                DBField = "OpeningBalanceDate",
                SingleDatePicker = true,
                DefaultShowDateValue = true,
                Value = firstDay.ToString("yyyy-MM-dd"),
            });

            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Is Default:",
            //    ID = "IsDefault",
            //    DBField = "IsDefault",
            //    Type = "checkbox",
            //    Class = "form-control form-control-sm col-1",
            //    Hide = true
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Default For Invoice:",
            //    ID = "DefaultForInvoice",
            //    DBField = "DefaultForInvoice",
            //    Type = "checkbox",
            //    Class = "form-control form-control-sm col-1"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Default For Bill:",
            //    ID = "DefaultForBill",
            //    DBField = "DefaultForBill",
            //    Type = "checkbox",
            //    Class = "form-control form-control-sm col-1"
            //});
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Status:",
            //    ID = "TxtStatus",
            //    DBField = "StatusID",
            //    Required = true,
            //    SQL_CBO = "select StatusID, [Status] from dbo.TblStatus where StatusID in(40, 43) order by StatusID",
            //    DisplayText = "Status",
            //    Value = "StatusID",
            //});
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "FiscalYear:",
                ID = "TxtFiscalYear",
                DBField = "FiscalYear",
                Hide = true,
                Value = html.DateTimeNow().ToString("yyyy"),
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "ActivateAccountAsOf:",
            //    ID = "TxtActivateAccountAsOf",
            //    DBField = "ActivateAccountAsOf",
            //    Hide = true,
            //    Value = html.GetCurrentDate(),
            //});

            html.SaveChangeCallbackOnCloseDialog = true;
            html.OnDialogCloseCallbackFunction = "savechangecallback();";
            //html.SaveChangeCallbackFunction = "Reload_TxtSubOfAccountType()";
            //html.RestFields = "TxtAccountNo,TxtAccountName,TxtOpeningBalance,TxtOpeningBalanceDate,TxtMemo";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public ActionResult HierarchyTrialBalance()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetHierarchyTrialBalance()
        {
            var data = new List<object>();
            decimal sumOpeningDebit = 0, sumOpeningCredit = 0;
            HTML html = new HTML();

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["AuthContextNSM"].ConnectionString;
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("sp_HierarchyTrialBalance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SystemID", html.SystemID);
                    cmd.Parameters.AddWithValue("@WarehouseID", html.WarehouseID);
                    cmd.Parameters.AddWithValue("@DomainID", html.DomainID);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new
                            {
                                AccountNameIndented = reader["AccountNameIndented"].ToString(),
                                AccountNo = reader["AccountNo"].ToString(),
                                AccountCategory = reader["AccountCategory"].ToString(),
                                DebitBalance = Convert.ToDecimal(reader["DebitBalance"]),
                                CreditBalance = Convert.ToDecimal(reader["CreditBalance"]),
                                NormalSide = reader["NormalSide"].ToString(),
                                AncestorLevel = Convert.ToInt32(reader["AncestorLevel"]),
                                ParentAccountNo = reader["ParentAccountNo"] == DBNull.Value ? null : reader["ParentAccountNo"].ToString()
                            });
                        }

                        if (reader.NextResult() && reader.Read())
                        {
                            sumOpeningDebit = Convert.ToDecimal(reader["SumOpeningDebit"]);
                            sumOpeningCredit = Convert.ToDecimal(reader["SumOpeningCredit"]);
                        }
                    }
                }

                return Json(new { data, sumOpeningDebit, sumOpeningCredit }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetAccountClassMapping(int? accountID = null)
        {
            HTML html = new HTML();
            string sql;
            if (accountID.HasValue && accountID.Value > 0)
            {
                sql = @"SELECT TOP 1 ChartOfAccountID, ClassID 
                        FROM dbo.TblChartOfAccount 
                        WHERE ChartOfAccountID = " + accountID.Value + " AND SystemID = " + html.SystemID + " AND ClassID IS NOT NULL";
                DataSet ds = html.LoadData(sql);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    int classId = Convert.ToInt32(ds.Tables[0].Rows[0]["ClassID"]);
                    return Json(new { ChartOfAccountID = accountID.Value, ClassID = classId }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { ChartOfAccountID = accountID.Value, ClassID = 0 }, JsonRequestBehavior.AllowGet);
            }

            sql = @"SELECT ChartOfAccountID, ClassID 
                    FROM dbo.TblChartOfAccount 
                    WHERE SystemID = " + html.SystemID + " AND ClassID IS NOT NULL";
            DataSet dataSet = html.LoadData(sql);
            var map = new Dictionary<string, int>();
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    if (row["ClassID"] != DBNull.Value)
                    {
                        map[row["ChartOfAccountID"].ToString()] = Convert.ToInt32(row["ClassID"]);
                    }
                }
            }
            return Json(map, JsonRequestBehavior.AllowGet);
        }

    }
}